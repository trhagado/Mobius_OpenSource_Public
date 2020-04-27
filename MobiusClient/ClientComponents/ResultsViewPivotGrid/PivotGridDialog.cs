using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;
using Mobius.SpotfireClient;

using DevExpress.XtraPivotGrid;
using DevExpress.XtraPivotGrid.Customization;
using DevExpress.XtraPivotGrid.Data;
using DevExpress.XtraPivotGrid.FilterDropDown;
using DevExpress.XtraPivotGrid.Frames;
using DevExpress.XtraPivotGrid.Printing;
using DevExpress.XtraPivotGrid.Selection;
using DevExpress.XtraPivotGrid.TypeConverters;
using DevExpress.XtraPivotGrid.Utils;
using DevExpress.XtraPivotGrid.ViewInfo;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

//using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using DevExpress.Data;
using DevExpress.Data.PivotGrid;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Show pivot table dialog
	/// </summary>

	public partial class PivotGridDialog : XtraForm
	{
		public static PivotGridDialog Instance;

		DataTable FieldGridDataTable = null; // DataTable of field properties

		internal PivotGridView PivotView; // the view associated with the dialog

		internal PivotGridPageControl PageControl { get { return PivotView.PageControl; } }
		internal PivotGridPanel PivotGridPanel { get { return PageControl.PivotGridPanel; } }
		internal PivotGridControlMx PivotGrid { get { return PivotGridPanel.PivotGrid; } } // the control
		//internal ChartControl Chart { get { return PivotGridPanel.Chart; } } // the control

		internal PivotGridFieldMx Field; // the current field that the menu is operating on in the pivot grid
		internal int FieldGridRow, FieldGridColumn; // Grid position setting value for or -1 if not from grid

		internal bool InSetup = false;

		// Tab page indexes

		static int Tpi = 0;
		static int GeneralTpi = Tpi++;

		string ReturnMessage = "";

		public PivotGridDialog()
		{
			InitializeComponent();
			Instance = this;

			InitChartTypeComboBox();
		}

		/// <summary>
		/// Show dialog & process results
		/// </summary>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			ViewManager view)
		{
			if (Instance == null) Instance = new PivotGridDialog();
			PivotGridDialog i = Instance;
			i.PivotView = view as PivotGridView;
			i.Setup();

			i.PivotGridPanel.UpdateViewWhenGridControlChanges = false; // suspend view updating
			try
			{
				DialogResult dr = i.ShowDialog(SessionManager.ActiveForm);

				i.PivotGrid.DestroyCustomization(); // explicitly hide the customization
				return dr;
			} 

			finally { i.PivotGridPanel.UpdateViewWhenGridControlChanges = true; } // restore immediate view updating
		}

		/// <summary>
		/// Adjust size and positions of main container controls
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void PivotGridDialog_Shown(object sender, EventArgs e)
		{
			TabPageSelector.Dock = DockStyle.Fill;

			Tabs.Dock = DockStyle.None;
			Tabs.Location = new Point(-1, -1);
			Tabs.Size = new Size(Panel2.Width + 5, Panel2.Height + 5);
			Tabs.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;
			Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));

			PopulateSelector(); // Fill the selector with all pages.

			PropertyDialogsUtil.SelectPropertyPage(Tabs.TabPages[0].Name, TabPageSelector, Tabs);

			return;
		}

		/// <summary>
		/// Populates the selector tree view.
		/// </summary>
		private void PopulateSelector()
		{
			this.TabPageSelector.Nodes.Clear();

			foreach (XtraTabPage page in Tabs.TabPages)
			{
				TreeNode node = new TreeNode(page.Text);
				node.Tag = page;
				this.TabPageSelector.Nodes.Add(node);
			}

			return;
		}

		private void TabPageSelector_AfterSelect(object sender, TreeViewEventArgs e)
		{
			XtraTabPage page = e.Node.Tag as XtraTabPage; // Get the page
			Tabs.SelectedTabPage = page;
		}

		private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (this.splitContainer.CanFocus)
			{
				this.splitContainer.ActiveControl = SelectorPanel;
			}
		}

		/// <summary>
		/// Copy values from view to form
		/// </summary>

		void Setup()
		{
			Mobius.Data.QueryColumn qc;
			string txt;
			int fi, fi2;

			InSetup = true;

			PivotGridView view = PivotView;

			// Setup General tab

			Title.Text = view.Title;
			Description.Text = view.Description;

			SetRowTotalsControlEnable();

			ShowColumnTotals.Checked = PivotGrid.OptionsView.ShowColumnTotals;
			ShowColumnGrandTotals.Checked = PivotGrid.OptionsView.ShowColumnGrandTotals;
			ShowRowTotals.Checked = PivotGrid.OptionsView.ShowRowTotals;
			ShowRowGrandTotals.Checked = PivotGrid.OptionsView.ShowRowGrandTotals;

			ChartType.Text = PivotGridPanel.ChartType;
			ChartShowSelectionOnly.Checked = PivotGrid.OptionsChartDataSource.SelectionOnly;
			ProvideDataByColumns.Checked = PivotGrid.OptionsChartDataSource.ProvideDataByColumns;
			//ChartShowPointLabels.Checked = DefaultBooleanMx.Convert(Chart.SeriesTemplate.LabelsVisibility);
			ProvideColumnGrandTotals.Checked = PivotGrid.OptionsChartDataSource.ProvideColumnGrandTotals;
			ProvideRowGrandTotals.Checked = PivotGrid.OptionsChartDataSource.ProvideRowGrandTotals;

			DataTable dt = CreateFieldGridDataTable(); // build the field grid

			PivotGridField[] Fields = new PivotGridField[PivotGrid.Fields.Count];
			for (fi = 0; fi < Fields.Length; fi++)
				Fields[fi] = PivotGrid.Fields[fi]; // copy ref

			for (fi = 1; fi < Fields.Length; fi++) // order fields by caption
			{
				PivotGridField f0 = Fields[fi];
				for (fi2 = fi - 1; fi2 >= 0; fi2--)
				{
					if (Lex.Le(Fields[fi2].Caption, Fields[fi].Caption)) break;
					Fields[fi2+1] = Fields[fi2]; // move down one
				}

				Fields[fi] = Fields[fi2 + 1];
			}

			for (fi = 0; fi < Fields.Length; fi++)
				PivotGrid.Fields[fi].Index = fi; // reorder indexes

			FillFieldDetailsGrid();

			PivotGrid.FieldsCustomization(); // integrate the Dx customization form

			InSetup = false;

			return;
		}

