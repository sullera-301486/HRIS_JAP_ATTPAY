using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class ManageLeave : Form
    {
        public ManageLeave()
        {
            InitializeComponent();
            SetFont();
        }
        private void SetFont()
        {
            try
            {
                labelLeaveManagement.Font = AttributesClass.GetFont("Roboto-Regular", 24f);
                labelAddLeave.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
                label1.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
                label2.Font = AttributesClass.GetFont("Roboto-Regular", 18f);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void labelAddLeave_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            NewLeave newLeaveForm = new NewLeave();
            AttributesClass.ShowWithOverlay(parentForm, newLeaveForm);
        }
        private void AddLeaveItem(string submittedBy, string employee, string date, string leaveType, string period, Image photo)
        {
            var item = new LeaveList(); // <-- custom UserControl for each row
            item.SetData(submittedBy, employee, date, leaveType, period, photo);

            // Hook up revoke event
            item.RevokeClicked += (s, e) =>
            {
                MessageBox.Show($"Revoked {employee}'s {leaveType} ({period})");
                flowLayoutPanel1.Controls.Remove(item);
            };

            flowLayoutPanel1.Controls.Add(item);
        }

        private void ManageLeave_Load(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();

            // Sample data
            AddLeaveItem("Marcus Verzo", "Ej Sullera", "June 6", "Sick Leave", "June 6", Properties.Resources.User1);
            AddLeaveItem("Marcus Verzo", "Ej Sullera", "June 6", "Vacation Leave", "June 6 - 9", Properties.Resources.User1);
            AddLeaveItem("Maria Hiwaga", "Elijah Siena", "June 10", "Emergency Leave", "June 10 - 12", Properties.Resources.User1);
            AddLeaveItem("Franz Capuno", "Charles Macaraig", "June 12", "Sick Leave", "June 12", Properties.Resources.User1);
            AddLeaveItem("Charles Macaraig", "Maria Hiwaga", "June 14", "Vacation Leave", "June 14 - 16", Properties.Resources.User1);
            AddLeaveItem("Elijah Siena", "Franz Capuno", "June 18", "Paternity Leave", "June 18 - 21", Properties.Resources.User1);
        }
    }
    }

