using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Data;
using System.Data.Common;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// Base class for metabrokers
	/// </summary>

	public partial class GenericMetaBroker : IMetaBroker 
	{
		// Class variables common between brokers

		internal int Id; // sequential Id assigned to queries 
		internal string Label = ""; // optional descriptive label for broker
		internal ExecuteQueryParms Eqp;
		internal QueryTable Qt; // query table for this broker
		internal MetaTable Mt { get { return Qt != null ? Qt.MetaTable : null; } } // associated  metatable
		internal Query Query { get { return Qt != null ? Qt.Query : null; } } // associated  Query
		internal QueryEngine Qe { get { return Eqp != null ? Eqp.Qe : null; } } // associated  QueryEngine 

		public List<MetaColumn> SelectList = null; // selected metacolumn objects
		internal int KeyQci = -1; // index of query col contain key field (-1 if undefined)
		internal MetaColumn KeyMc = null; // key metacolumn
		internal QueryColumn KeyQc = null; // key query column
		internal int KeyVoi = -1; // key index in vo
		internal string Exprs; // expressions to select
		internal string FromClause; // tables to be selected from
		internal string Criteria; // search criteria
		internal string OrderBy; // order clause
		internal string Sql; // sql statement (without key subset phrase)

		internal DbCommandMx DrDao;
		internal DbDataReader Dr;
		internal int FirstKeyIdx = 0; // index of the first key in the current chunk of keys
		internal int LastKeyIdx = 0; // index of the last key in the current chunk of keys
		internal int LastKeyCount = -1;
		internal bool BuildSqlOnly { get { if (Qe != null) return Qe.BuildSqlOnly; else return false; } } // just building Sql if true

		internal KeyListPredTypeEnum KeyListPredType = DbCommandMx.DefaultKeyListPredType;
		internal bool KeyListPredTypeParameterized { get { return KeyListPredType == KeyListPredTypeEnum.Parameterized; } }
		internal bool KeyListPredTypeLiteral { get { return KeyListPredType == KeyListPredTypeEnum.Literal; } }
		internal bool KeyListPredTypeDbTable { get { return KeyListPredType == KeyListPredTypeEnum.DbList; } }

		internal bool SingleKeyRetrieval = false; // if true retrieve keys singly for improved performance
		internal int KeyCount = 0; // number of keys in current chunk
		internal object [] KeyParmArray; // array containing parameter values to bind

		public static int RetrievalTimeoutCount; // number of timeouts on retrieval
		public static int SingleKeyRetrievalCount; // number of single key retrievals
		public static int MultipleKeyRetrievalCount; // number of multiple key retrievals
		public static int NonKeyRetrievalCount; // number of non-key retrievals

		public int ExecuteReaderCount = 0; // number of times ExecuteReader called by broker
		public double ExecuteReaderTime = 0;

		public int ReadRowCount = 0; // DbCommand Read operations (partial or full NextRow)
		public int ReadRowsFilteredByKeyExclusionList = 0; // number of low level rows filtered out via key exclusion list
		public double ReadRowTime = 0;

		public int NextRowCount = 0; // Number of QueryTable rows for selected columns returned by broker
		public double NextRowTime = 0; 

		double GetStructureTime = 0; 

		public bool UseOracleSource // use Oracle source for this broker
		{ get { return !UseNetezzaSource; } }

		public bool UseNetezzaSource  // Netezza is enabled for this broker instance
		{ get { return GetUseNetezzaSource(); }
			set { _allowNetezzaUse = value; }}
		bool _allowNetezzaUse = true; // allow by default if other conditions hold

		internal bool AllowMultiTablePivot = true; // allow simultaneous pivoting of multiple tables at a time

		internal bool PivotInCode = false; // if true retrieve unpivoted data and then pivot in code

		internal string MpGroupKey = ""; // key for tables in multi-table pivot group
		internal Dictionary<string, object[]> MultipivotRowDict; // buffered row vo's of data for this broker keyed by cid & possibly other row id cols
		internal List<object[]> MultipivotRowList; // buffered retrieved rows of data for this broker in optional list form

		static int BrokerIdSeq = 0; // sequence used to assign broker ids
		static int SubqueryIdSeq = 0; // sequence used to assign subquery ids


		// Class variables used only be GenericMetaBroker

		internal MoleculeMetaBroker MoleculeMetaBroker; // associated broker for special structure searches
		bool RootTableKeyOnlyQuery = false; // if true just return keys from root table without going to oracle for better performance

		// Constructor

		public GenericMetaBroker()
		{
			Id = BrokerIdSeq;
			BrokerIdSeq++;

			KeyListPredType = DbCommandMx.DefaultKeyListPredType; // default key list predicate form to QE default
		}

		/// <summary>
		/// Get the name of the SQL statement group that this table should be included in for the purposes of executing searches
		/// This base-class method returns the name of the root schema associated with the metatable
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public virtual string GetTableCriteriaGroupName(
			QueryTable qt)
		{
			DataSourceMx rootSource;
			DbSchemaMx rootSchema;

			MetaTable mt = qt.MetaTable;

			Dictionary<string, Dictionary<string, DbSchemaMx>> sourceDict =
				DbConnectionMx.GetDataSources(mt.TableMap, out rootSource, out rootSchema);

			if (rootSchema == null) return "";
			else return rootSchema.Name;
		}

/// <summary>
/// Check if use of Netezza is allowed at all levels
/// </summary>
/// <returns></returns>
		
			bool GetUseNetezzaSource()
			{
				if (!QueryEngine.AllowNetezzaUse) return false; // Global QE level
				if (Qt == null || Qt.Query == null || !Qt.Query.AllowNetezzaUse) return false; // query level
				if (!Qt.MetaTable.AllowNetezzaUse) return false; // metatable level
				if (Eqp == null || !Eqp.AllowNetezzaUse) return false; // Eqp/broker instance level
				return _allowNetezzaUse; 
			}

/// <summary>
/// Return true if key multipivot broker in group
/// </summary>

		internal bool IsKeyMultipivotBrokerInGroup
			{
				get
				{
					if (!PivotInCode) return false;
					if (Qt == null || Qe == null || Qe.Qtd == null) return false;

					Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = Qe.MetaBrokerStateInfo;

					MetaTable mt = Qt.MetaTable;

					// Create broker label to appear in SqlList

					if (!mbsi.ContainsKey(MpGroupKey)) return false;
					MultiTablePivotBrokerTypeData mpd = mbsi[MpGroupKey];
					if (Lex.Eq(mpd.FirstTableName, mt.Name)) return true;
					else return false;
				}
			}

/// <summary>
/// Return parameters associated with current ExecuteQuery
/// </summary>

		public ExecuteQueryParms GetExecuteQueryParms
		{
			get
			{
				return Eqp;
			}
		}

/// <summary>
/// Build the sql for a query (without keyset phrase)
/// </summary>
/// <param name="parms"></param>

		public virtual string PrepareQuery (
			ExecuteQueryParms eqp)
		{
			Eqp = eqp;
			Qt = eqp.QueryTable;

			if (IsUnmappedMetatable(Qt.MetaTable)) return "";

			else if (MetaBrokerUtil.IsNonSqlMetaTable(eqp.QueryTable.MetaTable) ||
				MoleculeMetaBroker.IsNonSqlStructureSearchQueryTable(eqp))
			{
				MoleculeMetaBroker = new MoleculeMetaBroker();
				string result = MoleculeMetaBroker.PrepareQuery(eqp);
				return "NoSql"; // no sql should be generated by this broker
			}

			else if (IsRootTableKeyOnlyQuery())
			{
				RootTableKeyOnlyQuery = true;
				return ""; // no sql
			}

			if (QueryEngine.IsOdbcMetatable(eqp.QueryTable.MetaTable))
				KeyListPredType = KeyListPredTypeEnum.Literal; // must be literal key values (i.e. not parameterized) for ODBC

			Sql = BuildSql(eqp);

			if (eqp.CallerSuppliedSql != "") // is the sql being supplied by caller?
				Sql = eqp.CallerSuppliedSql;

			string hint = "/*+ first_rows */";

			Sql = Lex.Replace(Sql, "/*+ hint */", hint);

			OrderBy = "";

			return Sql;
		}

/// <summary>
/// Return true if this query is just a retrieval (not a search) of the keys of a root table
/// </summary>
/// <returns></returns>

		bool IsRootTableKeyOnlyQuery()
		{
			if (!Lex.IsNullOrEmpty(Eqp.CallerSuppliedSql)) return false; // not if this is a search step

			if (!Qt.MetaTable.IsRootTable) return false; // must be a root table

			if (Qt.SelectedCount != 1) return false; // must be only key selected

			if (Qt.GetCriteriaCount(false, false) > 0) return false; // don't allow criteria on any other cols

			if (Eqp.Qe.RootTables.Count > 1) return false; // only one root table allowed

			if (MqlUtil.SingleStepExecution(Query)) return false; // would fail for single-step and between criteria on key

			return true; 
		}

		/// <summary>
		/// Execute a query
		/// </summary>
		/// <param name="QueryTable"></param>
		/// <param name="keySubset"></param>
		/// <param name="mode"></param>
		/// <returns></returns>

		public virtual void ExecuteQuery (
			ExecuteQueryParms eqp)
		{
			Eqp = eqp;
			Qt = eqp.QueryTable;

			SetKeyListPredicate();

			if (IsUnmappedMetatable(Qt.MetaTable)) return;

			else if (MoleculeMetaBroker != null)
			{
				if (MoleculeMetaBroker.BuildSqlOnly) return; // just building SQL

				MoleculeMetaBroker.ExecuteQuery(eqp);
				return;
			}

			else if (RootTableKeyOnlyQuery) // just returning keys
			{
				LastKeyIdx = -1; // init where we are at in the keys
				return;
			}

			if (DrDao!=null)
			{
				DrDao.CloseReader();
				DrDao = null;
			}

			DrDao = new DbCommandMx(); // get reader
			DrDao.Timeout = eqp.Qe.Query.Timeout; // copy any timeout value

			FirstKeyIdx = 0;
			LastKeyIdx = 0;
			LastKeyCount = -1; 
			KeyCount=0;

			PrepareForFetch(); // prepare for initial fetch
			return;
		}

		/// <summary>
		/// Build the sql for the query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public virtual string BuildSql(
			ExecuteQueryParms eqp)
		{
			List<MetaColumn> mcl;
			List<QueryColumn> qcl;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			QueryColumn structureCriteria = null; // any structure search criteria
			QueryColumn molDatabaseCriteria = null; // criteria for selected mol databases
			string sql, tok;
			int qci, i1;

			Eqp = eqp;
			Qt = eqp.QueryTable;

			qcl = Qt.QueryColumns;
			mt = Qt.MetaTable;
			mcl = mt.MetaColumns;

			KeyMc = mt.KeyMetaColumn;
			if (KeyMc == null)
				throw new Exception("Key (compound number) column not found for MetaTable " + mt.Name);
			KeyQci = Qt.GetQueryColumnIndexByName(KeyMc.Name);
			KeyQc = Qt.QueryColumns[KeyQci];
			KeyQc.Selected = true; // be sure selected

			int selectCount = 0;
			int criteriaCount = 0;
			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				mc = qc.MetaColumn;
				if (qc.Is_Selected_or_GroupBy_or_Sorted) selectCount++;
				if (qc.Criteria != "") criteriaCount++;
			}

			FromClause = AdjustTableMap(Qt);
			Exprs = Criteria = OrderBy = "";
			string innerExprs = ""; // mapping of selected & criteria columns
			bool addCtab = false;

			SelectList = new List<MetaColumn>(); // list of selected metacolumns gets built here

			// Process query column objects

			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				mc = qc.MetaColumn;
				if (mc == null) continue; // can happen in some multidatabase search cases

				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				if (qc.Is_Selected_or_GroupBy_or_Sorted) // if selected or sorting on it add to expression list
				{
					if (Exprs.Length > 0) Exprs += ",";
					Exprs += mc.Name;
					SelectList.Add(mc);
				}

				string innerExpr = GetColumnSelectionExpression(mc);
				if (innerExprs.Length > 0) innerExprs += ",";

				if (mc.ColumnMap.StartsWith("InternalMethod", StringComparison.OrdinalIgnoreCase) ||
					mc.ColumnMap.StartsWith("PluginMethod", StringComparison.OrdinalIgnoreCase) ||
					mc.ColumnMap.StartsWith("ExternalToolMethod", StringComparison.OrdinalIgnoreCase)) // old keyword
					innerExpr = "null " + mc.Name; // just return null initially if data is to be generated by external tool method

				else if (mc.DataType == MetaColumnType.QualifiedNo) // normal qualified number
					innerExpr = GetQualifiedNumberInnerExpr(qc);

				else if (mc.DetailsAvailable)
					innerExpr = GetDetailsAvailableInnerExpr(qc);

				else if (mc.DataType == MetaColumnType.Structure)
				{ // if Direct database add expression to extract chimestring if not already included
					if (MqlUtil.IsCartridgeMetaTable(mt))
					{
						if (!Lex.Contains(FromClause, "chime(") && // skip if already have chime function
						!Lex.Contains(FromClause, mc.Name)) // skip if already have molstructure column
						{
							innerExpr = "chime(ctab) " + mc.Name;
						}

						if (Lex.IsDefined(qc.Criteria)) addCtab = true; // be sure ctab col is added if searching
					}

					else { } // from other structure format (e.g. UCDB)
				}

				else if (mc.DataType == MetaColumnType.MolFormula)
				{
					if (MqlUtil.IsCartridgeMetaTable(mt) &&
						!Lex.Contains(FromClause, "molfmla(") && // skip if already have molfmla function
						!Lex.Contains(FromClause, mc.Name)) // skip if already have molformula column
					{
						innerExpr = "molfmla(ctab) " + mc.Name;
						addCtab = true; // add ctab for searching function
					}
					else { } // from other structure format (e.g. UCDB)
				}

				else if (mc.IsDatabaseSetColumn)
				{ // get label for database
					RootTable rti = RootTable.GetFromTableName(mt.Name);
					if (rti == null) innerExpr = mt.Name;
					else innerExpr = rti.Label;
					innerExpr = "'" + innerExpr + "' " + mc.Name;
				}

				//				else if (mc.Name != innerExpr) innerExpr += " " + mc.Name; // map name differs from col name

				innerExprs += innerExpr;

			} // end of query column loop

			if (addCtab) // need to add ctab column?
				innerExprs += ", ctab";

			FromClause = AdjustSqlForNotExistsCriteriaAsNeeded(eqp, Exprs, FromClause);

			string innerAlias = "";
			if (innerExprs == Exprs) // both expression lists the same?
			{
				sql = "select /*+ hint */ " + Exprs + " from " + FromClause;
			}

			else
			{
				SubqueryIdSeq++; // assign subquery name to inner select as needed by some SQL processors (e.g. ODBC/Netezza)
				innerAlias = " SQ_" + SubqueryIdSeq;
				sql =
					"select /*+ hint */ " + Exprs +
					" from (select " + innerExprs + " from " + FromClause + " " + innerAlias + SubqueryIdSeq + ") ";
			}

			if (addCtab) // use first_rows hint if RCG access for good performance (e.g. Jubilant databases)
				sql = sql.Replace("/*+ hint */", "/*+ first_rows */");

			if (Qt.Alias != "") sql += " " + Qt.Alias;
			if (eqp.CallerSuppliedCriteria != "")
				sql += " where " + eqp.CallerSuppliedCriteria;

			return sql;
		 }

		/// <summary>
		/// If not-exists criteria then fixup the Sql just retrieved
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="fromClause"></param>
		/// <returns></returns>

		public string AdjustSqlForNotExistsCriteriaAsNeeded(
			ExecuteQueryParms eqp,
			string exprs,
			string fromClause)
		{

			// This method implements the not-exists criteria when a key list will be provided as input 
			// when the statement is executed. It adds a subquery that consists of a table of all keys
			// that is outer joined to the supplied fromClause. The subquery table uses dbms_debug_vc2coll
			// to create an in-memory table that is used just for the execution of the current statement. 

			bool returnNormalName, returnValSuffixedName;

			QueryTable qt = eqp.QueryTable;

			if (!qt.HasNotExistsCriteria) return fromClause; // nothing to do if no not-exists

			if (qt.HasNormalDatabaseCriteria) return FromClause; // if other normal criteria no table then handle with regular outer-join

			if (!QueryEngine.IsOracleMetatable(qt.MetaTable)) // don't do special Oracle syntax if non-Oracle table (e.g. ODBC)
				return fromClause; 

			string keyMcName = qt.KeyQueryColumn.MetaColumnName;
			SubqueryIdSeq++; // assign subquery name to inner select as needed by some SQL processors (e.g. ODBC/Netezza)

			string bt = "nxb_" + SubqueryIdSeq; // alias for base table
			string kt = "nxk_" + SubqueryIdSeq; // alias for key table

			string exprs2 = ""; // build list of cols from both tables
			foreach (QueryColumn qc in qt.QueryColumns)
			{
				if (qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted)
				{
					GetColumnNameTypesToReturn(qc, out returnNormalName, out returnValSuffixedName);

					if (qc.IsKey)
						Lex.AppendToList(ref exprs2, ", ", kt + ".column_value " + qc.MetaColumnName);
					else
					{
						if (returnNormalName)
							Lex.AppendToList(ref exprs2, ", ", bt + "." + qc.MetaColumnName);

						if (returnValSuffixedName)
							Lex.AppendToList(ref exprs2, ", ", bt + "." + qc.MetaColumnName + "_val");
					}
				}
			}

			string sql1 = "(select " + exprs2 + " from ";

			string sql2 =
				@" nxb_<seqid>,
				(select column_value
				from (<not_exists_key_list_table>)) nxk_<seqid>
				where nxb_<seqid>.<nxb_key_name>(+) = nxk_<seqid>.column_value)";

			string sql2a =
				@" nxb_<seqid>,
				(select column_value
				from table(sys.dbms_debug_vc2coll(<not_exists_key_list>))) nxk_<seqid>
				where nxb_<seqid>.<nxb_key_name>(+) = nxk_<seqid>.column_value)";


			sql2 = Lex.Replace(sql2, "<seqid>", SubqueryIdSeq.ToString());
			sql2 = Lex.Replace(sql2, "<nxb_key_name>", keyMcName);

			fromClause = sql1 + fromClause + sql2; // build new from clause

			return fromClause;
		}

		/// <summary>
		/// Determine if we are returning a column with any combination of the following:
		/// 1. Using its normal metacolumn name
		/// 2. Using the metacolumn name with a "_val" suffix
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="returnNormalName"></param>
		/// <param name="returnValSuffixedName"></param>

		void GetColumnNameTypesToReturn(
			QueryColumn qc,
			out bool returnNormalName,
			out bool returnValSuffixedName)
		{
			returnNormalName = false;
			returnValSuffixedName = false;

			if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted)
				return; // nothing to return

			bool nonPrimitiveReturnValue = // these types can return non-primitive values
				(qc.DataType == MetaColumnType.QualifiedNo) ||
				(!qc.IsKey && qc.MetaColumn.DetailsAvailable);

			if (Eqp.ReturnPrimitiveDataTypesOnly || !nonPrimitiveReturnValue)
				returnNormalName = true;

			else // return one or both column name types
			{
				if (qc.Is_Selected_or_GroupBy_or_Sorted)
					returnNormalName = true;

				if (Lex.IsDefined(qc.Criteria))
					returnValSuffixedName = true;
			}

			return;
		}

		/// <summary>
		/// Make any adjustments to the source sql based on the selected columns & criteria
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		string AdjustTableMap(QueryTable qt)
		{
			QueryColumn qc;

			MetaTable mt = qt.MetaTable;
			string defaultMap = mt.TableMap; // original source
			Query q = qt.Query;

			return defaultMap; // noop
		}

