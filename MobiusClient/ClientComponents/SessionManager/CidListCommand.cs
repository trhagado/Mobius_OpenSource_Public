using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Process compound id list commands
	/// </summary>

	public class CidListCommand : ICidListDao
	{
		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		public CidListCommand()
		{
			return;
		}

		public static string Process (
			string command)
		{
			string tok;

			Lex lex = new Lex();
			lex.OpenString(command);
			tok = lex.Get();
			if (Lex.Eq(tok,"List")) tok = lex.Get();
			string arg = Lex.RemoveAllQuotes(lex.Get());

			if (Lex.Eq(tok,"New")) return CreateNewList();
			else if (Lex.Eq(tok, "EditSaved")) return EditSaved();
			else if (Lex.Eq(tok, "EditTemp")) return EditTemp(arg);
			else if (Lex.Eq(tok, "SaveCurrent")) return SaveCurrentCommand();
			else if (Lex.Eq(tok, "SaveCurrentToTemp")) return SaveCurrentToTempCommand(arg);
			else if (Lex.Eq(tok, "SaveCurrentToNewTemp")) return SaveCurrentToNewTempCommand();
			else if (Lex.Eq(tok, "CopySavedToCurrent")) return CopySavedToCurrentCommand();
			else if (Lex.Eq(tok, "CopyTempToCurrent")) return CopyTempToCurrentCommand(arg);
			else if (Lex.Eq(tok, "Delete")) return DeleteList(); 
			else if (Lex.Eq(tok, "Logic"))	return LogicallyCombineLists("");
			else if (Lex.Eq(tok, "Export")) return ExportList();
			else if (Lex.Eq(tok, "Import")) return ImportList();
			else if (Lex.Eq(tok, "Subset")) return SubsetDatabase(arg);

			else return tok + " is not a valid List command";
		}

		static string CreateNewList()
		{
			CidListEditor.Edit(""); // create new list
			return "";
		}

/// <summary>
/// Edit a saved list
/// </summary>
/// <returns></returns>

		static string EditSaved()
		{
			UserObject uo = SelectListDialog("Edit List");
			if (uo==null) return "";
			CidList cnList = CidListEditor.Edit(uo.InternalName);
			return "";
//			if (cnList==null) return "";
//			else return "List " + cnList.UserObject.Name + " now contains " + cnList.Count.ToString() + 
//						 " " + SS.I.CompoundIdLabel + "s";
		}

/// <summary>
/// Edit current list
/// </summary>
/// <returns></returns>

		public static string EditTemp(
			string tempListName)
		{
			tempListName = GetInternalTempListName(tempListName);

			CidList cnList = Read(tempListName);
			if (cnList==null) // create empty current if needed
			{
				cnList = new CidList();
				UserObject uo = UserObject.ParseInternalUserObjectName(tempListName, UserObjectType.CnList, SS.I.UserName);
				Write(cnList, uo);
			}
			CidListEditor.Edit(tempListName);
			return "";
		}

/// <summary>
/// Normalize a temp list name to a form: TEMP_FOLDER.listName
/// </summary>
/// <param name="tempListName"></param>
/// <returns></returns>

		public static string GetInternalTempListName(
			string tempListName)
		{
			tempListName = Lex.RemoveAllQuotes(tempListName);
			if (Lex.EndsWith(tempListName, "Current List"))
				tempListName = Lex.Replace(tempListName, "Current List", "Current");
			if (!Lex.StartsWith(tempListName, UserObject.TempFolderNameQualified))
				tempListName = UserObject.TempFolderNameQualified + tempListName;

			return tempListName;
		}

/// <summary>
/// Save current list to a saved list
/// </summary>
/// <returns></returns>

		public static string SaveCurrentCommand()
		{
			SaveCurrentList(false, null);
			return "";
		}

/// <summary>
/// Save the current list to a temp list
/// </summary>
/// <param name="command"></param>
/// <returns></returns>

		public static string SaveCurrentToTempCommand (
			string tempListName)
		{
			SaveCurrentList(true, tempListName);
			return "";
		}

/// <summary>
/// Save current list to a new temp list
/// </summary>
/// <returns></returns>

		public static string SaveCurrentToNewTempCommand()
		{
			SaveCurrentList(true, null);
			return "";
		}

/// <summary>
/// Save current to specified destination
/// </summary>
/// <param name="saveToTempList"></param>
/// <param name="tempListName"></param>
/// <returns></returns>

		static UserObject SaveCurrentList(
			bool saveToTempList,
			string tempListName)
		{
			CidList curList = Read(CidList.CurrentListInternalName);
			if (curList == null || curList.Count == 0)
			{
				MessageBoxMx.ShowError("The current list is empty");
				return null;
			}

			UserObject uo = SaveList(curList, "Save Current List", saveToTempList, tempListName);
			return uo;
		}

/// <summary>
/// PromptForNewTempListName()
/// </summary>
/// <returns></returns>

		public static string PromptForNewTempListName()
		{
			return InputBoxMx.Show("Enter the name for the new temporary list", "New Temporary List");
		}

/// <summary>
/// Save a temporary list to a saved list
/// </summary>
/// <returns></returns>

		public static UserObject SaveTempList(
			string tempListName)
		{
			UserObject uo = null;
			CidList tempList;
			String name, txt;

			Lex lex = new Lex();

			tempListName = GetInternalTempListName(tempListName);
			tempList = Read(tempListName);
			if (tempList == null || tempList.Count == 0)
			{
				MessageBoxMx.ShowError("Temporary list not found: " + tempListName);
				return null;
			}

			uo = SaveList(tempList, "Save Temporary List: " + tempListName, false, null);
			return uo;
		}

/// <summary>
/// Update collection of temp lists
/// </summary>
/// <param name="uo"></param>

		public static void UpdateTempListCollection(UserObject uo)
		{
			if (Lex.Ne(uo.ParentFolder, UserObject.TempFolderName)) return; // update for lists in temp folder only

			List<TempCidList> tLists = SS.I.TempCidLists;
			TempCidList tl = null;
			int tli;
			for (tli = 0; tli < tLists.Count; tli++)
			{
				tl = tLists[tli];
				if (Lex.Eq(tl.Name, uo.Name)) break;
			}

			if (tli >= tLists.Count)
			{
				tl = new TempCidList();
				tLists.Add(tl);
			}

			tl.Name = uo.Name;
			tl.Count = uo.Count;
			tl.Id = uo.Id;
			return;
		}

/// <summary>
/// Get a temp list by name 
/// </summary>
/// <param name="listName"></param>
/// <returns></returns>

		public static TempCidList GetTempList(string listName)
		{
			List<TempCidList> tLists = SS.I.TempCidLists;
			for (int tli = 0; tli < tLists.Count; tli++)
			{
				TempCidList tl = tLists[tli];
				if (Lex.Eq(tl.Name, listName)) return tl;
			}

			return null;
		}

		/// <summary>
		/// Save the specified list object
		/// </summary>
		/// <returns></returns>

		public static UserObject SaveList(
			CidList list,
			string caption,
			bool saveToTempList,
			string tempListName)
		{
			UserObject uo = null;
			String name, txt;

			uo = new UserObject(UserObjectType.CnList);
			if (saveToTempList)
			{
				if (Lex.IsNullOrEmpty(tempListName))
					tempListName = InputBoxMx.Show("Enter the name of the temporary list to be saved to:", caption);
				if (Lex.IsNullOrEmpty(tempListName)) return null;

				uo.Owner = SS.I.UserName;
				uo.ParentFolder = UserObject.TempFolderName;
				uo.Name = tempListName;
			}

			else uo = UserObjectSaveDialog.Show(caption, uo); // save to permanent list

			if (uo == null) return null;
			if (!uo.IsTempObject) // assign valid folder unless current list
				UserObjectTree.GetValidUserObjectTypeFolder(uo);

			SessionManager.DisplayStatusMessage("Saving List " + uo.Name + "...");

			list.UserObject = uo;
			int rc = Write(list);

			MainMenuControl.UpdateMruList(uo.InternalName);

			SessionManager.DisplayStatusMessage("");
			return uo;
		}

		/// <summary>
		/// Copy a saved list to the current list
		/// </summary>
		/// <returns></returns>

		static string CopySavedToCurrentCommand()
		{
			UserObject uo = SelectListDialog("Copy a Saved List to the Current list");
			if (uo == null) return "";

			SessionManager.DisplayStatusMessage("Copying List " + uo.Name + "...");
			UserObject uo2 = new UserObject(UserObjectType.CnList, SS.I.UserName, UserObject.TempFolderName, "Current");
			int count = CidListDao.CopyList(uo, uo2);
			CidListCommand.UpdateTempListCollection(uo2);
			SessionManager.CurrentResultKeys = ReadCurrentListRemote().ToStringList(); // get from server
			SessionManager.DisplayCurrentCount();
			return "";
		}

		/// <summary>
		/// Copy a temp list to the current list
		/// </summary>
		/// <returns></returns>

		static string CopyTempToCurrentCommand(
			string tempListName)
		{
			SessionManager.DisplayStatusMessage("Copying List " + tempListName + "...");
			UserObject uo = new UserObject(UserObjectType.CnList, SS.I.UserName, UserObject.TempFolderName, tempListName);
			UserObject uo2 = new UserObject(UserObjectType.CnList, SS.I.UserName, UserObject.TempFolderName, "Current");
			int count = CidListDao.CopyList(uo, uo2);
			CidListCommand.UpdateTempListCollection(uo2);
			SessionManager.CurrentResultKeys = ReadCurrentListRemote().ToStringList(); // get from server
			SessionManager.DisplayCurrentCount();
			return "";
		}

/// <summary>
/// Delete a list
/// </summary>
/// <returns></returns>

		static string DeleteList()
		{
			return "Not implemented";
		}

/// <summary>
/// Combine lists
/// </summary>
/// <param name="command"></param>
/// <returns></returns>

		static string LogicallyCombineLists (
			string command)
		{
			ListLogic.Show();
			return "";
		}

/// <summary>
/// Import a list
/// </summary>
/// <returns></returns>

		static string ImportList()
		{
			string filePath = SelectListFileDialog("List File to Import", "");
			if (String.IsNullOrEmpty(filePath)) return "";

			string fileName = Path.GetFileNameWithoutExtension(filePath);

			UserObject oListName = new UserObject(UserObjectType.CnList, SS.I.UserName, fileName);
			oListName = UserObjectSaveDialog.Show("Database List to Import Into",oListName);
			if (oListName == null) return "";

			Progress.Show("Importing List...");
			CidList list = CidList.ReadFromFile(filePath); // read file list
			Write(list, oListName); // write database list
			Progress.Hide();

			return(list.Count.ToString() + " " + MetaTable.KeyMetaTable.KeyMetaColumn.Label + "s have been imported");
		}

/// <summary>
/// Export a list
/// </summary>
/// <returns></returns>

		static string ExportList()
		{
			CidList list;

			UserObject iListName = SelectListDialog("Database List to Export");
			if (iListName == null) return "";

			string initialName = iListName.Name + ".lst";
			string fileName = UIMisc.GetSaveAsFilename("List File to Export Into", initialName, 
								"Lists (*.lst)|*.lst|All files (*.*)|*.*", "LST");

			if (fileName == "") return "";

			Progress.Show("Exporting List...");
			list = Read(iListName); // read database list
			if (list==null) return "Error reading list from database";

			list.WriteToFile(fileName, SS.I.RemoveLeadingZerosFromCids); // write file list
			Progress.Hide();

			return (list.Count.ToString() + " " + MetaTable.KeyMetaTable.KeyMetaColumn.Label + "s have been exported");
		}

/// <summary>
/// Process a database subset command
/// </summary>
/// <param name="arg"></param>
/// <returns></returns>

		static string SubsetDatabase(
			string arg)
		{
			string statusMsg="", returnMsg="";
			CidList newList = new CidList();
			if (Lex.Eq(arg,"Current"))
			{
				newList = Read("Current", QbUtil.CurrentQueryRoot);
				if (newList==null || newList.Count==0)
					return "Current list does not exist or is empty";

				SS.I.DatabaseSubsetListName = "Current";
				SS.I.DatabaseSubsetListSize = newList.Count;

				returnMsg =
					"Queries will now be limited to the " + newList.Count.ToString() + 
					" compound identifiers in the Current list.";
			}

			else if (Lex.Eq(arg,"List"))
			{
				UserObject uo = SelectListDialog("Database Subset");
				if (uo==null) return "";
				newList = Read(uo.InternalName, QbUtil.CurrentQueryRoot);
				if (newList==null || newList.Count==0)
					return "List " + uo.Name + " is empty or could not be read";

				SS.I.DatabaseSubsetListName = uo.Name;
				SS.I.DatabaseSubsetListSize = newList.Count;

				returnMsg =
					"Queries will now be limited to the " + newList.Count.ToString() + 
					" compound identifiers in list " + uo.Name + ".";
			}

			else if (Lex.Eq(arg,"All"))
			{
				SS.I.DatabaseSubsetListName = "";
				SS.I.DatabaseSubsetListSize = -1;
				returnMsg = "The complete database will now be searched.";
			}

			SessionManager.Instance.StatusBarManager.DisplaySearchDomain();

			QueryEngine.SetParameter("DatabaseSubset", newList.ToMultilineString());

			return returnMsg;
		}

		/// <summary>
		/// Prompt user for a saved list
		/// </summary>
		/// <param name="prompt"></param>
		/// <returns></returns>

		public static UserObject SelectListDialog(
			string prompt)
		{
			return SelectListDialog(prompt, null);
		}

		/// <summary>
		/// Prompt user for a saved list
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="defaultName"></param>
		/// <returns></returns>

		public static UserObject SelectListDialog(
			string prompt,
			UserObject defaultUo)
		{
			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.CnList, prompt, defaultUo);
			if (uo == null) return null;

			return uo;
		}

		/// <summary>
		/// Select a list file
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="defaultName"></param>
		/// <returns></returns>

		public static string SelectListFileDialog(
			string prompt,
			string defaultName)
		{
			string name = UIMisc.GetOpenFilename(prompt, defaultName,
				"Lists (*.lst)|*.lst|All files (*.*)|*.*", "LST");
			return name;
		}

		/// <summary>
		/// See if the named list exists
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static bool Exists(
			string name)
		{
			UserObject uo = UserObjectUtil.ParseInternalUserObjectName(name);
			uo.Type = UserObjectType.CnList;
			uo = UserObjectDao.ReadHeader(uo);
			return (uo != null);
		}

		/// <summary>
		/// Read a compound id list given an internal list name (e.g. FOLDER_123.name or LIST_1234)
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static CidList Read(
			string internalName)
		{
			return Read(internalName, null);
		}


