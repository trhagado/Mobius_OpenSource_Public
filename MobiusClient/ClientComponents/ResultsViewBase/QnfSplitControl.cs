using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Mobius.ClientComponents
{
	public partial class QnfSplitControl : DevExpress.XtraEditors.XtraUserControl
	{
		public QnfSplitControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup text control with split option
		/// </summary>
		/// <param name="rf"></param>
		/// <param name="control"></param>

		public void Setup(QnfEnum qnf)
		{
			if ((qnf & QnfEnum.Split) != 0)
			{
				qnf |= QnfEnum.Qualifier | QnfEnum.NumericValue; // these are required
				QnfSplit.Checked = true;
				string tok = SerializeQualifiedNumberSplit(qnf);
				QnfSplitFormat.Text = tok;
			}
			else
			{
				QnfCombined.Checked = true;
				QnfSplitFormat.Text = "Qualifier, Number";
			}

		}

		/// <summary>
		/// Serialize the QualifiedNumberSplit settings
		/// </summary>
		/// <param name="qnf"></param>
		/// <returns></returns>

		public static string SerializeQualifiedNumberSplit(
			QnfEnum qnf)
		{
			if ((qnf & QnfEnum.Combined) != 0) return "";

			string txt = "";
			if ((qnf & QnfEnum.Qualifier) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "Qualifier";
			}
			if ((qnf & QnfEnum.NumericValue) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "Number";
			}
			if ((qnf & QnfEnum.StdDev) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "SD";
			}
			if ((qnf & QnfEnum.StdErr) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "SE";
			}
			if ((qnf & QnfEnum.NValue) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "N";
			}
			if ((qnf & QnfEnum.NValueTested) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "NT";
			}

			if ((qnf & QnfEnum.TextValue) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "Text";
			}

			return txt;
		}

/// <summary>
/// Deserialize the QualifiedNumberSplit settings
/// </summary>
/// <returns></returns>

		public QnfEnum Get()
		{
			if (QnfCombined.Checked) return QnfEnum.Combined;
			else return DeserializeQualifiedNumberSplit(QnfSplitFormat.Text);
		}

		/// <summary>
		/// Deserialize the QualifiedNumberSplit settings
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static QnfEnum DeserializeQualifiedNumberSplit(string txt)
		{
			QnfEnum qnf;

			if (txt == null || txt.Trim().Length == 0) return QnfEnum.Combined; // not split

			qnf = QnfEnum.Split; // values should be split

			if (Lex.Contains(txt, "Qualifier")) qnf |= QnfEnum.Qualifier;
			if (Lex.Contains(txt, "Number")) qnf |= QnfEnum.NumericValue;
			if (Lex.Contains(txt, "SD")) qnf |= QnfEnum.StdDev;
			if (Lex.Contains(txt, "SE")) qnf |= QnfEnum.StdErr;
			if (Lex.Contains(txt, "N,") || Lex.EndsWith(txt, "N")) qnf |= QnfEnum.NValue;
			if (Lex.Contains(txt, "NT")) qnf |= QnfEnum.NValueTested;
			if (Lex.Contains(txt, "Text")) qnf |= QnfEnum.TextValue;

			return qnf;
		}

		private void QnfSplit_CheckedChanged(object sender, EventArgs e)
		{
			QnfSplitFormat.Enabled = QnfSplit.Checked;
			QnfSplitEdit.Enabled = QnfSplit.Checked;
		}

		private void QnfSplitEdit_Click(object sender, EventArgs e)
		{
			QnfSplitDialog qnf = new QnfSplitDialog();
			qnf.Deserialize(QnfSplitFormat.Text);
			DialogResult dr = qnf.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return;
			QnfSplitFormat.Text = qnf.Serialize();
			return;
		}

		private void QnfSplitFormat_MouseClick(object sender, MouseEventArgs e)
		{
			return;
			//QnfSplitEdit_Click(sender, null);
		}

		private void QnfSplitFormat_MouseDown(object sender, MouseEventArgs e)
		{
			QnfSplitEdit_Click(sender, null);
		}

		private void QnfSplitFormat_KeyDown(object sender, KeyEventArgs e)
		{
			QnfSplitEdit_Click(sender, null);
		}


	}
}
