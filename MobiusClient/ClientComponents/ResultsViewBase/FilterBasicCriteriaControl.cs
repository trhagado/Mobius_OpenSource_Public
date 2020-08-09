using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

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
	public partial class FilterBasicCriteriaControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal QueryManager QueryManager { get { return QueryResultsControl.GetCurrentViewQm(this); }}
		DataTableManager MolTable { get { return QueryManager.DataTableManager; } }
		ColumnInfo ColInfo; // info on column being edited
		IFilterManager FilterManager;
		CompareOp CompOp; 

		bool InSetup = false;

		public FilterBasicCriteriaControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Initial setup for control
		/// </summary>
		/// <param name="colInfo"></param>

		public void Setup(ColumnInfo colInfo, IFilterManager filterManager)
		{
			ColInfo = colInfo; // save ref to colInfo
			FilterManager = filterManager;
			Setup(CompareOp.Unknown); // setup, keeping any current operation or assigning "like" if no current op
		}

/// <summary>
/// Initial setup or change in operator
/// </summary>
/// <param name="compOp"></param>

		void Setup(CompareOp compOp)
		{
			string txt;

			InSetup = true;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(ColInfo.Qc.SecondaryCriteria); // parse criteria

			if (psc == null)
			{
				psc = new ParsedSingleCriteria();
				if (compOp == CompareOp.Unknown) compOp = CompareOp.Like; // if no existing or supplied op use "like"
			}
			else if (compOp == CompareOp.Unknown) compOp = psc.OpEnum; // if no existing but defined in criteria then use criteria

			psc.OpEnum = compOp;

			CompOp = compOp;
			int ctls = 1;
			if (compOp == CompareOp.Like) txt = ContainsMenuItem.Text;
			else if (compOp == CompareOp.Eq) txt = EqMenuItem.Text;
			else if (compOp == CompareOp.Ne) txt = NotEqMenuItem.Text;
			else if (compOp == CompareOp.Lt) txt = LtMenuItem.Text;
			else if (compOp == CompareOp.Le) txt = LeMenuItem.Text;
			else if (compOp == CompareOp.Gt) txt = GtMenuItem.Text;
			else if (compOp == CompareOp.Ge) txt = GeMenuItem.Text;
			else if (compOp == CompareOp.Between) { txt = BetweenMenuItem.Text; ctls = 2; }
			else if (compOp == CompareOp.IsNull) { txt = IsBlankMenuItem.Text; ctls = 0; }
			else if (compOp == CompareOp.IsNotNull) { txt = IsNotBlankMenuItem.Text; ctls = 0; }
			else
			{ // in case of non-match
				CompOp = CompareOp.Like;
				txt = ContainsMenuItem.Text;
			}

			if (ctls == 0)
				Value.Visible = Value2.Visible = false;

			else if (ctls == 1)
			{
				Value.Visible = true;
				Value2.Visible = false;
				Value.Width = (Width - 4) - Value.Left;
				Value.Text = FormatCriteriaForDisplay(compOp, psc.Value);
			}

			else
			{
				Value.Visible = Value2.Visible = true;
				ResizeValueControls();
				Value.Text = FormatCriteriaForDisplay(compOp, psc.Value);
				Value2.Text = FormatCriteriaForDisplay(compOp, psc.Value2);
			}

			OpButton.Text = txt;
			if (Value.Visible) Value.Focus();

			InSetup = false;
			return;
		}

