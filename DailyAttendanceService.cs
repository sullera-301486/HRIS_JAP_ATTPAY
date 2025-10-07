using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;

namespace HRIS_JAP_ATTPAY
{
    public class DailyAttendanceService
    {
        private readonly string _firebaseUrl;
        private readonly HttpClient _httpClient;

        public DailyAttendanceService()
        {
            _firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            Debug.WriteLine(" Attendance Service Created");
        }

        public async Task GenerateTodaysAttendanceOnceAsync()
        {
            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                DayOfWeek todayDay = DateTime.Now.DayOfWeek;

                Debug.WriteLine($" CHECKING ATTENDANCE FOR: {today} ({todayDay})");

                //  Skip Sunday
                if (todayDay == DayOfWeek.Sunday)
                {
                    Debug.WriteLine(" Sunday detected - No attendance generation");
                    return;
                }

                //  Proceed for Monday to Saturday
                Debug.WriteLine($" {todayDay} detected - Generating attendance...");

                // Step 1: Get active employees
                var activeEmployeeIds = await GetActiveEmployeeIdsAsync();
                Debug.WriteLine($" Found {activeEmployeeIds.Count} active employees");

                if (activeEmployeeIds.Count == 0)
                {
                    Debug.WriteLine(" No active employees found");
                    return;
                }

                // Step 2: Check if we already have attendance for today
                bool alreadyExists = await CheckIfAttendanceExistsForDateAsync(today);
                if (alreadyExists)
                {
                    Debug.WriteLine(" Attendance for today already exists - Skipping generation");
                    return;
                }

                // Step 3: Create attendance records
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var newRecords = CreateAttendanceRecords(activeEmployeeIds, today, currentTime);
                Debug.WriteLine($" Created {newRecords.Count} attendance records");

                // Step 4: Save to Firebase - FIXED: Use dictionary approach
                bool success = await SaveToFirebaseDictionaryAsync(newRecords);

                if (success)
                {
                    Debug.WriteLine($" SUCCESS! Generated {newRecords.Count} attendance records for {todayDay}");
                    MessageBox.Show($"Successfully generated attendance for {newRecords.Count} employees today ({todayDay})!",
                                  "Attendance Generated",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
                else
                {
                    Debug.WriteLine(" FAILED to save attendance records");
                    MessageBox.Show("Failed to generate attendance records",
                                  "Error",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ERROR: {ex.Message}");
                Debug.WriteLine($" STACK TRACE: {ex.StackTrace}");
                MessageBox.Show($"Error generating attendance: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> GetActiveEmployeeIdsAsync()
        {
            var activeIds = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync($"{_firebaseUrl}EmployeeDetails.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);

                    if (employees != null)
                    {
                        // Filter out archived employees
                        var archivedResponse = await _httpClient.GetAsync($"{_firebaseUrl}ArchivedEmployees.json");
                        var archived = new Dictionary<string, JObject>();

                        if (archivedResponse.IsSuccessStatusCode)
                        {
                            var archivedJson = await archivedResponse.Content.ReadAsStringAsync();
                            archived = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(archivedJson) ?? new Dictionary<string, JObject>();
                        }

                        foreach (var emp in employees)
                        {
                            if (!archived.ContainsKey(emp.Key))
                            {
                                activeIds.Add(emp.Key);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting employees: {ex.Message}");
            }

            return activeIds;
        }

        private async Task<bool> CheckIfAttendanceExistsForDateAsync(string date)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_firebaseUrl}Attendance.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // FIXED: Handle both dictionary and array formats
                    if (json.StartsWith("{"))
                    {
                        // Dictionary format
                        var attendanceDict = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);
                        if (attendanceDict != null)
                        {
                            return attendanceDict.Values.Any(a => a != null && a["attendance_date"]?.ToString() == date);
                        }
                    }
                    else if (json.StartsWith("["))
                    {
                        // Array format (fallback)
                        var attendanceList = JsonConvert.DeserializeObject<List<JObject>>(json);
                        if (attendanceList != null)
                        {
                            return attendanceList.Any(a => a != null && a["attendance_date"]?.ToString() == date);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" Error checking attendance: {ex.Message}");
            }

            return false;
        }

        private List<AttendanceRecord> CreateAttendanceRecords(List<string> employeeIds, string date, string createdDate)
        {
            var records = new List<AttendanceRecord>();

            foreach (var empId in employeeIds)
            {
                records.Add(new AttendanceRecord
                {
                    attendance_date = date,
                    employee_id = empId,
                    hours_worked = "0.00",
                    overtime_hours = "0.00",
                    overtime_in = "N/A",
                    overtime_out = "N/A",
                    schedule_id = "",
                    status = "Absent",
                    time_in = "N/A",
                    time_out = "N/A",
                    verification_method = "System Generated",
                    created_date = createdDate,
                    is_generated = true
                });
            }

            return records;
        }

        // FIXED: Use dictionary approach to save attendance
        private async Task<bool> SaveToFirebaseDictionaryAsync(List<AttendanceRecord> newRecords)
        {
            try
            {
                // Get existing attendance as dictionary
                var response = await _httpClient.GetAsync($"{_firebaseUrl}Attendance.json");
                Dictionary<string, AttendanceRecord> allAttendance = new Dictionary<string, AttendanceRecord>();

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"📥 Existing attendance JSON: {json}");

                    if (!string.IsNullOrEmpty(json) && json != "null")
                    {
                        if (json.StartsWith("{"))
                        {
                            // It's a dictionary
                            var existingDict = JsonConvert.DeserializeObject<Dictionary<string, AttendanceRecord>>(json);
                            if (existingDict != null)
                            {
                                allAttendance = existingDict;
                            }
                        }
                        else if (json.StartsWith("["))
                        {
                            // It's an array - convert to dictionary
                            var existingArray = JsonConvert.DeserializeObject<List<AttendanceRecord>>(json);
                            if (existingArray != null)
                            {
                                int counter = 1;
                                foreach (var record in existingArray)
                                {
                                    if (record != null)
                                    {
                                        allAttendance[counter.ToString()] = record;
                                        counter++;
                                    }
                                }
                            }
                        }
                    }
                }

                Debug.WriteLine($"📊 Existing records count: {allAttendance.Count}");

                // Find the next available key
                int nextKey = 1;
                if (allAttendance.Count > 0)
                {
                    // Get the highest numeric key and increment
                    var numericKeys = allAttendance.Keys
                        .Where(k => int.TryParse(k, out _))
                        .Select(k => int.Parse(k));

                    nextKey = numericKeys.Any() ? numericKeys.Max() + 1 : 1;
                }

                Debug.WriteLine($" Next available key: {nextKey}");

                // Add new records with sequential keys
                foreach (var record in newRecords)
                {
                    allAttendance[nextKey.ToString()] = record;
                    nextKey++;
                }

                Debug.WriteLine($" Saving {allAttendance.Count} total records");

                // Save back to Firebase
                var jsonToSave = JsonConvert.SerializeObject(allAttendance, Formatting.None);
                var content = new StringContent(jsonToSave, System.Text.Encoding.UTF8, "application/json");

                var putResponse = await _httpClient.PutAsync($"{_firebaseUrl}Attendance.json", content);

                if (putResponse.IsSuccessStatusCode)
                {
                    Debug.WriteLine(" Successfully saved attendance records");
                    return true;
                }
                else
                {
                    Debug.WriteLine($" Failed to save: {putResponse.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" Save error: {ex.Message}");
                Debug.WriteLine($" Save stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public class AttendanceRecord
        {
            public string attendance_date { get; set; }
            public string employee_id { get; set; }
            public string hours_worked { get; set; } = "0.00";
            public string overtime_hours { get; set; } = "0.00";
            public string overtime_in { get; set; } = "N/A";
            public string overtime_out { get; set; } = "N/A";
            public string schedule_id { get; set; } = "";
            public string status { get; set; } = "Absent";
            public string time_in { get; set; } = "N/A";
            public string time_out { get; set; } = "N/A";
            public string verification_method { get; set; } = "System Generated";
            public string created_date { get; set; }
            public bool is_generated { get; set; } = true;
        }
    }
}