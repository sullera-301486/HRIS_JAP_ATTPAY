using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class HREmployee : UserControl
    {
        // 🔹 Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public HREmployee()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();

            // 🔹 Add search filter (like AdminEmployee)
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;

            // 🔹 Load data from Firebase
            LoadFirebaseData();
        }

        private void HREmployee_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterHREmployee filterHREmployeeForm = new FilterHREmployee();
            AttributesClass.ShowWithOverlay(parentForm, filterHREmployeeForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("⚠ This table is for viewing only. Profiles cannot be opened from here.",
                "View Only", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            // ❌ Disable row clickable behavior (view only)
            dataGridViewEmployee.ClearSelection();
            dataGridViewEmployee.Enabled = true;

            // 🔹 Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // 1️⃣ Counter column
            var counterCol = new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // 🔹 Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Contact", HeaderText = "Contact Number", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
        }

        private void setFont()
        {
            labelHREmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelEmployeeDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
            textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterHREmployee filterHREmployeeForm = new FilterHREmployee();
            AttributesClass.ShowWithOverlay(parentForm, filterHREmployeeForm);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ManualAttendanceRequest editAttendanceForm = new ManualAttendanceRequest();
            AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest leaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestForm);
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        // 🔹 Search filter (like AdminEmployee)
        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearchEmployee.Text.Trim().ToLower();

            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                if (!row.IsNewRow & searchText != "find employee")
                {
                    bool isVisible = string.IsNullOrEmpty(searchText) ||
                        (row.Cells["FullName"].Value?.ToString().ToLower().Contains(searchText) ?? false) ||
                        (row.Cells["EmployeeId"].Value?.ToString().ToLower().Contains(searchText) ?? false) ||
                        (row.Cells["Department"].Value?.ToString().ToLower().Contains(searchText) ?? false);

                    row.Visible = isVisible;
                }
            }
        }

        // 🔹 Load Firebase Data - FIXED VERSION
        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();

                // 🔹 Get EmployeeDetails
                var firebaseEmployees = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<dynamic>();

                // 🔹 Get EmploymentInfo - handle mixed structure
                var firebaseEmployment = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<dynamic>();

                // 🔹 Employment info lookup - dynamic handling
                var employmentDict = new Dictionary<string, (string Department, string Position)>();

                if (firebaseEmployment != null)
                {
                    foreach (var emp in firebaseEmployment)
                    {
                        if (emp?.Object == null) continue;

                        try
                        {
                            dynamic empData = emp.Object;

                            // 🔹 Dynamic employee_id extraction
                            string empId = ExtractEmployeeId(empData, emp.Key);

                            if (string.IsNullOrEmpty(empId)) continue;

                            // 🔹 Dynamic department extraction
                            string dept = empData.department ?? empData.Department ?? "";
                            string pos = empData.position ?? empData.Position ?? "";

                            // 🔹 Only add if we have valid data
                            if (!string.IsNullOrEmpty(empId) && (!string.IsNullOrEmpty(dept) || !string.IsNullOrEmpty(pos)))
                            {
                                // 🔹 Handle duplicates - prefer the most complete record
                                if (!employmentDict.ContainsKey(empId) ||
                                    (string.IsNullOrEmpty(employmentDict[empId].Department) && !string.IsNullOrEmpty(dept)))
                                {
                                    employmentDict[empId] = (dept, pos);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // 🔹 Skip problematic records but continue processing others
                            System.Diagnostics.Debug.WriteLine($"Skipped employment record: {ex.Message}");
                            continue;
                        }
                    }
                }

                int counter = 1;
                foreach (var fbEmp in firebaseEmployees)
                {
                    try
                    {
                        dynamic data = fbEmp.Object;

                        // 🔹 Dynamic employee ID extraction
                        string employeeId = data.employee_id ?? data.Key ?? "";
                        string fullName = FormatFullName(data.first_name, data.middle_name, data.last_name);
                        string contact = data.contact ?? "";
                        string email = data.email ?? "";

                        // 🔹 Get employment info with fallback
                        string department = "";
                        string position = "";
                        if (!string.IsNullOrEmpty(employeeId) && employmentDict.ContainsKey(employeeId))
                        {
                            var employmentInfo = employmentDict[employeeId];
                            department = employmentInfo.Department;
                            position = employmentInfo.Position;
                        }

                        // 🔹 Add row (no Action column, no click)
                        dataGridViewEmployee.Rows.Add(
                            counter,
                            employeeId,
                            fullName,
                            department,
                            position,
                            contact,
                            email
                        );

                        counter++;
                    }
                    catch (Exception ex)
                    {
                        // 🔹 Skip problematic employee records but continue
                        System.Diagnostics.Debug.WriteLine($"Skipped employee record: {ex.Message}");
                        continue;
                    }
                }

                // 🔹 Show summary
                if (dataGridViewEmployee.Rows.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded {dataGridViewEmployee.Rows.Count} employees");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Firebase data: " + ex.Message +
                               "\n\nPlease check your internet connection and try again.",
                               "Data Load Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        // 🔹 Helper method to extract employee ID dynamically
        private string ExtractEmployeeId(dynamic empData, string firebaseKey)
        {
            // 🔹 Priority 1: Check for employee_id in the data
            string empId = empData.employee_id ?? empData.EmployeeId ?? "";

            // 🔹 Priority 2: If no employee_id in data, check if key is an employee ID format
            if (string.IsNullOrEmpty(empId))
            {
                // 🔹 Check if the Firebase key matches employee ID pattern (like "JAP-001")
                if (!string.IsNullOrEmpty(firebaseKey) && firebaseKey.StartsWith("JAP-"))
                {
                    empId = firebaseKey;
                }
                // 🔹 If key is numeric, this might be an array index, so we need to rely on data
                else if (int.TryParse(firebaseKey, out _))
                {
                    // This is likely an array index record, we'll use the employee_id from data
                    // If empId is still empty, this record will be skipped
                }
            }

            return empId?.Trim() ?? "";
        }

        // 🔹 Helper method to format full name safely
        private string FormatFullName(object firstName, object middleName, object lastName)
        {
            string first = firstName?.ToString() ?? "";
            string middle = middleName?.ToString() ?? "";
            string last = lastName?.ToString() ?? "";

            // 🔹 Clean up extra spaces
            string fullName = $"{first} {middle} {last}".Trim();
            while (fullName.Contains("  "))
                fullName = fullName.Replace("  ", " ");

            return fullName;
        }
    }
}