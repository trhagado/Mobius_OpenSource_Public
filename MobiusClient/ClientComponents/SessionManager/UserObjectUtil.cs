using System.Text;
using System.Xml;
using DevExpress.XtraPrinting.Native;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for UserObjectUtil
	/// </summary>
	public class UserObjectUtil
	{
		static int LastUserHasReadAccessId; // cached result for performance
		static bool LastUserHasReadAccessResult;
		static int LastUserHasWriteAccessId; // cached result for performance
		static bool LastUserHasWriteAccessResult;

		//public UserObjectUtil()
		//{
		//}

		/// <summary>
		/// Right-click commands on objects in a tree
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <param name="userObjectName">Name of user object to operate on</param>
		/// <param name="ctc">ContentsTreeControl to update</param>

		public static bool ProcessCommonRightClickObjectMenuCommands(
			string command,
			MetaTreeNode mtn,
			ContentsTreeControl ctc)
		{
			UserObject uo;
			UserObjectType objType;
			int id;

			string userObjectName = mtn.Target;
			UserObject.ParseObjectTypeAndIdFromInternalName(userObjectName, out objType, out id);
			uo = UserObjectDao.ReadHeader(id);
			if (uo == null)
			{
				MessageBoxMx.ShowError(userObjectName + " not found");
				return false;
			}
            UserObject[] uoArray = new UserObject[] {uo};

			return ProcessCommonRightClickObjectMenuCommands(command, mtn, uoArray, ctc);
		}

/// <summary>
/// Right-click commands on objects in a tree
/// </summary>
/// <param name="command">The command to process</param>
		/// <param name="mtn">MetaTreeNode</param>
		/// <param name="uo">Any associated user object</param>
		/// <param name="ctc">ContentsTreeControl</param>
/// <returns></returns>

		public static bool ProcessCommonRightClickObjectMenuCommands(
			string command,
			MetaTreeNode mtn,
			UserObject[] uoArray,
			ContentsTreeControl ctc)
		{
			UserObject uo2;
			string txt;
            UserObject uo = null;
            if (uoArray != null && uoArray.Length == 1) uo = uoArray[0];

			//if (mtn == null)
			//{
			//	MessageBoxMx.ShowError("Operation is not allowed for this Database Contents node.");
			//	return true;
			//}

			if (Lex.Eq(command, "Cut"))
			{
				CopyCutDelete(command, uoArray, ctc, true, true);
			}

			else if (Lex.Eq(command, "Copy"))
			{
				CopyCutDelete(command, uoArray, ctc, true, false);
			}

			else if (Lex.Eq(command, "Paste"))
			{
				try
				{
					txt = Clipboard.GetText();
					//uo2 = UserObject.Deserialize(txt);
				    uoArray = Deserialize(txt);
                    if (uoArray == null) throw new Exception("Not a UserObject");
					//if (uo2 == null) throw new Exception("Not a UserObject");
				}
				catch (Exception ex)
				{
					MessageBoxMx.ShowError("The clipboard does not contain a recognized user objects");
					return true;
				}

                foreach (var userObject in uoArray)
                {
                    Progress.Show("Pasting " + userObject.Name + "...", UmlautMobius.String, false);

                    Permissions.UpdateAclForNewOwner(userObject, userObject.Owner, SS.I.UserName); // fixup the ACL for the new owner
                    userObject.Owner = SS.I.UserName;
                    userObject.ParentFolder = mtn.Name;
                    mtn = UserObjectTree.GetValidUserObjectTypeFolder(userObject);

                    for (int ci = 0; ; ci++) // find a name that's not used
                    {
                        UserObject uo3 = UserObjectDao.ReadHeader(userObject);
                        if (uo3 == null) break;

                        if (ci == 0) userObject.Name = "Copy of " + userObject.Name;
                        else if (ci == 1) userObject.Name = Lex.Replace(userObject.Name, "Copy of ", "Copy (2) of ");
                        else userObject.Name = Lex.Replace(userObject.Name, "Copy (" + ci + ") of ", "Copy (" + (ci + 1) + ") of ");
                    }

                    UserObject userObjectFinal = null;
                    if (UserObjectDao.ReadHeader(userObject.Id) != null) // create a deep clone if orignal object exists
                        userObjectFinal = DeepClone(userObject);

                    if (userObjectFinal == null) userObjectFinal = userObject;

                    UserObjectDao.Write(userObjectFinal, userObjectFinal.Id); // write using the current id

                    Progress.Hide(); 
                }

				//if (ctc != null) // need to update form directly?
				//  UserObjectTree.RefreshSubtreeInTreeControl(uo2, ctc);
			}

			else if (Lex.Eq(command, "Delete"))
			{
				CopyCutDelete(command, uoArray, ctc, false, true);
			}

			else if (Lex.Eq(command, "Rename"))
			{
				if (!UserHasWriteAccess(uo))
				{
					MessageBoxMx.ShowError("You are not authorized to rename " + uo.Name);
					return true;
				}

				string newName = InputBoxMx.Show("Enter the new name for " + uo.Name,
					"Rename", uo.Name);

				if (newName == null || newName == "" || newName == uo.Name) return true;

				if (!IsValidUserObjectName(newName))
				{
					MessageBoxMx.ShowError("The name " + newName + " is not valid.");
					return true;
				}

				uo2 = uo.Clone();
				uo2.Name = newName;
				uo2.Id = 0; // clear Id so not looked up by id

				if (!Lex.Eq(newName, uo.Name) && UserObjectDao.ReadHeader(uo2) != null)
				{
					MessageBoxMx.ShowError(newName + " already exists.");
					return true;
				}

				uo2.Id = uo.Id;
				UserObjectDao.UpdateHeader(uo2);

				if (ctc != null)
					UserObjectTree.UpdateObjectInTreeControl(uo, uo2, ctc);
			}

			else if (Lex.Eq(command, "MakePublic") ||
			 Lex.Eq(command, "MakePrivate"))
			{
				UserObjectAccess newAccess;
				MetaTreeNode objFolder;

				if (!UserHasWriteAccess(uo))
				{
					MessageBoxMx.ShowError("You are not authorized to make " + uo.Name +
												((Lex.Eq(command, "MakePublic")) ? " public" : " private"));
					return true;
				}

				if (Lex.Eq(command, "MakePublic"))
				{
					if (uo.ParentFolder == "DEFAULT_FOLDER")
					{
						MessageBoxMx.ShowError("Items in the Default Folder cannot be made public");
						return true;
					}
					else
					{
						//check the folder id parentage to ensure that the current folder isn't a subfolder of the default folder
						if (UserObjectTree.FolderNodes.ContainsKey(uo.ParentFolder))
						{
							objFolder = UserObjectTree.FolderNodes[uo.ParentFolder];
							while (objFolder != null)
							{
								if (objFolder.Target == "DEFAULT_FOLDER")
								{
									MessageBoxMx.ShowError("Items in the Default Folder cannot be made public");
									return true;
								}
								objFolder = objFolder.Parent;
							}
						}
						else
						{
							throw new Exception("Failed to recognize the folder that this object is in!");
						}
					}

					newAccess = UserObjectAccess.Public;
				}
				else
				{
					//user folders cannot be made private if they contain public children
					if (uo.Type == UserObjectType.Folder)
					{
						objFolder = UserObjectTree.BuildNode(uo);
						if (UserObjectTree.FolderNodes.ContainsKey(objFolder.Target))
						{
							objFolder = UserObjectTree.FolderNodes[objFolder.Target];
							for (int i = 0; i < objFolder.Nodes.Count; i++)
							{
								MetaTreeNode currentChild = (MetaTreeNode)objFolder.Nodes[i];
							}
						}
					}

					newAccess = UserObjectAccess.Private;
				}

				uo2 = UserObjectDao.Read(uo);
				if (uo2 == null) return true;
				if (uo2.AccessLevel == newAccess) return true; // no change
				uo2.AccessLevel = newAccess;
				UserObjectDao.UpdateHeader(uo2);

				if (ctc != null) // need to update form directly?
					UserObjectTree.RefreshSubtreeInTreeControl(uo2, ctc);

				return true;
			}

			else if (Lex.Eq(command, "ChangeOwner"))
			{
				string newOwner = InputBoxMx.Show("Enter the user name of the person to transfer ownership of " + Lex.AddDoubleQuotes(uo.Name) + " to:",
					"Change Owner", "");

				if (Lex.IsNullOrEmpty(newOwner)) return true;

				string result = UserObjectUtil.ChangeOwner(uo.Id, newOwner);
				if (!Lex.IsNullOrEmpty(result)) MessageBoxMx.Show(result);
				return true;
			}

			else if (Lex.Eq(command, "Permissions")) // set object permissions
			{
				PermissionsDialog.Show(uo);
				return true;
			}

			else if (Lex.Eq(command, "ViewSource"))
			{
				uo = UserObjectDao.Read(uo);
				if (uo == null) return true;

				string ext = ".txt"; // default extension
				if (uo.Type == UserObjectType.Query ||
					uo.Type == UserObjectType.Annotation) ext = ".xml";
				string cFile = ClientDirs.TempDir + @"\Source" + ext;
				StreamWriter sw = new StreamWriter(cFile);
				sw.Write(uo.Content);
				sw.Close();

				SystemUtil.StartProcess(cFile); // show it
			}

			else return false; // command not recognized

			return true; // command recognized and processed
		}

/// <summary>
/// Do some combination of copy/cut/delete on a single userObject
/// </summary>
/// <param name="command"></param>
/// <param name="uoArray"></param>
/// <param name="ctc"></param>
/// <param name="copy"></param>
/// <param name="delete"></param>

        //private static void CopyCutDelete(
        //    string command,
        //    UserObject uo,
        //    ContentsTreeControl ctc,
        //    bool copy,
        //    bool delete)
        //{
        //    UserObject[] uoArray = new UserObject[1] { uo };
        //    CopyCutDelete(command, uoArray, ctc, copy, delete);
        //}

	    /// <summary>
/// Do some combination of copy/cut/delete on an array of Userobjects
/// </summary>
/// <param name="command"></param>
/// <param name="uoArray"></param>
/// <param name="ctc"></param>
/// <param name="copy"></param>
/// <param name="delete"></param>

		static void CopyCutDelete(
			string command,
			UserObject[] uoArray,
			ContentsTreeControl ctc,
			bool copy,
			bool delete)
		{

// Copy to clipboard

			if (copy)
			{

                if (!UserHasReadAccess(SS.I.UserName, uoArray))
                {
                    MessageBoxMx.ShowError("You are not authorized to copy these nodes. Copy canceled.");
                    return;
                }

                if (!UserObject.CanCopyPaste(uoArray))
                {
                    MessageBoxMx.ShowError("Can't copy/cut user objects of these types. Copy/cut canceled.");
                    return;
                }

                List<UserObject> userObjectArray = new List<UserObject>();
                foreach(UserObject userObject in uoArray)
                {
                    UserObject uo = UserObjectDao.Read(userObject.Id);
                    userObjectArray.Add(uo);
                }

                string serializedUos = Serialize(userObjectArray.ToArray());
			    if (serializedUos != null) Clipboard.SetText(serializedUos);
			}

// Delete object

			if (delete)
			{
				if (!UserHasWriteAccess(SS.I.UserName, uoArray))
				{
					MessageBoxMx.ShowError("You are not authorized to cut/delete these nodes.");
					return;
				}

				//for now, don't allow folders to be deleted if they aren't empty
                foreach (var uo in uoArray)
                {
                    if (uo.Type == UserObjectType.Folder)
                    {
                        MetaTreeNode folderNode = UserObjectTree.BuildNode(uo);
                        if (UserObjectTree.FolderNodes.ContainsKey(folderNode.Target.ToUpper()))
                        {
                            folderNode = UserObjectTree.FolderNodes[folderNode.Target.ToUpper()];

                            if (folderNode.Nodes.Count > 0)
                            {
                                MessageBoxMx.ShowError("Folder \"" + folderNode.Label + "\" is not empty and cannot be deleted.");
                                return;
                            }
                        }
                    } 
                }

			    if (!copy)
			    {
			        string caption = "Confirm " + UserObject.GetTypeLabel(uoArray[0].Type) + " Delete";
			        string txt = "Are you sure you want to delete " + Lex.Dq(uoArray[0].Name);
			        if (uoArray.Length > 1) txt = "Are you sure you want to delete these " + uoArray.Length + " items?";
			        DialogResult dr = MessageBoxMx.Show(txt, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			        if (dr != DialogResult.Yes) return;

			        foreach (var uo in uoArray)
			        {
			            UserObject userObject = null;
			            userObject = UserData.IsUserDataObject(uo) ? UserObjectDao.Read(uo.Id) : uo;
			            bool succeeded = userObject != null && UserObjectDao.Delete(userObject.Id);
			                // remove userobject from database
			            if (UserData.IsUserDataObject(userObject)) // start removing any associated user data
			                UserData.StartBackgroundDataDelete(userObject);
			        }

			        //if (UserData.IsUserDataObject(uo)) uo = UserObjectDao.Read(uo.Id); // will need content to del assoc user data
			        //bool succeeded = UserObjectDao.Delete(uo.Id); // remove userobject from database
			        //if (UserData.IsUserDataObject(uo)) // start removing any associated user data
			        //    UserData.StartBackgroundDataDelete(uo);
			    }

			    else
			    
			        foreach (var uo in uoArray)
			        {
			            UserObjectDao.Delete(uo.Id);  // just remove userobject from database, will leave orphan data for annotation tables but need for paste 
			        }
			    

			    if (ctc != null) // redraw the subtree without the deleted user object
					UserObjectTree.RefreshSubtreeInTreeControl(uoArray[0], ctc);
			}

		}

/// <summary>
/// Check if name is valid for a user object
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static bool IsValidUserObjectName(
			string name)
		{
			if (Lex.IsUndefined(name)) return false;

			char[] invalidChars = { '\\', '/', ':', '*', '?', '<', '>', '|' };
			if (name.IndexOfAny(invalidChars) >= 0) // check for disallowed characters
				return false;
			else return true;
		}

		/// <summary>
		/// See if the supplied user name can read the user object the supplied object
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool UserHasReadAccess(
			string userName,
			int objectId)
		{
			if (objectId <= 0) return true; // new unsaved object

			if (objectId == LastUserHasReadAccessId)
				return LastUserHasReadAccessResult;

			UserObject uo = UserObjectDao.ReadHeader(objectId);

			LastUserHasReadAccessId = objectId;
			LastUserHasReadAccessResult = Permissions.UserHasReadAccess(userName, uo);

			return LastUserHasReadAccessResult;
		}

		/// <summary>
		/// See if the current user can modify the supplied object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool UserHasReadAccess(
			string userName,
			UserObject uo)
		{
			if (uo == null || uo.Owner == null || uo.Owner == "") return true; // no current owner

			if (Lex.Eq(uo.Owner, userName) ||
			 Security.IsAdministrator(userName) ||
			 Permissions.UserHasReadAccess(userName, uo))
				return true;

			else return false;
		}

        /// <summary>
        /// See if the current user can modify the supplied object array
        /// </summary>
        /// <param name="uo"></param>
        /// <returns></returns>

        public static bool UserHasReadAccess(
            string userName,
            UserObject[] uoArray)
        {
            bool approved = false;
            foreach (var uo in uoArray)
            {
                if (uo == null || uo.Owner == null || uo.Owner == "") approved = true; // no current owner

                if (Lex.Eq(uo.Owner, userName) ||
                 Security.IsAdministrator(userName) ||
                 Permissions.UserHasReadAccess(userName, uo))
                    approved = true;

                else return false; 
            }
            return approved;
        }

		/// <summary>
		/// See if the current user can modify the supplied object
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static bool UserHasWriteAccess(
			string userName,
			int objectId)
		{
			if (objectId <= 0) return true; // new unsaved object

			if (objectId == LastUserHasWriteAccessId)
				return LastUserHasWriteAccessResult;

			UserObject uo = UserObjectDao.ReadHeader(objectId);

			LastUserHasWriteAccessId = objectId;
			LastUserHasWriteAccessResult = UserObjectUtil.UserHasWriteAccess(uo);

			return LastUserHasWriteAccessResult;
		}

/// <summary>
/// See if the current user can modify the supplied object
/// </summary>
/// <param name="uo"></param>
/// <returns></returns>

		public static bool UserHasWriteAccess(
			UserObject uo)
		{
			return UserHasWriteAccess(SS.I.UserName, uo);
		}

/// <summary>
/// See if user can modify the supplied object
/// </summary>
/// <param name="uo"></param>
/// <returns></returns>

		public static bool UserHasWriteAccess(
			string userName,
			UserObject uo)
		{
			if (uo == null || uo.Owner == null || uo.Owner == "") return true; // no current owner

			if (Lex.Eq(uo.Owner, userName) ||
			 (Security.IsAdministrator(userName) && SS.I.AllowAdminFullObjectAccess) ||
			 Permissions.UserHasWriteAccess(userName, uo))
				return true;

			else return false;
		}

/// <summary>
/// See if user can modify the supplied object
/// </summary>
/// <param name="uo"></param>
/// <returns></returns>

        public static bool UserHasWriteAccess(
            string userName,
            UserObject[] uoArray)
        {
            bool approved = false;
            foreach (var uo in uoArray)
            {
                if (uo == null || uo.Owner == null || uo.Owner == "") approved = true; // no current owner

                if (Lex.Eq(uo.Owner, userName) ||
                 (Security.IsAdministrator(userName) && SS.I.AllowAdminFullObjectAccess) ||
                 Permissions.UserHasWriteAccess(userName, uo))
                    approved = true;

                else return false; 
            }
            return approved;
        }

		/// <summary>
		/// Get name part of owner qualified name
		/// </summary>
		/// <param name="InternalUserObjectName"></param>
		/// <returns></returns>

		public static string GetName(
			string InternalUserObjectName)
		{
			UserObject uo = ParseInternalUserObjectName(InternalUserObjectName);
			return uo.Name;
		}

		/// <summary>
		/// Get owner part of owner qualified name
		/// </summary>
		/// <param name="InternalUserObjectName"></param>
		/// <returns></returns>

		private static string GetOwner(
			string InternalUserObjectName)
		{
			UserObject uo = ParseInternalUserObjectName(InternalUserObjectName);
			return uo.Owner;
		}

		/// <summary>
		/// Change the owner of a UserObject
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string ChangeOwner(
			int objId,
			string newOwner)
		{
			newOwner = newOwner.ToUpper();

			UserObject uo = UserObjectDao.ReadHeader(objId);
			if (uo == null) return "User object not found: " + objId;
			if (uo.Owner == newOwner) return "This user object is already owned by " + newOwner;
			if (!Security.IsAdministrator(SS.I.UserName) && Lex.Ne(uo.Owner, SS.I.UserName))
				return "You're not authorized to change the owner of this user object";

			if (!Security.CanLogon(newOwner))
				return "Not a valid userId: " + newOwner;

			UserInfo ui = Security.GetUserInfo(newOwner);
			string msg = "Are you sure you want to transfer ownership of " + Lex.AddDoubleQuotes(uo.Name) + "\n" +
				"to " + ui.FullName + "?";
			DialogResult dr = MessageBoxMx.Show(msg, "Change Owner", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dr != DialogResult.Yes) return "";

			UserObject uo2 = uo.Clone();
			uo2.Owner = newOwner;
			UserObjectTree.GetValidUserObjectTypeFolder(uo2); // set valid parent folder
			Permissions.UpdateAclForNewOwner(uo2, uo.Owner, newOwner); // Set the ACL to give us r/w access
			uo2.Content = "ChangeOwner"; // indicate changing owner
			if (UserObjectDao.ReadHeader(uo2) != null) return "A user object with that name already exists for the specified new user";
			UserObjectDao.UpdateHeader(uo2, false, false);

			if (Data.InterfaceRefs.IUserObjectIUD != null) Data.InterfaceRefs.IUserObjectIUD.UserObjectDeleted(uo); // remove from view

			string newOwnerName = SecurityUtil.GetPersonNameReversed(newOwner);
			return "Ownership of \"" + uo2.Name + "\" has been changed to " + newOwnerName;
		}

		/// <summary>
		/// Get parent folder name part of owner qualified name
		/// </summary>
		/// <param name="InternalUserObjectName"></param>
		/// <returns></returns>

		private static string GetParentFolderName(
			string InternalUserObjectName)
		{
			UserObject uo = ParseInternalUserObjectName(InternalUserObjectName);
			return uo.ParentFolder;
		}

		/// <summary>
		/// Parse an internal user object name into a UserObject setting type
		/// </summary>
		/// <param name="name">objectType_objectId (e.g. "QUERY_456") or folderId.name (e.g. "FOLDER_123.My Query")</param>
		/// <param name="objType">UserObjectType</param>
		/// <returns></returns>

		public static UserObject ParseInternalUserObjectName(
			string objectName,
			UserObjectType objType)
		{
			return UserObject.ParseInternalUserObjectName(objectName, objType, SS.I.UserName);
		}

		/// <summary>
		/// Parse an internal user object name into a UserObject setting type
		/// </summary>
		/// <param name="name">objectType_objectId (e.g. "QUERY_456") or folderId.name (e.g. "FOLDER_123.My Query")</param>
		/// <param name="objType">UserObjectType</param>
		/// <returns></returns>

		public static UserObject ParseInternalUserObjectName(
			string objectName)
		{
			return UserObject.ParseInternalUserObjectName(objectName, SS.I.UserName);
		}

		/// <summary>
		/// Perform a deep clone of a UserObject
		/// </summary>
		/// <returns></returns>

		public static UserObject DeepClone(UserObject uo)
		{
			UserObject uo2;

			if (uo.Type == UserObjectType.Annotation) // assign new ids to annotation & copy data
				uo2 = AnnotationDao.DeepClone(uo);

			else
		{
				uo2 = (UserObject)uo.Clone();
				uo2.Id = -1; // no id assigned yet
			}

			return uo2;
		}

        /// <summary>
        /// Serialize an array of UserObjects
        /// </summary>
        /// <param name="uoArray"></param>
        /// <returns></returns>
        public static string Serialize(UserObject[] uoArray)
	    {
            StringBuilder stringBuilder = new StringBuilder("<UserObjects>");
            foreach (UserObject uo in uoArray)
	        {
                string serializedUo = uo.Serialize();
	            stringBuilder.Append(serializedUo);
	        }
            stringBuilder.Append("</UserObjects>");
            return stringBuilder.ToString();
	    }

        /// <summary>
        /// DeSerialize an array of UserObjects
        /// </summary>
        /// <param name="uoArray"></param>
        /// <returns></returns>
        public static UserObject[] Deserialize(string uoText)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(uoText);

            List<UserObject> userObjects = new List<UserObject>();

            XmlNodeList elemList = doc.GetElementsByTagName("UserObject");
            foreach (XmlElement element in elemList)
            {
                UserObject userObject = UserObject.Deserialize(element);
                userObjects.Add(userObject);
            }

            return userObjects != null ? userObjects.ToArray() : null;

        }

	}
}
