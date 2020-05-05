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
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

namespace Mobius.ClientComponents
{
	public partial class CidListEditor : DevExpress.XtraEditors.XtraForm
	{
		static CidListEditor Instance;

		string InListName; // initial list name, may change
		string ListName; // current list name
		string Before; // initial list content
		int InitialCount; // initial count

		CidList CidList = null; // list being edited
		MetaTable RootTable; // root metatable to assoc with list
		int CidCount; // current count

		int PreviousSelectionStart;
		string PreviousCid = "";

		public const int EditInMemoryOnlyUoId = -2; // UserObject id that indicates the list is to be edited in memory only

		static MainMenuControl MainMenuControl { get { return SessionManager.Instance.MainMenuControl; }}
		static bool InTimer_Tick = false;

		[DllImport("User32.dll", EntryPoint = "SendMessage")]
		private static extern int SendMessage(
			int hWnd,
			int Msg,
			int wParam,
			int lParam);

		private const int WM_COMMAND = 0x111;
		private const int EM_GETLINECOUNT = 0xBA;
		private const int EM_LINEFROMCHAR = 0xC9;

		public CidListEditor()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Edit given a list name
		/// </summary>
		/// <returns></returns>

		public static CidList Edit(
			string inListName)
		{
			return Edit(inListName, QbUtil.CurrentQueryRoot);
		}

		/// <summary>
		/// Edit given a list name
		/// </summary>
		/// <returns></returns>

		public static CidList Edit(
			string inListName,
			MetaTable rootTable)
		{
			CidList cnList;
			if (Lex.Eq(inListName, "Current"))
				inListName = CidList.CurrentListInternalName;

			if (inListName == "")
			{
				cnList = new CidList(); // new list
				cnList.UserObject.Name = "New List";
			}

			else
			{
				cnList = CidListCommand.Read(inListName, rootTable);
				if (cnList == null) return null; // doesn't exist
				if (!cnList.UserObject.IsTempObject && MainMenuControl != null)
					MainMenuControl.UpdateMruList(cnList.UserObject.InternalName);
			}

			cnList = Edit(cnList, rootTable);
			return cnList;
		}

		/// <summary>
		/// Edit list
		/// </summary>
		/// <returns></returns>

		public static CidList Edit(
			CidList inList,
			MetaTable rootTable)
		{
			if (Instance == null) Instance = new CidListEditor();

			if (!Instance.Setup(inList, rootTable)) return null;

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK) return Instance.CidList;
			else return null;
		}

/// <summary>
/// Setup for edit
/// </summary>
/// <param name="inList"></param>
/// <param name="rootTable"></param>
/// <returns></returns>

