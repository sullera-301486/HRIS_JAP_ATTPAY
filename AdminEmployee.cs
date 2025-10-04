using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminEmployee : UserControl
    {
        // Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Store original data and current filters
        private List<EmployeeData> allEmployees = new List<EmployeeData>();
        private FilterCriteria currentFilters = new FilterCriteria();
        private static AdminEmployee _instance;

        public AdminEmployee()
        {
            InitializeComponent();
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
            _instance = this;

            // Load Firebase data
            LoadFirebaseData();
        }

        // Helper class to store employee data
        private class EmployeeData
        {
            public string EmployeeId { get; set; }
            public string FullName { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public string Contact { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Gender { get; set; }
            public string MaritalStatus { get; set; }
            public string ContractType { get; set; }
            public DateTime? DateHired { get; set; }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddNewEmployee addNewEmployeeForm = new AddNewEmployee();
            AttributesClass.ShowWithOverlay(parentForm, addNewEmployeeForm);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminEmployee filterAdminEmployeeForm = new FilterAdminEmployee();

            // Subscribe to filter events
            filterAdminEmployeeForm.FiltersApplied += ApplyFilters;
            filterAdminEmployeeForm.FiltersReset += ResetFilters;

            AttributesClass.ShowWithOverlay(parentForm, filterAdminEmployeeForm);
        }

        private void ApplyFilters(FilterCriteria filters)
        {
            currentFilters = filters;

            // DEBUG: Added filter debugging
            System.Diagnostics.Debug.WriteLine("=== APPLYING FILTERS ===");
            System.Diagnostics.Debug.WriteLine($"Department: '{filters.Department}'");
            System.Diagnostics.Debug.WriteLine($"Position: '{filters.Position}'");
            System.Diagnostics.Debug.WriteLine($"StatusActive: {filters.StatusActive}");
            System.Diagnostics.Debug.WriteLine($"StatusNotActive: {filters.StatusNotActive}");
            System.Diagnostics.Debug.WriteLine($"GenderMale: {filters.GenderMale}");
            System.Diagnostics.Debug.WriteLine($"GenderFemale: {filters.GenderFemale}");
            System.Diagnostics.Debug.WriteLine($"MaritalMarried: {filters.MaritalMarried}");
            System.Diagnostics.Debug.WriteLine($"MaritalSingle: {filters.MaritalSingle}");
            System.Diagnostics.Debug.WriteLine($"ContractRegular: {filters.ContractRegular}");
            System.Diagnostics.Debug.WriteLine($"ContractIrregular: {filters.ContractIrregular}");
            System.Diagnostics.Debug.WriteLine("========================");

            ApplyAllFilters();
        }

        private void ResetFilters()
        {
            currentFilters = new FilterCriteria();
            ApplyAllFilters();
        }

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
                    emp.Email,
                    Properties.Resources.ExpandRight
                );
                counter++;
            }
        }

        private bool MatchesSearchText(EmployeeData emp, string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || searchText == "find employee")
                return true;

            return (emp.FullName?.ToLower().Contains(searchText) ?? false) ||
                   (emp.EmployeeId?.ToLower().Contains(searchText) ?? false) ||
                   (emp.Department?.ToLower().Contains(searchText) ?? false) ||
                   (emp.Position?.ToLower().Contains(searchText) ?? false) ||
                   (emp.Email?.ToLower().Contains(searchText) ?? false);
        }

        // ONLY CHANGED METHOD - Fixed filtering logic
        private bool MatchesFilterCriteria(EmployeeData emp)
        {
            // DEBUG: Added employee-level debugging
            System.Diagnostics.Debug.WriteLine($"Filtering employee: {emp.FullName} - Dept: '{emp.Department}' - Pos: '{emp.Position}'");
            System.Diagnostics.Debug.WriteLine($"Filter Dept: '{currentFilters.Department}' - Filter Pos: '{currentFilters.Position}'");

            // Employee ID filter (optional if filter admin provides it)
            if (!string.IsNullOrEmpty(currentFilters.EmployeeId))
            {
                if (!(emp.EmployeeId?.ToLower().Contains(currentFilters.EmployeeId.ToLower()) ?? false))
                {
                    System.Diagnostics.Debug.WriteLine($"Employee ID filter failed for {emp.FullName}");
                    return false;
                }
            }

            // Name filters - FIXED
            if (!string.IsNullOrEmpty(currentFilters.FirstName) &&
                currentFilters.FirstName.Trim().ToLower() != "search name")
            {
                string searchFirstName = currentFilters.FirstName.Trim().ToLower();
                if (!(emp.FullName?.ToLower().Contains(searchFirstName) ?? false))
                {
                    System.Diagnostics.Debug.WriteLine($"First name filter failed for {emp.FullName}");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(currentFilters.LastName) &&
                currentFilters.LastName.Trim().ToLower() != "search name")
            {
                string searchLastName = currentFilters.LastName.Trim().ToLower();
                if (!(emp.FullName?.ToLower().Contains(searchLastName) ?? false))
                {
                    System.Diagnostics.Debug.WriteLine($"Last name filter failed for {emp.FullName}");
                    return false;
                }
            }

            // MAJOR FIX: Department filter - Added check to exclude "Select department"
            if (!string.IsNullOrEmpty(currentFilters.Department) &&
                !currentFilters.Department.Equals("Select department", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(emp.Department) ||
                    !emp.Department.Equals(currentFilters.Department, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Department filter failed for {emp.FullName}: emp='{emp.Department}' vs filter='{currentFilters.Department}'");
                    return false;
                }
            }

            // MAJOR FIX: Position filter - Added check to exclude "Select position"
            if (!string.IsNullOrEmpty(currentFilters.Position) &&
                !currentFilters.Position.Equals("Select position", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(emp.Position) ||
                    !emp.Position.Equals(currentFilters.Position, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Position filter failed for {emp.FullName}: emp='{emp.Position}' vs filter='{currentFilters.Position}'");
                    return false;
                }
            }

            // Status filter - Added debugging
            if (currentFilters.StatusActive || currentFilters.StatusNotActive)
            {
                bool statusMatch = false;
                string empStatus = emp.Status ?? "Active";

                if (currentFilters.StatusActive && empStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    statusMatch = true;
                if (currentFilters.StatusNotActive && (empStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ||
                                                       empStatus.Equals("Not Active", StringComparison.OrdinalIgnoreCase)))
                    statusMatch = true;

                if (!statusMatch)
                {
                    System.Diagnostics.Debug.WriteLine($"Status filter failed for {emp.FullName}: emp='{empStatus}' active={currentFilters.StatusActive} inactive={currentFilters.StatusNotActive}");
                    return false;
                }
            }

            // Gender filter - Added debugging
            if (currentFilters.GenderMale || currentFilters.GenderFemale)
            {
                bool genderMatch = false;
                if (currentFilters.GenderMale && (emp.Gender?.Equals("Male", StringComparison.OrdinalIgnoreCase) ?? false))
                    genderMatch = true;
                if (currentFilters.GenderFemale && (emp.Gender?.Equals("Female", StringComparison.OrdinalIgnoreCase) ?? false))
                    genderMatch = true;

                if (!genderMatch)
                {
                    System.Diagnostics.Debug.WriteLine($"Gender filter failed for {emp.FullName}: emp='{emp.Gender}' male={currentFilters.GenderMale} female={currentFilters.GenderFemale}");
                    return false;
                }
            }

            // Marital status filter - Added debugging
            if (currentFilters.MaritalMarried || currentFilters.MaritalSingle)
            {
                bool maritalMatch = false;
                if (currentFilters.MaritalMarried && (emp.MaritalStatus?.Equals("Married", StringComparison.OrdinalIgnoreCase) ?? false))
                    maritalMatch = true;
                if (currentFilters.MaritalSingle && (emp.MaritalStatus?.Equals("Single", StringComparison.OrdinalIgnoreCase) ?? false))
                    maritalMatch = true;

                if (!maritalMatch)
                {
                    System.Diagnostics.Debug.WriteLine($"Marital filter failed for {emp.FullName}: emp='{emp.MaritalStatus}' married={currentFilters.MaritalMarried} single={currentFilters.MaritalSingle}");
                    return false;
                }
            }

            // Contract type filter - Added debugging
            if (currentFilters.ContractRegular || currentFilters.ContractIrregular)
            {
                bool contractMatch = false;
                if (currentFilters.ContractRegular && (emp.ContractType?.Equals("Regular", StringComparison.OrdinalIgnoreCase) ?? false))
                    contractMatch = true;
                if (currentFilters.ContractIrregular &&
                   ((emp.ContractType?.Equals("Irregular", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (emp.ContractType?.Equals("Probationary", StringComparison.OrdinalIgnoreCase) ?? false)))
                {
                    contractMatch = true;
                }

                if (!contractMatch)
                {
                    System.Diagnostics.Debug.WriteLine($"Contract filter failed for {emp.FullName}: emp='{emp.ContractType}' regular={currentFilters.ContractRegular} irregular={currentFilters.ContractIrregular}");
                    return false;
                }
            }

            // Date filter - Added debugging for date issues
            if (!string.IsNullOrEmpty(currentFilters.DateFilterType) && !currentFilters.DateFilterType.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                if (!emp.DateHired.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Date filter failed for {emp.FullName}: no date hired");
                    return false;
                }
                DateTime dt = emp.DateHired.Value.Date;
                DateTime now = DateTime.Now.Date;

                switch (currentFilters.DateFilterType.ToLower())
                {
                    case "today":
                        if (dt != now) return false;
                        break;
                    case "this week":
                        var firstDay = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                        int diff = (7 + (int)now.DayOfWeek - (int)firstDay) % 7;
                        DateTime startOfWeek = now.AddDays(-diff).Date;
                        DateTime endOfWeek = startOfWeek.AddDays(6).Date;
                        if (!(dt >= startOfWeek && dt <= endOfWeek)) return false;
                        break;
                    case "this month":
                        if (dt.Year != now.Year || dt.Month != now.Month) return false;
                        break;
                    case "this year":
                        if (dt.Year != now.Year) return false;
                        break;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Employee {emp.FullName} PASSED all filters");
            return true;
        }

        private List<EmployeeData> ApplySorting(List<EmployeeData> employees)
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
                    // Sort by DateHired (newest first) then by EmployeeId as fallback
                    return employees.OrderByDescending(e => e.DateHired ?? DateTime.MinValue)
                                   .ThenByDescending(e => e.EmployeeId)
                                   .ToList();

                case "oldest-newest":
                    // Sort by DateHired (oldest first) then by EmployeeId as fallback
                    return employees.OrderBy(e => e.DateHired ?? DateTime.MaxValue)
                                   .ThenBy(e => e.EmployeeId)
                                   .ToList();

                default:
                    return employees;
            }
        }

        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            ApplyAllFilters();
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
            dataGridViewEmployee.CellMouseEnter += dataGridViewEmployee_CellMouseEnter;
            dataGridViewEmployee.CellMouseLeave += dataGridViewEmployee_CellMouseLeave;
            dataGridViewEmployee.CellClick += dataGridViewEmployee_CellClick;

            // Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // Leftmost: Counter column (narrower)
            var counterCol = new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 36
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 84 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 108 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Contact", HeaderText = "Contact Number", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 110 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148 });

            // Rightmost: Image column (narrower)
            var actionCol = new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 23
            };

            actionCol.Image = Properties.Resources.ExpandRight;
            dataGridViewEmployee.Columns.Add(actionCol);
        }

        private void setFont()
        {
            try
            {
                labelAdminEmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelEmployeeDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelAddEmployee.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
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

        private void dataGridViewEmployee_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
                dataGridViewEmployee.Cursor = Cursors.Hand;
        }

        private void dataGridViewEmployee_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewEmployee.Cursor = Cursors.Default;
        }

        private async void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
            {
                string employeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();

                if (string.IsNullOrEmpty(employeeId))
                    return;

                // Check if employee is a USER (has record in Users node)
                bool isUser = await CheckIfEmployeeIsUser(employeeId);

                Form parentForm = this.FindForm();
                Form profileForm;

                if (isUser)
                {
                    profileForm = new EmployeeProfile(employeeId);
                }
                else
                {
                    profileForm = new EmployeeProfileHR(employeeId);
                }

                AttributesClass.ShowWithOverlay(parentForm, profileForm);
            }
        }

        private async Task<bool> CheckIfEmployeeIsUser(string employeeId)
        {
            try
            {
                // Get all users from Firebase
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<Dictionary<string, object>>();

                // Check if any user has matching employee_id
                foreach (var user in users)
                {
                    var userData = user.Object as Dictionary<string, object>;
                    if (userData != null)
                    {
                        string userEmployeeId = GetValue(userData, "employee_id");
                        if (userEmployeeId == employeeId)
                        {
                            return true; // Employee is a USER
                        }
                    }
                }

                return false; // No user record found
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking user status: {ex.Message}");
                return false; // Default to EmployeeProfileHR on error
            }
        }

        // FIXED: Handle corrupted EmploymentInfo data with multiple fallback methods
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
                var employmentDict = new Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)>();

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
                        string gender = GetValue(data, "gender");
                        string maritalStatus = GetValue(data, "marital_status");

                        // Get employment info
                        string department = "";
                        string position = "";
                        string status = "Active";
                        string contractType = "";
                        DateTime? dateHired = null;

                        if (!string.IsNullOrEmpty(employeeId) && employmentDict.ContainsKey(employeeId))
                        {
                            var employmentInfo = employmentDict[employeeId];
                            department = employmentInfo.Department;
                            position = employmentInfo.Position;
                            status = employmentInfo.Status;
                            contractType = employmentInfo.ContractType;
                            dateHired = employmentInfo.DateHired;
                        }

                        var employee = new EmployeeData
                        {
                            EmployeeId = employeeId,
                            FullName = FormatFullName(firstName, middleName, lastName),
                            Department = department,
                            Position = position,
                            Contact = contact,
                            Email = email,
                            Gender = gender,
                            MaritalStatus = maritalStatus,
                            Status = status,
                            ContractType = contractType,
                            DateHired = dateHired
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

                // Apply current filters after loading data
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
        private async Task<Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)>> TryGetEmploymentInfoByIndex()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)>();

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
                                string status = GetValue(record, "status");
                                string contractType = GetValue(record, "contract_type");

                                DateTime? dateHired = null;
                                string dateStr = GetValue(record, "date_of_joining");
                                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime hiredDate))
                                    dateHired = hiredDate;

                                employmentDict[empId] = (dept, pos, status, contractType, dateHired);
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
        private async Task<Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)>> TryManualEmploymentInfoParsing()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)>();

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
                // Pattern to match employee_id, department, and position
                var pattern = @"employee_id['""]?:['""]([^'""]+)['""][^}]*?department['""]?:['""]([^'""]*)[^}]*?position['""]?:['""]([^'""]*)";
                var matches = Regex.Matches(rawData, pattern, RegexOptions.Singleline);

                System.Diagnostics.Debug.WriteLine($"Found {matches.Count} matches with regex");

                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 4)
                    {
                        string empId = match.Groups[1].Value.Trim();
                        string dept = match.Groups[2].Value.Trim();
                        string pos = match.Groups[3].Value.Trim();

                        if (!string.IsNullOrEmpty(empId))
                        {
                            employmentDict[empId] = (dept, pos, "Active", "", null);
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
        private Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)> ExtractEmploymentInfoFromString(string rawData)
        {
            var employmentDict = new Dictionary<string, (string Department, string Position, string Status, string ContractType, DateTime? DateHired)>();

            try
            {
                // Look for JAP employee IDs in the string
                var employeeIdMatches = Regex.Matches(rawData, @"JAP-\d+", RegexOptions.IgnoreCase);

                foreach (Match empIdMatch in employeeIdMatches)
                {
                    string empId = empIdMatch.Value;
                    string dept = "";
                    string pos = "";

                    // Extract text around the employee ID to find department and position
                    int startIndex = Math.Max(0, empIdMatch.Index - 200);
                    int length = Math.Min(400, rawData.Length - startIndex);
                    string context = rawData.Substring(startIndex, length);

                    // Look for department and position near the employee ID
                    var deptMatch = Regex.Match(context, @"department['""]?:['""]([^'""]+)", RegexOptions.IgnoreCase);
                    if (deptMatch.Success) dept = deptMatch.Groups[1].Value;

                    var posMatch = Regex.Match(context, @"position['""]?:['""]([^'""]+)", RegexOptions.IgnoreCase);
                    if (posMatch.Success) pos = posMatch.Groups[1].Value;

                    if (!string.IsNullOrEmpty(empId))
                    {
                        employmentDict[empId] = (dept, pos, "Active", "", null);
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

        private string FormatFullName(string firstName, string middleName, string lastName)
        {
            string first = firstName ?? "";
            string middle = middleName ?? "";
            string last = lastName ?? "";

            string fullName = $"{first} {middle} {last}".Trim();
            while (fullName.Contains("  "))
                fullName = fullName.Replace("  ", " ");

            return fullName;
        }

        public void RefreshData()
        {
            LoadFirebaseData();
        }
        public static void RefreshEmployeeData()
        {
            _instance?.RefreshData();
        }
    }

    public class FilterCriteria
    {
        public string EmployeeId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string DateFilterType { get; set; } = "";
        public string SortBy { get; set; } = "";
        public string Day { get; set; } = "";
        public string Month { get; set; } = "";
        public string Year { get; set; } = "";
        public bool StatusActive { get; set; } = false;
        public bool StatusNotActive { get; set; } = false;
        public bool GenderMale { get; set; } = false;
        public bool GenderFemale { get; set; } = false;
        public bool MaritalMarried { get; set; } = false;
        public bool MaritalSingle { get; set; } = false;
        public bool ContractRegular { get; set; } = false;
        public bool ContractIrregular { get; set; } = false;
    }
}