

using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.CdkMx;
using Mobius.CdkSearchMx;
using Mobius.MolLib1;

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// QueryEngine class data and top level ExecuteQuery method
	/// File: QueryEngine.cs
	/// </summary>

	public partial class QueryEngine : IDataSource, ICheckForCancel
	{
		public int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public QueryEngine ParentQe; // Parent Qe to piggyback off of

		public Query Query;
		public string QueryMql { get { return MqlUtil.ConvertQueryToMql(Query); } } // MQL for query
		public QueryLogicType MqlLogicType = QueryLogicType.Unknown; // note: may be complex even if Query logic type is And or OR
		public bool NotLogic = false; // true if any "not" logic in query
		public bool MustUseEquiJoins = true; // if true use equi-joins between tables. May be false for simple And logic
		public int TotalColumnsSelected = 0;
		public int TotalCriteria = 0;
		public List<QueryTable> TablesWithCriteria; // list of tables with criteria
		public List<MqlToken> CritToks = null; // semiprocessed complex criteria token list
		public List<MqlToken> CritToksForRoot = null; // criteria tokens adjusted for for current root
		public CompareOp KeyCompareOp = CompareOp.Unknown; // key comparison operator
		public List<int> KeyCriteriaPositions; // token positions for key column references in criteria
		public List<string> KeyCriteriaSavedListKeys; // list of keys in saved list in criteria
		public List<string> KeyCriteriaInListKeys; // list of keys in embedded in in-list criteria
		public List<int> KeyCriteriaConstantPositions; // token positions of key constant values
		public List<int> StructCriteriaList; // list of positions of structure criteria queryColumn references
		public List<int> NonSqlStructCriteriaList; // list of positions of non-Sql criteria

		public QueryTableData[] Qtd;
		public List<string> ResultsKeys; // keys for most recent query
		public Dictionary<string, object> ResultsKeysDict; // hash of keys with any selected sim scores or more-detailed search results

		public List<MetaColumn> ResultsMetaColumns; // metacolumns for most recent query (flattened)
		public List<MetaTable> RootTables; // metatables that are included in root
		public bool ReturnHierarchicalResults = false; // if false return flattened results
		public bool UseDistinctInSearch = true; // if true use distinct keyword in search part of query
		public Dictionary<string, MultiTablePivotBrokerTypeData> MetaBrokerStateInfo; // global metabroker data indexed by source identifier or broker type
		public bool ExecuteSearchStep = true; // if true execute the search step 
		public int SearchSubstep = 0; // current substep of search
		public List<GenericMetaBroker> SearchBrokerList; // list of brokers used in search step
		public bool QeSingleStepExecution = false; // true if executing as a single step
		public QueryTable DrivingTable = null; // the driving table for a single-step execution
		public int DrivingTableIdx = -1; // index of the driving table
		public List<object[]> DrivingTableVos; // buffered vos for driving table
		public object[] DrivingTableNextRow = null; // buffered row containing next key for driving table
		public bool BuildSqlOnly = false; // if true just building SQL not executing
		public bool SkipTablesThatSelectKeyOnly = true; // if only the key is selected and not the first table then don't retrieve data from the table

		public static bool MultithreadRowRetrieval = false; // if true use multithreading for row retrieval 
		public MultithreadLatch RetrieveThreadLatch; // used for synchronizing data retrieval
		public Exception RetrieveTableDataForKeysetException = null; // exception thrown within thread

		public QueryEngineStats Stats = new QueryEngineStats();

		public int NextRowsMinRows = -1; // minRows for current NextRows call
		public int NextRowsMaxRows = -1; // maxRows for current NextRows call
		public int NextRowsMaxTime = -1; // maxTime in ms for current NextRows call
		public Stopwatch NextRowsStopwatch = new Stopwatch(); // Stopwatch time for next rows
		public double NextRowsTotalTime = -1; // total time in ms for NextRows call execution for this QE instance
		public bool NextRowsMaxTimeExceeded() { return NextRowsMaxTime > 0 ? (NextRowsStopwatch.ElapsedMilliseconds > NextRowsMaxTime) : false; }

		public int ChunksRetrievedCount = 0; // total number of chunks retrieved
		public int ChunkKeyCount = 0; // number of keys to retrieve data
		public int ChunkExecuteTime = 0; // execution time for chunk for all tables in query
		public int ChunkRowCount = 0; // number of rows retrieved in chunk
		public int ChunkFetchTime = 0; // time to retrieve rows for chunk for all tables in query
		public int ChunkCompletedTime = 0; // time of day in ms that last chunk was completed

		public string Exprs; // expressions to select
		public string Tables; // tables to be selected from
		public string Criteria; // search criteria
		public string Sql; // full sql
		public List<List<string>> SqlList; // list of sql statements including search and retrieval steps. Each top level list item contains a statement ID followed by the SQL itself

		// QueryEngine state

		public QueryEngineState State = QueryEngineState.Closed; // state of search/retrieval
		public bool CanAttemptToFetchRows => (State == QueryEngineState.Retrieving || State == QueryEngineState.Closed); // true if the QE is in a state where we can attempt to fetch data rows

		int RootTableIdx = -1; // index of current root table
		List<string> KeysForRoot; // set of keys for current root
		int CurrentKeyIdx = -1; //{ get => RowCursor.CurrentKeyIdx; set => RowCursor.CurrentKeyIdx = value; } // index of current key in keyset

		// Set of retrieved rows and cursor on the row set

		public VoArrayList ResultsRows = null; // detailed result rows for most recent query

		public VoArrayListCursor RowCursor = new VoArrayListCursor(); // current cursor position in Result rows
		public int CursorPos { get => RowCursor.Position; set => RowCursor.Position = value; } // index of current row
		object[] CurrentRow { get => RowCursor.CurrentRow; set => RowCursor.CurrentRow = value; }

		// QueryEngine static/constants

		public static Dictionary<int, QueryEngine> IdToInstanceDict = new Dictionary<int, QueryEngine>(); // maps QE Ids to their corresponding instances 

		public static int ChunkKeyCountInitial = 1;
		public static int ChunkKeyCountNextMultiplier = 10;
		public static int ChunkKeyCountMax = 1000;

		//public static int ChunkKeyCountFirst = 1; // obsolete
		//public static int ChunkKeyCountSecond = 100; // obsolete
		//public static int ChunkKeyCountRest = 1000; // obsolete

		public static bool MinimizeDBLinkUse = true; // minimize use of database links to improve performance
		public static bool AllowNetezzaUse = false; // allow use of warehouse applicance (i.e. Netezza for session)
		public static bool VerifyImpreciseStructureSearches = true;
		public static bool AllowPrefetchOfImages = false; // turn this on has some delay issues and doesn't really seem to add value
		public static bool LogCurrentQueryByProcessId = false; // if true log current query by process id

		public static Dictionary<int, string> LoggedQueries = new Dictionary<int, string>(); // dictionary of error messages keyed by query id
		public static List<QueryEngineStats> SessionStats = new List<QueryEngineStats>(); // execution stats for all query engines for session
		public static bool ReturnBrokerStats = false; // if true return broker stats with each NextRow(s) request

		public static List<string> DatabaseSubset = null; // global subset to apply against queries
		public static bool AllowMultiTablePivot = true; // flag for allowing in-code pivoting of multiple tables in a single SQL statement
		public static bool FilterAllDataQueriesByDatabaseContents = true; // flag for contents filtering of all data queries

		public static bool PrefetchStoredHelmSvgImagesEnabled = true;

		public static bool LogBasics = false;
		public static bool LogDetail = false;
		public static bool LogDataRows = false;

		public static bool InitializedForSession = false; 

		ICheckForCancel _checkForCancel; // ref to this Qe class instance to call to check for cancel
		public ICheckForCancel CheckForCancel
		{
			get { return _checkForCancel; }
			set { _checkForCancel = value; }
		}

		bool _cancelRequested = false; // set to true if query execution cancelled
		public bool CancelRequested
		{
			get { return _cancelRequested; }
			set { _cancelRequested = value; }
		}

		bool _cancelled = false; // set to true if query execution cancelled
		public bool Cancelled
		{
			get { return _cancelled; }
			set { _cancelled = value; }
		}

		bool LoggedQueryException = false;

		/**************************************************************************/
		/******************************** Methods *********************************/
		/**************************************************************************/

		/// <summary>
		/// Constructor
		/// </summary>

		public QueryEngine()
		{
			lock (IdToInstanceDict) // save map of Id to instance
			{
				IdToInstanceDict.Add(Id, this);
			}

			SessionStats.Add(Stats); // include our stats in stats for session

			//if (ClientState.IsDeveloper) // debug
			//{
			//  DebugLevel = 1;
			//  DebugLog.LogFileName = Application.StartupPath + @"\DebugQE.log";
			//}
		}

		/// <summary>
		/// Get an instance by id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public static QueryEngine GetInstanceById(int id)
		{
			if (IdToInstanceDict.ContainsKey(id))
				return IdToInstanceDict[id]; 
			else return null;
		}

		/// <summary>
		/// Do session initialization
		/// </summary>

		public static void InitializeForSession()
		{
			if (InitializedForSession) return;

			MetaBrokerUtil.InitializeGlobalBrokers(); // initialize array of global metabrokers

			// Read in QueryEngine parameters

			MoleculeUtil.ShowStereoComments = ServicesIniFile.ReadBool("ShowStereoComments", false);
			MoleculeUtil.EnhanceStructureDisplay = ServicesIniFile.ReadBool("EnhanceStructureDisplay", true);
			MoleculeUtil.ScaleBondWidth = ServicesIniFile.ReadBool("ScaleBondWidth", true);
			MoleculeUtil.SaltGroupingEnabled = ServicesIniFile.ReadBool("ActivateSaltSiblings", true);

			if (MoleculeMx.HelmEnabled == null) // set value if not already defined
				MoleculeMx.HelmEnabled = ServicesIniFile.ReadBool("HelmEnabled", true);

			ServicesIniFile.TryReadBool("PrefetchStoredHelmSvgImages", ref QueryEngine.PrefetchStoredHelmSvgImagesEnabled);

			ServicesIniFile.TryReadBool("LogQeBasics", ref QueryEngine.LogBasics);
			ServicesIniFile.TryReadBool("LogQeDetail", ref QueryEngine.LogDetail);
			ServicesIniFile.TryReadBool("LogQeDataRows", ref QueryEngine.LogDataRows);

			ServicesIniFile.TryReadBool("LogDbCommandDetail", ref DbCommandMx.LogDbCommandDetail);
			ServicesIniFile.TryReadBool("LogDbConnectionDetail", ref DbConnectionMx.LogDbConnectionDetail);

			//ServicesIniFile.TryReadInt("ChunkKeyCountMinimum", ref ChunkKeyCountMinimum);

			ServicesIniFile.TryReadInt("ChunkKeyCountInitial", ref ChunkKeyCountInitial);
			ServicesIniFile.TryReadInt("ChunkKeyCountNextMultiplier", ref ChunkKeyCountNextMultiplier);
			ServicesIniFile.TryReadInt("ChunkKeyCountMax", ref ChunkKeyCountMax);

			//ServicesIniFile.TryReadInt("ChunkKeyCountFirst", ref ChunkKeyCountFirst);
			//ServicesIniFile.TryReadInt("ChunkKeyCountSecond", ref ChunkKeyCountSecond);
			//ServicesIniFile.TryReadInt("ChunkKeyCountRest", ref ChunkKeyCountRest);

			MultithreadRowRetrieval = ServicesIniFile.ReadBool("MultithreadRowRetrieval", false);

			AllowMultiTablePivot = ServicesIniFile.ReadBool("AllowMultiTablePivot", true);
			if (!AllowMultiTablePivot) // if no general multipivot then no specific multipivot either
			{
				AnnotationMetaBroker.AllowMultiTablePivot_Annotation =
				AssayMetaBroker.AllowMultiTablePivot_Assay = false;
			}

			else
			{
				AnnotationMetaBroker.AllowMultiTablePivot_Annotation = ServicesIniFile.ReadBool("AllowMultiTablePivot_Annotation", AnnotationMetaBroker.AllowMultiTablePivot_Annotation);
				//AssayMetaBroker.AllowMultiTablePivot_Assay = ServicesIniFile.ReadBool("AllowMultiTablePivot_Assay", AssayMetaBroker.AllowMultiTablePivot_Assay);
			}

			RelatedStructureSearch.InitializeForSession();
			CdkSimSearchMx.InitializeForSession();
			SmallWorldDao.InitializeForSession();

			AllowNetezzaUse = ServicesIniFile.ReadBool("AllowNetezzaUseForQueryEngine", false);
			VerifyImpreciseStructureSearches = ServicesIniFile.ReadBool("VerifyImpreciseStructureSearches", true);
			LogCurrentQueryByProcessId = ServicesIniFile.ReadBool("LogCurrentQueryByProcessId", false);

			// Create an initial empty current list for the session

			UserObject uo = new UserObject(UserObjectType.CnList, Security.UserName, UserObject.TempFolderName, "Current");
			int uoId = UserObjectDao.Write(uo);

			InitializedForSession = true;

			return;
		}

		/// <summary>
		/// Close the query engine
		/// </summary>

		public void Close()
		{
			if (ParentQe != null) // using parent Qe?
			{
				State = QueryEngineState.Closed;
				return;
			}

			// Note that: 
			//  1. Qe.GetImage may be called asynchronously to get images even after close
			//  2. State = QueryEngineState.Closed is set elsewhere independently within the qe

			try
			{
				int ti;
				for (ti = 0; ti < Query.Tables.Count; ti++)
				{
					if (Qtd[ti].Broker != null)
						Qtd[ti].Broker.Close();
					Qtd[ti].Closed = true;
				}

				if (RowCursor != null)
					RowCursor.Close();

				if (ResultsRows != null)
					ResultsRows.CloseCaching();
			}

			catch (Exception ex) // just log error
			{
				int qid = -1;
				if (Query != null) qid = Query.UoId;
				DebugLog.Message("Warning: Error closing QueryEngine: " + Id + ", QueryId: " + qid + "\r\n" + DebugLog.FormatExceptionMessage(ex));
			}

			State = QueryEngineState.Closed;
		}

		/// <summary>
		/// Remove QE instance from dictionary so it can be reclaimed by GC
		/// </summary>

		public void Dispose()
		{
			bool disposed = Dispose(Id);
			return;
		}

		/// <summary>
		/// Remove QE instance from dictionary so it can be reclaimed by GC
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool Dispose(int id)
		{
			lock (IdToInstanceDict) // remove from map of Id to instance
			{
				if (IdToInstanceDict.ContainsKey(id))
				{
					IdToInstanceDict.Remove(id);
					return true;
				}

				else return false;
			}
		}


		/// <summary>
		/// Execute Mql query
		/// </summary>
		/// <param name="mql"></param>
		/// <returns>
		/// </returns>

		public List<string> ExecuteQuery(
			string mql)
		{
			List<string> hitList = ExecuteQuery(mql, false);
			return hitList;
		}

		/// <summary>
		/// Execute Mql query in single or multiple steps
		/// </summary>
		/// <param name="mql"></param>
		/// <param name="singleStepExecution"></param>
		/// <returns></returns>

		public List<string> ExecuteQuery(
			string mql,
			bool singleStepExecution)
		{
			Permissions.AllowTemporaryPublicReadAccessToAllUserObjects = true;

			Query q = MqlUtil.ConvertMqlToQuery(mql);
			q.SingleStepExecution = singleStepExecution;
			List<string> hitList = ExecuteQuery(q);
			Permissions.AllowTemporaryPublicReadAccessToAllUserObjects = false;

			return hitList;
		}

		/// <summary>
		/// Execute query
		/// </summary>
		/// <param name="query"></param>
		/// <returns>
		/// List of results keys
		/// </returns>
		/// 

		public List<string> ExecuteQuery(
			Query query)
		{
			return ExecuteQuery(query, false, false);
		}

		/// <summary>
		/// Setup and execute the initial search step of the supplied query.
		/// This method returns the set of matching key values which are then
		/// used in the subsequent retrieval step.
		/// 
		/// In some cases the search step is not needed or requested (e.g. preview query
		/// or single-step query) in which case no keys are returned from this step
		/// 
		/// This method can optionally returning after building the sql for the search step without executing it.
		/// </summary>
		/// <param name="query">The query to execute</param>
		/// <param name="buildSqlOnly">Just build the search and retrieval SQL without executing</param>
		/// <param name="buildSingleSqlOnly">Build single-step SQL without executing</param>
		/// <returns>List of matching key values</returns>

		public List<string> ExecuteQuery( 
			Query query,
			bool buildSqlOnly,
			bool buildSingleSqlOnly)
		{
			MetaTable mt = null;

			List<string> existingResultKeysToReuse = null; 
			//List<string> resultKeys = null; // set of keys found in search

			ExecuteQueryParms eqp;
			GenericMetaBroker mb = null;
			string errorMsg;
			double d1;
			object[] vo;
			int ti, ci, errorPosition, tExecute, tDelta, i1;

			try
			{

				//query.FilterNullRows = false; // debug
				//query.FilterNullRowsStrong = false; // debug

				// Check validity

				Query = query;
				State = QueryEngineState.New;
				BuildSqlOnly = buildSqlOnly; // set global var so all brokers know as well

				if (query == null)
					throw new QueryException("Null Query");

				if (query.Tables.Count == 0)
					throw new QueryException("Empty query");

				query.ValidateAggregation();

				if (LogBasics)
					DebugLog.Message("====== ExecuteQuery" +
						", Id: " + Id +
						", tables: " + query.Tables.Count +
						", singleStepExecution: " + query.SingleStepExecution +
						", keySortDirection: " + query.KeySortOrder +
						", useResultKeys: " + query.UseResultKeys + " ======");

				Stats.ExecutionCount++;
				Stats.QueryId = query.UserObject.Id;
				Stats.Tables = query.Tables.Count;
				Stats.Columns = query.GetSelectedColumnCount();
				Stats.Criteria = query.GetCriteriaCount();
				Stats.BrokerTypeDict = query.CountTablesByBrokerType();

				if (LogCurrentQueryByProcessId)
					LogQueryByProcessId(query);

				ChunksRetrievedCount = 0;
				SearchSubstep = 0;

				//			if (this.Conn == null)
				//				throw new QueryException("No database connection");

				// If working off of existing query engine instance then do setup
				// Note that once a query engine instance is cancelled it will only 
				// return records up to the point of the cancel. A base level query 
				// must be rerun to retrieve the full set.

				if (ParentQe != null)
				{
					this.Query = query;
					this.TotalColumnsSelected = ParentQe.TotalColumnsSelected;
					this.TotalCriteria = ParentQe.TotalCriteria;
					this.Qtd = ParentQe.Qtd;
					this.Cancelled = false;
					this.CancelRequested = false;
					ParentQe.Cancelled = false;
					for (ti = 0; ti < query.Tables.Count; ti++) // clear broker cancel flags
					{
						if (Qtd[ti].Broker != null)
							Qtd[ti].Broker.GetExecuteQueryParms.Cancelled = false;
					}

					this.ResultsMetaColumns = ParentQe.ResultsMetaColumns;
					State = QueryEngineState.Retrieving; // set our state
					CursorPos = -1;
					ResultsKeys = ParentQe.ResultsKeys;
					ResultsKeysDict = ParentQe.ResultsKeysDict;
					return ResultsKeys;
				}

				// Check for tables whose datasources are not available

				Dictionary<string, List<string>> inaccessableData = CheckDataSourceAccessibility(query);

				if (inaccessableData.Count > 0)
				{
					string message =
						"The query cannot be executed because it accesses the\n" +
						"following currently unavailable data sources:\n";

					foreach (string s in inaccessableData.Keys)
					{
						message += "\n" + s + ":"; // datasource name or message
						foreach (string mtName in inaccessableData[s]) // list each table in source
						{
							mt = MetaTableCollection.Get(mtName);
							if (mt != null)
								message += "\n   " + mt.Label + " (" + mt.Name + ")";
						}
					}

					throw new UserQueryException(message);
				}

				DoPresearchInitialization(); // Do any presearch initialization for the QueryTables in a query

				AnalyzeQuery(); // do initial analysis of query

				// Init results data

				ResultsRows = null;
				ResultsKeys = null;
				ResultsKeysDict = null;
				CheckForCancel = this; // call back to us to check for cancel request
				CancelRequested = false;
				Cancelled = false;
				State = QueryEngineState.Searching;

				// Decide if query should be executed as a normal two step process or as a single step.
				// Queries not executing the search step can be either preview queries or queries marked
				// for SingleStepExecution or multitable queries marked to use incoming result keys.

				ExecuteSearchStep = true;
				QeSingleStepExecution = false;

				// If reusing existing set of result keys then don't redo search part

				if (Query.UseResultKeys) // try to use existing set of results keys
				{
					Query.UseResultKeys = false; // turn off since this is a single use flag

					if (Query.ResultKeys != null) // have keys?
					{
						existingResultKeysToReuse = Query.ResultKeys; // copy reference (need to Clone?)
						ExecuteSearchStep = false;
					}

					else if (!String.IsNullOrEmpty(Query.ResultKeysListName)) // have key list
					{
						CidList cidList = CidListDao.Read(Query.ResultKeysListName);
						if (cidList != null && cidList.Count > 0)
						{
							existingResultKeysToReuse = cidList.ToStringList();
							Query.ResultKeysListName = "";
							ExecuteSearchStep = false;
						}
					}
				}

				// If just previewing then don't do initial search

				if (Query.Preview)
				{
					QeSingleStepExecution = true;
					ExecuteSearchStep = false;
					if (Query.Tables[0].MetaTable.MultiPivot)
						throw new UserQueryException("Can't preview a table that needs to be pivoted during query execution");

					ResultsKeys = null; // no keys
					if (!OpenSingleStepQuery())
					{
						Cancelled = true;
						return null;
					}
				}

				// Single-step execution requested 

				else if (MqlUtil.SingleStepExecution(Query)) // try to do in a single step
				{
					List<string> singleStepQueryKeyList = GetQueryKeyList();
					if (existingResultKeysToReuse != null) singleStepQueryKeyList = existingResultKeysToReuse; // result keys override query key list since they may be a subset

					if (Query.GetTablesWithCriteriaCount(false, false) <= 1 && // must be only one table with non-key criteria
						Query.GetTablesRemappedCount() == 0 && // no table remapping
						RootTables.Count <= 1 && // and only one root table to qualify
						NonSqlStructCriteriaList.Count == 0) // no non-Oracle criteria allowed
					{
						QeSingleStepExecution = true;
						ExecuteSearchStep = false;
						ResultsKeys = singleStepQueryKeyList; // provide any subset of keys
						if (!OpenSingleStepQuery())
						{
							Cancelled = true;
							return null;
						}

						ResultsKeys = new List<string>(); // accumulate keys later during retrieval
						ResultsKeysDict = new Dictionary<string, object>();
					}

					else // if we have a set of result keys & no other criteria then use those keys & skip search
					{
						if (singleStepQueryKeyList != null && singleStepQueryKeyList.Count > 0 && TotalCriteria == 1)
						{
							ResultsKeys = singleStepQueryKeyList; // use these keys as result of search
							ExecuteSearchStep = false;
						}
					}

				}

				// No need to execute if no criteria && no QE data retrieved

				else if (!Query.RetrievesDataFromQueryEngine && TotalCriteria == 0) 
				{
					ResultsKeys = new List<string>(); // accumulate keys later during retrieval
					ResultsKeysDict = new Dictionary<string, object>();
					ExecuteSearchStep = false;
				}

				// Perform normal search step if requested

				if (ExecuteSearchStep)
				{
					ResultsKeys = ExecuteNormalSearchStep(query, BuildSqlOnly, buildSingleSqlOnly);
				}

				else if (existingResultKeysToReuse != null) //added the else if because alerts with lists were skipping the result keys
				{
					ResultsKeys = existingResultKeysToReuse;
				}

				ResultsRows = new VoArrayList(); // results will go here

				// Setup retrieval for first root table

				if (QeSingleStepExecution)
					RootTableIdx = 0; // set to first & only root, driving table already open
				else RootTableIdx = -1;

				KeysForRoot = new List<string>();
				CurrentKeyIdx = 0;

				ChunkKeyCount = 0;

				Query.SetTableIndexes(); 

				State = QueryEngineState.Retrieving;

				if (ResultsKeys != null) Stats.KeyCount = ResultsKeys.Count;

				return ResultsKeys;
			}

			catch (UserQueryException ex) // standard query error
			{
				//LogExceptionAndSerializedQuery(ex); // temporary
				throw new UserQueryException(ex.Message, ex); // pass it along
			}

			catch (Exception ex) // unexpected query error
			{
				string msg = ex.Message;
				msg += " " + LogExceptionAndSerializedQuery(ex);
				//SystemUtil.Beep(); // debug
				throw new Exception(msg, ex); // pass it along
			}

		}

		/// <summary>
		/// Execute the search step to get the set of matching keys to be used later in the data retrieval step
		/// </summary>
		/// 

		private List<string> ExecuteNormalSearchStep( 
			Query query,
			bool buildSqlOnly,
			bool buildSingleSqlOnly)
		{
			List<MqlToken> sourceCriteriaToks = null, nonSsCriteriaToks = null;
			List<CriteriaList> ssCriteriaLists;

			List<string> initialSearchKeys = null; // subset of key values to restrict search by
			List<string> resultKeys = null; // set of keys found in search
			Dictionary<string, object> structMatchScoresForRoot;

			List<CriteriaList> criteriaSubsets = null; // optimized list of criteria subsets to be executed in order
			List<LogicGroup> logicGroups = null; // list of groups of subsets with associated group logic type. Only complex logic queries have more than one group

			QueryTable qt = null, qt2 = null;
			QueryColumn qc;
			MetaTable mt = null, mt2, rootTable;
			MetaColumn mc = null;
			ExecuteQueryParms eqp;
			GenericMetaBroker mb = null;
			string errorMsg;
			DateTime t0, tKeyFetchStart;
			double d1;
			object[] vo;
			int ti, ci, errorPosition, tExecute, tDelta, i1;

			if (TotalCriteria == 0) // Be sure we have at least one criteria
				throw new UserQueryException("You must define at least one criteria for the query");

			// Determine any special handling for explicit lists of keys

			//if (LogicType != QueryLogicType.And || NotLogic)
			//  KeyCriteriaInListKeys = null; // keep keys in order only if simple "and" logic

			//else if (KeyCriteriaInListKeys != null && KeyCriteriaInListKeys.Count > 1000 &&
			//  KeyCriteriaSavedListKeys == null)
			//{ // if simple logic && In List key count > 1000 then disable key criteria with MQL to avoid Oracle error
			//  CritToks = // KeyCriteriaInListKeys are handled as with saved list for proper execution
			//    MqlUtil.DisableCriteria(CritToks, query, null, null, null, null, false, false, false);
			//}

			// Disable non-search criteria and get list of the tables that have criteria

			sourceCriteriaToks = // disable non-search criteria
			 MqlUtil.DisableCriteria(CritToks, query, null, null, null, null, true, false, true);

			Dictionary<string, QueryTable> criteriaTables = // get criteria tables with "real" criteria
				MqlUtil.GetCriteriaTables(query, sourceCriteriaToks, doFullErrorCheck: true); 

			// Get list of key values from any key criteria

			if (KeyCriteriaSavedListKeys == null && KeyCriteriaInListKeys == null) // any key criteria?
			{
				if (DatabaseSubset != null) // limit to any database subset if no key criteria
					initialSearchKeys = DatabaseSubset;
			}

			else // key list criteria, get the set of ids and intersect with any database subset
			{
				if (KeyCriteriaSavedListKeys != null)
					initialSearchKeys = KeyCriteriaSavedListKeys;
				else initialSearchKeys = KeyCriteriaInListKeys;

				if (initialSearchKeys != null && DatabaseSubset != null) // intersect keySubset with database subset
				{
					List<string> keySubset2 = new List<string>();
					foreach (string key in initialSearchKeys)
					{ // do binary search of sorted database subset
						if (DatabaseSubset.BinarySearch(key) >= 0)
							keySubset2.Add(key);
					}
					initialSearchKeys = keySubset2;
				}

				if (initialSearchKeys == null || initialSearchKeys.Count == 0) // be sure something is there
				{
					State = QueryEngineState.Closed;
					return new List<string>();
				}

				if (criteriaTables.Count == 0) // just key criteria
				{
					resultKeys = initialSearchKeys; // copy subset to keyset for query
					resultKeys = RestrictedDatabaseView.FilterOutRestrictedKeys(resultKeys);
					return resultKeys; // done with search part of query
				}
			}

// Break criteria into separate subqueries to optimize search response for each logic type

			CriteriaList sourceCriteria = new CriteriaList(sourceCriteriaToks);
			List<QueryColumn> specialSsCriteria = BuildSpecialStructureSearchCriteriaList(query, criteriaTables, sourceCriteriaToks);

			if (Query.LogicType == QueryLogicType.And) // Do "And" logic optimizations
				OptimizeAndLogic(query, criteriaTables, sourceCriteria, out criteriaSubsets, out logicGroups);

			else if (Query.LogicType == QueryLogicType.Or) // Do "Or" logic optimizations
				OptimizeOrLogic(query, criteriaTables, sourceCriteria, out criteriaSubsets, out logicGroups);

			else if (Query.LogicType == QueryLogicType.Complex)
				OptimizeComplexLogic(query, criteriaTables, sourceCriteria, out criteriaSubsets, out logicGroups);

			AdjustSpecialStructureSearchCriteria(query, specialSsCriteria, criteriaSubsets, logicGroups);

			AdjustNotExistsCriteria(query, criteriaSubsets, logicGroups);
			
			if (LogDetail) // debug
			{
				string logicTokenString = MqlUtil.CatenateCriteriaTokensForDebug(sourceCriteriaToks);
				string criteriaSubsetsString = CriteriaList.ListsToString(criteriaSubsets);
				string logicTreeString = logicGroups[0].ToString();
				string breakPoint = "";
			}

////////////////////////////////////////////////////////////////////////////////////////////////////
// Begin search:
// Loop through search part of query for each root table in query appending keys
// that are retrieved as hits for each root.
// The "Select" part of the Sql is generated for each querytable with criteria by the associated
// metabroker. Tables are joined on key values and "anded" together with 
// the complex criteria. Equijoins on the keys are done unless there are 
// some "or" conditions in which case outer joins are done.
////////////////////////////////////////////////////////////////////////////////////////////////////

			resultKeys = new List<string>(); // accumulate result keys here
			ResultsKeysDict = new Dictionary<string, object>(); // use to identify duplicates & store sim scores
			SearchBrokerList = new List<GenericMetaBroker>();
			QueryColumn selectedSimScoreQc = null; // column with selected similarity search score

			for (int rti = 0; rti < RootTables.Count; rti++) // execute for each root table
			{
				rootTable = RootTables[rti]; // current root

				List<string> currentRootSearchKeySubset = CompoundId.FilterKeysByRoot(initialSearchKeys, rootTable);
				if (currentRootSearchKeySubset != null && currentRootSearchKeySubset.Count == 0)
					continue; // if subsetting but no keys for this root then continue to next root

				structMatchScoresForRoot = new Dictionary<string, object>();

				LogicGroup rootLogicGroup = logicGroups[0]; // final results end up here

				foreach (LogicGroup lgO in logicGroups) // reset logic groups for reexecution
				{
					lgO.ElementsProcessed = 0;
					lgO.ResultKeys = null;
					lgO.SearchKeySubset = currentRootSearchKeySubset;
				}

				////////////////////////////////////////////////////////////////////////////////////////////////////
				// Execute a query for each criteriaSubset and combine the results appropriately
				// depending on the type of query logic (And, Or, Complex)
				////////////////////////////////////////////////////////////////////////////////////////////////////

				for (int csi = 0; csi < criteriaSubsets.Count; csi++)
				{
					CriteriaList criteriaSubset = criteriaSubsets[csi];
					List<MqlToken> subsetToks = criteriaSubset.Tokens;

					LogicGroup logicGroup = criteriaSubset.ParentLogicGroup;
					if (logicGroup == null) DebugMx.DataException("LogicGroup == null");

					QueryLogicType logicGroupLogicType = logicGroup.LogicType; // logic type for logic group, may differ from qeLogicType if query uses complex logic

					// If logic group uses "And" logic an we are past the first element and there are is nothing left in the ResultKeys then just proceed to the next subset until we reach the end

					bool noRemainingResults = (logicGroup.ResultKeys == null || logicGroup.ResultKeys.Count == 0);
					if (buildSqlOnly) noRemainingResults = false; // force build all steps

					if (logicGroupLogicType == QueryLogicType.And && criteriaSubset != logicGroup.ElementList[0] && noRemainingResults)
					{
						IfLastElementOfLogicGroupPassKeysUp(criteriaSubset, logicGroup);
						continue;
					}

					criteriaTables = MqlUtil.GetCriteriaTables(query, subsetToks);
					if (criteriaTables == null || criteriaTables.Count == 0) DebugMx.DataException("No criteria tables in criteria subset: " + criteriaSubset.ToString());

					if (criteriaTables.Count > 1 && // be sure 1st table included if more than one other table
						logicGroupLogicType != QueryLogicType.And)  // and not AND logic
						criteriaTables[query.Tables[0].Alias] = query.Tables[0];

					bool allowNetezzaUse = // allow Netezza use for query part of search if all criteria tables support Netezza
						AllowNetezzaForAllTables(criteriaTables.Values.ToList());

					QueryTable firstQueryTable = null; // first query table for this root
					string firstKeyFieldName = null;

					Exprs = "";
					Tables = "";
					Criteria = "";
					string tableNameList = "";
					int subsetCriteriaCount = 0;

					bool criteriaNotAssociatedWithThisRoot = false; // flag if we see criteria from table not associated with current root
					for (ti = 0; ti < query.Tables.Count; ti++) // get sql for each table
					{
						qt = query.Tables[ti];
						if (!criteriaTables.ContainsKey(qt.Alias) && // ignore if no criteria for table
								!buildSingleSqlOnly) // unless just building full sql
																		 //&& qt.HasNotExistsCriteria) // force tables with NoIsNullCriteria=false into the key search
							continue; // unless just building full sql

						qt2 = qt.MapToCurrentDb(rootTable); // actual query table (modifiable copy always created)
						if (qt2 == null) // this table is not part of the tree for this root
						{
							criteriaNotAssociatedWithThisRoot = true;
							continue;
						}

						subsetCriteriaCount += qt2.GetCriteriaCount(includeKey: false, includeDbSet: false);
						if (Lex.IsDefined(Query.KeyCriteria) ||
						 (logicGroupLogicType == QueryLogicType.And && ti > 0))
							subsetCriteriaCount++; // count key criteria as well if appropriate

						if (firstQueryTable == null)
						{
							firstQueryTable = qt2;
							firstKeyFieldName = // column selected & joined against
								qt2.Alias + "." + qt2.MetaTable.KeyMetaColumn.Name;
							Exprs = firstKeyFieldName; // select key column from 1st table to return

							if (qt2.HasNotExistsCriteria) firstKeyFieldName += "(+) "; // outer join if "is null" criteria

							string simExpr = "";
							selectedSimScoreQc = GetMolSimScoreExpressionIfSelected(qt2, subsetToks, out simExpr);
							if (Lex.IsDefined(simExpr)) Exprs += ", " + simExpr; //add sim score column for selection

							//UseDistinctInSearch = false; // debug

							if (buildSingleSqlOnly) Exprs = ""; // 
							else if (!Lex.Contains(Exprs, ",") && UseDistinctInSearch) Exprs = "distinct " + Exprs; // only need one of each key value if only selecting key value xxx
						}

						qt2.AggregationEnabled = false; // no aggregation during search
						mt2 = qt2.MetaTable;
						mb = MetaBrokerUtil.Create(mt2.MetaBrokerType); // get appropriate broker object

						for (ci = 0; ci < qt2.QueryColumns.Count; ci++)
						{
							qc = qt2.QueryColumns[ci];
							if (qc.MetaColumn == null) continue;

							mc = qc.MetaColumn;
							try
							{
								if (!buildSingleSqlOnly)
								{ // normal search, select only key column and possibly sim score column
									if (mc.IsKey || qc == selectedSimScoreQc) qc.Selected = true;
									else qc.Selected = false;
									qc.SortOrder = 0; // no extra sorting on this pass
									qc.Aggregation = null; // no aggregation on search
								}

								else // building Sql, include all selected cols
								{
									if (!qc.Selected) continue; // just need selected cols
									if (mc.IsKey && Exprs != "") continue; // just need key once
									if (Exprs != "") Exprs += ", ";
									Exprs += qt.Alias + "." + mc.Name + " " + qt.Alias + "_" + mc.Name;
									if (mc.DataType == MetaColumnType.QualifiedNo || // if qn then include value also
										(mc.DetailsAvailable && !mc.IsKey)) // if annotation treat as qn
										Exprs += ", " + qt.Alias + "." + mc.Name + "_val " + qt.Alias + "_" + mc.Name + "_val";
								}
							}
							catch (Exception ex) { continue; } // just ignore?
						}

						eqp = new ExecuteQueryParms(this, qt2);
						eqp.AllowNetezzaUse = allowNetezzaUse; // indicate if netezza use is allowed

						/************************************/
						/*** Build the SQL for this table ***/
						/************************************/

						string tableSql = mb.BuildSql(eqp);

						/************************************/

						i1 = tableSql.ToLower().IndexOf(" from ");
						string fromClause = tableSql.Substring(i1 + 6).Trim();
						//								 if (!fromClause.StartsWith("(")) fromClause = "(" + fromClause + ")";

						if (Tables == "")
						{
							Tables = fromClause;
							tableNameList = mt2.Name;
						}

						else // table beyond first, build key join to first table
						{
							Tables += ", " + fromClause;

							if (Criteria != "") Criteria += " and ";
							Criteria += qt.Alias + "." + qt.MetaTable.KeyMetaColumn.Name + " ";
							if (logicGroupLogicType != QueryLogicType.And || qt.HasNotExistsCriteria)
								Criteria += "(+) "; // outer join if necessary

							Criteria += " = " + firstKeyFieldName;

							tableNameList += ", " + mt2.Name;
						}
					}

					if (criteriaNotAssociatedWithThisRoot)
						continue; // can't have any hits if criteria for tables not associated with root (not necessarily true for complex criteria)

					List<MqlToken> subsetToksForRoot = // plug in proper key col names & constant values
						MqlUtil.TransformKeyCriteriaForRoot(
						 subsetToks,
						 rootTable,
						 KeyCriteriaPositions,
						 KeyCriteriaConstantPositions);

					string complexCriteria = // build the real sql for the query
						MqlUtil.CatenateCriteriaTokens(subsetToksForRoot);

					if (Criteria == "") Criteria = complexCriteria;
					else Criteria += " and (" + complexCriteria + ")";

					// Build full sql

					string hint = "";
					if (logicGroup.SearchKeySubset != null && logicGroup.SearchKeySubset.Count > 0) // if subsetting by keys then optimizer does better with FIRST_ROWS hint (i.e. Star)
						hint = "/*+ first_rows */ "; // note that adding this hint in general will make many other queries slow to a crawl.

					Sql = "Select " + hint + Exprs +
					" From " + Tables +
					" Where " + Criteria;

					if (buildSingleSqlOnly) return null; // return now if just building sql

					////////////////////////////////
					// Execute the search substep
					///////////////////////////////

					// DebugLog.Message("QueryEngine.ExecuteQuery Sql: " + sql); // log the sql

					SearchSubstep++;
					mb = new GenericMetaBroker(); // use generic metabroker to execute search & get keys
					SearchBrokerList.Add(mb); // keep list of brokers used in search
					mb.Label = // build a label with values
					"Search Substep " + SearchSubstep + ":" + tableNameList + ":-1:-1:" + subsetCriteriaCount + ":Search";

					eqp = new ExecuteQueryParms(this);
					eqp.QueryTable = firstQueryTable; // use first query table (just key field from this)
					eqp.CallerSuppliedSql = Sql; // use our sql
					eqp.KeyName = firstQueryTable.Alias;
					if (eqp.KeyName != "") eqp.KeyName += ".";
					eqp.KeyName += firstQueryTable.MetaTable.KeyMetaColumn.Name;

					eqp.SearchKeySubset = logicGroup.SearchKeySubset; // supply starting subset or subset from previous ExecuteQuery
					eqp.CheckForCancel = CheckForCancel;

					// Prepare the query

					t0 = DateTime.Now;
					string searchSql = mb.PrepareQuery(eqp); // here's the real action
					tDelta = (int)TimeOfDay.Delta(ref t0);

					if (eqp.Cancelled)
					{
						Cancelled = true;
						return null;
					}
					Stats.PrepareTime += tDelta;
					if (LogDetail) // bit higher level
					{
						DebugLog.Message("Search Prepare - Time: " + tDelta + ", " +
							"keySubsetCount: " + (eqp.SearchKeySubset != null ? eqp.SearchKeySubset.Count.ToString() : "0") + ", " +
							"sql: " + OracleDao.FormatSql(Sql));
					}

					// Execute the query

					mb.ExecuteQuery(eqp);
					tExecute = (int)TimeOfDay.Delta(ref t0);

					if (buildSqlOnly) continue; // if just building sql all done here for now

					if (eqp.Cancelled)
					{
						Cancelled = true;
						return null;
					}

					Stats.ExecuteTime += tExecute;
					if (LogBasics)
					{
						DebugLog.Message("Search Execute - Time: " + tExecute + ", " +
							"keySubsetCount: " + (eqp.SearchKeySubset != null ? eqp.SearchKeySubset.Count.ToString() : "0")); // + ", sql: " +  Lex.RemoveLineBreaksAndTabs(Sql));
					}

					// Retrieve rows

					if (logicGroupLogicType == QueryLogicType.And) // if "and" logic reset result keys
						logicGroup.ResultKeys = new Dictionary<string, object>();

					t0 = tKeyFetchStart = DateTime.Now;

					List<string> subQueryResultKeys = new List<string>();

					while (true)
					{ // read them in
						vo = mb.NextRow();
						if (eqp.Cancelled)
						{
							Cancelled = true;
							return null;
						}

						if (vo == null) break; // end of cursor
						NormalizeNullValues(vo);

						if (LogBasics && subQueryResultKeys.Count < 2) // show debug info for 1st two rows
						{
							tDelta = (int)TimeOfDay.Delta(ref t0);
							DebugLog.Message("Search Key Fetch - Key " + (subQueryResultKeys.Count + 1) +
								" (" + (vo != null && vo[0] != null ? GetKeyString(vo[0]) : "null") + "), time: " + tDelta);
						}

						if (vo[0] != null)  // add to results if not null 
						{
							string key = GetKeyString(vo[0]);
							if (vo.Length >= 2 && vo[1] != null && // include struct search sim score or more-detailed match data?
								!structMatchScoresForRoot.ContainsKey(key)) // only take first score since other lower-priority match scores may come later
							{
								if (vo[1] is StructSearchMatch) // detailed structure search match info?
								{
									StructSearchMatch ssm = (StructSearchMatch)vo[1];
									structMatchScoresForRoot[key] = ssm;
								}

								else // should be just a numeric score
								{
									bool isNumber = QualifiedNumber.TryConvertToDouble(vo[1], out d1);
									if (isNumber) structMatchScoresForRoot[key] = d1;
									else structMatchScoresForRoot[key] = 0;
								}
							}

							subQueryResultKeys.Add(key);
						}

					} // end of read hit keys loop

					mb.Close();

					// Merge hit keys for this criteria subset with any previous keys for the current logic group

					if (logicGroupLogicType == QueryLogicType.And) // if and logic then just replace prev keys with current keys
					{
						logicGroup.ResultKeys = new Dictionary<string, object>();
						logicGroup.SearchKeySubset = new List<string>();

						foreach (string key0 in subQueryResultKeys) // build new hit list and search key subset for limiting successive search steps
						{
							logicGroup.ResultKeys[key0] = null;
							logicGroup.SearchKeySubset.Add(key0);
						}

						foreach (LogicElement le0 in logicGroup.ElementList) // also pass search key subset down to any next-level groups
						{
							if (le0 is LogicGroup)
							{
								LogicGroup lg0 = le0 as LogicGroup;
								lg0.SearchKeySubset = logicGroup.SearchKeySubset;
							}
						}
					}

					else // Or logic, just add the new keys to the keys for the logic group
					{
						if (logicGroup.ResultKeys == null) logicGroup.ResultKeys = new Dictionary<string, object>();

						foreach (string key0 in subQueryResultKeys)
							logicGroup.ResultKeys[key0] = null;
					}

					tDelta = (int)TimeOfDay.Delta(tKeyFetchStart);
					Stats.KeyFetchTime += tDelta;

					if (LogBasics)
					{
						int count = subQueryResultKeys.Count;
						string msg = "QE Search Step for logic group: " + logicGroup.Id + ", criteria subset: " + csi + ", Total keys: " + count + ", time: " + tDelta;
						if (count > 0) msg += ", time/row: " + ((tDelta * 1000) / count) / 1000.0;
						DebugLog.Message(msg);

						DebugLog.Message("Search Execute/Key Fetch - Total time: " + (tExecute + tDelta));
					}

					// If last criteria subset of this logic group then pass keys up to previous level

					IfLastElementOfLogicGroupPassKeysUp(criteriaSubset, logicGroup);

				} // end of criteriaSubsets loop

				if (buildSqlOnly) return new List<string>();  // if just building sql all done here


				/////////////////////////////////////////////////////
				// Append keys for this root to cumulative set if new
				/////////////////////////////////////////////////////

				if (rootLogicGroup?.ResultKeys?.Keys != null)
				{
					foreach (string key in rootLogicGroup.ResultKeys.Keys)
					{
						if (!ResultsKeysDict.ContainsKey(key))
						{
							resultKeys.Add(key);
							ResultsKeysDict[key] = 0;
							if (structMatchScoresForRoot.ContainsKey(key)) // copy over any structure match score
								ResultsKeysDict[key] = structMatchScoresForRoot[key];
						}
					}
				}

			} // end of root loop

			if (resultKeys.Count == 0) // any hits?
				return resultKeys;

			resultKeys = // filter resultKeys by any restricted list of allowed keys
				RestrictedDatabaseView.FilterOutRestrictedKeys(resultKeys);

			// If this is a similarity search then sort by decreasing score and filter by max allowed sim hits if specified

			int maxSimHits;
			bool hasSimSearch = TryParseSimSearchMaxHits(out maxSimHits);
			if (hasSimSearch)
			{
				List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>(ResultsKeysDict);
				list.Sort(CompareSimScoresKeyValuePair); // sort by sim value
				int keyCnt = list.Count;

				bool truncate = (maxSimHits > 0 && maxSimHits < resultKeys.Count); // need to truncate list?
				if (truncate)
				{
					keyCnt = maxSimHits;
					ResultsKeysDict.Clear(); // need to rebuild dict
				}

				resultKeys.Clear(); // need to rebuild ordered list of keys
				for (i1 = 0; i1 < keyCnt; i1++) // take top values
				{
					resultKeys.Add(list[i1].Key);
					if (truncate)
						ResultsKeysDict.Add(list[i1].Key, list[i1].Value);
				}
			}

			// If started with key subset, keep keyset numbers in subset order

			else if (initialSearchKeys != null)
			{
				ResultsSorter.SortKeySet(resultKeys, SortOrder.Ascending);
				List<string> keySet2 = new List<string>();
				foreach (string key in initialSearchKeys)
				{ // do binary search of sorted database subset
					if (resultKeys.BinarySearch(key) >= 0)
						keySet2.Add(key);
				}
				resultKeys = keySet2;
			}

			// Didn't start with subset so sort as user wishes

			else
			{
				SortOrder sd = SortOrder.None;
				if (query.KeySortOrder > 0) sd = SortOrder.Ascending;
				else if (query.KeySortOrder < 0) sd = SortOrder.Descending;

				ResultsSorter.SortKeySet(resultKeys, sd);
				if (query.GroupSalts) resultKeys = MoleculeUtil.GroupSalts(resultKeys); // group salts if requested
			}

			return resultKeys;

		} // ExecuteSearchStepMethod

/// <summary>
/// If logicElement is the last element of its logic group then pass keys up to previous level
/// </summary>
/// <param name="logicElement"></param>
/// <param name="logicGroup"></param>

		private void IfLastElementOfLogicGroupPassKeysUp(
			LogicElement logicElement, 
			LogicGroup logicGroup)
		{
			if (logicGroup == null || logicGroup.ElementList.Count == 0 || logicElement == null) return;
			LogicElement lastLogicElement = logicGroup.ElementList[logicGroup.ElementList.Count - 1];
			if (logicElement != lastLogicElement) return; // last subset for logic group?

			LogicGroup plg = logicGroup.ParentLogicGroup;
			if (plg == null) return;

			// Pass list up 

			if (plg.LogicType == QueryLogicType.And) // if parent is and logic then just plug in this set of keys
				plg.ResultKeys = logicGroup.ResultKeys;

			else // just add these keys to parent with Or logic
			{
				foreach (string key0 in logicGroup.ResultKeys.Keys)
					plg.ResultKeys[key0] = null;
			}

			IfLastElementOfLogicGroupPassKeysUp(plg, plg.ParentLogicGroup); // recursively pass up the tree
			return;
		}

	} // end of QueryEngine class

} // end of namespace Mobius.QueryEngineLibrary
