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
    public partial class ManageLeave : Form
    {
        public ManageLeave()
        {
            InitializeComponent();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewLeave newLeaveForm = new NewLeave();
            AttributesClass.ShowWithOverlay(parentForm, newLeaveForm);
        }
    }
}
