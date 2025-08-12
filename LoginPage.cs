using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class LoginForm : Form
    {
        private AttributesClassAlt panelLoaderRectangle;
        private AttributesClassAlt panelLoaderInfo;
        public LoginForm()
        {
            InitializeComponent();
            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);
            panelLoaderRectangle = new AttributesClassAlt(LoginRectanglePanel);
            panelLoaderInfo = new AttributesClassAlt(LoginInfoPanel);
        }

        private void LoginPage_Load(object sender, EventArgs e)
        {
            panelLoaderInfo.LoadUserControl(new LoginInfo());
            panelLoaderRectangle.LoadUserControl(new LoginRectangle());
        }

        private void LoginBackground_Click(object sender, EventArgs e)
        {

        }
    }
}
