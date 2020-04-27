using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Data;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Diagnostics;

namespace Mobius.UAL
{
	/// <summary>
	/// Database subviews used to restrict access to less than the full database
	/// </summary>

	public class RestrictedDatabaseView
	{
		public string Name = "";
		public HashSet<string> Userids = null;
		public HashSet<string> MetaTables = null;
		public HashSet<int> CorpIds = null;

		public static Dictionary<string, RestrictedDatabaseView> ViewDict = null; // dictionary of all subviews

		static int RestrictedAccessAttempts = 0;
		static string RejectedMetaTables = "";

		/// <summary>
		/// Get named restricted view if it exists
		/// </summary>
		/// <param name="viewName"></param>
		/// <returns></returns>

		public static RestrictedDatabaseView GetRestrictedView(string viewName)
		{
			if (ViewDict == null) ReadViews();

			viewName = viewName.Trim().ToUpper();
			if (!RestrictedDatabaseView.ViewDict.ContainsKey(viewName)) return null;

			RestrictedDatabaseView v = RestrictedDatabaseView.ViewDict[viewName];

			if (v.MetaTables == null) ReadViewMetaTables(v);
			if (v.CorpIds == null) ReadViewCorpIds(v);
			return v;
		}

		/// <summary>
		/// Get any RestrictedDatabaseView that applies to the current user
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>

		public static RestrictedDatabaseView GetRestrictedViewForUser(string userId)
		{
			if (ViewDict == null) ReadViews();

			userId = userId.Trim().ToUpper();
			foreach (RestrictedDatabaseView v in ViewDict.Values)
			{
				if (v.Userids == null) continue; // don't consider if no users defined
				if (v.Userids.Contains(userId))
				{
					if (v.MetaTables == null) ReadViewMetaTables(v);
					if (v.CorpIds == null) ReadViewCorpIds(v);
					return v;
				}
			}

			return null;
		}

		/// <summary>
		/// Return true if user is a member of the current restricted view
		/// or if there is no restricted view in effect.
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool UserIsMemberOfCurrentView(string userName)
		{
			HashSet<string> userSet = ClientState.UserInfo?.RestrictedViewUsers;
			if (userSet == null) return true;

			userName = userName.Trim().ToUpper();
			if (userSet.Contains(userName)) return true;
			else return false;
		}

		/// <summary>
		/// Filter key list if restricted view
		/// </summary>
		/// <param name="resultKeys"></param>
		/// <returns></returns>

		public static List<string> FilterOutRestrictedKeys(
			List<string> resultKeys)
		{
			HashSet<string> allowedMetatables = ClientState.UserInfo?.RestrictedViewAllowedMetaTables;
			HashSet<int> allowedCorpIds = ClientState.UserInfo?.RestrictedViewAllowedCorpIds;

			if (allowedMetatables == null && allowedCorpIds == null) return resultKeys; // return as is if no limits

			if (allowedCorpIds == null) return new List<string>(); // return nothing if metatable limit but no CorpId limit (for now)

			return FilterOutRestrictedKeys(resultKeys, allowedCorpIds);
		}


		/// <summary>
		/// Filter a key list by the supplied list of allowed CorpIds
		/// </summary>
		/// <param name="resultKeys"></param>
		/// <param name="allowedCorpIds"></param>
		/// <returns></returns>

		static List<string> FilterOutRestrictedKeys(
			List<string> resultKeys,
			HashSet<int> allowedCorpIds)
		{
			if (allowedCorpIds == null) return resultKeys; // no filtering

			List<string> resultKeys2 = new List<string>();

			foreach (string key in resultKeys)
			{
				if (KeyIsNotRetricted(key, allowedCorpIds))
					resultKeys2.Add(key);
			}

			return resultKeys2;
		}

		/// <summary>
		/// Return true is key IS restricted in the current view
		/// 
		/// </summary>
		/// <param name="corpIdString"></param>
		/// <returns></returns>

		public static bool KeyIsRetricted(
			string corpIdString)
		{
			return !KeyIsNotRetricted(corpIdString);
		}

		/// <summary>
		/// Return true is key IS NOT restricted in the current view
		/// </summary>
		/// <param name="corpIdString"></param>
		/// <returns></returns>

