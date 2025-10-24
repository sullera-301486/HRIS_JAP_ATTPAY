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
using System.Text.RegularExpressions;

namespace HRIS_JAP_ATTPAY
{
    public partial class FilterAuditTrails : Form
    {
        // Events so AdminOverview can subscribe
        public event Action<AuditTrailFilterCriteria> FiltersApplied;
        public event Action FiltersReset;

        private FirebaseClient firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public FilterAuditTrails()
        {
            InitializeComponent();
            setFont();

            // Load action types from Firebase
            LoadActionTypes();

            // Set default date to today
            dateTimePicker1.Value = DateTime.Today;
        }

        private void setFont()
        {
            try
            {
                labelSearchFilters.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelActionType.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                labelDate.Font = AttributesClass.GetFont("Roboto-Regular", 12f, FontStyle.Bold);
                comboBoxActionType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                dateTimePicker1.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonApply.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
                buttonReset.Font = AttributesClass.GetFont("Roboto-Regular", 16f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private async void LoadActionTypes()
        {
            try
            {
                var actionTypes = new HashSet<string>();

                // Load from AdminLogs
                var adminLogs = await firebase.Child("AdminLogs").OnceAsync<dynamic>();

                if (adminLogs != null && adminLogs.Any())
                {
                    foreach (var log in adminLogs)
                    {
                        try
                        {
                            var logData = log.Object;

                            // Extract action type
                            string actionType = logData.action_type?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(actionType))
                            {
                                actionTypes.Add(actionType);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing log: {ex.Message}");
                        }
                    }
                }

                // Populate Action Type ComboBox
                comboBoxActionType.Items.Clear();
                comboBoxActionType.Items.Add("Select action type");
                foreach (var actionType in actionTypes.OrderBy(a => a))
                {
                    comboBoxActionType.Items.Add(actionType);
                }
                comboBoxActionType.SelectedIndex = 0;

                System.Diagnostics.Debug.WriteLine($"Loaded {actionTypes.Count} action types");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load audit trail filters: {ex.Message}");

                // Add default items if loading fails
                comboBoxActionType.Items.Clear();
                comboBoxActionType.Items.Add("Select action type");
                comboBoxActionType.SelectedIndex = 0;
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            var filters = new AuditTrailFilterCriteria
            {
                ActionType = comboBoxActionType.SelectedItem?.ToString() ?? "",
                Date = dateTimePicker1.Value.ToString("yyyy-MM-dd") // Get date from DateTimePicker
            };

            FiltersApplied?.Invoke(filters);
            this.Close();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            // Reset the form controls
            comboBoxActionType.SelectedIndex = 0;
            dateTimePicker1.Value = DateTime.Today;

            FiltersReset?.Invoke();
            this.Close();
        }
    }

    // Filter criteria class for audit trails
    public class AuditTrailFilterCriteria
    {
        public string ActionType { get; set; }
        public string Date { get; set; }
    }
}