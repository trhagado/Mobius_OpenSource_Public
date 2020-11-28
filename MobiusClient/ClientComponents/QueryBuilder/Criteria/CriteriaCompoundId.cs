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
using System.IO;

namespace Mobius.ClientComponents
{
	public partial class CriteriaCompoundId : DevExpress.XtraEditors.XtraForm
	{
		static CriteriaCompoundId Instance;

		QueryColumn Qc; // column being edited
		UserObject SavedListUo = null; // UserObject associated with any selected saved list
		string CidListString = ""; // list of cids if criteria is an embedded list

		public CriteriaCompoundId()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Invoke the editor
		/// </summary>
		/// <param name="qc">QueryColumn to edit</param>
		/// <returns></returns>

		public static bool Edit(
			QueryColumn qc)
		{
			AssertMx.IsNotNull(qc, "qc");

			if (Instance == null) Instance = new CriteriaCompoundId();

			new SyncfusionConverter().ToRazor(Instance);

			if (qc.IsKey) // if key col be sure qc is in sync with Query.KeyCriteria 
				qc.CopyCriteriaFromQueryKeyCriteria();

			if (qc.Criteria.Trim().StartsWith("=") && qc.MetaColumn != null)
				qc.Criteria = qc.MetaColumnName + " " + qc.Criteria;

			Instance.Qc = qc;
			Instance.Setup(qc);

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				if (qc.IsKey) // if key col then update Query.KeyCriteria
					qc.CopyCriteriaToQueryKeyCritera();
				return true;
			}
			else return false;
		}

		/// <summary>
/// Setup the form with existing criteria
/// </summary>
/// <param name="qc"></param>

		void Setup(
			QueryColumn qc)
		{
			MetaTable mt = null;
			string listName, mcName, tok, tok1, tok2, tok3, txt;
			int i1;

			if (qc != null && qc.QueryTable != null) 
				mt = qc.QueryTable.MetaTable;

			Text = qc.ActiveLabel + " Criteria";
			Prompt.Text =
				"Choose one of the following " + qc.ActiveLabel + " criteria options.";

			Cid.Text = "";
			CidListDisplay.Text = "";
			ListName.Text = "";
			SavedListUo = null;
			CidLo.Text = "";
			CidHi.Text = "";

			TempListName.Properties.Items.Clear(); // build dropdown of available temp list names
			foreach (TempCidList tl0 in SS.I.TempCidLists)
				TempListName.Properties.Items.Add(tl0.Name);
			TempListName.Text = "Current";

			Lex lex = new Lex();
			lex.OpenString(qc.Criteria);
			mcName = lex.GetUpper(); // metacolumn name
			if (Lex.Ne(mcName, qc.MetaColumnName)) // check if col name is first token
			{ // if not assume it's simply missing and that the operator is the first token
				lex.Backup();
				mcName = qc.MetaColumnName;
			}

			tok = lex.GetUpper(); // operator
			tok1 = lex.GetUpper(); // following tokens */
			tok2 = lex.GetUpper();
			tok3 = lex.GetUpper();

			if (tok == "=" || tok == "")
			{
				tok1 = CompoundId.Format(tok1, mt);
				Cid.Text = tok1;
				EQ.Checked = true;
			}

			else if (tok == "IN" && tok1.StartsWith("(")) // list saved with query only
			{
				InList.Checked = true;

				CidListString = qc.Criteria;
				i1 = CidListString.IndexOf("("); // just get the list itself if parenthesized
				if (i1 >= 0) CidListString = CidListString.Substring(i1 + 1);
				if (CidListString.EndsWith(")")) CidListString = CidListString.Substring(0, CidListString.Length - 1);

				SetCidListDisplay();
			}

			else if (tok == "IN" && (Lex.Eq(Lex.RemoveAllQuotes(tok2), "CURRENT") ||
				Lex.StartsWith(tok2, UserObject.TempFolderNameQualified)))
			{ // temp list
				if (Lex.StartsWith(tok2, UserObject.TempFolderNameQualified))
					tok2 = tok2.Substring(UserObject.TempFolderNameQualified.Length);
				TempListName.Text = tok2;
				TempList.Checked = true;
			}

			else if (tok == "IN") // saved list
			{
				SavedListUo = QueryEngine.ResolveCnListReference(tok2);
				if (SavedListUo != null)
					listName = SavedListUo.Name;

				else listName = "Nonexistant list"; // list deleted

				ListName.Text = listName;
				SavedList.Checked = true;
			}

			else if (tok == "BETWEEN")
			{
				tok1 = CompoundId.Format(tok1);
				CidLo.Text = tok1;
				tok3 = CompoundId.Format(tok3);
				CidHi.Text = tok3;
				Between.Checked = true;
			}
		}

/// <summary>
/// Set the display form of the list
/// </summary>

