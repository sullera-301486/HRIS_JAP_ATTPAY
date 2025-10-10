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
    public partial class ConfirmPayrollExportAll : Form
    {
        public ConfirmPayrollExportAll()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
            buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Payroll_All_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                     await PayrollExportAllData.GenerateAllPayrollsAsync(sfd.FileName);
                }
            }
        }
    }
}
