using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Metabroker for retrieving and integrating assay data across multiple assay databases (e.g. In-house proprietary, ChEmbl etc) summarized by target/gene symbol
	/// </summary>

	public partial class MultiDbAssayMetaBroker : GenericMetaBroker
	{
		bool Summarizing = false;
		UnpivotedAssayResult LastSummaryRow = null;
		bool Pivoting = false;
		QueryEngine SubQe; // query engine instance fo getting lower level data
		object[] NextSubQeVo; // buffered vo from subquery engine
		UnpivotedAssayResult NextSubQeRow; // bufferered row from subquery engine
		List<UnpivotedAssayResult> BufferedRows; // contains raw assay results used by SummarizeByTarget
		bool MarkedBounds = false;
		string CurrentCid;
		Dictionary<string, int> CidPosDict = new Dictionary<string, int>(); // position of each compoundid

    UnpivotedAssayResultFieldPositionMap MdbAssayVoMap; // map col name to vo index
    MultiDbAssayMetaBroker SecondaryMetaBroker; // secondary broker for subquery
    ExecuteQueryParms Eqp2; // secondary Eqp for subquery
    static Dictionary<string, List<object[]>> TargetPivotedResults; // pivoted results summarized by target keyed by cid (static since built in DoPreRetrievalTransformation which uses a different broker instance)
		static int TargetResultsKeyPos; // position within set of keys
		static int TargetResultsPosWithinKey; // position in target results for current key

		Dictionary<string, int> FilterableTargetsResultPivotMap; // map for optional pivoted values for TargetUnpivoted view
		Dictionary<string, int> FilterableTargetsBinPivotMap; // map for optional pivoted values for TargetUnpivoted view
		List<int> FilterableTargetsVoiList; // list of vo positions that contain filterable target info

		TargetSummaryOptions Tso; // view options
		TargetMap TargetMap = null; // any target map to display data on

		//double MaxCRC = NullValue.NullNumber; // limits for data to include
		//double MinSP = NullValue.NullNumber;

		public static AssayDict TaaDict; // target assay attributes for all assays
		public static string Databases = "All"; // list of databases to search

/// <summary>
/// Return true if table name is a summarized multidatabase assay table
/// </summary>
/// <param name="mtName"></param>
/// <returns></returns>

		public bool IsSummarizedMdbAssayTable(string mtName)
		{
			return UnpivotedAssayResult.IsSummarizedMdbAssayTable(mtName);
		}

		/// <summary>
		/// Return true if pivoted table
		/// </summary>

		public bool IsPivotedMdbAssayTableName
		{
			get
			{
				return UnpivotedAssayResult.IsPivotedMdbAssayTableName(MtName);
			}
		}

		/// <summary>
		/// Underlying metatable name
		/// </summary>

		public string MtName
		{
			get
			{
				if (Qt == null) return "";
				else return Qt.MetaTable.Name;
			}
		}

		// Constructor

		public MultiDbAssayMetaBroker()
		{
			AssayDict tad = TargetAssayDict; // be sure we have target assay assoc info
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
			return DataSchemaMx.GetDataSourceForSchemaName("<schemaName>")?.DataSourceName;
		}

		/// <summary>
		/// Prepare query
		/// </summary>
		/// <param name="parms"></param>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			List<MultiDbAssayMetaBroker> mbList;
			MultiTablePivotBrokerTypeData mpd; // multipivot data for this broker type
			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi;
			QueryColumn qc;
			MetaTable mt;

			Eqp = eqp;
			Qt = eqp.QueryTable;
			mt = eqp.QueryTable.MetaTable;
			BuildActivityBinCondFormat();
			MdbAssayVoMap = UnpivotedAssayResultFieldPositionMap.NewMdbAssayMap(Qt);
			MdbAssayVoMap.InitializeForQueryTable(Qt);
			LoadTargetMap();

			if ( // check for basic conditions that disallow multipivoting
				!eqp.ReturnQNsInFullDetail || // no multipivot if part of calc field
				eqp.Qe == null || // need to be able to access queryengine info
				//!QueryEngine.AllowMultiTablePivot || // is multipivot even allowed
				!UnpivotedAssayResult.IsSummarizedMdbAssayTable(mt.Name)) // summarized tables only
					return base.PrepareQuery(eqp);

			int pivotedColCount = 0;
			foreach (QueryColumn qc1 in Qt.QueryColumns)
			{ // if any non-key criteria then pivot individually rather than via multipivot
				if (qc1.Criteria != "" && !qc1.IsKey)
				{
					if (UnpivotedAssayResult.IsUnpivotedSummarizedMdbAssayTable(mt.Name) && Qt.Query.SingleStepExecution)
					{ }  // special case: allow criteria on unpivoted summary table which also apply to associated pivoted tables
					else return base.PrepareQuery(eqp);
				}
				if (IsPivotedColumn(qc1.MetaColumn)) pivotedColCount++;
			}

			if (pivotedColCount == 0) return base.PrepareQuery(eqp); // must have at least one column to pivot

			// Store pivot info for queryTable

			PivotInCode = true;
			Sql = BuildSql(eqp);

			if (eqp.Qe.MetaBrokerStateInfo == null)
				eqp.Qe.MetaBrokerStateInfo = new Dictionary<string, MultiTablePivotBrokerTypeData>();

			mbsi = eqp.Qe.MetaBrokerStateInfo;

			MpGroupKey = MetaBrokerType.TargetAssay.ToString(); // key for broker for query

			if (!QueryEngine.AllowMultiTablePivot)
				MpGroupKey += "_" + Qt.MetaTable.Name;

			mpd = MultiTablePivotBrokerTypeData.GetMultiPivotData(eqp.Qe.MetaBrokerStateInfo, MpGroupKey, mt.Name);

			string geneSymbol = mt.Code;
			if (!Lex.IsNullOrEmpty(geneSymbol) && !mpd.TableCodeDict.ContainsKey(geneSymbol))
			{
				mpd.TableCodeDict[geneSymbol] = new MpdResultTypeData(); // add key to hash list
				if (mpd.TableCodeCsvList.Length > 0) mpd.TableCodeCsvList.Append(",");

				mpd.TableCodeCsvList.Append(Lex.AddSingleQuotes(geneSymbol));
			}

			mpd.AddMetaBroker(mt.Name, this);

			return Sql;
		}

