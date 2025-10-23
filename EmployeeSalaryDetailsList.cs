using System;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class EmployeeSalaryDetailsList : UserControl
    {
        public EmployeeSalaryDetailsList()
        {
            InitializeComponent();

            // 🔹 Prevent automatic resizing and remove gaps
            this.AutoSize = false;
            this.Margin = new Padding(0);
            this.Padding = new Padding(0);
            this.Height = 24; // ensures tight stacking of rows

            setFont();
        }

        private void setFont()
        {
            try
            {
                lblDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDailyRateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblSalaryInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        // 🔹 Properties for setting display values
        public string DateValue
        {
            get => lblDateInput.Text;
            set => lblDateInput.Text = value;
        }

        public string DepartmentValue
        {
            get => lblDepartmentInput.Text;
            set => lblDepartmentInput.Text = value;
        }

        public string PositionValue
        {
            get => lblPositionInput.Text;
            set => lblPositionInput.Text = value;
        }

        public string DailyRateValue
        {
            get => lblDailyRateInput.Text;
            set => lblDailyRateInput.Text = value;
        }

        public string SalaryValue
        {
            get => lblSalaryInput.Text;
            set => lblSalaryInput.Text = value;
        }
    }
}
