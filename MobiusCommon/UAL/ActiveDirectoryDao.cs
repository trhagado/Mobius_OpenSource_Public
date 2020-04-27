using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Data;
using System.Text;
using System.IO;
using System.Data.Common;

namespace Mobius.UAL
{

	/// <summary>
	/// Methods for accessing Active Directory
	/// </summary>

	public class ActiveDirectoryDao
	{

		// Terminology:
		//
		// Distinguished Name (DN) - e.g. CN=user, OU=USERS, DC=contoso, DC=com
		// Common Name (DbMetaDataCollectionNames)
		// Organizational Unit (OU)
		// Domain Component  (DC)
		// FriendlyDomainName: the non qualified domain name (contoso - NOT contoso.com)
		// LdapDomain: the fully qualified domain such as contoso.com or dc=contoso,dc=com
		// ObjectPath: the fully qualified path to the object: CN=user, OU=USERS, DC=contoso, DC=com(same as objectDn)
		// ObjectDn: the distinguishedName of the object: CN=group, OU=GROUPS, DC=contoso, DC=com
		// UserDn: the distinguishedName of the user: CN=user, OU=USERS, DC=contoso, DC=com
		// GroupDn: the distinguishedName of the group: CN=group,OU=GROUPS,DC=contoso,DC=com

		static bool Debug
		{
			get
			{
				if (!_readDebugParameter)
				{
					if (ServicesIniFile.IniFile != null)
						_debug = ServicesIniFile.ReadBool("DebugActiveDirectoryDao", false);
					_readDebugParameter = true;
				}

				return _debug;
			}
		}
		static bool _debug = false;
		static bool _readDebugParameter = false;

		/// <summary>
		/// Translate the Friendly Domain Name to Fully Qualified Domain
		/// </summary>
		/// <param name="friendlyDomainName"></param>
		/// <returns></returns>

		public static string FriendlyDomainToLdapDomain(string friendlyDomainName)
		{
			string ldapPath = null;
			try
			{
				DirectoryContext objContext = new DirectoryContext(
								DirectoryContextType.Domain, friendlyDomainName);
				Domain objDomain = Domain.GetDomain(objContext);
				ldapPath = objDomain.Name;
			}
			catch (DirectoryServicesCOMException e)
			{
				ldapPath = e.Message.ToString();
			}
			return ldapPath;
		}

		/// <summary>
		/// Enumerate Domains in the Current Forest
		/// </summary>
		/// <returns></returns>

		public static ArrayList EnumerateDomains()
		{
			ArrayList alDomains = new ArrayList();
			Forest currentForest = Forest.GetCurrentForest();
			DomainCollection myDomains = currentForest.Domains;

			foreach (Domain objDomain in myDomains)
			{
				alDomains.Add(objDomain.Name);
			}
			return alDomains;
		}

		/// <summary>
		/// Enumerate Global Catalogs in the Current Forest
		/// </summary>
		/// <returns></returns>

		public static ArrayList EnumerateCatalogs()
		{
			ArrayList alGCs = new ArrayList();
			Forest currentForest = Forest.GetCurrentForest();
			foreach (GlobalCatalog gc in currentForest.GlobalCatalogs)
			{
				alGCs.Add(gc.Name);
			}
			return alGCs;
		}

		/// <summary>
		/// Enumerate Domain Controllers in a Domain
		/// </summary>
		/// <returns></returns>

		public static ArrayList EnumerateDomainControllers()
		{
			ArrayList alDcs = new ArrayList();
			Domain domain = Domain.GetCurrentDomain();
			foreach (DomainController dc in domain.DomainControllers)
			{
				alDcs.Add(dc.Name);
			}
			return alDcs;
		}

		/// <summary>
		/// Enumerate Objects in an OU
		/// </summary>
		/// <param name="OuDn">Organizational Unit distinguishedName such as OU=Users,dc=myDomain,dc=com</param>
		/// <returns></returns>

		public ArrayList EnumerateOU(string OuDn)
		{
			ArrayList alObjects = new ArrayList();
			try
			{
				DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + OuDn);
				foreach (DirectoryEntry child in directoryObject.Children)
				{
					string childPath = child.Path.ToString();
					alObjects.Add(childPath.Remove(0, 7));
					//remove the LDAP prefix from the path

					child.Close();
					child.Dispose();
				}
				directoryObject.Close();
				directoryObject.Dispose();
			}
			catch (DirectoryServicesCOMException e)
			{
				Console.WriteLine("An Error Occurred: " + e.Message.ToString());
			}
			return alObjects;
		}

