using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace HRIS_JAP_ATTPAY
{
    public partial class LeaveRequest : Form
    {
        // 🔹 Firebase client (use your real database URL)
        private static FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        public LeaveRequest()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();
        }

        private void setFont()
        {
            try
            {
                labelDash.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelLeaveRequest.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelRequestLeaveEntry.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelReason.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                textBoxNameInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveTypeInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxReasonInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonSendRequest.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void buttonSendRequest_Click(object sender, EventArgs e)
        {
            // build request object
            var request = new LeaveNotificationItems.LeaveNotificationModel
            {
                Title = $"Leave Request - {comboBoxLeaveTypeInput.Text?.Trim()}",
                SubmittedBy = textBoxNameInput.Text?.Trim(),
                Employee = textBoxNameInput.Text?.Trim(),
                LeaveType = comboBoxLeaveTypeInput.Text?.Trim(),
                Period = $"{textBoxStartPeriod.Text?.Trim()} - {textBoxEndPeriod.Text?.Trim()}",
                Notes = textBoxReasonInput.Text?.Trim(),
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // confirm dialog for preview
            var preview = new LeaveRequestData
            {
                Title = request.Title,
                SubmittedBy = request.SubmittedBy,
                EmployeeName = request.Employee,
                LeaveType = request.LeaveType,
                Start = textBoxStartPeriod.Text?.Trim(),
                End = textBoxEndPeriod.Text?.Trim(),
                Notes = request.Notes,
                Photo = null,
                CreatedAt = DateTime.Now
            };

            using (var confirm = new LeaveRequestConfirm(preview))
            {
                var result = confirm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // 🔹 Save directly to Firebase
                    await FirebaseSave(request);

                    MessageBox.Show("Leave request submitted successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Close();
                }
            }
        }

        private async Task FirebaseSave(LeaveNotificationItems.LeaveNotificationModel notif)
        {
            await firebase.Child("LeaveNotifications").PostAsync(notif);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxStartPeriod, "mm/dd/yyyy");
            AttributesClass.TextboxPlaceholder(textBoxEndPeriod, "mm/dd/yyyy");
        }
    }
}
