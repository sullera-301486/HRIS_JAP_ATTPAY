using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Net.Http;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveList : UserControl
    {
        public event EventHandler RevokeClicked;

        // 🔹 Firebase connection
        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        // 🔹 Tooltip for overflowing labels
        private ToolTip infoToolTip = new ToolTip();

        public LeaveList()
        {
            InitializeComponent();
            SetFont();

            infoToolTip.InitialDelay = 100;
            infoToolTip.AutoPopDelay = 8000;
            infoToolTip.ReshowDelay = 100;
        }

        private void SetFont()
        {
            try
            {
                lblSubmittedBy.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblEmployee.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDate.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblLeaveType.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPeriod.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                label5.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label6.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label1.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                btnRevoke.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        public async void SetData(string submittedBy, string employee, string date, string leaveType, string period, Image photo)
        {
            // Assign all labels first
            lblSubmittedBy.Text = submittedBy;
            lblEmployee.Text = employee;
            lblLeaveType.Text = leaveType;
            lblPeriod.Text = period;
            picEmployee.SizeMode = PictureBoxSizeMode.Zoom;

            // 🟢 1️⃣ Get actual creation date from Firebase ManageLeave table
            string createdDate = await GetManageLeaveDateAsync(submittedBy, employee, leaveType, period);
            lblDate.Text = createdDate ?? "Unknown Date";

            // 🟢 2️⃣ Get image based on SubmittedBy
            Image fetchedImage = await GetImageBySubmittedByAsync(submittedBy);
            picEmployee.Image = fetchedImage ?? Properties.Resources.User1;

            // 🟢 3️⃣ Apply tooltips if text is too long
            ApplyTooltipIfTruncated(lblSubmittedBy);
            ApplyTooltipIfTruncated(lblEmployee);
            ApplyTooltipIfTruncated(lblPeriod);
            ApplyTooltipIfTruncated(lblDate);
        }

        private async Task<string> GetManageLeaveDateAsync(string submittedBy, string employee, string leaveType, string period)
        {
            try
            {
                var leaves = await firebase.Child("ManageLeave").OnceAsync<dynamic>();

                foreach (var leave in leaves)
                {
                    string fbSubmitted = (leave.Object.submitted_by ?? "").ToString().Trim();
                    string fbEmployee = (leave.Object.employee ?? "").ToString().Trim();
                    string fbLeaveType = (leave.Object.leave_type ?? "").ToString().Trim();
                    string fbPeriod = (leave.Object.period ?? "").ToString().Trim();

                    if (string.Equals(fbSubmitted, submittedBy, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(fbEmployee, employee, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(fbLeaveType, leaveType, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(fbPeriod, period, StringComparison.OrdinalIgnoreCase))
                    {
                        string createdAt = leave.Object.created_at?.ToString();
                        if (!string.IsNullOrEmpty(createdAt))
                        {
                            DateTime parsedDate;
                            if (DateTime.TryParse(createdAt, out parsedDate))
                                return parsedDate.ToString("MMMM dd, yyyy");
                            else
                                return createdAt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching ManageLeave date: " + ex.Message);
            }

            return null;
        }

        private async Task<Image> GetImageBySubmittedByAsync(string submittedBy)
        {
            try
            {
                var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                foreach (var emp in employees)
                {
                    string firstName = (emp.Object.first_name ?? "").ToString().Trim();
                    string middleName = (emp.Object.middle_name ?? "").ToString().Trim();
                    string lastName = (emp.Object.last_name ?? "").ToString().Trim();

                    string fullName = $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();
                    string fullNameNoMiddle = $"{firstName} {lastName}".Trim();

                    if (string.Equals(submittedBy, fullName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(submittedBy, fullNameNoMiddle, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(submittedBy, firstName, StringComparison.OrdinalIgnoreCase))
                    {
                        string imageUrl = emp.Object.image_url?.ToString();
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                var bytes = await client.GetByteArrayAsync(imageUrl);
                                using (var ms = new System.IO.MemoryStream(bytes))
                                {
                                    return Image.FromStream(ms);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching image for SubmittedBy: " + ex.Message);
            }

            return Properties.Resources.User1;
        }

        private void ApplyTooltipIfTruncated(Label label)
        {
            try
            {
                using (Graphics g = label.CreateGraphics())
                {
                    SizeF textSize = g.MeasureString(label.Text, label.Font);
                    if (textSize.Width > label.Width)
                        infoToolTip.SetToolTip(label, label.Text);
                    else
                        infoToolTip.SetToolTip(label, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying tooltip: " + ex.Message);
            }
            }

        //  When revoke is clicked, restore leave + delete record from Firebase
        private async void btnRevoke_Click(object sender, EventArgs e)
        {
            try
            {
                string employeeName = lblEmployee.Text.Trim();
                string leaveType = lblLeaveType.Text.Trim();
                string submittedBy = lblSubmittedBy.Text.Trim();
                string period = lblPeriod.Text.Trim();

                if (string.IsNullOrEmpty(employeeName) || string.IsNullOrEmpty(leaveType))
                {
                    MessageBox.Show("Invalid employee or leave type.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Step 1: Determine total leave days (excluding Sundays)
                int totalDays = 0;
                string[] parts = period.Split('-');

                if (parts.Length == 2)
                {
                    string startPart = parts[0].Trim();
                    string endPart = parts[1].Trim();

                    // 🟢 Support multiple date formats (numeric + long names)
                    string[] formats = {
                "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd",
                "dd/MM/yyyy", "d/M/yyyy",
                "MMMM dd, yyyy", "MMMM d, yyyy",
                "MMM dd, yyyy", "MMM d, yyyy"
            };

                    bool startParsed = DateTime.TryParseExact(
                        startPart,
                        formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime startDate
                    );

                    bool endParsed = DateTime.TryParseExact(
                        endPart,
                        formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime endDate
                    );

                    if (startParsed && endParsed)
                    {
                        for (DateTime d = startDate; d <= endDate; d = d.AddDays(1))
                        {
                            if (d.DayOfWeek != DayOfWeek.Sunday)
                                totalDays++;
                        }
                    }
                }

                if (totalDays <= 0)
                {
                    MessageBox.Show("Invalid or Sunday-only leave period.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Step 2: Add back the totalDays leave credits
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
                    MessageBox.Show($"Employee '{employeeName}' not found in Leave Credits.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int sickLeave = empData.sick_leave != null ? (int)empData.sick_leave : 6;
                int vacationLeave = empData.vacation_leave != null ? (int)empData.vacation_leave : 6;

                if (leaveType.ToLower().Contains("sick"))
                    sickLeave += totalDays;
                else if (leaveType.ToLower().Contains("vacation"))
                    vacationLeave += totalDays;

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

                // ✅ Step 3: Remove record from ManageLeave
                var leaves = await firebase.Child("ManageLeave").OnceAsync<dynamic>();
                foreach (var leave in leaves)
                {
                    string fbSubmitted = (leave.Object.submitted_by ?? "").ToString().Trim();
                    string fbEmployee = (leave.Object.employee ?? "").ToString().Trim();
                    string fbLeaveType = (leave.Object.leave_type ?? "").ToString().Trim();
                    string fbPeriod = (leave.Object.period ?? "").ToString().Trim();

                    if (string.Equals(fbSubmitted, submittedBy, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(fbEmployee, employeeName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(fbLeaveType, leaveType, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(fbPeriod, period, StringComparison.OrdinalIgnoreCase))
                    {
                        await firebase.Child("ManageLeave").Child(leave.Key).DeleteAsync();
                        break;
                    }
                }

                // ✅ Step 4: Remove from UI + trigger refresh
                this.Parent?.Controls.Remove(this);
                RevokeClicked?.Invoke(this, EventArgs.Empty);

                // ✅ Step 5: Refresh ManageLeave form (if open)
                Form parentForm = this.FindForm();
                if (parentForm is ManageLeave manageLeaveForm)
                {
                    await manageLeaveForm.LoadManageLeaveDataAsync();
                }

                MessageBox.Show($"{totalDays} day(s) restored to {employeeName}'s {leaveType} leave.",
                    "Leave Revoked", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error revoking leave: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
