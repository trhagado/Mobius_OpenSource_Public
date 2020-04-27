using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents.Dialogs;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Docking;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Mobius.ClientComponents
{

/// <summary>
/// Collection of views onto the dataset along with the basic set of toolbar items for manipulating the views
/// </summary>

	public partial class QueryResultsControl : XtraUserControl
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public Query BaseQuery; // the base query associated with this Qrc (must be set by caller of the Qrc)

		public Query RootQuery // base query or query that base was derived from
			{ get { return (BaseQuery != null) ? BaseQuery.Root : null; } }
		public QueryManager RootQueryQm { get { return RootQuery == null ? null : RootQuery.QueryManager as QueryManager; } }

		public ResultsPageControl ResultsPageControl; // page control with common tools and display area for views
		public int CurrentPageIndex { get { return Tabs.SelectedTabPageIndex; } } // index of currently displayed page/view
		public ResultsPage CurrentResultsPage { get { return GetCurrentResultsPage(); } }
		public ResultsPagePanel CurrentResultsPagePanel { get { return GetCurrentResultsPagePanel(); } }

		public MoleculeGridPageControl MoleculeGridPageControl = new MoleculeGridPageControl(); // historic use, keep for backward compatibility

// Current view and associated Query elements

		public ViewManager CurrentView { get { return GetCurrentView(); } }
		public Query CrvQuery { get { return (CurrentView != null && CurrentView.BaseQuery != null) ? CurrentView.BaseQuery : BaseQuery; } }
		public QueryManager CrvQm { get { return CurrentView == null ? null : CurrentView.Qm; } }
		public DataTableMx CrvDataTable { get { return CrvQm == null ? null : CrvQm.DataTable; } }
		public DataTableManager CrvDtm { get { return CrvQm == null ? null : CrvQm.DataTableManager; } }
		public ResultsFormat CrvRf { get { return CrvQm == null ? null : CrvQm.ResultsFormat; } } // associated results format
		public ResultsFormatter CrvFormatter { get { return CrvQm == null ? null : CrvQm.ResultsFormatter; } } // associated formatter
		public MoleculeGridControl CrvMoleculeGrid { get { return CrvQm == null ? null : CrvQm.MoleculeGrid; } }

		// Other members

		TableList TableList; // dropdown list of tables from Results button

		Dictionary<string, int> BandCaptionDict; // associated band captions and caption index

		static public ResultsViewModel LastResultsViewItemSelected = null;

		static public bool ShowResultsViewTabs = true; // show or hide the full set of results views tabs

/// <summary>
/// Constructor
/// </summary>

		public QueryResultsControl()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			while (this.Tabs.TabPages.Count > 0) // remove existing tabs
				this.Tabs.TabPages.RemoveAt(0);

			//Tabs.Visible = ShowResultsViewTabs;

			//ViewTypeBitmaps16x16 = Bitmaps.ViewTypeBitmaps; // images to display on tabs (Note: images seem to need to be in this file to work)

			Tabs.Location = new Point(122, 2); // be sure in proper location
			Tabs.Size = new Size(374, 23); // and proper size
			Tabs.SendToBack();

			ToolPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

			return;
		}

