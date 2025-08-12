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
    public partial class NewLeave : Form
    {
        public NewLeave()
        {
            InitializeComponent();
        }


        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSendRequest_Click_1(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmLeaveEntry confirmLeaveEntryForm = new ConfirmLeaveEntry();
            AttributesClass.ShowWithOverlay(parentForm, confirmLeaveEntryForm);
        }
    }
}
