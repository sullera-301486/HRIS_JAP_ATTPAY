using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class PayrollSummary : Form
    {
        public PayrollExportData ExportData { get; private set; }

        private string currentEmployeeId = "";
        private DateTime cutoffStartDate;
        private DateTime cutoffEndDate;

        private PayrollEmployeeData employeeData;
        private List<PayrollAttendance> attendanceRecords = new List<PayrollAttendance>();
        private List<PayrollLoan> employeeLoans = new List<PayrollLoan>();
        private PayrollEarnings payrollEarnings = new PayrollEarnings();
        private PayrollDeductions employeeDeductions = new PayrollDeductions();
        private PayrollGovDeductions governmentDeductions = new PayrollGovDeductions();
        private PayrollLeave leaveCredits = new PayrollLeave();

        public PayrollSummary(string employeeId, string period = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            currentEmployeeId = employeeId;

            cutoffStartDate = startDate ?? new DateTime(DateTime.Today.Year, 9, 1);
            cutoffEndDate = endDate ?? new DateTime(DateTime.Today.Year, 9, 15);

            InitializeComponent();
            SetFont();

            this.Load += (sender, e) => LoadPayrollData();
        }

        public void SetEmployeeId(string employeeId)
        {
            currentEmployeeId = employeeId;
            LoadPayrollData();
        }

        private void LoadPayrollData()
        {
            employeeData = new PayrollEmployeeData();
            attendanceRecords = new List<PayrollAttendance>();
            employeeLoans = new List<PayrollLoan>();
            payrollEarnings = new PayrollEarnings();
            employeeDeductions = new PayrollDeductions();
            governmentDeductions = new PayrollGovDeductions();
            leaveCredits = new PayrollLeave();

            DisplayEmployeeInfo();
            CalculateAndDisplayPayroll();
        }

        private void DisplayEmployeeInfo()
        {
            labelIDInput.Text = "JAP-001";
            labelNameInput.Text = "Franz Louies Deloritos";
            labelDepartmentInput.Text = "Human Resource";
            labelPositionInput.Text = "Head of HR";
            labelDateCoveredInput.Text = "September 1 - 15, 2025";
        }

        private void CalculateAndDisplayPayroll()
        {
            decimal dailyRate = 664.23m;
            int workDays = 13;
            decimal daysPresent = 12.5m;
            decimal overtime = 0m;
            decimal basicPay = 8302.88m;
            decimal overtimePay = 0m;

            labelDailyRateInput.Text = dailyRate.ToString("0.00");
            labelDaysInput.Text = workDays.ToString();
            labelDaysPresentInput.Text = daysPresent.ToString("0.0");
            labelSalaryInput.Text = "8635.00";
            labelBasicPayAmountBaseInput.Text = "8365.00";
            labelBasicPayAmountCreditInput.Text = basicPay.ToString("0.00");
            labelBasicPayCredit.Text = "12.50";

            labelOvertimeInput.Text = overtime.ToString("0.00");
            labelOvertimePerHourAmountBaseInput.Text = "83.03";
            labelOvertimePerHourAmountCreditInput.Text = overtimePay.ToString("0.00");
            labelOvertimePerMinuteAmountBaseInput.Text = "1.38";
            labelOvertimePerMinuteAmountCreditInput.Text = "0.00";

            DisplayEarnings();
            DisplayDeductions();
            DisplayLoanDeductions();
            DisplayDetails();
            CalculateTotals(basicPay, overtimePay);
        }

        private void DisplayEarnings()
        {
            labelCommissionAmountBaseInput.Text = "0.00";
            labelCommissionAmountCreditInput.Text = "0.00";
            labelIncentivesAmountBaseInput.Text = "0.00";
            labelIncentivesAmountCreditInput.Text = "0.00";
            labelFoodAllowanceAmountBaseInput.Text = "0.00";
            labelFoodAllowanceAmountCreditInput.Text = "0.00";
            labelGasAllowanceAmountBaseInput.Text = "0.00";
            labelGasAllowanceAmountCreditInput.Text = "0.00";
            labelCommunicationAmountBaseInput.Text = "0.00";
            labelCommunicationAmountCreditInput.Text = "0.00";
            labelGondolaAmountBaseInput.Text = "0.00";
            labelGondolaAmountCreditInput.Text = "0.00";
        }

        private void DisplayDeductions()
        {
            labelSSSAmountDebitInput.Text = "250.00";
            labelPhilhealthAmountDebitInput.Text = "125.00";
            labelPagIbigAmountDebitInput.Text = "100.00";
            labelWithTaxAmountDebitInput.Text = "0.00";

            labelCashAdvanceAmountDebitInput.Text = "1000.00";
            labelCoopContriAmountDebitInput.Text = "1000.00";
            labelOthersAmountDebitInput.Text = "534.15";

            DisplayLeaveCredits();
        }

        private void DisplayLoanDeductions()
        {
            labelSSSLoanAmountDebitInput.Text = "230.00";
            labelPagIbigLoanAmountDebitInput.Text = "386.62";
            labelCarLoanAmountDebitInput.Text = "0.00";
            labelHousingLoanAmountDebitInput.Text = "0.00";
            labelCoopLoanAmountDebitInput.Text = "1287.00";
        }

        private void DisplayDetails()
        {
            labelWithTaxDetails.Text = "";
            labelSSSDetails.Text = "";
            labelPagIbigDetails.Text = "";
            labelPhilhealthDetails.Text = "";
            labelCarLoanDetails.Text = "";
            labelHousingLoanDetails.Text = "";
            labelCashAdvanceDetails.Text = "";
            labelCoopContriDetails.Text = "";

            labelSSSLoanDetails.Text = "7/48";
            labelPagIbigLoanDetails.Text = "16/48";
            labelCoopLoanDetails.Text = "1/4";
            labelOthersDetails.Text = "Late";
        }

        private void DisplayLeaveCredits()
        {
            labelSickLeaveCredit.Text = "6.00";
            labelVacationLeaveCredit.Text = "6.00";
            labelSickLeaveDebit.Text = "6.00";
            labelVacationLeaveDebit.Text = "2.00";
            labelSickLeaveBalance.Text = "0.00";
            labelVacationLeaveBalance.Text = "4.00";
        }

        private void CalculateTotals(decimal basicPay, decimal overtimePay)
        {
            decimal grossPay = 8302.88m;

            decimal totalDeductions = 250.00m + 125.00m + 100.00m + 0.00m +
                                    230.00m + 386.62m + 0.00m + 0.00m +
                                    1000.00m + 1000.00m + 534.15m;

            decimal netPay = grossPay - totalDeductions;

            labelGrossPayInput.Text = grossPay.ToString("0.00");
            labelDeductionsInput.Text = totalDeductions.ToString("0.00");
            labelOverallTotalInput.Text = netPay.ToString("0.00");

            PrepareExportData(grossPay, totalDeductions, netPay);
        }

        private void PrepareExportData(decimal grossPay, decimal totalDeductions, decimal netPay)
        {
            ExportData = new PayrollExportData
            {
                EmployeeId = "JAP-001",
                EmployeeName = "Franz Louies Deloritos",
                Department = "Human Resource",
                Position = "Head of HR",
                DateCovered = "September 1 - 15, 2025",
                Days = "13",
                DaysPresent = "12.5",
                DailyRate = "664.23",
                Salary = "8635.00",
                Overtime = "0",
                BasicPay = "8302.88",
                OvertimePerHour = "0.00",
                Incentives = "0.00",
                Commission = "0.00",
                FoodAllowance = "0.00",
                Communication = "0.00",
                GasAllowance = "0.00",
                Gondola = "0.00",
                GrossPay = grossPay.ToString("0.00"),
                WithholdingTax = "0.00",
                SSS = "250.00",
                PagIbig = "100.00",
                Philhealth = "125.00",
                SSSLoan = "230.00",
                PagIbigLoan = "386.62",
                CarLoan = "0.00",
                HousingLoan = "0.00",
                CashAdvance = "1000.00",
                CoopLoan = "1287.00",
                CoopContribution = "1000.00",
                Others = "534.15",
                TotalDeductions = totalDeductions.ToString("0.00"),
                NetPay = netPay.ToString("0.00"),
                VacationLeaveCredit = "6.00",
                VacationLeaveDebit = "2.00",
                VacationLeaveBalance = "4.00",
                SickLeaveCredit = "6.00",
                SickLeaveDebit = "6.00",
                SickLeaveBalance = "0.00",
                SSSLoanDetails = "7/48",
                PagIbigLoanDetails = "16/48",
                CarLoanDetails = "",
                HousingLoanDetails = "",
                CoopLoanDetails = "1/4",
                OthersDetails = "Late"
            };
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (ExportData == null)
            {
                MessageBox.Show("No payroll data to export. Please load employee data first.");
                return;
            }

            Form parentForm = this.FindForm();
            ConfirmPayrollExportIndividual confirmForm = new ConfirmPayrollExportIndividual(ExportData);
            AttributesClass.ShowWithOverlay(parentForm, confirmForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            PayrollSummaryEdit payrollSummaryEditForm = new PayrollSummaryEdit(currentEmployeeId);
            AttributesClass.ShowWithOverlay(parentForm, payrollSummaryEditForm);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            buttonCancel_Click(sender, e);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetFont()
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
            labelCashAdvance.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCashAdvanceAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommission.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCommissionAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommissionAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommunication.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCommunicationAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCommunicationAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopContri.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCoopContriAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCoopLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDailyRate.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDateCovered.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDateCoveredInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDays.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDaysPresent.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDaysPresentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDeductions.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDeductionsInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
            labelOverallTotalInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerHour.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimePerHourAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerHourAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerMinute.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimePerMinuteAmountBaseInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerMinuteAmountCreditInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbig.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelPagIbigAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelPagIbigLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPhilhealth.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelPhilhealthAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
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
            labelSSSLoan.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelSSSLoanAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelVacationLeaveBalance.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelVacationLeaveCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelVacationLeaveDebit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelWithTax.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelWithTaxAmountDebitInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            buttonExport.Font = AttributesClass.GetFont("Roboto-Regular", 14f);

            labelWithTaxDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelSSSDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPhilhealthDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelSSSLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPagIbigLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCarLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelHousingLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCashAdvanceDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopLoanDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelCoopContriDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOthersDetails.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelPayAndAllowances.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelCredit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDebit.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelDetails.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            labelOvertimePerHourCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            labelOvertimePerMinuteCredit.Font = AttributesClass.GetFont("Roboto-Light", 10f);
        }
    }

    public class PayrollEmployeeData
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string ContractType { get; set; }
        public string DateOfJoining { get; set; }

        public string FullName => $"{LastName}, {FirstName} {MiddleName}".Trim().Replace("  ", " ");
    }

    public class PayrollAttendance
    {
        public string EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public string Status { get; set; }
    }

    public class PayrollLoan
    {
        public string LoanType { get; set; }
        public decimal BiMonthlyAmortization { get; set; }
        public decimal MonthlyAmortization { get; set; }
        public string Status { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal Balance { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class PayrollEarnings
    {
        public decimal Commission { get; set; }
        public decimal Incentives { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal GasAllowance { get; set; }
        public decimal Communication { get; set; }
        public decimal Gondola { get; set; }
        public decimal BasicPay { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal DailyRate { get; set; }
        public decimal DaysPresent { get; set; }
    }

    public class PayrollDeductions
    {
        public decimal CashAdvance { get; set; }
        public decimal CoopContribution { get; set; }
        public decimal OtherDeductions { get; set; }
    }

    public class PayrollGovDeductions
    {
        public decimal SSS { get; set; }
        public decimal Philhealth { get; set; }
        public decimal Pagibig { get; set; }
        public decimal WithholdingTax { get; set; }
    }

    public class PayrollLeave
    {
        public decimal SickLeave { get; set; }
        public decimal VacationLeave { get; set; }
    }
}