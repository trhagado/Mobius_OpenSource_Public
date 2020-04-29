using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.ClientComponents;

using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// This class contains static methods for manipulating tree of user objects.
	/// Each subtree is associated with a project and contains folders for public
	/// and private objects for each type.  The nodes in the tree are MetaTreeNode
	/// objects (not UserObjectNode or UserObjectTreeNode objects or some such).
	/// </summary>

	public class UserObjectTree
	{
		// FolderNodes contains one key for each projectId and user folder.
		// For projects, the corresponding value contains a MetaTreeNode with 
		// the set of Query, List, etc objects organized below it.
		// For user folders, the corresponding value may contain any type of
		// objects (or combination of types of objects) that the user desires.

		public static Dictionary<string, MetaTreeNode> FolderNodes = new Dictionary<string,MetaTreeNode>();

		internal static object BuildLock = new object();
		internal static Dictionary<string, MetaTreeNode> BuildNodes; // build area for nodes
		internal static bool BuildPublicObjectContentsTrees = true; // include public objects

		// For parallel retrieval of user objects

		public static bool BuildStarted
		{
			get { return UserObjectCollection.BuildStarted; }
			set { UserObjectCollection.BuildStarted = value; }
		}

		public static bool BuildComplete { 
			get { return UserObjectCollection.BuildComplete; } 
			set { UserObjectCollection.BuildComplete = value; }}

		public static int BuildStartTime = -1;

		public static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; } }

		public UserObjectTree()
		{
			return;
		}

		/// <summary>
		/// Reset UserObject state
		/// </summary>

		public static void Reset()
		{
			FolderNodes = null;
			MetaTableCollection.UserObjectTables = null;
			UserObjectCollection.Initialize();
			return;
		}

/// <summary>
/// Do a full synchronous build of the full user object tree
/// </summary>

		public static void BuildTree()
		{
			// Stats: 15-June-2010
			// -------------------
			// Objects: 10993
			// 2505 Folder
			// 53 UserDatabase
			// 3820 Query
			// 2243 CnList
			// 819 CalcField
			// 1517 Annotation
			// 36 MultiTable
			//
			// ~3mb data transferred
			// ~5 secs user clock time
			//
			// Takes about 7 secs (L2) to read in single sql statement (no data transfer to client)

			int t0 = TimeOfDay.Milliseconds();
			BuildTreeStart();
			BuildTreeWaitForCompletion();
			if (SessionManager.LogStartupDetails)
				ClientLog.TimeMessage("Parallel data retrieval wait time", t0);
			return;
		}


/// <summary>
/// Start asynch build of full tree
/// </summary>

		public static void BuildTreeStart()
		{
			BuildStartTime = TimeOfDay.Milliseconds();
			if (SessionManager.LogStartupDetails)
				ClientLog.Message("...");

			BuildNodes = new Dictionary<string, MetaTreeNode>();

			BuildPublicObjectContentsTrees = // allow speedup dev by reading public userobjects only
				ServicesIniFile.ReadBool("BuildPublicObjectContentsTrees", true);

			UserObjectCollection.Initialize();
			UserObjectCollection.BuildStarted = true;
			UserObjectCollection.BuildWaitEvent.Reset(); // not complete

			UserObjectType[] uoTypesToRead = {
				UserObjectType.Folder,
				UserObjectType.UserDatabase,
				UserObjectType.Query,
				UserObjectType.CnList,
				UserObjectType.CalcField,
				UserObjectType.CondFormat,
				UserObjectType.Annotation,
				UserObjectType.MultiTable,
				UserObjectType.SpotfireLink };

			UserObjectCollection.SubTreesToRead = uoTypesToRead.Length; // number of trees to read

			int t0 = TimeOfDay.Milliseconds();
			bool BuildAsynch = false; // set to false for debug
			if (BuildAsynch) // normal case
			{
				ParameterizedThreadStart pts = new ParameterizedThreadStart(ReadMultipleThreadMethod);
				foreach (UserObjectType uot in uoTypesToRead)
				{
					(new Thread(pts)).Start(uot);
				}
				
				if (SessionManager.LogStartupDetails)
					t0 = ClientLog.TimeMessage("Parallel thread setup/startup time", t0);
			}

			else // do synch for debug
			{
				foreach (UserObjectType uot in uoTypesToRead)
				{
					ReadMultipleThreadMethod(uot);
				}

				UserObjectCollection.BuildComplete = true;
			}

			return;
		}

		public static bool UoTypeRetrievalComplete(UserObjectType uoType)
		{
			if (UserObjectCollection.UserObjectsByType != null && UserObjectCollection.UserObjectsByType.ContainsKey(uoType))
				return true;
			else return false;
		}

		/// <summary>
		/// Wait for build to complete
		/// </summary>
		/// <returns></returns>

		public static bool WaitForBuildComplete(bool updateProgress)
		{
			if (BuildComplete) return true;

			if (updateProgress)
				Progress.Show("Completing the retrieval of the names of all Queries, Lists, Annotation Tables...");

			while (true)
			{
				if (updateProgress && Progress.CancelRequested)
				{
					Progress.Hide();
					return false;
				}

				if (UserObjectTree.BuildComplete) break;
				Thread.Sleep(100);
				Application.DoEvents();
			}
			if (updateProgress)
				Progress.Hide();

			return true;
		}

		/// <summary>
		/// Thread method to read objects of a single type
		/// </summary>
		/// <param name="uoType"></param>

		public static void ReadMultipleThreadMethod(object uoType)
		{
			List<UserObject> uoList = null;
			int t0 = TimeOfDay.Milliseconds();
			UserObjectType objType = (UserObjectType)uoType;

			try
			{
				bool includeContent = (objType == UserObjectType.CondFormat);
				uoList = UserObjectDao.ReadMultiple(objType, SS.I.UserName, BuildPublicObjectContentsTrees, includeContent);

				if (SessionManager.LogStartupDetails)
					ClientLog.TimeMessage("" + objType.ToString() + " count: " + uoList.Count + ", Retrieval Time (async): ", t0);

				if (uoList == null || uoList.Count == 0) uoList = uoList; // debug
			}
			catch (Exception ex)
			{
				ex = ex;
				//ClientLog.Message("Exception reading user objects (" + uoType.ToString() + "): " + ClientLog.FormatExceptionMessage(ex));
			}

			lock (UserObjectCollection.UserObjectsByType)
			{
				UserObjectCollection.UserObjectsByType.Add(objType, uoList);
				UserObjectCollection.SubtreesRead++;
			}

			if (UserObjectCollection.SubtreesRead < UserObjectCollection.SubTreesToRead) return; // just return if not all read

// If all types of objects are read then build the full tree of user objects

			lock (BuildLock)
			{
				BuildNodes = FolderNodes; // work from existing nodes (new may have been added)
				if (BuildNodes == null) 
					BuildNodes = new Dictionary<string, MetaTreeNode>();

				int folders = BuildUserFolderTree(); // user folders are needed first
				int ucdbs = BuildUcdbFolderTree(); // build before annotations that may go in these

				int queries = BuildLeafObjectTree(UserObjectType.Query);
				int lists = BuildLeafObjectTree(UserObjectType.CnList);
				int calcFields = BuildLeafObjectTree(UserObjectType.CalcField);
				int condFormats = BuildLeafObjectTree(UserObjectType.CondFormat);
				int annotations = BuildLeafObjectTree(UserObjectType.Annotation);
				int multitables = BuildLeafObjectTree(UserObjectType.MultiTable);
				int spotfireLinks = BuildLeafObjectTree(UserObjectType.SpotfireLink);

				FolderNodes = BuildNodes; // Replace original FolderNodes with completed BuildNodes

				UserObjectCollection.BuildWaitEvent.Set(); // indicate complete
				UserObjectCollection.BuildComplete = true;
				//SystemUtil.Beep(); // debug - finished loading user objects

				if (SessionManager.LogStartupDetails)
				{
					string msg =
						"Asynch build complete" +
						", folders=" + folders +
						", ucdbs=" + ucdbs +
						", queries=" + queries +
						", lists=" + lists +
						", calcFields=" + calcFields +
						", condFormats=" + condFormats +
						", annotations=" + annotations +
						", multitables=" + multitables +
						", spotfireLinks=" + spotfireLinks +
						", time=";
					ClientLog.TimeMessage(msg, BuildStartTime);
				}
			} // unlock nodes

			return;
		}

