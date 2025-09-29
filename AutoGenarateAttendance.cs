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

        public async Task GenerateWeeklyAttendance(DateTime startDate)
        {
            try
            {
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
                    var attendanceRecord = CreateInitialAbsentRecord(employee, date);
                    attendanceRecords.Add(attendanceRecord);
                }

                // Save to Firebase
                await SaveAttendanceRecords(attendanceRecords);

                Console.WriteLine($"Initial absent attendance for {date:yyyy-MM-dd} generated successfully for {employees.Count} employees!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating daily attendance: {ex.Message}");
                throw;
            }
        }

        private object CreateInitialAbsentRecord(Employee employee, DateTime date)
        {
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
                is_pending = true  // Flag to indicate this record needs RFID update
            };
        }

        // Method to update attendance when RFID is tapped
        public async Task UpdateAttendanceFromRFID(string employeeId, DateTime tapTime, bool isTimeIn)
        {
            try
            {
                string today = DateTime.Today.ToString("yyyy-MM-dd");

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

                    if (recordEmployeeId == employeeId && recordDate == today && isPending)
                    {
                        if (isTimeIn)
                        {
                            // Update time in and change status from Absent to Pending (will be calculated later)
                            record["time_in"] = tapTime.ToString("HH:mm:ss");
                            record["status"] = "Pending Calculation";
                            record["verification_method"] = "RFID";
                        }
                        else
                        {
                            // Update time out
                            record["time_out"] = tapTime.ToString("HH:mm:ss");
                            record["verification_method"] = "RFID";

                            // If both time in and time out are set, calculate hours and status
                            string timeInStr = record["time_in"]?.ToString();
                            if (!string.IsNullOrEmpty(timeInStr) && timeInStr != "N/A")
                            {
                                CalculateAndUpdateAttendance(record, tapTime);
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

        private void CalculateAndUpdateAttendance(JToken record, DateTime timeOut)
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

                    // Calculate overtime (anything over 8 hours)
                    double overtimeHours = Math.Max(0, totalHours - 8.0);

                    // Calculate status
                    string status = CalculateStatus(timeIn, timeOut);

                    // Update record
                    record["hours_worked"] = totalHours.ToString("0.00");
                    record["overtime_hours"] = overtimeHours.ToString("0.00");
                    record["status"] = status;
                    record["time_out"] = timeOut.ToString("HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating attendance: {ex.Message}");
            }
        }

        private string CalculateStatus(DateTime timeIn, DateTime timeOut)
        {
            DateTime expectedStart = timeIn.Date.AddHours(8); // 8:00 AM expected start
            DateTime expectedEnd = timeIn.Date.AddHours(17); // 5:00 PM expected end
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
                        FirstName = emp.Object.first_name ?? "",
                        MiddleName = emp.Object.middle_name ?? "",
                        LastName = emp.Object.last_name ?? "",
                        EmploymentStatus = "Active",
                        RFIDTag = emp.Object.rfid_tag?.ToString() ?? ""
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

                Console.WriteLine($"Saved {records.Count} initial absent attendance records to Firebase.");
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
            var generator = new AttendanceGenerator();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                bool exists = await generator.CheckAttendanceExists(date);
                if (!exists)
                {
                    missingDates.Add(date);
                }
            }

            return missingDates;
        }
    }

    public class Employee
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmploymentStatus { get; set; }
        public string RFIDTag { get; set; }
    }
}