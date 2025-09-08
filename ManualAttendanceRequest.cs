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
    public partial class ManualAttendanceRequest : Form
    {
        public ManualAttendanceRequest()
        {
            InitializeComponent();
            SetFont();
        }

        private void SetFont()
        {
            try
            {
                labelManualAttendanceRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelRequestAttendanceEntry.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelDateInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOverTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOverTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxOverTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxOverTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelHoursWorked.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelHoursWorkedInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);

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
            EditAttendanceConfirm editAttendanceConfirmForm = new EditAttendanceConfirm();
            AttributesClass.ShowWithOverlay(parentForm, editAttendanceConfirmForm);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
