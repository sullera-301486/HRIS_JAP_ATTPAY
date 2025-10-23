using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class EmployeeSalaryDetailsEdit : Form
    {
        private readonly string employeeId;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public EmployeeSalaryDetailsEdit(string employeeId)
        {
            InitializeComponent();
            this.employeeId = employeeId;

            // Wire up event handlers
            tBDailyRateInput.TextChanged += tBDailyRateInput_TextChanged;
            cBDepartmentInput.SelectedIndexChanged += cBDepartmentInput_SelectedIndexChanged;

            LoadEmployeeSalaryList();
            ConfigureFlowLayoutPanel();
            SetFont();
            GetPayrollEarningsDirect();
            
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await InitializeDepartmentComboBox();
            await LoadCurrentSalaryInformation();
            await LoadSalaryHistory();
        }

        private async Task InitializeDepartmentComboBox()
        {
            try
            {
                cBDepartmentInput.Items.Clear();
                cBPositionInput.Items.Clear();

                // Get departments from Firebase
                var departments = await GetDepartmentsFromFirebase();

                if (departments != null && departments.Any())
                {
                    foreach (var department in departments)
                    {
                        if (!string.IsNullOrEmpty(department))
                        {
                            cBDepartmentInput.Items.Add(department);
                        }
                    }
                }
                else
                {
                    // Fallback departments
                    string[] defaultDepartments = {
                        "Engineering",
                        "Purchasing",
                        "Operations",
                        "Finance",
                        "Human Resource"
                    };

                    cBDepartmentInput.Items.AddRange(defaultDepartments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message, "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetDepartmentsFromFirebase()
        {
            try
            {
                var departments = new List<string>();

                // Method 1: Try to get from a dedicated Departments node
                var departmentData = await firebase
                    .Child("Departments")
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (departmentData != null)
                {
                    foreach (var dept in departmentData.Values)
                    {
                        if (dept != null && !string.IsNullOrEmpty(dept.ToString()))
                        {
                            departments.Add(dept.ToString());
                        }
                    }
                }

                // Method 2: If no dedicated departments node, extract from EmploymentInfo
                if (!departments.Any())
                {
                    var employmentInfo = await firebase
                        .Child("EmploymentInfo")
                        .OnceAsync<Dictionary<string, object>>();

                    if (employmentInfo != null)
                    {
                        foreach (var emp in employmentInfo)
                        {
                            if (emp.Object != null)
                            {
                                var department = emp.Object.GetValueOrDefault("department")?.ToString();
                                if (!string.IsNullOrEmpty(department) && !departments.Contains(department))
                                {
                                    departments.Add(department);
                                }
                            }
                        }
                    }
                }

                // Method 3: Try array format for EmploymentInfo
                if (!departments.Any())
                {
                    try
                    {
                        var employmentArray = await firebase
                            .Child("EmploymentInfo")
                            .OnceSingleAsync<JArray>();

                        if (employmentArray != null)
                        {
                            foreach (var item in employmentArray)
                            {
                                if (item?.Type == JTokenType.Null) continue;

                                if (item?.Type == JTokenType.Object)
                                {
                                    var empObj = (JObject)item;
                                    var department = empObj["department"]?.ToString();
                                    if (!string.IsNullOrEmpty(department) && !departments.Contains(department))
                                    {
                                        departments.Add(department);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore array format errors
                    }
                }

                return departments.Distinct().ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting departments from Firebase: {ex.Message}");
                return new List<string>();
            }
        }

        private async Task LoadCurrentSalaryInformation()
        {
            try
            {
                // Load basic employee info
                var employeeDetails = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (employeeDetails != null)
                {
                    SafeSetLabelText(lblEmployeeIDInput, employeeDetails.GetValueOrDefault("employee_id")?.ToString());
                    SafeSetLabelText(lblNameInput, $"{employeeDetails.GetValueOrDefault("first_name")} {employeeDetails.GetValueOrDefault("last_name")}");
                }

                // Load payroll earnings
                var payrollEarnings = await GetPayrollEarningsDirect();
                if (payrollEarnings != null)
                {
                    // Use BasicPay as salary if available, otherwise calculate from daily rate
                    decimal basicPay = payrollEarnings.BasicPay > 0 ? payrollEarnings.BasicPay :
                                     payrollEarnings.DailyRate > 0 ? payrollEarnings.DailyRate * 22 : 0;

                    SafeSetTextBoxText(tBDailyRateInput, payrollEarnings.DailyRate.ToString("0.00"));
                    SafeSetLabelText(lblSalaryInput, basicPay.ToString("0.00"));
                }

                // Load employment info for department and position
                await LoadEmploymentInfoDirect();

                // Set current date as effectivity date
                SafeSetTextBoxText(tBEffectivityDateInput, DateTime.Now.ToString("MM-dd-yyyy"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading current salary information: " + ex.Message);
            }
        }

        private async Task<EmployeeSalaryDetails.PayrollEarnings> GetPayrollEarnings()
        {
            try
            {
                // Method 1: Try to get from PayrollEarnings directly
                var allEarnings = await firebase.Child("PayrollEarnings").OnceAsync<Dictionary<string, object>>();

                foreach (var earningRecord in allEarnings)
                {
                    var earningData = earningRecord.Object;
                    var payrollId = earningData.GetValueOrDefault("payroll_id")?.ToString();

                    if (!string.IsNullOrEmpty(payrollId))
                    {
                        var payrollData = await firebase.Child("Payroll").Child(payrollId).OnceSingleAsync<Dictionary<string, object>>();
                        if (payrollData != null && payrollData.GetValueOrDefault("employee_id")?.ToString() == employeeId)
                        {
                            return new EmployeeSalaryDetails.PayrollEarnings
                            {
                                DailyRate = decimal.TryParse(earningData.GetValueOrDefault("daily_rate")?.ToString(), out decimal rate) ? rate : 0,
                                BasicPay = decimal.TryParse(earningData.GetValueOrDefault("basic_pay")?.ToString(), out decimal basic) ? basic : 0
                            };
                        }
                    }
                }

                // Method 2: Try PayrollData -> PayrollEarnings lookup
                var payrollDataRecords = await firebase.Child("PayrollData").OnceAsync<Dictionary<string, object>>();
                foreach (var payrollRecord in payrollDataRecords)
                {
                    var payroll = payrollRecord.Object;
                    if (payroll.GetValueOrDefault("employee_id")?.ToString() == employeeId)
                    {
                        var payrollId = payroll.GetValueOrDefault("payroll_id")?.ToString();
                        if (!string.IsNullOrEmpty(payrollId))
                        {
                            var earningsData = await firebase
                                .Child("PayrollEarnings")
                                .Child(payrollId)
                                .OnceSingleAsync<Dictionary<string, object>>();

                            if (earningsData != null)
                            {
                                return new EmployeeSalaryDetails.PayrollEarnings
                                {
                                    DailyRate = decimal.TryParse(earningsData.GetValueOrDefault("daily_rate")?.ToString(), out decimal rate) ? rate : 0,
                                    BasicPay = decimal.TryParse(earningsData.GetValueOrDefault("basic_pay")?.ToString(), out decimal basic) ? basic : 0
                                };
                            }
                        }
                    }
                }

                // Default values if not found
                return new EmployeeSalaryDetails.PayrollEarnings { DailyRate = 400m, BasicPay = 8800m };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading payroll earnings: {ex.Message}");
                return new EmployeeSalaryDetails.PayrollEarnings { DailyRate = 400m, BasicPay = 8800m };
            }
        }

        private async Task LoadEmploymentInfo()
        {
            try
            {
                var employmentInfo = await GetEmploymentInfo();

                if (employmentInfo != null)
                {
                    // Set department combo box - add if not exists and select
                    if (!string.IsNullOrEmpty(employmentInfo.Department))
                    {
                        if (!cBDepartmentInput.Items.Contains(employmentInfo.Department))
                        {
                            cBDepartmentInput.Items.Add(employmentInfo.Department);
                        }
                        cBDepartmentInput.SelectedItem = employmentInfo.Department;
                    }

                    // Load positions based on selected department
                    await LoadPositionsForDepartment(employmentInfo.Department);

                    // Set position combo box
                    if (!string.IsNullOrEmpty(employmentInfo.Position))
                    {
                        if (!cBPositionInput.Items.Contains(employmentInfo.Position))
                        {
                            cBPositionInput.Items.Add(employmentInfo.Position);
                        }
                        cBPositionInput.SelectedItem = employmentInfo.Position;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employment information: " + ex.Message);
            }
        }

        private async Task LoadPositionsForDepartment(string department)
        {
            try
            {
                cBPositionInput.Items.Clear();

                if (string.IsNullOrEmpty(department))
                    return;

                var positions = await GetPositionsForDepartment(department);

                if (positions.Any())
                {
                    foreach (var position in positions)
                    {
                        if (!string.IsNullOrEmpty(position))
                        {
                            cBPositionInput.Items.Add(position);
                        }
                    }
                }
                else
                {
                    // Add default positions based on department
                    var defaultPositions = GetDefaultPositionsForDepartment(department);
                    cBPositionInput.Items.AddRange(defaultPositions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading positions: {ex.Message}");
            }
        }

        private async Task<List<string>> GetPositionsForDepartment(string department)
        {
            try
            {
                var positions = new List<string>();

                // Get positions from EmploymentInfo for this department
                var employmentInfo = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<Dictionary<string, object>>();

                if (employmentInfo != null)
                {
                    foreach (var emp in employmentInfo)
                    {
                        if (emp.Object != null)
                        {
                            var empDepartment = emp.Object.GetValueOrDefault("department")?.ToString();
                            var position = emp.Object.GetValueOrDefault("position")?.ToString();

                            if (empDepartment == department && !string.IsNullOrEmpty(position) && !positions.Contains(position))
                            {
                                positions.Add(position);
                            }
                        }
                    }
                }

                // Try array format
                try
                {
                    var employmentArray = await firebase
                        .Child("EmploymentInfo")
                        .OnceSingleAsync<JArray>();

                    if (employmentArray != null)
                    {
                        foreach (var item in employmentArray)
                        {
                            if (item?.Type == JTokenType.Null) continue;

                            if (item?.Type == JTokenType.Object)
                            {
                                var empObj = (JObject)item;
                                var empDepartment = empObj["department"]?.ToString();
                                var position = empObj["position"]?.ToString();

                                if (empDepartment == department && !string.IsNullOrEmpty(position) && !positions.Contains(position))
                                {
                                    positions.Add(position);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore array errors
                }

                return positions.Distinct().ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting positions: {ex.Message}");
                return new List<string>();
            }
        }

        private string[] GetDefaultPositionsForDepartment(string department)
        {
            switch (department?.ToLower())
            {
                case "human resource":
                    return new[] { "HR Manager", "HR Specialist", "Recruiter", "HR Assistant", "Staff" };
                case "engineering":
                    return new[] { "Senior Engineer", "Engineer", "Junior Engineer", "Team Lead", "Supervisor" };
                case "finance":
                    return new[] { "Finance Manager", "Accountant", "Financial Analyst", "Cashier", "Staff" };
                case "purchasing":
                    return new[] { "Purchasing Manager", "Buyer", "Procurement Specialist", "Assistant", "Staff" };
                case "operations":
                    return new[] { "Operations Manager", "Supervisor", "Coordinator", "Operator", "Staff" };
                default:
                    return new[] { "Manager", "Supervisor", "Specialist", "Assistant", "Staff" };
            }
        }

        private async Task<EmployeeSalaryDetails.EmploymentInfoData> GetEmploymentInfo()
        {
            try
            {
                // Try array-based approach first
                var employmentArray = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<JArray>();

                if (employmentArray != null)
                {
                    foreach (var item in employmentArray)
                    {
                        if (item?.Type == JTokenType.Null) continue;

                        if (item?.Type == JTokenType.Object)
                        {
                            var empObj = (JObject)item;
                            var empId = empObj["employee_id"]?.ToString();

                            if (empId == employeeId)
                            {
                                return new EmployeeSalaryDetails.EmploymentInfoData
                                {
                                    Department = empObj["department"]?.ToString(),
                                    Position = empObj["position"]?.ToString()
                                };
                            }
                        }
                    }
                }

                // Fallback to keyed collection
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<Dictionary<string, object>>();

                if (employmentData != null)
                {
                    foreach (var item in employmentData)
                    {
                        if (item?.Object == null) continue;

                        try
                        {
                            var empId = item.Object.GetValueOrDefault("employee_id")?.ToString();

                            if (empId == employeeId)
                            {
                                return new EmployeeSalaryDetails.EmploymentInfoData
                                {
                                    Department = item.Object.GetValueOrDefault("department")?.ToString(),
                                    Position = item.Object.GetValueOrDefault("position")?.ToString()
                                };
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employment info: {ex.Message}");
            }

            return new EmployeeSalaryDetails.EmploymentInfoData();
        }

        private void SafeSetTextBoxText(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
                textBox.Invoke(new Action(() => textBox.Text = text ?? ""));
            else
                textBox.Text = text ?? "";
        }

        private void SafeSetLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
                label.Invoke(new Action(() => label.Text = text ?? ""));
            else
                label.Text = text ?? "";
        }

        private void SetFont()
        {
            try
            {
                lblEmployeeSalaryDetails.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                lblSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                comboBoxNamesSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                lblCurrentInformations.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                lblPreviousSalaryHistory.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                lblNameInput.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                lblEffectivityDate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblPosition.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblSalary.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblEmployeeIDInput.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                tBEffectivityDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                cBDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                cBPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                tBDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);

                buttonUpdate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                lblEffectivityDatePSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDepartmentPSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblPositionPSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDailyRatePSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblSalaryPSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void ConfigureFlowLayoutPanel()
        {
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Padding = new Padding(0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.AutoSize = false;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void LoadEmployeeSalaryList()
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                var salaryData = new List<(string Date, string Dept, string Pos, string Rate, string Salary)>
                {
                    ("9-15-2025", "Human Resource", "Staff", "50.00", "750.00"),
                    ("8-15-2025", "IT", "Staff", "20.00", "300.00"),
                    ("7-15-2025", "Finance", "Cashier", "100.00", "1500.00"),
                    ("6-10-2025", "Engineering", "Supervisor", "200.00", "3000.00")
                };

                foreach (var data in salaryData)
                {
                    EmployeeSalaryDetailsList item = new EmployeeSalaryDetailsList
                    {
                        DateValue = data.Date,
                        DepartmentValue = data.Dept,
                        PositionValue = data.Pos,
                        DailyRateValue = data.Rate,
                        SalaryValue = data.Salary,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        Height = 24
                    };

                    flowLayoutPanel1.Controls.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load salary list: " + ex.Message);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e) { }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(cBDepartmentInput.SelectedItem?.ToString()) ||
                    string.IsNullOrEmpty(cBPositionInput.SelectedItem?.ToString()) ||
                    string.IsNullOrEmpty(tBDailyRateInput.Text) ||
                    string.IsNullOrEmpty(tBEffectivityDateInput.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(tBDailyRateInput.Text, out decimal dailyRate) || dailyRate <= 0)
                {
                    MessageBox.Show("Please enter a valid daily rate.", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Use the salary value directly (no calculation)
                if (!decimal.TryParse(lblSalaryInput.Text, out decimal salary) || salary <= 0)
                {
                    MessageBox.Show("Please enter a valid salary amount.", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Form parentForm = this.FindForm();
                ConfirmEditSalary confirmEditSalaryForm = new ConfirmEditSalary(
                    employeeId,
                    tBEffectivityDateInput.Text,
                    cBDepartmentInput.SelectedItem.ToString(),
                    cBPositionInput.SelectedItem.ToString(),
                    dailyRate,
                    salary,
                    this
                );

                AttributesClass.ShowWithOverlay(parentForm, confirmEditSalaryForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing update: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async void RefreshSalaryHistory()
        {
            await LoadSalaryHistory();
        }
        private async Task LoadSalaryHistory()
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                // Load salary history from Firebase
                var salaryHistory = await GetSalaryHistoryFromFirebase();

                if (salaryHistory.Any())
                {
                    foreach (var record in salaryHistory)
                    {
                        AddHistoryItem(record);
                    }
                }
                else
                {
                    // If no history exists, create a record from current data
                    var currentDailyRate = decimal.TryParse(tBDailyRateInput.Text, out decimal rate) ? rate : 0;
                    var currentSalary = decimal.TryParse(lblSalaryInput.Text, out decimal salary) ? salary : 0;

                    var currentRecord = new SalaryHistoryRecord
                    {
                        EffectivityDate = DateTime.Now.ToString("MM-dd-yyyy"),
                        Department = cBDepartmentInput.SelectedItem?.ToString() ?? "Not set",
                        Position = cBPositionInput.SelectedItem?.ToString() ?? "Not set",
                        DailyRate = currentDailyRate,
                        MonthlySalary = currentSalary
                    };

                    AddHistoryItem(currentRecord);

                    // Also show a message that this is current data, not historical
                    Label infoLabel = new Label
                    {
                        Text = "No salary history found. Showing current salary information.",
                        ForeColor = Color.Gray,
                        Font = new Font("Roboto", 9f, FontStyle.Italic),
                        AutoSize = true,
                        Margin = new Padding(5)
                    };

                    if (flowLayoutPanel1.InvokeRequired)
                        flowLayoutPanel1.Invoke(new Action(() => flowLayoutPanel1.Controls.Add(infoLabel)));
                    else
                        flowLayoutPanel1.Controls.Add(infoLabel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load salary history: " + ex.Message);

                // Add error item to flow layout
                Label errorLabel = new Label
                {
                    Text = "Error loading salary history",
                    ForeColor = Color.Red,
                    Font = new Font("Roboto", 9f, FontStyle.Regular),
                    AutoSize = true,
                    Margin = new Padding(5)
                };

                if (flowLayoutPanel1.InvokeRequired)
                    flowLayoutPanel1.Invoke(new Action(() => flowLayoutPanel1.Controls.Add(errorLabel)));
                else
                    flowLayoutPanel1.Controls.Add(errorLabel);
            }
        }

        private async Task<List<SalaryHistoryRecord>> GetSalaryHistoryFromFirebase()
        {
            try
            {
                var historyRecords = new List<SalaryHistoryRecord>();

                // Try to get salary history for this employee
                var historyData = await firebase
                    .Child("SalaryHistory")
                    .Child(employeeId)
                    .OnceAsync<Dictionary<string, object>>();

                if (historyData != null)
                {
                    foreach (var record in historyData)
                    {
                        var recordData = record.Object;
                        if (recordData != null)
                        {
                            historyRecords.Add(new SalaryHistoryRecord
                            {
                                EffectivityDate = recordData.GetValueOrDefault("effectivity_date")?.ToString() ??
                                                recordData.GetValueOrDefault("date")?.ToString() ?? "N/A",
                                Department = recordData.GetValueOrDefault("previous_department")?.ToString() ??
                                           recordData.GetValueOrDefault("department")?.ToString() ?? "N/A",
                                Position = recordData.GetValueOrDefault("previous_position")?.ToString() ??
                                         recordData.GetValueOrDefault("position")?.ToString() ?? "N/A",
                                DailyRate = decimal.TryParse(recordData.GetValueOrDefault("previous_daily_rate")?.ToString(), out decimal rate) ? rate :
                                          decimal.TryParse(recordData.GetValueOrDefault("daily_rate")?.ToString(), out rate) ? rate : 0,
                                MonthlySalary = decimal.TryParse(recordData.GetValueOrDefault("previous_salary")?.ToString(), out decimal salary) ? salary :
                                              decimal.TryParse(recordData.GetValueOrDefault("salary")?.ToString(), out salary) ? salary :
                                              decimal.TryParse(recordData.GetValueOrDefault("basic_pay")?.ToString(), out salary) ? salary : 0
                            });
                        }
                    }
                }

                // Sort by effectivity date descending (newest first)
                return historyRecords.OrderByDescending(r =>
                {
                    if (DateTime.TryParse(r.EffectivityDate, out DateTime date))
                        return date;
                    return DateTime.MinValue;
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading salary history: {ex.Message}");
                return new List<SalaryHistoryRecord>();
            }
        }
        private void tBDailyRateInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (decimal.TryParse(tBDailyRateInput.Text, out decimal dailyRate) && dailyRate > 0)
                {
                    // Calculate monthly salary (daily rate × 22 working days)
                    decimal monthlySalary = dailyRate * 22;
                    SafeSetLabelText(lblSalaryInput, monthlySalary.ToString("0.00"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating salary: {ex.Message}");
            }
        }

        private void AddHistoryItem(SalaryHistoryRecord record)
        {
            EmployeeSalaryDetailsList item = new EmployeeSalaryDetailsList
            {
                DateValue = record.EffectivityDate,
                DepartmentValue = record.Department,
                PositionValue = record.Position,
                DailyRateValue = record.DailyRate.ToString("0.00"),
                SalaryValue = record.MonthlySalary.ToString("0.00"),
                Margin = new Padding(0),
                Padding = new Padding(0),
                Height = 24
            };

            flowLayoutPanel1.Controls.Add(item);
        }

        // Event handler for department selection change
        private async void cBDepartmentInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDepartment = cBDepartmentInput.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedDepartment))
            {
                await LoadPositionsForDepartment(selectedDepartment);
            }
        }
        private async Task<EmployeeSalaryDetails.EmploymentInfoData> GetEmploymentInfoDirect()
        {
            try
            {
                // Method 1: Try reading as JArray first (handles the array structure with null element)
                try
                {
                    var employmentArray = await firebase
                        .Child("EmploymentInfo")
                        .OnceSingleAsync<JArray>();

                    if (employmentArray != null)
                    {
                        foreach (var item in employmentArray)
                        {
                            // Skip null elements
                            if (item?.Type == JTokenType.Null)
                                continue;

                            if (item?.Type == JTokenType.Object)
                            {
                                var empObj = (JObject)item;
                                var empId = empObj["employee_id"]?.ToString();

                                if (empId == employeeId)
                                {
                                    return new EmployeeSalaryDetails.EmploymentInfoData
                                    {
                                        Department = empObj["department"]?.ToString(),
                                        Position = empObj["position"]?.ToString()
                                    };
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fall through to method 2 if array approach fails
                }

                // Method 2: Try as keyed collection (backward compatibility)
                try
                {
                    var employmentData = await firebase
                        .Child("EmploymentInfo")
                        .OnceAsync<object>();

                    if (employmentData != null)
                    {
                        foreach (var item in employmentData)
                        {
                            // Skip null items
                            if (item?.Object == null)
                                continue;

                            try
                            {
                                var empObj = JObject.FromObject(item.Object);
                                var empId = empObj["employee_id"]?.ToString();

                                if (empId == employeeId)
                                {
                                    return new EmployeeSalaryDetails.EmploymentInfoData
                                    {
                                        Department = empObj["department"]?.ToString(),
                                        Position = empObj["position"]?.ToString()
                                    };
                                }
                            }
                            catch
                            {
                                // Skip items that can't be converted to JObject
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in keyed collection method: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employment info: {ex.Message}");
            }

            return new EmployeeSalaryDetails.EmploymentInfoData();
        }
        private async Task LoadEmploymentInfoDirect()
        {
            try
            {
                var employmentInfo = await GetEmploymentInfoDirect();

                if (employmentInfo != null)
                {
                    // Set department combo box
                    if (!string.IsNullOrEmpty(employmentInfo.Department))
                    {
                        // Add to combobox if not already there
                        if (!cBDepartmentInput.Items.Contains(employmentInfo.Department))
                        {
                            cBDepartmentInput.Items.Add(employmentInfo.Department);
                        }
                        // Set selected item
                        if (cBDepartmentInput.InvokeRequired)
                            cBDepartmentInput.Invoke(new Action(() => cBDepartmentInput.SelectedItem = employmentInfo.Department));
                        else
                            cBDepartmentInput.SelectedItem = employmentInfo.Department;
                    }

                    // Load positions based on selected department
                    if (!string.IsNullOrEmpty(employmentInfo.Department))
                    {
                        await LoadPositionsForDepartment(employmentInfo.Department);
                    }

                    // Set position combo box after positions are loaded
                    if (!string.IsNullOrEmpty(employmentInfo.Position))
                    {
                        // Add delay to ensure positions are loaded first
                        await Task.Delay(100);

                        if (!cBPositionInput.Items.Contains(employmentInfo.Position))
                        {
                            cBPositionInput.Items.Add(employmentInfo.Position);
                        }
                        // Set selected item
                        if (cBPositionInput.InvokeRequired)
                            cBPositionInput.Invoke(new Action(() => cBPositionInput.SelectedItem = employmentInfo.Position));
                        else
                            cBPositionInput.SelectedItem = employmentInfo.Position;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employment information: " + ex.Message);
            }
        }
        private void SafeSetComboBoxSelected(ComboBox comboBox, string value)
        {
            if (comboBox.InvokeRequired)
                comboBox.Invoke(new Action(() => comboBox.SelectedItem = value));
            else
                comboBox.SelectedItem = value;
        }
        private async Task<EmployeeSalaryDetails.PayrollEarnings> GetPayrollEarningsDirect()
        {
            try
            {
                // Method 1: Try reading PayrollEarnings as JArray first
                try
                {
                    var earningsArray = await firebase
                        .Child("PayrollEarnings")
                        .OnceSingleAsync<JArray>();

                    if (earningsArray != null)
                    {
                        foreach (var item in earningsArray)
                        {
                            if (item?.Type == JTokenType.Null)
                                continue;

                            if (item?.Type == JTokenType.Object)
                            {
                                var earningsObj = (JObject)item;
                                var payrollId = earningsObj["payroll_id"]?.ToString();

                                if (!string.IsNullOrEmpty(payrollId))
                                {
                                    // Check if this payroll belongs to current employee
                                    var payrollData = await GetPayrollDataByPayrollId(payrollId);
                                    if (payrollData?.GetValueOrDefault("employee_id")?.ToString() == employeeId)
                                    {
                                        return ExtractEarningsFromObject(earningsObj);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fall through to method 2
                }

                // Method 2: Try as keyed collection
                try
                {
                    var earningsData = await firebase
                        .Child("PayrollEarnings")
                        .OnceAsync<object>();

                    if (earningsData != null)
                    {
                        foreach (var item in earningsData)
                        {
                            if (item?.Object == null)
                                continue;

                            try
                            {
                                var earningsObj = JObject.FromObject(item.Object);
                                var payrollId = earningsObj["payroll_id"]?.ToString();

                                if (!string.IsNullOrEmpty(payrollId))
                                {
                                    var payrollData = await GetPayrollDataByPayrollId(payrollId);
                                    if (payrollData?.GetValueOrDefault("employee_id")?.ToString() == employeeId)
                                    {
                                        return ExtractEarningsFromObject(earningsObj);
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in keyed collection method: {ex.Message}");
                }

                // Return defaults if no data found
                return await GetDefaultEarnings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payroll earnings: {ex.Message}");
                return await GetDefaultEarnings();
            }
        }
        private async Task<Dictionary<string, object>> GetPayrollDataByPayrollId(string payrollId)
        {
            try
            {
                // Try array format first
                try
                {
                    var payrollArray = await firebase
                        .Child("PayrollData")
                        .OnceSingleAsync<JArray>();

                    if (payrollArray != null)
                    {
                        foreach (var item in payrollArray)
                        {
                            if (item?.Type == JTokenType.Null)
                                continue;

                            if (item?.Type == JTokenType.Object)
                            {
                                var payrollObj = (JObject)item;
                                if (payrollObj["payroll_id"]?.ToString() == payrollId)
                                {
                                    return payrollObj.ToObject<Dictionary<string, object>>();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fall through to keyed collection
                }

                // Try keyed collection - FIXED VERSION
                var payrollData = await firebase
                    .Child("PayrollData")
                    .OnceAsync<Dictionary<string, object>>();

                if (payrollData != null)
                {
                    foreach (var payrollItem in payrollData)
                    {
                        if (payrollItem?.Object != null)
                        {
                            var payrollIdFromData = payrollItem.Object.GetValueOrDefault("payroll_id")?.ToString();
                            if (payrollIdFromData == payrollId)
                            {
                                return payrollItem.Object;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payroll data: {ex.Message}");
                return null;
            }
        }
        

        private EmployeeSalaryDetails.PayrollEarnings ExtractEarningsFromObject(JObject earningsObj)
        {
            return new EmployeeSalaryDetails.PayrollEarnings
            {
                DailyRate = decimal.TryParse(earningsObj["daily_rate"]?.ToString(), out decimal rate) ? rate : 0,
                BasicPay = decimal.TryParse(earningsObj["basic_pay"]?.ToString(), out decimal basic) ? basic : 0,
                Commission = decimal.TryParse(earningsObj["commission"]?.ToString(), out decimal comm) ? comm : 0,
                Incentives = decimal.TryParse(earningsObj["incentives"]?.ToString(), out decimal inc) ? inc : 0
            };
        }
        private async Task<EmployeeSalaryDetails.PayrollEarnings> GetDefaultEarnings()
        {
            var earnings = new EmployeeSalaryDetails.PayrollEarnings();

            try
            {
                // Load employment info to determine appropriate defaults
                var employmentInfo = await GetEmploymentInfoDirect();

                if (employmentInfo != null)
                {
                    // Set different defaults based on department/position
                    string department = employmentInfo.Department ?? "";
                    string position = employmentInfo.Position ?? "";

                    if (department.Equals("Human Resource", StringComparison.OrdinalIgnoreCase))
                    {
                        earnings.DailyRate = 500m;
                        earnings.BasicPay = 11000m; // Monthly equivalent
                    }
                    else if (department.Equals("Finance", StringComparison.OrdinalIgnoreCase))
                    {
                        earnings.DailyRate = 450m;
                        earnings.BasicPay = 9900m;
                    }
                    else if (department.Equals("Engineering", StringComparison.OrdinalIgnoreCase))
                    {
                        earnings.DailyRate = 600m;
                        earnings.BasicPay = 13200m;
                    }
                    else
                    {
                        // Default values
                        earnings.DailyRate = 400m;
                        earnings.BasicPay = 8800m;
                    }
                }
                else
                {
                    // Fallback defaults
                    earnings.DailyRate = 400m;
                    earnings.BasicPay = 8800m;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting default earnings: {ex.Message}");
                earnings.DailyRate = 400m;
                earnings.BasicPay = 8800m;
            }

            return earnings;
        }

    }
    public class SalaryHistoryRecord
    {
        public string EffectivityDate { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public decimal DailyRate { get; set; }
        public decimal MonthlySalary { get; set; }
        public string EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}