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
    public partial class AdminForm : Form
    {
        private AttributesClassAlt panelLoaderView;
        private AttributesClassAlt panelLoaderMenu;
        private string currentUserId;

        public AdminForm(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);

            panelLoaderMenu = new AttributesClassAlt(AdminMenuPanel);
            panelLoaderView = new AttributesClassAlt(AdminViewPanel);
        }

        private void AdminPage_Load(object sender, EventArgs e)
        {
            panelLoaderMenu.LoadUserControl(new AdminMenu(AdminViewPanel, currentUserId));
            panelLoaderView.LoadUserControl(new AdminOverview(currentUserId));
        }
    }
}
