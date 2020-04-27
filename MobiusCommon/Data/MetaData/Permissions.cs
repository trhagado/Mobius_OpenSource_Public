using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{
/// <summary>
/// High level permission methods
/// </summary>

	public class Permissions
	{

/// <summary>
/// Cache of permissions keyed by userName_internalObjectName
/// Improves performance by eliminating the need to read a UserObject if we already have its permissions.
/// It is assumed that permissions will not change for the current user during a single session.
/// </summary>

		public static Dictionary<string, PermissionEnum> PermissionsCache = new Dictionary<string, PermissionEnum>();

/// <summary>
/// Allow temp read so public queries can read their underlying user objects.
/// Only supported for server side operations
/// </summary>

		public static bool AllowTemporaryPublicReadAccessToAllUserObjects = false; // allow new objects to be added to the dict 
		public static Dictionary<int, object> TemporaryPublicReadAccessUserObjects = new Dictionary<int, object>(); // dict of objects with temp read access

		/// <summary>
		/// Return true if the user object is shared with anyone other than the owner
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool IsShared(
			UserObject uo)
		{
			if (uo.AccessLevel == UserObjectAccess.Public) return true;
			else if (uo.AccessLevel == UserObjectAccess.Private) return false;
			else if (uo.AccessLevel == UserObjectAccess.ACL)
			{
				AccessControlList acl = AccessControlList.Deserialize(uo.ACL);
				if (acl.ReaderCount >= 2) return true; // say shared if two or more readers
				else return false;
			}

			else return false; // unexpected AccessLevel
		}

/// <summary>
/// Return true if the user has read access to the specified data/user object
/// </summary>
/// <param name="userName"></param>
/// <param name="internalObjectName"></param>
/// <returns></returns>

		public static bool UserHasReadAccess(
			string userName,
			string internalObjectName)
		{
			PermissionEnum permissions = GetUserPermissions(userName, internalObjectName);
			return ReadIsAllowed(permissions);
		}

/// <summary>
/// Return true if the user has read access to the specified data/user object
/// </summary>
/// <param name="userName"></param>
/// <param name="objId"></param>
/// <returns></returns>

		public static bool UserHasReadAccess(
			string userName,
			int objId)
		{
			PermissionEnum permissions = GetUserObjectPermissions(userName, objId);
			return ReadIsAllowed(permissions);
		}

		/// <summary>
		/// Return true if permissions include read
		/// </summary>
		/// <param name="permissions"></param>
		/// <returns></returns>

		public static bool ReadIsAllowed(PermissionEnum permissions)
		{
			return (permissions & PermissionEnum.Read) != 0;
		}

/// <summary>
/// Return true if the user has read access to the specified data/user object
/// </summary>
/// <param name="userName"></param>
/// <param name="internalObjectName"></param>
/// <returns></returns>

		public static bool UserHasWriteAccess(
			string userName,
			string internalObjectName)
		{
			PermissionEnum permissions = GetUserPermissions(userName, internalObjectName);
			return WriteIsAllowed(permissions);
		}

		/// <summary>
		/// Return true if the user has write access to the specified data/user object
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="objId"></param>
		/// <returns></returns>

		public static bool UserHasWriteAccess(
			string userName,
			int objId)
		{
			PermissionEnum permissions = GetUserObjectPermissions(userName, objId);
			return WriteIsAllowed(permissions);
		}

		/// <summary>
		/// Return true if permissions include write
		/// </summary>
		/// <param name="permissions"></param>
		/// <returns></returns>

		public static bool WriteIsAllowed(PermissionEnum permissions)
		{
			return (permissions & PermissionEnum.Write) != 0;
		}
			
/// <summary>
/// Get the list of permissions the user has for the specified object
/// </summary>
/// <param name="userName"></param>
/// <param name="internalObjectName"></param>
/// <returns></returns>

		public static PermissionEnum GetUserPermissions(
			string userName,
			string internalObjectName)
		{
			PermissionEnum permissions = PermissionEnum.None;

			if (UserObject.IsUserObjectInternalName(internalObjectName)) // UserObject
			{
				UserObjectType type;
				int id;

				string key = userName.ToUpper() + "_" + internalObjectName.ToUpper();
				if (PermissionsCache.ContainsKey(key))
					return PermissionsCache[key];

				UserObject.ParseObjectTypeAndIdFromInternalName(internalObjectName, out type, out id);
				if (id <= 0)
				{
					if (ClientState.IsAdministrator()) return PermissionEnum.Read;
					else return PermissionEnum.None;
				}

				UserObject uo = InterfaceRefs.IUserObjectDao.ReadHeader(id);
				AccessControlList acl = AccessControlList.Deserialize(uo);

				permissions = acl.GetUserPermissions(userName);
				PermissionsCache[key] = permissions; // update cache
			}

			else // assume database table name with full access
				permissions = PermissionEnum.Read | PermissionEnum.Write;

			return permissions;
		}

/// <summary>
/// Return true if user object exists
/// </summary>
/// <param name="id"></param>
/// <returns></returns>

		public static bool UserObjectExists (
			int id)
		{
			UserObject uo = InterfaceRefs.IUserObjectDao.ReadHeader(id);
			if (uo != null) return true;
			else return false;
		}

/// <summary>
/// Get permissions for specified user object
/// </summary>
/// <param name="userName"></param>
/// <param name="id"></param>
/// <returns></returns>

		public static PermissionEnum GetUserObjectPermissions(
			string userName,
			int id)
		{
			PermissionEnum permissions = PermissionEnum.None;
			UserObject uo = InterfaceRefs.IUserObjectDao.ReadHeader(id);
			AccessControlList acl = AccessControlList.Deserialize(uo);

			permissions = acl.GetUserPermissions(userName);
			return permissions;
		}

		/// <summary>
		/// See if the supplied user name can read the user object the supplied object
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool UserHasReadAccess(
			string userName,
			UserObject uo)
		{
            if (uo == null) return false;
            else if (Lex.Eq(uo.Owner, userName) || // do we own it
             Lex.Eq(uo.Owner, "Mobius") || // allow anyone to read these
             Lex.Eq(uo.Owner, "AlertMonitor")) // or these
                return true;
            else if (uo.AccessLevel == UserObjectAccess.None) return true; // allow all to read if no security
            else if (uo.AccessLevel == UserObjectAccess.Public) return true;

            else if (TemporaryPublicReadAccessUserObjects.ContainsKey(uo.Id)) return true; // temp read turned on for this object
            else if (AllowTemporaryPublicReadAccessToAllUserObjects) // temp read all turned on
            {
                Permissions.TemporaryPublicReadAccessUserObjects[uo.Id] = null; // mark to allow temp read later also
                return true;
            }

            else if (uo.AccessLevel == UserObjectAccess.ACL) // check the acl
            {
                AccessControlList acl = AccessControlList.Deserialize(uo.ACL);
                if (acl.UserHasReadAccess(userName))
                    return true;
                else return false;
            }

            else return false; // unexpected AccessLevel
		}

		/// <summary>
		/// See if the supplied user name can write the user object the supplied object
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool UserHasWriteAccess(
			string userName,
			UserObject uo)
		{
			if (uo == null) return false;
			else if (Lex.Eq(uo.Owner, userName)) return true;
			else if (uo.AccessLevel == UserObjectAccess.Public) return false;
			else if (uo.AccessLevel == UserObjectAccess.ACL)
			{
				AccessControlList acl = AccessControlList.Deserialize(uo.ACL);
				return acl.UserHasWriteAccess(userName);
			}

			else return false; // unexpected AccessLevel
		}

/// <summary>
/// Update permissions to reflect a change in ownership
/// </summary>
/// <param name="uo"></param>
/// <param name="userName"></param>

		public static void UpdateAclForNewOwner(
			UserObject uo,
			string oldOwner,
			string newOwner)
		{
			if (uo.AccessLevel != UserObjectAccess.ACL) return; // only need to do for ACL

			AccessControlList acl = AccessControlList.Deserialize(uo.ACL);
			acl.RemoveUserItem(oldOwner);
			acl.AddReadWriteUserItem(newOwner);
			uo.ACL = acl.Serialize();
		}

	}

/// <summary>
/// Access Control List
/// </summary>

	public class AccessControlList
	{
		public List<AclItem> Items = new List<AclItem>();


/// <summary>
/// Constructor
/// </summary>

		public AccessControlList()
		{
			return;
		}

/// <summary>
/// Serialize into a UserObject
/// </summary>
/// <param name="uo"></param>

		public void Serialize(UserObject uo)
		{
			uo.ACL = "";
			if (IsPrivate)
				uo.AccessLevel = UserObjectAccess.Private;
			else if (IsPublic)
				uo.AccessLevel = UserObjectAccess.Public;
			else
			{
				uo.AccessLevel = UserObjectAccess.ACL;
				uo.ACL = Serialize();
			}
		}

/// <summary>
/// Return true if private, i.e. only the owner has access
/// </summary>

		public bool IsPrivate
		{
			get
			{
				if (ReaderCount <= 1) return true;
				else return false; 
			}
		}

/// <summary>
/// Return true is some sharing of read
/// </summary>

		public bool IsShared
		{
			get
			{
				if (ReaderCount > 1) return true;
				else return false;
			}
		}

/// <summary>
/// Return true if strictly public 
/// </summary>

		public bool IsPublic
		{
			get
			{
				if (Items.Count == 2 && HasPublicRead) return true;
				else return false;
			}
		}

/// <summary>
/// Return true if object has public read
/// </summary>

		public bool HasPublicRead
		{
			get
			{
				foreach (AclItem p in Items)
				{
					if (p.IsGroup && Lex.Eq(p.AssignedTo, "Public") &&
					 p.Permissions == PermissionEnum.Read)
						return true;
				}

				return false;
			}
		}

/// <summary>
/// Serialize
/// </summary>
/// <returns></returns>

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			mstw.Writer.Formatting = Formatting.Indented;
			Serialize(mstw.Writer);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("AccessControlList");
			foreach (AclItem p in Items)
			{
				tw.WriteStartElement("Item");
				tw.WriteAttributeString("GroupItem", p.IsGroup.ToString());
				tw.WriteAttributeString("AssignedTo", p.AssignedTo);
				tw.WriteAttributeString("Permissions", ((int)p.Permissions).ToString());
				tw.WriteEndElement();
			}
			tw.WriteEndElement(); // for ACL

			return;
		}

/// <summary>
/// Deserialize from a user object
/// </summary>
/// <param name="uo"></param>
/// <returns></returns>

		public static AccessControlList Deserialize(UserObject uo)
		{
			AccessControlList acl;

			if (uo == null || uo.AccessLevel == UserObjectAccess.None)
				return new AccessControlList();

			else if (uo.AccessLevel == UserObjectAccess.Private ||
				uo.AccessLevel == UserObjectAccess.Public)
			{
				acl = new AccessControlList();
				acl.AddReadWriteUserItem(uo.Owner);

				if (uo.AccessLevel == UserObjectAccess.Public)
					acl.AddPublicReadItem();

				return acl;
			}

			else if (uo.AccessLevel == UserObjectAccess.ACL)
			{
				return Deserialize(uo.ACL);
			}

			else throw new Exception("Unexpected AccessLevel: " + uo.AccessLevel);
		}

/// <summary>
/// Make the list public
/// </summary>
/// <param name="owner"></param>

		public void MakePublic(string owner)
		{
			Items.Clear();
			AddReadWriteUserItem(owner);
			AddPublicReadItem();
		}

/// <summary>
/// Make the list private
/// </summary>
/// <param name="owner"></param>

		public void MakePrivate(string owner)
		{
			Items.Clear();
			AddReadWriteUserItem(owner);
		}

/// <summary>
/// Add specied user with read/write privs
/// </summary>
/// <param name="userName"></param>
/// <returns></returns>

		public bool AddReadWriteUserItem(string userName)
		{
			foreach (AclItem item0 in Items)
			{
				if (Lex.Eq(item0.AssignedTo, userName))
				{
					item0.Permissions = PermissionEnum.Read | PermissionEnum.Write;
					return false;
				}
			}

			AclItem item = new AclItem();
			item.IsUser = true;
			item.AssignedTo = userName;
			item.Permissions = PermissionEnum.Read | PermissionEnum.Write;
			Items.Add(item);

			return true;
		}

/// <summary>
/// Add read access for user
/// </summary>
/// <param name="userName"></param>
/// <returns></returns>

		public bool AddReadUserItem(string userName)
		{
			foreach (AclItem item0 in Items)
			{
				if (Lex.Eq(item0.AssignedTo, userName))
				{
					item0.Permissions |= PermissionEnum.Read;
					return false;
				}
			}

			AclItem item = new AclItem();
			item.IsUser = true;
			item.AssignedTo = userName;
			item.Permissions = PermissionEnum.Read;
			Items.Add(item);

			return true;
		}

/// <summary>
/// Add a public read-only access item to the ACL
/// </summary>

		public bool AddPublicReadItem()
		{
			foreach (AclItem item0 in Items)
			{
				if (Lex.Eq(item0.AssignedTo, "Public"))
				{
					return false;
				}
			}

			AclItem item = new AclItem();
			item = new AclItem();
			item.IsGroup = true;
			item.AssignedTo = "Public";
			item.Permissions = PermissionEnum.Read;
			Items.Add(item);
			return true;
		}

/// <summary>
/// Remove a user item from the acl
/// </summary>
/// <param name="userName"></param>

		public void RemoveUserItem(string userName)
		{
			if (Lex.IsNullOrEmpty(userName)) return;

			int ii = 0;
			while (ii < Items.Count)
			{
				AclItem item = Items[ii];
				if (item.IsUser && Lex.Eq(item.AssignedTo, userName))
				{
					Items.Remove(item);
					return;
				}
				else ii++;
			}
		}

/// <summary>
/// Remove a user item from the acl
/// </summary>
/// <param name="groupName"></param>

		public void RemoveGroupItem(string groupName)
		{
			if (Lex.IsNullOrEmpty(groupName)) return;

			int ii = 0;
			while (ii < Items.Count)
			{
				AclItem item = Items[ii];
				if (item.IsGroup && Lex.Eq(item.AssignedTo, groupName))
				{
					Items.Remove(item);
					return;
				}
				else ii++;
			}
		}

/// <summary>
/// Deserialize
/// </summary>
/// <param name="serializedForm"></param>
/// <returns></returns>

		public static AccessControlList Deserialize(
			string serializedForm)
		{
			if (Lex.IsNullOrEmpty(serializedForm)) return new AccessControlList();

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get AccessControlList element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "AccessControlList"))
				throw new Exception("\"AccessControlList\" element not found");

			if (tr.IsEmptyElement) return new AccessControlList(); // if nothing there return empty rule list

			AccessControlList acl = Deserialize(mstr.Reader);
			mstr.Close();
			return acl;
		}