/// <summary>
/// Wait for asynch build of tree to complete
/// </summary>

		public static void BuildTreeWaitForCompletion()
		{
			UserObjectCollection.BuildWaitEvent.WaitOne();
			return;
		}

		/// <summary>
		/// Build the full tree of user folders so that MetaTreeNode objects exist for objects that
		/// live in folders to be children of.
		/// The user object headers for all visible user folders (currently public or owned by
		/// the current user) are retrieved.  The name of the parent folder as
		/// recorded in the user object header, is used to group
		/// </summary>

		static int BuildUserFolderTree()
		{
			int t0 = TimeOfDay.Milliseconds();

			List<UserObject> uoList = null; // the list of user object headers for visible folder objects
			if (UserObjectCollection.UserObjectsByType != null && UserObjectCollection.UserObjectsByType.ContainsKey(UserObjectType.Folder))
			{
				uoList = UserObjectCollection.UserObjectsByType[UserObjectType.Folder];
			}

			else
			{
				uoList = UserObjectDao.ReadMultiple(UserObjectType.Folder, SS.I.UserName, BuildPublicObjectContentsTrees, false);
				if (SessionManager.LogStartupDetails)
					ClientLog.TimeMessage("BuildFolderTree Time:", t0);
			}

			BuildFolderTree(uoList);

			return uoList != null ? uoList.Count : 0;
		}

/// <summary>
/// Build Ucdb folders
/// </summary>
/// <returns></returns>

		static int BuildUcdbFolderTree()
		{
			int t0 = TimeOfDay.Milliseconds();

			List<UserObject> uoList = null; // the list of user object headers for visible folder objects
			if (UserObjectCollection.UserObjectsByType != null && UserObjectCollection.UserObjectsByType.ContainsKey(UserObjectType.UserDatabase))
			{
				if (!UserObjectCollection.UserObjectsByType.ContainsKey(UserObjectType.UserDatabase)) // if no UOs of this type just return
					return 0;

				uoList = UserObjectCollection.UserObjectsByType[UserObjectType.UserDatabase];
			}
			else
			{
				uoList = UserObjectDao.ReadMultiple(UserObjectType.UserDatabase, SS.I.UserName, BuildPublicObjectContentsTrees, false);
				if (SessionManager.LogStartupDetails)
					ClientLog.TimeMessage("BuildUcdbFolderTree Time:", t0);
			}

			//foreach (UserObject uo0 in uoList) // debug
			//{
			//  if (uo0.Id == 366800) break;
			//}

			BuildFolderTree(uoList);

			return uoList != null ? uoList.Count : 0;
		}

		/// <summary>
		/// Retrieve user objects and build the user object sub tree for the specified folder node and add to current tree
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
			
		public static int RetrieveAndBuildSubTree(string folderName)
		{
			int t0 = TimeOfDay.Milliseconds();
			//SystemUtil.Beep();

			folderName = folderName.ToUpper();

			BuildPublicObjectContentsTrees = // allow speedup dev by reading public userobjects only
				ServicesIniFile.ReadBool("BuildPublicObjectContentsTrees", true);

			List<UserObject> uos = // get the set of objects for the supplied folder
				UserObjectDao.ReadMultiple(folderName, SS.I.UserName, BuildPublicObjectContentsTrees, false);

			//Dictionary<UserObjectType, int> otc = new Dictionary<UserObjectType, int>();
			//foreach (UserObject uo0 in uos)
			//{ // count how many of each type
			//  if (!otc.ContainsKey(uo0.Type)) otc[uo0.Type] = 1;
			//  else otc[uo0.Type]++;
			//}

			//foreach (UserObject uo0 in uos) if (uo0.Id == 49432) uos = uos; // debug

			lock (BuildLock)
			{
				if (FolderNodes == null) FolderNodes = new Dictionary<string, MetaTreeNode>();
				BuildNodes = FolderNodes;
				if (uos.Count > 0)
				{
					BuildFolderTree(uos);
					BuildLeafObjectTree(uos);
				}

				else // no user objects in folder, put a placeholder in the tree to indicate we've been here
				{
					MetaTreeNode mtn = new MetaTreeNode();
					mtn.Name = mtn.Target = folderName;
					mtn.Type = MetaTreeNodeType.Project;
					BuildNodes[folderName] = mtn;
				}

				FolderNodes = BuildNodes; // make active
			}

			if (SessionManager.LogStartupDetails)
				ClientLog.TimeMessage(folderName + ", objects = " + uos.Count + ", time = ", t0);

			return uos.Count;
		}

/// <summary>
/// Build user object folder tree
/// </summary>
/// <param name="uoList"></param>

		public static void BuildFolderTree(List<UserObject> uoList)
		{
			MetaTreeNode parentNode = null, userFolderNode;
			UserObject uo; // header info for node to build
			string parentFolderName, userFolderName;
			int i1;

			if (uoList == null) return;

			// Insert all of the nodes in the tree

			List<MetaTreeNode> nodes = new List<MetaTreeNode>();
			for (i1 = 0; i1 < uoList.Count; i1++)
			{
				uo = uoList[i1];

				//if (uo.Id == 366800 || uo.Id == 367220) uo = uo; // debug

				if (uo.Type != UserObjectType.Folder && uo.Type != UserObjectType.UserDatabase) 
					continue; // ignore all but folders & user databases

				userFolderName = uo.Type.ToString().ToUpper() + "_" + uo.Id; //i.e. "FOLDER_12345" or USERDATABASE_67890

				if (BuildNodes.ContainsKey(userFolderName)) // already exist?
				{
					userFolderNode = BuildNodes[userFolderName];
					if (userFolderNode.Parent == null) // if not linked to parent
					{ // add to list to link
						BuildUserFolderNode(userFolderNode, uo); // be sure values are copied
						nodes.Add(userFolderNode); 
					}
					else continue; // already linked to parent
					//ClientLog.Message("Folder node already exists in tree: " + userFolderName);
				}

				else // create the node
				{
					userFolderNode = new MetaTreeNode();
					BuildUserFolderNode(userFolderNode, uo); // be sure values are copied
					BuildNodes.Add(userFolderName, userFolderNode);
					nodes.Add(userFolderNode); // add to list to link
				}
			}

			// Link parents & children

			for (int ni = 0; ni < nodes.Count; ni++)
			{
				MetaTreeNode mtn = nodes[ni];
				//if (mtn.Name == "FOLDER_240174") nodes = nodes; // debug
				//if (mtn.Name == "USERDATABASE_366800") nodes = nodes; // debug
				parentFolderName = mtn.Target;
				mtn.Target = mtn.Name;
				if (BuildNodes.ContainsKey(parentFolderName)) // already seen the parent folder
					parentNode = BuildNodes[parentFolderName];
				else // must be in fixed system part of tree
					parentNode = AddSystemFolderNode(parentFolderName, BuildNodes);

				mtn.Parent = parentNode; // link to our parent

				string label = mtn.Label;
				if (mtn.Owner != SS.I.UserName)
					label += SecurityUtil.GetShortPersonNameReversed(mtn.Owner); // sort by owner

				for (i1 = 0; i1 < parentNode.Nodes.Count; i1++)
				{ // put in correct position
					MetaTreeNode mtn2 = parentNode.Nodes[i1];

					string label2 = mtn2.Label;
					if (mtn2.Owner != SS.I.UserName)
						label2 += SecurityUtil.GetShortPersonNameReversed(mtn2.Owner); // sort by owner

					if (String.Compare(label, label2, true) < 0) break;
				}

				parentNode.Nodes.Insert(i1, mtn);
			}

			return;
		}

/// <summary>
/// Set the values for a user folder or user database MetaTreeNode from a UserObject
/// </summary>
/// <param name="userFolderNode"></param>
/// <param name="uo"></param>

		static void BuildUserFolderNode (
			MetaTreeNode userFolderNode,
			UserObject uo)
		{
			userFolderNode.Type = UserObjectTypeToMetaTreeNodeType(uo.Type);

			string userFolderName = uo.Type.ToString().ToUpper() + "_" + uo.Id; //i.e. "FOLDER_12345" or USERDATABASE_67890
			userFolderNode.Name = userFolderName;

			userFolderNode.Target = uo.ParentFolder; // put parent folder in target temporarily
			userFolderNode.Shared = (uo.AccessLevel == UserObjectAccess.Public); // (correct?)
			userFolderNode.Label = uo.Name;
			userFolderNode.Owner = uo.Owner.ToUpper();

			return;
		}

		/// <summary>
		/// Build full object contents tree for an object type 
		/// </summary>
		/// <param name="objType">Type of user object to add to the current tree</param>

		static int BuildLeafObjectTree(
			UserObjectType objType)
		{

			List<UserObject> uoList = null;
			int t0 = TimeOfDay.Milliseconds();

			//			MetaTreeNodeType nodeType = UserObjectTree.GetMetaTreeNodeType(objType);

			//consider the existence of userObjectsByType to safely indicate that the user objects have been retrieved in parallel

			if (UserObjectCollection.UserObjectsByType != null && UserObjectCollection.UserObjectsByType.ContainsKey(objType))
			{
				if (!UserObjectCollection.UserObjectsByType.ContainsKey(objType))
					return 0; // nothing for object type
				uoList = UserObjectCollection.UserObjectsByType[objType];
			}
			else
			{
				bool includeContent = (objType == UserObjectType.CondFormat);

				uoList = UserObjectDao.ReadMultiple(objType, SS.I.UserName, BuildPublicObjectContentsTrees, includeContent);
				if (SessionManager.LogStartupDetails)
					ClientLog.TimeMessage(objType.ToString() + " Time:", t0);
			}

			BuildLeafObjectTree(uoList);

			return uoList!= null? uoList.Count : 0;
		}