		void SetCidListDisplay()
		{
			string txt = CidList.FormatAbbreviatedCidListForDisplay(Qc, CidListString);
			CidListDisplay.Text = txt;
		}

/// <summary>
/// Allow editing of list in control if not a summarized version of the list
/// </summary>
/// <returns></returns>

		bool CidListDisplayIsEditable()
		{
			return CidListDisplay.Enabled && !Lex.Contains(CidListDisplay.Text, "..."); 
		}

/// <summary>
/// Set initial focus
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CriteriaCompoundId_Activated(object sender, EventArgs e)
		{
			if (EQ.Checked)
			{
				Cid.Focus();
				if (Cid.Text.Length == 1)
				{ // if coming in with 1st typed character the position cursor after this
					Cid.SelectionStart = 1;
					Cid.SelectionLength = 0;
				}
			}

			if (InList.Checked)
			{
				if (CidListDisplayIsEditable()) CidListDisplay.Focus();
				else InList.Focus();
			}

			if (TempList.Checked) TempList.Focus();
			if (SavedList.Checked) ListName.Focus();
			if (Between.Checked) CidLo.Focus();
			if (None.Checked) None.Focus();
		}

		private void EQ_CheckedChanged(object sender, EventArgs e)
		{
			DisableCriteriaChecks();
			Cid.Enabled = true;
			BrowseRN.Enabled = true;
			if (Visible) Cid.Focus();
		}

		private void BrowseRN_Click(object sender, EventArgs e)
		{
			EQ.Checked = true;
			Cid.Enabled = true;

			string prompt = "Enter the " + Qc.ActiveLabel + " to be retrieved";
			string title = "Limit " + Qc.ActiveLabel;
			string tok = InputCompoundId.Show(prompt, title, Cid.Text, Qc.MetaColumn.MetaTable);
			if (tok != null) Cid.Text = tok;
			return;
		}

		private void InList_CheckedChanged(object sender, EventArgs e)
		{
			DisableCriteriaChecks();
			CidListDisplay.Enabled = true;
			ImportList.Enabled = true;
			EditList.Enabled = true;
			if (Visible && CidListDisplayIsEditable()) CidListDisplay.Focus();
		}

		private void ImportList_Click(object sender, EventArgs e)
		{
			string filePath = UIMisc.SelectListFileDialog("List File to Import", "");
			if (String.IsNullOrEmpty(filePath)) return;

			string fileName = Path.GetFileNameWithoutExtension(filePath);
			CidList cnList = CidList.ReadFromFile(filePath); // read file list
			if (cnList == null) return;

			CidListString = CidList.BuildListCsvStringOfFormattedCids(Qc, cnList.ToStringList());
			SetCidListDisplay();
			if (CidListDisplay.Text.Length > 0) // no text selected
				CidListDisplay.Select(0, 0);
			return;
		}

		private void EditList_Click(object sender, EventArgs e)
		{
			string listText = CidListString.Trim();
			List<string> al = Csv.SplitCsvString(listText);
			CidList cnList = new CidList(al, true);
			cnList.UserObject.Name = "Criteria List";
			cnList.UserObject.Id = CidListEditor.EditInMemoryOnlyUoId; // indicate not to be persisted to user object
			cnList = CidListEditor.Edit(cnList, Qc.MetaColumn.MetaTable.Root);
			if (cnList == null) return;

			CidListString = CidList.BuildListCsvStringOfFormattedCids(Qc, cnList.ToStringList());
			SetCidListDisplay();
			if (CidListDisplayIsEditable() && CidListString.Length > 0) // no text selected
				CidListDisplay.Select(0, 0);
			else InList.Focus();
		}

		private void TempList_CheckedChanged(object sender, EventArgs e)
		{
			DisableCriteriaChecks();
			TempListName.Enabled = true;
			EditTempList.Enabled = true;
			if (Visible) TempListName.Focus();
		}

