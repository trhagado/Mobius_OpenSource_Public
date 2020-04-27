using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraRichEdit.Import.OpenXml;

namespace Mobius.ClientComponents
{
	public partial class QueryOptionsDialog : DevExpress.XtraEditors.XtraForm
	{
		public static QueryOptionsDialog Instance;

		Query Query;
		Dictionary<string, QueryTable> OriginalQueryTables;
		bool EditAlertOnOpen = false;
		MetaColumn DnfMc;
		QueryColumn DnfQc;

		DataTable DataTable = null; // DataTable of grid information
		GridView V { get { return GridView; } }
		GridHitInfo DownHitInfo = null;

		public QueryOptionsDialog()
		{
			InitializeComponent();

			//if (ClientState.IsDeveloper) // enable for dev only for now
			//  UseCachedData.Enabled = UseCachedDataAlways.Enabled = true;
		}

		public QueryOptionsDialog(Query query)
		{
			InitializeComponent();

			Query = query;
		}

/// <summary>
/// Show the QueryOptions dialog
/// </summary>
/// <param name="query"></param>
/// <param name="optionPage"></param>
/// <returns></returns>

		public static DialogResult Show(
			Query query,
			string optionPage)
		{
			int ti = 0;
			return Show(query, optionPage, ref ti);
		}

/// <summary>
/// Show the QueryOptions dialog
/// </summary>
/// <param name="query"></param>
/// <param name="optionPage"></param>
/// <param name="qt"></param>
/// <returns></returns>

		public static DialogResult Show(
			Query query,
			string optionPage,
			QueryTable qt)
		{
			int ti = query.GetQueryTableIndex(qt);
			return Show(query, optionPage, ref ti);
		}

/// <summary>
/// Show the QueryOptions dialog
/// </summary>
/// <param name="query"></param>
/// <param name="tabName"></param>
/// <param name="?"></param>
/// <returns></returns>

		public static DialogResult Show(
			Query query,
			string optionPage,
			ref int ti)
		{
			if (Instance == null) Instance = new QueryOptionsDialog();
			Instance.Setup(query, optionPage, ref ti);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			return dr;
		}

/// <summary>
/// Setup the form
/// </summary>
/// <param name="query"></param>
/// <param name="optionPage"></param>
/// <param name="ti"></param>

		void Setup (
			Query query,
			string optionPage,
			ref int ti)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			string txt;
			int i1, i2;

			Query = query;

			string formCaption = "Query Options";
			if (Security.IsAdministrator(Security.UserName) && query.UserObject.Id > 0)
				formCaption += " (Query Id: " + query.UserObject.Id + ")";
			Text = formCaption;

			int tpi = -1;

			if (optionPage == "SumPos") tpi = 0; 
			else if (optionPage == "Alert") tpi = 1;
			else if (optionPage == "Misc") tpi = 1;
			else if (optionPage == "Advanced") tpi = 2;

			if (tpi >= 0) Tabs.SelectedTabPageIndex = tpi; // goto any specified page, otherwise stay where we were last time

			// Setup general tab

			txt = SS.I.DefaultNumberFormat.ToString() + " (" + SS.I.DefaultDecimals.ToString() + ")";
			DefaultNumberFormat.Text = txt;

			txt = QbUtil.ConvertStatDisplayFormat(query.StatDisplayFormat);
			QnfStatsTextEdit.Text = txt;

			SetAlertText(query);

			TableColumnZoom.ZoomPct = SS.I.TableColumnZoom; 	// Setup zoom controls
			GraphicsColumnZoom.ZoomPct = SS.I.GraphicsColumnZoom;

// Advanced options - query specific

			DuplicateKeyValues.Checked = query.DuplicateKeyValues;
			FilterNullRows.Checked = query.FilterNullRows;
			ShowCondFormatLabels.Checked = query.ShowCondFormatLabels;
			//BrowseExistingResultsWhenOpened.Checked = query.BrowseExistingResultsWhenOpened;
			RunQueryWhenOpened.Checked = query.RunQueryWhenOpened;
			//UseCachedData.Checked = query.UseCachedData;
			CombineSearchAndRetrieval.Checked = query.SingleStepExecution;
			Multitable.Checked = query.Multitable; // (not visible)
            MobileQuery.Checked = query.Mobile;


// Advanced options - applies to all queries

