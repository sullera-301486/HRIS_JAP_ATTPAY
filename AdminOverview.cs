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
            setPayrollLogsDataGridViewAttributes();
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
            LoadPayrollLogs();
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
                labelPayrollLog.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelAdminLog.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
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

                // Add special options
                comboBoxSelectDate.Items.Add("Today");
                comboBoxSelectDate.Items.Add("Yesterday");

                // Get ALL unique dates from Firebase attendance data
                var allDates = await GetAllAttendanceDatesAsync();

                if (allDates.Count > 0)
                {
                    // Add all unique dates to the combo box
                    foreach (var date in allDates)
                    {
                        comboBoxSelectDate.Items.Add(date);
                    }
                }
                else
                {
                    // Fallback: Add dates for the last 30 days if no data in Firebase
                    for (int i = 2; i < 30; i++)
                    {
                        comboBoxSelectDate.Items.Add(DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
                    }
                }

                // Set default selection
                comboBoxSelectDate.SelectedItem = "Today";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating date combo box: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetAllAttendanceDatesAsync()
        {
            var uniqueDates = new HashSet<string>();

            try
            {
                // FIX: Use the same pattern as AdminAttendance
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();

                System.Diagnostics.Debug.WriteLine($"Total attendance records found: {attendanceRecords?.Count()}");

                foreach (var attendanceRecord in attendanceRecords)
                {
                    if (attendanceRecord?.Object != null)
                    {
                        try
                        {
                            // Use the robust date extraction method from AdminAttendance
                            string dateStr = ExtractDateFromAnyRecordType(attendanceRecord.Object, attendanceRecord.Key);

                            if (!string.IsNullOrEmpty(dateStr))
                            {
                                DateTime parsedDate;
                                if (DateTime.TryParse(dateStr, out parsedDate))
                                {
                                    uniqueDates.Add(parsedDate.ToString("yyyy-MM-dd"));
                                }
                                else if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd",
                                         System.Globalization.CultureInfo.InvariantCulture,
                                         System.Globalization.DateTimeStyles.None, out parsedDate))
                                {
                                    uniqueDates.Add(parsedDate.ToString("yyyy-MM-dd"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing record {attendanceRecord.Key}: {ex.Message}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Total unique dates found: {uniqueDates.Count}");

                // Sort dates in descending order (most recent first)
                return uniqueDates.OrderByDescending(d => DateTime.Parse(d)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving attendance dates: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }

        private void comboBoxSelectDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectDate.SelectedItem == null)
                return;

            string selectedText = comboBoxSelectDate.SelectedItem.ToString();
            DateTime selectedDate;

            if (selectedText == "Today")
            {
                selectedDate = DateTime.Today;
            }
            else if (selectedText == "Yesterday")
            {
                selectedDate = DateTime.Today.AddDays(-1);
            }
            else if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime parsedDate))
            {
                selectedDate = parsedDate;
            }
            else
            {
                selectedDate = DateTime.Today;
            }

            LoadAttendanceSummary(selectedDate);
        }

        private async void LoadAttendanceSummary(DateTime date)
        {
            try
            {
                // Show loading state
                labelTotalNumber.Text = "";
                labelOnTimeNumber.Text = "0";
                labelLateNumber.Text = "0";
                labelAbsentNumber.Text = "0";

                // Load ALL employee data and filter out archived employees
                var allEmployees = await GetAllActiveEmployeesAsync();
                int totalEmployees = allEmployees.Count;

                // If no active employees found, show zeros
                if (totalEmployees == 0)
                {
                    labelTotalNumber.Text = "0";
                    labelOnTimeNumber.Text = "0";
                    labelLateNumber.Text = "0";
                    labelAbsentNumber.Text = "0";
                    labelAlertDateRange.Text = $"Date: {date.ToString("yyyy-MM-dd")}";
                    return;
                }

                // Initialize counters
                int onTimeCount = 0;
                int lateCount = 0;
                int absentCount = 0;

                // FIX: Count using status field directly
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                string targetDate = date.ToString("yyyy-MM-dd");

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            // Convert dynamic to dictionary for safe access
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                // Extract values safely using helper methods
                                string status = GetSafeString(attendanceDict, "status", "Absent");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use robust date extraction
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, attendanceRecord.Key);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDate;

                                // Only count if employee is active (not archived) and matches date
                                if (matchesDate && !string.IsNullOrEmpty(employeeId) && allEmployees.Contains(employeeId))
                                {
                                    string lowerStatus = status.ToLower();

                                    if (lowerStatus == "on time" || lowerStatus == "ontime" || lowerStatus == "on-time")
                                    {
                                        onTimeCount++;
                                    }
                                    else if (lowerStatus.Contains("late") || lowerStatus.Contains("early out") || lowerStatus == "late")
                                    {
                                        lateCount++;
                                    }
                                    else if (lowerStatus == "absent")
                                    {
                                        absentCount++;
                                    }
                                }
                            }
                        }
                    }
                }

                // DEBUG: Check the counts
                System.Diagnostics.Debug.WriteLine($"Total Employees: {totalEmployees}");
                System.Diagnostics.Debug.WriteLine($"On Time: {onTimeCount}");
                System.Diagnostics.Debug.WriteLine($"Late/Early Out: {lateCount}");
                System.Diagnostics.Debug.WriteLine($"Absent (from status): {absentCount}");

                // Update UI with counts
                labelTotalNumber.Text = totalEmployees.ToString();
                labelOnTimeNumber.Text = onTimeCount.ToString();
                labelLateNumber.Text = lateCount.ToString();
                labelAbsentNumber.Text = absentCount.ToString();

                // Update the date range label
                labelAlertDateRange.Text = $"Date: {targetDate}";

                // Also refresh the alert grids
                LoadTodaysAbsentEmployees();
                LoadTodaysLateEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance summary: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Reset UI on error
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
                // Load current employees
                var currentEmployees = await firebase.Child("EmployeeDetails").OnceAsync<JObject>();
                if (currentEmployees != null)
                {
                    foreach (var employee in currentEmployees)
                    {
                        activeEmployees.Add(employee.Key);
                    }
                }

                // Load archived employees and remove them from active list
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

        private void setPayrollLogsDataGridViewAttributes()
        {
            dataGridViewPayrollLogs.GridColor = Color.White;
            dataGridViewPayrollLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewPayrollLogs.ColumnHeadersHeight = 40;
            dataGridViewPayrollLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewPayrollLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
        }

        private void setAdminLogsDataGridViewAttributes()
        {
            dataGridViewAdminLogs.GridColor = Color.White;
            dataGridViewAdminLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAdminLogs.ColumnHeadersHeight = 40;
            dataGridViewAdminLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAdminLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewAdminLogs.Rows.Add("6 - " + i + " - 25", "10:30 PM", "Updated Attendance", "Franz Louies Deloritos attendance updated.");
            }
        }

        private void setDailyEmployeeLogsDataGridViewAttributes()
        {
            dataGridViewDailyEmployeeLogs.GridColor = Color.White;
            dataGridViewDailyEmployeeLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewDailyEmployeeLogs.ColumnHeadersHeight = 40;
            dataGridViewDailyEmployeeLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewDailyEmployeeLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewDailyEmployeeLogs.Rows.Add("John Doe time out.");
            }
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

            // Clear existing columns and add the correct ones
            dataGridViewTodo.Columns.Clear();

            // Task column
            dataGridViewTodo.Columns.Add("ColumnTask", "Task");
            dataGridViewTodo.Columns["ColumnTask"].Width = 250;
            dataGridViewTodo.Columns["ColumnTask"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Due Date column
            dataGridViewTodo.Columns.Add("ColumnDueDate", "Due Date");
            dataGridViewTodo.Columns["ColumnDueDate"].Width = 150;

            // Delete button column (make sure you have a trash icon resource)
            DataGridViewImageColumn deleteColumn = new DataGridViewImageColumn();
            deleteColumn.Name = "ColumnTrash";
            deleteColumn.HeaderText = "";
            deleteColumn.Width = 40;
            deleteColumn.Image = Properties.Resources.TrashBin;
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
                var today = DateTime.Today.ToString("yyyy-MM-dd");

                // FIX: Use status field to find absent employees
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var absentEmployees = new Dictionary<string, int>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            // Convert dynamic to dictionary for safe access
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use robust date extraction
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, attendanceRecord.Key);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == today;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) && status.ToLower() == "absent")
                                {
                                    if (absentEmployees.ContainsKey(employeeId))
                                    {
                                        absentEmployees[employeeId]++;
                                    }
                                    else
                                    {
                                        absentEmployees[employeeId] = 1;
                                    }
                                }
                            }
                        }
                    }
                }

                // Load employee names
                var employeeDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var absentEmployeeCounts = new Dictionary<string, int>();

                foreach (var entry in absentEmployees)
                {
                    string empId = entry.Key;
                    int count = entry.Value;

                    var empRecord = employeeDetails.FirstOrDefault(e => e.Key == empId);
                    if (empRecord != null)
                    {
                        Dictionary<string, object> empDict = ConvertToDictionary(empRecord.Object);
                        string firstName = GetSafeString(empDict, "first_name");
                        string lastName = GetSafeString(empDict, "last_name");
                        string fullName = $"{firstName} {lastName}".Trim();

                        if (!string.IsNullOrEmpty(fullName))
                        {
                            absentEmployeeCounts[fullName] = count;
                        }
                    }
                }

                // Clear and add to DataGridView
                dataGridViewAlertAbsent.Rows.Clear();
                foreach (var entry in absentEmployeeCounts)
                {
                    dataGridViewAlertAbsent.Rows.Add(entry.Key, entry.Value);
                }

                // If no absent employees, show message
                if (dataGridViewAlertAbsent.Rows.Count == 0)
                {
                    dataGridViewAlertAbsent.Rows.Add("No absent employees today", "0");
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
                var today = DateTime.Today.ToString("yyyy-MM-dd");

                // FIX: Use the same robust pattern as AdminAttendance
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var lateEmployees = new Dictionary<string, int>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            // Convert dynamic to dictionary for safe access
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use robust date extraction
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, attendanceRecord.Key);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == today;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) &&
                                    (status.ToLower() == "late" || status.ToLower() == "early out" || status.ToLower() == "late & early out"))
                                {
                                    if (lateEmployees.ContainsKey(employeeId))
                                    {
                                        lateEmployees[employeeId]++;
                                    }
                                    else
                                    {
                                        lateEmployees[employeeId] = 1;
                                    }
                                }
                            }
                        }
                    }
                }

                // Load employee names
                var employeeDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var lateEmployeeCounts = new Dictionary<string, int>();

                foreach (var entry in lateEmployees)
                {
                    string empId = entry.Key;
                    int count = entry.Value;

                    var empRecord = employeeDetails.FirstOrDefault(e => e.Key == empId);
                    if (empRecord != null)
                    {
                        Dictionary<string, object> empDict = ConvertToDictionary(empRecord.Object);
                        string firstName = GetSafeString(empDict, "first_name");
                        string lastName = GetSafeString(empDict, "last_name");
                        string fullName = $"{firstName} {lastName}".Trim();

                        if (!string.IsNullOrEmpty(fullName))
                        {
                            lateEmployeeCounts[fullName] = count;
                        }
                    }
                }

                // Clear and add to DataGridView
                dataGridViewAlertLate.Rows.Clear();
                foreach (var entry in lateEmployeeCounts)
                {
                    dataGridViewAlertLate.Rows.Add(entry.Key, entry.Value);
                }

                // If no late employees, show message
                if (dataGridViewAlertLate.Rows.Count == 0)
                {
                    dataGridViewAlertLate.Rows.Add("No late/early out employees today", "0");
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
                // Convert to dictionary first
                Dictionary<string, object> dict = ConvertToDictionary(record);

                if (dict != null)
                {
                    // Check all possible date fields in priority order
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
                                // Extract just the date part (before space if there's time)
                                if (dateStr.Contains(" "))
                                {
                                    dateStr = dateStr.Split(' ')[0];
                                }

                                System.Diagnostics.Debug.WriteLine($"Found {field}: {dateStr} for key: {recordKey}");
                                return dateStr;
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"No date field found for key: {recordKey}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ExtractDateFromAnyRecordType for key {recordKey}: {ex.Message}");
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
            panelPayrollLog.Paint += panelPayrollLog_Paint;
            panelAdminLog.Paint += panelAdminLog_Paint;
            panelDailyEmployeeLog.Paint += panelDailyEmployeeLog_Paint;
            panelAttendanceSummary.Paint += panelAttendanceSummary_Paint;
            panelCalendar.Paint += panelCalendar_Paint;
            panelTodo.Paint += panelTodo_Paint;
        }

        private void panelPayrollLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelPayrollLog.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelPayrollLog_Resize(object sender, EventArgs e)
        {
            panelPayrollLog.Invalidate();
        }

        private void panelAdminLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelAdminLog.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelAdminLog_Resize(object sender, EventArgs e)
        {
            panelAdminLog.Invalidate();
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
        private async void LoadPayrollLogs()
        {
            try
            {
                dataGridViewPayrollLogs.Rows.Clear();

                // Load payroll logs from Firebase
                var payrollLogs = await firebase
                    .Child("PayrollLogs")
                    .OnceAsync<Dictionary<string, object>>();

                if (payrollLogs != null && payrollLogs.Any())
                {
                    foreach (var log in payrollLogs.OrderByDescending(l => l.Key))
                    {
                        var logData = log.Object;
                        if (logData != null)
                        {
                            string date = logData.ContainsKey("date") ? logData["date"]?.ToString() ?? "" : "";
                            string time = logData.ContainsKey("time") ? logData["time"]?.ToString() ?? "" : "";
                            string action = logData.ContainsKey("action") ? logData["action"]?.ToString() ?? "" : "";
                            string details = logData.ContainsKey("details") ? logData["details"]?.ToString() ?? "" : "";

                            // Add to DataGridView
                            dataGridViewPayrollLogs.Rows.Add(date, time, action, details);
                        }
                    }
                }

                // If no logs found, show message
                if (dataGridViewPayrollLogs.Rows.Count == 0)
                {
                    dataGridViewPayrollLogs.Rows.Add("No payroll logs found", "", "", "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payroll logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewPayrollLogs.Rows.Add("Error loading logs", "", "", "");
            }
        }
        private async void LoadAdminLogs()
        {
            try
            {
                // Clear existing rows
                dataGridViewAdminLogs.Rows.Clear();

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
                        dataGridViewAdminLogs.Rows.Add(entry.date, entry.time, entry.action, entry.details);
                    }
                }

                // Show placeholder if no logs
                if (dataGridViewAdminLogs.Rows.Count == 0)
                {
                    // Add sample data matching your required format
                    dataGridViewAdminLogs.Rows.Add("N/A", "N/A", "N/A", "N/A.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading admin logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Add sample data on error for testing
                dataGridViewAdminLogs.Rows.Add("Error", "Error", "Error", "Error");
            }
        }

        // Helper method to format action types
        // Helper method to format action types
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
