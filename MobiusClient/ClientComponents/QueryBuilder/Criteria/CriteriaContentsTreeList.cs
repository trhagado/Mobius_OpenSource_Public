using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaContentsTreeList : XtraForm
	{
		static CriteriaContentsTreeList Instance; // current instance

		QueryColumn Qc;
		DataTable ItemGridDataTable; // DataTable for parameters
		bool Updated = false;
		bool IsClosed = false;
		bool InSetup = false;

		public CriteriaContentsTreeList()
		{
			InitializeComponent();
		}

/// <summary>
/// Edit list of items from contents tree
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public static bool Edit(
			QueryColumn qc)
		{
			Instance = new CriteriaContentsTreeList();
			CriteriaContentsTreeList i = Instance;

			i.Qc = qc;
			i.SetupForm();

			QbUtil.SetProcessTreeItemOperationMethod(TreeItemOperation);

			DialogResult dr = i.ShowDialog(SessionManager.ActiveForm);

			QbUtil.RestoreProcessTreeItemOperationMethod();

			if (dr == DialogResult.OK) return true;
			else return false;
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
				Instance.AddMetaTableItemToDataTable(args);

			else Instance.ContentsTreeWithSearch.ContentsTreeCtl.FindInContents(args); 

			return;
		}

/// <summary>
/// SetupForm
/// </summary>
		
		void SetupForm()
		{
			string selectedNodeTarget = "", topNodeTarget = "";

			CriteriaContentsTreeList i = this;

			string title = "Search criteria for " + Qc.ActiveLabel;
			i.Text = title;

			string prompt = "Use the Database Contents tree and QuickSearch line to select items to be added to this list.";
			i.Prompt.Text = Lex.Replace(prompt, "items", Qc.ActiveLabel);

			i.CreateItemDataTable();
			i.FillItemDataTable();
			i.ItemGrid.DataSource = i.ItemGridDataTable;

			ContentsTreeWithSearch.ResetTreeAndCommandLine();

			//topNodeTarget = SS.I.PreferredProjectId;

			//MetaTreeNodeType contentsFilter = MetaTreeNodeType.MetaTable | // database content types
			//  MetaTreeNodeType.CalcField | MetaTreeNodeType.Annotation;

			//i.ContentsTreeWithSearch.ContentsTree.Fill(
			//  "root",
			//  contentsFilter,
			//  selectedNodeTarget,
			//  topNodeTarget,
			//  null,
			//  true,
			//  false);

			return;
		}

		/// <summary>
		/// Create the item DataTable
		/// </summary>

		void CreateItemDataTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("LabelColumn", typeof(string)));
			dt.Columns.Add(new DataColumn("InternalNameColumn", typeof(string)));

			dt.RowChanged += new DataRowChangeEventHandler(CreateColumns_RowChanged);

			ItemGridDataTable = dt;
		}

		/// <summary>
		/// Fill the grid of col defs 
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		void FillItemDataTable()
		{
			MetaTreeNode mtn;
			string txt = "", tok, name, label;
			
			ItemGridDataTable.Rows.Clear();

			List<string> vList = new List<string>();
			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(Qc.Criteria);
			if (psc != null)
			{
				if (psc.ValueList != null) vList = psc.ValueList;
				else if (!Lex.IsNullOrEmpty(psc.Value)) vList.Add(psc.Value);
			}

			MetaTable mt = Qc.MetaColumn.MetaTable;
			MetaColumn mc = Qc.MetaColumn;
			string mcDict = mc.Dictionary;
			bool searchId = Lex.EndsWith(mcDict, ".Id");
			bool searchLabel = Lex.EndsWith(mcDict, ".Label");

			for (int si = 0; si < vList.Count; si++)
			{
				string s = vList[si];
				if (Lex.IsUndefined(s)) continue;

				name = label = "";
				if (searchId) // assume numeric ASSAY code for metatable for now
				{
					name = "ASSAY_" + s;
					mtn = MetaTree.GetNode(name); // try to look up by internal name
				}

				else
				{
					label = s;
					mtn = MetaTree.GetNodeByLabel(label);
				}

				if (mtn != null)
				{
					name = mtn.Target;
					label = mtn.Label;
				}

				DataRow dr = ItemGridDataTable.NewRow();
				dr["LabelColumn"] = label;
				dr["InternalNameColumn"] = name;

				ItemGridDataTable.Rows.Add(dr);
			}

			return;
		}

