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
        private Dictionary<string, Dictionary<string, string>> payrollData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollSummaryData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> govDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> employeeDeductions = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> payrollEarnings = new Dictionary<string, Dictionary<string, string>>();
        private List<Dictionary<string, string>> attendanceRecords = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> employeeLoans = new List<Dictionary<string, string>>();

        // Store current employee ID to track selection
        private string currentEmployeeId = "";

        // Constants for validation
        private const decimal MINIMUM_DAILY_RATE = 500m;
        private const decimal MINIMUM_WORK_MINUTES = 30m;
        private const decimal MAX_DEDUCTION_PERCENTAGE = 0.4m;

        public PayrollSummary(string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();

            this.Load += async (sender, e) => await LoadAndUpdatePayrollDataAsync();
        }

        public async void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            await LoadAndUpdatePayrollDataAsync();
        }

        private async Task LoadAndUpdatePayrollDataAsync()
        {
            await LoadPayrollDataAsync();

            if (ExportData != null)
            {
                await UpdatePayrollDataAsync(currentEmployeeId, ExportData);
            }
        }

        // Simplified JSON parsing method - READ ONLY for EmployeeLoans
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

        // READ-ONLY: Load EmployeeLoans without modifying Firebase structure
        private async Task LoadEmployeeLoansDirectly()
        {
            try
            {
                employeeLoans.Clear();

                var loansData = await firebase.Child("EmployeeLoans").OnceAsJsonAsync();
                string rawJson = loansData?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

                // Use parser but DO NOT modify the original Firebase structure
                var records = ParseFirebaseArrayStructure(rawJson);

                foreach (var record in records)
                {
                    if (record != null && record.Count > 0)
                    {
                        // Create read-only copy for calculations only
                        var loanCopy = new Dictionary<string, string>(record);
                        employeeLoans.Add(loanCopy);
                    }
                }

                Console.WriteLine($"Total loans loaded (READ-ONLY): {employeeLoans.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EmployeeLoans (READ-ONLY): {ex.Message}");
            }
        }

        // READ-ONLY: Load attendance data
        private async Task LoadAttendanceDataWithMixedFormats()
        {
            try
            {
                attendanceRecords.Clear();

                var attendanceData = await firebase.Child("Attendance").OnceAsJsonAsync();
                string rawJson = attendanceData?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

                var records = ParseFirebaseArrayStructure(rawJson);

                foreach (var record in records)
                {
                    if (record != null && record.Count > 0)
                    {
                        var normalizedRecord = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var kvp in record)
                        {
                            normalizedRecord[kvp.Key.ToLower()] = kvp.Value;
                        }

                        if (IsValidAttendanceRecord(normalizedRecord))
                        {
                            attendanceRecords.Add(normalizedRecord);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendance data: {ex.Message}");
            }
        }

        // READ-ONLY: Load employment info
        private async Task LoadEmploymentInfoWithFirebaseStructure()
        {
            try
            {
                employmentInfo.Clear();

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EmploymentInfo: {ex.Message}");
            }
        }

        private bool IsValidAttendanceRecord(Dictionary<string, string> record)
        {
            string attendanceDate = GetDictionaryValue(record, "attendancedate", "") ??
                                   GetDictionaryValue(record, "attendance_date", "");

            if (attendanceDate.Contains("0001-01-01") || string.IsNullOrEmpty(attendanceDate))
                return false;

            string employeeId = GetDictionaryValue(record, "employeeid", "") ??
                               GetDictionaryValue(record, "employee_id", "");

            return !string.IsNullOrEmpty(employeeId);
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

        // Main payroll data loading method
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

                // Load other data
                await LoadEmploymentInfoWithFirebaseStructure();
                await LoadEmployeeLoansDirectly();
                await LoadAttendanceDataWithMixedFormats();

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

                // Calculate daily rate
                decimal dailyRate = CalculateDailyRate(employment);
                labelDailyRateInput.Text = dailyRate.ToString("0.00");

                // Calculate work days
                int requiredDays = 22; // Default
                labelDaysInput.Text = requiredDays.ToString();

                int daysWorked = CalculateDaysWorked(currentEmployeeId);
                labelDaysPresentInput.Text = daysWorked.ToString();

                // Calculate basic salary
                decimal totalSalary = dailyRate * requiredDays;
                labelSalaryInput.Text = totalSalary.ToString("0.00");

                decimal basicPay = dailyRate * daysWorked;
                labelBasicPayAmountBaseInput.Text = basicPay.ToString("0.00");
                labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");

                // Calculate overtime
                decimal totalOvertimeHours = CalculateOvertimeForEmployee(currentEmployeeId);
                labelOvertimeInput.Text = totalOvertimeHours.ToString("0.00");
                decimal overtimePay = CalculateOvertimePay(dailyRate, totalOvertimeHours);
                labelOvertimePerHourAmountCreditInput.Text = overtimePay.ToString("0.00");

                // Set earnings (simplified - you can load from Firebase if needed)
                decimal commission = 0m, communication = 0m, foodAllowance = 0m, gasAllowance = 0m, gondola = 0m, incentives = 0m;

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

                // Calculate loan deductions (READ-ONLY from EmployeeLoans)
                var loanDeductions = CalculateLoanDeductionsFromExistingValues(currentEmployeeId);
                labelSSSLoanAmountDebitInput.Text = loanDeductions.sssLoan.ToString("0.00");
                labelPagIbigLoanAmountDebitInput.Text = loanDeductions.pagibigLoan.ToString("0.00");
                labelCarLoanAmountDebitInput.Text = loanDeductions.carLoan.ToString("0.00");
                labelHousingLoanAmountDebitInput.Text = loanDeductions.housingLoan.ToString("0.00");
                labelCoopLoanAmountDebitInput.Text = loanDeductions.coopLoan.ToString("0.00");

                // Other deductions
                decimal cashAdvance = 0m, coopContribution = 0m, otherDeductions = 0m;
                labelCashAdvanceAmountDebitInput.Text = cashAdvance.ToString("0.00");
                labelCoopContriAmountDebitInput.Text = coopContribution.ToString("0.00");
                labelOthersAmountDebitInput.Text = otherDeductions.ToString("0.00");

                // Calculate gross pay
                decimal grossPay = basicPay + overtimePay + commission + communication + foodAllowance + gasAllowance + gondola + incentives;
                labelGrossPayInput.Text = grossPay.ToString("0.00");

                // Calculate total deductions
                decimal totalDeductions = sss + philhealth + pagibig + withholdingTax +
                                         loanDeductions.sssLoan + loanDeductions.pagibigLoan + loanDeductions.carLoan +
                                         loanDeductions.housingLoan + loanDeductions.coopLoan + cashAdvance +
                                         coopContribution + otherDeductions;

                totalDeductions = ValidateDeductions(totalDeductions, grossPay, monthlySalary);
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
                    DateCovered = "Current Period",
                    Days = requiredDays.ToString(),
                    DaysPresent = daysWorked.ToString(),
                    DailyRate = dailyRate.ToString("0.00"),
                    Salary = totalSalary.ToString("0.00"),
                    Overtime = totalOvertimeHours.ToString("0.00"),
                    BasicPay = basicPay.ToString("0.00"),
                    OvertimePerHour = overtimePay.ToString("0.00"),
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
                    SSSLoan = loanDeductions.sssLoan.ToString("0.00"),
                    PagIbigLoan = loanDeductions.pagibigLoan.ToString("0.00"),
                    CarLoan = loanDeductions.carLoan.ToString("0.00"),
                    HousingLoan = loanDeductions.housingLoan.ToString("0.00"),
                    CashAdvance = cashAdvance.ToString("0.00"),
                    CoopLoan = loanDeductions.coopLoan.ToString("0.00"),
                    CoopContribution = coopContribution.ToString("0.00"),
                    Others = otherDeductions.ToString("0.00"),
                    TotalDeductions = totalDeductions.ToString("0.00"),
                    NetPay = netPay.ToString("0.00")
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

            // Try employment info first
            if (employment != null && employment.ContainsKey("daily_rate") && !string.IsNullOrEmpty(employment["daily_rate"]))
            {
                decimal.TryParse(employment["daily_rate"], out dailyRate);
            }

            // Fallback: Check employee details
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

        private decimal ValidateDeductions(decimal totalDeductions, decimal grossPay, decimal monthlySalary)
        {
            decimal maxAllowedDeductions = grossPay * MAX_DEDUCTION_PERCENTAGE;
            return totalDeductions > maxAllowedDeductions ? maxAllowedDeductions : totalDeductions;
        }

        // READ-ONLY: Calculate loan deductions from existing Firebase values
        private (decimal sssLoan, decimal pagibigLoan, decimal carLoan, decimal housingLoan, decimal coopLoan) CalculateLoanDeductionsFromExistingValues(string employeeId)
        {
            decimal sssLoan = 0m, pagibigLoan = 0m, carLoan = 0m, housingLoan = 0m, coopLoan = 0m;

            foreach (var loan in employeeLoans)
            {
                string loanEmployeeId = GetDictionaryValue(loan, "employee_id", "");
                string loanStatus = GetDictionaryValue(loan, "status", "");
                string loanType = GetDictionaryValue(loan, "loan_type", "Unknown");

                if (loanEmployeeId == employeeId && loanStatus == "Active")
                {
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

        private decimal ParseDecimalSafe(Dictionary<string, string> dict, string key)
        {
            if (dict != null && dict.ContainsKey(key) && decimal.TryParse(dict[key], out decimal result))
            {
                return result;
            }
            return 0m;
        }

        private decimal CalculateOvertimeForEmployee(string employeeId)
        {
            decimal totalOvertime = 0;

            foreach (var attendance in attendanceRecords)
            {
                string attEmployeeId = GetDictionaryValue(attendance, "employeeid", "") ??
                                      GetDictionaryValue(attendance, "employee_id", "");

                if (attEmployeeId == employeeId)
                {
                    if (attendance.ContainsKey("overtime_hours") && decimal.TryParse(attendance["overtime_hours"], out decimal overtime))
                    {
                        if (overtime >= 0 && overtime <= 12)
                        {
                            totalOvertime += overtime;
                        }
                    }
                }
            }

            return totalOvertime;
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
        #endregion

        // UPDATED: Firebase update - NO EmployeeLoans modification
        private async Task UpdatePayrollDataAsync(string employeeId, PayrollExportData computedData)
        {
            try
            {
                string payrollId = $"payroll_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}";
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Update only payroll-related nodes - NO EmployeeLoans
                await firebase.Child("EmployeeDetails").Child(employeeId).PatchAsync(new
                {
                    daily_rate = decimal.Parse(computedData.DailyRate),
                    gross_pay = decimal.Parse(computedData.GrossPay),
                    net_pay = decimal.Parse(computedData.NetPay),
                    total_deductions = decimal.Parse(computedData.TotalDeductions),
                    salary_frequency = "Semi-Monthly",
                    last_updated = timestamp
                });

                await firebase.Child("PayrollEarnings").Child(payrollId).PatchAsync(new
                {
                    basic_pay = decimal.Parse(computedData.BasicPay),
                    overtime_pay = decimal.Parse(computedData.OvertimePerHour),
                    total_earnings = decimal.Parse(computedData.GrossPay),
                    daily_rate = decimal.Parse(computedData.DailyRate),
                    days_present = int.Parse(computedData.DaysPresent),
                    last_updated = timestamp
                });

                await firebase.Child("PayrollSummary").Child(payrollId).PatchAsync(new
                {
                    gross_pay = decimal.Parse(computedData.GrossPay),
                    net_pay = decimal.Parse(computedData.NetPay),
                    total_deductions = decimal.Parse(computedData.TotalDeductions),
                    last_updated = timestamp
                });

                await firebase.Child("GovernmentDeductions").Child(payrollId).PatchAsync(new
                {
                    sss = decimal.Parse(computedData.SSS),
                    philhealth = decimal.Parse(computedData.Philhealth),
                    pagibig = decimal.Parse(computedData.PagIbig),
                    withholding_tax = decimal.Parse(computedData.WithholdingTax),
                    total_gov_deductions = decimal.Parse(computedData.TotalDeductions),
                    last_updated = timestamp
                });

                await firebase.Child("EmployeeDeductions").Child(payrollId).PatchAsync(new
                {
                    cash_advance = decimal.Parse(computedData.CashAdvance),
                    coop_contribution = decimal.Parse(computedData.CoopContribution),
                    other_deductions = decimal.Parse(computedData.Others),
                    last_updated = timestamp
                });

                await firebase.Child("Payroll").Child(payrollId).PatchAsync(new
                {
                    employee_id = employeeId,
                    net_pay = decimal.Parse(computedData.NetPay),
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    last_updated = timestamp
                });

                Console.WriteLine($"Payroll data saved (EmployeeLoans UNTOUCHED) at {timestamp}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Rest of your UI event handlers remain the same
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

    // SSS Computation Classes (keep as is)
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
}