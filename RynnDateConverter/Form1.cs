using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RynnDateConverter
{
    public partial class Form1 : Form
    {
        public MultiformatDate MyDate; //to hell with encapsulation

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonStandardCalculate_Click(object sender, EventArgs e)
        {
            MyDate = new MultiformatDate(Convert.ToInt64(textBoxStandard.Text));
            UpdateEarthDisplay();
            UpdateRynnDisplay();
            UpdateGalacticDisplay();
            UpdateStandardDisplay();
        }

        protected void UpdateEarthDisplay()
        {

        }

        protected void UpdateRynnDisplay()
        {

        }

        protected void UpdateStandardDisplay()
        {

        }

        protected void UpdateGalacticDisplay()
        {

        }
    }
}
