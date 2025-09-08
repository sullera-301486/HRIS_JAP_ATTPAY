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
            setFont();
            setTextBoxAttributes();
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmLeaveEntry confirmLeaveEntryForm = new ConfirmLeaveEntry();
            AttributesClass.ShowWithOverlay(parentForm, confirmLeaveEntryForm);
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
                labelAddLeaveRecord.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDash.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNewLeave.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelSickLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSickLeaveInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelVacationLeaveInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNameInput.Font = AttributesClass.GetFont("Roboto-Light", 15f);   
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveType.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxStartPeriod, "Start of leave");
            AttributesClass.TextboxPlaceholder(textBoxEndPeriod, "End of leave");
        }

    }
}
