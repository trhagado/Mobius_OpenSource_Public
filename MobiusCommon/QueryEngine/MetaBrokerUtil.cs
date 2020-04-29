using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Common MetaBroker utility operations
	/// </summary>

	public class MetaBrokerUtil
	{

		/// <summary>
		/// Static globally used broker instances for broker-generic methods, one per broker type
		/// </summary>

		public static IMetaBroker[] GlobalBrokers = null;

		/// <summary>
		/// Get metabroker object for metatable & initialize
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static IMetaBroker GetGlobalBroker(MetaBrokerType mbType)
		{
			if (mbType == MetaBrokerType.Unknown) return null;

			if (GlobalBrokers == null) return null;

			int gbi = (int)mbType;

			if (gbi < 0 || gbi > GlobalBrokers.Length) return null;


			IMetaBroker mb = GlobalBrokers[gbi];
			return mb;
		}

		/// <summary>
		/// Return true if global broker type exists
		/// </summary>
		/// <param name="mbType"></param>
		/// <returns></returns>
		/// 
		public static bool GlobalBrokerExists(
			MetaBrokerType mbType)
		{
			if (GlobalBrokers == null) return false;
			if (mbType <= MetaBrokerType.Unknown ||
				mbType > MetaBrokerType.MaxBrokerType) return false;

			if (GlobalBrokers[(int)mbType] == null) return false;

			return true;
		}

		/// <summary>
		/// Create broker of specified type
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static GenericMetaBroker Create(
			MetaBrokerType type)
		{
			GenericMetaBroker mb = null;

			if (type == MetaBrokerType.Generic)
				return new GenericMetaBroker();

			if (type == MetaBrokerType.Pivot)
			{
				PivotMetaBroker pivotMb = new PivotMetaBroker();
				mb = (PivotMetaBroker)pivotMb;
				return mb;
			}

			if (type == MetaBrokerType.Annotation)
			{
				AnnotationMetaBroker aMb = new AnnotationMetaBroker();
				mb = (GenericMetaBroker)aMb;
				return mb;
			}

			if (type == MetaBrokerType.CalcField)
			{
				CalcFieldMetaBroker cfMb = new CalcFieldMetaBroker();
				mb = (GenericMetaBroker)cfMb;
				return mb;
			}

			if (type == MetaBrokerType.CalcTable)
			{
				CalcTableMetaBroker ctMb = new CalcTableMetaBroker();
				mb = (GenericMetaBroker)ctMb;
				return mb;
			}

			if (type == MetaBrokerType.MultiTable)
			{
				MultiTableMetaBroker mtMb = new MultiTableMetaBroker();
				mb = (GenericMetaBroker)mtMb;
				return mb;
			}

			if (type == MetaBrokerType.TargetAssay)
			{
				QueryEngineLibrary.MultiDbAssayMetaBroker taMb = new MultiDbAssayMetaBroker();
				mb = (GenericMetaBroker)taMb;
				return mb;
			}

			if (type == MetaBrokerType.Assay)
			{
				AssayMetaBroker assayMb = new AssayMetaBroker();
				mb = (GenericMetaBroker)assayMb;
				return mb;
			}

			if (type == MetaBrokerType.UnpivotedAssay)
			{
				UnpivotedAssayMetaBroker urMb = new UnpivotedAssayMetaBroker();
				mb = (GenericMetaBroker)urMb;
				return mb;
			}

			if (type == MetaBrokerType.SpotfireLink)
			{
				SpotfireLinkMetaBroker slMb = new SpotfireLinkMetaBroker();
				mb = (GenericMetaBroker)slMb;
				return mb;
			}

			if (type == MetaBrokerType.NoSql)
			{
				NoSqlMetaBroker nsMb = new NoSqlMetaBroker();
				mb = (GenericMetaBroker)nsMb;
				return mb;
			}

			if (type == MetaBrokerType.RgroupDecomp)
			{
				RgroupMetaBroker rgMb = new RgroupMetaBroker();
				mb = (GenericMetaBroker)rgMb;
				return mb;
			}

			throw new QueryException("Unknown metabroker type " + type.ToString());
		}

		/// <summary>
		/// Return true if this metatable maps to a sql source
		/// </summary>

		public static bool IsSqlMetaTable(MetaTable mt)
		{
			return MetaBrokerUtil.IsSqlMetaBroker(mt.MetaBrokerType);
		}

		/// <summary>
		/// Return true if this metatable maps to a non-SQL source
		/// </summary>

		public static bool IsNonSqlMetaTable(MetaTable mt)
		{
			return MetaBrokerUtil.IsNonSqlMetaBroker(mt.MetaBrokerType);
		}

		/// <summary>
		/// Return true if this metabroker operates against a SQL source
		/// </summary>

		public static bool IsSqlMetaBroker(MetaBrokerType mbt)
		{
			bool isSql = !IsNonSqlMetaBroker(mbt);
			return isSql;
		}

		/// <summary>
		/// Return true if this metabroker operates against a non-sql SQL source
		/// </summary>

		public static bool IsNonSqlMetaBroker(MetaBrokerType mbt)
		{
			bool isNonSql = (mbt == MetaBrokerType.NoSql);
			return isNonSql;
		}

		/// <summary>
		/// Initialize the set of global brokers
		/// </summary>

		public static void InitializeGlobalBrokers()
		{
			Array brokerList = Enum.GetValues(typeof(MetaBrokerType));
			int maxBroker = 0;
			bool b;

			foreach (int enumValue in brokerList)
			{
				if (enumValue > maxBroker) maxBroker = enumValue;
			}
			GlobalBrokers = new IMetaBroker[maxBroker + 1];
			foreach (int enumValue in brokerList)
			{
				if (enumValue == (int)MetaBrokerType.Unknown) continue;
				GlobalBrokers[enumValue] = MetaBrokerUtil.Create((MetaBrokerType)enumValue);
				if (enumValue > maxBroker) maxBroker = enumValue;
			}
		}

		/// <summary>
		/// Return true if table allows the use of temp database table key lists
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static bool AllowTempDbTableKeyLists(
			QueryTable qt)
		{
			MetaTable mt = qt.MetaTable;
			if (mt == null) return false;

			if (mt.MetaBrokerType == MetaBrokerType.Unknown ||
				!GlobalBrokerExists(mt.MetaBrokerType)) return false;

			IMetaBroker mb = GlobalBrokers[(int)mt.MetaBrokerType];
			return mb.AllowTempDbTableKeyLists(qt);
		}

		/// <summary>
		/// Get the name of the SQL statement group that this table should be included in for the purposes of executing searches
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public static string GetTableCriteriaGroupName(QueryTable qt)
		{
			MetaTable mt = null;
			IMetaBroker imb = null;
			string groupName = null;

			try
			{
				mt = qt.MetaTable;
				imb = MetaBrokerUtil.GetGlobalBroker(mt.MetaBrokerType);
				groupName = imb.GetTableCriteriaGroupName(qt).ToUpper();
				return groupName;
			}
			catch (Exception ex)
			{
				DebugLog.Message(ex);
				return "";
			}

		}
	}

	/// <summary>
	/// IMetabroker contains the definition of the interface for metabroker methods.
	/// A broker can be used in multiple ways including:
	/// 1. Building the sql associated with a single query table. The Query Engine
	///    will merge these individual sql statements in to a complete sql statement.
	/// 2. The Generic metabroker is then called to execute the merged query and to
	///    return the hit list.
	/// 3. A metabroker is then created and called for each query table to retrieve 
	///    the data for that table using the hitset.
	/// 4. The Calculated Field will create and call the associated brokers
	///    to search and retrieve the values associated with the field.
	/// </summary>

	public interface IMetaBroker
	{

		/// <summary>
		/// Return true if a description of the specified table is available
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		bool TableDescriptionIsAvailable(
			MetaTable mt);

		/// <summary>
		/// Return description for table.
		/// May be html or simple text
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		TableDescription GetTableDescription(
			MetaTable mt);

		/// <summary>
		/// Return true if criteria from this table are not used for searching but
		/// are used for parameters & should be ignored.
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		bool IgnoreCriteria(
			MetaTable mt);

		/// <summary>
		/// Return true if table should be checked/transformed at presearch time
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		bool ShouldPresearchCheckAndTransform(
			MetaTable mt);

		/// <summary>
		/// Prepare query
		/// </summary>
		/// <param name="parms"></param>

		string PrepareQuery(
			ExecuteQueryParms eqp);

		/// <summary>
		/// Generate the sql for retrieval from a query table
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		string BuildSql(
			ExecuteQueryParms eqp);

		/// <summary>
		/// Execute query in preparation for retrieving rows
		/// </summary>
		/// <param name="parms"></param>

		void ExecuteQuery(
			ExecuteQueryParms parms);

		/// <summary>
		/// Retrieve next result row
		/// </summary>
		/// <returns></returns>

		object[] NextRow();

		/// <summary>
		/// Return true if this table's results can be built from other retrieved data
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		bool CanBuildDataFromOtherRetrievedData(
			QueryEngine qe,
			int ti);

		/// <summary>
		/// Build a set of rows from other table data in the buffer
		/// </summary>

		void BuildDataFromOtherRetrievedData(
			QueryEngine qe,
			int ti,
			List<object[]> results,
			int firstResult,
			int resultCount);

		/// <summary>
		/// Return true if broker can build a standard form unpivoted assay data query
		/// </summary>
		/// <returns></returns>

		bool CanBuildUnpivotedAssayResultsSql
		{
			get;
		}

		/// <summary>
		/// Build sql to select unpivoted assay results for the UnpivotedAssayMetaBroker
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		string BuildUnpivotedAssayResultsSql(
			ExecuteQueryParms eqp);

		/// <summary>
		/// Build sql to select unpivoted assay results for the TargetAssayMetaBroker
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="innerExprs"></param>
		/// <param name="innerTables"></param>
		/// <param name="innerCriteria"></param>

		void BuildUnpivotedAssayResultsSqlOld(
			ExecuteQueryParms eqp,
			string innerExprs,
			string innerTables,
			string innerCriteria,
			ref string fromClause);

		/// <summary>
		/// Get parameters used to execute query
		/// </summary>

		ExecuteQueryParms GetExecuteQueryParms
		{
			get;
		}

		/// <summary>
		/// Close query
		/// </summary>
		void Close();


		/// <summary>
		/// Get a pivoted vo given an unpivoted result identifier
		/// </summary>
		/// <param name="mti"></param>
		/// <param name="voi"></param>
		/// <param name="voOffset"></param>
		/// <param name="mt2"></param>
		/// <param name="vo2"></param>
		/// <returns></returns>

		bool GetPivotedResult(
			MetaTable mti,
			object[] voi,
			int voOffset,
			ref MetaTable mt2,
			ref object[] vo2);

		/// <summary>
		/// Build query that returns detailed drilldown data on a result
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string linkInfo);

		/// <summary>
		/// Build query that returns detailed drilldown data on a result
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="metaColumnId"></param>
		/// <param name="level"></param>
		/// <param name="linkInfo"></param>
		/// <returns></returns>

		Query GetDrilldownDetailQuery(
			MetaTable mt,
			string metaColumnId,
			int level,
			string linkInfo);

		/// <summary>
		/// Do presearch initialization for a QueryTable
		/// </summary>
		/// <param name="qt"></param>

		void DoPresearchInitialization(
			QueryTable qt);

		/// <summary>
		/// Do presearch transform of table & add to query
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="q"></param>

		void DoPreSearchTransformation(
			Query originalQuery,
			QueryTable qt,
			Query newQuery);

		/// <summary>
		/// Convert a multipivot table into a set of tables where data exists for
		/// one or more of the compound identifiers in the list.
		/// </summary>
		/// <param name="qt">Current form of query table</param>
		/// <param name="q">Query to add remapped tables to</param>
		/// <param name="ResultKeys">Keys data will be retrieved for</param>

		void ExpandToMultipleTables(
			QueryTable qt,
			Query q,
			List<string> ResultKeys);

		/// <summary>
		/// Get metacolumn specific conditional formatting
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="qn"></param>
		/// <returns></returns>

		CondFormat GetMetaColumnConditionalFormatting(
			MetaColumn mc,
			QualifiedNumber qn);

		/// <summary>
		/// Get metacolumn specific conditional formatting colors
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="qn"></param>
		/// <param name="foreColor"></param>
		/// <param name="backColor"></param>
		/// <returns></returns>

		bool GetMetaColumnConditionalFormattingCellColors(
			MetaColumn mc,
			QualifiedNumber qn,
			out Color foreColor,
			out Color backColor);

		/// <summary>
		/// Get an concentration response curve or other image
		/// </summary>
		/// <param name="metaColumn"></param>
		/// <param name="resultId"></param>
		/// <param name="desiredWidth"></param>
		/// <returns></returns>

		Bitmap GetImage(
			MetaColumn metaColumn,
			string resultId,
			int desiredWidth);

