using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;

namespace HRIS_JAP_ATTPAY
{
    public partial class UserProfile : Form
    {
        // Firebase client
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private HttpClient httpClient = new HttpClient();
        private string identifier; // can be user_id or employee_id

        public UserProfile()
        {
            InitializeComponent();
            SetFont();
            identifier = SessionClass.CurrentUserId; // no identifier passed          
            LoadUserData();
        }

        public UserProfile(string adminTag)
        {
            InitializeComponent();
            SetFont();
            identifier = adminTag; // store for lookup
            LoadUserData();
        }

        private void SetFont()
        {
            try
            {
                labelProfile.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 16f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 11f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelContactInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelEmailInput.Font = AttributesClass.GetFont("Roboto-Light", 11f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void LoadUserData()
        {
            try
            {
                // Step 1: Load all Users as dictionary
                var usersJson = await firebase.Child("Users").OnceAsJsonAsync();
                var usersDict = ParseJsonAsDictionary(usersJson);

                Dictionary<string, string> userEntry = null;

                if (!string.IsNullOrEmpty(identifier))
                {
                    // Lookup by user_id or employee_id
                    foreach (var user in usersDict.Values)
                    {
                        if ((user.ContainsKey("user_id") && user["user_id"] == identifier) ||
                            (user.ContainsKey("employee_id") && user["employee_id"] == identifier))
                        {
                            userEntry = user;
                            break;
                        }
                    }
                }

                // fallback: first user
                if (userEntry == null && usersDict.Count > 0)
                {
                    userEntry = usersDict.Values.First();
                }

                if (userEntry == null)
                {
                    labelNameInput.Text = "User not found";
                    return;
                }

                string employeeId = userEntry.ContainsKey("employee_id") ? userEntry["employee_id"] : null;
                bool isAdmin = userEntry.ContainsKey("isAdmin") && userEntry["isAdmin"] == "True";

                // Step 2: Load EmployeeDetails as dictionary
                var empDetailsJson = await firebase.Child("EmployeeDetails").OnceAsJsonAsync();
                var empDetailsDict = ParseJsonAsDictionary(empDetailsJson);

                Dictionary<string, string> empDetails = null;
                if (!string.IsNullOrEmpty(employeeId) && empDetailsDict.ContainsKey(employeeId))
                {
                    empDetails = empDetailsDict[employeeId];
                }

                string firstName = empDetails != null && empDetails.ContainsKey("first_name") ? empDetails["first_name"] : "";
                string lastName = empDetails != null && empDetails.ContainsKey("last_name") ? empDetails["last_name"] : "";
                string fullName = $"{firstName} {lastName}".Trim();
                string contact = empDetails != null && empDetails.ContainsKey("contact") ? empDetails["contact"] : null;
                string email = empDetails != null && empDetails.ContainsKey("email") ? empDetails["email"] : null;
                string imageUrl = empDetails != null && empDetails.ContainsKey("image_url") ? empDetails["image_url"] : null;

                // Step 3: Load EmploymentInfo as list
                var employmentJson = await firebase.Child("EmploymentInfo").OnceAsJsonAsync();
                var employmentList = ParseMalformedJson(employmentJson);

                var employment = employmentList
                    .FirstOrDefault(e => e.ContainsKey("employee_id") && e["employee_id"] == employeeId);

                string department = employment != null && employment.ContainsKey("department") ? employment["department"] : "N/A";
                string position = employment != null && employment.ContainsKey("position") ? employment["position"] : "N/A";

                // Step 4: Update UI
                labelNameInput.Text = !string.IsNullOrEmpty(fullName) ? fullName : "Unknown";
                labelIDInput.Text = (employeeId ?? "N/A") + (isAdmin ? " (Admin)" : "");
                labelDepartmentInput.Text = department;
                labelPositionInput.Text = position;
                labelContactInput.Text = contact ?? "N/A";
                labelEmailInput.Text = email ?? "N/A";

                // Step 5: Load and display user image
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await LoadUserImage(imageUrl);
                }
                else
                {
                    // Set default image if no image URL
                    SetDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user data: " + ex.Message);
                SetDefaultImage();
            }
        }

        private async Task LoadUserImage(string imageUrl)
        {
            try
            {
                // Download image from URL
                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(ms);

                    // Set PictureBox to Zoom mode (maintains aspect ratio within square)
                    pictureBoxEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBoxEmployee.Image = image;

                    // Remove any circular region to keep it square
                    pictureBoxEmployee.Region = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            // Set a default user icon with square format
            try
            {
                // If you have a default image in resources:
                // pictureBoxEmployee.Image = Properties.Resources.DefaultUser;

                // Or create a simple square placeholder
                Bitmap defaultImage = new Bitmap(pictureBoxEmployee.Width, pictureBoxEmployee.Height);
                using (Graphics g = Graphics.FromImage(defaultImage))
                {
                    g.Clear(Color.LightGray);
                    using (Font font = new Font("Arial", 12))
                    using (Brush brush = new SolidBrush(Color.DarkGray))
                    {
                        StringFormat sf = new StringFormat();
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        g.DrawString("No Image", font, brush,
                            new RectangleF(0, 0, defaultImage.Width, defaultImage.Height), sf);
                    }
                }
                pictureBoxEmployee.Image = defaultImage;
                pictureBoxEmployee.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxEmployee.Region = null; // Ensure square shape
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting default image: {ex.Message}");
            }
        }

        private List<Dictionary<string, string>> ParseMalformedJson(string rawJson)
        {
            var records = new List<Dictionary<string, string>>();

            if (string.IsNullOrEmpty(rawJson) || rawJson.Trim() == "null")
                return records;

            try
            {
                string cleanedJson = rawJson.Replace("\n", "").Replace("\r", "").Replace("\t", "")
                    .Replace("'", "\"").Replace("(", "[").Replace(")", "]")
                    .Replace("[null,", "[").Replace("], [", ",").Replace("}, {", "},{")
                    .Replace("},(", "},{").Replace("),{", "},{")
                    .Replace("}{", "},{").Replace("}}", "}").Replace("{{", "{");

                if (string.IsNullOrEmpty(cleanedJson) || cleanedJson == "null")
                    return records;

                var matches = Regex.Matches(cleanedJson, @"\{[^{}]*\}");
                foreach (Match match in matches)
                {
                    try
                    {
                        var record = new Dictionary<string, string>();
                        string objectStr = match.Value;

                        var kvpMatches = Regex.Matches(objectStr, @"""([^""]+)""\s*:\s*(""[^""]*""|\d+\.?\d*|true|false|null|\[[^\]]*\]|\{[^{}]*\})");

                        foreach (Match kvpMatch in kvpMatches)
                        {
                            if (kvpMatch.Groups.Count >= 3)
                            {
                                string key = kvpMatch.Groups[1].Value;
                                string value = kvpMatch.Groups[2].Value;

                                if (value.StartsWith("\"") && value.EndsWith("\""))
                                {
                                    value = value.Trim('"');
                                }
                                record[key] = value;
                            }
                        }

                        if (record.Count > 0)
                        {
                            records.Add(record);
                        }
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("JSON parsing error: " + ex.Message);
            }

            return records;
        }

        // Parse JSON as dictionary keyed by top-level key (used for Users & EmployeeDetails)
        private Dictionary<string, Dictionary<string, string>> ParseJsonAsDictionary(string rawJson)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();

            if (string.IsNullOrEmpty(rawJson) || rawJson.Trim() == "null")
                return dict;

            try
            {
                JObject obj = JObject.Parse(rawJson);
                foreach (var prop in obj.Properties())
                {
                    if (prop.Value.Type == JTokenType.Object)
                    {
                        var innerDict = new Dictionary<string, string>();
                        foreach (var kv in (JObject)prop.Value)
                        {
                            innerDict[kv.Key] = kv.Value.ToString();
                        }
                        dict[prop.Name] = innerDict;
                    }
                }
            }
            catch { }

            return dict;
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}