/// <summary>
/// CreateFieldGridDataTable
/// </summary>
/// <returns></returns>

		DataTable CreateFieldGridDataTable()
		{
			DataTable dt = new DataTable();
			FieldGridDataTable = dt; // save ref to table

			dt.Columns.Add("ColTypeImageCol", typeof(Image));
			dt.Columns.Add("PivotFieldCol", typeof(PivotGridField));
			dt.Columns.Add("FieldNameCol", typeof(string));
			dt.Columns.Add("AggRoleCol", typeof(string));
			dt.Columns.Add("AggTypeCol", typeof(string));
			//dt.Columns.Add("HeaderBinningCol", typeof(string));
			dt.Columns.Add("SourceColumnCol", typeof(string));
			dt.Columns.Add("SourceTableCol", typeof(string));

			return dt;
		}

		/// <summary>
/// Fill the grid with the current field details
/// </summary>

		void FillFieldDetailsGrid()
		{
			DataTable dt = FieldGridDataTable;
			dt.Clear();
			foreach (PivotGridFieldMx f0 in PivotGrid.Fields) // fill the grid
			{
				DataRow dr = dt.NewRow();
				SetFieldGridDataRow(dr, f0);
				dt.Rows.Add(dr);
			}

			FieldGrid.DataSource = dt;
			FieldGrid.Refresh();
		}

