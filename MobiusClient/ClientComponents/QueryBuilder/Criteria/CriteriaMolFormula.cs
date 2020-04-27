using Mobius.ComOps;
using Mobius.Data;

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
	public partial class CriteriaMolFormula : DevExpress.XtraEditors.XtraForm
	{
		static CriteriaMolFormula Instance;

		public CriteriaMolFormula()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Invoke the editor
		/// </summary>
		/// <param name="qc">QueryColumn to edit</param>
		/// <returns></returns>

		public static bool Edit(
			QueryColumn qc)
		{
			ParsedSingleCriteria psc;

			if (qc.MetaColumn.DictionaryMultipleSelect) return CriteriaDictMultSelect.Edit(qc);

			if (Instance == null) Instance = new CriteriaMolFormula();

			if (qc.Criteria != "")
				psc = MqlUtil.ParseQueryColumnCriteria(qc);

			else
			{
				psc = new ParsedSingleCriteria();
				psc.OpEnum = CompareOp.FormulaEqual;
			}

			Instance.Formula.Text = psc.Value;
			if (psc.OpEnum == CompareOp.FormulaEqual) Instance.ExactMF.Checked = true;
			else Instance.PartialMF.Checked = true;

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return false;

			string val = Instance.Formula.Text.Trim();
			if (val == "") // no criteria
			{
				qc.Criteria = qc.CriteriaDisplay = "";
				return true;
			}

			if (Instance.ExactMF.Checked)
			{
				qc.Criteria = qc.MetaColumn.Name + " fmla_eq " + Lex.AddSingleQuotes(val);;
				qc.CriteriaDisplay = "= " + val;
			}

			else
			{
				qc.Criteria = qc.MetaColumn.Name + " fmla_like " + Lex.AddSingleQuotes(val);
				qc.CriteriaDisplay = "like " + val;
			}

			return true;
		}

		private void CriteriaMolFormula_Activated(object sender, EventArgs e)
		{
			Formula.Focus();
		}

	}
}