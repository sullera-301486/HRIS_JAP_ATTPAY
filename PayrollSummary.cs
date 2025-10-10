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
        private Dictionary<string, Dictionary<string, string>> employmentInfo = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> leaveCredits = new Dictionary<string, Dictionary<string, string>>();
        private List<Dictionary<string, string>> attendanceRecords = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> employeeLoans = new List<Dictionary<string, string>>();

        // Store current employee ID to track selection
        private string currentEmployeeId = "";

        // Constants for validation
        private const decimal MINIMUM_DAILY_RATE = 500m;
        private const decimal MAX_DEDUCTION_PERCENTAGE = 0.4m;

        // Date coverage
        private DateTime cutoffStartDate = new DateTime(2025, 9, 1);
        private DateTime cutoffEndDate = new DateTime(2025, 9, 15);

        public PayrollSummary(string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();

            this.Load += async (sender, e) => await LoadPayrollDataAsync();
        }

        public async void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            await LoadPayrollDataAsync();
        }

        // Main payroll data loading method
        private async Task LoadPayrollDataAsync()
        {
            try
            {
                // Clear previous data
                employeeDetails.Clear();
                employmentInfo.Clear();
                attendanceRecords.Clear();
                employeeLoans.Clear();
                leaveCredits.Clear();

                // Load all required data
                await LoadEmployeeDetails();
                await LoadEmploymentInfo();
                await LoadEmployeeLoans();
                await LoadAttendanceData();
                await LoadLeaveCredits();

                // Basic validation
                if (string.IsNullOrEmpty(currentEmployeeId) || !employeeDetails.ContainsKey(currentEmployeeId))
                {
                    MessageBox.Show("Employee not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get employee data
                var emp = employeeDetails[currentEmployeeId];
                Dictionary<string, string> employment = employmentInfo.ContainsKey(currentEmployeeId)
                    ? employmentInfo[currentEmployeeId]
                    : new Dictionary<string, string>();

                // Set basic employee information
                labelIDInput.Text = currentEmployeeId;
                labelNameInput.Text = FormatEmployeeName(emp);
                labelDepartmentInput.Text = GetSafeValue(employment, "department") ?? "Not Specified";
                labelPositionInput.Text = GetSafeValue(employment, "position") ?? "Not Specified";

                // Set date covered
                labelDateCoveredInput.Text = $"{cutoffStartDate:MMM dd, yyyy} - {cutoffEndDate:MMM dd, yyyy}";

                // Calculate daily rate
                decimal dailyRate = CalculateDailyRate(employment);
                labelDailyRateInput.Text = dailyRate.ToString("0.00");

                // Calculate work days for the period
                var workDays = CalculateWorkDaysInPeriod(cutoffStartDate, cutoffEndDate);
                int requiredDays = workDays.fullDays + workDays.halfDays;
                labelDaysInput.Text = requiredDays.ToString();

                int daysWorked = CalculateDaysWorked(currentEmployeeId);
                labelDaysPresentInput.Text = daysWorked.ToString();

                // Calculate basic salary
                decimal totalSalary = dailyRate * requiredDays;
                labelSalaryInput.Text = totalSalary.ToString("0.00");

                // Calculate basic pay
                decimal basicPay = dailyRate * daysWorked;
                labelBasicPayAmountBaseInput.Text = basicPay.ToString("0.00");
                labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");

                // Calculate overtime
                var overtimeData = CalculateOvertimeForEmployee(currentEmployeeId);
                decimal totalOvertimeHours = overtimeData.totalHours;
                decimal totalOvertimeMinutes = overtimeData.totalMinutes;

                // Calculate overtime rates
                decimal overtimeHourlyRate = dailyRate / 8m;
                decimal overtimeMinuteRate = overtimeHourlyRate / 60m;

                // Calculate overtime pay
                decimal overtimePayHours = totalOvertimeHours * overtimeHourlyRate * 1.25m; // 25% premium
                decimal overtimePayMinutes = totalOvertimeMinutes * overtimeMinuteRate * 1.25m;
                decimal totalOvertimePay = overtimePayHours + overtimePayMinutes;

                // Display overtime calculations
                labelOvertimeInput.Text = totalOvertimeHours.ToString("0.00");
                labelOvertimePerHourAmountBaseInput.Text = totalOvertimeHours.ToString("0.00");
                labelOvertimePerHourAmountCreditInput.Text = overtimePayHours.ToString("0.00");
                labelOvertimePerMinuteAmountBaseInput.Text = totalOvertimeMinutes.ToString("0.00");
                labelOvertimePerMinuteAmountCreditInput.Text = overtimePayMinutes.ToString("0.00");

                // Load and display earnings from existing payroll data
                var earnings = await LoadEarningsFromPayroll(currentEmployeeId);

                labelCommissionAmountBaseInput.Text = earnings.Commission.ToString("0.00");
                labelCommissionAmountCreditInput.Text = earnings.Commission.ToString("0.00");
                labelCommunicationAmountBaseInput.Text = earnings.Communication.ToString("0.00");
                labelCommunicationAmountCreditInput.Text = earnings.Communication.ToString("0.00");
                labelFoodAllowanceAmountBaseInput.Text = earnings.FoodAllowance.ToString("0.00");
                labelFoodAllowanceAmountCreditInput.Text = earnings.FoodAllowance.ToString("0.00");
                labelGasAllowanceAmountBaseInput.Text = earnings.GasAllowance.ToString("0.00");
                labelGasAllowanceAmountCreditInput.Text = earnings.GasAllowance.ToString("0.00");
                labelGondolaAmountBaseInput.Text = earnings.Gondola.ToString("0.00");
                labelGondolaAmountCreditInput.Text = earnings.Gondola.ToString("0.00");
                labelIncentivesAmountBaseInput.Text = earnings.Incentives.ToString("0.00");
                labelIncentivesAmountCreditInput.Text = earnings.Incentives.ToString("0.00");

                // Calculate government contributions
                decimal monthlySalary = dailyRate * requiredDays * 2;
                var sssContribution = SssCalculator.Compute(monthlySalary);
                decimal sss = sssContribution.EmployeeTotal / 2;
                decimal philhealth = CalculatePhilHealthContribution(monthlySalary);
                decimal pagibig = CalculatePagibigContribution(monthlySalary);
                decimal withholdingTax = CalculateWithholdingTax(monthlySalary);

                labelSSSAmountDebitInput.Text = sss.ToString("0.00");
                labelPhilhealthAmountDebitInput.Text = philhealth.ToString("0.00");
                labelPagIbigAmountDebitInput.Text = pagibig.ToString("0.00");
                labelWithTaxAmountDebitInput.Text = withholdingTax.ToString("0.00");

                // Calculate loan deductions using bi_monthly_amortization
                var loanDeductions = CalculateLoanDeductions(currentEmployeeId);
                labelSSSLoanAmountDebitInput.Text = loanDeductions.sssLoan.ToString("0.00");
                labelPagIbigLoanAmountDebitInput.Text = loanDeductions.pagibigLoan.ToString("0.00");
                labelCarLoanAmountDebitInput.Text = loanDeductions.carLoan.ToString("0.00");
                labelHousingLoanAmountDebitInput.Text = loanDeductions.housingLoan.ToString("0.00");
                labelCoopLoanAmountDebitInput.Text = loanDeductions.coopLoan.ToString("0.00");

                // Load and display other deductions from existing payroll data
                var deductions = await LoadDeductionsFromPayroll(currentEmployeeId);
                labelCashAdvanceAmountDebitInput.Text = deductions.CashAdvance.ToString("0.00");
                labelCoopContriAmountDebitInput.Text = deductions.CoopContribution.ToString("0.00");
                labelOthersAmountDebitInput.Text = deductions.OtherDeductions.ToString("0.00");

                // Update leave credits display
                UpdateLeaveCreditsDisplay(currentEmployeeId);

                // Calculate gross pay (sum of all earnings)
                decimal grossPay = basicPay + totalOvertimePay + earnings.Commission + earnings.Communication +
                                 earnings.FoodAllowance + earnings.GasAllowance + earnings.Gondola + earnings.Incentives;
                labelGrossPayInput.Text = grossPay.ToString("0.00");

                // Calculate total deductions
                decimal totalDeductions = sss + philhealth + pagibig + withholdingTax +
                                         loanDeductions.sssLoan + loanDeductions.pagibigLoan + loanDeductions.carLoan +
                                         loanDeductions.housingLoan + loanDeductions.coopLoan + deductions.CashAdvance +
                                         deductions.CoopContribution + deductions.OtherDeductions;

                totalDeductions = ValidateDeductions(totalDeductions, grossPay);
                labelDeductionsInput.Text = totalDeductions.ToString("0.00");

                // Calculate net pay
                decimal netPay = Math.Max(0, grossPay - totalDeductions);
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
                    OvertimePerHour = totalOvertimePay.ToString("0.00"),
                    OvertimePerMinute = totalOvertimeMinutes.ToString("0.00"),
                    Incentives = earnings.Incentives.ToString("0.00"),
                    Commission = earnings.Commission.ToString("0.00"),
                    FoodAllowance = earnings.FoodAllowance.ToString("0.00"),
                    Communication = earnings.Communication.ToString("0.00"),
                    GasAllowance = earnings.GasAllowance.ToString("0.00"),
                    Gondola = earnings.Gondola.ToString("0.00"),
                    GrossPay = grossPay.ToString("0.00"),
                    WithholdingTax = withholdingTax.ToString("0.00"),
                    SSS = sss.ToString("0.00"),
                    PagIbig = pagibig.ToString("0.00"),
                    Philhealth = philhealth.ToString("0.00"),
                    SSSLoan = loanDeductions.sssLoan.ToString("0.00"),
                    PagIbigLoan = loanDeductions.pagibigLoan.ToString("0.00"),
                    CarLoan = loanDeductions.carLoan.ToString("0.00"),
                    HousingLoan = loanDeductions.housingLoan.ToString("0.00"),
                    CashAdvance = deductions.CashAdvance.ToString("0.00"),
                    CoopLoan = loanDeductions.coopLoan.ToString("0.00"),
                    CoopContribution = deductions.CoopContribution.ToString("0.00"),
                    Others = deductions.OtherDeductions.ToString("0.00"),
                    TotalDeductions = totalDeductions.ToString("0.00"),
                    NetPay = netPay.ToString("0.00"),
                    TaxDetails = labelWithTaxDetails.Text,
                    SSSDetails = labelSSSDetails.Text,
                    PhilhealthDetails = labelPhilhealthDetails.Text,
                    PagIbigDetails = labelPagIbigDetails.Text,
                    SSSLoanDetails = labelSSSLoanDetails.Text,
                    PagIbigLoanDetails = labelPagIbigLoanDetails.Text,
                    CarLoanDetails = labelCarLoanDetails.Text,
                    HousingLoanDetails = labelHousingLoanDetails.Text,
                    CashAdvanceDetails = labelCashAdvanceDetails.Text,
                    CoopLoanDetails = labelCoopLoanDetails.Text,
                    CoopContributionDetails = labelCoopContriDetails.Text,
                    OthersDetails = labelOthersDetails.Text,
                    VacationLeaveCredit = labelVacationLeaveCredit.Text,
                    VacationLeaveDebit = labelVacationLeaveDebit.Text,
                    VacationLeaveBalance = labelVacationLeaveBalance.Text,
                    SickLeaveCredit = labelSickLeaveCredit.Text,
                    SickLeaveDebit = labelSickLeaveDebit.Text,
                    SickLeaveBalance = labelSickLeaveBalance.Text
                };

                // Update payroll data in Firebase using existing payroll IDs (1,2,3 format)
                await UpdateExistingPayrollData(currentEmployeeId, ExportData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Data Loading Methods
        private async Task LoadEmployeeDetails()
        {
            var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
            foreach (var e in empDetails)
            {
                if (e.Object != null)
                {
                    employeeDetails[e.Key] = e.Object;
                }
            }
        }

        private async Task LoadEmploymentInfo()
        {
            var employmentData = await firebase.Child("EmploymentInfo").OnceAsJsonAsync();
            string rawJson = employmentData?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

            var records = ParseFirebaseArrayStructure(rawJson);
            foreach (var record in records)
            {
                if (record != null && record.Count > 0)
                {
                    string employeeId = GetDictionaryValue(record, "employee_id", "");
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        employmentInfo[employeeId] = record;
                    }
                }
            }
        }

        private async Task LoadEmployeeLoans()
        {
            var loansData = await firebase.Child("EmployeeLoans").OnceAsJsonAsync();
            string rawJson = loansData?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

            var records = ParseFirebaseArrayStructure(rawJson);
            foreach (var record in records)
            {
                if (record != null && record.Count > 0)
                {
                    employeeLoans.Add(record);
                }
            }
        }

        private async Task LoadAttendanceData()
        {
            var attendanceData = await firebase.Child("Attendance").OnceAsJsonAsync();
            string rawJson = attendanceData?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

            var allRecords = new List<Dictionary<string, string>>();

            try
            {
                var jObject = JObject.Parse(rawJson);
                foreach (var property in jObject.Properties())
                {
                    if (property.Value.Type == JTokenType.Object)
                    {
                        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var prop in ((JObject)property.Value).Properties())
                        {
                            record[prop.Name] = prop.Value?.ToString() ?? "";
                        }
                        if (record.Count > 0 && IsInDateRange(record))
                        {
                            attendanceRecords.Add(record);
                        }
                    }
                }
            }
            catch
            {
                var records = ParseFirebaseArrayStructure(rawJson);
                foreach (var record in records)
                {
                    if (record != null && record.Count > 0 && IsInDateRange(record))
                    {
                        attendanceRecords.Add(record);
                    }
                }
            }
        }

        private async Task LoadLeaveCredits()
        {
            try
            {
                var leaveData = await firebase.Child("Leave Credits").OnceAsync<dynamic>();
                foreach (var leave in leaveData)
                {
                    if (leave.Object != null)
                    {
                        var leaveDict = new Dictionary<string, string>();
                        var obj = leave.Object as IDictionary<string, object>;

                        if (obj != null)
                        {
                            foreach (var prop in obj)
                            {
                                leaveDict[prop.Key] = prop.Value?.ToString() ?? "";
                            }

                            string employeeId = leaveDict.ContainsKey("employee_id") ? leaveDict["employee_id"] : "";
                            if (!string.IsNullOrEmpty(employeeId))
                            {
                                leaveCredits[employeeId] = leaveDict;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leave credits: {ex.Message}");
            }
        }
        #endregion

        #region Calculation Methods
        private (decimal totalHours, decimal totalMinutes) CalculateOvertimeForEmployee(string employeeId)
        {
            decimal totalHours = 0;
            decimal totalMinutes = 0;

            foreach (var attendance in attendanceRecords)
            {
                string attEmployeeId = GetDictionaryValue(attendance, "employeeid", "") ??
                                      GetDictionaryValue(attendance, "employee_id", "");

                if (attEmployeeId == employeeId)
                {
                    // Get overtime hours from overtime_hours field
                    if (attendance.ContainsKey("overtime_hours") && decimal.TryParse(attendance["overtime_hours"], out decimal overtime))
                    {
                        totalHours += overtime;
                    }

                    // Calculate additional overtime from overtime_in and overtime_out
                    string overtimeIn = GetDictionaryValue(attendance, "overtime_in", "");
                    string overtimeOut = GetDictionaryValue(attendance, "overtime_out", "");

                    if (!string.IsNullOrEmpty(overtimeIn) && !string.IsNullOrEmpty(overtimeOut) &&
                        overtimeIn != "N/A" && overtimeOut != "N/A" &&
                        DateTime.TryParse(overtimeIn, out DateTime otIn) && DateTime.TryParse(overtimeOut, out DateTime otOut))
                    {
                        TimeSpan otDuration = otOut - otIn;
                        totalHours += (decimal)otDuration.TotalHours;
                    }
                }
            }

            return (totalHours, totalMinutes);
        }

        private async Task<EmployeeEarnings> LoadEarningsFromPayroll(string employeeId)
        {
            var earnings = new EmployeeEarnings();

            try
            {
                // Find existing payroll record for this employee
                var payrollRecords = await firebase.Child("Payroll")
                    .OnceAsync<dynamic>();

                var employeePayroll = payrollRecords
                    .FirstOrDefault(p =>
                    {
                        var obj = p.Object as IDictionary<string, object>;
                        return obj != null && obj.ContainsKey("employee_id") &&
                               obj["employee_id"]?.ToString() == employeeId;
                    });

                if (employeePayroll != null)
                {
                    string payrollId = employeePayroll.Key;

                    // Load earnings from PayrollEarnings
                    var earningsData = await firebase.Child("PayrollEarnings")
                        .Child(payrollId)
                        .OnceSingleAsync<dynamic>();

                    if (earningsData != null)
                    {
                        var earningsObj = earningsData as IDictionary<string, object>;
                        if (earningsObj != null)
                        {
                            earnings.Commission = earningsObj.ContainsKey("commission") ? Convert.ToDecimal(earningsObj["commission"]) : 0m;
                            earnings.Communication = earningsObj.ContainsKey("communication") ? Convert.ToDecimal(earningsObj["communication"]) : 0m;
                            earnings.FoodAllowance = earningsObj.ContainsKey("food_allowance") ? Convert.ToDecimal(earningsObj["food_allowance"]) : 0m;
                            earnings.GasAllowance = earningsObj.ContainsKey("gas_allowance") ? Convert.ToDecimal(earningsObj["gas_allowance"]) : 0m;
                            earnings.Gondola = earningsObj.ContainsKey("gondola") ? Convert.ToDecimal(earningsObj["gondola"]) : 0m;
                            earnings.Incentives = earningsObj.ContainsKey("incentives") ? Convert.ToDecimal(earningsObj["incentives"]) : 0m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading earnings from payroll: {ex.Message}");
            }

            return earnings;
        }

        private async Task<EmployeeDeductions> LoadDeductionsFromPayroll(string employeeId)
        {
            var deductions = new EmployeeDeductions();

            try
            {
                // Find existing payroll record for this employee
                var payrollRecords = await firebase.Child("Payroll")
                    .OnceAsync<dynamic>();

                var employeePayroll = payrollRecords
                    .FirstOrDefault(p =>
                    {
                        var obj = p.Object as IDictionary<string, object>;
                        return obj != null && obj.ContainsKey("employee_id") &&
                               obj["employee_id"]?.ToString() == employeeId;
                    });

                if (employeePayroll != null)
                {
                    string payrollId = employeePayroll.Key;

                    // Load deductions from EmployeeDeductions
                    var deductionsData = await firebase.Child("EmployeeDeductions")
                        .Child(payrollId)
                        .OnceSingleAsync<dynamic>();

                    if (deductionsData != null)
                    {
                        var deductionsObj = deductionsData as IDictionary<string, object>;
                        if (deductionsObj != null)
                        {
                            deductions.CashAdvance = deductionsObj.ContainsKey("cash_advance") ? Convert.ToDecimal(deductionsObj["cash_advance"]) : 0m;
                            deductions.CoopContribution = deductionsObj.ContainsKey("coop_contribution") ? Convert.ToDecimal(deductionsObj["coop_contribution"]) : 0m;
                            deductions.OtherDeductions = deductionsObj.ContainsKey("other_deductions") ? Convert.ToDecimal(deductionsObj["other_deductions"]) : 0m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading deductions from payroll: {ex.Message}");
            }

            return deductions;
        }

        private (decimal sssLoan, decimal pagibigLoan, decimal carLoan, decimal housingLoan, decimal coopLoan) CalculateLoanDeductions(string employeeId)
        {
            decimal sssLoan = 0m, pagibigLoan = 0m, carLoan = 0m, housingLoan = 0m, coopLoan = 0m;

            foreach (var loan in employeeLoans)
            {
                string loanEmployeeId = GetDictionaryValue(loan, "employee_id", "");
                string loanStatus = GetDictionaryValue(loan, "status", "");
                string loanType = GetDictionaryValue(loan, "loan_type", "Unknown");

                if (loanEmployeeId == employeeId && loanStatus == "Active")
                {
                    // Use bi_monthly_amortization field directly from Firebase
                    decimal biMonthlyAmortization = ParseDecimalSafe(loan, "bi_monthly_amortization");

                    if (biMonthlyAmortization > 0)
                    {
                        if (loanType.ToLower().Contains("sss"))
                            sssLoan += biMonthlyAmortization;
                        else if (loanType.ToLower().Contains("pagibig") || loanType.ToLower().Contains("pag-ibig"))
                            pagibigLoan += biMonthlyAmortization;
                        else if (loanType.ToLower().Contains("car"))
                            carLoan += biMonthlyAmortization;
                        else if (loanType.ToLower().Contains("housing"))
                            housingLoan += biMonthlyAmortization;
                        else if (loanType.ToLower().Contains("coop"))
                            coopLoan += biMonthlyAmortization;
                    }
                }
            }

            return (sssLoan, pagibigLoan, carLoan, housingLoan, coopLoan);
        }

        private void UpdateLeaveCreditsDisplay(string employeeId)
        {
            try
            {
                if (leaveCredits.ContainsKey(employeeId))
                {
                    var leaveData = leaveCredits[employeeId];

                    // Get sick leave and vacation leave from Leave Credits
                    decimal sickLeave = ParseDecimalSafe(leaveData, "sick_leave");
                    decimal vacationLeave = ParseDecimalSafe(leaveData, "vacation_leave");

                    // Display current balances
                    labelSickLeaveBalance.Text = sickLeave.ToString("0");
                    labelVacationLeaveBalance.Text = vacationLeave.ToString("0");

                    // For this payroll period, we'll show current balances and no new credits/debits
                    labelSickLeaveCredit.Text = "0";
                    labelSickLeaveDebit.Text = "0";
                    labelVacationLeaveCredit.Text = "0";
                    labelVacationLeaveDebit.Text = "0";
                }
                else
                {
                    // Default values if no leave data found
                    labelSickLeaveBalance.Text = "0";
                    labelVacationLeaveBalance.Text = "0";
                    labelSickLeaveCredit.Text = "0";
                    labelSickLeaveDebit.Text = "0";
                    labelVacationLeaveCredit.Text = "0";
                    labelVacationLeaveDebit.Text = "0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating leave credits: {ex.Message}");
            }
        }
        #endregion

        #region Helper Methods
        private string FormatEmployeeName(dynamic emp)
        {
            string lastName = GetDynamicValue(emp, "last_name") ?? "";
            string firstName = GetDynamicValue(emp, "first_name") ?? "";
            string middleName = GetDynamicValue(emp, "middle_name") ?? "";
            return $"{lastName}, {firstName} {middleName}".Trim().Replace("  ", " ");
        }

        private string GetDynamicValue(dynamic obj, string propertyName)
        {
            try
            {
                if (obj is IDictionary<string, object> dict)
                {
                    return dict.ContainsKey(propertyName) ? dict[propertyName]?.ToString() : "";
                }
                return obj[propertyName]?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private string GetSafeValue(Dictionary<string, string> dict, string key)
        {
            return dict != null && dict.ContainsKey(key) ? dict[key] : "";
        }

        private string GetDictionaryValue(Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict != null && dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        private decimal CalculateDailyRate(Dictionary<string, string> employment)
        {
            decimal dailyRate = 0m;

            if (employment != null && employment.ContainsKey("daily_rate") && !string.IsNullOrEmpty(employment["daily_rate"]))
            {
                decimal.TryParse(employment["daily_rate"], out dailyRate);
            }

            if (dailyRate == 0m && employeeDetails.ContainsKey(currentEmployeeId))
            {
                var emp = employeeDetails[currentEmployeeId];
                string dailyRateStr = GetDynamicValue(emp, "daily_rate");
                if (!string.IsNullOrEmpty(dailyRateStr))
                {
                    decimal.TryParse(dailyRateStr, out dailyRate);
                }
            }

            return dailyRate == 0m ? MINIMUM_DAILY_RATE : dailyRate;
        }

        private decimal ValidateDeductions(decimal totalDeductions, decimal grossPay)
        {
            decimal maxAllowedDeductions = grossPay * MAX_DEDUCTION_PERCENTAGE;
            return totalDeductions > maxAllowedDeductions ? maxAllowedDeductions : totalDeductions;
        }

        private decimal ParseDecimalSafe(Dictionary<string, string> dict, string key)
        {
            if (dict != null && dict.ContainsKey(key) && decimal.TryParse(dict[key], out decimal result))
            {
                return result;
            }
            return 0m;
        }

        private int CalculateDaysWorked(string employeeId)
        {
            int daysWorked = 0;
            HashSet<string> uniqueDates = new HashSet<string>();

            foreach (var attendance in attendanceRecords)
            {
                string attEmployeeId = GetDictionaryValue(attendance, "employeeid", "") ??
                                      GetDictionaryValue(attendance, "employee_id", "");

                string attendanceDate = GetDictionaryValue(attendance, "attendancedate", "") ??
                                       GetDictionaryValue(attendance, "attendance_date", "");

                if (attEmployeeId == employeeId && !uniqueDates.Contains(attendanceDate))
                {
                    string status = GetDictionaryValue(attendance, "status", "");
                    bool isAbsent = string.Equals(status, "Absent", StringComparison.OrdinalIgnoreCase);

                    if (!isAbsent)
                    {
                        uniqueDates.Add(attendanceDate);
                        daysWorked++;
                    }
                }
            }

            return daysWorked;
        }

        private bool IsInDateRange(Dictionary<string, string> record)
        {
            string attendanceDate = GetDictionaryValue(record, "attendancedate", "") ??
                                   GetDictionaryValue(record, "attendance_date", "");

            if (DateTime.TryParse(attendanceDate, out DateTime recordDate))
            {
                return recordDate >= cutoffStartDate && recordDate <= cutoffEndDate;
            }

            return false;
        }

        private List<Dictionary<string, string>> ParseFirebaseArrayStructure(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();

            try
            {
                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return records;

                var jArray = JArray.Parse(rawJson);
                foreach (var item in jArray)
                {
                    if (item.Type == JTokenType.Null) continue;

                    if (item.Type == JTokenType.Object)
                    {
                        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var prop in item.Children<JProperty>())
                        {
                            record[prop.Name] = prop.Value?.ToString() ?? "";
                        }
                        if (record.Count > 0) records.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Firebase array parsing error: " + ex.Message);
            }

            return records;
        }

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
        #endregion

        #region Government Contributions & Tax Calculations
        private decimal CalculatePhilHealthContribution(decimal monthlySalary)
        {
            decimal employeeShareRate = 0.0275m;
            decimal contribution = monthlySalary * employeeShareRate;

            if (contribution < 200m) contribution = 200m;
            if (contribution > 400m) contribution = 400m;

            return Math.Round(contribution / 2, 2);
        }

        private decimal CalculatePagibigContribution(decimal monthlySalary)
        {
            decimal rate = monthlySalary > 1500m ? 0.02m : 0.01m;
            decimal contribution = monthlySalary * rate;
            return Math.Round(Math.Min(contribution, 100m) / 2, 2);
        }

        private decimal CalculateWithholdingTax(decimal monthlySalary)
        {
            decimal semiMonthlySalary = monthlySalary / 2;

            if (semiMonthlySalary <= 10417m) return 0;
            if (semiMonthlySalary <= 16666m) return (semiMonthlySalary - 10417m) * 0.15m;
            if (semiMonthlySalary <= 33332m) return 937.50m + (semiMonthlySalary - 16666m) * 0.20m;
            if (semiMonthlySalary <= 83332m) return 4270.50m + (semiMonthlySalary - 33332m) * 0.25m;
            if (semiMonthlySalary <= 333332m) return 16770.50m + (semiMonthlySalary - 83332m) * 0.30m;
            return 91770.50m + (semiMonthlySalary - 333332m) * 0.35m;
        }
        #endregion

        #region Firebase Update Methods
        private async Task UpdateExistingPayrollData(string employeeId, PayrollExportData data)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Find existing payroll record for this employee (using 1,2,3 format)
                var payrollRecords = await firebase.Child("Payroll")
                    .OnceAsync<dynamic>();

                var existingPayroll = payrollRecords
                    .FirstOrDefault(p =>
                    {
                        var obj = p.Object as IDictionary<string, object>;
                        return obj != null && obj.ContainsKey("employee_id") &&
                               obj["employee_id"]?.ToString() == employeeId;
                    });

                if (existingPayroll != null)
                {
                    string payrollId = existingPayroll.Key;
                    Console.WriteLine($"Updating existing payroll record: {payrollId}");

                    // Update all payroll collections with the existing payroll ID
                    var updateTasks = new List<Task>
                    {
                        // Update EmployeeDetails
                        firebase.Child("EmployeeDetails").Child(employeeId).PatchAsync(new
                        {
                            daily_rate = decimal.Parse(data.DailyRate),
                            gross_pay = decimal.Parse(data.GrossPay),
                            net_pay = decimal.Parse(data.NetPay),
                            total_deductions = decimal.Parse(data.TotalDeductions),
                            last_updated = timestamp
                        }),

                        // Update PayrollEarnings
                        firebase.Child("PayrollEarnings").Child(payrollId).PatchAsync(new
                        {
                            basic_pay = decimal.Parse(data.BasicPay),
                            overtime_pay = decimal.Parse(data.OvertimePerHour),
                            commission = decimal.Parse(data.Commission),
                            communication = decimal.Parse(data.Communication),
                            food_allowance = decimal.Parse(data.FoodAllowance),
                            gas_allowance = decimal.Parse(data.GasAllowance),
                            gondola = decimal.Parse(data.Gondola),
                            incentives = decimal.Parse(data.Incentives),
                            total_earnings = decimal.Parse(data.GrossPay),
                            last_updated = timestamp
                        }),

                        // Update PayrollSummary
                        firebase.Child("PayrollSummary").Child(payrollId).PatchAsync(new
                        {
                            gross_pay = decimal.Parse(data.GrossPay),
                            net_pay = decimal.Parse(data.NetPay),
                            total_deductions = decimal.Parse(data.TotalDeductions),
                            last_updated = timestamp
                        }),

                        // Update GovernmentDeductions
                        firebase.Child("GovernmentDeductions").Child(payrollId).PatchAsync(new
                        {
                            sss = decimal.Parse(data.SSS),
                            philhealth = decimal.Parse(data.Philhealth),
                            pagibig = decimal.Parse(data.PagIbig),
                            withholding_tax = decimal.Parse(data.WithholdingTax),
                            total_gov_deductions = decimal.Parse(data.TotalDeductions),
                            last_updated = timestamp
                        }),

                        // Update EmployeeDeductions
                        firebase.Child("EmployeeDeductions").Child(payrollId).PatchAsync(new
                        {
                            cash_advance = decimal.Parse(data.CashAdvance),
                            coop_contribution = decimal.Parse(data.CoopContribution),
                            other_deductions = decimal.Parse(data.Others),
                            last_updated = timestamp
                        }),

                        // Update Payroll
                        firebase.Child("Payroll").Child(payrollId).PatchAsync(new
                        {
                            net_pay = decimal.Parse(data.NetPay),
                            last_updated = timestamp
                        })
                    };

                    await Task.WhenAll(updateTasks);
                    Console.WriteLine($"Successfully updated payroll data for {employeeId} using record {payrollId}");
                }
                else
                {
                    Console.WriteLine($"No existing payroll record found for {employeeId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating payroll data: {ex.Message}");
            }
        }
        #endregion

        #region UI Event Handlers
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
        #endregion

        private void setFont()
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
    }

    #region Helper Classes
    public class EmployeeEarnings
    {
        public decimal Commission { get; set; }
        public decimal Communication { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal GasAllowance { get; set; }
        public decimal Gondola { get; set; }
        public decimal Incentives { get; set; }
    }

    public class EmployeeDeductions
    {
        public decimal CashAdvance { get; set; }
        public decimal CoopContribution { get; set; }
        public decimal OtherDeductions { get; set; }
    }

    public class SssContribution
    {
        public decimal MSC { get; set; }
        public decimal EmployerSS { get; set; }
        public decimal EmployerMPF { get; set; }
        public decimal EmployerEC { get; set; }
        public decimal EmployerTotal => EmployerSS + EmployerMPF + EmployerEC;
        public decimal EmployeeSS { get; set; }
        public decimal EmployeeMPF { get; set; }
        public decimal EmployeeTotal => EmployeeSS + EmployeeMPF;
        public decimal GrandTotal => EmployerTotal + EmployeeTotal;
    }

    public static class SssCalculator
    {
        public static SssContribution Compute(decimal salary)
        {
            decimal msc;
            if (salary < 5250) msc = 5000;
            else if (salary >= 34750) msc = 35000;
            else msc = Math.Ceiling((salary + 250) / 500) * 500;

            var result = new SssContribution { MSC = msc };
            result.EmployerSS = msc * 0.09m;
            result.EmployeeSS = msc * 0.045m;

            if (msc >= 20250)
            {
                result.EmployerMPF = msc * 0.01m;
                result.EmployeeMPF = msc * 0.005m;
            }

            result.EmployerEC = (msc <= 14999) ? 10 : 30;
            return result;
        }
    }
    #endregion
}