/// <summary>
/// SetFieldGridDataRow
/// </summary>
/// <param name="dr"></param>
/// <param name="f"></param>

		void SetFieldGridDataRow(DataRow dr, PivotGridFieldMx f)
		{
			string label, txt;

			ResultsField rfld = f.ResultsField as ResultsField;
			ResultsTable rt = rfld.ResultsTable;
			ResultsFormat rf = rt.ResultsFormat;
			MetaColumn mc = rfld.MetaColumn;
			QueryColumn qc = rfld.QueryColumn;
			QueryTable qt  = qc.QueryTable;
			Query q = qt.Query;

			dr["ColTypeImageCol"] = QueryTableControl.GetMetaColumnDataTypeImage(mc);
			dr["PivotFieldCol"] = f; // store reference to pivot field
			dr["FieldNameCol"] = f.Caption;

			dr["AggRoleCol"] = f.Aggregation.RoleLabel;

			dr["AggTypeCol"] = f.Aggregation.TypeLabel;

			//			txt = GroupingTypeLabel(f);
			//			dr["HeaderBinningCol"] = txt;

			dr["SourceColumnCol"] = PivotGridControlMx.BuildFieldCaption(f);

			label = qt.ActiveLabel;
			if (rf.Tables.Count > 1)
				label = "T" + (rt.Position + 1) + " - " + label;
			dr["SourceTableCol"] = label;

			return;
		}

		/////////////////////////////////////////////////////////
		///////////////////// General Tab ///////////////////////
		/////////////////////////////////////////////////////////

		private void Title_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			PivotView.Title = Title.Text;
			PivotView.UpdateContainerTitles();
		}

		private void Title_VisibleChanged(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == GeneralTpi) Title.Focus();
		}

		private void Description_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			PivotView.Description = Description.Text;
		}

		/////////////////////////////////////////////////////////
		///////////////////// General Tab ///////////////////////
		/////////////////////////////////////////////////////////

		private void CompactLayout_CheckedChanged(object sender, EventArgs e)
		{
			if (!CompactLayout.Checked) return;
			if (InSetup) return;

			PivotGrid.OptionsView.ShowRowTotals = true;
			PivotGrid.OptionsView.ShowTotalsForSingleValues = true;
			PivotGrid.OptionsView.RowTotalsLocation = PivotRowTotalsLocation.Tree;
			PivotGrid.OptionsBehavior.HorizontalScrolling = PivotGridScrolling.CellsArea;
		}

		/////////////////////////////////////////////////////////
		///////////////// Layout Options Tab ////////////////////
		/////////////////////////////////////////////////////////

		private void FullLayout_CheckedChanged(object sender, EventArgs e)
		{
			if (!FullLayout.Checked) return;
			if (InSetup) return;

			PivotGrid.OptionsView.RowTotalsLocation = PivotRowTotalsLocation.Far;
			PivotGrid.OptionsBehavior.HorizontalScrolling = PivotGridScrolling.Control;
			SetRowTotalsControlEnable();
		}

		void SetRowTotalsControlEnable()
		{
			if (PivotGrid.OptionsView.RowTotalsLocation == PivotRowTotalsLocation.Tree)
			{ // must show totals in this case
				ShowRowTotals.Checked = true;
				ShowRowTotals.Enabled = false;
			}
			else
			{
				ShowRowTotals.Enabled = true;
			}
		}

		private void ShowColumnTotals_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			PivotGrid.OptionsView.ShowColumnTotals = ShowColumnTotals.Checked;
		}

		private void ShowColumnGrandTotals_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			PivotGrid.OptionsView.ShowColumnGrandTotals = ShowColumnGrandTotals.Checked;
		}

		private void ShowRowTotals_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			PivotGrid.OptionsView.ShowRowTotals = ShowRowTotals.Checked;
		}

		private void ShowRowGrandTotals_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			PivotGrid.OptionsView.ShowRowGrandTotals = ShowRowGrandTotals.Checked;
		}

		////////////////////////////////////////////////////////
		///////////////// Chart Options Tab ////////////////////
		////////////////////////////////////////////////////////

		/// <summary>
		/// InitChartTypeComboBox
		/// </summary>

		void InitChartTypeComboBox()
		{
			Array arr;

			//ChartType.Properties.Items.AddRange(new DevExpress.XtraCharts.ViewType[] { 
			//	DevExpress.XtraCharts.ViewType.Area3D, 
			//	DevExpress.XtraCharts.ViewType.Bar, 
			//	DevExpress.XtraCharts.ViewType.Bar3D, 
			//	DevExpress.XtraCharts.ViewType.Doughnut, 
			//	DevExpress.XtraCharts.ViewType.Doughnut3D, 
			//	DevExpress.XtraCharts.ViewType.FullStackedArea, 
			//	DevExpress.XtraCharts.ViewType.FullStackedArea3D, 
			//	DevExpress.XtraCharts.ViewType.FullStackedBar, 
			//	DevExpress.XtraCharts.ViewType.FullStackedBar3D, 
			//	DevExpress.XtraCharts.ViewType.FullStackedSplineArea, 
			//	DevExpress.XtraCharts.ViewType.FullStackedSplineArea3D, 
			//	DevExpress.XtraCharts.ViewType.Line, 
			//	DevExpress.XtraCharts.ViewType.Line3D,
			//	DevExpress.XtraCharts.ViewType.ManhattanBar, 
			//	DevExpress.XtraCharts.ViewType.Pie, 
			//	DevExpress.XtraCharts.ViewType.Pie3D, 
			//	DevExpress.XtraCharts.ViewType.Point, 
			//	DevExpress.XtraCharts.ViewType.SideBySideFullStackedBar,
			//	DevExpress.XtraCharts.ViewType.SideBySideFullStackedBar3D, 
			//	DevExpress.XtraCharts.ViewType.SideBySideStackedBar, 
			//	DevExpress.XtraCharts.ViewType.SideBySideStackedBar3D, 
			//	DevExpress.XtraCharts.ViewType.Spline, 
			//	DevExpress.XtraCharts.ViewType.Spline3D,
			//	DevExpress.XtraCharts.ViewType.SplineArea, 
			//	DevExpress.XtraCharts.ViewType.SplineArea3D, 
			//	DevExpress.XtraCharts.ViewType.StackedArea, 
			//	DevExpress.XtraCharts.ViewType.StackedArea3D, 
			//	DevExpress.XtraCharts.ViewType.StackedBar, 
			//	DevExpress.XtraCharts.ViewType.StackedBar3D, 
			//	DevExpress.XtraCharts.ViewType.StackedSplineArea, 
			//	DevExpress.XtraCharts.ViewType.StackedSplineArea3D,
			//	DevExpress.XtraCharts.ViewType.StepLine});

			ChartType.Properties.Items.Add("None");

			return;
		}

		private void comboChartType_SelectedIndexChanged(object sender, EventArgs e)
		{
			PivotGridPanel.ChartType = ChartType.SelectedItem.ToString();
		}

		private void ceChartDataVertical_CheckedChanged(object sender, EventArgs e)
		{
			PivotGrid.OptionsChartDataSource.ProvideDataByColumns = ProvideDataByColumns.Checked;
		}

		private void ceSelectionOnly_CheckedChanged(object sender, EventArgs e)
		{
			PivotGrid.OptionsChartDataSource.SelectionOnly = ChartShowSelectionOnly.Checked;
		}

		private void checkShowPointLabels_CheckedChanged(object sender, EventArgs e)
		{
			//Chart.SeriesTemplate.LabelsVisibility = DefaultBooleanMx.Convert(ChartShowPointLabels.Checked);
		}

		private void ceShowColumnGrandTotals_CheckedChanged(object sender, EventArgs e)
		{
			PivotGrid.OptionsChartDataSource.ProvideColumnGrandTotals = ProvideColumnGrandTotals.Checked;
		}

		private void ceShowRowGrandTotals_CheckedChanged(object sender, EventArgs e)
		{
			PivotGrid.OptionsChartDataSource.ProvideRowGrandTotals = ProvideRowGrandTotals.Checked;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			CopyGridPropsToView();

			DialogResult = DialogResult.OK;
		}

