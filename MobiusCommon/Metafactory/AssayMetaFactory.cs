using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Build metatables for ASSAY database 
	/// </summary>

	public class AssayMetaFactory : IMetaFactory, IMetaFactory2
	{
		/// <summary>
		/// Basic information on an Assay
		/// </summary>

		internal class AssayMetadata
		{
			public int AssayId = -1; // assay id from AssayMetadata
			public int AssayAssayId = -1; // internal id
			public string AssayName = "";
			public string AssayDesc = "";
			public MetaTable MergeDefTable; // if this is a merged assay then this is a pointer to the merge information
			public List<ResultTypeData> ResultTypeData = new List<ResultTypeData>();
			public Dictionary<int, ResultTypeData> RtdDict = new Dictionary<int, ResultTypeData>(); // maps from assay result type to data for type
		}

		/// <summary>
		/// Basic information on each result type for an assay
		/// </summary>

		internal class ResultTypeData
		{
			public int AssayId = -1;
			public int AssayResultTypeId = -1; // internal Assay type id
			public int AssayMetadataResultTypeId = -1; // type id from AssayMetadata 
			public int Position = -1; // for ordering purposes
			public string Name = ""; // result type name for display  to user
			public string SourceTable = ""; // dtld, actvty
			public string SummarizationMethod = ""; // summarization method (Arithmetic, Geometric, None)
			public bool SubgroupOnConc = false; // if true then summaries are subgrouped by concentration value
			public int ResultCount; // total row count for this type
			public int ResultGroupCount; // number of result groups for type (should match ResultCount)
			public int ValueNbrNonnullCount;
			public int ValueNbrCardinality;
			public int ValueTxtNonnullCount;
			public int ValueTxtCardinality;
			public string UomName = ""; // "Mltiple" if more than one unit 

			public int ConcNbrNonnullCount;
			public int ConcNbrCardinality;
			public string ConcUomName = ""; // "Mltiple" if more than one unit 

			public int ImageNonnullCount;
			public int CommentNonnullCount;

			public int ResultCountSummarized; // total summarized row count for this type
			public int ConcNbrNonnullCountSummarized;
			public int ConcNbrCardinalitySummarized;

			public string TargetSymbol = ""; // gene symbol for the gene associated with the target
			public int TargetId = NullValue.NullNumber; // Entrez gene id for target
			public string GeneFamily = ""; // Kinase, GPCR, Ion channel, NHR, Phosphatase, Protease...
			public string AssayType = ""; // BINDING, FUNCTIONAL, UNKNOWN
			public string AssayMode = ""; // AG, ANTAG, INH, POT...
			public string AssayLocation = ""; // where the assay is normally run

			//public ResultTypeData AssociatedRtd; (not used)
		}

		public static Dictionary<int, string> TextualMetadata = null; // assay metadata in text form keyed by assayId
		public static Dictionary<string, MetaTableStats> TableStats = null;
		public static HashSet<string> ProductionAssays = null; // list of assays moved to production

		public static bool SummarizedConcentrationsExist = false; // if true then include concentrations in summarized data

		public static int GetMetaTableCount, GetMetaTableTime, GetMetaTableAvgTime; // performance data

		const ColumnSelectionEnum Selected = ColumnSelectionEnum.Selected;
		const ColumnSelectionEnum Unselected = ColumnSelectionEnum.Unselected;
		const ColumnSelectionEnum Hidden = ColumnSelectionEnum.Hidden;

		static ColumnSelectionEnum WellLevelColVisibility = ColumnSelectionEnum.Hidden; // visibility of well-level cols

		static bool IniFileSettingsRead = false;

		static HashSet<int> MigratingAssayTables;
		static HashSet<int> MigratedAssayTables;
		static List<KeyValuePair<int, int>> MigratedAssayTablesRange;

		static bool DefaultAssayMetatablesToAssay = true;

		static AssayMetaFactory Instance;

		/// <summary>
		/// Initialize factory 
		/// </summary>

		public AssayMetaFactory()
		{
			if (Instance == null)
			{
				Instance = this;
				AssayMetaBroker.AssayMetaFactory = this;
			}

			if (TableStats == null) // get table stats since may be needed by brokers
				LoadMetaTableStats();

			if (TextualMetadata == null)
				ReadPrecomputedAssayMetadata();

			if (QueryEngineLibrary.AssayResultType.ExtraResultTypeAttributes == null)
				QueryEngineLibrary.AssayResultType.ReadExtraResultTypeAttributes();

			if (!IniFileSettingsRead)
				ReadIniFileSettings();
		}

		/// <summary>
		/// ReadIniFileSettings
		/// </summary>

		static void ReadIniFileSettings()
		{
			if (ServicesIniFile.IniFile == null) return;

			if (!ServicesIniFile.ReadBool("AssayWellLevelColsVisible", false))
				WellLevelColVisibility = ColumnSelectionEnum.Hidden;
			else WellLevelColVisibility = ColumnSelectionEnum.Unselected;

			SummarizedConcentrationsExist = ServicesIniFile.ReadBool("AssaySummarizedConcentrationsExist", true);

			DefaultAssayMetatablesToAssay = ServicesIniFile.ReadBool("DefaultAssayMetatablesToAssay", true);

			IniFileSettingsRead = true;

			return;
		}

		/// <summary>
		/// Create MetaTable for a Assay assay
		/// </summary>

		public MetaTable GetMetaTable(
			string mtName)
		{
			MetaTable mt = null, mt2 = null, mtc = null;
			MetaColumn mc, mcc, mcc2;
			string mcName, mcNamePrefix, mcNameSuffix, mcLabel, dictKey, assayMetadataCode, txt, tok;
			int assayId, assayMetadataType;
			AssayMetadata assayData = null;
			ResultTypeData rtd = null;
			bool unsummarized = true, summarized = false, unpivotedView = false, pivotedView = true, unpivotedDetailLevelView = false, unpivotedWellLevelView = false;
			int i1, i2;

			try
			{
				mtName = mtName.Trim().ToUpper();


				if (mtName.StartsWith("Assay_")) { } // regular Assay prefix

				else if (IsAssayMetaTable(mtName)) { } // production ASSAY assay with ASSAY_xxxx style name?

				else return null;

				int t0 = TimeOfDay.Milliseconds();

				if (Lex.StartsWith(mtName, "ASSAY_UNPIVOTED"))
				{
					unpivotedView = true;
					pivotedView = false;

					assayId = -1;

					if (Lex.Eq(mtName, "ASSAY_UNPIVOTED"))
						unpivotedDetailLevelView = true;

					else if (Lex.Eq(mtName, "ASSAY_UNPIVOTED_WELL_LEVEL"))
						unpivotedWellLevelView = true;

					else if (Lex.Eq(mtName, "ASSAY_UNPIVOTED" + MetaTable.SummarySuffix)) // unpivoted summary
						summarized = true;

					else return null;
				}

				else // must be pivoted view
				{
					MetaTable.ParseMetaTableName(mtName, out assayId, out summarized);
					if (assayId < 0) return null;
				}

				unsummarized = !summarized;

				try
				{
					assayData = GetAssayMetadata(assayId);
					if (assayData == null) throw new Exception("GetAssayData returned null");

					else if (assayData.ResultTypeData.Count == 0)
						throw new Exception("No result types are defined for assay " + assayData.AssayName + " (" + mtName + ")");
				}
				catch (MobiusConnectionOpenException ex)
				{
					DbConnectionMx.ThrowSpecificConnectionOpenException(ex, "assay_owner");
				}

				catch (Exception ex)
				{
					DebugLog.Message("GetAssayData Error:\n" + ex.Message);
					return null;
				}

				mt = new MetaTable(); // build metatable here
				if (pivotedView)
				{
					//mt.Id = assayId;
					mt.Code = assayId.ToString();

					//if (assayId == 7168)	assayId = assayId; // debug

					//mt.Name = "ASSAY_" + assayId.ToString(); // all table names start with ASSAY_ even if coming in as ASSAY_

					if (mtName.StartsWith("ASSAY_"))
						mt.Name = "ASSAY_" + assayId.ToString();

					else if (mtName.StartsWith("ASSAY_")) // if migrated ASSAY keep same name
						mt.Name = "ASSAY_" + assayId.ToString();
				}

				else mt.Name = mtName; // use name as is for unpivoted

				MetaTreeNode mtn = MetaTreeFactory.GetNode(mt.Name); // try to get label from contents tree
				if (mtn != null) mt.Label = mtn.Label;
				if (mt.Label == "") mt.Label = assayData.AssayName;
				if (mt.Label == "") mt.Label = mt.Name;

				if (mtn == null && MetaTableFactory.ShowDataSource) // add source if requested
					mt.Label = MetaTableFactory.AddSourceToLabel(mt.Name, mt.Label);

				mt.Parent = MetaTableCollection.Get("corp_structure");
				mt.Description = mt.Name; // just need place holder for now, description is generated when requested.
				mt.MetaBrokerType = MetaBrokerType.Assay;
				mt.SummarizedExists = true; // assume some summary data always exists for table

				mt2 = mt.Clone(); // initial location for cols not selected by default

				int detailResultTypeCount = 0;
				int activityResultTypeCount = 0;
				int crcImageCount = 0;
				MetaColumn crcMc = null, potencyMc = null, resultValidityMc = null, resultCommentMc = null, wellLevelCommentMc = null;

				bool allowSecondaryImageColumns = ServicesIniFile.ReadBool("AssayAllowSecondaryImageColumns", false);

				mc = // add key metacolumn
					AddUnpivotedMetaColumn(mt, "<keyColumnName", "<keyColumnLabel>", MetaColumnType.CompoundId, Selected, 7, AssaySqlElements.RsltSubqueryName, "<keyColumnName>");

				if (unpivotedView) // if unpivoted add assay and result type name columns and hidden columns for codes
				{
					mc = AddUnpivotedMetaColumn(mt, "assay_nm", "Assay Name", MetaColumnType.String, Selected, 30, AssaySqlElements.Assay, "nm_txt");
					mc.Dictionary = "assay_assay_nm";
					mc.TextCase = ColumnTextCaseEnum.Mixed;

					mc = AddAssayAttrMetaColumn(mt, "assy_typ", ColumnSelectionEnum.Unselected); // Type of assay, e.g. binding, functional
					mc = AddAssayAttrMetaColumn(mt, "assy_mode", ColumnSelectionEnum.Unselected); // Mode of assay, e.g. agonist, antagonist, potentiator
					mc = AddUnpivotedMetaColumn(mt, "assay_id", "ASSAY Id", MetaColumnType.Integer, Unselected, 5, AssaySqlElements.Assay, "id_nbr"); // AssayMetadata ASSAY Id
					mc = AddUnpivotedMetaColumn(mt, "assay_id", "ASSAY Assay Id", MetaColumnType.Integer, Hidden, 5, AssaySqlElements.RsltSubqueryName, "assay_id");

					// Add target gene-related cols

					mc = AddGeneAttrMetaColumn(mt, "gene_symbl", ColumnSelectionEnum.Unselected);
					mc = AddGeneAttrMetaColumn(mt, "gene_id", ColumnSelectionEnum.Unselected);
					mc = AddGeneAttrMetaColumn(mt, "gene_fmly", ColumnSelectionEnum.Unselected);
					mc = AddGeneAttrMetaColumn(mt, "assy_gene_cnt", ColumnSelectionEnum.Unselected);

					// Add result type info

					mc = AddUnpivotedMetaColumn(mt, "rslt_typ_nm", "Result Type Name", MetaColumnType.String, ColumnSelectionEnum.Selected, 10, AssaySqlElements.ResultType, "nm_txt");
					mc.Dictionary = "assay_rslt_typ_nm";
					mc.TextCase = ColumnTextCaseEnum.Mixed;

					mc = AddUnpivotedMetaColumn(mt, "SECONDARY_RSLT_TYP_ID", "Secondary Result Type Id", MetaColumnType.Integer, Unselected, 5, AssaySqlElements.ResultType, "id_nbr"); // AssayMetadata Result Type Id
					mc = AddUnpivotedMetaColumn(mt, "rslt_typ_id", "ASSAY Result Type Id", MetaColumnType.Integer, Hidden, 5, AssaySqlElements.RsltSubqueryName, "rslt_typ_id");
					mc = AddResultTypeAttrMetaColumn(mt, "rslt_typ", ColumnSelectionEnum.Unselected); // "Result SP/CRC"
					mc = AddResultTypeAttrMetaColumn(mt, "top_lvl_rslt", ColumnSelectionEnum.Unselected); // "Is Primary Result"
				} // unpivotedView

				MetaColumn primaryMc = null, secondaryMc = null;
				int pMcNonnullCount = -1, sMcNonnullCount = -1;

				// Count number of detailed and activity result types

				for (int ci = 0; ci < assayData.ResultTypeData.Count; ci++)
				{
					rtd = assayData.ResultTypeData[ci];

					if (Lex.Eq(rtd.SourceTable, "dtld"))
						detailResultTypeCount++;

					else if (Lex.Eq(rtd.SourceTable, "actvty"))
						activityResultTypeCount++;

					else throw new Exception("Unrecognized results source table: " + rtd.SourceTable);
				}

				// Create metacolumns for each result type

				for (int ci = 0; ci < assayData.ResultTypeData.Count; ci++)
				{
					rtd = assayData.ResultTypeData[ci];

					//if (rtd.AssayMetadataResultTypeId == 3) rtd = rtd; // debug

					mc = new MetaColumn();
					mc.MetaTable = mt;
					mc.Id = rtd.AssayResultTypeId; // use AssayResultTypeId because AssayMetadataResultTypeId codes are reused between result types and stat types
					if (mc.Id > 0)
					{
						mc.ResultCode = mc.Id.ToString();
						mc.ResultCode2 = rtd.AssayMetadataResultTypeId.ToString(); // store AssayMetadata result type code in code2
					}

					if (Lex.Eq(rtd.SourceTable, "dtld"))
						mc.SummarizationRole = SummarizationRole.Dependent;

					else // from "activity" table
						mc.SummarizationRole = SummarizationRole.Derived;

					if (pivotedView)
					{
						mc.Name = GetPivotedColNamePrefix(rtd.AssayMetadataResultTypeId);
						mcNamePrefix = mc.Name + "_"; // name prefix for all columns for this result type
					}

					else // unpivoted view
					{
						mc.Name = "RSLT_VALUE";  // name for basic value
						mcNamePrefix = "RSLT_"; // no prefix for other result cols
						mc.DetailsAvailable = true; // allow drilldown 
						mc.DataTypeImageName = "favorites"; // hilight key col
						mc.SinglePoint = mc.MultiPoint = true; // both SP and CRC results
						mc.PrimaryResult = true;
					}

					mc.Label = rtd.Name;
					if (mc.Label == "") mc.Label = mc.Name;

					if (mc.SummarizationRole == SummarizationRole.Derived)
						mc.Label += " (Well Level)"; // indicate that this is well-level activity detail

					if (Lex.Ne(mc.Units, "Mltiple"))
						mc.Units = rtd.UomName;

					if (rtd.UomName == "%") // prepend percent  units
					{
						mc.Label = "% " + mc.Label;
						mc.ShortLabel += "% " + mc.ShortLabel;
					}

					else if (Lex.IsDefined(rtd.UomName) && Lex.Ne(rtd.UomName, "Mltiple") && !unpivotedView) // append other single unit names
					{
						mc.Label += " (" + rtd.UomName + ")";
						mc.ShortLabel += " (" + rtd.UomName + ")";
					}

					//if (mc.ResultCode == "13706" || mc.ResultCode == "13708") mc = mc; // debug

					//if (mc.Name == "R_13404" || mc.ResultCode == "106461") mc = mc; // debug

					mc.TableMap = AssaySqlElements.RsltSubqueryName;

					int nbrNN = rtd.ValueTxtNonnullCount;
					int nbrCard = rtd.ValueNbrCardinality;
					int txtNN = rtd.ValueTxtNonnullCount;
					int txtCard = rtd.ValueTxtCardinality;

					if // check if this should be a text type field 
						((Lex.Eq(rtd.SummarizationMethod, "None") && txtCard > 0 && ((double)nbrCard) / txtCard < .9) || // no summarization && < 90% of values are numbers?
						QueryEngineLibrary.AssayResultType.IsTextResultType(mc) || // check special exception list
						(txtNN > 0 && nbrNN == 0)) // text values but no numeric
					{ // treat as a string type
						mc.DataType = MetaColumnType.String;
						mc.ColumnMap = "value_txt";
					}

					else // normal qualified number
					{
						mc.DataType = MetaColumnType.QualifiedNo;
						mc.ColumnMap = "value_nbr";
					}

					if (unsummarized) mc.Width = 12;
					else mc.Width = 13; // bit more width for stats

					mc.Format = ColumnFormatEnum.Default;
					mc.Decimals = -1;

					bool mcSummarizedExists = // summary data exists for column if:
						(mc.SummarizationRole == SummarizationRole.Dependent && // detailed result (activity results not summarized)
						rtd.ResultCountSummarized > 0 && // summarized data exists
						Lex.IsDefined(rtd.SummarizationMethod) && Lex.Ne(rtd.SummarizationMethod, "None")); // summarization method is defined'

					//if (unpivotedDetailLevelView) mcSummarizedExists = true; // always exists for unpivoted detail view (no)
					mc.SummarizedExists = mcSummarizedExists; // summarized data exists for column

					string keyResultType = "";
					if (rtd.AssayMetadataResultTypeId > 0)
					{
						string assayMetadataTypeString = rtd.AssayMetadataResultTypeId.ToString();
						if (QueryEngineLibrary.AssayResultType.KeyResultTypes.ContainsKey(assayMetadataTypeString)) keyResultType = assayMetadataTypeString;  // key result type? (check AssayMetadata code)
					}

					if (Lex.IsDefined(keyResultType))
					{
						string type = QueryEngineLibrary.AssayResultType.KeyResultTypes[keyResultType];
						if (Lex.Eq(type, "CRC"))
						{ // CRC is primary if exists, May be two CRC values in which case the one with the largest result count is primary
							mc.MultiPoint = true;
							if (rtd.ValueNbrNonnullCount > pMcNonnullCount)
							{
								primaryMc = mc;
								pMcNonnullCount = rtd.ValueNbrNonnullCount;
							}
						}
						else if (Lex.Eq(type, "SP"))
						{
							mc.SinglePoint = true;
							if (rtd.ValueNbrNonnullCount > sMcNonnullCount)
							{
								secondaryMc = mc;
								sMcNonnullCount = rtd.ValueNbrNonnullCount;
							}
						}

						mc.DataTypeImageName = "favorites"; // hilight key col
						mc.DetailsAvailable = true; // allow drilldown for all primary results
					}

					mc.InitialSelection = ColumnSelectionEnum.Selected; // selected by default
					mtc = mt; // if selected add to main table now

					if (QueryEngineLibrary.AssayResultType.IsDeselectedResultType(mc)) // is column to be deselected by default?
						mc.InitialSelection = ColumnSelectionEnum.Unselected;

					else if (mc.SummarizationRole == SummarizationRole.Derived) // set visibility of cols from the activity table (i.e. SP data for CRC assays) 
					{
						mc.InitialSelection = WellLevelColVisibility;
						mtc = mt2; // put at end
					}

					mtc.AddMetaColumn(mc);

					if (QueryEngineLibrary.AssayResultType.IsPotencyColumn(mc) && potencyMc == null) // save first potency metacolumn
						potencyMc = mc;

					// Units column - add if multiple units

					bool includeUnits = (Lex.Eq(rtd.UomName, "Mltiple") || unpivotedView);

					if (includeUnits)
					{
						mcName = mcNamePrefix + "UNITS";
						mcc = mtc.AddMetaColumn(mcName, "Units", MetaColumnType.String, mc.InitialSelection, 5, AssaySqlElements.RsltSubqueryName, "rslt_units");
						mcc.Dictionary = "assay_uom";

						if (pivotedView) mcc.Label = mc.Label + " Units";

						if (rtd.AssayResultTypeId > 0)
						{
							mcc.ResultCode = rtd.AssayResultTypeId.ToString();
							mcc.ResultCode2 = rtd.AssayMetadataResultTypeId.ToString(); // store AssayMetadata result type code in code2
						}
					}

					// Add CRC image column if appropriate

					//if (mc.ResultCode == "13706" || mc.ResultCode == "13708") mc = mc; // debug

					//bool useSingleImageColumn = true; // combine multiple col value cols in single image column
					//bool useMultipleImageColumns = false; // multiple image cols allowed flag (define further (e.g. IC50_NS, EC50_NS)?)

					bool includeImage = QueryEngineLibrary.AssayResultType.CanHaveRelatedImageColumn(mc);
					if (includeImage && crcMc != null && !allowSecondaryImageColumns) // if already have image col, see if secondary allowed
						includeImage = false;

					if (includeImage)
					{
						string crcMcName = "rslt_img_url"; // default name
						string crcMcLabel = "Response Curve";
						if (allowSecondaryImageColumns) // use qualifying label if more than one col allowed
							crcMcLabel = mc.Label + " " + crcMcLabel;

						//if (crcMc == null) // || useMultipleImageColumns) // add new image column

						if (crcMc != null) // if already exists qualify this additional by result type (allows multiple of same result type per assay)
						{
							crcMcName = mcNamePrefix + crcMcName;
						}

						crcMc = new MetaColumn(crcMcName, crcMcLabel, MetaColumnType.Image, ColumnSelectionEnum.Unselected, 40);
						if (summarized) crcMc.Width *= 2; // wider if summarized and overlays shown
						crcMc.TableMap = mc.TableMap;
						crcMc.ColumnMap = "RSLT_ID"; // get detailed result id initially and then image at render time
						crcMc.ResultCode = mc.ResultCode;
						crcMc.ResultCode2 = mc.ResultCode2;
						crcMc.IsSearchable = false;
						crcMc.SummarizedExists = mc.SummarizedExists;
						mtc.AddMetaColumn(crcMc);

						//}

						//else // map multiple value columns to single image column
						//  crcMc.ResultCode += "," + mc.ResultCode;
					}

					// Add hidden columns that split qualified numbers into individual columns

					if (pivotedView) mcNameSuffix = "_qnd";
					else mcNameSuffix = "";

					bool includeDetails = (mc.DataType == MetaColumnType.QualifiedNo && MetaTableFactory.IncludeQualifiedNumberDetailColumnsInMetaTables);

					if (includeDetails)
					{
						mcc = AddHiddenResultDetailField(mtc, rtd, "value_nbr", mc.Label + " (Number)", mcNamePrefix, mcNameSuffix, MetaColumnType.Number, mc.Width, mc.Decimals, true, false); // unsummarized details
						mcc = AddHiddenResultDetailField(mtc, rtd, "prfx_txt", "Qualifier", mcNamePrefix, mcNameSuffix, MetaColumnType.String, 6, 0, true, false);
						mcc = AddHiddenResultDetailField(mtc, rtd, "value_txt", "Text", mcNamePrefix, mcNameSuffix, MetaColumnType.String, 8, 0, true, false);
					}

					// add additional cols available for summarized data

					if (mcSummarizedExists)
					{
						if (includeDetails) // add mean details
						{
							mcc = AddHiddenResultDetailField(mtc, rtd, "mean_value_nbr", "Mean " + mc.Label + " (Number)", mcNamePrefix, mcNameSuffix, MetaColumnType.Number, 8, mc.Decimals, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "mean_prfx_txt", "Mean Qualifier", mcNamePrefix, mcNameSuffix, MetaColumnType.String, 6, 0, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "mean_value_txt", "Mean Text", mcNamePrefix, mcNameSuffix, MetaColumnType.String, 8, 0, false, true);

							mcc = AddHiddenResultDetailField(mtc, rtd, "nbr_vals_cnsdrd", "N Considered", mcNamePrefix, mcNameSuffix, MetaColumnType.Integer, 6, 0, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "nbr_vals_incld", "N Included", mcNamePrefix, mcNameSuffix, MetaColumnType.Integer, 6, 0, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "nbr_exprmnts_incld", "N Runs", mcNamePrefix, mcNameSuffix, MetaColumnType.Integer, 6, 0, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "std_dvtn_nbr", "Std. Dev.", mcNamePrefix, mcNameSuffix, MetaColumnType.Number, 6, 3, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "std_err_nbr", "Std. Err.", mcNamePrefix, mcNameSuffix, MetaColumnType.Number, 6, 3, false, true);
						}

						if (includeUnits) // add mean units
						{
							mcName = mcNamePrefix + "MEAN_UNITS";
							mcc = mtc.AddMetaColumn(mcName, "Units", MetaColumnType.String, mc.InitialSelection, 5, AssaySqlElements.RsltSubqueryName, "rslt_mean_units");
							mcc.SummarizedExists = true; mcc.UnsummarizedExists = false;
							mcc.Dictionary = "assay_uom";
						}

						if (includeDetails) // add median details
						{
							mcc = AddHiddenResultDetailField(mtc, rtd, "median_value_nbr", "Median " + mc.Label + " (Number)", mcNamePrefix, mcNameSuffix, MetaColumnType.Number, 8, mc.Decimals, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "median_prfx_txt", "Median Qualifier", mcNamePrefix, mcNameSuffix, MetaColumnType.String, 6, 0, false, true);
							mcc = AddHiddenResultDetailField(mtc, rtd, "median_value_txt", "Median Text", mcNamePrefix, mcNameSuffix, MetaColumnType.String, 8, 0, false, true);

							if (includeUnits) // add median units
							{
								mcName = mcNamePrefix + "MEDIAN_UNITS";
								mcc = mtc.AddMetaColumn(mcName, "Median Units", MetaColumnType.String, Hidden, 5, AssaySqlElements.RsltSubqueryName, "rslt_median_units");
								mcc.SummarizedExists = true; mcc.UnsummarizedExists = false;
								mcc.Dictionary = "assay_uom";
							}
						}
					}

					// Concentration column

					if (rtd.ConcNbrNonnullCount > 0 || unpivotedView) // add if concentrations exist
					{
						if (pivotedView) mcName = mcNamePrefix + "CMPND_CONC_VAL_NBR"; // maintain old name for compatibility
						else mcName = mcNamePrefix + "CONC_NBR"; // shorter name for unpivoted

						mcc = mtc.AddMetaColumn(mcName, "Conc.", MetaColumnType.Number, mc.InitialSelection, 7, AssaySqlElements.RsltSubqueryName, "CONC_NBR");
						mcc.MetaTable = mt;
						mcc.ResultCode = mc.ResultCode;
						mcc.ResultCode2 = mc.ResultCode2;
						mcc.SummarizationRole = mc.SummarizationRole;
						mcc.Concentration = true;

						if (Lex.IsDefined(rtd.ConcUomName) && Lex.Ne(rtd.ConcUomName, "Mltiple") && !unpivotedView) // append conc units to col name if not multiple
						{
							mcc.Label += " (" + rtd.ConcUomName + ")";
							mcc.ShortLabel += " (" + rtd.ConcUomName + ")";
						}

						if (pivotedView && assayData.ResultTypeData.Count > 1) // if pivoted & more than one result type, qualify conc col name with basic result type name
							mcc.Label = mc.Label + " " + mcc.Label;

						if (rtd.ConcNbrNonnullCountSummarized > 0 || rtd.SubgroupOnConc || unpivotedView) // summarized conc exists
							mcc.SummarizedExists = mc.SummarizedExists; // summarized data exists for column

						mcName = mcNamePrefix + "CONC_UNITS"; // conc units (normally hidden)
						mcc2 = mtc.AddMetaColumn(mcName, "Conc. Units", MetaColumnType.String, ColumnSelectionEnum.Hidden, 5, AssaySqlElements.RsltSubqueryName, "conc_units");
						mcc2.MetaTable = mt;
						mcc2.Dictionary = "assay_uom";
						mcc2.ResultCode = mc.ResultCode;
						mcc2.ResultCode2 = mc.ResultCode2;
						mcc2.SummarizationRole = mc.SummarizationRole;
						mcc2.SummarizedExists = mcc.SummarizedExists;

						if (Lex.Eq(rtd.ConcUomName, "Mltiple") || unpivotedView) // show conc units col if multiple units exist or unpivoted
							mcc2.InitialSelection = mc.InitialSelection;
					}

					// Add "Excluded Flag" result detail from ASSAY_ACTVTY_RSLT

					if (mc.SummarizationRole == SummarizationRole.Derived || unpivotedView)
					{
						mcc = AddHiddenResultDetailField(mtc, rtd, "excld_flg", "Excluded Flag", mcNamePrefix, "", MetaColumnType.String, 6, 0, true, false);
						mcc.HorizontalAlignment = HorizontalAlignmentEx.Center;
						mcc.Dictionary = "yes_no";
					}

					// Result comments column

					if (rtd.CommentNonnullCount > 0) // add if comments exist
					{
						mcc = new MetaColumn();
						mcc.MetaTable = mt;
						mcc.ResultCode = mc.ResultCode;
						mcc.ResultCode2 = mc.ResultCode2;
						mcc.SummarizationRole = mc.SummarizationRole;

						mcc.Name = mcNamePrefix + "CMNT_TXT";

						mcc.Label = "Result Comments";
						if (pivotedView && assayData.ResultTypeData.Count > 1) // if more than one result type, qualify name
							mcc.Label = mc.Label + " Comments";

						mcc.TableMap = mc.TableMap;
						mcc.ColumnMap = "CMNT_TXT";

						mcc.DataType = MetaColumnType.String;
						mcc.Width = 12;

						mcc.InitialSelection = ColumnSelectionEnum.Hidden; // hide by default

						if (unpivotedView) mcc.InitialSelection = ColumnSelectionEnum.Unselected; // but not for unpivoted view

						else if (mc.SummarizationRole == SummarizationRole.Dependent && resultCommentMc == null)
						{ // if first detail result type comment then add additional visible column for this
							resultCommentMc = mcc.Clone();
							mcc2 = resultCommentMc;
							mcc2.InitialSelection = ColumnSelectionEnum.Unselected;
							mcc2.Name = "RSLT_CMNT_TXT";
							mcc2.Label = "Result Comments";
							mtc.AddMetaColumn(resultCommentMc); // add here, gets repositioned later according to Assay preferred column order
						}

						else if (mc.SummarizationRole == SummarizationRole.Derived && wellLevelCommentMc == null)
						{ // if first activity result type comment then make visible
							wellLevelCommentMc = mcc;
							mcc.InitialSelection = WellLevelColVisibility;
							mcc.Label = "Result Comments (Well Level)";
						}

						mtc.AddMetaColumn(mcc);
					}

					// Add result validity column

					mcc = AddHiddenResultDetailField(mtc, rtd, "APRVL_STS_CD", "Result Is Valid", mcNamePrefix, "", MetaColumnType.String, 5, 0, true, false);
					mcc.HorizontalAlignment = HorizontalAlignmentEx.Center;
					mcc.Dictionary = "yes_no";

					if (pivotedView && assayData.ResultTypeData.Count > 1) // if more than one result type, qualify name
						mcc.Label = mc.Label + " Valid";

					mcc.InitialSelection = ColumnSelectionEnum.Hidden; // hide by default
					if (unpivotedView) mcc.InitialSelection = ColumnSelectionEnum.Unselected; // but not for unpivoted view

					else if (mc.SummarizationRole == SummarizationRole.Dependent && resultValidityMc == null)
					{ // if first detail result type then add additional visible column for this
						resultValidityMc = mcc.Clone();
						mcc2 = resultValidityMc;
						mcc2.InitialSelection = ColumnSelectionEnum.Unselected;
						mcc2.Name = "RSLT_APRVL_STS_CD";
						mcc2.Label = "Result is Valid";
						mtc.AddMetaColumn(resultValidityMc); // add here, gets repositioned later according to Assay preferred column order
					}

					else if (mc.SummarizationRole == SummarizationRole.Derived && wellLevelCommentMc == null)
					{ // if first activity result type comment then make visible
						wellLevelCommentMc = mcc;
						mcc.InitialSelection = WellLevelColVisibility;
						mcc.Label = "Result is Valid (Well Level)";
					}

					// Add result classification columns 

					if (unpivotedView)
					{
						mcc = AddUnpivotedMetaColumn(mt, "activity_class", "Activity Class", MetaColumnType.String, Hidden, 10,
							AssaySqlElements.CaaResultTypeAttrTable, AssaySqlElements.ActivityClassSqlExpression);
						mcc.DataTypeImageName = "CondFormatSmall";
						mcc.CondFormat = UnpivotedAssayResult.BuildActivityClassCondFormat();

						mcc = AddUnpivotedMetaColumn(mt, "activity_bin", "Activity Bin", MetaColumnType.Integer, Unselected, 4,
						 AssaySqlElements.CaaResultTypeAttrTable, AssaySqlElements.ActivityBinSqlExpression);
						mcc.DataTypeImageName = "CondFormatSmall";
						mcc.CondFormat = UnpivotedAssayResult.BuildActivityBinCondFormat();

						//  mcc = AddUnpivotedMetaColumn(mt, "cond_fmt_color", "Conditional Formatting Color", MetaColumnType.Integer, Hidden, 10, ColumnFormatEnum.Decimal, 0, "tm", "cm");
					}

					// Add misc hidden columns for result

					mcc = AddHiddenResultDetailField(mtc, rtd, "msrmnt_tmstmp", "Measurement Date", mcNamePrefix, "", MetaColumnType.Date, 9, 0, true, false);
					mcc = AddHiddenResultDetailField(mtc, rtd, "crtd_dt", "Result DB Insert Date", mcNamePrefix, "", MetaColumnType.Date, 9, 0, true, true);
					mcc = AddHiddenResultDetailField(mtc, rtd, "mdfd_dt", "Result DB Updated Date", mcNamePrefix, "", MetaColumnType.Date, 9, 0, true, true);

					//if (Lex.Eq(rtd.SourceTable, "dtld")) // count result types with image data
					//{ 
					//  if (rtd.ImageNonnullCount > 0) crcImageCount++; // have data for image

					//  else if (rtd.ResultCount == 0 && mt.Label.ToUpper().IndexOf(" CRC") > 0)
					//    crcImageCount++; // also include if no data for assay but CRC in label
					//}

					if (rtd.TargetSymbol != "") // extra attributes from cmn_assy_atrbts
					{
						mt.TargetSymbol = rtd.TargetSymbol;
						mt.TargetId = rtd.TargetId;
						mt.GeneFamily = rtd.GeneFamily;
						mt.AssayType = rtd.AssayType;
						mt.AssayMode = rtd.AssayMode;
						mt.AssayLocation = rtd.AssayLocation;
					}

					bool merged = (TryResultTypeMerge(mc, mt) || TryResultTypeMerge(mc, mt2)); // merge this result type with an existing type if appropriate

				} // end of result type loop

				// Be sure expected stats are present if potency col seen

				if (potencyMc != null && pivotedView)
				{
					mtc = potencyMc.MetaTable;
					mcc = AddExpectedStatColumn(mtc, 106347, -86, "95% LCL");
					mcc = AddExpectedStatColumn(mtc, 106346, -85, "95% UCL");
					mcc = AddExpectedStatColumn(mtc, 106306, -5, "MCR");
				}

				mt.TableMap = AssaySqlElements.RsltSubqueryName; // all map to assay_rslt with clause


				mcc = mt.AddMetaColumn("strt_dt", "Run Date", MetaColumnType.Date, Unselected, 10, AssaySqlElements.Run, "start_dt");
				mcc = mt.AddMetaColumn("end_dt", "Run End Date", MetaColumnType.Date, Hidden, 10, AssaySqlElements.Run, "end_dt");
				mcc = mt.AddMetaColumn("dose_dt", "Dose Date", MetaColumnType.Date, Hidden, 10, AssaySqlElements.Run, "dose_dt");
				mcc = mt.AddMetaColumn("read_dt", "Read Date", MetaColumnType.Date, Hidden, 10, AssaySqlElements.Run, "read_dt");

				mcc = mt.AddMetaColumn("run_nbr", "Run Number (Obsolete)", MetaColumnType.Integer, Hidden, 10, AssaySqlElements.Run, "null");
				mcc = mt.AddMetaColumn("rslt_ntbk_lbl_txt", "Assayer Notebook Reference", MetaColumnType.String, Unselected, 10, AssaySqlElements.Run, "ntbk_rfrnc_txt");

				mcc.ClickFunction = "ShowElnPage()";

				mcc = mt.AddMetaColumn("op_systm_nm", "Run Operational System Name", MetaColumnType.String, Unselected, 10, "assay_owner.assay_oprtnl_systm", "dscrptn_txt");
				mcc = mt.AddMetaColumn("site_nm", "Run Site Name", MetaColumnType.String, Unselected, 10, "assay_owner.assay_site", "dscrptn_txt");
				mcc = mt.AddMetaColumn("lab_nm", "Run Lab Name", MetaColumnType.String, Unselected, 10, "assay_owner.assay_lab", "dscrptn_txt");
				mcc = mt.AddMetaColumn("assyr_prsn_full_nm", "Assayer Name", MetaColumnType.String, Unselected, 10, "person_table_owner.dit_person", "full_name");
				mcc = mt.AddMetaColumn("pblshr_prsn_full_nm", "Run Publisher", MetaColumnType.String, Unselected, 10, "person_table_owner.dit_person2", "full_name");
				mcc = mt.AddMetaColumn("pblctn_trnsctn_nm", "Publication Transaction", MetaColumnType.String, Unselected, 10, "assay_owner.assay_pblctn_trnsctn", "pblctn_trnsctn_nm");
				mcc = mt.AddMetaColumn("src_run_id", "Publication Run", MetaColumnType.String, Unselected, 10, "assay_owner.assay_pblctn_trnsctn_run", "src_run_id");
				mcc = mt.AddMetaColumn("cmnt_txt", "Run Comments", MetaColumnType.String, Unselected, 10, AssaySqlElements.Run, "cmnt_txt");

				mcc = mt.AddMetaColumn("run_crtd_dt", "Run DB Insert Date", MetaColumnType.Date, Hidden, 10, AssaySqlElements.Run, "crtd_dt");
				mcc = mt.AddMetaColumn("run_mdfd_dt", "Run DB Update Date", MetaColumnType.Date, Hidden, 10, AssaySqlElements.Run, "mdfd_dt");
				mcc = mt.AddMetaColumn("run_id", "Run_Id", MetaColumnType.Integer, Hidden, 10, AssaySqlElements.Run, "run_id");

				// Add lot/submission columns

				mcc = mt.AddMetaColumn("ntbk_rfrnc_txt", "Lot Number", MetaColumnType.String, Unselected, 16, AssaySqlElements.SubmissionIdLotIdCidId, "ntbk_rfrnc");
				//mcc = mt.AddMetaColumn("ntbk_rfrnc_txt", "Lot Number", MetaColumnType.String, Unselected, 16, AssayTable.Lot, "lot_nbr");
				mcc = mt.AddMetaColumn("corp_sbmsn_id", "Sample Submission Id", MetaColumnType.Integer, Unselected, 10, AssaySqlElements.RsltSubqueryName, "sbmsn_id");
				mcc.ClickFunction = "ShowSubmissionRegistrationHistory"; // allow drilldown on submission

				// Add misc hidden columns

				mcc = mt2.AddMetaColumn("rslt_grp_id", "Result Group Id", MetaColumnType.Integer, Hidden, 10, AssaySqlElements.RsltSubqueryName, "rslt_grp_id"); // result group id
				//mcc = AddUnpivotedMetaColumn(mt2, "rslt_id", "Result Id", MetaColumnType.Integer, Hidden, 10, AssayOracleElements.RsltSubqueryName, "rslt_id"); // (no, rslt_id is not common for the result group)

				if (unpivotedView) // add misc unpivoted hidden cols
				{
					mcc = AddUnpivotedMetaColumn(mt2, "rslt_id", "Result Id", MetaColumnType.Integer, Hidden, 10, AssaySqlElements.RsltSubqueryName, "rslt_id");

					mcc = AddGeneAttrMetaColumn(mt2, "TARGET_MAP_X", Hidden);
					mcc.ColumnMap = "X";
					mcc = AddGeneAttrMetaColumn(mt2, "TARGET_MAP_Y", Hidden);
					mcc.ColumnMap = "Y";
				}

				if (primaryMc != null)
					primaryMc.PrimaryResult = true;

				if (secondaryMc != null)
				{
					if (primaryMc == null) // make this primary if no primary seen
						secondaryMc.PrimaryResult = true;
					else secondaryMc.SecondaryResult = true;
				}

				// Indicate if we should join on concentration for this assay
				// Join on conc to separate results if % inh type assay with possibly multiple concentrations and other join vars the same

				for (int ci = 0; ci < assayData.ResultTypeData.Count; ci++)
				{
					rtd = assayData.ResultTypeData[ci];
					if (rtd.SubgroupOnConc)
					{
						mt.JoinOnConcentration = true;
						break;
					}
				}

				// Apply preferred ordering of columns

				bool applyPreferredOrdering = pivotedView; // use prefered ordering only if pivoted view (e.g. not unpivoted)

				if (applyPreferredOrdering)
				{

					int npi = 200000; // not preferred index starts here

					foreach (MetaColumn mc0 in mt.MetaColumns) // assign relative positions
					{
						//if (assayData.AssayId == 12032) assayData = assayData; // debug
						//if (Lex.Contains(mc0.Name, "RSLT_IMG_URL")) npi = npi; // debug

						if (mc0.IsKey) mc0.Position = 0; // always keep key first

						// Check for other matches in the following order
						// 
						else
						{
							dictKey = "";

							Dictionary<string, int> dict = QueryEngineLibrary.AssayResultType.ExtraResultTypeAttributes;
							string assay = "ASSAY_" + assayData.AssayId + "."; // Prefix if assay specific
							assayMetadataCode = MetaColumn.FirstResultCode(mc0.ResultCode2);

							if (dict.ContainsKey(assay + mc0.Name))
								dictKey = assay + mc0.Name;

							else if (dict.ContainsKey(mc0.Name))
								dictKey = mc0.Name;

							else if (Lex.IsDefined(assayMetadataCode)) // if AssayMetadata code is defined use it as key
							{
								if (dict.ContainsKey(assay + assayMetadataCode))
									dictKey = assay + assayMetadataCode;

								else if (dict.ContainsKey(assayMetadataCode))
									dictKey = assayMetadataCode;
							}

							if (dict.ContainsKey(dictKey)) // preferred position
							{
								mc0.Position = dict[dictKey];
								if (!dictKey.Contains(".")) // use higher number range to sort later if not metatable specific
									mc0.Position += 100000;
							}

							else // no preferred position, put at end in order
							{
								mc0.Position = npi;
								npi++;
							}
						}
					}

					for (i1 = 1; i1 < mt.MetaColumns.Count; i1++) // sort by preferred position
					{
						mc = mt.MetaColumns[i1];
						for (i2 = i1 - 1; i2 >= 0; i2--)
						{
							if (mt.MetaColumns[i2].Position <= mc.Position) break;
							mt.MetaColumns[i2 + 1] = mt.MetaColumns[i2];
						}

						mt.MetaColumns[i2 + 1] = mc;
					}
				}

				// Append mt2 secondary cols to end

				foreach (MetaColumn mc2 in mt2.MetaColumns) // add non-selected results columns
				{
					mt.AddMetaColumn(mc2);
					mc2.MetaTable = mt; // point to proper metatable
				}

				// Finish up

				MetaTableFactory.AdjustForSummarization(mt, summarized);
				if (summarized) AdjustMapsForSummarization(mt);

				MetaTableFactory.SetAnyNewMetaTableLabel(mt); // if table has been renamed the set new label
				foreach (MetaColumn mc2 in mt.MetaColumns)
					MetaTableFactory.SetAnyNewMetaColumnLabel(mc2); // if col has been renamed then set new label

				if (assayData.MergeDefTable != null)
				{ // if this is a merged ASSAY metatable then store mapping information
					int tableId;
					bool summary;

					mt.SummarizedExists = false; // can't view summarized data
					mt.Description = ""; // no description
					mt.TableFilterValues = new List<string>();
					MetaTable mmt = assayData.MergeDefTable;
					string[] sa = mmt.TableMap.Split(','); // get list of assoc metatable names
					foreach (string s in sa)
					{
						MetaTable.ParseMetaTableName(s.Trim(), out tableId, out summary);
						mt.TableFilterValues.Add(tableId.ToString());
					}
				}

				GetMetaTableCount++;
				t0 = TimeOfDay.Milliseconds() - t0;
				GetMetaTableTime += t0;
				GetMetaTableAvgTime = GetMetaTableTime / GetMetaTableCount;

				return mt;
			}

			catch (Exception ex)
			{
				string msg = "Error building ASSAY MetaTable: " + mtName;
				DebugLog.Message(msg);
				throw new Exception(msg, ex);
			}
		}

		/// <summary>
		/// AddResultTypeAttrMetaColumn
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mcName"></param>
		/// <param name="initialSelection"></param>
		/// <returns></returns>

		MetaColumn AddResultTypeAttrMetaColumn(
			MetaTable mt,
			string mcName,
			ColumnSelectionEnum initialSelection)
		{
			return AddCommonAttrMetaColumn(mt, mcName, initialSelection, AssaySqlElements.CaaResultTypeAttrTable);
		}

		/// <summary>
		/// AddAssayAttrMetaColumn
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mcName"></param>
		/// <param name="initialSelection"></param>
		/// <returns></returns>

		MetaColumn AddAssayAttrMetaColumn(
			MetaTable mt,
			string mcName,
			ColumnSelectionEnum initialSelection)
		{
			return AddResultTypeAttrMetaColumn(mt, mcName, initialSelection); // treat everything as result type attribute
		}

		/// <summary>
		/// AddGeneAttrMetaColumn
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mcName"></param>
		/// <param name="initialSelection"></param>
		/// <returns></returns>

		MetaColumn AddGeneAttrMetaColumn(
			MetaTable mt,
			string mcName,
			ColumnSelectionEnum initialSelection)
		{
			return AddResultTypeAttrMetaColumn(mt, mcName, initialSelection); // treat everything as result type attribute
		}

		/// <summary>
		/// Add assay Attr metacolumn to a metatable
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mcName"></param>
		/// <param name="initialSelection"></param>
		/// <returns></returns>

		MetaColumn AddCommonAttrMetaColumn(
			MetaTable mt,
			string mcName,
			ColumnSelectionEnum initialSelection,
			string tableMap)
		{
			string caaMtName = UnpivotedAssayView.AssayAttributesMetaTableName;
			MetaTable aMt = MetaTableCollection.Get(caaMtName);

			if (aMt == null)
			{
				DebugLog.Message("Can't find table: " + caaMtName);
				return null;
			}

			MetaColumn aMc = aMt.GetMetaColumnByName(mcName);
			if (aMc == null)
			{
				DebugLog.Message("Can't find column " + mcName);
				return null;
			}

			MetaColumn mc = aMc.Clone();

			if (initialSelection != ColumnSelectionEnum.Unknown)
				mc.InitialSelection = initialSelection;

			mc.SummarizedExists = true;
			mc.TableMap = tableMap;
			mc.ColumnMap = mcName;

			mt.AddMetaColumn(mc);

			return mc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="name"></param>
		/// <param name="label"></param>
		/// <param name="dataType"></param>
		/// <param name="displayLevel"></param>
		/// <param name="displayWidth"></param>
		/// <param name="tableMap"></param>
		/// <param name="columnMap"></param>
		/// <returns></returns>

		MetaColumn AddUnpivotedMetaColumn(
			MetaTable mt,
			string name,
			string label,
			MetaColumnType dataType,
			ColumnSelectionEnum displayLevel,
			float displayWidth,
			string tableMap,
			string columnMap)
		{
			MetaColumn mc = mt.AddMetaColumn(name, label, dataType, displayLevel, displayWidth, tableMap, columnMap);
			mc.SummarizedExists = true;
			return mc;
		}

		/// <summary>
		/// Get prefix for pivoted column that includes the result type code
		/// </summary>
		/// <param name="rtd"></param>
		/// <returns></returns>

		static string GetPivotedColNamePrefix(int assayMetadataResultTypeId)
		{
			string prefix;

			if (assayMetadataResultTypeId > 0) // R_xxx where xxx is assay_rslt_typ_id (dim = 70)
				prefix = "R_" + assayMetadataResultTypeId;

			else if (assayMetadataResultTypeId < 0) // S_xxx where xxx is rslt_stat_typ_id (dim = 71)
			{
				int refStatTypId = -assayMetadataResultTypeId;
				prefix = "S_" + refStatTypId;
			}

			else throw new Exception("Undefined result type");

			return prefix;
		}

		/// <summary>
		/// Get AssayMetadata ResultTypeId associated with MetaColumn
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static int GetAssayMetadataResultTypeId(MetaColumn mc)
		{
			int assayMetadataTypeId;

			if (Lex.IsUndefined(mc.ResultCode2)) return 0;

			string assayMetadataTypeIdString = MetaColumn.FirstResultCode(mc.ResultCode2);
			if (int.TryParse(assayMetadataTypeIdString, out assayMetadataTypeId))
				return assayMetadataTypeId;

			else return 0;
		}

		/// <summary>
		/// Attempt to merge the specified metacolumn with a metacolumn already in the metatable
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="mt"></param>
		/// <returns>Added MetaColumn that is merged</returns>

		bool TryResultTypeMerge(
			MetaColumn mc,
			MetaTable mt)
		{
			bool primary;

			MetaColumn mc0 = GetMetaColumnToMergeWith(mc, mt, out primary);
			if (mc0 == null) return false;

			string appendedResultCodes = mc0.ResultCode + "," + mc.ResultCode; // appended list of assay result codes
			string appendedResultCodes2 = mc0.ResultCode2 + "," + mc.ResultCode2; // get appended list of assayMetadata result codes

			string mmcName = mc0.Name + Lex.Replace(mc.Name, "_", ""); // catenate removing extra underscores
			string mmcLabel = mc0.Label + ", " + mc.Label;

			List<MetaColumn> addedMcList = new List<MetaColumn>();

			foreach (MetaColumn mc2 in mt.MetaColumns) // update result code list & plug primary names and labels into all metacolumns that match the mmcResultCode
			{
				//if (Lex.Contains(mc2.Name, "img")) mmcName = mmcName; // debug
				if (mc2.ResultCode != mc0.ResultCode) continue; // || mc2.Name == mmcName || mc2.Name.StartsWith(mmcName + "_")

				//if (mc2.InitialSelection == ColumnSelectionEnum.Hidden) continue;

				MetaColumn mmc = mc2.Clone();
				mmc.ResultCode = appendedResultCodes; // Lex.Replace(mc2.ResultCode, mmcResultCode, appendedResultCodes);
				mmc.ResultCode2 = appendedResultCodes2;

				//if (primary) // if mc is primary then substitute its names also
				//{
				if (Lex.Contains(mc2.Name, "rslt_img_url")) mmc.Name = mmcName + "_rslt_img_url"; // handle special CRC col naming
				else mmc.Name = Lex.Replace(mc2.Name, mc0.Name, mmcName);

				mmc.Label = Lex.Replace(mc2.Label, mc0.Label, mmcLabel);
				//}

				addedMcList.Add(mmc);
			}

			foreach (MetaColumn mc2 in addedMcList) // update result code list & plug primary names and labels into all metacolumns that match the mmcResultCode
			{
				mt.AddMetaColumn(mc2);
			}

			return true;
		}

		/// <summary>
		/// Get any existing metacolumn that this code should be merged with 
		/// </summary>
		/// <param name="resultCode"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		MetaColumn GetMetaColumnToMergeWith(
			MetaColumn mc,
			MetaTable mt,
			out bool primary)
		{
			string resultCode;
			List<string> mergeWithCodes;
			primary = true; // true if mc.Code is listed as first result type in AssayMergedResultTypes

			if (!mc.Name.StartsWith("R_") || Lex.IsUndefined(mc.ResultCode)) return null;

			resultCode = mc.Name.Substring(2);

			if (QueryEngineLibrary.AssayResultType.MergedResultTypes == null) QueryEngineLibrary.AssayResultType.ReadMergedResultTypes();

			string code1 = mt.Code + "," + resultCode; // specific to assay
			string code2 = "*," + resultCode; // general to all assays

			if (QueryEngineLibrary.AssayResultType.MergedResultTypes.ContainsKey(code1))
				mergeWithCodes = QueryEngineLibrary.AssayResultType.MergedResultTypes[code1];

			else if (QueryEngineLibrary.AssayResultType.MergedResultTypes2.ContainsKey(code1))
			{
				mergeWithCodes = QueryEngineLibrary.AssayResultType.MergedResultTypes2[code1];
				primary = false;
			}

			else if (QueryEngineLibrary.AssayResultType.MergedResultTypes.ContainsKey(code2))
				mergeWithCodes = QueryEngineLibrary.AssayResultType.MergedResultTypes[code2];

			else if (QueryEngineLibrary.AssayResultType.MergedResultTypes2.ContainsKey(code2))
			{
				mergeWithCodes = QueryEngineLibrary.AssayResultType.MergedResultTypes2[code2];
				primary = false;
			}

			else return null;

			for (int i1 = 0; i1 < mt.MetaColumns.Count; i1++) // see if match exists
			{
				MetaColumn mc2 = mt.MetaColumns[i1];
				foreach (string mergeWithCode in mergeWithCodes)
				{
					string mergeWithName = "R_" + mergeWithCode;
					if (Lex.Eq(mc2.Name, mergeWithName)) return mc2;
				}
			}

			return null;
		}

		/// <summary>
		/// Adjust table & column maps for summarized view
		/// </summary>
		/// <param name="mt"></param>

		void AdjustMapsForSummarization(MetaTable mt)
		{
			foreach (MetaColumn mc in mt.MetaColumns)
			{
				// Adjust table and col maps for specific table/col combinations

				AdjustMap(mc, AssaySqlElements.RsltSubqueryName, "value_nbr", AssaySqlElements.RsltSubqueryName, "mean_value_nbr");
				AdjustMap(mc, AssaySqlElements.RsltSubqueryName, "prfx_txt", AssaySqlElements.RsltSubqueryName, "mean_prfx_txt");
				AdjustMap(mc, AssaySqlElements.RsltSubqueryName, "value_txt", AssaySqlElements.RsltSubqueryName, "mean_value_txt");
				AdjustMap(mc, AssaySqlElements.RsltSubqueryName, "rslt_id", AssaySqlElements.RsltSubqueryName, "smrzd_rslt_id");

				if (Lex.Eq(mc.Name, "activity_class"))
					mc.ColumnMap = AssaySqlElements.AdjustForSummarizedView(AssaySqlElements.ActivityClassSqlExpression);

				if (Lex.Eq(mc.Name, "activity_bin"))
					mc.ColumnMap = AssaySqlElements.AdjustForSummarizedView(AssaySqlElements.ActivityBinSqlExpression);
			}

			return;
		}


		/// <summary>
		/// AdjustMap
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="unsummarizedTableMap"></param>
		/// <param name="unsummarizedColumnMap"></param>
		/// <param name="summarizedTableMap"></param>
		/// <param name="summarizedColumnMap"></param>

		void AdjustMap(
			MetaColumn mc,
			string unsummarizedTableMap,
			string unsummarizedColumnMap,
			string summarizedTableMap,
			string summarizedColumnMap)
		{
			if (Lex.Eq(mc.TableMap, unsummarizedTableMap) || Lex.Eq(mc.TableMap, summarizedTableMap)) // match unsummarized table name or summarized table name that is already adjusted
			{
				if (Lex.Eq(mc.TableMap, unsummarizedTableMap)) mc.TableMap = summarizedTableMap; // change table map (column name need not match)

				if (Lex.Eq(mc.ColumnMap, unsummarizedColumnMap)) mc.ColumnMap = summarizedColumnMap; // change column map
			}

			return;
		}

		/// <summary>
		/// Get number of data rows from stats that are updated nightly
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static int TableRowCount(string mtName)
		{
			Dictionary<string, MetaTableStats> stats = GetMetaTableStats();

			mtName = mtName.ToUpper();
			if (Lex.EndsWith(mtName, "_SUMMARY")) // if summary name remove suffix to get approximate value
				mtName = Lex.Replace(mtName, "_SUMMARY", "");

			if (!stats.ContainsKey(mtName)) return 0;
			else return (int)stats[mtName].RowCount;
		}

		/// <summary>
		///  Add a hidden field spliting out one of detail columns from the result table
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="rtd"></param>
		/// <param name="columnMap"></param>
		/// <param name="label"></param>
		/// <param name="mcNameSuffix"></param>
		/// <param name="dataType"></param>
		/// <param name="displayWidth"></param>
		/// <param name="decimals"></param>
		/// <param name="unsummarizedExists"></param>
		/// <param name="summarizedExists"></param>
		/// <returns></returns>

		MetaColumn AddHiddenResultDetailField(
			MetaTable mt,
			ResultTypeData rtd,
			string columnMap,
			string label,
			string mcNamePrefix,
			string mcNameSuffix,
			MetaColumnType dataType,
			float displayWidth,
			int decimals,
			bool unsummarizedExists,
			bool summarizedExists)
		{
			ColumnSelectionEnum defaultSelection = ColumnSelectionEnum.Hidden;
			MetaColumn mc = AddResultDetailField(mt, rtd, columnMap, label, mcNamePrefix, mcNameSuffix, dataType, displayWidth, decimals, defaultSelection, unsummarizedExists, summarizedExists);
			return mc;
		}

		/// <summary>
		///  Add a field spliting out one of detail columns from the result table
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="rtd"></param>
		/// <param name="columnMap"></param>
		/// <param name="label"></param>
		/// <param name="mcNamePrefix"></param>
		/// <param name="mcNameSuffix"></param>
		/// <param name="dataType"></param>
		/// <param name="displayWidth"></param>
		/// <param name="decimals"></param>
		/// <param name="defaultSelection"></param>
		/// <param name="unsummarizedExists"></param>
		/// <param name="summarizedExists"></param>
		/// <returns></returns>

		MetaColumn AddResultDetailField(
			MetaTable mt,
			ResultTypeData rtd,
			string columnMap,
			string label,
			string mcNamePrefix,
			string mcNameSuffix,
			MetaColumnType dataType,
			float displayWidth,
			int decimals,
			ColumnSelectionEnum defaultSelection,
			bool unsummarizedExists,
			bool summarizedExists)
		{
			MetaColumn mc = new MetaColumn();
			string mcName = mcNamePrefix + columnMap.ToUpper() + mcNameSuffix;
			mc.Name = mt.GetValidMetaColumnName(mcName);
			if (rtd.AssayResultTypeId > 0)
			{
				mc.ResultCode = rtd.AssayResultTypeId.ToString(); // assay result type code
				mc.ResultCode2 = rtd.AssayMetadataResultTypeId.ToString(); // store AssayMetadata result type code in ResultCode2
			}

			mc.Label = label;
			mc.DataType = dataType;
			mc.InitialSelection = defaultSelection;
			mc.Width = displayWidth;
			mc.Decimals = decimals;
			mc.MetaTable = mt;
			mc.ColumnMap = columnMap;
			mc.TableMap = AssaySqlElements.RsltSubqueryName;

			if (Lex.Eq(rtd.SourceTable, "dtld"))
				mc.SummarizationRole = SummarizationRole.Dependent;

			else if (Lex.Eq(rtd.SourceTable, "actvty"))
				mc.SummarizationRole = SummarizationRole.Derived;

			mc.UnsummarizedExists = unsummarizedExists;

			if (summarizedExists)
				mc.SummarizedExists = true; // summarized data exists for column

			mt.AddMetaColumn(mc);
			return mc;
		}

		/// <summary>
		/// Add an expected stat column if not present in table already
		/// Note that stat types are not present in AssayMetadata expected result types
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="assayType"></param>
		/// <param name="assayMetadataType"></param>
		/// <param name="label"></param>
		/// <returns></returns>

		MetaColumn AddExpectedStatColumn(
			MetaTable mt,
			int assayType,
			int assayMetadataType,
			string label)
		{
			string mcName = GetPivotedColNamePrefix(assayMetadataType);
			MetaColumn mc = mt.GetMetaColumnByName(mcName);
			if (mc != null) return mc;

			mc = mt.AddMetaColumn(mcName, label, MetaColumnType.QualifiedNo, Selected, 12, AssaySqlElements.RsltSubqueryName, "value_nbr");
			mc.ResultCode = assayType.ToString();
			mc.ResultCode2 = assayMetadataType.ToString();
			mc.SummarizationRole = SummarizationRole.Dependent;
			if (QueryEngineLibrary.AssayResultType.IsDeselectedResultType(mc))
				mc.InitialSelection = ColumnSelectionEnum.Unselected;
			return mc;
		}

		/// <summary>
		/// Read in MetaTree data for ASSAY
		/// </summary>

		public void BuildMetaTree()
		{

			if (ServicesIniFile.IniFile == null || !ServicesIniFile.ReadBool("LoadAssayMetaData", false))
				return;


			//BuildResearchEffortAssaySubtree(); // build simple tree for dev work

			//BuildResearchEffortPlatformSubtree();
			//BuildTargetPlatformSubtree(); // skip targets subtree for now

			return;
		}

		/// <summary>
		/// Build simple tree for dev work
		/// </summary>

		void BuildResearchEffortAssaySubtree()
		{
			DbCommandMx drd;
			DbDataReader dr = null;
			MetaTreeNode assayRoot, mtn, reMtn;
			string sql, tok;
			int i1;

			assayRoot = MetaTreeFactory.GetNode("ASSAY_DATA");
			if (assayRoot == null) return;

			// Sql for research effort won't work for now because rsrch_efrt_id not currently set in assay_well_mtrl

			sql = @"
					select distinct  /* RSRCH_EFRT_ID NOT CURRENTLY SET IN ASSAY_WELL_MTRL */
						re.id_nbr rsrch_efrt_id, 
						re.nm_txt rsrch_efrt_nm,
						a.id_nbr assay_id,
						a.nm_txt assay_nm  
					from 
						assay_owner.assay_rsrch_efrt re,
						assay_owner.assay_assay a,
						assay_owner.assay_well_mtrl wm
					where
						re.rsrch_efrt_id = wm.rsrch_efrt_id
						and a.assay_id = wm.assay_id
					order by upper(re_nm), upper(assay_nm)";

			// Just do simple list of assays for now

			if (TextualMetadata == null)
				ReadPrecomputedAssayMetadata();

			List<AssayMetadata> aList = new List<AssayMetadata>();

			foreach (string s in TextualMetadata.Values) // get list of assays sorted by name
			{
				AssayMetadata assay = new AssayMetadata();

				string[] rows = s.Split('\n');
				string row = rows[0]; // get assay info
				string[] sa = row.Split('\t');

				assay.AssayId = int.Parse(sa[0]);
				assay.AssayAssayId = int.Parse(sa[1]);
				assay.AssayName = sa[2];

				for (i1 = aList.Count - 1; i1 >= 0; i1--)
				{
					if (Lex.Gt(assay.AssayName, aList[i1].AssayName)) break;
				}

				aList.Insert(i1 + 1, assay);
			}

			foreach (AssayMetadata assay0 in aList)
			{
				mtn = new MetaTreeNode(); // build node for assay
				mtn.Type = MetaTreeNodeType.MetaTable;
				mtn.Name = "ASSAY_" + assay0.AssayId;
				mtn.Target = mtn.Name;
				mtn.Label = assay0.AssayName;

				MetaTreeFactory.AddNode(mtn);
				assayRoot.Nodes.Add(mtn);
			}

			return;
		}

		/// <summary>
		/// Get Assay data from precomputed data
		/// </summary>
		/// <param name="assayId"></param>
		/// <returns></returns>

		AssayMetadata GetAssayMetadata(
			int assayId)
		{
			AssayMetadata mtd = new AssayMetadata();
			ResultTypeData rtd = null, srtd = null;
			string[] rows, sa;
			string txt;
			int ri;

			if (assayId == -1) return GetAssayUnpivotedMetaData(assayId); // special unpivoted table

			if (!TextualMetadata.ContainsKey(assayId)) // if not in cached data, try to get from ASSAY Oracle MetaTables
			{
				mtd = GetAssayMetadataFromExpectedResultTypes(assayId);
				return mtd;
			}

			txt = TextualMetadata[assayId];
			rows = txt.Split('\n');

			for (ri = 0; ri < rows.Length; ri++)
			{
				txt = rows[ri];
				sa = txt.Split('\t');
				if (ri == 0) // first row is assay header
				{
					mtd.AssayId = assayId;
					mtd.AssayAssayId = int.Parse(sa[1]);
					mtd.AssayName = sa[2].Trim();
					continue;
				}

				rtd = new ResultTypeData();
				mtd.ResultTypeData.Add(rtd);

				int sai = 1; // start indexing sa at RdwResultTypeId
				rtd.AssayId = assayId;
				rtd.AssayResultTypeId = int.Parse(sa[sai++]);
				rtd.AssayMetadataResultTypeId = int.Parse(sa[sai++]);
				rtd.SourceTable = sa[sai++].Trim();
				rtd.ResultCount = Int32.Parse(sa[sai++]);
				rtd.ValueNbrCardinality = Int32.Parse(sa[sai++]);
				rtd.ValueNbrNonnullCount = Int32.Parse(sa[sai++]);
				rtd.ValueTxtCardinality = Int32.Parse(sa[sai++]);
				rtd.ValueTxtNonnullCount = Int32.Parse(sa[sai++]);
				rtd.UomName = sa[sai++].Trim();
				rtd.ConcNbrCardinality = Int32.Parse(sa[sai++]);
				rtd.ConcNbrNonnullCount = Int32.Parse(sa[sai++]);
				rtd.ConcUomName = sa[sai++].Trim();
				rtd.ImageNonnullCount = Int32.Parse(sa[sai++]);
				rtd.CommentNonnullCount = Int32.Parse(sa[sai++]);

				rtd.ResultCountSummarized = Int32.Parse(sa[sai++]);
				rtd.SummarizationMethod = sa[sai++].Trim();
				bool.TryParse(sa[sai++], out rtd.SubgroupOnConc);
				rtd.ConcNbrNonnullCountSummarized = Int32.Parse(sa[sai++]);
				rtd.ConcNbrCardinalitySummarized = Int32.Parse(sa[sai++]);

				rtd.Name = sa[sai++].Trim();

				mtd.RtdDict[rtd.AssayResultTypeId] = rtd; // include in dict

				if (sa.Length > sai) // include extra attributes from cmn_assy_atrbts
				{
					if (sa[sai] != "") rtd.TargetSymbol = sa[sai++].Trim();
					if (sa[sai] != "") rtd.TargetId = Int32.Parse(sa[sai++]);
					if (sa[sai] != "") rtd.GeneFamily = sa[sai++].Trim();
					if (sa[sai] != "") rtd.AssayType = sa[sai++].Trim();
					if (sa[sai] != "") rtd.AssayMode = sa[sai++].Trim();
					if (sa[sai] != "") rtd.AssayLocation = sa[sai++].Trim();
				}

			}

			return mtd;
		}

		/// <summary>
		/// Get metadata for assay from ASSAY & AssayMetadata metadata tables
		/// </summary>
		/// <param name="assayId">AssayMetadata ID of ASSAY to lookup</param>
		/// <returns></returns>

		AssayMetadata GetAssayMetadataFromExpectedResultTypes(
				int assayId)
		{
			AssayMetadata assay = null;
			ResultTypeData rtd = null, srtd = null;
			Hashtable resTypes = new Hashtable();
			DbDataReader dr = null;

			int si, typeId, count;
			string tok;
			int t0, t1;

			t0 = TimeOfDay.Milliseconds();

			MetaTable mmt = MetaTableCollection.Get("ASSAYMERGEDEF_" + assayId);
			if (mmt != null) return GetAssayDataMergeProxy(mmt);

			DbCommandMx drd = new DbCommandMx();
			string sql = QueryEngineLibrary.AssayResultType.SelectExpectedResultTypesSqlTemplate;
			drd.PrepareMultipleParameter(sql, 1); // (takes ~700 ms first time)
			t1 = TimeOfDay.Milliseconds() - t0;
			dr = drd.ExecuteReader(assayId);
			t1 = TimeOfDay.Milliseconds() - t0;

			while (dr.Read())
			{
				if (assay == null)
				{
					assay = new AssayMetadata();
					assay.AssayId = drd.GetIntByName("assay_id");
					assay.AssayAssayId = drd.GetIntByName("assay_id");
					assay.AssayName = drd.GetStringByName("assay_nm");
					//assay.AssayDesc = drd.GetStringByName("mthd_vrsn_desc_txt");

					if (AssayMetaBroker.AssayId2ToAssayIdMap == null)
					{
						AssayMetaBroker.AssayId2ToAssayIdMap = new Dictionary<int, int>();
						AssayMetaBroker.AssayIdToAssayId2Map = new Dictionary<int, int>();
					}

					AssayMetaBroker.AssayId2ToAssayIdMap[assay.AssayAssayId] = assayId;
					AssayMetaBroker.AssayIdToAssayId2Map[assayId] = assay.AssayAssayId;
				}

				rtd = new ResultTypeData();
				rtd.AssayId = assay.AssayId;
				rtd.AssayResultTypeId = drd.GetIntByName("rslt_typ_id");

				rtd.AssayMetadataResultTypeId = drd.GetIntByName("SECONDARY_RSLT_TYP_ID");
				int dimTypeId = drd.GetIntByName("dmnsn_typ_id");
				if (dimTypeId != QueryEngineLibrary.AssayResultType.AssayRsltTypIdDim) 
					rtd.AssayMetadataResultTypeId = -rtd.AssayMetadataResultTypeId;

				rtd.SummarizationMethod = drd.GetStringByName("smrzd_mean_typ_cd");
				if (Lex.IsDefined(rtd.SummarizationMethod))
				{
					rtd.ResultCountSummarized = 2; // assume some summarized data so column appears even if no data yet
					if (Lex.Eq(rtd.SummarizationMethod, "Arithmetic")) rtd.ConcNbrNonnullCount = 2; // make conc column appear for SP type results with arithmetic summarization
				}
				rtd.Name = drd.GetStringByName("rslt_typ_nm");
				rtd.SourceTable = "dtld"; // assume from detailed results table
				bool.TryParse(drd.GetStringByName("subgrp_on_conc_cd"), out rtd.SubgroupOnConc); // true/false for summarize on concentration subroups
				if (rtd.SubgroupOnConc) rtd.ConcNbrNonnullCountSummarized = 2; // assume some summarized data so column appears even if no data yet

				assay.ResultTypeData.Add(rtd);
				if (!assay.RtdDict.ContainsKey(rtd.AssayResultTypeId))
					assay.RtdDict.Add(rtd.AssayResultTypeId, rtd);
				else continue;
			}

			if (assay == null)
			{
				assay = new AssayMetadata();
				assay.AssayId = assayId;
			}

			drd.CloseReader();
			drd.Dispose();

			ApplyPreferredColumnOrder(assay); // put in desired order

			t0 = TimeOfDay.Milliseconds() - t0;
			return assay;
		}

		/// <summary>
		/// Get dummy metadata for unsummarized view
		/// </summary>
		/// <param name="assayId"></param>
		/// <returns></returns>

		AssayMetadata GetAssayUnpivotedMetaData(
			int assayId)
		{
			AssayMetadata assay = new AssayMetadata();
			ResultTypeData rtd = null, srtd = null;
			string[] rows, sa;
			string txt;

			assay.AssayId = assayId;
			assay.AssayAssayId = -1;
			assay.AssayName = "Unpivoted ASSAY Data";

			rtd = new ResultTypeData(); // single result type
			assay.ResultTypeData.Add(rtd);

			int sai = 1; // start indexing sa at RdwResultTypeId
			rtd.AssayId = assayId;
			rtd.AssayMetadataResultTypeId = -1;
			rtd.AssayResultTypeId = -1;
			rtd.SourceTable = "dtld";
			rtd.ResultCount = 100;
			rtd.ValueNbrCardinality = 100;
			rtd.ValueNbrNonnullCount = 100;
			rtd.ValueTxtCardinality = 100;
			rtd.ValueTxtNonnullCount = 100;
			rtd.UomName = "";
			rtd.ConcNbrCardinality = 100;
			rtd.ConcNbrNonnullCount = 100;
			rtd.ConcUomName = "Mltiple";
			rtd.ImageNonnullCount = 0; // no images
			rtd.CommentNonnullCount = 100;

			rtd.SummarizationMethod = "Mltiple"; // multiple summarization methods
			rtd.ResultCountSummarized = 100;
			rtd.ConcNbrNonnullCountSummarized = 100;
			rtd.ConcNbrCardinalitySummarized = 100;

			rtd.Name = "Result Value";

			return assay;
		}

		/// <summary>
		/// Build a merge proxy table def
		/// </summary>
		/// <param name="mmt"></param>
		/// <returns></returns>

		AssayMetadata GetAssayDataMergeProxy(MetaTable mmt)
		{
			AssayMetadata mtd = null, mtd2;
			string keyTableName = null;
			int tableId;
			bool summary;

			string[] sa = mmt.TableMap.Split(','); // get list of assoc metatable names

			foreach (string tableName0 in sa) // get table with most result types as key table
			{
				MetaTable.ParseMetaTableName(tableName0, out tableId, out summary);
				mtd2 = GetAssayMetadata(tableId); // get info for 1st table as model
				if (mtd2 == null) continue;

				if (mtd == null || mtd2.ResultTypeData.Count > mtd.ResultTypeData.Count)
				{
					keyTableName = tableName0;
					mtd = mtd2;
				}
			}

			foreach (string tableName0 in sa) // see if other tables have any other result types that should be included
			{
				if (tableName0 == keyTableName) continue; // skip if key table

				MetaTable.ParseMetaTableName(tableName0, out tableId, out summary);
				mtd2 = GetAssayMetadata(tableId); // get info for next table
				foreach (ResultTypeData rtd2 in mtd2.ResultTypeData)
				{
					if (!mtd.RtdDict.ContainsKey(rtd2.AssayResultTypeId))
					{ // additional type?
						mtd.ResultTypeData.Add(rtd2);
						mtd.RtdDict.Add(rtd2.AssayResultTypeId, rtd2);
					}
				}
			}

			mtd.MergeDefTable = mmt;
			int.TryParse(mmt.Code, out mtd.AssayId); // = mmt.Id;
			mtd.AssayName = mmt.Label;
			if (mmt.Description != null)
				mtd.AssayDesc = mmt.Description;

			return mtd;
		}

		/// <summary>
		/// ReadPrecomputedAssayMetaData()
		/// </summary>

		void ReadPrecomputedAssayMetadata()
		{
			StreamReader sr;
			Dictionary<int, string> resTypes;
			ResultTypeData pcRtd;
			StringBuilder sb;
			string fileName, txt;
			string[] sa = null;
			int currentAssayId = -1, assayId, assayId2, t0, t1;

			t0 = TimeOfDay.Milliseconds();

			TextualMetadata = new Dictionary<int, string>();

			AssayMetaBroker.AssayId2ToAssayIdMap = new Dictionary<int, int>(); // internal database-specific assayId map to external assayId map
			AssayMetaBroker.AssayIdToAssayId2Map = new Dictionary<int, int>(); // external assayId map to internal database-specific assayId map

			fileName = ServicesDirs.MetaDataDir + @"\Metatables\AssayPrecomputedMetadata.txt";
			try
			{
				sr = new StreamReader(fileName);
			}
			catch (Exception ex)
			{
				DebugLog.Message("AssayMetaFactory.MergePrecomputedMetadata: Can't open file " + fileName);
				return;
			}

			sb = new StringBuilder();

			txt = sr.ReadLine(); // read & ignore header line
			while (true)
			{
				txt = sr.ReadLine();

				if (txt != null)
				{
					sa = txt.Split('\t');
					assayId = Int32.Parse(sa[0]);
				}
				else
				{
					if (currentAssayId == -1) break; // nothing in file
					assayId = -1;
				}

				if (assayId != currentAssayId) // first row for next assay or all done
				{
					if (currentAssayId > 0 && sb.Length > 0) // store text form for curremt assay id
						TextualMetadata[currentAssayId] = sb.ToString();

					if (assayId > 0) // next assay, store mapping between assay id and assay assay id
					{
						assayId = Int32.Parse(sa[1]);
						AssayMetaBroker.AssayId2ToAssayIdMap[assayId] = assayId;
						AssayMetaBroker.AssayIdToAssayId2Map[assayId] = assayId;
					}

					currentAssayId = assayId;
					sb.Length = 0; // reset buffer
				}

				else sb.Append("\n");

				if (txt == null) break;

				sb.Append(txt);
			}

			sr.Close();
			t1 = TimeOfDay.Milliseconds() - t0;

			return;
		}

		/// <summary>
		/// Scan data tables & store metadata that would take too long to retrieve at runtime
		/// </summary>

		public void UpdatePrecomputedAssayMetaData(string singleAssayId)
		{
			string txt;

			if (!Lex.IsNullOrEmpty(singleAssayId))
			{
				if (Lex.StartsWith(singleAssayId, "ASSAY_") || Lex.StartsWith(singleAssayId, "ASSAY_"))
					singleAssayId = singleAssayId.Substring(4);
				if (!Lex.IsInteger(singleAssayId))
					throw new Exception("Assay Id must be an integer");
			}

			bool useExpectedResultTypes = ServicesIniFile.ReadBool("AssayUseExpectedResultTypes", false);

			Dictionary<int, AssayMetadata> metadataDict =
				ReadAndAnalyzeAssayData(singleAssayId, useExpectedResultTypes);

			string fileName = MetaTableFactory.MetaTableXmlFolder + @"\AssayPrecomputedMetadata";
			string path = fileName + ".new";
			StreamWriter sw = new StreamWriter(path);

			if (Lex.IsUndefined(singleAssayId))
				sw.WriteLine(SerializeResultTypeData(null)); // write out header line

			else // if assay specified read & write all data but this assay
			{
				path = fileName + ".txt";
				if (File.Exists(path))
				{
					StreamReader sr = new StreamReader(path); // open existing file
					while (true)
					{
						string rec = sr.ReadLine();
						if (rec == null) break;
						if (rec.StartsWith(singleAssayId + "\t")) continue; // skip existing recs

						sw.WriteLine(rec); // write all other recs
					}
					sr.Close();
				}

				else // no file just write header
					sw.WriteLine(SerializeResultTypeData(null)); // write out header line
			}

			foreach (AssayMetadata md0 in metadataDict.Values)
			{
				txt = md0.AssayId.ToString() + "\t" + md0.AssayAssayId.ToString() + "\t" + md0.AssayName; // header for assay
				sw.WriteLine(txt);
				foreach (ResultTypeData rtd0 in md0.ResultTypeData)
				{
					sw.WriteLine(SerializeResultTypeData(rtd0));
				}
			}

			sw.Close();
			FileUtil.BackupAndReplaceFile(fileName + ".txt", fileName + ".bak", fileName + ".new");
			return;
		}

		/// <summary>
		/// SerializeDataForResultType
		/// </summary>
		/// <param name="rtd2"></param>
		/// <returns></returns>

		string SerializeResultTypeData(
			ResultTypeData rtd2)
		{
			string targetId, txt;

			if (rtd2 == null) // list of types
			{
				txt =
					"AssayId\t" +
					"AssayTyp\t" +
					"AssayId2\t" +
					"SrcTbl\t" +
					"RowCnt\t" +
					"NbrCard\t" +
					"NbrNn\t" +
					"TxtCard\t" +
					"TxtNn\t" +
					"Uom\t" +
					"CncCard\t" +
					"CncNn\t" +
					"CncUom\t" +
					"ImgNn\t" +
					"CmntNn\t" +

					"SRowCnt\t" +
					"SumMthd   \t" +
					"SGrpCnc\t" +
					"SCncNn\t" +
					"SCncCrd\t" +

					"RsltTypeName\t";
			}

			else
			{
				txt =
					rtd2.AssayId.ToString() + "\t" +
					rtd2.AssayResultTypeId + "\t" +
					rtd2.AssayMetadataResultTypeId + "\t" +
					rtd2.SourceTable + "\t" +
					rtd2.ResultCount + "\t" +
					rtd2.ValueNbrCardinality + "\t" +
					rtd2.ValueNbrNonnullCount + "\t" +
					rtd2.ValueTxtCardinality + "\t" +
					rtd2.ValueTxtNonnullCount + "\t" +
					rtd2.UomName + "\t" +
					rtd2.ConcNbrCardinality + "\t" +
					rtd2.ConcNbrNonnullCount + "\t" +
					rtd2.ConcUomName + "\t" +
					rtd2.ImageNonnullCount + "\t" +
					rtd2.CommentNonnullCount + "\t" +

					rtd2.ResultCountSummarized + "\t" +
					rtd2.SummarizationMethod.PadRight(10) + "\t" + // pad for file readability
					rtd2.SubgroupOnConc + "\t" +
					rtd2.ConcNbrNonnullCountSummarized + "\t" +
					rtd2.ConcNbrCardinalitySummarized + "\t" +

					rtd2.Name + "\t";

				targetId = (rtd2.TargetId > 0) ? rtd2.TargetId.ToString() : "";
				txt +=
					rtd2.TargetSymbol + "\t" +
					targetId + "\t" +
					rtd2.GeneFamily + "\t" +
					rtd2.AssayType + "\t" +
					rtd2.AssayMode + "\t" +
					rtd2.AssayLocation;
			}

			return txt;
		}

		/// <summary>
		/// AnalyzeAssayData
		/// </summary>
		/// <param name="singleAssayIdString"></param>
		/// <returns></returns>

		Dictionary<int, AssayMetadata> ReadAndAnalyzeAssayData(
			string singleAssayIdString,
			bool useExpectedResultTypes)
		{

			// Sql to get stats for unsummarized Data

			string[] unsummarizedStatsSql = 
			{
/* [0] - count rows by result type, also get assay and result type internal ids and names (labels) */ 
				@"<sqlGoesHere>",

/* [1] - Available */ 
				@"",
			 
/* [2] - count number of result groups by assay and result type */ 
				@"<sqlGoesHere>",

/* [3] - count of non-null numeric values */
				@"<sqlGoesHere>",

/* [4] - count of different numeric values */
				@"<sqlGoesHere>",

/* [5] - count of non-null txt values */
				@"<sqlGoesHere>",
																																																   
/* [6] - count of different txt values */
				@"<sqlGoesHere>",

/* [7] - uoms (may be mult per result type) */ 
				@"<sqlGoesHere>",
																																																  
/* [8] - count of non-null concentration values */ 
				@"<sqlGoesHere>",

/* [9] - count of different concentration values */ 
				@"<sqlGoesHere>",

/* [10] - concentration uoms (may be mult per result type) */ 
				@"<sqlGoesHere>",

				// Do next two for unsummarized data only

/* [11] - count of non-null image values (todo when image links available) */ 
				@"<sqlGoesHere>",
																																																  
/* [12] - count of non-null comments */ 
				@"<sqlGoesHere>",
			};

			// Sql to get stats for summarized Data

			string[] summarizedStatsSql =
			{

/* [0] - Count summary rows by result type */ 
				@"<sqlGoesHere>",

/* [1] - Count of non-null summarized concentration values */ 
				@"<sqlGoesHere>",
			
/* [2] - count of different summarized concentration values */ 
				@"<sqlGoesHere>",

			};

			Dictionary<int, AssayMetadata> metadataDict = new Dictionary<int, AssayMetadata>();
			Dictionary<int, int> assayIdToAssayId = new Dictionary<int, int>();
			AssayMetadata mtd;
			ResultTypeData rtd;
			DbCommandMx drd;
			DbDataReader dr;
			int assayId, rsltTypId, assayId2, step = -1, si = -1;
			string sql = "", tok, sourceTable = "";
			int singleAssayId = -1;

			try
			{

				if (Lex.IsDefined(singleAssayIdString))
					int.TryParse(singleAssayIdString, out singleAssayId);

				// Process unsummarized data 
				// Step 1. ASSAY_DTLD_RSLT - main SP or CRC results. 
				// Setp 2. ASSAY_ACTVTY_RSLT - single point data for CRC assays whose CRC values are in ASSAY_DTLD_RSLT
				//
				// The steps are actually done in reverse order (2 first, then 1) 
				// This is case the same result codes are in both tables
				// the result code for the ASSAY_DTLD_RSLT will overwrite the code for the lower level ASSAY_ACTVTY_RSLT table.

				for (step = 2; step >= 1; step--) // note reverse order
				{
					if (step == 1) // assay_dtld_rslt  							
						sourceTable = "dtld";

					else // assay_actvty_rslt
						sourceTable = "actvty";

					//DebugLog.Message("ASSAY_DTLD_RSLT, ASSAY_ACTVTY_RSLT, ASSAY_SMRZD_RSLT = " + step.ToString());
					//				if (System.Math.Sqrt(2) !=  0) break; // skip for now

					for (si = 0; si < unsummarizedStatsSql.Length; si++) // execute each sql statement
					{
						//DebugLog.Message("Sql statement = " + si.ToString());

						if (si == 1 || si == 2) continue; // unused
						if (si == 11) continue; // no images yet

						sql = AdjustSql(unsummarizedStatsSql[si], step, singleAssayIdString);

						drd = new DbCommandMx();
						drd.Prepare(sql);
						dr = drd.ExecuteReader();

						while (dr.Read())
						{
							if (dr.IsDBNull(0)) continue; // check assayId
							assayId = drd.GetInt(0);
							if (dr.IsDBNull(1)) continue; // type is null in some cases, ignore
							rsltTypId = drd.GetInt(1);
							//if (typeId == 0) typeId = typeId; // debug
							if (dr.IsDBNull(2)) continue;
							int count = drd.GetInt(2);

							bool newAssay = false;

							if (assayIdToAssayId.ContainsKey(assayId))
								assayId = assayIdToAssayId[assayId];

							else // going to new assay
							{
								assayId = drd.GetIntByName("assay_mthd_vrsn_id");

								if (si != 0) DebugLog.Message("First stat sql > 0 with data: si=" + si + ", assayId=" + assayId + ", assayId=" + assayId);

								if (useExpectedResultTypes) // start with list of expected types
									mtd = GetAssayMetadataFromExpectedResultTypes(assayId);

								else // init empty structure & just use the types seen in the results
								{
									mtd = new AssayMetadata();
									mtd.AssayId = assayId;
								}

								metadataDict[assayId] = mtd;
								mtd.AssayAssayId = assayId;
								assayIdToAssayId[assayId] = assayId;
								newAssay = true;
							}

							mtd = metadataDict[assayId];

							if (!mtd.RtdDict.ContainsKey(rsltTypId))
							{ // first data for type
								rtd = new ResultTypeData();
								rtd.AssayId = assayId;
								rtd.AssayResultTypeId = rsltTypId;
								rtd.SourceTable = sourceTable;
								mtd.ResultTypeData.Add(rtd);
								mtd.RtdDict.Add(rsltTypId, rtd);
							}

							else rtd = mtd.RtdDict[rsltTypId]; // get existing data for type

							rtd.SourceTable = sourceTable; // be sure we have the source table

							//if (typeId == 85) typeId = typeId; // debug
							//rtd.SourceTableNameAbbrv = sourceTableNameAbbrv; // debug

							switch (si)
							{
								case 0: // count rows by result type 
									rtd.ResultCount = count;

									if (Lex.IsUndefined(mtd.AssayName)) // get assay id and name
										mtd.AssayName = drd.GetStringByName("assay_name");

									rtd.AssayMetadataResultTypeId = drd.GetIntByName("rslt_typ_id");
									int dimTypeId = drd.GetIntByName("dmnsn_typ_id");
									if (dimTypeId != QueryEngineLibrary.AssayResultType.AssayRsltTypIdDim) // store as negative if not from assay_rslt_typ_id dimension
										rtd.AssayMetadataResultTypeId = -rtd.AssayMetadataResultTypeId;

									rtd.Name = drd.GetStringByName("result_type_name");
									rtd.SummarizationMethod = drd.GetStringByName("smrzd_mean_typ_cd");

									tok = drd.GetStringByName("subgrp_on_conc_cd");
									bool.TryParse(tok, out rtd.SubgroupOnConc);

									break;

								case 1: // available
									break;

								case 2: // count result groups by result type (should be same as ResultCount)
									rtd.ResultGroupCount = count;
									if (rtd.ResultGroupCount != rtd.ResultCount) rtd = rtd; // shouldn't happen
									break;

								case 3: // count of non-null numeric values
									rtd.ValueNbrNonnullCount = count;
									break;

								case 4: // count of different numeric values
									rtd.ValueNbrCardinality = count;
									break;

								case 5: // count of non-null txt values
									rtd.ValueTxtNonnullCount = count;
									break;

								case 6: // count of different txt values
									rtd.ValueTxtCardinality = count;
									break;

								case 7: // uoms (may be mult per result type)
									if (!dr.IsDBNull(3)) tok = dr.GetString(3);
									else tok = "";
									if (tok.ToUpper() == "NA") tok = ""; // treat not applicable as undefined
									if (tok == "") break;
									if (rtd.UomName == "") rtd.UomName = tok;
									else if (Lex.Ne(rtd.UomName, tok))
										rtd.UomName = "Mltiple";
									break;

								case 8: // count of non-null concentration values
									rtd.ConcNbrNonnullCount = count;
									break;

								case 9: // count of different concentration values
									rtd.ConcNbrCardinality = count;
									break;

								case 10:  // concentration uoms (may be mult per result type)
									if (!dr.IsDBNull(3)) tok = dr.GetString(3);
									else tok = "";
									if (tok.ToUpper() == "NA") tok = ""; // treat not applicable as undefined
									if (tok == "") break;
									if (rtd.ConcUomName == "")
										rtd.ConcUomName = tok;
									else if (Lex.Ne(rtd.ConcUomName, tok))
										rtd.ConcUomName = "Mltiple";
									break;

								case 11: // count of non-null image values
									rtd.ImageNonnullCount = count;
									break;

								case 12: // count of non-null comments
									rtd.CommentNonnullCount = count;
									break;
							}

							string mtName = "ASSAY_" + rtd.AssayId;
							AssayDict tad = MultiDbAssayMetaBroker.TargetAssayDict;
							if (newAssay && tad != null && tad.MetaTableNameMap.ContainsKey(mtName))
							{ // include target info with first result type only
								AssayAttributes tar = tad.MetaTableNameMap[mtName];
								rtd.TargetSymbol = tar.GeneSymbol;
								rtd.TargetId = tar.GeneId;
								rtd.GeneFamily = tar.GeneFamily;
								rtd.AssayType = tar.AssayType;
								rtd.AssayMode = tar.AssayMode;
								rtd.AssayLocation = tar.AssaySource;
							}
						}

						drd.CloseReader();
						drd.Dispose();
					}
				}

				// Process summarized data

				sourceTable = "summarized";

				for (si = 0; si < summarizedStatsSql.Length; si++) // execute each sql statement
				{

					sql = summarizedStatsSql[si];
					if (!Lex.IsNullOrEmpty(singleAssayIdString))  // single assay
						sql = sql.Replace("/*<assayFilter>*/", "and a.id_nbr in (" + singleAssayIdString + ")");

					drd = new DbCommandMx();
					drd.Prepare(sql);
					dr = drd.ExecuteReader();

					while (dr.Read())
					{
						if (dr.IsDBNull(0)) continue; // check assayId
						assayId = drd.GetInt(0);
						if (dr.IsDBNull(1)) continue; // type is null in some cases, ignore
						rsltTypId = drd.GetInt(1);
						//if (typeId == 0) typeId = typeId; // debug
						if (dr.IsDBNull(2)) continue;
						int count = drd.GetInt(2);

						if (!assayIdToAssayId.ContainsKey(assayId)) continue;  // shouldn't happen
						assayId = assayIdToAssayId[assayId];
						if (!metadataDict.ContainsKey(assayId)) continue; // shouldn't happen
						mtd = metadataDict[assayId];

						if (!mtd.RtdDict.ContainsKey(rsltTypId)) continue;
						rtd = mtd.RtdDict[rsltTypId]; // get existing data for type
						switch (si)
						{
							case 0: // count rows by result type 
								rtd.ResultCountSummarized = count;
								break;

							case 1: // count of non-null summarized concentration values
								rtd.ConcNbrNonnullCountSummarized = count;
								break;

							case 2: // count of different concentration values
								rtd.ConcNbrCardinalitySummarized = count;
								break;
						}
					}

					drd.Dispose();
				}

				if (Lex.IsDefined(singleAssayIdString) && metadataDict.Keys.Count == 0)
				{ // if singleAssay but no data then get from expected data
					mtd = GetAssayMetadataFromExpectedResultTypes(singleAssayId);
					if (mtd != null && mtd.ResultTypeData.Count > 0)
						metadataDict[singleAssayId] = mtd;
				}


				// Apply preferred ordering of result types

				foreach (AssayMetadata mtd0 in metadataDict.Values)
					ApplyPreferredColumnOrder(mtd0);

				return metadataDict;
			}

			catch (Exception ex)
			{
				string msg = "Exception for sourceTable: " + sourceTable + ", si: " + si + ", sql: " + sql;
				throw new Exception(msg, ex);
			}
		}


		/// <summary>
		/// Order columns in preferred order
		/// </summary>
		/// <param name="mtd"></param>

		void ApplyPreferredColumnOrder(AssayMetadata mtd)
		{
			ResultTypeData rtd;
			string dictKey;

			int npi = 200000; // not preferred index start
			string assayMetadataType;
			int i1, i2;

			for (i1 = 0; i1 < mtd.ResultTypeData.Count; i1++) // assign relative positions
			{
				rtd = mtd.ResultTypeData[i1];

				if (rtd.AssayId == 12032) rtd = rtd; // debug

				Dictionary<string, int> dict = QueryEngineLibrary.AssayResultType.ExtraResultTypeAttributes;
				string assay = "ASSAY_" + mtd.AssayId + "."; // Prefix if assay specific
				assayMetadataType = rtd.AssayMetadataResultTypeId.ToString();

				if (dict.ContainsKey(assay + assayMetadataType)) // check assayid specific ordering first
				{
					dictKey = assay + assayMetadataType;
					rtd.Position = dict[dictKey];
				}

				else if (dict.ContainsKey(assayMetadataType)) // generic order for type?
				{
					dictKey = assayMetadataType;
					rtd.Position = dict[dictKey] + 100000; // use higher number range to sort later if not metatable specific
				}

				else // no preferred position, put at end
				{
					rtd.Position = npi;
					npi++;
				}
			}

			for (i1 = 1; i1 < mtd.ResultTypeData.Count; i1++) // sort by preferred position
			{
				rtd = mtd.ResultTypeData[i1];
				for (i2 = i1 - 1; i2 >= 0; i2--)
				{
					if (mtd.ResultTypeData[i2].Position <= rtd.Position) break;
					mtd.ResultTypeData[i2 + 1] = mtd.ResultTypeData[i2];
				}

				mtd.ResultTypeData[i2 + 1] = rtd;
			}

			return;
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			return UpdateMetaTableStatistics(null);
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics(string singleAssayId)
		{
			Dictionary<string, MetaTableStats> stats = new Dictionary<string, MetaTableStats>();
			int assayId;
			DateTime dt;
			string mtName, sql, txt;
			long cnt;
			int step;

			try
			{
				if (!Lex.IsNullOrEmpty(singleAssayId))
				{
					if (Lex.StartsWith(singleAssayId, "ASSAY_") || Lex.StartsWith(singleAssayId, "ASSAY_"))
						singleAssayId = singleAssayId.Substring(4);
					if (!Lex.IsInteger(singleAssayId))
						throw new Exception("Assay Id must be an integer");
				}

				DbCommandMx dao = new DbCommandMx();

				// Get row counts

				string rowCntSql = @"<sqlGoesHere>";

				string lastUpdateSql = @"<sqlGoesHere>";

				for (step = 1; step <= 2; step++) // do assay_dtld_rslt counts/dates first then assay_actvty_rslt
				{
					sql = AdjustSql(rowCntSql, step, singleAssayId);

					dao.Prepare(sql);
					dao.ExecuteReader();
					while (dao.Read())
					{
						assayId = dao.GetInt(0);
						mtName = "ASSAY_" + assayId;
						cnt = dao.GetLong(1);
						if (!stats.ContainsKey(mtName)) stats[mtName] = new MetaTableStats();
						if (cnt > stats[mtName].RowCount) stats[mtName].RowCount = cnt;
					}

					dao.CloseReader();

					sql = AdjustSql(lastUpdateSql, step, singleAssayId);

					dao.Prepare(sql);
					dao.ExecuteReader();
					while (dao.Read())
					{
						assayId = dao.GetInt(0);
						mtName = "ASSAY_" + assayId;
						dt = dao.GetDateTime(1);
						if (!stats.ContainsKey(mtName)) stats[mtName] = new MetaTableStats();
						if (DateTime.Compare(stats[mtName].UpdateDateTime, dt) < 0) stats[mtName].UpdateDateTime = dt;
					}
					dao.CloseReader();
				}

				dao.Dispose();

				string fileName = MetaTableFactory.MetaTableXmlFolder + @"\AssayMetaTableStats";

				if (!Lex.IsNullOrEmpty(singleAssayId)) singleAssayId = "ASSAY_" + singleAssayId; // convert to full table name
				MetaTableFactory.WriteMetaTableStats(stats, singleAssayId, fileName);
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return stats.Count;
		}

		/// <summary>
		/// AdjustSql
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="step"></param>
		/// <param name="singleAssayId"></param>
		/// <returns></returns>

		static string AdjustSql(string sql, int step, string singleAssayId)
		{
			if (step == 2) sql =
				sql = sql.Replace("dtld_rslt", "actvty_rslt"); // adjust table & col names
																											 //sql = sql.Replace("assay_dtld_rslt", "assay_actvty_rslt");
																											 // Lex.Replace(sql, "dtld", "actvty"); // get proper table

			if (step == 1)
				sql = Lex.Replace(sql, "/*<validFilter>*/", " and dr.aprvl_sts_cd = 'Y' "); // add valid results only filter for assay_dtld_rslt

			if (!Lex.IsNullOrEmpty(singleAssayId))  // single assay
				sql = sql.Replace("/*<assayFilter>*/", "and a.id_nbr in (" + singleAssayId + ")");

			return sql;
		}

		/// <summary>
		/// Get the metatable stats for the database
		/// </summary>
		/// <returns></returns>

		public static Dictionary<string, MetaTableStats> GetMetaTableStats()
		{
			if (Instance == null) Instance = new AssayMetaFactory();
			if (TableStats == null) Instance.LoadMetaTableStats();
			return TableStats;
		}

		/// <summary>
		/// Load stats for metatables & add to existing collection
		/// </summary>
		/// <param name="metaTableStats"></param>

		public int LoadMetaTableStats(
			Dictionary<string, MetaTableStats> metaTableStats)
		{
			if (TableStats == null)
				LoadMetaTableStats();

			foreach (string mtName in TableStats.Keys)
			{
				metaTableStats[mtName] = TableStats[mtName];
			}

			return TableStats.Count;
		}

		/// <summary>
		/// Load stats for metatables
		/// </summary>
		/// <returns></returns>

		public int LoadMetaTableStats()
		{
			TableStats = new Dictionary<string, MetaTableStats>();
			AssayMetaBroker.TableStats = TableStats; // pass to metabroker also

			string fileName = MetaTableFactory.MetaTableXmlFolder + @"\AssayMetatableStats.txt";
			int cnt = MetaTableFactory.LoadMetaTableStats(fileName, TableStats);

			return cnt;
		}

		/// <summary>
		/// Return true if this metatable should be handled by ASSAY
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public bool IsMemberMetaTable(string mtName)
		{
			return IsAssayMetaTable(mtName);
		}

		/// <summary>
		/// Return true if this metatable should be retrieved from the ASSAY
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsAssayMetaTable(string mtName)
		{
			MetaTreeNode parent;
			string assayLabel;
			int assayId, screenId;
			bool isSummary;

			mtName = mtName.ToUpper();
			MetaTable.ParseMetaTableName(mtName, out assayId, out isSummary);
			string assayName = "ASSAY_" + assayId; // basic name (not ASSAY_123_SUMMARY)

			if (Lex.Contains(mtName, "_6777")) mtName = mtName; // debug

			// If starts with ASSAY then definitely ASSAY table

			if (Lex.StartsWith(mtName, "ASSAY_")) return true; // if starts with ASSAY_ then true

			if (!Lex.StartsWith(mtName, "ASSAY_")) return false; // if not starts with ASSAY_ then false

			// See if assay is in the list of migrating or migrated tables

			if (MigratedAssayTables == null)
				ReadMigrationStatus();

#if false // old method from Root.xml
			if (nodes == null) return false;

			string migratingNodeName = "ASSAY_ASSAYS_MIGRATING";
			string migratedNodeName = "ASSAY_ASSAYS_MIGRATED";

			if (MigratedAssayTables == null)
			{
				MigratingAssayTables = SetupAssayTableHashSet(migratingNodeName, nodes);
				MigratedAssayTables = SetupAssayTableHashSet(migratedNodeName, nodes);
			}
#endif

			//if (assayId == 2305) assayId = assayId; // debug

			if (MigratedAssayTables.Contains(assayId)) return true; // if migrated then ASSAY

			if (MigratingAssayTables.Contains(assayId)) return false; // if migrating then not ASSAY

			foreach (KeyValuePair<int, int> range in MigratedAssayTablesRange) // check ranges of ASSAY assays
			{
				if (assayId >= range.Key && assayId <= range.Value) return true;
			}

			//  If data in the ASSAY database then true

			int assayCount = AssayMetaFactory.TableRowCount(assayName);
			if (assayCount > 0) // have ASSAY data
			{
				return true;
			}


			if (!IniFileSettingsRead) ReadIniFileSettings();

			return DefaultAssayMetatablesToAssay; // Return DefaultAssayMetatableNamesToAssay setting value
		}

		/// <summary>
		/// ReadMigrationStatus
		/// </summary>

		static void ReadMigrationStatus()
		{
			StreamReader sr = null;
			string tableType, tableType2, txt, tok1, tok2;
			int tableId, tableId2;
			bool summary, summary2;

			MigratingAssayTables = new HashSet<int>();
			MigratedAssayTables = new HashSet<int>();
			MigratedAssayTablesRange = new List<KeyValuePair<int, int>>();

			try
			{
				string fileName = ServicesDirs.MetaDataDir + @"\Metatables\AssayAssaysMigrated.txt";
				sr = new StreamReader(fileName);

				while (true)
				{
					txt = sr.ReadLine();
					if (txt == null) break;
					if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

					Lex.Split(txt, "-", out tok1, out tok2); // single or range

					MetaTable.ParseMetaTableName(tok1.Trim(), out tableType, out tableId, out summary);

					if (Lex.IsUndefined(tok2)) // single ASSAY
					{
						if (Lex.Eq(tableType, "ASSAY")) MigratedAssayTables.Add(tableId);
						else MigratingAssayTables.Add(tableId);
					}

					else // ASSAY range
					{
						MetaTable.ParseMetaTableName(tok2.Trim(), out tableType2, out tableId2, out summary2);
						MigratedAssayTablesRange.Add(new KeyValuePair<int, int>(tableId, tableId2));
					}
				}

				sr.Close();
				return;
			}

			catch (Exception ex)
			{
				FileUtil.CloseStreamReader(sr);
				return;
			}

		}

		/// <summary>
		/// Get assays that should definitely come from the ASSAY (not used)
		/// </summary>
		/// <returns></returns>

		static public HashSet<string> GetAssayProductionAssayList()
		{
			int assayId;

			if (ProductionAssays != null) return ProductionAssays;

			ProductionAssays = new HashSet<string>();
			string fileName = MetaTableFactory.MetaTableXmlFolder + @"\AssayProductionAssays.txt";

			if (!File.Exists(fileName)) return ProductionAssays;
			StreamReader sr = new StreamReader(fileName);
			int cnt = 0;
			while (true)
			{
				string rec = sr.ReadLine();
				if (rec == null) break;
				if (Lex.IsUndefined(rec) || rec.Trim().StartsWith(";")) continue;

				string tok = rec.Trim().ToUpper();
				int i1 = tok.IndexOf("_"); // remove any prefix
				if (i1 >= 0) tok = tok.Substring(i1 + 1);

				if (!int.TryParse(tok, out assayId)) continue;

				ProductionAssays.Add(assayId.ToString());
			}

			sr.Close();
			return ProductionAssays;
		}
	}
}
