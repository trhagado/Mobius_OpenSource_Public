using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.Data

{
	/// <summary>
	/// Data Aggregation class
	/// Aggregates lists of QueryEngine rows for individual QueryTables containing built in C# data types and MobiusDataType-derived datatypes
	/// </summary>

	public class Aggregator
	{
		public Dictionary<GroupingValues, List<object[]>> GroupByValuesToRows = null; // cumulative maps a set of specific group values to the associated initial data rows
		public Dictionary<int, List<object[]>> GroupIdToGroupRows = new Dictionary<int, List<object[]>>();

		public int GroupIdCount = 0; // used to assign drilldown links

		public static bool IsAvailableForUser
		{
			get
			{
				string userName = ClientState.UserInfo.UserName;
				bool isAggUser = (ClientState.IsDeveloper); // SS.I.IsDeveloper
				return isAggUser;

				//DictionaryMx d = DictionaryMx.Get("SmallWorldUsers");
				//if ((d != null && d.Lookup(userName) != null)) return true; // must be in allowed user list 
				//																														//if (Security.IsAdministrator(SS.I.UserName)) return true; // or a Mobius admin for now
				//else return false;
			}
		}

		/// <summary>
		/// Basic constructor
		/// </summary>

		public Aggregator()
		{
			return;
		}

		/// <summary>
		/// Aggregate a list of QueryEngine rows for a single QueryTable
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="voList"></param>
		/// <returns></returns>

		public List<object[]> AggregateQueryTable(
			int queryEngineInstanceId,
			QueryTable qt,
			List<object[]> voList)
		{
			List<AggColInfo> allAggCols = new List<AggColInfo>();
			List<AggColInfo> groupCols = new List<AggColInfo>();
			List<AggColInfo> statCols = new List<AggColInfo>();

			AggColInfo lastNumberQualifierAci = null;
			GroupingValues gva;
			int voDelta = NullValue.NullNumber;
			object vo, o;
			string s;
			int voi;

			try
			{

				// Setup aggregation column information for each col that produces an aggregate value (really just need once per results set)

				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (!qc.Is_Selected_or_GroupBy_or_Sorted) continue;

					AggregationDef ad = qc.GetAggregation();

					if (ad.Role == AggregationRole.Undefined) // define default role if undefined
						ad.Role = AggregationRole.RowGrouping;

					if (ad.IsGroupingType) // define default grouping type if undefined
					{
						if (ad.GroupingType == GroupingTypeEnum.Undefined)
							ad.GroupingType = GroupingTypeEnum.EqualValues;
					}

					else if (ad.Role == AggregationRole.DataSummary) // define default summary type if undefined
					{
						if (ad.SummaryType == SummaryTypeEnum.Undefined)
							ad.SummaryType = SummaryTypeEnum.Count;
					}

					else // unexpected type, do as grouping
					{
						ad.Role = AggregationRole.RowGrouping;
						ad.GroupingType = GroupingTypeEnum.EqualValues;
					}

					AggColInfo aci = new AggColInfo();
					aci.Qc = qc;
					aci.AggDef = ad;
					MetaColumnType dt = qc.MetaColumn.DataType;

					if (voDelta == NullValue.NullNumber)
						voDelta = -qc.VoPosition;

					aci.Voi = qc.VoPosition + voDelta; // adjust position of col in voa

					if (qc.AggregationSummaryType == SummaryTypeEnum.NumberQualifier)
						lastNumberQualifierAci = aci;

					else
					{
						if (lastNumberQualifierAci != null) // associate any qualifier col
						{
							aci.QualifierAci = lastNumberQualifierAci;
							lastNumberQualifierAci = null;
						}

						if (qc.IsAggregationGroupBy)
							groupCols.Add(aci);

						else
							statCols.Add(aci);

						allAggCols.Add(aci);
					}
				}

				int groupColCnt = groupCols.Count;
				if (groupColCnt == 0) return voList; // no grouping

				// Build a grouping value array (GroupingValues) for all group values for row set 
				// and assign each row to a group

				Dictionary<GroupingValues, List<object[]>> groupByValuesToRows = // maps grouping cols value arrays to list of associated vos for the group
					new Dictionary<GroupingValues, List<object[]>>();

				int voaLength = -1;
				foreach (object[] voa in voList)
				{
					if (voaLength < 0) voaLength = voa.Length; // make sure all data rows are same length
					else if (voaLength != voa.Length) throw new Exception("voaLength(" + voaLength + " ) != voa.Length(" + voa.Length + " )");

					gva = new GroupingValues(groupColCnt);

					for (int gci = 0; gci < groupColCnt; gci++)
					{
						AggColInfo agc = groupCols[gci];
						voi = agc.Voi;
						if (voi < 0 || voi >= voa.Length) throw new Exception("voi(" + voi + " ) not within voa.Length(" + voa.Length + " )");
						vo = voa[voi];

						if (NullValue.IsNull(vo)) vo = null;
						else
						{
							vo = agc.AggTypeDetail.GroupingMethod(agc.Qc, agc.AggDef, vo);
						}

						gva.SetVo(gci, vo);
					}

					if (!groupByValuesToRows.ContainsKey(gva)) // new group?
						groupByValuesToRows[gva] = new List<object[]>();

					groupByValuesToRows[gva].Add(voa); // add to list of rows for group
				}

				// Calc group stats for each column for each group of rows

				List<object[]> voOutList = new List<object[]>();

				// Loop over set of groups calulating aggregated value for each group

				Dictionary<int, List<object[]>> groupIdToGroupRows = // maps group number to list of underlying vos in group for drilldown
					new Dictionary<int, List<object[]>>();

				foreach (KeyValuePair<GroupingValues, List<object[]>> kvp in groupByValuesToRows)
				{
					gva = kvp.Key;
					List<object[]> groupVoList = kvp.Value;
					GroupIdCount++;
					groupIdToGroupRows[GroupIdCount] = groupVoList; // save assoc from group Id number to list of associated group rows for drilldown link

					object[] voaOut = new object[voaLength]; // build output row for this group here

					// Loop over set of columns in current group

					foreach (AggColInfo aci in allAggCols)
					{
						AggregateRowList(aci, groupVoList);

						if (aci.QualifierAci != null && aci.Result is QualifiedNumber) // return qualifier in separate col?
						{
							QualifiedNumber qn = aci.Result as QualifiedNumber;
							voaOut[aci.QualifierAci.Voi] = qn.Qualifier; // store qualifier in separate column
							qn.Qualifier = ""; // clear qualifier for this col
						}

						object r = aci.Result; // assign gr
						MobiusDataType mdt = r as MobiusDataType;
						if (mdt != null)
						{
							mdt.Hyperlink =  // build hyperlink to the group that includes the group and queryengine instance ids
								"http://Mobius/command?ClickFunction DisplayDrilldownDetail " +
								qt.MetaTable.Name + " <AggregationGroupRows> " +
								queryEngineInstanceId + " " + GroupIdCount;
						}
						else r = r; // non-MDT (debug)
						voaOut[aci.Voi] = r; // store result in output voa
					}

					voOutList.Add(voaOut); // include output results row in output list
				}

				// Accumulate grouping information to be used for drilldown operations

				if (GroupByValuesToRows == null) // just copy first time
				{
					GroupByValuesToRows = groupByValuesToRows;
					GroupIdToGroupRows = groupIdToGroupRows;
				}

				else // append subsequent times
				{

					foreach (KeyValuePair<GroupingValues, List<object[]>> kvp in groupByValuesToRows)
					{
						if (GroupByValuesToRows.ContainsKey(kvp.Key))
							throw new Exception("GroupByValuesToRows already contains key: " + kvp.Key);

						GroupByValuesToRows.Add(kvp.Key, kvp.Value);
					}

					foreach (KeyValuePair<int, List<object[]>> kvp in groupIdToGroupRows)
					{
						if (GroupIdToGroupRows.ContainsKey(kvp.Key))
							throw new Exception("GroupIdToGroupRows already contains key: " + kvp.Key);

						GroupIdToGroupRows.Add(kvp.Key, kvp.Value);
					}
				}

				return voOutList;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

		}

		/// <summary>
		/// Aggregate a QueryColumn for the specified set of cols and return aggregated value
		/// </summary>
		/// <param name="queryEngineInstanceId"></param>
		/// <param name="qc"></param>
		/// <param name="binning"></param>
		/// <param name="aggType"></param>
		/// <param name="voList"></param>
		/// <returns></returns>

		public object AggregateQueryColumn(
			int queryEngineInstanceId,
			QueryColumn qc,
			AggregationDef aggDef,
			List<object[]> voList)
		{
			QueryTable qt = qc.QueryTable;
			AggColInfo lastNumberQualifierAci = null;
			int voDelta = 0; // offset to use when getting values from vo
			object r; // summarized result value

			try
			{
				AggColInfo aci = new AggColInfo();
				aci.Qc = qc;
				aci.AggDef = aggDef;

				MetaColumnType dt = qc.MetaColumn.DataType;

				aci.Voi = qc.VoPosition + voDelta; // adjust position of col in voa

				if (qc.AggregationSummaryType == SummaryTypeEnum.NumberQualifier)
				{
					lastNumberQualifierAci = aci;
					aci.QualifierAci = lastNumberQualifierAci;
				}

				else
				{
					if (lastNumberQualifierAci != null) // associate any qualifier col
					{
						aci.QualifierAci = lastNumberQualifierAci;
						lastNumberQualifierAci = null;
					}
				}

				AggregateRowList(aci, voList);
				r = aci.Result; // get result

				MobiusDataType mdt = r as MobiusDataType;
				if (mdt != null)
				{
					mdt.Hyperlink =  // build hyperlink to the group that includes the group and queryengine instance ids
						"http://Mobius/command?ClickFunction DisplayDrilldownDetail " +
						qt.MetaTable.Name + " <AggregationGroupRows> " +
						queryEngineInstanceId + " " + GroupIdCount;
				}

				else r = r; // non-MDT (debug)


				return r;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

		}

		/// <summary>
		/// Aggregate a list of rows performing both summarization and grouping
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		void AggregateRowList(
			AggColInfo aci,
			List<object[]> voList)
		{
			if (aci == null)
				DebugMx.ArgException("AggColInfo is null");

			if (aci.AggTypeDetail == null)
				DebugMx.DataException("AggTypeDetail is null");

			if (aci.AggDef.Role == AggregationRole.DataSummary) // calc summary value
			{

				if (aci.AggTypeDetail.SummarizationMethod == null)
					DebugMx.DataException("SummarizationMethod is null");

				CheckAndAdjustUnpivotedSummaryTypeAsNecessary(aci, voList);
				aci.AggTypeDetail.SummarizationMethod(aci, voList); // call aggregation method
			}

			else // calc grouping value
			{
				if (aci.AggTypeDetail.GroupingMethod == null)
					DebugMx.DataException("GroupingMethod is null");

				if (voList.Count > 0) // just get grouping value for first row since all should be the same
					aci.Result = aci.AggTypeDetail.GroupingMethod(aci.Qc, aci.Qc.Aggregation, voList[0][aci.Voi]);
				else aci.Result = null;
			}

			return;

		}

		/// <summary>
		/// If this is an unpivoted table then check the result type to see if we should do an arithmetic (SP data) or geometric (CRC data) mean
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		void CheckAndAdjustUnpivotedSummaryTypeAsNecessary(
			AggColInfo aci,
			List<object[]> voList)
		{
			AggColInfo aci2 = null;
			QueryColumn qc, qc2;
			DictionaryMx crcRsltTypeDict = null;
			string typeName;
			double d1;
			bool isCrc = false;
			object vo;

			qc = aci.Qc;
			if (qc == null || qc.QueryTable == null) return;
			QueryTable qt = qc.QueryTable;

			if (voList == null || voList.Count == 0) return;

			if (!aci.AggDef.IsMeanSummaryType) return;

			if (aci.AssocResultTypeId == AssocResultTypeId.None) return;
			else if (aci.AssocResultTypeId == AssocResultTypeId.Undefined) 
			{
				if (!Lex.Contains(qt.MetaTableName, "UNPIVOTED"))
				{
					aci.AssocResultTypeId = AssocResultTypeId.None;
					return;
				}

				if ((qc2 = qt.GetQueryColumnByName("SECONDARY_RSLT_TYP_ID")) != null && qc2.Selected)
					aci.AssocResultTypeId = AssocResultTypeId.AssayTypeId;

				else if ((qc2 = qt.GetQueryColumnByName("RSLT_TYP_NM")) != null && qc2.Selected)
					aci.AssocResultTypeId = AssocResultTypeId.AssayTypeName;

				else if ((qc2 = qt.GetQueryColumnByName("RSLT_TYP")) != null && qc2.Selected)
					aci.AssocResultTypeId = AssocResultTypeId.CRCorSP;

				else
				{
					aci.AssocResultTypeId = AssocResultTypeId.None;
					return;
				}
				
				int voDelta = -qt.KeyQueryColumn.VoPosition; // adjust for just this table
				aci.AssocResultTypeVoi = qc2.VoPosition + voDelta;
			}

			// Look at value in current type col & set summary method accordingly

			vo = voList[0][aci.AssocResultTypeVoi];
			if (vo == null) return;

			switch (aci.AssocResultTypeId)
			{
				case AssocResultTypeId.AssayTypeId:

					if (!QualifiedNumber.TryConvertToDouble(vo, out d1)) break;
					string typeIdString = d1.ToString();

					crcRsltTypeDict = DictionaryMx.Get("assay_smrzd_mean_rslt_typ_nm");
					if (crcRsltTypeDict == null) return;
					if (crcRsltTypeDict.LookupWordByDefinition(typeIdString) != null) isCrc = true;
					break;

				case AssocResultTypeId.AssayTypeName:
					typeName = vo.ToString();
					crcRsltTypeDict = DictionaryMx.Get("assay_smrzd_mean_rslt_typ_nm");
					if (crcRsltTypeDict == null) return;
					if (crcRsltTypeDict.LookupDefinition(typeName) != null) isCrc = true;
					return;

				case AssocResultTypeId.CRCorSP:
					typeName = vo.ToString();
					if (Lex.Eq(typeName, "CRC")) isCrc = true;
					break;

				default:
					break;
			}

			if (isCrc)
				aci.AggDef.SummaryType = SummaryTypeEnum.GeometricMean;

			else  // make arithmetic otherwise
				aci.AggDef.SummaryType = SummaryTypeEnum.ArithmeticMean;

			return;
		}

	}

	/// <summary>
	///////////////////////////////////////////
	//// Individual Summarization methods ////
	//////////////////////////////////////////
	/// </summary>

	public class SummarizationMethods
	{

		/// <summary>
		/// Normalize a value within a group such that all values in the group (bin) have the same normalized value
		/// </summary>
		/// <param name="agc"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static object ApplyGroupingOld(
			AggColInfo agc,
			object vo)
		{
			AggregationTypeDetail at = agc.AggTypeDetail;
			if (at == null) return vo; // just return if no subtype

			if (NullValue.IsNull(vo)) return null;

			//if (agc.DataType == MetaColumnType.Date) return DateTimeMx.ApplyDateGrouping(agc, vo);

			else return vo; // todo: handle other tyeps
		}

		/// <summary>
		/// CalculateCount
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateCount(
			AggColInfo aci,
			List<object[]> voList)
		{
			object vo;
			int li, n = 0;

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				if (aci.Voi >= voa.Length || aci.Voi < 0)
					throw new InvalidDataException("Index out of range");
				vo = voa[aci.Voi];

				if (!NullValue.IsNull(vo))
					n++;
			}

			NumberMx nmx = new NumberMx();
			nmx.Value = n;
			aci.Result = nmx;
			return;
		}

		/// <summary>
		/// CalculateCountDistinct
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateCountDistinct(
			AggColInfo aci,
			List<object[]> voList)
		{
			double d;
			string qualifier = null, lastQualifier = null;
			int n = 0; // count in stats
			int nullCnt = 0, nonNullCnt = 0;
			object vo, o;
			string s;
			int li;

			aci.Result = null; // clear result

			MetaColumn mc = aci.Qc.MetaColumn;

			bool isNumeric = mc.IsNumeric;

			HashSet<string> hashSet = new HashSet<string>();

			// Scan values and add to hashSet

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];

				if (NullValue.IsNull(vo))
				{
					nullCnt++;
					continue;
				}

				s = FormatValueSimplified(aci.Qc, vo);
				s = s.ToUpper();
				hashSet.Add(s);
			}

			NumberMx nmx = new NumberMx();
			nmx.Value = hashSet.Count;
			aci.Result = nmx;

			return;
		}

		/// <summary>
		/// Calculate Minimum or Maximum
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateMinMax(
			AggColInfo aci,
			List<object[]> voList)
		{
			object minMax = null;
			int n = 0; // count in stats
			int nullCnt = 0, nonNullCnt = 0;
			object vo, o;
			double d;
			DateTime dt;
			bool calcMin = false, calcMax = false;
			bool isNull;
			string qualifier, s;
			int li;

			if (aci.AggTypeDetail.SummaryTypeId == SummaryTypeEnum.Minimum)
				calcMin = true;

			else if (aci.AggTypeDetail.SummaryTypeId == SummaryTypeEnum.Maximum)
				calcMax = true;

			else throw new Exception("Unexpected type: " + aci.AggTypeDetail.TypeName);

			MetaColumn mc = aci.Qc.MetaColumn;


			aci.Result = null; // clear result

			// Scan values

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];

				if (NullValue.IsNull(vo))
				{
					nullCnt++;
					continue;
				}

				if (minMax == null) minMax = vo;
				else
				{
					int icv = MobiusDataType.Compare(vo, minMax);

					if ((calcMin && icv < 0) || (calcMax && icv > 0))
						minMax = vo;
				}
			}

			aci.Result = minMax;
			return;
		}

		/// <summary>
		/// Calculate the sum of a list of qualified numbers
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateSum(
			AggColInfo aci,
			List<object[]> voList)
		{
			double d;
			string qualifier = null;
			int n = 0, nTested = 0; // count in stats
			int nullCnt = 0, nonNullCnt = 0;
			List<double> vals; // simple number values
			List<double> ltVals; // less than (<) qualified values
			List<double> gtVals; // greater than (>) qualified values

			double sum = 0;
			object vo, vo2;
			string s;
			int li;

			aci.Result = null; // clear result

			MetaColumn mc = aci.Qc.MetaColumn;

			GetNumericValueLists(aci, voList, out vals, out ltVals, out gtVals); // Get lists of values

			n = vals.Count;
			nTested = n + ltVals.Count + gtVals.Count;
			qualifier = "";
			sum = 0;

// If simple unqualified numeric values exist use those to calculate the sum and ignore qualified values

			if (vals.Count > 0)
			{
				foreach (double d0 in vals) // sum well-defined values
					sum += d0; // sum for arithmetic mean
			}

// Only qualified values exist for the group

			else
			{
				n = 0;
				nTested = ltVals.Count + gtVals.Count;
				sum = 0;

				if (ltVals.Count > 0 && gtVals.Count > 0) // less-than (<) and greater-than (>) values 
					sum = NullValue.NullNumber; // no mean value determined

				// Less-than (>) values only

				else if (ltVals.Count > 0) // less-than (<) values only
				{
					foreach (double d0 in ltVals)
						sum += d0;

					qualifier = "<";
					n = 1; // always 1, even if multiple results are exactly the same
				}

				// Greater-than (>) values only

				else if (gtVals.Count > 0)
				{
					foreach (double d0 in gtVals)
						sum += d0;

					qualifier = ">";
					n = 1; // always 1, even if multiple results are exactly the same
				}

				else return; // no result
			}

			// Return result as qualified number

			QualifiedNumber qn = new QualifiedNumber();
			qn.Qualifier = qualifier;
			qn.NumberValue = sum;
			qn.NValue = n;
			qn.NValueTested = nTested;
			qn.StandardDeviation = NullValue.NullNumber;
			aci.Result = qn;

			return;
		}

		/// <summary>
		/// Calculate Arithmetic, Geometric and Result Means
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateMean(
			AggColInfo aci,
			List<object[]> voList)
		{
			double d;
			string qualifier = null;
			int n = 0, nTested = 0; // count in stats
			int nullCnt = 0, nonNullCnt = 0;
			List<double> vals; // simple number values
			List<double> ltVals; // less than (<) qualified values
			List<double> gtVals; // greater than (>) qualified values

			double conc = NullValue.NullNumber;
			string concUnits = "";
			double delta, sum = 0, sumLogs = 0, sumSq = 0, product = 1;
			double arithmeticMean = 0, geometricMean = 0, mean = 0, variance = 0, stdDev = 0;
			double minVal = 0, maxVal = 0;
			double d1;
			int minValCnt = 0, maxValCnt = 0;
			bool almostEqual;

			object vo, vo2;
			string s;
			int li;

			aci.Result = null; // clear result

			MetaColumn mc = aci.Qc.MetaColumn;

			bool calcArithmeticMean = false, calcGeometricMean = false, resultMean = false;

			if (aci.AggTypeDetail.SummaryTypeId == SummaryTypeEnum.ArithmeticMean)
				calcArithmeticMean = true;

			else if (aci.AggTypeDetail.SummaryTypeId == SummaryTypeEnum.GeometricMean)
				calcGeometricMean = true;

			else if (aci.AggTypeDetail.SummaryTypeId == SummaryTypeEnum.ResultMean)
			{
				if (mc.MultiPoint) calcGeometricMean = true;
				else calcArithmeticMean = true;
			}

			else throw new Exception("Not mean type: " + aci.AggTypeDetail.TypeName);

			GetNumericValueLists(aci, voList, out vals, out ltVals, out gtVals); // Get lists of values

			n = vals.Count;
			nTested = n + ltVals.Count + gtVals.Count;
			qualifier = "";

// Handle trivial case of only one non-qualified value

			if (vals.Count == 1)
			{
				mean = vals[0];
					stdDev = NullValue.NullNumber; // no std dev
			}

			// If simple unqualified numeric values exist use those to calculate means and ignore qualified values

			else if (vals.Count > 1)
			{
				if (calcArithmeticMean)
					CalculateArithmeticMean(vals, out mean, out stdDev);

				else if (calcGeometricMean)
					CalculateGeometricMean(vals, out mean, out stdDev);

				else throw new InvalidDataException("Mean type not recognized");
			}

// Only qualified values exist for the group

			else
			{
				n = 0;
				nTested = ltVals.Count + gtVals.Count;
				stdDev = NullValue.NullNumber; // no standard deviation
				sum = 0;

				if (ltVals.Count > 0 && gtVals.Count > 0) // less-than (<) and greater-than (>) values 
				{
					mean = NullValue.NullNumber; // no mean value determined
				}

// Less-than (>) values only

				else if (ltVals.Count > 0) // less-than (<) values only
				{
					foreach (double d0 in ltVals)
					{
						almostEqual = NumberEx.DoublesAlmostEqual(d0, minVal);
						if (minValCnt == 0 || (!almostEqual && d0 < minVal))
						{
							minVal = d0;
							minValCnt = 1;
						}

						else if (almostEqual)
							minValCnt++;
					}

					qualifier = "<";
					n = 1; // always 1, even if multiple results are exactly the same
					mean = minVal;
				}

// Greater-than (>) values only

				else if (gtVals.Count > 0)
				{
					foreach (double d0 in gtVals)
					{
						almostEqual = NumberEx.DoublesAlmostEqual(d0, maxVal);
						if (maxValCnt == 0 || (!almostEqual && d0 > maxVal))
						{
							maxVal = d0;
							maxValCnt = 1;
						}

						else if (almostEqual)
							maxValCnt++;
					}

					qualifier = ">";
					n = 1; // always 1, even if multiple results are exactly the same
					mean = maxVal;
				}

				else return; // no result
			}

			// Return result as qualified number

			QualifiedNumber qn = new QualifiedNumber();
			qn.Qualifier = qualifier;
			qn.NumberValue = mean;
			qn.NValue = n;
			qn.NValueTested = nTested;
			qn.StandardDeviation = stdDev;
			aci.Result = qn;

			return;
		}

		/// <summary>
		/// Calculate Geometric Mean
		/// </summary>
		/// <param name="vals"></param>
		/// <param name="mean"></param>
		/// <param name="stdDev"></param>

		static void CalculateArithmeticMean(
			List<double> vals,
			out double mean,
			out double stdDev)
		{
			double delta, sum = 0, sumLogs = 0, sumSq = 0, variance = 0;
			double d1;

			mean = 0;
			stdDev = 0;
			int n = vals.Count;

			for (int vi = 0; vi < vals.Count; vi++) // sum list of values
			{
				double d0 = vals[vi];
				sum += d0; // sum for arithmetic mean
			}

			mean = sum / n;

			sumSq = 0; // calc standard deviation
			for (int vi = 0; vi < n; vi++)
			{
				delta = vals[vi] - mean;
				sumSq += (delta * delta);
			}

			variance = sumSq / (n - 1); // "Sample" standard deviation uses n - 1, Population uses n
			stdDev = Math.Sqrt(variance);
		}

		/// <summary>
		/// Calculate Geometric Mean
		/// Note that the SD value differs from the "Standard" method
		/// </summary>
		/// <param name="vals"></param>
		/// <param name="geoMean"></param>
		/// <param name="geoSD"></param>

		static void CalculateGeometricMean(
			List<double> vals,
			out double geoMean,
			out double geoSD)
		{
			double sum = 0, sumLogs = 0, sumSq = 0, product = 1, variance = 0, arithMean = 0, arithSD = 0;
			double d1, delta;

			geoMean = 0;
			geoSD = 0;
			int n = vals.Count;
			//n = 1; // debug

			for (int vi = 0; vi < n; vi++) // sum list of values
			{
				d1 = Math.Log10(vals[vi]);
				sumLogs += d1; // sum of log values 
				vals[vi] = d1; // replace raw values with log-10 transformed values for later use
			}

			arithMean = sumLogs / n;
			geoMean = Math.Pow(10, arithMean);

			sumSq = 0; // calc standard deviation
			for (int vi = 0; vi < n; vi++)
			{
				delta = vals[vi] - arithMean;
				sumSq += Math.Pow(delta, 2);
			}

			variance = sumSq / (double)(n - 1); // "Sample" standard deviation uses n - 1, "Population" uses n
			arithSD = Math.Sqrt(variance);

			geoSD = geoMean * arithSD * Math.Log(10);

			return;
		}

		/// <summary>
		/// Calculate Geometric Mean
		/// "Standard" geometric mean calculation method. SD is calculated in the common way but using the geo mean
		/// </summary>
		/// <param name="vals"></param>
		/// <param name="mean"></param>
		/// <param name="stdDev"></param>

		static void CalculateGeometricMean2(
			List<double> vals,
			out double mean,
			out double stdDev)
		{
			double sum = 0, sumLogs = 0, sumSq = 0, product = 1, variance = 0;
			double d1, delta;

			//double aMean, aSD; // debug
			//CalculateArithmeticMean(vals, out aMean, out aSD); 

			mean = 0;
			stdDev = 0;
			int n = vals.Count;
			//n = 1; // debug

			for (int vi = 0; vi < n; vi++) // sum list of values
			{
				d1 = vals[vi];
				sumLogs += Math.Log10(d1); // sum of log values geometric mean
 			  //product *= d1; // product for geometric mean (less accurate)
			}

			mean = Math.Pow(10, (sumLogs / n));
			//mean = Math.Pow(product, (1.0 / n)); // product for geometric mean (less accurate)

			sumSq = 0; // calc standard deviation
			for (int vi = 0; vi < n; vi++)
			{
				delta = vals[vi] - mean;
				sumSq += Math.Pow(delta, 2);//  * delta);
			}

			variance = sumSq / (double)(n - 1); // "Sample" standard deviation uses n - 1, "Population" uses n
			stdDev = Math.Sqrt(variance);

			//sum = 0; // calc geo mean standard deviation (poor results)
			//foreach (double d0 in vals) // sum of squares
			//{
			//	d1 = Math.Log(d0 / mean);
			//	sum += d1 * d1;
			//}
			//stdDev = Math.Exp(Math.Sqrt(sum / (n- 1)));

			return;
		}

		/// <summary>
		/// Get qualified/unqualified number lists
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>
		/// <param name="vals"></param>
		/// <param name="ltVals"></param>
		/// <param name="gtVals"></param>

		static void GetNumericValueLists(
			AggColInfo aci,
			List<object[]> voList,
			out List<double> vals,
			out List<double> ltVals,
			out List<double> gtVals)
		{
			object[] voa;
			object vo = null, vo2 = null;
			double d;
			string qualifier;
			int li, nullCnt = 0;

			vals = new List<double>(voList.Count); // simple number values
			ltVals = new List<double>(voList.Count); // less than (<) qualified values
			gtVals = new List<double>(voList.Count); // greater than (>) qualified values

			for (li = 0; li < voList.Count; li++)
			{
				voa = voList[li];
				vo = voa[aci.Voi];

				bool isNull = !QualifiedNumber.TryConvertToQualifiedDouble(vo, out d, out qualifier);

				if (isNull)
				{
					nullCnt++;
					continue;
				}

				if (aci.QualifierAci != null && String.IsNullOrEmpty(qualifier)) // can qualifier come from other column?
				{
					vo2 = voa[aci.QualifierAci.Voi]; // get potential qualifier
					bool isQualifier = QualifiedNumber.TryConvertToQualifier(vo2, out qualifier);
				}

				if (String.IsNullOrEmpty(qualifier))
				 vals.Add(d);

				else if (qualifier == "<")
					ltVals.Add(d);

				else if (qualifier == ">")
					gtVals.Add(d);
			}
		}


		/// <summary>
		/// CalculateMostPotent
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateMostPotent(
			AggColInfo aci,
			List<object[]> voList)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// CalculateMedian
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateMedian(
			AggColInfo aci,
			List<object[]> voList)
		{
			int nullCnt = 0, nonNullCnt = 0;
			object[] vals = new object[voList.Count];

			object median = null;

			object vo, o;
			string s;
			int li;

			aci.Result = null; // clear result

			MetaColumn mc = aci.Qc.MetaColumn;

			// Get list of values

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];

				if (NullValue.IsNull(vo))
				{
					nullCnt++;
					continue;
				}

				else
				{
					vals[nonNullCnt] = vo;
					nonNullCnt++;
				}
			}

			if (nonNullCnt == 0) return; // no non-null data

			IComparer<object> comparer = (IComparer<object>)new SingleColumnSortComparer();
			Array.Sort(vals, 0, nonNullCnt, comparer);

			median = vals[nonNullCnt / 2];
			MobiusDataType mdt = MobiusDataType.New(mc.DataType, median);
			aci.Result = mdt;

			return;
		}

		/// <summary>
		/// CalculateMode
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateMode(
			AggColInfo aci,
			List<object[]> voList)
		{
			int n = 0; // count in stats
			int nullCnt = 0, nonNullCnt = 0;
			SortedDictionary<IComparable, int> dict = // use sorted dictionary so IComparable is used for key comparison
				new SortedDictionary<IComparable, int>();

			object mode = null;
			MobiusDataType mdt = null;

			object vo, o;
			string s;
			int li;

			aci.Result = null; // clear result

			MetaColumn mc = aci.Qc.MetaColumn;

// Get list of values

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];
				if (NullValue.IsNull(vo))
				{
					nullCnt++;
					continue;
				}

				else
				{
					IComparable ic = vo as IComparable;
					if (ic == null) continue; // skip if not comparable


					if (!dict.ContainsKey(ic))
						dict[ic] = 0;

					dict[ic]++;

					nonNullCnt++;
				}
			}

			if (nonNullCnt == 0) return; // no non-null data

			int max = 0;
			foreach (KeyValuePair<IComparable, int> kv in dict)
			{
				if (kv.Value > max)
				{
					mode = kv.Key;
					max = kv.Value;
				}
			}

			// Convert mode value to QualifiedNumber so count can be included

			if (MobiusDataType.TryConvertTo(mc.DataType, mode, out mdt))
			{
				aci.Result = mdt;
			}

			else // return as is if can't convert
			{
				aci.Result = mode;
			}

			return;
		}

		/// <summary>
		/// CalculateSingleValue
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateSingleValue(
			AggColInfo aci,
			List<object[]> voList)
		{
			throw new NotImplementedException();

#if false // not currently used
			object singleVal = null;
			int n = 0; // count in stats
			int nullCnt = 0, nonNullCnt = 0;
			object vo, o;
			bool isNull;
			int li;

			MetaColumn mc = aci.Qc.MetaColumn;

			aci.Result = null; // clear result

			// Scan values

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];

				if (NullValue.IsNull(vo))
				{
					nullCnt++;
					continue;
				}

				if (singleVal == null) singleVal = vo;
				else
				{
					int icv = MobiusDataType.Compare(vo, singleVal);
					if (icv != 0) return; // if different value then return null
				}
			}

			aci.Result = singleVal;
			return;
