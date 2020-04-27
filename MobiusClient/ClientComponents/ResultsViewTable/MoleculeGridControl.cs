using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Layout;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraRichEdit;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.XtraGrid.Views.Card.ViewInfo;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraGrid.Drawing;
using DevExpress.XtraGrid.Registrator;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;

namespace Mobius.ClientComponents
{

	// See: http://www.bobpowell.net/toolboxbitmap.htm to make bitmap associated with 
	//  UserControl appear in the Visual Studio toolbox
	//    [ToolboxBitmap(typeof(MoleculeGrid), "Mobius.Controls.Mobius16x16.bmp")]
	//    [ToolboxBitmap(typeof(MoleculeGrid), "Controls.Mobius16x16.bmp")]

	/// <summary>
	/// Extended DevExpress.XtraGrid.GridControl to handle molecule data type
	/// </summary>
	/// 
	public partial class MoleculeGridControl : GridControl
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		internal QueryManager QueryManager { get { return _queryManager; } set { _queryManager = value; } }
		QueryManager _queryManager;

		internal DataTableMx DataTable { get { return QueryManager.DataTable; } }
		internal DataTableManager Dtm { get { return QueryManager.DataTableManager; } }
		internal Query Query { get { return QueryManager.Query; } } // associated query
		internal ResultsFormat ResultsFormat { get { return QueryManager.ResultsFormat; } } // associated results format
		internal ResultsFormat Rf { get { return QueryManager.ResultsFormat; } } // alias
		internal ResultsFormatter Formatter { get { return QueryManager.ResultsFormatter; } } // associated formatter
		internal QueryEngine Qe { get { return QueryManager.QueryEngine; } }
		internal MoleculeGridPanel GridPanel; // containing panel, must be initialized by owner
		internal bool InitializeRowState = true; // init row state when initializing attributes

		DataTableMx _dataSource; // associated data source (must currently be a DataTableMx)

		internal ColumnView V { get { return MainView as ColumnView; } } // common view
		internal GridView GV { get { return MainView as GridView; } }
		internal BandedGridView BGV { get { return MainView as BandedGridView; } }
		internal LayoutView LV { get { return MainView as LayoutView; } }
		internal CardView CV { get { return MainView as CardView; } }

		internal int UpdateDepth2 = 0; // depth of control locking for updating

		internal Graphics ControlGraphics; // GDI+ / device context for control

		internal RepositoryItemMemoEdit RepositoryItemMemoEdit; // repository item for viewing multiline text
		internal RepositoryItemPictureEdit RepositoryItemPictureEdit; // repository item for viewing structures and images
		internal RepositoryItemCheckEdit RepositoryItemCheckEdit; // repository item for check mark column

		internal RepositoryItemHypertextLabel RepositoryItemHypertextLabel; // repository item for HTML text to be rendered as HTML using limited set of tags
		internal RepositoryItemRichTextEdit RepositoryItemRichTextEdit; // full HTML control
		internal bool UseRichHtmlColumnRendering = DebugMx.False; // mode for rendering HTML in grid

		internal int[] GridToDataTableColumnMap; // original column indexes from grid column into datatable column
		internal int[] DataTableToGridColumnMap; // original column indexes from datatable column to grid column

		internal MoleculeGridHelpers Helpers; // assoc form instance containg menus & other referenced controls
		internal MoleculeGridMode Mode = MoleculeGridMode.QueryView; // default to query viewer mode

		internal bool InSetup = false; // grid is being setup/modified by code

		internal UserDataEditor Editor; // any editor class associated with grid
		internal bool UpdateImmediately = true; // if true do any updates immediately

		internal bool _autoNumberRows = false; // if true use first column to number rows
		internal bool _showNewRow = false; // automatically show a new row at end of grid
		internal string _checkmarkColumnName = ""; // name of bool column to store checkmark information in

		internal bool ShowMarkedRowsOnly = false; // show marked rows only
		internal bool ShowSelectedRowsOnly = false; // show marked rows only
        internal bool ShowUnmarkedRowsOnly = false; // show unmarked rows only
        internal int NotFocusedRowToHighlight = -1; // row to highlight when grid not focused
		internal int NotFocusedColToHighlight = -1; // col to highlight when grid not focused

		internal List<string> RowBuffer = new List<string>(); // buffered rows for WSGrid

		//internal MoleculeControl EditStructureHiddenControl;
		//internal MoleculeControl CopyPasteHiddenControl;

		internal int EditStructureRow; // row of structure currently being edited
		internal int EditStructureColumn; // column of structure currently being edited

		internal bool StructurePopupEnabled = true;
		internal int StructurePopupRow; // where current structure popup is displayed
		internal int StructurePopupColumn;
		internal System.Windows.Forms.Timer StructurePopupTimer; // timer to monitor when to hide structure popup

		internal string HeaderLinesString = null; // number of header lines to include in copy
		internal bool InPaste = false; // executing a paste operation

		internal string ShowDescriptionArg; // TableHeaderPopup name of metatable to show description for
		internal string AddToQueryArg; // TableHeaderPopup name of metatable to add to query

		internal string PreviousCn = ""; // most recently seen QuickSearch number
		internal string SelectedCid = ""; // Cn in selected grid cell
		internal string SelectedTarget = ""; // currently selected target symbol

		public GridCell LastSelectionMouseDown; // last grid cell selected
		public MouseEventArgs LastMouseDownEventArgs; // event data for last mousedown event
		public int LastMouseDownRowIdx, LastMouseDownCol;
		public HitInfo LastMouseHitInfo;
		public GridColumn LastMouseDownGridCol;
		public CellInfo LastMouseDownCellInfo;
		public QueryTable LastMouseDownQueryTable;

		public EventHandler SelectionChangedCallback; // optional callback for selection change
		public MouseEventHandler MouseDownCallback;
		public MouseEventHandler MouseUpCallback;
		public MouseEventHandler MouseClickCallback;

		internal CidList InitialGridList; // current list when QueryExec entered
		internal bool CheckMarkInitially = false; // if true checkmark grid items initially
		internal bool CheckMarksChanged = false; // if true user has edited checkmarks
		internal bool ShowCheckMarkCol = true;

		internal System.Windows.Forms.Timer RetrievalMonitorTimer; // timer to monitor retrieval progress
		internal int LastRowRendered = -1; // GridView index of last row rendered
		internal bool RetrievalMonitorEnabled = false;
		//internal bool RefreshDataSourceRequested = false; // if true re-render the results
		internal bool HasBeenFocusedInCode = false; // indicates whether grid has been focused in code

		const string UnboundSuffix = ".Unbound";

		// Metrics for BandedGridView

		public int IndicatorColWidth = 50; // width of indicator col, enough for 6 digits
		const int CheckMarkColWidth = 20; // width of checkmark column
		const int BandPanelRowHeight = 18; // height of a single row for all bands in view (seems to have no effect below a certain value)
		const int ScrollBarWidth = 22; // width of vertical scrollbar
		int TableHeaderBandRows; // number of rows in table headers
		const int ColumnPanelRowHeight = 18; // height of a single column header row
		int TotalColumnPanelHeight; // total height for column panel
		const float FontSize = 8.25f;

		// Metrics for LayoutView for display of keys & structures only

		const int LayoutMinWidth = 212;
		const int LayoutMinHeight = 112;

		// Metrics for CardView used for printing for keys & structures only

		const int CardWidth = 236; // match LayoutStructWidth to avoid clipping of larger structures (redo, not really constant)

		/// <summary>
		/// Needed for creating default MoleculeBandedGridView
		/// </summary>
		/// <returns></returns>

		protected override BaseView CreateDefaultView()
		{
			return CreateView("MoleculeGridView");
			//return CreateView("MoleculeBandedGridView");
		}

		/// <summary>
		/// Needed for creating default MoleculeBandedGridView
		/// </summary>
		/// <returns></returns>
		/// 
		//protected override void RegisterAvailableViewsCore(InfoCollection collection)
		//{
		//	base.RegisterAvailableViewsCore(collection);
		//	collection.Add(new MoleculeBandedGridViewInfoRegistrator());
		//	collection.Add(new MoleculeGridViewInfoRegistrator());
		//}

		/// <summary>
		/// Associated data source, either a DataTable or a RawTable
		/// </summary>

		public new DataTableMx DataSource
		{
			get { return _dataSource; }
			set
			{
				_dataSource = value;

				if (QueryManager != null) QueryManager.DataTable = _dataSource;
				if (_dataSource == null) // if null just set grid source & return
				{
					base.DataSource = _dataSource;
					return;
				}

				DataTableMx dt = _dataSource;
				dt.RowChanged += new DataRowMxChangeEventHandler(Dt_RowChanged); // Monitor row changes

				if (QueryManager == null)
				{
					SetupDefaultQueryManager(dt);
					SetupUnboundColumns(dt);
				}

				SaveGridToDataTableColumnMapping(dt);

				DataTableManager dtm = QueryManager.DataTableManager;
				if (dtm == null) // allocate manager if needed
					dtm = QueryManager.DataTableManager = new DataTableManager();
				//else dtm.ResetDataState();

				if (dtm.RowAttributesVoPos >= 0) dtm.InitializeRowAttributes(InitializeRowState);

				//				dtm.ApplyFilters();
				dtm.UpdateFilterState();
				if (QueryManager.StatusBarManager != null)
					QueryManager.StatusBarManager.DisplayFilterCountsAndString();

				base.DataSource = _dataSource; // fire up the XtraGrid view
				return;
			}
		}

		/// <summary>
		/// Prevents the grid control from being updated while changes are made
		/// Needed since DevExpress code may make unbalanced calls
		/// </summary>

		public void BeginUpdate2()
		{
			BeginUpdate();
			UpdateDepth2++;
			//ClientLog.Message("BeginUpdate2, depth: " + UpdateDepth2);
		}

		/// <summary>
		/// Prevents the grid control from being updated while changes are made
		/// Needed since DevExpress code may make unbalanced calls
		/// </summary>

		public override void BeginUpdate()
		{
			if (UpdateDepth2 == 0) base.BeginUpdate();
			//ClientLog.Message("BeginUpdate, depth: " + UpdateDepth2);
		}

		/// <summary>
		/// Unlocks the grid control after changes & updates the UI
		/// </summary>

		public void EndUpdate2()
		{
			if (UpdateDepth2 > 0) UpdateDepth2--;
			EndUpdate();
			//ClientLog.Message("EndUpdate2, depth: " + UpdateDepth2);
		}

		/// <summary>
		/// Unlocks the grid control after changes & updates the UI
		/// </summary>

		public override void EndUpdate()
		{
			try
			{
				if (UpdateDepth2 == 0) base.EndUpdate();
				else return;
			}
			catch (Exception ex) { ex = ex; }
			//ClientLog.Message("EndUpdate, depth: " + UpdateDepth2);
		}

		/// <summary>
		/// Refresh control if not in update
		/// </summary>

		public override void Refresh()
		{
			if (UpdateDepth2 == 0) base.Refresh();
			else return;
			//ClientLog.Message("Refresh");
		}

		/// <summary>
		/// Monitor row changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void Dt_RowChanged(object sender, DataRowMxChangeEventArgs e)
		{
			if (Editor != null) Helpers.NewRow = null;

			return;
		}

		/// <summary>
		/// Start output to grid
		/// </summary>

		internal void StartDisplay()
		{
			CheckMarkInitially = SS.I.GridMarkCheckBoxesInitially;
			CheckMarksChanged = false;
			LastRowRendered = -1;
			UpdateDepth2 = 0;

			DataSource = QueryManager.DataTable;
			QueryManager.DataTableManager.StartRowRetrieval();

			RetrievalMonitorEnabled = true;

			HasBeenFocusedInCode = false;

			return;
		}

		/// <summary>
		/// StartRetrievalMonitorTimer
		/// </summary>

		internal void StartRetrievalMonitorTimer()
		{
			if (RetrievalMonitorTimer == null)
			{
				RetrievalMonitorTimer = new System.Windows.Forms.Timer();
				RetrievalMonitorTimer.Interval = 500;
				RetrievalMonitorTimer.Tick += new System.EventHandler(this.RetrievalMonitorTimer_Tick);
			}

			if (!RetrievalMonitorTimer.Enabled)
				RetrievalMonitorTimer.Enabled = true;

			return;
		}

		/// <summary>
		/// Check progress of retrieval & return to query builder if no data returned
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RetrievalMonitorTimer_Tick(object sender, EventArgs e)
		{

			//if (RefreshDataSourceRequested) // reqkuest to refresh data source
			//{
			//	UIMisc.Beep();
			//	RefreshDataSourceMx();
			//	RefreshDataSourceRequested = false;
			//	return;
			//}

			if (!RetrievalMonitorEnabled) return;

			if (Dtm.RowRetrievalState == RowRetrievalState.Complete)
			//				QueryManager.Query.Mode == QueryMode.Browse)
			{
				//RetrievalMonitorTimer.Enabled = false;
				RetrievalMonitorEnabled = false;
				Progress.Hide(); // be sure progress is hidden
				if (Editor == null && // back to EditQuery if no editor && no rows retrieved
					Dtm.TotalQeRowsRetrieved == 0 && DataTable.Rows.Count == 0)
				{
					CommandExec.ExecuteCommandAsynch("EditQuery");
					MessageBoxMx.ShowError("No data have been found that matches your query.");
				}
			}

			else if (DateTime.Now.Subtract(MoleculeGridHelpers.WaitForMoreDataStartTime).TotalMilliseconds > 500 &&
				MoleculeGridHelpers.WaitForMoreDataStartTime.Equals(DateTime.MinValue) == false)
			{ // show wait message if waiting for more than a sec
				//ClientLog.Message("Show Progress: " + MoleculeGridHelpers.WaitForMoreDataStartTime.ToString());
				//MoleculeGridHelpers.WaitForMoreDataStartTime = DateTime.MinValue;
				//Progress.Show("Retrieving data...");

				//bool hide = true;
				//if (BGV != null && LastRowRendered >= 0)
				//{
				//  Rectangle r = GetCellRect(LastRowRendered, 0);
				//  if (r != Rectangle.Empty && r.Bottom < this.Bottom) hide = false;
				//}
				//if (hide) Progress.Hide(); // be sure progress is hidden

				//else Progress.Hide(); // todo: better for card view
			}

			if (DataTable.Rows.Count >= 5 && !HasBeenFocusedInCode)
			{ // after a few rows are retrieved check that grid control is focused so page up/page down works
				try { Focus(); HasBeenFocusedInCode = true; /* SystemUtil.Beep(); */ }
				catch (Exception ex) { ex = ex; }
			}

			return;
		}

		/// <summary>
		/// Automatically number rows 1 - n if true
		/// </summary>

		public bool AutoNumberRows
		{
			get { return _autoNumberRows; }
			set { _autoNumberRows = value; }
		}

		/// <summary>
		/// Add a new row at the end of the grid if true;
		/// </summary>
		/// 
		public bool ShowNewRow
		{
			get { return _showNewRow; }
			set { _showNewRow = value; }
		}

		/// <summary>
		/// Name of optional bool column containing checkmark information
		/// </summary>

