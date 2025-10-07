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

        // Constants for validation
        private const decimal MINIMUM_DAILY_RATE = 500m;
        private const decimal MINIMUM_WORK_MINUTES = 30m; // Minimum 30 minutes to count as a work day
        private const decimal MAX_DEDUCTION_PERCENTAGE = 0.4m; // Max 40% of gross pay for deductions

        public PayrollSummary(string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();

            // Load data when form loads and auto-save to Firebase
            this.Load += async (sender, e) => await LoadAndUpdatePayrollDataAsync();
        }

        public async void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            await LoadAndUpdatePayrollDataAsync();
        }

        // Combined method to load data, compute, and auto-update Firebase
        private async Task LoadAndUpdatePayrollDataAsync()
        {
            await LoadPayrollDataAsync();

            // Auto-save to Firebase after computation
            if (ExportData != null)
            {
                await UpdatePayrollDataAsync(currentEmployeeId, ExportData);
            }
        }

        // NEW: Improved JSON parsing method specifically for [null, {...}] structure
        private List<Dictionary<string, string>> ParseFirebaseArrayStructure(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();

            try
            {
                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return records;

                // Parse as JArray to handle the [null, {...}, {...}] structure
                var jArray = JArray.Parse(rawJson);

                foreach (var item in jArray)
                {
                    if (item.Type == JTokenType.Null) continue; // Skip null elements

                    if (item.Type == JTokenType.Object)
                    {
                        // Direct object - convert to dictionary
                        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var prop in item.Children<JProperty>())
                        {
                            record[prop.Name] = prop.Value?.ToString() ?? "";
                        }
                        if (record.Count > 0) records.Add(record);
                    }
                    else if (item.Type == JTokenType.Array)
                    {
                        // Handle nested arrays [['key':'value'], ...] structure
                        foreach (var nestedItem in item)
                        {
                            if (nestedItem.Type == JTokenType.Object)
                            {
                                var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                foreach (var prop in nestedItem.Children<JProperty>())
                                {
                                    record[prop.Name] = prop.Value?.ToString() ?? "";
                                }
                                if (record.Count > 0) records.Add(record);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Firebase array parsing error: " + ex.Message);
                // Fallback to original parsing method
                return ParseMalformedJson(rawJson);
            }

            return records;
        }

        // UPDATED: Enhanced JSON parsing method with better array handling
        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();

            try
            {
                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return records;

                // Handle array with null first element - FIXED
                if (rawJson.StartsWith("[null,"))
                {
                    rawJson = rawJson.Replace("[null,", "[");
                }
                if (rawJson.StartsWith("[null]"))
                {
                    return records;
                }

                // Try to parse as JArray first
                try
                {
                    var jArray = JArray.Parse(rawJson);
                    foreach (var item in jArray)
                    {
                        if (item.Type == JTokenType.Null) continue; // Skip null elements

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
                    return records;
                }
                catch
                {
                    // If JArray fails, try JObject
                    try
                    {
                        var jObject = JObject.Parse(rawJson);
                        foreach (var property in jObject.Properties())
                        {
                            if (property.Value.Type == JTokenType.Object)
                            {
                                var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                foreach (var prop in property.Value.Children<JProperty>())
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
                        // Continue with regex parsing if both fail
                    }
                }

                // Fallback: regex parsing for malformed JSON
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

                        // Improved regex to handle key-value pairs
                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(""[^""]*""|[^,}]+)");
                        foreach (Match kvpMatch in kvpMatches)
                        {
                            if (kvpMatch.Groups.Count >= 3)
                            {
                                string key = kvpMatch.Groups[1].Value;
                                string value = kvpMatch.Groups[2].Value.Trim();

                                // Remove quotes from value if present
                                if (value.StartsWith("\"") && value.EndsWith("\""))
                                    value = value.Substring(1, value.Length - 2);

                                // Handle null values
                                if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
                                    value = "";

                                record[key] = value;
                            }
                        }
                        if (record.Count > 0) records.Add(record);
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("JSON parsing error: " + ex.Message);
            }

            return records;
        }

        // UPDATED: Improved array data loading with Firebase structure handling
        private async Task LoadArrayBasedData(string childPath, Action<Dictionary<string, string>> processItem)
        {
            try
            {
                var jsonResponse = await firebase.Child(childPath).OnceAsJsonAsync();
                string rawJson = jsonResponse?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

                // Use the new Firebase array structure parser
                var records = ParseFirebaseArrayStructure(rawJson);

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

        // UPDATED: Direct Firebase query for EmployeeLoans to handle the [null, {...}] structure
        private async Task LoadEmployeeLoansDirectly()
        {
            try
            {
                employeeLoans.Clear();

                var loansData = await firebase.Child("EmployeeLoans").OnceAsJsonAsync();
                string rawJson = loansData?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

                // Use the new parser for consistent handling
                var records = ParseFirebaseArrayStructure(rawJson);

                foreach (var record in records)
                {
                    if (record != null && record.Count > 0)
                    {
                        employeeLoans.Add(record);
                        Console.WriteLine($"Loaded loan for employee: {GetDictionaryValue(record, "employee_id", "Unknown")}");
                    }
                }

                Console.WriteLine($"Total loans loaded: {employeeLoans.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EmployeeLoans directly: {ex.Message}");
            }
        }

        // UPDATED: Improved attendance data loading with validation and filtering
        private async Task LoadAttendanceDataWithMixedFormats()
        {
            try
            {
                attendanceRecords.Clear();

                var attendanceData = await firebase.Child("Attendance").OnceAsJsonAsync();
                string rawJson = attendanceData?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

                // Use the new parser for consistent handling
                var records = ParseFirebaseArrayStructure(rawJson);

                foreach (var record in records)
                {
                    if (record != null && record.Count > 0)
                    {
                        // Normalize field names for consistent access
                        var normalizedRecord = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        foreach (var kvp in record)
                        {
                            string normalizedKey = kvp.Key.ToLower();
                            normalizedRecord[normalizedKey] = kvp.Value;
                        }

                        // FIXED: Validate and filter attendance records
                        if (IsValidAttendanceRecord(normalizedRecord))
                        {
                            attendanceRecords.Add(normalizedRecord);
                        }
                    }
                }

                Console.WriteLine($"Total valid attendance records loaded: {attendanceRecords.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendance data: {ex.Message}");
            }
        }

        // NEW: Special method to load EmploymentInfo with proper Firebase structure handling
        private async Task LoadEmploymentInfoWithFirebaseStructure()
        {
            try
            {
                employmentInfo.Clear();

                var employmentData = await firebase.Child("EmploymentInfo").OnceAsJsonAsync();
                string rawJson = employmentData?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "null") return;

                Console.WriteLine($"Raw EmploymentInfo JSON: {rawJson}");

                // Use the new parser for Firebase array structure
                var records = ParseFirebaseArrayStructure(rawJson);

                foreach (var record in records)
                {
                    if (record != null && record.Count > 0)
                    {
                        string employeeId = GetDictionaryValue(record, "employee_id", "");

                        if (!string.IsNullOrEmpty(employeeId))
                        {
                            employmentInfo[employeeId] = record;
                            Console.WriteLine($"Successfully loaded employment info for: {employeeId}");
                        }
                        else
                        {
                            Console.WriteLine("Warning: Employment record without employee_id found");
                        }
                    }
                }

                Console.WriteLine($"Total employment records loaded: {employmentInfo.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EmploymentInfo: {ex.Message}");
            }
        }

        // FIXED: Validate attendance records to filter out invalid data
        private bool IsValidAttendanceRecord(Dictionary<string, string> record)
        {
            // Check for invalid default dates
            string attendanceDate = GetDictionaryValue(record, "attendancedate", "") ??
                                   GetDictionaryValue(record, "attendance_date", "");

            if (attendanceDate.Contains("0001-01-01") || string.IsNullOrEmpty(attendanceDate))
                return false;

            // Check for unrealistic work durations
            string hoursWorkedStr = GetDictionaryValue(record, "hoursworked", "") ??
                                   GetDictionaryValue(record, "hours_worked", "0");

            if (decimal.TryParse(hoursWorkedStr, out decimal hoursWorked))
            {
                // Filter out records with less than 30 minutes of work
                if (hoursWorked < 0.5m) // 0.5 hours = 30 minutes
                    return false;
            }

            // Check if record has valid employee ID
            string employeeId = GetDictionaryValue(record, "employeeid", "") ??
                               GetDictionaryValue(record, "employee_id", "");

            return !string.IsNullOrEmpty(employeeId);
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

        // UPDATED: Enhanced payroll data loading with comprehensive validation and Firebase structure handling
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

                // Load EmployeeDetails with null checking
                var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                foreach (var e in empDetails)
                {
                    if (e.Object != null)
                    {
                        employeeDetails[e.Key] = e.Object;
                    }
                }

                // UPDATED: Use the new method to load EmploymentInfo with Firebase structure handling
                await LoadEmploymentInfoWithFirebaseStructure();

                // Load other data using array-based method with Firebase structure handling
                await LoadArrayBasedData("Payroll", (item) => {
                    if (item.ContainsKey("employee_id"))
                    {
                        payrollData[item["employee_id"]] = item;
                    }
                });

                await LoadArrayBasedData("PayrollSummary", (item) => {
                    if (item.ContainsKey("payroll_id"))
                    {
                        payrollSummaryData[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("EmployeeDeductions", (item) => {
                    if (item.ContainsKey("payroll_id"))
                    {
                        employeeDeductions[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("GovernmentDeductions", (item) => {
                    if (item.ContainsKey("payroll_id"))
                    {
                        govDeductions[item["payroll_id"]] = item;
                    }
                });

                await LoadArrayBasedData("PayrollEarnings", (item) => {
                    if (item.ContainsKey("payroll_id"))
                    {
                        payrollEarnings[item["payroll_id"]] = item;
                    }
                });

                // UPDATED: Use direct loading for EmployeeLoans and Attendance with Firebase structure handling
                await LoadEmployeeLoansDirectly();
                await LoadAttendanceDataWithMixedFormats();

                // FIXED: Enhanced employee validation
                if (string.IsNullOrEmpty(currentEmployeeId))
                {
                    MessageBox.Show("No employee selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!employeeDetails.ContainsKey(currentEmployeeId))
                {
                    MessageBox.Show($"Employee details not found for {currentEmployeeId}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate employee data completeness
                if (!ValidateEmployeeData(currentEmployeeId))
                {
                    MessageBox.Show($"Incomplete employee data for {currentEmployeeId}. Please check employee records.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get employee and employment data with null safety
                var emp = employeeDetails[currentEmployeeId];
                Dictionary<string, string> employment = employmentInfo.ContainsKey(currentEmployeeId)
                    ? employmentInfo[currentEmployeeId]
                    : new Dictionary<string, string>();

                // Find payroll data for this employee
                Dictionary<string, string> payroll = FindPayrollData(currentEmployeeId);

                // Set basic employee information with safe property access
                labelIDInput.Text = currentEmployeeId;
                labelNameInput.Text = FormatEmployeeName(emp);

                // FIXED: Department and position lookup with fallback
                labelDepartmentInput.Text = GetDepartmentWithFallback(employment, currentEmployeeId);
                labelPositionInput.Text = GetPositionWithFallback(employment, currentEmployeeId);

                // Handle payroll period with improved date handling
                string cutoffStart = "";
                string cutoffEnd = "";
                if (payroll != null)
                {
                    cutoffStart = GetSafeValue(payroll, "cutoff_start");
                    cutoffEnd = GetSafeValue(payroll, "cutoff_end");
                }

                if (string.IsNullOrEmpty(cutoffStart) && !string.IsNullOrEmpty(payrollPeriod))
                {
                    cutoffStart = payrollPeriod;
                    cutoffEnd = payrollPeriod;
                }

                labelDateCoveredInput.Text = FormatDateCovered(cutoffStart, cutoffEnd);

                // Calculate daily rate with fallback mechanisms
                decimal dailyRate = CalculateDailyRate(employment, payroll);
                labelDailyRateInput.Text = dailyRate.ToString("0.00");

                // Calculate required work days
                int requiredDays = CalculateRequiredWorkDays(cutoffStart, cutoffEnd);
                labelDaysInput.Text = requiredDays.ToString();

                // FIXED: Calculate actual days worked with improved attendance parsing
                int daysWorked = CalculateDaysWorked(currentEmployeeId, cutoffStart, cutoffEnd);
                labelDaysPresentInput.Text = daysWorked.ToString();

                // Calculate basic salary components
                decimal totalSalary = dailyRate * requiredDays;
                labelSalaryInput.Text = totalSalary.ToString("0.00");

                // FIXED: Basic pay should be based on actual days worked, not required days
                decimal basicPay = dailyRate * daysWorked;
                labelBasicPayAmountBaseInput.Text = basicPay.ToString("0.00");
                labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");

                // Calculate overtime - FIXED: Use actual overtime data from attendance
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
                string payrollId = payroll != null && payroll.ContainsKey("payroll_id") ? payroll["payroll_id"] : "";
                Dictionary<string, string> earnings = !string.IsNullOrEmpty(payrollId) && payrollEarnings.ContainsKey(payrollId)
                    ? payrollEarnings[payrollId]
                    : null;

                // Parse earnings with safe parsing and validation
                decimal commission = ValidateEarning(ParseDecimalSafe(earnings, "commission"), "Commission");
                decimal communication = ValidateEarning(ParseDecimalSafe(earnings, "communication"), "Communication");
                decimal foodAllowance = ValidateEarning(ParseDecimalSafe(earnings, "food_allowance"), "Food Allowance");
                decimal gasAllowance = ValidateEarning(ParseDecimalSafe(earnings, "gas_allowance"), "Gas Allowance");
                decimal gondola = ValidateEarning(ParseDecimalSafe(earnings, "gondola"), "Gondola");
                decimal incentives = ValidateEarning(ParseDecimalSafe(earnings, "incentives"), "Incentives");

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

                // Use the new SSS computation
                var sssContribution = SssCalculator.Compute(monthlySalary);
                decimal sss = sssContribution.EmployeeTotal / 2; // Half for bi-monthly
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
                    cashAdvance = ParseDecimalSafe(deductions, "cash_advance");
                    coopContribution = ParseDecimalSafe(deductions, "coop_contribution");
                    otherDeductions = ParseDecimalSafe(deductions, "other_deductions");
                }

                // FIXED: Calculate loan deductions using bi_monthly_amortization values from Firebase with validation
                var loanDeductions = CalculateLoanDeductionsFromExistingValues(currentEmployeeId);
                labelSSSLoanAmountDebitInput.Text = loanDeductions.sssLoan.ToString("0.00");
                labelPagIbigLoanAmountDebitInput.Text = loanDeductions.pagibigLoan.ToString("0.00");
                labelCarLoanAmountDebitInput.Text = loanDeductions.carLoan.ToString("0.00");
                labelHousingLoanAmountDebitInput.Text = loanDeductions.housingLoan.ToString("0.00");
                labelCoopLoanAmountDebitInput.Text = loanDeductions.coopLoan.ToString("0.00");
                labelCashAdvanceAmountDebitInput.Text = cashAdvance.ToString("0.00");
                labelCoopContriAmountDebitInput.Text = coopContribution.ToString("0.00");
                labelOthersAmountDebitInput.Text = otherDeductions.ToString("0.00");

                // Calculate gross pay - FIXED: Include all earnings components
                decimal grossPay = basicPay + overtimePay + commission + communication + foodAllowance + gasAllowance + gondola + incentives;
                labelGrossPayInput.Text = grossPay.ToString("0.00");

                // Calculate total deductions with validation
                decimal totalDeductions = sss + philhealth + pagibig + withholdingTax +
                                         loanDeductions.sssLoan + loanDeductions.pagibigLoan + loanDeductions.carLoan +
                                         loanDeductions.housingLoan + loanDeductions.coopLoan + cashAdvance +
                                         coopContribution + otherDeductions;

                // FIXED: Validate deductions don't exceed legal limits
                totalDeductions = ValidateDeductions(totalDeductions, grossPay, monthlySalary);

                labelDeductionsInput.Text = totalDeductions.ToString("0.00");

                // Calculate net pay - FIXED: Ensure positive net pay with proper validation
                decimal netPay = grossPay - totalDeductions;

                // Ensure net pay is not negative (minimum zero) but log the issue
                if (netPay < 0)
                {
                    Console.WriteLine($"WARNING: Negative net pay for {currentEmployeeId}. Gross: {grossPay}, Deductions: {totalDeductions}");
                    netPay = 0;
                }

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
                    SSSLoan = loanDeductions.sssLoan.ToString("0.00"),
                    PagIbigLoan = loanDeductions.pagibigLoan.ToString("0.00"),
                    CarLoan = loanDeductions.carLoan.ToString("0.00"),
                    HousingLoan = loanDeductions.housingLoan.ToString("0.00"),
                    CashAdvance = cashAdvance.ToString("0.00"),
                    CoopLoan = loanDeductions.coopLoan.ToString("0.00"),
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

        #region Helper Methods

        // FIXED: Enhanced validation methods
        private bool ValidateEmployeeData(string employeeId)
        {
            if (!employeeDetails.ContainsKey(employeeId))
                return false;

            var emp = employeeDetails[employeeId];

            // Check for essential fields
            string firstName = GetDynamicValue(emp, "first_name");
            string lastName = GetDynamicValue(emp, "last_name");

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                Console.WriteLine($"Missing name fields for employee {employeeId}");
                return false;
            }

            return true;
        }

        private decimal ValidateEarning(decimal earning, string earningType)
        {
            // Validate that earnings are reasonable
            if (earning < 0)
            {
                Console.WriteLine($"WARNING: Negative {earningType} value: {earning}. Setting to 0.");
                return 0;
            }

            // Set reasonable maximum limits for each earning type
            decimal maxLimit = 10000m; // Default maximum

            // Use if-else instead of switch expression for C# 7.3 compatibility
            if (earningType == "Commission")
                maxLimit = 10000m;
            else if (earningType == "Incentives")
                maxLimit = 5000m;
            else if (earningType == "Food Allowance")
                maxLimit = 2000m;
            else if (earningType == "Communication")
                maxLimit = 2000m;
            else if (earningType == "Gas Allowance")
                maxLimit = 3000m;
            else if (earningType == "Gondola")
                maxLimit = 2000m;

            if (earning > maxLimit)
            {
                Console.WriteLine($"WARNING: {earningType} value {earning} exceeds reasonable limit {maxLimit}. Capping at limit.");
                return maxLimit;
            }

            return earning;
        }

        private decimal ValidateDeductions(decimal totalDeductions, decimal grossPay, decimal monthlySalary)
        {
            // FIXED: Ensure deductions don't exceed legal limits
            decimal maxAllowedDeductions = grossPay * MAX_DEDUCTION_PERCENTAGE;

            if (totalDeductions > maxAllowedDeductions)
            {
                Console.WriteLine($"WARNING: Total deductions {totalDeductions} exceed {MAX_DEDUCTION_PERCENTAGE * 100}% of gross pay {grossPay}. Capping at {maxAllowedDeductions}.");
                return maxAllowedDeductions;
            }

            return totalDeductions;
        }

        private Dictionary<string, string> FindPayrollData(string employeeId)
        {
            // Try direct lookup first
            if (payrollData.ContainsKey(employeeId))
            {
                return payrollData[employeeId];
            }

            // Search through all payroll records
            foreach (var payroll in payrollData.Values)
            {
                if (payroll.ContainsKey("employee_id") && payroll["employee_id"] == employeeId)
                {
                    return payroll;
                }
            }

            return null;
        }

        private string FormatEmployeeName(dynamic emp)
        {
            string lastName = GetDynamicValue(emp, "last_name") ?? "";
            string firstName = GetDynamicValue(emp, "first_name") ?? "";
            string middleName = GetDynamicValue(emp, "middle_name") ?? "";

            return $"{lastName}, {firstName} {middleName}".Trim().Replace("  ", " ");
        }

        // FIXED: Improved department lookup with fallback
        private string GetDepartmentWithFallback(Dictionary<string, string> employment, string employeeId)
        {
            string department = GetSafeValue(employment, "department");

            if (string.IsNullOrEmpty(department))
            {
                // Search through all employment records for this employee
                foreach (var empInfo in employmentInfo.Values)
                {
                    if (empInfo.ContainsKey("employee_id") && empInfo["employee_id"] == employeeId)
                    {
                        department = GetSafeValue(empInfo, "department");
                        if (!string.IsNullOrEmpty(department)) break;
                    }
                }
            }

            return string.IsNullOrEmpty(department) ? "Not Specified" : department;
        }

        // FIXED: Improved position lookup with fallback
        private string GetPositionWithFallback(Dictionary<string, string> employment, string employeeId)
        {
            string position = GetSafeValue(employment, "position");

            if (string.IsNullOrEmpty(position))
            {
                // Search through all employment records for this employee
                foreach (var empInfo in employmentInfo.Values)
                {
                    if (empInfo.ContainsKey("employee_id") && empInfo["employee_id"] == employeeId)
                    {
                        position = GetSafeValue(empInfo, "position");
                        if (!string.IsNullOrEmpty(position)) break;
                    }
                }
            }

            return string.IsNullOrEmpty(position) ? "Not Specified" : position;
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

        // Helper method for dictionary safe access (replaces DictionaryExtensions)
        private string GetDictionaryValue(Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict != null && dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        private string FormatDateCovered(string start, string end)
        {
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end)) return "Not specified";
            if (start == end) return start;
            return $"{start} to {end}";
        }

        private decimal CalculateDailyRate(Dictionary<string, string> employment, Dictionary<string, string> payroll)
        {
            decimal dailyRate = 0m;

            // Try employment info first
            if (employment != null && employment.ContainsKey("daily_rate") && !string.IsNullOrEmpty(employment["daily_rate"]))
            {
                decimal.TryParse(employment["daily_rate"], out dailyRate);
            }

            // Fallback: Check employee details for daily rate
            if (dailyRate == 0m && employeeDetails.ContainsKey(currentEmployeeId))
            {
                var emp = employeeDetails[currentEmployeeId];
                string dailyRateStr = GetDynamicValue(emp, "daily_rate");
                if (!string.IsNullOrEmpty(dailyRateStr))
                {
                    decimal.TryParse(dailyRateStr, out dailyRate);
                }
            }

            // Fallback: Check payroll earnings for daily rate
            string payrollId = payroll != null && payroll.ContainsKey("payroll_id") ? payroll["payroll_id"] : "";
            if (dailyRate == 0m && !string.IsNullOrEmpty(payrollId) && payrollEarnings.ContainsKey(payrollId) && payrollEarnings[payrollId].ContainsKey("daily_rate"))
            {
                decimal.TryParse(payrollEarnings[payrollId]["daily_rate"], out dailyRate);
            }

            // Default minimum rate if still zero
            if (dailyRate == 0m)
            {
                dailyRate = MINIMUM_DAILY_RATE;
            }

            return dailyRate;
        }

        private int CalculateRequiredWorkDays(string cutoffStart, string cutoffEnd)
        {
            int requiredDays = 0;

            if (!string.IsNullOrEmpty(cutoffStart) && !string.IsNullOrEmpty(cutoffEnd) &&
                DateTime.TryParse(cutoffStart, out DateTime startDate) && DateTime.TryParse(cutoffEnd, out DateTime endDate))
            {
                var (fullDays, halfDays) = CalculateWorkDaysInPeriod(startDate, endDate);
                requiredDays = fullDays + halfDays;
            }
            else
            {
                // Default to 22 days if no cutoff dates provided
                requiredDays = 22;
            }

            return requiredDays;
        }

        private decimal ParseDecimalSafe(Dictionary<string, string> dict, string key)
        {
            if (dict != null && dict.ContainsKey(key) && decimal.TryParse(dict[key], out decimal result))
            {
                return result;
            }
            return 0m;
        }

        // FIXED: Directly use bi_monthly_amortization values from Firebase for loan deductions with validation
        private (decimal sssLoan, decimal pagibigLoan, decimal carLoan, decimal housingLoan, decimal coopLoan) CalculateLoanDeductionsFromExistingValues(string employeeId)
        {
            decimal sssLoan = 0m, pagibigLoan = 0m, carLoan = 0m, housingLoan = 0m, coopLoan = 0m;

            Console.WriteLine($"=== CALCULATING LOANS FOR {employeeId} ===");
            Console.WriteLine($"Total loans available: {employeeLoans.Count}");

            foreach (var loan in employeeLoans)
            {
                string loanEmployeeId = GetDictionaryValue(loan, "employee_id", "");
                string loanStatus = GetDictionaryValue(loan, "status", "");
                string loanType = GetDictionaryValue(loan, "loan_type", "Unknown");

                if (loanEmployeeId == employeeId && loanStatus == "Active")
                {
                    // DIRECTLY USE bi_monthly_amortization from Firebase
                    decimal biMonthlyAmortization = 0m;
                    if (loan.ContainsKey("bi_monthly_amortization"))
                    {
                        biMonthlyAmortization = ParseDecimalSafe(loan, "bi_monthly_amortization");

                        // FIXED: Validate loan amortization amount
                        if (biMonthlyAmortization > 0)
                        {
                            Console.WriteLine($"Active Loan: {loanType}, Bi-Monthly Amort: {biMonthlyAmortization}");

                            // Use if-else instead of switch for C# 7.3 compatibility
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
                        else
                        {
                            Console.WriteLine($"Active Loan: {loanType}, ZERO BI-MONTHLY AMORTIZATION - Skipping");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Active Loan: {loanType}, NO BI-MONTHLY AMORTIZATION FIELD FOUND");
                    }
                }
            }

            Console.WriteLine($"TOTALS - SSS: {sssLoan}, Pag-IBIG: {pagibigLoan}, Car: {carLoan}, Housing: {housingLoan}, Coop: {coopLoan}");
            Console.WriteLine("=============================");

            return (sssLoan, pagibigLoan, carLoan, housingLoan, coopLoan);
        }

        #endregion

        // FIXED: Improved overtime calculation with validation
        private decimal CalculateOvertimeForEmployee(string employeeId, string cutoffStart, string cutoffEnd)
        {
            decimal totalOvertime = 0;

            foreach (var attendance in attendanceRecords)
            {
                string attEmployeeId = GetDictionaryValue(attendance, "employeeid", "") ??
                                      GetDictionaryValue(attendance, "employee_id", "");

                string attendanceDate = GetDictionaryValue(attendance, "attendancedate", "") ??
                                       GetDictionaryValue(attendance, "attendance_date", "");

                if (attEmployeeId == employeeId && IsDateInRange(attendanceDate, cutoffStart, cutoffEnd))
                {
                    // Check overtime_hours field with validation
                    if (attendance.ContainsKey("overtime_hours") && decimal.TryParse(attendance["overtime_hours"], out decimal overtime))
                    {
                        // FIXED: Validate overtime hours are reasonable
                        if (overtime >= 0 && overtime <= 12) // Max 12 hours overtime per day
                        {
                            totalOvertime += overtime;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid overtime hours {overtime} for {employeeId} on {attendanceDate}");
                        }
                    }
                }
            }

            Console.WriteLine($"Total validated overtime for {employeeId}: {totalOvertime} hours");
            return totalOvertime;
        }

        // FIXED: Improved days worked calculation with realistic validation
        private int CalculateDaysWorked(string employeeId, string cutoffStart, string cutoffEnd)
        {
            int daysWorked = 0;
            HashSet<string> uniqueDates = new HashSet<string>();

            foreach (var attendance in attendanceRecords)
            {
                string attEmployeeId = GetDictionaryValue(attendance, "employeeid", "") ??
                                      GetDictionaryValue(attendance, "employee_id", "");

                string attendanceDate = GetDictionaryValue(attendance, "attendancedate", "") ??
                                       GetDictionaryValue(attendance, "attendance_date", "");

                if (attEmployeeId == employeeId && IsDateInRange(attendanceDate, cutoffStart, cutoffEnd))
                {
                    // Check if employee actually worked (has time in/out and not absent)
                    string timeIn = GetDictionaryValue(attendance, "timein", "") ??
                                   GetDictionaryValue(attendance, "time_in", "");
                    string timeOut = GetDictionaryValue(attendance, "timeout", "") ??
                                    GetDictionaryValue(attendance, "time_out", "");
                    string status = GetDictionaryValue(attendance, "status", "");
                    string hoursWorkedStr = GetDictionaryValue(attendance, "hoursworked", "") ??
                                           GetDictionaryValue(attendance, "hours_worked", "0");

                    bool hasTimeIn = !string.IsNullOrEmpty(timeIn) && !string.Equals(timeIn, "N/A", StringComparison.OrdinalIgnoreCase);
                    bool hasTimeOut = !string.IsNullOrEmpty(timeOut) && !string.Equals(timeOut, "N/A", StringComparison.OrdinalIgnoreCase);
                    bool isAbsent = string.Equals(status, "Absent", StringComparison.OrdinalIgnoreCase);

                    // FIXED: Validate minimum work duration
                    bool hasMinimumWork = decimal.TryParse(hoursWorkedStr, out decimal hoursWorked) && hoursWorked >= 0.5m; // At least 30 minutes

                    if ((hasTimeIn || hasTimeOut) && !isAbsent && hasMinimumWork && !uniqueDates.Contains(attendanceDate))
                    {
                        uniqueDates.Add(attendanceDate);
                        daysWorked++;
                        Console.WriteLine($"Valid work day: {attendanceDate} - Hours: {hoursWorked}");
                    }
                }
            }

            Console.WriteLine($"Validated days worked for {employeeId}: {daysWorked}");
            return daysWorked;
        }

        private bool IsDateInRange(string date, string startDate, string endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                    return false;

                // Handle different date formats
                DateTime checkDate = DateTime.Parse(date.Split('T')[0]); // Handle ISO format
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);

                return checkDate >= start && checkDate <= end;
            }
            catch
            {
                return false;
            }
        }

        // Get or create payroll ID for employee
        private string GetPayrollId(string employeeId)
        {
            // Try to find existing payroll ID
            foreach (var payroll in payrollData.Values)
            {
                if (payroll.ContainsKey("employee_id") && payroll["employee_id"] == employeeId)
                {
                    return payroll.ContainsKey("payroll_id") ? payroll["payroll_id"] : "";
                }
            }

            // Create new payroll ID if none exists
            return $"payroll_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}";
        }

        // FIXED: Enhanced Firebase update with data validation and audit trail
        private async Task UpdatePayrollDataAsync(string employeeId, PayrollExportData computedData)
        {
            try
            {
                string payrollId = GetPayrollId(employeeId);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // FIXED: Validate data before writing to Firebase
                if (!ValidatePayrollData(computedData))
                {
                    MessageBox.Show("Payroll data validation failed. Please check the calculations.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update EmployeeDetails with computed values and audit trail
                await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .PatchAsync(new
                    {
                        daily_rate = decimal.Parse(computedData.DailyRate),
                        gross_pay = decimal.Parse(computedData.GrossPay),
                        net_pay = decimal.Parse(computedData.NetPay),
                        total_deductions = decimal.Parse(computedData.TotalDeductions),
                        salary_frequency = "Semi-Monthly",
                        last_updated = timestamp
                    });

                // Update PayrollEarnings
                await firebase
                    .Child("PayrollEarnings")
                    .Child(payrollId)
                    .PatchAsync(new
                    {
                        basic_pay = decimal.Parse(computedData.BasicPay),
                        overtime_pay = decimal.Parse(computedData.OvertimePerHour),
                        total_earnings = decimal.Parse(computedData.GrossPay),
                        commission = decimal.Parse(computedData.Commission),
                        communication = decimal.Parse(computedData.Communication),
                        food_allowance = decimal.Parse(computedData.FoodAllowance),
                        gas_allowance = decimal.Parse(computedData.GasAllowance),
                        gondola = decimal.Parse(computedData.Gondola),
                        incentives = decimal.Parse(computedData.Incentives),
                        daily_rate = decimal.Parse(computedData.DailyRate),
                        days_present = int.Parse(computedData.DaysPresent),
                        last_updated = timestamp
                    });

                // Update PayrollSummary
                await firebase
                    .Child("PayrollSummary")
                    .Child(payrollId)
                    .PatchAsync(new
                    {
                        gross_pay = decimal.Parse(computedData.GrossPay),
                        net_pay = decimal.Parse(computedData.NetPay),
                        total_deductions = decimal.Parse(computedData.TotalDeductions),
                        last_updated = timestamp
                    });

                // Update GovernmentDeductions
                await firebase
                    .Child("GovernmentDeductions")
                    .Child(payrollId)
                    .PatchAsync(new
                    {
                        sss = decimal.Parse(computedData.SSS),
                        philhealth = decimal.Parse(computedData.Philhealth),
                        pagibig = decimal.Parse(computedData.PagIbig),
                        withholding_tax = decimal.Parse(computedData.WithholdingTax),
                        total_gov_deductions = decimal.Parse(computedData.TotalDeductions),
                        last_updated = timestamp
                    });

                // Update EmployeeDeductions
                await firebase
                    .Child("EmployeeDeductions")
                    .Child(payrollId)
                    .PatchAsync(new
                    {
                        cash_advance = decimal.Parse(computedData.CashAdvance),
                        coop_contribution = decimal.Parse(computedData.CoopContribution),
                        other_deductions = decimal.Parse(computedData.Others),
                        last_updated = timestamp
                    });

                // Update main Payroll record
                await firebase
                    .Child("Payroll")
                    .Child(payrollId)
                    .PatchAsync(new
                    {
                        employee_id = employeeId,
                        net_pay = decimal.Parse(computedData.NetPay),
                        created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                        last_updated = timestamp
                    });

                // Show success message
                Console.WriteLine($"Payroll data automatically saved to Firebase at {timestamp}!");

                // Optional: Show success notification to user
                MessageBox.Show("Payroll data has been successfully calculated and saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // FIXED: Additional validation method for payroll data
        private bool ValidatePayrollData(PayrollExportData data)
        {
            try
            {
                decimal grossPay = decimal.Parse(data.GrossPay);
                decimal totalDeductions = decimal.Parse(data.TotalDeductions);
                decimal netPay = decimal.Parse(data.NetPay);

                // Check for negative values
                if (grossPay < 0 || totalDeductions < 0 || netPay < 0)
                {
                    Console.WriteLine("Validation failed: Negative values detected");
                    return false;
                }

                // Check if deductions exceed gross pay (after capping)
                if (totalDeductions > grossPay)
                {
                    Console.WriteLine("Validation failed: Deductions exceed gross pay");
                    return false;
                }

                // Check if net pay calculation is correct
                if (Math.Abs(grossPay - totalDeductions - netPay) > 0.01m)
                {
                    Console.WriteLine("Validation failed: Net pay calculation error");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Data validation error: {ex.Message}");
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
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }

    // SSS Computation Classes
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
            // Determine MSC (monthly salary credit)
            decimal msc;
            if (salary < 5250) msc = 5000;
            else if (salary >= 34750) msc = 35000;
            else msc = Math.Ceiling((salary + 250) / 500) * 500;

            // Compute contributions based on MSC
            var result = new SssContribution { MSC = msc };
            result.EmployerSS = msc * 0.09m;     // 9% employer share
            result.EmployeeSS = msc * 0.045m;    // 4.5% employee share

            // MPF applies only for MSC >= 20,250
            if (msc >= 20250)
            {
                result.EmployerMPF = msc * 0.01m;  // 1% employer
                result.EmployeeMPF = msc * 0.005m; // 0.5% employee
            }

            // EC is 10 for MSC <= 14999, otherwise 30 (caps at 30)
            result.EmployerEC = (msc <= 14999) ? 10 : 30;

            return result;
        }
    }
}