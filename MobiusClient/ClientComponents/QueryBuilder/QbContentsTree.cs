using System.Linq;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// QueryBuild Contents Tree control including search and user object/add buttons
	/// </summary>

	public partial class QbContentsTree : XtraUserControl
	{
		internal static QbContentsTree Instance;

		[DefaultValue(true)]
		public bool OptionShowUserObjectButtons { get { return _showUserObjectButtons; } set { _showUserObjectButtons = value; } }
		bool _showUserObjectButtons = true; // show the user object/add buttons at bottom of control

		internal MouseEventArgs CurrentContentsTreeMouseDownEvent; // most-recent MouseDownEvent
		internal MetaTreeNode CurrentContentsMetaTreeNode; // MetaTreeNode clicked in most-recent MouseDownEvent
		internal TreeListNode CurrentContentsTreeListNode; // TreeListNode  clicked in most-recent MouseDownEvent
		internal QbContentsTreeMenus ContentsTreeContextMenus; // context menus for main contents tree
		internal MetaTreeNode[] CurrentContentsMetaTreeNodes; // MetaTreeNodes currently selected

		public QbContentsTree()
		{
			InitializeComponent();

			ContentsTreeContextMenus = new QbContentsTreeMenus(); // context menus for main contents tree
			ContentsTreeContextMenus.QbContentsTree = this;
			Instance = this;
		}

		/// <summary>
		/// To allow non-GUI threads to invoke ShowNormal
		/// </summary>
		public void InvokeShowNormal()
		{
			this.Invoke(new SimpleDelegate(this.ShowNormal));
		}

		/// <summary>
		/// Show normal menu with preferred project selected
		/// </summary>

		public void ShowNormal()
		{
			QbContentsTreeCtl.ShowFullTree();
			return;
		}

		/// <summary>
		/// Show normal menu with specified node open and at top
		/// </summary>

		public void ShowNormal(
			string topNodeTarget)
		{
			QbContentsTreeCtl.ShowFullTree(topNodeTarget);
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
			QbContentsTreeCtl.FindInContents("");
		}

		private void ContentsFindReset_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsFindReset");
			ShowNormal();
		}

		private void AnnotationButton_Click(object sender, EventArgs e)
		{
			AnnotationButtonContextMenu.Show(AnnotationButton,
				new System.Drawing.Point(0, AnnotationButton.Height));
		}

		private void CalcFieldButton_Click(object sender, EventArgs e)
		{
			CalcFieldButtonContextMenu.Show(CalcFieldButton,
				new System.Drawing.Point(0, CalcFieldButton.Height));
		}

		private void UserDatabaseButton_Click(object sender, EventArgs e)
		{
			UserDatabaseButtonContextMenu.Show(UserDatabaseButton,
				new System.Drawing.Point(0, UserDatabaseButton.Height));
		}

		private void AddTable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddTableButton");

			MetaTreeNode node = QbContentsTreeCtl.FocusedMetaTreeNode;
			if (node == null) return;
			if (node.Target == null || node.Target == "") return;
			ExecuteNodeAction(node);
		}

		private void ContentsTree_MouseDown(object sender, MouseEventArgs e)
		{
			CurrentContentsTreeMouseDownEvent = e; // save event info for later use

			MetaTreeNode mtn = QbContentsTreeCtl.GetMetaTreeNodeAt(QbContentsTreeCtl.PointToClient(Cursor.Position), out CurrentContentsTreeListNode);
			if (mtn == null || mtn.Target == null) return;
			CurrentContentsMetaTreeNode = mtn;

			if (e.Button != MouseButtons.Right) return; // all done if other than right button

			ContextMenuStrip ms = ContentsTreeContextMenus.GetNodeContextMenu(mtn);

			bool displayingSearchResults = (QbContentsTreeCtl.DisplayingAsList);
			ContentsTreeContextMenus.ShowInTreeMenuItem.Visible = displayingSearchResults; // if showing search results enable item to show in tree

			if (ms != null)
				ms.Show(QbContentsTreeCtl, new System.Drawing.Point(e.X, e.Y));
			return;
		}

		private void ContentsTree_FocusedNodeChanged(object sender, EventArgs e)
		{
			ContentsTreeControl ctc = sender as ContentsTreeControl;
			if (ctc == null) return;
			MetaTreeNode node = ctc.GetMetaTreeNode(ctc.FocusedTreeListNode);
			if (node == null) return;

			if (String.IsNullOrEmpty(node.Target)) { }
			else if (node.Type == MetaTreeNodeType.MetaTable ||
				node.Type == MetaTreeNodeType.Annotation ||
				node.Type == MetaTreeNodeType.CalcField ||
				node.Type == MetaTreeNodeType.CnList || // for open list
				node.Type == MetaTreeNodeType.Query) // for open condFormat
			{
				AddTableButton.Enabled = true; // set enabled flag for "Add >>>" (to query) button
			}

			else AddTableButton.Enabled = false;
		}

		private void ContentsTree_Click(object sender, EventArgs e)
		{
			MouseEventArgs me = e as MouseEventArgs;
			if (me == null) return;
			TreeListHitInfo hi = QbContentsTreeCtl.TreeList.CalcHitInfo(me.Location);
			if (hi == null || hi.Node == null)
			{
				QbContentsTreeCtl.TreeList.OptionsSelection.MultiSelect = false;
				return;
			}

			MetaTreeNode currentNode = QbContentsTreeCtl.GetMetaTreeNode(hi.Node);
			if (currentNode == null) return;
			if (string.IsNullOrEmpty(currentNode.Target))
			{
				DisableMultiselect(hi.Node);
				return;
			}

			// support multiple selections until we find otherwise
			QbContentsTreeCtl.TreeList.OptionsSelection.MultiSelect = true;
			CurrentContentsMetaTreeNodes = QbContentsTreeCtl.GetCurrentSelectedNodes();

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
			if (QbContentsTreeCtl.TreeList.Selection.Count > 1 && ModifierKeys.HasFlag(Keys.Control))
                MessageBox.Show("Multiselection is supported only for the following combinations\r\n\u2022 Lists, Queries, Annotation Tables, Calc Fields (Cut, Copy, Paste, Delete)\r\n\u2022 Data Tables, Anotation Tables, Calc Fields (Add to a Query)");
			QbContentsTreeCtl.TreeList.Selection.Clear();
			QbContentsTreeCtl.TreeList.Selection.Add(currentnode);
			QbContentsTreeCtl.TreeList.FocusedNode = currentnode;
			QbContentsTreeCtl.TreeList.OptionsSelection.MultiSelect = false;
		}

		public bool SingleNodeSelected()
		{
			return (QbContentsTreeCtl.TreeList.Selection.Count == 1);
		}

		private void ContentsTree_DoubleClick(object sender, EventArgs e)
		{
			MetaTreeNode node = QbContentsTreeCtl.FocusedMetaTreeNode;
			if (node == null || node.Target == null || node.Target == "") return;

			if (node.Type != MetaTreeNodeType.Url && node.Type != MetaTreeNodeType.Action) // if not immediate action node then post message
				ExecuteNodeAction(node);
		}

		void ExecuteNodeAction(MetaTreeNode node)
		{
			OpenMetaTreeNode(node);
			return;
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
			 nodeType == MetaTreeNodeType.Annotation ||
			 nodeType == MetaTreeNodeType.ResultsView)
			{
				QbUtil.AddAndRenderTables(target); //.CallCurrentProcessTreeItemOperationMethod("Open", target);
			}

			else if (nodeType == MetaTreeNodeType.CondFormat)
			{
				uo = ParseAndReadUserObject(mtn.Name);
				if (uo == null) return;
				tok = uo.InternalName;
				CondFormatEditor.EditUserObject(uo.InternalName);
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
				CommandLine.Execute(mtn.Target);
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
		/// Annotation table context menu clicks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void menuNewAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddNewAnnotation");
			CommandExec.ExecuteCommandAsynch("NewAnnotation");
		}

		private void menuExistingAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddExistingAnnotation");
			CommandExec.ExecuteCommandAsynch("ExistingAnnotation");
		}

		private void menuOpenAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsOpenAnnotation");
			CommandExec.ExecuteCommandAsynch("OpenAnnotation");
		}

		/// <summary>
		/// Annotation table context menu clicks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void menuNewCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddNewCalcField");
			CommandExec.ExecuteCommandAsynch("NewCalcField");
		}

		private void menuExistingCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsAddExistingCalcField");
			CommandExec.ExecuteCommandAsynch("ExistingCalcField");
		}

		private void menuOpenCalcField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsOpenCalcField");
			CommandExec.ExecuteCommandAsynch("OpenCalcField");
		}

		/// <summary>
		/// UserCompound Database context menu clicks 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void NewUserDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsNewUserDatabase");
			CommandExec.ExecuteCommandAsynch("NewUserDatabase");
		}

		private void OpenUserDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ContentsOpenUserDatabase");
			CommandExec.ExecuteCommandAsynch("OpenUserDatabase");
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
			CurrentContentsMetaTreeNodes = QbContentsTreeCtl.GetCurrentSelectedNodes();
		}

		public void SetItemEnabledState(
				ToolStripMenuItem contentsCutQuery,
				ToolStripMenuItem contentsCopyQuery,
				ToolStripMenuItem contentsDeleteQuery,
				ToolStripMenuItem contentsRenameQuery,
                ToolStripMenuItem contentsAddQuery)
		{
            // set all to true or they will never be tested below
            contentsCutQuery.Enabled =
            contentsDeleteQuery.Enabled =
            contentsDeleteQuery.Enabled =
            contentsRenameQuery.Enabled =
            contentsCopyQuery.Enabled =
            contentsAddQuery.Enabled = true;

            foreach (TreeListNode node in QbContentsTreeCtl.TreeList.Selection)
			{


                // if any node in the selection turns the menu to false, it stays false
                MetaTreeNode mtn = QbContentsTreeCtl.GetMetaTreeNode(node);
				bool hasWriteAccess = Permissions.UserHasWriteAccess(SS.I.UserName, mtn.Target);
				if (contentsCutQuery.Enabled) contentsCutQuery.Enabled = hasWriteAccess;
				if (contentsDeleteQuery.Enabled) contentsDeleteQuery.Enabled = hasWriteAccess;
				if (contentsRenameQuery.Enabled) contentsRenameQuery.Enabled = hasWriteAccess;
				if (contentsCopyQuery.Enabled) contentsCopyQuery.Enabled =
						hasWriteAccess || 
                        (MetaTreeNode.IsUserObjectNodeType(mtn.Type) && !MetaTreeNode.IsLeafNodeType(mtn.Type));
			    if (contentsAddQuery.Enabled)
			        contentsAddQuery.Enabled =
			            mtn.Type == MetaTreeNodeType.MetaTable ||
			            mtn.Type == MetaTreeNodeType.Annotation ||
			            mtn.Type == MetaTreeNodeType.CalcField;
			}
		}

/// <summary>
/// Adjust control to hide or show UserObject buttons at bottom
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void QbContentsTree_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible) return;

			if (!OptionShowUserObjectButtons) // hide the table and field labels if requested
			{
				AnnotationButton.Visible = false;
				CalcFieldButton.Visible = false;
				UserDatabaseButton.Visible = false;
				AddTableButton.Visible = false;

				QbContentsTreeCtl.Height += this.Bottom - QbContentsTreeCtl.Bottom;

				return;
			}

		}
	}
}
