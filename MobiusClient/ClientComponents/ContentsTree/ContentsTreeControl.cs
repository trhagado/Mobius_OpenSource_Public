using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Nodes.Operations;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Display and handle interaction with a view of the database contents
	/// </summary>

	public partial class ContentsTreeControl : DevExpress.XtraEditors.XtraUserControl
	{
		public Dictionary<string, MetaTreeNode> Nodes; // basic set of nodes associated with tree control

		public bool DisplayingFullContents = true; // displaying full DB contents tree rather than find results (in tree or list form)
		public bool DisplayingFindResults { get { return !DisplayingFullContents; } }

		public bool DisplayingAsTree = true;
		public bool DisplayingAsList { get { return !DisplayingAsTree; } }

		string ExpandNodeTarget = ""; // node to expand target value
		string ExpandNodeTarget2 = ""; // secondary node to expand target value
		string SelectedNodeTarget = ""; // node to select target value

		ContentsTreeFindDialog ContentsTreeFindDialog; // find dialog

		public MetaTreeNodeType TreeNodeTypeFilter; // type filter for the control
		public ContentsTreeFindParms FindParms; // parameters used for most recent find command
		public Dictionary<string, string> NodeFilter; // list of nodes to limit filtering to

		public List<MetaTreeNode> FoundList; // list of nodes found in search
		public Dictionary<string, string> FoundDict; // dictionary from node name to level numbering
		int MatchAttempts; // count of attempted matches in FindInContents

		public bool NumberItems = true; // show hierarchical numbering of items
		public bool ShowAllObjTypeFolderNames = true; // show folder names for all user object types
		bool ShowNodeToolTips = false;
		public bool NumberUserObjects = true; // number user objects in tree

		public event EventHandler FocusedNodeChanged; // Events that may be overridden by a higher level control
		public new event EventHandler Click;
		public new event EventHandler DoubleClick;
		public new event MouseEventHandler MouseDown;

		public TreeListNode FocusedTreeListNode = null; // TreeListNode currently focused
		public MetaTreeNode FocusedMetaTreeNode // MetaTreeNode corresponding to focused TreeListNode
		{
			get
			{
				if (FocusedTreeListNode == null) return null;
				else return FocusedTreeListNode.GetValue(0) as MetaTreeNode;
			}
		}

		public int Id = IdCounter++; // tree control instance id
		static int IdCounter = 0;

		public ContentsTreeControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			TreeList.SelectImageList = Bitmaps.Bitmaps16x16;

			Nodes = MetaTree.Nodes; // default set of nodes

			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Gets the currently selected TreeNodes and returns the associated MetaTreeNodes.  This is designed for external controls (MainMenu)
		/// that are not concerned with the current position of the cursor (as most context menus are), but rather the currenlty selected tree nodes.
		/// This is because when a user selects the Edit Menu from the menubar, the cursor is on the menu and not the TreeNode.
		/// </summary>
		/// <returns></returns>
		public MetaTreeNode[] GetCurrentSelectedNodes()
		{
			List<MetaTreeNode> metaTreeNodeList = new List<MetaTreeNode>();
			var enumerator = TreeList.Selection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TreeListNode treeListNode = (TreeListNode)enumerator.Current;
				MetaTreeNode mtn = GetMetaTreeNode(treeListNode);
				metaTreeNodeList.Add(mtn);
			}
			return metaTreeNodeList.ToArray();
		}

		/// <summary>
		/// Show normal menu with preferred project selected
		/// </summary>

		public void ShowFullTree()
		{
			string topNodeTarget = "";

			if (DisplayingAsTree && !DisplayingFullContents)
			{
				DisplayingFullContents = true;
				SetTreeListNodeVisibility(null); // show all nodes
				return;
			}

			MetaTreeNode mtn = UserObjectTree.FindFolderNode(SS.I.PreferredProjectId);
			if (mtn == null) //try the MetaTreeFactory for the node...
				mtn = MetaTree.GetNode(SS.I.PreferredProjectId);

			if (mtn != null) topNodeTarget = mtn.Type + " " + mtn.Name;  //mtn.Target;

			ShowFullTree(topNodeTarget);
			return;
		}

		/// <summary>
		/// Show normal menu with specified node open and at top
		/// </summary>

		public void ShowFullTree(
			string topNodeTarget)
		{
			MetaTreeNodeType contentsFilter = ContentsTreeControl.GetNormalViewFilter();

			FillTree(
				"root",
				contentsFilter, // must have metatable (calcfield, annotation)
				null,
				topNodeTarget, // open to preferred project
				null,
				true,  // level numbering
				false); // show all folders with object-type folder names
			return;
		}



