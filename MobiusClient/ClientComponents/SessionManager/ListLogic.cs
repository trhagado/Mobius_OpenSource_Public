using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
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
	public partial class ListLogic : DevExpress.XtraEditors.XtraForm
	{
		static ListLogic Instance;

		List<MetaTreeNode> TempNodes; // temp nodes
		string ParentFolder = ""; // folder that contains temp nodes

		public ListLogic()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the form
/// </summary>

		public static new void Show()
		{
			if (Instance == null) Instance = new ListLogic();
			Instance.Setup();

			CidListCommand.WriteCurrentList(); // be sure current is on the server
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			return;
		}

/// <summary>
/// Setup the form 
/// </summary>

		void Setup()
		{
			// Temporary surrogates for current and other temp lists that gets added to the preferred project

			TempNodes = new List<MetaTreeNode>();

			foreach (TempCidList tcl in SS.I.TempCidLists)
			{
				UserObject uo = new UserObject(UserObjectType.CnList);
				uo.ParentFolder = SS.I.PreferredProjectId;
				uo.ParentFolderType = (uo.ParentFolder.ToUpper().StartsWith("FOLDER_")) ? FolderTypeEnum.User : FolderTypeEnum.System;
				uo.Id = tcl.Id;
				uo.Name = "*" + tcl.Name;
				uo.AccessLevel = UserObjectAccess.Private;
				uo.Count = tcl.Count;
				MetaTreeNode mtn = UserObjectTree.GetValidUserObjectTypeFolder(uo);
				mtn = UserObjectTree.AddObjectToTree(uo);
				ParentFolder = uo.ParentFolder;
				TempNodes.Add(mtn);
			}

			ListTree1.FillTree(UserObjectType.CnList);
			ListTree2.FillTree(UserObjectType.CnList);

			StatusMessage.Caption = "";

			return;
		}

		private void ListAndButton_Click(object sender, EventArgs e)
		{
			ListAnd.Checked = true;
		}

		private void ListOrButton_Click(object sender, EventArgs e)
		{
			ListOr.Checked = true;
		}

		private void ListNotButton_Click(object sender, EventArgs e)
		{
			ListNot.Checked = true;
		}

		private void ListLogic_Resize(object sender, EventArgs e)
		{ // keep panels centered & proportional
			LeftPanel.Width = Width / 2 - CenterPanel.Width / 2;
			RightPanel.Width = LeftPanel.Width - 8;
			CenterPanel.Left = LeftPanel.Right;
			RightPanel.Left = CenterPanel.Right;
		}

		private void Combine_Click(object sender, EventArgs e)
		{
			ListLogicType op;

			MetaTreeNode mtn1 = GetListMetaTreeNode(ListTree1.TreeList.FocusedNode);
			if (mtn1 == null) return;

			MetaTreeNode mtn2 = GetListMetaTreeNode(ListTree2.TreeList.FocusedNode);
			if (mtn2 == null) return;

			if (ListAnd.Checked) op = ListLogicType.Intersect;
			else if (ListOr.Checked) op = ListLogicType.Union;
			else op = ListLogicType.Difference;
			int count = CidListDao.ExecuteListLogic(mtn1.Target, mtn2.Target, op);

			UserObject uo = CidListCommand.ReadCurrentListHeader();
			TempCidList tl = CidListCommand.GetTempList("Current");
			if (uo != null && tl != null)
			{
				tl.Count = uo.Count;
				tl.Id = uo.Id;
			}

			UpdateNode("Current"); // refresh the node to show the new count

			StatusMessage.Caption = count + " " + MetaTable.PrimaryKeyColumnLabel +
			 "s have passed the combine and have been saved in *Current";

			SessionManager.CurrentResultKeys = CidListCommand.ReadCurrentListRemote().ToStringList();
			SessionManager.DisplayCurrentCount();
			return;
		}

/// <summary>
/// Update temp tree node with new count
/// </summary>
/// <param name="nodeName"></param>

		void UpdateNode(string nodeName)
		{
			TreeListNode tln;

			foreach (MetaTreeNode mtn in TempNodes)
			{
				if (Lex.Eq(mtn.Label, "*" + nodeName))
				{
					foreach (TempCidList tcl in SS.I.TempCidLists)
					{
						if (Lex.Eq(tcl.Name, nodeName))
						{
							mtn.Size = tcl.Count; // update the node
							mtn.Name = "CNLIST_" + tcl.Id;
							mtn.Target = mtn.Name;

							tln = ListTree1.FindNodeByTarget(mtn.Target);
							ListTree1.RefreshNode(tln);

							tln = ListTree2.FindNodeByTarget(mtn.Target);
							ListTree2.RefreshNode(tln);
						}
					}

				}
			}
		}

		MetaTreeNode GetListMetaTreeNode(TreeListNode tln)
		{
			MetaTreeNode mtn = null;

			if (tln != null) mtn = tln.GetValue(0) as MetaTreeNode;
			if (mtn != null && mtn.Type != MetaTreeNodeType.CnList) mtn = null;
			if (mtn == null) 
				MessageBoxMx.ShowError("A list must be selected for both List 1 and List 2");

			return mtn;
		}

		private void EditCurrent_Click(object sender, EventArgs e)
		{
			CidListCommand.ReadCurrentListRemote(); // get the current list from server
			CidListEditor.Edit("Current");
			return;
		}

		private void SaveCurrentList_Click(object sender, EventArgs e)
		{
			CidListCommand.ReadCurrentListRemote(); // get the current list from server
			MainMenuControl.SetupTempListMenu(SaveCurrentListContextMenu.Items, false, SaveTempListMenuItem_Click);
			SaveCurrentListContextMenu.Show(SaveCurrent, 0, SaveCurrent.Height);
			return;
		}

		private void SavedListMenuItem_Click(object sender, EventArgs e)
		{
			UserObject uo = CidListCommand.SaveTempList("Current");
			if (uo == null) return;
			RemoveAddedNodes(); // rebuild the full trees
			Setup();
		}

		private void SaveTempListMenuItem_Click(object sender, EventArgs e)
		{
			string listName = ((ToolStripMenuItem)sender).Text;
			CommandExec.ExecuteCommandAsynch("List SaveCurrentToTemp " + Lex.AddDoubleQuotes(listName));
			UpdateNode(listName);
		}

		private void SaveNewTempListMenuItem_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("List SaveCurrentToNewTemp " + Lex.AddDoubleQuotes(((ToolStripMenuItem)sender).Text));
			RemoveAddedNodes(); // rebuild the full trees
			Setup();
		}

		private void ListLogic_FormClosing(object sender, FormClosingEventArgs e)
		{
			RemoveAddedNodes();
			return;
		}

		void RemoveAddedNodes()
		{
			foreach (TempCidList tcl in SS.I.TempCidLists)
			{
				UserObject uo = new UserObject(UserObjectType.CnList, SS.I.UserName, ParentFolder, "*" + tcl.Name);
				uo.Id = tcl.Id;
				UserObjectTree.RemoveObjectFromTree(uo);
			}

		}

	}
}