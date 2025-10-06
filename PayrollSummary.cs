using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class PayrollSummary : Form
    {
        public PayrollExportData ExportData { get; private set; }

        // Firebase client
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        private string payrollPeriod;

        // Data dictionaries
        private Dictionary<string, dynamic> employeeDetails = new Dictionary<string, dynamic>();
        private Dictionary<string, Dictionary<string, string>> workSchedules = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> employmentInfo = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollSummaryData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> govDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> employeeDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollEarnings = new Dictionary<string, Dictionary<string, string>>();
        private List<Dictionary<string, string>> attendanceRecords = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> employeeLoans = new List<Dictionary<string, string>>();

        // Store current employee ID to track selection
        private string currentEmployeeId = "";

        public PayrollSummary(string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();

            // Load data when form loads
            this.Load += async (sender, e) => await LoadPayrollDataAsync();
        }

        public async void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            await LoadPayrollDataAsync();
        }

        // Improved JSON parsing method
        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();
            try
            {
                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null")
                    return records;

                // Handle array with null first element
                if (rawJson.StartsWith("[null,"))
                {
                    rawJson = rawJson.Replace("[null,", "[");
                }
                if (rawJson.StartsWith("[null]"))
                {
                    return records;
                }

                // Try to parse as JObject first (for object structures)
                try
                {
                    var jObject = JObject.Parse(rawJson);
                    foreach (var property in jObject.Properties())
                    {
                        if (property.Value.Type == JTokenType.Object)
                        {
                            var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (JProperty prop in property.Value.Children<JProperty>())
                            {
                                record[prop.Name] = prop.Value?.ToString() ?? "";
                            }
                            records.Add(record);
                        }
                    }
                    return records;
                }
                catch
                {
                    // Continue with regex parsing if JObject fails
                }

                // Normalization for regex parsing
                string cleanedJson = rawJson.Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"").Replace("], [", ",").Replace("}, {", "},{")
                    .Replace("},(", "},{").Replace("),{", "},{");

                // Find all {...} blocks
                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");

                foreach (Match match in matches)
                {
                    try
                    {
                        string objectStr = match.Value;
                        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        // Improved regex to handle various value types
                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(?:""([^""]*)"")?",
                            RegexOptions.IgnoreCase);

                        foreach (Match kvpMatch in kvpMatches)
                        {
                            if (kvpMatch.Groups.Count >= 3)
                            {
                                string key = kvpMatch.Groups[1].Value;
                                string value = kvpMatch.Groups[2].Success ? kvpMatch.Groups[2].Value : "";

                                // Handle null values
                                if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
                                    value = "";

                                record[key] = value;
                            }
                        }

                        if (record.Count > 0)
                            records.Add(record);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("JSON parsing error: " + ex.Message);
            }

            return records;
        }

        private async Task LoadArrayBasedData(string childPath, Action<Dictionary<string, string>> processItem)
        {
            try
            {
                var jsonResponse = await firebase.Child(childPath).OnceAsJsonAsync();
                string rawJson = jsonResponse?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null")
                    return;

                var records = ParseMalformedJson(rawJson);

                foreach (var record in records)
                {
                    if (record != null && record.Count > 0)
                    {
                        processItem(record);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {childPath}: {ex.Message}");
            }
        }

        // Calculate work days based on real calendar (Mon-Fri full day, Sat half day)
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

        #region Government Contributions & Tax Calculations

        private decimal CalculateSSSContribution(decimal monthlySalary)
        {
            // SSS contribution table (2024 rates)
            if (monthlySalary <= 4249.99m) return 180.00m / 2; // Bi-monthly
            if (monthlySalary <= 4749.99m) return 202.50m / 2;
            if (monthlySalary <= 5249.99m) return 225.00m / 2;
            if (monthlySalary <= 5749.99m) return 247.50m / 2;
            if (monthlySalary <= 6249.99m) return 270.00m / 2;
            if (monthlySalary <= 6749.99m) return 292.50m / 2;
            if (monthlySalary <= 7249.99m) return 315.00m / 2;
            if (monthlySalary <= 7749.99m) return 337.50m / 2;
            if (monthlySalary <= 8249.99m) return 360.00m / 2;
            if (monthlySalary <= 8749.99m) return 382.50m / 2;
            if (monthlySalary <= 9249.99m) return 405.00m / 2;
            if (monthlySalary <= 9749.99m) return 427.50m / 2;
            if (monthlySalary <= 10249.99m) return 450.00m / 2;
            if (monthlySalary <= 10749.99m) return 472.50m / 2;
            if (monthlySalary <= 11249.99m) return 495.00m / 2;
            if (monthlySalary <= 11749.99m) return 517.50m / 2;
            if (monthlySalary <= 12249.99m) return 540.00m / 2;
            if (monthlySalary <= 12749.99m) return 562.50m / 2;
            if (monthlySalary <= 13249.99m) return 585.00m / 2;
            if (monthlySalary <= 13749.99m) return 607.50m / 2;
            if (monthlySalary <= 14249.99m) return 630.00m / 2;
            if (monthlySalary <= 14749.99m) return 652.50m / 2;
            if (monthlySalary <= 15249.99m) return 675.00m / 2;
            if (monthlySalary <= 15749.99m) return 697.50m / 2;
            if (monthlySalary <= 16249.99m) return 720.00m / 2;
            if (monthlySalary <= 16749.99m) return 742.50m / 2;
            if (monthlySalary <= 17249.99m) return 765.00m / 2;
            if (monthlySalary <= 17749.99m) return 787.50m / 2;
            if (monthlySalary <= 18249.99m) return 810.00m / 2;
            if (monthlySalary <= 18749.99m) return 832.50m / 2;
            if (monthlySalary <= 19249.99m) return 855.00m / 2;
            if (monthlySalary <= 19749.99m) return 877.50m / 2;
            if (monthlySalary <= 20249.99m) return 900.00m / 2;
            if (monthlySalary <= 20749.99m) return 922.50m / 2;
            if (monthlySalary <= 21249.99m) return 945.00m / 2;
            if (monthlySalary <= 21749.99m) return 967.50m / 2;
            if (monthlySalary <= 22249.99m) return 990.00m / 2;
            if (monthlySalary <= 22749.99m) return 1012.50m / 2;
            if (monthlySalary <= 23249.99m) return 1035.00m / 2;
            if (monthlySalary <= 23749.99m) return 1057.50m / 2;
            if (monthlySalary <= 24249.99m) return 1080.00m / 2;
            if (monthlySalary <= 24749.99m) return 1102.50m / 2;
            return 1125.00m / 2; // Maximum contribution
        }

        private decimal CalculatePhilHealthContribution(decimal monthlySalary)
        {
            // PhilHealth rate 4% split between employer and employee (2.75% employee share)
            decimal employeeShareRate = 0.0275m;
            decimal contribution = monthlySalary * employeeShareRate;

            // Minimum and maximum limits
            if (contribution < 200m) contribution = 200m;
            if (contribution > 400m) contribution = 400m;

            return Math.Round(contribution / 2, 2); // Half for bi-monthly
        }

        private decimal CalculatePagibigContribution(decimal monthlySalary)
        {
            // Pag-IBIG: 2% if monthly salary > 1500, otherwise 1%
            decimal rate = monthlySalary > 1500m ? 0.02m : 0.01m;
            decimal contribution = monthlySalary * rate;

            // Maximum contribution of 100
            return Math.Round(Math.Min(contribution, 100m) / 2, 2); // Half for bi-monthly
        }

        private decimal CalculateWithholdingTax(decimal monthlySalary)
        {
            // Calculate tax based on semi-monthly income
            decimal semiMonthlySalary = monthlySalary / 2;

            if (semiMonthlySalary <= 10417m) return 0;
            if (semiMonthlySalary <= 16666m) return (semiMonthlySalary - 10417m) * 0.15m;
            if (semiMonthlySalary <= 33332m) return 937.50m + (semiMonthlySalary - 16666m) * 0.20m;
            if (semiMonthlySalary <= 83332m) return 4270.50m + (semiMonthlySalary - 33332m) * 0.25m;
            if (semiMonthlySalary <= 333332m) return 16770.50m + (semiMonthlySalary - 83332m) * 0.30m;
            return 91770.50m + (semiMonthlySalary - 333332m) * 0.35m;
        }

        private decimal CalculateOvertimePay(decimal dailyRate, decimal overtimeHours)
        {
            decimal hourlyRate = dailyRate / 8m;
            decimal overtimeRate = hourlyRate * 1.25m; // 25% premium for overtime
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
                employeeDeductions.Clear();
                payrollEarnings.Clear();
                attendanceRecords.Clear();
                employeeLoans.Clear();

                // Load EmployeeDetails
                var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                foreach (var e in empDetails)
                {
                    if (e.Object != null)
                    {
                        employeeDetails[e.Key] = e.Object;
                    }
                }

                // Load EmploymentInfo - handle mixed key formats
                var employmentData = await firebase.Child("EmploymentInfo").OnceAsync<dynamic>();
                foreach (var e in employmentData)
                {
                    if (e.Object != null)
                    {
                        var empDict = new Dictionary<string, string>();
                        foreach (var prop in e.Object)
                        {
                            empDict[prop.Key] = prop.Value?.ToString() ?? "";
                        }

                        if (empDict.ContainsKey("employee_id"))
                        {
                            employmentInfo[empDict["employee_id"]] = empDict;
                        }
                    }
                }

                // Load other data using array-based method
                await LoadArrayBasedData("Payroll", (item) =>
                {
                    if (item.ContainsKey("employee_id"))
                    {
                        payrollData[item["employee_id"]] = item;
                    }
                });

                await LoadArrayBasedData("PayrollSummary", (item) =>
                {
                    if (item.ContainsKey("payroll_id"))
                    {
                        payrollSummaryData[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("EmployeeDeductions", (item) =>
                {
                    if (item.ContainsKey("payroll_id"))
                    {
                        employeeDeductions[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("GovernmentDeductions", (item) =>
                {
                    if (item.ContainsKey("payroll_id"))
                    {
                        govDeductions[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    if (item.ContainsKey("payroll_id"))
                    {
                        payrollEarnings[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("EmployeeLoans", (item) =>
                {
                    if (item != null && item.Count > 0)
                    {
                        employeeLoans.Add(item);
                    }
                });

                await LoadArrayBasedData("Attendance", (item) =>
                {
                    if (item != null && item.Count > 0)
                    {
                        attendanceRecords.Add(item);
                    }
                });

                // Validate employee data
                if (string.IsNullOrEmpty(currentEmployeeId))
                {
                    MessageBox.Show("No employee selected.");
                    return;
                }

                if (!employeeDetails.ContainsKey(currentEmployeeId))
                {
                    MessageBox.Show($"Employee details not found for {currentEmployeeId}");
                    return;
                }

                // Get employee and employment data
                var emp = employeeDetails[currentEmployeeId];
                Dictionary<string, string> employment = employmentInfo.ContainsKey(currentEmployeeId) ?
                    employmentInfo[currentEmployeeId] : null;

                // Find payroll data for this employee
                Dictionary<string, string> payroll = null;
                if (payrollData.ContainsKey(currentEmployeeId))
                {
                    payroll = payrollData[currentEmployeeId];
                }
                else
                {
                    // Search through all payroll records
                    foreach (var p in payrollData.Values)
                    {
                        if (p.ContainsKey("employee_id") && p["employee_id"] == currentEmployeeId)
                        {
                            payroll = p;
                            break;
                        }
                    }
                }

                // Set basic employee information
                labelIDInput.Text = currentEmployeeId;
                labelNameInput.Text = $"{emp["last_name"] ?? ""}, {emp["first_name"] ?? ""} {emp["middle_name"] ?? ""}".Trim();
                labelDepartmentInput.Text = employment != null && employment.ContainsKey("department") ? employment["department"] : "";
                labelPositionInput.Text = employment != null && employment.ContainsKey("position") ? employment["position"] : "";

                // Handle payroll period
                string cutoffStart = "";
                string cutoffEnd = "";

                if (payroll != null)
                {
                    cutoffStart = payroll.ContainsKey("cutoff_start") ? payroll["cutoff_start"] : "";
                    cutoffEnd = payroll.ContainsKey("cutoff_end") ? payroll["cutoff_end"] : "";
                }

                if (string.IsNullOrEmpty(cutoffStart) && !string.IsNullOrEmpty(payrollPeriod))
                {
                    cutoffStart = payrollPeriod;
                    cutoffEnd = payrollPeriod;
                }

                labelDateCoveredInput.Text = (!string.IsNullOrEmpty(cutoffStart) && !string.IsNullOrEmpty(cutoffEnd)) ?
                    $"{cutoffStart} to {cutoffEnd}" : "Not specified";

                // Calculate daily rate
                decimal dailyRate = 0m;
                if (employment != null && employment.ContainsKey("daily_rate") && !string.IsNullOrEmpty(employment["daily_rate"]))
                {
                    decimal.TryParse(employment["daily_rate"], out dailyRate);
                }

                // Fallback: Check payroll earnings for daily rate
                string payrollId = payroll != null && payroll.ContainsKey("payroll_id") ? payroll["payroll_id"] : "";
                if (dailyRate == 0m && !string.IsNullOrEmpty(payrollId) && payrollEarnings.ContainsKey(payrollId) &&
                    payrollEarnings[payrollId].ContainsKey("daily_rate"))
                {
                    decimal.TryParse(payrollEarnings[payrollId]["daily_rate"], out dailyRate);
                }

                labelDailyRateInput.Text = dailyRate.ToString("0.00");

                // Calculate required work days
                int requiredDays = 0;
                if (!string.IsNullOrEmpty(cutoffStart) && !string.IsNullOrEmpty(cutoffEnd) &&
                    DateTime.TryParse(cutoffStart, out DateTime startDate) && DateTime.TryParse(cutoffEnd, out DateTime endDate))
                {
                    var (fullDays, halfDays) = CalculateWorkDaysInPeriod(startDate, endDate);
                    requiredDays = fullDays + halfDays;
                }
                labelDaysInput.Text = requiredDays.ToString();

                // Calculate actual days worked
                int daysWorked = CalculateDaysWorked(currentEmployeeId, cutoffStart, cutoffEnd);
                labelDaysPresentInput.Text = daysWorked.ToString();

                // Calculate basic salary components
                decimal totalSalary = dailyRate * requiredDays;
                labelSalaryInput.Text = totalSalary.ToString("0.00");

                decimal basicPay = dailyRate * daysWorked;
                labelBasicPayAmountBaseInput.Text = basicPay.ToString("0.00");
                labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");

                // Calculate overtime
                decimal totalOvertimeHours = CalculateOvertimeForEmployee(currentEmployeeId, cutoffStart, cutoffEnd);
                labelOvertimeInput.Text = totalOvertimeHours.ToString("0.00");

                decimal overtimePay = CalculateOvertimePay(dailyRate, totalOvertimeHours);
                labelOvertimePerHourAmountCreditInput.Text = overtimePay.ToString("0.00");

                decimal overtimePerHourRate = dailyRate / 8m;
                decimal overtimePerMinuteRate = overtimePerHourRate / 60m;
                labelOvertimePerHourAmountBaseInput.Text = overtimePerHourRate.ToString("0.00");
                labelOvertimePerMinuteAmountBaseInput.Text = overtimePerMinuteRate.ToString("0.00");
                labelOvertimePerMinuteAmountCreditInput.Text = CalculateOvertimeMinutesPay(dailyRate, 1).ToString("0.00");

                // Load earnings data
                Dictionary<string, string> earnings = !string.IsNullOrEmpty(payrollId) && payrollEarnings.ContainsKey(payrollId) ?
                    payrollEarnings[payrollId] : null;

                decimal commission = earnings != null && earnings.ContainsKey("commission") && decimal.TryParse(earnings["commission"], out decimal cVal) ? cVal : 0m;
                decimal communication = earnings != null && earnings.ContainsKey("communication") && decimal.TryParse(earnings["communication"], out decimal comVal) ? comVal : 0m;
                decimal foodAllowance = earnings != null && earnings.ContainsKey("food_allowance") && decimal.TryParse(earnings["food_allowance"], out decimal faVal) ? faVal : 0m;
                decimal gasAllowance = earnings != null && earnings.ContainsKey("gas_allowance") && decimal.TryParse(earnings["gas_allowance"], out decimal gaVal) ? gaVal : 0m;
                decimal gondola = earnings != null && earnings.ContainsKey("gondola") && decimal.TryParse(earnings["gondola"], out decimal gVal) ? gVal : 0m;
                decimal incentives = earnings != null && earnings.ContainsKey("incentives") && decimal.TryParse(earnings["incentives"], out decimal inVal) ? inVal : 0m;

                labelCommissionAmountBaseInput.Text = commission.ToString("0.00");
                labelCommissionAmountCreditInput.Text = commission.ToString("0.00");
                labelCommunicationAmountBaseInput.Text = communication.ToString("0.00");
                labelCommunicationAmountCreditInput.Text = communication.ToString("0.00");
                labelFoodAllowanceAmountBaseInput.Text = foodAllowance.ToString("0.00");
                labelFoodAllowanceAmountCreditInput.Text = foodAllowance.ToString("0.00");
                labelGasAllowanceAmountBaseInput.Text = gasAllowance.ToString("0.00");
                labelGasAllowanceAmountCreditInput.Text = gasAllowance.ToString("0.00");
                labelGondolaAmountBaseInput.Text = gondola.ToString("0.00");
                labelGondolaAmountCreditInput.Text = gondola.ToString("0.00");
                labelIncentivesAmountBaseInput.Text = incentives.ToString("0.00");
                labelIncentivesAmountCreditInput.Text = incentives.ToString("0.00");

                // Calculate government contributions (bi-monthly)
                decimal monthlySalary = dailyRate * requiredDays * 2; // Approximate monthly salary
                decimal sss = CalculateSSSContribution(monthlySalary);
                decimal philhealth = CalculatePhilHealthContribution(monthlySalary);
                decimal pagibig = CalculatePagibigContribution(monthlySalary);
                decimal withholdingTax = CalculateWithholdingTax(monthlySalary);

                labelSSSAmountDebitInput.Text = sss.ToString("0.00");
                labelPhilhealthAmountDebitInput.Text = philhealth.ToString("0.00");
                labelPagIbigAmountDebitInput.Text = pagibig.ToString("0.00");
                labelWithTaxAmountDebitInput.Text = withholdingTax.ToString("0.00");

                // Load employee deductions
                decimal cashAdvance = 0m, coopContribution = 0m, otherDeductions = 0m;
                if (!string.IsNullOrEmpty(payrollId) && employeeDeductions.ContainsKey(payrollId))
                {
                    var deductions = employeeDeductions[payrollId];
                    if (deductions.ContainsKey("cash_advance")) decimal.TryParse(deductions["cash_advance"], out cashAdvance);
                    if (deductions.ContainsKey("coop_contribution")) decimal.TryParse(deductions["coop_contribution"], out coopContribution);
                    if (deductions.ContainsKey("other_deductions")) decimal.TryParse(deductions["other_deductions"], out otherDeductions);
                }

                // Calculate loan deductions (bi-monthly payments)
                decimal sssLoan = 0m, pagibigLoan = 0m, carLoan = 0m, housingLoan = 0m, coopLoan = 0m;
                foreach (var loan in employeeLoans)
                {
                    if (loan.ContainsKey("employee_id") && loan["employee_id"] == currentEmployeeId &&
                        loan.ContainsKey("status") && loan["status"] == "Active")
                    {
                        decimal loanAmount = loan.ContainsKey("loan_amount") && decimal.TryParse(loan["loan_amount"], out decimal amt) ? amt : 0m;
                        int totalTerms = loan.ContainsKey("total_payment_terms") && int.TryParse(loan["total_payment_terms"], out int terms) ? terms : 0;

                        if (totalTerms > 0)
                        {
                            decimal monthlyPayment = loanAmount / totalTerms;
                            decimal biMonthlyPayment = monthlyPayment / 2; // Split monthly payment into two

                            string loanType = loan.ContainsKey("loan_type") ? loan["loan_type"].ToLower() : "";

                            if (loanType.Contains("sss")) sssLoan += biMonthlyPayment;
                            else if (loanType.Contains("pagibig") || loanType.Contains("pag-ibig")) pagibigLoan += biMonthlyPayment;
                            else if (loanType.Contains("car")) carLoan += biMonthlyPayment;
                            else if (loanType.Contains("housing")) housingLoan += biMonthlyPayment;
                            else if (loanType.Contains("coop")) coopLoan += biMonthlyPayment;
                        }
                    }
                }

                labelSSSLoanAmountDebitInput.Text = sssLoan.ToString("0.00");
                labelPagIbigLoanAmountDebitInput.Text = pagibigLoan.ToString("0.00");
                labelCarLoanAmountDebitInput.Text = carLoan.ToString("0.00");
                labelHousingLoanAmountDebitInput.Text = housingLoan.ToString("0.00");
                labelCoopLoanAmountDebitInput.Text = coopLoan.ToString("0.00");
                labelCashAdvanceAmountDebitInput.Text = cashAdvance.ToString("0.00");
                labelCoopContriAmountDebitInput.Text = coopContribution.ToString("0.00");
                labelOthersAmountDebitInput.Text = otherDeductions.ToString("0.00");

                // Calculate gross pay
                decimal grossPay = basicPay + overtimePay + commission + communication + foodAllowance + gasAllowance + gondola + incentives;
                labelGrossPayInput.Text = grossPay.ToString("0.00");

                // Calculate total deductions
                decimal totalDeductions = sss + philhealth + pagibig + withholdingTax +
                                         sssLoan + pagibigLoan + carLoan + housingLoan + coopLoan +
                                         cashAdvance + coopContribution + otherDeductions;
                labelDeductionsInput.Text = totalDeductions.ToString("0.00");

                // Calculate net pay
                decimal netPay = grossPay - totalDeductions;
                labelOverallTotalInput.Text = netPay.ToString("0.00");

                // Populate export data
                ExportData = new PayrollExportData
                {
                    EmployeeId = currentEmployeeId,
                    EmployeeName = labelNameInput.Text,
                    Department = labelDepartmentInput.Text,
                    Position = labelPositionInput.Text,
                    DateCovered = labelDateCoveredInput.Text,
                    Days = requiredDays.ToString(),
                    DaysPresent = daysWorked.ToString(),
                    DailyRate = dailyRate.ToString("0.00"),
                    Salary = totalSalary.ToString("0.00"),
                    Overtime = totalOvertimeHours.ToString("0.00"),
                    BasicPay = basicPay.ToString("0.00"),
                    OvertimePerHour = overtimePay.ToString("0.00"),
                    OvertimePerMinute = CalculateOvertimeMinutesPay(dailyRate, 1).ToString("0.00"),
                    Incentives = incentives.ToString("0.00"),
                    Commission = commission.ToString("0.00"),
                    FoodAllowance = foodAllowance.ToString("0.00"),
                    Communication = communication.ToString("0.00"),
                    GasAllowance = gasAllowance.ToString("0.00"),
                    Gondola = gondola.ToString("0.00"),
                    GrossPay = grossPay.ToString("0.00"),
                    WithholdingTax = withholdingTax.ToString("0.00"),
                    SSS = sss.ToString("0.00"),
                    PagIbig = pagibig.ToString("0.00"),
                    Philhealth = philhealth.ToString("0.00"),
                    SSSLoan = sssLoan.ToString("0.00"),
                    PagIbigLoan = pagibigLoan.ToString("0.00"),
                    CarLoan = carLoan.ToString("0.00"),
                    HousingLoan = housingLoan.ToString("0.00"),
                    CashAdvance = cashAdvance.ToString("0.00"),
                    CoopLoan = coopLoan.ToString("0.00"),
                    CoopContribution = coopContribution.ToString("0.00"),
                    Others = otherDeductions.ToString("0.00"),
                    TotalDeductions = totalDeductions.ToString("0.00"),
                    NetPay = netPay.ToString("0.00"),
                    VacationLeaveCredit = "0.00",
                    VacationLeaveDebit = "0.00",
                    VacationLeaveBalance = "0.00",
                    SickLeaveCredit = "0.00",
                    SickLeaveDebit = "0.00",
                    SickLeaveBalance = "0.00"
                };

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    attendance.ContainsKey("status") && !string.Equals(attendance["status"], "Absent", StringComparison.OrdinalIgnoreCase) &&
                    attendance.ContainsKey("time_in") && !string.IsNullOrEmpty(attendance["time_in"]) &&
                    !string.Equals(attendance["time_in"], "N/A", StringComparison.OrdinalIgnoreCase))
                {
                    daysWorked++;
                }
            }
            return daysWorked;
        }

        private bool IsDateInRange(string date, string startDate, string endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                    return false;

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
                // Your existing font setting code remains the same
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
                labelOvertimeCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerHour.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimePerHourAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerHourAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinute.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
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