/// <summary>
/// Read a compound id list given an internal list name (e.g. FOLDER_123.name or LIST_1234)
/// </summary>
/// <param name="internalName"></param>
/// <param name="mt"></param>
/// <returns></returns>

		public static CidList Read(
			string internalName,
			MetaTable mt)
		{
			return Read(internalName, mt, true); // local or remote
		}

		/// <summary>
		/// Read a compound id list given an internal list name (e.g. FOLDER_123.name or LIST_1234)
		/// </summary>
		/// <param name="name"></param>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static CidList Read(
			string internalName,
			MetaTable mt,
			bool allowLocalRead)
		{
			UserObject uo, uo2;
			string fileName, cn;
			int i1;

			uo = UserObjectUtil.ParseInternalUserObjectName(internalName, UserObjectType.CnList);
			if (uo == null) return null;

			if (allowLocalRead && UserObject.IsCurrentObjectInternalName(internalName)) // get from the current query
				return ReadCurrentListLocal();

			uo2 = UserObjectDao.Read(uo); // get from the server side
			if (uo2 == null)
			{
				MessageBoxMx.ShowError(
					"Unable to find list: " + internalName + "\r\n\r\n" +
					CommandExec.GetUserObjectReadAccessErrorMessage(uo.Id, "list"));
				return null;
			}

			CidList cnList = CidList.Deserialize(uo2, mt);

			if (UserObject.IsCurrentObjectInternalName(internalName)) // if current list store keys with query
				SessionManager.CurrentResultKeys = cnList.ToStringList();

			return cnList;
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
			return Read(name, mt);
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
			return Read(name, null);
		}

