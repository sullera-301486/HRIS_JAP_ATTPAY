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
    public partial class EmployeeProfile : Form
    {
        public EmployeeProfile()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmArchive confirmArchiveForm = new ConfirmArchive();
            AttributesClass.ShowWithOverlay(parentForm, confirmArchiveForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditEmployeeProfile editEmployeeProfileForm = new EditEmployeeProfile();
            AttributesClass.ShowWithOverlay(parentForm, editEmployeeProfileForm);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