#endif
		}

		/// <summary>
		/// Build a string containing the list of values in the group
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateConcatenate(
			AggColInfo aci,
			List<object[]> voList)
		{
			int nullCnt = 0, nonNullCnt = 0;
			HashSet<string> valSet = new HashSet<string>();
			object[] vals = new object[voList.Count];

			object vo, o;
			string s, s2;
			int li;

			bool distinct = (aci.AggTypeDetail.SummaryTypeId == SummaryTypeEnum.ConcatenateDistinct);

			aci.Result = null; // clear result
			StringBuilder sb = new StringBuilder();

			MetaColumn mc = aci.Qc.MetaColumn;

			// Get list of non-null values

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];
				if (NullValue.IsNull(vo))
				{
					nullCnt++;
					continue;
				}

				else
				{
					vals[nonNullCnt] = vo;
					nonNullCnt++;
				}
			}

			IComparer<object> comparer = (IComparer<object>)new SingleColumnSortComparer();
			Array.Sort(vals, 0, nonNullCnt, comparer); // sort values

			for (li = 0; li < nonNullCnt; li++)
			{
				vo = vals[li];
				s = FormatValueSimplified(aci.Qc, vo);

				if (distinct) // keep distinct values only?
				{
					s2 = s.ToUpper();
					if (valSet.Contains(s2)) continue;

					valSet.Add(s2);
				}

				if (sb.Length > 0) sb.Append(", ");
				sb.Append(s);
			}

			s = sb.ToString();
			StringMx smx = new StringMx(s);
			aci.Result = smx;

			return;
		}

		/// <summary>
		/// Format a simplified value (i.e. no stats for Qualified numbers
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="o"></param>
		/// <returns></returns>

		public static string FormatValueSimplified(
			QueryColumn qc,
			object o)
		{
			MobiusDataType mdt = null;
			string s;

			MetaColumn mc = qc.MetaColumn;
			if (NullValue.IsNull(o)) return "";

			if (MobiusDataType.TryConvertTo(mc.DataType, o, out mdt))
				s = mdt.ToString();

			else s = o.ToString(); // if fails just convert to string

			return s;
		}


		/// <summary>
		/// Calculate Qualifier - This is a noop.
		/// The qualifier is factored into the calculation for the associated numeric value column
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateQualifier(
			AggColInfo aci,
			List<object[]> voList)
		{
			return;
		}

	} // SummarizationMethods

	public class GroupingMethods
	{
		/// <summary>
		/// CalculateGroup
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="voList"></param>

		public static void CalculateGroupByOld(
			AggColInfo aci,
			List<object[]> voList)
		{
			object vo, o;
			string s;
			int li;

			aci.Result = null;

			for (li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				vo = voa[aci.Voi];
				if (NullValue.IsNull(vo)) continue; // just in case...

				aci.Result = vo; // pick first non-null result
				return; // return after we have a value
			}

			return;
		}

/// <summary>
/// Equal Values
/// </summary>
/// <param name="aci"></param>
/// <param name="vo"></param>

		public static object EqualValues(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			object o;
			string s;
			int li;

			o = vo; // just use value as is

			return o;
		}

/// <summary>
/// First Letter
/// </summary>
/// <param name="aci"></param>
/// <param name="vo"></param>

		public static object FirstLetter(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			if (vo == null) return vo;

			string s = vo.ToString();
			if (s.Length > 0)
			s = Char.ToUpper(s[0]).ToString();

			return s;
		}

		/// <summary>
		/// Numeric Interval
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object NumericInterval(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			double dVal;
			string qualifier;

			if (!QualifiedNumber.TryConvertToQualifiedDouble(vo, out dVal, out qualifier)) return vo;

			Int64 bin = (Int64)(dVal / (double)ad.NumericIntervalSize);
			return bin;
		}

		/// <summary>
		/// By Date - Ignore time
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object Date(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			DateTime dt = GetDateTime(vo);
			DateTime dt2 = new DateTime(dt.Year, dt.Month, dt.Day);
			return dt2;
		}

		/// <summary>
		/// Date Month, Year
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object DateMonthYear(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			DateTime dt = GetDateTime(vo);
			DateTime dt2 = new DateTime(dt.Year, dt.Month, 1);
			return dt2;
		}

		/// <summary>
		/// Date Quarter, Year
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object DateQuarterYear(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			DateTime dt = GetDateTime(vo);
			int fmoq = ((dt.Month - 1) / 3) * 3 + 1; // convert to first month of quarter 
			DateTime dt2 = new DateTime(dt.Year, fmoq, 1);
			return dt2;
		}

		/// <summary>
		/// Date Year
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object DateYear(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			DateTime dt = GetDateTime(vo);
			DateTime dt2 = new DateTime(dt.Year, 1, 1);
			return dt2;
		}

		/// <summary>
		/// Date Month
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object DateMonth(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			DateTime dt = GetDateTime(vo);
			DateTime dt2 = new DateTime(0, dt.Month, 1);
			return dt2;
		}

		/// <summary>
		/// Date Month
		/// </summary>
		/// <param name="aci"></param>
		/// <param name="vo"></param>

		public static object DateQuarter(
			QueryColumn qc,
			AggregationDef ad,
			object vo)
		{
			DateTime dt = GetDateTime(vo);
			int fmoq = ((dt.Month - 1) / 3) * 3 + 1; // convert to first month of quarter 
			DateTime dt2 = new DateTime(0, fmoq, 1);
			return dt2;
		}

		/// <summary>
		/// Convert a vo to a DateTime
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static DateTime GetDateTime (
			object vo)
		{
			if (vo == null) return DateTime.MinValue;

			else if (vo is DateTime) return (DateTime)vo;

			else if (vo is Int64) return DateTime.FromBinary((Int64)vo);

			else if (vo is DateTimeMx)
			{
				DateTimeMx dtMx = (DateTimeMx)vo;
				return dtMx.Value;
			}

			else return DateTime.MinValue; // shouldn't happpen
		}

		/// <summary>
		/// Format a grouped value for display
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="agg"></param>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static string FormatGroupedValue(
			QueryColumn qc,
			AggregationDef agg,
			object vo)
		{
			QualifiedNumber qn;
			double d1;
			string txt = "";

			if (qc == null || qc.MetaColumn == null) return null;
			MetaColumn mc = qc.MetaColumn;

			if (agg == null) return null;
			if (!agg.IsGroupingType) return null;
			GroupingTypeEnum groupingType = agg.GroupingType;

			AggregationTypeDetail atd = agg.TypeDetail;
			if (atd == null) return null;

			if (vo == null) return null;
			else if (NullValue.IsNull(vo)) return null;

			if (mc.DataType == MetaColumnType.CompoundId)
			{
				txt = CompoundId.Format(vo.ToString());
			}

			// Numeric value

			else if (mc.IsNumeric)
			{
				ColumnFormatEnum displayFormat = qc.ActiveDisplayFormat;
				int decimals = qc.ActiveDecimals;

				// Simple numeric value

				bool formatAsSimpleNumber = (groupingType != GroupingTypeEnum.NumericInterval);
				if (formatAsSimpleNumber)
				{
					if (!QualifiedNumber.TryConvertTo(vo, out qn)) return vo.ToString();

					if (mc.DataType == MetaColumnType.Integer)
					{
						displayFormat = ColumnFormatEnum.Decimal;
						decimals = 1;
					}

					txt = qn.Format(qc, false, displayFormat, decimals, QnfEnum.Combined | QnfEnum.NValue, OutputDest.WinForms);
				}

				// Interval value

				else if (groupingType == GroupingTypeEnum.NumericInterval)
				{
					if (vo.GetType() != typeof(Int64)) return vo.ToString();
					Int64 bin = Convert.ToInt64(vo);

					Decimal width = agg.NumericIntervalSize;
					Decimal dLow = width * bin;
					Decimal dHigh = width * (bin + 1);

					if (width == 1) // bins are all of size 1
					{
						txt = dLow.ToString();
					}

					else if (Decimal.Truncate(width) == width) // whole number (non-fractional) bins
					{
						txt = dLow.ToString() + " - " + (dHigh - 1).ToString();
					}

					else // fractional-size bins, format: 10 <= x < 20
					{
						txt = dLow.ToString() + " \u2264 x < " + dHigh.ToString();
					}
				}

				else txt = vo.ToString();
			}

			// Date type

			else if (mc.DataType == MetaColumnType.Date)
			{
				DateTime dt = GroupingMethods.GetDateTime(vo);
				txt = DateTimeMx.Format(dt, atd.FormatString);
			}

			// First Letter

			else if (groupingType == GroupingTypeEnum.FirstLetter)
			{
				txt = vo.ToString();
				if (txt.Length > 0) txt = txt[0].ToString();
			}

			// Default to ToString()

			else txt = vo.ToString();

			return txt;
		}


	}

	/// <summary>
	/// GroupingValues Class
	/// 
	/// This class is used to store and compare arrays of grouping column values
	/// for results rows to determine group membership for each row for aggregation.
	/// This class implements the Object.GetHashCode and Object.Equals methods so
	/// it can be used as key values in a Dictionary. It calls lower level 
	/// GetHashCode and Equals methods so these must be implemented for all
	/// MobiusDataType classes and for any other types that can appear in
	/// results value object arrays (VOAs).
	/// </summary>

	public class GroupingValues : IComparable
	{
		object Vo = null; // vo used if single column in groupBy
		object[] Voa = null; // array of group values for a data row

		static bool ForceVoaUseAlways = false; // for debug

/// <summary>
/// Constructor
/// </summary>
/// <param name="size"></param>

		public GroupingValues(int size)
		{
			if (size > 1 || ForceVoaUseAlways)
				Voa = new object[size];
			return;
		}

		/// <summary>
		/// SetVo
		/// </summary>
		/// <param name="idx"></param>
		/// <param name="vo"></param>

		public void SetVo(
			int idx,
			object vo)
		{
			if (Voa != null) Voa[idx] = vo;
			else Vo = vo;
		}

		public object GetVo(int idx)
		{
			if (Voa != null) return Voa[idx];
			else return Vo;
		}

		/// <summary>
		/// Get hash value for combined group values
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			if (Vo != null) return Vo.GetHashCode(); // single value

			else if (Voa != null) // array of values
			{
				int hash = 13;

				for (int voi = 0; voi < Voa.Length; voi++)
				{
					object vo = Voa[voi];
					unchecked { hash = (hash * 7) + (vo != null ? vo.GetHashCode() : 0); }
				}
				return hash;
			}

			else return 0; // single null value
		}

		/// <summary>
		/// Compare this GroupingValues and another for equality
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public override bool Equals(object obj)
		{
			GroupingValues gv2 = obj as GroupingValues;
			if (gv2 == null) return false; // null arg?

			if (Vo != null) return Vo.Equals(gv2.Vo); // single value group

			else if (Voa != null) // array of values
			{
				for (int voi = 0; voi < Voa.Length; voi++)
				{
					object vo = Voa[voi];
					object vo2 = gv2.Voa[voi];
					if (Object.ReferenceEquals(vo, vo2)) continue; // continue if same object or two nulls
					else if (vo == null) return false; // vo null but vo2 is not
					else if (!vo.Equals(vo2)) return false;
				}

				return true;
			}

			else // "this" must be a single column with a null value
			{
				if (gv2.Vo != null) return false; // if arg isn't a single column null value then not equal
				else return true;
			}
		}

		/// <summary>
		/// Compare two values (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public int CompareTo(object o)
		{
			GroupingValues gva1 = this;
			object[] voa1 = gva1.Voa;

			GroupingValues gva2 = o as GroupingValues;
			object[] voa2 = gva2.Voa;

			for (int voi = 0; voi < Voa.Length; voi++)
			{
				IComparable c1 = voa1[voi] as IComparable;
				IComparable c2 = voa2[voi] as IComparable;

				if (c2 == null) return 1; // say not null c1 is greater than null c2 (by definition)

				else
				{
					int iv = c1.CompareTo(c2); // compare current array element
					if (iv != 0) return iv; // return on first difference
				}
			}

			return 0; // equal if all elements are equal
		}

	}

	/// <summary>
	/// Information for each column to be included in the aggregation
	/// </summary>

	public class AggColInfo
	{
		public QueryColumn Qc;
		public int Voi; // index into Vo array
		public AggregationDef AggDef;

		public int AssocResultTypeVoi = -1; // if summarized role from unpivoted table then info on associated col with result type info
		public AssocResultTypeId AssocResultTypeId = AssocResultTypeId.Undefined;

		public AggColInfo QualifierAci; // ACI for any associated qualifier column

		public object Result = null; // result of calculation goes here

		public AggregationTypeDetail AggTypeDetail
		{
			get
			{
				if (AggDef == null) return null;
				else return AggDef.TypeDetail;
			}
		}

	}

/// <summary>
/// Type of associated column to use to determine if a value is a CRC value that 
/// </summary>

	public enum AssocResultTypeId
	{
		Undefined = 0,
		None = 1,
		AssayTypeId = 2, // ASSAY type id from assay metadata
		AssayTypeName = 3, // ASSAY type name from assay metadata
		CRCorSP = 4 // "CRC" or "SP" string value in column
	}

	/// <summary>
	/// Delegate for performing a summarization of a value object list for a QueryColumn
	/// </summary>
	/// <param name="aci"></param>
	/// <param name="voList"></param>

	public delegate void SummarizationMethodDelegate(
		AggColInfo aci,
		List<object[]> voList);

	/// <summary>
	/// Delegate for assigning a group to a value for a QueryColumn for aggregation purposes
	/// </summary>
	/// <param name="aci"></param>
	/// <param name="voList"></param>

	public delegate object GroupingMethodDelegate(
		QueryColumn qc,
		AggregationDef ad,
		object vo);


}
