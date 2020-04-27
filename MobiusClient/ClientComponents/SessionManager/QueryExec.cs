using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Query execution and results generator executive
	/// </summary>

	public class QueryExec : IQueryExec
	{
		internal QueryManager QueryManager; // links together elements of query
		public static QueryResultsControl QueryResultsControl
		{ get { return SessionManager.Instance.QueriesControl.QueryResultsControl; } }
		internal Query Query
		{
			get { return QueryManager.Query; }
			set { QueryManager.Query = value; }
		}
		internal ResultsFormat ResultsFormat
		{
			get { return QueryManager.ResultsFormat; }
			set { QueryManager.ResultsFormat = value; }
		}
		internal ResultsFormat Rf { get { return QueryManager.ResultsFormat; } } // ResultsFormat alias
		internal ResultsFormatter ResultsFormatter
		{
			get { return QueryManager.ResultsFormatter; }
			set { QueryManager.ResultsFormatter = value; }
		}
		internal DataTableManager DtMgr { get { return QueryManager.DataTableManager; } } // DataTableManager alias
		internal QueryEngine QueryEngine
		{
			get { return QueryManager.QueryEngine; }
			set { QueryManager.QueryEngine = value; }
		}
		internal List<string> ResultsKeys
		{
			get { return DtMgr.ResultsKeys; }
			set { DtMgr.ResultsKeys = value; }
		}

		internal Exception QueryEngineException; // exception from Qe
		internal bool QueryResultsAvailable = false;
		internal Query OriginalQuery = null;
		internal Query TransformedQuery = null;

		bool RestartDisplay; // restart display if true
		int DisplayInterruptedPage; // page to restart display at
		bool OutputCancelled; // user has cancelled report output
		string ReturnMsg = "";

		// Static vars

		internal static QueryExec LastQEx; // last QueryExec created & executed

		/// <summary>
		/// Basic constructor
		/// </summary>

		public QueryExec()
		{
			QueryManager = new QueryManager();
			QueryManager.QueryExec = this;
			return;
		}

		/// <summary>
		/// Construct using supplied QueryManager
		/// </summary>

		public QueryExec(QueryManager qm)
		{
			QueryManager = qm;
			QueryManager.QueryExec = this;
			return;
		}

/// <summary>
/// Construct with supplied results format
/// </summary>
/// <param name="q"></param>
/// <param name="resultsFormat"></param>

		public QueryExec(
			Query q, 
			ResultsFormat resultsFormat)
		{
			QueryManager = new QueryManager();
			QueryManager.QueryExec = this;

			Query = q;
			ResultsFormat = resultsFormat;
			return;
		}

		/// <summary>
		/// Construct with supplied results format
		/// </summary>
		/// <param name="resultsFormat"></param>

		public QueryExec(ResultsFormat resultsFormat)
		{
			if (resultsFormat.QueryManager != null)
				QueryManager = resultsFormat.QueryManager;
			else QueryManager = new QueryManager();
			QueryManager.QueryExec = this;
			ResultsFormat = resultsFormat;
			return;
		}

		/// <summary>
		/// Run query using supplied OutputDest displaying any error message
		/// </summary>
		/// <param name="query"></param>
		/// <param name="outputDest"></param>
		/// <returns></returns>

		public static string RunQuery(
			Query query,
			OutputDest outputDest)
		{
			try
			{
				bool browseExistingResults = QbUtil.BrowseExistingDataTable(query);
				return RunQuery(query, outputDest, OutputFormContext.Session, null, browseExistingResults);
			}

			catch (UserQueryException ex)
			{ // just show message
				Progress.Hide();
				MessageBoxMx.ShowError(ex.Message);
				return "";
			}

			catch (Exception ex)
			{ // non-standard query exception, provide more detail
				Progress.Hide();
				string msg = DebugLog.FormatExceptionMessage(ex);

				if (!Lex.Contains(msg, "QueryLogged:")) // exception & query
					QueryEngine.LogExceptionAndSerializedQuery(msg, query);
				else ServicesLog.Message(msg); // just log exception

				MessageBoxMx.ShowError("Unexpected Exception\n\n" + msg);
				return "";
			}
		}

		/// <summary>
		/// Prepare and run query that includes export options
		/// </summary>
		/// <param name="q"></param>
		/// <param name="ep"></param>
		/// <returns></returns>

		public string RunQuery(
			Query query,
			ExportParms ep)
		{
			ResultsFormat rf = new ResultsFormat();
			rf.CopyFromExportParms(ep);
			return RunQuery2(query, rf);
		}

		/// <summary>
		/// Run Query & return any error message
		/// </summary>
		/// <param name="query"></param>
		/// <param name="queryDest"></param>
		/// <param name="outputFormContext"></param>
		/// <param name="browseExistingResults">If true browse existing results</param>
		/// <param name="suppressNoDataMessage"></param>
		/// <returns>Command command or an error message</command></returns>

		public static string RunQuery(
				Query query,
				OutputDest outputDest,
				OutputFormContext outputFormContext,
				ExitingQueryResultsDelegate exitingQueryResultsCallBack = null,
				bool browseExistingResults = false,
				bool suppressNoDataMessage = false
			)
		{
			ResultsFormat rf = new ResultsFormat();
			rf.OutputDestination = outputDest;
			rf.OutputFormContext = outputFormContext;
			rf.CustomExitingQueryResultsCallback = exitingQueryResultsCallBack;
			rf.SuppressNoDataMessage = suppressNoDataMessage;

			string response = RunQuery2(query, rf, browseExistingResults);
			query.UseResultKeys = false; // turn off use of keys

			if (response != "" && !response.ToLower().StartsWith("command") && !suppressNoDataMessage)
			{
				MessageBoxMx.Show(
					response, UmlautMobius.String,
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				response = "Command EditQuery";
			}

			return response;
		}

		/// <summary>
		/// Run Query & return any error message
		/// </summary>
		/// <param name="query"></param>
		/// <param name="rf"></param>
		/// <param name="browseExistingResults">If true browse existing results</param>
		/// <returns>Command command or an error message</command></returns>

		public static string RunQuery2(
			Query query,
			ResultsFormat rf,
			bool browseExistingResults = false)
		{
			QueryManager qm = null;
			DataTableMx dt;
			QueryTable qt = null;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			QueryExec qex;
			ResultsFormatter rfmtr;
			Query modifiedQuery = null;
			DialogResult dr;
			bool success;
			int ti, ci, count;

			if (query == null) throw new Exception("Null Query");


			qm = query.QueryManager as QueryManager;

			if (qm == null || !qm.ContainsRenderedResults())
				browseExistingResults = false;

			if (ToolHelper.IsOldToolQuery(query) && !browseExistingResults)
			{
				return ToolHelper.RunOldToolQuery(query);
			}

			//if (query.FirstView == null)
			//{
			ResultsViewType tableViewType = ResultsViewType.Table;
			if (rf.OutputDestination == OutputDest.Html)
				tableViewType = ResultsViewType.HtmlTable;

			//}

			//CheckForConnectionLeaks(); // do routine check for connection leaks (disabled for now)

// Build/update the QueryManager as needed (including the QueryExec) if running the query rather than just browsing existing results

			if (!browseExistingResults) // initialize QueryManager
			{
				query.SetupQueryPagesAndViews(tableViewType); // adjust views as necessary to match query
				qm = BuildQueryManager(query, rf);
			}

// Attempt to display already retrieved data in memory or in cached results file

			else 
			{
				if (qm != null) 
				{
					if (qm.QueryExec == null) // create QueryExec if not defined
						qex = new QueryExec(qm);

					if (qm.ResultsFormatter == null) // create ResultsFormatter if not defined
						rfmtr = new ResultsFormatter(qm);
				}

				else
				{
					qm = BuildQueryManager(query, rf);
				}

				if (Lex.IsDefined(query.ResultsDataTableFileName)) // get data from file
					DataTableManager.LoadDataTableFromFile(query);

				else // should have data in DataTable already
					DataTableManager.ValidateExistingDataTable(query);

				if (qm.DataTableManager != null && qm.QueryExec != null)
					qm.QueryExec.ResultsKeys = qm.DataTableManager.ResultsKeys; // have data in memory already
			}

			if (rf.OutputDestination == OutputDest.WinForms && rf.SessionOutputFormContext)
			{ // save qm and query if this is a runquery from the main window
				QueriesControl.Instance.CurrentBrowseQuery = query; // save ref to the query being browsed
			}

			if (rf.SessionOutputFormContext) // if not a popup then update main status bar
				qm.StatusBarManager = SessionManager.Instance.StatusBarManager;

			LastQEx = qm.QueryExec; // keep track of last QueryExec

		SetupRunQuery:

			bool saveHitList = (rf.SessionOutputFormContext && !browseExistingResults); // if popup or browsing existing results don't change hitlist
			qex = qm.QueryExec;
			string response = qex.RunQuery3(rf, saveHitList, browseExistingResults); // run the query

			if (qex.ResultsKeys != null && qex.ResultsKeys.Count > 0 && // save in history list if normal query that resulted in hits
				!query.Preview && !MqlUtil.SingleStepExecution(query) && !browseExistingResults &&
				rf.OutputDestination == OutputDest.WinForms)
			{
				CidList keyList = new CidList(qex.ResultsKeys);
				AddToHistoryList(query, keyList); // use original query
			}

			if (response != "" && !response.ToLower().StartsWith("command"))
				return response;

			else if (Lex.EndsWith(response, "refetch")) // data to be retrieved has changed, redo fetch part of query
			{
				if (!query.Preview) // use current keys if not preview
					query.ResultKeys = qex.QueryEngine.GetKeys();
				goto SetupRunQuery;
			}

			//else if (response == "") response = "Command EditQuery";

			//if (rf.OutputDestination == OutputDest.Grid)
			//{
			//  QbUtil.SetMode(QueryMode.Build); // back to build mode
			//  QbUtil.RenderQuery(QbUtil.Qt);
			//}

			return response;
		}

		/// <summary>
		/// If query uses the current list be sure the server has the proper current list
		/// </summary>
		/// <param name="query"></param>

		static void WriteCurrentListToServerIfNeeded(
			Query q)
		{
			if (Lex.Contains(q.KeyCriteria, UserObject.TempFolderName + ".Current"))
				CidListCommand.WriteCurrentList();
		}

		/// <summary>
		/// Check for any connection leaks
		/// </summary>

		public static void CheckForConnectionLeaks()
		{
			if (!UserObjectTree.BuildComplete) return; // don't check while building tree

			string leakMsg = DbConnection.CheckForConnectionLeaks(); // see if any connection leaks
			if (String.IsNullOrEmpty(leakMsg)) return;

			ServicesLog.Message(leakMsg); // log it
			if (SS.I.IsDeveloper) // show to dev
				MessageBoxMx.ShowError(leakMsg);
			return;
		}

		/// <summary>
		/// Do basic checks for a valid query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static DialogResult ValidateQuery(Query query)
		{
			QueryTable firstQueryTable = null, qt;
			QueryColumn qc;
			string firstTableKey = "";
			int qti, qci;

			if (query.Tables.Count == 0)
				throw new QueryException("Query contains no query tables");

			// If multiple tables make sure key table is first

			if (query.IncludeRootTableAsNeeded() && query == QbUtil.Query)
				QbUtil.RenderQuery(0); // rerender query

			firstQueryTable = query.Tables[0]; // make a not of the first table and key 
			firstTableKey = GetParentKey(firstQueryTable); // so we can compare first key with other table keys

			// Be sure we have some criteria (if other than preview)	

			int totalCriteria = 0;
			if (query.KeyCriteria != "") totalCriteria++;

			int notExistsCriteria = 0;
			for (qti = 0; qti < query.Tables.Count; qti++)
			{
				qt = query.Tables[qti];

				string currentTableKey = GetParentKey(qt);

				if (qt.KeyQueryColumn == null || currentTableKey == null) 
					throw new QueryException("Key column is not defined for table: " + qt.MetaTable.Name);

				// Code below commented out because it disallowed valid multidatabase queries.
				// e.g. Search CorpDb and ACD together in same query with a subtable from each database included
				// Similar to MqlUtil.CheckTableCompatibility which does work properly
				// Unsure of original intent
				// --------------------------------------
				/// Tables with different keys cannot be combined
				//if (qti > 0 && firstTableKey != currentTableKey)
				//	throw new DevExpress.PivotGrid.QueryMode.QueryException(
				//			"Tables with different keys cannot be in the same query.\n\n" +
				//			"Key for " + firstQueryTable.ActiveLabel + ": " + firstTableKey + "\n" +
				//			"Key for " + qt.ActiveLabel + ": " + currentTableKey + "\n");

				if (!qt.KeyQueryColumn.Selected) // select key column if not done yet
					qt.KeyQueryColumn.Selected = true;

				for (qci = 0; qci < qt.QueryColumns.Count; qci++)
				{
					qc = qt.QueryColumns[qci];
					if (qc.MetaColumn == null) continue;

					if (qc.Criteria != "" &&
						qc.FilterSearch && // must be used to filter search
						!qc.MetaColumn.IsDatabaseSetColumn &&
						!qc.IsKey) // key already counted
					{
						totalCriteria++;
						if (qc.HasNotExistsCriteria) notExistsCriteria++;

						string msg = ""; // check for summarization type mismatch on criteria
						if (qt.MetaTable.UseSummarizedData && !qc.MetaColumn.SummarizedExists)
							msg = "summarized";
						else if (!qt.MetaTable.UseSummarizedData && !qc.MetaColumn.UnsummarizedExists)
							msg = "unsummarized";

						if (msg != "" && !SS.I.QueryTestMode)
						{
							Progress.Hide();

							msg = "This query has criteria on " + qc.ActiveLabel +
								" which does not exist in the\n" +
								msg + " view of " + qt.ActiveLabel + ".\n" +
								"Do you want to remove this criteria from the query?";
							DialogResult dr = MessageBoxMx.Show(msg, "Invalid Criteria",
								MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

							if (dr == DialogResult.Yes)
							{
								qc.Criteria = qc.CriteriaDisplay = "";
								totalCriteria--;
								if (query == QbUtil.Query) // redraw with possible change (referencing global query)
									QbUtil.RenderQuery(QbUtil.Qt);
							}

							else if (dr == DialogResult.Cancel || !Security.IsAdministrator(SS.I.UserName))
								return DialogResult.Cancel; // can't continue unless administrator

						}
					}
				}
			}

			bool noCriteria = false;
			if (query.Preview) noCriteria = false; // don't need criteria if in preview mode
			else if (query.LogicType == QueryLogicType.Complex && query.ComplexCriteria != "") noCriteria = false; // something there for complex
			else if (!query.RetrievesDataFromQueryEngine) noCriteria = false; // don't need criteria if no tables retrieved from query engine
			else if (totalCriteria == 0) noCriteria = true;

			//if (query.

			if (noCriteria)
			{
				//					if (query.Tables.Count == 1) query.Preview = true; // do as a preview if just one table (this changes expected behavior and can't be done)
				//          else

				if (SessionManager.CurrentResultKeysCount <= 0)
					throw new UserQueryException("You must define at least one criteria for the query");

				else // see if user wants to use current list
				{
					Progress.Hide();

					DialogResult dr = MessageBoxMx.Show(
						"This query contains no criteria.\n" +
						"Do you want to use the current search results (" + SessionManager.CurrentResultKeysCount + ") ?",
						"No Criteria", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (dr != DialogResult.Yes) return DialogResult.Cancel;

					query.KeyCriteria = " IN LIST CURRENT";
					query.KeyCriteriaDisplay = "In current search results list (" + SessionManager.CurrentCountNotNull.ToString() + ")";

					foreach (QueryTable qt2 in query.Tables)
					{
						qc = qt2.KeyQueryColumn;
						if (qc != null)
						{
							qc.Criteria = query.KeyCriteria;
							qc.CriteriaDisplay = query.KeyCriteriaDisplay;
						}
					}
				}

			}


			else if(query.LogicType != QueryLogicType.Complex && notExistsCriteria >= totalCriteria && !query.Preview && query.RetrievesDataFromQueryEngine)
				throw new UserQueryException("At least one table in the query must contain criteria other than \"Doesn't Exist\".");

			query.ValidateAggregation();

			return DialogResult.OK;
		}

/// <summary>
/// Build a QueryManager from the supplied Query and ResultsFormat
/// </summary>
/// <param name="query"></param>
/// <param name="rf"></param>

	static QueryManager BuildQueryManager(
		Query query,
		ResultsFormat rf)
		{
			query.ClearQueryManagers(); // clear any existing managers

			QueryManager qm = new QueryManager();
			QueryExec qex = new QueryExec(qm);
			qm.Initialize(query, rf, null, null);

			rf = qm.ResultsFormat;

			if (rf.Query.ResultsDataTable != null) // if existing DataTable use it
				qm.DataTable = rf.Query.ResultsDataTable as DataTableMx;

			return qm;
		}

		/// <summary>
		/// Run the query
		/// </summary>
		/// <param name="browseExistingResults">If true browse existing results</param>
		/// <returns></returns>

		public string RunQuery3(
			ResultsFormat rf,
			bool saveHitlist,
			bool browseExistingResults)
		{
			Query modifiedQuery;
			QueryTable qt;
			QueryColumn qc;
			ResultsTable rt;
			ResultsField rfld;
			MetaTable mt;
			MetaColumn mc;
			string txt, msg;
			DialogResult dr;
			bool success;
			CellGraphic cg;
			Lex lex = new Lex();
			string tempfile, tok, command, unrecognizedCommand, response;
			int ti, gi, rc, i1, i2;

			// Begin execution

			if (rf == null)
				throw new Exception("QueryExec.Run - Null ResultsFormat");

			if (ResultsFormatter == null)
				throw new Exception("QueryExec.Run - Null ResultsFormatter");

			if (rf.Segments == null)
				throw new Exception("QueryExec.Run - Null ResultsFormat.Segments");

			if (Query == null)
				throw new Exception("QueryExec.Run - Null Rf.Query");

			if (Query.Tables == null || Query.Tables.Count <= 0)
				throw new QueryException("QueryExec.Run - No Query Tables");

			QueryManager qm = QueryManager;

			ReturnMsg = "";

			//bool useExistingQueryEngine = Rf.ParentQe != null;
			//bool useExistingDataTable = Query.BrowseExistingResultsWhenOpened && Query.SerializeResults && 
			//  qm.DataTable != null && qm.DataTable.Rows.Count > 0;

			try
			{

				//if (Math.Sqrt(4) == 2) throw new Exception("test"); // debug

				if (!browseExistingResults) // normal open of search
				{
					Progress.Show("Analyzing query..."); // put up a status message to the user as soon as possible to let them know something is happening...

					dr = ValidateQuery(Query);
					if (dr == DialogResult.Cancel) return "";

					WriteCurrentListToServerIfNeeded(Query);

					if (rf.OutputDestination == OutputDest.WinForms) // update access stats if grid
						UpdateTableUsageStatistics(Query);

					Query.ResultsDataTable = null; // be sure to get new results

					qm = BuildQueryManager(Query, rf);

					Query.ResetViewStates(); // reset state of views for proper operation

					if (Rf.ParentQe == null) // open search unless using existing query engine
					{
						if (!ExecuteSearch(saveHitlist)) // returns false if cancelled by user
						{
							Progress.Hide();
							return "";
						}
					}

					if ((ResultsKeys == null || ResultsKeys.Count == 0) &&  // nothing for search
						!Query.Preview &&
						!MqlUtil.SingleStepExecution(Query) &&
						qm.DataTable.Rows.Count == 0 &&
						Query.RetrievesDataFromQueryEngine)
					{
						// if (!Rf.PopupDisplay) 
						Progress.Hide();
						if (qm.StatusBarManager != null)
							qm.StatusBarManager.DisplayStatusMessage("");
						// if (QueryEngine.Cancelled) return ""; // cancelled by user
						msg = "No data have been found that matches your query.";
						if (ResultsFormat.PopupOutputFormContext && !ResultsFormat.SuppressNoDataMessage)
						{
							MessageBoxMx.Show(msg, "Search Result",
								MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return "Command EditQuery"; // return to edit query menu 
						}
						else return (msg);
					}

					//if (ResultsFormat.PopupDisplay)
					//  SessionManager.DisplayStatusMessage("Retrieving data...");
					//else Progress.Show("Retrieving data...", UmlautMobius.Value, true, "Cancelling Retrieval...");

					//Progress.Show("Retrieving data...", UmlautMobius.String, true, "Cancelling Retrieval...");
					Progress.Hide(); // hide progress - "Retrieving data..." message now appears as bottom line of grid

					if (ResultsFormat.Grid)
					{
						if (ResultsFormat.SessionOutputFormContext) // if normal main session form grid display, set browse mode & view state
						{
							Query.ResetViewStates(); // reset view state for all views
							QbUtil.SetMode(QueryMode.Browse, Query);

							if (ResultsFormat.Query.LogicType == QueryLogicType.And) // log grid query by logic type
								UsageDao.LogEvent("QueryGridAnd", "");
							else if (ResultsFormat.Query.LogicType == QueryLogicType.Or)
								UsageDao.LogEvent("QueryGridOr", "");
							else if (ResultsFormat.Query.LogicType == QueryLogicType.Complex)
								UsageDao.LogEvent("QueryGridComplex", "");
						}

						else if (ResultsFormat.PopupOutputFormContext) // create popup window & configure
						{
							PopupResults.Show(qm);
							//MoleculeGridPanel.ConfigureAndShow(qm, null);
						}

						else if (ResultsFormat.ToolOutputFormContext)
						{
							ContainerControl cc;
							QueryResultsControl qrc = ResultsFormat.OutputContainerControl as QueryResultsControl;
							AssertMx.IsTrue(qrc != null, "ResultsFormat.OutputContainerControl must be a QueryResultsControl");
							if (!WindowsHelper.FindContainerControl(qrc, typeof(ToolResultsContainer), out cc))
								throw new Exception("ToolResultsContainer not found");

							ToolResultsContainer trc = cc as ToolResultsContainer;
							trc.SetupQueryResultsControlForResultsDisplay(qm);
						}

						else throw new Exception("Invalid OutputformContext: " + ResultsFormat.OutputFormContext);

					}
				}

				else // reentering display switch to browse tab
				{
					QbUtil.SetMode(QueryMode.Browse, Query);
				}

				response = ResultsFormatter.BeginFormatting(browseExistingResults); // format the data

				if (ResultsFormat.SessionOutputFormContext) // normal display
				{
					if (MqlUtil.SingleStepExecution(Query))
					{ // be sure hit count display is up to date
						if (ResultsKeys != null)

							if (qm.StatusBarManager != null)
								qm.StatusBarManager.DisplayCurrentCount();
					}

					if (saveHitlist)
					{
						CidList hitList = new CidList(ResultsKeys);
						rc = CidListCommand.WriteCurrentList(hitList);
						SessionManager.DisplayCurrentCount();
					}

				}

				return response;
			} // end of surrounding try

			catch (Exception ex)
			{
				Progress.Hide();
				if (ex is UserQueryException) // exception that can occur from user error
					throw new UserQueryException(ex.Message, ex);

				else
				{
					msg = DebugLog.FormatExceptionMessage(ex);
					if (!Lex.Contains(msg, "QueryLogged:")) // exception & query
						QueryEngine.LogExceptionAndSerializedQuery(msg, Query);
					else ServicesLog.Message(msg); // just log exception
					throw new Exception(ex.Message, ex); // pass it up
				}
			}
		}

		/// <summary>
		/// Execute search & get hit list
		/// </summary>
		/// <param name="saveHitlist"></param>
		/// <returns></returns>

		public bool ExecuteSearch(bool saveHitlist)
		{
			int voi, ti, fi;
			ResultsTable rt;
			ResultsField rfld;
			MetaTable mt;
			MetaColumn mc, mc2;

			if (saveHitlist) // clear current list
			{
				SessionManager.CurrentResultKeys = new List<string>();
				if (QueryManager.StatusBarManager != null)
					QueryManager.StatusBarManager.DisplayCurrentCount();
			}

			// Start thread running the query

			QueryResultsAvailable = false;
			QueryEngineException = null;
			ThreadStart ts = new ThreadStart(ExecuteQueryThreadMethod);
			Thread executeQueryThread = new Thread(ts);
			executeQueryThread.Name = "ExecuteSearch";
			executeQueryThread.IsBackground = true;
			executeQueryThread.SetApartmentState(ApartmentState.STA);
			executeQueryThread.Start();

			// Put up message for user

			if (((Query.ResultKeys != null && Query.UseResultKeys) || // already have list
			 ResultsFormat.SessionOutputFormContext) && !QueryEngineStatsForm.ShowStats)
			{
				Progress.Show("Retrieving data...", UmlautMobius.String, true, "Cancelling retrieval...");
			}

			else // normal type search
			{
				Progress.Show("Searching database - 0:00", UmlautMobius.String, true, "Cancelling search...");
				if (QueryEngineStatsForm.ShowStats)
				{
					QueryEngineStatsForm.StartNewQueryExecution(Query);
					QueryEngineStatsForm.StartingSearch();
				}
			}

			// Wait until results available or the query is cancelled by the user

			while (true)
			{
				Thread.Sleep(100);
				Application.DoEvents();

				if (QueryResultsAvailable) // completed normally
				{
					ResultsKeys = QueryEngine.GetKeys();
					if (saveHitlist) // store for session manager also (may differ but probably shouldn't)
						SessionManager.CurrentResultKeys = ResultsKeys;
					//Progress.Hide();
					break;
				}

				else if (QueryEngineException != null)
				{
					Progress.Hide();
					if (QueryEngineException is QueryException ||
						QueryEngineException is UserQueryException) throw QueryEngineException;
					else throw new Exception(QueryEngineException.Message, QueryEngineException);
				}

				else if (Progress.CancelRequested)
				{
					if (QueryEngine != null) QueryEngine.Cancel(false); // start the cancel
					Thread.Sleep(250);
					Application.DoEvents();
					if (executeQueryThread != null) executeQueryThread.Abort(); // kill the local thread executing the query
					Progress.Hide();
					return false;
				}
			}

			// If the query contains tables marked for remapping then build new expanded query & use going forward

			// modifiedQuery = QueryEngine.DoPresearchChecksAndTransforms(Query); // do any presearch transforms

			OriginalQuery = Query;
			if (TransformedQuery != null)
			{
				//ResultsPages qrp = query.ResultsPages;
				//ResultsPages mqrp = modifiedQuery.ResultsPages; // use same set of results pages so view changes propagate back to original query
				Query.PresearchDerivedQuery = TransformedQuery; // link original query to the transformed query
				Query = TransformedQuery; // replace original query with this query
			}

			else Query.PresearchDerivedQuery = null;

			InitializeQueryManager(QueryManager, Query, QueryManager.ResultsFormat, QueryEngine, ResultsKeys);

			// Save the hit list as needed

			if (saveHitlist)
			{
				CidList currentList = new CidList(ResultsKeys);
				CidListCommand.WriteCurrentList(currentList);
				SessionManager.DisplayCurrentCount();
			}

			return true;
		}

		/// <summary>
		/// Setup the QueryManager members for the specified query 
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="q"></param>
		/// <param name="qe"></param>
		/// <param name="resultsKeys"></param>

		public static void InitializeQueryManager(
			QueryManager qm,
			Query q,
			OutputDest outputDest,
			QueryEngine qe,
			List<string> resultsKeys)
		{
			ResultsFormat rf = new ResultsFormat(qm, outputDest);
			InitializeQueryManager(qm, q, rf, qe, resultsKeys);
			return;
		}

/// <summary>
/// Setup the QueryManager members for the specified query 
/// </summary>
/// <param name="qm"></param>
/// <param name="q"></param>
/// <param name="rf"></param>
/// <param name="qe"></param>
/// <param name="resultsKeys"></param>

		public static void InitializeQueryManager(
			QueryManager qm,
			Query q,
			ResultsFormat rf,
			QueryEngine qe,
			List<string> resultsKeys)
		{
			qm.Initialize(q, rf, qe, resultsKeys);

			if (q.InaccessableData != null)
			{
				if (SS.I.Attended) QbUtil.ShowUnavailableDataMessage(q);
				q.InaccessableData = null;
			}

			return;
		}

		/// <summary>
		/// (Obsolete)
		/// Examine query & expand any tables marked for remapping
		/// Existing remap place holder tables are removed & expansion tables
		/// are added at the end of the query in alphabetical order sorted across
		/// all added tables.
		/// </summary>
		/// <returns></returns>
		/// <param name="qm">QueryManager</param>
		/// <param name="q">Original Query</param>
		/// <param name="qe">Original QueryEngine</param>
		/// <param name="resultsKeys">Search-step results keys to consider</param>
		/// <returns>Modified Query</returns>

		public static Query RemapTablesForRetrieval(QueryManager qm, Query q, QueryEngine qe, List<string> resultsKeys)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Thread method that executes query & return when results are available or an exception occurs
		/// </summary>

		public void ExecuteQueryThreadMethod()
		{
			QueryEngine = new QueryEngine();
			ResultsKeys = null;
			QueryResultsAvailable = false;
			QueryEngineException = null;

			//QueryEngine.ParentQe = this.ResultsFormat.ParentQe;
			//QueryEngine.DebugLevel = SS.I.DebugQueryEngineLevel;
			//CheckForCancel cfc = new CheckForCancel();
			//QueryEngine.CheckForCancel = cfc;
			//QueryEngine.UseDistinctInSearch = SS.I.UseDistinctInSearch;
			try // do key search part of query 
			{
				ResultsKeys = QueryEngine.TransformAndExecuteQuery(Query, out TransformedQuery);
				QueryResultsAvailable = true;
				return;
			}
			catch (Exception ex)  // some exceptions are normal, e.g. no criteria, others may be bugs
			{
				QueryEngineException = ex;
				return;
			}
		}

		/// <summary>
		/// Add a query and it's result list to session query history
		/// </summary>
		/// <param name="query"></param>
		/// <param name="cnList"></param>

		public static void AddToHistoryList(
			Query query,
			CidList cnList)
		{
			HistoryItem hi = new HistoryItem();
			hi.DateTime = DateTime.Now;

			Query q2 = query.Clone(); // clone for possible name change
			string name = q2.UserObject.Name;
			string pattern = @" - [0-9]+\:[0-9]+\:[0-9]+ (AM|PM)$"; // capture long time
			name = Regex.Replace(name, pattern, "", RegexOptions.IgnoreCase); // and remove if present

			q2.UserObject.Name = name;

			hi.QueryName = q2.UserObject.Name;
			hi.QueryFileName = TempFile.GetTempFileName(ClientDirs.TempDir, ".qry");
			StreamWriter sw = new StreamWriter(hi.QueryFileName);
			string queryText = q2.Serialize();
			sw.Write(queryText);
			sw.Close();

			hi.ListFileName = TempFile.GetTempFileName(ClientDirs.TempDir, ".lst");
			sw = new StreamWriter(hi.ListFileName);
			string listText = cnList.ToMultilineString();
			sw.Write(listText);
			sw.Close();
			hi.ListCount = cnList.Count;

			SS.I.History.Add(hi);
		}

		/// <summary>
		/// Update table usage stats
		/// </summary>
		/// <param name="query"></param>

		public static void UpdateTableUsageStatistics(
			Query query)
		{
			int count, ti, ci;
			QueryTable qt;
			QueryColumn qc;

			if (SS.I.QueryTestMode) return; // don't do if test mode

			for (ti = 0; ti < query.Tables.Count; ti++) // update table usage stats
			{
				qt = query.Tables[ti];
				string mtName = qt.MetaTable.Name;
				if (SS.I.TableUsageStatistics.ContainsKey(mtName))
					count = SS.I.TableUsageStatistics[mtName];
				else count = 0;
				SS.I.TableUsageStatistics[qt.MetaTable.Name] = count + 1;

				for (ci = 0; ci < qt.QueryColumns.Count; ci++) // count structure search types
				{
					qc = qt.QueryColumns[ci];
					if (qc.MetaColumn == null) continue;
					if (qc.MetaColumn.DataType != MetaColumnType.Structure) continue;
					if (qc.Criteria == "") continue;

					ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
					if (psc != null && psc.Op != null && psc.Op.Length >= 1)
					{
						string txt = psc.Op;
						if (!Char.IsLetter(txt[0])) txt = "FSS " + txt;
						else if (Lex.Eq(txt, "msimilar")) // include type of 
						{
							int i1 = psc.Value2.IndexOf(" ");
							if (i1 >= 0) psc.Value2 = psc.Value2.Substring(0, i1);
							txt += " " + psc.Value2;
						}
						txt = "StrSrch " + txt;
						UsageDao.LogEvent(txt);
					}
				}
			}
		}

		/// <summary>
		/// Get the actual key for the table.  Use the parent because somtimes the child table uses a different name
		/// (e.g. CID vs CORP_CID)
		/// </summary>
		/// <param name="queryTable"></param>
		/// <returns>Parent Key for the QueryTable</returns>
		private static string GetParentKey(QueryTable queryTable)
		{
			string parentKey;
			if (queryTable.MetaTable.Parent != null)
			{
				MetaTable parentMetaTable = queryTable.MetaTable.Parent;
				parentKey = parentMetaTable.KeyMetaColumn.ColumnMap;
			}
			else
			{
				parentKey = queryTable.KeyQueryColumn.MetaColumn.ColumnMap;
			}
			return parentKey;
		}

	} // end of QueryExec

} 