/// <summary>
/// Add leaf UserObjects to tree
/// </summary>
/// <param name="uos"></param>

		static void BuildLeafObjectTree(
			List<UserObject> uoList)
		{
			UserObject uo;
			MetaTreeNode parentFolder = null, folderNode = null, myObjs = null, objNode;
			string curFolderName, curOwner;
			int uoi, pni;

			if (uoList == null) return;

			curFolderName = "";
			curOwner = "";
			for (uoi = 0; uoi < uoList.Count; uoi++)
			{
				uo = (UserObject)uoList[uoi];
				//if (uo.Id == 234482) uo = uo; // debug
				if (UserObject.IsFolderType(uo.Type)) continue;
				if (uo.ParentFolder == null || uo.ParentFolder == "") continue;
				if (uo.ParentFolderType == 0) continue; // don't include current/previous list, query, ...
				if (uo.ParentFolder != curFolderName || uo.Owner != curOwner) // going to new folder?
				{
					//if (uo.Id == 213094) uo = uo; // debug
					parentFolder = FindFolderNode(uo.ParentFolder, BuildNodes);
					if (parentFolder == null)
					{
						if (uo.ParentFolderType == FolderTypeEnum.System) // if parent is a system folder add it to the BuildNodes
							parentFolder = AddSystemFolderNode(uo.ParentFolder, BuildNodes);
						else
						{
							//ClientLog.Message("Failed to find parent folder " + uo.ParentFolder + " for user object " + uo.Id); // debug
							continue;
						}
					}

					curFolderName = uo.ParentFolder;
					curOwner = uo.Owner;
				}

				objNode = BuildNode(uo);

				for (pni = 0; pni < parentFolder.Nodes.Count; pni++)
				{ // check for dups
					if (parentFolder.Nodes[pni].Name == objNode.Name) break;
				}
				if (pni < parentFolder.Nodes.Count) continue;

				objNode.Owner = curOwner;

				objNode.Parent = parentFolder; // store under basic tree folder
				if (objNode.Type == MetaTreeNodeType.Annotation && objNode.Label.ToLower().Contains("structures"))
					parentFolder.Nodes.Insert(0, objNode); // put any chem structure annotation table first
				else parentFolder.Nodes.Add(objNode); // add to end

				if (objNode.IsFolderType && !BuildNodes.ContainsKey(objNode.Target))
					BuildNodes[objNode.Target] = objNode; // add to tree so items below will find
			}

			return;
		}

		/// <summary>
		/// Updates the access level on parent user folders so that they are public iff
		/// they contain one or more public user objects
		/// </summary>
		/// <param name="mtn">User object for which to try to cascade permissios upward</param>

		public static void CascadeAccess(MetaTreeNode mtn)
		{
			if (mtn == null) return;

			try
			{
				MetaTreeNode folderNode = ((mtn.Type == MetaTreeNodeType.UserFolder) ? mtn : mtn.Parent);
				if (mtn.Shared)
				{
					//public is really simple -- just cascade the public flag
					while (folderNode != null && folderNode.Type == MetaTreeNodeType.UserFolder)
					{
						//make this parent folder public

						// in the metatree -- cheaper to just update
						folderNode.Shared = true;

						// and in a persistent way -- check before updating
						string target = folderNode.Target;
						int id = Int32.Parse(target.Substring(target.LastIndexOf("_") + 1));
						UserObject parentUo = UserObjectDao.ReadHeader(id);
						if (parentUo != null)
						{
							if (parentUo.AccessLevel != UserObjectAccess.Public)
							{
								parentUo.AccessLevel = UserObjectAccess.Public;
								UserObjectDao.UpdateHeader(parentUo);
							}
						}

						//move up a folder
						folderNode = folderNode.Parent;
					}
				}
				else
				{
					//must be private
					//private is only slightly less simple -- climb, checking children for the public flag.
					// no public children means that it becomes private -- Other users get their
					// own copy of the folders when they create objects in them, so this is safe
					MetaTreeNode childNode;
					while (folderNode != null && folderNode.Nodes != null && folderNode.Type == MetaTreeNodeType.UserFolder)
					{
						//check this parent folder's children
						bool hasPublicChild = false;
						for (int i = 0; i < folderNode.Nodes.Count; i++)
						{
							childNode = folderNode.Nodes[i];
							if (childNode.Shared)
							{
								hasPublicChild = true;
								break;
							}
						}

						//since the child was public before, the parent chain should have been as well.
						//make private if necessary
						if (!hasPublicChild)
						{
							// make private in the metatree
							folderNode.Shared = false;

							// and in a persistent way
							string target = folderNode.Target;
							int id = -1;
							int startIdx = target.LastIndexOf("_") + 1;
							if (startIdx > 0 && int.TryParse(target.Substring(startIdx), out id))
							{
								UserObject parentUo = UserObjectDao.ReadHeader(id);
								if (parentUo != null && parentUo.AccessLevel != UserObjectAccess.Private)
								{
									parentUo.AccessLevel = UserObjectAccess.Private;
									UserObjectDao.UpdateHeader(parentUo);
								}
							}

							//move up a folder
							folderNode = folderNode.Parent;
						}
						else
						{
							//if this parent has at least one public child, then it must remain public
							//stop climbing
							break;
						}
					}
				}
			}
			catch (Exception e) { e = e; };
		}

		/// <summary>
		/// Add a newly created user object or user folder to the object contents tree
		/// </summary>
		/// <param name="uo"></param>

		public static MetaTreeNode AddObjectToTree(UserObject uo)
		{
			MetaTreeNode objNode, folderNode, pubSubfolderNode, objTypeFolderNode;

			string folderName = uo.ParentFolder;
			if (folderName == null || folderName == "") return null;

			UserObjectTree.RemoveObjectFromTree(uo); // remove any existing copy

			folderNode = FindFolderNode(folderName); //get the folder node
			if (folderNode == null) // if new user folder & parent folder not in tree add it
			{
				folderNode = AddSystemFolderNode(folderName);
				if (folderNode == null) throw new Exception("Can't find folder node: " + folderName);
			}

			objNode = AddObjectToTree(uo, folderNode); // add to object folder
			return objNode;
		}

		/// <summary>
		/// Add object to specific folder
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static MetaTreeNode AddObjectToTree(
			UserObject uo,
			MetaTreeNode parentNode)
		{
			MetaTreeNode objNode, on2;
			int compRes, i1;

			objNode = BuildNode(uo);
			if (FolderNodes.ContainsKey(objNode.Target.ToUpper()))
			{ // if objNode is a folder node that already exists use it
				objNode = FolderNodes[objNode.Target.ToUpper()];
			}

			for (i1 = 0; i1 < parentNode.Nodes.Count; i1++)
			{ // look under parent to check if node already exists
				on2 = parentNode.Nodes[i1];
				if (Lex.Eq(objNode.Name, on2.Name))
					return on2; // already exists
			}

			string label = objNode.Label;
			if (UserObjectTree.StoreInObjectTypeFolder(uo))
			{
				if (objNode.IsFolderType)
				{
					if (objNode.Type == MetaTreeNodeType.UserFolder)
					{
						//newly added folders need to be in the folder hash
						if (!FolderNodes.ContainsKey(objNode.Target.ToUpper()))
						{
							FolderNodes.Add(objNode.Target.ToUpper(), objNode);
						}
					}

					label = " " + label; // sort folders to top
					if (objNode.Owner != SS.I.UserName)
						label += objNode.Owner; // sort by owner
				}

				for (i1 = 0; i1 < parentNode.Nodes.Count; i1++)
				{ // put in correct position
					on2 = parentNode.Nodes[i1];

					string label2 = on2.Label;
					if (on2.IsFolderType)
					{
						label2 = " " + label2; // sort folders to top
						if (on2.Owner != SS.I.UserName)
							label2 += on2.Owner; // sort by owner
					}

					if (String.Compare(label, label2, true) < 0) break;
				}

				objNode.Parent = parentNode;
				parentNode.Nodes.Insert(i1, objNode);

				if (objNode.Shared)
				{
					CascadeAccess(objNode);
				}
			}

// Simple store of object under parent

			else
			{
				if (objNode.IsFolderType)
					FolderNodes.Add(objNode.Target.ToUpper(), objNode);

				objNode.Parent = parentNode;
				parentNode.Nodes.Add(objNode);
			}

			return objNode;
		}

		/// <summary>
		/// Remove an object from all folders
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static void RemoveObjectFromTree(
			UserObject uo)
		{
			MetaTreeNode folderNode, pubSubfolderNode, objTypeFolderNode;

			//get the folder node
			string folderName = uo.ParentFolder;
			MetaTreeNode parentFolder = FindFolderNode(folderName);
			if (parentFolder == null) return;
			//{
			//  //how did the user ever see this to delete it?!?
			//  throw new Exception("Failed to find folder from which to remove object!");
			//}

			//If the object was moved, then the folder id obtained from the user object isn't valid
			//If we fail to find the node to remove under the parent, then it's probably a move
			// Do a full search to find the node!
			bool foundNode = false, isMove = false;
			//there should only be one occurrence of the user object in the metatree for a
			// private object.  public copies must be cleaned up also
			MetaTreeNode nodeToRemove = BuildNode(uo);
			for (int i = 0; i < parentFolder.Nodes.Count; i++) //ok to loop this way ONLY because we stop after the first match
			{
				MetaTreeNode currentNode = parentFolder.Nodes[i];
				if (currentNode.Target.ToUpper() == nodeToRemove.Target.ToUpper())
				{
					nodeToRemove = currentNode;
					parentFolder.Nodes.RemoveAt(i);
					foundNode = true;
					break;
				}
			}

			//if it's a move, we'll need to check all folders until we find the node
			if (!foundNode)
			{
				nodeToRemove = GetNodeByTarget(nodeToRemove.Target);
				if (nodeToRemove != null)
				{
					nodeToRemove.Parent.Nodes.Remove(nodeToRemove);
					//note the parent folder so that we can remove the public copy (if any) later
					parentFolder = nodeToRemove.Parent;
					foundNode = true;
				}
			}

			if (!foundNode)
			{
				//since remove gets double-called in an update, this may not be an error.  Shortcut out of here...
				//MessageBoxMx.ShowError("Error finding a node that needed to be removed!");
				return;
			}

			//remove object from label path and folder id hashes (if present)
			if (nodeToRemove.Type == MetaTreeNodeType.UserFolder)
			{
				if (FolderNodes.ContainsKey(nodeToRemove.Target.ToUpper()))
				{
					FolderNodes.Remove(nodeToRemove.Target.ToUpper());
				}
			}

			else if (nodeToRemove.IsFolderType)
			{
				if (FolderNodes.ContainsKey(nodeToRemove.Target.ToUpper()))
					FolderNodes.Remove(nodeToRemove.Target.ToUpper());
			}

			return;
		}

		/// <summary>
		/// Find a userobject of the specfied type and id in BuildState dictionary
		/// </summary>
		/// <param name="uoType"></param>
		/// <param name="objId"></param>
		/// <returns></returns>

		public UserObject FindUserObjectInBuildState(
			UserObjectType uoType,
			int objId)
		{
			WaitForUoTypeRetrievalComplete(uoType, true);
			if (!UserObjectCollection.UserObjectsByType.ContainsKey(uoType))
				throw new Exception("Expected type not found: " + uoType);

			List<UserObject>  uoList = UserObjectCollection.UserObjectsByType[uoType];
			foreach (UserObject uo in uoList)
			{
				if (uo.Id == objId) return uo;
			}

			return null;
		}

		/// <summary>
		/// Wait for the retrieval of a particular type to complete
		/// </summary>
		/// <param name="uoType"></param>
		/// <param name="updateProgress"></param>
		/// <returns></returns>

		public static List<UserObject> WaitForUoTypeRetrievalComplete(
			UserObjectType uoType,
			bool updateProgress)
		{
			if (UserObjectTree.UoTypeRetrievalComplete(uoType)) return UserObjectCollection.UserObjectsByType[uoType];

			if (updateProgress)
				Progress.Show("Completing the retrieval of the " + UserObject.GetTypeLabelPlural(uoType) + "...");

			while (true)
			{
				if (updateProgress && Progress.CancelRequested)
				{
					Progress.Hide();
					return null;
				}

				if (UserObjectTree.UoTypeRetrievalComplete(uoType)) break;
				Thread.Sleep(100);
				Application.DoEvents();
			}
			if (updateProgress)
				Progress.Hide();

			return UserObjectCollection.UserObjectsByType[uoType];
		}


		/// <summary>
		/// Find the specified folder node without user folder merging
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>

		public static MetaTreeNode FindFolderNode(
			string folderName)
		{
			return FindFolderNode(folderName, FolderNodes, false);
		}

