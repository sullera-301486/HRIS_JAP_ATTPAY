using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class PayrollSummaryEdit : Form
    {
        private List<PayrollLoan> employeeLoans = new List<PayrollLoan>();

        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string currentEmployeeId;
        private string payrollPeriod;
        private string payrollId;

        // Data storage
        private Dictionary<string, Dictionary<string, string>> payrollData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollEarnings = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> govDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> loanDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollSummary = new Dictionary<string, Dictionary<string, string>>();

        // Track if we're currently recalculating to avoid infinite loops
        private bool isRecalculating = false;

        public PayrollSummaryEdit(string employeeId)
        {
            InitializeComponent();
            currentEmployeeId = employeeId;
            setFont();
            _ = LoadPayrollDataAsync();
        }

        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();
            try
            {
                string cleanedJson = rawJson.Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"").Replace("(", "[").Replace(")", "]")
                    .Replace("[null,", "[").Replace("], [", ",").Replace("}, {", "},{")
                    .Replace("},(", "},{").Replace("),{", "},{");

                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");

                foreach (Match match in matches)
                {
                    try
                    {
                        var record = new Dictionary<string, string>();
                        string objectStr = match.Value;
                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(""[^""]*""|\d+\.?\d*|true|false|null)");

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
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }
            return records;
        }

        private async Task LoadArrayBasedData(string childPath, Action<Dictionary<string, string>> processItem)
        {
            try
            {
                var jsonResponse = await firebase.Child(childPath).OnceAsJsonAsync();
                string rawJson = jsonResponse.ToString();
                var records = ParseMalformedJson(rawJson);

                foreach (var record in records)
                {
                    processItem(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {childPath}: " + ex.Message);
            }
        }

        private async Task LoadPayrollDataAsync()
        {
            try
            {
                // Clear previous data
                payrollData.Clear();
                payrollEarnings.Clear();
                govDeductions.Clear();
                loanDeductions.Clear();
                payrollSummary.Clear();
                employeeLoans.Clear(); // Clear loans

                // Load payroll data
                await LoadArrayBasedData("Payroll", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId) && employeeId == currentEmployeeId)
                    {
                        payrollData[employeeId] = item;
                        payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : "";
                        System.Diagnostics.Debug.WriteLine($"Found payroll record for {employeeId}, payroll_id: {payrollId}");
                    }
                });

                if (string.IsNullOrEmpty(payrollId))
                {
                    MessageBox.Show("No payroll data found for this employee.", "Information",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Load payroll earnings
                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    var pId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(pId) && pId == payrollId)
                    {
                        payrollEarnings[pId] = item;
                        System.Diagnostics.Debug.WriteLine($"Found earnings for payroll_id: {pId}");
                    }
                });

                // Load government deductions
                await LoadArrayBasedData("GovernmentDeductions", (item) =>
                {
                    var pId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(pId) && pId == payrollId)
                    {
                        govDeductions[pId] = item;
                        System.Diagnostics.Debug.WriteLine($"Found gov deductions for payroll_id: {pId}");
                    }
                });

                // Load loan deductions
                await LoadArrayBasedData("LoansAndOtherDeductions", (item) =>
                {
                    var pId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(pId) && pId == payrollId)
                    {
                        loanDeductions[pId] = item;
                        System.Diagnostics.Debug.WriteLine($"Found loans/deductions for payroll_id: {pId}");
                    }
                });

                // Load payroll summary
                await LoadArrayBasedData("PayrollSummary", (item) =>
                {
                    var pId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(pId) && pId == payrollId)
                    {
                        payrollSummary[pId] = item;
                        System.Diagnostics.Debug.WriteLine($"Found summary for payroll_id: {pId}");
                    }
                });

                // Load employee loans from EmployeeLoans table
                await LoadEmployeeLoans();

                // Now populate the UI with loaded data
                if (payrollData.ContainsKey(currentEmployeeId))
                {
                    var payroll = payrollData[currentEmployeeId];
                    var earnings = payrollEarnings.ContainsKey(payrollId) ? payrollEarnings[payrollId] : null;
                    var govDeduction = govDeductions.ContainsKey(payrollId) ? govDeductions[payrollId] : null;
                    var loanDeduction = loanDeductions.ContainsKey(payrollId) ? loanDeductions[payrollId] : null;
                    var summary = payrollSummary.ContainsKey(payrollId) ? payrollSummary[payrollId] : null;

                    // Load Employee Basic Info
                    await LoadEmployeeBasicInfo();

                    // Basic Information
                    if (earnings != null)
                    {
                        textBoxDailyRateInput.Text = earnings.ContainsKey("daily_rate") ? earnings["daily_rate"] : "0.00";
                        labelDaysPresentInput.Text = earnings.ContainsKey("days_present") ? earnings["days_present"] : "0";
                    }
                    if (payrollData.ContainsKey(currentEmployeeId))
                    {
                        payroll = payrollData[currentEmployeeId];
                        if (payroll.ContainsKey("cutoff_start") && payroll.ContainsKey("cutoff_end"))
                        {
                            DateTime startDate = DateTime.Parse(payroll["cutoff_start"]);
                            DateTime endDate = DateTime.Parse(payroll["cutoff_end"]);

                            // Calculate work days (excluding Sundays)
                            int totalWorkDays = 0;
                            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                            {
                                if (date.DayOfWeek != DayOfWeek.Sunday)
                                    totalWorkDays++;
                            }

                            labelDaysInput.Text = totalWorkDays.ToString();

                            // Calculate salary
                            decimal dailyRate = SafeParseDecimal(textBoxDailyRateInput.Text);
                            decimal salary = dailyRate * totalWorkDays;
                            labelSalaryInput.Text = salary.ToString("0.00");
                        }
                    }

                    // Allowances (Base Amounts)
                    if (earnings != null)
                    {
                        textBoxIncentivesAmountBaseInput.Text = earnings.ContainsKey("incentives") ? earnings["incentives"] : "0.00";
                        textBoxCommissionAmountBaseInput.Text = earnings.ContainsKey("commission") ? earnings["commission"] : "0.00";
                        textBoxGasAllowanceAmountBaseInput.Text = earnings.ContainsKey("gas_allowance") ? earnings["gas_allowance"] : "0.00";
                        textBoxFoodAllowanceAmountBaseInput.Text = earnings.ContainsKey("food_allowance") ? earnings["food_allowance"] : "0.00";
                        textBoxCommunicationAmountBaseInput.Text = earnings.ContainsKey("communication") ? earnings["communication"] : "0.00";
                        textBoxGondolaAmountBaseInput.Text = earnings.ContainsKey("gondola") ? earnings["gondola"] : "0.00";
                    }

                    // Overtime - Calculate from attendance
                    decimal overtimeHours = await CalculateOvertimeForEmployee(currentEmployeeId,
                                            payroll["cutoff_start"], payroll["cutoff_end"]);
                    labelOvertimeInput.Text = overtimeHours.ToString("0.00");

                    // Government Deductions
                    if (govDeduction != null)
                    {
                        labelWithTaxAmountDebitInput.Text = govDeduction.ContainsKey("withholding_tax") ? govDeduction["withholding_tax"] : "0.00";
                        labelSSSAmountDebitInput.Text = govDeduction.ContainsKey("sss") ? govDeduction["sss"] : "0.00";
                        labelPagIbigAmountDebitInput.Text = govDeduction.ContainsKey("pagibig") ? govDeduction["pagibig"] : "0.00";
                        labelPhilhealthAmountDebitInput.Text = govDeduction.ContainsKey("philhealth") ? govDeduction["philhealth"] : "0.00";
                    }

                    // Other Deductions (non-loan items) - Load from EmployeeDeductions if available
                    await LoadEmployeeDeductions();

                    // Display loan details from EmployeeLoans table (this will populate loan amounts and details)
                    DisplayLoanDetails();

                    // Totals from PayrollSummary
                    if (summary != null)
                    {
                        labelGrossPayInput.Text = summary.ContainsKey("gross_pay") ? summary["gross_pay"] : "0.00";
                        labelDeductionsInput.Text = summary.ContainsKey("total_deductions") ? summary["total_deductions"] : "0.00";
                        labelOverallTotalInput.Text = summary.ContainsKey("net_pay") ? summary["net_pay"] : "0.00";
                    }

                    // Set date covered
                    labelDateCoveredInput.Text = $"{payroll["cutoff_start"]} to {payroll["cutoff_end"]}";

                    // Trigger initial calculation
                    RecalculateTotals();
                }
                else
                {
                    MessageBox.Show("No payroll data found for this employee.", "Information",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payroll data: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"LoadPayrollDataAsync Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private async Task LoadEmployeeDeductions()
        {
            try
            {
                await LoadArrayBasedData("EmployeeDeductions", (item) =>
                {
                    var empId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(empId) && empId == currentEmployeeId)
                    {
                        // Load Cash Advance
                        if (item.ContainsKey("cash_advance"))
                        {
                            textBoxCashAdvanceAmountDebitInput.Text = item["cash_advance"];
                        }

                        // Load Coop Contribution
                        if (item.ContainsKey("coop_contribution"))
                        {
                            textBoxCoopContriAmountDebitInput.Text = item["coop_contribution"];
                        }

                        // Load Other Deductions
                        if (item.ContainsKey("other_deductions"))
                        {
                            textBoxOthersAmountDebitInput.Text = item["other_deductions"];
                        }

                        System.Diagnostics.Debug.WriteLine($"Loaded employee deductions for {currentEmployeeId}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee deductions: {ex.Message}");
            }
        }

        private async Task LoadEmployeeBasicInfo()
        {
            try
            {
                var empData = await firebase
                    .Child("EmployeeDetails")
                    .Child(currentEmployeeId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (empData != null)
                {
                    string firstName = empData.ContainsKey("first_name") ? empData["first_name"]?.ToString() ?? "" : "";
                    string middleName = empData.ContainsKey("middle_name") ? empData["middle_name"]?.ToString() ?? "" : "";
                    string lastName = empData.ContainsKey("last_name") ? empData["last_name"]?.ToString() ?? "" : "";

                    labelIDInput.Text = empData.ContainsKey("employee_id") ? empData["employee_id"]?.ToString() ?? currentEmployeeId : currentEmployeeId;
                    labelNameInput.Text = $"{lastName}, {firstName} {middleName}".Trim().Replace("  ", " ");
                }

                // Load department and position from EmploymentInfo
                await LoadArrayBasedData("EmploymentInfo", (item) =>
                {
                    var empId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(empId) && empId == currentEmployeeId)
                    {
                        labelDepartmentInput.Text = item.ContainsKey("department") ? item["department"] : "N/A";
                        labelPositionInput.Text = item.ContainsKey("position") ? item["position"] : "N/A";
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee basic info: {ex.Message}");
            }
        }

        private async Task<decimal> CalculateOvertimeForEmployee(string employeeId, string cutoffStart, string cutoffEnd)
        {
            decimal totalOvertime = 0;
            try
            {
                var attendanceRecords = new List<Dictionary<string, string>>();
                await LoadArrayBasedData("Attendance", (item) =>
                {
                    var attEmployeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(attEmployeeId) && attEmployeeId == employeeId)
                    {
                        attendanceRecords.Add(item);
                    }
                });

                foreach (var attendance in attendanceRecords)
                {
                    if (attendance.ContainsKey("attendance_date") && IsDateInRange(attendance["attendance_date"], cutoffStart, cutoffEnd))
                    {
                        if (attendance.ContainsKey("overtime_hours") && decimal.TryParse(attendance["overtime_hours"], out decimal overtime))
                        {
                            totalOvertime += overtime;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating overtime: " + ex.Message);
            }
            return totalOvertime;
        }

        private bool IsDateInRange(string date, string startDate, string endDate)
        {
            try
            {
                DateTime checkDate = DateTime.Parse(date);
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);
                return checkDate >= start && checkDate <= end;
            }
            catch
            {
                return false;
            }
        }
        private async Task LoadEmployeeLoans()
        {
            try
            {
                employeeLoans.Clear();

                await LoadArrayBasedData("EmployeeLoans", (item) =>
                {
                    var empId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    var status = item.ContainsKey("status") ? item["status"] : null;

                    if (!string.IsNullOrEmpty(empId) && empId == currentEmployeeId &&
                        status != null && status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        var loan = new PayrollLoan
                        {
                            LoanType = item.ContainsKey("loan_type") ? item["loan_type"] : "",
                            Status = status
                        };

                        if (decimal.TryParse(item.ContainsKey("loan_amount") ? item["loan_amount"] : "0", out decimal amount))
                            loan.LoanAmount = amount;

                        if (decimal.TryParse(item.ContainsKey("balance") ? item["balance"] : "0", out decimal balance))
                            loan.Balance = balance;

                        if (int.TryParse(item.ContainsKey("total_payment_terms") ? item["total_payment_terms"] : "0", out int totalTerms))
                            loan.TotalPaymentTerms = totalTerms;

                        if (int.TryParse(item.ContainsKey("total_payment_done") ? item["total_payment_done"] : "0", out int paymentDone))
                            loan.TotalPaymentDone = paymentDone;

                        loan.StartDate = item.ContainsKey("start_date") ? item["start_date"] : "";
                        loan.EndDate = item.ContainsKey("end_date") ? item["end_date"] : "";

                        // Calculate bi-monthly amortization
                        loan.BiMonthlyAmortization = CalculateLoanAmortization(loan);

                        if (decimal.TryParse(item.ContainsKey("monthly_amortization") ? item["monthly_amortization"] : "0", out decimal monthlyAmort))
                            loan.MonthlyAmortization = monthlyAmort;

                        employeeLoans.Add(loan);

                        System.Diagnostics.Debug.WriteLine($"Loaded loan for {currentEmployeeId}: {loan.LoanType}, " +
                            $"TotalTerms: {loan.TotalPaymentTerms}, Paid: {loan.TotalPaymentDone}, " +
                            $"BiMonthly: {loan.BiMonthlyAmortization}");
                    }
                });

                System.Diagnostics.Debug.WriteLine($"Total loans loaded for {currentEmployeeId}: {employeeLoans.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading loans: {ex.Message}");
            }
        }


        private decimal CalculateLoanAmortization(PayrollLoan loan)
        {
            try
            {
                if (loan.TotalPaymentDone >= loan.TotalPaymentTerms || loan.Balance <= 0)
                    return 0;

                if (loan.MonthlyAmortization > 0)
                {
                    return loan.MonthlyAmortization / 2;
                }

                int remainingTerms = loan.TotalPaymentTerms - loan.TotalPaymentDone;
                if (remainingTerms > 0 && loan.Balance > 0)
                {
                    decimal monthlyPayment = loan.Balance / remainingTerms;
                    return monthlyPayment / 2;
                }

                if (loan.TotalPaymentTerms > 0 && loan.LoanAmount > 0)
                {
                    decimal monthlyPayment = loan.LoanAmount / loan.TotalPaymentTerms;
                    return monthlyPayment / 2;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating loan amortization: {ex.Message}");
                return 0;
            }
        }

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            using (ConfirmPayrollUpdate confirmForm = new ConfirmPayrollUpdate())
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                confirmForm.ShowDialog(this);

                if (confirmForm.UserConfirmed)
                {
                    await ExecutePayrollUpdate();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Payroll update cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async Task ExecutePayrollUpdate()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (string.IsNullOrEmpty(payrollId))
                {
                    MessageBox.Show("No payroll ID found. Cannot update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Update PayrollEarnings
                if (payrollEarnings.ContainsKey(payrollId))
                {
                    var earningsUpdate = payrollEarnings[payrollId];
                    earningsUpdate["daily_rate"] = textBoxDailyRateInput.Text;
                    earningsUpdate["days_present"] = labelDaysPresentInput.Text;

                    // Allowances
                    earningsUpdate["incentives"] = textBoxIncentivesAmountBaseInput.Text;
                    earningsUpdate["commission"] = textBoxCommissionAmountBaseInput.Text;
                    earningsUpdate["gas_allowance"] = textBoxGasAllowanceAmountBaseInput.Text;
                    earningsUpdate["food_allowance"] = textBoxFoodAllowanceAmountBaseInput.Text;
                    earningsUpdate["communication"] = textBoxCommunicationAmountBaseInput.Text;
                    earningsUpdate["gondola"] = textBoxGondolaAmountBaseInput.Text;


                    await UpdateFirebaseRecord("PayrollEarnings", "payroll_id", payrollId, earningsUpdate);
                }

                // Update GovernmentDeductions
                if (govDeductions.ContainsKey(payrollId))
                {
                    var govUpdate = govDeductions[payrollId];
                    govUpdate["withholding_tax"] = labelWithTaxAmountDebitInput.Text;
                    govUpdate["sss"] = labelSSSAmountDebitInput.Text;
                    govUpdate["pagibig"] = labelPagIbigAmountDebitInput.Text;
                    govUpdate["philhealth"] = labelPhilhealthAmountDebitInput.Text;

                    await UpdateFirebaseRecord("GovernmentDeductions", "payroll_id", payrollId, govUpdate);
                }

                // Update LoansAndOtherDeductions
                if (loanDeductions.ContainsKey(payrollId))
                {
                    var loansUpdate = loanDeductions[payrollId];
                    loansUpdate["sss_loan"] = labelSSSLoanAmountDebitInput.Text;
                    loansUpdate["pagibig_loan"] = labelPagIbigLoanAmountDebitInput.Text;
                    loansUpdate["car_loan"] = labelCarLoanAmountDebitInput.Text;
                    loansUpdate["housing_loan"] = labelHousingLoanAmountDebitInput.Text;
                    loansUpdate["cash_advance"] = textBoxCashAdvanceAmountDebitInput.Text;
                    loansUpdate["coop_loan"] = labelCoopLoanAmountDebitInput.Text;
                    loansUpdate["coop_contribution"] = textBoxCoopContriAmountDebitInput.Text;
                    loansUpdate["other_deduction"] = textBoxOthersAmountDebitInput.Text;

                   
                    await UpdateFirebaseRecord("LoansAndOtherDeductions", "payroll_id", payrollId, loansUpdate);
                }

                // Update PayrollSummary
                if (payrollSummary.ContainsKey(payrollId))
                {
                    var summaryUpdate = payrollSummary[payrollId];
                    summaryUpdate["gross_pay"] = labelGrossPayInput.Text;
                    summaryUpdate["total_deductions"] = labelDeductionsInput.Text;
                    summaryUpdate["net_pay"] = labelOverallTotalInput.Text;

                    await UpdateFirebaseRecord("PayrollSummary", "payroll_id", payrollId, summaryUpdate);
                }

                // Force a final recalculation to ensure all displays are updated
                RecalculateTotals();

                MessageBox.Show("Payroll updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Set dialog result to OK so the parent form knows to refresh
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating payroll: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void DisplayLoanDetails()
        {
            // Reset all loan displays first
            labelSSSLoanAmountDebitInput.Text = "0.00";
            labelPagIbigLoanAmountDebitInput.Text = "0.00";
            labelCarLoanAmountDebitInput.Text = "0.00";
            labelHousingLoanAmountDebitInput.Text = "0.00";
            labelCoopLoanAmountDebitInput.Text = "0.00";

            // Clear details labels
            if (this.Controls.Find("textBoxSSSLoanDetails", true).Length > 0)
                ((TextBox)this.Controls.Find("textBoxSSSLoanDetails", true)[0]).Text = "";
            if (this.Controls.Find("textBoxPagIbigLoanDetails", true).Length > 0)
                ((TextBox)this.Controls.Find("textBoxPagIbigLoanDetails", true)[0]).Text = "";
            if (this.Controls.Find("textBoxCarLoanDetails", true).Length > 0)
                ((TextBox)this.Controls.Find("textBoxCarLoanDetails", true)[0]).Text = "";
            if (this.Controls.Find("textBoxHousingLoanDetails", true).Length > 0)
                ((TextBox)this.Controls.Find("textBoxHousingLoanDetails", true)[0]).Text = "";
            if (this.Controls.Find("textBoxCoopLoanDetails", true).Length > 0)
                ((TextBox)this.Controls.Find("textBoxCoopLoanDetails", true)[0]).Text = "";

            System.Diagnostics.Debug.WriteLine($"Displaying loans for {currentEmployeeId}, count: {employeeLoans.Count}");

            foreach (var loan in employeeLoans)
            {
                if (loan.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                    loan.TotalPaymentDone < loan.TotalPaymentTerms)
                {
                    string progressText = $"{loan.TotalPaymentDone}/{loan.TotalPaymentTerms} (₱{loan.BiMonthlyAmortization:0.00}/cutoff)";

                    switch (loan.LoanType)
                    {
                        case "SSS Loan":
                            labelSSSLoanAmountDebitInput.Text = loan.BiMonthlyAmortization.ToString("0.00");
                            if (this.Controls.Find("textBoxSSSLoanDetails", true).Length > 0)
                                ((TextBox)this.Controls.Find("textBoxSSSLoanDetails", true)[0]).Text = progressText;
                            break;
                        case "Pag-IBIG Loan":
                            labelPagIbigLoanAmountDebitInput.Text = loan.BiMonthlyAmortization.ToString("0.00");
                            if (this.Controls.Find("textBoxPagIbigLoanDetails", true).Length > 0)
                                ((TextBox)this.Controls.Find("textBoxPagIbigLoanDetails", true)[0]).Text = progressText;
                            break;
                        case "Car Loan":
                            labelCarLoanAmountDebitInput.Text = loan.BiMonthlyAmortization.ToString("0.00");
                            if (this.Controls.Find("textBoxCarLoanDetails", true).Length > 0)
                                ((TextBox)this.Controls.Find("textBoxCarLoanDetails", true)[0]).Text = progressText;
                            break;
                        case "Housing Loan":
                            labelHousingLoanAmountDebitInput.Text = loan.BiMonthlyAmortization.ToString("0.00");
                            if (this.Controls.Find("textBoxHousingLoanDetails", true).Length > 0)
                                ((TextBox)this.Controls.Find("textBoxHousingLoanDetails", true)[0]).Text = progressText;
                            break;
                        case "Coop Loan":
                            labelCoopLoanAmountDebitInput.Text = loan.BiMonthlyAmortization.ToString("0.00");
                            if (this.Controls.Find("textBoxCoopLoanDetails", true).Length > 0)
                                ((TextBox)this.Controls.Find("textBoxCoopLoanDetails", true)[0]).Text = progressText;
                            break;
                    }

                    System.Diagnostics.Debug.WriteLine($"Loan displayed - Type: {loan.LoanType}, Amount: {loan.BiMonthlyAmortization}");
                }
            }
        }
        private async Task UpdateFirebaseRecord(string childPath, string idField, string idValue, Dictionary<string, string> updatedData)
        {
            try
            {
                // Get all records from the specified path
                var jsonResponse = await firebase.Child(childPath).OnceAsJsonAsync();
                string rawJson = jsonResponse.ToString();
                var records = ParseMalformedJson(rawJson);

                // Find the record with matching ID
                for (int i = 0; i < records.Count; i++)
                {
                    if (records[i].ContainsKey(idField) && records[i][idField] == idValue)
                    {
                        // Update the record
                        records[i] = updatedData;

                        // Rebuild the array with null at index 0 (to match Firebase structure)
                        var updatedArray = new List<Dictionary<string, string>> { null };
                        updatedArray.AddRange(records);

                        // Update the entire node
                        await firebase.Child(childPath).PutAsync(updatedArray);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating {childPath}: {ex.Message}");
            }
        }

        // REAL-TIME CALCULATION METHODS
        private void RecalculateTotals()
        {
            if (isRecalculating) return;

            isRecalculating = true;
            try
            {
                // Parse all values safely
                decimal dailyRate = SafeParseDecimal(textBoxDailyRateInput.Text);
                decimal daysPresent = SafeParseDecimal(labelDaysPresentInput.Text);
                decimal overtimeHours = SafeParseDecimal(labelOvertimeInput.Text);

                // Update salary display
                int totalWorkDays = 0;
                if (int.TryParse(labelDaysInput.Text, out totalWorkDays))
                {
                    decimal salary = dailyRate * totalWorkDays;
                    labelSalaryInput.Text = salary.ToString("0.00");
                }

                // Calculate basic pay
                decimal basicPay = dailyRate * daysPresent;
                labelBasicPayAmountBaseInput.Text = basicPay.ToString("0.00");
                labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");

                // Calculate overtime pay
                decimal overtimeRatePerHour = (dailyRate / 8m) * 1.5m;
                decimal overtimePay = overtimeHours * overtimeRatePerHour;
                labelOvertimePerHourAmountBaseInput.Text = overtimeRatePerHour.ToString("0.00");
                labelOvertimePerHourAmountCreditInput.Text = overtimePay.ToString("0.00");

                // Sum allowances
                decimal incentives = SafeParseDecimal(textBoxIncentivesAmountBaseInput.Text);
                decimal commission = SafeParseDecimal(textBoxCommissionAmountBaseInput.Text);
                decimal gasAllowance = SafeParseDecimal(textBoxGasAllowanceAmountBaseInput.Text);
                decimal foodAllowance = SafeParseDecimal(textBoxFoodAllowanceAmountBaseInput.Text);
                decimal communication = SafeParseDecimal(textBoxCommunicationAmountBaseInput.Text);
                decimal gondola = SafeParseDecimal(textBoxGondolaAmountBaseInput.Text);

                decimal totalAllowances = incentives + commission + gasAllowance + foodAllowance + communication + gondola;

                // Calculate gross pay
                decimal grossPay = basicPay + overtimePay + totalAllowances;
                labelGrossPayInput.Text = grossPay.ToString("0.00");

                // Sum deductions
                decimal withholdingTax = SafeParseDecimal(labelWithTaxAmountDebitInput.Text);
                decimal sss = SafeParseDecimal(labelSSSAmountDebitInput.Text);
                decimal pagibig = SafeParseDecimal(labelPagIbigAmountDebitInput.Text);
                decimal philhealth = SafeParseDecimal(labelPhilhealthAmountDebitInput.Text);
                decimal sssLoan = SafeParseDecimal(labelSSSLoanAmountDebitInput.Text);
                decimal pagibigLoan = SafeParseDecimal(labelPagIbigLoanAmountDebitInput.Text);
                decimal carLoan = SafeParseDecimal(labelCarLoanAmountDebitInput.Text);
                decimal housingLoan = SafeParseDecimal(labelHousingLoanAmountDebitInput.Text);
                decimal cashAdvance = SafeParseDecimal(textBoxCashAdvanceAmountDebitInput.Text);
                decimal coopLoan = SafeParseDecimal(labelCoopLoanAmountDebitInput.Text);
                decimal coopContribution = SafeParseDecimal(textBoxCoopContriAmountDebitInput.Text);
                decimal otherDeductions = SafeParseDecimal(textBoxOthersAmountDebitInput.Text);

                decimal totalDeductions = withholdingTax + sss + pagibig + philhealth +
                                         sssLoan + pagibigLoan + carLoan + housingLoan +
                                         cashAdvance + coopLoan + coopContribution + otherDeductions;

                labelDeductionsInput.Text = totalDeductions.ToString("0.00");

                // Calculate net pay
                decimal netPay = grossPay - totalDeductions;
                labelOverallTotalInput.Text = netPay > 0 ? netPay.ToString("0.00") : "0.00";
            }
            catch (Exception ex)
            {
                // Silent fail for calculation errors during typing
                System.Diagnostics.Debug.WriteLine($"Calculation error: {ex.Message}");
            }
            finally
            {
                isRecalculating = false;
            }
        }

        private decimal SafeParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0m;
            if (decimal.TryParse(text, out decimal result)) return result;
            return 0m;
        }

        // REAL-TIME EVENT HANDLERS
        private void textBoxDailyRateInput_TextChanged(object sender, EventArgs e)
        {
            // Update salary in real-time based on daily rate
            UpdateSalaryDisplay();
            RecalculateTotals();
        }

        private void textBoxIncentivesAmountBaseInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxCommissionAmountBaseInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxGasAllowanceAmountBaseInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxFoodAllowanceAmountBaseInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxCommunicationAmountBaseInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxGondolaAmountBaseInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxCashAdvanceAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxCoopContriAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void textBoxOthersAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        // For government deductions that are labels but might need to be editable
        // You might want to change these to textboxes for full editing capability
        private void labelWithTaxAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelSSSAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelPagIbigAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelPhilhealthAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelSSSLoanAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelPagIbigLoanAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelCarLoanAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelHousingLoanAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void labelCoopLoanAmountDebitInput_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        // Empty event handlers for details fields (not used in calculations)
        private void textBoxWithTaxDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxSSSDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxPagIbigDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxPhilhealthDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxSSSLoanDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxPagIbigLoanDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxCarLoanDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxHousingLoanDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxCashAdvanceDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxCoopLoanDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxCoopContriDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxOthersDetails_TextChanged(object sender, EventArgs e) { }
        private void textBoxVacationLeaveCredit_TextChanged(object sender, EventArgs e) { }
        private void textBoxSickLeaveCredit_TextChanged(object sender, EventArgs e) { }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void UpdateSalaryDisplay()
        {
            try
            {
                decimal dailyRate = SafeParseDecimal(textBoxDailyRateInput.Text);
                int totalWorkDays = 0;

                // Calculate total work days from the payroll period
                if (payrollData.ContainsKey(currentEmployeeId))
                {
                    var payroll = payrollData[currentEmployeeId];
                    if (payroll.ContainsKey("cutoff_start") && payroll.ContainsKey("cutoff_end"))
                    {
                        DateTime startDate = DateTime.Parse(payroll["cutoff_start"]);
                        DateTime endDate = DateTime.Parse(payroll["cutoff_end"]);

                        // Calculate work days (excluding Sundays)
                        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            if (date.DayOfWeek != DayOfWeek.Sunday)
                                totalWorkDays++;
                        }
                    }
                }

                // Update the Days label
                labelDaysInput.Text = totalWorkDays.ToString();

                // Calculate and update salary
                decimal salary = dailyRate * totalWorkDays;
                labelSalaryInput.Text = salary.ToString("0.00");

                System.Diagnostics.Debug.WriteLine($"Updated Salary: Daily Rate={dailyRate}, Work Days={totalWorkDays}, Salary={salary}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating salary: {ex.Message}");
            }
        }

        private void setFont()
        {
            try
            {
                labelAmountBase.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelAmountCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelAmountDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelBasicPay.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelBasicPayAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelBasicPayAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelBasicPayCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCarLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCarLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCarLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCashAdvance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxCashAdvanceAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxCashAdvanceDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommission.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxCommissionAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommissionAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommunication.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxCommunicationAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommunicationAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopContri.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxCoopContriAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxCoopContriDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCoopLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDateCovered.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDateCoveredInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDays.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDaysPresent.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDaysPresentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDeductions.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDeductionsInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDetails.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelFoodAllowance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxFoodAllowanceAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelFoodAllowanceAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGasAllowance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxGasAllowanceAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGasAllowanceAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGondola.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxGondolaAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGondolaAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGrossPay.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGrossPayInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHousingLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelHousingLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelIncentives.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxIncentivesAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelIncentivesAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveBalance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOthers.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxOthersAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxOthersDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOverallTotalInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimePerHourCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerHour.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimePerHourAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerHourAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinute.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimePerMinuteCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinuteAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinuteAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbig.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPagIbigAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPagIbigLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPayAndAllowances.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPayrollSummary.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelPhilhealth.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPhilhealthAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPhilhealthDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSalary.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSickLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSS.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSSSAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSSSLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelVacationLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelWithTax.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelWithTaxAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelWithTaxDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                buttonUpdate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }

    public class PayrollDataModel
    {
        public string employee_id { get; set; }
        public string payroll_period { get; set; }
        public string date_covered { get; set; }
        public string daily_rate { get; set; }
        public string days_present { get; set; }
        public string basic_pay { get; set; }

        // Allowances
        public string food_allowance { get; set; }
        public string gas_allowance { get; set; }
        public string communication_allowance { get; set; }
        public string gondola_allowance { get; set; }
        public string incentives { get; set; }
        public string commission { get; set; }

        // Overtime
        public string overtime_hours { get; set; }
        public string overtime_per_hour { get; set; }
        public string overtime_per_minute { get; set; }

        // Leave Credits
        public string vacation_leave_credit { get; set; }
        public string sick_leave_credit { get; set; }

        // Statutory Deductions
        public string sss_deduction { get; set; }
        public string sss_details { get; set; }
        public string pagibig_deduction { get; set; }
        public string pagibig_details { get; set; }
        public string philhealth_deduction { get; set; }
        public string philhealth_details { get; set; }

        // Loans
        public string sss_loan { get; set; }
        public string sss_loan_details { get; set; }
        public string pagibig_loan { get; set; }
        public string pagibig_loan_details { get; set; }
        public string car_loan { get; set; }
        public string car_loan_details { get; set; }
        public string housing_loan { get; set; }
        public string housing_loan_details { get; set; }

        // Advances and Contributions
        public string cash_advance { get; set; }
        public string cash_advance_details { get; set; }
        public string coop_loan { get; set; }
        public string coop_loan_details { get; set; }
        public string coop_contribution { get; set; }
        public string coop_contribution_details { get; set; }

        // Others
        public string withholding_tax { get; set; }
        public string withholding_tax_details { get; set; }
        public string others_deduction { get; set; }
        public string others_details { get; set; }

        // Totals (editable directly)
        public string gross_pay { get; set; }
        public string total_deductions { get; set; }
        public string net_pay { get; set; }

        public string updated_at { get; set; }
    }
}