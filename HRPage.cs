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
    public partial class HRForm : Form
    {
        private AttributesClassAlt panelLoaderView;
        private AttributesClassAlt panelLoaderMenu;
        public HRForm()
        {
            InitializeComponent();

            AttributesClass.SetMinSize(this, 1440, 1024);
            this.Size = new Size(1240, 824);
            panelLoaderMenu = new AttributesClassAlt(HRMenuPanel);
            panelLoaderView = new AttributesClassAlt(HRViewPanel);

        }
        private void HRPage_Load(object sender, EventArgs e)
        {
            panelLoaderMenu.LoadUserControl(new HRMenu(HRViewPanel));
            panelLoaderView.LoadUserControl(new HROverview());
        }

    }
}
