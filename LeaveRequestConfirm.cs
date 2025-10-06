using System;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveRequestConfirm : Form
    {
        private readonly LeaveRequestData request;

        public LeaveRequestConfirm(LeaveRequestData req)
        {
            InitializeComponent();
            request = req;
            setFont();
        }

        private void setFont()
        {
            try
            {
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            // confirm returns OK to the caller (LeaveRequest) which will save the request
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