/// <summary>
/// Get sql expression for selecting column qualified by table alias
/// </summary>
/// <param name="mc"></param>
/// <returns></returns>

		public static string GetColumnSelectionExpression(
			MetaColumn mc)
		{
			return GetColumnSelectionExpression(mc, null);
		}

/// <summary>
/// Get sql expression for selecting column qualified by table alias
/// </summary>
/// <param name="mc"></param>
/// <param name="tableAlias"></param>
/// <returns></returns>

		public static string GetColumnSelectionExpression(
			MetaColumn mc,
			string tableAlias)
		{
			string expr = null;

			if (String.IsNullOrEmpty(mc.ColumnMap) || Lex.Eq(mc.Name, mc.ColumnMap))
				expr = mc.Name;
			else expr = mc.ColumnMap + " " + mc.Name;

			if (!String.IsNullOrEmpty(tableAlias))
			{
				string ta = "tableAlias";
				int i1 = expr.IndexOf(ta, StringComparison.OrdinalIgnoreCase);
				if (i1 >= 0) // if "tableAlias" contained in map then substitute in that position
				{
					ta = expr.Substring(i1, ta.Length);
					expr = expr.Replace(ta, tableAlias);
				}

				else expr = tableAlias + "." + expr;
			}
			return expr;
		}

