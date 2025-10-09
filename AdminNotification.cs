using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminNotification : Form
    {
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

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

            var allNotifications = new List<NotificationBase>();

            // 🔹 1. Load Leave Notifications
            var leaveNotifications = await LeaveNotificationItems.GetAllLeaveNotificationsAsync();
            allNotifications.AddRange(leaveNotifications.Select(n => new NotificationBase
            {
                Key = n.Key,
                Type = "Leave",
                Title = n.Title ?? ("Leave Request - " + n.LeaveType),
                SubmittedBy = n.SubmittedBy ?? ("Submitted by " + n.Employee),
                Employee = n.Employee,
                CreatedAt = n.CreatedAt,
                LeaveType = n.LeaveType,
                Period = n.Period,
                Notes = n.Notes
            }));

            // 🔹 2. Load Attendance Notifications
            var attendanceData = await firebase.Child("AttendanceNotifications").OnceAsync<JObject>();
            foreach (var att in attendanceData)
            {
                var data = att.Object;
                if (data == null) continue;

                string status = data["request_status"]?.ToString() ?? "Pending";
                if (!status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    continue;

                allNotifications.Add(new NotificationBase
                {
                    Key = att.Key,
                    Type = "Attendance",
                    Title = data["request_type"]?.ToString() ?? "Manual Edit Request",
                    SubmittedBy = data["requested_by_name"]?.ToString() ?? "N/A",
                    Employee = data["employee_id"]?.ToString() ?? "N/A",
                    CreatedAt = data["request_timestamp"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeIn = data["time_in"]?.ToString() ?? "N/A",
                    TimeOut = data["time_out"]?.ToString() ?? "N/A",
                    OvertimeIn = data["overtime_in"]?.ToString() ?? "N/A",
                    OvertimeOut = data["overtime_out"]?.ToString() ?? "N/A",
                    AttendanceDate = data["attendance_date"]?.ToString() ?? "N/A"
                });
            }

            // ✅ Sort by CreatedAt descending
            allNotifications = allNotifications
                .OrderByDescending(n => SafeParseDate(n.CreatedAt))
                .ToList();

            // 🔹 3. Render all notifications
            foreach (var notif in allNotifications)
            {
                if (notif.Type == "Leave")
                {
                    var leaveNotif = new LeaveNotificationItems();

                    DateTime created = SafeParseDate(notif.CreatedAt);
                    string[] periodParts = notif.Period?.Split('-') ?? new[] { "N/A", "N/A" };

                    leaveNotif.SetData(
                        notif.Title,
                        notif.SubmittedBy,
                        notif.Employee,
                        notif.LeaveType,
                        periodParts[0].Trim(),
                        periodParts.Length > 1 ? periodParts[1].Trim() : "N/A",
                        notif.Notes,
                        null,
                        created,
                        saveToFirebase: false,
                        firebaseKey: notif.Key
                    );

                    leaveNotif.ApproveClicked += async (s, ev) =>
                    {
                        await Task.Delay(300);
                        await LoadNotifications();
                    };
                    leaveNotif.DeclineClicked += async (s, ev) =>
                    {
                        await Task.Delay(300);
                        await LoadNotifications();
                    };

                    flowLayoutPanel1.Controls.Add(leaveNotif);
                }
                else if (notif.Type == "Attendance")
                {
                    var attNotif = new AttendanceNotificationItems();
                    DateTime parsedDate = DateTime.TryParse(notif.AttendanceDate, out DateTime tempDate)
                        ? tempDate : DateTime.Now;
                    DateTime createdAt = SafeParseDate(notif.CreatedAt); // ⏰ Use request_timestamp for time ago

                    // ✅ Pass createdAt to SetData so "time ago" displays correctly
                    attNotif.SetData(
                        notif.Title,
                        notif.SubmittedBy,
                        notif.Employee,
                        parsedDate,
                        notif.TimeIn,
                        notif.TimeOut,
                        notif.OvertimeIn,
                        notif.OvertimeOut,
                        null,
                        createdAt
                    );

                    // 🔹 APPROVE BUTTON ACTION
                    attNotif.ApproveClicked += async (s, ev) =>
                    {
                        await ApproveAttendanceRequest(notif.Key);
                        await LoadNotifications();
                    };

                    // 🔹 DECLINE BUTTON ACTION
                    attNotif.DeclineClicked += async (s, ev) =>
                    {
                        await DeclineAttendanceRequest(notif.Key);
                        await LoadNotifications();
                    };

                    flowLayoutPanel1.Controls.Add(attNotif);
                }
            }

            // 🔸 If no notifications
            if (allNotifications.Count == 0)
            {
                flowLayoutPanel1.Controls.Add(new Label()
                {
                    Text = "No pending notifications.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.Gray
                });
            }
        }

        // ✅ Helper: Safe date parse
        private DateTime SafeParseDate(string dateStr)
        {
            if (DateTime.TryParse(dateStr, out DateTime result))
                return result;
            return DateTime.Now;
        }

        // --- ATTENDANCE APPROVE / DECLINE ---

        private async Task ApproveAttendanceRequest(string key)
        {
            try
            {
                // 🔹 Step 1: Retrieve notification data
                var notif = await firebase.Child("AttendanceNotifications").Child(key).OnceSingleAsync<JObject>();
                if (notif == null)
                {
                    MessageBox.Show("Notification not found.");
                    return;
                }

                // Extract all fields first
                string employeeId = notif["employee_id"]?.ToString() ?? "Unknown";
                string attendanceDate = notif["attendance_date"]?.ToString() ?? "Unknown";
                string timeIn = notif["time_in"]?.ToString() ?? "N/A";
                string timeOut = notif["time_out"]?.ToString() ?? "N/A";
                string overtimeIn = notif["overtime_in"]?.ToString() ?? "N/A";
                string overtimeOut = notif["overtime_out"]?.ToString() ?? "N/A";
                string hoursWorked = notif["hours_worked"]?.ToString() ?? "0.00";
                string overtimeHours = notif["overtime_hours"]?.ToString() ?? "0.00";
                string status = notif["status"]?.ToString() ?? "Approved";
                string firebaseKey = notif["firebase_key"]?.ToString() ?? "";

                // 🔹 Step 2: Update the Attendance table
                if (!string.IsNullOrEmpty(firebaseKey))
                {
                    var updatedRecord = new
                    {
                        employee_id = employeeId,
                        attendance_date = attendanceDate,
                        time_in = timeIn,
                        time_out = timeOut,
                        overtime_in = overtimeIn,
                        overtime_out = overtimeOut,
                        hours_worked = hoursWorked,
                        overtime_hours = overtimeHours,
                        status = status,
                        verification_method = "Approved Manual Edit (Admin)"
                    };

                    await firebase.Child("Attendance").Child(firebaseKey).PutAsync(updatedRecord);
                }

                // 🔹 Step 3: Delete the notification after updating attendance
                await firebase.Child("AttendanceNotifications").Child(key).DeleteAsync();

                // 🔹 Step 4: Refresh UI
                MessageBox.Show("Attendance request approved and removed successfully!");
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error approving attendance: " + ex.Message);
            }
        }

        private async Task DeclineAttendanceRequest(string key)
        {
            try
            {
                // 🔹 Directly delete the declined notification
                await firebase.Child("AttendanceNotifications").Child(key).DeleteAsync();

                // 🔹 Refresh UI after deletion
                MessageBox.Show("Attendance request declined and removed successfully!");
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error declining attendance: " + ex.Message);
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

        // --- Internal Notification Data Structure ---
        private class NotificationBase
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public string SubmittedBy { get; set; }
            public string Employee { get; set; }
            public string CreatedAt { get; set; }
            public string LeaveType { get; set; }
            public string Period { get; set; }
            public string Notes { get; set; }
            public string AttendanceDate { get; set; }
            public string TimeIn { get; set; }
            public string TimeOut { get; set; }
            public string OvertimeIn { get; set; }
            public string OvertimeOut { get; set; }
        }
    }
}
