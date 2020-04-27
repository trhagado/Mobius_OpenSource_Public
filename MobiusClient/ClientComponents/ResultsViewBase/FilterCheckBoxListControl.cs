using Mobius.ComOps;
using Mobius.Data;
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
	public partial class FilterCheckBoxListControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal QueryManager QueryManager { get { return QueryResultsControl.GetCurrentViewQm(this); } }
		DataTableManager MolTable { get { return QueryManager.DataTableManager; } }
		ColumnInfo ColInfo; // info on column being edited

		int AllPos = 0;
		int BlanksPos = 1;
		int NonBlanksPos = 2;

		bool InSetup = false;

		public FilterCheckBoxListControl()
		{
			InitializeComponent();
		}

		public void Setup(ColumnInfo colInfo)
		{
			bool check;
			string s;

			InSetup = true;
			ColInfo = colInfo; // save ref to colInfo
			QueryColumn qc = colInfo.Qc;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(ColInfo.Qc.SecondaryCriteria); // parse criteria

			Dictionary<string, object> listDict = new Dictionary<string,object>();

			if (psc != null && psc.OpEnum != CompareOp.In) 
				psc = null; // switching from other criteria type

			if (psc != null && Lex.Contains(qc.SecondaryCriteria, "(All)"))
				psc = null; // rebuild if "all" items

			if (psc != null) // list of previously selected items
			{
				foreach (string vs in psc.ValueList)
				{
					if (qc.MetaColumn.DataType == MetaColumnType.CompoundId)
						s = CompoundId.Format(vs);
					else if (qc.MetaColumn.DataType == MetaColumnType.Date)
						s = DateTimeMx.Format(vs);
					else s = vs;
					listDict[s.ToUpper()] = null;
				}
			}

			else // setup default criteria
			{
				qc.SecondaryCriteria = qc.MetaColumn.Name + " in ('(All)')";
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " in list (All)";
			}

			ColumnStatistics stats = colInfo.Rfld.GetStats();
			CheckedListBoxItemCollection items = ItemList.Items;
			items.Clear();
			check = (psc == null || listDict.ContainsKey("(All)".ToUpper()) || listDict.ContainsKey("All".ToUpper()));
			AddItem("(All)", check);

			if (stats.NullsExist || Math.Abs(1) == 1)
			{ // always include blank/nonblank since there may be null rows because other tables have more rows for key
				check = (psc == null || listDict.ContainsKey("(Blanks)".ToUpper()) || listDict.ContainsKey("Blanks".ToUpper()));
				BlanksPos = items.Count;
				AddItem("(Blanks)", check);

				check = (psc == null || listDict.ContainsKey("(Non blanks)".ToUpper()) || listDict.ContainsKey("Nonblanks".ToUpper()));
				NonBlanksPos = items.Count;
				AddItem("(Non blanks)", check);
			}

			else BlanksPos = NonBlanksPos = -1;

			foreach (MobiusDataType mdt in stats.DistinctValueList)
			{
				s = mdt.FormattedText;
				check = (psc == null || listDict.ContainsKey(s.ToUpper()));
				AddItem(s, check);
				if (items.Count >= 100)
				{
					AddItem("...", check);
					break;
				}
			}

			int itemHeight = 18; // height of single item (really 17 but add a bit extra)
			int fullHeight = ItemList.Top + 6 + ItemList.Items.Count * itemHeight; // full height of control
			if (fullHeight < this.Height) this.Height = fullHeight;

			InSetup = false;
			return;
		}

		void AddItem(string desc, bool check)
		{
			int ii = ItemList.Items.Add(null, check);
			ItemList.Items[ii].Description = desc;
			return;
		}

/// <summary>
/// When check state changes update the display to reflect
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ItemList_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			ItemList.BeginUpdate();

			CheckedListBoxItemCollection items = ItemList.Items;
			string txt = items[e.Index].Description;
			bool isChecked = (e.State == CheckState.Checked);

			if (txt == "(All)") // check/uncheck everything
			{
				foreach (CheckedListBoxItem i in items)
					i.CheckState = e.State;
			}

			else if (txt == "(Non blanks)")
			{
				foreach (CheckedListBoxItem i in items)
				{
					if (i.Description == "(All)" || i.Description == "(Blanks)") continue;
					i.CheckState = e.State;
				}
			}

			else if (e.State == CheckState.Unchecked) // turned item off; turn off All & Non blanks as well
			{
				items[AllPos].CheckState = CheckState.Unchecked;
				if (NonBlanksPos >= 0 && txt != "(Blanks)")
					items[NonBlanksPos].CheckState = CheckState.Unchecked;
			}

			if (BlanksPos >= 0) // if blanks allowed set (All) based on Blanks/Non blanks settings
			{
				if (items[BlanksPos].CheckState == CheckState.Checked &&
					items[NonBlanksPos].CheckState == CheckState.Checked)
					items[AllPos].CheckState = CheckState.Checked;
				else items[AllPos].CheckState = CheckState.Unchecked;
			}

			ItemList.EndUpdate();
			InSetup = false;

// Generate new criteria from set of checks

			QueryColumn qc = ColInfo.Qc;

			if (items[AllPos].CheckState == CheckState.Checked) // everything
			{
				qc.SecondaryCriteria = qc.MetaColumn.Name + " in ('(All)')";
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " in list (All)";
			}

			//else if (NonBlanksPos >= 0 && items[NonBlanksPos].CheckState == CheckState.Checked)
			//{ // just non-null
			//  qc.SecondaryCriteria = qc.MetaColumn.Name + " is not null";
			//  qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " is not null";
			//}

			else // build list of checked items possibly including "(Blanks)" and "(Non blanks)"
			{
				int itemsChecked = 0;
				qc.SecondaryCriteria = qc.MetaColumn.Name + " in (";

				foreach (CheckedListBoxItem i in items)
				{
					string normalizedString;

					if (i.CheckState != CheckState.Checked) continue;

					if (itemsChecked > 0) qc.SecondaryCriteria += ", ";

					normalizedString = i.Description;
					if (i.Description == "(All)" || i.Description == "(Blanks)" || i.Description == "(Non blanks)")
					{ } // these are always ok as is
					else if (qc.MetaColumn.DataType == MetaColumnType.CompoundId || 
						qc.MetaColumn.DataType == MetaColumnType.Date)
					{ // store these in internal format
						MobiusDataType mdt = MobiusDataType.New(qc.MetaColumn.DataType, i.Description);
						normalizedString = mdt.FormatForCriteria();
					}
					qc.SecondaryCriteria += Lex.AddSingleQuotes(normalizedString);
					itemsChecked++;
				}

				qc.SecondaryCriteria += ")";
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " in list";
			}

			qc.SecondaryFilterType = FilterType.CheckBoxList;

			QueryManager.QueryResultsControl.UpdateFiltering(ColInfo);
			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(qc); // sync any base query

			return;
		}

	}
}
