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
    public partial class LoanDetails : Form
    {
        private string currentEmployeeId;
        public LoanDetails(string employeeId)
        {
            InitializeComponent();
            setFont();
            currentEmployeeId = employeeId;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LoanDetailsEdit loanDetailsEdit= new LoanDetailsEdit(currentEmployeeId);
            AttributesClass.ShowWithOverlay(parentForm, loanDetailsEdit);
        }

        private void setFont()
        {
            try
            {
                comboBoxSelectedInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAmount.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAmountInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelBalance.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBalanceInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelBimonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBimonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEndDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEndDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLoanDetails.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelMonthlyAmortization.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMonthlyAmortizationInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelSelected.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStartDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStartDateInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelTotalPayment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTotalPaymentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
