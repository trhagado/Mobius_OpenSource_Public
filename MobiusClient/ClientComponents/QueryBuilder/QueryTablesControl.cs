using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo; 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class QueryTablesControl : XtraUserControl
	{
		public QueryTable CurrentQt; // current query table
		TableList TableListForm; // form instance currently displaying the table list

		Point TabDragPoint = Point.Empty;
		XtraTabPage TabDragPage = null, LastSwappedPage = null;

		public QueryTablesControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Query object to edit
		/// </summary>

		public Query Query
		{
			get
			{
				return _query;
			}

			set
			{
				_query = value;
        SetCurrentQt(); // set current query table also
				//Render(); // show it (don't because too many repeats)
			}
		}
		private Query _query; // internal object

		/// <summary>
		/// Show the tool header above the set of tables
		/// </summary>

		public bool ShowHeader
		{
			get
			{
				return _showHeader;
			}

			set
			{
				_showHeader = value;
				if (_showHeader)
				{
					Tabs.Top = 28;
				}
				else
				{
					Tabs.Top = 0;
				}
			}
		}
		private bool _showHeader = true;

/// <summary>
/// Show the criteria tab
/// </summary>

		public bool ShowCriteriaTab
		{
			get
			{
				return _showCriteriaTab;
			}

			set
			{
				_showCriteriaTab = value;
			}
		}
		private bool _showCriteriaTab = true;

/// <summary>
/// Set query & render
/// </summary>
/// <param name="query"></param>

		public void Render(
			Query query)
		{
			Query = query;
			Render(0);
		}

		/// <summary>
		/// Render & display 1st table
		/// </summary>

		public void Render()
		{
			Render(0);
		}

		/// <summary>
		/// Render query and set current table tab
		/// </summary>
		/// <param name="query"></param>
		/// <param name="ti"></param>

		public void Render(
			int ti)
		{
			QueryTable qt;

			if (Query == null) return;

			//Tabs.BeginUpdate();
			if (ti >= 0 && ti < Query.Tables.Count)
				qt = Query.Tables[ti];
			else qt = null;
			Render(qt);
			//Tabs.EndUpdate();
			Application.DoEvents();
		}

		/// <summary>
		/// Render query and set current table tab
		/// </summary>
		/// <param name="query"></param>
		/// <param name="qt"></param>

		public void Render(
			QueryTable qt)
		{
			QueryTable qt2;
			MetaTable mt;
			int i1;

			if (Query == null) return;

			try
			{
				Tabs.TabPages.Clear();

				if (Query.Tables.Count > 0 && ShowCriteriaTab)
					AddCriteriaTab(); // insert initial tab

				for (i1 = 0; i1 < Query.Tables.Count; i1++)
				{
					qt2 = Query.Tables[i1];
					AddQueryTableTab(qt2);
				}

				//if (Query.KeyCriteria.ToUpper().Trim() == "IN LIST CURRENT") // update current list count
				//  Query.KeyCriteriaDisplay = "In current search results list (" + SS.I.CurrentCountNotNull.ToString() + ")";

				if (qt == null) qt = Query.CurrentTable;
				if (qt != null)
				{
					int qti = SelectQueryTableTab(qt);
					if (qti >= 0)
					{
						Tabs.TabPages[qti].Controls.Add(TableControlPrototype); // store the prototype control
						RenderQueryTable(qt);
						CurrentQt = qt; // make this the current query table
						if (QueryEngineStatsForm.ShowStats) QueryEngineStatsForm.StartNewQueryExecution(Query);
						return;
					}
				}

				CurrentQt = null;
				if (Query.Tables.Count > 0)
				{
					if (ShowCriteriaTab) RenderCriteriaTab();

					else
					{
						Tabs.TabPages[0].Controls.Add(TableControlPrototype); // store the prototype control
						qt = Query.Tables[0];
						RenderQueryTable(qt);
						CurrentQt = qt; // make this the current query table
					}
				}

				else // render empty query table
				{
					qt = new QueryTable();
					qt.MetaTable = new MetaTable();
					RenderQueryTable(qt);
				}

				if (QueryEngineStatsForm.ShowStats) QueryEngineStatsForm.StartNewQueryExecution(Query);
				return;
			}

			catch (Exception ex)
			{
				string msg = DebugLog.FormatExceptionMessage(ex);
				QueryEngine.LogExceptionAndSerializedQuery(msg, Query);
				throw new Exception(ex.Message, ex); // pass it up
			}
		}

/// <summary>
/// Set the current query table for the current query
/// </summary>

		public void SetCurrentQt()
		{
			if (Query != null && Query.Tables.Count > 0)
				CurrentQt = Query.Tables[0];
			else CurrentQt = null;
		}

/// <summary>
/// Display query table in query grid
/// </summary>
/// <param name="qt"></param>
/// <returns></returns>

		public void RenderQueryTable ( 
			QueryTable qt)
		{
			if (qt == null) return;
			if (Tabs.SelectedTabPageIndex < 0) return;

			XtraTabPage tp = Tabs.TabPages[Tabs.SelectedTabPageIndex];
			tp.Controls.Clear();
			tp.Controls.Add(TableControlPrototype);
			TableControlPrototype.Render(qt);
		}       

		/// <summary>
		/// Add criteria tab if no tabs so far
		/// </summary>

		public void AddCriteriaTab()
		{
			XtraTabPage tp = null;
			if (Tabs.TabPages.Count != 0) return;

			tp = Tabs.TabPages.Add("Criteria Summary");
			tp.Controls.Add(CriteriaPanelPrototype);
			ClearCriteriaTab();
			return;
		}

		/// <summary>
		/// Select criteria tab (always first tab)
		/// </summary>

		public void SelectCriteriaTab()
		{
			if (Tabs.TabPages.Count == 0) return;

			if (Tabs.SelectedTabPageIndex != 0) Tabs.SelectedTabPageIndex = 0;
			return;
		}

/// <summary>
/// Render the criteria tab panel
/// </summary>

		public void RenderCriteriaTab()
		{
			SelectCriteriaTab();
			CriteriaPanelPrototype.Render(Query);
			return;
		}


		/// <summary>
		/// Remove controls from criteria tab
		/// </summary>

		void ClearCriteriaTab()
		{
			if (Tabs.TabPages.Count == 0) return;

			CriteriaPanelPrototype.Clear();

			if (Tabs.SelectedTabPageIndex != 0)	Tabs.SelectedTabPageIndex = 0; // criteria tab is first
		}

		/// <summary>
		/// Add a tab for a new query table
		/// </summary>
		/// <param name="title"></param>

		public void AddQueryTableTab(
			QueryTable qt)
		{
			string label = qt.ActiveLabel;
			Tabs.TabPages.Add(label);
			return;
		}

/// <summary>
/// Insert a new tab for a QueryTable based on the position of the table in the Query.Tables list
/// </summary>
/// <param name="qt"></param>

		public void InsertQueryTableTab(
			QueryTable qt)
		{
			int ti = Query.GetQueryTableIndex(qt);
			if (ti < 0) return;
			if (ShowCriteriaTab) ti++; // adjust for criteria tab

			if (ti > Tabs.TabPages.Count) throw new Exception("ti > Tabs.TabPages.Count");
			Tabs.TabPages.Insert(ti);
			Tabs.TabPages[ti].Text = qt.ActiveLabel;

			return;
		}


		/// <summary>
		/// Select an item in the querybuilder table list
		/// </summary>
		/// <param name="qt"></param>

		public int SelectQueryTableTab(
			QueryTable qt)
		{
			QueryTable qt2;
			int qti = Query.GetQueryTableIndex(qt);

			if (qti < 0)
			{
				qti = Query.GetQueryTableIndexByName(qt.MetaTable.Name);
				if (qti < 0) return -1;
			}

			if (ShowCriteriaTab) qti++;

			if (Tabs.SelectedTabPageIndex != qti) Tabs.SelectedTabPageIndex = qti;
			return qti;
		}

		/// <summary>
		/// Select an item in the querybuilder table list
		/// </summary>
		/// <param name="ti">Index of item to show</param>

		public void SelectQueryTableTab (
			int qti) 
		{
			if (ShowCriteriaTab) qti++;

			if (Tabs.SelectedTabPageIndex != qti)
			{
				try { Tabs.SelectedTabPageIndex = qti; }
				catch (Exception ex) // occasionally fails for unknown reason
				{ 	DebugLog.Message(ex); 	} // just log it & continue
			}
			return;
		}

/// <summary>
/// Remove specified tab
/// </summary>
/// <param name="tabIndex"></param>

		public void RemoveTab(
			int tabIndex)
		{
			Tabs.TabPages.RemoveAt(tabIndex);
			return;
		}

		///////////////////////////////////////////////////////////////
		////////////////////// Event code /////////////////////////
		///////////////////////////////////////////////////////////////

		//private void FormattingButton_Click(object sender, EventArgs e)
		//{
		//  QueryColumn qc = TableControlPrototype.CurrentQc;
		//  if (TableControlPrototype.CurrentQc == null)
		//  {
		//    MessageBoxMx.Show(
		//      "Before using this button to define formatting for a field\r\n" +
		//      "you must select the row in the grid corresponding to the field\r\n" +
		//      "that you want to define formatting for.\r\n\r\n" +
		//      "You can also define formatting for a field by clicking on\r\n" +
		//      "one of the small arrows in the Data Field column.", 
		//      UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		//    return;
		//  }

		//  QueryTableControl.SetupColumnFormattingContextMenu(TableControlPrototype.ColumnFormattingContextMenu, qc, TableControlPrototype.UseNamedCfMenuItem_Click);
		//  TableControlPrototype.ColumnFormattingContextMenu.Show(FormattingButton,
		//    new System.Drawing.Point(0, FormattingButton.Height));
		//}

// RunQuery click & dropdown menu item clicks

		private void RunQueryButton_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTablesRunQuery");
			QueryExec.RunQuery(Query, OutputDest.WinForms);
		}

		private void RunQueryContextMenuButton_Click(object sender, EventArgs e)
		{
			if (ShowCriteriaTab && Tabs.SelectedTabPageIndex == 0)
				RunQuerySingleTableMenuItem.Enabled = RunQueryPreviewMenuItem.Enabled = false;

			else RunQuerySingleTableMenuItem.Enabled = RunQueryPreviewMenuItem.Enabled = true;

			RunQueryContextMenu.Show(RunQueryButton,
				new System.Drawing.Point(0, RunQueryButton.Size.Height));
		}

		private void RunQueryPreviewMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTablesPreviewTable");
			CommandExec.ExecuteCommandAsynch("RunQuerySingleTablePreview " + CurrentQt.MetaTable.Name);
		}

		private void RunQuerySingleTableMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTablesRunQuerySingleTable");
			CommandExec.ExecuteCommandAsynch("RunQuerySingleTable " + CurrentQt.MetaTable.Name);
		}

		private void RunQueryDetachedMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTablesRunQueryInBackground");

			CommandExec.ExecuteCommandAsynch("SpawnQueryInBackground");
		}

		private void RunQueryBrowseMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryTablesRunQueryBrowsePreviousResults");
			CommandExec.ExecuteCommandAsynch("Browse");
		}

		private void OptionsButton_Click(object sender, EventArgs e)
		{
			if (CurrentQt == null) return;

			QueryOptionsDialog.Show(Query, "", CurrentQt); // open with last selected page
		}

		private void OptionsButton_ArrowButtonClick(object sender, EventArgs e)
		{
			OptionsContextMenu.Show(OptionsButton, // show popup menu
				new System.Drawing.Point(0, OptionsButton.Size.Height));
		}

		private void TableSumPosMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentQt == null) return;

			SessionManager.LogCommandUsage("QueryTablesOptionsTableSummary");
			QueryOptionsDialog.Show(Query, "SumPos", CurrentQt);
		}

		private void AlertOptionsMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentQt == null) return;

			SessionManager.LogCommandUsage("QueryTablesOptionsAlerts");
			QueryOptionsDialog.Show(Query, "Alert", CurrentQt);
		}

		private void MiscOptionsMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentQt == null) return;

			SessionManager.LogCommandUsage("QueryTablesOptionsMisc");
			QueryOptionsDialog.Show(Query, "Misc", CurrentQt);
		}

		private void AdvancedOptionsMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentQt == null) return;

			SessionManager.LogCommandUsage("QueryTablesOptionsAdvanced");
			QueryOptionsDialog.Show(Query, "Advanced", CurrentQt);
		}

		private void SelectedDataButton_Click(object sender, EventArgs e)
		{ // show dropdown list of selected tables
			if (Tabs.TabPages.Count == 0) return;

			TableListForm = new TableList();
			for (int i1 = 0; i1 < Tabs.TabPages.Count; i1++)
			{
				XtraTabPage tp = Tabs.TabPages[i1];
				TableListForm.List.Items.Add(tp.Text);
				if (Tabs.SelectedTabPageIndex == i1)
					TableListForm.List.SelectedIndex = TableListForm.List.Items.Count - 1;
			}

			Point p = SelectedDataButton.PointToScreen(new Point(0, SelectedDataButton.Height));
			TableListForm.Left = p.X;
			TableListForm.Top = p.Y;
			TableListForm.SelectedMethod = new System.EventHandler(this.SelectedDataLabelMenuItem_Click);
			TableListForm.Show(SessionManager.ActiveForm);
			return;
		}

		private void SelectedDataButton_ArrowButtonClick(object sender, EventArgs e)
		{ // show dropdown list of selected tables
			SelectedDataButton_Click(sender, e);
		}

		public void SelectedDataLabelMenuItem_Click(object sender, System.EventArgs e)
		{ // item selected from menu, select corresponding query builder table tab
			Tabs.SelectedTabPageIndex = TableListForm.List.SelectedIndex;
		}

