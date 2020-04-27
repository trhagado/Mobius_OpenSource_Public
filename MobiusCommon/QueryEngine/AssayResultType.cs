using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mobius.QueryEngineLibrary
{
/// <summary>
/// Assay result type information
/// </summary>

	public class AssayResultType
	{
		public string Name = ""; // type name
		public int ResultTypeId1 = -1; // (member vars not yet in use)
		public int ResultTypeId2 = -1;
		public bool IsBiologicalResultType = false;
		public bool IsStatResultType = false;
		public int PreferredRelativePosition = -1; // in metatable
		public string KeyResultType = ""; // key = code, value = type e.g. SP, CRC, Primary, Secondary
		public string SummarizedMeanTypeCode = ""; // Summarization method
		public int DimensionTypeId = -1; // dmnsn_typ_id: 111 = 'Result', 222 = 'Stat'
		public bool Deselected = false;

// Statics

		public static Dictionary<int, AssayResultType> ResultTypeDict1 
			{ get { if (resultTypeDict1 == null) BuildResultTypeDicts(); return resultTypeDict1; } }

		public static Dictionary<int, AssayResultType> ResultTypeDict2 
			{ get { if (resultTypeDict2 == null) BuildResultTypeDicts(); return resultTypeDict2; } }

		public static Dictionary<string, int> ExtraResultTypeAttributes = // extra result type attributes
			null; // Keyed on col name or result type Id (value is relative preferred column position)
		public static Dictionary<string, string> KeyResultTypes = null; // key = code, value = type e.g. SP, CRC, Primary, Secondary
		public static List<string> TextResultTypes = null; // identifying info for types that should be treated as text
		public static Dictionary<string, List<string>> MergedResultTypes = null; // merged result type dict. Key is resultTypeId, resultTypeId2, value is list of types that can be merged with
		public static Dictionary<string, List<string>> MergedResultTypes2 = null; // merged result type dict with result code order reversed
		public static List<string> ImageResultTypes = null; // identifying info for types that can have images
		public static HashSet<string> DeselectedResultTypes = null;

		public const int AssayRsltTypIdDim = 70; // result type from assay_rslt_typ_id for this dimension value
		public const int AssayStatTypIdDim = 61; // result type from assay_stat_typ_id for this dimension value

		public static string SelectExpectedResultTypesSqlTemplate = // template SQL for selecting expected result type info
				@"select <columns> /* get expected types for an assay */
				from <tables>
				where <conditions>
				order by lower(<orderColumns>)";

/// <summary>
/// Return true if the result type for this column contains a potency value
/// </summary>
/// <param name="mc"></param>
/// <returns></returns>

		public static bool IsPotencyColumn(
			MetaColumn mc)
		{
			bool result = CanHaveRelatedImageColumn(mc);
			return result;
		}

		/// <summary>
		/// Return metacolumn we can drill down from this column to a response curve image
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static MetaColumn GetRelatedImageColumn(
			MetaColumn mc)
		{
			if (mc.DataType == MetaColumnType.Image) return mc;

			if (!CanHaveRelatedImageColumn(mc)) return null;

			MetaColumn crcMc = null; // save last image col seen here
			foreach (MetaColumn mc2 in mc.MetaTable.MetaColumns)
			{
				if (mc2.DataType != MetaColumnType.Image) continue;

				crcMc = mc2;
				if (mc2.ResultCode == mc.ResultCode) return mc2;
			}

			if (HasRelatedImageColumn(mc)) return crcMc; // image exists but not mapped to this metacolumn
			else return null;
		}

		/// <summary>
		/// Return true if this column can have an associated image column 
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static bool CanHaveRelatedImageColumn(
			MetaColumn mc)
		{
			if (ImageResultTypes == null) ReadImageResultTypes();

			if (!mc.Name.StartsWith("R_")) return false; // must be assay metadata result type column

			foreach (string s in ImageResultTypes)
			{
				//if (s == "7168.11446") mc = mc; // debug

				if (Lex.IsDouble(s)) 
				{
					string resultCode = mc.Name.Substring(2); // get the code

					if (s == resultCode)
						return true; // matches result code

					if (s == mc.MetaTable.Code + "." + resultCode)
						return true; 
				}

				else if (Lex.Contains(mc.Label, s)) // see if label is contained
					return true; // string contained in label
			}

			return false;
		}

		/// <summary>
		/// Return true if this column can be a secondary image column 
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static bool CanBeSecondaryImageColumnObsolete(
			MetaColumn mc)
		{
			if (!CanHaveRelatedImageColumn(mc)) return false; // must be image column

			bool allowSecondaryImageColumns = ServicesIniFile.ReadBool("AllowSecondaryImageColumns", false);
			if (!allowSecondaryImageColumns) return false;

			if (Lex.Contains(mc.Label, "Non Standard") || // nonstandards can be secondary
			 Lex.Contains(mc.Label, "_NS") ||
			 Lex.Contains(mc.Label, "(NS)")) 
				return true; 

			foreach (string s in ImageResultTypes) 
			{
				string resultCode = mc.Name.Substring(2); // get the assay metadata code

				if (s == mc.MetaTable.Code + "." + resultCode)
					return true; 
			}

			return false;
		}

		/// <summary>
		/// Return true if this column always has an associated image column 
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>
		/// 
		public static bool HasRelatedImageColumn(
			MetaColumn mc)
		{
			if (ImageResultTypes == null) ReadImageResultTypes();

			foreach (string s in ImageResultTypes)
			{
				if (s == mc.MetaTable.Code + "." + mc.ResultCode)
					return true; 
			}

			return false;
		}

		/// <summary>
		/// See if column is a deselected result type
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static bool IsDeselectedResultType(
			MetaColumn mc)
		{
			StreamReader sr;
			string fileName, txt;
			int i1;

			if (DeselectedResultTypes == null)
			{
				DeselectedResultTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				fileName = ServicesDirs.MetaDataDir + @"\Metatables\DeselectedResultTypes.txt";
				try { sr = new StreamReader(fileName); }
				catch (Exception ex) { return false; }

				while (true)
				{
					txt = sr.ReadLine();
					if (txt == null) break;

					txt = txt.Trim();
					if (Lex.IsUndefined(txt)) continue;

					for (i1 = 0; i1 < txt.Length; i1++)
					{
						if (!Char.IsDigit(txt[i1])) break;
					}
					txt = txt.Substring(0, i1);
					if (Lex.IsUndefined(txt)) continue;
					DeselectedResultTypes.Add(txt);
				}

				sr.Close();
			}

			//if (mc.ResultCode == "106306") mc = mc; // debug
			if (DeselectedResultTypes.Contains(mc.ResultCode)) return true;
			else return false;
		}

		/// <summary>
		/// Read ExtraResultTypeAttributes including preferred column order info
		/// </summary>

		public static void ReadExtraResultTypeAttributes()
		{
			StreamReader sr;
			string fileName, txt, txt2;
			string[] sa = null;

			int t0 = TimeOfDay.Milliseconds();

			ExtraResultTypeAttributes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			KeyResultTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			fileName = ServicesDirs.MetaDataDir + @"\Metatables\ExtraResultTypeAttributes.txt";
			try
			{
				sr = new StreamReader(fileName);
			}
			catch (Exception ex)
			{
				DebugLog.Message("Error opening file: " + fileName + ", " + ex.Message);
				return; // just ignore for now if not found
			}

			int row = 0;
			while (true)
			{
				txt = sr.ReadLine();

				if (txt == null) break;
				int i1 = txt.IndexOf(";"); // remove any comment
				if (i1 >= 0) txt = txt.Substring(0, i1);
				if (Lex.IsUndefined(txt)) continue;

				if (!txt.Contains("\"")) sa = txt.Split(','); // handle quoted items
				else sa = Lex.ParseAllExcludingDelimiters(txt).ToArray(); 

				if (sa.Length < 1) continue;

				string typeId = sa[0].Trim().ToUpper();
				ExtraResultTypeAttributes[typeId] = row;

				if (sa.Length >= 3 && Lex.IsDefined(sa[2]))
				{
					string krt = sa[2].Trim().ToUpper();
					if (krt == "SP" || krt == "CRC")
					 KeyResultTypes[typeId] = krt;
				}

				row++;
			}

			sr.Close();
			return;
		}

		/// <summary>
		/// Check if an assay metadata result type id is a key result & return the conc type if so
		/// </summary>
		/// <param name="assayMetadataType"></param>
		/// <param name="resultTypeConcType"></param>
		/// <returns></returns>

		public static bool IskeyResultType(
			int assayMetadataType,
			out string resultTypeConcType)
		{
			resultTypeConcType = "";

			if (KeyResultTypes == null)
				ReadExtraResultTypeAttributes();

			string rtString = assayMetadataType.ToString();
			if (KeyResultTypes.ContainsKey(rtString))
			{
				resultTypeConcType = KeyResultTypes[rtString];
				return true;
			}

			else return false;
		}

		/// <summary>
		/// ReadMergedResultTypes
		/// </summary>

		public static void ReadMergedResultTypes()
		{
			int i1;

			MergedResultTypes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			MergedResultTypes2 = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

			string fileName = ServicesDirs.MetaDataDir + @"\Metatables\MergedResultTypes.txt";
			StreamReader sr;
			try
			{
				sr = new StreamReader(fileName);
			}
			catch (Exception ex)
			{ return; }

			while (true)
			{
				string txt = sr.ReadLine();
				if (txt == null) break;
				i1 = txt.IndexOf(";");
				if (i1 >= 0) txt = txt.Substring(0, i1);
				if (Lex.IsUndefined(txt)) continue;

				string[] sa = txt.Split(',');
				if (sa.Length != 3) continue;
				for (i1 = 0; i1 < sa.Length; i1++)
					sa[i1] = sa[i1].Trim();

				string typeKey = sa[0] + "," + sa[1];
				string mergeCode = sa[2];
				if (!MergedResultTypes.ContainsKey(typeKey))
					MergedResultTypes[typeKey] = new List<string>();
				if (!MergedResultTypes[typeKey].Contains(mergeCode))
					MergedResultTypes[typeKey].Add(mergeCode);

				string typeKey2= sa[0] + "," + sa[2];
				string mergeCode2 = sa[1];
				if (!MergedResultTypes2.ContainsKey(typeKey2))
					MergedResultTypes2[typeKey2] = new List<string>();
				if (!MergedResultTypes2[typeKey2].Contains(mergeCode2))
					MergedResultTypes2[typeKey2].Add(mergeCode2);
			}

			sr.Close();
			return;
		}

		/// <summary>
		/// ReadImageResultTypes
		/// </summary>

		public static void ReadImageResultTypes()
		{
			ImageResultTypes = new List<string>();

			string fileName = ServicesDirs.MetaDataDir + @"\Metatables\ImageResultTypes.txt";
			StreamReader sr;
			try
			{
				sr = new StreamReader(fileName);
			}
			catch (Exception ex)
			{ return; }

			while (true)
			{
				string txt = sr.ReadLine();
				if (txt == null) break;
				if (txt == "" || txt.StartsWith(";")) continue;
				ImageResultTypes.Add(txt.Trim());
			}

			sr.Close();
		}

		/// <summary>
		/// Return true if this column is a forced text result type
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static bool IsTextResultType(
			MetaColumn mc)
		{
			if (TextResultTypes == null) ReadTextResultTypes();

			foreach (string s in TextResultTypes)
			{
				if (s == mc.ResultCode2) // compare with assay metadata result type code rather than the internal result type code (ResultCode)
					return true; // matches result code

				if (s == mc.MetaTable.Code + "." + mc.ResultCode2)
					return true; 
			}

			return false;
		}

		public static void ReadTextResultTypes()
		{
			TextResultTypes = new List<string>();

			string fileName = ServicesDirs.MetaDataDir + @"\Metatables\TextResultTypes.txt";
			StreamReader sr;
			try
			{
				sr = new StreamReader(fileName);
			}
			catch (Exception ex)
			{ return; }

			while (true)
			{
				string txt = sr.ReadLine();
				if (txt == null) break;
				if (txt == "" || txt.StartsWith(";")) continue;
				TextResultTypes.Add(txt.Trim());
			}

			sr.Close();
		}

		/// <summary>
		/// Get dict of assay result types
		/// </summary>
		/// <param name="assayId"></param>
		/// <returns></returns>

		public static Dictionary<int, int> GetExpectedAssayResultTypes(int assayId)
		{
			Dictionary<int, int> dict = new Dictionary<int, int>();
			DbCommandMx drd = new DbCommandMx();

			string sql = AssayResultType.SelectExpectedResultTypesSqlTemplate;

			drd.PrepareMultipleParameter(sql, 1); // (takes ~700 ms first time)
			drd.ExecuteReader(assayId);

			while (drd.Read())
			{
				int resultType = drd.GetIntByName("<resultTypeId>");
				int secondaryResultType = drd.GetIntByName("<secondaryResultTypeId");
				dict[secondaryResultType] = resultType;
			}

			drd.Dispose();

			return dict;
		}

		/// <summary>
		/// Get dict relating types between different assay databases
		/// </summary>

		static void BuildResultTypeDicts()
		{
			string sql = @"
			select <columns>
      from <tables>
      order by lower(<orderColumns>)";

			resultTypeDict1 = new Dictionary<int, AssayResultType>();
			resultTypeDict2 = new Dictionary<int, AssayResultType>();
			DbCommandMx drd = new DbCommandMx();

			drd.Prepare(sql);
			drd.ExecuteReader();

			while (drd.Read())
			{
				AssayResultType r = new AssayResultType();
				r.Name = drd.GetStringByName("rslt_typ_nm");
				r.ResultTypeId1 = drd.GetIntByName("rslt_typ_id");
				r.ResultTypeId2 = drd.GetIntByName("SECONDARY_RSLT_TYP_ID");
				r.SummarizedMeanTypeCode = drd.GetStringByName("smrzd_mean_typ_cd");

				r.DimensionTypeId = drd.GetIntByName("dmnsn_typ_id");
				if (r.DimensionTypeId != 70) // dmnsn_typ_id: 70 = 'Result', 61 = 'Stat'
					r.ResultTypeId2 = -r.ResultTypeId2;

				resultTypeDict1[r.ResultTypeId1] = r;
				resultTypeDict2[r.ResultTypeId2] = r;
			}

			drd.Dispose();

			return;
		}

		static Dictionary<int, AssayResultType> resultTypeDict1 = null;
		static Dictionary<int, AssayResultType> resultTypeDict2 = null;


	}
}
