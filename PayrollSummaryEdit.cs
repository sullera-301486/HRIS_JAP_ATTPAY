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

                // Load payroll data
                await LoadArrayBasedData("Payroll", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId) && employeeId == currentEmployeeId)
                    {
                        payrollData[employeeId] = item;
                        payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : "";
                    }
                });

                // Load payroll earnings
                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        payrollEarnings[payrollId] = item;
                });

                // Load government deductions
                await LoadArrayBasedData("GovernmentDeductions", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        govDeductions[payrollId] = item;
                });

                // Load loan deductions
                await LoadArrayBasedData("LoansAndOtherDeductions", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        loanDeductions[payrollId] = item;
                });

                // Load payroll summary
                await LoadArrayBasedData("PayrollSummary", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        payrollSummary[payrollId] = item;
                });

                if (!string.IsNullOrEmpty(payrollId) && payrollData.ContainsKey(currentEmployeeId))
                {
                    var payroll = payrollData[currentEmployeeId];
                    var earnings = payrollEarnings.ContainsKey(payrollId) ? payrollEarnings[payrollId] : null;
                    var govDeduction = govDeductions.ContainsKey(payrollId) ? govDeductions[payrollId] : null;
                    var loanDeduction = loanDeductions.ContainsKey(payrollId) ? loanDeductions[payrollId] : null;
                    var summary = payrollSummary.ContainsKey(payrollId) ? payrollSummary[payrollId] : null;

                    // Basic Information
                    if (earnings != null)
                    {
                        textBoxDailyRateInput.Text = earnings.ContainsKey("daily_rate") ? earnings["daily_rate"] : "0.00";
                        labelDaysPresentInput.Text = earnings.ContainsKey("days_present") ? earnings["days_present"] : "0";
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
                    decimal overtimeHours = await CalculateOvertimeForEmployee(currentEmployeeId, payroll["cutoff_start"], payroll["cutoff_end"]);
                    labelOvertimeInput.Text = overtimeHours.ToString("0.00");

                    // Government Deductions
                    if (govDeduction != null)
                    {
                        labelWithTaxAmountDebitInput.Text = govDeduction.ContainsKey("withholding_tax") ? govDeduction["withholding_tax"] : "0.00";
                        labelSSSAmountDebitInput.Text = govDeduction.ContainsKey("sss") ? govDeduction["sss"] : "0.00";
                        labelPagIbigAmountDebitInput.Text = govDeduction.ContainsKey("pagibig") ? govDeduction["pagibig"] : "0.00";
                        labelPhilhealthAmountDebitInput.Text = govDeduction.ContainsKey("philhealth") ? govDeduction["philhealth"] : "0.00";
                    }

                    // Loans and Other Deductions
                    if (loanDeduction != null)
                    {
                        textBoxSSSLoanAmountDebitInput.Text = loanDeduction.ContainsKey("sss_loan") ? loanDeduction["sss_loan"] : "0.00";
                        textBoxPagIbigLoanAmountDebitInput.Text = loanDeduction.ContainsKey("pagibig_loan") ? loanDeduction["pagibig_loan"] : "0.00";
                        textBoxCarLoanAmountDebitInput.Text = loanDeduction.ContainsKey("car_loan") ? loanDeduction["car_loan"] : "0.00";
                        textBoxHousingLoanAmountDebitInput.Text = loanDeduction.ContainsKey("housing_loan") ? loanDeduction["housing_loan"] : "0.00";
                        textBoxCashAdvanceAmountDebitInput.Text = loanDeduction.ContainsKey("cash_advance") ? loanDeduction["cash_advance"] : "0.00";
                        textBoxCoopLoanAmountDebitInput.Text = loanDeduction.ContainsKey("coop_loan") ? loanDeduction["coop_loan"] : "0.00";
                        textBoxCoopContriAmountDebitInput.Text = loanDeduction.ContainsKey("coop_contribution") ? loanDeduction["coop_contribution"] : "0.00";
                        textBoxOthersAmountDebitInput.Text = loanDeduction.ContainsKey("other_deduction") ? loanDeduction["other_deduction"] : "0.00";
                    }

                    // Totals from PayrollSummary
                    if (summary != null)
                    {
                        labelGrossPayInput.Text = summary.ContainsKey("gross_pay") ? summary["gross_pay"] : "0.00";
                        labelDeductionsInput.Text = summary.ContainsKey("total_deductions") ? summary["total_deductions"] : "0.00";
                        labelOverallTotalInput.Text = summary.ContainsKey("net_pay") ? summary["net_pay"] : "0.00";
                    }

                    // Set date covered
                    labelDateCoveredInput.Text = $"{payroll["cutoff_start"]} to {payroll["cutoff_end"]}";
                }
                else
                {
                    MessageBox.Show("No payroll data found for this employee.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payroll data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            using (ConfirmPayrollUpdate confirmForm = new ConfirmPayrollUpdate())
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                confirmForm.ShowDialog(this);

                if (confirmForm.UserConfirmed)
                {
                    await ExecutePayrollUpdate();
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
                    loansUpdate["sss_loan"] = textBoxSSSLoanAmountDebitInput.Text;
                    loansUpdate["pagibig_loan"] = textBoxPagIbigLoanAmountDebitInput.Text;
                    loansUpdate["car_loan"] = textBoxCarLoanAmountDebitInput.Text;
                    loansUpdate["housing_loan"] = textBoxHousingLoanAmountDebitInput.Text;
                    loansUpdate["cash_advance"] = textBoxCashAdvanceAmountDebitInput.Text;
                    loansUpdate["coop_loan"] = textBoxCoopLoanAmountDebitInput.Text;
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

                MessageBox.Show("Payroll updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Empty event handlers - no calculations, just editing
        private void textBoxDailyRateInput_TextChanged(object sender, EventArgs e) { }

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

        private void textBoxSSSLoanAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxPagIbigLoanAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxCarLoanAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxHousingLoanAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxCashAdvanceAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxCoopLoanAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxCoopContriAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxOthersAmountDebitInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxIncentivesAmountBaseInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxCommissionAmountBaseInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxGasAllowanceAmountBaseInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxFoodAllowanceAmountBaseInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxCommunicationAmountBaseInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxGondolaAmountBaseInput_TextChanged(object sender, EventArgs e) { }

        private void textBoxVacationLeaveCredit_TextChanged(object sender, EventArgs e) { }

        private void textBoxSickLeaveCredit_TextChanged(object sender, EventArgs e) { }

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
                textBoxCarLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxCarLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
                textBoxCoopLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
                textBoxHousingLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
                labelOvertimePerMinuteAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinuteAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbig.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPagIbigAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxPagIbigDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxPagIbigLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxPagIbigLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPayAndAllowances.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPayrollSummary.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelPhilhealth.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPhilhealthAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxPhilhealthDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSalary.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSickLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxSickLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSS.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSSSAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxSSSDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                textBoxSSSLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxSSSLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelVacationLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxVacationLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelWithTax.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelWithTaxAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxWithTaxDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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

