using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

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
using DevExpress.Data.PivotGrid;
//using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.Utils.Menu;

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
	/// Panel containing the PivotGridControl derived from the DevExpress PivotGridControl
	/// </summary>
	
	public partial class PivotGridPanel : DevExpress.XtraEditors.XtraUserControl
	{
		internal PivotGridPageControl PageControl;  // the results page associated with the page panel
		internal PivotGridView View { get { return PageControl.View; } } // current PivotGridView for the page panel

		internal QueryManager Qm { get { return PivotGrid.View.Qm; } } // get QueryManager for the panel from the associated pivotgrid 

		internal DataTableManager Dtm
		{ get { return (Qm != null) ? Qm.DataTableManager : null; } }

		internal MoleculeGridControl MoleculeGrid // molecule grid for selected data rows
		{ get { return (Qm != null) ? Qm.MoleculeGrid : null; } }

		internal PivotGridFieldMx PivotGridField; // the current field that the menu is operating on in the pivot grid

		public bool UpdateViewWhenGridControlChanges = true; // if true update the view state when the GridControl is changed

		bool InSetup = false;
		
		/// <summary>
		/// Chart type property
		/// </summary>

		internal string ChartType
		{
			get
			{
				return _chartType;
			}

			set
			{
				_chartType = value;
				SetChartViewType(value); 
			}
		}
		private string _chartType = "None";


/// <summary>
/// Constructor
/// </summary>

		public PivotGridPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			// Link chart to grid & do initial setup

			//Chart.DataSource = PivotGrid;
			//Chart.SeriesDataMember = "Series";
			//Chart.SeriesTemplate.ArgumentDataMember = "Arguments";
			//Chart.SeriesTemplate.ValueDataMembers.AddRange(new string[] { "Values" });
			//Chart.SeriesTemplate.LegendTextPattern = "{} {S} - {A}: {V}";
			//// Chart.SeriesTemplate.LegendPointOptions.PointView = PointView.ArgumentAndValues;

			//if (!SystemUtil.InDesignMode) // fill panel with the chart control initially
			//{
			//	SplitContainerControl.PanelVisibility = SplitPanelVisibility.Panel1; // just show grid initially
			//	Chart.Visible = false;
			//	Chart.DataSource = null;
			//}

			return;
		}

/// <summary>
/// Get unbound data
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void PivotGridControl_CustomUnboundFieldData(object sender, DevExpress.XtraPivotGrid.CustomFieldDataEventArgs e)
		{
			MobiusDataType mdt = null;
			NumberMx nex;
			StringMx sex;
			DateTimeMx dex;
			QualifiedNumber qn;
			double dVal;
			string cid;
			int iVal, rti, dri, dri2;

			//int t0 = TimeOfDay.Milliseconds();

			if (PivotGrid.DataSource == null) return;

			PivotGridFieldContext fc = GetPivotGridFieldContext(e.Field);
			dri = e.ListSourceRowIndex;
			object vo = Dtm.GetColumnValueWithRowDuplication(fc.ResultsField, dri);
			if (vo != null)
			{
				Type voType = vo.GetType();
				if (MobiusDataType.IsMobiusDataType(vo))
				{
					if (vo is QualifiedNumber)
					{
						qn = vo as QualifiedNumber;
						if (!Lex.IsDefined(qn.Qualifier)) // if no qualifier then just use double NumberValue
							vo = qn.NumberValue;
					}

					else // otherwise convert to a primitive
					{
						vo = MobiusDataType.ConvertToPrimitiveValue(vo);
					}
				}

				if (vo is DateTime)
					vo = ((DateTime)vo).ToBinary(); // handle DateTimes as int64 (longs) to avoid performance hit in Dx code
			}

			e.Value = vo;

			return;
		}

