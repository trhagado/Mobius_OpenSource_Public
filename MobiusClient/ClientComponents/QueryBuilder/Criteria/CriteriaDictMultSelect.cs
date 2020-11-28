using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaDictMultSelect : DevExpress.XtraEditors.XtraForm
	{
		bool Insetup = false;

		static CriteriaDictMultSelect Instance = null;

		public CriteriaDictMultSelect()
		{
			InitializeComponent();
		}

/// <summary>
/// Invoke the editor
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public static bool Edit(
			QueryColumn qc)
		{
			if (Lex.StartsWith(qc.MetaColumn.Dictionary, "ContentsTree"))
				return CriteriaContentsTreeList.Edit(qc);

			if (Instance == null) Instance = new CriteriaDictMultSelect();

			new SyncfusionConverter().ToRazor(Instance);

			string title = "Search criteria for " + qc.ActiveLabel;
			string prompt = "Select one or more " + qc.ActiveLabel + " from the list below.";

			string dictName = GetCriteriaSpecificDictionary(qc);

			string selections = CheckedListBoxDialog(title, prompt, dictName, qc.Criteria);

			if (selections == null) return false;

			else
			{
				if (selections == "") qc.Criteria = qc.CriteriaDisplay = "";
				else
				{
					qc.CriteriaDisplay = selections;

					if (!qc.MetaColumn.IsNumeric)
					{ // quote items if necessary
						List<string> items = Csv.SplitCsvString(selections);
						for (int i1 = 0; i1 < items.Count; i1++)
							items[i1] = Lex.AddSingleQuotes(items[i1]);
						selections = Csv.JoinCsvString(items);
					}

					qc.Criteria = qc.MetaColumn.Name + " in (" + selections + ")";
				}
				return true;
			}
		}

/// <summary>
/// Get the appropriate dictionary for possible criteria values based on the details of the current criteria
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public static string GetCriteriaSpecificDictionary(
			QueryColumn qc)
		{
			string dictName = qc.MetaColumn.Dictionary;
			QueryTable qt = qc.QueryTable;

			if (!qc.MetaColumn.IsDatabaseSetColumn) return dictName;
			if (qt.FirstStructureQueryColumn == null) return dictName; // structure col in table?

			if (Lex.Eq(dictName, "Structure_Databases"))
				dictName = "Structure_Databases"; 

			ParsedStructureCriteria pssc = qt.GetParsedStructureSearchCriteria();
			if (pssc == null) return dictName;

			if (pssc.IsChemistrySearch) return "Structure_Databases";
			else if (pssc.SearchType == StructureSearchType.SmallWorld) return "SmallWorldDatabases";
			else return dictName;
		}

		/// <summary>
		/// Dialog to allow user to select one or more items from a dictionary
		/// </summary>
		/// <param name="title"></param>
		/// <param name="prompt"></param>
		/// <param name="dictionaryName"></param>
		/// <param name="selections"></param>
		/// <returns></returns>

		public static string CheckedListBoxDialog(
			string title,
			string prompt,
			string dictionaryName,
			string selections) // change to criteria
		{
			if (Instance == null) Instance = new CriteriaDictMultSelect();

			Instance.Text = title;
			Instance.Prompt.Text = prompt;
			Instance.SetupCheckedListBoxFromDictionary(dictionaryName, selections);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			string newSelections = Instance.GetCheckedListItems();
			return newSelections;
		}

		internal void SetupCheckedListBoxFromDictionary(
			string dictName,
			string criteria)
		{
			List<string> vList = new List<string>();
			List<string> excludeList = new List<string>();

			List<string> dictWords;
			bool itemChecked;

			SearchText.Text = "";

			dictWords = DictionaryMx.GetWords(dictName, false);
			if (dictWords == null) throw new Exception("Dictionary not found: " + dictName);

			Insetup = true;

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(criteria);
			if (psc != null)
			{
				if (psc.ValueList != null) vList = psc.ValueList;
				else if (!Lex.IsNullOrEmpty(psc.Value)) vList.Add(psc.Value);
			}
			HashSet<string> hashSet = new HashSet<string>(vList);

			CheckList.Items.Clear();
			int si = -1;
			foreach (string key in dictWords)
			{
				if (Lex.Eq(dictName, "STRUCTURE_DATABASES") && Lex.Eq(key, "UserDatabase"))
					continue; // special-case fix - generalize later

				CheckedListBoxItem clbi = new CheckedListBoxItem();
				clbi.Description = key;
				if (hashSet.Contains(key))
				{
					clbi.CheckState = CheckState.Checked;
					if (si < 0) si = CheckList.Items.Count;
				}
				else clbi.CheckState = CheckState.Unchecked;
				CheckList.Items.Add(clbi);
			}

			if (si >= 0)
			{
				CheckList.SelectedIndex = si;
				CheckedListBoxItem clbi = CheckList.Items[si];
				SearchText.Text = clbi.Description;
			}

			SelectedItemText.Text =
				SelectedItemText.ToolTip = GetCheckedListItems();

			Insetup = false;
			return;
		}

/// <summary>
/// GetCheckedListItems
/// </summary>
/// <returns></returns>

		internal string GetCheckedListItems()
		{
			StringBuilder sb = new StringBuilder();
			//CheckedListBoxControl.CheckedItemCollection cList = CheckList.CheckedItems; // gets in order checked
			//foreach (CheckedListBoxItem clbi in cList)

			foreach (CheckedListBoxItem clbi in CheckList.Items)
			{
				if (clbi.CheckState != CheckState.Checked) continue;

				if (sb.Length > 0) sb.Append(", ");
				sb.Append(clbi.Description);
			}

			return sb.ToString();
		}

		private void SelectAll_Click(object sender, EventArgs e)
		{
			Insetup = true;

			CheckList.CheckAll();

			SelectedItemText.Text =
				SelectedItemText.ToolTip = GetCheckedListItems();

			Insetup = false;
			return;
		}

		private void DeselectAll_Click(object sender, EventArgs e)
		{
			Insetup = true;

			CheckList.UnCheckAll();

			SelectedItemText.Text =
				SelectedItemText.ToolTip = GetCheckedListItems();

			Insetup = false;
			return;
		}

/// <summary>
/// Put focus on search box upon enter
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CriteriaDictMultSelect_Activated(object sender, EventArgs e)
		{
			try { SearchText.Focus(); }
			catch { }
		}

/// <summary>
/// Text box value changed, update the cursor position
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SearchText_EditValueChanged(object sender, EventArgs e)
		{
			int i1;

			if (Insetup) return;

			string txt = SearchText.Text;
			txt = txt.Trim();
			if (txt == "") return;

			Insetup = true;

			int firstCasedMatch = -1;
			int firstUncasedMatch = -1;

			for (i1 = 0; i1 < CheckList.Items.Count; i1++)
			{
				CheckedListBoxItem clbi = CheckList.Items[i1];
				string iTxt = clbi.ToString();

				if (Lex.StartsWith(iTxt, txt))
				{
					if (firstCasedMatch < 0 && iTxt.StartsWith(txt))
						firstCasedMatch = i1;

					if (firstUncasedMatch < 0)
						firstUncasedMatch = i1;
				}

				if (firstCasedMatch >= 0 && firstUncasedMatch >= 0) break;
			}

			if (firstCasedMatch >= 0) i1 = firstCasedMatch;
			else if (firstUncasedMatch >= 0) i1 = firstUncasedMatch;
			else i1 = -1;

			if (CheckList.SelectedIndex != i1 && i1 >= 0)
				CheckList.SelectedIndex = i1;

			Insetup = false;

			return;
		}

/// <summary>
/// Key pressed
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SearchText_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (int)Keys.Return) return;

			int i1 = CheckList.SelectedIndex;
			if (i1 < 0) return;

			CheckedListBoxItem clbi = CheckList.Items[i1]; // switch check state

			if (clbi.CheckState == CheckState.Unchecked)
				clbi.CheckState = CheckState.Checked;

			else clbi.CheckState = CheckState.Unchecked;

			return;
		}

/// <summary>
/// CheckList_SelectedIndexChanged
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CheckList_SelectedIndexChanged(object sender, EventArgs e)
		{
			return;
		}

/// <summary>
/// CheckList_ItemCheck
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CheckList_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
		{
			if (Insetup) return;

			int i1 = e.Index;
			if (i1 < 0) return;

			Insetup = true;
			CheckedListBoxItem clbi = CheckList.Items[i1];
			if (clbi.CheckState == CheckState.Checked)
				SearchText.Text = clbi.Description;
			else SearchText.Text = "";

			SelectedItemText.Text = 
				SelectedItemText.ToolTip = GetCheckedListItems();

			Insetup = false;
			return;
		}

	}
}