/// <summary>
/// Gets the QueryManager for the current view for the QRC that contains the specified control
/// </summary>
/// <param name="ctl"></param>
/// <returns></returns>

		public static QueryManager GetCurrentViewQm(Control ctl)
		{
			QueryResultsControl qrc = GetQrcThatContainsControl(ctl);
			if (qrc != null) return qrc.CrvQm;
			else return null;
		}

		/// <summary>
		/// Find the first QueryResultsControl that contains the specified control
		/// </summary>
		/// <param name="ctl"></param>
		/// <returns></returns>

		public static QueryResultsControl GetQrcThatContainsControl(Control ctl)
		{
			Control prevCtl = null;

			while (true) 		// Try to scan up from rendering control
			{
				if (ctl == null) return null;

				else if (ctl is QueryResultsControl)
					return ctl as QueryResultsControl;

				else if (ctl is Form) // if at form level then use the base QRC for the session
					return SessionManager.Instance.QueriesControl.QueryResultsControl;

				else
				{
					prevCtl = ctl;
					ctl = ctl.Parent;
				}
			}

			//Form form = ctl.FindForm();
			//if (form == null) return null;
			//Control[] ctls = form.Controls.Find("QueryResultsControl", true);
			//if (ctls.Length > 0) return ctls[0] as QueryResultsControl;
			//else return null;
		}

		/// <summary>
		/// Get the current ResultsView
		/// </summary>

		public ViewManager GetCurrentView()
		{
			ResultsPage page = GetCurrentResultsPage();
			if (page == null || page.Views.Count <= 0) return null;

			ViewManager view = page.ActiveView as ViewManager;
			if (view != null) return view;

			else if (page.Views.Count > 0) // if no active view return the first view
				return page.Views[0] as ViewManager;

			else return null;
		}

		/// <summary>
		/// Get the current ResultsPagePanel
		/// </summary>
		/// <returns></returns>

		public ResultsPagePanel GetCurrentResultsPagePanel()
		{
			ResultsPage rp = GetCurrentResultsPage();
			if (rp != null) return rp.ResultsPagePanel as ResultsPagePanel;
			else return null;
		}

/// <summary>
/// Get the current ResultsPage
/// </summary>
/// <returns></returns>

		public ResultsPage GetCurrentResultsPage()
		{
			Query q = RootQuery;
			if (q == null) return null;
			int cpi = CurrentPageIndex;
			if (cpi < 0) return null;
			if (cpi >= q.ResultsPages.Pages.Count) return null; // shouldn't happen

			ResultsPage page = q.ResultsPages.Pages[cpi]; // all pages are associated with the root query
			return page;
		}

		/// <summary>
		/// Store new filter type for column updating filtering as needed
		/// </summary>
		/// <param name="colInfo"></param>
		/// <param name="ft"></param>

		internal void ChangeFilterType(ColumnInfo colInfo, FilterType ft)
		{
			QueryColumn qc = colInfo.Qc;

			if (qc.SecondaryFilterType != FilterType.Unknown &&
				qc.SecondaryFilterType != ft)
			{ // changing filter types, clear current criteria & update the filtering
				qc.SecondaryFilterType = FilterType.Unknown;
				qc.SecondaryCriteria = qc.SecondaryCriteriaDisplay = "";
				UpdateFiltering(colInfo);
			}

			qc.SecondaryFilterType = ft;

			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(qc); // sync any base query

			return;
		}

/// <summary>
/// Apply a filter to a column & then update the filters for each row and the view
/// </summary>
/// <param name="colInfo"></param>

		internal void UpdateFiltering(ColumnInfo colInfo)
		{
			CrvDtm.FiltersEnabled = true; // filters are now enabled

			if (ResultsPageControl != null && 	ResultsPageControl.ResultsPagePanel != null &&
			 ResultsPageControl.ResultsPagePanel.FilterPanel != null)
				ResultsPageControl.ResultsPagePanel.FilterPanel.FiltersEnabled = true; // also show in filter panel

			if (!CrvDtm.FiltersApplied) CrvDtm.ApplyFilters(); // apply existing filters if not done yet
			CrvDtm.ApplyFilter(colInfo);
			CrvDtm.UpdateFilterState();
			UpdateFilteredViews();

			if (CrvQm.StatusBarManager != null)
			{
				CrvQm.StatusBarManager.DisplayFilterCountsAndString();
				CrvQm.StatusBarManager.SetFiltersEnabledCtlValue(CrvDtm.FiltersEnabled);
			}

			return;
		}


/// <summary>
/// SetCurrentPageTabTitleAndImage
/// </summary>

		internal void SetCurrentPageTabTitleAndImage()
		{
			ResultsPages pages = BaseQuery.ResultsPages;
			if (pages == null || pages.Pages.Count < Tabs.SelectedTabPageIndex) return;
			ResultsPage page = pages[Tabs.SelectedTabPageIndex];
			SetCurrentTabTitle(page.ActiveTitle);
			SetCurrentTabImage(page.PageHeaderImage);
			return;
		}

		/// <summary>
