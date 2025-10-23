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
    public partial class EmployeeSalaryDetails : Form
    {
        private readonly string employeeId;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private List<SalaryHistoryRecord> allSalaryHistory;

        public EmployeeSalaryDetails(string employeeId)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            SetFont();
            ConfigureFlowLayoutPanel();

            // Wire up the event handler for date selection
            comboBoxNamesSelected.SelectedIndexChanged += ComboBoxNamesSelected_SelectedIndexChanged;
            comboBoxNamesSelected.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            await LoadEmployeeSalaryData();
        }

        private async Task LoadEmployeeSalaryData()
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show("Employee ID is missing.");
                    return;
                }

                // Load salary history first
                allSalaryHistory = await GetSalaryHistoryFromFirebase();

                // Load dates into combo box
                await LoadEffectivityDatesIntoComboBox();

                // Load the history list
                await LoadSalaryHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load employee salary data: " + ex.Message);
            }
        }

        private async Task LoadEffectivityDatesIntoComboBox()
        {
            try
            {
                comboBoxNamesSelected.Items.Clear();

                if (allSalaryHistory != null && allSalaryHistory.Any())
                {
                    // Add all effectivity dates to combo box
                    foreach (var record in allSalaryHistory)
                    {
                        comboBoxNamesSelected.Items.Add(record.EffectivityDate);
                    }

                    // Select the most recent date (first item)
                    if (comboBoxNamesSelected.Items.Count > 0)
                    {
                        comboBoxNamesSelected.SelectedIndex = 0;
                    }
                }
                else
                {
                    // No history found, add current date
                    comboBoxNamesSelected.Items.Add(DateTime.Now.ToString("MM-dd-yyyy"));
                    comboBoxNamesSelected.SelectedIndex = 0;

                    // Load current salary information
                    await LoadCurrentSalaryInformation();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dates into combo box: {ex.Message}");
            }
        }

        private void ComboBoxNamesSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxNamesSelected.SelectedItem == null)
                    return;

                string selectedDate = comboBoxNamesSelected.SelectedItem.ToString();

                // Find the matching salary record
                var selectedRecord = allSalaryHistory?.FirstOrDefault(r => r.EffectivityDate == selectedDate);

                if (selectedRecord != null)
                {
                    // Load the selected record's information
                    LoadSalaryRecordIntoCurrentInfo(selectedRecord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading selected salary record: " + ex.Message);
            }
        }

        private void LoadSalaryRecordIntoCurrentInfo(SalaryHistoryRecord record)
        {
            try
            {
                SafeSetLabelText(lblEmployeeIDInput, employeeId);
                SafeSetLabelText(lblEffectivityDateInput, record.EffectivityDate);
                SafeSetLabelText(lblDepartmentInput, record.Department);
                SafeSetLabelText(lblPositionInput, record.Position);
                SafeSetLabelText(lblDailyRateInput, record.DailyRate.ToString("0.00"));
                SafeSetLabelText(lblSalaryInput, record.MonthlySalary.ToString("0.00"));

                // Also load the employee name if available
                LoadEmployeeNameLabel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading record into current info: {ex.Message}");
            }
        }

        private async void LoadEmployeeNameLabel()
        {
            try
            {
                var employeeDetails = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (employeeDetails != null)
                {
                    string firstName = employeeDetails.GetValueOrDefault("first_name")?.ToString() ?? "";
                    string lastName = employeeDetails.GetValueOrDefault("last_name")?.ToString() ?? "";
                    SafeSetLabelText(lblNameInput, $"{firstName} {lastName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee name: {ex.Message}");
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
                    SafeSetLabelText(lblEmployeeIDInput, employeeDetails.TryGetValue("employee_id", out var empId) ? empId?.ToString() : "");
                    SafeSetLabelText(lblNameInput, $"{employeeDetails.GetValueOrDefault("first_name")} {employeeDetails.GetValueOrDefault("last_name")}");
                }

                // Load payroll earnings to get basic pay (salary)
                var payrollEarnings = await GetPayrollEarnings();
                if (payrollEarnings != null)
                {
                    // Use BasicPay as the salary - similar to PayrollSummary form
                    decimal basicPay = payrollEarnings.BasicPay > 0 ? payrollEarnings.BasicPay :
                                     payrollEarnings.DailyRate > 0 ? payrollEarnings.DailyRate * 22 : 0; // Default calculation

                    SafeSetLabelText(lblSalaryInput, basicPay.ToString("0.00"));
                    SafeSetLabelText(lblDailyRateInput, payrollEarnings.DailyRate.ToString("0.00"));
                }

                // Load employment info for department and position - USING THE SAME APPROACH AS EMPLOYEE PROFILE
                await LoadEmploymentInfoDirect();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading current salary information: " + ex.Message);
            }
        }

        private async Task<PayrollEarnings> GetPayrollEarnings()
        {
            try
            {
                // Method 1: Try to get payroll data from the employee's payroll_data node
                var payrollData = await firebase
                    .Child("PayrollData")
                    .Child(employeeId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (payrollData != null)
                {
                    // Get the payroll ID from the employee's payroll data
                    var payrollId = payrollData.GetValueOrDefault("payroll_id")?.ToString();

                    if (!string.IsNullOrEmpty(payrollId))
                    {
                        // Now get the earnings using this payroll ID
                        var earningsData = await firebase
                            .Child("PayrollEarnings")
                            .Child(payrollId)
                            .OnceSingleAsync<Dictionary<string, object>>();

                        if (earningsData != null)
                        {
                            return new PayrollEarnings
                            {
                                DailyRate = decimal.TryParse(earningsData.GetValueOrDefault("daily_rate")?.ToString(), out decimal rate) ? rate : 0,
                                BasicPay = decimal.TryParse(earningsData.GetValueOrDefault("basic_pay")?.ToString(), out decimal basic) ? basic : 0,
                                Commission = decimal.TryParse(earningsData.GetValueOrDefault("commission")?.ToString(), out decimal comm) ? comm : 0,
                                Incentives = decimal.TryParse(earningsData.GetValueOrDefault("incentives")?.ToString(), out decimal inc) ? inc : 0
                            };
                        }
                    }
                }

                // Method 2: Try direct lookup in PayrollEarnings by scanning all records
                var allEarnings = await firebase.Child("PayrollEarnings").OnceAsync<Dictionary<string, object>>();

                foreach (var earningRecord in allEarnings)
                {
                    var earningData = earningRecord.Object;
                    var earningPayrollId = earningRecord.Key; // Use the key as payroll ID

                    // Check if this payroll belongs to current employee by looking up in PayrollData
                    var correspondingPayroll = await firebase
                        .Child("PayrollData")
                        .OnceAsync<Dictionary<string, object>>();

                    foreach (var payrollRecord in correspondingPayroll)
                    {
                        var payroll = payrollRecord.Object;
                        if (payroll.GetValueOrDefault("employee_id")?.ToString() == employeeId &&
                            payrollRecord.Key == earningPayrollId)
                        {
                            return new PayrollEarnings
                            {
                                DailyRate = decimal.TryParse(earningData.GetValueOrDefault("daily_rate")?.ToString(), out decimal rate) ? rate : 0,
                                BasicPay = decimal.TryParse(earningData.GetValueOrDefault("basic_pay")?.ToString(), out decimal basic) ? basic : 0,
                                Commission = decimal.TryParse(earningData.GetValueOrDefault("commission")?.ToString(), out decimal comm) ? comm : 0,
                                Incentives = decimal.TryParse(earningData.GetValueOrDefault("incentives")?.ToString(), out decimal inc) ? inc : 0
                            };
                        }
                    }
                }

                // If no earnings found, return defaults
                return await GetDefaultEarnings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading payroll earnings: {ex.Message}");
                return await GetDefaultEarnings();
            }
        }

        private async Task<PayrollEarnings> GetDefaultEarnings()
        {
            var earnings = new PayrollEarnings();

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

        // NEW METHOD: Using the same approach as EmployeeProfile form
        private async Task<EmploymentInfoData> GetEmploymentInfoDirect()
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
                                    return new EmploymentInfoData
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
                                    return new EmploymentInfoData
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

            return new EmploymentInfoData();
        }

        // UPDATED METHOD: Now using the direct approach
        private async Task LoadEmploymentInfoDirect()
        {
            try
            {
                var employmentInfo = await GetEmploymentInfoDirect();

                SafeSetLabelText(lblDepartmentInput, employmentInfo.Department ?? "N/A");
                SafeSetLabelText(lblPositionInput, employmentInfo.Position ?? "N/A");

                // Set current date as effectivity date
                SafeSetLabelText(lblEffectivityDateInput, DateTime.Now.ToString("MM-dd-yyyy"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employment information: " + ex.Message);
            }
        }

        private async Task LoadSalaryHistory()
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                if (allSalaryHistory != null && allSalaryHistory.Any())
                {
                    foreach (var record in allSalaryHistory)
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
                }
                else
                {
                    // Show current data if no history
                    var currentRecord = new SalaryHistoryRecord
                    {
                        EffectivityDate = DateTime.Now.ToString("MM-dd-yyyy"),
                        Department = lblDepartmentInput.Text,
                        Position = lblPositionInput.Text,
                        DailyRate = decimal.TryParse(lblDailyRateInput.Text, out decimal rate) ? rate : 0,
                        MonthlySalary = decimal.TryParse(lblSalaryInput.Text, out decimal salary) ? salary : 0
                    };

                    AddHistoryItem(currentRecord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load salary history: " + ex.Message);
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

        private void SafeSetLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
                label.Invoke(new Action(() => label.Text = text ?? "N/A"));
            else
                label.Text = text ?? "N/A";
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

                lblEffectivityDate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblPosition.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblSalary.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);

                lblEffectivityDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);

                buttonEdit.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EmployeeSalaryDetailsEdit EmployeeSalaryDetailsEditForm = new EmployeeSalaryDetailsEdit(employeeId);
            AttributesClass.ShowWithOverlay(parentForm, EmployeeSalaryDetailsEditForm);
        }

        // Helper classes
        public class PayrollEarnings
        {
            public decimal DailyRate { get; set; }
            public decimal BasicPay { get; set; }
            public decimal Commission { get; set; }
            public decimal Incentives { get; set; }
        }

        public class EmploymentInfoData
        {
            public string Department { get; set; }
            public string Position { get; set; }
        }

        public class SalaryHistoryRecord
        {
            public string EffectivityDate { get; set; }
            public string Department { get; set; }
            public string Position { get; set; }
            public decimal DailyRate { get; set; }
            public decimal MonthlySalary { get; set; }
        }
    }
}