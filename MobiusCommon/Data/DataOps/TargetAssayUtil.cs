using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Mobius.Data
{
/// <summary>
/// Utility methods for Target-Assay data
/// </summary>

	public class TargetAssayUtil
	{

/// <summary>
/// Create a new pivoted DataTable in the specified format from the existing unpivoted DataTable
/// </summary>
/// <param name="rows"></param>
/// <param name="pivotType"></param>
/// <param name="qt"></param>
/// <param name="newQuery"></param>
/// <param name="ResultKeys"></param>

		void CreatePivotedTable (
			DataSet rows,
			TargetAssayPivotFormat pivotType,
			QueryTable qt,
			Query newQuery,
			List<string> ResultKeys)
		{
#if false			
			UnpivotedAssayResult rr;
			QueryColumn qc, qc2;
			MetaColumn mc, mc2;
			string pivotValue;
			int row, pivotPos, pos, colsPerPivotValue, qci, mci, pi, ri;

			bool pivotByTarget = false, pivotByFamily = false, orderByFamily = false;
			if (pivotType == TargetAssayPivotFormat.
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
			GetSummarizationParameterValues(qt);
			SubQe = PrepareAndOpenAssayDataSubquery(qe.Query, qt, ResultKeys);

			List<UnpivotedAssayResult> rrList = new List<UnpivotedAssayResult>(); // list of results summarized by target & result type
			List<string> pivotList = new List<string>(); // list of pivot values to use in header labels
			List<Dictionary<string, int>> pivotVoiList = new List<Dictionary<string, int>>(); // mapping of col names to vo position for each pivot
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
					pivotValue += rr.TargetSymbol;
				}
				else // header is family and result type
					pivotValue = rr.GeneFamily + " - " + rr.ResultType;

				if (!pivotDict.ContainsKey(pivotValue.ToUpper()))
				{
					pivotList.Add(pivotValue);
					pivotVoiList.Add(new Dictionary<string, int>());
					pivotDict[pivotValue.ToUpper()] = pivotDict.Count; // store position
				}
			}

			if (rrList.Count == 0) return;

			// Build pivoted metatable with prototype cols

			MetaTable mt3 = mt.Clone();
			mt3.Name += "_2"; // different name for derived table
			mt3.RemapForRetrieval = false; // don't attempt to remap further
			mt3.MetaColumns.Clear();

			for (qci = 0; qci < qt.QueryColumns.Count; qci++)
			{ // get list of columns along with key
				qc = qt.QueryColumns[qci];
				mc = qc.MetaColumn;
				if (!qc.Selected_or_Sorted) continue;
				if (Lex.Eq(mc.Name, "gene_fmly")) continue; // goes to header
				if (pivotByTarget && Lex.Eq(mc.Name, "gene_symbl")) continue; // goes to header if pivoting by target
				if (pivotByFamily && Lex.Eq(mc.Name, "rslt_typ")) continue; // goes to header if pivoting by family

				mt3.AddMetaColumn(qc.MetaColumn.Clone());
			}

			// Build column labels

			colsPerPivotValue = mt3.MetaColumns.Count - 1; // no. of cols to duplicate (don't include key)
			for (pi = 0; pi < pivotList.Count; pi++)
			{
				Dictionary<string, int> voi = pivotVoiList[pi];
				for (mci = 1; mci < colsPerPivotValue + 1; mci++)
				{ // duplicate cols for each target & modify names & labels
					MetaColumn modelMc = mt3.MetaColumns[mci];
					MetaColumn mc3 = modelMc.Clone();
					mc3.Name += "_" + pi;

					if (Lex.Eq(modelMc.Name, "gene_symbl")) mc3.Label = pivotList[pi] + " Gene"; // complete gene-family header
					else if (Lex.Eq(modelMc.Name, "rslt_val")) mc3.Label = pivotList[pi] + " Result"; // complete result header
					// (don't qualify for other cols) else mc3.Label = pivotList[pi] + " - " + mc3.Label; // other results with label
					mt3.AddMetaColumn(mc3);
					voi[modelMc.Name] = mt3.MetaColumns.Count - colsPerPivotValue - 1; // vo position for this column
				}
			}

			mt3.MetaColumns.RemoveRange(1, colsPerPivotValue); // remove prototype cols
			MetaTableCollection.Add(mt3); // update map
			QueryTable qt3 = new QueryTable(newQuery, mt3);
			qt3.SelectAll();
#endif
		}


/// <summary>
/// Calculate summarized value for a compound, target or assay, result type, assay type group of results
/// </summary>
/// <param name="rows">Data to summarize</param>
/// <param name="sumLevel">Summarization level, Assay or Target</param>
/// <param name="sumMethod">Summarization method</param>
/// <returns></returns>

		public static List<UnpivotedAssayResult> SummarizeData(
			List<UnpivotedAssayResult> rows,
			TargetAssaySummarizationLevel sumLevel,
			SummarizationType sumMethod,
			bool ignoreSecondaryResultIfPrimaryResult,
			double minSPToInclude,
			double maxCRCToInclude,
			TargetMap targetMap)
		{
			List<UnpivotedAssayResult> sumRows = SummarizeData(
				rows,
				sumLevel,
				sumMethod,
				ignoreSecondaryResultIfPrimaryResult,
				minSPToInclude,
				maxCRCToInclude,
				targetMap,
				false, // includeResultDetail,
				false, // useCatenatedResultValueFormat,
				QnfEnum.Combined, // qnFormat,
				false); // formatForSpotfire

			return sumRows;
		}

/// <summary>
/// Calculate summarized value for a compound, target or assay, result type, assay type group of results
/// </summary>
/// <param name="rows">Data to summarize</param>
/// <param name="sumLevel">Summarization level, Assay or Target</param>
/// <param name="sumMethod">Summarization method</param>
/// <param name="ignoreSecondaryResultIfPrimaryResult">Ignore secondary result if primary result exists</param>
/// <param name="minSPToInclude">Minimum SP value to include</param>
/// <param name="maxCRCToInclude">Maximum CRC value to include</param>
/// <param name="targetMap">Target map to map summarized results onto if any</param>
/// <param name="includeResultDetail"></param>
/// <param name="useCatenatedResultValueFormat"></param>
/// <param name="qnFormat"></param>
/// <param name="formatForSpotfire"></param>
/// <returns></returns>

		public static List<UnpivotedAssayResult> SummarizeData(
			List<UnpivotedAssayResult> rows,
			TargetAssaySummarizationLevel sumLevel,
			SummarizationType sumMethod,
			bool ignoreSecondaryResultIfPrimaryResult,
			double minSPToInclude,
			double maxCRCToInclude,
			TargetMap targetMap,
			bool includeResultDetail,
			bool useCatenatedResultValueFormat,
			QnfEnum qnFormat,
			bool formatForSpotfire)
		{
			TargetMapCoords tmc;
			UnpivotedAssayResult lastSummaryRow = null;
			string targetFamily = null;
			string currentCid = "", sumKey;
			int currentCidPos = -1;
			double mean, sumSq, variance;
			int tci;
			bool meanSummarization;
			bool byTarget = false, byAssay = false;

			if (sumLevel == TargetAssaySummarizationLevel.Target) byTarget = true;
			else if (sumLevel == TargetAssaySummarizationLevel.Assay) byAssay = true;
			else throw new Exception("Invalid summarization level: " + sumLevel);

			if (sumMethod == SummarizationType.BioResponseMean)
				meanSummarization = true;
			else if (sumMethod == SummarizationType.MostPotent)
				meanSummarization = false;
			else throw new Exception("Unsupported summarization type: " + sumMethod);

			List<UnpivotedAssayResult> results = new List<UnpivotedAssayResult>(); // results are built here

			//object[] sVo = null, vo = null;
			UnpivotedAssayResult sRow = null, row = null, rr2 = null;

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

			MetaTable mt = MetaTableCollection.GetWithException(MultiDbAssayDataNames.CombinedNonSumTableName);
			MetaColumn resultValMc = mt.GetMetaColumnByNameWithException("rslt_val");
			QueryColumn resultValQc = new QueryColumn();
			resultValQc.MetaColumn = resultValMc;
			MetaColumn concValMc = mt.GetMetaColumnByNameWithException("conc_val");

			string resultDetail = "";
			QualifiedNumber fmtQn = new QualifiedNumber();
			Dictionary<string, int> subVoi = null;

			string fontTag = "", fontTag2 = "";
			string breakTag = "\r\n";

			if (formatForSpotfire)
			{ // if going to Spotfire format as html with red highlight of basic result value
				fontTag = "<font color=\"#FF0000\">";
				fontTag2 = "</font>";
				breakTag = "<br>";
			}

			Dictionary<string, List<UnpivotedAssayResult>> resultGroups = new Dictionary<string, List<UnpivotedAssayResult>>();

			int ri = 0;
			bool eof = false;
			while (true) // loop till all values for a compoundid or end of data
			{
				if (ri < rows.Count)
				{
					row = rows[ri];
					if (currentCid == "") currentCid = row.CompoundId;
					ri++;
				}
				else eof = true;

				if (!eof && row.CompoundId == currentCid)
				{
					row.NormalizeValues();
					if (row.ResultNumericValue == NullValue.NullNumber) continue; // skip if no numeric value
					if (Lex.Eq(row.AssayType, "FUNCTIONAL") && Lex.Eq(row.GeneFamily, "Kinase"))
						continue; // always skip these since modulated & monitored protein may differ

					if (byTarget) sumKey = row.GeneSymbol.ToUpper();
					else sumKey = row.AssayName; // key on assay name (if possibility of dup names key on MetaTable name)
					if (Lex.IsNullOrEmpty(sumKey)) sumKey = "Unknown";
					sumKey = sumKey.ToUpper();
					if (!resultGroups.ContainsKey(sumKey))
						resultGroups[sumKey] = new List<UnpivotedAssayResult>();

					resultGroups[sumKey].Add(row);
				}

				else
				{
					if (!eof) ri--; // backup to process this row next time through loop
					currentCid = "";

					foreach (List<UnpivotedAssayResult> resultGroup in resultGroups.Values)
					{ // process all results for this resultGroup (i.e. all rows for this assay or target)
						sRow = resultGroup[0]; // copy first row
						nResults = 0;
						n = 0; // init stats for group
						vals = new List<double>();
						min = NullValue.NullNumber;
						max = NullValue.NullNumber;
						sum = 0;
						product = 1;
						qualifier = "";
						assayTypes = "";
						withinActivityLimits = false;
						ignoreTargetResult = false;

						for (ri = 0; ri < resultGroup.Count; ri++) // process each row in group
						{
							row = resultGroup[ri];
							nResults++; // count the results
							bool reset = false; // flag for resetting accumulation of values
							bool skip = false;

							if (Lex.Eq(sRow.ResultTypeConcType, "SP")) // filter SP data for a single concentration
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

								if (ignoreSecondaryResultIfPrimaryResult && // ignore SP if CRC value for any assay for target
									byTarget && // must be summarizing by target
									lastSummaryRow != null && // CRC would have been just before SP
									row.CompoundId == lastSummaryRow.CompoundId && // same compound
									row.GeneId == lastSummaryRow.GeneId && // and target
									lastSummaryRow.ResultTypeConcType == "CRC") // just see CRC
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

							if (Lex.Eq(sRow.ResultTypeConcType, "SP"))
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

								if (minSPToInclude != NullValue.NullNumber && row.ResultNumericValue >= minSPToInclude)
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

								if (maxCRCToInclude != NullValue.NullNumber && row.ResultNumericValue <= maxCRCToInclude)
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

							if (!String.IsNullOrEmpty(row.GeneFamily) && targetFamily != "Multiple")
							{ // keep track of target families seen to be used to select proper section of general family dendogram
								if (targetFamily == null) targetFamily = row.GeneFamily;
								else if (Lex.Ne(targetFamily, row.GeneFamily)) targetFamily = "Multiple";
							}
						}

						// Do statistics & store row

						if (((Lex.Eq(sRow.ResultTypeConcType, "SP") && minSPToInclude != NullValue.NullNumber) ||
							(Lex.Eq(sRow.ResultTypeConcType, "CRC") && maxCRCToInclude != NullValue.NullNumber)) &&
							!withinActivityLimits) // skip if not within requested activity limits
							continue;

						if (ignoreTargetResult) continue;

						if (n == 0) // can happen if mixed < & > but no unqualified values
						{
							sRow.ResultNumericValue = NullValue.NullNumber;
							sRow.StdDev = NullValue.NullNumber;
							if (maxCRCToInclude != NullValue.NullNumber && minSPToInclude != NullValue.NullNumber)
								continue; // skip if limits are set
						}

						else if (Lex.Eq(sRow.ResultTypeConcType, "SP"))
						{
							if (meanSummarization)
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

							else // just take max value
							{
								sRow.ResultNumericValue = max;
								qualifier = maxQualifier;
							}
						}

						else  // geometric mean for CRC data
						{
							if (meanSummarization)
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
						sRow.ResultValue.DbLink = sRow.CompoundId + "," + sRow.GeneId + "," + sRow.ResultTypeConcType;

						if (useCatenatedResultValueFormat)
						{ // format result as text string with value, units and concentration in a single string
							QualifiedNumber qn2 = new QualifiedNumber();
							if (sRow.ResultValue.NValue > 1) { int nnn = 2; }
							//QnfEnum qnFormat = QnfEnum.Combined | q.StatDisplayFormat;
							qn2.TextValue = sRow.ResultValue.Format(resultValQc, false, resultValMc.Format, resultValMc.Decimals, qnFormat, false);

							if (Lex.Eq(sRow.ResultTypeConcType, "SP"))
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

						// --- Include later
						//if (formatForSpotfire) // format target symbol with html link for Spotfire
						//{
						//  string uri = TargetAssayMetaBroker.GetTargetDescriptionUrl(sRow.TargetSymbol);
						//  if (uri != null)
						//  { // include symbol with link to description at astart
						//    sRow.TargetDescription = "<a href=\"" + uri + "\" target=_blank>" +
						//     sRow.TargetSymbol + "</a> - " + sRow.TargetDescription;
						//  }
						//}

						sRow.CidOrder = currentCidPos; // set "Load Order" (todo include row index?)

						//if (sRow.TargetId != NullValue.NullNumber) sRow = sRow; // debug

						if (targetMap != null && targetMap.Coords != null && !Lex.IsNullOrEmpty(sRow.GeneSymbol) && // Store coordinates
							targetMap.Coords.ContainsKey(sRow.GeneId)) 
						{
							List<TargetMapCoords> coordList = targetMap.Coords[sRow.GeneId];
							for (tci = 0; tci < coordList.Count; tci++)
							{
								if (tci == 0) rr2 = sRow; // if first coords put in sRow
								else rr2 = sRow.CloneUnpivotedAssayResult(); // otherwise add a new row

								tmc = coordList[tci];
								rr2.TargetMapX = tmc.X;
								rr2.TargetMapY = -tmc.Y; // use negative value for y
								if (tci > 0) results.Add(rr2); // add to buffer if beyond first row that is added below
							}
						}

						lastSummaryRow = sRow;
						results.Add(sRow);
					} // end of summarizing group

					if (eof) return results;
				} // end of summarizing groups
			}
		}

/// <summary>
/// GetChartCornerRows
/// </summary>
/// <param name="targetMap"></param>
/// <param name="targetFamily"></param>
/// <returns></returns>

		public static List<UnpivotedAssayResult> GetChartCornerRows(
				TargetMap targetMap,
				string targetFamily)
		{
			List<UnpivotedAssayResult> results = new List<UnpivotedAssayResult>(); // results are built here
			UnpivotedAssayResult rr2;

			rr2 = new UnpivotedAssayResult();
			rr2.CompoundId = "00000000"; // query engine needs a valid id, changed to null on output
			rr2.ActivityBin = NullValue.NullNumber;
			rr2.ResultNumericValue = NullValue.NullNumber;
			rr2.TargetMapX = targetMap.Bounds.Left;
			rr2.TargetMapY = -targetMap.Bounds.Top;
			results.Add(rr2);

			rr2 = new UnpivotedAssayResult();
			rr2.CompoundId = "00000000";
			rr2.ActivityBin = NullValue.NullNumber;
			rr2.ResultNumericValue = NullValue.NullNumber;
			rr2.TargetMapX = targetMap.Bounds.Right;
			rr2.TargetMapY = -targetMap.Bounds.Bottom;
			results.Add(rr2);

			return results;
		}

/// <summary>
/// Copy MetaColumn values other than name
/// </summary>
/// <param name="mc1"></param>
/// <param name="mc2"></param>

		static void CopyMcVals(
			MetaColumn mc1,
			MetaColumn mc2)
		{
			string name = mc2.Name;
			string label = mc2.Label;
			ObjectEx.CopyObjectData(mc1, mc2, null, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			//ObjectEx.MemberwiseCopy(mc1, mc2);
			mc2.Name = name;
			mc2.Label = label;
			return;
		}

		/// <summary>
		/// Convert dendogram coordinates to KEGG format used by Mobius
		/// Sample command: TargetResultsViewer ConvertDendogramCoords C:\MobiusServerData\MetaData\TargetMaps\protein_coordinates.txt
		/// </summary>
		/// <param name="lex"></param>
		/// <returns></returns>

		public static string ConvertDendogramCoords(
			Lex lex)
		{
			string path = lex.Get();
			StreamReader sr = new StreamReader(path);

			path = System.IO.Path.GetDirectoryName(path) + @"\Dendograms.coord";
			StreamWriter sw = new StreamWriter(path);

			string rec = sr.ReadLine(); // read header line
			while (true)
			{
				rec = sr.ReadLine();
				if (rec == null) break;

				if (Lex.Contains(rec, "mTOR_pathway")) continue; // part of other pathway
				string[] toks = rec.Split('\t');
				sw.WriteLine(
					toks[0] + "\t" + // gene Id
					toks[3] + "\t" + // x
					toks[4]); // y
			}

			sr.Close();
			sw.Close();

			return "Converted";
		}


	} // TargetAssayUtil

	/// <summary>
	/// Summarization method
	/// </summary>

	public enum SummarizationType
	{
		Undefined = 0,
		Count = 1,
		Sum = 2,
		Min = 3,
		Max = 4,
		ArithmeticMean = 5,
		GeometricMean = 6,
		Median = 7,
		Mode = 8,

		BioResponseMean = 11, // Geometric mean for CRC data, Arithmetic mean for SP data
		MostPotent = 12, // Min for CRC data, Max for SP data

		None = 999 // do not summarize
	}

	/// <summary>
	/// Columns to transform
	/// </summary>

	public enum ColumnsToTransform
	{
		Unknown = 0,
		PrimaryResults = 1,
		ColsWithCondFormat = 2
	}

	/// <summary>
	/// Summarization level for substance
	/// </summary>

	public enum TargetAssaySummarizationLevel
	{
		None = 0, // do not summarize by compound
		Assay = 1, // summarize the data for a compound by assay
		Target = 2 // summarize the data for a compound by target (possibly multiple assays)
	}

	/// <summary>
	/// Pivoted format of data
	/// </summary>

	public enum TargetAssayPivotFormat
	{
		Unknown = 0,
		Unpivoted = 1,
		QueryOrder = 2,
		ByAssay = 3,
		ByGeneSymbol = 4,
		ByGeneFamily = 5
	}

}
