using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Control for selecting the Mobius query that provides data for the associated Spotfire data table
	/// </summary>

	public partial class QuerySelectorControl : DevExpress.XtraEditors.XtraUserControl
	{
		public bool ShowCurrentQueryMenuItem = true;

		public int SelectedQueryId = -1;
		public string SelectedQueryName = "";

		public event EventHandler CallerEditValueChangedHandler; // event to fire back to caller when edit value changes here

		bool InSetup = false;

		MainMenuControl MainMenu { get { return SessionManager.Instance?.MainMenuControl; } }

		public QuerySelectorControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup the control for specified view
		/// </summary>
		/// <param name="view"></param>

		public void Setup(
			int selectedQueryId,
			string selectedQueryName,
			EventHandler callerEditValueChangedEventHandler = null)
		{
			DataRow dr;
			DataColumn dc;
			string queryName = "";

			InSetup = true;

			SelectedQueryId = selectedQueryId;
			SelectedQueryName = selectedQueryName;
			CallerEditValueChangedHandler = callerEditValueChangedEventHandler;

			if (SelectedQueryId < 0)
			{
				if (Lex.IsDefined(SelectedQueryName) && ShowCurrentQueryMenuItem) // if undefined use current query if available
				{
					SelectedQueryId = 0;
					queryName = "Current Query";
				}

				else // start with no query
				{
					SelectedQueryName = null;
					queryName = "";
				}
			}

			else if (SelectedQueryId == 0)
			{
				SelectedQueryName = SelectedQueryName;
				queryName = "Current Query";
			}

			else
			{
				Query q = QbUtil.ReadQuery(SelectedQueryId);
				if (q != null)
				{
					SelectedQueryName = q.Name;
					SelectedQueryId = q.UserObject.Id;
				}

				else
				{
					SelectedQueryId = -1;
					SelectedQueryName = null;
				}
			}

			SourceQueryComboBox.Text = queryName;

			InSetup = false;

			return;
		}

		private void SourceQueryComboBox_Click(object sender, EventArgs e)
		{
			this.Focus(); // move focus away
			ShowSelectQueryMenu();
		}

		/// <summary>
		/// Show the menu at the specified location
		/// </summary>
		/// <param name="selectedQueryId"></param>
		/// <param name="screenLoc"></param>
		/// <returns></returns>

		public int ShowSelectQueryMenu(
			int selectedQueryId,
			Point screenLoc)
		{
			SelectedQueryId = selectedQueryId;

			ShowSelectQueryMenu(screenLoc);

			if (SelectedQueryId >= 0) // selection made?
				return SelectedQueryId;

			else return selectedQueryId; // no selection, return initial id
		}

		/// <summary>
		/// Show the select query menu
		/// </summary>

		public void ShowSelectQueryMenu()
		{
			Point screenLoc = SourceQueryComboBox.PointToScreen(new Point(0, SourceQueryComboBox.Height));
			ShowSelectQueryMenu(screenLoc);
			return;
		}
		public void ShowSelectQueryMenu(Point screenLoc)
		{
			CurrentQueryMenuItem.MouseDown += new MouseEventHandler(CurrentQueryMenuItem_MouseDown);

			CurrentQueryMenuItem.Visible = ShowCurrentQueryMenuItem;

			BuildMruListMenu();
			BuildFavoritesMenu();

			SelectedQueryId = -1;

			SourceQueryMenu.Show(screenLoc);
			SourceQueryMenu.Focus();

			while (SourceQueryMenu.Visible) // wait til menu closes
			{
				Application.DoEvents();
				Thread.Sleep(100);
			}

			return;
		}

		public void BuildMruListMenu()
		{
			if (MainMenu?.MruList == null) return;

			if (MruListMenuItem.DropDownItems.Count > 0) return;

			List<MetaTreeNode> nodes = BuildMetaTreeNodeList(MainMenu.MruList);

			foreach (MetaTreeNode node in nodes)
			{
				if (MruListMenuItem.DropDownItems.Count >= MainMenuControl.MaxDisplayedMruItems) return;

				ToolStripMenuItem mi = new ToolStripMenuItem();
				mi.Image = Bitmaps.Bitmaps16x16.Images[node.GetImageIndex()];
				mi.Text = node.Label;
				//mi.ForeColor = NewMenuItem.ForeColor;
				mi.Tag = node;
				mi.MouseDown += new MouseEventHandler(QueryListMenuItem_MouseDown);
				MruListMenuItem.DropDownItems.Add(mi);
			}

		}

		public void BuildFavoritesMenu()
		{
			if (FavoritesMenuItem.DropDownItems.Count > 0) return;

			List<MetaTreeNode> nodes = BuildMetaTreeNodeList(MainMenu.Favorites);

			foreach (MetaTreeNode node in nodes)
			{
				ToolStripMenuItem mi = new ToolStripMenuItem();
				mi.Image = Bitmaps.Bitmaps16x16.Images[node.GetImageIndex()];
				mi.Text = node.Label;
				//mi.ForeColor = NewMenuItem.ForeColor;
				mi.Tag = node;
				mi.MouseDown += new MouseEventHandler(QueryListMenuItem_MouseDown);
				FavoritesMenuItem.DropDownItems.Add(mi);
			}

		}

		/// <summary>
		/// Build a MetaTreeNodeList containing just queries or data table objects from a list of node names (MruList or Favorites)
		/// </summary>
		/// <param name="nodeNameList"></param>
		/// <returns></returns>

		List<MetaTreeNode> BuildMetaTreeNodeList(List<string> nodeNameList)
		{
			List<MetaTreeNode> nodes = new List<MetaTreeNode>();
			int li = 0;
			while (li < nodeNameList.Count)
			{
				string nodeName = nodeNameList[li];
				if (String.IsNullOrEmpty(nodeName))
				{
					nodeNameList.RemoveAt(li);
					continue;
				}

				MetaTreeNode node = MainMenu.GetMetaTreeNode(nodeName);
				if (node != null)
				{
					li++;
					if (node.Type == MetaTreeNodeType.Query) // || MetaTreeNode.IsDataTableNodeType(node.Type))
						nodes.Add(node);

					else continue;
				}

				else // remove from list if can't find
					nodeNameList.RemoveAt(li); // remove from list if not in db
			}

			return nodes;
		}

		private void CurrentQueryMenuItem_MouseDown(object sender, MouseEventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;

			SourceQueryComboBox.Text = "Current Query";

			throw new NotImplementedException();

			//CurrentQueryId = 0; // indicate current query
			//CurrentQuery = null;
			//return;
		}

		private void QueryListMenuItem_MouseDown(object sender, MouseEventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			MetaTreeNode node = mi.Tag as MetaTreeNode;

			SetMobiusSourceQuery(node);
			return;
		}

		private void SelectFromContentsTreeMenuItem_Click(object sender, EventArgs e)
		{
			MetaTreeNode node = SelectFromContents.SelectSingleItem(
				"Select a Query",
				"Select a Query to use to use for data retrieval",
				MetaTreeNodeType.Query,
				null,
				false);

			SetMobiusSourceQuery(node);
			return;
		}

		void SetMobiusSourceQuery(MetaTreeNode node)
		{
			if (node == null || node.Type != MetaTreeNodeType.Query) return;

			SelectedQueryName = node.Label;
			SelectedQueryId = UserObject.ParseObjectIdFromInternalName(node.Name);
			SourceQueryComboBox.Text = SelectedQueryName;

			return;
		}

		private void ViewQueryMenuItem_Click(object sender, EventArgs e)
		{
			return;
		}

		private void SourceQueryComboBox_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		/// <summary>
		/// Value of some form field has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (CallerEditValueChangedHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedHandler(this, EventArgs.Empty);
		}

	}
}
