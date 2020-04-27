using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class SelectFieldFromContents : DevExpress.XtraEditors.XtraForm
	{
		static SelectFieldFromContents Instance; // current instance
		static QueryTable SelectedQt; // last query table rendered

		MetaTable FocusedMetaTable;
		MetaColumn SelectedMc;
		bool SelectSummarizedDataByDefault = true;
		bool CheckmarkDefaultColumn = true;

/// <summary>
/// Constructor
/// </summary>

		public SelectFieldFromContents()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the field selector dialog
/// </summary>
/// <param name="prompt"></param>
/// <param name="caption"></param>
		/// <param name="mc"></param>
		/// <param name="checkmarkDefaultColumn">If mc == null checkmark a default column</param>
		/// <returns></returns>

		public static MetaColumn ShowDialog (
			string prompt,
			string caption,
			MetaColumn mc,
			bool selectSummarizedDataByDefault,
			bool checkmarkDefaultColumn)
		{
			Instance = new SelectFieldFromContents();
			Instance.Setup(prompt, caption, mc, selectSummarizedDataByDefault, checkmarkDefaultColumn);

			QbUtil.SetProcessTreeItemOperationMethod(TreeItemOperation);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);

			QbUtil.RestoreProcessTreeItemOperationMethod();

			if (dr == DialogResult.OK) return Instance.SelectedMc;
			else return null;
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
			//MetaTreeNode node = ContentsTreeWithSearch.ContentsTree.GetMetaTreeNodeAt(ContentsTreeWithSearch.ContentsTree.PointToClient(Cursor.Position));
			//if (node == null || !node.IsDataTableType) return;

			string mtName = nodeTarget;
			MetaTable mt = MetaTableCollection.Get(mtName);
			if (mt == null) return;

			if (mt.UseSummarizedData || !mt.SummarizedExists)
				RenderTable(mt, null); // use table as selected

			else // select table based on desired summarization level
			{
				if (mt.SummarizedExists && SelectSummarizedDataByDefault)
				{
					string mtName2 = mtName += MetaTable.SummarySuffix;
					MetaTable mt2 = MetaTableCollection.Get(mtName2);
					if (mt2 != null)
					{
						RenderTable(mt2, null); // use table as selected
						return;
					}
				}

// Prompt user for summarization-level choice

				FocusedMetaTable = mt;
				Point p = Form.MousePosition;
				p = ContentsTreeWithSearch.ContentsTreeCtl.PointToClient(p);
				SummarizationLevelContextMenu.Show(ContentsTreeWithSearch.ContentsTreeCtl, p);
			}
		}

/// <summary>
/// Setup
/// </summary>
/// <param name="prompt"></param>
/// <param name="caption"></param>
/// <param name="mc"></param>
/// <param name="checkmarkDefaultColumn"></param>

		void Setup (
			string prompt,
			string caption,
			MetaColumn mc,
			bool selectSummarizedDataByDefault,
			bool checkmarkDefaultColumn)
		{
			MetaTable mt, mt2;
			QueryTable qt = null;
			QueryColumn qc = null;
			string selectedNodeTarget = "", topNodeTarget = "";
			int i1;

			SelectSummarizedDataByDefault = selectSummarizedDataByDefault;
			CheckmarkDefaultColumn = checkmarkDefaultColumn;

			if (mc != null) mt = mc.MetaTable; // specify starting table/field
			else if (SelectedQt != null) mt = SelectedQt.MetaTable; // use any last table
			else mt = null;

			if (mt != null)
			{
				selectedNodeTarget = mt.Name;

				MetaTreeNode mtn = MetaTree.GetNodeByTarget(selectedNodeTarget);
				if (mtn == null && Lex.EndsWith(selectedNodeTarget, MetaTable.SummarySuffix))
				{
					selectedNodeTarget = Lex.Replace(selectedNodeTarget, MetaTable.SummarySuffix, "");
					mtn = MetaTree.GetNodeByTarget(selectedNodeTarget);
				}

				if (mtn != null) topNodeTarget = selectedNodeTarget;
			}

			if (String.IsNullOrEmpty(topNodeTarget)) 
				topNodeTarget = SS.I.PreferredProjectId;

			ContentsTreeWithSearch.ContentsTreeCtl.FillTree(
				"root",
				MetaTreeNodeType.DataTableTypes,
				selectedNodeTarget,
				topNodeTarget,
				null,
				true,
				false);

			RenderTable(mt, mc);

			if (String.IsNullOrEmpty(caption)) 
				caption = "Select Data Field";
			Text = caption;

			if (String.IsNullOrEmpty(prompt))
				prompt = "Select a data table/assay from Database Contents and a Data Field";
			Prompt.Text = prompt;

		}

/// <summary>
/// Check that a column is selected
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void OK_Click(object sender, EventArgs e)
		{
			SelectedMc = null;
			if (SelectedQt != null)
			{
				foreach (QueryColumn qc in SelectedQt.QueryColumns)
				{
					if (qc.Selected)
					{
						SelectedMc = qc.MetaColumn;
						break;
					}
				}
			}

			if (SelectedMc == null)
			{
				MessageBoxMx.Show("You must select a data table and data field");
				return;
			}

			DialogResult = DialogResult.OK;
			return;
		}

		private void UnsummarizedMenuItem_Click(object sender, EventArgs e)
		{
			RenderTable(FocusedMetaTable, null);
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

			RenderTable(mt, null);
		}

		/// <summary>
		/// Render the specified table with any specified column marked
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="mc"></param>

		void RenderTable(
			MetaTable mt, 
			MetaColumn mc)
		{
			QueryTable qt;

			if (mt == null) // no query table
			{
				mt = new MetaTable();
				qt = new QueryTable(mt);
			}

			else // mark selected field in query table to render
			{
				qt = new QueryTable(mt);
				qt.DeselectAll();
				bool selectedField = false;

				if (mc != null)
				{
					for (int i1 = 0; i1 < qt.QueryColumns.Count; i1++)
					{
						QueryColumn qc = qt.QueryColumns[i1];
						if (qc.MetaColumn == mc)  // is this the column to select 
						{
							qc.Selected = true;
							selectedField = true;
						}
						else qc.Selected = false;
					}
				}

				else if (CheckmarkDefaultColumn && qt.QueryColumns.Count > 1) // default to 1st metacolumn past key
					qt.QueryColumns[1].Selected = true; 
			}

			FieldGrid.Render(qt);
			SelectedQt = qt; // keep qt where selected column will be found
			return;
		}

	}
}