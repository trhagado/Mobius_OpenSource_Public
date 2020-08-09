using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010.Base;
using DevExpress.XtraBars.Docking2010;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Panels containing the views for the current page
	/// </summary>

	public partial class ViewsPanel : DevExpress.XtraEditors.XtraUserControl
	{
		internal ResultsPagePanel ResultsPagePanel; // containing panel
		internal ResultsPage ResultsPage;  // the results page associated with the page panel

		internal static int RenderCount;
		internal static int PaintCount;

		/// <summary>
		/// Constructor
		/// </summary>

		public ViewsPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			return;
		}

		/// <summary>
		/// Layout and render the page using the current page layout
		/// </summary>

		internal void LayoutAndRenderViews()
		{
			ResultsPage page = ResultsPage;
			if (page == null) return;

			else if (page.TabbedLayout) // show in tabbed form
			{
				CreateStandardLayoutAndRenderViews(ViewLayout.Tabbed);
			}

			else if (page.Views.Count <= 1) // if one or fewer views then do simple layout
			{
				CreateStandardLayoutAndRenderViews(ViewLayout.SideBySide);
			}

			else if (Lex.IsNullOrEmpty(page.LayoutXml)) // if no layout defined default to rows and cols
			{
				CreateStandardLayoutAndRenderViews(ViewLayout.RowsAndCols);
			}

			else // Use the page LayoutXml to create the dock panels and then render the views into the panels
			{
				RestoreLayout();
				RenderViews();
			}

			RenderCount++;
			//DebugLog.Message("LayoutAndRenderViews: " + RenderCount);

			return;
		}

		/// <summary>
		/// Render the views in prebuilt DockPanel layout
		/// </summary>

		internal void RenderViews()
		{
			ResultsViewProps view = null;
			XtraPanel viewPanel; // panel that contains current view control and is contained in a docking panel or directly in the views panel if single view on page
			DockPanel activePanel = null;

			ResultsPage page = ResultsPage;
			if (page == null) return;

			List<ResultsViewProps> views = page.Views;
			if (views.Count == 0) // just have a single blank panel if no views
			{
				RenderEmptyPage();
				return;
			}

			// See if single view without DockPanels

			else if (ResultsPage.Views.Count == 1 && Controls.Count == 1 && Controls[0] is XtraPanel)
			{
				view = ResultsPage.Views[0];
				viewPanel = Controls[0] as XtraPanel;
				ConfigureViewInPanel(view, viewPanel);
				return;
			}

			// Scan the set of DockPanels and render the view associated with each panel

			else
			{
				DockManager dm = DockManager;
				foreach (DockPanel dp0 in dm.Panels)
				{
					view = dp0.Tag as ResultsViewProps;
					if (view == null) continue;

					Control.ControlCollection cc = dp0.ControlContainer.Controls;
					cc.Clear();
					viewPanel = new XtraPanel(); // the panel that will contain the view control
					viewPanel.Dock = DockStyle.Fill;
					viewPanel.BackColor = Color.White;
					cc.Add(viewPanel);
					ConfigureViewInPanel(view, viewPanel);

					SetupDockPanel(dp0, view, viewPanel);

					if (view == page.ActiveView)
						activePanel = dp0; // save ref to active panel
				}

				if (activePanel != null)
					DockManager.ActivePanel = activePanel;

				return;
			}

		}

		/// <summary>
		/// Render an empty page
		/// </summary>

		internal void RenderEmptyPage()
		{
			Controls.Clear();

			XtraPanel viewPanel = new XtraPanel(); // the panel that will contain the view control
			viewPanel.Dock = DockStyle.Fill;
			viewPanel.BackColor = Color.White;
			//viewPanel.BorderStyle = BorderStyle.FixedSingle;
			//viewPanel.Appearance.BorderColor = Color.CornflowerBlue;
			Controls.Add(viewPanel);
			return;
		}

		/// <summary>
		/// Layout and render the page in the specified layout format
		/// </summary>
		/// <param name="layout"></param>

		internal void CreateStandardLayoutAndRenderViews(
			ViewLayout layout)
		{
			XtraPanel viewPanel; // panel that contains current view control and is contained in a docking panel or directly in the views panel if single view on page

			ResultsPage page = ResultsPage;
			if (page == null) return;

			List<ResultsViewProps> views = page.Views;
			if (views.Count == 0) // just have a single blank panel if no views
			{
				RenderEmptyPage();
				return;
			}

			UIMisc.EnteringSetup();

			//Visible = false;
			SuspendLayout(); // suspend layout while building

			if (layout == ViewLayout.RowsAndCols)
				CreateRowsAndColsLayoutAndRenderViews();

			else // other common type view
				CreateCommonLayoutAndRenderViews(layout);

			DockPanel dp = DockManager.ActivePanel; // get the active dock panel associated with current view

			RenderViews();

			ResumeLayout();
			//Visible = true;

			SaveLayout();

			FocusActiveView();

			QueryResultsControl qrc = QueryResultsControl.GetQrcThatContainsControl(this);
			if (qrc != null) qrc.SetCurrentPageTabTitleAndImage(); // update the page tab


			UIMisc.LeavingSetup();

			return;
		}

		/// <summary>
		/// Focus the currently active view
		/// </summary>

		internal void FocusActiveView()
		{
			if (ResultsPage == null ||
			 ResultsPage.Views.Count == 0) // no need to focus if not multiple views
				return;

			else if (ResultsPage.Views.Count == 1) // single view
			{
				ResultsViewProps view = ResultsPage.Views[0];
				return; // todo?
			}

			else // multiple views set focus for active view and associated DockPanel
			{
				DockPanel dp = DockManager.ActivePanel;
				if (dp != null && dp.ControlContainer != null)
					dp.ControlContainer.Focus();
			}

			return;
		}

		/// <summary>
		/// Render views for stacked, side by side or tabbed
		/// </summary>

		void CreateCommonLayoutAndRenderViews(ViewLayout layout)
		{
			DockPanel pdp = null; // parent dock panel which is a vertical or horizontal split panel or a tab panel
			DockPanel dp = null; // current docking panel that contains the view panel & is stored in the ResultsDisplayPanel
			DockPanel lastDp = null;
			XtraPanel viewPanel; // panel that contains current view control and is contained in a docking panel or directly in the views panel if single view on page
			ResultsViewProps view;
			DockingStyle dockingStyle = DockingStyle.Top;

			if (layout == ViewLayout.SideBySide)
			{
				dockingStyle = DockingStyle.Top;
				ResultsPage.LastLayout = layout;
			}

			else if (layout == ViewLayout.Stacked)
			{
				dockingStyle = DockingStyle.Left;
				ResultsPage.LastLayout = layout;
			}

			else if (layout == ViewLayout.Tabbed)
			{
				dockingStyle = DockingStyle.Left;
				ResultsPage.TabbedLayout = true; // set flag indicating tabbed layout
			}

			ResultsPage page = ResultsPage;
			List<ResultsViewProps> views = page.Views;

			DockManager dm = DockManager;
			dm.Clear();

			DockPanel activePanel = null;

			for (int vi = 0; vi < views.Count; vi++)
			{
				view = views[vi];
				viewPanel = new XtraPanel(); // the panel that will contain the view control, goes into a DockPanel or directly into the viewsPanel if only one view
				viewPanel.Dock = DockStyle.Fill; // it will fill its containing panel

				if (views.Count == 1) // if just one view then let it use the full panel (no DockPanel with view header)
				{
					Controls.Add(viewPanel);
				}

				else // create a new DockPanel, add the viewPanel to it and
				{
					if (vi == 0) // first panel, add to DockManager
						dp = dm.AddPanel(dockingStyle);

					else if (vi == 1) // 2nd panel, add to first which creates splitter
						dp = lastDp.AddPanel();

					else // additional panels are just added to splitter
						dp = dm.RootPanels[0].AddPanel();

					lastDp = dp;

					SetupDockPanel(dp, view, viewPanel);

					if (page.ActiveViewIndex == vi)
						activePanel = dp;
				}

			} // view loop

			if (activePanel != null) // set the active panel
				dm.ActivePanel = activePanel;

			if (dm.RootPanels.Count > 0)
				dm.RootPanels[0].Size = Size;

			if (layout == ViewLayout.SideBySide)
			{
				foreach (DockPanel dp0 in dm.Panels)
					if (dp0.Count == 0)
						dp0.Height = Height;
				//dp0.Width = (int)(Width / (double)views.Count + .5);
			}

			else if (layout == ViewLayout.Stacked)
			{
				foreach (DockPanel dp0 in dm.Panels)
					if (dp0.Count == 0)
						dp0.Width = Width;
				//dp0.Height = (int)(Height / (double)views.Count + .5);
			}

			else if (layout == ViewLayout.Tabbed && dm.RootPanels.Count > 0)
				SetupTabbedPanel(dm.RootPanels[0]); // make it tabbed

			return;
		}

		/// <summary>
		/// Layout views in even rows and columns
		/// </summary>

		void CreateRowsAndColsLayoutAndRenderViews()
		{
			DockPanel dp, rowDp, lastRp = null, lastDp = null, activePanel = null;
			int vi, ri, ci;

			if (ResultsPage.Views.Count < 3) // if less than three views do a simple side by side
			{
				CreateCommonLayoutAndRenderViews(ViewLayout.SideBySide);
				return;
			}

			DockManager dm = DockManager;
			dm.Clear();
			Controls.Clear();

			int pc = dm.Panels.Count;
			int rpc = dm.RootPanels.Count;

			List<ResultsViewProps> views = ResultsPage.Views;
			if (views.Count == 0) return;

			double d = Math.Sqrt(views.Count);
			if ((int)d != d) d += 1;
			int cols = (int)d;
			if (cols < 1) cols = 1;
			int rows = (views.Count + (cols - 1)) / cols;
			if (rows < 1) rows = 1;

			int width = (int)(Width / (double)cols + .5);
			int height = (int)(Height / (double)rows + .5);

			DockPanel masterPanel = null;
			List<DockPanel> rowPanels = new List<DockPanel>();
			List<DockPanel> viewPanels = new List<DockPanel>();

			// Create master panel and one panel per row

			for (ri = 0; ri < rows; ri++)
			{
				if (ri == 0) // first row, create first panel
					dp = dm.AddPanel(DockingStyle.Left);

				else if (ri == 1) // 2nd row, add panel to first panel create row splitter
				{
					dp = rowPanels[0].AddPanel();
					masterPanel = dm.RootPanels[0];
					masterPanel.Size = Size;
					//masterPanel.DockVertical = DefaultBoolean.False; // (not needed)
				}

				else // subsequent row, add panel to row splitter
					dp = masterPanel.AddPanel();

				rowPanels.Add(dp);
			}

			// Add column/cell panel members to each row panel

			for (vi = 0; vi < views.Count; vi++)
			{
				ri = vi / cols;
				ci = vi % cols;

				DockPanel rp = rowPanels[ri];

				if (ci == 0) // first col, create first panel
					dp = rp; // get single panel

				else if (ci == 1) // 2nd col, add panel to first panel create splitter to contain cell panels
				{
					dp = rp.AddPanel();
					rp = rowPanels[ri] = dp.ParentPanel; // store new parent
																							 //rp.DockVertical = DefaultBoolean.True; // (not needed)
				}

				else // subsequent row, add panel to row splitter
					dp = rp.AddPanel();

				viewPanels.Add(dp);

				lastDp = dp;

				ResultsViewProps view = views[vi];
				XtraPanel viewPanel = new XtraPanel(); // the panel that will contain the view control, goes into a DockPanel or directly into the viewsPanel if only one view
				viewPanel.Dock = DockStyle.Fill; // it will fill its containing panel

				SetupDockPanel(dp, view, viewPanel);

				if (ResultsPage.ActiveViewIndex == vi)
					activePanel = dp;
			}

			if (activePanel != null) // set the active panel
				dm.ActivePanel = activePanel;

			ResultsPage.LastLayout = ViewLayout.RowsAndCols;

			return;
		}

		/// <summary>
		/// Configure the view in the view panel
		/// </summary>
		/// <param name="view"></param>
		/// <param name="viewPanel"></param>

		void ConfigureViewInPanel(
			ResultsViewProps view,
			XtraPanel viewPanel)
		{
			Stopwatch sw = Stopwatch.StartNew();

			long ms1 = sw.ElapsedMilliseconds;
			sw.Restart();

			if (viewPanel.Parent != null)
				viewPanel.Size = viewPanel.Parent.Size;

			//if (view.RenderingControl == null) // create the rendering control for the view if not done yet
			{
				view.AllocateRenderingControl(); // allocate rendering control if needed
			}

			long ms2 = sw.ElapsedMilliseconds;
			sw.Restart();

			view.InsertRenderingControlIntoDisplayPanel(viewPanel);

			if (view.ConfigureCount == 0) // configure if not done yet
			{
				view.ConfigureRenderingControl();
			}

			long ms3 = sw.ElapsedMilliseconds;
			return;
		}

		/// <summary>
		/// SetupDockPanel
		/// </summary>
		/// <param name="dp"></param>
		/// <param name="view"></param>

		void SetupDockPanel(
			DockPanel dp,
			ResultsViewProps view,
			XtraPanel viewPanel)
		{
			dp.Options.AllowFloating = false;
			dp.Options.FloatOnDblClick = false;
			dp.Options.ShowMaximizeButton = false;
			dp.Options.ShowAutoHideButton = false;
			dp.Options.ShowCloseButton = false;

			ButtonCollection chbs = dp.CustomHeaderButtons;
			chbs.Clear();

			CustomHeaderButton chb = new CustomHeaderButton // add close button
				("", null, 2, HorizontalImageLocation.Default, ButtonStyle.PushButton, "Close", false, -1, true, null, true, false, true, null, null, "Close", -1);
			chbs.Add(chb);

			chb = new CustomHeaderButton // add maximize button
				("", null, 0, HorizontalImageLocation.Default, ButtonStyle.PushButton, "Maximize", false, -1, true, null, true, false, true, null, null, "Maximize", -1);
			chbs.Add(chb);

			dp.CustomButtonClick += new ButtonEventHandler(ViewDockPanel_CustomButtonClick);
			dp.Enter += new EventHandler(ViewDockPanel_Enter); // when dock panel is selected
			dp.Leave += new EventHandler(ViewDockPanel_Leave); // when dock panel loses focus
			dp.VisibilityChanged += new VisibilityChangedEventHandler(ViewDockPanel_VisibilityChanged);
			dp.ClosedPanel += new DockPanelEventHandler(ViewDockPanel_ClosedPanel); // when dock panel is closed
			dp.MouseClick += new MouseEventHandler(ViewDockPanel_Click);

			dp.Tag = view; // link the dockpanel to the view

			dp.Text = view.Title; // set the view title

			int vi = view.GetViewIndex();

			if (dp.Text == "") dp.Text = "View " + (vi + 1);
			dp.Name = dp.Text;

			dp.ControlContainer.Controls.Add(viewPanel); // put the view panel in the dock panel

			return;
		}

		/// <summary>
		/// Custom DockPanel button clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewDockPanel_CustomButtonClick(object sender, ButtonEventArgs e)
		{
			DockPanel dp, pp;

			dp = sender as DockPanel;
			if (dp == null) return;

			CustomHeaderButton chb = e.Button as CustomHeaderButton;
			if (dp == null || chb == null) return;
			string buttonName = (chb.Tag != null) ? chb.Tag.ToString() : "";
			if (buttonName == "Maximize") // convert parent panel to tabbed form
			{
				if (dp.ParentPanel == null) return;
				CreateStandardLayoutAndRenderViews(ViewLayout.Tabbed);
			}

			else if (buttonName == "Close")
			{
				DockPanel dp2 = dp;
				while (dp2.HasChildren && dp2.ActiveChild != null) // dp may be a tab container, if so get active child 
					dp2 = dp2.ActiveChild;

				ResultsViewProps view = dp2.Tag as ResultsViewProps;
				if (view == null) return;


				CloseView(view);

				return;
			}

			else return; // shouldn't happen
		}

		/// <summary>
		/// Setup a panel with the tabbed view
		/// </summary>
		/// <param name="rpc"></param>

		void SetupTabbedPanel(
			DockPanel dp)
		{
			dp.Options.AllowFloating = false;
			dp.Options.FloatOnDblClick = false;
			dp.Options.ShowMaximizeButton = false;
			dp.Options.ShowAutoHideButton = false;
			dp.Options.ShowCloseButton = false;

			dp.Tabbed = true; // tabbed view

			ButtonCollection chbs = dp.CustomHeaderButtons;
			chbs.Clear();

			CustomHeaderButton chb = new CustomHeaderButton // add close button
				("", null, 2, HorizontalImageLocation.Default, ButtonStyle.PushButton, "Close", false, -1, true, null, true, false, true, null, null, "Close", -1);
			chbs.Add(chb);

			chb = new CustomHeaderButton // add restore button
				("", null, 1, HorizontalImageLocation.Default, ButtonStyle.PushButton, "Restore", false, -1, true, null, true, false, true, null, null, "Restore", -1);
			chbs.Add(chb);

			dp.CustomButtonClick += new ButtonEventHandler(TabDockPanel_CustomButtonClick);

			return;
		}

		/// <summary>
		/// Handle custom buttons on tabbed view panel 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void TabDockPanel_CustomButtonClick(object sender, ButtonEventArgs e)
		{
			DockPanel dp, pp;

			dp = sender as DockPanel;
			CustomHeaderButton chb = e.Button as CustomHeaderButton;
			if (dp == null || chb == null) return;
			string buttonName = (chb.Tag != null) ? chb.Tag.ToString() : "";
			if (buttonName == "Restore") // convert from tabbed form back to previous form
			{
				ResultsPage.TabbedLayout = false; // no longer in tabbed layout
				LayoutAndRenderViews(); // restore previous layout
				QueryResultsControl qrc = QueryResultsControl.GetQrcThatContainsControl(this);
				if (qrc != null) qrc.SetCurrentPageTabTitleAndImage(); // update the page tab
			}

			else if (buttonName == "Close")
			{
				ViewDockPanel_CustomButtonClick(dp, e);
			}

			else return;
		}

		private void SetDockPanelMaximizeButtonStyle(
			CustomHeaderButton chb,
			bool maximized)
		{
			if (maximized) // setup in maximized form
			{
				chb.ImageIndex = 1; // restore image index
				chb.Tag = "Restore";
				chb.ToolTip = "Restore";
			}
			else // setup in normal form
			{
				chb.ImageIndex = 0; // maximize image index
				chb.Tag = "Maximize";
				chb.ToolTip = "Maximize";
			}
		}

		/// <summary>
		/// ViewDockPanel_Enter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewDockPanel_Enter(object sender, EventArgs e)
		{
			DockPanel dp = sender as DockPanel;
			if (dp == null) return;
			ViewManager vm = dp.Tag as ViewManager;
			if (vm == null) return;

			DebugLog.Message("ViewDockPanel_Enter: " + dp.Text + "\r\nStack:\r\n" + new StackTrace(true));

			if (UIMisc.InSetup) return;

			ResultsPage.ActiveView = vm; // change the active view

			if (ResultsPage.ShowDetailsOnDemand) // show proper details for this view
				ResultsPagePanel.RenderDetailsOnDemandPanel();

			if (ResultsPage.ShowFilterPanel)
				ResultsPagePanel.RenderFilterPanel();

			vm.InsertToolsIntoDisplayPanel(); // get the correct set of tools

			return;
		}

		/// <summary>
		/// Focus is leaving dock panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewDockPanel_Leave(object sender, EventArgs e)
		{
			DockPanel dp = sender as DockPanel;
			if (dp == null) return;
			ViewManager vm = dp.Tag as ViewManager;
			if (vm == null) return;

			DebugLog.Message("ViewDockPanel_Leave: " + dp.Text + "\r\nStack:\r\n" + new StackTrace(true));
			return;
		}

		/// <summary>
		/// Visibility of dock panel changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void ViewDockPanel_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Close the specified view
		/// </summary>
		/// <param name="view"></param>

		internal void CloseView(ResultsViewProps view)
		{

			int vi = ResultsPage.Views.IndexOf(view);
			if (vi < 0) return;

			UIMisc.EnteringSetup();

			ResultsPage.Views.RemoveAt(vi); // get rid of the view

			DockPanel dp = GetContainingDockPanel(view.RenderingControl);
			if (dp != null) // if in a dock panel
				DockManager.RemovePanel(dp); // render the corresponding panel

			else RenderViews(); // just a single view, render empty page

			if (ResultsPage.ActiveViewIndex >= ResultsPage.Views.Count)
				ResultsPage.ActiveViewIndex--; // normally go to next view but keep index within range

			SaveLayout();

			UIMisc.LeavingSetup();

			return;
		}

		/// <summary>
		/// Get the DockPanel withing the page view, if any, that contains the specified control
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>

		internal DockPanel GetContainingDockPanel(Control ctl)
		{
			while (true)    // Try to scan up from rendering control
			{
				if (ctl == null || ctl is ResultsPageControl) return null;

				else if (ctl is DockPanel)
					return ctl as DockPanel;

				else ctl = ctl.Parent;
			}
		}

		/// <summary>
		/// DockPanel closed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewDockPanel_ClosedPanel(object sender, DockPanelEventArgs e)
		{
			DockPanel dp = sender as DockPanel;
			if (dp == null) return;
			ViewManager vm = dp.Tag as ViewManager;
			if (vm == null) return;

			ResultsPage.RemoveView(vm); // remove the view from the page

			return;
		}

		/// <summary>
		/// Click in header
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewDockPanel_Click(object sender, MouseEventArgs e)
		{
			return;
		}


		/// <summary>
		/// DockManager_ActiveChildChanged 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DockManager_ActiveChildChanged(object sender, DockPanelEventArgs e)
		{
			if (ResultsPagePanel == null || UIMisc.InSetup) return;

			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_ActiveChildChanged " + id);
		}

		/// <summary>
		/// Active panel changed for the DockManager
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DockManager_ActivePanelChanged(object sender, ActivePanelChangedEventArgs e)
		{
			if (ResultsPagePanel == null || UIMisc.InSetup) return;

			if (e.Panel != null)
			{
				//Application.DoEvents();
				e.Panel.Refresh(); // must redraw to get correct focus on header
													 //e.Panel.Focus();
			}

			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_ActivePanelChanged " + id);
		}

		private void DockManager_ClosingPanel(object sender, DockPanelCancelEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_ClosingPanel " + id);
		}

		private void DockManager_ClosedPanel(object sender, DockPanelEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_ClosedPanel " + id);
		}

		private void DockManager_EndDocking(object sender, EndDockingEventArgs e)
		{
			SaveLayout();

			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_EndDocking " + id);
		}

		private void DockManager_EndSizing(object sender, EndSizingEventArgs e)
		{
			SaveLayout();

			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_EndSizing " + id);
		}

		private void DockManager_RegisterDockPanel(object sender, DockPanelEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_RegisterDockPanel " + id);
		}

		private void DockManager_UnregisterDockPanel(object sender, DockPanelEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_UnregisterDockPanel " + id);
		}

		/// <summary>
		/// DockManager_TabbedChanged - Changed to tabbed/not tabbed state
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DockManager_TabbedChanged(object sender, DockPanelEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_TabbedChanged " + id);
		}

		/// <summary>
		/// DockManager_TabsPositionChanged - Current tab in a tabbed view has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DockManager_TabsPositionChanged(object sender, TabsPositionChangedEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_TabsPositionChanged " + id + ", " + e.OldTabsPosition + ", " + e.TabsPosition);
		}

		private void DockManager_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
		{
			string id = (e.Panel != null) ? e.Panel.Name : "null";
			LogDockManagerEvent("DockManager_VisibilityChanged " + id);
		}

		void LogDockManagerEvent(string msg)
		{
			//DebugLog.Message(msg);
			return;
		}

		/// <summary>
		/// Save layout to ResultsPage.LayoutXml
		/// </summary>

		internal void SaveLayout()
		{
			string tempFile = @"c:\download\Layout.xml";

			if (ResultsPage == null || ResultsPage.TabbedLayout) return;
			if (ResultsPage.Views.Count <= 1) // no layout if only one view
			{
				ResultsPage.LayoutXml = "";
				return;
			}

			//SystemUtil.Beep(); // debug

			CopyViewIdsToHints();

			MemoryStream ms = new MemoryStream();
			DockManager.SaveLayoutToStream(ms);
			string xml = Convert.ToBase64String(ms.ToArray());
			ResultsPage.LayoutXml = xml;

			//DockManager.SaveLayoutToXml(tempFile); // debug
			//xml = FileUtil.ReadFile(tempFile);
			//DockManager.SaveToXml(tempFile); // same length as SaveLayoutToXml
			//xml = FileUtil.ReadFile(tempFile);
			//int len = xml.Length; 

			ClearDisplayPanelHints(); // clear the hints

			return;
		}

		/// <summary>
		/// Restore layout from ResultsPage.LayoutXml
		/// </summary>

		internal void RestoreLayout()
		{
			//Controls.Clear(); // (needed?)

			if (ResultsPage == null ||
			 Lex.IsNullOrEmpty(ResultsPage.LayoutXml))
				throw new Exception("Layout not defined");

			DockManager dm = DockManager;

			MemoryStream ms = new MemoryStream(Convert.FromBase64String(ResultsPage.LayoutXml));
			dm.RestoreLayoutFromStream(ms);
			ms.Dispose();

			int rpc = dm.RootPanels.Count; // for debug
			int pc = dm.Panels.Count;

			RestoreViewsFromHints(); // restore view references

			if (dm.RootPanels.Count > 0) // size the first dock root panel to our size
			{
				DockPanel masterPanel = dm.RootPanels[0];
				masterPanel.Size = Size;
			}

			return;
		}

		/// <summary>
		/// Copy view ids to hints so they get serialized
		/// </summary>

		void CopyViewIdsToHints()
		{
			return; // noop
#if false
			foreach (DockPanel dp0 in DockManager.Panels)
			{
				ResultsViewProps view = dp0.Tag as ResultsViewProps;
				if (view != null)
					dp0.Hint = view.IdForPage.ToString();
				else dp0.Hint = "";
			}
			return;
#endif
		}

		/// <summary>
		/// ClearDisplayPanelHints
		/// </summary>

		void ClearDisplayPanelHints()
		{
			RestoreViewsFromHints(false, true);
		}

		/// <summary>
		/// Restore views from hints and clear hints
		/// </summary>

		void RestoreViewsFromHints()
		{
			RestoreViewsFromHints(true, true);
		}

		/// <summary>
		/// Use view ids stored in DockPanel hints to restore the associated view
		/// </summary>

		void RestoreViewsFromHints(
			bool setView,
			bool clearHint)
		{
			return; // noop
#if false
			int id;

			foreach (DockPanel dp0 in DockManager.Panels)
			{
				if (setView) dp0.Tag = null;

				string hint = dp0.Hint;
				if (clearHint) dp0.Hint = "";

				if (Lex.IsNullOrEmpty(hint)) continue;
				if (!int.TryParse(hint, out id)) continue;
				if (setView)
				{
					ResultsViewProps v = ResultsPage.GetViewFromIdForPage(id);
					if (v != null) dp0.Tag = v;
				}
			}

			return;
#endif
		}

		/// <summary>
		/// Don't show default dockmanager menu, substitute our custom menu instead
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DockManager_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
		{
			e.Cancel = true; // cancel default menu

			DockPanel dp = e.Control as DockPanel;
			if (dp == null) return;

			DockPanel dp2 = dp;
			while (dp2.Count > 0 && dp2.ActiveChild != null) // dp may be a tab container, if so get active child 
				dp2 = dp2.ActiveChild;

			ResultsViewProps view = dp2.Tag as ResultsViewProps;
			if (view == null) return;

			ContextMenuStrip menu = view.GetContextMenuStrip();
			if (menu == null) return;
			menu.Show(dp, e.Point);
		}

		private void ResultsPageViewsPanel_Paint(object sender, PaintEventArgs e)
		{
			PaintCount++;
			//DebugLog.Message("ResultsPageViewsPanel_Paint: " + PaintCount + "\r\nStack:\r\n" + new StackTrace(true).ToString());		}
		}

		/// <summary>
		/// Dispose
		/// </summary>

		public new void Dispose()
		{
			ResultsPagePanel = null;

			ResultsPage = null;

			base.Dispose();
			return;

		}
	}
}
