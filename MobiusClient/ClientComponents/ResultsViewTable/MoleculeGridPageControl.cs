using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.BandedGrid;

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
/// User control containing a results toolbar and a MoleculeGridPanel
/// </summary>

	public partial class MoleculeGridPageControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal QueryManager QueryManager { get { return MoleculeGridPanel.Grid.QueryManager; }} // use QueryManager for current contained grid
		internal DataTableMx DataTable { get { return QueryManager.DataTable; } }
		internal DataTableManager Dtm { get { return QueryManager.DataTableManager; } }
		internal Query Query { get { return QueryManager.Query; } } // associated query
		internal ResultsFormat ResultsFormat { get { return QueryManager.ResultsFormat; } } // associated results format
		internal ResultsFormat Rf { get { return QueryManager.ResultsFormat; } } // alias
		internal ResultsFormatter Formatter { get { return QueryManager.ResultsFormatter; } } // associated formatter
		internal MoleculeGridControl MoleculeGrid { get { return QueryManager.MoleculeGrid; } }

		TableList TableList; // dropdown list of tables from Results button
		Dictionary<string, int> BandCaptionDict; // associated band captions and caption index

		public static bool RemoveToolPanelInConstructor = true;

		/// <summary>
		/// Constructor - Normally remove the Tool panel which will be later inserted into a higher level panel
		/// </summary>

		public MoleculeGridPageControl()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			MoleculeGridPanel.BorderStyle = BorderStyle.None;

			if (RemoveToolPanelInConstructor)
			{
				Controls.Remove(ToolPanel); // remove the tool panel and tools from this control
				MoleculeGridPanel.Dock = DockStyle.Fill; // fill page with grid control
			}

			MoleculeGridPanel.PageControl = this;
			ExportSpotfireMenuItem.Visible = ClientState.IsDeveloper;
			return;
		}

		/// <summary>
		/// AdjustToolPanelButtonSpacing based on which buttons are visible
		/// </summary>
		public void AdjustToolPanelButtonSpacing()
		{
			Control[] ca = new Control[] { SortButton, MarkDataButton, FormattingButton, PrintButton, EditQueryButton, ExportButton, ScrollToBottom, ScrollToTop };

			int gap = 4;
			int x = ToolPanel.Width;

			for (int ci = ca.Length - 1; ci >= 0; ci--)
			{
				Control c = ca[ci];
				if (!c.Visible) continue;
				c.Left = x - c.Width;
				x = c.Left - gap;
			}
		}

		/// <summary>
		/// Setup the proper grid, banded or layout, based on the ResultsFormat setting
		/// </summary>

		internal MoleculeGridControl SetGrid()
		{
			return MoleculeGridPanel.SelectBaseGridViewGrid(QueryManager);
		}

		internal MoleculeBandedGridViewManager View; // the view associated with the control

		/// <summary>
		/// Link the view and the page
		/// </summary>

		public void LinkViewToMoleculeGridPageControl(
			MoleculeBandedGridViewManager view)
		{
			View = view;

			if (!Visible) return;
			if (SS.I.UISetupLevel >= 0) return;
			MoleculeGridPanel.LinkViewToPanel(view);

			return;
		}

/// <summary>
/// Show dropdown list of table headers
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void BrowseLabel_Click(object sender, EventArgs e)
		{
			BrowseLabel_ArrowButtonClick(sender, e);
		}

		private void BrowseLabel_ArrowButtonClick(object sender, EventArgs e)
		{
			//Point location = TableViewDropDownButton.PointToScreen(new Point(0, TableViewDropDownButton.Height)); // (removed)
			//ShowTableListDropDown(location);
		}

