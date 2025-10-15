using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text; 

namespace HRIS_JAP_ATTPAY
{
    public class RFIDAttendanceHandler
    {
        private HttpClient client;
        private string firebaseUrl = "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/";
        private Dictionary<string, Employee> employees = new Dictionary<string, Employee>();
        private Dictionary<string, List<EmployeeSchedule>> employeeSchedules = new Dictionary<string, List<EmployeeSchedule>>();
        private string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Events for UI updates
        public event Action<string> OnScanHistoryUpdate;
        public event Action<string, string, string, string, string, string> OnEmployeeInfoUpdate;
        public event Action<List<string>> OnEmployeeListUpdate;

        public RFIDAttendanceHandler()
        {
            InitializeFirebase();
        }

        public async Task InitializeAsync()
        {
            await LoadEmployeeData();
            await LoadEmployeeDepartmentData();
            await LoadEmployeeSchedulesData();
        }

        private void InitializeFirebase()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(firebaseUrl);
        }

        public async Task ProcessRFIDScan(string rfidTag)
        {
            try
            {
                if (!employees.ContainsKey(rfidTag))
                {
                    AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] Unknown RFID: {rfidTag}");
                    OnScanHistoryUpdate?.Invoke($"Unknown RFID: {rfidTag}");
                    return;
                }

                var employee = employees[rfidTag];
                var currentTime = DateTime.Now;
                var existingRecord = await GetTodaysAttendance(employee.employee_id);

                // Check if employee has schedule for today
                var todaysSchedule = GetTodaysScheduleForEmployee(employee);

                if (todaysSchedule == null)
                {
                    AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] No work schedule found for today for employee: {employee.employee_id}");
                    OnScanHistoryUpdate?.Invoke($"No work schedule configured for today for employee: {employee.first_name} {employee.last_name}");
                    return;
                }

                // Debug overtime timing
                DebugOvertimeTiming(employee, todaysSchedule);

