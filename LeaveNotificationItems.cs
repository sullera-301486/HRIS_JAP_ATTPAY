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
    public partial class LeaveNotificationItems : UserControl
    {
        private DateTime createdAt;
        private Timer refreshTimer;

        public LeaveNotificationItems()
        {
            InitializeComponent();
            SetFont();
        }
        private void SetFont()
        {
            try
            {
                lblLeaveTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                lblSubmittedLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblEmployeeLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblLeave.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPeriod.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblNotes.Font = AttributesClass.GetFont("Roboto-Light", 9f);
                lblTimeAgo.Font = AttributesClass.GetFont("Roboto-Regular", 9f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label6.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label5.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                btnApprove.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                btnDecline.Font = AttributesClass.GetFont("Roboto-Regular", 10f);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        public void SetData(string title, string submitted, string employee,
                     string leaveType, string period, string notes, Image photo = null)
        {
            lblLeaveTitle.Text = title;
            lblSubmittedLeave.Text = submitted;
            lblEmployeeLeave.Text = employee;
            lblLeave.Text = leaveType;
            lblPeriod.Text = period;
            lblNotes.Text = notes;

            if (photo != null)
            {
                picEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                picEmployee.Image = photo;
            }

            // store when this notification was created
            createdAt = DateTime.Now;

            UpdateTimeAgo();

            // start auto-refresh timer if not already started
            if (refreshTimer == null)
            {
                refreshTimer = new Timer();
                refreshTimer.Interval = 60000; // 1 min
                refreshTimer.Tick += (s, e) => UpdateTimeAgo();
                refreshTimer.Start();
            }
        }

        private void UpdateTimeAgo()
        {
            TimeSpan diff = DateTime.Now - createdAt;

            if (diff.TotalMinutes < 1)
                lblTimeAgo.Text = "Just now";
            else if (diff.TotalMinutes < 60)
                lblTimeAgo.Text = $"{(int)diff.TotalMinutes}m ago";
            else if (diff.TotalHours < 24)
                lblTimeAgo.Text = $"{(int)diff.TotalHours}h ago";
            else
                lblTimeAgo.Text = $"{(int)diff.TotalDays}d ago";
        }

        // Button events
        public event EventHandler ApproveClicked;
        public event EventHandler DeclineClicked;

        private void btnApprove_Click(object sender, EventArgs e)
        {
            ApproveClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            DeclineClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
