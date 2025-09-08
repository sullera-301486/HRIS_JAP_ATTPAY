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
    public partial class PayrollSummary : Form
    {
        public PayrollSummary()
        {
            InitializeComponent();
            setFont();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmPayrollExportIndividual confirmPayrollExportIndividualForm = new ConfirmPayrollExportIndividual();
            AttributesClass.ShowWithOverlay(parentForm, confirmPayrollExportIndividualForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            PayrollSummaryEdit payrollSummaryEditForm = new PayrollSummaryEdit();
            AttributesClass.ShowWithOverlay(parentForm, payrollSummaryEditForm);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelAmountBase.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelAmountCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelAmountDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelBasicPay.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelBasicPayAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelBasicPayAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelBasicPayCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCarLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCarLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCarLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCashAdvance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCashAdvanceAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCashAdvanceDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommission.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCommissionAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommissionAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommunication.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCommunicationAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCommunicationAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopContri.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCoopContriAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopContriDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelCoopLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDateCovered.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDateCoveredInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDays.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDaysPresent.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDaysPresentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDeductions.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDeductionsInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelDetails.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelFoodAllowance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelFoodAllowanceAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelFoodAllowanceAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGasAllowance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGasAllowanceAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGasAllowanceAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGondola.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGondolaAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGondolaAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelGrossPay.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelGrossPayInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHousingLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelHousingLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelIncentives.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelIncentivesAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelIncentivesAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveBalance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOthers.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOthersAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOthersDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOverallTotalInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimeCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerHour.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimePerHourAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerHourAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinute.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelOvertimePerMinuteAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertimePerMinuteAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbig.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPagIbigAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPagIbigLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPagIbigLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPayAndAllowances.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPayrollSummary.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelPhilhealth.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPhilhealthAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPhilhealthDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSalary.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSickLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSickLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSS.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSSSAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelSSSLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelSSSLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelVacationLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelVacationLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelWithTax.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelWithTaxAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelWithTaxDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonExport.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
