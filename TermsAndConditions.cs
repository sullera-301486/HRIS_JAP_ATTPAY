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
    public partial class TermsAndConditions : Form
    {
        private AttributesClassAlt panelLoaderRectangle;
        public TermsAndConditions()
        {
            InitializeComponent();
            setFont();
            panelLoaderRectangle = new AttributesClassAlt(LoginRectanglePanel);
        }

        private void setFont()
        {
            name1.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            name2.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            name3.Font = AttributesClass.GetFont("Roboto-Regular", 14f);
            labelTermsAndConditions.Font = AttributesClass.GetFont("Roboto-Regular", 20f);
            buttonAccept.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            buttonDecline.Font = AttributesClass.GetFont("Roboto-Regular", 12f);
            labelLastUpdate.Font = AttributesClass.GetFont("Roboto-Light", 10f);
        }

        private void TermsAndConditions_Load(object sender, EventArgs e)
        {
            panelLoaderRectangle.LoadUserControl(new TermsAndConditionsRectangle());
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonDecline_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
