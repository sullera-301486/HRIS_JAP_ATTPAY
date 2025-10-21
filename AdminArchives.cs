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
    public partial class AdminArchives : Form
    {
        public AdminArchives()
        {
            InitializeComponent();
            setFont();
            setDataGridViewAttributes();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelArchivedEmployees.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelArchivedRecords.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                labelSelectAll.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Underline);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setDataGridViewAttributes()
        {
            dataGridViewEmployee.ReadOnly = false;
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


            // Ensure columns exist
            dataGridViewEmployee.Columns.Clear();

            // Leftmost: Counter column (narrower)
            var counterCol = new DataGridViewCheckBoxColumn
            {
                Name = "CB",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 84,
                TrueValue = true,
                FalseValue = false,
            };
            dataGridViewEmployee.Columns.Add(counterCol);

            // Main Data Columns
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", HeaderText = "ID", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 104, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Position", HeaderText = "Position", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 108, ReadOnly = true });
            dataGridViewEmployee.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date Archived", HeaderText = "Date Archived", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 148, ReadOnly = true });

            // Rightmost: Button column
            var actionCol = new DataGridViewButtonColumn
            {
                Name = "Restore",
                HeaderText = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 148,
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                ReadOnly = false
            };
            dataGridViewEmployee.Columns.Add(actionCol);

            for (int i = 0; i < 30; i++)
            {
                dataGridViewEmployee.Rows.Add(false, "JAP-00" + i, "Franz Louies Deloritos", "Human Resources", "Head of HR", "10-10-2025", "Restore");
            }

            dataGridViewEmployee.CellFormatting += (s, e) =>
            {
                if (dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Restore" && e.RowIndex >= 0)
                {
                    DataGridViewButtonCell btnCell = (DataGridViewButtonCell)dataGridViewEmployee.Rows[e.RowIndex].Cells["Restore"];
                    btnCell.Style.BackColor = Color.FromArgb(95, 218, 71);
                    btnCell.Style.ForeColor = Color.White;
                    btnCell.FlatStyle = FlatStyle.Flat;
                }
            };

            dataGridViewEmployee.CellPainting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dataGridViewEmployee.Columns[e.ColumnIndex].Name == "Restore" && e.RowIndex >= 0)
                {
                    e.CellStyle.SelectionBackColor = Color.FromArgb(95, 218, 71); // same green
                    e.CellStyle.SelectionForeColor = Color.White;
                }
            };

            // --- Make Checkbox Clickable Immediately ---
            dataGridViewEmployee.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dataGridViewEmployee.IsCurrentCellDirty)
                    dataGridViewEmployee.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // --- Hand Cursor for Buttons and Checkboxes ---
            dataGridViewEmployee.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var colName = dataGridViewEmployee.Columns[e.ColumnIndex].Name;
                    if (colName == "Restore" || colName == "CB")
                        dataGridViewEmployee.Cursor = Cursors.Hand;
                }
            };

            dataGridViewEmployee.CellMouseLeave += (s, e) =>
            {
                dataGridViewEmployee.Cursor = Cursors.Default;
            };
        }

        private void labelSelectAll_Click(object sender, EventArgs e)
        {
            dataGridViewEmployee.ClearSelection();
            dataGridViewEmployee.CurrentCell = null;

            if (labelSelectAll.Text == "Select All")
            {
                labelSelectAll.Text = "Deselect All";
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    // Only modify rows that have a checkbox column
                    if (row.Cells["CB"] is DataGridViewCheckBoxCell cbCell)
                    {
                        cbCell.Value = true;
                    }
                }
            }
            else
            {
                labelSelectAll.Text = "Select All";
                foreach (DataGridViewRow row in dataGridViewEmployee.Rows)
                {
                    // Only modify rows that have a checkbox column
                    if (row.Cells["CB"] is DataGridViewCheckBoxCell cbCell)
                    {
                        cbCell.Value = false;
                    }
                }
            }
        }

        private void buttonRestoreSelected_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmRecoverEmployee confirmRecoverEmployee = new ConfirmRecoverEmployee();
            AttributesClass.ShowWithOverlay(parentForm, confirmRecoverEmployee);
        }
    }
}
