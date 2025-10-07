using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AddNewEmployee : Form
    {
        private readonly FirebaseClient firebase;
        private string localImagePath;

        private static readonly string FirebaseStorageBucket = "thesis151515.firebasestorage.app";

        public AddNewEmployee()
        {
            InitializeComponent();
            setFont();
            firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
            InitializeDepartmentComboBox();
            UpdateAlternateTextboxAccessibility();
            UpdatePasswordFieldAccessibility();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBoxEmployee.Image == null)
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
                    localImagePath = ofd.FileName;
                    pictureBoxEmployee.Image = Image.FromFile(localImagePath);
                    pictureBoxEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBoxEmployee.Paint -= pictureBox1_Paint;
                    pictureBoxEmployee.Invalidate();
                }
            }
        }
        private async Task SyncAllEmployeesToLeaveCredits()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 🔹 1. Get all employees from EmployeeDetails
                    string employeeUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmployeeDetails.json";
                    var employeeResponse = await httpClient.GetAsync(employeeUrl);

                    if (!employeeResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Failed to fetch EmployeeDetails data from Firebase.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string employeeJson = await employeeResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(employeeJson) || employeeJson == "null")
                    {
                        MessageBox.Show("No employees found in EmployeeDetails.",
                            "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    JObject employeeData = JObject.Parse(employeeJson);

                    // 🔹 2. Get existing Leave Credits
                    string leaveCreditsUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits.json";
                    var leaveResponse = await httpClient.GetAsync(leaveCreditsUrl);
                    string leaveJson = await leaveResponse.Content.ReadAsStringAsync();
                    JObject existingLeaveData = null;
                    if (!string.IsNullOrWhiteSpace(leaveJson) && leaveJson != "null")
                        existingLeaveData = JObject.Parse(leaveJson);

                    int createdCount = 0;

                    // 🔹 3. Loop through employees and only create missing leave credit records
                    foreach (var emp in employeeData)
                    {
                        string employeeId = emp.Key;
                        JObject empObj = (JObject)emp.Value;

                        string firstName = empObj["first_name"]?.ToString() ?? "";
                        string middleName = empObj["middle_name"]?.ToString() ?? "";
                        string lastName = empObj["last_name"]?.ToString() ?? "";
                        string fullName = $"{firstName} {middleName} {lastName}".Trim();

                        bool alreadyExists = existingLeaveData != null && existingLeaveData[employeeId] != null;
                        if (alreadyExists)
                            continue; // 🟢 Skip existing records

                        var leaveCredits = new
                        {
                            employee_id = employeeId,
                            full_name = fullName,
                            sick_leave = 6,
                            vacation_leave = 6,
                            created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                        };

                        string leaveCreditsChildUrl =
                            $"https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits/{employeeId}.json";

                        string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(leaveCredits);
                        var stringContent = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                        var putResponse = await httpClient.PutAsync(leaveCreditsChildUrl, stringContent);

                        if (putResponse.IsSuccessStatusCode)
                            createdCount++;
                    }

                    // 🔹 4. Summary
                    MessageBox.Show(
                        $"Leave Credits sync completed.\n" +
                        $"New Leave Credits created for {createdCount} missing employees.\n" +
                        $"(Existing records were left unchanged.)",
                        "Sync Completed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error syncing Leave Credits: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async void AddNewEmployee_Load(object sender, EventArgs e)
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
                        box.BackColor = Color.FromArgb(96, 81, 148);
                        box.ForeColor = Color.White;
                        DisableCorrespondingDay(box);
                    }
                    else
                    {
                        box.BackColor = Color.FromArgb(217, 217, 217);
                        box.ForeColor = Color.Black;
                        EnableCorrespondingDay(box);
                    }
                    UpdateAlternateTextboxAccessibility();
                };

                cb.Checked = false;
            }
            UpdateAlternateTextboxAccessibility();

            await SyncAllEmployeesToLeaveCredits();
        }

        private void UpdateAlternateTextboxAccessibility()
        {
            bool anyAltDayChecked = checkBoxAltM.Checked || checkBoxAltT.Checked ||
                                   checkBoxAltW.Checked || checkBoxAltTh.Checked ||
                                   checkBoxAltF.Checked || checkBoxAltS.Checked;

            textBoxAltWorkHoursA.Enabled = anyAltDayChecked;
            textBoxAltWorkHoursB.Enabled = anyAltDayChecked;

            if (anyAltDayChecked)
            {
                textBoxAltWorkHoursA.BackColor = SystemColors.Window;
                textBoxAltWorkHoursB.BackColor = SystemColors.Window;
                textBoxAltWorkHoursA.ForeColor = SystemColors.WindowText;
                textBoxAltWorkHoursB.ForeColor = SystemColors.WindowText;
            }
            else
            {
                textBoxAltWorkHoursA.BackColor = SystemColors.Control;
                textBoxAltWorkHoursB.BackColor = SystemColors.Control;
                textBoxAltWorkHoursA.ForeColor = SystemColors.GrayText;
                textBoxAltWorkHoursB.ForeColor = SystemColors.GrayText;
                textBoxAltWorkHoursA.Text = "";
                textBoxAltWorkHoursB.Text = "";
            }
        }

        private void DisableCorrespondingDay(CheckBox checkedBox)
        {
            CheckBox correspondingBox = GetCorrespondingCheckbox(checkedBox);
            if (correspondingBox != null)
            {
                correspondingBox.Enabled = false;
            }
        }

        private void EnableCorrespondingDay(CheckBox uncheckedBox)
        {
            CheckBox correspondingBox = GetCorrespondingCheckbox(uncheckedBox);
            if (correspondingBox != null)
            {
                correspondingBox.Enabled = true;
            }
        }

        private CheckBox GetCorrespondingCheckbox(CheckBox sourceBox)
        {
            Dictionary<CheckBox, CheckBox> dayMapping = new Dictionary<CheckBox, CheckBox>
            {
                { checkBoxM, checkBoxAltM }, { checkBoxT, checkBoxAltT }, { checkBoxW, checkBoxAltW },
                { checkBoxTh, checkBoxAltTh }, { checkBoxF, checkBoxAltF }, { checkBoxS, checkBoxAltS },
                { checkBoxAltM, checkBoxM }, { checkBoxAltT, checkBoxT }, { checkBoxAltW, checkBoxW },
                { checkBoxAltTh, checkBoxTh }, { checkBoxAltF, checkBoxF }, { checkBoxAltS, checkBoxS }
            };
            return dayMapping.ContainsKey(sourceBox) ? dayMapping[sourceBox] : null;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            using (ConfirmAddEmployee confirmForm = new ConfirmAddEmployee())
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                var result = confirmForm.ShowDialog(this);

                if (result == DialogResult.OK || confirmForm.UserConfirmed)
                {
                    if (!ValidateWorkSchedule()) return;
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
                // [⚠ All original font assignments kept here exactly as before]
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
                string employeeId = textBoxEmployeeID.Text.Trim();

                bool idExists = await IsEmployeeIdExists(employeeId);
                if (idExists)
                {
                    MessageBox.Show($"Employee ID '{employeeId}' already exists.", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string imageUrl = null;
                if (!string.IsNullOrEmpty(localImagePath))
                    imageUrl = await UploadImageToFirebase(localImagePath, employeeId);

                string firstName = textBoxFirstName.Text.Trim();
                string middleName = textBoxMiddleName.Text.Trim();
                string lastName = textBoxLastName.Text.Trim();
                string fullName = $"{firstName} {middleName} {lastName}".Trim();

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
                string dateOfExit = textBoxDateOfExit.Text.Trim();
                string managerName = textBoxManager.Text.Trim();
                string password = textBoxPassword.Text.Trim();

                string rfidTag = !string.IsNullOrEmpty(labelRFIDTagInput.Text) && labelRFIDTagInput.Text != "Scan RFID Tag"
                    ? labelRFIDTagInput.Text
                    : employeeId;

                var employeeDetailsObj = new
                {
                    employee_id = employeeId,
                    first_name = firstName,
                    middle_name = middleName,
                    last_name = lastName,
                    full_name = fullName,
                    gender = gender,
                    date_of_birth = dateOfBirth,
                    marital_status = maritalStatus,
                    nationality = nationality,
                    contact = contact,
                    email = email,
                    address = address,
                    rfid_tag = rfidTag,
                    image_url = imageUrl,
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                string numericPart = employeeId.Split('-')[1];
                // Convert to integer to remove leading zeros, then back to string
                string employmentId = int.Parse(numericPart).ToString(); 

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

                await firebase.Child("EmployeeDetails").Child(employeeId).PutAsync(employeeDetailsObj);
                await firebase.Child("EmploymentInfo").Child(employmentId).PutAsync(employmentInfoObj);

                await AddWorkSchedulesAsync(employeeId);

                // ✅ ADD LEAVE CREDITS SECTION
                await CreateDefaultLeaveCredits(employeeId, fullName);

                if (!string.IsNullOrEmpty(password))
                {
                    string userId = (100 + int.Parse(numericPart)).ToString();
                    string salt = "RANDOMSALT" + numericPart;
                    string passwordHash = HashPassword(password, salt);

                    var userObj = new
                    {
                        user_id = userId,
                        employee_id = employeeId,
                        password_hash = passwordHash,
                        salt = salt,
                        isAdmin = "False",
                        created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                    };

                    await firebase.Child("Users").Child(userObj.user_id).PutAsync(userObj);
                }

                MessageBox.Show("Employee successfully added to Firebase.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding employee to Firebase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ NEW FUNCTION: Create Leave Credits
        private async Task CreateDefaultLeaveCredits(string employeeId, string fullName)
        {
            try
            {
                var leaveCreditsObj = new
                {
                    employee_id = employeeId,
                    full_name = fullName,
                    sick_leave = 6,
                    vacation_leave = 6,
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                await firebase.Child("Leave Credits").Child(employeeId).PutAsync(leaveCreditsObj);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating default leave credits: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

private string HashPassword(string password, string salt)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                // Return uppercase hash to match JAP-002 format
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
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

        private async Task<bool> IsEmployeeIdExists(string employeeId)
        {
            try
            {
                // Check if employee ID already exists in EmployeeDetails
                var existingEmployee = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                return existingEmployee != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking employee ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // Return true to prevent adding in case of error
            }
        }

        private void buttonScanRFID_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ScanRFID scanRFID = new ScanRFID(this);
            AttributesClass.ShowWithOverlay(parentForm, scanRFID);
        }

        public void SetRFIDTag(string tag)
        {
            labelRFIDTagInput.Text = tag;
        }
        private void InitializeDepartmentComboBox()
        {
            // Add departments to combobox
            comboBoxDepartment.Items.AddRange(new string[] {
                    "Engineering",
                    "Purchasing",
                    "Operations",
                    "Finance",
                    "Human Resource"
            });
        }

        private void comboBoxDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePasswordFieldAccessibility();
        }
        private void UpdatePasswordFieldAccessibility()
        {
            string selectedDepartment = comboBoxDepartment.SelectedItem?.ToString();

            if (selectedDepartment == "Human Resource")
            {
                // Enable password field for HR department
                textBoxPassword.Enabled = true;
                textBoxPassword.BackColor = SystemColors.Window;
                textBoxPassword.ForeColor = SystemColors.WindowText;

                // Clear any placeholder text and set actual text if needed
                if (textBoxPassword.Text == "HR department only" || textBoxPassword.Text == "Select HR department to enable")
                {
                    textBoxPassword.Text = "";
                }
            }
            else
            {
                // Disable password field for other departments
                textBoxPassword.Enabled = false;
                textBoxPassword.BackColor = SystemColors.Control;
                textBoxPassword.ForeColor = SystemColors.GrayText;
                textBoxPassword.Text = "HR department only"; // Use Text instead of PlaceholderText
            }
        }
        private async Task<int> GetNextScheduleId()
        {
            try
            {
                // Use HttpClient to get raw JSON to properly handle array
                using (var httpClient = new HttpClient())
                {
                    // CORRECTED URL - fixed "trdb" to "rtdb"
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        // If empty or null, start from 1
                        if (string.IsNullOrEmpty(json) || json == "null")
                            return 1;

                        var schedulesArray = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(json);

                        if (schedulesArray == null || schedulesArray.Count == 0)
                            return 1;

                        int maxId = 0;
                        foreach (var item in schedulesArray)
                        {
                            if (item != null && item.Type != Newtonsoft.Json.Linq.JTokenType.Null)
                            {
                                var scheduleId = item["schedule_id"]?.ToString();
                                if (int.TryParse(scheduleId, out int id))
                                {
                                    if (id > maxId)
                                        maxId = id;
                                }
                            }
                        }

                        return maxId + 1;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next schedule ID: {ex.Message}");
                return 1;
            }
        }


        private bool ValidateWorkSchedule()
        {
            // Check if at least one day is selected
            CheckBox[] allCheckboxes = {
                checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS,
                checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS
            };

            bool hasSelectedDays = false;
            foreach (var cb in allCheckboxes)
            {
                if (cb != null && cb.Checked)
                {
                    hasSelectedDays = true;
                    break;
                }
            }

            if (!hasSelectedDays)
            {
                MessageBox.Show("Please select at least one work day for the employee.",
                               "Work Schedule Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if regular schedule has time when days are selected
            CheckBox[] regularDays = { checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS };
            bool hasRegularDays = false;
            foreach (var cb in regularDays)
            {
                if (cb != null && cb.Checked)
                {
                    hasRegularDays = true;
                    break;
                }
            }

            if (hasRegularDays && (string.IsNullOrEmpty(textBoxWorkHoursA.Text) || string.IsNullOrEmpty(textBoxWorkHoursB.Text)))
            {
                MessageBox.Show("Please enter work hours for the selected regular days.",
                               "Work Hours Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if alternate schedule has time when days are selected
            CheckBox[] altDays = { checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS };
            bool hasAltDays = false;
            foreach (var cb in altDays)
            {
                if (cb != null && cb.Checked)
                {
                    hasAltDays = true;
                    break;
                }
            }

            if (hasAltDays && (string.IsNullOrEmpty(textBoxAltWorkHoursA.Text) || string.IsNullOrEmpty(textBoxAltWorkHoursB.Text)))
            {
                MessageBox.Show("Please enter work hours for the selected alternate days.",
                               "Work Hours Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        private async Task AddWorkSchedulesAsync(string employeeId)
        {
            try
            {
                // Get the next available array index and schedule_id
                int nextArrayIndex = await GetNextScheduleArrayIndex();
                int nextScheduleId = await GetNextScheduleId();

                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                // Regular work schedule
                CheckBox[] regularDays = { checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS };
                string workHoursA = textBoxWorkHoursA.Text.Trim();
                string workHoursB = textBoxWorkHoursB.Text.Trim();

                for (int i = 0; i < regularDays.Length; i++)
                {
                    if (regularDays[i] != null && regularDays[i].Checked)
                    {
                        await CreateScheduleArrayEntry(nextArrayIndex, nextScheduleId, employeeId, days[i], workHoursA, workHoursB, "Regular");
                        nextArrayIndex++;
                        nextScheduleId++;
                    }
                }

                // Alternate work schedule
                CheckBox[] altDays = { checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS };
                string altWorkHoursA = textBoxAltWorkHoursA.Text.Trim();
                string altWorkHoursB = textBoxAltWorkHoursB.Text.Trim();

                for (int i = 0; i < altDays.Length; i++)
                {
                    if (altDays[i] != null && altDays[i].Checked)
                    {
                        await CreateScheduleArrayEntry(nextArrayIndex, nextScheduleId, employeeId, days[i], altWorkHoursA, altWorkHoursB, "Alternate");
                        nextArrayIndex++;
                        nextScheduleId++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding work schedules: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> GetNextScheduleArrayIndex()
        {
            try
            {
                // Use HttpClient to get raw JSON to properly handle array
                using (var httpClient = new HttpClient())
                {
                    // CORRECTED URL - fixed "trdb" to "rtdb"
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        // If empty or null, start from 0
                        if (string.IsNullOrEmpty(json) || json == "null")
                            return 0;

                        // Parse as JArray to handle array structure
                        var schedulesArray = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(json);

                        if (schedulesArray == null || schedulesArray.Count == 0)
                            return 0;

                        // Find the highest index (array position)
                        return schedulesArray.Count;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next schedule array index: {ex.Message}");
                return 0;
            }
        }

        private async Task CreateScheduleArrayEntry(int arrayIndex, int scheduleId, string employeeId, string dayOfWeek, string startTime, string endTime, string scheduleType)
        {
            var scheduleObj = new
            {
                schedule_id = scheduleId.ToString(),
                employee_id = employeeId,
                day_of_week = dayOfWeek,
                start_time = startTime,
                end_time = endTime,
                schedule_type = scheduleType
            };

            // Use array index as the key - this will maintain the array structure in Firebase
            await firebase
                .Child("Work_Schedule")
                .Child(arrayIndex.ToString())
                .PutAsync(scheduleObj);
        }
    }
}