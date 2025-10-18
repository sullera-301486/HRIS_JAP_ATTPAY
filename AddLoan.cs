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
using System.Net.Http;

namespace HRIS_JAP_ATTPAY
{
    // Helper class to store employee information
   

    public partial class AddLoan : Form
    {
        private string currentEmployeeId;
        private static readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public AddLoan(string employeeId)
        {
            InitializeComponent();
            setFont();
            currentEmployeeId = employeeId;

            // Wire up event handlers
            comboBoxNameInput.SelectedIndexChanged += comboBoxNameInput_SelectedIndexChanged;
            textBoxAmountInput.TextChanged += textBoxAmountInput_TextChanged;
            textBoxEndDateInput.TextChanged += textBoxEndDateInput_TextChanged;
            labelStartDateInput.TextChanged += labelStartDateInput_TextChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Load data from Firebase
            InitializeFormAsync();
        }

        private async void InitializeFormAsync()
        {
            try
            {
                // Disable controls while loading
                DisableControls();

                // Load all active employees
                await LoadAllActiveEmployees();

                // Load loan types from cloud
                await LoadLoanTypes();

                // Set the current employee as selected
                SetSelectedEmployee(currentEmployeeId);

                // Enable controls after loading
                EnableControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadAllActiveEmployees()
        {
            try
            {
                var employees = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<dynamic>();

                var activeEmployees = new List<Employee>();

                foreach (var employee in employees)
                {
                    if (employee.Object != null)
                    {
                        // Check if employee is active (adjust the status field name as needed)
                        string status = employee.Object.status?.ToString() ?? "Active";

                        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                        {
                            string employeeId = employee.Key;
                            string firstName = employee.Object.first_name?.ToString() ?? "";
                            string middleName = employee.Object.middle_name?.ToString() ?? "";
                            string lastName = employee.Object.last_name?.ToString() ?? "";
                            string fullName = $"{firstName} {middleName} {lastName}".Trim();

                            activeEmployees.Add(new Employee
                            {
                                EmployeeId = employeeId,
                                FullName = fullName
                            });
                        }
                    }
                }

                // Update UI on main thread
                comboBoxNameInput.Items.Clear();
                comboBoxNameInput.DisplayMember = "FullName";
                comboBoxNameInput.ValueMember = "EmployeeId";

                foreach (var emp in activeEmployees.OrderBy(e => e.FullName))
                {
                    comboBoxNameInput.Items.Add(emp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetSelectedEmployee(string employeeId)
        {
            try
            {
                // Make sure we're on the UI thread
                if (comboBoxNameInput.InvokeRequired)
                {
                    comboBoxNameInput.Invoke(new Action(() => SetSelectedEmployee(employeeId)));
                    return;
                }

                foreach (Employee emp in comboBoxNameInput.Items)
                {
                    if (emp.EmployeeId == employeeId)
                    {
                        comboBoxNameInput.SelectedItem = emp;
                        labelIDInput.Text = emp.EmployeeId;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting selected employee: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadLoanTypes()
        {
            try
            {
                var loanTypes = await firebase
                    .Child("LoanTypes")
                    .OnceAsync<dynamic>();

                comboBoxTypeInput.Items.Clear();

                foreach (var loanType in loanTypes)
                {
                    if (loanType.Object != null && loanType.Object.type_name != null)
                    {
                        // Only add loan types that are available/active
                        string status = loanType.Object.status?.ToString() ?? "Active";

                        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                            status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                        {
                            comboBoxTypeInput.Items.Add(loanType.Object.type_name.ToString());
                        }
                    }
                }

                if (comboBoxTypeInput.Items.Count > 0)
                {
                    comboBoxTypeInput.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No active loan types available!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loan types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisableControls()
        {
            comboBoxTypeInput.Enabled = false;
            textBoxAmountInput.Enabled = false;
            labelStartDateInput.Enabled = false;
            textBoxEndDateInput.Enabled = false;
            labelTotalPaymentInput.Enabled = false;
            buttonAdd.Enabled = false;
            comboBoxNameInput.Enabled = false;
        }

        private void EnableControls()
        {
            comboBoxTypeInput.Enabled = true;
            textBoxAmountInput.Enabled = true;
            labelStartDateInput.Enabled = true;
            textBoxEndDateInput.Enabled = true;
            labelTotalPaymentInput.Enabled = false; // Calculated field
            buttonAdd.Enabled = true;
            comboBoxNameInput.Enabled = true;

            // Make bi-monthly amortization read-only (calculated field)
            labelBimonthlyAmortizationAMOUNT.Enabled = false;

            // Initialize start date with today's date if empty
            if (string.IsNullOrWhiteSpace(labelStartDateInput.Text))
            {
                labelStartDateInput.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        // Event handler when employee selection changes
        private void comboBoxNameInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxNameInput.SelectedItem is Employee selectedEmployee)
            {
                labelIDInput.Text = selectedEmployee.EmployeeId;
                currentEmployeeId = selectedEmployee.EmployeeId;
            }
        }

        // Optional helper method to clear calculations
        private void textBoxEndDateInput_TextChanged(object sender, EventArgs e)
        {
            CalculateAmortizations();
        }

        private void labelStartDateInput_TextChanged(object sender, EventArgs e)
        {
            CalculateAmortizations();
        }

        private void CalculateAmortizations()
        {
            try
            {
                // Get loan amount
                if (!decimal.TryParse(textBoxAmountInput.Text, out decimal loanAmount) || loanAmount <= 0)
                {
                    return;
                }

                // Get start date and end date
                if (!DateTime.TryParse(labelStartDateInput.Text, out DateTime startDate))
                {
                    return;
                }

                if (!DateTime.TryParse(textBoxEndDateInput.Text, out DateTime endDate))
                {
                    return;
                }

                // Validate that end date is after start date
                if (endDate <= startDate)
                {
                    return;
                }

                // Calculate number of months between start and end date
                int totalMonths = ((endDate.Year - startDate.Year) * 12) + (endDate.Month - startDate.Month);

                if (totalMonths <= 0)
                {
                    return;
                }

                // Calculate total payment terms (2 payments per month = bi-monthly)
                int totalPaymentTerms = totalMonths * 2;

                // Calculate monthly amortization
                decimal monthlyAmortization = loanAmount / totalMonths;

                // Calculate bi-monthly amortization (50% of monthly)
                decimal biMonthlyAmortization = monthlyAmortization / 2;

                // Update UI
                labelTotalPaymentInput.Text = totalPaymentTerms.ToString();
                labelMonthlyAmortizationInput.Text = $"₱{monthlyAmortization:N2}";
                labelBimonthlyAmortizationAMOUNT.Text = $"₱{biMonthlyAmortization:N2}";
                labelBalanceInput.Text = $"₱{loanAmount:N2}";
            }
            catch (Exception ex)
            {
                // Silent fail for calculation
            }
        }

        // Add this event handler to recalculate when amount or terms change
        private void textBoxAmountInput_TextChanged(object sender, EventArgs e)
        {
            CalculateAmortizations();
        }

        private void labelTotalPaymentInput_TextChanged(object sender, EventArgs e)
        {
            CalculateAmortizations();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (!ValidateInputs())
                {
                    return;
                }

                // Get values
                string loanType = comboBoxTypeInput.SelectedItem?.ToString();
                decimal loanAmount = decimal.Parse(textBoxAmountInput.Text);
                string startDate = labelStartDateInput.Text;
                string endDate = textBoxEndDateInput.Text;
                int paymentTerms = int.Parse(labelTotalPaymentInput.Text);

                // Calculate amortizations
                decimal biMonthlyAmortization = loanAmount / paymentTerms;
                decimal monthlyAmortization = biMonthlyAmortization * 2;

                string employeeName = comboBoxNameInput.Text;

                // Show confirmation dialog with full details
                using (ConfirmAddLoan confirmAddLoan = new ConfirmAddLoan())
                {

                    confirmAddLoan.StartPosition = FormStartPosition.CenterParent;
                    var result = confirmAddLoan.ShowDialog(this);

                    // If user confirms, THEN update cloud
                    if (result == DialogResult.OK)
                    {
                        // Update cloud here, not in ConfirmAddLoan
                        await AddLoanToFirebaseAsync(currentEmployeeId, employeeName, loanType, loanAmount,
                            startDate, endDate, paymentTerms, monthlyAmortization, biMonthlyAmortization);

                        // Show success message and close
                        MessageBox.Show("Loan successfully added to cloud database!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in loan process: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Validate loan type
            if (comboBoxTypeInput.SelectedItem == null)
            {
                MessageBox.Show("Please select a loan type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate loan amount
            if (!decimal.TryParse(textBoxAmountInput.Text, out decimal loanAmount) || loanAmount <= 0)
            {
                MessageBox.Show("Please enter a valid loan amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxAmountInput.Focus();
                return false;
            }

            // Validate start date
            if (string.IsNullOrWhiteSpace(labelStartDateInput.Text) || !DateTime.TryParse(labelStartDateInput.Text, out DateTime startDate))
            {
                MessageBox.Show("Please enter a valid start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate end date
            if (string.IsNullOrWhiteSpace(textBoxEndDateInput.Text) || !DateTime.TryParse(textBoxEndDateInput.Text, out DateTime endDate))
            {
                MessageBox.Show("Please enter a valid end date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxEndDateInput.Focus();
                return false;
            }

            // Validate that end date is after start date
            if (endDate <= startDate)
            {
                MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxEndDateInput.Focus();
                return false;
            }

            return true;
        }

        // UPDATED METHOD - Following Console App Logic for Array Indexing
        private async Task<int> GetNextLoanArrayIndex()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmployeeLoans.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        // If empty or null, start from 0
                        if (string.IsNullOrEmpty(json) || json == "null")
                        {
                            return 0;
                        }

                        var loansArray = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(json);

                        if (loansArray == null || loansArray.Count == 0)
                        {
                            return 0;
                        }

                        // Count non-null entries and find the highest index - FOLLOWING CONSOLE APP LOGIC
                        int maxIndex = -1;
                        int nonNullCount = 0;

                        for (int i = 0; i < loansArray.Count; i++)
                        {
                            if (loansArray[i] != null && loansArray[i].Type != JTokenType.Null)
                            {
                                nonNullCount++;
                                maxIndex = i;
                            }
                        }

                        // Return the next index after the highest found
                        return maxIndex + 1;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting next loan array index: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        private async Task AddLoanToFirebaseAsync(string employeeId, string employeeName, string loanType,
     decimal loanAmount, string startDate, string endDate, int totalPaymentTerms,
     decimal monthlyAmortization, decimal biMonthlyAmortization)
        {
            try
            {
                // Get the next array index for cloud storage
                int nextArrayIndex = await GetNextLoanArrayIndex();

                // Create loan object for cloud storage
                var loanObj = new
                {
                    employee_id = employeeId,
                    employee_name = employeeName,
                    loan_type = loanType,
                    loan_amount = loanAmount,
                    balance = loanAmount, // Initial balance equals loan amount
                    start_date = startDate,
                    end_date = endDate,
                    total_payment_terms = totalPaymentTerms,
                    monthly_amortization = monthlyAmortization,
                    bi_monthly_amortization = biMonthlyAmortization,
                    total_payment_done = 0,
                    status = "Active",
                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Add to EmployeeLoans array in cloud at the calculated index
                await firebase
                    .Child("EmployeeLoans")
                    .Child(nextArrayIndex.ToString())
                    .PutAsync(loanObj);

                // Add admin log to cloud
                await AddAdminLog("Loan Added", employeeId, employeeName,
                    $"Added {loanType} of ₱{loanAmount:N2} for {employeeName}");

                Console.WriteLine($"Loan successfully added to cloud at index: {nextArrayIndex}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding loan to Firebase: {ex.Message}");
            }
        }

        private async Task AddAdminLog(string actionType, string targetEmployeeId, string targetEmployeeName, string description)
        {
            try
            {
                // Default admin info - you can modify this to get from logged-in user
                string adminEmployeeId = "JAP-001";
                string adminName = "System Administrator";
                string adminUserId = "101";

                var adminLog = new
                {
                    action_type = actionType,
                    admin_employee_id = adminEmployeeId,
                    admin_name = adminName,
                    admin_user_id = adminUserId,
                    description = description,
                    details = $"Employee ID: {targetEmployeeId}, Employee Name: {targetEmployeeName}",
                    target_employee_id = targetEmployeeId,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase.Child("AdminLogs").PostAsync(adminLog);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - admin log is not critical
                System.Diagnostics.Debug.WriteLine($"Error adding admin log: {ex.Message}");
            }
        }

        private void setFont()
        {
            try
            {
                labelAddLoan.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelAmount.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBalanceInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelBalance.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxAmountInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelBimonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBimonthlyAmortizationAMOUNT.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEndDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxEndDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                comboBoxNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDetailsFor.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStartDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStartDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelTotalPayment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTotalPaymentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                comboBoxTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}