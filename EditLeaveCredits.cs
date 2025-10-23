using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class EditLeaveCredits : Form
    {
        private readonly LeaveCreditModel _credit;

        public EditLeaveCredits()
        {
            InitializeComponent();
            setFont();
        }

        public EditLeaveCredits(LeaveCreditModel credit)
        {
            InitializeComponent();
            _credit = credit;
            setFont();
            LoadEmployeeDetails();
        }

        private void LoadEmployeeDetails()
        {
            if (_credit == null) return;

            labelIDInput.Text = _credit.employee_id ?? "N/A";
            labelNameInput.Text = _credit.full_name ?? "N/A";
            labelDepartmentInput.Text = _credit.department ?? "N/A";
            labelPositionInput.Text = _credit.position ?? "N/A";
            labelSLLeftInput.Text = _credit.sick_leave.ToString();
            labelVLLeftInput.Text = _credit.vacation_leave.ToString();
            comboBoxSLCreditInput.Text = _credit.sick_leave_base_value.ToString();
            comboBoxVLCreditInput.Text = _credit.vacation_leave_base_value.ToString();

            labelTotalCreditInput.Text = (_credit.sick_leave_base_value + _credit.vacation_leave_base_value).ToString();
            labelCreditLeftInput.Text = (_credit.sick_leave + _credit.vacation_leave).ToString();
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
