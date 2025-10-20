using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace HRIS_JAP_ATTPAY
{
    public partial class FilterAdminAttendance : Form
    {
        // Events so AdminAttendance can subscribe
        public event Action<AttendanceFilterCriteria> FiltersApplied;
        public event Action FiltersReset;


        // Firebase client for ComboBox population
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public FilterAdminAttendance()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();

            // Load departments and positions from Firebase
            LoadDepartmentsAndPositions();

            // Initialize sorting options with only A-Z and Z-A based on first name
            InitializeSortingOptions();

            // ADD THIS: Initialize bi-monthly range
            InitializeBiMonthlyRange();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonApply.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonReset.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelHoursWorked.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelSort.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                checkBoxAboveTwoHours.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxAbsent.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxBelowEightHours.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxEarlyOut.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxEightHours.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxLate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxOneHour.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxPresent.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                dtpCutOffSelector.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxID, "Search ID");
            AttributesClass.TextboxPlaceholder(textBoxName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxTimeIn, "Select time");
            AttributesClass.TextboxPlaceholder(textBoxTimeOut, "Select time");
        }

        // Load departments and positions from Firebase
        private async void LoadDepartmentsAndPositions()
        {
            try
            {
                var departments = new HashSet<string>();
                var positions = new HashSet<string>();

                // Try to get employment info using multiple approaches
                var employmentDict = await TryGetEmploymentInfoByIndex();

                // If first approach failed, try manual parsing
                if (employmentDict.Count == 0)
                {
                    employmentDict = await TryManualEmploymentInfoParsing();
                }

                // Extract unique departments and positions
                foreach (var employment in employmentDict.Values)
                {
                    if (!string.IsNullOrEmpty(employment.Department) && employment.Department.Trim() != "")
                    {
                        departments.Add(employment.Department.Trim());
                    }
                    if (!string.IsNullOrEmpty(employment.Position) && employment.Position.Trim() != "")
                    {
                        positions.Add(employment.Position.Trim());
                    }
                }

                // Populate Department ComboBox
                comboBoxDepartment.Items.Clear();
                comboBoxDepartment.Items.Add("Select department");
                foreach (var dept in departments.OrderBy(d => d))
                {
                    comboBoxDepartment.Items.Add(dept);
                }
                comboBoxDepartment.SelectedIndex = 0;

                // Populate Position ComboBox
                comboBoxPosition.Items.Clear();
                comboBoxPosition.Items.Add("Select position");
                foreach (var pos in positions.OrderBy(p => p))
                {
                    comboBoxPosition.Items.Add(pos);
                }
                comboBoxPosition.SelectedIndex = 0;

                System.Diagnostics.Debug.WriteLine($"Attendance Filter: Loaded {departments.Count} departments and {positions.Count} positions");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Attendance Filter: Failed to load departments/positions: {ex.Message}");

                // Add default items if loading fails
                comboBoxDepartment.Items.Clear();
                comboBoxDepartment.Items.Add("Select department");
                comboBoxDepartment.SelectedIndex = 0;

                comboBoxPosition.Items.Clear();
                comboBoxPosition.Items.Add("Select position");
                comboBoxPosition.SelectedIndex = 0;
            }
        }

        // Get individual records by index
        private async Task<Dictionary<string, (string Department, string Position)>> TryGetEmploymentInfoByIndex()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
                for (int i = 1; i <= 20; i++) // Increased from 10 to 20 to match AdminAttendance
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
                            // If we hit a null record, we might have reached the end
                            break;
                        }
                    }
                    catch
                    {
                        // If we get consecutive failures, break
                        if (i > 5 && employmentDict.Count == 0)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Attendance Filter: Index approach failed: {ex.Message}");
            }

            return employmentDict;
        }

        // Manual parsing fallback
        private async Task<Dictionary<string, (string Department, string Position)>> TryManualEmploymentInfoParsing()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
                var response = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<object>();

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
                        {
                            employmentDict[empId] = (dept, pos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Attendance Filter: Manual parsing failed: {ex.Message}");
            }

            return employmentDict;
        }

        // Helper method to get values safely
        private string GetValue(Dictionary<string, object> data, string key)
        {
            if (data == null) return "";

            if (!data.ContainsKey(key)) return "";

            var value = data[key];
            if (value == null) return "";

            // Handle different data types that Firebase might return
            if (value is string stringValue)
                return stringValue;

            if (value is Newtonsoft.Json.Linq.JValue jValue)
                return jValue.Value?.ToString() ?? "";

            if (value is Newtonsoft.Json.Linq.JToken jToken)
                return jToken.ToString();

            // For other types, convert to string
            return value.ToString();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            // Check if user actually selected a cut-off date (format changed from blank)
            bool hasCutOffDate = dtpCutOffSelector.CustomFormat != " ";

            var filters = new AttendanceFilterCriteria
            {
                EmployeeId = textBoxID.Text.Trim(),
                Name = textBoxName.Text.Trim(),
                Department = comboBoxDepartment.SelectedItem?.ToString() ?? "",
                Position = comboBoxPosition.SelectedItem?.ToString() ?? "",
                SortBy = comboBoxSort.SelectedItem?.ToString() ?? "",
                TimeIn = textBoxTimeIn.Text.Trim(),
                TimeOut = textBoxTimeOut.Text.Trim(),

                // Status filters
                StatusPresent = checkBoxPresent.Checked,
                StatusAbsent = checkBoxAbsent.Checked,
                StatusLate = checkBoxLate.Checked,
                StatusEarlyOut = checkBoxEarlyOut.Checked,

                // Hours worked filters
                HoursEight = checkBoxEightHours.Checked,
                HoursBelowEight = checkBoxBelowEightHours.Checked,

                // Overtime filters
                OvertimeOneHour = checkBoxOneHour.Checked,
                OvertimeTwoHoursPlus = checkBoxAboveTwoHours.Checked,

                // Cut-off date - ONLY set if user selected one
                UseCutOffDate = hasCutOffDate,
                CutOffDate = hasCutOffDate ? (DateTime?)dtpCutOffSelector.Value : null,
                IsFirstHalf = hasCutOffDate && dtpCutOffSelector.Value.Day <= 15
            };

            System.Diagnostics.Debug.WriteLine($"Filter Apply: UseCutOffDate = {filters.UseCutOffDate}, CutOffDate = {filters.CutOffDate}");
            FiltersApplied?.Invoke(filters);
            this.Close();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Reset button clicked in filter form");

            // Reset all filter controls
            textBoxID.Text = "";
            textBoxName.Text = "";

            if (comboBoxDepartment.Items.Count > 0)
                comboBoxDepartment.SelectedIndex = 0;

            if (comboBoxPosition.Items.Count > 0)
                comboBoxPosition.SelectedIndex = 0;

            comboBoxSort.SelectedIndex = -1;

            textBoxTimeIn.Text = "";
            textBoxTimeOut.Text = "";

            checkBoxPresent.Checked = false;
            checkBoxAbsent.Checked = false;
            checkBoxLate.Checked = false;
            checkBoxEarlyOut.Checked = false;
            checkBoxEightHours.Checked = false;
            checkBoxBelowEightHours.Checked = false;
            checkBoxOneHour.Checked = false;
            checkBoxAboveTwoHours.Checked = false;

            // Reset DateTimePicker back to blank
            dtpCutOffSelector.Format = DateTimePickerFormat.Custom;
            dtpCutOffSelector.CustomFormat = " ";
            dtpCutOffSelector.Value = DateTime.Today;

            FiltersReset?.Invoke();
            this.Close();
        }

        // FIXED: Initialize sorting options with only A-Z and Z-A (based on first name initial)
        private void InitializeSortingOptions()
        {
            comboBoxSort.Items.Clear();

            // Only include A-Z and Z-A sorting options
            comboBoxSort.Items.AddRange(new object[] {
                "A-Z",  // Sort by first name A-Z
                "Z-A"   // Sort by first name Z-A
            });

            // Don't set default selection - let it be empty initially
        }
        private void InitializeBiMonthlyRange()
        {
            // Leave the DateTimePicker empty/null by default
            dtpCutOffSelector.Format = DateTimePickerFormat.Custom;
            dtpCutOffSelector.CustomFormat = " "; // Shows as blank
            dtpCutOffSelector.Value = DateTime.Today;

            // Show date format when user clicks/selects
            dtpCutOffSelector.ValueChanged += (s, e) => {
                if (dtpCutOffSelector.Focused || dtpCutOffSelector.CustomFormat == " ")
                {
                    dtpCutOffSelector.Format = DateTimePickerFormat.Custom;
                    dtpCutOffSelector.CustomFormat = "yyyy-MM-dd";
                }
            };
        }
    }

    // Filter criteria class for attendance
    public class AttendanceFilterCriteria
    {
        public string EmployeeId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string DateFilterType { get; set; } = "";
        public string SortBy { get; set; } = "";
        public string TimeIn { get; set; } = "";
        public string TimeOut { get; set; } = "";

        // Status filters
        public bool StatusPresent { get; set; } = false;
        public bool StatusAbsent { get; set; } = false;
        public bool StatusLate { get; set; } = false;
        public bool StatusEarlyOut { get; set; } = false;

        // Hours worked filters
        public bool HoursEight { get; set; } = false;
        public bool HoursBelowEight { get; set; } = false;

        // Overtime filters
        public bool OvertimeOneHour { get; set; } = false;
        public bool OvertimeTwoHoursPlus { get; set; } = false;
        public bool UseCutOffDate { get; set; } = false;
        public DateTime? CutOffDate { get; set; } = null;
        public bool IsFirstHalf { get; set; } = true;
    }
    public class AttendanceRowData
    {
        public string RowNumber { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string HoursWorked { get; set; }
        public string Status { get; set; }
        public string OvertimeHours { get; set; }
        public string VerificationMethod { get; set; }
        public string AttendanceDate { get; set; }
        public string FirebaseKey { get; set; }
    }
}