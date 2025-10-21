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
    public partial class ManageLeaveCredits : Form
    {
        public ManageLeaveCredits()
        {
            InitializeComponent();
            setTextBoxAttributes();
            SetFont();
            setDataGridViewAttributes();
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewLeave newLeaveForm = new NewLeave();
            AttributesClass.ShowWithOverlay(parentForm, newLeaveForm);
        }

        private void XpictureBox_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

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

            // Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // Leftmost: Counter column (narrower)
            var counterCol = new DataGridViewTextBoxColumn
            {
                Name = "ID",
                HeaderText = "ID",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 104
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 168 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "SL Credit", HeaderText = "SL Credit", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "SL Left", HeaderText = "SL Left", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "VL Credit", HeaderText = "VL Credit", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "VL Left", HeaderText = "VL Left", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104 });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Last Updated", HeaderText = "Last Updated", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 168 });

            // Rightmost: Image column (narrower)
            var actionCol = new DataGridViewImageColumn
            {
                Name = "Action",
                HeaderText = "",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 23
            };

            actionCol.Image = Properties.Resources.VerticalThreeDots;
            dataGridViewEmployee.Columns.Add(actionCol);

            for (int i = 0; i < 30; i++)
            {
                dataGridViewEmployee.Rows.Add("JAP-00" + i, "Franz Louies Deloritos", "5", "4", "3", "2", "10-10-2025"); //test
            }
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
                EditLeaveCredits editLeaveCredits = new EditLeaveCredits();
                AttributesClass.ShowWithOverlay(parentForm, editLeaveCredits);
            }
        }
    }
}