/// <summary>
/// Find the specified folder node
/// </summary>
/// <param name="folderName"></param>
/// <param name="mergeUserObjectFolders"></param>
/// <returns></returns>

		public static MetaTreeNode FindFolderNode(
			string folderName,
			bool mergeUserObjectFolders)
		{
			return FindFolderNode(folderName, FolderNodes, mergeUserObjectFolders);
		}

/// <summary>
/// Find the specified folder node
/// </summary>
/// <param name="folderName"></param>
/// <param name="folderNodes"></param>
/// <returns></returns>

		public static MetaTreeNode FindFolderNode(
			string folderName,
			Dictionary<string, MetaTreeNode> folderNodes)
		{
			return FindFolderNode(folderName, folderNodes, false);
		}

/// <summary>
/// Find the specified folder node
/// </summary>
/// <param name="folderName">Folder to find: user, system or virtual merged folder</param>
/// <param name="folderNodes">Dictionary of nodes to build</param>
/// <param name="mergeUserObjectFolders">If true do merging of standard top level user object folders</param>
/// <returns></returns>

		public static MetaTreeNode FindFolderNode(
			string folderName,
			Dictionary<string, MetaTreeNode> folderNodes,
			bool mergeUserObjectFolders)
		{
			MetaTreeNode objTypeFolderNode = null, tertiaryLevelNode = null, systemFolderNode = null, mNode;
			string unmergedNodeName, objTypeString, systemNodeName, typeNodeName, m1, m2;
			int i1, i2;

			//			if (Lex.Eq(folderName,"VALIDATIONQUERIES")) folderName = folderName; // debug

			if (folderNodes == null ||
				folderName == null || folderName == "")
				return null;

			folderName = folderName.ToUpper();

// Merge any standard top level user object folders into a virtual node

			if (folderNodes.ContainsKey(folderName)) 
			{
				MetaTreeNode folderNode = folderNodes[folderName];

				//if (folderNode != null && Lex.IsUndefined(folderNode?.Target)) folderNode.Target = folderNode.Name; // fixup

				if (!mergeUserObjectFolders) return folderNode;

				mNode = BuildMergedStandardUserObjectFolders(folderNode);
				if (mNode != null) return mNode;
				else return folderNode;
			}

// Merge at the virtual folder level

			else // if this is a merged virtual folder then generate merged virtual nodes
			{ // or return the system folder if no merging is necessary
				if (!mergeUserObjectFolders) return null;

				m1 = "MERGED_";
				m2 = "MERGED_PUBLIC_";

				if (!folderName.StartsWith(m1)) return null;

				if (folderName.StartsWith(m2)) i1 = m2.Length;
				else i1 = m1.Length;

				unmergedNodeName = folderName.Substring(i1);
				i1 = unmergedNodeName.IndexOf("_");
				objTypeString = unmergedNodeName.Substring(0, i1);
				systemNodeName = unmergedNodeName.Substring(i1 + 1);
				typeNodeName = m1 + objTypeString + "_" + systemNodeName;

				if (!folderNodes.ContainsKey(systemNodeName)) // in case of an unexpected merged node name
				{
					DebugLog.Message("Node " + systemNodeName + " not found in folderNodes dictionary (count = " + folderNodes.Count + ")");
					return null;
				}

				mNode = BuildMergedStandardUserObjectFolders(folderNodes[systemNodeName]);
				if (mNode == null) return null;

				for (i1 = 0; i1 < mNode.Nodes.Count; i1++)
				{ // find node of the specified type
					objTypeFolderNode = mNode.Nodes[i1];
					if (Lex.Eq(objTypeFolderNode.Name, typeNodeName)) break;
				}

				if (i1 >= mNode.Nodes.Count) throw new Exception("Node not found: " + typeNodeName);

				if (!folderName.StartsWith(m2))
				{ // virtual secondary level merged folder - public object type folder (if exists) + private objects (if exist)
					return objTypeFolderNode;
				}

				else // virtual tertiary level merged folder - one entry for each public object & folder
				{
					for (i1 = 0; i1 < objTypeFolderNode.Nodes.Count; i1++)
					{
						tertiaryLevelNode = objTypeFolderNode.Nodes[i1];
						if (Lex.Eq(tertiaryLevelNode.Name, folderName)) break;
					}

					if (i1 < objTypeFolderNode.Nodes.Count) return tertiaryLevelNode;
					else throw new Exception("Node not found: " + typeNodeName);
				}

			}
		}


