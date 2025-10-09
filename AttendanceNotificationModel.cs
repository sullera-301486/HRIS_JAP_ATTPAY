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
        public string EmployeeId { get; set; }
        public string Date { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string OvertimeIn { get; set; }
        public string OvertimeOut { get; set; }
        public string CreatedAt { get; set; }

        public DateTime CreatedAtDateTime
        {
            get
            {
                if (DateTime.TryParse(CreatedAt, out DateTime dt))
                    return dt;
                return DateTime.Now;
            }
        }

        public static async Task<List<AttendanceNotificationModel>> GetAllAttendanceNotificationsAsync()
        {
            var firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
            var result = await firebase.Child("AttendanceNotifications").OnceAsync<AttendanceNotificationModel>();
            List<AttendanceNotificationModel> list = new List<AttendanceNotificationModel>();

            foreach (var item in result)
            {
                var obj = item.Object;
                obj.Key = item.Key;
                list.Add(obj);
            }

            return list;
        }
    }
}
