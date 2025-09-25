using DocumentFormat.OpenXml.Drawing.Charts;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        // Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string currentEmployeeId;
        private string payrollPeriod;

        // Search timer for delayed filtering
        private System.Threading.Timer searchTimer;

        public AdminPayroll(Panel targetPanel, string employeeId, string period = null)
        {
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();
            setTextBoxAttributes();

            // Add event handler for search textbox
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            // Load data from Firebase
            LoadFirebaseData();

            AdminViewPanel = targetPanel;
            panelLoaderAdminLoan = new AttributesClassAlt(AdminViewPanel);
        }

        // Search functionality implementation
        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            // Dispose of existing timer if any
            searchTimer?.Dispose();

            // If search is empty, show all rows immediately
            if (string.IsNullOrEmpty(textBoxSearchEmployee.Text) ||
                textBoxSearchEmployee.Text == "find employee")
            {
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
                return;
            }

            // Create a new timer that will trigger after 300ms of inactivity
            searchTimer = new System.Threading.Timer(_ =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    PerformSearch();
                });
            }, null, 300, System.Threading.Timeout.Infinite);
        }

        private void PerformSearch()
        {
            string searchText = textBoxSearchEmployee.Text.Trim();

            // If search is empty, show all rows
            if (string.IsNullOrEmpty(searchText) || searchText == "find employee")
            {
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
                return;
            }

            // Filter rows based on search text
            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (!row.IsNewRow)
                {
                    // Get values from relevant columns (only name, id, and department)
                    string employeeId = row.Cells["EmployeeId"].Value?.ToString() ?? "";
                    string fullName = row.Cells["FullName"].Value?.ToString() ?? "";
                    string department = row.Cells["Department"].Value?.ToString() ?? "";

                    // Check if any of the columns contain the search text (case-insensitive)
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

            // Setup columns
            dataGridViewEmployee.Columns.Clear();

            // Numbering column (without header text)
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
                FillWeight = 25
            };
            actionCol.Image = Properties.Resources.ExpandRight;
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

        private void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
            {
                // Get the selected employee ID from the grid
                string selectedEmployeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();

                if (!string.IsNullOrEmpty(selectedEmployeeId))
                {
                    Form parentForm = this.FindForm();
                    PayrollSummary payrollSummaryForm = new PayrollSummary(currentEmployeeId);

                    // Pass the selected employee ID to the form
                    payrollSummaryForm.SetEmployeeId(selectedEmployeeId);

                    AttributesClass.ShowWithOverlay(parentForm, payrollSummaryForm);
                }
            }
        }

        // Handle malformed JSON from Attendance
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
                    catch { continue; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }

            return records;
        }

        //  Government contributions & tax calculations (BI-MONTHLY)
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
        #endregion

        //  Load Firebase Data
        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();

                // Load all necessary data
                var employeeDetails = new Dictionary<string, dynamic>();
                var employmentInfo = new Dictionary<string, Dictionary<string, string>>();
                var payrollEarnings = new Dictionary<string, Dictionary<string, string>>();
                var loanDeductions = new Dictionary<string, Dictionary<string, string>>();
                var attendanceRecords = new List<Dictionary<string, string>>();
                var payrollData = new Dictionary<string, Dictionary<string, string>>();

                // EmployeeDetails
                var empDetails = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();
                foreach (var emp in empDetails)
                    employeeDetails[emp.Key] = emp.Object;

                // EmploymentInfo
                await LoadArrayBasedData("EmploymentInfo", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        employmentInfo[employeeId] = item;
                });

                // PayrollEarnings
                await LoadArrayBasedData("PayrollEarnings", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        payrollEarnings[payrollId] = item;
                });

                // LoansAndOtherDeductions
                await LoadArrayBasedData("LoansAndOtherDeductions", (item) =>
                {
                    var payrollId = item.ContainsKey("payroll_id") ? item["payroll_id"] : null;
                    if (!string.IsNullOrEmpty(payrollId))
                        loanDeductions[payrollId] = item;
                });

                // Attendance
                await LoadArrayBasedData("Attendance", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        attendanceRecords.Add(item);
                });

                // Payroll (for payroll_id per employee)
                await LoadArrayBasedData("Payroll", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        payrollData[employeeId] = item;
                });

                // Populate DataGrid
                int counter = 1;
                foreach (var empEntry in employeeDetails)
                {
                    string employeeId = empEntry.Key;
                    dynamic empData = empEntry.Value;

                    string fullName = $"{empData.first_name ?? ""} {empData.middle_name ?? ""} {empData.last_name ?? ""}".Trim();

                    string department = "";
                    string position = "";
                    decimal dailyRate = 0;
                    decimal monthlySalary = 0;

                    if (employmentInfo.ContainsKey(employeeId))
                    {
                        var empInfo = employmentInfo[employeeId];
                        department = empInfo.ContainsKey("department") ? empInfo["department"] : "";
                        position = empInfo.ContainsKey("position") ? empInfo["position"] : "";
                        decimal.TryParse(empInfo.ContainsKey("daily_rate") ? empInfo["daily_rate"] : "0", out dailyRate);
                        decimal.TryParse(empInfo.ContainsKey("monthly_salary") ? empInfo["monthly_salary"] : "0", out monthlySalary);
                    }

                    //  Calculate days worked and overtime
                    int daysWorked = 0;
                    decimal totalOvertime = 0;
                    foreach (var attendance in attendanceRecords)
                    {
                        if (attendance.ContainsKey("employee_id") && attendance["employee_id"] == employeeId &&
                            attendance.ContainsKey("status") && attendance["status"] != "Absent" &&
                            attendance.ContainsKey("time_in") && !string.IsNullOrEmpty(attendance["time_in"]))
                        {
                            daysWorked++;
                        }

                        if (attendance.ContainsKey("employee_id") && attendance["employee_id"] == employeeId &&
                            attendance.ContainsKey("overtime_hours") &&
                            decimal.TryParse(attendance["overtime_hours"], out decimal overtime))
                        {
                            totalOvertime += overtime;
                        }
                    }

                    //  Basic Pay (daily rate × days worked)
                    decimal basicPay = dailyRate * daysWorked;

                    // Overtime Pay
                    decimal overtimePay = CalculateOvertimePay(dailyRate, totalOvertime);

                    //  Get payroll ID
                    string payrollId = payrollData.ContainsKey(employeeId) ? payrollData[employeeId]["payroll_id"] : "";

                    // Compute Gross Pay (basic + overtime + allowances/earnings)
                    decimal grossPay = basicPay + overtimePay;

                    if (!string.IsNullOrEmpty(payrollId) && payrollEarnings.ContainsKey(payrollId))
                    {
                        var earnings = payrollEarnings[payrollId];
                        grossPay += earnings.ContainsKey("commission") ? decimal.Parse(earnings["commission"]) : 0;
                        grossPay += earnings.ContainsKey("communication") ? decimal.Parse(earnings["communication"]) : 0;
                        grossPay += earnings.ContainsKey("food_allowance") ? decimal.Parse(earnings["food_allowance"]) : 0;
                        grossPay += earnings.ContainsKey("gas_allowance") ? decimal.Parse(earnings["gas_allowance"]) : 0;
                        grossPay += earnings.ContainsKey("gondola") ? decimal.Parse(earnings["gondola"]) : 0;
                        grossPay += earnings.ContainsKey("incentives") ? decimal.Parse(earnings["incentives"]) : 0;
                    }

                    //  Compute deductions based on monthly salary
                    // Note: deduction functions already return SEMI-MONTHLY share (/2)
                    decimal computedMonthlySalary = dailyRate * 26; // assumes 26 working days in a month
                    decimal sss = CalculateSSSContribution(computedMonthlySalary);
                    decimal philhealth = CalculatePhilHealthContribution(computedMonthlySalary);
                    decimal pagibig = CalculatePagibigContribution(computedMonthlySalary);
                    decimal withholdingTax = CalculateWithholdingTax(computedMonthlySalary);

                    decimal totalDeductions = sss + philhealth + pagibig + withholdingTax;

                    // Add loan deductions (already cutoff-based)
                    if (!string.IsNullOrEmpty(payrollId) && loanDeductions.ContainsKey(payrollId))
                    {
                        var loan = loanDeductions[payrollId];
                        totalDeductions += loan.ContainsKey("car_loan") ? decimal.Parse(loan["car_loan"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("cash_advance") ? decimal.Parse(loan["cash_advance"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("coop_loan") ? decimal.Parse(loan["coop_loan"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("housing_loan") ? decimal.Parse(loan["housing_loan"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("sss_loan") ? decimal.Parse(loan["sss_loan"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("pagibig_loan") ? decimal.Parse(loan["pagibig_loan"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("coop_contribution") ? decimal.Parse(loan["coop_contribution"]) / 2 : 0;
                        totalDeductions += loan.ContainsKey("other_deduction") ? decimal.Parse(loan["other_deduction"]) / 2 : 0;
                    }

                    // Net Pay = Gross − Total Deductions
                    decimal netPay = grossPay - totalDeductions;

                    // Add row to grid
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Firebase data: " + ex.Message);
            }
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

        private void labelMoveLoan_Click(object sender, EventArgs e)
        {
            panelLoaderAdminLoan.LoadUserControl(new AdminLoan(AdminViewPanel, currentEmployeeId));
        }
    }
}