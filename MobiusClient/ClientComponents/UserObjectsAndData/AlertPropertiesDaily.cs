using TextBox = DevExpress.XtraRichEdit.API.Native.TextBox;

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
    public partial class AlertPropertiesDaily : UserControl
    {
        private static AlertPropertiesDaily _instance;

        public static AlertPropertiesDaily Instance 
        {
            get
            {
                if (_instance == null)
                    _instance = new AlertPropertiesDaily();
                return _instance;
            }
            
        }
        public AlertPropertiesDaily()
        {
            InitializeComponent();
            textBoxEveryDay.Focus();
        }

        public void SetData(Alert alert) //(int interval, bool weekDay)
        {
            textBoxEveryDay.Text = alert.Interval.ToString();
            bool weekday = IsEveryWeekday(alert);
            rbtnEveryWeekday.Checked = weekday;
            rbtnEvery.Checked = !weekday;
            if (!weekday) textBoxEveryDay.Focus();
        }

        public Dictionary<string, string> GetData()
        {
            Dictionary<string,string> dictionary = new Dictionary<string,string>();
            dictionary.Add("Interval", textBoxEveryDay.Text);
            dictionary.Add("Days", rbtnEvery.Checked ? "" : Alert.DaysEnum.WeekDay.ToString());
            return dictionary;
        } 

        private bool IsEveryWeekday(Alert alert)
        {
            if (string.IsNullOrEmpty(alert.Days)) return false;
            return alert.Days.Contains(Alert.DaysEnum.WeekDay.ToString());
        }


        private void textBoxEveryDay_Enter(object sender, EventArgs e)
        {
            rbtnEvery.Checked = true;
            rbtnEveryWeekday.Checked = false;
        }

        public bool ValidateData()
        {
            if (textBoxEveryDay.EditValue == null || int.Parse(textBoxEveryDay.EditValue.ToString()) < 1)
            {
                MessageBoxMx.Show("You must select a valid number of days.");
                return false;
            }
            return true;
        }
    }

}