/// <summary>
/// Deserialize assuming the initial AccessControlList element has be read in already
/// </summary>
/// <param name="tr"></param>
/// <returns></returns>

		public static AccessControlList Deserialize(
			XmlTextReader tr)
		{
			string txt;
			int i1 = 0;

			AccessControlList acl = new AccessControlList();

			while (true) // loop on list of permissions
			{
				tr.Read(); // move to next permission
				tr.MoveToContent();

				if (tr.NodeType == XmlNodeType.EndElement) break; // end AccessControlList tag

				else if (Lex.Ne(tr.Name, "Item"))
					throw new Exception("Unexpected element: " + tr.Name);

				AclItem p = new AclItem();
				acl.Items.Add(p);

				XmlUtil.GetBoolAttribute(tr, "GroupItem", ref p.IsGroup);
				XmlUtil.GetStringAttribute(tr, "AssignedTo", ref p.AssignedTo);
				if (XmlUtil.GetIntAttribute(tr, "Permissions", ref i1))
					p.Permissions = (PermissionEnum)i1;

				if (!tr.IsEmptyElement)
				{
					tr.Read(); tr.MoveToContent();
					if (tr.NodeType != XmlNodeType.EndElement) throw new Exception("Expected EndElement");
				}
			}

			return acl;
		}