		private void EditTempList_Click(object sender, EventArgs e)
		{
			if (Lex.IsNullOrEmpty(TempListName.Text))
			{
				MessageBoxMx.ShowError("A temporary list name must be selected first");
				return;
			}

			CidListCommand.EditTemp(TempListName.Text);
		}

		private void SavedList_CheckedChanged(object sender, EventArgs e)
		{
			DisableCriteriaChecks();
			ListName.Enabled = true;
			BrowseSaved.Enabled = true;
			EditSaved.Enabled = true;
			if (Visible) ListName.Focus();
		}

		private void ListName_TextChanged(object sender, EventArgs e)
		{
			if (ListName.Text.Length == 0) // allow for new list creation
				EditSaved.Text = "New";
			else EditSaved.Text = "Edit...";
		}

		private void BrowseSaved_Click(object sender, EventArgs e)
		{
			SavedList.Checked = true;
			ListName.Enabled = true;

			string tok = ListName.Text.Trim();
			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.CnList, "Browse Lists", SavedListUo);
			if (uo != null)
			{
				ListName.Text = uo.Name;
				string txt = uo.InternalName;
				SavedListUo = uo;
			}
		}

		private void EditSaved_Click(object sender, EventArgs e)
		{
			string internalName = ResolveSavedListUo();
			if (internalName == null) return; // nonexistant

			CidList cnList = CidListEditor.Edit(internalName);
			if (cnList != null) // set any new name
			{
				ListName.Text = cnList.UserObject.Name;
				SavedListUo = cnList.UserObject;
			}
		}

		private void Between_CheckedChanged(object sender, EventArgs e)
		{
			DisableCriteriaChecks();
			CidLo.Enabled = true;
			CidHi.Enabled = true;
			if (Visible) CidLo.Focus();
		}

		private void None_CheckedChanged(object sender, EventArgs e)
		{
			DisableCriteriaChecks();
		}

		private void OK_Click(object sender, EventArgs e)
		{
			int CnCount;
			string listName, listText, errMsg = "", tok, cidLo, cidHi, cid = null, cid2 = null;

			QueryColumn qc = Qc;
			MetaColumn mc = qc.MetaColumn;
			MetaTable mt = mc.MetaTable;

			// Perform validity check on form

			// Equality

			if (EQ.Checked)
			{
				tok = Cid.Text.ToUpper();

				if (tok == "")
				{
					XtraMessageBox.Show("Single compound value is missing", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
					Cid.Focus();
					return;
				}

				if (SS.I.ValidateCompoundIds)
				{
					if (tok == "")
						errMsg = "You must supply a " + qc.ActiveLabel;

					else if (RootTable.IsStructureDatabaseRootTable(mt.Root))
					{
						if (!CompoundId.IsValidForDatabase(tok, mt)) 
							errMsg = tok + " is not a valid " + qc.ActiveLabel;

						else
						{
							cid = CompoundId.Normalize(tok, mt);
							tok = CompoundId.Format(cid, mt);
							errMsg = "";
						}
					}

					else
					{
						cid = tok; // use as is
						errMsg = ""; // no error
					}

					if (errMsg != "")
					{
						MessageBoxMx.Show(errMsg, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
				}
				qc.CriteriaDisplay = "= " + tok;
				qc.Criteria = mc.Name + " = " + Lex.AddSingleQuotes(cid);
				CnCount = 1;
			}

// List

			else if (InList.Checked)
			{
				listText = CidListString.Trim();
				if (listText == "")
				{
					XtraMessageBox.Show("The list must contain one or more items", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
					if (CidListDisplayIsEditable()) CidListDisplay.Focus();
					return;
				}

				qc.CriteriaDisplay = CidList.FormatAbbreviatedCidListForDisplay(qc, listText);

				qc.Criteria = mc.Name + " IN (" + listText + ")";
			}

// Current list

			else if (TempList.Checked)
			{
				qc.CriteriaDisplay = "In temporary list: " + TempListName.Text;
				qc.Criteria = mc.Name + " IN LIST " + UserObject.TempFolderNameQualified + TempListName.Text;
				CnCount = SessionManager.CurrentResultKeysCount;
			}

// Saved list

			else if (SavedList.Checked)
			{
				if (ListName.Text.Trim() == "")
				{
					XtraMessageBox.Show("A list name must be supplied", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
					ListName.Focus();
					return;
				}

				string internalName = ResolveSavedListUo();
				if (internalName == null) return;

				qc.CriteriaDisplay = "In list: " + SavedListUo.Name;
				qc.Criteria = mc.Name + " IN LIST " + Lex.AddSingleQuotes("CNLIST_" + SavedListUo.Id.ToString()); // quote list name
				CnCount = 1; // may be larger
			}

// Between

			else if (Between.Checked)
			{
				cidLo = CidLo.Text.Trim();
				cidHi = CidHi.Text.Trim();
				if (cidLo == "" || cidHi == "")
				{
					XtraMessageBox.Show("Between value is missing", UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.None);
					if (CidLo.Text.Trim() == "") CidLo.Focus();
					else if (CidHi.Text.Trim() == "") CidHi.Focus();
					return;
				}

				if (SS.I.ValidateCompoundIds)
				{
					errMsg = "";
					if (cidLo == "")
						errMsg = "You must supply a " + qc.ActiveLabel;

					else
					{
						if (!CompoundId.IsValidForDatabase(cidLo, mt))
							errMsg = cidLo + " is not a valid " + qc.ActiveLabel;
						else
						{
							cid = CompoundId.Normalize(cidLo, mt);
							cidLo = CompoundId.Format(cid);
						}
					}
					if (errMsg != "")
					{
						MessageBoxMx.Show(errMsg, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
				}

				else cid = cidLo; // use as is

				if (SS.I.ValidateCompoundIds)
				{
					errMsg = "";
					if (cidHi == "")
						errMsg = "You must supply a " + qc.ActiveLabel;

					else
					{
						if (!CompoundId.IsValidForDatabase(cidHi, mt))
							errMsg = cidHi + " is not a valid " + qc.ActiveLabel;
						else
						{
							cid2 = CompoundId.Normalize(cidHi, mt);
							cidHi = CompoundId.Format(cid2);
						}
					}

					if (errMsg != "")
					{
						MessageBoxMx.Show(errMsg, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
				}

				else cid2 = cidHi; // use as is

				qc.CriteriaDisplay = "Between " + cidLo + " and " + cidHi;
				qc.Criteria = mc.Name + " BETWEEN " + Lex.AddSingleQuotes(cid) + " AND " + Lex.AddSingleQuotes(cid2);
			}

			else if (None.Checked)
			{
				qc.CriteriaDisplay = "";
				qc.Criteria = "";
				CnCount = 0;
			}

			DialogResult = DialogResult.OK;
//			this.Hide();
		}

/// <summary>
/// If SavedListUo is defined just return otherwise check in default folder for ListName 
/// </summary>

		string ResolveSavedListUo()
		{
			if (SavedListUo != null) return "CNLIST_" + SavedListUo.Id;
			if (ListName.Text == "") return "";

			UserObject uo = new UserObject(UserObjectType.CnList, SS.I.UserName, ListName.Text);
			UserObjectTree.AssignDefaultObjectFolder(uo, UserObjectType.CnList);
			SavedListUo = UserObjectDao.ReadHeader(uo);
			if (SavedListUo != null) return "CNLIST_" + SavedListUo.Id;

			string errMsg = "List " + ListName.Text + " does not exist";
			MessageBoxMx.Show(errMsg, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return null; // return null if nonexistant list
		}

		void DisableCriteriaChecks()
		{
			Cid.Enabled = false;
			BrowseRN.Enabled = false;
			CidListDisplay.Enabled = false;
			ImportList.Enabled = false;
			EditList.Enabled = false;
			TempListName.Enabled = false;
			EditTempList.Enabled = false;
			ListName.Enabled = false;
			BrowseSaved.Enabled = false;
			EditSaved.Enabled = false;
			CidLo.Enabled = false;
			CidHi.Enabled = false;
		}

		private void ListName_KeyPress(object sender, KeyPressEventArgs e)
		{
			SavedListUo = null; // SavedListUo no longer valid
		}

/// <summary>
/// If editable then get any changed value & assign to internal value CidListString
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CidListDisplay_Leave(object sender, EventArgs e)
		{
			if (!CidListDisplayIsEditable()) return;

			string listText = CidListDisplay.Text;
			if (Lex.IsDefined(listText))
				CidListString = listText;
		}

/// <summary>
/// If list can't be edited in the control then open it in the editor
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void CidListDisplay_Click(object sender, EventArgs e)
		{
			if (!CidListDisplayIsEditable()) EditList_Click(null, null);
		}

	}
}