/// <summary>
/// Build condformatting for activity bins
/// </summary>

		void BuildActivityBinCondFormat()
		{
			QueryColumn qc;

			qc = Qt.GetQueryColumnByName(MultiDbAssayDataNames.ActivityBin); // assign default cond formatting for activity bin
			if (qc != null && qc.Selected && qc.CondFormat == null)
				qc.CondFormat = UnpivotedAssayResult.BuildActivityBinCondFormat();

			qc = Qt.GetQueryColumnByName(MultiDbAssayDataNames.ActivityBinMostPotent);
			if (qc != null && qc.Selected && qc.CondFormat == null)
				qc.CondFormat = UnpivotedAssayResult.BuildActivityBinCondFormat();

			return;
		}

/// <summary>
/// Load any target map info
/// </summary>

		void LoadTargetMap()
		{
			try
			{
				Tso = TargetSummaryOptions.GetFromMdbAssayOptionsColumn(Qt);
				if (Tso == null || Lex.IsNullOrEmpty(Tso.TargetMapName)) return;

				TargetMap = TargetMapDao.GetMapByLabel(Tso.TargetMapName); // get the basics
				TargetMap = TargetMapDao.GetMapWithCoords(TargetMap.Name); // get the coords
				return;
			}
			catch (Exception ex) { return; } // just return if "none" or no longer exists
		}

        /// <summary>
        /// Return true if specified column is a pivoted column
        /// </summary>
        /// <param name="mc"></param>
        /// <returns></returns>

        bool IsPivotedColumn(
            MetaColumn mc)
        {
            if (mc.IsKey) return false; // just key not pivoted for now
            else return true;
        }

/// <summary>
/// Generate SQL
/// </summary>
/// <param name="eqp"></param>
/// <returns></returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			Eqp = eqp;
			Qt = eqp.QueryTable;
			MetaTable mt = Qt.MetaTable;
			KeyMc = mt.KeyMetaColumn;

			string sql = base.BuildSql(eqp);

			sql = FormatActivityBinExpression(sql);

			return sql;
		}

