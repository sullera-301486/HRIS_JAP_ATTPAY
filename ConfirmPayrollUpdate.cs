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
    public partial class ConfirmPayrollUpdate : Form
    {
        public bool UserConfirmed { get; private set; } = false;

        public ConfirmPayrollUpdate()
        {
            InitializeComponent();
            setFont();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            UserConfirmed = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            UserConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
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