/// Set text for current tab
/// </summary>
/// <param name="title"></param>

		internal void SetCurrentTabTitle(string title)
		{
			Tabs.TabPages[Tabs.SelectedTabPageIndex].Text = title;
			return;
		}

/// <summary>
/// Set image for current tab
/// </summary>
/// <param name="image"></param>

		internal void SetCurrentTabImage(Image image)
		{
			SetTabImage(Tabs.SelectedTabPage, image);
			return;
		}

		/// <summary>
		/// Set image for tab
		/// </summary>
		/// <param name="tabPageIndex"></param>
		/// <param name="idx"></param>

		internal void SetTabImage(
			int tabPageIndex,
			string viewTypeImageName)
		{
			if (tabPageIndex < 0 || tabPageIndex >= Tabs.TabPages.Count) return;

			SetTabImage(Tabs.TabPages[tabPageIndex], viewTypeImageName);
			return;
		}

		/// <summary>
		/// Set image for tab
		/// </summary>
		/// <param name="tabPage"></param>
		/// <param name="idx"></param>

		internal void SetTabImage(
			XtraTabPage tabPage,
			string viewTypeImageName)
		{
			Image image = Bitmaps.GetImageFromName(Bitmaps.I.ViewTypeImages, viewTypeImageName, true);

			SetTabImage(tabPage, image);
			return;
		}

		/// <summary>
		/// Set image for tab
		/// </summary>
		/// <param name="tabPage"></param>
		/// <param name="image"></param>

		internal void SetTabImage(
			XtraTabPage tabPage,
			Image image)
		{
			tabPage.ImageOptions.Image = image;
			return;
		}

		/// <summary>
		/// Select page to display
		/// </summary>
		/// <param name="ti"></param>

		internal void SelectPage(int ti)
		{
			XtraTabPageCollection tabs = Tabs.TabPages;
			if (tabs.Count == 0) return;

			int maxTab = tabs.Count - 1;
			XtraTabPage tp = tabs[maxTab]; // last tab
			string tabName = (tp.Tag != null) ? tp.Tag.ToString() : "";

			if (ti < 0) ti = 0;
			else if (ti > maxTab)
				ti = maxTab;

			int pageCount = RootQuery.ResultsPages.Pages.Count;

			if (ti >= pageCount) ti = pageCount - 1; // tried to go too far use last page
			if (ti < 0) return; // just return if invalid

			if (Tabs.SelectedTabPageIndex != ti)
				Tabs.SelectedTabPageIndex = ti;

			else // if already selected then must call change event directly
				Tabs_SelectedPageChanged(null, null);

			return;
		}

