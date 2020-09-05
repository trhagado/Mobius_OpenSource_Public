using System.Diagnostics;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DevExpress.XtraSplashScreen;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Main menu control
	/// Font: Segoe UI 9pt (previously Tahoma)
	/// </summary>

	public partial class MainMenuControl : UserControl
	{
		// Event raised when menu is selected so menuitems can be enabled/disabled
		public event Action AfterEditMenuSelected;

		public List<string> MruList = new List<string>(); // list of most-recently-used user objects
		public static int MaxStoredMruItems = 100;
		public static int MaxDisplayedMruItems = 15;

		public List<string> Favorites = new List<string>(); // list of favorite contents tree node names
		public Dictionary<string, UserObject> MenuUserObjects; // UserObjects in MruList & Favorites

		public ToolStripMenuItem CurrentUserObjectMenuItem; // used for deletion
		ToolStripMenuItem PendingMainMenuItem; // pending command to be executed at next tick

		private RibbonForm RibbonForm // get the RibbonForm associated with the session
		{
			get
			{
				if (SessionManager.Instance == null) return null;
				return SessionManager.Instance.ShellForm as RibbonForm;
			}
		}

		private RibbonControl RibbonControl // get the RibbonControl associated with the session
		{
			get
			{
				if (SessionManager.Instance == null) return null;
				return SessionManager.Instance.RibbonCtl;
			}
		}

		private ApplicationMenu ApplicationMenu // get the RibbonControl Application menu associated with the session
		{
			get
			{
				if (RibbonControl == null) return null;
				return RibbonControl.ApplicationButtonDropDownControl as ApplicationMenu;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>

		public MainMenuControl()
		{
			InitializeComponent();
			Height = 19;
		}

		/// <summary>
		/// Scan menus checking for shortcuts
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>

		public bool CheckShortcuts(Keys keyData)
		{
			KeyEventArgs e = new KeyEventArgs(keyData);

			if (e.Control && // special handling for copy, cut paste shortcuts
				(e.KeyCode == Keys.C ||
				e.KeyCode == Keys.X ||
				e.KeyCode == Keys.V))
			{
				Control c = UIMisc.GetFocusedControl();

				if (c is MoleculeGridControl)
					return false; // grid handles these itself, don't post command here or duplicates (e.g. control-V annotation pastes may occur)

				else if (c is DevExpress.XtraTreeList.TreeList && c.Parent is Mobius.ClientComponents.ContentsTreeControl &&
					c.Parent.Parent is Mobius.ClientComponents.QbContentsTree) { } // special handling for Contents Tree in menu command

				else return false; // let other controls handle the keys themselves
			}

			ToolStripMenuItem tsmi = CheckShortcuts(e);
			if (tsmi == null) return false;

			PendingMainMenuItem = tsmi;
			Timer.Enabled = true;
			return true;
		}

		public bool CheckMenuActivationKey(Keys keyData)
		{
			ContextMenuStrip menu = null;
			KeyEventArgs e = new KeyEventArgs(keyData);
			if (e.Control) return false;

			if (e.KeyCode == Keys.F) ShowContextMenu(FileMenu);
			else if (e.KeyCode == Keys.E) ShowContextMenu(EditMenu);
			else if (e.KeyCode == Keys.Q) ShowContextMenu(QueryMenu);
			else if (e.KeyCode == Keys.L) ShowContextMenu(ListMenu);
			else if (e.KeyCode == Keys.F) ShowContextMenu(FavoritesMenu);
			else if (e.KeyCode == Keys.H) ShowContextMenu(HistoryMenu);
			else if (e.KeyCode == Keys.T) ShowContextMenu(ToolsMenu);
			else if (e.KeyCode == Keys.H) ShowContextMenu(HelpMenu);
			else return false;

			return true;
	}

		//public bool CheckShortcuts(Keys keyData)
		//{
		////(keyData & Keys.Control) != 0 && 
		//  KeyEventArgs e = new KeyEventArgs(keyData);
		//  if (!e.Control) return false;
		//  if (e.KeyCode == Keys.R)
		//  {
		//    return true;
		//  }

		//  else if (e.KeyCode == Keys.F)
		//  {menuEditFind_Click
		//    return true;
		//  }

		//  else return false;
		//}

		/// <summary>
		/// Scan menus checking for shortcuts
		/// </summary>
		/// <param name="e"></param>

		public ToolStripMenuItem CheckShortcuts(KeyEventArgs e)
		{
			if (!e.Control) return null; // control keys only

			ContextMenuStrip[] MainContextMenus = new ContextMenuStrip[] {
				FileMenu,
				EditMenu,
				QueryMenu,
				ListMenu,
				FavoritesMenu,
				HistoryMenu,
				ToolsMenu,
				HelpMenu };

			foreach (ContextMenuStrip menu in MainContextMenus)
			{
				ToolStripMenuItem tsmi = CheckShortcuts(e, menu.Items);
				if (tsmi != null) return tsmi;
			}

			return null;
		}

		public ToolStripMenuItem CheckShortcuts(
			KeyEventArgs e,
			ToolStripItemCollection items)
		{
			foreach (ToolStripItem item in items)
			{
				ToolStripMenuItem tsmi = item as ToolStripMenuItem;
				if (tsmi == null) continue;

				Keys k = tsmi.ShortcutKeys;
				if ((k & Keys.Control) != 0)
				{
					k = k & ~Keys.Control;
					if (k == e.KeyCode)
					{
						e.SuppressKeyPress = true;
						return tsmi;

					}
				}

				if (tsmi.HasDropDownItems)
				{
					tsmi = CheckShortcuts(e, tsmi.DropDown.Items);
					if (tsmi != null) return tsmi;
				}

			}

			return null;
		}

		public void ShowContextMenu(
			ContextMenuStrip menu)
		{
			BarStaticItem barItem = menu.Tag as BarStaticItem;
			if (barItem == null) return;

			Rectangle r = barItem.Links[0].ScreenBounds;

			menu.Show(r.Left + 8, r.Bottom);
			menu.Focus();

			return;
		}

		public ContextMenuStrip BuildContextMenu(
			ToolStripMenuItem sourceMenu,
			BarStaticItem barItem)
		{
			ContextMenuStrip m = null;

			if (sourceMenu.DropDownItems.Count > 0)
			{
				m = new ContextMenuStrip();
				m.ImageList = MainMenu.ImageList;

				List<ToolStripItem> l = new List<ToolStripItem>();
				foreach (ToolStripItem tsi in sourceMenu.DropDownItems)
					l.Add(tsi);

				m.Items.AddRange(l.ToArray());
				sourceMenu.Tag = m;
			}

			m = sourceMenu.Tag as ContextMenuStrip;
			if (m == null) m = new ContextMenuStrip();
			return m;

			//foreach (ToolStripItem item in sourceMenu.DropDownItems)
			//{
			//	ToolStripMenuItem tsmi = item as ToolStripMenuItem;
			//	if (tsmi == null) continue;

			//	m.Items.Add(tsmi);
			//}


				//foreach (QueryColumn qc in qt.QueryColumns)
				//{ // get list of allowed field labels/names for each table
				//	if (!QueryTableControl.QueryColumnVisible(qc) || !qc.MetaColumn.IsSearchable) continue;
				//	if (firstLabel) firstLabel = false;
				//	string label = CriteriaEditor.GetUniqueColumnLabel(qc);
				//	ToolStripMenuItem mi = new ToolStripMenuItem(label, null, new System.EventHandler(SelectedField_Click));
				//	mi.Tag = "T" + (ri + 1);
				//	m.Items.Add(mi);
				//}


				//		this.FileContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
				//		this.FileContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				//           this.dudeToolStripMenuItem
				//});
				//		this.FileContextMenu.Name = "FileContextMenu";
				//		this.FileContextMenu.Size = new System.Drawing.Size(211, 56);

				//return m;
			}



		////////////////////////////////////////////////
		///////////////// File Menu ////////////////////
		////////////////////////////////////////////////

		public void ExecuteCommand(string command)
		{
			CommandExec.ExecuteCommandAsynch(command);
		}

		private void menuFileNewQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewQuery");
			CommandExec.ExecuteCommandAsynch("NewQuery");
		}

		private void menuFileNewList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewList");
			CommandExec.ExecuteCommandAsynch("List New");
		}

		private void FileNewAnnotationTable_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewAnnotation");
			CommandExec.ExecuteCommandAsynch("NewAnnotation");
		}

		private void FileNewCalculatedField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewCalculatedField");
			CommandExec.ExecuteCommandAsynch("NewCalcField");
		}

		private void FileNewConditionalFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewCondFormat");
			CommandExec.ExecuteCommandAsynch("NewCondFormat");
		}

		private void FileOpenCondFormat_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileExistingCondFormat");
			CommandExec.ExecuteCommandAsynch("ExistingCondFormat");
		}

		private void FileNewUserDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewUserDatabase");
			CommandExec.ExecuteCommandAsynch("NewUserDatabase");
		}

		private void FileNewSpotfireLinkMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileNewSpotfireLink");
			CommandExec.ExecuteCommandAsynch("NewSpotfireLink");
		}

		private void FileOpenQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileOpenQuery");
			CommandExec.ExecuteCommandAsynch("OpenQuery");
		}

		private void FileOpenList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileOpenList");
			CommandExec.ExecuteCommandAsynch("List EditSaved");
		}

		private void FileOpenAnnotation_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileOpenAnnotation");
			CommandExec.ExecuteCommandAsynch("OpenAnnotation");
		}

		private void FileOpenCalculatedField_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileOpenCalculatedField");
			CommandExec.ExecuteCommandAsynch("OpenCalcField");
		}

		private void FileEditSpotfireLinkMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileEditSpotfireLink");
			CommandExec.ExecuteCommandAsynch("EditSpotfireLink");
		}

		private void FileOpenUserDatabaseMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileOpenUserDatabase");
			CommandExec.ExecuteCommandAsynch("OpenUserDatabase");
		}

		private void menuClose_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileClose");
			CommandExec.ExecuteCommandAsynch("FileClose");
		}

		private void menuCloseAll_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileCloseAll");
			CommandExec.ExecuteCommandAsynch("FileCloseAll");
		}

		private void menuSave_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileSave");
			CommandExec.ExecuteCommandAsynch("FileSave");
		}

		private void menuSaveAs_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileSaveAs");
			CommandExec.ExecuteCommandAsynch("FileSaveAs");
		}

		private void Print_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FilePrint");
			CommandExec.ExecuteCommandAsynch("Print");
		}

		private void Exit_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("FileExit");
			CommandExec.ExecuteCommandAsynch("Exit");
			return;
		}

		////////////////////////////////////////////////
		///////////////// Edit Menu ////////////////////
		////////////////////////////////////////////////

		private void menuCut_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("EditCut");
			try
			{
				Control c = UIMisc.GetFocusedControl();
				if (c == null) return;

				if (c is MoleculeGridControl) // special case for MoleculeGrid
					(c as MoleculeGridControl).Helpers.CutGridRangeMenuItem_Click(null, null);

				else if (c is DevExpress.XtraTreeList.TreeList && c.Parent is Mobius.ClientComponents.ContentsTreeControl &&
					c.Parent.Parent is Mobius.ClientComponents.QbContentsTree)
				{
					QbContentsTree qbct = c.Parent.Parent as QbContentsTree;
					qbct.ExecuteContentsTreeCommand("ContentsCut");
					return;
				}

				else // send cut command to currently active control
					SendKeys.Send("^X");
			}
			catch (Exception ex) { return; }
		}

		private void menuCopy_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("EditCopy");

			try
			{
				Control c = UIMisc.GetFocusedControl();
				if (c == null) return;

				if (c is MoleculeGridControl) // special case for MoleculeGrid
					(c as MoleculeGridControl).Helpers.CopyGridRangeMenuItem_Click(null, null);

				else if (c is DevExpress.XtraTreeList.TreeList && c.Parent is Mobius.ClientComponents.ContentsTreeControl &&
					c.Parent.Parent is Mobius.ClientComponents.QbContentsTree)
				{
					QbContentsTree qbct = c.Parent.Parent as QbContentsTree;
					MetaTreeNode node = qbct.CurrentContentsMetaTreeNode; // node operating on

					if (node == null || !node.IsUserObjectType)
						return; // must be UserObject node (note: ignore spurious copy commands in China)

					qbct.ExecuteContentsTreeCommand("ContentsCopy");
					return;
				}

				else // send copy command to currently active control
					SendKeys.Send("^C");
			}
			catch (Exception ex) { return; }
		}

		private void menuPaste_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("EditPaste");

			try
			{
				Control c = UIMisc.GetFocusedControl();
				if (c == null) return;

				if (c is MoleculeGridControl) // special case for MoleculeGrid
					(c as MoleculeGridControl).Helpers.PasteGridRangeMenuItem_Click(null, null);

				else if (c is DevExpress.XtraTreeList.TreeList && c.Parent is Mobius.ClientComponents.ContentsTreeControl &&
					c.Parent.Parent is Mobius.ClientComponents.QbContentsTree)
				{
					QbContentsTree qbct = c.Parent.Parent as QbContentsTree;
					qbct.ExecuteContentsTreeCommand("ContentsPaste");
					return;
				}

				else // send paste command to currently active control
					SendKeys.Send("^V");
			}
			catch (Exception ex) { return; }
		}

		private void EditDeleteMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("EditDelete");

			try
			{
				Control c = UIMisc.GetFocusedControl();
				if (c == null) return;

				//if (c is MoleculeGridControl) // special case for MoleculeGrid // (just send as key)
				//  (c as MoleculeGridControl).Helpers.DeleteGridRangeMenuItem_Click(null, null);

				else if (c is DevExpress.XtraTreeList.TreeList && c.Parent is Mobius.ClientComponents.ContentsTreeControl &&
					c.Parent.Parent is Mobius.ClientComponents.QbContentsTree)
				{
					QbContentsTree qbct = c.Parent.Parent as QbContentsTree;
					qbct.ExecuteContentsTreeCommand("ContentsDelete");
					return;
				}

				else // send delete command to currently active control
					SendKeys.Send("{DELETE}");
			}
			catch (Exception ex) { return; }
		}

		private void menuEditFind_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("EditFind");

			if (QbUtil.CurrentMode == QueryMode.Build)
			{ // if build mode search contents
				ContentsTreeControl ctc = SessionManager.Instance.MainContentsTree;
				if (ctc == null) return;
				SessionManager.LogCommandUsage("EditFindInContents");
				ctc.FindInContents("");
				return;
			}


			else if (QbUtil.CurrentMode == QueryMode.Browse)
			{ // if browse mode search grid
				MoleculeGridControl mg = QbUtil.QueryResultsControl?.RootQueryQm?.MoleculeGrid;
				if (mg == null) return;

				SessionManager.LogCommandUsage("EditFindInGrid");
				EditFind.Show(mg);
			}
		}

		////////////////////////////////////////////////
		///////////////// Query Menu ////////////////////
		////////////////////////////////////////////////

		private void menuNewQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryNew");
			CommandExec.ExecuteCommandAsynch("NewQuery");
		}

		private void menuOpenSavedQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryOpen");
			CommandExec.ExecuteCommandAsynch("OpenQuery");
		}

		private void menuOpenPrevQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryOpenPrevious");
			CommandExec.ExecuteCommandAsynch("PreviousQuery");
		}

		private void menuCloseQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryClose");
			CommandExec.ExecuteCommandAsynch("FileClose");
		}

		private void menuSaveQuery_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QuerySave");
			CommandExec.ExecuteCommandAsynch("FileSave");
		}

		private void menuSaveQueryAs_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QuerySaveAs");
			CommandExec.ExecuteCommandAsynch("FileSaveAs");
		}

		public void QueryRunMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryRun");
			CommandExec.ExecuteCommandAsynch("RunQuery");
		}

		private void menuQueryOptions_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryOptions");
			CommandExec.ExecuteCommandAsynch("QueryOptions");
		}

		private void DuplicateMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryDuplicate");
			CommandExec.ExecuteCommandAsynch("DuplicateQuery");
		}

		private void mainMenuRemoveAllCriteria_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryRemoveAllCriteria");
			CommandExec.ExecuteCommandAsynch("RemoveAllCriteria");
		}

		private void menuClearQuery_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ClearQuery");
		}

		private void RenameQueryMenuItem_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("RenameQuery");
		}

		////////////////////////////////////////////////
		///////////////// List Menu ////////////////////
		////////////////////////////////////////////////

		private void NewListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListNew");
			CommandExec.ExecuteCommandAsynch("List New");
		}

		private void OpenSavedListMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			SetupTempListMenu(OpenListMenuItem.DropDownItems, OpenTempListMenuItem_Click);
		}

		private void OpenSavedListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListOpenSaved");
			CommandExec.ExecuteCommandAsynch("List EditSaved");
		}

		private void OpenTempListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListEditTemp");
			CommandExec.ExecuteCommandAsynch("List EditTemp " + Lex.AddDoubleQuotes(((ToolStripMenuItem)sender).Text));
		}

		private void SaveTempListMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			SetupTempListMenu(SaveCurrentListMenuItem.DropDownItems, false, SaveAsTempListMenuItem_Click);
		}

		private void SaveAsSavedListMenuItem_Click(object sender, EventArgs e)
		{ // save current as a saved list
			SessionManager.LogCommandUsage("ListSave");
			CommandExec.ExecuteCommandAsynch("List SaveCurrent");
		}

		private void SaveAsTempListMenuItem_Click(object sender, EventArgs e)
		{ // save current to an existing temp list
			SessionManager.LogCommandUsage("ListSaveCurrentToTemp");
			CommandExec.ExecuteCommandAsynch("List SaveCurrentToTemp " + Lex.AddDoubleQuotes(((ToolStripMenuItem)sender).Text));
		}

		private void SaveAsNewTempListMenuItem_Click(object sender, EventArgs e)
		{ // save current to a new temp list
			SessionManager.LogCommandUsage("ListSaveCurrentToNewTemp");
			CommandExec.ExecuteCommandAsynch("List SaveCurrentToNewTemp " + Lex.AddDoubleQuotes(((ToolStripMenuItem)sender).Text));
		}

		private void CopyToTempListMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			SetupTempListMenu(CopyToCurrentListMenuItem.DropDownItems, false, CopyTempToCurrentMenuItem_Click);
		}

		private void CopySavedToCurrentMenuItem_Click(object sender, EventArgs e)
		{ // copy a saved list to the current list
			SessionManager.LogCommandUsage("List CopySavedToCurrent");
			CommandExec.ExecuteCommandAsynch("List CopySavedToCurrent");
		}

		private void CopyTempToCurrentMenuItem_Click(object sender, EventArgs e)
		{ // copy current list to a temp list
			SessionManager.LogCommandUsage("ListCopyTempToCurrent");
			CommandExec.ExecuteCommandAsynch("List CopyTempToCurrent " + Lex.AddDoubleQuotes(((ToolStripMenuItem)sender).Text));
		}

		private void CombineListsMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListLogically Combine");
			CommandExec.ExecuteCommandAsynch("List Logic");
		}

		private void ImportListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListImport");
			CommandExec.ExecuteCommandAsynch("List Import");
		}

		private void ExportListMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListExport");
			CommandExec.ExecuteCommandAsynch("List Export");
		}

		/// <summary>
		/// Setup a temp list menu
		/// </summary>
		/// <param name="items"></param>
		/// <param name="eventHandler"></param>

		public static void SetupTempListMenu(
			ToolStripItemCollection items,
			EventHandler eventHandler)
		{
			SetupTempListMenu(items, true, eventHandler);
		}

		/// <summary>
		/// Setup a temp list menu
		/// </summary>
		/// <param name="items"></param>
		/// <param name="includeCurrentList"></param>
		/// <param name="eventHandler"></param>

		public static void SetupTempListMenu(
			ToolStripItemCollection items,
			bool includeCurrentList,
			EventHandler eventHandler)
		{
			ToolStripItem item = null;

			int firstTempObjPos = -1;
			int mi = 0;
			while (mi < items.Count)
			{ // remove any existing temp list items from menu
				item = items[mi];
				if (Lex.Eq(item.Text, "Current") || Lex.Eq(item.Text, "Current List") ||
					item.Tag is TempCidList)
				{
					if (firstTempObjPos < 0) firstTempObjPos = mi;
					items.Remove(item);
				}

				else if (Lex.Eq(item.Text, "New Temporary List...") && firstTempObjPos < 0)
				{ // in case no temp lists
					firstTempObjPos = mi;
					mi++;
				}

				else mi++;
			}

			if (firstTempObjPos < 0) firstTempObjPos = items.Count; // put at end if not defined

			int addCount = 0;
			for (int li = 0; li < SS.I.TempCidLists.Count; li++)
			{ // build list of current temp list items
				TempCidList list = SS.I.TempCidLists[li];

				if (!includeCurrentList && Lex.Eq(list.Name, "Current"))
					continue;

				item = new ToolStripMenuItem();
				item.Text = list.Name;
				item.Tag = list;
				item.Click += eventHandler;
				items.Insert(firstTempObjPos + addCount, item);
				addCount++;
			}

		}

		/// <summary>
		/// Check proper list item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void menuSubset_DropDownOpening(object sender, EventArgs e)
		{
			menuSubsetCurrent.Checked = menuSubsetList.Checked =
			 menuSubsetAll.Checked = false;

			string name = SS.I.DatabaseSubsetListName;
			if (Lex.IsNullOrEmpty(name))
				menuSubsetAll.Checked = true;

			else if (Lex.Eq(name, "Current"))
				menuSubsetCurrent.Checked = true;

			else menuSubsetList.Checked = true;
		}

		private void menuSubsetCurrent_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListSubsetCurrent");
			CommandExec.ExecuteCommandAsynch("Subset Current");
		}

		private void menuSubsetList_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListSubset");
			CommandExec.ExecuteCommandAsynch("Subset List");
		}

		private void menuSubsetAll_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ListSubsetAll");
			CommandExec.ExecuteCommandAsynch("Subset All");
		}

		////////////////////////////////////////////////
		////////////// Favorites Menu //////////////////
		////////////////////////////////////////////////

		/// <summary>
		/// Load favorites menu
		/// </summary>

		public void LoadFavorites()
		{
			Favorites = new List<string>();
			string txt = Preferences.Get("Favorites");
			if (String.IsNullOrEmpty(txt)) return;

			string[] sa = txt.Split('\n');

			foreach (string target in sa)
			{
				if (String.IsNullOrEmpty(target)) continue;
				MetaTreeNode node = GetMetaTreeNode(target);
				if (node == null) continue; // not currently available

				Favorites.Add(target);
				if (SessionManager.Instance.MainMenuControl != null)
					SessionManager.Instance.MainMenuControl.AddToFavoritesMenu(node);
			}

			return;
		}

		//////////////////////////////////////////
		////////////// Mru List //////////////////
		//////////////////////////////////////////

		/// <summary>
		/// Load list of recently accessed user objects
		/// </summary>

		public void LoadMruList()
		{
			MruList = new List<string>();
			string txt = Preferences.Get("MruList");
			if (!String.IsNullOrEmpty(txt))
			{
				string[] sa = txt.Split('\n');
				foreach (string uoName in sa)
				{
					MruList.Add(uoName.Trim());
				}
			}

			BuildMruMenus();
			return;
		}

		/// <summary>
		/// Add an item to the menu
		/// </summary>
		/// <param name="node"></param>

		internal void AddToFavoritesMenu(MetaTreeNode node)
		{
			ToolStripMenuItem mi = new ToolStripMenuItem();
			mi.Image = Bitmaps.Bitmaps16x16.Images[node.GetImageIndex()];
			mi.Text = node.Label;
			mi.ForeColor = NewMenuItem.ForeColor;
			mi.Tag = node;
			mi.MouseDown += new MouseEventHandler(FavoritesMenuItem_MouseDown);
			FavoritesMenu.Items.Add(mi);
		}

		/// <summary>
		/// Add/update an item in the recently accessed UserObject menu (include any MetaTreeNode (e.g. DataTableType) accessed?)
		/// </summary>
		/// <param name="node"></param>

		internal void UpdateMruList(
			string nodeName,
			bool persistList = true)
		{
			int id;

			if (String.IsNullOrEmpty(nodeName)) return;
			if (UserObject.TryParseObjectIdFromInternalName(nodeName, out id)) // get id if user object
			{
				if (id <= 1) return; // ignore if "current" list or other object
			}

			string match = MruList.Find(delegate (string s) // remove if already exists
			{ return Lex.Eq(s, nodeName); });
			if (match != null) MruList.Remove(match);

			MruList.Insert(0, nodeName);

			while (MruList.Count > MaxStoredMruItems)
				MruList.RemoveAt(MaxStoredMruItems);

			BuildMruMenus();

			if (persistList) StoreModifiedMruList();
			return;
		}

		/// <summary>
		/// Remove a user object from the Mru list if currently contained in list
		/// </summary>
		/// <param name="nodeName"></param>

		internal void RemoveFromMruList(
			string nodeName,
			bool persistList = true)
		{
			if (String.IsNullOrEmpty(nodeName)) return;

			string match = MruList.Find(delegate (string s) // remove if already exists
			{ return Lex.Eq(s, nodeName); });
			if (match == null) return;

			MruList.Remove(match);
			BuildMruMenus();

			if (persistList) StoreModifiedMruList();
			return;
		}

		/// <summary>
		/// Store modified Mru list in preferences & menus
		/// </summary>

		internal void StoreModifiedMruList()
		{
			string mruList = "";
			foreach (string s in MruList)
			{
				if (mruList != "") mruList += '\n';
				mruList += s;
			}

			Preferences.Set("MruList", mruList);
			return;
		}

		/// <summary>
		/// Build menus of recently used UserObjects
		/// </summary>

		public void BuildMruMenus()
		{
			MainMenuControl mm = SessionManager.Instance.MainMenuControl;
			if (mm == null) return;

			List<MetaTreeNode> nodes = new List<MetaTreeNode>();
			int li = 0;
			while (li < MruList.Count)
			{
				string nodeName = MruList[li];
				if (String.IsNullOrEmpty(nodeName))
				{
					MruList.RemoveAt(li);
					continue;
				}

				MetaTreeNode node = GetMetaTreeNode(nodeName);
				if (node == null)
					MruList.RemoveAt(li); // remove from list if not in db

				else
				{
					if (node.IsUserObjectType) // user objects only
					{
						nodes.Add(node);
						if (nodes.Count >= MaxDisplayedMruItems) break;
					}
					li++;
				}
			}

			mm.BuildMruMenu(nodes);
			mm.BuildRibbonMruMenu(nodes);
			return;
		}

		/// <summary>
		/// Get the MetaTreeNode associated with a UserObject
		/// </summary>
		/// <param name="nodeName"></param>
		/// <returns></returns>

		public MetaTreeNode GetMetaTreeNode(string uoName)
		{
			MetaTreeNode node = MetaTreeNodeCollection.GetNode(uoName);

			if (node == null) // see if in database even though not in tree
			{
				if (MenuUserObjects == null) // read in objects if don't have already
					ReadMenuUserObjects();

				if (MenuUserObjects.ContainsKey(uoName.ToUpper()))
				{
					UserObject uo = MenuUserObjects[uoName.ToUpper()];
					node = UserObjectTree.BuildNode(uo);
					MetaTree.AddNode(node);
				}
			}

			return node;
		}

		/// <summary>
		/// Read the set of UserObjects associated with the menus.
		/// These objects may not be available when the menus are build
		/// due to the asynch loading of the user objects.
		/// </summary>

		void ReadMenuUserObjects()
		{
			UserObjectType uoType;
			int uoId;

			MenuUserObjects = new Dictionary<string, UserObject>();
			List<int> objIds = new List<int>();
			for (int step = 1; step <= 2; step++)
			{
				List<string> targets = MruList;
				if (step == 2) targets = Favorites;
				foreach (string target in targets)
				{
					if (UserObject.ParseObjectTypeAndIdFromInternalName(target, out uoType, out uoId))
						objIds.Add(uoId);
				}
			}

			if (objIds.Count > 0)
			{
				List<UserObject> uoList = UserObjectDao.ReadMultiple(objIds, false);
				foreach (UserObject uo in uoList)
				{
					MenuUserObjects[uo.InternalName.ToUpper()] = uo;
				}
			}
			return;
		}

		/// <summary>
		/// Build menu of recently used UserObjects within File menu
		/// </summary>
		/// <param name="tsmi"></param>

		public void BuildMruMenu(List<MetaTreeNode> nodes)
		{
			RecentFilesMenu.DropDownItems.Clear();

			for (int ni = 0; ni < nodes.Count; ni++)
			{
				MetaTreeNode node = nodes[ni];
				ToolStripMenuItem mi = new ToolStripMenuItem();
				mi.Image = Bitmaps.Bitmaps16x16.Images[node.GetImageIndex()];
				mi.Text = (ni + 1).ToString() + ". " + node.Label;
				mi.ForeColor = NewMenuItem.ForeColor;
				mi.Tag = node;
				mi.MouseDown += new MouseEventHandler(RecentFilesMenuItem_MouseDown);

				RecentFilesMenu.DropDownItems.Add(mi);
			}

			return;
		}

		private void RecentFilesMenuItem_MouseDown(object sender, MouseEventArgs e)
		{
			SessionManager.LogCommandUsage("MruItemSelected");
			UserObjectMenuItem_MouseDown(sender, e, false);
		}

		/// <summary>
		/// Clicked on MRU item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RecentFilesMenu_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cs, cs2;

			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			MetaTreeNode node = (MetaTreeNode)mi.Tag;
			EditMetaTreeNodeObject(node);
			return;
		}

		/// <summary>
		/// Build menu of recently used UserObjects within ribbon app menu
		/// </summary>

		public void BuildRibbonMruMenu(List<MetaTreeNode> nodes)
		{
			ContextMenuStrip cs, cs2;
			if (RibbonForm == null) return;
			//rc.ForceInitialize();

			if (RibbonMruMenuContainer.Parent != SessionManager.Instance.ShellForm) // make shell our parent
			{
				if (RibbonMruMenuContainer.Parent == this) // remove from main menu
				{
					this.Controls.Remove(RibbonMruMenuContainer);
					RibbonMruMenuContainer.Parent = null;
				}

				//SessionManager.Instance.ShellForm.Controls.Add(RibbonMruMenuContainer); // (causes parts of menu to appear when they shouldn't)
				//RibbonMruMenuContainer.Parent = SessionManager.Instance.ShellForm;

				if (ApplicationMenu == null) return;
				ApplicationMenu.RightPaneControlContainer = RibbonMruMenuContainer;
				RibbonMruMenuContainer.Ribbon = RibbonControl;
			}

			RibbonMruMenuLabels.BeginInit();
			RibbonMruMenuLabels.Controls.Clear();

			for (int ni = nodes.Count - 1; ni >= 0; ni--)
			{ // build in reverse order since docking at top
				MetaTreeNode node = nodes[ni];
				AppMenuFileLabel ml = new AppMenuFileLabel();
				RibbonMruMenuLabels.Controls.Add(ml);
				ml.Tag = node;
				ml.Caption = MenuUnderline((ni + 1).ToString()); // underline the item number
				ml.Text = node.Label;
				ml.ForeColor = NewMenuItem.ForeColor;
				ml.Checked = false;
				ml.AutoHeight = true;
				ml.Dock = DockStyle.Top;

				ml.Glyph = Bitmaps.Bitmaps16x16.Images[node.GetImageIndex()];
				//ml.LabelClick += new EventHandler(RibbonMruMenuLabelClicked);
				ml.MouseDown += new MouseEventHandler(RibbonMruMenuLabel_MouseDown);

				//QbContentsTree ct = new QbContentsTree(); // not needed, also slow
				//QbContentsTreeMenus cm = new QbContentsTreeMenus();
				//ct.ContentsTreeContextMenus = cm;
				//ct.CurrentContentsMetaTreeNode = node; // this is the node to operate on
				//ct.CurrentContentsTreeListNode = null; // tree list node not known
				//cm.QbContentsTree = ct;
			}
			RibbonMruMenuLabels.EndInit();
			return;
		}

		/// <summary>
		/// Handle MouseDown event for a ribbon MRU menu label (i.e. UserObject)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void RibbonMruMenuLabel_MouseDown(object sender, MouseEventArgs e)
		{
			ContextMenuStrip cs, cs2;

			AppMenuFileLabel ml = (AppMenuFileLabel)sender;
			MetaTreeNode node = (MetaTreeNode)ml.Tag;

			SessionManager.LogCommandUsage("FavoritesItemSelected");

			if (e.Button == MouseButtons.Right)
			{
				QbContentsTree ct = new QbContentsTree();
				QbContentsTreeMenus cm = new QbContentsTreeMenus();
				ct.ContentsTreeContextMenus = cm;
				ct.CurrentContentsMetaTreeNode = node; // this is the node to operate on
				ct.CurrentContentsTreeListNode = null; // no assoc TreeListNode
				cm.QbContentsTree = ct;
				cs = cm.GetSingleNodeContextMenu(node); // get the context menu for the associated user object type
				cs.Show(ml.Parent, new Point(ml.Right, ml.Top));
			}

			else
			{
				if (RibbonForm == null || ApplicationMenu == null) return;
				ApplicationMenu.HidePopup();
				RibbonForm.Refresh();

				OpenMetaTreeNodeObject(node); // left-click opens
			}
		}

		/// <summary>
		/// Put amperstands before each character for underlining in menu items
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string MenuUnderline(string s)
		{
			string s2 = "";
			foreach (char c in s)
				s2 += "&" + c;

			return s2;
		}

		/// <summary>
		/// Ribbon Mru item clicked, open the object
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void RibbonMruMenuLabelClicked(object sender, EventArgs e)
		{
			if (RibbonForm == null || RibbonControl == null || ApplicationMenu == null) return;

			ApplicationMenu.HidePopup();
			RibbonForm.Refresh();

			MetaTreeNode node = ((AppMenuFileLabel)sender).Tag as MetaTreeNode;
			EditMetaTreeNodeObject(node);
			return;
		}

		/// <summary>
		/// Setup the Add to Favorites submenu items for selected contents tree items and current query table
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FavoritesMenu_DropDownOpening(object sender, EventArgs e)
		{ 
			Setup_AddContentsItemsToFavoritesMenuItem();
			Setup_AddQueryItemToFavoritesMenuItem();

			AddToFavoritesMenuItem.Enabled = 
				AddContentsItemsToFavoritesMenuItem.Enabled | AddQueryItemToFavoritesMenuItem.Enabled;

			return;
		}

