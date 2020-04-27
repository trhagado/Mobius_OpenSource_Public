using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Mobius.Data
{

/// <summary>
/// Parameters for Target Results View of data
/// </summary>

	public class TargetSummaryOptions
	{ // The members with names in parens also appear separately in the database and MetaTables
		public string DbName = "Corp"; // Corp or CorpChembl
		public string CidCriteria = ""; // criteria on compound id (e.g. CORP_SRL_NBR)
		public string TargetList = ""; // list of targets to include data for, all targets if blank or "All" - (GENE_SYMBL)

		public string GeneFamilies = ""; // list of gene families to include data for, all targets if blank- (GENE_FMLY)
		public string AssayTypesToInclude = "BINDING, FUNCTIONAL"; // type of assay, e.g. binding, functional" - (ASSY_TYP)
		public string AssayModesToInclude = ""; // mode of assay, e.g. agonist, antagonist, potentiator" - (ASSY_MODE)

		public string ResultTypesToInclude = ""; // type of result, e.g. single point(SP), concentration response curve (CRC) - (RSLT_TYP)
		public bool UseTopLevelResultTypeOnly = true; // if true ignore SP if CRC value for any assay for target - (TOP_LVL_RSLT)

		public bool   TargetsWithActivesOnly = false; // only include targets with "active" values
		public string CrcUpperBound = "1"; // upper limit of "active" CRC values in uM units
		public string SpLowerBound = "90"; // lower limit of "active" SP values in % units 

		public bool   UseMeans = true; // summarize by means 
		public bool   UseMostPotent { get { return !UseMeans; } } // summarize by min/max value

		public bool   IncludeStructures = false;
		public string FilterableTargets = ""; // targets to include pivoted results for to allow filtering of results on

		public string TargetMapName = ""; // map to use for this data set
		public string UserMapNames = ""; // maps specific to this user
		public string PreferredView = ""; // Preferred view format 
		public bool   FormatForMobius = true;
		public bool   FormatForSpotfire { get { return !FormatForMobius; } set { FormatForMobius = !value; } }
		public bool   ShowAdvancedDialog = false;


		/// <summary>
		/// Get any MultiDb view options from query
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static QueryTable GetTsoTableFromQuery(
			Query q)
		{
			foreach (QueryTable qt in q.Tables)
			{
				if (qt.GetQueryColumnByName(MultiDbAssayDataNames.MultiDbViewOptions) != null)
					return qt;
			}

			return null;
		}

/// <summary>
/// Get TargetSummaryOptions from QueryTable criteria values
/// </summary>
/// <param name="targetSumTable"></param>
/// <returns></returns>

		public static TargetSummaryOptions GetFromQueryTable (
			QueryTable targetSumTable)
		{
			QueryTable qt = targetSumTable;
			TargetSummaryOptions tso = GetFromMdbAssayOptionsColumn(qt);
			if (tso == null) tso = new TargetSummaryOptions(); // use default values
			if (qt == null) return tso;

			if (qt.Query != null) tso.CidCriteria = qt.Query.KeyCriteria;
			else tso.CidCriteria = qt.KeyQueryColumn.Criteria; // keep in variable criteria form
			tso.TargetList = GetListFromColCriteria(qt, MultiDbAssayDataNames.GeneSymbol);
			tso.GeneFamilies = GetListFromColCriteria(qt, MultiDbAssayDataNames.GeneFamily);
			tso.AssayTypesToInclude = GetListFromColCriteria(qt, MultiDbAssayDataNames.AssayType);
			tso.AssayModesToInclude = GetListFromColCriteria(qt, MultiDbAssayDataNames.AssayMode);
			tso.ResultTypesToInclude = GetListFromColCriteria(qt, MultiDbAssayDataNames.ResultType);

			//string criteria = qt.GetQueryColumnByName("top_lvl_rslt").Criteria;
			//tso.UseTopLevelResultTypeOnly = Lex.IsNullOrEmpty(criteria) || Lex.Contains(criteria, "'Y'");

			return tso;
		}


/// <summary>
/// Get any existing target summary options from option column
/// </summary>
/// <param name="qt"></param>
/// <returns></returns>

		public static TargetSummaryOptions GetFromMdbAssayOptionsColumn(
			QueryTable qt)
		{
			TargetSummaryOptions tso = null;

			if (qt == null) return null;
			QueryColumn qc = qt.GetQueryColumnByName(MultiDbAssayDataNames.MultiDbViewOptions);
			if (qc == null) return null;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			if (psc == null) return null;
			string tsoString = Lex.RemoveSingleQuotes(psc.Value);
			if (Lex.IsNullOrEmpty(tsoString)) return null;

			tso = TargetSummaryOptions.Deserialize(tsoString);
			return tso;
		}

/// <summary>
/// Set QueryTable criteria values from TargetSummaryOptions
/// </summary>
/// <param name="TargetSumTable"></param>

		public bool SetInQueryTable(
			QueryTable targetSumTable)
		{
			QueryTable qt = targetSumTable;
			if (qt == null) return false;
			Query q = qt.Query;
			QueryColumn qc = qt.GetQueryColumnByNameWithException(MultiDbAssayDataNames.MultiDbViewOptions);
			if (qc == null) return false;

			string tsoString = Serialize();
			qc.Criteria = qc.MetaColumn.Name + " = " + Lex.AddSingleQuotes(tsoString);
			qc.CriteriaDisplay = "Edit...";

			qt.KeyQueryColumn.Criteria = CidCriteria; // keep in variable criteria form
			SetListCriteriaForCol(qt, MultiDbAssayDataNames.GeneSymbol, TargetList);
			SetListCriteriaForCol(qt, MultiDbAssayDataNames.GeneFamily, GeneFamilies);

			return true;
		}

		/// <summary>
		/// Get a simple comma separated list of allowed values for the column
		/// </summary>
		/// <param name="colname"></param>
		/// <returns></returns>

		static string GetListFromColCriteria(
			QueryTable qt,
			string colName)
		{
			string list = "";
			QueryColumn qc = qt.GetQueryColumnByName(colName);
			if (qc == null || Lex.IsNullOrEmpty(qc.Criteria)) return "";
			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(qc.Criteria);
			if (psc.ValueList == null) return "";
			foreach (string s in psc.ValueList)
			{
				if (list != "") list += ", ";
				list += s;
			}

			return list;
		}

		/// <summary>
		/// Convert a simple value list to criteria for a column
		/// </summary>
		/// <param name="colName"></param>
		/// <param name="list"></param>

		public static QueryColumn SetListCriteriaForCol(
			QueryTable qt,
			string colName,
			string list)
		{
			string tok;

			QueryColumn qc = qt.GetQueryColumnByName(colName);
			if (qc == null) return null;

			if (Lex.Eq(list, "All")) list = ""; // treat all as no criteria
			List<string> values = Csv.SplitCsvString(list);
			string c = "", cd = "";
			foreach (string s in values)
			{
				tok = Lex.RemoveAllQuotes(s);
				if (Lex.IsNullOrEmpty(tok)) continue;

				if (cd != "") cd += ", ";
				cd += tok;

				if (c != "") c += ", ";
				if (!qc.MetaColumn.IsNumeric) // quote it if not numeric
					tok = Lex.AddSingleQuotes(tok);
				c += tok;
			}

			if (!Lex.IsNullOrEmpty(c))
				c = qc.MetaColumn.Name + " IN (" + c + ")";

			qc.Criteria = c;
			qc.CriteriaDisplay = cd;
			return qc;
		}

/// <summary>
/// Set the initial browse page for the query based on the preferred view
/// </summary>
/// <param name="preferredView"></param>
/// <param name="q"></param>

		public static void  SetInitialBrowsePageToPreferredView (
			string preferredView, 
			Query q)
		{
			string pv =  preferredView;
			int pi = 0;
			if (Lex.Eq(pv, "Table")) pi = 0;
			else if (Lex.Eq(pv, "Map")) pi = 1;
			else if (Lex.Eq(pv, "Heatmap")) pi = 2;
			else if (Lex.Eq(pv, "Network")) pi = 3;
			else if (Lex.Eq(pv, "WebPlayer")) { } // not currently supported

			if (pi < q.ResultsPages.Pages.Count) 
				q.InitialBrowsePage = pi;

			return;
		}

/// <summary>
/// Get the name of the main summarized table name associated with the current database 
/// </summary>
/// <returns></returns>

		public string GetSummarizedMetaTableName()
		{
			if (Lex.Eq(DbName, "<baseDatabaseName"))
				return MultiDbAssayDataNames.BaseTableName;
			else
				return MultiDbAssayDataNames.CombinedTableName;
		}

/// <summary>
/// Serialize
/// </summary>
/// <returns></returns>

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			XmlTextWriter tw = mstw.Writer;
			tw.Formatting = Formatting.Indented;
			Serialize(tw);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

/// <summary>
/// Serialize to an XmlTextWriter
/// </summary>
/// <param name="tw"></param>

		public void Serialize(XmlTextWriter tw)
		{
			tw.WriteStartElement("MultiDbViewerPrefs");

			tw.WriteAttributeString("DbName", DbName);
			tw.WriteAttributeString("CidCriteria", CidCriteria);
			tw.WriteAttributeString("TargetList", TargetList);
			tw.WriteAttributeString("GeneFamilies", GeneFamilies);

			tw.WriteAttributeString("AssayTypesToInclude", AssayTypesToInclude);
			tw.WriteAttributeString("AssayModesToInclude", AssayModesToInclude);
			tw.WriteAttributeString("ResultTypesToInclude", ResultTypesToInclude);
			tw.WriteAttributeString("UseTopLevelResultTypeOnly", UseTopLevelResultTypeOnly.ToString());

			tw.WriteAttributeString("TargetsWithActivesOnly", TargetsWithActivesOnly.ToString());
			tw.WriteAttributeString("CrcUpperBound", CrcUpperBound);
			tw.WriteAttributeString("SpLowerBound", SpLowerBound);
			tw.WriteAttributeString("UseMeans", UseMeans.ToString());

			tw.WriteAttributeString("IncludeStructures", IncludeStructures.ToString());
			tw.WriteAttributeString("FilterableTargets", FilterableTargets);

			tw.WriteAttributeString("TargetMap", TargetMapName);
			tw.WriteAttributeString("UserMapNames", UserMapNames);

			tw.WriteAttributeString("OutputFormat", PreferredView);
			tw.WriteAttributeString("FormatForMobius", FormatForMobius.ToString());
			tw.WriteAttributeString("ShowAdvancedDialog", ShowAdvancedDialog.ToString());

			tw.WriteEndElement();
		}

/// <summary>
/// Deserialize
/// </summary>
/// <param name="serializedForm"></param>
/// <returns></returns>

		public static TargetSummaryOptions Deserialize(string serializedForm)
		{

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);
			XmlTextReader tr = mstr.Reader;
			tr.Read(); 
			tr.MoveToContent();

			TargetSummaryOptions p = Deserialize(tr);
			mstr.Close();
			return p;
		}

/// <summary>
/// Deserialize from XmlTextReader
/// </summary>
/// <param name="tr"></param>
/// <returns></returns>

		public static TargetSummaryOptions Deserialize(XmlTextReader tr)
		{
			TargetSummaryOptions p = new TargetSummaryOptions();

			if (!Lex.Eq(tr.Name, "MultiDbViewerPrefs") && // new name
				!Lex.Eq(tr.Name, "TargetResultsViewerParms")) // old name
					throw new Exception("\"MultiDbViewerPrefs\" element not found");

			XmlUtil.GetStringAttribute(tr, "DbName", ref p.DbName);
			XmlUtil.GetStringAttribute(tr, "CidCriteria", ref p.CidCriteria);
			XmlUtil.GetStringAttribute(tr, "TargetList", ref p.TargetList);
			XmlUtil.GetStringAttribute(tr, "GeneFamilies", ref p.GeneFamilies);
			if (Lex.Contains(p.GeneFamilies, "Ion, channel")) // fixup
				p.GeneFamilies = Lex.Replace(p.GeneFamilies, "Ion, channel", "Ion channel");

			if (Lex.Contains(p.GeneFamilies, "Nuclear, hormone, receptor")) 
				p.GeneFamilies = Lex.Replace(p.GeneFamilies, "Nuclear, hormone, receptor", "Nuclear hormone receptor");

			XmlUtil.GetStringAttribute(tr, "AssayTypesToInclude", ref p.AssayTypesToInclude);
			XmlUtil.GetStringAttribute(tr, "AssayTypesToInclude", ref p.AssayTypesToInclude);
			XmlUtil.GetStringAttribute(tr, "ResultTypesToInclude", ref p.ResultTypesToInclude);
			XmlUtil.GetBoolAttribute(tr, "UseTopLevelResultTypeOnly", ref p.UseTopLevelResultTypeOnly);

			XmlUtil.GetBoolAttribute(tr, "TargetsWithActivesOnly", ref p.TargetsWithActivesOnly);
			XmlUtil.GetStringAttribute(tr, "CrcUpperBound", ref p.CrcUpperBound);
			XmlUtil.GetStringAttribute(tr, "SpLowerBound", ref p.SpLowerBound);
			XmlUtil.GetBoolAttribute(tr, "UseMeans", ref p.UseMeans);

			XmlUtil.GetBoolAttribute(tr, "IncludeStructures", ref p.IncludeStructures);
			XmlUtil.GetStringAttribute(tr, "FilterableTargets", ref p.FilterableTargets);

			XmlUtil.GetStringAttribute(tr, "TargetMap", ref p.TargetMapName);
			XmlUtil.GetStringAttribute(tr, "UserMapNames", ref p.UserMapNames);

			XmlUtil.GetStringAttribute(tr, "OutputFormat", ref p.PreferredView);
			XmlUtil.GetBoolAttribute(tr, "FormatForMobius", ref p.FormatForMobius);
			XmlUtil.GetBoolAttribute(tr, "ShowAdvancedDialog", ref p.ShowAdvancedDialog);

			return p;
		}

	}
}
