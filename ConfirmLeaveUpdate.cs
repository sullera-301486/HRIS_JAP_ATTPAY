using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmLeaveUpdate : Form
    {
        public event EventHandler Confirmed;
        private readonly string employeeName;

        public ConfirmLeaveUpdate(string empName = "")
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e) => this.Close();
        private void buttonCancel_Click(object sender, EventArgs e) => this.Close();

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            Confirmed?.Invoke(this, EventArgs.Empty);
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
    }
}