/// <summary>
/// FieldAreaChanging
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_FieldAreaChanging(object sender, PivotAreaChangingEventArgs e)
		{
			return;
		}

		/// <summary>
		/// The area that a field is located in has changed.
		///  If changed by the DX UI then the added members in PivotGridFieldMx must be updated
		///  If changed by Mobius code (InSetup = true) then nothing needed here.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Grid_FieldAreaChanged(object sender, PivotFieldEventArgs e)
		{
			if (InSetup) return;

			try
			{
				InSetup = true;
				PivotGridFieldContext fc = GetPivotGridFieldContext(e.Field);
				if (fc == null) return;

				PivotGridFieldMx f = fc.F;

				f.SyncMxRoleToDxArea();

				ResultsField rfld = fc.ResultsField;
				if (rfld == null || rfld.MetaColumn == null) return;

				f.Aggregation.SetDefaultTypeIfUndefined(rfld.MetaColumn); // may need to set default type as well

				PivotGridControlMx.SetFieldCaption(f);

				PivotGrid.RefreshData(); // recalc summaries

				if (UpdateViewWhenGridControlChanges)
					View.UpdateViewFieldsFromGridFields();

				return;
			}

			finally
			{
				InSetup = false;
			}
		}

		/// <summary>
		/// Changed summary type
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PivotGridField_SummaryTypeChanged(object sender, EventArgs e)
		{
			PivotGridFieldMx f = sender as PivotGridFieldMx;
			return;
		}

		PivotGridField GetViewField(PivotGridField f)
		{
			PivotGridPropertiesMx p = View.PivotGridPropertiesMx;

			foreach (PivotGridField f0 in p.PivotFields)
			{
				if (f0.UnboundFieldName == f.UnboundFieldName) return f0;
			}

			throw new Exception("PivotGridField not found: " + f.UnboundFieldName);
		}

		/// <summary>
		/// Embed CustomizationForm in a PivotGridDialog.CustomizationFormPanel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Grid_ShowingCustomizationForm(object sender, CustomizationFormShowingEventArgs e)
		{
			if (InSetup) return;

			if (PivotGridDialog.Instance == null || !PivotGridDialog.Instance.InSetup) // activated from native DX menu not Mobius menu
			{
				DialogResult dr = PivotGridDialog.ShowDialog(View);
				return;
			}

			else // embed CustomizationForm in a PivotGridDialog.CustomizationFormPanel
			{
				e.ParentControl = PivotGridDialog.Instance.CustomizationFormPanel;
				e.CustomizationForm.Dock = DockStyle.Fill;
			}
		}

/// <summary>
/// Draw conditional formatting field value headers with appropriate background color
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_CustomDrawFieldValue(object sender, PivotCustomDrawFieldValueEventArgs e)
		{
			string txt;
			PivotGridFieldContext fc = GetPivotGridFieldContext(e.Field);
			if (fc == null) return;
			ResultsField rfld = fc.ResultsField;
			if (rfld == null) return;

			//DebugLog.Message(rfld.QueryColumn.MetaColumn.Name);

			//if (rfld.QueryColumn.MetaColumn.Name == "ACTIVITY_BIN") rfld = rfld; // debug
			//if (e.DisplayText == "0") e = e; // debug

			CondFormat cf = rfld.QueryColumn.CondFormat;
			if (cf == null || cf.Rules.Count == 0) return;

			foreach (CondFormatRule rule in cf.Rules)
			{ // look for rule name that matches field value text
				if (Lex.Eq(rule.Name, e.DisplayText) || Lex.Eq(rule.Value, e.DisplayText))
				{
					if (!Lex.IsNullOrEmpty(rule.Name)) txt = rule.Name; // display rule name if defined
					else txt = e.DisplayText; // otherwise display field value

					Rectangle rect = e.Bounds;
					ControlPaint.DrawBorder3D(e.Graphics, e.Bounds);
					Brush brush =
							e.GraphicsCache.GetSolidBrush(rule.BackColor1);
					rect.Inflate(-1, -1);
					e.Graphics.FillRectangle(brush, rect);
					e.Appearance.DrawString(e.GraphicsCache, txt, e.Info.CaptionRect);
					//e.Appearance.DrawString(e.GraphicsCache, e.Info.Caption, e.Info.CaptionRect);
					foreach (DevExpress.Utils.Drawing.DrawElementInfo info in e.Info.InnerElements)
					{
						DevExpress.Utils.Drawing.ObjectPainter.DrawObject(e.GraphicsCache, info.ElementPainter,
								info.ElementInfo);
					}
					e.Handled = true;
					return;

					//e.Painter.DrawObject(e.Info); // change tone of skin
					//e.Graphics.FillRectangle(e.GraphicsCache.GetSolidBrush(Color.FromArgb(50, 0, 0, 200)), e.Bounds);
					//e.Handled = true;
				}
			}

			return;
		}

