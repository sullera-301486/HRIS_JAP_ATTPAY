using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class NewLeave : Form
    {
        private readonly FirebaseClient firebase;
        private Dictionary<string, JObject> employeeData = new Dictionary<string, JObject>();

        public NewLeave()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();

            // 🔹 Initialize Firebase connection
            firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

            // 🔹 Load employee list on form load
            this.Load += async (s, e) => await LoadEmployeeNames();
            comboBoxInputName.SelectedIndexChanged += ComboBoxInputName_SelectedIndexChanged;
        }

        // 🔹 Load all employees from Leave Credits table
        private async Task LoadEmployeeNames()
        {
            try
            {
                var leaveCredits = await firebase
                    .Child("Leave Credits")
                    .OnceAsync<object>();

                comboBoxInputName.Items.Clear();
                employeeData.Clear();

                foreach (var item in leaveCredits)
                {
                    if (item.Object == null) continue;

                    var emp = JObject.FromObject(item.Object);

                    string fullName = emp["full_name"]?.ToString();
                    if (string.IsNullOrEmpty(fullName)) continue;

                    comboBoxInputName.Items.Add(fullName);
                    employeeData[fullName] = emp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee names: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🔹 When an employee is selected, show their info
        private void ComboBoxInputName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedName = comboBoxInputName.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedName) || !employeeData.ContainsKey(selectedName))
                    return;

                JObject emp = employeeData[selectedName];

                labelDepartmentInput.Text = emp["department"]?.ToString() ?? "N/A";
                labelPositionInput.Text = emp["position"]?.ToString() ?? "N/A";
                labelSickLeaveInput.Text = emp["sick_leave"]?.ToString() ?? "0";
                labelVacationLeaveInput.Text = emp["vacation_leave"]?.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee details: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🟢 Send Request → validate dates and show confirmation
        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            string employee = comboBoxInputName.SelectedItem?.ToString();
            string leaveType = comboBoxLeaveType.SelectedItem?.ToString();
            string start = textBoxStartPeriod.Text.Trim();
            string end = textBoxEndPeriod.Text.Trim();

            if (string.IsNullOrEmpty(employee) ||
                string.IsNullOrEmpty(leaveType) ||
                string.IsNullOrEmpty(start) ||
                string.IsNullOrEmpty(end))
            {
                MessageBox.Show("Please complete all fields before sending the request.",
                    "Incomplete Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Validate date formats
            string[] formats = { "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" };
            if (!DateTime.TryParseExact(start, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact(end, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                MessageBox.Show("Invalid date format. Please use MM/DD/YYYY or YYYY-MM-DD.",
                    "Invalid Dates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Block if start date is before today
            if (startDate.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Start date cannot be earlier than today's date.",
                    "Invalid Start Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (endDate < startDate)
            {
                MessageBox.Show("End date cannot be earlier than start date.",
                    "Invalid Dates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Compute total days excluding Sundays
            int totalDays = CountDaysExcludingSundays(startDate, endDate);

            if (totalDays <= 0)
            {
                MessageBox.Show("Selected period contains no valid leave days (all Sundays).",
                    "Invalid Leave Period", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Confirm summary
            DialogResult confirm = MessageBox.Show(
                $"Confirm leave request?\n\nEmployee: {employee}\nType: {leaveType}\n" +
                $"Period: {startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}\n" +
                $"Total Days (excluding Sundays): {totalDays}",
                "Confirm Leave Request", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (confirm != DialogResult.OK) return;

            // 🟢 If admin (ID = 101), apply directly (no confirmation form)
            if (SessionClass.CurrentEmployeeId == "101")
            {
                _ = ProcessLeaveRequest(employee, leaveType, startDate, endDate, totalDays);
                return;
            }

            // 🔹 Otherwise, show confirmation form for regular users
            Form confirmForm = new ConfirmLeaveEntry();

            confirmForm.FormClosed += async (s, ev) =>
            {
                if (confirmForm.DialogResult == DialogResult.OK)
                {
                    await ProcessLeaveRequest(employee, leaveType, startDate, endDate, totalDays);
                }
            };

            Form parentForm = this.FindForm();
            AttributesClass.ShowWithOverlay(parentForm, confirmForm);
        }

        // ✅ Function: Count total days excluding Sundays
        private int CountDaysExcludingSundays(DateTime start, DateTime end)
        {
            int count = 0;
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Sunday)
                {
                    count++;
                }
            }
            return count;
        }

        // 🟢 Process Leave Request with totalDays deduction
        private async Task ProcessLeaveRequest(string employee, string leaveType, DateTime startDate, DateTime endDate, int totalDays)
        {
            try
            {
                string submittedBy = !string.IsNullOrEmpty(SessionClass.CurrentEmployeeName)
                    ? SessionClass.CurrentEmployeeName
                    : "Unknown User";

                // 🟢 Deduct total days from leave credits
                bool deducted = await DeductLeaveBalanceAsync(employee, leaveType, totalDays);
                if (!deducted)
                {
                    MessageBox.Show("Unable to deduct leave. Please check leave credits.",
                        "Leave Deduction Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🟢 Save leave request to ManageLeave
                var manageLeaveData = new
                {
                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    submitted_by = submittedBy,
                    employee = employee,
                    leave_type = leaveType,
                    period = $"{startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}",
                    total_days = totalDays,
                    status = SessionClass.CurrentEmployeeId == "101" ? "Approved" : "Pending"
                };

                await firebase.Child("ManageLeave").PostAsync(manageLeaveData);

                MessageBox.Show(
                    SessionClass.CurrentEmployeeId == "101"
                    ? $"Leave successfully applied and approved.\n{totalDays} day(s) deducted from {leaveType}."
                    : $"Leave request successfully sent.\n{totalDays} day(s) deducted from {leaveType} (Sundays excluded).",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing leave request: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🟢 Deduct leave credits (multi-day, Sundays excluded)
        private async Task<bool> DeductLeaveBalanceAsync(string employeeName, string leaveType, int totalDays)
        {
            try
            {
                var employees = await firebase.Child("Leave Credits").OnceAsync<dynamic>();
                string employeeId = null;
                dynamic empData = null;

                foreach (var emp in employees)
                {
                    string fullName = emp.Object.full_name?.ToString().Trim();
                    if (string.Equals(fullName, employeeName, StringComparison.OrdinalIgnoreCase))
                    {
                        employeeId = emp.Key;
                        empData = emp.Object;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show($"Employee '{employeeName}' not found in Leave Credits.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                int sickLeave = 6, vacationLeave = 6;
                try { sickLeave = Convert.ToInt32(empData.sick_leave); } catch { }
                try { vacationLeave = Convert.ToInt32(empData.vacation_leave); } catch { }

                if (leaveType.ToLower().Contains("sick"))
                {
                    if (sickLeave < totalDays)
                    {
                        MessageBox.Show($"{employeeName} has only {sickLeave} sick leave(s) left but requested {totalDays}.",
                            "Insufficient Sick Leave", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    sickLeave -= totalDays;
                }
                else if (leaveType.ToLower().Contains("vacation"))
                {
                    if (vacationLeave < totalDays)
                    {
                        MessageBox.Show($"{employeeName} has only {vacationLeave} vacation leave(s) left but requested {totalDays}.",
                            "Insufficient Vacation Leave", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    vacationLeave -= totalDays;
                }

                var updatedCredits = new
                {
                    employee_id = empData.employee_id ?? employeeId,
                    full_name = empData.full_name,
                    department = empData.department ?? "",
                    position = empData.position ?? "",
                    sick_leave = sickLeave,
                    vacation_leave = vacationLeave,
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase.Child("Leave Credits").Child(employeeId).PutAsync(updatedCredits);

                labelSickLeaveInput.Text = sickLeave.ToString();
                labelVacationLeaveInput.Text = vacationLeave.ToString();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deducting leave: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelAddLeaveRecord.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDash.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNewLeave.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelSickLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSickLeaveInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelVacationLeaveInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxInputName.Font = AttributesClass.GetFont("Roboto-Light", 15f);
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveType.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxStartPeriod, "Start of leave");
            AttributesClass.TextboxPlaceholder(textBoxEndPeriod, "End of leave");
        }
    }
}