		/// <summary>
		/// Check for the Existence of an Object
		/// This method does not need you to know the distinguishedName, you can concat strings or 
		/// even guess a location and it will still run (and return false if not found).
		/// </summary>
		/// <param name="objectPath"></param>
		/// <returns></returns>

		public static bool DirectoryEntryExists(string objectPath)
		{
			bool found = false;
			if (DirectoryEntry.Exists("LDAP://" + objectPath))
			{
				found = true;
			}
			return found;
		}

		/// <summary>
		/// Get Group Members
		/// </summary>
		/// <param name="groupDn"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>

		public static ArrayList GetGroupMembers(string userDn, bool recursive)
		{
			ArrayList groupMemberships = new ArrayList();
			return AttributeValuesMultiString("member", userDn,
							groupMemberships, recursive);
		}

		/// <summary>
		/// Get User Group Memberships for user
		/// </summary>
		/// <param name="userDn"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>

		public static ArrayList GetGroupsUserIsMemberOf(string userDn, bool recursive)
		{
			ArrayList groupMemberships = new ArrayList();
			return AttributeValuesMultiString("memberOf", userDn,
							groupMemberships, recursive);
		}

		/// <summary>
		/// Get User Group Commons Names for user
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>

		public static List<string> GetGroupCommonNamesUserIsMemberOf(string userName, string domainName, bool recursive)
		{
			string userDn = GetDistinguishedName(userName, domainName);
			ArrayList userGroups = GetGroupsUserIsMemberOf(userDn, recursive);
			List<string> userGroupCommonNames = new List<string>();

			foreach (string userGroup in userGroups)
			{
				if (userGroup != null && userGroup.Contains(",") && userGroup.StartsWith("CN="))
					userGroupCommonNames.Add(userGroup.Substring(3, userGroup.IndexOf(",") - 3).ToUpper());
			}

			return userGroupCommonNames;
		}

		/// <summary>
		/// Enumerate Multi-String Attribute Values of an Object
		/// This method includes a recursive flag in case you want to recursively dig up properties of properties such as 
		/// enumerating all the member values of a group and then getting each member group's groups all the way up the tree.
		/// </summary>
		/// <param name="attributeName"></param>
		/// <param name="objectDn"></param>
		/// <param name="valuesCollection"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>

		public static ArrayList AttributeValuesMultiString(
				string attributeName,
		 string objectDn,
				ArrayList valuesCollection,
				bool recursive)
		{
			string path = "LDAP://" + objectDn;
			DirectoryEntry ent = new DirectoryEntry(path);
			PropertyValueCollection ValueCollection = ent.Properties[attributeName];

			//string[] propNames = new string[ent.Properties.PropertyNames.Count];
			//ent.Properties.PropertyNames.CopyTo(propNames, 0);

			//object[] propValues = new object[ent.Properties.Values.Count];
			//ent.Properties.Values.CopyTo(propValues, 0);

			IEnumerator en = ValueCollection.GetEnumerator(); // note: limited to 1500 entries

			while (en.MoveNext())
			{
				if (en.Current != null)
				{
					if (!valuesCollection.Contains(en.Current.ToString()))
					{
						valuesCollection.Add(en.Current.ToString());
						if (recursive)
						{
							AttributeValuesMultiString(attributeName, "LDAP://" +
							en.Current.ToString(), valuesCollection, true);
						}
					}
				}
			}
			ent.Close();
			ent.Dispose();
			return valuesCollection;
		}

		/// <summary>
		/// Check in Active Directory to see if user is authorized
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <param name="ui"></param>
		/// <returns></returns>

