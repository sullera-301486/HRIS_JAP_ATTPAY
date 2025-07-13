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
    public partial class LoginRectangle : UserControl
    {
        public LoginRectangle()
        {
            InitializeComponent();
            SetFont();
        }

        private void LoginRectangle_Load(object sender, EventArgs e)
        {

        }

        private void SetFont()
        {
            try
            {
                labelWelcome.Font = AttributesClass.GetFont("Roboto-Light", 24f);
                labelLogin.Font = AttributesClass.GetFont("Roboto-Regular", 36f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelPassword.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                textBoxPassword.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                buttonLogin.Font = AttributesClass.GetFont("Roboto-Regular", 30f);
                labelFailed.Font = AttributesClass.GetFont("Roboto-Light", 10f, FontStyle.Italic);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
        private void MoveToAdmin()
        {
            Form parentForm = AttributesClass.GetRealOwnerForm(this.FindForm());

            if (parentForm != null)
            {
                parentForm.Tag = "OpenNewForm1"; // Tell Program.cs to open AdminForm
                parentForm.Close();              // Triggers the transition
            }
        }

        private void MoveToHR()
        {
            Form parentForm = AttributesClass.GetRealOwnerForm(this.FindForm());

            if (parentForm != null)
            {
                parentForm.Tag = "OpenNewForm2"; // Tell Program.cs to open HRForm
                parentForm.Close();              // Triggers the transition
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            //don't forget to change this code ofc; temporary code only
            if (textBoxID.Text == "1")
            {
                MoveToAdmin();
                labelFailed.Visible = false;
            }
            else if (textBoxID.Text == "2")
            {
                MoveToHR();
                labelFailed.Visible = false;
            }
            else
            {
                labelFailed.Visible = true;
            }
        }
    }
}