			RepeatReport.Checked =  SS.I.RepeatReport;
			ShowStereoComments.Checked =  SS.I.ShowStereoComments;
			BreakHtmlPopupsAtPageWidth.Checked =  SS.I.BreakHtmlPopupsAtPageWidth;
			RestoreWindowsAtStartup.Checked =  SS.I.RestoreWindowsAtStartup;

			HilightCorpIdChanges.Checked = SS.I.HilightCidChanges;
			RemoveLeadingZerosFromCids.Checked = SS.I.RemoveLeadingZerosFromCids;
			MarkCheckBoxesInitially.Checked = SS.I.GridMarkCheckBoxesInitially;

			EvenRowBackgroundColor.Color = SS.I.EvenRowBackgroundColor;
			OddRowBackgroundColor.Color = SS.I.OddRowBackgroundColor;

			DnfMc = new MetaColumn(); // setup to get new number format info
			DnfMc.DataType = MetaColumnType.QualifiedNo; 
			DnfMc.Format = SS.I.DefaultNumberFormat;
			DnfMc.Decimals = SS.I.DefaultDecimals;
			DnfQc = new QueryColumn();
			DnfQc.MetaColumn = DnfMc;

			DefaultToSingleStepQueryExecution.Checked = MqlUtil.DefaultToSingleStepQueryExecution;

			// Setup summarization / table position tab

			CustomSummarization.Visible = Aggregator.IsAvailableForUser;

			DataTable dt = CreateDataTable();

			OriginalQueryTables = new Dictionary<string, QueryTable>(); // keep set of original tables
			for (i1=0; i1<query.Tables.Count; i1++)
			{
				qt = query.Tables[i1];
				mt = qt.MetaTable;

				object cs;
				if (mt.SummarizedExists)
				{
					if (qt.MetaTable.UseSummarizedData) cs = true;
					else cs = false;
				}
				else cs = DBNull.Value;

				string tableLabel = qt.ActiveLabel;
				while (OriginalQueryTables.ContainsKey(tableLabel)) tableLabel += "."; // avoid unlikely but possible dup name
				OriginalQueryTables.Add(tableLabel, qt); // accumulate original labels

				int selectCount = 0, criteriaCount = 0;
				for (i2 = 0; i2 < qt.QueryColumns.Count; i2++)
				{
					qc = qt.QueryColumns[i2];
					if (qc.Selected) selectCount++;
					if (qc.Criteria != "") criteriaCount++;
				}
				txt = selectCount.ToString() + "/" + 
					qt.QueryColumns.Count.ToString() + ", " + criteriaCount.ToString();

				DataRow dr = DataTable.NewRow();
				dr["SummarizedCol"] = cs;
				dr["TableNameCol"] = tableLabel;
				dr["SelectedFieldsAndCriteriaCountsCol"] = txt;
				DataTable.Rows.Add(dr);
			}

			TableGrid.DataSource = DataTable;

			if (ti >= 0 && ti < query.Tables.Count)
				SelectRow(ti);

			if (ServicesIniFile.Read("QueryOptionsHelpUrl") != "")
				Help.Enabled =  true;

			EditAlertOnOpen = Lex.Eq(optionPage, "Alert");

			return;
		}

		DataTable CreateDataTable()
		{
			DataTable dt = new DataTable();
			DataTable = dt; // save ref to table
			dt.Columns.Add("SummarizedCol", typeof(bool));
			dt.Columns.Add("TableNameCol", typeof(string));
			dt.Columns.Add("SelectedFieldsAndCriteriaCountsCol", typeof(string));

			dt.RowChanged += new DataRowChangeEventHandler(DataRowChangeEventHandler);

			return dt;
		}