/// <summary>
/// Switching QueryTables
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void QbTabs_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
		{
			if (!Visible) return;

			int ti = Tabs.SelectedTabPageIndex;
			if (ti >= 0) // real query table?
			{
				if (ShowCriteriaTab)
				{
					if (ti == 0) // are we going to the criteria tab? 
					{
						RenderCriteriaTab();
						CurrentQt = null;
						return;
					}

					ti--; // adjust for criteria tab
				}

				if (Query != null && Query.Tables != null && ti < Query.Tables.Count)
				{
					CurrentQt = Query.Tables[ti];
					RenderQueryTable(CurrentQt);
					return;
				}
			}

// Draw empty query table

			QueryTable qt2 = new QueryTable();
			qt2.MetaTable = new MetaTable();
			RenderQueryTable(qt2);
		}

		/// <summary>
		/// Show menu for table
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tabs_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				XtraTabHitInfo hitInfo = Tabs.CalcHitInfo(Tabs.PointToClient(Control.MousePosition));
				if (hitInfo.HitTest == XtraTabHitTest.PageHeader)
				{
					XtraTabPage tp = hitInfo.Page;
					if (ShowCriteriaTab && tp == Tabs.TabPages[0])
						return; // no menu if criteria tab

					int x = Cursor.Position.X;
					int y = Cursor.Position.Y;
					Point p = new Point(x, y);
					p = this.PointToClient(p);
					TableControlPrototype.QbTableLabelContextMenu.Show(this, p);
				}
			}
		}

	/// <summary>
	/// Remove table from query
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>

	private void Tabs_CloseButtonClick(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 0 && ShowCriteriaTab)
			{
				MessageBoxMx.ShowError("The Criteria Summary tab cannot be removed.");
				return;
			}

			int qti = Tabs.SelectedTabPageIndex;
			if (ShowCriteriaTab) qti--;
			QbUtil.RemoveQueryTable(qti);
		}

