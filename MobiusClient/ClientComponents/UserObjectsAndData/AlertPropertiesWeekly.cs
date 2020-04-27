using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
    public partial class AlertPropertiesWeekly : UserControl
    {
        private static AlertPropertiesWeekly _instance;

        public static AlertPropertiesWeekly Instance
        {
            get
            {
                if (_instance == null) _instance = new AlertPropertiesWeekly();
                return _instance;
            }

        }
        public AlertPropertiesWeekly()
        {
            InitializeComponent();
        }

        public void SetData(Alert alert)
        {
            checkBoxSunday.Checked = alert.Days.Contains("Sunday"); 
            checkBoxMonday.Checked = alert.Days.Contains("Monday");
            checkBoxTuesday.Checked = alert.Days.Contains("Tuesday");
            checkBoxWednesday.Checked = alert.Days.Contains("Wednesday");
            checkBoxThursday.Checked = alert.Days.Contains("Thursday");
            checkBoxFriday.Checked = alert.Days.Contains("Friday");
            checkBoxSaturday.Checked = alert.Days.Contains("Saturday");
            textEditNumWeeks.Text = alert.Interval.ToString();
        }

        public Dictionary<string, string> GetData()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("Interval", textEditNumWeeks.Text);

            StringBuilder stringBuilder = new StringBuilder();
            if (checkBoxSunday.Checked) stringBuilder.Append("Sunday|");
            if (checkBoxMonday.Checked) stringBuilder.Append("Monday|");
            if (checkBoxTuesday.Checked) stringBuilder.Append("Tuesday|");
            if (checkBoxWednesday.Checked) stringBuilder.Append("Wednesday|");
            if (checkBoxThursday.Checked) stringBuilder.Append("Thursday|");
            if (checkBoxFriday.Checked) stringBuilder.Append("Friday|");
            if (checkBoxSaturday.Checked) stringBuilder.Append("Saturday|");

            if (stringBuilder.Length > 1) stringBuilder.Remove(stringBuilder.Length - 1, 1); // drop last "|"
            string days = stringBuilder.ToString();

            dictionary.Add("Days", string.IsNullOrEmpty(days) ? "" : days);
            return dictionary;
        }

        public bool ValidateData()
        {
            bool dayChecked =
                 checkBoxSunday.Checked ||
                 checkBoxMonday.Checked ||
                 checkBoxTuesday.Checked ||
                 checkBoxWednesday.Checked ||
                 checkBoxThursday.Checked ||
                 checkBoxFriday.Checked ||
                 checkBoxSaturday.Checked;
            if (!dayChecked)
            {
                MessageBoxMx.Show("You must select a day of the week.");
                return false;
            }

            if (textEditNumWeeks.EditValue == null || int.Parse(textEditNumWeeks.EditValue.ToString()) < 1)
            {
                MessageBoxMx.Show("You must enter the weekly interval.");
                return false;
            }

            return true;
        }
    }
}
