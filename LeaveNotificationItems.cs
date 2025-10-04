using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveNotificationItems : UserControl
    {
        private DateTime createdAt;
        private Timer refreshTimer;

        // 🔹 Firebase client (your actual DB URL)
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
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label6.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label5.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
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

            createdAt = created ?? DateTime.Now;
            UpdateTimeAgo();

            if (refreshTimer == null)
            {
                refreshTimer = new Timer();
                refreshTimer.Interval = 60000;
                refreshTimer.Tick += (s, e) => UpdateTimeAgo();
                refreshTimer.Start();
            }

            // Store Firebase key (so buttons know what to delete)
            this.Tag = firebaseKey;

            // 🔹 Save to Firebase if it's a new record
            if (saveToFirebase)
            {
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

        /// <summary>
        /// Update the 'time ago' text.
        /// </summary>
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

        // 🔹 Save to Firebase
        private static async Task SaveLeaveNotificationAsync(LeaveNotificationModel leaveNotif)
        {
            await firebase
              .Child("LeaveNotifications")
              .PostAsync(leaveNotif);
        }

        // 🔹 Load all notifications from Firebase
        public static async Task<List<LeaveNotificationModelWithKey>> GetAllLeaveNotificationsAsync()
        {
            var notifications = await firebase
                .Child("LeaveNotifications")
                .OnceAsync<LeaveNotificationModel>();

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
            return list;
        }

        // 🔹 Delete notification from Firebase
        public static async Task DeleteNotificationAsync(string key)
        {
            await firebase
                .Child("LeaveNotifications")
                .Child(key)
                .DeleteAsync();
        }

        // 🔹 Model class
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

        // 🔹 Model with Firebase key (for updates/deletes)
        public class LeaveNotificationModelWithKey : LeaveNotificationModel
        {
            public string Key { get; set; }
        }

        // Button events
        public event EventHandler ApproveClicked;
        public event EventHandler DeclineClicked;

        private void btnApprove_Click(object sender, EventArgs e)
        {
            ApproveClicked?.Invoke(this, EventArgs.Empty);
        }

        // 🔹 Decline button removes from Firebase + UI
        private async void btnDecline_Click(object sender, EventArgs e)
        {
            if (Tag is string firebaseKey && !string.IsNullOrEmpty(firebaseKey))
            {
                // Remove from Firebase
                await DeleteNotificationAsync(firebaseKey);
            }

            // Remove from parent container (UI)
            this.Parent?.Controls.Remove(this);

            // Fire event
            DeclineClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