/// <summary>
/// Add any selected contents menu item(s) to the AddToFavorites submenu
/// </summary>

		void Setup_AddContentsItemsToFavoritesMenuItem()
		{
			string currentTableName = "";
			int mii;
			ImageList bitmaps16x16 = Bitmaps.Bitmaps16x16;

			ToolStripMenuItem cmi = AddContentsItemsToFavoritesMenuItem;
			cmi.Visible = false;
			cmi.Enabled = false;

			MetaTreeNode[] selectedNodes = SessionManager.Instance.MainContentsControl.QbContentsTreeCtl.GetCurrentSelectedNodes();
			if (selectedNodes == null || selectedNodes.Length == 0) return;

			else if (selectedNodes.Length == 1)
			{
				MetaTreeNode node = selectedNodes[0];
				cmi.Image = bitmaps16x16.Images[node.GetImageIndex()];
				cmi.Text = node.Label;
				cmi.Tag = node;
				cmi.Visible = true;
				cmi.Enabled = true;
			}

			else
			{
				cmi.Image = bitmaps16x16.Images[(int)Bitmaps16x16Enum.CidList];
				cmi.Text = "Selected Contents Tree Items (" + selectedNodes.Length + ")";
				cmi.Tag = null;
				cmi.Visible = true;
				cmi.Enabled = true;
			}
			return;
		}

		/// <summary>
		/// Add an item from the contents to favorites 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AddContentsItemsToFavorites_Click(object sender, EventArgs e)
		{
			MetaTreeNode[] selectedContentsNodes = SessionManager.Instance.MainContentsControl.QbContentsTreeCtl.GetCurrentSelectedNodes();
			foreach (MetaTreeNode node in selectedContentsNodes)
				AddToFavorites(node);
		}

		/// <summary>
		/// Setup to add any current table in the QueryBuilder
		/// </summary>

		void Setup_AddQueryItemToFavoritesMenuItem()
		{
			string currentTableName = "";
			int mii;

			ImageList bitmaps16x16 = Bitmaps.Bitmaps16x16;

			QueryTable currentQt = SessionManager.Instance.QueryTablesControl.CurrentQt;
			if (currentQt != null) currentTableName = currentQt.MetaTable.Name;

			ToolStripMenuItem qmi = AddQueryItemToFavoritesMenuItem;
			qmi.Visible = false; // setup to add current table
			qmi.Enabled = false;
			if (!String.IsNullOrEmpty(currentTableName))
			{
				for (mii = 2; mii < FavoritesMenu.Items.Count; mii++)
				{ // see if already have this item
					MetaTreeNode node2 = (MetaTreeNode)FavoritesMenu.Items[mii].Tag;
					if (node2.Target.EndsWith(" " + currentTableName, StringComparison.OrdinalIgnoreCase))
						break;
				}

				MetaTreeNode mtn = new MetaTreeNode(); // make a temp metatree node to refer to

				if (mii >= FavoritesMenu.Items.Count)
				{
					if (Lex.StartsWith(currentTableName, "Annotation_"))
					{
						qmi.Image = bitmaps16x16.Images[(int)Bitmaps16x16Enum.AnnotationTable];
						mtn.Type = MetaTreeNodeType.Annotation;
					}

					else if (Lex.StartsWith(currentTableName, "CalcField_"))
					{
						qmi.Image = bitmaps16x16.Images[(int)Bitmaps16x16Enum.CalcField];
						mtn.Type = MetaTreeNodeType.CalcField;
					}

					else
					{
						qmi.Image = bitmaps16x16.Images[(int)Bitmaps16x16Enum.Table];
						mtn.Type = MetaTreeNodeType.MetaTable;
					}

					mtn.Name = mtn.Target = currentTableName;
					mtn.Label = currentQt.ActiveLabel;

					qmi.Text = currentQt.ActiveLabel;
					qmi.Tag = mtn;
					qmi.Visible = true;
					qmi.Enabled = true;
				}
			}
		}

		/// <summary>
		/// Add the current table in the current query to favorites
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AddQueryItemToFavorites_Click(object sender, EventArgs e)
		{
			AddToFavorites(AddQueryItemToFavoritesMenuItem.Tag as MetaTreeNode);
		}

		/// <summary>
		/// Clicked on a favorites item. Show appropriate dropdown if right click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FavoritesMenuItem_MouseDown(object sender, MouseEventArgs e)
		{
			SessionManager.LogCommandUsage("FavoritesItemSelected");
			UserObjectMenuItem_MouseDown(sender, e, true);
		}

		/// <summary>
		/// Handle MouseDown on a UserObject menu item from Favorites or the MRU list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <param name="includeDeleteItem"></param>

		private void UserObjectMenuItem_MouseDown(object sender, MouseEventArgs e, bool includeDeleteItem)
		{
			ContextMenuStrip cs, cs2;

			ToolStripMenuItem mi = CurrentUserObjectMenuItem = (ToolStripMenuItem)sender;
			MetaTreeNode node = (MetaTreeNode)mi.Tag;


			if (e.Button == MouseButtons.Right)
			{
				if (mi.DropDownItems.Count == 0) // need to create?
				{
					QbContentsTree ct = new QbContentsTree();
					QbContentsTreeMenus cm = new QbContentsTreeMenus();
					ct.ContentsTreeContextMenus = cm;
					ct.CurrentContentsMetaTreeNode = node; // this is the node to operate on
					ct.CurrentContentsTreeListNode = null; // no assoc TreeListNode
					cm.QbContentsTree = ct;

					if (node.IsFolderType) cs = cm.FavoritesFolderContextMenu; // simple open & delete menu

					else
					{
						cs = cm.GetSingleNodeContextMenu(node); // regular leaf context menu
						if (includeDeleteItem)
						{
							for (int mii = 0; mii < cs.Items.Count; mii++)
							{ // replace Add to Favorites with delete & remove remaining items

								if (Lex.Ne(cs.Items[mii].Text, "Add to Favorites")) continue;

								while (cs.Items.Count > mii) cs.Items.RemoveAt(mii); // remove rest

								cs2 = cm.FavoritesFolderContextMenu;
								cs.Items.Add(cs2.Items[2]); // add delete item
								break;
							}
						}
					}
					mi.DropDown = cs;
				}
				mi.ShowDropDown();
			}

			else OpenMetaTreeNodeObject(node); // left-click opens
		}

		/// <summary>
		/// Add an item to the persisted list of favorites and the menu
		/// </summary>
		/// <param name="node"></param>

		internal void AddToFavorites(MetaTreeNode node)
		{
			int i1;

			if (node == null || Lex.IsNullOrEmpty(node.Target)) return;

			string favs = "";
			for (i1 = 0; i1 < Favorites.Count; i1++)
			{ // see if already exists & build new preferences list
				if (Lex.Eq(Favorites[i1], node.Target)) break;
				if (favs.Length > 0) favs += "\n";
				favs += Favorites[i1];
			}

			if (i1 < Favorites.Count) return; // already have it

			Favorites.Add(node.Name);
			if (favs.Length > 0) favs += "\n";
			favs += node.Name; // add new one
			Preferences.Set("Favorites", favs);

			AddToFavoritesMenu(node);
		}

		/// <summary>
		/// Remove an item from the persisted list of favorites and the menu
		/// </summary>
		/// <param name="node"></param>

		internal void DeleteFavorite(MetaTreeNode node)
		{
			string favs = "";
			int i1 = 0;
			while (i1 < Favorites.Count)
			{
				if (Lex.Eq(Favorites[i1], node.Name))
					Favorites.RemoveAt(i1);

				else
				{
					if (favs.Length > 0) favs += "\n";
					favs += Favorites[i1];
					i1++;
				}
			}

			Preferences.Set("Favorites", favs);

			if (CurrentUserObjectMenuItem != null)
				FavoritesMenu.Items.Remove(CurrentUserObjectMenuItem);
		}

		/// <summary>
		/// Open the object associated with the specified node
		/// </summary>

		internal static void OpenMetaTreeNodeObject(MetaTreeNode node)
		{
			if (MetaTreeNode.IsFolderNodeType(node.Type))
			{ // just select and expand the node if folder
				ContentsTreeControl treeCtl = SessionManager.Instance.MainContentsControl.QbContentsTreeCtl;
				if (treeCtl.FindNodeByTarget(node.Target) == null) // may not be in tree if looking at subset
				{
					MessageBoxMx.Show("Could not find the folder \"" + node.Label + "\" in the current contents tree view");
					return;
				}

				treeCtl.ExpandNode(node.Target);
				treeCtl.SelectNode(node.Target);
				return;
			}

			else CommandExec.ExecuteCommandAsynch("ContentsDoubleClick " + node.Type + " " + node.Target); // execute default command
		}

		/// <summary>
		/// Open the associated node UserObject for editing
		/// </summary>
		/// <param name="node"></param>

		internal static void EditMetaTreeNodeObject(MetaTreeNode node)
		{
			if (node.Type == MetaTreeNodeType.Query)
				QbUtil.OpenQuery(node.Target);

			else if (node.Type == MetaTreeNodeType.CnList)
				CidListEditor.Edit(node.Target);

			else if (node.Type == MetaTreeNodeType.Annotation)
				UserDataEditor.Edit(node.Target);

			else if (node.Type == MetaTreeNodeType.CalcField)
				CalcFieldEditor.Edit(node.Target);

			else if (node.Type == MetaTreeNodeType.CondFormat)
				CondFormatEditor.EditUserObject(node.Target);

			else if (Lex.StartsWith(node.Target, "USERDATABASE_"))
				UserDataEditor.Edit(node.Target);

			return;
		}

		////////////////////////////////////////////////
		///////////////// Tools Menu ////////////////////
		////////////////////////////////////////////////

		/// <summary>
		/// Set visibility/enable state for menu items
		/// </summary>

		public void EnableToolsMenuItems()
		{
			AdminCommandsMenuItem.Visible = SS.I.IsAdmin;

			bool canEditContentsTree = SS.I.UserInfo.Privileges.HasPrivilege("ContentsTreeEditor") || SS.I.UserInfo.Privileges.HasPrivilege("ProjectEditor");
			ContentsTreeEditorMenuItem.Visible = canEditContentsTree;
			return;
		}

		/// <summary>
		/// Add a menu item to the tools menu
		/// </summary>
		/// <param name="itemLabel"></param>
		/// <param name="command"></param>

		public void AddToolsMenuPluginItem(
			string itemLabel,
			string command,
			Image image,
			bool visible,
			bool enabled)
		{
			ToolStripItemCollection mic = ToolsMenu.Items;
			int mii;
			for (mii = 0; mii < mic.Count; mii++) // position in proper place
			{
				if (Lex.Eq(itemLabel, mic[mii].Text)) return; // already have
				if (mic[mii] is ToolStripSeparator) break;
			}

			ToolStripMenuItem nmi = new ToolStripMenuItem(itemLabel, null, new System.EventHandler(ToolsMenuPluginItem_Click));
			nmi.ForeColor = mic[0].ForeColor; // set proper foreColor
			nmi.Tag = command;
			if (image != null) nmi.Image = image;
			else nmi.Image = global::Mobius.ClientComponents.Properties.Resources.Tools;
			nmi.ImageTransparentColor = System.Drawing.Color.Cyan;
			nmi.Visible = visible;
			nmi.Enabled = enabled;
			mic.Insert(mii, nmi);
			return;
		}

		/// <summary>
		/// Handler for clicks on Tools menu plugin items
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void ToolsMenuPluginItem_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			string command = mi.Tag as string;

			CallPlugin(command);
		}

		/// <summary>
		/// Call plugin
		/// </summary>
		/// <param name="command">PluginCommand Mobius.Tools.toolName</param>

		public void CallPlugin(string command)
		{
			string response;

			string commandName = "PluginCommand";
			if (String.IsNullOrEmpty(command) || !Lex.StartsWith(command, commandName)) return;

			string extensionId = command.Substring(commandName.Length + 1).Trim();
			try
			{
				response = Plugins.CallExtensionPointRunMethod(extensionId, null);
				if (Lex.StartsWith(response, "Command "))
				{
					string nextCommand = response.Substring(8); // extract real command
					CommandExec.ExecuteCommandAsynch(nextCommand);
				}
			}
			catch (Exception ex)
			{
				//if (ex.InnerException != null) ex = ex.InnerException;
				response = "Exception in Mobius plugin run method for extension id: " + extensionId + "\r\n" + DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(response);
				MessageBoxMx.ShowError(response);
			}

			return;
		}

		private void AlertsMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ToolsAlerts");

			CommandExec.ExecuteCommandAsynch("CommandLine ShowAlerts"); // ShowAlerts ia a commandline command
		}

		private void AdminCommandsMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			CommandLine.BuildDropDownMenu(AdminCommandsMenuItem.DropDownItems);
		}

		private void PreferencesMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("ToolsPreferences");

			PreferencesDialog.Edit();
		}

		////////////////////////////////////////////////
		///////////////// Help Menu ////////////////////
		////////////////////////////////////////////////

		private void MobiusHelpMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("Help");

			string fileName = ServicesIniFile.Read("HelpFile");
			if (fileName == "") return;
			SystemUtil.StartProcess(fileName);
		}

		private void TrainingVideosMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("HelpTrainingVideos");

			string fileName = ServicesIniFile.Read("VideosDirectory");
			if (fileName == "") return;
			SystemUtil.StartProcess(fileName);
		}

		private void MobiusNewsMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("HelpNews");

			NewsDialog.ShowNews();
		}

		private void MobiusSupportMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("HelpSupport");

			string fileName = ServicesIniFile.Read("SupportFile");
			if (fileName == "") return;
			SystemUtil.StartProcess(fileName);
		}

		private void AboutMobiusMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("HelpAbout");

			AboutDialog.Show();
		}

		////////////////////////////////////////////////
		//////////////// Misc methods //////////////////
		////////////////////////////////////////////////

