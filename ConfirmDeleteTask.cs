using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HRIS_JAP_ATTPAY.AdminOverview;

namespace HRIS_JAP_ATTPAY
{
    public partial class ConfirmDeleteTask : Form
    {
        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private string taskKey;
        private string taskText;
        private Action onTaskDeleted;
        public ConfirmDeleteTask(string taskKey, string taskText, Action onTaskDeletedCallback = null)
        {
            InitializeComponent();
            this.taskKey = taskKey;
            this.taskText = taskText;
            this.onTaskDeleted = onTaskDeletedCallback;
            setFont();
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
                labelMessage.Font = AttributesClass.GetFont("Roboto-Light", 11f);
                labelDeleteTask.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonConfirm.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Show loading state
                buttonConfirm.Enabled = false;
                buttonConfirm.Text = "Deleting...";

                // Delete the task from Firebase
                await firebase.Child("Todos").Child(taskKey).DeleteAsync();

                // Invoke the callback if provided
                onTaskDeleted?.Invoke();

                MessageBox.Show("Task deleted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting task: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Reset button state on error
                buttonConfirm.Enabled = true;
                buttonConfirm.Text = "Confirm";
            }
        }
    }
}