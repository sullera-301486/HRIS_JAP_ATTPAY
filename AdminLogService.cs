using System;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    /// <summary>
    /// Service for logging admin actions to Firebase
    /// </summary>
    public static class AdminLogService
    {
        private static FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        public const string ADD_LEAVE_ENTRY = "Leave Entry Added";
        public const string APPROVE_LEAVE = "Leave Approved";

        /// <summary>
        /// Logs an admin action to Firebase
        /// </summary>
        public static async Task LogAdminAction(string action, string description, string affectedEmployeeId = null)
        {
            try
            {
                // DEBUG: Check session values
                string adminUserId = SessionClass.CurrentUserId;
                string adminName = SessionClass.CurrentEmployeeName;

                Console.WriteLine($"DEBUG: Session - UserID: {adminUserId}, Name: {adminName}");

                if (string.IsNullOrEmpty(adminUserId))
                {
                    Console.WriteLine("ERROR: No user session found");
                    return; // Exit if no session
                }

                // Rest of your existing code remains the same
                string affectedEmployeeName = "";
                if (!string.IsNullOrEmpty(affectedEmployeeId))
                {
                    affectedEmployeeName = await GetEmployeeFullName(affectedEmployeeId);
                }

                var logEntry = new
                {
                    action_type = action,
                    admin_employee_id = adminUserId,
                    admin_name = adminName,
                    admin_user_id = adminUserId,
                    description = description,
                    details = affectedEmployeeId != null ? $"Employee ID: {affectedEmployeeId}" : "",
                    target_employee_id = affectedEmployeeId ?? "N/A",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                Console.WriteLine($"DEBUG: Saving to Firebase - {description}");

                await firebase
                    .Child("AdminLogs")
                    .PostAsync(logEntry);

                Console.WriteLine($"SUCCESS: Admin log created: {action} by {adminName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Logging failed - {ex.Message}");
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
                Console.WriteLine($"Error getting employee name: {ex.Message}");
            }
            return "Unknown Employee";
        }

        /// <summary>
        /// Common log actions for consistency
        /// </summary>
        public static class Actions
        {
            public const string ADD_EMPLOYEE = "Employee Added";
            public const string UPDATE_EMPLOYEE = "Employee Updated";
            public const string ARCHIVE_EMPLOYEE = "Employee Archived";
            public const string UPDATE_ATTENDANCE = "Attendance Updated";
            public const string APPROVE_LEAVE = "Leave Approved";
            public const string REJECT_LEAVE = "Leave Rejected";
            public const string GENERATE_PAYROLL = "Payroll Generated";
            public const string UPDATE_PAYROLL = "Payroll Updated";
            public const string EXPORT_PAYROLL = "Payroll Exported";
            public const string UPDATE_WORK_SCHEDULE = "Work Schedule Updated";
            public const string ADD_USER = "User Account Added";
            public const string UPDATE_USER = "User Account Updated";
            public const string REMOVE_USER = "User Account Removed";
        }
    }
}