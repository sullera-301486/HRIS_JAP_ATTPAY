using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

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
            InitializeOtherComboBoxes();
            InitializeOptionalDateFields();
            UpdateAlternateTextboxAccessibility();
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

        private async Task CreateDefaultLeaveCredits(string employeeId, string fullName)
        {
            try
            {
                string department = "Unknown";
                string position = "Unknown";

                // 🔹 STEP 1: Fetch EmploymentInfo
                string employmentUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmploymentInfo.json";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(employmentUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrWhiteSpace(json) && json != "null")
                        {
                            var token = JToken.Parse(json);

                            // ✅ Handle object or array
                            if (token is JObject obj)
                            {
                                foreach (var prop in obj.Properties())
                                {
                                    var emp = prop.Value as JObject;
                                    if (emp == null) continue;

                                    if (emp["employee_id"]?.ToString() == employeeId)
                                    {
                                        department = emp["department"]?.ToString() ?? "Unknown";
                                        position = emp["position"]?.ToString() ?? "Unknown";
                                        break;
                                    }
                                }
                            }
                            else if (token is JArray arr)
                            {
                                foreach (var emp in arr.OfType<JObject>())
                                {
                                    if (emp["employee_id"]?.ToString() == employeeId)
                                    {
                                        department = emp["department"]?.ToString() ?? "Unknown";
                                        position = emp["position"]?.ToString() ?? "Unknown";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // 🔹 STEP 2: Check if Leave Credits already exist
                var existing = await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .OnceSingleAsync<JObject>();

                if (existing != null)
                {
                    // ✅ Only update missing department/position — keep existing leave balances
                    bool updated = false;

                    if (existing["department"] == null || existing["department"].ToString() == "Unknown")
                    {
                        existing["department"] = department;
                        updated = true;
                    }

                    if (existing["position"] == null || existing["position"].ToString() == "Unknown")
                    {
                        existing["position"] = position;
                        updated = true;
                    }

                    if (existing["full_name"] == null || string.IsNullOrEmpty(existing["full_name"].ToString()))
                    {
                        existing["full_name"] = fullName;
                        updated = true;
                    }

                    // ✅ Add base values if they don't exist
                    if (existing["sick_leave_base_value"] == null)
                    {
                        existing["sick_leave_base_value"] = 6;
                        updated = true;
                    }

                    if (existing["vacation_leave_base_value"] == null)
                    {
                        existing["vacation_leave_base_value"] = 6;
                        updated = true;
                    }

                    if (updated)
                    {
                        await firebase
                            .Child("Leave Credits")
                            .Child(employeeId)
                            .PutAsync(existing);
                        Console.WriteLine($"✅ Updated missing info for {employeeId}");
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ Leave Credits already complete for {employeeId}");
                    }

                    return; // stop here — do not overwrite
                }

                // 🔹 STEP 3: Create only if not existing with both current and base values
                var leaveCreditsObj = new
                {
                    employee_id = employeeId,
                    full_name = fullName,
                    department = department,
                    position = position,
                    sick_leave = 6,  // Current available sick leave
                    vacation_leave = 6,  // Current available vacation leave
                    sick_leave_base_value = 6,  // Base value for sick leave
                    vacation_leave_base_value = 6,  // Base value for vacation leave
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebase
                    .Child("Leave Credits")
                    .Child(employeeId)
                    .PutAsync(leaveCreditsObj);

                Console.WriteLine($"✅ Leave Credits created for {employeeId} with base values of 6 each");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating default leave credits: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task FixExistingLeaveCredits()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string leaveCreditsUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits.json";
                    string employmentUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmploymentInfo.json";

                    string leaveJson = await httpClient.GetStringAsync(leaveCreditsUrl);
                    string empJson = await httpClient.GetStringAsync(employmentUrl);

                    if (leaveJson == "null" || string.IsNullOrWhiteSpace(leaveJson)) return;
                    if (empJson == "null" || string.IsNullOrWhiteSpace(empJson)) return;

                    JObject leaveData = JObject.Parse(leaveJson);
                    JToken empToken = JToken.Parse(empJson);

                    IEnumerable<JProperty> empProperties;

                    if (empToken is JObject)
                        empProperties = ((JObject)empToken).Properties();
                    else if (empToken is JArray)
                        empProperties = ((JArray)empToken)
                            .OfType<JObject>()
                            .Select((obj, index) => new JProperty(index.ToString(), obj));
                    else
                    {
                        MessageBox.Show("EmploymentInfo JSON is not in a recognized format.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int fixedCount = 0;

                    foreach (var item in leaveData)
                    {
                        string empId = item.Key;
                        JObject leaveObj = (JObject)item.Value;

                        string department = "Unknown";
                        string position = "Unknown";

                        foreach (var empProp in empProperties)
                        {
                            var emp = empProp.Value as JObject;
                            if (emp == null) continue;

                            if (emp["employee_id"]?.ToString() == empId)
                            {
                                department = emp["department"]?.ToString() ?? "Unknown";
                                position = emp["position"]?.ToString() ?? "Unknown";
                                break;
                            }
                        }

                        // ✅ Add base values if they don't exist
                        if (leaveObj["sick_leave_base_value"] == null)
                        {
                            leaveObj["sick_leave_base_value"] = 6;
                        }

                        if (leaveObj["vacation_leave_base_value"] == null)
                        {
                            leaveObj["vacation_leave_base_value"] = 6;
                        }

                        leaveObj["department"] = department;
                        leaveObj["position"] = position;
                        leaveObj["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string updateUrl = $"https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits/{empId}.json";

                        var jsonBody = new StringContent(leaveObj.ToString(), System.Text.Encoding.UTF8, "application/json");
                        await httpClient.PutAsync(updateUrl, jsonBody);
                        fixedCount++;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fixing existing Leave Credits: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SyncAllEmployeesToLeaveCredits()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 1️⃣ Get EmployeeDetails
                    string empUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmployeeDetails.json";
                    var empResponse = await httpClient.GetAsync(empUrl);
                    if (!empResponse.IsSuccessStatusCode) return;

                    string empJson = await empResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(empJson) || empJson == "null") return;

                    JObject empData = JObject.Parse(empJson);

                    // 2️⃣ Get EmploymentInfo
                    string empInfoUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmploymentInfo.json";
                    var empInfoResponse = await httpClient.GetAsync(empInfoUrl);
                    string empInfoJson = await empInfoResponse.Content.ReadAsStringAsync();

                    // Handle both array and object formats
                    List<JObject> empInfoList = new List<JObject>();
                    if (!string.IsNullOrWhiteSpace(empInfoJson) && empInfoJson != "null")
                    {
                        var token = JToken.Parse(empInfoJson);
                        if (token is JArray arr)
                            empInfoList.AddRange(arr.OfType<JObject>());
                        else if (token is JObject obj)
                            empInfoList.AddRange(obj.Properties().Select(p => (JObject)p.Value));
                    }

                    // 3️⃣ Get existing Leave Credits
                    string leaveUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits.json";
                    var leaveResponse = await httpClient.GetAsync(leaveUrl);
                    string leaveJson = await leaveResponse.Content.ReadAsStringAsync();

                    JObject existingLeave = new JObject();
                    if (!string.IsNullOrWhiteSpace(leaveJson) && leaveJson != "null")
                        existingLeave = JObject.Parse(leaveJson);

                    int created = 0;
                    int updated = 0;
                    int skipped = 0;

                    // 4️⃣ Loop through EmployeeDetails
                    foreach (var emp in empData)
                    {
                        string empId = emp.Key;
                        JObject empObj = (JObject)emp.Value;

                        string firstName = empObj["first_name"]?.ToString() ?? "";
                        string middleName = empObj["middle_name"]?.ToString() ?? "";
                        string lastName = empObj["last_name"]?.ToString() ?? "";
                        string fullName = $"{firstName} {middleName} {lastName}".Trim();

                        // 🔹 Find position and department from EmploymentInfo
                        string position = "Unknown";
                        string department = "Unknown";

                        foreach (var info in empInfoList)
                        {
                            if (info?["employee_id"]?.ToString() == empId)
                            {
                                position = info["position"]?.ToString() ?? "Unknown";
                                department = info["department"]?.ToString() ?? "Unknown";
                                break;
                            }
                        }

                        // Check if record already exists
                        if (existingLeave.ContainsKey(empId))
                        {
                            JObject existingRecord = (JObject)existingLeave[empId];

                            // ✅ Update existing record to ensure base values exist
                            bool needsUpdate = false;

                            if (existingRecord["sick_leave_base_value"] == null)
                            {
                                existingRecord["sick_leave_base_value"] = 6;
                                needsUpdate = true;
                            }

                            if (existingRecord["vacation_leave_base_value"] == null)
                            {
                                existingRecord["vacation_leave_base_value"] = 6;
                                needsUpdate = true;
                            }

                            if (needsUpdate)
                            {
                                existingRecord["department"] = department;
                                existingRecord["position"] = position;
                                existingRecord["updated_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                string putUrl = $"https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits/{empId}.json";
                                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(existingRecord);
                                var response = await httpClient.PutAsync(putUrl, new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"));
                                if (response.IsSuccessStatusCode)
                                    updated++;
                            }
                            else
                            {
                                skipped++;
                            }
                        }
                        else
                        {
                            // 🔹 Create new leave record with both current and base values
                            var leaveObj = new
                            {
                                employee_id = empId,
                                full_name = fullName,
                                position = position,
                                department = department,
                                sick_leave = 6,  // Current available sick leave
                                vacation_leave = 6,  // Current available vacation leave
                                sick_leave_base_value = 6,  // Base value for sick leave
                                vacation_leave_base_value = 6,  // Base value for vacation leave
                                created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                                updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            };

                            string putUrl = $"https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits/{empId}.json";
                            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(leaveObj);
                            var response = await httpClient.PutAsync(putUrl, new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"));
                            if (response.IsSuccessStatusCode)
                                created++;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error syncing Leave Credits: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SyncMissingLeaveCredits()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 🔹 Fetch EmployeeDetails
                    string empUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmployeeDetails.json";
                    string empJson = await httpClient.GetStringAsync(empUrl);
                    if (string.IsNullOrWhiteSpace(empJson) || empJson == "null")
                    {
                        MessageBox.Show("❌ No EmployeeDetails found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    JObject employeeDetails = JObject.Parse(empJson);

                    // 🔹 Fetch EmploymentInfo
                    string empInfoUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/EmploymentInfo.json";
                    string empInfoJson = await httpClient.GetStringAsync(empInfoUrl);
                    List<JObject> employmentList = new List<JObject>();

                    if (!string.IsNullOrWhiteSpace(empInfoJson) && empInfoJson != "null")
                    {
                        var empToken = JToken.Parse(empInfoJson);

                        // Handle both array and object formats safely
                        if (empToken is JArray arr)
                        {
                            foreach (var item in arr)
                                if (item is JObject o) employmentList.Add(o);
                        }
                        else if (empToken is JObject obj)
                        {
                            foreach (var prop in obj.Properties())
                                if (prop.Value is JObject o) employmentList.Add(o);
                        }
                    }

                    // 🔹 Fetch existing Leave Credits
                    string leaveUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits.json";
                    string leaveJson = await httpClient.GetStringAsync(leaveUrl);
                    JObject existingLeave = new JObject();
                    if (!string.IsNullOrWhiteSpace(leaveJson) && leaveJson != "null")
                        existingLeave = JObject.Parse(leaveJson);

                    int added = 0;
                    int skipped = 0;

                    // 🔹 Loop through all employees
                    foreach (var emp in employeeDetails)
                    {
                        string empId = emp.Key;
                        JObject empObj = (JObject)emp.Value;

                        // ✅ Skip employees who already have a leave record
                        if (existingLeave.ContainsKey(empId))
                        {
                            skipped++;
                            continue;
                        }

                        string firstName = empObj["first_name"]?.ToString() ?? "";
                        string middleName = empObj["middle_name"]?.ToString() ?? "";
                        string lastName = empObj["last_name"]?.ToString() ?? "";
                        string fullName = $"{firstName} {middleName} {lastName}".Trim();

                        string position = "Unknown";
                        string department = "Unknown";

                        // Match in EmploymentInfo
                        foreach (var empInfo in employmentList)
                        {
                            if (empInfo?["employee_id"]?.ToString() == empId)
                            {
                                position = empInfo["position"]?.ToString() ?? "Unknown";
                                department = empInfo["department"]?.ToString() ?? "Unknown";
                                break;
                            }
                        }

                        // Build new Leave Credit record with both current and base values
                        var leaveRecord = new
                        {
                            employee_id = empId,
                            full_name = fullName,
                            department = department,
                            position = position,
                            sick_leave = 6,  // Current available sick leave
                            vacation_leave = 6,  // Current available vacation leave
                            sick_leave_base_value = 6,  // Base value for sick leave
                            vacation_leave_base_value = 6,  // Base value for vacation leave
                            created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                            updated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        // Upload only for missing employees
                        string putUrl = $"https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Leave%20Credits/{empId}.json";
                        string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(leaveRecord);
                        var response = await httpClient.PutAsync(putUrl, new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"));

                        if (response.IsSuccessStatusCode)
                            added++;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error syncing Leave Credits:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void AddNewEmployee_Load(object sender, EventArgs e)
        {
            await GenerateNextEmployeeId();
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
            await FixExistingLeaveCredits();
            await SyncMissingLeaveCredits();
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
                buttonScanRFID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 14f);
                comboBoxDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAddNewEmployee.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelAddress.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelAltWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelAltWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelCreateEmployeeProfile.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelContractType.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelDateOfExit.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelDateOfJoining.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelEmploymentInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelManager.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelMiddleName.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelNationality.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelRFIDTag.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelRFIDTagInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelShiftSchedule.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                textBoxAddress.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAltWorkHoursA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxAltWorkHoursB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxContact.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEmail.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                lblempID.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxFirstName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxLastName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMiddleName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNationality.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxWorkHoursA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxWorkHoursB.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                // Updated controls font
                cbGender.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbMaritalStatus.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbContractType.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbPosition.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbManager.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                dtpDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                dtpDateStarted.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                dtpDatePeriod.Font = AttributesClass.GetFont("Roboto-Regular", 14f);

                // Set custom format to match AdminAttendance (yyyy-MM-dd)
                dtpDateOfBirth.Format = DateTimePickerFormat.Custom;
                dtpDateOfBirth.CustomFormat = "yyyy-MM-dd";

                dtpDateStarted.Format = DateTimePickerFormat.Custom;
                dtpDateStarted.CustomFormat = "yyyy-MM-dd";

                dtpDatePeriod.Format = DateTimePickerFormat.Custom;
                dtpDatePeriod.CustomFormat = "yyyy-MM-dd";
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
                string employeeId = lblempID.Text.Trim();

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

                // Get values from updated controls
                string gender = cbGender.SelectedItem?.ToString() ?? "";
                string dateOfBirth = dtpDateOfBirth.Value.ToString("yyyy-MM-dd");
                string maritalStatus = cbMaritalStatus.SelectedItem?.ToString() ?? "";
                string nationality = textBoxNationality.Text.Trim();
                string contact = textBoxContact.Text.Trim();
                string email = textBoxEmail.Text.Trim();
                string address = textBoxAddress.Text.Trim();
                string position = cbPosition.SelectedItem?.ToString() ?? "";
                string department = comboBoxDepartment.Text.Trim();
                string contractType = cbContractType.SelectedItem?.ToString() ?? "";
                string dateOfJoining = dtpDateStarted.Value.ToString("yyyy-MM-dd");
                // Set date_of_exit to null instead of using the DateTimePicker value
                string dateOfExit = ""; // This will be stored as empty string in Firebase
                string managerName = cbManager.SelectedItem?.ToString() ?? "";

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
                    date_of_exit = dateOfExit, // This will be empty string (null equivalent in Firebase)
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt")
                };

                // ✅ STEP 1: Add basic employee data
                await firebase.Child("EmployeeDetails").Child(employeeId).PutAsync(employeeDetailsObj);
                await firebase.Child("EmploymentInfo").Child(employmentId).PutAsync(employmentInfoObj);

                // ✅ STEP 2: Add work schedules
                await AddWorkSchedulesAsync(employeeId);

                // ✅ STEP 3: Add leave credits with base values
                await CreateDefaultLeaveCredits(employeeId, fullName);

                // ✅ STEP 4: ADD PAYROLL DATA
                await CreateBasePayrollData(employeeId, fullName, department, position);
                await CreateBaseEmployeeDeductions(employeeId);
                await CreateBaseGovernmentDeductions(employeeId);

                // ✅ STEP 5: Add admin log
                await AddAdminLog("Employee Created", employeeId, fullName, $"{fullName} added new employee: {fullName}");

                MessageBox.Show("Employee successfully added to Firebase with complete payroll setup.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding employee to Firebase: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ CREATE BASE GOVERNMENT DEDUCTIONS METHOD
        private async Task CreateBaseGovernmentDeductions(string employeeId)
        {
            try
            {
                int nextArrayIndex = await GetNextGovernmentDeductionArrayIndex();
                int nextGovDeductionId = await GetNextGovernmentDeductionId();

                var governmentDeductionsObj = new
                {
                    gov_deduction_id = nextGovDeductionId.ToString(),
                    last_updated = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    pagibig = "0.0",
                    payroll_id = nextGovDeductionId.ToString(),
                    philhealth = "0.0",
                    sss = "0.0",
                    total_gov_deductions = "0.0",
                    withholding_tax = "0.0"
                };

                await AddToGovernmentDeductionsArray(nextArrayIndex, governmentDeductionsObj);
                Console.WriteLine($"✅ Base government deductions created for {employeeId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating base government deductions: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ HELPER METHOD TO GET NEXT GOVERNMENT DEDUCTION ARRAY INDEX
        private async Task<int> GetNextGovernmentDeductionArrayIndex()
        {
            try
            {
                var govDeductions = await firebase
                    .Child("GovernmentDeductions")
                    .OnceAsync<dynamic>();

                // Find the highest array index
                int maxIndex = 0;
                foreach (var deduction in govDeductions)
                {
                    if (int.TryParse(deduction.Key, out int index))
                    {
                        if (index > maxIndex) maxIndex = index;
                    }
                }
                return maxIndex + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next government deduction array index: {ex.Message}");
                return 3; // Based on your JSON, highest index is 2 (0=null, 1, 2, 3)
            }
        }

        // ✅ HELPER METHOD TO GET NEXT GOVERNMENT DEDUCTION ID
        private async Task<int> GetNextGovernmentDeductionId()
        {
            try
            {
                var govDeductions = await firebase
                    .Child("GovernmentDeductions")
                    .OnceAsync<dynamic>();

                int maxId = 0;
                foreach (var deduction in govDeductions)
                {
                    if (deduction.Object != null && deduction.Object.gov_deduction_id != null)
                    {
                        if (int.TryParse(deduction.Object.gov_deduction_id.ToString(), out int id))
                        {
                            if (id > maxId) maxId = id;
                        }
                    }
                }
                return maxId + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next government deduction ID: {ex.Message}");
                return 4; // Based on your JSON, highest is 3
            }
        }

        // ✅ HELPER METHOD TO ADD TO GOVERNMENT DEDUCTIONS ARRAY
        private async Task AddToGovernmentDeductionsArray(int index, object govDeductionsObj)
        {
            await firebase
                .Child("GovernmentDeductions")
                .Child(index.ToString())
                .PutAsync(govDeductionsObj);
        }

        // ✅ ADD ADMIN LOG METHOD
        private async Task AddAdminLog(string actionType, string targetEmployeeId, string targetEmployeeName, string description)
        {
            try
            {
                // Get the current admin user info (you might want to pass this from the parent form)
                string adminEmployeeId = "JAP-001"; // Default admin
                string adminName = "System Administrator";
                string adminUserId = "101";

                var adminLog = new
                {
                    action_type = actionType,
                    admin_employee_id = adminEmployeeId,
                    admin_name = adminName,
                    admin_user_id = adminUserId,
                    description = description,
                    details = $"Employee ID: {targetEmployeeId}, Added by: {adminName} (User ID: {adminUserId})",
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

        private void InitializeOtherComboBoxes()
        {
            // Initialize Gender ComboBox
            cbGender.Items.AddRange(new string[] {
                "Male",
                "Female",
                "Other"
            });

            // Initialize Marital Status ComboBox
            cbMaritalStatus.Items.AddRange(new string[] {
                "Single",
                "Married",
                "Widowed"
            });

            // Initialize Contract Type ComboBox
            cbContractType.Items.AddRange(new string[] {
                "Regular",
                "Contractual"

            });

            // Initialize Position ComboBox
            cbPosition.Items.AddRange(new string[] {
                "Analyst",
                "HR Manager",
                "Accountanting Manager",
                "Operations Manager",
                "Assistant",
                "Staff"
            });

            // Initialize Manager ComboBox
            cbManager.Items.AddRange(new string[] {
                "Franz Louies Deloritos",
                "Charles Macaraig"
            });

            // Set default dates
            dtpDateOfBirth.Value = DateTime.Now.AddYears(-25);
            dtpDateStarted.Value = DateTime.Now;
            dtpDatePeriod.Value = DateTime.Now.AddYears(1);
        }

        private async Task<string> GetNextEmployeeId()
        {
            try
            {
                var allEmployeeIds = new List<int>();

                // Method to extract numeric ID from employee ID string
                int ExtractNumericId(string empId)
                {
                    if (empId.StartsWith("JAP-"))
                    {
                        if (int.TryParse(empId.Split('-')[1], out int idNum))
                        {
                            return idNum;
                        }
                    }
                    return 0;
                }

                // Get all employee details (active employees)
                var activeEmployees = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<dynamic>();

                foreach (var employee in activeEmployees)
                {
                    if (employee.Object != null)
                    {
                        string empId = employee.Object.employee_id;
                        int numericId = ExtractNumericId(empId);
                        if (numericId > 0)
                        {
                            allEmployeeIds.Add(numericId);
                        }
                    }
                }

                // Get all archived employees
                var archivedEmployees = await firebase
                    .Child("ArchivedEmployees")
                    .OnceAsync<dynamic>();

                foreach (var archived in archivedEmployees)
                {
                    if (archived.Object != null && archived.Object.employee_data != null)
                    {
                        string empId = archived.Object.employee_data.employee_id;
                        int numericId = ExtractNumericId(empId);
                        if (numericId > 0)
                        {
                            allEmployeeIds.Add(numericId);
                        }
                    }
                }

                // Find the highest ID number
                int maxId = allEmployeeIds.Count > 0 ? allEmployeeIds.Max() : 0;

                // Generate next ID - following your pattern JAP-001, JAP-002, etc.
                int nextIdNum = maxId + 1;
                return $"JAP-{nextIdNum:D3}"; // Format as 3-digit number with leading zeros
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetNextEmployeeId: {ex.Message}");
                // Fallback: if error occurs, return next logical ID
                return "JAP-010"; // Based on your data, JAP-009 is the highest
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

        private async Task GenerateNextEmployeeId()
        {
            try
            {
                // Get all employee IDs from both active and archived employees
                var activeEmployees = await firebase
                    .Child("EmployeeDetails")
                    .OnceAsync<dynamic>();

                var archivedEmployees = await firebase
                    .Child("ArchivedEmployees")
                    .OnceAsync<dynamic>();

                List<int> employeeNumbers = new List<int>();

                // Extract numbers from active employees
                foreach (var emp in activeEmployees)
                {
                    if (emp.Object != null && emp.Key.StartsWith("JAP-"))
                    {
                        string idPart = emp.Key.Substring(4); // Remove "JAP-" prefix
                        if (int.TryParse(idPart, out int number))
                        {
                            employeeNumbers.Add(number);
                        }
                    }
                }

                // Extract numbers from archived employees
                foreach (var emp in archivedEmployees)
                {
                    if (emp.Object != null && emp.Key.StartsWith("JAP-"))
                    {
                        string idPart = emp.Key.Substring(4); // Remove "JAP-" prefix
                        if (int.TryParse(idPart, out int number))
                        {
                            employeeNumbers.Add(number);
                        }
                    }
                }

                // Find the highest number and increment
                int nextNumber = employeeNumbers.Count > 0 ? employeeNumbers.Max() + 1 : 1;

                // Format as three-digit number
                lblempID.Text = $"JAP-{nextNumber:D3}";
            }
            catch (Exception ex)
            {
                // Fallback to simple increment if there's an error
                MessageBox.Show("Error generating employee ID: " + ex.Message + "\nUsing default ID.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lblempID.Text = "JAP-001";
            }
        }

        // ✅ CREATE BASE PAYROLL DATA METHOD - FIXED FOR EXACT ARRAY FORMAT
        private async Task CreateBasePayrollData(string employeeId, string fullName, string department, string position)
        {
            try
            {
                // Get next array index and IDs
                int nextPayrollArrayIndex = await GetNextPayrollArrayIndex();
                int nextPayrollId = await GetNextPayrollId();

                // Create base payroll record
                var payrollObj = new
                {
                    created_at = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    cutoff_end = DateTime.Now.ToString("yyyy-MM-dd"),
                    cutoff_start = DateTime.Now.AddDays(-15).ToString("yyyy-MM-dd"),
                    employee_id = employeeId,
                    last_updated = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    net_pay = 0,
                    payroll_id = nextPayrollId.ToString()
                };

                // Create base payroll earnings
                var payrollEarningsObj = new
                {
                    basic_pay = "0.0",
                    commission = "0.0",
                    communication = "0.0",
                    daily_rate = "0",
                    days_present = "0",
                    earning_id = nextPayrollId.ToString(),
                    food_allowance = "0.0",
                    gas_allowance = "0.0",
                    gondola = "0.0",
                    incentives = "0.0",
                    last_updated = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    overtime_pay = "0.0",
                    payroll_id = nextPayrollId.ToString(),
                    total_earnings = "0.0"
                };

                // Create base payroll summary
                var payrollSummaryObj = new
                {
                    gross_pay = "0.00",
                    last_updated = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss tt"),
                    net_pay = "0.00",
                    payroll_id = nextPayrollId.ToString(),
                    summary_id = nextPayrollId.ToString(),
                    total_deductions = "0.00"
                };

                // Add to arrays using exact indices
                await AddToPayrollArray(nextPayrollArrayIndex, payrollObj);
                await AddToPayrollEarningsArray(nextPayrollArrayIndex, payrollEarningsObj);
                await AddToPayrollSummaryArray(nextPayrollArrayIndex, payrollSummaryObj);

                Console.WriteLine($"✅ Base payroll data created for {employeeId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating base payroll data: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ UPDATED HELPER METHODS FOR EXACT ARRAY FORMAT

        private async Task<int> GetNextPayrollArrayIndex()
        {
            try
            {
                var payrolls = await firebase
                    .Child("Payroll")
                    .OnceAsync<dynamic>();

                // Find the highest array index (not payroll_id)
                int maxIndex = 0;
                foreach (var payroll in payrolls)
                {
                    if (int.TryParse(payroll.Key, out int index))
                    {
                        if (index > maxIndex) maxIndex = index;
                    }
                }
                return maxIndex + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next payroll array index: {ex.Message}");
                return 4; // Based on your JSON, highest index is 3
            }
        }

        private async Task<int> GetNextPayrollId()
        {
            try
            {
                var payrolls = await firebase
                    .Child("Payroll")
                    .OnceAsync<dynamic>();

                // Find the highest payroll_id
                int maxId = 0;
                foreach (var payroll in payrolls)
                {
                    if (payroll.Object != null && payroll.Object.payroll_id != null)
                    {
                        if (int.TryParse(payroll.Object.payroll_id.ToString(), out int id))
                        {
                            if (id > maxId) maxId = id;
                        }
                    }
                }
                return maxId + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next payroll ID: {ex.Message}");
                return 4; // Based on your JSON, highest is 3
            }
        }

        // ✅ UPDATED ARRAY MANAGEMENT METHODS - USING EXPLICIT INDICES

        private async Task AddToPayrollArray(int index, object payrollObj)
        {
            await firebase
                .Child("Payroll")
                .Child(index.ToString())
                .PutAsync(payrollObj);
        }

        private async Task AddToPayrollEarningsArray(int index, object earningsObj)
        {
            await firebase
                .Child("PayrollEarnings")
                .Child(index.ToString())
                .PutAsync(earningsObj);
        }

        private async Task AddToPayrollSummaryArray(int index, object summaryObj)
        {
            await firebase
                .Child("PayrollSummary")
                .Child(index.ToString())
                .PutAsync(summaryObj);
        }

        // ✅ SIMILAR UPDATES FOR DEDUCTIONS ARRAYS

        private async Task<int> GetNextEmployeeDeductionArrayIndex()
        {
            try
            {
                var deductions = await firebase
                    .Child("EmployeeDeductions")
                    .OnceAsync<dynamic>();

                int maxIndex = 0;
                foreach (var deduction in deductions)
                {
                    if (int.TryParse(deduction.Key, out int index))
                    {
                        if (index > maxIndex) maxIndex = index;
                    }
                }
                return maxIndex + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next deduction array index: {ex.Message}");
                return 3; // Based on your JSON, highest index is 2
            }
        }

        private async Task<int> GetNextEmployeeDeductionId()
        {
            try
            {
                var deductions = await firebase
                    .Child("EmployeeDeductions")
                    .OnceAsync<dynamic>();

                int maxId = 0;
                foreach (var deduction in deductions)
                {
                    if (deduction.Object != null && deduction.Object.deduction_id != null)
                    {
                        if (int.TryParse(deduction.Object.deduction_id.ToString(), out int id))
                        {
                            if (id > maxId) maxId = id;
                        }
                    }
                }
                return maxId + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next deduction ID: {ex.Message}");
                return 4; // Based on your JSON, highest is 3
            }
        }

        private async Task AddToEmployeeDeductionsArray(int index, object deductionsObj)
        {
            await firebase
                .Child("EmployeeDeductions")
                .Child(index.ToString())
                .PutAsync(deductionsObj);
        }

        // Update the CreateBaseEmployeeDeductions method to use array indices
        private async Task CreateBaseEmployeeDeductions(string employeeId)
        {
            try
            {
                int nextArrayIndex = await GetNextEmployeeDeductionArrayIndex();
                int nextDeductionId = await GetNextEmployeeDeductionId();

                var employeeDeductionsObj = new
                {
                    cash_advance = 0,
                    coop_contribution = 0,
                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    deduction_id = nextDeductionId.ToString(),
                    employee_id = employeeId,
                    last_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    other_deductions = 0,
                    payroll_id = nextDeductionId.ToString()
                };

                await AddToEmployeeDeductionsArray(nextArrayIndex, employeeDeductionsObj);
                Console.WriteLine($"✅ Base employee deductions created for {employeeId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating base employee deductions: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // Add this method to handle optional date of exit
        private void InitializeOptionalDateFields()
        {
            // You could add a checkbox like "Set End Date" that enables/disables the date picker
            // Or simply set the dtpDatePeriod to a default null state
            dtpDatePeriod.Format = DateTimePickerFormat.Custom;
            dtpDatePeriod.CustomFormat = " "; // Empty space to show as blank
            dtpDatePeriod.ValueChanged += (s, e) =>
            {
                if (dtpDatePeriod.Value != dtpDatePeriod.MinDate)
                {
                    dtpDatePeriod.CustomFormat = "yyyy-MM-dd";
                }
            };
        }

        private void btnSalary_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewEmployeeSalary NewEmployeeSalaryForm = new NewEmployeeSalary();
            AttributesClass.ShowWithOverlay(parentForm, NewEmployeeSalaryForm);
        }
    }
}