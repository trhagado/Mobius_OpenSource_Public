using Mobius.ComOps;
using Mobius.Data;

using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010.Base;
using DevExpress.XtraBars.Docking2010;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Results Page Panel containing one or more views and optionally the FilterPanel and DetailsPanel
	/// </summary>

	public partial class ResultsPagePanel : XtraUserControl, IResultsPagePanel
	{
		internal ResultsPageControl ResultsPageControl;  // ResultsPageControl that contains us

		internal ResultsPage ResultsPage  // the results page associated with the page panel
		{
			get { return ViewsPanel.ResultsPage; }
			set { ViewsPanel.ResultsPage = value; }
		}

		internal ViewManager ActiveView // currently active view
		{ get { return ResultsPage.ActiveView as ViewManager; } }

		internal QueryManager DodQm; // the QueryManager currently associated with the DoD panel

		internal static int FiltersDockPanelWidth = 200;
		internal static int DodGridDockPanelHeight = 200;

		internal static int RenderCount;
		internal static int PaintCount;

		/// <summary>
		/// Basic Constructor
		/// </summary>

		public ResultsPagePanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			ViewsPanel.ResultsPagePanel = this;

			if (!SystemUtil.InDesignMode)
			{
				//FiltersDockPanel.Visibility = DockVisibility.Hidden;
				//DetailsOnDemandDockPanel.Visibility = DockVisibility.Hidden;
				//Dock = DockStyle.Fill;
				//Visible = false;
			}

			return;
		}

		/// <summary>
		/// Select/deselect all rows in current view
		/// </summary>

		internal void SelectAll(bool selected)
		{
			if (ActiveView == null) return;

			ActiveView.Dtm.SelectAllRows(selected);
			RefreshDetailsOnDemand();
		}


		/// <summary>
		/// Configure and show the Details on Demand Panel for the currently active view
		/// </summary>

		internal void RenderDetailsOnDemandPanel()
		{
			MoleculeGridControl grid;

			if (ResultsPage == null) return;

			if (!ResultsPage.ShowDetailsOnDemand)
			{
				DodDockPanel.Visibility = DockVisibility.Hidden;
				return;
			}

			if (ActiveView == null || ActiveView.Qm == null || ActiveView.Qm.Query == null) return;
			QueryManager qm = ActiveView.Qm;
			Query q = qm.Query;

			if (DodQm != qm) // setup DoD panel for current QueryManager if not done yet
			{
				if (qm.ResultsFormat == null) // be sure we have 
					qm.CompleteInitialization();

				grid = DoDGridPanel.SelectBaseGridViewGrid(qm);

				qm.ResultsFormat.ShowCondFormatLabels = false; // temporarily hide any cf labels
				grid.ShowCheckMarkCol = true;
				grid.ShowSelectedRowsOnly = true; // show selected rows only

				DataTableMx dt = qm.DataTable; // save ref to data table
				if (dt == null) dt = dt; // debug
				grid.DataSource = null; // clear source for header build
				grid.FormatGridHeaders(qm.ResultsFormat);
				qm.DataTable = dt; // restore data table

				DodQm = qm;
			}

			else grid = qm.MoleculeGrid;

			DodDockPanel.Visibility = DockVisibility.Visible;
			DoDGridPanel.Visible = true;
			grid.DataSource = qm.DataTable;
			grid.ForceRefilter();
			grid.Refresh();
			Application.DoEvents();

			DisplayDodSelectedCount();
			return;
		}

		/// <summary>
		/// Selected rows have changed, refresh associated views and DoD panel
		/// </summary>

		internal void RefreshDetailsOnDemand()
		{
			///ChartControl.RefreshData(); // redraw the chart control (todo: refresh all relevant controls on page)

			if (ActiveView == null) return;
			if (DodQm == null) return;

			DodQm.MoleculeGrid.RefreshDataSource(); // redraw the grid control
			DodQm.DataTableManager.UpdateSelectedAndMarkedCounts();

			DisplayDodSelectedCount(); //also update selected count

			DodDockPanel.Visibility = DockVisibility.Visible;
			return;
		}


		/// <summary>
		/// Save the size and position of the views on the page
		/// </summary>

		internal void SaveViewSizesAndPositions()
		{
			return; // todo
		}

		/// <summary>
		/// Display the number of marked rows for current view
		/// </summary>

		internal void DisplayDodSelectedCount()
		{
			int cpdCount = 0, rowCount = 0;

			if (ActiveView != null && ActiveView.Qm != null && ActiveView.Qm.DataTableManager != null)
			{
				DataTableManager dtm = ActiveView.Qm.DataTableManager;
				cpdCount = dtm.SelectedKeyCount;
				if (cpdCount < 0) cpdCount = 0;

				rowCount = dtm.SelectedRowCount;
				if (rowCount < 0) rowCount = 0;
			}

			DodDockPanel.Text = "Selected Data - Compounds: " + StringMx.FormatIntegerWithCommas(cpdCount) +
				", Rows: " + StringMx.FormatIntegerWithCommas(rowCount);
			return;
		}

		/// <summary>
		/// Restore sizes of right and bottom subpanels
		/// </summary>

		internal void SetSubpanelSizes()
		{
			FiltersDockPanel.Width = FiltersDockPanelWidth;
			DodDockPanel.Height = DodGridDockPanelHeight;
		}

		/// <summary>
		/// Render the Filters and SelectedRows panels
		/// </summary>

		internal void RenderFilterAndDodPanels()
		{
			RenderFilterPanel();
			RenderDetailsOnDemandPanel();

			return;
		}

		/// <summary>
		/// SetFiltersPanelVisibility
		/// </summary>

		internal void RenderFilterPanel()
		{
			if (ResultsPage == null) return;

			if (ActiveView == null || ActiveView.Qm == null || ActiveView.Qm.Query == null) return;

			UIMisc.EnteringSetup();

			QueryManager qm = ActiveView.Qm;
			Query q = qm.Query;

			if (FilterPanel.Qm != qm) // setup filter panel for current QueryManager if not done yet
			{
				FilterPanel.Qm = qm;
			}

			if (ResultsPage.ShowFilterPanel)
			{
				FilterPanel.Clear();
				FiltersDockPanel.Visibility = DockVisibility.Visible;
				FilterPanel.Render();
			}

			else FiltersDockPanel.Visibility = DockVisibility.Hidden;

			UIMisc.LeavingSetup();
			return;
		}

		/// <summary>
		/// If Filters panel closed indicate so in the page view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FiltersDockPanel_ClosedPanel(object sender, DockPanelEventArgs e)
		{
			if (UIMisc.InSetup) return;

			ResultsPage.ShowFilterPanel = false;
		}

		/// <summary>
		/// If Dod panel closed indicate so in the page view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DetailsOnDemandDockPanel_ClosedPanel(object sender, DockPanelEventArgs e)
		{
			if (UIMisc.InSetup) return;

			ResultsPage.ShowDetailsOnDemand = false;
		}

		/// <summary>
		/// Views Panel resizing, keep contained control(s) filling the panel 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ViewsPanel_Resize(object sender, EventArgs e)
		{
			//DebugLog.Message("ViewsPanel_Resize , Size: " + this.Size);

			if (UIMisc.InSetup) return;

			if (ViewsPanel.Controls.Count != 1) return;

			if (ViewsPanel.Controls[0].Size != ViewsPanel.Size) // keep top-level contained control filling the panel 
				ViewsPanel.Controls[0].Size = ViewsPanel.Size; // (needed for DockPanel since Dock.Fill doesn't seem to always work)

			return;
		}


		/// <summary>
		/// Close the specified view
		/// </summary>
		/// <param name="view"></param>

		internal void CloseView(ResultsViewProps view)
		{
			ViewsPanel.CloseView(view);
		}

		/// <summary>
		/// Don't show default dockmanager menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void DockManager_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
		{
			e.Cancel = true;
		}

		private void ResultsPagePanel_Paint(object sender, PaintEventArgs e)
		{
			PaintCount++;
			//DebugLog.Message("ResultsPagePanel_Paint: " + PaintCount + "\nStack:\n" + new StackTrace(true).ToString());
		}

		/// <summary>
		/// Dispose
		/// </summary>

		public new void Dispose()
		{
			if (ResultsPageControl != null)
			{
				ResultsPageControl.ResultsPagePanel = null;
				ResultsPageControl = null;
			}

			if (DodQm != null)
			{
				DodQm.Dispose();
				DodQm = null;
			}

			base.Dispose();
			return;
		}
	}
}
