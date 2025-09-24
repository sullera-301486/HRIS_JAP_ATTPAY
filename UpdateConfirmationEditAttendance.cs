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
    public partial class UpdateConfirmationEditAttendance : Form
    {
        public bool UserConfirmed { get; private set; } = false;

        public UpdateConfirmationEditAttendance()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            UserConfirmed = false;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            UserConfirmed = false;
            this.Close();
        }

        private void setFont()
        {
            buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            UserConfirmed = true;
            this.DialogResult = DialogResult.OK; // Add this line
            this.Close();
        }
    }
}