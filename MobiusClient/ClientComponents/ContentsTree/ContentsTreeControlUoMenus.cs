using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
/// <summary>
/// ContentsTreeControl with menu for common UserObject Open, Move and Save operations
/// </summary>

	public partial class ContentsTreeControlUoMenus : ContentsTreeControl
	{

		internal MouseEventArgs CurrentContentsTreeMouseDownEvent;
		internal MetaTreeNode CurrentContentsMetaTreeNode;
		internal TreeListNode CurrentContentsTreeListNode;
		internal ContentsTreeControl ContentsTreeControl;

		public ContentsTreeControlUoMenus()
		{
			InitializeComponent();
		}

		internal void SetupMouseDown(ContentsTreeControl control, MouseEventArgs e)
		{
			ContentsTreeControl = control;

			CurrentContentsTreeMouseDownEvent = e; // save event info for later use

			MetaTreeNode mtn = control.GetMetaTreeNodeAt(e.Location, out CurrentContentsTreeListNode);
			if (mtn == null || mtn.Target == null) return;
			CurrentContentsMetaTreeNode = mtn;

			if (e.Button != MouseButtons.Right) return; // all done if other than right button

			if (mtn.Owner != SS.I.UserName) return; // not allowed to do anything with another user's folder

			if (mtn.Type == MetaTreeNodeType.Project || mtn.Type == MetaTreeNodeType.SystemFolder ||
			 mtn.Type == MetaTreeNodeType.UserFolder) // can only create a user folder under these
				CreateUserFolderMenuItem.Visible = true; 
			else CreateUserFolderMenuItem.Visible = false;

			TreePopupMenu.Show(control, new System.Drawing.Point(e.X, e.Y));
			return;
		}

		private void CreateUserFolderMenuItem_Click(object sender, EventArgs e)
		{
			QbUtil.CreateUserFolder(CurrentContentsMetaTreeNode.Target, ContentsTreeControl);
		}

		private void PermissionsMenuItem_Click(object sender, EventArgs e)
		{
			UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Permissions", CurrentContentsMetaTreeNode, ContentsTreeControl);
		}

		private void RenameMenuItem_Click(object sender, EventArgs e)
		{
			UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Rename", CurrentContentsMetaTreeNode, ContentsTreeControl);
		}

		private void DeleteMenuItem_Click(object sender, EventArgs e)
		{
			UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("Delete", CurrentContentsMetaTreeNode, ContentsTreeControl);
		}

		private void ViewSourceMenuItem_Click(object sender, EventArgs e)
		{
			UserObjectUtil.ProcessCommonRightClickObjectMenuCommands("ViewSource", CurrentContentsMetaTreeNode, ContentsTreeControl);
		}

	}
}
