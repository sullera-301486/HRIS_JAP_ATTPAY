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
        // Firebase client
        private AttendanceFilterCriteria currentAttendanceFilters = new AttendanceFilterCriteria();
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );
        private Dictionary<string, (string Department, string Position)> employeeDepartmentMap = new Dictionary<string, (string Department, string Position)>();
        private bool isLoading = false;
        // Store attendance records with their Firebase keys
        private Dictionary<int, string> attendanceKeyMap = new Dictionary<int, string>();
        public AdminAttendance()
        {
            InitializeComponent();

            // Remove existing event handlers to prevent duplicates
            comboBoxSelectDate.SelectedIndexChanged -= comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged -= textBoxSearchEmployee_TextChanged;

            // Add event handlers
            comboBoxSelectDate.SelectedIndexChanged += comboBoxSelectDate_SelectedIndexChanged;
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
           

            // Load data asynchronously
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            // Load employee department mapping first
            await LoadEmployeeDepartmentMap();

            // Then load attendance data
            LoadFirebaseAttendanceData();
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
                // Handle error silently or log it
                System.Diagnostics.Debug.WriteLine($"Error loading employee department map: {ex.Message}");
            }
        }

        // 🔹 FILTER HANDLERS - FIXED TO MATCH HRAttendance
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

        // 🔹 SORTING - FIXED TO MATCH HRAttendance
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
                        return rows.OrderBy(r => r.Cells["FullName"]?.Value?.ToString() ?? "",
                                          StringComparer.OrdinalIgnoreCase).ToList();

                    case "z-a":
                        return rows.OrderByDescending(r => r.Cells["FullName"]?.Value?.ToString() ?? "",
                                                  StringComparer.OrdinalIgnoreCase).ToList();

                    case "oldest-newest":
                        return rows.OrderBy(r =>
                        {
                            string timeIn = r.Cells["TimeIn"]?.Value?.ToString() ?? "";
                            string timeOut = r.Cells["TimeOut"]?.Value?.ToString() ?? "";
                            return GetAttendanceDateForSorting(timeIn, timeOut);
                        }).ToList();

                    case "newest-oldest":
                        return rows.OrderByDescending(r =>
                        {
                            string timeIn = r.Cells["TimeIn"]?.Value?.ToString() ?? "";
                            string timeOut = r.Cells["TimeOut"]?.Value?.ToString() ?? "";
                            return GetAttendanceDateForSorting(timeIn, timeOut);
                        }).ToList();

                    default:
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

        // 🔹 SEARCH / FILTER LOGIC - FIXED TO MATCH HRAttendance
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

            // Status filters - UPDATED TO MATCH HRAttendance
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

        private string ExtractFirstName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "";

            string[] nameParts = fullName.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return nameParts.Length > 0 ? nameParts[0] : "";
        }

        // Helper method to get attendance date for sorting
        private DateTime GetAttendanceDate(string timeIn, string timeOut)
        {
            if (!string.IsNullOrEmpty(timeIn) && timeIn != "N/A")
            {
                if (DateTime.TryParse(timeIn, out DateTime timeInDate))
                    return timeInDate.Date;
            }

            if (!string.IsNullOrEmpty(timeOut) && timeOut != "N/A")
            {
                if (DateTime.TryParse(timeOut, out DateTime timeOutDate))
                    return timeOutDate.Date;
            }

            return DateTime.MinValue;
        }

        private DateTime ParseAttendanceDate(string timeIn, string timeOut)
        {
            // Try to parse time in first
            if (!string.IsNullOrEmpty(timeIn) && timeIn != "N/A")
            {
                if (DateTime.TryParse(timeIn, out DateTime tIn))
                    return tIn;

                // Try parsing with common formats
                string[] formats = { "HH:mm", "hh:mm tt", "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss" };
                if (DateTime.TryParseExact(timeIn, formats, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out tIn))
                    return tIn;
            }

            // If time in fails, try time out
            if (!string.IsNullOrEmpty(timeOut) && timeOut != "N/A")
            {
                if (DateTime.TryParse(timeOut, out DateTime tOut))
                    return tOut;

                string[] formats = { "HH:mm", "hh:mm tt", "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss" };
                if (DateTime.TryParseExact(timeOut, formats, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out tOut))
                    return tOut;
            }

            // If both fail, return minimum date (will appear at top for oldest-first, bottom for newest-first)
            return DateTime.MinValue;
        }

        // ADD THESE METHODS (same as in the filter form):
        // Helper method to get values safely
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

        private async Task<Dictionary<string, (string Department, string Position)>> TryGetEmploymentInfoByIndex()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
                // First, try to get the entire EmploymentInfo node to see its structure
                var employmentInfoNode = await firebase.Child("EmploymentInfo").OnceSingleAsync<object>();

                if (employmentInfoNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("EmploymentInfo node is null");
                    return employmentDict;
                }

                // Try different approaches based on the data structure
                try
                {
                    // Approach 1: Try as indexed records (1, 2, 3, etc.)
                    for (int i = 1; i <= 20; i++) // Increased range to cover more records
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
                                    System.Diagnostics.Debug.WriteLine($"Loaded employment info for {empId}: {dept}, {pos}");
                                }
                            }
                            else
                            {
                                // If we hit a null record, we might have reached the end
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log but continue trying
                            System.Diagnostics.Debug.WriteLine($"Failed to get record {i}: {ex.Message}");

                            // If we get consecutive failures, break
                            if (i > 5 && employmentDict.Count == 0)
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Index approach failed: {ex.Message}");
                }

                // Approach 2: Try to get all records at once if index approach didn't work well
                if (employmentDict.Count == 0)
                {
                    try
                    {
                        var allRecords = await firebase.Child("EmploymentInfo").OnceAsync<Dictionary<string, object>>();

                        foreach (var firebaseRecord in allRecords)
                        {
                            if (firebaseRecord.Object != null)
                            {
                                string empId = GetValue(firebaseRecord.Object, "employee_id");
                                if (!string.IsNullOrEmpty(empId))
                                {
                                    string dept = GetValue(firebaseRecord.Object, "department");
                                    string pos = GetValue(firebaseRecord.Object, "position");
                                    employmentDict[empId] = (dept, pos);
                                    System.Diagnostics.Debug.WriteLine($"Loaded employment info (bulk) for {empId}: {dept}, {pos}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Bulk approach failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryGetEmploymentInfoByIndex failed: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"Total employment records loaded: {employmentDict.Count}");
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

        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            // Just call the unified filter method instead of doing separate logic
            ApplyAllAttendanceFilters();
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

            // Subscribe to filter events - FIXED: Only subscribe once
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

            // Refresh attendance with selected date
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
            dataGridViewAttendance.CellFormatting += dataGridViewAttendance_CellFormatting;
            // Define columns
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

        private void dataGridViewAttendance_CellContentClick(object sender, DataGridViewCellEventArgs e)
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
                    case "Late & Early Out":  // Added this line
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

        // NEW METHOD: Calculate status based on business rules
        // Updated CalculateStatus method for AdminAttendance.cs
        private string CalculateStatus(string timeInStr, string timeOutStr, string existingStatus = "")
        {
            // If time in is N/A, then it's Absent
            if (string.IsNullOrEmpty(timeInStr) || timeInStr == "N/A")
                return "Absent";

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

        private async void LoadFirebaseAttendanceData(DateTime? selectedDate = null)
        {
            if (isLoading) return;
            isLoading = true;

            try
            {
                // Clear both grid and data collection
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

                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                var employeeDict = new Dictionary<string, dynamic>();
                foreach (var emp in firebaseEmployees)
                    employeeDict[emp.Key] = emp.Object;

                var attendanceData = await firebase.Child("Attendance").OnceSingleAsync<JArray>();

                if (attendanceData == null || attendanceData.Count == 0)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show("No attendance records found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    return;
                }

                int counter = 1;
                int rowIndex = 0;

                foreach (var attendanceItem in attendanceData)
                {
                    if (attendanceItem != null && attendanceItem.Type != JTokenType.Null)
                    {
                        string firebaseKey = (counter - 1).ToString();
                        bool recordAdded = ProcessAttendanceRecord(attendanceItem, employeeDict, selectedDate, counter, rowIndex, firebaseKey);
                        if (recordAdded)
                        {
                            counter++;
                            rowIndex++;
                        }
                    }
                }

                UpdateStatusLabel(selectedDate);

                // Apply filters after loading data
                ApplyAllAttendanceFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                Cursor.Current = Cursors.Default;
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

        private bool ProcessAttendanceRecord(JToken attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, int counter, int rowIndex, string firebaseKey)
        {
            try
            {
                string timeInStr = attendance["time_in"]?.ToString() ?? "";
                string timeOutStr = attendance["time_out"]?.ToString() ?? "";
                string attendanceDateStr = attendance["attendance_date"]?.ToString() ?? "";
                string existingStatus = attendance["status"]?.ToString() ?? "";

                // Apply date filter if a date is selected - SAME AS HRAttendance
                if (selectedDate.HasValue)
                {
                    bool shouldInclude = false;

                    if (!string.IsNullOrEmpty(attendanceDateStr) && DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        if (attendanceDate.Date == selectedDate.Value.Date)
                            shouldInclude = true;
                    }

                    if (!shouldInclude && !string.IsNullOrEmpty(timeInStr) && DateTime.TryParse(timeInStr, out DateTime timeInDate))
                    {
                        if (timeInDate.Date == selectedDate.Value.Date)
                            shouldInclude = true;
                    }

                    if (!shouldInclude && !string.IsNullOrEmpty(timeOutStr) && DateTime.TryParse(timeOutStr, out DateTime timeOutDate))
                    {
                        if (timeOutDate.Date == selectedDate.Value.Date)
                            shouldInclude = true;
                    }

                    if (!shouldInclude)
                        return false;
                }

                string employeeId = attendance["employee_id"]?.ToString() ?? "N/A";
                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);
                string hoursWorked = attendance["hours_worked"]?.ToString() ?? "0.00";
                string status = CalculateStatus(timeInStr, timeOutStr, existingStatus);
                string verification = attendance["verification_method"]?.ToString() ?? "Manual";
                string overtimeHoursStr = attendance["overtime_hours"]?.ToString() ?? "0.00";
                string overtime = CalculateOvertimeHours(overtimeHoursStr, hoursWorked, employeeId, "");

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
                        // ADD ROW WITH ALL COLUMNS INCLUDING HIDDEN ATTENDANCE DATE
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
                            attendanceDateStr, // STORE IN HIDDEN COLUMN
                            Properties.Resources.VerticalThreeDots
                        );
                        attendanceKeyMap[newRowIndex] = firebaseKey;
                    });
                }
                else
                {
                    // ADD ROW WITH ALL COLUMNS INCLUDING HIDDEN ATTENDANCE DATE
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
                        attendanceDateStr, // STORE IN HIDDEN COLUMN
                        Properties.Resources.VerticalThreeDots
                    );
                    attendanceKeyMap[newRowIndex] = firebaseKey;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing attendance record: {ex.Message}");
                return false;
            }
        }

        // Sorting methods for external access
        public void SortAttendanceAZ()
        {
            currentAttendanceFilters.SortBy = "a-z";
            ApplyAllAttendanceFilters();
        }

        public void SortAttendanceZA()
        {
            currentAttendanceFilters.SortBy = "z-a";
            ApplyAllAttendanceFilters();
        }

        public void SortAttendanceOldestNewest()
        {
            currentAttendanceFilters.SortBy = "oldest-newest";
            ApplyAllAttendanceFilters();
        }

        public void SortAttendanceNewestOldest()
        {
            currentAttendanceFilters.SortBy = "newest-oldest";
            ApplyAllAttendanceFilters();
        }
    }
}