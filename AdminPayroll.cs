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
    public partial class AdminPayroll : UserControl
    {
        // 🔹 Firebase client
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public AdminPayroll()
        {
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();
            setTextBoxAttributes();

            // Load data from Firebase
            LoadFirebaseData();
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

            // 🔹 Setup columns
            dataGridViewEmployee.Columns.Clear();

            // Numbering column (without header text)
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "RowNumber", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "GrossPay", HeaderText = "Gross Pay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "NetPay", HeaderText = "Net Pay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });

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
                Form parentForm = this.FindForm();
                PayrollSummary payrollSummaryForm = new PayrollSummary();
                AttributesClass.ShowWithOverlay(parentForm, payrollSummaryForm);
            }
        }

        // 🔹 Handle malformed JSON from Attendance
        private List<Dictionary<string, string>> ParseMalformedAttendanceJson(string rawJson)
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
                        records.Add(record);
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

        // 🔹 Load Firebase Data
        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();

                // EmployeeDetails
                var firebaseEmployees = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                // EmploymentInfo (with daily_rate)
                var firebaseEmployment = await firebase.Child("EmploymentInfo").OnceSingleAsync<List<dynamic>>();
                var employmentDict = new Dictionary<string, (string Department, string Position, decimal DailyRate)>();

                if (firebaseEmployment != null)
                {
                    foreach (var emp in firebaseEmployment)
                    {
                        if (emp == null) continue;
                        string empId = emp.employee_id ?? "";
                        string dept = emp.department ?? "";
                        string pos = emp.position ?? "";
                        decimal.TryParse(emp.daily_rate?.ToString() ?? "0", out decimal dailyRate);

                        if (!string.IsNullOrEmpty(empId))
                            employmentDict[empId] = (dept, pos, dailyRate);
                    }
                }

                // Attendance (raw JSON may be malformed)
                string rawAttendanceJson = await firebase.Child("Attendance").OnceAsJsonAsync();
                var attendanceRecords = ParseMalformedAttendanceJson(rawAttendanceJson);

                // Count days present per employee
                var attendanceDict = new Dictionary<string, int>();
                foreach (var record in attendanceRecords)
                {
                    if (record.ContainsKey("employee_id"))
                    {
                        string empId = record["employee_id"];
                        if (!attendanceDict.ContainsKey(empId))
                            attendanceDict[empId] = 0;
                        attendanceDict[empId]++;
                    }
                }

                // Populate DataGrid
                int counter = 1;
                foreach (var fbEmp in firebaseEmployees)
                {
                    dynamic data = fbEmp.Object;

                    string employeeId = data.employee_id ?? "";
                    string fullName = $"{data.first_name ?? ""} {data.middle_name ?? ""} {data.last_name ?? ""}".Trim();

                    string department = "";
                    string position = "";
                    decimal dailyRate = 0;
                    if (employmentDict.ContainsKey(employeeId))
                    {
                        var empData = employmentDict[employeeId];
                        department = empData.Department;
                        position = empData.Position;
                        dailyRate = empData.DailyRate;
                    }

                    int daysPresent = attendanceDict.ContainsKey(employeeId) ? attendanceDict[employeeId] : 0;

                    // 🔹 Gross Pay = daily rate × days present
                    decimal grossPay = dailyRate * daysPresent;

                    // 🔹 Net Pay (for now same as gross)
                    decimal netPay = grossPay;

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
    }
}