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
    public partial class ConfirmAddEmployee : Form
    {
        public bool UserConfirmed { get; private set; } = false;

        public ConfirmAddEmployee()
        {
            InitializeComponent();
            setFont();
            this.Load += ConfirmProfileUpdate_Load;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = false; // Fixed: Should be false for cancel/X
            this.DialogResult = DialogResult.Cancel; // Fixed: Should be Cancel
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = false; // Fixed: Should be false for cancel
            this.DialogResult = DialogResult.Cancel; // Fixed: Should be Cancel
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonConfirm_Click(object sender, EventArgs e) // Removed async - not needed
        {
            this.UserConfirmed = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void ConfirmProfileUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(200);
                // Add any initialization code here if needed
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during form load: " + ex.Message);
            }
        }
    }
}