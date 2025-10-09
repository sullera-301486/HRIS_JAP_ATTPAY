using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRIS_JAP_ATTPAY
{
    public partial class SummaryNotificationItem : UserControl
    {
        

        public SummaryNotificationItem()
        {
            InitializeComponent();
            SetFont();
        }

        private void SetFont()
        {
            lblMessage.Font = AttributesClass.GetFont("Roboto-Light", 10f);
        }

        public void SetData(string message, Image icon)
        {
            lblMessage.Text = message;
            picIcon.Image = icon;
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;    
        }

        
    }
}
