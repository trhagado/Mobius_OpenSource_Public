using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class FilterStructureControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal QueryManager QueryManager { get { return QueryResultsControl.GetCurrentViewQm(this); } }
		DataTableManager MolTable { get { return QueryManager.DataTableManager; } }
		ColumnInfo ColInfo; // info on column being edited
		bool StructureChanged = false;

		bool InSetup = false;

		public FilterStructureControl()
		{
			InitializeComponent();
		}

		public void Setup(ColumnInfo colInfo)
		{
			bool check;

			InSetup = true;
			ColInfo = colInfo; // save ref to colInfo

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(ColInfo.Qc.SecondaryCriteria); // parse criteria
			if (psc == null) StructureRenditor.MolfileString = "";
			else
			{
				MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, psc.Value);
				MoleculeMx.SetRendererStructure(StructureRenditor, cs);
			}

			Timer.Enabled = true;
			StructureChanged = false;

			InSetup = false;
			return;
		}

/// <summary>
/// Structure from editor has changed. Set flag here to be picked up in timer tick
/// since trying to update view in this event throws and exception.
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Structure_StructureChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			if (!Visible) return;
			this.Focus();
			StructureChanged = true;
			return;
		}

		private void Structure_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) CdkMx.MoleculeControl.EditStructure(StructureRenditor); // edit on single click
			return;
		}

		private void Structure_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			return;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (!StructureChanged) return;

			QueryColumn qc = ColInfo.Qc;

			if (String.IsNullOrEmpty(StructureRenditor.MolfileString))
				qc.SecondaryCriteria = qc.SecondaryCriteriaDisplay = "";

			else
			{
				// Chime: CYAAFQwAncwQGj8h7GZ^yjgsajoFd0PQ1OYrnIdaPTl0lGnQYLHH2prJeJi$BhUHcMsE1TyQisJflsW2r293v92iC1^wVm$8wwLM7^krIFa8A1X6Jvu8VIYgCgJ8$y1RuqgCc5ifKbMAflB
				string chimeString = CdkMx.StructureConverter.MolfileStringToChimeString(StructureRenditor.MolfileString);
				qc.SecondaryCriteria = "SSS ( " + qc.MetaColumn.Name + ", " + Lex.AddSingleQuotes(chimeString) + ") = 1";
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " contains substructure";
			}

			qc.SecondaryFilterType = FilterType.StructureSearch;

			QueryManager.QueryResultsControl.UpdateFiltering(ColInfo);
			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(qc); // sync any base query

			StructureChanged = false;
			return;
		}

		private void Structure_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible) Timer.Enabled = true;
			else Timer.Enabled = false;
		}

	}
}
