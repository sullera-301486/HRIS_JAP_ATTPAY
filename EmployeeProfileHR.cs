using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class EmployeeProfileHR : Form
    {
        // 🔹 Store employeeId for reference
        private readonly string employeeId;

        // 🔹 Firebase client
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public EmployeeProfileHR(string employeeId)
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
                labelAltWorkHoursInputB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
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

                Console.WriteLine($"Loading details for employee: {employeeId}");

                // 🔹 Test Firebase connectivity first
                try
                {
                    var test = await firebase.Child("EmployeeDetails").OnceAsync<object>();
                    Console.WriteLine($"Firebase connection successful. Found {test.Count} employee records.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Firebase connection failed: {ex.Message}");
                    MessageBox.Show("Cannot connect to Firebase. Please check your internet connection.");
                    return;
                }

                // 🔹 APPROACH 1: Use dynamic objects for more flexibility
                await LoadEmployeeDetailsDynamic();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load employee details: " + ex.Message);
                Console.WriteLine($"Error details: {ex}");
            }
        }

        private async Task LoadEmployeeDetailsDynamic()
        {
            try
            {
                // 🔹 Fetch personal info by employeeId using dynamic
                var employeeResponse = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (employeeResponse != null)
                {
                    dynamic employee = employeeResponse;
                    labelEmployeeIDInput.Text = employee?.employee_id ?? "N/A";
                    labelFirstNameInput.Text = employee["first_name"]?.ToString() ?? "N/A";
                    labelMiddleNameInput.Text = employee["middle_name"]?.ToString() ?? "N/A";
                    labelLastNameInput.Text = employee["last_name"]?.ToString() ?? "N/A";
                    labelGenderInput.Text = employee["gender"]?.ToString() ?? "N/A";
                    if (!string.IsNullOrEmpty(employee?.date_of_birth?.ToString()))
                    {
                        if (DateTime.TryParse(employee.date_of_birth.ToString(), out DateTime dob))
                            labelDateOfBirthInput.Text = dob.ToString("yyyy-MM-dd"); // Only date
                        else
                            labelDateOfBirthInput.Text = employee.date_of_birth.ToString();
                    }
                    else
                    {
                        labelDateOfBirthInput.Text = "N/A";
                    }
                    labelEmailInput.Text = employee["email"]?.ToString() ?? "N/A";
                    labelAddressInput.Text = employee["address"]?.ToString() ?? "N/A";
                    labelContactInput.Text = employee["contact"]?.ToString() ?? "N/A";

                    // 🔹 Add these
                    labelMaritalStatusInput.Text = employee?.marital_status ?? "N/A";
                    labelNationalityInput.Text = employee?.nationality ?? "N/A";
                    labelRFIDTagInput.Text = employee?.rfid_tag ?? "N/A";
                }
                else
                {
                    MessageBox.Show("Employee not found.");
                    return;
                }

                // 🔹 APPROACH 2: Direct JSON parsing for EmploymentInfo array
                await LoadEmploymentInfoDirect();

                // 🔹 APPROACH 3: Manual iteration for Work_Schedule array
                await LoadWorkScheduleManual();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in dynamic loading: " + ex.Message);
            }
        }

        private async Task LoadEmploymentInfoDirect()
        {
            try
            {
                // 🔹 Get all EmploymentInfo as raw object
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<object>();

                if (employmentData != null)
                {
                    // Convert to JArray
                    var employmentArray = JArray.FromObject(employmentData);

                    foreach (var item in employmentArray)
                    {
                        if (item != null && item.Type != JTokenType.Null)
                        {
                            var empId = item["employee_id"]?.ToString();
                            if (empId == employeeId)
                            {
                                labelDepartmentInput.Text = item["department"]?.ToString() ?? "N/A";
                                labelPositionInput.Text = item["position"]?.ToString() ?? "N/A";
                                labelContractTypeInput.Text = item["contract_type"]?.ToString() ?? "N/A";
                                labelManagerInput.Text = item["manager_name"]?.ToString() ?? "N/A";
                                if (!string.IsNullOrEmpty(item["date_of_joining"]?.ToString()))
                                {
                                    if (DateTime.TryParse(item["date_of_joining"].ToString(), out DateTime doj))
                                        labelDateOfJoiningInput.Text = doj.ToString("yyyy-MM-dd"); // Only date
                                    else
                                        labelDateOfJoiningInput.Text = item["date_of_joining"].ToString();
                                }
                                else
                                {
                                    labelDateOfJoiningInput.Text = "N/A";
                                }
                                // Date of Exit
                                if (!string.IsNullOrEmpty(item["date_of_exit"]?.ToString()))
                                {
                                    if (DateTime.TryParse(item["date_of_exit"].ToString(), out DateTime doe))
                                        labelDateOfExitInput.Text = doe.ToString("yyyy-MM-dd"); // Only date
                                    else
                                        labelDateOfExitInput.Text = item["date_of_exit"].ToString();
                                }
                                else
                                {
                                    labelDateOfExitInput.Text = "N/A";
                                }
                                Console.WriteLine("Found employment info for " + employeeId);
                                return;
                            }
                        }
                    }
                }

                // If no match found
                Console.WriteLine("No employment info found for " + employeeId);
                labelDepartmentInput.Text = "N/A";
                labelPositionInput.Text = "N/A";
                labelContractTypeInput.Text = "N/A";
                labelManagerInput.Text = "N/A";
                labelDateOfJoiningInput.Text = "N/A";
                labelDateOfExitInput.Text = "N/A";
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
                // 🔹 Get all Work_Schedule as JSON string
                var scheduleData = await firebase
    .Child("Work_Schedule")
    .OnceSingleAsync<object>();

                if (scheduleData != null)
                {
                    var scheduleArray = JArray.FromObject(scheduleData);
                    var regularDays = new List<string>();
                    var alternateDays = new List<string>();
                    string regularStart = "", regularEnd = "", alternateStart = "", alternateEnd = "";

                    var dayMap = new Dictionary<string, string>
    {
        {"Monday", "M"}, {"Tuesday", "T"}, {"Wednesday", "W"},
        {"Thursday", "Th"}, {"Friday", "F"}, {"Saturday", "S"}, {"Sunday", "Su"}
    };

                    foreach (var item in scheduleArray)
                    {
                        if (item != null && item.Type != JTokenType.Null)
                        {
                            var empId = item["employee_id"]?.ToString();
                            if (empId == employeeId)
                            {
                                var scheduleType = item["schedule_type"]?.ToString();
                                var dayOfWeek = item["day_of_week"]?.ToString();
                                var startTime = item["start_time"]?.ToString();
                                var endTime = item["end_time"]?.ToString();

                                var shortDay = dayMap.ContainsKey(dayOfWeek) ? dayMap[dayOfWeek] : dayOfWeek;

                                if (scheduleType?.Equals("Regular", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    if (!regularDays.Contains(shortDay))
                                        regularDays.Add(shortDay);

                                    if (string.IsNullOrEmpty(regularStart))
                                    {
                                        regularStart = startTime;
                                        regularEnd = endTime;
                                    }
                                }
                                else if (scheduleType?.Equals("Alternate", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    if (!alternateDays.Contains(shortDay))
                                        alternateDays.Add(shortDay);

                                    if (string.IsNullOrEmpty(alternateStart))
                                    {
                                        alternateStart = startTime;
                                        alternateEnd = endTime;
                                    }
                                }
                            }
                        }
                    }

                    labelWorkDaysInput.Text = regularDays.Any() ? string.Join(" - ", regularDays) : "N/A";
                    labelWorkHoursInputA.Text = !string.IsNullOrEmpty(regularStart) ? regularStart : "N/A";
                    labelWorkHoursInputB.Text = !string.IsNullOrEmpty(regularEnd) ? regularEnd : "N/A";

                    labelAltWorkDaysInput.Text = alternateDays.Any() ? string.Join(" - ", alternateDays) : "N/A";
                    labelAltWorkHoursInputA.Text = !string.IsNullOrEmpty(alternateStart) ? alternateStart : "N/A";
                    labelAltWorkHoursInputB.Text = !string.IsNullOrEmpty(alternateEnd) ? alternateEnd : "N/A";
                }
                else
                {
                    Console.WriteLine("No schedule data found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading work schedule: {ex.Message}");
            }
        }


        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditEmployeeProfileHR editEmployeeProfileHRForm = new EditEmployeeProfileHR(employeeId);
            AttributesClass.ShowWithOverlay(parentForm, editEmployeeProfileHRForm);
        }

        private void buttonArchive_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmArchive confirmArchiveForm = new ConfirmArchive();
            AttributesClass.ShowWithOverlay(parentForm, confirmArchiveForm);
        }
    }

    // 🔹 Keep your model classes as backup
    public class EmployeeFields
    {
        public string employee_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public string date_of_birth { get; set; }
        public string address { get; set; }
        public string gender { get; set; }
        public string marital_status { get; set; }
        public string nationality { get; set; }
        public string rfid_tag { get; set; }
    }

    public class EmploymentFields
    {
        public string employee_id { get; set; }
        public string contract_type { get; set; }
        public string date_of_joining { get; set; }
        public string date_of_exit { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public string manager_name { get; set; }
    }

    public class WorkScheduleFields
    {
        public string employee_id { get; set; }
        public string day_of_week { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string schedule_type { get; set; }
        public string schedule_id { get; set; }
    }
}