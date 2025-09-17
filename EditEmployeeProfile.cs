using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;

namespace HRIS_JAP_ATTPAY
{
    public partial class EditEmployeeProfile : Form
    {
        private readonly string _employeeId; // store employee ID
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis-2e8d2-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // 🔹 Default constructor (no ID)
        public EditEmployeeProfile()
        {
            InitializeComponent();
            setFont();
        }

        // 🔹 Constructor with employee ID
        public EditEmployeeProfile(string employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            setFont();
        }

        private async void EditEmployeeProfile_Load(object sender, EventArgs e)
        {
            // Setup checkboxes safely
            var dayBoxes = new[]
            {
                checkBoxM, checkBoxT, checkBoxW, checkBoxTh,
                checkBoxF, checkBoxS,
                checkBoxAltM, checkBoxAltT, checkBoxAltW,
                checkBoxAltTh, checkBoxAltF, checkBoxAltS
            }
            .Where(cb => cb != null)
            .ToArray();

            foreach (var cb in dayBoxes)
            {
                cb.Appearance = Appearance.Button;
                cb.TextAlign = ContentAlignment.MiddleCenter;
                cb.FlatStyle = FlatStyle.Flat;
                cb.UseVisualStyleBackColor = false;
                cb.Size = new Size(45, 45);
                cb.Font = new Font("Roboto-Regular", 8f);
                cb.FlatAppearance.CheckedBackColor = Color.FromArgb(96, 81, 148);
                cb.Cursor = Cursors.Hand;

                cb.CheckedChanged += (s, ev) =>
                {
                    var box = s as CheckBox;
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
                cb.BackColor = Color.FromArgb(217, 217, 217);
            }

            // 🔹 Load employee data if an ID was passed
            if (!string.IsNullOrEmpty(_employeeId))
            {
                await LoadEmployeeData(_employeeId);
            }
        }

        private async Task LoadEmployeeData(string employeeId)
        {
            try
            {
                // ✅ Load EmployeeDetails from Firebase
                var empDetails = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<EmployeeDetailsModel>();

                if (empDetails != null)
                {
                    textBoxFirstName.Text = empDetails.first_name;
                    textBoxMiddleName.Text = empDetails.middle_name;
                    textBoxLastName.Text = empDetails.last_name;
                    textBoxEmail.Text = empDetails.email;
                    textBoxContact.Text = empDetails.contact;
                    textBoxAddress.Text = empDetails.address;
                    textBoxDateOfBirth.Text = empDetails.date_of_birth;
                    textBoxGender.Text = empDetails.gender;
                    textBoxMaritalStatus.Text = empDetails.marital_status;
                    textBoxNationality.Text = empDetails.nationality;
                    labelEmployeeIDInput.Text = empDetails.employee_id;
                    labelRFIDTagInput.Text = empDetails.rfid_tag;
                }

                // ✅ Load EmploymentInfo from Firebase
                var empInfo = await firebase
                    .Child("EmploymentInfo")
                    .Child(employeeId)
                    .OnceSingleAsync<EmploymentInfoModel>();

                if (empInfo != null)
                {
                    comboBoxDepartment.Text = empInfo.department;
                    labelPositionInput.Text = empInfo.position;
                    textBoxContractType.Text = empInfo.contract_type;
                    textBoxDateOfJoining.Text = empInfo.date_of_joining;
                    textBoxDateOfExit.Text = empInfo.date_of_exit;
                    textBoxManager.Text = empInfo.manager_name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee data: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void buttonChangePhoto_Click(object sender, EventArgs e)
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
                labelPassword.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPasswordInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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
    }

    // 🔹 Firebase models
    public class EmployeeDetailsModel
    {
        public string employee_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string date_of_birth { get; set; }
        public string gender { get; set; }
        public string marital_status { get; set; }
        public string nationality { get; set; }
        public string contact { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string rfid_tag { get; set; }
        public string created_at { get; set; }
    }

    public class EmploymentInfoModel
    {
        public string employee_id { get; set; }
        public string position { get; set; }
        public string department { get; set; }
        public string contract_type { get; set; }
        public string date_of_joining { get; set; }
        public string date_of_exit { get; set; }
        public string manager_name { get; set; }
        public string created_at { get; set; }
    }
}