/// <summary>
/// Show dropdown list of table names to choose from to scroll to
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		internal void ShowTableListDropDown(Point location)
		{ 
			TableList = new TableList();

			BandCaptionDict = new Dictionary<string, int>();

			BandedGridView v = MoleculeGridPanel.BandedViewGrid.V as BandedGridView;
			if (v == null) return;

			for (int i1 = 0; i1 < v.Bands.Count; i1++)
			{ // scan top row for table labels
				string txt = v.Bands[i1].Caption;
				if (txt == null) continue;
				txt = txt.Replace("  ", " ");
				if (txt.Length <= 1) continue;
				if (BandCaptionDict.ContainsKey(txt)) continue;

				BandCaptionDict[txt] = i1; // store band index
				TableList.List.Items.Add(txt);
			}

			BandCaptionDict = BandCaptionDict;
			TableList.Location = location;
			TableList.List.SelectedIndexChanged += new System.EventHandler(TableList_List_SelectedIndexChanged);
			TableList.Show();
		}

		private void TableList_List_SelectedIndexChanged(object sender, EventArgs e)
		{ // item selected from list, select corresponding band in grid
			GridBandColumnCollection cols;

			string txt = (string)TableList.List.SelectedItem;
			TableList.Close();

			if (!BandCaptionDict.ContainsKey(txt)) return;
			int leftCol = BandCaptionDict[txt];

			BandedGridView v = MoleculeGridPanel.BandedViewGrid.V as BandedGridView;
			if (v == null) return;

			cols = v.Bands[v.Bands.Count - 1].Columns; // go to end first
			v.MakeColumnVisible(cols[cols.Count - 1]);
			cols = v.Bands[BandCaptionDict[txt]].Columns; // then back to beginning
			v.MakeColumnVisible(cols[0]);

			return;
		}

// Sorting

		internal void SortBut_Click(object sender, EventArgs e)
		{
			SimpleButton b = sender as SimpleButton;
			SortContextMenu.Show(b,
				new System.Drawing.Point(0, b.Height));
		}

		internal void SortAscendingMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.SortOnCurrentColumn(SortOrder.Ascending);
		}

		internal void SortDescendingMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.SortOnCurrentColumn(SortOrder.Descending);
		}

		internal void SortMultipleMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.SortMultiple();
		}

