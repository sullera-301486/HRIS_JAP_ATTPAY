using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
                    }
                    else
                    {
                        box.BackColor = Color.FromArgb(217, 217, 217); // Unselected
                        box.ForeColor = Color.Black;
                    }
                };

                cb.Checked = false;
            }

            // 🔹 Load employee data if an ID was passed
            if (!string.IsNullOrEmpty(selectedEmployeeId))
            {
                await LoadEmployeeData(selectedEmployeeId);
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

                // ✅ Load User data for password
                await LoadUserData(employeeId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee data: " + ex.Message,
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
                if (user != null)
                {
                    // Show placeholder text when no password is entered
                    textboxPassword.Text = "••••••••";
                    textboxPassword.ForeColor = Color.Gray;

                    // Add event handlers to handle placeholder behavior
                    textboxPassword.Enter += PasswordTextBox_Enter;
                    textboxPassword.Leave += PasswordTextBox_Leave;
                }
                else
                {
                    textboxPassword.Text = "No password set";
                    textboxPassword.ForeColor = Color.Gray;

                    // Add event handlers to handle placeholder behavior
                    textboxPassword.Enter += PasswordTextBox_Enter;
                    textboxPassword.Leave += PasswordTextBox_Leave;
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
            if (textBox.Text == "••••••••" || textBox.Text == "No password set")
            {
                textBox.Text = "";
                textBox.ForeColor = SystemColors.WindowText;
            }
        }

        private void PasswordTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Determine which placeholder to show based on user existence
                var users = firebase.Child("Users").OnceAsync<UserModel>().Result;
                var userExists = users.Any(u => u.Object?.employee_id == selectedEmployeeId);

                textBox.Text = userExists ? "••••••••" : "No password set";
                textBox.ForeColor = Color.Gray;
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
                // 1. Handle image upload first (if new image was selected)
                string imageUrl = null;
                if (!string.IsNullOrEmpty(localImagePath))
                {
                    imageUrl = await UploadImageToFirebase(localImagePath, selectedEmployeeId);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        await UpdateEmployeeImage(selectedEmployeeId, imageUrl);
                    }
                }

                // 2. Update EmployeeDetails
                await UpdateEmployeeDetails(imageUrl);

                // 3. Update EmploymentInfo
                await UpdateEmploymentInfo();

                // 4. Update Password if provided - USING THE COPIED LOGIC FROM AddNewEmployee
                if (!string.IsNullOrEmpty(textboxPassword.Text.Trim()))
                {
                    await UpdateUserPassword(selectedEmployeeId, textboxPassword.Text.Trim());
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
        }

        private async Task UpdateEmployeeDetails(string imageUrl = null)
        {
            var employeeDetails = new EmployeeDetailsModel
            {
                employee_id = selectedEmployeeId,
                first_name = textBoxFirstName.Text,
                middle_name = textBoxMiddleName.Text,
                last_name = textBoxLastName.Text,
                date_of_birth = textBoxDateOfBirth.Text,
                gender = textBoxGender.Text,
                marital_status = textBoxMaritalStatus.Text,
                nationality = textBoxNationality.Text,
                contact = textBoxContact.Text,
                email = textBoxEmail.Text,
                address = textBoxAddress.Text,
                rfid_tag = labelRFIDTagInput.Text,
                image_url = imageUrl, // Use the provided imageUrl
                created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await firebase
                .Child("EmployeeDetails")
                .Child(selectedEmployeeId)
                .PutAsync(employeeDetails);
        }

        private async Task UpdateEmploymentInfo()
        {
            var employmentInfo = new EmploymentInfoModel
            {
                employee_id = selectedEmployeeId,
                position = labelPositionInput.Text,
                department = comboBoxDepartment.Text,
                contract_type = textBoxContractType.Text,
                date_of_joining = textBoxDateOfJoining.Text,
                date_of_exit = textBoxDateOfExit.Text,
                manager_name = textBoxManager.Text,
                created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await firebase
                .Child("EmploymentInfo")
                .Child(selectedEmployeeId)
                .PutAsync(employmentInfo);
        }

        // COPIED FROM AddNewEmployee.cs - Password functionality
        private async Task UpdateUserPassword(string employeeId, string newPassword)
        {
            try
            {
                // Find the user record that matches this employee ID
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                var user = users.FirstOrDefault(u => u.Object?.employee_id == employeeId);

                if (user != null)
                {
                    // Generate salt and hash for the new password - USING COPIED LOGIC
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
                }
                else
                {
                    // Create new user record if it doesn't exist - USING COPIED LOGIC
                    await CreateNewUser(employeeId, newPassword);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating password: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // COPIED FROM AddNewEmployee.cs
        private async Task CreateNewUser(string employeeId, string password)
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
            // Optional: Add real-time validation or strength indicator
            if (!string.IsNullOrEmpty(textboxPassword.Text))
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
}