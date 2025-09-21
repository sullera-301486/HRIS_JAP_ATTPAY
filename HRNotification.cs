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
    public partial class HRNotification : Form
    {
        
        public HRNotification()
        {
            InitializeComponent();
            SetFont();
        }
        

        private void HRNotification_Load(object sender, EventArgs e)
        {
            // ✅ Example 1 - Pending
            var notif1 = new SummaryNotificationItem();
            notif1.SetData(
                "Ej Sullera’s entry for 5-28-2025 pending admin review.",
                Properties.Resources.icon_pending
            );
            flowSummary.Controls.Add(notif1);

            // ❌ Declined
            var notif2 = new SummaryNotificationItem();
            notif2.SetData(
                "Marcus Verzo’s attendance on 5-28-2025 declined by admin.",
                Properties.Resources.icon_cross
            );
            flowSummary.Controls.Add(notif2);

            // ✅ Approved
            var notif3 = new SummaryNotificationItem();
            notif3.SetData(
                "Elijah Siena’s attendance on 5-22-2025 approved by admin.",
                Properties.Resources.icon_check
            );
            flowSummary.Controls.Add(notif3);

            var notif4 = new SummaryNotificationItem();
            notif4.SetData(
                "Maria Hiwaga’s sick attendance was approved by the admin.",
                Properties.Resources.icon_check
            );
            flowSummary.Controls.Add(notif4);

            var notif5 = new SummaryNotificationItem();
            notif5.SetData(
                "Franz Capuno’s attendance on 5-18-2025 approved by admin.",
                Properties.Resources.icon_check
            );
            flowSummary.Controls.Add(notif5);

            var notif6 = new SummaryNotificationItem();
            notif6.SetData(
                "Charles Macaraig’s attendance on 5-11-2025 approved by admin.",
                Properties.Resources.icon_check
            );
            flowSummary.Controls.Add(notif6);

            var notif7 = new SummaryNotificationItem();
            notif7.SetData(
                "Marcus Verzo’s attendance on 4-27-2025 declined by admin.",
                Properties.Resources.icon_cross
            );
            flowSummary.Controls.Add(notif7);

            var notif8 = new SummaryNotificationItem();
            notif8.SetData(
                "Marcus Verzo’s attendance on 4-27-2025 declined by admin.",
                Properties.Resources.icon_cross
            );
            flowSummary.Controls.Add(notif8);

            var notif9 = new SummaryNotificationItem();
            notif9.SetData(
                "Marcus Verzo’s attendance on 4-27-2025 declined by admin.",
                Properties.Resources.icon_cross
            );
            flowSummary.Controls.Add(notif9);

            var notif10 = new SummaryNotificationItem();
            notif10.SetData(
                "Marcus Verzo’s attendance on 4-27-2025 declined by admin.",
                Properties.Resources.icon_cross
            );
            flowSummary.Controls.Add(notif10);

            var notif11 = new SummaryNotificationItem();
            notif11.SetData(
                "Marcus Verzo’s attendance on 4-27-2025 declined by admin.",
                Properties.Resources.icon_cross
            );
            flowSummary.Controls.Add(notif11);
        }
        
        private void SetFont()
        {
            labelNotification.Font = AttributesClass.GetFont("Roboto-Regular", 26f);
        }

        private void XpictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
