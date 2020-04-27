using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;


namespace Mobius.Data
{

	/// <summary>
	/// Base Mobius Query object containing:
	/// 1. List of QueryTables containing
	///    1a. Column selections
	///    1b. Column criteria
	///    1c. Sorting instructions
	///    1d. Conditional and other custom column formatting information
	/// 2. Misc query attributes
	/// 3. Subqueries
	/// 4. ResultsPages and ResultsViews for the query results
	/// </summary>

	public partial class Query
	{
		public int InstanceId = InstanceIdCount++; // id of this instance for this session
		internal static int InstanceIdCount = 0; // used to sequentially assign Ids for instances for this session

		public UserObject UserObject = new UserObject(UserObjectType.Query); // name, persisted Id and other persisted properties of query
		public string Name { get => UserObject.Name; set => UserObject.Name = value; } // alias for UserObject.Name
		public int UoId { get => UserObject.Id; set => UserObject.Id = value; } // alias for Userobject.Id
		
		public QueryMode Mode = QueryMode.Unknown; // current mode
		public QueryMode OldMode;
		public List<QueryTable> Tables = new List<QueryTable>(); // list of tables in query
		public QueryTable CurrentTable; // current query table in builder
		public string KeyCriteriaDisplay = ""; // display form of criteria on key
		public string KeyCriteria = ""; // sql form of criteria on key
		public HashSet<string> KeysToExclude = null; // keys to be excluded from search results
		public QueryLogicType LogicType = QueryLogicType.And; // QueryBuilder type of logic for query, default is And
		public string ComplexCriteria = ""; // full Oracle-style criteria expression 
		public int SmallSsqMaxAtoms = -1; // if >=0 then upper atom count limit for imprecise structs
		public bool MinimizeDbLinkUse = true; // if true use database links only when necessary
		public bool FilterResults = true; // if true filter results by search criteria 
		public bool FilterNullRows = true; // if true filter null rows keeping at least 1 row per hit
		public bool FilterNullRowsStrong = false; // if true filter null rows possibly removing hits from results
		public bool Multitable = false; // allow query to be treated as a single table within other queries
		public int KeySortOrder = -1; // default initial key sort direction is descending
		public bool GroupSalts = false; // group results by common salts (if feature enabled)
		public bool AllowColumnMerging = false; // true; // allow query columns to be merged
		public bool SingleStepExecution = false; // if true then combine search and retrieval into a single step
		public bool Preview = false; // if true then just preview a single table
		public bool AllowNetezzaUse = true; // if true allow query to use

		public bool RunQueryWhenOpened = false; // if true automatically run query when opened
		public bool BrowseSavedResultsUponOpen = false; // if true go to browse of existing results (created by alert & background query) when opened
		public int InitialBrowsePage = 0; // initial page to display when entering browse mode 

		public bool SerializeMetaTablesWithQuery = false; // if true serialize MetaTables at beginning of query
		public bool SerializeResultsWithQuery = false; // if true serialize results with query when saving

		public QnfEnum StatDisplayFormat = // content & format for stat display
			QnfEnum.StdDev | QnfEnum.NValue | QnfEnum.NValueTested | QnfEnum.DisplayNLabel;
		public bool RepeatReport = true; // repeat report multiple times across grid if possible
		public bool DuplicateKeyValues = false; // duplicate data that occurs only once per compound
		public bool ShowCondFormatLabels = true; // if true show labels for cond formatting
		public bool ShowGridCheckBoxes = true; // if output to grid show checkboxes
		public bool PrefetchImages = true; // fetch images rather than just the pointers to the images
		public bool ShowStereoComments = true; // if true show any stereochemistry comments with structure (currently overridden by global var)

		public int ViewScale = 100; // view zooming of the grid as a percentage
		public int PrintScale = 100; // print scale of the grid as a percentage, if negative then number of pages wide
		public int PrintMargins = -1; // Print margins in milliinches
		public Orientation PrintOrientation = Orientation.Vertical;

		public string SpotfireDataFileName = ""; // base name of file used when exporting results from this query for Spotfire use
                                             // should be cleared each time the query is run to flag that new files need to be written						

		public int AlertInterval = -1; // number of days between each check for new data (not used in post version 2.0 queries)
		public string AlertQueryState = ""; // subset of query used to determine if alert needs resetting (not saved)

		public ResultsPages ResultsPages = new ResultsPages(); // list of results pages common to the root query and all associated subqueries

		public List<Query> Subqueries = new List<Query>(); // list of transformed views of this query
		public Query Parent; // pointer to parent query if this is a subquery
		public string Transform = ""; // transform name & parameters, can be used with queries with tables or no tables for non-table-based queries

		public int Timeout = 0; // timeout for query in seconds

		public bool DuplicateNamesAndLabelsMarked = false; // true if DuplicateNamesAndLabels called for this Query object

		public IQueryManager QueryManager; // manages set of objects associated with the query at run time

		public Query PresearchBaseQuery; // if this query was created via a presearch transformation then PresearchBaseQuery is the original query
		public Query PresearchDerivedQuery; // any derived query created via a presearch transformation of this query

		public bool UseResultKeys = false; // use any existing ResultKeys, gets reset to false after each ExecuteQuery call
		public string ResultKeysListName = ""; // name of result keys list
		public List<string> ResultKeys = null; // if this keylist is set use it instead of other criteria
		public IDataTableMx ResultsDataTable = null; // DataTable of results
		public string ResultsDataTableFileName = ""; // name of file containing results (usually only set for queries created by alerts)

		public string WarningMessage = ""; // any warning messages associated with this query to display to user
		public Dictionary<string, List<string>> InaccessableData = null; // Schemas and MetaTables that were removed during query processing because of inaccessible data sources
		public bool Mobile = false;
		public bool InResetViewState = false; // resetting the view state
		public int IndexedTableCount = -1; // the number of tables then last time table indexes were assignedw

		// Static members

		public static Version SerializerVersion = new Version(4, 0); // version of the query serializer
		public static Version DeserializerVersion = new Version(0, 0); // latest deserializer seen
		public static HashSet<string> SerializedMetaTableNames = new HashSet<string>();
		public static bool DeserializeTruncateToSingleResultsPage = false;  // if true only include the first resultspage when deserializing
		public static bool SerializeMetaTables = false; // if true serialize MetaTables at beginning of query
		public static IDataTableManager IDataTableManager; // access to DataTableManager methods for serialization
		public static int // Counts for GetQueryTableIndex hit type
			GqtiByIdxCnt = 0, GqtiByAliasIdxCnt = 0, GqtiByAliasScanCnt = 0, GqtiByMtNameCnt = 0, GqtiNotFoundCnt = 0;


		/// <summary>
		/// Basic constructor
		/// </summary>

		public Query()
		{
			ResultsPages = ResultsPages.NewResultsPages(this);
			return;
		}

		public Query(IQueryManager queryManager)
		{
			queryManager.Query = this;
			QueryManager = queryManager;

			ResultsPages = ResultsPages.NewResultsPages(this);
		}

		/// <summary>
		/// Get reference to query from a QueryId
		/// </summary>
		/// <param name="q"></param>
		/// <param name="qid"></param>
		/// <returns></returns>

		public Query GetQueryRefFromId(
			int qid)
		{
			Query q2 = Root.GetQueryRefFromIdRecursive(qid);
			if (q2 == null)
				return null; // DebugLog.Message("Unable to find QueryId within query: " + qid); - Message occurs regularly but shouldn't

			return q2;
		}

