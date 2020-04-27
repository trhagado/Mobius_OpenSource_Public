using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Mobius.UAL
{
	/// <summary>
	/// Higher level object-type-specific operations on user objects
	/// </summary>

	public class CidListDao : ICidListDao
	{
		/// <summary>
		/// See if the named list exists
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static bool Exists(
			string name)
		{
			UserObject uo = UserObject.ParseInternalUserObjectName(name, Security.UserName);
			uo.Type = UserObjectType.CnList;
			uo = UserObjectDao.ReadHeader(uo);
			return (uo != null);
		}

		/// <summary>
		/// Read a compound id list given a user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static CidList Read(
			UserObject uo)
		{
			string name = uo.InternalName;
			return CidListDao.Read(name, null);
		}

		/// <summary>
		/// Read a compound id list given a user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static CidList Read(
			UserObject uo,
			MetaTable mt)
		{
			string name = uo.InternalName;
			return CidListDao.Read(name, mt);
		}

		/// <summary>
		/// Read a compound id list given an internal list name (e.g. FOLDER_123.name or LIST_1234)
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static CidList Read(
			string internalName)
		{
			return CidListDao.Read(internalName, null);
		}

		/// <summary>
		/// Read a compound id list given a list name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static CidList Read(
			string name,
			MetaTable mt)
		{
			UserObject uo;
			CidList cnList = new CidList();
			string fileName, cn;
			int i1;

			uo = UserObject.ParseInternalUserObjectName(name, UserObjectType.CnList, Security.UserName);
			if (uo == null) return null;

			uo = UserObjectDao.Read(uo);
			if (uo == null) return new CidList();

			return CidList.Deserialize(uo, mt);
		}

/// <summary>
/// Read a compoundId list given the listId
/// </summary>
/// <param name="listId"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public virtual CidList VirtualRead(
			int listId,
			MetaTable mt)
		{
			return Read(listId, mt);
		}

		/// <summary>
		/// Read a compoundId list given the listId
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static CidList Read(
			int listId,
			MetaTable mt)
		{
			UserObject uo = UserObjectDao.Read(listId);
			if (uo == null) return new CidList();
			else return CidList.Deserialize(uo, mt);
		}

		/// <summary>
		/// Write a list
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static int Write(
			CidList list,
			string name)
		{
			UserObject UserObject = new UserObject(UserObjectType.CnList);
			UserObject.Name = name;
			return Write(list, UserObject);
		}

		public static int Write(
			CidList list,
			string name,
			string description)
		{
			UserObject UserObject = new UserObject(UserObjectType.CnList);
			UserObject.Name = name;
			UserObject.Description = description;
			return Write(list, UserObject);
		}

		public static int Write(
			CidList list,
			string name,
			string description,
			string folderId,
			UserObjectAccess accessLevel)
		{
			UserObject UserObject = new UserObject(UserObjectType.CnList); 
			UserObject.Name = name;
			UserObject.Description = description;
			UserObject.ParentFolder = folderId;
			UserObject.ParentFolderType = (folderId.ToUpper().StartsWith("FOLDER_")) ? FolderTypeEnum.User : FolderTypeEnum.System;
			UserObject.AccessLevel = accessLevel;
			return Write(list, UserObject);
		}

		public static int WriteWithClonedUserObject(
			CidList list,
			UserObject uo)
		{
			UserObject UserObject = new UserObject(UserObjectType.CnList);
			UserObject.Owner = uo.Owner;
			UserObject.Name = uo.Name;
			UserObject.Description = uo.Description;
			UserObject.ParentFolder = uo.ParentFolder;
			UserObject.ParentFolderType = uo.ParentFolderType;
			UserObject.AccessLevel = uo.AccessLevel;
			return Write(list, UserObject);
		}

		/// <summary>
		/// Write a compound number list
		/// </summary>
		/// <returns></returns>

		public static int Write(
			CidList list)
		{
			return Write(list, list.UserObject);
		}

		/// <summary>
		/// Write a compound number list
		/// </summary>
		/// <returns></returns>

		public static int Write(
			CidList list,
			UserObject uo)
		{
			string fileName;

			string content = list.ToMultilineString();

			uo.Type = UserObjectType.CnList;
			uo.Owner = Security.UserName; // assume current user

			if (!uo.HasDefinedParentFolder &&
				(Lex.Eq(uo.Name, "Current") || Lex.Eq(uo.Name, "Previous") || Lex.Eq(uo.Name, "Criteria List")))
			{
				uo.ParentFolder = UserObject.TempFolderName;
				uo.ParentFolderType = FolderTypeEnum.None;
				uo.Owner = Security.UserName;
			}

			else if (!uo.HasDefinedParentFolder) throw new Exception("No parent folder for list");

			uo.Content = content;
			uo.Count = list.Count;
			UserObjectDao.Write(uo);

			return list.Count;
		}

		/// <summary>
		/// Execute list logic on a pair of persisted list objects & store results in current list for user
		/// </summary>
		/// <param name="list1InternalName"></param>
		/// <param name="list2InternalName"></param>
		/// <param name="op"></param>
		/// <returns>Number of results from logic operation</returns>

		public static int ExecuteListLogic(
			string list1InternalName,
			string list2InternalName,
			ListLogicType op)
		{
			CidList list1 = Read(list1InternalName);
			if (list1 == null) throw new Exception("List not found: " + list1InternalName);

			CidList list2 = Read(list2InternalName);
			if (list2 == null) throw new Exception("List not found: " + list2InternalName);
			
			list1.ApplyListLogic(list2, op);

			CidListDao.Write(list1, "Current"); // write new current list
			return list1.Count;
		}

