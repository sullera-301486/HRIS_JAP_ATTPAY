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
        private List<AttendanceRowData> allAttendanceData = new List<AttendanceRowData>();  
        private bool isLoading = false;
        

        public HRAttendance()
        {
            InitializeComponent();

            // FIXED: Remove existing event handlers BEFORE adding new ones
            comboBoxSelectDate.SelectedIndexChanged -= comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged -= textBoxSearchEmployee_TextChanged;

            // Add event handlers
            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
            comboBoxSelectDate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSelectDate.IntegralHeight = false;
            comboBoxSelectDate.MaxDropDownItems = 5;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            currentAttendanceFilters = new AttendanceFilterCriteria();
            await LoadEmployeeDepartmentMap();
            PopulateDateComboBox();
        }

        private async Task LoadEmployeeDepartmentMap()
        {
            try
            {
                employeeDepartmentMap = await TryGetEmploymentInfoByIndex();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee department map: {ex.Message}");
            }
        }

        // 🔹 FILTER HANDLERS
        private void ApplyAttendanceFilters(AttendanceFilterCriteria filters)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyAttendanceFilters called with SortBy: '{filters?.SortBy}'");
            currentAttendanceFilters = filters ?? new AttendanceFilterCriteria();
            ApplyAllAttendanceFilters();
        }

        // REPLACE: ResetAttendanceFilters
        private void ResetAttendanceFilters()
        {
            System.Diagnostics.Debug.WriteLine("=== RESETTING FILTERS ===");

            currentAttendanceFilters = new AttendanceFilterCriteria();

            // Clear search text WITHOUT triggering reload
            if (textBoxSearchEmployee.InvokeRequired)
            {
                textBoxSearchEmployee.Invoke((MethodInvoker)delegate
                {
                    textBoxSearchEmployee.Text = "";
                });
            }
            else
            {
                textBoxSearchEmployee.Text = "";
            }

            // Get current date selection
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

            // Reload Firebase data with current date selection
            LoadFirebaseAttendanceData(selectedDate);

            System.Diagnostics.Debug.WriteLine("=== FILTERS RESET COMPLETE ===");
        }

        private void ApplyAllAttendanceFilters()
        {
            dataGridViewAttendance.SuspendLayout();

            try
            {
                string searchText = textBoxSearchEmployee.Text.Trim().ToLower();

                // Treat placeholder text as empty search
                if (searchText == "find employee")
                {
                    searchText = "";
                }

                System.Diagnostics.Debug.WriteLine($"ApplyAllAttendanceFilters: SearchText='{searchText}', SortBy='{currentAttendanceFilters?.SortBy}'");

                // Start with ALL loaded data
                var filteredData = new List<AttendanceRowData>();

                foreach (var rowData in allAttendanceData)
                {
                    bool matchesSearch = MatchesSearchText(rowData, searchText);
                    bool matchesFilters = MatchesAttendanceFilters(rowData);

                    if (matchesSearch && matchesFilters)
                    {
                        filteredData.Add(rowData);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Matching rows count before sorting: {filteredData.Count}");

                // Apply sorting if specified
                List<AttendanceRowData> sortedData;
                if (!string.IsNullOrEmpty(currentAttendanceFilters?.SortBy))
                {
                    System.Diagnostics.Debug.WriteLine($"Applying sorting: {currentAttendanceFilters.SortBy}");
                    sortedData = ApplySortingToData(filteredData);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No sorting - using natural order");
                    sortedData = filteredData;
                }

                System.Diagnostics.Debug.WriteLine($"Final rows count: {sortedData.Count}");

                // Clear grid and re-add rows
                dataGridViewAttendance.Rows.Clear();
                attendanceKeyMap.Clear();

                int counter = 1;
                foreach (var rowData in sortedData)
                {
                    int newRowIndex = dataGridViewAttendance.Rows.Add(
                        counter,
                        rowData.EmployeeId,
                        rowData.FullName,
                        rowData.TimeIn,
                        rowData.TimeOut,
                        rowData.HoursWorked,
                        rowData.Status,
                        rowData.OvertimeHours,
                        rowData.VerificationMethod,
                        rowData.AttendanceDate,
                        Properties.Resources.VerticalThreeDots
                    );

                    if (!string.IsNullOrEmpty(rowData.FirebaseKey))
                    {
                        attendanceKeyMap[newRowIndex] = rowData.FirebaseKey;
                    }

                    counter++;
                }
            }
            finally
            {
                dataGridViewAttendance.ResumeLayout();
                dataGridViewAttendance.Refresh();
            }
        }
        private List<AttendanceRowData> ApplySortingToData(List<AttendanceRowData> data)
        {
            System.Diagnostics.Debug.WriteLine($"ApplySortingToData called with {data.Count} items");

            if (currentAttendanceFilters == null || string.IsNullOrEmpty(currentAttendanceFilters.SortBy))
            {
                System.Diagnostics.Debug.WriteLine("No sort criteria specified - returning data as-is");
                return data;
            }

            string sort = currentAttendanceFilters.SortBy.ToLower().Trim();
            System.Diagnostics.Debug.WriteLine($"Sorting by: '{sort}'");

            try
            {
                List<AttendanceRowData> sortedData = new List<AttendanceRowData>();

                switch (sort)
                {
                    case "a-z":
                        System.Diagnostics.Debug.WriteLine("Applying A-Z sort by FULL NAME");
                        sortedData = data.OrderBy(d => d.FullName?.Trim() ?? "",
                            StringComparer.OrdinalIgnoreCase).ToList();
                        break;

                    case "z-a":
                        System.Diagnostics.Debug.WriteLine("Applying Z-A sort by FULL NAME");
                        sortedData = data.OrderByDescending(d => d.FullName?.Trim() ?? "",
                            StringComparer.OrdinalIgnoreCase).ToList();
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown sort option: {sort} - returning original order");
                        sortedData = data;
                        break;
                }

                System.Diagnostics.Debug.WriteLine($"After sorting, data count: {sortedData.Count}");
                return sortedData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ApplySortingToData: {ex.Message}");
                return data;
            }
        }


        // 🔹 SEARCH / FILTER LOGIC
        private bool MatchesSearchText(AttendanceRowData rowData, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "find employee")
                return true;

            string employeeId = rowData.EmployeeId?.ToLower() ?? "";
            string fullName = rowData.FullName?.ToLower() ?? "";
            string status = rowData.Status?.ToLower() ?? "";

            return employeeId.Contains(searchText) ||
                   fullName.Contains(searchText) ||
                   status.Contains(searchText);
        }

        private bool MatchesAttendanceFilters(AttendanceRowData rowData)
        {
            string employeeId = rowData.EmployeeId ?? "";
            string fullName = rowData.FullName ?? "";

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

            // Status filters
            if (currentAttendanceFilters.StatusPresent || currentAttendanceFilters.StatusAbsent ||
                currentAttendanceFilters.StatusLate || currentAttendanceFilters.StatusEarlyOut)
            {
                bool statusMatch = false;
                string status = rowData.Status ?? "";

                if (currentAttendanceFilters.StatusPresent && status.Equals("On Time", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;
                if (currentAttendanceFilters.StatusAbsent && status.Equals("Absent", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;
                if (currentAttendanceFilters.StatusLate && status.Equals("Late", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;
                if (currentAttendanceFilters.StatusEarlyOut && status.Equals("Early Out", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;

                if (currentAttendanceFilters.StatusLate && status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;
                if (currentAttendanceFilters.StatusEarlyOut && status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;

                if (!statusMatch) return false;
            }

            // Hours worked filters
            if (currentAttendanceFilters.HoursEight || currentAttendanceFilters.HoursBelowEight)
            {
                bool hoursMatch = false;
                string hoursWorkedStr = rowData.HoursWorked ?? "0";

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
                string overtimeStr = rowData.OvertimeHours ?? "0";

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

        // 🔹 STATUS CALCULATION - EXACT COPY FROM ADMINATTENDANCE
        private async Task<string> CalculateStatusWithSchedule(string timeInStr, string timeOutStr, string employeeId, string attendanceDate, string existingStatus = "")
        {
            try
            {
                // DEBUG: Log what we're processing
                System.Diagnostics.Debug.WriteLine($"🔍 HR Status Calc: Emp={employeeId}, Date={attendanceDate}, TimeIn={timeInStr}, ExistingStatus={existingStatus}");

                // 1. PRESERVE SYSTEM-GENERATED STATUSES
                // If Firebase already has a valid status, use it (especially for "Day Off", "Absent", etc.)
                if (!string.IsNullOrEmpty(existingStatus) && existingStatus != "N/A")
                {
                    System.Diagnostics.Debug.WriteLine($"✅ HR Using existing status from Firebase: {existingStatus}");
                    return existingStatus;
                }

                // 2. CHECK IF EMPLOYEE WORKED TODAY
                bool hasWorked = !(string.IsNullOrEmpty(timeInStr) || timeInStr == "N/A");

                // 3. GET SCHEDULE FOR THIS DAY
                var schedule = await GetEmployeeScheduleForDay(employeeId, attendanceDate);

                // 4. DECISION LOGIC
                if (!schedule.HasValue)
                {
                    // No schedule for this day
                    if (!hasWorked)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ HR No schedule + no work = Day Off for {employeeId}");
                        return "Day Off";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ HR No schedule but employee worked = On Time (unscheduled)");
                        return "On Time";
                    }
                }
                else
                {
                    // Has schedule for this day
                    if (!hasWorked)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ HR Has schedule but no work = Absent for {employeeId}");
                        return "Absent";
                    }
                    else
                    {
                        // Calculate based on actual times vs schedule WITH GRACE PERIOD
                        return await CalculateTimingStatusWithGracePeriod(timeInStr, timeOutStr, schedule.Value, employeeId, attendanceDate);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HR Error in CalculateStatusWithSchedule: {ex.Message}");
                return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
            }
        }

        private async Task<string> CalculateTimingStatusWithGracePeriod(string timeInStr, string timeOutStr, (TimeSpan startTime, TimeSpan endTime) schedule, string employeeId, string attendanceDate)
        {
            try
            {
                TimeSpan expectedStart = schedule.startTime;
                TimeSpan expectedEnd = schedule.endTime;

                System.Diagnostics.Debug.WriteLine($"📅 HR Schedule: {expectedStart} - {expectedEnd} for {employeeId}");

                DateTime timeIn, timeOut;

                // Parse Time In
                bool timeInParsed = DateTime.TryParseExact(timeInStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInStr, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParse(timeInStr, out timeIn);

                // Parse Time Out
                bool timeOutParsed = DateTime.TryParseExact(timeOutStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutStr, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParse(timeOutStr, out timeOut);

                if (!timeInParsed)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ HR Failed to parse time in: {timeInStr}");
                    return "Absent";
                }

                TimeSpan actualTimeIn = new TimeSpan(timeIn.Hour, timeIn.Minute, timeIn.Second);

                // 5-MINUTE GRACE PERIOD FOR LATE ARRIVAL
                bool isLate = actualTimeIn > expectedStart.Add(TimeSpan.FromMinutes(5));
                bool isEarlyOut = false;

                // Check early out only if we have valid time out WITH 5-MINUTE GRACE PERIOD
                if (timeOutParsed && timeOutStr != "N/A" && !string.IsNullOrEmpty(timeOutStr))
                {
                    TimeSpan actualTimeOut = new TimeSpan(timeOut.Hour, timeOut.Minute, timeOut.Second);
                    isEarlyOut = actualTimeOut < expectedEnd.Subtract(TimeSpan.FromMinutes(5));
                    System.Diagnostics.Debug.WriteLine($"⏰ HR Time Out: {actualTimeOut} vs Expected: {expectedEnd}, Early Out: {isEarlyOut}");
                }

                System.Diagnostics.Debug.WriteLine($"⏰ HR Time In: {actualTimeIn} vs Expected: {expectedStart}, Late: {isLate}");

                // Determine status
                if (isLate && isEarlyOut)
                    return "Late & Early Out";
                else if (isLate)
                    return "Late";
                else if (isEarlyOut)
                    return "Early Out";
                else
                    return "On Time";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HR Error in CalculateTimingStatusWithGracePeriod: {ex.Message}");
                return "On Time";
            }
        }

       
        private string CalculateOvertimeHours(string overtimeStr, string hoursWorked, string employeeId, string scheduleId)
        {
            // First try to use the overtime_hours field directly from Firebase
            if (!string.IsNullOrEmpty(overtimeStr) && double.TryParse(overtimeStr, out double overtimeHours))
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

        // 🔹 LOAD EMPLOYEE DEPARTMENT MAPPING
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
                for (int i = 1; i <= 20; i++)
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
                        else
                        {
                            break;
                        }
                    }
                    catch
                    {
                        if (i > 5 && employmentDict.Count == 0)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryGetEmploymentInfoByIndex failed: {ex.Message}");
            }

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
        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            ApplyAllAttendanceFilters();
        }

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

            dataGridViewAttendance.CellClick += dataGridViewAttendance_CellClick;
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                Color statusBackColor = Color.White;
                Color statusForeColor = Color.Black;

                switch (e.Value.ToString())
                {
                   case "On Time":
                        statusBackColor = Color.FromArgb(95, 218, 71); // Green
                        statusForeColor = Color.White;
                        break;
                    case "Late":
                        statusBackColor = Color.FromArgb(255, 163, 74); 
                        statusForeColor = Color.White;
                        break;
                    case "Early Out":
                        statusBackColor = Color.FromArgb(255, 163, 74);
                        statusForeColor = Color.White;
                        break;
                    case "Late & Early Out":
                        statusBackColor = Color.FromArgb(255, 163, 74);
                        statusForeColor = Color.White;
                        break;
                    case "Absent":
                        statusBackColor = Color.FromArgb(221, 60, 60); 
                        statusForeColor = Color.White;
                        break;
                    case "Leave":
                        statusBackColor = Color.FromArgb(180, 174, 189); 
                        statusForeColor = Color.White;
                        break;
                    case "Day Off":
                        statusBackColor = Color.FromArgb(71, 93, 218); 
                        statusForeColor = Color.White;
                        break;
                }

                e.CellStyle.BackColor = statusBackColor;
                e.CellStyle.ForeColor = statusForeColor;
                e.CellStyle.SelectionBackColor = statusBackColor;
                e.CellStyle.SelectionForeColor = statusForeColor;
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
                DataGridViewRow selectedRow = dataGridViewAttendance.Rows[e.RowIndex];

                string firebaseKey = null;
                if (attendanceKeyMap.ContainsKey(e.RowIndex))
                {
                    firebaseKey = attendanceKeyMap[e.RowIndex];
                }

                string attendanceDate = selectedRow.Cells["AttendanceDate"].Value?.ToString();

                Form parentForm = this.FindForm();
                EditAttendance editAttendanceForm = new EditAttendance();

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
                    attendanceDate
                );

                AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);

                editAttendanceForm.FormClosed += (s, args) => {
                    if (editAttendanceForm.DataUpdated)
                    {
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

                comboBoxSelectDate.Items.Clear();
                comboBoxSelectDate.Items.Add("All Dates");
                comboBoxSelectDate.Items.Add(DateTime.Today.ToString("yyyy-MM-dd"));
                comboBoxSelectDate.SelectedIndex = 1;
                LoadFirebaseAttendanceData(DateTime.Today);
            }
        }

        private string ExtractDateFromAnyRecordType(dynamic record, string recordKey)
        {
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
                // Clear the data collection
                allAttendanceData.Clear();

                Cursor.Current = Cursors.WaitCursor;

                System.Diagnostics.Debug.WriteLine($"Loading attendance data. Selected date: {selectedDate}");

                // Load employee data
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                System.Diagnostics.Debug.WriteLine($"Loaded {employeeDict.Count} employees");

                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();

                System.Diagnostics.Debug.WriteLine($"Found {attendanceRecords?.Count()} attendance records");

                if (attendanceRecords == null || !attendanceRecords.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No attendance records found in Firebase");
                    return;
                }

                int processedCount = 0;

                // Load data into memory instead of directly to grid
                foreach (var attendanceRecord in attendanceRecords)
                {
                    if (attendanceRecord?.Object != null)
                    {
                        string firebaseKey = attendanceRecord.Key;

                        var rowData = await ProcessAttendanceRecordToData(
                            attendanceRecord.Object,
                            employeeDict,
                            selectedDate,
                            firebaseKey
                        );

                        if (rowData != null)
                        {
                            allAttendanceData.Add(rowData);
                            processedCount++;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Successfully loaded {processedCount} records into memory");

                UpdateStatusLabel(selectedDate);

                // Apply filters and display the data
                ApplyAllAttendanceFilters();
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

        private async Task<bool> ProcessAttendanceRecord(dynamic attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, int counter, int rowIndex, string firebaseKey)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"HR: === Processing record {firebaseKey} ===");

                Dictionary<string, object> attendanceDict = new Dictionary<string, object>();

                if (attendance == null)
                {
                    System.Diagnostics.Debug.WriteLine($"HR: Record {firebaseKey} is NULL");
                    return false;
                }

                try
                {
                    if (attendance is IDictionary<string, object> directDict)
                    {
                        attendanceDict = new Dictionary<string, object>(directDict);
                        System.Diagnostics.Debug.WriteLine("HR: Used direct dictionary cast");
                    }
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

                string timeInStr = GetSafeString(attendanceDict, "time_in");
                string timeOutStr = GetSafeString(attendanceDict, "time_out");
                string attendanceDateStr = GetSafeString(attendanceDict, "attendance_date");
                string existingStatus = GetSafeString(attendanceDict, "status");
                string employeeId = GetSafeString(attendanceDict, "employee_id", "N/A");
                string hoursWorked = GetSafeString(attendanceDict, "hours_worked", "0.00");
                string verification = GetSafeString(attendanceDict, "verification_method", "Manual");
                string overtimeHoursStr = GetSafeString(attendanceDict, "overtime_hours", "0.00");

                System.Diagnostics.Debug.WriteLine($"HR: Extracted - Employee: {employeeId}, Date: {attendanceDateStr}, TimeIn: {timeInStr}, TimeOut: {timeOutStr}, Status: {existingStatus}");

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

                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);

                // USE THE EXACT SAME METHOD AS ADMINATTENDANCE
                string status = await CalculateStatusWithSchedule(timeInStr, timeOutStr, employeeId, attendanceDateStr, existingStatus);

                string overtime = CalculateOvertimeHours(overtimeHoursStr, hoursWorked, employeeId, "");

                if (double.TryParse(hoursWorked, out double hw) && double.TryParse(overtime, out double ot))
                {
                    hoursWorked = Math.Round(hw - ot, 2).ToString("0.00");
                }

                string fullName = "N/A";
                if (employeeDict.ContainsKey(employeeId))
                {
                    var employee = employeeDict[employeeId];
                    string firstName = GetEmployeeProperty(employee, "first_name");
                    string middleName = GetEmployeeProperty(employee, "middle_name");
                    string lastName = GetEmployeeProperty(employee, "last_name");

                    List<string> nameParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(firstName)) nameParts.Add(firstName.Trim());
                    if (!string.IsNullOrWhiteSpace(middleName)) nameParts.Add(middleName.Trim());
                    if (!string.IsNullOrWhiteSpace(lastName)) nameParts.Add(lastName.Trim());

                    fullName = string.Join(" ", nameParts);

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

                System.Diagnostics.Debug.WriteLine($"HR: Final data - ID: {employeeId}, Name: {fullName}, Status: {status}, Hours: {hoursWorked}, Overtime: {overtime}");

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

        private string GetEmployeeProperty(dynamic employee, string propertyName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"HR: GetEmployeeProperty: Looking for {propertyName}, Employee type: {employee?.GetType()}");

                if (employee is IDictionary<string, object> employeeDict)
                {
                    if (employeeDict.ContainsKey(propertyName) && employeeDict[propertyName] != null)
                    {
                        string value = employeeDict[propertyName].ToString();
                        System.Diagnostics.Debug.WriteLine($"HR: Found {propertyName} in dictionary: '{value}'");
                        return value;
                    }
                }
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

        private async Task<(TimeSpan startTime, TimeSpan endTime)?> GetEmployeeScheduleForDay(string employeeId, string attendanceDate)
        {
            try
            {
                if (!DateTime.TryParse(attendanceDate, out DateTime date))
                {
                    System.Diagnostics.Debug.WriteLine($"HR: Failed to parse attendance date: {attendanceDate}");
                    return null;
                }

                string dayOfWeek = date.DayOfWeek.ToString();
                string normalizedTargetDay = NormalizeDayOfWeek(dayOfWeek);

                System.Diagnostics.Debug.WriteLine($"HR: Looking for schedule: Employee {employeeId} on {dayOfWeek} ({normalizedTargetDay})");

                var scheduleRecords = await firebase.Child("Work_Schedule").OnceAsync<dynamic>();

                if (scheduleRecords == null || !scheduleRecords.Any())
                {
                    System.Diagnostics.Debug.WriteLine("HR: No schedule records found in Firebase");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"HR: Found {scheduleRecords.Count()} total schedule records");

                foreach (var scheduleRecord in scheduleRecords)
                {
                    if (scheduleRecord?.Object != null)
                    {
                        try
                        {
                            string schedEmpId = GetScheduleProperty(scheduleRecord.Object, "employee_id");
                            string schedDay = GetScheduleProperty(scheduleRecord.Object, "day_of_week");

                            string normalizedSchedDay = NormalizeDayOfWeek(schedDay);

                            System.Diagnostics.Debug.WriteLine($"HR: Checking schedule: EmpID={schedEmpId}, Day={schedDay} ({normalizedSchedDay})");

                            if (schedEmpId == employeeId && normalizedSchedDay == normalizedTargetDay)
                            {
                                string startTimeStr = GetScheduleProperty(scheduleRecord.Object, "start_time");
                                string endTimeStr = GetScheduleProperty(scheduleRecord.Object, "end_time");

                                System.Diagnostics.Debug.WriteLine($"HR: ✅ Found matching schedule for {employeeId} on {dayOfWeek}: {startTimeStr} - {endTimeStr}");

                                if (TryParseScheduleTime(startTimeStr, out TimeSpan startTime) &&
                                    TryParseScheduleTime(endTimeStr, out TimeSpan endTime))
                                {
                                    System.Diagnostics.Debug.WriteLine($"HR: ✅ Parsed times successfully: Start={startTime}, End={endTime}");
                                    return (startTime, endTime);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"HR: ❌ Failed to parse times: Start='{startTimeStr}', End='{endTimeStr}'");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"HR: Error processing schedule record: {ex.Message}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"HR: ❌ No matching schedule found for {employeeId} on {dayOfWeek}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HR: ❌ Error getting schedule: {ex.Message}");
                return null;
            }
        }

        private string NormalizeDayOfWeek(string day)
        {
            if (string.IsNullOrEmpty(day)) return day;

            string normalized = day.Trim().ToLower();

            if (normalized == "mon" || normalized == "monday")
                return "monday";
            else if (normalized == "tue" || normalized == "tues" || normalized == "tuesday")
                return "tuesday";
            else if (normalized == "wed" || normalized == "wednesday")
                return "wednesday";
            else if (normalized == "thu" || normalized == "thur" || normalized == "thurs" || normalized == "thursday")
                return "thursday";
            else if (normalized == "fri" || normalized == "friday")
                return "friday";
            else if (normalized == "sat" || normalized == "saturday")
                return "saturday";
            else if (normalized == "sun" || normalized == "sunday")
                return "sunday";
            else
                return normalized;
        }

        private string GetScheduleProperty(dynamic scheduleObj, string propertyName)
        {
            try
            {
                if (scheduleObj is IDictionary<string, object> dict)
                {
                    if (dict.ContainsKey(propertyName) && dict[propertyName] != null)
                        return dict[propertyName].ToString();
                }

                try
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(scheduleObj);
                    var jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (jsonDict != null && jsonDict.ContainsKey(propertyName) && jsonDict[propertyName] != null)
                        return jsonDict[propertyName].ToString();
                }
                catch { }

                try
                {
                    var prop = scheduleObj.GetType().GetProperty(propertyName);
                    if (prop != null)
                    {
                        var value = prop.GetValue(scheduleObj);
                        if (value != null)
                            return value.ToString();
                    }
                }
                catch { }

                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HR: Error getting property {propertyName}: {ex.Message}");
                return "";
            }
        }

        private bool TryParseScheduleTime(string timeStr, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (string.IsNullOrEmpty(timeStr))
                return false;

            if (TimeSpan.TryParse(timeStr, out time))
                return true;

            if (DateTime.TryParse(timeStr, out DateTime dateTime))
            {
                time = dateTime.TimeOfDay;
                return true;
            }

            return false;
        }
        private async Task<AttendanceRowData> ProcessAttendanceRecordToData(
    dynamic attendance,
    Dictionary<string, dynamic> employeeDict,
    DateTime? selectedDate,
    string firebaseKey)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Processing record {firebaseKey} ===");

                Dictionary<string, object> attendanceDict = new Dictionary<string, object>();

                if (attendance == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} is NULL");
                    return null;
                }

                // Extract attendance data (use your existing logic)
                try
                {
                    if (attendance is IDictionary<string, object> directDict)
                    {
                        attendanceDict = new Dictionary<string, object>(directDict);
                    }
                    else
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(attendance);
                        attendanceDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    }
                }
                catch (Exception serializationEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Serialization failed: {serializationEx.Message}");
                    // Try reflection approach
                    var properties = attendance.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        try
                        {
                            var value = prop.GetValue(attendance);
                            attendanceDict[prop.Name] = value;
                        }
                        catch { }
                    }
                }

                if (attendanceDict == null || !attendanceDict.Any())
                {
                    return null;
                }

                // Extract values
                string timeInStr = GetSafeString(attendanceDict, "time_in");
                string timeOutStr = GetSafeString(attendanceDict, "time_out");
                string attendanceDateStr = GetSafeString(attendanceDict, "attendance_date");
                string existingStatus = GetSafeString(attendanceDict, "status");
                string employeeId = GetSafeString(attendanceDict, "employee_id", "N/A");
                string hoursWorked = GetSafeString(attendanceDict, "hours_worked", "0.00");
                string verification = GetSafeString(attendanceDict, "verification_method", "Manual");
                string overtimeHoursStr = GetSafeString(attendanceDict, "overtime_hours", "0.00");

                // Apply date filter
                if (selectedDate.HasValue)
                {
                    bool shouldInclude = false;

                    if (!string.IsNullOrEmpty(attendanceDateStr) &&
                        DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        if (attendanceDate.Date == selectedDate.Value.Date)
                            shouldInclude = true;
                    }

                    if (!shouldInclude)
                    {
                        return null;
                    }
                }

                // Process the data (use your existing methods)
                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);
                string status = await CalculateStatusWithSchedule(timeInStr, timeOutStr, employeeId, attendanceDateStr, existingStatus);
                string overtime = CalculateOvertimeHours(overtimeHoursStr, hoursWorked, employeeId, "");

                // Adjust hours worked by subtracting overtime
                if (double.TryParse(hoursWorked, out double hw) && double.TryParse(overtime, out double ot))
                {
                    hoursWorked = Math.Round(hw - ot, 2).ToString("0.00");
                }

                string fullName = "N/A";
                if (employeeDict.ContainsKey(employeeId))
                {
                    var employee = employeeDict[employeeId];
                    string firstName = GetEmployeeProperty(employee, "first_name");
                    string middleName = GetEmployeeProperty(employee, "middle_name");
                    string lastName = GetEmployeeProperty(employee, "last_name");
                    fullName = $"{firstName} {middleName} {lastName}".Trim();
                }

                // Return data object instead of adding to grid
                return new AttendanceRowData
                {
                    EmployeeId = employeeId,
                    FullName = fullName,
                    TimeIn = timeIn,
                    TimeOut = timeOut,
                    HoursWorked = hoursWorked,
                    Status = status,
                    OvertimeHours = overtime,
                    VerificationMethod = verification,
                    AttendanceDate = attendanceDateStr,
                    FirebaseKey = firebaseKey
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ERROR processing record {firebaseKey}: {ex.Message} ===");
                return null;
            }
        }
    }
}