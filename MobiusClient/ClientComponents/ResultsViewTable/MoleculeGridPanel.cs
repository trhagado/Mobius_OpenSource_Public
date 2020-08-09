using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
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
	/// This class contains two partially preformatted MoleculeGridControls.
	/// 1. BandedGridViewGrid is setup with a single BandedGridView for both
	///    display and printing.
	/// 2. LayoutViewGrid contains both a LayoutView for viewing and a 
	///    CardView for printing.
	/// </summary>

	public partial class MoleculeGridPanel : DevExpress.XtraEditors.XtraUserControl
	{
		bool Debug = false;

		internal ResultsPage ResultsPage;  // the results page associated with the page panel
		internal MoleculeGridPageControl PageControl; // the page that contains the panel
		internal MoleculeBandedGridViewManager View; // current MoleculeBandedGridView for the page panel
		internal MoleculeGridControl Grid; // currently selected grid

		public EventArgs Last_TopRowChangedEvent = null;

		public static int PaintCount = 0; // number of times paint called

		public MoleculeGridPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			if (SystemUtil.InDesignMode) return;

			BandedViewGrid.AddStandardEventHandlers();
			BandedViewGrid.GridPanel = this;

			LayoutViewGrid.AddStandardEventHandlers();
			LayoutViewGrid.GridPanel = this;

			//DevExpress.XtraEditors.Drawing.MouseWheelHelper.SmartMouseWheelProcessing = false; // so mouse wheel works without first clicking it (remove comment when get new version)

			return;
		}

		/// <summary>
		/// Link the view to the panel
		/// </summary>
		/// <param name="view"></param>

		internal void LinkViewToPanel(MoleculeBandedGridViewManager view)
		{
			View = view;
			return;
		}

		/// <summary>
		/// Allocate a new MoleculeGridPageControl
		/// and link into any associated QueryResultsContol
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="container"></param>
		/// <returns></returns>

		public static MoleculeGridPageControl AllocateNewMoleculeGridPageControl(
			QueryManager qm,
			Control container)
		{
			QueryResultsControl qrc = null;
			PopupGrid pug = null;
			MoleculeGridPageControl page;
			MoleculeGridPanel panel;
			MoleculeGridControl grid;

			Query q = qm.Query;
			ResultsFormat rf = qm.ResultsFormat;

			page = new MoleculeGridPageControl(); // Create a new, clean page, panel and grids

			panel = page.MoleculeGridPanel;
			grid = panel.SelectBaseGridViewGrid(qm);
			qm.LinkMember(grid); // link grid into qm
			grid.ShowCheckMarkCol = q.ShowGridCheckBoxes;

			if (container is QueryResultsControl && rf.NotPopupOutputFormContext) // normal query results
			{
				qrc = container as QueryResultsControl;
				qrc.RemoveExistingControlsFromResultsPageControlContainer(); // properly dispose of any existing DevExpress controls

				qrc.MoleculeGridPageControl = page; // link query results to this page
				qrc.ResultsPageControlContainer.Controls.Add(page);
				page.Dock = DockStyle.Fill;
			}

			return page;
		}

		/// <summary>
		/// Format grid and show the data
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="container">Either a QueryResultsControl or a PopupGrid</param>

		public static void ConfigureAndShow(
			QueryManager qm,
			Control container)
		{
			QueryResultsControl qrc = null;
			PopupGrid pug = null;
			MoleculeGridPageControl page;
			MoleculeGridPanel panel;
			MoleculeGridControl grid;

			Query q = qm.Query;
			ResultsFormat rf = qm.ResultsFormat;
			//qm.QueryResultsControl = null;

			page = new MoleculeGridPageControl(); // Create a new, clean page, panel and grids

			panel = page.MoleculeGridPanel;
			grid = panel.SelectBaseGridViewGrid(qm);
			qm.LinkMember(grid); // link grid into qm
			grid.ShowCheckMarkCol = q.ShowGridCheckBoxes;

			DataTableMx dt = qm.DataTable; // save ref to data table
			grid.DataSource = null; // clear source for header build
			qm.DataTable = dt; // restore data table
			grid.FormatGridHeaders(qm.ResultsFormat); // qm.MoleculeGrid.V.Columns.Count should be set for proper cols

			if (container is QueryResultsControl && rf.NotPopupOutputFormContext) // normal query results
			{
				qrc = container as QueryResultsControl;
				qrc.RemoveExistingControlsFromResultsPageControlContainer(); // properly dispose of any existing DevExpress controls

				qrc.MoleculeGridPageControl = page; // link query results to this page
				qrc.ResultsPageControlContainer.Controls.Add(page);
				page.Dock = DockStyle.Fill;
				//qm.QueryResultsControl = qrc; // link view set into query manager (used for filtering)

				if (q.Parent == null) // switch display to browse mode if root query
					QbUtil.SetMode(QueryMode.Browse, q);

				if (rf.Query.LogicType == QueryLogicType.And) // log grid query by logic type
					UsageDao.LogEvent("QueryGridAnd", "");
				else if (rf.Query.LogicType == QueryLogicType.Or)
					UsageDao.LogEvent("QueryGridOr", "");
				else if (rf.Query.LogicType == QueryLogicType.Complex)
					UsageDao.LogEvent("QueryGridComplex", "");
			}

			else if (container is PopupGrid || rf.PopupOutputFormContext) // popup results
			{
				if (container is PopupGrid)
					pug = container as PopupGrid;

				else // create a popup 
				{
					pug = new PopupGrid(qm);
					pug.Text = q.UserObject.Name;
				}

				if (pug.Controls.ContainsKey(panel.Name)) // remove any existing panel control
					pug.Controls.RemoveByKey(panel.Name);
				pug.Controls.Add(panel);
				pug.MoleculeGridPanel = panel; // restore direct link as well

				grid.ScaleView(q.ViewScale);
				UIMisc.PositionPopupForm(pug);
				pug.Text = q.UserObject.Name;
				pug.Show();
			}

			else throw new Exception("Invalid container type: " + container.GetType());

			// Set the DataSource to the real DataTable

			panel.SetDataSource(qm, dt);

			return;
		}

		/// <summary>
		/// Set the DataSource for the grid
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="dt"></param>

		public void SetDataSource(
			QueryManager qm,
			DataTableMx dt)
		{
			DataTableManager dtm = qm.DataTableManager;
			if (dtm != null)
			{ // be sure DataTable is ready for display
				//dtm.FiltersEnabled = false; // filters not enabled on entry
				//dtm.InitializeRowAttributes(true);
				//dtm.ApplyFilters();
				//dtm.UpdateFilterState();

				//if (QueryManager.StatusBarManager != null)
				//  QueryManager.StatusBarManager.DisplayFilterCountsAndString();
			}

			Grid.DataSource = dt; // set the datasource for the grid to the datatable
			Grid.ForceRefilter(); // (why?)
			Grid.Refresh();
			Grid.Focus(); // place focus on grid so page up/down & scroll keys work
		}

		/// <summary>
		/// Setup the proper grid, banded or layout, based on the ResultsFormat setting
		/// </summary>

		internal MoleculeGridControl SelectBaseGridViewGrid(QueryManager qm)
		{
			MoleculeGridControl grid;

			//MoleculeGridPanel mgp = new MoleculeGridPanel(); // start with clean grids (not working for some reason)
			//mgp.Controls.Remove(mgp.LayoutViewGrid);
			//mgp.Controls.Remove(mgp.BandedViewGrid);
			//LayoutViewGrid = mgp.LayoutViewGrid;
			//BandedViewGrid = mgp.BandedViewGrid;

			if (qm == null || qm.ResultsFormat == null ||
			 qm.ResultsFormat.UseBandedGridView) // select grid with desired view
				grid = BandedViewGrid;

			else grid = LayoutViewGrid; // show layout form

			Controls.Clear();

			if (grid == null) return null;

			Controls.Add(grid); // add the grid to the grid panel

			grid.Dock = DockStyle.Fill;
			grid.QueryManager = qm;
			Grid = grid;

			if (qm != null)
				qm.MoleculeGrid = grid; // link this grid into query manager
			else qm = qm; // debug;

			return grid;
		}

		private void BandedGridView_BandWidthChanged(object sender, DevExpress.XtraGrid.Views.BandedGrid.BandEventArgs e)
		{
			return; // prototype method
		}

		private void BandedGridView_DragObjectDrop(object sender, DevExpress.XtraGrid.Views.Base.DragObjectDropEventArgs e)
		{
			return; // prototype method
		}

		private void BandedGridView_DragObjectOver(object sender, DevExpress.XtraGrid.Views.Base.DragObjectOverEventArgs e)
		{
			return; // prototype method
		}

		private void BandedGridView_DragObjectStart(object sender, DevExpress.XtraGrid.Views.Base.DragObjectStartEventArgs e)
		{
			return; // prototype method
		}

		private void BandedGridView_HiddenEditor(object sender, EventArgs e)
		{
			if (Debug) ClientLog.Message("HiddenEditor");
			return;
		}

		private void BandedGridView_ShowingEditor(object sender, CancelEventArgs e)
		{
			if (Debug) ClientLog.Message("ShowingEditor");
			return;
		}

		private void BandedGridView_ShownEditor(object sender, EventArgs e)
		{
			if (Debug) ClientLog.Message("ShownEditor");
			return;
		}

		private void BandedGridView_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
		{
			if (Debug) ClientLog.Message("ValidatingEditor");
			return;
		}

		private void BandedGridView_GotFocus(object sender, EventArgs e)
		{
			if (Debug) ClientLog.Message("GotFocus");
			return;
		}

		private void BandedGridView_LostFocus(object sender, EventArgs e)
		{
			if (Debug) ClientLog.Message("LostFocus");
			return;
		}

		private void BandedGridView_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
		{
			if (Debug) ClientLog.Message("SelectionChanged");
			return;
		}

		private void BandedGridView_DataSourceChanged(object sender, EventArgs e)
		{
			if (Debug) ClientLog.Message("DataSourceChanged");
			return;
		}

		private void BandedGridView_FocusedColumnChanged(object sender, FocusedColumnChangedEventArgs e)
		{
			if (Debug) ClientLog.Message("FocusedColumnChanged");
			return;
		}

		private void BandedGridView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
		{
			if (Debug) ClientLog.Message("FocusedRowChanged");
			return;
		}

		/// <summary>
		/// Handle Mouse-wheel event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void BandedGridView_MouseWheel(object sender, MouseEventArgs e)
		{
			BandedGridView view = (sender as BandedGridView);
			try
			{
				int wheelDeltaFactor = 6;
				int minPixels = 18 + 3; // 1 normal row plus divider

				if (SS.I.ScrollGridByPixel)
				{
					int sign = e.Delta >= 0 ? -1 : 1; // note that the sign of e.Delta is opposite that of what is wanted for the pixels dx
					int pixels = -e.Delta / wheelDeltaFactor;
					if (Math.Abs(pixels) < minPixels)
						pixels = minPixels * sign;

					view.TopRowPixel += pixels;
				}

				else // Do single-line scrolling when using mouse wheel rather than the default multiline
				{
					if (e.Delta < 0) view.TopRowIndex++;
					else view.TopRowIndex--;
				}

				((DXMouseEventArgs)e).Handled = true;
			}
			catch (Exception ex) { ex = ex; }

			//throw new HideException(); // don't do normal DevExpress processing of this event
		}

		private void BandedViewGrid_Paint(object sender, PaintEventArgs e)
		{
			PaintCount++;
			return;
		}

		private void BandedViewGrid_VisibleChanged(object sender, EventArgs e)
		{
			return;
		}

		private void LayoutViewGrid_VisibleChanged(object sender, EventArgs e)
		{
			return;
		}

		private void BandedViewGrid_KeyPress(object sender, KeyPressEventArgs e)
		{
			//SystemUtil.Beep(); // debug
			return;
		}

		private void BandedGridView_KeyDown(object sender, KeyEventArgs e)
		{
			return;
		}

		private void BandedGridView_KeyPress(object sender, KeyPressEventArgs e)
		{
			return;
		}

		private void BandedGridView_KeyUp(object sender, KeyEventArgs e)
		{
			return;
		}

		private void BandedGridView_MouseUp(object sender, MouseEventArgs e)
		{
			//UIMisc.Beep();
			return;
		}

		private void BandedGridView_TopRowChanged(object sender, EventArgs e)
		{
			Last_TopRowChangedEvent = e;
			return;
		}

		/// <summary>
		/// Dispose
		/// </summary>

		public new void Dispose()
		{
			ResultsPage = null;
			PageControl = null;
			View = null;
			Grid = null;

			if (LayoutViewGrid != null) LayoutViewGrid.Dispose();

			if (BandedViewGrid != null) BandedViewGrid.Dispose();

			base.Dispose();

			return;
		}
	}
}
