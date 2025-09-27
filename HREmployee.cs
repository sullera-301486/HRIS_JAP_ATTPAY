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
    public partial class HREmployee : UserControl
    {
        // Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // ADDED: Store original data and current filters (like AdminEmployee)
        private List<HREmployeeData> allEmployees = new List<HREmployeeData>();
        private HRFilterCriteria currentFilters = new HRFilterCriteria();

        // ADDED: Helper class to store employee data
        private class HREmployeeData
        {
            public string EmployeeId { get; set; }
            public string FullName { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string Contact { get; set; }
            public string Email { get; set; }
        }

        public HREmployee()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();

            // Add search filter (like AdminEmployee)
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            // Load data from Firebase
            LoadFirebaseData();
        }

        private void HREmployee_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterHREmployee filterHREmployeeForm = new FilterHREmployee();

            // ADDED: Subscribe to filter events
            filterHREmployeeForm.FiltersApplied += ApplyFilters;
            filterHREmployeeForm.FiltersReset += ResetFilters;

            AttributesClass.ShowWithOverlay(parentForm, filterHREmployeeForm);
        }

        // ADDED: Filter event handlers
        private void ApplyFilters(HRFilterCriteria filters)
        {
            currentFilters = filters;
            ApplyAllFilters();
        }

        private void ResetFilters()
        {
            currentFilters = new HRFilterCriteria();
            ApplyAllFilters();
        }

        // ADDED: Apply all filters method
        private void ApplyAllFilters()
        {
            dataGridViewEmployee.Rows.Clear();

            string searchText = textBoxSearchEmployee.Text.Trim().ToLower();

            var filteredEmployees = allEmployees
                .Where(emp => MatchesSearchText(emp, searchText) && MatchesFilterCriteria(emp))
                .ToList();

            // Apply sorting
            filteredEmployees = ApplySorting(filteredEmployees);

            // Populate grid with filtered data
            int counter = 1;
            foreach (var emp in filteredEmployees)
            {
                dataGridViewEmployee.Rows.Add(
                    counter,
                    emp.EmployeeId,
                    emp.FullName,
                    emp.Department,
                    emp.Position,
                    emp.Contact,
                    emp.Email
                );
                counter++;
            }
        }

        // ADDED: Search text matching
        private bool MatchesSearchText(HREmployeeData emp, string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || searchText == "find employee")
                return true;

            return (emp.FullName?.ToLower().Contains(searchText) ?? false) ||
                   (emp.EmployeeId?.ToLower().Contains(searchText) ?? false) ||
                   (emp.Department?.ToLower().Contains(searchText) ?? false) ||
                   (emp.Position?.ToLower().Contains(searchText) ?? false) ||
                   (emp.Email?.ToLower().Contains(searchText) ?? false);
        }

        // ADDED: Filter criteria matching
        private bool MatchesFilterCriteria(HREmployeeData emp)
        {
            // Employee ID filter
            if (!string.IsNullOrEmpty(currentFilters.EmployeeId) &&
                currentFilters.EmployeeId.Trim().ToLower() != "search id")
            {
                string searchId = currentFilters.EmployeeId.Trim().ToLower();
                if (!(emp.EmployeeId?.ToLower().Contains(searchId) ?? false))
                    return false;
            }

            // Name filter
            if (!string.IsNullOrEmpty(currentFilters.Name) &&
                currentFilters.Name.Trim().ToLower() != "search name")
            {
                string searchName = currentFilters.Name.Trim().ToLower();
                if (!(emp.FullName?.ToLower().Contains(searchName) ?? false))
                    return false;
            }

            // Department filter
            if (!string.IsNullOrEmpty(currentFilters.Department) &&
                currentFilters.Department != "Select department")
            {
                if (!(emp.Department?.Equals(currentFilters.Department, StringComparison.OrdinalIgnoreCase) ?? false))
                    return false;
            }

            // Position filter
            if (!string.IsNullOrEmpty(currentFilters.Position) &&
                currentFilters.Position != "Select position")
            {
                if (!(emp.Position?.Equals(currentFilters.Position, StringComparison.OrdinalIgnoreCase) ?? false))
                    return false;
            }

            return true;
        }

        // ADDED: Apply sorting
        private List<HREmployeeData> ApplySorting(List<HREmployeeData> employees)
        {
            if (string.IsNullOrEmpty(currentFilters.SortBy))
            {
                return employees;
            }

            string sort = currentFilters.SortBy.ToLower();

            switch (sort)
            {
                case "a-z":
                    return employees.OrderBy(e => e.FullName).ToList();

                case "z-a":
                    return employees.OrderByDescending(e => e.FullName).ToList();

                case "newest-oldest":
                    // Sort by EmployeeId (newest first) - assuming higher IDs are newer
                    return employees.OrderByDescending(e => ExtractNumericId(e.EmployeeId))
                                   .ThenByDescending(e => e.EmployeeId)
                                   .ToList();

                case "oldest-newest":
                    // Sort by EmployeeId (oldest first) - assuming lower IDs are older
                    return employees.OrderBy(e => ExtractNumericId(e.EmployeeId))
                                   .ThenBy(e => e.EmployeeId)
                                   .ToList();

                default:
                    return employees;
            }
        }
        private int ExtractNumericId(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return 0;

            // Extract numbers from ID (e.g., "JAP-001" -> 1)
            var match = System.Text.RegularExpressions.Regex.Match(employeeId, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int numericId))
                return numericId;

            return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This table is for viewing only. Profiles cannot be opened from here.",
                "View Only", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewEmployee.ReadOnly = true;
            dataGridViewEmployee.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewEmployee.MultiSelect = false;
            dataGridViewEmployee.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewEmployee.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewEmployee.DefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewEmployee.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewEmployee.GridColor = Color.White;
            dataGridViewEmployee.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewEmployee.ColumnHeadersHeight = 40;
            dataGridViewEmployee.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewEmployee.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            // Disable row clickable behavior (view only)
            dataGridViewEmployee.ClearSelection();
            dataGridViewEmployee.Enabled = true;

            // Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // Counter column
            var counterCol = new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Contact", HeaderText = "Contact Number", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
        }

        private void setFont()
        {
            labelHREmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelEmployeeDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
            textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterHREmployee filterHREmployeeForm = new FilterHREmployee();

            // ADDED: Subscribe to filter events
            filterHREmployeeForm.FiltersApplied += ApplyFilters;
            filterHREmployeeForm.FiltersReset += ResetFilters;

            AttributesClass.ShowWithOverlay(parentForm, filterHREmployeeForm);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ManualAttendanceRequest editAttendanceForm = new ManualAttendanceRequest();
            AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest leaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestForm);
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        // MODIFIED: Search filter now uses ApplyAllFilters
        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
        }

        // MODIFIED: Load Firebase Data with multiple fallback methods
        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();
                allEmployees.Clear();

                // Get EmployeeDetails (this should work fine)
                var firebaseEmployees = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<Dictionary<string, object>>();

                System.Diagnostics.Debug.WriteLine($"Found {firebaseEmployees.Count} employee details");

                // Try multiple approaches to get EmploymentInfo
                var employmentDict = new Dictionary<string, (string Department, string Position)>();

                // Approach 1: Try to get individual records by index
                employmentDict = await TryGetEmploymentInfoByIndex();

                // Approach 2: If first approach failed, try manual parsing
                if (employmentDict.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Trying manual parsing approach...");
                    employmentDict = await TryManualEmploymentInfoParsing();
                }

                System.Diagnostics.Debug.WriteLine($"Found {employmentDict.Count} employment records");

                // Process each employee
                foreach (var fbEmp in firebaseEmployees)
                {
                    try
                    {
                        var data = fbEmp.Object as Dictionary<string, object>;
                        if (data == null) continue;

                        // Extract employee data
                        string employeeId = GetValue(data, "employee_id");
                        string firstName = GetValue(data, "first_name");
                        string middleName = GetValue(data, "middle_name");
                        string lastName = GetValue(data, "last_name");
                        string contact = GetValue(data, "contact");
                        string email = GetValue(data, "email");

                        // Get employment info
                        string department = "";
                        string position = "";

                        if (!string.IsNullOrEmpty(employeeId) && employmentDict.ContainsKey(employeeId))
                        {
                            var employmentInfo = employmentDict[employeeId];
                            department = employmentInfo.Department;
                            position = employmentInfo.Position;
                        }

                        var employee = new HREmployeeData
                        {
                            EmployeeId = employeeId,
                            FullName = FormatFullName(firstName, middleName, lastName),
                            Department = department,
                            Position = position,
                            Contact = contact,
                            Email = email
                        };

                        allEmployees.Add(employee);

                        System.Diagnostics.Debug.WriteLine($"Added employee {employeeId}: {department} - {position}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipped employee record: {ex.Message}");
                        continue;
                    }
                }

                // Apply filters after loading data
                ApplyAllFilters();

                System.Diagnostics.Debug.WriteLine($"Loaded {allEmployees.Count} employees total");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Firebase data: " + ex.Message +
                               "\n\nPlease check your internet connection and try again.",
                               "Data Load Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        // Approach 1: Get individual records by index to avoid corrupted array
        private async Task<Dictionary<string, (string Department, string Position)>> TryGetEmploymentInfoByIndex()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
                // Try indices 1 through 10 (since index 0 is null)
                for (int i = 1; i <= 10; i++)
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
                                System.Diagnostics.Debug.WriteLine($"Index {i}: Found employment for {empId}");
                            }
                        }
                    }
                    catch
                    {
                        // Stop when we can't find more records
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Index approach failed: {ex.Message}");
            }

            return employmentDict;
        }

        // Approach 2: Manual parsing of corrupted JSON response
        private async Task<Dictionary<string, (string Department, string Position)>> TryManualEmploymentInfoParsing()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
                // Get the raw response as string
                var response = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<object>();

                if (response == null || !response.Any()) return employmentDict;

                // Convert to string for manual parsing
                string rawData = response.First()?.Object?.ToString() ?? "";

                if (string.IsNullOrEmpty(rawData)) return employmentDict;

                System.Diagnostics.Debug.WriteLine($"Raw data length: {rawData.Length}");

                // Use regex to extract employee records from corrupted JSON
                var pattern = @"employee_id['""]?:['""]([^'""]+)['""][^}]*?department['""]?:['""]([^'""]*)[^}]*?position['""]?:['""]([^'""]*)";
                var matches = System.Text.RegularExpressions.Regex.Matches(rawData, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);

                System.Diagnostics.Debug.WriteLine($"Found {matches.Count} matches with regex");

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Groups.Count >= 4)
                    {
                        string empId = match.Groups[1].Value.Trim();
                        string dept = match.Groups[2].Value.Trim();
                        string pos = match.Groups[3].Value.Trim();

                        if (!string.IsNullOrEmpty(empId))
                        {
                            employmentDict[empId] = (dept, pos);
                            System.Diagnostics.Debug.WriteLine($"Manual parse: {empId} -> {dept} / {pos}");
                        }
                    }
                }

                // If regex failed, try simple string searching
                if (employmentDict.Count == 0)
                {
                    employmentDict = ExtractEmploymentInfoFromString(rawData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Manual parsing failed: {ex.Message}");
            }

            return employmentDict;
        }

        // Fallback: Simple string extraction
        private Dictionary<string, (string Department, string Position)> ExtractEmploymentInfoFromString(string rawData)
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
                // Look for JAP employee IDs in the string
                var employeeIdMatches = System.Text.RegularExpressions.Regex.Matches(rawData, @"JAP-\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (System.Text.RegularExpressions.Match empIdMatch in employeeIdMatches)
                {
                    string empId = empIdMatch.Value;
                    string dept = "";
                    string pos = "";

                    // Extract text around the employee ID to find department and position
                    int startIndex = Math.Max(0, empIdMatch.Index - 200);
                    int length = Math.Min(400, rawData.Length - startIndex);
                    string context = rawData.Substring(startIndex, length);

                    // Look for department and position near the employee ID
                    var deptMatch = System.Text.RegularExpressions.Regex.Match(context, @"department['""]?:['""]([^'""]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (deptMatch.Success) dept = deptMatch.Groups[1].Value;

                    var posMatch = System.Text.RegularExpressions.Regex.Match(context, @"position['""]?:['""]([^'""]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (posMatch.Success) pos = posMatch.Groups[1].Value;

                    if (!string.IsNullOrEmpty(empId))
                    {
                        employmentDict[empId] = (dept, pos);
                        System.Diagnostics.Debug.WriteLine($"String extraction: {empId} -> {dept} / {pos}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"String extraction failed: {ex.Message}");
            }

            return employmentDict;
        }

        // Helper method to get values safely from dictionary
        private string GetValue(Dictionary<string, object> data, string key)
        {
            if (data != null && data.ContainsKey(key) && data[key] != null)
                return data[key].ToString();
            return "";
        }

        // Helper method to extract employee ID dynamically
        private string ExtractEmployeeId(dynamic empData, string firebaseKey)
        {
            // Priority 1: Check for employee_id in the data
            string empId = empData.employee_id ?? empData.EmployeeId ?? "";

            // Priority 2: If no employee_id in data, check if key is an employee ID format
            if (string.IsNullOrEmpty(empId))
            {
                // Check if the Firebase key matches employee ID pattern (like "JAP-001")
                if (!string.IsNullOrEmpty(firebaseKey) && firebaseKey.StartsWith("JAP-"))
                {
                    empId = firebaseKey;
                }
                // If key is numeric, this might be an array index, so we need to rely on data
                else if (int.TryParse(firebaseKey, out _))
                {
                    // This is likely an array index record, we'll use the employee_id from data
                    // If empId is still empty, this record will be skipped
                }
            }

            return empId?.Trim() ?? "";
        }

        // Helper method to format full name safely
        private string FormatFullName(string firstName, string middleName, string lastName)
        {
            string first = firstName ?? "";
            string middle = middleName ?? "";
            string last = lastName ?? "";

            // Clean up extra spaces
            string fullName = $"{first} {middle} {last}".Trim();
            while (fullName.Contains("  "))
                fullName = fullName.Replace("  ", " ");

            return fullName;
        }
    }

    // ADDED: Filter criteria class for HR Employee
    public class HRFilterCriteria
    {
        public string EmployeeId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string DateFilterType { get; set; } = "";
        public string SortBy { get; set; } = "";
    }
}