using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminAttendance : UserControl
    {
        // 🔹 Firebase client
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public AdminAttendance()
        {
            InitializeComponent();
            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();

            // 🔹 Load Firebase attendance data and populate date combo box
            DebugFirebaseData(); // Add this for debugging
            LoadFirebaseAttendanceData();
            PopulateDateComboBox();
        }

        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearchEmployee.Text.Trim().ToLower();

            // If search is empty, show all rows
            if (string.IsNullOrEmpty(searchText))
            {
                foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
                return;
            }

            // Filter rows based on search text
            foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
            {
                if (!row.IsNewRow)
                {
                    // Get values from relevant columns
                    string employeeId = row.Cells["EmployeeId"].Value?.ToString()?.ToLower() ?? "";
                    string fullName = row.Cells["FullName"].Value?.ToString()?.ToLower() ?? "";
                    string status = row.Cells["Status"].Value?.ToString()?.ToLower() ?? "";

                    // Check if any of the columns contain the search text
                    bool isMatch = employeeId.Contains(searchText) ||
                                  fullName.Contains(searchText) ||
                                  status.Contains(searchText);

                    row.Visible = isMatch;
                }
            }
        }

        private void labelManageLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ManageLeave manageLeaveForm = new ManageLeave();
            AttributesClass.ShowWithOverlay(parentForm, manageLeaveForm);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminAttendance filterAdminAttendanceForm = new FilterAdminAttendance();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminAttendanceForm);
        }

        private void comboBoxSelectDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
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

            // 🔹 Refresh attendance with selected date
            LoadFirebaseAttendanceData(selectedDate);
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewAttendance.ReadOnly = true;
            dataGridViewAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAttendance.MultiSelect = false;
            dataGridViewAttendance.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewAttendance.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewAttendance.DefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewAttendance.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewAttendance.GridColor = Color.White;
            dataGridViewAttendance.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAttendance.ColumnHeadersHeight = 40;
            dataGridViewAttendance.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAttendance.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewAttendance.CellMouseEnter += dataGridViewAttendance_CellMouseEnter;
            dataGridViewAttendance.CellMouseLeave += dataGridViewAttendance_CellMouseLeave;
            dataGridViewAttendance.CellClick += dataGridViewAttendance_CellClick;
            dataGridViewAttendance.CellFormatting += dataGridViewAttendance_CellFormatting;

            // 🔹 Define columns
            dataGridViewAttendance.Columns.Clear();
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "RowNumber", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeIn", HeaderText = "Time In", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeOut", HeaderText = "Time Out", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoursWorked", HeaderText = "Hours Worked", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "OvertimeHours", HeaderText = "Overtime Hours", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn { Name = "VerificationMethod", HeaderText = "Verification", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewAttendance.Columns.Add(new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25,
                Image = Properties.Resources.VerticalThreeDots
            });
        }

        private void setFont()
        {
            try
            {
                labelAdminAttendance.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelAttendanceDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManageLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
                comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void dataGridViewAttendance_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Action")
                dataGridViewAttendance.Cursor = Cursors.Hand;
        }

        private void dataGridViewAttendance_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewAttendance.Cursor = Cursors.Default;
        }

        private void dataGridViewAttendance_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Action")
            {
                Form parentForm = this.FindForm();
                EditAttendance editAttendanceForm = new EditAttendance();
                AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);
            }
        }

        private void dataGridViewAttendance_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                // Store the original background and foreground colors for the status cell
                Color statusBackColor = Color.White;
                Color statusForeColor = Color.Black;

                switch (e.Value.ToString())
                {
                    case "On Time":
                        statusBackColor = Color.FromArgb(95, 218, 71);
                        statusForeColor = Color.White;
                        break;
                    case "Late":
                    case "Early Out":
                        statusBackColor = Color.FromArgb(255, 163, 74);
                        statusForeColor = Color.White;
                        break;
                    case "Absent":
                        statusBackColor = Color.FromArgb(221, 60, 60);
                        statusForeColor = Color.White;
                        break;
                    case "Leave":
                        statusBackColor = Color.FromArgb(71, 93, 218);
                        statusForeColor = Color.White;
                        break;
                    case "Day Off":
                        statusBackColor = Color.FromArgb(180, 174, 189);
                        statusForeColor = Color.White;
                        break;
                }

                // Apply the colors - always show the exact same status color, even when selected
                e.CellStyle.BackColor = statusBackColor;
                e.CellStyle.ForeColor = statusForeColor;
                e.CellStyle.SelectionBackColor = statusBackColor;
                e.CellStyle.SelectionForeColor = statusForeColor;
            }
        }

        private string FormatFirebaseTime(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr) || dateTimeStr == "N/A")
                return "N/A";

            try
            {
                if (DateTime.TryParse(dateTimeStr, out DateTime dt))
                    return dt.ToString("h:mm tt"); // Only time, e.g., 8:30 AM

                return dateTimeStr;
            }
            catch
            {
                return dateTimeStr;
            }
        }

        private string CalculateOvertimeHours(string overtimeHoursStr, string hoursWorked, string employeeId, string scheduleId)
        {
            // First try to use the overtime_hours field directly from Firebase
            if (!string.IsNullOrEmpty(overtimeHoursStr) && double.TryParse(overtimeHoursStr, out double overtimeHours))
            {
                return Math.Round(overtimeHours, 2).ToString("0.00");
            }

            // Fallback calculation if overtime_hours is not available
            if (string.IsNullOrEmpty(hoursWorked) || hoursWorked == "N/A" || !double.TryParse(hoursWorked, out double workedHours))
                return "0.00";

            double regularHours = 8.0;
            double overtime = Math.Max(0, workedHours - regularHours);

            return Math.Round(overtime, 2).ToString("0.00");
        }

        private async void PopulateDateComboBox()
        {
            try
            {
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates"); // Default option

                // Get unique dates from attendance records
                var attendanceArray = await firebase.Child("Attendance").OnceSingleAsync<JArray>();
                if (attendanceArray != null)
                {
                    var uniqueDates = new HashSet<string>();

                    foreach (var attendanceItem in attendanceArray)
                    {
                        if (attendanceItem != null && attendanceItem.Type != JTokenType.Null)
                        {
                            string dateStr = attendanceItem["attendance_date"]?.ToString();
                            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime date))
                            {
                                // Always format as yyyy-MM-dd
                                uniqueDates.Add(date.ToString("yyyy-MM-dd"));
                            }
                        }
                    }

                    // Add dates to combo box in descending order (newest first)
                    var sortedDates = uniqueDates.Select(d => DateTime.Parse(d))
                                                .OrderByDescending(d => d)
                                                .Select(d => d.ToString("yyyy-MM-dd"))
                                                .ToList();

                    foreach (var date in sortedDates)
                    {
                        comboBoxSelectDate.Items.Add(date);
                    }
                }

                // Set default selection to "All Dates"
                comboBoxSelectDate.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error populating date combo box: " + ex.Message);
            }
        }

        private bool isLoading = false;

        private async void LoadFirebaseAttendanceData(DateTime? selectedDate = null)
        {
            // Prevent multiple simultaneous loads
            if (isLoading) return;

            isLoading = true;

            try
            {
                dataGridViewAttendance.Rows.Clear();
                dataGridViewAttendance.Refresh();

                // Rest of your loading code remains the same...
                // Show loading indicator
                Cursor.Current = Cursors.WaitCursor;

                // Load employee data
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                // Try to load attendance data
                try
                {
                    // Get the entire Attendance array
                    var attendanceData = await firebase.Child("Attendance").OnceSingleAsync<JArray>();

                    if (attendanceData == null || attendanceData.Count == 0)
                    {
                        MessageBox.Show("No attendance records found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int counter = 1;
                    foreach (var attendanceItem in attendanceData)
                    {
                        if (attendanceItem != null && attendanceItem.Type != JTokenType.Null)
                        {
                            bool recordAdded = ProcessAttendanceRecord(attendanceItem, employeeDict, selectedDate, counter);
                            if (recordAdded)
                            {
                                counter++;
                            }
                        }
                    }

                    // Update the status label based on filtering
                    if (selectedDate.HasValue)
                    {
                        labelAttendanceDate.Text = $"Attendance for {selectedDate.Value.ToString("yyyy-MM-dd")}";
                    }
                    else
                    {
                        labelAttendanceDate.Text = "All Attendance Records";
                    }
                }
                catch (FirebaseException fex)
                {
                    if (fex.ResponseData.Contains("404") || fex.ResponseData.Contains("null"))
                    {
                        MessageBox.Show("Attendance database is empty or doesn't exist yet.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Firebase error: {fex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private bool ProcessAttendanceRecord(JToken attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, int counter)
        {
            try
            {
                // Extract values from JToken
                string timeInStr = attendance["time_in"]?.ToString() ?? "";
                string timeOutStr = attendance["time_out"]?.ToString() ?? "";
                string attendanceDateStr = attendance["attendance_date"]?.ToString() ?? "";

                // Debug: Check what dates we're working with
                Console.WriteLine($"Attendance Date: {attendanceDateStr}, TimeIn: {timeInStr}, TimeOut: {timeOutStr}");

                // Apply date filter if a date is selected
                if (selectedDate.HasValue)
                {
                    bool shouldInclude = false;

                    // Check attendance_date first
                    if (!string.IsNullOrEmpty(attendanceDateStr) && DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        if (attendanceDate.Date == selectedDate.Value.Date)
                        {
                            shouldInclude = true;
                            Console.WriteLine($"Included by attendance_date: {attendanceDate.Date}");
                        }
                    }

                    // Check time_in date if not already included
                    if (!shouldInclude && !string.IsNullOrEmpty(timeInStr) && DateTime.TryParse(timeInStr, out DateTime timeInDate))
                    {
                        if (timeInDate.Date == selectedDate.Value.Date)
                        {
                            shouldInclude = true;
                            Console.WriteLine($"Included by time_in: {timeInDate.Date}");
                        }
                    }

                    // Check time_out date if not already included
                    if (!shouldInclude && !string.IsNullOrEmpty(timeOutStr) && DateTime.TryParse(timeOutStr, out DateTime timeOutDate))
                    {
                        if (timeOutDate.Date == selectedDate.Value.Date)
                        {
                            shouldInclude = true;
                            Console.WriteLine($"Included by time_out: {timeOutDate.Date}");
                        }
                    }

                    if (!shouldInclude)
                    {
                        Console.WriteLine($"Record excluded - no date matches selected date: {selectedDate.Value.Date}");
                        return false;
                    }
                }

                // Extract other fields
                string employeeId = attendance["employee_id"]?.ToString() ?? "N/A";
                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);
                string hoursWorked = attendance["hours_worked"]?.ToString() ?? "0.00";
                string status = attendance["status"]?.ToString() ?? "Absent";
                string verification = attendance["verification_method"]?.ToString() ?? "Manual";
                string scheduleId = attendance["schedule_id"]?.ToString() ?? "";
                string overtimeHoursStr = attendance["overtime_hours"]?.ToString() ?? "0.00"; // Get overtime_hours directly

                // Use the fixed CalculateOvertimeHours method
                string overtime = CalculateOvertimeHours(overtimeHoursStr, hoursWorked, employeeId, scheduleId);

                // Adjust hours worked if needed
                if (double.TryParse(hoursWorked, out double hw) && double.TryParse(overtime, out double ot))
                {
                    hoursWorked = Math.Round(hw - ot, 2).ToString("0.00");
                }

                string fullName = "N/A";
                if (employeeDict.ContainsKey(employeeId))
                {
                    var employee = employeeDict[employeeId];
                    fullName = $"{employee.first_name} {employee.middle_name} {employee.last_name}".Trim();
                }

                // Add the row to the DataGridView
                if (dataGridViewAttendance.InvokeRequired)
                {
                    dataGridViewAttendance.Invoke((MethodInvoker)delegate
                    {
                        dataGridViewAttendance.Rows.Add(
                            counter,
                            employeeId,
                            fullName,
                            timeIn,
                            timeOut,
                            hoursWorked,
                            status,
                            overtime,
                            verification,
                            Properties.Resources.VerticalThreeDots
                        );
                    });
                }
                else
                {
                    dataGridViewAttendance.Rows.Add(
                        counter,
                        employeeId,
                        fullName,
                        timeIn,
                        timeOut,
                        hoursWorked,
                        status,
                        overtime,
                        verification,
                        Properties.Resources.VerticalThreeDots
                    );
                }

                Console.WriteLine($"Record added for employee {employeeId} on date {attendanceDateStr}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing attendance record: {ex.Message}");
                return false;
            }
        }

        private List<Dictionary<string, string>> ParseMalformedAttendanceJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();
            try
            {
                string cleanedJson = rawJson.Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"").Replace("(", "[").Replace(")", "]")
                    .Replace("[null,", "[").Replace("], [", ",").Replace("}, {", "},{")
                    .Replace("},(", "},{").Replace("),{", "},{");

                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");
                foreach (Match match in matches)
                {
                    try
                    {
                        var record = new Dictionary<string, string>();
                        string objectStr = match.Value;
                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(""[^""]*""|\d+\.?\d*|true|false|null)");
                        foreach (Match kvpMatch in kvpMatches)
                        {
                            if (kvpMatch.Groups.Count >= 3)
                            {
                                string key = kvpMatch.Groups[1].Value;
                                string value = kvpMatch.Groups[2].Value.Trim('"');
                                record[key] = value;
                            }
                        }
                        records.Add(record);
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }

            return records;
        }

        private void ProcessManualRecord(Dictionary<string, string> record, Dictionary<string, dynamic> employeeDict, int counter)
        {
            try
            {
                string employeeId = record.ContainsKey("employee_id") ? record["employee_id"] : "N/A";
                string timeIn = FormatFirebaseTime(record.ContainsKey("time_in") ? record["time_in"] : "N/A");
                string timeOut = FormatFirebaseTime(record.ContainsKey("time_out") ? record["time_out"] : "N/A");
                string hoursWorked = record.ContainsKey("hours_worked") ? record["hours_worked"] : "0.00";
                string status = record.ContainsKey("status") ? record["status"] : "Absent";
                string verification = record.ContainsKey("verification_method") ? record["verification_method"] : "Manual";
                string scheduleId = record.ContainsKey("schedule_id") ? record["schedule_id"] : "";
                string overtimeHoursStr = record.ContainsKey("overtime_hours") ? record["overtime_hours"] : "0.00";

                string overtime = CalculateOvertimeHours(overtimeHoursStr, hoursWorked, employeeId, scheduleId);

                if (double.TryParse(hoursWorked, out double hw) && double.TryParse(overtime, out double ot))
                {
                    hoursWorked = Math.Round(hw - ot, 2).ToString("0.00");
                }

                string fullName = "N/A";
                if (employeeDict.ContainsKey(employeeId))
                {
                    var employee = employeeDict[employeeId];
                    fullName = $"{employee.first_name} {employee.middle_name} {employee.last_name}".Trim();
                }

                if (dataGridViewAttendance.InvokeRequired)
                {
                    dataGridViewAttendance.Invoke((MethodInvoker)delegate
                    {
                        dataGridViewAttendance.Rows.Add(
                            counter,
                            employeeId,
                            fullName,
                            timeIn,
                            timeOut,
                            hoursWorked,
                            status,
                            overtime,
                            verification,
                            Properties.Resources.VerticalThreeDots
                        );
                    });
                }
                else
                {
                    dataGridViewAttendance.Rows.Add(
                        counter,
                        employeeId,
                        fullName,
                        timeIn,
                        timeOut,
                        hoursWorked,
                        status,
                        overtime,
                        verification,
                        Properties.Resources.VerticalThreeDots
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing manual record: " + ex.Message);
            }
        }
        private async void DebugFirebaseData()
        {
            try
            {
                // Test if we can access the data
                var testData = await firebase.Child("Attendance").OnceSingleAsync<JArray>();
                if (testData != null)
                {
                    Console.WriteLine($"Found {testData.Count} attendance records");
                    foreach (var item in testData)
                    {
                        if (item != null && item.Type != JTokenType.Null)
                        {
                            Console.WriteLine($"Record: {item}");
                            string empId = item["employee_id"]?.ToString() ?? "null";
                            string date = item["attendance_date"]?.ToString() ?? "null";
                            string overtime = item["overtime_hours"]?.ToString() ?? "null";
                            Console.WriteLine($"Employee: {empId}, Date: {date}, Overtime: {overtime}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No attendance data found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug error: {ex.Message}");
            }
        }
    }
}