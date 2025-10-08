using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class EditEmployeeProfile : Form
    {
        private string selectedEmployeeId;
        private string localImagePath; // Store the selected image path

        // ---- Firebase Configuration for v7.3 ----
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private static readonly string FirebaseStorageBucket = "thesis151515.firebasestorage.app";
        // ------------------------------------------

        // 🔹 Constructor with employee ID
        public EditEmployeeProfile(string employeeId)
        {
            InitializeComponent();
            selectedEmployeeId = employeeId; // Store the employee ID
            setFont();
        }

        private async void EditEmployeeProfile_Load(object sender, EventArgs e)
        {
            // Setup checkboxes safely
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

            // Initialize department combo box
            InitializeDepartmentComboBox();

            // 🔹 Load employee data if an ID was passed
            if (!string.IsNullOrEmpty(selectedEmployeeId))
            {
                await LoadEmployeeData(selectedEmployeeId);
            }

            // Initialize password field accessibility based on loaded department
            UpdatePasswordFieldAccessibility();

            // Initialize alternate textbox state
            UpdateAlternateTextboxAccessibility();
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
                    textBoxFirstName.Text = empDetails.first_name ?? "";
                    textBoxMiddleName.Text = empDetails.middle_name ?? "";
                    textBoxLastName.Text = empDetails.last_name ?? "";
                    textBoxEmail.Text = empDetails.email ?? "";
                    textBoxContact.Text = empDetails.contact ?? "";
                    textBoxAddress.Text = empDetails.address ?? "";
                    textBoxDateOfBirth.Text = empDetails.date_of_birth ?? "";
                    textBoxGender.Text = empDetails.gender ?? "";
                    textBoxMaritalStatus.Text = empDetails.marital_status ?? "";
                    textBoxNationality.Text = empDetails.nationality ?? "";
                    labelEmployeeIDInput.Text = empDetails.employee_id ?? "";
                    labelRFIDTagInput.Text = empDetails.rfid_tag ?? "";

                    // Load existing image if available
                    if (!string.IsNullOrEmpty(empDetails.image_url))
                    {
                        try
                        {
                            pictureBoxEmployee.Load(empDetails.image_url);
                            pictureBoxEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                        catch
                        {
                            // If image loading fails, use default or leave empty
                            pictureBoxEmployee.Image = null;
                        }
                    }
                }

                // ✅ Load EmploymentInfo from Firebase - FIXED VERSION
                await LoadEmploymentInfoFixed(employeeId);

                // ✅ Load User data for password
                await LoadUserData(employeeId);

                // ✅ Load Work Schedule data
                await LoadWorkScheduleData(employeeId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee data: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NEW METHOD: Fixed EmploymentInfo loading that handles null elements
        private async Task LoadEmploymentInfoFixed(string employeeId)
        {
            try
            {
                // Method 1: Try reading as JArray first (handles the array structure with null element)
                try
                {
                    var employmentArray = await firebase
                        .Child("EmploymentInfo")
                        .OnceSingleAsync<JArray>();

                    if (employmentArray != null)
                    {
                        foreach (var item in employmentArray)
                        {
                            // Skip null elements
                            if (item?.Type == JTokenType.Null)
                                continue;

                            if (item?.Type == JTokenType.Object)
                            {
                                var empObj = (JObject)item;
                                var empId = empObj["employee_id"]?.ToString();

                                if (empId == employeeId)
                                {
                                    SetEmploymentInfoUI(empObj);
                                    return;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fall through to method 2 if array approach fails
                }

                // Method 2: Try as keyed collection (backward compatibility)
                try
                {
                    var employmentData = await firebase
                        .Child("EmploymentInfo")
                        .OnceAsync<object>();

                    if (employmentData != null)
                    {
                        foreach (var item in employmentData)
                        {
                            // Skip null items
                            if (item?.Object == null)
                                continue;

                            try
                            {
                                var empObj = JObject.FromObject(item.Object);
                                var empId = empObj["employee_id"]?.ToString();

                                if (empId == employeeId)
                                {
                                    SetEmploymentInfoUI(empObj);
                                    return;
                                }
                            }
                            catch
                            {
                                // Skip items that can't be converted to JObject
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in keyed collection method: {ex.Message}");
                }

                // Set defaults if no data found
                SetDefaultEmploymentInfoUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employment info: {ex.Message}");
                SetDefaultEmploymentInfoUI();
            }
        }

        // NEW METHOD: Set employment info to UI controls
        private void SetEmploymentInfoUI(JObject empObj)
        {
            // Use Invoke to ensure thread-safe UI updates
            if (comboBoxDepartment.InvokeRequired)
            {
                comboBoxDepartment.Invoke(new Action(() =>
                    comboBoxDepartment.Text = empObj["department"]?.ToString() ?? ""));
            }
            else
            {
                comboBoxDepartment.Text = empObj["department"]?.ToString() ?? "";
            }

            if (textBoxPositionInput.InvokeRequired)
            {
                textBoxPositionInput.Invoke(new Action(() =>
                    textBoxPositionInput.Text = empObj["position"]?.ToString() ?? ""));
            }
            else
            {
                textBoxPositionInput.Text = empObj["position"]?.ToString() ?? "";
            }

            if (textBoxContractType.InvokeRequired)
            {
                textBoxContractType.Invoke(new Action(() =>
                    textBoxContractType.Text = empObj["contract_type"]?.ToString() ?? ""));
            }
            else
            {
                textBoxContractType.Text = empObj["contract_type"]?.ToString() ?? "";
            }

            if (textBoxDateOfJoining.InvokeRequired)
            {
                textBoxDateOfJoining.Invoke(new Action(() =>
                    textBoxDateOfJoining.Text = empObj["date_of_joining"]?.ToString() ?? ""));
            }
            else
            {
                textBoxDateOfJoining.Text = empObj["date_of_joining"]?.ToString() ?? "";
            }

            if (textBoxDateOfExit.InvokeRequired)
            {
                textBoxDateOfExit.Invoke(new Action(() =>
                    textBoxDateOfExit.Text = empObj["date_of_exit"]?.ToString() ?? ""));
            }
            else
            {
                textBoxDateOfExit.Text = empObj["date_of_exit"]?.ToString() ?? "";
            }

            if (textBoxManager.InvokeRequired)
            {
                textBoxManager.Invoke(new Action(() =>
                    textBoxManager.Text = empObj["manager_name"]?.ToString() ?? ""));
            }
            else
            {
                textBoxManager.Text = empObj["manager_name"]?.ToString() ?? "";
            }

            // Update password field accessibility after setting department
            UpdatePasswordFieldAccessibility();
        }

        // NEW METHOD: Set default employment info to UI
        private void SetDefaultEmploymentInfoUI()
        {
            if (comboBoxDepartment.InvokeRequired)
            {
                comboBoxDepartment.Invoke(new Action(() => comboBoxDepartment.Text = ""));
            }
            else
            {
                comboBoxDepartment.Text = "";
            }

            if (textBoxPositionInput.InvokeRequired)
            {
                textBoxPositionInput.Invoke(new Action(() => textBoxPositionInput.Text = ""));
            }
            else
            {
                textBoxPositionInput.Text = "";
            }

            if (textBoxContractType.InvokeRequired)
            {
                textBoxContractType.Invoke(new Action(() => textBoxContractType.Text = ""));
            }
            else
            {
                textBoxContractType.Text = "";
            }

            if (textBoxDateOfJoining.InvokeRequired)
            {
                textBoxDateOfJoining.Invoke(new Action(() => textBoxDateOfJoining.Text = ""));
            }
            else
            {
                textBoxDateOfJoining.Text = "";
            }

            if (textBoxDateOfExit.InvokeRequired)
            {
                textBoxDateOfExit.Invoke(new Action(() => textBoxDateOfExit.Text = ""));
            }
            else
            {
                textBoxDateOfExit.Text = "";
            }

            if (textBoxManager.InvokeRequired)
            {
                textBoxManager.Invoke(new Action(() => textBoxManager.Text = ""));
            }
            else
            {
                textBoxManager.Text = "";
            }

            // Update password field accessibility
            UpdatePasswordFieldAccessibility();
        }
        private async Task LoadWorkScheduleData(string employeeId)
        {
            try
            {
                var allSchedules = await GetAllSchedules();
                var employeeSchedules = allSchedules.Where(s => s.employee_id == employeeId).ToList();

                // Reset UI elements
                System.Windows.Forms.CheckBox[] dayBoxes = {
            checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS,
            checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS
        };

                foreach (var cb in dayBoxes)
                {
                    cb.Checked = false;
                }

                textBoxWorkHoursA.Text = "";
                textBoxWorkHoursB.Text = "";
                textBoxAltWorkHoursA.Text = "";
                textBoxAltWorkHoursB.Text = "";

                // Populate UI with schedule data
                foreach (var schedule in employeeSchedules)
                {
                    if (schedule.schedule_type == "Regular")
                    {
                        textBoxWorkHoursA.Text = schedule.start_time ?? "";
                        textBoxWorkHoursB.Text = schedule.end_time ?? "";
                    }
                    else if (schedule.schedule_type == "Alternate")
                    {
                        textBoxAltWorkHoursA.Text = schedule.start_time ?? "";
                        textBoxAltWorkHoursB.Text = schedule.end_time ?? "";
                    }

                    switch (schedule.day_of_week?.ToLower())
                    {
                        case "monday":
                            if (schedule.schedule_type == "Regular")
                                checkBoxM.Checked = true;
                            else
                                checkBoxAltM.Checked = true;
                            break;
                        case "tuesday":
                            if (schedule.schedule_type == "Regular")
                                checkBoxT.Checked = true;
                            else
                                checkBoxAltT.Checked = true;
                            break;
                        case "wednesday":
                            if (schedule.schedule_type == "Regular")
                                checkBoxW.Checked = true;
                            else
                                checkBoxAltW.Checked = true;
                            break;
                        case "thursday":
                            if (schedule.schedule_type == "Regular")
                                checkBoxTh.Checked = true;
                            else
                                checkBoxAltTh.Checked = true;
                            break;
                        case "friday":
                            if (schedule.schedule_type == "Regular")
                                checkBoxF.Checked = true;
                            else
                                checkBoxAltF.Checked = true;
                            break;
                        case "saturday":
                            if (schedule.schedule_type == "Regular")
                                checkBoxS.Checked = true;
                            else
                                checkBoxAltS.Checked = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading work schedule: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadUserData(string employeeId)
        {
            try
            {
                // Find the user record that matches this employee ID
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                var user = users.FirstOrDefault(u => u.Object?.employee_id == employeeId);
                string currentDepartment = comboBoxDepartment.Text;

                if (currentDepartment == "Human Resource")
                {
                    // For HR department, show appropriate placeholder
                    if (user != null)
                    {
                        textboxPassword.Text = "••••••••";
                        textboxPassword.ForeColor = Color.Gray;
                        textboxPassword.UseSystemPasswordChar = false;
                    }
                    else
                    {
                        textboxPassword.Text = "No password set";
                        textboxPassword.ForeColor = Color.Gray;
                        textboxPassword.UseSystemPasswordChar = false;
                    }

                    // Add event handlers for placeholder behavior
                    textboxPassword.Enter += PasswordTextBox_Enter;
                    textboxPassword.Leave += PasswordTextBox_Leave;
                }
                else
                {
                    // For non-HR departments
                    if (user != null)
                    {
                        // User exists but department is not HR - this shouldn't normally happen
                        textboxPassword.Text = "User access will be revoked";
                        textboxPassword.ForeColor = Color.Orange;
                    }
                    else
                    {
                        textboxPassword.Text = "HR department only";
                        textboxPassword.ForeColor = Color.Gray;
                    }

                    textboxPassword.UseSystemPasswordChar = false;
                    textboxPassword.Enabled = false;
                    textboxPassword.BackColor = SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user data: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasswordTextBox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            // Only handle if department is HR and field is enabled
            if (comboBoxDepartment.Text == "Human Resource" && textBox.Enabled)
            {
                if (textBox.Text == "••••••••" || textBox.Text == "No password set" || textBox.Text == "HR department only")
                {
                    textBox.Text = "";
                    textBox.ForeColor = SystemColors.WindowText;
                    textBox.UseSystemPasswordChar = true;
                }
            }
        }

        private void PasswordTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            // Only handle if department is HR and field is enabled
            if (comboBoxDepartment.Text == "Human Resource" && textBox.Enabled)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Check if user exists to determine which placeholder to show
                    var users = firebase.Child("Users").OnceAsync<UserModel>().Result;
                    var userExists = users.Any(u => u.Object?.employee_id == selectedEmployeeId);

                    textBox.Text = userExists ? "••••••••" : "No password set";
                    textBox.ForeColor = Color.Gray;
                    textBox.UseSystemPasswordChar = false;
                }
                // If user typed something but then deleted it, show appropriate placeholder
                else if (textBox.Text.Length == 0)
                {
                    var users = firebase.Child("Users").OnceAsync<UserModel>().Result;
                    var userExists = users.Any(u => u.Object?.employee_id == selectedEmployeeId);

                    textBox.Text = userExists ? "••••••••" : "No password set";
                    textBox.ForeColor = Color.Gray;
                    textBox.UseSystemPasswordChar = false;
                }
            }
        }

        private void buttonChangePhoto_Click(object sender, EventArgs e)
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
                }
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonChangePhoto.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonUpdate.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                buttonScanRFID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
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
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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
                textboxPassword.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            using (ConfirmProfileUpdate confirmForm = new ConfirmProfileUpdate())
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                confirmForm.ShowDialog(this);

                if (confirmForm.UserConfirmed)
                {
                    await ExecuteFinalUpdate();
                }
                else
                {
                    MessageBox.Show("Update cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async Task ExecuteFinalUpdate()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Get current department and password info
                string newDepartment = comboBoxDepartment.Text.Trim();
                string password = textboxPassword.Text.Trim();

                // Check if password is actually a placeholder
                bool isPasswordPlaceholder = password == "••••••••" ||
                                           password == "No password set" ||
                                           password == "HR department only";

                // Validate HR department password requirement
                if (newDepartment == "Human Resource")
                {
                    if (string.IsNullOrEmpty(password) || isPasswordPlaceholder)
                    {
                        // Check if user already exists
                        var users = await firebase.Child("Users").OnceAsync<UserModel>();
                        var userExists = users.Any(u => u.Object?.employee_id == selectedEmployeeId);

                        if (!userExists)
                        {
                            MessageBox.Show("Password is required when assigning employee to Human Resource department.",
                                          "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        // If user exists and password is placeholder, preserve existing password
                    }
                }

                // Validate work schedule before proceeding
                if (!ValidateWorkSchedule())
                {
                    return;
                }

                // 1. Handle image upload first (if new image was selected)
                string imageUrl = null;
                if (!string.IsNullOrEmpty(localImagePath))
                {
                    imageUrl = await UploadImageToFirebase(localImagePath, selectedEmployeeId);
                }

                // 2. Update EmployeeDetails (imageUrl will be null if no new image, preserving existing)
                await UpdateEmployeeDetails(imageUrl);

                // 3. Update EmploymentInfo
                await UpdateEmploymentInfo();

                // 4. Handle User Management based on department change
                await HandleUserManagement(selectedEmployeeId, newDepartment, password, isPasswordPlaceholder);

                // 5. Update Work Schedule
                await UpdateWorkSchedule(selectedEmployeeId);

                // Add admin log for profile update
                await AddAdminLog("Employee Updated", selectedEmployeeId,
                                 $"Employee profile updated for {selectedEmployeeId}",
                                 $"Department: {newDepartment}");

                MessageBox.Show("Employee profile updated successfully!", "Success",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close(); // Close the form after successful update
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private async Task UpdateEmployeeDetails(string imageUrl = null)
        {
            try
            {
                // First, get the existing employee data to preserve all current values
                var existingEmployee = await firebase
                    .Child("EmployeeDetails")
                    .Child(selectedEmployeeId)
                    .OnceSingleAsync<EmployeeDetailsModel>();

                var employeeDetails = new EmployeeDetailsModel
                {
                    employee_id = selectedEmployeeId,
                    first_name = string.IsNullOrWhiteSpace(textBoxFirstName.Text) ? existingEmployee?.first_name ?? "" : textBoxFirstName.Text,
                    middle_name = string.IsNullOrWhiteSpace(textBoxMiddleName.Text) ? existingEmployee?.middle_name ?? "" : textBoxMiddleName.Text,
                    last_name = string.IsNullOrWhiteSpace(textBoxLastName.Text) ? existingEmployee?.last_name ?? "" : textBoxLastName.Text,
                    date_of_birth = string.IsNullOrWhiteSpace(textBoxDateOfBirth.Text) ? existingEmployee?.date_of_birth ?? "" : textBoxDateOfBirth.Text,
                    gender = string.IsNullOrWhiteSpace(textBoxGender.Text) ? existingEmployee?.gender ?? "" : textBoxGender.Text,
                    marital_status = string.IsNullOrWhiteSpace(textBoxMaritalStatus.Text) ? existingEmployee?.marital_status ?? "" : textBoxMaritalStatus.Text,
                    nationality = string.IsNullOrWhiteSpace(textBoxNationality.Text) ? existingEmployee?.nationality ?? "" : textBoxNationality.Text,
                    contact = string.IsNullOrWhiteSpace(textBoxContact.Text) ? existingEmployee?.contact ?? "" : textBoxContact.Text,
                    email = string.IsNullOrWhiteSpace(textBoxEmail.Text) ? existingEmployee?.email ?? "" : textBoxEmail.Text,
                    address = string.IsNullOrWhiteSpace(textBoxAddress.Text) ? existingEmployee?.address ?? "" : textBoxAddress.Text,
                    rfid_tag = string.IsNullOrWhiteSpace(labelRFIDTagInput.Text) || labelRFIDTagInput.Text == "Scan RFID Tag"
                              ? existingEmployee?.rfid_tag ?? ""
                              : labelRFIDTagInput.Text,
                    // Use new image URL if provided, otherwise keep existing image URL
                    image_url = !string.IsNullOrEmpty(imageUrl) ? imageUrl : (existingEmployee?.image_url ?? ""),
                    created_at = existingEmployee?.created_at ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("EmployeeDetails")
                    .Child(selectedEmployeeId)
                    .PutAsync(employeeDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating employee details: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private async Task UpdateEmploymentInfo()
        {
            try
            {
                // Get the current EmploymentInfo array
                var employmentArray = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<JArray>();

                JArray updatedArray = employmentArray ?? new JArray();
                int targetIndex = -1;

                // Find the employee's record in the array (skip null elements)
                for (int i = 0; i < updatedArray.Count; i++)
                {
                    if (updatedArray[i]?.Type == JTokenType.Null)
                        continue;

                    if (updatedArray[i]?.Type == JTokenType.Object)
                    {
                        var empObj = (JObject)updatedArray[i];
                        var empId = empObj["employee_id"]?.ToString();

                        if (empId == selectedEmployeeId)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }

                // Get existing values or defaults
                string existingPosition = "";
                string existingDepartment = "";
                string existingContractType = "";
                string existingDateOfJoining = "";
                string existingDateOfExit = "";
                string existingManagerName = "";
                string existingCreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (targetIndex >= 0 && updatedArray[targetIndex]?.Type == JTokenType.Object)
                {
                    var existingObj = (JObject)updatedArray[targetIndex];
                    existingPosition = existingObj["position"]?.ToString() ?? "";
                    existingDepartment = existingObj["department"]?.ToString() ?? "";
                    existingContractType = existingObj["contract_type"]?.ToString() ?? "";
                    existingDateOfJoining = existingObj["date_of_joining"]?.ToString() ?? "";
                    existingDateOfExit = existingObj["date_of_exit"]?.ToString() ?? "";
                    existingManagerName = existingObj["manager_name"]?.ToString() ?? "";
                    existingCreatedAt = existingObj["created_at"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // Create updated employment info
                var employmentInfo = new EmploymentInfoModel
                {
                    employee_id = selectedEmployeeId,
                    position = string.IsNullOrWhiteSpace(textBoxPositionInput.Text) ? existingPosition : textBoxPositionInput.Text,
                    department = string.IsNullOrWhiteSpace(comboBoxDepartment.Text) ? existingDepartment : comboBoxDepartment.Text,
                    contract_type = string.IsNullOrWhiteSpace(textBoxContractType.Text) ? existingContractType : textBoxContractType.Text,
                    date_of_joining = string.IsNullOrWhiteSpace(textBoxDateOfJoining.Text) ? existingDateOfJoining : textBoxDateOfJoining.Text,
                    date_of_exit = string.IsNullOrWhiteSpace(textBoxDateOfExit.Text) ? existingDateOfExit : textBoxDateOfExit.Text,
                    manager_name = string.IsNullOrWhiteSpace(textBoxManager.Text) ? existingManagerName : textBoxManager.Text,
                    created_at = existingCreatedAt
                };

                // Convert to JObject for Firebase
                var employmentJObject = JObject.FromObject(employmentInfo);

                if (targetIndex >= 0)
                {
                    // Update existing record
                    updatedArray[targetIndex] = employmentJObject;
                }
                else
                {
                    // Add new record - find first available slot or append
                    bool added = false;
                    for (int i = 0; i < updatedArray.Count; i++)
                    {
                        if (updatedArray[i]?.Type == JTokenType.Null)
                        {
                            updatedArray[i] = employmentJObject;
                            added = true;
                            break;
                        }
                    }

                    if (!added)
                    {
                        // Append to the end if no null slots found
                        updatedArray.Add(employmentJObject);
                    }
                }

                // Save the entire array back to Firebase
                await firebase
                    .Child("EmploymentInfo")
                    .PutAsync(updatedArray);

                Console.WriteLine("Employment info updated successfully in array format");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating employment info: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // COPIED FROM AddNewEmployee.cs - Password functionality
        private async Task UpdateUserPassword(string employeeId, string newPassword, bool isPasswordPlaceholder = false)
        {
            try
            {
                // Only proceed if department is HR
                if (comboBoxDepartment.Text != "Human Resource")
                {
                    return;
                }

                // Find the user record that matches this employee ID
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                var user = users.FirstOrDefault(u => u.Object?.employee_id == employeeId);

                if (user != null)
                {
                    // If password is null or placeholder "••••••••", preserve existing password
                    if (newPassword == null || isPasswordPlaceholder)
                    {
                        // Password remains unchanged - no update needed
                        System.Diagnostics.Debug.WriteLine("Password preserved - no changes made");
                        return;
                    }

                    // Generate salt and hash for the new password
                    string numericPart = employeeId.Split('-')[1]; // Extract "001" from "JAP-001"
                    string salt = "RANDOMSALT" + numericPart; // Format like "RANDOMSALT2" for JAP-002
                    string passwordHash = HashPassword(newPassword, salt);

                    // Update the user record with new password
                    var updatedUser = new UserModel
                    {
                        user_id = user.Object.user_id,
                        employee_id = user.Object.employee_id,
                        password_hash = passwordHash,
                        salt = salt,
                        isAdmin = user.Object.isAdmin,
                        created_at = user.Object.created_at
                    };

                    await firebase
                        .Child("Users")
                        .Child(user.Key)
                        .PutAsync(updatedUser);

                    System.Diagnostics.Debug.WriteLine("Password updated successfully");
                }
                else
                {
                    // Create new user record if it doesn't exist
                    // Only create if we have an actual password (not placeholder)
                    if (newPassword != null && !isPasswordPlaceholder)
                    {
                        await editEmployeeHR(employeeId, newPassword);
                    }
                    else
                    {
                        MessageBox.Show("Cannot create user account: Password is required for HR department employees.",
                                      "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating password: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async Task editEmployeeHR(string employeeId, string password)
        {
            try
            {
                // Generate a new user ID
                var users = await firebase.Child("Users").OnceAsync<UserModel>();
                int newUserId = users.Count + 1;

                // Generate salt and hash - USING COPIED LOGIC
                string numericPart = employeeId.Split('-')[1]; // Extract "001" from "JAP-001"
                string salt = "RANDOMSALT" + numericPart; // Format like "RANDOMSALT2" for JAP-002
                string passwordHash = HashPassword(password, salt);

                var newUser = new UserModel
                {
                    user_id = newUserId.ToString(),
                    employee_id = employeeId,
                    password_hash = passwordHash,
                    salt = salt,
                    isAdmin = "False",
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                await firebase
                    .Child("Users")
                    .Child(newUserId.ToString())
                    .PutAsync(newUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // COPIED FROM AddNewEmployee.cs - Password hashing methods
        private string GenerateRandomSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // COPIED FROM AddNewEmployee.cs
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                // Return uppercase hash to match JAP-002 format
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
        }

        // CORRECTED for Firebase Storage v7.3
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

        private async Task<bool> UpdateEmployeeImage(string employeeId, string imageUrl)
        {
            var updateData = new
            {
                image_url = imageUrl
            };

            string jsonBody = JsonConvert.SerializeObject(updateData);

            using (var http = new HttpClient())
            {
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                string[] pathsToTry = {
            "EmployeeDetails/" + employeeId + ".json",
            "employees/" + employeeId + ".json"
        };

                foreach (var path in pathsToTry)
                {
                    // Use the same URL as your FirebaseClient
                    string fullUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/" + path;

                    try
                    {
                        var response = await http.PatchAsync(fullUrl, content);
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show($"Failed at {path}: {response.StatusCode}\n{responseContent}", "Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating {path}: {ex.Message}", "Error");
                    }
                }

                return false;
            }
        }

        private void textboxPassword_TextChanged(object sender, EventArgs e)
        {
            // Only validate if department is HR and field is enabled
            if (comboBoxDepartment.Text == "Human Resource" && textboxPassword.Enabled)
            {
                if (!string.IsNullOrEmpty(textboxPassword.Text) && textboxPassword.Text != "••••••••" && textboxPassword.Text != "No password set" && textboxPassword.Text != "HR department only")
                {
                    // You can add password strength validation here
                    if (textboxPassword.Text.Length < 6)
                    {
                        // Weak password indicator
                        textboxPassword.ForeColor = Color.Red;
                    }
                    else
                    {
                        textboxPassword.ForeColor = Color.Green;
                    }
                }
                else
                {
                    textboxPassword.ForeColor = SystemColors.WindowText;
                }
            }
        }

        private void buttonScanRFID_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ScanRFID scanRFID = new ScanRFID(this); // This will use the new constructor
            AttributesClass.ShowWithOverlay(parentForm, scanRFID);
        }

        public void SetRFIDTag(string tag)
        {
            if (labelRFIDTagInput.InvokeRequired)
            {
                labelRFIDTagInput.Invoke(new Action(() => labelRFIDTagInput.Text = tag));
            }
            else
            {
                labelRFIDTagInput.Text = tag;
            }
        }

        private void comboBoxDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePasswordFieldAccessibility();
        }
        private void InitializeDepartmentComboBox()
        {
            // Add departments to combobox if not already done
            if (comboBoxDepartment.Items.Count == 0)
            {
                comboBoxDepartment.Items.AddRange(new string[] {
                    "Engineering",
                    "Purchasing",
                    "Operations",
                    "Finance",
                    "Human Resource"
                });
            }

        }
        private void UpdatePasswordFieldAccessibility()
        {
            string selectedDepartment = comboBoxDepartment.SelectedItem?.ToString();

            if (selectedDepartment == "Human Resource")
            {
                // Enable password field for HR department
                textboxPassword.Enabled = true;
                textboxPassword.BackColor = SystemColors.Window;
                textboxPassword.ForeColor = SystemColors.WindowText;

                // Clear any placeholder text when enabling
                if (textboxPassword.Text == "HR department only")
                {
                    textboxPassword.Text = "";
                    textboxPassword.UseSystemPasswordChar = true;
                }
            }
            else
            {
                // Disable password field for other departments
                textboxPassword.Enabled = false;
                textboxPassword.BackColor = SystemColors.Control;
                textboxPassword.ForeColor = SystemColors.GrayText;
                textboxPassword.UseSystemPasswordChar = false;
                textboxPassword.Text = "HR department only";
            }
        }
        private async Task UpdateWorkSchedule(string employeeId)
        {
            try
            {
                // First, get all existing schedules to find and delete this employee's schedules
                var allSchedules = await GetAllSchedules();

                // Delete existing schedules for this employee
                await DeleteEmployeeSchedules(employeeId, allSchedules);

                // Get the next available array index and schedule_id
                int nextArrayIndex = await GetNextScheduleArrayIndex(allSchedules);
                int nextScheduleId = await GetNextScheduleId(allSchedules);

                // Create schedules based on checkbox selections
                List<WorkScheduleModel> newSchedules = new List<WorkScheduleModel>();

                // Add regular schedules
                newSchedules.AddRange(CreateRegularSchedules(employeeId, ref nextScheduleId));

                // Add alternate schedules
                newSchedules.AddRange(CreateAlternateSchedules(employeeId, ref nextScheduleId));

                // Only save schedules if there are any to save
                if (newSchedules.Count > 0)
                {
                    // Save all new schedules to Firebase
                    foreach (var schedule in newSchedules)
                    {
                        await SaveSchedule(schedule, nextArrayIndex);
                        nextArrayIndex++;
                    }

                    // Show success message with schedule details
                    ShowScheduleCreationSummary(newSchedules);
                }
                else
                {
                    MessageBox.Show("No work schedules were created or updated.", "Information",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating work schedule: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> GetNextScheduleArrayIndex(List<WorkScheduleModel> allSchedules = null)
        {
            try
            {
                if (allSchedules == null)
                {
                    allSchedules = await GetAllSchedules();
                }
                return allSchedules.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next schedule array index: {ex.Message}");
                return 0;
            }
        }

        private List<WorkScheduleModel> CreateRegularSchedules(string employeeId, ref int nextScheduleId)
        {
            var schedules = new List<WorkScheduleModel>();
            string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            CheckBox[] regularCheckboxes = { checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS };

            string startTime = textBoxWorkHoursA.Text.Trim();
            string endTime = textBoxWorkHoursB.Text.Trim();

            // Only create schedules if times are provided
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                for (int i = 0; i < regularCheckboxes.Length; i++)
                {
                    if (regularCheckboxes[i].Checked)
                    {
                        var schedule = new WorkScheduleModel
                        {
                            schedule_id = nextScheduleId.ToString(),
                            employee_id = employeeId,
                            day_of_week = days[i],
                            start_time = startTime,
                            end_time = endTime,
                            schedule_type = "Regular",
                        };

                        schedules.Add(schedule);
                        nextScheduleId++;
                    }
                }
            }

            return schedules;
        }
        private void ShowScheduleCreationSummary(List<WorkScheduleModel> schedules)
        {
            if (schedules.Count == 0)
            {
                MessageBox.Show("No work schedules were created.", "Information",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var regularSchedules = schedules.Where(s => s.schedule_type == "Regular").ToList();
            var alternateSchedules = schedules.Where(s => s.schedule_type == "Alternate").ToList();

            string message = $"Work schedules created successfully!\n\n";

            if (regularSchedules.Any())
            {
                message += $"Regular Schedule:\n";
                foreach (var schedule in regularSchedules)
                {
                    message += $"  Schedule {schedule.schedule_id}: {schedule.day_of_week}, {schedule.start_time}-{schedule.end_time}\n";
                }
            }

            if (alternateSchedules.Any())
            {
                message += $"\nAlternate Schedule:\n";
                foreach (var schedule in alternateSchedules)
                {
                    message += $"  Schedule {schedule.schedule_id}: {schedule.day_of_week}, {schedule.start_time}-{schedule.end_time}\n";
                }
            }

            MessageBox.Show(message, "Schedule Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private List<WorkScheduleModel> CreateAlternateSchedules(string employeeId, ref int nextScheduleId)
        {
            var schedules = new List<WorkScheduleModel>();
            string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            CheckBox[] altCheckboxes = { checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS };

            string startTime = textBoxAltWorkHoursA.Text.Trim();
            string endTime = textBoxAltWorkHoursB.Text.Trim();

            // Only create schedules if times are provided
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                for (int i = 0; i < altCheckboxes.Length; i++)
                {
                    if (altCheckboxes[i].Checked)
                    {
                        var schedule = new WorkScheduleModel
                        {
                            schedule_id = nextScheduleId.ToString(),
                            employee_id = employeeId,
                            day_of_week = days[i],
                            start_time = startTime,
                            end_time = endTime,
                            schedule_type = "Alternate",
                        };

                        schedules.Add(schedule);
                        nextScheduleId++;
                    }
                }
            }

            return schedules;
        }
        private async Task<int> GetNextScheduleId(List<WorkScheduleModel> allSchedules = null)
        {
            try
            {
                if (allSchedules == null)
                {
                    allSchedules = await GetAllSchedules();
                }

                int maxId = 0;
                foreach (var schedule in allSchedules)
                {
                    if (schedule != null && int.TryParse(schedule.schedule_id, out int id))
                    {
                        if (id > maxId)
                            maxId = id;
                    }
                }

                return maxId + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next schedule ID: {ex.Message}");
                return 1;
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
        private bool ValidateWorkSchedule()
        {
            // Check if at least one day is selected
            CheckBox[] allCheckboxes = {
                checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS,
                checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS
            };

            bool hasSelectedDays = allCheckboxes.Any(cb => cb.Checked);

            if (!hasSelectedDays)
            {
                MessageBox.Show("Please select at least one work day for the employee.",
                               "Work Schedule Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if regular schedule has time when days are selected
            CheckBox[] regularDays = { checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS };
            bool hasRegularDays = regularDays.Any(cb => cb.Checked);

            if (hasRegularDays && (string.IsNullOrEmpty(textBoxWorkHoursA.Text) || string.IsNullOrEmpty(textBoxWorkHoursB.Text)))
            {
                MessageBox.Show("Please enter work hours for the selected regular days.",
                               "Work Hours Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if alternate schedule has time when days are selected
            CheckBox[] altDays = { checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS };
            bool hasAltDays = altDays.Any(cb => cb.Checked);

            if (hasAltDays && (string.IsNullOrEmpty(textBoxAltWorkHoursA.Text) || string.IsNullOrEmpty(textBoxAltWorkHoursB.Text)))
            {
                MessageBox.Show("Please enter work hours for the selected alternate days.",
                               "Work Hours Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        private async Task<List<WorkScheduleModel>> GetAllSchedules()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        // If empty or null, return empty list
                        if (string.IsNullOrEmpty(json) || json == "null")
                            return new List<WorkScheduleModel>();

                        // Handle both array and object formats
                        if (json.Trim().StartsWith("["))
                        {
                            // Array format
                            var schedulesArray = JsonConvert.DeserializeObject<JArray>(json);
                            var schedules = new List<WorkScheduleModel>();

                            if (schedulesArray != null)
                            {
                                foreach (var item in schedulesArray)
                                {
                                    if (item != null && item.Type != JTokenType.Null)
                                    {
                                        try
                                        {
                                            var schedule = item.ToObject<WorkScheduleModel>();
                                            if (schedule != null)
                                            {
                                                schedules.Add(schedule);
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                            return schedules;
                        }
                        else
                        {
                            // Object/collection format
                            var schedulesDict = JsonConvert.DeserializeObject<Dictionary<string, WorkScheduleModel>>(json);
                            return schedulesDict?.Values.ToList() ?? new List<WorkScheduleModel>();
                        }
                    }
                }
                return new List<WorkScheduleModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all schedules: {ex.Message}");
                return new List<WorkScheduleModel>();
            }
        }

        private async Task DeleteEmployeeSchedules(string employeeId, List<WorkScheduleModel> allSchedules)
        {
            try
            {
                // First, let's check the current structure
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(json) || json == "null")
                            return;

                        if (json.Trim().StartsWith("["))
                        {
                            // Array structure - we need to rebuild the entire array without this employee's schedules
                            var schedulesArray = JsonConvert.DeserializeObject<JArray>(json);
                            if (schedulesArray == null) return;

                            var newSchedulesArray = new JArray();

                            foreach (var item in schedulesArray)
                            {
                                if (item != null && item.Type != JTokenType.Null)
                                {
                                    try
                                    {
                                        var schedule = item.ToObject<WorkScheduleModel>();
                                        if (schedule != null && schedule.employee_id != employeeId)
                                        {
                                            newSchedulesArray.Add(item);
                                        }
                                    }
                                    catch
                                    {
                                        // Keep invalid entries to avoid data loss
                                        newSchedulesArray.Add(item);
                                    }
                                }
                            }

                            // Put the modified array back
                            var newJson = JsonConvert.SerializeObject(newSchedulesArray);
                            var content = new StringContent(newJson, Encoding.UTF8, "application/json");
                            await httpClient.PutAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json", content);
                        }
                        else
                        {
                            // Object structure - delete individual schedule entries
                            var allSchedulesResponse = await firebase
                                .Child("Work_Schedule")
                                .OnceAsync<object>();

                            if (allSchedulesResponse != null)
                            {
                                foreach (var scheduleItem in allSchedulesResponse)
                                {
                                    if (scheduleItem.Object != null && scheduleItem.Object is JObject jobject)
                                    {
                                        try
                                        {
                                            var schedule = jobject.ToObject<WorkScheduleModel>();
                                            if (schedule != null && schedule.employee_id == employeeId)
                                            {
                                                await firebase
                                                    .Child("Work_Schedule")
                                                    .Child(scheduleItem.Key)
                                                    .DeleteAsync();
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting employee schedules: {ex.Message}");
                throw;
            }
        }

        private async Task SaveSchedule(WorkScheduleModel schedule, int arrayIndex)
        {
            try
            {
                // Check current structure first
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(json) || json == "null" || json.Trim().StartsWith("["))
                        {
                            // Array structure - we need to handle this differently
                            await SaveScheduleToArray(schedule, arrayIndex);
                        }
                        else
                        {
                            // Object structure - use FirebaseClient
                            await firebase
                                .Child("Work_Schedule")
                                .Child(arrayIndex.ToString())
                                .PutAsync(schedule);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving schedule: {ex.Message}");
                throw;
            }
        }

        private async Task SaveScheduleToArray(WorkScheduleModel schedule, int arrayIndex)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // First, get the current array
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    JArray schedulesArray;

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(json) || json == "null")
                        {
                            schedulesArray = new JArray();
                        }
                        else
                        {
                            schedulesArray = JsonConvert.DeserializeObject<JArray>(json) ?? new JArray();
                        }
                    }
                    else
                    {
                        schedulesArray = new JArray();
                    }

                    // Ensure the array is large enough
                    while (schedulesArray.Count <= arrayIndex)
                    {
                        schedulesArray.Add(JValue.CreateNull());
                    }

                    // Replace the element at the specified index
                    schedulesArray[arrayIndex] = JObject.FromObject(schedule);

                    // Put the modified array back
                    var newJson = JsonConvert.SerializeObject(schedulesArray);
                    var content = new StringContent(newJson, Encoding.UTF8, "application/json");
                    await httpClient.PutAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json", content);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving schedule to array: {ex.Message}");
                throw;
            }
        }
        // NEW METHOD: Handle user creation/revocation based on department
        private async Task HandleUserManagement(string employeeId, string newDepartment, string password, bool isPasswordPlaceholder)
        {
            try
            {
                // Find existing user for this employee
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                var existingUser = users.FirstOrDefault(u => u.Object?.employee_id == employeeId);

                if (newDepartment == "Human Resource")
                {
                    // Department is HR - ensure user exists
                    if (existingUser != null)
                    {
                        // User already exists, update password if provided
                        if (!isPasswordPlaceholder && !string.IsNullOrEmpty(password))
                        {
                            await UpdateExistingUserPassword(existingUser, password);
                        }
                        // If password is placeholder, preserve existing password
                    }
                    else
                    {
                        // Create new user for HR department
                        if (!isPasswordPlaceholder && !string.IsNullOrEmpty(password))
                        {
                            await CreateNewUserForHR(employeeId, password);
                        }
                        else
                        {
                            MessageBox.Show("Password is required when assigning employee to Human Resource department.",
                                          "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    // Department is NOT HR - revoke user if exists
                    if (existingUser != null)
                    {
                        await RevokeUserAccess(existingUser);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing user access: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NEW METHOD: Revoke user access when department changes from HR to non-HR
        private async Task RevokeUserAccess(FirebaseObject<UserModel> user)
        {
            try
            {
                await firebase
                    .Child("Users")
                    .Child(user.Key)
                    .DeleteAsync();

                // Add admin log for user revocation
                await AddAdminLog("User Revoked", user.Object.employee_id,
                                 $"User access revoked due to department change from HR to non-HR",
                                 $"User ID: {user.Object.user_id} was removed from Users");

                System.Diagnostics.Debug.WriteLine($"User access revoked for employee: {user.Object.employee_id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error revoking user access: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // NEW METHOD: Create new user for HR department
        private async Task CreateNewUserForHR(string employeeId, string password)
        {
            try
            {
                // Generate a new user ID
                var allUsers = await firebase.Child("Users").OnceAsync<UserModel>();
                int maxUserId = 0;

                foreach (var user in allUsers)
                {
                    if (int.TryParse(user.Object?.user_id, out int userId))
                    {
                        if (userId > maxUserId) maxUserId = userId;
                    }
                }

                string newUserId = (maxUserId + 1).ToString();

                // Generate salt and hash
                string numericPart = employeeId.Split('-')[1];
                string salt = "RANDOMSALT" + numericPart;
                string passwordHash = HashPassword(password, salt);

                var newUser = new UserModel
                {
                    user_id = newUserId,
                    employee_id = employeeId,
                    password_hash = passwordHash,
                    salt = salt,
                    isAdmin = "False",
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                await firebase
                    .Child("Users")
                    .Child(newUserId)
                    .PutAsync(newUser);

                // Add admin log for user creation
                await AddAdminLog("User Created", employeeId,
                                 $"User account created for HR department employee",
                                 $"User ID: {newUserId} created for Employee ID: {employeeId}");

                System.Diagnostics.Debug.WriteLine($"New user created for HR employee: {employeeId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // NEW METHOD: Update existing user password
        private async Task UpdateExistingUserPassword(FirebaseObject<UserModel> user, string newPassword)
        {
            try
            {
                string numericPart = user.Object.employee_id.Split('-')[1];
                string salt = "RANDOMSALT" + numericPart;
                string passwordHash = HashPassword(newPassword, salt);

                var updatedUser = new UserModel
                {
                    user_id = user.Object.user_id,
                    employee_id = user.Object.employee_id,
                    password_hash = passwordHash,
                    salt = salt,
                    isAdmin = user.Object.isAdmin,
                    created_at = user.Object.created_at
                };

                await firebase
                    .Child("Users")
                    .Child(user.Key)
                    .PutAsync(updatedUser);

                System.Diagnostics.Debug.WriteLine($"Password updated for user: {user.Object.user_id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating password: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // NEW METHOD: Add admin log (similar to AddNewEmployee)
        private async Task AddAdminLog(string actionType, string targetEmployeeId, string description, string details = "")
        {
            try
            {
                // Get current admin info - you might want to pass this from parent form
                string adminEmployeeId = "JAP-001"; // Default admin from your JSON
                string adminName = "Franz Louies Deloritos";
                string adminUserId = "101";

                var adminLog = new
                {
                    action_type = actionType,
                    admin_employee_id = adminEmployeeId,
                    admin_name = adminName,
                    admin_user_id = adminUserId,
                    description = description,
                    details = string.IsNullOrEmpty(details) ?
                             $"Employee ID: {targetEmployeeId}, Updated by: {adminName} (User ID: {adminUserId})" :
                             details,
                    target_employee_id = targetEmployeeId,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase.Child("AdminLogs").PostAsync(adminLog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding admin log: {ex.Message}");
            }
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
        public string image_url { get; set; } // Added image_url field
        public string created_at { get; set; }
    }

    public class EmploymentInfoModel
    {
        public string employment_id { get; set; }
        public string employee_id { get; set; }
        public string position { get; set; }
        public string department { get; set; }
        public string contract_type { get; set; }
        public string date_of_joining { get; set; }
        public string date_of_exit { get; set; }
        public string manager_name { get; set; }
        public string created_at { get; set; }
    }

    public class UserModel
    {
        public string user_id { get; set; }
        public string employee_id { get; set; }
        public string password_hash { get; set; }
        public string salt { get; set; }
        public string isAdmin { get; set; }
        public string created_at { get; set; }
    }
    public class WorkScheduleModel
    {
        public string schedule_id { get; set; }
        public string employee_id { get; set; }
        public string day_of_week { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string schedule_type { get; set; }
    }
}