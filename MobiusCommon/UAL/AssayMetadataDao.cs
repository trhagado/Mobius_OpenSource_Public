using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Text;

namespace Mobius.UAL
{

/// <summary>
/// UAL for assay metadata
/// </summary>

	public class AssayMetadataDao
	{
		static Dictionary<int, AssayDbMetadata> AssayTargetGeneDataDict = null;
		static Dictionary<int, AssayDbResultType> ResultTypeDict = null;

		public static bool ForceAssayId = false; // debug with single assay
		public static int ForcedAssayId = 0;

		/// <summary>
		/// Get biological target & gene(s) for an assay
		/// </summary>
		/// <param name="assayId"></param>
		/// <returns></returns>

		public static AssayDbMetadata GetAssayTargetGeneData(
			int assayId)
		{
			Dictionary<int, AssayDbMetadata> dict = GetAssayDbDict(assayId);
			if (dict.ContainsKey(assayId)) return dict[assayId];
			else throw new Exception("Assay not found: " + assayId);
		}

		/// <summary>
		/// Get biological target(s) & gene(s) for all assays
		/// </summary>
		/// <returns></returns>

		public static Dictionary<int, AssayDbMetadata> GetAssayTargetGeneData()
		{
			Dictionary<int, AssayDbMetadata> dict = GetAssayDbDict(-1);
			return dict;
		}

		/// <summary>
		/// Get Assay data including biological target(s) & gene(s) for a single Assay or all Assays
		/// </summary>
		/// <param name="assayId">Assay to get data for or -1 to get for all assays</param>
		/// <returns></returns>

		public static Dictionary<int, AssayDbMetadata> GetAssayDbDict
			(int assayId)
		{
			if (ForceAssayId) assayId = ForcedAssayId;
			else if (AssayTargetGeneDataDict != null) return AssayTargetGeneDataDict;

			string sql = @"
				SELECT <columns>
        FROM <tables>
        WHERE 1 = 1
				 and <conditions>";

			DbCommandMx drd = new DbCommandMx();

			if (assayId > 0) // get for single assay
			{
				sql = sql.Replace("1=1", "<keyColExpr> = :0");
				drd.PrepareParameterized(sql, DbType.Int32);
				drd.ExecuteReader(assayId);
			}

			else // get for all assays
			{
				drd.Prepare(sql);
				drd.ExecuteReader();
			}

			Dictionary<int, AssayDbMetadata> dict = new Dictionary<int, AssayDbMetadata>();
			if (ForceAssayId) AssayTargetGeneDataDict = dict;

			AssayDbMetadata assay = null;
			AssayDbTarget target = null;
			int currentAssayId = -1;
			int currentTrgtId = -1;

			while (true)
			{
				if (!drd.Read()) break;
				assayId = drd.GetInt(0);
				string assayName = drd.GetString(1);
				string MthdVrsnMdTxt = drd.GetString(2);
				if (currentAssayId < 0 || currentAssayId != assayId)
				{
					assay = new AssayDbMetadata();
					assay.AssayId = assayId;
					assay.Name = assayName;
					if (Lex.Eq(MthdVrsnMdTxt, "Single Point")) // translate values
						assay.SP = true;
					else if (Lex.Eq(MthdVrsnMdTxt, "Conc./Dose Response Curve"))
						assay.CRC = true;

					dict[assayId] = assay;
					currentAssayId = assayId;
					currentTrgtId = -1;
				}

				int trgtId = drd.GetInt(3); // target id
				if (currentTrgtId < 0 || currentTrgtId != trgtId)
				{ // new target
					target = new AssayDbTarget();
					target.TargetId = trgtId;
					target.TargetName = drd.GetString(4); // target name
					target.TargetDesc = drd.GetString(5); // target desc
					target.TargetTypeName = drd.GetString(6); // e.g. G protein coupled receptor
					target.TargetTypeShortName = drd.GetString(7); // e.g. GPCR
					assay.Targets.Add(target);
					currentTrgtId = trgtId;
				}

				string geneId = drd.GetString(8);
				if (String.IsNullOrEmpty(geneId)) continue;

				AssayDbGene gene = new AssayDbGene();
				gene.GeneId = geneId;
				gene.GeneSymbol = drd.GetString(9);
				target.Genes.Add(gene);
			}

			drd.Dispose();
			return dict;
		}

		/// <summary>
		/// Get approximation of assay type (binding/functional) and mode (agonist, antagonist, potentiator)
		/// </summary>
		/// <param name="assayTypeDict"></param>
		/// <param name="assayModeDict"></param>