/// <summary>
/// Get additional data from the broker
/// </summary>
/// <param name="command"></param>
/// <returns></returns>

		object GetAdditionalData(
			string command);

		/// <summary>
		/// Return null if data source is available otherwise return Schema of 1st unavailable source for table
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		DataSchemaMx CheckDataSourceAccessibility(
			MetaTable mt);

		/// <summary>
		/// Transform a field value after retrieval but prior to formatting
		/// </summary>
		/// <param name="qc">Associated QueryColumn</param>
		/// <param name="initialValue">Initial data value</param>
		/// <param name="vo">value object array of associated row</param>
		/// <returns>Transformed data value</returns>

		object TransformData(
			QueryColumn qc,
			object initialValue,
			object[] vo);

		/// <summary>
		/// Return true if table allows temp db tables to be used for key lists
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		bool AllowTempDbTableKeyLists(
			QueryTable qt);

		/// <summary>
		/// Get the name of the SQL statement group that this table should be included in for the purposes of executing searches
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		string GetTableCriteriaGroupName(
			QueryTable qt);
	}

	/// <summary>
	/// Parameters passed to and returned from RunQuery method
	/// </summary>

	public class ExecuteQueryParms
	{
		public QueryEngine Qe; // query engine instance
		public QueryTable QueryTable; // query table to search over 
		public string CallerSuppliedSql = ""; // if nonblank use this sql instead of building and using our own SQL
		public string CallerSuppliedCriteria = ""; // criteria to include in where clause
		public string KeyName = ""; // qualified name used in sql for key field
		public List<string> SearchKeySubset; // list of keys to limit query to
		public SortOrder KeySortDirection; // sort order to return keys in
		public bool ReturnQNsInFullDetail = true; // return QualifiedNumbers in full detail
		public bool ReturnPrimitiveDataTypesOnly { get { return !ReturnQNsInFullDetail; } } // return Qualified Numbers in simple numeric form
		public bool AllowNetezzaUse = true; // allow use of warehouse (Netezza) by default if other conditions hold
		public ICheckForCancel CheckForCancel; // method to call to check to see if cancelled
		public bool Cancelled; // flag set if cancelled

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ExecuteQueryParms(
			QueryEngine qe)
		{
			Qe = qe;
		}

		/// <summary>
		/// Construct with intitial query table
		/// </summary>
		/// <param name="qt"></param>

		public ExecuteQueryParms(
			QueryEngine qe,
			QueryTable qt)
		{
			Qe = qe;
			QueryTable = qt;
		}

		/// <summary>
		/// Clone ExecuteQueryParms
		/// </summary>
		/// <returns></returns>

		public ExecuteQueryParms Clone()
		{
			ExecuteQueryParms eqp = (ExecuteQueryParms)this.MemberwiseClone();
			return eqp;
		}


	}

}
