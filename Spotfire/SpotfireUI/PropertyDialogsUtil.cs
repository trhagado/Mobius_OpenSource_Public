using Mobius.ComOps;
using Mobius.Data;
//using Mobius.ClientComponents;

using Mobius.SpotfireDocument;
using Mobius.SpotfireClient;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
/// <summary>
/// SpotfireApiClient for session
/// </summary>

	public class SpotfireSession
	{
		public static SpotfireApiClient SpotfireApiClient;
	}

/// <summary>
/// Spotfire property dialogs utility functions
/// </summary>

	public class PropertyDialogsUtil
	{

		/// <summary>
		/// Do common setup of tabs in visualization property dialogs
		/// </summary>

		public static void AdjustPropertyPageTabs(
			XtraTabControl tabCtl,
			TreeView treeViewCtl,
			PanelControl containerPanel)
		{
			treeViewCtl.Dock = DockStyle.Fill;

			containerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

			tabCtl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

			tabCtl.Dock = DockStyle.None;
			tabCtl.Location = new Point(-1, -1);
			tabCtl.Size = new Size(containerPanel.Width + 5, containerPanel.Height + 4);
			//Tabs.Size = new Size(TabsContainerPanel.Width - 10, TabsContainerPanel.Height - 10);
			tabCtl.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;
			tabCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));

			// Populates the left-side property page selector

			treeViewCtl.Nodes.Clear();

			foreach (XtraTabPage page in tabCtl.TabPages)
			{
				TreeNode node = new TreeNode(page.Text);
				node.Tag = page;
				treeViewCtl.Nodes.Add(node);
			}

			return;
		}

		/// <summary>
		/// Select the supplied property page from both the Treeview and the Tab controls
		/// </summary>
		/// <param name="pageName"></param>
		/// <param name="treeView"></param>
		/// <param name="tabs"></param>

		public static void SelectPropertyPage(
			string pageName,
			TreeView treeView,
			XtraTabControl tabs)
		{
			TreeNode fallbackNode = null;

			foreach (TreeNode node in treeView.Nodes) // scan nodes
			{
				XtraTabPage page = node.Tag as XtraTabPage; // get corresponding tab page
				if (page == null) continue;

				if (page == tabs.SelectedTabPage) fallbackNode = node;

				if (Lex.Eq(page.Name, pageName) || Lex.Eq(page.Name, pageName + "Tab") ||
					Lex.Eq(page.Name, pageName + "TabPage"))
				{
					treeView.SelectedNode = node;
					tabs.SelectedTabPage = page;
					return;
				}
			}

			if (fallbackNode != null) treeView.SelectedNode = fallbackNode;

			return;
		}


	}
}
