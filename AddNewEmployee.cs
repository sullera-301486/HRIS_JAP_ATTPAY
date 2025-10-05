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
            InitializeDepartmentComboBox();
            // Initialize alternate textbox state
            UpdateAlternateTextboxAccessibility();

            // Initialize password field accessibility
            UpdatePasswordFieldAccessibility();
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

                        // Disable the corresponding checkbox in the other schedule
                        DisableCorrespondingDay(box);
                    }
                    else
                    {
                        box.BackColor = Color.FromArgb(217, 217, 217); // Unselected
                        box.ForeColor = Color.Black;

                        // Enable the corresponding checkbox in the other schedule
                        EnableCorrespondingDay(box);
                    }

                    // Update alternate textbox accessibility whenever any alternate checkbox changes
                    UpdateAlternateTextboxAccessibility();
                };

                cb.Checked = false;
            }

            // Initialize alternate textbox state
            UpdateAlternateTextboxAccessibility();
        }
        private void UpdateAlternateTextboxAccessibility()
        {
            // Check if any alternate workday is checked
            bool anyAltDayChecked = checkBoxAltM.Checked || checkBoxAltT.Checked ||
                                   checkBoxAltW.Checked || checkBoxAltTh.Checked ||
                                   checkBoxAltF.Checked || checkBoxAltS.Checked;

            // Enable/disable alternate work hours textboxes based on alternate day selection
            textBoxAltWorkHoursA.Enabled = anyAltDayChecked;
            textBoxAltWorkHoursB.Enabled = anyAltDayChecked;

            // Optional: Change appearance to visually indicate disabled state
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

                // Optional: Clear the text when disabled
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
            // Map regular days to alternate days and vice versa
            Dictionary<CheckBox, CheckBox> dayMapping = new Dictionary<CheckBox, CheckBox>
    {
        // Regular to Alternate mapping
        { checkBoxM, checkBoxAltM },
        { checkBoxT, checkBoxAltT },
        { checkBoxW, checkBoxAltW },
        { checkBoxTh, checkBoxAltTh },
        { checkBoxF, checkBoxAltF },
        { checkBoxS, checkBoxAltS },
        
        // Alternate to Regular mapping
        { checkBoxAltM, checkBoxM },
        { checkBoxAltT, checkBoxT },
        { checkBoxAltW, checkBoxW },
        { checkBoxAltTh, checkBoxTh },
        { checkBoxAltF, checkBoxF },
        { checkBoxAltS, checkBoxS }
    };

            return dayMapping.ContainsKey(sourceBox) ? dayMapping[sourceBox] : null;
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
                    // Validate work schedule before proceeding
                    if (!ValidateWorkSchedule())
                    {
                        return;
                    }

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
                // === VALIDATION CHECK ===
                string employeeId = textBoxEmployeeID.Text.Trim();

                // Check if employee ID already exists
                bool idExists = await IsEmployeeIdExists(employeeId);
                if (idExists)
                {
                    MessageBox.Show($"Employee ID '{employeeId}' already exists. Please use a different ID.", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Stop the process
                }
                // === END OF VALIDATION CHECK ===

                // 1. Handle image upload first (if new image was selected)
                string imageUrl = null;
                if (!string.IsNullOrEmpty(localImagePath))
                {
                    imageUrl = await UploadImageToFirebase(localImagePath, employeeId);
                }

                // 2. Collect all UI values
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
                string password = textBoxPassword.Text.Trim(); // Get the password

                // RFID TAG HANDLING - Use scanned RFID if available, otherwise use employee ID
                string rfidTag = !string.IsNullOrEmpty(labelRFIDTagInput.Text) && labelRFIDTagInput.Text != "Scan RFID Tag"
                    ? labelRFIDTagInput.Text
                    : employeeId;

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
                    rfid_tag = rfidTag, // Now using scanned RFID if available, otherwise employee ID
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

                // 3. Add work schedules based on checkbox selections (NEW CORRECTED VERSION)
                await AddWorkSchedulesAsync(employeeId);

                // 4. DETERMINE IF EMPLOYEE SHOULD BE A USER BASED ON PASSWORD
                if (!string.IsNullOrEmpty(password))
                {
                    // Create user account with password (like JAP-001 and JAP-002)
                    string userId = (100 + int.Parse(numericPart)).ToString(); // 101, 102, 103, etc.

                    // Generate salt and hash the password - matching JAP-002 format
                    string salt = "RANDOMSALT" + numericPart; // Format like "RANDOMSALT2" for JAP-002
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

                    await firebase
                        .Child("Users")
                        .Child(userObj.user_id)
                        .PutAsync(userObj);
                }
                // If password is null or empty, DO NOT create user account (no else block)

                MessageBox.Show("Employee successfully added to Firebase.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding employee to Firebase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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