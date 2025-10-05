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

        public LoanDetails(string employeeId)
        {
            InitializeComponent();
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

            // ✅ Ensure combo box event is subscribed
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
                var loansData = await firebase.Child("EmployeeLoans").OnceSingleAsync<object>();
                var employeeLoans = new List<Dictionary<string, object>>();

                if (loansData != null)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(loansData);
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
                                if (!string.IsNullOrEmpty(loanEmployeeId) &&
                                    loanEmployeeId.Trim().Equals(currentEmployeeId.Trim(), StringComparison.OrdinalIgnoreCase))
                                {
                                    employeeLoans.Add(loanDict);
                                }
                            }
                        }
                    }
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

        private void UpdateLoansUI(List<Dictionary<string, object>> employeeLoans)
        {
            try
            {
                comboBoxSelectedInput.Items.Clear();

                // ✅ Temporarily detach event to avoid early firing
                comboBoxSelectedInput.SelectedIndexChanged -= comboBoxSelectedInput_SelectedIndexChanged;

                if (employeeLoans.Count > 0)
                {
                    foreach (var loan in employeeLoans)
                    {
                        string loanType = loan.GetValueOrDefault("loan_type")?.ToString() ?? "Unknown";
                        string loanId = loan.GetValueOrDefault("loan_id")?.ToString() ?? "N/A";
                        comboBoxSelectedInput.Items.Add($"{loanType} (ID: {loanId})");
                    }

                    // ✅ Reattach event after loading
                    comboBoxSelectedInput.SelectedIndexChanged += comboBoxSelectedInput_SelectedIndexChanged;
                    comboBoxSelectedInput.SelectedIndex = 0;

                    // ✅ Explicitly refresh UI for first loan
                    DisplayLoanDetails(employeeLoans[0]);
                }
                else
                {
                    DisplayNoLoans();
                    MessageBox.Show($"No loans found for employee {currentEmployeeId}.");
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
                string loanType = loanData.GetValueOrDefault("loan_type")?.ToString() ?? "N/A";
                labelTypeInput.Text = loanType;

                if (decimal.TryParse(loanData.GetValueOrDefault("loan_amount")?.ToString(), out decimal loanAmount))
                    labelAmountInput.Text = $"₱ {loanAmount:N2}";
                else labelAmountInput.Text = "₱ 0.00";

                if (decimal.TryParse(loanData.GetValueOrDefault("balance")?.ToString(), out decimal balance))
                    labelBalanceInput.Text = $"₱ {balance:N2}";
                else labelBalanceInput.Text = "₱ 0.00";

                labelStartDateInput.Text = FormatDate(loanData.GetValueOrDefault("start_date")?.ToString() ?? "N/A");
                labelEndDateInput.Text = FormatDate(loanData.GetValueOrDefault("end_date")?.ToString() ?? "N/A");

                // <-- UPDATED: show total payment in "done/terms" format (e.g., "0/48")
                int paymentDone = 0;
                int paymentTerms = 0;

                if (!int.TryParse(loanData.GetValueOrDefault("total_payment_done")?.ToString(), out paymentDone))
                    paymentDone = 0;

                if (!int.TryParse(loanData.GetValueOrDefault("total_payment_terms")?.ToString(), out paymentTerms))
                    paymentTerms = 0;

                labelTotalPaymentInput.Text = $"{paymentDone}/{paymentTerms}";
                Console.WriteLine($"Total Payment Done: {paymentDone}/{paymentTerms}");

                if (int.TryParse(loanData.GetValueOrDefault("total_payment_terms")?.ToString(), out int paymentTermsForCalc) &&
                    decimal.TryParse(loanData.GetValueOrDefault("loan_amount")?.ToString(), out decimal amount) &&
                    paymentTermsForCalc > 0)
                {
                    decimal monthly = amount / paymentTermsForCalc;
                    decimal biMonthly = monthly / 2;
                    labelMonthlyAmortizationInput.Text = $"₱ {monthly:N2}";
                    labelBimonthlyAmortizationInput.Text = $"₱ {biMonthly:N2}";
                }
                else
                {
                    labelMonthlyAmortizationInput.Text = "₱ 0.00";
                    labelBimonthlyAmortizationInput.Text = "₱ 0.00";
                }
            }
            catch
            {
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
            // <-- UPDATED default to match "done/terms" format
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

        // ✅ Final fixed combo box logic
        private void comboBoxSelectedInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (allEmployeeLoans == null || allEmployeeLoans.Count == 0) return;
            int index = comboBoxSelectedInput.SelectedIndex;
            if (index < 0 || index >= allEmployeeLoans.Count) return;

            var selectedLoan = allEmployeeLoans[index];
            DisplayLoanDetails(selectedLoan);
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
