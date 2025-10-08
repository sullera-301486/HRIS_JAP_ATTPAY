using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class NewLeave : Form
    {
        private readonly FirebaseClient firebase;
        private Dictionary<string, JObject> employeeData = new Dictionary<string, JObject>();

        public NewLeave()
        {
            InitializeComponent();
            setFont();
            setTextBoxAttributes();

            // 🔹 Initialize Firebase connection
            firebase = new FirebaseClient("https://thesis151515-default-rtdb.asia-southeast1.firebasedatabase.app/");

            // 🔹 Load employee list on form load
            this.Load += async (s, e) => await LoadEmployeeNames();
            comboBoxInputName.SelectedIndexChanged += ComboBoxInputName_SelectedIndexChanged;
        }

        // 🔹 Load all employees from Leave Credits table
        private async Task LoadEmployeeNames()
        {
            try
            {
                var leaveCredits = await firebase
                    .Child("Leave Credits")
                    .OnceAsync<object>();

                comboBoxInputName.Items.Clear();
                employeeData.Clear();

                foreach (var item in leaveCredits)
                {
                    if (item.Object == null) continue;

                    var emp = JObject.FromObject(item.Object);

                    string fullName = emp["full_name"]?.ToString();
                    if (string.IsNullOrEmpty(fullName)) continue;

                    comboBoxInputName.Items.Add(fullName);
                    employeeData[fullName] = emp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee names: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🔹 When an employee is selected, show their info
        private void ComboBoxInputName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedName = comboBoxInputName.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedName) || !employeeData.ContainsKey(selectedName))
                    return;

                JObject emp = employeeData[selectedName];

                labelDepartmentInput.Text = emp["department"]?.ToString() ?? "N/A";
                labelPositionInput.Text = emp["position"]?.ToString() ?? "N/A";
                labelSickLeaveInput.Text = emp["sick_leave"]?.ToString() ?? "0";
                labelVacationLeaveInput.Text = emp["vacation_leave"]?.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employee details: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            ConfirmLeaveEntry confirmLeaveEntryForm = new ConfirmLeaveEntry();
            AttributesClass.ShowWithOverlay(parentForm, confirmLeaveEntryForm);
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
                labelAddLeaveRecord.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelDash.Font = AttributesClass.GetFont("Roboto-Regular", 10f);
                labelDepartment.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelDepartmentInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelName.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelNewLeave.Font = AttributesClass.GetFont("Roboto-Regular", 18f);
                labelLeaveType.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPeriod.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPosition.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelPositionInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelSickLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelSickLeaveInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                labelVacationLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                labelVacationLeaveInput.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                textBoxEndPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxInputName.Font = AttributesClass.GetFont("Roboto-Light", 15f);
                textBoxStartPeriod.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                buttonAdd.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                buttonCancel.Font = AttributesClass.GetFont("Roboto-Light", 12f);
                comboBoxLeaveType.Font = AttributesClass.GetFont("Roboto-Light", 12f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void setTextBoxAttributes()
        {
            AttributesClass.TextboxPlaceholder(textBoxStartPeriod, "Start of leave");
            AttributesClass.TextboxPlaceholder(textBoxEndPeriod, "End of leave");
        }
    }
}