		public static bool IsAuthorizedAD(
				string userName,
				string domainName,
				out UserInfo ui)
		{
			ui = null;

			try
			{
				bool isauthorized = IsAuthorizedADWithException(userName, domainName, out ui);
				return isauthorized;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Check in Active Directory to see if user is authorized (allowing exceptions)
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <returns></returns>

		public static bool IsAuthorizedADWithException(
				string userName,
				string domainName,
				out UserInfo ui)
		{
			GroupPrincipal gp;
			UserPrincipal up;
			PrincipalContext pc;
			string domainAndUserName = "";

			ui = new UserInfo();

			ui.Privileges.CanLogon = false; // be sure false by default
			ui.Privileges.CanRetrieveStructures = false;
			ui.Privileges.CanRetrieveSequences = false;

			ui.UserName = userName;
			if (!Lex.IsNullOrEmpty(ui.UserDomainName))
				ui.UserDomainName = domainName;

			if (!Lex.IsNullOrEmpty(domainName))
				domainAndUserName = domainName + @"\" + userName;
			else domainAndUserName = userName;

			// Get user identity information (UserPrincipal)

			DateTime t0 = DateTime.Now;

			up = GetUserPrincipal(ref domainName, userName);
			if (up == null)
			{
				DebugLog.Message("UserPrincipal not found: " + domainAndUserName);
				return false;
			}

			string dn = GetDistinguishedName(userName, domainName);
			if (dn == null)
			{
				DebugLog.Message("User not found by GetDistinguishedName: " + domainAndUserName);
				return false;
			}

			if (!Lex.IsNullOrEmpty(domainName))
			{
				domainAndUserName = domainName + @"\" + userName;
				ui.UserDomainName = domainName;
			}
			else domainAndUserName = userName;

			if (Debug) DebugLog.Message("UserPrincipal.FindByIdentity (" + domainAndUserName + ") time: " + (int)TimeOfDay.Delta(ref t0));

			// Check which groups the user is in

			bool isAuthorized = UserIsInGroup(dn, Security.ApprovedMobiusUsersGroupName);
			if (isAuthorized)
			{
				bool canRetrieveStructures = UserIsInGroup(dn, Security.ApprovedMobiusChemViewGroupName);
				ui.Privileges.CanRetrieveStructures = canRetrieveStructures;

				bool canRetrieveSequences = UserIsInGroup(dn, Security.ApprovedSequenceViewGroupName);
				ui.Privileges.CanRetrieveSequences = canRetrieveSequences;
			}

			else
			{
				DebugLog.Message("User not authorized for Mobius access: " + domainAndUserName);
				return false;
			}

			// Fill in rest of user information

			ui.Privileges.CanLogon = true;

			ui.UserName = userName.ToUpper();
			ui.UserDomainName = domainName.ToUpper();

			ui.FirstName = up.GivenName;
			if (!Lex.IsNullOrEmpty(up.MiddleName))
				ui.MiddleInitial = up.MiddleName[0].ToString();
			ui.LastName = up.Surname;
			ui.EmailAddress = up.EmailAddress;

			Security.SetUserParmPrivileges(ui);

			return true;
		}

		/// <summary>
		/// Get UserPrincipal
		/// </summary>
		/// <param name="domainName"></param>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static UserPrincipal GetUserPrincipal(
						ref string domainName,
						string userName)
		{
			PrincipalContext pc = null;
			UserPrincipal up = null;
			Exception ex1 = null, ex2 = null;

			if (!Lex.IsNullOrEmpty(domainName))
			{

				// Try to find by search of multiple user properties

				// Workaround - .net 4.5 Active Directory Bug
				try
				{
					//Original code
					//up = UserPrincipal.FindByIdentity(pc, userName);
					//DebugLog.Message("domainName: " + domainName);
					//DebugLog.Message("pc.name:    " + pc.Name);
					//DebugLog.Message("userName:   " + userName);

					// Second Attempt
					//UserPrincipal userPrincipalForQueryFilter = new UserPrincipal(pc) {SamAccountName = userName};
					//PrincipalSearcher principalSearcher = new PrincipalSearcher {QueryFilter = userPrincipalForQueryFilter};
					//up = principalSearcher.FindOne() as UserPrincipal;

					// Third AttemptR
					//// get a DirectorySearcher object
					var currentForest = Forest.GetCurrentForest();
					var globalCatalog = currentForest.FindGlobalCatalog();

					DirectorySearcher search = globalCatalog.GetDirectorySearcher();

					//// specify the search filter
					search.Filter = "(&(objectClass=user)(anr=" + userName + "))";

					//// specify which property values to return in the search
					search.PropertiesToLoad.Add("givenName");       // first name
					search.PropertiesToLoad.Add("sn");              // last name
					search.PropertiesToLoad.Add("mail");            // smtp mail address
					search.PropertiesToLoad.Add("samaccountname");  // smtp mail address
					search.PropertiesToLoad.Add("distinguishedname");

					//// perform the search
					SearchResult result = search.FindOne();

					pc = new PrincipalContext(ContextType.Domain, domainName);
					up = new UserPrincipal(pc);
					up.GivenName = (string)result.Properties["givenName"][0];
					up.SamAccountName = (string)result.Properties["samaccountname"][0];
					up.Surname = (string)result.Properties["sn"][0];
					up.EmailAddress = (string)result.Properties["mail"][0];
					return up;
				}

				catch (Exception ex)
				{
					string msg = "Failed 1st attempt (FindOne) to get UserPrincipal for user: " + domainName + @"\" + userName;
					DebugLog.Message(msg);
					//DebugLog.Message(DebugLog.FormatExceptionMessage(ex, msg));
				}

				// Try to find by Identity

				try
				{
					pc = new PrincipalContext(ContextType.Domain, domainName);
					up = UserPrincipal.FindByIdentity(pc, userName);
					return up;
				}

				catch (Exception ex)
				{
					string msg = "Failed 2nd attempt (FindByIdentity) to get UserPrincipal for user: " + domainName + @"\" + userName;
					DebugLog.Message(DebugLog.FormatExceptionMessage(ex, msg));
					throw new Exception(msg, ex);
				}

				//DebugLog.Message("Global Catalog GetDirectorySearcher first attempt due to .NET 4.5 Bug");
				//DebugLog.Message("FindByIdentity suceeded second attempt");

			}

			else // workaround - check all domains
			{
				//The operational domains that represent the Zones at Corp are:
				//  Zone 1: (United States + Puerto Rico)
				//          (Central and South America)
				//          (North America - Canada)
				//  Zone 2:	(Europe, Middle East and Africa)
				//  Zone 3:	Asia Pacific (Russia, China, Japan, Korea, India, Australia, etc...)

				string[] domains = { "ZD1", "ZD2", "ZD3" };

				//if (Debug) DebugLog.Message("Domain not defined, checking...");

				foreach (string d in domains)
				{
					domainName = d;
					up = GetUserPrincipal(ref domainName, userName);
					if (up != null)
					{
						//if (Debug) DebugLog.Message("Domain found for user: " + domainName + @"\" + userName);
						return up;
					}
				}

				domainName = "";
			}
			return null;
		}

		/// <summary>
		/// Get GroupPrincipal for security group
		/// </summary>
		/// <param name="groupFullName"></param>
		/// <returns></returns>

		public static GroupPrincipal GetSecurityGroupPrincipal(string groupFullName)
		{
			PrincipalContext ctxGroup, ctxUser;
			string groupDomain, groupUserName;
			GroupPrincipal gp;

			if (UserInfo.SplitDomainAndAccountName(groupFullName, out groupDomain, out groupUserName))
				ctxGroup = new PrincipalContext(ContextType.Domain, groupDomain);
			else ctxGroup = new PrincipalContext(ContextType.Domain); // assume current domain

			//GroupPrincipal gp = GroupPrincipal.FindByIdentity(ctxGroup, groupFullName);
			// Workaround - .net 4.5 Active Directory Bug
			try
			{
				gp = GroupPrincipal.FindByIdentity(ctxGroup, groupFullName);
			}
			catch (Exception e)
			{
				DebugLog.Message("FindByIdentity failed first attempt due to .NET 4.5 Bug");
				gp = GroupPrincipal.FindByIdentity(ctxGroup, groupFullName);
				DebugLog.Message("FindByIdentity suceeded second attempt");
			}

			return gp;
		}

		/// <summary>
		/// Determines whether or not the specified user is a member of the group.
		/// Based on Article: "Using LDAP to Enumerate Large Groups in UWWI"
		/// </summary>
		/// <param name="userDistinguishedName">A System.String containing the user's distinguished name (DN).</param>
		/// <param name="groupFullName">Domain-qualified group name.</param>

		static bool UserIsInGroup(
				string userDistinguishedName,
				string groupFullName)
		{
			SearchResult searchResults = null;
			Boolean userFound = false;
			Boolean isLastQuery = false;
			Boolean exitLoop = false;
			Int32 rangeStep = 1500;
			Int32 rangeLow = 0;
			Int32 rangeHigh = rangeLow + (rangeStep - 1);
			String attributeWithRange;
			int rangeMemberCount = 0;

			DateTime t0 = DateTime.Now;

			GroupPrincipal gp = GetSecurityGroupPrincipal(groupFullName);
			if (gp == null) throw new Exception("Security Group not found: " + groupFullName); // shouldn't happen
			if (Debug) DebugLog.Message("GetSecurityGroupPrincipal " + groupFullName + ": " + (int)TimeOfDay.Delta(ref t0));

			string path = "LDAP://" + gp.DistinguishedName;
			DirectoryEntry groupDe = new DirectoryEntry(path);
			DirectorySearcher groupSearch = new DirectorySearcher(groupDe);
			groupSearch.Filter = "(objectClass=*)";

			do
			{
				if (!isLastQuery)
					attributeWithRange = String.Format("member;range={0}-{1}", rangeLow, rangeHigh);
				else
					attributeWithRange = String.Format("member;range={0}-*", rangeLow);

				groupSearch.PropertiesToLoad.Clear();
				groupSearch.PropertiesToLoad.Add(attributeWithRange);

				searchResults = groupSearch.FindOne();
				groupSearch.Dispose();

				if (searchResults.Properties.Contains(attributeWithRange))
				{
					rangeMemberCount = searchResults.Properties.Count;

					if (searchResults.Properties[attributeWithRange].Contains(userDistinguishedName))
						userFound = true;

					if (isLastQuery)
						exitLoop = true;
				}
				else
				{
					isLastQuery = true;
				}

				if (!isLastQuery)
				{
					rangeLow = rangeHigh + 1;
					rangeHigh = rangeLow + (rangeStep - 1);
				}
			}
			while (!(exitLoop | userFound));

			if (Debug) DebugLog.Message("UserIsInGroup(" + userDistinguishedName + ", " + groupFullName + "): " + (int)TimeOfDay.Delta(ref t0));

			return userFound;
		}

		/// <summary>
		/// Get members in specified group
		/// </summary>
		/// <param name="groupFullName"></param>
		/// <returns></returns>

		public static List<string> GetGroupMembers(
				string groupFullName)
		{
			SearchResult searchResults = null;
			Boolean isLastQuery = false;
			Boolean exitLoop = false;
			Int32 rangeStep = 1500;
			Int32 rangeLow = 0;
			Int32 rangeHigh = rangeLow + (rangeStep - 1);
			String attributeWithRange;

			DateTime t0 = DateTime.Now;

			GroupPrincipal gp = GetSecurityGroupPrincipal(groupFullName);
			if (gp == null) throw new Exception("Security Group not found: " + groupFullName); // shouldn't happen
			if (Debug) DebugLog.Message("GetSecurityGroupPrincipal " + groupFullName + ": " + (int)TimeOfDay.Delta(ref t0));

			string path = "LDAP://" + gp.DistinguishedName;
			DirectoryEntry groupDe = new DirectoryEntry(path);
			DirectorySearcher groupSearch = new DirectorySearcher(groupDe);
			groupSearch.Filter = "(objectClass=*)";

			int memberCount = 0;
			List<string> members = new List<string>();

			do
			{
				if (!isLastQuery)
					attributeWithRange = String.Format("member;range={0}-{1}", rangeLow, rangeHigh);
				else
					attributeWithRange = String.Format("member;range={0}-*", rangeLow);

				groupSearch.PropertiesToLoad.Clear();
				groupSearch.PropertiesToLoad.Add(attributeWithRange);

				searchResults = groupSearch.FindOne();
				groupSearch.Dispose();

				if (searchResults.Properties.Contains(attributeWithRange))
				{
					memberCount += searchResults.Properties.Count;
					ResultPropertyValueCollection p = searchResults.Properties[attributeWithRange];
					IEnumerator en = p.GetEnumerator();

					while (en.MoveNext())
					{
						if (en.Current != null)
						{
							string valString = en.Current.ToString();
							members.Add(valString);
						}
					}

					if (isLastQuery)
						exitLoop = true;
				}

				else
				{
					isLastQuery = true;
				}

				if (!isLastQuery)
				{
					rangeLow = rangeHigh + 1;
					rangeHigh = rangeLow + (rangeStep - 1);
				}
			}
			while (!exitLoop);

			//StringBuilder sb = new StringBuilder();
			//foreach (object o in members)
			//{
			//  if (sb.Length > 0) sb.Append("\r\n");
			//  sb.Append(o.ToString());
			//}

			return members;
		}

		/// <summary>
		/// UserIsInGroup
		/// </summary>
		/// <param name="userDistinguishedName"></param>
		/// <param name="groupFullName"></param>
		/// <returns></returns>

		public static bool UserIsInGroup2(
		string userDistinguishedName,
		string groupFullName)
		{
			DateTime t0 = DateTime.Now;

			GroupPrincipal gp = GetSecurityGroupPrincipal(groupFullName);
			gp = GetSecurityGroupPrincipal(groupFullName);
			if (gp == null) throw new Exception("Security Group not found: " + groupFullName); // shouldn't happen
			if (Debug) DebugLog.Message("GetSecurityGroupPrincipal " + groupFullName + ": " + (int)TimeOfDay.Delta(ref t0));

			string groupDn = gp.DistinguishedName;
			ArrayList members = GetGroupMembers(groupDn, false);
			if (members == null) return false;

			bool inGroup = false;
			foreach (string s in members)
			{
				if (Lex.Eq(s, userDistinguishedName))
				{
					inGroup = true;
					break;
				}
			}

			if (Debug) DebugLog.Message("UserIsInGroup(" + userDistinguishedName + ", " + groupFullName + "): " + (int)TimeOfDay.Delta(ref t0));

			return inGroup;
		}

		/// <summary>
		/// List all members in group (slow)
		/// </summary>
		/// <param name="groupFullName"></param>

		public static void ListMembers(string groupFullName)
		{
			GroupPrincipal gp;
			PrincipalContext pc;

			StringBuilder sb = new StringBuilder();
			gp = GetSecurityGroupPrincipal(groupFullName);
			if (gp == null) throw new Exception("Security Group not found: " + groupFullName);
			int count = 0;
			foreach (Principal p in gp.Members)
			{
				sb.Append(p.UserPrincipalName);
				sb.Append(", ");
				sb.Append(p.DisplayName);
				sb.Append("\n");
				count++;
			}

			string s = sb.ToString();
			return;
		}

		/// <summary>
		/// Check list of users
		/// </summary>

		public static void CheckList()
		{
			UserInfo ui;

			string[] list = { // users not authorized
@"<user1>",
@"<user2>"};

			foreach (string s in list)
			{
				string[] sa = s.Split('\\');
				bool authorized = IsAuthorizedAD(sa[1], sa[0], out ui);
				if (!authorized) continue;
				else continue;
			}

			return;
		}

		/// <summary>
		/// Get users in group
		/// </summary>

		public static void Test()
		{
			ListMembers(@"<domain>\Approved_Mobius_Users");
			GroupPrincipal gp = GetSecurityGroupPrincipal(@"<domain>\Approved_Mobius_Users");

			string groupDn = gp.DistinguishedName;
			ArrayList members = GetGroupMembers(groupDn, false);

			ArrayList domains = EnumerateDomains();
			ArrayList domainControllers = EnumerateDomainControllers();
			ArrayList catalogs = EnumerateCatalogs();

			return;
		}

		/// <summary>
		/// This is a tempoaray fix/method to get the distinguished name.  Normally we would use the UserPrincipal.DistinguishedName.
		/// However there is a .NET 4.5 Active Directory bug that sometime fails when trying to get the UserPrincipal.  
		/// Therefore we are using the global catalog for now, and calling this method to get the distinguished name.  
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="domainName"></param>
		/// <returns></returns>

		private static string GetDistinguishedName(string userName, string domainName)
		{
			string dn = null;

			try
			{
				var currentForest = Forest.GetCurrentForest();
				var globalCatalog = currentForest.FindGlobalCatalog();
				DirectorySearcher search = globalCatalog.GetDirectorySearcher();
				search.Filter = "(&(objectClass=user)(anr=" + userName + "))";
				search.PropertiesToLoad.Add("distinguishedname");
				SearchResult result = search.FindOne();
				dn = (string)result.Properties["distinguishedname"][0];
			}
			catch (Exception ex)
			{
				DebugLog.Message("Failed search.FindOne");
			}
			return dn;
		}

	}
}
