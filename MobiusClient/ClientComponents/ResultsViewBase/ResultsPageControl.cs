using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010.Base;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraTab;
//using DevExpress.XtraCharts;

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
/// ResultsPageControl - Contains common page tools and a ResultsPagePanel with views, filter panel and DoD panel
/// </summary>

	public partial class ResultsPageControl : DevExpress.XtraEditors.XtraUserControl
	{
		bool InSetup = false;

		internal ResultsPage ResultsPage  // the results page associated with the page panel
		{
			get { return ResultsPagePanel.ResultsPage; }
			set { ResultsPagePanel.ResultsPage = value; }
		}

		internal ViewManager ActiveView // currently active view
		{
			get { return ResultsPage.ActiveView as ViewManager; }
		}

/// <summary>
/// Constructor
/// </summary>

		public ResultsPageControl()
		{
			InitializeComponent();

			ResultsPagePanel.ResultsPageControl = this; // link ResultsPagePanel up to us

			if (!SystemUtil.InDesignMode) // fill page control with page panel
			{
				Controls.Clear();

				//ResultsPagePanel.Dock = DockStyle.Fill;
				//Controls.Remove(Tools);
			}
		}

/// <summary>
/// Configure the results page control for the current page
/// </summary>
/// <param name="qrc"></param>
/// <param name="page"></param>
/// <returns></returns>

		static internal ResultsPageControl Configure (
			QueryResultsControl qrc, 
			ResultsPage page)
	{
			qrc.RemoveExistingControlsFromResultsPageControlContainer(); // properly dispose of any existing DevExpress controls

			ResultsPageControl rpc = new ResultsPageControl(); // page control with common tools and display area

			qrc.ResultsPageControl = rpc; // link the QueryResultsControl to rpc
			rpc.ResultsPage = page; // set the page that the rpc references

			PanelControl commonTools = rpc.Tools; // the array of common tools
			ResultsPagePanel rpp = rpc.ResultsPagePanel; // panel containing the display, filters and DoD panels
			page.ResultsPagePanel = rpp;

			//PanelControl rppc = qrc.ResultsPageControlContainer; // panel that contains the dock panels and rendering controls for the associated page views (1 per dock panel)
			//QueryResultsControl.DisposeOfChildMobiusControls(rppc); // properly dispose of any existing DevExpress controls
			//rppc.Controls.Clear(); // clear the view while building

			PanelControl rppc = qrc.ResultsPageControlContainer; // panel that contains the dock panels and rendering controls for the associated page views (1 per dock panel)
			rpp.Location = new Point(0, 0); // make the rpp the same size as rppc before adding it to avoid a resize
			rpp.Size = rppc.Size;
			rpp.Visible = false; // don't make visible until later
			rpp.Parent = rppc; // associate to parent before added to parent

			ViewsPanel viewsPanel = rpp.ViewsPanel; // Views DockPanels and their contained rendering controls go in here

			Query q = qrc.CrvQuery; // be sure we have a base QM

			if (q != null)
			{ // be sure the QueryManager is complete
				QueryManager qm = q.QueryManager as QueryManager;

				if (qm == null) // allocate query manager if needed
				{
					qm = new QueryManager();
					q.QueryManager = qm;
					qm.LinkMember(q);
				}

				qm.CompleteInitialization(OutputDest.WinForms); // be sure QueryManager is complete
				qm.LinkMember(qrc); // be sure qrc is linked to qm
			}

			////rpp.SuspendLayout();
			////rppc.Controls.Add(rpp); // add the results page panel to the page container panel (needed to look up control tree to get Qrc)

			if (page.ShowFilterPanel)
			{
				rpp.RenderFilterPanel();
				rpp.FiltersDockPanel.Visibility = DockVisibility.Visible;
			}
			else rpp.FiltersDockPanel.Visibility = DockVisibility.Hidden;

			if (page.ShowDetailsOnDemand)
			{
				rpp.RenderDetailsOnDemandPanel();
				rpp.DodDockPanel.Visibility = DockVisibility.Visible;
			}
			else rpp.DodDockPanel.Visibility = DockVisibility.Hidden;

			bool showBeforeLayout = (page.Views.Count == 1 && page.Views[0].ViewType == ResultsViewType.Spotfire);

			if (showBeforeLayout)
			{
				rpp.Visible = true;
				rppc.Controls.Add(rpp); // add the results page panel to the page container panel
				rpp.Dock = DockStyle.Fill;
				rpp.ViewsPanel.LayoutAndRenderViews(); // add the data views
			}

			else // layout before showing
			{
				rpp.ViewsPanel.LayoutAndRenderViews(); // add the data views
				rpp.Visible = true;
				rppc.Controls.Add(rpp); // add the results page panel to the page container panel
			}

				////rpp.ResumeLayout();

				rpp.ViewsPanel.FocusActiveView();

			return rpc;
		}

/// <summary>
/// Setup the tools for the current view
/// </summary>                         

		internal void SetupTools(PanelControl tools)
		{
			if (ResultsPage == null) return;

			InSetup = true;

			foreach (Control c in tools.Controls) // link any common tools from view to our handlers
			{
				if (Lex.Eq(c.Name, MarkDataBut.Name))
				{
					c.Click += new System.EventHandler(MarkDataBut_Click);
				}
			}

			InSetup = false;

			return;
		}

/// <summary>
/// Display mark-data menu
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		internal void MarkDataBut_Click(object sender, EventArgs e)
		{
			SimpleButton b = sender as SimpleButton;

			if (ActiveView == null || ActiveView.Qm == null || ActiveView.Qm.MoleculeGrid == null) return;

			ActiveView.Qm.MoleculeGrid.Helpers.ShowMarkedDataContextMenu(b,
				new System.Drawing.Point(0, b.Height), 
				MarkSelectedRowsMenuItem_Click,
				UnmarkSelectedRowsMenuItem_Click,
				SelectMarkedRowsMenuItem_Click,
				UnselectMarkedRowsMenuItem_Click);
		}

		internal void MarkSelectedRowsMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveView == null) return;

			ActiveView.Dtm.MarkSelected(true);
			return;
		}

		internal void UnmarkSelectedRowsMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveView == null) return;

			ActiveView.Dtm.MarkSelected(false);
			ResultsPagePanel.RefreshDetailsOnDemand();
			return;
		}

		internal void SelectMarkedRowsMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveView == null) return;

			ActiveView.Dtm.SelectMarked(true);
			ResultsPagePanel.RefreshDetailsOnDemand();
			return;
		}

		internal void UnselectMarkedRowsMenuItem_Click(object sender, EventArgs e)
		{
			if (ActiveView == null) return;

			ActiveView.Dtm.SelectMarked(false);
			ResultsPagePanel.RefreshDetailsOnDemand();
			return;
		}

		/// <summary>
		/// Dispose
		/// </summary>

		public new void Dispose()
		{
			base.Dispose();
			return;
		}

		private void ResultsPageControl_Resize(object sender, EventArgs e)
		{
			return;
		}

		private void ResultsPagePanel_Resize(object sender, EventArgs e)
		{
			return;
		}
	}
}
