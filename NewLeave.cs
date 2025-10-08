using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        // 🟢 Send Request → show confirmation form first
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

            // 🔹 Show confirm dialog first
            ConfirmLeaveEntry confirmForm = new ConfirmLeaveEntry();
            confirmForm.FormClosed += async (s, ev) =>
            {
                // ✅ If user confirmed
                if (confirmForm.DialogResult == DialogResult.OK)
                {
                    await ProcessLeaveRequest(employee, leaveType, start, end);
                }
            };

            Form parentForm = this.FindForm();
            AttributesClass.ShowWithOverlay(parentForm, confirmForm);
        }

        // 🟢 Actual process to save and deduct leave
        private async Task ProcessLeaveRequest(string employee, string leaveType, string start, string end)
        {
            try
            {
                // 🟢 Get current logged-in user
                string submittedBy = !string.IsNullOrEmpty(SessionClass.CurrentEmployeeName)
                    ? SessionClass.CurrentEmployeeName
                    : "Unknown User";

                // 🟢 Deduct leave first
                bool deducted = await DeductLeaveBalanceAsync(employee, leaveType);
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
                    period = $"{start} - {end}"
                };

                await firebase.Child("ManageLeave").PostAsync(manageLeaveData);

                MessageBox.Show("Leave request successfully sent and leave deducted.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing leave request: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🟢 Deduct leave credits from "Leave Credits" table
        private async Task<bool> DeductLeaveBalanceAsync(string employeeName, string leaveType)
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

                int sickLeave = empData.sick_leave != null ? (int)empData.sick_leave : 6;
                int vacationLeave = empData.vacation_leave != null ? (int)empData.vacation_leave : 6;

                // 🧮 Check and deduct based on leave type
                if (leaveType.ToLower().Contains("sick"))
                {
                    if (sickLeave <= 0)
                    {
                        MessageBox.Show($"{employeeName} has no remaining Sick Leave.",
                            "No Leave Left", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    sickLeave = Math.Max(0, sickLeave - 1);
                }
                else if (leaveType.ToLower().Contains("vacation"))
                {
                    if (vacationLeave <= 0)
                    {
                        MessageBox.Show($"{employeeName} has no remaining Vacation Leave.",
                            "No Leave Left", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    vacationLeave = Math.Max(0, vacationLeave - 1);
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

                // Update displayed values
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
