using System;
using System.Collections.Generic;   // For List and Dictionary
using System.Linq;                  // For Any()
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;          // For JArray, JToken

namespace HRIS_JAP_ATTPAY
{
    public partial class EmployeeProfile : Form
    {
        private readonly string employeeId;

        // 🔹 Firebase client
        private readonly FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

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
            await LoadEmploymentInfo();     // ✅ corrected
            await LoadWorkSchedule();       // ✅ corrected
            await LoadUserPassword();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmArchive confirmArchiveForm = new ConfirmArchive();
            AttributesClass.ShowWithOverlay(parentForm, confirmArchiveForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            EditEmployeeProfile editEmployeeProfileForm = new EditEmployeeProfile(employeeId);
            AttributesClass.ShowWithOverlay(parentForm, editEmployeeProfileForm);
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

                // Personal Info
                labelEmployeeProfile.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelPersonalInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelEmployeeID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmployeeIDInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelFirstName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelFirstNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMiddleName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMiddleNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelLastName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLastNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelGender.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelGenderInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfBirth.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfBirthInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelEmail.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelEmailInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAddress.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAddressInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContact.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContactInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelMaritalStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelMaritalStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelNationality.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNationalityInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelRFIDTag.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelRFIDTagInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                // Employment Info
                labelEmploymentInformation.Font = AttributesClass.GetFont("Roboto-Regular", 15f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelManager.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelManagerInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelContractType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelContractTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfJoining.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfJoiningInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDateOfExit.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateOfExitInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);

                // Work Schedule
                labelShiftSchedule.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelWorkHoursInputA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHours.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkHoursInputA.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkHoursInputB.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelAltWorkDays.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelAltWorkDaysInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        // Section 1: Employee Details
        private async Task LoadEmployeeDetails()
        {
            try
            {
                var emp = await firebase
                    .Child("EmployeeDetails")
                    .Child(employeeId)
                    .OnceSingleAsync<dynamic>();

                if (emp != null)
                {
                    labelEmployeeIDInput.Text = emp?.employee_id ?? "N/A";
                    labelFirstNameInput.Text = emp?["first_name"]?.ToString() ?? "N/A";
                    labelMiddleNameInput.Text = emp?["middle_name"]?.ToString() ?? "N/A";
                    labelLastNameInput.Text = emp?["last_name"]?.ToString() ?? "N/A";
                    labelGenderInput.Text = emp?["gender"]?.ToString() ?? "N/A";
                    if (!string.IsNullOrEmpty(emp?.date_of_birth?.ToString()))
                    {
                        if (DateTime.TryParse(emp.date_of_birth.ToString(), out DateTime dob))
                            labelDateOfBirthInput.Text = dob.ToString("yyyy-MM-dd"); // Only date
                        else
                            labelDateOfBirthInput.Text = emp.date_of_birth.ToString();
                    }
                    else
                    {
                        labelDateOfBirthInput.Text = "N/A";
                    }
                    labelEmailInput.Text = emp?["email"]?.ToString() ?? "N/A";
                    labelAddressInput.Text = emp?["address"]?.ToString() ?? "N/A";
                    labelContactInput.Text = emp?["contact"]?.ToString() ?? "N/A";
                    labelMaritalStatusInput.Text = emp?.marital_status ?? "N/A";
                    labelNationalityInput.Text = emp?.nationality ?? "N/A";
                    labelRFIDTagInput.Text = emp?.rfid_tag ?? "No RFID assigned";  // ✅ custom fallback
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee details: " + ex.Message);
            }
        }

        private async Task LoadUserPassword()
        {
            try
            {
                var users = await firebase.Child("Users").OnceAsync<dynamic>();

                foreach (var u in users)
                {
                    // u.Key is the Firebase key, u.Object is the payload
                    dynamic obj = u.Object;
                    string empId = obj?.employee_id;
                    if (empId == employeeId)
                    {
                        // 1) If plaintext password exists (insecure) show it:
                        var plaintext = (string)obj?.password; // common insecure name

                        // 2) Otherwise show the stored hash (likely)
                        var hash = (string)obj?.password_hash ?? (string)obj?.passwordHash ?? (string)obj?.hash;

                        if (!string.IsNullOrEmpty(plaintext))
                        {
                            // WARNING: displaying plaintext is insecure — do this only for trusted debug/admin
                            labelPasswordInput.Text = plaintext;
                        }
                        else if (!string.IsNullOrEmpty(hash))
                        {
                            // Show the stored hash (this is not the plaintext)
                            labelPasswordInput.Text = hash;
                        }
                        else
                        {
                            labelPasswordInput.Text = "No password record";
                        }

                        return;
                    }
                }

                // no user record found
                labelPasswordInput.Text = "No login";
            }
            catch (Exception ex)
            {
                labelPasswordInput.Text = "Error";
                Console.WriteLine("LoadUserPassword error: " + ex.Message);
            }
        }

        // Section 2: Employment Information
        private async Task LoadEmploymentInfo()   // ✅ corrected name
        {
            try
            {
                var employmentData = await firebase
                    .Child("EmploymentInfo")
                    .OnceSingleAsync<object>();

                if (employmentData != null)
                {
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

        // Section 3: Work Schedule
        private async Task LoadWorkSchedule()   // ✅ corrected name
        {
            try
            {
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
    }
}