/// <summary>
/// Copy a list to another list
/// </summary>
/// <param name="sourceList"></param>
/// <param name="destList"></param>
/// <returns></returns>

		public static int CopyList(
						UserObject sourceList,
						UserObject destList)
		{
			CidList list = CidListDao.Read(sourceList);
			CidListDao.Write(list, destList);
			return list.Count;
		}

/// <summary>
/// Copy a list to another list
/// </summary>
/// <param name="sourceListUOInternalName"></param>
/// <param name="destList"></param>
/// <returns></returns>

		public static int CopyList(
						string sourceListUOInternalName,
						UserObject destList)
		{
			CidList list = CidListDao.Read(sourceListUOInternalName);
			CidListDao.Write(list, destList);
			return list.Count;
		}

/// <summary>
/// Read the list of compounds for a library
/// </summary>
/// <param name="libId"></param>
/// <returns></returns>

		public static CidList ReadLibrary(
			int libId)
		{
			CidList l = new CidList();
			DbCommandMx dao = new DbCommandMx();
			string sql = @"
				SELECT l.library_name, l.library_desc_text, s.corp_nbr 
				FROM corp_owner.corp_substance s,
				 corp_owner.corp_library_substance ls,
				 corp_owner.corp_library l
				WHERE
				 l.lib_id = <libId> and 
				 s.cpd_id = ls.cpd_id and
				 l.lib_id = ls.lib_id";

			sql = Lex.Replace(sql, "<libId>", libId.ToString());
			dao.Prepare(sql);
			dao.ExecuteReader();

			MetaTable rootMt = MetaTableCollection.Get(MetaTable.PrimaryRootTable);

			while (dao.Read())
			{
				if (Lex.IsNullOrEmpty(l.UserObject.Name))
				{
					string name = dao.GetString(0);
					if (Lex.IsNullOrEmpty(name) || Lex.IsInteger(name)) name = dao.GetString(1); // use desc if no name or just a number
					if (Lex.IsNullOrEmpty(name)) name = "Library " + libId;
					l.UserObject.Name = name;
				}

				int intCorpId = dao.GetInt(2);
				string corpId = CompoundId.Normalize(intCorpId, rootMt);
				l.Add(corpId);
			}

			dao.CloseReader();
			dao.Dispose();

			return l;
		}

	}
}
