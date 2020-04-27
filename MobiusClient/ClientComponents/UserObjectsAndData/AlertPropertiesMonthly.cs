using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

    public partial class AlertPropertiesMonthly : UserControl
    {
        private static AlertPropertiesMonthly _instance;
        public static AlertPropertiesMonthly Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AlertPropertiesMonthly();
                return _instance;
            }

        }
        public AlertPropertiesMonthly()
        {
            InitializeComponent();
        }

        public bool ValidateData()
        {
            if (radioButton1.Checked && int.Parse(textEditDayOfMonth.EditValue.ToString()) < 1)
            {
                MessageBoxMx.Show("You must enter a valid Day value.");
                return false;
            }

            if (radioButton1.Checked && int.Parse(textEdit2.EditValue.ToString()) < 1)
            {
                MessageBoxMx.Show("You must enter a valid Month value.");
                return false;
            }

            if (radioButton2.Checked && int.Parse(textEditMonthInterval.EditValue.ToString()) < 1)
            {
                MessageBoxMx.Show("You must select a valid Month interval.");
                return false;
            }

            return true;
        }

        private void textEditDayOfMonth_Enter(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        private void textEdit2_EditValueChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        private void comboBoxEdit1_Enter(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = true;
        }

        private void comboBoxEdit2_Enter(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = true;
        }

        private void textEditMonthInterval_Enter(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = true;
        }

        private void textEdit2_Enter(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        public void SetData(Alert alert)
        {
            if (string.IsNullOrEmpty(alert.Days)) // Day <DayInterval> of every <Intrval> month
            {
                textEditDayOfMonth.Text = alert.DayInterval.ToString();
                textEdit2.Text = alert.Interval.ToString();
                radioButton1.Checked = true;
            }
            else // The second Monday of each 1 Month
            {
                comboBoxEdit1.SelectedIndex = alert.DayInterval - 1;
                comboBoxEdit2.Text = alert.Days;
                textEditMonthInterval.Text = alert.Interval.ToString();
                radioButton2.Checked = true;
            }


        }

        public Dictionary<string, string> GetData()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (radioButton1.Checked)
            {
                string dayInterval = textEditDayOfMonth.Text;
                string interval = textEdit2.Text;
                dictionary.Add("Interval", string.IsNullOrEmpty(interval) ? "" : interval);
                dictionary.Add("DayInterval", string.IsNullOrEmpty(dayInterval) ? "" : dayInterval);
                dictionary.Add("Days","");
            } else if (radioButton2.Checked)
            {
                string interval = textEditMonthInterval.Text;
                if (interval != null) dictionary.Add("Interval", interval);

                string dayInterval = comboBoxEdit1.Text;
                if (dayInterval=="first") dictionary.Add("DayInterval","1");
                if (dayInterval == "second") dictionary.Add("DayInterval", "2");
                if (dayInterval == "third") dictionary.Add("DayInterval", "3");
                if (dayInterval == "fourth") dictionary.Add("DayInterval", "4");
                if (dayInterval == "last") dictionary.Add("DayInterval", "5");

                string days = comboBoxEdit2.Text;
                if (days!=null) dictionary.Add("Days", days);
            }



            return dictionary;
        }
    }
}
