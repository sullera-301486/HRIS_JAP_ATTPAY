using Firebase.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HRIS_JAP_ATTPAY
{
    public partial class HROverview : UserControl
    {
        private FirebaseClient firebase = new FirebaseClient(
        "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string currentUserId;
        public HROverview(string userId)
        {
            currentUserId = userId;
            InitializeComponent();
            setFont();
            setActivityLogsDataGridViewAttributes();
            setTodoDataGridViewAttributes();
            setAlertAbsentDataGridViewAttributes();
            setAlertLateDataGridViewAttributes();
            setPanelAttributes();
            setCalendar();
            LoadAttendanceSummary(DateTime.Today);
            PopulateDateComboBox();
            LoadTodoList();
            LoadTodaysAbsentEmployees();
            LoadTodaysLateEmployees();
            comboBoxSelectDate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSelectDate.IntegralHeight = false;
            comboBoxSelectDate.MaxDropDownItems = 5;
            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
        }
        private void setFont()
        {
            labelHROverview.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelOverviewDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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
        }
        private async void PopulateDateComboBox()
        {
            try
            {
                comboBoxSelectDate.Items.Clear();

                // Get ALL unique dates from Firebase attendance data
                var allDates = await GetAllAttendanceDatesAsync();

                if (allDates.Count > 0)
                {
                    // Add all unique dates to the combo box (already sorted by GetAllAttendanceDatesAsync)
                    foreach (var date in allDates)
                    {
                        comboBoxSelectDate.Items.Add(date);
                    }

                    // ALWAYS select the most recent date (index 0 since they're sorted descending)
                    comboBoxSelectDate.SelectedIndex = 0;

                    System.Diagnostics.Debug.WriteLine($"Default selected date: {comboBoxSelectDate.SelectedItem}");
                }
                else
                {
                    // Fallback: Add dates for the last 7 days if no data in Firebase
                    for (int i = 0; i < 7; i++)
                    {
                        string date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                        comboBoxSelectDate.Items.Add(date);
                    }

                    if (comboBoxSelectDate.Items.Count > 0)
                    {
                        // Select the most recent date (today)
                        comboBoxSelectDate.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating date combo box: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Simple fallback
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add(DateTime.Today.ToString("yyyy-MM-dd"));
                comboBoxSelectDate.SelectedIndex = 0;
            }
        }
        private async Task<List<string>> GetAllAttendanceDatesAsync()
        {
            var uniqueDates = new HashSet<string>();

            try
            {
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();

                System.Diagnostics.Debug.WriteLine($"Total attendance records found: {attendanceRecords?.Count()}");

                foreach (var attendanceRecord in attendanceRecords)
                {
                    if (attendanceRecord?.Object != null)
                    {
                        try
                        {
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

                // Sort dates in descending order (most recent first) - THIS IS KEY
                var sortedDates = uniqueDates.Select(d => DateTime.Parse(d))
                                     .OrderByDescending(d => d)
                                     .Select(d => d.ToString("yyyy-MM-dd"))
                                     .ToList();

                // Debug: Show the order of dates
                System.Diagnostics.Debug.WriteLine("Dates in combo box (most recent first):");
                foreach (var date in sortedDates)
                {
                    System.Diagnostics.Debug.WriteLine($"  {date}");
                }

                return sortedDates;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving attendance dates: {ex.Message}");

                // Return empty list - the calling method will handle fallback
                return new List<string>();
            }
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

        // NEW METHOD TO GET ALL ACTIVE EMPLOYEES (EXCLUDING ARCHIVED ONES)
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

        // UPDATED ALERT METHODS
        private async void LoadTodaysAbsentEmployees()
        {
            try
            {
                // Get the selected date from combo box
                string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
                DateTime targetDate;

                if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    targetDate = parsedDate;
                }
                else
                {
                    targetDate = DateTime.Today;
                }

                string targetDateStr = targetDate.ToString("yyyy-MM-dd");

                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var absentEmployees = new Dictionary<string, int>();

                // Load employee data
                var employeeDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use date extraction
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, attendanceRecord.Key);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDateStr;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) && status.ToLower() == "absent")
                                {
                                    // Get employee name
                                    string fullName = "N/A";
                                    var empRecord = employeeDetails.FirstOrDefault(e => e.Key == employeeId);
                                    if (empRecord != null)
                                    {
                                        Dictionary<string, object> empDict = ConvertToDictionary(empRecord.Object);
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

                // Update DataGridView
                dataGridViewAlertAbsent.Rows.Clear();
                foreach (var entry in absentEmployees)
                {
                    dataGridViewAlertAbsent.Rows.Add(entry.Key, entry.Value);
                }

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
                // Get the selected date from combo box
                string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
                DateTime targetDate;

                if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    targetDate = parsedDate;
                }
                else
                {
                    targetDate = DateTime.Today;
                }

                string targetDateStr = targetDate.ToString("yyyy-MM-dd");

                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var lateEmployees = new Dictionary<string, int>();

                // Load employee data
                var employeeDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                if (attendanceRecords != null)
                {
                    foreach (var attendanceRecord in attendanceRecords)
                    {
                        if (attendanceRecord?.Object != null)
                        {
                            Dictionary<string, object> attendanceDict = ConvertToDictionary(attendanceRecord.Object);

                            if (attendanceDict != null && attendanceDict.Any())
                            {
                                string status = GetSafeString(attendanceDict, "status");
                                string employeeId = GetSafeString(attendanceDict, "employee_id");

                                // Use date extraction
                                string extractedDate = ExtractDateFromAnyRecordType(attendanceRecord.Object, attendanceRecord.Key);
                                bool matchesDate = !string.IsNullOrEmpty(extractedDate) && extractedDate == targetDateStr;

                                if (matchesDate && !string.IsNullOrEmpty(employeeId) &&
                                    (status.ToLower() == "late" || status.ToLower() == "early out" || status.ToLower() == "late & early out"))
                                {
                                    // Get employee name
                                    string fullName = "N/A";
                                    var empRecord = employeeDetails.FirstOrDefault(e => e.Key == employeeId);
                                    if (empRecord != null)
                                    {
                                        Dictionary<string, object> empDict = ConvertToDictionary(empRecord.Object);
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

                // Update DataGridView
                dataGridViewAlertLate.Rows.Clear();
                foreach (var entry in lateEmployees)
                {
                    dataGridViewAlertLate.Rows.Add(entry.Key, entry.Value);
                }

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
        // ADD THESE HELPER METHODS FROM AdminOverview
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
            panelCalendar.Paint += panelCalendar_Paint;
            panelAttendanceSummary.Paint += panelAttendanceSummary_Paint;
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

        private void setActivityLogsDataGridViewAttributes()
        {
            dataGridViewActivityLogs.GridColor = Color.White;
            dataGridViewActivityLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewActivityLogs.ColumnHeadersHeight = 40;
            dataGridViewActivityLogs.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewActivityLogs.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewActivityLogs.Rows.Add("6 - " + i + " - 25", "10:30 PM", "Manual Entry Added", "Added attendance for Franz Louies Deloritos.");
            }
        }

        private void panelActivityLog_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelActivityLog.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelActivityLog_Resize(object sender, EventArgs e)
        {
            panelActivityLog.Invalidate();
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

            // Delete button column
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

            dataGridViewAlertAbsent.Rows.Clear();
        }

        private void setAlertLateDataGridViewAttributes()
        {
            dataGridViewAlertLate.GridColor = Color.White;
            dataGridViewAlertLate.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAlertLate.ColumnHeadersHeight = 40;
            dataGridViewAlertLate.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAlertLate.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            // REMOVE TEST DATA - data will be loaded from Firebase
            dataGridViewAlertLate.Rows.Clear();
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
                    string taskKey = todoItemKeys[e.RowIndex];
                    string taskText = dataGridViewTodo.Rows[e.RowIndex].Cells["ColumnTask"].Value?.ToString() ?? "";

                    Form parentForm = this.FindForm();
                    ConfirmDeleteTask confirmDeleteTaskForm = new ConfirmDeleteTask(taskKey, taskText, LoadTodoList);
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

        private Dictionary<int, string> todoItemKeys = new Dictionary<int, string>(); // Add this at class level

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
        private void comboBoxSelectDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectDate.SelectedItem == null)
                return;

            string selectedText = comboBoxSelectDate.SelectedItem.ToString();
            DateTime selectedDate;

            // All items in combo box are now actual dates in "yyyy-MM-dd" format
            if (DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime parsedDate))
            {
                selectedDate = parsedDate;
            }
            else
            {
                // Fallback to today if parsing fails
                selectedDate = DateTime.Today;
            }

            // Load the attendance summary for the selected date
            LoadAttendanceSummary(selectedDate);
        }
    }
}
