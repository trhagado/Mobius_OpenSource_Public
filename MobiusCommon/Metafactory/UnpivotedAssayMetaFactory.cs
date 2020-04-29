using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{
	public class UnpivotedAssayMetaFactory : IMetaFactory
	{

		static LogFile Log;

		/// <summary>
		/// Create MetaTable from template
		/// </summary>

		public MetaTable GetMetaTable(
				string mtName)
		{
			if (!Lex.StartsWith(mtName, UnpivotedAssayView.UnsummarizedMetaTableName)) return null;

			MetaTable mt0 = MetaTableCollection.Get(UnpivotedAssayView.UnsummarizedMetaTableName + "_TEMPLATE"); // get template metatable
			if (mt0 == null) return null;

			MetaTable mt = mt0.Clone();

			string labelSuffix = ""; // default to integrated view of these

			mt.Label += labelSuffix;

			List<MetaColumn> mcList = new List<MetaColumn>();

// Build summarized version of table

			if (Lex.EndsWith(mtName, MetaTable.SummarySuffix)) // summarized version
			{
				mt.Name = mtName;
				mt.Label += " Summary";
				mt.UseSummarizedData = true;

				foreach (MetaColumn mc in mt.MetaColumns)
				{
					if (mc.SummarizedExists) mcList.Add(mc);
				}

				mt.MetaColumns = mcList;
				return mt;
			}

			// Build unsummarized version of table

			else
			{
				mt.Name = mtName;
				foreach (MetaColumn mc in mt.MetaColumns)
				{
					if (mc.UnsummarizedExists) mcList.Add(mc);
				}

				mt.MetaColumns = mcList;
				return mt;
			}
		}

		/// <summary>
		/// Update Common Assay Attributes table (mbs_owner.cmn_assy_atrbts)
		/// 
		/// Command: Update AssayAttributesTable
		/// 
		/// This command builds an entry (or two) in the cmn_assy_atrbts table for each assay 
		/// referenced in the Mobius contents tree that reports an SP or CRC value as determined
		/// by the associated metafactory and available in each metatable.
		/// If the gene target associated with an assay can be identified then information on that
		/// gene is included as well. 
		/// 
		/// Additional gene information may come from the metadata for a source such as the results
		/// warehouse.
		/// 
		/// Note that this function must be run under an account that has access to all restricted data so that it
		/// can properly see what's available and build the table.
		/// </summary>
		/// <param name="lex"></param>
		/// <returns></returns>

		public static string UpdateAssayAttributesTable(string args)
		{
			MetaTable mt;
			MetaColumn mc;
			AssayAttributes aa, aa2;

			Dictionary<int, int> geneIdCounts = new Dictionary<int,int>(); // genes and counts keyed by entrez gene id
			Dictionary<int, double> targetMapXDict; // dendogram X coord keyed by gene id
			Dictionary<int, double> targetMapYDict; // dendogram Y coord keyed by gene id

			Dictionary<string, int> targetTypeCounts = new Dictionary<string, int>(); // target types and counts
			Dictionary<int, string> assayTypeDict;
			Dictionary<int, string> assayModeDict;

			//UnpivotedAssayResult rr, rr2;
			List<string> toks;
			string mtName, tableNamePrefix, key, fileName, msg;
			bool crcExists, spExists, isSummary;
			int tableId, step, ri, resultTypeId, assayRowCount;

			Log = new LogFile(ServicesDirs.LogDir + @"\UpdateCommonAssayAttributes.log");
			Log.ResetFile();

// Get list of all of the assays in the tree

			LogMessage("Accumulating assays...");

			HashSet<string> mtNameHash = new HashSet<string>();

			foreach (MetaTreeNode mtn0 in MetaTree.Nodes.Values)
			{
				if (mtn0.Type != MetaTreeNodeType.MetaTable) continue;

				mtName = mtn0.Target;

				if (AssayMetaData.IsAssayMetaTableName(mtName))
				{
					mtNameHash.Add(mtName);
				}

			}

			bool debug = false; // set to true to debug with limited list of assays from below
			if (debug) 
			{
				mtNameHash.Clear();

				//assayHash.Add("ASSAY_1"); 
				//assayHash.Add("ASSAY_2"); 
				//assayHash.Add("ASSAY_3"); 

			}

			// Get other informatin needed from AssayMetadata

			LogMessage("Reading AssayMetadata ASSAY metadata...");
			Dictionary<int, AssayDbMetadata> assayMetadataAssayDict = // get assays and associated target/gene information
				AssayMetadataDao.GetAssayTargetGeneData();

			LogMessage("Getting AssayMetadata result types...");
			Dictionary<int, AssayDbResultType> resultTypeDict = AssayMetadataDao.GetResultTypeDict();

			LogMessage("Getting assay types and modes...");
			AssayMetadataDao.GetAssayTypesAndModes(out assayTypeDict, out assayModeDict);

			LogMessage("Getting gene dendogram coordinates...");
			try
			{
				AssayMetadataDao.GetImageCoords(out targetMapXDict, out targetMapYDict);
			}
			catch (Exception ex) // may fail if problem with data source
			{
				LogMessage(DebugLog.FormatExceptionMessage(ex, true));
				targetMapXDict = new Dictionary<int, double>();
				targetMapYDict = new Dictionary<int, double>();
			}


			// Process each assay

			int metatablesFound = 0, metatablesNotFound = 0;

			int assaysProcessed = 0;
			int assaysWithGenes = 0;
			int assaysWithGeneCoords = 0;
			int assaysWithTargets = 0;
			int assaysWithSpOnly = 0;
			int assaysWithCrcOnly = 0;
			int assaysWithNoCrcSP = 0;
			int assaysWithOtherTypes = 0;
			int assaysWithCrcAndSp = 0;
			int assaysWithNoKeyTypes = 0;
			int assaysWithProcessingErrors = 0;

			Dictionary<string, int> CrcAssayCnt = new Dictionary<string, int>() { { "ASSAY_DB1", 0 }, { "ASSAY_DB2", 0 }, { "ASSAY_DB3", 0 } };
			Dictionary<string, int> SpAssayCnt = new Dictionary<string, int>() { { "ASSAY_DB1", 0 }, { "ASSAY_DB2", 0 }, { "ASSAY_DB3", 0 } };
			Dictionary<string, int> OtherAssayCnt = new Dictionary<string, int>() { { "ASSAY_DB1", 0 }, { "ASSAY_DB2", 0 }, { "ASSAY_DB3", 0 } };

			List<AssayAttributes> resultTypeRows = new List<AssayAttributes>();
			List<AssayAttributes> geneRows = new List<AssayAttributes>();
			List<AssayAttributes> dbRows = new List<AssayAttributes>();

			string copyUpdateMsg = "";

			foreach (string mtName0 in mtNameHash)
			{
				AssayMetaData assayMetaData = null; // metadata for assay
				bool isAssay = false;
				int assayIdNbr  = NullValue.NullNumber;
				int assayId = NullValue.NullNumber;

				//if (assaysProcessed >= 100) break; // debug

				mtName = mtName0;
				MetaTable.ParseMetaTableName(mtName, out tableNamePrefix, out tableId, out isSummary);

				string resultType = "";
				string rtId = "";
				string assayName, assayDb;

				mt = MetaTableCollection.Get(mtName); // get metatable
				if (mt == null)
				{
					metatablesNotFound++;
					LogMessage("MetaTable not found: " + mtName);
					continue;
				}

				metatablesFound++;

				if (mt.Code == "") continue; // must be single pivoted assay

				assayDb = "ASSAY_DB"; // customize
				assayIdNbr = -1; 

				if (UalUtil.IClient != null && UalUtil.IClient.Attended)
					UAL.Progress.Show((assaysProcessed + 1).ToString() + " / " + mtNameHash.Count + " - " + mt.Name + "\r\n" + mt.Label);

				aa = new AssayAttributes();
				aa.AssayDatabase = assayDb;
				aa.AssayIdNbr = assayIdNbr; // data-source-specific assay Id
				aa.AssayIdTxt = mt.Name; // store ASSAY_1234  type table name
				aa.AssayId2 = assayId; // any associated assay id

				if (isAssay)
					aa.AssayName = assayMetaData.Name; // name from AssayMetadata
				else aa.AssayName = MetaTable.RemoveSuffixesFromName(mt.Label); // name from metatable

				if (mt.SummarizedExists) aa.SummarizedAvailable = true;
				else aa.SummarizedAvailable = false;

				if (isAssay)
				{
					if (assayTypeDict.ContainsKey(tableId))
						aa.AssayType = assayTypeDict[tableId];

					if (assayModeDict.ContainsKey(tableId))
						aa.AssayMode = assayModeDict[tableId];

					aa.AssaySource = AssayMetaData.GetAssaySource(tableId);
					aa.AssociationSource = "TODO"; // customize
				}

				aa.AssayStatus = "Active"; // say all active for now

				MetaTableStats mts = MetaTableFactory.GetStats(mtName);
				if (mts != null)
				{
					aa.ResultCount = (int)mts.RowCount;
					aa.AssayUpdateDate = mts.UpdateDateTime;
				}
				else aa.ResultCount = 0; // assume no results if no stats

				if (mt.DescriptionIsAvailable()) // use description from Mobius
          aa.AssayDesc = "Y";

        if (String.IsNullOrEmpty(aa.GeneFamily)) aa.GeneFamily = "Unknown"; // set these to "Unknown" rather than null
        if (String.IsNullOrEmpty(aa.AssayType)) aa.AssayType = "UNKNOWN"; // upper case UNKNOWN
        if (String.IsNullOrEmpty(aa.AssayMode)) aa.AssayMode = "UNKNOWN"; // upper case UNKNOWN

// Step1: Add a row for primary & any secondary results

				resultTypeRows.Clear();
				MetaColumn firstResultCol = null, firstKeyResultCol = null, firstOtherKeyResultCol = null;
				string resultTypeConcType;
				HashSet<string> keyResultTypeCodes = new HashSet<string>();
				int spCnt = 0, crcCnt = 0, otherCnt = 0;

				for (int mci = 0; mci < mt.MetaColumns.Count; mci++) // pick first col with result code (could also check summarization method)
				{
					mc = mt.MetaColumns[mci];

					if (Lex.IsUndefined(mc.ResultCode)) continue; // must have code defined
					if (keyResultTypeCodes.Contains(mc.ResultCode)) continue; // and not included so far
					if (mc.InitialSelection != ColumnSelectionEnum.Selected) continue; // selected only

					if (firstResultCol == null)
						firstResultCol = mc;

					if (!IsKeyResultType(mc, out resultTypeConcType)) continue;

					if (firstKeyResultCol == null)
						firstKeyResultCol = mc;

					keyResultTypeCodes.Add(mc.ResultCode);
					
					aa2 = aa.Clone();

					if (resultTypeRows.Count == 0)
						aa2.TopLevelResult = "Y";
					else aa2.TopLevelResult = "N";

					aa2.ResultTypeId2 = GetAssayResultTypeId(mc); // AssayMetadata result type id
					aa2.ResultTypeIdNbr = GetInternalResultTypeId(mc); // Internal database result type id 
					aa2.ResultTypeIdTxt = mc.Name; // Mobius column name

					if (isAssay && resultTypeDict.ContainsKey(aa2.ResultTypeId2))
						aa2.ResultName = resultTypeDict[aa2.ResultTypeId2].Name; // use name from AssayMetadata result type dict
					else aa2.ResultName = mc.Label; // use label from Mobius

					aa2.ResultTypeUnits = mc.Units; // result units

					if (Lex.Eq(resultTypeConcType, "SP"))
					{
						aa2.ResultTypeConcType = "SP";
						spCnt++;
					}

					else if (Lex.Eq(resultTypeConcType, "CRC"))
					{
						aa2.ResultTypeConcType = "CRC";
						crcCnt++;
					}

					else 
					{
						aa2.ResultTypeConcType = "";
						otherCnt++;
						if (firstOtherKeyResultCol == null)
							firstOtherKeyResultCol = mc;
					}

					aa2.ResultTypeConcUnits = ""; // todo

					resultTypeRows.Add(aa2);
				}

				if (resultTypeRows.Count >= 1)
				{
					if (crcCnt > 0) CrcAssayCnt[assayDb]++; // count primary type by db
					else if (spCnt > 0) SpAssayCnt[assayDb]++;
					else OtherAssayCnt[assayDb]++;

					if (crcCnt > 0 && spCnt == 0) assaysWithCrcOnly++; // count overall primary/secondary types
					else if (crcCnt == 0 && spCnt > 0) assaysWithSpOnly++;
					else if (crcCnt > 0 && spCnt > 0) assaysWithCrcAndSp++;

					if (crcCnt == 0 && spCnt == 0) // no SP or CRC result types
					{
						assaysWithNoCrcSP++;
						mc = firstKeyResultCol;
						LogMessage("Assay with No SP/CRC key results: " + mt.Name + "." + mc.Name + " (" + mc.ResultCode + "), " + mt.Label + "." + mc.Label);
					}

					else if (otherCnt > 0) // no SP or CRC result types
					{
						assaysWithOtherTypes++;
						mc = firstOtherKeyResultCol;
						LogMessage("Non SP/CRC key result: " + mt.Name + "." + mc.Name + " (" + mc.ResultCode + "), " + mt.Label + "." + mc.Label);
					}

				}

				else // no key result type
				{
					aa2 = aa.Clone();
					resultTypeRows.Add(aa2); // include row for step1

					OtherAssayCnt[assayDb]++;
					assaysWithNoKeyTypes++;
					LogMessage("No key result type for metatable: " + mt.Name + ", " + mt.Label);
				}

// Build a step2 row for each target/gene

				geneRows.Clear();
				List<AssayTarget> targets = new List<AssayTarget>();
				int geneCount = 0;

				if (isAssay) targets = assayMetaData.Targets;
				if (targets.Count > 0)
					assaysWithTargets++;

				foreach (AssayTarget target in targets)
				{
					aa = new AssayAttributes();
					aa.GeneFamily = target.TargetTypeShortName; // count target type occurance
					if (Lex.IsUndefined(aa.GeneFamily)) aa.GeneFamily = "Unknown";
					if (!targetTypeCounts.ContainsKey(aa.GeneFamily))
						targetTypeCounts[aa.GeneFamily] = 0;
					targetTypeCounts[aa.GeneFamily]++;

					if (target.Genes == null || target.Genes.Count == 0) // if no genes add a single target row
					{
						geneRows.Add(aa);
						continue;
					}

					foreach (AssayGene rg in target.Genes)
					{
						if (!Lex.IsDefined(rg.GeneSymbol))
							continue;

						aa2 = aa.Clone();
						geneRows.Add(aa2);

						aa2.GeneSymbol = rg.GeneSymbol;
						int.TryParse(rg.GeneId, out aa2.GeneId);

						if (aa2.GeneId > 0 && targetMapXDict.ContainsKey(aa2.GeneId))
						{
							aa2.TargetMapX = targetMapXDict[aa2.GeneId];
							aa2.TargetMapY = targetMapYDict[aa2.GeneId];
							if (geneCount == 0)
								assaysWithGeneCoords++;
						}

						if (!geneIdCounts.ContainsKey(aa2.GeneId)) // count gene occurance
							geneIdCounts[aa2.GeneId] = 0;
						geneIdCounts[aa2.GeneId]++;

						if (geneCount == 0)
							assaysWithGenes++;

						geneCount++;
					}
				}

				if (geneRows.Count == 0) // if no step 2 rows (i.e. no targets), create a single step2 row
				{
					aa = new AssayAttributes();
					geneRows.Add(aa);
				}

// Combine key result types with target/genes

				for (int i1 = 0; i1 < resultTypeRows.Count; i1++)
				{
					AssayAttributes s1aa = resultTypeRows[i1];
					for (int i2 = 0; i2 < geneRows.Count; i2++)
					{
						AssayAttributes s2aa = geneRows[i2];

						aa = s1aa.Clone();
						aa.GeneId = s2aa.GeneId;
						aa.GeneSymbol = s2aa.GeneSymbol;
						aa.GeneFamily = s2aa.GeneFamily;

						aa.TargetMapX = s2aa.TargetMapX;
						aa.TargetMapY = s2aa.TargetMapY;

						aa.GeneCount = geneCount;

						if (i2 > 0) aa.GeneCount = -geneCount; // negative for other than 1st gene

						dbRows.Add(aa);
					}
				}

				assaysProcessed++;
			}

			// Update table

			bool updateTable = true; // set to false for debug

			if (dbRows.Count <= 0)
				LogMessage("No rows in new dataset, table not updated");

			else if (updateTable)
			{
				LogMessage("Deleting existing data...");
				DbCommandMx dao = new DbCommandMx();
				string sql = "delete from mbs_owner.cmn_assy_atrbts";

				sql = AssayAttributesDao.AdjustAssayAttrsTableName(sql);

				dao.Prepare(sql);
				dao.BeginTransaction();
				int delCnt = dao.ExecuteNonReader();

				LogMessage("Inserting new data...");
				int t0 = TimeOfDay.Milliseconds();
				for (ri = 0; ri < dbRows.Count; ri++)
				{
					aa = dbRows[ri];
					aa.Id = ri + 1;

					//aa.Id += 10000; // debug

					if (aa.GeneSymbol != null) aa.GeneSymbol = aa.GeneSymbol.ToUpper(); // be sure key match cols are upper case
					if (aa.GeneFamily != null) aa.GeneFamily = aa.GeneFamily.ToUpper();
					if (aa.GeneFamilyTargetSymbol != null) aa.GeneFamilyTargetSymbol = aa.GeneFamilyTargetSymbol.ToUpper();
					if (aa.ResultTypeConcType != null) aa.ResultTypeConcType = aa.ResultTypeConcType.ToUpper();
					if (aa.AssayType != null) aa.AssayType = aa.AssayType.ToUpper();
					if (aa.AssayMode != null) aa.AssayMode = aa.AssayMode.ToUpper();

					AssayAttributesDao.InsertCommonAssayAttributes(aa, dao);
					if (TimeOfDay.Milliseconds() - t0 > 1000)
					{
						//Progress.Show("Inserting new data " + (ri + 1) + "/" + rows.Count + "...");
						t0 = TimeOfDay.Milliseconds();
					}
				}

				dao.Commit();
				dao.Dispose();

				copyUpdateMsg = UpdateCmnAssyAtrbtsCopies();
			}

			string response =
				"----------------------------------\r\n" +
				"Assays processed: " + assaysProcessed + "\r\n" +
				"Assays with processing errors: " + assaysWithProcessingErrors + "\r\n" +
				"Rows inserted: " + dbRows.Count + "\r\n" +
				 copyUpdateMsg +
				"----------------------------------\r\n" +
				"Assays with CRC only: " + assaysWithCrcOnly + "\r\n" +
				"Assays with SP only: " + assaysWithSpOnly + "\r\n" +
				"Assays with CRC and SP: " + assaysWithCrcAndSp + "\r\n" +
				"Assays with no CRC or SP: " + assaysWithNoCrcSP + "\r\n" +
				"Assays with non CRC/SP key types: " + assaysWithOtherTypes + "\r\n" +
				"Assays with no key types: " + assaysWithNoKeyTypes + "\r\n" +
				"----------------------------------\r\n" +
				"Assays with targets defined: " + assaysWithTargets + "\r\n" +
				"Assays with genes defined: " + assaysWithGenes + "\r\n" +
				"Assays with gene map coordinates: " + assaysWithGeneCoords + "\r\n" +
				"----------------------------------\r\n" +
				//"CRC Assays: " + CrcAssayCnt["ASSAY"] + "\r\n" +
				//"SP  Assays: " + SpAssayCnt["ASSAY"] + "\r\n" +
				//"??? Assays: " + OtherAssayCnt["ASSAY"] + "\r\n" +
				"----------------------------------";

			LogMessage("\r\n" + response);

			UAL.Progress.Hide();
			return response;
		}

/// <summary>
/// Update copies of CMN_ASSY_ATRBTS in other Oracle instances
/// </summary>
/// <returns></returns>

		static string UpdateCmnAssyAtrbtsCopies()
		{
			DbCommandMx dao;
			string sql, msg = "", errMsg = "";
			int cnt;

			string[] dbList = new string[]
			{
			//"AssayDB1", 
			//"AssayDB2", 
			//"...", 
			};

			foreach (string conName in dbList)
			{

				try
				{
					dao = new DbCommandMx();
					dao.MxConn = DbConnectionMx.GetConnection(conName);

					sql = "delete from MBS_OWNER.CMN_ASSY_ATRBTS";
					dao.PrepareUsingDefinedConnection(sql);
					cnt = dao.ExecuteNonReader();

					sql = @"
					insert into MBS_OWNER.CMN_ASSY_ATRBTS 
					 select * from MBS_OWNER.CMN_ASSY_ATRBTS";

					dao.PrepareUsingDefinedConnection(sql);
					cnt = dao.ExecuteNonReader();

					dao.Commit();
					dao.Dispose();

					if (msg != "") msg += ", ";
					msg += conName;
				}

				catch (Exception ex)
				{
					errMsg += "Error updating " + conName + ": " + ex.Message + "\r\n";
				}

			}

			if (msg != "") msg = "Updated copies of CMN_ASSY_ATRBTS in: " + msg;
			if (errMsg != "") msg += "\r\n" +  errMsg;
			return msg;
		}

/// <summary>
/// Return key result type if col is a key result col
/// </summary>
/// <param name="mc"></param>
/// <param name="resultTypeConcType"></param>
/// <returns></returns>

		static bool IsKeyResultType(
			MetaColumn mc,
			out string resultTypeConcType)
		{
			resultTypeConcType = "";
			if (Lex.IsUndefined(mc.ResultCode)) return false;

			int assayMetadataType = GetAssayResultTypeId(mc);
			if (assayMetadataType == 0) return false;

			bool iskeyType = QueryEngineLibrary.AssayResultType.IskeyResultType(assayMetadataType, out resultTypeConcType);
			return iskeyType;
		}

/// <summary>
/// Look at metatable title for possible CRC / SP assay type
/// </summary>
/// <param name="mt"></param>
/// <returns></returns>

		static string GetAssayConcType(MetaTable mt)
		{
			string[] concTypes = { "CRC", "SP" };
			string concType = "";
			string n = mt.Label;

			foreach (string ct0 in concTypes)
			{
				if (Lex.StartsWith(n, ct0 + " ") || Lex.Contains(n, " " + ct0 + " ") || Lex.EndsWith(n, " " + ct0))
				{
					concType = ct0;
					break;
				}
			}

			return concType;
		}

/// <summary>
/// Get primary or secondary key result (Obsolete)
/// </summary>
/// <param name="assayMetadataAssay"></param>
/// <param name="rti"></param>
/// <returns></returns>

		static MetaColumn GetKeyResult(
			AssayDbMetadata assayMetadataAssay,
			MetaTable mt,
			int rti,
			MetaColumn primary)
		{
			Dictionary<string, string> keyTypeDict = new Dictionary<string, string> 
				{ 
					{ "Inh", "SP" },
					{ "% Inh", "SP" },
					{ "Inhibition", "SP" },
					{ "% Inhibition", "SP" },
					{ "Stim", "SP" },
					{ "% Stim", "SP" },
					{ "Stimulation", "SP" },
					{ "% Stimulation", "SP" },
					{ "Pot", "SP" },
					{ "Potentiation", "SP" },
					{ "IC50", "CRC" },
					{ "EC50", "CRC" },
					{ "LC50", "CRC" },
					{ "Ki", "CRC" },
					{ "Kb", "CRC" },
					{ "Ka", "CRC" }
				};

			MetaColumn mc = null;
			bool isAssay = (assayMetadataAssay != null);

			if (rti == 1) mc = mt.PrimaryResult;
			else mc = mt.SecondaryResult;

			if (mc != null) // defined in metatable
			{
				mc = mc.Clone();
				return mc;
			}

			if (rti == 2) return null; // if secondary then dont' try to second-guess the metatable

			// See if CRC or SP in assay label

			string[] concTypes = { "CRC", "SP" };
			string concType = "";
			string n = mt.Label;
			foreach (string ct0 in concTypes)
			{
				if (Lex.StartsWith(n, ct0 + " ") || Lex.Contains(n, " " + ct0 + " ") || Lex.EndsWith(n, " " + ct0))
				{
					concType = ct0;
					break;
				}
			}

			//if (Lex.IsUndefined(type) && assay.CRC) // undefined in assay name but marked as CRC in AssayMetadata?
			//  type = "CRC";

			foreach (MetaColumn mc0 in mt.MetaColumns) // pick first col with result code (could also check summarization method)
			{
				if (Lex.IsUndefined(mc0.ResultCode)) continue;

				mc = mc0; 
				break;
			}
			if (mc == null) return null;

			mc = mc.Clone(); // make copy

			if (isAssay)
			{
				Dictionary<int, AssayDbResultType> resultTypeDict = AssayMetadataDao.GetResultTypeDict();

				if (resultTypeDict.ContainsKey(mc.Id) && // if mc is summarized as Log Normal assume CRC 
				 Lex.Eq(resultTypeDict[mc.Id].SumMdTxt, "Log Normal"))
				{
					concType = "CRC";
				}


				else if (Lex.IsUndefined(concType) && assayMetadataAssay.CRC) // undefined in assay name but marked as CRC in AssayMetadata?
					concType = "CRC";
			}

			if (Lex.IsUndefined(concType)) // check exact match with key column names
			{
				foreach (string ktn0 in keyTypeDict.Keys)
				{
					if (Lex.Eq(mc.Label, ktn0))
					{
						concType = keyTypeDict[ktn0];
						break;
					}
				}
			}

			if (Lex.IsUndefined(concType)) // check substring match with key column names
			{
				foreach (string ktn0 in keyTypeDict.Keys)
				{
					if (Lex.StartsWith(mc.Label, ktn0 + " ") || Lex.Contains(mc.Label, " " + ktn0 + " ") || Lex.EndsWith(mc.Label, " " + ktn0))
					{
						concType = keyTypeDict[ktn0];
						break;
					}
				}
			}

			mc.PrimaryResult = true;
			mc.SecondaryResult = false;
			mc.SinglePoint = (concType == "SP");
			mc.MultiPoint = (concType == "CRC");

			return mc;
		}

/// <summary>
/// Get the shared result type id associated with the supplied metacolumn (e.g. AssayMetadata result type id)
/// </summary>
/// <param name="mc"></param>
/// <returns></returns>

		static int GetAssayResultTypeId(MetaColumn mc)
		{
			string[] sa;
			int typeId = -1;
			bool parseOk = false;

			MetaBrokerType mbt = mc.MetaTable.MetaBrokerType;

			if (mbt == MetaBrokerType.Assay)
			{
				typeId = AssayMetaFactory.GetAssayMetadataResultTypeId(mc);
			}

			else
				LogMessage("Unexpected MetaBrokerType: " + mbt + " for " + mc.MetaTable.Name + "." + mc.Name);

			return typeId;
		}

/// <summary>
/// Get the internal database result type id
/// </summary>
/// <param name="mc"></param>
/// <param name="sharedTypeId"></param>
/// <param name="internalDbTypeId"></param>
/// <returns></returns>

		static int GetInternalResultTypeId(MetaColumn mc)
		{
			Dictionary<int, QueryEngineLibrary.AssayResultType> dict;

			bool parseOk;
			int iTypeId = NullValue.NullNumber, typeId = NullValue.NullNumber;

			MetaBrokerType mbt = mc.MetaTable.MetaBrokerType;

			if (mbt == MetaBrokerType.Assay)
			{
				parseOk = int.TryParse(mc.ResultCode, out typeId);
				if (parseOk) iTypeId = typeId;
			}

			else
				LogMessage("Unexpected MetaBrokerType: " + mbt + " for " + mc.MetaTable.Name + "." + mc.Name);

			return iTypeId;
		}


		/// <summary>
		/// Update the cached TargetAssayDictionary file from the database
		/// </summary>

		public static string UpdateTargetAssayAttributesDictionaryCacheFileFromDatabase()
		{
			AssayDict tad = AssayAttributesDao.ReadAssayAttributesTable(); // get data from database
			string fileName = ServicesDirs.MetaDataDir + @"\AssayAttributes.bin";
			tad.SerializeToFile(fileName);
			return "Assay attributes dictionary file updated, rows: " + tad.AssayList.Count;
		}

		/// <summary>
		/// Log message to log file & progress
		/// </summary>
		/// <param name="msg"></param>
		static void LogMessage(string msg)

		{
			Log.Message(msg);
			UAL.Progress.Show(msg);
		}

		/// <summary>
		/// Convert string to Y/N value
		/// </summary>
		/// <param name="tok"></param>
		/// <returns></returns>

		string ToYN(
			string tok)
		{
			if (String.IsNullOrEmpty(tok)) return null;
			else if (Lex.Eq(tok, "Y") || Lex.Eq(tok, "T") || Lex.Eq(tok, "1"))
				return "Y";
			else return "N";
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			return 0;
		}

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public int LoadMetaTableStats(
			Dictionary<string, MetaTableStats> metaTableStats)
		{
			return 0;
		}
	}
}
