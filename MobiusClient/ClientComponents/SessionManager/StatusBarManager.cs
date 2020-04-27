using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using Dx = DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraBars.Ribbon.ViewInfo;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Handle status bar interactions
	/// </summary>

	public class StatusBarManager
	{
		public QueryManager QueryManager; // current QueryManager associated with status bar
		public QueryResultsControl Qrc { get { return QueryManager != null ? QueryManager.QueryResultsControl : null; } } // the current queryresults control
		public bool LockResultsKeys = false; // if true lock results keys from update

		public RibbonControl RibbonCtl { get { return SessionManager.Instance.RibbonCtl; } } // any ribbon control on main form (eliminate if filters generalize to any form)

		public RowRetrievalState previousRowRetrievalState = RowRetrievalState.UnInitialized;
		public MarqueeProgressBarControl RetrievalProgressBar; // shows progress of retrieval
		public SimpleButton RetrievalProgressButton; // shows progress of retrieval (complete, paused);
		public BarButtonItem RowCountCtl; // where row count is displayed
		public RibbonStatusBar StatusBarCtl; // any status bar control
		public BarButtonItem ClearFiltersCtl; // button that clears filters
		public BarButtonItem DatabaseSubsetButton; // button with database subset state
		public BarEditItem FiltersEnabledCtl; // checkbox to enable/disable filters
		public BarStaticItem FilterStringCtl; // current filterstring

		public BarButtonItem ZoomButtonItem; // the zoom button
		public BarStaticItem ZoomPctBarItem; // label for zoom slider
		public BarEditItem ZoomSliderBarItem; // zoom slider for zooming results
		public TrackBarControl ZoomSliderCtl; // instance of zoom slider created from ZoomSliderBarItem

		//public ChartTools ChartTools;
		public BarButtonItem ChartSelectButton;
		public BarButtonItem ChartZoomButton;
		public BarButtonItem ChartTranslateButton;
		public BarButtonItem ChartRotateButton;
		public BarButtonItem ChartResetViewButton;

		public static Exception LastException = null;

		public void SetupStatusControls(
			MarqueeProgressBarControl retrievalProgressBar,
			SimpleButton retrievalProgressButton,
			BarButtonItem rowCountCtl,
			BarButtonItem databaseSubsetButtonItem,
			BarButtonItem clearFiltersCtl,
			BarEditItem filtersEnabledCtl,
			BarStaticItem filterStringCtl)
		{
			RetrievalProgressBar = retrievalProgressBar;
			RetrievalProgressButton = retrievalProgressButton;
			RowCountCtl = rowCountCtl;
			DatabaseSubsetButton = databaseSubsetButtonItem;
			ClearFiltersCtl = clearFiltersCtl;
			FiltersEnabledCtl = filtersEnabledCtl;
			FilterStringCtl = filterStringCtl;

			// Setup event handlers & hide initially

			if (RetrievalProgressButton != null) // add handler for button that clears filters
			{
				RetrievalProgressButton.Click += // add item to handle clear click
					new EventHandler(RetrievalProgressButton_ItemClick);
			}

			if (RowCountCtl != null) // add handler for click on row count
			{
				RowCountCtl.ItemClick += // add item to handle clear click
					new DevExpress.XtraBars.ItemClickEventHandler(RowCountCtl_ItemClick);

				ClearFiltersCtl.Visibility = BarItemVisibility.Never;
			}

			if (DatabaseSubsetButton != null) // add handler for click on row count
			{
				DatabaseSubsetButton.ItemClick += // add item to handle clear click
					new DevExpress.XtraBars.ItemClickEventHandler(DatabaseSubsetButton_ItemClick);

				DatabaseSubsetButton.Visibility = BarItemVisibility.Never;
			}

			if (ClearFiltersCtl != null) // add handler for button that clears filters
			{
				ClearFiltersCtl.ItemClick += // add item to handle clear click
					new DevExpress.XtraBars.ItemClickEventHandler(ClearFilters_ItemClick);

				ClearFiltersCtl.Visibility = BarItemVisibility.Never;
			}

			if (FiltersEnabledCtl != null)
			{
				FiltersEnabledCtl.ShownEditor += // add event to handle checkbox click
					new DevExpress.XtraBars.ItemClickEventHandler(FiltersEnabledCtl_ShownEditor);

				FiltersEnabledCtl.Visibility = BarItemVisibility.Never;
			}

			if (FilterStringCtl != null)
				FilterStringCtl.Visibility = BarItemVisibility.Never;

		}


		/// <summary>
		/// SetupViewZoomControls
		/// </summary>
		/// <param name="zoomButtonItem"></param>
		/// <param name="zoomPctBarItem"></param>
		/// <param name="zoomSliderBarItem"></param>

		public void SetupViewZoomControls(
			BarButtonItem zoomButtonItem,
			BarStaticItem zoomPctBarItem,
			BarEditItem zoomSliderBarItem)
		{
			ZoomButtonItem = zoomButtonItem;
			ZoomPctBarItem = zoomPctBarItem;
			ZoomSliderBarItem = zoomSliderBarItem;

			if (ZoomButtonItem != null)
				ZoomButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ZoomButtonItem_ItemClick);

			if (ZoomPctBarItem != null) // clicking on pct brings up zoom dialog
				ZoomPctBarItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ZoomButtonItem_ItemClick);

			if (ZoomSliderBarItem != null)
				ZoomSliderBarItem.ShownEditor += new DevExpress.XtraBars.ItemClickEventHandler(ZoomSliderBarItem_ShownEditor);
		}

		/// <summary>
		/// SetupChartViewTools
		/// </summary>
		/// <param name="chartSelectButton"></param>
		/// <param name="chartZoomButton"></param>
		/// <param name="chartTranslateButton"></param>
		/// <param name="chartRotateButton"></param>
		/// <param name="chartResetViewButton"></param>

		////public void SetupChartViewTools(
		////  BarButtonItem chartSelectButton,
		////  BarButtonItem chartZoomButton,
		////  BarButtonItem chartTranslateButton,
		////  BarButtonItem chartRotateButton,
		////  BarButtonItem chartResetViewButton)
		////{
		////  ChartSelectButton = chartSelectButton;
		////  ChartZoomButton = chartZoomButton;
		////  ChartTranslateButton = chartTranslateButton;
		////  ChartRotateButton = chartRotateButton;
		////  ChartResetViewButton = chartResetViewButton;

		////  ChartTools ct = ChartTools = new ChartTools();

		////  if (ChartSelectButton != null)
		////    ChartSelectButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ct.ChartSelectButton_ItemClick);

		////  if (ChartZoomButton != null)
		////    ChartZoomButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ct.ChartZoomButton_ItemClick);

		////  if (ChartTranslateButton != null)
		////    ChartTranslateButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ct.ChartTranslateButton_ItemClick);

		////  if (ChartRotateButton != null)
		////    ChartRotateButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ct.ChartRotateButton_ItemClick);

		////  if (ChartResetViewButton != null)
		////    ChartResetViewButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ct.ChartResetViewButton_ItemClick);
		////}
		///

		public delegate void DisplayStatusMessageDelegate(string message);

		/// <summary>
		/// Show status message, can be called from non-ui thread
		/// </summary>
		/// <param name="txt"></param>

		public void DisplayStatusMessage(
		string txt)
		{
			if (!SS.I.Attended) return;
			try
			{
				StatusBarCtl.Invoke(new DisplayStatusMessageDelegate(DisplayStatusMessageDelegateMethod), txt);
			}
			catch (Exception ex)
			{
				LastException = ex;
			}
			return;
		}

		/// <summary>
		/// Show status message 
		/// </summary>
		/// <param name="txt"></param>

		public void DisplayStatusMessageDelegateMethod(
			string txt)
		{
			if (!SS.I.Attended) return;
			if (RowCountCtl == null) return;

			try
			{
				if (!String.IsNullOrEmpty(txt))
				{
					RowCountCtl.Visibility = BarItemVisibility.Always;
					RowCountCtl.Caption = txt;
					RowCountCtl.Refresh();
				}

				else DisplayFilterCounts(true); // redisplay any filter counts
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

			return;
		}

		/// <summary>
		/// Show the database subset search domain in the status bar
		/// </summary>
		/// <param name="txt"></param>

		public void DisplaySearchDomain()

		{
			try
			{
				if (Lex.IsNullOrEmpty(SS.I.DatabaseSubsetListName))
					DatabaseSubsetButton.Visibility = BarItemVisibility.Never;

				else if (Lex.Eq(SS.I.DatabaseSubsetListName, "Current"))
				{
					DatabaseSubsetButton.Caption = "Subset: " + "(" + SS.I.DatabaseSubsetListSize + ")";
					DatabaseSubsetButton.Visibility = BarItemVisibility.Always;
				}

				else // subset on a list
				{
					DatabaseSubsetButton.Caption = "Subset: " + SS.I.DatabaseSubsetListName + " (" + SS.I.DatabaseSubsetListSize + ")";
					DatabaseSubsetButton.Visibility = BarItemVisibility.Always;
				}
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

			return;
		}

		/// <summary>
		/// Display current list hit count
		/// </summary>

		public void DisplayCurrentCount()
		{
			//QueryManager qm = null;
			//if (Qrc != null) qm = Qrc.CurrentViewQm;

			try
			{

				QueryManager qm = QueryManager;
				bool resetCounts = true;
				if (qm == null || qm.DataTableManager == null || qm.Query == null)
					resetCounts = false;
				else if (qm.Query.Mode == QueryMode.Browse && qm.DataTableManager.RowCount > 0)
					resetCounts = false;

				if (resetCounts)
				{
					DataTableManager dtm = qm.DataTableManager;
					if (dtm.ResultsKeys != null)
						dtm.KeyCount = dtm.ResultsKeys.Count;
					else dtm.KeyCount = -1;

					dtm.PassedFiltersKeyCount = -1;
					dtm.RowCount = -1;
					dtm.PassedFiltersRowCount = -1;
				}

				bool showFilterString = true;
				if (qm == null || qm.Query == null || qm.Query.Mode != QueryMode.Browse) // show filter string if browse mode only
					showFilterString = false;
				DisplayFilterString(showFilterString);

				DisplayFilterCounts(true);
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

		}

		/// <summary>
		/// Update the filterstring & row counts on the status bar
		/// </summary>

		public void DisplayFilterCountsAndString()
		{
			DisplayFilterCountsAndString(true);
		}
		/// <summary>
		/// Update the filterstring & row counts on the status bar
		/// </summary>

		public void DisplayFilterCountsAndString(bool show)
		{
			DisplayFilterCounts(show);
			DisplayFilterString(show);
		}

		/// <summary>
		/// Display the current row counts in the status bar
		/// </summary>

		public void DisplayFilterCounts()
		{
			DisplayFilterCounts(true);
		}

		/// <summary>
		/// Display the current row counts in the status bar
		/// </summary>

		public void DisplayFilterCounts(bool show)
		{
			if (!SS.I.Attended) return;
			if (RowCountCtl == null) return;

			try
			{
				if (!show)
				{
					RowCountCtl.Visibility = BarItemVisibility.Never;
					DisplayRetrievalProgressState(RowRetrievalState.Undefined);
					return;
				}

				QueryManager qm = QueryManager;
				if (qm != null && qm.Query != null && qm.Query.Mode == QueryMode.Browse && // if in browse mode get the query manager for the current view 
				 Qrc != null && Qrc.CrvQm != null)
					qm = Qrc.CrvQm;

				if (qm == null || qm.DataTableManager == null)
				{ // nothing running
					DisplayRetrievalProgressState(RowRetrievalState.Undefined);
					RowCountCtl.Caption = "";
					return;
				}

				DataTableManager dtm = qm.DataTableManager;
				RowRetrievalState state = RowRetrievalState.Complete;
				if (dtm.Query != null && dtm.Query.Mode == QueryMode.Browse)
					state = dtm.RowRetrievalState;
				else if (dtm.KeyCount < 0) state = RowRetrievalState.Undefined;
				DisplayRetrievalProgressState(state);

				String txt = "";
				if (dtm.KeyCount >= 0)
				{ // display count of key values retrieved and total key value count
					string keyName = MetaTable.PrimaryRootTable; // "Key Value" (start with specific compound id rather than generic "Key Value");
					if (qm.Query != null && !String.IsNullOrEmpty(qm.Query.KeyColumnLabel))
						keyName = qm.Query.KeyColumnLabel;

					if (keyName.EndsWith("."))
						keyName = keyName.Substring(0, keyName.Length - 1) + "s.";
					else keyName += "s";
					txt += keyName + ": ";
					if (dtm.RowRetrievalComplete && dtm.FiltersEnabled && dtm.PassedFiltersKeyCount >= 0 && dtm.PassedFiltersKeyCount < dtm.KeyCount)
						txt += FIC(dtm.PassedFiltersKeyCount) + "/"; // count of reduced set of keys passing filter

					if (MqlUtil.SingleStepExecution(qm.Query) && !LockResultsKeys) // get accurate list and count of keys since may be out of order for single step with list criteria
					{
						dtm.ResultsKeys = dtm.GetResultsKeysFromDataTable();
						dtm.KeyCount = dtm.ResultsKeys.Count;
					}
					txt += FIC(dtm.KeyCount); // count of keys in table

					//if (dtm.KeyCount > 500) dtm = dtm; // debug

					if (dtm.ResultsKeys != null && dtm.ResultsKeys.Count > dtm.KeyCount && // include total keys if greater than currently retrieved keys
					 !MqlUtil.SingleStepExecution(qm.Query))
						txt += "/" + FIC(dtm.ResultsKeys.Count);
				}

				if (dtm.RowCount >= 0 && (dtm.RowCount != dtm.KeyCount || dtm.PassedFiltersRowCount != dtm.PassedFiltersKeyCount))
				{ // display count of rows and total rows
					if (txt != "") txt += "; ";
					txt += "Rows: ";
					if (dtm.PassedFiltersRowCount >= 0 && dtm.PassedFiltersRowCount < dtm.RowCount)
						txt += FIC(dtm.PassedFiltersRowCount) + "/";
					txt += FIC(dtm.RowCount);
				}

				RowCountCtl.Visibility = BarItemVisibility.Always;
				RowCountCtl.Caption = txt;
				RowCountCtl.Refresh();
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

			return;
		}

		static string FIC(int n)
		{
			return StringMx.FormatIntegerWithCommas(n);
		}

		/// <summary>
		/// DisplayRetrievalProgressStateAndFilterCounts 
		/// </summary>
		/// <param name="state"></param>

		public void DisplayRetrievalProgressStateAndFilterCounts(RowRetrievalState state)
		{
			DisplayRetrievalProgressState(state);
			DisplayFilterCounts();
		}

		/// <summary>
		/// Show proper row retrieval state image
		/// </summary>
		/// <param name="state"></param>

		public void DisplayRetrievalProgressState(RowRetrievalState state)
		{
			if (RetrievalProgressButton == null || RetrievalProgressBar == null || StatusBarCtl == null) return;

			if (state == previousRowRetrievalState) return;

			try
			{
				AdjustRetrievalProgressBarLocation();

				if (state == RowRetrievalState.Running)
				{
					RetrievalProgressBar.Visible = true;
					RetrievalProgressButton.Visible = false;
				}

				else if (state == RowRetrievalState.Paused)
				{
					RetrievalProgressBar.Visible = false;
					RetrievalProgressButton.Visible = true;
					RetrievalProgressButton.ImageIndex = 1;
				}

				else if (state == RowRetrievalState.Complete)
				{
					RetrievalProgressBar.Visible = false;
					RetrievalProgressButton.Visible = true;
					RetrievalProgressButton.ImageIndex = 0;
				}

				else // not visible
				{
					RetrievalProgressButton.Visible = false;
					RetrievalProgressBar.Visible = false;
				}

				previousRowRetrievalState = state;

				return;
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

			//if (SS.I.UISetupLevel < 0)
			//  try { Application.DoEvents(); }
			//  catch {}

			return;
		}

		/// <summary>
		/// Set the location for the progress controls
		/// </summary>
		/// 
		public void AdjustRetrievalProgressBarLocation()
		{
			try
			{
				if (RetrievalProgressButton == null || RetrievalProgressBar == null || StatusBarCtl == null) return;

				RibbonStatusBarViewInfo vi = StatusBarCtl.ViewInfo.Items.Owner as RibbonStatusBarViewInfo;
				if (vi == null || vi.Items.Count == 0) return;

				Rectangle r = vi.Items[0].Bounds; // get bounds of first item
				int ctr = r.Top + r.Height / 2; // relative center of status bar
				int top = StatusBarCtl.Bounds.Top + ctr - RetrievalProgressBar.Height / 2;
				RetrievalProgressBar.Top = top;
				RetrievalProgressButton.Location = RetrievalProgressBar.Location;
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

		}

		/// <summary>
		/// Handle click on progress button in statusbar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void RetrievalProgressButton_ItemClick(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("List EditTemp Current");
		}

		/// <summary>
		/// Handle click on row count in status bar by displaying associated context menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RowCountCtl_ItemClick(object sender, ItemClickEventArgs e)
		{
			SessionManager sm = SessionManager.Instance;
			if (sm == null) return;
			if (sm.RibbonCtl == null) return;
			if (sm.MainMenuControl == null) return;
			Point p = new Point(e.Link.ScreenBounds.Left, e.Link.ScreenBounds.Bottom);
			sm.MainMenuControl.StatusBarCurrrentListContextMenu.ShowPopup(p);
			return;
		}

		/// <summary>
		/// Handle click on DatabaseSubset button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DatabaseSubsetButton_ItemClick(object sender, ItemClickEventArgs e)
		{
			SessionManager sm = SessionManager.Instance;
			if (sm == null) return;
			if (sm.RibbonCtl == null) return;
			if (sm.MainMenuControl == null) return;

			MainMenuControl mmc = sm.MainMenuControl;

			mmc.CurrentListCheckItem.Checked = mmc.OtherListCheckItem.Checked =
			 mmc.SubsetAllCheckItem.Checked = false;

			string name = SS.I.DatabaseSubsetListName;
			if (Lex.IsNullOrEmpty(name))
				mmc.SubsetAllCheckItem.Checked = true;

			else if (Lex.Eq(name, "Current"))
				mmc.CurrentListCheckItem.Checked = true;

			else mmc.OtherListCheckItem.Checked = true;

			Point p = new Point(e.Link.ScreenBounds.Left, e.Link.ScreenBounds.Bottom);
			sm.MainMenuControl.StatusBarDatabaseSubsetContextMenu.ShowPopup(p);
			return;
		}

		/// <summary>
		/// Display filter string & associated controls
		/// </summary>
		/// <param name="filterString"></param>

		public void DisplayFilterString(bool show)
		{
			if (Qrc == null || Qrc.CrvQm == null) return;

			try
			{
				Query q = Qrc.CrvQm.Query;
				string filterString = q.BuildSecondaryFilterString();
				if (!String.IsNullOrEmpty(filterString))
					filterString = "Filters: " + filterString;
				QueryResultsControl rvc = Qrc;

				BarItemVisibility biv;

				if (String.IsNullOrEmpty(filterString) || !show) biv = BarItemVisibility.Never;
				else biv = BarItemVisibility.Always;

				if (ClearFiltersCtl != null) // add handler for button that clears filters
					ClearFiltersCtl.Visibility = biv;

				if (FiltersEnabledCtl != null)
				{
					FiltersEnabledCtl.Visibility = biv;
					FiltersEnabledCtl.EditValue = Qrc.CrvQm.DataTableManager.FiltersEnabled;
				}

				if (FilterStringCtl != null)
				{
					FilterStringCtl.Caption = filterString;
					FilterStringCtl.Visibility = biv;
				}
			}

			catch (Exception ex)
			{
				LastException = ex;
			}

			return;
		}

		/// <summary>
		/// ClearFilters button pressed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ClearFilters_ItemClick(object sender, ItemClickEventArgs e)
		{
			SessionManager.Instance.ClearFilters();
		}

		/// <summary>
		/// When the FiltersEnabledCtl editor is shown add an event handler to process user input on a per-click basis
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FiltersEnabledCtl_ShownEditor(object sender, ItemClickEventArgs e)
		{
			CheckEdit edit = RibbonCtl.Manager.ActiveEditor as CheckEdit;
			if (edit == null) return;

			try
			{
				edit.EditValueChanged -= // remove any existing handler
					new System.EventHandler(FiltersEnabled_EditValueChanged);
			}

			catch (Exception ex) { ex = ex; }

			edit.EditValueChanged += new System.EventHandler(FiltersEnabled_EditValueChanged);
			return;
		}


		/// <summary>
		/// When the retrieval progress control is shown get its underlying control for modification
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RetrievalProgressCtl_ShownEditor(object sender, ItemClickEventArgs e)
		{
			MarqueeProgressBarControl mpbc = RibbonCtl.Manager.ActiveEditor as MarqueeProgressBarControl;
			if (mpbc == null) return;

			try
			{
				mpbc.Height = 14;
			}

			catch (Exception ex) { ex = ex; }

			return;
		}

		/// <summary>
		/// Set the value of FiltersEnabledCtl
		/// </summary>
		/// <param name="value"></param>
		/// 
		internal void SetFiltersEnabledCtlValue(bool value)
		{
			if (FiltersEnabledCtl != null)
				FiltersEnabledCtl.EditValue = value;
		}

		/// <summary>
		/// Handle change in value for FiltersEnabled Ctl
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FiltersEnabled_EditValueChanged(object sender, EventArgs e)
		{
			if (Qrc.CrvQm == null || Qrc.CrvQm.DataTableManager == null) return;

			CheckEdit edit = RibbonCtl.Manager.ActiveEditor as CheckEdit;
			if (edit == null) return;

			SessionManager.Instance.ActivateQuickSearchControl(); // move focus away so status is properly redrawn 
			SessionManager.Instance.EnableFilters(edit.Checked); // (fixed width of 220 for filter count string also works)
		}

		/// <summary>
		/// Clicked on zoom button in status bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void ZoomButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			int pct;

			Query q = QueriesControl.BaseQuery;
			if (q == null) return;

			string txt = ZoomPctBarItem.Caption;
			txt = txt.Replace("%", "");
			if (Lex.IsInteger(txt)) pct = int.Parse(txt);
			else pct = q.ViewScale; // use existing query value if above fails

			DialogResult dr = ZoomDialog.Show(ref pct, new Point(), QueryManager);
			if (dr == DialogResult.Cancel) return;

			ZoomSliderPct = pct; // update zoom control display

			if (QueryManager == null || Qrc == null) return;

			Qrc.CurrentView.ScaleView(pct);

			return;
		}

		/// <summary>
		/// Set zoom controls visibility
		/// </summary>

		public bool ZoomControlVisible
		{
			set
			{
				BarItemVisibility v;

				try
				{
					v = value ? BarItemVisibility.Always : BarItemVisibility.Never; // set visibility of grid zoom
					if (ZoomButtonItem != null) ZoomButtonItem.Visibility = v;
					if (ZoomPctBarItem != null) ZoomPctBarItem.Visibility = v;
					if (ZoomSliderBarItem != null) ZoomSliderBarItem.Visibility = v;
				}

				catch (Exception ex)
				{
					LastException = ex;
				}

			}
		}

		/// <summary>
		/// Get ref to control when editor shown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ZoomSliderBarItem_ShownEditor(object sender, ItemClickEventArgs e)
		{
			if (e.Link != null && e.Link.Manager != null && e.Link.Manager.ActiveEditor is TrackBarControl)
			{ // get the active editor & add ValueChanged event to it
				ZoomSliderCtl = e.Link.Manager.ActiveEditor as TrackBarControl;
				ZoomSliderCtl.ValueChanged += new EventHandler(ZoomSlider_ValueChanged);
				ZoomSliderCtl.Properties.UseParentBackground = true; // needed for proper background display
			}
		}

		/// <summary>
		/// Update zoom value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ZoomSlider_ValueChanged(object sender, EventArgs e)
		{
			int ctlVal = ZoomSliderCtl.Value; // ranges 0 - 100
			int pct = SliderValToPctVal(ctlVal);
			ZoomPctBarItem.Caption = pct + "%";

			if (Qrc == null) return;
			ResultsViewProps view = Qrc.CurrentView;
			if (view != null) view.ScaleView(pct);

			return;
		}

		/// <summary>
		/// Getter/Setter for ZoomSlider percentage
		/// </summary>

		public int ZoomSliderPct
		{
			get
			{
				if (ZoomSliderBarItem == null ||
					ZoomPctBarItem == null) return 100;

				return SliderValToPctVal(ZoomSliderCtl.Value);
			}

			set
			{
				if (ZoomSliderBarItem == null ||
					ZoomPctBarItem == null) return;

				ZoomSliderBarItem.EditValue = PctValToSliderVal(value); // ranges 0 - 100
				ZoomPctBarItem.Caption = value + "%";
			}
		}

		static int SliderValToPctVal(int sliderVal)
		{
			int min, max;

			if (sliderVal <= 50) // 0 - 100 %
			{
				min = 10; // 10% zoom is minimum
				max = 100; // 100 % is maximum
			}
			else // 100 - 500%
			{
				sliderVal -= 50; // reduce to 0 - 50 range
				min = 100; // 100% is minimum
				max = 1000; // 1000% is maximum
			}

			int pctVal = (int)(min + (max - min) * (sliderVal / 50.0));
			return pctVal;
		}

		static int PctValToSliderVal(int pctVal)
		{
			int min, max;
			float pctRange = 0;

			if (pctVal <= 100) // 0 - 100 range
			{
				min = 0; // 0 is minimum value
				max = 50; // 50 is max value at midpoint
				pctRange = 100;
			}
			else // 100 - 1000%
			{
				pctVal -= 100; // reduce to 0 - 900 range
				min = 50; // midpoint
				max = 100; // max slider value
				pctRange = 900;
			}

			int sliderVal = (int)(min + (max - min) * (pctVal / pctRange));
			return sliderVal;
		}

	}
}