/// <summary>
/// Copy grid/chart control properties to the view
/// </summary>

		void CopyGridPropsToView()
		{
			PivotGridPropertiesMx p = PivotView.PivotGridPropertiesMx;
			if (p == null) return;

			PivotView.UpdateViewFieldsFromGridFields();

			p.CompactLayout = 
				(PivotGrid.OptionsView.RowTotalsLocation == PivotRowTotalsLocation.Tree);

			p.ShowColumnTotals = PivotGrid.OptionsView.ShowColumnTotals;
			p.ShowColumnGrandTotals = PivotGrid.OptionsView.ShowColumnGrandTotals;
			p.ShowRowTotals = PivotGrid.OptionsView.ShowRowTotals;
			p.ShowRowGrandTotals = PivotGrid.OptionsView.ShowRowGrandTotals;
			p.ShowFilterHeaders = PivotGrid.OptionsView.ShowFilterHeaders;

			p.PivotGridChartType = PivotGridPanel.ChartType;
			p.PgcShowSelectionOnly = PivotGrid.OptionsChartDataSource.SelectionOnly;
			p.PgcProvideDataByColumns = PivotGrid.OptionsChartDataSource.ProvideDataByColumns;
			//p.PgcShowPointLabels = DefaultBooleanMx.Convert(Chart.SeriesTemplate.LabelsVisibility);
			p.PgcShowColumnGrandTotals = PivotGrid.OptionsChartDataSource.ProvideColumnGrandTotals;
			p.PgcShowRowGrandTotals = PivotGrid.OptionsChartDataSource.ProvideRowGrandTotals;

			return;
		}

		/// <summary>
		/// Cancel and restore previous state
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Cancel_Click(object sender, EventArgs e)
		{
			PivotView.ConfigureRenderingControl(); // reconfigure from original view settings
			return;
		}

		private void PivotGridDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
				Cancel_Click(null, null);
		}

		private void PivotGridDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			return;
		}

