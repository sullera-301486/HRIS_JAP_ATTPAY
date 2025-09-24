using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class AddNewEmployee : Form
    {
        // Firebase client using your given URL
        private readonly FirebaseClient firebase;
        private string localImagePath; // Store the selected image path

        // ---- Firebase Configuration for v7.3 ----
        private static readonly string FirebaseStorageBucket = "thesis151515.firebasestorage.app";
        // ------------------------------------------

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
                    localImagePath = ofd.FileName; // Store the file path
                    pictureBoxEmployee.Image = Image.FromFile(localImagePath);
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
            // Show confirmation dialog first
            using (ConfirmAddEmployee confirmForm = new ConfirmAddEmployee())
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                var result = confirmForm.ShowDialog(this);

                if (result == DialogResult.OK || confirmForm.UserConfirmed) // Check both for safety
                {
                    await AddEmployeeToFirebaseAsync();
                }
                else
                {
                    MessageBox.Show("Employee addition cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
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
                // 1. Handle image upload first (if new image was selected)
                string imageUrl = null;
                if (!string.IsNullOrEmpty(localImagePath))
                {
                    imageUrl = await UploadImageToFirebase(localImagePath, textBoxEmployeeID.Text.Trim());
                }

                // 2. Collect all UI values
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

                // For shift schedule & work days & alt schedule
                string workHoursA = textBoxWorkHoursA.Text.Trim();
                string workHoursB = textBoxWorkHoursB.Text.Trim();
                string altWorkHoursA = textBoxAltWorkHoursA.Text.Trim();
                string altWorkHoursB = textBoxAltWorkHoursB.Text.Trim();

                // Collect which day boxes are checked
                bool[] workDays = {
                    checkBoxM.Checked, checkBoxT.Checked, checkBoxW.Checked,
                    checkBoxTh.Checked, checkBoxF.Checked, checkBoxS.Checked
                };
                bool[] altWorkDays = {
                    checkBoxAltM.Checked, checkBoxAltT.Checked, checkBoxAltW.Checked,
                    checkBoxAltTh.Checked, checkBoxAltF.Checked, checkBoxAltS.Checked
                };

                // Set RFID tag to be the same as employee ID
                string rfidTag = employeeId;

                // Create EmployeeDetails object matching your JSON structure
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
                    rfid_tag = rfidTag, // Now using employee ID as RFID tag
                    image_url = imageUrl, // Add the image URL here
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                // Generate employment_id following your pattern (1, 2, 3, etc.)
                string numericPart = employeeId.Split('-')[1]; // Extract "001" from "JAP-001"
                string employmentId = numericPart; // This will be "001", "002", etc.

                // Create EmploymentInfo object matching your JSON structure
                var employmentInfoObj = new
                {
                    employment_id = employmentId,
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
                    .PutAsync(employeeDetailsObj);

                // Push to EmploymentInfo using the generated employment_id
                await firebase
                    .Child("EmploymentInfo")
                    .Child(employmentId)
                    .PutAsync(employmentInfoObj);

                // Push schedules for regular work days
                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                // Generate schedule IDs following your pattern (1, 2, 3, etc.)
                int scheduleCounter = 1;

                for (int i = 0; i < workDays.Length; i++)
                {
                    if (workDays[i])
                    {
                        string scheduleId = scheduleCounter.ToString();
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
                        scheduleCounter++;
                    }
                }

                // Alternate schedule if any alt days checked
                for (int i = 0; i < altWorkDays.Length; i++)
                {
                    if (altWorkDays[i])
                    {
                        string scheduleId = scheduleCounter.ToString();
                        var scheduleObj = new
                        {
                            schedule_id = scheduleId,
                            employee_id = employeeId,
                            day_of_week = days[i],
                            start_time = altWorkHoursA,
                            end_time = altWorkHoursB,
                            schedule_type = "Alternate"
                        };
                        await firebase
                            .Child("Work_Schedule")
                            .Child(scheduleId)
                            .PutAsync(scheduleObj);
                        scheduleCounter++;
                    }
                }

                // Create default user account
                var userObj = new
                {
                    user_id = (100 + int.Parse(numericPart)).ToString(), // 101, 102, 103, etc.
                    employee_id = employeeId,
                    password_hash = "", // You can set a default or generate one
                    salt = "",
                    isAdmin = "False",
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                await firebase
                    .Child("Users")
                    .Child(userObj.user_id)
                    .PutAsync(userObj);

                MessageBox.Show("Employee successfully added to Firebase.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding employee to Firebase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // CORRECTED for Firebase Storage v7.3 - Same as in EditEmployeeProfile
        private async Task<string> UploadImageToFirebase(string localFilePath, string employeeId)
        {
            string fileNameOnly = Path.GetFileName(localFilePath);
            string objectName = $"employee_images/{employeeId}_{fileNameOnly}";

            // CORRECT URL FOR v7.3 - using the full bucket domain
            string uploadUrl = $"https://firebasestorage.googleapis.com/v0/b/thesis151515.firebasestorage.app/o?uploadType=media&name={Uri.EscapeDataString(objectName)}";

            using (var http = new HttpClient())
            using (var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                var content = new StreamContent(stream);

                string ext = Path.GetExtension(localFilePath).ToLowerInvariant();
                string mime;

                switch (ext)
                {
                    case ".png":
                        mime = "image/png";
                        break;
                    case ".jpg":
                    case ".jpeg":
                        mime = "image/jpeg";
                        break;
                    case ".gif":
                        mime = "image/gif";
                        break;
                    default:
                        mime = "application/octet-stream";
                        break;
                }

                content.Headers.ContentType = new MediaTypeHeaderValue(mime);

                try
                {
                    var resp = await http.PostAsync(uploadUrl, content);
                    string respText = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Upload failed ({(int)resp.StatusCode}): {respText}", "Upload error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    // Parse response for v7.3
                    var j = JObject.Parse(respText);
                    string name = j["name"]?.ToString() ?? objectName;
                    string bucket = j["bucket"]?.ToString() ?? FirebaseStorageBucket;
                    string tokens = j["downloadTokens"]?.ToString();

                    // CORRECT download URL for v7.3
                    string downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/thesis151515.firebasestorage.app/o/{Uri.EscapeDataString(name)}?alt=media";
                    if (!string.IsNullOrEmpty(tokens))
                        downloadUrl += "&token=" + tokens;

                    return downloadUrl;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Upload error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }
    }
}