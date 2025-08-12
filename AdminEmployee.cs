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
    public partial class AdminEmployee : UserControl
    {
        public AdminEmployee()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            AddNewEmployee addNewEmployeeForm = new AddNewEmployee();
            AttributesClass.ShowWithOverlay(parentForm, addNewEmployeeForm);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            FilterAdminEmployee filterAdminEmployeeForm = new FilterAdminEmployee();
            AttributesClass.ShowWithOverlay(parentForm, filterAdminEmployeeForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EmployeeProfile employeeProfileForm = new EmployeeProfile();
            AttributesClass.ShowWithOverlay(parentForm, employeeProfileForm);
        }
    }
}
