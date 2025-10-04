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
    public partial class LoanDetails : Form
    {
        private string currentEmployeeId;
        private FirebaseClient firebase;
        private List<Dictionary<string, string>> allEmployeeLoans;

        public LoanDetails(string employeeId)
        {
            InitializeComponent();
            setFont();
            currentEmployeeId = employeeId;

            // Initialize Firebase client
            try
            {
                firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize Firebase: {ex.Message}");
                return;
            }

            // Load loan data when form is created
            LoadLoanData();
        }

        // Helper method to replace GetValueOrDefault
        private string GetValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        private async void LoadLoanData()
        {
            try
            {
                if (firebase == null)
                {
                    MessageBox.Show("Firebase client is not initialized.");
                    return;
                }

                // Show loading state
                SetLoadingState(true);

                // Get employee details
                object employeeDetails = null;
                try
                {
                    employeeDetails = await firebase
                        .Child("EmployeeDetails")
                        .Child(currentEmployeeId)
                        .OnceSingleAsync<object>();
                    Console.WriteLine($"Loaded employee details for: {currentEmployeeId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not load employee details: {ex.Message}");
                    // Continue without employee details
                }

                // Get all employee loans using raw JSON approach
                allEmployeeLoans = new List<Dictionary<string, string>>();
                try
                {
                    // Get raw JSON response
                    var jsonResponse = await firebase
                        .Child("EmployeeLoans")
                        .OnceAsJsonAsync();

                    string rawJson = jsonResponse.ToString();
                    Console.WriteLine($"Raw JSON response length: {rawJson.Length}");

                    // Parse the malformed JSON
                    var loanRecords = ParseMalformedJson(rawJson);
                    Console.WriteLine($"Parsed {loanRecords.Count} total loan records");

                    // Debug: Print all employee IDs found in loans
                    Console.WriteLine("All employee IDs found in loans:");
                    foreach (var loan in loanRecords)
                    {
                        if (loan.ContainsKey("employee_id"))
                        {
                            Console.WriteLine($"  - Employee ID: {loan["employee_id"]}, Loan Type: {GetValueOrDefault(loan, "loan_type", "Unknown")}");
                        }
                    }

                    // Filter loans for current employee
                    var employeeLoans = new List<Dictionary<string, string>>();
                    foreach (var loan in loanRecords)
                    {
                        if (loan.ContainsKey("employee_id") && loan["employee_id"] == currentEmployeeId)
                        {
                            employeeLoans.Add(loan);
                            Console.WriteLine($"Found loan for {currentEmployeeId}: {GetValueOrDefault(loan, "loan_type", "Unknown")}");
                        }
                    }

                    Console.WriteLine($"Found {employeeLoans.Count} loans for employee {currentEmployeeId}");

                    // Store all loans for combo box switching
                    allEmployeeLoans = employeeLoans;

                    // Populate combo box with loan types
                    comboBoxSelectedInput.Items.Clear();
                    foreach (var loan in employeeLoans)
                    {
                        string loanType = loan.ContainsKey("loan_type") ? loan["loan_type"] : "Unknown";
                        if (!string.IsNullOrEmpty(loanType) && !comboBoxSelectedInput.Items.Contains(loanType))
                        {
                            comboBoxSelectedInput.Items.Add(loanType);
                            Console.WriteLine($"Added loan type to combo box: {loanType}");
                        }
                    }

                    // Select first loan if available
                    if (comboBoxSelectedInput.Items.Count > 0)
                    {
                        comboBoxSelectedInput.SelectedIndex = 0;
                        DisplayLoanDetails(employeeLoans[0], employeeDetails);
                        Console.WriteLine($"Displaying first loan: {GetValueOrDefault(employeeLoans[0], "loan_type", "Unknown")}");
                    }
                    else
                    {
                        MessageBox.Show($"No loans found for employee {currentEmployeeId}. Please check if the employee ID is correct.");
                        Console.WriteLine($"No loans found for employee: {currentEmployeeId}");
                        this.Close();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading loans: {ex.Message}");
                    Console.WriteLine($"Loan loading error: {ex.Message}\n{ex.StackTrace}");
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loan data: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        // Handle malformed JSON from Firebase
        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();
            try
            {
                // Clean the JSON - fix common issues in Firebase response
                string cleanedJson = rawJson
                    .Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"")  // Replace single quotes with double quotes
                    .Replace("(", "[").Replace(")", "]")  // Fix parentheses
                    .Replace("[null,", "[")  // Remove null entries
                    .Replace("], [", ",").Replace("}, {", "},{")  // Fix array formatting
                    .Replace("},(", "},{").Replace("),{", "},{")  // More fixes
                    .Replace("[[", "[").Replace("]]", "]");  // Fix double brackets

                Console.WriteLine($"Cleaned JSON length: {cleanedJson.Length}");

                // Extract objects using regex
                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");
                Console.WriteLine($"Found {matches.Count} JSON objects using regex");

                foreach (Match match in matches)
                {
                    try
                    {
                        var record = new Dictionary<string, string>();
                        string objectStr = match.Value;

                        // Extract key-value pairs with improved regex
                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(""[^""]*""|[\d\.]+|true|false|null)");
                        foreach (Match kvpMatch in kvpMatches)
                        {
                            if (kvpMatch.Groups.Count >= 3)
                            {
                                string key = kvpMatch.Groups[1].Value;
                                string value = kvpMatch.Groups[2].Value.Trim('"');
                                record[key] = value;
                            }
                        }

                        if (record.Count > 0)
                        {
                            records.Add(record);
                            Console.WriteLine($"Parsed object with {record.Count} fields, EmployeeID: {GetValueOrDefault(record, "employee_id", "Not Found")}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing object: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON parsing error: " + ex.Message);
                Console.WriteLine($"JSON parsing error: {ex.Message}\n{ex.StackTrace}");
            }

            Console.WriteLine($"Total records parsed: {records.Count}");
            return records;
        }

        private void SetLoadingState(bool isLoading)
        {
            // You can add a loading indicator here if needed
            comboBoxSelectedInput.Enabled = !isLoading;
            buttonEdit.Enabled = !isLoading;
            buttonCancel.Enabled = !isLoading;
        }

        private void DisplayLoanDetails(Dictionary<string, string> loanData, object employeeDetails)
        {
            try
            {
                Console.WriteLine($"Displaying loan details for employee: {currentEmployeeId}");

                // Display employee information
                if (employeeDetails != null)
                {
                    var empData = ParseFirebaseObject(employeeDetails);
                    string firstName = empData.ContainsKey("first_name") ? empData["first_name"] : "";
                    string middleName = empData.ContainsKey("middle_name") ? empData["middle_name"] : "";
                    string lastName = empData.ContainsKey("last_name") ? empData["last_name"] : "";

                    labelIDInput.Text = currentEmployeeId;
                    labelNameInput.Text = $"{firstName} {middleName} {lastName}".Trim();
                    Console.WriteLine($"Employee: {labelNameInput.Text}");
                }
                else
                {
                    labelIDInput.Text = currentEmployeeId;
                    labelNameInput.Text = "Unknown";
                    Console.WriteLine("Employee details not found");
                }

                // Display loan details
                string loanType = loanData.ContainsKey("loan_type") ? loanData["loan_type"] : "Unknown";
                labelTypeInput.Text = loanType;
                Console.WriteLine($"Loan Type: {loanType}");

                // Loan amount
                if (loanData.ContainsKey("loan_amount") && decimal.TryParse(loanData["loan_amount"], out decimal loanAmount))
                {
                    labelAmountInput.Text = $"₱ {loanAmount:N2}";
                    Console.WriteLine($"Loan Amount: {loanAmount:N2}");
                }
                else
                {
                    labelAmountInput.Text = "₱ 0.00";
                    Console.WriteLine("Loan Amount: Not found or invalid");
                }

                // Balance
                if (loanData.ContainsKey("balance") && decimal.TryParse(loanData["balance"], out decimal balance))
                {
                    labelBalanceInput.Text = $"₱ {balance:N2}";
                    Console.WriteLine($"Balance: {balance:N2}");
                }
                else
                {
                    labelBalanceInput.Text = "₱ 0.00";
                    Console.WriteLine("Balance: Not found or invalid");
                }

                // Dates
                string startDate = loanData.ContainsKey("start_date") ? FormatDate(loanData["start_date"]) : "N/A";
                string endDate = loanData.ContainsKey("end_date") ? FormatDate(loanData["end_date"]) : "N/A";
                labelStartDateInput.Text = startDate;
                labelEndDateInput.Text = endDate;
                Console.WriteLine($"Dates: {startDate} to {endDate}");

                // Payment information
                int totalPaymentDone = 0;
                int totalPaymentTerms = 0;

                if (loanData.ContainsKey("total_payment_done") && int.TryParse(loanData["total_payment_done"], out totalPaymentDone))
                {
                    labelTotalPaymentInput.Text = $"{totalPaymentDone}";
                }
                else
                {
                    labelTotalPaymentInput.Text = "0";
                }

                if (loanData.ContainsKey("total_payment_terms") && int.TryParse(loanData["total_payment_terms"], out totalPaymentTerms))
                {
                    // Calculate amortizations
                    if (loanData.ContainsKey("loan_amount") && decimal.TryParse(loanData["loan_amount"], out decimal amt) && totalPaymentTerms > 0)
                    {
                        decimal monthlyAmortization = amt / totalPaymentTerms;
                        decimal bimonthlyAmortization = monthlyAmortization / 2;

                        labelMonthlyAmortizationInput.Text = $"₱ {monthlyAmortization:N2}";
                        labelBimonthlyAmortizationInput.Text = $"₱ {bimonthlyAmortization:N2}";
                        Console.WriteLine($"Amortization: Monthly {monthlyAmortization:N2}, Bi-monthly {bimonthlyAmortization:N2}");
                    }
                    else
                    {
                        labelMonthlyAmortizationInput.Text = "₱ 0.00";
                        labelBimonthlyAmortizationInput.Text = "₱ 0.00";
                    }
                }
                else
                {
                    labelMonthlyAmortizationInput.Text = "₱ 0.00";
                    labelBimonthlyAmortizationInput.Text = "₱ 0.00";
                }

                Console.WriteLine("Loan details displayed successfully");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying loan details: {ex.Message}");
                Console.WriteLine($"Display error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private string FormatDate(string dateString)
        {
            try
            {
                if (DateTime.TryParse(dateString, out DateTime date))
                {
                    return date.ToString("MMM dd, yyyy");
                }
                return dateString; // Return original if parsing fails
            }
            catch
            {
                return dateString;
            }
        }

        private Dictionary<string, string> ParseFirebaseObject(object firebaseObject)
        {
            var result = new Dictionary<string, string>();

            try
            {
                if (firebaseObject == null) return result;

                // Handle JObject directly
                if (firebaseObject is JObject jObject)
                {
                    foreach (var property in jObject.Properties())
                    {
                        result[property.Name] = property.Value?.ToString() ?? "";
                    }
                    return result;
                }

                // Handle dictionary types
                if (firebaseObject is IDictionary<string, object> dict)
                {
                    foreach (var kvp in dict)
                    {
                        result[kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                    return result;
                }

                // Fallback: serialize to JSON and parse
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(firebaseObject);
                var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (parsed != null)
                {
                    foreach (var kvp in parsed)
                    {
                        result[kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Firebase object: {ex.Message}");
            }

            return result;
        }

        private void comboBoxSelectedInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When user selects different loan type, reload that loan's details
            if (comboBoxSelectedInput.SelectedItem != null && allEmployeeLoans != null)
            {
                string selectedLoanType = comboBoxSelectedInput.SelectedItem.ToString();
                Console.WriteLine($"User selected loan type: {selectedLoanType}");

                // Find the loan with matching type
                var selectedLoan = allEmployeeLoans.FirstOrDefault(loan =>
                    loan.ContainsKey("loan_type") && loan["loan_type"] == selectedLoanType);

                if (selectedLoan != null)
                {
                    // Get employee details again
                    ReloadEmployeeDetailsAndDisplay(selectedLoan);
                }
            }
        }

        private async void ReloadEmployeeDetailsAndDisplay(Dictionary<string, string> loanData)
        {
            try
            {
                object employeeDetails = null;
                try
                {
                    employeeDetails = await firebase
                        .Child("EmployeeDetails")
                        .Child(currentEmployeeId)
                        .OnceSingleAsync<object>();
                }
                catch
                {
                    // Continue without employee details
                }

                DisplayLoanDetails(loanData, employeeDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reloading loan details: {ex.Message}");
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