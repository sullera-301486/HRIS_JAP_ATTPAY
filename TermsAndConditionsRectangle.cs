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
    public partial class TermsAndConditionsRectangle : UserControl
    {
        public TermsAndConditionsRectangle()
        {
            InitializeComponent();
            setFont();
        }

        private void setFont()
        {
            try
            {
                labelAgreeIntro1.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelAgreeIntro2.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f, FontStyle.Italic);
                labelAgreeIntro3.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc1.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc2.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc3.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc4.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc5.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc6.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc7.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelDesc8.Font = AttributesClass.GetFont("Roboto-Light", 9.5f);
                labelMain1.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain2.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain3.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain4.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain5.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain6.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain7.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
                labelMain8.Font = AttributesClass.GetFont("Roboto-Bold", 9.5f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Font load failed: " + ex.Message);
            }
        }
    }
}
