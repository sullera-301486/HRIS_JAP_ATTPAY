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
    public partial class LoanDetailsEdit : Form
    {
        private readonly string employeeId;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private List<Dictionary<string, object>> allEmployeeLoans = new List<Dictionary<string, object>>();
        private Dictionary<string, string> loanKeys = new Dictionary<string, string>();
        private class ExistingLoanInfo
        {
            public int LoanIndex { get; set; }
            public string CreatedAt { get; set; }
            public decimal CurrentBalance { get; set; }
            public int CurrentPaymentDone { get; set; }
        }
        public LoanDetailsEdit(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    throw new ArgumentException("Employee ID cannot be null or empty", nameof(employeeId));
                }

                InitializeComponent();
                this.employeeId = employeeId;
                setFont();

                // Safe event handler wiring
                if (textBoxEndDateInput != null)
                    textBoxEndDateInput.TextChanged += textBoxEndDateInput_TextChanged;

                if (comboBoxSelectedInput != null)
                    comboBoxSelectedInput.SelectedIndexChanged += comboBoxSelectedInput_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Form constructor error: {ex.Message}");
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override async void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                await LoadLoanDetails();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Form load error: {ex.Message}");
                MessageBox.Show($"Error loading form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadLoanDetails()
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show("Employee ID is missing.");
                    return;
                }

                await LoadEmployeeDetails();
                await LoadAndDisplayEmployeeLoans();
                await PopulateLoanComboBox();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load loan details error: {ex.Message}");
                MessageBox.Show("Failed to load loan details: " + ex.Message);
            }
        }

        private async Task LoadEmployeeDetails()
        {
            try
            {
                var employeeResponse = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (employeeResponse != null)
                {
                    dynamic employee = employeeResponse;

                    SafeSetLabelText(labelIDInput, employee?.employee_id?.ToString());

                    // Format full name matching LoanDetails logic
                    string firstName = employee?.first_name?.ToString() ?? "";
                    string middleName = employee?.middle_name?.ToString() ?? "";
                    string lastName = employee?.last_name?.ToString() ?? "";
                    string fullName = BuildFullName(firstName, middleName, lastName);

                    SafeSetLabelText(labelNameInput, fullName);
                }
                else
                {
                    MessageBox.Show("Employee not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee details: {ex.Message}");
                MessageBox.Show("Error loading employee details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildFullName(string firstName, string middleName, string lastName)
        {
            var nameParts = new List<string> { firstName, middleName, lastName }
                .Where(part => !string.IsNullOrWhiteSpace(part) && part != "N/A")
                .Select(part => part.Trim());

            string fullName = string.Join(" ", nameParts);
            return string.IsNullOrWhiteSpace(fullName) ? "Unknown Employee" : fullName;
        }

        private async Task LoadAndDisplayEmployeeLoans()
        {
            try
            {
                Console.WriteLine($"Loading updated loans from Firebase for employee: {employeeId}");

                var loansData = await firebase.Child("EmployeeLoans").OnceSingleAsync<object>();
                allEmployeeLoans.Clear();
                loanKeys.Clear();

                if (loansData != null)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(loansData);
                    Console.WriteLine($"Loading loans JSON: {json.Substring(0, Math.Min(200, json.Length))}...");

                    if (json.Trim().StartsWith("["))
                    {
                        // Array structure
                        var jArray = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(json);
                        if (jArray != null)
                        {
                            for (int i = 0; i < jArray.Count; i++)
                            {
                                var item = jArray[i];
                                if (item?.Type == JTokenType.Null) continue;

                                var loanDict = new Dictionary<string, object>();
                                foreach (JProperty property in item.Children<JProperty>())
                                    loanDict[property.Name] = property.Value?.ToString() ?? "";

                                if (loanDict.ContainsKey("employee_id"))
                                {
                                    string loanEmployeeId = loanDict["employee_id"]?.ToString();
                                    string loanType = GetSafeValue(loanDict, "loan_type");

                                    Console.WriteLine($"Checking loan for display: Type={loanType}, EmployeeID='{loanEmployeeId}'");

                                    if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                        loanEmployeeId.Trim().Equals(employeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        allEmployeeLoans.Add(loanDict);
                                        loanKeys[loanType] = i.ToString(); // Store array index as key
                                        Console.WriteLine($"✅ Adding to display: {loanType}");
                                    }
                                }
                            }
                        }
                    }
                    else if (json.Trim().StartsWith("{"))
                    {
                        // Object structure
                        var jObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                        if (jObject != null)
                        {
                            foreach (var property in jObject.Properties())
                            {
                                if (property.Value.Type == JTokenType.Object)
                                {
                                    var loanDict = new Dictionary<string, object>();
                                    foreach (JProperty prop in property.Value.Children<JProperty>())
                                        loanDict[prop.Name] = prop.Value?.ToString() ?? "";

                                    if (loanDict.ContainsKey("employee_id"))
                                    {
                                        string loanEmployeeId = loanDict["employee_id"]?.ToString();
                                        string loanType = GetSafeValue(loanDict, "loan_type");

                                        if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                            loanEmployeeId.Trim().Equals(employeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                                        {
                                            allEmployeeLoans.Add(loanDict);
                                            loanKeys[loanType] = property.Name; // Store property name as key
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Total loans found for display: {allEmployeeLoans.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee loans: {ex.Message}");
                allEmployeeLoans.Clear();
            }
        }

        private async Task PopulateLoanComboBox()
        {
            try
            {
                if (comboBoxSelectedInput == null)
                {
                    System.Diagnostics.Debug.WriteLine("ComboBox is null, cannot populate");
                    return;
                }

                if (comboBoxSelectedInput.InvokeRequired)
                {
                    comboBoxSelectedInput.Invoke(new Action(async () => await PopulateLoanComboBox()));
                    return;
                }

                comboBoxSelectedInput.Items.Clear();

                if (allEmployeeLoans?.Any() == true)
                {
                    foreach (var loan in allEmployeeLoans)
                    {
                        string loanType = GetSafeValue(loan, "loan_type");
                        string loanId = GetSafeValue(loan, "loan_id");

                        if (!string.IsNullOrEmpty(loanType))
                        {
                            string displayText = string.IsNullOrEmpty(loanId) || loanId == "N/A"
                                ? loanType
                                : $"{loanType} (ID: {loanId})";

                            comboBoxSelectedInput.Items.Add(displayText);
                        }
                    }

                    if (comboBoxSelectedInput.Items.Count > 0)
                    {
                        comboBoxSelectedInput.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("No valid loans found for this employee.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("No existing loans found for this employee.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error populating combo box: {ex.Message}");
                MessageBox.Show("Error loading loan types.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxSelectedInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender == null || comboBoxSelectedInput?.SelectedItem == null) return;

                int index = comboBoxSelectedInput.SelectedIndex;
                if (index >= 0 && index < allEmployeeLoans.Count)
                {
                    var selectedLoan = allEmployeeLoans[index];
                    LoadSelectedLoanDetails(selectedLoan);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Combo box selection error: {ex.Message}");
            }
        }

        private void LoadSelectedLoanDetails(Dictionary<string, object> loanData)
        {
            try
            {
                if (loanData == null)
                {
                    System.Diagnostics.Debug.WriteLine("Loan data is null");
                    return;
                }

                // Set loan details (read-only except end date)
                string loanType = GetSafeValue(loanData, "loan_type");
                SafeSetLabelText(labelTypeInput, loanType);

                // Amount - matching LoanDetails format
                if (decimal.TryParse(GetSafeValue(loanData, "loan_amount"), out decimal loanAmount))
                    SafeSetLabelText(labelAmountInput, $"₱ {loanAmount:N2}");
                else
                    SafeSetLabelText(labelAmountInput, "₱ 0.00");

                // Balance - matching LoanDetails format
                if (decimal.TryParse(GetSafeValue(loanData, "balance"), out decimal balance))
                    SafeSetLabelText(labelBalanceInput, $"₱ {balance:N2}");
                else
                    SafeSetLabelText(labelBalanceInput, "₱ 0.00");

                // Dates - matching LoanDetails format
                SafeSetLabelText(labelStartDateInput, FormatDate(GetSafeValue(loanData, "start_date")));
                textBoxEndDateInput.Text = GetSafeValue(loanData, "end_date"); // Editable field

                // Payment information
                int paymentDone = 0;
                int paymentTerms = 0;

                if (!int.TryParse(GetSafeValue(loanData, "total_payment_done"), out paymentDone))
                    paymentDone = 0;

                if (!int.TryParse(GetSafeValue(loanData, "total_payment_terms"), out paymentTerms))
                    paymentTerms = 0;

                SafeSetLabelText(labelTotalPaymentInput, $"{paymentDone}/{paymentTerms}");

                // Amortizations - matching LoanDetails format
                if (decimal.TryParse(GetSafeValue(loanData, "monthly_amortization"), out decimal monthly))
                    SafeSetLabelText(labelMonthlyAmortizationInput, $"₱ {monthly:N2}");
                else
                    SafeSetLabelText(labelMonthlyAmortizationInput, "₱ 0.00");

                if (decimal.TryParse(GetSafeValue(loanData, "bi_monthly_amortization"), out decimal biMonthly))
                    SafeSetLabelText(labelBimonthlyAmortizationInput, $"₱ {biMonthly:N2}");
                else
                    SafeSetLabelText(labelBimonthlyAmortizationInput, "₱ 0.00");

                // Make only end date editable
                SetFieldsEditable(false);
                if (textBoxEndDateInput != null)
                    textBoxEndDateInput.Enabled = true;

                Console.WriteLine($"✅ Successfully displayed UI for {loanType} loan");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading loan details: {ex.Message}");
                DisplayNoLoans();
            }
        }

        private void DisplayNoLoans()
        {
            SafeSetLabelText(labelTypeInput, "N/A");
            SafeSetLabelText(labelAmountInput, "₱ 0.00");
            SafeSetLabelText(labelBalanceInput, "₱ 0.00");
            SafeSetLabelText(labelStartDateInput, "N/A");
            if (textBoxEndDateInput != null)
                textBoxEndDateInput.Text = "";
            SafeSetLabelText(labelTotalPaymentInput, "0/0");
            SafeSetLabelText(labelMonthlyAmortizationInput, "₱ 0.00");
            SafeSetLabelText(labelBimonthlyAmortizationInput, "₱ 0.00");
        }

        private string FormatDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
                return date.ToString("MMM dd, yyyy");
            return dateString;
        }

        private void SetFieldsEditable(bool editable)
        {
            try
            {
                // Null check each control before modifying
                if (labelTypeInput != null) labelTypeInput.Enabled = editable;
                if (labelAmountInput != null) labelAmountInput.Enabled = editable;
                if (labelBalanceInput != null) labelBalanceInput.Enabled = editable;
                if (labelStartDateInput != null) labelStartDateInput.Enabled = editable;
                if (labelMonthlyAmortizationInput != null) labelMonthlyAmortizationInput.Enabled = editable;
                if (labelBimonthlyAmortizationInput != null) labelBimonthlyAmortizationInput.Enabled = editable;
                if (labelTotalPaymentInput != null) labelTotalPaymentInput.Enabled = editable;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting fields editable: {ex.Message}");
            }
        }

        private void textBoxEndDateInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender == null) return;
                RecalculateAmortizations();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Text changed event error: {ex.Message}");
            }
        }

        private void RecalculateAmortizations()
        {
            try
            {
                if (comboBoxSelectedInput?.SelectedItem == null) return;

                int index = comboBoxSelectedInput.SelectedIndex;
                if (index < 0 || index >= allEmployeeLoans.Count) return;

                var loan = allEmployeeLoans[index];
                if (loan == null) return;

                // Get original loan amount and start date with null checks
                if (!decimal.TryParse(GetSafeValue(loan, "loan_amount"), out decimal loanAmount) || loanAmount <= 0)
                    return;

                if (!DateTime.TryParse(GetSafeValue(loan, "start_date"), out DateTime startDate))
                    return;

                // Get new end date from textbox with null check
                if (textBoxEndDateInput?.Text == null || !DateTime.TryParse(textBoxEndDateInput.Text, out DateTime newEndDate))
                    return;

                // Validate that new end date is after start date
                if (newEndDate <= startDate)
                {
                    MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Calculate number of months between start and new end date (matching AddLoan logic)
                int totalMonths = ((newEndDate.Year - startDate.Year) * 12) + (newEndDate.Month - startDate.Month);

                if (totalMonths <= 0)
                {
                    MessageBox.Show("Invalid date range. End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Calculate total payment terms (2 payments per month = bi-monthly) - matching AddLoan logic
                int totalPaymentTerms = totalMonths * 2;

                // Calculate monthly amortization (following AddLoan computation)
                decimal monthlyAmortization = loanAmount / totalMonths;

                // Calculate bi-monthly amortization (50% of monthly)
                decimal biMonthlyAmortization = monthlyAmortization / 2;

                // Update UI with new calculations
                SafeSetLabelText(labelTotalPaymentInput, $"0/{totalPaymentTerms}");
                SafeSetLabelText(labelMonthlyAmortizationInput, $"₱ {monthlyAmortization:N2}");
                SafeSetLabelText(labelBimonthlyAmortizationInput, $"₱ {biMonthlyAmortization:N2}");

                System.Diagnostics.Debug.WriteLine($"Recalculated: Months={totalMonths}, Terms={totalPaymentTerms}, Monthly=₱{monthlyAmortization:N2}, BiMonthly=₱{biMonthlyAmortization:N2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in recalculation: {ex.Message}");
            }
        }

        private string GetSafeValue(Dictionary<string, object> dict, string key)
        {
            try
            {
                if (dict != null && dict.ContainsKey(key) && dict[key] != null)
                {
                    string value = dict[key].ToString();
                    return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting value for key '{key}': {ex.Message}");
            }
            return "";
        }

        private void SafeSetLabelText(Label label, string text)
        {
            try
            {
                if (label == null) return;

                if (label.InvokeRequired)
                {
                    label.Invoke(new Action(() =>
                    {
                        if (label != null && !label.IsDisposed)
                            label.Text = text ?? "N/A";
                    }));
                }
                else
                {
                    if (!label.IsDisposed)
                        label.Text = text ?? "N/A";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting label text: {ex.Message}");
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender == null) return;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Close button error: {ex.Message}");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender == null) return;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cancel button error: {ex.Message}");
            }
        }

        private void setFont()
        {
            try
            {
                // Null check every control before setting font (same as your LoanDetails)
                if (comboBoxSelectedInput != null)
                    comboBoxSelectedInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelAmount != null)
                    labelAmount.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelAmountInput != null)
                    labelAmountInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelBalance != null)
                    labelBalance.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelBalanceInput != null)
                    labelBalanceInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelBimonthlyAmortization != null)
                    labelBimonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelBimonthlyAmortizationInput != null)
                    labelBimonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelEndDate != null)
                    labelEndDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (textBoxEndDateInput != null)
                    textBoxEndDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelID != null)
                    labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelIDInput != null)
                    labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelLoanDetails != null)
                    labelLoanDetails.Font = AttributesClass.GetFont("Roboto-Regular", 20f);

                if (labelMonthlyAmortization != null)
                    labelMonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelMonthlyAmortizationInput != null)
                    labelMonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelName != null)
                    labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelNameInput != null)
                    labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelSelected != null)
                    labelSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelStartDate != null)
                    labelStartDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelStartDateInput != null)
                    labelStartDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelTotalPayment != null)
                    labelTotalPayment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelTotalPaymentInput != null)
                    labelTotalPaymentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (labelType != null)
                    labelType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

                if (labelTypeInput != null)
                    labelTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (buttonCancel != null)
                    buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                if (buttonUpdate != null)
                    buttonUpdate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font load failed: {ex.Message}");

                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    MessageBox.Show("Font load failed: " + ex.Message);
                }
            }
        }

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (!ValidateInputs())
                {
                    return;
                }

                // Get selected loan
                if (comboBoxSelectedInput?.SelectedItem == null)
                {
                    MessageBox.Show("Please select a loan to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int index = comboBoxSelectedInput.SelectedIndex;
                if (index < 0 || index >= allEmployeeLoans.Count)
                {
                    MessageBox.Show("Invalid loan selection.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedLoan = allEmployeeLoans[index];
                string loanType = GetSafeValue(selectedLoan, "loan_type");

                // Get updated values from the form
                string endDate = textBoxEndDateInput.Text;

                // Parse payment terms from the label
                string paymentText = labelTotalPaymentInput.Text;
                int totalPaymentTerms = 0;
                if (paymentText.Contains("/"))
                {
                    string[] parts = paymentText.Split('/');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int terms))
                    {
                        totalPaymentTerms = terms;
                    }
                }

                // Parse amortizations from labels
                decimal monthlyAmortization = 0;
                decimal biMonthlyAmortization = 0;

                string monthlyText = labelMonthlyAmortizationInput.Text.Replace("₱", "").Replace(",", "").Trim();
                if (decimal.TryParse(monthlyText, out monthlyAmortization))
                {
                    // Successfully parsed
                }

                string biMonthlyText = labelBimonthlyAmortizationInput.Text.Replace("₱", "").Replace(",", "").Trim();
                if (decimal.TryParse(biMonthlyText, out biMonthlyAmortization))
                {
                    // Successfully parsed
                }

                string employeeName = labelNameInput.Text;
                string employeeId = labelIDInput.Text;

                // Show confirmation dialog with full details
                using (ConfirmLoanUpdate confirmUpdate = new ConfirmLoanUpdate())
                {
                    confirmUpdate.StartPosition = FormStartPosition.CenterParent;
                    var result = confirmUpdate.ShowDialog(this);

                    // If user confirms, THEN update cloud
                    if (result == DialogResult.OK)
                    {
                        // Update cloud here, not in ConfirmLoanUpdate
                        await UpdateLoanInFirebaseAsync(employeeId, employeeName, loanType, endDate,
                            totalPaymentTerms, monthlyAmortization, biMonthlyAmortization);

                        // Show success message and close
                        MessageBox.Show("Loan successfully updated in cloud database!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in loan update process: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Validate end date
            if (string.IsNullOrWhiteSpace(textBoxEndDateInput.Text) || !DateTime.TryParse(textBoxEndDateInput.Text, out DateTime endDate))
            {
                MessageBox.Show("Please enter a valid end date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxEndDateInput.Focus();
                return false;
            }

            // Validate that a loan is selected
            if (comboBoxSelectedInput?.SelectedItem == null)
            {
                MessageBox.Show("Please select a loan to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int index = comboBoxSelectedInput.SelectedIndex;
            if (index < 0 || index >= allEmployeeLoans.Count)
            {
                MessageBox.Show("Invalid loan selection.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate that end date is after start date
            var selectedLoan = allEmployeeLoans[index];
            if (DateTime.TryParse(GetSafeValue(selectedLoan, "start_date"), out DateTime startDate))
            {
                if (endDate <= startDate)
                {
                    MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxEndDateInput.Focus();
                    return false;
                }
            }

            return true;
        }

        private async Task UpdateLoanInFirebaseAsync(string employeeId, string employeeName, string loanType,
            string endDate, int totalPaymentTerms, decimal monthlyAmortization, decimal biMonthlyAmortization)
        {
            try
            {
                // Find the existing loan to update
                var existingLoan = await FindExistingLoan(employeeId, loanType);

                if (existingLoan == null)
                {
                    MessageBox.Show("Could not find existing loan to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get the current loan data to preserve important fields
                int index = comboBoxSelectedInput.SelectedIndex;
                var currentLoanData = allEmployeeLoans[index];

                // Create updated loan object for cloud storage (PRESERVE balance and payment progress)
                var updatedLoan = new
                {
                    employee_id = employeeId,
                    employee_name = employeeName,
                    loan_type = loanType,
                    loan_amount = decimal.Parse(GetSafeValue(currentLoanData, "loan_amount")),
                    balance = decimal.Parse(GetSafeValue(currentLoanData, "balance")), // PRESERVE existing balance
                    start_date = GetSafeValue(currentLoanData, "start_date"),
                    end_date = endDate,
                    total_payment_terms = totalPaymentTerms,
                    monthly_amortization = monthlyAmortization,
                    bi_monthly_amortization = biMonthlyAmortization,
                    total_payment_done = int.Parse(GetSafeValue(currentLoanData, "total_payment_done")), // PRESERVE existing payment progress
                    status = "Active",
                    created_at = GetSafeValue(currentLoanData, "created_at") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Keep original creation date
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // OVERWRITE the existing loan in Firebase
                await firebase
                    .Child("EmployeeLoans")
                    .Child(existingLoan.LoanIndex.ToString())
                    .PutAsync(updatedLoan);

                // Add admin log to cloud
                await AddAdminLog("Loan Updated", employeeId, employeeName,
                    $"Updated {loanType} terms for {employeeName} (Balance: ₱{existingLoan.CurrentBalance:N2})");

                Console.WriteLine($"Loan successfully updated in cloud at index: {existingLoan.LoanIndex}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating loan in Firebase: {ex.Message}");
            }
        }

        private async Task<ExistingLoanInfo> FindExistingLoan(string employeeId, string loanType)
        {
            try
            {
                var loansData = await firebase.Child("EmployeeLoans").OnceSingleAsync<object>();
                if (loansData == null) return null;

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(loansData);

                if (json.Trim().StartsWith("["))
                {
                    var jArray = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(json);
                    if (jArray != null)
                    {
                        for (int i = 0; i < jArray.Count; i++)
                        {
                            var item = jArray[i];
                            if (item.Type == JTokenType.Null) continue;

                            var loanDict = new Dictionary<string, object>();
                            foreach (JProperty property in item.Children<JProperty>())
                                loanDict[property.Name] = property.Value?.ToString() ?? "";

                            string loanEmployeeId = loanDict.GetValueOrDefault("employee_id")?.ToString();
                            string currentLoanType = loanDict.GetValueOrDefault("loan_type")?.ToString();

                            if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                loanEmployeeId.Trim().Equals(employeeId.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                currentLoanType?.Equals(loanType, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                decimal currentBalance = 0;
                                int currentPaymentDone = 0;

                                decimal.TryParse(loanDict.GetValueOrDefault("balance")?.ToString(), out currentBalance);
                                int.TryParse(loanDict.GetValueOrDefault("total_payment_done")?.ToString(), out currentPaymentDone);

                                return new ExistingLoanInfo
                                {
                                    LoanIndex = i,
                                    CreatedAt = loanDict.GetValueOrDefault("created_at")?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    CurrentBalance = currentBalance,
                                    CurrentPaymentDone = currentPaymentDone
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding existing loan: {ex.Message}");
                return null;
            }
        }

        private async Task AddAdminLog(string actionType, string targetEmployeeId, string targetEmployeeName, string description)
        {
            try
            {
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
                System.Diagnostics.Debug.WriteLine($"Error adding admin log: {ex.Message}");
            }
        }

        // Add form closing and disposal safety
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Form closing error: {ex.Message}");
            }
        }
    }
}