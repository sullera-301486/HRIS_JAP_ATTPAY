using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class ManageLeaveCredits : Form
    {
        private readonly FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        private List<LeaveCreditModel> allLeaveCredits = new List<LeaveCreditModel>();
        private bool isOpeningDialog = false;

        public ManageLeaveCredits()
        {
            InitializeComponent();
            setTextBoxAttributes();
            SetFont();
            setDataGridViewAttributes();
            _ = LoadLeaveCreditsAsync();
            textBoxSearchEmployee.TextChanged += textBoxSearchEmployee_TextChanged;
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewLeave newLeaveForm = new NewLeave();
            AttributesClass.ShowWithOverlay(parentForm, newLeaveForm);
        }

        private void XpictureBox_Click_1(object sender, EventArgs e) => this.Close();

        private void SetFont()
        {
            try
            {
                labelLeaveManagement.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                labelAddLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeLeaveCredits.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelManageLeaveCredits.Font = AttributesClass.GetFont("Roboto-Light", 14f);
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

            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 168 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "SL Credit", HeaderText = "SL Credit", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "SL Left", HeaderText = "SL Left", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "VL Credit", HeaderText = "VL Credit", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "VL Left", HeaderText = "VL Left", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Last Updated", HeaderText = "Last Updated", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 168 });

            var actionCol = new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 23,
                Image = Properties.Resources.VerticalThreeDots
            };
            dataGridViewEmployee.Columns.Add(actionCol);
        }

        private async Task LoadLeaveCreditsAsync()
        {
            try
            {
                if (isOpeningDialog) return;

                var leaveCredits = await firebase
                    .Child("Leave Credits")
                    .OnceAsync<LeaveCreditModel>();

                allLeaveCredits = leaveCredits.Select(x => x.Object).ToList();
                DisplayLeaveCredits(allLeaveCredits);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading leave credits: " + ex.Message);
            }
        }

        private void DisplayLeaveCredits(IEnumerable<LeaveCreditModel> credits)
        {
            dataGridViewEmployee.Rows.Clear();
            foreach (var credit in credits)
            {
                dataGridViewEmployee.Rows.Add(
                    credit.employee_id,
                    credit.full_name,
                    credit.sick_leave_base_value,
                    credit.sick_leave,
                    credit.vacation_leave_base_value,
                    credit.vacation_leave,
                    credit.updated_at
                );
            }
        }

        private void textBoxSearchEmployee_TextChanged(object sender, EventArgs e)
        {
            string query = textBoxSearchEmployee.Text.Trim().ToLower();

            // 🟣 Skip filtering when it's the placeholder text
            if (string.IsNullOrEmpty(query) || query == "find employee")
            {
                DisplayLeaveCredits(allLeaveCredits);
                return;
            }

            var filtered = allLeaveCredits
                .Where(emp =>
                    (!string.IsNullOrEmpty(emp.full_name) && emp.full_name.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(emp.employee_id) && emp.employee_id.ToLower().Contains(query)))
                .ToList();

            DisplayLeaveCredits(filtered);
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
            if (isOpeningDialog) return;
            if (e.RowIndex < 0 || dataGridViewEmployee.Columns[e.ColumnIndex].Name != "Action") return;

            var row = dataGridViewEmployee.Rows[e.RowIndex];
            var empId = row.Cells["ID"].Value?.ToString();

            if (string.IsNullOrEmpty(empId)) return;

            try
            {
                isOpeningDialog = true;

                // 🔹 FIXED: find record by employee_id (not Firebase key)
                var leaveCredits = await firebase.Child("Leave Credits").OnceAsync<LeaveCreditModel>();
                var empData = leaveCredits.FirstOrDefault(x => x.Object.employee_id == empId)?.Object;

                if (empData != null)
                {
                    Form parentForm = this.FindForm();
                    EditLeaveCredits editLeaveCredits = new EditLeaveCredits(empData);
                    AttributesClass.ShowWithOverlay(parentForm, editLeaveCredits);
                }
                else
                {
                    MessageBox.Show($"No record found for {empId}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load employee details: " + ex.Message);
            }
            finally
            {
                isOpeningDialog = false;
                DisplayLeaveCredits(allLeaveCredits);
            }
        }
    }

    public class LeaveCreditModel
    {
        public string employee_id { get; set; }
        public string full_name { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public int sick_leave { get; set; }
        public int sick_leave_base_value { get; set; }
        public int vacation_leave { get; set; }
        public int vacation_leave_base_value { get; set; }
        public string updated_at { get; set; }
    }
}
