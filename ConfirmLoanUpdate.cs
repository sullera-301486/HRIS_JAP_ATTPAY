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
    public partial class ConfirmLoanUpdate : Form
    {
        public bool UserConfirmed { get; private set; } = false;

        public ConfirmLoanUpdate()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            // Null check for sender
            if (sender == null) return;

            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Null check for sender
            if (sender == null) return;

            UserConfirmed = false;
            this.Close();
        }

        private void setFont()
        {
            try
            {
                // Null checks for all controls before setting fonts
                if (labelRequestConfirm != null)
                {
                    labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                    labelRequestConfirm.ForeColor = Color.Black; // Changed to black
                }

                if (labelMessage != null)
                {
                    labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                    labelMessage.ForeColor = Color.Black; // Changed to black
                }

                if (buttonConfirm != null)
                {
                    buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                    buttonConfirm.ForeColor = Color.Black; // Changed to black
                }

                if (buttonCancel != null)
                {
                    buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                    buttonCancel.ForeColor = Color.Black; // Changed to black
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Font load failed: {ex.Message}");

                // Show message only if form is fully loaded
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    MessageBox.Show("Font load failed: " + ex.Message);
                }
            }
        }

        // Add form load event for additional safety
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Form load error: {ex.Message}");
            }
        }

        // Add form closing event for cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Form closing error: {ex.Message}");
            }
        }

        private void buttonConfirm_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}