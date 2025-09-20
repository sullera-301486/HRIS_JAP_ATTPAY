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
        public LeaveNotificationItems()
        {
            InitializeComponent();
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
                picEmployee.SizeMode = PictureBoxSizeMode.Zoom; picEmployee.Image = photo;
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
