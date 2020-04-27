using Mobius.ComOps;
using Mobius.Data;

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
	public partial class SelectFromContents : DevExpress.XtraEditors.XtraForm
	{
		static SelectFromContents Instance;

		ContentsTreeControl ContentsTree { get { return ContentsTreeWithSearch.ContentsTreeCtl; } }

		MetaTreeNodeType TypeFilter;
		MetaTable FocusedMetaTable;

		public SelectFromContents()
		{
			InitializeComponent();
			//ContentsTree.MouseDown += ContentsTree_MouseDown;
			//ContentsTree.FocusedNodeChanged += ContentsTree_FocusedNodeChanged;
		}

		/// <summary>
		/// Select a single item from the filtered types from the contents tree
		/// </summary>
		/// <param name="title"></param>
		/// <param name="prompt"></param>
		/// <param name="typeFilter"></param>
		/// <param name="initialSelection"></param>
		/// <param name="numberItems"></param>
		/// <returns></returns>

		public static MetaTreeNode SelectSingleItem(
			string title,
			string prompt,
			MetaTreeNodeType typeFilter,
			MetaTreeNode initialSelection,
			bool numberItems)
		{
			if (Instance == null) Instance = new SelectFromContents();
			Instance.Text = title;
			Instance.Prompt.Text = prompt;

			string selectedNodeTarget = (initialSelection != null ? initialSelection.Target : null);
			string expandNodeTarget = ContentsTreeControl.GetPreferredProjectNodeTarget();

			Instance.ContentsTree.FillTree(
				"root", 
				typeFilter,
				selectedNodeTarget,
				expandNodeTarget, 
				null,
				false,
				false);

			Instance.ContentsTreeWithSearch.CommandLineControl.Text = ""; 

			Instance.SelectionName.Text = (initialSelection != null ? initialSelection.Label : null);
			Instance.SelectionTarget.Text = (initialSelection != null ? initialSelection.Target : null); 
			Instance.TypeFilter = typeFilter;

			QbUtil.SetProcessTreeItemOperationMethod(TreeItemOperation);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			QbUtil.RestoreProcessTreeItemOperationMethod();

			if (dr == DialogResult.Cancel) return null;

			MetaTreeNode mtn = Instance.ContentsTree.FocusedMetaTreeNode;
			return mtn;
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
				Instance.ContentsTreeItemSelected(args);

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
			if (node == null || (node.Type & TypeFilter) == 0)
			{
				SelectionName.Text = SelectionTarget.Text = "";
				return;
			}

			else if (node.IsDataTableType)
			{
				MetaTable mt = MetaTableCollection.Get(node.Target);
				if (mt == null) return; // shouldn't happen
				if (mt.SummarizedExists) // Prompt user for summarization-level choice
				{
					FocusedMetaTable = mt;
					Point p = Form.MousePosition;
					p = ContentsTreeWithSearch.ContentsTreeCtl.PointToClient(p);
					SummarizationLevelContextMenu.Show(ContentsTreeWithSearch.ContentsTreeCtl, p);
					return;
				}
			}

			SelectionName.Text = node.Label;
			SelectionTarget.Text = node.Target;

// If item is double clicked then we're done

			MouseEventArgs ma = ContentsTreeWithSearch.CurrentContentsTreeMouseDownEvent;
			if (ma != null && ma.Clicks >= 2)
				DialogResult = DialogResult.OK;

			return;
		}

		private void UnsummarizedMenuItem_Click(object sender, EventArgs e)
		{
			SelectionName.Text = FocusedMetaTable.Label;
			SelectionTarget.Text = FocusedMetaTable.Name;
		}

		private void SummarizedViewMenuItem_Click(object sender, EventArgs e)
		{
			string mtName = FocusedMetaTable.Name + MetaTable.SummarySuffix; // indicate summary table
			MetaTable mt = MetaTableCollection.Get(mtName);
			if (mt == null)
			{
				MessageBoxMx.ShowError("MetaTable not found: " + mtName);
				return;
			}

			SelectionName.Text = mt.Label;
			SelectionTarget.Text = mt.Name;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (Lex.IsUndefined(SelectionName.Text))
			{
				MessageBoxMx.ShowError("You must select an item of the required type");
				return;
			}

			//MetaTreeNode node = ContentsTree.FocusedMetaTreeNode;
			//if ((node.Type & TypeFilter) == 0)
			//{
			//	MessageBoxMx.ShowError("You must select an item of the required type");
			//	return;
			//}

			else DialogResult = DialogResult.OK;
		}

		private void SelectFromContents_Shown(object sender, EventArgs e)
		{
			ContentsTreeWithSearch.CommandLineControl.Focus(); // put focus on command line
		}
	}
}