		bool Setup(
			CidList inList,
			MetaTable rootTable)
		{
			string formattedCid, label, txt;

			RootTable = rootTable;

			InListName = inList.UserObject.Name;
			ListName = InListName;

			CidList = inList.Clone(); // make copy of list to edit
			CidCount = InitialCount = CidList.Count;
			if (CidCount > 32000) // too big to edit?
			{
				txt =
						"The list contains " + CidList.Count.ToString() + " compound numbers which\n" +
						"exceeds the maximum of 32,000 that can be directly edited.\n" +
						" Do you want to clear the list and start a new list?";

				DialogResult dr = MessageBoxMx.Show(txt, "List Too Large to Edit", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

				if (dr != DialogResult.Yes) return false;

					CidList = new CidList();
					CidList.UserObject = // set name properly
							UserObjectUtil.ParseInternalUserObjectName(InListName, UserObjectType.CnList);
			}

			Text = "Edit List - [" + CidList.UserObject.Name + "]";

			Before = FormatList(); // convert & save initial list

			if (rootTable != null) label = rootTable.KeyMetaColumn.Label; 
			else label = "Compound Id";
			
			ListLabel.Text = " " + label + " - (Enter one number per line) ";
			RNLabel.Text = label + ":";

			MolCtl.ClearMolecule(); // clear any structure
			CidCtl.Text = HeavyAtoms.Text = Weight.Text = Formula.Text = "";

			CidListCtl.Text = Before; // put text in editor

			DisplayStatusMsg("");

			if (SS.I.AllowGroupingBySalts) SaltsMenu.Enabled = true;
			else SaltsMenu.Enabled = false;

			return true;
		}

/// <summary>
/// Format list for display
/// </summary>
/// <param name="rootTable"></param>
/// <returns></returns>

		string FormatList()
		{
			string formattedCid;

			StringBuilder listBuild = new StringBuilder(CidList.Count * 12 + 32); // allocate expected length
			for (int li = 0; li < CidList.Count; li++)
			{
				if (listBuild.Length > 0) listBuild.Append("\r\n");
				string cid = CidList[li].Cid;
				if (SS.I.RemoveLeadingZerosFromCids) // normal formatting
					formattedCid = CompoundId.Format(cid, RootTable);
				else formattedCid = cid; // just use internal formatting
				listBuild.Append(formattedCid);
			}

			return listBuild.ToString();
		}

		void DisplayStatusMsg(
			string msg)
		{
			StatusMessage.Text = msg;
			Application.DoEvents();
			return;
		}

/// <summary>
/// Save changes & close
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveAndClose_Click(object sender, EventArgs e)
		{
			if (!Save()) return; // save but return without closing if any error
			DialogResult = DialogResult.OK;
			Visible = false;
		}

/// <summary>
/// Occurs after FileMenu item is pressed show dropdown
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void FileMenu_GetItemData(object sender, EventArgs e)
		{
			MainMenuControl.SetupTempListMenu(SaveAsMenuItem.DropDownItems, SaveAsTempListMenuItem_Click);
			FileContextMenu.Show(this, FileMenuItemPositionPanel.Left, FileMenuItemPositionPanel.Top);
		}

		private void SaveAsMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			MainMenuControl.SetupTempListMenu(SaveAsMenuItem.DropDownItems, SaveAsTempListMenuItem_Click);
		}

/// <summary>
/// Save the list prompting if no name yet
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveMenuItem_Click(object sender, EventArgs e)
		{
			Save();
		}

/// <summary>
/// SaveAs to a persisted list
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveAsSavedListMenuItem_Click(object sender, EventArgs e)
		{
			Save(true, false, null);
		}

/// <summary>
/// SaveAs to existing temp list
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveAsTempListMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			Save(true, true, mi.Text);
		}

/// <summary>
/// SaveAs to new temp list
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void SaveAsNewTempListMenuItem_Click(object sender, EventArgs e)
		{
			Save(true, true, null);
		}

		private void CloseMenuItem_Click(object sender, EventArgs e)
		{
			CloseButton_Click(sender, null);
		}

/// <summary>
/// Save to existing list
/// </summary>
/// <returns></returns>

		bool Save()
		{
			if (CidList.UserObject.Id == EditInMemoryOnlyUoId)
			{
				if (!ValidateList()) return false; // Validate the list if requested
				GetNormalizedListFromControl(CidList);
				return true;
			}

			else return Save(false, false, null);
		}

