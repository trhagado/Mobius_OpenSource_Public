using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class UserObjectOpenDialog : DevExpress.XtraEditors.XtraForm
	{
		internal static UserObjectOpenDialog Instance;
		UserObjectType UoType;
		UserObject OpenUoInfo; // Attributes for UserObject to open
		MetaTreeNode SelectedNode; // the selected node

		ContentsTreeControlUoMenus CommonUoMenus; // common menus

		public UserObjectOpenDialog()
		{
			InitializeComponent();

			Instance = this;

			ContentsTreeWithSearch.FocusedNodeChanged += SearchableContentsTree_FocusedNodeChanged;
			ContentsTreeWithSearch.DoubleClick += SearchableContentsTree_DoubleClick;
			ContentsTreeWithSearch.MouseDown += SearchableContentsTree_MouseDown; ;
	}

	internal static UserObjectOpenDialog GetInstance()
		{
			if (Instance == null) Instance = new UserObjectOpenDialog();
			return Instance;
		}

			/// <summary>
		/// Dialog to open a user object
		/// </summary>
		/// <param name="objType">Type of object to open</param>
		/// <param name="prompt"></param>
		/// <param name="defaultName"></param>
		/// <returns></returns>

		public static UserObject ShowDialog(
			UserObjectType objType, 
			string title)
		{
			return ShowDialog(objType, title, null);
		}

		/// <summary>
		/// Dialog to open a user object
		/// </summary>
		/// <param name="objType">Type of object to open</param>
		/// <param name="prompt"></param>
		/// <param name="defaultName"></param>
		/// <returns></returns>

		public static UserObject ShowDialog(
			UserObjectType objType, 
			string title,
			UserObject defaultUo)
		{
			MetaTreeNode mtn = null;
			MetaTreeNodeType mtnFilter;
			string response, objectTypeName;
			int i1;

			GetInstance();

			objectTypeName = UserObject.GetTypeLabel(objType);
			string defaultName = "";
			string defaultFolder = SS.I.PreferredProjectId;
			if (defaultUo != null)
			{
				defaultName = defaultUo.Name;
				defaultFolder = defaultUo.ParentFolder;
			}

			Instance.UoType = objType;
			Instance.Text = title;
			Instance.Prompt.Text =
				"Select a " + objectTypeName + " from the choices shown below";

			mtnFilter = MetaTreeNodeType.Project; // show project folders and above
			mtnFilter |= UserObjectTree.UserObjectTypeToMetaTreeNodeType(objType); // user objects of specified type
			mtnFilter |= MetaTreeNodeType.UserFolder; // and any user folders

			string expandNodeTarget = "",  expandNodeTarget2 = "";

			if (objType != UserObjectType.UserDatabase)
			{
				mtn = UserObjectTree.FindObjectTypeSubfolder(SS.I.PreferredProjectId, objType, SS.I.UserName);
				if (mtn == null)
				{
					MetaTreeNode defProjNode = UserObjectTree.FindFolderNode(SS.I.PreferredProjectId);
					if (defProjNode == null)
					{
						defProjNode = UserObjectTree.AddSystemFolderNode(SS.I.PreferredProjectId);
					}
					mtn = UserObjectTree.FindObjectTypeSubfolder(defProjNode.Name, objType, SS.I.UserName);
					if (mtn == null)
						mtn = UserObjectTree.CreateUserFolderObjectAndNode(defProjNode, UserObject.GetTypeLabelPlural(objType), SS.I.UserName);
				}
			}

			else
			{
				if (UserObjectTree.FolderNodes.ContainsKey(defaultFolder))
					mtn = UserObjectTree.FolderNodes[defaultFolder];
				else DebugLog.Message("Couldn't find default folder in tree: " + defaultFolder); // just log & ignore
			}

			if (mtn?.Target != null)
			{
				expandNodeTarget = mtn.Target;
				if (mtn.Parent != null)
					expandNodeTarget2 = mtn.Parent.Target;
			}

			Instance.ContentsTreeWithSearch.ContentsTreeCtl.FillTree("root", mtnFilter, null, expandNodeTarget, expandNodeTarget2, false, false);
			Instance.ContentsTreeWithSearch.ContentsTreeCtl.SelectNode(expandNodeTarget);
			Instance.ContentsTreeWithSearch.CommandLineControl.Text = "";
			Instance.ObjectName.Text = defaultName;
			Instance.SelectedNode = null; // node selected from tree

			QbUtil.SetProcessTreeItemOperationMethod(TreeItemOperation);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			QbUtil.RestoreProcessTreeItemOperationMethod();

			if (dr == DialogResult.Cancel) return null;

			return Instance.OpenUoInfo;
		}

		/// <summary>
		/// Pass the tree operation to the proper handler
		/// </summary>
		/// <param name="op"></param>
		/// <param name="args"></param>

		public static void TreeItemOperation(
			string op,
			string args)
		{
			if (Lex.Eq(op, "Open"))
			{
				MetaTreeNode mtn = MetaTreeNodeCollection.GetNode(args);
				if (mtn == null || !mtn.IsUserObjectType) return;

				Instance.ObjectName.Text = mtn.Label;
				Instance.SelectedNode = mtn;
				Instance.OK_Click(null, null);
				return;
			}

			else Instance.ContentsTreeWithSearch.ContentsTreeCtl.FindInContents(args);

			return;
		}

		/// <summary>
		/// ContentsTreeItemSelected
		/// </summary>
		/// <param name="nodeTarget"></param>

		private void ContentsTreeItemSelected(string nodeTarget)
		{
			MetaTreeNode node = MetaTreeNodeCollection.GetNode(nodeTarget);
			if (node == null || !node.IsUserObjectType) return;

			ObjectName.Text = node.Name; // show node name
			SelectedNode = node; // The selected node
		}

		private void UserObjectOpenDialog_Activated(object sender, EventArgs e)
		{
			ContentsTreeWithSearch.CommandLineControl.Focus(); // put focus on search control
			return;
		}

/// <summary>
/// Move to selection when focused
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SearchableContentsTree_FocusedNodeChanged(object sender, EventArgs e)
		{
			MetaTreeNode mtn = ContentsTreeWithSearch.ContentsTreeCtl.FocusedMetaTreeNode;
			if (mtn == null) return;
			if (ContentsTreeWithSearch.ContentsTreeCtl.GetMetaTreeNodeChildren(mtn).Count == 0 && // terminal node?
				(!mtn.IsFolderType || mtn.IsUserObjectType))
			{
				ObjectName.Text = mtn.Label;
				SelectedNode = mtn;
			}
		}

		private void ContentsTreeWithSearch_DoubleClick(object sender, EventArgs e)
		{
			SearchableContentsTree_DoubleClick(sender, e);
		}

		private void ContentsTreeWithSearch_MouseDown(object sender, MouseEventArgs e)
		{
			SearchableContentsTree_MouseDown(sender, e);
		}

		/// <summary>
		/// Open immediately on double-click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SearchableContentsTree_DoubleClick(object sender, EventArgs e)
		{
			ContentsTreeControl ctc = sender as ContentsTreeControl;
			MetaTreeNode mtn = ContentsTreeWithSearch.ContentsTreeCtl.FocusedMetaTreeNode;
			if (mtn == null) return;
			if (ContentsTreeWithSearch.ContentsTreeCtl.GetMetaTreeNodeChildren(mtn).Count == 0 && // terminal node?
				(!mtn.IsFolderType || mtn.IsUserObjectType))
			{
				ObjectName.Text = mtn.Label;
				SelectedNode = mtn;
				OK_Click(null, null);
			}
		}

		private void SearchableContentsTree_MouseDown(object sender, MouseEventArgs e)
		{
			if (CommonUoMenus == null)
				CommonUoMenus = new ContentsTreeControlUoMenus();

			CommonUoMenus.SetupMouseDown(ContentsTreeWithSearch.ContentsTreeCtl, e); // show common menus if right-click
			return;
		}

		/// <summary>
		/// Validate the selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OK_Click(object sender, EventArgs e)
		{
			UserObjectType type;
			int id;

			if (ObjectName.Text == "")
			{
				MessageBoxMx.Show("A name must be supplied", UmlautMobius.String, MessageBoxButtons.OK);
				return;
			}

			if (SelectedNode != null) // selected from tree
			{
				UserObject.ParseObjectTypeAndIdFromInternalName(SelectedNode.Target, out type, out id);

				OpenUoInfo = UserObjectDao.ReadHeader(id);
				DialogResult = DialogResult.OK;
				return;
			}

			else // name typed in, determine the location
			{ 
				MetaTreeNode folderNode = ContentsTreeWithSearch.ContentsTreeCtl.FocusedMetaTreeNode;
				bool foundFolder = false;
				while (folderNode != null)
				{
					if (folderNode.IsFolderType)
					{
						foundFolder = true;
						break;
					}
					folderNode = folderNode.Parent; // move up in tree
				}

				if (!foundFolder)
				{ // could not determine where to look for the object
					MessageBoxMx.Show("You must select the folder in which " + ObjectName.Text + " is to be found!",
							"Which Folder?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					DialogResult = DialogResult.None;
					return;
				}
				else
				{ // have a folder, look for entered name
					string folderTarget = folderNode.Target;
					if (!UserObjectTree.FolderNodes.ContainsKey(folderTarget))
					{
						MessageBoxMx.ShowError("Unexpected error - Failed to find folder: " + folderTarget);
						return;
					}
					MetaTreeNode mtn = UserObjectTree.FolderNodes[folderTarget];
					MetaTreeNodeType mtnType = UserObjectTree.UserObjectTypeToMetaTreeNodeType(UoType);
					mtn = UserObjectTree.GetNodeByLabel(mtn, mtnType, ObjectName.Text);
					if (mtn != null)
					{
						UserObject.ParseObjectTypeAndIdFromInternalName(mtn.Target, out type, out id);
						OpenUoInfo = UserObjectDao.ReadHeader(id);
						DialogResult = DialogResult.OK;
					}

					else
					{
						string msg =
							ObjectName.Text + "\n" +
							"File not found.\n" +
							"Please verify that the correct name was given.";

						MessageBoxMx.Show(msg, Instance.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						DialogResult = DialogResult.None;
						return;
					}
				}
			}

		}

		private void ObjectName_KeyPress(object sender, KeyPressEventArgs e)
		{
			SelectedNode = null; // if text typed then clear selected node
		}

	}

}