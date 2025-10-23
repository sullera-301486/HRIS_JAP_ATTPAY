using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class EmployeeSalaryDetails : Form
    {
        public EmployeeSalaryDetails()
        {
            InitializeComponent();
            SetFont();
            ConfigureFlowLayoutPanel();
            LoadEmployeeSalaryList();
        }

        private void SetFont()
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

                lblEffectivityDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);

                buttonEdit.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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

        private void ConfigureFlowLayoutPanel()
        {
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Padding = new Padding(0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.AutoSize = false;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void LoadEmployeeSalaryList()
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                var salaryData = new List<(string Date, string Dept, string Pos, string Rate, string Salary)>
                {
                    ("9-15-2025", "Human Resource", "Staff", "50.00", "750.00"),
                    ("8-15-2025", "IT", "Staff", "20.00", "300.00"),
                    ("7-15-2025", "Finance", "Cashier", "100.00", "1500.00"),
                    ("6-10-2025", "Engineering", "Supervisor", "200.00", "3000.00")
                };

                foreach (var data in salaryData)
                {
                    EmployeeSalaryDetailsList item = new EmployeeSalaryDetailsList
                    {
                        DateValue = data.Date,
                        DepartmentValue = data.Dept,
                        PositionValue = data.Pos,
                        DailyRateValue = data.Rate,
                        SalaryValue = data.Salary,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        Height = 24
                    };

                    flowLayoutPanel1.Controls.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load salary list: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblCurrentInformations_Click(object sender, EventArgs e)
        {

        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EmployeeSalaryDetailsEdit EmployeeSalaryDetailsEditForm = new EmployeeSalaryDetailsEdit();
            AttributesClass.ShowWithOverlay(parentForm, EmployeeSalaryDetailsEditForm);
        }
    }
}