/// <summary>
/// Show or hide the AddNewView Tab (adds pages and views)
/// </summary>

		internal void ShowAddNewViewTab(bool show)
		{
			if (!SS.I.ShowAddNewViewTab) return;

			AddTabButton("AddNewTab", "Add View", "Add View", "Add a new data view for this result set.",	"Add", show);

			return;
		}

		XtraTabPage AddTabButton(
			string name, 
			string text,
			string ttHeader, 
			string ttText, 
			string imageName, 
			bool show)
		{
			XtraTabPage tp;
			XtraTabPageCollection tabs = Tabs.TabPages;

			int tpi = 0;
			while (tpi < tabs.Count) // always remove any existing item
			{
				tp = tabs[tpi];
				if (tp.Tag != null && Lex.Eq(tp.Tag.ToString(), name))
					tabs.RemoveAt(tpi);
				else tpi++;
			}

			if (!show) return null;

			tp = new XtraTabPage();
			SetTabImage(tp, imageName);
			tp.Text = text; 
			tp.Tag = name; // tab id
			SuperToolTip stt = new SuperToolTip();

			ToolTipTitleItem ttti = new DevExpress.Utils.ToolTipTitleItem();
			ttti.Text = ttHeader;

			ToolTipItem tti = new ToolTipItem();
			tti.LeftIndent = 6;
			tti.Text = ttText;
			stt.Items.Add(ttti);
			stt.Items.Add(tti);
			tp.SuperTip = stt;
			tabs.Add(tp);
			return tp;
		}

		/// <summary>
		/// Handle click of AddNewTab
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void Tabs_SelectedPageChanging(object sender, TabPageChangingEventArgs e)
		{
			if (SS.I.UISetupLevel >= 0 ||
				SessionManager.Instance == null) return;

			XtraTabPage tp = e.Page; // page we're goint to
			string tabName = (tp.Tag != null) ? tp.Tag.ToString() : "";
			if (Lex.Ne(tabName, "AddNewTab")) return;

			e.Cancel = true; // cancel the switch
			ShowAddViewContextMenu();

			return;
		}

		/// <summary>
		/// Show the AddView context menu
		/// </summary>

		void ShowAddViewContextMenu()
		{
			List<ResultsViewModel> modelViews = ViewManager.GetResultsViewModels();

			ContextMenuStrip menu = AddViewContextMenu;
			menu.Items.Clear();

			foreach (ResultsViewModel view in modelViews)
			{
				if (!view.ShowInViewsMenu) continue;

				if (Lex.StartsWith(view.Name, "Separator"))
				{
					menu.Items.Add(new ToolStripSeparator());
					continue;
				}

				ToolStripMenuItem item = new ToolStripMenuItem();

				ResultsViewModel rvm = view.Clone(); // get a clone that we can modify
				item.Tag = rvm;
				
				item.Text = view.Title;

				string imageName = view.CustomViewTypeImageName;
				if (!Lex.IsDefined(imageName))
					imageName = "Spotfire";

				item.Image  = Bitmaps.GetImageFromName(Bitmaps.I.ViewTypeImages, imageName);

				rvm.Query = this.BaseQuery;
				rvm.QueryResultsControl = this;

				item.Click += AddViewMenuItem_Click;

				menu.Items.Add(item);
			}

			int x = Cursor.Position.X;
			int y = Tabs.PointToScreen(Tabs.Location).Y + 22; // +Tabs.DisplayRectangle.Top;
			Point p = new Point(x, y);
			AddViewContextMenu.Show(p);

			return;
		}

/// <summary>
/// Switching to new results page
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Tabs_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
		{
			//DebugLog.Message("Tabs_SelectedPageChanged to: " + Tabs.SelectedTabPageIndex);

			if (UIMisc.InSetup ||
				SessionManager.Instance == null) return;

			Query q = BaseQuery; // was RootQuery but QM may not be defined for this
			if (q == null) return;

			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null) return;

			int pi = Tabs.SelectedTabPageIndex;
			List<ResultsPage> pages = q.ResultsPages.Pages;

			if (pi < 0 || pi >= pages.Count) return; // just return if invalid page
			ResultsPage page = pages[pi];

			bool tableView = // set flag for simple table view for this page
			 (page.Views.Count == 1 &&
				page.Views[0].ViewType == ResultsViewType.Table);

			bool spotfireView = // set flag for simple Spotfire view for this page
			 (page.Views.Count == 1 &&
				page.Views[0].ViewType == ResultsViewType.Spotfire);

			bool pageNeedsAllData = !(tableView || spotfireView);

			pageNeedsAllData = Math.Abs(1) == 2; // debug

			if (pageNeedsAllData) // if page needs all data to render read it in here
			{
				DataTableManager dtm = qm.DataTableManager;
				DialogResult dr = dtm.CompleteRetrieval(); // be sure we have all data
				if (dr == DialogResult.Cancel) return;
			}

			ConfigureResultsPage(pi); // render the page

			return;
		}

/// <summary>
/// Tabs_MouseUp - Handle mouse right-click operations
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Tabs_MouseUp(object sender, MouseEventArgs e)
		{
			Point mousePosition = Control.MousePosition;
			if (e.Button == MouseButtons.Right)
			{
				XtraTabHitInfo hitInfo = Tabs.CalcHitInfo(new Point(e.X, e.Y));
				if (hitInfo.HitTest == XtraTabHitTest.PageHeader)
				{
					XtraTabPage tp = hitInfo.Page;
					string tabName = (tp.Tag != null) ? tp.Tag.ToString() : "";

					if (tabName == "AddNewTab") return;

					else // show the ResultsPage menu
					{
						int x = Cursor.Position.X;
						int y = Cursor.Position.Y;
						Point p = new Point(x, y);
						p = this.PointToClient(p);
						PageContextMenu.Show(this, p);
					}
				}
			}
		}

