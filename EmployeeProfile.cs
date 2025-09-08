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
    public partial class EmployeeProfile : Form
    {
        public EmployeeProfile()
        {
            InitializeComponent();
            setFont();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmArchive confirmArchiveForm = new ConfirmArchive();
            AttributesClass.ShowWithOverlay(parentForm, confirmArchiveForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditEmployeeProfile editEmployeeProfileForm = new EditEmployeeProfile();
            AttributesClass.ShowWithOverlay(parentForm, editEmployeeProfileForm);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonArchive.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelAddress.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAddressInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkHoursInputA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHoursInputB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContactInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContractType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContractTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfBirthInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfExit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfExitInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfJoining.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfJoiningInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmailInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmployeeProfile.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelEmploymentInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFirstNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGenderInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelManager.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManagerInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMiddleName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMiddleNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelNationality.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNationalityInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelPassword.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPasswordInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPersonalAndEmploymentRecord.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelRFIDTag.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelRFIDTagInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelShiftSchedule.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkHoursInputA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHoursInputB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