/// <summary>
/// Count number of readers
/// </summary>

		public int ReaderCount
		{
			get
			{
				int readerCount = 0;

				foreach (AclItem p in Items)
				{
					if (p.ReadIsAllowed && !Lex.IsNullOrEmpty(p.AssignedTo))
						readerCount++;
				}

				return readerCount;
			}
		}

/// <summary>
/// Count number of writers
/// </summary>

		public int WriterCount
		{
			get
			{
				int writerCount = 0;

				foreach (AclItem p in Items)
				{
					if (p.WriteIsAllowed && !Lex.IsNullOrEmpty(p.AssignedTo))
						writerCount++;
				}

				return writerCount;
			}
		}

/// <summary>
/// Return true if the user has read access via the ACL
/// </summary>
/// <param name="userName"></param>
/// <returns></returns>

		public bool UserHasReadAccess(string userName)
		{
			PermissionEnum permissions = GetUserPermissions(userName);
			if (Permissions.ReadIsAllowed(permissions)) return true;
			else return false;
		}

		/// <summary>
		/// Return true if the user has write access via the ACL
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public bool UserHasWriteAccess(string userName)
		{
			PermissionEnum permissions = GetUserPermissions(userName);
			if (Permissions.WriteIsAllowed(permissions)) return true;
			else return false;
		}

		/// <summary>
		/// Get the permissions that a user has
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public PermissionEnum GetUserPermissions(string userName)
		{
			PermissionEnum permissions = PermissionEnum.None;

			foreach (AclItem item in Items)
			{
				if (item.IsUser) // user name
				{
					if (Lex.Eq(item.AssignedTo, userName) || Lex.Eq(item.AssignedTo, "Public"))
						permissions |= item.Permissions; // or permissions together
				}

				else // group name
				{
					if (UserGroups.GroupContainsUser(item.AssignedTo, userName) || Lex.Eq(item.AssignedTo, "Public"))
						permissions |= item.Permissions; // or permissions together
				}
			}

			return permissions;
		}

