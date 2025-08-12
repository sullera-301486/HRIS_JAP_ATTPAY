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
    public partial class LeaveRequestConfirm : Form
    {
        public LeaveRequestConfirm()
        {
            InitializeComponent();
            SetFont();
        }
        private void SetFont()
        {
            try
            {
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelMessage.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Regular", 12f);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();  
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void labelMessage_Click(object sender, EventArgs e)
        {

        }

        private void labelRequestConfirm_Click(object sender, EventArgs e)
        {

        }
    }
}
