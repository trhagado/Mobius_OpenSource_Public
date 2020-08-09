using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Mobius.ClientComponents
{
	public partial class QuickSearchPopup : DevExpress.XtraEditors.XtraUserControl
	{
		string Cid = ""; // current compound id
		MetaTable CidMt = null; // metatable associated with current cid
		MoleculeMx Molecule = null; // structure of current cid

		MRUEdit CommandLineControl; // command line control 
		DateTime LastCommandLineControlKeyDownTime = DateTime.MinValue;
		string PreviousInput = "";
		Control LastFocusedControl = null;

		ContentsTreeControl ContentsTree; // the contents tree we are searching

		ContextMenuStrip ActiveContextMenu;
		int RenderId = 0; // Id of current structure render
		List<MetaTreeNode> MatchingContentsNodes; // current set of matching contents tree nodes
		RelatedStructureControlManager RSM;

		int 
			InitialStructurePanelWidth = -1, // initial StructurePanel sizes 
			InitialStructurePanelWidthWithoutRelatedStructureOptions = -1,
			InitialStructurePanelHeight = -1;

		bool Initialized = false;

		static bool Debug = true;

		/// <summary>
		/// Enable popup of matching structures
		/// </summary>

		public bool StructurePopupEnabled
		{
			get { return _structurePopupEnabled; }
			set { _structurePopupEnabled = value; }
		}
		public bool _structurePopupEnabled = true;

		/// <summary>
		/// Enable popup of matching database contents
		/// </summary>

		public bool ContentsPopupEnabled
		{
			get { return _contentsPopupEnabled; }
			set { _contentsPopupEnabled = value; }
		}
		public bool _contentsPopupEnabled = true;

		public bool ConsiderShellFocus = true;

		/// <summary>
		/// Default constructor
		/// </summary>

		public QuickSearchPopup()
		{
			InitializeComponent();

			WinFormsUtil.LogControlChildren(this);

			if (SystemUtil.InRuntimeMode)
				RelatedStructuresControl.BorderStyle = BorderStyle.None;

			return;
		}

		/// <summary>
		/// Initialize with associated command line control
		/// </summary>
		/// <param name="commandLineControl"></param>

		public static QuickSearchPopup Initialize(
			MRUEdit commandLineControl, 
			Form parentForm,
			ContentsTreeControl contentstree)
		{
			QuickSearchPopup qsp = null;

			AssertMx.IsNotNull(commandLineControl, "CommandLineControl");
			AssertMx.IsNotNull(parentForm, "ParentForm");

			foreach (Control c in parentForm.Controls) // see if already exists
			{
				if (c is QuickSearchPopup)
				{
					qsp = c as QuickSearchPopup;
					break;
				}
			}

			bool newPopup = false;
			if (qsp == null) // need to create new popup in parent form
			{
				qsp = new QuickSearchPopup();
				parentForm.Controls.Add(qsp);
				newPopup = true;
			}

			qsp.Initialize2(commandLineControl, parentForm, contentstree, newPopup);

			return qsp;
		}

		void Initialize2(
			MRUEdit clc, 
			Form parentForm,
			ContentsTreeControl contentsTree,
			bool newPopup)
		{
			QuickSearchPopup qsp = this; // debug

			//if (newPopup) // just created?
			if (!Initialized)
			{
				Visible = false;
				//Width = 236; // set proper size
				//Height = 186;

				Point p = clc.PointToScreen(new Point(0, clc.Bottom));
				Point p2 = parentForm.PointToClient(p);
				Left = p.X;  // position below input control
				Top = p.Y + clc.Height * 2;

				BringToFront();

				ContentsTree = contentsTree;

				RelatedStructuresControl rsp = RelatedStructuresControl;

				InitialStructurePanelWidth = StructurePanel.Width; //rsp.Width + 12; // initial StructurePanel width
				InitialStructurePanelWidthWithoutRelatedStructureOptions = rsp.MoleculeControl.Right + 10;

				//InitialStructurePanelWidth = 400; // debug

				InitialStructurePanelHeight = rsp.Top + rsp.MoleculeControl.Height + 15; // initial StructurePanel height to just below structure box
				StructurePanel.Dock = DockStyle.Fill;

				OtherCidsList.Visible = false;

				MoleculeControl strBox = RelatedStructuresControl.MoleculeControl; // make structure box wider to the left
				int newBoxLeft = 4;
				int dx = strBox.Left - newBoxLeft; // amount to widen structure box
				int w2 = strBox.Width + dx;
				int h2 = strBox.Height;

				RSM = new RelatedStructureControlManager();
				RelatedStructuresControl.RSM = RSM;
				RSM.RSC = RelatedStructuresControl;
				RSM.HideUndefinedStructureList = true; // don't show structure panel until after search complete

				RelatedStructuresControl.RenderSearchResultsCompleteCallBack = RenderSearchResultsCompleteCallBack;

				RelatedStructuresControl.SetupCheckmarks();

				Initialized = true;
			}

			CommandLineControl = clc; // associated command line comtrol
			clc.GotFocus += CommandLine_GotFocus;
			clc.KeyDown += CommandLine_KeyDown; // add command line events for us
			clc.KeyUp += CommandLine_KeyUp;
			clc.PreviewKeyDown += CommandLine_PreviewKeyDown;

			if (!Timer.Enabled)
			{
				Timer.Enabled = true;
				PreviousInput = ""; // need to clear to get redisplay of structure
			}

			return;
		}

		public void RenderSearchResultsCompleteCallBack()
		{
			int h = Parent.Height - Top - 20; // height from top of popup to bottom of containing form minus a bit
			if (Height != h)
				Height = h;
			return;
		}

		private void CommandLine_GotFocus(object sender, EventArgs e)
		{
			Timer.Enabled = true; // be sure timer is running so we can respond to entered text
			return;
		}

		/// <summary>
		/// Handle special KeyDown events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CommandLine_KeyDown(object sender, KeyEventArgs e)
		{
			LastCommandLineControlKeyDownTime = DateTime.Now;

			if (CheckForEscapePressed(e)) { }

			else if (e.KeyCode == Keys.Enter) // command entered
			{
				e.Handled = true;

				ExecuteCommandLine();
				return;

#if false // experimental code that opens the currently selected item in the dropdown rather than doing a more-complete search
				int i1 = ListControl.SelectedIndex;

				if (ListControl.Visible &&
					i1 >= 0 && // something selected
					i1 < MatchingContentsNodes.Count) // within list of nodes found in the latest quicksearch
				{
					MetaTreeNode mtn = MatchingContentsNodes[i1];
					QbUtil.CallCurrentProcessTreeItemOperationMethod("Open", mtn.Target);
					HideQuickSearchPopup();
				}

				else ExecuteCommandLine(); // just execute the command line

				return;
#endif

			}
		}

		public void ExecuteCommandLine()
		{
			string s = CommandLineControl.Text;
			if (s == "") return;

			HideQuickSearchPopup();

			DevExpress.XtraEditors.Controls.MRUEditItemCollection items = CommandLineControl.Properties.Items;
			if (items.Count > 0 && Lex.Eq(items[0].ToString(), s)) { } // don't insert same item again
			else CommandLineControl.Properties.Items.Insert(0, s); // insert in list manually since we clear it here
			CommandLineControl.Text = "";
			PreviousInput = ""; // avoid any additional QuickDisplays

			QbUtil.CallCurrentProcessTreeItemOperationMethod("CommandLine", s);
			//CommandExec.Execute("CommandLine " + s);
		}

		/// <summary>
		/// HideQuickSearchPopup
		/// </summary>

		public void HideQuickSearchPopup()
		{
			Visible = false; // hide the popup
			RelatedStructuresControl.PreserveRelatedCheckmarkPreferences();
			PreviousInput = ""; // avoid any additional QuickDisplays

			return;
		}

		/// <summary>
		/// Handle special KeyUp events (disabled)
		/// Ctrl+C, Ctrl+X & Ctrl+V only pass through on KeyUp
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CommandLine_KeyUp(object sender, KeyEventArgs e)
		{
			return;
		}

		/// <summary>
		/// CommandLine PreviewKeyDown event 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CommandLine_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Update display if input has changed, or hide display if focus has been lost 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void QuickSearchPopupTimer_Tick(object sender, EventArgs e)
		{
			Point mousePosition = Point.Empty, ul = Point.Empty;
			Rectangle rt = Rectangle.Empty;

			Timer.Enabled = false;
			//SystemUtil.Beep(); // debug

			//SystemUtil.Beep();

			SessionManager sm = SessionManager.Instance;
			Form shell = sm.ShellForm;
			bool shellContainsFocus = shell != null ? shell.ContainsFocus : false;
			bool quickSearchPopupVisible = this.Visible;
			bool quickSearchPopupContainsFocus = this.ContainsFocus;
			bool ribbonCtlContainsFocus = sm.RibbonCtl.ContainsFocus;
			bool gridCtlContainsFocus = sm.RibbonCtl.ContainsFocus;
			bool commandLineControlContainsFocus = CommandLineControl.ContainsFocus; // capture values for debugging
			bool listControlcontainsFocus = ListControl.ContainsFocus;
			bool relatedStructuresPanelContainsFocus = RelatedStructuresControl.ContainsFocus;

			SessionManager.AdjustMainFormControlPositionsAsNeeded();

			Control focusedCtl = UIMisc.GetFocusedControl();
			Form af = Form.ActiveForm;
			Control ac = af != null ? af.ActiveControl : null;
			MouseButtons mouseButtons = MouseButtons;
			bool activeContextMenuVisible = (ActiveContextMenu != null) ? ActiveContextMenu.Visible : false;

			if (focusedCtl != LastFocusedControl)
			{
				LastFocusedControl = focusedCtl;

				//string fch = UIMisc.GetControlParentsString(focusedCtl);
				//DebugLog.Message("FocusedControl: " + fch);
			}

			bool mouseInQuickSearchPopup = false;
			if (this.Visible)
			{
				mousePosition = System.Windows.Forms.Control.MousePosition;

				ul = this.PointToScreen(new Point(0, 0));
				rt = new Rectangle(ul, this.Size);
				if (rt.Contains(mousePosition))
					mouseInQuickSearchPopup = true;
			}

			bool hasFocus = // should we keep the popup visible
			 (quickSearchPopupContainsFocus ||
				commandLineControlContainsFocus || // quick search or assoc control have focus?
				(ConsiderShellFocus && !shellContainsFocus));
				//ribbonCtlContainsFocus || // seems to get focus after scroll of grid
				//gridCtlContainsFocus ||
				//activeContextMenuVisible);

			if (mouseInQuickSearchPopup && !hasFocus) // if mouse is in the popup but we don't have focus then give it to ourselves
			{ // focus seems to go elsewhere sometimes when paging down in grid control
				this.Focus();
				//UIMisc.Beep();
			}

			bool keepVisible = mouseInQuickSearchPopup || hasFocus;

			bool hidePopup = quickSearchPopupVisible && !keepVisible; // should hide if currently visible but don't want to keep it visible

			if (hidePopup) // focus has moved away from popup
			{
				HideQuickSearchPopup();
				CommandLineControl.Text = ""; // clear any text
				PreviousInput = "";
				ActiveContextMenu = null;
				//Timer.Enabled = true;
				return; // leave timer disabled
			}

			//			if (Instance.CidAllDataCommandLineContextMenu.Visible) return; // don't send if popup already visible

			if (CommandLineControl.ContainsFocus)
			{
				string cid = CommandLineControl.Text;
				if (PreviousInput != cid)
				{
					if (DateTime.Now.Subtract(LastCommandLineControlKeyDownTime).TotalSeconds > .5)
					{ // wait for a pause in typing before processing input
						PreviousInput = cid;
						ShowQuickSearchPopup(CommandLineControl.Text, true);
					}
					//else SystemUtil.Beep(); // debug
				}
			}

			Timer.Enabled = true;
			return;
		}

		private void OtherCidsList_SelectedIndexChanged(object sender, EventArgs e)
		{
			PreviousInput = OtherCidsList.Text;
			ShowQuickSearchPopup(OtherCidsList.Text, false);
		}

		/// <summary>
		/// Display any structure and related compounds for input string or
		/// Matching database contents
		/// </summary>
		/// <param name="inputString"></param>
		/// <param name="showRelatedCompounds"></param>

		public void ShowQuickSearchPopup(
			string inputString,
			bool updateRelatedCompounds)
		{
			string extCid = "", tok, tok2;

			MRUEdit clc = CommandLineControl; // be sure popup is properly positioned

			if (clc.FindForm() == SessionManager.Instance.ShellForm) // slightly different position for Shell form that other forms
			{
				Left = clc.Left;
				Top = clc.Bottom;
			}

			else
			{
				Point p = clc.PointToScreen(new Point(0, clc.Bottom)); // get screen coord for upper left corner of popup
				Point p2 = this.Parent.PointToClient(p); // convert screen coord back to relative
				Left = p2.X;  // position below input control
				Top = p2.Y;
			}

			// See if input matches structure

			Molecule = null;
			if (StructurePopupEnabled && !String.IsNullOrEmpty(inputString))
			{
				Cid = CompoundId.Normalize(inputString);
				extCid = CompoundId.Format(Cid);
				CidMt = CompoundId.GetRootMetaTableFromCid(Cid);
				Molecule = MoleculeUtil.SelectMoleculeForCid(Cid, CidMt);
				RelatedDataButton.Enabled = (CidMt != null && Lex.Eq(CidMt.Root.Name, MetaTable.PrimaryRootTable));
				AllDataButtonStb.Enabled = QbUtil.IsMdbAssayDataViewAvailable(CidMt);
			}

			if (Molecule != null)
			{
				string txt = extCid;
				if (CidMt != null) txt = CidMt.KeyMetaColumn.Label + " " + txt;
				if (Lex.IsUndefined(Molecule.Id))
					Molecule.Id = txt;

				if (SS.I.FindRelatedCpdsInQuickSearch) txt += " and Related Structures";
				StructurePanel.Text = txt;

				RelatedStructuresControl.MoleculeControl.Molecule = Molecule;

				if (SS.I.FindRelatedCpdsInQuickSearch)
					Width = InitialStructurePanelWidth; // assure correct width

				else
				{
					Width = InitialStructurePanelWidthWithoutRelatedStructureOptions;
					RSM.ClearSearchStatus();
				}

				if (!StructurePanel.Visible) // if structure not showing then set height to initial height
					Height = InitialStructurePanelHeight;

				StructurePanel.Location = new Point(0, 0);
				StructurePanel.Visible = true;
				//Size = StructurePanel.Size;
				ListControl.Visible = false;
				Visible = true;
				RenderId++;

				if (updateRelatedCompounds)
					UpdateRelatedCidsDisplay(CidMt, Cid, Molecule, RenderId);

				return;
			}

			// See if input matches database contents tree

			else if (ContentsPopupEnabled)
			{
				DoQuickSearchAndDisplayOfContentsTree(inputString);
				return;
			}

			HideQuickSearchPopup();
			return;
		}

		/// <summary>
		/// DoQuickSearchAndDisplayOfContentsTree
		/// </summary>
		/// <param name="inputString"></param>

		public void DoQuickSearchAndDisplayOfContentsTree(
			string inputString)
		{
			int maxNodes = 10;
			MatchingContentsNodes = // find the first set of matching nodes
				ContentsTree.FindNodesBySubstringQuick(inputString, maxNodes);

			if (ListControl.ImageList == null) // be sure we have the images
				ListControl.ImageList = Bitmaps.Bitmaps16x16;

			if (MatchingContentsNodes.Count > 0)
			{
				HashSet<string> allowedMts = ClientState.UserInfo.RestrictedViewAllowedMetaTables;

				ListControl.Items.Clear();
				foreach (MetaTreeNode mtn0 in MatchingContentsNodes)
				{
					if (allowedMts != null && MetaTreeNode.IsDataTableNodeType(mtn0.Type)) // see if datatable (metatable) allowed for user
					{
						string mtName = mtn0.Target.Trim().ToUpper();
						if (!allowedMts.Contains(mtName)) continue;
					}

					ImageListBoxItem lbi = new ImageListBoxItem();
					lbi.ImageIndex = mtn0.GetImageIndex();
					int i1 = mtn0.Label.IndexOf(inputString, StringComparison.OrdinalIgnoreCase);
					if (i1 >= 0) lbi.Value = mtn0.Label;
					else lbi.Value = mtn0.Target + " - " + mtn0.Label;
					ListControl.Items.Add(lbi);
				}

				if (MatchingContentsNodes.Count >= maxNodes) // show elipsis if only showing partial results
				{
					ImageListBoxItem lbi = new ImageListBoxItem();
					lbi.Value = "...";
					ListControl.Items.Add(lbi);
				}

				ListControl.Location = new Point(0, 0);
				Size = ListControl.Size;
				ListControl.Visible = true;
				StructurePanel.Visible = false;
				Visible = true;
				RenderId++;
				ListControl.BringToFront();
				return;
			}

			else HideQuickSearchPopup(); // nothing found

			return;
		}

		/// <summary>
		/// New update the list of related compounds
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="cid"></param>
		/// <param name="queryStruct"></param>

		private void UpdateRelatedCidsDisplay(
			MetaTable mt,
			string cid,
			MoleculeMx queryStruct,
			int renderId)
		{
			bool useNew = ServicesIniFile.ReadBool("UseNewRelatedCompoundsPopup", true); // use new or old related compounds display

			if (!useNew)
			{
				UpdateRelatedCidsDisplayOld(mt, cid, queryStruct);
				return;
			}

			//if (!SS.I.AllowGroupingBySalts || // need this?
			//	Lex.Ne(MetaTable.DefaultRootTable, mt.Name))
			//{
			//	this.Height = RelatedStructuresPanel.Top;
			//	return;
			//}

			if (SS.I.FindRelatedCpdsInQuickSearch)
			{
				RSM.StartSearchAndRetrievalOfRelatedStructures(mt.Name, cid, queryStruct, RelatedStructureBrowser.StructureSearchTypes);
			}

			return;
		}

		/// <summary>
		/// Update the list of related compounds
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="cid"></param>
		/// <param name="str"></param>

		private void UpdateRelatedCidsDisplayOld(
			MetaTable mt,
			string cid,
			MoleculeMx str)
		{
			if (SS.I.AllowGroupingBySalts) // build list of salts & send
			{
				List<string> salts = MoleculeUtil.GetAllSaltForms(Cid);
				if (salts == null || salts.Count == 0) // nothing related
					OtherCidsList.Visible = false; // hide list

				else
				{
					OtherCidsList.Properties.Items.Clear();
					foreach (string s in salts)
					{
						string extCid = CompoundId.Format(s);
						OtherCidsList.Properties.Items.Add(extCid);
					}
					string listHeader = salts.Count.ToString() + " Other Match";
					if (salts.Count > 1) listHeader += "es"; // use proper grammar
					OtherCidsList.Text = listHeader; // select first item
					OtherCidsList.Visible = true;
				}
			}

			else OtherCidsList.Visible = false; // hide list
		}


		private void ListControl_MouseDown(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("Down " + ListControl.SelectedIndex);
			int i1 = ListControl.SelectedIndex; //  e.Y / ListControl.ItemHeight;
			if (i1 >= MatchingContentsNodes.Count) return; // selected elipsis
			MetaTreeNode mtn = MatchingContentsNodes[i1];
			if (e.Button == MouseButtons.Left) // immediate action
			{
				QbUtil.CallCurrentProcessTreeItemOperationMethod("Open", mtn.Target); 
				//MainMenuControl.OpenMetaTreeNodeObject(mtn);
				HideQuickSearchPopup();
			}

			else // right button click, show menu
			{
				QbContentsTree treeCtl = SessionManager.Instance.MainContentsControl;
				treeCtl.CurrentContentsMetaTreeNode = mtn;
				treeCtl.CurrentContentsTreeListNode = null; // tree list node not known

				ActiveContextMenu = treeCtl.ContentsTreeContextMenus.GetSingleNodeContextMenu(mtn);
				if (ActiveContextMenu != null)
				{
					ActiveContextMenu.Show(ListControl, new System.Drawing.Point(e.X, e.Y));
					while (ActiveContextMenu != null && ActiveContextMenu.Visible) // keep this visible as long as popup is visible
					{
						System.Threading.Thread.Sleep(250);
						Application.DoEvents();
					}
					HideQuickSearchPopup();
				}
			}

			return;
		}

		private void ListControl_MouseUp(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("Up " + ListControl.SelectedIndex);
			return;
		}

		private void ListControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			//ClientLog.Message("SelectedIndexChanged " + ListControl.SelectedIndex);
			return;
		}

		private void ListControl_Click(object sender, EventArgs e)
		{
			//ClientLog.Message("Click " + e);
			return;
		}

		private void ListControl_Enter(object sender, EventArgs e)
		{
			//ClientLog.Message("Enter " + e);
			return;
		}

		private void ListControl_Leave(object sender, EventArgs e)
		{
			//ClientLog.Message("Leave " + e);
			return;
		}

		private void ListControl_MouseClick(object sender, MouseEventArgs e)
		{
			//ClientLog.Message("MouseClick. " + e);
			return;
		}

		private void ListControl_MouseEnter(object sender, EventArgs e)
		{
			//ClientLog.Message("MouseEnter " + e);
			return;
		}

		private void ListControl_MouseLeave(object sender, EventArgs e)
		{
			//ClientLog.Message("MouseLeave " + e);
			return;
		}

		private void Structure_KeyDown(object sender, KeyEventArgs e)
		{
			CheckForEscapePressed(e);
		}

		private void OtherCidsList_KeyDown(object sender, KeyEventArgs e)
		{
			CheckForEscapePressed(e);
		}

		private void ListControl_KeyDown(object sender, KeyEventArgs e)
		{
			CheckForEscapePressed(e);
		}

		/// <summary>
		/// If Esc pressed hide the popup control
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>

		bool CheckForEscapePressed(KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Escape) return false;
			HideQuickSearchPopup();
			SessionManager sm = SessionManager.Instance;
			sm.ActivateQuickSearchControl();
			if (sm.CommandLineControl != null)
				sm.CommandLineControl.Text = "";
			PreviousInput = ""; // avoid any additional QuickDisplays
			e.Handled = true;
			return true;
		}

		/// <summary>
		/// Get table view of all data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AllDataButton_Click(object sender, EventArgs e)
		{
			if (!Lex.IsDefined(Cid)) return;
			
			HideQuickSearchPopup();
			PreviousInput = ""; // avoid any additional QuickDisplays
			CommandExec.ExecuteCommandAsynch("SelectAllCompoundData " + Cid);

			return;
		}

		private void RelatedDataButton_Click(object sender, EventArgs e)
		{
			if (!Lex.IsDefined(Cid)) return;

			HideQuickSearchPopup();
			PreviousInput = ""; // avoid any additional QuickDisplays

			RelatedCompoundsDialog.Show(Cid);

			return;
		}


		/// <summary>
		/// Close popup
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CloseButton_Click(object sender, EventArgs e)
		{
			HideQuickSearchPopup();
			return;
		}

		/// <summary>
		/// Get Spotfire multi-database assay view of data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void AllDataButtonMdbAssay_Click(object sender, EventArgs e)
		{
			if (Lex.IsDefined(Cid))
			{
				HideQuickSearchPopup();
				PreviousInput = ""; // avoid any additional QuickDisplays
				CommandLine.Execute("TargetResultsViewer ShowPopup " + Cid);
			}
			return;
		}

	}

}
