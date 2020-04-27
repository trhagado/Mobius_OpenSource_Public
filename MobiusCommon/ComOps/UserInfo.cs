using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Mobius.ComOps
{

    /// <summary>
    /// Basic user identification info
    /// </summary>

    public class UserInfo
    {
        public string UserDomainName = ""; // lan domain for client machine
        public string UserName = ""; // Windows NT userid of client
        public string LastName = "";
        public string FirstName = "";
        public string MiddleInitial = "";
        public string EmailAddress = "";
        public string Company = "";
        public string Site = "";
        public string Department = "";

        public PrivilegesMx Privileges = new PrivilegesMx();

        public HashSet<string> RestrictedViewUsers = null; // list of users in the view if user is restricted to a subset of the database (not serialized to client)
        public HashSet<string> RestrictedViewAllowedMetaTables = null; // list of allowed metatables if user is restricted to a subset of the database
        public HashSet<int> RestrictedViewAllowedCorpIds = null; // list of allowed CorpIds if user is restricted to a subset of the database (not serialized to client)        
        // <summary>
        // HashSet of generally restricted to the current user        
        // </summary>		
        public HashSet<string> GenerallyRestrictedMetatables = null;

        /// <summary>
        /// Default constructor
        /// </summary>

        public UserInfo()
        {
            return;
        }

        /// <summary>
        /// Get the full person name, "John Q Public"
        /// </summary>

        public string FullName
        {
            get
            {
                if (Lex.IsUndefined(LastName) && Lex.IsUndefined(FirstName))
                {
                    if (Lex.IsDefined(UserName)) return UserName;
                    else throw new Exception("User not defined");
                }

                string name = FirstName;
                if (MiddleInitial != "") name += " " + MiddleInitial;
                name += " " + LastName;
                return name;
            }
        }

        /// <summary>
        /// Get the full person name with the last name first, "Public, John Q"
        /// </summary>

        public string FullNameReversed
        {
            get
            {
                if (Lex.IsUndefined(LastName) && Lex.IsUndefined(FirstName))
                {
                    if (Lex.IsDefined(UserName)) return UserName;
                    else throw new Exception("User not defined");
                }

                string name = LastName + ", " + FirstName;
                if (MiddleInitial != "") name += " " + MiddleInitial;
                return name;
            }
        }

        /// <summary>
        /// Get short person name, e.g. "J. Smith"
        /// </summary>

        public string ShortName
        {
            get
            {
                if (Lex.IsUndefined(LastName) && Lex.IsUndefined(FirstName))
                {
                    if (Lex.IsDefined(UserName)) return UserName;
                    else throw new Exception("User not defined");
                }

                string name = "";
                if (FirstName.Length > 0) name = FirstName.Substring(0, 1) + ". ";
                name += LastName;

                return name;
            }
        }

        /// <summary>
        /// Get short person name, e.g. "Smith, J."
        /// </summary>

        public string ShortNameReversed
        {
            get
            {
                if (Lex.IsUndefined(LastName) && Lex.IsUndefined(FirstName))
                {
                    if (Lex.IsDefined(UserName)) return UserName;
                    else throw new Exception("User not defined");
                }

                string name = LastName;
                if (FirstName.Length > 0)
                    name += ", " + FirstName.Substring(0, 1) + ".";

                return name;
            }
        }

        /// <summary>
        /// NormalizeUserName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>

        public static string NormalizeUserName(string userName)
        {
            string domainName;
            if (userName.Contains(@"\"))
                SplitDomainAndAccountName(userName, out domainName, out userName);

            return userName.ToUpper();
        }

        /// <summary>
        /// Split domain\userName
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="domainName"></param>
        /// <param name="userName"></param>
        /// <returns>True if successful, false if user name only</returns>

        public static bool SplitDomainAndAccountName(
                string fullName,
                out string domainName,
                out string userName)
        {
            string[] sa = fullName.Split('\\');
            if (sa.Length == 2)
            {
                domainName = sa[0];
                userName = sa[1];
                return true;
            }

            else
            {
                domainName = "";
                userName = fullName;
                return false;
            }
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>

        public UserInfo Clone()
        {
            UserInfo ui = (UserInfo)this.MemberwiseClone();
            return ui;
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>

        public string Serialize()
        {
            string serializedUserInfo =
                            this.FirstName + "|" +
                            this.MiddleInitial + "|" +
                            this.LastName + "|" +
                            this.EmailAddress + "|" +
                            this.Company + "|" +
                            this.Site + "|" +
                            this.Department + "|" +
                            this.UserDomainName + "|" +
                            this.UserName + "|" +
                            this.Privileges.PrivilegeListString;

            if (this.RestrictedViewAllowedMetaTables != null) // include allowed metatables if defined
            {
                string txt = Lex.HashSetToCsvString(RestrictedViewAllowedMetaTables, false);
                serializedUserInfo += "|" + txt;
            }

            bool newClient = ClientState.MobiusClientVersionIsAtLeast(5, 0); //remove after client is updated with RestrictedMetatable code. nwr 08/01/2019
            if (newClient)
            {
                if (this.GenerallyRestrictedMetatables != null && this.GenerallyRestrictedMetatables.Count > 0)
                {
                    string txt = Lex.HashSetToCsvString(GenerallyRestrictedMetatables, false);

                    if (this.RestrictedViewAllowedMetaTables == null)
                        serializedUserInfo += "||" + txt;
                    else
                        serializedUserInfo += "|" + txt;
                }
            }

            return serializedUserInfo;
        }

        public static UserInfo Deserialize(string serializedUserInfo)
        {
            UserInfo userInfo = null;
            if (serializedUserInfo != null)
            {
                userInfo = new UserInfo();
                string[] sa = serializedUserInfo.Split('|');
                if (sa.Length > 0) userInfo.FirstName = sa[0];
                if (sa.Length > 1) userInfo.MiddleInitial = sa[1];
                if (sa.Length > 2) userInfo.LastName = sa[2];
                if (sa.Length > 3) userInfo.EmailAddress = sa[3];
                if (sa.Length > 4) userInfo.Company = sa[4];
                if (sa.Length > 5) userInfo.Site = sa[5];
                if (sa.Length > 6) userInfo.Department = sa[6];
                if (sa.Length > 7) userInfo.UserDomainName = sa[7].ToUpper();

                if (sa.Length > 8) // user name
                {
                    userInfo.UserName = sa[8].ToUpper();
                    //if (userInfo.UserName.StartsWith(":")) // data fixup
                    //  userInfo.UserName = userInfo.UserName.Substring(1);
                }

                if (sa.Length > 9) // privileges
                {
                    userInfo.Privileges.PrivilegeListString = sa[9];
                }

                if (sa.Length > 10) // allowed metatables
                {
                    string[] sa2 = sa[10].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (sa2.Length > 0)
                        userInfo.RestrictedViewAllowedMetaTables = new HashSet<string>(sa2);
                }
                if (sa.Length > 11) //generally restricted metatables.
                {
                    string[] sa2 = sa[11].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (sa2.Length > 0)
                        userInfo.GenerallyRestrictedMetatables = new HashSet<string>(sa2);
                }

            }

            return userInfo;
        }
    }

    /// <summary>
    /// Privileges a user has
    /// </summary>

    public class PrivilegesMx
    {
        public static string[] ValidPrivilegeList = // list of valid privileges
                new string[] { // default list, update from config with full list
						"Logon",
                        "RetrieveStructures",
                        "RetrieveSequences",
                        "Developer",
                        "Administrator",
                        "Impersonate",
                        "CreateUserGroup",
                        "ContentsTreeEditor",
                        "ProjectEditor" };

        /// <summary>
        /// SetValidPrivilegeListFromInifile
        /// </summary>
        /// <param name="iniFile"></param>

        public static void SetValidPrivilegeListFromInifile(IniFile iniFile)
        {
            if (iniFile == null) return;
            string list = iniFile.Read("ValidPrivilegeList");
            if (Lex.IsUndefined(list)) return;

            string[] sa = list.Split(',');
            for (int i1 = 0; i1 < sa.Length; i1++)
            {
                sa[i1] = sa[i1].Trim();
            }

            ValidPrivilegeList = sa;
            return;
        }

        /// <summary>
        /// List of privileges user has in string form
        /// </summary>

        public string PrivilegeListString // privileges user has
        {
            get { return _privilegeListString; }
            set { _privilegeListString = value; }
        }
        string _privilegeListString = "";

        public bool CanLogon
        {
            get { return HasPrivilege("Logon"); }
            set { SetPrivilege("Logon", value); }
        }

        public bool CanRetrieveStructures
        { // default to false when CanRetrieveStructures(userName) is always called when authorizing
            get { return HasPrivilege("RetrieveStructures"); }
            set { SetPrivilege("RetrieveStructures", value); }
        }

        public bool CanRetrieveSequences
        { // default to false when CanRetrieveSequences(userName) is always called when authorizing
            get { return HasPrivilege("RetrieveSequences"); }
            set { SetPrivilege("RetrieveSequences", value); }
        }

        public bool CanEditContentsTree
        {
            get { return HasPrivilege("ContentsTreeEditor"); }
        }

        public bool CanCreateProjects
        {
            get
            {
                return (HasPrivilege("ProjectCreator") ||
                                        HasPrivilege("ContentsTreeEditor"));
            }
        }

        public bool CanEditProjects
        {
            get
            {
                return (HasPrivilege("ProjectEditor") ||
                                HasPrivilege("ContentsTreeEditor") ||
                                HasPrivilege("ProjectCreator"));
            }
        }

        /// <summary>
        /// Assign or remove specified privilege
        /// </summary>
        /// <param name="privname"></param>
        /// <param name="value"></param>

        public void SetPrivilege(
                string privName,
                bool value)
        {
            if (!IsValidPrivilegeName(privName))
                throw new Exception("Invalid privilege name: " + privName);

            if (value == true) // add the priv
            {
                if (!HasPrivilege(privName))
                    PrivilegeListString += "<" + privName + ">";
            }

            else // remove the priv
            {
                if (HasPrivilege(privName))
                    PrivilegeListString = Lex.Replace(PrivilegeListString, "<" + privName + ">", "");
            }

            return;
        }

        /// <summary>
        /// Return true if user has privilege
        /// </summary>
        /// <param name="privName"></param>
        /// <returns></returns>

        public bool HasPrivilege(string privName)
        {
            bool hasPriv = ContainsPriv(PrivilegeListString, privName);
            return hasPriv;
        }

        static bool ContainsPriv(string content, string s)
        {
            s = "<" + s + ">";
            return Lex.Contains(content, s);
        }

        /// <summary>
        /// See if a privilege name is valid
        /// </summary>
        /// <param name="priv"></param>
        /// <returns></returns>

        public static bool IsValidPrivilegeName(
                string priv)
        {
            if (ValidPrivilegeList == null) return false;

            foreach (string s in ValidPrivilegeList)
            {
                if (Lex.Eq(s, priv)) return true;
            }

            return false;
        }

    }

}