		void DataRowChangeEventHandler(object sender, DataRowChangeEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Edit alert properties
		/// </summary>

		void EditAlert()
		{
			int alertId = 0;
			Alert alert;

			if (Query.UserObject.Id <= 0) // query been saved?
			{
				MessageBoxMx.ShowError("This query must be saved before an alert can be defined on it.");
				return;
			}

			if (!Alert.QueryValidForAlert(Query)) return; // be sure it is valid

			if (!QbUtil.CanRunQueryInBackground(Query)) return; // further validity checks

			int newAlertId = AlertProperties.Edit(Query.UserObject.Id, out alert);
			if (newAlertId == 0) return; // no change

			SetAlertText(alert);
			return;
		}

/// <summary>
/// Set the text for any alert associated with the query for this user
/// </summary>
/// <param name="queryId"></param>

		public void SetAlertText (
			Query query)
		{
			Alert alert = Alert.GetAlertByQueryId(query.UserObject.Id);
			SetAlertText(alert);
			return;
		}

/// <summary>
/// Set alert text from alert
/// </summary>
/// <param name="alert"></param>

		public void SetAlertText (
			Alert alert)
		{
			string txt = "None";

			if (alert != null)
				txt = "Check every " + alert.Interval + " days";

			AlertDesc.Text = txt;
			return;
		}

		private void RepositoryItemCheckEdit_EditValueChanging(object sender, ChangingEventArgs e)
		{
			if (NullValue.IsNull(e.OldValue)) e.Cancel = true; // don't allow change of indeterminates
			else if (e.OldValue is bool && (bool)e.OldValue == true && NullValue.IsNull(e.NewValue)) 
				e.NewValue = false; // go from checked to not checked and skip indeterminate
		}

		private void Summarize_Click(object sender, EventArgs e)
		{
			if (GridView.RowCount == 0) return;
			int sri = GetSelectedRow();
			object cs = DataTable.Rows[sri]["SummarizedCol"]; 
			if (NullValue.IsNull(cs)) return;
			DataTable.Rows[sri]["SummarizedCol"] = true;
		}

		private void CustomSummarization_Click(object sender, EventArgs e)
		{
			return; // todo
		}

		private void Unsummarized_Click(object sender, EventArgs e)
		{
			if (GridView.RowCount == 0) return;
			int sri = GetSelectedRow();
			object cs = DataTable.Rows[sri]["SummarizedCol"];
			if (NullValue.IsNull(cs)) return;
			DataTable.Rows[sri]["SummarizedCol"] = false;
		}

		void SelectRow(int ri)
		{
			GridView.SelectRow(ri);
			GridView.FocusedRowHandle = ri;
			GridView.Focus();
		}

		int GetSelectedRow()
		{
			int[] rows = GridView.GetSelectedRows();
			if (rows.Length > 0) return rows[0];
			else if (GridView.RowCount > 0) // pick first row if none selected
			{
				SelectRow(0);
				return 0;
			}
			else return -1; // no rows to select
		}

		private void SummarizeAll_Click(object sender, EventArgs e)
		{
			for (int i1 = 0; i1 < DataTable.Rows.Count; i1++)
			{
				object cs = DataTable.Rows[i1]["SummarizedCol"];
				if (NullValue.IsNull(cs)) continue;
				DataTable.Rows[i1]["SummarizedCol"] = true;
			}
		}

		private void UnsummarizedAll_Click(object sender, EventArgs e)
		{
			for (int i1 = 0; i1 < DataTable.Rows.Count; i1++)
			{
				object cs = DataTable.Rows[i1]["SummarizedCol"];
				if (NullValue.IsNull(cs)) continue;
				DataTable.Rows[i1]["SummarizedCol"] = false;
			}
		}

		private void MoveUp_Click(object sender, EventArgs e)
		{
			int[] selectedRows = GridView.GetSelectedRows();
			if (selectedRows.Length == 0 || selectedRows[0] == 0) return;

			int rowDelta = -1;
			MoveRows(selectedRows, rowDelta);
			return;
		}

		private void MoveToTop_Click(object sender, EventArgs e)
		{
			int[] selectedRows = GridView.GetSelectedRows();
			if (selectedRows.Length == 0 || selectedRows[0] == 0) return;

			int rowDelta = -selectedRows[0];
			MoveRows(selectedRows, rowDelta);
			return;
		}

		private void MoveDown_Click(object sender, EventArgs e)
		{
			int[] selectedRows = GridView.GetSelectedRows();
			if (selectedRows.Length == 0) return;

			int lastRi = selectedRows[selectedRows.Length - 1];
			int lastDtRi = DataTable.Rows.Count - 1;
			if (lastDtRi == lastRi) return; // already at bottom?

			int rowDelta = 1;

			MoveRows(selectedRows, rowDelta);
			return;
		}

		private void MoveToBot_Click(object sender, EventArgs e)
		{
			int[] selectedRows = GridView.GetSelectedRows();
			if (selectedRows.Length == 0) return;

			int lastRi = selectedRows[selectedRows.Length - 1];
			int lastDtRi = DataTable.Rows.Count - 1;
			int rowDelta = (lastDtRi - lastRi); 
			if (rowDelta == 0) return; // already at bottom?

			MoveRows(selectedRows, rowDelta);
			return;
		}

		/// <summary>
		/// Move a group of rows up or down in the grid & refresh display
		/// </summary>
		/// <param name="selectedRows"></param>
		/// <param name="rowDelta">Pos or neg number of rows to move the selected rows</param>

		void MoveRows(
			int[] selectedRows,
			int rowDelta)
		{
			if (rowDelta == 0) return;
			int lastSri = selectedRows.Length - 1;

			if (rowDelta < 0) // moving rows up - start with first row & work down
			{
				for (int rai = 0; rai <= lastSri; rai++)
				{
					int sri = selectedRows[rai];
					DataTableUtil.MoveRow(DataTable, sri, sri + rowDelta);
				}
			}

			else // move rows down - start with last row & work up
			{
				for (int rai = lastSri; rai >= 0; rai--)
				{
					int sri = selectedRows[rai];
					DataTableUtil.MoveRow(DataTable, sri, sri + rowDelta);
				}
			}

			int startRow = selectedRows[0] + rowDelta;
			int endRow = selectedRows[lastSri] + rowDelta;
			GridView.SelectRows(startRow, endRow);
            GridView.MakeRowVisible(startRow, true);
            GridView.MakeRowVisible(endRow, true);

            TableGrid.RefreshDataSource();
			return;
		}

		private void RemoveTable_Click(object sender, EventArgs e)
		{
			int[] selectedRows = GridView.GetSelectedRows();
			if (selectedRows.Length == 0) return;
			int lastSri = selectedRows.Length - 1;


			for (int rai = lastSri; rai >= 0; rai--)
			{
					int sri = selectedRows[rai];
					DataTable.Rows.RemoveAt(sri);
			}

			if (DataTable.Rows.Count == 0) return;

			int si = selectedRows[0]; // first row that was deleted
			if (si >= DataTable.Rows.Count) si--;
			SelectRow(si);
			return;
		}

		private void DefaultNumberFormat_KeyPress(object sender, KeyPressEventArgs e)
		{
			ChangeDefaultNumberFormat_Click(null, null);
		}

		private void DefaultNumberFormat_MouseDown(object sender, MouseEventArgs e)
		{
			ChangeDefaultNumberFormat_Click(null, null);
		}

		private void ChangeDefaultNumberFormat_Click(object sender, EventArgs e)
		{
			if (NumberFormatDialog.Show(DnfQc) == DialogResult.OK)
			{ // display new values if changed
				string txt = DnfQc.DisplayFormat.ToString() + " (" + DnfQc.Decimals.ToString() + ")";
				DefaultNumberFormat.Text = txt;
			}
		}

		private void QnfStats_KeyPress(object sender, KeyPressEventArgs e)
		{
			QnfStatsEdit_Click(null, null);
		}

		private void QnfStats_MouseDown(object sender, MouseEventArgs e)
		{
			QnfStatsEdit_Click(null, null);
		}

		private void QnfStatsEdit_Click(object sender, EventArgs e)
		{
			string newStats = QnfStats.Show(QnfStatsTextEdit.Text);
			if (newStats != null)
				QnfStatsTextEdit.Text = newStats;

			return;
		}

		private void AlertDesc_KeyPress(object sender, KeyPressEventArgs e)
		{
			EditAlert_Click(null, null);
		}

		private void AlertDesc_MouseDown(object sender, MouseEventArgs e)
		{
			EditAlert_Click(null, null);
		}

		private void EditAlert_Click(object sender, EventArgs e)
		{
			EditAlert();
		}

		private void AdvancedShowButton_Click(object sender, EventArgs e)
		{
			//SaveSQLForSpotfireUseMenuItem.Visible = SS.I.IsAdmin;

			AdvancedContextMenu.Show(AdvancedShowButton,
				new System.Drawing.Point(0, AdvancedShowButton.Size.Height));
		}

/// <summary>
/// Show the url to open the current query
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ShowOpenQueryURLMenuItem_Click(object sender, EventArgs e)
		{
			ShowOpenQueryURL();
		}

		internal void ShowOpenQueryURL()
		{
			if (Query == null || Query.UserObject == null || Query.UserObject.Id <= 0)
			{
				MessageBoxMx.ShowError("The query must first be saved.");
				return;
			}

			string folder = ServicesIniFile.Read("QueryLinksNetworkFolder"); // get unc form of folder
			string fileName = "Open_Query_" + Query.UserObject.Id + ".bat"; // file name
			string uncPath = folder + '\\' + fileName; // unc file to write now and read later

			string tempFile = TempFile.GetTempFileName();
			StreamWriter sw = new StreamWriter(tempFile);
			string cmd = // batch command to start the Mobius client and open the specified query
				Lex.AddDoubleQuotes(ClientDirs.ExecutablePath) + // path to executable in quotes
				" Mobius:Command='Open Query " + Query.UserObject.Id + "'"; // the mobius command to open
			sw.WriteLine(cmd);
			sw.Close();

			ServerFile.CopyToServer(tempFile, uncPath);
			FileUtil.DeleteFile(tempFile);

			string url = "file:///" + folder.Replace('\\', '/') + "/" + fileName; // put in "file:" schema & switch slashes to get URL from UNC name
			InputBoxMx.Show(
				"The following URL can be used from a web page " +
				"to start Mobius and open the current query:",
				"Open Query URL",
				url);
			//"Mobius:Command='Open Query " + Query.UserObject.Id + "'"); // this is better but isn't accepted by SharePoint

			return;
		}

/// <summary>
/// Show the url to run the current query
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ShowRunQueryUrlMenuItem_Click(object sender, EventArgs e)
		{
			ShowRunQueryUrl();
		}

		internal void ShowRunQueryUrl()
		{
			if (Query == null || Query.UserObject == null || Query.UserObject.Id <= 0)
			{
				MessageBoxMx.ShowError("The query must first be saved.");
				return;
			}

			string folder = ServicesIniFile.Read("QueryLinksNetworkFolder"); // get unc form of folder
			string fileName = "Run_Query_" + Query.UserObject.Id + ".bat"; // file name
			string uncPath = folder + '\\' + fileName; // unc file to write now and read later

			string tempFile = TempFile.GetTempFileName();
			StreamWriter sw = new StreamWriter(tempFile);
			string cmd = // batch command to start the Mobius client and run the specified query
				Lex.AddDoubleQuotes(ClientDirs.ExecutablePath) + // path to executable in quotes
				" Mobius:Command='Run Query " + Query.UserObject.Id + "'"; // the mobius command to run
			sw.WriteLine(cmd);
			sw.Close();

			ServerFile.CopyToServer(tempFile, uncPath);
			FileUtil.DeleteFile(tempFile);

			string url = "file:///" + folder.Replace('\\', '/') + "/" + fileName; // put in "file:" schema & switch slashes to get URL from UNC name
			InputBoxMx.Show(
				"The following URL can be used from a web page " +
				"to start Mobius and run the current query:",
				"Run Query URL",
				url);
			//"Mobius:Command='Run Query " + Query.UserObject.Id + "'"); // this is better but isn't accepted by SharePoint

			return;
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
			string tableLabel = null;

			int ti = GetSelectedRow();
			if (ti >= 0) tableLabel = (string)DataTable.Rows[ti]["TableNameCol"];
			if (ti < 0 || tableLabel == null)
			{
				MessageBoxMx.ShowError("You must first select the desired table from the table list.");
				return;
			}
			if (!OriginalQueryTables.ContainsKey(tableLabel)) return;
			QueryTable qt = OriginalQueryTables[tableLabel]; // map text label back to query table

			CommandLine.ShowMetaTableXml(qt.MetaTable.Name);
		}

		private void QueryOptionsDialog_Shown(object sender, EventArgs e)
		{
			if (EditAlertOnOpen) EditAlert(); // start with alert edit
		}

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
		private void OkButton_Click(object sender, EventArgs e)
		{
			QueryTable qt;
			QueryColumn qc, qc2;
			MetaTable mt;
			string tName, tableLabel, selectedTableLabel = null, txt;
			bool b1;
			int ti, i1;

// Get summarization & table position tab info

			ti = GetSelectedRow();
			if (ti >= 0) selectedTableLabel = (string)DataTable.Rows[ti]["TableNameCol"];

			List<QueryTable> newTables = new List<QueryTable>(); // build new query table list
			Query.Tables = newTables; // new set of reordered query tables

			for (i1=0; i1<DataTable.Rows.Count; i1++)
			{
				tableLabel = (string)DataTable.Rows[i1]["TableNameCol"];
				if (tableLabel=="") continue;
				qt = OriginalQueryTables[tableLabel]; // map text label back to query table
				if (qt==null) continue; // shouldn't happen

				object cs = DataTable.Rows[i1]["SummarizedCol"];
				bool useSummarized = (cs is bool && (bool)cs == true);

				if ((useSummarized && !qt.MetaTable.UseSummarizedData) || // summary status change?
				(!useSummarized && qt.MetaTable.UseSummarizedData))
				{
					qt = qt.AdjustSummarizationLevel(useSummarized);
				}

				//if (Query.GetQueryTableByName(qt.MetaTable.Name) != null) continue; // don't allow same table twice (comment out, multiples of same table allowed now)

				newTables.Add(qt); // no change to summary info
				qt.Query = Query;
			}

			if (Query.Tables.Count>0 && ti<0) ti=0; // in case nothing selected

// Get basic options

			if (DnfQc.DisplayFormat != ColumnFormatEnum.Unknown) // default number format changed?
			{
				SS.I.DefaultNumberFormat = DnfQc.DisplayFormat;
				Preferences.Set("DefaultNumberFormat",SS.I.DefaultNumberFormat.ToString());
				
				SS.I.DefaultDecimals = DnfQc.Decimals;
				Preferences.Set("DefaultDecimals",SS.I.DefaultDecimals.ToString());
			}

			txt = QnfStatsTextEdit.Text;
			Query.StatDisplayFormat=QbUtil.ConvertStatDisplayFormat(txt);

			if (SS.I.TableColumnZoom != TableColumnZoom.ZoomPct)
			{
				SS.I.TableColumnZoom = TableColumnZoom.ZoomPct;
				Preferences.Set("TableColumnZoom", TableColumnZoom.ZoomPct);
			}

			if (SS.I.GraphicsColumnZoom != GraphicsColumnZoom.ZoomPct)
			{
				SS.I.GraphicsColumnZoom = GraphicsColumnZoom.ZoomPct;
				Preferences.Set("GraphicsColumnZoom", GraphicsColumnZoom.ZoomPct);
			}

// Advanced options - query specific

			Query.DuplicateKeyValues = DuplicateKeyValues.Checked;
			Query.FilterNullRows = FilterNullRows.Checked;
			Query.ShowCondFormatLabels = ShowCondFormatLabels.Checked;
			//Query.BrowseExistingResultsWhenOpened = BrowseExistingResultsWhenOpened.Checked;
			Query.RunQueryWhenOpened = RunQueryWhenOpened.Checked;
			Query.SingleStepExecution = CombineSearchAndRetrieval.Checked;
			//Query.UseCachedData = UseCachedData.Checked;
			Query.Multitable = Multitable.Checked; // (not visible)
            Query.Mobile = MobileQuery.Checked;

            //if (cbDefaultMobileQuery.Checked) Preferences.Set("MobileDefaultQuery",Query.UserObject.Id);

// Advanced options - applies to all queries

			if (RestoreWindowsAtStartup.Checked != SS.I.RestoreWindowsAtStartup)
			{
				SS.I.RestoreWindowsAtStartup = !SS.I.RestoreWindowsAtStartup;
				Preferences.Set("RestoreWindowsAtStartup", SS.I.RestoreWindowsAtStartup);
			}

			if (HilightCorpIdChanges.Checked != SS.I.HilightCidChanges)
			{
				SS.I.HilightCidChanges = !SS.I.HilightCidChanges;
				Preferences.Set("HilightCorpIdChanges", SS.I.HilightCidChanges);
			}

			if (RemoveLeadingZerosFromCids.Checked != SS.I.RemoveLeadingZerosFromCids)
			{
				SS.I.RemoveLeadingZerosFromCids = !SS.I.RemoveLeadingZerosFromCids;
				Preferences.Set("RemoveLeadingZerosFromCids", SS.I.RemoveLeadingZerosFromCids);
			}

			if (RepeatReport.Checked != SS.I.RepeatReport)
			{ 
				SS.I.RepeatReport = !SS.I.RepeatReport;
				Preferences.Set("RepeatReport", SS.I.RepeatReport);
			}

			if (ShowStereoComments.Checked != SS.I.ShowStereoComments)
			{
				SS.I.ShowStereoComments = !SS.I.ShowStereoComments;
				Preferences.Set("ShowStereoComments", SS.I.ShowStereoComments);
			}

			if (BreakHtmlPopupsAtPageWidth.Checked != SS.I.BreakHtmlPopupsAtPageWidth)
			{
				SS.I.BreakHtmlPopupsAtPageWidth = !SS.I.BreakHtmlPopupsAtPageWidth;
				Preferences.Set("BreakHtmlPopupsAtPageWidth", SS.I.BreakHtmlPopupsAtPageWidth);
			}

			if (DefaultToSingleStepQueryExecution.Checked != MqlUtil.DefaultToSingleStepQueryExecution)
			{
				MqlUtil.DefaultToSingleStepQueryExecution = !MqlUtil.DefaultToSingleStepQueryExecution;
				Preferences.Set("DefaultToSingleStepQueryExecution", MqlUtil.DefaultToSingleStepQueryExecution); // set user preference
				QueryEngine.SetParameter("DefaultToSingleStepQueryExecution", MqlUtil.DefaultToSingleStepQueryExecution.ToString()); // set in QE for current session
			}

			//if (AllowNetezzaUse.Checked != SS.I.AllowNetezzaUse)
			//{
			//  SS.I.AllowNetezzaUse = AllowNetezzaUse.Checked;
			//  Preferences.Set("AllowNetezzaUse", SS.I.AllowNetezzaUse);
			//}

			b1 = MarkCheckBoxesInitially.Checked; 
			if (b1 != SS.I.GridMarkCheckBoxesInitially)
			{
				SS.I.GridMarkCheckBoxesInitially = b1;
				Preferences.Set("GridMarkCheckBoxesInitially", b1);
			}

			if (SS.I.EvenRowBackgroundColor != EvenRowBackgroundColor.Color)
			{
				SS.I.EvenRowBackgroundColor = EvenRowBackgroundColor.Color;
				Preferences.Set("EvenRowBackgroundColor", EvenRowBackgroundColor.Color.ToArgb());
			}

			if (SS.I.OddRowBackgroundColor != OddRowBackgroundColor.Color)
			{
				SS.I.OddRowBackgroundColor = OddRowBackgroundColor.Color;
				Preferences.Set("OddRowBackgroundColor", OddRowBackgroundColor.Color.ToArgb());
			}

			QbUtil.RenderQuery(ti);
			Progress.Hide();
			DialogResult = DialogResult.OK;
			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("QueryOptions.htm", "Query Options Help");
		}

		private void GridView_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
		{
			if (e.Info.IsRowIndicator)
			{
				e.Info.DisplayText = (e.RowHandle + 1).ToString();
				e.Info.ImageIndex = -1; // no image
			}
		}

		private void GridView_MouseDown(object sender, MouseEventArgs e)
		{
			GridView view = sender as GridView;
			DownHitInfo = null;

			GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
			if (Control.ModifierKeys != Keys.None)
				return;
			if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.RowHandle != GridControl.NewItemRowHandle)
				DownHitInfo = hitInfo;
		}