/// <summary>
/// Display cell text as hyperlinks to allow drilldown 
/// Also, set the background color if appropriate (e.g. bin that an SP/CRC value is in)
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_CustomAppearance(object sender, PivotCustomAppearanceEventArgs e)
		{
			e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Underline);
			e.Appearance.ForeColor = Color.Blue;

			PivotGridFieldContext f = GetPivotGridFieldContext(e.DataField);
			if (f == null) return;

			ResultsField rfld = f.ResultsField;
			if (rfld == null) return;
			Mobius.Data.QueryColumn qc = rfld.QueryColumn;
			MetaColumn mc = qc.MetaColumn;

			CondFormat cf = f.Qc.CondFormat;
			//if (cf == null || cf.Rules.Count == 0) return;

			object cv = e.GetCellValue(e.ColumnIndex, e.RowIndex);
			double d = -1;
			if (cv is int)
			{
				d = (int)cv;
			}

			else if (cv is double)
			{
				d = (double)cv;
			}

			else return;

			if (Math.Abs(1) == 2) // for compound by target
			{
				if (d >= 1 && d <= 10)
				{ // todo: set proper color
					Color c = UnpivotedAssayResult.CalculateBinColor((int)d);
					e.Appearance.BackColor = c;
				}
			}

			if (DebugMx.False) // color by count
			{
				d = d / 45;
				if (d > 10) d = 10;
				if (d < 1) d = 1;
				if (d >= 1 && d <= 10)
				{ // todo: set proper color
					Color c = UnpivotedAssayResult.CalculateBinColor((int)d);
					e.Appearance.BackColor = c;
				}
			}

			if (DebugMx.False) // for bin by target
			{
				PivotGridField[] rowFields = e.GetRowFields();
				if (d > 0 && rowFields.Length > 0)
				{
					object rfv = e.GetFieldValue(rowFields[0]);
					if (rfv == null) return;
					
					else if (rfv is int)
					{
						Color c = UnpivotedAssayResult.CalculateBinColor((int)rfv);
						e.Appearance.BackColor = c;
					}
				}
			} 
		}

		/// <summary>
		/// Customize the display text for the cells displayed within the Data Area.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Grid_CustomCellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
		{
			string txt;

			QualifiedNumber qn = e.Value as QualifiedNumber; // summary values should be QNs
			if (qn == null) return;

			PivotGridFieldContext f = GetPivotGridFieldContext(e.DataField);

			AggregationTypeDetail atd = f.Aggregation.TypeDetail;
			if (atd == null || !atd.FractionalSummaryResult) return; // just return if not numeric type

			if (qn.IsNull)
			{
				e.DisplayText = "";
				return;
			}

			ColumnFormatEnum displayFormat = f.Qc.ActiveDisplayFormat;
			int decimals = f.Qc.ActiveDecimals;

			if (f.Mc.DataType == MetaColumnType.Integer)
			{
				displayFormat = ColumnFormatEnum.Decimal;
				decimals = 1;
			}

			txt = qn.Format(f.Qc, false, displayFormat, decimals, QnfEnum.Combined | QnfEnum.NValue, OutputDest.WinForms);
			e.DisplayText = txt;
			return;
		}

		/// <summary>
		/// Customize the display text of individual column and row headers and filter dropdown items
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Grid_FieldValueDisplayText(object sender, PivotFieldDisplayTextEventArgs e)
		{
			string txt;

			PivotGridValueType vt = e.ValueType; // Value, Total, GrantTotal, CustomTotal

			PivotGridFieldContext fc = GetPivotGridFieldContext(e.Field);
			if (fc == null) return;

			if (fc.Aggregation.IsGroupingType)
			{
				object vo = e.Value;
				txt = GroupingMethods.FormatGroupedValue(fc.Qc, fc.Aggregation, vo);
				e.DisplayText = txt;
			}

			else if (fc.Aggregation.Role == AggregationRole.DataSummary) // data area header when 2 or more data fields
			{
				txt = PivotGridControlMx.BuildFieldCaption(fc.F);
				e.DisplayText = txt;
			}

			else return;


			return;
		}