/// <summary>
/// Format SQL expression for activity bin from simple meta-expression (e.g. CalcActivityBin(PREFIX, AVERAGE_VAL))
/// </summary>
/// <param name="sql"></param>
/// <returns></returns>

	string  FormatActivityBinExpression(
		string sql)
	{
		// Model template for binning % (SP) and uM (CRC) data

		string template = @" 
	   (case
       when uom = '%' then (case
			  when prefix = '<' then 0
        when average_val<70 then 1 
        when average_val<90 then 4 
        when average_val>=90 then 5  
        else 0
        end)
       when uom = 'uM' then (case
			  when prefix = '>' then 0
        when average_val>10 then 1
        when average_val>5 then 4 
        when average_val>1 then 5 
        when average_val>0.5 then 7 
        when average_val>0.1 then 8 
        when average_val>0.01 then 9 
        when average_val<=0.01 then 10 
        else 0
        end)
       else 0
       end)";

		while (true)
		{
			int i1 = Lex.IndexOf(sql, "CalcActivityBin(");
			if (i1 < 0) break;
			string sql2 = sql.Substring(i1);
			int i2 = sql2.IndexOf('(');
			int i3 = sql2.IndexOf(')');
			if (i3 < 0) break;

			string source = sql.Substring(i1, i3 + 1); // string to replace
			string cols = source.Substring(i2 + 1, i3 - i2 - 1);
			string[] sa = cols.Split(',');
			string prefixColName = sa[0].Trim(); // name of prefix column
			string valColName = sa[1].Trim(); // name of value column

			string expr = Lex.Replace(template, "prefix", prefixColName);
			expr = Lex.Replace(expr, "average_val", valColName);
			sql = Lex.Replace(sql, source, expr);
		}

		return sql;
	}

		/// <summary>
		/// Execute query in preparation for retrieving rows
		/// </summary>
		/// <param name="parms"></param>

	public override void ExecuteQuery(
		ExecuteQueryParms eqp)
	{
		BufferedRows = new List<UnpivotedAssayResult>();

		MultiTablePivotBrokerTypeData mpd = null;
		MetaTable mt;
		List<MultiDbAssayMetaBroker> mbList = null;
		MultiDbAssayMetaBroker mb, mb2 = null;
		UnpivotedAssayResult rr, rr2;
		List<UnpivotedAssayResult> rrList = new List<UnpivotedAssayResult>(); // list of results summarized by target & result type
		bool unpivotedTableIsFirst;
		object[] vo;
		string mtName = null;

		int t0 = TimeOfDay.Milliseconds();

		mt = eqp.QueryTable.MetaTable;

		Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = eqp.Qe.MetaBrokerStateInfo;
		if (PivotInCode)
		{
			mpd = mbsi[MpGroupKey];

			if (PivotInCode && mpd.MbInstances.Count == 1 && UnpivotedAssayResult.IsUnpivotedSummarizedMdbAssayTable(mt.Name))  
		    PivotInCode = false; // don't multipivot if single unpivoted summary table
		}

		if (!PivotInCode) // if not multipivot then call generic broker 
		{
			base.ExecuteQuery(eqp);
			return;
		}

		if (mpd.FirstTableName != Qt.MetaTable.Name) return; // retrieve data for all tables when we see first table

		SetupSecondaryMetaBroker(eqp, mpd, out unpivotedTableIsFirst);
		mb2 = SecondaryMetaBroker;

		mpd.ClearBuffers();

		// Retrieve data & store for associated metabrokers

		if (mb2 == this) base.ExecuteQuery(eqp); // execute with the base generic broker
		else mb2.ExecuteQuery(Eqp2); // use the secondary broker that was created

		AssayDict dict = new AssayDict();

		rr = new UnpivotedAssayResult();
		int readCnt = 0;
		bool includeResultDetail = MdbAssayVoMap.ResultDetailId.Voi >= 0;
		while (true)
		{
			if (mb2 == this) vo = base.NextRow(); // get data via generic broker
			else vo = mb2.NextRow(); // get data with secondary broker
			if (vo == null) break;

			rr.FromValueObject(vo, mb2.MdbAssayVoMap); // parse values into a UnpivotedAssayResult
			int rowsFetched = 0, vosCreated = 0;

			for (int pup = 0; pup < 2; pup++) // first pass for unpivoted table, 2nd for pivoted by gene
			{
				//try
				//{
				if (pup == 0)
				{
					if (!unpivotedTableIsFirst) continue; // if no unpivoted first table skip this
					mtName = mpd.FirstTableName; // unpivoted table should be first
				}

				else // pivoted table
				{
					if (Lex.IsNullOrEmpty(rr.GeneSymbol)) continue; // skip if no target symbol
					if (Lex.Contains(mpd.FirstTableName, "CORP")) // mapped to pivoted corp only table
						mtName = MultiDbAssayDataNames.BasePivotTablePrefix + rr.GeneSymbol.ToUpper(); // name of table mapped to
					else // combined tables
						mtName = MultiDbAssayDataNames.CombinedPivotTablePrefix + rr.GeneSymbol.ToUpper();
				}
				//}
				//catch (Exception ex) { ex = ex; }
				mt = MetaTableCollection.Get(mtName);
				if (mt == null) continue;
				if (!mpd.MbInstances.ContainsKey(mt.Name)) continue; // have row hash for broker?

				int mbIdx = 0;
				if (mpd.MbInstances[mtName] is MultiDbAssayMetaBroker)
					mb = (MultiDbAssayMetaBroker)mpd.MbInstances[mtName]; // broker assoc w/table
				else
				{
					mbList = (List<MultiDbAssayMetaBroker>)mpd.MbInstances[mtName];
					mb = (MultiDbAssayMetaBroker)mbList[0];
				}

				while (true) // copy out for each metabroker for metatable
				{
					UnpivotedAssayResultFieldPositionMap voMap = mb.MdbAssayVoMap;
					vo = rr.ToValueObject(mb.Qt.SelectedCount, voMap);

					if (mb.MultipivotRowList == null)
						mb.MultipivotRowList = new List<object[]>();
					mb.MultipivotRowList.Add(vo);

					if (mbList == null) break; // single broker
					mbIdx++; // go to next broker
					if (mbIdx >= mbList.Count) break; // at end of brokers?
					mb = (MultiDbAssayMetaBroker)mbList[mbIdx];
				}
			} // tables to copy data to loop
		} // row fetch loop

		return;
	}