/// <summary>
/// Set the tool bar tools in the upper right corner of the QueryResultsControl
/// </summary>
/// <param name="tools"></param>
/// <param name="zoomPct">Percent to show on zoom control, hide control if negative</param>

		internal void SetToolBarTools(
			PanelControl tools,
			int zoomPct)
		{
			ToolPanel.Controls.Clear();

			if (tools != null)
			{
				ResultsPageControl rpc = ResultsPageControl;
				if (rpc == null) throw new Exception("ResultsPageControl not defined");

				int width = tools.Width;
				ToolPanel.Width = width;
				ToolPanel.Left = Width - width;

				rpc.SetupTools(tools);

				ToolPanel.Controls.Add(tools); // add tools for the view
				tools.Location = new Point(0, 0); // position in upper left corner

				Tabs.Width = (ToolPanel.Left - 6) - Tabs.Left;
			}

			SetZoomSliderPct(zoomPct);

			//Application.DoEvents(); // update the UI
			return;
		}

/// <summary>
/// Set the percentage value on the zoom slider in the tool bar
/// </summary>
/// <param name="zoomPct"></param>

		internal void SetZoomSliderPct (int zoomPct)
		{
			StatusBarManager sbm = SessionManager.Instance.StatusBarManager;
			if (zoomPct >= 0)
			{
				sbm.ZoomControlVisible = true;
				sbm.ZoomSliderPct = zoomPct;
			}

			else sbm.ZoomControlVisible = false;
		}

/// <summary>
/// Build the set of ResultsPages tabs for the query 
/// </summary>
/// <param name="q"></param>
/// 
		public void BuildResultsPagesTabs(Query q)
		{
			ResultsPages pages;
			ResultsPage page;
			ResultsViewProps view;
			XtraTabPage ctp; // tab page containing chart panel

			SS.I.UISetupLevel++;

			BaseQuery = q; // save reference to the query

			Tabs.TabPages.Clear(); // clear the tab list
			ResultsPageControlContainer.Controls.Clear(); // clear the display panel
			//RemoveExistingResultsControls(); // (don't) dispose of any existing DevExpress controls

			pages = q.Root.ResultsPages; // get any view pages for query
			if (pages != null && pages.Pages != null && // add a tab for each view page
				pages.Pages.Count > 0 && pages.Pages[0].Views != null)
			{
				for (int ci = 0; ci < pages.Pages.Count; ci++)
				{
					page = pages.Pages[ci];
					if (String.IsNullOrEmpty(page.Title))
						page.SetDefaultTitle(pages);

					ctp = Tabs.TabPages.Add(page.ActiveTitle);
					SetTabImage(ctp, page.PageHeaderImage);
				}

			}

			ShowAddNewViewTab(true);
			SS.I.UISetupLevel--;

			return;
		}

/// <summary>
/// Add a new tab page & display the specified view in it
/// </summary>
/// <param name="cp"></param>

		internal void AddResultsPageTabPage(ResultsPage page)
		{
			SS.I.UISetupLevel++;

			XtraTabControl tc = Tabs;

			Tabs.SelectedTabPageIndex = (Tabs.TabPages.Count - 1) - 1; // backup to tab beforew added button
			ShowAddNewViewTab(false); // remove added menu button tab

			XtraTabPage tp = tc.TabPages.Add(page.ActiveTitle);
			SetTabImage(tp, page.PageHeaderImage);

			//RenderResultsPage(page); // (not needed, done when tab changed)

			ShowAddNewViewTab(true); // add the menu button tab back in
			SS.I.UISetupLevel--; // say not in setup so tab change below causes render

			try { Tabs.SelectedTabPageIndex = (Tabs.TabPages.Count - 1) - 1; } // select new tab page and render(occasionally fails because of lower-level error)
			catch (Exception ex) { ex = ex; }

			return;
		}

