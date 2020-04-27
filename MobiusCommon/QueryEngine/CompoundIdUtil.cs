using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Utility CompoundId methods
	/// </summary>

	public class CompoundIdUtil
	{
		public String Value;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public CompoundIdUtil()
		{
		}

		/// <summary>
		/// See if a compound id exists
		/// </summary>
		/// <param name="cid"></param>
		/// <returns></returns>

		public static bool Exists(
			string cid)
		{
			return Exists(cid, null);
		}

		/// <summary>
		/// See if a compound id exists
		/// </summary>
		/// <param name="cid"></param>
		/// <param name="mt">MetaTable to check in or if null check based on prefix</param>
		/// <returns></returns>

		public static bool Exists(
			string cid,
			MetaTable mt)
		{
			DbDataReader dr = null;
			MetaTable keyMt;
			object obj;
			bool result;
			string table, prefix, suffix, cid2;
			int number;

			if (cid == null || cid == "") return false;

			if (RestrictedDatabaseView.KeyIsRetricted(cid)) return false;

			if (mt != null && mt.Root.IsUserDatabaseStructureTable) // if root metatable is user database then normalize based on key
			{
				keyMt = mt.Root; // be sure we have root
				cid = CompoundId.Normalize(cid, keyMt);
			}

			else // from non-user database, get key table based on prefix
			{
				cid = CompoundId.Normalize(cid, mt);
				keyMt = CompoundId.GetRootMetaTableFromCid(cid, mt);
				if (keyMt != null && Lex.Eq(keyMt.Name, "corp_structure") && MetaTableCollection.Get("corp_structure2") != null)
					keyMt = MetaTableCollection.Get("corp_structure2"); // hack to search both small & large mols for Corp database
			}

			if (keyMt == null) return false;

			if (cid.StartsWith("pbchm", StringComparison.OrdinalIgnoreCase))
				return true; // hack for pubchem until full set of compound ids available in Oracle

			cid = CompoundId.NormalizeForDatabase(cid, keyMt); // get form that will match database
			if (cid == null) return false;
			string keyCol = keyMt.KeyMetaColumn.ColumnMap;
			if (keyCol == "") keyCol = keyMt.KeyMetaColumn.Name;

			string sql =
				"select " + keyCol +
				" from " + keyMt.GetTableMapWithAliasAppendedIfNeeded() +
				" where " + keyCol + " = :0";

			DbCommandMx drd = new DbCommandMx();
			try
			{
				drd.PrepareMultipleParameter(sql, 1);
				dr = drd.ExecuteReader(cid);
				result = drd.Read();
			}
			catch (Exception ex)
			{
				// Don't log error since usually because of invalid format number
				//				DebugLog.Message("CompoundIdUtil.Exists - SQL Error:\n" + sql + "\n" + ex.Message);
				drd.Dispose();
				return false;
			}

			if (result == true)
			{
				obj = dr.GetValue(0);
				cid2 = CompoundId.Normalize(obj.ToString()); // need to normalize if number
				//				if (cid2 != CompoundId.Normalize(cid)) result=false;
			}

			dr.Close(); // need to close cursor
			drd.Dispose();
			return result;
		}
		
/// <summary>
/// Check that each of the numbers in the list exist in the database
/// </summary>
/// <param name="list">String form of list as entered by user</param>
/// <param name="rootTableName">Root table to check against</param>
/// <returns></returns>

		public static string ValidateList(
			string listText,
			string rootTableName)
		{
			string cn, extCn, errorMsg;
			int i1, i2;

			if (String.IsNullOrEmpty(rootTableName)) return null;
			MetaTable rootTable = MetaTableCollection.GetWithException(rootTableName);

			string[] cna = listText.Split(new Char[] { '\n', '\r', ' ' });

			int t0 = TimeOfDay.Milliseconds();

			for (i1 = 0; i1 < cna.Length; i1++)
			{
				if (i1 > 1000 || TimeOfDay.Milliseconds() - t0 > 3000) return null; // limit size & time of check

				extCn = cna[i1].Trim();
				if (extCn == "" || extCn.StartsWith("(")) continue;

				i2 = extCn.IndexOf("("); // strip trailing text in parens
				if (i2 > 0) extCn = extCn.Substring(0, i2);
				cn = CompoundId.BestMatch(extCn, rootTable);
				if (cn == "") cn = extCn; // nothing found, use original text
				if (!CompoundIdUtil.Exists(cn, rootTable)) return extCn;
			}

			return null;
		}

	}

}
