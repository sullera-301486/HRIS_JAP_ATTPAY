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

        // 🔹 Firebase client
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

        /// <summary>
        /// Bind data from LeaveRequestData. Saves to Firebase if saveToFirebase = true.
        /// </summary>
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
                refreshTimer.Interval = 60000; // every minute
                refreshTimer.Tick += (s, e) => UpdateTimeAgo();
                refreshTimer.Start();
            }

            this.Tag = firebaseKey;

            // 🔹 Validate employee before saving new data
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

        // 🆕 Load employee photo from Firebase Storage URL
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

        // 🔹 Save notification to Firebase
        private static async Task SaveLeaveNotificationAsync(LeaveNotificationModel leaveNotif)
        {
            await firebase.Child("LeaveNotifications").PostAsync(leaveNotif);
        }

        // 🔹 Load all notifications from Firebase (newest first)
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

        // 🔹 Delete notification
        public static async Task DeleteNotificationAsync(string key)
        {
            await firebase.Child("LeaveNotifications").Child(key).DeleteAsync();
        }

        // 🔹 Validate employee
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

        // 🆕 Deduct leave & handle yearly reset (only January 1)
        private static async Task DeductLeaveBalanceAsync(string employeeName, string leaveType)
        {
            try
            {
                var employees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                // 🧠 Find employee ID by flexible name matching
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
                        string.Equals(fullNameNoMiddle, employeeName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(firstName, employeeName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(lastName, employeeName, StringComparison.OrdinalIgnoreCase))
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

                // 🟢 Get or create leave credit
                var currentLeave = await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                int sickLeave = 6;
                int vacationLeave = 6;
                int lastUpdatedYear = DateTime.Now.Year;

                if (currentLeave != null)
                {
                    sickLeave = currentLeave.sick_leave != null ? (int)currentLeave.sick_leave : 6;
                    vacationLeave = currentLeave.vacation_leave != null ? (int)currentLeave.vacation_leave : 6;

                    if (currentLeave.updated_at != null)
                    {
                        DateTime.TryParse(currentLeave.updated_at.ToString(), out DateTime parsedDate);
                        if (parsedDate != default)
                            lastUpdatedYear = parsedDate.Year;
                    }
                }

                // 🧠 Reset on January 1 of new year
                if (lastUpdatedYear < DateTime.Now.Year)
                {
                    sickLeave = 6;
                    vacationLeave = 6;
                }

                // Deduct depending on leave type
                if (leaveType.ToLower().Contains("sick"))
                    sickLeave = Math.Max(0, sickLeave - 1);
                else if (leaveType.ToLower().Contains("vacation"))
                    vacationLeave = Math.Max(0, vacationLeave - 1);

                var updatedCredits = new
                {
                    employee_id = employeeId,
                    full_name = fullName,
                    sick_leave = sickLeave,
                    vacation_leave = vacationLeave,
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .PutAsync(updatedCredits);

                MessageBox.Show(
                    $"{employeeName}'s {leaveType} leave deducted successfully.\n" +
                    $"Sick Leave: {sickLeave}\nVacation Leave: {vacationLeave}",
                    "Leave Deducted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deducting leave balance: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🔹 Models
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

        // 🔹 Events
        public event EventHandler ApproveClicked;
        public event EventHandler DeclineClicked;

        private async void btnApprove_Click(object sender, EventArgs e)
        {
            await DeductLeaveBalanceAsync(lblEmployeeLeave.Text, lblLeave.Text);
            ApproveClicked?.Invoke(this, EventArgs.Empty);

            if (Tag is string firebaseKey && !string.IsNullOrEmpty(firebaseKey))
            {
                await DeleteNotificationAsync(firebaseKey);
                this.Parent?.Controls.Remove(this);
            }
        }

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