/// <summary>
/// Merge standard user object folders
/// </summary>
/// <param name="folderNode"></param>
/// <returns></returns>

		static MetaTreeNode BuildMergedStandardUserObjectFolders(MetaTreeNode folderNode)
		{

			// If not merging then simply lookup folderName in folderNodes.
			// If merging then transform at three levels below the system node
			// 1. For the top level below a system node merge by userFolder name for the 
			//		standard object types (Query, List, Annotation & Calculated Field)
			//    The virtual node created has a name of the form MERGED_<folderName> and its children
			//    have names like MERGED_<objectTypeName>_<folderName>
			// 2. Secondary level - Create a virtual node containing a virtual public folder for the 
			//    object type, a public folder for the object type if public objects exist for other users
			//    and any private objects in the the owners folder
			// 3. Teriary level - Return a list of merged public objects identified by owner.

			MetaTreeNode objTypeFolder = null, pubObjTypeFolder = null, mtn;
			int i1;

			List<MetaTreeNode> mNodes = new List<MetaTreeNode>(); // virtual folder with merged nodes (may contain unmerged nodes)
			Dictionary<string, MetaTreeNode> motfs = new Dictionary<string, MetaTreeNode>(); // the merged node for each object type
			bool merged = false;

			//if (folderNode.Name == "USERDATABASE_234480") folderNode = folderNode; // debug

// Merge folders for mergable types (i.e. Queries, Lists...)

			for (int ni = 0; ni < folderNode.Nodes.Count; ni++)
				{
					MetaTreeNode mtn0 = folderNode.Nodes[ni];
					//if (mtn0.Label == "TestUcdbWithAnnot") mtn0.Label = mtn0.Label; // debug
					//if (mtn0.Name == "USERDATABASE_234480") mtn0.Label = mtn0.Label; // debug
					UserObjectType uot = UserObject.GetTypeFromPlural(mtn0.Label);
					if (mtn0.Type == MetaTreeNodeType.UserFolder && uot != UserObjectType.Unknown)
					{ // this is a user folder of one of the mergable types (i.e. Queries, Lists...)
						string otfName = "MERGED_" + mtn0.Label.ToUpper() + "_" + folderNode.Name.ToUpper(); // object type virtual folder name
						string otpfName = "MERGED_PUBLIC_" + mtn0.Label.ToUpper() + "_" + folderNode.Name.ToUpper(); // object type public objects virtual folder name
						if (!motfs.ContainsKey(otfName))
						{ // create new dict entry for object type
							objTypeFolder = new MetaTreeNode();
							objTypeFolder.Type = MetaTreeNodeType.UserFolder;
							objTypeFolder.Name = otfName;
							objTypeFolder.Label = mtn0.Label;
							objTypeFolder.Target = mtn0.Target; // set target to allow positioning to this node
							objTypeFolder.Parent = folderNode;
							motfs[objTypeFolder.Name] = objTypeFolder;
							mNodes.Add(objTypeFolder);
						}

						else merged = true; // say merged if two or more of any standard object type folder

						// Put personal objects in object type folder

						if (Lex.Eq(mtn0.Owner, SS.I.UserName))
						{ // if current user then add all objects, if any, to tree
							for (int ni2 = 0; ni2 < mtn0.Nodes.Count; ni2++)
							{
								MetaTreeNode mtn00 = mtn0.Nodes[ni2];
								//if (mtn00.Label == "User Folder 2") mtn00.Label = mtn00.Label; // debug
								int f00 = mtn00.IsFolderType ? 0 : 1;
								for (i1 = 0; i1 < objTypeFolder.Nodes.Count; i1++)
								{ // insert in proper position
									mtn = objTypeFolder.Nodes[i1];
									int f = mtn.IsFolderType ? 0 : 1;
									if (f00 < f) break; // put folders first
									else if (f00 > f) continue;
									else if (Lex.Le(mtn00.Label, mtn.Label)) break;
								}

								if (i1 < objTypeFolder.Nodes.Count)
									objTypeFolder.Nodes.Insert(i1, mtn00);
								else objTypeFolder.Nodes.Add(mtn00);
							}
						}

						// Add shared object to shared folder

						pubObjTypeFolder = null;
						if (objTypeFolder.Nodes.Count > 0 && objTypeFolder.Nodes[0].Name == otpfName)
							pubObjTypeFolder = objTypeFolder.Nodes[0];

						for (int ni2 = 0; ni2 < mtn0.Nodes.Count; ni2++)
						{
							MetaTreeNode mtn00 = mtn0.Nodes[ni2];
							if (!mtn00.Shared) continue;
							if (pubObjTypeFolder == null)
							{ // create public folder
								pubObjTypeFolder = new MetaTreeNode();
								pubObjTypeFolder.Type = MetaTreeNodeType.UserFolder;
								pubObjTypeFolder.Name = otpfName;
								pubObjTypeFolder.Label = "Shared " + mtn0.Label;
								pubObjTypeFolder.Parent = folderNode;
								objTypeFolder.Nodes.Insert(0, pubObjTypeFolder); // put first
							}

							int f00 = mtn00.IsFolderType ? 0 : 1;
							for (i1 = 0; i1 < pubObjTypeFolder.Nodes.Count; i1++)
							{ // insert in proper position
								mtn = pubObjTypeFolder.Nodes[i1];
								int f = mtn.IsFolderType ? 0 : 1;
								if (f00 < f) break; // put folders first
								else if (f00 > f) continue;
								else if (Lex.Le(mtn00.Label, mtn.Label)) break;
							}

							if (i1 < pubObjTypeFolder.Nodes.Count)
								pubObjTypeFolder.Nodes.Insert(i1, mtn00);
							else pubObjTypeFolder.Nodes.Add(mtn00);
						}
					}

					else mNodes.Add(mtn0); // just add as is if not not mergable
				}

			//if (!merged) return null; // return null if not merged (no, this causes nothing to show)

			//else // return top level merged virtual folder
			//{
				folderNode = folderNode.Clone();
				folderNode.Name = "MERGED_" + folderNode.Name.ToUpper();
				folderNode.Nodes = mNodes;
				return folderNode;
			//}
		}

		/// <summary>
		/// Lookup any subfolder that contains objects of the specified type owned by the current user 
		/// </summary>
		/// <param name="parentFolderName"></param>
		/// <param name="objType"></param>
		/// <returns></returns>

		public static MetaTreeNode FindObjectTypeSubfolder(
			string parentFolder,
			UserObjectType objType,
			string userName)
		{
			MetaTreeNode parentNode = FindFolderNode(parentFolder);
			if (parentNode == null) return null;

			if (parentNode.Type == MetaTreeNodeType.UserFolder)
				return parentNode; // since it was a user folder just return it without consideration of type

			string nodeLabel = UserObject.GetTypeLabelPlural(objType);
			string nodeOwner = userName;
			for (int i = 0; i < parentNode.Nodes.Count; i++)
			{
				MetaTreeNode childNode = parentNode.Nodes[i];
				if ((Lex.Eq(childNode.Owner, userName) || Lex.Eq(childNode.Owner, SS.I.UserInfo.FullName)) &&
					Lex.Eq(childNode.Label, nodeLabel))
					return childNode;
			}

			return null;
		}

		/// <summary>
		/// Find a node by node name within the full set of user objects
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNodeByName(	string name)
		{
			MetaTreeNode node = null;

			Dictionary<string, Mobius.Data.MetaTreeNode>.ValueCollection fnList = FolderNodes?.Values;

			if (fnList == null) return null;

			foreach (MetaTreeNode folder in fnList)
			{
				node = UserObjectTree.GetNode(folder, name);
				if (node != null) break;
			}

			return node;
		}

		/// <summary>
		/// Find a node by target within the specified folder
		/// </summary>
		/// <param name="parentFolder"></param>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNode(
			MetaTreeNode parentNode,
			string name)
		{
			MetaTreeNode childNode = null, currentNode;
			for (int i = 0; i < parentNode.Nodes.Count; i++)
			{
				currentNode = parentNode.Nodes[i];

				if (Lex.Eq(currentNode.Name, name))
				{
					childNode = currentNode;
					break;
				}
				if (currentNode.Nodes.Count > 0)
				{
					childNode = GetNode(currentNode, name);
					if (childNode != null)
					{
						break;
					}
				}
			}
			return childNode;
		}

		/// <summary>
		/// Find a node by target within the full set of user objects
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNodeByTarget(
			string name)
		{
			MetaTreeNode node = null;

			if (Lex.IsUndefined(name)) return null;

			Dictionary<string, Mobius.Data.MetaTreeNode>.ValueCollection fnList = FolderNodes?.Values;

			if (fnList == null) return null;

			foreach (MetaTreeNode folder in fnList)
			{
				node = UserObjectTree.GetNodeByTarget(folder, name);
				if (node != null) break;
			}

			return node;
		}

		/// <summary>
		/// Find a node by target within the specified folder
		/// </summary>
		/// <param name="parentFolder"></param>
		/// <param name="name"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNodeByTarget(
			MetaTreeNode parentNode,
			string target)
		{
			MetaTreeNode childNode = null, currentNode;
			for (int i = 0; i < parentNode.Nodes.Count; i++)
			{
				currentNode = parentNode.Nodes[i];

				//				if (currentNode.Target.ToUpper().StartsWith("ANNOTATION_"))	childNode = childNode; // debug
				//				ClientLog.Message(currentNode.Target); // debug

				if (Lex.Eq(currentNode.Target, target))
				{
					childNode = currentNode;
					break;
				}
				if (currentNode.Nodes.Count > 0)
				{
					childNode = GetNodeByTarget(currentNode, target);
					if (childNode != null)
					{
						break;
					}
				}
			}
			return childNode;
		}

		/// <summary>
		/// Find a node by it's label (i.e. UserObject Name) within the specified folder
		/// </summary>
		/// <param name="parentFolder"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		public static MetaTreeNode GetNodeByLabel(
			MetaTreeNode parentNode,
			MetaTreeNodeType nodeType,
			string label)
		{
			MetaTreeNode childNode = null, currentNode;
			for (int i = 0; i < parentNode.Nodes.Count; i++)
			{
				currentNode = parentNode.Nodes[i];

				if (currentNode.Type == nodeType &&
					Lex.Eq(currentNode.Label, label))
				{
					childNode = currentNode;
					break;
				}
				if (currentNode.Nodes.Count > 0)
				{
					childNode = GetNodeByLabel(currentNode, nodeType, label);
					if (childNode != null)
					{
						break;
					}
				}
			}
			return childNode;
		}

		/// <summary>
		/// Refresh the subtree containing the specified user object
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="treeCtl"></param>

		public static void RefreshSubtreeInTreeControl(
				UserObject uo,
				ContentsTreeControl treeCtl)
		{
			if (!FolderNodes.ContainsKey(uo.ParentFolder)) return;
			MetaTreeNode mtn = FolderNodes[uo.ParentFolder]; // node containing the user object
			treeCtl.RefreshSubtree(mtn.Target);
			return;
		}

		/// <summary>
		/// Update the view of a UserObject. May have moved to different subtree
		/// </summary>
		/// <param name="currentUo"></param>
		/// <param name="newUo"></param>
		/// <param name="treeCtl"></param>

		public static void UpdateObjectInTreeControl(
			UserObject currentUo,
			UserObject newUo,
			ContentsTreeControl treeCtl)
		{
			RefreshSubtreeInTreeControl(currentUo, treeCtl);
			RefreshSubtreeInTreeControl(newUo, treeCtl);
			return;
		}

		/// <summary>
		/// Convert the UserObject ParentFolder into a proper folder that can hold user objects
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>Parent folder node in UserObjectTree</returns>

		public static MetaTreeNode GetValidUserObjectTypeFolder(UserObject uo)
		{
			MetaTreeNode pNode = null;

			if (String.IsNullOrEmpty(uo.Owner))
				uo.Owner = SS.I.UserName;

			if (String.IsNullOrEmpty(uo.Name))
				throw new Exception("UserObject name not defined");

			if (uo.Type == UserObjectType.Unknown)
				throw new Exception("UserObject type not defined");

			if (String.IsNullOrEmpty(uo.ParentFolder)) // need to assign folder?
			{ // use default if not supplied
				uo.ParentFolder = SS.I.PreferredProjectId;
				uo.ParentFolderType = FolderTypeEnum.System;
			}

			// User objects (other than folders) should not be direct children of project folders
			// We need a user folder that the current user has write access to

			WaitForBuildComplete(true); // be sure tree completely built before proceeding

			pNode = UserObjectTree.FindFolderNode(uo.ParentFolder);

			while (pNode != null && pNode.Type == MetaTreeNodeType.UserFolder && 
				pNode.Owner != uo.Owner && !Permissions.UserHasWriteAccess(uo.Owner, pNode.Name))
			{ // if parent folder is a user folder but user isn't owner & user can't write then shift up to a node the user can write to
				pNode = pNode.Parent; 
				uo.ParentFolder = pNode.Name;
			}

			if ((pNode != null && // if found &
			 pNode.Type == MetaTreeNodeType.UserFolder) || // parent folder is a user folder
			 uo.Type == UserObjectType.Folder) // or this object is a user folder
			{ } // parent is ok

// See if appropriate user object folder already exists

			else
			{
				if (pNode == null) // not in user object tree
				{ // should be able to get from main tree & add to user obj tree
					pNode = UserObjectTree.AddSystemFolderNode(uo.ParentFolder);
					if (pNode == null) throw new Exception("Expected parent node not found: " + uo.ParentFolder);
				}

				if (pNode.Type != MetaTreeNodeType.UserFolder &&
				 uo.Type != UserObjectType.Folder)
				{ // need to find or create user object folder to contain this object
					AssignObjectTypeFolder(uo);
					pNode = UserObjectTree.FindFolderNode(uo.ParentFolder);
					if (pNode == null) throw new Exception("Expected parent node not found: " + uo.ParentFolder);
				}
			}

			uo.ParentFolder = pNode.Target.ToUpper();
			uo.ParentFolderType = FolderTypeEnum.User;

			//if (uo.Owner != pNode.Owner) // try to catch accidental changed ownership bug
			//{
			//  string msg =
			//    "UserObject folder is owned by another user:\r\n" + 
			//    "Id: " + uo.Id + ", Obj Owner: " + uo.Owner + ", Folder: " + uo.ParentFolder + ", Folder Owner: " + pNode.Owner + "\r\n" + 
			//    new StackTrace(true).ToString();

			//  CommandLine.SendCriticalEventNotificationToMobiusAdmin("User Object Ownership Change Bug", msg);
			//  ServicesLog.Message(msg);
			//}

			return pNode;
		}

