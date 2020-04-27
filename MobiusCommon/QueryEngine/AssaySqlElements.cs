using Mobius.ComOps;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// SQL elements used in building full SQL for querying assay data
	/// </summary>

	public class AssaySqlElements
	{
		public static string KeySchemaName = "<keySchemaName>";

		public static string Assay = "<assay>"; // Assay info
		public static string ResultType = "<sql>"; // Result type info


		public static string SubmissionIdLotIdCidId = "<submissionIdLotIdCidId>"; // links all submission ids for molecules to lots and cids

		public static string DetailedResultsTableName = "<>"; // main table of unsummarized results

		public static string ActivityResultsTableName = "<>"; // contains well-level activities that went into an IC50/EC50 calculation if available

		public static string CidWmSubqueryName = "cid_wm_assay_subquery"; // integrated subquery to get compound and well material columns
		public static string RsltSubqueryName = "rslt_assay_subquery"; // integrated subquery of detailed and activity unsummarized results
		public static string SubqueryNameSuffix = "_assay_subquery"; // suffix for all subqueries

		public static string Run = "<assayRunTable>";

// Common Assay Attributes (CMN_ASSY_ATRBTS) elements
// Note that assy_gene_cnt is > 0 for 1st gene negative for others or zero is no associated genes
// For assays that don't have any key types CMN_ASSY_ATRBTS contains a single row with rslt_typ_id_nbr = null

		public static string CaaResultTypeAttrTable = "<column>"; // common result type attributes
		public static string CaaOuterJoinExpr = "(+) /*caa.oj*/"; // this expression marks outer joins which can be removed to get just the results for key result types
		public static string CaaResultTypeAttrJoinCriteria = /* all key results (first gene only) plus non-key results across all genes */
			@"<sql>";

		public static string CaaAssayAttrTable { get { return CaaResultTypeAttrTable + "2"; } } // local copy of common assay attributes
		public static string CaaAssayAttrJoinCriteria = /* top level results only for first gene or only gene associated with the assay */
			@"<sql>";

		public static string CaaGeneAttrTable { get { return CaaResultTypeAttrTable + "3"; } } // common gene attributes
		public static string CaaGeneAttrJoinCriteria = /* top level results only for all genes associated with the assay (dup result values) */
			@"<sql>";

		/// <summary>
		/// Adjust sql expression to retrieved summarized columns
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string AdjustForSummarizedView(string sql)
		{
			string sql2 = sql;
			sql2 = Lex.Replace(sql2, "prfx_txt", "mean_prfx_txt");
			sql2 = Lex.Replace(sql2, "value_nbr", "mean_value_nbr");
			sql2 = Lex.Replace(sql2, "rslt_units", "rslt_mean_units");
			return sql2;
		}

// Calculate activity class for SP & CRC results
// SP results are in % units
// CRC results are normally in uM or nM units

		public static string ActivityClassSqlExpression = @" 
	   (case
       when caa.rslt_typ = 'SP' then (case
			  when prfx_txt = '<' then 'Fail'
        when value_nbr<70 then 'Fail'
        when value_nbr<90 then 'BorderLine' 
        when value_nbr>=90 then 'Pass'  
        else null
        end)
       when caa.rslt_typ = 'CRC' then 
       (case
        when lower(rslt_units) = 'nm' then
				(case
					when prfx_txt = '>' then 'Fail'
					when value_nbr> 10*1000. then 'Fail'
					when value_nbr> 5*1000. then 'Fail' 
					when value_nbr> 1*1000. then 'BorderLine' 
					when value_nbr> 0.5*1000. then 'BorderLine' 
					when value_nbr> 0.1*1000. then 'Pass' 
					when value_nbr> 0.01*1000. then 'Pass' 
					when value_nbr<= 0.01*1000. then 'Pass' 
					else null
				end)
        when lower(rslt_units) = 'um' then
				(case
					when prfx_txt = '>' then 'Fail'
					when value_nbr> 10 then 'Fail'
					when value_nbr> 5 then 'Fail' 
					when value_nbr> 1 then 'BorderLine' 
					when value_nbr> 0.5 then 'BorderLine' 
					when value_nbr> 0.1 then 'Pass' 
					when value_nbr> 0.01 then 'Pass' 
					when value_nbr<= 0.01 then 'Pass' 
					else null
				end)
        when lower(rslt_units) = 'mm' then
				(case
					when prfx_txt = '>' then 'Fail'
					when value_nbr> 10/1000. then 'Fail'
					when value_nbr> 5/1000. then 'Fail' 
					when value_nbr> 1/1000. then 'BorderLine' 
					when value_nbr> 0.5/1000. then 'BorderLine' 
					when value_nbr> 0.1/1000. then 'Pass' 
					when value_nbr> 0.01/1000. then 'Pass' 
					when value_nbr<= 0.01/1000. then 'Pass' 
					else null
				end)
        else null
        end)
       else null
       end)";

// Calculate activity bin for SP & CRC results

		public static string ActivityBinSqlExpression = @" 
	   (case
       when caa.rslt_typ = 'SP' then (case
			  when prfx_txt = '<' then 0
        when value_nbr<70 then 1 
        when value_nbr<90 then 4 
        when value_nbr>=90 then 5  
        else null
        end)
       when caa.rslt_typ = 'CRC' then (case
			  when prfx_txt = '>' then 0
        when value_nbr>10 then 1
        when value_nbr>5 then 4 
        when value_nbr>1 then 5 
        when value_nbr>0.5 then 7 
        when value_nbr>0.1 then 8 
        when value_nbr>0.01 then 9 
        when value_nbr<=0.01 then 10 
        else null
        end)
       else null
       end)";

/// <summary>
/// Subquery to get Compound and Well Material fields
/// For queries that contain lists of CIDS the materialize hint should be activated (i.e. add the "+") which is much more efficient than
/// combining this and the detail/activity table subqueries into a single subquery
/// </summary>

		public const string CidWmSubquerySql = 
			@"<sql>";

		public const string DetailTableSubquerySql =
			@"<sql>";

		/// <summary>
		/// Basic sql for activity and activity group tables that can be unioned with the detail table as needed
		/// Incld_in_smrzd_cd and aprvl_sts_cd only exist in the detail table and are returned as null here
		/// </summary>

		public const string ActivityTableSubQuerySql =
			@"<sql>";

		// Basic sql for summarized results table
		// Allows assayIdlist and CorpId list to be inserted and materialize hint to be activated

		public static string SmrzdTableSql =
			@"<sql>";

		// Criteria substitution strings for DetailTableSql and ActvtyTableSql
		// Note that substituting these criteria here is much faster for pivoted views 
		// than substituting elsewhere.

		public static string UnsummarizedInnerCorpIdCriteria = " and c.,<cid> in (<list>) ";

// Criteria substitution strings for SmrzdTableSql
// Note that substituting these criteria here is much faster for pivoted views 
// than substituting elsewhere.

		public static string SummarizedInnerAssayCriteria = " and <assay_id> in (<assayIdList>?) ";
		public static string SummarizedInnerCidCriteria = " and <cid> in (<list>) ";

		// Sql that returns the set of assay ids and associated gene ids and symbols from assay metadata tables

		const string AssayTargetGeneSql =
			@"<sql>";

	}
}
