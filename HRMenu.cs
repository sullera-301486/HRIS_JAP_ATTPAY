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

    public partial class HRMenu : UserControl
    {
        private AttributesClassAlt panelLoaderHR;
        public Panel HRViewPanel;
        public HRMenu(Panel targetPanel)
        {
            InitializeComponent();
            SetFont();
            HRViewPanel = targetPanel;
            panelLoaderHR = new AttributesClassAlt(HRViewPanel);
        }

        private void HRMenu_Load(object sender, EventArgs e)
        {

        }

        private void SetFont()
        {
            try
            {
                buttonOverview.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                buttonEmployee.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                buttonAttendance.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                labelLogOut.Font = AttributesClass.GetFont("Roboto-Medium", 18f, FontStyle.Bold);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonOverview_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(126, 112, 175);
            buttonOverview.ForeColor = Color.FromArgb(255, 255, 255);
            buttonEmployee.BackColor = Color.FromArgb(153, 137, 207);
            buttonEmployee.ForeColor = Color.FromArgb(43, 23, 112);
            buttonAttendance.BackColor = Color.FromArgb(153, 137, 207);
            buttonAttendance.ForeColor = Color.FromArgb(43, 23, 112);
            panelLoaderHR.LoadUserControl(new HROverview());
        }

        private void buttonEmployee_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(153, 137, 207);
            buttonOverview.ForeColor = Color.FromArgb(43, 23, 112);
            buttonEmployee.BackColor = Color.FromArgb(126, 112, 175);
            buttonEmployee.ForeColor = Color.FromArgb(255, 255, 255);
            buttonAttendance.BackColor = Color.FromArgb(153, 137, 207);
            buttonAttendance.ForeColor = Color.FromArgb(43, 23, 112);
            panelLoaderHR.LoadUserControl(new HREmployee());
        }

        private void buttonAttendance_Click(object sender, EventArgs e)
        {
            buttonOverview.BackColor = Color.FromArgb(153, 137, 207);
            buttonOverview.ForeColor = Color.FromArgb(43, 23, 112);
            buttonEmployee.BackColor = Color.FromArgb(153, 137, 207);
            buttonEmployee.ForeColor = Color.FromArgb(43, 23, 112);
            buttonAttendance.BackColor = Color.FromArgb(126, 112, 175);
            buttonAttendance.ForeColor = Color.FromArgb(255, 255, 255);
            panelLoaderHR.LoadUserControl(new HRAttendance());
        }

        private void pictureBoxUserProfile_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            UserProfile userProfileForm = new UserProfile();
            AttributesClass.ShowWithOverlay(parentForm, userProfileForm);
        }

        private void labelLogOut_Click(object sender, EventArgs e)
        {
            Form realOwner = AttributesClass.GetRealOwnerForm(this.FindForm());
            AttributesClass.ShowWithOverlay(realOwner, new LogOut());
        }

        private void pictureBoxNotification_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            HRNotification HRNotificationForm = new HRNotification();
            AttributesClass.ShowWithOverlay(parentForm, HRNotificationForm);
        }
    }
}