using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Mobius.Data
{
	/// <summary>
	/// Summary description for CompoundId.
	/// Compound ids (cids) are used in several ways:
	/// 1. They exist in databases
	/// 2. Users enter them
	/// 3. They exist in stored lists
	/// 4. They are used internally in the program
	/// 
	/// Compound ids may be stored as numbers or as strings (which may be numbers, e.g 012345 for MDDR)
	/// Internally within Mobius and in stored lists cids are stored as normalized strings. 
	/// Normalization includes storing numeric cids as fixed length strings and adding
	/// prefixes as necessary to keep cids within each database unique.
	/// The exception to this is annotation data in which the number is stored as
	/// a string with no leading zeros to allow searches across numeric/string 
	/// columns to match.
	/// Numeric cids displayed to the user and and exported in various forms are normally formatted without leading
	/// zeros unless the RemoveLeadingZerosFromCids flag is set to false.
	/// </summary>

	[DataContract]
	public class CompoundId : MobiusDataType, IComparable
	{
/// <summary>
/// String value of the compound id
/// </summary>

		[DataMember]
		public String Value;

// Dictionaries mapping prefixes to associated root table info

		static Dictionary<string, RootTable> PrefixToRootTableDict = new Dictionary<string, RootTable>();
		static Dictionary<string, MetaTable> PrefixToRootMetaTableDict = new Dictionary<string, MetaTable>();

/// <summary>
/// Basic constructor
/// </summary>

		public CompoundId()
		{
		}

/// <summary>
/// Construct with intial value
/// </summary>
/// <param name="value"></param>

		public CompoundId(
			string value)
		{
			Value = Normalize(value); // store normalized value
		}

// State info to speed up Normalize function (not thread safe)

		static MetaTable LastNormalizeRootMetaTable = null;
		static RootTable LastNormalizeRootTable = null;
		static string LastNormalizeFmt = null;

/// <summary>
/// Convert a compound number from external to internal format 
/// normalizing and adding a prefix as needed.
/// </summary>
/// <param name="cid"></param>
/// <returns></returns>
/// 
		public static string Normalize (
			string cid)
		{
			return Normalize(cid, null);
		}

/// <summary>
/// Convert a compound number from external to internal format 
/// normalizing and adding a prefix as needed.
/// A false prefix is added for compound collections that don't normally include
/// a prefix or contain a overlapping prefix to keep compoundIds associated
/// with the proper compound collection.
/// This routine handles several cases:
/// 1 - If mt defined & root is not a compound table then just return key as is
/// 2 - If mt defined & root is a UCDB then normalize to fixed length if numeric otherwise return as is
/// 3 - If mt defined & compound table root & no prefix then get any default prefix
/// 4 - If string cid value then add any prefix & return as is
/// 5 - if numeric cid (stored as integer or string) then add any prefix & format numeric part
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static string Normalize (
			string unnormalizedCid,
			MetaTable mt)
		{
			RootTable rti = null;
			MetaTable rootMt;
			string prefix="", suffix="", remainder = "", nTxt="", fmt, nCid;
			bool dashBeforeNumber, canFormat;
			int number = -1;

			if (mt == null) mt = mt; // debug

			string cid = NormalizeCase(unnormalizedCid, mt);
			if (String.IsNullOrEmpty(cid)) return cid;

			if (cid.IndexOf(",") >= 0) // if comma in list then just use chars up to comma
				cid = cid.Substring(0, cid.IndexOf(","));

			AnalyzeCid(cid, mt, out prefix, out dashBeforeNumber, out number, out suffix, out remainder, out rti, out rootMt, out canFormat); 
			if (!canFormat) return cid;

			//if (!CanFormat(cid, prefix

			//if (!String.IsNullOrEmpty(remainder)) // multiple tokens, just return as is
			//  return cid; // (for a cid like "2." the wrong value is returned, i.e. 2.

//			if (Lex.Eq(prefix, "MDDR") && number == 91341) prefix = prefix; // debug

			if (mt != null) // know the metatable this should belong to
			{ // note: for multi-root table search the root table may not correspond to the compoundId prefix & value
				if (!RootTable.IsStructureDatabaseRootTable(mt.Root)) // non structure root table?
				{
					if (mt.Root.KeyMetaColumn.IsNumeric && Lex.IsInteger(cid))
					{ // if numeric then default internal format is length 10 with leading zeros
						number = int.Parse(cid);
						nCid = string.Format("{0:0000000000}", number);
						return nCid;
					}

					else return cid; // just return as is
				}

				else if (UserObject.IsUserDatabaseStructureTableName(mt.Root.Name))
					return NormalizeForDatabase(cid, mt); // same as for database if user database

				else // Normal case, non-userDatabase structure root table
				{
					rti = RootTable.GetFromTableName(mt.Root.Name);
					if (prefix == "") prefix = GetPrefixFromRootTable(mt);

					if (rti != null && rti.IsStringType && String.IsNullOrEmpty(rti.InternalNumberFormat))
					{ // general unformatted string, just add any required prefix & return
						if (!String.IsNullOrEmpty(rti.Prefix) && !Lex.StartsWith(cid, rti.Prefix))
							nCid = rti.Prefix + cid; // add prefix if needed
						else nCid = cid; // just return as is
						return nCid;
					}
				}
			}

			else // metatable not defined, try to determine from prefix
			{
				if (!String.IsNullOrEmpty(prefix)) 
					rti = RootTable.GetFromPrefix(prefix);

				else if (Lex.IsInteger(cid)) // if integer cid then use default integer cid database
					rti = RootTable.GetFromPrefix("");
			}

			if (rti==null || Lex.IsNullOrEmpty(rti.InternalNumberFormat))
				fmt = "{0}"; // unknown prefix
			else fmt = "{0," + rti.InternalNumberFormat + "}";

			if (prefix=="") 
				suffix = ""; // if no prefix, disallow suffix to avoid invalid numbers for numeric keys (possibly enhance later)

			if (number<0) nTxt = ""; // no number
			else 
			{
				try {	nTxt = String.Format(fmt,number); }
				catch (Exception ex) { nTxt = number.ToString(); }
			}

			AddDashIfAppropriate(ref prefix, dashBeforeNumber, rti);

			nCid = prefix + nTxt + suffix;
			if (nCid == "") nCid = cid.Trim(); // if nothing then return original input
			return nCid;
		}

/// <summary>
/// Add dash to prefix if appropriate
/// May be because the prefix normally has a dash or because of negative cid value
/// </summary>
/// <param name="prefix"></param>
/// <param name="dashBeforeNumber"></param>
/// <param name="rti"></param>

		static void AddDashIfAppropriate(
			ref string prefix,
			bool dashBeforeNumber,
			RootTable rti)
		{
			if (prefix.EndsWith("-")) return;

			if (rti == null) return;

			if ((prefix != "" && rti.Prefix.EndsWith("-")) || // if we have a prefix & the prefix for this table ends with a dash
			 (dashBeforeNumber && rti.NegativeNumberAllowed)) // or there's a dash & negative cids are allowed
				prefix += "-";

			return;
		}

/// <summary>
/// Normalize compound number for database searching/storage
/// </summary>
/// <param name="cid"></param>
/// <returns></returns>

		public static string NormalizeForDatabase (
			string cid)
		{
			return NormalizeForDatabase(cid,null);
		}

/// <summary>
/// Normalize compound number for database searching/storage
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static string NormalizeForDatabase (
			string unnormalizedCid,
			MetaTable mt)
		{
			RootTable rti = null;
			MetaTable rootMt;
			string cid, prefix = "", suffix = "", remainder = "", nTxt = "", fmt, nCid;
			bool dashBeforeNumber = false, canFormat;
			int number = -1;

//			if (cid.Contains("91341")) cid = cid; // debug
//			if (mt == null) mt = mt; // debug

			cid = NormalizeCase(unnormalizedCid, mt);
			if (String.IsNullOrEmpty(cid)) return cid;

			if (cid.Contains(" ")) cid = cid.Replace(" ", "_"); // replace any spaces with underscores
			if (cid.Contains("\n")) cid = cid.Replace("\n", ""); // remove newlines
			if (cid.Contains("\r")) cid = cid.Replace("\r", ""); // remove returns

			if (mt != null && !RootTable.IsStructureDatabaseRootTable(mt.Root))
				return cid;

			if (RootTable.IsMultiplePrefixTable(mt))
				return cid; // no formatting just return as is

			if (mt != null && UserObject.IsUserDatabaseStructureTableName(mt.Root.Name))
			{ // user database
				prefix = "userdatabase"; // used to lookup format
				if (Lex.IsInteger(cid)) number = Int32.Parse(cid);
				else suffix = cid;
			}

			else // other database
			{
				if (mt != null && RootTable.GetFromTableName(mt.Root.Name) == null)
					return cid; // if not a recognized database return cid as is

				AnalyzeCid(cid, mt, out prefix, out dashBeforeNumber, out number, out suffix, out remainder, out rti, out rootMt, out canFormat);
				if (!canFormat) return cid;

				string mtPrefix = "";
				if (mt != null)	// get any prefix for metatable
					mtPrefix = GetPrefixFromRootTable(mt);

				if (mtPrefix.EndsWith("-") && prefix != "" && !prefix.EndsWith("-"))
				{ // adjust to allow prefix match
					prefix += "-";
					dashBeforeNumber = false;
				}

				if (prefix == "") // if no prefix in number then add any metatable prefix
				{
					if (mtPrefix != "") prefix = mtPrefix;
					else prefix = "none";
				}

				else if (mt != null && !Lex.Eq(prefix, mtPrefix))
					return null; // if prefix invalid for database return null
			}

			rti = RootTable.GetFromPrefix(prefix);
			if (rti == null && prefix == "userdatabase")
			{
				fmt = "{0,8:00000000}"; // default format for user databases
				prefix = "";
			}

			else if (rti == null || Lex.IsNullOrEmpty(rti.InternalNumberFormat))
			{ // unknown format
				fmt = "{0}"; // default format
				if (rti != null && !rti.PrefixIsStored) prefix = ""; // database doesn't contain prefix
			}

			else // use supplied format
			{
				fmt = "{0," + rti.InternalNumberFormat + "}";
				if (!rti.PrefixIsStored) prefix = ""; // database doesn't contain prefix
			}

			if (number < 0) nTxt = "";
			else 
			{
				try {	nTxt = String.Format(fmt,number); }
				catch (Exception ex) { nTxt = number.ToString(); }
			}

			//if (Lex.StartsWith(cid, "GGO-")) nTxt = "-" + nTxt; // hack for GeneGo database, fix later

			if (prefix == "none") prefix = "";

			AddDashIfAppropriate(ref prefix, dashBeforeNumber, rti);

			nCid = (prefix + nTxt + suffix).ToUpper();
			if (nCid.Contains(" ")) nCid = nCid.Replace(" ", "_"); // replace any internal spaces with underscores
			return nCid;
		}

/// <summary>
/// Normalize from int value (fast)
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static string Normalize(
			int intCid,
			MetaTable mt)
		{
			RootTable rt;
			string fmt;

			if (mt == null) return intCid.ToString();

			if (mt.Root == LastNormalizeRootMetaTable)
			{ // same as last time?
				rt = LastNormalizeRootTable;
				fmt = LastNormalizeFmt;
			}

			else
			{
				MetaTable mtRoot = mt.Root;
				rt = RootTable.GetFromTableName(mtRoot.Name);
				if (rt == null) return intCid.ToString();
				fmt = "{0," + rt.InternalNumberFormat + "}";

				LastNormalizeRootMetaTable = mtRoot;
				LastNormalizeRootTable = rt;
				LastNormalizeFmt = fmt;
			}

			string cidString = String.Format(fmt, intCid);
			if (rt.HasSinglePrefix) cidString = rt.Prefix + cidString;
			return cidString;
		}

		/// <summary>
		/// NormalizeCase of Cid
		/// </summary>
		/// <param name="unnormalizedCid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static string NormalizeCase(
			string unnormalizedCid,
			MetaTable mt)
		{
			if (mt == null || mt.KeyMetaColumn == null)
				return Trim(unnormalizedCid.ToUpper()); // default to upper case

			MetaColumn mc = mt.KeyMetaColumn;

			string cid = Trim(unnormalizedCid);

			if (mc.TextCase == ColumnTextCaseEnum.Upper || mc.TextCase == ColumnTextCaseEnum.Unknown)
				cid = cid.ToUpper();
			else if (mc.TextCase == ColumnTextCaseEnum.Lower)
				cid = cid.ToLower();
			else if (mc.TextCase == ColumnTextCaseEnum.Mixed)
				cid = cid; // leave as is
			else cid = Trim(unnormalizedCid).ToUpper(); // make upper case (normal case)

			return cid;
		}

/// <summary>
/// Convert a compound number from internal to external format
/// </summary>
/// <param name="cid"></param>
/// <returns></returns>

		public static string Format (
			string cid)
		{
			return Format(cid, null);
		}

/// <summary>
/// Convert a compound number from internal to external format
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static string Format (
			string normalizedCid,
			MetaTable mt)
		{
			string prefix="", suffix="", remainder = "", nTxt, fmt, fCid;
			bool dashBeforeNumber, canFormat;
			RootTable rti = null, rtiBase;
			MetaTable rootMt;
			int number = -1;

			//if (mt == null) mt = mt; // debug

			string cid = normalizedCid;

			if (String.IsNullOrEmpty(cid)) return cid;

			if (mt != null && !RootTable.IsStructureDatabaseRootTable(mt.Root)) // non structure root table
			{
				if (mt.Root.KeyMetaColumn.IsNumeric && Lex.IsInteger(cid))
				{ // if numeric then return as simple integer with leading zeros removed
					number = int.Parse(cid);
					fCid = number.ToString();
					return fCid;
				}

				else return cid; // just return as is
			}

			cid = Lex.RemoveAllQuotes(cid.Trim()).Trim(); // clean up
			if (cid.Length==0) return ""; // something really there

			if (mt != null && UserObject.IsUserDatabaseStructureTableName(mt.Root.Name))
			{ // user database
				if (Lex.IsInteger(cid)) // if number format with no leading zeros
					return Int32.Parse(cid).ToString();
				else return cid; // otherwise return as is
			}

			if (RootTable.IsMultiplePrefixTable(mt))
				return cid; // no formatting just return as is

			AnalyzeCid(cid, mt, out prefix, out dashBeforeNumber, out number, out suffix, out remainder, out rti, out rootMt, out canFormat);
			if (!canFormat) return cid;

			if (prefix=="") prefix="none";

			rti = RootTable.GetFromPrefix(prefix);

			if (rti == null || Lex.IsNullOrEmpty(rti.ExternalNumberFormat))
				fmt = "{0}"; // unknown prefix

			else fmt = "{0," + rti.ExternalNumberFormat + "}";

			if (prefix=="none") prefix = "";

			if (number<0) nTxt = "";
			else 
			{
				try {	nTxt = String.Format(fmt,number); }
				catch (Exception ex) { nTxt = number.ToString(); }
			}

			AddDashIfAppropriate(ref prefix, dashBeforeNumber, rti);

			fCid = prefix + nTxt + suffix;

			if (fCid == "") return cid;
			return fCid;
		}

/// <summary>
/// Trim extra spaces and any quotes from compound id
/// </summary>
/// <param name="cid"></param>
/// <returns></returns>

		public static string Trim(
			string cid)
		{
			if (String.IsNullOrEmpty(cid)) return cid;
			cid = cid.Trim();
			if (cid.Length == 0) return cid;
			if (cid[0] == '\'' || cid[0] == '\"')
			{
				cid = Lex.RemoveAllQuotes(cid); // remove quotes
				cid = cid.Trim();
			}
			return cid;
		}

/// <summary>
/// Find best matching number
/// </summary>
/// <param name="cid"></param>
/// <returns></returns>

		public static string BestMatch (
			string cid,
			MetaTable mt)
		{
			return Normalize(cid, mt); // todo: just normalize for now, do something better later
		}

/// <summary>
/// Return true if cid is valid for its associated database
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static bool IsValidForDatabase (
			string cid,
			MetaTable mt)
		{
			if (Lex.IsNullOrEmpty(cid)) return false;
			if (mt == null) return true; // assume ok if no table

			mt = mt.Root;
			string intCid = Normalize(cid, mt); // internal format
			string fmtCid = Format(intCid, mt); // external format
			string dbCid = NormalizeForDatabase(cid, mt); // db format
			if (Lex.IsNullOrEmpty(intCid) ||
				Lex.IsNullOrEmpty(fmtCid) ||
				Lex.IsNullOrEmpty(dbCid))
					return false;

			MetaColumnStorageType st = mt.KeyMetaColumn.StorageType; // check storage type
			if ((st == MetaColumnStorageType.Integer || st == MetaColumnStorageType.Decimal) &&
			 !Lex.IsInteger(dbCid))
				return false;

			return true;
		}

/// <summary>
/// Return true if the column is a Corp compound id column
/// </summary>
/// <param name="mc"></param>
/// <returns></returns>

		public static bool IsCorpCompoundId(
			MetaColumn mc)
		{
			if (mc == null) return false;

			if (mc.DataType != MetaColumnType.CompoundId) return false;

			if (!Lex.StartsWith(mc?.MetaTable?.Root?.Name, MetaTable.PrimaryRootTable)) return false;

			return true;
		}

/// <summary>
/// Analyze compound id string
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <param name="prefix"></param>
/// <param name="dashBeforeNumber"></param>
/// <param name="number"></param>
/// <param name="suffix"></param>
/// <param name="remainder"></param>
/// <param name="rti"></param>
/// <param name="rootMt"></param>
/// <param name="canFormat"></param>

		public static void AnalyzeCid (
			string cid,
			MetaTable mt,
			out string prefix,
			out bool dashBeforeNumber,
			out int number,
			out string suffix,
			out string remainder,
			out RootTable rti,
			out MetaTable rootMt,
			out bool canFormat)
		{

// Tests for proper cid conversions
// 1. Quick search
// 2. Key match, equality
// 3. Key match, list editor
// 4. Retrieve without structure & check that struct pops up on mouseover of cid
// 5. Add structure search for struct model retrieved using key
// 6. Run BQ12 to be sure multidatabase search is working (once only)

			canFormat = true; 

			ParseCid(cid, out prefix, out dashBeforeNumber, out number, out suffix, out remainder);
			if (prefix == "CorpId") prefix = ""; // fix for old lists containing CorpId prefix

			if (mt != null && mt.IsUserDatabaseStructureTable) // user database structure table
			{
				rti = RootTable.GetFromPrefix("USERDATABASE");
				rootMt = mt;
			}

			else GetRootMetaTableFromPrefix(prefix, out rti, out rootMt); // normal case

			if (RootTable.IsMultiplePrefixTable(mt))
				canFormat = false; // can't format if multiple prefix table

			if (rti == null) canFormat = false;

			return;
		}

/// <summary>
/// Split Cid into parts
/// </summary>
/// <param name="cid"></param>
/// <param name="prefix"></param>
/// <param name="dashBeforeNumber"></param>
/// <param name="number"></param>
/// <param name="suffix"></param>
/// <param name="remainder"></param>

		public static void ParseCid (
			string cid,
			out string prefix,
			out bool dashBeforeNumber,
			out int number,
			out string suffix,
			out string remainder)
		{
			prefix = suffix = remainder = "";
			dashBeforeNumber = false;
			number = -1;

			if (int.TryParse(cid, out number)) // check for simple number first for faster performance
				return;

			int p = 0; // current position
			if (p >= cid.Length) return;
			int ps = -1;

			if (cid.StartsWith(" "))
			{
				for (p = 0; p < cid.Length; p++) // position past leading spaces
					if (cid[p] != ' ') break;
				if (p >= cid.Length) return;
			}

			if (cid.StartsWith("\'") || cid.StartsWith("\""))
			{
				cid = Lex.RemoveAllQuotes(cid);
				p = 0;
				if (p >= cid.Length) return;
			}

			int i1 = cid.IndexOf('-');
			if (i1 >= 0) // prefix containing dash
			{
				prefix = cid.Substring(0, i1); // get prefix (may be blank if leading dash)
				p = i1 + 1;
				dashBeforeNumber = true;
			}

			else // prefix consists of initial letters
			{
				if (Char.IsLetter(cid[p])) // prefix
				{
					ps = p;
					for (p = p; p < cid.Length; p++)
					{
						if (!Char.IsLetter(cid[p])) break;
					}
					prefix = cid.Substring(ps, p - ps).ToUpper();
					if (p >= cid.Length) return;
				}
			}

			for (p = p; p < cid.Length; p++) // skip non-letters/digits
				if (Char.IsLetterOrDigit(cid[p])) break;
			if (p >= cid.Length) return;

			if (p < cid.Length && Char.IsDigit(cid[p])) // number
			{
				ps = p;
				for (p = p; p < cid.Length; p++) // find end of digits, avoid overflow
					if (!Char.IsDigit(cid[p])) break;

                if (!Int32.TryParse(cid.Substring(ps, p - ps), out number))
                {
                    number = -1; // if parsing failed, probably overflowed
                }

				if (p >= cid.Length) return;
			}

			//for (p = p; p < cid.Length; p++) // skip non-letters
			//  if (Char.IsLetter(cid[p])) break;
			//if (p >= cid.Length) return;

			if (p < cid.Length && Char.IsLetter(cid[p])) // remainder up to space is suffix
			{
				ps = p;
				for (p = p; p < cid.Length; p++)
					if (cid[p] == ' ') break;
				suffix = cid.Substring(ps, p - ps).ToUpper();
				if (p >= cid.Length) return;
			}

			if (p < cid.Length) remainder = cid.Substring(p).Trim(); // something left over

			return;
		}

/// <summary>
/// Get the RootTable associated with a compound id
/// </summary>
/// <param name="cid"></param>
/// <returns></returns>

		public static RootTable GetRootTableFromCid(
			string cid)
		{
			RootTable rootTable;
			MetaTable rootMt;
			string prefix, suffix, remainder, table;
			bool dashBeforeNumber, canFormat;
			int number;

			AnalyzeCid(cid, null, out prefix, out dashBeforeNumber, out number, out suffix, out remainder, out rootTable, out rootMt, out canFormat);
			return rootTable;
		}

		/// <summary>
		/// Get the root table associated with a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		public static MetaTable GetRootMetaTableFromCid(
			string cid)
		{
			return GetRootMetaTableFromCid(cid, null);
		}

