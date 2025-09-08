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
    public partial class HRAttendance : UserControl
    {
        public HRAttendance()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
            setDataGridViewAttributes();
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest leaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestForm);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Column6" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "On Time":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(95, 218, 71);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(95, 218, 71);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Late":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Early Out":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(255, 163, 74);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Absent":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(221, 60, 60);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(221, 60, 60);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Leave":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(71, 93, 218);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(71, 93, 218);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                    case "Day Off":
                        e.CellStyle.SelectionBackColor = Color.FromArgb(180, 174, 189);
                        e.CellStyle.SelectionForeColor = Color.White;
                        e.CellStyle.BackColor = Color.FromArgb(180, 174, 189);
                        e.CellStyle.ForeColor = Color.White;
                        break;
                }
            }
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewAttendance.GridColor = Color.White;
            dataGridViewAttendance.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewAttendance.ColumnHeadersHeight = 40;
            dataGridViewAttendance.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewAttendance.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            dataGridViewAttendance.CellFormatting += dataGridView1_CellFormatting;
            dataGridViewAttendance.CellMouseEnter += dataGridViewAttendance_CellMouseEnter;
            dataGridViewAttendance.CellMouseLeave += dataGridViewAttendance_CellMouseLeave;
            dataGridViewAttendance.CellClick += dataGridViewAttendance_CellClick;

            for (int i = 1; i < 30; i++) //test code; will be replaced with actual data from database
            {
                dataGridViewAttendance.Rows.Add(i + ".", "JAP-001", "Franz Louies Deloritos", "4:00", "5:00", "Early Out", "0:00", "0:00", "Manual");
            }
        }

        private void setFont()
        {
            labelHREmployee.Font = AttributesClass.GetFont("Roboto-Light", 20f);
            labelAttendanceDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelAddLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelFiltersName.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
            textBoxSearchEmployee.Font = AttributesClass.GetFont("Roboto-Light", 15f);
            comboBoxSelectDate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
        }

        private void pictureBoxFilters_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminAttendance filterAdminAttendanceForm = new FilterAdminAttendance();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminAttendanceForm);
        }
        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxSearchEmployee, "Find Employee");
        }

        private void dataGridViewAttendance_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Column10")
                dataGridViewAttendance.Cursor = Cursors.Hand;
        }

        private void dataGridViewAttendance_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridViewAttendance.Cursor = Cursors.Default;
        }

        private void dataGridViewAttendance_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewAttendance.Columns[e.ColumnIndex].Name == "Column10")
            {
                Form parentForm = this.FindForm();
                EditAttendance editAttendanceForm = new EditAttendance();
                AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm); //should depend on database in the future
            }
        }

    }
}
