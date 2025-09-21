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
            SetFont();
        }
        private void SetFont()
        {
            try
            {
                lblSubmittedBy.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblEmployee.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblDate.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblLeaveType.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                lblPeriod.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                label5.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label6.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label1.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label3.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                btnRevoke.Font = AttributesClass.GetFont("Roboto-Regular", 11f);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
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
