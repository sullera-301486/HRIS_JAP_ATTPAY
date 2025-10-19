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

namespace HRIS_JAP_ATTPAY
{
    public partial class FilterAdminLoan : Form
    {
        // Events so AdminLoan can subscribe
        public event Action<LoanFilterCriteria> FiltersApplied;
        public event Action FiltersReset;

        // Firebase client for ComboBox population
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public FilterAdminLoan()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();

            // Load departments, positions, and loan types from Firebase
            LoadDepartmentsAndPositions();
            LoadLoanTypes();

            // Initialize sorting options
            InitializeSortingOptions();
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
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLoanTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAmountMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAmountMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxBalanceMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxBalanceMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAmount.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBalance.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashC.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateRange.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLoanType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelSort.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxEndDate, "End date");
            AttributesClass.TextboxPlaceholder(textBoxAmountMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxAmountMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxID, "Search ID");
            AttributesClass.TextboxPlaceholder(textBoxName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxBalanceMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxBalanceMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxStartDate, "Start date");
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

                System.Diagnostics.Debug.WriteLine($"Loan Filter: Loaded {departments.Count} departments and {positions.Count} positions");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loan Filter: Failed to load departments/positions: {ex.Message}");

                // Add default items if loading fails
                comboBoxDepartment.Items.Clear();
                comboBoxDepartment.Items.Add("Select department");
                comboBoxDepartment.SelectedIndex = 0;

