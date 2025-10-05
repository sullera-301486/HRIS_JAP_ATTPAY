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
                var allEmploymentInfo = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<EmploymentInfoModel>();

                // Find the employment info that matches this employee ID
                var empInfo = allEmploymentInfo?
                    .FirstOrDefault(e => e.Object?.employee_id == employeeId)?.Object;

                if (empInfo != null)
                {
                    comboBoxDepartment.Text = empInfo.department ?? "";
                    labelPositionInput.Text = empInfo.position ?? "";
                    textBoxContractType.Text = empInfo.contract_type ?? "";
                    textBoxDateOfJoining.Text = empInfo.date_of_joining ?? "";
                    textBoxDateOfExit.Text = empInfo.date_of_exit ?? "";
                    textBoxManager.Text = empInfo.manager_name ?? "";
                }
                else
                {
                    // If no employment info found, clear the fields
                    comboBoxDepartment.Text = "";
                    labelPositionInput.Text = "";
                    textBoxContractType.Text = "";
                    textBoxDateOfJoining.Text = "";
                    textBoxDateOfExit.Text = "";
                    textBoxManager.Text = "";
                }

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

                // Check if department is HR to determine password field behavior
                bool isHRDepartment = comboBoxDepartment.Text == "Human Resource";

                if (user != null)
                {
                    if (isHRDepartment)
                    {
                        // For HR department, show placeholder but allow editing
                        textboxPassword.Text = "••••••••";
                        textboxPassword.ForeColor = Color.Gray;
                        textboxPassword.UseSystemPasswordChar = false;

                        // Add event handlers to handle placeholder behavior
                        textboxPassword.Enter += PasswordTextBox_Enter;
                        textboxPassword.Leave += PasswordTextBox_Leave;
                    }
                    else
                    {
                        // For non-HR departments, show disabled state
                        textboxPassword.Text = "HR department only";
                        textboxPassword.ForeColor = Color.Gray;
                        textboxPassword.UseSystemPasswordChar = false;
                        textboxPassword.Enabled = false;
                        textboxPassword.BackColor = SystemColors.Control;
                    }
                }
                else
                {
                    if (isHRDepartment)
                    {
                        // For HR department without password, show placeholder but allow editing
                        textboxPassword.Text = "No password set";
                        textboxPassword.ForeColor = Color.Gray;
                        textboxPassword.UseSystemPasswordChar = false;

                        // Add event handlers to handle placeholder behavior
                        textboxPassword.Enter += PasswordTextBox_Enter;
                        textboxPassword.Leave += PasswordTextBox_Leave;
                    }
                    else
                    {
                        // For non-HR departments, show disabled state
                        textboxPassword.Text = "HR department only";
                        textboxPassword.ForeColor = Color.Gray;
                        textboxPassword.UseSystemPasswordChar = false;
                        textboxPassword.Enabled = false;
                        textboxPassword.BackColor = SystemColors.Control;
                    }
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
            using (EditAttendanceConfirm confirmForm = new EditAttendanceConfirm())
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

                // Validate HR department password requirement
                string department = comboBoxDepartment.Text.Trim();
                string password = textboxPassword.Text.Trim();

                // Check if password is actually a placeholder and should be preserved
                bool isPasswordPlaceholder = password == "••••••••" ||
                                           password == "No password set" ||
                                           password == "HR department only";

                if (department == "Human Resource")
                {
                    if (string.IsNullOrEmpty(password) || isPasswordPlaceholder)
                    {
                        // For HR department, we need to ensure password is preserved
                        if (password == "••••••••")
                        {
                            // This means there's an existing password that should be preserved
                            // We'll handle this in UpdateUserPassword by not changing the password
                            password = null; // Special indicator to preserve existing password
                        }
                        else if (password == "No password set" || string.IsNullOrEmpty(password))
                        {
                            MessageBox.Show("Password is required for Human Resource department employees.",
                                          "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
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

                // 4. Update Work Schedule
                await UpdateWorkSchedule(selectedEmployeeId);

                // 5. Update Password handling - only for HR department
                if (department == "Human Resource")
                {
                    await UpdateUserPassword(selectedEmployeeId, password, isPasswordPlaceholder);
                }

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
                // First, get the existing employment info to preserve the employment_id and existing values
                var allEmploymentInfo = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<EmploymentInfoModel>();

                var existingEmpInfo = allEmploymentInfo?
                    .FirstOrDefault(e => e.Object?.employee_id == selectedEmployeeId);

                string employmentId;

                if (existingEmpInfo != null)
                {
                    // Use existing employment_id
                    employmentId = existingEmpInfo.Key;
                }
                else
                {
                    // Generate new employment_id (extract number from employee_id)
                    employmentId = selectedEmployeeId.Split('-')[1]; // Gets "001" from "JAP-001"
                }

                var employmentInfo = new EmploymentInfoModel
                {
                    employee_id = selectedEmployeeId,
                    position = string.IsNullOrWhiteSpace(labelPositionInput.Text) ? existingEmpInfo?.Object?.position ?? "" : labelPositionInput.Text,
                    department = string.IsNullOrWhiteSpace(comboBoxDepartment.Text) ? existingEmpInfo?.Object?.department ?? "" : comboBoxDepartment.Text,
                    contract_type = string.IsNullOrWhiteSpace(textBoxContractType.Text) ? existingEmpInfo?.Object?.contract_type ?? "" : textBoxContractType.Text,
                    date_of_joining = string.IsNullOrWhiteSpace(textBoxDateOfJoining.Text) ? existingEmpInfo?.Object?.date_of_joining ?? "" : textBoxDateOfJoining.Text,
                    date_of_exit = string.IsNullOrWhiteSpace(textBoxDateOfExit.Text) ? existingEmpInfo?.Object?.date_of_exit ?? "" : textBoxDateOfExit.Text,
                    manager_name = string.IsNullOrWhiteSpace(textBoxManager.Text) ? existingEmpInfo?.Object?.manager_name ?? "" : textBoxManager.Text,
                    created_at = existingEmpInfo?.Object?.created_at ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("EmploymentInfo")
                    .Child(employmentId)
                    .PutAsync(employmentInfo);
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