/// <summary>
/// Get the root table associated with a compound id prefix
/// </summary>
/// <param name="cid"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static MetaTable GetRootMetaTableFromCid(
			string cid,
			MetaTable mt)
		{
			RootTable rti;
			MetaTable rootMt;
			string prefix, suffix, remainder, table;
			bool dashBeforeNumber, canFormat;
			int number;

			if (mt != null) // if metatable is defined see if it is a root table
			{
				if (mt.IsRootTable) return mt; // if this is a root table then we're done

				if (RootTable.IsMultiplePrefixTable(mt))
					return mt; // if isolated then ignore prefix & use this table
			}

			AnalyzeCid(cid, mt, out prefix, out dashBeforeNumber, out number, out suffix, out remainder, out rti, out rootMt, out canFormat);
			return rootMt;
		}

/// <summary>
/// Get the root table associated with a compound id prefix
/// </summary>
/// <param name="prefix"></param>
/// <returns></returns>

		public static MetaTable GetRootMetaTableFromPrefix (
			string prefix)
		{
			RootTable rti;
			MetaTable rootMt;

			GetRootMetaTableFromPrefix(prefix, out rti, out rootMt);
			return rootMt;
		}

/// <summary>
/// Get the root table associated with a compound id prefix
/// </summary>
/// <param name="prefix"></param>
/// <param name="rti"></param>
/// <param name="mtRoot"></param>

		public static void GetRootMetaTableFromPrefix (
			string prefix,
			out RootTable rti,
			out MetaTable mtRoot)
		{

			rti = null;
			mtRoot = null;

			if (prefix == "") prefix = "none"; // no prefix

			if (PrefixToRootTableDict.ContainsKey(prefix)) // already have root info?
			{
				rti = PrefixToRootTableDict[prefix];
				mtRoot = PrefixToRootMetaTableDict[prefix];
				return;
			}

			rti = RootTable.GetFromPrefix(prefix);
			if (rti == null) return;

			string mtName = rti.MetaTableName;
			mtRoot = MetaTableCollection.Get(mtName);

			PrefixToRootTableDict[prefix] = rti;
			PrefixToRootMetaTableDict[prefix] = mtRoot;

			return;
		}

