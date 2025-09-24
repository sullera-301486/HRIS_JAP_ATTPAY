using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminEmployee : UserControl
    {
        // 🔹 Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public AdminEmployee()
        {
            InitializeComponent();
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();

            // 🔹 Load Firebase data
            LoadFirebaseData();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddNewEmployee addNewEmployeeForm = new AddNewEmployee();
            AttributesClass.ShowWithOverlay(parentForm, addNewEmployeeForm);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminEmployee filterAdminEmployeeForm = new FilterAdminEmployee();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminEmployeeForm);
        }

        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearchEmployee.Text.Trim().ToLower();

            foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
            {
                // Check if the row is not the new row and if the search text is not empty
                if (!row.IsNewRow & searchText != "find employee")
                {
                    // Check various cells for a match (e.g., Name, ID, Department)
                    bool isVisible = string.IsNullOrEmpty(searchText) ||
                        (row.Cells["FullName"].Value?.ToString().ToLower().Contains(searchText) ?? false) ||
                        (row.Cells["EmployeeId"].Value?.ToString().ToLower().Contains(searchText) ?? false) ||
                        (row.Cells["Department"].Value?.ToString().ToLower().Contains(searchText) ?? false);

                    row.Visible = isVisible;
                }
            }
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

            // 🔹 Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // 1️⃣ Leftmost: Counter column (narrower)
            var counterCol = new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25 // 🔹 smaller width
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // 🔹 Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Contact", HeaderText = "Contact Number", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });

            // 2️⃣ Rightmost: Image column (narrower)
            var actionCol = new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25 // 🔹 smaller width
            };

            actionCol.Image = Properties.Resources.ExpandRight;
            dataGridViewEmployee.Columns.Add(actionCol);
        }

        private void setFont()
        {
            try
            {
                labelAdminEmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelEmployeeDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelAddEmployee.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
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
                // 🔹 Get values from the clicked row
                string employeeId = dataGridViewEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value?.ToString();
                string fullName = dataGridViewEmployee.Rows[e.RowIndex].Cells["FullName"].Value?.ToString();
                string department = dataGridViewEmployee.Rows[e.RowIndex].Cells["Department"].Value?.ToString();
                string position = dataGridViewEmployee.Rows[e.RowIndex].Cells["Position"].Value?.ToString();

                Form parentForm = this.FindForm();
                Form profileForm;

                // 🔹 Check if HR
                if (!string.IsNullOrEmpty(department) &&
                    department.Equals("Human Resources", StringComparison.OrdinalIgnoreCase))
                {
                    // HR employee
                    profileForm = new EmployeeProfile(employeeId);
                }
                else
                {
                    // Non-HR employee
                    profileForm = new EmployeeProfileHR(employeeId);
                }

                // 🔹 Show with overlay (like your AddNewEmployee & Filter forms)
                AttributesClass.ShowWithOverlay(parentForm, profileForm);
            }
        }

        // 🔹 Load Firebase data into DataGridView - FIXED VERSION
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

                        // 🔹 Add row with counter + image
                        dataGridViewEmployee.Rows.Add(
                            counter,
                            employeeId,
                            fullName,
                            department,
                            position,
                            contact,
                            email,
                            Properties.Resources.ExpandRight
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
        public void RefreshData()
        {
            // Refresh the data grid view
            LoadFirebaseData();
        }
    }
}