		public string CheckmarkColumnName
		{
			get { return _checkmarkColumnName; }
			set { _checkmarkColumnName = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>

		public MoleculeGridControl()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			Helpers = new MoleculeGridHelpers(); // allocate associated form
			Helpers.Grid = this;

			//this.SuspendLayout();

			//EditStructureHiddenControl = Helpers.EditStructureHiddenControl; // Structure renditor for structure editing
			//Size size = EditStructureHiddenControl.Size;
			//EditStructureHiddenControl.Size = new Size(1, 1); // make small to avoid flash
			//this.Controls.Add(EditStructureHiddenControl); // automatically made visible when added
			//EditStructureHiddenControl.Visible = false;
			//EditStructureHiddenControl.Size = size;
			//EditStructureHiddenControl.Parent = this;

			//CopyPasteHiddenControl = Helpers.CopyPasteHiddenControl; // Structure renderer for structure copy/paste
			//size = CopyPasteHiddenControl.Size;
			//CopyPasteHiddenControl.Size = new Size(1, 1); // make small to avoid flash
			//this.Controls.Add(CopyPasteHiddenControl); // automatically made visible when added
			//CopyPasteHiddenControl.Visible = false;
			//CopyPasteHiddenControl.Size = size;
			//CopyPasteHiddenControl.Parent = this;

			//this.ResumeLayout(false);

			RepositoryItemMemoEdit = new RepositoryItemMemoEdit(); // used to display text values
			RepositoryItemMemoEdit.ScrollBars = ScrollBars.None;
			RepositoryItems.Add(RepositoryItemMemoEdit);

			RepositoryItemPictureEdit = new RepositoryItemPictureEdit(); // used to display structures and images
			RepositoryItems.Add(RepositoryItemPictureEdit);

			RepositoryItemCheckEdit = new RepositoryItemCheckEdit(); // used to display checkmark column
			RepositoryItems.Add(RepositoryItemCheckEdit);

			RepositoryItemHypertextLabel = new RepositoryItemHypertextLabel(); // used to display marked-up HTML
			RepositoryItems.Add(RepositoryItemHypertextLabel);
			RepositoryItemHypertextLabel.ReadOnly = true;
			RepositoryItemHypertextLabel.OpenHyperlink += OpenClickedHyperlinkWithinHtml;

			RepositoryItemRichTextEdit = new RepositoryItemRichTextEdit(); // full HTML allowed
			RepositoryItems.Add(RepositoryItemRichTextEdit);
			RepositoryItemRichTextEdit.ReadOnly = true; // don't allow in-place editing
			RepositoryItemRichTextEdit.DocumentFormat = DocumentFormat.Html;

#pragma warning disable
			DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true; // avoid cross threading errors
#pragma warning restore

			StructurePopupTimer = new System.Windows.Forms.Timer(); // build timer
			StructurePopupTimer.Interval = 500;
			StructurePopupTimer.Tick += new System.EventHandler(this.StructurePopupTimer_Tick);
			StructurePopupTimer.Enabled = false;

			return;
		}

		/// <summary>
		/// This completes the initialization by adding standard event handlers.
		/// It must be done after the primary initialization because the grid View is defined after the MoleculeGridControl is initialized
		/// </summary>
		/// 

		public void AddStandardEventHandlers()
		{

			this.GotFocus += new EventHandler(Helpers.MoleculeGridControl_GotFocus);
			this.LostFocus += new EventHandler(Helpers.MoleculeGridControl_LostFocus);

			foreach (ColumnView v in ViewCollection) // add handlers for each view
			{
				v.CustomUnboundColumnData += // Handle conversion of Mobius custom data types between the grid and the underlying DataSet
					new CustomColumnDataEventHandler(Helpers.OnCustomUnboundColumnData);

				v.ShowFilterPopupListBox += // handle display of popup filters
					new FilterPopupListBoxEventHandler(Helpers.PrototypeGridView_ShowFilterPopupListBox);

				v.CustomRowFilter += // do custom row filtering
					new RowFilterEventHandler(Helpers.PrototypeGridView_CustomRowFilter);

				v.ShowingEditor += // do any pre-editing operations
					new CancelEventHandler(Helpers.PrototypeGridView_ShowingEditor);

				if (v is BandedGridView)
				{
					BandedGridView bv = v as BandedGridView;

					bv.RowCellStyle += // handle styling of cell
						new RowCellStyleEventHandler(Helpers.PrototypeGridView_RowCellStyle);

					bv.CustomDrawBandHeader += // handle drawing of band headers
						new BandHeaderCustomDrawEventHandler(Helpers.BandedGridView_CustomDrawBandHeader);

					bv.CustomDrawColumnHeader += // handle drawing of column headers
						new ColumnHeaderCustomDrawEventHandler(Helpers.BandedGridView_CustomDrawColumnHeader);

					bv.CustomDrawRowIndicator += // handle drawing of row indicator
						new RowIndicatorCustomDrawEventHandler(Helpers.PrototypeGridView_CustomDrawRowIndicator);

					bv.CustomDrawCell += // do custom drawng of cell
						new RowCellCustomDrawEventHandler(Helpers.PrototypeGridView_CustomDrawCell);

					bv.CustomColumnDisplayText += // set custom text just before painting
						new CustomColumnDisplayTextEventHandler(Helpers.PrototypeGridView_CustomColumnDisplayText);

					bv.ColumnWidthChanged += // handle resizing of columns
						new ColumnEventHandler(Helpers.PrototypeGridView_ColumnWidthChanged);

					bv.DragObjectStart +=
						new DragObjectStartEventHandler(Helpers.BandedGridView_DragObjectStart);

					bv.DragObjectOver +=
						new DragObjectOverEventHandler(Helpers.BandedGridView_DragObjectOver);

					bv.DragObjectDrop +=
						new DragObjectDropEventHandler(Helpers.BandedGridView_DragObjectDrop);

					bv.BandWidthChanged += // handle resizing of bands
						new BandEventHandler(Helpers.BandedGridView_BandWidthChanged);

					bv.RowCellClick += // handle click on cells
						new RowCellClickEventHandler(Helpers.PrototypeGridView_RowCellClick);

					bv.ShownEditor += GridView_ShownEditor;
				}
			}

			return;
		}

		/// <summary>
		/// AddMobiusCustomUnboundColumnDataEventHandler
		/// </summary>
		/// <param name="v"></param>

		public void AddMobiusCustomUnboundColumnDataEventHandler(ColumnView v)
		{
			v.CustomUnboundColumnData += // Handle conversion of Mobius custom data types between the grid and the underlying DataSet
				new CustomColumnDataEventHandler(Helpers.OnCustomUnboundColumnData);
		}

		/// <summary>
		/// Get grid to dataset column info
		/// </summary>
		/// <param name="gridCol"></param>
		/// <returns></returns>

		public ColumnInfo GetColumnInfo(
				GridColumn gridCol)
		{
			ColumnInfo ci = new ColumnInfo();
			GetColumnInfo(gridCol, ci);
			return ci;
		}

		/// <summary>
		/// Get grid to dataset column info
		/// </summary>
		/// <param name="gridCol"></param>
		/// <param name="ci"></param>

		public void GetColumnInfo(
			GridColumn gridCol,
			ColumnInfo ci)
		{
			ci.GridColumn = gridCol;
			ci.GridColAbsoluteIndex = gridCol.AbsoluteIndex;
			int colAbsoluteIndex = gridCol.AbsoluteIndex;

			if (GridToDataTableColumnMap == null || colAbsoluteIndex < 0 ||
				colAbsoluteIndex >= GridToDataTableColumnMap.Length)
			{
				ci.DataColIndex = -1;
				ci.DataColumn = null;
			}

			else
			{
				ci.DataColIndex = GridToDataTableColumnMap[colAbsoluteIndex];
				ci.DataColumn = DataTable.Columns[ci.DataColIndex];
			}

			if (ci.DataColIndex >= 0 && ci.DataColIndex < DataTable.Columns.Count &&
			 QueryManager != null)
				GetMetaDataForGridCol(colAbsoluteIndex, ci);

			return;
		}

		/// <summary>
		/// Row, column indexer into DataSource DataTable
		/// </summary>
		/// <param name="gr">Grid row</param>
		/// <param name="gc">Grid col</param>
		/// <returns></returns>

		public object this[int gr, int gc]
		{
			get
			{
				int dtRow = this.V.GetDataSourceRowIndex(gr);
				int dtCol = GridToDataTableColumnMap[gc];
				DataTableMx dt = DataSource as DataTableMx;
				if (dt == null) throw new Exception("No DataTable DataSource");
				return dt.Rows[dtRow][dtCol];
			}

			set
			{
				int dtRow = this.V.GetDataSourceRowIndex(gr);
				int dtCol = GridToDataTableColumnMap[gc];
				DataTableMx dt = DataSource as DataTableMx;
				if (dt == null) throw new Exception("No DataTable DataSource");
				dt.Rows[dtRow][dtCol] = value;
			}
		}

		/// <summary>
		/// Get/set row handle of the currently focused row
		/// </summary>

		public int Row
		{
			get
			{
				if (V == null) return -1;
				int rowHandle = V.FocusedRowHandle;
				return rowHandle;
			}

			set
			{
				if (V == null) return;
				V.FocusedRowHandle = value;
			}
		}

		/// <summary>
		/// Get/set column index within the view of the currently focused column
		/// </summary>

		public int Col
		{
			get
			{
				if (V == null) return -1;
				GridColumn c = V.FocusedColumn;
				if (c == null) return -1;
				return c.AbsoluteIndex;
			}

			set
			{
				if (V == null) return;
				if (value >= V.Columns.Count) return;
				if (value < 0) return;
				V.FocusedColumn = V.Columns[value];
			}
		}

		/// <summary>
		/// Get view row handle from data source index
		/// </summary>
		/// <param name="dataSourceIndex"></param>
		/// <returns></returns>

		public int GetRowHandle(
			int dataSourceIndex)
		{
			if (GV != null) return GV.GetRowHandle(dataSourceIndex);

			else if (LV != null) return LV.GetRowHandle(dataSourceIndex);

			else if (CV != null) return CV.GetRowHandle(dataSourceIndex);

			else throw new ArgumentException(MainView.GetType().ToString());
		}

		/// <summary>
		/// Return true if row is fully or partially visible
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <returns></returns>

		bool RowIsVisible(object sender, int rowHandle)
		{
			if (sender is GridView)
			{
				GridView view = sender as GridView;
				GridViewInfo viewInfo = view.GetViewInfo() as GridViewInfo;
				if (viewInfo == null || viewInfo.RowsInfo == null || viewInfo.RowsInfo.Count == 0)
					return true; // say true if no row info

				int topHandle = viewInfo.RowsInfo[0].RowHandle;
				int bottomHandle = viewInfo.RowsInfo[viewInfo.RowsInfo.Count - 1].RowHandle;

				if (topHandle < 0 || bottomHandle < 0) return true; // say true if either undefined

				if (rowHandle >= topHandle - 1 && rowHandle <= bottomHandle + 1) return true;
				else return false;
			}

			else if (sender is LayoutView)
			{
				return ((LayoutView)sender).IsCardVisible(rowHandle);
			}

			else if (sender is CardView)
			{
				CardVisibleState cvs = ((CardView)sender).IsCardVisible(rowHandle);
				return (cvs != CardVisibleState.Hidden);
			}

			else throw new ArgumentException(sender.GetType().ToString());
		}

		/// <summary>
		/// Return true if last visible row in grid
		/// </summary>
		/// <param name="view"></param>
		/// <param name="rowHandle"></param>
		/// <returns></returns>

		public bool IsLastVisibleRow(object sender, int rowHandle)
		{
			if (sender is GridView)
			{
				GridView view = sender as GridView;
				GridViewInfo viewInfo = view.GetViewInfo() as GridViewInfo;
				if (viewInfo == null || viewInfo.RowsInfo == null || viewInfo.RowsInfo.Count == 0)
					return false;
				return viewInfo.RowsInfo[viewInfo.RowsInfo.Count - 1].RowHandle == rowHandle;
			}

			else throw new ArgumentException(sender.GetType().ToString());
		}

		public int GetCellInt(
			int gr,
			int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return NullValue.NullNumber;

			object o = (DataTable.Rows[ci.DataRowIndex][ci.DataColIndex]);
			if (o is int) return (int)o;
			else return NullValue.NullNumber;
		}

		/// <summary>
		/// Set cell value
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="value"></param>

		public void SetCellValue(
		int gr,
		int gc,
		MobiusDataType mdt)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return;

			DataTable.Rows[ci.DataRowIndex][ci.DataColIndex] = mdt;
			RefreshRowCell(gr, ci.GridColumn);
			return;
		}

		/// <summary>
		/// Get cell value
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns></returns>

		public MobiusDataType GetCellValue(
			int gr,
			int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return null;

			MobiusDataType mdt = (DataTable.Rows[ci.DataRowIndex][ci.DataColIndex] as MobiusDataType);
			return mdt;
		}

		/// <summary>
		/// Get cell molecule
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns></returns>

		public MoleculeMx GetCellMolecule(
			int gr,
			int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return null;
			if (ci.DataRowIndex >= DataTable.Rows.Count) return null;

			object value = DataTable.Rows[ci.DataRowIndex][ci.DataColIndex];
			return value as MoleculeMx;
		}

		/// <summary>
		/// Get cell Image
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns></returns>

		public Image GetCellImage(
		int gr,
		int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return null;

			object value = DataTable.Rows[ci.DataRowIndex][ci.DataColIndex];
			if (value is MobiusDataType)
			{
				MobiusDataType mdt = value as MobiusDataType;
				return mdt.FormattedBitmap;
			}
			else if (value is Bitmap) return value as Bitmap;
			else return null;
		}

		/// <summary>
		/// Set cell ForeColor
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="color"></param>

		public void SetCellForeColor(
		int gr,
		int gc,
		Color color)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return;

