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
    public partial class PayrollSummaryEdit : Form
    {
        public PayrollSummaryEdit()
        {
            InitializeComponent();
        }
        public void CloseMe()
        {
            this.Close();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmPayrollUpdate confirmPayrollUpdateForm = new ConfirmPayrollUpdate();
            AttributesClass.ShowWithOverlay(parentForm, confirmPayrollUpdateForm);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
