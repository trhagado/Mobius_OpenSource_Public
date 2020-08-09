using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraEditors.Controls;

using System;
using System.Linq;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// QueryBuild Contents Tree control including search and user object/add buttons
	/// </summary>

	public partial class ContentsTreeWithSearch : XtraUserControl
	{
		internal static ContentsTreeWithSearch Instance;

		internal MouseEventArgs CurrentContentsTreeMouseDownEvent; // most-recent MouseDownEvent
		internal MetaTreeNode CurrentContentsMetaTreeNode; // MetaTreeNode clicked in most-recent MouseDownEvent
		internal TreeListNode CurrentContentsTreeListNode; // TreeListNode  clicked in most-recent MouseDownEvent
		internal MetaTreeNode[] CurrentContentsMetaTreeNodes; // MetaTreeNodes currently selected

		public event EventHandler FocusedNodeChanged; // Events that may be overridden by a higher level control
		public new event EventHandler Click;
		public new event EventHandler DoubleClick;
		public new event MouseEventHandler MouseDown;

		public QuickSearchPopup QuickSearchPopup;

		internal QbContentsTreeMenus ContentsTreeContextMenus; // context menus for main contents tree

		public delegate void InvokeShowNormalDelegate();

		public ContentsTreeWithSearch()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			ContentsTreeContextMenus = new QbContentsTreeMenus(); // context menus for main contents tree
			//ContentsTreeContextMenus.QbContentsTree = this;
			Instance = this;
		}

