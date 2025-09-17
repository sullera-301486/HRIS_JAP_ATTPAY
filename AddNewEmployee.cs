using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class AddNewEmployee : Form
    {
        // Firebase client using your given URL
        private readonly FirebaseClient firebase;

        public AddNewEmployee()
        {
            InitializeComponent();
            setFont();
            // initialize firestore client here
            firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBoxEmployee.Image == null) // Only draw text if no image
            {
                string text = "ADD PHOTO";
                using (Font font = new Font("Roboto-Regular", 14f))
                {
                    SizeF textSize = e.Graphics.MeasureString(text, font);

                    float x = (pictureBoxEmployee.Width - textSize.Width) / 2;
                    float y = (pictureBoxEmployee.Height - textSize.Height) / 2;

                    e.Graphics.DrawString(text, font, Brushes.Black, x, y);
                }
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
                    pictureBoxEmployee.Image = Image.FromFile(ofd.FileName);
                    pictureBoxEmployee.SizeMode = PictureBoxSizeMode.Zoom;

                    // remove the "Add Photo" text after image is loaded
                    pictureBoxEmployee.Paint -= pictureBox1_Paint;
                    pictureBoxEmployee.Invalidate();
                }
            }
        }

        private void AddNewEmployee_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox[] dayBoxes = {
                checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS,
                checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS
            };
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
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmAddEmployee confirmAddEmployeeForm = new ConfirmAddEmployee();
            AttributesClass.ShowWithOverlay(parentForm, confirmAddEmployeeForm);

            // After showing confirmation, add employee to Firebase
            await AddEmployeeToFirebaseAsync();
        }

        private void setFont()
        {
            try
            {
                buttonScanRFID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAddNewEmployee.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelAddress.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelCreateEmployeeProfile.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContractType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfExit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfJoining.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmploymentInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManager.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMiddleName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNationality.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                // labelPassword etc., if there is
                labelPassword.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
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
                textBoxEmployeeID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxFirstName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxGender.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxLastName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxManager.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMaritalStatus.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMiddleName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNationality.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxPassword.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxPosition.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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

        private async Task AddEmployeeToFirebaseAsync()
        {
            try
            {
                // Collect all UI values
                string employeeId = textBoxEmployeeID.Text.Trim();
                string firstName = textBoxFirstName.Text.Trim();
                string middleName = textBoxMiddleName.Text.Trim();
                string lastName = textBoxLastName.Text.Trim();
                string gender = textBoxGender.Text.Trim();
                string dateOfBirth = textBoxDateOfBirth.Text.Trim();
                string maritalStatus = textBoxMaritalStatus.Text.Trim();
                string nationality = textBoxNationality.Text.Trim();
                string contact = textBoxContact.Text.Trim();
                string email = textBoxEmail.Text.Trim();
                string address = textBoxAddress.Text.Trim();
                string position = textBoxPosition.Text.Trim();
                string department = comboBoxDepartment.Text.Trim();
                string contractType = textBoxContractType.Text.Trim();
                string dateOfJoining = textBoxDateOfJoining.Text.Trim();
                string dateOfExit = textBoxDateOfExit.Text.Trim(); // can be blank
                string managerName = textBoxManager.Text.Trim();

                // For shift schedule & work days & alt schedule, you will likely want to gather the checked boxes & hours
                // Assuming you have textBoxWorkHoursA and textBoxWorkHoursB, alt times etc.
                string workHoursA = textBoxWorkHoursA.Text.Trim(); // e.g. "08:00 AM"
                string workHoursB = textBoxWorkHoursB.Text.Trim(); // e.g. "05:00 PM"
                string altWorkHoursA = textBoxAltWorkHoursA.Text.Trim(); // alternate hours
                string altWorkHoursB = textBoxAltWorkHoursB.Text.Trim();

                // Collect which day boxes are checked
                // e.g.
                bool[] workDays = {
                    checkBoxM.Checked, checkBoxT.Checked, checkBoxW.Checked,
                    checkBoxTh.Checked, checkBoxF.Checked, checkBoxS.Checked
                };
                bool[] altWorkDays = {
                    checkBoxAltM.Checked, checkBoxAltT.Checked, checkBoxAltW.Checked,
                    checkBoxAltTh.Checked, checkBoxAltF.Checked, checkBoxAltS.Checked
                };

                // Generate a random RFID
                var rnd = new Random();
                string rfidTag = "RFID" + rnd.Next(100000, 999999).ToString();

                // Create object matching EmployeeDetails
                var employeeDetailsObj = new
                {
                    employee_id = employeeId,
                    first_name = firstName,
                    middle_name = middleName,
                    last_name = lastName,
                    gender = gender,
                    date_of_birth = dateOfBirth,
                    marital_status = maritalStatus,
                    nationality = nationality,
                    contact = contact,
                    email = email,
                    address = address,
                    rfid_tag = rfidTag,
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                // Create object matching EmploymentInfo
                var employmentInfoObj = new
                {
                    employment_id = Guid.NewGuid().ToString(), // or some unique id
                    employee_id = employeeId,
                    contract_type = contractType,
                    department = department,
                    position = position,
                    manager_name = managerName,
                    date_of_joining = dateOfJoining,
                    date_of_exit = dateOfExit,
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                // Push to EmployeeDetails
                await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .PutAsync(employeeDetailsObj); // using Put so key = employeeId

                // Push to EmploymentInfo using generated employment_id
                await firebase
                    .Child("EmploymentInfo")
                    .Child(employmentInfoObj.employment_id)
                    .PutAsync(employmentInfoObj);

                // Push schedules for regular work days
                // For each day of week, if checked, create a Work_Schedule entry
                // Example mapping: 0=M,1=T,2=W,3=Th,4=F,5=S
                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                for (int i = 0; i < workDays.Length; i++)
                {
                    if (workDays[i])
                    {
                        // generate a schedule_id
                        string scheduleId = Guid.NewGuid().ToString();
                        var scheduleObj = new
                        {
                            schedule_id = scheduleId,
                            employee_id = employeeId,
                            day_of_week = days[i],
                            start_time = workHoursA,
                            end_time = workHoursB,
                            schedule_type = "Regular"
                        };
                        await firebase
                            .Child("Work_Schedule")
                            .Child(scheduleId)
                            .PutAsync(scheduleObj);
                    }
                }

                // Alternate schedule if any alt days checked
                for (int i = 0; i < altWorkDays.Length; i++)
                {
                    if (altWorkDays[i])
                    {
                        string scheduleId2 = Guid.NewGuid().ToString();
                        var scheduleObj2 = new
                        {
                            schedule_id = scheduleId2,
                            employee_id = employeeId,
                            day_of_week = days[i],
                            start_time = altWorkHoursA,
                            end_time = altWorkHoursB,
                            schedule_type = "Alternate"
                        };
                        await firebase
                            .Child("Work_Schedule")
                            .Child(scheduleId2)
                            .PutAsync(scheduleObj2);
                    }
                }

                MessageBox.Show("Employee successfully added to Firebase.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding employee to Firebase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