/// <summary>
/// Drill down into data
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_CellClick(object sender, PivotCellEventArgs e)
		{
			BuildAndRunDrillDownQuery(sender, e);
		}

/// <summary>
/// Drill down into data
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_CellDoubleClick(object sender, PivotCellEventArgs e)
		{
			BuildAndRunDrillDownQuery(sender, e);
		}

/// <summary>
/// Create and execute the drill down query
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		void BuildAndRunDrillDownQuery(object sender, PivotCellEventArgs e)
		{
			object o;

			PivotDrillDownDataSource ds = e.CreateDrillDownDataSource();
			if (ds.RowCount <= 0) return; // no data

			Query q2 = Qm.Query.Clone();
			q2.ShowGridCheckBoxes = false;

// Build name for drilldown from row and column fields values

			string txt = "";
			PivotGridField[] rf = e.GetRowFields();
			PivotGridField[] ca = e.GetColumnFields();
			Array.Resize(ref rf, rf.Length + ca.Length);
			Array.Copy(ca, 0, rf, rf.Length - ca.Length, ca.Length);

			foreach (PivotGridField f in rf)
			{
				o = e.GetFieldValue(f);
				if (o != null && o.ToString() != "")
				{
					if (txt != "") txt += ", ";
					txt += o.ToString();
				}
			}

			if (e.ColumnValueType == PivotGridValueType.GrandTotal ||
			 e.RowValueType == PivotGridValueType.GrandTotal)
				txt += " Grand Total";

			else if (e.ColumnValueType == PivotGridValueType.Total ||
			 e.RowValueType == PivotGridValueType.Total) 
				txt += " Total";

			q2.UserObject.Name = txt;

// Create DataTable containing drill down data

			DataTableMx dt2 = DataTableManager.BuildDataTable(q2);

			for (int ri = 0; ri < ds.RowCount; ri++)
			{ // copy rows over
				DataRowMx dr2 = dt2.NewRow();
				object[] vo = dr2.ItemArrayRef; // get ref to the item array
				for (int ci = 0; ci < dt2.Columns.Count; ci++)
				{
					//if (ci == 14) ci = ci; // debug
					vo[ci] = ds.GetValue(ri, ci); // direct copy into ItemArray
				}

				dt2.Rows.Add(dr2);
			}

			QueryManager qm2 = ToolHelper.SetupQueryManager(q2, dt2);
			qm2.ResultsFormat.OutputFormContext = OutputFormContext.Popup;

			PopupGrid pug = new PopupGrid(qm2);
			MoleculeGridPanel.ConfigureAndShow(qm2, pug);
			return;
		}

		/// <summary>
		/// Set the viewtype for the chart
		/// </summary>
		/// <param name="viewTypeName"></param>

		private void SetChartViewType(string viewTypeName)
		{
			//if (Lex.Eq(viewTypeName, "None"))
			//{
			//	SplitContainerControl.PanelVisibility = SplitPanelVisibility.Panel1;
			//	Chart.Visible = false;
			//	Chart.DataSource = null;
			//	return;
			//}

			//else
			//{
			//	Chart.DataSource = PivotGrid;
			//	DevExpress.XtraCharts.ViewType viewType =
			//		(DevExpress.XtraCharts.ViewType)Enum.Parse(typeof(DevExpress.XtraCharts.ViewType), viewTypeName, true);
			//	Chart.SeriesTemplate.ChangeView(viewType);
			//	if (Chart.Diagram is Diagram3D)
			//	{
			//		Diagram3D diagram = (Diagram3D)Chart.Diagram;
			//		diagram.RuntimeRotation = true;
			//		diagram.RuntimeZooming = true;
			//		diagram.RuntimeScrolling = true;
			//	}
			//	foreach (Series series in Chart.Series)
			//	{
			//		ISupportTransparency supportTransparency = series.View as ISupportTransparency;
			//		if (supportTransparency != null)
			//		{
			//			if ((series.View is AreaSeriesView) || (series.View is Area3DSeriesView))
			//				supportTransparency.Transparency = 135;
			//			else
			//				supportTransparency.Transparency = 0;
			//		}
			//	}

			//	SplitContainerControl.SplitterPosition = SplitContainerControl.Height * 3 / 5; // make grid 3/5 of control size & chart 2/5
			//	SplitContainerControl.PanelVisibility = SplitPanelVisibility.Both;
			//	Chart.Visible = true;
			//}
		}


