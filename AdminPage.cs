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
        private string currentEmployeeId;
        private string payrollPeriod;

        public AdminForm(string userId, string employeeId, string period = null)
        {

            InitializeComponent();
            currentUserId = userId;
            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);
            currentEmployeeId = employeeId;
            payrollPeriod = period;
            panelLoaderMenu = new AttributesClassAlt(AdminMenuPanel);
            panelLoaderView = new AttributesClassAlt(AdminViewPanel);
        }

        private void AdminPage_Load(object sender, EventArgs e)
        {
            panelLoaderMenu.LoadUserControl(new AdminMenu(AdminViewPanel, currentUserId, currentEmployeeId));
            panelLoaderView.LoadUserControl(new AdminOverview(currentUserId));
        }
    }
}
