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
    public partial class AddNewTask : Form
    {
        public AddNewTask()
        {
            InitializeComponent();
            setFont();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelAddNewTask.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelAddTaskDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDueDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTaskDesc.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxDueDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTaskDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
