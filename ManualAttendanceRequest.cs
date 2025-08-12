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
                labelRequestAttendanceEntry.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelDateInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelOverTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelOverTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                textBoxTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxOverTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxOverTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelHoursWorked.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                labelHoursWorkedInput.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
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
            EditAttendanceConfirm editAttendanceConfirmForm = new EditAttendanceConfirm();
            AttributesClass.ShowWithOverlay(parentForm, editAttendanceConfirmForm);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelRequestAttendanceEntry_Click(object sender, EventArgs e)
        {

        }

        private void labelManualAttendanceRequest_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panelStatus_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelStatusInput_Click(object sender, EventArgs e)
        {

        }

        private void labelDateInput_Click(object sender, EventArgs e)
        {

        }

        private void labelNameInput_Click(object sender, EventArgs e)
        {

        }

        private void labelIDInput_Click(object sender, EventArgs e)
        {

        }

        private void labelStatus_Click(object sender, EventArgs e)
        {

        }

        private void labelDate_Click(object sender, EventArgs e)
        {

        }

        private void labelName_Click(object sender, EventArgs e)
        {

        }

        private void labelID_Click(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelOverTimeOut_Click(object sender, EventArgs e)
        {

        }

        private void labelOverTimeIn_Click(object sender, EventArgs e)
        {

        }

        private void labelTimeOut_Click(object sender, EventArgs e)
        {

        }

        private void labelTimeIn_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBoxOverTimeOut_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxOverTimeIn_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxTimeOut_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxTimeIn_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelOvertimeInput_Click(object sender, EventArgs e)
        {

        }

        private void labelOvertime_Click(object sender, EventArgs e)
        {

        }

        private void labelHoursWorkedInput_Click(object sender, EventArgs e)
        {

        }

        private void labelHoursWorked_Click(object sender, EventArgs e)
        {

        }
    }
}