/// <summary>
/// Sort the ACL first by group names then by individual names
/// </summary>

		public void Sort()
		{
			int i1, i2;
			string v1, v2;

			for (i1 = 2; i1 < Items.Count; i1++)
			{
				AclItem item1 = Items[i1];
				if (Items[i1].IsGroup) v1 = "G";
				else v1 = "U";
				v1 += Items[i1].GetExternalName();
				for (i2 = i1 - 1; i2 >= 0; i2--)
				{
					if (Items[i2].IsGroup) v2 = "G";
					else v2 = "U";
					v2 += Items[i2].GetExternalName();

					if (Lex.Le(v2, v1)) break;

					Items[i2 + 1] = Items[i2];
				}

				Items[i2 + 1] = item1;
			}
		}


		/// <summary>
		/// Acl for admin group (USERGROUP_307263), owner r/w access and users read access
		/// </summary>

		public static string GetAdministratorGroupRwPublicReadAcl (string ownerId)
		{
			string xml =
@"<AccessControlList>
  <Item GroupItem = 'True' AssignedTo='USERGROUP_307263' Permissions='3' />
  <Item GroupItem='False' AssignedTo='<ownerId>' Permissions='3' />
  <Item GroupItem = 'True' AssignedTo='Public' Permissions='1' />
</AccessControlList>";
			xml = xml.Replace('\'', '\"'); // replace single with double quotes
			xml = Lex.Replace(xml,"<ownerId>", ownerId);
			return xml;
		}

	}

