using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveRequest : Form
    {
        public LeaveRequest()
        {
            InitializeComponent();
            SetFont();
        }
        private void SetFont()
        {
            try
            {
                labelLeaveRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelRequestLeaveEntry.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelReason.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                textBoxNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxReasonInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequestConfirm leaveRequestConfirmForm = new LeaveRequestConfirm();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestConfirmForm);
        }
    }
}
