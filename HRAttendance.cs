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
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class HRAttendance : UserControl
    {
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        private AttendanceFilterCriteria currentAttendanceFilters = new AttendanceFilterCriteria();
        private Dictionary<string, (string Department, string Position)> employeeDepartmentMap = new Dictionary<string, (string Department, string Position)>();
        private Dictionary<int, string> attendanceKeyMap = new Dictionary<int, string>();
        private Dictionary<string, List<EmployeeSchedule>> employeeSchedules = new Dictionary<string, List<EmployeeSchedule>>();

        private bool isLoading = false;

        public HRAttendance()
        {
            InitializeComponent();

            comboBoxSelectDate.SelectedIndexChanged -= comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged -= textBoxSearchEmployee_TextChanged;

            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
            comboBoxSelectDate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSelectDate.IntegralHeight = false;
            comboBoxSelectDate.MaxDropDownItems = 5;
            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
            LoadDataAsync();
            LoadEmployeeSchedules();
        }

        private async void LoadDataAsync()
        {
            currentAttendanceFilters = new AttendanceFilterCriteria();

            // Load employee department mapping first
            await LoadEmployeeDepartmentMappingAsync();

            // Then populate dates which will trigger data loading with today's date
            PopulateDateComboBox();
        }

        private async Task LoadEmployeeDepartmentMappingAsync()
        {
            try
            {
                employeeDepartmentMap.Clear();
                var employmentDict = await TryGetEmploymentInfoByIndex();
                if (employmentDict.Count == 0)
                    employmentDict = await TryManualEmploymentInfoParsing();
                employeeDepartmentMap = employmentDict;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load employee department mapping: {ex.Message}");
            }
        }



        // 🔹 FILTER HANDLERS
        private void ApplyAttendanceFilters(AttendanceFilterCriteria filters)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyAttendanceFilters called with SortBy: '{filters?.SortBy}'");
            currentAttendanceFilters = filters ?? new AttendanceFilterCriteria();
            ApplyAllAttendanceFilters();
        }

        private void ResetAttendanceFilters()
        {
            currentAttendanceFilters = new AttendanceFilterCriteria();
            ApplyAllAttendanceFilters();
        }

        private void ApplyAllAttendanceFilters()
        {
            string searchText = textBoxSearchEmployee.Text.Trim().ToLower();
            var filteredRows = new List<DataGridViewRow>();

            foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
            {
                if (!row.IsNewRow)
                {
                    bool matchesSearch = MatchesSearchText(row, searchText);
                    bool matchesFilters = MatchesAttendanceFilters(row);

                    if (matchesSearch && matchesFilters)
                        filteredRows.Add(row);

                    row.Visible = false;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Filtered rows count: {filteredRows.Count}");

            // IMPORTANT: Check if we have a sort criteria before calling ApplySorting
            if (!string.IsNullOrEmpty(currentAttendanceFilters?.SortBy))
            {
                System.Diagnostics.Debug.WriteLine($"Calling ApplySorting with: {currentAttendanceFilters.SortBy}");
                var sortedRows = ApplySorting(filteredRows);
                System.Diagnostics.Debug.WriteLine($"After sorting, rows count: {sortedRows.Count}");

                foreach (var row in sortedRows)
                    row.Visible = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No sorting applied - SortBy is empty or null");
                foreach (var row in filteredRows)
                    row.Visible = true;
            }

            UpdateRowNumbers();
        }

        // 🔹 SORTING
        // 🔹 SORTING - FIXED TO USE EXISTING EXTRACTFIRSTNAME METHOD
        private List<DataGridViewRow> ApplySorting(List<DataGridViewRow> rows)
        {
            System.Diagnostics.Debug.WriteLine($"ApplySorting called with {rows.Count} rows");

            if (currentAttendanceFilters == null || string.IsNullOrEmpty(currentAttendanceFilters.SortBy))
            {
                System.Diagnostics.Debug.WriteLine("No sort criteria specified");
                return rows;
            }

            string sort = currentAttendanceFilters.SortBy.ToLower().Trim();
            System.Diagnostics.Debug.WriteLine($"Sorting by: '{sort}'");

            try
            {
                switch (sort)
                {
                    case "a-z":
                        System.Diagnostics.Debug.WriteLine("Applying A-Z sort by first name");
                        return rows.OrderBy(r =>
                        {
                            string fullName = r.Cells["FullName"]?.Value?.ToString() ?? "";
                            return ExtractFirstName(fullName); // Use your existing method
                        }, StringComparer.OrdinalIgnoreCase).ToList();

                    case "z-a":
                        System.Diagnostics.Debug.WriteLine("Applying Z-A sort by first name");
                        return rows.OrderByDescending(r =>
                        {
                            string fullName = r.Cells["FullName"]?.Value?.ToString() ?? "";
                            return ExtractFirstName(fullName); // Use your existing method
                        }, StringComparer.OrdinalIgnoreCase).ToList();

                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown sort option: {sort}");
                        return rows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ApplySorting: {ex.Message}");
                return rows;
            }
        }

        private DateTime GetAttendanceDateForSorting(string timeIn, string timeOut)
        {
            // Try to parse the date from timeIn first
            if (!string.IsNullOrEmpty(timeIn) && timeIn != "N/A")
            {
                if (DateTime.TryParse(timeIn, out DateTime timeInDate))
                    return timeInDate;

                // Try parsing with time-only formats
                if (DateTime.TryParseExact(timeIn, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out timeInDate))
                    return DateTime.Today.AddHours(timeInDate.Hour).AddMinutes(timeInDate.Minute);
            }

            // If timeIn fails, try timeOut
            if (!string.IsNullOrEmpty(timeOut) && timeOut != "N/A")
            {
                if (DateTime.TryParse(timeOut, out DateTime timeOutDate))
                    return timeOutDate;

                if (DateTime.TryParseExact(timeOut, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out timeOutDate))
                    return DateTime.Today.AddHours(timeOutDate.Hour).AddMinutes(timeOutDate.Minute);
            }

            return DateTime.MinValue;
        }
        private async Task LoadEmployeeSchedules()
        {
            try
            {
                employeeSchedules.Clear();

                var scheduleRecords = await firebase.Child("Work_Schedule").OnceAsync<EmployeeSchedule>();

                foreach (var record in scheduleRecords)
                {
                    if (record?.Object != null && !string.IsNullOrEmpty(record.Object.employee_id))
                    {
                        string employeeId = record.Object.employee_id;

                        if (!employeeSchedules.ContainsKey(employeeId))
                        {
                            employeeSchedules[employeeId] = new List<EmployeeSchedule>();
                        }

                        employeeSchedules[employeeId].Add(record.Object);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"HR: Loaded schedules for {employeeSchedules.Count} employees");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HR: Error loading schedules: {ex.Message}");
            }
        }

        private string ExtractFirstName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "";

            string[] nameParts = fullName.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string firstName = nameParts.Length > 0 ? nameParts[0] : "";
            System.Diagnostics.Debug.WriteLine($"ExtractFirstName: '{fullName}' -> '{firstName}'");
            return firstName;
        }

        // Helper method to get attendance date for sorting
        private DateTime GetAttendanceDate(string timeIn, string timeOut)
        {
            if (!string.IsNullOrEmpty(timeIn) && timeIn != "N/A")
            {
                if (DateTime.TryParse(timeIn, out DateTime timeInDate))
                {
                    System.Diagnostics.Debug.WriteLine($"GetAttendanceDate from timeIn: {timeInDate.Date}");
                    return timeInDate.Date;
                }
            }

            if (!string.IsNullOrEmpty(timeOut) && timeOut != "N/A")
            {
                if (DateTime.TryParse(timeOut, out DateTime timeOutDate))
                {
                    System.Diagnostics.Debug.WriteLine($"GetAttendanceDate from timeOut: {timeOutDate.Date}");
                    return timeOutDate.Date;
                }
            }

            System.Diagnostics.Debug.WriteLine("GetAttendanceDate: returning DateTime.MinValue");
            return DateTime.MinValue;
        }

        private DateTime GetAttendanceDateTime(string timeIn, string timeOut)
        {
            if (DateTime.TryParse(timeIn, out DateTime tIn)) return tIn;
            if (DateTime.TryParse(timeOut, out DateTime tOut)) return tOut;
            return DateTime.MinValue;
        }

        private void UpdateRowNumbers()
        {
            int counter = 1;
            foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    row.Cells["RowNumber"].Value = counter;
                    counter++;
                }
            }
        }

        // 🔹 SEARCH / FILTER LOGIC
        private bool MatchesSearchText(DataGridViewRow row, string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || searchText == "find employee")
                return true;

            string employeeId = row.Cells["EmployeeId"].Value?.ToString()?.ToLower() ?? "";
            string fullName = row.Cells["FullName"].Value?.ToString()?.ToLower() ?? "";
            string status = row.Cells["Status"].Value?.ToString()?.ToLower() ?? "";

            return employeeId.Contains(searchText) ||
                   fullName.Contains(searchText) ||
                   status.Contains(searchText);
        }

        private bool MatchesAttendanceFilters(DataGridViewRow row)
        {
            string employeeId = row.Cells["EmployeeId"].Value?.ToString() ?? "";
            string fullName = row.Cells["FullName"].Value?.ToString() ?? "";

            // Employee ID filter
            if (!string.IsNullOrEmpty(currentAttendanceFilters.EmployeeId) &&
                currentAttendanceFilters.EmployeeId.Trim().ToLower() != "search id")
            {
                if (!employeeId.ToLower().Contains(currentAttendanceFilters.EmployeeId.ToLower()))
                    return false;
            }

            // Name filter
            if (!string.IsNullOrEmpty(currentAttendanceFilters.Name) &&
                currentAttendanceFilters.Name.Trim().ToLower() != "search name")
            {
                if (!fullName.ToLower().Contains(currentAttendanceFilters.Name.ToLower()))
                    return false;
            }

            // Department filter
            if (!string.IsNullOrEmpty(currentAttendanceFilters.Department) &&
                !currentAttendanceFilters.Department.Equals("Select department", StringComparison.OrdinalIgnoreCase))
            {
                if (employeeDepartmentMap.ContainsKey(employeeId))
                {
                    string empDepartment = employeeDepartmentMap[employeeId].Department;
                    if (string.IsNullOrEmpty(empDepartment) ||
                        !empDepartment.Equals(currentAttendanceFilters.Department, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else return false;
            }

            // Position filter
            if (!string.IsNullOrEmpty(currentAttendanceFilters.Position) &&
                !currentAttendanceFilters.Position.Equals("Select position", StringComparison.OrdinalIgnoreCase))
            {
                if (employeeDepartmentMap.ContainsKey(employeeId))
                {
                    string empPosition = employeeDepartmentMap[employeeId].Position;
                    if (string.IsNullOrEmpty(empPosition) ||
                        !empPosition.Equals(currentAttendanceFilters.Position, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else return false;
            }

            // Status filters - UPDATED TO MATCH AdminAttendance
            if (currentAttendanceFilters.StatusPresent || currentAttendanceFilters.StatusAbsent ||
                currentAttendanceFilters.StatusLate || currentAttendanceFilters.StatusEarlyOut)
            {
                bool statusMatch = false;
                string status = row.Cells["Status"].Value?.ToString() ?? "";

                if (currentAttendanceFilters.StatusPresent && status.Equals("On Time", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusAbsent && status.Equals("Absent", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusLate && status.Equals("Late", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusEarlyOut && status.Equals("Early Out", StringComparison.OrdinalIgnoreCase)) statusMatch = true;

                // Add support for "Late & Early Out" - matches both Late and Early Out filters
                if (currentAttendanceFilters.StatusLate && status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusEarlyOut && status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase)) statusMatch = true;

                if (!statusMatch) return false;
            }

            // Hours worked filters
            if (currentAttendanceFilters.HoursEight || currentAttendanceFilters.HoursBelowEight)
            {
                bool hoursMatch = false;
                string hoursWorkedStr = row.Cells["HoursWorked"].Value?.ToString() ?? "0";

                if (double.TryParse(hoursWorkedStr, out double hoursWorked))
                {
                    if (currentAttendanceFilters.HoursEight && Math.Abs(hoursWorked - 8.0) < 0.1)
                        hoursMatch = true;
                    if (currentAttendanceFilters.HoursBelowEight && hoursWorked < 8.0)
                        hoursMatch = true;
                }

                if (!hoursMatch) return false;
            }

            // Overtime filters
            if (currentAttendanceFilters.OvertimeOneHour || currentAttendanceFilters.OvertimeTwoHoursPlus)
            {
                bool overtimeMatch = false;
                string overtimeStr = row.Cells["OvertimeHours"].Value?.ToString() ?? "0";

                if (double.TryParse(overtimeStr, out double overtime))
                {
                    if (currentAttendanceFilters.OvertimeOneHour && Math.Abs(overtime - 1.0) < 0.1)
                        overtimeMatch = true;
                    if (currentAttendanceFilters.OvertimeTwoHoursPlus && overtime >= 2.0)
                        overtimeMatch = true;
                }

                if (!overtimeMatch) return false;
            }

            return true;
        }

        // 🔹 STATUS + OVERTIME - UPDATED TO MATCH ADMINATTENDANCE LOGIC
        // 🔹 STATUS + OVERTIME - UPDATED TO PRESERVE "DAY OFF" LIKE ADMINATTENDANCE
        private string CalculateStatus(string timeInStr, string timeOutStr, string employeeId, string attendanceDateStr, string existingStatus = "")
        {
            // If the existing status is already "Day Off", preserve it
            if (!string.IsNullOrEmpty(existingStatus) && existingStatus.Equals("Day Off", StringComparison.OrdinalIgnoreCase))
                return "Day Off";

            // Check if employee has schedule for this day
            if (!HasScheduleForDate(employeeId, attendanceDateStr))
            {
                return "Day Off";
            }

            // If time in is N/A and employee should be working, mark as Absent
            if (string.IsNullOrEmpty(timeInStr) || timeInStr == "N/A")
            {
                // If we have an existing status that's not Day Off, use it
                if (!string.IsNullOrEmpty(existingStatus) && !existingStatus.Equals("Day Off", StringComparison.OrdinalIgnoreCase))
                    return existingStatus;
                return "Absent";
            }

            try
            {
                // Get schedule for this employee and date
                var schedule = GetScheduleForDate(employeeId, attendanceDateStr);
                if (schedule == null)
                {
                    return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
                }

                DateTime timeIn, timeOut;
                DateTime expectedStart, expectedEnd;

                // Parse expected times from schedule (not hardcoded)
                if (!DateTime.TryParse(schedule.start_time, out expectedStart) ||
                    !DateTime.TryParse(schedule.end_time, out expectedEnd))
                {
                    return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "On Time";
                }

                // Try to parse actual times
                bool timeInParsed = DateTime.TryParseExact(timeInStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInStr, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParse(timeInStr, out timeIn);

                bool timeOutParsed = DateTime.TryParseExact(timeOutStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutStr, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParse(timeOutStr, out timeOut);

                if (timeInParsed)
                {
                    // Create comparable times using schedule times instead of hardcoded 8-5
                    DateTime actualTimeIn = DateTime.Today.AddHours(timeIn.Hour).AddMinutes(timeIn.Minute);
                    DateTime scheduleStart = DateTime.Today.AddHours(expectedStart.Hour).AddMinutes(expectedStart.Minute);
                    DateTime scheduleEnd = DateTime.Today.AddHours(expectedEnd.Hour).AddMinutes(expectedEnd.Minute);

                    bool isLate = actualTimeIn > scheduleStart.AddMinutes(15); // 15 minutes grace period
                    bool isEarlyOut = false;

                    // Check for early out only if we have valid time out
                    if (timeOutParsed && timeOutStr != "N/A" && !string.IsNullOrEmpty(timeOutStr))
                    {
                        DateTime actualTimeOut = DateTime.Today.AddHours(timeOut.Hour).AddMinutes(timeOut.Minute);
                        isEarlyOut = actualTimeOut < scheduleEnd.AddMinutes(-15); // 15 minutes early out threshold
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

        private string CalculateOvertimeHours(string overtimeStr, string hoursWorkedStr, string employeeId, string attendanceDateStr)
        {
            // First try to use the overtime_hours field directly from Firebase
            if (!string.IsNullOrEmpty(overtimeStr) && double.TryParse(overtimeStr, out double overtimeHours))
            {
                return Math.Round(overtimeHours, 2).ToString("0.00");
            }

            // Fallback calculation using schedule hours
            double regularHours = 8.0; // Default fallback

            // Try to get actual scheduled hours
            var schedule = GetScheduleForDate(employeeId, attendanceDateStr);
            if (schedule != null)
            {
                DateTime startTime, endTime;
                if (DateTime.TryParse(schedule.start_time, out startTime) &&
                    DateTime.TryParse(schedule.end_time, out endTime))
                {
                    regularHours = (endTime - startTime).TotalHours;
                }
            }

            if (string.IsNullOrEmpty(hoursWorkedStr) || hoursWorkedStr == "N/A" || !double.TryParse(hoursWorkedStr, out double workedHours))
                return "0.00";

            double overtime = Math.Max(0, workedHours - regularHours);
            return Math.Round(overtime, 2).ToString("0.00");
        }

        // 🔹 LOAD EMPLOYEE DEPARTMENT MAPPING (same as before)
        private async void LoadEmployeeDepartmentMapping()
        {
            try
            {
                employeeDepartmentMap.Clear();
                var employmentDict = await TryGetEmploymentInfoByIndex();
                if (employmentDict.Count == 0)
                    employmentDict = await TryManualEmploymentInfoParsing();
                employeeDepartmentMap = employmentDict;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load employee department mapping: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, (string Department, string Position)>> TryGetEmploymentInfoByIndex()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();
            try
            {
                for (int i = 1; i <= 20; i++) // Increased from 20 to match AdminAttendance
                {
                    try
                    {
                        var record = await firebase
                            .Child("EmploymentInfo")
                            .Child(i.ToString())
                            .OnceSingleAsync<Dictionary<string, object>>();

                        if (record != null)
                        {
                            string empId = GetValue(record, "employee_id");
                            if (!string.IsNullOrEmpty(empId))
                            {
                                string dept = GetValue(record, "department");
                                string pos = GetValue(record, "position");
                                employmentDict[empId] = (dept, pos);
                            }
                        }
                        else break;
                    }
                    catch
                    {
                        // If we get consecutive failures, break
                        if (i > 5 && employmentDict.Count == 0)
                            break;
                    }
                }
            }
            catch { }
            return employmentDict;
        }

        private async Task<Dictionary<string, (string Department, string Position)>> TryManualEmploymentInfoParsing()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();
            try
            {
                var response = await firebase.Child("EmploymentInfo").OnceAsync<object>();
                if (response == null || !response.Any()) return employmentDict;

                string rawData = response.First()?.Object?.ToString() ?? "";
                if (string.IsNullOrEmpty(rawData)) return employmentDict;

                var pattern = @"employee_id['""]?:['""]([^'""]+)['""][^}]*?department['""]?:['""]([^'""]*)[^}]*?position['""]?:['""]([^'""]*)";
                var matches = Regex.Matches(rawData, pattern, RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 4)
                    {
                        string empId = match.Groups[1].Value.Trim();
                        string dept = match.Groups[2].Value.Trim();
                        string pos = match.Groups[3].Value.Trim();

                        if (!string.IsNullOrEmpty(empId))
                            employmentDict[empId] = (dept, pos);
                    }
                }
            }
            catch { }
            return employmentDict;
        }

        private string GetValue(Dictionary<string, object> data, string key)
        {
            if (data == null) return "";
            if (!data.ContainsKey(key)) return "";
            var value = data[key];
            if (value == null) return "";

            if (value is string s) return s;
            if (value is JValue jv) return jv.Value?.ToString() ?? "";
            if (value is JToken jt) return jt.ToString();
            return value.ToString();
        }

        // 🔹 EVENT HANDLERS
        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e) => ApplyAllAttendanceFilters();

        // FIXED: Remove duplicate event subscription
        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminAttendance filterAdminAttendanceForm = new FilterAdminAttendance();
            filterAdminAttendanceForm.FiltersApplied += ApplyAttendanceFilters;
            filterAdminAttendanceForm.FiltersReset += ResetAttendanceFilters;
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

            // Only load data for the selected date
            LoadFirebaseAttendanceData(selectedDate);
        }

        // 🔹 DATAGRIDVIEW SETUP
        private void setDataGridViewAttributes()
        {
            dataGridViewAttendance.ReadOnly = true;
            dataGridViewAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAttendance.MultiSelect = false;
            dataGridViewAttendance.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewAttendance.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewAttendance.GridColor = Color.White;
            dataGridViewAttendance.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAttendance.ColumnHeadersHeight = 40;
            dataGridViewAttendance.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAttendance.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);


            dataGridViewAttendance.CellFormatting += dataGridView1_CellFormatting;
            dataGridViewAttendance.CellMouseEnter += dataGridViewAttendance_CellMouseEnter;
            dataGridViewAttendance.CellMouseLeave += dataGridViewAttendance_CellMouseLeave;

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

            // ADD HIDDEN COLUMN FOR ATTENDANCE DATE
            dataGridViewAttendance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AttendanceDate",
                HeaderText = "Attendance Date",
                Visible = false,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 80
            });

            dataGridViewAttendance.Columns.Add(new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25,
                Image = Properties.Resources.VerticalThreeDots
            });

            // Add cell click event for action column
            dataGridViewAttendance.CellClick += dataGridViewAttendance_CellClick;
        }

        // UPDATED CELL FORMATTING TO MATCH ADMINATTENDANCE
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "On Time":
                        e.CellStyle.BackColor = Color.FromArgb(95, 218, 71);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Late":
                    case "Early Out":
                    case "Late & Early Out":  // Added this line
                        e.CellStyle.BackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Absent":
                        e.CellStyle.BackColor = Color.FromArgb(221, 60, 60);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Leave":
                        e.CellStyle.BackColor = Color.FromArgb(71, 93, 218);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Day Off":
                        e.CellStyle.BackColor = Color.FromArgb(180, 174, 189);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                }
                e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }
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
                // Get the selected row data
                DataGridViewRow selectedRow = dataGridViewAttendance.Rows[e.RowIndex];

                // Get the Firebase key for this record
                string firebaseKey = null;
                if (attendanceKeyMap.ContainsKey(e.RowIndex))
                {
                    firebaseKey = attendanceKeyMap[e.RowIndex];
                }

                // GET ATTENDANCE DATE FROM HIDDEN COLUMN
                string attendanceDate = selectedRow.Cells["AttendanceDate"].Value?.ToString();

                Form parentForm = this.FindForm();
                EditAttendance editAttendanceForm = new EditAttendance();

                // Pass the attendance data AND the Firebase key AND attendance date to the edit form
                editAttendanceForm.SetAttendanceData(
                    selectedRow.Cells["EmployeeId"].Value?.ToString(),
                    selectedRow.Cells["FullName"].Value?.ToString(),
                    selectedRow.Cells["TimeIn"].Value?.ToString(),
                    selectedRow.Cells["TimeOut"].Value?.ToString(),
                    selectedRow.Cells["HoursWorked"].Value?.ToString(),
                    selectedRow.Cells["Status"].Value?.ToString(),
                    selectedRow.Cells["OvertimeHours"].Value?.ToString(),
                    selectedRow.Cells["VerificationMethod"].Value?.ToString(),
                    firebaseKey,
                    attendanceDate  // PASS THE ATTENDANCE DATE
                );

                AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);

                // Refresh data after editing
                editAttendanceForm.FormClosed += (s, args) => {
                    if (editAttendanceForm.DataUpdated)
                    {
                        // Refresh with current filter
                        string selectedText = comboBoxSelectDate.SelectedItem?.ToString();
                        DateTime? selectedDate = null;

                        if (!string.IsNullOrEmpty(selectedText) && selectedText != "All Dates")
                        {
                            DateTime.TryParseExact(selectedText, "yyyy-MM-dd",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime parsedDate);
                            selectedDate = parsedDate;
                        }

                        LoadFirebaseAttendanceData(selectedDate);
                    }
                };
            }
        }

        private async void PopulateDateComboBox()
        {
            try
            {
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates");

                // Get all attendance records
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

                // Add fallback dates if none found
                if (uniqueDates.Count == 0)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        uniqueDates.Add(DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"));
                    }
                }

                // Add dates to combo box in descending order
                var sortedDates = uniqueDates.Select(d => DateTime.Parse(d))
                                     .OrderByDescending(d => d)
                                     .Select(d => d.ToString("yyyy-MM-dd"))
                                     .ToList();

                foreach (var date in sortedDates)
                {
                    comboBoxSelectDate.Items.Add(date);
                }

                // Set today's date as default
                string todayString = DateTime.Today.ToString("yyyy-MM-dd");

                if (comboBoxSelectDate.Items.Contains(todayString))
                {
                    comboBoxSelectDate.SelectedItem = todayString;
                    LoadFirebaseAttendanceData(DateTime.Today);
                }
                else if (comboBoxSelectDate.Items.Count > 1)
                {
                    comboBoxSelectDate.SelectedIndex = 1;
                    string firstDate = comboBoxSelectDate.SelectedItem.ToString();
                    LoadFirebaseAttendanceData(DateTime.Parse(firstDate));
                }
                else
                {
                    comboBoxSelectDate.SelectedIndex = 0;
                    LoadFirebaseAttendanceData(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error populating date combo box: " + ex.Message);

                // Simple fallback
                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates");
                comboBoxSelectDate.Items.Add(DateTime.Today.ToString("yyyy-MM-dd"));
                comboBoxSelectDate.SelectedIndex = 1;
                LoadFirebaseAttendanceData(DateTime.Today);
            }
        }


        // Add this method from AdminAttendance
        private string ExtractDateFromAnyRecordType(dynamic record, string recordKey)
        {
            try
            {
                // Method 1: Try direct property access
                try
                {
                    if (record is IDictionary<string, object> dict)
                    {
                        if (dict.ContainsKey("attendance_date") && dict["attendance_date"] != null)
                        {
                            string dateStr = dict["attendance_date"].ToString();
                            if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                            {
                                return dateStr;
                            }
                        }
                    }
                }
                catch { }

                // Method 2: Try JSON approach
                try
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(record);
                    var jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (jsonDict != null)
                    {
                        string[] possibleDateFields = { "attendance_date", "date", "attendanceDate", "AttendanceDate" };
                        foreach (string field in possibleDateFields)
                        {
                            if (jsonDict.ContainsKey(field) && jsonDict[field] != null)
                            {
                                string dateStr = jsonDict[field].ToString();
                                if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                                {
                                    return dateStr;
                                }
                            }
                        }
                    }
                }
                catch { }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting date for key {recordKey}: {ex.Message}");
                return null;
            }
        }

        private async void LoadFirebaseAttendanceData(DateTime? selectedDate = null)
        {
            if (isLoading) return;
            isLoading = true;

            try
            {
                // Clear grid using your existing method
                if (dataGridViewAttendance.InvokeRequired)
                {
                    dataGridViewAttendance.Invoke((MethodInvoker)delegate
                    {
                        dataGridViewAttendance.Rows.Clear();
                        attendanceKeyMap.Clear();
                    });
                }
                else
                {
                    dataGridViewAttendance.Rows.Clear();
                    attendanceKeyMap.Clear();
                }

                Cursor.Current = Cursors.WaitCursor;

                // Load employee data (use your existing method)
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                // KEY: Only process records for the selected date
                var allAttendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();
                var recordsToProcess = new List<(dynamic record, string key)>();

                foreach (var attendanceRecord in allAttendanceRecords)
                {
                    if (attendanceRecord?.Object != null)
                    {
                        string firebaseKey = attendanceRecord.Key;
                        string recordDateStr = ExtractDateFromAnyRecordType(attendanceRecord.Object, firebaseKey);

                        if (selectedDate.HasValue)
                        {
                            // Only include records that match the selected date
                            if (!string.IsNullOrEmpty(recordDateStr) &&
                                DateTime.TryParse(recordDateStr, out DateTime recordDate))
                            {
                                if (recordDate.Date == selectedDate.Value.Date)
                                {
                                    recordsToProcess.Add((attendanceRecord.Object, firebaseKey));
                                }
                            }
                        }
                        else
                        {
                            // If no date selected (All Dates), include all records
                            recordsToProcess.Add((attendanceRecord.Object, firebaseKey));
                        }
                    }
                }

                // Process only the filtered records using your existing ProcessAttendanceRecord method
                int counter = 1;
                foreach (var (record, key) in recordsToProcess)
                {
                    // Use your existing ProcessAttendanceRecord method - it will work as-is
                    // since we're only passing records that match the date filter
                    ProcessAttendanceRecord(record, employeeDict, selectedDate, counter, counter - 1, key);
                    counter++;
                }

                // Update UI
                UpdateStatusLabel(selectedDate);
                ApplyAllAttendanceFilters(); // Use your existing filter method
                dataGridViewAttendance.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadFirebaseAttendanceData: {ex.Message}");
                MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                Cursor.Current = Cursors.Default;
            }
        }



        private bool ProcessAttendanceRecord(dynamic attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, int counter, int rowIndex, string firebaseKey)
        {
            try
            {

                System.Diagnostics.Debug.WriteLine($"HR: === Processing record {firebaseKey} ===");

                // Handle the dynamic data more carefully
                Dictionary<string, object> attendanceDict = new Dictionary<string, object>();

                if (attendance == null)
                {
                    System.Diagnostics.Debug.WriteLine($"HR: Record {firebaseKey} is NULL");
                    return false;
                }

                // Try multiple approaches to extract the data
                try
                {
                    // Approach 1: Direct cast to dictionary
                    if (attendance is IDictionary<string, object> directDict)
                    {
                        attendanceDict = new Dictionary<string, object>(directDict);
                        System.Diagnostics.Debug.WriteLine("HR: Used direct dictionary cast");
                    }
                    // Approach 2: Use Newtonsoft.Json to convert
                    else
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(attendance);
                        attendanceDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        System.Diagnostics.Debug.WriteLine("HR: Used JSON serialization approach");
                    }
                }
                catch (Exception serializationEx)
                {
                    System.Diagnostics.Debug.WriteLine($"HR: Serialization failed: {serializationEx.Message}");
                    return false;
                }

                if (attendanceDict == null || !attendanceDict.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"HR: Record {firebaseKey} resulted in empty dictionary");
                    return false;
                }

                // Extract values with safe fallbacks

                string timeInStr = GetSafeString(attendanceDict, "time_in");
                string timeOutStr = GetSafeString(attendanceDict, "time_out");
                string attendanceDateStr = GetSafeString(attendanceDict, "attendance_date");
                string existingStatus = GetSafeString(attendanceDict, "status");
                string employeeId = GetSafeString(attendanceDict, "employee_id", "N/A");
                string hoursWorked = GetSafeString(attendanceDict, "hours_worked", "0.00");
                string verification = GetSafeString(attendanceDict, "verification_method", "Manual");
                string overtimeHoursStr = GetSafeString(attendanceDict, "overtime_hours", "0.00");

                System.Diagnostics.Debug.WriteLine($"HR: Extracted - Employee: {employeeId}, Date: {attendanceDateStr}, TimeIn: {timeInStr}, TimeOut: {timeOutStr}, Status: {existingStatus}");

                // Apply date filter if a date is selected
                if (selectedDate.HasValue)
                {
                    bool shouldInclude = false;

                    if (!string.IsNullOrEmpty(attendanceDateStr) && DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        if (attendanceDate.Date == selectedDate.Value.Date)
                            shouldInclude = true;

                        System.Diagnostics.Debug.WriteLine($"HR: Date filter: {attendanceDate.Date} == {selectedDate.Value.Date} = {shouldInclude}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"HR: Could not parse attendance date: {attendanceDateStr}");
                    }

                    if (!shouldInclude)
                    {
                        System.Diagnostics.Debug.WriteLine($"HR: Record {firebaseKey} filtered out by date selection");
                        return false;
                    }
                }

                // Process the data
                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);
                string status = CalculateStatus(timeInStr, timeOutStr, employeeId, attendanceDateStr, existingStatus);
                string overtime = CalculateOvertimeHours(overtimeHoursStr, hoursWorked, employeeId, attendanceDateStr);

                // Adjust hours worked by subtracting overtime
                if (double.TryParse(hoursWorked, out double hw) && double.TryParse(overtime, out double ot))
                {
                    hoursWorked = Math.Round(hw - ot, 2).ToString("0.00");
                }

                string fullName = "N/A";
                if (employeeDict.ContainsKey(employeeId))
                {
                    var employee = employeeDict[employeeId];
                    // Safely access employee properties
                    string firstName = GetEmployeeProperty(employee, "first_name");
                    string middleName = GetEmployeeProperty(employee, "middle_name");
                    string lastName = GetEmployeeProperty(employee, "last_name");

                    // Build name carefully, handling empty parts
                    List<string> nameParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(firstName)) nameParts.Add(firstName.Trim());
                    if (!string.IsNullOrWhiteSpace(middleName)) nameParts.Add(middleName.Trim());
                    if (!string.IsNullOrWhiteSpace(lastName)) nameParts.Add(lastName.Trim());

                    fullName = string.Join(" ", nameParts);

                    // If still empty, use a fallback
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        fullName = "Unknown Employee";
                        System.Diagnostics.Debug.WriteLine($"HR: Employee {employeeId} has no name data");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"HR: Found employee: '{fullName}'");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"HR: Employee {employeeId} not found in employee dictionary");
                    fullName = $"Unknown ({employeeId})";
                }

                // DEBUG: Show final data that will be added to grid
                System.Diagnostics.Debug.WriteLine($"HR: Final data - ID: {employeeId}, Name: {fullName}, Status: {status}, Hours: {hoursWorked}, Overtime: {overtime}");

                // Add to DataGridView
                if (dataGridViewAttendance.InvokeRequired)
                {
                    dataGridViewAttendance.Invoke((MethodInvoker)delegate
                    {
                        AddRowToDataGridView(counter, employeeId, fullName, timeIn, timeOut, hoursWorked, status, overtime, verification, attendanceDateStr, firebaseKey);
                    });
                }
                else
                {
                    AddRowToDataGridView(counter, employeeId, fullName, timeIn, timeOut, hoursWorked, status, overtime, verification, attendanceDateStr, firebaseKey);
                }

                System.Diagnostics.Debug.WriteLine($"HR: === Successfully processed record {firebaseKey} ===\n");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HR: === ERROR processing record {firebaseKey}: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"HR: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private string FormatFirebaseTime(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr) || dateTimeStr == "N/A")
                return "N/A";

            try
            {
                if (DateTime.TryParse(dateTimeStr, out DateTime dt))
                    return dt.ToString("h:mm tt");
                return dateTimeStr;
            }
            catch { return dateTimeStr; }
        }

        // Helper method to safely extract string values
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
                System.Diagnostics.Debug.WriteLine($"HR: Error getting {key}: {ex.Message}");
            }
            return defaultValue;
        }

        // Helper method to add row to DataGridView
        private void AddRowToDataGridView(int counter, string employeeId, string fullName, string timeIn, string timeOut, string hoursWorked, string status, string overtime, string verification, string attendanceDateStr, string firebaseKey)
        {
            try
            {
                int newRowIndex = dataGridViewAttendance.Rows.Add(
                    counter,
                    employeeId,
                    fullName,
                    timeIn,
                    timeOut,
                    hoursWorked,
                    status,
                    overtime,
                    verification,
                    attendanceDateStr,
                    Properties.Resources.VerticalThreeDots
                );
                attendanceKeyMap[newRowIndex] = firebaseKey;
                System.Diagnostics.Debug.WriteLine($"HR: ✓ Added row at index {newRowIndex} for employee {employeeId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HR: ✗ Failed to add row to DataGridView: {ex.Message}");
            }
        }

        // Helper method to safely get employee properties
        private string GetEmployeeProperty(dynamic employee, string propertyName)
        {
            try
            {
                // DEBUG: Check what type we're dealing with
                System.Diagnostics.Debug.WriteLine($"HR: GetEmployeeProperty: Looking for {propertyName}, Employee type: {employee?.GetType()}");

                // Try multiple approaches to access the property

                // Approach 1: Direct dictionary access
                if (employee is IDictionary<string, object> employeeDict)
                {
                    if (employeeDict.ContainsKey(propertyName) && employeeDict[propertyName] != null)
                    {
                        string value = employeeDict[propertyName].ToString();
                        System.Diagnostics.Debug.WriteLine($"HR: Found {propertyName} in dictionary: '{value}'");
                        return value;
                    }
                }
                // Approach 2: Use JSON conversion
                else
                {
                    try
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(employee);
                        var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        if (dict != null && dict.ContainsKey(propertyName) && dict[propertyName] != null)
                        {
                            string value = dict[propertyName].ToString();
                            System.Diagnostics.Debug.WriteLine($"HR: Found {propertyName} via JSON: '{value}'");
                            return value;
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"HR: JSON approach failed for {propertyName}: {jsonEx.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"HR: Property {propertyName} not found");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HR: Error in GetEmployeeProperty for {propertyName}: {ex.Message}");
                return "";
            }
        }

        private void UpdateStatusLabel(DateTime? selectedDate)
        {
            if (labelAttendanceDate.InvokeRequired)
            {
                labelAttendanceDate.Invoke((MethodInvoker)delegate
                {
                    if (selectedDate.HasValue)
                    {
                        labelAttendanceDate.Text = $"Attendance for {selectedDate.Value.ToString("yyyy-MM-dd")}";
                    }
                    else
                    {
                        labelAttendanceDate.Text = "All Attendance Records";
                    }
                });
            }
            else
            {
                if (selectedDate.HasValue)
                {
                    labelAttendanceDate.Text = $"Attendance for {selectedDate.Value.ToString("yyyy-MM-dd")}";
                }
                else
                {
                    labelAttendanceDate.Text = "All Attendance Records";
                }
            }
        }
        private void setFont()
        {
            labelHREmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelAttendanceDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelAddLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
            textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
            comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest LeaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, LeaveRequestForm);
        }
        private EmployeeSchedule GetScheduleForDate(string employeeId, string dateStr)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(dateStr))
                return null;

            try
            {
                DateTime attendanceDate;
                if (!DateTime.TryParse(dateStr, out attendanceDate))
                    return null;

                // Get day of week from attendance date
                string dayOfWeek = attendanceDate.DayOfWeek.ToString();

                if (employeeSchedules.ContainsKey(employeeId))
                {
                    return employeeSchedules[employeeId]
                        .FirstOrDefault(s => s.day_of_week?.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) == true);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        private bool HasScheduleForDate(string employeeId, string dateStr)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(dateStr))
                return false;

            try
            {
                DateTime attendanceDate;
                if (!DateTime.TryParse(dateStr, out attendanceDate))
                    return false;

                // Get day of week from attendance date
                string dayOfWeek = attendanceDate.DayOfWeek.ToString();

                if (employeeSchedules.ContainsKey(employeeId))
                {
                    return employeeSchedules[employeeId]
                        .Any(s => s.day_of_week?.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) == true);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}