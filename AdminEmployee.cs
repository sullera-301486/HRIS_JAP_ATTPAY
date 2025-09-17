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
                if (!row.IsNewRow)
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

        // 🔹 Load Firebase data into DataGridView
        private async void LoadFirebaseData()
        {
            try
            {
                dataGridViewEmployee.Rows.Clear();

                // 🔹 Get EmployeeDetails
                var firebaseEmployees = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<dynamic>();

                // 🔹 Get EmploymentInfo
                var firebaseEmployment = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<List<dynamic>>();

                // 🔹 Employment info lookup
                var employmentDict = new Dictionary<string, (string Department, string Position)>();

                if (firebaseEmployment != null)
                {
                    foreach (var emp in firebaseEmployment)
                    {
                        if (emp == null) continue;
                        string empId = emp.employee_id ?? "";
                        string dept = emp.department ?? "";
                        string pos = emp.position ?? "";

                        if (!string.IsNullOrEmpty(empId))
                            employmentDict[empId] = (dept, pos);
                    }
                }

                int counter = 1;
                foreach (var fbEmp in firebaseEmployees)
                {
                    dynamic data = fbEmp.Object;

                    string employeeId = data.employee_id ?? "";
                    string fullName = $"{data.first_name ?? ""} {data.middle_name ?? ""} {data.last_name ?? ""}".Trim();
                    string contact = data.contact ?? "";
                    string email = data.email ?? "";

                    string department = "";
                    string position = "";
                    if (!string.IsNullOrEmpty(employeeId) && employmentDict.ContainsKey(employeeId))
                    {
                        department = employmentDict[employeeId].Department;
                        position = employmentDict[employeeId].Position;
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
                        Properties.Resources.ExpandRight // same icon as Action column
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
