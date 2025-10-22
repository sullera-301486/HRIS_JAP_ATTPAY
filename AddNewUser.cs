using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class AddNewUser : Form
    {
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string selectedEmployeeId;
        private string generatedUserId;

        public AddNewUser()
        {
            InitializeComponent();
            setFont();
            LoadHREmployees();
        }

        private async void LoadHREmployees()
        {
            try
            {
                cbEmpID.Items.Clear();

                // Load only HR department employees
                var employmentInfo = await firebase
                    .Child("EmploymentInfo")
                    .OnceAsync<object>();

                var hrEmployees = new List<string>();

                foreach (var item in employmentInfo)
                {
                    if (item?.Object == null) continue;

                    try
                    {
                        var empObj = JObject.FromObject(item.Object);
                        var department = empObj["department"]?.ToString();
                        var employeeId = empObj["employee_id"]?.ToString();

                        if (department == "Human Resource" && !string.IsNullOrEmpty(employeeId))
                        {
                            hrEmployees.Add(employeeId);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // Also check EmployeeDetails for any additional HR employees
                var employeeDetails = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<EmployeeDetailsModel>();

                foreach (var employee in employeeDetails)
                {
                    if (employee?.Object != null && !hrEmployees.Contains(employee.Object.employee_id))
                    {
                        // Check if this employee is in HR department by cross-referencing
                        var empId = employee.Object.employee_id;
                        var isInHR = employmentInfo.Any(e =>
                        {
                            if (e?.Object == null) return false;
                            try
                            {
                                var empObj = JObject.FromObject(e.Object);
                                return empObj["employee_id"]?.ToString() == empId &&
                                       empObj["department"]?.ToString() == "Human Resource";
                            }
                            catch
                            {
                                return false;
                            }
                        });

                        if (isInHR)
                        {
                            hrEmployees.Add(empId);
                        }
                    }
                }

                // Add sorted employee IDs to combobox
                cbEmpID.Items.AddRange(hrEmployees.OrderBy(id => id).ToArray());

                if (cbEmpID.Items.Count > 0)
                {
                    cbEmpID.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No employees found in Human Resource department.",
                                  "No HR Employees",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                    cbEmpID.Enabled = false;
                    comboBoxUserType.Enabled = false;
                    tbPassword.Enabled = false;
                    buttonConfirm.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading HR employees: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                labelRequestConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                labelUserType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                comboBoxUserType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                cbEmpID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                tbPassword.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                lblEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                lblPassword.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                lblUserID.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                // Removed lblUserIDD reference since we're using lblUserID now
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void cbEmpID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedEmployeeId = cbEmpID.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selectedEmployeeId))
                    return;

                // Check if user already exists
                var existingUser = await GetExistingUser(selectedEmployeeId);

                if (existingUser != null)
                {
                    // User exists - show existing user ID
                    lblUserID.Text = existingUser.Object.user_id;
                    lblUserID.ForeColor = Color.Blue;

                    // Set user type based on existing user
                    comboBoxUserType.SelectedItem = existingUser.Object.isAdmin == "True" ? "Admin" : "Staff";
                }
                else
                {
                    // Generate new user ID
                    generatedUserId = await GenerateNewUserId();
                    lblUserID.Text = generatedUserId;
                    lblUserID.ForeColor = Color.Green;

                    // Default to Staff for new users
                    comboBoxUserType.SelectedIndex = 1; // Default to Staff
                }

                // Always enable user type selection for HR employees
                if (comboBoxUserType.Items.Count == 0)
                {
                    comboBoxUserType.Items.AddRange(new string[] { "Admin", "Staff", "Revoke" });
                }
                comboBoxUserType.Enabled = true;

                // Enable password field
                tbPassword.Enabled = true;
                tbPassword.BackColor = SystemColors.Window;
                tbPassword.Text = "Enter password...";
                tbPassword.ForeColor = Color.Gray;
                tbPassword.UseSystemPasswordChar = false;

                // Add event handlers for placeholder behavior
                tbPassword.Enter -= PasswordTextBox_Enter;
                tbPassword.Leave -= PasswordTextBox_Leave;
                tbPassword.Enter += PasswordTextBox_Enter;
                tbPassword.Leave += PasswordTextBox_Leave;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employee data: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> GenerateNewUserId()
        {
            try
            {
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                int maxUserId = 100; // Starting point based on your data

                foreach (var user in users)
                {
                    if (int.TryParse(user.Object?.user_id, out int userId))
                    {
                        if (userId > maxUserId)
                            maxUserId = userId;
                    }
                }

                return (maxUserId + 1).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating user ID: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "101"; // Fallback
            }
        }

        private void PasswordTextBox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Enter password...")
            {
                textBox.Text = "";
                textBox.ForeColor = SystemColors.WindowText;
                textBox.UseSystemPasswordChar = true;
            }
        }

        private void PasswordTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter password...";
                textBox.ForeColor = Color.Gray;
                textBox.UseSystemPasswordChar = false;
            }
        }

        private async void comboBoxUserType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedType = comboBoxUserType.SelectedItem?.ToString();

                if (selectedType == "Revoke")
                {
                    // Hide password field for revoke
                    tbPassword.Visible = false;
                    lblPassword.Visible = false;

                    // Update user ID display for revoke action
                    var existingUser = await GetExistingUser(selectedEmployeeId);
                    if (existingUser != null)
                    {
                        lblUserID.Text = existingUser.Object.user_id;
                        lblUserID.ForeColor = Color.Red;
                    }
                }
                else
                {
                    // Show password field for Admin/Staff
                    tbPassword.Visible = true;
                    lblPassword.Visible = true;

                    // Update user ID display for create/update action
                    var existingUser = await GetExistingUser(selectedEmployeeId);
                    if (existingUser != null)
                    {
                        lblUserID.Text = existingUser.Object.user_id;
                        lblUserID.ForeColor = Color.Blue;
                    }
                    else
                    {
                        lblUserID.Text = generatedUserId ?? await GenerateNewUserId();
                        lblUserID.ForeColor = Color.Green;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user type: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            string selectedType = comboBoxUserType.SelectedItem?.ToString();

            // Only validate password for Admin/Staff (not Revoke)
            if (selectedType != "Revoke" &&
                !string.IsNullOrEmpty(tbPassword.Text) &&
                tbPassword.Text != "Enter password...")
            {
                if (tbPassword.Text.Length < 6)
                {
                    tbPassword.ForeColor = Color.Red;
                }
                else if (tbPassword.Text.Length < 8)
                {
                    tbPassword.ForeColor = Color.Orange;
                }
                else
                {
                    tbPassword.ForeColor = Color.Green;
                }
            }
        }

        private async Task HandleRevokeAction()
        {
            var existingUser = await GetExistingUser(selectedEmployeeId);

            if (existingUser == null)
            {
                MessageBox.Show($"No user account found for employee {selectedEmployeeId}.",
                              "No User Found",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                await RevokeUserAccess(existingUser);

                MessageBox.Show($"User privileges successfully revoked for {selectedEmployeeId}!",
                              "Success",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error revoking user privileges: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private async Task HandleCreateUpdateAction(string userType)
        {
            string password = tbPassword.Text.Trim();
            bool isPasswordPlaceholder = password == "Enter password...";

            if (string.IsNullOrEmpty(password) || isPasswordPlaceholder)
            {
                MessageBox.Show("Please enter a password for the user account.", "Password Required",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Weak Password",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                bool isAdmin = userType == "Admin";
                var existingUser = await GetExistingUser(selectedEmployeeId);

                string userIdToUse;
                if (existingUser != null)
                {
                    // Update existing user - use existing user ID
                    userIdToUse = existingUser.Object.user_id;
                    await UpdateExistingUser(existingUser, password, isAdmin);
                    MessageBox.Show($"User account updated successfully for {selectedEmployeeId} as {userType}!",
                                  "Success",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
                else
                {
                    // Create new user - use generated user ID
                    userIdToUse = generatedUserId ?? await GenerateNewUserId();
                    await CreateNewUser(selectedEmployeeId, password, isAdmin, userIdToUse);
                    MessageBox.Show($"User account created successfully for {selectedEmployeeId} as {userType}!",
                                  "Success",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }

                // Add admin log
                await AddAdminLog("User Created", selectedEmployeeId,
                                $"User account created for {selectedEmployeeId} as {userType}",
                                $"Employee ID: {selectedEmployeeId}, User Type: {userType}, User ID: {userIdToUse}");

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing user account: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private async Task<FirebaseObject<UserModel>> GetExistingUser(string employeeId)
        {
            try
            {
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                return users.FirstOrDefault(u => u.Object?.employee_id == employeeId);
            }
            catch
            {
                return null;
            }
        }

        private async Task CreateNewUser(string employeeId, string password, bool isAdmin, string userId)
        {
            // Generate salt and hash
            string numericPart = employeeId.Split('-')[1];
            string salt = "RANDOMSALT" + numericPart;
            string passwordHash = HashPassword(password, salt);

            var newUser = new UserModel
            {
                user_id = userId,
                employee_id = employeeId,
                password_hash = passwordHash,
                salt = salt,
                isAdmin = isAdmin ? "True" : "False",
                created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt").ToLower()
            };

            await firebase
                .Child("Users")
                .Child(userId)
                .PutAsync(newUser);
        }

        private async Task UpdateExistingUser(FirebaseObject<UserModel> user, string newPassword, bool isAdmin)
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
                isAdmin = isAdmin ? "True" : "False",
                created_at = user.Object.created_at
            };

            await firebase
                .Child("Users")
                .Child(user.Key)
                .PutAsync(updatedUser);
        }

        private async Task RevokeUserAccess(FirebaseObject<UserModel> user)
        {
            try
            {
                await firebase
                    .Child("Users")
                    .Child(user.Key)
                    .DeleteAsync();

                await AddAdminLog("User Revoked", user.Object.employee_id,
                                $"User access revoked for {user.Object.employee_id}",
                                $"User ID: {user.Object.user_id} was removed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error revoking user access: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
        }

        private async Task AddAdminLog(string actionType, string targetEmployeeId, string description, string details = "")
        {
            try
            {
                var adminLog = new
                {
                    action_type = actionType,
                    admin_employee_id = "JAP-001",
                    admin_name = "System Administrator",
                    admin_user_id = "101",
                    description = description,
                    details = details,
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

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedEmployeeId))
            {
                MessageBox.Show("Please select an employee.", "Selection Required",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxUserType.SelectedItem == null)
            {
                MessageBox.Show("Please select a user type.", "Selection Required",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedType = comboBoxUserType.SelectedItem.ToString();

            // Show confirmation dialog
            using (ConfirmAddNewUser confirmForm = new ConfirmAddNewUser())
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;

                var result = confirmForm.ShowDialog(this);

                if (result == DialogResult.OK && confirmForm.UserConfirmed)
                {
                    if (selectedType == "Revoke")
                    {
                        await HandleRevokeAction();
                    }
                    else
                    {
                        await HandleCreateUpdateAction(selectedType);
                    }
                }
                else
                {
                    MessageBox.Show("Action cancelled.", "Cancelled",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}