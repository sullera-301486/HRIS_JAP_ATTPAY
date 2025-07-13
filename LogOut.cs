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
    public partial class LogOut : Form
    {
        public LogOut()
        {
            InitializeComponent();
            SetFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetFont()
        {
            try
            {
                labelLogOutConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 21f);
                labelLogOutDetails1.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLogOutDetails2.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLogOutDetails3.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonLogOut.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonLogOut_Click(object sender, EventArgs e)
        {
            this.Close();

            var activeForm = Application.OpenForms
                .OfType<Form>()
                .FirstOrDefault(f => f != this && !(f is FormHost));

            if (activeForm != null)
            {
                activeForm.Tag = "LoginForm";
                activeForm.Close();
            }
        }
    }
}