			if (ci.DataValue is MobiusDataType)
			{
				((MobiusDataType)ci.DataValue).ForeColor = color;
				RefreshRowCell(gr, V.Columns[gc]);
			}
		}

		/// <summary>
		/// Get cell ForeColor
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns></returns>

		public Color GetCellForeColor(
		int gr,
		int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return Color.Transparent;

			if (ci.DataValue is MobiusDataType)
				return ((MobiusDataType)ci.DataValue).ForeColor;
			else return Color.Transparent;
		}

		/// <summary>
		/// Set cell BackColor
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="color"></param>

		public void SetCellBackColor(
			int gr,
			int gc,
			Color color)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return;

			if (ci.DataValue is MobiusDataType)
			{
				((MobiusDataType)ci.DataValue).BackColor = color;
			}

			else if (ci.DataColumn.DataType.GetType() == typeof(Color))
				DataTable.Rows[ci.DataRowIndex][ci.DataColIndex] = color;

			RefreshRowCell(gr, V.Columns[gc]);
		}

		/// <summary>
		/// Get cell BackColor
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns></returns>

		public Color GetCellBackColor(
			int gr,
			int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return Color.Empty;

			if (ci.DataValue is MobiusDataType)
				return ((MobiusDataType)ci.DataValue).BackColor;
			else if (ci.DataValue is Color) return (Color)ci.DataValue;
			else return Color.Empty;
		}

		/// <summary>
		/// Set Hyperlink of cell
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="Hyperlink"></param>

		public void SetCellHyperlink(
			int gr,
			int gc,
			string hyperlink)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			if (!ci.IsValidDataCell) return;

			if (ci.DataValue is MobiusDataType)
			{
				((MobiusDataType)ci.DataValue).Hyperlink = hyperlink;
				RefreshRowCell(gr, V.Columns[gc]);
			}
		}

		/// <summary>
		/// Redraw a cell
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="gridCol"></param>

		internal void RefreshRowCell(int rowHandle, GridColumn gridCol)
		{
			if (MainView is BandedGridView)
				(MainView as BandedGridView).RefreshRowCell(rowHandle, gridCol);

			else if (MainView is LayoutView) (MainView as LayoutView).RefreshRow(rowHandle);
		}

		/// <summary>
		/// Get text value for a cell
		/// </summary>
		/// <param name="gr"></param>
		/// <param name="gc"></param>
		/// <returns></returns>

		public string GetCellText(
			int gr,
			int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			return GetCellText(ci);
		}

		/// <summary>
		/// Get text value for a cell
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>

		public string GetCellText(
			CellInfo ci)
		{
			if (!ci.IsValidDataCell) return "";

			else if (NullValue.IsNull(ci.DataValue)) return "";

			else if (ci.DataValue is MobiusDataType)
				return ((MobiusDataType)ci.DataValue).FormattedText;

			else return ci.DataValue.ToString();
		}

		/// <summary>
		/// Get any hyperlink associated with last cell mouse was down in
		/// </summary>
		/// <returns></returns>

		public string GetCellHyperlinkLastMouseDownCell()
		{
			string uri = GetCellHyperlink(LastMouseDownRowIdx, LastMouseDownCol);
			return uri;
		}

		/// <summary>
		/// Get any hyperlink associated with a cell
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns></returns>

		public string GetCellHyperlink(
			int gr,
			int gc)
		{
			CellInfo ci = GetCellInfo(gr, gc);
			return GetCellHyperlink(ci);
		}

		/// <summary>
		/// Get any hyperlink associated with a cell
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>

		public string GetCellHyperlink(
			CellInfo ci)
		{
			if (!ci.IsValidDataCell) return "";

			if (ci.DataValue is MobiusDataType)
			{
				MobiusDataType mdt = ci.DataValue as MobiusDataType;
				if (mdt.IsNull || !mdt.Hyperlinked) return "";

				string hyperlink = mdt.Hyperlink; // get any preformatted hyperlink (e.g. for annotation tables)
				if (Lex.IsNullOrEmpty(hyperlink)) // if not defined 
				{
					ResultsFormatter fmtr = QueryManager.ResultsFormatter;
					hyperlink = fmtr.FormatHyperlink(ci);
				}
				return (hyperlink);
			}

			else return "";
		}

		/// <summary>
		/// Get basic cell information given a grid row index (rowHandle) and grid column
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <returns></returns>

		public CellInfo GetCellInfo(
				int rowHandle,
				int colAbsoluteIndex)
		{
			GridColumn gc = null;

			if (colAbsoluteIndex >= 0)
				gc = V.Columns[colAbsoluteIndex];

			return GetGridCellInfo(rowHandle, gc);
		}

		/// <summary>
		/// Get basic cell information given a grid row index (rowHandle) and column index (colAbsoluteIndex)
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="gridCol"></param>
		/// <returns></returns>

		public CellInfo GetGridCellInfo(
			int rowHandle,
			GridColumn gridCol)
		{
			CellInfo ci = new CellInfo();
			if (gridCol != null) GetColumnInfo(gridCol, ci);

			ci.GridRowHandle = rowHandle;
			ci.DataRowIndex = V.GetDataSourceRowIndex(rowHandle);
			GetCellInfoDataValue(ci);

			return ci;
		}

		/// <summary>
		/// Get basic cell information given a DataTable row index and grid column index (colAbsoluteIndex)
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="gridCol"></param>
		/// <returns></returns>

		public CellInfo GetDataTableCellInfo(
			int rowIndex,
			GridColumn gridCol)
		{
			CellInfo ci = new CellInfo();
			if (gridCol != null) GetColumnInfo(gridCol, ci);

			ci.DataRowIndex = rowIndex;
			GetCellInfoDataValue(ci);

			return ci;
		}

		/// <summary>
		/// Pick up the cell data value
		/// </summary>
		/// <param name="ci"></param>

		void GetCellInfoDataValue(CellInfo ci)
		{
			if (ci.DataColIndex >= 0 && ci.DataColIndex < DataTable.Columns.Count &&
			 ci.DataRowIndex >= 0 && ci.DataRowIndex < DataTable.Rows.Count)
			{ // check for within range of DataTable
				DataRowMx dr = DataTable.Rows[ci.DataRowIndex];
				ci.DataValue = dr[ci.DataColIndex];
				if (NullValue.IsNull(ci.DataValue) && ci.Mc != null && ci.Mc.IsKey)
				{ // if key val & row after first row for key then try to get value from first row for key
					DataRowAttributes dra = Dtm.GetRowAttributes(dr);
					if (dra != null && dra.FirstRowForKey >= 0)
					{
						dr = DataTable.Rows[dra.FirstRowForKey];
						ci.DataValue = dr[ci.DataColIndex];
					}
				}
			}
		}

		/// <summary>
		/// Get bounding rectangle for cell
		/// </summary>
		/// <param name="view"></param>
		/// <param name="rowHandle"></param>
		/// <param name="column"></param>
		/// <returns></returns>

		internal Rectangle GetCellRect(int rowHandle, int colAbsoluteIndex)
		{
			if (BGV != null)
			{
				GridViewInfo info = BGV.GetViewInfo() as GridViewInfo;
				GridCellInfo cell = info.GetGridCellInfo(rowHandle, BGV.Columns[colAbsoluteIndex]);
				if (cell != null)
					return cell.Bounds;
			}

			return Rectangle.Empty;
		}

		/// <summary>
		/// Get the metadata associated with a grid column
		/// </summary>
		/// <param name="gridColIdx">Grid column</param>
		/// <param name="ci">Cell metadata for column</param>
		/// <returns>True if found information</returns>

		public bool GetMetaDataForGridCol(
			int gridColIdx,
			ColumnInfo ci)
		{
			ResultsField rfld = null;

			ResultsFormat rf = QueryManager.ResultsFormat;

			if (gridColIdx < 0 || rf.GridColumnIndexToResultsField == null || 
				gridColIdx >= rf.GridColumnIndexToResultsField.Count) return false;

			rfld = rf.GridColumnIndexToResultsField[gridColIdx];

			rfld = rf.GridColumnIndexToResultsField[gridColIdx];
			if (rfld?.GridColumn == null || rfld.GridColumn.AbsoluteIndex != gridColIdx) return false;

			ci.Rt = rfld.ResultsTable;
			ci.Rfld = rfld;
			ci.FieldIndex = rfld.FieldPosition; // fi;
			ci.Qt = ci.Rt.QueryTable;
			ci.Qc = rfld.QueryColumn;
			ci.Mt = ci.Qt.MetaTable;
			ci.Mc = ci.Qc.MetaColumn;
			ci.TableIndex = ci.Qt.TableIndex; // ti;
			return true;
		}

		/// <summary>
		/// Get the metacolumn type associated with a column
		/// </summary>
		/// <param name="col"></param>
		/// <returns></returns>

		public MetaColumnType GetMetaColumnDataType(
			int col)
		{
			ColumnInfo ci = GetColumnInfo(V.Columns[col]);
			if (ci != null && ci.Mc != null) return ci.Mc.DataType;
			else return MetaColumnType.Unknown;
		}

		/// <summary>
		/// Select cell with focus
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="colHandle"></param>

		public void FocusCell(
		int rowHandle,
		int colHandle)
		{
			V.FocusedRowHandle = rowHandle;
			V.FocusedColumn = V.Columns[colHandle];
		}

		/// <summary>
		/// Start editing specified cell
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="colHandle"></param>

		public void EditCell(
		int rowHandle,
		int colHandle)
		{
			V.FocusedRowHandle = rowHandle;
			V.FocusedColumn = V.Columns[colHandle];
			V.ShowEditor();
		}

		/// <summary>
		/// Return true if any column in the specified band is selected
		/// </summary>
		/// <param name="gb"></param>
		/// <returns></returns>

		public bool AnyBandColumnSelected(GridBand gb)
		{
			GridCell[] cells;
			int topRow, bottomRow, ai;
			GridColumn leftCol, rightCol;

			GetSelectedCells(out cells, out topRow, out bottomRow, out leftCol, out rightCol);
			if (leftCol == null) return false;

			for (ai = leftCol.AbsoluteIndex; ai <= rightCol.AbsoluteIndex; ai++)
			{
				BandedGridColumn bgc = BGV.Columns[ai];
				if (bgc.OwnerBand == gb) return true;
			}

			return false;
		}

		/// <summary>
		/// Return true if all columns in the specified band is selected
		/// </summary>
		/// <param name="gb"></param>
		/// <returns></returns>

		public bool AllBandColumnsSelected(GridBand gb)
		{
			foreach (BandedGridColumn bgc in gb.Columns)
			{
				if (!ColumnSelected(bgc)) return false;
			}

			return true;
		}

		/// <summary>
		/// Return true if 1st row of column is selected
		/// </summary>
		/// <param name="gc"></param>
		/// <returns></returns>

		public bool ColumnSelected(GridColumn gc)
		{
			GridCell[] cells = GetSelectedCells();
			foreach (GridCell c in cells)
			{
				if (c.RowHandle == 0 && c.Column.AbsoluteIndex == gc.AbsoluteIndex)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Return true if all cells in column are selected
		/// </summary>
		/// <param name="gc"></param>
		/// <returns></returns>

		public bool AllColumnCellsSelected(GridColumn gc)
		{
			if (BGV == null) return false;

			for (int ri = 0; ri < BGV.RowCount; ri++)
			{
				if (!BGV.IsCellSelected(ri, gc)) return false;
			}

			return true;
		}

		/// <summary>
		/// Select cells for a grid band (i.e. Data Table)
		/// </summary>
		/// <param name="band"></param>

		public void SelectCells(GridBand band)
		{
			if (Control.ModifierKeys != Keys.Shift && Control.ModifierKeys != Keys.Control)
				V.ClearSelection();

			if (Control.ModifierKeys == Keys.Control && AllBandColumnsSelected(band))
			{ // if band already selected & Control key down then deselect the band
				foreach (BandedGridColumn column in band.Columns)
				{
					DeselectCells(column);
				}
			}

			else // add the band
			{
				foreach (BandedGridColumn column in band.Columns)
				{
					SelectCells(column, true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gc"></param>
		public void SelectCells(
				GridColumn gc)
		{
			if (Control.ModifierKeys == Keys.Control && AllColumnCellsSelected(gc))
				DeselectCells(gc);

			else SelectCells(gc, false);
			return;
		}

		/// <summary>
		/// Select cells for a grid column
		/// </summary>
		/// <param name="gc"></param>
		/// <param name="alwaysExtend"></param>

		public void SelectCells(
			GridColumn gc,
			bool alwaysExtend)
		{
			GridCell[] cells;
			int topRow, bottomRow, ai0, ai1;
			GridColumn leftCol, rightCol;

			GridCell prevLastSelectionMouseDown = LastSelectionMouseDown;
			LastSelectionMouseDown = new GridCell(0, gc);

			GetSelectedCells(out cells, out topRow, out bottomRow, out leftCol, out rightCol);

			if (cells != null && cells.Length > 0)
			{ // existing selection

				ai1 = gc.AbsoluteIndex;
				if (Control.ModifierKeys == Keys.Shift) // extending selection via shift key
				{
					if (prevLastSelectionMouseDown != null && prevLastSelectionMouseDown.Column != null)
					{ // know column of last selection
						ai0 = prevLastSelectionMouseDown.Column.AbsoluteIndex;
					}

					else
					{
						if (ai1 < leftCol.AbsoluteIndex)
						{
							ai0 = ai1;
							ai1 = rightCol.AbsoluteIndex;
						}

						else if (ai1 > rightCol.AbsoluteIndex)
						{
							ai0 = leftCol.AbsoluteIndex;
						}
						else
						{
							ai0 = leftCol.AbsoluteIndex;
							ai1 = rightCol.AbsoluteIndex;
						}
					}

					for (int ai = ai0; ai <= ai1; ai++) // extend to range of cols
						ExtendSelection(0, V.RowCount - 1, ai0, ai1);

					return;
				}

				else if (Control.ModifierKeys == Keys.Control || alwaysExtend) // extending selection via control key
				{
					ExtendSelection(0, V.RowCount - 1, gc.AbsoluteIndex, gc.AbsoluteIndex);
					return;
				}
			}

			// Default: select the full column

			V.ClearSelection();
			ExtendSelection(0, V.RowCount - 1, gc.AbsoluteIndex, gc.AbsoluteIndex);
		}

		/// <summary>
		/// Deselect cells for a grid column
		/// </summary>
		/// <param name="gc"></param>

		public void DeselectCells(
			GridColumn gc)
		{
			if (BGV == null) return;

			GridCell[] cells = GetSelectedCells();

			BGV.ClearSelection();

			List<GridCell> cellList = new List<GridCell>();

			BeginUpdate();
			foreach (GridCell c in cells)
			{
				if (c.Column.AbsoluteIndex != gc.AbsoluteIndex)
					BGV.SelectCell(c);
			}
			EndUpdate();
			return;
		}

		/// <summary>
		/// Extend the current selection with a range of cells
		/// </summary>
		/// <param name="row"></param>
		/// <param name="rowSel"></param>
		/// <param name="col"></param>
		/// <param name="colSel"></param>

		public void ExtendSelection(
		int startRowHandle,
		int endRowHandle,
		int startColIndex,
		int endColIndex)
		{
			try
			{
				if (V is BandedGridView)
					(V as BandedGridView).SelectCells(startRowHandle, V.Columns[startColIndex], endRowHandle, V.Columns[endColIndex]);
			}
			catch (Exception ex) { string msg = ex.Message; }
		}

		/// <summary>
		/// Select a single cell within the view
		/// </summary>
		/// <param name="rowHandle"></param>
		/// <param name="colAbsoluteIndex"></param>

		public void SelectCell(
			int rowHandle,
			int colAbsoluteIndex)
		{
			if (rowHandle < 0 || colAbsoluteIndex < 0) return;

			V.ClearSelection();
			if (V is BandedGridView)
				BGV.SelectCell(rowHandle, V.Columns[colAbsoluteIndex]);

			else if (V is LayoutView)
				LV.SelectRow(rowHandle);

			return;
		}

		/// <summary>
		/// Handle mouse interaction with cells
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void MoleculeGridControl_MouseDown(object sender, MouseEventArgs e)
		{
			QueryTable qt = null;
			QueryColumn qc = null;
			MetaColumn mc = null;
			MetaColumnType colType = MetaColumnType.Unknown;
			Point p;
			Rectangle rect;
			SortOrder sortDir;
			int i1;
			string uri = "";

			if (SystemUtil.InDesignMode || // just return if in design mode
				Mode == MoleculeGridMode.IgnoreEvents) // just return if ignoring events
				return;

			GetMouseEventInfo(e);

			if (MouseDownCallback != null)
			{
				MouseDownCallback(sender, e);
				return;
			}

			HitInfo hi = LastMouseHitInfo;
			int gri = LastMouseDownRowIdx;
			int gci = LastMouseDownCol;
			GridColumn gc = LastMouseDownGridCol;
			CellInfo ci = LastMouseDownCellInfo;

			if (ci?.Mc != null) // Get basic info on any MetaColumn clicked on
			{
				qc = ci?.Qc;
				mc = ci?.Mc;
				colType = mc.DataType;
			}

			// Clicked in band (i.e. data table header)

			if (hi.InBandPanel)
			{
				if (hi.Band == null || !(hi.Band.Tag is QueryTable)) return;
				if (Mode != MoleculeGridMode.QueryView) return;
				qt = hi.Band.Tag as QueryTable;

				if (e.Button == MouseButtons.Left) // left click
					SelectCells(hi.Band);

				else // right-click
				{
					if (!AnyBandColumnSelected(hi.Band)) SelectCells(hi.Band); // select band if not already selected
					ShowTableHeaderPopup(qt, e.X, e.Y);
				}

				return;
			}

			// Click in column header area

			else if (hi.InColumn)
			{
				GridViewInfo viewInfo = V.GetViewInfo() as GridViewInfo;
				GridColumnInfoArgs colInfo = GetGridColumnInfoArgs(gc);

				rect = colInfo.Bounds;
				int left = rect.Left;
				if (GV != null && gc.Fixed == FixedStyle.Left) // if grid view && not fixed col then adjust for any horiz scrolling
					left -= GV.LeftCoord;
				p = new Point(left, rect.Bottom); // where to put menu

				// Filter button clicked (not active)

				if (hi.HitTest == BandedGridHitTest.ColumnFilterButton)
				{
					DialogResult dr = Dtm.CompleteRetrieval(); // be sure we have full data before filtering
					FilterDialog.Edit(ci?.GridColumn);
					return;
				}

				// Click in CheckMark column header

				if (Lex.Eq(ci.GridColumn.FieldName, "CheckMark"))
				{
					Helpers.ShowMarkedDataContextMenu(this, p);
					return;
				}

				// Click in data column header

				else
				{
					if (e.Button == MouseButtons.Left || colInfo == null) // left click
					{
						SelectCells(gc);
						return;
					}

					else // right-click
					{

						if (!ColumnSelected(gc)) SelectCells(gc); // select column if not already selected

						if (Mode == MoleculeGridMode.QueryView) // full menu
						{
							QueryTableControl.SetupColumnFormattingContextMenu(Helpers.ColumnHeaderContextMenu, qc, Helpers.UseNamedCfMenuItem_Click);

							Helpers.FilterMenuItem.Enabled = mc.IsSearchable || !mc.IsGraphical;
							Helpers.SortAscendingMenuItem.Enabled = mc.IsSortable;
							Helpers.SortDescendingMenuItem.Enabled = mc.IsSortable;
							Helpers.SortMultipleMenuItem.Enabled = mc.IsSortable;
							Helpers.ColumnFormatMenuItem.Enabled = mc.IsFormattable;

							Helpers.ColumnHeaderContextMenu.Show(this, p);
						}

						else if (Mode == MoleculeGridMode.DataSetView) // limited menu
						{
							Helpers.SortAscendingMenuItem2.Enabled = mc.IsSortable;
							Helpers.SortDescendingMenuItem2.Enabled = mc.IsSortable;
							Helpers.SortMultipleMenuItem2.Enabled = mc.IsSortable;

							//Helpers.ColumnFormatMenuItem2.Enabled = mc.IsNumeric;
							Helpers.ColumnFormatMenuItem2.Enabled = mc.IsFormattable;

							Helpers.ColumnHeaderContextMenu2.Show(this, p);
						}
					}
				}

				return;
			}

			// Clicked in row indicator

			else if (hi.HitTest == BandedGridHitTest.RowIndicator)
			{ // need to select?
				if (e.Button == MouseButtons.Right)
				{
					bool rowInsDelAllowed = false;

					if (Helpers.Grid.Mode == MoleculeGridMode.DataSetView && // only allow row add/delete in dsviewer for now
						Helpers.Grid.Mode == MoleculeGridMode.LocalView) rowInsDelAllowed = true;

					Helpers.RowInsertDeleteMenuItemSeparator.Visible = rowInsDelAllowed;
					Helpers.RowMenuInsertRowMenuItem.Visible = rowInsDelAllowed;
					Helpers.RowMenuDeleteRowMenuItem.Visible = rowInsDelAllowed;

					p = new Point(e.X, e.Y);
					Helpers.RowContextMenu.Show(this, p);
				}
				return;
			}

			// Clicked in data cell associated with a MetaColumn

			else if (mc != null)
			{
				SelectedCid = "";
				uri = GetCellHyperlink(gri, gci);
				if (!String.IsNullOrEmpty(uri)) // see if cell contains a compoundId
				{
					string tok = "CompoundIdContextMenu";
					i1 = uri.IndexOf(tok);
					if (i1 >= 0)
					{
						string txt = uri.Substring(i1 + tok.Length + 1);
						txt = txt.Replace("\"", ""); // remove any extra double quote
						SelectedCid = txt; // cache cmpdno here for later pickup
					}
				}

				// Left mouse button actions

				if (e.Button == MouseButtons.Left)
				{
					if (e.Clicks == 1) // if single click jump to any link (show link menu if cid/key) or just select if no link
					{
						if (SelectionChangedCallback != null)
							SelectionChangedCallback(sender, e);

						return; // handle other details in click event
					}

				} // end of left button click

				// Right mouse button actions

				else if (e.Button == MouseButtons.Right)
				{
					Focus(); // be sure focus is here (right click doesn't set focus)

					// Clicked in band header (i.e. data table header)

					if (hi.InBandPanel)
					{
						if (hi.Band == null || !(hi.Band.Tag is QueryTable)) return;
						if (Mode != MoleculeGridMode.QueryView) return;
						qt = hi.Band.Tag as QueryTable;

						if (e.Button == MouseButtons.Left) // left click
							SelectCells(hi.Band);

						else // right-click
						{
							if (!AnyBandColumnSelected(hi.Band)) SelectCells(hi.Band); // select band if not already selected
							ShowTableHeaderPopup(qt, e.X, e.Y);
						}

						return;
					}


					if (gri >= V.RowCount) return; // within grid?
					if (gci >= V.Columns.Count) return;

					// if (selected cell outside of current selection then reset selection

					GridCell[] cells = GetSelectedCells();
					if (cells != null)
					{
						for (i1 = 0; i1 < cells.Length; i1++)
						{
							if (cells[i1].RowHandle == gri &&
								cells[i1].Column.AbsoluteIndex == gci) break;
						}
						if (i1 >= cells.Length) SelectCell(gri, gci);
					}

					Helpers.ShowCellRightClickContextMenu(ci, e);
				}
			}

//  Clicked somewhere else in the grid

			else return;
		}

		public void GetMouseEventInfo(MouseEventArgs e)
		{
			GridColumn gc = null;
			int gci, gri;
			QueryTable qt;

			HitInfo hi = CalcHitInfo(e.Location);
			LastMouseHitInfo = hi;

			LastMouseDownEventArgs = e;
			LastMouseDownRowIdx = -1;
			LastMouseDownCol = -1;
			LastMouseDownGridCol = null;
			LastMouseDownCellInfo = null;

			LastMouseDownRowIdx = gri = hi.RowHandle;

			// Clicked in row indicator column

			if (hi.HitTest == BandedGridHitTest.RowIndicator)
				return;

			// Clicked in band header (i.e. data table header)

			else if (hi.InBandPanel)
				return; 

			else
			{
				if (hi.Column == null || (!hi.InColumn && !hi.InRowCell)) // ignore if in grid "whitespace"
				{
					return;
				}
				gc = hi.Column;
				gci = gc.AbsoluteIndex;
			}

			LastMouseDownRowIdx = gri;
			LastMouseDownCol = gci;
			LastMouseDownGridCol = gc;
			LastMouseDownCellInfo = GetCellInfo(gri, gci);
			return;
		}

		/// <summary>
		/// Open a hyperlink within a Html cell that has been clicked for HypertextLabel control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void OpenClickedHyperlinkWithinHtml(object sender, OpenHyperlinkEventArgs e)
		{
			string uri = e.Link;
			e.Handled = true;
			ProcessHyperlinkClick(uri, LastMouseDownCellInfo);
			return;
		}

		/// <summary>
		/// Catch RepositoryItemRichTextEdit HyperlinkClick event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void GridView_ShownEditor(object sender, EventArgs e)
		{
			RichTextEdit activeEditor = GV?.ActiveEditor as RichTextEdit;
			if (activeEditor != null)
			{
				RichEditControl richEditControl = (RichEditControl)activeEditor.Controls[0];
				richEditControl.HyperlinkClick += RichEditControl_HyperlinkClick;
			}

		}

		/// <summary>
		/// Open a hyperlink within a Html cell that has been clicked for RichEdit control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void RichEditControl_HyperlinkClick(object sender, DevExpress.XtraRichEdit.HyperlinkClickEventArgs e)
		{
			string uri = e.Hyperlink.NavigateUri;
			e.Handled = true;
			ProcessHyperlinkClick(uri, LastMouseDownCellInfo);
			return;
		}


		/// <summary>
		/// Get the view info for the specified grid column
		/// </summary>
		/// <param name="gc"></param>
		/// <returns></returns>

		GridColumnInfoArgs GetGridColumnInfoArgs(GridColumn gc)
		{
			if (gc == null) return null;
			GridViewInfo viewInfo = V.GetViewInfo() as GridViewInfo;
			if (viewInfo == null) return null;

			foreach (GridColumnInfoArgs gcia in viewInfo.ColumnsInfo)
			{ // only visible columns are in viewInfo.ColumnsInfo so look for column match
				if (gcia.Column == gc) return gcia;
			}

			return null;
		}

		/// <summary>
		/// Mouse up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void MoleculeGridControl_MouseUp(object sender, MouseEventArgs e)
		{
			if (MouseUpCallback != null)
			{
				MouseUpCallback(sender, e);
				return;
			}

			return;
		}

		/// <summary>
		/// Mouse click - handle left clicks here so we know we've already processed the MouseUp event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MoleculeGridControl_MouseClick(object sender, MouseEventArgs e)
		{

			if (MouseClickCallback != null)
			{
				MouseClickCallback(sender, e);
				return;
			}

			if (e.Button != MouseButtons.Left) return; // handle left button clicks only

			HitInfo hi = CalcHitInfo(e.Location);
			if (hi.InBandPanel) return; // ignore if click is in header

			CellInfo ci = LastMouseDownCellInfo;
			if (ci == null) return;

			if (IsHtmlColumn(ci))
				return; // just return, don't edit here to allow hyperlinks to be clicked, must use rt-click to edit cell

			if (Editor != null && !UpdateImmediately) return; // don't do single click hyperlinks if in UserDataEditor

			Keys keys = Control.ModifierKeys;
			if (keys == Keys.Shift || keys == Keys.Control)
			{ } // don't process hyperlink if shift or control is down

			else if (ProcessHyperlinkClick(ci, false)) return; // try to process any hyperlinks

			if (ci.GridColumn != null && Lex.Eq(ci.GridColumn.FieldName, "CheckMark"))
			{
				if (ci.DataRowIndex < 0) return; // ignore if not a real row 
				CheckMarksChanged = true;
				//if (BGV != null && BGV.OptionsSelection.MultiSelect && 
				// BGV.OptionsSelection.MultiSelectMode == GridMultiSelectMode.CellSelect)
				{ // switch the value here since it takes 3 clicks for the control to respond for this selection mode
					bool currentMark = Dtm.RowIsMarked(ci.DataRowIndex);
					Dtm.SetRowMark(ci.DataRowIndex, !currentMark);
					Application.DoEvents();
				}
			}
			return;
		}

		bool IsHtmlColumn(CellInfo ci)
		{
			RepositoryItemRichTextEdit rte = (ci?.GridColumn?.ColumnEdit as RepositoryItemRichTextEdit); // full HTML tags
			RepositoryItemHypertextLabel hti = (ci?.GridColumn?.ColumnEdit as RepositoryItemHypertextLabel); // limited HTML tags
			if (rte != null || hti != null)
				return true;
			else return false;
		}

		/// <summary>
		/// Process a hyperlink click
		/// </summary>
		/// <param name="cInf"></param>
		/// <param name="fromContextMenu"></param>
		/// <returns></returns>

		internal bool ProcessHyperlinkClick(CellInfo cInf, bool fromContextMenu = false)
		{
			string uri = GetCellHyperlink(LastMouseDownRowIdx, LastMouseDownCol);
			return ProcessHyperlinkClick(uri, cInf, fromContextMenu);
		}

		/// <summary>
		/// Process a hyperlink click
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="cInf"></param>
		/// <param name="fromContextMenu"></param>
		/// <returns></returns>

		internal bool ProcessHyperlinkClick(string uri, CellInfo cInf, bool fromContextMenu = false)
		{
			string arg;

			if (cInf == null) return false;

			int r = cInf.GridRowHandle;
			int c = cInf.GridColAbsoluteIndex;

			int x = LastMouseDownEventArgs.X;
			int y = LastMouseDownEventArgs.Y;

			if (String.IsNullOrEmpty(uri)) return false;

			if (!fromContextMenu && cInf.Mc != null && // avoid having single left-click on structure do a link
			 cInf.Mc.DataType == MetaColumnType.Structure && cInf.Mc.ClickFunction == "")
				return false;

			try
			{
				if (SelectedCid != "" && r >= 0)
					Helpers.ShowCidClickContextMenu(cInf);

				else if (uri.ToLower().IndexOf("mobius/command") > 0) // new command
				{
					int i1 = uri.IndexOf("?");
					string cmd = uri.Substring(i1 + 1);
					if (cmd.EndsWith("\""))
						cmd = cmd.Substring(0, cmd.Length - 1);

					i1 = cmd.IndexOf("\">"); // trim any trailing html tag
					if (i1 > 0) cmd = cmd.Substring(0, i1);

					if (IsShowContextMenuCommand(cmd, "ShowContextMenu:TargetContextMenu", out arg))
					{
						SelectedTarget = arg;
						Helpers.TargetContextMenu.Show(this,
							new System.Drawing.Point(x, y));
					}

					else if ((i1 = Lex.IndexOf(cmd, "ClickFunction")) >= 0)
					{
						cmd = cmd.Substring(i1 + "ClickFunction".Length + 1); // get function name
						ClickFunctions.Process(cmd, QueryManager, cInf);
					}

					else ResultsFormatter.ProcessCommonDisplayResponses(cmd);
				}

				else SystemUtil.StartProcess(uri); // direct uri open
			}

			catch (Exception ex)
			{
				XtraMessageBox.Show("Cannot open the hyperlink " + Lex.Dq(uri), UmlautMobius.String);
			}

			return true;
		}

		/// <summary>
		/// Check for command to show context menu 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cmdName"></param>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static bool IsShowContextMenuCommand(
			string cmd,
			string cmdName,
			out string arg)
		{
			string tok;

			arg = "";
			int pos = cmd.IndexOf(cmdName, StringComparison.OrdinalIgnoreCase);
			if (pos < 0) return false;

			string args = cmd.Substring(pos + cmdName.Length);
			Lex lex = new Lex(", : ( )");
			lex.OpenString(args);
			arg = lex.Get();
			if (arg == ":" || arg == "(") arg = lex.Get();
			arg = Lex.RemoveAllQuotes(arg);
			return true;
		}

		/// <summary>
		/// Show table header popup menu
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>

		public void ShowTableHeaderPopup(
			QueryTable qt,
			int x,
			int y)
		{
			LastMouseDownQueryTable = qt;
			MetaTable mt = qt.MetaTable;

			Helpers.ShowTableDescriptionMenuItem.Enabled = mt.DescriptionIsAvailable();
			Helpers.AddTableToQueryMenuItem.Enabled = !QbUtil.BaseQueryContainsMetaTable(mt.Name);
			Helpers.TableHeaderlContextMenu.Show(this,
				new System.Drawing.Point(x, y));
		}

		/// <summary>
		/// Cut selected data
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>

		public void CutCommand(
			int gr,
			int gc)
		{
			CopyCutDeleteCommand(gr, gc, true, true);
		}

		/// <summary>
		/// Copy to clipboard
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>

		public void CopyCommand(
			int gr,
			int gc)
		{
			CopyCutDeleteCommand(gr, gc, true, false);
		}

		/// <summary>
		/// Delete data in specified cell
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>

		public void DeleteCommand(
			int r,
			int c)
		{
			CopyCutDeleteCommand(r, c, false, true);
		}

		/// <summary>
		/// Copy, cut and delete current selection command
		/// </summary>
		/// <param name="gr"></param>
		/// <param name="gc"></param>
		/// <param name="copy"></param>
		/// <param name="delete"></param>

		public void CopyCutDeleteCommand(
			int gr,
			int gc,
			bool copy,
			bool delete)
		{
			try
			{
				GridCell cell;
				string txt, errMsg;

				// Multiple cell copy (delete not allowed)

				GridCell[] cells = GetSelectedCells();
				int[] rows = V.GetSelectedRows();
				if (cells != null && cells.Length > 1 && rows != null && rows.Length > 0) // copy multiple cells
				{
					if (delete)
					{
						MessageBoxMx.ShowError("Cannot cut or delete a range of cells");
						return;
					}

					int headerLines = 0;
					//if (ci.GridRowHandle == 0)
					//{
					string prompt =
						"Enter the number of header lines (0 - 2)\n" +
						"you would like to have copied to the clipboard\n" +
						"along with the selected cells.";

					if (Lex.IsNullOrEmpty(HeaderLinesString)) HeaderLinesString = "0";
					HeaderLinesString = InputBoxMx.Show(prompt, "Include Header Cells", HeaderLinesString);
					try { headerLines = Convert.ToInt32(HeaderLinesString); }
					catch (Exception ex) { return; }
					if (headerLines < 0) headerLines = 0;
					if (headerLines > 2) headerLines = 2;
					//}

					StringBuilder sb = new StringBuilder();

					if (headerLines > 0) // include header lines
					{
						string bh = ""; // band headers
						string ch = ""; // column headers
						for (int cidx = 0; cidx < cells.Length; cidx++)
						{
							cell = cells[cidx];
							GridColumn cc = cell.Column;
							if (cell.RowHandle != cells[0].RowHandle) break;

							if (cidx > 0) { bh += "\t"; ch += "\t"; }
							ch += cc.Caption;
							if (cc is BandedGridColumn)
							{
								BandedGridColumn bcc = cc as BandedGridColumn;
								if (bcc.OwnerBand != null) bh += bcc.OwnerBand.Caption;
							}
						}
						if (headerLines == 2) sb.Append(bh + "\r\n"); // include band headers if requested
						sb.Append(ch); // include column headers
					}

					int ri = -1;
					for (int cidx = 0; cidx < cells.Length; cidx++)
					{
						cell = cells[cidx];
						if (cell.RowHandle != ri && sb.Length > 0) sb.Append("\r\n"); // need \r\n so list pastes properly into criteria list, etc
						ri = cell.RowHandle;
						txt = V.GetRowCellValue(cell.RowHandle, cell.Column) as string;
						if (txt != null)
						{
							txt = txt.Replace("\r", ""); // remove any newline info from text
							txt = txt.Replace("\n", "");
							sb.Append(txt);
						}
						sb.Append("\t"); // tab between values
					}

					Clipboard.SetDataObject(sb.ToString(), true); // copy to clipboard

					return;
				}

				// Single cell copy or delete

				CellInfo ci = GetCellInfo(gr, gc);
				if (ci.Mc == null) return;
				if (NullValue.IsNull(ci.DataValue)) return;

				if (delete && !UserData.CanModifyTable(ci.Mt))
				{
					MessageBoxMx.ShowError("You are not authorized to modify this data.");
					return;
				}

				if (copy)
				{
					if (ci.Mc.DataType == MetaColumnType.Structure)
					{ // copy structure to clipboard
						MoleculeMx cs = ci.DataValue as MoleculeMx;
						MoleculeControl.CopyMoleculeToClipboard(cs);

						//if (cs == null) return;
						//MoleculeMx.SetRenderStructure(CopyPasteHiddenControl, cs);
						//if (CopyPasteHiddenControl.CanCopy)
						//	CopyPasteHiddenControl.CopyToClipboard();
					}

					else if (ci.Mc.DataType == MetaColumnType.Image)
					{ // copy image
						Image i = GetCellImage(gr, gc);
						if (i == null) return;
						Clipboard.SetImage(i);
					}

					else // copy formatted text representation of cell
					{
						txt = V.GetRowCellValue(ci.GridRowHandle, ci.GridColumn) as string;
						if (ci.Mt.IsAnnotationTable) // include hyperlink with text if annotation table
						{
							string uri = GetCellHyperlink(ci.GridRowHandle, ci.GridColAbsoluteIndex);
							if (!String.IsNullOrEmpty(uri) && // cell contain a link?
							 !Lex.Contains(uri, "Mobius/command?")) // not an internally-generated Mobius link?
								txt = txt + "\v" + uri; // include uri after text separated by vertical tab
						}
						Clipboard.SetData(DataFormats.Text, txt);
					}
				}

				if (delete)
				{
					GetEditor().SetCellValue(ci, null); // null the value
					RefreshRowCell(gr, ci.GridColumn);
				}

			}
			catch (Exception ex) { MessageBoxMx.ShowError(ex.Message); }
		}

		/// <summary>
		/// Get the grid editor, allocating if necessary
		/// </summary>
		/// <returns></returns>

		internal UserDataEditor GetEditor()
		{
			if (Editor == null) // create editor if needed
				Editor = new UserDataEditor();

			Editor.QueryManager = QueryManager; // be sure we have the current QueryManager associated with the grid
			return Editor;
		}

		/// <summary>
		/// Edit a cell that contains Html
		/// </summary>
		/// <param name="ci"></param>

		internal void EditHtmlTextCellValue(CellInfo ci)
		{
			string html = "", html2 = "";

			QueryColumn qc = ci?.Qc;
			if (qc == null) return;

			object v = ci.DataValue;

			if (v == null || v.GetType().Equals(typeof(DBNull))) { }

			else if (v.GetType().Equals(typeof(string)))
			{
				html = (string)v;
			}

			else if (v.GetType().Equals(typeof(StringMx)))
			{
				html = ((StringMx)v).Value;
			}

			else html = "";

			string label = "Edit " + qc.ActiveLabel + " HTML";
			if (UseRichHtmlColumnRendering)
				html2 = EditHtmlRich.ShowDialog(html);

			else html2 = EditHtmlBasic.ShowDialog(html);

			if (html2 != null)
			{
				StringMx s2 = new StringMx(html2);
				GetEditor().SetCellValue(ci, s2); // store in DataTable & any assoc Oracle table
				RefreshDataSource(); // force redraw of grid to set proper row height for structure
			}
		}


		/// <summary>
		/// Paste from clipboard
		/// </summary>
		/// <param name="gr"></param>
		/// <param name="gc"></param>

		public void PasteCommand(
			int gr,
			int gc)
		{
			try
			{
				gr = Helpers.MakeGridRowReal(gr);

				CellInfo ci = GetCellInfo(gr, gc);

				if (!UserData.CanModifyTable(ci.Mt))
				{
					MessageBoxMx.ShowError("You are not authorized to modify this data.");
					return;
				}

				MobiusDataType mdt = ci.DataValue as MobiusDataType;
				if (mdt == null) mdt = MobiusDataType.New(ci.Mc.DataType); // allocate if necessary
				if (ci.Mc == null) return;
				DataRowMx dr = DataTable.Rows[ci.DataRowIndex];

				GridCell[] cells = GetSelectedCells();
				int[] rows = V.GetSelectedRows();
				if (cells != null && cells.Length > 1 && rows != null && rows.Length > 0) // copy rows
				{
					MessageBoxMx.ShowError("Cannot paste into a range of cells");
					return;
				}

				if (ci.Mc.DataType == MetaColumnType.Structure)
				{ // paste structure
					MoleculeMx mol = MoleculeControl.GetMoleculeFromClipboard();
					if (MoleculeMx.IsDefined(mol))
						GetEditor().SetCellValue(ci, mol);
				}

				else if (ci.Mc.DataType == MetaColumnType.Image)
				{ // paste image
					Bitmap bm = Clipboard.GetImage() as Bitmap;
					ImageMx iex = new ImageMx(bm);
					GetEditor().SetCellValue(ci, iex);
				}

				else // paste text
				{
					IDataObject iData = Clipboard.GetDataObject();
					if (iData == null) return;
					string txt = Clipboard.GetDataObject().GetData(DataFormats.Text).ToString();
					if (String.IsNullOrEmpty(txt)) return;

					try { GetEditor().SetCellValueFromText(ci, txt, ""); }
					catch (Exception ex)
					{
						MessageBoxMx.ShowError(ex.Message);
					}

				}
			}
			catch (Exception ex) { }
		}

		public void MoleculeGridControl_KeyUp(object sender, KeyEventArgs e)
		{
			GridCell[] cells = GetSelectedCells();
			if (cells == null || cells.Length != 1) return;
			int r = V.GetDataSourceRowIndex(cells[0].RowHandle);
			int c = cells[0].Column.AbsoluteIndex;

			if (e.Modifiers == Keys.Control)
			{
				if (e.KeyCode == Keys.X)
				{
					CutCommand(r, c);
					e.Handled = true;
				}
				else if (e.KeyCode == Keys.C)
				{
					CopyCommand(r, c);
					e.Handled = true;
				}
				else if (e.KeyCode == Keys.V)
				{
					PasteCommand(r, c);
					e.Handled = true;
				}
			}

			else if (e.KeyCode == Keys.Delete && e.Modifiers == 0)
			{
				DeleteCommand(r, c);
				e.Handled = true;
			}

			AddNewRowAsNeeded();
			return;
		}

		/// <summary>
		/// Get array of selected cells and their bounds
		/// </summary>
		/// <param name="?"></param>
		/// <param name="upperLeft"></param>
		/// <param name="lowerRight"></param>

		internal void GetSelectedCells(
			out GridCell[] cells,
			out int topRow,
			out int bottomRow,
			out GridColumn leftCol,
			out GridColumn rightCol)
		{
			topRow = bottomRow = -1;
			leftCol = rightCol = null;

			cells = GetSelectedCells();

			if (cells == null || cells.Length == 0) return;

			topRow = bottomRow = cells[0].RowHandle;
			leftCol = rightCol = cells[0].Column;

			foreach (GridCell c in cells)
			{
				if (c.RowHandle < topRow) topRow = c.RowHandle;
				if (c.RowHandle > bottomRow) bottomRow = c.RowHandle;

				if (c.Column.AbsoluteIndex < leftCol.AbsoluteIndex) leftCol = c.Column;
				if (c.Column.AbsoluteIndex > rightCol.AbsoluteIndex) rightCol = c.Column;
			}

			return;
		}

		/// <summary>
		/// Get array of selected cells
		/// </summary>
		/// <returns></returns>

		internal GridCell[] GetSelectedCells()
		{
			if (BGV != null) return BGV.GetSelectedCells();

			else if (GV != null) return GV.GetSelectedCells();

			else if (LV != null)
			{
				if (LV.FocusedColumn == null || LV.FocusedRowHandle < 0) return null;
				GridCell[] gca = new GridCell[1];
				GridCell gc = new GridCell(LV.FocusedRowHandle, LV.FocusedColumn);
				gca[0] = gc;
				return gca;
			}

			else if (CV != null)
			{
				if (CV.FocusedColumn == null || CV.FocusedRowHandle < 0) return null;
				GridCell[] gca = new GridCell[1];
				GridCell gc = new GridCell(CV.FocusedRowHandle, CV.FocusedColumn);
				gca[0] = gc;
				return gca;
			}

			else return null;
		}

		/// <summary>
		/// Check to see if AllowAddNew is true & be sure that new row is included at end of grid if so
		/// </summary>

		public void AddNewRowAsNeeded()
		{
			DataRowMx dr;
			int ci;

			if (Math.Sqrt(4) == 2) return; // disabled for now

			if (!ShowNewRow) return;

			if (DataTable.Rows.Count > 0)
			{
				// todo 
				//dr = DataTable[DataRows.Count - 1];
				//for (ci = 0; ci < DataColumns.Count; ci++)
				//{
				//  if (dr.ItemArray != null && dr.ItemArray[ci] != null && !(dr.ItemArray[ci] is System.DBNull)) break;
				//  if (dr.ItemArrayEx != null && dr.ItemArrayEx[ci] != null) break;
				//}
				//if (ci >= DataColumns.Count) return;
			}

			dr = DataTable.NewRow();
			DataTable.Rows.Add(dr);
			return;
		}

		/// <summary>
		/// Save mapping of grid columns to DataTable columns for both bound and unbound columns
		/// </summary>
		/// <param name="dt"></param>

		public void SaveGridToDataTableColumnMapping(
			DataTableMx dt)
		{
			GridColumn c;
			int ci;

			if (dt == null || MainView == null) return; // may be initialization

			GridToDataTableColumnMap = new int[V.Columns.Count]; // save handles here
			DataTableToGridColumnMap = new int[DataTable.Columns.Count]; // and reverse map here
			for (ci = 0; ci < V.Columns.Count; ci++)
			{
				string fieldName = V.Columns[ci].FieldName;

				if (Lex.Eq(fieldName, "RowNo")) continue; // row number column in LayoutView doesn't map to a DataColumn

				if (fieldName.EndsWith(UnboundSuffix))
					fieldName = fieldName.Substring(0, fieldName.Length - UnboundSuffix.Length);

				DataColumn dc = dt.Columns[fieldName];
				if (dc == null) throw new Exception("Grid column field name not found in DataTable: " + fieldName);

				GridToDataTableColumnMap[ci] = dc.Ordinal; // save handles
				DataTableToGridColumnMap[dc.Ordinal] = ci;
			}

			return;
		}

		/// <summary>
		/// Adjust the binding of the DataTable to the grid so that Mobius custom data types
		/// are handled as unbound columns. Note that the grid requires at least one bound column
		/// which is usually the CheckMark column which may or may not be visible.
		/// </summary>
		/// <param name="dt"></param>

		public void SetupUnboundColumns(
			DataTableMx dt)
		{
			GridColumn c;
			int ci;

			if (dt == null || MainView == null) return; // may be initialization

			GridToDataTableColumnMap = new int[V.Columns.Count]; // save handles here
			for (ci = 0; ci < V.Columns.Count; ci++)
			{
				c = V.Columns[ci];
				if (c.FieldName.EndsWith(UnboundSuffix))
				{ // reset name & editor
					c.FieldName = c.FieldName.Substring(0, c.FieldName.Length - UnboundSuffix.Length);
					c.ColumnEdit = null;
				}

				DataColumn dc = dt.Columns[c.FieldName];

				if (dc == null) throw new Exception("Grid column field name not found in DataSet: " + c.FieldName);

				if (c.ColumnEdit != null) continue; // if have assigned column editor then don't change binding\

				c.FieldName = dc.ColumnName + UnboundSuffix; // set unique name not in source DataTable that indicates this col is unbound

				if (dc.DataType == typeof(MoleculeMx) || dc.DataType == typeof(byte[])) // structures & images
				{
					c.UnboundType = UnboundColumnType.Object;
					c.ColumnEdit = RepositoryItemPictureEdit;
				}

				else if (c.ColumnEdit == null)
				{ // edit everything else without a predefined editor with a MemoEdit control
					c.UnboundType = UnboundColumnType.String;
					c.ColumnEdit = RepositoryItemMemoEdit;
				}
			}

			return;
		}

		/// <summary>
		/// Setup default query manager for MoleculeGrid with no associated manager
		/// </summary>
		/// <param name="dt"></param>

		public QueryManager SetupDefaultQueryManager(
			DataTableMx dt)
		{
			QueryManager qm = new QueryManager();
			QueryManager = qm;
			qm.MoleculeGrid = this;
			qm.DataTable = dt;

			Query q = new Query();
			q.AllowColumnMerging = false; // prevent structure col from being merged
			q.ShowGridCheckBoxes = false; // no check boxes by default
			qm.Query = q;
			q.QueryManager = qm;

			List<int> rfPosToGridColIdx = new List<int>();

			MetaTable mt = new MetaTable();
			if (!String.IsNullOrEmpty(dt.TableName))
				mt.Name = dt.TableName;
			else mt.Name = "MainTable";

			QueryTable qt = new QueryTable(q, mt);

			for (int ci = 0; ci < dt.Columns.Count; ci++)
			{
				DataColumn dc = dt.Columns[ci];
				GridColumn gc = V.Columns.ColumnByFieldName(dc.ColumnName);
				if (gc == null)
					gc = V.Columns.ColumnByFieldName(dc.ColumnName + UnboundSuffix);
				if (gc == null) continue;

				int gci = gc.AbsoluteIndex;
				rfPosToGridColIdx.Add(gci);

				MetaColumn mc = new MetaColumn();
				mc.Name = dc.ColumnName;

				int miWidth = GraphicsMx.PixelsToMilliinches(gc.Width);
				mc.Width = GraphicsMx.MilliinchesToColumns(miWidth);

				if (ci == 0) mc.KeyPosition = 0; // make 1st col key

				if (dc.DataType == typeof(MoleculeMx))
					mc.DataType = MetaColumnType.Structure;

				else if (dc.DataType == typeof(DateTimeMx) || dc.DataType == typeof(DateTime))
					mc.DataType = MetaColumnType.Date;

				else // type of other cols doesn't matter for now
					mc.DataType = MetaColumnType.String;

				mt.AddMetaColumn(mc);

				QueryColumn qc = qt.AddQueryColumnByName(mc.Name, true);
			}

			qm.DataTableManager = new DataTableManager(QueryManager);
			Dtm.RowAttributesVoPos = -1; // special initial columns assumed to not be present
			Dtm.CheckMarkVoPos = -1;
			Dtm.KeyValueVoPos = -1;

			ResultsFormatFactory rff = new ResultsFormatFactory(QueryManager, q, OutputDest.WinForms);
			rff.Build();
			qm.ResultsFormat = rff.Rf;

			SetupGridColumnIndexToResultsField(rff.Rf, rfPosToGridColIdx); // extra member of ResultsFormat that needs setting up

			ResultsFormatter rfmt = new ResultsFormatter(qm);

			return qm;
		}

		/// <summary>
		/// SetupGridColumnIndexToResultsField
		/// </summary>
		/// <param name="rf"></param>
		/// <param name="rfPosToGridColIdx"></param>

		public void SetupGridColumnIndexToResultsField(
			ResultsFormat rf,
			List<int> rfPosToGridColIdx)
		{

			ResultsTable rt = rf.Tables[0];
			List<ResultsField> gci2rf = new List<ResultsField>(new ResultsField[V.Columns.Count]);
			rf.GridColumnIndexToResultsField = gci2rf;

			for (int rfi = 0; rfi < rt.Fields.Count; rfi++)
			{
				ResultsField rfld = rt.Fields[rfi];

				int gcIdx = rfPosToGridColIdx[rfi];
				gci2rf[gcIdx] = rfld;
				rfld.GridColumn = V.Columns[gcIdx];
			}

			return;
		}

		/// <summary>
		/// Setup grid columns and headers
		/// </summary>
		/// <param name="rf"></param>

		public void FormatGridHeaders(ResultsFormat rf)
		{
			if (rf == null) throw new Exception("ResultsFormat not defined");

			QueryManager.ResultsFormat = rf;
			QueryManager.ResultsFormatter = rf.Formatter;

			//ClientLog.Message("FormatGridHeaders1");

			if (rf.UseBandedGridView) FormatBandedGridViewHeaders(rf);
			else FormatLayoutAndCardViewHeaders(rf);

			if (QueryManager.DataTable != null) Refresh();

			//ClientLog.Message("FormatGridHeaders2");

			return;
		}

		/// <summary>
		/// Format the LayoutView
		/// </summary>
		/// <param name="rf"></param>

		// The card contains the following four columns that are created in advance
		// in the designer. The key and structure columns need to be linked to the 
		// underlying DataTable names for the columns associated columns. The label 
		// the key field also needs to be set.
		//
		//    Name                FieldName
		//    -----------------   --------------------
		// 1. LayoutCheckMark     CheckMark - This is the only 
		// 2. LayoutKeyCol        keyTable.keyColName.Unbound
		// 3. LayoutStructureCol  keyTable.structureColName.Unbound

		public void FormatLayoutAndCardViewHeaders(
			ResultsFormat rf)
		{
			GridColumn gc;
			int ci, width;

			int t0 = TimeOfDay.Milliseconds();

			Rf.GridColumnIndexToResultsField = new List<ResultsField>(); // build map of col index to rfld
			
			if (Query.Tables.Count == 0) return;
			QueryTable qt = Query.Tables[0];
			MetaTable mt = qt.MetaTable;

			rf.GridColumnIndexToResultsField.Add(null); // add grid col map item for checkmark that is not mapped to a results field

			MetaColumn keyCol = mt.KeyMetaColumn;
			ResultsField keyRfld = rf.Tables[0].GetResultsFieldByName(keyCol.Name);
			Rf.GridColumnIndexToResultsField.Add(keyRfld);

			MetaColumn strCol = mt.FirstStructureMetaColumn;
			ResultsField strRfld = rf.Tables[0].GetResultsFieldByName(strCol.Name);
			Rf.GridColumnIndexToResultsField.Add(strRfld);

			BeginUpdate();

			// Setup LayoutView used for display

			LayoutView lv = GetLayoutView();
			LayoutViewColumn lvc;
			LayoutViewField lvf;
			MainView = lv;

			for (ci = 0; ci < lv.Columns.Count; ci++)
			{
				lvc = lv.Columns[ci];
				lvf = lvc.LayoutViewField;

				if (Lex.Eq(lvc.Name, "LayoutCheckMark"))
					lvc.Visible = ShowCheckMarkCol;

				else if (Lex.Eq(lvc.Name, "LayoutKeyCol"))
				{
					lvc.FieldName = qt.Alias + "." + keyCol.Name + UnboundSuffix; // make unbound name
					lvc.Caption = keyCol.Label; // set proper caption
					if (keyRfld != null) keyRfld.GridColumn = lvc;
				}

				else if (Lex.Eq(lvc.Name, "LayoutStructureCol"))
				{
					lvc.FieldName = qt.Alias + "." + strCol.Name + UnboundSuffix; // make unbound name
					width = ResultsFieldToGridColumnWidth(strRfld, Rf.PageScale / 100.0, -4);
					lvf.Width = width;
					lvf.EditorPreferredWidth = width;
					lv.CardMinSize = new Size(width, lv.CardMinSize.Height); // set card size based on structure
					if (strRfld != null) strRfld.GridColumn = lvc;
				}

				lvc.OptionsColumn.AllowEdit = UserData.CanModifyTable(mt);
			}

			// Setup CardView used for printing
			// This is necessary since LayoutView splits up a single card and has some problems formatting problems

			CardView cv = GetCardView();
			for (ci = 0; ci < cv.Columns.Count; ci++)
			{
				gc = cv.Columns[ci];

				if (Lex.Eq(cv.Name, "CardKeyCol"))
				{
					gc.FieldName = qt.Alias + "." + keyCol.Name + UnboundSuffix; // make unbound name
					cv.CardCaptionFormat = keyCol.Label + ": {2}"; // put key value in card caption
				}

				else if (Lex.Eq(gc.Name, "CardStructureCol"))
				{
					gc.FieldName = qt.Alias + "." + strCol.Name + UnboundSuffix; // make unbound name
					width = ResultsFieldToGridColumnWidth(strRfld, Rf.PageScale / 100.0, -4);
					gc.Width = width;
					cv.CardWidth = width;
				}
			}

			EndUpdate();

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Setup grid columns and headers
		/// This routine sets grid column widths and justification for both the header
		/// and data rows. The header labels are output as a normal part of the 
		/// OutputRecord code. 
		/// </summary>

		public void FormatBandedGridViewHeaders(
			ResultsFormat rf)
		{
			ResultsTable rt;
			ResultsField rfld;
			ResultsSegment seg;
			MetaTable mt;
			MetaColumn mc;
			QueryTable qt;
			QueryColumn qc;
			int rep, ci, ri, ti, fi, i1, width;
			HorzAlignment hAlign;
			VertAlignment vAlign;
			GridBand checkMarkBand, tableBand, parentBand = null;
			BandedGridColumn col;

			int t0 = TimeOfDay.Milliseconds();

			Rf.GridColumnIndexToResultsField = new List<ResultsField>(); // build map of col index to rfld

			BeginUpdate();

			BandedGridView v = GetBandedGridView();
			MainView = v;

			// Initial grid setup

			v.Bands.Clear();
			v.Columns.Clear();

			v.LeftCoord = 0; // reset scroll to left
			v.Bands.Clear();
			v.Columns.Clear();
			v.OptionsView.ShowGroupPanel = false; // hide group panel for now

			v.IndicatorWidth = IndicatorColWidth;

			v.BandPanelRowHeight = (int)(BandPanelRowHeight * ColumnScale);
			v.ColumnPanelRowHeight = TotalColumnPanelHeight = ColumnPanelRowHeight;

			if (SS.I.ScrollGridByPixel)
				v.OptionsBehavior.AllowPixelScrolling = DefaultBoolean.True;
			else v.OptionsBehavior.AllowPixelScrolling = DefaultBoolean.False;

			// Setup odd/even background colors

			v.OptionsView.EnableAppearanceEvenRow = true; // odd & even row background colors
			v.Appearance.EvenRow.Options.UseBackColor = true;
			v.Appearance.EvenRow.BackColor = SS.I.EvenRowBackgroundColor;

			v.OptionsView.EnableAppearanceOddRow = true;
			v.Appearance.OddRow.Options.UseBackColor = true;
			v.Appearance.OddRow.BackColor = SS.I.OddRowBackgroundColor;


			// Setup band and column for check marks

			checkMarkBand = new GridBand();
			checkMarkBand.Width = CheckMarkColWidth;
			if (Rf.Tables.Count > 1) checkMarkBand.Fixed = FixedStyle.Left;
			v.Bands.Add(checkMarkBand);
			checkMarkBand.Name = "CheckMark";

			BandedGridColumn checkMarkCol = new BandedGridColumn(); // add column for check mark
			checkMarkCol.ColumnEdit = RepositoryItemCheckEdit;
			checkMarkCol.FieldName = "CheckMark";
			checkMarkCol.ImageAlignment = System.Drawing.StringAlignment.Center;
			checkMarkCol.ImageIndex = (int)SmallHeaderGlyphs.CheckMark;
			checkMarkCol.Caption = " ";
			checkMarkCol.Width = CheckMarkColWidth;
			checkMarkCol.Visible = true;
			checkMarkCol.OptionsFilter.AllowFilter = false;
			checkMarkCol.OptionsColumn.AllowSort = DefaultBoolean.False;
			checkMarkCol.ToolTip = "Checkmarks for selected compounds";
			checkMarkBand.Columns.Add(checkMarkCol); // add column to band (also gets added to view)
			if (!ShowCheckMarkCol) checkMarkBand.Visible = false;
			rf.GridColumnIndexToResultsField.Add(null); // // add grid col map item for checkmark that is not mapped to a results field

			// Calc number of rows for table header bands

			TableHeaderBandRows = 1;
			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				qt = rt.QueryTable;
				mt = rt.MetaTable;

				if (rt.Fields.Count == 0) continue; // no fields for table
				int bandWidth = MilliinchesToPixels(rt.Width) - 4; // width in pixels minus some side padding
				int rows = CalculateRowCount(qt.ActiveLabel, v.Appearance.BandPanel.Font, bandWidth);
				if (rows > TableHeaderBandRows) TableHeaderBandRows = rows;
			}

			if (TableHeaderBandRows >= 4) TableHeaderBandRows--; // remove unnecessary extra band height

			// Setup bands & cols for each table

			int qcCount = 0;
			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				qt = rt.QueryTable;
				mt = rt.MetaTable;

				if (rt.Fields.Count == 0) continue; // no fields for table

				// Create a band for the table

				tableBand = SetupBand(rt.Header, null);
				tableBand.RowCount = TableHeaderBandRows;
				tableBand.AutoFillDown = true;

				tableBand.Tag = qt; // put QueryTable ref in tag
				if (ti == 0 && Rf.Tables.Count > 1 &&
				 rt.Fields.Count < 8) // fix first table if not too many cols
					tableBand.Fixed = FixedStyle.Left;

				// Process each field

				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					rfld = rt.Fields[fi];
					qc = rfld.QueryColumn;
					mc = rfld.MetaColumn;

					parentBand = tableBand;
					if (SS.I.ShowConditionalFormatting && Rf.Query.ShowCondFormatLabels &&
						Rf.ShowCondFormatLabels)
					{
						if (rfld.CondFmtHeaders != null) // insert band for each conditional format
						{
							foreach (MobiusDataType cfHdr in rfld.CondFmtHeaders)
							{
								parentBand = SetupBand(cfHdr, parentBand);
							}
						}
					}

					col = new BandedGridColumn();
					parentBand.Columns.Add(col); // add column to band (also gets added to view)
					rfld.GridColumn = col;
					rf.GridColumnIndexToResultsField.Add(rfld);

					//if (Lex.Eq(rfld.QueryColumn.MetaColumn.Name, "Structure")) col = col; // debug

					// Can't set column FixedStyle if using banded view
					//if (ti == 0 && Rf.Tables.Count == 1 && parentBand.Columns.Count == 1)
					//  col.Fixed = FixedStyle.Left; 

					col.Caption = rfld.QueryColumn.ActiveLabel;

					col.AppearanceHeader.Options.UseTextOptions = true;
					col.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
					col.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
					col.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

					col.Width = ResultsFieldToGridColumnWidth(rfld, Rf.PageScale / 100.0, 0);

					int rowCount = CalculateRowCount(col.Caption, v.Appearance.HeaderPanel.Font, col.Width);
					int fontHeight = (int)(22 * ColumnScale);
					int height = rowCount * fontHeight; // calculate height for this caption including pading

					if (ColumnScale != 1)
					{
						col.AppearanceHeader.Font = ScaleFont(col.AppearanceHeader.Font, ColumnScale);
						col.AppearanceCell.Font = ScaleFont(col.AppearanceCell.Font, ColumnScale);
					}

					if (rfld.Header is MoleculeMx) // put structure in header
					{
						MoleculeMx cs = rfld.Header as MoleculeMx;
						height = height + 70; // include fixed height for structure

						col.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near; // put caption in upper left
						col.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
					}

					if (height > TotalColumnPanelHeight)
						v.ColumnPanelRowHeight = TotalColumnPanelHeight = height; // keep greatest height

					col.Visible = true;

					col.AppearanceHeader.ForeColor = rfld.Header.ForeColor;
					col.AppearanceHeader.BackColor = rfld.Header.BackColor;

					col.FieldName = qt.Alias + "." + mc.Name + UnboundSuffix; // make unbound name based on QueryTable alias
																																		//col.FieldName = mt.Name + "." + mc.Name + UnboundSuffix; // make unbound name based on MetaTable name (old obsolete form)

					col.OptionsColumn.AllowSort = DefaultBoolean.False; // sorting is handled by custom Mobius code
					col.OptionsFilter.AllowFilter = false; // filtering is handled by custom Mobius code, also this frees space used by filter icon
					col.OptionsColumn.AllowEdit = UserData.CanModifyTable(mt);

					//if (mc.DataType == MetaColumnType.Image)
					//  col.OptionsFilter.AllowFilter = false;
					//else col.OptionsFilter.AllowFilter = true;

					col.AppearanceCell.TextOptions.WordWrap = WordWrap.Wrap; // set wrapping
					col.AppearanceCell.ForeColor = Color.Black;

					col.UnboundType = UnboundColumnType.String; // default to string type

					if (mc.DataType == MetaColumnType.Structure || // display structure or image
							mc.DataType == MetaColumnType.Image)
					{
						col.UnboundType = UnboundColumnType.Object;
					}

					SetColumnEditRepositoryItem(col, qc);

					SetColumnAlignment(col, qc);

					if (qcCount == 0)
					{ // match indicator col alignment to 1st data col alignment, can't currently align checkmark vertically
						v.Appearance.HeaderPanel.TextOptions.VAlignment = col.AppearanceCell.TextOptions.VAlignment;
					}

					qcCount++;
				}
			}

			SetHeaderGlyphs();
			EndUpdate();

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Get the scaled width of a grid column in pixels from a column width in milliinches
		/// </summary>
		/// <param name="rfld"></param>
		/// <param name="pctScale"></param>
		/// <param name="pixDelta"></param>
		/// <returns></returns>

		internal int ResultsFieldToGridColumnWidth(
			ResultsField rfld,
			double scale,
			int pixDelta)
		{
			scale *= ColumnScale; // include overall column scaling
			if (rfld.MetaColumn.IsGraphical) scale *= GraphicsScale; // include extra graphics column scaling if appropriate

			int width = MoleculeGridControl.MilliinchesToPixels(rfld.ColumnWidth) + pixDelta;
			width = (int)(scale * width);
			return width;
		}

		/// <summary>
		/// Get a results field column width in milliinches from a grid column width in pixels
		/// </summary>
		/// <param name="gridColWidth"></param>
		/// <param name="rfld"></param>
		/// <param name="scale"></param>
		/// <returns></returns>

		internal int GridToResultsFieldColumnWidth(
			int gridColWidth,
			ResultsField rfld,
			double scale)
		{
			scale *= ColumnScale; // include overall column scaling
			if (rfld.MetaColumn.IsGraphical) scale *= GraphicsScale; // include extra graphics column scaling if appropriate

			int width = MoleculeGridControl.PixelsToMilliinches(gridColWidth);
			width = (int)(width / scale);
			return width;
		}

		/// <summary>
		/// Get the zoom scale for columns
		/// </summary>

		public static double ColumnScale
		{
			get { return SS.I.TableColumnZoom / 100.0; }
		}

		/// <summary>
		/// Get the scale for graphics columns
		/// </summary>

		public static double GraphicsScale
		{
			get { return SS.I.GraphicsColumnZoom / 100.0; }
		}

		public void SetColumnEditRepositoryItem(GridColumn col, QueryColumn qc)
		{
			if (qc == null) return;

			MetaColumn mc = qc.MetaColumn;

			col.ColumnEdit = RepositoryItemMemoEdit; // default to text editor

			if (mc.DataType == MetaColumnType.Structure || // display structure or image
					mc.DataType == MetaColumnType.Image)
			{
				col.ColumnEdit = RepositoryItemPictureEdit;
			}

			else if (mc.DataType == MetaColumnType.String && qc.ActiveDisplayFormat == ColumnFormatEnum.HtmlText)
			{
				if (UseRichHtmlColumnRendering)
					col.ColumnEdit = RepositoryItemRichTextEdit; // rich HTML

				else col.ColumnEdit = RepositoryItemHypertextLabel; // basic HTML (partial set of tags)
			}
			
			else if (mc.DataType == MetaColumnType.Html)
			{
					col.ColumnEdit = RepositoryItemRichTextEdit; // rich HTML
			}

			return;
		}

		/// <summary>
		/// Set column auto width for grid view
		/// </summary>
		/// <param name="value"></param>

		public void SetColumnAutoWidth(bool value)
		{
			GetBandedGridView().OptionsView.ColumnAutoWidth = value;
			return;
		}

/// <summary>
/// Set the horizontal & vertical alignment of a grid column from the corresponding QueryColumn values
/// </summary>
/// <param name="col"></param>
/// <param name="qc"></param>

		public void SetColumnAlignment(GridColumn col, QueryColumn qc)
		{

			// Vertical alignment

			VertAlignment vAlign = VertAlignment.Top;
			if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Top)
				vAlign = VertAlignment.Top;
			else if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Middle)
				vAlign = VertAlignment.Center;
			else if (qc.ActiveVerticalAlignment == VerticalAlignmentEx.Bottom)
				vAlign = VertAlignment.Bottom;

			col.AppearanceCell.TextOptions.VAlignment = vAlign;

			// Horizontal alignment

			HorzAlignment hAlign = HorzAlignment.Near;

			switch (qc.MetaColumn.DataType)
			{

				case MetaColumnType.CompoundId:
					hAlign = HorzAlignment.Near;
					break;

				case MetaColumnType.Structure:
				case MetaColumnType.Image:
					hAlign = HorzAlignment.Center;
					vAlign = VertAlignment.Center;
					col.UnboundType = UnboundColumnType.Object;
					col.ColumnEdit = RepositoryItemPictureEdit;
					break;

				case MetaColumnType.Number:
				case MetaColumnType.Integer:
				case MetaColumnType.QualifiedNo:
					hAlign = HorzAlignment.Far;
					break;

				case MetaColumnType.String:
					hAlign = HorzAlignment.Near;
					break;

				case MetaColumnType.Date:
					hAlign = HorzAlignment.Near;
					break;

				default: // all other stuff
					hAlign = HorzAlignment.Near;
					break;
			}

			if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Left) // specific alignment selected
				hAlign = HorzAlignment.Near;

			else if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Center)
				hAlign = HorzAlignment.Center;

			else if (qc.ActiveHorizontalAlignment == HorizontalAlignmentEx.Right)
				hAlign = HorzAlignment.Far;

			col.AppearanceCell.TextOptions.HAlignment = hAlign;

			return;
		}

		/// <summary>
		/// Get scale in percentage to make full query results fit within page
		/// </summary>
		/// <returns></returns>

		public int GetFitPageWidthScale()
		{
			if (Rf == null) return 100;

			if (MainView is BandedGridView) return GetFitBandedPageWidthScale();
			else return GetFitLayoutPageWidthScale();
		}

		/// <summary>
		/// Get scale in percentage to make full query results fit within a banded grid view page
		/// </summary>
		/// <returns></returns>

		public int GetFitBandedPageWidthScale()
		{
			BandedGridView v = MainView as BandedGridView;

			int rw = (int)(GraphicsMx.MilliinchesToPixels(Rf.CalcTotalWidth()) * (SS.I.GridScaleAdjustment / 100.0));
			if (rw <= 0) return 100;
			rw += IndicatorColWidth; // v.IndicatorWidth; // indicator width

			int cw = Width;
			if (ShowCheckMarkCol) cw -= CheckMarkColWidth; // subtract any checkmark width
			cw -= ScrollBarWidth;
			int pct = (int)(100.0 * cw / rw);
			return pct;
		}

		/// <summary>
		/// Get scale in percentage to make full query results fit within a layout view page
		/// </summary>
		/// <returns></returns>

		public int GetFitLayoutPageWidthScale()
		{
			int rw = CardWidth; // cardwidth is fixed size in pixels
			if (rw <= 0) return 100;

			int cw = Width;
			cw -= ScrollBarWidth;
			int pct = (int)(100.0 * cw / rw);
			return pct;
		}


		/// <summary>
		/// Overall view scaling
		/// </summary>
		/// <param name="zoomFactor"></param>
		/// 
		public void ScaleView(int scalePct)
		{
			if (Query != null) Query.ViewScale = scalePct;
			if (Rf != null) Rf.PageScale = scalePct;

			double scale = scalePct / 100.0;

			if (MainView is BandedGridView) ScaleBandedGridView(scale);
			else ScaleLayoutView(scale);

			return;
		}

		/// <summary>
		/// Zoom a BandedGridView
		/// </summary>
		/// <param name="zoomFactor"></param>

		public void ScaleBandedGridView(
			double scale)
		{
			ResultsTable rt;
			ResultsField rfld;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			GridBand checkMarkBand, tableBand, parentBand = null;
			BandedGridColumn col;
			Font font;
			int ti, fi;

			BeginUpdate();

			double fontScale = scale * ColumnScale; // scale for fonts

			BandedGridView v = GetBandedGridView();

			v.IndicatorWidth = (int)(IndicatorColWidth * scale);

			v.BandPanelRowHeight = (int)(BandPanelRowHeight * fontScale);
			v.ColumnPanelRowHeight = (int)(TotalColumnPanelHeight * fontScale);

			font = v.Appearance.HeaderPanel.Font;
			v.Appearance.HeaderPanel.Font = new Font(font.FontFamily, (int)(FontSize * fontScale), font.Style);

			foreach (GridBand gb0 in v.Bands)
				ScaleBandFonts(gb0, fontScale);

			// Setup bands & cols for each table

			for (ti = 0; ti < Rf.Tables.Count; ti++)
			{
				rt = Rf.Tables[ti];
				qt = rt.QueryTable;
				mt = rt.MetaTable;

				if (rt.Fields.Count == 0) continue; // no fields for table

				// Process each field

				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					rfld = rt.Fields[fi];
					qc = rfld.QueryColumn;
					mc = rfld.MetaColumn;

					col = rfld.GridColumn as BandedGridColumn;
					if (col == null) return;
					col.Width = ResultsFieldToGridColumnWidth(rfld, scale, 0);

					col.AppearanceHeader.Font = new Font(font.FontFamily, (int)(FontSize * fontScale), font.Style);
					col.AppearanceCell.Font = new Font(font.FontFamily, (int)(FontSize * fontScale), font.Style);
				}
			}

			if (QueryManager != null && QueryManager.DataTableManager != null)
				QueryManager.DataTableManager.ResetFormattedBitmaps(); // need to redraw structures to scale

			EndUpdate();

			RefreshDataSourceMx(); // needed for structure redraw
			return;
		}

		/// <summary>
		/// Alternate debuggable call to RefreshDataSource
		/// </summary>

		public void RefreshDataSourceMx()
		{
			//UIMisc.Beep();
			QueryManager.DataTableManager.ResetFormattedValues(); // clear existing formatted values
			RefreshDataSource();
			return;
		}

		/// <summary>
		/// Scale a Layout view
		/// </summary>

		public void ScaleLayoutView(
			double scale)
		{
			LayoutViewColumn col;
			LayoutViewField lvf;
			int width, height;

			QueryTable qt = Query.Tables[0];
			MetaTable mt = qt.MetaTable;

			MetaColumn keyCol = mt.KeyMetaColumn;
			ResultsField keyRfld = Rf.Tables[0].GetResultsFieldByName(keyCol.Name);

			MetaColumn strCol = mt.FirstStructureMetaColumn;
			ResultsField strRfld = Rf.Tables[0].GetResultsFieldByName(strCol.Name);
			BeginUpdate();

			LayoutView lv = GetLayoutView();

			if (QueryManager != null && QueryManager.DataTableManager != null)
				QueryManager.DataTableManager.ResetFormattedBitmaps(); // need to redraw structures to scale

			width = (int)(LayoutMinWidth * ColumnScale);
			height = (int)(LayoutMinHeight * ColumnScale);
			lv.CardMinSize = new Size(width, height);

			// Key column

			col = lv.Columns[1];
			Font font = lv.Appearance.HeaderPanel.Font;
			col.AppearanceHeader.Font = new Font(font.FontFamily, (int)(FontSize * scale), font.Style);
			col.AppearanceCell.Font = new Font(font.FontFamily, (int)(FontSize * scale), font.Style);
			width = ResultsFieldToGridColumnWidth(keyRfld, scale, -4);
			SetLayoutViewFieldWidth(col.LayoutViewField, width);

			// Structure column

			col = lv.Columns[2];
			width = ResultsFieldToGridColumnWidth(strRfld, scale, -4);
			SetLayoutViewFieldWidth(col.LayoutViewField, width);

			lv.CardMinSize = new Size(width, lv.CardMinSize.Height); // set card size based on structure

			EndUpdate();
			return;
		}

		/// <summary>
		/// SetLayoutViewFieldWidth
		/// </summary>
		/// <param name="lvf"></param>
		/// <param name="width"></param>

		void SetLayoutViewFieldWidth(LayoutViewField lvf, int width)
		{
			lvf.MinSize = new Size(width, lvf.MinSize.Height);
			lvf.MaxSize = new Size(width, lvf.MaxSize.Height);
			lvf.Width = width;
			lvf.EditorPreferredWidth = width;
			return;
		}

		/// <summary>
		/// Scale a CardView for printing
		/// </summary>
		/// <param name="zoomFactor"></param>

		public void ScaleCardView(
			double scale)
		{
			GridColumn gc;
			int width;

			QueryTable qt = Query.Tables[0];
			MetaTable mt = qt.MetaTable;

			MetaColumn keyCol = mt.KeyMetaColumn;
			ResultsField keyRfld = Rf.Tables[0].GetResultsFieldByName(keyCol.Name);

			MetaColumn strCol = mt.FirstStructureMetaColumn;
			ResultsField strRfld = Rf.Tables[0].GetResultsFieldByName(strCol.Name);

			BeginUpdate();

			if (QueryManager != null && QueryManager.DataTableManager != null)
				QueryManager.DataTableManager.ResetFormattedBitmaps(); // need to redraw structures to scale

			CardView cv = GetCardView();

			gc = cv.Columns[1]; // key col
			width = ResultsFieldToGridColumnWidth(keyRfld, scale, -4);
			gc.Width = width;

			gc = cv.Columns[2]; // str col;
			width = ResultsFieldToGridColumnWidth(strRfld, scale, -4);
			gc.Width = width;

			cv.CardWidth = width;
			cv.Appearance.CardCaption.Font = ScaleFont(cv.Appearance.CardCaption.Font, scale);
			cv.OptionsPrint.UsePrintStyles = true;

			EndUpdate();
			return;
		}

		/// <summary>
		/// Recursively scale band fonts
		/// </summary>
		/// <param name="tableBand"></param>
		/// <param name="scale"></param>

		void ScaleBandFonts(
			GridBand tableBand,
			double scale)
		{
			Font font = tableBand.AppearanceHeader.Font;
			tableBand.AppearanceHeader.Font = ScaleFont(tableBand.AppearanceHeader.Font, scale);

			foreach (GridBand gb in tableBand.Children)
				ScaleBandFonts(gb, scale);

			return;
		}

		Font ScaleFont(
			Font font,
			double scale)
		{
			int size = (int)(FontSize * scale + .5);
			if (size == 0) size = 1; // smallest allowed size
			if (size != font.Size)
			{
				Font original = font;
				font = new Font(font.FontFamily, size, font.Style);
				//original.Dispose();
			}

			return font;
		}

		/// <summary>
		/// Get the width of the current view in milliinches
		/// </summary>
		/// <returns></returns>

		public int ViewWidth
		{
			get
			{
				int width;
				BaseView view = MainView;

				if (view is BandedGridView)
					width = Width; // todo: make more accurate

				else if (view is GridView)
					width = Width;

				else if (view is LayoutView)
					width = (view as LayoutView).CardMinSize.Width;

				else if (view is CardView)
					width = (view as CardView).CardWidth;

				else throw new Exception("Unexpected grid view type");

				width = GraphicsMx.PixelsToMilliinches(width);

				return width;
			}
		}

		/// <summary>
		/// Calculate the number of rows that a caption will require in a band or column header
		/// </summary>
		/// <param name="caption"></param>
		/// <param name="font"></param>
		/// <param name="colWidth"></param>
		/// <returns></returns>

		int CalculateRowCount(
			string caption,
			Font font,
			int colWidth)
		{
			if (ControlGraphics == null)
			{
				try { ControlGraphics = CreateGraphics(); }
				catch (Exception ex) { return 1; }
			}

			string[] words = caption.Split(' ');
			if (words.Length <= 1) return 1;

			int rowCount = 1;
			float accumWidth = 0;

			for (int wi = 0; wi < words.Length; wi++)
			{
				SizeF size = ControlGraphics.MeasureString(words[wi] + " ", font);
				accumWidth += size.Width;
				if (accumWidth > colWidth)
				{
					rowCount++;
					accumWidth = size.Width;
				}
			}

			return rowCount;
		}

		/// <summary>
		/// Set the header text for a QueryTable in the grid
		/// </summary>
		/// <param name="ti"></param>
		/// <param name="title"></param>

		public void SetTableHeaderCaption(
			int ti,
			string title)
		{
			BandedGridView bgv = MainView as BandedGridView;
			if (bgv == null) return;

			bgv.Bands[ti + 1].Caption = title;
			return;
		}

		/// <summary>
		/// Set the sorting and filter header glyphs
		/// </summary>

		internal void SetHeaderGlyphs()
		{
			BandedGridView bgv = MainView as BandedGridView;
			if (bgv == null) return;
			bgv.Images = Helpers.SmallHeaderGlyphs5x8;

			ColumnInfo cinf = new ColumnInfo();
			for (int ci = 0; ci < bgv.Columns.Count; ci++)
			{
				if (!GetMetaDataForGridCol(ci, cinf)) continue;
				ResultsField rf = cinf.Rfld;
				BandedGridColumn col = bgv.Columns[ci];
				int soi = Math.Min(Math.Abs(rf.SortOrder), 3); // keep within range of glyphs
				if (rf.SortOrder > 0)
					col.ImageIndex = (int)SmallHeaderGlyphs.Ascending + soi;
				else if (rf.SortOrder < 0)
					col.ImageIndex = (int)SmallHeaderGlyphs.Descending + soi;
				else if (rf.QueryColumn.SecondaryCriteria != "")
					col.ImageIndex = (int)SmallHeaderGlyphs.Filter; // show filter icon
				else col.ImageIndex = -1;

				col.ImageAlignment = StringAlignment.Far; // put on rt
			}
		}

		/// <summary>
		/// Set a filter glyph
		/// </summary>
		/// <param name="rf"></param>

		internal void SetFilterGlyph(ColumnInfo cinf)
		{
			BandedGridColumn col = cinf.GridColumn as BandedGridColumn;
			if (col == null) return;
			if (cinf.Rfld.QueryColumn.SecondaryCriteria != "")
				col.ImageIndex = 9; // show filter icon
			else col.ImageIndex = -1;
		}

		/// <summary>
		/// Get the first BandedGridView associated with the grid
		/// </summary>
		/// <returns></returns>

		internal BandedGridView GetBandedGridView()
		{
			foreach (ColumnView v in ViewCollection)
			{
				if (v is BandedGridView) return v as BandedGridView;
			}

			throw new Exception("No BandedGridView found");
		}

		/// <summary>
		/// Get the first CardView associated with the grid
		/// </summary>
		/// <returns></returns>

		internal CardView GetCardView()
		{
			foreach (ColumnView v in ViewCollection)
			{
				if (v is CardView) return v as CardView;
			}

			throw new Exception("No CardView found");
		}

		/// <summary>
		/// Get the first LayoutView associated with the grid
		/// </summary>
		/// <returns></returns>

		internal LayoutView GetLayoutView()
		{
			foreach (ColumnView v in ViewCollection)
			{
				if (v is LayoutView) return v as LayoutView;
			}

			throw new Exception("No LayoutView found");
		}

		/// <summary>
		/// Convert from milliinches to pixel width for grid
		/// </summary>
		/// <param name="miWidth"></param>
		/// <returns></returns>

		public static int MilliinchesToPixels(int miWidth)
		{
			int width = GraphicsMx.MilliinchesToPixels(miWidth);
			return width;
		}

		/// <summary>
		/// Convert from grid pixels to milliinches
		/// </summary>
		/// <param name="pixWidth"></param>
		/// <returns></returns>

		public static int PixelsToMilliinches(int pixWidth)
		{
			int width = GraphicsMx.PixelsToMilliinches(pixWidth);
			return width;
		}

		/// <summary>
		/// Setup a table header or conditional formatting band
		/// </summary>
		/// <param name="header"></param>
		/// <param name="miWidth"></param>
		/// <param name="parentBand"></param>
		/// <returns></returns>

		GridBand SetupBand(
			MobiusDataType header,
			GridBand parentBand)
		{
			GridBand band = new GridBand();

			if (parentBand == null) BGV.Bands.Add(band);
			else parentBand.Children.Add(band);

			if (header is StringMx)
			{
				StringMx sh = (StringMx)header;
				band.Caption = sh.Value;
				//if (sh.Value != "") sh = sh; // debug
			}

			if (header.BackColor != Color.Empty)
			{
				band.AppearanceHeader.Options.UseBackColor = true;
				band.AppearanceHeader.BackColor = header.BackColor;
			}

			//			band.Width = MilliinchesToGridWidth(miWidth); // (don't set band width, just col widths)
			band.AutoFillDown = false;

			band.AppearanceHeader.Options.UseTextOptions = true;
			band.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			band.AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Bottom;
			band.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

			if (!String.IsNullOrEmpty(header.Hyperlink))
			{
				band.AppearanceHeader.Options.UseFont = true;
				band.AppearanceHeader.Font = new Font(band.AppearanceHeader.Font, FontStyle.Underline);
				band.AppearanceHeader.Options.UseForeColor = true;
				band.AppearanceHeader.ForeColor = Color.Blue;
			}

			else
			{
				band.AppearanceHeader.Options.UseForeColor = true;
				band.AppearanceHeader.ForeColor = Color.Black;
			}

			if (ColumnScale != 1)
				band.AppearanceHeader.Font = ScaleFont(band.AppearanceHeader.Font, ColumnScale);

			return band;

			//				band.OptionsBand.FixedWidth = true;
			//				if (ti == 0) band.Fixed = FixedStyle.Left; // freeze first band
			// band.RowCount = 123; // set number of rows in band based on label
		}

		/// <summary>
		/// Create a new row and add it to the end of the data
		/// </summary>
		/// <returns></returns>

		public DataRowMx AddNewRow()
		{
			DataRowMx dr = DataTable.NewRow();
			DataTable.Rows.Add(dr);
			return dr;
		}

		/// <summary>
		/// Insert a new row in the grid
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="r"></param>

		public DataRowMx InsertRowAt(
			int r)
		{
			DataRowMx dr = DataTable.NewRow();
			DataTable.Rows.InsertAt(dr, r);
			return dr;
		}

		/// <summary>
		/// Remove a row from the grid
		/// </summary>
		/// <param name="r"></param>

		public void RemoveRowAt(
			int r)
		{
			DataTable.Rows.RemoveAt(r);
			return;
		}

		/// <summary>
		/// Move a row up one
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		public DataRowMx MoveRowUp(
			int r)
		{
			DataRowMx dr = DataTable.Rows[r];
			if (r == 0) return dr;

			DataRowMx newRow = DataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			DataTable.Rows.RemoveAt(r);
			DataTable.Rows.InsertAt(newRow, r - 1);
			return dr;
		}

		/// <summary>
		/// Move a row down one
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		public DataRowMx MoveRowDown(
			int r)
		{
			DataRowMx dr = DataTable.Rows[r];
			if (r == DataTable.Rows.Count - 1) return dr;

			DataRowMx newRow = DataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			DataTable.Rows.RemoveAt(r);
			DataTable.Rows.InsertAt(newRow, r + 1);
			return dr;
		}

		private void MoleculeGridControl_DoubleClick(object sender, EventArgs e)
		{
			int r = Row;
			int c = Col;

			if ((r < 0 && r != GridControl.NewItemRowHandle) || c < 0) return; // return if outside of area

			CellInfo ci = GetCellInfo(r, c);
			if (ci.Mc == null) return;

			MetaColumnType colType = ci.Mc.DataType;
			if (colType == MetaColumnType.Structure && ci.GridColumn.OptionsColumn.AllowEdit)
			{ // edit structure on double click if allowed
				Helpers.EditNonTextMenuItem_Click(null, null);
				return;
			}

			if (IsHtmlColumn(ci)) // edit html cell on double click
				EditHtmlTextCellValue(ci);
		}

		/// <summary>
		/// Set the grid ColumnFilterInfo to match column criteria
		/// </summary>

		internal void SetColumnFilterInfo(ColumnInfo colInfo)
		{
			ColumnFilterInfo cfi;

			if (colInfo.Qc.SecondaryCriteria != "")
			{
				string filterString = // set dummy new properly-formatted filter string that has no matches
					"[" + colInfo.GridColumn.FieldName + "] = '" + TimeOfDay.Milliseconds() + "'";
				string displayText = colInfo.Qc.SecondaryCriteriaDisplay;
				cfi = new ColumnFilterInfo(filterString, displayText);
			}

			else cfi = new ColumnFilterInfo(); // no criteria

			colInfo.GridColumn.FilterInfo = cfi; // set new info for column
																					 //			Invalidate(); // redraw the grid

			return;
		}

		/// <summary>
		/// Clear all filters for the GridView
		/// </summary>

		internal void ClearFilters()
		{
			V.ClearColumnsFilter();
		}

		/// <summary>
		/// Force the XtraGrid to refilter the data
		/// </summary>

		internal void ForceRefilter()
		{
			string filterString = // set dummy new properly-formatted filter string that has no matches
				"[" + "CheckMark" + "] = '" + TimeOfDay.Milliseconds() + "'";
			ColumnFilterInfo cfi = new ColumnFilterInfo(filterString, filterString);

			V.Columns[0].FilterInfo = cfi;
			return;
		}

		/// <summary>
		/// Show/hide chemical structure popup if appropriate and mouse is over a compoundId 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void StructurePopupTimer_Tick(object sender, EventArgs e)
		{
			ToolTipController ttc = ToolTipController;
			Point mouseLocation;
			MoleculeMx mol = null;
			CellInfo ci = null;
			MetaColumn mc = null;
			string structureLabel = "";
			int gr = -1, gc = -1;

			StructurePopupTimer.Enabled = false; // disable timer during processing

			try
			{
				if (Mode == MoleculeGridMode.IgnoreEvents || Mode == MoleculeGridMode.LocalView) return;
				if (Editor != null && !UpdateImmediately) return; // don't do show popup if in UserDataEditor
				if (!StructurePopupEnabled) return;
				if (BGV == null) return;

				if (!WindowsHelper.IsMouseWithinControl(this)) // do correct check of mouse withing control (i.e. control not hidded behind something else)
				{ HideStructurePopup(); return; }

				mouseLocation = PointToClient(Control.MousePosition);
				HitInfo hi = CalcHitInfo(mouseLocation);
				gr = hi.RowHandle;
				if (hi.Column == null || !hi.InRowCell)
				{ HideStructurePopup(); return; }

				gc = hi.Column.AbsoluteIndex;
				if (gr < 0 || gc < 0 ||	this[gr, gc] == null) // anything in cell?
				{ HideStructurePopup(); return; }

				ci = GetCellInfo(gr, gc);
				mc = ci?.Mc;
				if (!ci.IsValidDataCell || mc == null) // associated metacolumn?
				{ HideStructurePopup(); return; }

				Rectangle cellRect = GetCellRect(gr, gc); // cell rectangle relative to grid control
				Rectangle cellScreenRect = RectangleToScreen(cellRect); // cell rectangle in screen coords
				Rectangle r = cellScreenRect;
				int activeWidth = r.Width / 5; // active area is 1/5 of the cell width on the right side of the cell
				Rectangle activePopupAreaRect = new Rectangle(r.Right - activeWidth, r.Top, activeWidth, r.Height);
				Rectangle r2 = activePopupAreaRect;
				Point mp = Control.MousePosition; // in screen coords
				//string msg = String.Format("R: {0}, {1}, {2}, {3}, M: {4}, {5}", r2.Left, r2.Right, r2.Top, r2.Bottom, mp.X, mp.Y); 
					
				if (!activePopupAreaRect.Contains(mp))
				{
					//DebugLog.Message("Hide " + msg);
					HideStructurePopup();
					return;
				}

				//DebugLog.Message("Show " + msg);

				if (gr == StructurePopupRow && gc == StructurePopupColumn) return; // same cell as last time just return

				HideStructurePopup(); // hide current popup

				bool canPopupStruct = (mc.DataType == MetaColumnType.CompoundId); // can popup if compound Id
				canPopupStruct |= // or Smiles or ChimeString
					(mc.DataType == MetaColumnType.String && (Lex.Eq(mc.DataTransform, "PopupSmilesStructure") || Lex.Eq(mc.DataTransform, "PopupChimeStructure")));

				canPopupStruct |= // any structure
					(mc.DataType == MetaColumnType.Structure);

				if (!canPopupStruct) return;

				if (Dtm != null && Dtm.Grid != null && Dtm.Grid.Helpers != null)
					Dtm.Grid.Helpers.AdjustDataRowToRender(ci); // be sure we have the proper row
				if (ci.DataValue == null) return;

				if (gc + 1 < BGV.Columns.Count && GetMetaColumnDataType(gc + 1) == MetaColumnType.Structure && // if structure in the following column
					BGV.Columns[gc + 1].Visible) return; // and it's visible don't show popup

				if (ci.DataValue is CompoundId)
				{
					CompoundId cid = ci.DataValue as CompoundId;
					//string link = cid.Hyperlink;
					//if (link == null || !Lex.Contains(link, "CompoundIdContextMenu")) return; // cell must contain compound number link

					MetaTable root = Rf.Query.Tables[0].MetaTable.Root;
					structureLabel = CompoundId.Format(cid.Value, root);
					mol = MoleculeUtil.SelectMoleculeForCid(cid.Value, root);
				}

				else if (mc.DataType == MetaColumnType.String) // Smiles or Chime
				{
					string molString = ci.DataValue.ToString();
					if (Lex.IsUndefined(molString)) return;

					structureLabel = molString;

					if (Lex.Eq(mc.DataTransform, "PopupSmilesStructure"))
					{
						mol = new MoleculeMx(MoleculeFormat.Smiles, molString);
					}

					else if (Lex.Eq(mc.DataTransform, "PopupChimeStructure"))
					{
						mol = new MoleculeMx(MoleculeFormat.Chime, molString);
					}

					else return;
				}

				else if (mc.DataType == MetaColumnType.Structure)
				{
					mol = ci.DataValue as MoleculeMx;
				}

				else return;

				SuperToolTip stt = null;
				if (mol != null) 
					stt = mol.BuildStructureTooltip(MoleculeMx.DefaultStructurePopupWidth, structureLabel);

				if (stt == null) 
				{ // just hide if no structure
					HideStructurePopup();
					//DebugLog.Message("Hide: No Structure");
					return;
				}

				// Build and display the popup

				ToolTipControllerShowEventArgs ttcArgs = ToolTipUtil.BuildSuperTooltipArgs(stt, this); 
				ttcArgs.ToolTipLocation = ToolTipLocation.BottomRight;
				ttc.HideHint(); // be sure any existing tooltip is hidden first

				Point p = new Point(cellScreenRect.Right, cellScreenRect.Top - 22);
				ttc.ShowHint(ttcArgs, p);

				this.Refresh(); // necessary to refresh grid to prevent ghosting of popup
				Application.DoEvents();

				StructurePopupRow = gr; // save popup row & col
				StructurePopupColumn = gc;

				//DebugLog.Message("Showing");
			}
			catch (Exception ex)
			{
				return; // just ignore error for now
			}

			finally // if control is still visible enable timer otherwise just hide popup
			{
				if (Visible)
					StructurePopupTimer.Enabled = true; // reenable timer

				else HideStructurePopup();
			}
		}

		/// <summary>
		/// Check if hint is visible (via DevExpress)
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		private bool IsHintVisible(ToolTipController controller)
		{
			System.Reflection.FieldInfo fi = typeof(ToolTipController).GetField("toolWindow", BindingFlags.NonPublic | BindingFlags.Instance);
			DevExpress.Utils.Win.ToolTipControllerBaseWindow window = fi.GetValue(controller) as DevExpress.Utils.Win.ToolTipControllerBaseWindow;
			return window != null && !window.IsDisposed && window.Visible;
		}

		HitInfo CalcHitInfo(Point location)
		{
			return HitInfo.CalcHitInfo(this, location);
		}

		/// <summary>
		/// Stop grid display
		/// </summary>

		public void StopGridDisplay()
		{
			if (CheckMarksChanged)
			{
				DialogResult dr =
					MessageBoxMx.Show("Do you want to save a list of the compounds that you have marked?", "Save Marked Compounds",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr == DialogResult.Yes)
					SaveMarkedGridItems(false, null); // save as persisted list
			}

			if (QueryManager != null)
			{
				if (QueryManager.DataTableManager != null)
					QueryManager.DataTableManager.CancelRowRetrieval();

				if (Formatter != null && Dtm.DataTableFetchPosition >= 0)
					UsageDao.LogEvent("QuerySearchRowsFetched", Dtm.DataTableFetchPosition.ToString());
			}

			return;
		}

		/// <summary>
		/// Mark all items
		/// </summary>
		/// <param name="prompt"></param>

		internal void MarkAllItems(bool prompt, bool markValue)
		{
			Dtm.UpdateFilterState();
			if (Dtm.MarkedKeyCount == Dtm.KeyCount || Dtm.MarkedKeyCount == 0)
				prompt = false; // don't prompt if all set or all clear

			if (prompt)
			{
				DialogResult dr = XtraMessageBox.Show(
					"All of your existing marks will be reset.\n" +
					"Do you want to continue?", "Mark All Rows",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (dr != DialogResult.Yes) return;
			}

			CheckMarkInitially = markValue;
			Dtm.MarkAllRows(markValue);
			CheckMarksChanged = true;

			return;
		}

		/// <summary>
		/// Save marked grid items in list
		/// </summary>

		internal void SaveMarkedGridItems(
			bool saveToTempList,
			string tempListName)
		{
			CidList list = GetMarkedList();
			if (list == null || list.Count == 0)
			{
				MessageBoxMx.ShowError("No items are marked.");
				return;
			}

			UserObject uo = CidListCommand.SaveList(list, "Save marked items", saveToTempList, tempListName);
			if (uo != null)
				CheckMarksChanged = false; // reset change flag
			return;
		}

		/// <summary>
		/// Add the set of marked items to an existing list
		/// </summary>

		internal void AddMarkedGridItemsToList(string tempListName)
		{
			UserObject uo;
			CidList list;

			if (!Lex.IsNullOrEmpty(tempListName)) list = CidListCommand.Read(UserObject.TempFolderNameQualified + tempListName);
			else
			{
				uo = CidListCommand.SelectListDialog("Existing List to Add Marked Items to");
				if (uo == null) return;

				if (!UserObjectUtil.UserHasWriteAccess(uo))
				{
					MessageBoxMx.ShowError("You are not authorized to modify " + uo.Name);
					return;
				}

				list = CidListCommand.Read(uo);
			}

			if (list == null) return;
			CidList checkedItems = GetMarkedList();

			list.Union(checkedItems);
			CidListCommand.Write(list, list.UserObject);

			CheckMarksChanged = false; // reset change flag
			return;
		}

		/// <summary>
		/// Remove the set of marked items from an existing list
		/// </summary>

		internal void RemoveMarkedGridItemsFromList(string tempListName)
		{
			UserObject uo;
			CidList list;

			if (!Lex.IsNullOrEmpty(tempListName)) list = CidListCommand.Read(UserObject.TempFolderNameQualified + tempListName);
			else
			{
				uo = CidListCommand.SelectListDialog("Existing List to Remove Marked Items from");
				if (uo == null) return;

				if (!UserObjectUtil.UserHasWriteAccess(uo))
				{
					MessageBoxMx.ShowError("You are not authorized to modify " + uo.Name);
					return;
				}

				list = CidListCommand.Read(uo);
			}

			CidList checkedItems = GetMarkedList();

			list.Difference(checkedItems);
			CidListCommand.Write(list, list.UserObject);

			CheckMarksChanged = false; // reset change flag
			return;
		}

		public List<string> GetColumnValues(ArrayList rows, DataColumn dataColumn)
		{
			List<string> values = new List<string>();

			foreach (DataRowMx row in rows)
			{
				string cellValue = "";
				Object obj = row[dataColumn.ColumnName];
				cellValue = obj.ToString();
				if (!values.Contains(cellValue)) values.Add(cellValue);
			}
			return values;
		}

		public ArrayList GetCheckedRows()
		{
			ArrayList rows = new ArrayList();
			foreach (DataRowMx dataRow in DataTable.Rows)
			{
				object obj = dataRow[Rf.CheckMarkVoPos];
				if (!(obj is bool)) continue;
				bool rowChecked = (bool)obj;
				if (rowChecked)
					rows.Add(dataRow);
			}
			return rows;
		}

		/// <summary>
		/// Get list of marked items
		/// </summary>
		/// <returns></returns>

		public CidList GetMarkedList()
		{
			CidList markedList = new CidList();
			CidList unmarkedList = new CidList();
			if (Rf.CheckMarkVoPos < 0) return markedList;

			for (int ti = 0; ti < DataTable.Rows.Count; ti++)
			{
				DataRowMx dr = DataTable.Rows[ti];
				//DataRowAttributes dra = Dtm.GetRowAttributes(dr);
				string key = Dtm.GetRowKey(dr);
				if (String.IsNullOrEmpty(key)) continue;
				object o = dr[Rf.CheckMarkVoPos];
				if (!(o is bool)) continue;

				bool marked = (bool)o;
				if (marked) // this row is marked
				{
					if (CheckMarkInitially)
					{ // if marked & default is marked then add to marked list if not already in unmarked or marked list
						if (!unmarkedList.Contains(key) && !markedList.Contains(key))
							markedList.Add(key);
					}
					else // default is unmarked so marked overrides unmarked
					{
						if (!markedList.Contains(key)) markedList.Add(key); // add to marked
						if (unmarkedList.Contains(key)) unmarkedList.Remove(key); // remove from unmarked
					}
				}

				else // this row is not marked
				{
					if (!CheckMarkInitially)
					{ // if unmarked & default is unmarked then add to unmarked list if not already in unmarked or marked list
						if (!markedList.Contains(key) && !unmarkedList.Contains(key))
							unmarkedList.Add(key);
					}
					else // default is marked so unmarked overrides marked
					{
						if (!unmarkedList.Contains(key)) unmarkedList.Add(key); // add to unmarked
						if (markedList.Contains(key)) markedList.Remove(key); // remove from marked
					}
				}
			}

			if (CheckMarkInitially && Dtm.ResultsKeys != null)
			{ // if marking then include any keys not read in yet
				foreach (string key in Dtm.ResultsKeys)
				{
					if (!markedList.Contains(key) && !unmarkedList.Contains(key))
						markedList.Add(key);
				}
			}
			return markedList;
		}

		/// <summary>
		/// MouseMove - StructurePopup display was controled from this event;
		/// however, move out of control was sometimes missed and popup display remained visible
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MoleculeGridControl_MouseMove(object sender, MouseEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Enable/disable StructurePopup / timer based on control visibility change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MoleculeGridControl_VisibleChanged(object sender, EventArgs e)
		{
			EnableStructurePopup(Visible);
		}

		private void MoleculeGridControl_Enter(object sender, EventArgs e)
		{
			EnableStructurePopup(true);
			//DebugLog.Message("Hide: Control_Enter");
		}

		private void MoleculeGridControl_Leave(object sender, EventArgs e)
		{
			EnableStructurePopup(false);
			//DebugLog.Message("Hide: Control_Leave");
		}

		private void MoleculeGridControl_MouseEnter(object sender, EventArgs e)
		{
			EnableStructurePopup(true);
			//DebugLog.Message("Hide: Control_MouseEnter");
		}

		private void MoleculeGridControl_MouseLeave(object sender, EventArgs e)
		{
			EnableStructurePopup(false);
			//DebugLog.Message("Hide: Control_MouseLeave");
		}

		void EnableStructurePopup(bool enable)
		{
			if (SystemUtil.InDesignMode) return; // not if in design mode

			if (enable)
			{
				StructurePopupTimer.Enabled = true;
			}

			else
			{
				StructurePopupTimer.Enabled = false;
				HideStructurePopup();
			}
		}

		/// <summary>
		/// Hide structure popup control
		/// </summary>

		void HideStructurePopup()
		{
			ToolTipController ttc = ToolTipController;
			if (ttc != null) ttc.HideHint();

			StructurePopupRow = StructurePopupColumn = -1; // indicate no being shown
		}

		/// <summary>
		/// Scroll to top of grid
		/// </summary>

		public void ScrollToTop()
		{
			V.FocusedRowHandle = 0;
		}

		/// <summary>
		/// Scroll to bottom of grid
		/// </summary>

		public void ScrollToBottom()
		{
			if (Dtm.RowRetrievalState != RowRetrievalState.Complete &&
				QueryManager.QueryEngine != null)
			{
				Dtm.CompleteRetrieval();
				Progress.Hide();
			}
			V.FocusedRowHandle = V.RowCount - 1;
		}

		/// <summary>
		/// Free resources linked to this instance
		/// </summary>

		public new void Dispose()
		{
			try
			{
				if (QueryManager != null)
					QueryManager = null;

				//EditStructureHiddenControl = CopyPasteHiddenControl = null;
				if (StructurePopupTimer != null) StructurePopupTimer.Enabled = false;

				DataTableMx dt = _dataSource;
				if (dt != null)
				{
					dt.RowChanged -= Dt_RowChanged; // remove event reference
					_dataSource = null;
				}

				if (Helpers != null)
				{
					Helpers.Dispose();
					Helpers = null;
				}

				while (ViewCollection.Count > 0)
				{
					ViewCollection[0].Dispose();
				}

				base.Dispose(); // call Dispose for underlying DevExpress GridControl

				return;
			}

			catch (Exception ex) { DebugLog.Message(ex); }
		}

	}

	/// <summary>
	/// Basic modes for molecule grid
	/// </summary>

	public enum MoleculeGridMode
	{
		QueryView = 1, // full event & menu processing for query results
		DataSetView = 2, // limited dataset viewing
		LocalView = 3, // local editing of data within grid
		IgnoreEvents = 4 // ignore all events & pass through to parent form
	}


	/// <summary>
	/// HitInfo that combines the BandedGridView and the LayoutView
	/// </summary>

	public class HitInfo
	{
		internal GridHitInfo Ghi;
		internal BandedGridHitInfo Bghi;
		internal CardHitInfo Chi;
		internal LayoutViewHitInfo Lhi;

		public static HitInfo CalcHitInfo(GridControl ctl, Point location)
		{
			HitInfo hi = new HitInfo();
			BaseView view = ctl.MainView;

			if (view is BandedGridView)
				hi.Bghi = (view as BandedGridView).CalcHitInfo(location);

			else if (view is GridView)
				hi.Ghi = (view as GridView).CalcHitInfo(location);

			else if (view is LayoutView)
				hi.Lhi = (view as LayoutView).CalcHitInfo(location);

			else if (view is CardView)
				hi.Chi = (view as CardView).CalcHitInfo(location);

			else throw new Exception("Unexpected grid view type");

			return hi;
		}

		public BandedGridHitTest HitTest
		{
			get
			{
				if (Ghi != null) return (BandedGridHitTest)Ghi.HitTest;
				else if (Bghi != null) return Bghi.HitTest;
				else return BandedGridHitTest.None; // note: may want to return something for Lhi/Chi
			}
		}

		public bool InRow
		{
			get
			{
				if (Ghi != null) return Ghi.InRow;
				else if (Bghi != null) return Bghi.InRow;
				else return false;
			}
		}

		public bool InRowCell
		{
			get
			{
				if (Ghi != null) return Ghi.InRowCell;
				else if (Bghi != null) return Bghi.InRowCell;
				else if (Lhi != null) return Lhi.InCard;
				else if (Chi != null) return Chi.InCard;
				else return false;
			}
		}


		public int RowHandle
		{
			get
			{
				if (Ghi != null) return Ghi.RowHandle;
				else if (Bghi != null) return Bghi.RowHandle;
				else if (Lhi != null) return Lhi.RowHandle;
				else if (Chi != null) return Chi.RowHandle;
				else return -1;
			}
		}

		public GridColumn Column
		{
			get
			{
				if (Ghi != null) return Ghi.Column;
				else if (Bghi != null) return Bghi.Column;
				else if (Lhi != null) return Lhi.Column;
				else if (Chi != null) return Chi.Column;
				else return null;
			}
		}

		public bool InColumn
		{
			get
			{
				if (Ghi != null) return Ghi.InColumn;
				else if (Bghi != null) return Bghi.InColumn;
				else if (Lhi != null) return Lhi.InCardCaption; // treat caption like column header
				else if (Chi != null) return Chi.InCardCaption;
				else return false;
			}
		}

		public bool InBandPanel
		{
			get
			{
				if (Bghi != null) return Bghi.InBandPanel;
				else return false;
			}
		}

		public GridBand Band
		{
			get
			{
				if (Bghi != null) return Bghi.Band;
				else return null;
			}
		}
	}

	/// <summary>
	/// Basic information on a grid cell and its associated DataTable cell and ResultsField
	/// </summary>

	public class ColumnInfo
	{
		public int GridColAbsoluteIndex = -1; // column index with GridView
		public GridColumn GridColumn; // GridColumn in GridView

		public int DataColIndex = -1; // column index in DataTable
		public DataColumn DataColumn; // DataColumn in DataTable

		public int TableIndex = -1; // index of table within report tables
		public ResultsTable Rt;
		public int FieldIndex = -1;
		public ResultsField Rfld;

		public QueryTable Qt;
		public QueryColumn Qc;

		public MetaTable Mt;
		public MetaColumn Mc;
	}

	/// <summary>
	/// Basic information on a grid cell and its associated DataTable cell and ResultsField
	/// </summary>

	public class CellInfo : ColumnInfo
	{
		public int GridRowHandle = -1; // row index within GridView

		public int DataRowIndex = -1; // row index within data
		public object DataValue; // data value: DataTable.Rows[DataRowIndex][DataColIndex]

		public bool IsValidDataCell
		{
			get
			{
				if (DataRowIndex >= 0 && DataColIndex >= 0) return true;
				else return false;
			}
		}
	}
}