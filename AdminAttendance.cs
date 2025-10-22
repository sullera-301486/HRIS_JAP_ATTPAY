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
        private List<AttendanceRowData> allAttendanceData = new List<AttendanceRowData>();
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

            dtpSingleDateSelector.ValueChanged -= dtpSingleDateSelector_ValueChanged;
            dtpSingleDateSelector.ValueChanged += dtpSingleDateSelector_ValueChanged;

            textBoxSearchEmployee.TextChanged -= textBoxSearchEmployee_TextChanged;
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            // Initialize with no specific sorting (will use natural order from Firebase)
            currentAttendanceFilters = new AttendanceFilterCriteria();

            // Load employee department mapping first
            await LoadEmployeeDepartmentMap();

            // Then initialize date selector which will trigger data loading with today's date
            InitializeDateSelector(); // CHANGED: From PopulateDateComboBox to InitializeDateSelector
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
            System.Diagnostics.Debug.WriteLine($"ApplyAttendanceFilters called with SortBy: '{filters?.SortBy}', UseCutOffDate: {filters?.UseCutOffDate}");
            currentAttendanceFilters = filters ?? new AttendanceFilterCriteria();

            // CRITICAL: If cut-off date is selected in filter, reload ALL data with date range
            if (currentAttendanceFilters.UseCutOffDate && currentAttendanceFilters.CutOffDate.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"Loading with cut-off date range: {currentAttendanceFilters.CutOffDate.Value}, First Half: {currentAttendanceFilters.IsFirstHalf}");
                LoadFirebaseAttendanceData(null); // null = use cut-off range instead of single date
            }
            else
            {
                // No cut-off date filter, just apply filters to currently loaded data (single date remains active)
                System.Diagnostics.Debug.WriteLine("No cut-off date, applying filters to current view with single date selector");
                ApplyAllAttendanceFilters();
            }
        }

        private void ResetAttendanceFilters()
        {
            System.Diagnostics.Debug.WriteLine("=== RESETTING FILTERS ===");

            // Reset the filter criteria
            currentAttendanceFilters = new AttendanceFilterCriteria();

            // Clear the search text WITHOUT triggering reload
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

            // CRITICAL: Get the CURRENTLY selected date from DateTimePicker
            DateTime selectedDate = dtpSingleDateSelector.Value;

            // Reload the Firebase data with the CURRENT date selection (single date selector takes priority)
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

                // FIXED: Start with ALL loaded data, not just visible rows
                var filteredData = new List<AttendanceRowData>();

                foreach (var rowData in allAttendanceData)
                {
                    // Check if this row matches search and filters
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

                // Clear the grid and re-add rows
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
                        sortedData = data.OrderBy(d =>
                        {
                            string fullName = d.FullName?.Trim() ?? "";
                            System.Diagnostics.Debug.WriteLine($"A-Z Sorting: '{fullName}'");
                            return fullName;
                        }, StringComparer.OrdinalIgnoreCase).ToList();
                        break;

                    case "z-a":
                        System.Diagnostics.Debug.WriteLine("Applying Z-A sort by FULL NAME");
                        sortedData = data.OrderByDescending(d =>
                        {
                            string fullName = d.FullName?.Trim() ?? "";
                            System.Diagnostics.Debug.WriteLine($"Z-A Sorting: '{fullName}'");
                            return fullName;
                        }, StringComparer.OrdinalIgnoreCase).ToList();
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown sort option: {sort} - returning original order");
                        sortedData = data;
                        break;
                }

                System.Diagnostics.Debug.WriteLine($"After sorting, data count: {sortedData.Count}");

                // Debug: Print the sorted order
                foreach (var item in sortedData.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"Sorted Data: '{item.FullName}'");
                }

                return sortedData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ApplySortingToData: {ex.Message}");
                return data;
            }
        }

        private void UpdateRowNumbers()
        {
            int counter = 1;
            foreach (DataGridViewRow row in dataGridViewAttendance.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    // Ensure the RowNumber cell exists and is accessible
                    if (row.Cells["RowNumber"] != null)
                    {
                        row.Cells["RowNumber"].Value = counter;
                    }
                    counter++;
                }
                else if (!row.IsNewRow)
                {
                    // Ensure hidden rows don't show row numbers
                    if (row.Cells["RowNumber"] != null)
                    {
                        row.Cells["RowNumber"].Value = "";
                    }
                }
            }
        }

        // 🔹 SEARCH / FILTER LOGIC - FIXED TO MATCH HRAttendance
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

                if (currentAttendanceFilters.StatusPresent && status.Equals("On Time", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusAbsent && status.Equals("Absent", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusLate && status.Equals("Late", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusEarlyOut && status.Equals("Early Out", StringComparison.OrdinalIgnoreCase)) statusMatch = true;

                if (currentAttendanceFilters.StatusLate && status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase)) statusMatch = true;
                if (currentAttendanceFilters.StatusEarlyOut && status.Equals("Late & Early Out", StringComparison.OrdinalIgnoreCase)) statusMatch = true;

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

        private void dtpSingleDateSelector_ValueChanged(object sender, EventArgs e)
        {
            DateTime selectedDate = dtpSingleDateSelector.Value;

            // CRITICAL: Clear the cut-off date filter when user manually changes the date
            if (currentAttendanceFilters.UseCutOffDate)
            {
                System.Diagnostics.Debug.WriteLine("DateTimePicker changed - clearing cut-off filter");
                currentAttendanceFilters.UseCutOffDate = false;
                currentAttendanceFilters.CutOffDate = null;
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
                buttonExportDTR.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAdminAttendance.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelAttendanceDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManageLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
                dtpSingleDateSelector.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
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
                        DateTime selectedDate = dtpSingleDateSelector.Value;

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
                        statusBackColor = Color.FromArgb(71, 93, 218); 
                        statusForeColor = Color.White;
                        break;
                    case "Day Off":
                        statusBackColor = Color.FromArgb(71, 93, 218); 
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
        private async Task<string> CalculateStatusWithSchedule(string timeInStr, string timeOutStr, string employeeId, string attendanceDate, string existingStatus = "")
        {
            try
            {
                // DEBUG: Log what we're processing
                System.Diagnostics.Debug.WriteLine($"🔍 Status Calc: Emp={employeeId}, Date={attendanceDate}, TimeIn={timeInStr}, ExistingStatus={existingStatus}");

                // 1. PRESERVE SYSTEM-GENERATED STATUSES
                // If Firebase already has a valid status, use it (especially for "Day Off", "Absent", etc.)
                if (!string.IsNullOrEmpty(existingStatus) && existingStatus != "N/A")
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Using existing status from Firebase: {existingStatus}");
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
                        System.Diagnostics.Debug.WriteLine($"✅ No schedule + no work = Day Off for {employeeId}");
                        return "Day Off";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ No schedule but employee worked = On Time (unscheduled)");
                        return "On Time";
                    }
                }
                else
                {
                    // Has schedule for this day
                    if (!hasWorked)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Has schedule but no work = Absent for {employeeId}");
                        return "Absent";
                    }
                    else
                    {
                        // Calculate based on actual times vs schedule
                        return await CalculateTimingStatus(timeInStr, timeOutStr, schedule.Value, employeeId, attendanceDate);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in CalculateStatusWithSchedule: {ex.Message}");
                return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
            }
        }

        private async Task<string> CalculateTimingStatus(string timeInStr, string timeOutStr, (TimeSpan startTime, TimeSpan endTime) schedule, string employeeId, string attendanceDate)
        {
            try
            {
                TimeSpan expectedStart = schedule.startTime;
                TimeSpan expectedEnd = schedule.endTime;

                System.Diagnostics.Debug.WriteLine($"📅 Schedule: {expectedStart} - {expectedEnd} for {employeeId}");

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
                    System.Diagnostics.Debug.WriteLine($"❌ Failed to parse time in: {timeInStr}");
                    return "Absent";
                }

                TimeSpan actualTimeIn = new TimeSpan(timeIn.Hour, timeIn.Minute, timeIn.Second);
                bool isLate = actualTimeIn > expectedStart.Add(TimeSpan.FromMinutes(5)); // 5-minute grace period
                bool isEarlyOut = false;

                // Check early out only if we have valid time out
                if (timeOutParsed && timeOutStr != "N/A" && !string.IsNullOrEmpty(timeOutStr))
                {
                    TimeSpan actualTimeOut = new TimeSpan(timeOut.Hour, timeOut.Minute, timeOut.Second);
                    isEarlyOut = actualTimeOut < expectedEnd.Subtract(TimeSpan.FromMinutes(5)); // 5-minute grace period
                    System.Diagnostics.Debug.WriteLine($"⏰ Time Out: {actualTimeOut} vs Expected: {expectedEnd}, Early Out: {isEarlyOut}");
                }

                System.Diagnostics.Debug.WriteLine($"⏰ Time In: {actualTimeIn} vs Expected: {expectedStart}, Late: {isLate}");

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
                System.Diagnostics.Debug.WriteLine($"❌ Error in CalculateTimingStatus: {ex.Message}");
                return "On Time"; // Default to On Time if calculation fails
            }
        }

        private async void InitializeDateSelector()
        {
            try
            {
                // Set default to today's date
                dtpSingleDateSelector.Value = DateTime.Today;

                // Optional: Set custom format if needed
                dtpSingleDateSelector.Format = DateTimePickerFormat.Custom;
                dtpSingleDateSelector.CustomFormat = "yyyy-MM-dd";

                // Optional: Set min and max dates if needed
                // Get all attendance records to determine date range
                var attendanceRecords = await firebase.Child("Attendance").OnceAsync<dynamic>();

                if (attendanceRecords != null && attendanceRecords.Any())
                {
                    DateTime minDate = DateTime.Today;
                    DateTime maxDate = DateTime.Today;
                    bool datesSet = false;

                    foreach (var record in attendanceRecords)
                    {
                        if (record?.Object != null)
                        {
                            string dateStr = ExtractDateFromAnyRecordType(record.Object, record.Key);
                            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime recordDate))
                            {
                                if (!datesSet)
                                {
                                    minDate = recordDate;
                                    maxDate = recordDate;
                                    datesSet = true;
                                }
                                else
                                {
                                    if (recordDate < minDate) minDate = recordDate;
                                    if (recordDate > maxDate) maxDate = recordDate;
                                }
                            }
                        }
                    }

                    if (datesSet)
                    {
                        // Set the range to available dates (optional)
                        dtpSingleDateSelector.MinDate = minDate;
                        dtpSingleDateSelector.MaxDate = maxDate;
                    }
                }

                // Load data for today's date initially
                LoadFirebaseAttendanceData(DateTime.Today);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error initializing date selector: " + ex.Message);
                // Fallback: Set to today and load data
                dtpSingleDateSelector.Value = DateTime.Today;
                LoadFirebaseAttendanceData(DateTime.Today);
            }
        }

        // NEW METHOD: Extract date from any record type
        private string ExtractDateFromAnyRecordType(dynamic record, string recordKey)
        {
            try
            {
                // Method 1: Try direct property access (for dictionary-like records)
                try
                {
                    if (record is IDictionary<string, object> dict)
                    {
                        if (dict.ContainsKey("attendance_date") && dict["attendance_date"] != null)
                        {
                            string dateStr = dict["attendance_date"].ToString();
                            if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                            {
                                System.Diagnostics.Debug.WriteLine($"Found attendance_date in dict: {dateStr} for key: {recordKey}");
                                return dateStr;
                            }
                        }
                    }
                }
                catch { }

                // Method 2: Try JSON serialization approach
                try
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(record);
                    var jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (jsonDict != null)
                    {
                        if (jsonDict.ContainsKey("attendance_date") && jsonDict["attendance_date"] != null)
                        {
                            string dateStr = jsonDict["attendance_date"].ToString();
                            if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                            {
                                System.Diagnostics.Debug.WriteLine($"Found attendance_date in JSON: {dateStr} for key: {recordKey}");
                                return dateStr;
                            }
                        }

                        // Also try other possible date field names
                        string[] possibleDateFields = { "date", "attendanceDate", "attendance_date", "AttendanceDate" };
                        foreach (string field in possibleDateFields)
                        {
                            if (jsonDict.ContainsKey(field) && jsonDict[field] != null)
                            {
                                string dateStr = jsonDict[field].ToString();
                                if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                                {
                                    System.Diagnostics.Debug.WriteLine($"Found {field} in JSON: {dateStr} for key: {recordKey}");
                                    return dateStr;
                                }
                            }
                        }
                    }
                }
                catch { }

                // Method 3: Try reflection as last resort
                try
                {
                    var properties = record.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        if (prop.Name.ToLower().Contains("date") && !prop.Name.ToLower().Contains("time"))
                        {
                            var value = prop.GetValue(record);
                            if (value != null)
                            {
                                string dateStr = value.ToString();
                                if (!string.IsNullOrEmpty(dateStr) && dateStr != "N/A")
                                {
                                    System.Diagnostics.Debug.WriteLine($"Found {prop.Name} via reflection: {dateStr} for key: {recordKey}");
                                    return dateStr;
                                }
                            }
                        }
                    }
                }
                catch { }

                System.Diagnostics.Debug.WriteLine($"No date field found for key: {recordKey}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ExtractDateFromAnyRecordType for key {recordKey}: {ex.Message}");
                return null;
            }
        }
        private async void LoadFirebaseAttendanceData(DateTime? selectedDate = null)
        {
            if (isLoading) return;
            isLoading = true;

            try
            {
                // Clear the data collection (not the grid yet)
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
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show("No attendance records found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    return;
                }

                int processedCount = 0;

                // Load data into memory instead of directly to grid
                foreach (var attendanceRecord in attendanceRecords)
                {
                    if (attendanceRecord?.Object != null)
                    {
                        string firebaseKey = attendanceRecord.Key;

                        System.Diagnostics.Debug.WriteLine($"Processing record {firebaseKey}");

                        var rowData = await ProcessAttendanceRecordToData(attendanceRecord.Object, employeeDict, selectedDate, firebaseKey);
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


        private async Task<AttendanceRowData> ProcessAttendanceRecordToData(dynamic attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, string firebaseKey)
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

                // Try multiple approaches to extract the data
                try
                {
                    if (attendance is IDictionary<string, object> directDict)
                    {
                        attendanceDict = new Dictionary<string, object>(directDict);
                        System.Diagnostics.Debug.WriteLine("Used direct dictionary cast");
                    }
                    else
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(attendance);
                        attendanceDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        System.Diagnostics.Debug.WriteLine("Used JSON serialization approach");
                    }
                }
                catch (Exception serializationEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Serialization failed: {serializationEx.Message}");

                    try
                    {
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
                        System.Diagnostics.Debug.WriteLine("Used reflection approach");
                    }
                    catch (Exception reflectionEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Reflection also failed: {reflectionEx.Message}");
                        return null;
                    }
                }

                if (attendanceDict == null || !attendanceDict.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} resulted in empty dictionary");
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
                bool shouldInclude = true;

                System.Diagnostics.Debug.WriteLine($"Extracted - Employee: {employeeId}, Date: {attendanceDateStr}, TimeIn: {timeInStr}");

                // 🔹 PRIORITY 1: Check if CUT-OFF DATE FILTER is active
                if (currentAttendanceFilters.UseCutOffDate && currentAttendanceFilters.CutOffDate.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Using CUT-OFF DATE filter: {currentAttendanceFilters.CutOffDate.Value}, First Half: {currentAttendanceFilters.IsFirstHalf}");
                    shouldInclude = IsInBiMonthlyRange(attendanceDateStr,
                        currentAttendanceFilters.CutOffDate.Value,
                        currentAttendanceFilters.IsFirstHalf);

                    System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} date {attendanceDateStr}: shouldInclude = {shouldInclude}");
                }
                // 🔹 PRIORITY 2: If NO cut-off filter, use SINGLE DATE from DateTimePicker (default behavior)
                else if (!currentAttendanceFilters.UseCutOffDate && selectedDate.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Using SINGLE DATE filter: {selectedDate.Value}");
                    if (!string.IsNullOrEmpty(attendanceDateStr) && DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        shouldInclude = attendanceDate.Date == selectedDate.Value.Date;
                    }
                    else
                    {
                        shouldInclude = false;
                    }

                    System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} date {attendanceDateStr}: shouldInclude = {shouldInclude}");
                }
                // 🔹 FALLBACK: If neither filter is active, include everything
                else
                {
                    System.Diagnostics.Debug.WriteLine("No date filter active, including all records");
                    shouldInclude = true;
                }

                // If this record doesn't match the date filter, skip it
                if (!shouldInclude)
                {
                    return null;
                }

                // Process the data
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

                // Return data object
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


        private async Task<bool> ProcessAttendanceRecord(dynamic attendance, Dictionary<string, dynamic> employeeDict, DateTime? selectedDate, int counter, int rowIndex, string firebaseKey)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Processing record {firebaseKey} ===");

                // Handle the dynamic data more carefully
                Dictionary<string, object> attendanceDict = new Dictionary<string, object>();

                if (attendance == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} is NULL");
                    return false;
                }

                // Try multiple approaches to extract the data
                try
                {
                    // Approach 1: Direct cast to dictionary
                    if (attendance is IDictionary<string, object> directDict)
                    {
                        attendanceDict = new Dictionary<string, object>(directDict);
                        System.Diagnostics.Debug.WriteLine("Used direct dictionary cast");
                    }
                    // Approach 2: Use Newtonsoft.Json to convert
                    else
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(attendance);
                        attendanceDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        System.Diagnostics.Debug.WriteLine("Used JSON serialization approach");
                    }
                }
                catch (Exception serializationEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Serialization failed: {serializationEx.Message}");

                    // Approach 3: Manual extraction for common Firebase structure
                    try
                    {
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
                        System.Diagnostics.Debug.WriteLine("Used reflection approach");
                    }
                    catch (Exception reflectionEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Reflection also failed: {reflectionEx.Message}");
                        return false;
                    }
                }

                if (attendanceDict == null || !attendanceDict.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} resulted in empty dictionary");
                    return false;
                }

                // DEBUG: Show all keys and values
                System.Diagnostics.Debug.WriteLine($"Keys found: {string.Join(", ", attendanceDict.Keys)}");
                foreach (var key in attendanceDict.Keys)
                {
                    System.Diagnostics.Debug.WriteLine($"  {key}: {attendanceDict[key]?.ToString() ?? "NULL"}");
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

                System.Diagnostics.Debug.WriteLine($"Extracted - Employee: {employeeId}, Date: {attendanceDateStr}, TimeIn: {timeInStr}, TimeOut: {timeOutStr}, Status: {existingStatus}");

                // Apply date filter if a date is selected
                if (selectedDate.HasValue)
                {
                    bool shouldInclude = false;

                    if (!string.IsNullOrEmpty(attendanceDateStr) && DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
                    {
                        if (attendanceDate.Date == selectedDate.Value.Date)
                            shouldInclude = true;

                        System.Diagnostics.Debug.WriteLine($"Date filter: {attendanceDate.Date} == {selectedDate.Value.Date} = {shouldInclude}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not parse attendance date: {attendanceDateStr}");
                    }

                    if (!shouldInclude)
                    {
                        System.Diagnostics.Debug.WriteLine($"Record {firebaseKey} filtered out by date selection");
                        return false;
                    }
                }

                // Process the data
                string timeIn = FormatFirebaseTime(timeInStr);
                string timeOut = FormatFirebaseTime(timeOutStr);

                // FIXED: Call async method with correct parameters
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
                    // Safely access employee properties
                    string firstName = GetEmployeeProperty(employee, "first_name");
                    string middleName = GetEmployeeProperty(employee, "middle_name");
                    string lastName = GetEmployeeProperty(employee, "last_name");
                    fullName = $"{firstName} {middleName} {lastName}".Trim();
                    System.Diagnostics.Debug.WriteLine($"Found employee: {fullName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Employee {employeeId} not found in employee dictionary");
                }

                // DEBUG: Show final data that will be added to grid
                System.Diagnostics.Debug.WriteLine($"Final data - ID: {employeeId}, Name: {fullName}, Status: {status}, Hours: {hoursWorked}, Overtime: {overtime}");

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

                System.Diagnostics.Debug.WriteLine($"=== Successfully processed record {firebaseKey} ===\n");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ERROR processing record {firebaseKey}: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
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
                System.Diagnostics.Debug.WriteLine($"Error getting {key}: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"✓ Added row at index {newRowIndex} for employee {employeeId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to add row to DataGridView: {ex.Message}");
            }
        }

        // Helper method to safely get employee properties
        private string GetEmployeeProperty(dynamic employee, string propertyName)
        {
            try
            {
                // DEBUG: Check what type we're dealing with
                System.Diagnostics.Debug.WriteLine($"GetEmployeeProperty: Looking for {propertyName}, Employee type: {employee?.GetType()}");

                // Try multiple approaches to access the property

                // Approach 1: Direct dictionary access
                if (employee is IDictionary<string, object> employeeDict)
                {
                    if (employeeDict.ContainsKey(propertyName) && employeeDict[propertyName] != null)
                    {
                        string value = employeeDict[propertyName].ToString();
                        System.Diagnostics.Debug.WriteLine($"Found {propertyName} in dictionary: '{value}'");
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
                            System.Diagnostics.Debug.WriteLine($"Found {propertyName} via JSON: '{value}'");
                            return value;
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON approach failed for {propertyName}: {jsonEx.Message}");
                    }
                }

                // Approach 3: Reflection as last resort
                try
                {
                    var prop = employee.GetType().GetProperty(propertyName);
                    if (prop != null)
                    {
                        var value = prop.GetValue(employee)?.ToString() ?? "";
                        System.Diagnostics.Debug.WriteLine($"Found {propertyName} via reflection: '{value}'");
                        return value;
                    }
                }
                catch (Exception reflectionEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Reflection failed for {propertyName}: {reflectionEx.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"Property {propertyName} not found");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetEmployeeProperty for {propertyName}: {ex.Message}");
                return "";
            }
        }
        private async Task<(TimeSpan startTime, TimeSpan endTime)?> GetEmployeeScheduleForDay(string employeeId, string attendanceDate)
        {
            try
            {
                // Parse the attendance date to get day of week
                if (!DateTime.TryParse(attendanceDate, out DateTime date))
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse attendance date: {attendanceDate}");
                    return null;
                }

                string dayOfWeek = date.DayOfWeek.ToString();
                string normalizedTargetDay = NormalizeDayOfWeek(dayOfWeek);

                System.Diagnostics.Debug.WriteLine($"Looking for schedule: Employee {employeeId} on {dayOfWeek} ({normalizedTargetDay})");

                // Get all schedules from Firebase
                var scheduleRecords = await firebase.Child("Work_Schedule").OnceAsync<dynamic>();

                if (scheduleRecords == null || !scheduleRecords.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No schedule records found in Firebase");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Found {scheduleRecords.Count()} total schedule records");

                // First, try to find exact match for the day
                foreach (var scheduleRecord in scheduleRecords)
                {
                    if (scheduleRecord?.Object != null)
                    {
                        try
                        {
                            // Extract schedule data safely
                            string schedEmpId = GetScheduleProperty(scheduleRecord.Object, "employee_id");
                            string schedDay = GetScheduleProperty(scheduleRecord.Object, "day_of_week");

                            // Normalize the schedule day for comparison
                            string normalizedSchedDay = NormalizeDayOfWeek(schedDay);

                            System.Diagnostics.Debug.WriteLine($"  Checking schedule: EmpID={schedEmpId}, Day={schedDay} ({normalizedSchedDay})");

                            // Match employee ID and day of week
                            if (schedEmpId == employeeId && normalizedSchedDay == normalizedTargetDay)
                            {
                                string startTimeStr = GetScheduleProperty(scheduleRecord.Object, "start_time");
                                string endTimeStr = GetScheduleProperty(scheduleRecord.Object, "end_time");

                                System.Diagnostics.Debug.WriteLine($"✅ Found matching schedule for {employeeId} on {dayOfWeek}: {startTimeStr} - {endTimeStr}");

                                // Parse times - handle both HH:mm:ss and HH:mm formats
                                if (TryParseScheduleTime(startTimeStr, out TimeSpan startTime) &&
                                    TryParseScheduleTime(endTimeStr, out TimeSpan endTime))
                                {
                                    System.Diagnostics.Debug.WriteLine($"✅ Parsed times successfully: Start={startTime}, End={endTime}");
                                    return (startTime, endTime);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"❌ Failed to parse times: Start='{startTimeStr}', End='{endTimeStr}'");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing schedule record: {ex.Message}");
                        }
                    }
                }

                // If no exact match found, check if employee has any schedule at all
                System.Diagnostics.Debug.WriteLine($"No exact schedule match for {employeeId} on {dayOfWeek}, checking if employee has any schedule...");

                bool hasAnySchedule = scheduleRecords.Any(r =>
                {
                    string schedEmpId = GetScheduleProperty(r.Object, "employee_id");
                    return schedEmpId == employeeId;
                });

                if (!hasAnySchedule)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Employee {employeeId} has no schedule defined at all");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ℹ️ Employee {employeeId} has schedules but not for {dayOfWeek}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting schedule: {ex.Message}");
                return null;
            }
        }
        private string NormalizeDayOfWeek(string day)
        {
            if (string.IsNullOrEmpty(day)) return day;

            string normalized = day.Trim().ToLower();

            // Handle common abbreviations and variations
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
        private string CalculateStatusWithDefaultTimes(string timeInStr, string timeOutStr, string existingStatus = "")
        {
            try
            {
                DateTime timeIn, timeOut;

                bool timeInParsed = DateTime.TryParseExact(timeInStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInStr, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInStr, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParse(timeInStr, out timeIn);

                bool timeOutParsed = DateTime.TryParseExact(timeOutStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutStr, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutStr, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParse(timeOutStr, out timeOut);

                if (timeInParsed)
                {
                    TimeSpan expectedStart = new TimeSpan(8, 0, 0);  // 8:00 AM
                    TimeSpan expectedEnd = new TimeSpan(17, 0, 0);   // 5:00 PM

                    TimeSpan actualTimeIn = new TimeSpan(timeIn.Hour, timeIn.Minute, timeIn.Second);

                    bool isLate = actualTimeIn > expectedStart;
                    bool isEarlyOut = false;

                    if (timeOutParsed && timeOutStr != "N/A" && !string.IsNullOrEmpty(timeOutStr))
                    {
                        TimeSpan actualTimeOut = new TimeSpan(timeOut.Hour, timeOut.Minute, timeOut.Second);
                        isEarlyOut = actualTimeOut < expectedEnd;
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
                    return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalculateStatusWithDefaultTimes: {ex.Message}");
                return !string.IsNullOrEmpty(existingStatus) ? existingStatus : "Absent";
            }
        }
        private string GetScheduleProperty(dynamic scheduleObj, string propertyName)
        {
            try
            {
                // Try direct property access
                if (scheduleObj is IDictionary<string, object> dict)
                {
                    if (dict.ContainsKey(propertyName) && dict[propertyName] != null)
                        return dict[propertyName].ToString();
                }

                // Try JSON conversion approach
                try
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(scheduleObj);
                    var jsonDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (jsonDict != null && jsonDict.ContainsKey(propertyName) && jsonDict[propertyName] != null)
                        return jsonDict[propertyName].ToString();
                }
                catch { }

                // Try reflection as last resort
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
                System.Diagnostics.Debug.WriteLine($"Error getting property {propertyName}: {ex.Message}");
                return "";
            }
        }
        private bool TryParseScheduleTime(string timeStr, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (string.IsNullOrEmpty(timeStr))
                return false;

            // Clean the time string
            timeStr = timeStr.Trim();

            // Try standard TimeSpan parsing (handles HH:mm:ss)
            if (TimeSpan.TryParse(timeStr, out time))
                return true;

            // Try parsing as DateTime with various formats
            string[] timeFormats = {
        "HH:mm:ss",
        "HH:mm",
        "h:mm tt",
        "h:mm:ss tt",
        "H:mm",
        "H:mm:ss"
    };

            if (DateTime.TryParseExact(timeStr, timeFormats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dateTime))
            {
                time = dateTime.TimeOfDay;
                return true;
            }

            // Try parsing as simple time without seconds
            if (timeStr.Length == 4 && int.TryParse(timeStr, out int timeInt))
            {
                // Handle "0830" format
                int hours = timeInt / 100;
                int minutes = timeInt % 100;
                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                {
                    time = new TimeSpan(hours, minutes, 0);
                    return true;
                }
            }

            System.Diagnostics.Debug.WriteLine($" Could not parse time string: '{timeStr}'");
            return false;
        }
        private bool IsInBiMonthlyRange(string attendanceDateStr, DateTime cutOffDate, bool isFirstHalf)
        {
            if (string.IsNullOrEmpty(attendanceDateStr) || attendanceDateStr == "N/A")
                return false;

            if (DateTime.TryParse(attendanceDateStr, out DateTime attendanceDate))
            {
                if (isFirstHalf)
                {
                    // First half: 1st to 15th of the SAME MONTH as cut-off date
                    DateTime startDate = new DateTime(cutOffDate.Year, cutOffDate.Month, 1);
                    DateTime endDate = new DateTime(cutOffDate.Year, cutOffDate.Month, 15);
                    bool inRange = attendanceDate.Date >= startDate && attendanceDate.Date <= endDate;
                    System.Diagnostics.Debug.WriteLine($"First Half Check: {attendanceDate:yyyy-MM-dd} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd} = {inRange}");
                    return inRange;
                }
                else
                {
                    // Second half: 16th to end of month of the SAME MONTH as cut-off date
                    DateTime startDate = new DateTime(cutOffDate.Year, cutOffDate.Month, 16);
                    DateTime endDate = new DateTime(cutOffDate.Year, cutOffDate.Month, DateTime.DaysInMonth(cutOffDate.Year, cutOffDate.Month));
                    bool inRange = attendanceDate.Date >= startDate && attendanceDate.Date <= endDate;
                    System.Diagnostics.Debug.WriteLine($"Second Half Check: {attendanceDate:yyyy-MM-dd} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd} = {inRange}");
                    return inRange;
                }
            }

            return false;
        }

        private void buttonExportDTR_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmExportDTR confirmExportDTR = new ConfirmExportDTR();

            // Get the current date range text from the label
            string dateRange = labelAttendanceDate.Text;

            // Pass the DataGridView and date range to the export form
            confirmExportDTR.SetExportData(dataGridViewAttendance, dateRange);

            AttributesClass.ShowWithOverlay(parentForm, confirmExportDTR);
        }
    }

}