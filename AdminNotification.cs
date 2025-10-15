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

                    // ✅ ADD HR NOTIFICATION FOR PENDING LEAVE
                    await firebase.Child("HRNotifications").Child(notif.Key).PutAsync(new
                    {
                        message = $"{notif.Employee} filed a leave request ({notif.LeaveType}) pending admin review.",
                        status = "Pending",
                        createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    leaveNotif.ApproveClicked += async (s, ev) =>
                    {
                        await ApproveLeaveRequest(notif.Key, notif.Employee, notif.LeaveType);
                        await LoadNotifications();
                    };
                    leaveNotif.DeclineClicked += async (s, ev) =>
                    {
                        await DeclineLeaveRequest(notif.Key, notif.Employee, notif.LeaveType);
                        await LoadNotifications();
                    };

                    flowLayoutPanel1.Controls.Add(leaveNotif);
                }
                else if (notif.Type == "Attendance")
                {
                    var attNotif = new AttendanceNotificationItems();
                    DateTime parsedDate = DateTime.TryParse(notif.AttendanceDate, out DateTime tempDate)
                        ? tempDate : DateTime.Now;
                    DateTime createdAt = SafeParseDate(notif.CreatedAt);

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

                    // ✅ ADD HR NOTIFICATION FOR PENDING ATTENDANCE
                    await firebase.Child("HRNotifications").Child(notif.Key).PutAsync(new
                    {
                        message = $"{notif.SubmittedBy} has a pending attendance edit request for {notif.AttendanceDate}.",
                        status = "Pending",
                        createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    attNotif.ApproveClicked += async (s, ev) =>
                    {
                        await ApproveAttendanceRequest(notif.Key);
                        await LoadNotifications();
                    };

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
                var notif = await firebase.Child("AttendanceNotifications").Child(key).OnceSingleAsync<JObject>();
                if (notif == null)
                {
                    MessageBox.Show("Notification not found.");
                    return;
                }

                string employeeName = notif["requested_by_name"]?.ToString() ?? "Unknown";
                string employeeId = notif["employee_id"]?.ToString() ?? "Unknown";
                string date = notif["attendance_date"]?.ToString() ?? "Unknown";
                string firebaseKey = notif["firebase_key"]?.ToString();

                // ✅ LOG THE ACTIVITY - ADD THIS SECTION
                await ActivityLogService.LogActivity(
                    ActivityLogService.Actions.ATTENDANCE_EDIT_APPROVED,
                    $"Approved attendance edit request for {employeeName} on {date}",
                    SessionClass.CurrentEmployeeId, // admin who approved
                    employeeId // affected employee
                );

                // ✅ UPDATE THE ORIGINAL ATTENDANCE RECORD (your existing code)
                if (!string.IsNullOrEmpty(firebaseKey))
                {
                    var updatedAttendanceRecord = new
                    {
                        employee_id = notif["employee_id"]?.ToString(),
                        attendance_date = notif["attendance_date"]?.ToString(),
                        time_in = notif["time_in"]?.ToString(),
                        time_out = notif["time_out"]?.ToString(),
                        overtime_in = notif["overtime_in"]?.ToString(),
                        overtime_out = notif["overtime_out"]?.ToString(),
                        hours_worked = notif["hours_worked"]?.ToString(),
                        status = notif["status"]?.ToString(),
                        overtime_hours = notif["overtime_hours"]?.ToString(),
                        verification_method = "Approved Manual Edit (Admin)",
                        schedule_id = ""
                    };

                    await firebase.Child("Attendance").Child(firebaseKey).PutAsync(updatedAttendanceRecord);
                }

                // ✅ Notify HR (your existing code)
                await firebase.Child("HRNotifications").Child(key).PutAsync(new
                {
                    message = $"{employeeName}'s attendance on {date} approved by admin.",
                    status = "Approved",
                    createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });

                await firebase.Child("AttendanceNotifications").Child(key).DeleteAsync();
                MessageBox.Show("Attendance request approved and attendance record updated!");
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
                var notif = await firebase.Child("AttendanceNotifications").Child(key).OnceSingleAsync<JObject>();
                if (notif == null) return;

                string employeeName = notif["requested_by_name"]?.ToString() ?? "Unknown";
                string employeeId = notif["employee_id"]?.ToString() ?? "Unknown";
                string date = notif["attendance_date"]?.ToString() ?? "Unknown";

                // ✅ LOG THE ACTIVITY - ADD THIS SECTION
                await ActivityLogService.LogActivity(
                    ActivityLogService.Actions.ATTENDANCE_EDIT_DECLINED,
                    $"Declined attendance edit request for {employeeName} on {date}",
                    SessionClass.CurrentEmployeeId, // admin who declined
                    employeeId // affected employee
                );

                // ✅ Notify HR (your existing code)
                await firebase.Child("HRNotifications").Child(key).PutAsync(new
                {
                    message = $"{employeeName}'s attendance on {date} was declined by admin.",
                    status = "Declined",
                    createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });

                await firebase.Child("AttendanceNotifications").Child(key).DeleteAsync();
                MessageBox.Show("Attendance request declined!");
                await LoadNotifications();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error declining attendance: " + ex.Message);
            }
        }

        // --- LEAVE APPROVE / DECLINE ---
        private async Task ApproveLeaveRequest(string key, string employee, string leaveType)
        {
            await firebase.Child("HRNotifications").Child(key).PutAsync(new
            {
                message = $"{employee}'s {leaveType} leave request approved by admin.",
                status = "Approved",
                createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            await firebase.Child("LeaveNotifications").Child(key).DeleteAsync();
            MessageBox.Show("Leave request approved!");
        }

        private async Task DeclineLeaveRequest(string key, string employee, string leaveType)
        {
            await firebase.Child("HRNotifications").Child(key).PutAsync(new
            {
                message = $"{employee}'s {leaveType} leave request declined by admin.",
                status = "Declined",
                createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            await firebase.Child("LeaveNotifications").Child(key).DeleteAsync();
            MessageBox.Show("Leave request declined!");
        }

        private void SetFont()
        {
            labelNotification.Font = AttributesClass.GetFont("Roboto-Regular", 26f);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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
