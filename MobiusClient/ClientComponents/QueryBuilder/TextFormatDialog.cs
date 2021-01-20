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
	public partial class TextFormatDialog : XtraForm
	{

		static TextFormatDialog Instance = null;

		public TextFormatDialog()
		{
			InitializeComponent();
		}

		public static DialogResult Show(
			QueryColumn qc)
		{
			string tok;

			if (Instance == null) Instance = new TextFormatDialog();
			TextFormatDialog dfd = Instance;

			MetaColumn mc = qc.MetaColumn;
			if (mc.DataType != MetaColumnType.String)
			{
				XtraMessageBox.Show(mc.Label + " is not a string field");
				return DialogResult.Cancel;
			}

			// Setup

			new PlotlyDashConverter().ToDash(Instance);

			ColumnFormatEnum df = qc.DisplayFormat;
			if (df != ColumnFormatEnum.HtmlText)
				df = qc.MetaColumn.Format;

			if (df == ColumnFormatEnum.NormalText) dfd.NormalFormatCheckEdit.Checked = true;
			else if (df == ColumnFormatEnum.HtmlText) dfd.HtmlCheckEdit.Checked = true;
			else dfd.NormalFormatCheckEdit.Checked = true; // in case of no match

// Show form & get new values

			DialogResult dr = dfd.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				if (dfd.NormalFormatCheckEdit.Checked) qc.DisplayFormat = ColumnFormatEnum.NormalText;
				else qc.DisplayFormat = ColumnFormatEnum.HtmlText;
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
