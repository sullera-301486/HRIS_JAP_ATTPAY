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

            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            LoadFirebaseData();

            AdminViewPanel = targetPanel;
            panelLoaderAdminLoan = new AttributesClassAlt(AdminViewPanel);
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

        private void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Action")
            {
                string selectedEmployeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();
                if (!string.IsNullOrEmpty(selectedEmployeeId))
                {
                    Form parentForm = this.FindForm();
                    PayrollSummary payrollSummaryForm = new PayrollSummary(currentEmployeeId);
                    payrollSummaryForm.SetEmployeeId(selectedEmployeeId);
                    AttributesClass.ShowWithOverlay(parentForm, payrollSummaryForm);
                }
            }
        }

        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();

                var employeeDetails = new Dictionary<string, dynamic>();
                var employmentInfo = new Dictionary<string, Dictionary<string, string>>();
                var payrollData = new Dictionary<string, Dictionary<string, string>>();

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

                // Load Payroll (directly from Firebase)
                await LoadArrayBasedData("Payroll", (item) =>
                {
                    var employeeId = item.ContainsKey("employee_id") ? item["employee_id"] : null;
                    if (!string.IsNullOrEmpty(employeeId))
                        payrollData[employeeId] = item;
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

                    decimal grossPay = 0;
                    decimal netPay = 0;

                    if (payrollData.ContainsKey(employeeId))
                    {
                        var payroll = payrollData[employeeId];
                        decimal.TryParse(payroll.ContainsKey("gross_pay") ? payroll["gross_pay"] : "0", out grossPay);
                        decimal.TryParse(payroll.ContainsKey("net_pay") ? payroll["net_pay"] : "0", out netPay);
                    }

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
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }
            return records;
        }

        private void labelMoveLoan_Click(object sender, EventArgs e)
        {
            panelLoaderAdminLoan.LoadUserControl(new AdminLoan(AdminViewPanel, currentEmployeeId));
        }
    }
}