/// <summary>
/// Get the compound id prefix associated with a metatable
/// </summary>
/// <param name="mt"></param>
/// <returns></returns>

		public static string GetPrefixFromRootTable (
			MetaTable mt)
		{
			string prefix, suffix, table;
			int number;

			if (mt==null) return "";
			mt = mt.Root; // move to root

			RootTable rti = RootTable.GetFromTableName(mt.Name);
			if (rti == null) return "";
			if (Lex.Eq(rti.Prefix, "none") || Lex.Eq(rti.Prefix, "multiple")) return "";
			else return rti.Prefix;
		}

/// <summary>
/// Filter a set of keys by prefix to match just the supplied root
/// </summary>
/// <param name="inputKeys"></param>
/// <param name="rootTable"></param>
/// <returns></returns>

		public static List<string> FilterKeysByRoot(
			List <string> inputKeys,
			MetaTable rootTable)
		{
			if (inputKeys==null) return inputKeys;

			RootTable rti = RootTable.GetFromTableName(rootTable.Name);
			if (rti==null || !rti.IsStructureTable) return inputKeys; // if no structure database associated with root then return keys as is

			string prefix = GetPrefixFromRootTable(rootTable);

			int addCount=0;
			foreach (string key in inputKeys) // see how many keys are associated with the current root
			{
				if ((prefix=="" && key.Length>0 && Char.IsDigit(key[0])) || // number with no prefix
					(prefix!="" & key.StartsWith(prefix)) || // non-blank prefix
					rti.HasMultiplePrefixes) // single table that stands by itself
					addCount++;
			}

			if (addCount == inputKeys.Count) return inputKeys; // no change

			List<string> keys = new List<string>(); // need to build subset of keys
			foreach (string key in inputKeys)
			{
				if ((prefix=="" && key.Length>0 && Char.IsDigit(key[0])) || // number with no prefix
					(prefix!="" & key.StartsWith(prefix)) || // non-blank prefix
					rti.HasMultiplePrefixes) // single table that stands by itself
					keys.Add(key);
			}

			return keys;
		}

		/// <summary>
		/// Convert the specified object to the corresponding MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static CompoundId ConvertTo(
			object o)
		{
			if (o is CompoundId) return (CompoundId)o;
			else if (NullValue.IsNull(o)) return new CompoundId();
			else if (o is string) return new CompoundId((string)o);

			throw new InvalidCastException(o.GetType().ToString());
		}

		/// <summary>
		/// Get hash code for this MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			if (Lex.IsUndefined(Value)) return 0;
			else return Value.GetHashCode();
		}

		/// <summary>
		/// Compare this MobiusDataType to another for equality 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public override bool Equals(object obj)
		{
			return (CompareTo(obj) == 0);
		}

		/// <summary>
		/// Compare two values (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public override int CompareTo(
			object o)
		{
			CompoundId s = this;

			if (o == null) return 1;

			else if (o is CompoundId)
			{
				CompoundId s2 = o as CompoundId;

				if (s.IsNull && s2.IsNull) return 0;
				else if (!s.IsNull && s2.IsNull) return 1;
				else if (s.IsNull && !s2.IsNull) return -1;
				else return s.Value.CompareTo(s2.Value);
			}

			else return s.Value.CompareTo(o.ToString()); // convert object to string & compare to Cid

			//throw new Exception("Can't compare a " + GetType().Name + " to a " + o.GetType());
		}

		/// <summary>
		/// Return true if null value
		/// </summary>

		[XmlIgnore]
		public override bool IsNull
		{
			get
			{
				if (Value == null || Value.Trim() == "")
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Get/set numeric value
		/// </summary>

		[XmlIgnore]
		public override double NumericValue
		{
			get
			{
				int numVal;

				if (Lex.IsNullOrEmpty(Value)) return NullValue.NullNumber;
				if (int.TryParse(Value, out numVal)) return numVal;
				else return NullValue.NullNumber;
			}

			set
			{
				throw new Exception("Can't set CompoundId to numeric value");
			}
		}


		/// <summary>
		/// Convert the value to the nearest primitive type
		/// </summary>
		/// <returns></returns>

		public override object ToPrimitiveType()
		{
			return Value;
		}

		/// <summary>
		/// Return the internal normalized string version of the MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override string FormatForCriteria()
		{
			return Value;
		}

		/// <summary>
		/// Return a formatted string for the MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override string FormatCriteriaForDisplay()
		{
			return Format(Value);
		}

		/// <summary>
		/// Custom compact serialization
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<C>");
			StringBuilder sb = BeginSerialize("C");
			sb.Append(NormalizeForSerialize(Value));
			sb.Append(">");
			return sb;
		}

		/// <summary>
		/// Custom Compact deserialization
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="sai"></param>
		/// <returns></returns>

		public static CompoundId Deserialize(string[] sa, int sai)
		{
			CompoundId cid = new CompoundId();
			cid.Value = sa[sai];
			return cid;
		}

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.CompoundId);
			base.SerializeBinary(bw);
			bw.Write(Lex.S(Value));
		}

		/// <summary>
		/// Binary Deserialize
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>

		public static CompoundId DeserializeBinary(BinaryReader br)
		{
			CompoundId sx = new CompoundId();
			MobiusDataType.DeserializeBinary(br, sx);
			sx.Value = br.ReadString();
			return sx;
		}

		/// <summary>
		/// Clone 
		/// </summary>
		/// <returns></returns>

		public new CompoundId Clone()
		{
			CompoundId cid = (CompoundId)this.MemberwiseClone();
			if (cid.MdtExAttr != null) cid.MdtExAttr = MdtExAttr.Clone();
			return cid;
		}

/// <summary>
/// Simply return value when converting to string
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			//if (!String.IsNullOrEmpty(FormattedText))
			//  return FormattedText;
			return Value; // just return value since it will always be the same whether the data is formatted or not
		}

	}

}
