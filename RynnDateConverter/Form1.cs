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

        private void buttonEarthCalculate_Click(object sender, EventArgs e)
        {
            EarthFormatDate efd = new EarthFormatDate();
            efd.Year = Convert.ToInt32(textBoxEarthYear.Text);
            efd.Month = comboBoxEarthMonth.SelectedIndex + 1;
            efd.Day = (int)numericUpDownEarthDay.Value; //very safe!
            efd.Hour = (int)numericUpDownEarthHour.Value;
            efd.Minute = (int)numericUpDownEarthMinute.Value;
            efd.Second = (int)numericUpDownEarthSecond.Value;
            MyDate = new MultiformatDate(efd);

            UpdateRynnDisplay();
            UpdateGalacticDisplay();
            UpdateStandardDisplay();
        }

        //why protected? I have no idea!

        protected void UpdateEarthDisplay()
        {
            EarthFormatDate efd = MyDate.EarthDate;
            textBoxEarthYear.Text = efd.Year.ToString();
            comboBoxEarthMonth.SelectedIndex = efd.Month-1;
            numericUpDownEarthDay.Value = efd.Day;
            numericUpDownEarthHour.Value = efd.Hour;
            numericUpDownEarthMinute.Value = efd.Minute;
            numericUpDownEarthSecond.Value = efd.Second;
        }

        protected void UpdateRynnDisplay()
        {
            RynnFormatDate rfd = MyDate.RynnDate;
            comboBoxRynnEra.SelectedIndex = rfd.Era - 1;
            textBoxRynnYear.Text = rfd.Year.ToString();
            comboBoxRynnMonth.SelectedIndex = rfd.Month - 1;
            numericUpDownRynnDay.Value = rfd.Day;

            if(rfd.OnEraBoundary)
            {
                labelRynnEra.Text = "On Era Boundary";
            }
            else
            {
                labelRynnEra.Text = "";
            }
        }

        protected void UpdateStandardDisplay()
        {
            textBoxStandard.Text = MyDate.Timestamp.ToString();
        }

        protected void UpdateGalacticDisplay()
        {

        }

        
    }
}
