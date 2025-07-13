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
    public partial class UserProfile : Form
    {

        public UserProfile()
        {
            InitializeComponent();
            label1.Text = "NOT ADMIN"; //temporary checker; change if needed
        }

        public UserProfile(string adminTag)
        {
            InitializeComponent();
            label1.Text = adminTag; //for admin version via constructor overloading
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //temporary method; rework if necessary
            //code below shows how to call a new form even when layered, blocking all background forms until foreground form is closed
            UserProfile dialog = new UserProfile();
            Form trueOwner = AttributesClass.GetRealOwnerForm(this.FindForm());
            AttributesClass.ShowWithOverlay(trueOwner, dialog);
        }
    }
}
