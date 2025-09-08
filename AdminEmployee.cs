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
    public partial class AdminEmployee : UserControl
    {
        public AdminEmployee()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
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

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewEmployee.Rows.Add(i + ".", "JAP-001", "Franz Louies Deloritos", "Human Resource", "Staff", "09151238765", "franz.deloritos@gmail.com");
            }
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
            } catch (Exception ex) {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void dataGridViewEmployee_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Column8")
                dataGridViewEmployee.Cursor = Cursors.Hand;
        }

        private void dataGridViewEmployee_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewEmployee.Cursor = Cursors.Default;
        }

        private void dataGridViewEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Column8")
            {
                Form parentForm = this.FindForm();
                EmployeeProfile employeeProfileForm = new EmployeeProfile();
                AttributesClass.ShowWithOverlay(parentForm, employeeProfileForm); //should depend on database in the future
            }
        }
    }
}
