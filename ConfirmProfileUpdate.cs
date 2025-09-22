using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmProfileUpdate : Form
    {
        public bool UserConfirmed { get; private set; } = false;

        // ✅ Constructor must NOT be async
        public ConfirmProfileUpdate()
        {
            InitializeComponent();
            setFont();

            // Hook the Load event to run async work later
            this.Load += ConfirmProfileUpdate_Load;
        }

        // ✅ Async work belongs in Load or other event handlers
        private async void ConfirmProfileUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                // Example async call (replace with Firebase if needed)
                await Task.Delay(200); // simulate async loading

                // Update UI once async work finishes
                labelMessage.Text = "Please confirm if you want to update this profile.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during form load: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            this.UserConfirmed = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