                comboBoxPosition.Items.Clear();
                comboBoxPosition.Items.Add("Select position");
                comboBoxPosition.SelectedIndex = 0;
            }
        }

        // Load loan types from Firebase
        private async void LoadLoanTypes()
        {
            try
            {
                var loanTypes = new HashSet<string>();

                // Get loan types from LoanTypes table
                var loanTypeRecords = await firebase.Child("LoanTypes").OnceAsync<dynamic>();

                foreach (var record in loanTypeRecords)
                {
                    try
                    {
                        string typeName = record.Object?.type_name?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            loanTypes.Add(typeName.Trim());
                        }
                    }
                    catch { }
                }

                // If no loan types found in LoanTypes table, get from EmployeeLoans
                if (loanTypes.Count == 0)
                {
                    var loanRecords = await firebase.Child("EmployeeLoans").OnceAsync<dynamic>();

                    foreach (var record in loanRecords)
                    {
                        try
                        {
                            string loanType = record.Object?.loan_type?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(loanType))
                            {
                                loanTypes.Add(loanType.Trim());
                            }
                        }
                        catch { }
                    }
                }

                // Populate Loan Type ComboBox
                comboBoxLoanTypeInput.Items.Clear();
                comboBoxLoanTypeInput.Items.Add("Select loan type");
                foreach (var type in loanTypes.OrderBy(t => t))
                {
                    comboBoxLoanTypeInput.Items.Add(type);
                }
                comboBoxLoanTypeInput.SelectedIndex = 0;

                System.Diagnostics.Debug.WriteLine($"Loan Filter: Loaded {loanTypes.Count} loan types");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loan Filter: Failed to load loan types: {ex.Message}");

                // Add default items if loading fails
                comboBoxLoanTypeInput.Items.Clear();
                comboBoxLoanTypeInput.Items.Add("Select loan type");
                comboBoxLoanTypeInput.SelectedIndex = 0;
            }
        }

        // Get individual records by index
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
                System.Diagnostics.Debug.WriteLine($"Loan Filter: Index approach failed: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Loan Filter: Manual parsing failed: {ex.Message}");
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

            if (value is string stringValue)
                return stringValue;

            if (value is Newtonsoft.Json.Linq.JValue jValue)
                return jValue.Value?.ToString() ?? "";

            if (value is Newtonsoft.Json.Linq.JToken jToken)
                return jToken.ToString();

            return value.ToString();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Reset button clicked in loan filter form");

            // Reset all filter controls
            textBoxID.Text = "";
            textBoxName.Text = "";
            textBoxStartDate.Text = "";
            textBoxEndDate.Text = "";
            textBoxAmountMinimum.Text = "";
            textBoxAmountMaximum.Text = "";
            textBoxBalanceMinimum.Text = "";
            textBoxBalanceMaximum.Text = "";

            // Reset ComboBoxes
            if (comboBoxDepartment.Items.Count > 0)
                comboBoxDepartment.SelectedIndex = 0;

            if (comboBoxPosition.Items.Count > 0)
                comboBoxPosition.SelectedIndex = 0;

            if (comboBoxLoanTypeInput.Items.Count > 0)
                comboBoxLoanTypeInput.SelectedIndex = 0;

            // Clear sort selection
            comboBoxSort.SelectedIndex = -1;

            // Trigger the reset event
            FiltersReset?.Invoke();
            this.Close();
        }

        // Initialize sorting options
        private void InitializeSortingOptions()
        {
            comboBoxSort.Items.Clear();

            // Add sorting options for loans (only A-Z and Z-A)
            comboBoxSort.Items.AddRange(new object[] {
                "A-Z",  // Sort by name A-Z
                "Z-A"   // Sort by name Z-A
            });

            // Don't set default selection - let it be empty initially
        }

        private void buttonApply_Click_1(object sender, EventArgs e)
        {
            var filters = new LoanFilterCriteria
            {
                EmployeeId = textBoxID.Text.Trim(),
                Name = textBoxName.Text.Trim(),
                Department = comboBoxDepartment.SelectedItem?.ToString() ?? "",
                Position = comboBoxPosition.SelectedItem?.ToString() ?? "",
                LoanType = comboBoxLoanTypeInput.SelectedItem?.ToString() ?? "",
                SortBy = comboBoxSort.SelectedItem?.ToString() ?? "",

                // Date range
                StartDate = textBoxStartDate.Text.Trim(),
                EndDate = textBoxEndDate.Text.Trim(),

                // Amount range
                AmountMinimum = textBoxAmountMinimum.Text.Trim(),
                AmountMaximum = textBoxAmountMaximum.Text.Trim(),

                // Balance range
                BalanceMinimum = textBoxBalanceMinimum.Text.Trim(),
                BalanceMaximum = textBoxBalanceMaximum.Text.Trim()
            };

            System.Diagnostics.Debug.WriteLine($"Loan Filter Apply: SortBy = '{filters.SortBy}'");
            FiltersApplied?.Invoke(filters);
            this.Close();
        }

        private void buttonReset_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Reset button clicked in loan filter form");

            // Reset all filter controls
            textBoxID.Text = "";
            textBoxName.Text = "";
            textBoxStartDate.Text = "";
            textBoxEndDate.Text = "";
            textBoxAmountMinimum.Text = "";
            textBoxAmountMaximum.Text = "";
            textBoxBalanceMinimum.Text = "";
            textBoxBalanceMaximum.Text = "";

            // Reset ComboBoxes
            if (comboBoxDepartment.Items.Count > 0)
                comboBoxDepartment.SelectedIndex = 0;

            if (comboBoxPosition.Items.Count > 0)
                comboBoxPosition.SelectedIndex = 0;

            if (comboBoxLoanTypeInput.Items.Count > 0)
                comboBoxLoanTypeInput.SelectedIndex = 0;

            // Clear sort selection
            comboBoxSort.SelectedIndex = -1;

            // Trigger the reset event
            FiltersReset?.Invoke();
            this.Close();
        }
    }

    // Filter criteria class for loans
    public class LoanFilterCriteria
    {
        public string EmployeeId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string LoanType { get; set; } = "";
        public string SortBy { get; set; } = "";

        // Date range
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";

        // Amount range
        public string AmountMinimum { get; set; } = "";
        public string AmountMaximum { get; set; } = "";

        // Balance range
        public string BalanceMinimum { get; set; } = "";
        public string BalanceMaximum { get; set; } = "";
    }

    // Row data class for loans
    public class LoanRowData
    {
        public string RowNumber { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string LoanType { get; set; }
        public string LoanAmount { get; set; }
        public string Balance { get; set; }
        public string Remarks { get; set; }
        public decimal LoanAmountValue { get; set; }
        public decimal BalanceValue { get; set; }
    }
}