/// <summary>
/// Aggregation role changed
/// </summary>
/// <param name="ats"></param>

		private void AggregationRoleChanged(AggregationDefMenus ats)
		{
			FieldGridView.SetRowCellValue(FieldGridRow, "AggRoleCol", Field.Aggregation.RoleLabel);
			FieldGridView.SetRowCellValue(FieldGridRow, "AggTypeCol", Field.Aggregation.TypeLabel);
			Field.SyncDxAreaToMxRole(); // sync Dx area
			PivotGrid.RefreshData(); // refresh the PivotGrid to show new agg type results
		}

		/// <summary>
		/// Store selected group interval type
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AggregationTypeChanged(AggregationDefMenus ats)
		{
			FieldGridView.SetRowCellValue(FieldGridRow, "AggTypeCol", Field.Aggregation.TypeLabel);
			Field.SyncDxAreaToMxRole(); // sync Dx area
			PivotGrid.RefreshData(); // refresh the PivotGrid to show new agg type results

			//PivotGrid.BeginUpdate();
			//object ds = PivotGrid.DataSource;
			//PivotGrid.DataSource = null;
			// -make changes
			//PivotGrid.DataSource = ds;
			//PivotGrid.EndUpdate();

		}

		//void SetMeasureUnits(params DateTimeMeasureUnit[] units)
		//{
		//	object prevUnit = String.IsNullOrEmpty(ChartDataMeasureUnit.SelectedItem.ToString()) ? null : Enum.Parse(typeof(DateTimeMeasureUnit), ChartDataMeasureUnit.SelectedItem.ToString());
		//	string prevItem = "";
		//	ChartDataMeasureUnit.Properties.Items.Clear();
		//	foreach (DateTimeMeasureUnit unit in units)
		//	{
		//		string unitName = Enum.GetName(typeof(DateTimeMeasureUnit), unit);
		//		ChartDataMeasureUnit.Properties.Items.Add(unitName);
		//		if (prevUnit != null && object.Equals(unit, (DateTimeMeasureUnit)prevUnit))
		//			prevItem = unitName;
		//	}
		//	if (!String.IsNullOrEmpty(prevItem))
		//		ChartDataMeasureUnit.SelectedItem = prevItem;
		//	else
		//		ChartDataMeasureUnit.SelectedIndex = 0;
		//}


		//private void cbChartDataMeasureUnit_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	XYDiagram diagram = null; // (XYDiagram)Chart.Diagram;
		//	DateTimeMeasureUnit unit = (DateTimeMeasureUnit)Enum.Parse(typeof(DateTimeMeasureUnit), ChartDataMeasureUnit.SelectedItem.ToString());
		//	diagram.AxisX.DateTimeScaleOptions.GridAlignment = (DateTimeGridAlignment)unit;
		//	diagram.AxisX.DateTimeScaleOptions.MeasureUnit = unit;
		//	switch (unit)
		//	{
		//		case DateTimeMeasureUnit.Year:
		//			diagram.AxisX.Label.TextPattern = "{A:yyyy}";
		//			break;
		//		case DateTimeMeasureUnit.Quarter:
		//			diagram.AxisX.Label.TextPattern = "{A:yyyy}"; // todo: fix
		//			//diagram.AxisX.Label.DateTimeOptions.Format = DateTimeFormat.QuarterAndYear;
		//			break;
		//		case DateTimeMeasureUnit.Month:
		//			diagram.AxisX.Label.TextPattern = "{A:yyyy}"; // todo: fix
		//			//diagram.AxisX.DateTimeOptions.Format = DateTimeFormat.MonthAndYear;
		//			break;
		//		default:
		//			break;
		//	}
		//}

