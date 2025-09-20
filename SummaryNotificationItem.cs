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
    public partial class SummaryNotificationItem : UserControl
    {
        public SummaryNotificationItem()
        {
            InitializeComponent();
        }
        public void SetData(string message, Image icon, string timeAgo)
        {
            lblMessage.Text = message;
            lblTime.Text = timeAgo;
            picIcon.Image = icon;
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
        }
    }
}
