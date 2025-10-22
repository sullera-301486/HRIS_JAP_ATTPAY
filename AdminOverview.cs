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
    public partial class AdminOverview : UserControl
    {
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private Dictionary<string, dynamic> employeeCache = new Dictionary<string, dynamic>();
        private DateTime? lastEmployeeCacheUpdate = null;
        private const int CACHE_EXPIRY_MINUTES = 5;
        private string currentUserId;
        private Dictionary<int, string> todoItemKeys = new Dictionary<int, string>();

        public AdminOverview(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            setFont();
            setAdminLogsDataGridViewAttributes();
            setDailyEmployeeLogsDataGridViewAttributes();
            setTodoDataGridViewAttributes();
            setAlertAbsentDataGridViewAttributes();
            setAlertLateDataGridViewAttributes();
            setCalendar();
            setPanelAttributes();
            LoadAttendanceSummary(DateTime.Today);
            PopulateDateComboBox();
            LoadAttendanceSummary(DateTime.Today);
            LoadAdminLogs();
            LoadTodoList();
            comboBoxSelectDate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSelectDate.IntegralHeight = false;
            comboBoxSelectDate.MaxDropDownItems = 5;
        }


        private void setFont()
        {
            try
            {
                labelAdminOverview.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelOverviewDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAuditTrails.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelDailyEmployeeLog.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelAttendanceSummary.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelTotalNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
                labelOnTimeNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
                labelAbsentNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
                labelLateNumber.Font = AttributesClass.GetFont("Roboto-Medium", 52f, FontStyle.Bold);
                labelTotalTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelOnTimeTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelLateTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelAbsentTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelTotalDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelTotalDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOnTimeDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOnTimeDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelLateDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelLateDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelAbsentDescA.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelAbsentDescB.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelTodoDesc.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelAlertDesc.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelAlertDateRange.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelDayOffDate.Font = AttributesClass.GetFont("Roboto-Light", 11f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void PopulateDateComboBox()
        {
            try
            {
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates");

                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var uniqueDates = new HashSet<string>();

                foreach (var record in attendanceRecords)
                {
                    if (record?.Object != null)
                    {
                        try
                        {
                            string dateStr = ExtractDateFromAnyRecordType(record.Object, record.Key);
                            if (!string.IsNullOrEmpty(dateStr))
                            {
                                DateTime parsedDate;
                                if (DateTime.TryParse(dateStr, out parsedDate) ||
                                    DateTime.TryParseExact(dateStr, "yyyy-MM-dd",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out parsedDate))
                                {
                                    uniqueDates.Add(parsedDate.ToString("yyyy-MM-dd"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing record {record.Key}: {ex.Message}");
                        }
                    }
                }

                if (uniqueDates.Count == 0)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        uniqueDates.Add(DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
                    }
                }

                var sortedDates = uniqueDates.Select(d => DateTime.Parse(d))
                                     .OrderByDescending(d => d)
                                     .Select(d => d.ToString("yyyy-MM-dd"))
                                     .ToList();

                foreach (var date in sortedDates)
                {
                    comboBoxSelectDate.Items.Add(date);
                }

                string todayString = DateTime.Today.ToString("yyyy-MM-dd");
                if (comboBoxSelectDate.Items.Contains(todayString))
                {
                    comboBoxSelectDate.SelectedItem = todayString;
                    DateTime selectedDate = DateTime.Parse(todayString);
                    LoadAttendanceSummary(selectedDate);
                }
                else if (comboBoxSelectDate.Items.Count > 1)
                {
                    comboBoxSelectDate.SelectedIndex = 1;
                    string firstDate = comboBoxSelectDate.SelectedItem.ToString();
                    DateTime selectedDate = DateTime.Parse(firstDate);
                    LoadAttendanceSummary(selectedDate);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error populating date combo box: " + ex.Message);
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates");
                for (int i = 0; i < 60; i++)
                {
                    string testDate = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                    comboBoxSelectDate.Items.Add(testDate);
                }
                comboBoxSelectDate.SelectedItem = DateTime.Today.ToString("yyyy-MM-dd");
                LoadAttendanceSummary(DateTime.Today);
            }
        }

        private void comboBoxSelectDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectDate.SelectedItem == null)
                return;

            string selectedText = comboBoxSelectDate.SelectedItem.ToString();
            DateTime? selectedDate = null;

            if (!string.IsNullOrEmpty(selectedText) && selectedText != "All Dates")
            {
                if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate))
                {
                    selectedDate = parsedDate;
                }
            }

            if (selectedDate.HasValue)
            {
                LoadAttendanceSummary(selectedDate.Value);
            }
        }

        private async void LoadAttendanceSummary(DateTime date)
        {
            try
            {
                labelTotalNumber.Text = "";
                labelOnTimeNumber.Text = "0";
                labelLateNumber.Text = "0";
                labelAbsentNumber.Text = "0";

                var allEmployees = await GetAllActiveEmployeesAsync();
                int totalEmployees = allEmployees.Count;

                if (totalEmployees == 0)
                {
                    labelTotalNumber.Text = "0";
                    labelOnTimeNumber.Text = "0";
                    labelLateNumber.Text = "0";
                    labelAbsentNumber.Text = "0";
                    labelAlertDateRange.Text = $"Date: {date.ToString("yyyy-MM-dd")}";
                    return;
                }

                int onTimeCount = 0;
                int lateCount = 0;
                int absentCount = 0;
                int dayOffCount = 0;

                var allAttendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                string targetDate = date.ToString("yyyy-MM-dd");

                if (allAttendanceRecords != null)
                {
                    foreach (var attendanceRecord in allAttendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            string firebaseKey = attendanceRecord.Key;
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string timeInStr = GetSafeString(attendanceDict, "time_in");
                                string timeOutStr = GetSafeString(attendanceDict, "time_out");
                                string existingStatus = GetSafeString(attendanceDict, "status");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, firebaseKey);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDate;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) && allEmployees.Contains(employeeId))
                                {
                                    string status = CalculateStatus(timeInStr, timeOutStr, existingStatus);

                                    if (status.Equals("On Time", StringComparison.OrdinalIgnoreCase))
                                    {
                                        onTimeCount++;
                                    }
                                    else if (status.Equals("Late", StringComparison.OrdinalIgnoreCase) ||
                                             status.Equals("Early Out", StringComparison.OrdinalIgnoreCase) ||
                                             status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase))
                                    {
                                        lateCount++;
                                    }
                                    else if (status.Equals("Absent", StringComparison.OrdinalIgnoreCase))
                                    {
                                        absentCount++;
                                    }
                                    else if (status.Equals("Day Off", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dayOffCount++;
                                    }
                                }
                            }
                        }
                    }
                }

                int totalExpectedEmployees = totalEmployees - dayOffCount;
                labelTotalNumber.Text = totalExpectedEmployees.ToString();
                labelOnTimeNumber.Text = onTimeCount.ToString();
                labelLateNumber.Text = lateCount.ToString();
                labelAbsentNumber.Text = absentCount.ToString();
                labelAlertDateRange.Text = $"Date: {targetDate}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance summary: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                labelTotalNumber.Text = "0";
                labelOnTimeNumber.Text = "0";
                labelLateNumber.Text = "0";
                labelAbsentNumber.Text = "0";
            }
        }

        // New method to get all active employees (excluding archived ones)
        private async Task<HashSet<string>> GetAllActiveEmployeesAsync()
        {
            var activeEmployees = new HashSet<string>();
            try
            {
                var currentEmployees = await firebase.Child("EmployeeDetails").OnceAsync<JObject>();
                if (currentEmployees != null)
                {
                    foreach (var employee in currentEmployees)
                    {
                        activeEmployees.Add(employee.Key);
                    }
                }

                var archivedEmployees = await firebase.Child("ArchivedEmployees").OnceAsync<JObject>();
                if (archivedEmployees != null)
                {
                    foreach (var archivedEmployee in archivedEmployees)
                    {
                        activeEmployees.Remove(archivedEmployee.Key);
                    }
                }

                return activeEmployees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading active employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return activeEmployees;
            }
        }


        private void setAdminLogsDataGridViewAttributes()
        {
            dataGridViewAuditTrails.GridColor = Color.White;
            dataGridViewAuditTrails.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAuditTrails.ColumnHeadersHeight = 40;
            dataGridViewAuditTrails.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAuditTrails.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
        }

        private void setDailyEmployeeLogsDataGridViewAttributes()
        {
            dataGridViewDayOffLogs.GridColor = Color.White;
            dataGridViewDayOffLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewDayOffLogs.ColumnHeadersHeight = 40;
            dataGridViewDayOffLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewDayOffLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewDayOffLogs.Columns.Clear();

            dataGridViewDayOffLogs.Columns.Add("ColumnName", "Name");
            dataGridViewDayOffLogs.Columns.Add("ColumnCount", "Count");

            dataGridViewDayOffLogs.Columns["ColumnName"].Width = 200;
            dataGridViewDayOffLogs.Columns["ColumnCount"].Width = 100;

            dataGridViewDayOffLogs.Rows.Clear();
            LoadTodaysDayOffEmployees();
        }
        private async void LoadTodaysDayOffEmployees()
        {
            try
            {
                // Get the selected date from the combo box - same pattern as alert grids
                string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
                DateTime targetDate;

                if (string.IsNullOrEmpty(selectedText) || selectedText == "All Dates")
                    targetDate = DateTime.Today;
                else if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                         System.Globalization.CultureInfo.InvariantCulture,
                         System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    targetDate = parsedDate;
                else
                    targetDate = DateTime.Today;

                string targetDateStr = targetDate.ToString("yyyy-MM-dd");

                // Use the same approach as AdminAttendance - following alert grid pattern
                var allAttendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var dayOffEmployees = new Dictionary<string, int>(); // Name -> Count (following alert grid pattern)

                // Load employee data first (same as alert grids)
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                // Also load archived employees for complete data
                var archivedEmployees = await firebase.Child("ArchivedEmployees").OnceAsync<dynamic>();
                foreach (var archivedEmp in archivedEmployees)
                {
                    if (archivedEmp.Object != null && archivedEmp.Object.employee_data != null)
                    {
                        employeeDict[archivedEmp.Key] = archivedEmp.Object.employee_data;
                    }
                }

                if (allAttendanceRecords != null)
                {
                    foreach (var attendanceRecord in allAttendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            string firebaseKey = attendanceRecord.Key;

                            // Use the same robust processing as alert grids
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status", "Absent");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use the same date extraction method as alert grids
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, firebaseKey);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDateStr;

                                // Check for Day Off status - same logic pattern as alert grids
                                if (matchesDate && !string.IsNullOrEmpty(employeeId) &&
                                    status.Equals("Day Off", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Get employee name using the ConvertToDictionary approach - same as alert grids
                                    string fullName = "N/A";
                                    if (employeeDict.ContainsKey(employeeId))
                                    {
                                        var employee = employeeDict[employeeId];
                                        Dictionary<string, object> empDict = ConvertToDictionary(employee);
                                        string firstName = GetSafeString(empDict, "first_name");
                                        string middleName = GetSafeString(empDict, "middle_name");
                                        string lastName = GetSafeString(empDict, "last_name");

                                        // Try full_name field if available
                                        string fullNameField = GetSafeString(empDict, "full_name");
                                        if (!string.IsNullOrEmpty(fullNameField))
                                        {
                                            fullName = fullNameField;
                                        }
                                        else
                                        {
                                            fullName = $"{firstName} {middleName} {lastName}".Trim();
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(fullName) && fullName != "N/A")
                                    {
                                        // Following the same pattern as alert grids - count occurrences
                                        if (dayOffEmployees.ContainsKey(fullName))
                                        {
                                            dayOffEmployees[fullName]++;
                                        }
                                        else
                                        {
                                            dayOffEmployees[fullName] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Clear and add to DataGridView - same pattern as alert grids
                dataGridViewDayOffLogs.Rows.Clear();
                foreach (var entry in dayOffEmployees)
                {
                    dataGridViewDayOffLogs.Rows.Add(entry.Key, entry.Value);
                }

                // If no day off employees, show message - same pattern as alert grids
                if (dataGridViewDayOffLogs.Rows.Count == 0)
                {
                    dataGridViewDayOffLogs.Rows.Add("No employees on day off", "0");
                }

                // Update the date label for Day Off section to match the alert format
                UpdateDayOffDateLabel(targetDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading day off employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewDayOffLogs.Rows.Clear();
                dataGridViewDayOffLogs.Rows.Add("Error loading data", "0");
            }
        }
        private void UpdateDayOffDateLabel(DateTime date)
        {

            labelDayOffDate.Text = $"Date: {date.ToString("yyyy-MM-dd")}";

        }

        private void setTodoDataGridViewAttributes()
        {
            dataGridViewTodo.GridColor = Color.White;
            dataGridViewTodo.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewTodo.ColumnHeadersHeight = 40;
            dataGridViewTodo.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewTodo.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewTodo.CellMouseEnter += dataGridViewTodo_CellMouseEnter;
            dataGridViewTodo.CellMouseLeave += dataGridViewTodo_CellMouseLeave;
            dataGridViewTodo.CellClick += dataGridViewTodo_CellClick;

            dataGridViewTodo.Columns.Clear();

            dataGridViewTodo.Columns.Add("ColumnTask", "Task");
            dataGridViewTodo.Columns["ColumnTask"].Width = 250;
            dataGridViewTodo.Columns["ColumnTask"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTodo.Columns["ColumnTask"].ReadOnly = true;

            dataGridViewTodo.Columns.Add("ColumnDueDate", "Due Date");
            dataGridViewTodo.Columns["ColumnDueDate"].Width = 150;
            dataGridViewTodo.Columns["ColumnDueDate"].ReadOnly = true;

            DataGridViewImageColumn deleteColumn = new DataGridViewImageColumn();
            deleteColumn.Name = "ColumnTrash";
            deleteColumn.HeaderText = "";
            deleteColumn.Width = 40;
            deleteColumn.Image = Properties.Resources.TrashBin;
            deleteColumn.ReadOnly = true;
            dataGridViewTodo.Columns.Add(deleteColumn);
        }

        private void setAlertAbsentDataGridViewAttributes()
        {
            dataGridViewAlertAbsent.GridColor = Color.White;
            dataGridViewAlertAbsent.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertAbsent.ColumnHeadersHeight = 40;
            dataGridViewAlertAbsent.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertAbsent.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            // Clear existing rows
            dataGridViewAlertAbsent.Rows.Clear();

            // Load today's absent employees
            LoadTodaysAbsentEmployees();
        }

        private void setAlertLateDataGridViewAttributes()
        {
            dataGridViewAlertLate.GridColor = Color.White;
            dataGridViewAlertLate.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertLate.ColumnHeadersHeight = 40;
            dataGridViewAlertLate.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertLate.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            // Clear existing rows
            dataGridViewAlertLate.Rows.Clear();

            // Load today's late/early out employees
            LoadTodaysLateEmployees();
        }

        private async void LoadTodaysAbsentEmployees()
        {
            try
            {
                // Get the selected date from the combo box
                string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
                DateTime targetDate;

                if (selectedText == "Today")
                    targetDate = DateTime.Today;
                else if (selectedText == "Yesterday")
                    targetDate = DateTime.Today.AddDays(-1);
                else if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    targetDate = parsedDate;
                else
                    targetDate = DateTime.Today;

                string targetDateStr = targetDate.ToString("yyyy-MM-dd");

                // Use the same approach as AdminAttendance
                var allAttendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var absentEmployees = new Dictionary<string, int>();

                // Load employee data first (same as AdminAttendance)
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                if (allAttendanceRecords != null)
                {
                    foreach (var attendanceRecord in allAttendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            string firebaseKey = attendanceRecord.Key;

                            // Use the same robust processing as AdminAttendance
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status", "Absent");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use the same date extraction method as AdminAttendance
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, firebaseKey);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDateStr;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) && status.ToLower() == "absent")
                                {
                                    // Get employee name using the ConvertToDictionary approach
                                    string fullName = "N/A";
                                    if (employeeDict.ContainsKey(employeeId))
                                    {
                                        var employee = employeeDict[employeeId];
                                        Dictionary<string, object> empDict = ConvertToDictionary(employee);
                                        string firstName = GetSafeString(empDict, "first_name");
                                        string middleName = GetSafeString(empDict, "middle_name");
                                        string lastName = GetSafeString(empDict, "last_name");
                                        fullName = $"{firstName} {middleName} {lastName}".Trim();
                                    }

                                    if (!string.IsNullOrEmpty(fullName) && fullName != "N/A")
                                    {
                                        if (absentEmployees.ContainsKey(fullName))
                                        {
                                            absentEmployees[fullName]++;
                                        }
                                        else
                                        {
                                            absentEmployees[fullName] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Clear and add to DataGridView
                dataGridViewAlertAbsent.Rows.Clear();
                foreach (var entry in absentEmployees)
                {
                    dataGridViewAlertAbsent.Rows.Add(entry.Key, entry.Value);
                }

                // If no absent employees, show message
                if (dataGridViewAlertAbsent.Rows.Count == 0)
                {
                    dataGridViewAlertAbsent.Rows.Add("No absent employees", "0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading absent employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewAlertAbsent.Rows.Clear();
                dataGridViewAlertAbsent.Rows.Add("Error loading data", "0");
            }
        }

        private async void LoadTodaysLateEmployees()
        {
            try
            {
                // Get the selected date from the combo box
                string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
                DateTime targetDate;

                if (selectedText == "Today")
                    targetDate = DateTime.Today;
                else if (selectedText == "Yesterday")
                    targetDate = DateTime.Today.AddDays(-1);
                else if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    targetDate = parsedDate;
                else
                    targetDate = DateTime.Today;

                string targetDateStr = targetDate.ToString("yyyy-MM-dd");

                // Use the same approach as AdminAttendance
                var allAttendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var lateEmployees = new Dictionary<string, int>();

                // Load employee data first (same as AdminAttendance)
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                if (allAttendanceRecords != null)
                {
                    foreach (var attendanceRecord in allAttendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            string firebaseKey = attendanceRecord.Key;

                            // Use the same robust processing as AdminAttendance
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status", "Absent");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use the same date extraction method as AdminAttendance
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, firebaseKey);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDateStr;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) &&
                                    (status.ToLower() == "late" || status.ToLower() == "early out" || status.ToLower() == "late & early out"))
                                {
                                    // Get employee name using the ConvertToDictionary approach
                                    string fullName = "N/A";
                                    if (employeeDict.ContainsKey(employeeId))
                                    {
                                        var employee = employeeDict[employeeId];
                                        Dictionary<string, object> empDict = ConvertToDictionary(employee);
                                        string firstName = GetSafeString(empDict, "first_name");
                                        string middleName = GetSafeString(empDict, "middle_name");
                                        string lastName = GetSafeString(empDict, "last_name");
                                        fullName = $"{firstName} {middleName} {lastName}".Trim();
                                    }

                                    if (!string.IsNullOrEmpty(fullName) && fullName != "N/A")
                                    {
                                        if (lateEmployees.ContainsKey(fullName))
                                        {
                                            lateEmployees[fullName]++;
                                        }
                                        else
                                        {
                                            lateEmployees[fullName] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Clear and add to DataGridView
                dataGridViewAlertLate.Rows.Clear();
                foreach (var entry in lateEmployees)
                {
                    dataGridViewAlertLate.Rows.Add(entry.Key, entry.Value);
                }

                // If no late employees, show message
                if (dataGridViewAlertLate.Rows.Count == 0)
                {
                    dataGridViewAlertLate.Rows.Add("No late/early out employees", "0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading late employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewAlertLate.Rows.Clear();
                dataGridViewAlertLate.Rows.Add("Error loading data", "0");
            }
        }
        // ADD THESE HELPER METHODS FROM AdminAttendance
        private string ExtractDateFromAnyRecordType(dynamic record, string recordKey)
        {
            try
            {
                Dictionary<string, object> dict = ConvertToDictionary(record);
                if (dict != null)
                {
                    string[] possibleDateFields = {
                "attendance_date", "created_date", "date",
                "attendanceDate", "AttendanceDate"
            };
                    foreach (string field in possibleDateFields)
                    {
                        if (dict.ContainsKey(field) && dict[field] != null)
                        {
                            string dateStr = dict[field].ToString();
                            if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                            {
                                if (dateStr.Contains(" "))
                                    dateStr = dateStr.Split(' ')[0];
                                return dateStr;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private Dictionary<string, object> ConvertToDictionary(dynamic obj)
        {
            try
            {
                if (obj is IDictionary<string, object> directDict)
                {
                    return new Dictionary<string, object>(directDict);
                }
                else
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                }
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private string GetSafeString(Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            try
            {
                if (dict.ContainsKey(key) && dict[key] != null)
                {
                    return dict[key].ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting {key}: {ex.Message}");
            }
            return defaultValue;
        }

        private void setPanelAttributes()
        {
            panelAuditTrails.Paint += panelAdminLog_Paint;
            panelDailyEmployeeLog.Paint += panelDailyEmployeeLog_Paint;
            panelAttendanceSummary.Paint += panelAttendanceSummary_Paint;
            panelCalendar.Paint += panelCalendar_Paint;
            panelTodo.Paint += panelTodo_Paint;
        }

        private void panelAdminLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAuditTrails.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAdminLog_Resize(object sender, EventArgs e)
        {
            panelAuditTrails.Invalidate();
        }

        private void panelDailyEmployeeLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelDailyEmployeeLog.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelDailyEmployeeLog_Resize(object sender, EventArgs e)
        {
            panelDailyEmployeeLog.Invalidate();
        }

        private void panelAttendanceSummary_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAttendanceSummary.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAttendanceSummary_Resize(object sender, EventArgs e)
        {
            panelAttendanceSummary.Invalidate();
        }

        private void panelCalendar_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelCalendar.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelCalendar_Resize(object sender, EventArgs e)
        {
            panelCalendar.Invalidate();
        }

        private void setCalendar()
        {
            Calendar myCalendar = new Calendar();
            int borderPadding = 1;

            myCalendar.Location = new Point(borderPadding, borderPadding);
            myCalendar.Size = new Size(
                panelCalendar.ClientSize.Width - borderPadding * 2,
                panelCalendar.ClientSize.Height - borderPadding * 2
            );

            // make it resize along with the panel
            myCalendar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            panelCalendar.Controls.Clear();
            panelCalendar.Controls.Add(myCalendar);
        }

        private void panelTodo_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelTodo.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelTodo_Resize(object sender, EventArgs e)
        {
            panelTodo.Invalidate();
        }

        private void panelAlert_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAlert.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAlert_Resize(object sender, EventArgs e)
        {
            panelAlert.Invalidate();
        }

        private void dataGridViewTodo_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewTodo.Columns[e.ColumnIndex].Name == "ColumnTrash")
                dataGridViewTodo.Cursor = Cursors.Hand;
        }

        private void dataGridViewTodo_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewTodo.Cursor = Cursors.Default;
        }

        private void dataGridViewTodo_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewTodo.Columns[e.ColumnIndex].Name == "ColumnTrash")
            {
                if (todoItemKeys.ContainsKey(e.RowIndex))
                {
                    // Debug: Check what columns you actually have
                    string columnNames = "";
                    foreach (DataGridViewColumn col in dataGridViewTodo.Columns)
                    {
                        columnNames += col.Name + ", ";
                    }
                    Console.WriteLine($"Available columns: {columnNames}");

                    string taskKey = todoItemKeys[e.RowIndex];

                    // Try different possible column names
                    string taskText = "";
                    if (dataGridViewTodo.Columns.Contains("ColumnTask"))
                    {
                        taskText = dataGridViewTodo.Rows[e.RowIndex].Cells["ColumnTask"].Value?.ToString() ?? "";
                    }
                    else if (dataGridViewTodo.Columns.Contains("Task"))
                    {
                        taskText = dataGridViewTodo.Rows[e.RowIndex].Cells["Task"].Value?.ToString() ?? "";
                    }
                    else if (dataGridViewTodo.Columns.Count > 0)
                    {
                        taskText = dataGridViewTodo.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? ""; // First column
                    }

                    Form parentForm = this.FindForm();
                    ConfirmDeleteTask confirmDeleteTaskForm = new ConfirmDeleteTask(
                        taskKey,
                        taskText,
                        LoadTodoList
                    );
                    AttributesClass.ShowWithOverlay(parentForm, confirmDeleteTaskForm);
                }
            }
        }

        private void labelTodoAdd_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddNewTask addNewTaskForm = new AddNewTask(currentUserId);
            addNewTaskForm.TaskAdded += (s, args) =>
            {
                LoadTodoList();
            };
            AttributesClass.ShowWithOverlay(parentForm, addNewTaskForm);
        }

        public void RefreshTodoList()
        {
            LoadTodoList();
        }

        private async void LoadTodoList()
        {
            try
            {
                dataGridViewTodo.Rows.Clear();
                todoItemKeys.Clear();

                Console.WriteLine($"Loading todos for user ID: {currentUserId}");

                // Load todo items from Firebase
                var todoItems = await firebase
                    .Child("Todos")
                    .OnceAsync<Dictionary<string, object>>();

                if (todoItems != null && todoItems.Any())
                {
                    int rowIndex = 0;
                    int totalTasks = 0;
                    int userTasks = 0;

                    foreach (var todoItem in todoItems)
                    {
                        totalTasks++;
                        var todoData = todoItem.Object;

                        if (todoData != null && todoData.ContainsKey("task"))
                        {
                            string task = todoData["task"]?.ToString() ?? "";
                            string dueDate = todoData.ContainsKey("dueDate") ? todoData["dueDate"].ToString() : "";
                            string createdBy = todoData.ContainsKey("createdBy") ? todoData["createdBy"].ToString() : "";
                            string assignedTo = todoData.ContainsKey("assignedTo") ? todoData["assignedTo"].ToString() : "";

                            // Debug output for each task
                            Console.WriteLine($"Task: {task}, CreatedBy: {createdBy}, AssignedTo: {assignedTo}, CurrentUser: {currentUserId}");

                            // FIX: Only show tasks created by OR assigned to the current user
                            bool isCreator = createdBy == currentUserId;
                            bool isAssigned = assignedTo == currentUserId || assignedTo == "all";

                            if (isCreator || isAssigned)
                            {
                                userTasks++;

                                // Format date if possible
                                string formattedDate = dueDate;
                                if (DateTime.TryParse(dueDate, out DateTime parsedDate))
                                {
                                    formattedDate = parsedDate.ToString("MM/dd/yyyy");
                                }
                                else if (string.IsNullOrEmpty(dueDate))
                                {
                                    formattedDate = "No due date";
                                }

                                // Add to DataGridView
                                dataGridViewTodo.Rows.Add(task, formattedDate);

                                // Store the Firebase key for this row
                                todoItemKeys[rowIndex] = todoItem.Key;
                                rowIndex++;
                            }
                        }
                    }

                    Console.WriteLine($"Total tasks: {totalTasks}, User tasks: {userTasks}");
                }

                // If no tasks found, show a message
                if (dataGridViewTodo.Rows.Count == 0)
                {
                    dataGridViewTodo.Rows.Add("No tasks found", "");
                    Console.WriteLine("No tasks found for current user");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading todo list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewTodo.Rows.Add("Error loading tasks", "");
                Console.WriteLine($"Error loading todos: {ex.Message}");
            }
        }

        private async void LoadAdminLogs()
        {
            try
            {
                // Clear existing rows
                dataGridViewAuditTrails.Rows.Clear();

                // Load admin logs from Firebase
                var adminLogs = await firebase
                    .Child("AdminLogs")
                    .OnceAsync<dynamic>();

                if (adminLogs != null && adminLogs.Any())
                {
                    // Create a list to sort by timestamp
                    var logEntries = new List<(DateTime timestamp, string date, string time, string action, string details)>();

                    foreach (var log in adminLogs)
                    {
                        try
                        {
                            var logData = log.Object;

                            // Extract data from Firebase structure
                            string timestampStr = logData.timestamp?.ToString() ?? "";
                            string actionType = logData.action_type?.ToString() ?? "Unknown Action";
                            string description = logData.description?.ToString() ?? "";
                            string details = logData.details?.ToString() ?? "";
                            string adminName = logData.admin_name?.ToString() ?? "";
                            string targetEmployeeId = logData.target_employee_id?.ToString() ?? "";

                            // Parse timestamp for sorting
                            DateTime timestamp = DateTime.MinValue;
                            if (!string.IsNullOrEmpty(timestampStr) &&
                                DateTime.TryParse(timestampStr, out DateTime parsedTime))
                            {
                                timestamp = parsedTime;
                            }
                            else
                            {
                                // Use current time if parsing fails
                                timestamp = DateTime.Now;
                            }

                            // Format date and time in the required format
                            string date = timestamp.ToString("M - d - yy"); // "6 - 10 - 25"
                            string time = timestamp.ToString("h:mm tt");    // "10:30 PM"

                            // Format the action and details
                            string action = FormatActionType(actionType);

                            logEntries.Add((timestamp, date, time, action, description));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing log entry: {ex.Message}");
                            // Continue to next log entry
                        }
                    }

                    // Sort by timestamp descending (most recent first) and take last 30
                    var sortedLogs = logEntries
                        .OrderByDescending(entry => entry.timestamp)
                        .Take(30);

                    foreach (var entry in sortedLogs)
                    {
                        dataGridViewAuditTrails.Rows.Add(entry.date, entry.time, entry.action, entry.details);
                    }
                }

                // Show placeholder if no logs
                if (dataGridViewAuditTrails.Rows.Count == 0)
                {
                    // Add sample data matching your required format
                    dataGridViewAuditTrails.Rows.Add("N/A", "N/A", "N/A", "N/A.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading admin logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Add sample data on error for testing
                dataGridViewAuditTrails.Rows.Add("Error", "Error", "Error", "Error");
            }
        }

        // Helper method to format action types
        // Helper method to format action types
        private string CalculateStatus(string timeInStr, string timeOutStr, string existingStatus = "")
        {
            // If the existing status is already "Day Off", preserve it
            if (!string.IsNullOrEmpty(existingStatus) && existingStatus.Equals("Day Off", StringComparison.OrdinalIgnoreCase))
                return "Day Off";

            // If time in is N/A, then it's Absent (unless it's already marked as something else)
            if (string.IsNullOrEmpty(timeInStr) || timeInStr == "N/A")
            {
                // If we have an existing status that's not Day Off, use it
                if (!string.IsNullOrEmpty(existingStatus) && !existingStatus.Equals("Day Off", StringComparison.OrdinalIgnoreCase))
                    return existingStatus;
                return "Absent";
            }

            try
            {
                DateTime timeIn, timeOut;

                // Try to parse different time formats for Time In
                bool timeInParsed = DateTime.TryParseExact(timeInStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInStr, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParse(timeInStr, out timeIn);

                bool timeOutParsed = DateTime.TryParseExact(timeOutStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutStr, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParse(timeOutStr, out timeOut);

                if (timeInParsed)
                {
                    // Basic status calculation - adjust based on your business rules
                    DateTime expectedStart = DateTime.Today.AddHours(8); // 8:00 AM
                    DateTime expectedEnd = DateTime.Today.AddHours(17); // 5:00 PM

                    DateTime actualTimeIn = DateTime.Today.AddHours(timeIn.Hour).AddMinutes(timeIn.Minute);

                    bool isLate = actualTimeIn > expectedStart; // No grace period - late if after 8:00 AM
                    bool isEarlyOut = false;

                    // Check for early out only if we have valid time out
                    if (timeOutParsed && timeOutStr != "N/A" && !string.IsNullOrEmpty(timeOutStr))
                    {
                        DateTime actualTimeOut = DateTime.Today.AddHours(timeOut.Hour).AddMinutes(timeOut.Minute);
                        isEarlyOut = actualTimeOut < expectedEnd; // Early out if before 5:00 PM
                    }

                    if (isLate && isEarlyOut)
                        return "Late & Early Out";
                    else if (isLate)
                        return "Late";
                    else if (isEarlyOut)
                        return "Early Out";
                    else
                        return "On Time";
                }
                else
                {
                    // If parsing fails, return the existing status or Absent
                    return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
                }
            }
            catch
            {
                // If any error occurs, return the existing status or Absent
                return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
            }
        }
        private void dataGridViewDayOffLogs_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private string FormatActionType(string actionType)
        {
            if (string.IsNullOrEmpty(actionType))
                return "Unknown Action";

            // Map Firebase action types to display format using classic switch case
            switch (actionType)
            {
                case "Employee Updated":
                    return "Updated Employee Profile";
                case "Employee Added":
                    return "Added Employee";
                case "Employee Archived":
                    return "Archived Employee";
                case "Attendance Updated":
                    return "Updated Attendance";
                case "Leave Approved":
                    return "Approved Leave";
                case "Leave Rejected":
                    return "Rejected Leave";
                case "Payroll Generated":
                    return "Generated Payroll";
                case "Payroll Exported":
                    return "Exported Payroll";
                case "Work Schedule Updated":
                    return "Updated Work Schedule";
                case "User Account Added":
                    return "Added User Account";
                case "User Account Updated":
                    return "Updated User Account";
                case "User Account Removed":
                    return "Removed User Account";
                default:
                    return actionType;
            }
        }
    }
}