		public static void GetAssayTypesAndModes(
			out Dictionary<int, string> assayTypeDict,
			out Dictionary<int, string> assayModeDict)
		{
			assayTypeDict = new Dictionary<int, string>();
			assayModeDict = new Dictionary<int, string>();

			string sql = @"
			 select <columns>
      from <tables> 
      where 1=1
        and <conditions>";

			if (ForceAssayId)
				sql = sql.Replace("1=1", "<keyCriteria> = " + ForcedAssayId);

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();

			bool readOk = true;
			int assayId = -1, currentAssayId = -1;
			string assayType = "", assayMode = "";

			while (true)
			{
				if (drd.Read())
					assayId = drd.GetInt(0);
				else readOk = false;

				if (assayId != currentAssayId || !readOk)
				{
					if (currentAssayId > 0)
					{
						assayTypeDict[currentAssayId] = assayType;
						assayModeDict[currentAssayId] = assayMode;
						assayType = assayMode = "";
					}

					if (!readOk) break;

					currentAssayId = assayId;
				}

			}

			drd.CloseReader();
			drd.Dispose();

			return;
		}

/// <summary>
/// Get image coordinates
/// </summary>
/// <param name="targetMapXDict"></param>
/// <param name="targetMapYDict"></param>

		public static void GetImageCoords(
			out Dictionary<int, double> targetMapXDict,
			out Dictionary<int, double> targetMapYDict)
		{
			targetMapXDict = new Dictionary<int, double>();
			targetMapYDict = new Dictionary<int, double>();

			string sql = @"
				select entrezgene_id, x, y
				from mdbassay_owner.image_coord";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();

			while (drd.Read())
			{
				int geneId = drd.GetInt(0);
				double x = drd.GetDouble(1);
				targetMapXDict[geneId] = x;

				double y = drd.GetDouble(2);
				targetMapYDict[geneId] = y;
			}

			drd.CloseReader();
			drd.Dispose();

			return;
		}

		/// <summary>
		/// Get the source of the assay results
		/// </summary>
		/// <param name="assayId"></param>
		/// <returns></returns>

		public static string GetAssaySource(int assayId)
		{
			return ""; // todo
		}

/// <summary>
/// Get dictionary of result types
/// </summary>
/// <returns></returns>

		public static Dictionary<int, AssayDbResultType> GetResultTypeDict()
		{
			if (ResultTypeDict != null) return ResultTypeDict;

			string sql = @"
				select
				 rt.assay_rslt_typ_id, 
				 rt.blgcl_rslt_typ_shrt_nm,
				 rt.sum_md_txt,
				 l.rslt_lvl_nm  
				from 
					metadata_owner.rslt_typ rt,
					metadata_owner.rslt_lvl l
				where 
					l.rslt_lvl_id = rt.rslt_lvl_id
          and rt.sts_id = 1
          and l.sts_id = 1";

			Dictionary<int, AssayDbResultType> dict = new Dictionary<int, AssayDbResultType>();
			ResultTypeDict = dict;

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();

			while (true)
			{
				if (!drd.Read()) break;
				AssayDbResultType rt = new AssayDbResultType();

				rt.RsltTypeId = drd.GetInt(0);
				rt.Name = drd.GetString(1);
				rt.SumMdTxt = drd.GetString(2);
				rt.RsltLvl = drd.GetString(3);

				dict[rt.RsltTypeId] = rt;
			}

			drd.CloseReader();

			return dict;
		}

