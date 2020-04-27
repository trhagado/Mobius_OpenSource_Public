using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Summary description for GenericMetaBroker.
	/// </summary>

	public class AssayMetaBroker : GenericMetaBroker
	{
		GenericMetaBroker Gmb; // generic metabroker used in our place

		// Broker vars used to build SQL

		bool JoinOnConcentration = false; // join on conc to separate results if % inh type assay with multiple concentrations and other join vars the same

		string InnerExprs = ""; // list of inner select expressions returned by the broker for the  selected/criteria/ordering QueryTable cols
		string InnerTables = ""; // list of inner tables
		string InnerCriteria = ""; // inner criteria for filtering and joins to return desired data

		List<MetaColumn> Mcl { get { return Mt != null ? Mt.MetaColumns : null; } } // MetaColumn list
		List<QueryColumn> Qcl { get { return Qt != null ? Qt.QueryColumns : null; } } // QueryColumn list

		int DetailedResultPivotedColsFetched = 0; // number ASSAY_DTLD_RSLT table pivoted cols to fetch (Selected_or_Criteria_or_Sorted)
		int ActivityResultPivotedColsFetched = 0; // number ASSAY_ACTVTY_RSLT table pivoted cols to fetch (Selected_or_Criteria_or_Sorted)

		int SelectCnt = 0, CriteriaCnt = 0;
		List<QueryColumn> PivotedColumns = null; // list of pivoted columns
		int[] PivotedToUnpivotedVoi = null; // map from pivoted metatable column vo index to unpivoted metatable column vo index
		HashSet<string> PivotedCodes = null; // set of pivoted codes
		QueryColumn FirstPivotedColWithCriteria = null; // first pivoted column with criteria used for query optimization
		QueryColumn FirstPivotedColWithAllResults = null; // first pivoted column with a database result row for each result group for the assay
		PivotGroupCollection PivotGroups = null;

		AssayCurveService CrcService; // service instance for getting images
		static int CrcServiceDepth = 0;

		public static bool CacheBitmaps = false; // turn caching on or off
		static Dictionary<string, Bitmap> BitmapCache = null; // dict of retrieved bitmaps

		// Global statics

		public static IMetaFactory2 AssayMetaFactory; // allows calls into generic AssayMetafactory

		public static MetaTable ResultsRowMetatable; // 
		public static string ResultsRowMetatableName = "AssayResultsRow";
		public static int SubqueryId = 0; // Id to uniquely identify individual AssayRslt with-clause instances

		public static bool AllowMultiTablePivot_Assay = true; // local multipivot flag for ASSAY
		public static int MultipivotExecuteQueryCount, MultipivotExecuteQueryTime, MultipivotExecuteQueryAvgTime; // performance data

		public static Dictionary<int, int> AssayId2ToAssayIdMap = null; // map between ASSAY assay ids and RDW assay ids
		public static Dictionary<int, int> AssayIdToAssayId2Map = null;
		public static int UniqueGroupId = 0; // to provide unique row grouping when not in DB

		public static Dictionary<string, MetaTableStats> TableStats = null; // stats for tables loaded by AssayMetaFactory
		
		public static bool Debug = false;

		// Constructor

		public AssayMetaBroker()
		{
			return;
		}

		/// <summary>
		/// Set the Url for the CRC service
		/// </summary>
		/// <returns></returns>

		public static string CrcServiceUrl
		{
			get
			{
				if (_crcServiceUrl != null) return _crcServiceUrl;
				if (ServicesIniFile.IniFile == null) return AssayCurveService.ServiceUrl;
				return null;
			}
		}
		static string _crcServiceUrl = null; // URL of service to provide CRCs

		/// <summary>
		/// Get the name of the SQL statement group that this table should be included in for the purposes of executing searches
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override string GetTableCriteriaGroupName(
			QueryTable qt)
		{
			return qt.MetaTable.Name; // just a single metatable in a group, queries combining two assay tables (e.g. data exists) can be very slow
		}

		/// <summary>
		/// Build the sql for a query or setup for multipivot retrieval if possible
		/// </summary>
		/// <param name="eqp">ExecuteQueryParms</param>
		/// <returns>Sql for the query</returns>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			MultiTablePivotBrokerTypeData mpd; // multipivot data for this broker type

			Eqp = eqp;
			Qt = eqp.QueryTable;
			MetaTable mt = Qt.MetaTable;

			if ( // check for basic conditions that disallow multipivoting
				//IsMergedTable(mt) || // remove comment to disallow pivoting in code for merged tables
				!eqp.ReturnQNsInFullDetail || // no pivot in code if part of calc field
				eqp.SearchKeySubset == null || // no pivot in code unless have subset
				eqp.Qe == null) // need to be able to access queryengine info
				//!QueryEngine.AllowMultiTablePivot || // is multipivot even allowed
				//!AllowMultiTablePivot_Assay)
				return base.PrepareQuery(eqp);

			int pivotedColCount = 0;
			int detailedPivotCount = 0;
			int activityPivotCount = 0;
			foreach (QueryColumn qc1 in Qt.QueryColumns)
			{ // if any non-key criteria then pivot individually rather than via multipivot
				MetaColumn mc = qc1.MetaColumn;
				if (qc1.Criteria != "" && !mc.IsKey) return base.PrepareQuery(eqp);
				if (qc1.Selected && IsPivotedColumn(mc))
				{
					pivotedColCount++;
					if (mc.SummarizationRole == SummarizationRole.Dependent) detailedPivotCount++;
					else if (mc.SummarizationRole == SummarizationRole.Derived) activityPivotCount++;
				}
			}

			if (pivotedColCount == 0 || // must have at least one column to pivot
			 activityPivotCount > 0) // disallow for data from activity table for now
			{
				string sql = base.PrepareQuery(eqp);

				//if (Lex.Eq(Qt.MetaTableName, "ASSAY_UNPIVOTED") || Lex.Eq(Qt.MetaTableName, "ASSAY_UNPIVOTED_SUMMARY"))
				//{
				//	sql = AdjustSqlFor_ASSAY_UNPIVOTED_Tables(sql);
				//	Sql = sql;
				//}

				return sql;
			}

			// Store pivoting info for queryTable

			PivotInCode = true;
			BuildSql(eqp); // setup SelectList (don't really need sql)

			MpGroupKey = // key for broker/detail level
				MetaBrokerType.Assay.ToString() + "_" + mt.UseSummarizedData.ToString();

			if (!AllowMultiTablePivot_Assay)
				MpGroupKey += "_" + Qt.MetaTable.Name;

			if (eqp.Qe.MetaBrokerStateInfo == null)
				eqp.Qe.MetaBrokerStateInfo = new Dictionary<string, MultiTablePivotBrokerTypeData>();

			mpd = MultiTablePivotBrokerTypeData.GetMultiPivotData(eqp.Qe.MetaBrokerStateInfo, MpGroupKey, mt.Name);

			List<string> codes = new List<string>();
			if (!IsMergedTable(mt))
				codes.Add(mt.Code); // normal single code case
			else codes = new List<string>(mt.TableFilterValues);

			foreach (string code0 in codes)
			{
				if (code0 != "" && !mpd.TableCodeDict.ContainsKey(code0)) // include assay code
				{
					mpd.TableCodeDict[code0] = new MpdResultTypeData(); // add key to hash list
					if (mpd.TableCodeCsvList.Length > 0) mpd.TableCodeCsvList.Append(",");

					mpd.TableCodeCsvList.Append(code0);
				}
			}

			mpd.AddMetaBroker(mt.Name, this);

			return null; // no sql generated here
		}

		/// <summary>
		/// Execute query
		/// Just call base method if not multipivoting. If multipivoting &
		/// this is the first table retrieve & buffer the data.
		/// </summary>
		/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			if (!PivotInCode) // if not multipivot then call generic broker 
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
		/// Read unsummarized or summarized data for one or more assays & pivot in code
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteMultipivotQuery(
				ExecuteQueryParms eqp,
				MultiTablePivotBrokerTypeData mpd)
		{
			string sql;
			DbCommandMx drd;
			AssayMetaBroker pMb;
			MetaTable pMt;
			MetaColumn pMc;
			List<GenericMetaBroker> mbList;
			object[] vo = null;
			int si, pVoi, uVoi, i1;
			int rowsFetched = 0, vosCreated = 0;
			string txt;

			bool unsummarized = Qt.MetaTable.UseUnsummarizedData;
			bool summarized = !unsummarized;

			sql = BuildMultipivotSql(eqp, mpd);

			List<MetaTable> mtList = new List<MetaTable>();

			drd = new DbCommandMx();
			PrepareListReader(drd, sql, DbType.Int32);
			ExecuteListReader(drd, eqp.SearchKeySubset);

			if (BuildSqlOnly) return; // if just building sql return;

			while (Read(drd))
			{
				rowsFetched++;
				int corpId = drd.GetIntByName("<cid>");
				string assayId = drd.GetIntByName("assay_id").ToString();
				string assayTypeId = drd.GetIntByName("rslt_typ_id").ToString();
				int rsltId = drd.GetIntByName("rslt_id");

				int rsltGrpId = 0;
				if (unsummarized) rsltGrpId = drd.GetIntByName("rslt_grp_id"); // result group only exists for unsummarized

				double concNbr = drd.GetDoubleByName("rslt_conc_nbr");

				mtList.Clear(); // clear list of metatables this code applies to

				foreach (string mtName0 in mpd.MbInstances.Keys)
				{
					pMt = MetaTableCollection.Get(mtName0);
					if (CodeAppliesToTable(assayId, pMt))
						mtList.Add(pMt);
				}

				for (int mti = 0; mti < mtList.Count; mti++)
				{
					pMt = mtList[mti];
					if (pMt == null) continue;
					string mtName = pMt.Name;

					if (!mpd.MbInstances.ContainsKey(pMt.Name)) continue; // have broker(s) for table?

					int mbIdx = 0;
					pMb = (AssayMetaBroker)mpd.GetFirstBroker(mtName, out mbList);

					while (true) // copy out values for each metabroker
					{
						pMt = pMb.Qt.MetaTable;
						if (pMt == null) continue;

						if (pMb.MultipivotRowDict == null)
							pMb.MultipivotRowDict = new Dictionary<string, object[]>();

						string rowKey = corpId.ToString();

						if (unsummarized)
						{
							rowKey += "_" + rsltGrpId.ToString();
							if (pMb.PivotedCodes.Count <= 1) // if only one pivoted result code be sure unique (same GrpNbr may be used for multiple results for SP type assays with a single result type)
								rowKey += "_" + (UniqueGroupId++);
						}

						else // summarized key;
						{
							if (pMt.JoinOnConcentration && pMb.PivotedCodes.Count <= 1) // join on conc as well (only one result type allowed))
								rowKey += "_" + concNbr;
						}

						if (pMb.MultipivotRowDict.ContainsKey(rowKey)) // have vo for this key value?
							vo = (object[])pMb.MultipivotRowDict[rowKey];

						else // create vo for this key & fill in values that are constant
						{
							vo = new Object[pMb.SelectList.Count];
							pMb.MultipivotRowDict[rowKey] = vo;
							vosCreated++;

							for (si = 0; si < pMb.SelectList.Count; si++) // set non-pivoted fields
							{
								pMc = pMb.SelectList[si];
								if (IsPivotedColumn(pMc)) continue;

								pVoi = si; // pivoted value vo index
								uVoi = pMb.PivotedToUnpivotedVoi[pVoi];
								CopyUnpivotedValueToPivotedVo(pMc, drd, uVoi, vo, pVoi);
							}
						}

						// Copy pivoted values

						for (si = 0; si < pMb.SelectList.Count; si++) // find selected column(s) corresponding to result type id
						{ // (could optimize by keeping list of cols associated with each code)
							pMc = pMb.SelectList[si];

							if (!IsPivotedColumn(pMc)) continue; // must be pivoted
							if (!pMc.IsValidResultCode(assayTypeId)) continue; // must match on a valid result type

							pVoi = si; // pivoted value vo index
							if (pMc.DataType == MetaColumnType.QualifiedNo) // qualified number value
							{
								if (unsummarized)
								{
									QualifiedNumber qn = new QualifiedNumber();
									vo[si] = qn; // plug into the vo

									qn.Qualifier = drd.GetStringByName("rslt_prfx_txt");
									if (qn.Qualifier == "=") qn.Qualifier = "";

									qn.NumberValue = drd.GetDoubleByName("rslt_value_nbr");
									qn.TextValue = drd.GetStringByName("rslt_value_txt");

									if (pMc.DetailsAvailable && !qn.IsNull) // qn.NumberValue != QualifiedNumber.NullNumber) // add drilldown identifier for primary result types
										qn.DbLink = assayId + "," + assayTypeId + "," + rsltGrpId + "," + rsltId;
								}

								else // summarized value
								{
									QualifiedNumber qn = new QualifiedNumber();
									vo[si] = qn; // plug into the vo

									qn.Qualifier = drd.GetStringByName("rslt_mean_prfx_txt");
									if (qn.Qualifier == "=") qn.Qualifier = "";

									qn.NumberValue = drd.GetDoubleByName("rslt_mean_value_nbr");
									qn.TextValue = drd.GetStringByName("rslt_mean_value_txt");

									qn.NValue = drd.GetIntByName("rslt_nbr_vals_incld");
									qn.NValueTested = drd.GetIntByName("rslt_nbr_vals_cnsdrd");
									qn.StandardDeviation = drd.GetDoubleByName("rslt_std_dvtn_nbr");
									qn.StandardError = drd.GetDoubleByName("rslt_std_err_nbr");

									if (qn.IsNull)  // if null since row exists must be underlying data
									{
										qn.TextValue = "ND"; // create not determined text value that we can drill down to
										qn.NValue = 0; // force hyperlink
									}

									qn.DbLink = // drill down for row is assayId, assayTypeId, CorpId
										assayId + "," + assayTypeId + ",CorpId" + corpId + "," + rsltId;
								}
							}

							else if (pMc.DataType == MetaColumnType.Image) // copy link to image
							{
								bool definedValue = false;

								if (unsummarized)
								{
									double value = drd.GetDoubleByName("rslt_value_nbr"); // get numeric value
									definedValue = (value != QualifiedNumber.NullNumber);
									if (!definedValue)
									{
										string rsltValueTxt = drd.GetStringByName("rslt_value_txt");
										if (Lex.IsDefined(rsltValueTxt)) definedValue = true;
									}

									if (definedValue) // if value is defined then store along with link
									{
										string imageRef = assayId + "," + assayTypeId + "," + rsltGrpId + "," + rsltId;
										vo[pVoi] = imageRef;
									}
								}

								else // summarized, store CorpId (with prefix) in result group position
								{
									string imageRef = assayId + "," + assayTypeId + ",CorpId" + corpId + "," + rsltId;
									vo[pVoi] = imageRef;
								}
							}

							else // copy simple scalar value
							{
								uVoi = pMb.PivotedToUnpivotedVoi[pVoi];
								CopyUnpivotedValueToPivotedVo(pMc, drd, uVoi, vo, pVoi);
							}
						}

						if (mbList == null) break; // single broker
						mbIdx++; // go to next broker
						if (mbIdx >= mbList.Count) break; // at end of brokers?
						pMb = (AssayMetaBroker)mbList[mbIdx];
					}
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
			MetaTable uMt, pMt;
			QueryTable uQt, pQt;
			AssayMetaBroker uMb, pMb;
			List<GenericMetaBroker> mbList = null;
			string sql = "", assayList = "", assayIdList, resultTypeList = "";

			// Initialize Unsummarized QueryTable

			if (eqp.QueryTable.MetaTable.UseUnsummarizedData)
			{
				uMt = MetaTableCollection.GetWithException("assay_unpivoted");
				uQt = new QueryTable(uMt);
				uQt.DeselectAll();

				uQt.SelectColumns( // select basic cols that are always needed
					"<colNameList>");
			}

			// Initialize Summarized QueryTable

			else // summarized
			{
				uMt = MetaTableCollection.GetWithException("assay_unpivoted" + MetaTable.SummarySuffix);
				uQt = new QueryTable(uMt);
				uQt.DeselectAll();

				uQt.SelectColumns( // select basic cols that are always needed
					"<colNameList>");
			}

			// Process each metabroker

			foreach (object mbo in mpd.MbInstances.Values)
			{ // accumulate list of cols in unpivoted table corresponding to selected pivoted cols
				int mbIdx = 0;
				pMb = MultiTablePivotBrokerTypeData.GetFirstBroker(mbo, out mbList) as AssayMetaBroker;
				while (true)
				{
					pQt = pMb.Qt;
					foreach (QueryColumn qc2 in pQt.QueryColumns)
					{
						if (!qc2.Selected) continue;
						string unpivColName = GetUnpivotedColumnName(qc2.MetaColumn);
						if (Lex.IsNullOrEmpty(unpivColName)) continue;

						QueryColumn qc = uQt.GetQueryColumnByNameWithException(unpivColName);
						qc.Selected = true;

						string code = qc2.MetaColumn.ResultCode;
						if (!Lex.IsNullOrEmpty(code))
							Lex.AppendItemToStringIfNew(ref resultTypeList, code, ", ");
					}

					if (mbList == null) break; // single broker
					mbIdx++; // go to next broker
					if (mbIdx >= mbList.Count) break; // at end of brokers?
					pMb = mbList[mbIdx] as AssayMetaBroker;
				}
			}

			uMb = new AssayMetaBroker(); // broker for unpivoted query
			ExecuteQueryParms eqp2 = new ExecuteQueryParms(null, uQt);
			eqp2.ReturnQNsInFullDetail = true;
			sql = uMb.BuildSql(eqp2); // build the sql

			uQt.SetSimpleVoPositions(); // set voi position

			foreach (object mbo in mpd.MbInstances.Values)
			{ // setup unpivoted to pivoted vo position mapping 
				int mbIdx = 0;
				pMb = MultiTablePivotBrokerTypeData.GetFirstBroker(mbo, out mbList) as AssayMetaBroker;
				while (true)
				{
					pQt = pMb.Qt;
					pMb.PivotedToUnpivotedVoi = new int[pMb.SelectList.Count]; // allocate array to make from selected pivoted view col to unpivoted view col

					for (int mci = 0; mci < pMb.SelectList.Count; mci++)
					{
						MetaColumn pMc = pMb.SelectList[mci];
						string unpivColName = GetUnpivotedColumnName(pMc);
						if (Lex.IsNullOrEmpty(unpivColName)) continue;

						QueryColumn qc = uQt.GetQueryColumnByNameWithException(unpivColName);
						int uVoi = qc.VoPosition;

						pMb.PivotedToUnpivotedVoi[mci] = uVoi;
					}

					if (mbList == null) break; // single broker
					mbIdx++; // go to next broker
					if (mbIdx >= mbList.Count) break; // at end of brokers?
					pMb = mbList[mbIdx] as AssayMetaBroker;
				}
			}

			string criteria = "";

			assayList = mpd.TableCodeCsvList.ToString();
			if (!Lex.IsNullOrEmpty(assayList))
			{
				assayIdList = GetExternalAssayIdList(mpd.TableCodeDict);

				string[] assayIds = assayList.Split(',');
				if (assayIds.Length > 1000) // Have we exceeded the Oracle 1000 limit for the in clause?
				{
					criteria = BuildSqlInCriteriaFromList("assay_id", assayList);
				}
				else
				{
					Lex.AppendItemToString(ref criteria, " and ", "assay_id in (" + assayList + ") "); // restrict to list of assays
				 //Lex.AppendToList(ref criteria, " and ", "assay_id in (" + assayIdList + ") "); // restrict to list of assay assay_ids (surprisingly this is slower than using assay ids)
				}

				sql = TentativelyActivateInnerAssayIdListCriteria(assayIdList, sql);
			}

			if (!Lex.IsNullOrEmpty(resultTypeList))
				Lex.AppendItemToString(ref criteria, " and ", "rslt_typ_id in (" + resultTypeList + ") ");

			Lex.AppendItemToString(ref criteria, " and ", "<cid> in (<list>)"); // add list placeholder that is filled in at run time 

			sql = ActivateInnerKeyListCriteria(sql, "<list>"); // put list in inner sql also for performance enhancement

			sql += " where " + criteria;

			return sql;
		}


		/// <summary>
		/// Tentatively activate the any assayIdList placeholder in the sql
		/// If ActivateInnerKeyListCriteria is later called then fully activate the assayIdList
		/// This is done as a two-step process since for unsummarized activity/detailed results a
		/// activating the innter assayIdList without also including a keyset makes performance significantly worse.
		/// </summary>
		/// <param name="assayIdList"></param>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string TentativelyActivateInnerAssayIdListCriteria(
			string assayIdList,
			string sql,
			string matchToken = "<assayIdList>")
		{
			if (Lex.IsUndefined(assayIdList)) return sql;

			string[] ids = assayIdList.Split(',');
			if (ids.Length > 1000) return sql; // do nothing for now, needs splitting into OR groups which needs col name

			string matchString = matchToken + "?";
			string replaceString = matchToken + "=" + assayIdList;

			if (Lex.Contains(sql, matchString)) // && !Lex.Contains(sql, replaceString))
				sql = Lex.Replace(sql, matchString, replaceString);

			return sql;
		}

		/// <summary>
		/// ActivateInnerNonKeyListCriteria
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string ActivateInnerNonKeyListCriteria(
			string sql)
		{
			sql = ActivateInnerAssayIdListCriteria(sql, "<assayIdList>");

			sql = AdjustAssayIdListForRestrictedDatabaseView(sql);
			return sql;
		}


		public static string ActivateInnerAssayIdListCriteria(
			string sql,
			string matchToken)
		{
			string matchString = matchToken + "=";
			while (true)
			{
				int i1 = Lex.IndexOf(sql, matchString);
				if (i1 < 0) break;

				int i2 = Lex.IndexOfPrevious(sql, i1, "/*");
				int i3 = Lex.IndexOfNext(sql, i1, "*/");
				if (i2 >= 0 && i3 >= 0)
				{
					sql = Lex.RemoveSubstring(sql, i3, 2);
					sql = Lex.RemoveSubstring(sql, i1, matchString.Length);
					sql = Lex.RemoveSubstring(sql, i2, 2);
				}
			}

			return sql;
		}

		/// <summary>
		/// Adjust SQL for queries on ASSAY_UNPIVOTED and ASSAY_UNPIVOTED_SUMMARY
		/// to filter for restricted database views
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		static string AdjustAssayIdListForRestrictedDatabaseView(string sql)
		{
			string tableNamePrefix;
			int assay;
			bool isSummary;

			HashSet<string> allowedMts = ClientState.UserInfo?.RestrictedViewAllowedMetaTables; // get any set of limited metatables
			if (allowedMts == null) return sql;

			string assayIdList = "";
			foreach (string mtName in allowedMts)
			{
				MetaTable.ParseMetaTableName(mtName, out tableNamePrefix, out assay, out isSummary);
				if (assay <= 0) continue;

				if (Lex.Eq(tableNamePrefix, "ASSAY") || Lex.Eq(tableNamePrefix, "ASSAY")) { }
				else continue;

				if (!AssayIdToAssayId2Map.ContainsKey(assay)) continue;

				if (assayIdList.Length > 0) assayIdList += ",";
				string assayId = AssayIdToAssayId2Map[assay].ToString();
				assayIdList += assayId;
			}

			if (Lex.IsUndefined(assayIdList)) assayIdList = "-1";

			sql = TentativelyActivateInnerAssayIdListCriteria(assayIdList, sql, "<assayIdRestrictedDatabaseList>");
			sql = ActivateInnerAssayIdListCriteria(sql, "<assayIdRestrictedDatabaseList>");

			return sql;
		}

		/// <summary>
		/// Activate CorpIdList criteria for both summarized and unsummarized views
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="newKeyList"></param>
		/// <returns></returns>

		public static string ActivateInnerKeyListCriteria(
			string sql,
			string newKeyList)
		{
			//bool contains_Assay_89493 = sql.Contains("89493"); // debug
			//bool contains_Assay_89494 = sql.Contains("89494");

			if (Lex.Contains(sql, AssaySqlElements.UnsummarizedInnerCorpIdCriteria)) // unsummarized data
			{
				sql = ActivateCriteria(sql, AssaySqlElements.UnsummarizedInnerCorpIdCriteria, "<list>", newKeyList); // unsummarized replacement
				bool activateMaterialize = true;
				if (Lex.Contains(sql, "ASSAY_OWNER.ASSAY_RUN")) activateMaterialize = false; // seems to be a bad interaction between run table and materialize in CidWmSubquerySql (see ASSAY4 query)
				if (activateMaterialize) sql = ActivateMaterializeHint(sql);
			}

			if (Lex.Contains(sql, AssaySqlElements.SummarizedInnerCidCriteria)) // summarized data
			{
				sql = ActivateCriteria(sql, AssaySqlElements.SummarizedInnerCidCriteria, "<list>", newKeyList); // summarized replacement
				sql = ActivateMaterializeHint(sql);  // need to materialize for summary data joined to CMN_ASSY_ATRBTS
			}

			sql = ActivateInnerNonKeyListCriteria(sql);

			return sql;
		}

		/// <summary>
		/// Activate materialize hint in SQL
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string ActivateMaterializeHint(string sql)
		{
			string inactiveMaterialize = "/* materialize */"; // also activate the materialize keyword
			string activeMaterialize = "/*+ materialize */";

			if (Lex.Contains(sql, inactiveMaterialize))
				sql = Lex.Replace(sql, inactiveMaterialize, activeMaterialize);

			return sql;
		}

		/// <summary>
		/// Substitute commented innercriteria with actual criteria for performance enhancement
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="matchString"></param>
		/// <returns></returns>

		public static string ActivateCriteria(
			string sql,
			string matchString)
		{
			return ActivateCriteria(sql, matchString, matchString);
		}

		/// <summary>
		/// Substitute commented innercriteria with actual criteria for performance enhancement
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="matchString"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>

		public static string ActivateCriteria(
			string sql,
			string matchString,
			string replacement)
		{
			return ActivateCriteria(sql, matchString, "", replacement);
		}

		/// <summary>
		/// /// Substitute commented innercriteria with actual criteria for performance enhancement
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="matchString"></param>
		/// <param name="matchSubstring"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>

		public static string ActivateCriteria(
			string sql,
			string matchString,
			string matchSubstring,
			string replacement)
		{
			string matchCommented = "/*" + matchString + "*/";
			if (!Lex.Contains(sql, matchCommented)) return sql;

			string[] ids = replacement.Split(',');
			if (ids.Length > 1000) return sql; // do nothing for now, needs splitting into OR groups which needs col name

			// todo if needed: split into "or" groups
			//{ 
			//	replacement = BuildSqlInCriteriaFromList(columnName, replacement);
			//	replacement = " and " + replacement;
			//}

			else
			{
				if (Lex.IsDefined(matchSubstring)) replacement = Lex.Replace(matchString, matchSubstring, replacement);
			}

			sql = Lex.Replace(sql, matchCommented, replacement);
			return sql;
		}

		/// <summary>
		/// Convert list of external assay ids to list of internal database-specific assay ids
		/// </summary>
		/// <param name="codeDict"></param>
		/// <returns></returns>

		string GetExternalAssayIdList(Dictionary<string, MpdResultTypeData> codeDict)
		{
			string assayIdList = "";

			foreach (string assayString in codeDict.Keys)
			{
				int assay = int.Parse(assayString);
				if (!AssayIdToAssayId2Map.ContainsKey(assay)) continue;

				if (assayIdList.Length > 0) assayIdList += ",";
				string assayId = AssayIdToAssayId2Map[assay].ToString();
				assayIdList += assayId;
			}

			return assayIdList;
		}

		/// <summary>
		/// Get next row of data using metabroker buffering for speed where possible
		/// </summary>
		/// <returns></returns>

		public override Object[] NextRow()
		{
			//System.Threading.Thread.Sleep(1000); // debug (delay)

			if (!PivotInCode) // if not multipivot then call generic broker 
				return base.NextRow();

			object[] vo = GetNextMultipivotRowDictVo();
			return vo;
		}

		/// <summary>
		/// Build sql to select unpivoted assay results for the UnpivotedAssayMetaBroker
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string BuildUnpivotedAssayResultsSql(
			ExecuteQueryParms eqp)
		{
			QueryTable qt, qt2;
			QueryColumn qc2;
			MetaTable mt, mt2;
			MetaColumn mc, mc2;
			string mtName, mtName2, mcName, mapsTo, exprs = "", map0 = "", tok;

			qt = eqp.QueryTable;
			mt = qt.MetaTable;
			mc = mt.KeyMetaColumn;

			Eqp = eqp;
			Qt = eqp.QueryTable;

			mtName2 = "assay_unpivoted";

			if (mt.UseSummarizedData) mtName2 += MetaTable.SummarySuffix;
			mt2 = MetaTableCollection.GetWithException(mtName2);
			qt2 = new QueryTable(mt2);
			qt2.DeselectAll();
			qt2.Alias = qt.Alias; // use same alias for all inner tables

			Dictionary<string, string> map = UnpivotedAssayResultsColMap;

			foreach (QueryColumn qc in qt.QueryColumns)
			{
				mc = qc.MetaColumn;
				mcName = mc.Name;
				//if (mt.UseUnsummarizedData && !mc.UnsummarizedExists) continue;
				//if (mt.UseSummarizedData && !mc.SummarizedExists) continue;

				//				map0 += "{\"" + mc.Name + "\", \"" + mc.Name + "\"},\r\n"; // build the initial map

				mapsTo = "null";
				qc2 = null;
				if (map.ContainsKey(mcName)) // in map?
				{
					mapsTo = map[mcName]; // get value mapped to
					if (Lex.IsDefined(mapsTo) && !mapsTo.StartsWith("'") && Lex.Ne(mapsTo, "null")) // if defined and not a constant check for col name in metatable
					{
						qc2 = qt2.GetQueryColumnByName(mapsTo);
						if (qc2 == null) mapsTo = "null"; // shouldn't happen
						else
						{
							qc2.Selected = true;
							qc2.SortOrder = qc.SortOrder;
							qc2.Criteria = qc.Criteria;
							qc2.CriteriaDisplay = qc.CriteriaDisplay;
						}
					}
				}

				if (qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted)
				{
					if (exprs.Length > 0) exprs += ", ";
					exprs += mapsTo;
					if (Lex.Ne(mcName, mapsTo)) exprs += " " + mcName;
				}

			}

			ExecuteQueryParms eqp2 = eqp.Clone();
			eqp2.QueryTable = qt2;
			eqp2.CallerSuppliedCriteria = "";

			string sql = BuildSql(eqp2);

			if (Lex.Contains(sql, AssaySqlElements.CaaOuterJoinExpr)) // remove outer join expression that allows non-key result types to be included
				sql = Lex.Replace(sql, AssaySqlElements.CaaOuterJoinExpr, "");

			sql = // build full expr
			"select " + exprs +
			" from (" + sql + ")";

			return sql;
		}

		/// <summary>
		/// Column map from the UNPIVOTED_ASSAY_RESULTS integrated view columns to the corresponging ASSAY_UNPIVOTED columns
		/// </summary>
		Dictionary<string, string> UnpivotedAssayResultsColMap
		{
			get
			{
				Dictionary<string, string> map = new Dictionary<string, string> {
					{"<cid>", "<cid>"},
					{"NTBK_RFRNC_TXT", "NTBK_RFRNC_TXT"},
					{"CORP_SBMSN_ID", "CORP_SBMSN_ID"},
					{"ASSY_DB", "'ASSAY'"}, // all rows from  ASSAY database
					{"ASSY_NM", "ASSAY_NM"},
					{"ASSY_TYP", "ASSY_TYP"},
					{"ASSY_MODE", "ASSY_MODE"},
					{"ASSY_SRC", "null"},
					{"ASSY_ID", "ASSAY_ID"},
					{"ASSY_ID_NBR", "ASSAY_ID"},
					{"ASSY_ID_TXT", "ASSY_ID_TXT"},
					{"ASSY_SUM_LVL", "null"},
					{"GENE_SYMBL", "GENE_SYMBL"},
					{"GENE_ID", "GENE_ID"},
					{"GENE_FMLY", "GENE_FMLY"},
					{"ASSY_GENE_CNT", "ASSY_GENE_CNT"},
					{"RSLT_TYP_NM", "RSLT_TYP_NM"},
					{"RSLT_TYP", "RSLT_TYP"},
					{"RSLT_TYP_ID_SECONDARY", "SECONDARY_RSLT_TYP_ID"},
					{"RSLT_TYP_ID_NBR", "RSLT_TYP_ID"},
					{"RSLT_TYP_ID_TXT", "RSLT_TYP_ID_TXT"},
					{"TOP_LVL_RSLT", "TOP_LVL_RSLT"},
					{"RSLT_VALUE", "RSLT_VALUE"},

					{"RSLT_VALUE_NBR", "RSLT_VALUE_NBR"},
					{"RSLT_PRFX_TXT", "RSLT_PRFX_TXT"},
					{"RSLT_VALUE_TXT", "RSLT_VALUE_TXT"},
					{"RSLT_UNITS", "RSLT_UNITS"},

					{"RSLT_MEAN_VALUE_NBR", "RSLT_MEAN_VALUE_NBR"},
					{"RSLT_MEAN_PRFX_TXT", "RSLT_MEAN_PRFX_TXT"},
					{"RSLT_MEAN_VALUE_TXT", "RSLT_MEAN_VALUE_TXT"},
					{"RSLT_NBR_VALS_CNSDRD", "RSLT_NBR_VALS_CNSDRD"},
					{"RSLT_NBR_VALS_INCLD", "RSLT_NBR_VALS_INCLD"},
					{"RSLT_NBR_EXPRMNTS_INCLD", "RSLT_NBR_EXPRMNTS_INCLD"},
					{"RSLT_STD_DVTN_NBR", "RSLT_STD_DVTN_NBR"},
					{"RSLT_STD_ERR_NBR", "RSLT_STD_ERR_NBR"},
					{"RSLT_MEAN_UNITS", "RSLT_MEAN_UNITS"},

					{"RSLT_CONC_NBR", "RSLT_CONC_NBR"},
					{"RSLT_CONC_UNITS", "RSLT_CONC_UNITS"},
					{"ACTIVITY_BIN", "ACTIVITY_BIN"},
					{"ACTIVITY_CLASS", "ACTIVITY_CLASS"},
					{"COND_FMT_COLOR", "null"},
					{"STRT_DT", "STRT_DT"},
					{"RSLT_NTBK_LBL_TXT", "RSLT_NTBK_LBL_TXT"},
					{"SITE_NM", "SITE_NM"},

					{"RSLT_GRP_ID", "RSLT_GRP_ID"},
					{"RSLT_ID", "RSLT_ID"},
					{"TARGET_MAP_X", "TARGET_MAP_X"},
					{"TARGET_MAP_Y", "TARGET_MAP_Y"}
				};

				return map;
			}
		}

		/// <summary>
		/// Build the sql for the query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			Hashtable resultKeys;
			string sql;
			int ci;

			try
			{
				Eqp = eqp;
				Qt = eqp.QueryTable;

				Exprs = FromClause = OrderBy = ""; // outer sql elements
				InnerTables = InnerExprs = InnerCriteria = "";

				KeyMc = Mt.KeyMetaColumn;
				resultKeys = new Hashtable();
				SelectList = new List<MetaColumn>(); // list of selected metacolumns

				// Add key column to selected list if not already selected

				if (Mt.KeyMetaColumn == null)
					throw new Exception("Key column not found for MetaTable " + Mt.Name);

				for (ci = 0; ci < Qcl.Count; ci++)
				{
					qc = Qcl[ci];
					mc = qc.MetaColumn;
					if (!mc.IsKey) continue;
					if (qc.Selected) break;
				}

				if (ci >= Qcl.Count) // put key at start of list if not already selected
				{
					SelectList.Add(Mt.KeyMetaColumn);
					KeyVoi = 0;
					Exprs = Mt.KeyMetaColumn.ColumnMap;
					if (Mt.KeyMetaColumn.Name != Exprs) // append name if different than map
						Exprs += " " + Mt.KeyMetaColumn.Name;
				}

// Build sql to pivot data out of unsummarized tables

				if (Qt.MetaTable.UseUnsummarizedData)
					BuildPivotedUnsummarizedSql(); // build sql to pivot data out of unsummarized tables

// Build sql to pivot data out of summarized table

				else BuildPivotedSummarizedSql();

				// Put final SQL together

				FromClause = AdjustSqlForNotExistsCriteriaAsNeeded(eqp, Exprs, FromClause);

				sql =
					"select /*+ hint */ " + Exprs +
					" from " + FromClause;

				if (Qt.Alias != "") sql += " " + Qt.Alias;
				if (eqp.CallerSuppliedCriteria != "")
					sql += " where " + eqp.CallerSuppliedCriteria;

				if (CanConvertOuterJoinToEquijoin (Qt, AssaySqlElements.CaaResultTypeAttrTable))
				{ // Optimization: If criteria exist on a CAA column then convert CAA outer joins to equi-joins for better performance
					if (Lex.Contains(sql, AssaySqlElements.CaaOuterJoinExpr)) 
						sql = Lex.Replace(sql, AssaySqlElements.CaaOuterJoinExpr, "");
				}

				return sql;
			}

			catch (Exception ex) { throw new Exception(ex.Message, ex); }
		}

		/// <summary>
		/// Return true if an join can be done in place of an outer join for the specified query table and owner.tableName
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="outerjoinTable"></param>
		/// <returns></returns>

		bool CanConvertOuterJoinToEquijoin(
			QueryTable qt,
			string outerjoinTable)
		{
			if (qt.Query != null && qt.Query.LogicType == QueryLogicType.And)
			{
				if (QtContainsCriteriaForColumnTableMap(qt, outerjoinTable))
					return true;
			}

			return false;
		}