/// <summary>
/// Handle rearranging of tab order for QueryTables
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Tabs_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			XtraTabControl c = sender as XtraTabControl;
			TabDragPoint = new Point(e.X, e.Y);
			XtraTabHitInfo hi = c.CalcHitInfo(TabDragPoint);
			if (hi.Page == null || c.TabPages.IndexOf(hi.Page) == 0) // page other than first (criteria) page must be selected
			{
				TabDragPoint = Point.Empty;
				hi.Page = null;
			}

			TabDragPage = hi.Page;
			LastSwappedPage = null;
			return;
		}

		private void Tabs_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				if ((TabDragPoint != Point.Empty) && ((Math.Abs(e.X - TabDragPoint.X) > SystemInformation.DragSize.Width) || (Math.Abs(e.Y - TabDragPoint.Y) > SystemInformation.DragSize.Height)))
					Tabs.DoDragDrop(sender, DragDropEffects.Move);
		}

		private void Tabs_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			XtraTabControl c = sender as XtraTabControl;
			if (c == null) return;

			XtraTabHitInfo hi = c.CalcHitInfo(c.PointToClient(new Point(e.X, e.Y)));
			if (hi.Page != null && c.TabPages.IndexOf(hi.Page) != 0)
			{
				if (hi.Page != TabDragPage && hi.Page != LastSwappedPage) // move it
				{
					int hpi = c.TabPages.IndexOf(hi.Page);
					int dpi = c.TabPages.IndexOf(TabDragPage);
					if (hpi < dpi) // moving left
					{
						c.TabPages.Move(hpi, TabDragPage);
					}

					else // moving right
					{
						c.TabPages.Move(hpi + 1, TabDragPage);
					}

					hpi--; // adjust for criteria tab
					dpi--;
					QueryTable qt = Query.Tables[dpi]; // move the query table
					Query.RemoveQueryTableAt(dpi);
					Query.InsertQueryTable(hpi, qt);
				}

				LastSwappedPage = hi.Page;
				e.Effect = DragDropEffects.Move;
			}

			else e.Effect = DragDropEffects.None;
		}

		private void OptionsContextMenu_Opening(object sender, CancelEventArgs e)
		{
			string queryId = Query.UserObject.Id > 0 ? Query.UserObject.Id.ToString() : "Undefined";
			QueryIdMenuItem.Text = "=== Query Id: " + queryId + " ===";
			//SaveSQLForSpotfireUseMenuItem.Visible = SS.I.IsAdmin;
		}

		private void ShowOpenQueryURLMenuItem_Click(object sender, EventArgs e)
		{
			new QueryOptionsDialog(Query).ShowOpenQueryURL();
		}

		private void ShowRunQueryUrlMenuItem_Click(object sender, EventArgs e)
		{
			new QueryOptionsDialog(Query).ShowRunQueryUrl();
		}

		private void ShowMqlMenuItem_Click(object sender, EventArgs e)
		{
			CommandLine.ShowMql("format");
		}

		private void ShowSqlMenuItem_Click(object sender, EventArgs e)
		{
			CommandLine.ShowQuerySqlStatements();
		}

		private void SaveSQLForSpotfireUseMenuItem_Click(object sender, EventArgs e)
		{
			SpotfireLinkUI.StoreSpotfireQueryTableSql();
		}

		private void ShowQueryXmlMenuItem_Click(object sender, EventArgs e)
		{
			CommandLine.ShowQueryXml();
		}

		private void ShowMetaTableXmlMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentQt == null)
			{
				MessageBoxMx.ShowError("You must first select the desired table from the table list.");
				return;
			}

			CommandLine.ShowMetaTableXml(CurrentQt.MetaTable.Name);
		}


	}
}