                if (existingRecord == null || string.IsNullOrEmpty(existingRecord.time_in))
                {
                    // TIME IN
                    await RecordTimeIn(employee, todaysSchedule, currentTime);
                    DisplayEmployeeInfo(employee, "TIME IN", CalculateTimeInStatus(employee, todaysSchedule, currentTime), "N/A");
                    AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - TIME IN - {CalculateTimeInStatus(employee, todaysSchedule, currentTime)}");
                }
                else
                {
                    // Determine what type of scan this is based on employee's schedule
                    var scanType = DetermineScanType(existingRecord, employee, todaysSchedule, currentTime);

                    switch (scanType)
                    {
                        case "TIME_OUT":
                            await RecordTimeOut(existingRecord, employee, todaysSchedule, currentTime);
                            DisplayEmployeeInfo(employee, "TIME OUT", CalculateTimeOutStatus(employee, todaysSchedule, currentTime), "N/A");
                            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - TIME OUT - {CalculateTimeOutStatus(employee, todaysSchedule, currentTime)}");
                            break;

                        case "OVERTIME_IN":
                            await RecordOvertimeIn(existingRecord, employee, currentTime);
                            DisplayEmployeeInfo(employee, "OVERTIME IN", "Overtime Started", "N/A");
                            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - OVERTIME IN - Started at {currentTime:hh:mm tt}");
                            break;

                        case "OVERTIME_OUT":
                            await RecordOvertimeOut(existingRecord, employee, currentTime);
                            // Calculate overtime hours after recording overtime out
                            var updatedRecord = await GetTodaysAttendance(employee.employee_id);
                            var overtimeHours = updatedRecord?.overtime_hours ?? "N/A";
                            DisplayEmployeeInfo(employee, "OVERTIME OUT", "Overtime Completed", overtimeHours);
                            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - OVERTIME OUT - Completed at {currentTime:hh:mm tt} - Overtime: {overtimeHours} hours");
                            break;

                        case "WAITING_FOR_OVERTIME":
                            var regularEndTime = ParseTimeSpan(todaysSchedule.end_time);
                            var overtimeStartTime = regularEndTime.Add(TimeSpan.FromHours(1));
                            var waitingUntil = DateTime.Today.Add(overtimeStartTime);

                            var timeUntilOvertime = waitingUntil - currentTime;
                            DisplayEmployeeInfo(employee, "WAITING",
                                $"Regular hours done - Overtime starts at {waitingUntil:hh:mm tt} ({timeUntilOvertime.TotalMinutes:0} minutes)",
                                "N/A");
                            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - Waiting for overtime period (starts at {waitingUntil:hh:mm tt})");
                            break;

                        case "ALREADY_COMPLETED":
                            var overtimeHoursCompleted = existingRecord.overtime_hours ?? "N/A";
                            DisplayEmployeeInfo(employee, "COMPLETED", "All entries recorded today", overtimeHoursCompleted);
                            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - All entries already recorded today - Overtime: {overtimeHoursCompleted} hours");
                            break;

                        case "INVALID_SCAN_TIME":
                            DisplayEmployeeInfo(employee, "INVALID", "Cannot scan at this time", "N/A");
                            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] {employee.first_name} {employee.last_name} - Invalid scan time for current schedule");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Processing RFID scan: {ex.Message}");
                OnScanHistoryUpdate?.Invoke($"Error processing RFID scan: {ex.Message}");
            }
        }

        private async Task LoadEmployeeData()
        {
            try
            {
                var response = await client.GetAsync("EmployeeDetails.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employeeData = JsonConvert.DeserializeObject<Dictionary<string, Employee>>(json);

                    employees.Clear();
                    if (employeeData != null)
                    {
                        foreach (var kvp in employeeData)
                        {
                            if (kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.rfid_tag) && kvp.Value.rfid_tag != "N/A")
                            {
                                employees[kvp.Value.rfid_tag] = kvp.Value;
                            }
                        }
                        UpdateEmployeeList();
                        AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] Loaded {employees.Count} employees with RFID tags");
                    }
                    else
                    {
                        AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] No employee data found in Firebase");
                    }
                }
                else
                {
                    AddToScanHistory($"[ERROR] Failed to load employee data: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Loading employees: {ex.Message}");
            }
        }

        private async Task LoadEmployeeDepartmentData()
        {
            try
            {
                var response = await client.GetAsync("EmploymentInfo.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employmentData = JsonConvert.DeserializeObject<List<EmploymentInfo>>(json);

                    if (employmentData != null)
                    {
                        foreach (var employment in employmentData)
                        {
                            if (employment != null && !string.IsNullOrEmpty(employment.employee_id))
                            {
                                // Update employee department from employment info if available
                                var employee = employees.Values.FirstOrDefault(e => e.employee_id == employment.employee_id);
                                if (employee != null && string.IsNullOrEmpty(employee.department) && !string.IsNullOrEmpty(employment.department))
                                {
                                    employee.department = employment.department;
                                }
                            }
                        }
                        AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] Updated departments from employment info");
                    }
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Loading employment data: {ex.Message}");
            }
        }

        private async Task LoadEmployeeSchedulesData()
        {
            try
            {
                var response = await client.GetAsync("Work_Schedule.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    AddToScanHistory($"[DEBUG] Raw JSON response: {json}");

                    var schedulesList = JsonConvert.DeserializeObject<List<EmployeeSchedule>>(json);

                    employeeSchedules.Clear();
                    if (schedulesList != null)
                    {
                        AddToScanHistory($"[DEBUG] Found {schedulesList.Count} schedule entries");

                        foreach (var schedule in schedulesList)
                        {
                            if (schedule != null && !string.IsNullOrEmpty(schedule.employee_id))
                            {
                                AddToScanHistory($"[DEBUG] Processing schedule - EmployeeID: '{schedule.employee_id}', Day: '{schedule.day_of_week}', Start: '{schedule.start_time}', End: '{schedule.end_time}'");

                                if (!employeeSchedules.ContainsKey(schedule.employee_id))
                                {
                                    employeeSchedules[schedule.employee_id] = new List<EmployeeSchedule>();
                                }
                                employeeSchedules[schedule.employee_id].Add(schedule);
                            }
                        }

                        AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] Loaded schedules for {employeeSchedules.Count} employees");

                        // Debug: Show all available schedules
                        DebugAvailableSchedules();

                        UpdateEmployeeListWithSchedules();
                    }
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Loading schedules: {ex.Message}");
            }
        }

        private void UpdateEmployeeList()
        {
            UpdateEmployeeListWithSchedules();
        }

        private void UpdateEmployeeListWithSchedules()
        {
            var employeeList = new List<string>();
            foreach (var employee in employees.Values)
            {
                if (employee.rfid_tag != "N/A" && !string.IsNullOrEmpty(employee.rfid_tag))
                {
                    var scheduleInfo = GetEmployeeScheduleInfo(employee);
                    employeeList.Add($"{employee.employee_id}: {employee.first_name} {employee.last_name} ({scheduleInfo})");
                }
            }
            OnEmployeeListUpdate?.Invoke(employeeList);
        }

        private string GetEmployeeScheduleInfo(Employee employee)
        {
            var todaysSchedule = GetTodaysScheduleForEmployee(employee);
            if (todaysSchedule != null)
            {
                return $"{todaysSchedule.start_time} - {todaysSchedule.end_time}";
            }

            // Check if employee has any schedules at all
            if (employeeSchedules.ContainsKey(employee.employee_id))
            {
                var allSchedules = employeeSchedules[employee.employee_id];
                var scheduleDays = allSchedules.Select(s => s.day_of_week).Distinct();
                return $"Scheduled on: {string.Join(", ", scheduleDays)}";
            }

            return "No Schedule";
        }

        private EmployeeSchedule GetTodaysScheduleForEmployee(Employee employee)
        {
            var today = DateTime.Now.DayOfWeek.ToString();
            AddToScanHistory($"[DEBUG] === Checking schedule for {employee.employee_id} on {today} ===");

            if (!employeeSchedules.ContainsKey(employee.employee_id))
            {
                AddToScanHistory($"[DEBUG] ❌ No schedules found for employee: {employee.employee_id}");
                AddToScanHistory($"[DEBUG] Available employee IDs in schedules: {string.Join(", ", employeeSchedules.Keys)}");
                return null;
            }

            var todaySchedules = employeeSchedules[employee.employee_id];
            AddToScanHistory($"[DEBUG] Found {todaySchedules.Count} schedules for employee {employee.employee_id}");

            foreach (var schedule in todaySchedules)
            {
                AddToScanHistory($"[DEBUG] Checking schedule: Day='{schedule.day_of_week}', Start='{schedule.start_time}', End='{schedule.end_time}'");

                if (schedule.day_of_week != null)
                {
                    var scheduleDay = schedule.day_of_week.Trim();

                    // Enhanced day matching with full day names
                    var normalizedScheduleDay = NormalizeDayName(scheduleDay);
                    var normalizedToday = NormalizeDayName(today);

                    AddToScanHistory($"[DEBUG] Comparing: ScheduleDay='{scheduleDay}' (normalized: '{normalizedScheduleDay}') vs Today='{today}' (normalized: '{normalizedToday}')");

                    if (string.Equals(normalizedScheduleDay, normalizedToday, StringComparison.OrdinalIgnoreCase))
                    {
                        AddToScanHistory($"[DEBUG] ✅ MATCH FOUND for {employee.employee_id} on {today}");
                        return schedule;
                    }
                    else
                    {
                        AddToScanHistory($"[DEBUG] ❌ No match: '{normalizedScheduleDay}' != '{normalizedToday}'");
                    }
                }
                else
                {
                    AddToScanHistory($"[DEBUG] ❌ Schedule has null day_of_week");
                }
            }

            AddToScanHistory($"[DEBUG] ❌ No matching schedule found for {employee.employee_id} on {today}");
            return null;
        }

        private string NormalizeDayName(string dayName)
        {
            if (string.IsNullOrEmpty(dayName))
                return dayName;

            // Handle common day name variations
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

        private void DebugAvailableSchedules()
        {
            AddToScanHistory($"[DEBUG] === AVAILABLE SCHEDULES ===");
            foreach (var empId in employeeSchedules.Keys)
            {
                var schedules = employeeSchedules[empId];
                AddToScanHistory($"[DEBUG] Employee {empId} has {schedules.Count} schedules:");
                foreach (var schedule in schedules)
                {
                    if (schedule != null)
                    {
                        AddToScanHistory($"[DEBUG]   - Day: '{schedule.day_of_week}', Time: {schedule.start_time} to {schedule.end_time}");
                    }
                }
            }
            AddToScanHistory($"[DEBUG] Today is: {DateTime.Now.DayOfWeek}");
        }

        // METHOD 1: With 1-Hour Cooldown (Based on Schedule)
        private string DetermineScanType(AttendanceRecord record, Employee employee, EmployeeSchedule schedule, DateTime currentTime)
        {
            var now = currentTime.TimeOfDay;
            TimeSpan regularEndTime = ParseTimeSpan(schedule.end_time);
            TimeSpan overtimeStartTime = regularEndTime.Add(TimeSpan.FromHours(1)); // 1 hour after scheduled end time

            // 1️⃣ TIME OUT (anytime after time in, before overtime starts)
            if (string.IsNullOrEmpty(record.time_out) &&
                !string.IsNullOrEmpty(record.time_in) &&
                now < overtimeStartTime) // Can time out anytime before overtime period
            {
                return "TIME_OUT";
            }

            // 2️⃣ WAITING PERIOD BEFORE OVERTIME (between scheduled end time and overtime start)
            if (now >= regularEndTime &&
                now < overtimeStartTime &&
                !string.IsNullOrEmpty(record.time_out) &&
                string.IsNullOrEmpty(record.overtime_in))
            {
                return "WAITING_FOR_OVERTIME";
            }

            // 3️⃣ OVERTIME IN (1 hour after scheduled end time, regardless of when they tapped out)
            if (now >= overtimeStartTime &&
                string.IsNullOrEmpty(record.overtime_in) &&
                !string.IsNullOrEmpty(record.time_out))
            {
                return "OVERTIME_IN";
            }

            // 4️⃣ OVERTIME OUT
            if (!string.IsNullOrEmpty(record.overtime_in) &&
                string.IsNullOrEmpty(record.overtime_out))
            {
                return "OVERTIME_OUT";
            }

            // 5️⃣ COMPLETE
            if (!string.IsNullOrEmpty(record.overtime_out))
            {
                return "ALREADY_COMPLETED";
            }

            return "INVALID_SCAN_TIME";
        }

        private DateTime GetOvertimeStartTime(EmployeeSchedule schedule)
        {
            TimeSpan endTime = ParseTimeSpan(schedule.end_time);
            // Overtime starts exactly 1 hour after scheduled end time
            return DateTime.Today.Add(endTime).AddHours(1);
        }

        private TimeSpan ParseTimeSpan(string timeString)
        {
            if (DateTime.TryParse(timeString, out DateTime time))
            {
                return time.TimeOfDay;
            }
            return new TimeSpan(8, 0, 0); // Default to 8:00 AM if parsing fails
        }

        private async Task<AttendanceRecord> GetTodaysAttendance(string employeeId)
        {
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");

                // Fetch all attendance records once
                var response = await client.GetAsync("Attendance.json");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var allRecords = JsonConvert.DeserializeObject<Dictionary<string, AttendanceRecord>>(json);

                    if (allRecords != null)
                    {
                        foreach (var kvp in allRecords)
                        {
                            var rec = kvp.Value;
                            if (rec != null && rec.employee_id == employeeId && rec.attendance_date == today)
                            {
                                rec.RecordKey = kvp.Key; // Save the key for PUT updates

                                // Handle "N/A" values by converting them to empty strings
                                rec.time_in = rec.time_in == "N/A" ? "" : rec.time_in;
                                rec.time_out = rec.time_out == "N/A" ? "" : rec.time_out;
                                rec.overtime_in = rec.overtime_in == "N/A" ? "" : rec.overtime_in;
                                rec.overtime_out = rec.overtime_out == "N/A" ? "" : rec.overtime_out;

                                return rec; // Found today's record
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Getting attendance: {ex.Message}");
            }
            return null;
        }

        private string CalculateTimeInStatus(Employee employee, EmployeeSchedule schedule, DateTime currentTime)
        {
            var time = currentTime.TimeOfDay;
            var startTime = ParseTimeSpan(schedule.start_time);
            var gracePeriod = TimeSpan.FromMinutes(15);

            if (time <= startTime.Add(gracePeriod))
            {
                return time <= startTime ? "On Time" : "Late";
            }
            else
            {
                return "Late";
            }
        }

        private string CalculateTimeOutStatus(Employee employee, EmployeeSchedule schedule, DateTime currentTime)
        {
            var time = currentTime.TimeOfDay;
            var endTime = ParseTimeSpan(schedule.end_time);

            if (time < endTime)
            {
                return "Early Out";
            }
            else
            {
                return "On Time";
            }
        }

        private async Task RecordTimeIn(Employee employee, EmployeeSchedule schedule, DateTime currentTime)
        {
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                var timestamp = currentTime.ToString("yyyy-MM-dd HH:mm:ss");

                // First, check if a record already exists for today
                var existingRecord = await GetTodaysAttendance(employee.employee_id);

                var attendanceData = new
                {
                    employee_id = employee.employee_id,
                    attendance_date = today,
                    time_in = timestamp,
                    time_out = existingRecord?.time_out ?? "",
                    overtime_in = existingRecord?.overtime_in ?? "",
                    overtime_out = existingRecord?.overtime_out ?? "",
                    hours_worked = existingRecord?.hours_worked ?? "0.00",
                    overtime_hours = existingRecord?.overtime_hours ?? "0.00",
                    status = CalculateTimeInStatus(employee, schedule, currentTime),
                    verification_method = "RFID",
                    created_date = existingRecord?.created_date ?? timestamp,
                    schedule_id = schedule.schedule_id
                };

                if (existingRecord != null)
                {
                    // UPDATE existing record using the auto-generated key
                    var json = JsonConvert.SerializeObject(attendanceData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"Attendance/{existingRecord.RecordKey}.json", content);

                    if (response.IsSuccessStatusCode)
                    {
                        AddToScanHistory($"[SUCCESS] Time In UPDATED for {employee.employee_id}");
                    }
                    else
                    {
                        AddToScanHistory($"[ERROR] Firebase returned: {response.StatusCode}");
                    }
                }
                else
                {
                    // CREATE new record (let Firebase auto-generate the key)
                    var json = JsonConvert.SerializeObject(attendanceData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("Attendance.json", content);

                    if (response.IsSuccessStatusCode)
                    {
                        AddToScanHistory($"[SUCCESS] Time In recorded for {employee.employee_id}");
                    }
                    else
                    {
                        AddToScanHistory($"[ERROR] Firebase returned: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Recording Time In: {ex.Message}");
                throw;
            }
        }

        private async Task RecordTimeOut(AttendanceRecord existingRecord, Employee employee, EmployeeSchedule schedule, DateTime currentTime)
        {
            try
            {
                var timestamp = currentTime.ToString("yyyy-MM-dd HH:mm:ss");

                // Calculate hours worked (capped at regular hours)
                double hoursWorked = 0;
                if (!string.IsNullOrEmpty(existingRecord.time_in))
                {
                    if (DateTime.TryParse(existingRecord.time_in, out var timeIn))
                    {
                        hoursWorked = (currentTime - timeIn).TotalHours;

                        // Cap regular hours based on schedule
                        var regularHours = (ParseTimeSpan(schedule.end_time) - ParseTimeSpan(schedule.start_time)).TotalHours;
                        if (hoursWorked > regularHours)
                            hoursWorked = regularHours;

                        // Ensure minimum 1 hour worked if employee leaves early
                        if (hoursWorked < 1.0)
                            hoursWorked = 1.0;
                    }
                }

                var attendanceData = new
                {
                    employee_id = employee.employee_id,
                    attendance_date = existingRecord.attendance_date,
                    time_in = existingRecord.time_in,
                    time_out = timestamp, // UPDATE time_out
                    overtime_in = existingRecord.overtime_in,
                    overtime_out = existingRecord.overtime_out,
                    hours_worked = Math.Round(hoursWorked, 2).ToString("0.00"),
                    overtime_hours = existingRecord.overtime_hours,
                    status = CalculateTimeOutStatus(employee, schedule, currentTime),
                    verification_method = "RFID",
                    created_date = existingRecord.created_date,
                    schedule_id = schedule.schedule_id
                };

                // UPDATE existing record using the auto-generated key
                var json = JsonConvert.SerializeObject(attendanceData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"Attendance/{existingRecord.RecordKey}.json", content);

                if (response.IsSuccessStatusCode)
                {
                    AddToScanHistory($"[SUCCESS] Time Out UPDATED for {employee.employee_id}");
                }
                else
                {
                    AddToScanHistory($"[ERROR] Firebase returned: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Recording Time Out: {ex.Message}");
                throw;
            }
        }

        private async Task RecordOvertimeIn(AttendanceRecord existingRecord, Employee employee, DateTime currentTime)
        {
            try
            {
                var timestamp = currentTime.ToString("yyyy-MM-dd HH:mm:ss");

                var attendanceData = new
                {
                    employee_id = employee.employee_id,
                    attendance_date = existingRecord.attendance_date,
                    time_in = existingRecord.time_in,
                    time_out = existingRecord.time_out,
                    overtime_in = timestamp, // UPDATE overtime_in
                    overtime_out = existingRecord.overtime_out,
                    hours_worked = existingRecord.hours_worked,
                    overtime_hours = existingRecord.overtime_hours,
                    status = "OVERTIME STARTED",
                    verification_method = "RFID",
                    created_date = existingRecord.created_date,
                    schedule_id = existingRecord.schedule_id
                };

                // UPDATE existing record using the auto-generated key
                var json = JsonConvert.SerializeObject(attendanceData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"Attendance/{existingRecord.RecordKey}.json", content);

                if (response.IsSuccessStatusCode)
                {
                    AddToScanHistory($"[SUCCESS] Overtime In UPDATED for {employee.employee_id}");
                }
                else
                {
                    AddToScanHistory($"[ERROR] Firebase returned: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Recording Overtime In: {ex.Message}");
                throw;
            }
        }

        private async Task RecordOvertimeOut(AttendanceRecord existingRecord, Employee employee, DateTime currentTime)
        {
            try
            {
                var timestamp = currentTime.ToString("yyyy-MM-dd HH:mm:ss");

                // Calculate overtime hours (capped at 4 hours)
                double overtimeHours = 0;
                if (!string.IsNullOrEmpty(existingRecord.overtime_in))
                {
                    if (DateTime.TryParse(existingRecord.overtime_in, out var overtimeIn))
                    {
                        overtimeHours = (currentTime - overtimeIn).TotalHours;

                        // Cap overtime at 4 hours maximum
                        if (overtimeHours > 4.0)
                            overtimeHours = 4.0;

                        // Minimum 0.5 hours overtime
                        if (overtimeHours < 0.5)
                            overtimeHours = 0.5;
                    }
                }

                var attendanceData = new
                {
                    employee_id = employee.employee_id,
                    attendance_date = existingRecord.attendance_date,
                    time_in = existingRecord.time_in,
                    time_out = existingRecord.time_out,
                    overtime_in = existingRecord.overtime_in,
                    overtime_out = timestamp, // UPDATE overtime_out
                    hours_worked = existingRecord.hours_worked,
                    overtime_hours = Math.Round(overtimeHours, 2).ToString("0.00"),
                    status = "OVERTIME COMPLETED",
                    verification_method = "RFID",
                    created_date = existingRecord.created_date,
                    schedule_id = existingRecord.schedule_id
                };

                // UPDATE existing record using the auto-generated key
                var json = JsonConvert.SerializeObject(attendanceData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"Attendance/{existingRecord.RecordKey}.json", content);

                if (response.IsSuccessStatusCode)
                {
                    AddToScanHistory($"[SUCCESS] Overtime Out UPDATED for {employee.employee_id}");
                }
                else
                {
                    AddToScanHistory($"[ERROR] Firebase returned: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                AddToScanHistory($"[ERROR] Recording Overtime Out: {ex.Message}");
                throw;
            }
        }

        private void DisplayEmployeeInfo(Employee employee, string action, string status, string overtimeHours)
        {
            OnEmployeeInfoUpdate?.Invoke(
                $"{employee.first_name} {employee.last_name}",
                employee.employee_id,
                employee.department ?? "Not specified",
                $"{action} - {status}",
                DateTime.Now.ToString("HH:mm:ss"),
                overtimeHours
            );
        }

        private void AddToScanHistory(string message)
        {
            OnScanHistoryUpdate?.Invoke(message);
        }

        private void DebugOvertimeTiming(Employee employee, EmployeeSchedule schedule)
        {
            var regularEnd = ParseTimeSpan(schedule.end_time);
            var overtimeStart = regularEnd.Add(TimeSpan.FromHours(1));

            AddToScanHistory($"[DEBUG] Overtime Timing for {employee.employee_id}:");
            AddToScanHistory($"[DEBUG]   Scheduled End: {DateTime.Today.Add(regularEnd):HH:mm}");
            AddToScanHistory($"[DEBUG]   Overtime Start: {DateTime.Today.Add(overtimeStart):HH:mm} (1 hour after scheduled end)");
            AddToScanHistory($"[DEBUG]   Current Time: {DateTime.Now:HH:mm}");

            // Show waiting period info
            if (DateTime.Now.TimeOfDay >= regularEnd && DateTime.Now.TimeOfDay < overtimeStart)
            {
                var timeUntilOvertime = overtimeStart - DateTime.Now.TimeOfDay;
                AddToScanHistory($"[DEBUG]   Waiting Period: {timeUntilOvertime.TotalMinutes:0} minutes until overtime starts");
            }
        }

        public async Task RefreshData()
        {
            await LoadEmployeeData();
            await LoadEmployeeSchedulesData();
            AddToScanHistory($"[{DateTime.Now:HH:mm:ss}] Employee and schedule data refreshed from Firebase");
        }
    }

    public class AttendanceRecord
    {
        public string RecordKey { get; set; }
        public string employee_id { get; set; }
        public string attendance_date { get; set; }
        public string time_in { get; set; }
        public string time_out { get; set; }
        public string overtime_in { get; set; }
        public string overtime_out { get; set; }
        public string hours_worked { get; set; }
        public string overtime_hours { get; set; }
        public string status { get; set; }
        public string verification_method { get; set; }
        public string created_date { get; set; }
        public string schedule_id { get; set; }
    }

    public class EmploymentInfo
    {
        public string employee_id { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public string contract_type { get; set; }
        public string date_of_joining { get; set; }
        public string manager_name { get; set; }
        public string created_at { get; set; }
    }
}