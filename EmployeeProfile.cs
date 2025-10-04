using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Net.Http;
using System.IO;

namespace HRIS_JAP_ATTPAY
{
    public partial class EmployeeProfile : Form
    {
        private readonly string employeeId;
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        public bool UserConfirmed { get; private set; } = false;
        public EmployeeProfile(string employeeId)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            setFont();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadEmployeeDetails();
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonArchive.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                buttonEdit.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelAddress.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAddressInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkHoursInputA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHoursInputB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContactInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContractType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContractTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDashA.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDashB.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfBirthInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfExit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfExitInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfJoining.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfJoiningInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmailInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmployeeProfile.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelEmploymentInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFirstNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGenderInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelManager.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManagerInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMiddleName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMiddleNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelNationality.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNationalityInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPersonalAndEmploymentRecord.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelRFIDTag.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelRFIDTagInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelShiftSchedule.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkHoursInputA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelWorkHoursInputB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async Task LoadEmployeeDetails()
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    MessageBox.Show("Employee ID is missing.");
                    return;
                }

                // Test Firebase connectivity
                try
                {
                    var test = await firebase.Child("EmployeeDetails").OnceAsync<object>();
                }
                catch
                {
                    MessageBox.Show("Cannot connect to Firebase. Please check your internet connection.");
                    return;
                }

