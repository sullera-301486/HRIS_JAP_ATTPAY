using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveRequest : Form
    {
        // 🔹 Firebase client (use your real database URL)
        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public LeaveRequest()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
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
                textBoxNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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
            string employeeInput = textBoxNameInput.Text?.Trim();
            string leaveType = comboBoxLeaveTypeInput.Text?.Trim();
            string start = textBoxStartPeriod.Text?.Trim();
            string end = textBoxEndPeriod.Text?.Trim();
            string notes = textBoxReasonInput.Text?.Trim();

            // 🔹 Ensure all fields are filled
            if (string.IsNullOrWhiteSpace(employeeInput) ||
                string.IsNullOrWhiteSpace(leaveType) ||
                string.IsNullOrWhiteSpace(start) ||
                string.IsNullOrWhiteSpace(end))
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate if the employee exists in the database
            bool isValidEmployee = await IsEmployeeValidAsync(employeeInput);
            if (!isValidEmployee)
            {
                MessageBox.Show(
                    $"Employee '{employeeInput}' does not exist in the database.",
                    "Invalid Employee",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // 🔹 SubmittedBy = currently logged-in user
            string submittedBy = !string.IsNullOrEmpty(SessionClass.CurrentEmployeeName)
                ? SessionClass.CurrentEmployeeName
                : "Unknown User";

            // 🔹 Build the request object
            var request = new LeaveNotificationItems.LeaveNotificationModel
            {
                Title = $"Leave Request - {leaveType}",
                SubmittedBy = submittedBy,    // Logged-in user
                Employee = employeeInput,     // Employee typed in textbox
                LeaveType = leaveType,
                Period = $"{start} - {end}",
                Notes = notes,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 🔹 Show confirm preview window
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

            using (var confirm = new LeaveRequestConfirm(preview))
            {
                var result = confirm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // 🔹 Save to Firebase
                    await FirebaseSave(request);

                    MessageBox.Show(
                        "Leave request submitted successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    this.Close();
                }
            }
        }

        // 🔹 Save to Firebase
        private async Task FirebaseSave(LeaveNotificationItems.LeaveNotificationModel notif)
        {
            await firebase.Child("LeaveNotifications").PostAsync(notif);
        }

        // 🔹 Validate if employee name exists in EmployeeDetails node
        private static async Task<bool> IsEmployeeValidAsync(string employeeName)
        {
            if (string.IsNullOrWhiteSpace(employeeName))
                return false;

            var employees = await firebase
                .Child("EmployeeDetails")
                .OnceAsync<dynamic>();

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
                catch
                {
                    continue;
                }
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
