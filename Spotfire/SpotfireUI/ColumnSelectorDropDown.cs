using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Select a DataTable, Column and optional aggregation for a scale or grouping
	/// For advanced use may also be a general expression with multiple columns and functions
	/// </summary>

	public partial class ColumnSelectorDropDown : DevExpress.XtraEditors.XtraUserControl
	{
		public ParsedColumnExpressionMsx ParsedColExpr; // parsed version of the expression including any changes made here
		List<DataTableMsx> AllowedDataTables = new List<DataTableMsx>();
		DataTableMsx MainVisualTable; // main table for Visual

		string SelectedColumnLabel => ParsedColExpr.GetColumnLabel(); // can be column name or pseudo column name
		DataColumnMsx SelectedDataColumn => ParsedColExpr.GetDataColumn(MainVisualTable, AllowedDataTables);
		DataTableMsx SelectedDataTable => SelectedDataColumn?.DataTable;

		DataTableMsx TentativelySelectedDataTable = null; // if just switched to new table without selecting a column yet

		List<string> CurrentAggList = null;
		string SelectedAggName = null;

		DropDownButton InvokingButton; // button that is causing this dropdown to be created
		ColumnSelectorControl PC = null; // parent control

		public new event EventHandler Click; // event to fire when control is clicked

		SpotfireViewProps SVP; // associated Spotfire View Properties
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps;
		DataTableMapMsx DataMap => DataTableMaps?.CurrentMap; // associated DataMap
		Query DmQuery { get { return DataMap?.Query; } } // query associated with datamap (same as ViewQuery?)
		ColumnMapCollection ColumnMap => DataMap?.ColumnMapCollection;

		public HashSet<MetaColumnType> ExcludedDataTypes { get { return _excludedDataTypes; } } // dict of excluded data types
		HashSet<MetaColumnType> _excludedDataTypes = new HashSet<MetaColumnType>();

		public event EventHandler<string> CallerEventHandler; // event callback

		bool InSetup { get => (SetupLevel > 0); set => SetupLevel = (value == true ? SetupLevel + 1 : SetupLevel - 1); }
		int SetupLevel = 0;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ColumnSelectorDropDown()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			//AggregationListPopupContainerControl.Dock = DockStyle.Fill;
		}

		/// <summary>
		/// Setup the dropdown for a Column(s) expression
		/// </summary>
		/// <param name="parsedColExpr"></param>
		/// <param name="visual"></param>
		/// <param name="allowedDataTables"></param>
		/// <param name="b"></param>
		/// <param name="parentColumnExressionSelectorControl"></param>
		/// <param name="callerEventHandler"></param>

		public void Setup(
			ParsedColumnExpressionMsx parsedColExpr,
			List<DataTableMsx> allowedDataTables,
			DataTableMsx mainVisualTable,
			DropDownButton b,
			ColumnSelectorControl parentColumnExressionSelectorControl,
			EventHandler<string> callerEventHandler = null)
		{
			InSetup = true;

			try
			{
				string colName, tableName;
				ParsedColExpr = parsedColExpr;
				AllowedDataTables = allowedDataTables;
				MainVisualTable = mainVisualTable;
				AllowedDataTables = allowedDataTables;

				InvokingButton = b;
				PC = parentColumnExressionSelectorControl;
				SVP = PC.SVP;
				CallerEventHandler = callerEventHandler;

				TableSelector.Setup(AllowedDataTables, SelectedDataColumn?.DataTable, SVP, DataTableChangedChangedEventHandler);

				SetupControls();

				return;
			}

			finally { InSetup = false; }
		}

		/// <summary>
		/// Setup all controls on this UserControl other than the TableSelector
		/// </summary>

		void SetupControls()
		{
			InSetup = true;

			try
			{

				SetupAggregationList();

				SetupColumnList();

				DisplayNameTextEdit.Text = ParsedColExpr.Alias;

				ExpressionTextBox.Text = ParsedColExpr.Expression;

				return;
			}

			finally { InSetup = false; }
		}

		/// <summary>
		/// Setup list of allowed columns

		void SetupAggregationList()
		{
			InSetup = true;

			try
			{
				string fullName = "";
				string shortName = "";

				DataTypeMsxEnum dataType = DataTypeMsxEnum.Undefined;

				if (SelectedDataColumn != null)
					dataType = SelectedDataColumn.DataType;

				List<string> list = Aggregation.GetAggregationListForDataType(dataType);

				if (list == null || list.Count == 0)
				{
					AggregationDropDownButton.Text = "No aggregation possible";
					AggregationDropDownButton.Enabled = false;
					return;
				}

				List<string> methods = ParsedColExpr.MethodNames;
				if (methods != null && methods.Count > 1) // only support a single method for now
				{
					AggregationDropDownButton.Text = "Expression uses multiple methods";
					AggregationDropDownButton.Enabled = false;
					return;
				}

				if (methods == null || methods.Count == 0)
					shortName = fullName = "(None)";

				else
				{
					shortName = methods[0];
					fullName = Aggregation.GetMatchingLongName(list, shortName);
					if (Lex.IsUndefined(fullName)) fullName = shortName; // just use short name if not found
				}

				AggregationDropDownButton.Text = shortName;
				AggregationDropDownButton.Enabled = true;

				CurrentAggList = new List<string>(list); // make a copy we can change
				CurrentAggList.Insert(0, "(None)");

				AggregationList.Items.Clear();
				AggregationSearchControl.Text = "";

				foreach (string s in CurrentAggList)
				{
					CheckedListBoxItem item = new CheckedListBoxItem();
					item.Description = s;
					item.Value = s;
					AggregationList.Items.Add(item);

					if (Lex.Eq(s, fullName))
					{
						item.CheckState = CheckState.Checked;
						AggregationList.SelectedItem = item;
					}

				}

				return;
			}

			finally { InSetup = false; }
		}

		private void AggregationList_ItemChecking(object sender, ItemCheckingEventArgs e)
		{
			if (InSetup) return;

			if (e.NewValue == CheckState.Unchecked) e.Cancel = true; // disallow unchecking
		}

		/// <summary>
		/// Aggregation list item checked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AggregationList_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
		{
			if (InSetup) return;

			CheckedListBoxItem item = AggregationList.Items[e.Index];

			if (item.CheckState != CheckState.Checked) return;

			string longName = item.Description;


			string shortName = Aggregation.GetShortName(longName);
			ParsedColExpr.AggregationMethod = shortName;

			SetupAggregationList();

			AggregationListPopupControlContainer.HidePopup();

			DelayedCallback.Schedule(EditValueChanged);
			return;
		}


		/// <summary>
		/// Aggregation selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AggregationList_SelectedIndexChanged(object sender, EventArgs e)
		{
			//if (InSetup) return;

			//string longName = "(None)";

			//int i = AggregationList.SelectedIndex;
			//if (i >= 0 && i < AggregationList.Items.Count)
			//{
			//	CheckedListBoxItem item = AggregationList.Items[AggregationList.SelectedIndex];
			//	longName = item.Description;
			//}

			//string shortName = Aggregation.GetShortName(longName);
			//ParsedColExpr.AggregationMethod = shortName;

			//SetupAggregationList();

			//AggregationListPopupControlContainer.HidePopup();

			//DelayedCallback.Schedule(EditValueChanged);
			return;
		}


		private void AggregationDropDownButton_Click(object sender, EventArgs e)
		{
			SimpleButton b = AggregationDropDownButton;
			Point p = PointToScreen(new Point(b.Location.X, b.Location.Y + b.Height));
			AggregationListPopupControlContainer.ShowPopup(p);
			AggregationSearchControl.Focus();
		}

		/// <summary>
		///  Setup list of allowed columns
		/// </summary>

		void SetupColumnList()
		{
			InSetup = true;

			try
			{
				ColumnList.Items.Clear();
				ColumnSearchControl.Text = "";

				DataTableMsx dt = SelectedDataTable;
				if (dt == null)
				{
					dt = MainVisualTable;
					if (dt == null) return;
				}

				string dcName = SelectedColumnLabel;

				if (TentativelySelectedDataTable != null)
				{
					dt = TentativelySelectedDataTable;
					dcName = ""; // nothing selected initially
				}

				foreach (DataColumnMsx col in dt.Columns)
				{
					CheckedListBoxItem item = new CheckedListBoxItem();
					item.Description = col.Name;
					item.Value = col;
					ColumnList.Items.Add(item);

					if (Lex.Eq(col.Name, dcName))
					{
						item.CheckState = CheckState.Checked;
						ColumnList.SelectedItem = item;
					}

				}

				return;
			}

			finally { InSetup = false; }
		}

		/// <summary>
		/// User has selected a new DataTable
		/// </summary>
		void DataTableChangedChangedEventHandler(object sender, EventArgs e)
		{
			if (InSetup) return;

			TentativelySelectedDataTable = TableSelector.SelectedDataTable; // tentative
			SetupControls(); // for tentative new table, no col selected
			return;
		}

		private void ColumnList_ItemChecking(object sender, ItemCheckingEventArgs e)
		{
			if (InSetup) return;

			if (e.NewValue == CheckState.Unchecked) e.Cancel = true; // disallow unchecking
		}

		/// <summary>
		/// Column  list item checked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ColumnList_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
		{
			if (InSetup) return;

			CheckedListBoxItem item = ColumnList.Items[e.Index];
			string colName = item.Description;
			string tableName = "";

			if (TentativelySelectedDataTable != null) // had we tentatively selected a new table?
			{
				tableName = TentativelySelectedDataTable.Name;
				ParsedColExpr.SetColumnAndTableName(colName, tableName);
				TentativelySelectedDataTable = null;
			}

			else // new col in current table
			{
				if (SelectedDataTable != MainVisualTable)
					tableName = SelectedDataTable.Name;

				ParsedColExpr.SetColumnAndTableName(colName, tableName);
			}

			ParsedColExpr.AggregationMethod = ""; // clear any existing aggregation

			SetupAggregationList(); // setup list for new columns

			DelayedCallback.Schedule(EditValueChanged);
			return;
		}

		/// <summary>
		/// New column selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ColumnList_SelectedIndexChanged(object sender, EventArgs e)
		{
			//if (InSetup) return;

			//CheckedListBoxItem item = ColumnList.Items[ColumnList.SelectedIndex];
			//string colName = item.Description;
			//string tableName = "";

			//if (TentativelySelectedDataTable != null) // had we tentatively selected a new table?
			//{
			//	tableName = TentativelySelectedDataTable.Name;
			//	ParsedColExpr.SetColumnAndTableName(colName, tableName);
			//	TentativelySelectedDataTable = null;
			//}

			//else // new col in current table
			//{
			//	if (SelectedDataTable != MainVisualTable)
			//		tableName = SelectedDataTable.Name;

			//	ParsedColExpr.SetColumnAndTableName(colName, tableName);
			//}

			//ParsedColExpr.AggregationMethod = ""; // clear any existing aggregation

			//SetupAggregationList(); // setup list for new columns

			//DelayedCallback.Schedule(EditValueChanged);

			return;
		}

		private void DisplayNameTextEdit_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			ParsedColExpr.Alias = DisplayNameTextEdit.Text;
			DelayedCallback.Schedule(EditValueChanged);

			return;
		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			if (CallerEventHandler != null)
				CallerEventHandler(this, "RemoveButtonClicked");

			return;
		}

		/// <summary>
		/// Value of some subcontrol on the user control has changed
		/// Update the 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void EditValueChanged()
		{
			if (InSetup) return;

			ParsedColExpr.Expression = ParsedColExpr.FormatEscapedExpression(); // update the full formatted expression
			ExpressionTextBox.Text = ParsedColExpr.Expression;

			if (CallerEventHandler != null) // fire EditValueChanged event if handler is defined
				CallerEventHandler(this, "EditValueChanged");
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			if (CallerEventHandler != null) 
				CallerEventHandler(this, "CloseButtonClicked");

			return;
		}

	} // ColumnSelectorDropDown

}