		public static bool KeyIsNotRetricted(
			string corpIdString)
		{
			HashSet<string> allowedMetatables = ClientState.UserInfo?.RestrictedViewAllowedMetaTables;
			HashSet<int> allowedCorpIds = ClientState.UserInfo?.RestrictedViewAllowedCorpIds;

			if (allowedMetatables == null && allowedCorpIds == null) return true; // ok if no limits

			if (allowedCorpIds == null) return false; // return false if metatable limit but no CorpId limit (for now)

			return KeyIsNotRetricted(corpIdString, ClientState.UserInfo?.RestrictedViewAllowedCorpIds);
		}

		/// <summary>
		/// Filter a key list by the supplied list of allowed CorpIds
		/// </summary>
		/// <param name="resultKeys"></param>
		/// <param name="allowedCorpIds"></param>
		/// <returns></returns>

		static bool KeyIsNotRetricted(
			string corpIdString,
			HashSet<int> allowedCorpIds)
		{
			int corpId;

			if (allowedCorpIds == null) return true; // no filtering

			if (!int.TryParse(corpIdString, out corpId)) return false;

			if (allowedCorpIds.Contains(corpId)) return true;
			else return false;
		}

		/// <summary>
		/// Do initial read of the list of views and the users for each view.
		/// </summary>
		public static void ReadViews()
		{
			StreamReader sr;

			ViewDict = new Dictionary<string, RestrictedDatabaseView>();

			bool restrictionsActive = ServicesIniFile.ReadBool("UseRestrictedDatabaseViews", false);
			if (!restrictionsActive) return;

			string dirName = ServicesDirs.MetaDataDir + @"\RestrictedDatabaseViews";
			if (!Directory.Exists(dirName)) return; // throw new Exception("RestrictedDatabaseViews Directory does not exist: " + dirName);
			string fileName = dirName + @"\ViewList.txt";
			if (!File.Exists(fileName)) return;
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
				if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

				string viewName = txt.Trim().ToUpper();

				RestrictedDatabaseView v = new RestrictedDatabaseView();
				ViewDict[viewName] = v;
				v.Name = viewName;
				ReadViewUsers(v);
			}

			sr.Close();
		}

		public static void ReadViewUsers(RestrictedDatabaseView v)
		{
			StreamReader sr;

			v.Userids = new HashSet<string>();

			string dirName = ServicesDirs.MetaDataDir + @"\RestrictedDatabaseViews";
			string fileName = dirName + @"\" + v.Name + "Users.txt";
			if (!File.Exists(fileName)) throw new Exception("Missing file: " + fileName);
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
				if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

				string userName = txt.Trim().ToUpper();
				v.Userids.Add(userName);
			}

			sr.Close();

			if (v.Userids.Contains("<ALL>") && v.Userids.Count == 1)
				v.Userids = null; // special keyword to include all users

			return;
		}

		public static void ReadViewMetaTables(RestrictedDatabaseView v)
		{
			StreamReader sr;
			string tableNamePrefix;
			int assay;
			bool isSummary;

			v.MetaTables = new HashSet<string>();

			string dirName = ServicesDirs.MetaDataDir + @"\RestrictedDatabaseViews";
			string fileName = dirName + @"\" + v.Name + "MetaTables.txt";
			if (!File.Exists(fileName)) throw new Exception("Missing file: " + fileName);
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
				if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

				string mtName = txt.Trim().ToUpper();
				v.MetaTables.Add(mtName);

				MetaTable.ParseMetaTableName(mtName, out tableNamePrefix, out assay, out isSummary);
				if (assay <= 0) continue;
				if (isSummary) continue;

				if (Lex.Eq(tableNamePrefix, "ASSAY"))
				{
					string mtName2 = mtName + "_SUMMARY";
					v.MetaTables.Add(mtName2);
				}
			}

			sr.Close();

			if (v.MetaTables.Contains("<ALL>") && v.MetaTables.Count == 1)
				v.MetaTables = null; // special keyword to include all metatables

			return;
		}

		public static void ReadViewCorpIds(RestrictedDatabaseView v)
		{
			StreamReader sr;
			int corpId;

			v.CorpIds = new HashSet<int>();

			string dirName = ServicesDirs.MetaDataDir + @"\RestrictedDatabaseViews";
			string fileName = dirName + @"\" + v.Name + "CorpIds.txt";
			if (!File.Exists(fileName)) throw new Exception("Missing file: " + fileName);
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
				if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

				string corpIdTxt = txt.Trim().ToUpper();
				if (!int.TryParse(corpIdTxt, out corpId)) continue;
				v.CorpIds.Add(corpId);
			}

			sr.Close();

			if (v.CorpIds.Contains(-1) && v.CorpIds.Count == 1)
				v.CorpIds = null; // special keyword to include all CorpIds

			return;
		}

	}
}