		Query GetQueryRefFromIdRecursive( // recursively scan the query tree
			int qid)
		{
			Query q2;
			if (InstanceId == qid) return this;
			else if (Subqueries != null)
			{
				foreach (Query sq in Subqueries)
				{
					q2 = sq.GetQueryRefFromIdRecursive(qid);
					if (q2 != null) return q2;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the root query of the query tree
		/// </summary>

		public Query Root
		{
			get
			{
				Query q = this;
				while (q.Parent != null)
					q = q.Parent;

				if (q.PresearchBaseQuery != null) // get presearch base
					return q.PresearchBaseQuery;
				else return q;
			}
		}

/// <summary>
/// Merge the tables from another query into this one
/// </summary>
/// <param name="q2"></param>

		public void MergeQuery(Query q2)
		{
			Query q = this;
			QueryTable qt;

			if (Lex.IsUndefined(q.KeyCriteria))
			{
				q.KeyCriteria = q2.KeyCriteria;
				q.KeyCriteriaDisplay = q2.KeyCriteriaDisplay;
			}

			foreach (QueryTable qt2 in q2.Tables)
			{
				qt = q.GetQueryTableByName(qt2.MetaTable.Name);
				if (qt == null) q.AddQueryTable(qt2.Clone());
			}

			return;
		}

		/// <summary>
		/// Add a QueryTable to the query not allowing duplicates of a table
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public void AddQueryTableUnique(
			QueryTable qt)
		{
			int ti = GetQueryTableIndex(qt);
			if (ti >= 0)
			{
				QueryTable oldQt = Tables[ti];

				Tables[ti] = qt; // replace if match
				qt.Query = this;

				if (oldQt != null) // unlink old table;
				{
					oldQt.Query = null;
					oldQt.TableIndex = -1;
				}

			}

			else AddQueryTable(qt);

			return;
		}

		/// <summary>
		/// Add a QueryTable to the query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public void AddQueryTable(
			QueryTable qt)
		{
			Tables.Add(qt);
			qt.Query = this;

			SetTableIndexes();
			return;
		}


		/// <summary>
		/// Insert a QueryTable into the query
		/// </summary>
		/// <param name="index"></param>
		/// <param name="qt"></param>

		public void InsertQueryTable(
			int index,
			QueryTable qt)
		{
			Tables.Insert(index, qt);

			qt.Query = this;

			SetTableIndexes();
			return;
		}

		/// <summary>
		/// Remove QueryTable from collection
		/// </summary>
		/// <param name="qt"></param>

		public void RemoveQueryTable(
			QueryTable qt)
		{
			Tables.Remove(qt);

			SetTableIndexes();
			return;
		}

		/// <summary>
		/// Remove QueryTable at specified position
		/// </summary>
		/// <param name="index"></param>

		public void RemoveQueryTableAt(
			int index)
		{
			Tables.RemoveAt(index);

			SetTableIndexes();
			return;
		}

		/// <summary>
		/// Get key column label
		/// </summary>

		public string KeyColumnLabel
		{
			get
			{
				if (Tables.Count == 0) return "";
				else return Tables[0].KeyQueryColumn.ActiveLabel;
			}
		}

		/// <summary>
		/// Get index of specified QueryTable matching on QueryTable reference or alias name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetQueryTableIndex(
			QueryTable qt)
		{
			int ti;

			AssertMx.IsNotNull(qt, "qt");

			if (qt.Query != this) qt = qt; // note case where the qt does not point to this Query object

			if (Tables.Count != IndexedTableCount)
				SetTableIndexes(); // set/reset the indexes

			int qti = qt._tableIndex;

			if (qti >= 0 && qti < Tables.Count) // possible direct match?
			{
				if (Tables[qti] == qt) // exact object match
				{
					GqtiByIdxCnt++;
					return qti;
				}

				else if (qt.Alias != null && qt.Alias != "" && Lex.Eq(Tables[qti].Alias, qt.Alias)) // alias match?
				{
					GqtiByAliasIdxCnt++;
					return qti;
				}
			}

			//for (ti = 0; ti < Tables.Count; ti++) // do a full scan (necessary?)
			//{
			//	if (Tables[ti] == qt)
			//	{
			//		GqtiByQtScanCnt++;
			//		qt._tableIndex = ti;
			//		return ti;
			//	}
			//}

			for (ti = 0; ti < Tables.Count; ti++) // check for same alias
			{ 
				if (qt.Alias != null && qt.Alias != "" && Lex.Eq(Tables[ti].Alias, qt.Alias))
				{
					GqtiByAliasScanCnt++;
					qt._tableIndex = ti;
					return ti;
				}
			}

			for (ti = 0; ti < Tables.Count; ti++) // finally, check for metatable name (ok only if mt is allowed only once per query)
			{
				if (Lex.Eq(qt.MetaTable.Name, Tables[ti].MetaTable.Name))
				{
					GqtiByMtNameCnt++;
					qt._tableIndex = ti;
					return ti;
				}
			}

			GqtiNotFoundCnt++;
			return -1;
		}

		/// <summary>
		/// Get index of specified QueryTable with specified alias
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>

		public QueryTable GetQueryTableByAlias(
			string alias)
		{
			int i1 = GetQueryTableIndexByAlias(alias);
			if (i1 >= 0) return Tables[i1];
			else return null;
		}

		/// <summary>
		/// Get index of specified QueryTable with specified alias
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetQueryTableIndexByAlias(
			string alias)
		{
			for (int i1 = 0; i1 < Tables.Count; i1++)
			{
				if (Lex.Eq(Tables[i1].Alias, alias)) return i1;
			}
			return -1;
		}

		/// <summary>
		/// Lookup a query table by the underlying metatable names
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public QueryTable GetQueryTableByNameWithException(string metaTableName)
		{
			QueryTable qt = GetQueryTableByName(metaTableName);
			if (qt != null) return qt;
			else throw new Exception("Unable to find QueryTable: " + metaTableName);
		}

		/// <summary>
		/// Lookup a query table by the underlying metatable names
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public QueryTable GetQueryTableByName(string metaTableName)
		{
			int qti = GetQueryTableIndexByName(metaTableName);
			if (qti >= 0) return this.Tables[qti];
			else return null;
		}


		/// <summary>
		/// Lookup a query table index by the underlying metatable names
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetQueryTableIndexByName(string metaTableName)
		{
			QueryTable qt;
			MetaTable mt;

			for (int qti = 0; qti < this.Tables.Count; qti++)
			{
				qt = this.Tables[qti];
				mt = qt.MetaTable;
				if (String.Compare(mt.Name, metaTableName, true) == 0)
					return qti;
			}

			return -1;
		}

		/// <summary>
		/// Get first key column in query
		/// </summary>
		/// <returns></returns>

		public QueryColumn GetFirstKeyColumn()
		{
			if (Tables.Count > 0) return Tables[0].KeyQueryColumn;
			else return null;
		}

		/// <summary>
		/// Return first structure column for query 
		/// </summary>
		/// <returns></returns>

		public QueryColumn GetFirstStructureColumn()
		{
			foreach (QueryTable qt in this.Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					MetaColumn mc = qc.MetaColumn;
					if (mc.DataType == MetaColumnType.Structure)
						return qc;
				}
			}

			return null;
		}

		/// <summary>
		/// Return first structure column with criteria for query.
		/// Note that the criteria may be complex in which case 
		/// the criteria for the column will simply say "Criteria".
		/// </summary>
		/// <returns></returns>

		public QueryColumn GetFirstStructureCriteriaColumn()
		{
			foreach (QueryTable qt in this.Tables)
			{
				if (qt.MetaTable.IgnoreCriteria) continue;

				QueryColumn qc = qt.FirstStructureQueryColumn;
				if (qc != null) return qc;
			}

			return null;
		}

		/// <summary>
		/// Find the first QueryColumn in the query that references the specified metacolumn by name
		/// Note: This should be in the Query class but there is a "Could not find method" in the CalcFieldColumnControl.cs Design view
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public QueryColumn GetFirstMatchingQueryColumnByMetaColumn(
			MetaColumn mc)
		{
			string mtName = mc.MetaTable.Name;
			string mcName = mc.Name;
			foreach (QueryTable qt in Tables)
			{
				if (!Lex.Eq(qt.MetaTableName, mtName)) continue;

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (Lex.Eq(qc.MetaColumnName, mcName))
						return qc;
				}
			}

			return null;
		}

		/// <summary>
		/// Get first result column in priority order
		/// </summary>
		/// <param name="mustBeNumeric"></param>
		/// <returns></returns>

		public QueryColumn GetFirstResultColumnByPriority(
			bool mustBeNumeric = true)
		{
			QueryColumn qc = GetFirstMatchingQueryColumn(mustBeSelected: true, mustBeNumeric: mustBeNumeric, mustBePrimaryResult: true);
			if (qc == null) qc = GetFirstMatchingQueryColumn(mustBeSelected: true, mustBeNumeric: mustBeNumeric, mustBeSecondaryResult: true);
			if (qc == null) qc = GetFirstMatchingQueryColumn(mustBeSelected: true, mustBeNumeric: mustBeNumeric);
			return qc;
		}

		/// <summary>
		/// Get the first QueryColumn in the first QueryTable with the specified attribute values
		/// </summary>
		/// <param name="columnType"></param>
		/// <param name="mustBeSelected"></param>
		/// <param name="mustHaveCriteria"></param>
		/// <param name="mustBeSorted"></param>
		/// <returns></returns>

		public QueryColumn GetFirstMatchingQueryColumn(
			MetaColumnType columnType = MetaColumnType.Any,
			bool mustBeSelected = false,
			bool mustHaveCriteria = false,
			bool mustBeSorted = false,
			bool mustBePrimaryResult = false,
			bool mustBeSecondaryResult = false,
			bool mustBeNumeric = false)
		{
			foreach (QueryTable qt in this.Tables)
			{
				QueryColumn qc = qt.GetFirsMatchingQueryColumn(columnType, mustBeSelected, mustHaveCriteria, mustBeSorted, mustBePrimaryResult, mustBeSecondaryResult, mustBeNumeric);
				if (qc != null) return qc;
			}

			return null;
		}

		/// <summary>
		/// Lookup a querytable by the name of the associated metatable & return index
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public QueryTable GetTableByName(
			string name)
		{
			int ti = GetTableIndexByName(name);
			if (ti >= 0) return Tables[ti];
			else return null;
		}

        /// <summary>
        /// Lookup a querytable by the name of the associated metatable & return table
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public int GetTableIndexByName(
            string name)
        {
            for (int ti = 0; ti < this.Tables.Count; ti++)
            {
                QueryTable qt = Tables[ti];
                if (Lex.Eq(qt.MetaTable.Name, name)) return ti;
            }
            return -1; // not found
        }

        public List<int> GetAllTableIndexesByName(
           string name)
        {
            List<int> tableIdx = new List<int>();
            for (int ti = 0; ti < this.Tables.Count; ti++)
            {
                QueryTable qt = Tables[ti];
                if (Lex.Eq(qt.MetaTable.Name, name)) tableIdx.Add(ti);
            }

            if (tableIdx.Count > 0) return tableIdx;
            else return null; // not found            
        }

        public List<int> GetAllCalcFieldTableIndexesWithMetaColumn(
           MetaColumn mcToFind)
        {            
            List<int> tableIdx = new List<int>();
            for (int ti = 0; ti < this.Tables.Count; ti++)
            {
                if (Tables[ti].MetaTable.IsCalculatedField)
                {                    
                    foreach (MetaColumn mc in Tables[ti].MetaTable.MetaColumns)
                    {
                        if (mc.TableMap == mcToFind.MetaTableDotMetaColumnName)
                        {
                            tableIdx.Add(ti);
                            continue;
                        }
                    }
                }                
            }

            if (tableIdx.Count > 0) return tableIdx;
            else return null; // not found            
        }

        /// <summary>
        /// Count number of selected columns for query
        /// </summary>
        /// <returns></returns>

        public int GetSelectedColumnCount()
		{
			int count = 0;
			foreach (QueryTable qt in Tables)
			{
				count += qt.SelectedCount;
			}

			return count;
		}

		/// <summary>
		/// Count criteria for query (doesn't include complex criteria)
		/// </summary>
		/// <returns></returns>

		public int GetCriteriaCount()
		{
			return GetCriteriaCount(true, false);
		}

		/// <summary>
		/// Count criteria for query (doesn't include complex criteria)
		/// </summary>
		/// <param name="includeKey"></param>
		/// <param name="includeDbSet"></param>
		/// <returns></returns>

		public int GetCriteriaCount(
			bool includeKey,
			bool includeDbSet)
		{
			int criteriaCount = 0;
			if (includeKey && KeyCriteria != "") criteriaCount++;

			foreach (QueryTable qt in Tables)
			{
				criteriaCount += qt.GetCriteriaCount(false, includeDbSet);
			}

			return criteriaCount;
		}

		/// <summary>
		/// Get a count of tables with criteria
		/// </summary>
		/// <param name="includeKey"></param>
		/// <param name="includeDbSet"></param>
		/// <returns></returns>

		public int GetTablesWithCriteriaCount(
			bool includeKey,
			bool includeDbSet)
		{
			int criteriaCount = 0;

			foreach (QueryTable qt in Tables)
			{
				if (qt.GetCriteriaCount(false, includeDbSet) > 0)
					criteriaCount++;
			}

			if (criteriaCount == 0 && includeKey && KeyCriteria != "")
				criteriaCount++; // count key criteria if exists & no tables with criteria so far & want to include

			return criteriaCount;
		}

		/// <summary>
		/// Get the first table with table-specific criteria
		/// </summary>
		/// <returns></returns>

		public QueryTable GetFirstTableWithCriteria()
		{
			foreach (QueryTable qt in Tables)
			{
				if (qt.GetCriteriaCount(false, false) > 0) return qt;
			}

			return null;
		}