/// <summary>
/// From sample code
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		void BuildAndRunSimpleDrillDownQuery(object sender, PivotCellEventArgs e)
		{
			// Create a new form.
			Form form = new Form();
			form.Text = "Records";
			// Place a DataGrid control on the form.
			DataGrid grid = new DataGrid();
			grid.Parent = form;
			grid.Dock = DockStyle.Fill;
			// Get the recrd set associated with the current cell and bind it to the grid.
			grid.DataSource = e.CreateDrillDownDataSource();
			form.Bounds = new Rectangle(100, 100, 500, 400);
			// Display the form.
			form.ShowDialog();
			form.Dispose();
		}

/// <summary>
/// Calculate summary value
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_CustomSummary(object sender, PivotGridCustomSummaryEventArgs e)
		{
			int ri, dri;
			int queryEngineInstanceId = -1;

			PivotGridFieldContext f = GetPivotGridFieldContext(e.DataField);
			if (f.Aggregation.Role != AggregationRole.DataSummary) // must check this since when moving a field to a new area in the PivotGridDialog, this event fires before the FieldAreaChanged event
				return;	// throw new Exception("Expected DataSummary Role");

			DataTableMx dt = f.Qm.DataTable;

			PivotDrillDownDataSource ds = e.CreateDrillDownDataSource();
			List<object[]> voList = new List<object[]>();

			for (ri = 0; ri < ds.RowCount; ri++) // get list of rows containing values to summarize
			{
				PivotDrillDownDataRow row = ds[ri];
				dri = row.ListSourceRowIndex;
				if (dri >= dt.Rows.Count) throw new Exception("dri >= dt.Rows.Count");
				object[] voa = dt.Rows[dri].ItemArrayRef;
				voList.Add(voa);
				continue;
			}

			if (f.Qe != null)
				queryEngineInstanceId = f.Qe.Id;

			Aggregator ag = new Aggregator();
			object r = ag.AggregateQueryColumn(queryEngineInstanceId, f.Qc, f.Aggregation, voList); // do the aggregation
			e.CustomValue = r; // return value
			return;
		}

