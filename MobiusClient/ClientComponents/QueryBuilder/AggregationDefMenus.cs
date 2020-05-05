using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraTab;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// This class provides the menus for selecting AggregationDef attributes
	/// </summary>

	public partial class AggregationDefMenus : XtraUserControl
	{
		public QueryColumn Qc; // QueryColumn aggregation is being selected for
		public QueryTable Qt = null; // QueryTable object to edit
		public AggregationDef AggregationDef = null; // current aggregation definition being edited (may or may not be in Qc)

		public AggregationTypeChangedDelegate AggregationChangedDelegate;

		bool AggregationTypeMenuInitialized = false;
		bool AggregationSubtypeMenuInitialized = false; // (obsolete, remove)

		/// <summary>
		/// Control to select type of aggregation for a data column
		/// </summary>

		public AggregationDefMenus()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the aggregation role menu
/// </summary>
/// <param name="qc"></param>
/// <param name="ad"></param>
/// <param name="p"></param>
/// <param name="aggRoleChangedDelegate"></param>

		public static void ShowRoleMenu(
			QueryColumn qc,
			AggregationDef ad,
			Point p,
			AggregationTypeChangedDelegate aggRoleChangedDelegate)
		{
			AggregationDefMenus ats = new AggregationDefMenus();
			ContextMenuStrip menu = ats.SetupAggregationRoleMenu(qc, ad, aggRoleChangedDelegate);
			menu.Show(p);
			return;
		}

		public ContextMenuStrip SetupAggregationRoleMenu(
			QueryColumn qc,
			AggregationDef ad,
			AggregationTypeChangedDelegate aggRoleChangedDelegate = null)
		{
			Qc = qc; // save call parms
			if (qc != null) Qt = qc.QueryTable;
			AggregationDef = ad;
			AggregationChangedDelegate = aggRoleChangedDelegate;

			AggregationRole r = ad.Role;

			ColumnAreaMenuItem.Checked = (ad.Role == AggregationRole.ColumnGrouping);
			RowAreaMenuItem.Checked = (ad.Role == AggregationRole.RowGrouping);
			DataAreaMenuItem.Checked = (ad.Role == AggregationRole.DataSummary);
			FilterAreaMenuItem.Checked = (ad.Role == AggregationRole.Filtering);
			UndefinedAreaMenuItem.Checked = (ad.Role == AggregationRole.Undefined);

			return AggRoleMenu;
		}

		/// <summary>
		/// ShowAggregationTypeMenu
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="ad"></param>
		/// <param name="p"></param>
		/// <param name="aggTypeChangedDelegate"></param>

		public static void ShowTypeMenu(
			QueryColumn qc,
			AggregationDef ad,
			Point p,
			AggregationTypeChangedDelegate aggTypeChangedDelegate)
		{
			AggregationDefMenus ats = new AggregationDefMenus();
			//if (qc.Aggregation == null) qc.Aggregation = new AggregationDef();

			int qtCount = -1;
			if (qc.QueryTable != null && qc.QueryTable.Query != null)
				qtCount = qc.QueryTable.Query.Tables.Count;

			bool includeGroupingItems = true;
			bool includeSummaryItems = true;
			if (qc.IsKey && qtCount > 1) // if key and more than one table in query then no summarization allowed (only allow grouping on key)
				includeSummaryItems = false;

			ContextMenuStrip menu = ats.SetupAggregationTypeMenu(qc, ad, aggTypeChangedDelegate, includeGroupingItems, includeSummaryItems);
			menu.Show(p);

			return;
		}

		/// <summary>
		/// Build and show the menu for selecting aggregation type
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="p"></param>
		/// <param name="typeDelegate"></param>

		public void ShowMenu(
			QueryColumn qc,
			AggregationDef ad,
			Point p,
			AggregationTypeChangedDelegate typeDelegate)
		{
			ContextMenuStrip menu = SetupAggregationTypeMenu(qc, ad, typeDelegate);
			menu.Show(p);
			return;
		}

		/// <summary>
		/// Build aggregation type menu for specific field
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public ContextMenuStrip SetupAggregationTypeMenu(
			QueryColumn qc,
			AggregationDef ad,
			AggregationTypeChangedDelegate typeDelegate = null,
			bool includeGroupingItems = true,
			bool includeSummaryItems = true)
		{
			Qc = qc; // save call parms
			Qt = qc.QueryTable;
			AggregationDef = ad;
			AggregationChangedDelegate = typeDelegate;

			MetaColumn mc = Qc.MetaColumn;

			// Set Visibility for menu items as appropriate for data type and args

			InitMenuItems();

			if (includeGroupingItems)
			{
				GroupByHeaderMenuItem.Available =
				AggregationSeparator1.Available = true;
			}

			if (includeSummaryItems) // summary types allowed for all data types
			{ 
				SummarizationHeaderMenuItem.Available =
				AggregationSeparator1.Available =
				CountMenuItem.Available =
				CountDistinctMenuItem.Available =
				MinMenuItem.Available =
				MaxMenuItem.Available =
				MedianMenuItem.Available =
				ModeMenuItem.Available =
				ConcatenateMenuItem.Available =
				ConcatenateDistinctMenuItem.Available =
				AggregationSeparator2.Available = true;
			}

			if (mc.IsKey)
			{
				if (includeGroupingItems)
					MatchingValuesMenuItem.Available = true;
			}

			else if (mc.DataType == MetaColumnType.String)
			{
				if (includeGroupingItems)
				{
					MatchingValuesMenuItem.Available =
					FirstLetterMenuItem.Available = true;
				}
			}

			else if (mc.IsNumeric)
			{
				if (includeGroupingItems)
				{
					MatchingValuesMenuItem.Available =
					NumericIntervalMenuItem.Available = true;
				}

				if (includeSummaryItems)
				{
					SumMenuItem.Available = true;

					ArithmeticMeanMenuItem.Available =
					GeometricMeanMenuItem.Available = true;

					if (UnpivotedAssayResult.IsSpAndCrcUnpivotedAssayResultColumn(mc)) // col contains both SP and CRC values
					{
						MostPotentMenuItem.Available =
						ResultMeanMenuItem.Available = true;

						ArithmeticMeanMenuItem.Available =
						GeometricMeanMenuItem.Available = false;
					}
				}

			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				if (includeGroupingItems)
				{
					GroupByDateMenuItem.Available =
					QuarterYearMenuItem.Available =
					MonthYearMenuItem.Available =
					GroupByYearMenuItem.Available = true;
				}
			}

			else if (mc.DataType == MetaColumnType.Structure) // no grouping, count only for summary for now
			{
				InitMenuItems();

				if (includeGroupingItems)
				{
					//GroupByHeaderMenuItem.Available =
					//AggregationSeparator1.Available = true;

					includeGroupingItems = false;	// todo, grouping on frag
				}

				if (includeSummaryItems) 
				{
					SummarizationHeaderMenuItem.Available =
					CountMenuItem.Available =
					AggregationSeparator2.Available = true; 
				}
			}

			else if (mc.DataType == MetaColumnType.Image) // no grouping, count only for summary
			{
				InitMenuItems();

				if (includeGroupingItems)
				{
					includeGroupingItems = false; // not allowed
				}

				if (includeSummaryItems) // summary types allowed for all data types
				{
					SummarizationHeaderMenuItem.Available =
					CountMenuItem.Available =
					AggregationSeparator2.Available = true;
				}
			}

			else // other types
			{
				if (includeGroupingItems)
					MatchingValuesMenuItem.Available = true;

				if (includeSummaryItems)
					NumberQualifierMenuItem.Available = Qc.IsPotentialNumberQualifier;
				//SingleValueMenuItem.Available = true; // (Not currently used)
			}

			if (!includeGroupingItems || !includeSummaryItems)
				AggregationSeparator1.Available = AggregationSeparator2.Available = false;

			//  Checkmark current selection

			string typeName = ad.TypeName;

			foreach (ToolStripItem tsi in AggTypeMenu.Items)
			{
				ToolStripMenuItem tsmi = tsi as ToolStripMenuItem;
				if (tsmi == null) continue; // separator of other non-ToolStripMenuItem

				string menuItemTypeName = tsmi.Tag != null ? tsmi.Tag.ToString() : "<null>";
				if (Lex.Eq(menuItemTypeName, typeName))
					tsmi.Checked = true;
				else tsmi.Checked = false;
			}

			return AggTypeMenu;
		}

/// <summary>
/// Set all items except "None" to unavailable & add click event
/// </summary>

		void InitMenuItems()
		{
			foreach (ToolStripItem tsi in AggTypeMenu.Items) // disable & init items
			{
				tsi.Available = false; // hide all initially

				if (!AggregationTypeMenuInitialized)
				{
					ToolStripMenuItem tsmi = tsi as ToolStripMenuItem;
					if (tsmi != null) // set handler for real menu item
						tsmi.Click += new System.EventHandler(AggregationTypeMenuItem_Click);
				}
			}

			AggregationTypeMenuInitialized = true;

			AggNoneMenuItem.Available = true; // Always include None option
			return;
		}

		/// <summary>
		/// Process aggregation item click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AggregationTypeMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripItem mi = sender as ToolStripItem;
			if (mi.Tag == null) return; // tag should contain internal AggregationType.Name

			string aggTypeName = mi.Tag.ToString();
			AggregationTypeDetail atd =  AggregationTypeDetail.GetByTypeName(aggTypeName);
			if (atd == null) throw new Exception("Unrecognized aggregation type: " + aggTypeName);

			if (Qc.MetaColumn == null) return;

			if (Qc.MetaColumn.DataType == MetaColumnType.Structure ||
				Qc.MetaColumn.DataType == MetaColumnType.Image)
			{
				bool allowed = ((atd.Role == AggregationRole.DataSummary && atd.SummaryTypeId == SummaryTypeEnum.Count)
					|| atd.Role == AggregationRole.Undefined);
				if (!allowed)
				{
					MessageBoxMx.ShowError("Aggregation is not supported for this column");
					return;
				}
			}

			if (atd.GroupingType == GroupingTypeEnum.NumericInterval) // prompt for numeric interval
			{
				DialogResult dr = NumericIntervalDialog.ShowDialog(AggregationDef, UIMisc.MousePosition);
				if (dr != DialogResult.OK) return;
			}

			AggregationDef.SetFromTypeName(aggTypeName);

			if (AggregationChangedDelegate != null) AggregationChangedDelegate(this);

			return;
		}

		private void ColumnAreaMenuItem_Click(object sender, EventArgs e)
		{
			ChangeRole(AggregationRole.ColumnGrouping);
		}

		private void RowAreaMenuItem_Click(object sender, EventArgs e)
		{
			ChangeRole(AggregationRole.RowGrouping);
		}

		private void DataAreaMenuItem_Click(object sender, EventArgs e)
		{
			ChangeRole(AggregationRole.DataSummary);
		}

		private void FilterAreaMenuItem_Click(object sender, EventArgs e)
		{
			ChangeRole(AggregationRole.Filtering);
		}

		private void UnassignedAreaMenuItem_Click(object sender, EventArgs e)
		{
			ChangeRole(AggregationRole.Undefined);
		}

		private void ChangeRole(AggregationRole role)
		{
			AggregationDef.Role = role; // set the role for Mobius
			AggregationDef.SetDefaultTypeIfUndefined(Qc.MetaColumn); // and default type if needed

			if (AggregationChangedDelegate != null) AggregationChangedDelegate(this);

			return;
		}

	}

	/// <summary>
	/// Callback delegate for aggregation type changed
	/// </summary>
	/// <param name="atm"></param>
	
	public delegate void AggregationTypeChangedDelegate(AggregationDefMenus ats);

}
