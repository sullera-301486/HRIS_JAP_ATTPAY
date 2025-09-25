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

        public EditAttendance()
        {
            InitializeComponent();
            setFont();
        }

        public void SetAttendanceData(string employeeId, string fullName, string timeIn, string timeOut,
                                    string hoursWorked, string status, string overtimeHours, string verificationMethod,
                                    string firebaseKey)
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

            // Set the editable fields
            textBoxTimeIn.Text = timeIn ?? "";
            textBoxTimeOut.Text = timeOut ?? "";
            textBoxOverTimeIn.Text = "";
            textBoxOverTimeOut.Text = "";
            statusColorCheck();
            // Set the date (you'll need to get this from the main form or Firebase)
            // Try to extract date from the original time in or use current date
            if (!string.IsNullOrEmpty(timeIn) && timeIn != "N/A")
            {
                try
                {
                    if (DateTime.TryParse(timeIn, out DateTime timeInDate))
                    {
                        labelDateInput.Text = timeInDate.ToString("yyyy-MM-dd");
                        currentDate = labelDateInput.Text;
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

            Console.WriteLine($"EditAttendance: Firebase Key = {currentFirebaseKey}, Employee ID = {currentEmployeeId}, Date = {currentDate}");
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(textBoxTimeIn.Text) || string.IsNullOrEmpty(textBoxTimeOut.Text))
            {
                MessageBox.Show("Please enter both Time In and Time Out values.", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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
                string newHoursWorked = CalculateHoursWorked();
                string newOvertime = CalculateOvertime();
                string newStatus = CalculateStatus();

                // Create updated record
                var updatedRecord = new
                {
                    employee_id = currentEmployeeId,
                    attendance_date = currentDate,
                    time_in = newTimeIn,
                    time_out = newTimeOut,
                    hours_worked = newHoursWorked,
                    status = newStatus,
                    overtime_hours = newOvertime,
                    verification_method = "Manual Edit",
                    schedule_id = "" // You may need to preserve the original schedule_id
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
            if (string.IsNullOrEmpty(timeText) || timeText == "N/A" || timeText == "")
                return "N/A";

            try
            {
                // Try to parse the time input
                if (DateTime.TryParse(timeText, out DateTime time))
                {
                    // Combine with the current attendance date
                    if (DateTime.TryParse(currentDate, out DateTime date))
                    {
                        DateTime combinedDateTime = new DateTime(date.Year, date.Month, date.Day,
                                                                time.Hour, time.Minute, time.Second);
                        return combinedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        // If date parsing fails, use today's date
                        DateTime today = DateTime.Today;
                        DateTime combinedDateTime = new DateTime(today.Year, today.Month, today.Day,
                                                                time.Hour, time.Minute, time.Second);
                        return combinedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                return timeText;
            }
            catch
            {
                return timeText;
            }
        }

        private string CalculateHoursWorked()
        {
            try
            {
                if (DateTime.TryParse(textBoxTimeIn.Text, out DateTime timeIn) &&
                    DateTime.TryParse(textBoxTimeOut.Text, out DateTime timeOut))
                {
                    if (timeOut > timeIn)
                    {
                        TimeSpan hoursWorked = timeOut - timeIn;
                        return Math.Round(hoursWorked.TotalHours, 2).ToString("0.00");
                    }
                    else
                    {
                        // Time out is before time in (overnight shift)
                        TimeSpan hoursWorked = (timeOut.AddDays(1) - timeIn);
                        return Math.Round(hoursWorked.TotalHours, 2).ToString("0.00");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating hours worked: {ex.Message}");
            }
            return "0.00";
        }

        private string CalculateOvertime()
        {
            try
            {
                if (double.TryParse(CalculateHoursWorked(), out double hoursWorked))
                {
                    double regularHours = 8.0;
                    double overtime = Math.Max(0, hoursWorked - regularHours);
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
                if (DateTime.TryParse(textBoxTimeIn.Text, out DateTime timeIn) &&
                    DateTime.TryParse(textBoxTimeOut.Text, out DateTime timeOut))
                {
                    // Basic status calculation - adjust based on your business rules
                    DateTime expectedStart = DateTime.Today.AddHours(9); // 9:00 AM
                    DateTime expectedEnd = DateTime.Today.AddHours(17); // 5:00 PM

                    bool isLate = timeIn.TimeOfDay > expectedStart.TimeOfDay.Add(TimeSpan.FromMinutes(15));
                    bool isEarlyOut = timeOut.TimeOfDay < expectedEnd.TimeOfDay.Subtract(TimeSpan.FromMinutes(15));

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
            UpdateCalculations();
        }

        private void textBoxTimeOut_TextChanged(object sender, EventArgs e)
        {
            UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            labelHoursWorkedInput.Text = CalculateHoursWorked();
            labelOvertimeInput.Text = CalculateOvertime();
            labelStatusInput.Text = CalculateStatus();
            statusColorCheck();
        }

        private void textBoxOverTimeIn_TextChanged(object sender, EventArgs e)
        {
            // You can add overtime-specific calculations here if needed
        }

        private void textBoxOverTimeOut_TextChanged(object sender, EventArgs e)
        {
            // You can add overtime-specific calculations here if needed
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

    }
}