		private void GridView_MouseMove(object sender, MouseEventArgs e)
		{
			GridView view = sender as GridView;
			if (e.Button == MouseButtons.Left && DownHitInfo != null)
			{
				Size dragSize = SystemInformation.DragSize;
				Rectangle dragRect = new Rectangle(new Point(DownHitInfo.HitPoint.X - dragSize.Width / 2,
						DownHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

				if (!dragRect.Contains(new Point(e.X, e.Y)))
				{
					view.GridControl.DoDragDrop(DownHitInfo, DragDropEffects.All);
					DownHitInfo = null;
				}
			}
		}

	private void TableGrid_DragDrop(object sender, DragEventArgs e)
		{
			GridControl grid = sender as GridControl;
			GridView view = grid.MainView as GridView;
			GridHitInfo srcHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
			GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
			int sourceRow = srcHitInfo.RowHandle;
			int targetRow = hitInfo.RowHandle;
			MoveRow(sourceRow, targetRow);
		}

		private void TableGrid_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(GridHitInfo)))
			{
				GridHitInfo downHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
				if (downHitInfo == null)
					return;

				GridControl grid = sender as GridControl;
				GridView view = grid.MainView as GridView;
				GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
				if (hitInfo.InRow && hitInfo.RowHandle != downHitInfo.RowHandle && hitInfo.RowHandle != GridControl.NewItemRowHandle)
					e.Effect = DragDropEffects.Move;
				else
					e.Effect = DragDropEffects.None;
			}
		}

/// <summary>
/// Move row to new postion
/// </summary>
/// <param name="sourceRow"></param>
/// <param name="targetRow"></param>

		private void MoveRow(int sourceRow, int targetRow)
		{
			if (sourceRow == targetRow)
				return;

			GridView view = GridView;
			DataRow row1 = view.GetDataRow(targetRow);
			DataRow row2 = view.GetDataRow(targetRow + 1);
			DataRow dragRow = view.GetDataRow(sourceRow);

			DataRow newRow = DataTable.NewRow();
			newRow.ItemArray = dragRow.ItemArray; // copy data

			DataTable.Rows.RemoveAt(sourceRow);

			DataTable.Rows.InsertAt(newRow, targetRow);

			return;
		}

        /// <summary>
        /// The "Default Mobile Query" Checkbox should only be enabled when the "Available As Mobile Query" Checkbox is checked.
        /// If the "Default Mobile Query" is uncheched, then we have to uncheck the "Default Mobile Query" checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobileQuery_CheckedChanged(object sender, EventArgs e)
        {
            cbDefaultMobileQuery.Enabled = MobileQuery.Checked;
            if (!MobileQuery.Checked) cbDefaultMobileQuery.Checked = false;
        }

	}
}