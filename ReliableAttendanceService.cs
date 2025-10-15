using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public class ReliableAttendanceService
    {
        private readonly string _firebaseUrl;
        private readonly HttpClient _httpClient;

        public ReliableAttendanceService()
        {
            _firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            Debug.WriteLine("Reliable Attendance Service Created");
        }

        public async Task GenerateTodaysAttendanceOnceAsync()
        {
            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                DayOfWeek todayDay = DateTime.Now.DayOfWeek;

                Debug.WriteLine($" CHECKING ATTENDANCE FOR: {today} ({todayDay})");

                // Skip Sunday
                if (todayDay == DayOfWeek.Sunday)
                {
                    Debug.WriteLine(" Sunday detected - No attendance generation");
                    return;
                }

                // Proceed for Monday to Saturday
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

                // Step 3: Create attendance records with DYNAMIC schedule ID matching
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var newRecords = await CreateAttendanceRecordsWithDynamicScheduleIdsAsync(activeEmployeeIds, today, currentTime);

                if (newRecords == null || newRecords.Count == 0)
                {
                    Debug.WriteLine(" No attendance records created - possible error in schedule lookup");
                    return;
                }

                Debug.WriteLine($" Created {newRecords.Count} attendance records");

                // Step 4: Save to Firebase
                bool success = await SaveToFirebaseDictionaryAsync(newRecords);

                if (success)
                {
                    var dayOffCount = newRecords.Count(r => r.status == "Day Off");
                    var absentCount = newRecords.Count(r => r.status == "Absent");

                    Debug.WriteLine($" SUCCESS! Generated {newRecords.Count} attendance records for {todayDay}");
                    Debug.WriteLine($" Summary - Day Off: {dayOffCount}, Absent: {absentCount}");

                    MessageBox.Show($"Successfully generated attendance for {newRecords.Count} employees today ({todayDay})!\nDay Off: {dayOffCount}, Absent: {absentCount}",
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
                Debug.WriteLine($" Error getting employees: {ex.Message}");
            }

            return activeIds;
        }

        private async Task<Dictionary<string, List<EmployeeSchedule>>> GetAllEmployeeSchedulesAsync()
        {
            var allSchedules = new Dictionary<string, List<EmployeeSchedule>>();

            try
            {
                var response = await _httpClient.GetAsync($"{_firebaseUrl}Work_Schedule.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    List<EmployeeSchedule> schedulesList = new List<EmployeeSchedule>();

                    // Handle both array and dictionary formats
                    if (json.StartsWith("["))
                    {
                        schedulesList = JsonConvert.DeserializeObject<List<EmployeeSchedule>>(json) ?? new List<EmployeeSchedule>();
                        // FILTER OUT NULL ENTRIES
                        schedulesList = schedulesList.Where(s => s != null).ToList();
                    }
                    else if (json.StartsWith("{"))
                    {
                        var schedulesDict = JsonConvert.DeserializeObject<Dictionary<string, EmployeeSchedule>>(json);
                        schedulesList = schedulesDict?.Values.Where(s => s != null).ToList() ?? new List<EmployeeSchedule>();
                    }

                    // Group schedules by employee_id AND FILTER OUT NULL employee_id
                    allSchedules = schedulesList
                        .Where(s => !string.IsNullOrEmpty(s.employee_id))
                        .GroupBy(s => s.employee_id)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    Debug.WriteLine($" Loaded schedules for {allSchedules.Count} employees");

                    // Debug: Show schedule counts per employee
                    foreach (var empSchedule in allSchedules)
                    {
                        Debug.WriteLine($"   - {empSchedule.Key}: {empSchedule.Value.Count} schedules");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" Error loading all schedules: {ex.Message}");
            }

            return allSchedules;
        }

        private string FindScheduleIdForDay(string employeeId, string dayOfWeek, Dictionary<string, List<EmployeeSchedule>> allSchedules)
        {
            if (string.IsNullOrEmpty(employeeId) || !allSchedules.ContainsKey(employeeId))
            {
                Debug.WriteLine($" No schedules found for employee {employeeId}");
                return "";
            }

            var employeeSchedules = allSchedules[employeeId];
            string normalizedTargetDay = NormalizeDayOfWeek(dayOfWeek);

            // Find the schedule for this specific day WITH NULL SAFETY
            var scheduleForDay = employeeSchedules.FirstOrDefault(s =>
                s != null && // ADD NULL CHECK
                !string.IsNullOrEmpty(s.day_of_week) &&
                NormalizeDayOfWeek(s.day_of_week) == normalizedTargetDay);

            if (scheduleForDay != null)
            {
                Debug.WriteLine($" Found schedule for {employeeId} on {dayOfWeek}: ID {scheduleForDay.schedule_id}");
                return scheduleForDay.schedule_id ?? "";
            }

            Debug.WriteLine($" No schedule found for {employeeId} on {dayOfWeek}");
            return "";
        }

        private async Task<List<AttendanceRecord>> CreateAttendanceRecordsWithDynamicScheduleIdsAsync(List<string> employeeIds, string date, string createdDate)
        {
            var records = new List<AttendanceRecord>();
            var dayOfWeek = DateTime.Parse(date).DayOfWeek.ToString();

            Debug.WriteLine($" Creating attendance records for {date} ({dayOfWeek})");

            // Load ALL schedules to ensure we have the latest data
            var allSchedules = await GetAllEmployeeSchedulesAsync();

            foreach (var empId in employeeIds)
            {
                // ALWAYS find the CURRENT schedule ID dynamically - never use cached/old data
                string currentScheduleId = FindScheduleIdForDay(empId, dayOfWeek, allSchedules);

                if (!string.IsNullOrEmpty(currentScheduleId))
                {
                    // Employee has schedule for this day - mark as Absent initially
                    records.Add(new AttendanceRecord
                    {
                        attendance_date = date,
                        employee_id = empId,
                        hours_worked = "0.00",
                        overtime_hours = "0.00",
                        overtime_in = "N/A",
                        overtime_out = "N/A",
                        schedule_id = currentScheduleId, // This will ALWAYS be the current/latest ID
                        status = "Absent",
                        time_in = "N/A",
                        time_out = "N/A",
                        verification_method = "System Generated",
                        created_date = createdDate,
                        is_generated = true
                    });

                    Debug.WriteLine($"Created attendance for {empId} with CURRENT schedule ID: {currentScheduleId}");
                }
                else
                {
                    // No schedule - mark as Day Off
                    records.Add(new AttendanceRecord
                    {
                        attendance_date = date,
                        employee_id = empId,
                        hours_worked = "0.00",
                        overtime_hours = "0.00",
                        overtime_in = "N/A",
                        overtime_out = "N/A",
                        schedule_id = "", // No schedule ID for day off
                        status = "Day Off",
                        time_in = "N/A",
                        time_out = "N/A",
                        verification_method = "System Generated",
                        created_date = createdDate,
                        is_generated = true
                    });

                    Debug.WriteLine($"❌ No schedule for {empId} - marked as Day Off");
                }
            }

            Debug.WriteLine($" Created {records.Count(r => !string.IsNullOrEmpty(r.schedule_id))} records with schedule IDs");
            Debug.WriteLine($" Created {records.Count(r => string.IsNullOrEmpty(r.schedule_id))} records as Day Off");

            return records;
        }

        private async Task<bool> CheckIfAttendanceExistsForDateAsync(string date)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_firebaseUrl}Attendance.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Handle both dictionary and array formats
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

        // Helper method to normalize day names for comparison
        private string NormalizeDayOfWeek(string day)
        {
            if (string.IsNullOrEmpty(day)) return day;

            string normalized = day.Trim().ToLower();

            // Handle common abbreviations and variations
            if (normalized == "mon" || normalized == "monday")
                return "monday";
            else if (normalized == "tue" || normalized == "tues" || normalized == "tuesday")
                return "tuesday";
            else if (normalized == "wed" || normalized == "wednesday")
                return "wednesday";
            else if (normalized == "thu" || normalized == "thur" || normalized == "thurs" || normalized == "thursday")
                return "thursday";
            else if (normalized == "fri" || normalized == "friday")
                return "friday";
            else if (normalized == "sat" || normalized == "saturday")
                return "saturday";
            else if (normalized == "sun" || normalized == "sunday")
                return "sunday";
            else
                return normalized;
        }

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
                    Debug.WriteLine($" Existing attendance JSON: {json}");

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

                Debug.WriteLine($" Existing records count: {allAttendance.Count}");

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

        // Method to fix existing records with outdated schedule IDs
        public async Task FixOutdatedScheduleIdsAsync()
        {
            try
            {
                Debug.WriteLine(" Fixing outdated schedule IDs in existing attendance records...");

                // Get current schedules
                var allSchedules = await GetAllEmployeeSchedulesAsync();

                // Get all attendance records
                var response = await _httpClient.GetAsync($"{_firebaseUrl}Attendance.json");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(" Failed to fetch attendance records");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    Debug.WriteLine(" No attendance records found");
                    return;
                }

                Dictionary<string, AttendanceRecord> allAttendance = new Dictionary<string, AttendanceRecord>();
                bool hasChanges = false;
                int updatedCount = 0;

                // Parse existing attendance
                if (json.StartsWith("{"))
                {
                    allAttendance = JsonConvert.DeserializeObject<Dictionary<string, AttendanceRecord>>(json) ?? new Dictionary<string, AttendanceRecord>();
                }

                Debug.WriteLine($" Processing {allAttendance.Count} attendance records...");

                // Fix records with potentially outdated schedule IDs
                foreach (var attendanceEntry in allAttendance)
                {
                    var record = attendanceEntry.Value;

                    // ADD NULL CHECK FOR RECORD
                    if (record != null &&
                        !string.IsNullOrEmpty(record.attendance_date) &&
                        !string.IsNullOrEmpty(record.employee_id))
                    {
                        // Get the day of week from attendance date
                        if (DateTime.TryParse(record.attendance_date, out DateTime attendanceDate))
                        {
                            var dayOfWeek = attendanceDate.DayOfWeek.ToString();
                            var currentScheduleId = FindScheduleIdForDay(record.employee_id, dayOfWeek, allSchedules);

                            // Update if we found a different schedule ID or if current one is empty
                            if (!string.IsNullOrEmpty(currentScheduleId) && record.schedule_id != currentScheduleId)
                            {
                                Debug.WriteLine($" Updated schedule ID for {record.employee_id} on {record.attendance_date}: {record.schedule_id} -> {currentScheduleId}");
                                record.schedule_id = currentScheduleId;
                                hasChanges = true;
                                updatedCount++;
                            }
                        }
                    }
                }

                // Save changes if any
                if (hasChanges)
                {
                    var jsonToSave = JsonConvert.SerializeObject(allAttendance, Formatting.None);
                    var content = new StringContent(jsonToSave, System.Text.Encoding.UTF8, "application/json");

                    var putResponse = await _httpClient.PutAsync($"{_firebaseUrl}Attendance.json", content);

                    if (putResponse.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($" Successfully fixed {updatedCount} outdated schedule IDs");
                        MessageBox.Show($"Successfully updated {updatedCount} schedule IDs in existing attendance records!", "Schedule IDs Fixed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Debug.WriteLine($" Failed to save fixed schedule IDs: {putResponse.StatusCode}");
                    }
                }
                else
                {
                    Debug.WriteLine(" No outdated schedule IDs found");
                    MessageBox.Show("No outdated schedule IDs found in attendance records.", "No Changes Needed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" Error fixing schedule IDs: {ex.Message}");
                MessageBox.Show($"Error fixing schedule IDs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public class EmployeeSchedule
        {
            public string schedule_id { get; set; }
            public string employee_id { get; set; }
            public string day_of_week { get; set; }
            public string start_time { get; set; }
            public string end_time { get; set; }
            public string schedule_type { get; set; }
        }
    }
}