/// <summary>
///  Setup/configure any needed secondary metabroker
/// </summary>
/// <param name="eqp"></param>
/// <param name="mpd"></param>
/// <param name="unpivotedTableIsFirst"></param>

        void SetupSecondaryMetaBroker(
            ExecuteQueryParms eqp,
            MultiTablePivotBrokerTypeData mpd,
            out bool unpivotedTableIsFirst)
        {
            QueryEngine qe2;
            Query q2;
            QueryTable qt2;
            QueryColumn qc2;
            MetaTable mt2;

            string firstMtName = mpd.FirstTableName;
            MetaTable firstMt = MetaTableCollection.GetWithException(firstMtName);
            unpivotedTableIsFirst = UnpivotedAssayResult.IsUnpivotedSummarizedMdbAssayTable(firstMtName);

            if (unpivotedTableIsFirst) // just use unpivoted table as is
            {
                SecondaryMetaBroker = this;
                Eqp2 = eqp;
            }

            else // all pivoted tables, create secondary query on the summarized unpivoted table
            {
                if (SecondaryMetaBroker == null)
                {
                    MultiDbAssayMetaBroker mb2 = SecondaryMetaBroker = new MultiDbAssayMetaBroker();

                    if (UnpivotedAssayResult.IsCombinedMdbAssayTable(firstMtName))
                        mt2 = MetaTableCollection.GetWithException(MultiDbAssayDataNames.CombinedTableName);
                    else mt2 = MetaTableCollection.GetWithException(MultiDbAssayDataNames.BaseTableName);

                    q2 = new Query();
                    qt2 = new QueryTable(mt2);

                    qc2 = qt2.GetQueryColumnByNameWithException(MultiDbAssayDataNames.GeneSymbol);
                    if (mpd.TableCodeCsvList.Length != 0) // limit by codes
                        qc2.Criteria = qc2.CriteriaDisplay = MultiDbAssayDataNames.GeneSymbol + " in (" + mpd.TableCodeCsvList + ")";

                    q2.AddQueryTable(qt2);

                    qe2 = new QueryEngine();
                    qe2.Query = q2;
                    Eqp2 = new ExecuteQueryParms(qe2, qt2);
                    mb2.PrepareQuery(Eqp2);

                    mb2.Sql += " where " + qc2.Criteria;
                }

                Eqp2.SearchKeySubset = eqp.SearchKeySubset;
            }

            return;
        }

		/// <summary>
		/// Parse Qualified number from database format
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public override QualifiedNumber ParseQualifiedNumber(string txt)
		{
			if (txt == null) return null;

			QualifiedNumber qn = new QualifiedNumber();
			string[] args = txt.Split('\v'); // elements are separated by vertical tab chars

			if (args == null || args.Length == 0) return null;

			if (args.Length >= 4) // average val
			{
				if (args.Length > 0) qn.Qualifier = args[0];
				if (qn.Qualifier == "=") qn.Qualifier = "";
				if (args.Length > 1 && args[1] != "") qn.NumberValue = Double.Parse(args[1]);

				if (args.Length > 2 && args[2] != "") qn.NValue = Int32.Parse(args[2]);
				if (args.Length > 3 && args[3] != "") qn.StandardDeviation = Double.Parse(args[3]);
				if (args.Length > 4 && args[4] != "") qn.DbLink = args[4];
			}

			else // most potent
			{
				if (args.Length > 0) qn.Qualifier = args[0]; // not currently in db
				if (qn.Qualifier == "=") qn.Qualifier = "";
				if (args.Length > 1 && args[1] != "") qn.NumberValue = Double.Parse(args[1]);
				if (args.Length > 2 && args[2] != "") qn.DbLink = args[2];
			}

			return qn;
		}

		/// <summary>
		/// Retrieve next result row
		/// </summary>
		/// <returns></returns>

		public override object[] NextRow()
		{
			object[] vo;
			UnpivotedAssayResult rr;

			if (!PivotInCode) // if not multipivot then call generic broker 
				vo = base.NextRow();

			else if (MultipivotRowList == null || MultipivotRowList.Count == 0) vo = null;

			else // remove row from multipivot row list
			{
				vo = MultipivotRowList[0]; // note: iterating MultipivotRowDict is slow
				MultipivotRowList.RemoveAt(0);
			}

			if (vo != null)
			{
				rr = UnpivotedAssayResult.FromValueObjectNew(vo, MdbAssayVoMap); // set cond formatting
				rr.SetResultValueBackColor(rr.ResultValue, rr.ActivityBin);
				rr.SetResultValueBackColor(rr.MostPotentVal, rr.ActivityBinMostPotent);

				SetCoordinates(rr);
				int voLength = Qt.SelectedCount;
				//if (vo.Length == voLength)
				rr.ToValueObject(vo, MdbAssayVoMap); // if same length use existing vo
			}

			return vo;
		}

