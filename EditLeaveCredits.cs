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
    public partial class EditLeaveCredits : Form
    {
        public EditLeaveCredits()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click_1(object sender, EventArgs e)
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
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelTotalCredit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTotalCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelEditLeaveCredits.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCreditLeft.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSLCredit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelCreditLeftInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVLCredit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelUpdateLeaveData.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSLLeft.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelVLLeft.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSLLeftInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVLLeftInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxSLCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxVLCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelTotalCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmLeaveUpdate confirmLeaveUpdate = new ConfirmLeaveUpdate();
            AttributesClass.ShowWithOverlay(parentForm, confirmLeaveUpdate);
        }
    }
}
