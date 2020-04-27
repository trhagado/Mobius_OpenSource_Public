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
	/// Summary description for Security.
	/// </summary>
	public class Security
	{
		public static UserInfo UserInfo
		{
			get { return ClientState.UserInfo; }
			set { ClientState.UserInfo = value; }
		}

		public static bool UseActiveDirectory { get { ReadSecurityParms(); return _useActiveDirectory; } }
		public static string ApprovedMobiusUsersGroupName { get { ReadSecurityParms(); return _approvedMobiusUsersGroupName; } }
		public static string ApprovedMobiusChemViewGroupName { get { ReadSecurityParms(); return _approvedMobiusChemViewGroupName; } }
		public static string ApprovedSequenceViewGroupName { get { ReadSecurityParms(); return _approvedMobiusSequenceViewGroupName; } }
		public static HashSet<string> SpecialMobiusSystemAccounts { get { ReadSecurityParms(); return _specialMobiusSystemAccounts; } }

		/// <summary>
		/// Constructor
		/// </summary>

		public Security()
		{
			return;
		}

		/// <summary>
		/// Get security parms
		/// </summary>
		/// <returns></returns>

		public static void ReadSecurityParms()
		{
			if (_readSecurityParms) return;
			if (ServicesIniFile.IniFile == null) return;

			_useActiveDirectory = ServicesIniFile.ReadBool("UseActiveDirectory", true);
			_approvedMobiusUsersGroupName = ServicesIniFile.Read("ApprovedMobiusUsers", @"<domain>Approved_Mobius_Users");
			_approvedMobiusChemViewGroupName = ServicesIniFile.Read("ApprovedMobiusChemView", @"<domain>Approved_Mobius_Chemview");
			_approvedMobiusSequenceViewGroupName = ServicesIniFile.Read("ApprovedMobiusSequenceView", @"<domain>Lg_Mol_Search_Incl_Seq");
			string txt = ServicesIniFile.Read("SpecialMobiusSystemAccounts", "MOBIUS");
			_specialMobiusSystemAccounts = new HashSet<string>();
			string[] sa = txt.Split(',');
			foreach (string s in sa)
			{
				if (Lex.IsDefined(s))
					_specialMobiusSystemAccounts.Add(s.Trim().ToUpper());
			}

			_readSecurityParms = true;
		}

		static bool _readSecurityParms = false;
		static bool _useActiveDirectory = false; // if true use ActiveDirectory instead of Mobius Oracle objecs for authorization
		static string _approvedMobiusUsersGroupName = ""; // name of group for authorized Mobius users with full access
		static string _approvedMobiusChemViewGroupName = ""; // name of group for authorized Mobius users that can see data other than structures
		private static string _approvedMobiusSequenceViewGroupName = ""; // name of group for authorized Mobius Users to see Sequences

		static HashSet<string> _specialMobiusSystemAccounts = new HashSet<string>(); // special system accounts, e.g. MOBIUS, MOBIUS_MAINT

		/// <summary>
		/// Current user name
		/// </summary>

		public static string UserName
		{
			get
			{
				if (UserInfo == null || UserInfo.UserName == null) return "";
				else return UserInfo.UserName;
			}
		}

		/// <summary>
		/// Current domain name
		/// </summary>

		public static string UserDomainName
		{
			get
			{
				if (UserInfo == null || UserInfo.UserDomainName == null) return "";
				else return UserInfo.UserDomainName;
			}
		}

		/// <summary>
		/// Authenticate user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="password"></param>
		/// <returns></returns>

		public static bool Authenticate(
			string userName,
			string password)
		{
			LDAPMembershipProvider lmp = new LDAPMembershipProvider();
			if (!lmp.ValidateUser(userName, password)) return false;

			UserInfo = new UserInfo();
			UserInfo.UserName = userName.ToUpper();

			return true;
		}

		/// <summary>
		/// See if user authorized & logon as current user if so
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <returns></returns>

		public static bool Logon(
			string userName,
			string domainName,
			string clientName)
		{
			UserInfo ui;

			if (!IsAuthorized(userName, domainName, out ui)) return false; // authorized for logon?
			UserInfo = ui;

			ClientState.ClientName = clientName;
			ClientState.MobiusClientVersion = Lex.ExtractVersion(clientName);

			DebugLog.UserName = userName.ToUpper(); // set username so it appears in log messages
			string msg = "User logged on: " + ui.FirstName + " " + ui.LastName + " (" + domainName + @"\" + userName + ")" +
				", CanRetrieveStructures: " + ui.Privileges.CanRetrieveStructures +
				", CanRetrieveSequences: " + ui.Privileges.CanRetrieveSequences +
				", Process: " + Process.GetCurrentProcess().Id +
				", Client: " + clientName;
			// + ", ClientVersion: " + ClientState.MobiusClientVersion.ToString();
			DebugLog.Message(msg);

			return true;
		}

		/// <summary>
		/// See if user is authorized
		/// </summary>
		/// <param name="userFullName"></param>
		/// <returns></returns>

		public static bool IsAuthorized(
			string userFullName)
		{
			UserInfo ui;
			string domain, userName;

			UserInfo.SplitDomainAndAccountName(userFullName, out domain, out userName);
			bool isAuthorized = IsAuthorized(userName, domain, out ui);
			return isAuthorized;
		}

		/// <summary>
		/// See if user authorized
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <returns></returns>

		public static bool IsAuthorized(
			string userName,
			string domainName,
			out UserInfo ui)
		{
			bool isAuthorized = false;

			if (Security.IsSpecialMobiusAccount(userName) ||  // see if special Mobius system account
				UAL.DbConnectionMx.NoDatabaseAccessIsAvailable) // or if we aren't accessing any real databases
			{
				ui = CreateDefaultMobiusAccountUserInfo(userName);
				isAuthorized = true;
			}

			else if (UseActiveDirectory) // ActiveDirectory is new method
				isAuthorized = ActiveDirectoryDao.IsAuthorizedAD(userName, domainName, out ui);

			else isAuthorized = IsAuthorizedOldMethod(userName, domainName, out ui);

			if (isAuthorized)
			{
				RestrictedDatabaseView v = RestrictedDatabaseView.GetRestrictedViewForUser(userName);
				if (v != null)
				{
					ui.RestrictedViewUsers = v.Userids;
					ui.RestrictedViewAllowedMetaTables = v.MetaTables; 
					ui.RestrictedViewAllowedCorpIds = v.CorpIds;
				}
                               
                ui.GenerallyRestrictedMetatables = RestrictedMetatable.GetUsersGenerallyRestrictedMetatables(userName, domainName);                
            }

			return isAuthorized;
		}

		/// <summary>
		/// Old authorization method
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <param name="ui"></param>
		/// <returns></returns>

		public static bool IsAuthorizedOldMethod(
			string userName,
			string domainName,
			out UserInfo ui)
		{
			ui = Security.ReadUserInfo(userName); // have Mobius account?
			if (ui == null) return false;

			if (!HasPrivilege(userName, "Logon")) return false; // logon allowed for user?

			if (!Lex.IsNullOrEmpty(domainName)) // have domain name we can match on?
			{
				if (!Lex.IsNullOrEmpty(ui.UserDomainName)) // do we have the user's domain info yet?
				{
					if (Lex.Ne(ui.UserDomainName, domainName))
						return false;
				}
				else // persist domain name
				{
					ui.UserDomainName = domainName;
					CreateUser(ui);
				}
			}

			ui.Privileges.CanRetrieveStructures = true;

			return true;
		}

		/// <summary>
		/// Change the session user to a new user. Includes authentication.
		/// </summary>

		public static void SetUser(
			string userName)
		{
			if (!Logon(userName, "", "SetUser Command"))
				throw new Exception("User " + userName + " is not authorized for Mobius access.");
		}