/// <summary>
/// Set coords based on gene symbol & current target map
/// </summary>
/// <param name="rr"></param>

		void SetCoordinates(UnpivotedAssayResult rr)
		{
			UnpivotedAssayResult rr2;
			int tci;
			if (TargetMap == null || !TargetMap.Coords.ContainsKey(rr.GeneId)) return;

			//if (rr.TargetSymbol != "CHRM5") rr = rr; // debug

			List<TargetMapCoords> coordList = TargetMap.Coords[rr.GeneId];
			for (tci = 0; tci < coordList.Count; tci++)
			{
				if (tci == 0) rr2 = rr;
				else rr2 = rr.CloneUnpivotedAssayResult();

				TargetMapCoords tmc = coordList[tci];
				rr2.TargetMapX = tmc.X;
				rr2.TargetMapY = -tmc.Y; // use negative value for y
				if (tci > 0) BufferedRows.Add(rr2); // add to buffer
			}
		}

/// <summary>
/// NextTargetUnpivotedCombinedRow
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public UnpivotedAssayResult NextTargetUnpivotedCombinedRow(
			Query q)
		{
			UnpivotedAssayResult rr = new UnpivotedAssayResult();
			return rr;
		}

		/// <summary>
		/// Close broker
		/// </summary>

		public override void Close()
		{
			bool prefetched = true;

			if (prefetched) { } // already closed

			else base.Close(); // close normal query

			return;
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
			QueryColumn qc2;
			HashSet<string> geneDict = new HashSet<string>();
			string sql;
			string geneSymbol;

			int t0 = TimeOfDay.Milliseconds();

// Build query & get set of gene symbols

			Query q2 = new Query();
			q2.SingleStepExecution = true;

			q2.KeyCriteria = q2.KeyCriteriaDisplay =  // keylist to return (may want to include all target summary option criteria)
				" in (" + MqlUtil.FormatValueListString(resultKeys, true) + ")";

			qt2 = qt.Clone();
			qt2.SelectKeyOnly();
			qc2 = qt2.GetQueryColumnByNameWithException("gene_symbol");
			qc2.Selected = true;
			q2.AddQueryTable(qt2);

			QueryEngine qe2 = new QueryEngine();
			qe2.ExecuteQuery(q2);
			while (true)
			{
				object[] vo = qe2.NextRow();
				if (vo == null)
				{
					qe2.Close();
					break;
				}

				if (qe2.Cancelled)
				{
					qe2.Close();
					return;
				}

				geneSymbol = vo[2] as string;
				if (Lex.IsNullOrEmpty(geneSymbol)) continue;
				geneDict.Add(geneSymbol.ToUpper());
			}

			string[] sa = new string[geneDict.Count];
			geneDict.CopyTo(sa);
			Array.Sort(sa);

			foreach (string s0 in sa)
			{
				geneSymbol = s0;
				string mt2Name = MultiDbAssayDataNames.BasePivotTablePrefix + geneSymbol;

				//if (QueryEngine.FilterAllDataQueriesByDatabaseContents &&
				//    !MetaTableCollection.IsMetaTableInContents(mtName)) continue; // metatable must be in contents

				mt2 = MetaTableCollection.Get(mt2Name);
				if (mt2 == null) continue; // in case can't allocate for some reason
				qt2 = q.GetQueryTableByName(mt2.Name); // see if already in query
				if (qt2 != null) continue; // ignore if already there

				qt2 = new QueryTable(q, mt2); // allocate new query table & add to query
				qt2.DeselectAll();

				foreach (QueryColumn qc0 in qt.QueryColumns) // set selected columns to match those in the source qt
				{
					if (qc0.Selected && qt2.GetQueryColumnByName(qc0.MetaColumn.Name) != null)
						qt2.GetQueryColumnByName(qc0.MetaColumn.Name).Selected = true;
				}

				if (qt.HeaderBackgroundColor != Color.Empty)
					qt2.HeaderBackgroundColor = qt.HeaderBackgroundColor;
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Build summarization detail query
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>
		/// <param name="level"></param>
		/// <param name="linkInfo"></param>
		/// <returns></returns>

		public override Query GetDrilldownDetailQuery(
			MetaTable mt,
			MetaColumn mc,
			int level,
			string linkInfo)
		{
			QueryTable qt, qt2;
			MetaTable mt2;
			QueryColumn qc, qc2;
			MetaColumn mc2;
			string resultIdColName = "";

			Query q = new Query();

			// Summarized MdbAssay table drilling down to unsummarized MdbAssay table

			if (UnpivotedAssayResult.IsSummarizedMdbAssayTable(mt.Name))
			{
				if (UnpivotedAssayResult.IsCombinedMdbAssayTable(mt.Name)) // multiple tables
				{
					mt2 = MetaTableCollection.GetWithException(MultiDbAssayDataNames.CombinedNonSumTableName);
					resultIdColName = MultiDbAssayDataNames.SumResultId;
				}

				else // base unpivoted or pivoted by target
				{
					mt2 = MetaTableCollection.GetWithException(MultiDbAssayDataNames.BaseNonSumTableName);
					resultIdColName = MultiDbAssayDataNames.BaseSumResultId; 
				}

				qt = new QueryTable(q, mt2);
				qc = qt.GetQueryColumnByNameWithException(resultIdColName);
				qc.Criteria = resultIdColName + " = " + linkInfo;
				return q; 
			}

// Drilling down from old all_bioassay_unpivoted to specific source data

			else if (Lex.Eq(mt.Name, UnpivotedAssayView.UnsummarizedMetaTableName)) 
			{ // ResultId is formatted by the source with the first item being the source name
				Match m = Regex.Match(linkInfo, @"(.*),(.*),(.*)", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
				string mtName = m.Groups[1].Value;
				string mcId = m.Groups[2].Value;
				string linkInfo2 = m.Groups[3].Value;
				mt2 = MetaTableCollection.GetWithException(mtName);
				IMetaBroker imb = MetaBrokerUtil.GlobalBrokers[(int)mt2.MetaBrokerType];
				q = imb.GetDrilldownDetailQuery(mt2, mcId, level, linkInfo2);
				return q;
			}

			else throw new ArgumentException("Invalid tableName: " + mt.Name);
		}

		/// <summary>
		/// Build a query to fetch the assays for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayListQuery(
			string geneSymbol)
		{
			Query q = BuildTargetAssayQuery(MultiDbAssayDataNames.Assay2EGTableName , geneSymbol);
			return q;
		}

		/// <summary>
		/// Build query to fetch all unsummarized assay data for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayUnsummarizedDataQuery(
			string geneSymbol)
		{
			Query q = BuildTargetAssayQuery(MultiDbAssayDataNames.BaseNonSumTableName, geneSymbol);
			q.SingleStepExecution = true; // for performance
			q.KeySortOrder = 0; // no sorting
			return q;
		}

		/// <summary>
		/// Build query to fetch all summarized (by target) assay data for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssaySummarizedDataQuery(
			string geneSymbol)
		{
			Query q = BuildTargetAssayQuery( MultiDbAssayDataNames.BaseTableName, geneSymbol);
			q.SingleStepExecution = true; // for performance
			q.KeySortOrder = 0;
			return q;
		}

		/// <summary>
		/// General table / gene symbol query builder
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayQuery(
			string tableName,
			string geneSymbol)
		{
			Query q = new Query();
			MetaTable mt = MetaTableCollection.GetWithException(tableName);

			QueryTable qt = new QueryTable(q, mt);

			QueryColumn qc = qt.GetQueryColumnByNameWithException(MultiDbAssayDataNames.GeneSymbol);
			qc.Criteria = MultiDbAssayDataNames.GeneSymbol + " = " + Lex.AddSingleQuotes(geneSymbol);
			qc.CriteriaDisplay = "= " + geneSymbol;

			if (qc.IsKey) // if key column store for that as well
			{
				q.KeyCriteria = " = " + Lex.AddSingleQuotes(geneSymbol);
				q.KeyCriteriaDisplay = "= " + geneSymbol;
			}

			//qc = qt.GetQueryColumnByNameWithException("top_lvl_rslt");
			//qc.Criteria = "top_lvl_rslt = 'Y'"; // only top level results
			//qc.CriteriaDisplay = "= Y";

			return q;
		}

		/// <summary>
		/// Get the target assay dictionary, reading in if necessary
		/// </summary>

		public static AssayDict TargetAssayDict
		{
			get
			{
				if (AssayDict.Instance == null) try
					{
						string fileName = TargetMapDao.TargetMapDir + @"\AssayAttributes.bin";
						AssayDict.Instance = AssayDict.DeserializeFromFile(fileName);
					}

					catch (Exception ex) // if exception log error & continue with empty dictionary
					{
						DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
						AssayDict.Instance = new AssayDict();
					}

				return AssayDict.Instance;
			}
		}

		/// <summary>
		/// Show NCBI Entre Gene web page for supplied geneId and/or symbol
		/// </summary>
		/// <param name="geneIdSymbol"></param>

		public static string GetTargetDescriptionUrl(
			string targetIdSymbol)
		{
			int targetId;
			string targetSymbol;

			if (String.IsNullOrEmpty(targetIdSymbol))
				return null;

			else if (Lex.IsInteger(targetIdSymbol)) // numeric gene id
				targetId = int.Parse(targetIdSymbol);

			else if (targetIdSymbol.Contains("-")) // 1234-Symbol form
			{
				string[] sa = targetIdSymbol.Split('-');
				targetSymbol = sa[1].ToUpper();
				try { targetId = TargetSymbolToId(targetSymbol); }
				catch { return null; }
			}

			else // must be symbol
			{
				targetSymbol = targetIdSymbol.ToUpper();
				try { targetId = TargetSymbolToId(targetSymbol); }
				catch { return null; }
			}

			if (targetId <= 0) return null;

			string url = ServicesIniFile.Read("NcbiGeneIdUrlTemplate");
			if (String.IsNullOrEmpty(url)) return null;
			url = url.Replace("<GENE_ID>", targetId.ToString());
			return url;
		}

		/// <summary>
		/// Retrieve target Entrez gene id from symbol
		/// </summary>
		/// <param name="targetSymbol"></param>
		/// <returns></returns>

		public static int TargetSymbolToId(
			string targetSymbol)
		{
			string sql = 
				@"select entrezgene_id
				from <table>
				where upper(gene_symbol) = :0";

			DbCommandMx d = new DbCommandMx();
			d.PrepareParameterized(sql, DbType.String);
			d.ExecuteReader(targetSymbol.ToUpper());
			if (!d.Read())
			{
				d.Dispose();
				throw new Exception("Target Symbol not found: " + targetSymbol);
			}

			int targetId = d.GetInt(0);
			d.Dispose();
			return targetId;
		}

	}

}
