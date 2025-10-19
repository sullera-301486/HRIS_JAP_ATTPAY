using DocumentFormat.OpenXml.Drawing.Charts;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminPayroll : UserControl
    {
        private AttributesClassAlt panelLoaderAdminLoan;
        public Panel AdminViewPanel;
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string currentEmployeeId;
        private string payrollPeriod;

        private System.Threading.Timer searchTimer;

        public AdminPayroll(Panel targetPanel, string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();
            setTextBoxAttributes();
            LoadDateRanges();

            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;
            comboBoxSelectPayDate.SelectedIndexChanged += comboBoxSelectPayDate_SelectedIndexChanged;



            AdminViewPanel = targetPanel;
            panelLoaderAdminLoan = new AttributesClassAlt(AdminViewPanel);
        }


        private async void LoadDateRanges()
        {
            try
            {
                comboBoxSelectPayDate.Items.Clear();

                // Get available date ranges from attendance records
                var availablePeriods = await GetAvailableDateRangesFromAttendance();

                if (availablePeriods.Count == 0)
                {
                    // Fallback to current year if no attendance data
                    LoadDefaultDateRanges();
                    return;
                }

                // Add available periods to combobox
                foreach (var period in availablePeriods)
                {
                    comboBoxSelectPayDate.Items.Add(period);
                }

                // Set default to most recent period
                if (comboBoxSelectPayDate.Items.Count > 0)
                {
                    comboBoxSelectPayDate.SelectedIndex = comboBoxSelectPayDate.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load date ranges: " + ex.Message);
                // Fallback to default ranges
                LoadDefaultDateRanges();
            }
        }

        private async Task<List<string>> GetAvailableDateRangesFromAttendance()
        {
            var availablePeriods = new List<string>();
            var attendanceDates = new HashSet<DateTime>();

            try
            {
                // Load attendance records to find available dates
                var attendanceData = await firebase
                    .Child("Attendance")
                    .OnceAsync<Dictionary<string, object>>();

                // Extract unique dates from attendance records
                foreach (var attendance in attendanceData)
                {
                    var data = attendance.Object;
                    if (data.ContainsKey("attendance_date") &&
                        DateTime.TryParse(data["attendance_date"]?.ToString(), out DateTime attendanceDate))
                    {
                        attendanceDates.Add(attendanceDate.Date);
                    }
                }

                // Group dates by month and create semi-monthly periods
                var datesByMonth = attendanceDates
                    .GroupBy(d => new { d.Year, d.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month);

                foreach (var monthGroup in datesByMonth)
                {
                    int year = monthGroup.Key.Year;
                    int month = monthGroup.Key.Month;
                    string monthName = new DateTime(year, month, 1).ToString("MMMM");
                    int daysInMonth = DateTime.DaysInMonth(year, month);

                    // Check first half (1st to 15th)
                    bool hasFirstHalf = monthGroup.Any(d => d.Day >= 1 && d.Day <= 15);
                    if (hasFirstHalf)
                    {
                        availablePeriods.Add($"{monthName} 1 - 15, {year}");
                    }

                    // Check second half (16th to end of month)
                    bool hasSecondHalf = monthGroup.Any(d => d.Day >= 16 && d.Day <= daysInMonth);
                    if (hasSecondHalf)
                    {
                        availablePeriods.Add($"{monthName} 16 - {daysInMonth}, {year}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting available date ranges: {ex.Message}");
            }

            return availablePeriods;
        }

        private void LoadDefaultDateRanges()
        {
            try
            {
                comboBoxSelectPayDate.Items.Clear();

                // Fallback: Add current year periods
                int currentYear = DateTime.Now.Year;

                for (int month = 1; month <= 12; month++)
                {
                    string monthName = new DateTime(currentYear, month, 1).ToString("MMMM");
                    int daysInMonth = DateTime.DaysInMonth(currentYear, month);

                    // First half: 1st to 15th
                    comboBoxSelectPayDate.Items.Add($"{monthName} 1 - 15, {currentYear}");

                    // Second half: 16th to end of month
                    comboBoxSelectPayDate.Items.Add($"{monthName} 16 - {daysInMonth}, {currentYear}");
                }

                // Set default to current period
                int currentMonth = DateTime.Now.Month;
                int currentDay = DateTime.Now.Day;
                int currentIndex = (currentMonth - 1) * 2 + (currentDay > 15 ? 1 : 0);

                if (currentIndex < comboBoxSelectPayDate.Items.Count)
                {
                    comboBoxSelectPayDate.SelectedIndex = currentIndex;
                }
                else
                {
                    comboBoxSelectPayDate.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load default date ranges: " + ex.Message);
            }
        }

        private void comboBoxSelectPayDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectPayDate.SelectedItem != null)
            {
                string selectedDateRange = comboBoxSelectPayDate.SelectedItem.ToString();
                labelPayrollDate.Text = selectedDateRange;

                // Reload data to recalculate for new date range
                LoadFirebaseData();
            }
        }


        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            searchTimer?.Dispose();

            if (string.IsNullOrEmpty(textBoxSearchEmployee.Text) || textBoxSearchEmployee.Text == "Find Employee")
            {
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                    if (!row.IsNewRow) row.Visible = true;
                return;
            }

            searchTimer = new System.Threading.Timer(_ =>
            {
                this.Invoke((MethodInvoker)delegate { PerformSearch(); });
            }, null, 300, System.Threading.Timeout.Infinite);
        }

        private void PerformSearch()
        {
            string searchText = textBoxSearchEmployee.Text.Trim();
            if (string.IsNullOrEmpty(searchText) || searchText == "Find Employee")
            {
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                    if (!row.IsNewRow) row.Visible = true;
                return;
            }

            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (!row.IsNewRow)
                {
                    string employeeId = row.Cells["EmployeeId"].Value?.ToString() ?? "";
                    string fullName = row.Cells["FullName"].Value?.ToString() ?? "";
                    string department = row.Cells["Department"].Value?.ToString() ?? "";

                    bool isMatch = employeeId.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   fullName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   department.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                    row.Visible = isMatch;
                }
            }
        }

        private void buttonExportAll_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmPayrollExportAll confirmPayrollExportAllForm = new ConfirmPayrollExportAll();
            AttributesClass.ShowWithOverlay(parentForm, confirmPayrollExportAllForm);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminPayroll filterAdminPayrollform = new FilterAdminPayroll();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminPayrollform);
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewEmployee.ReadOnly = true;
            dataGridViewEmployee.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewEmployee.MultiSelect = false;
            dataGridViewEmployee.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewEmployee.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewEmployee.DefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 137, 207);
            dataGridViewEmployee.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridViewEmployee.GridColor = Color.White;
            dataGridViewEmployee.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewEmployee.ColumnHeadersHeight = 40;
            dataGridViewEmployee.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewEmployee.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewEmployee.CellMouseEnter += dataGridViewEmployee_CellMouseEnter;
            dataGridViewEmployee.CellMouseLeave += dataGridViewEmployee_CellMouseLeave;
            dataGridViewEmployee.CellClick += dataGridViewEmployee_CellClick;

            dataGridViewEmployee.Columns.Clear();
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "RowNumber", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "GrossPay", HeaderText = "Gross Pay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "NetPay", HeaderText = "Net Pay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });

            var actionCol = new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25,
                Image = Properties.Resources.ExpandRight
            };
            dataGridViewEmployee.Columns.Add(actionCol);
        }

        private void setFont()
        {
            try
            {
                labelAdminPayroll.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelPayrollDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMoveLoan.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Underline);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                buttonExportAll.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
                comboBoxSelectPayDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void dataGridViewEmployee_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
                dataGridViewEmployee.Cursor = Cursors.Hand;
        }

        private void dataGridViewEmployee_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewEmployee.Cursor = Cursors.Default;
        }

        private async void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
            {
                string selectedEmployeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();
                if (!string.IsNullOrEmpty(selectedEmployeeId))
                {
                    // Get the current selected date range
                    DateTime startDate, endDate;
                    ParseDateRange(comboBoxSelectPayDate.SelectedItem?.ToString(), out startDate, out endDate);

                    Form parentForm = this.FindForm();
                    PayrollSummary payrollSummaryForm = new PayrollSummary(selectedEmployeeId, null, startDate, endDate);
                    AttributesClass.ShowWithOverlay(parentForm, payrollSummaryForm);
                }
            }
        }

        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();

                // Get selected date range FIRST
                DateTime startDate, endDate;
                ParseDateRange(comboBoxSelectPayDate.SelectedItem?.ToString(), out startDate, out endDate);

                var employeeDetails = new Dictionary<string, dynamic>();
                var employmentInfo = new Dictionary<string, Dictionary<string, string>>();
                var attendanceRecords = new Dictionary<string, List<Dictionary<string, string>>>();
                var dailyRates = new Dictionary<string, decimal>();
                var employeeDeductions = new Dictionary<string, Dictionary<string, decimal>>();
                var governmentDeductions = new Dictionary<string, Dictionary<string, decimal>>();
                var employeeLoans = new Dictionary<string, List<Dictionary<string, string>>>();

                // Load EmployeeDetails
                var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                foreach (var emp in empDetails)
                    employeeDetails[emp.Key] = emp.Object;

                // Load EmploymentInfo
                await LoadArrayBasedData("EmploymentInfo", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        employmentInfo[employeeId] = item;
                });

                // Load Attendance Records - FILTER BY DATE RANGE
                await LoadArrayBasedData("Attendance", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        // Parse and check if attendance date is within selected range
                        if (DateTime.TryParse(item.ContainsKey("attendance_date") ? item["attendance_date"] : "", out DateTime attendanceDate))
                        {
                            if (attendanceDate >= startDate && attendanceDate <= endDate)
                            {
                                if (!attendanceRecords.ContainsKey(employeeId))
                                    attendanceRecords[employeeId] = new List<Dictionary<string, string>>();
                                attendanceRecords[employeeId].Add(item);
                            }
                        }
                    }
                });

                // Load Employee Loans - FILTER BY START DATE (only include loans that started before or during the period)
                await LoadArrayBasedData("EmployeeLoans", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    var status = item.ContainsKey("status") ? item["status"] : null;

                    if (!string.IsNullOrEmpty(employeeId) &&
                        status != null && status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if loan start date is before or during the selected period
                        if (DateTime.TryParse(item.ContainsKey("start_date") ? item["start_date"] : "", out DateTime loanStartDate))
                        {
                            if (loanStartDate <= endDate) // Loan started before or during the period
                            {
                                if (!employeeLoans.ContainsKey(employeeId))
                                    employeeLoans[employeeId] = new List<Dictionary<string, string>>();
                                employeeLoans[employeeId].Add(item);
                            }
                        }
                        else
                        {
                            // If no start date, include the loan
                            if (!employeeLoans.ContainsKey(employeeId))
                                employeeLoans[employeeId] = new List<Dictionary<string, string>>();
                            employeeLoans[employeeId].Add(item);
                        }
                    }
                });

                // Load Daily Rates from PayrollEarnings - Use most recent record within the period
                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                    {
                        // Get employee ID from Payroll table and check if it's within our period
                        var payrollTask = firebase.Child("Payroll").Child(payrollId).OnceSingleAsync<Dictionary<string, object>>();
                        payrollTask.Wait();
                        var payroll = payrollTask.Result;

                        if (payroll != null && payroll.ContainsKey("employee_id"))
                        {
                            string employeeId = payroll["employee_id"].ToString();

                            // Check if payroll period overlaps with our selected period
                            if (IsPayrollInPeriod(payroll, startDate, endDate))
                            {
                                if (decimal.TryParse(item.ContainsKey("daily_rate") ? item["daily_rate"] : "0", out decimal rate) && rate > 0)
                                {
                                    // Only update if we don't have a rate yet or if this is more recent
                                    if (!dailyRates.ContainsKey(employeeId) ||
                                        (payroll.ContainsKey("last_updated") &&
                                         DateTime.TryParse(payroll["last_updated"].ToString(), out DateTime lastUpdated) &&
                                         lastUpdated > startDate))
                                    {
                                        dailyRates[employeeId] = rate;
                                    }
                                }
                            }
                        }
                    }
                });

                // Load Employee Deductions - Use most recent record within the period
                await LoadArrayBasedData("EmployeeDeductions", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        // Check last updated date to see if it's within our period
                        if (DateTime.TryParse(item.ContainsKey("last_updated") ? item["last_updated"] : "", out DateTime lastUpdated))
                        {
                            if (lastUpdated >= startDate && lastUpdated <= endDate)
                            {
                                if (!employeeDeductions.ContainsKey(employeeId))
                                    employeeDeductions[employeeId] = new Dictionary<string, decimal>();

                                if (decimal.TryParse(item.ContainsKey("cash_advance") ? item["cash_advance"] : "0", out decimal cashAdvance))
                                    employeeDeductions[employeeId]["cash_advance"] = cashAdvance;

                                if (decimal.TryParse(item.ContainsKey("coop_contribution") ? item["coop_contribution"] : "0", out decimal coop))
                                    employeeDeductions[employeeId]["coop_contribution"] = coop;

                                if (decimal.TryParse(item.ContainsKey("other_deductions") ? item["other_deductions"] : "0", out decimal other))
                                    employeeDeductions[employeeId]["other_deductions"] = other;
                            }
                        }
                    }
                });

                // Load Government Deductions - Use most recent record within the period
                await LoadArrayBasedData("GovernmentDeductions", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                    {
                        // Get employee ID from Payroll table and check period
                        var payrollTask = firebase.Child("Payroll").Child(payrollId).OnceSingleAsync<Dictionary<string, object>>();
                        payrollTask.Wait();
                        var payroll = payrollTask.Result;

                        if (payroll != null && payroll.ContainsKey("employee_id") && IsPayrollInPeriod(payroll, startDate, endDate))
                        {
                            string employeeId = payroll["employee_id"].ToString();
                            governmentDeductions[employeeId] = new Dictionary<string, decimal>();

                            if (decimal.TryParse(item.ContainsKey("sss") ? item["sss"] : "0", out decimal sss))
                                governmentDeductions[employeeId]["sss"] = sss;

                            if (decimal.TryParse(item.ContainsKey("philhealth") ? item["philhealth"] : "0", out decimal philhealth))
                                governmentDeductions[employeeId]["philhealth"] = philhealth;

                            if (decimal.TryParse(item.ContainsKey("pagibig") ? item["pagibig"] : "0", out decimal pagibig))
                                governmentDeductions[employeeId]["pagibig"] = pagibig;

                            // Calculate withholding tax dynamically based on taxable income for the period
                            decimal taxableIncome = CalculateTaxableIncome(employeeId, startDate, endDate, attendanceRecords, dailyRates);
                            decimal withholdingTax = CalculateWithholdingTax(taxableIncome);
                            governmentDeductions[employeeId]["withholding_tax"] = withholdingTax;
                        }
                    }
                });

                int counter = 1;
                foreach (var empEntry in employeeDetails)
                {
                    string employeeId = empEntry.Key;
                    dynamic empData = empEntry.Value;
                    string fullName = $"{empData.first_name ?? ""} {empData.middle_name ?? ""} {empData.last_name ?? ""}".Trim();

                    string department = "";
                    string position = "";
                    if (employmentInfo.ContainsKey(employeeId))
                    {
                        var empInfo = employmentInfo[employeeId];
                        department = empInfo.ContainsKey("department") ? empInfo["department"] : "";
                        position = empInfo.ContainsKey("position") ? empInfo["position"] : "";
                    }

                    // Calculate payroll for the SPECIFIC date range
                    decimal grossPay = CalculateGrossPayFromAttendance(employeeId, startDate, endDate, attendanceRecords, dailyRates);
                    decimal netPay = CalculateNetPay(grossPay, employeeId, employeeDeductions, governmentDeductions, employeeLoans);

                    // Only show employees who have data in this period (attendance, loans, or deductions)
                    if (attendanceRecords.ContainsKey(employeeId) ||
                        employeeDeductions.ContainsKey(employeeId) ||
                        governmentDeductions.ContainsKey(employeeId) ||
                        employeeLoans.ContainsKey(employeeId))
                    {
                        dataGridViewEmployee.Rows.Add(
                            counter,
                            employeeId,
                            fullName,
                            department,
                            position,
                            $"₱ {grossPay:N2}",
                            $"₱ {netPay:N2}",
                            Properties.Resources.ExpandRight
                        );

                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Firebase data: " + ex.Message);
            }
        }
        private bool IsPayrollInPeriod(Dictionary<string, object> payroll, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (payroll.ContainsKey("cutoff_start") && payroll.ContainsKey("cutoff_end"))
                {
                    DateTime payrollStart = DateTime.Parse(payroll["cutoff_start"].ToString());
                    DateTime payrollEnd = DateTime.Parse(payroll["cutoff_end"].ToString());

                    // Check if payroll period overlaps with our selected period
                    return (payrollStart <= endDate) && (payrollEnd >= startDate);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking payroll period: {ex.Message}");
            }

            // If we can't determine, include it
            return true;
        }

        private decimal CalculateTaxableIncome(string employeeId, DateTime startDate, DateTime endDate,
            Dictionary<string, List<Dictionary<string, string>>> attendanceRecords,
            Dictionary<string, decimal> dailyRates)
        {
            decimal totalRegularHours = 0;
            decimal totalOvertimeHours = 0;

            if (attendanceRecords.ContainsKey(employeeId))
            {
                var employeeAttendance = attendanceRecords[employeeId];

                foreach (var record in employeeAttendance)
                {
                    string status = record.ContainsKey("status") ? record["status"] : "";

                    if (status != "Absent" && status != "Day Off")
                    {
                        if (decimal.TryParse(record.ContainsKey("hours_worked") ? record["hours_worked"] : "0", out decimal hoursWorked) && hoursWorked > 0)
                        {
                            decimal overtimeHours = 0;
                            if (decimal.TryParse(record.ContainsKey("overtime_hours") ? record["overtime_hours"] : "0", out overtimeHours))
                            {
                                totalRegularHours += hoursWorked - overtimeHours;
                                totalOvertimeHours += overtimeHours;
                            }
                            else
                            {
                                totalRegularHours += hoursWorked;
                            }
                        }
                    }
                }
            }

            // Get daily rate (default to 500 if not found)
            decimal dailyRate = dailyRates.ContainsKey(employeeId) ? dailyRates[employeeId] : 500m;
            decimal hourlyRate = dailyRate / 8m;

            // Calculate taxable income (basic pay + overtime)
            decimal regularPay = totalRegularHours * hourlyRate;
            decimal overtimePay = totalOvertimeHours * hourlyRate * 1.5m;

            return regularPay + overtimePay;
        }

        private decimal CalculateWithholdingTax(decimal taxableIncome)
        {
            // Philippine Tax Brackets for 2025 (Monthly)
            // Convert to monthly equivalent for tax calculation
            decimal monthlyIncome = taxableIncome * 2;

            if (monthlyIncome <= 20833.33m)
                return 0.0m;
            else if (monthlyIncome <= 33333.33m)
                return ((monthlyIncome - 20833.33m) * 0.20m) / 2;
            else if (monthlyIncome <= 66666.67m)
                return (2500.0m + (monthlyIncome - 33333.33m) * 0.25m) / 2;
            else if (monthlyIncome <= 166666.67m)
                return (10833.33m + (monthlyIncome - 66666.67m) * 0.30m) / 2;
            else if (monthlyIncome <= 666666.67m)
                return (40833.33m + (monthlyIncome - 166666.67m) * 0.32m) / 2;
            else
                return (200833.33m + (monthlyIncome - 666666.67m) * 0.35m) / 2;
        }

        private decimal CalculateNetPay(decimal grossPay, string employeeId,
            Dictionary<string, Dictionary<string, decimal>> employeeDeductions,
            Dictionary<string, Dictionary<string, decimal>> governmentDeductions,
            Dictionary<string, List<Dictionary<string, string>>> employeeLoans)
        {
            decimal totalDeductions = 0;

            // Add employee deductions
            if (employeeDeductions.ContainsKey(employeeId))
            {
                var deductions = employeeDeductions[employeeId];
                totalDeductions += deductions.ContainsKey("cash_advance") ? deductions["cash_advance"] : 0;
                totalDeductions += deductions.ContainsKey("coop_contribution") ? deductions["coop_contribution"] : 0;
                totalDeductions += deductions.ContainsKey("other_deductions") ? deductions["other_deductions"] : 0;
            }

            // Add government deductions
            if (governmentDeductions.ContainsKey(employeeId))
            {
                var govDeductions = governmentDeductions[employeeId];
                totalDeductions += govDeductions.ContainsKey("sss") ? govDeductions["sss"] : 0;
                totalDeductions += govDeductions.ContainsKey("philhealth") ? govDeductions["philhealth"] : 0;
                totalDeductions += govDeductions.ContainsKey("pagibig") ? govDeductions["pagibig"] : 0;
                totalDeductions += govDeductions.ContainsKey("withholding_tax") ? govDeductions["withholding_tax"] : 0;
            }

            // Add loan deductions (only for active loans within the period)
            if (employeeLoans.ContainsKey(employeeId))
            {
                foreach (var loan in employeeLoans[employeeId])
                {
                    // Use bi-monthly amortization if available, otherwise monthly divided by 2
                    if (decimal.TryParse(loan.ContainsKey("bi_monthly_amortization") ? loan["bi_monthly_amortization"] : "0", out decimal biMonthly) && biMonthly > 0)
                    {
                        totalDeductions += biMonthly;
                    }
                    else if (decimal.TryParse(loan.ContainsKey("monthly_amortization") ? loan["monthly_amortization"] : "0", out decimal monthly) && monthly > 0)
                    {
                        totalDeductions += monthly / 2; // Convert to semi-monthly
                    }
                }
            }

            decimal netPay = grossPay - totalDeductions;
            return netPay > 0 ? netPay : 0;
        }
        private decimal CalculateGrossPayFromAttendance(string employeeId, DateTime startDate, DateTime endDate,
    Dictionary<string, List<Dictionary<string, string>>> attendanceRecords, Dictionary<string, decimal> dailyRates)
        {
            decimal totalRegularHours = 0;
            decimal totalOvertimeHours = 0;
            int daysPresent = 0;

            if (attendanceRecords.ContainsKey(employeeId))
            {
                var employeeAttendance = attendanceRecords[employeeId];

                foreach (var record in employeeAttendance)
                {
                    // Parse attendance date
                    if (DateTime.TryParse(record.ContainsKey("attendance_date") ? record["attendance_date"] : "", out DateTime attendanceDate))
                    {
                        // Check if within selected period
                        if (attendanceDate >= startDate && attendanceDate <= endDate)
                        {
                            string status = record.ContainsKey("status") ? record["status"] : "";

                            // Skip absent and day off records
                            if (status != "Absent" && status != "Day Off")
                            {
                                // Parse hours worked
                                if (decimal.TryParse(record.ContainsKey("hours_worked") ? record["hours_worked"] : "0", out decimal hoursWorked) && hoursWorked > 0)
                                {
                                    daysPresent++;

                                    // Parse overtime hours
                                    decimal overtimeHours = 0;
                                    if (decimal.TryParse(record.ContainsKey("overtime_hours") ? record["overtime_hours"] : "0", out overtimeHours))
                                    {
                                        totalRegularHours += hoursWorked - overtimeHours;
                                        totalOvertimeHours += overtimeHours;
                                    }
                                    else
                                    {
                                        totalRegularHours += hoursWorked;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Get daily rate (default to 500 if not found)
            decimal dailyRate = dailyRates.ContainsKey(employeeId) ? dailyRates[employeeId] : 500m;
            decimal hourlyRate = dailyRate / 8m; // Assuming 8-hour work day

            // Calculate gross pay
            decimal regularPay = totalRegularHours * hourlyRate;
            decimal overtimePay = totalOvertimeHours * hourlyRate * 1.5m; // 1.5x for overtime
            decimal grossPay = regularPay + overtimePay;

            return grossPay;
        }
        private decimal CalculateNetPay(decimal grossPay, string employeeId,
    Dictionary<string, Dictionary<string, decimal>> employeeDeductions,
    Dictionary<string, Dictionary<string, decimal>> governmentDeductions)
        {
            decimal totalDeductions = 0;

            // Add employee deductions
            if (employeeDeductions.ContainsKey(employeeId))
            {
                var deductions = employeeDeductions[employeeId];
                totalDeductions += deductions.ContainsKey("cash_advance") ? deductions["cash_advance"] : 0;
                totalDeductions += deductions.ContainsKey("coop_contribution") ? deductions["coop_contribution"] : 0;
                totalDeductions += deductions.ContainsKey("other_deductions") ? deductions["other_deductions"] : 0;
            }

            // Add government deductions
            if (governmentDeductions.ContainsKey(employeeId))
            {
                var govDeductions = governmentDeductions[employeeId];
                totalDeductions += govDeductions.ContainsKey("sss") ? govDeductions["sss"] : 0;
                totalDeductions += govDeductions.ContainsKey("philhealth") ? govDeductions["philhealth"] : 0;
                totalDeductions += govDeductions.ContainsKey("pagibig") ? govDeductions["pagibig"] : 0;
                totalDeductions += govDeductions.ContainsKey("withholding_tax") ? govDeductions["withholding_tax"] : 0;
            }

            decimal netPay = grossPay - totalDeductions;
            return netPay > 0 ? netPay : 0; // Ensure non-negative net pay
        }
        private void ParseDateRange(string dateRange, out DateTime startDate, out DateTime endDate)
        {
            if (string.IsNullOrEmpty(dateRange))
            {
                // Default to current period
                DateTime now = DateTime.Now;
                if (now.Day <= 15)
                {
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = new DateTime(now.Year, now.Month, 15);
                }
                else
                {
                    startDate = new DateTime(now.Year, now.Month, 16);
                    endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                }
                return;
            }

            // Handle formats like "September 1 - 15, 2025" or "September 16 - 30, 2025"
            var match = Regex.Match(dateRange, @"(\w+) (\d+) - (\d+), (\d{4})");
            if (match.Success)
            {
                string month = match.Groups[1].Value;
                int startDay = int.Parse(match.Groups[2].Value);
                int endDay = int.Parse(match.Groups[3].Value);
                int year = int.Parse(match.Groups[4].Value);

                int monthNumber = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;
                startDate = new DateTime(year, monthNumber, startDay);
                endDate = new DateTime(year, monthNumber, endDay);
            }
            else
            {
                // Fallback to current period
                DateTime now = DateTime.Now;
                if (now.Day <= 15)
                {
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = new DateTime(now.Year, now.Month, 15);
                }
                else
                {
                    startDate = new DateTime(now.Year, now.Month, 16);
                    endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                }
            }
        }

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
                MessageBox.Show($"Error loading {childPath}: " + ex.Message);
            }
        }

        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();
            try
            {
                // Clean up the JSON string
                string cleanedJson = rawJson.Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"").Replace("(", "[").Replace(")", "]")
                    .Replace("[null,", "[").Replace("], [", ",").Replace("}, {", "},{")
                    .Replace("},(", "},{").Replace("),{", "},{");

                // Use regex to extract individual JSON objects
                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");
                foreach (Match match in matches)
                {
                    var record = new Dictionary<string, string>();
                    string objectStr = match.Value;

                    // Extract key-value pairs from each object
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
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }
            return records;
        }

        private void labelMoveLoan_Click(object sender, EventArgs e)
        {
            panelLoaderAdminLoan.LoadUserControl(new AdminLoan(AdminViewPanel, currentEmployeeId));
        }
        private Dictionary<string, decimal> GetEmployeeDailyRates()
        {
            var dailyRates = new Dictionary<string, decimal>();

            // From PayrollEarnings data in JSON
            var payrollEarnings = new Dictionary<string, decimal>
    {
        {"JAP-001", 500m},
        {"JAP-002", 644.23m},
        {"JAP-003", 644.23m},
        {"JAP-004", 0m}, // Not in current data - would need default
        {"JAP-005", 0m},
        {"JAP-006", 0m}
    };

            return dailyRates;
        }
    }
}