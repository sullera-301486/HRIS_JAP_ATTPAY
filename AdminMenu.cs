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
    public partial class AdminMenu : UserControl
    {
        private AttributesClassAlt panelLoaderAdmin;
        public Panel AdminViewPanel;
        private string currentUserId;
        public AdminMenu(Panel targetPanel, string userId)
        {
            InitializeComponent();
            SetFont();
            currentUserId = userId;
            AdminViewPanel = targetPanel;
            panelLoaderAdmin = new AttributesClassAlt(AdminViewPanel);
        }

        private void AdminMenu_Load(object sender, EventArgs e)
        {

        }

        private void buttonOverview_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(126, 112, 175);
            buttonOverview.ForeColor = Color.FromArgb(255, 255, 255);
            buttonEmployee.BackColor = Color.FromArgb(153, 137, 207);
            buttonEmployee.ForeColor = Color.FromArgb(43, 23, 112);
            buttonAttendance.BackColor = Color.FromArgb(153, 137, 207);
            buttonAttendance.ForeColor = Color.FromArgb(43, 23, 112);
            buttonPayroll.BackColor = Color.FromArgb(153, 137, 207);
            buttonPayroll.ForeColor = Color.FromArgb(43, 23, 112);
            panelLoaderAdmin.LoadUserControl(new AdminOverview(currentUserId));
        }

        private void buttonEmployee_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(153, 137, 207);
            buttonOverview.ForeColor = Color.FromArgb(43, 23, 112);
            buttonEmployee.BackColor = Color.FromArgb(126, 112, 175);
            buttonEmployee.ForeColor = Color.FromArgb(255, 255, 255);
            buttonAttendance.BackColor = Color.FromArgb(153, 137, 207);
            buttonAttendance.ForeColor = Color.FromArgb(43, 23, 112);
            buttonPayroll.BackColor = Color.FromArgb(153, 137, 207);
            buttonPayroll.ForeColor = Color.FromArgb(43, 23, 112);
            panelLoaderAdmin.LoadUserControl(new AdminEmployee());
        }

        private void buttonAttendance_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(153, 137, 207);
            buttonOverview.ForeColor = Color.FromArgb(43, 23, 112);
            buttonEmployee.BackColor = Color.FromArgb(153, 137, 207);
            buttonEmployee.ForeColor = Color.FromArgb(43, 23, 112);
            buttonAttendance.BackColor = Color.FromArgb(126, 112, 175);
            buttonAttendance.ForeColor = Color.FromArgb(255, 255, 255);
            buttonPayroll.BackColor = Color.FromArgb(153, 137, 207);
            buttonPayroll.ForeColor = Color.FromArgb(43, 23, 112);
            panelLoaderAdmin.LoadUserControl(new AdminAttendance());
        }

        private void buttonPayroll_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(153, 137, 207);
            buttonOverview.ForeColor = Color.FromArgb(43, 23, 112);
            buttonEmployee.BackColor = Color.FromArgb(153, 137, 207);
            buttonEmployee.ForeColor = Color.FromArgb(43, 23, 112);
            buttonAttendance.BackColor = Color.FromArgb(153, 137, 207);
            buttonAttendance.ForeColor = Color.FromArgb(43, 23, 112);
            buttonPayroll.BackColor = Color.FromArgb(126, 112, 175);
            buttonPayroll.ForeColor = Color.FromArgb(255, 255, 255);
            panelLoaderAdmin.LoadUserControl(new AdminPayroll());
        }

        private void SetFont()
        {
            try
            {
                buttonOverview.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                buttonEmployee.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                buttonAttendance.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                buttonPayroll.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                labelLogOut.Font = AttributesClass.GetFont("Roboto-Medium", 18f, FontStyle.Bold);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void pictureBoxUserProfile_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            UserProfile userProfileForm = new UserProfile("ADMIN");
            AttributesClass.ShowWithOverlay(parentForm, userProfileForm);
        }

        private void labelLogOut_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LogOut logOutForm = new LogOut();
            AttributesClass.ShowWithOverlay(parentForm, logOutForm);
        }

        private void pictureBoxNotification_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AdminNotification AdminNotificationForm = new AdminNotification();
            AttributesClass.ShowWithOverlay(parentForm, AdminNotificationForm);
        }
    }
}
