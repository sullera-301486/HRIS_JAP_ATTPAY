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
    public partial class EmployeeProfileHR : Form
    {
        public EmployeeProfileHR()
        {
            InitializeComponent();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditEmployeeProfileHR editEmployeeProfileHRForm = new EditEmployeeProfileHR();
            AttributesClass.ShowWithOverlay(parentForm, editEmployeeProfileHRForm);
        }
    }
}