		/// <summary>
		/// Return description for table.
		/// May be html or simple text
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static TableDescription GetTableDescription(
			MetaTable mt)
		{
			DateTime dt;
			bool firstRow;
			double d1;
			int assayId, i1;
			string geneSymbol, geneId, geneLinkUrl = "", html = "", url, txt;

			TableDescription td = new TableDescription();

			try
			{
				string templateFile = CommonConfigInfo.MiscConfigDir + @"<templateName>"; 
				StreamReader sr = new StreamReader(templateFile);
				html = sr.ReadToEnd();
				sr.Close();
			}
			catch (Exception ex)
			{
				td.TextDescription = "Error: " + ex.Message;
				return td;
			}

			try
			{ geneLinkUrl = ServicesIniFile.Read("LsgEntrezGeneUrlTemplate"); }
			catch (Exception ex) { }

			try { assayId = Convert.ToInt32(mt.Code); }
			catch (Exception ex) { return null; }

			AssayDbMetadata assay = AssayMetadataDao.GetAssayTargetGeneData(assayId); // get biological target(s) & gene(s) for assay

			string btStr = "";
			string gsStr = "";

			List<AssayDbTarget> targets = assay.Targets;
			foreach (AssayDbTarget target in assay.Targets)
			{
				string bt = target.TargetName;
				if (bt == "") continue;

				bt += "<Target_Link>";
				if (target.TargetDesc != "") bt += " - " + target.TargetDesc;

				if (btStr.IndexOf(bt) < 0) // new bio target
				{
					if (gsStr != "") gsStr = " (" + gsStr + ")";
					btStr = btStr.Replace("<Target_Link>", gsStr);
					gsStr = "";
					btStr += "<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
						bt;
				}

				if (target.Genes != null && target.Genes.Count > 0)
				{ // add link to gene if appropriate
					AssayDbGene gene = target.Genes[0]; // just first gene for now
					geneSymbol = gene.GeneSymbol;
					geneId = gene.GeneId;

					geneLinkUrl =
						"http:////Mobius/command?ShowContextMenu:TargetContextMenu(<Gene_Link>)";

					if (!String.IsNullOrEmpty(geneLinkUrl))
					{ // include link to gene description
						if (Lex.IsDefined(geneId)) 
							url = geneLinkUrl.Replace("<Gene_Link>", geneId); // link on gene id 
						else url = geneLinkUrl.Replace("<Gene_Link>", gene.GeneSymbol); // link on symbol
						geneSymbol = // define tag to open in new window
							"<a href=\"" + url + "\">" + geneSymbol + "</a>";
					}

					if (gsStr != "") gsStr += ", "; // same bt additional gene
					gsStr += geneSymbol;
				}
			}

			if (gsStr != "") gsStr = " (" + gsStr + ")";
			btStr = btStr.Replace("<Target_Link>", gsStr); // plug in last gene symbol

			html = html.Replace("blgcl_trgt_text", btStr);

			// Method type & descriptors

			string methodDescrSql = @"
				SELECT <columns>
        FROM <tables>  
        WHERE <criteria>
        ORDER BY LOWER(<orderCols>)";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareParameterized(methodDescrSql, DbType.Int32);

			assayId = Convert.ToInt32(mt.Code);
			drd.ExecuteReader(assayId);

			string mdStr = "";
			firstRow = true;
			while (true)
			{
				if (!drd.Read()) break;
				if (firstRow) // get single assay parameters
				{
					firstRow = false;
				}

				string mdtn = "<methodTypeName>";
				if (mdtn == "") continue;
				string bmdn = drd.GetStringByName("<methodDescriptionColumn>");
				string bmddt = drd.GetStringByName("<methodDescriptionColumn2>");
				mdtn = "<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
					mdtn + ": " + bmdn;
				if (bmddt != "" && bmdn.ToLower().IndexOf(bmddt.ToLower()) < 0) // include any description if don't already have
					mdtn += " - " + bmddt;
				mdStr += mdtn;
			}

			html = html.Replace("<methodDescriptionPlaceHolder>", mdStr);

			// Get Minimum Significant Results information

			string msrSql = @"
			SELECT <columns>
				FROM <tables>
				WHERE <conditions>";

			// RDM MSR Sql (obsolete)

			string msrSqlRdm = @"<todo>";
	 
			drd.PrepareParameterized(msrSql, DbType.Int32);

			assayId = Convert.ToInt32(mt.Code);
			drd.ExecuteReader(assayId);

			while (true)
			{
				if (!drd.Read()) break; // should always get at least one row even if no MSR data

			}

			// Pharmacological action

			// <todo>

			// Research effort & therapeutic target

			// <todo>

			// Biological Conditions

			// <todo>
			// All done

			drd.Dispose();

			td.TextDescription = html;
			return td;
		}

	} // AssayMetadataDao

	/// <summary>
	/// Class containing assay metadata read from a database
	/// </summary>

	public class AssayDbMetadata
	{
		public int AssayId;
		public string Name = ""; // assay name
		public string MdTxt = ""; // Conc./Dose Response Curve (111) or Single Point (222) - Not 100% reliable
		public bool SP = false; // Single Point assay
		public bool CRC = false; // Conc./Dose Response Curve assay

		public List<AssayDbTarget> Targets = new List<AssayDbTarget>();
	}

/// <summary>
/// Target information
/// </summary>

	public class AssayDbTarget
	{
		public int TargetId; 
		public string TargetName = "";
		public string TargetDesc = "";
		public string TargetTypeName = ""; // e.g. G protein coupled receptor
		public string TargetTypeShortName = ""; // e.g. GPCR
		public List<AssayDbGene> Genes = new List<AssayDbGene>(); // list of genes
	}

	/// <summary>
	/// Gene associated with biological target
	/// </summary>

	public class AssayDbGene
	{
		public string GeneId = "";
		public string GeneSymbol = "";
	}

/// <summary>
/// Assay metadata result type key fields
/// </summary>

	public class AssayDbResultType
	{
		public int RsltTypeId = -1; // assay metadata result type id
		public string Name = ""; // full name
		public string SumMdTxt = ""; // Linear (111), Log Normal (222), None (333), None (textual result) (444)
		public string RsltLvl = ""; // Raw (), Normalized (), Derived (), Summarized ()
	}

	/// <summary>
	/// Assay metadata result type codes
	/// </summary>

	public enum AssayMetadataResultTypeEnum
	{
		Inhibition = 1,
		IC50 = 2,
		Potency = 4,
		Ki = 5,
		Kb = 6,
		Stimulation = 8,
		EC50 = 9,
		Ka = 14,
		Kd = 11157
	}


}
