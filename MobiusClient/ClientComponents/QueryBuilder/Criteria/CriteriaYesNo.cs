using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Simple Yes/No criteria 
	/// </summary>

	public partial class CriteriaYesNo : XtraForm
	{
		static CriteriaYesNo Instance = null;

		public CriteriaYesNo()
		{
			InitializeComponent();
		}

		public static bool Edit(
			QueryColumn qc)
		{
			AssertMx.IsNotNull(qc, "qc");
			MetaColumn mc = qc.MetaColumn;

			if (Instance == null) Instance = new CriteriaYesNo();

			new PlotlyDashConverter().ToDash(Instance);

			Instance.Text = "Search criteria for " + qc.ActiveLabel;
			Instance.Prompt.Text = "Select a search option for " + qc.ActiveLabel + " from the list below.";

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			if (psc == null) psc = new ParsedSingleCriteria(); // no criteria

			if (Lex.Eq(psc.Value, "Y"))
				Instance.YesCheckEdit.Checked = true;

			else if (Lex.Eq(psc.Value, "N"))
				Instance.NoCheckEdit.Checked = true;

			else Instance.None.Checked = true;

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			if (dr == DialogResult.OK)
			{
				if (Instance.YesCheckEdit.Checked)
				{
					qc.CriteriaDisplay = "= Y";
					qc.Criteria = mc.Name + " = 'Y'";
				}

				else if (Instance.NoCheckEdit.Checked)
				{
					qc.CriteriaDisplay = "= N";
					qc.Criteria = mc.Name + " = 'N'";
				}

				else if (Instance.None.Checked)
				{
					qc.CriteriaDisplay = "";
					qc.Criteria = "";
				}

				return true;
			}

			else return false;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			return;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			return;
		}
	}
}