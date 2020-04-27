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
using System.Linq;

namespace Mobius.UAL
{
    /// <summary>
    /// Metatable with restricted access to less than all Mobius users
    /// </summary>

    public class RestrictedMetatable
    {

        static string RejectedMetaTables = "";

        /// <summary>
        /// Return true if current user is not authorized to access the metatable
        /// </summary>
        /// <param name="mtName"></param>
        /// <returns></returns>

        public static bool MetatableIsGenerallyRestricted(string mtName)
        {
            return !MetatableIsNotGenerallyRestricted(mtName);
        }

        /// <summary>
        /// Return true if current user is authorized to access the metatable
        /// </summary>
        /// <param name="mtName"></param>
        /// <returns></returns>

        public static bool MetatableIsNotGenerallyRestricted(string mtName)
        {            
            mtName = mtName.Trim().ToUpper();

            HashSet<string> GenerallyRestrictedMetatables = ClientState.UserInfo?.GenerallyRestrictedMetatables;

            if (GenerallyRestrictedMetatables == null) return true;

            if (GenerallyRestrictedMetatables.Contains(mtName))
            {
                string mtName2 = "<" + mtName + ">";
                if (!Lex.Contains(RejectedMetaTables, mtName2))
                {
                    DebugLog.Message("Attempt to access generally restricted metatable: " + mtName); // log for debugging
                    RejectedMetaTables += mtName2;
                }

                return false;
            }

            return true;
        }
               
        /// <summary>
        /// Do initial read of the list of restricted metatables and determine which metatables are restricted to the current user.
        /// </summary>
        public static HashSet<string> GetUsersGenerallyRestrictedMetatables(string userName, string domainName)
        {
            StreamReader sr;
            HashSet<string> generallyRestrictedMetatables = new HashSet<string>();

            bool metatableRestrictionsActive = ServicesIniFile.ReadBool("UseGenerallyRestrictedMetatables", false);
            if (!metatableRestrictionsActive) return null;

            string dirName = ServicesDirs.MetaDataDir + @"\RestrictedMetatables";
            if (!Directory.Exists(dirName)) return null;
            string fileName = dirName + @"\GenerallyRestrictedMetatables.txt";
            if (!File.Exists(fileName)) return null;
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception ex)
            {
                return null;
            }

            List<string> userADGroups = ActiveDirectoryDao.GetGroupCommonNamesUserIsMemberOf(userName, domainName, false);

            while (true)
            {
                string txt = sr.ReadLine();
                if (txt == null) break;
                if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

                string restrictedMetatableName = txt.Trim().ToUpper();

                if (!generallyRestrictedMetatables.Contains(restrictedMetatableName))
                {
                    if (IsGenerallyRestrictedMetatable(restrictedMetatableName, userName, userADGroups))
                        generallyRestrictedMetatables.Add(restrictedMetatableName);
                }
            }

            sr.Close();

            return generallyRestrictedMetatables.Count > 0 ? generallyRestrictedMetatables : null;
        }

        private static bool IsGenerallyRestrictedMetatable(string restrictedMetatableName, string userName, List<string> userADGroups)
        {
            List<string> restrictedMetatableUsers = GetRestrictedMetatableUsers(restrictedMetatableName);

            if (restrictedMetatableUsers == null)
                return false;

            //check users
            foreach (string restrictedMetatableUser in restrictedMetatableUsers)
            {
                if (restrictedMetatableUser == userName)
                {
                    return false;
                }
            }

            //check groups            
            List<string> restrictedMetatableGroups = GetRestrictedMetatableGroups(restrictedMetatableName);
            if (restrictedMetatableGroups == null)
                return false;

            foreach (string restrictedMetatableGroup in restrictedMetatableGroups)
            {
                if (userADGroups.Contains(restrictedMetatableGroup))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Get the allowed users and groups for a Generally Restricted Metatable
        /// </summary>
        /// <param name="RestrictedMetatableName"></param>
        /// <returns></returns>
        private static List<string> GetRestrictedMetatableUsers(string RestrictedMetatableName)
        {
            StreamReader sr;

            List<string> Users = new List<string>();

            string dirName = ServicesDirs.MetaDataDir + @"\RestrictedMetatables";
            string fileName = dirName + @"\" + RestrictedMetatableName + "_Users.txt";
            if (!File.Exists(fileName)) throw new Exception("Missing file: " + fileName);
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception ex)
            {
                return null;
            }

            while (true)
            {
                string txt = sr.ReadLine();
                if (txt == null) break;
                if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

                string user = txt.Trim().ToUpper();
                Users.Add(user);
            }

            sr.Close();

            if (Users.Contains("<ALL>") && Users.Count == 1)                
                Users = null; // special keyword to include all userids

            return Users;
        }

        /// <summary>
        /// Get the allowed users and groups for a Generally Restricted Metatable
        /// </summary>
        /// <param name="RestrictedMetatableName"></param>
        /// <returns></returns>
        private static List<string> GetRestrictedMetatableGroups(string RestrictedMetatableName)
        {
            StreamReader sr;

            List<string> groups = new List<string>();

            string dirName = ServicesDirs.MetaDataDir + @"\RestrictedMetatables";
            string fileName = dirName + @"\" + RestrictedMetatableName + "_Groups.txt";
            if (!File.Exists(fileName)) throw new Exception("Missing file: " + fileName);
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception ex)
            {
                return null;
            }

            while (true)
            {
                string txt = sr.ReadLine();
                if (txt == null) break;
                if (Lex.IsUndefined(txt) || txt.StartsWith(";")) continue;

                string group = txt.Trim().ToUpper();
                groups.Add(group);
            }

            sr.Close();

            if (groups.Contains("<ALL>") && groups.Count == 1)                
                groups = null; // special keyword to include all userids

            return groups;
        }
    }
}
