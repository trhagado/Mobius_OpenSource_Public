using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Mobius.ClientComponents
{

/// <summary>
/// Molecule grid view
/// </summary>

	public class MoleculeBandedGridViewManager : ViewManager
	{
		internal MoleculeGridPageControl GridPageControl; 
		internal MoleculeGridPanel GridPanel { get { return GridPageControl != null ? GridPageControl.MoleculeGridPanel : null; } }
		internal MoleculeGridControl GridControl { get { return GridPanel != null ? GridPanel.Grid : null; } }

		/// <summary>
		/// Basic constructor
		/// </summary>

		public MoleculeBandedGridViewManager()
		{
			ViewType = ResultsViewType.Table;
			Title = "Table View";
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			GridPageControl = new MoleculeGridPageControl();
			GridPanel.View = this; // link the grid control panel to this view
			GridPanel.BandedViewGrid.Visible = false; // initially hidden
			GridPanel.LayoutViewGrid.Visible = false; // initially hidden
			GridPanel.Dock = DockStyle.Fill; // dock full grid panel

			RenderingControl = GridPanel;
			ConfigureCount = 0;
			return RenderingControl;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel viewPanel)
		{
			viewPanel.Controls.Clear(); // remove anything in there
			viewPanel.Controls.Add(GridPanel); // add it to the display panel

			InsertToolsIntoDisplayPanel();

			GridPageControl.EditQueryButton.Enabled =
				ViewManager.IsControlContainedInQueriesControl(viewPanel) ||
				ViewManager.IsCustomExitingQueryResultsCallbackDefined(viewPanel);

			if (Qrc == null || BaseQuery == null) return;
			Qrc.SetToolBarTools(GridPageControl.ToolPanel, BaseQuery.ViewScale); // show the proper tools and zoom
			return;
		}

		/// <summary>
		/// Insert tools into display panel
		/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			if (!ViewManager.IsControlContainedInQueriesControl(Qrc)) // adjust tools if we're not contained in a QueriesControl
			{
				//GridPageControl.EditQueryButton.Visible = false;
				GridPageControl.FormattingButton.Visible = false;
				GridPageControl.AdjustToolPanelButtonSpacing();
			}

			return;
		}

		/// <summary>
		/// Render the view by configuring the control for the current view settings
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			if (Qm == null) return;

			MoleculeGridControl grid = GridPanel.SelectBaseGridViewGrid(Qm);
			Qm.LinkMember(grid); // link grid into qm

			DataTableMx dt = Qm.DataTable; // save ref to data table
			grid.DataSource = null; // clear source for header build
			Qm.DataTable = dt; // restore data table in qm

			grid.ShowCheckMarkCol = BaseQuery.ShowGridCheckBoxes;
			grid.FormatGridHeaders(Qm.ResultsFormat); // qm.MoleculeGrid.V.Columns.Count should be set for proper cols
			grid.Visible = true; // now visible

			Qrc.MoleculeGridPageControl = GridPageControl; // link query results to this page

			GridPanel.SetDataSource(Qm, dt);

			ResultsFormatter fmtr = Qm.ResultsFormatter;
			if (fmtr != null && fmtr.FormattingStarted  && !fmtr.BrowseExistingResults) // if already formatting need to start grid display
				grid.StartDisplay(); 

			ConfigureCount++;
			return;
		}

		/// <summary>
		/// Get the scale to fit view within pagewidth
		/// </summary>
		/// <param name="pct"></param>

		public override int GetFitPageWidthScale()
		{
			if (Qm != null && Qm.MoleculeGrid != null)
				return Qm.MoleculeGrid.GetFitPageWidthScale();

			else return 100; // default to 100%
		}


		/// <summary>
		/// Scale the view to the specified pct value
		/// </summary>
		/// <param name="pct"></param>

		public override void ScaleView(int pct)
		{
			BaseQuery.ViewScale = pct; // single scale for all grid views for now
			GridControl.ScaleView(pct);
		}

		/// <summary>
		/// Update the view to reflect changes in filtering
		/// </summary>

		public override void UpdateFilteredView()
		{
			if (GridControl == null) return;

			GridControl.RefreshDataSource();
			return;
		}

		/// <summary>
		/// Refresh the view
		/// </summary>

		public override void Refresh()
		{
			if (GridControl == null) return;

			GridControl.Refresh();
			return;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>
		
		public override void FreeControlReferences()
		{
			GridPageControl = null;

			base.FreeControlReferences();
			return;
		}

	}
}
