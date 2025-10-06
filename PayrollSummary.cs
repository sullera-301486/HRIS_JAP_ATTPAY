using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class PayrollSummary : Form
    {
        public PayrollExportData ExportData { get; private set; }

        //  Firebase client
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string payrollPeriod;

        //  Data dictionaries
        private Dictionary<string, dynamic> employeeDetails = new Dictionary<string, dynamic>();
        private Dictionary<string, Dictionary<string, string>> workSchedules = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> employmentInfo = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollSummaryData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> govDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> loanDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollEarnings = new Dictionary<string, Dictionary<string, string>>();
        private List<Dictionary<string, string>> attendanceRecords = new List<Dictionary<string, string>>();

        //  Store current employee ID to track selection
        private string currentEmployeeId = "";

        public PayrollSummary(string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();

            // Add this to load data when form loads
            this.Load += async (sender, e) => await LoadPayrollDataAsync();
        }

        public async void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            await LoadPayrollDataAsync();
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

        //  New: Calculate work days based on real calendar (2 weeks: Mon-Fri full day, Sat half day)
        private (int fullDays, int halfDays) CalculateWorkDaysInPeriod(DateTime startDate, DateTime endDate)
        {
            int fullDays = 0;
            int halfDays = 0;

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    halfDays++;
                }
                else if (date.DayOfWeek != DayOfWeek.Sunday)
                {
                    fullDays++;
                }
            }

            return (fullDays, halfDays);
        }

        // New: Government contributions & tax calculations (BI-MONTHLY)
        #region Government Contributions & Tax Calculations

        private decimal CalculateSSSContribution(decimal monthlySalary)
        {
            if (monthlySalary <= 4249.99m) return 157.50m / 2; // Half for bi-monthly
            if (monthlySalary <= 4749.99m) return 180m / 2;
            if (monthlySalary <= 5249.99m) return 202.50m / 2;
            if (monthlySalary <= 5749.99m) return 225m / 2;
            if (monthlySalary <= 6249.99m) return 247.50m / 2;
            if (monthlySalary <= 6749.99m) return 270m / 2;
            return 292.50m / 2;
        }

        private decimal CalculatePhilHealthContribution(decimal monthlySalary)
        {
            decimal rate = 0.04m;
            return Math.Round((monthlySalary * rate) / 2, 2); // Half for bi-monthly
        }

        private decimal CalculatePagibigContribution(decimal monthlySalary)
        {
            decimal contrib = (monthlySalary * 0.01m) / 2; // Half for bi-monthly
            return contrib > 100 ? 100 : contrib;
        }

        private decimal CalculateWithholdingTax(decimal monthlySalary)
        {
            // Calculate tax based on semi-monthly income (monthly salary divided by 2)
            decimal semiMonthlySalary = monthlySalary / 2;
            if (semiMonthlySalary <= 10416.50m) return 0;
            if (semiMonthlySalary <= 16666.00m) return (semiMonthlySalary - 10416.50m) * 0.20m;
            if (semiMonthlySalary <= 33333.00m) return 1250 + (semiMonthlySalary - 16666.00m) * 0.25m;
            if (semiMonthlySalary <= 83333.00m) return 5416.67m + (semiMonthlySalary - 33333.00m) * 0.30m;
            return 20416.67m + (semiMonthlySalary - 83333.00m) * 0.32m;
        }

        private decimal CalculateOvertimePay(decimal dailyRate, decimal overtimeHours)
        {
            decimal hourlyRate = dailyRate / 8m;
            decimal overtimeRate = hourlyRate * 1.25m;
            return Math.Round(overtimeHours * overtimeRate, 2);
        }

        private decimal CalculateOvertimeMinutesPay(decimal dailyRate, decimal overtimeMinutes)
        {
            decimal hourlyRate = dailyRate / 8m;
            decimal minuteRate = (hourlyRate / 60m) * 1.25m;
            return Math.Round(overtimeMinutes * minuteRate, 2);
        }

        #endregion

        private async Task LoadPayrollDataAsync()
        {
            try
            {
                // Clear previous data
                employeeDetails.Clear();
                employmentInfo.Clear();
                payrollData.Clear();
                payrollSummaryData.Clear();
                govDeductions.Clear();
                loanDeductions.Clear();
                payrollEarnings.Clear();
                attendanceRecords.Clear();

                var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                foreach (var emp in empDetails)
                    employeeDetails[emp.Key] = emp.Object;

                await LoadArrayBasedData("EmploymentInfo", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        employmentInfo[employeeId] = item;
                });

                await LoadArrayBasedData("Work_Schedule", (item) =>
                {
                    var scheduleId = item.ContainsKey("schedule_id") ? item["schedule_id"] : null;
                    if (!string.IsNullOrEmpty(scheduleId))
                        workSchedules[scheduleId] = item;
                });

                await LoadArrayBasedData("Payroll", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        payrollData[employeeId] = item;
                });

                await LoadArrayBasedData("PayrollSummary", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        payrollSummaryData[payrollId] = item;
                });

                await LoadArrayBasedData("LoansAndOtherDeductions", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        loanDeductions[payrollId] = item;
                });

                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        payrollEarnings[payrollId] = item;
                });

                await LoadArrayBasedData("Attendance", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        attendanceRecords.Add(item);
                });

                if (!string.IsNullOrEmpty(currentEmployeeId) && payrollData.ContainsKey(currentEmployeeId))
                {
                    var emp = employeeDetails.ContainsKey(currentEmployeeId) ? employeeDetails[currentEmployeeId] : null;
                    var payroll = payrollData[currentEmployeeId];
                    var payrollId = payroll["payroll_id"];
                    var summary = payrollSummaryData.ContainsKey(payrollId) ? payrollSummaryData[payrollId] : null;
                    var loan = loanDeductions.ContainsKey(payrollId) ? loanDeductions[payrollId] : null;
                    var earnings = payrollEarnings.ContainsKey(payrollId) ? payrollEarnings[payrollId] : null;

                    // FIX: Find employment info by searching for the employee ID in all records
                    Dictionary<string, string> employment = null;
                    foreach (var empInfo in employmentInfo.Values)
                    {
                        if (empInfo.ContainsKey("employee_id") && empInfo["employee_id"] == currentEmployeeId)
                        {
                            employment = empInfo;
                            break;
                        }
                    }

                    if (emp != null)
                    {
                        // Basic employee info
                        labelIDInput.Text = currentEmployeeId;
                        labelNameInput.Text = $"{emp["last_name"]}, {emp["first_name"]} {emp["middle_name"]}";
                        labelDepartmentInput.Text = employment != null && employment.ContainsKey("department") ? employment["department"] : "";
                        labelPositionInput.Text = employment != null && employment.ContainsKey("position") ? employment["position"] : "";
                        labelDateCoveredInput.Text = $"{payroll["cutoff_start"]} to {payroll["cutoff_end"]}";

                        // Parse rates safely
                        decimal dailyRate = 0m;
                        if (employment != null && employment.ContainsKey("daily_rate") && !string.IsNullOrEmpty(employment["daily_rate"]))
                        {
                            decimal.TryParse(employment["daily_rate"], out dailyRate);
                        }
                        labelDailyRateInput.Text = dailyRate.ToString("0.00");

                        // Determine required days (Mon-Fri full, Sat half)
                        DateTime startDate, endDate;
                        int requiredDays = 0;
                        if (payroll.ContainsKey("cutoff_start") && payroll.ContainsKey("cutoff_end") &&
                            DateTime.TryParse(payroll["cutoff_start"], out startDate) &&
                            DateTime.TryParse(payroll["cutoff_end"], out endDate) &&
                            startDate <= endDate)
                        {
                            var (fullDays, halfDays) = CalculateWorkDaysInPeriod(startDate, endDate);
                            requiredDays = fullDays + halfDays;
                        }
                        labelDaysInput.Text = requiredDays.ToString();

                        // Actual days worked (attendance)
                        int daysWorked = CalculateDaysWorked(currentEmployeeId, payroll["cutoff_start"], payroll["cutoff_end"]);
                        labelDaysPresentInput.Text = daysWorked.ToString();

                        // TOTAL SALARY (expected if attend all required days): dailyRate * requiredDays
                        decimal totalSalary = dailyRate * requiredDays;
                        labelSalaryInput.Text = totalSalary.ToString("0.00");

                        // BASIC PAY (attendance-based): dailyRate * daysWorked
                        decimal basicPay = dailyRate * daysWorked;
                        labelBasicPayAmountBaseInput.Text = basicPay.ToString("0.00");
                        labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");

                        // OVERTIME
                        decimal totalOvertime = CalculateOvertimeForEmployee(currentEmployeeId, payroll["cutoff_start"], payroll["cutoff_end"]);
                        labelOvertimeInput.Text = totalOvertime.ToString("0.00");

                        decimal overtimePay = CalculateOvertimePay(dailyRate, totalOvertime);
                        labelOvertimePerHourAmountCreditInput.Text = overtimePay.ToString("0.00");

                        // OVERTIME RATE breakdown
                        decimal overtimePerHourRate = (dailyRate / 8m);
                        decimal overtimePerMinuteRate = (overtimePerHourRate / 60m);
                        labelOvertimePerHourAmountBaseInput.Text = overtimePerHourRate.ToString("0.00");
                        labelOvertimePerMinuteAmountBaseInput.Text = overtimePerMinuteRate.ToString("0.00");

                        // Earnings (period-based allowances)
                        if (earnings != null)
                        {
                            decimal commission = earnings.ContainsKey("commission") ? decimal.Parse(earnings["commission"]) : 0;
                            labelCommissionAmountBaseInput.Text = commission.ToString("0.00");
                            labelCommissionAmountCreditInput.Text = commission.ToString("0.00");

                            decimal communication = earnings.ContainsKey("communication") ? decimal.Parse(earnings["communication"]) : 0;
                            labelCommunicationAmountBaseInput.Text = communication.ToString("0.00");
                            labelCommunicationAmountCreditInput.Text = communication.ToString("0.00");

                            decimal foodAllowance = earnings.ContainsKey("food_allowance") ? decimal.Parse(earnings["food_allowance"]) : 0;
                            labelFoodAllowanceAmountBaseInput.Text = foodAllowance.ToString("0.00");
                            labelFoodAllowanceAmountCreditInput.Text = foodAllowance.ToString("0.00");

                            decimal gasAllowance = earnings.ContainsKey("gas_allowance") ? decimal.Parse(earnings["gas_allowance"]) : 0;
                            labelGasAllowanceAmountBaseInput.Text = gasAllowance.ToString("0.00");
                            labelGasAllowanceAmountCreditInput.Text = gasAllowance.ToString("0.00");

                            decimal gondola = earnings.ContainsKey("gondola") ? decimal.Parse(earnings["gondola"]) : 0;
                            labelGondolaAmountBaseInput.Text = gondola.ToString("0.00");
                            labelGondolaAmountCreditInput.Text = gondola.ToString("0.00");

                            decimal incentives = earnings.ContainsKey("incentives") ? decimal.Parse(earnings["incentives"]) : 0;
                            labelIncentivesAmountBaseInput.Text = incentives.ToString("0.00");
                            labelIncentivesAmountCreditInput.Text = incentives.ToString("0.00");
                        }

                        // GOVERNMENT DEDUCTIONS — based on salary he will accumulate in a month
                        decimal computedMonthlySalary = dailyRate * requiredDays * 2;
                        decimal sss = CalculateSSSContribution(computedMonthlySalary);
                        decimal philhealth = CalculatePhilHealthContribution(computedMonthlySalary);
                        decimal pagibig = CalculatePagibigContribution(computedMonthlySalary);
                        decimal withholdingTax = CalculateWithholdingTax(computedMonthlySalary);

                        labelSSSAmountDebitInput.Text = sss.ToString("0.00");
                        labelPhilhealthAmountDebitInput.Text = philhealth.ToString("0.00");
                        labelPagIbigAmountDebitInput.Text = pagibig.ToString("0.00");
                        labelWithTaxAmountDebitInput.Text = withholdingTax.ToString("0.00");

                        // Loan deductions (divided by 2 for bi-monthly)
                        if (loan != null)
                        {
                            decimal carLoan = loan.ContainsKey("car_loan") ? decimal.Parse(loan["car_loan"]) / 2 : 0;
                            labelCarLoanAmountDebitInput.Text = carLoan.ToString("0.00");

                            decimal cashAdvance = loan.ContainsKey("cash_advance") ? decimal.Parse(loan["cash_advance"]) / 2 : 0;
                            labelCashAdvanceAmountDebitInput.Text = cashAdvance.ToString("0.00");

                            decimal coopLoan = loan.ContainsKey("coop_loan") ? decimal.Parse(loan["coop_loan"]) / 2 : 0;
                            labelCoopLoanAmountDebitInput.Text = coopLoan.ToString("0.00");

                            decimal housingLoan = loan.ContainsKey("housing_loan") ? decimal.Parse(loan["housing_loan"]) / 2 : 0;
                            labelHousingLoanAmountDebitInput.Text = housingLoan.ToString("0.00");

                            decimal sssLoan = loan.ContainsKey("sss_loan") ? decimal.Parse(loan["sss_loan"]) / 2 : 0;
                            labelSSSLoanAmountDebitInput.Text = sssLoan.ToString("0.00");

                            decimal pagibigLoan = loan.ContainsKey("pagibig_loan") ? decimal.Parse(loan["pagibig_loan"]) / 2 : 0;
                            labelPagIbigLoanAmountDebitInput.Text = pagibigLoan.ToString("0.00");

                            decimal coopContribution = loan.ContainsKey("coop_contribution") ? decimal.Parse(loan["coop_contribution"]) / 2 : 0;
                            labelCoopContriAmountDebitInput.Text = coopContribution.ToString("0.00");

                            decimal otherDeduction = loan.ContainsKey("other_deduction") ? decimal.Parse(loan["other_deduction"]) / 2 : 0;
                            labelOthersAmountDebitInput.Text = otherDeduction.ToString("0.00");
                        }

                        // GROSS PAY (for this period)
                        decimal grossPay = basicPay + overtimePay;
                        if (earnings != null)
                        {
                            grossPay += earnings.ContainsKey("commission") ? decimal.Parse(earnings["commission"]) : 0;
                            grossPay += earnings.ContainsKey("communication") ? decimal.Parse(earnings["communication"]) : 0;
                            grossPay += earnings.ContainsKey("food_allowance") ? decimal.Parse(earnings["food_allowance"]) : 0;
                            grossPay += earnings.ContainsKey("gas_allowance") ? decimal.Parse(earnings["gas_allowance"]) : 0;
                            grossPay += earnings.ContainsKey("gondola") ? decimal.Parse(earnings["gondola"]) : 0;
                            grossPay += earnings.ContainsKey("incentives") ? decimal.Parse(earnings["incentives"]) : 0;
                        }
                        labelGrossPayInput.Text = grossPay.ToString("0.00");

                        // TOTAL DEDUCTIONS (gov + loans)
                        decimal totalDeductions = sss + philhealth + pagibig + withholdingTax;
                        if (loan != null)
                        {
                            totalDeductions += loan.ContainsKey("car_loan") ? decimal.Parse(loan["car_loan"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("cash_advance") ? decimal.Parse(loan["cash_advance"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("coop_loan") ? decimal.Parse(loan["coop_loan"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("housing_loan") ? decimal.Parse(loan["housing_loan"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("sss_loan") ? decimal.Parse(loan["sss_loan"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("pagibig_loan") ? decimal.Parse(loan["pagibig_loan"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("coop_contribution") ? decimal.Parse(loan["coop_contribution"]) / 2 : 0;
                            totalDeductions += loan.ContainsKey("other_deduction") ? decimal.Parse(loan["other_deduction"]) / 2 : 0;
                        }
                        labelDeductionsInput.Text = totalDeductions.ToString("0.00");

                        // NET PAY (for this period)
                        decimal netPay = grossPay - totalDeductions;
                        labelOverallTotalInput.Text = netPay.ToString("0.00");

                        ExportData = new PayrollExportData
                        {
                            EmployeeId = currentEmployeeId,
                            EmployeeName = $"{emp["last_name"]}, {emp["first_name"]} {emp["middle_name"]}",
                            Department = employment != null && employment.ContainsKey("department") ? employment["department"] : "",
                            Position = employment != null && employment.ContainsKey("position") ? employment["position"] : "",
                            DateCovered = $"{payroll["cutoff_start"]} to {payroll["cutoff_end"]}",
                            Days = requiredDays.ToString(),
                            DaysPresent = daysWorked.ToString(),
                            DailyRate = dailyRate.ToString("0.00"),
                            Salary = totalSalary.ToString("0.00"),
                            Overtime = totalOvertime.ToString("0.00"),
                            BasicPay = basicPay.ToString("0.00"),
                            OvertimePerHour = overtimePay.ToString("0.00"),
                            OvertimePerMinute = CalculateOvertimeMinutesPay(dailyRate, 1).ToString("0.00"), // Rate per minute
                            Incentives = earnings != null && earnings.ContainsKey("incentives") ? decimal.Parse(earnings["incentives"]).ToString("0.00") : "0.00",
                            Commission = earnings != null && earnings.ContainsKey("commission") ? decimal.Parse(earnings["commission"]).ToString("0.00") : "0.00",
                            FoodAllowance = earnings != null && earnings.ContainsKey("food_allowance") ? decimal.Parse(earnings["food_allowance"]).ToString("0.00") : "0.00",
                            Communication = earnings != null && earnings.ContainsKey("communication") ? decimal.Parse(earnings["communication"]).ToString("0.00") : "0.00",
                            GasAllowance = earnings != null && earnings.ContainsKey("gas_allowance") ? decimal.Parse(earnings["gas_allowance"]).ToString("0.00") : "0.00",
                            Gondola = earnings != null && earnings.ContainsKey("gondola") ? decimal.Parse(earnings["gondola"]).ToString("0.00") : "0.00",
                            GrossPay = grossPay.ToString("0.00"),
                            WithholdingTax = withholdingTax.ToString("0.00"),
                            SSS = sss.ToString("0.00"),
                            PagIbig = pagibig.ToString("0.00"),
                            Philhealth = philhealth.ToString("0.00"),
                            SSSLoan = loan != null && loan.ContainsKey("sss_loan") ? (decimal.Parse(loan["sss_loan"]) / 2).ToString("0.00") : "0.00",
                            PagIbigLoan = loan != null && loan.ContainsKey("pagibig_loan") ? (decimal.Parse(loan["pagibig_loan"]) / 2).ToString("0.00") : "0.00",
                            CarLoan = loan != null && loan.ContainsKey("car_loan") ? (decimal.Parse(loan["car_loan"]) / 2).ToString("0.00") : "0.00",
                            HousingLoan = loan != null && loan.ContainsKey("housing_loan") ? (decimal.Parse(loan["housing_loan"]) / 2).ToString("0.00") : "0.00",
                            CashAdvance = loan != null && loan.ContainsKey("cash_advance") ? (decimal.Parse(loan["cash_advance"]) / 2).ToString("0.00") : "0.00",
                            CoopLoan = loan != null && loan.ContainsKey("coop_loan") ? (decimal.Parse(loan["coop_loan"]) / 2).ToString("0.00") : "0.00",
                            CoopContribution = loan != null && loan.ContainsKey("coop_contribution") ? (decimal.Parse(loan["coop_contribution"]) / 2).ToString("0.00") : "0.00",
                            Others = loan != null && loan.ContainsKey("other_deduction") ? (decimal.Parse(loan["other_deduction"]) / 2).ToString("0.00") : "0.00",
                            TotalDeductions = totalDeductions.ToString("0.00"),
                            NetPay = netPay.ToString("0.00"),
                            // Leave balances - test value; needs to be replaced with data from Firebase if available
                            VacationLeaveCredit = "6.00",
                            VacationLeaveDebit = "2.00",
                            VacationLeaveBalance = "4.00",
                            SickLeaveCredit = "6.00",
                            SickLeaveDebit = "6.00",
                            SickLeaveBalance = "0.00"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payroll data: " + ex.Message);
            }
        }

        private decimal CalculateOvertimeForEmployee(string employeeId, string cutoffStart, string cutoffEnd)
        {
            decimal totalOvertime = 0;
            foreach (var attendance in attendanceRecords)
            {
                if (attendance.ContainsKey("employee_id") && attendance["employee_id"] == employeeId &&
                    attendance.ContainsKey("attendance_date") && IsDateInRange(attendance["attendance_date"], cutoffStart, cutoffEnd))
                {
                    if (attendance.ContainsKey("overtime_hours") && decimal.TryParse(attendance["overtime_hours"], out decimal overtime))
                    {
                        totalOvertime += overtime;
                    }
                }
            }
            return totalOvertime;
        }

        private int CalculateDaysWorked(string employeeId, string cutoffStart, string cutoffEnd)
        {
            int daysWorked = 0;
            foreach (var attendance in attendanceRecords)
            {
                if (attendance.ContainsKey("employee_id") && attendance["employee_id"] == employeeId &&
                    attendance.ContainsKey("attendance_date") && IsDateInRange(attendance["attendance_date"], cutoffStart, cutoffEnd) &&
                    attendance.ContainsKey("status") && attendance["status"] != "Absent" &&
                    attendance.ContainsKey("time_in") && !string.IsNullOrEmpty(attendance["time_in"]))
                {
                    daysWorked++;
                }
            }
            return daysWorked;
        }

        private decimal CalculateBasicPay(string employeeId, string cutoffStart, string cutoffEnd)
        {
            decimal dailyRate = 0;
            if (employmentInfo.ContainsKey(employeeId) && employmentInfo[employeeId].ContainsKey("daily_rate"))
            {
                decimal.TryParse(employmentInfo[employeeId]["daily_rate"], out dailyRate);
            }
            int daysWorked = CalculateDaysWorked(employeeId, cutoffStart, cutoffEnd);
            return dailyRate * daysWorked;
        }

        private decimal CalculateOvertimePay(string employeeId, string cutoffStart, string cutoffEnd)
        {
            decimal dailyRate = 0;
            if (employmentInfo.ContainsKey(employeeId) && employmentInfo[employeeId].ContainsKey("daily_rate"))
            {
                decimal.TryParse(employmentInfo[employeeId]["daily_rate"], out dailyRate);
            }
            decimal hourlyRate = dailyRate / 8;
            decimal overtimeRate = hourlyRate * 1.25m;
            decimal totalOvertimeHours = CalculateOvertimeForEmployee(employeeId, cutoffStart, cutoffEnd);
            return totalOvertimeHours * overtimeRate;
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

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (ExportData == null)
            {
                MessageBox.Show("No payroll data to export. Please load employee data first.");
                return;
            }

            Form parentForm = this.FindForm();
            ConfirmPayrollExportIndividual confirmForm = new ConfirmPayrollExportIndividual(ExportData);
            AttributesClass.ShowWithOverlay(parentForm, confirmForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            PayrollSummaryEdit payrollSummaryEditForm = new PayrollSummaryEdit(currentEmployeeId);
            AttributesClass.ShowWithOverlay(parentForm, payrollSummaryEditForm);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
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
                labelCashAdvanceAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCashAdvanceDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommission.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCommissionAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommissionAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommunication.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCommunicationAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommunicationAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopContri.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCoopContriAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopContriDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCoopLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDetails.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelFoodAllowance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelFoodAllowanceAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelFoodAllowanceAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGasAllowance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGasAllowanceAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGasAllowanceAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGondola.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGondolaAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGondolaAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGrossPay.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGrossPayInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHousingLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelHousingLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelIncentives.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelIncentivesAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelIncentivesAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveBalance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOthers.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOthersAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOthersDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
                buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonExport.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}