/// <summary>
/// Get a valid user object type folder for a user other than the current user
/// </summary>

		public static UserObject GetValidUserObjectTypeFolder(string owner, UserObjectType type)
		{
			string parentFolder = UserObjectDao.GetUserParameter(owner, "PreferredProject", "DEFAULT_FOLDER");
			string folderName = UserObject.GetTypeLabelPlural(type);

			UserObject uo = UserObjectDao.ReadHeader(UserObjectType.Folder, owner, parentFolder, folderName);
			if (uo != null) return uo; // just return it if exists

			int newFolderId = UserObjectDao.GetNextId();
			uo = new UserObject();
			uo.Id = newFolderId;
			uo.Type = UserObjectType.Folder;
			uo.Owner = owner;
			uo.ParentFolder = parentFolder;
			uo.ParentFolderType = FolderTypeEnum.System;
			uo.AccessLevel = UserObjectAccess.Private;
			uo.Name = folderName;

			UserObjectDao.Write(uo, newFolderId, false); // create user object but don't add to tree
			return uo;
		}

		/// <summary>
		/// Create a new system folder (project, etc) root node for a project
		/// </summary>
		/// <param name="folderName">Name of the system folder to add</param>
		/// <returns></returns>

		public static MetaTreeNode AddSystemFolderNode(
			string folderName)
		{
			return AddSystemFolderNode(folderName, FolderNodes);
		}

