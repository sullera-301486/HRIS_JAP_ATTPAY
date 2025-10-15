using System;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    /// <summary>
    /// Service for logging user activities to Firebase
    /// </summary>
    public static class ActivityLogService
    {
        private static FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        /// <summary>
        /// Logs a user activity to Firebase
        /// </summary>
        public static async Task LogActivity(string action, string description, string performedByEmployeeId = null, string affectedEmployeeId = null)
        {
            try
            {
                // DEBUG: Check session values
                string currentUserId = SessionClass.CurrentUserId;
                string currentUserName = SessionClass.CurrentEmployeeName;
                string currentEmployeeId = SessionClass.CurrentEmployeeId;

                Console.WriteLine($"DEBUG: Activity Log Session - UserID: {currentUserId}, Name: {currentUserName}, EmployeeID: {currentEmployeeId}");

                if (string.IsNullOrEmpty(currentUserId))
                {
                    Console.WriteLine("ERROR: No user session found for activity log");
                    return; // Exit if no session
                }

                // Use provided employee ID or fall back to session
                string performerEmployeeId = performedByEmployeeId ?? currentEmployeeId;
                string performerName = currentUserName;

                // If a different employee performed the action, get their name
                if (!string.IsNullOrEmpty(performedByEmployeeId) && performedByEmployeeId != currentEmployeeId)
                {
                    performerName = await GetEmployeeFullName(performedByEmployeeId);
                }

                string affectedEmployeeName = "";
                if (!string.IsNullOrEmpty(affectedEmployeeId))
                {
                    affectedEmployeeName = await GetEmployeeFullName(affectedEmployeeId);
                }

                var logEntry = new
                {
                    action_type = action,
                    performed_by_employee_id = performerEmployeeId,
                    performed_by_name = performerName,
                    performed_by_user_id = currentUserId,
                    description = description,
                    details = affectedEmployeeId != null ? $"Employee ID: {affectedEmployeeId}" : "",
                    target_employee_id = affectedEmployeeId ?? "N/A",
                    target_employee_name = affectedEmployeeName,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                Console.WriteLine($"DEBUG: Saving Activity to Firebase - {description}");

                await firebase
                    .Child("ActivityLogs")
                    .PostAsync(logEntry);

                Console.WriteLine($"SUCCESS: Activity log created: {action} by {performerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Activity logging failed - {ex.Message}");
                // Don't show messagebox to avoid interrupting the flow
            }
        }

        /// <summary>
        /// Gets full name of an employee from Firebase
        /// </summary>
        private static async Task<string> GetEmployeeFullName(string employeeId)
        {
            try
            {
                var employee = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (employee != null)
                {
                    string firstName = employee.first_name?.ToString() ?? "";
                    string middleName = employee.middle_name?.ToString() ?? "";
                    string lastName = employee.last_name?.ToString() ?? "";

                    return $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee name for activity log: {ex.Message}");
            }
            return "Unknown Employee";
        }

        /// <summary>
        /// Common activity actions for consistency
        /// </summary>
        public static class Actions
        {
            // Attendance related
            public const string ATTENDANCE_EDIT_REQUESTED = "Attendance Edit Requested";
            public const string ATTENDANCE_EDIT_APPROVED = "Attendance Edit Approved";
            public const string ATTENDANCE_EDIT_DECLINED = "Attendance Edit Declined";
            public const string ATTENDANCE_MANUALLY_ADDED = "Attendance Manually Added";
            public const string ATTENDANCE_UPDATED = "Attendance Updated";

            // Leave related
            public const string LEAVE_REQUESTED = "Leave Requested";
            public const string LEAVE_APPROVED = "Leave Approved";
            public const string LEAVE_DECLINED = "Leave Declined";

            // Time tracking
            public const string TIME_IN_RECORDED = "Time In Recorded";
            public const string TIME_OUT_RECORDED = "Time Out Recorded";
            public const string OVERTIME_RECORDED = "Overtime Recorded";

            // Profile related
            public const string PROFILE_UPDATED = "Profile Updated";
            public const string PASSWORD_CHANGED = "Password Changed";

            // System activities
            public const string LOGIN = "User Login";
            public const string LOGOUT = "User Logout";
            public const string REPORT_GENERATED = "Report Generated";
            public const string DATA_EXPORTED = "Data Exported";
        }
    }
}