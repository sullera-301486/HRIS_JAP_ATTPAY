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
    public partial class UserProfile : Form
    {

        public UserProfile()
        {
            InitializeComponent();
            SetFont();
            labelIDInput.Text = "NOT ADMIN"; //temporary checker; change if needed
        }

        private void SetFont()
        {
            try
            {
                labelProfile.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 16f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelContactInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelEmailInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        public UserProfile(string adminTag)
        {
            InitializeComponent();
            SetFont();
            labelIDInput.Text = adminTag; //for admin version via constructor overloading
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
