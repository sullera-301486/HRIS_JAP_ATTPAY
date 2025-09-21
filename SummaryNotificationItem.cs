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
    public partial class SummaryNotificationItem : UserControl
    {
        private DateTime createdAt;
        private Timer refreshTimer;

        public SummaryNotificationItem()
        {
            InitializeComponent();
            SetFont();
        }
        private void SetFont()
        {
            try
            {
                lblTimeAgo.Font = AttributesClass.GetFont("Roboto-Regular", 9f);
                lblMessage.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        public void SetData(string message, Image icon)
        {
            lblMessage.Text = message;
            picIcon.Image = icon;
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;

            // mark the time this notification was created
            createdAt = DateTime.Now;

            UpdateTimeAgo();

            // start auto-refresh timer if not already running
            if (refreshTimer == null)
            {
                refreshTimer = new Timer();
                refreshTimer.Interval = 60000; // 1 minute
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
    }
}
