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
	public partial class UserObjectSaveDialog : DevExpress.XtraEditors.XtraForm
	{
		internal static UserObjectSaveDialog Instance;
		UserObject Uo; // model information to use
		ContentsTreeControlUoMenus CommonUoMenus; // common menus

		public UserObjectSaveDialog()
		{
			InitializeComponent();

			Instance = this;
		}

		internal static UserObjectSaveDialog GetInstance()
		{
			if (Instance == null) Instance = new UserObjectSaveDialog();
			return Instance;
		}

/// <summary>
/// Prompt for user object to save
/// </summary>
/// <param name="title"></param>
/// <param name="uo"></param>
/// <returns></returns>

		public static UserObject Show (
			string title,
			UserObject uo) 
		{
			String response, txt;
			int i1;

			GetInstance();

			Instance.Uo = uo;
			Instance.Uo.Id = 0; // since this is a SaveAs this will be a new UserObject

			if (Instance.Uo.AccessLevel == UserObjectAccess.None) // assign private access if not defined
				Instance.Uo.AccessLevel = UserObjectAccess.Private;

			string oldOwner = Instance.Uo.Owner; // old owner if any
			string newOwner = SS.I.UserName; // we will be the owner of the new object
			Instance.Uo.Owner = newOwner; 
			Permissions.UpdateAclForNewOwner(Instance.Uo, oldOwner, newOwner); // Set the ACL to give us r/w access
		
			MetaTreeNode mtn = Instance.SaveDialogContentsTree.FillTree(uo);

			string objectTypeName = UserObject.GetTypeLabel(uo.Type);
			Instance.Text = title;
			Instance.Prompt.Text = "Select a folder and enter a name for this " + objectTypeName;
			Instance.ObjectName.Text = uo.Name;
			if (mtn != null)
			{
				Instance.ProjectName.Text = mtn.Label;
				Instance.ProjectTarget.Text = mtn.Target;
			}

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			return Instance.Uo;
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			if (ObjectName.Text.Trim() == "")
			{
				MessageBoxMx.ShowError("You must supply a name");
				ObjectName.Focus();
				return;
			}

			if (ProjectName.Text.Trim() == "")
			{
				ProjectTarget.Text = "";
				if (Lex.Ne(ObjectName.Text.Trim(), "Current")) // blank project not allowed unless saving current 
				{
					MessageBoxMx.ShowError("You must supply a folder name");
					ProjectName.Focus();
					return;
				}
			}

			string txt = ProjectTarget.Text;
			if (txt.ToLower().IndexOf("project ") == 0 && txt.Length > 8)
			{
				txt = txt.Substring(8);
			}
			else if (txt.ToLower().IndexOf("folder ") == 0 && txt.Length > 7)
			{
				txt = txt.Substring(7).ToUpper();
			}
			else if (txt.ToLower().IndexOf("submenu ") == 0 && txt.Length > 8)
			{
				txt = txt.Substring(8).ToUpper();
			}
			MetaTreeNode mtn = MetaTree.GetNode(txt);
			if (mtn == null && UserObjectTree.FolderNodes.ContainsKey(txt))
				mtn = UserObjectTree.FolderNodes[txt];

			UserObject uo2 = new UserObject(Uo.Type);
			uo2.Owner = SS.I.UserName; // we will be the new owner
			uo2.Description = Uo.Description; // copy any description
			uo2.Name = ObjectName.Text;
			uo2.AccessLevel = Uo.AccessLevel;
			uo2.ACL = Uo.ACL;

			//reassigning the folder id to the object type folder happens in the "Save"
			if (mtn != null)
			{
				uo2.ParentFolder = mtn.Target.ToUpper();
				uo2.ParentFolderType = mtn.GetUserObjectFolderType();
			}

			UserObject uo3 = UserObjectDao.ReadHeader(uo2); // see if already exists
			if (uo3 != null)
			{
				string objectTypeName = UserObject.GetTypeLabel(Uo.Type);

				DialogResult dr = MessageBoxMx.Show(
					objectTypeName + " " + uo2.Name + " already exists. Do you want to replace it?",
					UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (dr == DialogResult.No) return;
				else if (dr == DialogResult.Cancel)
				{
					DialogResult = DialogResult.Cancel;
					return;
				}
				else uo2.Id = uo3.Id; // keep the id when overwriting
			}

			Uo = uo2; // info on object to save
			DialogResult = DialogResult.OK;
			return;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			return;
		}

/// <summary>
/// User has clicked node, copy selection to text controls
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveDialogContentsTree_FocusedNodeChanged(object sender, EventArgs e)
		{
			MetaTreeNode mtn = SaveDialogContentsTree.FocusedMetaTreeNode;
			if (mtn == null) return;

			if (SaveDialogContentsTree.GetMetaTreeNodeChildren(mtn).Count == 0 && // terminal node?
				(!mtn.IsFolderType || mtn.IsUserObjectType))
			{
				ObjectName.Text = mtn.Label;
				ObjectTarget.Text = mtn.Target;
			}

			while (mtn != null) // look from here on up for a project or a user folder node
			{
				if (mtn.Type == MetaTreeNodeType.Project || mtn.Type == MetaTreeNodeType.UserFolder)
				{
					ProjectName.Text = mtn.Label;

					while (mtn != null) // look from here on up for a project or a user folder node
					{
						if (Lex.StartsWith(mtn.Name, "MERGED_")) // continue up to a real node
							mtn = mtn.Parent;
						else break;
					}

					if (mtn != null) ProjectTarget.Text = mtn.Target;
					break;
				}
				mtn = mtn.Parent;
			}

			return;
		}

		private void SaveDialogContentsTree_MouseDown(object sender, MouseEventArgs e)
		{
			if (CommonUoMenus == null)
				CommonUoMenus = new ContentsTreeControlUoMenus();

			CommonUoMenus.SetupMouseDown(SaveDialogContentsTree,e); // show common menus if right-click
			return;
		}

/// <summary>
/// Save immediately on double click
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveDialogContentsTree_DoubleClick(object sender, EventArgs e)
		{
			ContentsTreeControl ctc = sender as ContentsTreeControl;
			MetaTreeNode mtn = SaveDialogContentsTree.FocusedMetaTreeNode;
			if (mtn == null) return;
			if (SaveDialogContentsTree.GetMetaTreeNodeChildren(mtn).Count == 0 && // terminal node?
				(!mtn.IsFolderType || mtn.IsUserObjectType))
			{
				ObjectName.Text = mtn.Label;
				ObjectTarget.Text = mtn.Target;
				SaveButton_Click(null, null);
			}
		}

		private void UserObjectSaveDialog_Activated(object sender, EventArgs e)
		{
			ObjectName.Focus();
		}

		private void Permissions_Click(object sender, EventArgs e)
		{
			PermissionsDialog.Show(Uo);
		}

	}
}