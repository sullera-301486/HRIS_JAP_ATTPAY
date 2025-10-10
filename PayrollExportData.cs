using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS_JAP_ATTPAY
{
    public class PayrollExportData
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string DateCovered { get; set; }
        public string Days { get; set; }
        public string DaysPresent { get; set; }
        public string DailyRate { get; set; }
        public string Salary { get; set; }
        public string Overtime { get; set; }

        // Earnings
        public string BasicPay { get; set; }
        public string OvertimePerHour { get; set; }
        public string OvertimePerMinute { get; set; }
        public string Incentives { get; set; }
        public string Commission { get; set; }
        public string FoodAllowance { get; set; }
        public string Communication { get; set; }
        public string GasAllowance { get; set; }
        public string Gondola { get; set; }
        public string GrossPay { get; set; }

        // Deductions
        public string WithholdingTax { get; set; }
        public string SSS { get; set; }
        public string PagIbig { get; set; }
        public string Philhealth { get; set; }
        public string SSSLoan { get; set; }
        public string PagIbigLoan { get; set; }
        public string CarLoan { get; set; }
        public string HousingLoan { get; set; }
        public string CashAdvance { get; set; }
        public string CoopLoan { get; set; }
        public string CoopContribution { get; set; }
        public string Others { get; set; }
        public string TotalDeductions { get; set; }

        // Details
        public string TaxDetails { get; set; }
        public string SSSDetails { get; set; }
        public string PagIbigDetails { get; set; }
        public string PhilhealthDetails { get; set; }
        public string SSSLoanDetails { get; set; }
        public string PagIbigLoanDetails { get; set; }
        public string CarLoanDetails { get; set; }
        public string HousingLoanDetails { get; set; }
        public string CashAdvanceDetails { get; set; }
        public string CoopLoanDetails { get; set; }
        public string CoopContributionDetails { get; set; }
        public string OthersDetails { get; set; }


        // Leave balances
        public string VacationLeaveCredit { get; set; }
        public string VacationLeaveDebit { get; set; }
        public string VacationLeaveBalance { get; set; }
        public string SickLeaveCredit { get; set; }
        public string SickLeaveDebit { get; set; }
        public string SickLeaveBalance { get; set; }

        public string NetPay { get; set; }
    }
}