/// <summary>
/// Return expression(s) for qualified number for selecting & searching
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		string GetQualifiedNumberInnerExpr(
			QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;
			string[] qnSubCols; // qualifier and basic numeric value columns
			string innerExpr = "";
			
			qnSubCols = mc.ColumnMap.Split(','); // Get subcolumns. If 2 then the first should be the string qualifier col and the 2nd a numeric col
			if (qnSubCols.Length == 1) // if just one col assume it is the name of a string column containing a possibly qualified number string
			{
				qnSubCols = new string[2];
				qnSubCols[0] = ExtractQualifierOracleExpression(mc.ColumnMap); // expression to extract qualifier
				qnSubCols[1] = ToNumberOracleExpr(mc.ColumnMap); // expression to extract numeric value
			}

			if (Eqp.ReturnQNsInFullDetail) // return as qualified number
			{
				if (qc.Is_Selected_or_GroupBy_or_Sorted)
				{
					for (int sci = 0; sci < qnSubCols.Length; sci++)
					{ // append values from all columns going into qualified number
						if (sci > 0) innerExpr += " || chr(11) || ";
						innerExpr += qnSubCols[sci];
					}

					innerExpr += " " + mc.Name;
				}

				if (qc.Criteria != "")
				{ // return number value with basic column name with "_val" appended
					if (innerExpr != "") innerExpr += ", ";
					innerExpr += qnSubCols[1] + " " + mc.Name + "_val "; 
				}
			}

			else innerExpr = // just return basic number value under column name
				qnSubCols[1] + " " + mc.Name;

			return innerExpr;
		}

/// <summary>
/// Oracle regular expression to extract the qualifier from a string
/// </summary>
/// <param name="arg"></param>
/// <returns></returns>

		public static string ExtractQualifierOracleExpression(string arg)
		{
				return "regexp_substr(" + arg + ", '^[<>]')"; 
		}

		/// <summary>
		/// Oracle regular expression to extract the number value from a qualified number string
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static string ToNumberOracleExpr(string arg)
		{
			return "to_number(regexp_substr(" + arg + 
				@", '(\.[0-9]+|\-\.[0-9]+|\-[0-9]+[\.]*[0-9]*|[0-9]+[\.]*[0-9]*)'))";
		}

/// <summary>
/// Return expression(s) for DetailsAvailable column for selecting & searching
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		string GetDetailsAvailableInnerExpr(
			QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;
			string innerExpr = mc.Name;

			if (Eqp.ReturnQNsInFullDetail && qc.Criteria != "") 
			{ 
				innerExpr += ", " + mc.Name + "_val "; // return val used for comparison
			}

			return innerExpr;
		}

