using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics;

// All DevExpress assemblies should be referenced here and in the Client project References to avoid having 
// them added to the final build directory for each build

using DevExpress.Skins;
using DevExpress.Data;
using DevExpress.Office;
using DevExpress.PivotGrid;
using DevExpress.Printing;
using DevExpress.RichEdit;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Ribbon.ViewInfo;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraLayout;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraPrinting;
using DevExpress.XtraRichEdit;
using DevExpress.XtraTreeList;

namespace Mobius.Client
{
	public partial class Shell : DevExpress.XtraBars.Ribbon.RibbonForm
	{

		public MainMenuControl MainMenuControl;
		Keys PrevKeyDown = Keys.None, LastKeyDown = Keys.None;
		int PaintCount = 0; 
		public static bool Debug = false;

		public Shell()
		{
			InitializeComponent();
			RibbonControl.Manager.ShortcutItemClick += new ShortcutItemClickEventHandler(Manager_ShortcutItemClick);
			MainMenuControl = new MainMenuControl();
			MainMenuControl.AfterEditMenuSelected += AfterEditMenuSelected;
			LinkContextMenusToMenuBarItems();
		}

		private void LinkContextMenusToMenuBarItems()
		{
			MainMenuControl.FileMenu.Tag = FileBarItem;
			MainMenuControl.EditMenu.Tag = EditBarItem;
			MainMenuControl.QueryMenu.Tag = QueryBarItem;
			MainMenuControl.ListMenu.Tag = ListBarItem;
			MainMenuControl.FavoritesMenu.Tag = FavoritesBarItem;
			MainMenuControl.HistoryMenu.Tag = HistoryBarItem;
			MainMenuControl.ToolsMenu.Tag = ToolsBarItem;
			MainMenuControl.HelpMenu.Tag = HelpBarItem;
			return;
		}

		/// <summary>
		/// When the EditMenu is selected, get the current metaNodes and pass them to the MainMenu to determine if menus
		/// should be enabled or disabled.
		/// </summary>
		private void AfterEditMenuSelected()
		{
			MetaTreeNode[] mtns = QueriesControl.QueryBuilderControl.QbContentsCtl.QbContentsTreeCtl.GetCurrentSelectedNodes();
			MainMenuControl.EnableMenuItems(mtns);
		}

		void Manager_ShortcutItemClick(object sender, ShortcutItemClickEventArgs e)
		{
			return;
		}

		// New menu

		private void AppNewMenu_BeforePopup(object sender, CancelEventArgs e)
		{
			bool v = SS.I.UserInfo.Privileges.HasPrivilege("Administrator");
			BarItemVisibility biv = v ? BarItemVisibility.Always : BarItemVisibility.Never;

			NewSpotfireLinkButtonItem.Visibility = BarItemVisibility.Never; // disable for now (was: biv)
		}

		private void NewQueryButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("NewQuery");
		}

