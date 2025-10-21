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
                labelMessage.Text = $"Are you sure you want to restore {empName}?";
            }
        }

        // Constructor for multiple employee restoration
        public ConfirmRecoverEmployee(List<string> empIds)
        {
            InitializeComponent();
            setFont();
            this.employeeIds = empIds;
            this.isMultipleRestore = true;

            // Update UI for multiple restoration
            labelMessage.Text = $"Are you sure you want to restore {empIds.Count} employees?";
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

                // Handle attendance_records
                if (data.attendance_records != null)
                {
                    archivedEmployee.attendance_records = new Dictionary<string, dynamic>();
                    var attendanceJObject = data.attendance_records as JObject;
                    if (attendanceJObject != null)
                    {
                        foreach (var prop in attendanceJObject.Properties())
                        {
                            archivedEmployee.attendance_records[prop.Name] = prop.Value;
                        }
                    }
                }

                // Handle leave_credits
                if (data.leave_credits != null)
                {
                    archivedEmployee.leave_credits = data.leave_credits;
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
                // Restore employee data - convert JObject to Dictionary
                if (archivedEmployee.employee_data != null)
                {
                    var employeeDataDict = archivedEmployee.employee_data.ToObject<Dictionary<string, object>>();
                    await firebase
                        .Child("EmployeeDetails")
                        .Child(empId)
                        .PutAsync(employeeDataDict);
                }

                // Restore leave credits - convert JObject to Dictionary
                if (archivedEmployee.leave_credits != null)
                {
                    var leaveCreditsDict = archivedEmployee.leave_credits.ToObject<Dictionary<string, object>>();
                    await firebase
                        .Child("Leave Credits")
                        .Child(empId)
                        .PutAsync(leaveCreditsDict);
                }

                // Restore attendance records
                if (archivedEmployee.attendance_records != null && archivedEmployee.attendance_records.Any())
                {
                    foreach (var attendance in archivedEmployee.attendance_records)
                    {
                        var attendanceDict = attendance.Value as JObject;
                        if (attendanceDict != null)
                        {
                            var attendanceData = attendanceDict.ToObject<Dictionary<string, object>>();
                            await firebase
                                .Child("Attendance")
                                .Child(attendance.Key)
                                .PutAsync(attendanceData);
                        }
                    }
                }

                // Restore employment info
                await RestoreEmploymentInfo(empId, archivedEmployee);
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
                // Get current EmploymentInfo
                var employmentInfo = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<dynamic>();

                // Check if employee already exists in EmploymentInfo
                bool exists = false;
                foreach (var item in employmentInfo)
                {
                    if (item.Object != null)
                    {
                        var employeeIdProperty = item.Object.employee_id;
                        if (employeeIdProperty != null && employeeIdProperty.ToString() == empId)
                        {
                            exists = true;
                            break;
                        }
                    }
                }

                if (!exists)
                {
                    // Find the next available index
                    int maxIndex = 0;
                    foreach (var item in employmentInfo)
                    {
                        if (int.TryParse(item.Key, out int index) && index > maxIndex)
                        {
                            maxIndex = index;
                        }
                    }

                    // Get department and position from leave_credits JObject
                    string department = "Not Assigned";
                    string position = "Not Assigned";

                    if (archivedEmployee.leave_credits != null)
                    {
                        var leaveCreditsDict = archivedEmployee.leave_credits.ToObject<Dictionary<string, object>>();
                        department = leaveCreditsDict.ContainsKey("department") ? leaveCreditsDict["department"]?.ToString() : "Not Assigned";
                        position = leaveCreditsDict.ContainsKey("position") ? leaveCreditsDict["position"]?.ToString() : "Not Assigned";
                    }

                    // Create new employment info
                    var newEmploymentInfo = new Dictionary<string, object>
            {
                { "employee_id", empId },
                { "department", department },
                { "position", position },
                { "contract_type", "" },
                { "created_at", DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt") },
                { "date_of_joining", "" },
                { "date_of_exit", "" },
                { "manager_name", "" }
            };

                    await firebase
                        .Child("EmploymentInfo")
                        .Child((maxIndex + 1).ToString())
                        .PutAsync(newEmploymentInfo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore employment info for {empId}: {ex.Message}");
                // Don't throw here as employment info restoration is secondary
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
    }
}