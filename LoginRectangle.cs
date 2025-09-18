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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void MoveToAdmin()
        {
            Form parentForm = AttributesClass.GetRealOwnerForm(this.FindForm());
            if (parentForm != null)
            {
                parentForm.Tag = "OpenNewForm1"; // open AdminForm
                parentForm.Close();
            }
        }

        private void MoveToHR()
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

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            string enteredID = textBoxID.Text.Trim();
            string enteredPassword = textBoxPassword.Text.Trim();

            try
            {
                var users = await firebase
                    .Child("Users")
                    .OnceAsync<UserFirebase>();

                var user = users.FirstOrDefault(u => u.Object.user_id == enteredID)?.Object;

                if (user == null)
                {
                    labelFailed.Visible = true;
                    return;
                }

                string computedHash = ComputeHash(enteredPassword + user.salt);

                if (computedHash.Equals(user.password_hash, StringComparison.OrdinalIgnoreCase))
                {
                    labelFailed.Visible = false;

                    if (user.isAdmin)
                        MoveToAdmin();
                    else
                        MoveToHR();
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
    }
}