/// <summary>
/// Configure the results page, creating first if necessary
/// </summary>
/// <param name="ti"></param>

		internal void ConfigureResultsPage(int pi)
		{
			Query q = RootQuery;
			if (q == null) return;
			if (q.PresearchDerivedQuery != null) // get any derived browse query
				q = q.PresearchDerivedQuery;

			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null) return;

			if (pi < 0) return;

			SS.I.UISetupLevel++;
			ResultsPages pages = RootQuery.ResultsPages; // get any additional pages for query
			ResultsPage page = pages[pi];
			SessionManager.Instance.StatusBarManager.ZoomControlVisible = false; // hide zoom controls initially
			ConfigureResultsPage(page);
			Tabs.ClosePageButtonShowMode = ClosePageButtonShowMode.InActiveTabPageHeader; // show close button in active tab

			q.InitialBrowsePage = pi; // set current tab page
			SS.I.UISetupLevel--;

			return;
		}

/// <summary>
/// Configure and display the results page, creating first if necessary
/// </summary>
/// <param name="page"></param>

		internal void ConfigureResultsPage(ResultsPage page)
		{
			ResultsPageControl = ResultsPageControl.Configure(this, page);
			return;
		}

/// <summary>
/// Reset all filters to retrieve all data
/// </summary>

		internal void ResetFilters()
		{
			CrvMoleculeGrid.ClearFilters(); // reset the filters via the grid view
			UpdateFilteredViews();
			RefreshFilterPanel();
		}

/// <summary>
/// Enable or disable filters for current views
/// </summary>
/// <param name="enable"></param>

		internal void EnableFilters(bool enable)
		{
			if (CrvQm != null && CrvQm.DataTableManager != null)
				CrvQm.DataTableManager.FiltersEnabled = enable;

			////if (MoleculeGrid.V.ActiveFilterEnabled == enable) // if already the same value then switch to force refilter
			////  MoleculeGrid.V.ActiveFilterEnabled = !enable;
			////MoleculeGrid.V.ActiveFilterEnabled = enable; // set filter enablement for grid view

			UpdateFilteredViews();
			RefreshFilterPanel();
			return;
		}

/// <summary>
/// Clear filters
/// </summary>

		internal void ClearFilters()
		{
			if (CrvMoleculeGrid != null)  // clear the filters from the grid view
				CrvMoleculeGrid.ClearFilters();
			UpdateFilteredViews();
			RefreshFilterPanel();
		}