/// <summary>
/// Set privileges that are stored as privs in Mobius user parameters
/// </summary>
/// <param name="ui"></param>

		public static void SetUserParmPrivileges(UserInfo ui)
		{
			Dictionary<string, string> ups = UserObjectDao.GetUserParameters(ui.UserName);

			string prefix = "Privilege";
			foreach (string uoName in ups.Keys) // store other non-ad privileges
			{
				if (!Lex.StartsWith(uoName, prefix) || // Privilege user parm?
					Lex.Ne(ups[uoName], "True")) continue; // and granted?

				string privName = uoName.Substring(prefix.Length);
				if (!PrivilegesMx.IsValidPrivilegeName(privName)) continue;
				if (Lex.Eq(privName, "Logon")) continue; // ignore old Logon priv from database

				ui.Privileges.SetPrivilege(privName, true);
			}

			if (!ups.ContainsKey("NAMEADDRESS")) // store user name and address in Mobius db if not there already
				Security.CreateUser(ui);

			return;
		}

		/// <summary>
		/// Process a command line to grant a user a privilege
		/// </summary>
		/// <param name="commandLine"></param>et
		/// <returns></returns>

		public static string GrantPrivilege(
			string commandLine)
		{
			if (UserInfo == null || !IsAdministrator(UserInfo.UserName))
				return "You must be an administrator to grant privileges";

			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string priv = lex.Get();
			string userName = lex.Get();
			if (Lex.Eq(userName, "to"))
				userName = lex.Get();
			if (priv == "" || userName == "") return "Syntax: GRANT privilege TO userid";
			if (!PrivilegesMx.IsValidPrivilegeName(priv)) return priv + " is not a valid privilege";
			if (!UserExists(userName)) return "User " + userName + " doesn't exist";
			GrantPrivilege(userName, priv);
			return "Privilege granted";
		}

		/// <summary>
		/// Process command line to revoke a user privilege
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		public static string RevokePrivilege(
			string commandLine)
		{
			if (UserInfo == null || !IsAdministrator(UserInfo.UserName))
				return "You must be an administrator to revoke privileges";

			Lex lex = new Lex();
			lex.OpenString(commandLine);

			string priv = lex.Get();
			string userName = lex.Get();
			if (Lex.Eq(userName, "from"))
				userName = lex.Get();
			if (priv == "" || userName == "") return "Syntax: REVOKE privilege FROM userid";
			if (!PrivilegesMx.IsValidPrivilegeName(priv)) return priv + " is not a valid privilege";
			if (!UserExists(userName)) return "User " + userName + " doesn't exist";
			if (!HasPrivilege(userName, priv)) return "User " + userName + " doesn't have this privilege";
			RevokePrivilege(userName, priv);
			return "Privilege revoked";
		}

		/// <summary>
		/// Create a new user entry
		/// </summary>
		/// <param name="userInfo"></param>
		/// <returns></returns>

		public static void CreateUser(
			UserInfo userInfo)
		{
			string txt;

			if (userInfo == null || Lex.IsNullOrEmpty(userInfo.UserName))
				throw new Exception("User not defined");
			//			txt = UserObjectDao.GetUserParameter(userInfo.UserName,"NameAddress");
			//			if (txt!="") return false;
			txt =
				userInfo.FirstName + "|" +
				userInfo.MiddleInitial + "|" +
				userInfo.LastName + "|" +
				userInfo.EmailAddress + "|" +
				userInfo.Company + "|" +
				userInfo.Site + "|" +
				userInfo.Department + "|" +
				userInfo.UserDomainName + "|" +
				userInfo.UserName;

			UserObjectDao.SetUserParameter(userInfo.UserName, "NameAddress", txt);
			GrantPrivilege(userInfo.UserName, "Logon"); // authorize by default
			return;
		}

		/// <summary>
		/// Delete user
		/// </summary>
		/// <param name="userName"></param>

		public static bool DeleteUser(
			string userName)
		{
			try { RevokePrivilege(userName, "Logon"); }
			catch (Exception ex) { }

			return UserObjectDao.DeleteAllUserObjects(userName);
		}

		/// <summary>
		/// Get user info for a single user
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static UserInfo ReadUserInfo(
			string userName)
		{
			if (UseActiveDirectory) return ReadUserInfoFromActiveDirectory(userName);
			else return ReadUserInfoFromNameAddressUserObject(userName);
		}


		/// <summary>
		/// Get user info for a single user from Active Directory
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static UserInfo ReadUserInfoFromActiveDirectory(
			string userName)
		{
			UserInfo ui;
			string domainName;

			if (userName == null || userName == "") return null;

			UserInfo.SplitDomainAndAccountName(userName, out domainName, out userName);

			bool isAuthorized = IsAuthorized(userName, domainName, out ui);

			return ui;
		}

		/// <summary>
		/// Return true if special Mobius account
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool IsSpecialMobiusAccount(string userName)
		{
			bool isSpecial = SpecialMobiusSystemAccounts.Contains(userName.Trim().ToUpper());
			return isSpecial;
		}

		/// <summary>
		/// Get user info for a single user from user object table
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static UserInfo ReadUserInfoFromNameAddressUserObject(
			string userName)
		{
			UserInfo ui;

			if (userName == null || userName == "") return null;
			string txt = UserObjectDao.GetUserParameter(userName, "NameAddress");
			if (txt == null || txt == "")
			{
				if (IsSpecialMobiusAccount(userName))
					return CreateDefaultMobiusAccountUserInfo(userName);
				else return null;
			}

			ui = UserInfo.Deserialize(txt);
			if (ui.UserName == "") ui.UserName = userName; // plug in user name in case not deserialized
			return ui;
		}

		/// <summary>
		/// CreateDefaultMobiusAccountUserInfo
		/// </summary>
		/// <returns></returns>

		public static UserInfo CreateDefaultMobiusAccountUserInfo(string userName)
		{
			UserInfo ui = new UserInfo();
			ui.UserName = userName.ToUpper();

			// Set privs that normally come from Active Directory

			ui.Privileges.CanLogon = true;
			ui.Privileges.CanRetrieveStructures = true;
			ui.Privileges.CanRetrieveSequences = true;

			// Set other non AD privs

			Security.SetUserParmPrivileges(ui); 

			return ui;
		}

		/// <summary>
		/// Get user info for all users
		/// </summary>
		/// <returns></returns>

		public static ArrayList GetAllUserInfo()
		{
			List<UserObject> al = UserObjectDao.ReadMultiple(UserObjectType.UserParameter, "NameAddress", false, false);
			ArrayList al2 = new ArrayList();
			for (int i1 = 0; i1 < al.Count; i1++)
			{
				UserObject uo = al[i1];
				UserInfo ui = UserInfo.Deserialize(uo.Description);
				ui.UserName = uo.Owner;
				al2.Add(ui);
			}

			return al2;
		}

		/// <summary>
		/// See if a user exists
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool UserExists(
			string userName)
		{
			if (ReadUserInfo(userName) != null) return true;
			else return false;
		}

		/// <summary>
		/// See if current user is an administrator
		/// </summary>
		/// <returns></returns>
		public static bool IsAdministrator()
		{
			if (UserInfo == null) return false;
			return IsAdministrator(UserInfo.UserName);
		}

		/// <summary>
		/// Get the full person name, "John Q. Public"
		/// </summary>
		/// <returns>Full person name</returns>

		public static string GetPersonName(
			string userName)
		{
			UserInfo ui = GetUserInfo(userName);
			if (ui == null) return userName;

			if (ui.LastName == "" && ui.FirstName == "")
				throw new Exception("No full name for user: " + userName);
			string name = ui.LastName + ", " + ui.FirstName;
			if (ui.MiddleInitial != "") name += " " + ui.MiddleInitial;
			return name;
		}

		/// <summary>
		/// Get short person name, e.g. "Smith, J."
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string GetShortPersonName(string userName)
		{
			UserInfo ui = GetUserInfo(userName);
			if (ui == null) return userName;

			if (ui.LastName == "" && ui.FirstName == "")
				throw new Exception("No full name for user: " + userName);
			string name = ui.LastName;

			if (ui.FirstName.Length > 0)
				name += ", " + ui.FirstName.Substring(0, 1) + ".";

			return name;
		}

		/// <summary>
		/// Get short person name, e.g. "J. Smith"
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string GetShortPersonNameWithInitialFirst(string userName)
		{
			UserInfo ui = GetUserInfo(userName);
			if (ui == null) return userName;

			string name = "";
			if (ui.FirstName.Length > 0) name = ui.FirstName.Substring(0, 1) + ". ";
			name += ui.LastName;

			return name;
		}

		/// <summary>
		/// Get user information for a user 
		/// Note: Will not get email address (avoids slowdown)
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>

		public static UserInfo GetUserInfo(
			string userName)
		{
			UserInfo ui;

			if (UserInfo != null && Lex.Eq(userName, UserInfo.UserName))
			{ // if current user then return complete information including privileges
				ui = UserInfo.Clone();
				return ui;
			}

			string uInf = DictionaryMx.LookupDefinition("UserName", userName); // lookup serialized user info in "UserName" dictionary
			if (uInf == null) return null; // just return null if not found
			UserInfo sui = UserInfo.Deserialize(uInf);
			return sui;
		}

		/// <summary>
		/// See if the user is the Mobius system account
		/// </summary>

		public static bool IsMobius
		{ get { return Lex.Eq(UserName, "Mobius"); } }

		/// <summary>
		/// See if a user is an administrator
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool IsAdministrator(
			string userName = null)
		{
			if (Lex.IsUndefined(userName))
				userName = UserName;

			return HasPrivilege(userName, "Administrator");
		}

		/// <summary>
		/// See if a user is a developer
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool IsDeveloper(
			string userName = null)
		{
			if (Lex.IsUndefined(userName))
				userName = UserName;

			return HasPrivilege(userName, "Developer");
		}

		/// <summary>
		/// Grant a privilege to a user
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="privilege"></param>
		/// <returns></returns>

		public static void GrantPrivilege(
			string userName,
			string privilege)
		{
			if (!PrivilegesMx.IsValidPrivilegeName(privilege)) throw new Exception("Not a valid privilege");
			UserObjectDao.SetUserParameter(userName, "Privilege" + Lex.CapitalizeFirstLetters(privilege), "True"); // authorize by default
			return;
		}

		/// <summary>
		/// Revoke a privilege from a user
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="privilege"></param>
		/// <returns></returns>

		public static void RevokePrivilege(
			string userName,
			string privilege)
		{
			if (!HasPrivilege(userName, privilege)) throw new Exception("User doesn't have privilege");
			if (!PrivilegesMx.IsValidPrivilegeName(privilege)) throw new Exception("Not a valid privilege");
			UserObjectDao.SetUserParameter(userName, "Privilege" + privilege, "False");
			return;
		}

		/// <summary>
		/// See if a user has a given privilege
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="privilege"></param>
		/// <returns></returns>

		public static bool HasPrivilege(
			string userName,
			string privilege)
		{
			if (Lex.IsUndefined(userName) || Lex.IsUndefined(privilege)) return false;

			if (IsSpecialMobiusAccount(userName)) return true; // full authority for this account

			string txt = UserObjectDao.GetUserParameter(userName, "Privilege" + privilege);
			if (Lex.Eq(txt, "true")) return true;
			else return false;
		}

		/// <summary>
		/// Get email address for a user
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string GetUserEmailAddress(
			string userName)
		{
			string emailAddr = UserObjectDao.GetUserParameter(userName, "EmailAddress");
			return emailAddr;
		}

		/// <summary>
		/// Get list of ACL roles
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>

		public static string[] GetACLRoles(string username)
		{
			string[] roles = null;
			try
			{
				DbCommandMx drd = new DbCommandMx();
				drd.PrepareMultipleParameter("SELECT acl_api_pkg.acl_get_user_roles_fnc(:0) FROM DUAL", 1);
				DbDataReader odr = drd.ExecuteReader(username.ToUpper());
				string response = null;
				if (odr.Read())
				{
					response = odr.GetString(0);
				}
				drd.CloseReader();
				drd.Dispose();
				if (response != null && response.StartsWith("OK") && response.IndexOf("|") > 0)
				{
					roles = response.Substring(response.IndexOf("|") + 1).Split(new char[] { ' ', ';' });
				}
			}
			catch (Exception ex)
			{
				//do nothing
			}
			return roles;
		}

		/// <summary>
		/// Be sure that the specified internal object matches the read permissions of the supplied user object
		/// </summary>
		/// <param name="uoId"></param>
		/// <param name="acl"></param>
		/// <param name="internalObjectNameToMatch"></param>
		/// <returns></returns>

		public static bool AssignMatchingReadAccess(
			int uoId,
			AccessControlList acl,
			string internalObjectNameToMatch)
		{
			AccessControlList acl2;
			UserObjectType uoType;
			int uoId2;

			if (!acl.IsShared) return false; // source object not shared

			string objName = internalObjectNameToMatch;

			if (!UserObject.ParseObjectTypeAndIdFromInternalName(objName, out uoType, out uoId2))
			{
				DebugLog.Message("Can't parse object name: " + objName);
				return false;
			}

			UserObject uo2 = UserObjectDao.ReadHeader(uoId2);

			if (uo2 == null)
			{
				DebugLog.Message("Can't read object: " + internalObjectNameToMatch + ", " + uoId2);
				return false;
			}

			acl2 = AccessControlList.Deserialize(uo2);

			if (acl.IsPublic)
			{
				if (acl2.IsPublic) return false;
			}

			// check if list of readers matches

			bool modified = false;
			foreach (AclItem item in acl.Items)
			{
				if (!item.ReadIsAllowed) continue;
				PermissionEnum permissions = acl2.GetUserPermissions(item.AssignedTo); // get permissions for corresponding user/group for obj2
				if (!Permissions.ReadIsAllowed(permissions)) // add read if doesn't have
				{
					if (item.IsPublic) acl2.AddPublicReadItem();
					else acl2.AddReadUserItem(item.AssignedTo);
					modified = true;
				}

			}

			if (modified) // update in db if modified
			{
				DebugLog.Message("AssignMatchingReadAccess: " + uoId + ", " + objName); // debug

				acl2.Serialize(uo2);
				UserObjectDao.UpdateHeader(uo2, false, false);
				return true;
			}

			else return false;
		}

	}
}

