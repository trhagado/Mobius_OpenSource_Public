using Mobius.Data;
using Mobius.ComOps;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

using System;
using System.IO;
using System.Text;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Mobius.ClientComponents
{ 

    /// <summary>
    /// Utility query builder methods
    /// </summary>

    public partial class QbUtil
	{
		public static QueriesControl QueriesControl // control containing queries & results
		{ // get reference to QueriesControl
			get { return QueriesControl.Instance; }
			set { QueriesControl.Instance = value; }
		}
		public static QueriesControl QC { get { return QueriesControl.Instance; } } // alias 

		public static Query Query // current query
		{
			get { return QC != null ? QC.CurrentQuery : null; } 
		    set { QC.CurrentQuery = value; }
		}

		public static QueryManager QueryManager // Query manager associated with current base query  
		{
			get { return Query != null ? Query.QueryManager as QueryManager : null; }
			set { QC.CurrentQuery.QueryManager = value; }
		}

		public static XtraTabControl QueriesControlTabs { get { return QueriesControl != null ? QueriesControl.Tabs : null; } } // tabs in Queries control
		public static XtraTabPage QueriesControlTabPage { get { return QueriesControlTabs != null ? QueriesControlTabs.SelectedTabPage : null; } } // current query tab
		public static int QueriesControlTabIndex { get { return QueriesControlTabs != null ? QueriesControlTabs.SelectedTabPageIndex : -1; } } // current query tab index

		public static List<Document> DocumentList // ordered list of documents, one per tab
		{ // get reference to QueriesControl
			get { return QC.DocumentList; }
			set { QC.DocumentList = value; }
		}

		public static int CurrentQueryIndex // index of current document
		{ // get reference to QueriesControl
			get { return QC.CurrentQueryIndex; }
			set { QC.CurrentQueryIndex = value; }
		}

		public static QbControl QueryBuilderControl
		{ get { return QueriesControl != null ? QueriesControl.QueryBuilderControl : null; } }

		public static QbContentsTree QbContentsCtl
		{ get { return QueryBuilderControl != null ? QueryBuilderControl.QbContentsCtl : null; } }

		public static ContentsTreeControl ContentsTreeCtl
		{ get { return QbContentsCtl != null ? QbContentsCtl.QbContentsTreeCtl : null; } }

		public static QueryResultsControl QueryResultsControl
		{ // get reference to current QueryResultsControl
			get { return QC != null ? QC.QueryResultsControl : null; }
		}

		public static QueryTablesControl QueryTablesControl // current query tables control
		{ // get reference to QueriesControl
			get { return QC.QueryBuilderControl.QueryTablesControl; }
			set { QC.QueryBuilderControl.QueryTablesControl = value; }
		}

		public static XtraTabPage QueryTablesControlTab { get { return QueryTablesControl.Tabs.SelectedTabPage; } } // current query table tab
		public static int QueryTablesControlTabIndex { get { return QueryTablesControl.Tabs.SelectedTabPageIndex; } } // current query table tab index

		public static QueryTable Qt // current query table
		{ // get reference to current query table
			get { return QC.QueryBuilderControl.QueryTablesControl.CurrentQt; }
			set { QC.QueryBuilderControl.QueryTablesControl.CurrentQt = value; }
		}

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		public static Query CurrentSetModeQuery; // query that was the object of the last SetMode
		public static QueryMode CurrentMode = QueryMode.Unknown; // last mode value for last query (build/browse/printPreview etc.)
		static int ShowQuerySqlStatementsCount = 0;
		static bool AskedAboutBrowseExistingResultsWhenOpened = false;

		/// <summary>
		/// Open a new query
		/// </summary>
		/// <returns></returns>

		public static Query NewQuery()
		{
			return QC.NewQuery();
		}

		/// <summary>
		/// Open a new query with specified name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static Query NewQuery(
			string name)
		{
			return QC.NewQuery(name);
		}

		/// <summary>
		/// Add a query
		/// </summary>
		/// <param name="newQuery"></param>

		public static void AddQuery(Query newQuery)
		{
			QC.AddQuery(newQuery);
			return;
		}

/// <summary>
/// Find a query in the open document list by name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static Query FindQueryByName(
			string name)
		{
			Query q = QC.FindQueryByName(name);
			return q;
		}

		/// <summary>
		/// Duplicate query
		/// </summary>

		public static void DuplicateQuery()
		{
			QC.DuplicateQuery();
			return; 
		}

		public static void ShowProjectDescription(
			string projNodeName)
		{
			ProjectDescriptionDialog.ShowProjectDescription(projNodeName);
			return;
		}

		/// <summary>
		/// Show the SQL for the search and retrieval steps of the query
		/// </summary>
		/// <returns></returns>

		public static void ShowQuerySqlStatements()
		{
			Query q = QbUtil.Query;
			ShowQuerySqlStatements(q);
			return;
		}

		/// <summary>
		/// Show the SQL for the search and retrieval steps of the query
		/// </summary>
		/// <returns></returns>

		public static void ShowQuerySqlStatements(Query q)
		{
			try
			{
				string sqlStmts = GetQuerySqlStatements(q);

				ShowQuerySqlStatementsCount++;
				string countString = ShowQuerySqlStatementsCount > 1 ? (" " + ShowQuerySqlStatementsCount) : "";

				string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\QuerySql" + countString + ".txt";
				if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return;

				FileUtil.WriteAndOpenTextDocument(cFile, sqlStmts);
			}

			catch (Exception ex)
			{
				string msg = ex.Message;

				if (!(ex is UserQueryException))
					msg += ",\r\n" + DebugLog.FormatExceptionMessage(ex);

				MessageBoxMx.ShowError(msg);
			}

			return;
		}

/// <summary>
/// Get the SQL statements for a query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public static string GetQuerySqlStatements(Query q)
		{
			q.IncludeRootTableAsNeeded();
			QueryEngine qe = new QueryEngine();
			string sql = qe.BuildSqlStatements(q);
			return sql;
		}
		
		/// <summary>
		/// Show any existing description for a MetaTable
		/// </summary>
		/// <param name="mtName"></param>

		public static void ShowTableDescription(
			string mtName)
		{
			TableDescription td = null;
			string desc = "";

			MetaTable mt = MetaTableCollection.Get(mtName);
			if (mt != null && mt.DescriptionIsAvailable())
			{
				td = MetaTableCollection.GetDescription(mtName);

				if (td != null && td.BinaryDescription != null && td.BinaryDescription.Length > 0)
				{ // binary file
					string fileName = TempFile.CreateValidFileName(mt.Label);
					if (!String.IsNullOrEmpty(td.TypeName)) fileName = fileName + "." + td.TypeName;
					UIMisc.SaveAndOpenBinaryDocument(fileName, td.BinaryDescription);
					UsageDao.LogEvent("ShowTableDescription", mt.Name);
					return;
				}
			}

			if (td != null && !String.IsNullOrEmpty(td.TextDescription))
				desc = td.TextDescription;
			else desc = "Description not available";

			if (!Lex.Contains(desc, "<html"))
			{ // if not html then convert new lines to html breaks
				desc = desc.Replace("\r\n", "<br>");
				desc = desc.Replace("\r", "<br>");
				desc = desc.Replace("\n", "<br>");
			}
			UIMisc.ShowHtmlPopupFormDocument(desc, mt.Label);
			UsageDao.LogEvent("ShowTableDescription", mt.Name);

			return;
		}

