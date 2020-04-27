using Mobius.ComOps;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class QnfSplitDialog : DevExpress.XtraEditors.XtraForm
	{
		static QnfSplitDialog Instance;

		public QnfSplitDialog()
		{
			InitializeComponent();
		}

		public static string Show(
			string qnfSplitText)
		{
			if (Instance == null) Instance = new QnfSplitDialog();

			Instance.Deserialize(qnfSplitText);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			qnfSplitText = Instance.Serialize();
			return qnfSplitText;
		}

		/// <summary>
		/// Convert serialized string into set of check boxes
		/// </summary>
		/// <param name="txt"></param>

		public void Deserialize(
			string txt)
		{
			Qualifier.Checked = (Lex.Contains(txt, "Qualifier") ? true : false);
			NumericValue.Checked = (Lex.Contains(txt, "Number") ? true : false);
			StdDev.Checked = (Lex.Contains(txt, "SD") ? true : false);
			StdErr.Checked = (Lex.Contains(txt, "SE") ? true : false);
			NValue.Checked = (Lex.Contains(txt, "N,") || txt.EndsWith("N") ? true : false);
			NValueTested.Checked = (Lex.Contains(txt, "NT") ? true : false);
			TextValue.Checked = (Lex.Contains(txt, "Text") ? true : false);
		}

		/// <summary>
		/// Convert check boxes into string
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			string txt = "";

			if (Qualifier.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "Qualifier";
			}
			if (NumericValue.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "Number";
			}
			if (StdDev.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "SD";
			}
			if (StdErr.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "SE";
			}
			if (NValue.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "N";
			}
			if (NValueTested.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "NT";
			}

			if (TextValue.Checked)
			{
				if (txt != "") txt += ", ";
				txt += "Text";
			}

			return txt;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (this.Serialize() == "")
			{
				MessageBoxMx.Show("At least one item must be selected", UmlautMobius.String);
				return;
			}
			this.DialogResult = DialogResult.OK;
		}
	}
}