/// <summary>
/// Refill tree using previous fill settings
/// </summary>

		public void RefillTree()
		{
			//string topNodeTarget = ContentsTreeControl.GetPreferredProjectNodeTarget();

			FillTree(
				"root",
				TreeNodeTypeFilter, // must have metatable (calcfield, annotation)
				SelectedNodeTarget,
				ExpandNodeTarget, // open to preferred project
				ExpandNodeTarget2,
				true,  // level numbering
				false); // show all folders with object-type folder names

			return;
		}


		/// <summary>
		/// Fill contents tree 
		/// </summary>
		/// <param name="rootNodeName"></param>
		/// <param name="typeFilter"></param>
		/// <param name="selectedNodeTarget"></param>
		/// <param name="expandNodeTarget"></param>
		/// <param name="expandNodeTarget2"></param>
		/// <param name="numberItems"></param>
		/// <param name="showAllObjTypeFolderNames"></param>

		public void FillTree(
			string rootNodeName,
			MetaTreeNodeType typeFilter,
			string selectedNodeTarget,
			string expandNodeTarget,
			string expandNodeTarget2,
			bool numberItems,
			bool showAllObjTypeFolderNames)
		{

			MetaTreeNode mtn = MetaTree.GetNode(rootNodeName);
			Dictionary<string, string> nodeFilter = null;

			FillTree(mtn, typeFilter, nodeFilter, selectedNodeTarget, expandNodeTarget, expandNodeTarget2,
				numberItems, showAllObjTypeFolderNames);

			return;
		}

		/// <summary>
		/// Fill contents tree
		/// </summary>
		/// <param name="rootNode"></param>
		/// <param name="typeFilter"></param>
		/// <param name="nodeFilter"></param>
		/// <param name="selectedNodeTarget"></param>
		/// <param name="expandNodeTarget"></param>
		/// <param name="expandNodeTarget2"></param>
		/// <param name="numberItems"></param>
		/// <param name="showAllObjTypeFolderNames"></param>

		public void FillTree(
			MetaTreeNode rootNode,
			MetaTreeNodeType typeFilter,
			Dictionary<string, string> nodeFilter,
			string selectedNodeTarget,
			string expandNodeTarget,
			string expandNodeTarget2,
			bool numberItems,
			bool showAllObjTypeFolderNames)
		{
			TreeListNode tln, tln2;

			int t0 = TimeOfDay.Milliseconds();

			TreeNodeTypeFilter = typeFilter; // set filter so other class methods can see it
			NodeFilter = nodeFilter;

			TreeList.ClearNodes();

			DisplayingFullContents = (nodeFilter == null);
			DisplayingAsTree = true;

			//MetaTreeNode mtn = MetaTree.GetNode("Root");
			//if (mtn == null) throw new Exception("Metatree root not found");

			object[] oa = new object[1];
			oa[0] = rootNode;
			tln = TreeList.AppendNode(oa, null);
			tln.HasChildren = NodeHasChildren(rootNode);
			CreateTreeListNodeChildren(rootNode, tln);

			ExpandNodeTarget = expandNodeTarget;
			ExpandNodeTarget2 = expandNodeTarget2;
			SelectedNodeTarget = selectedNodeTarget;

			tln = TreeList.Nodes.FirstNode;
			if (tln == null) return;

			tln.Expanded = true; // expand root

			if (!String.IsNullOrEmpty(ExpandNodeTarget))
			{
				tln2 = FindNodeByTarget(tln, ExpandNodeTarget);
				if (tln2 != null)
				{
					ExpandTreeListNode(tln2);
					TreeList.FocusedNode = tln2;
					//					Tree.SetFocusedNode(tln2);
					TreeList.Focus();
					TreeList.Refresh();
				}
			}

			if (!String.IsNullOrEmpty(ExpandNodeTarget2))
			{
				tln2 = FindNodeByTarget(tln, ExpandNodeTarget2);
				if (tln2 != null)
				{
					ExpandTreeListNode(tln2);
				}
			}

			if (!String.IsNullOrEmpty(SelectedNodeTarget))
			{
				tln2 = FindNodeByTarget(tln, SelectedNodeTarget);
				if (tln2 != null)
				{
					ExpandTreeListNode(tln2);
					TreeList.Focus();
					TreeList.FocusedNode = tln2;
				}
			}

			//ClientLog.Message("ContentsTreeControl.Fill time = " + (TimeOfDay.Milliseconds() - t0)); // debug
			//int nodeCount = Nodes.Count; // debug

			return;
		}

		/// <summary>
		/// Fill contents tree for specified object type
		/// </summary>
		/// <param name="type"></param>
		/// <returns>Node with focus</returns>

		public MetaTreeNode FillTree(UserObjectType type)
		{
			UserObject uo = new UserObject(type);
			return FillTree(uo);
		}

		/// <summary>
		/// Fill contents tree for specified object type
		/// </summary>
		/// <param name="uo"></param>
		/// <returns>Node with focus</returns>

		public MetaTreeNode FillTree(UserObject uo)
		{
			MetaTreeNode mtn = null;
			if (uo == null || uo.Type == UserObjectType.Unknown) return null;

			// Create the node type filter

			MetaTreeNodeType mtnFilter = MetaTreeNodeType.Project; // show project folders and above
			mtnFilter |= MetaTreeNodeType.UserFolder; // and user folders
			mtnFilter |= UserObjectTree.UserObjectTypeToMetaTreeNodeType(uo.Type); // and the object type

			string defaultFolder = uo.ParentFolder; // if object has existing parent then use that as defaule
			if (defaultFolder == "") // otherwise use preferred project
				defaultFolder = SS.I.PreferredProjectId;

			mtn = UserObjectTree.FindFolderNode(defaultFolder);
			if (mtn == null)
			{
				//assume that it's a project node...
				mtn = MetaTree.GetNode(defaultFolder);
				if (mtn == null)
				{
					//folder might have been deleted out from under an open object, try the preferred project
					mtn = UserObjectTree.FindFolderNode(SS.I.PreferredProjectId);

					if (mtn == null)
					{
						//use a blank mtn to avoid null pointer exceptions
						mtn = new MetaTreeNode();
					}
				}
			}

			// If defaulting to a project, prefer to use an object type folder

			if (mtn.Type == MetaTreeNodeType.Project)
			{
				string userName = uo.Owner;
				if (Lex.IsNullOrEmpty(userName)) userName = SS.I.UserName;
				MetaTreeNode otf = UserObjectTree.FindObjectTypeSubfolder(mtn.Target, uo.Type, userName);
				if (otf != null) mtn = otf;
			}

			// Determine the targets of the top/selected/expand nodes

			string topNodeTarget = mtn.Target;
			string expandNodeTarget = mtn.Target;
			string expandNodeTarget2 = "";

			bool showAllObjTypeFolderNames = (uo.Type == UserObjectType.Folder);
			FillTree("root", mtnFilter, topNodeTarget, expandNodeTarget, expandNodeTarget2, false, showAllObjTypeFolderNames);

			return mtn;
		}

		/// <summary>
		/// Get the target string for the preferred project node for the user
		/// </summary>
		/// <returns></returns>

		public static string GetPreferredProjectNodeTarget()
		{
			string target = "";

			MetaTreeNode mtn = ContentsTreeControl.GetPreferredProjectNode();

			if (mtn != null) target = mtn.Type + " " + mtn.Name;  //mtn.Target;

			return target;
		}

		/// <summary>
		/// Get the preferred project node for the user
		/// </summary>
		/// <returns></returns>

		public static MetaTreeNode GetPreferredProjectNode()
		{
			MetaTreeNode mtn = UserObjectTree.FindFolderNode(SS.I.PreferredProjectId); // see if user-defined folder

			if (mtn == null) //try the MetaTreeFactory for the node...
				mtn = MetaTree.GetNode(SS.I.PreferredProjectId);

			return mtn;
		}


		/// <summary>
		/// Get the MetaTreeNode at the specified point
		/// </summary>
		/// <param name="pt"></param>
		/// <returns></returns>

		public MetaTreeNode GetMetaTreeNodeAt(Point pt)
		{
			TreeListNode tln = GetTreeListNodeAt(pt);
			return GetMetaTreeNode(tln);
		}

		/// <summary>
		/// Get the MetaTreeNode at the specified point
		/// </summary>
		/// <param name="pt"></param>
		/// <returns></returns>

		public MetaTreeNode GetMetaTreeNodeAt(
			Point pt,
			out TreeListNode tln)
		{
			tln = GetTreeListNodeAt(pt);
			return GetMetaTreeNode(tln);
		}

		/// <summary>
		/// Get the TreeListNode at the specified point
		/// </summary>
		/// <param name="pt"></param>
		/// <returns></returns>

		public TreeListNode GetTreeListNodeAt(Point pt)
		{
			TreeListHitInfo hi = TreeList.CalcHitInfo(pt);
			TreeListNode tln = hi.Node;
			return tln;
		}

		/// <summary>
		/// Get the MetaTreeNode corresponding to a TreeListNode
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>

		public MetaTreeNode GetMetaTreeNode(TreeListNode node)
		{
			if (node == null) return null;
			else return node.GetValue(0) as MetaTreeNode;
		}

		/// <summary>
		/// Get the MetaTreeNode path from the top of the tree down to the current TreeListNode
		/// </summary>
		/// <param name="node"></param>
		/// <returns>treeRootMetaTreeNodeName.nextLevelDown...thisNode</returns>

		public string GetMetaTreeNodePath(TreeListNode node)
		{
			string path = "";
			if (node == null) return path;

			while (node != null)
			{
				MetaTreeNode mtn = node.GetValue(0) as MetaTreeNode;
				if (mtn == null) break;
				if (path == "") path = mtn.Name;
				else path = mtn.Name + "." + path;
				node = node.ParentNode;
			}

			return path;
		}

		/// <summary>
		/// Fill-in node children if any from associated MetaTreeNode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tree_BeforeExpand(object sender, BeforeExpandEventArgs e)
		{
			MetaTreeNode mtn = e.Node.GetValue(0) as MetaTreeNode;
			if (mtn == null) return;

			VerifyUserObjectNodesAvailable(mtn);

			//Tree_BeforeExpand: " + mtn.Name);

			CreateTreeListNodeChildren(mtn, e.Node);
			return;
		}

		/// <summary>
		/// Create children in a TreeListNode to match the associated MetaTreeNode
		/// </summary>
		/// <param name="mtn"></param>
		/// <param name="tln"></param>

		private void CreateTreeListNodeChildren(MetaTreeNode mtn, TreeListNode tln)
		{
			if (mtn == null) return;
			//if (Lex.Eq(mtn.Name, "DHT_VIEW")) mtn = mtn; // debug
			if (tln.Nodes.Count > 0) return; // already realized
			if (!NodeHasChildren(mtn)) return;
			List<MetaTreeNode> children = GetMetaTreeNodeChildren(mtn);

			bool publicOnly = false;
			if (tln.ParentNode != null)
			{ // get just public nodes if within a public folder
				MetaTreeNode mtn2 = tln.ParentNode.GetValue(0) as MetaTreeNode;
				if (mtn2 != null && Lex.StartsWith(mtn2.Name, "MERGED_PUBLIC_"))
					publicOnly = true;
			}

			object[] oa = new object[1];
			foreach (MetaTreeNode mtn0 in children)
			{
				if (publicOnly && !mtn0.Shared) continue;

				oa[0] = mtn0;
				TreeListNode n2 = TreeList.AppendNode(oa, tln);

				if (DisplayingFindResults && FoundDict != null)
					n2.Visible = FoundDict.ContainsKey(mtn0.Target);

				if (NodeHasChildren(mtn0))
					n2.HasChildren = true;
			}

			return;
		}

		/// <summary>
		/// Get the text of the node from the associated MetaTreeNode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tree_CustomDrawNodeCell(object sender, CustomDrawNodeCellEventArgs e)
		{
			FormatTreeListNode(e);

			return;
		}

		/// <summary>
		/// Return true if node has children
		/// </summary>
		/// <param name="mtn"></param>
		/// <returns></returns>

		internal bool NodeHasChildren(MetaTreeNode mtn)
		{
			//if (Lex.Eq(mtn.Label, "Queries")) mtn = mtn; // debug;
			if (mtn == null) return false;
			if (MetaTreeNode.IsLeafNodeType(mtn.Type)) return false;
			if (mtn.Nodes != null && mtn.Nodes.Count > 0) return true;

			if (Lex.Eq(mtn.Name, "DEFAULT_FOLDER")) return true; // special case for folder that may have user objects added later

			MetaTreeNode mtn2 = UserObjectTree.FindFolderNode(mtn.Name, true); // check in user object tree
			if (mtn2 != null && mtn2.Nodes != null && mtn2.Nodes.Count > 0) return true; // (need to filter?)

			return false;
		}


		/// <summary>
		/// Return children of specified node including userobjects
		/// </summary>
		/// <param name="mtn"></param>
		/// <returns></returns>

		internal List<MetaTreeNode> GetMetaTreeNodeChildren(MetaTreeNode mtn)
		{
			return GetMetaTreeNodeChildren(mtn, true);
		}

		/// <summary>
		/// Return children of specified node including userobjects
		/// </summary>
		/// <param name="mtn"></param>
		/// <returns></returns>

		internal List<MetaTreeNode> GetMetaTreeNodeChildren(
			MetaTreeNode mtn,
			bool mergeUserObjectFolders)
		{
			int t0 = TimeOfDay.Milliseconds();

			List<MetaTreeNode> children;
			MetaTreeNode mtn2, childMtn;
			int ci, ci2;

			children = new List<MetaTreeNode>();
			if (mtn == null) return children;

			//if (mtn.Name == "USERDATABASE_234480") mtn = mtn; // debug;
			//if (mtn.Label == "Annotation Subfolder") mtn = mtn; // debug;

			// Check in main tree & add any children while filtering

			mtn2 = MetaTree.GetNode(mtn.Name); // if node is in main tree then include those children
			if (mtn2 != null)
			{
				for (ci2 = 0; ci2 < mtn2.Nodes.Count; ci2++)
				{
					childMtn = mtn2.Nodes[ci2];

					if (TreeNodeTypeFilter != 0 && // check type filter
					 childMtn.IsLeafType && (TreeNodeTypeFilter & childMtn.Type) == 0)
						continue;

					//if (NodeFilter != null && childMtn.IsLeafType && // check node filter
					// NodeFilter.Count > 0 && !NodeFilter.ContainsKey(childMtn.Target))
					//	continue;

					children.Add(childMtn);
				}
			}

			// Check in user object tree & add any children while filtering

			mtn2 = UserObjectTree.FindFolderNode(mtn.Name, mergeUserObjectFolders); // check in user object tree
			if (mtn2 != null)
			{
				for (ci2 = 0; ci2 < mtn2.Nodes.Count; ci2++)
				{
					childMtn = mtn2.Nodes[ci2];

					//if (Lex.Eq(childMtn.Label, "test of 3.0")) mtn = mtn;

					if (TreeNodeTypeFilter != 0)
					{
						if (childMtn.IsLeafType && (TreeNodeTypeFilter & childMtn.Type) == 0)
							continue;

						else if (childMtn.IsFolderType && childMtn.Nodes.Count > 0 && !HaveChildrenOfAllowedType(childMtn) && // ignore if has children but none of allowed types
						 childMtn.Type != MetaTreeNodeType.Database) // always include databases
							continue;
					}

					//if (NodeFilter != null && childMtn.IsLeafType && // check node filter
					// NodeFilter.Count > 0 && !NodeFilter.ContainsKey(childMtn.Target))
					//	continue;

					children.Add(childMtn);
				}
			}

			//ClientLog.Message("GetNodeChildren " + mtn.Label + " (" + mtn.Name + ") time = " + (TimeOfDay.Milliseconds() - t0)); // debug 
			return children;
		}

		/// <summary>
		/// Scan a subtree of UserObjects looking for at least one leaf node that passes the filter
		/// </summary>
		/// <param name="mtn"></param>
		/// <param name="typeFilter"></param>
		/// <returns></returns>

		bool HaveChildrenOfAllowedType(MetaTreeNode mtn)
		{

			foreach (MetaTreeNode childMtn in mtn.Nodes)
			{
				if (childMtn.IsLeafType && (TreeNodeTypeFilter & childMtn.Type) != 0) return true;
				else if (HaveChildrenOfAllowedType(childMtn)) return true;
			}

			if (mtn.Type == MetaTreeNodeType.UserFolder)
			{ // return true if UserFolder with a name that corresponds to the allowed type
				UserObjectType uot = UserObject.GetTypeFromPlural(mtn.Label); // see if folder of allowed type
				if (uot == UserObjectType.Unknown) return false;

				MetaTreeNodeType mtnType = UserObjectTree.UserObjectTypeToMetaTreeNodeType(uot);
				if ((TreeNodeTypeFilter & mtnType) != 0) return true;
			}

			return false;
		}

		/// <summary>
		/// Get the hierarchical number label for the node
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>

		string GetNodeNumberLabel(TreeListNode tln)
		{
			// Recursively search up the tree until the top is reached.
			// Then number back down.
			// If displaying search results then check FoundDict to see if this node is in the dict

			MetaTreeNode mtn = tln.GetValue(0) as MetaTreeNode;
			if (DisplayingAsList && mtn != null && FoundDict != null && FoundDict.ContainsKey(mtn.Target))
				return FoundDict[mtn.Target];

			int idx = GetIndexWithinParent(tln);
			if (idx < 0) return "";

			string txt = GetNodeNumberLabel(tln.ParentNode);
			if (txt != "") txt += ".";
			txt += (idx + 1).ToString();
			return txt;
		}

		/// <summary>
		/// Get index of this node within its parent
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>

		int GetIndexWithinParent(TreeListNode tln)
		{
			if (tln.ParentNode == null) return -1;

			TreeListNode pNode = tln.ParentNode;
			for (int i1 = 0; i1 < pNode.Nodes.Count; i1++)
			{
				if (pNode.Nodes[i1] == tln) return i1;
			}

			return -1; // not found
		}

		/// <summary>
		/// Format the text of the node from it's elements
		/// </summary>
		/// <returns></returns>

		public void FormatTreeListNode(
			CustomDrawNodeCellEventArgs e)
		{
			if (e.Column.AbsoluteIndex != 0) return;
			if (e.CellText.Contains(" - ") && Char.IsNumber(e.CellText[0])) return; // already has numbering

			MetaTreeNode mtn = e.Node.GetValue(0) as MetaTreeNode;
			if (mtn == null) return;

			// if (mtn.Name == "FOLDER_211500") mtn = mtn; // debug

			string label = "";
			TreeListNode tln = e.Node;

			if (Lex.Eq(mtn.Target, "Root")) // set root node label based on whether we are showing full content or Find results
			{
				if (DisplayingFullContents)
				{
					e.CellText = "Contents";
					return;
				}

				else
				{
					e.Graphics.Clear(Color.Red); // hilight if search results
					string qs = FindParms != null ? FindParms.GetDisplayString() : "";
					e.CellText = qs;
					return;
				}
			}

// Build detailed label

			if (NumberItems && (NumberUserObjects || !mtn.IsUserObjectType)) // number user objects?
			{
				label = GetNodeNumberLabel(e.Node);
				if (!(String.IsNullOrEmpty(label)))
					label += " - ";
			}

			label += mtn.Label;

			StringBuilder sb = new StringBuilder(32);

			//if (Lex.Contains(mtn.Label, "Substance Data")) mtn = mtn; // debug

			if (!ShowNodeToolTips && mtn.IsLeafType) // add any stats here if not showing tooltips
				sb = MetaTreeNode.FormatStatistics(mtn);

			// Add owner name for user obj if not ours or owner different than parent owner or parent is virtual 
			// Must navigate via TreeListNodes since MetaTreeNodes will not point to any added virtual parents

			if (mtn.IsUserObjectType) do
				{
					bool addOwnerName = false;
					if (tln.ParentNode == null) break;
					MetaTreeNode parentMtn = tln.ParentNode.GetValue(0) as MetaTreeNode;
					if (parentMtn == null) break;

					//if (Lex.Contains(mtn.Label, "BA_query")) // mtn = mtn; // debug
					//  ClientLog.Message(mtn.Label + " " + parentMtn.Owner + " " + parentMtn.Label);

					string parentOwner = SS.I.UserName; // pretend we own the parent folder by default
					if (parentMtn.IsUserObjectType &&
					 (parentMtn.Owner != "" || // if specific owner
					 Lex.StartsWith(parentMtn.Label, "Shared "))) // or "Shared" folder
					{
						parentOwner = parentMtn.Owner; // use the owner of parent user folder for compare
					}

					if (mtn.Owner != parentOwner) // include owner if different than parent
					{
						if (sb.Length > 0) sb.Append("; ");
						string ownerName = SecurityUtil.GetShortPersonNameReversed(mtn.Owner);
						sb.Append(ownerName);
					}
				} while (false);

			if (sb.Length > 0)
				label += " - [" + sb.ToString() + "]";

			e.CellText = label;
			return;
		}

		/// <summary>
		/// Get the image associated with the node from the associated MetaTreeNode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tree_CustomDrawNodeImages(object sender, CustomDrawNodeImagesEventArgs e)
		{
			TreeListNode n = e.Node;
			MetaTreeNode mtn = n.GetValue(0) as MetaTreeNode;
			if (mtn == null) return;
			int ii = mtn.GetImageIndex();

			if ((ii == (int)Bitmaps16x16Enum.Folder || mtn.Type == MetaTreeNodeType.UserFolder) && n.Expanded)
				ii = (int)Bitmaps16x16Enum.FolderOpen;
			e.SelectImageIndex = ii;
		}

		/// <summary>
		/// Find first node in tree with specified target
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>

		public TreeListNode FindNodeByTarget(
			string target)
		{
			TreeListNode root = TreeList.Nodes.FirstNode.RootNode;
			return FindNodeByTarget(root, target);
		}

		/// <summary>
		/// Find first node in tree with specified target
		/// </summary>
		/// <param name="n">Node to start search at</param>
		/// <param name="nodeTarget">Target of node to find</param>
		/// <returns>Any matching TreeListNode</returns>

		public TreeListNode FindNodeByTarget(
			TreeListNode tln,
			string target)
		{
			TreeListNode tln2;

			if (tln == null || target == null || target == "") return null;
			try { tln2 = RealizePathToTarget(tln, target); }
			catch (Exception ex) { return null; } // catch exception and return null if not found
			return tln2;
		}

		/// <summary>
		/// Return the TreeListNode that matches a target creating the tree
		/// as necessary to build a path to the target.
		/// </summary>

		TreeListNode RealizePathToTarget(
			TreeListNode tln,
			string target)
		{
			MetaTreeNode mtn, mtn2;
			TreeListNode tln2 = null;
			int li, ci;

			mtn = tln.GetValue(0) as MetaTreeNode;
			List<MetaTreeNode> mtnList = FindMetaTreePathToTarget(mtn, target);
			if (mtnList == null) return null;

			for (li = 0; li < mtnList.Count; li++)
			{ // match TreeList to MetaTree creating nodes in the MetaTree as necessary
				mtn2 = mtnList[li]; // node to look for

				if (tln.Nodes.Count == 0)
				{
					VerifyUserObjectNodesAvailable(mtn);
					CreateTreeListNodeChildren(mtn, tln);
				}

				for (ci = 0; ci < tln.Nodes.Count; ci++)
				{
					tln2 = tln.Nodes[ci];
					if ((tln2.GetValue(0) as MetaTreeNode).Target == mtn2.Target) break;
				}

				if (ci >= tln.Nodes.Count) throw new Exception("Node not found");
				mtn = mtn2;
				tln = tln2;
			}

			return tln;
		}

		/// <summary>
		/// Verify that any UserObject nodes associated with the supplied MetaTreeNode have been read in and read them in if not
		/// </summary>
		/// <param name="mtn"></param>

		void VerifyUserObjectNodesAvailable(MetaTreeNode mtn)
		{
			if (mtn.IsSystemType && mtn.IsFolderType && !UserObjectTree.BuildComplete &&
				mtn.Type != MetaTreeNodeType.Root && mtn.Type != MetaTreeNodeType.Database &&
				UserObjectTree.FolderNodes != null && !UserObjectTree.FolderNodes.ContainsKey(mtn.Name))
			{
				UserObjectTree.RetrieveAndBuildSubTree(mtn.Name);
			}

			return;
		}

		/// <summary>
		/// Find path to specified target in MetaTree
		/// </summary>
		/// <param name="mtn"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		List<MetaTreeNode> FindMetaTreePathToTarget(
			MetaTreeNode mtn,
			string typeAndtarget)
		{
			MetaTreeNodeType type;
			string target;

			if (mtn == null || typeAndtarget == null || typeAndtarget == "") return null;
			MetaTreeNode.ParseTypeAndNameString(typeAndtarget, out type, out target);
			if (target == null) return null;
			target = target.ToUpper();

			List<MetaTreeNode> path = FindMetaTreePathByTarget2(mtn, target);
			return path;
		}

		List<MetaTreeNode> FindMetaTreePathByTarget2(
			MetaTreeNode mtn,
			string target)
		{
			List<MetaTreeNode> path;
			int ci;
			List<MetaTreeNode> children = GetMetaTreeNodeChildren(mtn);
			for (ci = 0; ci < children.Count; ci++)
			{
				MetaTreeNode n2 = children[ci];
				if (n2.Target == target)
				{
					path = new List<MetaTreeNode>();
					path.Add(n2);
					return path;
				}
				path = FindMetaTreePathByTarget2(n2, target);
				if (path != null)
				{
					path.Insert(0, n2);
					return path;
				}
			}

			return null;
		}

		/// <summary>
		/// Find all nodes in tree with the specified target
		/// </summary>
		/// <param name="nodeTarget"></param>
		/// <returns></returns>

		public List<TreeListNode> FindNodesByTarget(
			string nodeTarget)
		{
			if (TreeList.Nodes.Count == 0) return null;
			TreeListNode n = TreeList.Nodes[0].RootNode;
			return FindNodesByTarget(n, nodeTarget);
		}

		/// <summary>
		/// Find all nodes in tree with the specified target
		/// </summary>
		/// <param name="n"></param>
		/// <param name="nodeTarget"></param>
		/// <returns></returns>

		public List<TreeListNode> FindNodesByTarget(
			TreeListNode n,
			string nodeTarget)
		{
			List<TreeListNode> list = new List<TreeListNode>();
			FindNodesByTarget(n, nodeTarget, list); // enter recursive code
			if (list.Count == 0) list = null; // nothing found
			return list;
		}

		/// <summary>
		/// Recursive node searcher
		/// </summary>
		/// <param name="n"></param>
		/// <param name="nodeTarget"></param>
		/// <param name="list"></param>

		public void FindNodesByTarget(
			TreeListNode n,
			string nodeTarget,
			List<TreeListNode> list)
		{
			if (n == null || nodeTarget == null || nodeTarget == "") return;
			MetaTreeNode mtn = n.GetValue(0) as MetaTreeNode;
			if (Lex.Eq(mtn.Target, nodeTarget))
				list.Add(n); // we are one

			foreach (TreeListNode tn in n.Nodes) // check subnodes
			{
				FindNodesByTarget(tn, nodeTarget, list);
			}
			return;
		}

		/// <summary>
		/// Select tree node with specified target
		/// </summary>
		/// <param name="selectedNodeTarget"></param>

		public void SelectNode(
			string selectedNodeTarget)
		{
			TreeListNode tln = FindNodeByTarget(selectedNodeTarget);
			if (tln != null) TreeList.FocusedNode = tln;
		}

		/// <summary>
		/// Expand tree node with specified target (and any parent nodes that are not expanded as well)
		/// </summary>
		/// <param name="expandedNodeTarget"></param>

		public void ExpandNode(
			string selectedNodeTarget)
		{
			TreeListNode root = TreeList.Nodes.FirstNode.RootNode;
			TreeListNode tln = FindNodeByTarget(root, selectedNodeTarget);
			ExpandTreeListNode(tln);
			return;
		}

		void ExpandTreeListNode(TreeListNode tln)
		{
			if (tln == null || tln.Expanded) return;
			if (tln.ParentNode != null) ExpandTreeListNode(tln.ParentNode);
			tln.Expanded = true;
			return;
		}

		/// <summary>
		/// Expand all nodes in tree
		/// </summary>

		public void ExpandAllNodes()
		{
			TreeListNode tln = TreeList.Nodes.FirstNode;
			if (tln == null) return;
			TreeList.BeginUpdate();
			ExpandTreeListNodeToFullDepth(tln);
			TreeList.EndUpdate();
		}

		public void ExpandTreeListNodeToFullDepth(TreeListNode tln)
		{
			if (!tln.Expanded) tln.Expanded = true;
			foreach (TreeListNode child in tln.Nodes)
				ExpandTreeListNodeToFullDepth(child);

			return;
		}

		/// <summary>
		/// Redraw all occurances of specified subtree
		/// </summary>
		/// <param name="targetName"></param>
		/// <param name="typeFilter"></param>
		/// <param name="showAllObjTypeFolderNames"></param>

		public int RefreshSubtree(
			string nodeName,
			MetaTreeNodeType typeFilter,
			bool showAllObjTypeFolderNames)
		{
			TreeNodeTypeFilter = typeFilter; // reset filters
			NodeFilter = null;
			ShowAllObjTypeFolderNames = showAllObjTypeFolderNames;
			return RefreshSubtree(nodeName);
		}

		/// <summary>
		/// Redraw all occurances of specified subtree
		/// </summary>
		/// <param name="targetName"></param>

		public int RefreshSubtree(string nodeName)
		{
			List<TreeListNode> nodes = FindNodesByTarget(nodeName);
			if (nodes == null || nodes.Count == 0) return 0;

			TreeList.BeginUpdate();
			foreach (TreeListNode tln in nodes)
			{
				MetaTreeNode mtn = tln.GetValue(0) as MetaTreeNode;
				tln.Nodes.Clear();
				CreateTreeListNodeChildren(mtn, tln);
				continue;
			}

			TreeList.EndUpdate();
			return nodes.Count;
		}

		/// <summary>
		/// Refresh a single node
		/// </summary>
		/// <param name="tln"></param>

		public void RefreshNode(TreeListNode tln)
		{
			if (tln == null) return;
			tln.TreeList.Refresh(); // refresh in place
			return;

			//object[] oa = new object[1];
			//oa[0] = tln.GetValue(0);

			//TreeListNode parent = tln.ParentNode;
			//if (parent == null) return;
			//parent.Nodes.Remove(tln); // remove & readd node to redisplay
			//TreeListNode newNode = TreeList.AppendNode(oa, parent);
			//return;
		}

		/// <summary>
		/// Do a quick search of the tree by substring
		/// </summary>
		/// <param name="findParm"></param>
		/// <returns></returns>

		public List<MetaTreeNode> FindNodesBySubstringQuick(
			string findParm,
			int maxNodes)
		{
			int t0 = TimeOfDay.Milliseconds();
			int i1, i2;

			List<MetaTreeNode> matchingNodes = new List<MetaTreeNode>();
			Dictionary<string, MetaTreeNode> matchingNodeDict = new Dictionary<string, MetaTreeNode>();

			if (String.IsNullOrEmpty(findParm)) return matchingNodes;

			// Check basic set of nodes

			Dictionary<string, MetaTreeNode> nodes = Nodes;
			if (nodes == null) nodes = MetaTree.Nodes;
			if (nodes == null) return matchingNodes;

			foreach (MetaTreeNode mtn in nodes.Values)
			{
				if (Lex.StartsWith(mtn.Name, "HIDDEN")) continue; // ignore "hidden" items

				if (TreeNodeTypeFilter != 0 && (TreeNodeTypeFilter & mtn.Type) == 0) // filter by type
					continue;

				if (NodeFilter != null && !NodeFilter.ContainsKey(mtn.Target)) // filter by Nodes if defined
					continue;

				//if (mtn.IsLeafType && (mtn.Type | MetaTreeNodeType.Query) == 0) mtn.Type = mtn.Type; // debug

				i1 = mtn.Label.IndexOf(findParm, StringComparison.OrdinalIgnoreCase);
				if (i1 < 0)
				{
					i1 = mtn.Target.IndexOf(findParm, StringComparison.OrdinalIgnoreCase);
					if (i1 != 0) i1 = -1; // must start at beginning
				}

				if (i1 >= 0)
				{
					for (i2 = 0; i2 < matchingNodes.Count; i2++) // see if already have
					{
						if (Lex.Eq(matchingNodes[i2].Target, mtn.Target)) break;
					}

					if (i2 >= matchingNodes.Count)
					{
						matchingNodes.Add(mtn);
						if (maxNodes > 0 && matchingNodes.Count >= maxNodes) return matchingNodes;
					}
				}
			}

			// Check user object nodes

			if (UserObjectTree.FolderNodes == null) return matchingNodes;

			int exceptionCount = 0;
			while (true) try
				{ // list of nodes may change during loop by UserObject retrieval thread throwing exception
					foreach (MetaTreeNode mtn in UserObjectTree.FolderNodes.Values)
					{
						FindNodesBySubstringQuick(mtn, findParm, matchingNodes, matchingNodeDict, maxNodes);
					}
					break;
				}

				catch (Exception ex) // try again on exception
				{
					exceptionCount++;
					if (exceptionCount < 10) continue;
					else break;
				}

			t0 = TimeOfDay.Milliseconds() - t0;

			return matchingNodes;
		}

		/// <summary>
		/// Recursive searcher
		/// </summary>
		/// <param name="node"></param>
		/// <param name="findParm"></param>
		/// <param name="matchingNodes"></param>
		/// <param name="matchingNodeDict"></param>

		public void FindNodesBySubstringQuick(
			MetaTreeNode mtn,
			string findParm,
			List<MetaTreeNode> matchingNodes,
			Dictionary<string, MetaTreeNode> matchingNodeDict,
			int maxNodes)
		{
			int i1, i2;
			//if (mtn == null || mtn.Label == null) return;

			i1 = -1;
			if (mtn.Label != null) // check if substring is present in label
				i1 = mtn.Label.IndexOf(findParm, StringComparison.OrdinalIgnoreCase);

			if (i1 < 0)
			{
				if (mtn.Target != null) // if not, see if target prefix
					i1 = mtn.Target.IndexOf(findParm, StringComparison.OrdinalIgnoreCase);
				if (i1 != 0) i1 = -1; // must start at beginning
			}

			if (i1 >= 0) // see if passes filters
			{
				if (TreeNodeTypeFilter != 0 && (TreeNodeTypeFilter & mtn.Type) == 0) // filter by type
					i1 = -1;

				if (NodeFilter != null && !NodeFilter.ContainsKey(mtn.Target)) // filter by Nodes if defined
					i1 = -1;
			}

			if (i1 >= 0)
			{
				for (i2 = 0; i2 < matchingNodes.Count; i2++) // see if already have
				{
					if (Lex.Eq(matchingNodes[i2].Target, mtn.Target)) break;
				}

				if (i2 >= matchingNodes.Count)
				{
					matchingNodes.Add(mtn);
					if (maxNodes > 0 && matchingNodes.Count >= maxNodes) return;
				}
			}

			foreach (MetaTreeNode mtn2 in mtn.Nodes)
			{
				FindNodesBySubstringQuick(mtn2, findParm, matchingNodes, matchingNodeDict, maxNodes);
				if (maxNodes > 0 && matchingNodes.Count >= maxNodes) return;
			}
		}

		/// <summary>
		/// Search the contents tree
		/// </summary>
		/// <param name="findTextParm"></param>
		/// <returns></returns>

		public bool FindInContents(string findTextParm)
		{
			string tok;
			MetaTreeNode mtn;

			string promptTxt = "Enter one or more words, partial words or an assay code " +
				"that you want to search for in the contents tree.";

			FindParms = new ContentsTreeFindParms();

			while (true) // loop on input
			{
				if (findTextParm != "")
				{
					FindParms.QueryString = findTextParm;
					FindParms.NodeTypesToSearch = MetaTreeNodeType.All;
					FindParms.LowerDateLimit = DateTime.MinValue;
					FindParms.CheckMyUserObjectsOnly = false;
					FindParms.DisplayAsList = true;
				}

				else // prompt user for query
				{
					if (ContentsTreeFindDialog == null)
						ContentsTreeFindDialog = new ContentsTreeFindDialog();
					ContentsTreeFindDialog ctfd = ContentsTreeFindDialog;

					ctfd.Prompt.Text = promptTxt;
					DialogResult dr = ctfd.ShowDialog(SessionManager.ActiveForm);
					if (dr == DialogResult.Cancel) return false;

					FindParms = ctfd.GetValues();
				}

				//if (Lex.IsEmpty(ctfd.QueryString.Text)) return false; // (allow null string limited just by node type & ownership)

				bool complete = UserObjectTree.WaitForBuildComplete(true);
				if (!complete) return false;

				FindParms.QueryString = FindParms.QueryString.Trim();
				UsageDao.LogEvent("SearchContentsTree");

				FoundList = FindInContents2(FindParms); // do search, most recent find list, held between calls

				if (FoundList.Count == 0)
				{
					if (Lex.IsUndefined(findTextParm))
					{
						TreeList.ClearNodes();

						promptTxt = "Nothing was found in the tree that matched " +
								"your supplied criteria.";
						continue;
					}

					else return false; // just return if predefined search && nothing found
				}


				if (FindParms.DisplayAsTree)
				{
					DisplayFindResultsAsTree();
					return true;
				}

				else
				{
					DisplayFindResultsAsList();
					return true;
				}
			}

			//if (FoundList.Count <= 0) return false;
			//else return true;
		}

		/// <summary>
		/// DisplayFindResultsAsList
		/// </summary>

		void DisplayFindResultsAsList()
		{
			MetaTreeNode originalRoot = MetaTree.Nodes["ROOT"]; // save the original root

			MetaTreeNode mtn = new MetaTreeNode(); // build list from new root
			mtn.Type = MetaTreeNodeType.Root;
			mtn.Name = "ROOT";
			mtn.Target = "ROOT";
			mtn.Label = "ROOT";
			foreach (MetaTreeNode mtn0 in FoundList)
				mtn.Nodes.Add(mtn0);

			MetaTree.Nodes["ROOT"] = mtn; // set new root so build of TreeList works properly

			TreeList.ClearNodes();
			object[] oa = new object[1];
			oa[0] = mtn;
			TreeListNode tln = TreeList.AppendNode(oa, null);
			tln.HasChildren = NodeHasChildren(mtn);

			DisplayingFullContents = false; // filtered
			DisplayingAsTree = false; // displayed as list

			CreateTreeListNodeChildren(mtn, tln);
			tln.Expanded = true;

			MetaTree.Nodes["ROOT"] = originalRoot; // restore original root

			return;
		}


		/// <summary>
		/// DisplayFindResultsAsTree
		/// </summary>
		void DisplayFindResultsAsTree()
		{
			MetaTreeNode rootMtn = MetaTree.GetNode("root");

			if (DisplayingAsTree)
			{
				DisplayingFullContents = false; // i.e. DisplayingFindResults = true
				SetTreeListNodeVisibility(FoundDict);
			}

			else
			{

				FillTree(
					rootNode: rootMtn,
					typeFilter: MetaTreeNodeType.All, // show everything found
					nodeFilter: FoundDict,
					selectedNodeTarget: SelectedNodeTarget, // open to preferred project
					expandNodeTarget: ExpandNodeTarget,
					expandNodeTarget2: ExpandNodeTarget2,
					numberItems: true,  // level numbering
					showAllObjTypeFolderNames: false); // show all folders with object-type folder names
			}

			return;
		}

		public void SetTreeListNodeVisibility(
			Dictionary<string, string> nodeDict)
		{
			TreeListNodesIterator tlni = TreeList.NodesIterator;
			SetTreeListNodeVisibility sv = new SetTreeListNodeVisibility(nodeDict);
			tlni.DoOperation(sv);

			//TreeList.Refresh();
			return;
		}

		/// <summary>
		/// Execute the find 
		/// </summary>
		/// <param name="findTextParm"></param>
		/// <returns></returns>

		List<MetaTreeNode> FindInContents2(
					ContentsTreeFindParms ctfp)
		{
			int t0 = TimeOfDay.Milliseconds();
			MatchAttempts = 0;
			Lex lex = new Lex();
			lex.SetDelimiters(", ( ) [ ] + -"); // default delimiters
			string tok;

			// Parse & normalize query

			List<string> words = new List<string>(); // array of words in query
			lex.OpenString(ctfp.QueryString);
			tok = lex.Get();
			while (tok != "")
			{
				tok = NormalizeForContentsTreeSearch(tok);
				words.Add(tok);
				tok = lex.Get();
			}

			MetaTreeNode mtn = MetaTree.GetNode("Root");

			int[] levelPos = new int[100];
			int level = -1;

			FoundList = new List<MetaTreeNode>();
			FoundDict = new Dictionary<string, string>();
			NodeFilter = null; // clear any previous node filter
			FindInContents2(null, mtn, ctfp, words, levelPos, level);

			FoundList.Sort(new MetaTreeNodeSortComparer());

			t0 = TimeOfDay.Milliseconds() - t0; // ~ 150 ms
			int matchAttempts = MatchAttempts;
			return FoundList;
		}

		/// <summary>
		/// Recursively search the tree
		/// </summary>
		/// <param name="tln"></param>
		/// <param name="words"></param>

		void FindInContents2(
			MetaTreeNode parentMtn,
			MetaTreeNode mtn,
			ContentsTreeFindParms ctfp,
			List<string> words,
			int[] levelPos,
			int level)
		{
			int wi;

			string label = mtn.Label.ToLower();
			string target = mtn.Target.ToLower();

			//if (Lex.Contains(label, "AdjustStructSearch")) label = label; // debug
			//if (Lex.Eq(mtn.Name, "root")) mtn = mtn; // debug

			do
			{
				MatchAttempts++;

				if ((mtn.Type & ctfp.NodeTypesToSearch) == 0)  // included type?
				{
					if (!ctfp.DisplayAsList) // if tree format keep item if not included in search
					{
						FoundList.Add(mtn);
						FoundDict[mtn.Target] = ""; // put level in target
					}

					break;
				}

				string owner = "";
				if (mtn.IsUserObjectType) // do ownership check if UserObject
				{
					if (ctfp.CheckMyUserObjectsOnly)
					{ // see if object belongs to session owner
						if (Lex.Ne(mtn.Owner, SS.I.UserName)) break;
					}

					owner = SecurityUtil.GetShortPersonNameReversed(mtn.Owner).ToLower();
				}


				for (wi = 0; wi < words.Count; wi++)
				{
					if (label.IndexOf(words[wi], StringComparison.OrdinalIgnoreCase) < 0 && // basic label contain the word?
					 (owner == "" || owner.IndexOf(words[wi], StringComparison.OrdinalIgnoreCase) < 0)) // owner name contain the word?
						break;
				}

				if (wi < words.Count) // if no match see if matches with target
				{
					if (target.Equals(words[0], StringComparison.OrdinalIgnoreCase))
					{
						label = mtn.Target + " - " + label; // put target first
						wi = words.Count;
					}
				}

				bool wordsFound = wi >= words.Count;

				bool dateOk = true; // assume date is ok
				if (ctfp.LowerDateLimit != DateTime.MinValue)
				{
					if (ctfp.DisplayAsList) // check everything if list display
						dateOk = (ctfp.LowerDateLimit.CompareTo(mtn.UpdateDateTime) <= 0);

					else if (mtn.IsLeafType) // if tree display must be leaf node for check date
					{
						if (mtn.IsUserObjectType ||  // check user objects
							 (mtn.IsAssayNode)) // check assays tables
							dateOk = (ctfp.LowerDateLimit.CompareTo(mtn.UpdateDateTime) <= 0);
					}
				}

				if (wordsFound && dateOk &&
				 !FoundDict.ContainsKey(mtn.Target))
				{
					string levelString = "";
					//if ((mtn.Type != MetaTreeNodeType.UserFolder &&
					// (parentMtn == null || parentMtn.Type != MetaTreeNodeType.UserFolder)) ||
					if (NumberItems && (NumberUserObjects || !mtn.IsUserObjectType)) // number user objects?
					{ // number unless in user folder
						for (int l = 0; l <= level; l++)
						{
							if (levelString != "") levelString += ".";
							levelString += (levelPos[l] + 1).ToString();
						}
					}

					FoundList.Add(mtn);
					FoundDict[mtn.Target] = levelString; // put level in target
				}
			} while (false);

			List<MetaTreeNode> children = GetMetaTreeNodeChildren(mtn);
			for (int ci = 0; ci < children.Count; ci++) // check child nodes
			{
				MetaTreeNode mtn2 = children[ci];
				levelPos[level + 1] = ci;
				FindInContents2(mtn, mtn2, ctfp, words, levelPos, level + 1);
			}

			return;
		}

		/// <summary>
		/// Normalize a string for search purposes
		/// </summary>
		/// <param name="tok">String to normalize</param>
		/// <returns>Normalized string</returns>

		string NormalizeForContentsTreeSearch(string tok)
		{
			StringBuilder ctxt = new StringBuilder(tok.Length);
			char c;
			int i1, i2;

			ctxt.Length = 0;
			i2 = 0;
			for (i1 = 0; i1 < tok.Length; i1++)
			{
				c = Char.ToLower(tok[i1]);
				if (!Char.IsLetterOrDigit(c) && c != '.' && c != '_') continue; // only alphanumeric or underscore allowed
				ctxt.Append(c);
				i2++;
			}

			return ctxt.ToString();
		}

		/// <summary>
		/// GetNormalViewFilter
		/// </summary>
		/// <returns></returns>

		public static MetaTreeNodeType GetNormalViewFilter()
		{
			//filter for the "normal" view
			MetaTreeNodeType contentsFilter =
				MetaTreeNodeType.MetaTable | MetaTreeNodeType.Annotation |
				MetaTreeNodeType.CalcField | MetaTreeNodeType.CondFormat |
				MetaTreeNodeType.Query | MetaTreeNodeType.CnList |
				MetaTreeNodeType.Url | MetaTreeNodeType.Action |
				MetaTreeNodeType.Library |
				MetaTreeNodeType.ResultsView |
				MetaTreeNodeType.UserFolder;
			return contentsFilter;
		}

		//////////////////////////////////////////////////////////////////////////
		///////////////// Events passed through from TreeList ////////////////////
		//////////////////////////////////////////////////////////////////////////

		private void Tree_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
		{ // treat focused node as single selected node
			FocusedTreeListNode = e.Node;

			if (FocusedNodeChanged != null)
				FocusedNodeChanged(this, e);
		}

		private void Tree_Click(object sender, EventArgs e)
		{
			if (Click != null)
				Click(this, e);
		}

		private void Tree_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClick != null)
				DoubleClick(this, e);
		}

		private void Tree_MouseDown(object sender, MouseEventArgs e)
		{
			if (MouseDown != null)
				MouseDown(this, e);
		}

	} // ContentsTreeControl

	/// <summary>
	/// Handle filtering of nodes via node iterator
	/// </summary>

	public class SetTreeListNodeVisibility : TreeListOperation
	{
		Dictionary<string, string> FoundDict;

		public SetTreeListNodeVisibility(Dictionary<string, string> foundDict) // dictionary from node name to level numbering
		{
			this.FoundDict = foundDict;
		}

/// <summary>
/// Set node visibility based on any current found list
/// </summary>
/// <param name="node"></param>
		public override void Execute(DevExpress.XtraTreeList.Nodes.TreeListNode node)
		{
			MetaTreeNode mtn = node.GetValue(0) as MetaTreeNode;
			if (mtn == null) return;
			if (FoundDict == null) node.Visible = true;

			else node.Visible = FoundDict.ContainsKey(mtn.Target);
		}

	}
}