/// <summary>
/// Read current list object header
/// </summary>
/// <returns></returns>

		public static UserObject ReadCurrentListHeader()
		{
			return UserObjectDao.ReadHeader(UserObjectType.CnList, SS.I.UserName, UserObject.TempFolderName, "Current");
		}

/// <summary>
/// Read the current list from the server
/// </summary>
/// <returns></returns>

		public static CidList ReadCurrentListRemote()
		{
			string name = UserObject.TempFolderName + ".Current";
			return Read(name, null, false);
		}

/// <summary>
/// Read the current list from the current client query QueryManager data (server not accessed)
/// </summary>
/// <returns></returns>

		public static CidList ReadCurrentListLocal()
		{
			//CidList cidList = new CidList();
			//QueryManager qm =	QbUtil.QueryManager; // (error: may be original qm rather than derived with correct keys)
			//if (qm == null || qm.DataTableManager == null) return cidList;
			//List<string> keys = qm.DataTableManager.ResultsKeys;
			//cidList = new CidList(keys);

			CidList cidList = new CidList(SessionManager.CurrentResultKeys);
			UserObject uo = cidList.UserObject;
			uo.Owner = SS.I.UserName;
			uo.ParentFolder = UserObject.TempFolderName;
			uo.Name = "Current";
			uo.Id = SessionManager.CurrentListId;
			return cidList;
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
			UserObject uo = new UserObject(UserObjectType.CnList);
			uo.Name = name;
			return Write(list, uo);
		}

		public static int Write(
			CidList list,
			string name,
			string description)
		{
			UserObject uo = new UserObject(UserObjectType.CnList);
			uo.Name = name;
			uo.Description = description;
			return Write(list, uo);
		}

		public static int Write(
			CidList list,
			string parentFolder,
			string name,
			string description)
		{
			UserObject uo = new UserObject(UserObjectType.CnList);
			uo.Name = name;
			uo.Description = description;
			uo.ParentFolder = parentFolder;

			if (Lex.Eq(parentFolder, UserObject.TempFolderName)) 
				uo.ParentFolderType = FolderTypeEnum.None;

			else if (Lex.StartsWith(parentFolder, "FOLDER_")) 
				uo.ParentFolderType = FolderTypeEnum.User;

			else uo.ParentFolderType = FolderTypeEnum.System;

			return Write(list, uo);
		}

		public static int WriteWithClonedUserObject(
			CidList list,
			UserObject uo)
		{
			UserObject uo2 = uo.Clone(); 
			uo2.Type = UserObjectType.CnList;
			uo2.Id = 0; // assign new id
			return Write(list, uo2);
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
/// Write the current list to the server
/// </summary>
/// <returns></returns>

		public static int WriteCurrentList()
		{
			return WriteCurrentList(new CidList(SessionManager.CurrentResultKeys));
		}

		/// <summary>
		/// Write the current list to the server
		/// </summary>
		/// <returns></returns>

		public static int WriteCurrentList(
			CidList list)
		{
			UserObject uo = new UserObject(UserObjectType.CnList, SS.I.UserName, UserObject.TempFolderName, "Current");
			uo.Id = SessionManager.CurrentListId; // always use same UserObject id
			return Write(list, uo);
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
			if (Lex.IsNullOrEmpty(uo.Owner)) // set current user as owner if owner not defined
				uo.Owner = SS.I.UserName; 

			if (Lex.IsNullOrEmpty(uo.ParentFolder))
				throw new Exception("No parent folder for list");

			uo.Content = content;
			uo.Count = list.Count;
			UserObjectDao.Write(uo, uo.Id);

			if (uo.IsCurrentObject)
			{
				SessionManager.CurrentResultKeys = list.ToStringList();
				SessionManager.DisplayCurrentCount();
			}

			if (uo.HasDefinedParentFolder)
				MainMenuControl.UpdateMruList(uo.InternalName);

			return list.Count;
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
			return CidListDao.Read(listId, mt);
		}

	}
}
