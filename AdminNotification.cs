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
    public partial class AdminNotification : Form
    {
        public AdminNotification()
        {
            InitializeComponent();
            SetFont();
        }

        private void AdminNotification_Load(object sender, EventArgs e)
        {

        }

        private void SetFont()
        {
            labelNotification.Font = AttributesClass.GetFont("Roboto-Regular", 26f);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
