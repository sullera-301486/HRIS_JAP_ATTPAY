using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class EditAttendance : Form
    {
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public bool DataUpdated { get; private set; } = false;
        private string currentEmployeeId;
        private string currentDate;
        private string currentFirebaseKey;
        private string originalTimeIn;
        private string originalTimeOut;
        private ErrorProvider errorProvider1;

        public EditAttendance()
        {
            InitializeComponent();
            setFont();
            errorProvider1 = new ErrorProvider();
            errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        }

        public void SetAttendanceData(string employeeId, string fullName, string timeIn, string timeOut,
                             string hoursWorked, string status, string overtimeHours, string verificationMethod,
                             string firebaseKey, string attendanceDate = null)
        {
            currentEmployeeId = employeeId;
            currentFirebaseKey = firebaseKey;
            originalTimeIn = timeIn;
            originalTimeOut = timeOut;

            // Set the labels
            labelIDInput.Text = employeeId ?? "N/A";
            labelNameInput.Text = fullName ?? "N/A";
            labelHoursWorkedInput.Text = hoursWorked ?? "0.00";
            labelOvertimeInput.Text = overtimeHours ?? "0.00";
            labelStatusInput.Text = status ?? "Absent";

            // Set the editable fields - extract only time portion
            textBoxTimeIn.Text = ExtractTimeOnly(timeIn) ?? "";
            textBoxTimeOut.Text = ExtractTimeOnly(timeOut) ?? "";

            // You'll need to load overtime_in and overtime_out from Firebase if they exist
            textBoxOverTimeIn.Text = ""; // You'll need to load actual values here
            textBoxOverTimeOut.Text = ""; // You'll need to load actual values here

            statusColorCheck();

            // Set the date - priority: provided attendanceDate > extract from timeIn > current date
            if (!string.IsNullOrEmpty(attendanceDate))
            {
                // Use the explicitly provided date
                labelDateInput.Text = attendanceDate;
                currentDate = attendanceDate;
            }
            else if (!string.IsNullOrEmpty(timeIn) && timeIn != "N/A")
            {
                try
                {
                    // Try to extract date from the original timeIn (which may contain full datetime)
                    string extractedDate = ExtractDateFromDateTime(timeIn);
                    if (!string.IsNullOrEmpty(extractedDate))
                    {
                        labelDateInput.Text = extractedDate;
                        currentDate = extractedDate;
                    }
                    else
                    {
                        labelDateInput.Text = DateTime.Now.ToString("yyyy-MM-dd");
                        currentDate = labelDateInput.Text;
                    }
                }
                catch
                {
                    labelDateInput.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    currentDate = labelDateInput.Text;
                }
            }
            else
            {
                labelDateInput.Text = DateTime.Now.ToString("yyyy-MM-dd");
                currentDate = labelDateInput.Text;
            }

            // Update overtime fields state after setting all values
            UpdateOvertimeFieldsState();

            Console.WriteLine($"EditAttendance: Firebase Key = {currentFirebaseKey}, Employee ID = {currentEmployeeId}, Date = {currentDate}");
            Console.WriteLine($"Original TimeIn: {timeIn}, Extracted Time: {textBoxTimeIn.Text}");
        }

        private string ExtractTimeOnly(string datetimeString)
        {
            if (string.IsNullOrEmpty(datetimeString) || datetimeString.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return "N/A";

            try
            {
                if (DateTime.TryParse(datetimeString, out DateTime dt))
                {
                    // Return time in 12-hour format with AM/PM
                    return dt.ToString("hh:mm tt").ToLower();
                }
                else
                {
                    // If it's already just a time string, return as is
                    return datetimeString;
                }
            }
            catch
            {
                return datetimeString;
            }
        }

        private string ExtractDateFromDateTime(string datetimeString)
        {
            if (string.IsNullOrEmpty(datetimeString) || datetimeString.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                // First, check if it's a full datetime string with date
                if (datetimeString.Contains("-") && datetimeString.Length >= 10)
                {
                    string datePart = datetimeString.Substring(0, 10);
                    if (DateTime.TryParse(datePart, out DateTime date))
                    {
                        return date.ToString("yyyy-MM-dd");
                    }
                }

                // Try parsing as full datetime
                if (DateTime.TryParse(datetimeString, out DateTime dt))
                {
                    return dt.ToString("yyyy-MM-dd");
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return null;
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            // Validate inputs - check if both Time In and Time Out have values (not empty and not both N/A)
            string timeInText = textBoxTimeIn.Text?.Trim();
            string timeOutText = textBoxTimeOut.Text?.Trim();

            bool timeInEmpty = string.IsNullOrEmpty(timeInText);
            bool timeOutEmpty = string.IsNullOrEmpty(timeOutText);
            bool timeInNA = timeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase);
            bool timeOutNA = timeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase);

            // Both fields must either have valid time values or both be N/A
            if ((timeInEmpty && !timeOutEmpty) || (!timeInEmpty && timeOutEmpty))
            {
                MessageBox.Show("Please enter both Time In and Time Out values in HH:MM format or N/A.", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // If both have values, validate the format
            if (!timeInEmpty && !timeOutEmpty && !timeInNA && !timeOutNA)
            {
                if (!IsValidTimeInput(timeInText) || !IsValidTimeInput(timeOutText))
                {
                    MessageBox.Show("Please enter both Time In and Time Out values in HH:MM format or N/A.", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Additional validation: Check if overtime is entered when regular time is N/A
            if (IsRegularTimeNA())
            {
                if (!string.IsNullOrEmpty(textBoxOverTimeIn.Text) || !string.IsNullOrEmpty(textBoxOverTimeOut.Text))
                {
                    MessageBox.Show("Overtime is not allowed when Time In or Time Out is N/A.", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validate overtime fields - both should be N/A or both should be time values
            bool overtimeInNA = textBoxOverTimeIn.Text?.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase) ?? true;
            bool overtimeOutNA = textBoxOverTimeOut.Text?.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase) ?? true;

            if (overtimeInNA != overtimeOutNA)
            {
                MessageBox.Show("Overtime fields must both be N/A or both contain time values.", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate overtime fields if they're enabled
            if (!IsRegularTimeNA())
            {
                if (!IsValidTimeInput(textBoxOverTimeIn.Text) || !IsValidTimeInput(textBoxOverTimeOut.Text))
                {
                    MessageBox.Show("Please enter valid overtime values in HH:MM format or leave them empty.", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Show confirmation dialog
            Form parentForm = this.FindForm();
            UpdateConfirmationEditAttendance confirmForm = new UpdateConfirmationEditAttendance();

            // Set up event handler for when the confirmation form closes
            confirmForm.FormClosed += (s, args) => {
                if (confirmForm.UserConfirmed)
                {
                    // Run the update process
                    UpdateAttendanceInFirebase();
                }
            };

            AttributesClass.ShowWithOverlay(parentForm, confirmForm);
        }

        private async void UpdateAttendanceInFirebase()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                Console.WriteLine($"Updating attendance for key: {currentFirebaseKey}");

                if (string.IsNullOrEmpty(currentFirebaseKey))
                {
                    MessageBox.Show("Error: Cannot identify the attendance record to update.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Calculate new values
                string newTimeIn = FormatTimeForFirebase(textBoxTimeIn.Text);
                string newTimeOut = FormatTimeForFirebase(textBoxTimeOut.Text);
                string newOvertimeIn = FormatTimeForFirebase(textBoxOverTimeIn.Text);
                string newOvertimeOut = FormatTimeForFirebase(textBoxOverTimeOut.Text);
                string newHoursWorked = CalculateHoursWorked();
                string newOvertime = CalculateOvertime();
                string newStatus = CalculateStatus();

                // Create updated record - add overtime_in and overtime_out fields
                var updatedRecord = new
                {
                    employee_id = currentEmployeeId,
                    attendance_date = currentDate,
                    time_in = newTimeIn,
                    time_out = newTimeOut,
                    overtime_in = newOvertimeIn,  // Add this
                    overtime_out = newOvertimeOut, // Add this
                    hours_worked = newHoursWorked,
                    status = newStatus,
                    overtime_hours = newOvertime,
                    verification_method = "Manual Edit",
                    schedule_id = ""
                };

                // Update in Firebase using the key
                await firebase.Child("Attendance").Child(currentFirebaseKey).PutAsync(updatedRecord);

                DataUpdated = true;
                MessageBox.Show("Attendance updated successfully!", "Success",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating attendance: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private string FormatTimeForFirebase(string timeText)
        {
            if (string.IsNullOrEmpty(timeText) || timeText.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return "N/A";

            try
            {
                DateTime time;

                // Try to parse different time formats
                if (DateTime.TryParseExact(timeText, "HH:mm", null, System.Globalization.DateTimeStyles.None, out time) ||
                    DateTime.TryParseExact(timeText, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out time) ||
                    DateTime.TryParse(timeText, out time))
                {
                    if (DateTime.TryParse(currentDate, out DateTime date))
                    {
                        DateTime combinedDateTime = new DateTime(date.Year, date.Month, date.Day,
                                                                time.Hour, time.Minute, 0);
                        return combinedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        // If date parsing fails, use today's date
                        DateTime today = DateTime.Today;
                        DateTime combinedDateTime = new DateTime(today.Year, today.Month, today.Day,
                                                                time.Hour, time.Minute, 0);
                        return combinedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private string CalculateHoursWorked()
        {
            try
            {
                string timeInText = textBoxTimeIn.Text?.Trim() ?? "";
                string timeOutText = textBoxTimeOut.Text?.Trim() ?? "";
                string overtimeInText = textBoxOverTimeIn.Text?.Trim() ?? "";
                string overtimeOutText = textBoxOverTimeOut.Text?.Trim() ?? "";

                // Handle N/A values for regular time
                if (timeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                    timeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    // If regular time is N/A, check if overtime is provided and valid
                    bool overtimeInIsNA = overtimeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase);
                    bool overtimeOutIsNA = overtimeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase);

                    if (!overtimeInIsNA && !overtimeOutIsNA &&
                        !string.IsNullOrEmpty(overtimeInText) && !string.IsNullOrEmpty(overtimeOutText))
                    {
                        // Calculate hours based on overtime only
                        return CalculateTimeDifference(overtimeInText, overtimeOutText);
                    }
                    return "0.00";
                }

                // Calculate regular hours
                string regularHours = CalculateTimeDifference(timeInText, timeOutText);

                // Calculate overtime hours if overtime is provided and valid
                bool overtimeInNA = overtimeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase);
                bool overtimeOutNA = overtimeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase);

                if (!overtimeInNA && !overtimeOutNA &&
                    !string.IsNullOrEmpty(overtimeInText) && !string.IsNullOrEmpty(overtimeOutText))
                {
                    string overtimeHours = CalculateTimeDifference(overtimeInText, overtimeOutText);

                    if (double.TryParse(regularHours, out double regHours) &&
                        double.TryParse(overtimeHours, out double otHours))
                    {
                        return Math.Round(regHours + otHours, 2).ToString("0.00");
                    }
                }

                return regularHours;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating hours worked: {ex.Message}");
            }
            return "0.00";
        }

        private string CalculateTimeDifference(string startTime, string endTime)
        {
            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime) ||
                startTime.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                endTime.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return "0.00";

            DateTime timeStart, timeEnd;

            // Try to parse different time formats
            bool startParsed = DateTime.TryParseExact(startTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeStart) ||
                              DateTime.TryParseExact(startTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeStart) ||
                              DateTime.TryParse(startTime, out timeStart);

            bool endParsed = DateTime.TryParseExact(endTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeEnd) ||
                            DateTime.TryParseExact(endTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeEnd) ||
                            DateTime.TryParse(endTime, out timeEnd);

            if (startParsed && endParsed)
            {
                // Create full datetime objects with the current date
                DateTime fullTimeStart = DateTime.Today.AddHours(timeStart.Hour).AddMinutes(timeStart.Minute);
                DateTime fullTimeEnd = DateTime.Today.AddHours(timeEnd.Hour).AddMinutes(timeEnd.Minute);

                if (fullTimeEnd > fullTimeStart)
                {
                    TimeSpan timeDifference = fullTimeEnd - fullTimeStart;
                    return Math.Round(timeDifference.TotalHours, 2).ToString("0.00");
                }
                else
                {
                    // Time out is before time in (overnight shift)
                    TimeSpan timeDifference = (fullTimeEnd.AddDays(1) - fullTimeStart);
                    return Math.Round(timeDifference.TotalHours, 2).ToString("0.00");
                }
            }
            return "0.00";
        }

        private string CalculateOvertime()
        {
            try
            {
                // If regular time is N/A, no overtime is allowed
                if (IsRegularTimeNA())
                {
                    return "0.00";
                }

                string overtimeInText = textBoxOverTimeIn.Text?.Trim() ?? "";
                string overtimeOutText = textBoxOverTimeOut.Text?.Trim() ?? "";

                // Check if either overtime field contains "N/A"
                bool overtimeInNA = overtimeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase);
                bool overtimeOutNA = overtimeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase);

                // If both overtime fields are N/A, return 0.00
                if (overtimeInNA && overtimeOutNA)
                {
                    return "0.00";
                }

                // If one overtime field is N/A but the other isn't, this is invalid - return 0.00
                if (overtimeInNA != overtimeOutNA)
                {
                    return "0.00";
                }

                // If overtime fields are filled and not N/A, calculate overtime hours directly
                if (!string.IsNullOrEmpty(overtimeInText) && !overtimeInNA &&
                    !string.IsNullOrEmpty(overtimeOutText) && !overtimeOutNA)
                {
                    return CalculateTimeDifference(overtimeInText, overtimeOutText);
                }

                // If overtime fields are empty, fallback to regular calculation
                if (double.TryParse(CalculateHoursWorked(), out double totalHours))
                {
                    double regularHours = 8.0;
                    double overtime = Math.Max(0, totalHours - regularHours);
                    return Math.Round(overtime, 2).ToString("0.00");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating overtime: {ex.Message}");
            }
            return "0.00";
        }

        private string CalculateStatus()
        {
            try
            {
                string timeInText = textBoxTimeIn.Text;
                string timeOutText = textBoxTimeOut.Text;

                // Handle N/A values
                if (timeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                    timeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    return "Absent";
                }

                DateTime timeIn, timeOut;

                // Try to parse different time formats
                bool timeInParsed = DateTime.TryParseExact(timeInText, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParseExact(timeInText, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeIn) ||
                                   DateTime.TryParse(timeInText, out timeIn);

                bool timeOutParsed = DateTime.TryParseExact(timeOutText, "HH:mm", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParseExact(timeOutText, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out timeOut) ||
                                    DateTime.TryParse(timeOutText, out timeOut);

                if (timeInParsed && timeOutParsed)
                {
                    // Basic status calculation - adjust based on your business rules
                    DateTime expectedStart = DateTime.Today.AddHours(8); // 8:00 AM
                    DateTime expectedEnd = DateTime.Today.AddHours(17); // 5:00 PM

                    DateTime actualTimeIn = DateTime.Today.AddHours(timeIn.Hour).AddMinutes(timeIn.Minute);
                    DateTime actualTimeOut = DateTime.Today.AddHours(timeOut.Hour).AddMinutes(timeOut.Minute);

                    bool isLate = actualTimeIn > expectedStart; // No grace period - late if after 8:00 AM
                    bool isEarlyOut = actualTimeOut < expectedEnd; // Early out if before 5:00 PM

                    if (isLate && isEarlyOut)
                        return "Late & Early Out";
                    else if (isLate)
                        return "Late";
                    else if (isEarlyOut)
                        return "Early Out";
                    else
                        return "On Time";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating status: {ex.Message}");
            }
            return "On Time";
        }

        // Add this method to check if regular time is N/A
        private bool IsRegularTimeNA()
        {
            string timeInText = textBoxTimeIn.Text?.Trim();
            string timeOutText = textBoxTimeOut.Text?.Trim();

            return string.IsNullOrEmpty(timeInText) ||
                   timeInText.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                   string.IsNullOrEmpty(timeOutText) ||
                   timeOutText.Equals("N/A", StringComparison.OrdinalIgnoreCase);
        }

        // Add this method to enable/disable overtime fields
        private void UpdateOvertimeFieldsState()
        {
            bool isRegularTimeNA = IsRegularTimeNA();

            // Disable overtime fields when regular time is N/A
            textBoxOverTimeIn.Enabled = !isRegularTimeNA;
            textBoxOverTimeOut.Enabled = !isRegularTimeNA;

            // Clear overtime fields when disabled
            if (isRegularTimeNA)
            {
                textBoxOverTimeIn.Text = "";
                textBoxOverTimeOut.Text = "";

                // Change background color and text color to indicate disabled state
                textBoxOverTimeIn.BackColor = Color.LightGray;
                textBoxOverTimeOut.BackColor = Color.LightGray;
                textBoxOverTimeIn.ForeColor = Color.DarkGray;
                textBoxOverTimeOut.ForeColor = Color.DarkGray;
            }
            else
            {
                // Reset background color and text color when enabled
                textBoxOverTimeIn.BackColor = SystemColors.Window;
                textBoxOverTimeOut.BackColor = SystemColors.Window;
                textBoxOverTimeIn.ForeColor = SystemColors.WindowText;
                textBoxOverTimeOut.ForeColor = SystemColors.WindowText;
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setFont()
        {
            try
            {
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDateInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelHoursWorked.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelHoursWorkedInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelID.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelIDInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelManualAttendanceRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNameInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOvertime.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOverTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelOvertimeInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelOverTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelRequestAttendanceEntry.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelStatus.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelStatusInput.Font = AttributesClass.GetFont("Roboto-Light", 10f);
                labelTimeIn.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTimeOut.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxOverTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxOverTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTimeIn.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTimeOut.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void textBoxTimeIn_TextChanged(object sender, EventArgs e)
        {
            ValidateTimeFormat(textBoxTimeIn);
            UpdateOvertimeFieldsState(); // Add this line
            UpdateCalculations();
        }

        private void textBoxTimeOut_TextChanged(object sender, EventArgs e)
        {
            ValidateTimeFormat(textBoxTimeOut);
            UpdateOvertimeFieldsState(); // Add this line
            UpdateCalculations();
        }

        private void textBoxOverTimeIn_TextChanged(object sender, EventArgs e)
        {
            // Only validate and update if overtime is allowed
            if (!IsRegularTimeNA())
            {
                ValidateTimeFormat(textBoxOverTimeIn);
                UpdateCalculations();
            }
        }

        private void textBoxOverTimeOut_TextChanged(object sender, EventArgs e)
        {
            // Only validate and update if overtime is allowed
            if (!IsRegularTimeNA())
            {
                ValidateTimeFormat(textBoxOverTimeOut);
                UpdateCalculations();
            }
        }

        private void UpdateCalculations()
        {
            labelHoursWorkedInput.Text = CalculateHoursWorked();
            labelOvertimeInput.Text = CalculateOvertime();
            labelStatusInput.Text = CalculateStatus();
            statusColorCheck();
        }

        private void statusColorCheck()
        {
            switch (labelStatusInput.Text)
            {
                case "On Time":
                    labelStatusInput.BackColor = Color.FromArgb(95, 218, 71);
                    labelStatusInput.ForeColor = Color.White;
                    break;
                case "Late":
                case "Early Out":
                case "Late & Early Out":
                    labelStatusInput.BackColor = Color.FromArgb(255, 163, 74);
                    labelStatusInput.ForeColor = Color.White;
                    break;
                case "Absent":
                    labelStatusInput.BackColor = Color.FromArgb(221, 60, 60);
                    labelStatusInput.ForeColor = Color.White;
                    break;
                case "Leave":
                    labelStatusInput.BackColor = Color.FromArgb(71, 93, 218);
                    labelStatusInput.ForeColor = Color.White;
                    break;
                case "Day Off":
                    labelStatusInput.BackColor = Color.FromArgb(180, 174, 189);
                    labelStatusInput.ForeColor = Color.White;
                    break;
            }
        }

        // Validation Methods
        private void textBoxTimeIn_KeyPress(object sender, KeyPressEventArgs e)
        {
            ValidateTimeInput(textBoxTimeIn, e);
        }

        private void textBoxTimeOut_KeyPress(object sender, KeyPressEventArgs e)
        {
            ValidateTimeInput(textBoxTimeOut, e);
        }

        private void textBoxOverTimeIn_KeyPress(object sender, KeyPressEventArgs e)
        {
            ValidateTimeInput(textBoxOverTimeIn, e);
        }

        private void textBoxOverTimeOut_KeyPress(object sender, KeyPressEventArgs e)
        {
            ValidateTimeInput(textBoxOverTimeOut, e);
        }

        private void ValidateTimeInput(TextBox textBox, KeyPressEventArgs e)
        {
            // Allow control characters (backspace, delete, etc.)
            if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            // Allow 'N', 'A', '/' for typing "N/A"
            if (e.KeyChar == 'N' || e.KeyChar == 'n' || e.KeyChar == 'A' || e.KeyChar == 'a' || e.KeyChar == '/')
            {
                e.Handled = false;
                return;
            }

            // Allow only digits and colon
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ':')
            {
                e.Handled = true;
                return;
            }

            string currentText = textBox.Text;
            int selectionStart = textBox.SelectionStart;

            string proposedText = currentText.Substring(0, selectionStart) +
                                 e.KeyChar +
                                 currentText.Substring(selectionStart + textBox.SelectionLength);

            // Validate HH:MM format as user types
            if (!IsValidTimeFormat(proposedText) && !proposedText.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                e.Handled = true;
            }
        }

        private void ValidateTimeFormat(TextBox textBox)
        {
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
                return;

            // Allow "N/A" (case insensitive)
            if (text.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                textBox.Text = "N/A";
                textBox.SelectAll();
                return;
            }

            // Auto-format as HH:MM
            if (text.Length >= 2 && !text.Contains(":"))
            {
                string formatted = text.Insert(2, ":");
                if (IsValidTimeFormat(formatted))
                {
                    textBox.Text = formatted;
                    textBox.SelectionStart = formatted.Length;
                }
            }

            // Validate time format
            if (!IsValidTimeFormat(text))
            {
                errorProvider1.SetError(textBox, "Please enter time in HH:MM format or N/A");
            }
            else
            {
                errorProvider1.SetError(textBox, "");
            }
        }

        private bool IsValidTimeFormat(string timeText)
        {
            if (string.IsNullOrEmpty(timeText) || timeText.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return true;

            // Try to parse various time formats
            DateTime dummy;
            return DateTime.TryParseExact(timeText, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dummy) ||
                   DateTime.TryParseExact(timeText, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out dummy) ||
                   DateTime.TryParseExact(timeText, "h:mm tt", null, System.Globalization.DateTimeStyles.None, out dummy) ||
                   DateTime.TryParse(timeText, out dummy);
        }

        private bool IsValidTimeInput(string timeText)
        {
            return string.IsNullOrEmpty(timeText) ||
                   timeText.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                   IsValidTimeFormat(timeText);
        }
    }
}