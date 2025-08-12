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
    public partial class AdminPayroll : UserControl
    {
        public AdminPayroll()
        {
            InitializeComponent();

        }

        private void AdminPayroll_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminPayroll filterAdminPayrollform = new FilterAdminPayroll();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminPayrollform);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            PayrollSummary confirmPayrollExportAllForm = new PayrollSummary();
            AttributesClass.ShowWithOverlay(parentForm, confirmPayrollExportAllForm);
        }
    }
}