/// <summary>
/// Field grid cell clicked
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void FieldGrid_MouseClick(object sender, MouseEventArgs e)
		{
			GridHitInfo hi = FieldGridView.CalcHitInfo(e.Location);
			int ri = hi.RowHandle;
			if (ri < 0) return;

			PivotGridFieldMx f = Field = PivotGrid.Fields[ri] as PivotGridFieldMx;
			ResultsField rfld = f.ResultsField as ResultsField;
			AggregationDef agg = f.Aggregation;

			Mobius.Data.QueryColumn qc = rfld.QueryColumn;
			MetaColumn mc = qc.MetaColumn;

			GridColumn gc = hi.Column;
			if (gc == null) return;

			FieldGridRow = hi.RowHandle;
			FieldGridColumn = gc.AbsoluteIndex;

			GridViewInfo viewInfo = (GridViewInfo)FieldGridView.GetViewInfo();
			GridCellInfo cellInfo = viewInfo.GetGridCellInfo(hi);
			Point menuLoc = new Point(cellInfo.Bounds.Left, cellInfo.Bounds.Bottom);
			menuLoc = FieldGrid.PointToScreen(menuLoc);

			if (gc.FieldName == "AggRoleCol") // show appropriate aggregation type menu
			{
				AggregationDefMenus.ShowRoleMenu(qc, f.Aggregation, menuLoc, AggregationRoleChanged);
				return;
			}


			else if (gc.FieldName == "AggTypeCol") // show appropriate aggregation type menu
			{
				AggregationDefMenus.ShowTypeMenu(qc, f.Aggregation, menuLoc, AggregationTypeChanged);
				return;
			}

			else if (gc.FieldName == "SourceColumnCol" || gc.FieldName == "SourceTableCol")
			{
				return;
			}
		}

		/// <summary>
		/// Value tentatively changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldGridView_CellValueChanging(object sender, CellValueChangedEventArgs e)
		{
			int ri = e.RowHandle;
			if (ri < 0) return;
			DataRow dr = FieldGridDataTable.Rows[ri]; 

			GridColumn gc = e.Column;
			if (gc == null) return;
			int gci = gc.AbsoluteIndex;

			PivotGridFieldMx f = PivotGrid.Fields[ri] as PivotGridFieldMx;
			ResultsField rfld = f.ResultsField as ResultsField;
			Mobius.Data.QueryColumn qc = rfld.QueryColumn;
			MetaColumn mc = qc.MetaColumn;

			if (gc.FieldName == "FieldNameCol")
			{
				return; // wait til move out of cell
			}

			else if (gc.FieldName == "AggRoleCol")
			{
				//e=e;
				//object o = e.Value;
				//DataRow dr2 = FieldGridDataTable.Rows[e.RowHandle];
				//object vo = dr2["AggRoleCol"];
				//FieldGridDataTable

				f.SyncDxAreaToMxRole(); // sync Dx area
				PivotGrid.RefreshData(); // refresh the PivotGrid to show new agg type results

				//dr["AggRoleCol"] = f.Aggregation.TypeLabel;
				return; // throw new NotImplementedException();

				//string txt = e.Value.ToString();
				//if (Lex.IsNullOrEmpty(txt) || Lex.Eq(txt, "None"))
				//{
				//	f.Visible = false;

				//	dr["SummaryTypeCol"] = "";
				//	//dr["HeaderBinningCol"] = "";
				//}
				//else
				//{
				//	f.Area = (PivotArea)Enum.Parse(typeof(PivotArea), txt);
				//	f.Visible = true;

				//	dr["SummaryTypeCol"] = SummaryTypeLabel(f);
				//	//dr["HeaderBinningCol"] = GroupingTypeLabel(f);
				//}

				//PivotGrid.SetFilterHeaderVisibility();
			}

			else if (gc.FieldName == "AggTypeCol")
			{
				return;
			}

			//else if (gc.FieldName == "HeaderBinningCol")
			//{
			//	return;
			//}

			else if (gc.FieldName == "SourceColumnCol" || gc.FieldName == "SourceTableCol")
			{
				return;
			}

			else return; // something else

		}

		/// <summary>
