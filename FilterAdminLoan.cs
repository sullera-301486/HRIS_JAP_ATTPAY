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
    public partial class FilterAdminLoan : Form
    {
        public FilterAdminLoan()
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
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLoanTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAmountMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAmountMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxBalanceMaximum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxBalanceMinimum.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAmount.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelBalance.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashC.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateRange.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLoanType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelSort.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxEndDate, "End date");
            AttributesClass.TextboxPlaceholder(textBoxAmountMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxAmountMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxID, "Search ID");
            AttributesClass.TextboxPlaceholder(textBoxName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxBalanceMaximum, "Maximum");
            AttributesClass.TextboxPlaceholder(textBoxBalanceMinimum, "Minimum");
            AttributesClass.TextboxPlaceholder(textBoxStartDate, "Start date");
        }
    }
    }
