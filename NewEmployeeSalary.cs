using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Wordprocessing;

namespace HRIS_JAP_ATTPAY
{
    public partial class NewEmployeeSalary : Form
    {
        public NewEmployeeSalary()
        {
            InitializeComponent();
            setFont();
        }
        private void setFont()
        {
            try
            {
                lblEmployeeSalaryDetails.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                lblSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                comboBoxNamesSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                lblCurrentInformations.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                lblPreviousSalaryHistory.Font = AttributesClass.GetFont("Roboto-Regular", 16f);

                lblEffectivityDate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblPosition.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblSalary.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);

                tBEffectivityDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                cBDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                cBPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                tBDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);

                buttonAdd.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                lblEffectivityDatePSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDepartmentPSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblPositionPSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblDailyRatePSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
                lblSalaryPSH.Font = AttributesClass.GetFont("Roboto-Regular", 11f, FontStyle.Bold);
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

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmAddSalary ConfirmAddSalaryForm = new ConfirmAddSalary();
            AttributesClass.ShowWithOverlay(parentForm, ConfirmAddSalaryForm);
        }
    }
}
