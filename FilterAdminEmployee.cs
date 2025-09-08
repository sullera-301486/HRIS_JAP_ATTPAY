using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HRIS_JAP_ATTPAY
{
    public partial class FilterAdminEmployee : Form
    {
        public FilterAdminEmployee()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxLastName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxFirstName, "Search name");
            AttributesClass.TextboxPlaceholder(textBoxDay, "Day");
            AttributesClass.TextboxPlaceholder(textBoxMonth, "Month");
            AttributesClass.TextboxPlaceholder(textBoxYear, "Year");
        }

        private void setFont()
        {
            try
            {
                buttonApply.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonReset.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                comboBoxDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxLastName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxFirstName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxDay.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMonth.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxYear.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContract.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelSort.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                checkBoxActive.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxFemale.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxIrregular.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxMale.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxMarried.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxNotActive.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxRegular.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                checkBoxSingle.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