/// <summary>
/// Calculate group value from incoming data value
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_CustomGroupInterval(object sender, PivotCustomGroupIntervalEventArgs e)
		{
			PivotGridFieldContext fc = GetPivotGridFieldContext(e.Field);
			PivotGridFieldMx f = fc.F;
			AggregationDef ad = f.Aggregation;
			if (e.Field.Area != PivotArea.ColumnArea && e.Field.Area != PivotArea.RowArea)
				return; // not sure why this is happening but ignore if so

			if (!ad.IsGroupingType) // must check this since when moving a field to a grouping area in the PivotGridDialog, this event fires before the FieldAreaChanged event (true?)
				return;  // throw new Exception("Expected Group Role");

			AggregationTypeDetail td = ad.TypeDetail;
			if (td.GroupingMethod == null) return;

			QueryColumn qc = fc.Qc;
			object vo = e.Value; // value to apply group function to

			object groupValue = td.GroupingMethod(qc, ad, vo);
			e.GroupValue = groupValue;
		}

/// <summary>
/// Width of field changed
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_FieldWidthChanged(object sender, PivotFieldEventArgs e)
		{
			PivotGridField f = GetViewField(e.Field);
			f.Width = e.Field.Width;

			return;
		}

		private void Grid_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
		{

			if (DebugMx.False) return; // debug, just return & show default DX menus

			// Rt-Click on field in any area

			if (e.MenuType == PivotGridMenuType.Header)
			{
				BuildFieldPopupMenu(e);
				return;
			}

			// Simple click on column that is in the Data Area allowing a summary-type to be selected

			else if (e.MenuType == PivotGridMenuType.HeaderSummaries)
			{
				BuildFieldPopupMenu(e);
				return;
			}

			// Rt-Click in the empty part of the header area (not on a field name)
			// Has "Reload Data" and "Show Field List" items by default

			else if (e.MenuType == PivotGridMenuType.HeaderArea)
			{
				e.Allow = false; // don't show this menu
				//DXPopupMenu dxpMenu = e.Menu; 
				//for (int mi = 0; mi < e.Menu.Items.Count; mi++)
				//{
				//	DXMenuItem dxMi = e.Menu.Items[mi];
				//	continue;
				//}
				return;
			}

			// Rt-Click on a row or column header field value

			else if (e.MenuType == PivotGridMenuType.FieldValue)
			{ return; }


			// Rt-click in a grid cell
			//     Corresponds to the context menu, allowing end-users to apply conditional formatting
			//     to data cells (when the DevExpress.XtraPivotGrid.PivotGridOptionsMenu.EnableFormatRulesMenu
			//     option is enabled).

			else if (e.MenuType == PivotGridMenuType.Cell)
			{ return; }

			return;
		}

		/// <summary>
		/// BuildFieldPopupMenu
		/// </summary>
		/// <param name="e"></param>
		
		void BuildFieldPopupMenu(
			PopupMenuShowingEventArgs e)
		{
			PivotArea area = (PivotArea)(-1);
			string areaName = "";
			Image areaImage = null;
			DXSubMenuItem smi;

			PivotGridFieldContext f = GetPivotGridFieldContext(e.Field);
			if (f == null) return;
			PivotGridField = f.F; // save as current field

			ResultsField rfld = f.ResultsField;
			QueryColumn qc = f.Qc;

			GetFieldAreaAttributes(e, out area, out areaName, out areaImage);

			DXPopupMenu dxpMenu = e.Menu; // clear dest menu
			dxpMenu.Items.Clear();

			AggregationDefMenus ats = new AggregationDefMenus(); // used to build menus

			// Build DataArea summary type items

			if (area == PivotArea.DataArea)
			{
				ContextMenuStrip modelMenu = ats.SetupAggregationTypeMenu(qc, f.Aggregation, null, includeGroupingItems: false, includeSummaryItems: true);
				ConvertAndAppendContextMenuStripToDxPopupMenu(modelMenu, dxpMenu);
			}

			// Build ColumnArea or RowArea Grouping items

			if (area == PivotArea.ColumnArea || area == PivotArea.RowArea)
			{
				ContextMenuStrip modelMenu = ats.SetupAggregationTypeMenu(qc, f.Aggregation, null, includeGroupingItems: true, includeSummaryItems: false);
				ConvertAndAppendContextMenuStripToDxPopupMenu(modelMenu, dxpMenu);
			}

			if (dxpMenu.Items.Count == 0) // just add to menu if nothing there so far
				smi = dxpMenu; 

			else // add to submenu
			{
				smi = new DXSubMenuItem("Move Field to");
				smi.BeginGroup = true;
				dxpMenu.Items.Add(smi);
			}

			smi.Items.Add(new DXMenuCheckItem("Column Area", area == PivotArea.ColumnArea, Bitmaps16x16.Images[0], ColumnAreaMenuItem_Click));
			smi.Items.Add(new DXMenuCheckItem("Row Area", area == PivotArea.RowArea, Bitmaps16x16.Images[1], RowAreaMenuItem_Click));
			smi.Items.Add(new DXMenuCheckItem("Data Area", area == PivotArea.DataArea, Bitmaps16x16.Images[2], DataAreaMenuItem_Click));
			smi.Items.Add(new DXMenuCheckItem("Filter Area", area == PivotArea.FilterArea, Bitmaps16x16.Images[3], FilterAreaMenuItem_Click));
			smi.Items.Add(new DXMenuCheckItem("None", area == (PivotArea)(-1), null, UnassignedAreaMenuItem_Click));

			return;
		}

