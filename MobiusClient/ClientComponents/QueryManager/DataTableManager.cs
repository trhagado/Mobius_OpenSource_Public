using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;

using Spotfire.Dxp.Data.Formats.Stdf;
using Spotfire.Dxp.Data.Formats.Sbdf;

using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Data Table Manager
	/// </summary>

	public partial class DataTableManager : IDataTableManager
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public QueryManager QueryManager = null; // QueryManager for this DataTableManager
		internal QueryManager Qm { get { return QueryManager; } }
		internal Query Query { get { return QueryManager != null ? QueryManager.Query : null; } }
		internal MoleculeGridControl Grid { get { return QueryManager != null ? QueryManager.MoleculeGrid : null; } } // associated Grid 
		internal ResultsFormat Rf { get { return QueryManager != null ? QueryManager.ResultsFormat : null; } }
		internal QueryEngine Qe { get { return QueryManager != null ? QueryManager.QueryEngine : null; } }
		internal StatusBarManager StatusBarManager { get { return QueryManager != null ? QueryManager.StatusBarManager : null; } }

		public const string ResultsDataTableName = "Results";
		public const string RowAttributesColumnName = "RowAttributes";
		public const string CheckMarkColumnName = "CheckMark";
		public const string BaseKeyColumnName = "BaseKey";

		public const int NullRowIndex = -999; // data row index used for null rows

		internal RowRetrievalState RowRetrievalState; // state of row retrieval from source into the DataTable
		public bool RowRetrievalComplete // return true if retrieval is complete
		{ get { return RowRetrievalState == RowRetrievalState.Undefined || RowRetrievalState == RowRetrievalState.Cancelled || RowRetrievalState == RowRetrievalState.Complete; } }
		public bool RowRetrievalCancelled // return true if row retrieval has been cancelled
		{ get { return RowRetrievalState == RowRetrievalState.Cancelled; } }

		internal QueryEngineStats CompleteQueryEngineRowRetrievalStats; // stats from QueryEngine for partial (if cancelled) or complete retrieval of query data
		internal QueryEngineStats ExportDataToSpotfireFilesStats; // stats from QueryEngine export of complete data to Spotfire

		internal Dictionary<int, object> RowSubset = null; // if defined then just consider this subset when retrieving rows

		internal int KeyCount = -1, RowCount = -1; // total keys/rows that data has been retrieved for

		public List<string> ResultsKeys // full set of results keys if available (defined as property for debugging)
		{
			get { return _resultsKeys; }
			set { _resultsKeys = value; }
		}
		private List<string> _resultsKeys = new List<string>();

		internal int PassedFiltersKeyCount = -1, PassedFiltersRowCount = -1; // keys/rows remaining after filtering
		internal int MarkedKeyCount = -1, MarkedRowCount = -1; // keys/rows marked
		internal int SelectedKeyCount = -1, SelectedRowCount = -1; // keys/rows selected
		internal Dictionary<string, int> SelectedKeys; // indexes of selected key rows
		internal Dictionary<string, int> MarkedKeys; // indexes of marked key rows
		internal StructureMatcher StrMatcher; // used for structure filtering

		internal bool FiltersEnabled = false; // flag indicating if secondary filters are enabled (initially disabled)
		internal List<ColumnInfo> FilteredColumnList; // list of columns with secondary filters
		internal bool FiltersApplied = false; // true if existing filters have been applied to the current data table

		internal static int DefaultRowRequestSize = 16; // default number of rows to read
		internal int CurrentRowsRequested = 0; // number of rows to read for current request
		internal int CurrentRowsReadToBuffer = 0; // number of rows read and transferred to the buffer in current request
		internal int TotalRowsReadToBuffer = 0; // total number of rows read and transferred to the buffer in current request
		internal int CurrentRowsTransferredToDataTable = 0; // number of rows read and transferred to the DataTable in current request
		internal int TotalRowsTransferredToDataTable = 0; // total number of rows read and transferred to the DataTable in current request

		internal int[] MaxRowsPerKey; // max number of rows for any key value in the DataTable for each QueryTable
		internal int IndexOfLastRowAdded = -1;
		internal string KeyForLastRowAdded = "";
		internal int[] RowsForKeyForLastRowAdded; // number of rows for key for last row added for each QueryTable

		internal int NextRowsMinRows = 1; // minimum rows to retrieve in current call to QueryEngine.NextRows
		internal int NextRowsMaxRows = 1000; // max rows to retrieve in current call to QueryEngine.NextRows
		internal int NextRowsMaxTime = -1; // maxTime in ms for current call to QueryEngine.NextRows, no limit if negative 
		internal int ReadRequests = 0; // number of read requests
		internal bool PauseRowRetrievalRequested = false; // true if row retrieval should be paused
		internal bool CancelRowRetrievalRequested = false; // true if row retrieval should be cancelled
		internal bool PauseRowRetrievalThreadRequested = false; // if true row retrieval should be paused
		internal bool StopRowRetrievalThreadRequested = false; // if true then row retrieval should be stopped

		internal List<object[]> RetrievedRowBuffer = new List<object[]>(); // retrieved data rows waiting to be moved to the DataTable & grid
		internal DataTableMx DataTableMx => QueryManager?.DataTable; // the datatable
		public int DataTableFetchPosition = -1; // where we are in scan of data table
		public int LastDataTableFetchPosition = -1; // position in previous call
		public int LastDataTableRowCount = -1; // datatable size for last fetch
		public int DataTableFetchCount => DataTableFetchPosition + 1; // number of rows fetched from DataTable
		internal object DataTransferLock = new object(); // DataTableManager lock to synchronize moving rows in and out of RetrievedRowBuffer and qm.DataTable

		internal int RowsTransferredToDataTableCount = -1; // current number of rows transferred from the buffer to the datatable
		internal int MaxRowsToTransferToDataTablePerTick = 10000;
		internal int TotalQeRowsRetrieved = 0; // total rows retrieved from query engine

		internal bool AllRecordsBuffered = false; // true if all rows have been read from the Qe

		internal bool InReadNextRowsFromQueryEngine = false;

		internal bool InCompleteRetrieval = false; // completing retrieval of all data
		internal bool UpdateRetrievingAllDataMessage = false;
		internal string LastCompleteRetrievalProgressMessage = "";

		internal bool UsingRetrieveAndBufferRowsThread = true; // normally use separate thread for row retrieval and buffering
		internal bool RetrieveAndBufferRowsLoopThreadRunning = false; // timer used for special case retrieval of data in place of thread
		internal EventWaitHandle RetrieveAndBufferRowsThreadStartLatch = null; // latch to use to wait while RetrieveAndBufferRowsThread gets started up
		internal Exception RetrieveAndBufferRowsLoopThreadException; // unexpected exception in row retrieval

		internal bool UsingNonThreadedRetrieveAndBufferRowsTimer = false; // normally use separate thread for row retrieval
		internal System.Windows.Forms.Timer NonThreadedRetrieveAndBufferRowsTimer; // timer used for special case retrieval of data in place of thread

		internal List<MetaBrokerStats> MetaBrokerStats = null;
		internal int[] MetaBrokerRowCounts, MetaBrokerTimes;

		internal bool UpdateMaxRowsPerKeyEnabled = true; // if true update max rows per key

		internal System.Windows.Forms.Timer TransferFromBufferToDataTableTimer; // timer for transferring rows from RetrievedRowBuffer to DataTable
		internal bool InTransferFromBufferToDataTableTimer_Tick = false;
		internal bool InSecondaryGridFrfreshTimer_Tick = false;

		internal System.Windows.Forms.Timer SecondarGridRefreshTimer; // secondary timer 
		internal bool SecondaryGridRefreshScheduled = false;

		internal int CacheStartPosition = -1; // number of rows in DataTable when caching started
		internal static int CacheMiminumRowsRequiredForWriting = 5000; // minimum datatable rows required to do a cache write
		internal int CacheStartRowCount;
		internal int CacheStartKeyCount;
		internal int RowsRemovedFromDataTable = 0; // number of rows removed from DataTable (may or may not be written to cache)
		internal int RowsWrittenToCache = 0; // rows actually written to cache
		internal string CacheFile; // name of cache file
		internal StreamWriter CacheWriter; // for writing cache file
		internal bool PurgeDataTableWithoutWritingToCacheFile = false; // if true then skip actual writing of cache file
		internal StreamReader CacheReader; // for reading cache file

		internal bool DataModified = false; // data row modified since last save
		internal List<DataRowMx> DeletedRows = new List<DataRowMx>(); // list of rows deleted from DataTable but not yet the underlying database

		public const int DefaultRowAttributesVoPos = 0; // position in Vo for row attributes
		public const int DefaultCheckMarkVoPos = 1; // position in Vo for check mark
		public const int DefaultKeyValueVoPos = 2; // position in Vo for the common key value

		public int RowAttributesVoPos = DefaultRowAttributesVoPos; // position in Vo for row attributes
		public int CheckMarkVoPos = DefaultCheckMarkVoPos; // position in Vo for check mark
		public int KeyValueVoPos = DefaultKeyValueVoPos; // position in Vo for the common key value
		public int DataValuesVoOffset => KeyValueVoPos + 1; // position in Vo of first data column, usually the key value of the first table
		internal bool TriedVoPosFix = false;

		internal static bool? UseGridBeginAndEndUpdate = null; // if true use Grid.BeginUpdate, EndUpdate while moving data to the Grid DataTable so update is done all at once
		internal static bool PauseRowRetrievalEnabled = true; // pause row retrieval before full dataset is retrieved
		internal static bool AllowCaching = true; // global caching-allowed flag

		internal static bool DebugBasics = false;
		internal static bool DebugDetails = false;
		internal static bool DebugCaching = false;

		/// <summary>
		/// Default constructor
		/// </summary>

		public DataTableManager()
		{
			InitTimers();
			return;
		}

		/// <summary>
		/// Construct and link with associated grid
		/// </summary>
		/// <param name="grid"></param>

		public DataTableManager(
				QueryManager queryManager)
		{
			QueryManager = queryManager;
			QueryManager.DataTableManager = this;
			InitTimers();
			return;
		}

		void InitTimers()
		{
			if (TransferFromBufferToDataTableTimer == null)
			{
				TransferFromBufferToDataTableTimer = new System.Windows.Forms.Timer();
				TransferFromBufferToDataTableTimer.Interval = 100; // ms interval
			}

			TransferFromBufferToDataTableTimer.Tick += new System.EventHandler(TransferFromBufferToDataTableTimer_Tick);

			if (NonThreadedRetrieveAndBufferRowsTimer == null)
			{
				NonThreadedRetrieveAndBufferRowsTimer = new System.Windows.Forms.Timer();
				NonThreadedRetrieveAndBufferRowsTimer.Interval = 100; // ms interval
			}

			NonThreadedRetrieveAndBufferRowsTimer.Tick += new System.EventHandler(NonThreadedRetrieveAndBufferRowsTimer_Tick);

			if (SecondarGridRefreshTimer == null)
			{
				SecondarGridRefreshTimer = new System.Windows.Forms.Timer();
				SecondarGridRefreshTimer.Interval = 1; // ms interval
			}

			SecondarGridRefreshTimer.Tick += new System.EventHandler(SecondaryGridRefreshTimer_Tick);

			return;
		}

		/// <summary>
		/// Build a DataTable for a QueryManager Query
		/// </summary>
		/// <param name="qm"></param>
		/// <returns></returns>

		public static DataTableMx BuildDataTable(QueryManager qm)
		{
			qm.DataTable = BuildDataTable(qm.Query);
			return qm.DataTable;
		}

		/// <summary>
		/// Build a DataTable for the associated query for selected/sorted cols
		/// </summary>

		public static DataTableMx BuildDataTable(Query q)
		{
			MetaTable mt;
			MetaColumn mc;
			QueryTable qt;
			QueryColumn qc;
			Type type;
			string colName;
			int ti, ci;

			q.AssignUndefinedAliases(); // be sure aliases are defined for query since they are used in DataColumn names

			DataTableMx dt = new DataTableMx(ResultsDataTableName);
			dt.Columns.Add(RowAttributesColumnName, typeof(DataRowAttributes));
			dt.Columns.Add(CheckMarkColumnName, typeof(bool));
			dt.Columns.Add(BaseKeyColumnName, typeof(object));

			// Process each table

			for (ti = 0; ti < q.Tables.Count; ti++)
			{

				qt = q.Tables[ti];
				mt = qt.MetaTable;

				// Process each selected column

				for (ci = 0; ci < qt.QueryColumns.Count; ci++)
				{
					qc = qt.QueryColumns[ci];
					if (qc.MetaColumn == null) continue;

					if (!qc.Is_Selected_or_GroupBy_or_Sorted) continue;
					mc = qc.MetaColumn;
					colName = DataColName(qc);
					type = typeof(object); // define all columns as object type since multiple types may be stored
					DataColumn dc = dt.Columns.Add(colName, type);
					SetExtendedMobiusDataColumnProperties(dc, mc);
				}
			}

			return dt;
		}

		/// <summary>
		/// Return a QueryTable alias-based DataTable.DataColumn name associated with a QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static string DataColName(QueryColumn qc)
		{
			return qc.QueryTable.Alias + "." + qc.MetaColumn.Name;
		}

		/// <summary>
		/// Get the Datasource object
		/// </summary>

		public object DataSource
		{
			get
			{
				if (UseUnpivotedAssayResultsCache) // using "cached" UNPIVOTED_ASSAY_RESULTS data source
					return GetUnpivotedAssayResultsDataSource();

				else return DataTableMx; // data coming from DataTable (normal case)
			}
		}

		/// <summary>
		/// Return true if row attributes are available in the dataset
		/// </summary>

		public bool HasRowAttributes
		{
			get
			{
				if (DataTableMx != null &&
				 DataTableMx.Columns[0].DataType == typeof(DataRowAttributes))
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Initialize the datarow attributes for a DataTable
		/// 
		/// </summary>

		public void InitializeRowAttributes()
		{
			InitializeRowAttributes(true, true);
			return;
		}

		public void InitializeRowAttributes(
				bool initializeRowState)
		{
			InitializeRowAttributes(initializeRowState, true);
			return;
		}

		/// <summary>
		/// Initialize the datarow attributes for a DataTable
		/// </summary>
		/// <param name="initializeRowState"></param>

		public void InitializeRowAttributes(
				bool initializeRowState,
				bool initializeSelection)
		{
			bool selected;

			if (QueryManager == null || DataTableMx == null || RowAttributesVoPos < 0 || KeyValueVoPos < 0) return;
			// DataTable.Columns[KeyValueVoPos].DataType != typeof(DataRowAttributes)) return;

			RowCount = DataTableMx.Rows.Count;
			KeyCount = 0;
			PassedFiltersKeyCount = PassedFiltersRowCount = -1;

			string curKeyVal = "";
			int curKeyRow = -1;

			MaxRowsPerKey = new int[Rf.Tables.Count];
			int[] rowsPerKey = new int[Rf.Tables.Count];

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{
				DataRowMx dr = DataTableMx.Rows[dri];
				object[] drVo = dr.ItemArrayRef; // use row item array reference to avoid firing change events
				DataRowAttributes dra0 = GetRowAttributes(drVo); // get initial attributes

				if (initializeRowState || drVo[RowAttributesVoPos] == null)
					drVo[RowAttributesVoPos] = new DataRowAttributes();

				DataRowAttributes dra = GetRowAttributes(drVo);
				if (dra == null) throw new Exception("DataRowAttributes not defined");

				string keyVal = drVo[KeyValueVoPos] as string;
				if (keyVal == null && !NullValue.IsNull(dr[KeyValueVoPos + 1]))
				{
					keyVal = drVo[KeyValueVoPos + 1].ToString(); // if initial key is null try to get from next column
					drVo[KeyValueVoPos] = keyVal;
				}

				if (keyVal != curKeyVal) // || Rf.Tables.Count <= 1) // going to new key
				{
					curKeyVal = keyVal;
					curKeyRow = dri;
					//if (dra == null) dra = dra; // debug
					dra.FirstRowForKey = curKeyRow;

					Array.Clear(rowsPerKey, 0, rowsPerKey.Length); // reset rows per key per table
					KeyCount++; // count the key
				}

				else
				{
					dra.FirstRowForKey = curKeyRow;
				}

				dra.Key = drVo[KeyValueVoPos].ToString();

				if (initializeSelection || dra0 == null) selected = false;
				else selected = dra0.Selected;
				dra.Selected = selected;

				if (initializeRowState)
					InitializeTableRowState(dr);

				for (int ti = 0; ti < Rf.Tables.Count; ti++) // update rowsPerKey
				{
					if (dra.TableRowState[ti] != RowStateEnum.Undefined)
					{
						rowsPerKey[ti]++;
						if (rowsPerKey[ti] > MaxRowsPerKey[ti])
							MaxRowsPerKey[ti] = rowsPerKey[ti];
					}
				}

				UpdateUnfilteredSubrowPos(dri);
			}

			return;
		}

		/// <summary>
		/// Initialize the datarow attributes for a single DataRow
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="voa"></param>
		/// <returns></returns>

		public DataRowAttributes InitializeRowAttributes(DataRowMx dr)
		{
			DataRowAttributes dra = new DataRowAttributes();
			object[] drVo = dr.ItemArrayRef;
			dra.Key = drVo[KeyValueVoPos] as string; // common key value is in first position
			drVo[RowAttributesVoPos] = dra;
			InitializeTableRowState(dr);

			return dra;
		}

		/// <summary>
		/// Return true if more DataRows are available
		/// </summary>

		public bool MoreDataRowsAvailable
		{
			get
			{
				DataRowMx dr = FetchNextDataRow(false);
				if (dr == null) return false;
				DataTableFetchPosition--;
				LastDataTableFetchPosition--;
				return true;
			}
		}

		/// <summary>
		/// Return the next search result tuple
		/// </summary>
		/// <returns></returns>

		public DataRowMx FetchNextDataRow()
		{

			return FetchNextDataRow(true);
		}

		/// <summary>
		/// Return the next search result tuple
		/// </summary>
		/// <returns></returns>

		public DataRowMx FetchNextDataRow(bool allowCaching)
		{
			DataRowMx dr;

			int rowCount = DataTableMx.Rows.Count;
			if (HasRetrievingDataMessageRow()) rowCount--;

			while (true) // loop until end or get a valid row
			{
				if (DataTableFetchPosition + 1 >= rowCount) // any rows left in DataTable
				{
					if (RowRetrievalComplete)
					{
						VerifyResultsKeysUpToDate();
						if (DebugBasics) ClientLog.Message("RowRetrievalComplete");
						return null;
					}

					if (allowCaching) WriteRowsToCache(true);

					DialogResult dlgRslt = ReadNextRowsFromQueryEngine(); // start retrieval of next set of rows and wait until they make it to the DataTable

					if (dlgRslt == DialogResult.Cancel)
					{
						if (DebugBasics) ClientLog.Message("ReadNextRowsFromQueryEngine cancelled");
						return null;
					}

					if (DataTableFetchPosition + 1 >= DataTableMx.Rows.Count)
					{
						if (DebugBasics) ClientLog.Message("ReadNextRowsFromQueryEngine returned no rows");
						return null;
					}
				}

				lock (DataTransferLock) // lock DataTable while removing row
				{
					if (DebugBasics)
					{
						if (DataTableMx.Rows.Count != LastDataTableRowCount)
							ClientLog.Message("DataTable size changed: " + LastDataTableRowCount + " ==> " + DataTableMx.Rows.Count);

						if (DataTableFetchPosition != LastDataTableFetchPosition + 1)
							ClientLog.Message("Non-standard DataTableFetchPosition, Last: " + LastDataTableFetchPosition + ", Now: " + DataTableFetchPosition);
					}

					LastDataTableFetchPosition = DataTableFetchPosition;
					LastDataTableRowCount = DataTableMx.Rows.Count;

					DataTableFetchPosition++;
					dr = DataTableMx.Rows[DataTableFetchPosition];
				}

				//if (dr.ItemArray[KeyValueVoPos].ToString() == "02329256") return dr; // debug

				if (FiltersEnabled && RowIsFiltered(DataTableFetchPosition)) continue; // if filtered out continue

				//DataTableFetchCount++;
				return dr;

			}
		}

		/// <summary>
		/// Reset position of fetch cursor in DataTable
		/// </summary>

		public void ResetDataTableFetchPosition()
		{
			DataTableFetchPosition = -1;
			LastDataTableFetchPosition = -1;
			LastDataTableRowCount = -1;
			//DataTableFetchCount = 0;
			return;
		}

		/// <summary>
		/// Read the next chunk of rows from the QueryEngine into the DataTable
		/// </summary>
		/// <returns></returns>

		internal DialogResult ReadNextRowsFromQueryEngine()
		{
			int rowsRequested = DefaultRowRequestSize;
			return ReadNextRowsFromQueryEngine(rowsRequested);
		}

		/// <summary>
		/// Complete the retrieval of the DataSet
		/// </summary>
		/// <returns></returns>

		internal DialogResult CompleteRetrieval()
		{
			InCompleteRetrieval = true;
			UpdateRetrievingAllDataMessage = true;
			NextRowsMinRows = NextRowsMaxRows; // get max number of rows per call to QE

			try
			{
				if (RowRetrievalState == RowRetrievalState.Complete ||
				 QueryManager == null ||
				 QueryManager.QueryEngine == null)
					return DialogResult.OK; // return if complete

				Progress.Show("Retrieving all data..."); // let user know what we're doing

				bool gridDataTableSwitched = false;
				if (Grid != null && Grid.DataSource == DataTableMx)
					DataTableMx.EnableDataChangedEventHandlers(false);
				{ // switch out datatable while doing bulk insert to avoid out of memory problem with grid
					//DataTableMx dt = Qm.DataTableMx; // save ref to data table
					//Grid.DataSource = null; // clear source for header build
					//Qm.DataTableMx = dt; // restore data table
					//gridDataTableSwitched = true;
				}

				DialogResult result = ReadNextRowsFromQueryEngine(-1);
				Progress.Hide();

				DataTableMx.EnableDataChangedEventHandlers(true);
				if (Grid != null) // redisplay with full data if grid defined
					Grid.RefreshDataSource();

				return result;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

			finally
			{
				InCompleteRetrieval = false;
				UpdateRetrievingAllDataMessage = false;
			}
		}

		/// <summary>
		/// Complete retrieval of all data by the QueryEngine if not done yet
		/// Data is not returned to the client at this point
		/// </summary>
		/// <returns></returns>

		internal DialogResult CompleteQueryEngineRowRetrieval()
		{
			if (CompleteQueryEngineRowRetrievalStats != null && !CompleteQueryEngineRowRetrievalStats.Cancelled)
				return DialogResult.OK; // already done

			Progress.Show("Retrieving all data..."); // put up progress dialog since this may take a while

			CompleteQueryEngineRowRetrievalStats = Qm.QueryEngine.CompleteRowRetrieval();

			Progress.Hide();

			if (CompleteQueryEngineRowRetrievalStats.Cancelled) return DialogResult.Cancel;
			else return DialogResult.OK;
		}

		/// <summary>
		/// Export data to Spotfire files if not already done 
		/// </summary>
		/// <returns></returns>

		public DialogResult ExportDataToSpotfireFilesIfNeeded()
		{
			if (ExportDataToSpotfireFilesStats != null && !ExportDataToSpotfireFilesStats.Cancelled)
				return DialogResult.OK; // already done

			ExportParms ep = new ExportParms();
			ep.ExportFileFormat = ExportFileFormat.Stdf; // text format for now

			ep.ColumnNameFormat = ColumnNameFormat.SpotfireApiClient;

			string baseName = SpotfireViewProps.GetBaseExportFileName(Query);
			ep.OutputFileName = SpotfireViewProps.GetFullExportFilePathFromBaseFileName(baseName);
			ep.OutputFileName2 = "SingleAndMultiple"; // output both individual table files and a merged file
			ep.ExportStructureFormat = ExportStructureFormat.Molfile; // todo: make this configurable
			ep.QualifiedNumberSplit = QnfEnum.Split | QnfEnum.NumericValue | QnfEnum.Qualifier | QnfEnum.NValue; // need to split QNs

			//UIMisc.Beep();
			Progress.Show("Exporting data to Spotfire files...");
			Application.DoEvents();

			ExportDataToSpotfireFilesStats = Qm.DataTableManager.Qe.ExportDataToSpotfireFiles(ep);

			//UIMisc.Beep();
			Progress.Hide();

			if (ExportDataToSpotfireFilesStats.Cancelled) return DialogResult.Cancel;
			else return DialogResult.OK;
		}

		/// <summary>
		/// Read the next chunk of rows of the specified size into the DataTable.
		/// Return when the rows have been read, there are no more rows to read
		/// or the user has cancelled.
		/// </summary>
		/// <returns></returns>

		internal DialogResult ReadNextRowsFromQueryEngine(int rowsRequested)
		{
			int LastUpdateTime = 0; // time progress last updated
			int UpdateInterval = 1000; // update interval in milliseconds

			Stopwatch sw = Stopwatch.StartNew();

			if (DebugDetails) ClientLog.Message("RowsRequested = " + rowsRequested + ", RowRetrievalState = " + RowRetrievalState);

			if (InReadNextRowsFromQueryEngine)
			{
				if (DebugDetails) ClientLog.Message("InReadNextRowsFromQueryEngine = true");
				return DialogResult.Ignore; // return if already in here
			}

			if (RowRetrievalState == RowRetrievalState.Complete)
			{
				if (DebugDetails) ClientLog.Message("RowRetrievalState.Complete 1");
				return DialogResult.OK; // return if complete
			}

			if (Qe == null) // if no query engine say complete
			{
				RowRetrievalState = RowRetrievalState.Complete;
				RemoveRetrievingDataMessageRow();
				if (DebugDetails) ClientLog.Message("RowRetrievalState.Complete 2");
				return DialogResult.OK;
			}

			InReadNextRowsFromQueryEngine = true;

			DialogResult dialogResult = DialogResult.OK; // default response

			if (rowsRequested > 0) // tell user we're retrieving data if not retrieving all data
				AddRetrievingDataMessageRow();

			int startRowCount = DataTableMx.Rows.Count;
			if (RowRetrievalState == RowRetrievalState.Running)
			{
				if (rowsRequested < 0) CurrentRowsRequested = rowsRequested;
				else if (CurrentRowsRequested >= 0)
					CurrentRowsRequested += rowsRequested; // just increase current read
				PauseRowRetrievalThreadRequested = false; // cancel any requested pause
			}

			else StartRowRetrieval(rowsRequested);

			//bool showProgress = !Progress.IsVisible;
			//string msg = "Retrieving data...";
			//if (showProgress) Progress.Show(msg);

			int rowsRetrieved = 0;
			while (true) // loop until requested rows have been read, timout with one or more rows read, no more rows or cancel requested
			{
				Thread.Sleep(100);
				Application.DoEvents();

				rowsRetrieved = DataTableMx.Rows.Count - startRowCount;
				int elapsedTime = (int)sw.ElapsedMilliseconds;
				bool timeout = elapsedTime > 1000 && rowsRetrieved > 0;
				bool retrievedRequestedRows = rowsRetrieved >= rowsRequested;
				bool retrieveAllRows = rowsRequested < 0;

				if (!retrieveAllRows && // if not retrieving all rows and
				 ((timeout && rowsRetrieved > 0) || // timeout and we've retrieved something or
				 (rowsRequested > 0 && retrievedRequestedRows))) // we've retrieved the requested number of rows
					break; // then we're done

				else if (RowRetrievalComplete)
				{
					VerifyResultsKeysUpToDate();
					break;
				}

				else if (Progress.CancelRequested) // cancelled by user, just pause for now
				{
					PauseRowRetrieval();
					dialogResult = DialogResult.Cancel;
					break;
				}

				if (AllRecordsBuffered && (TotalRowsReadToBuffer == TotalRowsTransferredToDataTable)) // retrieval complete?
				{
					RowRetrievalState = RowRetrievalState.Complete;
					break;
				}

				if (RowRetrievalState == RowRetrievalState.Paused && !AllRecordsBuffered)
					StartRowRetrieval(rowsRequested); // restart if paused

				int rowCount = DataTableMx.Rows.Count + RetrievedRowBuffer.Count;

				if (UpdateRetrievingAllDataMessage && rowCount > 0 && // update retrieving all data message?
						sw.ElapsedMilliseconds - LastUpdateTime >= UpdateInterval)
				{
					LastUpdateTime = (int)sw.ElapsedMilliseconds;
					string msg = "Retrieving all data - 0:00, Rows: " + StringMx.FormatIntegerWithCommas(rowCount);
					if (Lex.IsDefined(LastCompleteRetrievalProgressMessage))
						msg = Lex.Replace(msg, "0:00", ".:.."); // continue without resetting the time

					if (msg != LastCompleteRetrievalProgressMessage)
					{
						Progress.Show(msg); // let user know what we're doing
						LastCompleteRetrievalProgressMessage = msg;
					}
				}
			}

			//if (showProgress) Progress.Hide();

			RemoveRetrievingDataMessageRow();
			InReadNextRowsFromQueryEngine = false;

			if (DebugDetails) ClientLog.Message("RowsRetrieved = " + rowsRetrieved);

			return dialogResult;
		}

		/// <summary>
		/// Start thread to retrieve rows from the QueryEngine & load into the DataTable
		/// </summary>

		internal void StartRowRetrieval()
		{
			int initialReadSize = DefaultRowRequestSize; // number of rows to read in initially
			StartRowRetrieval(initialReadSize);
			return;
		}

		/// <summary>
		/// Start thread to retrieve rows from the QueryEngine & load into the DataTable
		/// </summary>

		internal void StartRowRetrieval(int rowsRequested)
		{
			//
			// Row retrieval from the query engine consists of the following elements:
			//
			// 1. StartRowRetrieval - Starts retrieval for the next chunk of data by 
			//		starting the RetrieveAndBufferRowsThread and the TransferFromBufferToDataTableTimer.
			// 
			// 2. RetrieveAndBufferRowsLoopThread - Retrieves rows from the Qe and adds to the RetrievedRowBuffer until
			//     either allrows have been retrieved or a pause or stop request is posted.
			// 
			// 3. TransferFromBufferToDataTableTimer - Timer running on the UI thread that transfers
			//    rows from the RetrievedRowBuffer to the DataTable. Pauses retrieval when
			//    the number of rows in the current request has been fulfilled.
			//
			// 4. CancelRowRetrieval and PauseRowRetrieval allow retrieval to be stopped or paused
			//    RetrieveAndBufferRowsLoopThread and TransferFromBufferToDataTableTimer are stopped in both cases
			//		but the Qe is left open for a pause operation.

			if (DebugDetails) ClientLog.Message("RowsRequested = " + rowsRequested + ", RowRetrievalState = " + RowRetrievalState);

			if (RowRetrievalState == RowRetrievalState.Running)
			{
				if (DebugDetails) ClientLog.Message("Exit 1");
				return; // just return if already running
			}

			if (!Query.RetrievesDataFromQueryEngine)
			{
				if (DebugDetails) ClientLog.Message("Exit 2");
				return; // if no data retrieved from QueryEngine just return
			}

			if (Grid != null && Grid.V.IsEditing)
			{
				if (DebugDetails) ClientLog.Message("Exit 3");
				return; // don't allow any rows to be added to the datatable while editing since it puts a MemoEdit control in an invalid state
			}

			if (RowRetrievalState != RowRetrievalState.Paused) // starting if not paused
			{
				ResetDataState();

				ReadRequests = 1; // first request

				if (QueryManager.StatusBarManager != null)
					QueryManager.StatusBarManager.DisplayFilterCounts();
			}

			//readSize = 10; // debug - read small data set
			CurrentRowsRequested = rowsRequested; // number of rows to read for current request

			if (QueryManager.QueryEngine != null) //  && !AllRecordsBuffered) // if QueryEngine defined and not at end of cursor then start getting rows from it
			{
				StartRetrieveAndBufferRowsLoopThread();

				if (QueryEngineStatsForm.ShowStats) // && TotalQeRowsRetrieved == 0)
				{
					QueryEngineStatsForm.StartingRetrieval();
					QueryEngineStatsForm.UpdateStatsDisplay(Query, Qe, MetaBrokerStats); // show results of search step
				}
			}

			else
			{
				RowRetrievalState = RowRetrievalState.Complete; // say complete if no query engine
				RemoveRetrievingDataMessageRow();
				if (DebugDetails) ClientLog.Message("RowRetrievalState.Complete 5");
			}

			RetrieveAndBufferRowsLoopThreadException = null;

			DataTableMx.RowChanged += new DataRowMxChangeEventHandler(Dt_RowChanged); // Monitor row changes
			return;
		}

		/// <summary>
		/// Monitor row changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void Dt_RowChanged(object sender, DataRowMxChangeEventArgs e)
		{
			if (e.Action == DataRowAction.Add) // if row added be sure attributes are set up
			{
				if (RowAttributesVoPos >= 0 &&
				 e.Row[RowAttributesVoPos] == DBNull.Value)
					InitializeRowAttributes(e.Row);
			}

			if (DebugDetails)
			{
				int dri;
				for (dri = 0; dri < DataTableMx.Rows.Count; dri++)
				{
					if (DataTableMx.Rows[dri] == e.Row) break;
				}
				ClientLog.Message("Dt_RowChanged - Row: " + dri + "/" + DataTableMx.Rows.Count + ", Action: " + e.Action);
				//if (e.Action == DataRowAction.Delete) e = e; // catch deletes for dev
			}

			return;
		}

		/// <summary>
		/// Pause retrieval of rows from the QueryEngine
		/// </summary>

		internal void PauseRowRetrieval()
		{
			if (DebugDetails) ClientLog.Message("PauseRowRetrieval Called - RowRetrievalState: " + RowRetrievalState);

			if (RowRetrievalState != RowRetrievalState.Running) return;
			PauseRowRetrievalThreadRequested = true; // set flag to pause retrieval from query engine 
			if (DebugDetails) ClientLog.Message("PauseRowRetrievalThreadRequested 1");

			return;
		}

		/// <summary>
		/// Cancel retrieval of rows from the QueryEngine
		/// </summary>

		internal void CancelRowRetrieval()
		{
			if (DebugDetails) ClientLog.Message("CancelRowRetrieval Called - RowRetrievalState: " + RowRetrievalState);

			if (RowRetrievalState == RowRetrievalState.Running)
			{
				CancelRowRetrievalRequested = true; // set flag to stop retrieval on next tick
				for (int attempt = 0; attempt < 10; attempt++)
				{
					if (!CancelRowRetrievalRequested) return; // exit when cancel complete
					Thread.Sleep(250); // waite for cancel to occur
					Application.DoEvents();
				}

				return; // timed out, return anyway
			}

			else if (RowRetrievalState == RowRetrievalState.Paused)
			{
				CloseResultsReader();
				RowRetrievalState = RowRetrievalState.Cancelled;
				RemoveRetrievingDataMessageRow();
				if (DebugDetails) ClientLog.Message("RowRetrievalState.Cancelled 6");
			}
			return;
		}

		/// <summary>
		/// Set row retrieval to the Complete state
		/// </summary>

		internal void SetRowRetrievalStateComplete()
		{
			RowRetrievalState = RowRetrievalState.Complete;
			RemoveRetrievingDataMessageRow();
			if (DebugBasics) ClientLog.Message("RowRetrievalState.Complete 7");
			return;
		}

		/// <summary>
		/// Start the row retrieval and buffering cycle
		/// </summary>

		void StartRetrieveAndBufferRowsLoopThread()
		{
			TransferFromBufferToDataTableTimer.Enabled = true; // start timer to transfer rows

			if (DebugDetails) ClientLog.Message("RowRetrievalThreadRunning = " + RetrieveAndBufferRowsLoopThreadRunning);

			if (RetrieveAndBufferRowsLoopThreadRunning)
				return;

			//if ((Qe == null || Qe.State == QueryEngineState.Closed) && CacheReader == null)
			if ((Qe == null || AllRecordsBuffered) && CacheReader == null)
			{
				if (DebugDetails)
				{
					if (Qe == null) ClientLog.Message("QueryEngine Null 1");
					//else if (Qe.State == QueryEngineState.Closed) ClientLog.Message("Qe.State == QueryEngineState.Closed");
					if (AllRecordsBuffered) ClientLog.Message("AllRecordsBuffered 2");
				}
				return;
			}

			UsingRetrieveAndBufferRowsThread = true; // normally use separate thread for row retrieval
			UsingNonThreadedRetrieveAndBufferRowsTimer = false;

			if (UsingRetrieveAndBufferRowsThread)
			{
				if (DebugDetails) ClientLog.Message("Starting thread");

				RetrieveAndBufferRowsThreadStartLatch = new EventWaitHandle(false, EventResetMode.AutoReset);

				ThreadStart ts = new ThreadStart(RetrieveAndBufferRowsLoopThread);
				Thread newThread = new Thread(ts);
				newThread.Name = "RetrieveAndBufferRowsLoopThread";
				newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start();

				EventWaitHandle.WaitAny(new WaitHandle[] { RetrieveAndBufferRowsThreadStartLatch });
				if (DebugDetails) ClientLog.Message("Thread Started");
			}

			else // run on this main UI thread with timer to retrieve data
			{
				NonThreadedRetrieveAndBufferRowsTimer.Interval = 100;
				NonThreadedRetrieveAndBufferRowsTimer.Enabled = true;
				RetrieveAndBufferRowsLoopThreadRunning = true;
				return;
			}

			PauseRowRetrievalThreadRequested = false;
			StopRowRetrievalThreadRequested = false;
			RowRetrievalState = RowRetrievalState.Running;
			if (DebugDetails) ClientLog.Message("RowRetrievalState.Running 1");

			AddRetrievingDataMessageRow();
			Progress.GlobalCancelRequested = false; // catch global cancel
			return;
		}

		/// <summary>
		/// Thread method that loops reading rows from query engine & adding them to the buffer
		/// until all rows are read or a pause or stop request is seen
		/// </summary>

		void RetrieveAndBufferRowsLoopThread()
		{
			RetrieveAndBufferRowsLoopThreadRunning = true; // say we're running

			try { RetrieveAndBufferRowsThreadStartLatch.Set(); } // release latch on thread start
			catch (Exception ex) { ex = ex; } // just in case

			// Loop getting sets of rows & adding them to the RetrievedRowBuffer

			try
			{
				while (true)
				{
					int rows = RetrieveAndBufferRows();
					if (rows < 0) return; // exit the thread
				}
			}

			finally { RetrieveAndBufferRowsLoopThreadRunning = false; } // exiting the thread
		}

		/// <summary>
		/// Handle row retrieval via main UI thread timer tick 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void NonThreadedRetrieveAndBufferRowsTimer_Tick(object sender, EventArgs e)
		{
			NonThreadedRetrieveAndBufferRowsTimer.Enabled = false;
			int rows = RetrieveAndBufferRows();
			if (DebugBasics) ClientLog.Message("Rows Read = " + rows);
			if (rows < 0)  // if done exit without restarting timer
			{
				RetrieveAndBufferRowsLoopThreadRunning = false;
				return;
			}

			NonThreadedRetrieveAndBufferRowsTimer.Enabled = true;
			return;
		}

		/// <summary>
		/// Attempt to read and transfer the next set of rows to the RetrievedRowBuffer
		/// </summary>
		/// <returns></returns>

		int RetrieveAndBufferRows()
		{
			List<object[]> voRows = null;
			char[] recLenBuf = new char[8]; // caching buffer rec length

			try
			{
				Stopwatch sw = Stopwatch.StartNew();

				NextRowsMaxTime = 2000 + // 2 sec minimum
						DataTableMx.Rows.Count * 10; // plus 1 sec for every 100 rows

				if (NextRowsMaxTime > 10000) NextRowsMaxTime = 10000; // limit max to 10 seconds

				//if (DataTableMx.Rows.Count > 1000) // increase time if relatively large dataset
				//{
				//  maxTimeMs = 3000;
				//}

				//if (DataTableMx.Rows.Count < 20) // retrieve first few rows individually (old conservative method)
				//{
				//  maxRows = 2;
				//  maxTimeMs = 0;
				//}

				bool readFromQueryEngine = (CacheReader == null); // get from query engine if CacheReader is not open
				if (readFromQueryEngine)
				{
					if (!QueryEngineStatsForm.ShowStats)
						MetaBrokerStats = null;

					else if (MetaBrokerStats == null)
						MetaBrokerStats = new List<MetaBrokerStats>();

					bool getMaxRows = !SS.I.Attended; // // if running in background set high limits
					if (getMaxRows) // get max # of rows 
					{
						NextRowsMinRows = NextRowsMaxRows;
						NextRowsMaxTime = 0; // no time limit
					}

					if (DebugBasics)
						ClientLog.Message("Calling Qe.NextRows(MinRows = " + NextRowsMinRows + ", MaxRows = " + NextRowsMaxRows + ", MaxTime = " + NextRowsMaxTime + ")");

					voRows = Qe.NextRows(NextRowsMinRows, NextRowsMaxRows, NextRowsMaxTime, MetaBrokerStats); // get the rows 

					if (voRows != null && voRows.Count >= NextRowsMaxRows) NextRowsMaxRows *= 2; // increase max rows if got them this time

					if (voRows != null) // get data?
					{
						if (DebugBasics) ClientLog.Message("Rows read from Qe: " + voRows.Count + ", time: " + (int)sw.ElapsedMilliseconds);
					}

					else // indicate that QE has read all rows
					{
						AllRecordsBuffered = true; // indicates we have no records left in the Query engine, we have buffered all of them
						if (DebugBasics) ClientLog.Message("All rows have been read from the QE: " + TotalRowsReadToBuffer);
					}
				}

				else // get a single row from cache file
				{
					int charsRead = CacheReader.Read(recLenBuf, 0, 8);
					if (charsRead == 8)
					{
						int recLen = int.Parse(new string(recLenBuf));
						char[] recBuf = new char[recLen];
						charsRead = CacheReader.Read(recBuf, 0, recLen);
						object[] vo = VoArray.DeserializeText(new string(recBuf));
						voRows = new List<object[]>();
						voRows.Add(vo);
						if (DebugBasics && voRows != null) ClientLog.Message("RetrieveAndBufferRowsLoopThread rows read from cache: " + voRows.Count);
					}

					else if (charsRead == 0) // at end of caching file
					{
						CacheReader.Close();
						CacheReader = null;
						voRows = null;
					}

					else throw new Exception("Expected 8 char record length");

					if (DebugCaching) ClientLog.Message("RetrieveAndBufferRowsLoopThread CacheReader.Read: " + DataTableMx.Rows.Count +
							", " + DataTableFetchPosition);
				}

				if (voRows != null) TotalQeRowsRetrieved += voRows.Count;
				else // all rows have been read from the QE
				{
					//RetrieveAndBufferRowsThreadRunning = false;
					if (DebugDetails) ClientLog.Message("RetrieveAndBufferRowsLoopThread exit 1");
					return -1;
				}

				lock (DataTransferLock) // lock the row buffer while adding rows into it
				{

					foreach (object[] voRow in voRows)
					{
						RetrievedRowBuffer.Add(voRow);
					}

					CurrentRowsReadToBuffer += voRows.Count;
					TotalRowsReadToBuffer += voRows.Count;
				}

				//if (RowRetrievalState == RowRetrievalState.Paused) RowRetrievalState = RowRetrievalState; // debug

				if (PauseRowRetrievalThreadRequested)
				{
					PauseRowRetrievalThreadRequested = false;
					if (PauseRowRetrievalEnabled) // pause retrieval if enabled
					{
						RowRetrievalState = RowRetrievalState.Paused;
						if (DebugDetails) ClientLog.Message("RowRetrievalState.Paused 8");
						RemoveRetrievingDataMessageRow();
						//RetrieveAndBufferRowsThreadRunning = false;
						if (DebugDetails) ClientLog.Message("RetrieveAndBufferRowsLoopThread exit 2");
						return -1;
					}
				}

				else if (StopRowRetrievalThreadRequested)
				{
					CloseResultsReader();

					StopRowRetrievalThreadRequested = false;
					RowRetrievalState = RowRetrievalState.Cancelled;
					if (DebugDetails) ClientLog.Message("RowRetrievalState.Cancelled 9");
					//RowRetrievalThreadRunning = false;
					RemoveRetrievingDataMessageRow();
					if (DebugDetails) ClientLog.Message("RetrieveAndBufferRowsLoopThread exit 3");
					return -1;
				}

				if (voRows != null) return voRows.Count;
				else return 0;
			}

			catch (Exception ex) // unexpected exception
			{
				if (DebugDetails) ClientLog.Message("RetrieveAndBufferRowsLoopThread exception - ");
				StopRowRetrievalThreadRequested = false;
				RowRetrievalState = RowRetrievalState.Cancelled;
				//RetrieveAndBufferRowsThreadRunning = false;
				RemoveRetrievingDataMessageRow();
				RetrieveAndBufferRowsLoopThreadException = ex;

				string msg = DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg); // try to log on server
				return -1;
			}

		}

		/// <summary>
		/// Close any open query engine or caching reader
		/// </summary>

		void CloseResultsReader()
		{
			if (Qe != null)
				Qe.Close(); // close the query

			if (CacheReader != null)
			{
				CacheReader.Close();
				CacheReader = null;
			}

			return;
		}

		/// <summary>
		/// This method is called as a result of a timer tick and transfers any buffered rows
		/// from the RetrievedRowsBuffer to the DataTable from which they are rendered.
		/// This update must be done from the UI thread rather than the non-UI database-retrieval
		/// thread.
		/// </summary>

		private void TransferFromBufferToDataTableTimer_Tick(object sender, EventArgs e)
		{
			bool updatingGrid = false;

			if (UseGridBeginAndEndUpdate == null) // get inifile setting if needed
			{
				UseGridBeginAndEndUpdate = ServicesIniFile.ReadBool("UseGridBeginAndEndUpdate", true);
			}

			try
			{

				Stopwatch tsw = Stopwatch.StartNew(); // total stop watch
				Stopwatch sw = Stopwatch.StartNew(); // incremental sw

				//if (DebugDetails) ClientLog.Message("RetrieveAndBufferRowsTransferTimer_Tick Entry");

				if (DataTableMx == null) return;

				if (Grid?.V != null && Grid.V.IsEditing)
				{ // don't allow any rows to be added to the datatable while editing since it puts a MemoEdit control in an invalid state
					PauseRowRetrievalThreadRequested = true;
					if (DebugDetails) ClientLog.Message("PauseRowRetrievalThreadRequested 2");
					return;
				}

				if (InTransferFromBufferToDataTableTimer_Tick)
				{
					if (DebugDetails) ClientLog.Message("InTransferFromBufferToDataTableTimer_Tick = true");
					return;
				}

				if (SessionManager.IsInShutDown) return;

				InTransferFromBufferToDataTableTimer_Tick = true;
				TransferFromBufferToDataTableTimer.Enabled = false;

				bool addingNewRows = (RetrievedRowBuffer.Count > 0); // are there new rows to add to the data table?
				if (DebugBasics && addingNewRows) ClientLog.Message("Rows available in RetrievedRowBuffer (Pre-lock): " + RetrievedRowBuffer.Count);
				RowsTransferredToDataTableCount = 0; // init count

				updatingGrid = // flag indicating that Grid is being updated
				 DataTableMx.DataChangedEventHandlersEnabled &&
				 addingNewRows;

				//updatingGrid = false; // debug!!

				bool useGridBeginAndEndUpdate = (bool)UseGridBeginAndEndUpdate; // normally delay display update while rows are being added to data table

				if (updatingGrid && Grid?.LV != null && DataTableMx != null) // Special handling for layout view
				{
					//if (DataTableMx.Rows.Count < 10)
					//	useGridBeginAndEndUpdate = false; // turn off delay for first few LayoutView records
				}

				if (RetrieveAndBufferRowsLoopThreadException != null) // show any exception that occurred while retrieving rows
				{
					Exception ex = RetrieveAndBufferRowsLoopThreadException;
					string msg = "Unexpected error retrieving data: \r\n\r\n" +
							DebugLog.FormatExceptionMessage(ex);
					MessageBoxMx.ShowError(msg);
					RetrieveAndBufferRowsLoopThreadException = null; // don't show again
				}

				if (Grid != null && updatingGrid && useGridBeginAndEndUpdate)
				{
					Grid.BeginUpdate();

					if (DataTableMx != null) // turn off event handlers for table
						DataTableMx.DataChangedEventHandlersEnabled = false;

					if (DebugBasics) ClientLog.Message("Grid.BeginUpdate(), Time(ms): " + StopwatchMx.GetMsAndReset(sw));
				}

				// Transfer any new rows from the input buffer to the DataTable


				if (addingNewRows)
				{
					lock (DataTransferLock) // lock the row buffer and DataTable while moving rows between them
					{
						sw.Restart();
						bool removedRetrievingDataRow = RemoveRetrievingDataMessageRow();

						RowsTransferredToDataTableCount = RetrievedRowBuffer.Count; // initially set the number of rows to transfer to the number of rows available in buffer

						if (Rf.Grid && RowsTransferredToDataTableCount > DataTableMx.Rows.Count) // if grid, scale back number of rows to transfer initially for better interaction
							RowsTransferredToDataTableCount = DataTableMx.Rows.Count + 1;

						if (RowsTransferredToDataTableCount > RetrievedRowBuffer.Count) // be sure we haven't gone beyond the number of rows available in buffer
							RowsTransferredToDataTableCount = RetrievedRowBuffer.Count;

						//rowsToTransfer = 1; // debug!!!

						for (int voi = 0; voi < RowsTransferredToDataTableCount; voi++)
						{
							object[] voRow = RetrievedRowBuffer[voi];
							DataRowMx dr = AddDataRow(voRow);
						}

						if (RowsTransferredToDataTableCount >= RetrievedRowBuffer.Count)
							RetrievedRowBuffer.Clear();
						else RetrievedRowBuffer.RemoveRange(0, RowsTransferredToDataTableCount);

						CurrentRowsTransferredToDataTable += RowsTransferredToDataTableCount;
						TotalRowsTransferredToDataTable += RowsTransferredToDataTableCount;

						if (DebugBasics && RowsTransferredToDataTableCount > 0)
							ClientLog.Message("Rows added to DataTable: " + RowsTransferredToDataTableCount + ", total rows: " + DataTableMx.Rows.Count + ", time(ms): " + StopwatchMx.GetMsAndReset(sw));

						if (removedRetrievingDataRow) AddRetrievingDataMessageRow();

						if (CurrentRowsRequested > 0 && CurrentRowsTransferredToDataTable >= CurrentRowsRequested)
						{
							PauseRowRetrievalThreadRequested = true;
							if (DebugDetails) ClientLog.Message("PauseRowRetrievalThreadRequested 3");
						}
					}

					//if (DebugDetails) ClientLog.Message("Exited row lock, Tick time(ms): " + sw.Elapsed.TotalMilliseconds);
				}

				// Update row count used for display. Reduce by 1 if there is a retrieving data message row.

				if (DataTableMx != null)
				{
					RowCount = HasRetrievingDataMessageRow() ? DataTableMx.Rows.Count - 1 : DataTableMx.Rows.Count;
				}
				else
				{
					RowCount = 0;
				}

				if (!RetrieveAndBufferRowsLoopThreadRunning && RetrievedRowBuffer.Count == 0) // if RowRetrieval not running && all rows transferred then no reason for timer to run
				{ // disable timer & update status if row retrieval no longer running
					TransferFromBufferToDataTableTimer.Enabled = false;
					if (DebugDetails) ClientLog.Message("TransferFromBufferToDataTableTimer disabled 1");

					if (RowRetrievalState != RowRetrievalState.Paused) // if not pausing then must be complete
					{
						RowRetrievalState = RowRetrievalState.Complete;
						RemoveRetrievingDataMessageRow();
						if (DebugDetails) ClientLog.Message("RowRetrievalState.Complete 10");
					}
				}

				if (Progress.GlobalCancelRequested)
				{
					PauseRowRetrievalRequested = true; // just pause if cancelled
					Progress.GlobalCancelRequested = false;
				}

				if (PauseRowRetrievalRequested) // pause retrieval
				{
					PauseRowRetrievalThreadRequested = true;
					if (DebugDetails) ClientLog.Message("PauseRowRetrievalThreadRequested 4");
					TransferFromBufferToDataTableTimer.Enabled = false;
					if (DebugDetails) ClientLog.Message("TransferFromBufferToDataTableTimer disabled 2");
					PauseRowRetrievalRequested = false;
					RowRetrievalState = RowRetrievalState.Paused;
					if (DebugDetails) ClientLog.Message("RowRetrievalState.Paused 11");
					//if (StatusBarManager != null)
					//  StatusBarManager.DisplayRetrievalProgressStateAndFilterCounts(RowRetrievalState);
					RemoveRetrievingDataMessageRow();
					Progress.Hide();
				}

				else if (CancelRowRetrievalRequested) // cancel retrieval
				{
					StopRowRetrievalThreadRequested = true;
					if (Qm.ResultsFormat.ParentQe == null) // cancel query engine if not being used by a parent query
						Qe.Cancel(false);
					TransferFromBufferToDataTableTimer.Enabled = false;
					if (DebugDetails) ClientLog.Message("TransferFromBufferToDataTableTimer disabled 3");
					CancelRowRetrievalRequested = false;
					RowRetrievalState = RowRetrievalState.Cancelled;
					if (DebugDetails) ClientLog.Message("RowRetrievalState.Cancelled 12");
					//if (StatusBarManager != null)
					//  StatusBarManager.DisplayRetrievalProgressStateAndFilterCounts(RowRetrievalState);

					RemoveRetrievingDataMessageRow();
					Progress.Hide();
				}

				// Refresh the grid if data updated and delaying the update

				if (Grid != null && updatingGrid) // make changes appear in grid if updating
				{
					if (useGridBeginAndEndUpdate)
					{
						if (DataTableMx != null) // turn event handlers for table back on
							DataTableMx.DataChangedEventHandlersEnabled = true;

						Grid.EndUpdate();
						if (DebugBasics) ClientLog.Message("Grid.EndUpdate(), Time(ms): " + StopwatchMx.GetMsAndReset(sw));
					}

					if (RowsTransferredToDataTableCount > 0) // refresh grid if rows added
					{
						if (Grid?.BGV != null) // if BandedGridView refresh datasource now
							RefreshGridData();

						else if (Grid?.LV != null) // if LayoutView refresh datasource via a separate timer (still needed?)
						{
							SecondaryGridRefreshScheduled = true;
							SecondarGridRefreshTimer.Enabled = true;

							//RefreshGridData(); // direct update 
						}
					}
				}

				//if (KeyCount != DataTableMx.Rows.Count) KeyCount = KeyCount; // debug

				if (StatusBarManager != null) // update status bar, may process an event that generates a request for more rows
				{
					sw.Restart();
					StatusBarManager.DisplayRetrievalProgressStateAndFilterCounts(RowRetrievalState);
					if (DebugDetails) ClientLog.Message("DisplayRetrievalProgressStateAndFilterCounts, Time(ms): " + StopwatchMx.GetMsAndReset(sw));
					//if (Debug) ClientLog.Message("StatusBarUpdated 13");
				}

				bool reenableTimer = (RowRetrievalState == RowRetrievalState.Running);

				if (RowsTransferredToDataTableCount > 0)
				{

					double totalTime = tsw.Elapsed.TotalMilliseconds;

					if (QueryEngineStatsForm.ShowStats)
					{
						sw.Restart();
						QueryEngineStatsForm.UpdateStatsDisplay(Query, Qe, MetaBrokerStats);
						if (DebugDetails) ClientLog.Message("UpdateStatsDisplay, Time(ms): " + sw.Elapsed.TotalMilliseconds);
						if (AllRecordsBuffered) QueryEngineStatsForm.ExecutionComplete(); // RowRetrievalState == RowRetrievalState.Complete
					}

					if (DebugBasics)
					{
						ClientLog.Message("Exiting Tick, total time:(ms): " + totalTime + ", Timer.Enabled: " + reenableTimer);
					}
				}

				if (reenableTimer)  // restart timer if retrieval is still running
					TransferFromBufferToDataTableTimer.Enabled = true;

				else reenableTimer = reenableTimer; // debug

				InTransferFromBufferToDataTableTimer_Tick = false; // say not in tick so more rows can be retrieved

				return;
			}

			catch (Exception ex)
			{
				string msg = "Unexpected error retrieving data: \r\n\r\n" +
					DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				MessageBoxMx.ShowError(msg);
				if (Grid != null) Grid.EndUpdate();
				throw ex;
			}
		}

		/// <summary>
		/// Perform secondary refresh of grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SecondaryGridRefreshTimer_Tick(object sender, EventArgs e)
		{
			if (InSecondaryGridFrfreshTimer_Tick) return;

			if (!SecondaryGridRefreshScheduled) return;

			if (Grid?.Helpers != null && Grid.Helpers.InCustomUnboundColumnData) // avoid reentry to CustomUnboundColumnData
				return;
			
			InSecondaryGridFrfreshTimer_Tick = true;
			SecondaryGridRefreshScheduled = false;

			if (DebugBasics) ClientLog.Message("SecondaryGridRefreshTimer_Tick");

			RefreshGridData();

			InSecondaryGridFrfreshTimer_Tick = false;
			return;

			// Dev/Test code
			//Grid.SelectCell(Grid.DataTable.Rows.Count - 1, 0);
			//Grid.ScrollToBottom();
			//Size cms = Grid.LV.CardMinSize;
			//Grid.LV.CardMinSize = new Size(cms.Width-1, cms.Height+1);
			//Grid.LV.OptionsView.ViewMode = DevExpress.XtraGrid.Views.Layout.LayoutViewMode.Column;
			//Grid.LV.Refresh();
			//Grid.Invalidate();
			//Grid.RefreshDataSource();
			//Application.DoEvents();
		}

		/// <summary>
		/// Refresh the grid data
		/// </summary>

		private void RefreshGridData()
		{
			try
			{
				Stopwatch sw = Stopwatch.StartNew();

				Grid.RefreshDataSource(); // this may generate a request for more rows

				if (Grid?.BGV != null)
					Grid.BGV.RefreshData();

				else if (Grid?.LV != null) // need this refresh for LayoutView
					Grid.LV.Refresh();

				double gridRefreshTime = StopwatchMx.GetMsAndReset(sw);

				double rowRefreshTime = gridRefreshTime / RowsTransferredToDataTableCount; // keep refresh time interactive
				if (rowRefreshTime <= 0) rowRefreshTime = .2;
				MaxRowsToTransferToDataTablePerTick = (int)(2000 / rowRefreshTime) + 1;

				if (DebugBasics) ClientLog.Message("Grid.Refresh[Data] Time(ms): " + gridRefreshTime);

			}
			catch (Exception ex) { ex = ex; } // debug
		}

		/// <summary>
		/// Apply all defined filters
		/// </summary>

		internal void ApplyFilters()
		{
			if (FilteredColumnList == null) SetupFilteredColumnList();

			foreach (ColumnInfo colInfo in FilteredColumnList)
			{
				ApplyFilter(colInfo);
			}

			FiltersApplied = true;
		}

		/// <summary>
		/// Recalculate filter flags for current column
		/// </summary>

		internal void ApplyFilter(ColumnInfo colInfo)
		{
			// This routine handles the filter types listed below for all MetaColumnTypes other than structures. 
			// Unknown (i.e. No filter)
			// Eq 
			// Ne 
			// Lt 
			// Le 
			// Gt 
			// Ge 
			// In 
			// InList
			// Between
			// Like
			// IsNull
			// IsNotNull

			QueryManager qm = this.QueryManager;
			DataTableMx dt = qm.DataTable;

			int dci = colInfo.DataColIndex;
			ResultsTable rt = colInfo.Rt;
			ResultsField rfld = colInfo.Rfld;
			int ti = colInfo.TableIndex;
			int fi = colInfo.FieldIndex;
			QueryColumn qc = colInfo.Qc;
			MetaColumn mc = colInfo.Mc;
			MetaColumnType mct = mc.DataType;
			string compValString = null;
			Dictionary<string, object> compValStringList = null;
			List<MobiusDataType> compValNumericList1 = null, compValNumericList2 = null;
			MobiusDataType fv, cv = null, cv2 = null; // field value and comparison values in native column types
			bool pass;
			int rowPassCnt = 0, rowFailCnt = 0;
			string fieldValString;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(colInfo.Qc.SecondaryCriteria);
			if (psc == null || psc.OpEnum == CompareOp.Unknown) // no criteria, clear filter
			{
				ClearFilter(colInfo.Qc);
				return;
			}

			ResultsFormatter fmtr = qm.ResultsFormatter;

			compValString = psc.Value;

			if (
			 psc.OpEnum == CompareOp.Like ||
			 psc.OpEnum == CompareOp.IsNull || psc.OpEnum == CompareOp.IsNotNull)
			{ }

			else if (psc.OpEnum == CompareOp.In)
			{ // handle values (with epsilons for decimals) and 
				compValStringList = new Dictionary<string, object>();
				compValNumericList1 = new List<MobiusDataType>();
				compValNumericList2 = new List<MobiusDataType>();

				if (psc.ValueList != null && psc.ValueList.Count > 0)
				{
					foreach (string s in psc.ValueList)
					{
						compValStringList[s.ToUpper()] = null;

						if (mc.IsDecimal)
						{
							MobiusDataType.GetFuzzyEqualityComparators(qc.MetaColumn.DataType, s, out cv, out cv2);
							compValNumericList1.Add(cv);
							compValNumericList2.Add(cv2);
						}
					}
				}
			}

			else if ( // build appropriate MobiusDataTypes for the comparison values
			 psc.OpEnum == CompareOp.Eq || psc.OpEnum == CompareOp.Ne ||
			 psc.OpEnum == CompareOp.Lt || psc.OpEnum == CompareOp.Le ||
			 psc.OpEnum == CompareOp.Gt || psc.OpEnum == CompareOp.Ge ||
			 psc.OpEnum == CompareOp.Between)
			{
				cv = MobiusDataType.New(qc.MetaColumn.DataType, psc.Value);
				if (psc.OpEnum == CompareOp.Between)
					cv2 = MobiusDataType.New(qc.MetaColumn.DataType, psc.Value2);
				else cv2 = null;

				if (mc.IsDecimal)
				{ // adjust decimal comparison values by an epsilon
					double e = MobiusDataType.GetEpsilon(psc.Value);

					if (psc.OpEnum == CompareOp.Eq) // do equal as between with epsilons
					{
						cv2 = MobiusDataType.New(qc.MetaColumn.DataType, psc.Value);
						psc.OpEnum = CompareOp.Between;
					}

					double d1 = cv.NumericValue;

					if (psc.OpEnum == CompareOp.Le)
						cv.NumericValue += e;

					else if (psc.OpEnum == CompareOp.Ge)
						cv.NumericValue -= e;

					else if (psc.OpEnum == CompareOp.Between)
					{
						cv.NumericValue -= e; // reduce lower value by epsilon
						cv2.NumericValue += e; // increase upper value by epsilon
					}

				}
			}

			else if (psc.OpEnum == CompareOp.SSS)
			{
				MoleculeMx cs = new MoleculeMx(MoleculeFormat.Chime, psc.Value);
				StrMatcher = new StructureMatcher(); // allocate structure matcher
				StrMatcher.SetSSSQueryMolecule(cs);
			}

			else throw new Exception("Unexpected CompareOp: " + psc.Op);

			// Check each value in the column

			for (int dri = 0; dri < dt.Rows.Count; dri++)
			{
				//if (dri == 1) dri = dri; // debug

				DataRowMx dr = dt.Rows[dri];
				object o = dr[dci];

				// "Nonexistant" values always pass filter

				if (NullValue.IsNonExistant(o))
				{
					if (!(o is MobiusDataType))
					{
						MobiusDataType mdt = new MobiusDataType();
						mdt.IsNonExistant = true;
						o = mdt;
						dr[dci] = o; // store back so filtered state is saved in this object
					}
					fv = o as MobiusDataType; // get the MobiusDataType version of the field value

					fv.Filtered = false;
					rowPassCnt++;
					continue;
				}

				// Set filter value for a null field value

				if (NullValue.IsNull(o))
				{
					if (mc.IsKey) continue; // if null key value then must be null secondary master record (already processed primary) 

					if (!(o is MobiusDataType))
					{
						o = MobiusDataType.New(colInfo.Mc.DataType); // create null value of column type
						dr[dci] = o; // store back so filtered state is saved in this object
					}
					fv = o as MobiusDataType; // get the MobiusDataType version of the field value

					switch (psc.OpEnum)
					{
						case CompareOp.Like:
							pass = String.IsNullOrEmpty(compValString);
							break;

						case CompareOp.In:
							pass = compValStringList.ContainsKey("(ALL)") ||
							 compValStringList.ContainsKey("ALL") ||
							 compValStringList.ContainsKey("(BLANKS)") ||
							 compValStringList.ContainsKey("BLANKS");
							break;

						case CompareOp.IsNull:
							pass = true;
							break;

						default:
							pass = false;
							break;
					}

					fv.Filtered = !pass;
					if (pass) rowPassCnt++;
					else rowFailCnt++;

					continue;
				}

				// Set filter value for a non-null field value

				if (!(o is MobiusDataType))
				{
					o = MobiusDataType.New(o); // create null value of proper type based on type of existing object
					dr[dci] = o;
				}
				fv = o as MobiusDataType; // get the MobiusDataType version of the field value

				if (fv.FormattedText == null) // get formatted text if not done yet
				{
					FormattedFieldInfo ffi = fmtr.FormatField(rt, ti, rfld, fi, dr, dri, fv, -1, false);
					fv.FormattedText = ffi.FormattedText;
					if (fv.FormattedText == null) fv.FormattedText = "";
				}

				fieldValString = fv.FormattedText;

				if ((mc.DataType == MetaColumnType.CompoundId || mc.DataType == MetaColumnType.Date) &&
				 psc.OpEnum != CompareOp.Like) // for other than Like
					fieldValString = fv.FormatForCriteria(); // do string compare against normalized form of value for these
																									 //				ClientLog.Message(compValString);

				switch (psc.OpEnum) // handle each allowed operator (switch should generate optimized jump table)
				{
					case CompareOp.Eq:
						pass = fv.CompareTo(cv) == 0;
						break;

					case CompareOp.Ne:
						pass = fv.CompareTo(cv) != 0;
						if (pass && Lex.Eq(fieldValString, compValString)) pass = false;
						break;

					case CompareOp.Lt:
						pass = fv.CompareTo(cv) <= -1;
						break;

					case CompareOp.Le:
						pass = fv.CompareTo(cv) <= 0;
						break;

					case CompareOp.Gt:
						pass = fv.CompareTo(cv) >= 1;
						break;

					case CompareOp.Ge:
						pass = fv.CompareTo(cv) >= 0;
						break;

					case CompareOp.In:
						pass = compValStringList.ContainsKey(fieldValString.ToUpper()) ||
						 compValStringList.ContainsKey("(ALL)") ||
						 compValStringList.ContainsKey("ALL") ||
						 compValStringList.ContainsKey("(NOTBLANK)") ||
						 compValStringList.ContainsKey("NOTBLANK");

						if (!pass && mc.IsDecimal)
						{ // see if matches within epsilon tolerance
							for (int i1 = 0; i1 < compValNumericList1.Count; i1++)
							{
								cv = compValNumericList1[i1];
								cv2 = compValNumericList2[i1];
								pass = fv.CompareTo(cv) >= 0 && fv.CompareTo(cv2) <= 0;
								if (pass) break;
							}
						}
						break;

					case CompareOp.InList:
						pass = compValStringList.ContainsKey(fieldValString.ToUpper());
						break;

					case CompareOp.Between:
						pass = fv.CompareTo(cv) >= 0 && fv.CompareTo(cv2) <= 0;
						break;

					case CompareOp.Like:
						pass = Lex.Contains(fieldValString, compValString) || compValString == "";
						break;

					case CompareOp.IsNull:
						pass = false; // nulls checked earlier so this must be non-null
						break;

					case CompareOp.IsNotNull:
						pass = true; // nulls checked earlier so this must be non-null
						break;

					case CompareOp.SSS:
						MoleculeMx cs = fv as MoleculeMx;
						if (cs != null)
						{
							if (StrMatcher.IsSSSMatch(cs)) pass = true;
							else pass = false;
						}
						else pass = false; // not a structure
						break;

					default:
						throw new Exception("Unexpected CompareOp: " + psc.Op);

				} // end of switch

				fv.Filtered = !pass;
				if (pass) rowPassCnt++;
				else rowFailCnt++;
			} // row loop

			return;
		}

		/// <summary>
		/// Reset the data for this manager
		/// </summary>

		internal void ResetDataState()
		{
			RowRetrievalState = RowRetrievalState.Undefined;
			if (DebugDetails) ClientLog.Message("RowRetrievalState.Undefined 11");

			KeyCount = RowCount = -1; // total keys/rows
			PassedFiltersKeyCount = PassedFiltersRowCount = -1; // keys/rows remaining after filtering
			MarkedKeyCount = MarkedRowCount = -1; // keys/rows marked
			SelectedKeyCount = SelectedRowCount = -1; // keys/rows selected
			SelectedKeys = null; // indexes of selected key rows
			MarkedKeys = null; // indexes of marked key rows

			// Don't reset filter state since this may be an export of filered grid data
			//FiltersEnabled = false; // flag indicating if secondary filters are enabled 
			//FilteredColumnList = null; // list of columns with secondary filters
			//FiltersApplied = false; // true if existing filters have been applied to the current data table

			CurrentRowsRequested = 0; // number of rows to read for current request
			CurrentRowsReadToBuffer = 0; // number of rows read and transferred to the buffer in current request
			TotalRowsReadToBuffer = 0; // total number of rows read and transferred to the buffer in current request
			CurrentRowsTransferredToDataTable = 0; // number of rows read and transferred to the DataTable in current request
			TotalRowsTransferredToDataTable = 0; // total number of rows read and transferred to the DataTable in current request
			ReadRequests = 0; // number of read requests
			RetrieveAndBufferRowsLoopThreadRunning = false; // true if the row retrieval thread is running
			CancelRowRetrievalRequested = false; // true if row retrieval should be cancelled;
			PauseRowRetrievalThreadRequested = false; // if true row retrieval should be paused
			StopRowRetrievalThreadRequested = false; // if true then row retrieval should be stopped
			RetrievedRowBuffer = new List<object[]>(); // retrieved data rows waiting to be moved to the DataTable & grid

			TotalQeRowsRetrieved = 0; // total rows retrieved from query engine
			AllRecordsBuffered = false; // true if all rows have been read from the Qe

			DataTableFetchPosition = -1; // where we are in scan of data table
			LastDataTableFetchPosition = -1;
			LastDataTableRowCount = -1;
			//DataTableFetchCount = 0;

			return;
		}

		/// <summary>
		/// Reset all secondary filter criteria keeping any filter types
		/// </summary>

		internal void ResetFilters()
		{
			if (QueryManager == null || QueryManager.Query == null) return;

			foreach (QueryTable qt in QueryManager.Query.Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					QueryColumn bqQc = QueriesControl.BaseQueryQc(qc); // modify base query also
					qc.SecondaryCriteria = bqQc.SecondaryCriteria = "";
					qc.SecondaryCriteriaDisplay = bqQc.SecondaryCriteriaDisplay = "";
				}
			}

			return;
		}

		/// <summary>
		/// Clear all secondary filter criteria
		/// </summary>

		internal void ClearFilters()
		{
			if (QueryManager == null || QueryManager.Query == null) return;

			foreach (QueryTable qt in QueryManager.Query.Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
					ClearFilter(qc);
			}

			return;
		}

		/// <summary>
		/// Clear filter information for a single filter
		/// </summary>
		/// <param name="qc"></param>

		internal void ClearFilter(QueryColumn qc)
		{
			QueryColumn bqQc = QueriesControl.BaseQueryQc(qc); // modify base query also
			qc.SecondaryFilterType = bqQc.SecondaryFilterType = FilterType.Unknown;
			qc.SecondaryCriteria = bqQc.SecondaryCriteria = "";
			qc.SecondaryCriteriaDisplay = bqQc.SecondaryCriteriaDisplay = "";
		}

		/// <summary>
		/// Get the most complete set of results keys as they currently exist
		/// </summary>
		/// <returns></returns>

		public List<string> GetMostCompleteResultsKeyList()
		{
			List<string> resultKeys = ResultsKeys;
			List<string> resultKeys2 = GetResultsKeysFromDataTable();

			if (resultKeys2 != null)
			{
				if (resultKeys == null || resultKeys2.Count > resultKeys.Count)
					return resultKeys2;
			}

			return resultKeys;
		}

		/// <summary>
		/// Verify that the set of results keys has been updated
		/// </summary>

		void VerifyResultsKeysUpToDate()
		{
			if (ResultsKeys.Count < KeyCount && MqlUtil.SingleStepExecution(QueryManager.Query))
				SetResultsKeysFromDatatable();

			return;
		}

		/// <summary>
		/// Set the list of results keys from the data table contents
		/// </summary>
		public void SetResultsKeysFromDatatable()
		{
			ResultsKeys = GetResultsKeysFromDataTable();
			KeyCount = ResultsKeys.Count;
			return;
		}

		/// <summary>
		/// Scan the data table & return the list of keys
		/// </summary>
		/// <returns></returns>

		public List<string> GetResultsKeysFromDataTable()
		{
			string curKeyVal = "";
			int curKeyRow = -1;
			int rowsForKey = -1;
			List<string> keyList = new List<string>();
			if (DataTableMx == null) return keyList;

			HashSet<string> keySet = new HashSet<string>();

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{
				DataRowMx dr = DataTableMx.Rows[dri];
				object[] drVo = dr.ItemArrayRef;
				DataRowAttributes dra = GetRowAttributes(dri);

				string keyVal = drVo[KeyValueVoPos] as string;

				if (keyVal != curKeyVal) // going to new key
				{
					curKeyVal = keyVal;
					curKeyRow = dri;
					rowsForKey = 0;
					if (!keySet.Contains(keyVal)) // add if not in set yet
					{
						keyList.Add(keyVal);
						keySet.Add(keyVal);
					}
				}

				rowsForKey++;
			}

			return keyList;
		}

		/// <summary>
		/// Utility routine to refill the DataTable with a new set of rows
		/// </summary>

		public void FillDataTable(
				List<object[]> voList)
		{
			lock (DataTransferLock) // lock the DataTable while changing
			{
				DataTableMx.Clear();
				Grid.BeginUpdate();

				for (int voi = 0; voi < voList.Count; voi++)
				{
					object[] voRow = voList[voi];
					DataRowMx dr = AddDataRow(voRow);
				}

				InitializeRowAttributes();
				//InitializeSubrowPositions();

				SetResultsKeysFromDatatable(); // set the results keys

				Grid.DataSource = DataTableMx; // be sure datasource is properly set
				Grid.EndUpdate();
				Grid.RefreshDataSource();
			}

			return;
		}

		/// <summary>
		/// Get statistics for column ignoring any filtering
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public ColumnStatistics GetStats(QueryColumn qc)
		{
			return GetStats(qc, false);
		}

		/// <summary>
		/// Get statistics for column, calculating if necessary
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="filtered"></param>
		/// <returns></returns>

		public ColumnStatistics GetStats(QueryColumn qc, bool filtered)
		{
			if (qc == null) return null;
			ResultsField rf = Rf.GetResultsField(qc);
			if (rf == null) return new ColumnStatistics();
			return rf.GetStats(); // todo: move Rf stats methods to DataTableManager
		}

		/// <summary>
		/// Calc the stats for a column
		/// </summary>
		/// <param name="colInfo"></param>
		/// <returns></returns>

		internal ColumnStatistics CalcColumnStatistics(ColumnInfo colInfo)
		{
			ColumnStatistics stats = new ColumnStatistics();
			string dictKey;

			int dci = colInfo.DataColIndex;
			ResultsTable rt = colInfo.Rt;
			ResultsField rfld = colInfo.Rfld;
			int ti = colInfo.TableIndex;
			int fi = colInfo.FieldIndex;
			QueryColumn qc = colInfo.Qc;
			MetaColumn mc = colInfo.Mc;

			string curKeyVal = "";
			int curKeyRow = -1;
			int rowsForKey = -1;

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{
				DataRowMx dr = DataTableMx.Rows[dri];
				object[] drVo = dr.ItemArrayRef;
				DataRowAttributes dra = GetRowAttributes(drVo);

				string keyVal = drVo[KeyValueVoPos] as string;

				if (keyVal != curKeyVal || Rf.Tables.Count <= 1) // going to new key
				{
					curKeyVal = keyVal;
					curKeyRow = dri;
					rowsForKey = 0;
				}

				rowsForKey++;

				object o = drVo[dci];
				//				if (o is string && ((string)o) == "") o = o; // debug
				if (NullValue.IsNull(o))
				{
					if (rowsForKey == 0 || // count as null if first row for key
					 dra.TableRowState[colInfo.TableIndex] != RowStateEnum.Undefined) // or real row data but col is null
						stats.NullsExist = true;
					continue;
				}

				else if (!(o is MobiusDataType))
				{ // create a MobiusDataType that we can point to
					o = MobiusDataType.New(colInfo.Mc.DataType, o);
					drVo[dci] = o; // store back in dataRow (really want to do this?)
				}

				MobiusDataType val = o as MobiusDataType;

				try
				{
					//if (val is QualifiedNumber) // just format the basic number without qualifier, etc.
					//{
					//  QualifiedNumber qn = val as QualifiedNumber;
					//  val = qn.Clone(); // make clone since formatted text is in special format
					//  val.FormattedText = QualifiedNumber.FormatNumber(qn.NumberValue, qc.DisplayFormat, qc.Decimals);
					//}

					if (val.FormattedText == null) // get formatted text if not done yet
					{
						FormattedFieldInfo ffi = QueryManager.ResultsFormatter.FormatField(rt, ti, rfld, fi, dr, dri, val, -1, false);
						val.FormattedText = ffi.FormattedText;
						if (val.FormattedText == null) val.FormattedText = "";
					}

					//if (val is CompoundId) // use normalized form string (throws exception)
					//  dictKey = (val as CompoundId).Value;

					//else 
					dictKey = val.FormattedText.ToUpper();

					if (!stats.DistinctValueDict.ContainsKey(dictKey))
					{
						DistinctValueAndPosition dvp = new DistinctValueAndPosition();
						val = val.Clone(); // make copy to protect stats from unwanted changes
						dvp.Value = val;
						stats.DistinctValueDict[dictKey] = dvp;
					}
				}
				catch (Exception ex) { val = val; } // shouldn't happen
			} // row loop

			List<MobiusDataType> vl = new List<MobiusDataType>(stats.DistinctValueDict.Values.Count);

			// Build ordered list of distinct values

			foreach (DistinctValueAndPosition dvp0 in stats.DistinctValueDict.Values)
				vl.Add(dvp0.Value); // build basic list

			vl.Sort(); // sort it
			if (vl.Count > 0) // get min & max
			{
				stats.MinValue = vl[0];
				stats.MaxValue = vl[vl.Count - 1];
			}

			for (int dvi = 0; dvi < vl.Count; dvi++) // build ordered list
			{
				string dv = vl[dvi].FormattedText.ToUpper();
				DistinctValueAndPosition dvp = stats.DistinctValueDict[dv];
				MobiusDataType mdt = dvp.Value;
				stats.DistinctValueList.Add(mdt); // store Mobius data type as distinct value

				dvp.Ordinal = dvi; // ordinal for this value
			}

			stats.DistinctValueDict = stats.DistinctValueDict; // return dictionary also

			if (qc.MetaColumn.DataType == MetaColumnType.Structure)
			{
				//ClientLog.Message("");
				//ClientLog.Message("Stats");
				//foreach (MobiusDataType mdt in stats.DistinctValueList)
				//{
				//  ClientLog.Message(mdt.FormattedText);
				//}

			}

			return stats;
		}

		/// <summary>
		/// Get the ordinal position of a value within the list of values for a column
		/// </summary>
		/// <param name="colInfo"></param>
		/// <param name="stats"></param>
		/// <param name="obj"></param>
		/// <returns></returns>

		public int GetValueOrdinal(
				//ColumnInfo colInfo,
				ColumnStatistics stats,
				object obj)
		{
			//int dci = colInfo.DataColIndex;
			//ResultsTable rt = colInfo.Rt;
			//ResultsField rfld = colInfo.Rfld;
			//int ti = colInfo.TableIndex;
			//int fi = colInfo.FieldIndex;
			//QueryColumn qc = colInfo.Qc;
			//MetaColumn mc = colInfo.Mc;

			if (obj is MobiusDataType) throw new Exception("Value is not a MobiusDataType");
			MobiusDataType val = obj as MobiusDataType;
			if (val.FormattedText == null) throw new Exception("Value not formatted");

			string dictKey = val.FormattedText.ToUpper();
			if (!stats.DistinctValueDict.ContainsKey(dictKey)) throw new Exception("Value not found");

			int ordinal = stats.DistinctValueDict[dictKey].Ordinal;
			return ordinal;
		}

		/// <summary>
		/// Update list of columns with filters and the overall filter flags for each row in the dataset
		/// </summary>

		internal void UpdateFilterState()
		{

			// Filtering sets the Filtered flag and FilteredRowPos array.
			// Filtering is applied to the set of rows for each key value
			// as a group and each table is treated as an independent entity
			// so that table-rows that don't align in the DataTable may end up
			// aligned as a result of filtering. FilteredRowPos contains the index
			// of each row for each table that makes up the final row as viewed
			// by the user.

			DataRowMx dr, dr2;
			DataRowAttributes dra, dra2;
			ColumnInfo cInf = null;
			object[] vo;
			string keyVal, keyVal2, txt;
			int dri, dri0, firstRowForCurrentKey, firstRowForNextKey, noRowAssignedCount = 0, rowReassignedCount = 0, tri, ci, pi, ti, fi, voPos, i1;

			//ClientLog.Message("\r\nUpdateFilterState");

			if (RowAttributesVoPos < 0) return; // can't do if no attributes in table

			KeyCount = PassedFiltersKeyCount = 0;
			RowCount = PassedFiltersRowCount = 0;
			MarkedKeyCount = MarkedRowCount = 0;
			MarkedKeys = new Dictionary<string, int>();

			SelectedKeyCount = SelectedRowCount = 0;
			SelectedKeys = new Dictionary<string, int>();

			SetupFilteredColumnList();
			List<ColumnInfo>[] fpt = SetupFiltersPerTable(); // get list of filters for each table

			ResultsFormat rf = Rf;
			if (rf == null) rf = new ResultsFormat();
			int tableCount = rf.Tables.Count;

			int filteredTableCount = 0; // count number of tables with filters
			for (ti = 0; ti < tableCount; ti++) // assign a subrow from each table
			{
				if (FiltersEnabled && fpt[ti] != null && fpt[ti].Count > 0)
					filteredTableCount++;
			}

			InitializeSubrowPositions();

			dri = -1; // dri is the row in the table currently being assigned a Filtered value & SubRowPos
			firstRowForNextKey = 0;

			int[] subRowPos = // subrow position for each table within datatable for each main row
					new int[tableCount]; // these values are always greater or equal to the main row index depending on filtering

			////////////////////////////
			// Process each key value //
			////////////////////////////

			while (true)
			{
				firstRowForCurrentKey = firstRowForNextKey; // move to next key
				dri = firstRowForCurrentKey; // dri is used to scan forward through rows for the current key
				if (dri >= DataTableMx.Rows.Count) break;
				dr = DataTableMx.Rows[firstRowForCurrentKey]; // first row for new key
				dra = GetRowAttributes(dr);
				keyVal = dr[KeyValueVoPos] as string;
				KeyCount++;

				for (firstRowForNextKey = dri + 1; firstRowForNextKey < DataTableMx.Rows.Count; firstRowForNextKey++)
				{ // find beginning of following key && assign to dri2, this is where we stop in the current scan
					dr2 = DataTableMx.Rows[firstRowForNextKey];
					keyVal2 = dr2[KeyValueVoPos] as string;
					if (keyVal2 != keyVal) break;
				}

				for (ti = 0; ti < tableCount; ti++) // init row position for each table
					subRowPos[ti] = dri - 1; // set initial index to "last matched" row

				////////////////////////////////////////////////
				// Process each of the rows for the key value //
				////////////////////////////////////////////////

				int rowsPassedForKey = 0; // total rows passed for the key
				while (dri < firstRowForNextKey) // for each row for key assign a set of subrows from this or other rows
				{ // it not at least one row matches for each table with a filter eliminate the key all together
					dr = DataTableMx.Rows[dri]; // first row for new key
					dra = GetRowAttributes(dr);

					//if (dri == 1) dri = dri; // debug

					//////////////////////////////////////////////////////////////
					// Assign a subrow from each table (may be an empty subrow) //
					//////////////////////////////////////////////////////////////
					// For each table one of the following will be true
					//  1. Table has filters & a row that passes them (filteredTablesPassing)
					//  2. Table has filters, no passing row now but previously had one (filteredTablesPassingBasedOnPreviousPass)
					//  3. Table has filters but no row for the key passes them (filteredTablesNotPassing)
					//  4. Table has no filters and an unassigned real row exists (unfilteredTablesWithRealRow)
					//  5. Table has no filters and only null rows left (unfilteredTablesWithNullRow)

					int filteredTablesPassing = 0; // number of tables with filters and with rows that pass the filters
					int filteredTablesPassingBasedOnPreviousPass = 0; // number of tables with filters that had a previous pass for this key but no additional rows that pass
					int filteredTablesNotPassing = 0; // no passing rows for the key if this happens
					int unfilteredTablesWithRealRow = 0; // unfiltered tables that had non-null data subrows
					int unfilteredTablesWithNullRow = 0; // unfiltered tables that had null data subrows

					for (ti = 0; ti < tableCount; ti++) // assign a subrow from each table (may be a null row)
					{
						if (subRowPos[ti] < firstRowForNextKey && subRowPos[ti] != NullRowIndex)
							subRowPos[ti]++; // advance to next row for this table

						// Unfiltered table

						if (!FiltersEnabled || fpt[ti] == null || fpt[ti].Count == 0)
						{ // passes if not filtering or no filters for table
							if (subRowPos[ti] < firstRowForNextKey && subRowPos[ti] != NullRowIndex) // get the row
								dr2 = DataTableMx.Rows[subRowPos[ti]]; // get the row
							else dr2 = null; // no more rows for key

							if (dr2 == null)
								unfilteredTablesWithNullRow++;

							else if (ti == 0 && tableCount > 1 && dri > firstRowForCurrentKey) // key table beyond 1st row
								unfilteredTablesWithNullRow++;

							else // other table check for real row
							{
								//if (ti == 2 && rowPos[ti] == 3) dra2 = null; // debug
								dra2 = GetRowAttributes(dr2);
								RowStateEnum[] trsa = dra?.TableRowState;
								if (trsa != null && ti < trsa.Length && trsa[ti] != RowStateEnum.Undefined) // real data?
									unfilteredTablesWithRealRow++;
								else unfilteredTablesWithNullRow++;
							}

							if (rf.DuplicateKeyTableValues && MaxRowsPerKey[ti] == 1)
							{
								subRowPos[ti] = firstRowForCurrentKey; // reuse the first row for duplicates if only one row per key
								rowReassignedCount++;
							}

							else noRowAssignedCount++; // no row assigned

							continue; // to next table
						}

						// If filtered key table beyond 1st row then pass it always if it's passed once

						MetaTable mt = Query.Tables[ti].MetaTable;

						if (mt.IsRootTable && tableCount > 1 && rowsPassedForKey > 0)
						{ // for multi-table queries if 1st table has passed once then always pass it

							if (rf.DuplicateKeyTableValues)
							{
								subRowPos[ti] = firstRowForCurrentKey; // reuse the first row for duplicates
								rowReassignedCount++;
							}

							else subRowPos[ti] = dri; // use this row which just has key value

							filteredTablesPassingBasedOnPreviousPass++; // count it as passing
							continue;
						}

						// Other filtered table, scan rows until we find one that passes or reach end of rows for key

						while (true) // scan the rows for this table until we have a match or we are out of rows for this key
						{
							tri = subRowPos[ti]; // get row index for table

							// If past the rows for this key without a passing row then if least one row has already passed for this key pass this table anyway with a null row

							if (subRowPos[ti] >= firstRowForNextKey || subRowPos[ti] == NullRowIndex) // past the rows for this key?
							{
								if (tableCount > 1 && rowsPassedForKey > 0) // if we've had a hit for this key then this this col must have passed at some point
								{ // give this base row (i.e. dri) a tentative overall pass
									if (rf.DuplicateKeyTableValues && MaxRowsPerKey[ti] == 1)
									{
										subRowPos[ti] = firstRowForCurrentKey; // reuse the first row for duplicate
										rowReassignedCount++;
									}

									else
									{
										subRowPos[ti] = NullRowIndex; // no row assigned
										noRowAssignedCount++;
									}

									filteredTablesPassingBasedOnPreviousPass++; // count as passed
								}

								else filteredTablesNotPassing++;

								break; // done with this table in any case
							}

							// See if all filters for this row pass

							dr2 = DataTableMx.Rows[tri];
							for (fi = 0; fi < fpt[ti].Count; fi++) // check each filter for the table
							{
								cInf = fpt[ti][fi];
								MobiusDataType mdt = dr2[cInf.DataColIndex] as MobiusDataType;
								if (mdt == null) continue; // visible if null?
								if (mdt.Filtered) break; // break if filtered out
							}

							if (fi >= fpt[ti].Count) // pass all filters?
							{
								filteredTablesPassing++;
								break;
							}

							if (tableCount == 1) // if only one table and this row fails overall, don't scan further
								break;

							if (mt.IsRootTable && tableCount > 1) // if first row of the root table didn't pass then no rows for key will pass
								break;

							subRowPos[ti]++; // advance row position for this table

						} // row loop for current table

						if (filteredTablesPassing == 0 && rowsPassedForKey == 0) break; // break if this table didn't pass & no rows have passed for this key
					} // table loop


					////////////////////////////////////////////////////////////////////
					// If a row passed clear the filtered flag & store subrow indexes //
					////////////////////////////////////////////////////////////////////

					bool passed = false;

					if (!FiltersEnabled || filteredTableCount == 0) // pass if filters not enabled
						passed = true;

					else
					{
						bool newDataFound = // did we find some new data in either the filtered or unfiltered tables?
								filteredTablesPassing > 0 || unfilteredTablesWithRealRow > 0;

						if (filteredTablesPassing + filteredTablesPassingBasedOnPreviousPass >= filteredTableCount
								&& newDataFound) passed = true;
					}

					if (passed)
					{
						dra.Filtered = false;
						Array.Copy(subRowPos, dra.SubRowPos, subRowPos.Length); // copy subrow position for each table

						for (ti = 0; ti < tableCount; ti++)
						{ // point subrecords that passed back to the associated main record
							int srp = subRowPos[ti];
							if (srp != NullRowIndex)
							{
								dr2 = DataTableMx.Rows[srp]; // get row containing subrecord
								dra2 = GetRowAttributes(dr2);
								dra2.MainRowPos[ti] = dri;
							}
						}

						//string txt = ""; // debug
						//foreach (int i0 in rowPos) txt += " " + i0.ToString();
						//ClientLog.Message("FilteredRowPos dri = " + dri + txt);

						if (rowsPassedForKey == 0) PassedFiltersKeyCount++;
						PassedFiltersRowCount++;
						//if (PassedFiltersRowCount != PassedFiltersKeyCount) PassedFiltersRowCount = PassedFiltersRowCount; // debug
						rowsPassedForKey++;

						if (RowIsSelected(dr))
						{
							SelectedRowCount++;

							if (!SelectedKeys.ContainsKey(keyVal))
							{
								SelectedKeys[keyVal] = dri;
								SelectedKeyCount++;
							}
						}

						if (RowIsMarked(dr))
						{
							MarkedRowCount++;

							if (!MarkedKeys.ContainsKey(keyVal))
							{
								MarkedKeys[keyVal] = dri;
								MarkedKeyCount++;
							}
						}

						dri++; // go to next row
					}

					else if (tableCount == 1) // if just one table continue on
					{
						dra.Filtered = true;
						dri++;
					}

					else // didn't pass, mark this & remaining rows for key as filtered
					{
						for (dri = dri; dri < firstRowForNextKey; dri++)
						{
							dr = DataTableMx.Rows[dri];
							dra = GetRowAttributes(dr);
							dra.Filtered = true;
						}
					}

				} // row loop within key value

			} // key value loop

			RowCount = DataTableMx.Rows.Count;

			if (QueryManager.StatusBarManager != null)
				QueryManager.StatusBarManager.DisplayFilterCountsAndString();

			if (Math.Abs(1) == 2) // get debug dump
				txt = FilterStateFormattedForDebug;

			return;
		}

		/// <summary>
		/// Dump out the filter state for debugging purposes
		/// </summary>

		public string FilterStateFormattedForDebug
		{
			get
			{
				string txt = "\r\nFilter state: RowIndex, Filtered, SubRowPos, MainRowPos\r\n";

				for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
				{
					string rec = string.Format("{0,4}", dri);
					DataRowMx dr = DataTableMx.Rows[dri];
					DataRowAttributes dra = GetRowAttributes(dr);
					if (dra != null)
					{
						if (dra.Filtered) rec += " F";
						else rec += "  ";

						if (dra.SubRowPos != null)
						{
							rec += " : ";
							for (int i1 = 0; i1 < dra.SubRowPos.Length; i1++)
								rec += string.Format("{0,4} ", dra.SubRowPos[i1]);
						}

						if (dra.MainRowPos != null)
						{
							rec += " : ";
							for (int i1 = 0; i1 < dra.MainRowPos.Length; i1++)
								rec += string.Format("{0,4} ", dra.MainRowPos[i1]);
						}

					}
					txt += rec + "\r\n";
				}

				return txt;
				//DebugLog.Message(txt);
			}
		}

		/// <summary>
		/// Update selected and marked key lists/counts and row counts
		/// </summary>

		internal void UpdateSelectedAndMarkedCounts()
		{
			MarkedKeyCount = MarkedRowCount = 0;
			MarkedKeys = new Dictionary<string, int>();

			SelectedKeyCount = SelectedRowCount = 0;
			SelectedKeys = new Dictionary<string, int>();

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{
				DataRowMx dr = DataTableMx.Rows[dri];
				string keyVal = dr[KeyValueVoPos] as string;

				if (RowIsSelected(dr))
				{
					SelectedRowCount++;

					if (!SelectedKeys.ContainsKey(keyVal))
					{
						SelectedKeys[keyVal] = dri;
						SelectedKeyCount++;
					}
				}

				if (RowIsMarked(dr))
				{
					MarkedRowCount++;

					if (!MarkedKeys.ContainsKey(keyVal))
					{
						MarkedKeys[keyVal] = dri;
						MarkedKeyCount++;
					}
				}
			}

			return;
		}

		/// <summary>
		/// Setup the list of columns with secondary filters
		/// </summary>

		internal void SetupFilteredColumnList()
		{
			FilteredColumnList = new List<ColumnInfo>();
			ResultsFormat rf = Rf;

			for (int rti = 0; rti < rf.Tables.Count; rti++)
			{
				ResultsTable rt = rf.Tables[rti];
				for (int rfi = 0; rfi < rt.Fields.Count; rfi++)
				{
					ResultsField rfld = rt.Fields[rfi];
					QueryColumn qc = rfld.QueryColumn;
					ColumnInfo cInf = rf.GetColumnInfo(qc);
					if (cInf.Rfld == null || cInf.Qc.SecondaryCriteria == "") continue;
					FilteredColumnList.Add(cInf);
				}
			}

			return;
		}

		/// <summary>
		/// Get a list of the filters associated with each results table
		/// FilteredColumnList must be setup before call
		/// </summary>
		/// <param name="filteredColumnList"></param>
		/// <returns></returns>

		List<ColumnInfo>[] SetupFiltersPerTable()
		{
			int ti;

			if (Rf == null) return new List<ColumnInfo>[0];

			List<ColumnInfo>[] fpt = new List<ColumnInfo>[Rf.Tables.Count];

			foreach (ColumnInfo cInf in FilteredColumnList)
			{
				ti = cInf.TableIndex;

				if (fpt[ti] == null) fpt[ti] = new List<ColumnInfo>();
				fpt[ti].Add(cInf);
			}

			return fpt;
		}

		/// <summary>
		/// Scan upward in the DataTable to find the first root table row for this key
		/// </summary>
		/// <param name="ri"></param>
		/// <returns></returns>

		public int GetRootTableDataRowForKey(
				int ri)
		{
			int kci = KeyValueVoPos; // position for initial key values
			string thisKey = DataTableMx.Rows[ri][kci].ToString();
			int ri2;
			for (ri2 = ri - 1; ri2 >= 0; ri2--)
			{ // scan backwards to find first row for this key
				DataRowAttributes dra = GetRowAttributes(ri2);
				string key = DataTableMx.Rows[ri2][kci].ToString();
				if (key != thisKey) break;
			}

			return ri2 + 1;
		}

		/// <summary>
		/// Scan upward in the DataTable to see if the first root table row for this key 
		/// has been rendered and return the index of this row if not rendered.
		/// </summary>
		/// <param name="ri"></param>
		/// <returns></returns>

		internal int GetUnrenderedDataRow(
				int ri)
		{
			int kci = KeyValueVoPos; // position for initial key values
			string thisKey = DataTableMx.Rows[ri][kci].ToString();
			int ri2;
			for (ri2 = ri - 1; ri2 >= 0; ri2--)
			{
				DataRowAttributes dra = GetRowAttributes(ri);
				if (dra != null && dra.Filtered) continue;
				string key = DataTableMx.Rows[ri][kci].ToString();
				if (key == thisKey) return -1; // found unfiltered row that would have displayed the root row data
				else break;
			}

			ri2 += 1; // all preceeding rows for key were filtered so return index of data-containing row for key
			return ri2 + 1;
		}

		/// <summary>
		/// Move a row up one
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		public static DataRowMx MoveRowUp(
				DataTableMx dt,
				int r)
		{
			DataRowMx dr = dt.Rows[r];
			if (r == 0) return dr;
			dt.Rows.RemoveAt(r);
			dt.Rows.InsertAt(dr, r - 1);
			return dr;
		}

		/// <summary>
		/// Move a row down one
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="r"></param>
		/// <returns></returns>

		public static DataRowMx MoveRowDown(
				DataTableMx dt,
				int r)
		{
			DataRowMx dr = dt.Rows[r];
			if (r == dt.Rows.Count - 1) return dr;
			dt.Rows.RemoveAt(r);
			dt.Rows.InsertAt(dr, r + 1);
			return dr;
		}

		/// <summary>
		/// Return true if row is filtered
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <returns></returns>

		internal bool RowIsFiltered(int rowIndex)
		{
			DataRowAttributes dra = GetRowAttributes(rowIndex);
			if (dra == null) return false;
			return dra.Filtered;
		}

		/// <summary>
		/// Return true if row is marked
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <returns></returns>

		internal bool RowIsSelected(int rowIndex)
		{
			DataRowMx dr = DataTableMx.Rows[rowIndex];
			return RowIsSelected(dr);
		}

		/// <summary>
		/// Return true if row is marked
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>

		internal bool RowIsSelected(DataRowMx dr)
		{
			DataRowAttributes dra = GetRowAttributes(dr);
			if (dra == null) return false;
			return dra.Selected;
		}

		/// <summary>
		/// Select/unselect a row
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <param name="selected"></param>

		internal void SelectRow(int rowIndex, bool selected)
		{
			DataRowMx dr = DataTableMx.Rows[rowIndex];
			SelectRow(dr, selected);
			return;
		}

		/// <summary>
		/// Select/unselect a row
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="selected"></param>

		internal void SelectRow(DataRowMx dr, bool selected)
		{
			DataRowAttributes dra = GetRowAttributes(dr);
			if (dra == null) return;
			dra.Selected = selected;
			return;
		}

		/// <summary>
		/// Set the value of all row selection flags to the specified value
		/// </summary>
		/// <param name="selected"></param>

		internal void SelectAllRows(bool selected)
		{
			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				SelectRow(dr, selected);
			}

			return;
		}

		/// <summary>
		/// Mark/unmark the selected rows
		/// </summary>
		/// <param name="markValue"></param>

		internal void MarkSelected(bool markValue)
		{
			if (CheckMarkVoPos < 0) return;
			int attrPos = RowAttributesVoPos; // position for attributes
			if (attrPos < 0) return;

			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				DataRowAttributes dra = GetRowAttributes(dr);
				if (dra == null) return;
				if (dra.Selected)
					dr[CheckMarkVoPos] = markValue;
			}

			return;
		}

		/// <summary>
		/// Select/unselect the marked rows
		/// </summary>
		/// <param name="selectValue"></param>

		internal void SelectMarked(bool selectValue)
		{
			if (CheckMarkVoPos < 0) return;
			int attrPos = RowAttributesVoPos; // position for attributes
			if (attrPos < 0) return;

			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				DataRowAttributes dra = GetRowAttributes(dr);
				if (dra == null) return;

				object o = dr[CheckMarkVoPos];
				if (o is bool && ((bool)o) == true) // marked?
					dra.Selected = selectValue;
			}

			return;
		}

		/// <summary>
		/// Return true if row is marked
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <returns></returns>

		internal bool RowIsMarked(int rowIndex)
		{
			DataRowMx dr = DataTableMx.Rows[rowIndex];
			return RowIsMarked(dr);
		}

		/// <summary>
		/// Return true if row is marked
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>

		internal bool RowIsMarked(DataRowMx dr)
		{
			if (CheckMarkVoPos < 0) return false;
			object o = dr[CheckMarkVoPos];
			if (o is bool) return (bool)o;
			else return false;
		}

		/// <summary>
		/// Mark/unmark a row
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <param name="markValue"></param>

		internal void SetRowMark(int rowIndex, bool markValue)
		{
			DataRowMx dr = DataTableMx.Rows[rowIndex];
			SetRowMark(dr, markValue);
			return;
		}

		/// <summary>
		/// Mark/unmark a row
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="markValue"></param>

		internal void SetRowMark(DataRowMx dr, bool markValue)
		{
			if (CheckMarkVoPos < 0) return;
			dr[CheckMarkVoPos] = markValue;
			return;
		}

		/// <summary>
		/// Set the value of all row checkmarks to the specified value
		/// </summary>
		/// <param name="markValue"></param>

		internal void MarkAllRows(bool markValue)
		{
			if (CheckMarkVoPos < 0) return;

			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				dr[CheckMarkVoPos] = markValue;
			}

			return;
		}

		/// <summary>
		/// Checks if the DataTable has at least one unmarked row
		/// </summary>
		/// <returns></returns>
		internal bool HasUnmarkedRow()
		{
			bool UnmarkedRow = false;
			foreach (DataRowMx r in DataTableMx.Rows)
			{
				if (!RowIsMarked(r))
				{
					UnmarkedRow = true;
					break;
				}
			}

			return UnmarkedRow;
		}

		/// <summary>
		/// Checks if the DataTable has at least one marked row
		/// </summary>
		/// <returns></returns>
		internal bool HasMarkedRow()
		{
			bool MarkedRow = false;
			foreach (DataRowMx r in DataTableMx.Rows)
			{
				if (RowIsMarked(r))
				{
					MarkedRow = true;
					break;
				}
			}

			return MarkedRow;
		}

		/// <summary>
		/// Convert metatable to a DataTable
		/// </summary>
		/// <returns></returns>

		public static DataTableMx MetaTableToDataTable(
				MetaTable mt)
		{
			DataTableMx dt = new DataTableMx(mt.Name);
			for (int ci = 0; ci < mt.MetaColumns.Count; ci++)
			{
				MetaColumn mc = mt.MetaColumns[ci];
				DataColumn dc = new DataColumn();
				dc.ColumnName = mc.Name;
				dc.Caption = mc.Name;
				dc.DataType = MobiusDataType.MetaColumnTypeToClassType(mc.DataType);
				SetExtendedMobiusDataColumnProperties(dc, mc);
				dt.Columns.Add(dc);
			}

			return dt;
		}

		/// <summary>
		/// Set the extended Mobius properties for a DataColumn
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="mc"></param>

		public static void SetExtendedMobiusDataColumnProperties(
				DataColumn dc,
				MetaColumn mc)
		{
			dc.ExtendedProperties.Add("MetaTableName", mc.MetaTable.Name);
			dc.ExtendedProperties.Add("MetaColumnName", mc.Name);
			dc.ExtendedProperties.Add("MobiusDataType", MobiusDataType.MetaColumnTypeToClassType(mc.DataType));
		}



		/// <summary>
		/// Return true if row is an all null added row
		/// </summary>

		public bool IsNullAddedRow(
				DataRowMx dr)
		{
			object[] vo = dr.ItemArray;
			if (vo.Length == 0 || vo[0] == null || vo[0] is DBNull) return false;

			DataRowAttributes dra = GetRowAttributes(vo);
			if (dra == null || dra.TableRowState == null || dra.TableRowState.Length == 0) return false;

			RowStateEnum trs = dra.TableRowState[0];
			if (trs != RowStateEnum.Added && trs != RowStateEnum.Undefined) return false;

			for (int i1 = 1; i1 < vo.Length; i1++)
			{
				if (vo[i1] is bool || NullValue.IsNull(vo[i1])) continue;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Return true if row key is null or blank
		/// </summary>

		public bool IsNullKey(
				DataRowMx dr)
		{
			object o = dr[KeyValueVoPos];
			if (NullValue.IsNull(o) || o.ToString().Trim() == "") return true;
			else return false;
		}

		/// <summary>
		/// Get row attributes
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>

		public DataRowAttributes GetRowAttributes(DataRowMx dr)
		{
			if (RowAttributesVoPos < 0) return null;
			if (dr == null || RowAttributesVoPos >= dr.Length) return null; // be sure row attributes exist in dr

			DataRowAttributes dra = dr[RowAttributesVoPos] as DataRowAttributes;
			return dra;
		}

		/// <summary>
		/// Get row attributes
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public DataRowAttributes GetRowAttributes(
				object[] vo)
		{
			if (RowAttributesVoPos < 0) return null;

			DataRowAttributes dra = vo[RowAttributesVoPos] as DataRowAttributes;
			return dra;
		}

		/// <summary>
		/// Get row attributes
		/// </summary>
		/// <param name="dri"></param>
		/// <returns></returns>

		public DataRowAttributes GetRowAttributes(
				int dri)
		{
			if (dri < 0 || dri >= DataTableMx.Rows.Count || RowAttributesVoPos < 0) return null;

			DataRowMx dr = DataTableMx.Rows[dri];
			object[] drVo = dr.ItemArrayRef;
			DataRowAttributes dra = drVo[RowAttributesVoPos] as DataRowAttributes;
			return dra;
		}

		/// <summary>
		/// Get key value for row
		/// </summary>
		/// <param name="dri"></param>
		/// <returns></returns>

		public string GetRowKey(int dri)
		{
			DataRowMx dr = DataTableMx.Rows[dri];
			object[] drVo = dr.ItemArrayRef; // use row item array reference to avoid firing change events
			string key = GetRowKey(dr);
			return key;
		}

		/// <summary>
		/// Get key value for row
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>

		public string GetRowKey(DataRowMx dr)
		{
			if (KeyValueVoPos < 0 || dr == null) return null;

			string key = dr[KeyValueVoPos] as string;
			return key;
		}

		/// <summary>
		/// Return true if data exists in the DataRow for the specfied query table 
		/// </summary>
		/// <param name="ti"></param>
		/// <returns></returns>

		public static bool RowHasDataForTable(
				DataRowMx dr,
				int ti)
		{
			if (dr == null || dr.ItemArray == null || dr.ItemArray.Length == 0) return false; // undefined row (e.g. filtered-out)

			DataRowAttributes dra = dr[DefaultRowAttributesVoPos] as DataRowAttributes;
			if (dra == null) return true;
			if (dra.TableRowState == null) return false; // correct?

			//if (ti >= dra.TableRowState.Length) ti = ti; // debug

			switch (dra.TableRowState[ti])
			{
				case RowStateEnum.Added:
				case RowStateEnum.Modified:
				case RowStateEnum.Unchanged:
					return true;

				case RowStateEnum.Undefined:
				case RowStateEnum.Deleted:
					return false;

				default:
					throw new Exception("Unexpected DataRowAttributes.TableState value: " + dra.TableRowState[ti]);
			}
		}

		/// <summary>
		/// Add a new datarow and setup its content
		/// </summary>

		internal DataRowMx AddDataRow(object[] voa)
		{
			DataRowMx dr, dr2;
			DataRowAttributes dra = null, dra2 = null;

			string keyVal, keyVal2;
			int ci, ri;

			int colcnt = DataTableMx.Columns.Count;

			dr = DataTableMx.NewRow();
			object[] dria = dr.ItemArrayRef; // access the item array directly to avoid firing events on each element change

			if (voa.Length + KeyValueVoPos != dria.Length) // be sure vo and dr have matching number of items
			{
				string cols = ""; // build list of selected cols
				for (int i1 = KeyValueVoPos; i1 < DataTableMx.Columns.Count; i1++)
				{
					int voPos = i1;
					QueryColumn qc = Rf.VoIndexToQueryColumn[voPos];
					if (qc == null) continue;

					cols += voPos.ToString() + " " + DataTableMx.Columns[i1].ColumnName + "\r\n";
				}

				string msg = "Buffer length mismatch, voa.Length = " + voa.Length + ", dria.Length = " + dria.Length + "\r\n" + cols;
				if (Query != null) msg += "\r\n*****Query *****\r\n" + Query.Serialize();

				DebugMx.InvalidConditionException(msg);
			}

			// Copy data vrom voa to DataRow item array

			for (ci = 0; ci < voa.Length; ci++)
			{
				int drPos = ci + KeyValueVoPos;

				if (ci == 0) dria[drPos] = voa[ci]; // store common key value

				else if (voa[ci] == null || voa[ci] is DBNull) // store null value
					dria[drPos] = DBNull.Value;

				else // store non-null value in DataRow
				{
					dria[drPos] = voa[ci]; // store as is
																 //dria[drPos] = MobiusDataType.ConvertTo(mcType, voa[ci]); // convert to MDT (obsolete, takes too much space)
				}

			}

			if (RowAttributesVoPos >= 0)
				dra = InitializeRowAttributes(dr);

			if (CheckMarkVoPos >= 0 && Grid != null)
				dria[CheckMarkVoPos] = Grid.CheckMarkInitially;

			if (DataTableMx.Rows.Count == 0 || RowCount < 0) // int counts if first row added
			{
				PassedFiltersRowCount = RowCount = 0;
				PassedFiltersKeyCount = KeyCount = 0;
			}

			DataTableMx.Rows.Add(dr);
			ri = DataTableMx.Rows.Count - 1; // index of new row

			RowCount++;
			PassedFiltersRowCount = RowCount;

			bool newKey = false;
			keyVal = dria[KeyValueVoPos] as string;
			if (DataTableMx.Rows.Count == 1) newKey = true;
			else
			{
				dr2 = DataTableMx.Rows[DataTableMx.Rows.Count - 2];
				dra2 = GetRowAttributes(dr2);
				keyVal2 = dr2[KeyValueVoPos] as string;
				if (keyVal != keyVal2) newKey = true;
			}

			if (newKey)
			{
				PassedFiltersKeyCount++;
				//if (ResultsKeys != null) // if know full key count, use it
				//  KeyCount = ResultsKeys.Count;
				//else KeyCount++; // otherwise increment key count
				KeyCount++; // increment key count
				if (dra != null) // todo: take filtering into account
				{
					dra.FirstRowForKey = ri;
				}
			}

			else // not the first row for the key
			{
				if (dra != null) // todo: take filtering into account
				{
					dra.FirstRowForKey = dra2.FirstRowForKey;
				}
			}

			//if (KeyCount != DataTableMx.Rows.Count) KeyCount = KeyCount; // debug

			UpdateMaxRowsPerKeyForAddedRow(ri, dr, dra, dria, keyVal, newKey);

			UpdateUnfilteredSubrowPos(ri);

			return dr;
		}

		/// <summary>
		/// Initialize the RowState information for each table in the query indicating if data is available of not
		/// </summary>
		/// <param name="dr"></param>

		internal void InitializeTableRowState(DataRowMx dr)
		{
			ResultsTable rt;
			QueryTable qt;
			QueryColumn qc;
			int ti, voi;

			ResultsFormat rf = Rf; // for faster access

			DataRowAttributes dra = GetRowAttributes(dr);
			if (dra == null) return;

			RowStateEnum[] ts = dra.TableRowState;
			if (ts == null || ts.Length != rf.Tables.Count)
			{
				ts = new RowStateEnum[rf.Tables.Count];
				dra.TableRowState = ts;
			}

			for (ti = 0; ti < rf.Tables.Count; ti++)
			{
				rt = rf.Tables[ti];
				qt = rt.QueryTable;
				qc = qt.KeyQueryColumn;

				ValidateVoPositition(qc, dr); // check for valid position for key column

				voi = qc.VoPosition;

				object keyVal = dr[voi];
				if (keyVal != null && !(keyVal is DBNull))
					ts[ti] = RowStateEnum.Unchanged;
				else ts[ti] = RowStateEnum.Undefined;
			}

			return;
		}

		/// <summary>
		/// ValidateVoPositition
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="dr"></param>

		void ValidateVoPositition(
			QueryColumn qc,
			DataRowMx dr)
		{
			int voi = qc.VoPosition;
			if (voi >= 0 && voi < dr.Length) return;

			string msg = "VoPosition = " + voi + " out of range for Vo of size = " + dr.Length +
					", DtRows: " + DataTableMx.Rows.Count +
					", RfPqe: " + ((Rf == null || Rf.ParentQe == null) ? "null" : "not null") +
					", Column: " + qc.QueryTable.MetaTable.Name + "." + qc.QueryTable.Alias + "." + qc.MetaColumn.Name;

			if (Query != null) msg += "\r\n*****Query *****\r\n" + Query.Serialize();

			if (TriedVoPosFix) throw new Exception(msg);

			msg += "\r\nReassigning VoPositions\r\n" +
				new StackTrace(true);

			DebugLog.Message(msg);

			Query.SetSimpleVoPositions(DataValuesVoOffset); // see if we can fix by simple reassignment

			TriedVoPosFix = true;

			return;
		}

		/// <summary>
		/// Update UpdateMaxRowsPerKey using the rows with the same key value as the added row 
		/// </summary>
		/// <param name="dri"></param>
		/// <param name="dr"></param>
		/// <param name="dra"></param>
		/// <param name="dria"></param>
		/// <param name="keyVal"></param>
		/// <param name="newKey"></param>

		internal void UpdateMaxRowsPerKeyForAddedRow(
			int dri,
			DataRowMx dr,
			DataRowAttributes dra,
			object[] dria,
			string keyVal,
			bool newKey)
		{
			int firstRowToCheck = -1, lastRowToCheck = -1;
			if (!UpdateMaxRowsPerKeyEnabled) return;

			if (keyVal == null && !NullValue.IsNull(dr[KeyValueVoPos + 1]))
				keyVal = dr[KeyValueVoPos + 1].ToString(); // if initial key is null try to get from next column

			if (MaxRowsPerKey == null || MaxRowsPerKey.Length != Rf.Tables.Count) // be sure allocated
				MaxRowsPerKey = new int[Rf.Tables.Count];

			if (RowsForKeyForLastRowAdded == null || RowsForKeyForLastRowAdded.Length != Rf.Tables.Count) // be sure allocated
				RowsForKeyForLastRowAdded = new int[Rf.Tables.Count];

			if (!newKey) // if same key as last row added, just check this row (much faster if many rows per key)
			{
				for (int ti = 0; ti < Rf.Tables.Count; ti++)
				{ // increment row count for each table with data
					if (dra.TableRowState[ti] != RowStateEnum.Undefined) // count if not an undefined row
					{
						RowsForKeyForLastRowAdded[ti]++;
						if (RowsForKeyForLastRowAdded[ti] > MaxRowsPerKey[ti])
							MaxRowsPerKey[ti] = RowsForKeyForLastRowAdded[ti];
					}
				}
			}

			else // check full range of rows for key (slower)
			{
				Array.Clear(RowsForKeyForLastRowAdded, 0, RowsForKeyForLastRowAdded.Length);

				for (int dri2 = dra.FirstRowForKey; dri2 < DataTableMx.Rows.Count; dri2++)
				{ // scan from first row for key through last
					DataRowMx dr2 = DataTableMx.Rows[dri2];
					DataRowAttributes dra2 = GetRowAttributes(dr2);
					if (dra2 == null) throw new Exception("DataRowAttributes not defined");

					string keyVal2 = dr2[KeyValueVoPos] as string;
					if (keyVal2 == null && !NullValue.IsNull(dr2[KeyValueVoPos + 1]))
						keyVal2 = dr2[KeyValueVoPos + 1].ToString(); // if initial key is null try to get from next column

					if (Lex.Ne(keyVal2, keyVal2)) break;

					for (int ti = 0; ti < Rf.Tables.Count; ti++)
					{ // increment row count for each table with data
						if (dra2.TableRowState[ti] != RowStateEnum.Undefined) // count if not an undefined row
						{
							RowsForKeyForLastRowAdded[ti]++;
							if (RowsForKeyForLastRowAdded[ti] > MaxRowsPerKey[ti])
								MaxRowsPerKey[ti] = RowsForKeyForLastRowAdded[ti];
						}
					}
				}
			}

			IndexOfLastRowAdded = dri;
			KeyForLastRowAdded = keyVal;

			return;
		}

		/// <summary>
		/// Update subrow position for a filtered or unfiltered dataset
		/// </summary>

		internal void UpdateSubRowPos()
		{
			if (FiltersEnabled && FilteredColumnList != null && FilteredColumnList.Count > 0)
				UpdateFilterState(); // if filtering then update the filtering info

			else UpdateUnFilteredSubRowPos();
		}

		/// <summary>
		/// Update the subrow positions for each table for the specified DataTable when filters are off
		/// </summary>
		/// <param name="dataRowIndex"></param>

		internal void UpdateUnFilteredSubRowPos()
		{
			InitializeSubrowPositions();

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{
				UpdateUnfilteredSubrowPos(dri);
			}
		}

		/// <summary>
		/// Initialize the filtered subrow and main row positions
		/// </summary>

		internal void InitializeSubrowPositions()
		{
			int i1, dri;
			DataRowAttributes dra;

			ResultsFormat rf = Rf;
			if (rf == null) rf = new ResultsFormat();
			int tableCount = rf.Tables.Count;

			int[] rowPosInit = new int[tableCount];
			for (i1 = 0; i1 < tableCount; i1++) rowPosInit[i1] = -1;

			for (dri = 0; dri < DataTableMx.Rows.Count; dri++) // init subrow mapping
			{
				dra = GetRowAttributes(dri);
				dra.Filtered = true;

				if (dra.SubRowPos == null || dra.SubRowPos.Length != tableCount)
					dra.SubRowPos = new int[tableCount];
				rowPosInit.CopyTo(dra.SubRowPos, 0);

				if (dra.MainRowPos == null || dra.MainRowPos.Length != tableCount)
					dra.MainRowPos = new int[tableCount];
				rowPosInit.CopyTo(dra.MainRowPos, 0);
			}

			return;
		}

		/// <summary>
		/// Update the subrow positions for each table for the specified DataRow when filters are off
		/// </summary>
		/// <param name="dataRowIndex"></param>

		internal void UpdateUnfilteredSubrowPos(int dri)
		{
			int ti;

			DataRowAttributes dra = GetRowAttributes(dri);

			int tableCount = Rf.Tables.Count;

			if (dra.SubRowPos == null || dra.SubRowPos.Length != tableCount)
				dra.SubRowPos = new int[tableCount];

			if (dra.MainRowPos == null || dra.MainRowPos.Length != tableCount)
				dra.MainRowPos = new int[tableCount];

			if (Rf.DuplicateKeyTableValues) // duplicating key fields
			{
				//if (dri >= 360) dri = dri; // debug
				
				for (ti = 0; ti < Rf.Tables.Count; ti++)
				{
					if (MaxRowsPerKey[ti] <= 1) // if max 0 or 1 rows for this table then map to first row
						dra.SubRowPos[ti] = dra.FirstRowForKey; // map all subrows to this row

					else // otherwise map to self
						dra.SubRowPos[ti] = dri; 

					//dra.MainRowPos[ti] = dri; // testing: always map to self 
				}
			}

			else // not duplicating, use default mapping
			{
				for (ti = 0; ti < Rf.Tables.Count; ti++)
				{
					dra.SubRowPos[ti] = dri; // map all subtables to this row
					dra.MainRowPos[ti] = dri;
				}
			}

			return;
		}

		/// <summary>
		/// Adjust the data row to render based on the current data row and the specified table's SubRowPos
		/// </summary>
		/// <param name="dri"></param>
		/// <param name="ti"></param>
		/// <returns>Index of the row to render for the specified table</returns>

		public int AdjustDataRowToCurrentDataForTable(
				int dri,
				int ti,
				bool duplicateCommonData)
		{
			int dri2 = -1;

			if (RowAttributesVoPos < 0 || // must have necessary meta info
			 ti < 0) // and a valid table index
				return dri;

			DataRowAttributes dra = GetRowAttributes(dri);
			if (dra == null || dra.SubRowPos == null) return dri;

			dri2 = dra.SubRowPos[ti];

			if (duplicateCommonData) // return a pointer to the common root data
			{
				QueryTable qt = Query.Tables[ti];
				MetaTable mt = qt.MetaTable;
				if (mt.Parent == null)
					dri2 = dra.FirstRowForKey;
			}

			//if (dri2 != dri) dri = dri; // debug
			return dri2;
		}

		/// <summary>
		/// Sort the data table
		/// </summary>
		/// <param name="sCols"></param>

		internal void Sort(List<SortColumn> sCols)
		{
			List<string> resultKeys;
			object[] vo;
			int ri;

			List<object[]> rows = new List<object[]>();
			for (ri = 0; ri < DataTableMx.Rows.Count; ri++)
			{
				DataRowMx dr = DataTableMx.Rows[ri];
				rows.Add(dr.ItemArray);
			}

			ResultsSorter rs = new ResultsSorter();
			rows = rs.Sort(rows, sCols, QueryManager.Query, KeyValueVoPos, out resultKeys);

			for (ri = 0; ri < DataTableMx.Rows.Count; ri++)
			{
				vo = rows[ri];
				DataRowMx dr = DataTableMx.Rows[ri];
				for (int ci = 0; ci < vo.Length; ci++) // convert any nulls to DBNull.Values
					if (vo[ci] == null) vo[ci] = DBNull.Value;
				dr.ItemArray = vo;
			}

			InitializeRowAttributes(true, false); // reset all of the row attributes except selected (keep for details-on-demand view)
			if (FiltersEnabled && FilteredColumnList != null && FilteredColumnList.Count > 0)
				UpdateFilterState(); // if filtering then update the filtering info
			return;
		}

		/// <summary>
		/// Reset the formatted values for a QueryColumn
		/// </summary>
		/// <param name="?"></param>
		/// 
		internal void ResetFormattedValues(QueryColumn qc)
		{
			ResetFormattedValues(qc, true, true, false);
			return;
		}

		/// <summary>
		/// Reset all formatted values
		/// </summary>

		internal void ResetFormattedValues()
		{
			ResetFormattedValues(true, true, false);
		}

		/// <summary>
		/// Reset the formatted values for a QueryColumn
		/// </summary>
		/// <param name="?"></param>

		internal void ResetFormattedValues(
				QueryColumn qc,
				bool resetText,
				bool resetBitmap,
				bool resetBackgroundColor)
		{
			//DebugLog.Message("ResetFormattedValues " + qc.MetaColumn.Name);

			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				if (dr[qc.VoPosition] is MobiusDataType)
				{
					MobiusDataType mdt = dr[qc.VoPosition] as MobiusDataType;
					if (resetText)
						mdt.FormattedText = null;

					if (resetBitmap)
						mdt.FormattedBitmap = null;

					if (resetBackgroundColor && mdt.BackColor != Color.Empty)
						mdt.BackColor = Color.Empty;
				}
			}
		}

		/// <summary>
		/// Reset formatted values for all columns
		/// </summary>

		internal void ResetFormattedValues(
				bool resetText,
				bool resetBitmap,
				bool resetBackgroundColor)
		{
			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				foreach (object o in dr.ItemArray)
					if (o is MobiusDataType)
					{
						MobiusDataType mdt = o as MobiusDataType;
						if (resetText)
							mdt.FormattedText = null;
						if (resetBitmap)
							mdt.FormattedBitmap = null;

						if (resetBackgroundColor && mdt.BackColor != Color.Empty)
							mdt.BackColor = Color.Empty;
					}
			}
		}

		/// <summary>
		/// Reset all formatted bitmaps
		/// </summary>

		internal void ResetFormattedBitmaps()
		{
			int resetCnt = 0;

			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				foreach (object o in dr.ItemArray)
				{
					MobiusDataType mdt = o as MobiusDataType;
					if (mdt == null || !mdt.IsGraphical) continue;

					mdt.FormattedBitmap = null;
					mdt.FormattedText = null;
					resetCnt++;
				}
			}

			return;
		}

		/// <summary>
		/// Delete a row from the DataTable
		/// </summary>
		/// <param name="rowIndex"></param>

		public void DeleteRow(int rowIndex)
		{
			DataRowMx dr = DataTableMx.Rows[rowIndex];
			DataRowAttributes dra = GetRowAttributes(dr);

			if (dra.TableRowState[0] != RowStateEnum.Added)
			{
				dra.TableRowState[0] = RowStateEnum.Deleted;
				DataRowMx dr2 = DataTableMx.NewRow(); // must make a copy of the row to delete
				dr2.ItemArray = dr.ItemArray;
				DeletedRows.Add(dr2);
			}

			DataTableMx.Rows.Remove(dr);
			return;
		}

		/// <summary>
		/// Check that the DataTable associated with the query is valid
		/// </summary>
		/// <param name="q"></param>

		public static void ValidateExistingDataTable(Query q)
		{
			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null) throw new Exception("QueryManager not defined");
			DataTableMx dt = qm.DataTable;

			if (dt != null && dt.Columns.Count > 0) return;

			dt = q.ResultsDataTable as DataTableMx; // try ResultsDataTable as second choice (i.e. saved query with results embedded in query
			if (dt != null && dt.Columns.Count > 0)
			{
				qm.DataTable = dt; // plug into QueryManager for subsequent use
				return;
			}

			throw new Exception("DataTable not defined");
		}

		/// <summary>
		/// Get the value for a column field in the specified row 
		/// optionally converting the value to a primitive
		/// </summary>
		/// <param name="rfld"></param>
		/// <param name="dri"></param>
		/// <param name="getPrimitiveValue"></param>
		/// <returns></returns>

		public object GetColumnValueWithRowDuplication(
		ResultsField rfld,
		int dri,
		bool getPrimitiveValue = false)
		{
			object kVo, vo, value = null;
			MobiusDataType mdt;
			DateTimeMx dex;
			int rti, dri2, iVal;
			string cid;
			double dVal;

			if (UseUnpivotedAssayResultsCache)
				return GetPrimitiveValueFromTarCache(dri, rfld);

			ResultsTable rt = rfld.ResultsTable; // results table
			int ti = rt.Position;

			DataRowMx dr = DataTableMx.Rows[dri];
			DataRowAttributes dra = GetRowAttributes(dr);
			if (dra == null) return null;

			MetaColumn mc = rfld.MetaColumn;
			if (dra.SubRowPos != null) // use subrow if defined
			{

				dri2 = dra.SubRowPos[rt.Position];
				if (dri2 == DataTableManager.NullRowIndex) return null;
				if (dri2 != dri) dr = DataTableMx.Rows[dri2];
			}

			int keyFieldPos = rt.Fields[0].VoPosition; // key field position in vo
			kVo = dr[keyFieldPos]; // get key value for row
			if (NullValue.IsNull(kVo)) // if key value is null see if this is a column that should be duplicated across the row for each key
			{
				if (MaxRowsPerKey[ti] == 1) // must have previously seen a max of one row per key value
					dr = DataTableMx.Rows[dra.FirstRowForKey];
			}

			vo = dr[rfld.VoPosition];
			if (getPrimitiveValue) return MobiusDataType.ConvertToPrimitiveValue(vo, mc);
			else return vo;
		}

		/// <summary>
		/// AddRetrievingMessageDataRow
		/// </summary>

		public void AddRetrievingDataMessageRow()
		{
			DataRowMx dr;

			if (HasRetrievingDataMessageRow()) return;

			dr = DataTableMx.NewRow();
			if (RowAttributesVoPos >= 0)
			{
				DataRowAttributes ra = new DataRowAttributes();
				dr[RowAttributesVoPos] = ra;
			}

			dr.ItemArrayRef[KeyValueVoPos] = "Retrieving data...";

			DataTableMx.Rows.Add(dr);

			return;
		}

		/// <summary>
		/// RemoveRetrievingDataMessageRow
		/// </summary>

		public bool RemoveRetrievingDataMessageRow()
		{
			if (!HasRetrievingDataMessageRow()) return false;

			int rowCnt = DataTableMx.Rows.Count;
			DataTableMx.Rows.RemoveAt(rowCnt - 1);
			return true;
		}

		/// <summary>
		/// Return true if the data table contains the "Retrieving data..." row
		/// </summary>
		/// <returns></returns>

		public bool HasRetrievingDataMessageRow()
		{
			if (DataTableMx?.Rows == null) return false;

			int rowCnt = DataTableMx.Rows.Count;
			if (rowCnt <= 0) return false;

			DataRowMx dr = DataTableMx.Rows[rowCnt - 1];
			return IsRetrievingDataMessageRow(dr);
		}

		/// <summary>
		/// Return true if the data table contains a row image that is Retrieving data" row
		/// </summary>
		/// <returns></returns>

		public bool HasRetrievingImageRow()
		{
			foreach (DataRowMx dr in DataTableMx.Rows)
			{
				if (IsRetrievingImageRow(dr)) return true;
			}
			return false;
		}

		public bool IsRetrievingImageRow(DataRowMx dr)
		{
			foreach (Object obj in dr.ItemArray)
			{
				if (obj is ImageMx)
				{
					if ((obj as ImageMx).IsRetrievingValue) return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Return true if the specified row is the "Retrieving data..." row
		/// </summary>

		public bool IsRetrievingDataMessageRow(DataRowMx dr)
		{
			object[] ia = dr?.ItemArrayRef;
			if (ia == null) return false;

			if (ia.Length < KeyValueVoPos || KeyValueVoPos < 0)
				return false;

			string key = dr.ItemArray[KeyValueVoPos] as string;
			if (key != null && String.Equals(key, "Retrieving data...", StringComparison.OrdinalIgnoreCase))
				return true;

			else return false;
		}

		/// <summary>
		/// Merge the data (and queries) for two data table managers
		/// </summary>
		/// <param name="dtm2"></param>
		/// <returns></returns>

		public DataTableManager Merge(
				DataTableManager dtm2)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Free resources linked to this instance
		/// </summary>

		public void Dispose()
		{
			try
			{
				QueryManager = null;
				if (DataTableMx != null) DataTableMx.RowChanged -= Dt_RowChanged; // remove event reference
				return;
			}

			catch (Exception ex)
			{
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
			}
		}

	} // DataTableManager

	/// <summary>
	/// Attribute information for a DataRow, usually stored in first element of row
	/// </summary>

	public class DataRowAttributes
	{
		public string Key; // normalized key value
		public string OriginalKey; // key value before update
		public RowStateEnum[] TableRowState; // update state for each ResultsTable in tuple
		public int[] SubRowPos; // position of each ResultsTable subrow within the DataTable for this row
		public int[] MainRowPos; // main row that the subrow for this table is associated with, -1 if none
		public bool Filtered = false; // true if filtered out
		public bool Selected = false; // true if selected
		public int FirstRowForKey = -1; // index of the first row that contains the current key value
	}

	/// <summary>
	/// Key and row counts for current data set
	/// </summary>

	public class KeyAndRowCounts
	{
		public int KeyCount;
		public int VisibleKeyCount;
		public int RowCount;
		public int VisibleRowCount;
	}

	/// <summary>
	/// Rowstate for each QueryTable for a FlatRow
	/// </summary>

	public enum RowStateEnum
	{
		Undefined = 0,
		Added = 1,
		Unchanged = 2,
		Modified = 3,
		Deleted = 4
	}

	/// <summary>
	/// State for row retrieval from the source into the Dataset
	/// </summary>

	public enum RowRetrievalState
	{
		UnInitialized = -1,
		Undefined = 0,
		Running = 1,
		Paused = 2,
		Cancelled = 3,
		Complete = 4
	}

}
