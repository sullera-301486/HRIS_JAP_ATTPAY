using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveNotificationItems : UserControl
    {
        private DateTime createdAt;
        private Timer refreshTimer;

        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public LeaveNotificationItems()
        {
            InitializeComponent();
            SetFont();
        }

        private void SetFont()
        {
            try
            {
                lblLeaveTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                lblSubmittedLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblEmployeeLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPeriod.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblNotes.Font = AttributesClass.GetFont("Roboto-Light", 9f);
                lblTimeAgo.Font = AttributesClass.GetFont("Roboto-Regular", 9f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label6.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                label5.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                btnApprove.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                btnDecline.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        public async void SetData(
            string title,
            string submitted,
            string employee,
            string leaveType,
            string start,
            string end,
            string notes,
            Image photo = null,
            DateTime? created = null,
            bool saveToFirebase = true,
            string firebaseKey = null)
        {
            lblLeaveTitle.Text = title;
            lblSubmittedLeave.Text = submitted;
            lblEmployeeLeave.Text = employee;
            lblLeave.Text = leaveType;
            lblPeriod.Text = $"{start} - {end}";
            lblNotes.Text = notes;

            if (photo != null)
            {
                picEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                picEmployee.Image = photo;
            }
            else
            {
                await LoadEmployeeImageAsync(submitted);
            }

            createdAt = created ?? DateTime.Now;
            UpdateTimeAgo();

            if (refreshTimer == null)
            {
                refreshTimer = new Timer();
                refreshTimer.Interval = 60000;
                refreshTimer.Tick += (s, e) => UpdateTimeAgo();
                refreshTimer.Start();
            }

            this.Tag = firebaseKey;

            if (saveToFirebase)
            {
                bool isValid = await IsEmployeeValidAsync(employee);
                if (!isValid)
                {
                    MessageBox.Show(
                        $"Employee '{employee}' does not exist in the database.",
                        "Invalid Employee",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                var leaveNotif = new LeaveNotificationModel
                {
                    Title = title,
                    SubmittedBy = submitted,
                    Employee = employee,
                    LeaveType = leaveType,
                    Period = $"{start} - {end}",
                    Notes = notes,
                    CreatedAt = createdAt.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await SaveLeaveNotificationAsync(leaveNotif);
            }
        }

        private async Task LoadEmployeeImageAsync(string submittedBy)
        {
            try
            {
                var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                foreach (var emp in employees)
                {
                    string fullName = $"{emp.Object.first_name} {emp.Object.middle_name} {emp.Object.last_name}".Replace("  ", " ").Trim();
                    string firstName = emp.Object.first_name?.ToString();
                    string imageUrl = emp.Object.image_url?.ToString();

                    if (string.IsNullOrEmpty(imageUrl))
                        continue;

                    if (string.Equals(fullName, submittedBy, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(firstName, submittedBy, StringComparison.OrdinalIgnoreCase))
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var bytes = await client.GetByteArrayAsync(imageUrl);
                            using (var ms = new System.IO.MemoryStream(bytes))
                            {
                                picEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                                picEmployee.Image = Image.FromStream(ms);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading employee image: " + ex.Message);
            }
        }

        public void UpdateTimeAgo()
        {
            TimeSpan diff = DateTime.Now - createdAt;

            if (diff.TotalMinutes < 1)
                lblTimeAgo.Text = "Just now";
            else if (diff.TotalMinutes < 60)
                lblTimeAgo.Text = $"{(int)diff.TotalMinutes}m ago";
            else if (diff.TotalHours < 24)
                lblTimeAgo.Text = $"{(int)diff.TotalHours}h ago";
            else
                lblTimeAgo.Text = $"{(int)diff.TotalDays}d ago";
        }

        private static async Task SaveLeaveNotificationAsync(LeaveNotificationModel leaveNotif)
        {
            await firebase.Child("LeaveNotifications").PostAsync(leaveNotif);
        }

        public static async Task<List<LeaveNotificationModelWithKey>> GetAllLeaveNotificationsAsync()
        {
            var notifications = await firebase.Child("LeaveNotifications").OnceAsync<LeaveNotificationModel>();
            List<LeaveNotificationModelWithKey> list = new List<LeaveNotificationModelWithKey>();

            foreach (var item in notifications)
            {
                list.Add(new LeaveNotificationModelWithKey
                {
                    Key = item.Key,
                    Title = item.Object.Title,
                    SubmittedBy = item.Object.SubmittedBy,
                    Employee = item.Object.Employee,
                    LeaveType = item.Object.LeaveType,
                    Period = item.Object.Period,
                    Notes = item.Object.Notes,
                    CreatedAt = item.Object.CreatedAt
                });
            }

            list.Sort((a, b) =>
            {
                DateTime.TryParse(a.CreatedAt, out DateTime aDate);
                DateTime.TryParse(b.CreatedAt, out DateTime bDate);
                return bDate.CompareTo(aDate);
            });

            return list;
        }

        public static async Task DeleteNotificationAsync(string key)
        {
            await firebase.Child("LeaveNotifications").Child(key).DeleteAsync();
        }

        private static async Task<bool> IsEmployeeValidAsync(string employeeName)
        {
            var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
            foreach (var emp in employees)
            {
                string firstName = (emp.Object.first_name ?? "").ToString().Trim();
                string middleName = (emp.Object.middle_name ?? "").ToString().Trim();
                string lastName = (emp.Object.last_name ?? "").ToString().Trim();

                string fullNameWithMiddle = $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();
                string fullNameNoMiddle = $"{firstName} {lastName}".Trim();

                if (string.Equals(fullNameWithMiddle, employeeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fullNameNoMiddle, employeeName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ✅ APPROVE with Sunday Skipped
        private async void btnApprove_Click(object sender, EventArgs e)
        {
            ApproveClicked?.Invoke(this, EventArgs.Empty);

            if (!(Tag is string firebaseKey) || string.IsNullOrEmpty(firebaseKey))
            {
                MessageBox.Show("No Firebase key found — cannot move data.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var notif = await firebase
                    .Child("LeaveNotifications")
                    .Child(firebaseKey)
                    .OnceSingleAsync<LeaveNotificationModel>();

                if (notif == null)
                {
                    MessageBox.Show("Leave notification not found in Firebase.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] periodParts = notif.Period.Split('-');
                if (periodParts.Length != 2)
                {
                    MessageBox.Show("Invalid leave period format.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string startStr = periodParts[0].Trim();
                string endStr = periodParts[1].Trim();

                string[] formats = { "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" };
                if (!DateTime.TryParseExact(startStr, formats, null, System.Globalization.DateTimeStyles.None, out DateTime startDate) ||
                    !DateTime.TryParseExact(endStr, formats, null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    MessageBox.Show($"Failed to parse leave period.\nStart: {startStr}\nEnd: {endStr}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (endDate < startDate)
                {
                    MessageBox.Show("End date cannot be earlier than start date.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Count only non-Sunday days
                int totalDays = 0;
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Sunday)
                        totalDays++;
                }

                await DeductLeaveBalanceAsync(notif.Employee, notif.LeaveType, totalDays);

                // ✅ Move to ManageLeave
                var manageLeaveData = new
                {
                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    employee = notif.Employee,
                    leave_type = notif.LeaveType,
                    notes = notif.Notes,
                    period = notif.Period,
                    submitted_by = notif.SubmittedBy
                };

                await firebase.Child("ManageLeave").PostAsync(manageLeaveData);
                await firebase.Child("LeaveNotifications").Child(firebaseKey).DeleteAsync();
                this.Parent?.Controls.Remove(this);

                MessageBox.Show(
                    $"Leave approved for {notif.Employee}.\nDeducted {totalDays} day(s) (Sundays skipped).",
                    "Leave Approved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error approving leave: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ Deduct leave balance logic
        private static async Task DeductLeaveBalanceAsync(string employeeName, string leaveType, int totalDays)
        {
            try
            {
                var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                string employeeId = null;
                string fullName = null;

                foreach (var emp in employees)
                {
                    string firstName = (emp.Object.first_name ?? "").ToString().Trim();
                    string middleName = (emp.Object.middle_name ?? "").ToString().Trim();
                    string lastName = (emp.Object.last_name ?? "").ToString().Trim();

                    string fullNameWithMiddle = $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();
                    string fullNameNoMiddle = $"{firstName} {lastName}".Trim();

                    if (string.Equals(fullNameWithMiddle, employeeName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(fullNameNoMiddle, employeeName, StringComparison.OrdinalIgnoreCase))
                    {
                        employeeId = emp.Key;
                        fullName = fullNameWithMiddle;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show($"Employee '{employeeName}' not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var currentLeave = await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                int sickLeave = 6;
                int vacationLeave = 6;

                if (currentLeave != null)
                {
                    try { sickLeave = Convert.ToInt32(currentLeave.sick_leave); } catch { }
                    try { vacationLeave = Convert.ToInt32(currentLeave.vacation_leave); } catch { }
                }

                if (leaveType.ToLower().Contains("sick"))
                    sickLeave = Math.Max(0, sickLeave - totalDays);
                else if (leaveType.ToLower().Contains("vacation"))
                    vacationLeave = Math.Max(0, vacationLeave - totalDays);

                var updatedCredits = new
                {
                    employee_id = employeeId,
                    full_name = fullName,
                    sick_leave = sickLeave,
                    vacation_leave = vacationLeave,
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase.Child("Leave Credits").Child(employeeId).PutAsync(updatedCredits);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deducting leave balance: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class LeaveNotificationModel
        {
            public string Title { get; set; }
            public string SubmittedBy { get; set; }
            public string Employee { get; set; }
            public string LeaveType { get; set; }
            public string Period { get; set; }
            public string Notes { get; set; }
            public string CreatedAt { get; set; }
        }

        public class LeaveNotificationModelWithKey : LeaveNotificationModel
        {
            public string Key { get; set; }
        }

        public event EventHandler ApproveClicked;
        public event EventHandler DeclineClicked;

        private async void btnDecline_Click(object sender, EventArgs e)
        {
            if (Tag is string firebaseKey && !string.IsNullOrEmpty(firebaseKey))
            {
                await DeleteNotificationAsync(firebaseKey);
            }

            this.Parent?.Controls.Remove(this);
            DeclineClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