/// <summary>
/// Format a criteria value for display
/// </summary>
/// <param name="comOp"></param>
/// <param name="value"></param>
/// <returns></returns>

		string FormatCriteriaForDisplay(CompareOp comOp, string value)
		{
			if (CompOp == CompareOp.Like) return value;
			if (String.IsNullOrEmpty(value)) return "";
			MobiusDataType mdt = MobiusDataType.New(ColInfo.Mc.DataType, value);
			string formattedValue = mdt.FormatCriteriaForDisplay();
			return formattedValue;
		}

		void ResizeValueControls()
		{
			if (!Value2.Visible) return;
			int w = (Width - 4) - Value.Left;
			w = w / 2 - 2;
			Value.Width = Value2.Width = w;
			Value2.Left = Value.Right + 4;
			return;
		}

		/// <summary>
		/// Text changed, update filter flags accordingly
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Value_TextChanged(object sender, EventArgs e)
		{
			MobiusDataType mdt = null, mdt2 = null;
			string criteriaString = null, criteriaDisplayString = null, criteriaString2 = null, criteriaDisplayString2 = null;

			if (InSetup) return;

			QueryColumn qc = ColInfo.Qc;
			qc.SecondaryFilterType = FilterType.BasicCriteria;
			qc.SecondaryCriteria = qc.SecondaryCriteriaDisplay = "";
			bool validInput = true;

			if (Value.Visible && Value.Text.Trim() == "") // no filter if nothing entered
				qc.SecondaryCriteria = qc.SecondaryCriteriaDisplay = "";

			else if (CompOp == CompareOp.Like) // keep criteria in literal text form for like
			{
				qc.SecondaryCriteria = qc.MetaColumn.Name + " like " + Lex.AddSingleQuotes(Value.Text.Trim());
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " contains " + Lex.AddSingleQuotes(Value.Text.Trim());
			}

			else // normalize for comparison for other filter types
			{
				string op = CompareOpString.Values[(int)CompOp];
				string opd = CompareOpDisplayString.Values[(int)CompOp];

				if (Value.Visible) // get first value
				{
					if (MobiusDataType.TryParse(qc.MetaColumn.DataType, Value.Text, out mdt))
					{
						criteriaString = mdt.FormatForCriteria();
						criteriaDisplayString = mdt.FormatCriteriaForDisplay();
					}
					else validInput = false;
				}

				if (CompOp == CompareOp.Between) // get 2nd value
				{
					if (MobiusDataType.TryParse(qc.MetaColumn.DataType, Value2.Text, out mdt2))
					{
						criteriaString2 = mdt2.FormatForCriteria();
						criteriaDisplayString2 = mdt2.FormatCriteriaForDisplay();
					}
					else validInput = false;
				}

				if (validInput)
				{
					qc.SecondaryCriteria = qc.MetaColumn.Name + " " + op;
					qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " " + opd;

					if (mdt != null)
					{
						qc.SecondaryCriteria += " " + Lex.AddSingleQuotes(criteriaString);
						qc.SecondaryCriteriaDisplay += " " + Lex.AddSingleQuotes(criteriaDisplayString);
					}

					if (mdt2 != null)
					{
						qc.SecondaryCriteria += " and " + Lex.AddSingleQuotes(criteriaString2);
						qc.SecondaryCriteriaDisplay += " and " + Lex.AddSingleQuotes(criteriaDisplayString2);
					}

				}

				else
				{
					qc.SecondaryCriteria = qc.MetaColumn.Name + " like 'No-Match-String'"; // assure no matches
					qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " " + opd + " invalid value";
				}
			}

			QueryManager.QueryResultsControl.UpdateFiltering(ColInfo); // Update the filters and the associated view
			SyncBaseQuerySecondaryCriteria(qc); // sync any base query

			return;
		}

/// <summary>
/// Synch the secondary criteria for the QueryColumn an any base query
/// </summary>
/// <param name="qc"></param>

		internal static void SyncBaseQuerySecondaryCriteria(QueryColumn qc)
		{
			if (qc == null) return;
			QueryColumn qc0 = QueriesControl.BaseQueryQc(qc);
			qc0.SecondaryFilterType = qc.SecondaryFilterType;
			qc0.SecondaryCriteria = qc.SecondaryCriteria;
			qc0.SecondaryCriteriaDisplay = qc.SecondaryCriteriaDisplay;
		}

		private void Value2_TextChanged(object sender, EventArgs e)
		{
			Value_TextChanged(sender, e);
		}

		private void OpButton_Click(object sender, EventArgs e)
		{
			OpMenu.Show(OpButton, new Point(0, OpButton.Height));
		}

		private void OpButton_ShowDropDownControl(object sender, ShowDropDownControlEventArgs e)
		{
			OpMenu.Show(OpButton, new Point(0, OpButton.Height));
		}

		private void ContainsMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Like);
		}

		private void EqMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Eq);
		}

		private void NotEqMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Ne);
		}

		private void LtMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Lt);
		}

		private void LeMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Le);
		}

		private void GtMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Gt);
		}

		private void GeMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Ge);
		}

		private void BetweenMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.Between);
		}

		private void IsBlankMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.IsNull);
		}

		private void IsNotBlankMenuItem_Click(object sender, EventArgs e)
		{
			SwitchOperators(CompareOp.IsNotNull);
		}

		void SwitchOperators (CompareOp newOp)
		{
			Setup(newOp);
			Value_TextChanged(null, null); // Update the filters and the associated view
		}

		private void FilterBasicControl_Resize(object sender, EventArgs e)
		{
			ResizeValueControls();
		}

		private void ListFilterMenuItem_Click(object sender, EventArgs e)
		{
			if (FilterManager != null) FilterManager.ChangeFilterTypeCallback(ColInfo.Qc, FilterType.CheckBoxList);
		}

		private void ItemSliderMenuItem_Click(object sender, EventArgs e)
		{
			if (FilterManager != null) FilterManager.ChangeFilterTypeCallback(ColInfo.Qc, FilterType.ItemSlider);
		}

		private void RangeSliderMenuItem_Click(object sender, EventArgs e)
		{
			if (FilterManager != null) FilterManager.ChangeFilterTypeCallback(ColInfo.Qc, FilterType.RangeSlider);
		}

	}

/// <summary>
/// Interface for calling back to filter manager to switch from current filter control
/// </summary>

	public interface IFilterManager
	{

/// <summary>
/// Callback to change the filter type 
/// </summary>
/// <param name="qc"></param>
/// <param name="newType"></param>

		void ChangeFilterTypeCallback(QueryColumn qc, FilterType newType);

/// <summary>
/// Get the associated QueryManager
/// </summary>

		QueryManager GetQueryManager();

	}
}