		private void NewListButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("List New");
		}

		private void NewAnnotationTableMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("NewAnnotation");
		}

		private void NewCalculatedFieldButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("NewCalcField");
		}

		private void NewSpotfireLinkButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("NewSpotfireLink");
		}

		private void NewUserDatabaseButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("NewUserDatabase");
		}

		// Open menu

		private void AppOpenMenu_BeforePopup(object sender, CancelEventArgs e)
		{
			bool v = SS.I.UserInfo.Privileges.HasPrivilege("Administrator");
			BarItemVisibility biv = v ? BarItemVisibility.Always : BarItemVisibility.Never;

			OpenSpotfireLinkButtonItem.Visibility = BarItemVisibility.Never; // disable for now (was: biv)
		}

		private void OpenQueryButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("OpenQuery");
		}

		private void OpenListButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("List EditSaved");
		}

		private void OpenAnnotationTableButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("OpenAnnotation");
		}

		private void OpenCalculatedFieldButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("OpenCalcField");
		}

		private void OpenSpotfireLinkButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("EditSpotfireLink");
		}

		private void OpenUserDatabaseButtonItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("OpenUserDatabase");
		}

		// Other top level menu items

		private void SaveAppMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("FileSave");
		}

		private void SaveAsAppMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("FileSaveAs");
		}

		private void PrintAppMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("Print");
		}

		private void CloseAppMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("FileClose");
		}

		private void CloseAllAppMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("FileCloseAll");
		}

		private void ExitAppMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ExecuteCommand("Exit");
		}

		private void AppMenuExitButton_Click(object sender, EventArgs e)
		{
			MainMenuControl.ExecuteCommand("Exit");
		}

		private void Shell_Activated(object sender, EventArgs e)
		{
			//ClientLog.Message("Shell Activated");

			SessionManager.AdjustMainMenuPosition();

			if (Progress.Instance != null && Progress.Instance.Visible)
			{ // if progress dialog showing then treat as modal by keeping focus there and not responding to clicks on Shell controls 
				// (todo: replace this hack with better code in Progress)
				Progress.Instance.Focus();
				Application.DoEvents();
			}
		}

		/// <summary>
		/// Execute a QuickSearch for the entered text
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void QuickSearchGoButton_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (CommandLine.EditValue == null) return;
			string commandLine = CommandLine.EditValue.ToString();
			if (String.IsNullOrEmpty(commandLine)) return;
			Program.SessionManager.ExecuteCommand("CommandLine " + commandLine);
			return;
		}

		/// <summary>
		/// Perform a quick structure search Including existing criteria
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void QuickStructureSearchInclusiveMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			Program.SessionManager.ExecuteCommand("QuickStructureSearchInclusive");
			return;
		}

		/// <summary>
		/// Perform a quick structure search Excluding existing criteria
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void QuickStructureSearchExclusiveMenuItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			Program.SessionManager.ExecuteCommand("QuickStructureSearchExclusive");
			return;
		}

		/// <summary>
		/// Form is closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Shell_FormClosing(object sender, FormClosingEventArgs e)
		{
			Form saveShellForm = null;
			SessionManager sm = SessionManager.Instance;
			if (sm != null)
			{
				saveShellForm = sm.ShellForm;
				sm.ShellForm = null; // clear reference to us since no longer valid when form closing
			}

			bool success = Program.SessionManager.ShutDown(); // shut things down
			if (!success) // shutdown cancelled by user
			{
				e.Cancel = true; // cancel form closing
				if (sm != null) sm.ShellForm = saveShellForm; // restore ref to ShellForm
			}
		}

		private void RibbonControl_ApplicationButtonClick(object sender, EventArgs e)
		{
			return;
		}

		private void Shell_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			Keys keyData = e.KeyData;

			if (Debug) DebugLog.Message("KeyData: " + keyData);

			return;
		}

		private void RibbonControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			Keys keyData = e.KeyData;

			if (e.KeyCode != Keys.F) return;
			object o = RibbonControl.Manager.Items; // [0].PerformClick();

			if (Debug) DebugLog.Message("KeyData: " + keyData);

			return;
			//MouseEventArgs mea = new MouseEventArgs(MouseButtons.Left, 1, 40, 40, 0);
			//OnMouseClick(mea);
			//AppMenu.ShowPopup(new Point(0, 0));
		}

		private void Shell_Paint(object sender, PaintEventArgs e)
		{
			PaintCount++;

			return;
		}

		private void Shell_KeyDown(object sender, KeyEventArgs e)
		{
			return;
		}

		private void Shell_KeyPress(object sender, KeyPressEventArgs e)
		{
			return;
		}

		private void Shell_KeyUp(object sender, KeyEventArgs e)
		{
			return;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (Debug) DebugLog.Message("KeyData: " + keyData);

			if (MainMenuControl == null) return false; // no main menu

			if (keyData == (Keys.Menu | Keys.Alt)) // menu key?
				{
					PrevKeyDown = keyData; 
					return true; // say processed
				}

			if (PrevKeyDown == (Keys.Menu | Keys.Alt)) // previous key menu key?
			{
				PrevKeyDown = keyData;
				if (MainMenuControl.CheckMenuActivationKey(keyData)) // activating a menu?
					return true;
			}

			PrevKeyDown = keyData;

			if (MainMenuControl.CheckShortcuts(keyData)) // see if menu shortcut
				return true; // was a shortcut

			else return base.ProcessCmdKey(ref msg, keyData); // let base class handle
		}

		private void Shell_Resize(object sender, EventArgs e)
		{
			SessionManager.AdjustMainMenuPosition();

			if (WindowState == FormWindowState.Maximized ||
				WindowState == FormWindowState.Normal)
			{ // save window state and size if normal or maximized
				string state =
					((int)WindowState).ToString() + "," +
					Left.ToString() + "," +
					Top.ToString() + "," +
					Width.ToString() + "," +
					Height.ToString();

				if (SessionManager.Instance != null)
					SessionManager.Instance.ShellFormSize = state;
			}

		}

		private void FileBarItem_ItemClick(object sender, ItemClickEventArgs e)
		{
			return;
		}

		private void FileBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.FileMenu);
		}

		private void EditBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.EditMenu);
		}

		private void QueryBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.QueryMenu);
		}

		private void ListBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.ListMenu);
		}

		private void FavoritesBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.FavoritesMenu);
		}

		private void HistoryBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			Rectangle r = HistoryBarItem.Links[0].ScreenBounds;
			//Point p = PointToScreen(new Point(r.Left + 8, r.Bottom));
			Point p = new Point(r.Left + 0, r.Bottom);
			HistoryList.Show(p);
			return;
		}

		private void ToolsBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.ToolsMenu);
		}

		private void ToolTipController_GetActiveObjectInfo(object sender, ToolTipControllerGetActiveObjectInfoEventArgs e)
		{
				if (e.SelectedControl == RibbonControl)
				{
					RibbonHitInfo hitInfo = RibbonControl.CalcHitInfo(e.ControlMousePosition);
					if (hitInfo.InItem && hitInfo.Item is BarStaticItemLink)
						e.Info = null; // don't show tooltip that is the same text as the BarStaticItemLlink
				}
		}

		private void HelpBarItem_ItemPress(object sender, ItemClickEventArgs e)
		{
			MainMenuControl.ShowContextMenu(MainMenuControl.HelpMenu);
		}

	}
}
