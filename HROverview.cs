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
    public partial class HROverview : UserControl
    {
        public HROverview()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ManualAttendanceRequest editAttendanceForm = new ManualAttendanceRequest();
            AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            LeaveRequest leaveRequestForm = new LeaveRequest();
            AttributesClass.ShowWithOverlay(parentForm, leaveRequestForm);
        }
    }
}