/// User has changed the value of a cell
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void FieldGridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
		{
			int ri = e.RowHandle;
			if (ri < 0) return;

			GridColumn gc = e.Column;
			if (gc == null) return;
			int gci = gc.AbsoluteIndex;

			PivotGridFieldMx f = PivotGrid.Fields[ri] as PivotGridFieldMx;
			ResultsField rfld = f.ResultsField as ResultsField;
			Mobius.Data.QueryColumn qc = rfld.QueryColumn;
			MetaColumn mc = qc.MetaColumn;

			if (gc.FieldName == "FieldNameCol")
			{
				f.Caption = e.Value.ToString();
			}

			else if (gc.FieldName == "AggRoleCol")
			{
				return; // handled in CellValueChanging
			}

			else if (gc.FieldName == "AggTypeCol")
			{
				return;
			}

			//else if (gc.FieldName == "HeaderBinningCol")
			//{
			//	return;
			//}

			else if (gc.FieldName == "SourceColumnCol" || gc.FieldName == "SourceTableCol")
			{
				return;
			}

			else return; // something else
		}

		private void FieldGrid_VisibleChanged(object sender, EventArgs e)
		{
			if (!FieldGrid.Visible) return;
			FillFieldDetailsGrid(); // draw up-to-date version of the grid
		}

/// <summary>
/// Add a new pivot grid field
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void AddPivotField_Click(object sender, EventArgs e)
		{
			QueryManager qm = PivotView.Qm;
			Query q = qm.Query;
			FieldSelectorControl.Query = q;
			FieldSelectorControl.MetaColumn = null;
			Point p = AddPivotField.PointToScreen(new Point(0, AddPivotField.Height));
			Mobius.Data.QueryColumn qc = FieldSelectorControl.ShowMenu(p);
			if (qc == null) return;

			PivotGridPropertiesMx pp = PivotView.PivotGridPropertiesMx;

			ResultsField rfld = qm.ResultsFormat.GetResultsField(qc);
			if (rfld == null) return;

			PivotGridFieldMx field = // add new field (must add to base view)
				PivotGridView.AddField(rfld, pp.PivotFields, null, GroupingTypeEnum.EqualValues);

			PivotGridFieldMx field2 = new PivotGridFieldMx();
			field.CopyField(field2);
			pp.PivotFields.Remove(field); // remove from base view 

			PivotGrid.BeginUpdate();
			object ds = PivotGrid.DataSource;
			PivotGrid.DataSource = null;
			PivotGrid.Fields.Add(field2); // add to pivot grid
			PivotView.ConfigurePivotGridControlField(field2);
			PivotGrid.DataSource = ds;
			PivotGrid.EndUpdate();

			FillFieldDetailsGrid();
			return;
		}

		/// <summary>
		/// Duplicate an existing field
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DuplicatePivotField_Click(object sender, EventArgs e)
		{
			int[] rows = FieldGridView.GetSelectedRows();
			if (rows.Length == 0) return;

			PivotGrid.BeginUpdate();
			object ds = PivotGrid.DataSource;
			PivotGrid.DataSource = null;

			PivotGridFieldMx f = PivotGrid.Fields[rows[0]] as PivotGridFieldMx;
			if (f == null) return;

			PivotGridFieldMx f2 = new PivotGridFieldMx();
			f.CopyField(f2);
			PivotGrid.Fields.Add(f2);

			PivotGrid.DataSource = ds;
			PivotGrid.EndUpdate();

			FillFieldDetailsGrid();
			return;
		}

		/// <summary>
		/// Delete the pivot fields corresponding to the currently selected rows in the  field grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DeletePivotField_Click(object sender, EventArgs e)
		{
			int[] rows = FieldGridView.GetSelectedRows();
			if (rows.Length == 0) return;

			PivotGrid.BeginUpdate();
			object ds = PivotGrid.DataSource;
			PivotGrid.DataSource = null;

			for (int ri = rows.Length - 1; ri >= 0; ri--)
			{
				PivotGrid.Fields.RemoveAt(rows[ri]); // delete pivot field from pivot grid control
				FieldGridView.DeleteRow(rows[ri]); // also from the field grid
			}

			PivotGrid.DataSource = ds;
			PivotGrid.EndUpdate();
			
			return;
		}

	} // PivotGridDialog

}