using Mobius.ComOps;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Mobius.Data
{
	/// <summary>
	/// Summary description for QueryTable.
	/// </summary>

	public class QueryTable
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public MetaTable MetaTable = null; // associated metatable
		public Query Query; // associated query
		public List<QueryColumn> QueryColumns = new List<QueryColumn>(); // list of query column information
		public string Alias = ""; // table alias used to differentiate QueryTables (i.e. when same MetaTable is used multiple times in a Query) 
		public string Label = ""; // table label if renamed by user
		public bool HasDuplicateNames = false; // table has table or column names that exist elsewhere in the query
		public bool HasDuplicateLabels = false; // table has table or column labels that exist elsewhere in the query
		public bool AllowColumnMerging = true;
		public Color HeaderBackgroundColor = Color.Empty; // background color for table header line
		public int VoPosition = -1; // position within queryengine row where results for this QueryTable begin
		public int SpotfireExportPos = -1; // index assinged to this table when exported to Spotfire
		public bool AggregationEnabled = false; // true if data should be aggregated for this table if any columns have aggregation defined

		/// <summary>
		/// Basic constructor
		/// </summary>

		public QueryTable()
		{
			return;
		}

		/// <summary>
		/// Add new query table for the specified query
		/// </summary>
		/// <param name="q"></param>

		public QueryTable(Query q)
		{
			AddToQuery(q);
		}

		/// <summary>
		/// Initialize querytable from metatable
		/// </summary>
		/// <param name="mt"></param>

		public QueryTable(MetaTable mt)
		{
			InitForMetaTable(mt);
		}

		/// <summary>
		/// Add new query table for the specified query based on a metatable
		/// </summary>
		/// <param name="q"></param>
		/// <param name="mt"></param>

		public QueryTable(Query q, MetaTable mt)
		{
			InitForMetaTable(mt);
			AddToQuery(q);
		}

		/// <summary>
		/// Add a QueryTable to a Query linking each to the other
		/// </summary>
		/// <param name="q"></param>

		public void AddToQuery(Query q)
		{
			q.AddQueryTable(this);
		}

		/// <summary>
		/// Get position of this table within the Query table list
		/// </summary>

		public int TableIndex
		{
			get
			{
				if (Query == null) throw new Exception("Query table not in a query: " + MetaTableName);

				if (_tableIndex >= 0 && _tableIndex < Query.Tables.Count && Query.Tables[_tableIndex] == this &&
				 Query.Tables.Count == Query.IndexedTableCount)
					return _tableIndex;


				_tableIndex = Query.GetQueryTableIndex(this); // try alternative/slower method
				return _tableIndex;
			}

			set
			{
				_tableIndex = value;
				return;
			}
		}

		internal int _tableIndex = -1; // var with most-recent index assignment

		/// <summary>
		/// Position of this table within the Query table list (old)
		/// </summary>

		public int TableIndexOld
		{
			get
			{
				QueryTable qt;
				int ti, mti = -1;

				if (Query == null) return -1;

				for (ti = 0; ti < Query.Tables.Count; ti++)
				{
					qt = Query.Tables[ti];
					if (qt == this) return ti;
					if (Lex.IsDefined(qt.Alias) && qt.Alias == this.Alias) return ti;
					if (Lex.Eq(qt.MetaTableName, this.MetaTableName))
						mti = ti; // save any matching metatable name in case other matches fail
				}

				if (mti >= 0) return mti; // return match on metatable name
				else return -1; // not found
			}
		}


		/// <summary>
		/// Name of associated MetaTable
		/// </summary>

		public string MetaTableName
		{
			get
			{
				if (MetaTable != null) return MetaTable.Name;
				else return "";
			}
		}

		/// <summary>
		/// Remap a QueryTable to the current form of its associated MetaTable adding and removing columns as necessary
		/// </summary>

		public void RemapToMetaTable()
		{
			QueryColumn qc, qc2;

			MetaTable mt = MetaTableCollection.Get(MetaTable.Name); // get possibly-modified metatable
			if (mt == null) return;

			QueryTable qt2 = new QueryTable(mt); // make a temp QueryTable based on current form of MetaTable

			for (int qci = 0; qci < qt2.QueryColumns.Count; qci++)
			{
				qc2 = qt2.QueryColumns[qci];
				string mcName = qc2.MetaColumn.Name;
				qc = this.GetQueryColumnByName(mcName); // exist in old table?
				if (qc != null)
				{
					qt2.QueryColumns[qci] = qc; // use existing QueryColumn
					qc.MetaColumn = qc2.MetaColumn; // link to new MetaColumn
				}

				else qc2.QueryTable = this; // map added column to old QueryTable
			}

			this.MetaTable = mt; // point to new metatable
			this.QueryColumns = qt2.QueryColumns; // plug in new set of QueryColumns
			return;
		}

		/// <summary>
		/// Initialize QueryTable for specified MetaTable
		/// </summary>
		/// <param name="mt"></param>

		void InitForMetaTable(MetaTable mt)
		{
			MetaTable = mt;
			if (mt == null) throw new Exception("Null metatable");

			for (int i1 = 0; i1 < mt.MetaColumns.Count; i1++)
			{
				QueryColumn qc = new QueryColumn();
				qc.QueryTable = this;
				MetaColumn mc = mt.MetaColumns[i1];
				qc.MetaColumn = mc;

				if (mc.InitialSelection == ColumnSelectionEnum.Selected || // selected?
				 mc.IsKey) // always select key
					qc.Selected = true;

				//if (Lex.Eq(mc.Name, "primary_chain")) mc = mc; // debug

				if (Lex.IsDefined(mc.InitialCriteria))
				{
					string initialCriteria = mc.InitialCriteria;
					qc.FilterSearch = false; // don't use initial criteria to filter search but just display (default behavior)

					if (Lex.TryReplace(ref initialCriteria, "[NoFilterSearch]", "")) // don't use initial criteria to filter search
						qc.FilterSearch = false;

					if (Lex.TryReplace(ref initialCriteria, "[FilterSearch]", "")) // use initial criteria to filter search
						qc.FilterSearch = true;

					if (Lex.TryReplace(ref initialCriteria, "[NoFilterRetrieval]", "")) // don't use initial criteria to filter retrieval
						qc.FilterRetrieval = false;

					if (Lex.TryReplace(ref initialCriteria, "[FilterRetrieval]", "")) // don't use initial criteria to filter retrieval
						qc.FilterRetrieval = true;

					string delim = (initialCriteria.StartsWith(" ") ? "" : " ");
					qc.Criteria = mc.Name + delim + initialCriteria;
					qc.CriteriaDisplay = initialCriteria;
				}

				QueryColumns.Add(qc);
			}
			return;
		}

		/// <summary>
		/// Clone QueryTable
		/// </summary>
		/// <returns></returns>

		public QueryTable Clone()
		{
			QueryTable qt;
			QueryColumn qc;

			qt = (QueryTable)this.MemberwiseClone();
			qt._keyQueryColumn = null; // clear since new col will be key
			qt._tableIndex = -1; 

			qt.Id = InstanceCount++; // assign unique id to clone
			qt.QueryColumns = new List<QueryColumn>(); // duplicate columns also
			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				qc = (this.QueryColumns[i1]).Clone();
				qc.QueryTable = qt; // point to this query table
				qt.QueryColumns.Add(qc);
			}

			return qt;
		}

		/// <summary>
		/// Select default columns in query table
		/// </summary>

		public void SelectDefault()
		{
			DeselectAll();
			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				QueryColumn qc = (this.QueryColumns[i1]);
				if (qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Selected)
					qc.Selected = true;
			}

			return;
		}

		/// <summary>
		/// Select all non-hidden columns in query table
		/// </summary>

		public void SelectAllNonHidden()
		{
			DeselectAll();
			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				QueryColumn qc = (this.QueryColumns[i1]);
				if (qc.MetaColumn == null) continue;
				if (qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Selected ||
					qc.MetaColumn.InitialSelection == ColumnSelectionEnum.Unselected)
					qc.Selected = true;
			}

			return;
		}

		/// <summary>
		/// Select all columns in query table
		/// </summary>

		public void SelectAll()
		{
			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				QueryColumn qc = (this.QueryColumns[i1]);
				qc.Selected = true;
			}

			return;
		}

		/// <summary>
		/// Deselect all columns in query table
		/// </summary>

		public void DeselectAll()
		{
			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				QueryColumn qc = (this.QueryColumns[i1]);
				qc.Selected = false;
			}

			return;
		}

		/// <summary>
		/// Select a list of columns
		/// </summary>
		/// <param name="colList"></param>

		public void SelectColumns(
			string colList)
		{
			QueryColumn qc;

			Lex lex = new Lex();
			lex.OpenString(colList);

			DeselectAll();
			qc = QueryColumns[0]; // first column must stay first
			qc.Selected = true;

			while (true)
			{
				string colName = lex.Get();
				if (colName == "") break;
				if (colName == ",") continue;
				Select(colName);
			}

			return;
		}

		/// <summary>
		/// Select specified col by name
		/// </summary>
		/// <param name="colName"></param>

		public QueryColumn Select(string colName)
		{
			return Select(colName, true);
		}

		/// <summary>
		/// Deselect specified col by name
		/// </summary>
		/// <param name="colName"></param>

		public QueryColumn Deselect(string colName)
		{
			return Select(colName, true);
		}

		/// <summary>
		/// Set column selection by name
		/// </summary>
		/// <param name="colName"></param>

		public QueryColumn Select(
			string colName,
			bool selected)
		{
			QueryColumn qc = GetQueryColumnByNameWithException(colName);
			qc.Selected = selected;
			return qc;
		}

		/// <summary>
		/// Select a list of columns in the specified order
		/// </summary>
		/// <param name="colList"></param>

		public void SelectColumnsInOrder(
			string colList)
		{
			QueryColumn qc;

			Lex lex = new Lex();
			lex.OpenString(colList);

			DeselectAll();

			List<QueryColumn> qcl2 = new List<QueryColumn>();
			qc = QueryColumns[0]; // first column must stay first
			qc.Selected = true;
			qcl2.Add(qc);

			while (true)
			{
				string tok = lex.Get();
				if (tok == "") break;
				if (tok == ",") continue;
				qc = GetQueryColumnByName(tok);
				if (qc == null) throw new Exception("Column not found: " + tok);
				if (qc.Selected) continue;
				qc.Selected = true;
				qcl2.Add(qc);
			}

			foreach (QueryColumn qc_ in QueryColumns)
			{
				if (!qc_.Selected) qcl2.Add(qc_);
			}

			QueryColumns = qcl2;
			return;
		}

		/// <summary>
		/// Select key column only in query table
		/// </summary>
		public void SelectKeyOnly()
		{
			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				QueryColumn qc = QueryColumns[i1];
				if (qc.MetaColumn != null && qc.IsKey) qc.Selected = true;
				else qc.Selected = false;
			}

			return;
		}

		/// <summary>
		/// Return the key querycolumn for this querytable
		/// </summary>

		public QueryColumn KeyQueryColumn
		{
			get
			{
				QueryColumn qc;

				if (_keyQueryColumn != null) return _keyQueryColumn;

				for (int ci = 0; ci < QueryColumns.Count; ci++)
				{
					qc = QueryColumns[ci];
					if (qc.MetaColumn == null) continue;
					if (qc.IsKey)
					{
						_keyQueryColumn = qc;
						return qc; // just compound number for now
					}
				}

				return null;
			}
		}

		private QueryColumn _keyQueryColumn = null; // set first time retrieved for faster subsequent retrieval

		/// <summary>
		/// Get first structure QueryColumn if any
		/// </summary>

		public QueryColumn FirstStructureQueryColumn
		{
			get
			{
				MetaColumn mc = MetaTable.FirstStructureMetaColumn;
				if (mc == null) return null;
				QueryColumn qc = GetQueryColumnByName(mc.Name);
				return qc;
			}
		}

		/// <summary>
		/// Get the first QueryColumn with the specified attribute values
		/// </summary>
		/// <param name="columnType"></param>
		/// <param name="mustBeSelected"></param>
		/// <param name="mustHaveCriteria"></param>
		/// <param name="mustBeSorted"></param>
		/// <returns></returns>

		public QueryColumn GetFirsMatchingQueryColumn(
			MetaColumnType columnType = MetaColumnType.Any,
			bool mustBeSelected = false,
			bool mustHaveCriteria = false,
			bool mustBeSorted = false,
			bool mustBePrimaryResult = false,
			bool mustBeSecondaryResult = false,
			bool mustBeNumeric = false)
		{
			QueryColumn qc;
			MetaColumn mc;

			MetaTable mt = MetaTable;
			if (mt == null) return null;

			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				qc = this.QueryColumns[i1];
				mc = qc.MetaColumn;
				if (mc == null) continue;

				if (columnType != MetaColumnType.Any && columnType != mc.DataType) continue;

				if (mustBeSelected && !qc.Selected) continue;

				if (mustHaveCriteria && (Lex.IsUndefined(qc.Criteria) || mt.IgnoreCriteria)) continue;

				if (mustBeSorted && qc.SortOrder == 0) continue;

				if (mustBePrimaryResult && !qc.MetaColumn.PrimaryResult) continue;

				if (mustBeSecondaryResult && !qc.MetaColumn.SecondaryResult) continue;

				if (mustBeNumeric && !MetaColumn.IsNumericMetaColumnType(qc.MetaColumn.DataType)) continue;

				return qc;
			}

			return null; // no luck
		}

		/// <summary>
		/// Lookup a query column by the underlying metacolumn name and return if selected
		/// </summary>
		/// <param name="colNameList"></param>
		/// <returns></returns>

		public QueryColumn GetSelectedQueryColumnByName(params string[] colNameList)
		{
			foreach (string colName in colNameList)
			{
				QueryColumn qc = GetQueryColumnByName(colName);
				if (qc != null && qc.Selected) return qc;
			}

			return null;
		}

		/// <summary>
		/// Lookup a query column by the underlying metacolumn name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public QueryColumn GetQueryColumnByName(
			string name)
		{
			QueryColumn qc;
			MetaColumn mc;

			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				qc = this.QueryColumns[i1];
				if (qc.MetaColumn == null) continue;

				mc = qc.MetaColumn;
				if (Lex.Eq(mc.Name, name))
					return qc;
			}

			return null;
		}

		/// <summary>
		/// Lookup a query column by the underlying metacolumn name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public QueryColumn GetQueryColumnByNameWithException(
			string name)
		{
			QueryColumn qc = GetQueryColumnByName(name);
			if (qc != null) return qc;
			else throw new Exception("Query column not found: " + name);
		}

		/// <summary>
		/// Lookup a queryColumn by name and return it's index
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetQueryColumnIndexByName(
			string name)
		{
			QueryColumn qc;
			MetaColumn mc;

			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				qc = this.QueryColumns[i1];
				if (qc.MetaColumn == null) continue;

				mc = qc.MetaColumn;
				if (Lex.Eq(mc.Name, name))
					return i1;
			}
			return -1;
		}

		public QueryColumn GetQueryColumnByLabel(
			string label)
		{
			QueryColumn qc, mcMatchQc = null;
			MetaColumn mc; 


			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				qc = this.QueryColumns[i1];
				mc = qc.MetaColumn;
				if (mc == null) continue;
				if (Lex.Eq(qc.Label, label))
					return qc;

				if (Lex.Eq(mc.Label, label))
					mcMatchQc = qc;
			}

			if (mcMatchQc != null) return mcMatchQc;

			// If no luck try active label and MetaColumn name match

			for (int i1 = 0; i1 < this.QueryColumns.Count; i1++)
			{
				qc = this.QueryColumns[i1];
				mc = qc.MetaColumn;
				if (mc == null) continue;
				if (Lex.Eq(qc.ActiveLabel, label))
					return qc;

				if (Lex.Eq(mc.Name, label)) // try mcName
					mcMatchQc = qc;
			}

			if (mcMatchQc != null) return mcMatchQc;

			return null;
		}


		/// <summary>
		/// Get a count of visible columns (i.e. non-hidden cols and hidden
		/// </summary>

		public int VisibleColumnCount
		{
			get
			{
				int visibleCount = 0;
				foreach (QueryColumn qc in QueryColumns)
				{
					if (qc.MetaColumn.InitialSelection != ColumnSelectionEnum.Hidden ||
					qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) visibleCount++;
				}

				return visibleCount;
			}
		}

		/// <summary>
		/// Return number of columns selected
		/// </summary>

		public int SelectedCount
		{
			get
			{
				int selectCount = 0;
				foreach (QueryColumn qc in QueryColumns)
				{
					if (qc.Selected) selectCount++;
				}

				return selectCount;
			}
		}

		/// <summary>
		/// Return a list of selected QueryColumns
		/// </summary>

		public List<QueryColumn> SelectedColumns
		{
			get
			{
				List<QueryColumn> cols = new List<QueryColumn>();
				foreach (QueryColumn qc in QueryColumns)
				{
					if (qc.Selected) cols.Add(qc);
				}

				return cols;
			}
		}

		/// <summary>
		/// Return a list of selected QueryColumn indexes
		/// </summary>

		public List<int> SelectedIndexes
		{
			get
			{
				List<int> cols = new List<int>();
				for (int qci = 0; qci < QueryColumns.Count; qci++)
				{
					QueryColumn qc = QueryColumns[qci];
					if (qc.Selected) cols.Add(qci);
				}

				return cols;
			}
		}


		/// <summary>
		/// Get criteria count excluding key criteria
		/// </summary>

		public int CriteriaCount
		{
			get
			{
				return GetCriteriaCount(includeKey: false, includeDbSet: true);
			}
		}

		/// <summary>
		/// Get count criteria
		/// </summary>
		/// <returns></returns>

		public int GetCriteriaCount(
			bool includeKey,
			bool includeDbSet)
		{
			if (this.MetaTable.IgnoreCriteria) return 0;

			int criteriaCount = 0;
			foreach (QueryColumn qc in QueryColumns)
			{
				if (qc.Criteria == "") continue;

				if (qc.MetaColumn == null) continue;

				if (!includeKey && qc.IsKey)
					continue;

				if (!includeDbSet && qc.MetaColumn.IsDatabaseSetColumn)
					continue;

				criteriaCount++;
			}

			return criteriaCount;
		}

		/// <summary>
		/// HasDateCriteria
		/// </summary>
		/// <returns></returns>

		public bool HasDateCriteria
		{
			get
			{
				if (MetaTable.IgnoreCriteria) return false;

				foreach (QueryColumn qc in QueryColumns)
				{
					if (qc.Criteria == "") continue;

					if (qc.MetaColumn == null) continue;

					if (qc.IsKey)
						continue;

					if (qc.MetaColumn.IsDate) return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Return true if table has "normal" database searching criteria other than key and not-exists criteria
		/// </summary>

		public bool HasNormalDatabaseCriteria
		{
			get
			{
				if (MetaTable.IgnoreCriteria) return false;

				foreach (QueryColumn qc in QueryColumns)
				{
					if (Lex.IsUndefined(qc.Criteria)) continue;

					if (qc.IsKey) continue;

					if (qc.MetaColumn == null) continue;

					if (!qc.MetaColumn.IsSearchable) continue;

					if (qc.MetaColumn.IsDatabaseSetColumn) continue;

					if (!qc.HasNotExistsCriteria) return true;
				}

				return false;
			}
		}


		/// <summary>
		/// Return true if table has non-exists criteria on it.
		/// </summary>

		public bool HasNotExistsCriteria
		{
			get
			{
				if (MetaTable.IgnoreCriteria) return false;

				foreach (QueryColumn qc in QueryColumns)
				{
					if (Lex.IsUndefined(qc.Criteria)) continue;

					if (qc.IsKey) continue;

					if (qc.MetaColumn == null) continue;

					if (!qc.MetaColumn.IsSearchable) continue;

					if (qc.MetaColumn.IsDatabaseSetColumn) continue;

					if (qc.HasNotExistsCriteria) return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Return true if aggreation is enabled for the table and at lease one col defines an aggregation type
		/// </summary>

		public bool IsAggregated
		{
			get
			{
				return AggregationEnabled && AggregatedCount > 0;
			}
		}

		/// <summary>
		/// Count of columns with aggregation types assigned
		/// </summary>

		public int AggregatedCount
		{
			get
			{
				int aggCount;
				bool keyIsAggregated;
				AnalyzeAggregation(out aggCount, out keyIsAggregated);
				return aggCount;
			}
		}

/// <summary>
/// Return true if table is aggregated and the aggregation includes the key col
/// </summary>

		public bool IsKeyAggregation
		{
			get
			{
				if (!AggregationEnabled) return false;

				int aggCount;
				bool keyIsAggregated;
				AnalyzeAggregation(out aggCount, out keyIsAggregated);
				return keyIsAggregated;
			}
		}

/// <summary>
/// Return true if table is aggregated but not by key
/// </summary>
		public bool IsNonKeyAggregation
		{
			get
			{
				if (!AggregationEnabled) return false;

				int aggCount;
				bool keyIsAggregated;
				AnalyzeAggregation(out aggCount, out keyIsAggregated);
				return aggCount > 0 && !keyIsAggregated;
			}
		}


		/// <summary>
		/// AnalyzeAggregation
		/// </summary>
		/// <param name="aggCount"></param>
		/// <param name="keyIsAggregated"></param>
		/// <returns></returns>

		public bool AnalyzeAggregation(
			out int aggCount,
			out bool keyIsAggregated)
		{
			aggCount = 0;
			keyIsAggregated = false;
			foreach (QueryColumn qc in QueryColumns)
			{
				if (qc.IsAggregated)
				{
					aggCount++;
					if (qc.IsKey && qc.IsAggregationGroupBy) keyIsAggregated = true;
				}
			}

			return AggregationEnabled;
		}

		/// <summary>
		/// Return true if named column is selected, has criteria on it or has sorting on it
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public bool Column_Selected_or_Criteria_or_Sorted (
			string name) 
		{
			QueryColumn qc = GetQueryColumnByName(name);
			if (qc==null) return false;
			if (qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) 
				return true;
			else return false;
		}

/// <summary>
/// Add a QueryColumn to the QueryTable
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public QueryColumn AddQueryColumn (
			QueryColumn qc)
		{
			QueryColumns.Add(qc);
			qc.QueryTable = this;
			return qc;
		}

/// <summary>
/// Add a QueryColumn for the named MetaColumn
/// </summary>
/// <param name="name"></param>
/// <param name="selected"></param>
/// <returns></returns>

		public QueryColumn AddQueryColumnByName (
			string metaColumnName,
			bool selected) 
		{
			QueryColumn qc;
			MetaColumn mc;

			if (this.MetaTable==null) return null;

			mc = this.MetaTable.GetMetaColumnByName(metaColumnName);
			if (mc==null) return null;

			qc = this.GetQueryColumnByName(metaColumnName);
			if (qc == null) // create if don't already have
			{
				qc = new QueryColumn();
				qc.QueryTable = this;
				qc.MetaColumn = mc;
				this.QueryColumns.Add(qc);
			}
			qc.Selected=selected; 
			return qc;
		}

/// <summary>
/// Get a QueryTable for the default root MetaTabler
/// </summary>
/// <returns></returns>

		public static QueryTable GetDefaultRootQueryTable()
		{
			MetaTable mt = MetaTableCollection.GetDefaultRootMetaTable();
			AssertMx.IsNotNull(mt, "DefaultRootMetaTable");
			QueryTable qt = new QueryTable(mt);
			return qt;
		}

		/// <summary>
		/// Clone query table & map to proper metatable based on rootTable
		/// </summary>
		/// <param name="rootTable"></param>
		/// <returns></returns>

		public QueryTable MapToCurrentDb (
			MetaTable rootTable)
		{
			QueryTable qt = this;
			MetaTable mt = this.MetaTable;
			MetaColumn keyMc = mt.KeyMetaColumn;

			QueryTable qt2 = this.Clone();
			MetaTable mt2 = mt.MapToCurrentDb(rootTable); // reassign as needed based on current root
			if (mt2==null) return null;

			MetaColumn keyMc2 = mt2.KeyMetaColumn;
			if (mt2 == mt) return qt2; // if no change in metatable return as is

			qt2.MetaTable = mt2; // plug in metatable 
			foreach (QueryColumn qc2 in qt2.QueryColumns) // remap columns, assume name correspondence except for key
			{
				MetaColumn mc = qc2.MetaColumn;
				if (mc == keyMc) qc2.MetaColumn=keyMc2; // remap key
				else // remap others by name
				{
					MetaColumn mc2 = mt2.GetMetaColumnByName(mc.Name);
					if (mc2!=null) qc2.MetaColumn = mc2;
					else qc2.MetaColumn = null; // shouldn't happen
				}
			}

			return qt2;
		}

/// <summary>
/// Get list of databases from any database column in the table
/// </summary>
/// <returns></returns>

		public List<string> GetDatabaseList()
		{
			QueryColumn qc = GetDatabaseListQueryColumn();
			if (qc == null || Lex.IsUndefined(qc.Criteria)) return null;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria); // parse criteria
			return psc.ValueList;
		}

		public QueryColumn GetDatabaseListQueryColumn()
		{
			MetaColumn mc = MetaTable.DatabaseListMetaColumn;
			if (mc == null) return null;

			QueryColumn qc = GetQueryColumnByName(mc.Name);
			return qc;
		}

		public QueryColumn GetSimilarityScoreQueryColumn()
		{
			MetaColumn mc = MetaTable.SimilarityScoreMetaColumn;
			if (mc != null) return GetQueryColumnByName(mc.Name);
			else return null;
		}

		/// <summary>
		/// Return any structure search QueryColumn in the table in parsed format
		/// </summary>
		/// <returns></returns>

		public ParsedStructureCriteria GetParsedStructureSearchCriteria()
		{
			ParsedStructureCriteria pssc = null;

			QueryColumn qc = GetQueryColumnWithStructureSearchCriteria();
			if (qc == null) return null;

			bool parseOk = ParsedStructureCriteria.TryParse(qc, out pssc);
			if (parseOk) return pssc;
			else return null;
		}

		/// <summary>
		/// Return any structure search QueryColumn in the table
		/// </summary>
		/// <returns></returns>

		public QueryColumn GetQueryColumnWithStructureSearchCriteria ()
		{
			foreach (QueryColumn qc in this.QueryColumns)
			{
				if (qc.MetaColumn == null) continue;
				if (qc.MetaColumn.DataType != MetaColumnType.Structure) continue;
				if (Lex.IsUndefined(qc.Criteria)) continue; // || Lex.Eq(qc.Criteria,"Complex")) continue; // (include complex)
				return qc;
			}

			return null;
		}

		/// <summary>
		/// Get any query column that retrieves a molsim score
		/// </summary>
		/// <returns></returns>

		public QueryColumn GetMolSimScoreQueryColumn()
		{
			foreach (QueryColumn qc in this.QueryColumns)
			{
				if (qc.MetaColumn == null) continue;
				if (Lex.Eq(qc.MetaColumn.DataTransform, "GetMolsim")) return qc;
			}

			return null;
		}

		/// <summary>
		/// Return any selected similarity score column
		/// </summary>
		/// <returns></returns>

		public QueryColumn GetSelectedMolsimQueryColumn ()
		{
			foreach (QueryColumn qc in this.QueryColumns)
			{
				if (qc.MetaColumn == null) continue;
				if (qc.Selected && Lex.Eq(qc.MetaColumn.DataTransform, "GetMolsim")) return qc;
			}

			return null;
		}

/// <summary>
/// Get label from query table or from MetaTable if no query table label
/// </summary>

		public string ActiveLabel
		{
			get
			{
				return GetActiveLabel(includeAggregationEnabledSuffix: true);
			}
		}

		/// <summary>
		/// Get label from query table or from MetaTable if no query table label
		/// Optionionally include "Summary" suffix if aggregation is enabled
		/// </summary>

		public string GetActiveLabel(
			bool includeAggregationEnabledSuffix)
		{
			string label = "";
			if (!Lex.IsNullOrEmpty(Label)) label = Label;
			else if (MetaTable != null)
			{
				if (!Lex.IsNullOrEmpty(MetaTable.Label))
					label = MetaTable.Label;
				else label = Lex.InternalNameToLabel(MetaTable.Name);
			}

			if (includeAggregationEnabledSuffix && AggregationEnabled && !Lex.Contains(label, " Summary"))
				label += " Summary";

			return label;
		}

		/// <summary>
		/// Get QueryColumn reference string in the form [Alias.]MetatableName.MetaColumn name
		/// </summary>
		/// <returns></returns>

		public string GetQueryTableRefString()
		{
			string s = "";

			if (MetaTable == null) return "";

			if (Query?.Tables == null) return "";

			if (Lex.IsDefined(Alias))
				s += Alias + ".";

			s += MetaTable.Name;

			return s;
		}

		/// <summary>
		/// Get a QueryTable for a query from a QT reference string
		/// </summary>
		/// <param name="q"></param>
		/// <param name="qtRef"></param>
		/// <returns></returns>

		public static QueryTable GetQueryTableFromRefString(
			Query q,
			string qtRef)
		{
			QueryTable qt = null;
			QueryColumn qc = null;
			string alias, mtName, mcName;


			if (Lex.IsUndefined(qtRef)) return null;

			string[] sa = qtRef.Split('.');

			if (sa.Length == 2)
			{
				sa = Lex.Split(qtRef, ".");
				alias = sa[0];
				mtName = sa[1];

				qt = q.GetQueryTableByAlias(alias);
			}

			else if (sa.Length == 1)
			{
				mtName = sa[0];
			}

			else return null;

			if (qt == null) qt = q.GetQueryTableByName(mtName);

			if (qt != null) return qt;
			else return null;
		}

		/// <summary>
		/// Adjust the QueryTable label suffix so that it is unique within the query
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		public void AssignUniqueQueryTableLabel(
			Query q = null)
		{
			QueryTable qt2;
			int ti;

			if (q == null) q = this.Query;
			if (q == null) throw new Exception("Query not defined");

			string label = ActiveLabel;

			while (true)
			{
				for (ti = 0; ti < q.Tables.Count; ti++) // see if any table label matches
				{
					qt2 = q.Tables[ti];
					if (qt2 == this) continue; // ignore incoming table

					if (label == qt2.ActiveLabel) break;
				}

				if (ti >= q.Tables.Count) break; // break if no match found

				label = Lex.IncrementIntegerSuffix(label);
			}

			if (AggregationEnabled && Lex.Contains(label, " Summary"))
				label = Lex.Replace(label, " Summary", "");

			if (MetaTable == null || Lex.Ne(label, MetaTable.Label))
				this.Label = label;

			return;
		}

		/// <summary>
		/// Get name for table that is unique within the query
		/// </summary>

		public string UniqueName
		{
			get
			{
				if (Query == null) return MetaTable.Name;

				if (!Query.DuplicateNamesAndLabelsMarked)
					Query.MarkDuplicateNamesAndLabels();

				string name = MetaTable.Name;
				if (HasDuplicateNames)
					name = Alias + "_" + name;

				return name;
			}
		}

		/// <summary>
		/// Get label for table that is unique within the query
		/// </summary>

		public string UniqueLabel
		{
			get
			{
				if (Query == null) return ActiveLabel;

				if (!Query.DuplicateNamesAndLabelsMarked)
					Query.MarkDuplicateNamesAndLabels();

				string label = ActiveLabel;
				if (HasDuplicateNames)
					label = Alias + "." + label;

				return label;
			}
		}

/// <summary>
/// Create a new QueryTable with the desired summarization level from an existing table
/// </summary>
/// <param name="useSummarized"></param>
/// <returns></returns>

		public QueryTable AdjustSummarizationLevel(
			bool useSummarized)
		{
			QueryTable qt, qt2;
			QueryColumn qc, qc2;
			string tName2;
			MetaTable mt2;

			qt = this;
			if (qt.MetaTable.UseSummarizedData == useSummarized)  // already matches
			{
				qt2 = this.Clone();
				return qt2;
			}

			if (useSummarized) tName2 = qt.MetaTable.Name + MetaTable.SummarySuffix; // going to summarized table
			else tName2 = qt.MetaTable.Name.Substring(0, qt.MetaTable.Name.Length - MetaTable.SummarySuffix.Length); // going to unsummarized

			mt2 = MetaTableCollection.Get(tName2);
			if (mt2 == null) throw new Exception("Metatable not found: " + tName2);
			qt2 = new QueryTable(mt2);
			for (int qci = 0; qci < qt2.QueryColumns.Count; qci++)
			{ // copy any selections & criteria from old query table if column present in new table
				qc = qt.GetQueryColumnByName(qt2.QueryColumns[qci].MetaColumn.Name);
				if (qc != null)
				{
					qc2 = qc.Clone();
					qc2.MetaColumn = mt2.GetMetaColumnByName(qc2.MetaColumn.Name); // get metacolumn associated with summary table
					qc2.QueryTable = qt2;
					qt2.QueryColumns[qci] = qc2;
				}
			}

			return qt2;
		}

		/// <summary>
		/// Set simple vo positions for selected QueryColumns starting at offset value
		/// </summary>

		public int SetSimpleVoPositions(
			int voOffset = 0)
		{
			int voLength = 0;
			for (int ci = 0; ci < QueryColumns.Count; ci++)
			{
				QueryColumn qc = QueryColumns[ci];
				if (qc.Is_Selected_or_GroupBy_or_Sorted) // only selected, grouped & sorted fields
				{
					qc.VoPosition = voLength + voOffset;
					voLength++;
				}
				else qc.VoPosition = -1;
			}

			return voLength; // return vo length
		}

/// <summary>
/// Return value indicating if Netezza can be used for this QueryTable
/// </summary>

		public bool AllowNetezzaUse 
		{
			get
			{
				QueryColumn qc = GetQueryColumnWithStructureSearchCriteria();
				if (qc != null) 
					return false; // Netezza doesn't support structure searching

				else return MetaTable.AllowNetezzaUse; // return value for MetaTable
			}
		}

/// <summary>
/// ToString for debug
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			string txt = "QueryTable for MetaTable: " + MetaTable.Name + ", Alias: " + Alias + "\r\n";
			txt += "MetaColumn\tSelected\tCriteria\tSortOrder\tVoPosition\r\n";
			for (int qci = 0; qci < QueryColumns.Count; qci++)
			{
				QueryColumn qc = QueryColumns[qci];
				if (qc.MetaColumn == null) continue;
				txt += qc.MetaColumn.Name + "\t" + qc.Selected + "\t" + qc.Criteria + "\t" + qc.SortOrder + "\t" + qc.VoPosition + "\r\n";
			}

			return txt;
		}
	}

}
