using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class EditEmployeeProfileHR : Form
    {
        private string selectedEmployeeId;
        private string localImagePath; // Store the selected image path

        // ---- Firebase Configuration for v7.3 ----
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private static readonly string FirebaseStorageBucket = "thesis151515.firebasestorage.app";
        // ------------------------------------------

        public EditEmployeeProfileHR(string employeeId)
        {
            InitializeComponent();
            selectedEmployeeId = employeeId; // Store the employee ID
            setFont();
        }

        private void buttonChangePhoto_Click_1(object sender, EventArgs e)
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

        private async void EditEmployeeProfileHR_Load(object sender, EventArgs e)
        {
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

            // Initialize combo boxes
            InitializeOtherComboBoxes();
            InitializeOptionalDateFields();

            // 🔹 Load employee data if an ID was passed
            if (!string.IsNullOrEmpty(selectedEmployeeId))
            {
                await LoadEmployeeData(selectedEmployeeId);
            }

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

                    // Use DateTimePicker for Date of Birth
                    if (DateTime.TryParse(empDetails.date_of_birth, out DateTime dob))
                    {
                        dtpDateOfBirth.Value = dob;
                    }

                    // Use ComboBox for Gender
                    if (!string.IsNullOrEmpty(empDetails.gender))
                    {
                        cbGender.SelectedItem = empDetails.gender;
                    }

                    // Use ComboBox for Marital Status
                    if (!string.IsNullOrEmpty(empDetails.marital_status))
                    {
                        cbMaritalStatus.SelectedItem = empDetails.marital_status;
                    }

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

            // Use ComboBox for Position
            if (cbPosition.InvokeRequired)
            {
                cbPosition.Invoke(new Action(() =>
                    cbPosition.SelectedItem = empObj["position"]?.ToString() ?? ""));
            }
            else
            {
                cbPosition.SelectedItem = empObj["position"]?.ToString() ?? "";
            }

            // Use ComboBox for Contract Type
            if (cbContractType.InvokeRequired)
            {
                cbContractType.Invoke(new Action(() =>
                    cbContractType.SelectedItem = empObj["contract_type"]?.ToString() ?? ""));
            }
            else
            {
                cbContractType.SelectedItem = empObj["contract_type"]?.ToString() ?? "";
            }

            // Use DateTimePicker for Date of Joining
            if (dtpDateStarted.InvokeRequired)
            {
                dtpDateStarted.Invoke(new Action(() =>
                {
                    if (DateTime.TryParse(empObj["date_of_joining"]?.ToString(), out DateTime dateJoined))
                    {
                        dtpDateStarted.Value = dateJoined;
                    }
                }));
            }
            else
            {
                if (DateTime.TryParse(empObj["date_of_joining"]?.ToString(), out DateTime dateJoined))
                {
                    dtpDateStarted.Value = dateJoined;
                }
            }

            // Use DateTimePicker for Date of Exit
            if (dtpDatePeriod.InvokeRequired)
            {
                dtpDatePeriod.Invoke(new Action(() =>
                {
                    if (DateTime.TryParse(empObj["date_of_exit"]?.ToString(), out DateTime dateExit))
                    {
                        dtpDatePeriod.Value = dateExit;
                    }
                }));
            }
            else
            {
                if (DateTime.TryParse(empObj["date_of_exit"]?.ToString(), out DateTime dateExit))
                {
                    dtpDatePeriod.Value = dateExit;
                }
            }

            // Use ComboBox for Manager
            if (cbManager.InvokeRequired)
            {
                cbManager.Invoke(new Action(() =>
                    cbManager.SelectedItem = empObj["manager_name"]?.ToString() ?? ""));
            }
            else
            {
                cbManager.SelectedItem = empObj["manager_name"]?.ToString() ?? "";
            }
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

            if (cbPosition.InvokeRequired)
            {
                cbPosition.Invoke(new Action(() => cbPosition.SelectedIndex = -1));
            }
            else
            {
                cbPosition.SelectedIndex = -1;
            }

            if (cbContractType.InvokeRequired)
            {
                cbContractType.Invoke(new Action(() => cbContractType.SelectedIndex = -1));
            }
            else
            {
                cbContractType.SelectedIndex = -1;
            }

            if (dtpDateStarted.InvokeRequired)
            {
                dtpDateStarted.Invoke(new Action(() => dtpDateStarted.Value = DateTime.Now));
            }
            else
            {
                dtpDateStarted.Value = DateTime.Now;
            }

            if (dtpDatePeriod.InvokeRequired)
            {
                dtpDatePeriod.Invoke(new Action(() => dtpDatePeriod.Value = DateTime.Now.AddYears(1)));
            }
            else
            {
                dtpDatePeriod.Value = DateTime.Now.AddYears(1);
            }

            if (cbManager.InvokeRequired)
            {
                cbManager.Invoke(new Action(() => cbManager.SelectedIndex = -1));
            }
            else
            {
                cbManager.SelectedIndex = -1;
            }
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
                textBoxEmail.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxFirstName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxLastName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxMiddleName.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxNationality.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxWorkHoursA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxWorkHoursB.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                // Add font settings for new controls
                cbGender.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbMaritalStatus.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbContractType.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbPosition.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                cbManager.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                dtpDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                dtpDateStarted.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                dtpDatePeriod.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
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
                confirmForm.ShowDialog(this); // Just show the dialog, don't check result

                // Only check the UserConfirmed property
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
                // Show progress indicator
                Cursor.Current = Cursors.WaitCursor;

                // Validate work schedule first
                if (!ValidateWorkSchedule())
                {
                    return;
                }

                // 1. Handle image upload first (if new image was selected)
                string imageUrl = null;
                if (!string.IsNullOrEmpty(localImagePath))
                {
                    imageUrl = await UploadImageToFirebase(localImagePath, selectedEmployeeId);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        bool ok = await UpdateEmployeeImage(selectedEmployeeId, imageUrl);
                        if (ok)
                        {
                            MessageBox.Show("Profile image uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update employee record with image URL.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                // 2. Update EmployeeDetails
                await UpdateEmployeeDetails(imageUrl);

                // 3. Update EmploymentInfo
                await UpdateEmploymentInfo();

                // 4. Update Work Schedule
                await UpdateWorkSchedule(selectedEmployeeId);

                MessageBox.Show("Employee profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close(); // Close the form after successful update
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private async Task UpdateWorkSchedule(string employeeId)
        {
            try
            {
                // First, get all existing schedules
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
                    date_of_birth = dtpDateOfBirth.Value.ToString("yyyy-MM-dd"),
                    gender = cbGender.SelectedItem?.ToString() ?? existingEmployee?.gender ?? "",
                    marital_status = cbMaritalStatus.SelectedItem?.ToString() ?? existingEmployee?.marital_status ?? "",
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

                // Create updated employment info using the new controls
                var employmentInfo = new EmploymentInfoModel
                {
                    employee_id = selectedEmployeeId,
                    position = cbPosition.SelectedItem?.ToString() ?? existingPosition,
                    department = comboBoxDepartment.Text ?? existingDepartment,
                    contract_type = cbContractType.SelectedItem?.ToString() ?? existingContractType,
                    date_of_joining = dtpDateStarted.Value.ToString("yyyy-MM-dd"),
                    date_of_exit = dtpDatePeriod.Value.ToString("yyyy-MM-dd"),
                    manager_name = cbManager.SelectedItem?.ToString() ?? existingManagerName,
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
            var updateData = new { image_url = imageUrl };
            string jsonBody = JsonConvert.SerializeObject(updateData);

            using (var http = new HttpClient())
            {
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                string[] pathsToTry =
                {
                    "EmployeeDetails/" + employeeId + ".json",
                    "employees/" + employeeId + ".json"
                };

                foreach (var path in pathsToTry)
                {
                    string fullUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/" + path;

                    try
                    {
                        using (var response = await http.PatchAsync(fullUrl, content))
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                MessageBox.Show($"Failed at {path}: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseContent}", "Error");
                            }
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
        private async Task LoadWorkScheduleData(string employeeId)
        {
            try
            {
                // Use HttpClient to get raw JSON to properly handle array structure
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        // If empty or null, return
                        if (string.IsNullOrEmpty(json) || json == "null")
                            return;

                        var schedulesArray = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(json);

                        if (schedulesArray == null || schedulesArray.Count == 0)
                            return;

                        var employeeSchedules = new List<WorkScheduleModel>();

                        // Process each schedule in the array
                        foreach (var item in schedulesArray)
                        {
                            if (item != null && item.Type != Newtonsoft.Json.Linq.JTokenType.Null)
                            {
                                try
                                {
                                    var schedule = item.ToObject<WorkScheduleModel>();
                                    if (schedule != null && schedule.employee_id == employeeId)
                                    {
                                        employeeSchedules.Add(schedule);
                                    }
                                }
                                catch (Exception)
                                {
                                    // Skip invalid entries
                                    continue;
                                }
                            }
                        }

                        // Reset all checkboxes first
                        System.Windows.Forms.CheckBox[] dayBoxes = {
                            checkBoxM, checkBoxT, checkBoxW, checkBoxTh, checkBoxF, checkBoxS,
                            checkBoxAltM, checkBoxAltT, checkBoxAltW, checkBoxAltTh, checkBoxAltF, checkBoxAltS
                        };

                        foreach (var cb in dayBoxes)
                        {
                            cb.Checked = false;
                        }

                        // Clear work hours textboxes
                        textBoxWorkHoursA.Text = "";
                        textBoxWorkHoursB.Text = "";
                        textBoxAltWorkHoursA.Text = "";
                        textBoxAltWorkHoursB.Text = "";

                        // Populate schedules
                        foreach (var scheduleObj in employeeSchedules)
                        {
                            // Set work hours based on schedule type
                            if (scheduleObj.schedule_type == "Regular")
                            {
                                textBoxWorkHoursA.Text = scheduleObj.start_time ?? "";
                                textBoxWorkHoursB.Text = scheduleObj.end_time ?? "";
                            }
                            else if (scheduleObj.schedule_type == "Alternate")
                            {
                                textBoxAltWorkHoursA.Text = scheduleObj.start_time ?? "";
                                textBoxAltWorkHoursB.Text = scheduleObj.end_time ?? "";
                            }

                            // Check the appropriate day checkboxes
                            switch (scheduleObj.day_of_week?.ToLower())
                            {
                                case "monday":
                                    if (scheduleObj.schedule_type == "Regular")
                                        checkBoxM.Checked = true;
                                    else
                                        checkBoxAltM.Checked = true;
                                    break;
                                case "tuesday":
                                    if (scheduleObj.schedule_type == "Regular")
                                        checkBoxT.Checked = true;
                                    else
                                        checkBoxAltT.Checked = true;
                                    break;
                                case "wednesday":
                                    if (scheduleObj.schedule_type == "Regular")
                                        checkBoxW.Checked = true;
                                    else
                                        checkBoxAltW.Checked = true;
                                    break;
                                case "thursday":
                                    if (scheduleObj.schedule_type == "Regular")
                                        checkBoxTh.Checked = true;
                                    else
                                        checkBoxAltTh.Checked = true;
                                    break;
                                case "friday":
                                    if (scheduleObj.schedule_type == "Regular")
                                        checkBoxF.Checked = true;
                                    else
                                        checkBoxAltF.Checked = true;
                                    break;
                                case "saturday":
                                    if (scheduleObj.schedule_type == "Regular")
                                        checkBoxS.Checked = true;
                                    else
                                        checkBoxAltS.Checked = true;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading work schedule: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                        // Handle array format (your current Firebase structure)
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
                            // Object/collection format (fallback)
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
                // Since Firebase stores as array, we need to rebuild the entire array
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/Work_Schedule.json");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(json) || json == "null")
                            return;

                        // Array structure - rebuild without this employee's schedules
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
                // Use HTTP client for array structure
                await SaveScheduleToArray(schedule, arrayIndex);
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
        private void InitializeOtherComboBoxes()
        {
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

        private void InitializeOptionalDateFields()
        {
            // Set custom format to match AdminAttendance (yyyy-MM-dd)
            dtpDateOfBirth.Format = DateTimePickerFormat.Custom;
            dtpDateOfBirth.CustomFormat = "yyyy-MM-dd";

            dtpDateStarted.Format = DateTimePickerFormat.Custom;
            dtpDateStarted.CustomFormat = "yyyy-MM-dd";

            dtpDatePeriod.Format = DateTimePickerFormat.Custom;
            dtpDatePeriod.CustomFormat = "yyyy-MM-dd";
        }
    }


    // 🔹 Firebase models

    public static class HttpClientExtensionss
    {
        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return client.SendAsync(request);
        }
    }

}