using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminNotification : Form
    {
        
        public AdminNotification()
        {
            InitializeComponent();
            SetFont();
        }

        private void AdminNotification_Load(object sender, EventArgs e)
        {
            
            // Attendance Notification 1
            var attendanceNotif1 = new AttendanceNotificationItems();
            attendanceNotif1.SetData(
                "Manual Edit Request",
                "John Smith",
                "Mark Reyes",
                DateTime.Now,
                "8:05 AM",
                "5:10 PM",
                "7:30 PM",
                "10:00 PM",
                Properties.Resources.User1 // 👈 sample image
            );
            attendanceNotif1.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif1);
            attendanceNotif1.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif1);
            flowLayoutPanel1.Controls.Add(attendanceNotif1);

            // Leave Notification with photo
            var leaveNotif1 = new LeaveNotificationItems();
            leaveNotif1.SetData(
                "Leave Request - Vacation",
                "Marcus Verzo",
                "Ej Sullera",
                "Vacation Leave",
                "May 5 - May 8, 2025",
                "Ej is requesting a vacation leave to attend a family event.",
                Properties.Resources.User1 // 👈 another sample image
            );
            leaveNotif1.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(leaveNotif1);
            leaveNotif1.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(leaveNotif1);
            flowLayoutPanel1.Controls.Add(leaveNotif1);

            var attendanceNotif2 = new AttendanceNotificationItems();
            attendanceNotif2.SetData(
                "Manual Attendance Entry Request",
                "Maria Lopez",
                "Carlo Mendoza",
                DateTime.Now.AddDays(-1),
                "7:50 AM",
                "5:20 PM",
                "5:30 PM",
                "8:00 PM",
                Properties.Resources.User1
            );
            attendanceNotif2.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif2);
            attendanceNotif2.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif2);
            flowLayoutPanel1.Controls.Add(attendanceNotif2);

            // Leave Notification 2
            var leaveNotif2 = new LeaveNotificationItems();
            leaveNotif2.SetData(
                "Leave Request - Sick Leave",
                "Anna Cruz",
                "Paolo Santos",
                "Sick Leave",
                "June 10 - June 12, 2025",
                "Paolo is requesting sick leave due to flu and has provided a medical certificate.",
                Properties.Resources.User1
            );
            leaveNotif2.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(leaveNotif2);
            leaveNotif2.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(leaveNotif2);
            flowLayoutPanel1.Controls.Add(leaveNotif2);

            // Attendance Notification 3
            var attendanceNotif3 = new AttendanceNotificationItems();
            attendanceNotif3.SetData(
                "Overtime Approval Request",
                "Charles Dela Cruz",
                "Lea Villanueva",
                DateTime.Now.AddDays(-2),
                "9:00 AM",
                "6:00 PM",
                "6:30 PM",
                "11:00 PM",
                Properties.Resources.User1
            );
            attendanceNotif3.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif3);
            attendanceNotif3.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif3);
            flowLayoutPanel1.Controls.Add(attendanceNotif3);

            // Leave Notification 3
            var leaveNotif3 = new LeaveNotificationItems();
            leaveNotif3.SetData(
                "Leave Request - Emergency",
                "Mark Javier",
                "Sofia Cruz",
                "Emergency Leave",
                "June 15 - June 16, 2025",
                "Sofia has a family emergency and is requesting 2 days of leave.",
                Properties.Resources.User1
            );
            leaveNotif3.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(leaveNotif3);
            leaveNotif3.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(leaveNotif3);
            flowLayoutPanel1.Controls.Add(leaveNotif3);

            // Attendance Notification 4
            var attendanceNotif4 = new AttendanceNotificationItems();
            attendanceNotif4.SetData(
                "Attendance Correction Request",
                "Kevin Ramos",
                "Diana Lim",
                DateTime.Now.AddDays(-3),
                "8:10 AM",
                "5:05 PM",
                "N/A",
                "N/A",
                Properties.Resources.User1
            );
            attendanceNotif4.ApproveClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif4);
            attendanceNotif4.DeclineClicked += (s, ev) => flowLayoutPanel1.Controls.Remove(attendanceNotif4);
            flowLayoutPanel1.Controls.Add(attendanceNotif4);
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