/// <summary>
/// Return true if QueryTable contains criteria on a column associated with specified owner.tableName
/// </summary>
/// <param name="qt"></param>
/// <param name="tableMap"></param>
/// <returns></returns>

		bool QtContainsCriteriaForColumnTableMap(
			QueryTable qt,
			string tableMap)
		{
			if (qt.MetaTable.IgnoreCriteria) return false;

			foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (Lex.IsDefined(qc.Criteria) && Lex.Eq(qc.MetaColumn.TableMap, tableMap))
						return true;
				}
			return false;
		}

		/// <summary>
		/// Build sql for retrieving summarized data for a single asssay
		/// </summary>

		void BuildPivotedSummarizedSql()
		{
			////////////////////////////////////////////////////////////////////////////////
			// Build sql for summarized data
			////////////////////////////////////////////////////////////////////////////////
			//
			// 1. An inner query in the from clause pivots out the columns that are
			//    selected or have criteria
			// 
			// 2. assay_cmpnd (t0) is at the top of the the inner query and provides the cid
			// 
			// 3. Pivoted data is joined against t0.assay_cmpnd.
			//
			////////////////////////////////////////////////////////////////////////////////

			QueryColumn qc;
			MetaColumn mc;
			PivotGroup pivGrp = null;
			string tableAlias, tok;
			int assayId = -1, qci;

			PivotGroups = new PivotGroupCollection(this);

			AnalyzeQueryTable(); // do an analysis of the QueryTable

			PivotGroups.InitializeBaseGroup(); // select and init the base group

			InnerExprs = InnerTables = InnerCriteria = "";

			int pivotCount = 0;

			// Generate sql for each column that needs to be retrieved

			//if (JoinOnConcentration || // need to join on concentration value also in this case
			//(Eqp.Qe.Query != null && Eqp.Qe.Query.Preview)) // if preview then we need to be sure there is data for the compound id.
			//{
			//  BuildKeyOnlySummarizedT0(); // todo: fix this?
			//}

			// Process query column objects

			for (qci = 0; qci < Qcl.Count; qci++)
			{

				qc = Qcl[qci];
				mc = qc.MetaColumn;
				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				pivGrp = PivotGroups.AddQcToPivotGroup(qc);
				tableAlias = pivGrp.GroupName;

				if (mc.SummarizedExists)
				{
					if (mc.DataType == MetaColumnType.QualifiedNo)
						AppendSummarizedAvgQualifiedNoExprToPivotGroup(pivGrp, qc, null, null, true);

					else if (mc.DataType == MetaColumnType.Image)
						AppendSummarizedImageRefExprToPivotGroup(pivGrp, qc);

					else AppendScalarExpr(pivGrp, qc);

					if (IsPivotedColumn(mc)) pivotCount++;
				}

				else InnerExprs += ", null " + mc.Name; // no summarized value

				// Add to select list if selected

				if (qc.Is_Selected_or_GroupBy_or_Sorted) // if selected add to expression list
				{
					tok = mc.Name;
					if (Mt.KeyMetaColumn == mc)
						KeyVoi = SelectList.Count;

					if (Exprs.Length > 0) Exprs += ",";
					Exprs += tok;
					if (mc.Name != tok) Exprs += " " + mc.Name;
					SelectList.Add(mc);
				}
			} // end of column loop

			// Build the FromClause that returns the full table expression from the accumulated groups

			FromClause = PivotGroups.BuildFullBrokerFromClauseSql(this);

			// Plug assay code into "with" clause to improve performance

			string assayIdList = GetInternalAssayIdList(Qt);
			if (Lex.IsDefined(assayIdList))
			{
				FromClause = ActivateCriteria(FromClause, AssaySqlElements.SummarizedInnerAssayCriteria, "<assayIdList>?", assayIdList); // plug in desired ASSAY assay ids
			}

			return;
		}

		string GetInternalAssayIdList(
			QueryTable qt)
		{
			int assayId2 = -1;
			MetaTable mt = qt.MetaTable;

			if (int.TryParse(mt.Code, out assayId2) && AssayIdToAssayId2Map.ContainsKey(assayId2))
			{
				string assayId = AssayIdToAssayId2Map[assayId2].ToString();
				return assayId;
			}
            // GetAssayIdList

		    QueryColumn qc = qt.GetQueryColumnByName("assay_id");
		    if (qc != null && Lex.IsDefined(qc.Criteria))
		    {
		        ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);

		        if (psc == null) return "";
		        string assayIdList;
		        if (psc.OpEnum == CompareOp.Eq && Lex.IsDefined(psc.Value))
		            assayIdList = psc.Value;

		        else if (psc.OpEnum == CompareOp.In && psc.ValueList != null && psc.ValueList.Count > 0)
		            assayIdList = Lex.ListToCsvString(psc.ValueList, false);

		        else return "";

		        string idList = ConvertExternalAssayIdsToInternalAssayIds(assayIdList);
		        return idList;
		    }

            if (IsMergedTable(qt.MetaTable))
            {
                string assayIdList = "";
                foreach (string s in qt.MetaTable.TableFilterValues)
                {
                    if (assayIdList.Length > 0) assayIdList += ",";
                    assayIdList += ConvertExternalAssayIdsToInternalAssayIds(s);
                }
                return assayIdList;
            }
		    return "";
		}

		/// <summary>
		/// Build sql for retrieving unsummarized data
		/// </summary>

		void BuildPivotedUnsummarizedSql()
		{

			////////////////////////////////////////////////////////////////////////////////
			// Build sql for unsummarized sql for detailed/activity data
			////////////////////////////////////////////////////////////////////////////////
			//			
			// 1. The columns that the broker must retrieve are organized by InternalTableColumnGroups
			//     where each group is associated with a specific result code. The group generates sql
			//     to return all of the columns associated with it's result type.
			// 
			// 2. One group that generates exactly one row for each RSLT_GRP_ID is picked as the base
			//     group. If no such result type exists then a special non-result type expression is used
			//     to generate one row for each RSLT_GRP_ID for the associated assay. 
			//     All other column groups join against the BaseGroup by RSLT_GRP_ID;
			// 
			// 3. The sql generated consists of InnerExprs which are the accessed column set
			//     qualified by the group aliases and the MetaColumn name.
			//     InnerTables contain the sql for the groups with one block of sql for each group.
			//     InnerCriteria contain the joins on RSLT_GRP_ID between the base group and each
			//     of the other groups.
			//
			////////////////////////////////////////////////////////////////////////////////

			QueryColumn qc;
			MetaColumn mc;
			PivotGroup pivGrp = null;
			string tableAlias, tok;
			int assayId = -1, qci;

			PivotGroups = new PivotGroupCollection(this);

			AnalyzeQueryTable(); // do an analysis of the QueryTable

			PivotGroups.InitializeBaseGroup(); // select and init the base group 

			InnerExprs = InnerTables = InnerCriteria = "";

			int pivotCount = 0;

			// Generate sql for each column that needs to be retrieved

			for (qci = 0; qci < Qcl.Count; qci++)
			{
				qc = Qcl[qci];
				mc = qc.MetaColumn;
				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				pivGrp = PivotGroups.AddQcToPivotGroup(qc);
				tableAlias = pivGrp.GroupName;

				if (mc.UnsummarizedExists)
				{
					//if (mc.Name == "RSLT_IMG_URL") mc = mc; // debug

					if (mc.DataType == MetaColumnType.QualifiedNo)
						AppendUnsummarizedQualifiedNoExprToPivotGroup(pivGrp, qc, tableAlias, null, null, true);

					else if (mc.DataType == MetaColumnType.Image) // crc curve (rslt_img_url) - new pivoted images
						AppendUnsummarizedImageRefExprToPivotGroup(pivGrp, qc);

					else AppendScalarExpr(pivGrp, qc);

					if (IsPivotedColumn(mc)) pivotCount++;
				}

				else InnerExprs += ", null " + mc.Name;  // no unsummarized value

				// Add to select list if selected

				if (qc.Is_Selected_or_GroupBy_or_Sorted) // if selected add to expression list
				{
					tok = mc.Name;
					if (Mt.KeyMetaColumn == mc)
						KeyVoi = SelectList.Count;

					if (Exprs.Length > 0) Exprs += ",";
					Exprs += tok;
					if (mc.Name != tok) Exprs += " " + mc.Name;
					SelectList.Add(mc);
				}
			} // end of column loop

			// Build the FromClause that returns the full table expression from the accumulated groups

			FromClause = PivotGroups.BuildFullBrokerFromClauseSql(this);

			// Plug assay code into "with" clause to improve performance
            
			string assayIdList = GetInternalAssayIdList(Qt);
			FromClause = TentativelyActivateInnerAssayIdListCriteria(assayIdList, FromClause);
			return;
		}

		/// <summary>
		/// Do an analysis of the QueryTable
		/// </summary>

		internal void AnalyzeQueryTable()
		{
			QueryColumn qc;
			MetaColumn mc;
			int qci;

			DetailedResultPivotedColsFetched = 0; // number ASSAY_DTLD_RSLT table pivoted cols to fetch (Selected_or_Criteria_or_Sorted)
			ActivityResultPivotedColsFetched = 0; // number ASSAY_ACTVTY_RSLT table pivoted cols to fetch (Selected_or_Criteria_or_Sorted)
			SelectCnt = CriteriaCnt = 0;
			PivotedColumns = new List<QueryColumn>();
			PivotedCodes = new HashSet<string>();
			FirstPivotedColWithCriteria = null; // first pivoted column with criteria used for query optimization
			FirstPivotedColWithAllResults = null; // first pivoted column with a database result row for each result group for the assay

			KeyMc = Mt.KeyMetaColumn;
			SelectList = new List<MetaColumn>(); // list of selected metacolumns

			for (qci = 0; qci < Qcl.Count; qci++)
			{
				qc = Qcl[qci];
				mc = qc.MetaColumn;
				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				if (IsPivotedColumn(mc)) // result column to pivot?
				{
					PivotedColumns.Add(qc);
					PivotedCodes.Add(mc.ResultCode);

					if (mc.SummarizationRole == SummarizationRole.Dependent) DetailedResultPivotedColsFetched++;
					else if (mc.SummarizationRole == SummarizationRole.Derived) ActivityResultPivotedColsFetched++;
				}

				if (qc.Is_Selected_or_GroupBy_or_Sorted)
				{
					SelectCnt++;
				}

				if (qc.Criteria != "")
				{
					CriteriaCnt++;
					if (IsPivotedColumn(qc.MetaColumn) && FirstPivotedColWithCriteria == null)
					{
						FirstPivotedColWithCriteria = qc;
					}
				}

			}

			if (FirstPivotedColWithCriteria != null && // firstPivotedColWithCriteria defined?
			 ((Eqp.Qe.MqlLogicType != QueryLogicType.And && CriteriaCnt > 1) || // don't use this optimization unless "and" logic
			 Lex.EndsWith(FirstPivotedColWithCriteria.Criteria, " IS NULL"))) // don't use if "Is Null" predicate
				FirstPivotedColWithCriteria = null;

			return;
		}

		/// <summary>
		/// Convert list of external assay ids associated with Metatable to a list of internal database-specific assay ids
		/// </summary>
		/// <returns></returns>

		internal string GetInternalAssayIdList()
		{
			string assayIdList = GetAssayIdList();
			string assayIdList2 = ConvertExternalAssayIdsToInternalAssayIds(assayIdList);
			return assayIdList2;
		}

		/// <summary>
		/// Convert a list of RDW assay ids to the corresponding list of ASSAY assay ids
		/// </summary>
		/// <param name="assayIdList"></param>
		/// <returns></returns>

		static internal string ConvertExternalAssayIdsToInternalAssayIds(string assayIdList)
		{
			StringBuilder sb = new StringBuilder();

			if (Lex.IsNullOrEmpty(assayIdList)) return "";
			string[] sa = assayIdList.Split(',');

			foreach (string s in sa)
			{
				if (Lex.IsNullOrEmpty(s)) continue;

				int assayId = int.Parse(s.Trim());
				if (AssayIdToAssayId2Map.ContainsKey(assayId))
				{
					if (sb.Length > 0) sb.Append(", ");
					sb.Append(AssayIdToAssayId2Map[assayId].ToString());
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Return a list of the codes associated with the table
		/// Normally one but can be more if merged assays
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		string GetAssayIdList()
		{
			if (!IsMergedTable(Mt)) return Mt.Code;

			else
			{
				string tableCodes = "";
				foreach (string tableCode in Mt.TableFilterValues)
				{
					if (tableCodes != "") tableCodes += ", ";
					tableCodes += tableCode;
				}
				return tableCodes;
			}
		}

		/// <summary>
		/// Return true if this table represents a merging of multiple sub assays
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		bool IsMergedTable(MetaTable mt)
		{
			if (mt.TableFilterValues != null && mt.TableFilterValues.Count >= 0) return true;
			else return false;
		}

		/// <summary>
		/// Append expression for getting non-averaged qualified number value
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="mc"></param>
		/// <param name="pivotExprs"></param>

		void AppendUnsummarizedQualifiedNoExprToPivotGroup(
			PivotGroup pivGrp,
			QueryColumn qc,
			string tableAlias,
			string tableIdExpr,
			string columnIdExpr,
			bool includeComparisonValue)
		{
			string expr;
			MetaColumn mc = qc.MetaColumn;

			if (Lex.IsDefined(mc.ResultCode)) // be sure any result code is set for the group
				pivGrp.AssayResultTypeIds = mc.ResultCode;

			pivGrp.AddTableToPivotGroupAsNeeded(mc.TableMap);

			tableAlias = "r"; // fixed alias for results table

			if (Eqp.ReturnQNsInFullDetail)
			{
				if (qc.Is_Selected_or_GroupBy_or_Sorted)
				{
					expr =
						tableAlias + ".prfx_txt || chr(11) || " + // prefix
						tableAlias + ".value_nbr || chr(11) || " + // basic number
						tableAlias + ".value_txt || chr(11) || "; // text

					expr +=
						"chr(11) || " + // number of values in stats
						"chr(11) || " + // number of runs
						"chr(11) || " + // standard deviation
						"chr(11) || "; // standard error

					if (!String.IsNullOrEmpty(tableIdExpr))
						expr += tableIdExpr + " || ',' || ";
					if (!String.IsNullOrEmpty(columnIdExpr))
						expr += columnIdExpr + " || ',' || ";

					expr += // include info needed for drill down
						"r.assay_id || ',' || " + // assay assay id
						"r.rslt_typ_id || ',' || " + // assay result type id
						"r.rslt_grp_id || ',' || " + // result group id
						"r.rslt_id"; // result id

					pivGrp.AddGroupExpr(expr, mc.Name);
				}

				if (includeComparisonValue)
				{
					expr = "";
					if (qc.Criteria.ToLower().EndsWith(" is not null")) // if is-not-null criteria then any value will do not just number
						expr += tableAlias + ".rslt_typ_id";

					else expr += // numeric column for comparison
						tableAlias + ".value_nbr";

					pivGrp.AddGroupExpr(expr, mc.Name + "_val ");
				}
			}

			else AppendScalarExpr(pivGrp, qc); // just return float value
			//{
			//  expr = tableAlias + ".value_nbr ";
			//  itg.AddGroupExpr(expr,  mc.Name);
			//}

			return;
		}

		/// <summary>
		/// Append expression for getting CRC image reference
		/// </summary>
		/// <param name="pivGrp"></param>
		/// <param name="qc"></param>

		void AppendUnsummarizedImageRefExprToPivotGroup(
			PivotGroup pivGrp,
			QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;

			if (Lex.IsDefined(mc.ResultCode)) // be sure any result code is set for the group
				pivGrp.AssayResultTypeIds = mc.ResultCode; // be sure any result code is set for the group
			pivGrp.AddTableToPivotGroupAsNeeded(mc.TableMap);

			string tableAlias = "r"; // fixed alias for results table

			string expr = // include info needed for drill down
				"r.assay_id || ',' || " + // assay assay id
				"r.rslt_typ_id || ',' || " + // assay result type id
				"r.rslt_grp_id || ',' || " + // result group id
				"r.rslt_id"; // result id

			pivGrp.AddGroupExpr(expr, mc.Name);

			return;
		}

		/// <summary>
		/// Check if two metacolumns are the same or have equivalent values 
		/// Allows calc fields with overlaid curves to align with data from source ASSAY tables
		/// </summary>
		/// <param name="mc1"></param>
		/// <param name="mc2"></param>
		/// <returns></returns>

		public override bool ColumnValuesAreEquivalent(
			MetaColumn mc1,
			MetaColumn mc2)
		{
			if (Lex.Eq(mc1.Name, mc2.Name)) return true; // check if columns match on name

			if (mc1.ResultCode != mc2.ResultCode) return false;

			if (mc1.DataType == mc2.DataType) return true;

			if (mc1.DataType == MetaColumnType.QualifiedNo && mc2.DataType == MetaColumnType.Image) return true;

			if (mc2.DataType == MetaColumnType.QualifiedNo && mc1.DataType == MetaColumnType.Image) return true;

			return false;
		}

		/// <summary>
		/// Convert a database result value to an object reference
		/// This converts Qualified Numbers (e.g. IC50) values into an image reference that can be used 
		/// to render a CRC curve and is used to align ASSAY data for multiple assays with overlaid curve
		/// calculated fields.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>

		public override string ConvertValueToImageReference(object v)
		{
			if (NullValue.IsNull(v)) return null;

			else if (v is QualifiedNumber)
			{
				string imageRef = ((QualifiedNumber)v).DbLink;
				return imageRef;
			}

			else return v.ToString();
		}

		/// <summary>
		/// Append expression for getting averaged qualified number value
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="mc"></param>
		/// <param name="pivotExprs"></param>

		void AppendSummarizedAvgQualifiedNoExprToPivotGroup(
			PivotGroup pivGrp,
			QueryColumn qc,
			string tableIdExpr,
			string columnIdExpr,
			bool includeComparisonValue)
		{
			string expr;
			MetaColumn mc = qc.MetaColumn;

			if (Lex.IsDefined(mc.ResultCode)) // be sure any result code is set for the group
				pivGrp.AssayResultTypeIds = mc.ResultCode;
			pivGrp.AddTableToPivotGroupAsNeeded(mc.TableMap);

			string tableAlias = "r"; // fixed alias for results table

			if (Eqp.ReturnQNsInFullDetail)
			{
				if (qc.Is_Selected_or_GroupBy_or_Sorted)
				{
					expr =
						tableAlias + ".mean_prfx_txt || chr(11) || " + // prefix
						tableAlias + ".mean_value_nbr || chr(11) || " + // basic number
						tableAlias + ".mean_value_txt || chr(11) || " + // text 
						tableAlias + ".nbr_vals_incld || chr(11) || " + // number of values in stats
						tableAlias + ".nbr_vals_cnsdrd || chr(11) || " + // number of values considered
						tableAlias + ".std_dvtn_nbr || chr(11) || " + // standard deviation
						tableAlias + ".std_err_nbr || chr(11) || "; // standard error

					if (!String.IsNullOrEmpty(tableIdExpr))
						expr += tableIdExpr + " || ',' || ";
					if (!String.IsNullOrEmpty(columnIdExpr))
						expr += columnIdExpr + " || ',' || ";

					expr += // include info needed for drill down
						"r.assay_id || ',' || " + // assay assay id
						"r.rslt_typ_id || ',CorpId' || " + // assay result type id
						"r.corp_id || ',' || " + // CorpId
						"r.smrzd_rslt_id"; // result id

					pivGrp.AddGroupExpr(expr, mc.Name);
				}

				if (includeComparisonValue)
				{
					expr = "";
					if (qc.Criteria.ToLower().EndsWith(" is not null")) // if is-not-null criteria then any value will do not just number
						expr += tableAlias + ".rslt_typ_id";

					else expr += // numeric column for comparison
						tableAlias + ".mean_value_nbr";

					pivGrp.AddGroupExpr(expr, mc.Name + "_val ");
				}
			}

			else AppendScalarExpr(pivGrp, qc); // just return float value

			return;
		}

		/// <summary>
		/// Append expression for getting CRC image reference
		/// </summary>
		/// <param name="pivGrp"></param>
		/// <param name="qc"></param>

		void AppendSummarizedImageRefExprToPivotGroup(
			PivotGroup pivGrp,
			QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;

			if (Lex.IsDefined(mc.ResultCode)) // be sure any result code is set for the group
				pivGrp.AssayResultTypeIds = mc.ResultCode; // be sure any result code is set for the group
			pivGrp.AddTableToPivotGroupAsNeeded(mc.TableMap);

			string tableAlias = "r"; // fixed alias for results table

			string expr = // include info needed for drill down
				"r.assay_id || ',' || " + // assay assay id
				"r.rslt_typ_id || ',<CID>' || " + // assay result type id
				"r.<cid> || ',' || " + // CorpId
				"r.smrzd_rslt_id"; // result id

			pivGrp.AddGroupExpr(expr, mc.Name);

			return;
		}

		/// <summary>
		/// Append a simple scalar expression to a pivot group
		/// </summary>
		/// <param name="pivGrp"></param>
		/// <param name="qc"></param>

		void AppendScalarExpr(
			PivotGroup pivGrp,
			QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;

			if (Lex.IsDefined(mc.ResultCode)) // be sure any result code is set for the group
				pivGrp.AssayResultTypeIds = mc.ResultCode;

			string expr = GetSimpleMapExpr(mc);
			pivGrp.AddGroupExpr(expr, mc.Name);

			pivGrp.AddTableToPivotGroupAsNeeded(mc.TableMap);

			return;
		}

		/// <summary>
		/// Get a simple aliased column expression
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		string GetSimpleMapExpr(MetaColumn mc)
		{
			if (Lex.Eq(mc.ColumnMap, "null")) return mc.ColumnMap; // metacolumn returns null and maps to null
			else if (mc.ColumnMap.Contains("(") && mc.ColumnMap.Contains(")")) return mc.ColumnMap; // just return columnMap if complex expression

			string alias = OracleTableRelations.Get(mc.TableMap, false).Alias;
			string expr = alias + "." + mc.ColumnMap;
			return expr;
		}

		/// <summary>
		/// Return true if specified column is a pivoted column
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static bool IsPivotedColumn(
			MetaColumn mc)
		{
			if (mc.ResultCode == "") return false;
			//else if (Lex.Eq("rslt_img_url", mc.Name)) return false; // not pivoting for images now
			else return true;
		}

		/// <summary>
		/// Map pivoted view column name to associated unpivoted view column name
		/// Cases:
		/// 1. Names associated with result types: R_n_xxx -> RSLT_xxx
		/// 2. Other names are the same
		/// 3. A few special cases
		/// </summary>
		/// <param name="name">Pivoted view column name</param>
		/// <returns>Unpivoted view column name</returns>

		public static string GetUnpivotedColumnName(MetaColumn mc)
		{
			int i1;

			bool summarized = mc.MetaTable.UseSummarizedData;

			string name = mc.Name.ToUpper();
			if (Lex.EndsWith(name, "RSLT_IMG_URL")) return "rslt_id"; // return unpivoted result id col if CRC curve

			if (!IsPivotedColName(name)) return name; // if unpivoted, return as is

			// Result column associated with a result type (e.g. R_1, R_1_VALUE_NBR_QND, R_1_UNITS)

			for (i1 = 2; i1 < name.Length; i1++) // find 2nd underscore
			{
				if (name[i1] == '_') break;
			}

			if (i1 >= name.Length)  // basic value (e.g. R_1)
			{
				name = "RSLT_" + mc.ColumnMap;
				return name;
			}

			else // some simple value other than the main result value
			{
				name = name.Substring(i1 + 1); // get basic name without result type code

				if (summarized && name == "UNITS") // UNITS -> MEAN_UNITS if summarized (special case)
					name = "MEAN_UNITS";

				if (name == "CMPND_CONC_VAL_NBR") // CMPND_CONC_VAL_NBR -> CONC_NBR (special case)
					name = "CONC_NBR";

				name = "RSLT_" + name; // add result prefix used in unpivoted metatable
			}

			if (Lex.EndsWith(name, "_QND")) // remove any _QND suffix
				name = Lex.Replace(name, "_QND", "");

			return name;
		}

		/// <summary>
		/// Build query that returns detailed drilldown data on a result
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="metaColumnId"></param>
		/// <param name="level"></param>
		/// <param name="linkInfo"></param>
		/// <returns></returns>

		public override Query GetDrilldownDetailQuery(
			MetaTable mt,
			string metaColumnId,
			int level,
			string linkInfo)
		{
			foreach (MetaColumn mc in mt.MetaColumns) // look for matching code (summarized or unsummarized)
			{
				if (mc.ResultCode == metaColumnId)
					return GetDrilldownDetailQuery(mt, mc, level, linkInfo);
			}

			return null;
		}

		/// <summary>
		/// Build summarization detail query
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="level"></param>
		/// <param name="resultId"></param>
		/// <returns></returns>

		public override Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string linkInfoString)
		{
			Query q;
			QueryTable qt;
			QueryColumn qc;
			AssayLinkInfo assayLi;
			int assayAssayId, assayTypeId;
			string txt;

			List<AssayLinkInfo> liList = ParseLinkInfo(linkInfoString, level);
			if (liList.Count == 0) return null;

			// Drilling down from summarized data

			if (level == 1)
			{
				assayLi = liList[0]; // first or only table
				mt = MetaTableCollection.Get(assayLi.MetaTable.Name);
				if (mt == null) return null;

				q = new Query(); // query to do the drilldown
				qt = new QueryTable(q, mt);
				qt.SelectKeyOnly(); // deselect everything but the key

				q.KeyCriteria = " = " + assayLi.RsltGrpId;

				qt.SelectDefault();

				foreach (MetaColumn mc2 in mt.MetaColumns)
				{ // add all columns associated with this result code that are displayed by default
					//if (mc2.ResultCode == "38") mc2.ResultCode = mc2.ResultCode; // debug

					if (mc2.DataType == MetaColumnType.Image && mc2.InitialSelection != ColumnSelectionEnum.Hidden) // add any CRC images images
						qt.Select(mc2.Name);
					//if ((Lex.Eq(mc2.ResultCode, assayTypeIdString) && mc2.Selection != ColumnSelectionEnum.Hidden)) // this result code but not hidden
					//  qt.Select(mc2.Name);
				}

				qt.Select("strt_dt");
				qt.Select("rslt_ntbk_lbl_txt"); // notebook ref for run
				qt.Select("ntbk_rfrnc_txt"); // notebook ref for lot

				string commentField = mc.Name + "_cmnt_txt"; // add associated comments field if exists
				if (mt.GetMetaColumnByName(commentField) != null)
					qt.Select(commentField);
			}

// Drilling down from unsummarized data to larger image or default row data

			else if (liList.Count == 1)
			{
				assayLi = liList[0]; // first or only table
				mt = MetaTableCollection.Get(assayLi.MetaTable.Name);
				if (mt == null) return null;

				q = new Query(); // query to do the drilldown
				qt = new QueryTable(q, mt);
				qt.SelectAllNonHidden(); // select all nonhidden cols

				qc = qt.GetQueryColumnByName("rslt_grp_id"); // sort by result group id first
				if (qc != null) qc.SortOrder = 1;

				QueryColumn firstSortColumn = null;
				List<QueryColumn> activityCols = new List<QueryColumn>();

				foreach (QueryColumn qc0 in qt.QueryColumns)
				{
					mc = qc0.MetaColumn;
					if (mc.DataType == MetaColumnType.Image) // display image with enough width to hopefully show full size
						qc0.DisplayWidth = 60;

					// If col from detail (basic result) table set first col found to 2nd sort col

					if (mc.SummarizationRole == SummarizationRole.Dependent)
					{
						if (mc.IsSortable && firstSortColumn == null)
						{
							qc0.SortOrder = 2;
							firstSortColumn = qc0;
						}
					}

// If col from activity select & store list of cols to display

					else if (mc.SummarizationRole == SummarizationRole.Derived)
					{
						string prefix = GetPivotedColNamePrefix(mc.Name);

						bool concCol = (Lex.Eq(mc.Name, prefix + "_CMPND_CONC_VAL_NBR"));

						if (Lex.Eq(mc.Name, prefix) || // include basic value
						 concCol || // concentration col
						 Lex.Eq(mc.Name, prefix + "_EXCLD_FLG") || // exclusion flag
						 Lex.Eq(mc.Name, prefix + "_CMNT_TXT")) // comment col - (&& mc.Selection != ColumnSelectionEnum.Hidden))  
						{
							qc0.Selected = true;
							activityCols.Add(qc0);
							if (concCol) qc0.SortOrder = 3; // sort by ascending conc 3rd
						}
					}

				}

				// Move low level data cols to beginning of the table so they are visible on the left

				for (int qci = 0; qci < activityCols.Count; qci++)
				{
					qc = activityCols[qci];
					qt.QueryColumns.Remove(qc);
					qt.QueryColumns.Insert(qci + 1, qc);
				}

				qc = qt.GetQueryColumnByNameWithException("rslt_grp_id");
				qc.Criteria = "rslt_grp_id = " + assayLi.RsltGrpId;
			}

// Drilling down from a set of overlaid multiple result CRC curves to individual curves

			else
			{
				q = new Query(); // query to do the drilldown
				q.LogicType = QueryLogicType.Or; // any rslt_grp_id criteria match is ok

				assayLi = liList[0]; // first table
				qt = new QueryTable(q, assayLi.MetaTable.Root); // root table first
				qt.SelectKeyOnly();

				foreach (AssayLinkInfo li0 in liList) // add other tables
				{
					qt = new QueryTable(q, li0.MetaTable);
					qc = qt.GetQueryColumnByName("rslt_grp_id"); // put criteria on result group
					if (qc != null)
						qc.Criteria = "rslt_grp_id = " + li0.RsltGrpId;
					else { q.RemoveQueryTable(qt); continue; } // just in case

					qt.SelectKeyOnly();
					foreach (QueryColumn qc0 in qt.QueryColumns) // select just the CRC curveimage
					{
						if (qc0.MetaColumn.DataType == MetaColumnType.Image)
							qc0.Selected = true;
					}
				}
			}

			//string mql = MqlUtil.ConvertQueryToMql(q); // debug
			return q;
		}

		/// <summary>
		/// ParseLinkInfo
		/// </summary>
		/// <param name="linkInfoString"></param>
		/// <param name="level"></param>
		/// <returns></returns>

		List<AssayLinkInfo> ParseLinkInfo(
			string linkInfoString,
			int level)
		{
			AssayLinkInfo assayLi;
			MetaTable mt;
			int assayAssayId;
			string txt;

			List<AssayLinkInfo> liList = new List<AssayLinkInfo>();

			if (Lex.Contains(linkInfoString, " overlay ")) // calculated field overlaying multiple CRC curved in a single chart
				txt = Lex.Replace(linkInfoString, " overlay ", "\t");

			else txt = linkInfoString;

			string[] linkStrings = txt.Split('\t');
			foreach (string ls in linkStrings)
			{
				string[] linkInfo = ls.Split(',');
				int li = 0;
				if (Lex.Eq(linkInfo[0], "ASSAY")) li = 1; // offset if database name is first arg

				string assayAssayIdString = linkInfo[li++]; // get ASSAY assay id
				if (!int.TryParse(assayAssayIdString, out assayAssayId)) return null;
				if (!AssayId2ToAssayIdMap.ContainsKey(assayAssayId)) return null;
				string assayId = AssayId2ToAssayIdMap[assayAssayId].ToString();
				string mtName = "ASSAY_" + assayId;

				mt = MetaTableCollection.Get(mtName);
				if (mt == null) continue;

				assayLi = new AssayLinkInfo();
				assayLi.MetaTable = mt;

				assayLi.AssayTypeId = linkInfo[li++]; // ASSAY result type id
				assayLi.RsltGrpId = linkInfo[li++]; // result group for unsummarized, CorpId for summarized
				if (li < linkInfo.Length) assayLi.RsltId = linkInfo[li++]; // result row id

				//if (level == 1)
				//{
				//  // ResultId is of the form assayId, typeId, CorpId. Parse this
				//  // and get metatable/metacolumn since supplied parameters may
				//  // be from an unpivoted table & in which case they are unusable.

				//  assayLi.RsltGrpId = linkInfo[li++];
				//}

				//else
				//{
				//  assayLi.RsltGrpId = linkInfo[li++];
				//  assayLi.DtldRsltId = linkInfo[li++];
				//}

				liList.Add(assayLi);
			}

			return liList;
		}

		/// <summary>
		/// AssayLinkInfo
		/// </summary>

		internal class AssayLinkInfo
		{
			public MetaTable MetaTable;
			public string AssayTypeId = ""; // ASSAY result type id
			public string RsltGrpId = ""; // result group for unsummarized, CorpId for summarized
			public string RsltId = ""; // result row id
		}

		/// <summary>
		/// Return true if column name is the name of a pivoted column
		/// </summary>
		/// <param name="mcName"></param>

		public static bool IsPivotedColName(string mcName)
		{
			if (Lex.StartsWith(mcName, "R_") || Lex.StartsWith(mcName, "S_")) return true;
			else return false;
		}

		/// <summary>
		/// Get prefix for pivoted column from MetaColumn name
		/// </summary>
		/// <param name="rtd"></param>
		/// <returns></returns>

		public static string GetPivotedColNamePrefix(string mcName)
		{
			string prefix;

			if (mcName.Length < 3) return mcName;
			int i1 = mcName.Substring(2).IndexOf("_");
			if (i1 < 0) return mcName;

			prefix = mcName.Substring(0, i1 + 2);
			return prefix;
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
			int assayId;

			const string summarizedAssaySql = @"<sqlGoesHere>";

			const string unsummarizedAssaySqlNew = @"<sqlGoesHere>";

			const string unsummarizedAssaySql = @"<unsummarizedAssaySql>";

			// Code

			int t0 = TimeOfDay.Milliseconds();

			MetaTable mt = qt.MetaTable;
			//if (mt.RemapForRetrieval && mt.SummarizedExists) // get summarized multipivot data (not passed via metatable)
			// mt.UseSummarizedData = true; // Note: this can switch all_assay_pivoted (not summarized) to summarized mode with can cause issues

			if (mt.UseSummarizedData)
				sql = summarizedAssaySql;

			else sql = unsummarizedAssaySql;

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareListReader(sql, DbType.Int32);
			drd.ExecuteListReader(resultKeys);
			if (drd.Cancelled)
			{
				// todo qe.Cancelled = true;
				drd.Dispose();
				return;
			}

			Dictionary<string, object> mtDict = new Dictionary<string, object>();
			int assayIdCount = 0;
			while (true) // convert list of assays to set of metatable names
			{
				if (!drd.ListRead()) break;
				assayId = drd.GetInt(0);
				//if (assayId == 7008) assayId = assayId; // debug (sometimes not in contents)

				string assayName = "ASSAY_" + assayId; // ASSAY table table name

				if (QueryEngine.FilterAllDataQueriesByDatabaseContents &&
					!MetaTableCollection.IsMetaTableInContents(assayName)) continue; // metatable must be in contents

				if (!IsMemberMetaTable(assayName)) continue; // must also be a production ASSAY table

				if (mt.UseSummarizedData) assayName += MetaTable.SummarySuffix;
				mtDict[assayName] = null;
				assayIdCount++;
			}

			drd.Dispose();
			if (drd.Cancelled)
			{
				// todo qe.Cancelled = true;
				return;
			}

			ArrayList mtList = new ArrayList();
			foreach (string mtName2 in mtDict.Keys)
			{ // put metatable labels & names into a list for sorting
				mt2 = MetaTableCollection.Get(mtName2);
				if (mt2 == null) continue;
				if (Lex.Contains(mt2.Label, "Bogus")) continue; // filter out dev/test assays

				mtList.Add(mt2.Label.ToLower().PadRight(64) + "\t" + mt2.Name);
			}

			mtList.Sort();

			foreach (string mts in mtList)
			{ // add new querytables/metatables to query
				string[] sa = mts.Split('\t');
				mt2 = MetaTableCollection.Get(sa[1]);
				if (mt2 == null) continue;

				qt2 = q.GetQueryTableByName(mt2.Name); // see if already in query
				if (qt2 != null) continue; // ignore if already there

				qt2 = new QueryTable(q, mt2);
				if (qt.HeaderBackgroundColor != Color.Empty)
					qt2.HeaderBackgroundColor = qt.HeaderBackgroundColor;
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Return true if table is a production ASSAY metatable
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsMemberMetaTable(string mtName)
		{
			if (AssayMetaFactory == null) return false;
			return (AssayMetaFactory.IsMemberMetaTable(mtName));
		}

		/// <summary>
		/// Return true if a description of the specified table is available
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override bool TableDescriptionIsAvailable(
			MetaTable mt)
		{
			return true;
		}

		/// <summary>
		/// Return description for table.
		/// May be html or simple text
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override TableDescription GetTableDescription(
			MetaTable mt)
		{
			return AssayMetadataDao.GetTableDescription(mt);
		}

		/// <summary>
		/// Get a concentration response curve
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="resultLinks"></param>
		/// <param name="desiredWidth"></param>
		/// <returns></returns>

		public override Bitmap GetImage(
			MetaColumn mc,
			string resultLinks,
			int width)
		{
			int dtldRsltId;
			string dtldRsltIds;
			Bitmap image = null;
			string xAxisLabel = "", yAxisLabel = "";

			DateTime t0 = DateTime.Now;

			if (Lex.IsUndefined(resultLinks)) return null;
			string imageKey = resultLinks + "," + width; // key for image

			image = CheckCache(imageKey);
			if (image != null) return image;

			string[] resultLinkArray = SplitAndExpandResultLinks(resultLinks);

			dtldRsltIds = "";
			for (int ri = 0; ri < resultLinkArray.Length; ri++)
			{
				string rid = resultLinkArray[ri];
				dtldRsltId = GetRsltId(rid);
				if (dtldRsltId <= 0) continue;
				if (!IsApprovedResult(dtldRsltId)) continue; // check for approved result before attempting to render

				if (dtldRsltIds != "") dtldRsltIds += CalcField.OverlayOpPadded;
				dtldRsltIds += dtldRsltId.ToString();
			}

			if (dtldRsltIds == "") return null;

			if (Debug)
				DebugLog.Message("ASSAY GetImage IsApprovedResult time: " + TimeOfDay.Delta(ref t0));

			CrcServiceDepth++;
			//if (CrcServiceDepth > 1) CrcServiceDepth = CrcServiceDepth; // debug

			// Setup and make the call

			AssayCurveService.ServiceUrl = CrcServiceUrl;
			AssayCurveService crcService = new AssayCurveService();

			AssayGetImageParms p = new AssayGetImageParms();
			p.SourceName = "ASSAY";
			p.ResultIdsString = dtldRsltIds;
			p.Width = width;
			p.Height = (int)(width * 1.0);

			if (mc != null && (resultLinkArray.Length > 1 || mc.MetaTable.UseSummarizedData)) // if multiple results may be summarized data multiple results for a single assay or
			{
				if (!mc.MetaTable.UseSummarizedData) // if not summarized then assume multiple assays and show names in legend
					p.ShowAssayNameInLegend = true;
				p.ShowBookpageInLegend = true;
				p.ShowBookpageInXAxis = false;
				p.ShowCorpIdInLegend = false;
				p.ShowRunDateInLegend = true;

				//  SetCustomCurveLabels(metaColumn, ref p.XAxisLabel, ref p.YAxisLabel);
			}

			Bitmap bitmap = crcService.GetImage(p);

			if (Debug)
				DebugLog.Message("GetImage AssayCurveService.GetImage time: " + TimeOfDay.Delta(ref t0));

			CrcServiceDepth--;

			if (bitmap == null) return null;

			if (BitmapCache != null)
				BitmapCache[imageKey] = bitmap; // cache the bitmap

			return bitmap;
		}

		/// <summary>
		/// Split a list of result links
		/// </summary>
		/// <param name="resultLinks"></param>
		/// <returns></returns>

		public static string[] SplitAndExpandResultLinks(string resultLinks)
		{
			if (Lex.Contains(resultLinks, CalcField.OverlayOp)) // overlaid curves
				resultLinks = Lex.Replace(resultLinks, CalcField.OverlayOpPadded, "\t");

			string[] sa = resultLinks.Split('\t');
			List<string> ll = new List<string>();

			foreach (string l in sa)
			{
				if (Lex.Contains(l, "CorpId")) // summarized, expand
				{
					List<string> rl2 = ExpandSummarizedResultLink(l);
					ll.AddRange(rl2);
				}

				else ll.Add(l);
			}

			sa = ll.ToArray();
			return sa;
		}

		/// <summary>
		/// Expand a summarized result of the form: (assayid, resultTypeId, CorpId, smrzdRsltId) into multiple underlying detailed results
		/// </summary>
		/// <param name="smrzdResultLink"></param>
		/// <returns></returns>

		static List<string> ExpandSummarizedResultLink(
			string smrzdResultLink,
			bool getLinksWithWellLevelDataOnly = true)
		{
			int assayId, resultTypeId, groupId, smrzdRsltId, dtldRsltId;
			bool summarized;
			List<string> erl = new List<string>();

			string sql = @"<sqlGoesHere>";

			if (getLinksWithWellLevelDataOnly) // check that associated well-level data exists 
				sql += @"<sqlGoesHere>";

			SplitResultLink(smrzdResultLink, out assayId, out resultTypeId, out groupId, out smrzdRsltId, out summarized);
			if (smrzdRsltId < 0) return erl;

			DbCommandMx cmd = new DbCommandMx();
			cmd.PrepareParameterized(sql, DbType.Int32);
			cmd.ExecuteReader(smrzdRsltId);
			while (cmd.Read())
			{
				dtldRsltId = cmd.GetInt(0);
				erl.Add(assayId.ToString() + "," + resultTypeId + "," + groupId + "," + dtldRsltId);
			}
			cmd.Dispose();

			return erl;
		}

		/// <summary>
		/// Parse an image reference string and return the result id
		/// </summary>
		/// <param name="resultLink"></param>
		/// <returns></returns>

		int GetRsltId(string resultLink)
		{
			int assayId, resultTypeId, groupId, rsltId;
			bool summarized;

			if (SplitResultLink(resultLink, out assayId, out resultTypeId, out groupId, out rsltId, out summarized))
				return rsltId;
			else return -1;
		}

		/// <summary>
		/// Split a link to a result into its constituent parts
		/// </summary>
		/// <param name="resultLink"></param>
		/// <param name="assayId"></param>
		/// <param name="resultTypeId"></param>
		/// <param name="groupId"></param>
		/// <param name="rsltId"></param>
		/// <returns></returns>

		static bool SplitResultLink(
			string resultLink,
			out int assayId,
			out int resultTypeId,
			out int groupId,
			out int rsltId,
			out bool summarized)
		{
			assayId = resultTypeId = groupId = rsltId = -1;
			summarized = false;

			string[] sa = resultLink.Split(','); // assayid, resultTypeId, groupId, dtldRsltId
			if (sa.Length != 4) return false;

			int errorCnt = 0;

			if (!int.TryParse(sa[0], out assayId)) errorCnt++;
			if (!int.TryParse(sa[1], out resultTypeId)) errorCnt++;

			string gId = sa[2];
			if (Lex.StartsWith(gId, "CorpId")) // summarized if starts with "CorpId"
			{
				summarized = true;
				gId = gId.Substring(3);
			}
			if (!int.TryParse(gId, out groupId)) errorCnt++;

			if (!int.TryParse(sa[3], out rsltId)) errorCnt++;

			if (errorCnt == 0) return true;
			else return false;
		}

		/// <summary>
		/// Set custom curve labels
		/// Currently only sets X-axis concentration label for multiple overlaid curves from calculated fields
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="xAxisLabel"></param>
		/// <param name="yAxisLabel"></param>

		public void SetCustomCurveLabels(
			MetaColumn mc,
			ref string xAxisLabel,
			ref string yAxisLabel)
		{
			if (mc.MetaTable.MetaBrokerType != MetaBrokerType.CalcField) return; // must be calc field table that overlays curves
			if (Lex.Ne(mc.Name, "CALC_FIELD")) return; // must be calc_field result col

			foreach (MetaColumn mc0 in mc.MetaTable.MetaColumns) // look for source column that contains concentration units
			{
				if (!Lex.StartsWith(mc0.TableMap, "ASSAY_")) continue; // find input col

				string[] sa = mc0.TableMap.Split('.');
				if (sa.Length < 1) continue;
				string mtname = sa[0];
				MetaTable sourceMt = MetaTableCollection.Get(mtname);
				if (sourceMt == null) continue;

				foreach (MetaColumn smc in sourceMt.MetaColumns)
				{
					if (smc.SummarizationRole != SummarizationRole.Derived) continue; // well-level column
					int i1 = Lex.IndexOf(smc.Label, "conc");
					if (i1 < 0) continue;
					xAxisLabel = smc.Label.Substring(i1);
					break;
				}

				break;
			}

			return;
		}

		/// <summary>
		/// Check cache for image
		/// </summary>
		/// <param name="bitmapKey"></param>
		/// <returns></returns>

		Bitmap CheckCache(string bitmapKey)
		{
			if (!CacheBitmaps) return null;

			if (BitmapCache == null) BitmapCache = new Dictionary<string, Bitmap>();
			if (BitmapCache.ContainsKey(bitmapKey))
			{
				//DebugLog.Message("Returning cached bitmap: " + resultId);
				return BitmapCache[bitmapKey];
			}

			else return null;
		}

		/// <summary>
		/// Return true if result is approved
		/// </summary>
		/// <param name="dtldRsltId"></param>
		/// <returns></returns>

		bool IsApprovedResult(int dtldRsltId)
		{
			string sql = @"
select dtld_rslt_id
from assay_owner.assay_dtld_rslt
where aprvl_sts_cd = 'Y' and dtld_rslt_id = :0";

			DbCommandMx cmd = new DbCommandMx();
			cmd.PrepareParameterized(sql, DbType.Int32);
			cmd.ExecuteReader(dtldRsltId);
			bool approved = Read(cmd);
			cmd.Dispose();
			return approved;
		}

		/// <summary>
		/// See if source is available
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public override Schema CheckDataSourceAccessibility(
		MetaTable mt)
		{
			return GenericMetaBroker.CheckDataSourceAccessibility("assay_owner");
		}

		/// <summary>
		/// Collection of PivotGroups for broker
		/// </summary>

		internal class PivotGroupCollection
		{
			internal Dictionary<string, PivotGroup> GroupDict; // dictionary of groups
			internal PivotGroup BaseGroup; // base inner table group that other groups are joined to 
			internal AssayMetaBroker Broker; // associated broker

			internal bool Unsummarized { get { return Broker.Qt.MetaTable.UseUnsummarizedData; } }
			internal bool Summarized { get { return Broker.Qt.MetaTable.UseSummarizedData; } }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="broker"></param>

			internal PivotGroupCollection(AssayMetaBroker broker)
			{
				Broker = broker;
				GroupDict = new Dictionary<string, PivotGroup>();

				return;
			}

			/// <summary>
			/// Select and initialize the base group
			/// The base group is the group that the other groups are joined against.
			/// It is either a group for a result type that has criteria that must match or
			/// for a result type that always has a stored value or a synthesized key 
			/// that always has a value that can be joined against.
			/// </summary>

			internal void InitializeBaseGroup()
			{
				string baseGroupName;

				QueryColumn g0Qc = null;

				if (Broker.FirstPivotedColWithCriteria != null) // if pivoted col with criteria use that
					g0Qc = Broker.FirstPivotedColWithCriteria;

				else if (Broker.PivotedCodes.Count == 1) // if only one pivoted code then use that as base
					g0Qc = Broker.PivotedColumns[0];

				else
				{
					// potential optimization: scan columns looking for one that always has data
				}

				if (g0Qc != null) // define group used to establish row identity
				{
					baseGroupName = GetGroupName(g0Qc.MetaColumn.ResultCode);
					BaseGroup = AddPivotGroup(baseGroupName);
				}

				else if (Broker.Mt.Code != "") // must synthesize one row per rslt_grp_id and call this G_0 (e.g. criteria on run date or other non-result table column)
				{
					baseGroupName = "G_0"; // G_0 indicates that the t0 group is determined by a special piece of sql that generates one row per result group
					BaseGroup = AddPivotGroup(baseGroupName);

					BaseGroup.Exprs = // get unique row identifiers
						"unique " + BaseGroup.Exprs;
				}

				else // no code for table, must be unpivoted view
				{
					baseGroupName = "G_0"; // base is G_0 and is the only group 
					BaseGroup = AddPivotGroup(baseGroupName);
				}

				return;
			}

			/// <summary>
			/// Add QueryColumn to an innner table group
			/// </summary>
			/// <param name="qc"></param>

			internal PivotGroup AddQcToPivotGroup(
				QueryColumn qc)
			{
				PivotGroup pivGrp;

				string name = GetGroupName(qc);
				if (!GroupDict.ContainsKey(name))
					pivGrp = AddPivotGroup(name);
				else pivGrp = GroupDict[name];

				if (Lex.IsDefined(qc.Criteria) && qc.Query != null && qc.Query.LogicType == QueryLogicType.And)
					pivGrp.QcWithRequiredCriteriaExists = true; // required criteria, may be able to convert some outer joins for group to inner joins

				pivGrp.QcList.Add(qc);

				return pivGrp;
			}

			internal PivotGroup AddPivotGroup(string name)
			{
				if (Unsummarized) return AddUnsummarizedPivotGroup(name);
				else return AddSummarizedGroup(name);
			}

			/// <summary>
			/// Add a new unsummarized innertable group
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>

			internal PivotGroup AddUnsummarizedPivotGroup(string name)
			{
				//if (!Lex.StartsWith(name, "G_")) name = name; // debug

				PivotGroup pivGrp = new PivotGroup(this);
				pivGrp.GroupName = name;

				pivGrp.AddPivotGroupExpr("r.run_id", "", AssaySqlElements.RsltSubqueryName); // used to join between PivotGroups (appears that this must be first, why?)
				pivGrp.AddPivotGroupExpr("r.rslt_grp_id", "", AssaySqlElements.RsltSubqueryName);
				pivGrp.AddPivotGroupExpr("r.rslt_table_nm", "", AssaySqlElements.RsltSubqueryName); // include table name pseudo-column to separate activity and detail results
				pivGrp.AddPivotGroupExpr("r.assay_id", "", AssaySqlElements.RsltSubqueryName);

				GroupDict[name] = pivGrp;
				return pivGrp;
			}

			/// <summary>
			/// Add a new summarized innertable group
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>

			internal PivotGroup AddSummarizedGroup(string name)
			{
				//if (!Lex.StartsWith(name, "G_")) name = name; // debug

				PivotGroup pivGrp = new PivotGroup(this);
				pivGrp.GroupName = name;

				pivGrp.AddPivotGroupExpr("r.<cid>", "", AssaySqlElements.RsltSubqueryName); // be sure we have the cid
				pivGrp.AddPivotGroupExpr("r.conc_nbr", "", AssaySqlElements.RsltSubqueryName); // may need to join on conc in some cases
				pivGrp.AddPivotGroupExpr("r.assay_id", "", AssaySqlElements.RsltSubqueryName);

				GroupDict[name] = pivGrp;
				return pivGrp;
			}

			/// <summary>
			/// Get the internal table name for the query column
			/// </summary>
			/// <param name="qc"></param>
			/// <returns></returns>

			internal string GetGroupName(
				QueryColumn qc)
			{
				MetaColumn mc = qc.MetaColumn;
				string name = "";

				if (IsPivotedColumn(qc.MetaColumn))
					name = GetGroupName(mc.ResultCode);

				else name = BaseGroup.GroupName; // others from base group

				return name;
			}

			/// <summary>
			/// Build group name from result code(s)
			/// </summary>
			/// <param name="resultCode"></param>
			/// <returns></returns>

			internal string GetGroupName(string resultCode)
			{
				string groupName = "G_" + resultCode;
				if (groupName.Contains(","))
					groupName = groupName.Replace(",", "_");

				return groupName;
			}

			/// <summary>
			/// Complete the the sql for each column group, build the sql to join the groups and
			/// build the full "FromClause" sql for the broker
			/// </summary>
			/// <param name="broker"></param>
			/// <returns></returns>

			internal string BuildFullBrokerFromClauseSql(
				AssayMetaBroker broker)
			{
				PivotGroup pivGrp;
				string sql;

				pivGrp = BaseGroup; // build base group first
				pivGrp.BuildGroupSql();

				foreach (PivotGroup pivGrp0 in GroupDict.Values)
				{
					pivGrp = pivGrp0;
					if (pivGrp == BaseGroup) continue; // skip if already included base

					pivGrp.BuildGroupSql();
				}

				sql = // build full inner sql contained within parens
					"select " + broker.InnerExprs + " " +
					"from " + broker.InnerTables + " ";

				if (broker.InnerCriteria != "")
					sql += "where " + broker.InnerCriteria;

				// Prepend "with" clause to sql to define ASSAY_RSLT as selecting from unsummarized or summarized tables


				if (Unsummarized)
				{
					string withClause = "";

					int detailCnt = Broker.DetailedResultPivotedColsFetched; // should be lower-case "b"?
					int activityCnt = Broker.ActivityResultPivotedColsFetched;
					bool unpivotedunpivotedWellLevelView = (broker.Mt != null && Lex.Eq(broker.Mt.Name, "ASSAY_UNPIVOTED_WELL_LEVEL"));

					withClause = // start of with clause that contains the CID, Well material subquery
						"/* begin-with */ with " + AssaySqlElements.CidWmSubqueryName + " as (" + AssaySqlElements.CidWmSubquerySql + "), " +
						AssaySqlElements.RsltSubqueryName + " as ("; // plus result subquery

					if ((detailCnt > 0 || activityCnt == 0) && // detail only or (detail and activity)
						!unpivotedunpivotedWellLevelView)
					{
						withClause +=  AssaySqlElements.DetailTableSubquerySql;

						if (activityCnt > 0)
							withClause += " union all " + AssaySqlElements.ActivityTableSubQuerySql;
					}

					else withClause += AssaySqlElements.ActivityTableSubQuerySql; // just include activity table

					withClause += " ) /* end-with */ "; // close out the result subquery and with clause
					sql = withClause + "\r\n" + sql;
				}

				else // summarized
				{
					string withClause = "/* begin-with */ with " + AssaySqlElements.RsltSubqueryName + " as (" + AssaySqlElements.SmrzdTableSql + ") /* end-with */ ";
					sql = withClause + "\r\n" + sql;
				}

				SubqueryId++; 
				sql = Lex.Replace(sql, AssaySqlElements.SubqueryNameSuffix, AssaySqlElements.SubqueryNameSuffix + "_" + SubqueryId); // make subquery names unique
				
				sql = "(" + sql + ")"; // enclose in parens

				return sql;
			}
		} // class PivotGroupCollection

		/// <summary>
		/// Info on set of QueryColumns associated with a specific result type or the run or lot
		/// including the mappping of those QCs to the associated underlying Oracle tables.
		/// </summary>

		internal class PivotGroup
		{
			internal string GroupName; // group name, R_0 for result identity group, R_rsltId for pivoted col or RUN or LOT
			internal List<QueryColumn> QcList = new List<QueryColumn>(); // associated query columns
			internal bool QcWithRequiredCriteriaExists = false; // if true then a querycolumn with required criteria exists (e.g. AND logic query)
			internal string Exprs = ""; // selected columns/expressions
			internal string Tables = ""; // referenced tables
			internal string Criteria = ""; // criteria binding tables within the group together
			internal string AssayResultTypeIds = ""; // list of allowed result types

			internal HashSet<string> TableNamesWithSuffix = new HashSet<string>(); // table names with qualifying suffix that have already been added
			internal HashSet<string> ExprsColnames = new HashSet<string>(); // column names in expressions

			internal PivotGroupCollection GroupCollection;
			internal AssayMetaBroker Broker { get { return GroupCollection != null ? GroupCollection.Broker : null; } } // owning broker

			internal bool Unsummarized { get { return Broker.Qt.MetaTable.UseUnsummarizedData; } }
			internal bool Summarized { get { return Broker.Qt.MetaTable.UseSummarizedData; } }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="c"></param>

			internal PivotGroup(PivotGroupCollection c)
			{
				GroupCollection = c;
				return;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="expr">Group expression</param>
			/// <param name="brokerColName">Optional broker column name to assign to</param>
			/// <param name="tableName">Group table name</param>

			internal void AddPivotGroupExpr(
				string expr,
				string brokerColName,
				string tableName)
			{

				//if (Lex.Contains(expr, "well_mtrl") || Lex.Contains(tableName, "well_mtrl")) tableName = tableName; // debug
				AddGroupExpr(expr, brokerColName);
				AddTableToPivotGroupAsNeeded(tableName);
				return;
			}

			/// <summary>
			/// Add an expression to the group
			/// </summary>
			/// <param name="expr">Group expression</param>
			/// <param name="brokerColName">Optional broker column name to assign to</param>

			internal void AddGroupExpr(
				string expr,
				string brokerColName)
			{
				int i1;

				// Add to group expressions

				if (GroupName == "G_0") GroupName = GroupName; // debug

				string e2 = expr;
				string colName = null;

				if (Lex.Eq(brokerColName, expr))
					colName = expr.ToUpper();

				else if (Lex.IsNullOrEmpty(brokerColName)) // get the simple unqualified col name from expr
				{
					i1 = expr.LastIndexOf("."); // qualified?
					if (i1 >= 0)
						colName = expr.Substring(i1 + 1).ToUpper();

					else colName = expr.ToUpper(); // not qualified
				}

				else
				{
					colName = brokerColName.ToUpper();
					e2 += " " + brokerColName;
				}

				if (!ExprsColnames.Contains(colName)) // check for duplicate group col name
				{
					Lex.AppendItemToStringIfNew(ref Exprs, e2, ", ");
					ExprsColnames.Add(colName);
				}

				// Add just the col name to Broker.InnerExprs

				e2 = GroupName + "." + brokerColName; // column name qualified by group name
				if (!Lex.IsNullOrEmpty(brokerColName))
					Lex.AppendItemToStringIfNew(ref Broker.InnerExprs, e2, ", ");

				return;
			}

			/// <summary>
			/// Assure that the specified table any dependencies are included in the InnerTableGroup tables and join criteria
			/// </summary>
			/// <param name="itg"></param>
			/// <param name="qc"></param>

			internal void AddTableToPivotGroupAsNeeded(
				string tableName)
			{
				//if (Lex.Contains(tableName, "well_mtrl")) tableName = tableName; // debug

				//if (Broker.Qt.MetaTable.UseSummarizedData && Lex.Eq(tableName, AssayOracleElements.Result))
				//  tableName = AssayOracleElements.Summary;// using summarized data

				PivotGroup pivGrp = this;

				tableName = tableName.ToUpper();
				if (pivGrp.TableNamesWithSuffix.Contains(tableName)) return; // return if table already included
				pivGrp.TableNamesWithSuffix.Add(tableName); // add name in original form with any suffix

				OracleTableRelations itr = OracleTableRelations.Get(tableName, Summarized);

				if (!Lex.IsNullOrEmpty(itr.DependsOn) && Lex.Ne(itr.DependsOn, "G_0")) // add any dependencies if other than base relation
					AddTableToPivotGroupAsNeeded(itr.DependsOn);

				Lex.AppendSeparatorToStringIfNotBlank(ref pivGrp.Tables, ", ");
				int l = tableName.Length - 1; // index of last char
				if (Char.IsDigit(tableName[l]))  // remove if digit
					tableName = tableName.Substring(0, l);

				pivGrp.Tables += tableName + " " + itr.Alias;

				if (!Lex.IsNullOrEmpty(itr.JoinCriteria) && Lex.Ne(itr.DependsOn, "G_0")) // add any join conditions if other than base relation
				{
					Lex.AppendSeparatorToStringIfNotBlank(ref pivGrp.Criteria, " and ");
					pivGrp.Criteria += itr.JoinCriteria;
				}

				return;
			}

			/// <summary>
			/// Merge the sql for the group into the overall sql and join to base group
			/// </summary>
			/// <param name="brokerExprs">Selected values that are passed out from the broker sql</param>
			/// <param name="brokerFrom">The combined sql for the groups</param>
			/// <param name="brokerCriteria">Join criteria combining groups</param>
			/// <param name="broker"></param>
			/// <returns></returns>

			internal void BuildGroupSql()
			{
				string baseName = "", sql = "";

				// Add join criteria at the broker inner criteria level

				PivotGroup pivGrp = GroupCollection.BaseGroup;
				if (pivGrp != null && this != pivGrp) // base group must be defined and if we are the base don't join
				{
					baseName = pivGrp.GroupName;
					Lex.AppendSeparatorToStringIfNotBlank(ref Broker.InnerCriteria, " and ");

					string bic = Broker.InnerCriteria;

					if (Unsummarized)
					{
						bic += GroupName + ".rslt_grp_id (+) = " + baseName + ".rslt_grp_id and " + // outer join (todo: if value is part of "AND" criteria do regular join)
							GroupName + ".rslt_table_nm (+) = " + baseName + ".rslt_table_nm "; // match on table name to separate activity results from detailed results
					}

					else
					{
						bic +=
							GroupName + ".<cid> (+) = " + baseName + ".<cid> " + // outer join (todo: if value is part of "AND" criteria do regular join)
							"and " + GroupName + ".assay_id (+) = " + baseName + ".assay_id ";

						if (Broker.JoinOnConcentration) bic += // need to join on conc also?
							"and " + GroupName + ".conc_nbr (+) = " + baseName + ".conc_nbr_id ";
					}

					Broker.InnerCriteria = bic;
				}

				// Complete the sql for the group and add it to the overall broker From Sql

				AddGroupAssayIdsSql();
				AddGroupRdwResultTypeIdsSql();

				//bool debug = true;
				//if (debug)
				//{
				//  Lex.AppendIfNotBlank(ref Criteria, " and "); // limit for debug
				//  Criteria += "rg.rslt_grp_id = 1810058";
				//}

				sql = "(select " + Exprs + " " +
					"from " + Tables + " ";
				if (!Lex.IsNullOrEmpty(Criteria)) sql += "where " + Criteria;
				sql += ") " + GroupName;

				Lex.AppendSeparatorToStringIfNotBlank(ref Broker.InnerTables, ", ");
				Broker.InnerTables += sql;

				return;
			}

			/// <summary>
			/// Set the ASSAY id(s) for the group
			/// </summary>
			/// <param name="AssayIds"></param>

			internal void AddGroupAssayIdsSql()
			{
				string assayIds = Broker.GetAssayIdList();
				if (Lex.IsNullOrEmpty(assayIds)) return;

				Lex.AppendSeparatorToStringIfNotBlank(ref Criteria, " and ");

				string assayIdCol = "r.assay_id";
				if (Lex.IsDefined(assayIds))
				{
					if (!assayIds.Contains(","))
						Criteria += assayIdCol + " = " + assayIds;
					else Criteria += assayIdCol + " in (" + assayIds + ")";
				}

				else Criteria += assayIdCol + " is null"; // no matching ids (may be query on data that doesn't exist)
			}

			/// <summary>
			/// Set the AssayMetaData Assay result types for the group
			/// </summary>

			internal void AddGroupRdwResultTypeIdsSql()
			{
				if (Lex.IsUndefined(AssayResultTypeIds)) return;

				Lex.AppendSeparatorToStringIfNotBlank(ref Criteria, " and ");

				if (!AssayResultTypeIds.Contains(","))
					Criteria += "r.rslt_typ_id = " + AssayResultTypeIds;
				else Criteria += "r.rslt_typ_id in (" + AssayResultTypeIds + ")";

				//else Criteria += "r.rslt_typ_id is null"; // no matching ids (may be query on data that doesn't exist)
			}

		} // InnerTableColumnGroup

		/// <summary>
		/// Relationships and dependencies for ASSAY tables which provide information on joining tables between lower and higher levels
		/// </summary>

		internal class OracleTableRelations
		{
			internal string TableName = ""; // owner-qualified lower-level table name
			internal string Alias = ""; // alias for lower-level table
			internal string DependsOn = ""; // higher-level table that this links to (if R_0 then this is an intergroup dependency rather than a intragroup dependency)
			internal string JoinCriteria = ""; // criteria for join lower-level table to higher-level table

			internal OracleTableRelations SummarizedRelations = null; // summarized relations that differ from unsummarized relations

			internal static Dictionary<string, OracleTableRelations> Relations; // keyed on low-level table name

			internal static OracleTableRelations Get(
				string name,
				bool summarized)
			{
				if (Relations == null) Setup();
				name = name.ToUpper();
				if (!Relations.ContainsKey(name)) throw new InvalidDataException("Relations name not found: " + name);

				OracleTableRelations r = Relations[name];
				if (!summarized || r.SummarizedRelations == null) return r;
				else return r.SummarizedRelations;
			}

			/// <summary>
			/// Setup the table relations
			/// </summary>

			internal static void Setup()
			{
				Relations = new Dictionary<string, OracleTableRelations>();

				AddRelation(AssaySqlElements.RsltSubqueryName, "r", "G_0", "r.<cid> = G_0.corp_id and r.assay_id = G_0.assay_id"); // integrated unsummarized result view

				AddRelation(AssaySqlElements.Run, "rn", AssaySqlElements.RsltSubqueryName, "rn.run_id = r.run_id"); // run (links to base group)

				AddRelation(AssaySqlElements.SubmissionIdLotIdCidId, "ss", AssaySqlElements.RsltSubqueryName, "ss.sbmsn_id = r.sbmsn_id and ss.active_flg = 'A'"); // link active rows to ASSAY SBMSN_ID. Only use outer join if RegSad is not being properly replicated)
				AddRelation(AssaySqlElements.Assay, "a", AssaySqlElements.RsltSubqueryName, "a.assay_id = r.assay_id");

				AddRelation(AssaySqlElements.ResultType, "rt", AssaySqlElements.RsltSubqueryName, "rt.rslt_typ_id = r.rslt_typ_id");
				AddRelation("assay_owner.assay_site", "site", AssaySqlElements.Run, "site.site_id = rn.site_id");
				AddRelation("assay_owner.assay_lab", "lab", AssaySqlElements.Run, "lab.lab_id = rn.lab_id");
				AddRelation("person_table_owner.dit_person", "prsn", AssaySqlElements.Run, "prsn.standard_logon_id = rn.assyr_id");
				AddRelation("person_table_owner.dit_person2", "prsn2", AssaySqlElements.Run, "prsn2.standard_logon_id = rn.pblshr_id");

				AddRelation("assay_owner.assay_pblctn_trnsctn_run", "ptr", AssaySqlElements.Run, "ptr.pblctn_trnsctn_run_id = rn.pblctn_trnsctn_run_id");
				AddRelation("assay_owner.assay_pblctn_trnsctn", "pt", "assay_owner.assay_pblctn_trnsctn_run", "pt.pblctn_trnsctn_id = ptr.pblctn_trnsctn_id");
				AddRelation("assay_owner.assay_oprtnl_systm", "os", "assay_owner.assay_pblctn_trnsctn", "os.oprtnl_systm_id = pt.oprtnl_systm_id");

				AddRelation(AssaySqlElements.CaaResultTypeAttrTable, "caa", AssaySqlElements.RsltSubqueryName, AssaySqlElements.CaaResultTypeAttrJoinCriteria); // link common result type attrs to assays
				AddRelation(AssaySqlElements.CaaAssayAttrTable, "caa2", AssaySqlElements.RsltSubqueryName, AssaySqlElements.CaaAssayAttrJoinCriteria); // link common assay attrs to assays
				AddRelation(AssaySqlElements.CaaGeneAttrTable, "caa3", AssaySqlElements.RsltSubqueryName, AssaySqlElements.CaaGeneAttrJoinCriteria); // link common gene attrs to assays
			}

			/// <summary>
			/// Add a join relation
			/// </summary>
			/// <param name="name">Name of the relation</param>
			/// <param name="alias"></param>
			/// <param name="dependsOn"></param>
			/// <param name="joinCriteria"></param>
			/// 

			static void AddRelation(
				string name,
				string alias,
				string dependsOn,
				string joinCriteria)
			{
				AddRelation(name, alias, dependsOn, joinCriteria, "", "");
				return;
			}

			/// <summary>
			/// Add a join relation
			/// </summary>
			/// <param name="name"></param>
			/// <param name="alias"></param>
			/// <param name="dependsOn"></param>
			/// <param name="joinCriteria"></param>
			/// <param name="dependsOnSmrzd"></param>
			/// <param name="joinCriteriaSmrzd"></param>

			static void AddRelation(
				string name,
				string alias,
				string dependsOn,
				string joinCriteria,
				string dependsOnSmrzd,
				string joinCriteriaSmrzd)
			{
				OracleTableRelations r = new OracleTableRelations();
				name = name.ToUpper();
				r.TableName = name;
				r.Alias = alias;

				r.DependsOn = dependsOn;
				r.JoinCriteria = joinCriteria;

				if (Lex.IsDefined(dependsOnSmrzd)) // save any summarized relationship
				{
					OracleTableRelations r2 = new OracleTableRelations();
					r2.TableName = r.TableName;
					r2.Alias = r.Alias;
					r2.DependsOn = dependsOnSmrzd;
					r2.JoinCriteria = joinCriteriaSmrzd;
					r.SummarizedRelations = r2;
				}

				Relations[name] = r;
				return;
			}

		} // TableRelations

	}

}
