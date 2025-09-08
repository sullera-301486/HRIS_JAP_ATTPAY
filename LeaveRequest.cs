using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveRequest : Form
    {
        public LeaveRequest()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
        }
        private void setFont()
        {
            try
            {
                labelDash.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelRequestLeaveEntry.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelReason.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxReasonInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequestConfirm leaveRequestConfirmForm = new LeaveRequestConfirm();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestConfirmForm);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxStartPeriod, "Start of leave");
            AttributesClass.TextboxPlaceholder(textBoxEndPeriod, "End of leave");
        }
    }
}
