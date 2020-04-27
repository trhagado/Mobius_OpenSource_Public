using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Process clicks on Mobius command hyperlinks 
/// </summary>

	public partial class PopupMobiusCommandNavigation : Form
	{
		string ShowDescriptionArg; // name of metatable to show description for
		string AddToQueryArg; // name of metatable to add to query
		string SelectedCid = ""; // selected compound id
		string SelectedTarget = ""; // selected target
		string CommandToExecute = ""; // queued command to execute
		Control ParentControl;

		public PopupMobiusCommandNavigation()
		{
			InitializeComponent();
		}

/// <summary>
/// Process click on Mobius command hyperlinks 
/// </summary>
/// <param name="e"></param>
/// <param name="Panel"></param>
/// <returns></returns>

		public static bool Process(
			WebBrowserNavigatingEventArgs e,
			Control parentControl)
		{
			PopupMobiusCommandNavigation i = new PopupMobiusCommandNavigation();
			bool result = i.ProcessInternal(e, parentControl);
			return result;
		}

		public bool ProcessInternal(
			WebBrowserNavigatingEventArgs e,
			Control parentControl)
		{
			string url, arg;

			url = e.Url.ToString();

			ParentControl = parentControl; // panel to attach menu to

			if (!Lex.Contains(url, "mobius/command")) return false;

			int i1 = url.IndexOf("?");
			if (i1 < 0) return false;
			string cmd = url.Substring(i1 + 1);
			cmd = cmd.Replace("%20", " "); // todo: more complete translation
			Lex lex = new Lex();
			lex.OpenString(cmd);

			if (MoleculeGridControl.IsShowContextMenuCommand(cmd, "ShowContextMenu:MetaTableLabelContextMenu", out arg))
			{
				SetupTableHeaderPopup(cmd, menuShowDescription, out ShowDescriptionArg,
					menuAddToQuery, out AddToQueryArg);
				Point ptCursor = Cursor.Position;
				ptCursor = parentControl.PointToClient(ptCursor);
				MetaTableLabelContextMenu.Show(parentControl, ptCursor); // show on panel since events lost if on WebBrowser control

				while (MetaTableLabelContextMenu != null && MetaTableLabelContextMenu.Visible) // keep this visible as long as popup is visible
				{
					System.Threading.Thread.Sleep(250);
					Application.DoEvents();
				}
			}

			else if (MoleculeGridControl.IsShowContextMenuCommand(cmd, "ShowContextMenu:CompoundIdContextMenu", out arg))
			{
				SelectedCid = arg;
				Point ptCursor = Cursor.Position;
				ptCursor = parentControl.PointToClient(ptCursor);
				CidContextMenu.Show(parentControl, ptCursor); // show on panel since events lost if on WebBrowser control

				while (CidContextMenu != null && CidContextMenu.Visible) // keep this visible as long as popup is visible
				{
					System.Threading.Thread.Sleep(250);
					Application.DoEvents();
				}
			}

			else // queue the command to execute after exiting this event
			{
				CommandToExecute = cmd;
				Timer_Tick(null, null); // just call directly for now, may need to fixup later to do after exiting the event
				//Timer.Start();
			}

			e.Cancel = true; // we've taken care of it

			return true;
		}


/// <summary>
/// Process queued command
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Timer_Tick(object sender, EventArgs e)
		{
			string arg;
			int i1;

			Timer.Stop();

			string cmd = CommandToExecute;
			if (String.IsNullOrEmpty(cmd)) return;

			else if (MoleculeGridControl.IsShowContextMenuCommand(cmd, "ShowContextMenu:TargetContextMenu", out arg))
			{
				SelectedTarget = arg;
				Point ptCursor = Cursor.Position;
				ptCursor = PointToClient(ptCursor);
				TargetContextMenu.Show(ParentControl, ptCursor); // show on panel since events lost if on WebBrowser control
			}

			else if ((i1 = Lex.IndexOf(cmd, "ClickFunction")) >= 0)
			{
				cmd = cmd.Substring(i1 + "ClickFunction".Length + 1); // get function name
				ClickFunctions.Process(cmd, null);
			}

			else CommandExec.ExecuteCommandAsynch(cmd);

			return;
		}

		/// <summary>
		/// Setup menu for table header popup
		/// </summary>
		/// <param name="menuShowDescription"></param>
		/// <param name="ShowDescription"></param>
		/// <param name="menuAddToQuery"></param>
		/// <param name="AddToQuery"></param>

		public void SetupTableHeaderPopup(
			string argString,
			ToolStripMenuItem menuShowDescription,
			out string showDescriptionArg,
			ToolStripMenuItem menuAddToQuery,
			out string addToQueryArg)
		{
			string[] args = argString.Split('&');
			menuShowDescription.Enabled = false;
			menuAddToQuery.Enabled = false;
			showDescriptionArg = addToQueryArg = "";
			foreach (string s in args)
			{
				string[] args2 = s.Split('=');
				if (args2[0].ToLower().Trim() == "showdescription")
				{
					menuShowDescription.Enabled = true;
					showDescriptionArg = args2[1];
				}

				else if (args2[0].ToLower().Trim() == "addtoquery")
				{
					menuAddToQuery.Enabled = true;
					addToQueryArg = args2[1];
				}
			}
		}

		private void menuShowDescription_Click(object sender, System.EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ShowTableDescription " + ShowDescriptionArg);
		}

		private void menuAddToQuery_Click(object sender, System.EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("AddTableToQuery " + AddToQueryArg);
		}

		private void CnPopupStructure_Click(object sender, System.EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("SelectCompoundStructure " + SelectedCid);
		}

		private void CnPopupCopyStructure_Click(object sender, System.EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("CopyCompoundIdStructure " + SelectedCid);
		}

		private void CnPopupAllData_Click(object sender, System.EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("SelectAllCompoundData " + SelectedCid);
		}

		private void CnPopupShowStbView_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ShowStbViewForCompound " + SelectedCid);
		}

		private void ShowTargetDescriptionMenuItem_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ShowTargetDescription " + SelectedTarget);
		}

		private void ShowTargetAssayListMenuItem_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ShowTargetAssayList " + SelectedTarget);
		}

		private void ShowTargetAssayDataMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void ShowTargetAssayDataMenuItem_Click_1(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ShowAllTargetUnsummarizedAssayData " + SelectedTarget);
		}

		private void CnPopupShowCorpIdChangeHistory_Click(object sender, EventArgs e)
		{
			ClickFunctions.Process("ShowCorpIdChangeHistory " + SelectedCid, null);
		}

	}
}
