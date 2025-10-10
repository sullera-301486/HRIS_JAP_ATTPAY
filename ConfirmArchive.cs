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

                // 2. Get all related data
                var employmentData = await GetEmploymentInfo();
                var leaveCredits = await GetLeaveCredits();
                var leaveNotifications = await GetLeaveNotifications();
                var attendanceRecords = await GetAttendanceRecords();
                var employeeDeductions = await GetEmployeeDeductions();
                var governmentDeductions = await GetGovernmentDeductions();
                var employeeLoans = await GetEmployeeLoans();
                var payrollRecords = await GetPayrollRecords();
                var payrollEarnings = await GetPayrollEarnings();
                var payrollSummaries = await GetPayrollSummaries();
                var todos = await GetTodos();

                // 3. Create comprehensive archived employee record
                var archivedEmployee = new
                {
                    employee_data = employeeData,
                    employment_info = employmentData,
                    leave_credits = leaveCredits,
                    leave_notifications = leaveNotifications,
                    attendance_records = attendanceRecords,
                    employee_deductions = employeeDeductions,
                    government_deductions = governmentDeductions,
                    employee_loans = employeeLoans,
                    payroll_records = payrollRecords,
                    payroll_earnings = payrollEarnings,
                    payroll_summaries = payrollSummaries,
                    todos = todos,
                    archived_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    archived_by = SessionClass.CurrentEmployeeName, // Changed from "System"
                    is_archived = true
                };

                // 4. Save to ArchivedEmployees
                await firebase
                    .Child("ArchivedEmployees")
                    .Child(employeeId)
                    .PutAsync(archivedEmployee);

                // 5. Remove from all active tables
                await RemoveFromAllActiveTables();

                // 6. Log the archive action
                await LogArchiveAction(fullName);

                // Set UserConfirmed to true on success
                this.UserConfirmed = true;
                this.DialogResult = DialogResult.OK;

                // Trigger the event to notify that employee was archived
                EmployeeArchived?.Invoke();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error archiving employee: " + ex.Message);
                this.UserConfirmed = false;
            }
        }

        // Existing methods (GetEmploymentInfo, GetLeaveCredits, GetLeaveNotifications)
        private async Task<dynamic> GetEmploymentInfo()
        {
            try
            {
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<Dictionary<string, dynamic>>();

                if (employmentData != null)
                {
                    foreach (var kvp in employmentData)
                    {
                        if (kvp.Value != null)
                        {
                            var empId = kvp.Value.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                return kvp.Value;
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

        private async Task<Dictionary<string, dynamic>> GetLeaveNotifications()
        {
            try
            {
                var allNotifications = await firebase
                    .Child("LeaveNotifications")
                    .OnceAsync<dynamic>();

                var employeeNotifications = new Dictionary<string, dynamic>();

                if (allNotifications != null)
                {
                    foreach (var notification in allNotifications)
                    {
                        if (notification.Object != null)
                        {
                            var notificationEmployee = notification.Object.Employee?.ToString();
                            if (!string.IsNullOrEmpty(notificationEmployee) &&
                                notificationEmployee.Contains(employeeId))
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
                Console.WriteLine($"Error getting leave notifications: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        // NEW METHODS TO GET ADDITIONAL DATA
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

        private async Task<Dictionary<string, dynamic>> GetEmployeeDeductions()
        {
            try
            {
                var allDeductions = await firebase
                    .Child("EmployeeDeductions")
                    .OnceAsync<dynamic>();

                var employeeDeductions = new Dictionary<string, dynamic>();

                if (allDeductions != null)
                {
                    foreach (var deduction in allDeductions)
                    {
                        if (deduction.Object != null)
                        {
                            var empId = deduction.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employeeDeductions[deduction.Key] = deduction.Object;
                            }
                        }
                    }
                }
                return employeeDeductions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee deductions: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        private async Task<Dictionary<string, dynamic>> GetGovernmentDeductions()
        {
            try
            {
                var allGovDeductions = await firebase
                    .Child("GovernmentDeductions")
                    .OnceAsync<dynamic>();

                var employeeGovDeductions = new Dictionary<string, dynamic>();

                if (allGovDeductions != null)
                {
                    foreach (var deduction in allGovDeductions)
                    {
                        if (deduction.Object != null)
                        {
                            // Government deductions are linked via payroll_id, so we need to find matching payroll records
                            var payrollId = deduction.Key.ToString();
                            if (await IsPayrollForEmployee(payrollId))
                            {
                                employeeGovDeductions[deduction.Key] = deduction.Object;
                            }
                        }
                    }
                }
                return employeeGovDeductions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting government deductions: {ex.Message}");
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

        private async Task<Dictionary<string, dynamic>> GetPayrollRecords()
        {
            try
            {
                var allPayroll = await firebase
                    .Child("Payroll")
                    .OnceAsync<dynamic>();

                var employeePayroll = new Dictionary<string, dynamic>();

                if (allPayroll != null)
                {
                    foreach (var payroll in allPayroll)
                    {
                        if (payroll.Object != null)
                        {
                            var empId = payroll.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employeePayroll[payroll.Key] = payroll.Object;
                            }
                        }
                    }
                }
                return employeePayroll;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll records: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        private async Task<Dictionary<string, dynamic>> GetPayrollEarnings()
        {
            try
            {
                var allEarnings = await firebase
                    .Child("PayrollEarnings")
                    .OnceAsync<dynamic>();

                var employeeEarnings = new Dictionary<string, dynamic>();

                if (allEarnings != null)
                {
                    foreach (var earning in allEarnings)
                    {
                        if (earning.Object != null)
                        {
                            var payrollId = earning.Object.payroll_id?.ToString();
                            if (await IsPayrollForEmployee(payrollId))
                            {
                                employeeEarnings[earning.Key] = earning.Object;
                            }
                        }
                    }
                }
                return employeeEarnings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll earnings: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }

        private async Task<Dictionary<string, dynamic>> GetPayrollSummaries()
        {
            try
            {
                var allSummaries = await firebase
                    .Child("PayrollSummary")
                    .OnceAsync<dynamic>();

                var employeeSummaries = new Dictionary<string, dynamic>();

                if (allSummaries != null)
                {
                    foreach (var summary in allSummaries)
                    {
                        if (summary.Object != null)
                        {
                            var payrollId = summary.Object.payroll_id?.ToString();
                            if (await IsPayrollForEmployee(payrollId))
                            {
                                employeeSummaries[summary.Key] = summary.Object;
                            }
                        }
                    }
                }
                return employeeSummaries;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll summaries: {ex.Message}");
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
                    foreach (var todo in allTodos)
                    {
                        if (todo.Object != null)
                        {
                            var assignedTo = todo.Object.assignedTo?.ToString();
                            if (assignedTo != null && await IsUserForEmployee(assignedTo))
                            {
                                employeeTodos[todo.Key] = todo.Object;
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

        private async Task<bool> IsPayrollForEmployee(string payrollId)
        {
            if (string.IsNullOrEmpty(payrollId)) return false;

            try
            {
                var payroll = await firebase
                    .Child("Payroll")
                    .Child(payrollId)
                    .OnceSingleAsync<dynamic>();

                return payroll?.employee_id?.ToString() == employeeId;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsUserForEmployee(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            try
            {
                var user = await firebase
                    .Child("Users")
                    .Child(userId)
                    .OnceSingleAsync<dynamic>();

                return user?.employee_id?.ToString() == employeeId;
            }
            catch
            {
                return false;
            }
        }

        private async Task RemoveFromAllActiveTables()
        {
            try
            {
                // Remove from all tables
                await RemoveFromEmployeeDetails();
                await RemoveFromEmploymentInfo();
                await RemoveFromWorkSchedule();
                await RemoveUserAccess();
                await RemoveLeaveCredits();
                await RemoveLeaveNotifications();
                await RemoveAttendanceRecords();
                await RemoveEmployeeDeductions();
                await RemoveGovernmentDeductions();
                await RemoveEmployeeLoans();
                await RemovePayrollRecords();
                await RemovePayrollEarnings();
                await RemovePayrollSummaries();
                await RemoveTodos();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from active tables: {ex.Message}");
            }
        }

        // Existing removal methods
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
                    .OnceSingleAsync<Dictionary<string, dynamic>>();

                if (employmentData != null)
                {
                    string employmentKeyToRemove = null;
                    foreach (var kvp in employmentData)
                    {
                        if (kvp.Value != null)
                        {
                            var empId = kvp.Value.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                employmentKeyToRemove = kvp.Key;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(employmentKeyToRemove))
                    {
                        await firebase
                            .Child("EmploymentInfo")
                            .Child(employmentKeyToRemove)
                            .DeleteAsync();
                    }
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
                    .OnceSingleAsync<Dictionary<string, dynamic>>();

                if (usersData != null)
                {
                    string userKeyToRemove = null;
                    foreach (var kvp in usersData)
                    {
                        if (kvp.Value != null)
                        {
                            var empId = kvp.Value.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                userKeyToRemove = kvp.Key;
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

        private async Task RemoveLeaveNotifications()
        {
            try
            {
                var allNotifications = await firebase
                    .Child("LeaveNotifications")
                    .OnceAsync<dynamic>();

                if (allNotifications != null)
                {
                    foreach (var notification in allNotifications)
                    {
                        if (notification.Object != null)
                        {
                            var notificationEmployee = notification.Object.Employee?.ToString();
                            if (!string.IsNullOrEmpty(notificationEmployee) &&
                                notificationEmployee.Contains(employeeId))
                            {
                                await firebase
                                    .Child("LeaveNotifications")
                                    .Child(notification.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing leave notifications: {ex.Message}");
            }
        }

        // NEW REMOVAL METHODS
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

        private async Task RemoveEmployeeDeductions()
        {
            try
            {
                var allDeductions = await firebase
                    .Child("EmployeeDeductions")
                    .OnceAsync<dynamic>();

                if (allDeductions != null)
                {
                    foreach (var deduction in allDeductions)
                    {
                        if (deduction.Object != null)
                        {
                            var empId = deduction.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                await firebase
                                    .Child("EmployeeDeductions")
                                    .Child(deduction.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
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
                    .OnceAsync<dynamic>();

                if (allGovDeductions != null)
                {
                    foreach (var deduction in allGovDeductions)
                    {
                        if (deduction.Object != null)
                        {
                            var payrollId = deduction.Key.ToString();
                            if (await IsPayrollForEmployee(payrollId))
                            {
                                await firebase
                                    .Child("GovernmentDeductions")
                                    .Child(deduction.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing government deductions: {ex.Message}");
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

        private async Task RemovePayrollRecords()
        {
            try
            {
                var allPayroll = await firebase
                    .Child("Payroll")
                    .OnceAsync<dynamic>();

                if (allPayroll != null)
                {
                    foreach (var payroll in allPayroll)
                    {
                        if (payroll.Object != null)
                        {
                            var empId = payroll.Object.employee_id?.ToString();
                            if (empId == employeeId)
                            {
                                await firebase
                                    .Child("Payroll")
                                    .Child(payroll.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing payroll records: {ex.Message}");
            }
        }

        private async Task RemovePayrollEarnings()
        {
            try
            {
                var allEarnings = await firebase
                    .Child("PayrollEarnings")
                    .OnceAsync<dynamic>();

                if (allEarnings != null)
                {
                    foreach (var earning in allEarnings)
                    {
                        if (earning.Object != null)
                        {
                            var payrollId = earning.Object.payroll_id?.ToString();
                            if (await IsPayrollForEmployee(payrollId))
                            {
                                await firebase
                                    .Child("PayrollEarnings")
                                    .Child(earning.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing payroll earnings: {ex.Message}");
            }
        }

        private async Task RemovePayrollSummaries()
        {
            try
            {
                var allSummaries = await firebase
                    .Child("PayrollSummary")
                    .OnceAsync<dynamic>();

                if (allSummaries != null)
                {
                    foreach (var summary in allSummaries)
                    {
                        if (summary.Object != null)
                        {
                            var payrollId = summary.Object.payroll_id?.ToString();
                            if (await IsPayrollForEmployee(payrollId))
                            {
                                await firebase
                                    .Child("PayrollSummary")
                                    .Child(summary.Key)
                                    .DeleteAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing payroll summaries: {ex.Message}");
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
                    foreach (var todo in allTodos)
                    {
                        if (todo.Object != null)
                        {
                            var assignedTo = todo.Object.assignedTo?.ToString();
                            if (assignedTo != null && await IsUserForEmployee(assignedTo))
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