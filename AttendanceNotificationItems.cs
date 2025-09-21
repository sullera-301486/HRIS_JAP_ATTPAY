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
    public partial class AttendanceNotificationItems : UserControl
    {
        private DateTime createdAt;
        private Timer refreshTimer;
        public AttendanceNotificationItems()
        {
            InitializeComponent();
            SetFont();
        }

        private void SetFont()
        {
            try
            {
                lblTimeAgo.Font = AttributesClass.GetFont("Roboto-Regular", 9f);
                lblTitle.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label8.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label1.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label11.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label9.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label10.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                lblSubmittedBy.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblEmployee.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDate.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblOvertimeIn.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblOvertimeOut.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                btnApprove.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                btnDecline.Font = AttributesClass.GetFont("Roboto-Regular", 10f);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
        public void SetData(string title, string submitted, string employee, DateTime date,
            string timeIn, string timeOut, string overtimeIn, string overtimeOut, Image photo = null)
        {
            lblTitle.Text = title;
            lblSubmittedBy.Text = submitted;
            lblEmployee.Text = employee;
            lblDate.Text = date.ToShortDateString();
            lblTimeIn.Text = timeIn;
            lblTimeOut.Text = timeOut;
            lblOvertimeIn.Text = overtimeIn;
            lblOvertimeOut.Text = overtimeOut;

            if (photo != null)
            {
                picEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                picEmployee.Image = photo;
            }

            // store when this notification was added
            createdAt = DateTime.Now;

            UpdateTimeAgo();
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

        private void AttendanceNotificationItems_Load(object sender, EventArgs e)
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 60000; // 1 minute
            refreshTimer.Tick += (s, ev) => UpdateTimeAgo();
            refreshTimer.Start();
        }
    }
}