/// <summary>
/// New tab page added
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Tabs_ControlAdded(object sender, ControlEventArgs e)
		{
			return;
		}

	/// <summary>
	/// Adding control to display panel
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>

		private void ResultsDisplayPanel_ControlAdded(object sender, ControlEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Update any filtered views that are visible (max of 1 for now)
		/// </summary>

		public void UpdateFilteredViews()
		{
			ResultsPages pages;

			ResultsPage currentPage = null;
			ViewManager view = CurrentView;
			if (view != null) currentPage = view.ResultsPage;

			Query rq = RootQuery; // all pages are associated with root query
			if (rq == null) return;

			for (int pi = 0; pi < rq.ResultsPages.Pages.Count; pi++ )
			{
				ResultsPage page = rq.ResultsPages.Pages[pi];
				for (int vi = 0; vi < page.Views.Count; vi++)
				{
					view = page.Views[vi] as ViewManager;
					if (view == null) continue;

					if (page == currentPage) // if view on current then do a relatively quick filtered update
						view.UpdateFilteredView();
					else view.ConfigureCount = 0; // if view on other page mark as not rendered (note: this will lose any zoom & panning)
				}
			}
		}

/// <summary>
/// Redraw the filter panel
/// </summary>

		public void RefreshFilterPanel()
		{
			if (ResultsPageControl == null) return;
			ResultsPagePanel rpp = ResultsPageControl.ResultsPagePanel;
			if (rpp == null) return;
			rpp.RenderFilterPanel();
			return;
		}

		private void ResultsLabel_Click(object sender, EventArgs e)
		{
			//Point p = ResultsLabel.PointToScreen(new Point(0, 0));
			//AddChartViewMenu.ShowPopup(p);
		}

		/// <summary>
		/// Show the page context menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void WindowLayoutButton_Click(object sender, EventArgs e)
		{
			SimpleButton b = sender as SimpleButton;
			if (b == null) return;

			PageContextMenu.Show(b,
				new System.Drawing.Point(0, b.Height));
		}

		/// <summary>
		/// Setup layout menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void PageContextMenu_Opening(object sender, CancelEventArgs e)
		{
			if (CurrentResultsPage == null) return;

			ShowFilterPanelMenuItem.Checked = CurrentResultsPage.ShowFilterPanel;
			ShowSelectedDataRowsPanelMenuItem.Checked = CurrentResultsPage.ShowDetailsOnDemand;
		}

/// <summary>
/// Add new page
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void NewPageMenuItem_Click(object sender, EventArgs e)
		{
			ResultsPage page = ResultsPages.AddNewPage(BaseQuery);

			AddResultsPageTabPage(page); // add tab for page
			ConfigureResultsPage(page); // render the empty page
			return;
		}

		/// <summary>
		/// Show the add Page/View menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void NewViewMenuItem_Click(object sender, EventArgs e)
		{
			ShowAddViewContextMenu();
			return;
		}

		private void ArrangeStackedMenuItem_Click(object sender, EventArgs e)
		{
			CurrentResultsPagePanel.ViewsPanel.CreateStandardLayoutAndRenderViews(ViewLayout.Stacked);
			return;
		}

		private void ArrangeSideBySideMenuItem_Click(object sender, EventArgs e)
		{
			CurrentResultsPagePanel.ViewsPanel.CreateStandardLayoutAndRenderViews(ViewLayout.SideBySide);
			return;
		}

		private void ArrangeEvenlyMenuItem_Click(object sender, EventArgs e)
		{
			CurrentResultsPagePanel.ViewsPanel.CreateStandardLayoutAndRenderViews(ViewLayout.RowsAndCols);
			return;
		}

		private void ArrangeTabbedMenuItem_Click(object sender, EventArgs e)
		{
			CurrentResultsPagePanel.ViewsPanel.CreateStandardLayoutAndRenderViews(ViewLayout.Tabbed);
			return;
		}

		internal void ShowFilterPanelMenuItem_Click(object sender, EventArgs e)
		{
			CurrentResultsPage.ShowFilterPanel = !CurrentResultsPage.ShowFilterPanel;
			CurrentResultsPagePanel.RenderFilterPanel();
			return;
		}

		internal void ShowSelectedDataRowsPanelMenuItem_Click(object sender, EventArgs e)
		{
			CurrentResultsPage.ShowDetailsOnDemand = !CurrentResultsPage.ShowDetailsOnDemand;
			CurrentResultsPagePanel.RenderDetailsOnDemandPanel();
			return;
		}

		/// <summary>
		/// Rename a tab page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RenameMenuItem_Click(object sender, EventArgs e)
		{
			ResultsPage page = GetCurrentResultsPage();

			List<string> imageNameList = new List<string>(); // put list of image names to use in dropdown
			var l = Bitmaps.I.ViewTypeImages.Images.Keys;

			foreach (string s in l)
				imageNameList.Add(s);

			string newName = InputBoxMx.Show("Enter the new name for " + page.ActiveTitle,
				"Rename", page.ActiveTitle, imageNameList, -1, -1);

			if (Lex.IsNullOrEmpty(newName)) return;

			bool isImageName = imageNameList.Contains(newName);

			if (!isImageName) // set title
			{
				page.Title = newName;
				if (page.Views.Count == 1) // if only one view rename it as well
					page.Views[0].Title = newName;

				SetCurrentTabTitle(newName);
			}

			else // set custom image name to use in tab
			{
				if (page.Views.Count == 1) 
					page.Views[0].CustomViewTypeImageName = newName;

				SetCurrentPageTabTitleAndImage();
			}

			return;
		}

		/// <summary>
		/// Duplicate page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DuplicatePageMenuItem_Click(object sender, EventArgs e)
		{
			return; // todo...
		}

		/// <summary>
		/// Delete page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DeletePageMenuItem_Click(object sender, EventArgs e)
		{
			DeleteCurrentResultsPage();
			return;
		}

		private void Tabs_CloseButtonClick(object sender, EventArgs e)
		{
			DeleteCurrentResultsPage();
			return;
		}

		private void DeleteCurrentResultsPage()
		{
			ResultsPage page = GetCurrentResultsPage();

			if (page != null && page.Views.Count > 0)
			{
				if (MessageBoxMx.Show("Are you sure you want to delete the " + page.ActiveTitle + " view?", "Delete View",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes) return;
			}

			int qti = Tabs.SelectedTabPageIndex;
			RemoveTabAndPage(qti);
			return;
		}

		/// <summary>
		/// Remove specified tab and page
		/// </summary>
		/// <param name="qti"></param>

		public void RemoveTabAndPage(int qti)
		{
			int newTab = qti - 1; // carefully remove the tab page
			if (newTab < 0) newTab = qti + 1;
			Tabs.SelectedTabPageIndex = newTab; // move to another tab
			Tabs.TabPages.RemoveAt(qti); // remove this one

			ResultsPages pages = RootQuery.ResultsPages;
			pages.RemoveAt(qti); // remove the page from the query (must do 2nd)

			return;
		}

		/// <summary>
		/// RemoveExistingControlsFromResultsPageControlContainer
		/// </summary>

		public void RemoveExistingControlsFromResultsPageControlContainer()
		{
			if (WindowsHelper.ControlContainsChildControlOfSpecifiedType(ResultsPageControlContainer, typeof(Mobius.SpotfireClient.SpotfirePanel)))
			{ // if SpotfirePanel avoid disposing of controls that contain the currently open Visualization
				ResultsPageControlContainer.Controls.Clear();
				return;
			}

			// Properly dispose of any existing DevExpress controls that can cause odd exceptions if not disposed

			int dispCnt = DisposeOfChildMobiusControls(ResultsPageControlContainer); 
			return;
		}

		/// <summary>
		/// Dispose (and remove) all the children of a control
		/// </summary>

		public static int DisposeOfChildMobiusControls(
			Control c,
			int depth = 0)
		{
			int dispCnt = 0;

			if (c == null || c.Controls == null) return 0;

			depth++;

			while (c.Controls.Count > 0)
			{
				Control child = c.Controls[0];
				dispCnt += DisposeOfMobiusControl(child, depth);
			}

			depth--;
			return dispCnt;
		}

		/// <summary>
		/// Dispose a control and all its children with special processing for Mobius user controls
		/// to properly call "new" (i.e. "public new void Dispose()") methods.
		/// Note that the "protected override void Dispose(bool disposing)" method is already defined in
		/// the xxx.Designer.cs file so can't be defined here.
		/// </summary>

		public static int DisposeOfMobiusControl(
			Control c,
			int depth = 0)
		{
			if (c == null) return 0;

			if (ClientState.IsDeveloper && DebugMx.False)
				DebugLog.Message(new string(' ', depth) + "Disposing of " + depth + " " + c.GetType() + " " + c.Name);

			int dispCnt = DisposeOfChildMobiusControls(c, depth);

			c.Parent = null; // remove from parent first

			if (c is ResultsPageControl) // must cast to proper class to get the "new" Dispose associated with the class
				((ResultsPageControl)c).Dispose();

			else if (c is ResultsPagePanel) 
				((ResultsPagePanel)c).Dispose();

			else if (c is ViewsPanel)
				((ViewsPanel)c).Dispose();

			else if (c is MoleculeGridControl)
				((MoleculeGridControl)c).Dispose();

			else if (c is MoleculeGridHelpers)
				((MoleculeGridHelpers)c).Dispose();

			else if (c is MoleculeGridPanel)
				((MoleculeGridPanel)c).Dispose();

			else c.Dispose();

			dispCnt++;
			return dispCnt;
		}

		private void AddViewMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			LastResultsViewItemSelected = item.Tag as ResultsViewModel;
			AddViewHelper.AddTentativeNewViewDelayed(LastResultsViewItemSelected);

			return;
		}

	}

}
