using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

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

            // 🔹 Fetch notifications from Firebase
            var notifications = await LeaveNotificationItems.GetAllLeaveNotificationsAsync();

            // ✅ Sort newest first
            notifications = notifications
                .OrderByDescending(n => DateTime.Parse(n.CreatedAt))
                .ToList();

            foreach (var notif in notifications)
            {
                var leaveNotif = new LeaveNotificationItems();

                // ✅ Pass firebaseKey (notif.Key) so Approve button can move it
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

                // ✅ After approve or decline, just refresh the list (no manual delete)
                leaveNotif.ApproveClicked += async (s, ev) =>
                {
                    await Task.Delay(500); // small delay to allow Firebase update
                    await LoadNotifications();
                };

                leaveNotif.DeclineClicked += async (s, ev) =>
                {
                    await Task.Delay(500);
                    await LoadNotifications();
                };

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
