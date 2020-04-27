using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace Mobius.QueryEngineLibrary
{
	public class MoleculeUtil : IMoleculeMxUtil
	{
		public static bool SaltGroupingEnabled = true; // global flag for salt grouping enabled
		public static bool ShowStereoComments = false; // insert/remove any stereo comments (obsolete)
		public static bool EnhanceStructureDisplay = true; // do other structure display enhancements
		public static bool ScaleBondWidth = true; // scale down bond width for small structs

		public static object SupercedeableSearchLock = new object();
		public static int SupercedeableSearchCounter = 0; // Counter used to assign an Id to each supercedeable search

		public static string temp = "";

		// public static string CTabToCharExpression = // expression to convert a ctab into a Oracle char type. Returns 'Large Structure' if too big & handle at retrieve time
		//  "case when length(chime(ctab)) <= 4000 then to_char(chime(ctab)) else 'Large Structure' end";
		// public static string LargeStructureString = "Large Structure"; // string that indicates that chime for structure is > 4000 chars 
		// Examples: 3203030, 3020939, 3020943, 3046095, 2240877, 2312187, 2836746	

		/// <summary>
		/// Convert an objectinto a ChemicalStructure object
		/// </summary>
		/// <param name="fieldValue"></param>
		/// <returns></returns>

		public static MoleculeMx ConvertObjectToChemicalStructure(
			object fieldValue)
		{
			MoleculeMx cs;
			MoleculeFormat structureFormat;
			string txt;

			if (fieldValue == null) cs = null;

			else if (fieldValue is MoleculeMx)
				cs = (MoleculeMx)fieldValue;

			else if (fieldValue is string || fieldValue is CompoundId || fieldValue is QualifiedNumber)
			{
				if (fieldValue is string)
					txt = (string)fieldValue;

				else if (fieldValue is CompoundId)
					txt = ((CompoundId)fieldValue).Value;

				else txt = ((QualifiedNumber)fieldValue).TextValue;

				structureFormat = MoleculeMx.GetMoleculeFormatType(txt);
				if (structureFormat != MoleculeFormat.Unknown) // just convert is known format
					cs = MoleculeMx.MolStringToMoleculeMx(txt);

				else // see if unqualified compound id
					cs = MoleculeUtil.SelectMoleculeForCid(txt);
			}

			else throw new Exception("Object is not a recognized structure format");

			return cs;
		}


		/// <summary>
		/// Select a ChemicalStructure object for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public MoleculeMx SelectMoleculeFromCid( // required IChemicalStructureUtil interface method definition
			string cid,
			MetaTable mt = null)
		{ // required interface method definition
			return SelectMoleculeForCid(cid, mt);
		}

		/// <summary>
		/// Select a Molecule object for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static MoleculeMx SelectMoleculeForCid(
			string cid,
			MetaTable mt = null)
		{
			MetaColumn strMc = null;
			string mtName = null, keyColName, strColExpr, chimeString;
			MoleculeMx cs;

			if (cid == null || cid == "") return null;

			if (RestrictedDatabaseView.KeyIsRetricted(cid)) return null;

			//if (mt != null) mtName = mt.Name; // debug

			bool isUcdb = (mt != null && mt.Root.IsUserDatabaseStructureTable); // user compound database

			if (isUcdb) // if root metatable is user database then normalize based on key
			{
				mt = mt.Root; // be sure we have root
				cid = CompoundId.Normalize(cid, mt);
			}

			else
			{
				cid = CompoundId.Normalize(cid, mt);
				cs = MoleculeCache.Get(cid); // see if in cache
				if (cs != null) return cs;

				mt = CompoundId.GetRootMetaTableFromCid(cid, mt);
				if (mt != null && Lex.Eq(mt.Name, "corp_structure") && MetaTableCollection.Get("corp_structure2") != null)
					mt = MetaTableCollection.Get("corp_structure2"); // hack to search both small & large mols for Corp database
			}

			if (mt == null) return null;

			strMc = mt.FirstStructureMetaColumn; //.getmt.GetMetaColumnByName("molstructure");
			if (strMc == null) return null;

			cid = CompoundId.NormalizeForDatabase(cid, mt);
			if (String.IsNullOrEmpty(cid)) return null;

			// Call external method to select structure

			if (strMc.ColumnMap.StartsWith("InternalMethod", StringComparison.OrdinalIgnoreCase) || 
			 strMc.ColumnMap.StartsWith("PluginMethod", StringComparison.OrdinalIgnoreCase))
			{ // call external method to get structure
				List<MetaColumn> selectList = new List<MetaColumn>();
				selectList.Add(mt.KeyMetaColumn);
				selectList.Add(strMc);
				object[] vo = new object[2];
				vo[0] = cid;
				try { GenericMetaBroker.CallLateBindMethodToFillVo(vo, 1, mt, selectList); }
				catch (Exception ex)
				{ return null; }

				if (vo[1] is MoleculeMx)
					cs = (MoleculeMx)vo[1];
				else if (vo[1] is string)
					cs = MoleculeMx.MolStringToMoleculeMx((string)vo[1]);
				else cs = null;

				if (!isUcdb) MoleculeCache.AddMolecule(cid, cs);

				return cs;
			}

			// Normal case

			//if (HelmEnabled) // temp till server back
			//{
			//	cs = new MoleculeMx();
			//	MoleculeMx.SetMoleculeToTestHelmString(cid, cs);
			//	return cs;
			//}

			string tableMap = mt.GetTableMapWithAliasAppendedIfNeeded(); // some SQL (e.g. Postgres) requires an alias for subqueries)

			strColExpr = strMc.ColumnMap;
			if (strColExpr == "") strColExpr = strMc.Name;

			if (MqlUtil.IsCartridgeMetaTable(mt)) // selecting from Direct cartridge
			{
				if (!Lex.Contains(tableMap, "chime(")) // if no chime expression
					strColExpr = "chime(ctab)"; // then create one (gets clob)

				strColExpr += ", chime(ctab)"; // add 2nd column that gets clob in case first just gets first 4000 characters
			}

			keyColName = mt.KeyMetaColumn.ColumnMap;
			if (keyColName == "") keyColName = mt.KeyMetaColumn.Name;

			DbType parmType = DbType.String;
			object cidObj = cid;

			if (mt.KeyMetaColumn.IsNumeric)
			{
				if (!Lex.IsInteger(cid)) return null; // if numeric col be sure cid is numeric also
				parmType = DbType.Int64;
				cidObj = Int64.Parse(cid);
			}

			string sql =
				"select " + strColExpr + " " +
				"from " + tableMap + " " +
				"where " + keyColName + " = :0";

			DbCommandMx drd = new DbCommandMx();
			try
			{
				drd.PrepareParameterized(sql, parmType);
				drd.ExecuteReader(cidObj);

				if (!drd.Read() || drd.Rdr.IsDBNull(0))
				{
					drd.Dispose();
					return null;
				}

				string molString = drd.GetString(0);
				drd.Dispose();

				MoleculeMx.TrySetStructureFormatPrefix(ref molString, strMc.DataTransform); // add molString type if indicated by data transform

				cs = MoleculeMx.MolStringToMoleculeMx(molString);
				cs.StoreKeyValueInMolComments(strMc, cid);

				if (!isUcdb) MoleculeCache.AddMolecule(cid, cs);

				//if (MoleculeMx.HelmEnabled == true && Lex.IsInteger(cid))
				//	MoleculeMx.SetMoleculeToTestHelmString(cid, cs);

				return cs;
			}

			catch (Exception ex)
			{ // just log message & return;
				DebugLog.Message("SelectMoleculeForCid Exception, Cid: " + cid + ", table: " + mt.Name + "\n" +
					"sql: " + OracleDao.FormatSql(sql) + "\n" + DebugLog.FormatExceptionMessage(ex));

				if (drd != null) drd.Dispose();
				return null;
			}

		}

		/// <summary>
		/// Select a list of chime strings ChemicalStructure object for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt">MetaTable used to get root table to select structures from</param>
		/// <returns></returns>

		public static Dictionary<string, MoleculeMx> SelectMoleculesForCidList(
			List<string> cidList,
			MetaTable mt = null)
		{
			MetaColumn strMc = null;
			MoleculeMx cs;
			KeyValuePair<string, string> kvp;
			string mtName = null, keyColName, strColExpr, chimeString, cid;
			int li;

			if (cidList == null || cidList.Count == 0) return null;

			cid = cidList[0];

			mt = mt?.Root;
			bool isUcdb = (mt != null && mt.Root.IsUserDatabaseStructureTable); // user compound database

			if (!isUcdb)
			{
				cid = CompoundId.Normalize(cid, mt);

				mt = CompoundId.GetRootMetaTableFromCid(cid, mt);

				if (mt == null) return null;

				if (MqlUtil.IsCartridgeMetaTable(mt))
				{
					return SelectChemicalStructuresForCorpIdList(cidList, mt);
				}
			}

// Do one at a time

			Dictionary<string, MoleculeMx> csDict = new Dictionary<string, MoleculeMx>();

			foreach (string cid0 in cidList)
			{
				cs = SelectMoleculeForCid(cid0, mt);
				if (cs != null) csDict[cid0] = cs;
			}

			return csDict;
		}


		/// <summary>
		/// Select structures for a list of compound ids using a single SQL cursor.
		/// </summary>
		/// <param name="notInCache"></param>
		/// <param name="mt"></param>
		/// <param name="strMc"></param>

		static Dictionary<string, MoleculeMx> SelectChemicalStructuresForCorpIdList(
			List<string> cidList,
			MetaTable mt)
		{
			List<string> notInCacheKeyList;
			Dictionary<Int64, string> intKeyToStringKey;
			DbType parmType;
			MoleculeMx cs;
			Int64 intCid;
			string cid;
			int li;

			Stopwatch sw = Stopwatch.StartNew();

			Dictionary<string, MoleculeMx> csDict = new Dictionary<string, MoleculeMx>();
			List<string> notInCacheList = new List<string>();

			// Get structures already in the cache

			for (li = 0; li < cidList.Count; li++)
			{
				cid = cidList[li];

				if (RestrictedDatabaseView.KeyIsRetricted(cid)) continue;

				cid = CompoundId.Normalize(cid, mt);
				if (!Int64.TryParse(cid, out intCid)) continue; // make sure an integer

				if (MoleculeCache.Contains(cid)) // see if in cache
				{
					csDict[cid] = MoleculeCache.Get(cid);
					continue;
				}

				else notInCacheList.Add(cid);
			}

			// Retrieve structures from the database for those not in cache

			if (notInCacheList.Count == 0) return csDict;

			MetaColumn strMc = mt.GetMetaColumnByName("molstructure");
			if (strMc == null) return null;

			string tableMap = mt.GetTableMapWithAliasAppendedIfNeeded(); // some SQL (e.g. Postgres) requires an alias for subqueries)

			string keyColName = mt.KeyMetaColumn.ColumnMap;
			if (Lex.IsUndefined(keyColName)) keyColName = mt.KeyMetaColumn.Name;

			string strColExpr = strMc.ColumnMap;
			if (strColExpr == "") strColExpr = strMc.Name;

			if (MqlUtil.IsCartridgeMetaTable(mt)) // selecting from Direct cartridge
			{
				if (!Lex.Contains(tableMap, "chime(")) // if no chime expression
					strColExpr = "chime(ctab)"; // then create one (gets clob)

				strColExpr += ", chime(ctab)"; // add 2nd column that gets clob in case first just gets first 4000 characters
			}

			string sql =
				"select " + keyColName + ", " + strColExpr + " " +
				"from " + tableMap + " " +
				"where " + keyColName + " in (<list>)";

			DbCommandMx drd = new DbCommandMx();

			bool isNumericKey = (mt.KeyMetaColumn.IsNumeric);
			bool isStringKey = !isNumericKey;

			if (isStringKey) parmType = DbType.String;
			else parmType = DbType.Int64;

			try
			{
				drd.PrepareListReader(sql, parmType);
				drd.ExecuteListReader(notInCacheList);
				while (drd.Read())
				{
					if (drd.Rdr.IsDBNull(0)) continue;

					if (isNumericKey)
					{
						intCid = drd.GetLong(0);
						cid = intCid.ToString();
					}

					else // string cid
					{
						cid = drd.GetString(0);
					}

					cid = CompoundId.Normalize(cid, mt);

					string molString = drd.GetString(1);

					cs = new MoleculeMx(MoleculeFormat.Chime, molString);
					csDict[cid] = cs;

					MoleculeCache.AddMolecule(cid, cs);
				}

				drd.Dispose();
				int msTime = (int)sw.ElapsedMilliseconds;
				return csDict;
			}

			catch (Exception ex)
			{
				if (drd != null) drd.Dispose();

				throw new Exception(
					"SelectStructuresForCorpIdList, table: " + mt.Name + "\n" +
					"sql: " + OracleDao.FormatSql(sql) + "\n");
			}
		}

			/// <summary>
			/// Lookup all of the salt forms for a compound
			/// </summary>
			/// <param name="cn"></param>
			/// <returns></returns>

			public static List<string> GetAllSaltForms(
			string cn)
		{
			if (cn == null || cn == "") return null;
			List<string> list = new List<string>();
			list.Add(cn);
			Dictionary<string, List<string>> cidDict = GetAllSaltForms(list);
			if (cidDict.ContainsKey(cn)) return (List<string>)cidDict[cn];
			else return null;
		}

		/// <summary>
		/// Get list of compounds whose fragments match those of the compounds in the list.
		/// </summary>
		/// <param name="cnList"></param>
		/// <returns></returns>

		public static Dictionary<string, List<string>> GetAllSaltForms(
			List<string> cnList)
		{
			int t0, t1;

			t0 = TimeOfDay.Milliseconds();

			Dictionary<string, List<string>> cidDict = new Dictionary<string, List<string>>();

			List<string> cnList2 = new List<string>();
			foreach (string s in cnList)
			{ // get just the list entries that are integers (e.g. remove MFCD numbers)
				if (Lex.IsInteger(s)) cnList2.Add(s);
			}
			t1 = TimeOfDay.Milliseconds() - t0;
			if (cnList2.Count == 0) return cidDict;

			//MetaTable mt = MetaTableCollection.Get("frag_occurrence");
			MetaTable mt = MetaTableCollection.Get("CorpId_salt_isomer_info");
			if (mt == null) return cidDict;
			string sql = mt.TableMap; // get sql to use from metatable
			if (sql.StartsWith("(")) sql = sql.Substring(1, sql.Length - 2); // remove surround parens if necessary
			sql = Lex.Replace(sql, "where", "where CorpId in (<list>) and "); // add criteria needed to do list search

			DbCommandMx drd = new DbCommandMx();
			try
			{
				drd.PrepareListReader(sql, DbType.Int32);
				drd.ExecuteListReader(cnList2);

				if (drd.Cancelled)
				{
					drd.Dispose();
					return null;
				}

				while (true)
				{
					if (!drd.ListRead()) break;
					string cn = CompoundId.Normalize(drd.GetInt(0).ToString());
					string cn2 = CompoundId.Normalize(drd.GetInt(1).ToString());

					if (RestrictedDatabaseView.KeyIsRetricted(cn)) continue;
					if (RestrictedDatabaseView.KeyIsRetricted(cn2)) continue;

					if (!cidDict.ContainsKey(cn))
						cidDict[cn] = new List<string>();
					List<string> al = cidDict[cn];
					if (al.Count == 0 || al[al.Count - 1] != cn2) // add if not dup
						al.Add(cn2);
				}

				drd.Dispose();
			}

			catch (Exception ex)
			{ // catch case non-numeric item in list, single-row subquery returns more than one row, etc.
				drd.Dispose();
				return new Dictionary<string, List<string>>();
			}

			t1 = TimeOfDay.Milliseconds() - t0;
			return cidDict;
		}

		/// <summary>
		/// (OLD VERSION)
		/// Get list of compounds whose fragments match those of the compounds in the list.
		/// </summary>
		/// <param name="cnList"></param>
		/// <returns></returns>

		public static Dictionary<string, List<string>> GetAllSaltFormsNew(
			List<string> cnList)
		{
			int t0, t1;

			t0 = TimeOfDay.Milliseconds();

			Dictionary<string, List<string>> cidDict = new Dictionary<string, List<string>>();

			List<string> cnList2 = new List<string>();
			foreach (string s in cnList)
			{ // get just the list entries that are integers (e.g. remove MFCD numbers)
				if (Lex.IsInteger(s)) cnList2.Add(s);
			}
			t1 = TimeOfDay.Milliseconds() - t0;
			if (cnList2.Count == 0) return cidDict;

			//MetaTable mt = MetaTableCollection.Get("frag_occurrence");
			MetaTable mt = MetaTableCollection.Get("CorpId_salt_isomer_info");
			if (mt == null) return cidDict;
			string sql = mt.TableMap; // get sql to use from metatable
			if (sql.StartsWith("(")) sql = sql.Substring(1, sql.Length - 2); // remove surround parens if necessary
			sql = Lex.Replace(sql, "where", "where CorpId in (<list>) and "); // add criteria needed to do list search

			DbCommandMx drd = new DbCommandMx();
			try
			{
				drd.PrepareListReader(sql, DbType.Int32);
				drd.ExecuteListReader(cnList2);

				if (drd.Cancelled)
				{
					drd.Dispose();
					return null;
				}

				while (true)
				{
					if (!drd.ListRead()) break;
					string cn = CompoundId.Normalize(drd.GetInt(0).ToString());
					string cn2 = CompoundId.Normalize(drd.GetInt(1).ToString());
					if (!cidDict.ContainsKey(cn))
						cidDict[cn] = new List<string>();
					List<string> al = cidDict[cn];
					if (al.Count == 0 || al[al.Count - 1] != cn2) // add if not dup
						al.Add(cn2);
				}

				drd.Dispose();
			}

			catch (Exception ex)
			{ // catch case non-numeric item in list, single-row subquery returns more than one row, etc.
				drd.Dispose();
				return new Dictionary<string, List<string>>();
			}

			t1 = TimeOfDay.Milliseconds() - t0;
			return cidDict;
		}

		/// <summary>
		/// Insert salts into list, grouping together
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>

		public static List<string> InsertSalts(
			List<string> list)
		{
			Dictionary<string, List<string>> saltHash = GetAllSaltForms(list);
			List<string> newList = new List<string>();
			Dictionary<string, List<string>> newListHash = new Dictionary<string, List<string>>();

			foreach (string s in list)
			{
				if (s == null || s == "") continue;
				if (newListHash.ContainsKey(s)) continue;
				newList.Add(s);
				newListHash[s] = null;

				if (!saltHash.ContainsKey(s)) continue;
				foreach (string salt in saltHash[s])
				{
					if (newListHash.ContainsKey(salt)) continue;
					newList.Add(salt);
					newListHash[salt] = null;
				}
			}

			return newList;
		}

		/// <summary>
		/// Group together numbers with same parent structure
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>

		public static List<string> GroupSalts(
			List<string> list)
		{
			if (!SaltGroupingEnabled) return list;

			Dictionary<string, List<string>> saltHash = GetAllSaltForms(list);
			Dictionary<string, object> listHash = new Dictionary<string, object>();
			foreach (string s in list) listHash[s] = null;
			List<string> newList = new List<string>(); // build list here to check for dups
			Dictionary<string, object> newListHash = new Dictionary<string, object>();

			foreach (string s in list)
			{
				if (s == null || s == "") continue;
				if (newListHash.ContainsKey(s)) continue;
				newList.Add(s);
				newListHash[s] = null;

				if (!saltHash.ContainsKey(s)) continue;
				foreach (string salt in saltHash[s])
				{
					if (!listHash.ContainsKey(salt)) continue; // must be in original list
					if (newListHash.ContainsKey(salt)) continue; // don't add dups
					newList.Add(salt);
					newListHash[salt] = null;
				}
			}
			return newList;
		}

		/// <summary>
		/// Get list of related structure compound Ids for a compound id
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mtName"></param>
		/// <param name="chime"></param>
		/// <returns></returns>
		public static string GetRelatedMatchCounts(
			string cid,
			string mtName,
			string chime,
			StructureSearchType searchTypes,
			int searchId)
		{
			string result = RelatedStructureSearch.GetRelatedMatchCounts(cid, mtName, chime, searchTypes, searchId);
			return result;
		}

		/// <summary>
		/// Get list of related structures for compound id
		/// </summary>
		/// <param name="queryCid"></param>
		/// <param name="mtName"></param>
		/// <param name="chime"></param>
		/// <returns></returns>
		public static string GetRelatedMatchRowsSerialized(
			string queryCid,
			string mtName)
		{
			string result = RelatedStructureSearch.GetRelatedMatchRowsSerialized(queryCid, mtName);
			return result;
		}

		/// <summary>
		/// Execute a SmallWorld preview query with serialized parameters and results
		/// </summary>
		/// <param name="serializedQuery"></param>
		/// <returns>Object array with QE in first entry and serialized rows in second</returns>

		public static object[] ExecuteSmallWorldPreviewQuerySerialized(
			string serializedQuery)
		{
			QueryEngine qe;
			List<object[]> rows;

			Query q = Data.Query.Deserialize(serializedQuery);
			ExecuteSmallWorldPreviewQuery(q, out qe, out rows);
			if (rows == null) return null;

			byte[] ba = VoArray.SerializeBinaryVoArrayListToByteArray(rows);

			object[] oa = new object[2];
			oa[0] = qe.Id;
			oa[1] = ba;
			return oa;
		}

		/// <summary>
		/// Execute a SmallWorld preview query
		/// Note that if another preview query is received before this one is
		/// complete then this one will be cancelled and will return null.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static void ExecuteSmallWorldPreviewQuery(
			Query query,
			out QueryEngine qe,
			out List<object[]> rows)
		{
			int searchId = 0; // id for search associated with this thread
			rows = null;

			lock (SupercedeableSearchLock)
			{
				SupercedeableSearchCounter++; // assign id for the search associated with this thread
				searchId = SupercedeableSearchCounter;
			}

			qe = new QueryEngine();
			List<string> cidList = qe.ExecuteQuery(query);

			if (searchId == SupercedeableSearchCounter) // continue if not superceded
			{
				rows = qe.RetrieveAllRows();
				if (searchId != SupercedeableSearchCounter) // just return null if superceded
					rows = null;
			}
			qe.Close();

			return;
		}

		/// <summary>
		/// Extract any special Corp Stereocomments from molString (usually from Oracle)
		/// </summary>
		/// <param name="molString"></param>
		/// <returns></returns>

		public static string ExtractStereoComments(
			ref string molString)
		{
			string commentLabel = "\vSTEREO_FLGS:";
			string comment = "";

			int pos = Lex.IndexOf(molString, commentLabel);
			if (pos < 0) return "";

			comment = molString.Substring(pos + commentLabel.Length);
			molString = molString.Substring(0, pos); // remove from molString

			string[] sa = comment.Split(',');
			if (sa.Length < 9) return ""; // be sure long enough
			comment = "";
			string noStereo = sa[0];
			if (Lex.Eq(sa[1], "T")) // "abs_flg"
			{ if (comment != "") comment += ", "; comment += "Absolute"; }
			if (Lex.Eq(sa[2], "T")) // "unk_flg"
			{ if (comment != "") comment += ", "; comment += "Unknown Stereo"; }
			if (Lex.Eq(sa[2], "T")) // "e_isomer_flg"
			{ if (comment != "") comment += ", "; comment += "E Isomer"; }
			if (Lex.Eq(sa[4], "T")) // "z_isomer_flg"
			{ if (comment != "") comment += ", "; comment += "Z Isomer"; }
			if (Lex.Eq(sa[5], "T")) // "mix_flg"
			{ if (comment != "") comment += ", "; comment += "Stereo Mixture"; }
			if (Lex.Eq(sa[6], "T")) // "rac_flg"
			{ if (comment != "") comment += ", "; comment += "Racemic"; }

			string isomer = sa[7];  // isomer_number
			if (isomer != "")
			{ if (comment != "") comment += ", "; comment += "Isomer " + isomer; }

			string other = sa[8]; // other_stereo_txt
			if (other.StartsWith("[") && other.EndsWith("]"))
				other = other.Substring(1, other.Length - 2);
			if (other != "")
			{ if (comment != "") comment += ", "; comment += other; }

			return comment;
		}

		/// <summary>
		/// SetStereoChemistryComments in structure object
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="comments"></param>
		/// <param name="cid"></param>

		public static void SetStereochemistryComments(
			MoleculeMx cs,
			string comments,
			string cid)
		{
			if (Lex.IsUndefined(comments) || !ShowStereoComments) return;

			if (!ClientState.IsNativeSession) return; // don't include if not native session (i.e regular Mobius Client)

			cs.SetMolComments("CorpId=" + cid); // indicate this is an CorpId

			if (cs.PrimaryFormat == MoleculeFormat.Unknown)
			{ // be sure defined
				cs.SetPrimaryTypeAndValue(MoleculeFormat.Chime, "");
			}

			try { cs.CreateStructureCaption(comments); }
			catch (Exception ex)
			{ DebugLog.Message("CreateStructureCaption exception for compound: " + cid); } // issue #216

			return;
		}

		/// <summary>
		/// Return true if cid identifies a problem structure
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		static bool IsProblemStructure(string cid)
		{
			return false; // not implemented
		}

		public string InChIStringToMolfileString(string inchiString)
		{
			return InChIStringToMolfileStringStatic(inchiString);
		}

		/// <summary>
		/// Convert an Inchi string to a molfile (static method)
		/// </summary>
		/// <param name="inchiString"></param>
		/// <returns></returns>

		public static string InChIStringToMolfileStringStatic(string inchiString)
		{
			string smiles = "", molfile = "";

			return molfile;
		}

	}
}
