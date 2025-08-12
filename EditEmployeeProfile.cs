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
    public partial class EditEmployeeProfile : Form
    {
        public EditEmployeeProfile()
        {
            InitializeComponent();
        }

        private void EditEmployeeProfile_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox[] dayBoxes = { checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS, checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS };
            foreach (var cb in dayBoxes)
            {
                cb.Appearance = Appearance.Button;
                cb.TextAlign = ContentAlignment.MiddleCenter;
                cb.FlatStyle = FlatStyle.Flat;
                cb.Size = new Size(45, 45);
                cb.Font = new Font("Roboto-Regular", 8f);

                cb.CheckedChanged += (s, ev) =>
                {
                    var box = s as System.Windows.Forms.CheckBox;
                    if (box.Checked)
                    {
                        box.BackColor = Color.FromArgb(96, 81, 148);  // Selected
                        box.ForeColor = Color.White;
                    }
                    else
                    {
                        box.BackColor = Color.FromArgb(217, 217, 217); // Unselected
                        box.ForeColor = Color.Black;

                    }
                };

                cb.Checked = false;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an image";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmProfileUpdate confirmProfileUpdateForm = new ConfirmProfileUpdate();
            AttributesClass.ShowWithOverlay(parentForm, confirmProfileUpdateForm);
        }
    }
}
