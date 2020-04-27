using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Retrieve and summarize assay data by target
	/// </summary>

	public class TargetAssayMetaBroker : GenericMetaBroker
	{
		bool Summarizing = false;
		UnpivotedAssayResult LastSummaryRow = null;
		bool Pivoting = false;
		QueryEngine SubQe; // query engine instance fo getting lower level data
		object[] NextSubQeVo; // buffered vo from subquery engine
		UnpivotedAssayResult NextSubQeRow; // bufferered row from subquery engine
		List<UnpivotedAssayResult> BufferedRows; // contains raw assay results used by SummarizeByTarget
		bool MarkedBounds;
		string CurrentCid;
		Dictionary<string, int> CidPosDict = new Dictionary<string, int>(); // position of each compoundid

		UnpivotedAssayResultFieldPositionMap UnpivotedResultsVoMap; // map col name to vo index for basic unpivoted assay data
		static Dictionary<string, List<object[]>> TargetPivotedResults; // pivoted results summarized by target keyed by cid (static since built in DoPreRetrievalTransformation which uses a different broker instance)
		static int TargetResultsKeyPos; // position within set of keys
		static int TargetResultsPosWithinKey; // position in target results for current key

		Dictionary<string, int> FilterableTargetsResultPivotMap; // map for optional pivoted values for TargetUnpivoted view
		Dictionary<string, int> FilterableTargetsBinPivotMap; // map for optional pivoted values for TargetUnpivoted view
		List<int> FilterableTargetsVoiList; // list of vo positions that contain filterable target info

		TargetSummaryOptions Tso = new TargetSummaryOptions(); // parameters to use for target summarization

		double MaxCRC = NullValue.NullNumber; // limits for data to include
		double MinSP = NullValue.NullNumber;
		TargetMap TargetMap = null; // any target map to display data on
		string TargetFamily = null;

		//public static AssayDict TaaDict; // target assay attributes for all assays
		public static string Databases = "All"; // list of databases to search

		public static string AUTableName { get { return UnpivotedAssayResult.AllUnpivotedAssayDataTableName; } } // assay unpivoted table name
		public static string TUTableName { get { return UnpivotedAssayResult.AllUnpivotedTargetSummaryTableName; } } // target unpivoted table name

		public static bool AllowRdwData = true;
		public static bool AllowStarData = true;

		// Constructor

		public TargetAssayMetaBroker()
		{
			//AssayDict tad = AssayDict; // be sure we have target assay assoc info
			return;
		}

		/// <summary>
		/// Prepare query
		/// </summary>
		/// <param name="parms"></param>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			QueryColumn qc;
			int nextVoi = -1;

			Eqp = eqp;
			Qt = eqp.QueryTable;
			Sql = BuildSql(eqp);

			CurrentCid = "";

			if (AssayUnpivoted || TargetUnpivoted)
			{ // build map for fast indexing of unpivoted values by col name
				UnpivotedResultsVoMap = new UnpivotedAssayResultFieldPositionMap();
				nextVoi = UnpivotedResultsVoMap.InitializeForQueryTable(Qt);
			}

			if (TargetPivoted || TargetFamilyPivoted) { } // already have data for these

			else if (TargetUnpivoted)
				PrepareTargetUnpivoted(); // just prepare for filterable targets here

			else if (AssayUnpivoted)
			{ // assay data, set ordering needed by summarization
				int orderPos = 1;

				qc = Qt.KeyQueryColumn;
				//qc.SortOrder = orderPos++;

				//qc = Qt.GetQueryColumnByName("gene_fmly");
				//if (qc != null && (qc.Selected_or_Sorted))
				//  qc.SortOrder = orderPos++;

				//qc = Qt.GetQueryColumnByName("gene_symbl");
				//if (qc != null && (qc.Selected_or_Sorted))
				//  qc.SortOrder = orderPos++;

				//qc = Qt.GetQueryColumnByName("rslt_typ"); // CRC, SP
				//if (qc != null && (qc.Selected_or_Sorted))
				//  qc.SortOrder = orderPos++;

				//qc = Qt.GetQueryColumnByName("assy_typ"); // binding, functional...
				//if (qc != null && (qc.Selected_or_Sorted))
				//  qc.SortOrder = orderPos++;

				//qc = Qt.GetQueryColumnByName("rslt_val_nbr"); // put highest SP value first
				//if (qc != null && (qc.Selected_or_Sorted)) // if multiple concentrations use conc of highest SP value
				//  qc.SortOrder = orderPos++;

				if (Eqp.Qe.Query.KeySortOrder != 0)
				{ // assay data, set ordering needed by summarization
					OrderBy = "corp_srl_nbr"; // 

					qc = Qt.GetQueryColumnByName("gene_fmly");
					if (qc != null && (qc.Is_Selected_or_GroupBy_or_Sorted))
						OrderBy += ", gene_fmly";

					qc = Qt.GetQueryColumnByName("gene_symbl");
					if (qc != null && (qc.Is_Selected_or_GroupBy_or_Sorted))
						OrderBy += ", gene_symbl";

					qc = Qt.GetQueryColumnByName("rslt_typ"); // CRC, SP
					if (qc != null && (qc.Is_Selected_or_GroupBy_or_Sorted))
						OrderBy += ", rslt_typ";

					qc = Qt.GetQueryColumnByName("assy_typ"); // binding, functional...
					if (qc != null && (qc.Is_Selected_or_GroupBy_or_Sorted))
						OrderBy += ", assy_typ";

					qc = Qt.GetQueryColumnByName("rslt_val_nbr"); // put highest SP value first
					if (qc != null && (qc.Is_Selected_or_GroupBy_or_Sorted)) // if multiple concentrations use conc of highest SP value
						OrderBy += ", rslt_val_nbr desc";
				}

				qc = Qt.GetQueryColumnByName("activity_bin"); // assign default cond formatting for activity bin
				if (qc != null && qc.Selected && qc.CondFormat == null)
					qc.CondFormat = UnpivotedAssayResult.BuildActivityBinCondFormat();

				//if (Lex.Contains(Sql, "/*+ hint */")) // use first_rows hint for faster retrieval (no, this is actually much slower now)
				//  Sql = Lex.Replace(Sql, "/*+ hint */", "/*+ first_rows */");

				KeyListPredType = KeyListPredTypeEnum.Literal; // don't parameterize keys for this query
			}

			else throw new Exception("Unexpected query table");

			return Sql;
		}

		/// <summary>
		/// Prepare a combination of the target unpivoted table and a set of associated pivoted views
		/// </summary>
		/// <param name="nextVoi"></param>

		void PrepareTargetUnpivoted()
		{
			List<TargetAssayMetaBroker> mbList;
			MultiTablePivotBrokerTypeData mpd; // multipivot data for this broker type
			ExecuteQueryParms eqp = Eqp;
			MetaTable mt = eqp.QueryTable.MetaTable;
			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi;

			PivotInCode = true;
			BuildSql(eqp); // setup SelectList (don't really need sql)

			MpGroupKey = MetaBrokerType.TargetAssay.ToString(); // key for broker for query

			if (!QueryEngine.AllowMultiTablePivot)
				MpGroupKey += "_" + Qt.MetaTable.Name;

			if (eqp.Qe.MetaBrokerStateInfo == null)
				eqp.Qe.MetaBrokerStateInfo = new Dictionary<string, MultiTablePivotBrokerTypeData>();

			mbsi = eqp.Qe.MetaBrokerStateInfo;

			mpd = MultiTablePivotBrokerTypeData.GetMultiPivotData(eqp.Qe.MetaBrokerStateInfo, MpGroupKey, mt.Name);
			mpd.AddMetaBroker(mt.Name, this);

			return;
		}

		/// <summary>
		/// Setup the FilterableTargets columns for the TargetUnpivoted view (
		/// </summary>
		/// <param name="voi">Last used vo position</param>

		void PrepareTargetUnpivotedForPVColumns(
			int nextVoi)
		{
			List<string> filterableTargetsList;
			QueryColumn qc;
			int pv;

			Tso = GetSummarizationParameterValues(Qt);
			////TargetAssayUtil.SetupTargetUnpivotedFilterableTargets(Qt, Tso); // select proper columns

			FilterableTargetsResultPivotMap = new Dictionary<string, int>();
			FilterableTargetsBinPivotMap = new Dictionary<string, int>();
			FilterableTargetsVoiList = new List<int>();

			if (!Lex.IsNullOrEmpty(Tso.FilterableTargets)) // use specified filterable targets
			{
				filterableTargetsList = Csv.SplitCsvString(Tso.FilterableTargets);
			}

			else // if filterable targets not defined use the initial list of gene criteria targets
			{
				qc = Qt.GetQueryColumnByName("gene_symbl");

				string criteria = qc.Criteria;
				if (Lex.IsNullOrEmpty(criteria)) return;
				ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
				if (psc.ValueList == null) return;
				filterableTargetsList = psc.ValueList;
			}

			pv = 1;
			bool valSelected = Qt.GetQueryColumnByName("rslt_val").Selected;
			bool binSelected = Qt.GetQueryColumnByName("activity_bin").Selected;

			foreach (string target in filterableTargetsList)
			{
				if (Lex.IsNullOrEmpty(target)) continue;

				if (valSelected)
				{
					pv++;
					qc = Qt.GetQueryColumnByName("PV" + pv);
					if (qc == null) break; // break if at end
					nextVoi++;
					FilterableTargetsResultPivotMap[target.ToUpper()] = nextVoi;
					FilterableTargetsVoiList.Add(nextVoi);
				}

				if (binSelected)
				{
					pv++;
					qc = Qt.GetQueryColumnByName("PV" + pv);
					if (qc == null) break; // break if at end
					nextVoi++;
					FilterableTargetsBinPivotMap[target.ToUpper()] = nextVoi;
					FilterableTargetsVoiList.Add(nextVoi);
				}
			}
		}

		/// <summary>
		/// Generate the sql for retrieval of all_bioassay_unpivoted or all_bioassay_unpivoted
		/// </summary>
		/// <param name="eqp"></param>
		/// <returns></returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			List<MetaColumn> mcl;
			List<QueryColumn> qcl;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			Hashtable resultKeys;
			string sql, innerExpr, tMap, tAlias, resultCode, txt, tok;
			string pivotTable = "", pivotTables = "", pivotExprs = "", pivotCriteria;
			int ci, qci, i1;

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

			string assayAttrsTable = AssayAttributesDao.AssayAttrsTableName;
			string assayAttrsAlias = "caa";

			Exprs = "";
			string innerExprs = "";
			string innerCriteria = "";
			string innerTables = assayAttrsTable + " " + assayAttrsAlias;

			SelectList = new List<MetaColumn>(); // list of selected metacolumns gets built here

			// Process query column objects

			for (qci = 0; qci < qcl.Count; qci++)
			{
				qc = qcl[qci];
				if (!qc.Is_Selected_or_Criteria_or_GroupBy_or_Sorted) continue;

				mc = qc.MetaColumn;

				if (qc.Is_Selected_or_GroupBy_or_Sorted) // if selected add to expression list
				{
					if (Exprs.Length > 0) Exprs += ",";
					Exprs += mc.Name;
					SelectList.Add(mc);
				}

				if (Lex.Eq(mc.TableMap, "Result")) continue; // handle result cols separately

				tMap = mc.TableMap.ToLower();
				if (tMap == assayAttrsTable) tAlias = assayAttrsAlias;
				else if (tMap == "") tAlias = ""; // must be calculated field
				else throw new Exception("Unrecognized table map: " + tMap);

				if (tAlias != "")
				{
					innerExpr = GenericMetaBroker.GetColumnSelectionExpression(mc, tAlias);

					if (Lex.Eq(mc.Name, "assy_nm")) // hack to add space to end of assay name to indicate description is available
					{ // for assay name return link to any description
						string assyNm = tAlias + ".assy_nm";
						string assyDesc = tAlias + ".assy_desc";
						string assyDb = tAlias + ".assy_db";
						string assyId = tAlias + ".assy_id_txt";
						innerExpr = // add link info consisting of database and assay id if description exists
						 assyNm + " || decode(" + assyDesc + ", 'Y', chr(11) || " + assyDb + " || ',' || " + assyId + ", '') assy_nm, " +
						 assyNm + " assy_nm_val";
					}

					else if (Lex.Eq(mc.Name, "assy_mt_nm")) // build metatable name
					{
						string mtNm = tAlias + ".assy_mt_nm";
						string assyDb = tAlias + ".assy_db";
						string assyId = tAlias + ".assy_id_txt";
						innerExpr = // convert RDW to ASSAY & append assay id
						 " decode(" + assyDb + ", 'RDW', 'ASSAY'," + assyDb + ") || '_' || " + assyId + " assy_mt_nm";
					}

				}

				else innerExpr = "null " + mc.Name; // calculated value, retrieve as null initially

				if (innerExprs.Length > 0) innerExprs += ", ";
				innerExprs += innerExpr;
			}

			// Build Sql to union results from various sources

			qc = Qt.GetQueryColumnByName("assy_db"); // any criteria on database
			if (qc != null && qc.Criteria != "")
				Databases = qc.Criteria;

			FromClause = ""; // result accumulated here

			// Finish putting Sql together

			FromClause = "(" + FromClause + ")";
			if (Qt.Alias != "") FromClause += " " + Qt.Alias;

			sql =
				"select /*+ hint */ " + Exprs +
				" from " + FromClause;

			if (eqp.CallerSuppliedCriteria != "")
				sql += " where " + eqp.CallerSuppliedCriteria;

			//string criteria = ""; // debug
			//foreach (QueryColumn qc0 in Qt.QueryColumns)
			//{
			//  if (qc0.Criteria != "")
			//    criteria += qc0.MetaColumn.Name + " " + qc0.Criteria + "\r\n";
			//}

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

			if (TargetPivoted || TargetFamilyPivoted)  // have precalculated results already
			{
				Eqp = eqp;
				TargetResultsKeyPos = 0; // position in keys
				TargetResultsPosWithinKey = -1; // result position for key
			}

			else if (TargetUnpivoted) // open subquery on current key subset
				ExecuteTargetUnpivoted(eqp);

			else base.ExecuteQuery(eqp); // select data directly

			MarkedBounds = false;

			return;
		}

		/// <summary>
		/// Execute the target pivoted query
		/// </summary>
		/// <param name="eqp"></param>

		void ExecuteTargetUnpivoted(
			ExecuteQueryParms eqp)
		{
			DbCommandMx drd;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			List<TargetAssayMetaBroker> mbList = null;
			TargetAssayMetaBroker mb;
			UnpivotedAssayResult rr, rr2;
			List<UnpivotedAssayResult> rrList = new List<UnpivotedAssayResult>(); // list of results summarized by target & result type
			object[] vo;
			string mtName = null;

			int t0 = TimeOfDay.Milliseconds();

			Dictionary<string, MultiTablePivotBrokerTypeData> mbsi = eqp.Qe.MetaBrokerStateInfo;
			MultiTablePivotBrokerTypeData mpd = mbsi[MpGroupKey];
			if (mpd.FirstTableName != Qt.MetaTable.Name) return; // retrieve data for all tables when we see first table

			if (!Lex.Eq(Qt.MetaTable.Name, UnpivotedAssayResult.AllUnpivotedTargetSummaryTableName))
				throw new Exception(TUTableName + " table must be included in query before associated pivoted views");

			GetSummarizationParameterValues(eqp.QueryTable);
			SubQe = PrepareAndOpenAssayDataSubquery(eqp.Qe.Query, Qt, eqp.SearchKeySubset);

			mpd.ClearBuffers();

			// Summarize data & store for associated metabrokers

			int rowsFetched = 0, vosCreated = 0;
			bool includeResultDetail = UnpivotedResultsVoMap.ResultDetailId.Voi >= 0;

			while (true)
			{
				rr = SummarizeByTarget(this.Eqp.Qe.Query, includeResultDetail, false); // get a summarized value
				if (rr == null) break;
				if (Lex.IsNullOrEmpty(rr.GeneSymbol)) continue; // ignore if blank target symbol

				for (int pup = 0; pup < 2; pup++) // first pass for unpivoted table, 2nd for pivoted by gene
				{
					//try
					//{
					if (pup == 0) mtName = TUTableName;
					else mtName = UnpivotedAssayResult.TargetSummaryPivotTablePrefix + rr.GeneSymbol.ToUpper();
					//}
					//catch (Exception ex) { ex = ex; }
					mt = MetaTableCollection.Get(mtName);
					if (mt == null) continue;
					if (!mpd.MbInstances.ContainsKey(mt.Name)) continue; // have row hash for broker?

					int mbIdx = 0;
					if (!mpd.MbInstances.ContainsKey(mtName))
						throw new Exception("Metatable name not in MbInstances: " + mtName);
					if (mpd.MbInstances[mtName] is TargetAssayMetaBroker)
						mb = (TargetAssayMetaBroker)mpd.MbInstances[mtName]; // broker assoc w/table
					else
					{
						mbList = (List<TargetAssayMetaBroker>)mpd.MbInstances[mtName];
						mb = (TargetAssayMetaBroker)mbList[0];
					}

					while (true) // copy out for each metabroker for metatable
					{
						UnpivotedAssayResultFieldPositionMap voMap = mb.UnpivotedResultsVoMap;
						vo = rr.ToValueObject(mb.Qt.SelectedCount, voMap);

						if (mb.MultipivotRowList == null)
							mb.MultipivotRowList = new List<object[]>();
						mb.MultipivotRowList.Add(vo);

						if (mbList == null) break; // single broker
						mbIdx++; // go to next broker
						if (mbIdx >= mbList.Count) break; // at end of brokers?
						mb = (TargetAssayMetaBroker)mbList[mbIdx];
					}
				} // tables to copy data to loop
			} // target summarization loop

			return;
		}

		/// <summary>
		/// Retrieve next result row
		/// </summary>
		/// <returns></returns>

		public override object[] NextRow()
		{
			MetaTable subMt = null;
			object[] vo;
			UnpivotedAssayResult rr;

			if (TargetPivoted || TargetFamilyPivoted) // return precalculated results within current key subset
				return NextRowTargetPivoted();

			else if (TargetUnpivoted) // summarize by target returning unpivoted result
				return NextRowTargetUnpivoted();

			else // just retrieve unpivoted assay data
			{
				vo = base.NextRow(); // get data via generic broker
				if (vo == null) return null;
				try
				{
					rr = UnpivotedAssayResult.FromValueObjectNew(vo, UnpivotedResultsVoMap);
					rr.NormalizeValues(); // normalize values
					rr.SetActivityBin(); // set the activity bin if possible
					rr.SetActivityClass();
					rr.SetResultValueBackColor();
					vo = rr.ToValueObject(vo.Length, UnpivotedResultsVoMap);
				}
				catch (Exception ex) { ex = ex; }
				return vo;
			}

		}

		/// <summary>
		/// Return the next TargetPivoted or TargetFamilyPivoted row
		/// </summary>
		/// <returns></returns>

		object[] NextRowTargetPivoted()
		{
			MetaTable subMt = null;
			object[] vo;
			UnpivotedAssayResult rr;

			if (TargetPivotedResults == null || Eqp.SearchKeySubset == null) return null;

			while (true)
			{
				if (TargetResultsKeyPos >= Eqp.SearchKeySubset.Count) return null; // past end of keys?
				string cid = Eqp.SearchKeySubset[TargetResultsKeyPos];
				if (!TargetPivotedResults.ContainsKey(cid)) // any results for key?
				{
					TargetResultsKeyPos++;
					TargetResultsPosWithinKey = -1;
					continue;
				}

				List<object[]> keyResults = TargetPivotedResults[cid];
				TargetResultsPosWithinKey++;
				if (TargetResultsPosWithinKey >= keyResults.Count) // end of results for key?
				{
					TargetResultsKeyPos++;
					TargetResultsPosWithinKey = -1;
					continue;
				}

				vo = keyResults[TargetResultsPosWithinKey];
				return vo;
			}
		}

		/// <summary>
		/// Retrieve next TargetUnpivoted row
		/// </summary>
		/// <returns></returns>

		object[] NextRowTargetUnpivoted()
		{
			if (MultipivotRowList == null || MultipivotRowList.Count == 0) return null;
			object[] vo = MultipivotRowList[0]; // note: iterating MultipivotRowDict is slow
			MultipivotRowList.RemoveAt(0);
			return vo;
		}

		/// <summary>
		/// Copy results value and bin to any filterable target fields
		/// </summary>
		/// <param name="rr"></param>

		//void CopyFilterableTargetValues(
		//  UnpivotedAssayResult rr,
		//  object[] vo)
		//{
		//  int voi;

		//  //foreach (int voi0 in FilterableTargetsVoiList) // mark all filterable target values as nonexistant to begin with
		//  //{
		//  //  vo[voi0] = NullValue.NonExistantValue;
		//  //}

		//  string target = rr.GeneSymbol.ToUpper();

		//  if (FilterableTargetsResultPivotMap.ContainsKey(target))
		//  {
		//    voi = FilterableTargetsResultPivotMap[target];
		//    vo[voi] = rr.ResultValue;
		//  }

		//  if (FilterableTargetsBinPivotMap.ContainsKey(target))
		//  {
		//    voi = FilterableTargetsBinPivotMap[target];
		//    vo[voi] = rr.ActivityBin;
		//  }
		//}

		/// <summary>
		/// Get parameters needed for summarization
		/// </summary>
		/// <param name="qt"></param>

		TargetSummaryOptions GetSummarizationParameterValues(
			QueryTable qt)
		{
			QueryColumn qc = qt.GetQueryColumnByName("summarization_options");
			if (qc == null || Lex.IsNullOrEmpty(qc.Criteria))
				Tso = new TargetSummaryOptions(); // use default values
			else
			{
				ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
				string tsoString = Lex.RemoveSingleQuotes(psc.Value);
				Tso = TargetSummaryOptions.Deserialize(tsoString);
			}

			if (Tso.TargetsWithActivesOnly)
			{
				if (!Lex.IsNullOrEmpty(Tso.CrcUpperBound))
					MaxCRC = double.Parse(Tso.CrcUpperBound);

				if (!Lex.IsNullOrEmpty(Tso.SpLowerBound))
					MinSP = double.Parse(Tso.SpLowerBound);
			}

			if (!Lex.IsNullOrEmpty(Tso.TargetMapName)) do
				{
					try { TargetMap = TargetMapDao.GetMapByLabel(Tso.TargetMapName); }
					catch (Exception ex) { break; } // skip it if "none" or no longer exists
					TargetMap = TargetMapDao.GetMapWithCoords(TargetMap.Name);
				} while (false);

			return Tso;
		}

		/// <summary>
		/// Calculate summarized value for a compound, target, result type, assay type group of results
		/// </summary>
		/// <returns></returns>

		public UnpivotedAssayResult SummarizeByTarget(
			Query q,
			bool includeResultDetail,
			bool useCatenatedResultValueFormat)
		{
		Beginning:
			object[] sVo = null, vo = null;
			UnpivotedAssayResult sRow = null, row = null, rr2 = null;
			double mean, sumSq, variance;
			TargetMapCoords tmc;
			int tci;

			string qualifier = null;
			int n = 0; // count in stats
			int nResults = 0; // total number of results for target
			List<double> vals = new List<double>();
			double conc = NullValue.NullNumber;
			string concUnits = "";
			string minQualifier = "";
			double min = NullValue.NullNumber;
			string maxQualifier = "";
			double max = NullValue.NullNumber;
			double sum = 0;
			double product = 1;
			string assayTypes = "";
			bool withinActivityLimits = false;
			bool ignoreTargetResult = false;

			MetaTable mt = MetaTableCollection.GetWithException(AUTableName);
			MetaColumn resultValMc = mt.GetMetaColumnByNameWithException("rslt_val");
			QueryColumn resultValQc = new QueryColumn();
			resultValQc.MetaColumn = resultValMc;
			MetaColumn concValMc = mt.GetMetaColumnByNameWithException("conc_val");

			string resultDetail = "";
			QualifiedNumber fmtQn = new QualifiedNumber();
			UnpivotedAssayResultFieldPositionMap subVoMap = null;

			string fontTag = "", fontTag2 = "";
			string breakTag = "\r\n";

			if (Tso.FormatForSpotfire)
			{ // if going to Spotfire format as html with red highlight of basic result value
				fontTag = "<font color=\"#FF0000\">";
				fontTag2 = "</font>";
				breakTag = "<br>";
			}

		NextRow:
			if (BufferedRows == null) BufferedRows = BufferedRows;
			if (BufferedRows.Count > 0) // use any rows in buffer
			{
				sRow = BufferedRows[0];
				BufferedRows.RemoveAt(0);
				return sRow;
			}

			while (true) // loop till all values for group or end of cursor
			{
				if (NextSubQeVo != null) // use vo in buffer
				{
					vo = NextSubQeVo;
					NextSubQeVo = null;
				}
				else
				{
					vo = SubQe.NextRow();
					if (vo != null) Array.Copy(vo, 1, vo, 0, vo.Length - 1); // shift so Voi indexing is correct
				}

				if (vo == null && n == 0) // at end of data
				{
					if ((Tso.FormatForSpotfire || Tso.FormatForMobius) && TargetMap != null && TargetMap.MarkBounds && !MarkedBounds)
					{ // add two rows with coords at corners of pathway image so full image is displayed in Spotfire or Mobius

						if (Lex.Eq(TargetMap.Name, "Dendograms") && TargetFamily != null && TargetFamily != "Multiple")
						{ // if a single target family on the Dendograms map try to switch to a particular family map
							try
							{
								TargetMap = TargetMapDao.GetMap(TargetFamily + "s");
							}
							catch (Exception ex) { string msg = ex.Message; }
						}

						rr2 = new UnpivotedAssayResult();
						rr2.CompoundId = "00000000"; // query engine needs a valid id, changed to null on output
						rr2.ActivityBin = NullValue.NullNumber;
						rr2.ResultNumericValue = NullValue.NullNumber;
						rr2.TargetMapX = TargetMap.Bounds.Left;
						rr2.TargetMapY = -TargetMap.Bounds.Top;
						BufferedRows.Add(rr2);

						rr2 = new UnpivotedAssayResult();
						rr2.CompoundId = "00000000";
						rr2.ActivityBin = NullValue.NullNumber;
						rr2.ResultNumericValue = NullValue.NullNumber;
						rr2.TargetMapX = TargetMap.Bounds.Right;
						rr2.TargetMapY = -TargetMap.Bounds.Bottom;
						BufferedRows.Add(rr2);

						MarkedBounds = true;

						goto NextRow;
					}

					else return null;
				}

				if (vo != null)
				{
					if (subVoMap == null) // get vo mapping for subquery
					{
						TargetAssayMetaBroker mb2 = (TargetAssayMetaBroker)SubQe.Qtd[0].Broker;
						subVoMap = mb2.UnpivotedResultsVoMap;
					}

					row = UnpivotedAssayResult.FromValueObjectNew(vo, subVoMap);
					if (Lex.Eq(row.GeneSymbol, "CHRM2")) row = row; // debug
					row.NormalizeValues();
					if (row.ResultNumericValue == NullValue.NullNumber) continue; // skip if no numeric value
					if (Lex.Eq(row.AssayType, "FUNCTIONAL") && Lex.Eq(row.GeneFamily, "Kinase"))
						continue; // always skip these since modulated & monitored protein may differ

					if (sRow == null) // copy initial values if first row for group
					{
						sVo = new object[vo.Length];
						Array.Copy(vo, sVo, vo.Length);
						sRow = row; // keep the start row
					}
				}

				if (vo != null && // same group?
				 row.CompoundId == sRow.CompoundId &&
				 row.GeneId == sRow.GeneId &&
				 row.ResultTypeIdTxt == sRow.ResultTypeIdTxt) // don't mix SP && CRC
				{
					nResults++; // count the results
					bool reset = false; // flag for resetting accumulation of values
					bool skip = false;

					if (Lex.Eq(sRow.ResultTypeIdTxt, "SP")) // filter SP data for a single concentration
					{
						//						if (row.Conc <= 0) continue; // skip if no defined concentration (allow undefined now)
						if (row.Conc != conc)
						{
							if (conc == NullValue.NullNumber) // first defined concentration
							{
								conc = row.Conc;
								concUnits = row.ConcUnits;
								reset = true;
							}

							else continue; // ignore other concentrations
						}

						if (Tso.UseTopLevelResultTypeOnly && // ignore SP if CRC value for any assay for target
							LastSummaryRow != null && // CRC would have been just before SP
							row.CompoundId == LastSummaryRow.CompoundId && // same compound
							row.GeneId == LastSummaryRow.GeneId && // and target
							LastSummaryRow.ResultTypeIdTxt == "CRC") // just see CRC
						{
							ignoreTargetResult = true;
							continue;
						}
					}

					string q2 = row.ResultQualifier;
					if (q2 == "=") q2 = "";

					if (qualifier == null) qualifier = q2; // first data

					else if (q2 == qualifier) // same as current qualifier
					{
						if (qualifier == "<")
						{
							if (row.ResultNumericValue > min) continue; // skip if larger than current min
							else if (row.ResultNumericValue < min) reset = true; // reset to this if smaller than current min
						}

						else if (qualifier == ">")
						{
							if (row.ResultNumericValue < max) continue; // skip if smaller than current max
							else if (row.ResultNumericValue > max) reset = true; // reset to this if larger than current max
						}
					}

					else // different qualifier
					{
						if (qualifier == "") continue; // ignore < & > if equal qualifier already seen
						else if (q2 == "") reset = true;
						else // mixed < & >
						{
							qualifier = "";
							reset = true;
							skip = true;
						}
					}

					if (reset) // reset to current value
					{
						n = 0;
						vals = new List<double>();
						min = NullValue.NullNumber;
						max = NullValue.NullNumber;
						sum = 0;
						product = 1;
						qualifier = q2;
						assayTypes = "";
					}

					if (skip) continue; // ignore this value

					if (Lex.Eq(sRow.ResultTypeIdTxt, "SP"))
					{
						sum += row.ResultNumericValue; // sum for arithimetic mean
						vals.Add(row.ResultNumericValue);

						fmtQn.NumberValue = row.ResultNumericValue;
						fmtQn.Qualifier = row.ResultQualifier;
						if (fmtQn.Qualifier == "=") fmtQn.Qualifier = "";
						if (includeResultDetail)
						{
							string spTxt =
								fmtQn.Format(resultValMc.Format, resultValMc.Decimals);

							resultDetail += fontTag + "SP: " + spTxt + row.Units;
							if (!String.IsNullOrEmpty(row.ConcUnits))
							{
								string concTxt =
									QualifiedNumber.FormatNumber(row.Conc, concValMc.Format, concValMc.Decimals);
								resultDetail += " @ " + concTxt + " " + row.ConcUnits;
							}
							resultDetail += fontTag2 + ", " + row.AssayDatabase + " " + row.AssayIdTxt + ": " + row.AssayName + breakTag;
						}

						if (MinSP != NullValue.NullNumber && row.ResultNumericValue >= MinSP)
							withinActivityLimits = true;

						n++;
					}

					else // CRC
					{
						product *= row.ResultNumericValue; // multiply for geometric mean
						vals.Add(row.ResultNumericValue);

						fmtQn.NumberValue = row.ResultNumericValue;
						fmtQn.Qualifier = row.ResultQualifier;
						if (fmtQn.Qualifier == "=") fmtQn.Qualifier = "";

						if (includeResultDetail)
						{
							string crcTxt =
								fmtQn.Format(resultValMc.Format, resultValMc.Decimals);

							resultDetail += fontTag + "CRC: " + crcTxt + " " + row.Units + fontTag2 + ", " +
								row.AssayDatabase + " " + row.AssayIdTxt + ": " + row.AssayName + breakTag;
						}

						if (MaxCRC != NullValue.NullNumber && row.ResultNumericValue <= MaxCRC)
							withinActivityLimits = true;

						n++;
					}

					if (min == NullValue.NullNumber || row.ResultNumericValue < min)
					{
						min = row.ResultNumericValue;
						minQualifier = row.ResultQualifier;
					}

					if (max == NullValue.NullNumber || row.ResultNumericValue > max)
					{
						max = row.ResultNumericValue;
						maxQualifier = row.ResultQualifier;
					}

					if (!assayTypes.Contains(row.AssayType))
					{ // accumulate assay types
						if (assayTypes.Length > 0) assayTypes += ", ";
						assayTypes += row.AssayType;
					}

					if (!String.IsNullOrEmpty(row.GeneFamily) && TargetFamily != "Multiple")
					{ // keep track of target families seen to be used to select proper section of general family dendogram
						if (TargetFamily == null) TargetFamily = row.GeneFamily;
						else if (Lex.Ne(TargetFamily, row.GeneFamily)) TargetFamily = "Multiple";
					}
				}

				else // all done for this compound, target, result type; do statistics & return row
				{
					NextSubQeVo = vo; // buffer current row for refetch
					NextSubQeRow = row;

					//if (row.CompoundId != sRow.CompoundId) row = row; // debug

					if (((Lex.Eq(sRow.ResultTypeIdTxt, "SP") && MinSP != NullValue.NullNumber) ||
						(Lex.Eq(sRow.ResultTypeIdTxt, "CRC") && MaxCRC != NullValue.NullNumber)) &&
						!withinActivityLimits) // skip if not within requested activity limits
						goto Beginning;

					if (ignoreTargetResult) goto Beginning;

					if (n == 0) // can happen if mixed < & > but no unqualified values
					{
						sRow.ResultNumericValue = NullValue.NullNumber;
						sRow.StdDev = NullValue.NullNumber;
						if (MaxCRC != NullValue.NullNumber && MinSP != NullValue.NullNumber)
							goto Beginning; // skip if limits are set
					}

					else if (Lex.Eq(sRow.ResultTypeIdTxt, "SP"))
					{
						if (Tso.UseMeans)
						{
							if (n == 1)
							{
								sRow.ResultNumericValue = sum;
								sRow.StdDev = 0;
							}

							else
							{
								mean = sum / n; // arithmetic mean for SP data
								sRow.ResultNumericValue = mean;
								sumSq = 0;
								for (int vi = 0; vi < vals.Count; vi++)
								{
									sumSq += Math.Pow(vals[vi] - mean, 2);
								}
								variance = sumSq / n;
								sRow.StdDev = Math.Sqrt(variance);
							}
						}

						else // most potent, take max value
						{
							sRow.ResultNumericValue = max;
							qualifier = maxQualifier;
						}
					}

					else  // geometric mean for CRC data
					{
						if (Tso.UseMeans)
						{
							if (n == 1)
							{
								sRow.ResultNumericValue = product;
								sRow.StdDev = 0;
							}
							else
							{
								mean = Math.Pow(product, (1.0 / n));
								if (mean <= 0) mean = 0.0001; // min allowed value

								sRow.ResultNumericValue = mean;
								sumSq = 0;
								double logMean = Math.Log(mean);
								for (int vi = 0; vi < vals.Count; vi++)
								{
									double logVal = Math.Log(vals[vi]);
									sumSq += Math.Pow(logVal - logMean, 2);
								}
								variance = sumSq / n;
								variance = Math.Sqrt(variance);
								sRow.StdDev = Math.Exp(variance);
							}
						}

						else // just take min value
						{
							sRow.ResultNumericValue = min;
							qualifier = minQualifier;
						}
					}

					if (qualifier == "=") qualifier = "";
					sRow.ResultQualifier = qualifier;
					sRow.NValue = n;
					sRow.NValueTested = nResults;
					sRow.ResultTextValue = null;

					sRow.ResultValue = new QualifiedNumber(sRow.ResultNumericValue); // also copy to qualified number
					sRow.ResultValue.Qualifier = qualifier;
					sRow.ResultValue.NValue = n;
					sRow.ResultValue.NValueTested = nResults;
					sRow.ResultValue.StandardDeviation = sRow.StdDev;
					sRow.ResultValue.StandardError = sRow.StdErr;
					sRow.ResultValue.DbLink = sRow.CompoundId + "," + sRow.GeneId + "," + sRow.ResultTypeIdTxt;

					if (useCatenatedResultValueFormat)
					{ // format result as text string with value, units and concentration in a single string
						QualifiedNumber qn2 = new QualifiedNumber();
						if (sRow.ResultValue.NValue > 1) { int nnn = 2; }
						QnfEnum qnFormat = QnfEnum.Combined | q.StatDisplayFormat;
						qn2.TextValue = sRow.ResultValue.Format(resultValQc, false, resultValMc.Format, resultValMc.Decimals, qnFormat, false);

						if (Lex.Eq(sRow.ResultTypeIdTxt, "SP"))
						{
							qn2.TextValue += sRow.Units;
							string concTxt =
								QualifiedNumber.FormatNumber(conc, concValMc.Format, concValMc.Decimals);
							if (!String.IsNullOrEmpty(concUnits)) qn2.TextValue += " @ " + concTxt + " " + concUnits;
						}

						else // format CRC value
						{
							qn2.TextValue += " " + sRow.Units;
						}

						qn2.DbLink = sRow.ResultValue.DbLink;
						sRow.ResultValue = qn2;
					}

					sRow.SetActivityBin();
					sRow.SetActivityClass();
					sRow.SetResultValueBackColor();

					sRow.ResultDetail = resultDetail;
					sRow.AssayType = assayTypes; // store accumulated assay types

					sRow.SetDrawingOrder();

					string family = sRow.GeneFamily;
					if (Lex.Eq(family, "Nuclear Hormone Receptor")) family = "NHR";
					else if (Lex.Eq(family, "Phosphodiesterase")) family = "PDE";
					sRow.GeneFamilyTargetSymbol = // catenated gene-family and target symbol
						family + "-" + sRow.GeneSymbol;

					if (Tso.FormatForSpotfire) // format target symbol with html link for Spotfire
					{
						string uri = TargetAssayMetaBroker.GetTargetDescriptionUrl(sRow.GeneSymbol);
						if (uri != null)
						{ // include symbol with link to description at astart
							sRow.GeneDescription = "<a href=\"" + uri + "\" target=_blank>" +
							 sRow.GeneSymbol + "</a> - " + sRow.GeneDescription;
						}
					}

					if (sRow.CompoundId != CurrentCid)
						CurrentCid = sRow.CompoundId;

					if (!CidPosDict.ContainsKey(CurrentCid))
						CidPosDict[CurrentCid] = CidPosDict.Count + 1;
					sRow.CidOrder = CidPosDict[CurrentCid]; // set "Load Order"

					if (TargetMap != null && TargetMap.Coords.ContainsKey(sRow.GeneId)) // Store coordinates
					{
						List<TargetMapCoords> coordList = TargetMap.Coords[sRow.GeneId];
						for (tci = 0; tci < coordList.Count; tci++)
						{
							if (tci == 0) rr2 = sRow;
							else rr2 = sRow.CloneUnpivotedAssayResult();

							tmc = coordList[tci];
							rr2.TargetMapX = tmc.X;
							rr2.TargetMapY = -tmc.Y; // use negative value for y
							if (tci > 0) BufferedRows.Add(rr2); // add to buffer
						}
					}

					LastSummaryRow = sRow;
					return sRow;

				}
			}
		}

		/// <summary>
		/// Close broker
		/// </summary>

		public override void Close()
		{
			if (TargetPivoted || TargetFamilyPivoted) { } // already closed

			else if (TargetUnpivoted) // summarize by target returning unpivoted result
			{
				if (SubQe != null) SubQe.Close();
			}

			else base.Close(); // close normal query

			return;
		}

		/// <summary>
		/// Convert table to table summarized/pivoted by gene or gene family
		/// given result set.
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="q"></param>
		/// <param name="ResultKeys"></param>

		public override void ExpandToMultipleTables(
			QueryTable qt,
			Query q,
			List<string> ResultKeys)
		{
			QueryEngine qe = null;

			Qt = qt;

			if (TargetPivoted) RemapTargetPivoted("Target", qe, qt, q, ResultKeys); // summarize and pivot by target

			else if (TargetFamilyPivoted) RemapTargetPivoted("TargetFamily", qe, qt, q, ResultKeys); // summarize by target & pivot by target family

			else throw new Exception("Unexpected metatable: " + MtName);
		}

		/// <summary>
		/// Summarize and pivot by target and result type
		/// </summary>
		/// <param name="qe"></param>
		/// <param name="qt"></param>
		/// <param name="q"></param>
		/// <param name="ResultKeys"></param>

		void RemapTargetPivoted(
			string pivotType,
			QueryEngine qe,
			QueryTable qt,
			Query newQuery,
			List<string> ResultKeys)
		{
			UnpivotedAssayResult rr;
			QueryColumn qc, qc2;
			MetaColumn mc, mc2;
			string pivotValue;
			int row, pivotPos, pos, colsPerPivotValue, qci, mci, pi, ri;

			bool pivotByTarget = false, pivotByFamily = false, orderByFamily = false;
			if (pivotType == "Target")
			{
				pivotByTarget = true;
				orderByFamily = // if gene family selected then order by it first before target
					qt.GetQueryColumnByNameWithException("gene_fmly").Selected;
			}
			else
			{
				pivotByFamily = true;
				orderByFamily = true;
			}

			MetaTable mt = qt.MetaTable;
			SubQe = PrepareAndOpenAssayDataSubquery(qe.Query, qt, ResultKeys);

			List<UnpivotedAssayResult> rrList = new List<UnpivotedAssayResult>(); // list of results summarized by target & result type
			List<string> pivotList = new List<string>(); // list of pivot values to use in header labels
			List<UnpivotedAssayResultFieldPositionMap> pivotVoMapList = new List<UnpivotedAssayResultFieldPositionMap>(); // mapping of col names to vo position for each pivot
			Dictionary<string, int> pivotDict = new Dictionary<string, int>(); // dict with key = pivot value, value = pivot position

			BufferedRows = new List<UnpivotedAssayResult>();

			while (true) // read in the raw data & summarize it
			{
				rr = SummarizeByTarget(qe.Query, false, false);
				if (rr == null) break;
				rrList.Add(rr);
				if (pivotByTarget)
				{ // header is family symbol/target symbol
					if (orderByFamily) pivotValue = rr.GeneFamily + " ";
					else pivotValue = "";
					pivotValue += rr.GeneSymbol;
				}
				else // header is family and result type
					pivotValue = rr.GeneFamily + " - " + rr.ResultTypeIdTxt;

				if (!pivotDict.ContainsKey(pivotValue.ToUpper()))
				{
					pivotList.Add(pivotValue);
					pivotVoMapList.Add(new UnpivotedAssayResultFieldPositionMap());
					pivotDict[pivotValue.ToUpper()] = pivotDict.Count; // store position
				}
			}

			if (rrList.Count == 0) return;

			// Build pivoted metatable with prototype cols

			MetaTable mt3 = mt.Clone();
			mt3.Name += "_2"; // different name for derived table
			mt3.MultiPivot = false; // don't attempt to remap further
			mt3.MetaColumns.Clear();

			for (qci = 0; qci < qt.QueryColumns.Count; qci++)
			{ // get list of columns along with key
				qc = qt.QueryColumns[qci];
				mc = qc.MetaColumn;
				if (!qc.Is_Selected_or_GroupBy_or_Sorted) continue;
				if (Lex.Eq(mc.Name, "gene_fmly")) continue; // goes to header
				if (pivotByTarget && Lex.Eq(mc.Name, "gene_symbl")) continue; // goes to header if pivoting by target
				if (pivotByFamily && Lex.Eq(mc.Name, "rslt_typ")) continue; // goes to header if pivoting by family

				mt3.AddMetaColumn(qc.MetaColumn.Clone());
			}

			// Build column labels

			colsPerPivotValue = mt3.MetaColumns.Count - 1; // no. of cols to duplicate (don't include key)
			for (pi = 0; pi < pivotList.Count; pi++)
			{
				UnpivotedAssayResultFieldPositionMap voMap = pivotVoMapList[pi];
				for (mci = 1; mci < colsPerPivotValue + 1; mci++)
				{ // duplicate cols for each target & modify names & labels
					MetaColumn modelMc = mt3.MetaColumns[mci];
					MetaColumn mc3 = modelMc.Clone();
					mc3.Name += "_" + pi;

					if (Lex.Eq(modelMc.Name, "gene_symbl")) mc3.Label = pivotList[pi] + " Gene"; // complete gene-family header
					else if (Lex.Eq(modelMc.Name, "rslt_val")) mc3.Label = pivotList[pi] + " Result"; // complete result header
					// (don't qualify for other cols) else mc3.Label = pivotList[pi] + " - " + mc3.Label; // other results with label
					mt3.AddMetaColumn(mc3);
					voMap.TrySetVoi(modelMc.Name, mt3.MetaColumns.Count - colsPerPivotValue - 1); // vo position for this column
				}
			}

			mt3.MetaColumns.RemoveRange(1, colsPerPivotValue); // remove prototype cols
			MetaTableCollection.Add(mt3); // update map
			QueryTable qt3 = new QueryTable(newQuery, mt3);
			qt3.SelectAll();

			// Pivot data & format the pivoted rows for retrieval

			int vo3Len = 1 + pivotDict.Count * colsPerPivotValue;

			int firstRowForCurrentCid = 0;
			int[] rowsUsedPerColumn = new int[pivotDict.Count]; // keep count of number of rows filled for each target for current cid
			List<object[]> vo3List = new List<object[]>(); // list of pivoted vos
			string currentCid = "";
			for (ri = 0; ri < rrList.Count; ri++)
			{
				rr = rrList[ri];
				if (rr.CompoundId != currentCid)
				{
					Array.Clear(rowsUsedPerColumn, 0, rowsUsedPerColumn.Length);
					firstRowForCurrentCid = vo3List.Count;
					currentCid = rr.CompoundId;
				}

				if (pivotByTarget) // header is target symbol and result type
				{ // header is family symbol (optional), target symbol and result type
					if (orderByFamily) pivotValue = rr.GeneFamily + " ";
					else pivotValue = "";
					pivotValue += rr.GeneSymbol;
				}

				else // header is family and result type
					pivotValue = rr.GeneFamily + " - " + rr.ResultTypeIdTxt;

				pivotPos = pivotDict[pivotValue.ToUpper()];
				int vo3Row = firstRowForCurrentCid + rowsUsedPerColumn[pivotPos];
				rowsUsedPerColumn[pivotPos]++;
				if (vo3Row >= vo3List.Count)
					vo3List.Add(new object[vo3Len]);
				object[] vo3 = vo3List[vo3Row];
				vo3[0] = rr.CompoundId;
				rr.ToValueObject(vo3, pivotVoMapList[pivotPos]); // copy values to vo
			}

			TargetPivotedResults = new Dictionary<string, List<object[]>>();
			foreach (object[] oa in vo3List)
			{
				string cid = oa[0].ToString();
				if (!TargetPivotedResults.ContainsKey(cid))
					TargetPivotedResults[cid] = new List<object[]>();
				TargetPivotedResults[cid].Add(oa);
			}

			return;
		}

		/// <summary>
		/// Build and subquery to retrieve assay data for summarization by target
		/// </summary>

		QueryEngine PrepareAndOpenAssayDataSubquery(
			Query q,
			QueryTable qt,
			List<string> keyList)
		{
			MetaTable mt = qt.MetaTable;
			Query q2 = q.Clone(); // build query to select underlying data
			q2.Tables.Clear();
			q2.SingleStepExecution = true;
			q2.FilterNullRowsStrong = true;

			if (q2.KeySortOrder == 0) // need sort so grouping works
				q2.KeySortOrder = -1;

			MetaTable mt2 = MetaTableCollection.GetWithException(AUTableName);
			QueryTable qt2 = new QueryTable(q2, mt2);
			QueryColumn qc2;

			foreach (QueryColumn qc_ in qt.QueryColumns)
			{ // copy column selections & criteria to subquery table
				if (!qc_.Selected && qc_.SortOrder == 0 && String.IsNullOrEmpty(qc_.Criteria)) continue;
				qc2 = qt2.GetQueryColumnByName(qc_.MetaColumn.Name);
				if (qc2 == null) continue;
				qc2.Selected = qc_.Selected;
				//if (!qc_.MetaColumn.BrokerFiltering)
				qc2.Criteria = qc_.Criteria;
				qc2.SortOrder = qc_.SortOrder;
				string mcName = qc_.MetaColumn.Name;
			}

			if (keyList != null && keyList.Count > 0)
			{
				q2.ResultKeys = keyList;
				q2.UseResultKeys = true;
			}

			SetSelectionsNeededForSummarization(qt2); // be sure we have other necessary cols selected

			QueryEngine qe = new QueryEngine();
			qe.ExecuteQuery(q2);
			NextSubQeVo = null; // clear read-ahead data

			return qe;
		}

		/// <summary>
		/// Be sure columns are selected that are needed for summarization by target
		/// </summary>
		/// <param name="qt"></param>

		void SetSelectionsNeededForSummarization(
			QueryTable qt)
		{
			qt.GetQueryColumnByName("GENE_FMLY").Selected = true;
			qt.GetQueryColumnByName("GENE_ID").Selected = true;
			qt.GetQueryColumnByName("ASSY_ID_TXT").Selected = true;
			qt.GetQueryColumnByName("RSLT_TYP").Selected = true;
			qt.GetQueryColumnByName("RSLT_VAL_QUALIFIER").Selected = true;
			qt.GetQueryColumnByName("RSLT_VAL_NBR").Selected = true;
			qt.GetQueryColumnByName("CONC_VAL").Selected = true;
			qt.GetQueryColumnByName("CONC_UOM").Selected = true;
			qt.GetQueryColumnByName("ASSY_DB").Selected = true;
			qt.GetQueryColumnByName("ASSY_TYP").Selected = true; // type of assay, e.g. binding, functional
			qt.GetQueryColumnByName("RSLT_VAL").Selected = false; // gets rebuilt so not needed for selection
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
			QueryTable qt2;
			MetaTable mt2;
			QueryColumn qc2;
			MetaColumn mc2;

			Query q = new Query();

			if (Lex.Ne(mt.Name, AUTableName)) // drilling down from data summarized by target
			{ // ResultId is of the form compoundId, targetId, resultType (i.e. SP, CRC)
				string[] sa = linkInfo.Split(',');
				if (sa.Length != 3) throw new Exception("Invalid resultId: " + linkInfo);
				string cid = sa[0];
				string geneId = sa[1];
				string resultType = sa[2];

				mt2 = MetaTableCollection.Get(AUTableName);
				if (mt2 == null) throw new Exception("Can't find " + AUTableName);

				qt2 = new QueryTable(q, mt2);
				q.KeyCriteria = "= " + cid;

				qc2 = qt2.GetQueryColumnByNameWithException("gene_id");
				qc2.Criteria = "gene_id = " + geneId;

				qc2 = qt2.GetQueryColumnByNameWithException("rslt_typ");
				qc2.Criteria = "rslt_typ = " + Lex.AddSingleQuotes(resultType);

				qc2 = qt2.GetQueryColumnByNameWithException("top_lvl_rslt");
				qc2.Criteria = "top_lvl_rslt = 'Y'"; // only top level results

				return q;
			}

			else // drilling down from all_bioassay_unpivoted to specific source data
			{ // ResultId is formatted by the source with the first item being the source name
				Match m = Regex.Match(linkInfo, @"(.*),(.*),(.*)", RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
				string mtName = m.Groups[1].Value;
				string mcId = m.Groups[2].Value;
				string linkInfo2 = m.Groups[3].Value;
				mt2 = MetaTableCollection.GetWithException(mtName);
				IMetaBroker imb = (IMetaBroker)MetaBrokerUtil.GlobalBrokers[(int)mt2.MetaBrokerType];
				q = imb.GetDrilldownDetailQuery(mt2, mcId, level, linkInfo2);
				return q;
			}
		}

		/// <summary>
		/// Build a query to fetch the assays for a target
		/// </summary>
		/// <param name="geneSymbol"></param>
		/// <returns></returns>

		public static Query BuildTargetAssayListQuery(
			string geneSymbol)
		{
			Query q = BuildTargetAssayQuery("cmn_assy_atrbts", geneSymbol);
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
			Query q = BuildTargetAssayQuery(AUTableName, geneSymbol);
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
			Query q = BuildTargetAssayQuery(TUTableName, geneSymbol);
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

			QueryColumn qc = qt.GetQueryColumnByNameWithException("gene_symbl");
			qc.Criteria = "gene_symbl = " + Lex.AddSingleQuotes(geneSymbol);
			qc.CriteriaDisplay = "= " + geneSymbol;

			qc = qt.GetQueryColumnByNameWithException("top_lvl_rslt");
			qc.Criteria = "top_lvl_rslt = 'Y'"; // only top level results
			qc.CriteriaDisplay = "= Y";

			return q;
		}

		/// <summary>
		/// Summarized by assay, unpivoted
		/// </summary>

		public bool AssayUnpivoted
		{
			get
			{
				return Lex.Eq(MtName, AUTableName);
			}
		}
		/// <summary>
		/// Summarized by target, unpivoted
		/// </summary>

		public bool TargetUnpivoted
		{
			get
			{
				return Lex.Eq(MtName, TUTableName) ||
					Lex.StartsWith(MtName, UnpivotedAssayResult.TargetSummaryPivotTablePrefix);
			}
		}

		/// <summary>
		/// Summarized by target, pivoted
		/// </summary>

		public bool TargetPivoted
		{
			get
			{
				if (Lex.Eq(MtName, "all_target_assay_summary") || // initial table
					Lex.Eq(MtName, "all_target_assay_summary_2")) // transformed version
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Summarized by target family, pivoted
		/// </summary>

		public bool TargetFamilyPivoted
		{
			get
			{
				if (Lex.Eq(MtName, "all_target_assay_summary_by_gene_family") || // initial table
					Lex.Eq(MtName, "all_target_assay_summary_by_gene_family_2")) // transformed version
					return true;
				else return false;
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

		/// <summary>
		/// Get the target assay dictionary, reading in if necessary
		/// </summary>

		public static AssayDict AssayDict
		{
			get
			{
				if (AssayDict.Instance == null) try
					{
						string fileName = ServicesDirs.MetaDataDir + @"\TargetMaps\TargetAssayAttributes.bin";
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
		/// Read in Assay to Gene association info
		/// </summary>
		/// <returns></returns>

		public static AssayDict ReadAssayDictFromDatabase()
		{
			int t0 = TimeOfDay.Milliseconds();
			MetaTable mt = MetaTableCollection.Get("cmn_assy_atrbts");
			if (mt == null) throw new Exception("Can't get table cmn_assy_atrbts");

			UnpivotedAssayResultFieldPositionMap voMap = new UnpivotedAssayResultFieldPositionMap(); // used for fast indexing of value by col name
			string fList = "";
			for (int mci = 0; mci < mt.MetaColumns.Count; mci++)
			{
				string colMap = mt.MetaColumns[mci].ColumnMap;
				voMap.TrySetVoi(colMap, mci);
				if (fList != "") fList += ", ";
				fList += colMap;
			}

			string sql =
				"select " + fList + " " +
				"from mbs_owner.cmn_assy_atrbts " +
				"order by lower(gene_fmly), lower(gene_symbl), lower(assy_nm)";

			sql = AssayAttributesDao.AdjustAssayAttrsTableName(sql);

			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();

			AssayDict dict = new AssayDict();

			int readCnt = 0;
			while (true)
			{
				if (!dao.Read()) break;
				//if (readCnt > 100) break; // debug !!!
				readCnt++;
				object[] vo = new object[mt.MetaColumns.Count];
				for (int mci = 0; mci < mt.MetaColumns.Count; mci++)
					vo[mci] = dao.GetObject(mci);

				AssayAttributes row = AssayAttributes.FromValueObjectNew(vo, voMap);
				dict.AssayList.Add(row);
				dict.SetMaps(row);
			}

			dao.CloseReader();
			dao.Dispose();
			t0 = TimeOfDay.Milliseconds() - t0;
			return dict;
		}

		/// <summary>
		/// Show NCBI Entre Gene web page for supplied geneId and/or symbol
		/// </summary>
		/// <param name="geneIdSymbol"></param>

		public static string GetTargetDescriptionUrl(
			string targetIdSymbol)
		{
			int targetId;
			string GeneSymbol;

			if (String.IsNullOrEmpty(targetIdSymbol))
				return null;

			else if (Lex.IsInteger(targetIdSymbol)) // numeric gene id
				targetId = int.Parse(targetIdSymbol);

			else if (targetIdSymbol.Contains("-")) // 1234-Symbol form
			{
				string[] sa = targetIdSymbol.Split('-');
				GeneSymbol = sa[1].ToUpper();
				try { targetId = GeneSymbolToId(GeneSymbol); }
				catch { return null; }
			}

			else // must be symbol
			{
				GeneSymbol = targetIdSymbol.ToUpper();
				try { targetId = GeneSymbolToId(GeneSymbol); }
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
		/// <param name="GeneSymbol"></param>
		/// <returns></returns>

		public static int GeneSymbolToId(
			string GeneSymbol)
		{
			string sql =
				"select gene_id " +
				"from mbs_owner.cmn_assy_atrbts " +
				"where upper(gene_symbl) = :0";

			sql = AssayAttributesDao.AdjustAssayAttrsTableName(sql);

			DbCommandMx d = new DbCommandMx();
			d.Prepare(sql, OracleDbType.Varchar2);
			d.ExecuteReader(GeneSymbol.ToUpper());
			if (!d.Read())
			{
				d.Dispose();
				throw new Exception("Target Symbol not found: " + GeneSymbol);
			}

			int targetId = d.GetInt(0);
			d.Dispose();
			return targetId;
		}

		/// <summary>
		/// Update the file cache of all target assay data
		/// </summary>
		/// <returns></returns>

		public static string BuildAllTargetAssayDataCache()
		{
			UnpivotedAssayResultFieldPositionMap voMap = new UnpivotedAssayResultFieldPositionMap(); // map col name to vo index for basic unpivoted assay data

			int cid;

			DateTime baseDate = new DateTime(1900, 1, 1);

			Query q = new Query();
			MetaTable mt = MetaTableCollection.Get(AUTableName);
			QueryTable qt = new QueryTable(mt);
			qt.SelectColumns(
				"corp_srl_nbr, activity_bin, rslt_val, rslt_val_qualifier, rslt_val_nbr, rslt_val_txt, " +
				"rslt_uom, conc_val, conc_uom, run_dt, assy_id_nbr, rslt_typ_id_nbr");

			q.ClearSorting(); // don't care about order

			//q.KeyCriteria = "in ('3045982', 3045983')"; // limit keys for dev only

			QueryColumn qc = qt.GetQueryColumnByName("assy_db");
			qc.Criteria = "assy_db = 'RDW'";

			q.AddQueryTable(qt);
			q.SingleStepExecution = true;

			QueryEngine qe = new QueryEngine();
			List<string> keys = qe.ExecuteQuery(q);

			voMap.InitializeFromQueryTableVoPositions(qt, 0);

			BinaryWriter cidBw = OpenBinaryWriter("CompoundId.new");
			BinaryWriter assayIdBw = OpenBinaryWriter("AssayId.new");
			BinaryWriter resultTypeIdBw = OpenBinaryWriter("ResultTypeId.new");
			BinaryWriter activityBinBw = OpenBinaryWriter("ActivityBin.new");
			BinaryWriter resultQualifierBw = OpenBinaryWriter("ResultQualifier.new");
			BinaryWriter resultNumericValueBw = OpenBinaryWriter("ResultNumericValue.new");
			BinaryWriter resultTextValueBw = OpenBinaryWriter("ResultTextValue.new");
			BinaryWriter unitsBw = OpenBinaryWriter("Units.new");
			BinaryWriter concBw = OpenBinaryWriter("Conc.new"); // conc value & units
			BinaryWriter runDateBw = OpenBinaryWriter("RunDate.new"); // days since Jan 1, 1900

			int rowCount = 0;
			int failCount = 0;
			UnpivotedAssayResult tar = new UnpivotedAssayResult();

			while (true)
			{
				object[] vo = qe.NextRow();
				if (vo == null) break;

				tar.FromValueObject(vo, voMap);

				if (int.TryParse(tar.CompoundId, out cid))
					cidBw.Write(cid);
				else { failCount++; continue; }

				assayIdBw.Write((ushort)tar.AssayId2);

				resultTypeIdBw.Write((ushort)tar.ResultTypeId2);

				activityBinBw.Write((byte)tar.ActivityBin);

				if (!Lex.IsNullOrEmpty(tar.ResultQualifier))
				{
					char c = tar.ResultQualifier[0];
					if (c == '=') c = ' ';
					resultQualifierBw.Write(c);
				}
				else resultQualifierBw.Write(' ');

				resultNumericValueBw.Write((float)tar.ResultNumericValue);

				if (!Lex.IsNullOrEmpty(tar.ResultTextValue))
				{
					resultTextValueBw.Write((int)rowCount);
					resultTextValueBw.Write(Lex.S((string)tar.ResultTextValue.Trim()));
				}

				if (!Lex.IsNullOrEmpty(tar.Units))
				{
					unitsBw.Write((int)rowCount);
					unitsBw.Write(tar.Units.Trim());
				}

				if (tar.Conc > 0)
				{
					concBw.Write((float)tar.Conc);
					if (Lex.IsNullOrEmpty(tar.ConcUnits))
						tar.ConcUnits = "";
					concBw.Write(tar.ConcUnits.Trim());
				}

				if (tar.RunDate != DateTime.MinValue)
				{
					TimeSpan ts = tar.RunDate.Subtract(baseDate);
					if (ts.TotalDays > 0 && ts.TotalDays < 65534)
						runDateBw.Write((ushort)ts.TotalDays);
					else runDateBw.Write((ushort)0);
				}
				else runDateBw.Write((ushort)0);

				rowCount++;

				if (UAL.Progress.IsTimeToUpdate)
					UAL.Progress.Show("Rows processed: " + rowCount);
			}

			StreamWriter sw = new StreamWriter("xxx"); // UalUtil.CachedDataDir + @"\" + "CacheParms.new");
			sw.WriteLine(rowCount); // write out the count
			sw.Close();

			Progress.Hide();

			qe.Close();

			cidBw.Close();
			assayIdBw.Close();
			resultTypeIdBw.Close();
			activityBinBw.Close();
			resultQualifierBw.Close();
			resultNumericValueBw.Close();
			resultTextValueBw.Close();
			unitsBw.Close();
			concBw.Close();
			runDateBw.Close();

			ActivateNewFile("CompoundId");
			ActivateNewFile("AssayId");
			ActivateNewFile("ResultTypeId");
			ActivateNewFile("ActivityBin");
			ActivateNewFile("ResultQualifier");
			ActivateNewFile("ResultNumericValue");
			ActivateNewFile("ResultTextValue");
			ActivateNewFile("Units");
			ActivateNewFile("Conc");
			ActivateNewFile("RunDate");
			ActivateNewFile("CacheParms");

			return "";
		}

		static BinaryWriter OpenBinaryWriter(string fileName)
		{
			FileStream fs = File.Open("xxx", FileMode.Create); // UalUtil.CachedDataDir + @"\" + fileName, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);
			return bw;
		}

		public static void ActivateNewFile(
			string fileName)
		{
			string fBase = ""; //  UalUtil.CachedDataDir + @"\" + fileName;
			FileUtil.BackupAndReplaceFile(fBase + ".bin", fBase + ".bak", fBase + ".new");
			return;
		}

	}

}