/// <summary>
/// Add a MetaTableItem to the DataTable
/// </summary>
/// <param name="mtName"></param>

		void AddMetaTableItemToDataTable(
			string mtName)
		{

			MetaTreeNode mtn = MetaTreeNodeCollection.GetNode(mtName);
			if (mtn != null && mtn.IsFolderType) return; // ignore folders

			MetaTable mt = MetaTableCollection.Get(mtName);
			if (mt == null)
			{
				MessageBoxMx.ShowError("The selected item is not a recognized data table: " + mtName);
				return;
			}

			string allowedTypes = Qc.MetaColumn.TableMap;
			if (Lex.IsDefined(allowedTypes)) // see if supplied table is allowed
			{
				int ai;
				string[] sa = allowedTypes.Split(',');
				for (ai = 0; ai < sa.Length; ai++)
				{
					string allowedType = sa[ai];
					if (Lex.IsUndefined(allowedType)) continue;
					if (Lex.Contains(mt.Name, allowedType.Trim())) break;
				}

				if (ai >= sa.Length)
				{
					MessageBoxMx.ShowError("The selected data table is not is not of a type that can be added here (" + allowedTypes + ")");
					return;
				}
			}
			
			MetaTableItem i = new MetaTableItem();
			i.ExternalName = mt.Label;
			i.InternalName = mt.Name;
			AddMetaTableItemToDataTable(i);

			return;
		}

/// <summary>
/// Add a MetaTableItem to the DataTable
/// </summary>
/// <param name="mti"></param>

		void AddMetaTableItemToDataTable(
			MetaTableItem mti)
		{
			DataRow dr;
			int ri;
			DataRowCollection rows = ItemGridDataTable.Rows;
			for (ri = 0; ri < rows.Count; ri++)
			{
				dr = rows[ri];
				string label = dr["LabelColumn"] as string;
				string name = dr["InternalNameColumn"] as string;
				MetaTreeNode mtn = null;

				if (Lex.IsDefined(name) && Lex.IsDefined(mti.InternalName) && Lex.Eq(name, mti.InternalName)) break; // check internal name first for match

				if (Lex.IsDefined(label) && Lex.IsDefined(mti.ExternalName) && Lex.Eq(label, mti.ExternalName)) break; // check table label
			}

			if (ri < rows.Count)
			{
				return; // could focus on existing row
			}

			else
			{
				dr = ItemGridDataTable.NewRow();
				dr["LabelColumn"] = mti.ExternalName;
				dr["InternalNameColumn"] = mti.InternalName;

				ItemGridDataTable.Rows.Add(dr);
			}

			return;
		}

		/// <summary>
		/// If row added set DisplayByDefault to true if null
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void CreateColumns_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			if (InSetup) return;

			return;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (!UpdateCriteria()) return;

			Updated = true;
			DialogResult = DialogResult.OK;
		}

/// <summary>
/// Get values from form and update criteria in associated QueryColumn
/// </summary>
/// <returns></returns>

		bool UpdateCriteria()
		{
			MetaTable mt;
			string label, name, listString;
			bool summary;
			int tableId;

			DataRowCollection rows = ItemGridDataTable.Rows;
			string s = "";
			List<string> sl2 = new List<string>(new string[] { "", "" });
			foreach (DataRow dr in rows)
			{
				label = sl2[0] = dr["LabelColumn"] as string;
				name = sl2[1] = dr["InternalNameColumn"] as string;

				if (Lex.IsUndefined(label) && Lex.IsUndefined(name)) continue;

				string txt = Csv.JoinCsvString(sl2);
				s += txt + "\r\n";
			}

			List<MetaTableItem> list = MetaTableItem.ParseList(s, true, true);
			string valueList = ""; // list of values to match in criteria
			string displayList = "";

			string mcDict = Qc.MetaColumn.Dictionary;
			bool searchId = Lex.EndsWith(mcDict, ".Id");
			bool searchLabel = Lex.EndsWith(mcDict, ".Label");

			foreach (MetaTableItem mti in list)
			{
				mt = MetaTableCollection.Get(mti.InternalName);
				if (mt == null) continue;

				MetaTable.ParseMetaTableName(mt.Name, out tableId, out summary);

				label = MetaTable.RemoveSuffixesFromName(mt.Label);

				//mtn = MetaTree.GetNode(mti.InternalName); // need to check node name vs target?

				// Append to value list

				if (valueList.Length > 0) valueList += ", ";

				if (searchId) // extract numeric id for table if matching against numeric column
					valueList += tableId.ToString();

				else
					valueList += Lex.AddSingleQuotes(label);

				// Append to label list

				if (displayList.Length == 0) // include first item label only in CriteriaDisplay
					displayList = label + " - ASSAY: ";
				else displayList += ", ";

				displayList += tableId.ToString();
			}

			if (valueList == "") Qc.Criteria = Qc.CriteriaDisplay = "";
			else
			{
				Qc.Criteria = Qc.MetaColumn.Name + " in (" + valueList + ")";
				Qc.CriteriaDisplay = displayList;
			}

			return true;
		}

/// <summary>
/// Close form & end criteria editing
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Cancel_Click(object sender, EventArgs e)
		{
			Updated = false;
			DialogResult = DialogResult.Cancel;
		}

		private void CriteriaContentsTreeList_FormClosing(object sender, FormClosingEventArgs e)
		{
			return;
		}

		private void CriteriaContentsTreeList_FormClosed(object sender, FormClosedEventArgs e)
		{
			IsClosed = true;
			return;
		}

		private void DeleteRow_Click(object sender, EventArgs e)
		{
			DeleteRowMenuItem_Click(sender, e);
		}

