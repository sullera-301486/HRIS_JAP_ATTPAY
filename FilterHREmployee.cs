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
    public partial class FilterHREmployee : Form
    {
        public FilterHREmployee()
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
                comboBoxDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                comboBoxSort.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
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
            AttributesClass.TextboxPlaceholder(textBoxID, "Search ID");
            AttributesClass.TextboxPlaceholder(textBoxName, "Search name");
        }
    }
}