#if false // obsolete MainMenu adjustments
		public void SetMatchingBackgroundColor() 
		{
			Point p = new Point(Left - 2, Top + (Bottom - Top) / 2); // Left - 2 for non Windows 7, Left + 4 for Windows 7
			Point screenPos = this.Parent.PointToScreen(p);
			Bitmap bmp = new Bitmap(1, 1);
			Graphics g = Graphics.FromImage(bmp);
			g.CopyFromScreen(screenPos, new Point(0, 0), new Size(1, 1));
			Color c = bmp.GetPixel(0, 0);
			BackColor = c; // set user control background
			MainMenu.BackColor = c; // set menu background
			return;
		}

		/// <summary>
		/// Set the color for the main menu font
		/// </summary>
		/// <param name="fontColor"></param>

		public void SetFontColor(Color fontColor)
		{
			foreach (ToolStripItem tsi in MainMenu.Items)
			{
				SetFontColor(tsi, fontColor);
			}
		}

		/// <summary>
		/// Recursively set font colors for submenus
		/// </summary>
		/// <param name="tsi"></param>
		/// <param name="fontColor"></param>

		public void SetFontColor(ToolStripItem tsi, Color fontColor)
		{
			tsi.ForeColor = fontColor;
			ToolStripMenuItem tsmi = tsi as ToolStripMenuItem;
			if (tsmi == null) return;
			if (fontColor == Color.White) return; // don't set lower levels to white because they disappear

			foreach (ToolStripItem tsi2 in tsmi.DropDownItems)
			{
				SetFontColor(tsi2, fontColor);
			}

		}
