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
    public partial class AdminAttendance : UserControl
    {
        public AdminAttendance()
        {
            InitializeComponent();
        }

        private void AdminAttendance_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditAttendance editAttendanceForm = new EditAttendance();
            AttributesClass.ShowWithOverlay(parentForm, editAttendanceForm);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminAttendance filterAdminAttendanceForm = new FilterAdminAttendance();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminAttendanceForm);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