/// <summary>
/// Save/SaveAs the list
/// </summary>
/// <param name="saveAs">SaveAs under another list name</param>
/// <param name="saveAsTempList">Save as a temp list</param>
/// <param name="tempListName">Temp list name to save under, prompt if null</param>
/// <returns></returns>

		bool Save(
			bool saveAs,
			bool saveAsTempList,
			string tempListName)
		{
			if (!ValidateList()) return false; // Validate the list if requested

// Get list name if needed & check that user can modify it

			if (saveAs || CidList.UserObject.Id == 0)
			{
				CidList.UserObject.Id = 0; // assign new id
				if (saveAsTempList)
				{
					if (tempListName == null) // prompt for name
					{
						tempListName = CidListCommand.PromptForNewTempListName();
						if (tempListName == null) return false;
					}
					CidList.UserObject = new UserObject(UserObjectType.CnList);
					CidList.UserObject.ParentFolder = UserObject.TempFolderName;
					CidList.UserObject.Name = tempListName;
					CidList.UserObject.Owner = SS.I.UserName;
				}

				else // save as permanent list
				{
					UserObject existingUo = CidList.UserObject;
					if (CidList.UserObject.Id == 0) existingUo = new UserObject(UserObjectType.CnList);
					UserObject listUo = UserObjectSaveDialog.Show("Save As", existingUo);
					if (listUo == null) return false;
					CidList.UserObject = listUo;
				}
			}

			if (!UserObjectUtil.UserHasWriteAccess(CidList.UserObject))
			{ // is the user authorized to save this list?
				MessageBoxMx.ShowError("You are not authorized to save this list");
				return false;
			}

// Write out the list

			DisplayStatusMsg("Saving list...");

			if (CidList.UserObject.HasDefinedParentFolder)
			{

				MetaTreeNode targetFolder = UserObjectTree.GetValidUserObjectTypeFolder(CidList.UserObject);
				if (targetFolder == null)
				{ // shouldn't happen
					MessageBoxMx.ShowError("The list could not be saved because the folder it was in no longer exists.  Please try SaveAs instead.");
					return false;
				}
			}

			GetNormalizedListFromControl(CidList);

			if (!CidList.UserObject.IsCurrentObject) // normal list?
				CidListCommand.Write(CidList);

			else // special processing for current list
			{
				SessionManager.LockResultsKeys = true; // avoid possible overwrite of ResultsKeys if in single step query
				int rc = CidListCommand.Write(CidList);
				List<string> curListString = CidListCommand.ReadCurrentListLocal().ToStringList();
				SessionManager.CurrentResultKeys = curListString;
				SessionManager.DisplayCurrentCount();
				SessionManager.LockResultsKeys = false;
			}

			//string cFile = SS.I.ClientDefaultDir + @"\Previous.lst"
			//if (UIMisc.CanWriteFileToDefaultDir(cFile))
			//{
			//  StreamWriter sw = new StreamWriter(cFile); // save copy to disk
			//  sw.Write(CidList.UserObject.Content);
			//  sw.Close();
			//}

			int cncnt = CidList.Count;

			DisplayStatusMsg("List saved");

			Before = CidListCtl.Text;
			string tok = UserObjectUtil.GetName(ListName);
			Text = "Edit List - [" + CidList.UserObject.Name + "]";
			return true;
		}

/// <summary>
/// Validate list of cids
/// </summary>
/// <returns></returns>

		bool ValidateList()
		{
			if (!ValidateNumbers.Checked || RootTable == null) return true;

			DisplayStatusMsg("Validating numbers...");
			string invalidCid = CompoundIdUtil.ValidateList(CidListCtl.Text, RootTable.Name);
			if (!String.IsNullOrEmpty(invalidCid))
			{
				int i2 = Lex.IndexOf(CidListCtl.Text, invalidCid); // original position
				if (i2 >= 0)
				{
					CidListCtl.Select(i2, invalidCid.Length); // select bad compound id
					CidListCtl.Focus();
				}

				string errorMsg = invalidCid + " is an invalid " + RootTable.KeyMetaColumn.Label;
				DisplayStatusMsg(errorMsg); // put up error message
				SystemUtil.Beep();
				return false;
			}

			DisplayStatusMsg("");
			return true;
		}

