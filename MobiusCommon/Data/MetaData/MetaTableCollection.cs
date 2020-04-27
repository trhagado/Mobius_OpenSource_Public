using Mobius.ComOps;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mobius.Data
{
	/// <summary>
	/// Collection of metatables & method to look up
	/// </summary>

	public class MetaTableCollection
	{
		public static IMetaTableFactory MetaTableFactory; // factory to call to get tables we haven't seen yet
		public static Dictionary<string, MetaTable> TableMap = new Dictionary<string, MetaTable>(); // map of tables by name that have been realized

		public static MetaTreeNode ContentsRoot = null; // root of database contents tree
		public static Dictionary<string, object> ContentsTables = null; // dict of tables by name that appear in database contents
		public static Dictionary<string, object> UserObjectTables = null; // dict of annotation & calc fields that can appear in database contents

		/// <summary>
		/// Reset metatable data
		/// </summary>

		public static void Reset()
		{
			TableMap = new Dictionary<string, MetaTable>();
			return;
		}

		/// <summary>
		/// Add or update a metatable in the local table map
		/// </summary>
		/// <param name="mt"></param>

		public static bool Add(
			MetaTable mt)
		{
			mt.Name = mt.Name.Trim().ToUpper(); // normalize name

			if (RestrictedMetaTables.MetatableIsRestricted(mt.Name)) // if restricted then don't add to map
				return false;

            if (RestrictedMetaTables.MetatableIsGenerallyRestricted(mt.Name))
                return false; //if metatable is generally restricted to the user don't add to map

            TableMap[mt.Name] = mt; // store/update in map
			return true;
		}

		/// <summary>
		/// Add or update a metatable in the local and remote table map
		/// </summary>
		/// <param name="mt"></param>

		public static void UpdateGlobally(
			MetaTable mt)
		{
			Add(mt); // do local add

			if (MetaTableFactory == null) throw new Exception("MetaTableFactory instance is not defined");
			MetaTableFactory.AddServiceMetaTable(mt); // add to service also
																								//MetaTableFactory.RemoveServiceMetaTable(mt.Name); // remove from service side to force reload
			return;
		}

		/// <summary>
		/// Remove a metatable from both the local and remote table map
		/// </summary>
		/// <param name="mtName"></param>

		public static void RemoveGlobally(
			string mtName)
		{
			MetaTableCollection.Remove(mtName); // do local remove

			if (MetaTableFactory == null) throw new Exception("MetaTableFactory instance is not defined");
			MetaTableFactory.RemoveServiceMetaTable(mtName); // remove from service side to force reload
			return;
		}

		/// <summary>
		/// Add the name of a known UserObjectTable to UserObjectTables
		/// </summary>
		/// <param name="mtName"></param>

		public static void AddUserObjectTable(string mtName)
		{
			mtName = mtName.Trim().ToUpper();
			if (!UserObject.IsUserObjectTableName(mtName)) return;
			if (UserObjectTables == null) UserObjectTables = new Dictionary<string, object>();
			UserObjectTables[mtName] = null;
		}

		/// <summary>
		/// Remove the name of a known UserObjectTable from UserObjectTables
		/// </summary>
		/// <param name="mtName"></param>

		public static void RemoveUserObjectTable(string mtName)
		{
			if (!UserObject.IsUserObjectTableName(mtName)) return;
			if (UserObjectTables == null) return;
			mtName = mtName.Trim().ToUpper();
			if (!UserObjectTables.ContainsKey(mtName)) return;

			UserObjectTables.Remove(mtName);
			return;
		}

		/// <summary>
		/// Get any existing instance of metatable from map
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTable GetExisting(
			String name)
		{
			MetaTable mt;

			if (name == null || name == "") return null;
			name = name.Trim().ToUpper();

			if (TableMap.ContainsKey(name))
			{
				mt = TableMap[name];
				return mt;
			}

			else return null;
		}

		/// <summary>
		/// Lookup a MetaTable by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTable Get(
			String name)
		{
			try
			{
				MetaTable mt = GetWithException(name);
				return mt;
			}

			catch (Exception ex)
			{
				//DebugLog.Message(DebugLog.FormatExceptionMessage(ex, "Warning: Failed to get metatable: " + name + "\r\n" + new StackTrace(true)));
				return null;
			}
		}

		/// <summary>
		/// Lookup a MetaTable by name throwing any exceptions from underlying factories
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// 

		public static MetaTable GetWithException(
			String name)
		{
			MetaTable mt = null;

			// See if in map

			if (name == null || name.Trim() == "")
				throw new Exception("Undefined metatable name");
			name = name.Trim().ToUpper(); // normalize name

			if (TableMap.ContainsKey(name))
			{
				mt = TableMap[name];
				return mt;
			}

			if (RestrictedMetaTables.MetatableIsRestricted(name))
				mt = null;

            if (RestrictedMetaTables.MetatableIsGenerallyRestricted(name))
                mt = null;

            else
			{
				if (MetaTableFactory == null) throw new Exception("MetaTableFactory instance is not defined");
				mt = MetaTableFactory.GetMetaTable(name);
			}

			if (mt == null) // table not available
			{
				if (Lex.Contains(name, "calcfield")) name = name; // debug
				string msg = "Unable to find data table: " + name + "\r\n\r\n" +
				"The table may not exist or you may not be authorized for access.";

				//DebugLog.Message(msg); // log for debugging
				throw new Exception(msg); // no luck
			}

			mt.Name = mt.Name.Trim().ToUpper(); // normalize name

			TableMap[mt.Name] = mt; // store in map
			if (Lex.Ne(mt.Name, name)) // if original name was an alias then store under that alias also
				TableMap[name] = mt;

			return mt;
		}

		/// <summary>
		/// Get a table description
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static TableDescription GetDescription(string mtName)
		{
			if (MetaTableFactory == null) throw new Exception("MetaTableFactory instance is not defined");
			TableDescription td = MetaTableFactory.GetDescription(mtName);
			return td;
		}

		/// <summary>
		/// Remove a metatable from the collection
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static bool Remove(
			String name)
		{
			if (name == null || name == "") return false;
			name = name.Trim().ToUpper();

			if (!TableMap.ContainsKey(name)) return false;
			MetaTable oldMt = TableMap[name];
			TableMap.Remove(name);
			return true;
		}

		/// <summary>
		/// Return true if table is in contents tree including both the basic tree and the UserObject subtrees
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static bool IsMetaTableInContents(
			String name)
		{
			if (GetContentsTables() == null) return false;

			name = name.Trim().ToUpper();
			if (ContentsTables.ContainsKey(name)) return true; // check main tree
																												 //			if (Lex.Eq(name, "annotation_17068") name = "annotation_17068"; // debug

			if (GetUserObjectTables() == null) return false;

			if (UserObjectTables != null && UserObjectTables.ContainsKey(name)) return true; // check user objects maintained separately from basic tree

			return false;
		}

		/// <summary>
		/// Get dictionary of contents tables
		/// </summary>
		/// <returns></returns>

		public static Dictionary<string, object> GetContentsTables()
		{
			if (ContentsTables != null) return ContentsTables;
			int t0 = TimeOfDay.Milliseconds();
			ContentsTables = new Dictionary<string, object>();
			if (ContentsRoot == null) return null; // Can't find root

			AddNodeChildrenToContentsTables(ContentsRoot);
			t0 = TimeOfDay.Milliseconds() - t0;
			return ContentsTables;
		}

		/// <summary>
		/// Recursively add node children to hash of tables in contents tree
		/// </summary>
		/// <param name="parent"></param>

		static void AddNodeChildrenToContentsTables(
			MetaTreeNode parent)
		{
			foreach (MetaTreeNode mtn in parent.Nodes)
			{
				if (mtn.IsFolderType)
				{
					AddNodeChildrenToContentsTables(mtn); // go recursive
				}

				else if (mtn.Type == MetaTreeNodeType.MetaTable ||
					mtn.Type == MetaTreeNodeType.Annotation ||
					mtn.Type == MetaTreeNodeType.CalcField)
				{
					ContentsTables[mtn.Target.Trim().ToUpper()] = null;
				}
			}
		}

		/// <summary>
		/// Get dictionary of UserObject tables
		/// </summary>
		/// <returns></returns>

		public static Dictionary<string, object> GetUserObjectTables()
		{
			if (UserObjectTables != null) return UserObjectTables;

			IUserObjectTree iuot = InterfaceRefs.IUserObjectTree;
			if (iuot != null) // be sure the user object tree is built
				iuot.AssureTreeIsBuilt();

			int t0 = TimeOfDay.Milliseconds();
			UserObjectTables = new Dictionary<string, object>();
			if (ContentsRoot == null) return null; // Can't find root

			AddNodeChildrenToUserObjectTables(ContentsRoot);
			t0 = TimeOfDay.Milliseconds() - t0;
			return UserObjectTables;
		}

		/// <summary>
		/// Recursively add node children to hash of UserObject tables in contents tree
		/// </summary>
		/// <param name="parent"></param>

		static void AddNodeChildrenToUserObjectTables(
			MetaTreeNode parent)
		{
			foreach (MetaTreeNode mtn in parent.Nodes)
			{
				if (mtn.IsFolderType)
				{
					AddNodeChildrenToUserObjectTables(mtn); // go recursive
				}

				else if (mtn.Type == MetaTreeNodeType.Annotation ||
					mtn.Type == MetaTreeNodeType.CalcField)
				{
					UserObjectTables[mtn.Target.Trim().ToUpper()] = null;
				}
			}
		}

		/// <summary>
		/// Get default key MetaTable
		/// </summary>
		/// <returns></returns>

		public static MetaTable GetDefaultRootMetaTable()
		{
			return MetaTableCollection.Get("corp_structure"); // todo: set from parameter
		}

	} // MetaTableCollection

	/// <summary>
	/// Class for checking for restricted metatables
	/// </summary>

	public class RestrictedMetaTables
	{
		static int RejectedRestrictedAccessAttempts = 0;
		static string RejectedMetaTables = "";
		static bool LogMessageIfRestrictedEnabled = true; // Enables/disables logging of restricted table attempts

		/// <summary>
		/// Return true if current user is not authorized to access the metatable
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool MetatableIsRestricted(
			string mtName,
			bool logMessageIfRestricted = true)
		{
			return !MetatableIsNotRestricted(mtName, logMessageIfRestricted);
		}

		/// <summary>
		/// Return true if current user is authorized to access the metatable
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool MetatableIsNotRestricted(
			string mtName,
			bool logMessageIfRestricted = true)
		{
			mtName = mtName.Trim().ToUpper();

			HashSet<string> mtSet = ClientState.UserInfo?.RestrictedViewAllowedMetaTables;
			if (mtSet == null) return true;
			else if (mtSet.Contains(mtName)) return true;

			else // attempt to access restricted table
			{
				string mtName2 = "<" + mtName + ">";
				if (!Lex.Contains(RejectedMetaTables, mtName2)) // log restricted access attempt for each table once
				{
					if (logMessageIfRestricted)
					{
						RejectedRestrictedAccessAttempts++;
						RejectedMetaTables += mtName2;

						if (LogMessageIfRestrictedEnabled)
						{
							string msg = "Attempt to access restricted metatable: " + mtName;
							if (RejectedRestrictedAccessAttempts == 1) // log stack info the first time
								msg += "\r\n" + new StackTrace(true);
							DebugLog.Message(msg);
						}
					}

				}

				return false;
			}
		}

        /// <summary>
        /// Return true if current user is not authorized to access the metatable
        /// </summary>
        /// <param name="mtName"></param>
        /// <returns></returns>

        public static bool MetatableIsGenerallyRestricted(string mtName, bool logMessageIfRestricted = true)
        {
            return !MetatableIsNotGenerallyRestricted(mtName, logMessageIfRestricted);
        }

        /// <summary>
        /// Return true if current user is authorized to access the metatable
        /// </summary>
        /// <param name="mtName"></param>
        /// <returns></returns>

        public static bool MetatableIsNotGenerallyRestricted(string mtName, bool logMessageIfRestricted = true)
        {            
            HashSet<string> GenerallyRestrictedMetatables = ClientState.UserInfo?.GenerallyRestrictedMetatables;

            if (GenerallyRestrictedMetatables == null) return true;

            if (GenerallyRestrictedMetatables.Contains(mtName.ToUpper()))
            {
                // log attempt to access restricted table
                string mtName2 = "<" + mtName + ">";
                if (!Lex.Contains(RejectedMetaTables, mtName2)) // log for each table
                {
                    if (logMessageIfRestricted)
                    {
                        string msg = "Attempt to access generally restricted metatable: " + mtName;
                        RejectedRestrictedAccessAttempts++;
                        //if (RejectedRestrictedAccessAttempts == 1) // log stack info the first time 
                        //    msg += "\r\n" + new StackTrace(true);
                        DebugLog.Message(msg);
                    }

                    RejectedMetaTables += mtName2;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// ChangeRejectedAccessAttemptLogging
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>

        public static bool SetRejectedAccessAttemptLogging(bool newValue)
		{
			bool oldValue = LogMessageIfRestrictedEnabled;
			LogMessageIfRestrictedEnabled = newValue;
			return oldValue;
		}

		/// <summary>
		/// RestoreRejectedAccessAttemptLogging
		/// </summary>
		/// <param name="oldValue"></param>

		public static void RestoreRejectedAccessAttemptLogging(bool oldValue)
		{
			LogMessageIfRestrictedEnabled = oldValue;
			return;
		}
	}

    /// <summary>
    /// Interfact to MetaTableFactory
    /// Used by the client to access the factory via the service interface and
    /// by the service to access the factory internally as needed.
    /// </summary>

    public interface IMetaTableFactory
	{

		/// <summary>
		/// Build MetaTable
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		MetaTable GetMetaTable(string mtName);

		/// <summary>
		/// Add a metatable to the services side from the client 
		/// </summary>
		/// <param name="mt"></param>

		void AddServiceMetaTable(MetaTable mt);

		/// <summary>
		/// Remove a metatable on the services side from the client 
		/// </summary>
		/// <param name="mtName"></param>

		void RemoveServiceMetaTable(string mtName);

		/// <summary>
		/// Load/reload from specified file
		/// </summary>

		void BuildFromFile(string rootFile);

		/// <summary>
		/// Get the description for a table
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		TableDescription GetDescription(string mtName);
	}

	/// <summary>
	/// Table description
	/// </summary>

	public class TableDescription
	{
		public string TextDescription; // if the description is text or a URL/UNC link
		public byte[] BinaryDescription; // content of a binary document
		public string TypeName; // document type if binary description
	}

}