/// <summary>
/// A single ACL user or group item and associated permissions
/// </summary>

	public class AclItem
	{
		public bool IsGroup = false;
		public bool IsUser { get { return !IsGroup; } set { IsGroup = !value; } }
		public bool IsPublic { get { return IsGroup && Lex.Eq(AssignedTo, "Public"); } }

		public string AssignedTo = ""; // group or user the permission is assigned to
		public PermissionEnum Permissions = PermissionEnum.None; // set of assigned permissions

/// <summary>
/// Get the external group or user name corresponding to an internal name
/// </summary>
/// <returns></returns>

		public string GetExternalName()
		{
			string name = AssignedTo;

			if (IsGroup)
			{
				UserGroup g = UserGroups.LookupInternalName(name);
				if (g != null) name = g.ExternalName;
			}

			else // user name
			{
				DictionaryMx userDict = DictionaryMx.Get("UserName");
				if (userDict != null && userDict.LookupDefinition(name) != null)
				{
					string userInfoString = userDict.LookupDefinition(name);
					UserInfo ui = UserInfo.Deserialize(userInfoString);
					name = ui.FullNameReversed;
				}
			}

			return name;
		}

/// <summary>
/// Return true if permissions include read
/// </summary>

		public bool ReadIsAllowed
		{
			get { return (Permissions & PermissionEnum.Read) != 0; }
		}
		
/// <summary>
/// Return true if permissions include write
/// </summary>

		public bool WriteIsAllowed
		{
			get { return (Permissions & PermissionEnum.Write) != 0; }
		}

	}

	/// <summary>
	/// Bitwise permissions
	/// </summary>

	public enum PermissionEnum
	{
		None = 0,
		Read = 1,
		Write = 2
	}