/// <summary>
/// Create a new system folder (project, etc) root node for a project
/// </summary>
/// <param name="folderName"></param>
/// <param name="nodes"></param>
/// <returns></returns>

		public static MetaTreeNode AddSystemFolderNode(
			string folderName,
			Dictionary<string, MetaTreeNode> nodes)
		{
			MetaTreeNode node;

			//if (Lex.Eq(folderName, "leadlike_models")) folderName = folderName; // debug

			if (folderName == null || folderName == "") return null;
			node = new MetaTreeNode();
			node.Name = folderName;
			node.Target = folderName;
			node.Type = MetaTreeNodeType.Project;

			MetaTreeNode assocTreeNode = MetaTree.GetNode(folderName);
			if (assocTreeNode != null)
			{
				node.Label = assocTreeNode.Label;
				node.Type = assocTreeNode.Type;
			}

			nodes.Add(folderName, node);
			return node;
		}

		/// <summary>
		/// Create a user folder of the specified type
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="objType"></param>
		/// <returns></returns>

		public static MetaTreeNode CreateUserFolderObjectAndNode(
			MetaTreeNode parentNode, 
			UserObjectType objType,
			string userName)
		{
			return CreateUserFolderObjectAndNode(parentNode, UserObject.GetTypeLabelPlural(objType), userName);
		}

		/// <summary>
		/// Create a new user folder 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="label"></param>
		/// <returns></returns>

		public static MetaTreeNode CreateUserFolderObjectAndNode(
			MetaTreeNode parentNode, 
			string label,
			string userName)
		{
			int newFolderId = UserObjectDao.GetNextId();
			UserObject newFolderUo = new UserObject();
			newFolderUo.Id = newFolderId;
			newFolderUo.Type = UserObjectType.Folder;
			newFolderUo.Owner = userName;
			newFolderUo.ParentFolder = parentNode.Target.ToUpper();
			newFolderUo.ParentFolderType = parentNode.GetUserObjectFolderType();
			newFolderUo.AccessLevel = UserObjectAccess.Private;
			newFolderUo.Name = label;

			UserObjectDao.Write(newFolderUo, newFolderId); // create user object, gets added to tree

			string folderName = "FOLDER_" + newFolderId;
			MetaTreeNode folderNode = UserObjectTree.FindFolderNode(folderName);
			if (folderNode == null) throw new Exception("New folder node not found: " + folderName);

			return folderNode;
		}

		/// <summary>
		/// Build a MetaTreeNode for the supplied user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static MetaTreeNode BuildNode(
			UserObject uo)
		{
			MetaTreeNode objNode = new MetaTreeNode();
			objNode.Type = UserObjectTypeToMetaTreeNodeType(uo.Type);
			objNode.Label = uo.Name;
			objNode.Name = (uo.Type + "_" + uo.Id).ToUpper();
			objNode.Target = objNode.Name;

			objNode.Shared = Permissions.IsShared(uo);
			objNode.Owner = uo.Owner;

			if (objNode.IsLeafType && SS.I.ShowMetaTableStats)
			{ // add object date & size if requested
				objNode.UpdateDateTime = uo.UpdateDateTime;
				if (uo.Type == UserObjectType.Annotation || // display size for these types only
					uo.Type == UserObjectType.Query ||
					uo.Type == UserObjectType.CnList)
					objNode.Size = uo.Count;
			}

			return objNode;
		}

		/// <summary>
		/// Get the tree node type associated with an object type
		/// </summary>
		/// <param name="uot"></param>
		/// <returns></returns>

		public static MetaTreeNodeType UserObjectTypeToMetaTreeNodeType(
			UserObjectType uot)
		{
			if (uot == UserObjectType.Query) return MetaTreeNodeType.Query;
			else if (uot == UserObjectType.CnList) return MetaTreeNodeType.CnList;
			else if (uot == UserObjectType.CalcField) return MetaTreeNodeType.CalcField;
			else if (uot == UserObjectType.CondFormat) return MetaTreeNodeType.CondFormat;
			else if (uot == UserObjectType.Annotation) return MetaTreeNodeType.Annotation;
			else if (uot == UserObjectType.UserDatabase) return MetaTreeNodeType.Database;
			else if (uot == UserObjectType.Folder) return MetaTreeNodeType.UserFolder;
			else if (uot == UserObjectType.MultiTable) return MetaTreeNodeType.Query; // multitable is just another name for a query that is used differently
			else if (uot == UserObjectType.SpotfireLink) return MetaTreeNodeType.ResultsView;
			else if (uot == UserObjectType.UserDatabase) return MetaTreeNodeType.Database; // user database looks like database
			else return 0;
		}


		/// <summary>
		/// User object types that appear in main contents menu
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		public static bool IsMainFormContentsType(
			UserObjectType type)
		{
			if (type == UserObjectType.Query ||
				type == UserObjectType.CnList ||
				type == UserObjectType.CalcField ||
				type == UserObjectType.CondFormat ||
				type == UserObjectType.Annotation ||
				type == UserObjectType.UserDatabase ||
				type == UserObjectType.MultiTable ||
				type == UserObjectType.SpotfireLink ||
				type == UserObjectType.Folder)
				return true;
			else return false;
		}

		/// <summary>
		/// Return true if user objects of the specified type should be grouped into a 
		/// containing public/private folder.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		public static bool StoreInObjectTypeFolder(
			UserObject uo)
		{
			if (uo.Type == UserObjectType.UserDatabase)
				return false; // user database items not stored in object type folders

			else if (Lex.StartsWith(uo.ParentFolder, "USERDATABASE_"))
				return false; // items in user database folders not stored in object type folders

			else return true;
		}

		/// <summary>
		/// Add or remove data source identifiers in metatree nodes (not necessary)
		/// </summary>
		/// <param name="showSource"></param>

		public static void ShowHideDataSource(
			bool showSource)
		{
			int t0 = TimeOfDay.Milliseconds();

			MetaTreeNode[] mtna = new MetaTreeNode[FolderNodes.Count];
			FolderNodes.Values.CopyTo(mtna, 0); 
			for (int ni = 0; ni < mtna.Length; ni++)
			{
				MetaTreeNode projectRoot = mtna[ni];
				RecursiveShowDataSource(projectRoot, showSource);
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		public static void RecursiveShowDataSource(
			MetaTreeNode n,
			bool showSource)
		{
			for (int ni = 0; ni < n.Nodes.Count; ni++)
			{
				MetaTreeNode mtn = n.Nodes[ni];
				string source = null;
				if (mtn.IsFolderType && mtn.Nodes.Count > 0)
				{
					RecursiveShowDataSource(mtn, showSource);
					continue;
				}

				else if (mtn.Type == MetaTreeNodeType.Annotation) source = "Note";
				else if (mtn.Type == MetaTreeNodeType.CalcField) source = "Calc";
				else continue;

				source = " (" + source + ")";
				if (showSource)
				{
					if (!Lex.Contains(mtn.Label, source)) mtn.Label += source;
				}

				else mtn.Label = Lex.Replace(mtn.Label, source, "");
			}

			return;
		}

		/// <summary>
		/// This method assigns a user folder to the object in the preferred project with the specified type
		/// </summary>
		/// <param name="uo"></param>
		/// <param name="type"></param>

		public static void AssignDefaultObjectFolder(
			UserObject uo,
			UserObjectType objectType)
		{
			uo.ParentFolder = SS.I.PreferredProjectId;
			uo.Type = objectType;
			AssignObjectTypeFolder(uo);

			MetaTreeNode folderNode = UserObjectTree.FindObjectTypeSubfolder(SS.I.PreferredProjectId, objectType, SS.I.UserName);
			if (folderNode == null)
			{
				MetaTreeNode projNode = UserObjectTree.FindFolderNode(SS.I.PreferredProjectId);
				if (projNode != null)
					folderNode = CreateUserFolderObjectAndNode(projNode, objectType, SS.I.UserName);
			}

			if (folderNode != null)
			{
				uo.ParentFolder = folderNode.Target; // use default folder if none specified
				uo.ParentFolderType = FolderTypeEnum.User;
			}

			return;
		}

		/// <summary>
		/// This method assigns a user folder to the object based on the object type
		/// </summary>
		/// <param name="uo"></param>

		public static void AssignObjectTypeFolder(
			UserObject uo)
		{
			string parentFolder = uo.ParentFolder;
			//if (Lex.StartsWith(parentFolder, "FOLDER_")) return; // nothing to do if already in user folder (Note: This is not correct since parent folder may be owned by another user)
			if (parentFolder == "") parentFolder = SS.I.PreferredProjectId; // if no parent use preferred project
			UserObjectType objectType = uo.Type;
			string userName = uo.Owner;
			if (Lex.IsNullOrEmpty(userName)) userName = SS.I.UserName;
			MetaTreeNode folderNode = UserObjectTree.FindObjectTypeSubfolder(parentFolder, objectType, userName);
			if (folderNode == null)
			{
				MetaTreeNode projNode = UserObjectTree.FindFolderNode(parentFolder);
				if (projNode != null)
				{
					string owner = uo.Owner;
					if (Lex.IsNullOrEmpty(owner)) owner = SS.I.UserName;
					folderNode = CreateUserFolderObjectAndNode(projNode, objectType, owner);
				}
			}

			if (folderNode != null)
			{
				uo.ParentFolder = folderNode.Target; // use default folder if none specified
				uo.ParentFolderType = FolderTypeEnum.User;
			}

			return;
		}

/// <summary>
/// Serialize the tree
/// </summary>
/// <param name="folderNodes"></param>
/// <param name="fileName"></param>

		public static void WriteCache(
			Dictionary<string, MetaTreeNode> folderNodes,
			string fileName)
		{
			StreamWriter sw = null;

			try
			{
				int t0 = TimeOfDay.Milliseconds();
				sw = new StreamWriter(fileName);

				MetaTreeNode[] mtna = new MetaTreeNode[folderNodes.Values.Count];
				folderNodes.Values.CopyTo(mtna, 0);
				for (int ni = 0; ni < mtna.Length; ni++ )
				{
					MetaTreeNode mtn = mtna[ni];
					Serialize(mtn, sw);
				}

				if (SessionManager.LogStartupDetails)
					ClientLog.TimeMessage("Time", t0);

				return;
			}
			catch (Exception ex)
			{
				if (sw != null) sw.Close(); // be sure closed
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Serialize a single node & it's children
		/// </summary>
		/// <param name="mtn"></param>
		/// <param name="sw"></param>

		public static void Serialize(
			MetaTreeNode mtn,
			StreamWriter sw)
		{
			StringBuilder sb = new StringBuilder();

			sb.Length = 0;
			sb.Append(mtn.Name);
			sb.Append("\t");
			sb.Append(((int)mtn.Type).ToString());
			sb.Append("\t");
			sb.Append(mtn.Label);
			sb.Append("\t");
			if (mtn.Size >= 0) sb.Append(mtn.Size.ToString());
			sb.Append("\t");
			if (mtn.UpdateDateTime != DateTime.MinValue) sb.Append(mtn.UpdateDateTime.ToShortDateString());
			sb.Append("\t");
			sb.Append(mtn.Target);
			sb.Append("\t");

			for (int ni = 0; ni < mtn.Nodes.Count; ni++)
			{
				MetaTreeNode child = mtn.Nodes[ni];
				sb.Append(child.Name);
				sb.Append("\t");
			}

			sw.WriteLine(sb.ToString());

			for (int ni = 0; ni < mtn.Nodes.Count; ni++)
			{
				MetaTreeNode mtn2 = mtn.Nodes[ni];
				Serialize(mtn2, sw);
			}
			return;
		}

	}

	/// <summary>
	/// Implementation of IUserObjectTree methods
	/// </summary>

	public class IUserObjectTreeMethods : IUserObjectTree
	{
		/// <summary>
		/// Assure that the tree is built
		/// </summary>

		public void AssureTreeIsBuilt()
		{
			SessionState ss = SS.I;

			if (UserObjectTree.BuildComplete) return; // if already built just return;

			else if (UserObjectTree.BuildStarted) // if started wait for it to complete
				UserObjectTree.BuildTreeWaitForCompletion();

			else UserObjectTree.BuildTree(); // must build it

			return;
		}

		/// <summary>
		/// Get list of UserObjects by type
		/// </summary>
		/// <param name="uoType"></param>
		/// <returns></returns>

		public List<UserObject> GetUserObjectsByType(
			UserObjectType uoType)
		{
			lock (UserObjectCollection.UserObjectsByType)
			{
				if (UserObjectCollection.UserObjectsByType.ContainsKey(uoType)) // already have?
					return UserObjectCollection.UserObjectsByType[uoType];
			}

			bool includeContent = (uoType == UserObjectType.CondFormat);
			List<UserObject> uoList = // need to read them
				UserObjectDao.ReadMultiple(uoType, SS.I.UserName, UserObjectTree.BuildPublicObjectContentsTrees, includeContent);

			lock (UserObjectCollection.UserObjectsByType)
			{
				UserObjectCollection.UserObjectsByType.Add(uoType, uoList);
			}

			return uoList;
		}

		/// <summary>
		/// Interface method to find node
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>

		public MetaTreeNode FindUserObjectFolderNode(
			string folderName)
		{
			UserObjectTree.WaitForBuildComplete(false);

			return UserObjectTree.FindFolderNode(folderName, UserObjectTree.FolderNodes, false);
		}


		/// <summary>
		/// Find a node by target within the full set of user objects
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public MetaTreeNode GetUserObjectNodeBytarget(
			string name)
		{
			UserObjectTree.WaitForBuildComplete(false);

			return UserObjectTree.GetNodeByTarget(name);
		}

	}

	/// <summary>
	/// Interface for callbacks for UserObject Insert/Update/Delete
	/// </summary>

	public class IUserObjectIUDMethods : IUserObjectIUD
	{

		/// <summary>
		/// New object inserted (called from UserObjectDao)
		/// Update user object cache and Main TreeControl
		/// </summary>
		/// <param name="uo"></param>

		public void UserObjectInserted(
			UserObject uo)
		{
			if (uo.IsTempObject)
			{
				if (uo.Type == UserObjectType.CnList) CidListCommand.UpdateTempListCollection(uo);
				return;
			}

			else if (Lex.IsNullOrEmpty(uo.ParentFolder) || uo.ParentFolderType == FolderTypeEnum.None)
				return;

			else // user object that should appear in main contents tree
			{
				if (SessionManager.Instance.MainContentsTree != null && UserObject.IsUserEditableType(uo.Type) &&
					!(uo.ParentFolder == "" && uo.ParentFolderType == FolderTypeEnum.None))
				{
					UserObjectTree.AddObjectToTree(uo);
					UserObjectTree.RefreshSubtreeInTreeControl(uo, SessionManager.Instance.MainContentsTree);
				}
			}

			return;
		}

		/// <summary>
		/// Object updated (called from UserObjectDao)
		/// Update user object cache and Main TreeControl
		/// May be an update in place or a move to another user or system folder
		/// </summary>
		/// <param name="uo"></param>

		public void UserObjectUpdated(
			UserObject uo)
		{
			MetaTreeNode originalNode, updatedNode, o, n;

			if (uo.IsTempObject)
			{
				if (uo.Type == UserObjectType.CnList) CidListCommand.UpdateTempListCollection(uo);
				return;
			}

			else if (Lex.IsNullOrEmpty(uo.ParentFolder) || uo.ParentFolderType == FolderTypeEnum.None)
				return;

			// User object that should appear in main contents tree

			if (uo.Type == UserObjectType.Folder)
			{ //parent-child relationships are lost if user folders aren't updated in place

				updatedNode = UserObjectTree.BuildNode(uo);
				if (UserObjectTree.FolderNodes.ContainsKey(updatedNode.Target))
					originalNode = UserObjectTree.FolderNodes[updatedNode.Target];
				else originalNode = updatedNode; // in case not found

				if (originalNode.Shared != updatedNode.Shared)
				{
					originalNode.Shared = updatedNode.Shared;
					UserObjectTree.CascadeAccess(originalNode); //may be moving a public folder into a private folder
				}

				updatedNode.Parent = originalNode.Parent; // keep these the same
				updatedNode.Nodes = originalNode.Nodes;
				updatedNode.MemberwiseCopy(originalNode); // copy all other updated members
			}

			else
			{
				UserObjectTree.RemoveObjectFromTree(uo); // update user object tree with delete/add
				UserObjectTree.AddObjectToTree(uo); // todo: update in place?
			}

			bool refreshContentsSubtree = (SessionManager.Instance.MainContentsTree != null && UserObjectTree.IsMainFormContentsType(uo.Type) &&
			 !(uo.ParentFolder == "" && uo.ParentFolderType == FolderTypeEnum.None));
			if (refreshContentsSubtree)
				UserObjectTree.RefreshSubtreeInTreeControl(uo, SessionManager.Instance.MainContentsTree);

			return;
		}

		/// <summary>
		/// Object deleted (called from UserObjectDao)
		/// Update user object cache and MainForm.TreeControl
		/// </summary>
		/// <param name="uo"></param>

		public void UserObjectDeleted(
			UserObject uo)
		{
			if (UserObject.IsUserEditableType(uo.Type))
			{
				UserObjectTree.RemoveObjectFromTree(uo); // remove from in-memory tree
				UserObjectTree.MainMenuControl.RemoveFromMruList(uo.InternalName);
			}

			bool refreshContentsSubtree = (SessionManager.Instance.MainContentsTree != null && UserObjectTree.IsMainFormContentsType(uo.Type) &&
			 !(uo.ParentFolder == "" && uo.ParentFolderType == FolderTypeEnum.None));
			if (refreshContentsSubtree)
				UserObjectTree.RefreshSubtreeInTreeControl(uo, SessionManager.Instance.MainContentsTree);

			return;
		}

	}

}