/// <summary>
/// Convert and append the items in a ContextMenuStrip to a DXPopupMenu 
/// </summary>
/// <param name="modelMenu"></param>
/// <param name="dxpMenu"></param>

		void ConvertAndAppendContextMenuStripToDxPopupMenu(
			ContextMenuStrip modelMenu,
			DXPopupMenu dxpMenu)
		{
			DXMenuItem mi;
			DXMenuCheckItem cmi;
			DXSubMenuItem smi;
			ToolStripMenuItem tsmi, defaultTsmi = new ToolStripMenuItem();
			bool separatorSeen = false;

			int itemCount = modelMenu.Items.Count;

			int visCnt = 0;
			for (int mii = 0; mii < itemCount; mii++)
			{
				ToolStripItem tsi = modelMenu.Items[mii];
				if (!tsi.Available) continue;

				if (tsi is ToolStripSeparator)
				{
					if (dxpMenu.Items.Count > 0) // only note if something already in dxpMenu
						separatorSeen = true;
					continue;
				}

				cmi = new DXMenuCheckItem();
				if (separatorSeen)
				{
					cmi.BeginGroup = true;
					separatorSeen = false;
				}

				cmi.Caption = tsi.Text;
				cmi.Click += new System.EventHandler(AggregationTypeMenuItem_Click);
				cmi.Tag = tsi.Tag; // store summary type in tag

				if (tsi is ToolStripMenuItem)
				{
					tsmi = tsi as ToolStripMenuItem;
					cmi.Checked = tsmi.Checked;

					if (tsmi.BackColor != defaultTsmi.BackColor)
						cmi.Appearance.BackColor = tsi.BackColor;

					if (tsmi.Font.Style != 0)
						cmi.Appearance.Font = new Font(cmi.Appearance.Font, tsmi.Font.Style);
				}

				cmi.Visible = tsi.Available;
				if (cmi.Visible) visCnt++;

				dxpMenu.Items.Add(cmi);
			}

			return;
		}
	
