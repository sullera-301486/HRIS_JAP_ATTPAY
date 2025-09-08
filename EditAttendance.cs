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
    public partial class EditAttendance : Form
    {
        public EditAttendance()
        {
            InitializeComponent();
            setFont();
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            UpdateConfirmationEditAttendance UpdateConfirmationEditAttendanceForm = new UpdateConfirmationEditAttendance();
            AttributesClass.ShowWithOverlay(parentForm, UpdateConfirmationEditAttendanceForm);

        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHoursWorked.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelHoursWorkedInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelManualAttendanceRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOverTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOverTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelRequestAttendanceEntry.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxOverTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxOverTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
            }
    }
}
