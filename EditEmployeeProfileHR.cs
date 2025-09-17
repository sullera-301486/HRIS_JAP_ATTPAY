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
    public partial class EditEmployeeProfileHR : Form
    {
        public EditEmployeeProfileHR(string employeeId)
        {
            InitializeComponent();
            setFont();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select an image";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBoxEmployee.Image = Image.FromFile(ofd.FileName);
                    pictureBoxEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EditEmployeeProfileHR_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox[] dayBoxes = { checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS, checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS };
            foreach (var cb in dayBoxes)
            {
                cb.Appearance = Appearance.Button;
                cb.TextAlign = ContentAlignment.MiddleCenter;
                cb.FlatStyle = FlatStyle.Flat;
                cb.Size = new Size(45, 45);
                cb.Font = new Font("Roboto-Regular", 8f);
                cb.UseVisualStyleBackColor = false;
                cb.FlatAppearance.CheckedBackColor = Color.FromArgb(96, 81, 148);
                cb.Cursor = Cursors.Hand;

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

        private void setFont()
        {
            try
            {
                buttonChangePhoto.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonUpdate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAddress.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContractType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfExit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfJoining.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEditEmployeeDesc.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEditEmployeeDetails.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmploymentInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManager.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMiddleName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNationality.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelRFIDTag.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelRFIDTagInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelShiftSchedule.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxAddress.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAltWorkHoursA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAltWorkHoursB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxContact.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxContractType.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxDateOfBirth.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxDateOfExit.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxDateOfJoining.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEmail.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxFirstName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxGender.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxLastName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxManager.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMaritalStatus.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMiddleName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNationality.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxWorkHoursA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxWorkHoursB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmProfileUpdate confirmProfileUpdateForm = new ConfirmProfileUpdate();
            AttributesClass.ShowWithOverlay(parentForm, confirmProfileUpdateForm);
        }
    }
}