using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// Broker for Mobius Annotation tables
	/// It uses TableName to determine the table to operate on
	/// </summary>

	public class AnnotationMetaBroker : GenericMetaBroker
	{
		protected string TableName = "mbs_owner.mbs_adw_rslt"; // owner-qualified name of table containing data
		protected string SeqName = "mbs_owner.mbs_adw_rslt_seq"; // owner-qualified name of sequence to use


		List<object> BufferedRows = null; // buffered retrieved rows of data for this broker
		string FetchSql = null; // sql to fetch data to be pivoted
		bool IsNumericKey { get { return KeyMc.IsNumeric; } } // true if key col value is numeric and should come from ext_cmpnd_id_nbr
		bool IsTextKey { get { return !KeyMc.IsNumeric; } } // true if key col value is text and should come from ext_cmpnd_id_txt

		int PivotedColSelectCount = 0; // number of pivoted columns returned (selected or sorted)
		QueryColumn FirstPivotedColSelected = null; // first pivoted column selected

		int PivotedColCriteriaCount = 0; // number of pivoted columns with criteria
		QueryColumn FirstPivotedColWithCriteria = null; // first pivoted column with criteria used for query optimization

		StringBuilder ResultCodeList = null; // CSV list of selected result codes
		HashSet<string> ResultCodeSet = null; // hash set of selected result codes

		// Global statics

		public static bool AllowMultiTablePivot_Annotation = true; // if true, only pivot a single table in code at a time. This seems to be faster than multipivot
		public static int UniqueGroupId = 0; // to provide unique row grouping when not in DB
		public static int MultipivotExecuteQueryCount, MultipivotExecuteQueryTime, MultipivotExecuteQueryAvgTime; // performance data

		/// <summary>
		/// Constructor
		/// </summary>

		public AnnotationMetaBroker()
		{
			return;
		}

		/// <summary>
		/// Get the name of the SQL statement group that this table should be included in for the purposes of executing searches
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override string GetTableCriteriaGroupName(
			QueryTable qt)
		{
			string[] sa = TableName.Split('.'); 
			if (sa.Length > 0) return sa[0]; // i.e. MBS_OWNER
			else return "";
		}

		/// <summary>
		/// Build the sql for a query doing preparation for separate fetch of data.
		/// </summary>
		/// <param name="parms"></param>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			MultiTablePivotBrokerTypeData mpd; // multipivot data for this broker type
			int tableId;
			bool summary;

			Eqp = eqp;
			Qt = eqp.QueryTable;
			MetaTable mt = Qt.MetaTable;

			if (!eqp.ReturnQNsInFullDetail || // no multipivot if part of calc field
			eqp.Qe == null || // need to be able to access queryengine info
			(eqp.SearchKeySubset == null && !eqp.Qe.Query.Preview && !BuildSqlOnly)) // no multipivot unless have subset and not preview and not just building sql
			//!QueryEngine.AllowMultiTablePivot) // is multipivot even allowed
				return base.PrepareQuery(eqp);

			InitializeColumnInfo(eqp); // setup SelectList & col info

			if (PivotedColSelectCount == 0 || // must have at least one column to pivot
				PivotedColCriteriaCount > 1) // and no more than one non-key criteria
				 return base.PrepareQuery(eqp);

			// Store pivot info for queryTable

			PivotInCode = true;
			MpGroupKey = MetaBrokerType.Annotation.ToString(); // key for broker/detail level

			if (!AllowMultiTablePivot_Annotation || PivotedColCriteriaCount > 0)
				MpGroupKey += "_" + Qt.MetaTable.Name;

			if (eqp.Qe.MetaBrokerStateInfo == null)
				eqp.Qe.MetaBrokerStateInfo = new Dictionary<string, MultiTablePivotBrokerTypeData>();

			mpd = MultiTablePivotBrokerTypeData.GetMultiPivotData(eqp.Qe.MetaBrokerStateInfo, MpGroupKey, mt.Name);

			MetaTable.ParseMetaTableName(mt.Name, out tableId, out summary); // get the annotation table id

			string code = tableId.ToString();
			if (code != "" && !mpd.TableCodeDict.ContainsKey(code)) // include assay code
			{
				mpd.TableCodeDict[code] = new MpdResultTypeData(); // add key to hash list
				if (mpd.TableCodeCsvList.Length > 0) mpd.TableCodeCsvList.Append(",");

				mpd.TableCodeCsvList.Append(code);
			}

			HashSet<string> tch = mpd.TableCodeDict[code].ResultCodeSet;
			foreach (string rsltCode in ResultCodeSet) // add selected codes for this metatable
			{
				if (!tch.Contains(rsltCode)) tch.Add(rsltCode);
			}

			mpd.AddMetaBroker(mt.Name, this);

			return null; // sql built later
		}

		/// <summary>
		/// Read in data, pivot & buffer for supplied set of rows.
		/// This is called for retrieval only, not for search
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{

			if (!PivotInCode)
			{
				base.ExecuteQuery(eqp);
				return;
			}

			int t0 = TimeOfDay.Milliseconds();

			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = eqp.Qe.MetaBrokerStateInfo;
			MultiTablePivotBrokerTypeData mpd = (MultiTablePivotBrokerTypeData)mbsi[MpGroupKey];
			if (mpd.FirstTableName != Qt.MetaTable.Name) return; // retrieve data for all tables when we see first table

			mpd.ClearBuffers();
			ExecuteMultipivotQuery(eqp, mpd);

			// Finish up

			MultipivotExecuteQueryCount++;
			t0 = TimeOfDay.Milliseconds() - t0;
			MultipivotExecuteQueryTime += t0;
			MultipivotExecuteQueryAvgTime = MultipivotExecuteQueryTime / MultipivotExecuteQueryCount;

			return;
		}

		/// <summary>
		/// Read data for one or more annotation tables & pivot in code
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteMultipivotQuery(
				ExecuteQueryParms eqp,
				MultiTablePivotBrokerTypeData mpd)
		{
			MetaTable mt = Qt.MetaTable;
			MetaColumn mc = null;
			DateTime dt;
			string sql, keyVal;
			DbCommandMx drd;
			AnnotationMetaBroker mb;
			List<GenericMetaBroker> mbList;
			object[] vo = null;
			int si, voi, i1;
			int rowsFetched = 0, vosCreated = 0;
			string mtName, cid, link, txt;
			DbType keyDbType;

			sql = BuildMultipivotSql(eqp, mpd);

			List<string> keySubset = eqp.SearchKeySubset;
			if (keySubset == null && !BuildSqlOnly) keySubset = GetPreviewSubset(); // assume previewing if no subset

			drd = new DbCommandMx();

			if (IsNumericKey) keyDbType = DbType.Int32;
			else keyDbType = DbType.String;
			PrepareListReader(drd, sql, keyDbType);
			ExecuteListReader(drd, keySubset);

			while (Read(drd))
			{
				rowsFetched++;
				if (IsNumericKey)
				{
					int intCid = drd.GetIntByName("EXT_CMPND_ID_NBR");
					cid = CompoundId.Normalize(intCid, mt);
				}

				else cid = drd.GetStringByName("EXT_CMPND_ID_TXT");

				string tableId = drd.GetIntByName("MTHD_VRSN_ID").ToString();
				string rsltTypeId = drd.GetLongByName("RSLT_TYP_ID").ToString();
				long rsltId = drd.GetLongByName("RSLT_ID");
				long rsltGrpId = drd.GetLongByName("RSLT_GRP_ID"); 
				string hyperLink = drd.GetStringByName("DC_LNK");

				if (!mpd.TableCodeDict.ContainsKey(tableId)) continue; // known table?

				mtName = "ANNOTATION_" + tableId;
				mb = (AnnotationMetaBroker)mpd.GetFirstBroker(mtName, out mbList);

				int mbIdx = 0;

				while (true) // copy out values for each metabroker
				{
					mt = mb.Qt.MetaTable;
					if (mt == null) continue;

					if (mb.MultipivotRowDict == null)
						mb.MultipivotRowDict = new Dictionary<string, object[]>();

					string rowKey = cid;
					rowKey += "_" + rsltGrpId.ToString();

					if (mb.MultipivotRowDict.ContainsKey(rowKey)) // have vo for this key value?
						vo = (object[])mb.MultipivotRowDict[rowKey];

					else // create vo for this key & copy the CID
					{
						vo = new Object[mb.SelectList.Count];
						mb.MultipivotRowDict[rowKey] = vo;
						vo[0] = cid;
						vosCreated++;
					}

					// Copy pivoted values

					for (si = 0; si < mb.SelectList.Count; si++) // find selected column(s) corresponding to result type id
					{ // (could optimize by keeping list of cols associated with each code)
						mc = mb.SelectList[si];

						if (mc.ResultCode != rsltTypeId) continue; // must match on type

						voi = si; // pivoted value vo index
						MobiusDataType mdt = null; // retrieved value here, must include rslt_id to allow editing

						if (mc.DataType == MetaColumnType.QualifiedNo) // qualified number value
						{
							QualifiedNumber qn = new QualifiedNumber();
							vo[si] = qn; // plug into the vo

							qn.Qualifier = drd.GetStringByName("RSLT_VAL_PRFX_TXT");
							if (qn.Qualifier == "=") qn.Qualifier = "";

							qn.NumberValue = drd.GetDoubleByName("RSLT_VAL_NBR");
							qn.TextValue = drd.GetStringByName("RSLT_VAL_TXT");

							mdt = qn;
						}

						else if (mc.DataType == MetaColumnType.Integer || // integer or double
							mc.DataType == MetaColumnType.Number)
						{
							double val = drd.GetDoubleByName("RSLT_VAL_NBR");
							NumberMx nex = new NumberMx();
							nex.Value = val;
							mdt = nex;
						}

						else if (mc.DataType == MetaColumnType.String)
						{
							txt = drd.GetStringByName("RSLT_VAL_TXT");
							StringMx sx = new StringMx(txt);
							vo[voi] = sx;
							mdt = sx;
						}

						else if (mc.DataType == MetaColumnType.Date)
						{
							dt = drd.GetDateTimeByName("RSLT_VAL_DT");
							DateTimeMx dtx = new DateTimeMx(dt);
							mdt = dtx;
						}

						else if (mc.DataType == MetaColumnType.Structure)
						{
							string s = drd.GetStringByName("RSLT_VAL_TXT"); // moleculeString stored in txt
							MoleculeMx cs = new MoleculeMx(s);
							mdt = cs;
						}

						else throw new Exception("Unexpected data type for column: " + mc.Label + " (" + mc.DataType + ")");

						if (mdt != null) // copy any link info if mdt defined
						{
							mdt.DbLink = rsltId.ToString();
							mdt.Hyperlink = hyperLink;
							vo[voi] = mdt; // plug into the vo
						}

					}

					if (mbList == null) break; // single broker
					mbIdx++; // go to next broker
					if (mbIdx >= mbList.Count) break; // at end of brokers?
					mb = (AnnotationMetaBroker)mbList[mbIdx];
				}
			}

			drd.Dispose();
			return;
		}

				/// <summary>
		/// Build Sql for multipivot query
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="mpd"></param>
		/// <returns></returns>

		string BuildMultipivotSql(
			ExecuteQueryParms eqp,
			MultiTablePivotBrokerTypeData mpd)
		{
			string sqlTemplate =
				@"select /*+ hint */ 
						ext_cmpnd_id_nbr,
						mthd_vrsn_id,
						rslt_typ_id,
						rslt_val_prfx_txt,
						rslt_val_nbr,
						rslt_val_txt,
						rslt_val_dt,
						dc_lnk,
						rslt_id,
						rslt_grp_id
				from " + TableName + @"
				where mthd_vrsn_id in (<tidList>) and sts_id = 1 and ext_cmpnd_id_nbr in (<list>)";

			string tidList = mpd.TableCodeCsvList.ToString();
			string sql = sqlTemplate.Replace("<tidList>", tidList);

			if (IsTextKey) sql = Lex.Replace(sql, "ext_cmpnd_id_nbr", "ext_cmpnd_id_txt"); // use correct key col

			sql = SetHint(sql);

			// Including result type criteria for sql that selects from only a single mthd_vrsn_id seems like it 
			// could be helpful but actually slows response by about 50%

			bool includeResultTypeCriteria = false; // (mpd.TableCodeDict.Keys.Count == 1 && PivotedColCriteriaCount == 0 && mpd.TableCodeDict.ContainsKey(tidList) && false);
			if (includeResultTypeCriteria) 
			{
				string tid = tidList; // single table id
				string rtList = Lex.HashSetToCsvString(mpd.TableCodeDict[tid].ResultCodeSet, false);
				string resultCodeCriteria = " and RSLT_TYP_ID in (" + rtList + ")";
				sql += resultCodeCriteria;
			}

			// Optimization for single col criteria (e.g. eqp.CallerSuppliedCriteria = "(upper ( T1.R_1508632258_val ) = 'EP300')") 
			// Return all rows with rslt_grp_id matching that of the rows that pass the criteria and pivot in code

			else if (Lex.IsDefined(eqp.CallerSuppliedCriteria) && PivotedColCriteriaCount == 1)
			{
				string matchColExpr, replaceColExpr;
				QueryColumn qc;
				MetaColumn mc;

				string critExpr = eqp.CallerSuppliedCriteria; // original criteria expression

				// Adjust any key col criteria

				qc = eqp.QueryTable.KeyQueryColumn;
				mc = qc.MetaColumn;
				matchColExpr = qc.QueryTable.Alias + "." + mc.Name;

				if (IsNumericKey) replaceColExpr = "ext_cmpnd_id_nbr";
				else replaceColExpr = "ext_cmpnd_id_txt";

				critExpr = Lex.Replace(critExpr, matchColExpr, replaceColExpr); // adjust col name to apply criteria to

				// Adjust criteria for FirstPivotedColWithCriteria

				qc = FirstPivotedColWithCriteria;
				mc = qc.MetaColumn;
				matchColExpr = qc.QueryTable.Alias + "." + mc.Name + "_val";

				if (mc.IsNumeric) replaceColExpr = "rslt_val_nbr";
				else if (mc.DataType == MetaColumnType.Date) replaceColExpr = "rslt_val_dt";
				else replaceColExpr = "rslt_val_txt";

				critExpr = Lex.Replace(critExpr, matchColExpr, replaceColExpr); // adjust col name to apply criteria to

				// Build full sql

				string innerSql = "select rslt_grp_id from (" + // sql to get matching rslt_grp_ids
					sql + " and rslt_typ_id = " + qc.MetaColumn.ResultCode + " and " + critExpr + ")"; // apply criteria to appropriate result type

				sql += " and rslt_grp_id in( " + innerSql + ")"; // initial sql with additional criteria applied
			}

			return sql;
		}

		/// <summary>
		/// Build Sql for multipivot query (TableAndResultTypeIdCriteria - slower than singly)
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="mpd"></param>
		/// <returns></returns>

		string BuildMultipivotSqlTableAndResultTypeIdCriteria(
			ExecuteQueryParms eqp,
			MultiTablePivotBrokerTypeData mpd)
		{
			string assayList = "", resultTypeList = "";

			string sql = 
				@"select 
						ext_cmpnd_id_nbr,
						mthd_vrsn_id,
						rslt_typ_id,
						rslt_val_prfx_txt,
						rslt_val_nbr,
						rslt_val_txt,
						rslt_val_dt,
						dc_lnk,
						rslt_id,
						rslt_grp_id
				from " + TableName + " ";

			string criteria = "ext_cmpnd_id_nbr in (<list>) and sts_id = 1";

			if (IsTextKey) sql = Lex.Replace(sql, "ext_cmpnd_id_nbr", "ext_cmpnd_id_txt");

			string c2 = "";
			foreach (string tid in mpd.TableCodeDict.Keys)
			{
				if (c2 != "") c2 += " or ";
				c2 += "(MTHD_VRSN_ID = " + tid + " and RSLT_TYP_ID in (" + Lex.HashSetToCsvString(mpd.TableCodeDict[tid].ResultCodeSet, false) + "))";

				// criteria += BuildSqlInCriteriaFromList("MTHD_VRSN_ID", assayList);
			}

			if (c2 != "") criteria += " and (" + c2 + ")";

			sql += "where " + criteria;

			return sql;
		}

		/// <summary> 
		/// Build Sql for multipivot query (UnionAll - slower than singly)
		/// </summary>
		/// <param name="eqp"></param>
		/// <param name="mpd"></param>
		/// <returns></returns>

		string BuildMultipivotSqlUnionAll(
			ExecuteQueryParms eqp,
			MultiTablePivotBrokerTypeData mpd)
		{
			string assayList = "", resultTypeList = "";

			string sqlTemplate =
				@"select 
						ext_cmpnd_id_nbr,
						mthd_vrsn_id,
						rslt_typ_id,
						rslt_val_prfx_txt,
						rslt_val_nbr,
						rslt_val_txt,
						rslt_val_dt,
						dc_lnk,
						rslt_id,
						rslt_grp_id
				from " + TableName + @"
				where mthd_vrsn_id in (<tidList>) and sts_id = 1 and ext_cmpnd_id_nbr in (<list>)";

			string sql = "";
			foreach (string tid in mpd.TableCodeDict.Keys)
			{
				if (sql != "") sql += " union all ";
				string sql2 = sqlTemplate.Replace("<tidList>", tid);
				sql += sql2;
			}

			if (IsTextKey) sql = Lex.Replace(sql, "ext_cmpnd_id_nbr", "ext_cmpnd_id_txt");

			return sql;
		}

		/// <summary>
		/// Get next row of data from buffer
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			object[] vo;

			if (!PivotInCode)
			{
				vo = base.NextRow();
				if (vo == null) return vo;

				foreach (object o in vo) // split out any separate dbLink/hyperlink
				{
					MobiusDataType mdt = o as MobiusDataType;
					if (mdt != null && !String.IsNullOrEmpty(mdt.DbLink) && mdt.DbLink.Contains(","))
					{
						string[] sa = mdt.DbLink.Split(',');
						mdt.DbLink = sa[0].Trim();
						mdt.Hyperlink = sa[1].Trim();
					}
				}

				return vo;
			}

			vo = GetNextMultipivotRowDictVo();
			return vo;
		}

		/// <summary>
		/// Generate the sql for retrieval from a querytable
		/// If this query returns qualified numbers (i.e. is not search part of query or part of calc field)
		/// then don't retrieve any data other than the compound id & group id.
		/// </summary>
		/// <param name="qt"></param>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			QueryColumn qc;
			MetaColumn mc;
			string tableAlias, resultCode, txt, tok;
			string pivotTable = "", pivotTables = "", pivotExprs, pivotCriteria;
			int ci, qci, i1;

			InitializeColumnInfo(eqp);

			Exprs = FromClause = OrderBy = ""; // outer sql elements

			////////////////////////////////////////////////////////////////////////////////
			// Build sql
			////////////////////////////////////////////////////////////////////////////////
			//
			// An inner query in the from clause pivots out the columns that are
			// selected or have criteria
			//
			////////////////////////////////////////////////////////////////////////////////

			MetaTable mt = eqp.QueryTable.MetaTable;

			pivotExprs =
				"t0." + mt.KeyMetaColumn.ColumnMap + " " + mt.KeyMetaColumn.Name + ", " +
				"t0.rslt_grp_id, " +
				"t0.rslt_id, " +
				"t0.rslt_typ_id, " +
				"t0.rslt_val_prfx_txt, " +
				"t0.rslt_val_nbr, " +
				"t0.rslt_val_txt, " +
				"t0.rslt_val_dt, " +
				"t0.dc_lnk ";

			pivotCriteria = "";
			int pivotCount = 0;
			string pivotKeyExpr = mt.KeyMetaColumn.ColumnMap;

			bool joinAtResultLevel = true; // join at result level if more than one pivoted col is involved

			QueryColumn t0Col = null; // column assigned to t0
			if (FirstPivotedColWithCriteria != null)
				t0Col = FirstPivotedColWithCriteria; // if there is a pivoted column with criteria assign to t0

			else if (SelectList.Count == 2 && FirstPivotedColSelected != null)
				t0Col = FirstPivotedColSelected; // single selected pivoted col assigned to t0

			else // no pivoted cols selected or with criteria (e.g. key only or ALL_ANNOTATION_PIVOTED)
			{
				// Code below not needed
				//if (Lex.IsDefined(mt.Code) && SelectList.Count > 1) // at least one non-key col selected?
				//{
				//  pivotTables = // everything will join up to this
				//   "(select unique " + pivotKeyExpr + ", rslt_grp_id "; // group by key & result group
				//}

				//else 
				//{ <code below>... }

				pivotTables =
					"(select " +
					pivotKeyExpr + ", " +
					"rslt_grp_id, " +
					"rslt_id, " +
					"rslt_typ_id, " +
					"rslt_val_prfx_txt, " +
					"rslt_val_nbr, " +
					"rslt_val_txt, " +
					"rslt_val_dt, " +
					"dc_lnk ";

				pivotTables += "from " + TableName + " " +
					"where 1=1";

				if (mt.Code != "") // all data
					pivotTables += " and mthd_vrsn_id=" + mt.Code;

				if (ResultCodeList.Length > 0) // if we have any result codes then limit to those
					pivotTables += " and rslt_typ_id in (" + ResultCodeList.ToString() + ")";

				pivotTables += " and sts_id = 1) t0 ";
			}

			// Process query column objects

			List<QueryColumn> qcl = Qt.QueryColumns;
			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				mc = qc.MetaColumn;

				// Pivot out the result

				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				if (mt.KeyMetaColumn == mc) { } // already have key column

				else if (mc.ResultCode != "") // pivot out of results table
				{
					if (qc == t0Col) tableAlias = "t0"; // col to be used as t0
					else
					{
						pivotCount++;
						tableAlias = "t" + (pivotCount).ToString();
					}

					resultCode = mc.ResultCode;
					pivotTable = TableName; // table to pivot out of

					string colName = tableAlias + "." + GetOracleColumnName(mc);
					pivotExprs += ", ";
					if (Eqp.ReturnQNsInFullDetail && mc.DetailsAvailable)
					{
						if (qc.Is_Selected_or_GroupBy_or_Sorted)
						{ // build expression to retrieve catenated col value & linking info
							string colExpr = colName;

							if (mc.DataType == MetaColumnType.QualifiedNo)
							{
								colExpr =
									tableAlias + ".rslt_val_prfx_txt || chr(11) || " +
									tableAlias + ".rslt_val_nbr || chr(11) || " + // basic number
									tableAlias + ".rslt_val_txt " + // text
									" || chr(11) " + // number of values in stats
									" || chr(11) " + // number of runs
									" || chr(11) " + // standard deviation
									" || chr(11) "; // standard error
							}

							else if (mc.DataType == MetaColumnType.Date) // return normalized, sortable date string
								colExpr = "to_char(" + colName + ",'YYYYMMDD HH24MISS')";

							pivotExprs += // catenate column expression with link info
								colExpr + " || chr(11) || " + // basic value followed by tab delimiter
								tableAlias + ".rslt_id || ',' || " + // row id
								tableAlias + ".dc_lnk " + // any hyperlinklink info
								mc.Name + ", ";
						}

						pivotExprs += // native column value for comparison
							colName + " " + mc.Name + "_val ";
					}

					else
					{
						pivotExprs += // just return native value
							colName + " " + mc.Name + ", ";
						pivotExprs += // native column value for comparison
							colName + " " + mc.Name + "_val ";
					}

					if (pivotTables != "") pivotTables += ", ";
					pivotTables += pivotTable + " " + tableAlias + " ";

					txt = // pivot on method and result type
						"<TA>.mthd_vrsn_id (+) = <MTHD_ID> " +
						"and <TA>.rslt_typ_id (+) = <TYP_ID> " +
						"and <TA>.sts_id (+) = 1 ";

					if (qc != t0Col)  // joint to t0 by compound id unless we are t0
					{
						txt += "and <TA>." + mt.KeyMetaColumn.ColumnMap + " (+) = t0." + mt.KeyMetaColumn.ColumnMap + " ";
						if (joinAtResultLevel)
							txt += "and <TA>.rslt_grp_id (+) = t0.rslt_grp_id ";

						if ((Lex.IsDefined(qc.Criteria) && !qc.HasNotExistsCriteria)) // remove outer join if criteria other than not-exists defined for col
							txt = txt.Replace("(+)", ""); 
					}

					txt = txt.Replace("<TA>", tableAlias);
					txt = txt.Replace("<MTHD_ID>", mt.Code);
					txt = txt.Replace("<TYP_ID>", resultCode);
					if (pivotCriteria != "") pivotCriteria += " and ";
					pivotCriteria += txt + " ";
				}

				else // nulls for other non-result fields
				{
					pivotExprs += ", null " + mc.Name + " ";
				}

				// Add to select select expression to list if col selected or sorted selected

				if (qc.Is_Selected_or_GroupBy_or_Sorted) // if selected add to expression list
				{
					if (mt.KeyMetaColumn == mc) // save index of compound id value object
						KeyVoi = SelectList.Count;

					if (Exprs.Length > 0) Exprs += ",";

					//if (mc.ColumnMap!="" && mc.ColumnMap!=mc.Name && // column mapped?
					//  mt.MetaBrokerType != MetaBrokerType.Annotation) // don't use special annotation mapping here
					//  Exprs += mc.ColumnMap + " " + mc.Name;
					//else Exprs += mc.Name; 

					Exprs += mc.Name;
				}
			} // end of column loop

			FromClause = "(select " + pivotExprs + " " +
				"from " + pivotTables;
			if (pivotCriteria != "") FromClause += " where " + pivotCriteria;
			FromClause += ")";


			// Put final SQL together

			FromClause = AdjustSqlForNotExistsCriteriaAsNeeded(eqp, Exprs, FromClause);

			Sql = "select /*+ hint */ " + Exprs +
			 " from " + FromClause;
			Sql = SetHint(Sql);

			if (Qt.Alias != "") Sql += " " + Qt.Alias;
			if (eqp.CallerSuppliedCriteria != "")
				Sql += " where " + eqp.CallerSuppliedCriteria;

			return Sql;
		}

		static string SetHint(string sql)
		{
			string hint = ServicesIniFile.Read("AnnotationPivotHint");
			if (Lex.IsDefined(hint)) sql = Lex.Replace(sql, "/*+ hint */", "/*+ " + hint + " */");
			return sql;
		}

