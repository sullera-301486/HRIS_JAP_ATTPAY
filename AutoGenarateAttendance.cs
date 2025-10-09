using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public class AttendanceGenerator
    {
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        private Dictionary<string, List<EmployeeSchedule>> employeeSchedules = new Dictionary<string, List<EmployeeSchedule>>();

        public async Task GenerateWeeklyAttendance(DateTime startDate)
        {
            try
            {
                // Load employee schedules first
                await LoadEmployeeSchedules();

                // Get all active employees (excluding archived)
                var employees = await GetActiveEmployees();

                if (employees.Count == 0)
                {
                    Console.WriteLine("No active employees found to generate attendance for.");
                    return;
                }

                // Generate attendance for Monday to Saturday of the specified week
                for (int i = 0; i < 6; i++) // Monday (0) to Saturday (5)
                {
                    DateTime currentDate = startDate.AddDays(i);

                    // Skip if it's Sunday
                    if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    await GenerateDailyAttendance(currentDate, employees);
                }

                Console.WriteLine("Weekly attendance generated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating weekly attendance: {ex.Message}");
                throw;
            }
        }

        public async Task GenerateDailyAttendance(DateTime date, List<Employee> employees = null)
        {
            try
            {
                // Load employee schedules if not already loaded
                if (employeeSchedules.Count == 0)
                {
                    await LoadEmployeeSchedules();
                }

                if (employees == null)
                    employees = await GetActiveEmployees();

                if (employees.Count == 0)
                {
                    Console.WriteLine("No active employees found to generate attendance for.");
                    return;
                }

                // Check if attendance already exists for this date
                bool attendanceExists = await CheckAttendanceExists(date);
                if (attendanceExists)
                {
                    Console.WriteLine($"Attendance for {date:yyyy-MM-dd} already exists. Skipping...");
                    return;
                }

                var attendanceRecords = new List<object>();

                foreach (var employee in employees)
                {
                    var attendanceRecord = CreateInitialAttendanceRecord(employee, date);
                    attendanceRecords.Add(attendanceRecord);
                }

                // Save to Firebase
                await SaveAttendanceRecords(attendanceRecords);

                Console.WriteLine($"Initial attendance for {date:yyyy-MM-dd} generated successfully for {employees.Count} employees!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating daily attendance: {ex.Message}");
                throw;
            }
        }

        private object CreateInitialAttendanceRecord(Employee employee, DateTime date)
        {
            // Check if employee has schedule for this day
            var schedule = GetEmployeeScheduleForDay(employee.EmployeeId, date);

            if (schedule == null)
            {
                // Employee has no schedule for this day - mark as Day Off
                return new
                {
                    employee_id = employee.EmployeeId,
                    attendance_date = date.ToString("yyyy-MM-dd"),
                    time_in = "N/A",
                    time_out = "N/A",
                    hours_worked = "0.00",
                    overtime_hours = "0.00",
                    status = "Day Off",
                    verification_method = "System Generated",
                    created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    is_generated = true,
                    is_pending = false,  // No pending updates for day off
                    schedule_id = ""
                };
            }
            else
            {
                // Employee has schedule - mark as Absent initially
                return new
                {
                    employee_id = employee.EmployeeId,
                    attendance_date = date.ToString("yyyy-MM-dd"),
                    time_in = "N/A",
                    time_out = "N/A",
                    hours_worked = "0.00",
                    overtime_hours = "0.00",
                    status = "Absent",
                    verification_method = "Pending RFID",
                    created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    is_generated = true,
                    is_pending = true,  // Flag to indicate this record needs RFID update
                    schedule_id = schedule.schedule_id
                };
            }
        }

        private EmployeeSchedule GetEmployeeScheduleForDay(string employeeId, DateTime date)
        {
            try
            {
                var dayOfWeek = date.DayOfWeek.ToString();

                if (employeeSchedules.ContainsKey(employeeId))
                {
                    var normalizedTargetDay = NormalizeDayName(dayOfWeek);

                    foreach (var schedule in employeeSchedules[employeeId])
                    {
                        if (schedule != null && !string.IsNullOrEmpty(schedule.day_of_week))
                        {
                            var normalizedScheduleDay = NormalizeDayName(schedule.day_of_week);

                            if (string.Equals(normalizedScheduleDay, normalizedTargetDay, StringComparison.OrdinalIgnoreCase))
                            {
                                return schedule;
                            }
                        }
                    }
                }

                return null; // No schedule found for this day
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting schedule for employee {employeeId} on {date:yyyy-MM-dd}: {ex.Message}");
                return null;
            }
        }

        private string NormalizeDayName(string dayName)
        {
            if (string.IsNullOrEmpty(dayName))
                return dayName;

            var normalized = dayName.Trim().ToLower();

            switch (normalized)
            {
                case "monday": case "mon": return "monday";
                case "tuesday": case "tue": return "tuesday";
                case "wednesday": case "wed": return "wednesday";
                case "thursday": case "thu": return "thursday";
                case "friday": case "fri": return "friday";
                case "saturday": case "sat": return "saturday";
                case "sunday": case "sun": return "sunday";
                default: return normalized;
            }
        }

        private async Task LoadEmployeeSchedules()
        {
            try
            {
                employeeSchedules.Clear();

                var schedulesData = await firebase.Child("Work_Schedule").OnceSingleAsync<JArray>();
                if (schedulesData == null)
                {
                    Console.WriteLine("No work schedule data found.");
                    return;
                }

                foreach (var scheduleItem in schedulesData)
                {
                    if (scheduleItem != null && scheduleItem.HasValues)
                    {
                        var schedule = new EmployeeSchedule
                        {
                            schedule_id = scheduleItem["schedule_id"]?.ToString(),
                            employee_id = scheduleItem["employee_id"]?.ToString(),
                            day_of_week = scheduleItem["day_of_week"]?.ToString(),
                            start_time = scheduleItem["start_time"]?.ToString(),
                            end_time = scheduleItem["end_time"]?.ToString(),
                            schedule_type = scheduleItem["schedule_type"]?.ToString()
                        };

                        if (!string.IsNullOrEmpty(schedule.employee_id))
                        {
                            if (!employeeSchedules.ContainsKey(schedule.employee_id))
                            {
                                employeeSchedules[schedule.employee_id] = new List<EmployeeSchedule>();
                            }
                            employeeSchedules[schedule.employee_id].Add(schedule);
                        }
                    }
                }

                Console.WriteLine($"Loaded schedules for {employeeSchedules.Count} employees.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee schedules: {ex.Message}");
            }
        }

        // Method to update attendance when RFID is tapped - updated to check schedules
        public async Task UpdateAttendanceFromRFID(string employeeId, DateTime tapTime, bool isTimeIn)
        {
            try
            {
                string today = DateTime.Today.ToString("yyyy-MM-dd");

                // Check if employee has schedule for today
                var schedule = GetEmployeeScheduleForDay(employeeId, DateTime.Today);
                if (schedule == null)
                {
                    Console.WriteLine($"Employee {employeeId} has no schedule for today. Day off - no attendance update.");
                    return;
                }

                // Get today's attendance records
                var attendanceData = await firebase.Child("Attendance").OnceSingleAsync<JArray>();
                if (attendanceData == null) return;

                bool recordUpdated = false;

                for (int i = 0; i < attendanceData.Count; i++)
                {
                    var record = attendanceData[i];
                    string recordEmployeeId = record["employee_id"]?.ToString();
                    string recordDate = record["attendance_date"]?.ToString();
                    bool isPending = record["is_pending"]?.ToString() == "True";
                    string currentStatus = record["status"]?.ToString();

                    // Skip if record is "Day Off"
                    if (currentStatus == "Day Off")
                        continue;

                    if (recordEmployeeId == employeeId && recordDate == today && isPending)
                    {
                        if (isTimeIn)
                        {
                            // Update time in and change status from Absent to Pending (will be calculated later)
                            record["time_in"] = tapTime.ToString("yyyy-MM-dd HH:mm:ss");
                            record["status"] = "Pending Calculation";
                            record["verification_method"] = "RFID";
                        }
                        else
                        {
                            // Update time out
                            record["time_out"] = tapTime.ToString("yyyy-MM-dd HH:mm:ss");
                            record["verification_method"] = "RFID";

                            // If both time in and time out are set, calculate hours and status
                            string timeInStr = record["time_in"]?.ToString();
                            if (!string.IsNullOrEmpty(timeInStr) && timeInStr != "N/A")
                            {
                                CalculateAndUpdateAttendance(record, tapTime, schedule);
                            }
                        }

                        // Remove pending flag since record has been updated
                        record["is_pending"] = false;

                        attendanceData[i] = record;
                        recordUpdated = true;
                        break;
                    }
                }

                if (recordUpdated)
                {
                    await firebase.Child("Attendance").PutAsync(attendanceData);
                    Console.WriteLine($"Attendance updated for employee {employeeId} at {tapTime:HH:mm:ss} ({(isTimeIn ? "Time In" : "Time Out")})");
                }
                else
                {
                    Console.WriteLine($"No pending attendance record found for employee {employeeId} on {today}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating attendance from RFID: {ex.Message}");
            }
        }

        private void CalculateAndUpdateAttendance(JToken record, DateTime timeOut, EmployeeSchedule schedule)
        {
            try
            {
                string timeInStr = record["time_in"]?.ToString();
                if (string.IsNullOrEmpty(timeInStr) || timeInStr == "N/A") return;

                if (DateTime.TryParse(timeInStr, out DateTime timeIn))
                {
                    // Calculate hours worked
                    TimeSpan hoursWorked = timeOut - timeIn;
                    double totalHours = Math.Round(hoursWorked.TotalHours, 2);

                    // Calculate overtime (anything over scheduled hours)
                    TimeSpan scheduledStart = ParseTimeSpan(schedule.start_time);
                    TimeSpan scheduledEnd = ParseTimeSpan(schedule.end_time);
                    double scheduledHours = (scheduledEnd - scheduledStart).TotalHours;
                    double overtimeHours = Math.Max(0, totalHours - scheduledHours);

                    // Calculate status based on schedule
                    string status = CalculateStatus(timeIn, timeOut, schedule);

                    // Update record
                    record["hours_worked"] = totalHours.ToString("0.00");
                    record["overtime_hours"] = overtimeHours.ToString("0.00");
                    record["status"] = status;
                    record["time_out"] = timeOut.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating attendance: {ex.Message}");
            }
        }

        private TimeSpan ParseTimeSpan(string timeString)
        {
            if (DateTime.TryParse(timeString, out DateTime time))
            {
                return time.TimeOfDay;
            }
            return new TimeSpan(8, 0, 0); // Default to 8:00 AM if parsing fails
        }

        private string CalculateStatus(DateTime timeIn, DateTime timeOut, EmployeeSchedule schedule)
        {
            TimeSpan scheduledStart = ParseTimeSpan(schedule.start_time);
            TimeSpan scheduledEnd = ParseTimeSpan(schedule.end_time);

            DateTime expectedStart = timeIn.Date.Add(scheduledStart);
            DateTime expectedEnd = timeIn.Date.Add(scheduledEnd);
            DateTime gracePeriodEnd = expectedStart.AddMinutes(15); // 15 minutes grace period

            bool isLate = timeIn > gracePeriodEnd;
            bool isEarlyOut = timeOut < expectedEnd;

            if (isLate && isEarlyOut)
                return "Late & Early Out";
            else if (isLate)
                return "Late";
            else if (isEarlyOut)
                return "Early Out";
            else
                return "On Time";
        }

        private async Task<List<Employee>> GetActiveEmployees()
        {
            try
            {
                var employees = new List<Employee>();

                // Get current employee details
                var employeeData = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                // Get archived employees to exclude them
                var archivedEmployees = await GetArchivedEmployeeIds();

                foreach (var emp in employeeData)
                {
                    string employeeId = emp.Key;

                    // Skip if employee is archived
                    if (archivedEmployees.Contains(employeeId))
                        continue;

                    var employee = new Employee
                    {
                        EmployeeId = employeeId,
                        employee_id = employeeId,
                        FirstName = emp.Object.first_name ?? "",
                        first_name = emp.Object.first_name ?? "",
                        MiddleName = emp.Object.middle_name ?? "",
                        middle_name = emp.Object.middle_name ?? "",
                        LastName = emp.Object.last_name ?? "",
                        last_name = emp.Object.last_name ?? "",
                        full_name = emp.Object.full_name ?? "",
                        EmploymentStatus = "Active",
                        RFIDTag = emp.Object.rfid_tag?.ToString() ?? "",
                        rfid_tag = emp.Object.rfid_tag?.ToString() ?? "",
                        email = emp.Object.email ?? "",
                        contact = emp.Object.contact ?? "",
                        address = emp.Object.address ?? "",
                        date_of_birth = emp.Object.date_of_birth ?? "",
                        gender = emp.Object.gender ?? "",
                        marital_status = emp.Object.marital_status ?? "",
                        nationality = emp.Object.nationality ?? "",
                        image_url = emp.Object.image_url ?? "",
                        created_at = emp.Object.created_at ?? ""
                    };

                    employees.Add(employee);
                }

                Console.WriteLine($"Found {employees.Count} active employees (excluding {archivedEmployees.Count} archived).");
                return employees;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active employees: {ex.Message}");
                return new List<Employee>();
            }
        }

        private async Task<HashSet<string>> GetArchivedEmployeeIds()
        {
            try
            {
                var archivedIds = new HashSet<string>();
                var archivedData = await firebase.Child("ArchivedEmployees").OnceAsync<dynamic>();

                foreach (var archived in archivedData)
                {
                    archivedIds.Add(archived.Key);
                }

                Console.WriteLine($"Found {archivedIds.Count} archived employees.");
                return archivedIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting archived employees: {ex.Message}");
                return new HashSet<string>();
            }
        }

        private async Task<bool> CheckAttendanceExists(DateTime date)
        {
            try
            {
                var attendanceData = await firebase.Child("Attendance").OnceSingleAsync<JArray>();
                if (attendanceData == null) return false;

                string targetDate = date.ToString("yyyy-MM-dd");

                foreach (var record in attendanceData)
                {
                    string recordDate = record["attendance_date"]?.ToString();
                    if (recordDate == targetDate)
                    {
                        // Check if we have records for this date
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task SaveAttendanceRecords(List<object> records)
        {
            try
            {
                // Get existing attendance data
                var existingData = await firebase.Child("Attendance").OnceSingleAsync<JArray>() ?? new JArray();

                // Add new records
                foreach (var record in records)
                {
                    existingData.Add(JObject.FromObject(record));
                }

                // Save back to Firebase
                await firebase.Child("Attendance").PutAsync(existingData);

                Console.WriteLine($"Saved {records.Count} initial attendance records to Firebase.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving attendance records: {ex.Message}");
                throw;
            }
        }

        public DateTime GetCurrentWeekMonday()
        {
            DateTime today = DateTime.Today;
            int daysToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return today.AddDays(-daysToMonday);
        }

        public DateTime GetNextWeekMonday()
        {
            return GetCurrentWeekMonday().AddDays(7);
        }

        public DateTime GetPreviousWeekMonday()
        {
            return GetCurrentWeekMonday().AddDays(-7);
        }

        public async Task<bool> IsWeekGenerated(DateTime weekMonday)
        {
            // Check if Saturday of that week has attendance records
            DateTime saturday = weekMonday.AddDays(5);
            return await CheckAttendanceExists(saturday);
        }

        // Method to get employee by RFID tag
        public async Task<string> GetEmployeeIdByRFID(string rfidTag)
        {
            try
            {
                var employeeData = await firebase.Child("EmployeeDetails").OnceAsync<dynamic>();

                foreach (var emp in employeeData)
                {
                    string employeeRFID = emp.Object.rfid_tag?.ToString() ?? "";
                    if (employeeRFID == rfidTag)
                    {
                        return emp.Key; // Return employee ID
                    }
                }

                return null; // RFID not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee by RFID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<DateTime>> GetMissingAttendanceDates(DateTime startDate, DateTime endDate)
        {
            var missingDates = new List<DateTime>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                bool exists = await CheckAttendanceExists(date);
                if (!exists)
                {
                    missingDates.Add(date);
                }
            }

            return missingDates;
        }

        // Method to get employee schedule summary
        public async Task<Dictionary<string, string>> GetEmployeeScheduleSummary()
        {
            await LoadEmployeeSchedules();

            var summary = new Dictionary<string, string>();
            var employees = await GetActiveEmployees();

            foreach (var employee in employees)
            {
                var schedules = employeeSchedules.ContainsKey(employee.EmployeeId)
                    ? employeeSchedules[employee.EmployeeId]
                    : new List<EmployeeSchedule>();

                if (schedules.Count == 0)
                {
                    summary[employee.EmployeeId] = "No Schedule";
                }
                else
                {
                    var scheduleDays = schedules.Select(s => s.day_of_week).Distinct();
                    summary[employee.EmployeeId] = string.Join(", ", scheduleDays);
                }
            }

            return summary;
        }
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

    public class Employee
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmploymentStatus { get; set; }
        public string RFIDTag { get; set; }
        public string employee_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string full_name { get; set; }
        public string rfid_tag { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public string department { get; set; }
        public string address { get; set; }
        public string date_of_birth { get; set; }
        public string gender { get; set; }
        public string marital_status { get; set; }
        public string nationality { get; set; }
        public string image_url { get; set; }
        public string created_at { get; set; }
    }
}