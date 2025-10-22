using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
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
    public partial class ConfirmRecoverEmployee : Form
    {
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string employeeId;
        private string employeeName;
        private List<string> employeeIds;
        private bool isMultipleRestore = false;

        public event Action<string> EmployeeRestored;

        // Constructor for single employee restoration
        public ConfirmRecoverEmployee(string empId, string empName)
        {
            InitializeComponent();
            setFont();
            this.employeeId = empId;
            this.employeeName = empName;
            this.isMultipleRestore = false;

            // Update UI for single restoration
            if (!string.IsNullOrEmpty(empName))
            {
                
            }
        }

        // Constructor for multiple employee restoration
        public ConfirmRecoverEmployee(List<string> empIds)
        {
            InitializeComponent();
            setFont();
            this.employeeIds = empIds;
            this.isMultipleRestore = true;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonConfirm_Click_1(object sender, EventArgs e)
        {
            try
            {
                buttonConfirm.Enabled = false;
                buttonConfirm.Text = "Restoring...";

                if (isMultipleRestore)
                {
                    await RestoreMultipleEmployees();
                }
                else
                {
                    await RestoreSingleEmployee(employeeId);
                }

                MessageBox.Show("Employee(s) restored successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                EmployeeRestored?.Invoke(employeeId);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring employee: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                buttonConfirm.Enabled = true;
                buttonConfirm.Text = "Confirm";
            }
        }

        private async Task RestoreSingleEmployee(string empId)
        {
            try
            {
                // Get archived data
                var archivedData = await firebase
                    .Child("ArchivedEmployees")
                    .Child(empId)
                    .OnceSingleAsync<dynamic>();

                if (archivedData == null)
                {
                    throw new Exception($"Archived employee {empId} not found.");
                }

                // Convert to ArchivedEmployee class
                var archivedEmployee = ConvertToArchivedEmployee(archivedData, empId);

                // Restore employee data
                await RestoreEmployeeData(empId, archivedEmployee);

                // Delete from archived
                await firebase
                    .Child("ArchivedEmployees")
                    .Child(empId)
                    .DeleteAsync();

                // Log the restoration
                await LogRestoration(empId, archivedEmployee);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restore employee {empId}: {ex.Message}", ex);
            }
        }

        private ArchivedEmployee ConvertToArchivedEmployee(dynamic data, string employeeId)
        {
            try
            {
                var archivedEmployee = new ArchivedEmployee
                {
                    archived_by = data.archived_by ?? "System",
                    archived_date = data.archived_date ?? DateTime.Now.ToString(),
                    is_archived = true
                };

                // Handle employee_data
                if (data.employee_data != null)
                {
                    archivedEmployee.employee_data = data.employee_data;
                }

                // Handle employment_info
                if (data.employment_info != null)
                {
                    archivedEmployee.employment_info = data.employment_info;
                }

                // Handle leave_credits
                if (data.leave_credits != null)
                {
                    archivedEmployee.leave_credits = data.leave_credits;
                }

                // Handle attendance_records
                if (data.attendance_records != null)
                {
                    archivedEmployee.attendance_records = new Dictionary<string, JObject>();
                    var attendanceJObject = data.attendance_records as JObject;
                    if (attendanceJObject != null)
                    {
                        foreach (var prop in attendanceJObject.Properties())
                        {
                            archivedEmployee.attendance_records[prop.Name] = prop.Value as JObject;
                        }
                    }
                }

                // Handle work_schedule
                if (data.work_schedule != null)
                {
                    archivedEmployee.work_schedule = data.work_schedule as JArray;
                }

                // Handle user_data
                if (data.user_data != null)
                {
                    archivedEmployee.user_data = data.user_data;
                }

                // Handle manage_leave_records
                if (data.manage_leave_records != null)
                {
                    archivedEmployee.manage_leave_records = new Dictionary<string, JObject>();
                    var leaveJObject = data.manage_leave_records as JObject;
                    if (leaveJObject != null)
                    {
                        foreach (var prop in leaveJObject.Properties())
                        {
                            archivedEmployee.manage_leave_records[prop.Name] = prop.Value as JObject;
                        }
                    }
                }

                return archivedEmployee;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert archived data for {employeeId}: {ex.Message}", ex);
            }
        }
        private async Task RestoreEmployeeData(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                // 1. Restore EmployeeDetails
                if (archivedEmployee.employee_data != null)
                {
                    var employeeDataDict = archivedEmployee.employee_data.ToObject<Dictionary<string, object>>();
                    await firebase
                        .Child("EmployeeDetails")
                        .Child(empId)
                        .PutAsync(employeeDataDict);
                }

                // 2. Restore Leave Credits
                if (archivedEmployee.leave_credits != null)
                {
                    var leaveCreditsDict = archivedEmployee.leave_credits.ToObject<Dictionary<string, object>>();
                    await firebase
                        .Child("Leave Credits")
                        .Child(empId)
                        .PutAsync(leaveCreditsDict);
                }

                // 3. Restore Attendance Records
                if (archivedEmployee.attendance_records != null && archivedEmployee.attendance_records.Any())
                {
                    foreach (var attendance in archivedEmployee.attendance_records)
                    {
                        var attendanceDict = attendance.Value.ToObject<Dictionary<string, object>>();
                        await firebase
                            .Child("Attendance")
                            .Child(attendance.Key)
                            .PutAsync(attendance);
                    }
                }

                // 4. Restore Employment Info
                await RestoreEmploymentInfo(empId, archivedEmployee);

                // 5. Restore Work Schedule (if exists in archive)
                await RestoreWorkSchedule(empId, archivedEmployee);

                // 6. Restore User Account (if exists in archive)
                await RestoreUserAccount(empId, archivedEmployee);

                // 7. Restore Leave Records (if exists in archive)
                await RestoreManageLeaveRecords(empId, archivedEmployee);

                // 8. Restore Loan Data (if exists in archive)
                await RestoreEmployeeLoans(empId, archivedEmployee);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restore employee data: {ex.Message}", ex);
            }
        }

        private async Task RestoreMultipleEmployees()
        {
            var successfulRestorations = new List<string>();
            var failedRestorations = new List<string>();

            foreach (string empId in employeeIds)
            {
                try
                {
                    await RestoreSingleEmployee(empId);
                    successfulRestorations.Add(empId);
                }
                catch (Exception ex)
                {
                    failedRestorations.Add($"{empId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Failed to restore {empId}: {ex.Message}");
                }
            }

            if (failedRestorations.Any())
            {
                throw new Exception($"Failed to restore some employees:\n{string.Join("\n", failedRestorations)}");
            }
        }

        private async Task RestoreEmploymentInfo(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                // Get current EmploymentInfo list
                var employmentInfo = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<dynamic>();

                // Check if employee already exists in EmploymentInfo
                bool exists = false;
                string existingKey = null;

                foreach (var item in employmentInfo)
                {
                    if (item.Object != null)
                    {
                        var employeeIdProperty = item.Object.employee_id;
                        if (employeeIdProperty != null && employeeIdProperty.ToString() == empId)
                        {
                            exists = true;
                            existingKey = item.Key;
                            break;
                        }
                    }
                }

                if (exists && !string.IsNullOrEmpty(existingKey))
                {
                    // Update existing employment info
                    var employmentData = await GetEmploymentInfoFromArchive(archivedEmployee);
                    if (employmentData != null)
                    {
                        await firebase
                            .Child("EmploymentInfo")
                            .Child(existingKey)
                            .PutAsync(employmentData);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore employment info for {empId}: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, object>> GetEmploymentInfoFromArchive(ArchivedEmployee archivedEmployee)
        {
            try
            {
                if (archivedEmployee.employment_info != null)
                {
                    return archivedEmployee.employment_info.ToObject<Dictionary<string, object>>();
                }

                // Fallback: Create from available data
                string department = "Not Assigned";
                string position = "Not Assigned";

                if (archivedEmployee.leave_credits != null)
                {
                    var leaveCreditsDict = archivedEmployee.leave_credits.ToObject<Dictionary<string, object>>();
                    department = leaveCreditsDict.ContainsKey("department") ?
                        leaveCreditsDict["department"]?.ToString() : "Not Assigned";
                    position = leaveCreditsDict.ContainsKey("position") ?
                        leaveCreditsDict["position"]?.ToString() : "Not Assigned";
                }

                return new Dictionary<string, object>
        {
            { "employee_id", archivedEmployee.employee_data?["employee_id"]?.ToString() ?? "" },
            { "department", department },
            { "position", position },
            { "contract_type", archivedEmployee.employment_info?["contract_type"]?.ToString() ?? "" },
            { "created_at", DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt") },
            { "date_of_joining", archivedEmployee.employment_info?["date_of_joining"]?.ToString() ?? "" },
            { "date_of_exit", archivedEmployee.employment_info?["date_of_exit"]?.ToString() ?? "" },
            { "manager_name", archivedEmployee.employment_info?["manager_name"]?.ToString() ?? "" }
        };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting employment info from archive: {ex.Message}");
                return null;
            }
        }
        private async Task RestoreWorkSchedule(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                if (archivedEmployee.work_schedule != null)
                {
                    var workSchedules = archivedEmployee.work_schedule as JArray;
                    if (workSchedules != null)
                    {
                        // Get current work schedules
                        var currentSchedules = await firebase
                            .Child("Work_Schedule")
                            .OnceAsync<dynamic>();

                        // Remove existing schedules for this employee
                        foreach (var schedule in currentSchedules)
                        {
                            if (schedule.Object != null && schedule.Object.employee_id?.ToString() == empId)
                            {
                                await firebase
                                    .Child("Work_Schedule")
                                    .Child(schedule.Key)
                                    .DeleteAsync();
                            }
                        }

                        // Restore archived schedules
                        foreach (var schedule in workSchedules)
                        {
                            var scheduleDict = schedule.ToObject<Dictionary<string, object>>();
                            await firebase
                                .Child("Work_Schedule")
                                .PostAsync(scheduleDict);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore work schedule for {empId}: {ex.Message}");
            }
        }
        private async Task RestoreUserAccount(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                if (archivedEmployee.user_data != null)
                {
                    var userDataDict = archivedEmployee.user_data.ToObject<Dictionary<string, object>>();
                    var userId = userDataDict["user_id"]?.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Check if user already exists
                        var existingUser = await firebase
                            .Child("Users")
                            .Child(userId)
                            .OnceSingleAsync<dynamic>();

                        if (existingUser == null)
                        {
                            // Restore user account
                            await firebase
                                .Child("Users")
                                .Child(userId)
                                .PutAsync(userDataDict);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore user account for {empId}: {ex.Message}");
            }
        }

        private async Task LogRestoration(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                // Get current user info
                string currentUserId = SessionClass.CurrentUserId ?? "System";
                string currentUserName = SessionClass.CurrentEmployeeName ?? "System";
                string currentEmployeeId = SessionClass.CurrentEmployeeId ?? "System";

                // Get employee name from archived data
                string empName = "Unknown";
                if (archivedEmployee.employee_data != null)
                {
                    string firstName = archivedEmployee.employee_data["first_name"]?.ToString() ?? "";
                    string middleName = archivedEmployee.employee_data["middle_name"]?.ToString() ?? "";
                    string lastName = archivedEmployee.employee_data["last_name"]?.ToString() ?? "";
                    empName = $"{firstName} {middleName} {lastName}".Trim();
                }

                var logData = new
                {
                    action_type = "Employee Restored",
                    admin_employee_id = currentEmployeeId,
                    admin_name = currentUserName,
                    admin_user_id = currentUserId,
                    description = $"Restored employee: {empName}",
                    details = $"Employee ID: {empId}",
                    target_employee_id = empId,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("AdminLogs")
                    .PostAsync(logData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging restoration: {ex.Message}");
            }
        }
        private async Task RestoreManageLeaveRecords(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                if (archivedEmployee.manage_leave_records != null && archivedEmployee.manage_leave_records.Any())
                {
                    foreach (var leaveRecord in archivedEmployee.manage_leave_records)
                    {
                        var leaveRecordDict = leaveRecord.Value.ToObject<Dictionary<string, object>>();
                        await firebase
                            .Child("ManageLeave")
                            .Child(leaveRecord.Key)
                            .PutAsync(leaveRecordDict);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore leave records for {empId}: {ex.Message}");
            }
        }

        private async Task RestoreEmployeeLoans(string empId, ArchivedEmployee archivedEmployee)
        {
            try
            {
                // This would need to handle loan data restoration based on your archive structure
                // Similar pattern to other restoration methods
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore loans for {empId}: {ex.Message}");
            }
        }

    }
}