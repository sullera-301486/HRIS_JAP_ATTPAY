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
    public partial class HREmployee : UserControl
    {
        public HREmployee()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
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
            Form parentForm = this.FindForm();
            EmployeeProfileHR employeeProfileHRForm = new EmployeeProfileHR();
            AttributesClass.ShowWithOverlay(parentForm, employeeProfileHRForm);
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

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewEmployee.Rows.Add(i + ".", "JAP-001", "Franz Louies Deloritos", "Human Resource", "Staff", "09151238765", "franz.deloritos@gmail.com");
            }
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
    }
}