#endif

		/// <summary>
		/// Show query history list for this session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void HistoryMenu_Click(object sender, EventArgs e)
		{
			int x = HistoryMenu.Bounds.X;
			int y = HistoryMenu.Bounds.Bottom; //.Height;

			Point p = PointToScreen(new Point(x, y - 2));
			HistoryList.Show(p);
			return;
		}

		public void ShowHistoryList(int x, int y)
		{
// remove
		}

		/// <summary>
		/// Status bar menu item command to edit current list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void EditCurrentListButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("List EditTemp Current");
		}

		/// <summary>
		/// Status bar menu item command to save current list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SaveCurrentListButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("List SaveCurrent");
		}

		private void MainMenu_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			return;
		}

		private void MainMenu_Enter(object sender, EventArgs e)
		{
			return;
		}

		private void MainMenu_Leave(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Status bar menu items to change database subset
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CurrentListCheckItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			SessionManager.LogCommandUsage("ListSubsetCurrent");
			CommandExec.ExecuteCommandAsynch("Subset Current");
		}

		private void OtherListCheckItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			SessionManager.LogCommandUsage("ListSubset");
			CommandExec.ExecuteCommandAsynch("Subset List");
		}

		private void SubsetAllCheckItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			SessionManager.LogCommandUsage("ListSubsetAll");
			CommandExec.ExecuteCommandAsynch("Subset All");
		}

		/// <summary>
		/// Execute a pending command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			Timer.Enabled = false;

			if (PendingMainMenuItem == null) return;

			ToolStripMenuItem tsmi = PendingMainMenuItem;
			PendingMainMenuItem = null;
			tsmi.PerformClick(); // simulate click of a menu item
			return;
		}

		/// <summary>
		/// Edit database contents tree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ContentsEditorMenuItem_Click(object sender, EventArgs e)
		{
			bool edited = CommandExec.EditContentsTree("");
			return;
		}

		private void EditDeleteMenu_MouseEnter(object sender, EventArgs e)
		{
			if (AfterEditMenuSelected != null) AfterEditMenuSelected();
		}

		/// <summary>
		/// Based on some user interaction, enable or disable menu items based on the MetaNodes passed.
		/// </summary>
		/// <param name="MetaTreeNodeArray"></param>
		public void EnableMenuItems(MetaTreeNode[] MetaTreeNodeArray)
		{
			// if nothing selected then disable all menus and return.
			if (MetaTreeNodeArray == null)
			{
				EditCutMenuItem.Enabled =
				EditCopyMenuItem.Enabled =
				EditDeleteMenuItem.Enabled =
				EditPasteMenuItem.Enabled = false;
				return;
			}

			// Loop through menu items until you find one that the user does not have permission for.  If that happens
			// then set "enabled" to false and stop reading the rest. No point.
			bool enabled = false;
			foreach (MetaTreeNode mtn in MetaTreeNodeArray)
			{
				enabled = Mobius.Data.Permissions.UserHasWriteAccess(SS.I.UserName, mtn.Target);
				if (mtn.Parent == null) enabled = false; // root node 
				if (!enabled) break;
			}
			EditCutMenuItem.Enabled = enabled;
			EditCopyMenuItem.Enabled = enabled;
			EditDeleteMenuItem.Enabled = enabled;
			EditPasteMenuItem.Enabled = enabled;
			Enabled = true;
		}

		private void FileMenu_DropDownOpening(object sender, EventArgs e)
		{
			bool visibility = SS.I.UserInfo.Privileges.HasPrivilege("Administrator");

			FileNewConditionalFormat.Visible = visibility;
			FileOpenCondFormat.Visible = visibility;

			FileNewSpotfireLinkMenuItem.Visible = false; // disabled for now
			FileEditSpotfireLinkMenuItem.Visible = false;
		}

		private void MobiusContactUsMenuItem_Click(object sender, EventArgs e)
		{
			const string command = "mailto:mobius_admin?subject=Question / Comment from User";
			Process.Start(command);
		}

	}

}
