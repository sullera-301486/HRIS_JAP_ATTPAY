using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AdminLoan : UserControl
    {
        private AttributesClassAlt panelLoaderAdminPayroll;
        public Panel AdminViewPanel;
        private string currentEmployeeId;
        public AdminLoan(Panel targetPanel, string employeeId)
        {
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();
            setTextBoxAttributes();
            AdminViewPanel = targetPanel;
            currentEmployeeId = employeeId;
            panelLoaderAdminPayroll = new AttributesClassAlt(AdminViewPanel);
        }

        private void labelMovePayroll_Click(object sender, EventArgs e)
        {
            panelLoaderAdminPayroll.LoadUserControl(new AdminPayroll(AdminViewPanel, currentEmployeeId));
        }

        private void labelAddLoan_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LoanDetails loanDetails= new LoanDetails(currentEmployeeId);
            AttributesClass.ShowWithOverlay(parentForm, loanDetails);
        }

        private void setFont()
        {
            try
            {
                labelAdminPayroll.Font = AttributesClass.GetFont("Roboto-Light", 20f);
                labelPayrollDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMovePayroll.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Underline);
                labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelAddLoan.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
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

            // Setup columns
            dataGridViewEmployee.Columns.Clear();

            // Numbering column (without header text)
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "RowNumber", HeaderText = "", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "Balance", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remarks", HeaderText = "Remarks", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });

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

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }
    }
}
