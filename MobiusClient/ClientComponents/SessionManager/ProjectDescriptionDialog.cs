using Mobius.Data;
using Mobius.ComOps;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class ProjectDescriptionDialog : XtraForm
	{
		public ProjectDescriptionDialog()
		{
			InitializeComponent();

			//DiagramDisplayControl.Size = WebBrowser.Size; // size DiagramDisplayControl to match WebBrowser control
			//SetDiagramControlSize();
		}

		void SetDiagramControlSize()
		{
			Point p1 = Tabs.PointToScreen(Tabs.Location); // position diagram control as if on a tab page
			Point p2 = WebBrowser.PointToScreen(new Point(0, 0));
			int dx = p2.X - p1.X;
			int dy = p2.Y - p1.Y;
			//DiagramDisplayControl.Location = new Point(Tabs.Location.X + dx, Tabs.Location.Y + dy);
			//DiagramDisplayControl.Size = WebBrowser.Size;
		}

		public static void ShowProjectDescription(
			string projNodeName)
		{
			StreamWriter sw;
			string html, htmlFileName, flowScheme = "", flowSchemeFileName = "";

			ProjectDescriptionDialog form = new ProjectDescriptionDialog();

			MetaTreeNode mtn = MetaTree.GetNode(projNodeName);
			string title = (mtn != null ? mtn.Label : projNodeName);

			html = GetProjectHtmlDescription(projNodeName);
			if (Lex.IsNullOrEmpty(html))
				html = "Unable to retrieve project description";

			string[] sa = html.Split('\v');
			if (sa.Length >= 2) // write flowscheme if exists
			{
				html = sa[0];
				flowScheme = sa[1];
				flowSchemeFileName = ClientDirs.TempDir + @"\FlowScheme" + UIMisc.PopupCount + ".xml";
				sw = new StreamWriter(flowSchemeFileName);
				sw.Write(flowScheme);
				sw.Close();
			}

			htmlFileName = ClientDirs.TempDir + @"\PopupHtml" + UIMisc.PopupCount + ".htm";

			sw = new StreamWriter(htmlFileName);
			sw.Write(html);
			sw.Close();

			UIMisc.PositionPopupForm(form);
			form.Text = title;
			form.Show();

			form.WebBrowser.Navigate(htmlFileName);

			if (flowSchemeFileName != "")
				form.OpenFlowSchemeDiagram(flowSchemeFileName);

			form.Tabs.SelectedTabPageIndex = 0; 

			UsageDao.LogEvent("ShowProject", projNodeName);
			return;
		}


		/// <summary>
		/// Get the AFS project id, if any, associated with a project node name
		/// </summary>
		/// <param name="projNodeName"></param>
		/// <returns></returns>

		public static string GetProjectHtmlDescription(string projNodeName)
		{
			string html = CommandDao.Execute("ProjectTreeFactory.GetProjectHtmlDescription " + projNodeName);
			return html;
		}

		public void OpenFlowSchemeDiagram(string fileName) 
		{
			try
			{
				//xmlStore1.DirectoryName = Path.GetDirectoryName(fileName);
				//NShapeProject.Name = Path.GetFileNameWithoutExtension(fileName);
				//NShapeProject.LibrarySearchPaths.Add(ClientDirs.StartupDir); // path for Dataweb.NShape modules
				//NShapeProject.AutoLoadLibraries = true;
				//NShapeProject.Open();

				//DiagramDisplayControl.LoadDiagram("PROJ_FLOW_SCHEME"); // (any way to see diagram names?)
				//DiagramDisplayControl.EnsureVisible(0, 0); // be sure upper left corner shows initially
			}
			catch (Exception ex)
			{
				return;
			}
		}

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			//if (Tabs.SelectedTabPageIndex == 1)
			//	DiagramDisplayControl.BringToFront();

			//else DiagramDisplayControl.SendToBack();
		}


	}
}