/// <summary>
/// Initialize
/// </summary>

		public void Activate()
		{
			CommandLineControl.Focus();
		}

		/// <summary>
		/// Reset the tree with preferred project selected and a blank commandline
		/// </summary>

		public void ResetTreeAndCommandLine()
		{
			QuickSearchPopup.Hide();

			CommandLineControl.Text = "";
			CommandLineControl.ClosePopup();

			ContentsTreeCtl.RefillTree();

			CommandLineControl.Focus(); // do last

			return;
		}

		/// <summary>
		/// Search the contents tree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ContentsFind_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsFind");
			ContentsTreeCtl.FindInContents("");
		}

		private void AddTable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddTableButton");

			MetaTreeNode node = ContentsTreeCtl.FocusedMetaTreeNode;
			if (node == null) return;
			if (node.Target == null || node.Target == "") return;
			ExecuteNodeAction(node);
		}

		private void ContentsTree_MouseDown(object sender, MouseEventArgs e)
		{
			CurrentContentsTreeMouseDownEvent = e; // save event info for later use

			MetaTreeNode mtn = ContentsTreeCtl.GetMetaTreeNodeAt(ContentsTreeCtl.PointToClient(Cursor.Position), out CurrentContentsTreeListNode);
			if (mtn == null || mtn.Target == null) return;
			CurrentContentsMetaTreeNode = mtn;

			if (MouseDown != null) // if overridden, call the method
			{
				MouseDown(sender, e);
				return;
			}

			else
			 QbUtil.CallCurrentProcessTreeItemOperationMethod("Open", mtn.Target); // no popup menus for now, just process as default open operation

			return;
		}

		private void ContentsTree_FocusedNodeChanged(object sender, EventArgs e)
		{
			if (FocusedNodeChanged != null)
			{
				FocusedNodeChanged(sender, e);
				return;
			}

			else return;

			//ContentsTreeControl ctc = sender as ContentsTreeControl;
			//if (ctc == null) return;
			//MetaTreeNode node = ctc.GetMetaTreeNode(ctc.FocusedTreeListNode);
			//if (node == null) return;

			//if (String.IsNullOrEmpty(node.Target)) { }
			//else if (node.Type == MetaTreeNodeType.MetaTable ||
			//	node.Type == MetaTreeNodeType.Annotation ||
			//	node.Type == MetaTreeNodeType.CalcField ||
			//	node.Type == MetaTreeNodeType.CondFormat ||
			//	node.Type == MetaTreeNodeType.CnList || // for open list
			//	node.Type == MetaTreeNodeType.Query || // for open query
			//	node.Type == MetaTreeNodeType.CondFormat) // for open condFormat
			//{
			//	////AddTableButton.Enabled = true; // set enabled flag for "Add >>>" (to query) button
			//}

			//////else AddTableButton.Enabled = false;
		}

		private void ContentsTree_Click(object sender, EventArgs e)
		{
			if (Click != null)
			{
				Click(sender, e);
				return;
			}

			MouseEventArgs me = e as MouseEventArgs;
			if (me == null) return;
			TreeListHitInfo hi = ContentsTreeCtl.TreeList.CalcHitInfo(me.Location);
			if (hi == null || hi.Node == null)
			{
				ContentsTreeCtl.TreeList.OptionsSelection.MultiSelect = false;
				return;
			}

			MetaTreeNode currentNode = ContentsTreeCtl.GetMetaTreeNode(hi.Node);
			if (currentNode == null) return;
			if (string.IsNullOrEmpty(currentNode.Target))
			{
				DisableMultiselect(hi.Node);
				return;
			}

			// support multiple selections until we find otherwise
			ContentsTreeCtl.TreeList.OptionsSelection.MultiSelect = true;
			CurrentContentsMetaTreeNodes = ContentsTreeCtl.GetCurrentSelectedNodes();

			if (CurrentContentsMetaTreeNodes.Count() > 1)
			{
				// process multiple nodes
				bool enabled = false;

				// If the user selected all Data Objects than we will allow muliselect
				if (MetaTreeNode.IsDataTableNodeType(currentNode.Type))
				{
					foreach (MetaTreeNode node in CurrentContentsMetaTreeNodes)
					{
						if (node == currentNode) break;
						enabled = MetaTreeNode.IsDataTableNodeType(node.Type);
						if (!enabled) break;
					}
				}

				// If the Data Object check above failed, enable multiselect if we have all User Objects that are leafe nodes (no folders).
				if (!enabled && MetaTreeNode.IsUserObjectNodeType(currentNode.Type) && MetaTreeNode.IsLeafNodeType(currentNode.Type))
				{
					foreach (MetaTreeNode node in CurrentContentsMetaTreeNodes)
					{
						if (node == currentNode) break;
						enabled = MetaTreeNode.IsUserObjectNodeType(node.Type) &&
										MetaTreeNode.IsLeafNodeType(node.Type);
						if (!enabled) break;
					}
				}

				// Invalid combination of nodes, disable multiselect.
				if (!enabled)
				{
					DisableMultiselect(hi.Node);
					//MessageBox.Show("Multiselection is supported only for the following combinations\r\n\u2022 Lists, Queries, Annotation Tables, Calc Fields (Cut, Copy, Paste, Delete)\r\n\u2022 Data Tables, Anotation Tables, Calc Fields (Add to a Query)");
				}

			}
			else
			{
				//process single node
				if ((currentNode.Type == MetaTreeNodeType.Url || currentNode.Type == MetaTreeNodeType.Action) &&
						CurrentContentsTreeMouseDownEvent.Button != MouseButtons.Right)
					ExecuteNodeAction(currentNode); // if url send command on 1st click
			}
		}

		private void DisableMultiselect(TreeListNode currentnode)
		{
			if (ContentsTreeCtl.TreeList.Selection.Count > 1) MessageBox.Show("Multiselection is supported only for the following combinations\r\n\u2022 Lists, Queries, Annotation Tables, Calc Fields (Cut, Copy, Paste, Delete)\r\n\u2022 Data Tables, Anotation Tables, Calc Fields (Add to a Query)");
			ContentsTreeCtl.TreeList.Selection.Clear();
			ContentsTreeCtl.TreeList.Selection.Add(currentnode);
			ContentsTreeCtl.TreeList.FocusedNode = currentnode;
			ContentsTreeCtl.TreeList.OptionsSelection.MultiSelect = false;
		}

		public bool SingleNodeSelected()
		{
			return (ContentsTreeCtl.TreeList.Selection.Count == 1);
		}

		private void ContentsTree_DoubleClick(object sender, EventArgs e)
		{
			if (DoubleClick != null)
			{
				DoubleClick(sender, e);
				return;
			}

			MetaTreeNode node = ContentsTreeCtl.FocusedMetaTreeNode;
			if (node == null || node.Target == null || node.Target == "") return;

			if (node.Type != MetaTreeNodeType.Url && node.Type != MetaTreeNodeType.Action) // if not immediate action node then post message
				ExecuteNodeAction(node);
		}

		void ExecuteNodeAction(MetaTreeNode node)
		{
			OpenMetaTreeNode(node);
		}

		/// <summary>
		/// Open the specified target
		/// </summary>
		/// <param name="mtn"></param>

		public static void OpenMetaTreeNode(
			MetaTreeNode mtn)
		{
			UserObject uo;
			string tok;
			int i1;

			if (mtn == null) return;
			MetaTreeNodeType nodeType = mtn.Type;
			string target = mtn.Target;

			//if (!Permissions.UserHasReadAccess(SS.I.UserName, target))
			//{
			//  MessageBoxMx.ShowError("You are not authorized to open: " + mtn.Label);
			//  return;
			//}

			if (nodeType == MetaTreeNodeType.MetaTable || // data table types
			 nodeType == MetaTreeNodeType.CalcField ||
			 nodeType == MetaTreeNodeType.CondFormat ||
			 nodeType == MetaTreeNodeType.Annotation ||
			 nodeType == MetaTreeNodeType.ResultsView)
			{
				QbUtil.CallCurrentProcessTreeItemOperationMethod("Open", target);
			}

			else if (MetaTreeNode.IsFolderNodeType(nodeType))
			{
				if (Lex.StartsWith(target, "USERDATABASE_")) // edit a user compound database
				{
					uo = ParseAndReadUserObject(target);
					if (uo == null) return;
					UserData.OpenExistingUserDatabase(uo);
				}
			}

			else if (nodeType == MetaTreeNodeType.Url)
			{ // open url or execute click function
				if ((i1 = Lex.IndexOf(target, "ClickFunction")) >= 0)
				{
					string cmd = target.Substring(i1 + "ClickFunction".Length + 1); // get function name
					ClickFunctions.Process(cmd, null);
				}

				else if (Lex.Contains(target, "SpotfireWeb")) // link to Spotfire webplayer
					SpotfireLinkUI.OpenLink(target);

				else SystemUtil.StartProcess(target); // open in default user browser
			}

			else if (nodeType == MetaTreeNodeType.Action)
			{ // execute action
				return; //  CommandLineControl.Execute(mtn.Target);
			}

			else if (nodeType == MetaTreeNodeType.CnList) // open list 
			{
				uo = ParseAndReadUserObject(target);
				if (uo == null) return;
				tok = uo.InternalName;
				CidListEditor.Edit(tok);
			}

			else if (nodeType == MetaTreeNodeType.Query) // open query 
			{
				uo = ParseAndReadUserObject(target);
				if (uo == null) return;

				if (uo.Type == UserObjectType.Query) // normal query
				{
					tok = uo.InternalName;

					string nextCommand = QbUtil.OpenQuery(tok);
					while (!(String.IsNullOrEmpty(nextCommand)))
						nextCommand = CommandExec.ExecuteCommand(nextCommand);
				}

				else if (uo.Type == UserObjectType.MultiTable) // multitable query
				{
					QbUtil.AddAndRenderTables(target);
				}
			}

			else if (nodeType == MetaTreeNodeType.Library)
			{
				CommandExec.ExecuteCommandAsynch("ContentsViewLibAsList " + mtn.Name);
			}

		}

		static UserObject ParseAndReadUserObject(string tok)
		{
			int i1 = tok.IndexOf("_"); // parse the <Type>_<ObjectId> string
			if (i1 < 0) throw new Exception("Invalid input");
			int id = Int32.Parse(tok.Substring(i1 + 1));
			UserObject uo = UserObjectDao.ReadHeader(id);
			if (uo == null)
				MessageBoxMx.ShowError("UserObject " + tok + " not found");
			return uo;
		}

		/// <summary>
		/// This method is called to send a ContentsTree node click command with object type & name
		/// </summary>
		/// <param name="command"></param>

		internal void ExecuteContentsTreeCommand(
			string command)
		{
			// This indicates the user has come from the QuickSearch Menu. Either nothing is selected in the Tree
			// or the node we are trying to act upon is not ne in the tree selection.
			if (CurrentContentsMetaTreeNodes == null ||
					!CurrentContentsMetaTreeNodes.Contains(CurrentContentsMetaTreeNode))
			{
				CommandExec.ExecuteCommandAsynch(command + " " + CurrentContentsMetaTreeNode.Target);
			}
			else if (CurrentContentsMetaTreeNodes != null && CurrentContentsMetaTreeNodes.Length >= 1)
			{
				command += " ";
				foreach (var treeNode in CurrentContentsMetaTreeNodes)
				{
					command += treeNode.Target + ";";
				}
				char[] lastSemicolon = { ';' };
				command = command.TrimEnd(lastSemicolon);
				CommandExec.ExecuteCommandAsynch(command);
			}
		}

		/// <summary>
		/// Set visibility of menu items based on owner
		/// </summary>
		public void SetItemEnabledState(
				MetaTreeNode node,
				ToolStripMenuItem contentsCutQuery,
				ToolStripMenuItem contentsCopyQuery,
				ToolStripMenuItem contentsDeleteQuery,
				ToolStripMenuItem contentsRenameQuery)
		{
			contentsCutQuery.Enabled =
			contentsRenameQuery.Enabled =
			contentsDeleteQuery.Enabled = Permissions.UserHasWriteAccess(SS.I.UserName, node.Target);
			contentsCopyQuery.Enabled = true; // can alway copy if user can see it
		}

		private void ContentsTree_MouseClick(object sender, MouseEventArgs e)
		{
			CurrentContentsMetaTreeNodes = ContentsTreeCtl.GetCurrentSelectedNodes();
		}

		public void SetItemEnabledState(
				ToolStripMenuItem contentsCutQuery,
				ToolStripMenuItem contentsCopyQuery,
				ToolStripMenuItem contentsDeleteQuery,
				ToolStripMenuItem contentsRenameQuery)
		{
			foreach (TreeListNode node in ContentsTreeCtl.TreeList.Selection)
			{
				// if any node in the selection turns the menu to false, it stays false
				MetaTreeNode mtn = ContentsTreeCtl.GetMetaTreeNode(node);
				bool hasWriteAccess = Permissions.UserHasWriteAccess(SS.I.UserName, mtn.Target);
				if (contentsCutQuery.Enabled) contentsCutQuery.Enabled = hasWriteAccess;
				if (contentsDeleteQuery.Enabled) contentsDeleteQuery.Enabled = hasWriteAccess;
				if (contentsRenameQuery.Enabled) contentsRenameQuery.Enabled = hasWriteAccess;
				if (contentsCopyQuery.Enabled) contentsCopyQuery.Enabled =
						(hasWriteAccess || (MetaTreeNode.IsUserObjectNodeType(mtn.Type) && !MetaTreeNode.IsLeafNodeType(mtn.Type)));
			}
		}

		private void CommandLineControl_Enter(object sender, EventArgs e) // Received focus
		{
			QuickSearchPopup = // link to quicksearch control that processes keyboard input
				QuickSearchPopup.Initialize(CommandLineControl, CommandLineControl.FindForm(), ContentsTreeCtl);  

			QuickSearchPopup.StructurePopupEnabled = false; // no structure popup here
			QuickSearchPopup.ConsiderShellFocus = false; // need to turn this off
		}

		private void CommandLineControl_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			if (e.Button.Kind == ButtonPredefines.Search)
			{
				if (QuickSearchPopup != null)
					QuickSearchPopup.ExecuteCommandLine();

				CommandLineControl.Focus();
			}

			else if (e.Button.Kind == ButtonPredefines.Combo)
				CommandLineControl.ShowPopup();

			else // delete/clear text & close popup
			{
				DelayedCallback.Schedule(ResetTreeAndCommandLine);
			}

			return;
		}

		private void CommandLineControl_Properties_BeforePopup(object sender, EventArgs e)
		{
			return;
		}

		private void CommandLineControl_AddingMRUItem(object sender, AddingMRUItemEventArgs e)
		{
			return;
		}

		private void CommandLineControl_BeforePopup(object sender, EventArgs e)
		{
			return;
		}

		private void CommandLineControl_Popup(object sender, EventArgs e)
		{
			return;
		}

		private void CommandLineControl_QueryPopUp(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true; // don't show MRU popup since it overlays quicksearch results
		}
	}
}