////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// Do prep for next fetch of data rows including:
/// 1. Modifying base sql to include key set (if needed)
/// 2. Doing a PrepareQuery (if needed)
/// 3. ExecuteReader to open cursor
/// 
/// </summary>
////////////////////////////////////////////////////////////////////////////////

		void PrepareForFetch ()
		{
			string keyCriteria, keyListString, keyListParameterValue, sql2 = "", txt;
			int i1;

			string keyName = "", keyPredType = "";
			if (KeyMc != null) keyName = KeyMc.Name;
			if (Eqp.KeyName != "") keyName = Eqp.KeyName; // overriding key name

			if (IsUnmappedMetatable(Qt.MetaTable)) return;

			try
			{

				if (Lex.IsUndefined(Sql))
				{
					string msg = "Sql is undefined for table: " + Eqp?.QueryTable?.MetaTableName;
					QueryEngine.LogExceptionAndSerializedQuery(msg, Query);
					throw new Exception(msg); // pass it up;
				}

// Execute query as it, no key set provided

				if (Eqp.SearchKeySubset == null) // not limiting to set of keys
				{
					sql2 = ActivateInnerNonKeyListCriteria(Sql);

					if (!String.IsNullOrEmpty(OrderBy)) sql2 += " order by " + OrderBy;
					try { PrepareReader(DrDao, sql2); }
					catch (Exception ex)
					{
						DrDao.Dispose();
						throw new Exception(ex.Message, ex);
					}

					KeyParmArray = new Object[0]; // no parameters
				}

// Subsetting using database table with full key list
 
				else if (KeyListPredTypeDbTable) 
				{
					FirstKeyIdx = 0;
					LastKeyIdx = Eqp.SearchKeySubset.Count - 1;
					KeyCount = Eqp.SearchKeySubset.Count;

					keyCriteria = Eqp.Qe.BuildTempDbTableKeyListPredicate(Eqp, ref Sql, keyName, Eqp.SearchKeySubset, FirstKeyIdx, KeyCount);

					sql2 = Sql + " and " + keyCriteria;

					if (!String.IsNullOrEmpty(OrderBy)) sql2 += " order by " + OrderBy;

					try { PrepareReader(DrDao,sql2); }
					catch (Exception ex)
					{
						DrDao.Dispose();
						throw new Exception(ex.Message, ex);
					}

					KeyParmArray = null; // not passing key parameters
					LastKeyCount = KeyCount;
				}

// Subsetting, select next chunk of keys

				else 
				{
					int keyBlockSize = DbCommandMx.MaxOracleInListItemCount;
					if (SingleKeyRetrieval) keyBlockSize = 1;

					LastKeyIdx = FirstKeyIdx + keyBlockSize - 1;
					if (LastKeyIdx >= Eqp.SearchKeySubset.Count) // past end of subset?
						LastKeyIdx = Eqp.SearchKeySubset.Count - 1;
					KeyCount = LastKeyIdx - FirstKeyIdx + 1;

					//if (FirstKeyIdx <= 0) // if 1st chunk then set ParameterizeKeys value
					//  ParameterizeKeys = DbConnectionMx.CanParameterizeSql(Sql);

					// Pass keys in parameterized form

					if (KeyListPredTypeParameterized) // pass keys as parameters
					{
						if (KeyCount != LastKeyCount) // need to prepare sql?
						{
							txt = "";
							for (i1 = 0; i1 < KeyCount; i1++) // build parameters
							{
								txt += ":" + i1.ToString();

								if (i1 < KeyCount - 1)
								{
									txt += ",";
								}
							}

							keyCriteria = keyName + " in (" + txt + ")";

							sql2 = AppendSearchKeySubset(Sql, " and " + keyCriteria);

							sql2 = ActivateInnerKeyListCriteria(sql2, txt); // include list criteria any place else the list that it is needed

							if (!String.IsNullOrEmpty(OrderBy)) sql2 += " order by " + OrderBy;

							try { PrepareMultipleParameterReader(DrDao, sql2, KeyCount); }
							catch (Exception ex)
							{
								DrDao.Dispose();
								throw new Exception(ex.Message, ex);
							}

							LastKeyCount = KeyCount;
							KeyParmArray = new Object[KeyCount]; // array to contain parameter values
						}

						for (i1 = 0; i1 < KeyCount; i1++) // copy keys to parameter array properly normalized
						{
							string key = (string)Eqp.SearchKeySubset[FirstKeyIdx + i1];
							key = CompoundId.NormalizeForDatabase(key, Qt.MetaTable);

							if (key == null) key = NullValue.NullNumber.ToString(); // if fails supply a "null" numeric value
							KeyParmArray[i1] = key;
						}
					}

					// Pass keys in unparameterized list form 

					else
					{
						Eqp.Qe.BuildUnparameterizedKeyListPredicate(Eqp, ref Sql, keyName, Eqp.SearchKeySubset, FirstKeyIdx, KeyCount, out keyCriteria, out keyListString);

						sql2 = AppendSearchKeySubset(Sql, " and " + keyCriteria);

						sql2 = ActivateInnerKeyListCriteria(sql2, keyListString); // include list criteria any place else the list that it is needed

						if (!String.IsNullOrEmpty(OrderBy)) sql2 += " order by " + OrderBy;

						try { PrepareReader(DrDao, sql2); }
						catch (Exception ex)
						{
							DrDao.Dispose();
							throw new Exception(ex.Message, ex);
						}

						KeyParmArray = null; // not passing key parameters
						LastKeyCount = KeyCount;
					}
				}

				if (BuildSqlOnly) return; // if just building sql return;

				if (DrDao == null) return; // dao gone, cancelled?
				DrDao.CheckForCancel = Eqp.CheckForCancel; // set cancel check flag

				//if (KeyParmArray == null || KeyParmArray.Length == 0 || SingleKeyRetrieval)
				//  DrDao.Timeout = 0; // no timeout

				//else DrDao.Timeout = 1; // let it run for 1 second (causes big slowdown in large list-based searches)

				// Execute the query

				Stopwatch sw = Stopwatch.StartNew();

				try { Dr = ExecuteReader(DrDao, KeyParmArray); } // (!!! break here to pick up sql) 
				catch (Exception ex)
				{
					if (DrDao != null)
						DrDao.Dispose();

					if (MqlUtil.IsUserQueryErrorMessage(ex.Message))  // catch any user error a query structure and throw UserQueryException
						throw new UserQueryException("Invalid Query:\r\n\r\n" + ex.Message, ex);

					else throw new Exception(ex.Message, ex);
				}

				if (KeyParmArray == null || KeyParmArray.Length == 0) NonKeyRetrievalCount++;
				else if (SingleKeyRetrieval) SingleKeyRetrievalCount++;
				else MultipleKeyRetrievalCount++;

				if (QueryEngine.LogBasics)
				{
					if (Eqp.SearchKeySubset == null) keyPredType = "None"; // not subsetting
					else keyPredType = KeyListPredType.ToString();
					if (KeyCount > 0) keyPredType += "(" + KeyCount + ")";

					int tDelta = (int)sw.ElapsedMilliseconds;
					
					QueryTable qt = Eqp.QueryTable;
					string tName = (qt != null) ? tName = ", Table: " + qt.MetaTable.Name + " (" + qt.Alias + "), " : "";
					DebugLog.Message(this.GetType().Name + " Execute - " + tName + "KeyListPredType: " + keyPredType + ", time: " + tDelta + ", sql: " + Lex.RemoveLineBreaksAndTabs(sql2));
				}

				if (DrDao.Cancelled || DrDao == null)
				{
					if (Eqp != null) Eqp.Cancelled = true;
					if (DrDao != null) DrDao.Dispose(); // free connection
					return;
				}
			}

			catch (UserQueryException ex) // if UserQueryException just pass it along
			{ throw ex; }

			catch (Exception ex)
			{
				string lastSql = "";
				if (DrDao != null)
					lastSql = "lastSql = " + DrDao.LastSql;

				DebugLog.Message(
					"GenericMetaBroker.PrepareForFetch.ExecuteReader:\r\n" + DebugLog.FormatExceptionMessage(ex) + "\r\n" +
					"lastSql: " + OracleMx.FormatSql(lastSql) + "\r\n" +
					"FirstKeyIdx = " + FirstKeyIdx.ToString() + ", " +
					"LastKeyIdx = " + LastKeyIdx.ToString());
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// This is a helper method that will take a comma delimited list and a database columne name and
		/// build an Oracle "IN" clause.  If the number exceeds 1,000 it will break it into multiple statements
		/// (seperated by an "OR" clause) so the Oracle 1,000 max limit is not exceeded. 
		/// </summary>
		/// <param name="dbColumnName"></param>
		/// <param name="list"></param>
		/// <returns>SQL for criteria</returns>

		public static string BuildSqlInCriteriaFromList(string dbColumnName, string list)
		{
			string criteria = "";
			string[] items = list.Split(',');

			Lex.AppendToList(ref criteria, " and ", "(" + dbColumnName + " in (");
			var chunks = from index in Enumerable.Range(0, items.Length)
									 group items[index] by index / DbCommandMx.MaxOracleInListItemCount;
			int chunksRead = 0;
			foreach (var chunk in chunks)
			{
				if (chunksRead > 0) criteria += ") or " + dbColumnName + " in (";
				chunksRead++;
				var result = string.Join(",", chunk.ToArray());
				criteria += result;
				criteria = criteria.TrimEnd(',');
			}
			criteria += "))";

			return criteria;
		}

		/// <summary>
		/// Return true if the assayId matches the metatable or is under a merged metatable
		/// </summary>
		/// <param name="assayId"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool CodeAppliesToTable(
			string assayId,
			MetaTable mt)
		{
			if (mt.Code == assayId) return true;
			if (mt.TableFilterValues != null)
			{
				foreach (string tableCode in mt.TableFilterValues)
				{
					if (tableCode == assayId) return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get unpivoted value from reader & store in pivoted vo
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="drd"></param>
		/// <param name="uVoi"></param>
		/// <param name="vo"></param>
		/// <param name="voPosition"></param>

		public static void CopyUnpivotedValueToPivotedVo(
			MetaColumn mc,
			DbCommandMx drd,
			int uVoi,
			object[] vo,
			int voPosition)
		{
			string txt;

			if (mc.DataType == MetaColumnType.CompoundId)
			{
				int corpId = drd.GetInt(uVoi);
				string cid = CompoundId.Normalize(corpId.ToString(), mc.MetaTable); // normalize cid adding prefix as needed
				vo[voPosition] = cid;
			}

			else if (mc.DataType == MetaColumnType.String)
			{
				txt = drd.GetString(uVoi);
				if (txt != "") vo[voPosition] = txt;
			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				DateTime dt = drd.GetDateTime(uVoi);
				if (dt != DateTime.MinValue) vo[voPosition] = dt;
			}

			else if (mc.DataType == MetaColumnType.Integer)
			{
				int i1 = drd.GetInt(uVoi);
				if (i1 != NullValue.NullNumber) vo[voPosition] = i1;
			}

			else if (mc.DataType == MetaColumnType.Number)
			{
				double d1 = drd.GetDouble(uVoi);
				if (d1 != NullValue.NullNumber) vo[voPosition] = d1;
			}

			else throw new Exception("Unexpected MetaColumnType: " + mc.DataType);

			return;
		}

		/// <summary>
		/// Activate special inner CorpId list criteria for performance optimization
		/// (Generalize later)
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="newKeyList"></param>
		/// <returns></returns>

		string ActivateInnerKeyListCriteria(
			string sql,
			string newKeyList)
		{
			string sql2 = sql;
			//sql2 = AssayMetaBroker.ActivateInnerKeyListCriteria(sql, newKeyList); 

			string sql3 = GenericMetaBroker.ActivateInnerKeyListTable(sql2, newKeyList);

			return sql3;
		}


		/// <summary>
		/// Activate special inner non-keyList list criteria for performance optimization
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		string ActivateInnerNonKeyListCriteria(string sql)
		{
			string sql2 = sql;
			//sql2 = AssayMetaBroker.ActivateInnerNonKeyListCriteria(sql);

			return sql2;
		}

		/// <summary>
		/// Activate any inner "not exists" key list table 
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="newKeyList"></param>
		/// <returns></returns>

		public static string ActivateInnerKeyListTable(
			string sql,
			string newKeyList)
		{
			string select = "";
			int maxFuncArgs = 999;

			if (Lex.IsUndefined(sql) || Lex.IsUndefined(newKeyList)) return sql;

			if (!Lex.Contains(sql, "<not_exists_key_list_table>")) return sql;

			if (Lex.IsUndefined(newKeyList)) DebugMx.ArgException("Undefined key list");

			int totalKeys = Lex.CountCharacter(newKeyList, ',') + 1;

// If the total number of keys exceeds the max number of allowed args in the vc2coll function
// then break into multiple selects that are unioned together.
// The common case will be when there are 1000 keys but the function only allows a max of 999

			string[] sa = Lex.SplitListIntoSublists(newKeyList, ',', maxFuncArgs);

			StringBuilder sb = new StringBuilder();

			for (int li = 0; li < sa.Length; li++)
			{
				select = @"
					(select column_value
					 from table(sys.dbms_debug_vc2coll(<list>)))";

				select = Lex.Replace(select, "<list>", sa[li]);

				if (li > 0) sb.Append("\n union all \n");

				sb.Append(select);
			}

			if (sa.Length < 2)
				select = sb.ToString();

			else // wrap unioned sql
				select = "(select column_value from (" + sb + "))";

			string sql2 = Lex.Replace(sql, "<not_exists_key_list_table>", select);

			return sql2;
		}

		/// <summary>
		/// Append any "and-clause" key criteria to the sql adding parens to existing clause as necessary
		/// </summary>
		/// <returns></returns>

		string AppendSearchKeySubset(
			string sql,
			string keyCriteria)
		{
			if (Lex.IsUndefined(sql) || Lex.IsUndefined(keyCriteria)) return sql;

			string sql2 = Lex.RemoveLineBreaksAndTabs(sql);

			int i1 = Lex.LastIndexOf(sql2, "where ");

			if (i1 < 0) DebugMx.DataException("'Where' token not found in sql: " + sql2);

			string whereClause = sql2.Substring(i1);

			if (Lex.Contains(whereClause, " or ") || Lex.Contains(whereClause, "and not")) // if non-and logic then put existing logic in parens
			{
				string logic = whereClause.Substring(6); // extract logic
				whereClause = "where (" + logic + ")";
				sql2 = sql2.Substring(0, i1) + whereClause;
			}

			sql2 += keyCriteria;
			return sql2;
		}
		
		////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// NextRow - Return the next matching row value object
		/// </summary>
		/// <returns></returns>
		////////////////////////////////////////////////////////////////////////////////

		public virtual Object [] NextRow()
		{
			object [] vo = null;
			object obj;
			MetaTable mt;
			MetaColumn mc;
			Decimal sysDecimal;
			QualifiedNumber qn;
			MoleculeMx cs;
			string cid = null, rowKey = "", stringValue, tok, txt, txt2;
			string [] sa;
			int ci;

			if (IsUnmappedMetatable(Qt.MetaTable)) return null;

			else if (MoleculeMetaBroker != null) return MoleculeMetaBroker.NextRow();

			else if (RootTableKeyOnlyQuery) return RootTableKeyOnlyQueryNextRow();

			if (DrDao==null) return null;

			while (true)
			{
				bool readOk = Read(DrDao);
				if (readOk) break;

				DrDao.CloseReader(); // close the reader if done with rows or error

				if (DrDao.Cancelled)
				{
					Eqp.Cancelled = true;
					DrDao.Dispose();
					DrDao = null;
					return null;
				}

				if (Eqp.SearchKeySubset==null || LastKeyIdx + 1 >= Eqp.SearchKeySubset.Count) 
				{ // all done
					DrDao.Dispose();
					DrDao = null;
					return null;
				}

				FirstKeyIdx = LastKeyIdx + 1;
				PrepareForFetch(); // prep for the next chunk
				continue; // try to read it
			}

			// Fill in the vo

			vo = new Object[SelectList.Count];

			//object[] oa = new object[Dr.FieldCount]; 
			//int objCnt = Dr.GetValues(oa);

			mt = Eqp.QueryTable.MetaTable;
			bool selectedLateBindMethodColumn = false;
			for (ci=0; ci<SelectList.Count; ci++)
				try
				{
					mc = SelectList[ci];

					if (mc.ColumnMap.StartsWith("InternalMethod", StringComparison.OrdinalIgnoreCase) ||
					 mc.ColumnMap.StartsWith("PluginMethod", StringComparison.OrdinalIgnoreCase) ||
					 mc.ColumnMap.StartsWith("ExternalToolMethod", StringComparison.OrdinalIgnoreCase)) // old keyword
						selectedLateBindMethodColumn = true;

					if (Dr.IsDBNull(ci))
					{
						vo[ci] = null;
						continue;
					}

					if (mc.DataType == MetaColumnType.CompoundId)
					{
						tok = Dr.GetValue(ci).ToString();
						//if (tok.Contains("91341")) tok = tok; // debug;
						cid = CompoundId.Normalize(tok, Qt.MetaTable); // normalize adding prefix as needed
						vo[ci] = cid;
					}

					else if (mc.DataType == MetaColumnType.Structure)
					{
						Stopwatch sw = Stopwatch.StartNew();

						stringValue = Dr.GetString(ci);

						GetStructureTime += sw.Elapsed.TotalMilliseconds;
						
						string molString = stringValue;
						string stereoComments = MoleculeUtil.ExtractStereoComments(ref molString);

						string dbLink = "", hyperLink = "";
						int i1 = stringValue.IndexOf("\v"); // see if structure with link info
						if (i1 >= 0) // may contain hyperlink and/or database link
						{
							molString = stringValue.Substring(0, i1);

							if ((i1 + 1) < stringValue.Length)
							{
								txt = stringValue.Substring(i1 + 1);
								sa = txt.Split(',');
								if (sa.Length >= 1) dbLink = sa[0]; // database link
								if (sa.Length >= 2) hyperLink = sa[1]; // hyperlink
							}
						}

						MoleculeMx.TrySetStructureFormatPrefix(ref molString, mc.DataTransform); // be sure we have prefix if appropriate

						cs = new MoleculeMx(molString); // create the structure object

						bool isUcdb = (mt != null && mt.Root.IsUserDatabaseStructureTable); // user compound database
						if (Lex.IsDefined(cid) && !isUcdb)
							MoleculeCache.AddMolecule(cid, cs);

						cs.DbLink = dbLink;
						cs.Hyperlink = hyperLink;
						cs.StoreKeyValueInMolComments(mc, rowKey);

						//if (molString.Contains("CorpId")) molString = molString; // debug

						if (Lex.IsDefined(stereoComments) && mc.MetaTable.IsRootTable &&  // set stereo comments if root table
							(Query == null || Query.ShowStereoComments == true) && // don't do if turned off
							(Qe == null || Qe.QeSingleStepExecution == false)) // don't do if optimized single step query
							 MoleculeUtil.SetStereochemistryComments(cs, stereoComments, rowKey);

						vo[ci] = cs; // store structure
					}

					else if (mc.DataType == MetaColumnType.Integer ||
						mc.DataType == MetaColumnType.Number)
					{
						double d1;

						if (!mc.DetailsAvailable) // simple scalar value
						{

							if (mc.StorageType == MetaColumnStorageType.Unknown)
							{ // identify string type columns
								Type t = DrDao.Rdr.GetFieldType(ci);
								if (t.Name == "String")
									mc.StorageType = MetaColumnStorageType.String;
							}

							if (mc.StorageType == MetaColumnStorageType.String) // convert string to number
							{
								string s = DrDao.GetString(ci);
								if (!Double.TryParse(s, out d1)) // try to parse the string
								{
									vo[ci] = null;
									continue;
								}
							}

							else  d1 = DrDao.GetDouble(ci); // copy to double first to avoid possible underflow

							if (mc.DataType == MetaColumnType.Integer)
							{
								if (mc.StorageType == MetaColumnStorageType.Int64)
									vo[ci] = (Int64)d1;

								else vo[ci] = (int)d1; // convert to 32 bit integer otherwise
							}
							else vo[ci] = d1;
						}

						else // value with possible resultId, linking information
						{
							txt = Dr.GetString(ci);
							qn = QueryEngine.ParseScalarValue(txt, Qt, mc);
							NumberMx nex = new NumberMx(qn);
							vo[ci] = nex;
						}
					}

					else if (mc.DataType == MetaColumnType.QualifiedNo)
					{
						object o = Dr.GetValue(ci);

						if (mc.IsKey) // if key, convert to string & store
						{
							txt = o.ToString();
							vo[ci] = CompoundId.Normalize(txt, Qt.MetaTable); // normalize
						}

						else // normal qualified number
						{
							if (o is string)
							{
								txt = o.ToString(); // qualified numbers are catenated strings
								qn = ParseQualifiedNumber(txt);
								vo[ci] = qn;
							}

							else vo[ci] = new QualifiedNumber(o); // try converting to QN
						}
					}

					else if (mc.DataType == MetaColumnType.String ||
						mc.DataType == MetaColumnType.MolFormula ||
						mc.DataType == MetaColumnType.Hyperlink)
					{
						try { txt = Dr.GetValue(ci).ToString(); }
						catch (Exception ex) { txt = ex.Message; } // catch occasional errors (e.g. overflow)

						if (Lex.Eq(txt, "NULL")) // sometimes "NULL" e.g. XRay2 urls
						{
							vo[ci] = null;
							continue;
						}

						//if (txt == "strange curve") txt = txt; // debug

						bool singleElement = !txt.Contains("\v");
						if (singleElement) // simple text value
						{
							vo[ci] = txt;

							//if (txt.Contains("\v")) vo[ci] = txt.Replace('\v', '\n'); // special fix (obsolete)
						}

						else // text and dblink info
						{
							string[] args = txt.Split('\v'); // elements are separated by vertical tab chars
							StringMx sx = new StringMx();
							sx.Value = args[0];
							sx.DbLink = args[1];
							vo[ci] = sx;
						}
					}

					else if (mc.DataType == MetaColumnType.Date)
					{
						object o = Dr.GetValue(ci);
						if (o is DateTime) vo[ci] = (DateTime)o;

						else // string form with optional link info
						{
							txt = o.ToString();
							if (txt.IndexOf("\v") < 0)
								vo[ci] = DateTimeMx.NormalizedToDateTime(txt); // simple text value
							else
							{
								string[] args = txt.Split('\v'); // elements are separated by vertical tab chars
								DateTimeMx dex = new DateTimeMx(args[0]);
								dex.DbLink = args[1];
								vo[ci] = dex;
							}
						}
					}

					else if (mc.DataType == MetaColumnType.DictionaryId)
					{
						try // Id may be string or integer value
						{ vo[ci] = Dr.GetString(ci); }
						catch (Exception ex)
						{ vo[ci] = DrDao.GetDouble(ci).ToString(); }
					}

					else if (mc.DataType == MetaColumnType.Image)
					{
						object o = Dr.GetValue(ci);
						if (o is string) vo[ci] = (string)o;

						else if (o is byte[]) // if blob assume it contains a standard image format
						{
							byte[] ba = (byte[])o;
							Bitmap bmp = null;
							try
							{
								bmp = new Bitmap(new MemoryStream(ba));
								vo[ci] = bmp;
							}
							catch (Exception ex)
							{
								vo[ci] = null;

								//string tfn = TempFile.GetTempFileName("svg"); // debug
								//FileStream fs = new FileStream(tfn, FileMode.OpenOrCreate, FileAccess.Write);
								//fs.Write(ba, 0,ba.Length);
								//fs.Close();
							} // just return null if not a recognized format
						}

						else // assume numeric
							vo[ci] = DrDao.GetDouble(ci).ToString();
					}

					if (mc.IsKey)
					{
						rowKey = QueryEngine.GetKeyString(vo[ci]);
						if (rowKey == null) rowKey = "<null>";
					}
				}

// Catch errors in getting values & replace with null

				catch (Exception ex)
				{
					vo[ci] = null; // null the value so we can proceed

					string exMsg = DebugLog.FormatExceptionMessage(ex);

					if (Qt != null) // log the error
						QueryEngine.LogExceptionAndSerializedQuery(exMsg, Qt.Query);

					else DebugLog.Message(exMsg);
					continue;
				}

// Get any column values generated by external tool methods

			if (selectedLateBindMethodColumn)
				for (ci = 0; ci < SelectList.Count; ci++)
				{
					mc = SelectList[ci];
					mt = mc.MetaTable;

					if (mc.ColumnMap.StartsWith("InternalMethod", StringComparison.OrdinalIgnoreCase) ||
					 mc.ColumnMap.StartsWith("PluginMethod", StringComparison.OrdinalIgnoreCase) ||
					 mc.ColumnMap.StartsWith("ExternalToolMethod", StringComparison.OrdinalIgnoreCase)) // old keyword
					{
						try { CallLateBindMethodToFillVo(vo, ci, mt, SelectList); }
						catch (Exception ex) { vo[ci] = null; } // just null if error
					}
				}

			return vo;
		}

/// <summary>
/// Parse Qualified number from database format
/// </summary>
/// <param name="txt"></param>
/// <returns></returns>

		public virtual QualifiedNumber ParseQualifiedNumber(string txt)
		{
			QualifiedNumber qn = QueryEngine.ParseQualifiedNumber(txt);
			return qn;
		}

/// <summary>
/// Return the value object for the next key for a root table key only retrieval
/// </summary>
/// <returns></returns>

		Object[] RootTableKeyOnlyQueryNextRow()
		{
			if (Eqp?.SearchKeySubset == null || Eqp.SearchKeySubset.Count == 0) return null;

			if (LastKeyIdx + 1 >= Eqp.SearchKeySubset.Count) return null; // past end of subset?
			LastKeyIdx++;
			object [] vo = new object[1];
			vo[0] = Eqp.SearchKeySubset[LastKeyIdx];
			return vo;
		}

/// <summary>
/// Call an internal or plugin method to get value
/// </summary>
/// <param name="vo"></param>
/// <param name="colIndex"></param>
/// <param name="mt"></param>
/// <param name="selectList"></param>

		public static void CallLateBindMethodToFillVo(
			object[] vo,
			int colIndex,
			MetaTable mt,
			List<MetaColumn> selectList)
		{
			MetaColumn mc = selectList[colIndex];
			Lex lex = new Lex();
			lex.SetDelimiters("( , )");
			lex.OpenString(mc.ColumnMap);
			string methodType = lex.ParseNonDelimiter();
			string methodRef = lex.ParseNonDelimiter();

			List<object> args = new List<object>();
			while (true)
			{
				string tok = lex.ParseNonDelimiter();
				if (tok == "") break;
				if (tok.StartsWith("\'") || tok.StartsWith("\"")) // string constant
					args.Add(Lex.RemoveAllQuotes(tok));

				else if (Lex.IsDouble(tok)) // numeric constant
					args.Add(tok);

				else if (Lex.Eq(tok, "MetaTable")) // pass metatable
					args.Add(mt);

				else if (Lex.Eq(tok, "MetaColumn")) // pass metacolumn
					args.Add(selectList[colIndex]);

				else // must be column name, get value
				{
					MetaColumn mc2 = mt.GetMetaColumnByName(tok);
					if (mc2 == null) throw new Exception("Unrecognized column name: " + mc.Name);
					int ci2;
					for (ci2 = 0; ci2 < selectList.Count; ci2++)
					{
						if (Lex.Eq(mc2.Name, selectList[ci2].Name)) break;
					}

					if (ci2 >= selectList.Count)
						throw new Exception("Column name not selected in query: " + mc2.Name);

					args.Add(vo[ci2]); // add the value as an arg
				}
			}

			int dotCount = Lex.CountCharacter(methodRef, '.');
			if (dotCount == 1) // if just class.method assume in Mobius.QueryEngineLibrary namespace
				methodRef = "Mobius.QueryEngineLibrary." + methodRef;
			object o = UAL.PluginDao.CallInternalMethod(methodRef, args); 
			vo[colIndex] = o;
			return;

			// Handle everything internally for now
			//object o;
			//if (Lex.Eq(methodType,"InternalMethod"))
			//	o = UAL.PluginDao.CallInternalMethod(methodRef, args); 
			//else o = UAL.PluginDao.CallPluginMethod(methodRef, args);
		}

////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// Close broker & release resources
/// </summary>
////////////////////////////////////////////////////////////////////////////////

		public virtual void Close()
		{

			if (Qt?.MetaTable == null || IsUnmappedMetatable(Qt.MetaTable)) return;

			if (MoleculeMetaBroker != null)
			{
				MoleculeMetaBroker.Close();
				return;
			}

			if (DrDao==null) return;
			DrDao.CloseReader();
			DrDao.Dispose();
			DrDao = null;
		}

		/// <summary>
		/// Return true if metatable is not mapped to any data (i.e. TableMap == "null")
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsUnmappedMetatable(MetaTable mt)
		{
			if (Lex.Eq(mt.TableMap, "null"))
				return true;
			else return false;
		}

		public virtual bool GetPivotedResult ( // Get a pivoted vo given an unpivoted result identifier
			MetaTable mti,
			object [] voi,
			int voOffset,
			ref MetaTable mt2,
			ref object [] vo2)
		{
			return false;
		}

		public virtual Query GetDrilldownDetailQuery(
			MetaTable mt,
			string metaColumnId,
			int level,
			string linkInfo)
		{
			return null;
		}

		public virtual Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string linkInfo)
		{
			return null;
		}

		/// <summary>
		/// Do presearch initialization for a QueryTable
		/// </summary>
		/// <param name="qt"></param>

		public virtual void DoPresearchInitialization(
			QueryTable qt)
		{
			return; // no initialization for generic broker
		}

/// <summary>
/// Return true if table should be checked/transformed at presearch time
/// </summary>
/// <param name="qt"></param>
/// <returns></returns>

public virtual bool ShouldPresearchCheckAndTransform(
			MetaTable mt)
		{
			return false; // no transform for generic broker
		}

/// <summary>
/// Convert a multipivot table into a set of tables where data exists for
/// one or more of the compound identifiers in the list.
/// </summary>
/// <param name="qt">Current form of query table</param>
/// <param name="q">Query to add transformed tables to</param>
/// <param name="ResultKeys">Keys data will be retrieved for</param>

		public virtual void DoPreSearchTransformation(
			Query originalQuery,
			QueryTable qt,
			Query newQuery)
		{
			throw new Exception("Invalid DoPresearchTransform call for " + qt.MetaTable.Name);
		}

/// <summary>
/// Expand query to include the set of metatables that have data for the 
/// given result set.
/// </summary>
/// <param name="qt"></param>
/// <param name="q"></param>
/// <param name="ResultKeys"></param>

		public virtual void ExpandToMultipleTables(
			QueryTable qt,
			Query q,
			List<string> ResultKeys)
		{
			q.AddQueryTable(qt); // just add table as is to query
			return;
		}

/// <summary>
/// Get metacolumn specific conditional formatting
/// </summary>
/// <param name="mc"></param>
/// <param name="qn"></param>
/// <returns></returns>

		public virtual CondFormat GetMetaColumnConditionalFormatting(
			MetaColumn mc,
			QualifiedNumber qn)
		{
			return null; // no formatting by default
		}

/// <summary>
/// Get metacolumn specific conditional formatting colors
/// </summary>
/// <param name="mc"></param>
/// <param name="qn"></param>
/// <param name="foreColor"></param>
/// <param name="backColor"></param>
/// <returns></returns>

		public virtual bool GetMetaColumnConditionalFormattingCellColors(
			MetaColumn mc,
			QualifiedNumber qn,
			out Color foreColor,
			out Color backColor)
		{
			foreColor = Color.Black;
			backColor = Color.White;

			return false; // colors not defined by default
		}

/// <summary>
/// Return true if a description of the specified table is available
/// </summary>
/// <param name="mt"></param>
/// <returns></returns>

		public virtual bool TableDescriptionIsAvailable(
			MetaTable mt)
		{
			if (mt.Description != null && mt.Description.Trim().Length > 0) 
				return true;
			else return false;
		}

	/// <summary>
	/// Return description for table.
	/// May be html or simple text
	/// </summary>
	/// <param name="mt"></param>
	/// <returns></returns>

		public virtual TableDescription GetTableDescription(
			MetaTable mt)
		{
			if (String.IsNullOrEmpty(mt.Description)) return null;

			TableDescription td = new TableDescription();
			td.TextDescription = mt.Description;
			return td;
		}

/// <summary>
/// Return true if criteria from this table are not used for searching but
/// are used for parameters & should be ignored.
/// </summary>
/// <param name="mt"></param>
/// <returns></returns>

		public virtual bool IgnoreCriteria(
			MetaTable mt)
		{
			return false; // don't ignore unless specifically overridden
		}

/// <summary>
/// Get an concentration response curve or other image
/// </summary>
/// <param name="metaColumn"></param>
/// <param name="resultId"></param>
/// <param name="desiredWidth"></param>
/// <returns></returns>

		public virtual Bitmap GetImage(
			MetaColumn metaColumn,
			string resultId,
			int desiredWidth)
		{
			return null; // not defined for generic broker
		}

		/// <summary>
		/// Get additional data from the broker
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public virtual object GetAdditionalData(
			string command)
		{
			string cmdName, mcName;

			Lex.Split(command, " ", out cmdName, out mcName);

			if (Lex.Eq(cmdName, "GetDepictions"))
			{
				MoleculeMetaBroker mmb = MoleculeMetaBroker; // note that mmb must be temp local variable in case we need allocate an new one
				if (mmb == null)
					mmb = MoleculeMetaBroker.GetSearchStepMoleculeBroker(Qe, Qt.MetaTable.Name);

				if (mmb != null)
					return mmb.GetAdditionalData(command);

				else return null;
			}

			return null; // not defined for generic broker
		}

		/// <summary>
		/// Return null if data source for metatable is available otherwise return Schema of 1st unavailable source for table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public virtual DbSchemaMx CheckDataSourceAccessibility(
			MetaTable mt)
		{
			DataSourceMx firstSource;
			DbSchemaMx firstSchema;
			DbConnectionMx conn = null;

			if (mt.TableMap == null || mt.TableMap == "") return null;

			Dictionary<string, Dictionary<string, DbSchemaMx>> connDict =
				DbConnectionMx.GetDataSources(mt.TableMap, out firstSource, out firstSchema); // get sources listed in map

			if (connDict == null || connDict.Count == 0) return null; // no sources found

			foreach (string connName in connDict.Keys)
			{
				try
				{
					//if (Lex.Eq(connName, "DEV857")) mt.Name = mt.Name; // debug
					conn = DbConnectionMx.GetConnection(connName);
				}

				catch (MobiusConnectionOpenException ex)
				{
					foreach (DbSchemaMx schema in connDict[connName].Values) // return first schema for connection
						return schema;
				}

				if (conn == null) throw new Exception("Can't get connection");
				conn.Close();
			}

			return null; // all ok
		}

/// <summary>
/// Check connection accessibility for a single schema
/// </summary>
/// <param name="schemaName"></param>
/// <returns>Schema object that failed or null if ok</returns>

		public static DbSchemaMx CheckDataSourceAccessibility(
			string schemaName)
		{
			schemaName = schemaName.ToUpper(); // stored as upper case
			if (!DataSourceMx.Schemas.ContainsKey(schemaName))
				throw new Exception("Schema not found: " + schemaName);

			string connName = DataSourceMx.Schemas[schemaName].DataSourceName;
			DbConnectionMx conn = null;
			try { conn = DbConnectionMx.GetConnection(connName); }
			catch (Exception ex)
			{
				return DataSourceMx.Schemas[schemaName];
			}
			conn.Close();
			return null; // all ok
		}

/// <summary>
/// Transform a field value after retrieval but prior to formatting
/// </summary>
/// <param name="qc">Associated QueryColumn</param>
/// <param name="initialValue">Initial data value</param>
/// <param name="vo">value object array of associated row</param>
/// <returns>Transformed data value</returns>

		public virtual object TransformData (
			QueryColumn qc,
			object initialValue, 
			object[] vo)
		{
			return initialValue; // return as is

#if false // move to plugin if reactivated later
			int imgId = 0;
			Bitmap bmp = null;

			MetaColumn mc = metaColumn;
			if (initialValue == null) return null;

			if (Lex.Eq(mc.DataTransform, "GetImageFromIvdsResultId"))
			{
				try { imgId = Convert.ToInt32(initialValue); }
				catch (Exception ex) { return null; }
				if (imgId <= 0) return null;

				string sql = // get images from normalized & derived tables
					"select img_typ_nm, img_cntnt " +
					"from ivm_owner.IVM_rslt_IMG ri " + 
					"where ivd_rslt_id = :0";

				try
				{
					DbCommandMx drd = new DbCommandMx();
					drd.PrepareQuery(sql, DbType.Int32);
					OracleDataReader dr = drd.ExecuteReader(imgId);

					while (ReadRow(drd)
					{
						if (drd.IsNull(0) || drd.IsNull(1)) return null;
						string imgTypeName = drd.GetString(0);
						if (Lex.Ne(imgTypeName, "jpg") &&
						Lex.Ne(imgTypeName, "png") &&
						Lex.Ne(imgTypeName, "svg") &&
						Lex.Ne(imgTypeName, "gif")) continue;
						OracleBlob blob = dr.GetOracleBlob(1);
						bmp = new Bitmap(new MemoryStream(blob.Value));
						break;
					}

					drd.Dispose();
					return bmp;
				}
				catch (Exception ex)
				{
					DebugLog.Message("GetImageFromIvdsResultId Exception:\n" + sql + "\n" + ex.Message);
					return null;
				}
			}

			else return initialValue; // return as is if transform not recognized
#endif			
		}

		/// <summary>
		/// Return true if this table's results can be built from other retrieved data
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public virtual bool CanBuildDataFromOtherRetrievedData(
			QueryEngine qe,
			int ti)
		{
			return false;
		}

		/// <summary>
		/// Return true if broker can build a standard form unpivoted assay data query
		/// </summary>
		/// <returns></returns>

		public virtual bool CanBuildUnpivotedAssayResultsSql
		{
			get { return false; }
		}

		/// <summary>
		/// Build sql to select unpivoted assay results for the UnpivotedAssayMetaBroker
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public virtual string BuildUnpivotedAssayResultsSql(
			ExecuteQueryParms eqp)
		{
			throw new NotImplementedException(); // noop
		}

		/// <summary>
		/// Build sql to select unpivoted assay results for the TargetAssayMetaBroker (Older version)
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="innerExprs"></param>
		/// <param name="innerTables"></param>
		/// <param name="innerCriteria"></param>

		public virtual void BuildUnpivotedAssayResultsSqlOld(
			ExecuteQueryParms eqp,
			string innerExprs,
			string innerTables,
			string innerCriteria,
			ref string fromClause)
		{
			return; // noop
		}

/// <summary>
/// Convert a database result value to an object reference
/// </summary>
/// <param name="v"></param>
/// <returns></returns>

		public virtual string ConvertValueToImageReference(object v)
		{
			if (v == null) return null;
			else return v.ToString();
		}

/// <summary>
/// Check if two metacolumns are the same or have equivalent values
/// </summary>
/// <param name="mc1"></param>
/// <param name="mc2"></param>
/// <returns></returns>

		public virtual bool ColumnValuesAreEquivalent(
			MetaColumn mc1,
			MetaColumn mc2)
		{
			bool sameCol = Lex.Eq(mc1.Name, mc2.Name); // check if columns match on name
			return sameCol;
		}

		/// <summary>
		/// Build a set of rows from other table data in the buffer
		/// </summary>

		public virtual void BuildDataFromOtherRetrievedData(
			QueryEngine qe,
			int ti,
			List<object[]> results,
			int firstResult,
			int resultCount)
		{
			throw new Exception("Unimplemented BuildRowsFromOtherTableData call");
		}

/// <summary>
/// Return true if table allows temp db tables to be used for key lists
/// </summary>
/// <param name="qt"></param>
/// <returns></returns>

		public virtual bool AllowTempDbTableKeyLists(
			QueryTable qt)
		{
			return true; // todo
		}

/// <summary>
/// Get the next vo from the MultipivotRowDict
/// </summary>
/// <returns></returns>

		public object[] GetNextMultipivotRowDictVo()
		{
			object[] vo;
			string key = null;

			if (MultipivotRowDict == null || MultipivotRowDict.Count == 0) return null;

			Dictionary<string, object[]>.KeyCollection keys = MultipivotRowDict.Keys;
			foreach (string key0 in MultipivotRowDict.Keys)
			{
				key = key0;
				break;
			}

			vo = MultipivotRowDict[key];
			MultipivotRowDict.Remove(key);
			return vo;
		}

		/// <summary>
		/// Return depth of BuildSql calls across all brokers
		/// </summary>
		/// <returns></returns>

		public static int GetBuildSqlDepth()
		{
			int depth = 0;

			StackTrace trace = new StackTrace();
			foreach (StackFrame sf in trace.GetFrames())
			{
				if (Lex.Eq(sf.GetMethod().Name, "BuildSql")) depth++;
			}

			return depth;
		}

		/// <summary>
		/// There may be sme sceanrios where it makes sense to change the way a query is executed.  For example, change the query from
		/// a parameterized query to a query using literals (may need rewriting to be smarter).
		/// </summary>
		/// <param name="table"></param>

		public void SetKeyListPredicate()
		{
			if (Qt.HasDateCriteria)
			{
				KeyListPredType = KeyListPredTypeEnum.Literal;
			}
		}

	}

	/// <summary>
	/// Data for pivoting of multiple tables per SQL statement is stored for each metabroker type.
	/// This information is accessed via a QueryEngine MetaBrokerStateInfo field
	/// which is a dictionary keyed by MetaBrokerType (possibly including summarization level). 
	/// Dictionary values are MultipivotBrokerTypeData instances for the broker type.
	/// </summary>

	public class MultiTablePivotBrokerTypeData
	{
		public string FirstTableName; // first table to pivot
		public Dictionary<string, object> MbInstances; // Dictionary of metatable name to metabroker instances (single or list)
		public Dictionary<string, MetaTable> TableFilterValuesToMetaTableDict; // Dictionary of TableFilterValues to associated MetaTables (PivotMetaBroker)
		public StringBuilder TableCodeCsvList; // list of table codes to retrieve in csv sql format
		public Dictionary<string, MpdResultTypeData> TableCodeDict; // hashed set of table codes
		public StringBuilder TableCodeCsvList2; // secondary list of codes to retrieve in sql format
		public Dictionary<string, MpdResultTypeData> TableCodeDict2; // secondary dictionary of codes
		public Dictionary<int, int> CodeAliasDict; // map between alternate codes for a table

		public MetaTable FirstMetaTable {
			get { return MetaTableCollection.Get(FirstTableName); }}

/// <summary>
/// Return multipivot data for specified broker key
/// </summary>
/// <param name="mbsi"></param>
/// <param name="mpGroupKey"></param>
/// <param name="mtName"></param>
/// <returns></returns>

		public static MultiTablePivotBrokerTypeData GetMultiPivotData(
			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi,
			string mpGroupKey,
			string mtName)
		{
			MultiTablePivotBrokerTypeData mpd;

			if (!mbsi.ContainsKey(mpGroupKey))
			{
				mpd = new MultiTablePivotBrokerTypeData();
				mbsi[mpGroupKey] = mpd;
				mpd.FirstTableName = mtName;
				mpd.TableCodeCsvList = new StringBuilder();
				mpd.TableCodeDict = new Dictionary<string, MpdResultTypeData>();
				mpd.MbInstances = new Dictionary<string, object>();
			}
			else mpd = (MultiTablePivotBrokerTypeData)mbsi[mpGroupKey];
			return mpd;
		}

/// <summary>
/// Add another MetaBroker for the specified metatable
/// </summary>
/// <param name="mtName"></param>
/// <param name="mb"></param>

		public void AddMetaBroker(
			string mtName,
			GenericMetaBroker mb)
		{
			List<GenericMetaBroker> mbList;

			//if (Lex.Eq(mtName, "star_41481")) mtName = mtName; // debug;

			if (!MbInstances.ContainsKey(mtName)) // first broker for this table?
				MbInstances[mtName] = mb;
			else
			{
				if (MbInstances[mtName] is GenericMetaBroker)
				{ // need to convert to List of brokers?
					mbList = new List<GenericMetaBroker>();
					mbList.Add((GenericMetaBroker)MbInstances[mtName]);
					MbInstances[mtName] = mbList;
				}
				else mbList = MbInstances[mtName] as List<GenericMetaBroker>;
				mbList.Add(mb);
			}
			return;
		}

/// <summary>
/// Get first or only metabroker and any associated list of brokers
/// </summary>
/// <param name="mtName"></param>
/// <param name="mbList"></param>
/// <returns></returns>

		public IMetaBroker GetFirstBroker(
			string mtName,
			out List<GenericMetaBroker> mbList)
		{
			object mbInstancesObject = null;

			if (MbInstances.ContainsKey(mtName))
				mbInstancesObject = MbInstances[mtName];

			else	throw new Exception(mtName + " not found");

			return GetFirstBroker(mbInstancesObject, out mbList);
		}

/// <summary>
/// Get first or only metabroker and any associated list of brokers
/// </summary>
/// <param name="mbInstancesObject"></param>
/// <param name="mbList"></param>
/// <returns></returns>

		public static IMetaBroker GetFirstBroker(
			object mbInstancesObject,
			out List<GenericMetaBroker> mbList)
		{
			IMetaBroker mb;
			mbList = null;
			if (mbInstancesObject is GenericMetaBroker)
				mb = (IMetaBroker)mbInstancesObject; // broker assoc w/table
			else
			{
				mbList = (List<GenericMetaBroker>)mbInstancesObject;
				mb = (IMetaBroker)mbList[0];
			}

			return mb;
		}

/// <summary>
/// Clear the multipivot buffers
/// </summary>

		public void ClearBuffers()
		{
			List<GenericMetaBroker> mbList;
			GenericMetaBroker mb;

			foreach (object o1 in MbInstances.Values)
			{ // clear any existing buffered rows
				int mbIdx = 0;
				mb = (GenericMetaBroker)GetFirstBroker(o1, out mbList);
				while (true)
				{
					mb.MultipivotRowDict = null;
					mb.MultipivotRowList = null;
					if (mbList == null) break; // single broker
					mbIdx++; // go to next broker
					if (mbIdx >= mbList.Count) break; // at end of brokers?
					mb = mbList[mbIdx];
				}
			}
		}
	}

/// <summary>
/// Multipivot selected pivoted column result type data for a metatable
/// </summary>

	public class MpdResultTypeData
	{
		public HashSet<string> ResultCodeSet = new HashSet<string>(); // hashed set of result type codes
	}
}
