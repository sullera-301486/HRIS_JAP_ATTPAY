using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace HRIS_JAP_ATTPAY
{
    public partial class LoginRectangle : UserControl
    {
        // Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public LoginRectangle()
        {
            InitializeComponent();
            SetFont();
        }

        private void LoginRectangle_Load(object sender, EventArgs e) { }

        private void SetFont()
        {
            try
            {
                labelWelcome.Font = AttributesClass.GetFont("Roboto-Light", 24f);
                labelLogin.Font = AttributesClass.GetFont("Roboto-Regular", 36f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                textBoxID.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                labelPassword.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                textBoxPassword.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
                buttonLogin.Font = AttributesClass.GetFont("Roboto-Regular", 30f);
                labelFailed.Font = AttributesClass.GetFont("Roboto-Light", 10f, FontStyle.Italic);
                labelTermsAndConditions.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void MoveToAdmin(string userID)
        {
            Form parentForm = AttributesClass.GetRealOwnerForm(this.FindForm());
            if (parentForm != null)
            {
                parentForm.Tag = "OpenNewForm1"; // open AdminForm
                parentForm.Close();
            }
        }

        private void MoveToHR(string userID)
        {
            Form parentForm = AttributesClass.GetRealOwnerForm(this.FindForm());
            if (parentForm != null)
            {
                parentForm.Tag = "OpenNewForm2"; // open HRForm
                parentForm.Close();
            }
        }

        // SHA-256 hash
        private string ComputeHash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("X2")); // uppercase hex
                return builder.ToString();
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            doLogin();
        }

        private async void doLogin()
        {
            string enteredID = textBoxID.Text.Trim();
            string enteredPassword = textBoxPassword.Text.Trim();

            try
            {
                // Pull user from Firebase
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserFirebase>();

                var user = users.FirstOrDefault(u => u.Object.user_id == enteredID)?.Object;

                if (user == null)
                {
                    labelFailed.Visible = true;
                    return;
                }

                if (AttributesScanner.IsScannerConnected())
                {
                    Console.WriteLine("Scanner detected. Starting monitor...");
                }
                else
                {
                    Console.WriteLine("No scanner detected. Monitor not started.");
                }

                string computedHash = ComputeHash(enteredPassword + user.salt);
                if (computedHash.Equals(user.password_hash, StringComparison.OrdinalIgnoreCase))
                {
                    labelFailed.Visible = false;

                    //  NEW CODE: Fetch employee details and store session info
                    await SetCurrentUserSessionAsync(user);

                    // Move to correct form based on role
                    if (user.isAdmin)
                        MoveToAdmin(user.user_id);
                    else
                        MoveToHR(user.user_id);
                }
                else
                {
                    labelFailed.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }

        //  NEW METHOD: Fills SessionClass with the logged-in user's data
        private async Task SetCurrentUserSessionAsync(UserFirebase user)
        {
            try
            {
                var empDetails = await firebase
                    .Child("EmployeeDetails")
                    .Child(user.employee_id)
                    .OnceSingleAsync<EmployeeFirebase>();

                string fullName = $"{empDetails.first_name} {empDetails.middle_name} {empDetails.last_name}".Trim();

                SessionClass.CurrentUserId = user.user_id;
                SessionClass.CurrentEmployeeId = user.employee_id;
                SessionClass.CurrentEmployeeName = fullName;
                SessionClass.IsAdmin = user.isAdmin;

                Console.WriteLine($"Logged in: {SessionClass.CurrentEmployeeName} ({SessionClass.CurrentEmployeeId})");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set session: " + ex.Message);
            }
        }

        // Firebase User model
        private class UserFirebase
        {
            public string user_id { get; set; }
            public string employee_id { get; set; }
            public string password_hash { get; set; }
            public string salt { get; set; }
            public bool isAdmin { get; set; }
            public string created_at { get; set; }
        }

        // NEW MODEL: Employee details fetched from Firebase
        private class EmployeeFirebase
        {
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public string last_name { get; set; }
            public string email { get; set; }
            public string contact { get; set; }
            public string employee_id { get; set; }
        }

        private void textBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                doLogin();
                e.SuppressKeyPress = true;
            }
        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                doLogin();
                e.SuppressKeyPress = true;
            }
        }

        private void labelTermsAndConditions_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            TermsAndConditions termsAndConditions = new TermsAndConditions();
            AttributesClass.ShowFullCover(parentForm, termsAndConditions);
        }
    }
}