// Marking

		internal void MarkDataBut_Click(object sender, EventArgs e)
		{
			SimpleButton b = sender as SimpleButton;
			MoleculeGrid.Helpers.ShowMarkedDataContextMenu(b,
				new System.Drawing.Point(0, b.Height));
		}

		// Column formatting button & menu clicks

		internal void FormattingButton_Click(object sender, EventArgs e)
		{
			GridCell[] cells;
			int topRow, bottomRow;
			GridColumn leftCol, rightCol;

			if (MoleculeGrid.BGV == null) return;

			QueryColumn qc = null;
			MoleculeGrid.GetSelectedCells(out cells, out topRow, out bottomRow, out leftCol, out rightCol);
			if (leftCol != null)
			{
				ColumnInfo cInf = MoleculeGrid.GetColumnInfo(leftCol);
				if (cInf != null) qc = cInf.Qc;
			}

			QueryTableControl.SetupColumnFormattingContextMenu(FieldFormattingContextMenu, qc, MoleculeGrid.Helpers.UseNamedCfMenuItem_Click);
			SimpleButton b = sender as SimpleButton;
			FieldFormattingContextMenu.Show(b,
				new System.Drawing.Point(0, b.Height));
		}

		internal void CondFormatMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			MoleculeGrid.Helpers.DefineCondFormat("");
		}

		internal void UseNamedCfMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			MoleculeGrid.Helpers.DefineCondFormat(mi.Tag.ToString());
		}

		internal void ColWidthMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			MoleculeGrid.Helpers.SetColumnWidth();
		}

		internal void ColumnFormatMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			MoleculeGrid.Helpers.SetColumnFormat();
		}

		internal void TableHeaderBackgroundColorMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			MoleculeGrid.Helpers.SetTableHeaderBackgroundColor();
		}

		internal void RenameColMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			MoleculeGrid.Helpers.RenameColumn();
		}

		internal void RenameColForAllUsersMenuItem_Click(object sender, EventArgs e)
		{
			if (!SingleColumnSelected()) return;
			MoleculeGrid.Helpers.RenameColumnAllUsers();
		}

		internal void HideColumnsMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.HideColumns();
		}

		internal void HideRowsMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.HideRows();
		}

		internal void UnhideSelectedColumnsMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.UnhideColumns();
		}

		internal void UnhideAllColumnsMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.UnhideAllColumns();
		}

		internal void UnhideRowsMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.UnhideRows();
		}

		internal void UnhideAllRowsMenuItem_Click(object sender, EventArgs e)
		{
			MoleculeGrid.Helpers.UnhideAllRows();
		}

		internal bool SingleColumnSelected() // return true if a single column is selected
		{
			GridCell[] cells;
			int topRow, bottomRow;
			GridColumn leftCol, rightCol;

			if (MoleculeGrid.BGV == null) return false;

			MoleculeGrid.GetSelectedCells(out cells, out topRow, out bottomRow, out leftCol, out rightCol);
			if (leftCol == null) // column selected?
			{
				MessageBoxMx.Show(
					"Before using this button to define formatting for a field\r\n" +
					"you must select the column in the grid corresponding to the field\r\n" +
					"that you want to define formatting for.\r\n\r\n" +
					"You can also define formatting for a field by right-clicking on\r\n" +
					"the corresponding column header.",
					"Must First Select a Column to Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			else if (leftCol != rightCol) // just a single column
			{
				MessageBoxMx.Show(
					"Before using this button to define formatting for a field\r\n" +
					"you must select a SINGLE column in the grid corresponding to the field\r\n" +
					"that you want to define formatting for.\r\n\r\n" +
					"You can also define formatting for a field by right-clicking on\r\n" +
					"the corresponding column header.",
					"Can't Format Multiple Columns at One Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			else return true;
		}

		// Print

		internal void PrintBut_Click(object sender, EventArgs e)
		{
			PrintPreviewDialog.Show(QueryManager, QueryManager.MoleculeGrid);
		}

		// Export menu

		internal void ExportBut_Click(object sender, EventArgs e)
		{
			SimpleButton b = sender as SimpleButton;
			ExportContextMenu.Show(b,
				new System.Drawing.Point(0, b.Height));
		}

		internal void ExportExcelMenuItem_Click(object sender, EventArgs e)
		{
			//CommandExec.Execute("Export Excel");
			ResultsFormatter.ExecuteExportCommand("Excel", QueryManager);
		}

		internal void ExportWordMenuItem_Click(object sender, EventArgs e)
		{
			//CommandExec.Execute("Export Word");
			ResultsFormatter.ExecuteExportCommand("Word", QueryManager);
		}

		internal void ExportSpotfireMenuItem_Click(object sender, EventArgs e)
		{
			//CommandExec.Execute("Export Spotfire");
			ResultsFormatter.ExecuteExportCommand("Spotfire", QueryManager);
		}

		internal void ExportCSVMenuItem_Click(object sender, EventArgs e)
		{
			//CommandExec.Execute("Export Textfile");
			ResultsFormatter.ExecuteExportCommand("Textfile", QueryManager);
		}

		internal void ExportSDFileMenuItem_Click(object sender, EventArgs e)
		{
			//CommandExec.Execute("Export SDFile");
			ResultsFormatter.ExecuteExportCommand("SDFile", QueryManager);
		}

// Back to edit query

		internal void EditQueryBut_Click(object sender, EventArgs e)
		{
			ContainerControl cc;

			if (ViewManager.TryCallCustomExitingQueryResultsCallback(MoleculeGridPanel, ExitingQueryResultsType.EditQuery)) return;

			if (ViewManager.IsControlContainedInQueriesControl(MoleculeGridPanel))
				CommandExec.ExecuteCommandAsynch("EditQuery");

			else if (ViewManager.IsControlContainedInPopupResultsControl(MoleculeGridPanel))
			{
				string msg = "Do you want to create a new query from the currently displayed results?";
				DialogResult dr = MessageBoxMx.Show(msg, "Create New Query", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes) return;

				Query q = MoleculeGrid.Query.Clone(); // make copy of query

				QbUtil.AddQuery(q);
				QbUtil.EditQuery();
				return;
			}

			else
			{
				MessageBoxMx.ShowError("Unable to edit query. Not contained in a recognized container type");
				return;
			}
		}

		// Scrolling buttons

		internal void ScrollToBottom_Click(object sender, EventArgs e)
		{
			MoleculeGrid.ScrollToBottom();
		}

		internal void ScrollToTop_Click(object sender, EventArgs e)
		{
			MoleculeGrid.ScrollToTop();
		}

	}
}