                await LoadEmployeeDetailsDynamic();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load employee details: " + ex.Message);
            }
        }

        private async Task LoadEmployeeDetailsDynamic()
        {
            try
            {
                var employeeResponse = await firebase
                .Child("EmployeeDetails")
                .Child(employeeId)
                .OnceSingleAsync<dynamic>();

                if (employeeResponse != null)
                {
                    dynamic employee = employeeResponse;

                    SafeSetLabelText(labelEmployeeIDInput, employee?.employee_id?.ToString());
                    SafeSetLabelText(labelFirstNameInput, employee?.first_name?.ToString());
                    SafeSetLabelText(labelMiddleNameInput, employee?.middle_name?.ToString());
                    SafeSetLabelText(labelLastNameInput, employee?.last_name?.ToString());
                    SafeSetLabelText(labelGenderInput, employee?.gender?.ToString());
                    SafeSetLabelText(labelEmailInput, employee?.email?.ToString());
                    SafeSetLabelText(labelAddressInput, employee?.address?.ToString());
                    SafeSetLabelText(labelContactInput, employee?.contact?.ToString());
                    SafeSetLabelText(labelMaritalStatusInput, employee?.marital_status?.ToString());
                    SafeSetLabelText(labelNationalityInput, employee?.nationality?.ToString());
                    SafeSetLabelText(labelRFIDTagInput, employee?.rfid_tag?.ToString());

                    if (!string.IsNullOrEmpty(employee?.date_of_birth?.ToString()))
                    {
                        if (DateTime.TryParse(employee.date_of_birth.ToString(), out DateTime dob))
                            SafeSetLabelText(labelDateOfBirthInput, dob.ToString("yyyy-MM-dd"));
                        else
                            SafeSetLabelText(labelDateOfBirthInput, employee.date_of_birth.ToString());
                    }
                    else
                    {
                        SafeSetLabelText(labelDateOfBirthInput, "N/A");
                    }

                    string imageUrl = employee?.image_url?.ToString();
                    if (!string.IsNullOrEmpty(imageUrl))
                        await LoadProfileImage(imageUrl);
                    else
                        SafeSetPictureBoxImage(pictureBoxEmployee, null);
                }
                else
                {
                    MessageBox.Show("Employee not found.");
                    return;
                }

                await LoadEmploymentInfoDirect();
                await LoadWorkScheduleManual();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee data: " + ex.Message);
            }
        }

        private void SafeSetLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
                label.Invoke(new Action(() => label.Text = text ?? "N/A"));
            else
                label.Text = text ?? "N/A";
        }

        private void SafeSetPictureBoxImage(PictureBox pictureBox, Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() =>
                {
                    pictureBox.Image = image;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                }));
            }
            else
            {
                pictureBox.Image = image;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private async Task LoadProfileImage(string imageUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        var image = Image.FromStream(ms);
                        SafeSetPictureBoxImage(pictureBoxEmployee, image);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profile image: {ex.Message}");
                SafeSetPictureBoxImage(pictureBoxEmployee, null);
            }
        }

        private async Task LoadEmploymentInfoDirect()
        {
            try
            {
                var employmentData = await firebase
                .Child("EmploymentInfo")
                .OnceAsync<object>();

                if (employmentData != null)
                {
                    foreach (var item in employmentData)
                    {
                        if (item?.Object != null)
                        {
                            var empObj = JObject.FromObject(item.Object);
                            var empId = empObj["employee_id"]?.ToString();

                            if (empId == employeeId)
                            {
                                SafeSetLabelText(labelDepartmentInput, empObj["department"]?.ToString());
                                SafeSetLabelText(labelPositionInput, empObj["position"]?.ToString());
                                SafeSetLabelText(labelContractTypeInput, empObj["contract_type"]?.ToString());
                                SafeSetLabelText(labelManagerInput, empObj["manager_name"]?.ToString());

                                // Date of joining
                                if (!string.IsNullOrEmpty(empObj["date_of_joining"]?.ToString()))
                                {
                                    if (DateTime.TryParse(empObj["date_of_joining"].ToString(), out DateTime doj))
                                        SafeSetLabelText(labelDateOfJoiningInput, doj.ToString("yyyy-MM-dd"));
                                    else
                                        SafeSetLabelText(labelDateOfJoiningInput, empObj["date_of_joining"].ToString());
                                }
                                else
                                {
                                    SafeSetLabelText(labelDateOfJoiningInput, "N/A");
                                }

                                // Date of exit
                                if (!string.IsNullOrEmpty(empObj["date_of_exit"]?.ToString()))
                                {
                                    if (DateTime.TryParse(empObj["date_of_exit"].ToString(), out DateTime doe))
                                        SafeSetLabelText(labelDateOfExitInput, doe.ToString("yyyy-MM-dd"));
                                    else
                                        SafeSetLabelText(labelDateOfExitInput, empObj["date_of_exit"].ToString());
                                }
                                else
                                {
                                    SafeSetLabelText(labelDateOfExitInput, "N/A");
                                }
                                return;
                            }
                        }
                    }
                }

                // Set defaults if no data found
                SafeSetLabelText(labelDepartmentInput, "N/A");
                SafeSetLabelText(labelPositionInput, "N/A");
                SafeSetLabelText(labelContractTypeInput, "N/A");
                SafeSetLabelText(labelManagerInput, "N/A");
                SafeSetLabelText(labelDateOfJoiningInput, "N/A");
                SafeSetLabelText(labelDateOfExitInput, "N/A");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employment info: {ex.Message}");
            }
        }

        private async Task LoadWorkScheduleManual()
        {
            try
            {
                // Containers for parsed schedule
                var regularDays = new List<string>();
                var alternateDays = new List<string>();
                string regularStart = "", regularEnd = "", alternateStart = "", alternateEnd = "";

                var dayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
{"Monday", "M"}, {"Tuesday", "T"}, {"Wednesday", "W"},
{"Thursday", "Th"}, {"Friday", "F"}, {"Saturday", "S"}, {"Sunday", "Su"}
};

                bool anyFound = false;

                // Helper to get property value case-insensitively (and some common name variants)
                string GetProp(JObject o, params string[] names)
                {
                    foreach (var name in names)
                    {
                        var p = o.Properties().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                        if (p != null) return p.Value?.ToString();
                    }

                    // Try normalized match (remove underscores, lowercase)
                    foreach (var name in names)
                    {
                        var target = name.Replace("_", "").ToLower();
                        var p = o.Properties().FirstOrDefault(x => x.Name.Replace("_", "").ToLower() == target);
                        if (p != null) return p.Value?.ToString();
                    }

                    return null;
                }

                // Local function to process a schedule JObject
                void ProcessSchedObj(JObject schedObj)
                {
                    if (schedObj == null) return;

                    var empId = GetProp(schedObj, "employee_id", "employeeId", "emp_id")?.Trim();
                    if (string.IsNullOrEmpty(empId)) return;
                    if (!string.Equals(empId, employeeId, StringComparison.OrdinalIgnoreCase)) return;

                    anyFound = true;

                    var scheduleType = GetProp(schedObj, "schedule_type", "type", "scheduleType");
                    var dayOfWeek = GetProp(schedObj, "day_of_week", "day", "dayOfWeek");
                    var startTime = GetProp(schedObj, "start_time", "start", "startTime", "time_start");
                    var endTime = GetProp(schedObj, "end_time", "end", "endTime", "time_end");

                    // Normalize day to short form
                    string shortDay = null;
                    if (!string.IsNullOrEmpty(dayOfWeek))
                    {
                        if (!dayMap.TryGetValue(dayOfWeek.Trim(), out shortDay))
                        {
                            // fallback to first 3 letters (capitalized)
                            var clean = dayOfWeek.Trim();
                            shortDay = clean.Length >= 3 ? clean.Substring(0, 3) : clean;
                        }
                    }

                    if (scheduleType?.Equals("Regular", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (!string.IsNullOrEmpty(shortDay) && !regularDays.Contains(shortDay))
                            regularDays.Add(shortDay);

                        if (string.IsNullOrEmpty(regularStart) && !string.IsNullOrEmpty(startTime))
                        {
                            regularStart = startTime;
                            regularEnd = endTime;
                        }
                    }
                    else // treat everything else as alternate if not explicitly Regular
                    {
                        if (!string.IsNullOrEmpty(shortDay) && !alternateDays.Contains(shortDay))
                            alternateDays.Add(shortDay);

                        if (string.IsNullOrEmpty(alternateStart) && !string.IsNullOrEmpty(startTime))
                        {
                            alternateStart = startTime;
                            alternateEnd = endTime;
                        }
                    }
                }

                // Attempt 1: Read as keyed collection (most common Firebase shape)
                try
                {
                    var scheduleList = await firebase
                    .Child("Work_Schedule")
                    .OnceAsync<object>();

                    if (scheduleList != null && scheduleList.Any())
                    {
                        foreach (var item in scheduleList)
                        {
                            if (item?.Object == null) continue;

                            // item.Object can be a JObject-like or primitive - convert safely
                            try
                            {
                                var schedObj = JObject.FromObject(item.Object);
                                // If schedObj looks like nested keyed children (no employee_id at root), iterate its properties
                                if (!schedObj.Properties().Any(p => string.Equals(p.Name, "employee_id", StringComparison.OrdinalIgnoreCase)))
                                {
                                    // e.g. keyed object: { "-Mx...": { ... }, "-Mx...2": { ... } }
                                    foreach (var prop in schedObj.Properties())
                                    {
                                        if (prop.Value is JObject childObj)
                                            ProcessSchedObj(childObj);
                                        else if (prop.Value is JArray childArr)
                                        {
                                            foreach (var child in childArr.OfType<JObject>())
                                                ProcessSchedObj(child);
                                        }
                                    }
                                }
                                else
                                {
                                    ProcessSchedObj(schedObj);
                                }
                            }
                            catch
                            {
                                // ignore malformed item and continue
                                continue;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore and fallback to single fetch below
                }

                // Attempt 2: If nothing found yet, try OnceSingleAsync and handle array/object shapes
                if (!anyFound)
                {
                    try
                    {
                        var scheduleSingle = await firebase
                        .Child("Work_Schedule")
                        .OnceSingleAsync<object>();

                        if (scheduleSingle != null)
                        {
                            var token = JToken.FromObject(scheduleSingle);

                            if (token.Type == JTokenType.Array)
                            {
                                foreach (var child in token)
                                {
                                    if (child.Type == JTokenType.Object)
                                        ProcessSchedObj((JObject)child);
                                }
                            }
                            else if (token.Type == JTokenType.Object)
                            {
                                var rootObj = (JObject)token;

                                // If root has employee_id, it's a single object schedule
                                if (rootObj.Properties().Any(p => string.Equals(p.Name, "employee_id", StringComparison.OrdinalIgnoreCase)))
                                {
                                    ProcessSchedObj(rootObj);
                                }
                                else
                                {
                                    // Otherwise, treat as keyed children
                                    foreach (var prop in rootObj.Properties())
                                    {
                                        if (prop.Value.Type == JTokenType.Object)
                                            ProcessSchedObj((JObject)prop.Value);
                                        else if (prop.Value.Type == JTokenType.Array)
                                        {
                                            foreach (var child in prop.Value.OfType<JObject>())
                                                ProcessSchedObj(child);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore fallback errors
                    }
                }

                // Finalize UI update - set N/A if nothing found or set collected values
                SafeSetLabelText(labelWorkDaysInput, regularDays.Any() ? string.Join(", ", regularDays) : "N/A");
                SafeSetLabelText(labelWorkHoursInputA, !string.IsNullOrEmpty(regularStart) ? regularStart : "N/A");
                SafeSetLabelText(labelWorkHoursInputB, !string.IsNullOrEmpty(regularEnd) ? regularEnd : "N/A");

                SafeSetLabelText(labelAltWorkDaysInput, alternateDays.Any() ? string.Join(", ", alternateDays) : "N/A");
                SafeSetLabelText(labelAltWorkHoursInputA, !string.IsNullOrEmpty(alternateStart) ? alternateStart : "N/A");
                SafeSetLabelText(labelAltWorkHoursInputB, !string.IsNullOrEmpty(alternateEnd) ? alternateEnd : "N/A");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading work schedule: {ex.Message}");
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditEmployeeProfile editEmployeeProfileForm = new EditEmployeeProfile(employeeId);
            AttributesClass.ShowWithOverlay(parentForm, editEmployeeProfileForm);
        }

        private async void buttonArchive_Click_1(object sender, EventArgs e)
        {
            using (ConfirmArchive confirmForm = new ConfirmArchive(employeeId))
            {
                confirmForm.StartPosition = FormStartPosition.CenterParent;
                confirmForm.ShowDialog(this);

                if (confirmForm.UserConfirmed)
                    await ExecuteArchiveAndRefresh();
                else
                    MessageBox.Show("Archive cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task ExecuteArchiveAndRefresh()
        {
            RefreshAdminEmployee();
            this.Close();
            MessageBox.Show("Employee archived successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshAdminEmployee()
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                var adminEmployeeControl = FindControl<AdminEmployee>(parentForm);
                if (adminEmployeeControl != null)
                    adminEmployeeControl.RefreshData();
            }
        }

        private T FindControl<T>(Control control) where T : Control
        {
            foreach (Control child in control.Controls)
            {
                if (child is T found)
                    return found;

                var deeper = FindControl<T>(child);
                if (deeper != null)
                    return deeper;
            }
            return null;
        }
    }
}