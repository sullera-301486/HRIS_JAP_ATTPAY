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
    public partial class HRAttendance : UserControl
    {
        public HRAttendance()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ManualAttendanceRequest manualAttendanceRequestForm = new ManualAttendanceRequest();
            AttributesClass.ShowWithOverlay(parentForm, manualAttendanceRequestForm);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewLeave newLeaveForm = new NewLeave();
            AttributesClass.ShowWithOverlay(parentForm, newLeaveForm);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest leaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestForm);
        }
    }
}
