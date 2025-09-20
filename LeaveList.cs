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
    public partial class LeaveList : UserControl
    {
        public event EventHandler RevokeClicked;
        public LeaveList()
        {
            InitializeComponent();
        }
        public void SetData(string submittedBy, string employee, string date, string leaveType, string period, Image photo)
        {
            lblSubmittedBy.Text = submittedBy;
            lblEmployee.Text = employee;
            lblDate.Text = date;
            lblLeaveType.Text = leaveType;
            lblPeriod.Text = period;

            picEmployee.Image = photo;
            picEmployee.SizeMode = PictureBoxSizeMode.Zoom; // auto-fit
        }

        private void btnRevoke_Click(object sender, EventArgs e)
        {
            RevokeClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
