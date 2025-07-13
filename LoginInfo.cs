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
    public partial class LoginInfo : UserControl
    {
        public LoginInfo()
        {
            InitializeComponent();
            SetFont();
        }

        private void LoginInfo_Load(object sender, EventArgs e)
        {

        }

        private void SetFont()
        {
            try
            {
                name1.Font = AttributesClass.GetFont("Roboto-Regular", 22f);
                name2.Font = AttributesClass.GetFont("Roboto-Regular", 22f);
                name3.Font = AttributesClass.GetFont("Roboto-Regular", 22f);
                info1.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                info2.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                info3.Font = AttributesClass.GetFont("Roboto-Light", 14f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
