using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveRequest : Form
    {
        // 🔹 Firebase client
        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public LeaveRequest()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();

            // 🟢 Load employee names from Firebase
            _ = LoadEmployeeNamesAsync();
        }

        // 🟢 Load all names from "Leave Credits"
        private async Task LoadEmployeeNamesAsync()
        {
            try
            {
                comboBoxName.Items.Clear();

                var employees = await firebase.Child("Leave Credits").OnceAsync<dynamic>();
                List<string> names = new List<string>();

                foreach (var emp in employees)
                {
                    string fullName = emp.Object.full_name?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(fullName) && !names.Contains(fullName))
                        names.Add(fullName);
                }

                names.Sort();
                comboBoxName.Items.AddRange(names.ToArray());

                if (comboBoxName.Items.Count > 0)
                    comboBoxName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee names: " + ex.Message,
                    "Firebase Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void setFont()
        {
            try
            {
                labelDash.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelRequestLeaveEntry.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelReason.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                comboBoxName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxReasonInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void buttonSendRequest_Click(object sender, EventArgs e)
        {
            string employeeInput = comboBoxName.Text?.Trim();
            string leaveType = comboBoxLeaveTypeInput.Text?.Trim();
            string start = textBoxStartPeriod.Text?.Trim();
            string end = textBoxEndPeriod.Text?.Trim();
            string notes = textBoxReasonInput.Text?.Trim();

            // 🔹 Validate required fields
            if (string.IsNullOrWhiteSpace(employeeInput) ||
                string.IsNullOrWhiteSpace(leaveType) ||
                string.IsNullOrWhiteSpace(start) ||
                string.IsNullOrWhiteSpace(end))
            {
                MessageBox.Show("Please fill in all required fields.",
                    "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate date formats and restrictions (STRICT mm/dd/yyyy)
            if (!DateTime.TryParseExact(start, "MM/dd/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact(end, "MM/dd/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime endDate))
            {
                MessageBox.Show("Please enter dates in the correct format: mm/dd/yyyy.",
                    "Invalid Date Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (startDate.Date < DateTime.Now.Date)
            {
                MessageBox.Show("The start date cannot be earlier than today's date.",
                    "Invalid Start Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (endDate.Date < startDate.Date)
            {
                MessageBox.Show("The end date cannot be earlier than the start date.",
                    "Invalid End Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Compute total days excluding Sundays
            int totalDaysRequested = CountDaysExcludingSundays(startDate, endDate);

            if (totalDaysRequested <= 0)
            {
                MessageBox.Show("The selected period includes only Sundays. Please select valid leave days.",
                    "Invalid Leave Period", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Check if requested days exceed available leave credits
            int availableCredits = await GetAvailableLeaveCreditsAsync(employeeInput, leaveType);

            if (availableCredits == -1)
            {
                MessageBox.Show($"Could not retrieve leave credits for {employeeInput}.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (totalDaysRequested > availableCredits)
            {
                MessageBox.Show(
                    $"The requested leave period is {totalDaysRequested} day(s) (excluding Sundays), " +
                    $"but {employeeInput} only has {availableCredits} available {leaveType} credits.",
                    "Exceeded Leave Credits",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // 🔹 Validate if the employee exists
            bool isValidEmployee = await IsEmployeeValidAsync(employeeInput);
            if (!isValidEmployee)
            {
                MessageBox.Show($"Employee '{employeeInput}' does not exist in the database.",
                    "Invalid Employee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Check leave credits before submitting
            bool hasCredits = await HasAvailableLeaveCreditsAsync(employeeInput, leaveType);
            if (!hasCredits)
            {
                MessageBox.Show(
                    $"Employee '{employeeInput}' has no remaining {leaveType} credits.\n" +
                    "Leave request cannot be submitted.",
                    "Insufficient Leave Credits",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // 🔹 Get logged-in user name
            string submittedBy = !string.IsNullOrEmpty(SessionClass.CurrentEmployeeName)
                ? SessionClass.CurrentEmployeeName
                : "Unknown User";

            // 🔹 Build leave request notification
            var request = new LeaveNotificationItems.LeaveNotificationModel
            {
                Title = $"Leave Request - {leaveType}",
                SubmittedBy = submittedBy,
                Employee = employeeInput,
                LeaveType = leaveType,
                Period = $"{start} - {end}",
                Notes = notes,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 🔹 Preview confirmation window
            var preview = new LeaveRequestData
            {
                Title = request.Title,
                SubmittedBy = request.SubmittedBy,
                EmployeeName = request.Employee,
                LeaveType = request.LeaveType,
                Start = start,
                End = end,
                Notes = request.Notes,
                Photo = null,
                CreatedAt = DateTime.Now
            };

            Form parentForm = this.FindForm();
            LeaveRequestConfirm confirmForm = new LeaveRequestConfirm(preview);

            confirmForm.FormClosed += async (s, args) =>
            {
                if (confirmForm.DialogResult == DialogResult.OK)
                {
                    await FirebaseSave(request);
                    MessageBox.Show($"Leave request submitted successfully!\n" +
                        $"Total days (excluding Sundays): {totalDaysRequested}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            };

            // 🟢 Show confirmation overlay
            AttributesClass.ShowWithOverlay(parentForm, confirmForm);
        }

        // ✅ Function to count total days excluding Sundays
        private int CountDaysExcludingSundays(DateTime start, DateTime end)
        {
            int count = 0;
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Sunday)
                    count++;
            }
            return count;
        }

        // 🔹 Check if employee has available leave credits
        private static async Task<bool> HasAvailableLeaveCreditsAsync(string employeeName, string leaveType)
        {
            try
            {
                var leaves = await firebase.Child("Leave Credits").OnceAsync<dynamic>();
                foreach (var emp in leaves)
                {
                    string fullName = emp.Object.full_name?.ToString()?.Trim();
                    if (string.Equals(fullName, employeeName, StringComparison.OrdinalIgnoreCase))
                    {
                        int sickLeave = emp.Object.sick_leave != null ? (int)emp.Object.sick_leave : 0;
                        int vacationLeave = emp.Object.vacation_leave != null ? (int)emp.Object.vacation_leave : 0;

                        if (leaveType.ToLower().Contains("sick") && sickLeave > 0)
                            return true;
                        if (leaveType.ToLower().Contains("vacation") && vacationLeave > 0)
                            return true;

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking leave credits: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        // 🔹 Get available leave credits count
        private static async Task<int> GetAvailableLeaveCreditsAsync(string employeeName, string leaveType)
        {
            try
            {
                var leaves = await firebase.Child("Leave Credits").OnceAsync<dynamic>();
                foreach (var emp in leaves)
                {
                    string fullName = emp.Object.full_name?.ToString()?.Trim();
                    if (string.Equals(fullName, employeeName, StringComparison.OrdinalIgnoreCase))
                    {
                        int sickLeave = emp.Object.sick_leave != null ? (int)emp.Object.sick_leave : 0;
                        int vacationLeave = emp.Object.vacation_leave != null ? (int)emp.Object.vacation_leave : 0;

                        if (leaveType.ToLower().Contains("sick"))
                            return sickLeave;
                        if (leaveType.ToLower().Contains("vacation"))
                            return vacationLeave;

                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving leave credits: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return -1;
        }

        // 🔹 Save to Firebase
        private async Task FirebaseSave(LeaveNotificationItems.LeaveNotificationModel notif)
        {
            await firebase.Child("LeaveNotifications").PostAsync(notif);
        }

        // 🔹 Validate employee name
        private static async Task<bool> IsEmployeeValidAsync(string employeeName)
        {
            if (string.IsNullOrWhiteSpace(employeeName))
                return false;

            var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
            foreach (var emp in employees)
            {
                try
                {
                    string first = emp.Object.first_name ?? "";
                    string middle = emp.Object.middle_name ?? "";
                    string last = emp.Object.last_name ?? "";

                    string fullNameWithMiddle = $"{first} {middle} {last}".Replace("  ", " ").Trim();
                    string fullNameNoMiddle = $"{first} {last}".Trim();

                    if (string.Equals(employeeName, fullNameWithMiddle, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(employeeName, fullNameNoMiddle, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch { continue; }
            }
            return false;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxStartPeriod, "mm/dd/yyyy");
            AttributesClass.TextboxPlaceholder(textBoxEndPeriod, "mm/dd/yyyy");
        }
    }
}
