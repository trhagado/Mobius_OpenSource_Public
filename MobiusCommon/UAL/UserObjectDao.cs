using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{
	/// <summary>
	/// Read & write user objects to db (lists, queries, structs, ...)
	/// </summary>

	public class UserObjectDao
	{
		//public static IUserObjectIUD IUserObjectIUD; // callback for Inserts/Updates/Deletes in UserObjectTree (now in Mobius.ServiceFacade.UserObjectDao)

		public static int MaxTempUserObjectId;
		public static Dictionary<int, UserObject> TempUserObjects = new Dictionary<int, UserObject>(); // temporary user objects

		/// <summary>
		/// Be sure user object table is there and is readable
		/// </summary>

		public static void Ping()
		{
			int t0 = TimeOfDay.Milliseconds();
			string sql =
				"select * " +
				"from mbs_owner.mbs_usr_obj " +
				"where obj_id = -1";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();
			drd.Read();
			drd.CloseReader();
			drd.Dispose();
		}

		/// <summary>
		/// Fetch all objects of a given type regardless of who owns it or where it is located.
		/// </summary>
		/// <param name="type"></param>
		/// <returns>Set of user objects ordered by obj_id</returns>

		public static List<UserObject> ReadMultiple(
			UserObjectType type,
			bool includeContent)
		{
			UserObject uo;

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj " +
				"where obj_typ_id=:0 " +
				"order by obj_id";

			if (!includeContent) sql = sql.Replace("obj_cntnt", "null obj_cntnt");

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql, OracleDbType.Int32);
			drd.ExecuteReader((int)type);
			return FetchMultiple(drd, true, null);
		}

		/// <summary>
		/// Fetch info for the set of objects of a given object type visible to the specified user, optionally includes publicly visible objects as well.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <returns>Set of user objects ordered by folderType, parentFolder, & (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
			UserObjectType type,
			string userId,
			bool includeShared,
			bool includeContent)
		{
			UserObject uo;

			if (Lex.IsUndefined(userId)) userId = Security.UserName; // user current user if undefined

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj " +
				"where obj_typ_id=:0 " +
				" and (ownr_id=:1 "; // ok if we own 

			if (includeShared) // also get those shared for reading or writing
				sql += " or acs_lvl_id=" + ((int)UserObjectAccess.Public) +
					 " or acs_lvl_id=" + ((int)UserObjectAccess.ACL);
			sql += ")"; // close out the "or"

			//if (type == UserObjectType.Annotation) type = type; // debug

			//string dateCutoff = ServicesIniFile.Read("UserObjectUpdateDateDebugCutoff"); // debug - limit to small subset
			//if (!String.IsNullOrEmpty(dateCutoff) &&
			// type != UserObjectType.UserParameter &&
			// type != UserObjectType.Folder)
			//	sql += " and trunc(upd_dt) >= '" + dateCutoff + "' ";

			if (!includeContent) sql = sql.Replace("obj_cntnt", "null obj_cntnt");

			sql += "order by fldr_typ_id, fldr_nm, lower(obj_nm)";

			DbCommandMx drd = new DbCommandMx();

			OracleDbType[] pa = new OracleDbType[2];
			pa[0] = OracleDbType.Int32;
			pa[1] = OracleDbType.Varchar2;

			drd.Prepare(sql, pa);

			object[] p = new object[2];
			p[0] = (int)type;
			p[1] = userId.ToUpper();

			drd.ExecuteReader(p);

			List<UserObject> uoList = FetchMultiple(drd, includeShared, userId);
			UpdateUserObjectTableDict(uoList);
			return uoList;
		}

		/// <summary>
		/// Update the service-side UserObject table name list
		/// </summary>
		/// <param name="uoList"></param>

		internal static void UpdateUserObjectTableDict(
			List<UserObject> uoList)
		{
			foreach (UserObject uo in uoList)
			{
				if (!UserObject.IsMetaTableType(uo.Type)) continue; // only maintain these
				MetaTableCollection.AddUserObjectTable(uo.InternalName);
			}
		}

		/// <summary>
		/// Fetch user object info on set of objects under a specified folder.
		/// Based on what's visible to the given user, optionally including shared objects.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <returns>Set of user objects ordered by folderType, parentFolder, and (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
			string folderName,
			string userId,
			bool includeShared,
			bool includeContent)
		{
			int t0 = TimeOfDay.Milliseconds();
			List<UserObject> uos, uos2;

			uos = new List<UserObject>();
			List<string> folderNames = new List<string> { folderName };
			while (true)
			{
				uos2 = ReadMultiple(folderNames, userId, includeShared, includeContent);
				if (uos2 == null || uos2.Count == 0) break;
				folderNames.Clear();
				foreach (UserObject uo0 in uos2)
				{
					uos.Add(uo0);
					if (uo0.Type == UserObjectType.Folder)
						folderNames.Add("FOLDER_" + uo0.Id); // folder name children will use to refer to us

					else if (uo0.Type == UserObjectType.UserDatabase)
						folderNames.Add("USERDATABASE_" + uo0.Id); // name for user database folder
				}
				if (folderNames.Count == 0) break;
			}

			if (UalUtil.LogExecutionTimes)
				DebugLog.TimeMessage("ReadMultiple for folder: " + folderName + ", User Objects: " + uos.Count, t0);

			return uos;
		}

		/// <summary>
		/// Fetch all user objects whose parent folder is one of the supplied names
		/// </summary>
		/// <param name="folderNames"></param>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <param name="includeContent"></param>
		/// <returns></returns>

		public static List<UserObject> ReadMultiple(
			List<string> folderNames,
			string userId,
			bool includeShared,
			bool includeContent)
		{
			UserObject uo;

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj ";

			sql += "where fldr_nm in (";
			foreach (string fName in folderNames)
			{
				if (!sql.EndsWith("(")) sql += ",";
				sql += Lex.AddSingleQuotes(fName);
			}
			sql += ") "; // close out list

			sql += " and (ownr_id=:0 "; // ok if we own 
			if (includeShared) // also get those shared for reading or writing
				sql += " or acs_lvl_id=" + ((int)UserObjectAccess.Public) +
					 " or acs_lvl_id=" + ((int)UserObjectAccess.ACL);
			sql += ")"; // close out the "or"

			if (!includeContent) sql = sql.Replace("obj_cntnt", "null obj_cntnt");

			sql += "order by fldr_typ_id, fldr_nm, obj_nm";

			DbCommandMx drd = new DbCommandMx();

			drd.Prepare(sql, OracleDbType.Varchar2);

			drd.ExecuteReader(userId);

			List<UserObject> uos = FetchMultiple(drd, includeShared, userId);
			return uos;
		}

		/// <summary>
		/// Fetch user object info on set of objects based what's visible to the given user, optionally including shared objects.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="includeShared"></param>
		/// <returns>Set of user objects ordered by folderType, parentFolder, and (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
			String userId,
			bool includeShared,
			bool includeContent)
		{
			UserObject uo;

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj " +
				"where ownr_id=:0 "; // ok if we own 

			if (includeShared) // also get those shared for reading or writing
				sql += " or acs_lvl_id=" + ((int)UserObjectAccess.Public) +
				 " or acs_lvl_id=" + ((int)UserObjectAccess.ACL);

			if (!includeContent) sql = sql.Replace("obj_cntnt", "null obj_cntnt");

			sql += "order by fldr_typ_id, fldr_nm, obj_nm";

			DbCommandMx drd = new DbCommandMx();

			drd.Prepare(sql, OracleDbType.Varchar2);

			drd.ExecuteReader(userId);

			List<UserObject> uos = FetchMultiple(drd, includeShared, userId);
			return uos;
		}

		/// <summary>
		/// Fetch UserObject info for a list of object ids
		/// </summary>
		/// <param name="includeContent"></param>
		/// <returns>Set of user objects ordered by folderType, parentFolder, and (case insensitively) obj name</returns>

		public static List<UserObject> ReadMultiple(
			List<int> uoIds,
			bool includeContent)
		{
			UserObject uo;

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj where obj_id in (";

			for (int oi = 0; oi < uoIds.Count; oi++)
			{
				if (oi > 0) sql += ",";
				sql += uoIds[oi].ToString();
			}

			sql += ")";

			if (!includeContent) sql = sql.Replace("obj_cntnt", "null obj_cntnt");

			sql += "order by fldr_typ_id, fldr_nm, obj_nm";

			DbCommandMx drd = new DbCommandMx();

			drd.Prepare(sql);

			drd.ExecuteReader();

			List<UserObject> uos = FetchMultiple(drd, true, null);
			return uos;
		}

		/// <summary>
		/// Fetch multiple objects for open reader
		/// </summary>
		/// <param name="drd"></param>
		/// <param name="includeShared">Include shared objects that the user has read or write access to</param>
		/// <returns></returns>

		static List<UserObject> FetchMultiple(
			DbCommandMx drd,
			bool includeShared,
			string userId)
		{
			List<UserObject> uoList = new List<UserObject>();

			while (true)
			{
				UserObject uo = FetchUserObject(drd);
				if (uo == null) break;

				//if (uo.Id == 286102) uo = uo; // debug

				if (Lex.IsNullOrEmpty(userId) || // allow if no userid to check
					Lex.Eq(uo.Owner, userId) || // or the user owns it
				 (includeShared && Permissions.UserHasReadAccess(userId, uo)) || // or want shared & user has read access
				 (includeShared && Permissions.UserHasWriteAccess(userId, uo))) // or want shared and user has write access
				{
					uoList.Add(uo);
				}
			}

			drd.Dispose();
			return uoList;
		}

		/// <summary>
		/// Return true if user object exists
		/// </summary>

		public static bool Exists(
			UserObject uo)
		{
			UserObject uo2 = ReadHeader(uo);
			if (uo2 != null) return true;
			else return false;
		}

		/// <summary>
		/// Get header info for object
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static UserObject ReadHeader(
			int objectId) // id of item to read
		{
			UserObject uo = null;

			if (objectId <= MaxTempUserObjectId)
			{
				ReadTempObject(objectId, ref uo);
				if (uo != null) uo.Content = "";
				return uo;
			}

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, null obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj where obj_id=:0";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql, OracleDbType.Int32);
			drd.ExecuteReader(objectId);

			uo = FetchUserObject(drd);
			drd.Dispose();

			return uo;
		}

		/// <summary>
		/// Return the in-memory temp object
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool ReadTempObject(
			int objectId,
			ref UserObject uo)
		{
			if (TempUserObjects.ContainsKey(objectId))
			{
				uo = TempUserObjects[objectId].Clone();
				return true;
			}

			else
			{
				uo = null;
				return false;
			}
		}

		/// <summary>
		/// Return the in-memory current object of the specified type if name = CURRENT & parentFolder = "";
		/// </summary>
		/// <param name="type"></param>
		/// <param name="owner"></param>
		/// <param name="parentFolder"></param>
		/// <param name="name"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool ReadTempObject(
			UserObjectType type,
			string owner,
			string parentFolder,
			string name,
			ref UserObject uo)
		{
			foreach (UserObject uo0 in TempUserObjects.Values)
			{
				if (uo0.Type == type && Lex.Eq(uo0.Name, name))
				{
					uo = uo0.Clone();
					return true;
				}
			}

			uo = null;
			return false;
		}

		/// <summary>
		/// Fetch user object header by the type, owner, folder id, and name of the specified user object
		/// or alternatively by the object id if specified
		/// </summary>
		/// <param name="uo">User object</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadHeader(
			UserObject uo)
		{
			if (uo.Type != UserObjectType.Unknown && !String.IsNullOrEmpty(uo.Owner) &&
			 !String.IsNullOrEmpty(uo.Name)) // use path if defined (folder may be undefined)
				return ReadHeader(uo.Type, uo.Owner, uo.ParentFolder, uo.Name);

			else if (uo.Id > 0) return ReadHeader(uo.Id);

			else throw new Exception("Neither the UserObject path or Id is defined");
		}

		/// <summary>
		/// Fetch header for user object by the specified type, owner, folder id, and name
		/// </summary>
		/// <param name="type">type of object</param>
		/// <param name="owner">userid of object owner</param>
		/// <param name="parentFolder">folder that it's in</param>
		/// <param name="name">name of object</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject ReadHeader(
			UserObjectType type,
			string owner,
			string parentFolder,
			string name)
		{
			UserObject uo = null;

			if (owner == "" || name == "") return null;

			if (ReadTempObject(type, owner, parentFolder, name, ref uo))
			{
				if (uo != null) uo.Content = "";
				return uo;
			}

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, null obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj " +
				"where obj_typ_id=:0 and ownr_id=:1 and " +
				"fldr_nm=:2 and lower(obj_nm) = lower(:3)";

			DbCommandMx drd = new DbCommandMx();

			OracleDbType[] pa = new OracleDbType[4];
			pa[0] = OracleDbType.Int32;
			pa[1] = OracleDbType.Varchar2;
			pa[2] = OracleDbType.Varchar2;
			pa[3] = OracleDbType.Varchar2;

			try
			{
				drd.Prepare(sql, pa);
			}
			catch (Exception)
			{
				return null;
			}

			object[] p = new object[4];
			p[0] = (int)type;
			p[1] = owner.ToUpper();
			if (UserObject.IsUndefinedParentFolderName(parentFolder))
				p[2] = " "; // translated since null string is not an allowed value
			else p[2] = parentFolder;
			p[3] = name;

			drd.ExecuteReader(p);

			uo = FetchUserObject(drd);
			if (uo != null) // set current identifiers
			{
				uo.Owner = owner;
				uo.ParentFolder = parentFolder;
				uo.ParentFolderType = (parentFolder.ToUpper().StartsWith("FOLDER_")) ? FolderTypeEnum.User : FolderTypeEnum.System;
				uo.Name = name;
			}
			drd.Dispose();
			return uo;
		}

		/// <summary>
		/// Read user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject Read(UserObject uo)
		{
			if (uo.Id > 0) return Read(uo.Id); // if Id is defined then retrieve on that

			if (String.IsNullOrEmpty(uo.Name)) throw new Exception("UserObject name is not defined");

			if (uo.HasDefinedParentFolder)
			{
				if (!Lex.IsNullOrEmpty(uo.Owner) && uo.Type != UserObjectType.Unknown)
					return Read(uo.Type, uo.Owner, uo.ParentFolder, uo.Name); // fully qualified
				else return Read(uo.ParentFolder, uo.Name); // try with just folder & name
			}

			else // object not in a folder, must be something like a current list
			{
				if (uo.Type == UserObjectType.Unknown) throw new Exception("UserObject type is not defined");
				if (String.IsNullOrEmpty(uo.Owner)) uo.Owner = Security.UserName; // default to current user if not defined
				return Read(uo.Type, uo.Owner, uo.ParentFolder, uo.Name);
			}
		}

		/// <summary>
		/// Read object matching the type, folder, owner, name
		/// </summary>
		/// <param name="type">type of object</param>
		/// <param name="owner">userid of object owner</param>
		/// <param name="parentFolder">folder that it's in</param>
		/// <param name="objectName">name of object</param>
		/// <returns></returns>

		public static UserObject Read(
			UserObjectType type,
			string owner,
			string parentFolder,
			string objectName)
		{
			UserObject uo = null;

			if (owner == "" || objectName == "") return null;

			if (ReadTempObject(type, owner, parentFolder, objectName, ref uo))
				return uo;

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj " +
				"where obj_typ_id=:0 and ownr_id=:1 and " +
				"fldr_nm=:2 and lower(obj_nm) = lower(:3)";

			DbCommandMx drd = new DbCommandMx();

			OracleDbType[] pa = new OracleDbType[4];
			pa[0] = OracleDbType.Int32;
			pa[1] = OracleDbType.Varchar2;
			pa[2] = OracleDbType.Varchar2;
			pa[3] = OracleDbType.Varchar2;

			try
			{
				drd.Prepare(sql, pa);
			}
			catch (Exception)
			{
				return null;
			}

			object[] p = new object[4];
			p[0] = (int)type;
			p[1] = owner.ToUpper();
			if (UserObject.IsUndefinedParentFolderName(parentFolder))
				p[2] = " "; // translated since null string is not an allowed value
			else p[2] = parentFolder;
			p[3] = objectName;

			drd.ExecuteReader(p);

			uo = FetchUserObject(drd);
			if (uo != null) // set current identifiers
			{
				uo.ParentFolder = parentFolder;
				if (parentFolder == null || parentFolder.Trim() == "") uo.ParentFolderType = FolderTypeEnum.None;
				else if (parentFolder.ToUpper().StartsWith("FOLDER_")) uo.ParentFolderType = FolderTypeEnum.User;
				else uo.ParentFolderType = FolderTypeEnum.System;

				uo.Name = objectName;
			}
			drd.Dispose();
			return uo;
		}


		/// <summary>
		/// Read object matching the parent folder name and object name.
		/// Multiple objects with same folder & name may exist under different owners/types
		/// </summary>
		/// <param name="parentFolder"></param>
		/// <param name="objectName"></param>
		/// <returns></returns>

		public static UserObject Read(
			string parentFolder,
			string objectName)
		{
			UserObject uo = null;

			if (objectName == "") return null;

			if (ReadTempObject(UserObjectType.Unknown, null, parentFolder, objectName, ref uo))
				return uo;

			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
				"from mbs_owner.mbs_usr_obj " +
				"where  fldr_nm=:0 and lower(obj_nm) = lower(:1)";

			DbCommandMx drd = new DbCommandMx();

			OracleDbType[] pa = new OracleDbType[2];
			pa[0] = OracleDbType.Varchar2;
			pa[1] = OracleDbType.Varchar2;

			try
			{
				drd.Prepare(sql, pa);
			}
			catch (Exception)
			{
				return null;
			}

			object[] p = new object[2];
			if (UserObject.IsUndefinedParentFolderName(parentFolder))
				p[0] = " "; // translated since null string is not an allowed value
			else p[0] = parentFolder;
			p[1] = objectName;

			drd.ExecuteReader(p);

			uo = FetchUserObject(drd);
			if (uo != null) // set current identifiers
			{
				uo.ParentFolder = parentFolder;
				if (parentFolder == null || parentFolder.Trim() == "") uo.ParentFolderType = FolderTypeEnum.None;
				else if (parentFolder.ToUpper().StartsWith("FOLDER_")) uo.ParentFolderType = FolderTypeEnum.User;
				else uo.ParentFolderType = FolderTypeEnum.System;

				uo.Name = objectName;
			}
			drd.Dispose();
			return uo;
		}

		/// <summary>
		/// Read user object given the object id
		/// </summary>
		/// <param name="objectId">id of item to read</param>
		/// <returns>The requested user object or null if no matching user object is found.</returns>

		public static UserObject Read(int objectId)
		{
			UserObject uo = null;

			if (ReadTempObject(objectId, ref uo))
				return uo;

			string sql =
	"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
	"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt, acl " +
	"from mbs_owner.mbs_usr_obj where obj_id=:0";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql, OracleDbType.Int32);
			drd.ExecuteReader(objectId);

			uo = FetchUserObject(drd);
			drd.Dispose();

			string message = "Unable to fetch User Object. Verify User has permissions for this object: " + objectId;

			if (uo == null) DebugLog.Message(message);
			return uo;
		}


		/// <summary>
		/// Read next user object checking read permission (or null if no more remain)
		/// </summary>
		/// <param name="drd"></param>
		/// <returns>The next user object or null if there were no more to return.</returns>

		public static UserObject FetchUserObject(
			DbCommandMx drd)
		{
			UserObject uo;
			OracleDataReader dr = drd.OracleRdr;

			while (true)
			{
				if (!drd.Read()) return null;

				uo = new UserObject();
				if (!dr.IsDBNull(0)) uo.Id = (int)dr.GetOracleDecimal(0);
				if (!dr.IsDBNull(1)) uo.Type = (UserObjectType)(int)dr.GetOracleDecimal(1);
				if (!dr.IsDBNull(2)) uo.Owner = dr.GetString(2);
				if (!dr.IsDBNull(3)) uo.Name = dr.GetString(3);
				if (!dr.IsDBNull(4)) uo.Description = dr.GetString(4);
				if (!dr.IsDBNull(5)) uo.ParentFolderType = (FolderTypeEnum)(int)dr.GetOracleDecimal(5);
				if (!dr.IsDBNull(6)) uo.ParentFolder = dr.GetString(6);
				if (!dr.IsDBNull(7)) uo.AccessLevel = (UserObjectAccess)(int)dr.GetOracleDecimal(7);
				if (!dr.IsDBNull(8)) uo.Count = (int)dr.GetOracleDecimal(8);
				if (!dr.IsDBNull(9))
				{
					OracleClob ol = dr.GetOracleClob(9);
					uo.Content = ol.Value;
				}
				if (!dr.IsDBNull(10)) uo.CreationDateTime = dr.GetDateTime(10);
				if (!dr.IsDBNull(11)) uo.UpdateDateTime = dr.GetDateTime(11);
				if (!dr.IsDBNull(12)) uo.ACL = dr.GetString(12);

				if (Lex.IsNullOrEmpty(uo.Content) || // ok for all if no content
				 Permissions.UserHasReadAccess(Security.UserName, uo) || // ok if user has read access
				 Security.IsAdministrator(Security.UserName)) // admins can read all
				{
					if (RestrictedDatabaseView.UserIsMemberOfCurrentView(uo.Owner) && RestrictedMetatable.MetatableIsNotGenerallyRestricted(uo.Name)) // if restricted view then see if user is part of the view
                        
						break;
				}
			}

			return uo;
		}


		/// <summary>
		/// Write a new copy of the supplied user object to the database
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>True if successful, false otherwise.</returns>

		public static int Write(UserObject uo)
		{
			Write(uo, -1);
			return uo.Id;
		}

		/// <summary>
		/// Write user object to database
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="id"></param>

		public static int Write( // write user object
			UserObject uo,
			int id)
		{
			Write(uo, id, true);
			return uo.Id;
		}

		/// <summary>
		/// Write the user object to the database.
		/// If an id > 0 is supplied, then the user object, if any, with that id is deleted from the database to ensure that the id is available.
		/// Any other user object with the same type, owner, folder id, and name that exists will also be deleted.
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="id">preferred id (if id > 0)</param>

		public static int Write( // write user object
			UserObject uo,
			int id,
			bool setUpdateDateToCurrentDate)
		{
			bool deleted;
			string changeOpCode;
			UserObject existingUo = null;

			if (uo.IsTempObject) // if temp object just update in memory
			{
				WriteTempObject(uo, id);
				return uo.Id;
			}

			if (DbConnectionMx.NoDatabaseAccessIsAvailable)
				return -1; // don't try to write

			DbCommandMx drDao = null;
			try
			{
				//if (id == 273801) id = id; // debug

				if (id > 0) // id supplied?
				{
					existingUo = ReadHeader(id); // see if object with supplied id exists
					CheckForInvalidOwnerChange(existingUo, uo);
					deleted = Delete(id, false); // delete any existing object with id, don't notify ui
				}

				UserObject uo2 = ReadHeader(uo); // see if object with same name exists
				if (uo2 != null) // if object with same name but different id exists, delete it also
				{
					CheckForInvalidOwnerChange(uo2, uo);
					if (id != uo2.Id) deleted = Delete(uo2.Id);
					if (id <= 0) id = uo2.Id; // if no id supplied then use id of object with same name
					if (existingUo == null) existingUo = uo2; // say we're the existing object
				}

				if (existingUo == null) changeOpCode = "I"; // new of updated object
				else changeOpCode = "U";

				if (uo.CreationDateTime == DateTime.MinValue) // set creation date if not defined
					uo.CreationDateTime = DateTime.Now;

				if (uo.UpdateDateTime == DateTime.MinValue || setUpdateDateToCurrentDate) // set update date if not defined
					uo.UpdateDateTime = DateTime.Now;

				string sql =
					"insert into mbs_owner.mbs_usr_obj " +
					"(obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
					"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, chng_op_cd, chng_usr_id, crt_dt, upd_dt, acl) " +
					" values (:0,:1,:2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14)";

				drDao = new DbCommandMx();

				OracleDbType[] pa = new OracleDbType[15];

				pa[0] = OracleDbType.Int32;
				pa[1] = OracleDbType.Int32;
				pa[2] = OracleDbType.Varchar2;
				pa[3] = OracleDbType.Varchar2;
				pa[4] = OracleDbType.Varchar2;
				pa[5] = OracleDbType.Int32;
				pa[6] = OracleDbType.Varchar2;
				pa[7] = OracleDbType.Int32;
				pa[8] = OracleDbType.Int32;
				pa[9] = OracleDbType.Clob;
				pa[10] = OracleDbType.Varchar2;
				pa[11] = OracleDbType.Varchar2;
				pa[12] = OracleDbType.Date;
				pa[13] = OracleDbType.Date;
				pa[14] = OracleDbType.Varchar2;

				drDao.Prepare(sql, pa);

				object[] p = new object[15];
				if (id > 0) uo.Id = id; // keep same id if replacing existing object or id supplied
				else uo.Id = GetNextId();
				p[0] = uo.Id;
				p[1] = (int)uo.Type;
				p[2] = uo.Owner.ToUpper();
				p[3] = uo.Name;
				if (uo.Description != "") p[4] = uo.Description; else p[4] = " ";
				p[5] = uo.ParentFolderType;
				if (UserObject.IsUndefinedParentFolderName(uo.ParentFolder))
					p[6] = " "; // translated since null string is not an allowed value
				else p[6] = uo.ParentFolder;
				p[7] = uo.AccessLevel;
				p[8] = uo.Count;
				p[9] = uo.Content;
				p[10] = changeOpCode;
				p[11] = uo.Owner.ToUpper();
				p[12] = uo.CreationDateTime;
				p[13] = uo.UpdateDateTime;
				if (uo.ACL != "") p[14] = uo.ACL; else p[14] = " ";

				int count = drDao.ExecuteNonReader(p); // do the insert
				drDao.Dispose();

				if (uo.Type == UserObjectType.Query) // if object is a shared query check that underlying annotations, calc fields and lists are also shared
					AssureAccessToSharedQueryUserObjects(uo);

				//if (IUserObjectIUD != null) // notify other object of change 
				//{
				//  if (existingUo == null)
				//    IUserObjectIUD.UserObjectInserted(uo);
				//  else IUserObjectIUD.UserObjectUpdated(uo);
				//}

				return uo.Id;
			}
			catch (Exception e)
			{
				if (drDao != null) drDao.Dispose();
				DebugLog.Message(DebugLog.FormatExceptionMessage(e));
				throw new Exception(e.Message, e);
			}
		}

		/// <summary>
		/// Store in-memory temporary UserObject
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>True if this is a "Current" object</returns>

		public static void WriteTempObject(
			UserObject uo,
			int id)
		{
			int delKey = -1; // delete any object with same name
			foreach (int id0 in TempUserObjects.Keys)
			{
				if (Lex.Eq(TempUserObjects[id0].Name, uo.Name))
				{
					delKey = id0;
				}
			}

			if (delKey >= 0)
			{
				TempUserObjects.Remove(delKey);
				if (id <= 0) id = delKey; // use same id
			}

			if (id <= 0)
			{
				MaxTempUserObjectId++;
				id = MaxTempUserObjectId;
			}

			uo.Id = id;
			TempUserObjects[id] = uo.Clone(); // make copy & store in in-memory buffer
		}

		/// <summary>
		/// Get next available object id
		/// </summary>
		/// <returns></returns>
		public static int GetNextId()
		{
			int id = SequenceDao.NextVal("mbs_owner.mbs_usr_obj_seq");
			return id;
		}

		/// <summary>
		/// Update header for user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static void UpdateHeader( // update header info for object
			UserObject uo)
		{
			UpdateHeader(uo, true, true);
		}

		/// <summary>
		/// Update header for user object
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="setUpdateDateToCurrentDate"></param>

		public static void UpdateHeader( // update header info for object
			UserObject uo,
			bool setUpdateDateToCurrentDate)
		{
			UpdateHeader(uo, setUpdateDateToCurrentDate, true);
		}

		/// <summary>
		/// Update header for user object
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="setUpdateDateToCurrentDate"></param>
		/// <param name="notifyOthers"></param>

		public static void UpdateHeader( // update header info for object
			UserObject uo,
			bool setUpdateDateToCurrentDate,
			bool notifyOthers)
		{
			DbCommandMx drDao = null;
			try
			{

				UserObject uo0 = UserObjectDao.ReadHeader(uo.Id);
				if (uo0 == null) throw new Exception("User object not found: " + uo.Id);
				CheckForInvalidOwnerChange(uo0, uo);

				if (uo.UpdateDateTime == DateTime.MinValue || setUpdateDateToCurrentDate) // set update date if not defined
					uo.UpdateDateTime = DateTime.Now;

				string sql =
					"update mbs_owner.mbs_usr_obj " +
					"set obj_typ_id = :0, ownr_id = :1, obj_nm = :2, " +
					"obj_desc_txt = :3, fldr_typ_id = :4, fldr_nm = :5, " +
					"acs_lvl_id = :6, obj_itm_cnt=:7, chng_op_cd = 'U', chng_usr_id = :8, crt_dt = :9, upd_dt = :10, acl = :11 " +
					"where obj_id = :12";

				drDao = new DbCommandMx();

				OracleDbType[] pa = new OracleDbType[13];

				pa[0] = OracleDbType.Int32;
				pa[1] = OracleDbType.Varchar2;
				pa[2] = OracleDbType.Varchar2;
				pa[3] = OracleDbType.Varchar2;
				pa[4] = OracleDbType.Int32;
				pa[5] = OracleDbType.Varchar2;
				pa[6] = OracleDbType.Int32;
				pa[7] = OracleDbType.Int32;
				pa[8] = OracleDbType.Varchar2;
				pa[9] = OracleDbType.Date;
				pa[10] = OracleDbType.Date;
				pa[11] = OracleDbType.Varchar2;
				pa[12] = OracleDbType.Int32;

				drDao.Prepare(sql, pa);

				object[] p = new object[13];
				p[0] = (int)uo.Type;
				p[1] = uo.Owner.ToUpper();
				p[2] = uo.Name;
				if (uo.Description != "") p[3] = uo.Description; else p[3] = " ";
				p[4] = uo.ParentFolderType;
				if (uo.ParentFolder != "") p[5] = uo.ParentFolder; else p[5] = " "; //translated since null string is not an allowed value
				p[6] = uo.AccessLevel;
				p[7] = uo.Count;
				p[8] = uo.Owner.ToUpper();
				p[9] = uo.CreationDateTime;
				p[10] = uo.UpdateDateTime;
				if (!Lex.IsNullOrEmpty(uo.ACL)) p[11] = uo.ACL; else p[11] = " ";
				p[12] = uo.Id;

				int count = drDao.ExecuteNonReader(p);
				drDao.Dispose();

				if (uo.Type == UserObjectType.Query) // if object is a shared query check that underlying annotations, calc fields and lists are also shared
					AssureAccessToSharedQueryUserObjects(uo);

				//if (notifyOthers && IUserObjectIUD != null && uo.ParentFolderType != 0) // notify other objects of change 
				//  IUserObjectIUD.UserObjectUpdated(uo);

				return;
			}
			catch (Exception e)
			{
				if (drDao != null) drDao.Dispose();
				throw new Exception(e.Message, e);
			}
		}

		/// <summary>
		/// Check for invalid owner change
		/// </summary>
		/// <param name="uo0"></param>
		/// <param name="uo"></param>

		static void CheckForInvalidOwnerChange(
			UserObject oldUo,
			UserObject newUo)
		{
			if (oldUo == null || newUo == null) return;
			if (oldUo.Id <= 0 || newUo.Id <= 0 || oldUo.Id != newUo.Id) return;
			if (Lex.IsNullOrEmpty(oldUo.Owner) || Lex.IsNullOrEmpty(newUo.Owner) ||
				Lex.Eq(newUo.Owner, oldUo.Owner)) return;
			if (Lex.Eq(newUo.Content, "ChangeOwner")) return;

			string msg = "Invalid attempt to change owner for user object " + newUo.Id;

			string msg2 = "Invalid attempt to change owner for user object";
			msg2 += "\r\n Old: id: " + oldUo.Id + ", owner: " + oldUo.Owner + ", folder: " + oldUo.ParentFolder + ", name: " + oldUo.Name +
				 ", created: " + oldUo.CreationDateTime + ", updated: " + oldUo.UpdateDateTime;
			msg2 += "\r\n New: id: " + newUo.Id + ", owner: " + newUo.Owner + ", folder: " + newUo.ParentFolder + ", name: " + newUo.Name +
				", created: " + newUo.CreationDateTime + ", updated: " + newUo.UpdateDateTime;
			msg2 += "\r\n\r\n" + new StackTrace(true);

			Email.SendCriticalEventNotificationToMobiusAdmin(msg, msg2);

			throw new Exception(msg2);
		}

		/// <summary>
		/// Update the update date & count columns
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="updateDateTime"></param>
		/// <param name="count"></param>

		public static void UpdateUpdateDateAndCount(
			int objectId,
			DateTime updateDateTime,
			int count)
		{
			UserObject uo = UserObjectDao.ReadHeader(objectId);
			if (uo == null) throw new Exception("User object not found: " + objectId);
			if (uo.UpdateDateTime == updateDateTime && uo.Count == count) return;

			uo.UpdateDateTime = updateDateTime;
			uo.Count = count;
			UserObjectDao.UpdateHeader(uo, false);
			return;
		}


		/// <summary>
		/// Update the update date column
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="updateDateTime"></param>

		public static void UpdateUpdateDate(
			int objectId,
			DateTime updateDateTime)
		{
			UserObject uo = UserObjectDao.ReadHeader(objectId);
			if (uo == null) throw new Exception("User object not found: " + objectId);
			if (uo.UpdateDateTime == updateDateTime) return;

			uo.UpdateDateTime = updateDateTime; // 
			UserObjectDao.UpdateHeader(uo, false); //
			return;
		}

		/// <summary>
		/// Update the update count column
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="count"></param>

		public static void UpdateCount(
			int objectId,
			int count)
		{
			UserObject uo = UserObjectDao.ReadHeader(objectId);
			if (uo == null) throw new Exception("User object not found: " + objectId);
			if (uo.Count == count) return;

			uo.Count = count;
			UserObjectDao.UpdateHeader(uo, false);
			return;
		}

		/// <summary>
		/// Delete user object
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static bool Delete(int objectId)
		{
			return Delete(objectId, true);
		}

		/// <summary>
		/// Delete a user object by id.
		/// Optionally calls IUDUserObject.UserObjectDeleted to trigger content tree updates
		/// </summary>
		/// <param name="objectId">id of user object</param>
		/// <param name="notifyUI">if true notify any associated UI of change</param>
		/// <returns>True if successful, false otherwise.</returns>

		public static bool Delete(
			int objectId,
			bool notifyUI)
		{
			UserObject uo = ReadHeader(objectId);
			if (uo == null) return false;

			string sql =
				"delete from mbs_owner.mbs_usr_obj " +
				"where obj_id = :0";

			DbCommandMx drDao = new DbCommandMx();

			drDao.Prepare(sql, OracleDbType.Int32);
			int count = drDao.ExecuteNonReader(objectId);
			drDao.Dispose();

			//if (notifyUI && IUserObjectIUD != null && uo.ParentFolderType != 0) // notify other object of change 
			//  IUserObjectIUD.UserObjectDeleted(uo);

			if (count > 0) return true;
			else return false;
		}

		/// <summary>
		/// Get next sequence number for an event type
		/// </summary>
		/// <param name="eventName"></param>
		/// <returns></returns>

		public static int GetEventSequenceNumber(string eventName)
		{
			int eventId = 0;

			Mutex mutex = new Mutex(false, "MobiusAlertQueue");
			mutex.WaitOne(); // get exclusive access
			try
			{
				string txt = GetUserParameter("Mobius", "Event " + eventName, "0");
				int.TryParse(txt, out eventId);
				eventId++;
				SetUserParameter("Mobius", "Event " + eventName, eventId.ToString());
			}
			finally { mutex.ReleaseMutex(); }

			return eventId;
		}

		/// <summary>
		/// Get a user profile parameter 
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="parm"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>

		public static string GetUserParameter( // 
			string userid,
			string parm,
			string defaultValue)
		{
			string result = GetUserParameter(userid, parm);
			if (String.IsNullOrEmpty(result))
				result = defaultValue;
			return result;
		}

		/// <summary>
		/// Get a user profile parameter 
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="parm"></param>
		/// <returns></returns>

		public static string GetUserParameter(
			string userid,
			string parm)
		{
			string desc = "", datetime = "";
			int objid = 0;
			string share_with = "";
			int count = 0;

			UserObject uo = Read(UserObjectType.UserParameter, userid, "", parm);
			if (uo == null) uo = Read(UserObjectType.UserParameter, userid, "DEFAULT_FOLDER", parm); // try old default_folder if no luck
			if (uo == null) return "";

			string value = GetProperUserParameterValue(uo);
			return value;
		}

		/// <summary>
		/// Get all user parameters for a user
		/// </summary>
		/// <param name="userid"></param>
		/// <returns></returns>

		public static Dictionary<string, string> GetUserParameters(
			string userid)
		{
			string desc = "", datetime = "";
			int objid = 0;
			string share_with = "";
			int count = 0;

			Dictionary<string, string> upDict = new Dictionary<string, string>();

			List<UserObject> uoList = ReadMultiple(UserObjectType.UserParameter, userid, false, true);
			foreach (UserObject uo0 in uoList)
			{
				//if (Lex.Contains(uo0.Name, "FavoriteStructures")) uo0.Name = uo0.Name; // debug

				string value = GetProperUserParameterValue(uo0);
				upDict[uo0.Name.ToUpper()] = value;
			}

			return upDict;
		}

		/// <summary>
		/// GetUserParameterValue
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		static string GetProperUserParameterValue(UserObject uo)
		{
			if (uo == null) return "";

			string value = "";
			if (Lex.IsDefined(uo.Description)) // normal value (<4000 chars)
				value = uo.Description;

			else if (Lex.IsDefined(uo.Content)) // long value
				value = uo.Content;

			return value;
		}

		/// <summary>
		/// Set a user profile parameter 
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="parmName"></param>
		/// <param name="value"></param>

		public static void SetUserParameter(
			string userid,
			string parmName,
			string value)
		{
			UserObject uo = new UserObject();
			uo.Type = UserObjectType.UserParameter;
			uo.Owner = userid;
			uo.Name = parmName;

			if (value == null || value.Length < 4000) // max 4000 chars for desc column
				uo.Description = value; 
			else uo.Content = value; // use long content col for storage if >= 4000 chars to avoid overflow exceptions

			Write(uo);
			return;
		}

		/// <summary>
		/// Disable all account information and user objects for the specified user
		/// </summary>
		/// <param name="userName"></param>

		public static bool DeleteAllUserObjects(string userName)
		{
			string userName2 = userName + "~";

			string sql =
				"delete from mbs_owner.mbs_usr_obj " +
				"where ownr_id = " + Lex.AddSingleQuotes(userName2);

			int count = DbCommandMx.PrepareAndExecuteNonReaderSql(sql);

			sql =
				"update mbs_owner.mbs_usr_obj " +
				"set ownr_id = " + Lex.AddSingleQuotes(userName2) + " " +
				"where ownr_id = " + Lex.AddSingleQuotes(userName);

			count = DbCommandMx.PrepareAndExecuteNonReaderSql(sql);
			if (count > 0) return true;
			else return false;
		}

		/// <summary>
		/// Check that access to user objects (i.e. annotations, calc fields, lists) associated with a shared query are shared at the same level
		/// </summary>
		/// <param name="queryId"></param>

		public static int AssureAccessToSharedQueryUserObjects(
			int queryId)
		{
			UserObject uo = UserObjectDao.Read(queryId);
			return AssureAccessToSharedQueryUserObjects(uo);
		}

		/// <summary>
		/// Check that access to user objects (i.e. annotations, calc fields, lists) associated with a shared query are shared at the same level
		/// </summary>
		/// <param name="uo"></param>

		public static int AssureAccessToSharedQueryUserObjects(
			UserObject uo)
		{
			int updCount = 0;

			if (uo == null || uo.Type != UserObjectType.Query) return updCount;

			AccessControlList acl = AccessControlList.Deserialize(uo);
			if (!acl.IsShared) return updCount;

			if (Lex.IsNullOrEmpty(uo.Content)) uo = UserObjectDao.Read(uo.Id); // get content if don't have
			if (uo == null)
			{
				DebugLog.Message("Query not found: " + uo.Id);
				return updCount;
			}

			Query q = Query.Deserialize(uo.Content);
			if (!Lex.IsNullOrEmpty(q.KeyCriteria))
			{
				string criteria = "KeyColumn " + q.KeyCriteria;
				ParsedSingleCriteria psc = psc = MqlUtil.ParseSingleCriteria(criteria);
				psc.Value = Lex.RemoveAllQuotes(psc.Value);
				if (UserObject.IsCompoundIdListName(psc.Value))
				{
					string listName = psc.Value;
					if (Security.AssignMatchingReadAccess(uo.Id, acl, listName))
						updCount++;
				}
			}

			foreach (QueryTable qt in q.Tables)
			{
				MetaTable mt = qt.MetaTable;
				if (!mt.IsUserObjectTable) continue;

				if (Security.AssignMatchingReadAccess(uo.Id, acl, mt.Name))
					updCount++;
			}

			return updCount;
		}

		///// <summary>
		///// When the user is setting a Query to be their Default Mobile Query, we need to make sure it is the only default query for that owner.
		///// We pass the current user object and ignore clearing the MobileDefault for it, because there is no reason to clear the current query if 
		///// we are already in the process of saving it. But any other query set as DeFaultMobile must be
		///// </summary>
		///// <param name="uo"></param>
		//public static void ClearMobileDefaultQueryForOwner(UserObject uo)
		//{
		//    List<UserObject> mobiusUserObjects = ReadMultiple(Data.UserObjectType.Query, uo.Owner, false, true);
		//    foreach (UserObject userObject in mobiusUserObjects)
		//    {
		//        Query query = Query.Deserialize(userObject.Content);
		//        if (query.MobileDefault && userObject.Id != uo.Id)
		//        {
		//            query.MobileDefault = false;
		//            string content = query.Serialize();
		//            userObject.Content = content;
		//            Write(userObject);
		//        }
		//    }
		//}

		/// <summary>
		/// Set the current Query attribute MobileDefault to true and ensure all other Query objects are set to false.
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>
		public static bool SetMobileDefaultQueryForOwner(UserObject uo, string ownerId)
		{
			UserObjectDao.SetUserParameter(ownerId, "MobileDefaultQuery", uo.Id.ToString());
			return true;

			//   try
			//{
			//    if (uo.Owner.ToLower() != ownerId.ToLower())
			//    {
			//           DebugLog.Message("Query " + uo.Name + " is owned by " + uo.Owner + " and cannot be changed by " + ownerId);
			//           return false;
			//       }


			//       // read all query objects for the current user
			//       List<UserObject> mobiusUserObjects = ReadMultiple(Data.UserObjectType.Query, uo.Owner, false, true);
			//    if (mobiusUserObjects == null || mobiusUserObjects.Count == 0)
			//    {
			//           DebugLog.Message("No queries found for this user.");
			//        return false;
			//    }

			//    bool mobileDefaultExists = false;
			//    UserObject oldDefault = null;

			//       foreach (UserObject userObject in mobiusUserObjects)
			//       {
			//           Query query = Query.Deserialize(userObject.Content);

			//           if (query.MobileDefault)
			//           {
			//               // current query is already marked as default, do nothing
			//               if (userObject.Id == uo.Id)
			//               {
			//                   mobileDefaultExists = true;
			//                   continue;
			//               }

			//               // found a different query that is marked as default. Set Default to false
			//               query.MobileDefault = false;
			//               string content = query.Serialize();
			//               userObject.Content = content;
			//               oldDefault = userObject.Clone();
			//               //Write(userObject);
			//           }
			//           else if (userObject.Id == uo.Id)
			//           {
			//               // we found the matching query
			//               query.MobileDefault = true;
			//               string content = query.Serialize();
			//               userObject.Content = content;
			//               Write(userObject);
			//               mobileDefaultExists = true;
			//           }
			//       }
			//       // Turn MobileDefault to false only if we have set a new one to true
			//    if (mobileDefaultExists && oldDefault != null) Write(oldDefault);
			//    if (!mobileDefaultExists && oldDefault == null)
			//    {
			//           DebugLog.Message("Query " + uo.Name + " was not found and there is currently no default set for " + ownerId + ".");
			//           return false;
			//       }

			//}
			//catch (Exception ex)
			//{
			//    DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
			//}
			//   return true;
		}
	}

	/// <summary>
	/// An instance of this class is created to bridge between an IUserObjectDao interface reference and the static methods in UserObjectDao
	/// </summary>

	public class IUserObjectDaoMethods : IUserObjectDao
	{
		/// <summary>
		/// Read header for specified objectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public UserObject ReadHeader(
			int objectId)
		{
			return UserObjectDao.ReadHeader(objectId);
		}

		/// <summary>
		/// Read header for specified objectId
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public UserObject Read(
			int objectId)
		{
			return UserObjectDao.Read(objectId);
		}

		/// <summary>
		/// Fetch all objects of a given type regardless of who owns it or where it is located.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="includeContent"></param>
		/// <returns>Set of user objects ordered by obj_id</returns>

		public List<UserObject> ReadMultiple(
			UserObjectType type,
			bool includeContent)
		{
			return UserObjectDao.ReadMultiple(type, includeContent);
		}

	}

}
