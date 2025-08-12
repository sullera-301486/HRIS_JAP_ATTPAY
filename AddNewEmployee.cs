using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AddNewEmployee : Form
    {
        public AddNewEmployee()
        {
            InitializeComponent();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Image == null) // Only draw text if no image
            {
                string text = "Add Photo";
                Font font = new Font("Roboto-Regular", 14f);
                SizeF textSize = e.Graphics.MeasureString(text, font);

                float x = (pictureBox1.Width - textSize.Width) / 2;
                float y = (pictureBox1.Height - textSize.Height) / 2;

                e.Graphics.DrawString(text, font, Brushes.Black, x, y);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an image";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                    // Optional: Remove the "Add Photo" text after image is loaded
                    pictureBox1.Paint -= pictureBox1_Paint;
                    pictureBox1.Invalidate();
                }
            }
            }

        private void AddNewEmployee_Load(object sender, EventArgs e)
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

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmAddEmployee confirmAddEmployeeForm = new ConfirmAddEmployee();
            AttributesClass.ShowWithOverlay(parentForm, confirmAddEmployeeForm);
        }

        
    }

        
    }

