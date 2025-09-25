using System;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class FilterAdminEmployee : Form
    {
        // Events so AdminEmployee can subscribe
        public event Action<FilterCriteria> FiltersApplied;
        public event Action FiltersReset;

        // ADDED: Firebase client for ComboBox population
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public FilterAdminEmployee()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();

            // ADDED: Load departments and positions from Firebase
            LoadDepartmentsAndPositions();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxLastName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxFirstName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxDay, "Day");
            AttributesClass.TextboxPlaceholder(textBoxMonth, "Month");
            AttributesClass.TextboxPlaceholder(textBoxYear, "Year");
        }

        private void setFont()
        {
            try
            {
                buttonApply.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonReset.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                comboBoxDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxLastName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxFirstName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxDay.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMonth.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxYear.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContract.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelSort.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                checkBoxActive.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxFemale.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxIrregular.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxMale.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxMarried.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxNotActive.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxRegular.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxSingle.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        // ADDED: Load departments and positions from Firebase
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

                System.Diagnostics.Debug.WriteLine($"Admin Filter: Loaded {departments.Count} departments and {positions.Count} positions");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin Filter: Failed to load departments/positions: {ex.Message}");

                // Add default items if loading fails
                comboBoxDepartment.Items.Clear();
                comboBoxDepartment.Items.Add("Select department");
                comboBoxDepartment.SelectedIndex = 0;

                comboBoxPosition.Items.Clear();
                comboBoxPosition.Items.Add("Select position");
                comboBoxPosition.SelectedIndex = 0;
            }
        }

        // ADDED: Get individual records by index
        private async Task<Dictionary<string, (string Department, string Position)>> TryGetEmploymentInfoByIndex()
        {
            var employmentDict = new Dictionary<string, (string Department, string Position)>();

            try
            {
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
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin Filter: Index approach failed: {ex.Message}");
            }

            return employmentDict;
        }

        // ADDED: Manual parsing fallback
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
                System.Diagnostics.Debug.WriteLine($"Admin Filter: Manual parsing failed: {ex.Message}");
            }

            return employmentDict;
        }

        // ADDED: Helper method to get values safely
        private string GetValue(Dictionary<string, object> data, string key)
        {
            if (data != null && data.ContainsKey(key) && data[key] != null)
                return data[key].ToString();
            return "";
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            var filters = new FilterCriteria
            {
                FirstName = textBoxFirstName.Text.Trim(),
                LastName = textBoxLastName.Text.Trim(),
                Department = comboBoxDepartment.SelectedItem?.ToString() ?? "",
                Position = comboBoxPosition.SelectedItem?.ToString() ?? "",
                SortBy = comboBoxSort.SelectedItem?.ToString() ?? "",
                DateFilterType = comboBoxDate.SelectedItem?.ToString() ?? "",
                Day = textBoxDay.Text.Trim(),
                Month = textBoxMonth.Text.Trim(),
                Year = textBoxYear.Text.Trim(),
                StatusActive = checkBoxActive.Checked,
                StatusNotActive = checkBoxNotActive.Checked,
                GenderMale = checkBoxMale.Checked,
                GenderFemale = checkBoxFemale.Checked,
                MaritalMarried = checkBoxMarried.Checked,
                MaritalSingle = checkBoxSingle.Checked,
                ContractRegular = checkBoxRegular.Checked,
                ContractIrregular = checkBoxIrregular.Checked
            };

            FiltersApplied?.Invoke(filters);
            this.Close();
        }

        private void buttonReset_Click_1(object sender, EventArgs e)
        {
            FiltersReset?.Invoke();
            this.Close();
        }
    }
}