/// <summary>
/// Get list from control removing blank lines & normalizing the ids
/// </summary>
/// <param name="CidList"></param>

		void GetNormalizedListFromControl(CidList CidList)
		{
			CidList.Clear(); // clear contents, keep name

			string[] cna = CidListCtl.Lines;

			bool allowDuplicates = !RemoveDuplicates.Checked;

			for (int i1 = 0; i1 < cna.Length; i1++)
			{
				string extCn = cna[i1];
				if (extCn.Trim() == "" || extCn.StartsWith("(")) continue;
				string intCid = CompoundId.Normalize(extCn, RootTable);
				if (String.IsNullOrEmpty(intCid)) intCid = extCn;
				CidList.Add(intCid, allowDuplicates);
			}
			return;
		}


		private void CloseButton_Click(object sender, EventArgs e)
		{
			string listText = CidListCtl.Text;

			if (listText != Before) // any change?
			{
				DialogResult dr = MessageBoxMx.Show(
					"Do you want to save the changes you made to " + CidList.UserObject.Name + "?",
					UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (dr == DialogResult.Yes)
				{
					SaveAndClose_Click(null, null);
					return;
				}
				else if (dr == DialogResult.Cancel) return;
			}

			DialogResult = DialogResult.Cancel;
			Visible = false;
		}

		private void CidListEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!this.Visible) return; // just return if we already know it's closed
			CloseButton_Click(null, null);
		}

		private void CopyStructure_Click(object sender, EventArgs e)
		{
			if (CidCtl.Text != "") MolCtl.CopyMoleculeToClipboard();
		}

		private void CidListEditor_Activated(object sender, EventArgs e)
		{
			Timer.Enabled = true;
			PreviousSelectionStart = -1;
			if (PreviousCid != "")
				PreviousCid = "";
			CidListCtl.SelectionStart = 0;
			CidListCtl.SelectionLength = 0;
			CidListCtl.Focus();
		}

		private void CidListEditor_Deactivate(object sender, EventArgs e)
		{
			Timer.Enabled = false;
		}

/// <summary>
/// Handle timer tick & update currently selected structure
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			string cid;
			int count, position;

			if (ServiceFacade.ServiceFacade.InServiceCall) return; // avoid multiplexing service calls that could cause problems
			if (InTimer_Tick) return;
			InTimer_Tick = true;

			TextBoxMaskBox textBox = GetCidListTextBox();
			count = SendMessage(textBox.Handle.ToInt32(), EM_GETLINECOUNT, 0, 0); // total line count
			position = SendMessage(textBox.Handle.ToInt32(), EM_LINEFROMCHAR, -1, 0); // current line
			position++;

			string txt = textBox.MaskBoxText;
			if (txt.Length > 0 && txt[txt.Length - 1] == '\n') count--; // decrement count if last line is blank
			if (position > count) count = position;
			string label = "Item " + position + " of " + count;
			if (label != DisplayFrame.Text) DisplayFrame.Text = label;

			cid = GetCurrentCid(textBox, false);
			if (PreviousCid != cid)
			{
				DisplayCidData(cid);
				PreviousCid = cid;
			}
			InTimer_Tick = false;
			return;
		}

/// <summary>
/// Get the TextBoxMaskBox that is wrapped in the CidListCtl
/// </summary>
/// <returns></returns>

		TextBoxMaskBox GetCidListTextBox()
		{
			FieldInfo fi = typeof(TextEdit).GetField("_maskBox", BindingFlags.Instance | BindingFlags.NonPublic); // get underlying TextBox to SendMessage to
			TextBoxMaskBox textBox = fi.GetValue(CidListCtl) as TextBoxMaskBox;
			return textBox;
		}

		/// <summary>
		/// Get the compound id that the cursor is on
		/// </summary>
		/// <param name="cnList"></param>
		/// <param name="selectCid"></param>
		/// <returns></returns>

		string GetCurrentCid(bool selectCid)
		{
			return GetCurrentCid(GetCidListTextBox(), selectCid);
		}

