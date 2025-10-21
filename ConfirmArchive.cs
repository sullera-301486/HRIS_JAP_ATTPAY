using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmArchive : Form
    {
        private readonly string employeeId;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Add this property
        public bool UserConfirmed { get; private set; }

        // Add this event to notify when archiving is complete
        public event Action EmployeeArchived;

        public ConfirmArchive(string employeeId)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.StartPosition = FormStartPosition.CenterParent;
            setFont();

            // Ensure the form is fully loaded before any operations
            this.Load += ConfirmArchive_Load;
            this.Shown += ConfirmArchive_Shown;
        }

        private void ConfirmArchive_Load(object sender, EventArgs e)
        {
            // Initialization code here
        }

        private void ConfirmArchive_Shown(object sender, EventArgs e)
        {
            // Any operations that require the form to be fully shown can go here
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void setFont()
        {
            try
            {
                if (IsHandleCreated)
                {
                    if (labelMessage.InvokeRequired)
                    {
                        labelMessage.Invoke(new Action(() =>
                            labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f)));
                    }
                    else
                    {
                        labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                    }

                    // Apply fonts safely for all controls
                    SafeSetFont(labelRequestConfirm, AttributesClass.GetFont("Roboto-Regular", 16f));
                    SafeSetFont(buttonCancel, AttributesClass.GetFont("Roboto-Light", 12f));
                    SafeSetFont(buttonConfirm, AttributesClass.GetFont("Roboto-Regular", 12f));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void SafeSetFont(Control control, Font font)
        {
            if (control != null && !control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    if (IsHandleCreated)
                    {
                        control.Invoke(new Action(() => control.Font = font));
                    }
                    else
                    {
                        // If handle not created, set font directly (will be applied when handle is created)
                        control.Font = font;
                    }
                }
                else
                {
                    control.Font = font;
                }
            }
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable buttons during operation
                buttonConfirm.Enabled = false;
                buttonCancel.Enabled = false;

                await ArchiveEmployee();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error archiving employee: " + ex.Message);
            }
            finally
            {
                buttonConfirm.Enabled = true;
                buttonCancel.Enabled = true;
            }
        }

        private async Task ArchiveEmployee()
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show("Employee ID is missing.");
                    return;
                }

                if (!IsHandleCreated)
                {
                    await Task.Run(() =>
                    {
                        while (!IsHandleCreated)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    });
                }

                SafeSetButtonText(buttonConfirm, "Archiving...");

                // 1. Get all employee data
                var employeeData = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (employeeData == null)
                {
                    MessageBox.Show("Employee not found in database.");
                    return;
                }

                // Get employee name for logging
                string firstName = employeeData.first_name?.ToString() ?? "";
                string middleName = employeeData.middle_name?.ToString() ?? "";
                string lastName = employeeData.last_name?.ToString() ?? "";
                string fullName = $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();

                // 2. Get all related data based on ACTUAL JSON structure
                var employmentData = await GetEmploymentInfo();
                var leaveCredits = await GetLeaveCredits();
                var attendanceRecords = await GetAttendanceRecords();
                var employeeLoans = await GetEmployeeLoans();
                var workSchedule = await GetWorkSchedule();
                var userData = await GetUserData();
                var hrNotifications = await GetHRNotifications();
                var payrollData = await GetPayrollData();
                var payrollEarnings = await GetPayrollEarnings();
                var payrollSummary = await GetPayrollSummary();
                var employeeDeductions = await GetEmployeeDeductions();
                var governmentDeductions = await GetGovernmentDeductions();
                var manageLeaveRecords = await GetManageLeaveRecords();
                var todos = await GetTodos();

                // 3. Create comprehensive archived employee record
                var archivedEmployee = new
                {
                    employee_data = employeeData,
                    employment_info = employmentData,
                    leave_credits = leaveCredits,
                    attendance_records = attendanceRecords,
                    employee_loans = employeeLoans,
                    work_schedule = workSchedule,
                    user_data = userData,
                    hr_notifications = hrNotifications,
                    payroll_data = payrollData,
                    payroll_earnings = payrollEarnings,
                    payroll_summary = payrollSummary,
                    employee_deductions = employeeDeductions,
                    government_deductions = governmentDeductions,
                    manage_leave_records = manageLeaveRecords,
                    todos = todos,
                    archived_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    archived_by = SessionClass.CurrentEmployeeName,
                    is_archived = true
                };

                // 4. Save to ArchivedEmployees
                await firebase
                    .Child("ArchivedEmployees")
                    .Child(employeeId)
                    .PutAsync(archivedEmployee);

                // 5. Remove from all active tables (logs remain untouched)
                await RemoveFromAllActiveTables();

                // 6. Log the archive action
                await LogArchiveAction(fullName);

                // Set UserConfirmed to true on success
                this.UserConfirmed = true;
                this.DialogResult = DialogResult.OK;

                // Trigger the event to notify that employee was archived
                EmployeeArchived?.Invoke();

                MessageBox.Show($"Employee {fullName} has been successfully archived.", "Archive Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error archiving employee: {ex.Message}", "Archive Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.UserConfirmed = false;
            }
        }

        // CORRECTED METHODS BASED ON ACTUAL JSON STRUCTURE
        private async Task<dynamic> GetEmploymentInfo()
        {
            try
            {
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<List<dynamic>>();

                if (employmentData != null)
                {
                    foreach (var item in employmentData)
                    {
                        if (item != null)
                        {
                            var empId = item.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                return item;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employment info: {ex.Message}");
                return null;
            }
        }

        private async Task<dynamic> GetLeaveCredits()
        {
            try
            {
                var leaveCredits = await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                return leaveCredits;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting leave credits: {ex.Message}");
                return null;
            }
        }

        private async Task<Dictionary<string, dynamic>> GetAttendanceRecords()
        {
            try
            {
                var allAttendance = await firebase
                    .Child("Attendance")
                    .OnceAsync<dynamic>();

                var employeeAttendance = new Dictionary<string, dynamic>();

                if (allAttendance != null)
                {
                    foreach (var attendance in allAttendance)
                    {
                        if (attendance.Object != null)
                        {
                            var empId = attendance.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employeeAttendance[attendance.Key] = attendance.Object;
                            }
                        }
                    }
                }
                return employeeAttendance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting attendance records: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        private async Task<List<dynamic>> GetEmployeeLoans()
        {
            try
            {
                var allLoans = await firebase
                    .Child("EmployeeLoans")
                    .OnceSingleAsync<List<dynamic>>();

                var employeeLoans = new List<dynamic>();

                if (allLoans != null)
                {
                    foreach (var loan in allLoans)
                    {
                        if (loan != null)
                        {
                            var empId = loan.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employeeLoans.Add(loan);
                            }
                        }
                    }
                }
                return employeeLoans;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee loans: {ex.Message}");
                return new List<dynamic>();
            }
        }

        private async Task<List<dynamic>> GetWorkSchedule()
        {
            try
            {
                var allSchedules = await firebase
                    .Child("Work_Schedule")
                    .OnceSingleAsync<List<dynamic>>();

                var employeeSchedules = new List<dynamic>();

                if (allSchedules != null)
                {
                    foreach (var schedule in allSchedules)
                    {
                        if (schedule != null)
                        {
                            var empId = schedule.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employeeSchedules.Add(schedule);
                            }
                        }
                    }
                }
                return employeeSchedules;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting work schedule: {ex.Message}");
                return new List<dynamic>();
            }
        }

        private async Task<dynamic> GetUserData()
        {
            try
            {
                var allUsers = await firebase
                    .Child("Users")
                    .OnceAsync<dynamic>();

                if (allUsers != null)
                {
                    foreach (var user in allUsers)
                    {
                        if (user.Object != null)
                        {
                            var empId = user.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                return user.Object;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user data: {ex.Message}");
                return null;
            }
        }

        private async Task<Dictionary<string, dynamic>> GetHRNotifications()
        {
            try
            {
                var allNotifications = await firebase
                    .Child("HRNotifications")
                    .OnceAsync<dynamic>();

                var employeeNotifications = new Dictionary<string, dynamic>();

                if (allNotifications != null)
                {
                    foreach (var notification in allNotifications)
                    {
                        if (notification.Object != null)
                        {
                            var message = notification.Object.message?.ToString();
                            if (!string.IsNullOrEmpty(message) && message.Contains(employeeId))
                            {
                                employeeNotifications[notification.Key] = notification.Object;
                            }
                        }
                    }
                }
                return employeeNotifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting HR notifications: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        // NEW METHODS FOR ADDITIONAL DATA
        private async Task<dynamic> GetPayrollData()
        {
            try
            {
                var allPayroll = await firebase
                    .Child("Payroll")
                    .OnceSingleAsync<List<dynamic>>();

                if (allPayroll != null)
                {
                    foreach (var payroll in allPayroll)
                    {
                        if (payroll != null)
                        {
                            var empId = payroll.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                return payroll;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll data: {ex.Message}");
                return null;
            }
        }

        private async Task<dynamic> GetPayrollEarnings()
        {
            try
            {
                var allEarnings = await firebase
                    .Child("PayrollEarnings")
                    .OnceSingleAsync<List<dynamic>>();

                if (allEarnings != null)
                {
                    var payrollData = await GetPayrollData();
                    if (payrollData != null)
                    {
                        var payrollId = payrollData.payroll_id?.ToString();
                        foreach (var earning in allEarnings)
                        {
                            if (earning != null && earning.payroll_id?.ToString() == payrollId)
                            {
                                return earning;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll earnings: {ex.Message}");
                return null;
            }
        }

        private async Task<dynamic> GetPayrollSummary()
        {
            try
            {
                var allSummaries = await firebase
                    .Child("PayrollSummary")
                    .OnceSingleAsync<List<dynamic>>();

                if (allSummaries != null)
                {
                    var payrollData = await GetPayrollData();
                    if (payrollData != null)
                    {
                        var payrollId = payrollData.payroll_id?.ToString();
                        foreach (var summary in allSummaries)
                        {
                            if (summary != null && summary.payroll_id?.ToString() == payrollId)
                            {
                                return summary;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll summary: {ex.Message}");
                return null;
            }
        }

        private async Task<dynamic> GetEmployeeDeductions()
        {
            try
            {
                var allDeductions = await firebase
                    .Child("EmployeeDeductions")
                    .OnceSingleAsync<List<dynamic>>();

                if (allDeductions != null)
                {
                    foreach (var deduction in allDeductions)
                    {
                        if (deduction != null)
                        {
                            var empId = deduction.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                return deduction;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee deductions: {ex.Message}");
                return null;
            }
        }

        private async Task<dynamic> GetGovernmentDeductions()
        {
            try
            {
                var allGovDeductions = await firebase
                    .Child("GovernmentDeductions")
                    .OnceSingleAsync<List<dynamic>>();

                if (allGovDeductions != null)
                {
                    var payrollData = await GetPayrollData();
                    if (payrollData != null)
                    {
                        var payrollId = payrollData.payroll_id?.ToString();
                        foreach (var deduction in allGovDeductions)
                        {
                            if (deduction != null && deduction.payroll_id?.ToString() == payrollId)
                            {
                                return deduction;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting government deductions: {ex.Message}");
                return null;
            }
        }

        private async Task<Dictionary<string, dynamic>> GetManageLeaveRecords()
        {
            try
            {
                var allLeaveRequests = await firebase
                    .Child("ManageLeave")
                    .OnceAsync<dynamic>();

                var employeeLeaveRequests = new Dictionary<string, dynamic>();

                if (allLeaveRequests != null)
                {
                    foreach (var leaveRequest in allLeaveRequests)
                    {
                        if (leaveRequest.Object != null)
                        {
                            var employeeName = leaveRequest.Object.employee?.ToString();
                            if (!string.IsNullOrEmpty(employeeName) &&
                                await IsEmployeeNameMatch(employeeName, employeeId))
                            {
                                employeeLeaveRequests[leaveRequest.Key] = leaveRequest.Object;
                            }
                        }
                    }
                }
                return employeeLeaveRequests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting manage leave records: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        private async Task<Dictionary<string, dynamic>> GetTodos()
        {
            try
            {
                var allTodos = await firebase
                    .Child("Todos")
                    .OnceAsync<dynamic>();

                var employeeTodos = new Dictionary<string, dynamic>();

                if (allTodos != null)
                {
                    var userData = await GetUserData();
                    if (userData != null)
                    {
                        var userId = userData.user_id?.ToString();
                        foreach (var todo in allTodos)
                        {
                            if (todo.Object != null)
                            {
                                var assignedTo = todo.Object.assignedTo?.ToString();
                                var createdBy = todo.Object.createdBy?.ToString();
                                if (assignedTo == userId || createdBy == userId)
                                {
                                    employeeTodos[todo.Key] = todo.Object;
                                }
                            }
                        }
                    }
                }
                return employeeTodos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting todos: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        private async Task<bool> IsEmployeeNameMatch(string leaveRequestName, string employeeId)
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
                    string lastName = employee.last_name?.ToString() ?? "";
                    string fullName = $"{firstName} {lastName}".Trim();

                    return leaveRequestName.Contains(firstName) ||
                           leaveRequestName.Contains(lastName) ||
                           leaveRequestName.Contains(fullName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error matching employee name: {ex.Message}");
            }
            return false;
        }

        private async Task RemoveFromAllActiveTables()
        {
            try
            {
                // Remove from all tables based on actual JSON structure
                // Logs (ActivityLogs, AdminLogs, PayrollLogs) remain untouched and active
                await RemoveFromEmployeeDetails();
                await RemoveFromEmploymentInfo();
                await RemoveFromWorkSchedule();
                await RemoveUserAccess();
                await RemoveLeaveCredits();
                await RemoveAttendanceRecords();
                await RemoveEmployeeLoans();
                await RemoveHRNotifications();
                await RemovePayrollData();
                await RemovePayrollEarnings();
                await RemovePayrollSummary();
                await RemoveEmployeeDeductions();
                await RemoveGovernmentDeductions();
                await RemoveManageLeaveRecords();
                await RemoveTodos();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from active tables: {ex.Message}");
            }
        }

        private async Task RemoveFromEmployeeDetails()
        {
            try
            {
                await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from employee details: {ex.Message}");
            }
        }

        private async Task RemoveFromEmploymentInfo()
        {
            try
            {
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<List<dynamic>>();

                if (employmentData != null)
                {
                    var newEmploymentList = new List<dynamic>();
                    foreach (var item in employmentData)
                    {
                        if (item != null)
                        {
                            var empId = item.employee_id?.ToString();
                            if (empId != employeeId)
                            {
                                newEmploymentList.Add(item);
                            }
                        }
                    }
                    await firebase
                        .Child("EmploymentInfo")
                        .PutAsync(newEmploymentList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from employment info: {ex.Message}");
            }
        }

        private async Task RemoveFromWorkSchedule()
        {
            try
            {
                var scheduleData = await firebase
                    .Child("Work_Schedule")
                    .OnceSingleAsync<List<dynamic>>();

                if (scheduleData != null)
                {
                    var newScheduleList = new List<dynamic>();
                    foreach (var item in scheduleData)
                    {
                        if (item != null)
                        {
                            var empId = item.employee_id?.ToString();
                            if (empId != employeeId)
                            {
                                newScheduleList.Add(item);
                            }
                        }
                    }
                    await firebase
                        .Child("Work_Schedule")
                        .PutAsync(newScheduleList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from work schedule: {ex.Message}");
            }
        }

        private async Task RemoveUserAccess()
        {
            try
            {
                var usersData = await firebase
                    .Child("Users")
                    .OnceAsync<dynamic>();

                if (usersData != null)
                {
                    string userKeyToRemove = null;
                    foreach (var user in usersData)
                    {
                        if (user.Object != null)
                        {
                            var empId = user.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                userKeyToRemove = user.Key;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(userKeyToRemove))
                    {
                        await firebase
                            .Child("Users")
                            .Child(userKeyToRemove)
                            .DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing user access: {ex.Message}");
            }
        }

        private async Task RemoveLeaveCredits()
        {
            try
            {
                await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing leave credits: {ex.Message}");
            }
        }

        private async Task RemoveAttendanceRecords()
        {
            try
            {
                var allAttendance = await firebase
                    .Child("Attendance")
                    .OnceAsync<dynamic>();

                if (allAttendance != null)
                {
                    foreach (var attendance in allAttendance)
                    {
                        if (attendance.Object != null)
                        {
                            var empId = attendance.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                await firebase
                                    .Child("Attendance")
                                    .Child(attendance.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing attendance records: {ex.Message}");
            }
        }

        private async Task RemoveEmployeeLoans()
        {
            try
            {
                var allLoans = await firebase
                    .Child("EmployeeLoans")
                    .OnceSingleAsync<List<dynamic>>();

                if (allLoans != null)
                {
                    var newLoansList = new List<dynamic>();
                    foreach (var loan in allLoans)
                    {
                        if (loan != null)
                        {
                            var empId = loan.employee_id?.ToString();
                            if (empId != employeeId)
                            {
                                newLoansList.Add(loan);
                            }
                        }
                    }
                    await firebase
                        .Child("EmployeeLoans")
                        .PutAsync(newLoansList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing employee loans: {ex.Message}");
            }
        }

        private async Task RemoveHRNotifications()
        {
            try
            {
                var allNotifications = await firebase
                    .Child("HRNotifications")
                    .OnceAsync<dynamic>();

                if (allNotifications != null)
                {
                    foreach (var notification in allNotifications)
                    {
                        if (notification.Object != null)
                        {
                            var message = notification.Object.message?.ToString();
                            if (!string.IsNullOrEmpty(message) && message.Contains(employeeId))
                            {
                                await firebase
                                    .Child("HRNotifications")
                                    .Child(notification.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing HR notifications: {ex.Message}");
            }
        }

        // NEW REMOVAL METHODS
        private async Task RemovePayrollData()
        {
            try
            {
                var allPayroll = await firebase
                    .Child("Payroll")
                    .OnceSingleAsync<List<dynamic>>();

                if (allPayroll != null)
                {
                    var newPayrollList = new List<dynamic>();
                    foreach (var payroll in allPayroll)
                    {
                        if (payroll != null)
                        {
                            var empId = payroll.employee_id?.ToString();
                            if (empId != employeeId)
                            {
                                newPayrollList.Add(payroll);
                            }
                        }
                    }
                    await firebase
                        .Child("Payroll")
                        .PutAsync(newPayrollList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing payroll data: {ex.Message}");
            }
        }

        private async Task RemovePayrollEarnings()
        {
            try
            {
                var allEarnings = await firebase
                    .Child("PayrollEarnings")
                    .OnceSingleAsync<List<dynamic>>();

                if (allEarnings != null)
                {
                    var payrollData = await GetPayrollData();
                    if (payrollData != null)
                    {
                        var payrollId = payrollData.payroll_id?.ToString();
                        var newEarningsList = new List<dynamic>();
                        foreach (var earning in allEarnings)
                        {
                            if (earning != null && earning.payroll_id?.ToString() != payrollId)
                            {
                                newEarningsList.Add(earning);
                            }
                        }
                        await firebase
                            .Child("PayrollEarnings")
                            .PutAsync(newEarningsList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing payroll earnings: {ex.Message}");
            }
        }

        private async Task RemovePayrollSummary()
        {
            try
            {
                var allSummaries = await firebase
                    .Child("PayrollSummary")
                    .OnceSingleAsync<List<dynamic>>();

                if (allSummaries != null)
                {
                    var payrollData = await GetPayrollData();
                    if (payrollData != null)
                    {
                        var payrollId = payrollData.payroll_id?.ToString();
                        var newSummaryList = new List<dynamic>();
                        foreach (var summary in allSummaries)
                        {
                            if (summary != null && summary.payroll_id?.ToString() != payrollId)
                            {
                                newSummaryList.Add(summary);
                            }
                        }
                        await firebase
                            .Child("PayrollSummary")
                            .PutAsync(newSummaryList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing payroll summary: {ex.Message}");
            }
        }

        private async Task RemoveEmployeeDeductions()
        {
            try
            {
                var allDeductions = await firebase
                    .Child("EmployeeDeductions")
                    .OnceSingleAsync<List<dynamic>>();

                if (allDeductions != null)
                {
                    var newDeductionsList = new List<dynamic>();
                    foreach (var deduction in allDeductions)
                    {
                        if (deduction != null)
                        {
                            var empId = deduction.employee_id?.ToString();
                            if (empId != employeeId)
                            {
                                newDeductionsList.Add(deduction);
                            }
                        }
                    }
                    await firebase
                        .Child("EmployeeDeductions")
                        .PutAsync(newDeductionsList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing employee deductions: {ex.Message}");
            }
        }

        private async Task RemoveGovernmentDeductions()
        {
            try
            {
                var allGovDeductions = await firebase
                    .Child("GovernmentDeductions")
                    .OnceSingleAsync<List<dynamic>>();

                if (allGovDeductions != null)
                {
                    var payrollData = await GetPayrollData();
                    if (payrollData != null)
                    {
                        var payrollId = payrollData.payroll_id?.ToString();
                        var newGovDeductionsList = new List<dynamic>();
                        foreach (var deduction in allGovDeductions)
                        {
                            if (deduction != null && deduction.payroll_id?.ToString() != payrollId)
                            {
                                newGovDeductionsList.Add(deduction);
                            }
                        }
                        await firebase
                            .Child("GovernmentDeductions")
                            .PutAsync(newGovDeductionsList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing government deductions: {ex.Message}");
            }
        }

        private async Task RemoveManageLeaveRecords()
        {
            try
            {
                var allLeaveRequests = await firebase
                    .Child("ManageLeave")
                    .OnceAsync<dynamic>();

                if (allLeaveRequests != null)
                {
                    foreach (var leaveRequest in allLeaveRequests)
                    {
                        if (leaveRequest.Object != null)
                        {
                            var employeeName = leaveRequest.Object.employee?.ToString();
                            if (!string.IsNullOrEmpty(employeeName) &&
                                await IsEmployeeNameMatch(employeeName, employeeId))
                            {
                                await firebase
                                    .Child("ManageLeave")
                                    .Child(leaveRequest.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing manage leave records: {ex.Message}");
            }
        }

        private async Task RemoveTodos()
        {
            try
            {
                var allTodos = await firebase
                    .Child("Todos")
                    .OnceAsync<dynamic>();

                if (allTodos != null)
                {
                    var userData = await GetUserData();
                    if (userData != null)
                    {
                        var userId = userData.user_id?.ToString();
                        foreach (var todo in allTodos)
                        {
                            if (todo.Object != null)
                            {
                                var assignedTo = todo.Object.assignedTo?.ToString();
                                var createdBy = todo.Object.createdBy?.ToString();
                                if (assignedTo == userId || createdBy == userId)
                                {
                                    await firebase
                                        .Child("Todos")
                                        .Child(todo.Key)
                                        .DeleteAsync();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing todos: {ex.Message}");
            }
        }

        private void SafeSetButtonText(Button button, string text)
        {
            if (button != null && !button.IsDisposed)
            {
                if (button.InvokeRequired)
                {
                    if (IsHandleCreated)
                    {
                        button.Invoke(new Action(() => button.Text = text));
                    }
                    else
                    {
                        button.Text = text;
                    }
                }
                else
                {
                    button.Text = text;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        private async Task LogArchiveAction(string employeeName)
        {
            try
            {
                string description = $"Archived employee: {employeeName}";
                await AdminLogService.LogAdminAction(
                    AdminLogService.Actions.ARCHIVE_EMPLOYEE,
                    description,
                    employeeId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging archive action: {ex.Message}");
            }
        }
    }
}