/// <summary>
///  Get the attributes for the area that a field is currently in
/// </summary>
/// <param name="e"></param>
/// <param name="area"></param>
/// <param name="areaName"></param>
/// <param name="areaImage"></param>

	void GetFieldAreaAttributes(
			PopupMenuShowingEventArgs e,
			out PivotArea area,
			out string areaName,
			out Image areaImage)
		{
			area = (PivotArea)(-1);
			areaName = "";
			areaImage = null;

			PivotGridFieldMx f = e.Field as PivotGridFieldMx;
			if (f == null) return;

			area = f.Area;
			areaName = f.Area.ToString();
			if (f.AreaIndex < 0 || f.Visible == false)
			{
				area = (PivotArea)(-1);
				areaName = "None";
			}

			if (area == PivotArea.ColumnArea) areaImage = Bitmaps16x16.Images[0];
			else if (area == PivotArea.RowArea) areaImage = Bitmaps16x16.Images[1];
			else if (area == PivotArea.DataArea) areaImage = Bitmaps16x16.Images[2];
			else if (area == PivotArea.FilterArea) areaImage = Bitmaps16x16.Images[3];

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
			PivotGridFieldMx f = PivotGridField;
			ResultsField rfld = f.ResultsField as ResultsField;
			MetaColumn mc = rfld.MetaColumn;
			f.Aggregation.Role = role; // set the role for Mobius
			f.Aggregation.SetDefaultTypeIfUndefined(mc); // and default type if needed
			f.SyncDxAreaToMxRole(); // sync Dx Area

			if (UpdateViewWhenGridControlChanges)
				View.UpdateViewFieldsFromGridFields();

			return;
		}


		/// <summary>
		/// Summary type selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AggregationTypeMenuItem_Click(object sender, EventArgs e)
		{
			PivotGridFieldMx f = PivotGridField;

			DXMenuItem mi = sender as DXMenuItem;
			string typeName = mi.Tag.ToString(); // tag is enum member name

			AggregationTypeDetail atd = AggregationTypeDetail.GetByTypeName(typeName, true);
			if (atd.IsGroupingType && atd.GroupingType == GroupingTypeEnum.NumericInterval)
			{
				DialogResult dr = NumericIntervalDialog.ShowDialog(f.Aggregation, UIMisc.MousePosition);
				//if (dr == DialogResult.Cancel) return;
			}

			f.Aggregation.SetFromTypeName(typeName);
			f.SyncDxAreaToMxRole(); // sync Dx area
			PivotGrid.RefreshData();

			if (UpdateViewWhenGridControlChanges)
				View.UpdateViewFieldsFromGridFields();

			return;
		}

/// <summary>
/// GetPivotGridFieldContext
/// </summary>
/// <param name="f"></param>
/// <returns>PivotGridFieldContext</returns>

		public static PivotGridFieldContext GetPivotGridFieldContext(
			PivotGridFieldBase fb,
			bool throwException = false)
		{
			AggregationDef a;

			PivotGridFieldContext fc = new PivotGridFieldContext();

			PivotGridFieldMx f = fb as PivotGridFieldMx;
			if (f == null)
			{
				if (throwException)
					throw new Exception("PivotGridFieldMx is null");
				else return null;
			}

			fc.F = f;
			fc.ResultsField = f.ResultsField as ResultsField;
			if (fc.ResultsField == null)
			{
				if (throwException)
					throw new Exception("ResultsField is null");
				else return null;
			}

			fc.Qc = fc.ResultsField.QueryColumn;
			fc.Mc = fc.Qc.MetaColumn;

			fc.Q = fc.Qc.QueryTable.Query;
			fc.Qm = fc.Q.QueryManager as QueryManager;
			if (fc.Qm != null)
				fc.Qe = fc.Qm.QueryEngine;

			a = fc.Aggregation = f.Aggregation;
			if (a != null && a.Role != PivotGridFieldMx.DxAreaToMxRole(f)) // be sure Mx role is in synch with Dx FieldArea which may change before FieldAreaChanged event fires
			{
				a.Role = PivotGridFieldMx.DxAreaToMxRole(f);
				a.SetDefaultTypeIfUndefined(fc.Mc); // also set type if not defined yet
			}

			return fc;
		}

	}

/// <summary>
/// Mx context of PivotGridField
/// </summary>
	public class PivotGridFieldContext
	{
		public PivotGridFieldMx F;
		public ResultsField ResultsField;
		public QueryColumn Qc;
		public MetaColumn Mc;
		public Query Q;
		public QueryManager Qm;
		public QueryEngine Qe;

		public AggregationDef Aggregation;
	}
}
