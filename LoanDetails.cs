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
    //UPDATED
    // Static helper class for extension methods
    public static class DictionaryExtensions
    {
        public static object GetValueOrDefault(this Dictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        public static string GetValueOrDefault(this Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }

    public partial class LoanDetails : Form
    {
        private string currentEmployeeId;
        private FirebaseClient firebase;
        private List<Dictionary<string, object>> allEmployeeLoans;
        private string employeeId;
        private string loanType;

        public LoanDetails(string employeeId, string loanType)
    {
        InitializeComponent();
        this.employeeId = employeeId;
        this.loanType = loanType;
        setFont();

            currentEmployeeId = string.IsNullOrWhiteSpace(employeeId) ? "" : employeeId;
            Console.WriteLine($"LoanDetails initialized with Employee ID: '{currentEmployeeId}'");

            try
            {
                firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize Firebase: {ex.Message}");
                return;
            }

            comboBoxSelectedInput.SelectedIndexChanged += comboBoxSelectedInput_SelectedIndexChanged;
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                if (firebase == null)
                {
                    MessageBox.Show("Firebase client is not initialized.");
                    return;
                }

                SetLoadingState(true);

                // STEP 1: Calculate loan amortizations and update Firebase
                await CalculateAndUpdateLoanAmortizations();

                // STEP 2: Load data from Firebase (including updated calculations)
                await LoadAndDisplayEmployeeDetailsImproved();
                await LoadAndDisplayEmployeeLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        // STEP 1: CALCULATE IN C# AND UPDATE FIREBASE
        private async Task CalculateAndUpdateLoanAmortizations()
        {
            try
            {
                Console.WriteLine($"Calculating loan amortizations for employee: {currentEmployeeId}");

                var loansData = await firebase.Child("EmployeeLoans").OnceSingleAsync<object>();
                if (loansData == null) return;

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(loansData);
                Console.WriteLine($"Raw loans JSON structure: {json.Substring(0, Math.Min(200, json.Length))}...");

                // Handle different JSON structures
                if (json.Trim().StartsWith("["))
                {
                    // Array structure [null, {...}, {...}]
                    var jArray = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(json);
                    if (jArray != null)
                    {
                        await ProcessJArrayLoans(jArray);
                    }
                }
                else if (json.Trim().StartsWith("{"))
                {
                    // Object structure { "1": {...}, "2": {...} }
                    var jObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                    if (jObject != null)
                    {
                        await ProcessJObjectLoans(jObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating loan amortizations: {ex.Message}");
            }
        }

        private async Task ProcessJArrayLoans(JArray jArray)
        {
            Console.WriteLine($"Processing JArray with {jArray.Count} elements");

            for (int i = 0; i < jArray.Count; i++)
            {
                var item = jArray[i];
                if (item.Type == JTokenType.Null)
                {
                    Console.WriteLine($"Skipping null element at index {i}");
                    continue;
                }

                try
                {
                    var loanDict = new Dictionary<string, object>();
                    foreach (JProperty property in item.Children<JProperty>())
                        loanDict[property.Name] = property.Value?.ToString() ?? "";

                    // Check if this loan belongs to the current employee
                    if (loanDict.ContainsKey("employee_id"))
                    {
                        string loanEmployeeId = loanDict["employee_id"]?.ToString();
                        string loanType = loanDict.GetValueOrDefault("loan_type")?.ToString() ?? "Unknown";
                        string loanId = loanDict.GetValueOrDefault("loan_id")?.ToString() ?? "N/A";

                        Console.WriteLine($"Checking loan at index {i}: Type={loanType}, EmployeeID='{loanEmployeeId}', LoanID={loanId}");

                        if (!string.IsNullOrEmpty(loanEmployeeId) &&
                            loanEmployeeId.Trim().Equals(currentEmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"✅ Found matching loan: {loanType} for employee {currentEmployeeId}");

                            // PERFORM CALCULATIONS IN C#
                            var calculatedData = CalculateLoanAmortizations(loanDict);

                            // UPDATE FIREBASE WITH CALCULATED DATA
                            await UpdateLoanInFirebase(loanDict, calculatedData);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Loan at index {i} missing employee_id");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing loan at index {i}: {ex.Message}");
                }
            }
        }

        private async Task ProcessJObjectLoans(JObject jObject)
        {
            Console.WriteLine($"Processing JObject with {jObject.Count} properties");

            foreach (var property in jObject.Properties())
            {
                try
                {
                    if (property.Value.Type == JTokenType.Object)
                    {
                        var loanDict = new Dictionary<string, object>();
                        foreach (JProperty prop in property.Value.Children<JProperty>())
                            loanDict[prop.Name] = prop.Value?.ToString() ?? "";

                        // Check if this loan belongs to the current employee
                        if (loanDict.ContainsKey("employee_id"))
                        {
                            string loanEmployeeId = loanDict["employee_id"]?.ToString();
                            string loanType = loanDict.GetValueOrDefault("loan_type")?.ToString() ?? "Unknown";

                            if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                loanEmployeeId.Trim().Equals(currentEmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"✅ Found matching loan in JObject: {loanType}");

                                // PERFORM CALCULATIONS IN C#
                                var calculatedData = CalculateLoanAmortizations(loanDict);

                                // UPDATE FIREBASE WITH CALCULATED DATA
                                await UpdateLoanInFirebase(loanDict, calculatedData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing loan property {property.Name}: {ex.Message}");
                }
            }
        }

        // C# CALCULATION LOGIC
        private Dictionary<string, object> CalculateLoanAmortizations(Dictionary<string, object> loanData)
        {
            var calculatedData = new Dictionary<string, object>();

            try
            {
                // Extract loan data
                decimal loanAmount = 0;
                int totalPaymentTerms = 0;
                int paymentDone = 0;

                decimal.TryParse(loanData.GetValueOrDefault("loan_amount")?.ToString(), out loanAmount);
                int.TryParse(loanData.GetValueOrDefault("total_payment_terms")?.ToString(), out totalPaymentTerms);
                int.TryParse(loanData.GetValueOrDefault("total_payment_done")?.ToString(), out paymentDone);

                // Calculate remaining balance
                decimal balance = loanAmount;
                if (totalPaymentTerms > 0 && paymentDone > 0)
                {
                    decimal paymentPerTerm = loanAmount / totalPaymentTerms;
                    balance = loanAmount - (paymentPerTerm * paymentDone);
                    if (balance < 0) balance = 0;
                }

                // Calculate amortizations
                decimal monthlyAmortization = 0;
                decimal biMonthlyAmortization = 0;

                if (totalPaymentTerms > 0)
                {
                    monthlyAmortization = loanAmount / totalPaymentTerms;
                    biMonthlyAmortization = monthlyAmortization / 2;
                }

                // Store calculated values
                calculatedData["balance"] = Math.Round(balance, 2);
                calculatedData["monthly_amortization"] = Math.Round(monthlyAmortization, 2);
                calculatedData["bi_monthly_amortization"] = Math.Round(biMonthlyAmortization, 2);
                calculatedData["total_payment_done"] = paymentDone;
                calculatedData["total_payment_terms"] = totalPaymentTerms;

                Console.WriteLine($"Calculated for {loanData.GetValueOrDefault("loan_type")}: " +
                                $"Balance={balance}, Monthly={monthlyAmortization}, BiMonthly={biMonthlyAmortization}");

                return calculatedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in loan calculation: {ex.Message}");
                return calculatedData;
            }
        }

        // UPDATE FIREBASE WITH CALCULATED DATA
        private async Task UpdateLoanInFirebase(Dictionary<string, object> loanData, Dictionary<string, object> calculatedData)
        {
            try
            {
                string loanId = loanData.GetValueOrDefault("loan_id")?.ToString();
                if (string.IsNullOrEmpty(loanId))
                {
                    Console.WriteLine("Cannot update Firebase: Loan ID is null or empty");
                    return;
                }

                // Update the loan in Firebase with calculated values
                await firebase
                    .Child("EmployeeLoans")
                    .Child(loanId)
                    .PatchAsync(new
                    {
                        balance = calculatedData["balance"],
                        monthly_amortization = calculatedData["monthly_amortization"],
                        bi_monthly_amortization = calculatedData["bi_monthly_amortization"],
                        total_payment_done = calculatedData["total_payment_done"],
                        total_payment_terms = calculatedData["total_payment_terms"],
                        updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                Console.WriteLine($"✅ Updated Firebase for loan ID: {loanId}, Type: {loanData.GetValueOrDefault("loan_type")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Firebase: {ex.Message}");
            }
        }

        // STEP 2: LOAD FROM FIREBASE AND DISPLAY IN UI
        private async Task LoadAndDisplayEmployeeDetailsImproved()
        {
            try
            {
                if (string.IsNullOrEmpty(currentEmployeeId))
                {
                    SetDefaultEmployeeInfo();
                    return;
                }

                Console.WriteLine($"Loading employee details for: '{currentEmployeeId}'");

                try
                {
                    var employee = await firebase
                        .Child("EmployeeDetails")
                        .Child(currentEmployeeId)
                        .OnceSingleAsync<Dictionary<string, object>>();

                    if (employee != null && employee.Count > 0)
                    {
                        DisplayEmployeeDetailsImproved(employee);
                        return;
                    }
                }
                catch (Exception ex1)
                {
                    Console.WriteLine($"Direct access failed: {ex1.Message}");
                }

                try
                {
                    var allEmployees = await firebase
                        .Child("EmployeeDetails")
                        .OnceAsync<Dictionary<string, object>>();

                    if (allEmployees != null)
                    {
                        var employee = allEmployees
                            .Where(x => x.Key == currentEmployeeId)
                            .Select(x => x.Object)
                            .FirstOrDefault();

                        if (employee != null && employee.Count > 0)
                        {
                            DisplayEmployeeDetailsImproved(employee);
                            return;
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Filter method failed: {ex2.Message}");
                }

                SetDefaultEmployeeInfo();
                Console.WriteLine($"Employee {currentEmployeeId} not found in database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee details: {ex.Message}");
                SetDefaultEmployeeInfo();
            }
        }

        private void DisplayEmployeeDetailsImproved(Dictionary<string, object> employeeData)
        {
            try
            {
                string firstName = GetSafeString(employeeData, "first_name");
                string middleName = GetSafeString(employeeData, "middle_name");
                string lastName = GetSafeString(employeeData, "last_name");
                string department = GetSafeString(employeeData, "department");
                string position = GetSafeString(employeeData, "position");
                string fullName = BuildFullName(firstName, middleName, lastName);

                SafeSetLabelText(labelIDInput, currentEmployeeId);
                SafeSetLabelText(labelNameInput, fullName);

                if (Controls.Find("labelDepartmentInput", true).FirstOrDefault() is Label lblDept)
                    SafeSetLabelText(lblDept, department);

                if (Controls.Find("labelPositionInput", true).FirstOrDefault() is Label lblPos)
                    SafeSetLabelText(lblPos, position);
            }
            catch
            {
                SetDefaultEmployeeInfo();
            }
        }

        private string GetSafeString(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                string value = dict[key].ToString();
                return string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();
            }
            return "N/A";
        }

        private string BuildFullName(string firstName, string middleName, string lastName)
        {
            var nameParts = new List<string> { firstName, middleName, lastName }
                .Where(part => !string.IsNullOrWhiteSpace(part) && part != "N/A")
                .Select(part => part.Trim());

            string fullName = string.Join(" ", nameParts);
            return string.IsNullOrWhiteSpace(fullName) ? "Unknown Employee" : fullName;
        }

        private void SafeSetLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
                label.Invoke(new Action(() => label.Text = text ?? "N/A"));
            else
                label.Text = text ?? "N/A";
        }

        private void SetDefaultEmployeeInfo()
        {
            SafeSetLabelText(labelIDInput, currentEmployeeId ?? "Unknown");
            SafeSetLabelText(labelNameInput, "Employee Not Found");

            if (Controls.Find("labelDepartmentInput", true).FirstOrDefault() is Label lblDept)
                SafeSetLabelText(lblDept, "N/A");

            if (Controls.Find("labelPositionInput", true).FirstOrDefault() is Label lblPos)
                SafeSetLabelText(lblPos, "N/A");
        }

        private async Task LoadAndDisplayEmployeeLoans()
        {
            try
            {
                Console.WriteLine($"Loading updated loans from Firebase for employee: {currentEmployeeId}");

                var loansData = await firebase.Child("EmployeeLoans").OnceSingleAsync<object>();
                var employeeLoans = new List<Dictionary<string, object>>();

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
                            foreach (var item in jArray)
                            {
                                if (item.Type == JTokenType.Null) continue;

                                var loanDict = new Dictionary<string, object>();
                                foreach (JProperty property in item.Children<JProperty>())
                                    loanDict[property.Name] = property.Value?.ToString() ?? "";

                                if (loanDict.ContainsKey("employee_id"))
                                {
                                    string loanEmployeeId = loanDict["employee_id"]?.ToString();
                                    string loanType = loanDict.GetValueOrDefault("loan_type")?.ToString() ?? "Unknown";

                                    Console.WriteLine($"Checking loan for display: Type={loanType}, EmployeeID='{loanEmployeeId}'");

                                    if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                        loanEmployeeId.Trim().Equals(currentEmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        employeeLoans.Add(loanDict);
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
                                        if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                            loanEmployeeId.Trim().Equals(currentEmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                                        {
                                            employeeLoans.Add(loanDict);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Total loans found for display: {employeeLoans.Count}");
                foreach (var loan in employeeLoans)
                {
                    Console.WriteLine($"  - {loan.GetValueOrDefault("loan_type")} (ID: {loan.GetValueOrDefault("loan_id")})");
                }

                allEmployeeLoans = employeeLoans;
                UpdateLoansUI(employeeLoans);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee loans: {ex.Message}");
                UpdateLoansUI(new List<Dictionary<string, object>>());
            }
        }

        // STEP 3: DISPLAY IN UI (FROM UPDATED FIREBASE DATA)
        private void UpdateLoansUI(List<Dictionary<string, object>> employeeLoans)
        {
            try
            {
                comboBoxSelectedInput.Items.Clear();
                comboBoxSelectedInput.SelectedIndexChanged -= comboBoxSelectedInput_SelectedIndexChanged;

                if (employeeLoans.Count > 0)
                {
                    Console.WriteLine($"Populating combo box with {employeeLoans.Count} loans:");

                    foreach (var loan in employeeLoans)
                    {
                        string loanType = loan.GetValueOrDefault("loan_type")?.ToString() ?? "Unknown";
                        string loanId = loan.GetValueOrDefault("loan_id")?.ToString() ?? "N/A";
                        string displayText = $"{loanType} (ID: {loanId})";
                        comboBoxSelectedInput.Items.Add(displayText);
                        Console.WriteLine($"  - Added to combo box: {displayText}");
                    }

                    comboBoxSelectedInput.SelectedIndexChanged += comboBoxSelectedInput_SelectedIndexChanged;

                    if (comboBoxSelectedInput.Items.Count > 0)
                    {
                        int matchingIndex = employeeLoans.FindIndex(l =>
                            l.GetValueOrDefault("loan_type")?.ToString().Equals(loanType, StringComparison.OrdinalIgnoreCase) == true);

                        if (matchingIndex >= 0)
                        {
                            comboBoxSelectedInput.SelectedIndex = matchingIndex;
                            DisplayLoanDetails(employeeLoans[matchingIndex]);
                            Console.WriteLine($"✅ Initially displaying: {employeeLoans[matchingIndex].GetValueOrDefault("loan_type")}");
                        }
                        else
                        {
                            // fallback to first loan if no match found
                            comboBoxSelectedInput.SelectedIndex = 0;
                            DisplayLoanDetails(employeeLoans[0]);
                            Console.WriteLine($"⚠️ No matching loan type found, showing first loan instead.");
                        }
                    }

                }
                else
                {
                    DisplayNoLoans();
                    Console.WriteLine($"No loans found for employee {currentEmployeeId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating loans UI: {ex.Message}");
                DisplayNoLoans();
            }
        }

        private void DisplayLoanDetails(Dictionary<string, object> loanData)
        {
            try
            {
                // DISPLAY DATA THAT WAS CALCULATED IN C# AND STORED IN FIREBASE
                string loanType = loanData.GetValueOrDefault("loan_type")?.ToString() ?? "N/A";
                labelTypeInput.Text = loanType;

                Console.WriteLine($"🎯 Displaying loan details for: {loanType}");

                // These values now come from Firebase (which were calculated in C#)
                if (decimal.TryParse(loanData.GetValueOrDefault("loan_amount")?.ToString(), out decimal loanAmount))
                    labelAmountInput.Text = $"₱ {loanAmount:N2}";
                else labelAmountInput.Text = "₱ 0.00";

                if (decimal.TryParse(loanData.GetValueOrDefault("balance")?.ToString(), out decimal balance))
                    labelBalanceInput.Text = $"₱ {balance:N2}";
                else labelBalanceInput.Text = "₱ 0.00";

                labelStartDateInput.Text = FormatDate(loanData.GetValueOrDefault("start_date")?.ToString() ?? "N/A");
                labelEndDateInput.Text = FormatDate(loanData.GetValueOrDefault("end_date")?.ToString() ?? "N/A");

                int paymentDone = 0;
                int paymentTerms = 0;

                if (!int.TryParse(loanData.GetValueOrDefault("total_payment_done")?.ToString(), out paymentDone))
                    paymentDone = 0;

                if (!int.TryParse(loanData.GetValueOrDefault("total_payment_terms")?.ToString(), out paymentTerms))
                    paymentTerms = 0;

                labelTotalPaymentInput.Text = $"{paymentDone}/{paymentTerms}";

                // Display calculated amortizations from Firebase
                if (decimal.TryParse(loanData.GetValueOrDefault("monthly_amortization")?.ToString(), out decimal monthly))
                    labelMonthlyAmortizationInput.Text = $"₱ {monthly:N2}";
                else
                    labelMonthlyAmortizationInput.Text = "₱ 0.00";

                if (decimal.TryParse(loanData.GetValueOrDefault("bi_monthly_amortization")?.ToString(), out decimal biMonthly))
                    labelBimonthlyAmortizationInput.Text = $"₱ {biMonthly:N2}";
                else
                    labelBimonthlyAmortizationInput.Text = "₱ 0.00";

                Console.WriteLine($"✅ Successfully displayed UI for {loanType} loan");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying loan details: {ex.Message}");
                DisplayNoLoans();
            }
        }

        private void DisplayNoLoans()
        {
            labelTypeInput.Text = "N/A";
            labelAmountInput.Text = "₱ 0.00";
            labelBalanceInput.Text = "₱ 0.00";
            labelStartDateInput.Text = "N/A";
            labelEndDateInput.Text = "N/A";
            labelTotalPaymentInput.Text = "0/0";
            labelMonthlyAmortizationInput.Text = "₱ 0.00";
            labelBimonthlyAmortizationInput.Text = "₱ 0.00";
        }

        private string FormatDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
                return date.ToString("MMM dd, yyyy");
            return dateString;
        }

        private void SetLoadingState(bool isLoading)
        {
            comboBoxSelectedInput.Enabled = !isLoading;
            buttonEdit.Enabled = !isLoading;
            buttonCancel.Enabled = !isLoading;
        }

        private void comboBoxSelectedInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (allEmployeeLoans == null || allEmployeeLoans.Count == 0)
                {
                    Console.WriteLine("No loans available to display");
                    return;
                }

                int index = comboBoxSelectedInput.SelectedIndex;
                Console.WriteLine($"Combo box selection changed to index: {index}");

                if (index < 0 || index >= allEmployeeLoans.Count)
                {
                    Console.WriteLine($"Invalid index: {index}");
                    return;
                }

                var selectedLoan = allEmployeeLoans[index];
                string loanType = selectedLoan.GetValueOrDefault("loan_type")?.ToString() ?? "Unknown";
                Console.WriteLine($"🎯 User selected: {loanType} loan");

                DisplayLoanDetails(selectedLoan);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in combo box selection: {ex.Message}");
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LoanDetailsEdit loanDetailsEdit = new LoanDetailsEdit(currentEmployeeId);
            AttributesClass.ShowWithOverlay(parentForm, loanDetailsEdit);
        }

        private void setFont()
        {
            try
            {
                comboBoxSelectedInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAmount.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAmountInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelBalance.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBalanceInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelBimonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBimonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEndDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEndDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLoanDetails.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelMonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStartDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStartDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelTotalPayment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTotalPaymentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}