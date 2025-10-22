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
    public static class PayrollLogService
    {
        private static FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        /// <summary>
        /// Logs a payroll action to Firebase AdminLogs
        /// </summary>
        public static async Task LogPayrollAction(string action, string description, string affectedEmployeeId = null, string payrollPeriod = null)
        {
            try
            {
                // Get session values
                string adminUserId = SessionClass.CurrentUserId;
                string adminName = SessionClass.CurrentEmployeeName;

                Console.WriteLine($"DEBUG: Payroll Log - UserID: {adminUserId}, Name: {adminName}");

                if (string.IsNullOrEmpty(adminUserId))
                {
                    Console.WriteLine("ERROR: No user session found for payroll logging");
                    return;
                }

                string affectedEmployeeName = "";
                if (!string.IsNullOrEmpty(affectedEmployeeId))
                {
                    affectedEmployeeName = await GetEmployeeFullName(affectedEmployeeId);
                }

                // Build detailed description
                string detailedDescription = description;
                if (!string.IsNullOrEmpty(payrollPeriod))
                {
                    detailedDescription += $" for period {payrollPeriod}";
                }
                if (!string.IsNullOrEmpty(affectedEmployeeName))
                {
                    detailedDescription += $" - {affectedEmployeeName}";
                }

                var logEntry = new
                {
                    action_type = action,
                    admin_employee_id = adminUserId,
                    admin_name = adminName,
                    admin_user_id = adminUserId,
                    description = detailedDescription,
                    details = affectedEmployeeId != null ? $"Employee ID: {affectedEmployeeId}" : "All Employees",
                    target_employee_id = affectedEmployeeId ?? "ALL",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                Console.WriteLine($"DEBUG: Saving payroll log to Firebase - {detailedDescription}");

                await firebase
                    .Child("AdminLogs")
                    .PostAsync(logEntry);

                Console.WriteLine($"SUCCESS: Payroll log created: {action} by {adminName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Payroll logging failed - {ex.Message}");
                // Don't show messagebox to avoid interrupting the flow
            }
        }

        /// <summary>
        /// Logs individual payroll export action
        /// </summary>
        public static async Task LogIndividualPayrollExport(string employeeId, string employeeName, string payrollPeriod = null)
        {
            await LogPayrollAction(
                PayrollActions.EXPORT_INDIVIDUAL,
                $"Exported individual payroll for {employeeName}",
                employeeId,
                payrollPeriod
            );
        }

        /// <summary>
        /// Logs bulk payroll export action
        /// </summary>
        public static async Task LogBulkPayrollExport(string payrollPeriod = null)
        {
            await LogPayrollAction(
                PayrollActions.EXPORT_BULK,
                "Exported payroll for all employees",
                null, // No specific employee for bulk export
                payrollPeriod
            );
        }

        /// <summary>
        /// Logs payroll generation action
        /// </summary>
        public static async Task LogPayrollGeneration(string employeeId = null, string payrollPeriod = null)
        {
            string description = employeeId != null ?
                "Generated payroll for employee" :
                "Generated payroll for all employees";

            await LogPayrollAction(
                PayrollActions.GENERATE_PAYROLL,
                description,
                employeeId,
                payrollPeriod
            );
        }

        /// <summary>
        /// Logs payroll update action
        /// </summary>
        public static async Task LogPayrollUpdate(string employeeId, string employeeName, string payrollPeriod = null)
        {
            await LogPayrollAction(
                PayrollActions.UPDATE_PAYROLL,
                $"Updated payroll data for {employeeName}",
                employeeId,
                payrollPeriod
            );
        }

        /// <summary>
        /// Gets full name of an employee from Firebase
        /// </summary>
        private static async Task<string> GetEmployeeFullName(string employeeId)
        {
            try
            {
                // Try current employees first
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

                // If not found in current employees, check archived
                var archivedEmployee = await firebase
                    .Child("ArchivedEmployees")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (archivedEmployee != null && archivedEmployee.employee_data != null)
                {
                    var empData = archivedEmployee.employee_data;
                    string firstName = empData.first_name?.ToString() ?? "";
                    string middleName = empData.middle_name?.ToString() ?? "";
                    string lastName = empData.last_name?.ToString() ?? "";

                    return $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee name for payroll log: {ex.Message}");
            }
            return "Unknown Employee";
        }

        /// <summary>
        /// Common payroll log actions for consistency
        /// </summary>
        public static class PayrollActions
        {
            public const string GENERATE_PAYROLL = "Payroll Generated";
            public const string EXPORT_INDIVIDUAL = "Payroll Exported";
            public const string EXPORT_BULK = "Payroll Exported";
            public const string UPDATE_PAYROLL = "Payroll Updated";
            public const string CALCULATE_PAYROLL = "Payroll Calculated";
            public const string PROCESS_PAYROLL = "Payroll Processed";
        }
    }
}