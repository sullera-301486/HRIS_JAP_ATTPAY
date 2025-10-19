using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class PayrollSummary : Form
    {
        public PayrollExportData ExportData { get; private set; }

        private string currentEmployeeId = "";
        private DateTime cutoffStartDate;
        private DateTime cutoffEndDate;
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        private PayrollEmployeeData employeeData;
        private List<PayrollAttendance> attendanceRecords = new List<PayrollAttendance>();
        private List<PayrollLoan> employeeLoans = new List<PayrollLoan>();
        private PayrollEarnings payrollEarnings = new PayrollEarnings();
        private PayrollDeductions employeeDeductions = new PayrollDeductions();
        private PayrollGovDeductions governmentDeductions = new PayrollGovDeductions();
        private PayrollLeave leaveCredits = new PayrollLeave();

        public PayrollSummary(string employeeId, string period = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            currentEmployeeId = employeeId;
            cutoffStartDate = startDate ?? new DateTime(2025, 9, 1);
            cutoffEndDate = endDate ?? new DateTime(2025, 9, 15);

            InitializeComponent();
            SetFont();

            this.Load += async (sender, e) => await LoadPayrollData();
        }

        public void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            _ = LoadPayrollData();
        }

        private async Task LoadPayrollData()
        {
            try
            {
                await LoadEmployeeBasicInfo();
                await LoadEmploymentInfo(); // This should populate employeeData.Department etc.
                await LoadAttendanceRecords();
                await LoadPayrollEarnings(); // Now this can use employeeData for defaults
                await LoadEmployeeDeductions();
                await LoadGovernmentDeductions();
                await LoadEmployeeLoans();
                await LoadLeaveCredits();

                DisplayEmployeeInfo();
                CalculateAndDisplayPayroll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    employeeData = new PayrollEmployeeData
                    {
                        EmployeeId = GetValue(empData, "employee_id"),
                        FirstName = GetValue(empData, "first_name"),
                        MiddleName = GetValue(empData, "middle_name"),
                        LastName = GetValue(empData, "last_name"),
                        Email = GetValue(empData, "email"),
                        Contact = GetValue(empData, "contact")
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading basic info: {ex.Message}");
            }
        }

        private async Task LoadEmploymentInfo()
        {
            try
            {
                await LoadArrayBasedData("EmploymentInfo", (item) =>
                {
                    var empId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(empId) && empId == currentEmployeeId)
                    {
                        employeeData.Department = item.ContainsKey("department") ? item["department"] : "";
                        employeeData.Position = item.ContainsKey("position") ? item["position"] : "";
                        employeeData.ContractType = item.ContainsKey("contract_type") ? item["contract_type"] : "";
                        employeeData.DateOfJoining = item.ContainsKey("date_of_joining") ? item["date_of_joining"] : "";
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employment info: {ex.Message}");
            }
        }

        private async Task LoadAttendanceRecords()
        {
            try
            {
                await LoadArrayBasedData("Attendance", (item) =>
                {
                    var empId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(empId) && empId == currentEmployeeId)
                    {
                        if (DateTime.TryParse(item.ContainsKey("attendance_date") ? item["attendance_date"] : "", out DateTime attDate))
                        {
                            if (attDate >= cutoffStartDate && attDate <= cutoffEndDate)
                            {
                                var attendance = new PayrollAttendance
                                {
                                    EmployeeId = empId,
                                    AttendanceDate = attDate,
                                    Status = item.ContainsKey("status") ? item["status"] : ""
                                };

                                if (decimal.TryParse(item.ContainsKey("hours_worked") ? item["hours_worked"] : "0", out decimal hoursWorked))
                                    attendance.HoursWorked = hoursWorked;

                                if (decimal.TryParse(item.ContainsKey("overtime_hours") ? item["overtime_hours"] : "0", out decimal overtimeHours))
                                    attendance.OvertimeHours = overtimeHours;

                                attendanceRecords.Add(attendance);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading attendance: {ex.Message}");
            }
        }

        private async Task LoadPayrollEarnings()
        {
            try
            {
                // Try to find existing payroll earnings for this employee directly
                bool foundEarnings = false;

                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                    {
                        // Get employee ID from Payroll table to verify this belongs to current employee
                        var payrollTask = firebase.Child("Payroll").Child(payrollId).OnceSingleAsync<Dictionary<string, object>>();
                        payrollTask.Wait();
                        var payroll = payrollTask.Result;

                        if (payroll != null && payroll.ContainsKey("employee_id") && payroll["employee_id"].ToString() == currentEmployeeId)
                        {
                            // Load ALL earnings data
                            if (decimal.TryParse(item.ContainsKey("daily_rate") ? item["daily_rate"] : "0", out decimal rate))
                                payrollEarnings.DailyRate = rate;

                            if (decimal.TryParse(item.ContainsKey("commission") ? item["commission"] : "0", out decimal commission))
                                payrollEarnings.Commission = commission;

                            if (decimal.TryParse(item.ContainsKey("incentives") ? item["incentives"] : "0", out decimal incentives))
                                payrollEarnings.Incentives = incentives;

                            if (decimal.TryParse(item.ContainsKey("food_allowance") ? item["food_allowance"] : "0", out decimal food))
                                payrollEarnings.FoodAllowance = food;

                            if (decimal.TryParse(item.ContainsKey("gas_allowance") ? item["gas_allowance"] : "0", out decimal gas))
                                payrollEarnings.GasAllowance = gas;

                            if (decimal.TryParse(item.ContainsKey("communication") ? item["communication"] : "0", out decimal comm))
                                payrollEarnings.Communication = comm;

                            if (decimal.TryParse(item.ContainsKey("gondola") ? item["gondola"] : "0", out decimal gondola))
                                payrollEarnings.Gondola = gondola;

                            foundEarnings = true;

                            System.Diagnostics.Debug.WriteLine($"Found earnings for {currentEmployeeId}: DailyRate={rate}, Commission={commission}, Incentives={incentives}");
                        }
                    }
                });

                // If no earnings found through Payroll table, try direct lookup
                if (!foundEarnings)
                {
                    await TryDirectEarningsLookup();
                }

                // If still no earnings found, set defaults
                if (!foundEarnings)
                {
                    SetDefaultEarningsForEmployee();
                    System.Diagnostics.Debug.WriteLine($"Using default earnings for {currentEmployeeId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading earnings: {ex.Message}");
                SetDefaultEarningsForEmployee();
            }
        }
        private async Task TryDirectEarningsLookup()
        {
            try
            {
                // Try to get earnings directly by scanning all records
                var allEarnings = await firebase.Child("PayrollEarnings").OnceAsync<Dictionary<string, object>>();

                foreach (var earningRecord in allEarnings)
                {
                    var earningData = earningRecord.Object;
                    var payrollId = GetValue(earningData, "payroll_id");

                    if (!string.IsNullOrEmpty(payrollId))
                    {
                        // Check if this payroll belongs to current employee
                        var payrollData = await firebase.Child("Payroll").Child(payrollId).OnceSingleAsync<Dictionary<string, object>>();
                        if (payrollData != null && payrollData.ContainsKey("employee_id") &&
                            GetValue(payrollData, "employee_id") == currentEmployeeId)
                        {
                            // Found matching payroll record, load earnings
                            if (decimal.TryParse(GetValue(earningData, "daily_rate"), out decimal rate))
                                payrollEarnings.DailyRate = rate;

                            if (decimal.TryParse(GetValue(earningData, "commission"), out decimal commission))
                                payrollEarnings.Commission = commission;

                            if (decimal.TryParse(GetValue(earningData, "incentives"), out decimal incentives))
                                payrollEarnings.Incentives = incentives;

                            if (decimal.TryParse(GetValue(earningData, "food_allowance"), out decimal food))
                                payrollEarnings.FoodAllowance = food;

                            if (decimal.TryParse(GetValue(earningData, "gas_allowance"), out decimal gas))
                                payrollEarnings.GasAllowance = gas;

                            if (decimal.TryParse(GetValue(earningData, "communication"), out decimal comm))
                                payrollEarnings.Communication = comm;

                            if (decimal.TryParse(GetValue(earningData, "gondola"), out decimal gondola))
                                payrollEarnings.Gondola = gondola;

                            System.Diagnostics.Debug.WriteLine($"Direct lookup found earnings for {currentEmployeeId}");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in direct earnings lookup: {ex.Message}");
            }
        }
        private void SetDefaultEarningsForEmployee()
        {
            // Set default values based on employee data
            payrollEarnings.DailyRate = 500m; // Default fallback

            try
            {
                // Set different defaults based on department/position
                if (employeeData != null && !string.IsNullOrEmpty(employeeData.Department))
                {
                    if (employeeData.Department.Equals("Human Resource", StringComparison.OrdinalIgnoreCase))
                    {
                        payrollEarnings.DailyRate = 500m;
                        payrollEarnings.Incentives = 1000m;
                        payrollEarnings.Commission = 500m;
                    }
                    else if (employeeData.Department.Equals("Finance", StringComparison.OrdinalIgnoreCase))
                    {
                        payrollEarnings.DailyRate = 644.23m;
                        payrollEarnings.Incentives = 800m;
                        payrollEarnings.Commission = 400m;
                    }
                    // Add more department-based defaults as needed
                }

                // Set other earnings to reasonable defaults
                payrollEarnings.FoodAllowance = 400m;
                payrollEarnings.GasAllowance = 300m;
                payrollEarnings.Communication = 200m;
                payrollEarnings.Gondola = 400m;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting default earnings: {ex.Message}");
                // Set safe defaults
                payrollEarnings.DailyRate = 500m;
                payrollEarnings.Commission = 0m;
                payrollEarnings.Incentives = 0m;
                payrollEarnings.FoodAllowance = 0m;
                payrollEarnings.GasAllowance = 0m;
                payrollEarnings.Communication = 0m;
                payrollEarnings.Gondola = 0m;
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
                        if (decimal.TryParse(item.ContainsKey("cash_advance") ? item["cash_advance"] : "0", out decimal cashAdv))
                            employeeDeductions.CashAdvance = cashAdv;

                        if (decimal.TryParse(item.ContainsKey("coop_contribution") ? item["coop_contribution"] : "0", out decimal coop))
                            employeeDeductions.CoopContribution = coop;

                        if (decimal.TryParse(item.ContainsKey("other_deductions") ? item["other_deductions"] : "0", out decimal other))
                            employeeDeductions.OtherDeductions = other;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee deductions: {ex.Message}");
            }
        }

        private async Task LoadGovernmentDeductions()
{
    try
    {
        await LoadArrayBasedData("GovernmentDeductions", (item) =>
        {
            var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
            if (!string.IsNullOrEmpty(payrollId))
            {
                var payrollTask = firebase.Child("Payroll").Child(payrollId).OnceSingleAsync<Dictionary<string, object>>();
                payrollTask.Wait();
                var payroll = payrollTask.Result;

                if (payroll != null && payroll.ContainsKey("employee_id") && payroll["employee_id"].ToString() == currentEmployeeId)
                {
                    if (decimal.TryParse(item.ContainsKey("sss") ? item["sss"] : "0", out decimal sss))
                        governmentDeductions.SSS = sss;

                    if (decimal.TryParse(item.ContainsKey("philhealth") ? item["philhealth"] : "0", out decimal philhealth))
                        governmentDeductions.Philhealth = philhealth;

                    if (decimal.TryParse(item.ContainsKey("pagibig") ? item["pagibig"] : "0", out decimal pagibig))
                        governmentDeductions.Pagibig = pagibig;

                    // REMOVE THIS LINE - We calculate withholding tax dynamically
                    // if (decimal.TryParse(item.ContainsKey("withholding_tax") ? item["withholding_tax"] : "0", out decimal tax))
                    //     governmentDeductions.WithholdingTax = tax;
                }
            }
        });
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error loading government deductions: {ex.Message}");
    }
}

        private async Task LoadEmployeeLoans()
        {
            try
            {
                employeeLoans.Clear(); // Clear existing loans

                // EmployeeLoans is stored as an array, so we need to load it differently
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

                        // Load basic loan information
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

                        // Calculate the current period's amortization based on months paid vs total months
                        loan.BiMonthlyAmortization = CalculateCurrentAmortization(loan);

                        // Still load monthly for reference, but use calculated bi-monthly for payroll
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
        private decimal CalculateCurrentAmortization(PayrollLoan loan)
        {
            try
            {
                // If loan is already paid off, return 0
                if (loan.TotalPaymentDone >= loan.TotalPaymentTerms || loan.Balance <= 0)
                    return 0;

                // If we have the original amortization values, use them
                if (loan.MonthlyAmortization > 0)
                {
                    // For bi-monthly payroll, divide monthly by 2
                    return loan.MonthlyAmortization / 2;
                }

                // Calculate based on remaining balance and remaining terms
                int remainingTerms = loan.TotalPaymentTerms - loan.TotalPaymentDone;
                if (remainingTerms > 0 && loan.Balance > 0)
                {
                    // Calculate monthly payment based on remaining balance and terms
                    decimal monthlyPayment = loan.Balance / remainingTerms;
                    // For bi-monthly payroll, divide by 2
                    return monthlyPayment / 2;
                }

                // Fallback: try to calculate from loan amount and total terms
                if (loan.TotalPaymentTerms > 0 && loan.LoanAmount > 0)
                {
                    decimal monthlyPayment = loan.LoanAmount / loan.TotalPaymentTerms;
                    return monthlyPayment / 2;
                }

                return 0; // Default to 0 if we can't calculate
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating loan amortization: {ex.Message}");
                return 0;
            }
        }

        private async Task LoadLeaveCredits()
        {
            try
            {
                var leaveData = await firebase
                    .Child("Leave Credits")
                    .Child(currentEmployeeId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (leaveData != null)
                {
                    if (decimal.TryParse(GetValue(leaveData, "sick_leave"), out decimal sick))
                        leaveCredits.SickLeave = sick;

                    if (decimal.TryParse(GetValue(leaveData, "vacation_leave"), out decimal vacation))
                        leaveCredits.VacationLeave = vacation;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading leave credits: {ex.Message}");
            }
        }

        private void DisplayEmployeeInfo()
        {
            labelIDInput.Text = employeeData.EmployeeId ?? "N/A";
            labelNameInput.Text = employeeData.FullName ?? "N/A";
            labelDepartmentInput.Text = employeeData.Department ?? "N/A";
            labelPositionInput.Text = employeeData.Position ?? "N/A";
            labelDateCoveredInput.Text = $"{cutoffStartDate:MMMM d} - {cutoffEndDate:d}, {cutoffStartDate:yyyy}";
        }

        private void CalculateAndDisplayPayroll()
        {
            try
            {
                // Calculate work days and hours
                int totalWorkDays = CalculateWorkDays(cutoffStartDate, cutoffEndDate);
                decimal daysPresent = CalculateDaysPresent();
                decimal totalRegularHours = 0;
                decimal totalOvertimeHours = 0;
                int actualDaysWorked = 0;

                // Calculate actual attendance
                foreach (var record in attendanceRecords)
                {
                    if (record.Status != "Absent" && record.Status != "Day Off" && record.HoursWorked > 0)
                    {
                        actualDaysWorked++;
                        totalRegularHours += record.HoursWorked - record.OvertimeHours;
                        totalOvertimeHours += record.OvertimeHours;
                    }
                }

                // Get rates
                decimal dailyRate = payrollEarnings.DailyRate > 0 ? payrollEarnings.DailyRate : 500m;
                decimal hourlyRate = dailyRate / 8m;

                // Calculate earnings
                decimal basicPay = totalRegularHours * hourlyRate;
                decimal overtimePay = totalOvertimeHours * hourlyRate * 1.5m;

                // Calculate taxable income for withholding tax
                decimal taxableIncome = basicPay + overtimePay + payrollEarnings.Commission + payrollEarnings.Incentives;
                decimal withholdingTax = CalculateWithholdingTax(taxableIncome);
                governmentDeductions.WithholdingTax = withholdingTax;

                // Calculate gross pay (include all earnings)
                decimal grossPay = basicPay + overtimePay +
                                  payrollEarnings.Commission +
                                  payrollEarnings.Incentives +
                                  payrollEarnings.FoodAllowance +
                                  payrollEarnings.GasAllowance +
                                  payrollEarnings.Communication +
                                  payrollEarnings.Gondola;

                // Display basic info with detailed breakdown
                DisplayBasicCalculations(dailyRate, totalWorkDays, daysPresent, actualDaysWorked,
                                       totalRegularHours, totalOvertimeHours, basicPay, overtimePay);

                DisplayEarnings();
                DisplayDeductions();
                DisplayLoanDeductions();
                DisplayLeaveCredits();

                // Show computation breakdown
                DisplayComputationBreakdown(dailyRate, hourlyRate, totalRegularHours, totalOvertimeHours,
                                          basicPay, overtimePay, grossPay);

                CalculateTotals(grossPay, basicPay, overtimePay);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating payroll: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayBasicCalculations(decimal dailyRate, int totalWorkDays, decimal daysPresent,
                                    int actualDaysWorked, decimal totalRegularHours,
                                    decimal totalOvertimeHours, decimal basicPay, decimal overtimePay)
        {
            // Basic information
            labelDailyRateInput.Text = dailyRate.ToString("0.00");
            labelDaysInput.Text = totalWorkDays.ToString();
            labelDaysPresentInput.Text = $"{actualDaysWorked} days ({daysPresent:0.00} equivalent)";
            labelSalaryInput.Text = (dailyRate * totalWorkDays).ToString("0.00");

            // Basic pay breakdown
            labelBasicPayAmountBaseInput.Text = (dailyRate * daysPresent).ToString("0.00");
            labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");
            labelBasicPayCredit.Text = $"{totalRegularHours:0.00} hours";

            // Overtime breakdown
            labelOvertimeInput.Text = totalOvertimeHours.ToString("0.00");
            decimal overtimeHourlyRate = (dailyRate / 8m) * 1.5m;
            labelOvertimePerHourAmountBaseInput.Text = overtimeHourlyRate.ToString("0.00");
            labelOvertimePerHourAmountCreditInput.Text = overtimePay.ToString("0.00");
        }

        private void DisplayComputationBreakdown(decimal dailyRate, decimal hourlyRate,
                                               decimal totalRegularHours, decimal totalOvertimeHours,
                                               decimal basicPay, decimal overtimePay, decimal grossPay)
        {
            // You can add labels to show this breakdown or use Debug output
            System.Diagnostics.Debug.WriteLine("=== PAYROLL COMPUTATION BREAKDOWN ===");
            System.Diagnostics.Debug.WriteLine($"Daily Rate: ₱{dailyRate:0.00}");
            System.Diagnostics.Debug.WriteLine($"Hourly Rate: ₱{hourlyRate:0.00}");
            System.Diagnostics.Debug.WriteLine($"Regular Hours: {totalRegularHours:0.00} = ₱{basicPay:0.00}");
            System.Diagnostics.Debug.WriteLine($"Overtime Hours: {totalOvertimeHours:0.00} = ₱{overtimePay:0.00}");
            System.Diagnostics.Debug.WriteLine($"Basic + Overtime: ₱{basicPay + overtimePay:0.00}");
            System.Diagnostics.Debug.WriteLine($"Gross Pay: ₱{grossPay:0.00}");
        }

        private decimal CalculateGrossPayFromAttendance()
        {
            decimal totalRegularHours = 0;
            decimal totalOvertimeHours = 0;
            int daysPresent = 0;

            foreach (var record in attendanceRecords)
            {
                if (record.Status != "Absent" && record.Status != "Day Off")
                {
                    if (record.HoursWorked > 0)
                    {
                        daysPresent++;
                        totalRegularHours += record.HoursWorked - record.OvertimeHours;
                        totalOvertimeHours += record.OvertimeHours;
                    }
                }
            }

            // Get daily rate (default to 500 if not found)
            decimal dailyRate = payrollEarnings.DailyRate > 0 ? payrollEarnings.DailyRate : 500m;
            decimal hourlyRate = dailyRate / 8m;

            // Calculate gross pay - same logic as AdminPayroll
            decimal regularPay = totalRegularHours * hourlyRate;
            decimal overtimePay = totalOvertimeHours * hourlyRate * 1.5m;
            decimal grossPay = regularPay + overtimePay +
                              payrollEarnings.Commission +
                              payrollEarnings.Incentives +
                              payrollEarnings.FoodAllowance +
                              payrollEarnings.GasAllowance +
                              payrollEarnings.Communication +
                              payrollEarnings.Gondola;

            return grossPay;
        }

        private decimal CalculateNetPay(decimal grossPay)
        {
            decimal totalDeductions = 0;

            // Add employee deductions
            totalDeductions += employeeDeductions.CashAdvance;
            totalDeductions += employeeDeductions.CoopContribution;
            totalDeductions += employeeDeductions.OtherDeductions;

            // Add government deductions
            totalDeductions += governmentDeductions.SSS;
            totalDeductions += governmentDeductions.Philhealth;
            totalDeductions += governmentDeductions.Pagibig;
            totalDeductions += governmentDeductions.WithholdingTax;

            // Add loan deductions
            foreach (var loan in employeeLoans)
            {
                totalDeductions += loan.BiMonthlyAmortization;
            }

            decimal netPay = grossPay - totalDeductions;
            return netPay > 0 ? netPay : 0;
        }

        private int CalculateWorkDays(DateTime start, DateTime end)
        {
            int workDays = 0;
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Sunday)
                    workDays++;
            }
            return workDays;
        }

        private decimal CalculateDaysPresent()
        {
            decimal daysPresent = 0;
            foreach (var record in attendanceRecords)
            {
                if (record.Status != "Absent" && record.Status != "Day Off" && record.HoursWorked > 0)
                {
                    daysPresent += record.HoursWorked / 8m;
                }
            }
            return daysPresent;
        }

        private void DisplayEarnings()
        {
            labelCommissionAmountBaseInput.Text = payrollEarnings.Commission.ToString("0.00");
            labelCommissionAmountCreditInput.Text = payrollEarnings.Commission.ToString("0.00");

            labelIncentivesAmountBaseInput.Text = payrollEarnings.Incentives.ToString("0.00");
            labelIncentivesAmountCreditInput.Text = payrollEarnings.Incentives.ToString("0.00");

            labelFoodAllowanceAmountBaseInput.Text = payrollEarnings.FoodAllowance.ToString("0.00");
            labelFoodAllowanceAmountCreditInput.Text = payrollEarnings.FoodAllowance.ToString("0.00");

            labelGasAllowanceAmountBaseInput.Text = payrollEarnings.GasAllowance.ToString("0.00");
            labelGasAllowanceAmountCreditInput.Text = payrollEarnings.GasAllowance.ToString("0.00");

            labelCommunicationAmountBaseInput.Text = payrollEarnings.Communication.ToString("0.00");
            labelCommunicationAmountCreditInput.Text = payrollEarnings.Communication.ToString("0.00");

            labelGondolaAmountBaseInput.Text = payrollEarnings.Gondola.ToString("0.00");
            labelGondolaAmountCreditInput.Text = payrollEarnings.Gondola.ToString("0.00");
        }

        private void DisplayDeductions()
        {
            labelSSSAmountDebitInput.Text = governmentDeductions.SSS.ToString("0.00");
            labelPhilhealthAmountDebitInput.Text = governmentDeductions.Philhealth.ToString("0.00");
            labelPagIbigAmountDebitInput.Text = governmentDeductions.Pagibig.ToString("0.00");
            labelWithTaxAmountDebitInput.Text = governmentDeductions.WithholdingTax.ToString("0.00");

            labelCashAdvanceAmountDebitInput.Text = employeeDeductions.CashAdvance.ToString("0.00");
            labelCoopContriAmountDebitInput.Text = employeeDeductions.CoopContribution.ToString("0.00");
            labelOthersAmountDebitInput.Text = employeeDeductions.OtherDeductions.ToString("0.00");
        }

        private void DisplayLoanDeductions()
        {
            decimal sssLoan = 0, pagibigLoan = 0, carLoan = 0, housingLoan = 0, coopLoan = 0;
            string sssDetails = "", pagibigDetails = "", carDetails = "", housingDetails = "", coopDetails = "";

            System.Diagnostics.Debug.WriteLine($"Displaying loans for {currentEmployeeId}, count: {employeeLoans.Count}");

            foreach (var loan in employeeLoans)
            {
                System.Diagnostics.Debug.WriteLine($"Loan Type: {loan.LoanType}, " +
                    $"BiMonthly: {loan.BiMonthlyAmortization}, " +
                    $"Paid: {loan.TotalPaymentDone}/{loan.TotalPaymentTerms}, " +
                    $"Balance: {loan.Balance}");

                // Only include active loans that aren't fully paid
                if (loan.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                    loan.TotalPaymentDone < loan.TotalPaymentTerms)
                {
                    string progressText = $"{loan.TotalPaymentDone}/{loan.TotalPaymentTerms}  (₱{loan.BiMonthlyAmortization:0.00}/cutoff)";

                    switch (loan.LoanType)
                    {
                        case "SSS Loan":
                            sssLoan = loan.BiMonthlyAmortization;
                            sssDetails = progressText;
                            break;
                        case "Pag-IBIG Loan":
                            pagibigLoan = loan.BiMonthlyAmortization;
                            pagibigDetails = progressText;
                            break;
                        case "Car Loan":
                            carLoan = loan.BiMonthlyAmortization;
                            carDetails = progressText;
                            break;
                        case "Housing Loan":
                            housingLoan = loan.BiMonthlyAmortization;
                            housingDetails = progressText;
                            break;
                        case "Coop Loan":
                            coopLoan = loan.BiMonthlyAmortization;
                            coopDetails = progressText;
                            break;
                    }
                }
            }

            // Set the amortization amounts
            labelSSSLoanAmountDebitInput.Text = sssLoan.ToString("0.00");
            labelPagIbigLoanAmountDebitInput.Text = pagibigLoan.ToString("0.00");
            labelCarLoanAmountDebitInput.Text = carLoan.ToString("0.00");
            labelHousingLoanAmountDebitInput.Text = housingLoan.ToString("0.00");
            labelCoopLoanAmountDebitInput.Text = coopLoan.ToString("0.00");

            // SET THE DETAILS TEXT - THIS IS WHAT YOU'RE MISSING
            labelSSSLoanDetails.Text = sssDetails;
            labelPagIbigLoanDetails.Text = pagibigDetails;
            labelCarLoanDetails.Text = carDetails;
            labelHousingLoanDetails.Text = housingDetails;
            labelCoopLoanDetails.Text = coopDetails;

            System.Diagnostics.Debug.WriteLine($"Loan totals - SSS: {sssLoan}, PagIBIG: {pagibigLoan}, Car: {carLoan}, Housing: {housingLoan}, Coop: {coopLoan}");
        }
        private void DisplayLeaveCredits()
        {
            labelSickLeaveCredit.Text = leaveCredits.SickLeave.ToString("0.00");
            labelVacationLeaveCredit.Text = leaveCredits.VacationLeave.ToString("0.00");
            labelSickLeaveDebit.Text = "0.00";
            labelVacationLeaveDebit.Text = "0.00";
            labelSickLeaveBalance.Text = leaveCredits.SickLeave.ToString("0.00");
            labelVacationLeaveBalance.Text = leaveCredits.VacationLeave.ToString("0.00");
        }

        private void CalculateTotals(decimal grossPay, decimal basicPay, decimal overtimePay)
        {
            decimal totalDeductions = governmentDeductions.SSS +
                                     governmentDeductions.Philhealth +
                                     governmentDeductions.Pagibig +
                                     governmentDeductions.WithholdingTax +
                                     employeeDeductions.CashAdvance +
                                     employeeDeductions.CoopContribution +
                                     employeeDeductions.OtherDeductions;

            // Add loan deductions
            foreach (var loan in employeeLoans)
            {
                totalDeductions += loan.BiMonthlyAmortization;
            }

            decimal netPay = grossPay - totalDeductions;

            labelGrossPayInput.Text = grossPay.ToString("0.00");
            labelDeductionsInput.Text = totalDeductions.ToString("0.00");
            labelOverallTotalInput.Text = netPay.ToString("0.00");

            PrepareExportData(grossPay, totalDeductions, netPay, basicPay, overtimePay);
        }

        private void PrepareExportData(decimal grossPay, decimal totalDeductions, decimal netPay, decimal basicPay, decimal overtimePay)
        {
            ExportData = new PayrollExportData
            {
                EmployeeId = employeeData.EmployeeId,
                EmployeeName = employeeData.FullName,
                Department = employeeData.Department,
                Position = employeeData.Position,
                DateCovered = labelDateCoveredInput.Text,
                Days = labelDaysInput.Text,
                DaysPresent = labelDaysPresentInput.Text,
                DailyRate = labelDailyRateInput.Text,
                Salary = labelSalaryInput.Text,
                Overtime = labelOvertimeInput.Text,
                BasicPay = basicPay.ToString("0.00"),
                OvertimePerHour = labelOvertimePerHourAmountCreditInput.Text,
                OvertimePerMinute = "0.00", // Set appropriate value if needed
                Incentives = payrollEarnings.Incentives.ToString("0.00"),
                Commission = payrollEarnings.Commission.ToString("0.00"),
                FoodAllowance = payrollEarnings.FoodAllowance.ToString("0.00"),
                Communication = payrollEarnings.Communication.ToString("0.00"),
                GasAllowance = payrollEarnings.GasAllowance.ToString("0.00"),
                Gondola = payrollEarnings.Gondola.ToString("0.00"),
                GrossPay = grossPay.ToString("0.00"),
                // ... rest of your existing properties
                WithholdingTax = governmentDeductions.WithholdingTax.ToString("0.00"),
                SSS = governmentDeductions.SSS.ToString("0.00"),
                PagIbig = governmentDeductions.Pagibig.ToString("0.00"),
                Philhealth = governmentDeductions.Philhealth.ToString("0.00"),
                SSSLoan = labelSSSLoanAmountDebitInput.Text,
                PagIbigLoan = labelPagIbigLoanAmountDebitInput.Text,
                CarLoan = labelCarLoanAmountDebitInput.Text,
                HousingLoan = labelHousingLoanAmountDebitInput.Text,
                CashAdvance = employeeDeductions.CashAdvance.ToString("0.00"),
                CoopLoan = labelCoopLoanAmountDebitInput.Text,
                CoopContribution = employeeDeductions.CoopContribution.ToString("0.00"),
                Others = employeeDeductions.OtherDeductions.ToString("0.00"),
                TotalDeductions = totalDeductions.ToString("0.00"),
                NetPay = netPay.ToString("0.00"),
                VacationLeaveCredit = labelVacationLeaveCredit.Text,
                VacationLeaveDebit = labelVacationLeaveDebit.Text,
                VacationLeaveBalance = labelVacationLeaveBalance.Text,
                SickLeaveCredit = labelSickLeaveCredit.Text,
                SickLeaveDebit = labelSickLeaveDebit.Text,
                SickLeaveBalance = labelSickLeaveBalance.Text,
                //details
                TaxDetails = labelWithTaxDetails.Text,
                SSSDetails = labelSSSDetails.Text,
                PhilhealthDetails = labelPhilhealthDetails.Text,
                SSSLoanDetails = labelSSSLoanDetails.Text,
                PagIbigLoanDetails = labelPagIbigLoanDetails.Text,
                PagIbigDetails = labelPagIbigDetails.Text,
                CarLoanDetails = labelCarLoanDetails.Text,
                HousingLoanDetails = labelHousingLoanDetails.Text,
                CoopLoanDetails = labelCoopLoanDetails.Text,
                CoopContributionDetails = labelCoopContriDetails.Text,
                CashAdvanceDetails = labelCashAdvanceDetails.Text,
                OthersDetails = labelOthersDetails.Text

            };
        }

        // Helper methods for data loading
        private async Task LoadArrayBasedData(string childPath, Action<Dictionary<string, string>> processItem)
        {
            try
            {
                // Get raw JSON from Firebase
                var jsonResponse = await firebase.Child(childPath).OnceAsJsonAsync();
                string rawJson = jsonResponse.ToString();

                // Parse the malformed JSON structure
                var records = ParseMalformedJson(rawJson);
                foreach (var record in records)
                    processItem(record);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading {childPath}: {ex.Message}");
            }
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
                    if (record.Count > 0) records.Add(record);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON parsing error: {ex.Message}");
            }
            return records;
        }

        private string GetValue(Dictionary<string, object> data, string key)
        {
            try
            {
                if (data != null && data.ContainsKey(key) && data[key] != null)
                {
                    string value = data[key].ToString();
                    return string.IsNullOrWhiteSpace(value) || value == "N/A" ? "" : value.Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting value for key '{key}': {ex.Message}");
            }
            return "";
        }

        // Rest of your existing methods (button events, font setting, etc.)
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

            // Show the edit form as a dialog so we can refresh after it closes
            payrollSummaryEditForm.FormClosed += async (s, args) =>
            {
                // Refresh data when the edit form closes
                await RefreshPayrollData();
            };

            AttributesClass.ShowWithOverlay(parentForm, payrollSummaryEditForm);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            buttonCancel_Click(sender, e);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public async Task RefreshPayrollData()
        {
            try
            {
                // Clear ALL existing data
                employeeData = null;
                attendanceRecords.Clear();
                employeeLoans.Clear();
                payrollEarnings = new PayrollEarnings();
                employeeDeductions = new PayrollDeductions();
                governmentDeductions = new PayrollGovDeductions();
                leaveCredits = new PayrollLeave();

                // Reload ALL data from Firebase
                await LoadEmployeeBasicInfo();
                await LoadEmploymentInfo();
                await LoadAttendanceRecords();
                await LoadPayrollEarnings(); // This now loads the updated earnings
                await LoadEmployeeDeductions();
                await LoadGovernmentDeductions();
                await LoadEmployeeLoans();
                await LoadLeaveCredits();

                DisplayEmployeeInfo();
                CalculateAndDisplayPayroll();

                // Force UI refresh
                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing payroll data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private decimal CalculateWithholdingTax(decimal taxableIncome)
        {
            // Philippine Tax Brackets for 2025 (Monthly)
            // Note: Since your payroll appears to be semi-monthly (cutoff periods), 
            // we need to adjust the brackets accordingly

            // Convert to monthly equivalent for tax calculation
            decimal monthlyIncome = taxableIncome * 2;

            if (monthlyIncome <= 20833.33m)
                return 0.0m;
            else if (monthlyIncome <= 33333.33m)
                return ((monthlyIncome - 20833.33m) * 0.20m) / 2; // Divide by 2 for semi-monthly
            else if (monthlyIncome <= 66666.67m)
                return (2500.0m + (monthlyIncome - 33333.33m) * 0.25m) / 2;
            else if (monthlyIncome <= 166666.67m)
                return (10833.33m + (monthlyIncome - 66666.67m) * 0.30m) / 2;
            else if (monthlyIncome <= 666666.67m)
                return (40833.33m + (monthlyIncome - 166666.67m) * 0.32m) / 2;
            else
                return (200833.33m + (monthlyIncome - 666666.67m) * 0.35m) / 2;
        }
        private void SetFont()
        {
            labelSSSLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCarLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelAmountBase.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelAmountCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelAmountDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelBasicPay.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelBasicPayAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelBasicPayAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelBasicPayCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCarLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCarLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCashAdvance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCashAdvanceAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommission.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCommissionAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommissionAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommunication.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCommunicationAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommunicationAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopContri.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCoopContriAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCoopLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDateCovered.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDateCoveredInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDays.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDaysPresent.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDaysPresentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDeductions.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDeductionsInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
            labelOverallTotalInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerHour.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimePerHourAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerHourAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerMinute.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimePerMinuteAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerMinuteAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbig.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelPagIbigAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelPagIbigLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPhilhealth.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelPhilhealthAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
            labelSSSLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelSSSLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelVacationLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelVacationLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelVacationLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelWithTax.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelWithTaxAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            buttonExport.Font = AttributesClass.GetFont("Roboto-Regular", 14f);

            labelWithTaxDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelSSSDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPhilhealthDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelSSSLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCarLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCashAdvanceDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopContriDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOthersDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPayAndAllowances.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDetails.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimePerHourCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerMinuteCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
        }
    }
    //hatdog
    public class PayrollEmployeeData
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string ContractType { get; set; }
        public string DateOfJoining { get; set; }

        public string FullName => $"{LastName}, {FirstName} {MiddleName}".Trim().Replace("  ", " ");
    }

    public class PayrollAttendance
    {
        public string EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public string Status { get; set; }
    }

    public class PayrollLoan
    {
        public string LoanType { get; set; }
        public decimal BiMonthlyAmortization { get; set; }
        public decimal MonthlyAmortization { get; set; }
        public string Status { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal Balance { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int TotalPaymentTerms { get; set; }
        public int TotalPaymentDone { get; set; }
        public string PaymentProgress => $"{TotalPaymentDone}/{TotalPaymentTerms}";
        public int MonthsLeft => TotalPaymentTerms - TotalPaymentDone;
        public bool IsFullyPaid => TotalPaymentDone >= TotalPaymentTerms;
    }

    public class PayrollEarnings
    {
        public decimal Commission { get; set; }
        public decimal Incentives { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal GasAllowance { get; set; }
        public decimal Communication { get; set; }
        public decimal Gondola { get; set; }
        public decimal BasicPay { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal DailyRate { get; set; }
        public decimal DaysPresent { get; set; }
    }

    public class PayrollDeductions
    {
        public decimal CashAdvance { get; set; }
        public decimal CoopContribution { get; set; }
        public decimal OtherDeductions { get; set; }
    }

    public class PayrollGovDeductions
    {
        public decimal SSS { get; set; }
        public decimal Philhealth { get; set; }
        public decimal Pagibig { get; set; }
        public decimal WithholdingTax { get; set; }
    }

    public class PayrollLeave
    {
        public decimal SickLeave { get; set; }
        public decimal VacationLeave { get; set; }
    }
}