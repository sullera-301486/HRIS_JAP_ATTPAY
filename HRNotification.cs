using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace HRIS_JAP_ATTPAY
{
    public partial class HRNotification : Form
    {
        private FirebaseClient firebase = new FirebaseClient(
            "https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/"
        );

        // --- prevent duplicates and allow updates/removes ---
        private readonly HashSet<string> displayedKeys = new HashSet<string>();
        private readonly Dictionary<string, SummaryNotificationItem> itemMap = new Dictionary<string, SummaryNotificationItem>();
        private IDisposable hrSubscription;

        public HRNotification()
        {
            InitializeComponent();
            SetFont();
        }

        private async void HRNotification_Load(object sender, EventArgs e)
        {
            flowSummary.Controls.Clear();

            // Ensure the form handle is created BEFORE we might try to marshal to it.
            // This doesn't guarantee callbacks won't fire before the handle exists,
            // so we still use SafeInvoke in the subscription below.
            var _ = this.Handle;

            // --- Load existing HR notifications once (initial snapshot) ---
            var hrData = await firebase.Child("HRNotifications").OnceAsync<JObject>();
            foreach (var item in hrData)
            {
                var data = item.Object;
                string key = item.Key;
                if (data == null) continue;

                string message = data["message"]?.ToString() ?? "Notification";
                string status = data["status"]?.ToString() ?? "Pending";

                AddOrUpdateNotificationToFlow(key, message, status);
            }

            // --- Subscribe to realtime updates ---
            hrSubscription = firebase.Child("HRNotifications")
                .AsObservable<JObject>()
                .Subscribe(d =>
                {
                    string key = d.Key;

                    if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                    {
                        // remove notification on delete
                        SafeInvoke(() => RemoveNotificationFromFlow(key));
                        return;
                    }

                    if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate && d.Object != null)
                    {
                        string msg = d.Object["message"]?.ToString() ?? "Notification";
                        string stat = d.Object["status"]?.ToString() ?? "Pending";

                        SafeInvoke(() => AddOrUpdateNotificationToFlow(key, msg, stat));
                    }
                });
        }

        // Ensure we clean up subscription to avoid callbacks after dispose
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { hrSubscription?.Dispose(); } catch { }
            base.OnFormClosing(e);
        }

        // --- Safe invoker: won't call Invoke if handle isn't ready; queues for HandleCreated otherwise ---
        private void SafeInvoke(Action action)
        {
            if (action == null) return;

            // If handle is ready and control not disposed -> use BeginInvoke if required
            if (this.IsHandleCreated && !this.IsDisposed && !this.Disposing)
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(action);
                else
                    action();
                return;
            }

            // If handle not created yet, attach a one-time handler that will execute action when created
            void Handler(object s, EventArgs e)
            {
                this.HandleCreated -= Handler;
                if (!this.IsDisposed && !this.Disposing)
                {
                    try { this.BeginInvoke(action); } catch { /* ignore if still invalid */ }
                }
            }

            this.HandleCreated += Handler;
        }

        // --- Add new or update existing notification in the flow panel ---
        private void AddOrUpdateNotificationToFlow(string key, string message, string status)
        {
            if (string.IsNullOrEmpty(key))
                key = Guid.NewGuid().ToString();

            // update existing
            if (displayedKeys.Contains(key))
            {
                if (itemMap.TryGetValue(key, out SummaryNotificationItem existing))
                {
                    Image icon = IconForStatus(status);
                    existing.SetData(message, icon);

                    // Move updated notification to the top visually
                    flowSummary.Controls.SetChildIndex(existing, 0);
                }
                return;
            }

            // create new UI item
            var notif = new SummaryNotificationItem();
            notif.Tag = key;
            Image iconNew = IconForStatus(status);
            notif.SetData(message, iconNew);

            // Add to top instead of bottom
            flowSummary.Controls.Add(notif);
            flowSummary.Controls.SetChildIndex(notif, 0);

            displayedKeys.Add(key);
            itemMap[key] = notif;
        }


        // --- Remove notification by key ---
        private void RemoveNotificationFromFlow(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (itemMap.TryGetValue(key, out SummaryNotificationItem item))
            {
                flowSummary.Controls.Remove(item);
                try { item.Dispose(); } catch { }
                itemMap.Remove(key);
                displayedKeys.Remove(key);
            }
        }

        // --- Pick icon for status ---
        private Image IconForStatus(string status)
        {
            switch (status)
            {
                case "Approved":
                    return Properties.Resources.icon_check;
                case "Declined":
                    return Properties.Resources.icon_cross;
                default:
                    return Properties.Resources.icon_pending;
            }
        }

        private void SetFont()
        {
            labelNotification.Font = AttributesClass.GetFont("Roboto-Regular", 26f);
            buttonClearNotif.Font = AttributesClass.GetFont("Roboto-Regular", 9f);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void buttonClearNotif_Click(object sender, EventArgs e)
        {
            try
            {
                // Snapshot all HR notifications from Firebase
                var hrData = await firebase.Child("HRNotifications").OnceAsync<JObject>();

                // Loop through each and remove if NOT Pending
                foreach (var item in hrData)
                {
                    var data = item.Object;
                    if (data == null) continue;

                    string key = item.Key;
                    string status = data["status"]?.ToString() ?? "Pending";

                    if (!status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        // Delete from Firebase
                        await firebase.Child("HRNotifications").Child(key).DeleteAsync();

                        // Remove from UI immediately (if displayed)
                        SafeInvoke(() => RemoveNotificationFromFlow(key));
                    }
                }

                MessageBox.Show("Cleared all approved/declined notifications successfully!",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing notifications: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