/// <summary>
/// The list of defined user groups
/// </summary>

	public class UserGroups
	{

/// <summary>
/// The list of known UserGroups
/// </summary>
		
		public static List<UserGroup> Items 
		{
			get 
			{
				if (_items != null) return _items;

				List<UserObject> uoList = InterfaceRefs.IUserObjectDao.ReadMultiple(UserObjectType.UserGroup, false);
				_items = new List<UserGroup>();
				foreach (UserObject uo in uoList)
				{
					try
					{
						UserGroup g = new UserGroup();
						g.UserObjectId = uo.Id;
						g.ExternalName = uo.Name;
						g.ACL = AccessControlList.Deserialize(uo.ACL); // the list of users in the group and their access level to the group
						_items.Add(g);
					}
					catch (Exception ex) { continue; }
				}
				return _items;
			}
		}
		static List<UserGroup> _items = null;

		static string UserGroupMembershipUserName = ""; // user that we have membership info for
		static Dictionary<string, UserGroup> UserGroupMembership = null; // membership info for the user

/// <summary>
/// Lookup a user group by the internal name, e.g. USERGROUP_12345
/// </summary>
/// <param name="internallName"></param>
/// <returns></returns>

		public static UserGroup LookupInternalName(
			string internalName)
		{
			foreach (UserGroup g in Items)
			{
				if (Lex.Eq(g.InternalName, internalName)) return g;
			}

			return null;
		}

/// <summary>
/// Lookup a user group by the external name
/// </summary>
/// <param name="externalName"></param>
/// <returns></returns>

		public static UserGroup LookupExternalName(
			string externalName)
		{
			foreach (UserGroup g in Items)
			{
				if (Lex.Eq(g.ExternalName, externalName)) return g;
			}

			return null;
		}

		/// <summary>
		/// Get a list of all groups
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static List<string> GetAllGroups()
		{
			List<string> groups = new List<string>();
			foreach (UserGroup group in UserGroups.Items)
				groups.Add(group.ExternalName);

			return groups;
		}

/// <summary>
/// Get a list of all of the groups that the specified user can edit
/// or all groups if userName is not defined
/// </summary>
/// <param name="userName"></param>
/// <returns></returns>

		public static List<string> GetEditableGroups(string userName)
		{
			List<string> groups = new List<string>();
			foreach (UserGroup group in UserGroups.Items)
			{
				foreach (AclItem item in group.ACL.Items)
				{
					if (Lex.Eq(item.AssignedTo, userName) && item.WriteIsAllowed)
					{
						groups.Add(group.ExternalName);
						break;
					}
				}
			}

			return groups;
		}

/// <summary>
/// Return true if the specified group contains the supplied user name
/// </summary>
/// <param name="internalGroupName"></param>
/// <param name="userName"></param>
/// <returns></returns>

		public static bool GroupContainsUser(
			string internalGroupName,
			string userName)
		{
			if (Lex.Ne(UserGroupMembershipUserName, userName) || UserGroupMembership == null)
			{
				UserGroupMembershipUserName = userName;
				UserGroupMembership = GetMemberGroups(userName);
			}

			if (UserGroupMembership.ContainsKey(internalGroupName)) return true;
			else return false;
		}

/// <summary>
/// Get dictionary of all groups that specified user belongs to keyed by internal group name
/// </summary>
/// <param name="userName"></param>
/// <returns></returns>

		public static Dictionary<string, UserGroup> GetMemberGroups(string userName)
		{
			Dictionary<string, UserGroup> dict = new Dictionary<string, UserGroup>();
			for (int gi = 0; gi < UserGroups.Items.Count; gi++)
			{
				UserGroup group = UserGroups.Items[gi];
				if (group == null || group.ACL == null || group.ACL.Items == null) continue;

				for (int ii = 0; ii < group.ACL.Items.Count; ii++)
				{
					AclItem item = group.ACL.Items[ii];
					if (Lex.Eq(item.AssignedTo, userName))
					{
						dict[group.InternalName] = group;
						break;
					}
				}
			}

			return dict;
		}

/// <summary>
/// Add or update a group in the in-memory collecion
/// </summary>
/// <param name="name"></param>
/// <param name="acl"></param>

		public static void UpdateInMemoryCollection(
			string name,
			AccessControlList acl)
		{
			UserGroup g = LookupExternalName(name);
			if (g == null)
			{
				g = new UserGroup();
				g.ExternalName = name;
				UserGroups.Items.Add(g);
			}
			g.ACL = acl;
			return;
		}
	}

/// <summary>
/// UserGroup
/// </summary>

	public class UserGroup
	{
		public string ExternalName = "";
		public string InternalName { get { return "USERGROUP_" + UserObjectId; } }
		public int UserObjectId = -1;
		public AccessControlList ACL; // group members and their access level

/// <summary>
/// ToString
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			return ExternalName;
		}
	}

}
