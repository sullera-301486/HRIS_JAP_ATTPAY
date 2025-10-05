using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq; // ✅ Added for OrderByDescending

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminNotification : Form
    {
        public AdminNotification()
        {
            InitializeComponent();
            SetFont();
        }

        private async void AdminNotification_Load(object sender, EventArgs e)
        {
            await LoadNotifications();
        }

        private async Task LoadNotifications()
        {
            flowLayoutPanel1.Controls.Clear();

            // 🔹 Fetch notifications
            var notifications = await LeaveNotificationItems.GetAllLeaveNotificationsAsync();

            // ✅ Sort notifications by CreatedAt (most recent first)
            notifications = notifications
                .OrderByDescending(n => DateTime.Parse(n.CreatedAt))
                .ToList();

            foreach (var notif in notifications)
            {
                var leaveNotif = new LeaveNotificationItems();

                // Build UI but don’t save again
                leaveNotif.SetData(
                    notif.Title ?? ("Leave Request - " + notif.LeaveType),
                    notif.SubmittedBy ?? ("Submitted by " + notif.Employee),
                    notif.Employee,
                    notif.LeaveType,
                    notif.Period.Split('-')[0].Trim(),
                    notif.Period.Split('-')[1].Trim(),
                    notif.Notes,
                    null,
                    DateTime.Parse(notif.CreatedAt),
                    saveToFirebase: false,
                    firebaseKey: notif.Key
                );

                // Approve button → remove from Firebase + UI
                leaveNotif.ApproveClicked += async (s, ev) =>
                {
                    await LeaveNotificationItems.DeleteNotificationAsync(notif.Key);
                    flowLayoutPanel1.Controls.Remove(leaveNotif);
                };

                // Decline button → remove from Firebase + UI
                leaveNotif.DeclineClicked += async (s, ev) =>
                {
                    await LeaveNotificationItems.DeleteNotificationAsync(notif.Key);
                    flowLayoutPanel1.Controls.Remove(leaveNotif);
                };

                // ✅ Add controls in order (since we already sorted)
                flowLayoutPanel1.Controls.Add(leaveNotif);
            }
        }

        private void SetFont()
        {
            labelNotification.Font = AttributesClass.GetFont("Roboto-Regular", 26f);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