// Adjust button positions

		private void CriteriaContentsTreeList_Resize(object sender, EventArgs e)
		{
			DeleteRow.Left = ContentsTreeWithSearch.Right + 10;
			EditButton.Left = DeleteRow.Right + 10;
			return;
		}

/// <summary>
/// Put focus on search line when activated
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CriteriaContentsTreeList_Activated(object sender, EventArgs e)
		{
			ContentsTreeWithSearch.Activate();
		}

		private void CriteriaContentsTreeList_Shown(object sender, EventArgs e)
		{
			return;
		}

		private void CriteriaContentsTreeList_VisibleChanged(object sender, EventArgs e)
		{
			return;
		}

		private void EditButton_Click(object sender, EventArgs e)
		{
			Point p = new System.Drawing.Point(0, EditButton.Height);
			EditMenu.Show(EditButton, p);

			return;
		}

		private void CopyAssayNamesMenuItem_Click(object sender, EventArgs e)
		{
			CopyAssayData(true, false);
		}

		private void CopyInternalNamesMenuItem_Click(object sender, EventArgs e)
		{
			CopyAssayData(false, true);
		}

		private void CopyAssayAndInternalNamesMenuItem_Click(object sender, EventArgs e)
		{
			CopyAssayData(true, true);
		}

/// <summary>
/// Copy assay data to clipboard
/// </summary>
/// <param name="includeLabels"></param>
/// <param name="includeInternalNames"></param>

		private void CopyAssayData(
			bool includeLabels,
			bool includeInternalNames)
		{
			string s = "";
			DataRowCollection rows = ItemGridDataTable.Rows;
			for (int ri = 0; ri < rows.Count; ri++)
			{
				DataRow dr = rows[ri];
				string label = dr["LabelColumn"] as string;
				string name = dr["InternalNameColumn"] as string;

				string txt = "";

				if (includeLabels && includeInternalNames)
				{
					if (label.Contains(","))
						label = Lex.AddDoubleQuotes(label);
					txt = label + "," + name;
				}

				else if (includeLabels) txt = label;

				else if (includeInternalNames) txt = name;

				s += txt + "\r\n";
			}

			Clipboard.SetText(s);
			return;
		}

/// <summary>
/// Paste assay data from clipboard
/// </summary>
/// <param name="s"></param>

		private void PasteMenuItem_Click(object sender, EventArgs e)
		{
			string s = Clipboard.GetText();
			if (Lex.IsUndefined(s)) return;

			List<MetaTableItem> list = MetaTableItem.ParseList(s, false, false);
			foreach (MetaTableItem i in list)
			{
				AddMetaTableItemToDataTable(i);
			}

			return;
		}

		private void DeleteRowMenuItem_Click(object sender, EventArgs e)
		{
			int r = ColGridView.FocusedRowHandle;
			if (r < 0) return;

			ItemGridDataTable.Rows.RemoveAt(r);
			return;
		}

		private void DeleteAllMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to delete all rows?", "Delete All Rows",
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

			if (dr == DialogResult.Yes)
				ItemGridDataTable.Clear();
		}

	}

/// <summary>
/// Class of External and Internal name pairs
/// </summary>

	internal class MetaTableItem
	{
		public string ExternalName = "";
		public string InternalName = "";

/// <summary>
/// Parse a string into a list of Items
/// </summary>
/// <param name="s"></param>
/// <returns></returns>

		public static List<MetaTableItem> ParseList(
			string s,
			bool mustExist,
			bool removeSuffixes)
		{
			List<MetaTableItem> list = new List<MetaTableItem>();

			s = s.Replace("\r", "");
			string[] sa = s.Split('\n');

			foreach (string txt in sa)
			{
				string eName = "", iName = "";
				MetaTreeNode mtn = null;

				if (Lex.IsUndefined(txt)) continue;

				List<string> rowList = Csv.SplitCsvString(txt);
				if (rowList.Count == 0) continue;

				eName = rowList[0]; 
				mtn = GetNode(eName);
				if (mtn == null && rowList.Count > 1)
				{
					iName = rowList[1];
					mtn = GetNode(iName);
				}

				if (mtn != null)
				{
					iName = mtn.Target;
					eName = mtn.Label;
				}

				else // no node found
				{
					if (mustExist) continue; 
				}

				if (removeSuffixes)
					eName = MetaTable.RemoveSuffixesFromName(eName);

				MetaTableItem i = new MetaTableItem();
				i.InternalName = iName;
				i.ExternalName = eName;

				list.Add(i);
			}

			return list;
		}

/// <summary>
/// Try to lookup a MetaTreeNode by the supplied token
/// </summary>
/// <param name="tok"></param>
/// <returns></returns>

		static MetaTreeNode GetNode(string tok)
		{
			MetaTreeNode mtn;
			if (Lex.IsInteger(tok))
				tok = "ASSAY_" + tok; // if no prefix assume ASSAY for now

			mtn = MetaTree.GetNode(tok);
			if (mtn != null) return mtn;

			mtn = MetaTree.GetNodeByLabel(tok);
			if (mtn != null) return mtn;

			return null;
		}
	}
}
