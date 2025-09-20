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
        public AttendanceNotificationItems()
        {
            InitializeComponent();
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