/// <summary>
/// Get the compound id that the cursor is on
/// </summary>
/// <param name="cnList"></param>
/// <param name="selectCid"></param>
/// <returns></returns>

		string GetCurrentCid(
			TextBox listTextBox,
			bool selectCid)
		{
			string txt, cn;
			int i0, i1, i2;

			txt = listTextBox.Text;
			if (txt.Length == 0) return "";
			i0 = listTextBox.SelectionStart;
			if (i0 < 0) return "";
			for (i1 = i0 - 1; i1 >= 0; i1--)
			{
				if (txt[i1] == '\r' || txt[i1] == '\n') break;
			}
			i1++; // index of 1st char

			for (i2 = i0; i2 < txt.Length; i2++)
			{
				if (txt[i2] == '\r' || txt[i2] == '\n') break;
			}

			//			if (i2>=i1) return;

			cn = txt.Substring(i1, i2 - i1);
			if (selectCid) listTextBox.Select(i1, i2 - i1 + 2);
			return cn;
		}

/// <summary>
/// Display the structure and other basic data for the supplied cid
/// </summary>
/// <param name="cidArg"></param>

		void DisplayCidData(string cidArg)
		{
			MoleculeMx str = null;
			string mwTxt = "", mfTxt = "", haTxt = "", tok;

			string intCid = CompoundId.Normalize(cidArg, RootTable);
			string extCid = CompoundId.Format(intCid, RootTable);
			CidCtl.Text = extCid;

			if (RootTable == null || RootTable.KeyMetaColumn.DataType == MetaColumnType.CompoundId)
				str = MoleculeUtil.SelectMoleculeForCid(intCid, RootTable);

			if (Lex.IsDefined(str?.PrimaryValue)) // have a structure with at least one atom
			{
				if (str.PrimaryDisplayFormat == MoleculeRendererType.Chemistry)
				{
					int ha = str.HeavyAtomCount;
					if (ha > 0) haTxt = ha.ToString();

					double mw = str.MolWeight;
					if (mw >= 0) mwTxt = String.Format("{0:f3}", mw);
					HeavyAtoms.Text = "";
					Weight.Text = mwTxt;

					string mf = str.MolFormula;
					HeavyAtoms.Text = "";
					Formula.Text = mf;
				}

				else { } // biopolymer, don't calc structure props (too slow for now)

				HeavyAtoms.Text = haTxt;
				Weight.Text = mwTxt;
				Formula.Text = mfTxt;

				MolCtl.Molecule = str;

				return;
			}

			else // no structure
			{
				HeavyAtoms.Text = "";
				Weight.Text = "";
				Formula.Text = "";
				MolCtl.ClearMolecule();
				return;
			}
		}

		private void InsertSaltsForCurrentCompound_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			string cid = GetCurrentCid(false);

			if (cid.Trim().Length == 0) return;
			List<string> al = MoleculeUtil.GetAllSaltForms(cid);
			if (al == null || al.Count == 0)
			{
				DisplayStatusMsg("No salts for " + CompoundId.Format(cid, RootTable));
				SystemUtil.Beep();
				return;
			}

			StringBuilder listBuild = new StringBuilder();
			foreach (string s in al)
			{ // build formatted list with added items marked
				if (s == cid) continue; // don't add self
				listBuild.Append(CompoundId.Format(s, RootTable));
				listBuild.Append(" (+)");
				listBuild.Append("\r\n");
			}
			InsertCidListText(listBuild.ToString());
			return;
		}

		/// <summary>
		/// Insert text following current number
		/// </summary>
		/// <param name="text"></param>

		void InsertCidListText(
			string insText)
		{
			try
			{
				int i0, i1, i2;
				TextBoxMaskBox textBox = GetCidListTextBox();
				string txt = CidListCtl.Text;
				i0 = CidListCtl.SelectionStart;
				if (i0 < 0) return;

				for (i2 = i0; i2 < txt.Length; i2++)
				{
					if (txt[i2] == '\r' || txt[i2] == '\n') break;
				}

				if (i2 >= txt.Length)
					txt += "\r\n";

				i2 += 2;

				string endTxt = "";
				if (i2 < txt.Length) endTxt = txt.Substring(i2);

				CidListCtl.Text = txt.Substring(0, i2) + insText + endTxt; // set new text

				CidListCtl.SelectionStart = i2;
				CidListCtl.SelectionLength = insText.Length;
				CidListCtl.ScrollToCaret(); // be sure visible
			}
			catch (Exception ex) { return; }
		}

		private void InsertSaltsForAllCompounds_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			DisplayStatusMsg("Inserting Salts, wait please...");
			GetNormalizedListFromControl(CidList);
			List<string> al = MoleculeUtil.InsertSalts(CidList.ToStringList());

			CidList newList = new CidList(al);
			newList.UserObject = CidList.UserObject;
			StringBuilder listBuild = new StringBuilder(newList.Count * 12 + 32); // build formatted list here

			foreach (CidListElement cle in newList.List)
			{ // build formatted list with added items marked
				listBuild.Append(CompoundId.Format(cle.Cid, RootTable));
				if (!CidList.Contains(cle.Cid)) listBuild.Append(" (+)");
				listBuild.Append("\r\n");
			}

			CidList = newList;
			CidListCtl.Text = listBuild.ToString();
			DisplayStatusMsg("");
			return;
		}

		private void GroupSalts_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			DisplayStatusMsg("Grouping Salts, wait please...");
			GetNormalizedListFromControl(CidList);
			List<string> al = MoleculeUtil.GroupSalts(CidList.ToStringList());
			CidList newList = new CidList(al);
			newList.UserObject = CidList.UserObject;
			CidList = newList;
			string formattedList = FormatList();
			CidListCtl.Text = formattedList; // put text in editor
			CidListCtl.Select(0, 0); // position cursor at beginning of list
			DisplayStatusMsg("");
			return;
		}

		private void CutMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			Clipboard.SetDataObject(CidListCtl.SelectedText, true);
			CidListCtl.SelectedText = "";
			CidListCtl.Focus();
		}

		private void CopyMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			Clipboard.SetDataObject(CidListCtl.SelectedText, true);
			CidListCtl.Focus();
		}

		private void CopyAsCsvMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			string txt = CidListCtl.SelectedText;
			txt = txt.Replace("\n", ","); // replace new lines with commas
			txt = txt.Replace("\r", "");
			Clipboard.SetDataObject(txt, true);
			CidListCtl.Focus();
		}

		private void PasteMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			IDataObject iData = Clipboard.GetDataObject();
			if (iData.GetDataPresent(DataFormats.Text))
				CidListCtl.SelectedText = (string)iData.GetData(DataFormats.Text);
			CidListCtl.Focus();
		}

		private void DeleteMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			GetCurrentCid(true);
			CutMenuItem_ItemClick(sender, e);
		}

		private void ClearListMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			DialogResult dr = MessageBoxMx.Show(
				"Are you sure you want to clear the list?", "Clear List",
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

			if (dr == DialogResult.Yes)
			{
				CidListCtl.Text = "";
				CidCtl.Text = HeavyAtoms.Text = Weight.Text = Formula.Text = "";
				MolCtl.ClearMolecule();
				DisplayFrame.Text = "Item 1 of 1";
			}
			CidListCtl.Focus();
		}

		private void SortAscendingMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			DisplayStatusMsg("Sorting, wait please...");
			UserObject uo = CidList.UserObject;
			CidList = new CidList(CidListCtl.Text);
			CidList.UserObject = uo;
			CidList.Sort(SortOrder.Ascending);
			string formattedList = FormatList();
			CidListCtl.Text = formattedList;
			CidListCtl.Focus();
			DisplayStatusMsg("");
		}

		private void SortDescendingMenuItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			DisplayStatusMsg("Sorting, wait please...");
			UserObject uo = CidList.UserObject;
			CidList = new CidList(CidListCtl.Text);
			CidList.UserObject = uo;
			CidList.Sort(SortOrder.Descending);
			string formattedList = FormatList();
			CidListCtl.Text = formattedList;
			CidListCtl.Focus();
			DisplayStatusMsg("");
		}

	}
}