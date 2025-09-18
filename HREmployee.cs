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
using Firebase.Database.Query; // 🔹 Firebase references

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

        // 🔹 Load Firebase Data
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Firebase data: " + ex.Message);
            }
        }
    }
}
