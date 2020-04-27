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
	public partial class ContentsTreeFindDialog : DevExpress.XtraEditors.XtraForm
	{
		bool InSetup = false;

		public ContentsTreeFindDialog()
		{
			InitializeComponent();
		}

/// <summary>
/// Setup form
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ContentsTreeFindDialog_Shown(object sender, EventArgs e)
		{
			QueryStringCtl.Focus();
			return;
		}

/// <summary>
/// Get values from form
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void OK_Click(object sender, EventArgs e)
		{
			if (GetValues().NodeTypesToSearch == 0)
			{
				MessageBoxMx.ShowError("You must select at least one item type to search for.");
				return;
			}

			if (Lex.IsUndefined(QueryStringCtl.Text) && DateEdit.DateTime == DateTime.MinValue)
			{
				MessageBoxMx.ShowError("You must enter some text to search for or specify a date limit");
				return;
			}

			DialogResult = DialogResult.OK;
		}

		public ContentsTreeFindParms GetValues()
		{
			ContentsTreeFindParms ctfp = new ContentsTreeFindParms();
			ctfp.QueryString = QueryStringCtl.Text;
			ctfp.LowerDateLimit = DateEdit.DateTime;
			ctfp.CheckMyUserObjectsOnly = MyUserObjects.Checked;
			ctfp.DisplayAsList = ListResultDisplayCheckEdit.Checked;
			ctfp.NodeTypesToSearch = GetNodeTypesToSearch();

			return ctfp;
		}

		public void SetValues(ContentsTreeFindParms ctfp)
		{
			InSetup = true;
			QueryStringCtl.Text = ctfp.QueryString;
			DateEdit.DateTime = ctfp.LowerDateLimit;
			SetNodeTypesToSearch(ctfp.NodeTypesToSearch);
			if (ctfp.CheckMyUserObjectsOnly) MyUserObjects.Checked = true;
			else AllUserObjects.Checked = true;

			if (ctfp.DisplayAsList) ListResultDisplayCheckEdit.Checked = true;
			else TreeResultDisplayCheckEdit.Checked = true;

			InSetup = false;

			return;
		}

		MetaTreeNodeType GetNodeTypesToSearch()
		{
			MetaTreeNodeType nodeTypes = 0;
			if (Folders.Checked) nodeTypes |= MetaTreeNodeType.FolderMask; // all folder types
			if (MetaTables.Checked) nodeTypes |= MetaTreeNodeType.MetaTable;
			if (AnnotationTables.Checked) nodeTypes |= MetaTreeNodeType.Annotation;
			if (CalcFields.Checked) nodeTypes |= MetaTreeNodeType.CalcField;
			if (Queries.Checked) nodeTypes |= MetaTreeNodeType.Query;
			if (CidLists.Checked) nodeTypes |= MetaTreeNodeType.CnList;
			if (HyperLinks.Checked) nodeTypes |= MetaTreeNodeType.Url;

			return nodeTypes;
		}

		void SetNodeTypesToSearch(MetaTreeNodeType nodeTypes)
		{
			Folders.Checked = ((nodeTypes & MetaTreeNodeType.FolderMask) != 0) ? true : false;
			MetaTables.Checked = ((nodeTypes & MetaTreeNodeType.MetaTable) != 0) ? true : false;
			AnnotationTables.Checked = ((nodeTypes & MetaTreeNodeType.Annotation) != 0) ? true : false;
			CalcFields.Checked = ((nodeTypes & MetaTreeNodeType.CalcField) != 0) ? true : false;
			Queries.Checked = ((nodeTypes & MetaTreeNodeType.Query) != 0) ? true : false;
			CidLists.Checked = ((nodeTypes & MetaTreeNodeType.CnList) != 0) ? true : false;
			HyperLinks.Checked = ((nodeTypes & MetaTreeNodeType.Url) != 0) ? true : false;
		}



		private void AllButton_Click(object sender, EventArgs e)
		{
			SetCheckedForAllNodeTypes(true);
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			SetCheckedForAllNodeTypes(false);		
		}

		void SetCheckedForAllNodeTypes(bool check)
		{
			Folders.Checked = check;
			MetaTables.Checked = check;
			AnnotationTables.Checked = check;
			CalcFields.Checked = check;
			Queries.Checked = check;
			CidLists.Checked = check;
			HyperLinks.Checked = check;
		}

		private void TreeResultDisplayCheckEdit_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!TreeResultDisplayCheckEdit.Checked) return;

			Folders.Checked = false; // turn off folders by default for search with tree display to avoid over filtering
		}
	} // ContentsTreeFindDialog

	/// <summary>
	/// Parameters for ContentsTreeFindDialog calls
	/// </summary>

	public class ContentsTreeFindParms
	{
		public string QueryString = "";
		public DateTime LowerDateLimit = DateTime.MinValue;
		public bool CheckMyUserObjectsOnly = false;
		public bool DisplayAsList = true;
		public bool DisplayAsTree { get { return !DisplayAsList; } set { DisplayAsList = !value; } }
		public MetaTreeNodeType NodeTypesToSearch = MetaTreeNodeType.All; // set of bits for types to search

		public string GetDisplayString()
		{
			string qs = Lex.IsDefined(QueryString) ? "words (" + QueryString + ")": "";
			if (LowerDateLimit != DateTime.MinValue)
			{
				if (Lex.IsDefined(qs)) qs += " and ";
				qs += "date >= " + LowerDateLimit.ToShortDateString();
			}

			qs = ">>>  Find in Contents results for " + qs;
			return qs;
		}
	}

}