/// <summary>
/// Define a new method for handling ContentsTree item operations, saving the current method
/// </summary>
/// <param name="newMethod"></param>

		public static void SetProcessTreeItemOperationMethod(TreeItemOperationDelegate newMethod)
		{
			AssertMx.IsNull(SavedProcessTreeItemOperationMethod, "SavedProcessTreeItemMethod");

			SavedProcessTreeItemOperationMethod = ProcessTreeItemOperationMethod; // save current method
			ProcessTreeItemOperationMethod = newMethod;
			return;
		}

		/// <summary>
		/// Restore previous ProcessTreeItemOperationMethod
		/// </summary>

		public static void RestoreProcessTreeItemOperationMethod()
		{
			AssertMx.IsNotNull(SavedProcessTreeItemOperationMethod, "SavedProcessTreeItemMethod");

			ProcessTreeItemOperationMethod = SavedProcessTreeItemOperationMethod;
			SavedProcessTreeItemOperationMethod = null;
			return;
		}

		/// <summary>
		/// Call the current ProcessTreeItemOperationMethod
		/// </summary>
		/// <param name="op"></param>
		/// <param name="args"></param>

		public static void CallCurrentProcessTreeItemOperationMethod(
			string op,
			string args)
		{
			AssertMx.IsNotNull(ProcessTreeItemOperationMethod, "ProcessTreeItemOperationMethod");

			ProcessTreeItemOperationMethod(op, args);
			return;
		}

		public delegate void TreeItemOperationDelegate(string op, string args);
		static TreeItemOperationDelegate ProcessTreeItemOperationMethod = ProcessTreeItemOperation; // current method to call to process tree item operation
		static TreeItemOperationDelegate SavedProcessTreeItemOperationMethod = null; // saved method to call to process tree item operation

		/// <summary>
		/// Pass the tree operation to the proper handler
		/// </summary>
		/// <param name="op"></param>
		/// <param name="args"></param>

		public static void ProcessTreeItemOperation(
			string op,
			string args)
		{
			CommandExec.ExecuteCommandAsynch(op + " " + args);
		}

		/// <summary>
		/// Add a new MetaTable to the query (if not already added) and render it
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static QueryTable AddAndRenderTable(
			MetaTable mt)
		{
			return AddAndRenderTables(mt.Name);
		}

		/// <summary>
		/// Add table(s), select & render it
		/// </summary>
		/// <param name="tableString"></param>
		/// <returns></returns>

		public static QueryTable AddAndRenderTables(
			string tableString)
		{
			QueryTable Qt = AddTablesToQuery(tableString);
			if (Qt != null)
			{
				SelectQueryTableTab(Qt); // make assoc item current
				RenderQueryTable(Qt);
			}
			return Qt;
		}

		/// <summary>
		/// // Add one or more tables to the query
		/// </summary>
		/// <param name="tables"></param>
		/// <returns></returns>

		public static QueryTable AddTablesToQuery(
			string tableString)
		{
			QueryTable qt = null, qt2, matchingQt;
			QueryColumn qc, qc2;
			MetaTable mt, mt2;
			MetaColumn mc;
			string tName, tok, txt;
			ArrayList flist;
			int tcnt, fcnt, fi, i1, i2;

			Lex lex = new Lex();
			lex.SetDelimiters("( ) ,");
			lex.OpenString(tableString);

			tcnt = 0;

			while (true)
			{
				tName = lex.Get(); // get table name
				if (tName == ",") continue;
				if (tName == "") break;

				fcnt = 0; // get field list
				flist = new ArrayList();
				tok = lex.Get();
				if (tok != "(")
				{ // field list?
					if (tok != "") lex.Backup();
				}
				else
					while (true)
					{
						tok = lex.Get();
						if (tok == ")") break;
						if (tok == ",") continue;
						flist.Add(tok);
						fcnt++;
					}

				mt = GetMetaTable(tName);
				if (mt == null)
				{
					MessageBoxMx.ShowError(
						"Unable to find data table: " + tName + "\r\n\r\n" +
						"The table may not exist or you may not be authorized for access.");
					continue;
				}
				tName = mt.Name; // copy back in case name in contents wasn't correct

				tcnt++;

				if (!TableShouldBeAdded(Query, mt, out matchingQt))
				{
					qt = matchingQt; // show this if no others added
					continue;
				}

// See if this table is compatible with the others in the query

				try { MqlUtil.CheckTableCompatibility(Query, mt); } // check for compatibility of tables in query	
				catch (Exception ex)
				{
					string msg =
						ex.Message + "\n\n" +
						"Do you want to add this table to the query anyway?";

					DialogResult dr = MessageBoxMx.Show(msg, UmlautMobius.String, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					if (dr == DialogResult.No) return null;
				}

// Add tab for table

				if (Query.Tables.Count == 0 && QueryTablesControl.ShowCriteriaTab)
					AddCriteriaTab(); // insert initial criteria tab?

				qt = new QueryTable(mt); // create the query table 
				qt.Query = Query;

				qt.AssignUniqueQueryTableLabel(); // give it a unique label

				if (Query.Tables.Count >= 1 && mt.IsRootTable)
					Query.InsertQueryTable(0, qt); // insert root in first position

				else Query.AddQueryTable(qt); // add to end if not root table

				MetaColumn partitionMc = mt.DatabaseListMetaColumn;
				if (partitionMc != null) // if this is a partitioned table include this section
				{
					qc = qt.GetQueryColumnByName(partitionMc.Name);
					Query.AssureDbInSet(mt, qc);
				}

				mt = qt.MetaTable;

				if ((mt.IsAnnotationTable || mt.IsUserDatabaseStructureTable) && // if user table with undefined header color
					qt.HeaderBackgroundColor.Equals(Color.Empty)) // then use default for annotation tables
					qt.HeaderBackgroundColor = Color.FromArgb(255, 255, 128); // default color for annotation tables (light yellow)

				AddQueryTableTab(qt); // add it to the UI

				if (fcnt > 0) // field list included?
				{
					for (fi = 0; fi < qt.QueryColumns.Count; fi++) // set field selections
					{
						qc = qt.QueryColumns[fi];
						qc.Selected = false;
						qc.Aggregation = null;
						mc = qc.MetaColumn; // mc = (MetaColumn)mt.MetaColumns[qc.MetaColumnIdx]; 
						if (mc != null)
						{
							for (i1 = 0; i1 < fcnt; i1++)
							{
								if (Lex.Eq(mc.Name, (string)flist[i1]))
								{
									qc.Selected = false;
									break;
								}
							}
						}
					}
				}

			} // end of main loop

			if (tcnt == 0) return null;
			else return qt; // return last query table
		}

/// <summary>
/// Return true if the table is new to the query or the user allows duplicates
/// </summary>
/// <param name="q"></param>
/// <param name="mt"></param>
/// <param name="matchingQt"></param>
/// <returns></returns>

		public static bool TableShouldBeAdded(
			Query q,
			MetaTable mt,
			out QueryTable matchingQt)
		{
			MetaTable mt2;
			QueryTable qt2;
			QueryColumn qc2;
			string msg;
			int i1;

			matchingQt = null;
			MetaColumn partitionMc = mt.DatabaseListMetaColumn;

			for (i1 = 0; i1 < Query.Tables.Count; i1++)
			{
				qt2 = Query.Tables[i1];
				mt2 = qt2.MetaTable;
				if (Lex.Eq(mt2.Name, mt.Name)) // same table?
				{
					matchingQt = qt2;
					break;
				}

				MetaColumn partitionMc2 = mt2.DatabaseListMetaColumn; // see if this is part of a partitioned root table
				if (partitionMc == null || partitionMc2 == null) continue;
				if (partitionMc.Dictionary != partitionMc2.Dictionary) continue;

				qc2 = // this query column has criteria listing partitions to be included
					qt2.GetQueryColumnByName(partitionMc2.Name);
				if (qc2 == null) continue;
				Query.AssureDbInSet(mt, qc2);
				matchingQt = qt2;
				return false; // included in db set, no need to add table again
			}

			if (i1 < Query.Tables.Count) // find it?
			{
				if (mt.IsRootTable)
				{
					msg =
						"The \"" + mt.Label + "\" table\r\n" +
						"is already contained in the current query.\r\n" +
						"Since it is a \"Root Table\" it can't be added to the query an additional time.";
					MessageBoxMx.Show(msg, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}

				msg =
					"The \"" + mt.Label + "\" table\r\n" +
					"is already contained in the current query.\r\n" +
					"Do you want to add this table to the query an additional time?";
				DialogResult dr = MessageBoxMx.Show(msg, UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes)
				{ // don't add but show the existing instance of the table

					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Select an item in the querybuilder table list
		/// </summary>
		/// <param name="qt"></param>

		static void SelectQueryTableTab(
			QueryTable qt)
		{
			QueryTablesControl.SelectQueryTableTab(qt);
		}


		/// <summary>
		/// Select an item in the querybuilder table list
		/// </summary>
		/// <param name="ti">Index of item to show</param>

		static void SelectQueryTableTab(
			int ti)
		{
			QueryTablesControl.SelectQueryTableTab(ti);
		}

		/// <summary>
		/// Find or allocate selected table structure for specified table
		/// </summary>
		/// <param name="tname"></param>
		/// <returns></returns>

		public static QueryTable GetQueryTable(
			string tname)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			int fi, i1;

			// See if already exists

			for (i1 = 0; i1 < Query.Tables.Count; i1++)
			{
				qt = Query.Tables[i1];
				mt = qt.MetaTable;
				if (Lex.Eq(mt.Name, tname)) return qt;
			}

			mt = GetMetaTable(tname);
			if (mt == null) throw new Exception("No metatable  for " + tname);

			// New table

			qt = new QueryTable(Query, mt);
			return qt;
		}

		/// <summary>
		/// Add a new tab to the UI tab control for a query table
		/// </summary>
		/// <param name="qt"></param>

		internal static void AddQueryTableTab(
			QueryTable qt)
		{
			QueryTablesControl.AddQueryTableTab(qt);
		}

		/// <summary>
		/// Add criteria tab if needed
		/// </summary>
		/// <param name="query"></param>
		static void AddCriteriaTab()
		{
			QueryTablesControl.AddCriteriaTab();
		}

/// <summary>
/// Duplicate a query table already in the query and add the duplicate to the right of the existing table
/// </summary>
/// <param name="qt"></param>

		internal static void DuplicateQueryTable(
			QueryTable qt)
		{
			int ti = Query.GetQueryTableIndex(qt); // index of table to duplicate
			if (ti < 0) return;
			ti++; // where duplicate table will go

			QueryTable qt2 = qt.Clone(); // make copy that we can modify (i.e. qt2.Query modified when added to q2)
			qt2.Alias = "";
			qt2.AssignUniqueQueryTableLabel();
			if (ti > Query.Tables.Count) throw new Exception("ti > Query.Tables.Count");
			Query.InsertQueryTable(ti, qt2); // insert after current table
			QueryTablesControl.InsertQueryTableTab(qt2);

			Qt = qt2; // make it the current QueryTable
			QueryTablesControl.SelectQueryTableTab(Qt);
			QueryTablesControl.RenderQueryTable(Qt);

			return;
		}

		/// <summary>
		/// Remove a query table
		/// </summary>
		/// <param name="qti"></param>

		internal static void RemoveQueryTable(int ti)
		{
			if (ti < 0 || ti >= Query.Tables.Count) return; // be sure in range 

			Query.RemoveQueryTableAt(ti); // remove from query 

			int tabi = ti;
			if (QueryTablesControl.ShowCriteriaTab) tabi++;
			QueryTablesControl.RemoveTab(tabi);

			if (ti == Query.Tables.Count) // removed end table
				ti--;

			if (ti >= 0)
			{
				Qt = Query.Tables[ti];
				QueryTablesControl.SelectQueryTableTab(Qt);
				QueryTablesControl.RenderQueryTable(Qt);
			}
			else // no tables in query
			{
				if (QueryTablesControl.ShowCriteriaTab) QueryTablesControl.RemoveTab(0); // remove any criteria tab
				QueryTablesControl.Render();
				Query.KeyCriteria = Query.KeyCriteriaDisplay = ""; // clear key criteria if no tables
			}

			return;
		}

/// <summary>
/// Enter query building mode for the current query
/// </summary>

		public static void EditQuery()
		{
			SetMode(QueryMode.Build, QbUtil.CurrentSetModeQuery);
			SessionManager.ActivateShell();
			RenderQuery();
			QueriesControl.Focus();
			return;
		}

/// <summary>
/// Switch current query to build mode & render
/// </summary>
		static void RenderQueryInBuildMode()
		{
			int ti;
			SetMode(QueryMode.Build); // set mode & UI button
			if (QueryTablesControl.ShowCriteriaTab) ti = -1; // show criteria tab?
			else ti = 0;
			RenderQuery(ti);
		}

		/// <summary>
		/// Render the criteria tab
		/// </summary>
		/// <param name="query"></param>

		static void RenderCriteriaTab(
			Query query)
		{
			QueryTablesControl.RenderCriteriaTab();
		}

		/// <summary>
		/// Render query and make 1st table current
		/// </summary>
		/// <param name="ti"></param>

		public static void RenderQuery()
		{
			QueryTable qt = null;
			if (QueryTablesControl.Query != null)
				qt = QueryTablesControl.Query.CurrentTable;

			QueryTablesControl.Render(qt);
		}

		/// <summary>
		/// Render query and set current table tab
		/// </summary>
		/// <param name="ti"></param>

		public static void RenderQuery(
			int ti)
		{
			QueryTablesControl.Render(ti);
		}

		/// <summary>
		/// Render query and set current table tab
		/// </summary>
		/// <param name="qt"></param>

		public static void RenderQuery(
			QueryTable qt)
		{
			QueryTablesControl.Render(qt);
		}

		/// <summary>
		/// Display query table in query grid
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static void RenderQueryTable(
			QueryTable qt)
		{
			QueryTablesControl.Render(qt);
		}

		/// <summary>
		/// Build a query to select just the structure for a compound number
		/// </summary>
		/// <param name="cn"></param>
		/// <returns></returns>

		public static Query GetSelectStructureQuery(
			MetaTable keyMt,
			string cn)
		{
			Query q = new Query();
			MetaTable mt;
			MetaColumn keyMc, strMc;
			QueryTable qt;
			QueryColumn qc;

			if (keyMt != null && keyMt.Root.IsUserDatabaseStructureTable) // if root metatable is user database then normalize based on key
			{
				keyMt = keyMt.Root; // be sure we have root
				cn = CompoundId.Normalize(cn, keyMt);
			}

			else
			{
				cn = CompoundId.Normalize(cn);
				keyMt = CompoundId.GetRootMetaTableFromCid(cn, keyMt);
			}

			if (keyMt == null) return null;

			keyMc = keyMt.KeyMetaColumn;
			if (keyMc == null) return null;
			strMc = keyMt.FirstStructureMetaColumn;
			if (strMc == null) return null;

			qt = new QueryTable(q, keyMt);
			qc = qt.AddQueryColumnByName(keyMc.Name, true);
			if (qc == null) return null;
			qc = qt.AddQueryColumnByName(strMc.Name, true);
			if (qc == null) return null;
			q.KeyCriteria = " = " + cn;
			return q;
		}

		/// <summary>
		/// Return the root metatable for the current query (may not be part of query)
		/// </summary>
		/// <returns></returns>

		public static MetaTable CurrentQueryRoot
		{
			get
			{
				if (Query == null || Query.Tables.Count == 0) return null;
				MetaTable mt = QueryEngine.GetRootTable(Query);
				if (mt != null) mt = mt.Root;
				return mt;
			}
		}

		/// <summary>
		/// Run query & popup in browser window
		/// </summary>
		/// <param name="q"></param>
		/// <param name="title"></param>
		/// <returns></returns>

		public static bool RunPopupQuery(
			Query q,
			string title,
      bool suppressNoDataMessage = false)
		{
			return RunPopupQuery(q, title, OutputDest.Html, null, suppressNoDataMessage);
		}

		/// <summary>
		/// Run query & popup in separate window
		/// </summary>
		/// <param name="q"></param>
		/// <param name="title"></param>
		/// <param name="outputDest"></param>
		/// <returns></returns>

		public static bool RunPopupQuery(
			Query q,
			string title,
			OutputDest outputDest,
			ExitingQueryResultsDelegate exitingQueryResultsCallBack = null,
			bool suppressNoDataMessage = false)
		{
			ResultsViewType defaultViewType = ResultsViewType.Table; // default view type
			string response = null;
			string userObjectName = q.UserObject.Name;
			q.UserObject.Name = title; // store title in query

			if (outputDest == OutputDest.WinForms)
			{
				defaultViewType = ResultsViewType.Table;
			}

			else if (outputDest == OutputDest.Html) // create HTML view
				defaultViewType = ResultsViewType.HtmlTable;

			q.SetupQueryPagesAndViews(defaultViewType); // be sure we have default page

			try // run the query
			{
				response = QueryExec.RunQuery(q, outputDest, OutputFormContext.Popup, exitingQueryResultsCallBack, suppressNoDataMessage: suppressNoDataMessage);
			}

			catch (Exception ex)
			{
				response = ex.Message;
			} // return any exception as error

			q.UserObject.Name = userObjectName;

			if (response != null && response.Trim() != "" &&
				!response.ToLower().StartsWith("command "))
			{
        if (!suppressNoDataMessage)
        {
            MessageBoxMx.ShowError("Error running popup query: " + response);
            ServicesLog.Message("RunPopupQuery Error: " + response);
        }
				return false;
			}

			Progress.Hide();
			return true;
		}

		/// <summary>
		/// Open a query
		/// </summary>
		/// <returns></returns>

		public static Query OpenQueryDialog()
		{
			string prompt = "Open Query";
			String internalName = "";
			int i1;

			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.Query, prompt);
			if (uo == null) return null;
			internalName = uo.InternalName;

			Progress.Show("Opening Query:\r\n" + uo.Name, UmlautMobius.String, false);
			Query query = ReadQuery(internalName);
			Progress.Hide();

			return query;
		}

		/// <summary>
		/// Read a query given an object id
		/// </summary>
		/// <param name="queryId"></param>
		/// <returns></returns>

		public static Query ReadQuery(
			int queryId)
		{
			UserObject quo = QueryReader.ReadQueryWithMetaTables(queryId);
			return FinishReadQuery(quo);
		}

		/// <summary>
		/// Read a query given a name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static Query ReadQuery(
			string name)
		{
			string queryName, fileName, content = "", txt;

			if (SS.I.ConnectedToServices)
			{
				UserObject quo = UserObjectUtil.ParseInternalUserObjectName(name);
				quo.Type = UserObjectType.Query;
				quo = QueryReader.ReadQueryWithMetaTables(quo);
				return FinishReadQuery(quo);
			}

			else // read from file
			{
				StreamReader sr = new StreamReader(name);
				content = sr.ReadToEnd();
				sr.Close();
				UserObject quo = new UserObject(UserObjectType.Query);
				quo.Name = Path.GetFileName(name);
				quo.Content = content;
				return FinishReadQuery(quo);
			}
		}

		/// <summary>
		/// Finish up read of query
		/// </summary>
		/// <param name="quo"></param>
		/// <returns></returns>

		public static Query FinishReadQuery(
			UserObject quo)
		{
			Query q, q2;
			if (quo == null) return null;

			q = Query.Deserialize(quo.Content);
			q.UserObject = quo.Clone(); // clone user object to get current name, etc.

			q.AssignUndefinedAliases(); // be sure aliases are up to date
			q.UserObject.Content = SerializeForChangeDetection(q); // save in current format for better change detection
			q.AlertQueryState = Alert.GetAlertQueryCriteria(q); // set state info needed to determine if alert needs resetting
			return q;
		}

		/// <summary>
		/// Open a query and render it
		/// </summary>
		/// <param name="queryName"></param>
		/// <returns></returns>

		public static string OpenQuery(
			string queryName)
		{
			return OpenQuery(queryName, true, true);
		}

		/// <summary>
		/// Open a query and render it optionally allowing browsing
		/// </summary>
		/// <param name="queryName"></param>
		/// <returns></returns>

		public static string OpenQuery(
			string queryName,
			bool showProgressDialog,
			bool allowBrowse,
			bool updateMruList = true)
		{
			Query query = null;
			int ti;
			string nextCommand = "";
			UserObject uo;
			Document mtw, newMtw;

			if (queryName != null && queryName != "") // already have query name
			{
				try
				{
					SessionManager.DisplayStatusMessage("Opening Query...");
					if (showProgressDialog)
						Progress.Show("Opening Query...", UmlautMobius.String, false);

						UserObject quo = UserObjectUtil.ParseInternalUserObjectName(queryName);
						quo.Type = UserObjectType.Query;
						uo = UserObjectDao.ReadHeader(quo);
						if (uo == null) throw new Exception("Query not found");

					if (showProgressDialog)
						Progress.Show("Opening Query:\r\n" + uo.Name, UmlautMobius.String, false);

					query = ReadQuery(queryName);
					if (query == null) throw new Exception(CommandExec.GetUserObjectReadAccessErrorMessage(uo.Id, "query"));

					SessionManager.DisplayStatusMessage("");
					if (showProgressDialog) Progress.Hide();

				}
				catch (Exception ex)
				{
					string name = queryName;
					if (query != null) name = query.Name + " (" + queryName + ")";
					MessageBoxMx.ShowError("Unable to open query: " + name + "\r\n" + ex.Message);
					return "";
				}
			}

			else // prompt user for query
			{
				query = OpenQueryDialog();
				if (query == null) return ""; // cancelled
			}

			if (SS.I.QueryTestMode) { } // don't check for or display error if test mode

			else
			{
				if (Lex.IsDefined(query.WarningMessage))
				{
					MessageBoxMx.Show(query.WarningMessage, "Query Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					query.WarningMessage = "";
				}

				if (query.InaccessableData != null)
				{
					ShowUnavailableDataMessage(query);
					query.InaccessableData = null;
				}

				if (query.Tables == null || query.Tables.Count == 0)
					MessageBoxMx.ShowError("The query contains no query tables");
			}

			query.UseResultKeys = false; // be sure this is off when query is opened

			nextCommand = AddQueryAndRender(query, allowBrowse);

			if (updateMruList && query != null && query.UserObject.Id > 0)
			{ // add to mru list
				string internalName = "QUERY_" + query.UserObject.Id;
				MainMenuControl.UpdateMruList(internalName);
			}

			return nextCommand;
		}

/// <summary>
/// Show message on unavailable tables
/// </summary>
/// <param name="query"></param>

		public static void ShowUnavailableDataMessage(Query query)
		{

			if (query.InaccessableData == null || query.InaccessableData.Count == 0) return;

			string message =
				"Data on the following query tables could not be retrieved because of a\n" +
				"system problem and the tables have been removed from the query:\n\n";

			foreach (string schema in query.InaccessableData.Keys)
			{
				foreach (string mtName in query.InaccessableData[schema]) // list each table in source
				{
					MetaTable mt = MetaTableCollection.Get(mtName);
					if (mt != null) message += mt.Label + "\n" + schema + "\n\n";
					else message += mtName + "\n" + schema + "\n\n";
				}
			}

			MessageBoxMx.ShowError(message);
		}

		/// <summary>
		/// Add the supplied query object to the list of open queries & render appropriately
		/// </summary>
		/// <param name="query"></param>
		/// <param name="allowBrowse"></param>

		public static string AddQueryAndRender(
			Query query,
			bool allowBrowse)
		{
			int ti, i1;
			Query q2 = null;
			Document mtw, newMtw;
			string nextCommand = "";

			i1 = GetQueryIndexIndex(query);

			if (i1 >= 0) // already open, just switch to it
			{
				QC.SelectQuery(i1);
				return "";
			}

			// New query, add it to the query list, make current & render it

			QC.AddQuery(query);

			if (allowBrowse) try
				{
					if (Query.RunQueryWhenOpened) // run immediately
					{
						Query.ResultKeys = null; // force rerun of query
						nextCommand = QueryExec.RunQuery(Query, OutputDest.WinForms);
						return nextCommand;
					}

					else if (ShouldViewSavedResults(Query))
					{ // read in full dataset file & browse
						string fileName = query.ResultsDataTableFileName;
						if (Lex.IsNullOrEmpty(fileName)) fileName = "Query_" + Query.UserObject.Id + "_Results.bin";
						if (DataTableManager.LoadDataTableFromFile(Query, fileName)) // read the data
						{
							nextCommand = QueryExec.RunQuery(Query, OutputDest.WinForms, OutputFormContext.Session, null, browseExistingResults: true);
							return nextCommand;
						}
					}

					else if (Query.SerializeResultsWithQuery && Query.ResultsDataTable != null)
					{ // browse results that were serialized with the query
						nextCommand = QueryExec.RunQuery(Query, OutputDest.WinForms, OutputFormContext.Session, browseExistingResults: true);
						return nextCommand;
					}

					else if (Query.BrowseSavedResultsUponOpen && Query.Mode == QueryMode.Browse &&
						Query.ResultKeys != null)
					{ // browse results associated with ResultsKeys list
						Query.UseResultKeys = true;
						nextCommand = QueryExec.RunQuery(Query, OutputDest.WinForms, OutputFormContext.Session, browseExistingResults: false);
						return nextCommand;
					}
				}

				catch (Exception ex)
				{ ex = ex; }  // if exception occurs ignore it & open query in normal query builder mode

			RenderQueryInBuildMode();
			return "";
		}

/// <summary>
/// Return true if query exports a full native Mobius dataset that should be viewed
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public static bool ShouldViewSavedResults(Query q)
		{
			if (!Query.BrowseSavedResultsUponOpen) return false;
			Alert a = Alert.GetAlertByQueryId(q.UserObject.Id); // make sure alert is still there & set for full mobius-format data export
			if (a != null && a.ExportParms != null && a.ExportParms.OutputDestination == OutputDest.WinForms)
				return true;

			else if (!Lex.IsNullOrEmpty(q.ResultsDataTableFileName)) // this is set for background queries to support display of results
			{
				string clientFileName;
				DateTime clientFileDate;

				if (!DataTableManager.GetSavedDataTableFile(q.ResultsDataTableFileName, out clientFileName, out clientFileDate))
					return false;

				int daysOld = (int)DateTime.Now.Subtract(clientFileDate).TotalDays;
				if (daysOld > 7)
				{
					string msg = @"
A saved set of results for this query available; however, the
results are " + daysOld  + @" days old. Do you want to view this data?

If you anwser no, the query will be opened normally so that it can be rerun
to retrieve the latest data.
Also note that this message can be eliminated if the query owner refreshes
the saved results or re-saves the query without requesting saved results.";
					DialogResult dr = MessageBoxMx.Show(msg, UmlautMobius.String, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (dr != DialogResult.Yes) return false;
				}

				return true; // and when a user has saved a query when displaying results and asked for output data to be saved for later opening
			}

			else return false;
		}

		/// <summary>
		/// Get index of any open window with same name as supplied query
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static int GetQueryIndexIndex(
			Query q)
		{
			return GetQueryIndex(q.UserObject.Name);
		}

		/// <summary>
		/// Get index of any open window with supplied name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static int GetQueryIndex(
			string name)
		{
			int i1;
			for (i1 = 0; i1 < DocumentList.Count; i1++) // see if already open
			{
				Document mtw = DocumentList[i1];
				if (mtw.Type != DocumentType.Query) continue;
				Query q2 = (Query)mtw.Content;
				if (Lex.Eq(q2.UserObject.Name, name)) break;
			}

			if (i1 < DocumentList.Count) return i1;
			else return -1; // not found
		}

		/// <summary>
		/// Switch to the specified document window
		/// </summary>
		/// <param name="wi"></param>

		public static void SelectQuery(int wi)
		{
			SessionManager.Instance.QueriesControl.SelectQuery(wi);
		}

		/// <summary>
		/// UI code for saving query
		/// </summary>

		public static bool SaveQueryDialog(
			Query query)
		{
			UserObject uo = null, uo2 = null;
			String tok, txt, name, description, shareWith;

			if (query.Tables.Count == 0)
			{
				MessageBoxMx.Show("There are no tables in the current query", UmlautMobius.String,
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}

			if (!Alert.QuerySaveOkForAlertResults(query)) return false;

			if (SS.I.StandAlone) return SaveQueryToFileDialog(query);

		GetResponse:
			uo = UserObjectSaveDialog.Show("Save Query", query.UserObject);
			if (uo == null) return false;

			uo2 = UserObjectDao.ReadHeader(uo);
			if (uo2 != null)
			{
				if (uo.AccessLevel == UserObjectAccess.Private && uo2.AccessLevel == UserObjectAccess.Public)
				{ // going to private from public?
					DialogResult dr =
						MessageBoxMx.Show("This query is currently public.\n" +
						"Do you want to continue keep it public?",
						UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

					if (dr == DialogResult.Cancel) goto GetResponse;
					else if (dr == DialogResult.Yes) // keep public
					{
						uo.AccessLevel = uo2.AccessLevel;
						uo.ParentFolder = uo2.ParentFolder;
						uo.ParentFolderType = uo2.ParentFolderType;
					}
				}
			}

			query.UserObject = uo; // new user object info

			SessionManager.DisplayStatusMessage("Saving Query...");
			bool success = SaveQuery(query);
			SessionManager.DisplayStatusMessage("");

			if (success)
			{
				SessionManager.SetShellTitle(query.UserObject.Name);
				QueriesControl.SetQueryTabTitle(query.UserObject.Name);

				//updateQueryMru(query.UserObject.Name); // update the most-recently-used list
				//updateSearchPopupMenu(); // update mru in popup
				//MessageBoxMx.Show("Query " + query.UserObject.Name + " has been saved",MobiusString.Value,
				//MessageBoxButtons.OK,MessageBoxIcon.Information);
				return true;
			}

			else
			{
				MessageBoxMx.Show("Query " + query.UserObject.Name + " could not be saved", UmlautMobius.String,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}

		/// <summary>
		/// Save query to database
		/// </summary>
		/// <returns></returns>

		public static bool SaveQuery(
			Query query)
		{
			DialogResult dr;

			query.AssignUndefinedAliases(); // be sure aliases are complete

			if (!UserObjectUtil.UserHasWriteAccess(query.UserObject))
			{ // is the user authorized to save this?
				MessageBoxMx.ShowError("You are not authorized to save this query");
				return false;
			}

			if (!Alert.QuerySaveOkForAlertResults(query)) return false;

			UserObject uo = query.UserObject;

      if (uo.Id <= 0 && uo.AccessLevel == UserObjectAccess.None) uo.AccessLevel = UserObjectAccess.Private;

      // If saving from browse mode then save browse context with query

      if (query.Mode == QueryMode.Browse && query.QueryManager != null && // if browsing
			  QC.QueryResultsControl != null) // and defined query results control
			{
				query.InitialBrowsePage = // save current page as initial browse page
					QueryResultsControl.CurrentPageIndex;

				////if (!query.BrowseSavedResultsUponOpen) && !AskedAboutBrowseExistingResultsWhenOpened)
				////{

				//  dr = MessageBoxMx.Show( // always ask if user wants to do this
				//   "Do you want to save these query results and allow them to be viewed the next time this query is opened?",
				//    UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				//  if (dr == DialogResult.Yes)
				//    query.BrowseSavedResultsUponOpen = true;
				//  else if (dr == DialogResult.No)
				//    query.BrowseSavedResultsUponOpen = false;
				//  else return false; // cancel the save

				////  AskedAboutBrowseExistingResultsWhenOpened = true;
				////}

				query.BrowseSavedResultsUponOpen = false; // turn off saving results with query for now (still used by alerts & background queries)

				if (query.BrowseSavedResultsUponOpen)
				{ // set the proper file name before the save
					if (uo.Id <= 0) uo.Id = UserObjectDao.GetNextId();
					query.ResultsDataTableFileName = "Query_" + uo.Id + "_Results.bin";
				}
			}

			else // in build mode
			{
				query.ResultsDataTableFileName = ""; // clear any results reference since may be out of sync with query if modified
				// Don't clear BrowseSavedResultsUponOpen since this may be set as the result of an alert that writes the native results
			}

// Write the query to the database

			if (Lex.IsNullOrEmpty(uo.Owner)) // assign us as owner if not already defined
				uo.Owner = SS.I.UserName;

			UserObjectTree.GetValidUserObjectTypeFolder(uo);

			//if (uo.ParentFolder != "FOLDER_262028") uo = uo; // debug

			uo.Count = query.Tables.Count;
			uo.Content = query.Serialize();

      UserObjectDao.Write(uo);

			uo.Content = SerializeForChangeDetection(query); // reserialize in form for later change detection

			string internalName = "QUERY_" + uo.Id;
			MainMenuControl.UpdateMruList(internalName);

			MetaTableCollection.Remove("multitable_" + uo.Id.ToString()); // remove any associated multitable metatable so we can get new one

			if (Query.BrowseSavedResultsUponOpen && !Lex.IsNullOrEmpty(query.ResultsDataTableFileName))
			{ // start background query to create the results file to be displayed the next time the query is opened
				BackgroundQuery.SpawnBackgroundQuery(query.UserObject.Id, false);
			}

			return true;
		}

		/// <summary>
		/// Save the query to a file
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static bool SaveQueryToFileDialog(
			Query query)
		{
			string filter = "Queries (*.qry)|*.qry|All files (*.*)|*.*";
			string fileName = UIMisc.GetSaveAsFilename("Save Query", query.UserObject.Name, filter, "QRY");
			if (String.IsNullOrEmpty(fileName)) return false;

			string content = query.Serialize();

			StreamWriter sw = new StreamWriter(fileName);
			sw.Write(content);
			sw.Close();
			return true;
		}

		/// <summary>
		/// Serialize query in normalized to build mode for change detection.
		/// This avoids prompts for changed queries due to differences only in mode.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static string SerializeForChangeDetection(
			Query q)
		{
			q.UpdateAllKeySortOrders(); // be sure sort order is up to date for all keys

			ResultsPages saveCp = q.ResultsPages; // ignore charts for now
			q.ResultsPages = null;

			QueryMode saveQm = q.Mode;
			q.Mode = QueryMode.Build;

			bool saveSmt = q.SerializeMetaTablesWithQuery;
			q.SerializeMetaTablesWithQuery = false;

			bool saveSr = q.SerializeResultsWithQuery;
			q.SerializeResultsWithQuery = false;

			q.ResetVoPositions();

			string contents = q.Serialize();

			//XmlDocument doc = new XmlDocument();
			//doc.LoadXml(contents);
			//XmlNodeList nodes = doc.SelectNodes("//QueryTable");
			//foreach (XmlNode node in nodes)
			//{
			//  XmlAttribute xmlAttribute = node.Attributes["VoPosition"];
			//  if (xmlAttribute != null) node.Attributes.Remove(xmlAttribute);
			//}
			//contents = doc.OuterXml;

			q.Mode = saveQm;
			q.ResultsPages = saveCp;
			q.SerializeMetaTablesWithQuery = saveSmt;
			q.SerializeResultsWithQuery = saveSr;

			return contents;
		}

		/// <summary>
		/// Set the current query to a new query object
		/// </summary>
		/// <param name="q"></param>

		public static void SetCurrentQueryInstance(
			Query q)
		{
			QC.SetCurrentQueryInstance(q);
		}

		/// <summary>
		/// Save file prompting for name if not yet saved
		/// </summary>
		/// <returns></returns>

		public static bool SaveFile()
		{
			if (DocumentList.Count == 0 || CurrentQueryIndex < 0) return false;
			string tok = Query.UserObject.Name.ToLower();
			if (tok != "current" && Query.UserObject.Owner != "")
			{
				SessionManager.DisplayStatusMessage("Saving Query...");
				QbUtil.SaveQuery(Query);
				SessionManager.DisplayStatusMessage("");
				return true;
			}
			return QbUtil.SaveQueryDialog(Query);
		}

		/// <summary>
		/// Close all files
		/// </summary>
		/// <returns>True if all closed, false if cancelled</returns>

		public static bool CloseFileAll()
		{
			if (DocumentList.Count == 0 || CurrentQueryIndex < 0) return true; // nothing to close

			if (CurrentQueryIndex >= 0) SelectQueryTableTab(0); // move to first window

			bool prompt = true;
			while (DocumentList.Count > 0)
			{
				if (!prompt || Query.Tables.Count == 0 || !QueryModified(Query))
					CloseFile(false);

				else
				{
					string msg = "Do you want to save the changes you made to " + Query.UserObject.Name + "?";
					int dr = MessageBoxMx.ShowWithCustomButtons(msg, UmlautMobius.String, 
						"Yes", "No", "No to All", "Cancel", MessageBoxIcon.Warning);
					if (dr == 1)
					{
						if (!SaveFile()) continue; // if save cancelled then ask again
						else CloseFile(false);
					}

					else if (dr == 2)
						CloseFile(false);

					else if (dr == 3)
					{
						prompt = false; // don't prompt any longer
						CloseFile(false);
					}

					else return false; // must have been cancelled
				}
			}

			return true;
		}

		/// <summary>
		/// Close file conditionally prompting if changed
		/// </summary>
		/// <param name="promptIfChanged"></param>
		/// <returns></returns>
		/// 
		public static bool CloseFile(
			bool promptIfChanged)
		{
			QbUtil.SetMode(QueryMode.Build);
			if (DocumentList.Count == 0 || CurrentQueryIndex < 0) return false;

			Document doc = DocumentList[CurrentQueryIndex];

			if (promptIfChanged && Query.Tables.Count > 0 && QueryModified(Query))
			{
				string msg = "Do you want to save the changes you made to " + Query.UserObject.Name + "?";
				DialogResult dr = MessageBoxMx.Show(msg, UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (dr == DialogResult.Cancel) return false;
				else if (dr == DialogResult.Yes)
				{
					if (!SaveFile()) return false; // if save cancelled then don't close
				}
			}

			if (doc != null && doc.Content is Query)
			{
				Query q = doc.Content as Query;
				if (q.QueryManager is QueryManager)
				{
					QueryManager qm = q.QueryManager as QueryManager;
					qm.Dispose();
				}

				q.ClearQueryManagers();
			}

			DocumentList.RemoveAt(CurrentQueryIndex);
			QC.RemoveTab(CurrentQueryIndex); // remove from tab control also

			if (CurrentQueryIndex == DocumentList.Count) CurrentQueryIndex--;
			if (CurrentQueryIndex < 0) return true; // any windows left?

			QC.SelectQuery(CurrentQueryIndex);
			return true;
		}

		public static void ClearQueryReferences(
			Query q)
		{
			if (q.QueryManager is QueryManager)
			{
				QueryManager qm = q.QueryManager as QueryManager;

			}

			q.ClearQueryManagers();
		}

		/// <summary>
		/// See if query modified since opened
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool QueryModified(
			Query q)
		{
			string content = QbUtil.SerializeForChangeDetection(q);
			if (content == q.UserObject.Content)
				return false;

			//try // see where 1st difference is for debug purposes
			//{
			//	int i1;
			//	for (i1 = 0; i1 < content.Length; i1++)
			//	{ // find where different
			//		if (i1 >= q.UserObject.Content.Length) break;
			//		if (content[i1] != q.UserObject.Content[i1]) break;
			//	}
			//	string c1 = content.Substring(i1);
			//	string c2 = q.UserObject.Content.Substring(i1);
			//	i1 = i1; // break here to see difference
			//}
			//catch (Exception ex) { }

			return true;
		}

		/// <summary>
		/// Switch to query build mode
		/// </summary>

		/// <summary>
		/// Set the current query mode
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="qm"></param>

		public static void SetMode(
			QueryMode mode)
		{
			SetMode(mode, QC.CurrentQuery);
		}

		/// <summary>
		/// Set the current query mode
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="qm"></param>

		public static void SetMode(
			QueryMode mode,
			Query q)
		{
			if (q == null) return;

			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null) // be sure we have a QueryManger for the query
			{
				qm = new QueryManager();
				qm.LinkMember(q);
			}

			StatusBarManager sbm = SessionManager.Instance.StatusBarManager;
			qm.LinkMember(sbm); // link the StatusBarManager to this QueryManager

			if (CurrentSetModeQuery == q && CurrentMode == mode) // same as current query & mode?
				return; // avoid unnecessary panel flashing

			CurrentSetModeQuery = q;
			q.OldMode = q.Mode;
			q.Mode = mode;
			CurrentMode = mode;

			// Setup for Build mode: Put QueryBuilderControl in QueriesControlTabPage

			if (mode == QueryMode.Build)
			{
				if (q.OldMode == QueryMode.Browse)
				{
					if (qm.MoleculeGrid != null) // stop any retrieval in progress
					{
						qm.MoleculeGrid.StopGridDisplay();
					}

					EditFind.HideDialog(); // hide grid EditFind if showing
					Progress.Hide(); // hide progress just in case it's still showing
				}

				QueriesControlTabPage.Controls.Clear(); // remove any existing QueryBuilder controls
				QueriesControlTabPage.Controls.Add(QueriesControl.QueryBuilderControl);

				sbm.DisplayRetrievalProgressState(RowRetrievalState.Undefined); // update status bar
				sbm.DisplayCurrentCount();
				sbm.ZoomControlVisible = false;
			}

// Setup for Browse mode

			else if (mode == QueryMode.Browse)
			{
				QueryResultsControl qrc; // control containing query results
				XtraTabPage gtp; // tab page containing grid panel
				MoleculeGridControl grid = null; // MoleculeGridControl
				XtraTabPage ctp; // tab page containing chart panel
				ResultsPages pages; // list of results pages for query
				ResultsPage page;
				ResultsViewProps view;
				//MoleculeGridView mgv;

				SS.I.UISetupLevel++;

				XtraTabPage qctp = QueriesControlTabPage; // tab page for the current query
				if (qctp != null && qctp.Controls != null)
					qctp.Controls.Clear(); // clear the tabpage for this query

				qrc = QueriesControl.QueryResultsControl; // results pages for the current query
				qrc.Dock = DockStyle.Fill;
				qm.LinkMember(qrc); // link Qm & QueryResults control
				qrc.BuildResultsPagesTabs(q); // build tabs for set of results pages for current query (q not Query) 

				if (qctp != null && qctp.Controls != null)
					qctp.Controls.Add(qrc); // add QueryResultsControl to tab page for query

				SS.I.UISetupLevel--;
				QC.QueryResultsControl.SelectPage(q.InitialBrowsePage); // show the initial browse page
			}

			return;
		}

		/// <summary>
		/// Return true if the current query contains the supplied metatable
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool BaseQueryContainsMetaTable(
			string mtName)
		{
			if (QueriesControl == null || QueriesControl.CurrentQuery == null) return false;

			//MetaTable mt = GetMetaTable(mtName);
			//if (mt == null) return false;
			if (QueriesControl.CurrentQuery.GetQueryTableByName(mtName) != null)
				return true;
			else return false;
		}

		/// <summary>
		/// Lookup a metatable by name. On an exception show error & return null
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static MetaTable GetMetaTable(
			string mtName)
		{
			MetaTable mt = null;
			try
			{
				mt = MetaTableCollection.GetWithException(mtName);
				return mt;
			}
			catch (Exception ex)
			{
				if (!Lex.StartsWith(ex.Message, "Undefined metatable name") &&
					!Lex.StartsWith(ex.Message, "Metatable not found") &&
					!Lex.StartsWith(ex.Message, "Unable to find data table"))
				{
					MessageBoxMx.ShowError("Unexpected GetMetaTable exception: " + ex.Message); // show message except for not found
					ServicesLog.Message(DebugLog.FormatExceptionMessage(ex));
				}
				return null;
			}
		}

		/// <summary>
		/// Gui & execution code for a quick structure search
		/// </summary>
		/// <returns></returns>

		internal static string QuickStructureSearch(
			bool includeExistingCriteria)
		{
			Query ssq;
			MetaTable mt;
			QueryColumn qc = null;
			MetaColumn mc;
			string tName, response;

			if (DocumentList.Count == 0 || // need to build new query
				DocumentList[CurrentQueryIndex].Type != DocumentType.Query)
			{
				NewQuery();
			}

			if (Query != null && Query.Tables.Count > 0)
			{ // base on current query if it contains any tables
				mt = Query.Tables[0].MetaTable.Root;
				tName = mt.Name;
			}

			else // base on first structure table
			{
				List<RootTable> rtiList = RootTable.GetList();
				if (rtiList == null || rtiList.Count == 0) throw new Exception("No structure database information is available");
				RootTable rti = rtiList[0];
				tName = rti.MetaTableName;
			}

			mt = MetaTableCollection.Get(tName);
			if (tName == null) throw new Exception("Could not retrieve the metatable information for structure table " + tName);

			qc = Query.GetFirstStructureColumn();

			if (qc == null) // need to add structure table to query
			{
				AddTablesToQuery(tName);
				qc = Query.GetFirstStructureColumn();
				if (qc == null) throw new Exception ("Can't find structure column to search in table " + mt.Label);
			}

			qc.Selected = true; // make sure structure is selected
			Qt = qc.QueryTable; // structure table is current table
			//			SelectQueryTableTab(Qt); 
			//			RenderQueryTable(Qt); // be sure the structure table is visible
			RenderQuery(Qt);

			// Get structure query input from user & execute

			if (!CriteriaEditor.EditStructureCriteria(qc))
				return "";
			if (qc.Criteria == "") return "";

			RenderQueryTable(Qt); // show structure criteria

			if (includeExistingCriteria) ssq = Query; // ok as is
			else
			{
				ssq = Query.Clone(); // make copy so we can remove any other criteria
				ssq.KeyCriteria = ssq.KeyCriteriaDisplay = "";
				foreach (QueryTable qt2 in ssq.Tables)
				{
					foreach (QueryColumn qc2 in qt2.QueryColumns)
					{
						if (qc2.MetaColumn.Name != qc.MetaColumn.Name ||
							qc2.MetaColumn.MetaTable.Name != qc.MetaColumn.MetaTable.Name)
							qc2.Criteria = ""; // remove unless structure search criteria
					}
				}
			}
			response = QueryExec.RunQuery(ssq, SS.I.DefaultQueryDest);
			return response;
		}

		/// <summary>
		/// Add the specified list to the query as key criteria
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		internal static string QuickSetListCriteria(
			UserObject uo)
		{
			Query listq;
			MetaTable mt;
			QueryColumn qc = null;
			MetaColumn mc;
			string response;

			if (uo == null) return "";

			if (DocumentList.Count == 0 || // need to build new query
				DocumentList[CurrentQueryIndex].Type != DocumentType.Query)
			{
				NewQuery();
			}

			if (Query.Tables.Count == 0) // need to add a table to query
			{
				if (MetaTable.KeyMetaTable == null) return "Can't find key metatable"; // todo: be smarter about this
				AddAndRenderTable(MetaTable.KeyMetaTable);
			}

			Query.KeyCriteria = " IN LIST " + Lex.AddSingleQuotes(uo.InternalName);
			Query.KeyCriteriaDisplay = "In list: " + uo.Name;

			RenderQuery(Qt);
			return "";
		}

		/// <summary>
		/// Quick list search retrieving data in current query
		/// </summary>
		/// <returns></returns>

		internal static string QuickListSearch(
			UserObject uo)
		{
			Query listq;
			MetaTable mt;
			QueryColumn qc = null;
			MetaColumn mc;
			string response;

			if (uo == null) return "";

			if (DocumentList.Count == 0 || // need to build new query
				DocumentList[CurrentQueryIndex].Type != DocumentType.Query)
			{
				NewQuery();
			}

			if (Query.Tables.Count == 0) // need to add a table to query
			{
				if (MetaTable.KeyMetaTable == null) return "Can't find key metatable"; // todo: be smarter about this
				AddAndRenderTable(MetaTable.KeyMetaTable);
			}

			listq = Query.Clone(); // make copy so we can modify criteria

			listq.KeyCriteria = " IN LIST " + Lex.AddSingleQuotes(uo.InternalName);
			listq.KeyCriteriaDisplay = "In list: " + uo.Name;

			foreach (QueryTable qt2 in listq.Tables)
			{
				foreach (QueryColumn qc2 in qt2.QueryColumns)
					qc2.Criteria = "";
			}

			listq.LogicType = QueryLogicType.And; // be sure not complex logic

			response = QueryExec.RunQuery(listq, OutputDest.WinForms);
			return response; // just return after one try
		}

		/// <summary>
		///  Clear query
		/// </summary>
		/// <param name="verify"></param>
		/// <returns></returns>

		public static bool ResetQuery(
			bool verify)
		{
			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to clear the current query?", UmlautMobius.String,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

			if (dr != DialogResult.Yes) return false;

			UserObject uo = Query.UserObject; // save name info
			Query = new Query();
			Query.UserObject = uo;
			Document mtw = DocumentList[CurrentQueryIndex];
			mtw.Content = Query;
			RenderQuery();
			return true;
		}

		/// <summary>
		/// Rename a query
		/// </summary>
		/// <returns></returns>

		public static bool RenameQuery()
		{
			UserObject uo, uo2 = null;
			int qi;

			uo = Query.UserObject;

			if (!UserObjectUtil.UserHasWriteAccess(uo))
			{
				MessageBoxMx.ShowError("You are not authorized to rename " + uo.Name);
				return true;
			}

			string newName = uo.Name;

			while (true) // loop until we get a good name or user cancels
			{
				newName = InputBoxMx.Show("Enter the new name for " + uo.Name,
				"Rename", newName);

				if (Lex.IsUndefined(newName)) return false;

				if (!UserObjectUtil.IsValidUserObjectName(newName)) // check for disallowed characters
				{
					MessageBoxMx.ShowError("The name \"" + newName + "\" contains one or more disallowed characters.");
					continue;
				}

				Query q2 = FindQueryByName(newName);
				if (q2 != null && q2 != Query)
				{
					MessageBoxMx.ShowError("The name \"" + newName + "\" is already assigned to another open query.");
					continue;
				}

				uo2 = uo.Clone(); // make a temp copy of the uo to check for existing name
				uo2.Name = newName;

				if (uo2.Id > 0) // previously saved?
				{
					uo2.Id = 0; // clear Id so UO not looked up by id

					if (Lex.Ne(newName, uo.Name) && UserObjectDao.ReadHeader(uo2) != null) // another uo already exist under new name
					{
						MessageBoxMx.ShowError("\"" + newName + "\" already exists as a saved query.");
						continue;
					}

					uo2.Id = uo.Id; // restore UO Id
					UserObjectDao.UpdateHeader(uo2); // update saved UO with new name
				}

				break;
			}

			uo.Name = newName; // set new name in query uo
			SessionManager.SetShellTitle(newName); // update the UI
			QueriesControl.SetQueryTabTitle(newName);

			if (ContentsTreeCtl != null)
				UserObjectTree.UpdateObjectInTreeControl(uo, uo2, ContentsTreeCtl);

			return true;
		}

		/// <summary>
		/// Convert a stat display format (QnfEnum to string)
		/// </summary>
		/// <param name="statFormat"></param>
		/// <returns></returns>

		public static string ConvertStatDisplayFormat(
			QnfEnum statFormat)
		{
			string txt = "";

			if ((statFormat & QnfEnum.StdDev) != 0)
			{
				if ((statFormat & QnfEnum.DisplayStdDevLabel) != 0)
				{
					txt += "sd=";
				}

				txt += "stddev";
			}

			if ((statFormat & QnfEnum.StdErr) != 0)
			{
				if (txt != "") txt += ", ";
				if ((statFormat & QnfEnum.DisplayStdErrLabel) != 0)
				{
					txt += "se=";
				}

				txt += "stderr";
			}

			if ((statFormat & QnfEnum.NValue) != 0)
			{
				if (txt != "") txt += ", ";
				txt += "n=m/t";
			}

			return txt;
		}

		/// <summary>
		/// Convert a stat display format (string to QnfEnum)
		/// </summary>
		/// <param name="txt"></param>

		public static QnfEnum ConvertStatDisplayFormat(
			string txt)
		{
			QnfEnum qnf = 0;
			if (Lex.Contains(txt,"stddev")) qnf |= QnfEnum.StdDev;
			if (Lex.Contains(txt,"sd")) qnf |= QnfEnum.DisplayStdDevLabel;
			if (Lex.Contains(txt,"stderr")) qnf |= QnfEnum.StdErr;
			if (Lex.Contains(txt,"se")) qnf |= QnfEnum.DisplayStdErrLabel;
			if (Lex.Contains(txt,"n")) qnf |= QnfEnum.NValue | QnfEnum.NValueTested |
																	 QnfEnum.DisplayNLabel;
			return qnf;
		}

		/// <summary>
		/// Convert alert from internal to display format
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>

		public static string ConvertAlertDisplayFormat(
			int interval)
		{
			string txt;

			if (interval <= 0) txt = "None";
			else txt = "Check every " + interval.ToString() + " days";
			return txt;
		}

		/// <summary>
		/// Convert alert from display to internal
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>

		public static int ConvertAlertDisplayFormat(
			string txt)
		{
			if (txt.Trim() == "" || Lex.Eq(txt.Trim(), "None")) return -1;

			string[] sa = txt.Split(' ');

			try
			{
				int interval = Int32.Parse(sa[2]);
				return interval;
			}
			catch (Exception ex)
			{ return -1; }
		}

		/// <summary>
		/// See if command is quicksearch for cid after enter key is pressed & process if so
		/// </summary>
		/// <param name="item"></param>
		/// <returns>null if not a valid cid, blank if submenu displayed, SelectAllCompound data command if not target data</returns>

		public static string CheckIfQuickSearchArgIsCid(
			string tok)
		{
			string nextCommand = null;

			string cid = CompoundId.Normalize(tok);
			if (!CompoundIdUtil.Exists(cid))
				nextCommand = null;

			else // return command to display all data for drilldown
			{
				nextCommand = "SelectAllCompoundData " + cid;
			}

			return nextCommand;
		}

/// <summary>
/// Return true if we can do a multi-database Assay drilldown for the query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public static bool IsMdbAssayDataViewAvailable(
			Query q)
		{
			if (!IsMdbAssayDataViewAvailable())
				return false;

			else if (q != null && q.RootMetaTable != null && Lex.Eq(q.RootMetaTable.Name, MetaTable.PrimaryRootTable))
				return true;

			else return false;
		}

		/// <summary>
		/// Return true if we can do a multi-database assay drilldown for the metatable
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool IsMdbAssayDataViewAvailable(
			MetaTable mt)
		{
			if (!IsMdbAssayDataViewAvailable())
				return false;

			else if (mt != null && Lex.Eq(mt.Root.Name, MetaTable.PrimaryRootTable))
				return true;

			else return false;
		}

		public static bool IsMdbAssayDataViewAvailable()
		{
			if (ServicesIniFile.ReadBool("IsMdbAssayDataViewAvailable", true))
				return true;

			else return false;
		}

		/// <summary>
		/// Create a user folder
		/// </summary>
		/// <param name="parentFolder"></param>
		/// <param name="treeCtl"></param>
		/// <returns></returns>

		public static void CreateUserFolder(string parentFolder, ContentsTreeControl treeCtl)
		{
			MetaTreeNode mtn = null;
			int i;

			string folderName = "New Folder";
			while (true)
			{
				folderName = InputBoxMx.Show("Enter a name for the new folder", "Create folder", folderName);
				if (String.IsNullOrEmpty(folderName)) return;

				if (!UserObjectUtil.IsValidUserObjectName(folderName)) // check for disallowed characters
				{
					DialogResult dr = MessageBoxMx.Show(
						"The name \"" + folderName + "\" contains an invalid character\r\n" +
						"Please provide a different name.",
						"Invalid Folder Name", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					if (dr == DialogResult.Cancel) return;
				}

				else break;
			}

			MetaTreeNode parentNode = UserObjectTree.FindFolderNode(parentFolder);
			if (parentNode == null) // add system node to UserObjectTree if not there already
				parentNode = UserObjectTree.AddSystemFolderNode(parentFolder);

			for (i = 0; i < parentNode.Nodes.Count; i++)
			{
				MetaTreeNode childNode = parentNode.Nodes[i];
				if (childNode.Owner.ToUpper() == SS.I.UserName.ToUpper() &&
				 childNode.Label == folderName)
					break;
			}

			if (i < parentNode.Nodes.Count)
			{
				MessageBoxMx.ShowError("You already have an item named \"" + folderName + "\" here.");
				return;
			}

			MetaTreeNode folderNode = UserObjectTree.CreateUserFolderObjectAndNode(parentNode, folderName, SS.I.UserName);

			if (treeCtl.Name != "ContentsTree") // if this isn't the main contents tree update the view manually
				treeCtl.RefreshSubtree(folderNode.Parent.Name);

			treeCtl.ExpandNode(parentFolder); // be sure the parent of the new folder is expanded so that the new folder shows 

			return;
		}

		/// <summary>
		/// Show a document whose Url comes from a config file setting
		/// </summary>
		/// <param name="parmName"></param>

		public static void ShowConfigParameterDocument(
			string parmName,
			string title)
		{
			string fileName = ServicesIniFile.Read(parmName);
			if (!String.IsNullOrEmpty(fileName))
				UIMisc.ShowHelpFile(fileName, title);

			else MessageBoxMx.ShowError("Document not found.\r\n" + title + "\r\n" + parmName);

			return;
		}

		/// <summary>
		/// Return true if the query contains an existing DataTable that should be 
		/// browsed when the query is opened.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool BrowseExistingDataTable(Query q)
		{
			if (q == null) return false;
			if (!q.SerializeResultsWithQuery) return false;

			DataTableMx dt = q.ResultsDataTable as DataTableMx;
			if (dt == null) return false;

			if (dt != null && dt.Rows.Count > 0)
				return true;
			else return false;
		}

/// <summary>
/// Check if query can be run in background and display message if not
/// </summary>
/// <param name="query"></param>
/// <returns></returns>

		public static bool CanRunQueryInBackground(Query query)
		{
			if (query == null || query.UserObject.Id == 0)
			{
				MessageBoxMx.ShowError("This query must first be saved before it can be run in the background.");
				return false;
			}

			int criteriaCount = query.GetCriteriaCount(true, false); // count simple criteria
			if (query.LogicType == QueryLogicType.Complex && !String.IsNullOrEmpty(query.ComplexCriteria))
				criteriaCount = 1;

			if (criteriaCount == 0)
			{
				MessageBoxMx.ShowError("One or more search criteria must be defined for this query before it can be run in the background.");
				return false;
			}

			if (Lex.Contains(query.KeyCriteria, "in list current") || Lex.Contains(query.KeyCriteria, "in list " + UserObject.TempFolderNameQualified))
			{
				MessageBoxMx.ShowError(
					"This query cannot be run in the background because it uses a volatile temporary list criteria.\n" +
					"If you save the temporary list and then use this saved list as the criteria then the query will run.");
				return false;
			}

			if (QueryModified(query))
			{
				string msg = 
					"It appears that this query contains modifications that haven't been saved yet.\n" +
					"These changes must be saved before the query can be run properly in the background.\n" +
					"Do you want to save the query now?";
				DialogResult dr = MessageBoxMx.Show(msg, "Query Modified", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes) return false;

				if (!SaveQuery(query)) return false;
			}

			return true;
		}

		/// <summary>
		/// Update any existing internal metatable with new definition (note: already open query may be using current mt definition)
		/// </summary>
		/// <param name="tName"></param>

		public static MetaTable UpdateMetaTableCollection(
			string tName)
		{
			MetaTable mt = MetaTableCollection.GetExisting(tName); // get any already loaded metatable with this name
			MetaTableCollection.RemoveGlobally(tName); // remove on client and server
			MetaTable mt2 = MetaTableCollection.Get(tName); // build new definition

			if (mt != null && mt2 != null) // overwrite the elements of the metatable so that the reference stays the same
			{
				mt.Parent = mt2.Parent;
				mt.TableMap = mt2.TableMap; // copy new map value
				mt.Description = mt2.Description;
				mt.MetaColumns = mt2.MetaColumns; // also column info

				// Note: There's a bug here where updating a calc field so that the key field has a different name
				// causes the query to fail to run. It will run if saved and then reopened when it will pick up the new name.

				for (int di = 0; di < QueriesControl.Instance.DocumentList.Count; di++)
				{ // update any QueryColumn to MetaColumn references
					Document d = QueriesControl.Instance.DocumentList[di];
					if (d.Type != DocumentType.Query) continue;
					Query q = (Query)d.Content;
					foreach (QueryTable qt2 in q.Tables)
					{
						if (Lex.Ne(qt2.MetaTable.Name, mt.Name)) continue;
						foreach (QueryColumn qc2 in qt2.QueryColumns)
						{
							MetaColumn mc2 = mt.GetMetaColumnByName(qc2.MetaColumn.Name);
							if (mc2 != null) qc2.MetaColumn = mc2;
						}
					}
				}

				//MetaTableCollection.UpdateGlobally(mt); // update services
				return mt;
			}

			else // not in collection just return new table
			{
				//MetaTableCollection.UpdateGlobally(mt2);
				return mt2;
			}
		}


	} // QbUtil

}
