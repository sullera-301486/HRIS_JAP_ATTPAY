using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace HRIS_JAP_ATTPAY
{
    public partial class FilterAdminPayroll : Form
    {
        public FilterAdminPayroll()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonApply.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonReset.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxOvertimeHours.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxUnusedLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                textBoxEndDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxGrossPayMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxGrossPayMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNetPayMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNetPayMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxSalaryMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxSalaryMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashC.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashD.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateRange.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGrossPay.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNetPay.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOvertimeHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSalary.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelSort.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelUnusedLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxEndDate, "End date");
            AttributesClass.TextboxPlaceholder(textBoxGrossPayMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxGrossPayMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxID, "Search ID");
            AttributesClass.TextboxPlaceholder(textBoxName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxNetPayMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxNetPayMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxSalaryMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxSalaryMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxStartDate, "Start date");
        }
    }
}
