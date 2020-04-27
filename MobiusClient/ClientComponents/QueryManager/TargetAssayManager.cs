using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;

using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mobius.ClientComponents
{
/// <summary>
/// Data Table Manager - Additional methods
/// </summary>

	public partial class DataTableManager : IDataTableManager
	{

/// <summary>
/// Get the target assay dictionary, reading in if necessary
/// </summary>

		public static AssayDict TargetAssayDict
		{
			get
			{
				if (AssayDict.Instance == null) try
					{
						string serverFile = @"<MetaDataDir>\AssayAttributes.bin";
						string clientFile = ClientCacheDir + "AssayAttributes.bin";
						bool changed = ServerFile.GetIfChanged(serverFile, clientFile);
						AssayDict.Instance = AssayDict.DeserializeFromFile(clientFile);
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
/// Summarize the current data set producing one row per key value (e.g. CompoundId)
/// </summary>
/// <param name="sumMethod"></param>
/// <param name="outputFormat"></param>
/// <param name="targetMap"></param>
/// <returns></returns>

		public QueryManager Summarize(
			TargetAssaySummarizationLevel sumLevel,
			SummarizationType sumMethod,
			ColumnsToTransform colsToSumm,
			OutputDest outputDest,
			TargetMap targetMap)
		{
			//if (UnpivotedResults) return SummarizeUnpivoted(sumLevel, sumMethod, outputDest, targetMap);
			//else return SummarizePivoted(sumLevel, sumMethod, colsToSumm, outputDest, targetMap);
			throw new NotImplementedException();
		}


/// <summary>
/// Summarize unpivoted unsummarized assay data by target
/// </summary>
/// <param name="parms"></param>
/// <param name="qm2"></param>
/// <returns></returns>

		public QueryManager SummarizeByTargetUnpivoted(
			TargetSummaryOptions summaryOptions,
			QueryManager qm2)
		{
			// This method takes an unpivoted input query, summarizes it according to the summarization parameters
			// and then formats the summarized data according to the specified output format.
			// If a targetMap is specified then then coordinates are included in the output, the summarization 
			// level must be target, and the output format must be unpivoted.

			QueryTable qt, qt2;
			DataRowMx dr, dr2;
			DataRowAttributes dra;
			string cid = "", currentCid;
			int rti, rfi;

			AssayDict tad = TargetAssayDict; // be sure target assay dict has been loaded

			TargetAssaySummarizationLevel sumLevel = TargetAssaySummarizationLevel.Target; // target level
			SummarizationType sumMethod = summaryOptions.UseMeans ? SummarizationType.BioResponseMean : SummarizationType.MostPotent;
			OutputDest outputDest = OutputDest.WinForms;
			TargetMap targetMap = TargetMapDao.GetMapWithCoords(summaryOptions.TargetMapName);

			qt = Query.GetQueryTableByName(MultiDbAssayDataNames.CombinedNonSumTableName);
			if (qt == null) throw new Exception("Query table not found: " + MultiDbAssayDataNames.CombinedNonSumTableName);

			UnpivotedAssayResultFieldPositionMap voMap = UnpivotedAssayResultFieldPositionMap.NewOriginalMap(); // used for fast indexing of value by col name
			voMap.InitializeFromQueryTableVoPositions(qt, 0);

			if (qm2 == null) // need to create query manager?
			{
				qm2 = new QueryManager();
				qm2 = InitializeSubqueryQm(MultiDbAssayDataNames.CombinedNonSumTableName);
			}

			Query q2 = qm2.Query;
			qt2 = q2.GetQueryTableByNameWithException(MultiDbAssayDataNames.BaseTableName);

			UnpivotedAssayResultFieldPositionMap voMap2 = UnpivotedAssayResultFieldPositionMap.NewOriginalMap(); // used for fast indexing of value by col name
			voMap2.InitializeFromQueryTableVoPositions(qt2, 0);

// Summarize rows & store in DataSet

			qm2.DataTable.Clear();

			Dictionary<string, object> includedTargets = null;

			if (summaryOptions.TargetsWithActivesOnly)
			{ // scan data & make a list of targets to be included
				includedTargets = new Dictionary<string, object>();
				// ...
			}

			List<UnpivotedAssayResult> tars = new List<UnpivotedAssayResult>(); // build list of TA rows here
			currentCid = "";

			for (int dri = 0; dri <= DataTableMx.Rows.Count; dri++)
			{ 
				if (dri < DataTableMx.Rows.Count)
				{ 
					dr = DataTableMx.Rows[dri];
					cid = dr[KeyValueVoPos] as string;
					dra = GetRowAttributes(dr);
					if (dra != null && dra.Filtered) continue;
					if (currentCid == "") currentCid = cid;
				}
				else dr = null;
				
				if (dr == null || cid != currentCid)
				{ // summarize rows for current cid & add to new datatable 
					if (tars.Count > 0) // 
					{
						List<UnpivotedAssayResult> sumTars = TargetAssayUtil.SummarizeData(
							tars,	
							sumLevel, 
							sumMethod,
							true,
							NullValue.NullNumber,
							NullValue.NullNumber,
							targetMap);

						int voLength2 = qm2.DataTable.Columns.Count;
						foreach (UnpivotedAssayResult sumTar in sumTars)
						{
							object[] vo2 = sumTar.ToValueObject(voLength2, voMap2);
							dr2 = qm2.DataTable.NewRow();
							dr2.ItemArrayRef = vo2; // copy ref for efficiency since vo won't be changed
							qm2.DataTable.Rows.Add(dr2);
						}
					}
					if (dr == null) break;
					tars.Clear(); 
					currentCid = cid;
				}

				UnpivotedAssayResult tar = UnpivotedAssayResult.FromValueObjectNew(dr.ItemArray, voMap);
				tars.Add(tar); // store in form for summarization
			}

			qm2.DataTableManager.InitializeRowAttributes();

			return qm2;
		}

		/// <summary>
		/// Join an unpivoted target-summarized table to itself
		/// </summary>

		public QueryManager BuildTargetTargetData(
			QueryManager qm2)
		{
			QueryTable qt, qt2;
			DataRowMx dr, dr2;
			DataRowAttributes dra, dra2;
			string cid = "", cid2 = "", currentCid, ttKey;
			int rti, rfi;

			qt = Query.GetQueryTableByNameWithException(MultiDbAssayDataNames.BaseTableName); // source table

			UnpivotedAssayResultFieldPositionMap voMap = UnpivotedAssayResultFieldPositionMap.NewOriginalMap(); // used for fast indexing of value by col name
			voMap.InitializeFromQueryTableVoPositions(qt, 0);

			if (qm2 == null || Math.Abs(1) == 1) // need to create query manager? (needs fixup)
			{
				qm2 = new QueryManager();
				qm2 = InitializeSubqueryQm(MultiDbAssayDataNames.TargetTargetUnpivotedTableName);
			}

			Query q2 = qm2.Query;
			qt2 = q2.GetQueryTableByNameWithException(MultiDbAssayDataNames.TargetTargetUnpivotedTableName);

			// Join rows & store in DataSet

			qm2.DataTable.Clear();

			Dictionary<string, int> ttDict = new Dictionary<string, int>(); // maps target pair to row
			Dictionary<string, List<string>> cidTargetDict = new Dictionary<string, List<string>>();

			for (int dri = 0; dri < DataTableMx.Rows.Count; dri++)
			{ // get list of targets for each cid
				dr = DataTableMx.Rows[dri];
				cid = dr[KeyValueVoPos] as string;
				dra = GetRowAttributes(dr);
				if (dra != null && dra.Filtered) continue;
				object[] vo = dr.ItemArrayRef;

				if (!cidTargetDict.ContainsKey(cid))
					cidTargetDict[cid] = new List<string>();

				string target = AssayAttributes.GetStringVo(vo, voMap.TargetSymbol.Voi);
				cidTargetDict[cid].Add(target);
			}

			foreach (string cid0 in cidTargetDict.Keys)
			{ // sum count for pair of targets
				List<string> targets = cidTargetDict[cid0];
				for (int t1i = 0; t1i < targets.Count; t1i++)
				{
					string t1 = targets[t1i];
					for (int t2i = t1i; t2i < targets.Count; t2i++)
					{
						string t2 = targets[t2i];
						if (Lex.Lt(t1, t2)) ttKey = t1 + '\t' + t2;
						else ttKey = t2 + '\t' + t1;

						if (!ttDict.ContainsKey(ttKey))
							ttDict[ttKey] = 0;
						ttDict[ttKey]++;

					}
				}
			}

			foreach (string ttkey0 in ttDict.Keys)
			{ // build data rows
				string[] t1t2 = ttkey0.Split('\t');
				string t1 = t1t2[0];
				string t2 = t1t2[1];
				int cpdCount = ttDict[ttkey0];

				AddRow(qm2.DataTable, t1, t2, cpdCount);
				if (t2 != t1) AddRow(qm2.DataTable, t2, t1, cpdCount);
			}

			qm2.DataTableManager.InitializeRowAttributes();

			return qm2;
		}

		void AddRow(
			DataTableMx dt,
			string t1,
			string t2,
			int cpdCount)
		{
			int voLen = dt.Columns.Count;
			object[] vo = new object[voLen];
			vo[2] = t1;
			vo[3] = t1;
			vo[4] = t2;
			vo[5] = cpdCount;
			DataRowMx dr = dt.NewRow();
			dr.ItemArrayRef = vo; // copy ref for efficiency since vo won't be changed
			dt.Rows.Add(dr);
			return;
		}

/// <summary>
/// Build subquery QueryManager including the DataTableManger,
/// ResultsFormat and ResultsFormatter. 
/// Build empty DataTable
/// </summary>
/// <returns></returns>

		QueryManager InitializeSubqueryQm(
			string tableName)
		{
			Query q2 = null;
			DataTableManager dtm2;
			DataTableMx dt2;
			ResultsFormat rf2;
			ResultsFormatFactory rff;
			ResultsFormatter rfmtr;

			QueryTable qt;
			MetaTable mt;

			QueryManager qm2 = new QueryManager(); // build output query here

			// Setup unpivoted output query

			q2 = new Query(qm2);
			mt = MetaTableCollection.GetWithException(tableName);
			qt = new QueryTable(q2, mt); // be sure proper cols are selected

			dtm2 = new DataTableManager(qm2);
			dt2 = DataTableManager.BuildDataTable(qm2);

			rff = new ResultsFormatFactory(qm2, OutputDest.WinForms);
			rff.Build();

			rfmtr = new ResultsFormatter(qm2);

			return qm2;
		}


/// <summary>
/// Build unpivoted subquery QueryManager including the DataTableManger,
/// ResultsFormat and ResultsFormatter. 
/// Build empty DataTable
/// </summary>
/// <returns></returns>

		QueryManager InitializeUnpivotedSubqueryQm()
		{
			Query q2 = null;
			DataTableManager dtm2;
			DataTableMx dt2;
			ResultsFormat rf2;
			ResultsFormatFactory rff;
			ResultsFormatter rfmtr;

			QueryTable qt;
			MetaTable mt;

			QueryManager qm2 = new QueryManager(); // build output query here

			// Setup unpivoted output query

			q2 = new Query(qm2);
			mt = MetaTableCollection.GetWithException(MultiDbAssayDataNames.CombinedNonSumTableName);
			qt = new QueryTable(q2, mt); // be sure proper cols are selected

			dtm2 = new DataTableManager(qm2);
			dt2 = DataTableManager.BuildDataTable(qm2);

			rff = new ResultsFormatFactory(qm2, OutputDest.WinForms);
			rff.Build();

			rfmtr = new ResultsFormatter(qm2);

			return qm2;
		}

/// <summary>
/// Summarize pivoted data
/// </summary>
/// <param name="sumLevel"></param>
/// <param name="sumMethod"></param>
/// <returns></returns>

		public QueryManager SummarizePivoted(
			TargetAssaySummarizationLevel sumLevel,
			SummarizationType sumMethod,
			ColumnsToTransform colsToSumm,
			OutputDest outputDest,
			TargetMap targetMap)
		{
			QueryManager qm2;
			QueryTable qt;
			DataRow dr, dr2;
			DataRowAttributes dra;
			string cid = "", currentCid;
			int rti, rfi;

			qm2 = InitializeSubqueryQm(MultiDbAssayDataNames.CombinedNonSumTableName);

#if false
// Get the data for a compound, summarize & add results to data for charting 

				DataTableManager dtm = Qm0.DataTableManager;

			for (int dri = 0; dri < dtm.DataTable.Rows.Count; dri++)
			{
				DataRow dr = dtm.DataTable.Rows[dri];
				DataRowAttributes dra = GetRowAttributes(dr);

				string keyVal = dr[KeyValueVoPos] as string;

				if (keyVal != curKeyVal || Rf.Tables.Count <= 1) // going to new key
				{
					curKeyVal = keyVal;
					curKeyRow = dri;
					rowsForKey = 0;
				}

				rowsForKey++;

				object o = dr[dci];
//				if (o is string && ((string)o) == "") o = o; // debug
				if (NullValue.IsNull(o))
				{
					if (rowsForKey == 0 || // count as null if first row for key
					 dra.TableRowState[colInfo.TableIndex] != RowStateEnum.Undefined) // or real row data but col is null
						stats.NullsExist = true;
					continue;
				}

				else if (o is MobiusDataType)
				{ // create a MobiusDataType that we can point to
					o = MobiusDataType.New(o);
					dr[dci] = o;
				}
				MobiusDataType val = o as MobiusDataType;

				try
				{
					if (val.FormattedText == null) // get formatted text if not done yet
						val = QueryManager.ResultsFormatter.FormatField(rt, ti, rfld, fi, dr, dri, val, -1, false);
					dictKey = val.FormattedText.ToUpper();

					if (!stats.DistinctValueDict.ContainsKey(dictKey))
					{
						DistinctValueAndPosition dvp = new DistinctValueAndPosition();
						dvp.Value = val;
						stats.DistinctValueDict[dictKey] = dvp;
					}
				}
				catch (Exception ex) { val = val; }
			} // row loop

				dtm.n
#endif


			return qm2;
		}

/// <summary>
/// Convert a set of pivoted results to unpivoted form
/// </summary>
/// <returns></returns>

		public QueryManager ConvertPivotedToUnpivoted (
			ColumnsToTransform colsToUnpivot)
		{
			QueryManager qm2 = new QueryManager();

			// todo

			return qm2;
		}

/// <summary>
/// Get info on the primary/secondary results columns for each table in the query
/// </summary>
/// <param name="qm0"></param>

		public void GetMainResults(QueryManager qm)
		{
			Query q;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			ResultsTable rt;
			ResultsField rfld;
			int rti, rfi;

			q = qm.Query;

			List<UnpivotedAssayResult> tars = new List<UnpivotedAssayResult>(); // build rows to summarize here

			// Get indexes of data to retrieve for each assay

			List<AssayResultInfo> resInfoList = new List<AssayResultInfo>();

			qt = q.Tables[0];
			mt = qt.MetaTable;
			QueryColumn cidQc = qt.KeyQueryColumn;

			rti = 0;
			if (q.Tables.Count > 1) rti = 1;

			for (rti = rti; rti < qm.ResultsFormat.Tables.Count; rti++)
			{ // define one additional result for each table & try to pick out the key column
				rt = qm.ResultsFormat.Tables[rti];
				qt = rt.QueryTable;
				mt = rt.MetaTable;

				int primary = -1, secondary = -1;
				int firstNumeric = -1, firstString = -1, firstDate = -1;
				for (rfi = 0; rfi < rt.Fields.Count; rfi++)
				{ // select the best candidate
					rfld = rt.Fields[rfi];
					qc = rfld.QueryColumn;
					mc = rfld.MetaColumn;
					if (mc.PrimaryResult) primary = rfi;
					else if (mc.SecondaryResult) secondary = rfi;
					else if (mc.IsNumeric && !mc.IsKey) firstNumeric = rfi;
					else if (mc.DataType == MetaColumnType.String) firstString = rfi;
				}

				if (primary >= 0) rfi = primary;
				else if (secondary >= 0) rfi = secondary;
				else if (firstNumeric >= 0) rfi = firstNumeric;
				else if (firstString >= 0) rfi = firstString;
				else if (firstDate >= 0) rfi = firstDate;
				else continue; // nothing for this table

				AssayResultInfo resInfo = new AssayResultInfo();
				resInfo.Rti = rti;
				resInfo.Rt = rt;
				resInfo.Rfld = rt.Fields[rfi];

				resInfo.ResUnits = "";
				resInfo.ResUnitsVoi = -1;

				resInfo.Conc = "";
				resInfo.ConcVoi = -1;
				resInfo.ConcUnits = "";
				resInfo.ConcUnitsVoi = -1;

				resInfoList.Add(resInfo);

			}

			return;
		}


/// <summary>
/// Summarize by time period
/// </summary>
/// <param name="interval">Date interval to summarize by</param>
/// <param name="breakByCf">Break by conditional formatting</param>
/// <returns></returns>

		public QueryManager SummarizeByTimePeriod(
			DateInterval interval,
			bool breakByCf)
		{
			if (UnpivotedResults) return SummarizeUnpivotedByTimePeriod(interval, breakByCf);
			else return SummarizePivotedByTimePeriod(interval, breakByCf);
		}

		public QueryManager SummarizeUnpivotedByTimePeriod(
			DateInterval interval,
			bool breakByCf)
		{
			QueryManager qm = new QueryManager();
			return qm;
		}

		public QueryManager SummarizePivotedByTimePeriod(
			DateInterval interval,
			bool breakByCf)
		{
			QueryManager qm = new QueryManager();
			return qm;
		}

		/// <summary>
		/// Return true if results are in unpivoted form
		/// </summary>
		/// <returns></returns>

		public bool UnpivotedResults
		{
			get
			{
				MetaTable mt = null;
				if (Query.Tables.Count == 0) return false;
				else if (Query.Tables.Count == 1) mt = Query.Tables[0].MetaTable;
				else if (Query.Tables.Count > 1) mt = Query.Tables[1].MetaTable;

				if (Lex.Eq(mt.Name, MultiDbAssayDataNames.CombinedNonSumTableName)) return true;
				else return false;
			}
		}

/// <summary>
/// Get AssayResultInfo for unpivoted results
/// </summary>
/// <returns></returns>

		List<AssayResultInfo> GetAssayResultInfoFromUnpivotedData()
		{
			List<AssayResultInfo> ariList = new List<AssayResultInfo>();
			return ariList;
		}

/// <summary>
/// Get AssayResultInfo for pivoted results
/// </summary>
/// <returns></returns>

		List<AssayResultInfo> GetAssayResultInfoFromPivotedData()
		{
			List<AssayResultInfo> ariList = new List<AssayResultInfo>();
			return ariList;
		}

		/// <summary>
		/// Info on assay results
		/// </summary>

		class AssayResultInfo
		{
			public int Rti = -1; // results table index
			public int Ti = -1;
			public ResultsTable Rt;
			public ResultsField Rfld;

			public int ResVoi = -1; // index of result
			public string ResUnits = "";
			public int ResUnitsVoi = -1; // vo index if units in data

			public string Conc = ""; // constant concentration
			public int ConcVoi = -1; // index of concentration
			public string ConcUnits = ""; // constant conc units
			public int ConcUnitsVoi = -1; // vo index if units in data
		}

	}

}
