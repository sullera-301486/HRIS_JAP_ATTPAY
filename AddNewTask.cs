using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class AddNewTask : Form
    {
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string currentUserId;
        public event EventHandler TaskAdded;
        public AddNewTask(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            setFont();
            InitializeTextBox();
        }
        private void InitializeTextBox()
        {
            // Set default value to today's date in MM/dd/yyyy format
            textBoxDueDate.Text = DateTime.Today.ToString("MM/dd/yyyy");
        }
        private void TextBoxDueDate_Enter(object sender, EventArgs e)
        {
            if (textBoxDueDate.Text == "MM/DD/YYYY")
            {
                textBoxDueDate.Text = "";
                textBoxDueDate.ForeColor = SystemColors.WindowText;
            }
        }
        private void TextBoxDueDate_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxDueDate.Text))
            {
                textBoxDueDate.Text = "MM/DD/YYYY";
                textBoxDueDate.ForeColor = SystemColors.GrayText;
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
                labelAddNewTask.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelAddTaskDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDueDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelTaskDesc.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxDueDate.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxTaskDesc.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
        private bool IsValidDate(string dateString)
        {
            // Try parsing with MM/dd/yyyy format
            if (DateTime.TryParseExact(dateString, "MM/dd/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return true;
            }

            // Also allow other common formats
            if (DateTime.TryParse(dateString, out result))
            {
                return true;
            }

            return false;
        }
        private string FormatDate(string inputDate)
        {
            if (DateTime.TryParseExact(inputDate, "MM/dd/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date.ToString("MM/dd/yyyy");
            }

            if (DateTime.TryParse(inputDate, out date))
            {
                return date.ToString("MM/dd/yyyy");
            }

            return inputDate; // Return as-is if parsing fails (validation will catch it)
        }
        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxTaskDesc.Text))
            {
                MessageBox.Show("Please enter a task description.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxTaskDesc.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxDueDate.Text) || textBoxDueDate.Text == "MM/DD/YYYY")
            {
                MessageBox.Show("Please enter a due date.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDueDate.Focus();
                return;
            }

            if (!IsValidDate(textBoxDueDate.Text))
            {
                MessageBox.Show("Please enter a valid date format (MM/DD/YYYY).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDueDate.Focus();
                return;
            }

            try
            {
                // Format the date consistently
                string formattedDate = FormatDate(textBoxDueDate.Text);

                // Create task object - CRITICAL: assignedTo = currentUserId
                var newTask = new Dictionary<string, object>
        {
            { "task", textBoxTaskDesc.Text.Trim() },
            { "dueDate", formattedDate },
            { "createdDate", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") },
            { "status", "Pending" },
            { "priority", "Medium" },
            { "assignedTo", currentUserId }, // This ensures individual assignment
            { "createdBy", currentUserId }
        };

                // Add to Firebase
                var result = await firebase
                    .Child("Todos")
                    .PostAsync(newTask);

                if (result != null)
                {
                    MessageBox.Show("Task added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Raise the TaskAdded event
                    TaskAdded?.Invoke(this, EventArgs.Empty);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
