using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class DateFormatDialog : XtraForm
	{

		static DateFormatDialog Instance = null;

		public DateFormatDialog()
		{
			InitializeComponent();
		}

		public static DialogResult Show(
			QueryColumn qc)
		{
			string tok;

			if (Instance == null) Instance = new DateFormatDialog();
			DateFormatDialog dfd = Instance;

			MetaColumn mc = qc.MetaColumn;
			if (mc.DataType != MetaColumnType.Date)
			{
				XtraMessageBox.Show(mc.Label + " is not a date field");
				return DialogResult.Cancel;
			}

			// Setup

			new ControlMxConverter().Convert(Instance);

			string fmt = qc.DisplayFormatString;
			if (Lex.IsNullOrEmpty(fmt)) fmt = "d-MMM-yyyy"; // default format

			if (Lex.Contains(fmt, "d-MMM-yyyy")) dfd.d_MMM_yyyy.Checked = true;
			else if (Lex.Contains(fmt, "d-MMM-yy")) dfd.d_MMM_yy.Checked = true;
			else if (Lex.Contains(fmt, "M/d/yyyy")) dfd.M_d_yyyy.Checked = true;
			else if (Lex.Contains(fmt, "M/d/yy")) dfd.M_d_yy.Checked = true;
			else if (Lex.Contains(fmt, "none")) dfd.DateNone.Checked = true;
			else dfd.d_MMM_yyyy.Checked = true; // in case of no match

			if (Lex.Contains(fmt, "h:mm:ss tt")) dfd.h_mm_ss_tt.Checked = true; // check in reverse order to get correct hit
			else if (Lex.Contains(fmt, "h:mm tt")) dfd.h_mm_tt.Checked = true;
			else if (Lex.Contains(fmt, "H:mm:ss")) dfd.H_mm_ss.Checked = true;
			else if (Lex.Contains(fmt, "H:mm")) dfd.H_mm.Checked = true;
			else dfd.TimeNone.Checked = true;

// Show form & get new values

			DialogResult dr = dfd.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				if (dfd.d_MMM_yyyy.Checked) fmt = "d-MMM-yyyy";
				else if (dfd.d_MMM_yy.Checked) fmt = "d-MMM-yy";
				else if (dfd.M_d_yyyy.Checked) fmt = "M/d/yyyy";
				else if (dfd.M_d_yy.Checked) fmt = "M/d/yy";
				else if (dfd.DateNone.Checked) fmt = "none";

				if (dfd.H_mm.Checked) fmt += " H:mm";
				else if (dfd.H_mm_ss.Checked) fmt += " H:mm:ss";
				else if (dfd.h_mm_tt.Checked) fmt += " h:mm tt";
				else if (dfd.h_mm_ss_tt.Checked) fmt += " h:mm:ss tt"; 
				else if (dfd.TimeNone.Checked) { }; // nothing if no time

				qc.DisplayFormatString = fmt;
			}

			return dr;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void Scientific_CheckedChanged(object sender, EventArgs e)
		{

		}

	}
}