		/// <summary>
		/// Clear all simple criteria from query
		/// </summary>
		/// <param name="q"></param>

		public void ClearAllQueryColumnCriteria()
		{
			foreach (QueryTable qt in this.Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					qc.CriteriaDisplay = qc.Criteria = "";
					qc.MolString = "";
					qc.FilterRetrieval = qc.FilterSearch = true;
				}
			}

			this.KeyCriteriaDisplay = this.KeyCriteria = "";
			return;
		}

		/// <summary>
		/// Get a count of the tables that get remapped to other forms during query execution
		/// </summary>
		/// <returns></returns>

		public int GetTablesRemappedCount()
		{
			int count = 0;
			foreach (QueryTable qt in Tables)
			{
				if (qt.MetaTable.MultiPivot) count++;
			}

			return count;
		}

		public void ValidateAggregation()
		{
			Aggregation.ValidateForQuery(this);
			return;
		}

		/// <summary>
		/// Get count of tables in the query that include aggregation
		/// </summary>
		/// <returns></returns>

		public int GetTablesAggregatedCount()
		{
			return Aggregation.GetTablesAggregatedCount(this);
		}

		/// <summary>
		/// Return true if query contains aggregated tables where each table includes aggregation that includes the key column
		/// </summary>

		public bool IsKeyAggreation
		{
			get { return Aggregation.IsKeyAggreation(this); }
		}

		/// <summary>
		/// Check query for a single table that has non-key aggregation
		/// </summary>

		public bool IsNonKeyAggreation
		{
			get { return Aggregation.IsNonKeyAggreation(this); }
		}

		/// <summary>
		/// Clear sorting for query
		/// </summary>

		public void ClearSorting()
		{
			foreach (QueryTable qt in this.Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
					qc.SortOrder = 0;
			}

			KeySortOrder = 0;
			return;
		}

		/// <summary>
		/// Update the SortOrder for each key column to match the KeySortOrder
		/// </summary>

		public void UpdateAllKeySortOrders()
		{
			foreach (QueryTable qt in Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.IsKey)
					{
						qc.SortOrder = KeySortOrder;
						break;
					}
				}
			}

			return;
		}

		/// <summary>
		/// Return true if this query retrieves data from Mobius
		/// </summary>
		/// <returns></returns>

		public bool RetrievesMobiusData
		{
			get
			{
				foreach (QueryTable qt in Tables)
				{
					if (qt.MetaTable.RetrievesMobiusData) return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Return true if this query retrieves data from the QueryEngine
		/// </summary>
		/// <returns></returns>

		public bool RetrievesDataFromQueryEngine
		{
			get
			{
				foreach (QueryTable qt in Tables)
				{
					if (qt.MetaTable.RetrievesDataFromQueryEngine) return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Count tables by brokerType and store in stats
		/// </summary>
		/// <param name="q"></param>

		public Dictionary<MetaBrokerType, int> CountTablesByBrokerType()
		{
			Dictionary<MetaBrokerType, int> dict = new Dictionary<MetaBrokerType, int>();

			foreach (QueryTable qt in Tables) // count tables by broker type
			{
				MetaBrokerType mbt = qt.MetaTable.MetaBrokerType;
				if (!dict.ContainsKey(mbt))
					dict[mbt] = 0;

				dict[mbt]++;
			}

			return dict;
		}

		/// <summary>
		/// Get a dictionary of QueryTables classified by MetaBrokerType
		/// </summary>
		/// <returns></returns>

		public Dictionary<MetaBrokerType, List<QueryTable>> ClassifyTablesByBrokerType()
		{
			Dictionary<MetaBrokerType, List<QueryTable>> mbd = new Dictionary<MetaBrokerType, List<QueryTable>>();

			foreach (QueryTable qt in Tables)
			{
				MetaBrokerType mbt = qt.MetaTable.MetaBrokerType;
				if (!mbd.ContainsKey(mbt)) mbd[mbt] = new List<QueryTable>();
				mbd[mbt].Add(qt);
			}

			return mbd;
		}

		/// <summary>
		/// Get the root table associated with the query.
		/// This table may not be included in the query
		/// </summary>

		public MetaTable RootMetaTable
		{
			get
			{
				if (Tables.Count == 0) return null;
				MetaTable mt = Tables[0].MetaTable;
				while (mt.Parent != null)
				{
					mt = mt.Parent;
				}

				return mt;
			}
		}

		/// <summary>
		/// Return true if a root table needs to be added to the query
		/// </summary>
		/// <returns></returns>

		public bool RootTableNeedsToBeAdded()
		{
			int rootPos;
			int qeTableCount;
			bool multiPivotSeen;
			bool rootTableNeedsToBeAdded;

			AnalyzeTableDataRetrieval(out rootPos, out qeTableCount, out multiPivotSeen, out rootTableNeedsToBeAdded);
			return rootTableNeedsToBeAdded;
		}

		/// <summary>
		/// Add root table & position as necessary
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public bool IncludeRootTableAsNeeded()
		{
			Query query = this;

			int rootPos;
			int qeTableCount;
			bool multiPivotSeen;
			bool rootTableNeedsToBeAdded;

			AnalyzeTableDataRetrieval(out rootPos, out qeTableCount, out multiPivotSeen, out rootTableNeedsToBeAdded);

			// This was already checked in the AnalyzeTableDataRetrieval, does not need to be checked again
			//if (qeTableCount <= 1 && !multiPivotSeen) return false; // ok as is

			if (!rootTableNeedsToBeAdded) return false; // ok as is

			return AddRootTable(rootPos);
		}

		/// <summary>
		/// Add root table and properly position and connect within the query
		/// </summary>
		/// <param name="rootPos"></param>
		/// <returns></returns>

		public bool AddRootTable(
				int rootPos = -1)
		{
			QueryTable rootQt = null, qt;
			QueryColumn qc;
			MetaTable rootMt, mt;
			MetaColumn mc;
			int ti, ci;

			Query query = this;
			if (rootPos >= 0) rootQt = query.Tables[rootPos];

			bool modified = false;
			if (rootPos == 0) { } // root table is first as it should be
			else if (rootPos > 0 && rootPos < query.Tables.Count) // move to first position
			{
				query.RemoveQueryTableAt(rootPos);
				query.InsertQueryTable(0, rootQt);
				modified = true;
			}

			else // no root table, add it in
			{
				mt = (query.Tables[0]).MetaTable;
				if (mt.Root == mt) return false; // shouldn't happen
				rootQt = new QueryTable(mt.Root); // build query table on root
				rootQt.Query = query;
				query.InsertQueryTable(0, rootQt); // put at beginning
				SetTableIndexes();

				for (ci = 0; ci < rootQt.QueryColumns.Count; ci++) // select just key column
				{
					qc = rootQt.QueryColumns[ci];
					if (qc.MetaColumn != null && qc.IsKey) qc.Selected = true;
					else qc.Selected = false;
				}
				modified = true;

				query.AssignUndefinedAliases(); // set alias for added table
			}

			// Be sure root table includes references for databases associated with all tables in query

			rootMt = rootQt.MetaTable;
			MetaColumn dbSetMc = rootMt.DatabaseListMetaColumn;
			if (dbSetMc != null) // include if table has defined dbSetMc
			{
				QueryColumn dbSetQc = rootQt.GetQueryColumnByName(dbSetMc.Name);
				if (dbSetQc != null)
				{
					for (ti = 1; ti < query.Tables.Count; ti++)
					{
						qt = query.Tables[ti];
						mt = qt.MetaTable;
						AssureDbInSet(mt.Parent, dbSetQc);
					}
				}
			}

			return modified;
		}

		/// <summary>
		/// Analyze tables in query gathering the information below 
		/// </summary>
		/// <param name="rootPos"></param>
		/// <param name="qeTableCount"></param>
		/// <param name="multiPivotSeen"></param>
		/// <param name="rootTableNeedsToBeAdded"></param>

		public void AnalyzeTableDataRetrieval(
			out int rootPos,
			out int qeTableCount,
			out bool multiPivotSeen,
			out bool rootTableNeedsToBeAdded)
		{
			Query query = this;

			QueryTable rootQt = null, qt;
			QueryColumn qc;
			MetaTable rootMt, mt;
			MetaColumn mc;
			int ti, ci;

			rootPos = -1; // position of root table
			multiPivotSeen = false; // if multipivot seen then treat like multiple tables
			qeTableCount = 0;
			//int tablesContainingIsNull = 0;
			int tablesContainingNotExists = 0;
			bool otherCriteria = false;

			for (ti = 0; ti < query.Tables.Count; ti++) // see if root included already, count data retrieving tables
			{
				qt = query.Tables[ti];
				mt = qt.MetaTable;

				if (!mt.RetrievesDataFromQueryEngine) continue; // only consider tables that retrieve data from QE (e.g. not Spotfire views)

				//int criteriaWithIsNull = 0; // "is null" is not supported as "not exists"
				int criteriaWithNotExists = 0;
				int criteriaOther = 0;

				foreach (QueryColumn qc0 in qt.QueryColumns)
				{
					//if (qc0.HasNotExistsCriteria)
					//	criteriaWithIsNull++;

					if (qc0.HasNotExistsCriteria)
						criteriaWithNotExists++;

					else if (!String.IsNullOrEmpty(qc0.Criteria))
						otherCriteria = true;
				}

				//if (criteriaWithIsNull > 0) tablesContainingIsNull++;
				if (criteriaWithNotExists > 0) tablesContainingNotExists++;

				qeTableCount++;

				if (mt.Root == mt) // is this root 
				{
					rootPos = ti;
					rootQt = query.Tables[ti];
				}

				else if (mt.MultiPivot) multiPivotSeen = true;
			}

			if (rootPos >= 0) // we already have the root.  We are done here!
			{
				rootTableNeedsToBeAdded = false;
				return;
			}

			if (qeTableCount <= 1 && !multiPivotSeen) rootTableNeedsToBeAdded = false;
			else rootTableNeedsToBeAdded = true;

			// if all tables have "is null" criteria, and there is more than one table, the root needs to be added
			//if (tablesContainingIsNull == query.Tables.Count && query.Tables.Count > 1) rootTableNeedsToBeAdded = true;

			// If there is only one table, and there is both a DOES NOT EXIST criteria and other criteria than we need to add the root.
			// if (otherCriteria && tablesContainingIsNull == 1 && query.Tables.Count == 1) rootTableNeedsToBeAdded = true;

			return;
		}

		/// <summary>
		/// Assure that this database set query criteria includes the specified root table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool AssureDbInSet(
			MetaTable mt,
			QueryColumn qc)
		{
			bool modified = false;
			int i1;

			MetaColumn mc = qc.MetaColumn;

			if (mt == null) return false; // get value that appears in criteria for table
			RootTable rti = RootTable.GetFromTableName(mt.Name);
			if (rti == null) return false;

			ParsedSingleCriteria sc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			if (sc == null || sc.Op == "") // first entry 
			{
				sc = new ParsedSingleCriteria();
				sc.Op = "in";
				sc.ValueList = new List<string>();
				sc.ValueList.Add(rti.Label);
				modified = true;
			}

			else // see if in list & add if not 
			{
				for (i1 = 0; i1 < sc.ValueList.Count; i1++)
				{
					if (Lex.Eq((string)sc.ValueList[i1], rti.Label)) break;
				}
				if (i1 >= sc.ValueList.Count)
				{
					sc.ValueList.Add(rti.Label);
					modified = true;
				}
			}

			if (modified) MqlUtil.ConvertParsedSingleCriteriaToQueryColumnCriteria(sc, qc);
			return modified;
		}

		/// <summary>
		/// Deserialize a query from a file possibly containing complete metatables & data
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static Query DeserializeFromFile(
			string fileName)
		{
			StreamReader sr = new StreamReader(fileName);
			XmlTextReader tr = new XmlTextReader(sr);
			Query q = Deserialize(tr, null);
			tr.Close();
			sr.Close();
			return q;
		}

		/// <summary>
		/// Convert XML to query objects
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static Query Deserialize(
			string content)
		{
			if (Lex.IsNullOrEmpty(content)) return new Query();

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(content);
			XmlTextReader tr = mstr.Reader;
			Query q = Deserialize(tr, null);
			tr.Close();
			return q;
		}

		/// <summary>
		/// Deserialize query from XmlTextReader
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="parent">Any parent query</param>
		/// <returns></returns>

		public static Query Deserialize(
			XmlTextReader tr,
			Query parent)
		{
			QueryTable qt;
			QueryColumn qc;
			string versionString = "";
			string txt, key;
			int i1 = -1, parentId = -1;

			Query q = new Query();
			q.Parent = parent;
			if (parent != null) q.ResultsPages = parent.ResultsPages; // common set of resultspages for all queries in the hierarchy

			tr.MoveToContent();

			if (!Lex.Eq(tr.Name, "Query"))
				throw new Exception("No \"Query\" element in query");

			XmlUtil.GetStringAttribute(tr, "SerializerVersion", ref versionString);
			if (Lex.IsNullOrEmpty(versionString))
				DeserializerVersion = new Version(1, 0);
			else DeserializerVersion = new Version(versionString); // save the current version of the deserializer

			XmlUtil.GetIntAttribute(tr, "InstanceId", ref q.InstanceId);

			XmlUtil.GetStringAttribute(tr, "Transform", ref q.Transform);

			txt = tr.GetAttribute("Mode");
			if (txt != null)
				q.Mode = (QueryMode)EnumUtil.Parse(typeof(QueryMode), txt);
			if (q.Mode == QueryMode.Unknown) q.Mode = QueryMode.Build;

			XmlUtil.GetStringAttribute(tr, "KeyCriteriaDisplay", ref q.KeyCriteriaDisplay);
			XmlUtil.GetStringAttribute(tr, "KeyCriteria", ref q.KeyCriteria);

			XmlUtil.GetStringAttribute(tr, "Name", ref q.UserObject.Name);
			XmlUtil.GetStringAttribute(tr, "Description", ref q.UserObject.Description);
			XmlUtil.GetIntAttribute(tr, "UserObjectId", ref q.UserObject.Id);
			XmlUtil.GetStringAttribute(tr, "Owner", ref q.UserObject.Owner);
      XmlUtil.GetBoolAttribute(tr, "Mobile", ref q.Mobile);

      FolderTypeEnum folderType = FolderTypeEnum.None;
			txt = tr.GetAttribute("FolderId");
			if (txt != null)
			{
				q.UserObject.ParentFolder = txt;
				folderType = (txt.ToUpper().StartsWith("FOLDER_")) ? FolderTypeEnum.User : FolderTypeEnum.System;
			}
			string folderTypeTxt = tr.GetAttribute("FolderType");
			if (folderTypeTxt != null)
			{
				try
				{
					//try to parse the folder type in two ways
					int folderTypeInt = -1;
					if (Int32.TryParse(folderTypeTxt, out folderTypeInt))
						folderType = (FolderTypeEnum)folderTypeInt;
					else
						folderType = (FolderTypeEnum)Enum.Parse(typeof(FolderTypeEnum), folderTypeTxt, true);
				}
				catch (Exception e) { /* Do nothing */ }
			}

			q.UserObject.ParentFolderType = folderType;

			txt = tr.GetAttribute("UseAndLogic");
			if (txt != null)
			{
				bool b = Convert.ToBoolean(txt);
				if (b) q.LogicType = QueryLogicType.And;
			}

			txt = tr.GetAttribute("UseOrLogic");
			if (txt != null)
			{
				bool b = Convert.ToBoolean(txt);
				if (b) q.LogicType = QueryLogicType.Or;
			}

			txt = tr.GetAttribute("UseComplexLogic");
			if (txt != null)
			{
				bool b = Convert.ToBoolean(txt);
				if (b) q.LogicType = QueryLogicType.Complex;
			}

			txt = tr.GetAttribute("ComplexCriteria");
			if (txt != null) q.ComplexCriteria = txt;

			txt = tr.GetAttribute("FilterResults");
			if (txt != null) q.FilterResults = Convert.ToBoolean(txt);

			txt = tr.GetAttribute("FilterNullRows");
			if (txt != null) q.FilterNullRows = Convert.ToBoolean(txt);

			txt = tr.GetAttribute("KeySortDirection"); // obsolete, replaced with key sort order
			if (txt != null)
			{
				i1 = EnumUtil.Parse(typeof(SortOrder), txt);
				if (i1 == 1) q.KeySortOrder = 1; // single col ascending
				else if (i1 == 2) q.KeySortOrder = -1; // single col descending
				else q.KeySortOrder = 0; // no sorting
			}

			txt = tr.GetAttribute("KeySortOrder");
			if (txt != null) q.KeySortOrder = Convert.ToInt32(txt);

			XmlUtil.GetBoolAttribute(tr, "GroupSalts", ref q.GroupSalts);

			//XmlUtil.GetBoolAttribute(tr, "AllowColumnMerging", ref q.AllowColumnMerging);// no longer allowed

			XmlUtil.GetBoolAttribute(tr, "Preview", ref q.Preview);

			XmlUtil.GetBoolAttribute(tr, "SingleStepExecution", ref q.SingleStepExecution);

			XmlUtil.GetBoolAttribute(tr, "RunQueryWhenOpened", ref q.RunQueryWhenOpened);
			q.RunQueryWhenOpened = false; // disabled for now

			XmlUtil.GetBoolAttribute(tr, "BrowseSavedResultsUponOpen", ref q.BrowseSavedResultsUponOpen);

			XmlUtil.GetBoolAttribute(tr, "UseResultKeys", ref q.UseResultKeys);

			XmlUtil.GetStringAttribute(tr, "ResultKeysListName", ref q.ResultKeysListName);

			XmlUtil.GetBoolAttribute(tr, "SerializeMetaTablesWithQuery", ref q.SerializeMetaTablesWithQuery);

			XmlUtil.GetBoolAttribute(tr, "SerializeResultsWithQuery", ref q.SerializeResultsWithQuery);

			XmlUtil.GetStringAttribute(tr, "ResultsDataTableFileName", ref q.ResultsDataTableFileName);

			XmlUtil.GetStringAttribute(tr, "WarningMessage", ref q.WarningMessage);

			txt = tr.GetAttribute("InitialBrowsePage");
			if (txt != null) q.InitialBrowsePage = Convert.ToInt32(txt);

			XmlUtil.GetBoolAttribute(tr, "Multitable", ref q.Multitable);

			XmlUtil.GetBoolAttribute(tr, "Mobile", ref q.Mobile);

            //XmlUtil.GetBoolAttribute(tr, "MobileDefault", ref q.MobileDefault);

            XmlUtil.GetBoolAttribute(tr, "RepeatReport", ref q.RepeatReport);

			XmlUtil.GetBoolAttribute(tr, "DuplicateKeyValues", ref q.DuplicateKeyValues);

			//XmlUtil.GetIntAttribute(tr, "ViewScale", ref q.ViewScale); // disable getting scale for now

			XmlUtil.GetIntAttribute(tr, "PrintScale", ref q.PrintScale);

			XmlUtil.GetIntAttribute(tr, "PrintMargins", ref q.PrintMargins);

			txt = tr.GetAttribute("PrintOrientation");
			if (txt != null) q.PrintOrientation = (Orientation)Enum.Parse(typeof(Orientation), txt);

			XmlUtil.GetBoolAttribute(tr, "ShowCondFormatLabels", ref q.ShowCondFormatLabels);

			XmlUtil.GetBoolAttribute(tr, "ShowStereoComments", ref q.ShowStereoComments);

			XmlUtil.GetIntAttribute(tr, "SmallSsqMaxAtoms", ref q.SmallSsqMaxAtoms);

			XmlUtil.GetIntAttribute(tr, "Timeout", ref q.Timeout);

			//XmlUtil.GetStringAttribute(tr, "SpotfireTemplateName", ref q.SpotfireTemplateName);

			//XmlUtil.GetStringAttribute(tr, "SpotfireTemplateContent", ref q.SpotfireTemplateContent);

			//XmlUtil.GetStringAttribute(tr, "SpotfireDataFile", ref q.SpotfireDataFileName);

			txt = tr.GetAttribute("StatDisplayFormat");
			if (txt != null) q.StatDisplayFormat = (QnfEnum)Convert.ToInt32(txt);

			while (true) // loop on query tables && other main elements
			{
				if (!XmlUtil.MoreSubElements(tr, "Query")) break;

				// Prefixed list of MetaTables associated with query to support disconnected queries and 
				// MetaTables defined at runtime by Tools and not known elsewhere.

				else if (Lex.Eq(tr.Name, "MetaTables"))
				{
					if (!tr.IsEmptyElement)
					{
						MetaTable[] mts = MetaTable.DeserializeMetaTables(tr, null, true);
						int mtCount = mts.Length;
					}
				}

				// QueryTable 				

				else if (Lex.Eq(tr.Name, "QueryTable"))
				{
					qt = DeserializeQueryTable(tr, q, versionString);
				}

				// Results pages associated with query

				else if (Lex.Eq(tr.Name, "ResultsPages"))
				{
					q.ResultsPages = ResultsPages.Deserialize(q, tr);
				}

				// Obsolete ChartPages element, just parse & discard

				else if (Lex.Eq(tr.Name, "ChartPages"))
				{
					while (true)
					{
						tr.Read(); tr.MoveToContent();
						if (tr.NodeType == XmlNodeType.EndElement && Lex.Eq(tr.Name, "ChartPages")) break;
					}
				}

				// DataTable of results

				else if (Lex.Eq(tr.Name, "DataTable"))
				{
					if (IDataTableManager == null) throw new Exception("DataTableManager not defined");
					q.ResultsDataTable = IDataTableManager.Deserialize(tr);
				}

				// List of keys to be excluded from search results

				else if (Lex.Eq(tr.Name, "KeysToExclude"))
				{
					List<string> keyList = DeserializeKeyList(tr);
					q.KeysToExclude = new HashSet<string>(keyList);
				}

				// List of result keys

				else if (Lex.Eq(tr.Name, "ResultKeys"))
					q.ResultKeys = DeserializeKeyList(tr);

				// Subqueries

				else if (Lex.Eq(tr.Name, "Subqueries"))
				{
					if (tr.IsEmptyElement) continue; // just continue if empty

					while (true)
					{
						tr.Read(); // move to next main level element
						tr.MoveToContent();

						if (Lex.Eq(tr.Name, "Query"))
						{
							Query sq = Query.Deserialize(tr, q);
							q.Subqueries.Add(sq);
						}

						else if (tr.NodeType == XmlNodeType.EndElement &&
							Lex.Eq(tr.Name, "Subqueries")) break;

						else throw new Exception("Expected \"Query\" or \"Subqueries\" end element");

					}
				}

				else throw new Exception("Unexpected element: " + tr.Name);

			} // end of loop on query elements

			q.AssignUndefinedAliases(); // be sure every table has an alias

			List<ResultsPage> pages = q.ResultsPages.Pages;
			if (pages.Count == 1 && pages[0].Views.Count == 1 && pages[0].Views[0].ViewType == ResultsViewType.SecondaryQuery)
				q.ResultsPages.Pages.Clear(); // eliminate bogus CfSummaryView, see query 302361

			if (q.FirstView == null) // if no view setup default
				q.SetupQueryPagesAndViews();

			return q;
		}

		/// <summary>
		/// Setup query pages and views
		/// </summary>
		/// <param name="tableViewType"></param>

		public void SetupQueryPagesAndViews(
			ResultsViewType tableViewType)
		{
			ResultsPages.SetupQueryPagesAndViews(this, tableViewType);
			return;
		}

		/// <summary>
		/// Setup query pages and views for grid table
		/// </summary>
		/// <returns></returns>

		public void SetupQueryPagesAndViews()
		{
			ResultsPages.SetupQueryPagesAndViews(this, ResultsViewType.Table);
			return;
		}

		/// <summary>
		/// Deserialize a QueryTable
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="q"></param>
		/// <returns></returns>

		static QueryTable DeserializeQueryTable(
			XmlTextReader tr,
			Query q,
			string versionString)
		{
			QueryTable qt, qtColsNotFound;
			QueryColumn qc;
			MetaTable mt = null;
			MetaColumn mc;
			string mcName, txt;
			int i1;

			Color obsoleteNullColor = Color.FromArgb(85, 85, 85, 85); // obsolete null color value (alternate bits on)

			qt = new QueryTable();
			qtColsNotFound = new QueryTable();

			string mtName = tr.GetAttribute("MetaTable");
			if (mtName != null)
			{
				txt = tr.GetAttribute("Summarized"); // see if old query table summarized attribute
				if (txt != null && Convert.ToBoolean(txt) == true)
					mtName += MetaTable.SummarySuffix; // convert to new metatable name
				mtName = mtName.ToUpper(); // be sure upper case

				try
				{
					mt = MetaTableCollection.GetWithException(mtName); // may be null if table now gone or source not available
					qt.MetaTable = mt;
					if (mt == null || mt.MetaColumns.Count == 0)
						AddInaccessibleTableMessage(q, qt, mtName, "The data table does not contain any valid columns.");
				}
				catch (Exception ex)
				{
					AddInaccessibleTableMessage(q, qt, mtName, ex.Message);
				}

			}
			else throw new Exception("MetaTable attribute not found");

			XmlUtil.GetStringAttribute(tr, "Label", ref qt.Label);
			XmlUtil.GetStringAttribute(tr, "Alias", ref qt.Alias);

			XmlUtil.GetBoolAttribute(tr, "AllowColumnMerging", ref qt.AllowColumnMerging);

			txt = tr.GetAttribute("HeaderBackgroundColor");
			if (txt != null)
				try
				{
					qt.HeaderBackgroundColor = Color.FromArgb(Convert.ToInt32(txt));
					if (qt.HeaderBackgroundColor == obsoleteNullColor) // convert old query
						qt.HeaderBackgroundColor = Color.Empty;
				}
				catch (Exception ex) { }

			XmlUtil.GetIntAttribute(tr, "VoPosition", ref qt.VoPosition);

			XmlUtil.GetBoolAttribute(tr, "Aggregate", ref qt.AggregationEnabled);

			if (tr.IsEmptyElement) // just a querytable header?
			{
				return qt; // return qt without adding to query, don't read ahead
			}

			tr.Read(); // move to first QueryColumn
			tr.MoveToContent();

			while (true) // loop on query columns
			{
				if (tr.NodeType == XmlNodeType.EndElement &&
					Lex.Eq(tr.Name, "QueryColumn"))  // done with this column
				{
					tr.Read(); // move to next QueryColumn or QueryTable end element
					tr.MoveToContent();
					continue;
				}

				else if (tr.NodeType == XmlNodeType.EndElement && // done with all columns
					Lex.Eq(tr.Name, "QueryTable")) break;

				else if (tr.NodeType != XmlNodeType.Element ||
					!Lex.Eq(tr.Name, "QueryColumn"))
					throw new Exception("QueryColumn element not found");

				qc = new QueryColumn();
				qc.QueryTable = qt;

				mcName = tr.GetAttribute("MetaColumn");
				if (qt.MetaTable == null) { } // don't look up if metatable is null
				else if (Lex.IsDefined(mcName)) qc.MetaColumn = qt.MetaTable.GetMetaColumnByName(mcName);
				else throw new Exception("MetaColumn attribute not found");

				txt = tr.GetAttribute("Selected");
				if (txt != null) qc.Selected = Convert.ToBoolean(txt);

				txt = tr.GetAttribute("Hidden");
				if (txt != null) qc.Hidden = Convert.ToBoolean(txt);

				txt = tr.GetAttribute("Criteria");
				if (txt != null) qc.Criteria = txt;

				txt = tr.GetAttribute("CriteriaDisplay");
				if (txt != null) qc.CriteriaDisplay = txt;

				txt = tr.GetAttribute("FilterSearch");
				if (txt != null) qc.FilterSearch = Convert.ToBoolean(txt);

				txt = tr.GetAttribute("FilterRetrieval");
				if (txt != null) qc.FilterRetrieval = Convert.ToBoolean(txt);

				txt = tr.GetAttribute("SecondaryCriteria");
				if (txt != null) qc.SecondaryCriteria = txt;

				txt = tr.GetAttribute("SecondaryCriteriaDisplay");
				if (txt != null) qc.SecondaryCriteriaDisplay = txt;

				txt = tr.GetAttribute("SecondaryCriteriaType");
				if (txt != null)
					qc.SecondaryFilterType = (FilterType)EnumUtil.Parse(typeof(FilterType), txt);

				txt = tr.GetAttribute("ShowOnCriteriaForm");
				if (txt != null) qc.ShowOnCriteriaForm = Convert.ToBoolean(txt);

				txt = tr.GetAttribute("SortOrder");
				if (txt != null && Lex.Ge(versionString, "3.0")) // ignore invalid values if older version
					qc.SortOrder = Convert.ToInt32(txt);

				XmlUtil.GetBoolAttribute(tr, "Merge", ref qc.Merge);
				XmlUtil.GetStringAttribute(tr, "QueryStructure", ref qc.MolString);

				XmlUtil.GetStringAttribute(tr, "Label", ref qc.Label);

				txt = tr.GetAttribute("DisplayFormat");
				if (txt != null)
				{
					i1 = EnumUtil.Parse(typeof(ColumnFormatEnum), txt);
					qc.DisplayFormat = (ColumnFormatEnum)i1;
				}

				XmlUtil.GetStringAttribute(tr, "DisplayFormatString", ref qc.DisplayFormatString);
				XmlUtil.GetFloatAttribute(tr, "DisplayWidth", ref qc.DisplayWidth);
				XmlUtil.GetIntAttribute(tr, "Decimals", ref qc.Decimals);

				txt = tr.GetAttribute("HorizontalAlignment");
				if (txt != null) qc.HorizontalAlignment =
					(HorizontalAlignmentEx)Enum.Parse(typeof(HorizontalAlignmentEx), txt);

				txt = tr.GetAttribute("VerticalAlignment");
				if (txt != null) qc.VerticalAlignment =
					(VerticalAlignmentEx)Enum.Parse(typeof(VerticalAlignmentEx), txt);

				XmlUtil.GetIntAttribute(tr, "VoPosition", ref qc.VoPosition);

				if (!tr.IsEmptyElement) // if not empty, loop on any other QueryColumn elements
					while (true)
					{
						tr.Read(); // move to next main level element or QueryColumn end element
						tr.MoveToContent();

						if (tr.NodeType == XmlNodeType.EndElement && // done 
							Lex.Eq(tr.Name, "QueryColumn")) break;

						else if (tr.NodeType != XmlNodeType.Element)
							throw new Exception("Expected XmlNodeType.Element but saw: " + tr.NodeType.ToString() + " " + tr.Name);

						else if (Lex.Eq(tr.Name, "CondFormat")) // new form of formatting
						{
							qc.CondFormat = CondFormat.Deserialize(tr);
						}

						else if (tr.NodeType == XmlNodeType.Element &&
							Lex.Eq(tr.Name, "ConditionalFormatting")) // old form of formatting
						{
							txt = tr.ReadString();
							qc.CondFormat = CondFormat.DeserializeOld(txt);
						}

						else if (tr.NodeType == XmlNodeType.Element &&
							Lex.Eq(tr.Name, "Aggregation")) // Aggregation
						{
							qc.Aggregation = AggregationDef.Deserialize(tr);
						}

						else throw new Exception("Unexpected element: " + tr.Name);

					} // end of loop on additional QueryColumn elements

				tr.Read(); // advance to next main element
				tr.MoveToContent();

				if (qc.MetaColumn != null)
				{
					if (qc.IsKey) qt.QueryColumns.Insert(0, qc); // be sure key is first
					else qt.QueryColumns.Add(qc); // just append if not key
				}

				else if (Lex.IsDefined(mcName)) // accumulate query cols not found in metatable
				{
					qc.Label = mcName;  // store name in label
					qtColsNotFound.AddQueryColumn(qc);
				}
			} // end of loop on query columns

			if (qt.KeyQueryColumn != null && !qt.KeyQueryColumn.Selected)
				qt.KeyQueryColumn.Selected = true; // fixup to be sure key is selected

			if (qt.MetaTable != null) // add query table only if we have an associated metatable 
			{
				q.AddQueryTable(qt); // add table allowing dups

				foreach (MetaColumn mc0 in qt.MetaTable.MetaColumns)
				{ // see if there are any new metacolumns & if so add to end of query table
					if (mc0 == null) continue;

					qc = qt.GetQueryColumnByName(mc0.Name);
					if (qc != null) continue;
					qc = new QueryColumn();
					qc.MetaColumn = mc0;
					qc.QueryTable = qt;
					qc.Selected = false;
					qt.QueryColumns.Add(qc);
				}
			}
			return qt;
		}

		/// <summary>
		/// Maintain list of query tables that metatable couldn't be handled for
		/// </summary>
		/// <param name="q"></param>
		/// <param name="qt"></param>
		/// <param name="mtName"></param>
		/// <param name="msg"></param>

		static void AddInaccessibleTableMessage(Query q, QueryTable qt, string mtName, string msg)
		{
			if (q.InaccessableData == null)
				q.InaccessableData = new Dictionary<string, List<string>>();

			if (qt != null) qt.Label = mtName;
			string schemaKey = msg;
			if (q.InaccessableData.ContainsKey(schemaKey))
				q.InaccessableData[schemaKey].Add(mtName);

			else
			{
				List<string> qtList = new List<string>();
				qtList.Add(mtName);
				q.InaccessableData[schemaKey] = qtList;
			}
		}

		/// <summary>
		/// Serialize query for debugging (i.e. with no exceptions)
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static string SerializeForDebugging(QueryTable qt)
		{
			if (qt == null || qt.Query == null) return "";
			return qt.Query.SerializeForDebugging();
		}

		/// <summary>
		/// Serialize query for debugging (i.e. with no exceptions)
		/// </summary>
		/// <returns></returns>

		public string SerializeForDebugging(
			bool includeMetaTables = false,
			bool excludePreviouslyIncludedMetaTables = false)
		{
			try
			{
				string queryString = Serialize(includeMetaTables, excludePreviouslyIncludedMetaTables);
				return queryString;
			}
			catch (Exception ex)
			{
				return "Exception serializing: " + DebugLog.FormatExceptionMessage(ex);
			}
		}

		/// <summary>
		/// Serialize a query to a file
		/// </summary>
		/// <param name="fileName"></param>

		public void SerializeToFile(
			string fileName,
			bool includeMetaTables = false,
			bool excludePreviouslyIncludedMetaTables = false)
		{
			string s = Serialize(includeMetaTables, excludePreviouslyIncludedMetaTables);
			StreamWriter sw = new StreamWriter(fileName);
			sw.Write(s);
			sw.Close();
		}

		/// <summary>
		/// Serialize query into an Xml string
		/// </summary>
		/// <param name="includeMetaTables">If true include metatables</param>
		/// <returns></returns>

		public string Serialize(
			bool includeMetaTables = false,
			bool excludePreviouslyIncludedMetaTables = false)
		{
			MemoryStream ms = new MemoryStream();
			XmlTextWriter tw = new XmlTextWriter(ms, null);
			tw.Formatting = Formatting.Indented;
			tw.WriteStartDocument();
			Serialize(tw, includeMetaTables, excludePreviouslyIncludedMetaTables);
			tw.WriteEndDocument();

			tw.Flush();

			byte[] buffer = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(buffer, 0, (int)ms.Length);
			string content = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)ms.Length);
			tw.Close(); // must close after read
			return content;
		}

		/// <summary>
		/// Serialize query to Xml TextWriter
		/// </summary>
		/// <param name="tw"></param>

		public void Serialize(
			XmlTextWriter tw,
			bool includeMetaTables,
			bool excludePreviouslyIncludedMetaTables)
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			int ti, ci;
			string tok;

			int t0 = TimeOfDay.Milliseconds();

			if (Parent == null) NormalizeQueryIds();

			tw.WriteStartElement("Query");

			SerializeBasicQueryAttributes(tw);

			// Serialize MetaTables if requested.
			// This allows the serialize query to be used offline where there is not access to MetaTables

			if (this.SerializeMetaTablesWithQuery || Query.SerializeMetaTables || includeMetaTables)
			{
				tw.WriteStartElement("MetaTables");
				for (ti = 0; ti < Tables.Count; ti++)
				{
					qt = Tables[ti];
					mt = qt.MetaTable;
					if (excludePreviouslyIncludedMetaTables && SerializedMetaTableNames.Contains(mt.Name))
						continue;

					mt.Serialize(tw);
					if (excludePreviouslyIncludedMetaTables) SerializedMetaTableNames.Add(mt.Name);
				}
				tw.WriteEndElement();
			}

			// Serialize QueryTables.

			SerializeQueryTables(tw);

			// Serialize any subqueries 
			// Do before ResultsPages so ResultsPages deserialization will have any subqueries available to link to.

			if (Subqueries.Count > 0)
			{
				tw.WriteStartElement("SubQueries");
				foreach (Query sq in Subqueries)
				{
					sq.Serialize(tw, includeMetaTables, excludePreviouslyIncludedMetaTables);
				}
				tw.WriteEndElement();
			}

			// Serialize ResultsPages for root query

			if (ResultsPages != null && Parent == null)
			{
				ResultsPages.Serialize(tw);
			}

			// Write out list of keys to exclude if defined

			if (KeysToExclude != null && KeysToExclude.Count > 0)
			{
				tw.WriteStartElement("KeysToExclude");
				string serializedKeys = SerializeKeyList(new List<string>(KeysToExclude));
				tw.WriteCData(serializedKeys);
				tw.WriteEndElement();
			}

			// If saving & (browse mode or UseResultsKeys set) and we have a set of keys include those in xml

			if ((Mode == QueryMode.Browse || UseResultKeys) && ResultKeys != null)
			{
				tw.WriteStartElement("ResultKeys");
				string serializedKeys = SerializeKeyList(ResultKeys);
				tw.WriteCData(serializedKeys);
				tw.WriteEndElement();
			}

			// If results exist & should be serialized with query include those in xml

			if (SerializeResultsWithQuery && ResultsDataTable != null && IDataTableManager != null)
			{
				IDataTableManager.SerializeDataTable(tw, ResultsDataTable);
			}

			// Finish up

			tw.WriteEndElement(); // end of Query
			t0 = TimeOfDay.Milliseconds() - t0; // elapsed time

			return;
		}

		/// <summary>
		/// Normalize query ids within the set of root and any subqueries so they are 
		/// sequential and unique.
		/// </summary>

		public void NormalizeQueryIds()
		{
			int nextId = 1;
			NormalizeQueryIds(ref nextId);
			return;
		}

		/// <summary>
		/// Normalize query ids within the set of root and any subqueries so they are 
		/// sequential and unique.
		/// </summary>
		/// <param name="nextId"></param>

		public void NormalizeQueryIds(
			ref int nextId)
		{
			InstanceId = nextId;
			nextId++;

			foreach (Query sq in Subqueries)
			{
				sq.NormalizeQueryIds(ref nextId);
			}

			return;
		}

		/// <summary>
		/// Clear any existing QueryMangers for the root query and subqueries
		/// </summary>

		public void ClearQueryManagers()
		{
			ClearQueryManagers(this);
		}

		/// <summary>
		/// Clear QueryManagers for this query & lower level queries
		/// </summary>
		/// <param name="q"></param>

		public static void ClearQueryManagers(Query q)
		{
			q.QueryManager = null;
			foreach (Query sq in q.Subqueries)
			{
				ClearQueryManagers(sq);
			}
		}

/// <summary>
/// Clear any resources outside of the elements of the Query object that get serialized (e.g. Querymanager, forms, controls)
/// </summary>

		public void FreeControlReferences()
		{
			QueryManager = null;

			foreach (ResultsPage p in ResultsPages.Pages)
			{
				p.FreeControlReferences();
			}

			List<ResultsViewProps> views = GetResultsViews();

			foreach (ResultsViewProps v in views)
			{
				v.FreeControlReferences();
			}

			return;
		}

		/// <summary>
		/// Get a list of all of the views associated with the query
		/// </summary>
		/// <returns></returns>

		public List<ResultsViewProps> GetResultsViews()
		{
			ResultsViewProps activeView;

			List<ResultsViewProps> views = GetResultsViews(out activeView);
			return views;
		}

		/// <summary>
		/// Get a list of all of the views associated with the query and the current active view on the 
		/// </summary>
		/// <returns></returns>

		public List<ResultsViewProps> GetResultsViews(
		out ResultsViewProps activeView)
		{
			activeView = null;

			List<ResultsViewProps> views = new List<ResultsViewProps>();
			ResultsPages pages = ResultsPages;
			if (pages == null) return views;

			for (int pi = 0; pi < pages.Pages.Count; pi++)
				{
				ResultsPage page = pages.Pages[pi];

				foreach (ResultsViewProps view in page.Views)
				{
					if (pi == InitialBrowsePage)
						activeView = view;

					views.Add(view);
				}
			}
			return views;
		}

		/// <summary>
		/// Reset the state of the views 
		/// </summary>

		public void ResetViewStates()
		{
			SpotfireDataFileName = ""; // clear this so that if exporting to Spotfire we will export new files

			if (QueryManager != null) // clean up any DevExpress/Mobius controls 
				QueryManager.DisposeOfControls();

			if (ResultsPages == null || ResultsPages.Pages == null) return;
			foreach (ResultsPage rp in ResultsPages.Pages)
			{
				ResultsViewProps view = rp.FirstView;
				if (view == null) continue;

				view.FreeControlReferences();
				view.RenderingControl = null;
				view.ConfigureCount = 0;

				view.ResetStateForNewQueryExecution(); // reset other view-specific state information
			}

			if (!InResetViewState) // avoid recursive loop
			{
				InResetViewState = true;

				if (PresearchDerivedQuery != null) // also do for any derived queries
					PresearchDerivedQuery.ResetViewStates();

				if (PresearchBaseQuery != null)
					PresearchBaseQuery.ResetViewStates();

				InResetViewState = false;
			}
		}

		/// <summary>
		/// Return first view for Query
		/// </summary>

		public ResultsViewProps FirstView
		{
			get
			{
				if (ResultsPages == null) return null;
				else return ResultsPages.FirstView;
			}
		}

		/// <summary>
		/// Merge the subqueries of the supplied query into the current query
		/// </summary>
		/// <param name="q"></param>

		public void MergeSubqueries(Query q2)
		{
			if (q2 == null || q2.Tables.Count == 0 || q2.Subqueries.Count == 0)
				return;

			foreach (Query sq in q2.Subqueries) // add the subqueries
			{
				Subqueries.Add(sq);
				sq.Parent = this;
			}

			List<ResultsPage> pages = ResultsPages.Pages; // pages we have so far

			for (int pi = 0; pi < q2.ResultsPages.Pages.Count; pi++) // add the pages & views associated with the secondary query
			{
				ResultsPage sp = q2.ResultsPages.Pages[pi]; // add the results pages to the root query
				if (pi == 0) // if first page just copy title for first page & view
				{
					if (pages.Count > 0 && pages[0].Views.Count > 0 &&
					sp.Views.Count > 0 && sp.Views[0].BaseQuery == q2)
					{
						ResultsViewProps sv = sp.Views[0];

						ResultsPage page = pages[0];
						ResultsViewProps view = page.Views[0];
						page.Title = sp.Title; // copy page title
						view.Title = sv.Title; // copy view title
					}
				}

				else ResultsPages.AddPage(sp); // add the subpage
			}
		}

		/// <summary>
		/// Serialize query attributes
		/// </summary>
		/// <param name="tw"></param>

		void SerializeBasicQueryAttributes(XmlTextWriter tw)
		{
			Query q0 = new Query(); // used to check for non-default value

			tw.WriteAttributeString("SerializerVersion", SerializerVersion.ToString());

			if (InstanceId != q0.InstanceId)
				tw.WriteAttributeString("InstanceId", InstanceId.ToString());

			if (Transform != q0.Transform)
				tw.WriteAttributeString("Transform", Transform.ToString());

			if (Mode != q0.Mode)
				tw.WriteAttributeString("Mode", Mode.ToString());
			if (KeyCriteriaDisplay != q0.KeyCriteriaDisplay)
				tw.WriteAttributeString("KeyCriteriaDisplay", KeyCriteriaDisplay);
			if (KeyCriteria != q0.KeyCriteria)
				tw.WriteAttributeString("KeyCriteria", KeyCriteria);

			if (UserObject != null)
			{
				if (UserObject.Name != q0.UserObject.Name)
					tw.WriteAttributeString("Name", UserObject.Name);
				if (!Lex.IsNullOrEmpty(UserObject.Description))
					tw.WriteAttributeString("Description", UserObject.Description);
				if (UserObject.Id > 0)
					tw.WriteAttributeString("UserObjectId", UserObject.Id.ToString());
				if (UserObject.Owner != q0.UserObject.Owner)
					tw.WriteAttributeString("Owner", UserObject.Owner);
				if (UserObject.ParentFolder != q0.UserObject.ParentFolder)
					tw.WriteAttributeString("FolderId", UserObject.ParentFolder);
				if (UserObject.ParentFolderType != q0.UserObject.ParentFolderType)
					tw.WriteAttributeString("FolderType", UserObject.ParentFolderType.ToString());
			}

			if (LogicType == QueryLogicType.And)
				tw.WriteAttributeString("UseAndLogic", true.ToString());
			else if (LogicType == QueryLogicType.Or)
				tw.WriteAttributeString("UseOrLogic", true.ToString());
			else if (LogicType == QueryLogicType.Complex)
				tw.WriteAttributeString("UseComplexLogic", true.ToString());

			if (ComplexCriteria != q0.ComplexCriteria)
				tw.WriteAttributeString("ComplexCriteria", ComplexCriteria);
			if (FilterResults != q0.FilterResults)
				tw.WriteAttributeString("FilterResults", FilterResults.ToString());
			if (FilterNullRows != q0.FilterNullRows)
				tw.WriteAttributeString("FilterNullRows", FilterNullRows.ToString());

			if (KeySortOrder != q0.KeySortOrder)
				tw.WriteAttributeString("KeySortOrder", KeySortOrder.ToString());
			if (GroupSalts != q0.GroupSalts)
				tw.WriteAttributeString("GroupSalts", GroupSalts.ToString());

			if (AllowColumnMerging != q0.AllowColumnMerging)
				tw.WriteAttributeString("AllowColumnMerging", AllowColumnMerging.ToString());
			if (Preview != q0.Preview)
				tw.WriteAttributeString("Preview", Preview.ToString());
			if (SingleStepExecution != q0.SingleStepExecution)
				tw.WriteAttributeString("SingleStepExecution", SingleStepExecution.ToString());
			if (RunQueryWhenOpened != q0.RunQueryWhenOpened)
				tw.WriteAttributeString("RunQueryWhenOpened", RunQueryWhenOpened.ToString());
			if (BrowseSavedResultsUponOpen != q0.BrowseSavedResultsUponOpen)
				tw.WriteAttributeString("BrowseSavedResultsUponOpen", BrowseSavedResultsUponOpen.ToString());

			if (UseResultKeys != q0.UseResultKeys)
				tw.WriteAttributeString("UseResultKeys", UseResultKeys.ToString());
			if (ResultKeysListName != q0.ResultKeysListName)
				tw.WriteAttributeString("ResultKeysListName", ResultKeysListName.ToString());

			if (SerializeMetaTablesWithQuery != q0.SerializeMetaTablesWithQuery)
				tw.WriteAttributeString("SerializeMetaTablesWithQuery", SerializeMetaTablesWithQuery.ToString());

			if (SerializeResultsWithQuery != q0.SerializeResultsWithQuery)
				tw.WriteAttributeString("SerializeResultsWithQuery", SerializeResultsWithQuery.ToString());

			if (ResultsDataTableFileName != q0.ResultsDataTableFileName)
				tw.WriteAttributeString("ResultsDataTableFileName", ResultsDataTableFileName);

			if (WarningMessage != q0.WarningMessage)
				tw.WriteAttributeString("WarningMessage", WarningMessage);

			if (InitialBrowsePage != q0.InitialBrowsePage)
				tw.WriteAttributeString("InitialBrowsePage", InitialBrowsePage.ToString());

			if (Multitable != q0.Multitable)
				tw.WriteAttributeString("Multitable", Multitable.ToString());

			if (Mobile != q0.Mobile)
				tw.WriteAttributeString("Mobile", Mobile.ToString());

            //if (MobileDefault != q0.MobileDefault)
            //    tw.WriteAttributeString("MobileDefault", MobileDefault.ToString());

            if (DuplicateKeyValues != q0.DuplicateKeyValues)
				tw.WriteAttributeString("DuplicateKeyValues", DuplicateKeyValues.ToString());

			if (RepeatReport != q0.RepeatReport)
				tw.WriteAttributeString("RepeatReport", RepeatReport.ToString());

			if (ViewScale != q0.ViewScale)
				tw.WriteAttributeString("ViewScale", ViewScale.ToString());

			if (PrintScale != q0.PrintScale)
				tw.WriteAttributeString("PrintScale", PrintScale.ToString());

			if (PrintMargins != q0.PrintMargins)
				tw.WriteAttributeString("PrintMargins", PrintMargins.ToString());

			if (PrintOrientation != q0.PrintOrientation)
				tw.WriteAttributeString("PrintOrientation", PrintOrientation.ToString());

			if (ShowCondFormatLabels != q0.ShowCondFormatLabels)
				tw.WriteAttributeString("ShowCondFormatLabels", ShowCondFormatLabels.ToString());

			if (ShowStereoComments != q0.ShowStereoComments)
				tw.WriteAttributeString("ShowStereoComments", ShowStereoComments.ToString());

			if (Timeout != q0.Timeout)
				tw.WriteAttributeString("Timeout", Timeout.ToString());

			if (StatDisplayFormat != q0.StatDisplayFormat)
				tw.WriteAttributeString("StatDisplayFormat", ((int)StatDisplayFormat).ToString());

			//if (SpotfireTemplateName != q0.SpotfireTemplateName)
			//	tw.WriteAttributeString("SpotfireTemplateName", SpotfireTemplateName);

			//if (SpotfireTemplateContent != q0.SpotfireTemplateContent)
			//	tw.WriteAttributeString("SpotfireTemplateContent", SpotfireTemplateContent);

			//if (SpotfireDataFileName != q0.SpotfireDataFileName)
			//	tw.WriteAttributeString("SpotfireDataFile", SpotfireDataFileName);

			return;
		}

		/// <summary>
		/// Serialize a key list
		/// </summary>
		/// <param name="keyList"></param>
		/// <returns></returns>

		string SerializeKeyList(
			List<string> keyList)
		{
			StringBuilder sb = new StringBuilder();
			int keyCount = 0;
			foreach (string s in keyList)
			{
				if (s == "") continue;
				sb.Append(s + "\n");
				keyCount++;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Deserialize a key list
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		static List<string> DeserializeKeyList(
			XmlTextReader tr)
		{
			List<string> keyList = new List<string>();
			string txt = tr.ReadString();
			string[] sa = txt.Split('\n');
			for (int i1 = 0; i1 < sa.Length; i1++)
			{
				string key = sa[i1];
				if (key.IndexOf("\r") >= 0) key = key.Replace("\r", "");
				if (key.IndexOf(" ") >= 0) key = key.Replace(" ", "");
				if (key == "") continue;
				keyList.Add(key);
			}
			return keyList;
		}

	/// <summary>
	/// Serialize QueryTables
	/// </summary>
	/// <param name="tw"></param>

	void SerializeQueryTables(XmlTextWriter tw)
		{
			QueryTable qt;
			QueryColumn qc;
			int ti, ci;

			// Note that there is not currently a QueryTables xml element that wraps these

			QueryTable qt0 = new QueryTable(); // used to check for non-default value
			QueryColumn qc0 = new QueryColumn();

			for (ti = 0; ti < Tables.Count; ti++)
			{
				qt = Tables[ti];
				tw.WriteStartElement("QueryTable");
				tw.WriteAttributeString("MetaTable", qt.MetaTable.Name);
				//tw.WriteAttributeString("Summarized",qt.Summarized.ToString()); // obsolete
				//tw.WriteAttributeString("SummarizationType",qt.SummarizationType); // obsolete

				if (qt.Label != qt0.Label)
					tw.WriteAttributeString("Label", qt.Label);

				if (qt.Alias != qt0.Alias)
					tw.WriteAttributeString("Alias", qt.Alias);

				if (qt.AllowColumnMerging != qt0.AllowColumnMerging)
					tw.WriteAttributeString("AllowColumnMerging", qt.AllowColumnMerging.ToString());
				if (qt.HeaderBackgroundColor != qt0.HeaderBackgroundColor)
					tw.WriteAttributeString("HeaderBackgroundColor", qt.HeaderBackgroundColor.ToArgb().ToString());

				if (qt.VoPosition != qt0.VoPosition)
					tw.WriteAttributeString("VoPosition", qt.VoPosition.ToString());

				if (qt.AggregationEnabled != qt0.AggregationEnabled)
					tw.WriteAttributeString("Aggregate", qt.AggregationEnabled.ToString());

				for (ci = 0; ci < qt.QueryColumns.Count; ci++)
				{
					qc = qt.QueryColumns[ci];
					tw.WriteStartElement("QueryColumn");
					tw.WriteAttributeString("MetaColumn", qc.MetaColumn.Name);
					if (qc.Selected != qc0.Selected)
						tw.WriteAttributeString("Selected", qc.Selected.ToString());
					if (qc.Hidden != qc0.Hidden)
						tw.WriteAttributeString("Hidden", qc.Hidden.ToString());
					if (qc.Criteria != qc0.Criteria)
						tw.WriteAttributeString("Criteria", qc.Criteria);
					if (qc.CriteriaDisplay != qc0.CriteriaDisplay)
						tw.WriteAttributeString("CriteriaDisplay", qc.CriteriaDisplay);
					if (qc.FilterSearch != qc0.FilterSearch)
						tw.WriteAttributeString("FilterSearch", qc.FilterSearch.ToString());
					if (qc.FilterRetrieval != qc0.FilterRetrieval)
						tw.WriteAttributeString("FilterRetrieval", qc.FilterRetrieval.ToString());

					if (qc.SecondaryCriteria != qc0.SecondaryCriteria)
						tw.WriteAttributeString("SecondaryCriteria", qc.SecondaryCriteria);
					if (qc.SecondaryCriteriaDisplay != qc0.SecondaryCriteriaDisplay)
						tw.WriteAttributeString("SecondaryCriteriaDisplay", qc.SecondaryCriteriaDisplay);
					if (qc.SecondaryFilterType != qc0.SecondaryFilterType)
						tw.WriteAttributeString("SecondaryCriteriaType", qc.SecondaryFilterType.ToString());
					if (qc.ShowOnCriteriaForm != qc0.ShowOnCriteriaForm)
						tw.WriteAttributeString("ShowOnCriteriaForm", qc.ShowOnCriteriaForm.ToString());

					if (qc.SortOrder != qc0.SortOrder)
						tw.WriteAttributeString("SortOrder", qc.SortOrder.ToString());
					if (qc.Merge != qc0.Merge)
						tw.WriteAttributeString("Merge", qc.Merge.ToString());

					if (qc.MolString != qc0.MolString)
						tw.WriteAttributeString("QueryStructure", qc.MolString);

					if (qc.Label != qc0.Label)
						tw.WriteAttributeString("Label", qc.Label);

					if (qc.DisplayFormat != qc0.DisplayFormat)
						tw.WriteAttributeString("DisplayFormat", qc.DisplayFormat.ToString());
					if (qc.DisplayFormatString != qc0.DisplayFormatString)
						tw.WriteAttributeString("DisplayFormatString", qc.DisplayFormatString);

					if (qc.DisplayWidth != qc0.DisplayWidth)
						tw.WriteAttributeString("DisplayWidth", qc.DisplayWidth.ToString());
					if (qc.Decimals != qc0.Decimals)
						tw.WriteAttributeString("Decimals", qc.Decimals.ToString());

					if (qc.HorizontalAlignment != qc0.HorizontalAlignment)
						tw.WriteAttributeString("HorizontalAlignment", qc.HorizontalAlignment.ToString());

					if (qc.VerticalAlignment != qc0.VerticalAlignment)
						tw.WriteAttributeString("VerticalAlignment", qc.VerticalAlignment.ToString());

					// Additional non-attribute elements

					if (qc.VoPosition != qc0.VoPosition)
						tw.WriteAttributeString("VoPosition", qc.VoPosition.ToString());

					if (qc.CondFormat != null && !qc.CondFormatMatchesMetacolumnCf())
							qc.CondFormat.Serialize(tw); // serialize if defined and not the same as the MetaColumn CF

					if (qc.Aggregation != null)
						qc.Aggregation.Serialize(tw);

					tw.WriteEndElement(); // end of QueryColumn
				}
				tw.WriteEndElement(); // end of QueryTable
			}
			return;
		}

		/// <summary>
		/// Return the ordered list of sort columns
		/// </summary>
		/// <returns></returns>

		public List<SortColumn> GetSortColumns()
		{
			int ci;
			bool haveKey = false;

			List<SortColumn> sCols = new List<SortColumn>();
			foreach (QueryTable qt in Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.IsKey) // just get key once
					{
						if (haveKey) continue;
						qc.SortOrder = KeySortOrder; // use overall order
						haveKey = true;
					}

					if (qc.SortOrder == 0) continue;

					SortColumn sc = new SortColumn();
					sc.QueryColumn = qc;
					sc.Position = Math.Abs(qc.SortOrder);
					if (qc.SortOrder > 0) sc.Direction = SortOrder.Ascending;
					else sc.Direction = SortOrder.Descending;
					for (ci = sCols.Count - 1; ci >= 0; ci--)
						if (sCols[ci].Position < sc.Position) break;

					sCols.Insert(ci + 1, sc);
				}
			}
			return sCols;
		}

		/// <summary>
		/// Get list of named formatting for query sorted by name
		/// </summary>
		/// <returns></returns>

		public SortedDictionary<string, CondFormat> GetSortedCondFormats()
		{
			SortedDictionary<string, CondFormat> items = new SortedDictionary<string, CondFormat>();
			foreach (QueryTable qt in Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.CondFormat == null || qc.CondFormat.Name == "") continue;
					items[qc.CondFormat.Name.ToUpper()] = qc.CondFormat;
				}
			}

			return items;
		}

		/// <summary>
		/// Return true if query contains any conditional formatting
		/// </summary>

		public bool ContainsSelectedCondFormatting
		{
			get
			{
				List<QueryColumn> cfColList = GetSelectedCondFormatColumns();
				return cfColList.Count > 0;
			}
		}

		/// <summary>
		/// Get list of selected columns with conditional formatting
		/// </summary>
		/// <returns></returns>

		public List<QueryColumn> GetSelectedCondFormatColumns()
		{
			List<QueryColumn> cfColList = new List<QueryColumn>();
			foreach (QueryTable qt in Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.Selected && qc.CondFormat != null && qc.CondFormat.Rules.Count > 0)
						cfColList.Add(qc);
					else if (qc.Selected && qc.MetaColumn.MetaTable.Name == MultiDbAssayDataNames.CombinedNonSumTableName &&
					 (qc.MetaColumn.Name == "RSLT_VAL" || qc.MetaColumn.Name == "ACTIVITY_BIN")) // special case for general unpivoted view
						cfColList.Add(qc);
				}
			}

			return cfColList;
		}

		/// <summary>
		/// Get a conditional format by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public CondFormat GetCondFormatByName(
			string name)
		{
			foreach (QueryTable qt in Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.CondFormat != null && Lex.Eq(qc.CondFormat.Name, name)) return qc.CondFormat;
				}
			}

			return null;
		}

		/// <summary>
		/// Build secondary criteria filter string
		/// </summary>
		/// <returns></returns>

		public string BuildSecondaryFilterString()
		{
			string fs = "";
			foreach (QueryTable qt in Tables)
			{
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.SecondaryCriteria == "") continue;

					if (fs != "") fs += " and ";
					//					fs += "(" + qc.SecondaryCriteriaDisplay + ")";
					fs += qc.SecondaryCriteriaDisplay;
				}
			}
			return fs;
		}        

        /// <summary>
        /// Clone the query
        /// </summary>
        /// <returns></returns>

        public Query Clone()
		{
			string content = Serialize();
			Query clone = Deserialize(content);

			clone.InstanceId = InstanceIdCount; // be sure this clone has a new object InstanceId
			return clone;
		}

		/// <summary>
		/// TableListString - for debugging
		/// </summary>
		/// <returns></returns>

		public string TableListString
		{
			get
			{
				string s = "Tables: " + Tables.Count + ", Criteria: " + GetCriteriaCount(true, false) + "\r\n";
				for (int ti = 0; ti < Tables.Count; ti++)
					s += ti.ToString() + ". " + Tables[ti].MetaTable.Name + ", " + Tables[ti].MetaTable.Label + "\r\n";
				return s;
			}
		}


		/// <summary>
		/// Serialize - For debugging
		/// </summary>

		public string Serialized
		{
			get
			{
				return Serialize();
			}
		}

	} // end of Query

	/// <summary>
	/// Placeholder interface for QueryManager associated with a query 
	/// </summary>

	public interface IQueryManager
	{
		Query Query // reference to query
		{
			get;
			set;
		}

		void DisposeOfControls(); // clean up associated QM resources
	}

	/// <summary>
	/// Placeholder interface for QueryExec associated with a query 
	/// </summary>

	public interface IQueryExec
	{
		/// <summary>
		/// Prepare and run query that includes export options
		/// </summary>
		/// <param name="q"></param>
		/// <param name="ep"></param>
		/// <returns></returns>

		string RunQuery(
			Query q,
			ExportParms ep);
	}

	/// <summary>
	/// Placeholder interface for DataTableMx associated with a query 
	/// </summary>

	public interface IDataTableMx
	{
		// no members
	}

	/// <summary>
	/// Exception thrown for standard user query errors
	/// </summary>

	public class UserQueryException : Exception
	{
		public UserQueryException(string message) : base(message)
		{
		}

		public UserQueryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// Exception thrown for unexpected query errors or errors from user-generated MQL
	/// </summary>

	public class QueryException : Exception
	{
		public QueryException(string message)
			: base(message)
		{
		}

		public QueryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// Output destination for query
	/// </summary>

	public enum OutputDest
	{
		Unknown = 0,
		Search = 1,
		WinForms = 2,
		Html = 3,
		Console = 4,
		Excel = 5,
		Word = 6,
		TextFile = 7,
		SdFile = 8,
		Spotfire = 9
	}

	/// <summary>
	/// Type of (Windows) form in which the results will be displayed
	/// </summary>

	public enum OutputFormContext
	{
		Unknown = 0,
		Session = 1, // normal session form
		Popup = 2, // popup form
		Tool = 3 // tool form that contains both a query interface for the tool and the query results
	}

	/// <summary>
	/// Query Modes
	/// </summary>

	public enum QueryMode
	{
		Unknown = 0,
		Build = 1,
		Browse = 2
	}

	/// <summary>
	/// Type of logic to use in query or particular criteria or subset of criteria
	/// </summary>

	public enum QueryLogicType
	{
		Unknown = 0,
		And = 1,
		Or = 2,
		Complex = 3
	}

}
