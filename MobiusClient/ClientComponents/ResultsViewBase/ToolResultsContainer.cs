using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents.Dialogs;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Parent control for diplaying and managing Mobius tool results (in a QueryResultsControl)
	/// </summary>

	public partial class ToolResultsContainer : DevExpress.XtraEditors.XtraUserControl
	{
		public SimpleDelegate SetupQueryResultsControlMethod = null; // 

		public ToolResultsContainer()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		public void SetupQueryResultsControlForResultsDisplay(
			QueryManager qm)
		{
			Progress.Hide(); // hide any progress so popup gets focus

				//string title = qm.Query.UserObject.Name;

				//PopupResults pr = new PopupResults();
				//UIMisc.PositionPopupForm(pr);
				//pr.Text = title;

			QueryResultsControl qrc = QueryResultsControl;

			qm.LinkMember(qrc);
			Query q = qm.Query;

			if (SetupQueryResultsControlMethod != null) SetupQueryResultsControlMethod(); // let parent know that we're setting up the QueryResultsControl
			
			qrc.BuildResultsPagesTabs(q);

			if (q.ResultsPages.Pages.Count <= 1) // hide tabs if only one
			{
				qrc.ResultsLabel.Visible = qrc.Tabs.Visible = false;
			}

			//qrc.ConfigureResultsPage(0); // render the first page
			qrc.SelectPage(q.InitialBrowsePage); // show the initial browse page

		}

	}
}
