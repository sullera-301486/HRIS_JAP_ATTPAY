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
    public partial class DeductionMatrixDetails : UserControl
    {
        public DeductionMatrixDetails()
        {
            InitializeComponent();
            setFont();
            setPagIbigDataGridViewAttributes();
            setPhilHealthDataGridViewAttributes();
            setSSSDataGridViewAttributes();
            setWithTaxataGridViewAttributes();
        }
    private void setFont()
        {
            try
            {
                labelWithTax.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelSSS.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelPhilHealth.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelPagIbig.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelMisc.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setWithTaxataGridViewAttributes()
        {
            dataGridViewWithTax.GridColor = Color.White;
            dataGridViewWithTax.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewWithTax.ColumnHeadersHeight = 40;
            dataGridViewWithTax.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewWithTax.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 10f);

            dataGridViewWithTax.Rows.Add("₱0 – ₱20,833", "0%", "No Tax", "Exempt");
            dataGridViewWithTax.Rows.Add("₱20,833 – ₱33,333", "20%", "20% of excess over ₱20,833", "");
            dataGridViewWithTax.Rows.Add("₱33,333.01 – ₱66,666", "25%", "₱2,500 + 25% of excess over ₱33,333", "");
            dataGridViewWithTax.Rows.Add("₱66,666.01 – ₱166,666", "30%", "₱10,833 + 30% of excess over ₱66,666", "");
            dataGridViewWithTax.Rows.Add("₱166,666.01 – ₱666,666", "32%", "₱40,833 + 32% of excess over ₱166,666", "");
            dataGridViewWithTax.Rows.Add("Over ₱666,666", "35%", "₱200,833 + 35% of excess over ₱666,666", "");
        }

        private void setSSSDataGridViewAttributes()
        {
            dataGridViewSSS.GridColor = Color.White;
            dataGridViewSSS.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewSSS.ColumnHeadersHeight = 40;
            dataGridViewSSS.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewSSS.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 10f);

            dataGridViewSSS.Rows.Add("Below ₱5,250", "₱5,000", "₱510", "₱250", "₱760", "Minimum Bracket");
            dataGridViewSSS.Rows.Add("₱5,250 – ₱7,749.99", "₱7,000", "₱710", "₱350", "₱1,060", "9.5% ER + 4.5% EE");
            dataGridViewSSS.Rows.Add("₱7,750 – ₱10,249.99", "₱10,000", "₱1,010", "₱500", "₱1,510", "");
            dataGridViewSSS.Rows.Add("₱10,250 – ₱12,749.99", "₱12,000", "₱1,210", "₱600", "₱1,810", "");
            dataGridViewSSS.Rows.Add("₱12,750 – ₱15,249.99", "₱15,000", "₱1,510", "₱750", "₱2,260", "");
            dataGridViewSSS.Rows.Add("₱15,250 – ₱17,749.99", "₱17,000", "₱1,730", "₱850", "₱2,580", "");
            dataGridViewSSS.Rows.Add("₱17,750 – ₱20,249.99", "₱20,000", "₱2,030", "₱1,000", "₱3,030", "");
            dataGridViewSSS.Rows.Add("₱20,250 – ₱24,749.99", "₱23,000", "₱2,330", "₱1,150", "₱3,480", "");
            dataGridViewSSS.Rows.Add("₱24,750 – ₱29,749.99", "₱28,000", "₱2,830", "₱1,400", "₱4,230", "");
            dataGridViewSSS.Rows.Add("₱29,750 – ₱34,749.99", "₱33,000", "₱3,330", "₱1,650", "₱4,980", "");
            dataGridViewSSS.Rows.Add("₱34,750 and Above", "₱35,000", "₱3,530", "₱1,750", "₱5,280", "Max Contribution");
        }

        private void setPagIbigDataGridViewAttributes()
        {
            dataGridViewPagIbig.GridColor = Color.White;
            dataGridViewPagIbig.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewPagIbig.ColumnHeadersHeight = 40;
            dataGridViewPagIbig.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewPagIbig.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 10f);

            dataGridViewPagIbig.Rows.Add("₱1,500 and below", "1%", "2%", "3%", "Minimum rate");
            dataGridViewPagIbig.Rows.Add("Over ₱1,500", "2%", "2%", "4%", "Regular rate");
        }

        private void setPhilHealthDataGridViewAttributes()
        {
            dataGridViewPhilHealth.GridColor = Color.White;
            dataGridViewPhilHealth.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewPhilHealth.ColumnHeadersHeight = 40;
            dataGridViewPhilHealth.DefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            dataGridViewPhilHealth.ColumnHeadersDefaultCellStyle.Font = AttributesClass.GetFont("Roboto-Regular", 10f);

            dataGridViewPhilHealth.Rows.Add("₱10,000", "5%", "₱250", "₱250", "₱500", "Minimum base");
            dataGridViewPhilHealth.Rows.Add("₱10,001 – ₱999,999.99", "5%", "₱250–₱25,250", "₱250–₱25,250", "₱500–₱50,500", "Shared 50–50");
            dataGridViewPhilHealth.Rows.Add("₱1,000,000 and above", "5%", "₱25,000", "₱25,000", "₱50,000", "Maximum limit");
        }

        private void panelWithTax_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelWithTax.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelWithTax_Resize(object sender, EventArgs e)
        {
            panelWithTax.Invalidate();
        }

        private void panelSSS_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelSSS.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelSSS_Resize(object sender, EventArgs e)
        {
            panelSSS.Invalidate();
        }

        private void panelPagIbig_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelPagIbig.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelPagIbig_Resize(object sender, EventArgs e)
        {
            panelPagIbig.Invalidate();
        }

        private void panelPhilHealth_Paint(object sender, PaintEventArgs e)
        {
            //border design
            Color borderColor = Color.FromArgb(96, 81, 148);
            int borderWidth = 1;

            //draw the border
            ControlPaint.DrawBorder(e.Graphics, panelPhilHealth.ClientRectangle,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void panelPhilHealth_Resize(object sender, EventArgs e)
        {
            panelPhilHealth.Invalidate();
        }
    }
}