using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.ClientComponents;

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

	public partial class ColumnsSelectorDropDown : DevExpress.XtraEditors.XtraUserControl
	{
		ParsedColumnExpressionMsx ParsedColExpr; // col expression we are working on
		AxisMsx Axis; // axis we are working with
		VisualMsx Visual; // visual we are working with

		DropDownButton InvokingButton; // button that is causing this dropdown to be created
		ColumnsSelector PC = null; // parent control

		public new event EventHandler Click; // event to fire when control is clicked

		SpotfireViewManager SVM; // view manager associated with this dialog
		SpotfireApiClient Api => SVM?.SpotfireApiClient;
		Query ViewQuery => SVM.BaseQuery; // query associated with view (same as DmQuery?)
		Query BaseQuery => SVM.BaseQuery;
		SpotfireViewProps SVP => SVM?.SVP;
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps;
		DataTableMapMsx DataMap => DataTableMaps?.CurrentMap; // associated DataMap
		Query DmQuery { get { return DataMap?.Query; } } // query associated with datamap (same as ViewQuery?)
		ColumnMapCollection ColumnMap => DataMap?.ColumnMapCollection;

		public HashSet<MetaColumnType> ExcludedDataTypes { get { return _excludedDataTypes; } } // dict of excluded data types
		HashSet<MetaColumnType> _excludedDataTypes = new HashSet<MetaColumnType>();

		bool InSetup = false;
		public event EventHandler CallerEditValueChanged; // event to fire when edit value changes

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ColumnsSelectorDropDown()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup the dropdown for a Column(s) expression
		/// </summary>
		/// <param name="parsedColExpr"></param>
		/// <param name="axis"></param>
		/// <param name="visual"></param>
		/// <param name="b"></param>
		/// <param name="parentColumnExressionSelectorControl"></param>
		/// <param name="callerEditValueChanged"></param>

		public void Setup(
			ParsedColumnExpressionMsx parsedColExpr,
			AxisMsx axis,
			VisualMsx visual, 
			DropDownButton b,
			ColumnsSelector parentColumnExressionSelectorControl,
			EventHandler callerEditValueChanged = null)
		{
			InSetup = true;

			ParsedColExpr = parsedColExpr;
			Axis = axis;
			Visual = visual;

			InvokingButton = b;
			PC = parentColumnExressionSelectorControl;
			SVM = PC.SVM;
			CallerEditValueChanged = callerEditValueChanged;

			DataTableMsx dt = GetSelectorDataTable();
			if (dt != null) TableSelector.Name = dt.Name;
			else TableSelector.Name = "";

			List<DataTableMsx> tables = Axis.GetAllowedDataTables();

			ColumnList.Items.Clear();

			string selectedColName = null;
			if (parsedColExpr.ColumnNames.Count > 0)
				selectedColName = parsedColExpr.ColumnNames[0];

			foreach (DataColumnMsx col in dt.Columns)
			{
				CheckedListBoxItem item = new CheckedListBoxItem();
				item.Description = col.Name;
				if (Lex.Eq(col.Name, selectedColName))
					item.CheckState = CheckState.Checked;

				ColumnList.Items.Add(item);
			}

			//DataMapControl.ShowSelectedColumnCheckBoxes = true;
			//DataMapControl.SelectSingleColumn = true;
			//DataMapControl.ShowTableControls = false;
			//DataMapControl.Setup(SVM, DataMapSelectedColumnChanged);

			////DataMapControl.MobiusTableNameCol.Visible = false;
			////DataMapControl.MobiusColNameCol.Visible = false;

			//DataMapControl.FieldGridView.OptionsView.ColumnAutoWidth = false;
			////DataMapControl.FieldGridView.OptionsView.ShowColumnHeaders = false;

			InSetup = false;

			return;
		}

/// <summary>
/// Get the DataTable associated with the ColSelection
/// </summary>
/// <returns></returns>
/// 
		DataTableMsx GetSelectorDataTable()
		{
			return null;
		}

		/// <summary>
		/// Currently selected QueryColumn
		/// </summary>

		public QueryColumn QueryColumn
		{
			get // return the QueryColumn corresponding to the SelectedMetaColumn
			{
				QueryColumn qc = null;
				// todo...
				//if (Query == null || MetaColumn == null) return null;
				//MetaTable mt = MetaColumn.MetaTable;
				//QueryTable qt = Query.GetQueryTableByName(mt.Name);
				//if (qt == null) // if not in query create a temp query table so we can return a column
				//	qt = new QueryTable(mt);
				//qc = qt.GetQueryColumnByName(MetaColumn.Name);
				return qc;
			}
		}

		/// <summary>
		/// Currently selected MetaColumn
		/// </summary>

		public MetaColumn MetaColumn
		{
			get { return _MetaColumn; }

			set
			{
				_MetaColumn = value;
				if (value == null)
				{
					//ColumnName.Text = "";
				}

				else
				{
					//ColumnName.Text = value.Label;
				}
			}
		}
		MetaColumn _MetaColumn = null;

		/// <summary>
		/// Get the changed column expression and update the AxisExpression
		/// </summary>

		void DataMapSelectedColumnChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			ColumnMapCollection cml = null; // DataMapControl?.CurrentMap?.ColumnMapCollection?.GetSelectedColumnMapList();

			if (cml?.ColumnMapList.Count == 0) return; // ???
			else if (cml.ColumnMapList.Count > 1) cml = cml; // more than one col

			ColumnMapMsx cm = cml.ColumnMapList[0]; // new selected column

			if (ParsedColExpr.ColumnNames.Count == 0 && ParsedColExpr.AxisExpression != null) // if no cols then add button
			{
				ParsedColExpr.AxisExpression.ColExprList.Add(ParsedColExpr); // add to expression list
			}

			ParsedColExpr.ColumnNames = new List<string>();
			ParsedColExpr.ColumnNames.Add(cm.SpotfireColumnName);
			ParsedColExpr.Expression = ParsedColExpr.Format();
			if (ParsedColExpr.AxisExpression != null)
				ParsedColExpr.AxisExpression.Expression = ParsedColExpr.AxisExpression.Format();

			PC.SetupLayoutPanel();

			EditValueChanged();
		}

		/// <summary>
		/// Value of some form field has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (CallerEditValueChanged != null) // fire EditValueChanged event if handler is defined
				CallerEditValueChanged(this, EventArgs.Empty);
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{

		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{

		}

	} // class end

}
