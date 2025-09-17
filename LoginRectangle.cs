using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System.Drawing;

namespace HRIS_JAP_ATTPAY
{
    public partial class LoginRectangle : UserControl
    {
        // 🔹 Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public LoginRectangle()
        {
            InitializeComponent();
            SetFont();
        }

        private void LoginRectangle_Load(object sender, EventArgs e)
        {
        }

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

        // 🔑 SHA-256 hash
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
            string connStr = @"Server=localhost\SQLEXPRESS;Database=JAP_HRIS;Trusted_Connection=True;";
            string enteredID = textBoxID.Text.Trim();
            string enteredPassword = textBoxPassword.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = "SELECT u.password_hash, u.salt, u.isAdmin, u.employee_id " +
                                   "FROM Users u WHERE u.user_id = @id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", enteredID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbHash = reader["password_hash"] as string;
                                string dbSalt = reader["salt"] as string;
                                bool isAdmin = Convert.ToBoolean(reader["isAdmin"]);
                                string employeeId = reader["employee_id"].ToString();

                                if (string.IsNullOrEmpty(dbHash))
                                {
                                    labelFailed.Visible = true;
                                    return;
                                }

                                string computedHash = ComputeHash(enteredPassword + dbSalt);

                                if (computedHash.Equals(dbHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    labelFailed.Visible = false;

                                    // 🔄 Push all local data to Firebase
                                    await PushAllLocalToFirebase();

                                    if (isAdmin)
                                        MoveToAdmin();
                                    else
                                        MoveToHR();
                                }
                                else
                                {
                                    labelFailed.Visible = true;
                                }
                            }
                            else
                            {
                                labelFailed.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }

        // 🔹 Push all local tables to Firebase
        private async Task PushAllLocalToFirebase()
        {
            string connStr = @"Server=localhost\SQLEXPRESS;Database=JAP_HRIS;Trusted_Connection=True;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // =========================
                    // 1. EmployeeDetails
                    // =========================
                    string q1 = "SELECT * FROM EmployeeDetails";
                    using (SqlCommand cmd = new SqlCommand(q1, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var empDetails = new
                            {
                                employee_id = reader["employee_id"].ToString(),
                                first_name = reader["first_name"].ToString(),
                                middle_name = reader["middle_name"]?.ToString(),
                                last_name = reader["last_name"].ToString(),
                                date_of_birth = reader["date_of_birth"]?.ToString(),
                                gender = reader["gender"]?.ToString(),
                                marital_status = reader["marital_status"]?.ToString(),
                                nationality = reader["nationality"]?.ToString(),
                                contact = reader["contact"]?.ToString(),
                                email = reader["email"]?.ToString(),
                                address = reader["address"]?.ToString(),
                                rfid_tag = reader["rfid_tag"]?.ToString(),
                                created_at = reader["created_at"].ToString()
                            };

                            await firebase.Child("EmployeeDetails")
                                          .Child(empDetails.employee_id)
                                          .PutAsync(empDetails);
                        }
                    }

                    // =========================
                    // 2. EmploymentInfo
                    // =========================
                    string q2 = "SELECT * FROM EmploymentInfo";
                    using (SqlCommand cmd = new SqlCommand(q2, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var empInfo = new
                            {
                                employment_id = reader["employment_id"].ToString(),
                                employee_id = reader["employee_id"].ToString(),
                                position = reader["position"]?.ToString(),
                                department = reader["department"]?.ToString(),
                                contract_type = reader["contract_type"]?.ToString(),
                                date_of_joining = reader["date_of_joining"]?.ToString(),
                                date_of_exit = reader["date_of_exit"]?.ToString(),
                                manager_name = reader["manager_name"]?.ToString(),
                                created_at = reader["created_at"].ToString()
                            };

                            await firebase.Child("EmploymentInfo")
                                          .Child(empInfo.employment_id)
                                          .PutAsync(empInfo);
                        }
                    }

                    // =========================
                    // 3. Users
                    // =========================
                    string q3 = "SELECT * FROM Users";
                    using (SqlCommand cmd = new SqlCommand(q3, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userData = new
                            {
                                user_id = reader["user_id"].ToString(),
                                employee_id = reader["employee_id"].ToString(),
                                password_hash = reader["password_hash"]?.ToString(),
                                salt = reader["salt"]?.ToString(),
                                isAdmin = reader["isAdmin"].ToString(),
                                created_at = reader["created_at"].ToString()
                            };

                            await firebase.Child("Users")
                                          .Child(userData.user_id)
                                          .PutAsync(userData);
                        }
                    }

                    // =========================
                    // 4. Work_Schedule
                    // =========================
                    string q4 = "SELECT * FROM Work_Schedule";
                    using (SqlCommand cmd = new SqlCommand(q4, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sched = new
                            {
                                schedule_id = reader["schedule_id"].ToString(),
                                employee_id = reader["employee_id"].ToString(),
                                schedule_type = reader["schedule_type"].ToString(),
                                day_of_week = reader["day_of_week"].ToString(),
                                start_time = reader["start_time"].ToString(),
                                end_time = reader["end_time"].ToString()
                            };

                            await firebase.Child("Work_Schedule")
                                          .Child(sched.schedule_id)
                                          .PutAsync(sched);
                        }
                    }

                    // =========================
                    // 5. Attendance (UPDATED with overtime)
                    // =========================
                    string q5 = "SELECT * FROM Attendance";
                    using (SqlCommand cmd = new SqlCommand(q5, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var attendance = new
                            {
                                attendance_id = reader["attendance_id"].ToString(),
                                employee_id = reader["employee_id"].ToString(),
                                schedule_id = reader["schedule_id"].ToString(),
                                employment_id = reader["employment_id"].ToString(),
                                attendance_date = Convert.ToDateTime(reader["attendance_date"]).ToString("yyyy-MM-dd"),
                                time_in = reader["time_in"]?.ToString(),
                                time_out = reader["time_out"]?.ToString(),
                                hours_worked = reader["hours_worked"]?.ToString() ?? "0",
                                overtime_in = reader["overtime_in"]?.ToString(),
                                overtime_out = reader["overtime_out"]?.ToString(),
                                overtime_hours = reader["overtime_hours"]?.ToString() ?? "0",
                                status = reader["status"]?.ToString(),
                                verification_method = reader["verification_method"]?.ToString(),
                                created_at = reader["created_at"].ToString()
                            };

                            await firebase.Child("Attendance")
                                          .Child(attendance.attendance_id)
                                          .PutAsync(attendance);
                        }
                    }
                }

                MessageBox.Show("✅ All tables including Attendance (with overtime fields) synced to Firebase successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Push to Firebase error: " + ex.Message);
            }
        }
    }
}