/// <summary>
/// Initialize column info for broker QueryTable
/// </summary>
/// <param name="eqp"></param>

		void InitializeColumnInfo(
			ExecuteQueryParms eqp)
		{
			QueryColumn qc;
			MetaColumn mc;
			int qci;

			Eqp = eqp;
			Qt = eqp.QueryTable;

			List<QueryColumn> qcl = Qt.QueryColumns;
			MetaTable mt = Qt.MetaTable;
			KeyMc = mt.KeyMetaColumn;
			Hashtable resultKeys = new Hashtable();
			SelectList = new List<MetaColumn>(); // list of selected metacolumns

			PivotedColCriteriaCount = 0; // not counting key
			PivotedColSelectCount = 0;

			FirstPivotedColSelected = null; // first pivoted column with criteria
			FirstPivotedColWithCriteria = null; // first pivoted column that's selected

			ResultCodeList = new StringBuilder(); // build list of selected result codes
			ResultCodeSet = new HashSet<string>();

			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				mc = qc.MetaColumn;

				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue; // ignore it

				if (qc.Is_Selected_or_GroupBy_or_Sorted) SelectList.Add(mc);

				if (mc.ResultCode == "") continue; // done if not pivoted (i.e. key col)

// Col to pivot

				PivotedColSelectCount++;

				if (ResultCodeList.Length > 0) ResultCodeList.Append(",");
				ResultCodeList.Append(Lex.AddSingleQuotes(mc.ResultCode));
				ResultCodeSet.Add(mc.ResultCode);

				if (qc.Is_Selected_or_GroupBy_or_Sorted)
				{
					if (FirstPivotedColSelected == null &&
						!eqp.ReturnQNsInFullDetail) // don't count if getting qualified numbers since handled differently
						FirstPivotedColSelected = qc;
				}

				if (qc.Criteria != "")
				{
					PivotedColCriteriaCount++;
					if (FirstPivotedColWithCriteria == null)
						FirstPivotedColWithCriteria = qc;
				}
			}

			if ((eqp.Qe != null && eqp.Qe.MustUseEquiJoins && PivotedColCriteriaCount > 1) || PivotInCode)
				FirstPivotedColWithCriteria = FirstPivotedColSelected = null; // don't use the "FirstPivoted" optimization unless "and" logic

			// Add key column to selected list if not already included

			qc = Qt.KeyQueryColumn;
			if (qc == null)
				throw new Exception("Key column not found for MetaTable " + mt.Name);

			if (!qc.Is_Selected_or_GroupBy_or_Sorted) 
				SelectList.Add(mt.KeyMetaColumn);

			return;
		}

		/// <summary>
		/// Return column name associated with data type
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		string GetOracleColumnName(
			MetaColumn mc)
		{
			if (mc.DataType == MetaColumnType.Integer ||
				mc.DataType == MetaColumnType.Number ||
				mc.DataType == MetaColumnType.QualifiedNo)
				return "rslt_val_nbr";

			else if (mc.DataType == MetaColumnType.String || mc.DataType == MetaColumnType.Structure)
				return "rslt_val_txt";

			else if (mc.DataType == MetaColumnType.Date)
				return "rslt_val_dt";

			else throw new Exception("Unexpected data type for " + mc.MetaTable.Name + "." + mc.Name);
		}

		/// <summary>
		/// Select a full row of data for the specified metatable
		/// Only for pivoted tables with one result group per compound id
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="cn"></param>
		/// <returns></returns>

		public object[] SelectResultGroup(
			MetaTable mt,
			string keyVal)
		{
			long resultCode;
			MetaColumn mc;

			GenericDwDao dwd = new GenericDwDao(TableName, SeqName);
			keyVal = CompoundId.NormalizeForDatabase(keyVal); // normalize key to match database
			if (keyVal == null) return null;
			int mthdVrsnId = Int32.Parse(mt.Code);
			List<AnnotationVo> rows = dwd.Select(mthdVrsnId, keyVal);
			if (rows == null || rows.Count == 0) return null;

			object[] vo = new object[mt.MetaColumns.Count];
			vo[0] = keyVal;

			for (int ri = 0; ri < rows.Count; ri++)
			{
				AnnotationVo rvo = (AnnotationVo)rows[ri];

				for (int ci = 0; ci < mt.MetaColumns.Count; ci++) // find matching metacolumn
				{
					mc = (MetaColumn)mt.MetaColumns[ci];
					if (mc.ResultCode == null || mc.ResultCode == "") continue;
					try { resultCode = Int64.Parse(mc.ResultCode); }
					catch (Exception ex) { continue; }
					if (resultCode != rvo.rslt_typ_id) continue;

					if (mc.DataType == MetaColumnType.Integer)
					{
						if (rvo.rslt_val_nbr != NullValue.NullNumber) vo[ci] = (int)rvo.rslt_val_nbr;
					}

					else if (mc.DataType == MetaColumnType.Number)
					{
						if (rvo.rslt_val_nbr != NullValue.NullNumber) vo[ci] = (double)rvo.rslt_val_nbr;
					}

					else if (mc.DataType == MetaColumnType.String || mc.DataType == MetaColumnType.Structure)
					{
						if (rvo.rslt_val_txt != "") vo[ci] = rvo.rslt_val_txt;
					}

					else if (mc.DataType == MetaColumnType.Date)
					{
						if (rvo.rslt_val_dt != DateTime.MinValue) vo[ci] = rvo.rslt_val_dt;
					}
					break;
				}
			}

			dwd.Dispose(); // free connection

			return vo;
		}

		/// <summary>
		/// Insert a full row of data for the specified metatable
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		public bool InsertResultGroup(
			MetaTable mt,
			object[] vo,
			string userName)
		{
			AnnotationVo dwVo = null;

			if (mt == null || vo == null || vo[0] == null || GetKeyString(vo[0]) == "" || userName == null)
				return false;

			string cn = GetKeyString(vo[0]); // assume key is in first column
			cn = CompoundId.NormalizeForDatabase(cn);
			if (cn == null) return false;

			GenericDwDao dwDao = new GenericDwDao(TableName, SeqName);
			int tableCode = Int32.Parse(mt.Code);

			long rslt_grp_id = dwDao.GetNextIdLong(); // id to hold row together

			for (int ci = 0; ci < mt.MetaColumns.Count; ci++)
			{
				MetaColumn mc = (MetaColumn)mt.MetaColumns[ci];
				if (vo[ci] == null) continue;
				dwVo = new AnnotationVo();
				dwVo.rslt_grp_id = rslt_grp_id;

				if (mc.ResultCode == "") continue;

				if (mc.DataType == MetaColumnType.Integer)
				{
					dwVo.rslt_val_nbr = (int)vo[ci];
				}

				else if (mc.DataType == MetaColumnType.Number)
				{
					dwVo.rslt_val_nbr = (double)vo[ci];
				}

				else if (mc.DataType == MetaColumnType.String || mc.DataType == MetaColumnType.Structure)
				{
					dwVo.rslt_val_txt = (string)vo[ci];
				}

				else if (mc.DataType == MetaColumnType.Date)
				{
					dwVo.rslt_val_dt = (DateTime)vo[ci];
				}

				else continue;

				dwVo.ext_cmpnd_id_txt = cn;
				dwVo.mthd_vrsn_id = Int32.Parse(mt.Code);
				dwVo.rslt_typ_id = Int64.Parse(mc.ResultCode);
				dwVo.chng_op_cd = "I";
				dwVo.chng_usr_id = userName;
				dwDao.Insert(dwVo);
			}

			dwDao.Dispose(); // free connection
			return true;
		}

		/// <summary>
		/// Read a sample of numbers for previewing
		/// </summary>
		/// <returns></returns>

		List<string> GetPreviewSubset()
		{
			Dictionary<string, object> PreviewSubset = new Dictionary<string, object>();
			string sql =
				"select " + Eqp.QueryTable.MetaTable.KeyMetaColumn.ColumnMap + " " +
				"from " + TableName + " " +
				"where mthd_vrsn_id = " + Qt.MetaTable.Code;

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();
			while (drd.Read())
			{
				string cid = drd.GetObject(0).ToString();
				cid = CompoundId.Normalize(cid); // normalize cid adding prefix as needed
				PreviewSubset[cid] = null;
				if (PreviewSubset.Count >= 1000) break;
			}
			drd.Dispose();

			return new List<string>(PreviewSubset.Keys);
		}

		/// <summary>
		/// Convert a multipivot table into a set of tables where data exists for
		/// one or more of the compound identifiers in the list.
		/// </summary>
		/// <param name="qt">Current form of query table</param>
		/// <param name="q">Query to add transformed tables to</param>
		/// <param name="ResultKeys">Keys data will be retrieved for</param>

		public override void ExpandToMultipleTables(
			QueryTable qt,
			Query q,
			List<string> resultKeys)
		{
			MetaTable mt2;
			QueryTable qt2;
			string sql;
			int methodId;

			Stopwatch
				readSw = new Stopwatch(),
				contentsSw = new Stopwatch(),
				mtGetSw = new Stopwatch(),
				totalSw = Stopwatch.StartNew();

			sql =
				"select mthd_vrsn_id " +
				"from " + TableName + " " +
				"where " + qt.MetaTable.KeyMetaColumn.ColumnMap + " in (<list>) " +
				" and sts_id = 1 " + // active records only
				"group by mthd_vrsn_id";

			readSw.Start();
			DbCommandMx drd = new DbCommandMx();
			drd.PrepareListReader(sql, DbType.String); // comparing against varchar2 ext_cmpnd_id_txt col
			drd.ExecuteListReader(resultKeys);
			if (drd.Cancelled)
			{
				// todo qe.Cancelled = true;
				drd.Dispose();
				return;
			}

			List<int> methodIdList = new List<int>();
			while (true) // get list of method ids
			{
				if (!drd.ListRead()) break;
				methodId = drd.GetInt(0);
				methodIdList.Add(methodId);
			}
			drd.Dispose();
			if (drd.Cancelled)
			{
				// todo qe.Cancelled = true;
				return;
			}

			readSw.Stop();

			Dictionary<string, object> mtDict = new Dictionary<string, object>();
			int methodIdCount = 0, notInContents = 0, notInMetaTables = 0, notSameRoot = 0;

			for (int mi = 0; mi < methodIdList.Count; mi++) // convert list of methods to set of metatable names
			{
				methodId = methodIdList[mi];
				//if (methodId == 286102) methodId = methodId; // debug
				string mtName = "ANNOTATION_" + methodId.ToString();

				contentsSw.Start();
				if (QueryEngine.FilterAllDataQueriesByDatabaseContents &&
					!MetaTableCollection.IsMetaTableInContents(mtName)) // metatable must be in contents
				{
					notInContents++;
					contentsSw.Stop();
					continue;
				}
				contentsSw.Stop();

				mtGetSw.Start();
				mt2 = MetaTableCollection.Get(mtName);
				mtGetSw.Stop();

				if (mt2 == null)
				{
					notInMetaTables++;
					continue;
				}

				if (mt2.Parent == null || // skip if no parent
					Lex.Ne(mt2.Root.Name, qt.MetaTable.Root.Name)) // must have same root
				{
					notSameRoot++;
					continue;
				}

				if (qt.MetaTable.UseSummarizedData && qt.MetaTable.SummarizedExists) mtName += MetaTable.SummarySuffix;
				mtDict[mtName] = null;
				methodIdCount++;
			}

			List<string> mtList = new List<string>();
			foreach (string mtName2 in mtDict.Keys)
			{ // put metatable labels & names into a list for sorting
				mt2 = MetaTableCollection.Get(mtName2);
				mtList.Add(mt2.Label.ToLower().PadRight(64) + "\t" + mt2.Name);
			}

			mtList.Sort();

			foreach (string mts in mtList)
			{ // add new querytables/metatables to query
				string[] sa = mts.Split('\t');
				mt2 = MetaTableCollection.Get(sa[1]);
				if (mt2 == null) continue;
				qt2 = new QueryTable(q, mt2);
				if (qt.HeaderBackgroundColor != Color.Empty)
					qt2.HeaderBackgroundColor = qt.HeaderBackgroundColor;
			}

			totalSw.Stop();
			return;
		}

		/// <summary>
		/// See if source is available
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override DataSchemaMx CheckDataSourceAccessibility(
		MetaTable mt)
		{
			return GenericMetaBroker.CheckDataSourceAccessibility("mbs_owner");
		}

		/// <summary>
		/// Convert a key value to a string
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static string GetKeyString(object o)
		{
			return QueryEngine.GetKeyString(o);
		}
	}
}
