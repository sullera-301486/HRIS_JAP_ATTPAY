using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public class AttendanceNotificationModel
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string SubmittedBy { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public string Date { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string OvertimeIn { get; set; }
        public string OvertimeOut { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }

        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public static async Task<List<AttendanceNotificationModel>> GetAllAttendanceNotificationsAsync()
        {
            var list = new List<AttendanceNotificationModel>();

            try
            {
                var firebaseItems = await firebase.Child("AttendanceNotifications").OnceAsync<dynamic>();
                foreach (var item in firebaseItems)
                {
                    var obj = item.Object;
                    if (obj == null) continue;

                    list.Add(new AttendanceNotificationModel
                    {
                        Key = item.Key,
                        Title = obj.title,
                        SubmittedBy = obj.submitted_by,
                        EmployeeName = obj.employee_name,
                        EmployeeId = obj.employee_id,
                        Date = obj.date,
                        TimeIn = obj.time_in,
                        TimeOut = obj.time_out,
                        OvertimeIn = obj.overtime_in,
                        OvertimeOut = obj.overtime_out,
                        Status = obj.status,
                        CreatedAt = obj.created_at
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error fetching AttendanceNotifications: {ex.Message}");
            }

            return list;
        }
    }
}
