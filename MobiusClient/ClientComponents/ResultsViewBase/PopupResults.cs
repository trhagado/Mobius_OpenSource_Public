using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Mobius.ClientComponents

{
	public partial class PopupResults : DevExpress.XtraEditors.XtraForm
	{
		public PopupResults()
		{
			MoleculeGridPageControl.RemoveToolPanelInConstructor = false; // keep tool panel and properly layout of controls
			InitializeComponent();
			MoleculeGridPageControl.RemoveToolPanelInConstructor = true;

			if (SystemUtil.InDesignMode) return;

			//MoleculeGridPageControl.BorderStyle = BorderStyle.None;

		}

		/// <summary>
		/// Show popup results in a results window
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="title"></param>

		public static void Show( 
			QueryManager qm)
		{
			string uri;
			int pi, posDelta = 20;

			Progress.Hide(); // hide any progress so popup gets focus

			string title = qm.Query.UserObject.Name;

			PopupResults pr = new PopupResults();
			UIMisc.PositionPopupForm(pr);
			pr.Text = title;

			QueryResultsControl qrc = pr.QueryResultsControl;
			qm.LinkMember(qrc);
			Query q = qm.Query;
			qrc.BuildResultsPagesTabs(q);

			if (q.ResultsPages.Pages.Count <= 1) // hide tabs if only one
			{
				qrc.ResultsLabel.Visible = qrc.Tabs.Visible = false;
			}


			//qrc.ToolPanel.Visible = false; // hide tools for now (e.g. Edit Query)

			//if (q.ResultsPages.Pages.Count <= 1) // hide tabs if only one
			//{
			//	PanelControl pc = qrc.ResultsPagePanelContainer;
			//	int delta = pc.Top;
			//	pc.Top = 0;
			//	pc.Height += delta;
			//}

			pr.Show();
			//qrc.ConfigureResultsPage(0); // render the first page
			qrc.SelectPage(q.InitialBrowsePage); // show the initial browse page

			UIMisc.BringFormToFront(pr);
			return;
		}

/// <summary>
/// Show the specified query in a new PopupResults form
/// </summary>
/// <param name="qm"></param>
/// <param name="html"></param>
/// <param name="title"></param>

		public static void ShowHtml( // navigate browser to a document
			QueryManager qm,
			string html,
			string title)
		{
			string uri;
			int pi;

			Progress.Hide(); // hide any progress so popup gets focus

			uri = ClientDirs.TempDir + @"\PopupResults" + "1" + ".htm";

			StreamWriter sw = new StreamWriter(uri);
			sw.Write(html);
			sw.Close();

			PopupResults pr = new PopupResults();
			UIMisc.PositionPopupForm(pr);
			pr.Text = title;

			QueryResultsControl qrc = pr.QueryResultsControl;
			qm.LinkMember(qrc);
			Query q = qm.Query;
			qrc.BuildResultsPagesTabs(q); 

			for (pi = 0; pi < q.ResultsPages.Pages.Count; pi++)
			{
				ResultsPage page = q.ResultsPages.Pages[pi];
				for (int vi = 0; vi < page.Views.Count; vi++ )
				{
					ResultsViewProps view = page.Views[vi];
					if (view.ViewType == ResultsViewType.HtmlTable)
					{
						view.Title = title;
						view.ContentUri = uri; // plug in file name for uri
					}
				}
			}

			pr.Show();
			qrc.ConfigureResultsPage(0); // render the first page

			UIMisc.BringFormToFront(pr);
			return;
		}

		private void PopupResults_FormClosed(object sender, FormClosedEventArgs e)
		{
			ViewManager.TryCallCustomExitingQueryResultsCallback(QueryResultsControl, ExitingQueryResultsType.ClosingWindow);
		}
	}


	/// <summary>
	/// Method parms
	/// </summary>

	public class ExitingQueryResultsParms
	{
		public QueryResultsControl Qrc = null;
		public ExitingQueryResultsType ExitType;
	}

	/// <summary>
	/// Delegate used when exiting a QueryResults view
	/// </summary>
	/// <param name="qrc">QueryResultsControl being exited</param>
	/// <param name="exitType"></param>

	public delegate void ExitingQueryResultsDelegate(
		ExitingQueryResultsParms parms);

/// <summary>
/// Possible ways of exiting query results
/// </summary>

	public enum ExitingQueryResultsType
	{
		Unknown = 0,
		EditQuery = 1,
		ClosingWindow = 2
	}
}