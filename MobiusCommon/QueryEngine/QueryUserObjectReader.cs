using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Diagnostics;

namespace Mobius.QueryEngineLibrary
{

/// <summary>
/// Query UserObject Operations
/// </summary>

	public class QueryUserObjectReader
	{

		/// <summary>
		/// Read a query & return with metatables embedded in the query given the object id
		/// </summary>
		/// <param name="objectId">id of item to read</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadQueryWithMetaTables(int objectId)
		{
			UserObject uo = UserObjectDao.Read(objectId);
			if (uo == null) return null;

			Query q = Query.Deserialize(uo.Content);
			uo.Content = q.Serialize(true); // serialize with metatables
			return uo;
		}

		/// <summary>
		/// Read a query & return with metatables embedded in the query for UserObject
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadQueryWithMetaTables(UserObject uo)
		{
			Query q, q2;

			uo = UserObjectDao.Read(uo);
			if (uo == null) return null;

			// Sharing a query implicitly shares any underlying annotations, calc fields & lists.
			// Mark these objects as readable within the context of this query even if they are not shared.

			Permissions.AllowTemporaryPublicReadAccessToAllUserObjects = true;
			q = Query.Deserialize(uo.Content);

			foreach (QueryTable qt in q.Tables)
			{ // mark any saved lists in criteria as temporarily readable since the query is readable
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (qc.MetaColumn.DataType == MetaColumnType.CompoundId &&
						Lex.Contains(qc.Criteria, "IN LIST"))
					{
						int id;
						string criteria = qc.MetaColumn.Name + " " + qc.Criteria;
						ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(criteria);
						if (psc == null) continue;

						psc.Value = Lex.RemoveAllQuotes(psc.Value);
						if (Lex.IsUndefined(psc.Value)) continue;

						if (UserObject.TryParseObjectIdFromInternalName(psc.Value, out id) && id > 0)
							Permissions.TemporaryPublicReadAccessUserObjects[id] = null;
					}
				}
			}

			Permissions.AllowTemporaryPublicReadAccessToAllUserObjects = false;

			uo.Content = q.Serialize(true); // serialize with metatables
			return uo;
		}

/// <summary>
/// Get the MQL for a saved query
/// </summary>
/// <param name="queryId"></param>
/// <returns></returns>
/// 
		public static string GetSavedQueryMQL(
			int queryId, bool mobileQuery = false)
		{
			UserObject uo = UserObjectDao.Read(queryId);
			if (uo == null) return null;
			Query q = Query.Deserialize(uo.Content);
			string mql = MqlUtil.ConvertQueryToMql(q,mobileQuery); // to mql
			return mql;
		}

		/// <summary>
		/// Lookup a query by matching the supplied substring against the query name
		/// </summary>
		/// <param name="querySubString"></param>
		/// <param name="msg"></param>
		/// <returns></returns>

		public static int FindQueryByNameSubstring(string querySubString, out string msg)
		{
			string objNm, ownrId, fldrNm;
			int objId = -1, exactThisUser = -1, exactOtherUser = -1;

			string sql =
				"select obj_id, obj_nm, ownr_id, fldr_nm " +
				"from mbs_owner.mbs_usr_obj " +
				"where obj_typ_id = " + (int)UserObjectType.Query + " and lower (obj_nm) like lower('%" + querySubString + "%')";
			DbCommandMx dao = new DbCommandMx();
			dao.Prepare(sql);
			dao.ExecuteReader();
			msg = "";
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			int rowCnt = 0;

			while (dao.Read())
			{
				objId = dao.GetInt(0);
				objNm = dao.GetString(1);
				ownrId = dao.GetString(2);
				fldrNm = dao.GetString(3);
				sb.AppendLine(objId + ": " + objNm + "." + ownrId + "." + fldrNm);

				if (Lex.Eq(objNm, querySubString)) // exact match?
				{
					if (Lex.Eq(ownrId, Security.UserName))
					{
						if (exactThisUser == -1) exactThisUser = objId;
						else exactThisUser = -2;
					}

					else
					{
						if (exactOtherUser == -1) exactOtherUser = objId;
						else exactOtherUser = -2;
					}
				}

				rowCnt++;
			}
			dao.Dispose();

			if (rowCnt == 1) return objId; // if just one hit return its id
			else if (exactThisUser > 0) return exactThisUser;
			else if (exactThisUser == -1 && exactOtherUser > 0) return exactOtherUser;
			else
			{
				if (rowCnt == 0) msg = "No queries found";
				else if (rowCnt > 1) msg = "Multiple matches:\n\n" + sb.ToString();

				return -1;
			}
		}

	}
}
