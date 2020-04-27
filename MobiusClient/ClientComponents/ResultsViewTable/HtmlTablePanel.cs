using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
//using DevExpress.XtraCharts;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// Panel that contains a HTML display page
/// </summary>

	public partial class HtmlTablePanel : XtraUserControl
	{
		internal ResultsPage ResultsPage;  // the results page associated with the page panel
		internal HtmlTablePageControl PageControl; // the page that contains the panel
		internal HtmlTableView View; // current HtmlTableView for the page panel
		internal WebBrowser WebBrowserControl; // currently selected browser control

		public HtmlTablePanel()
		{
			InitializeComponent();
		}

		private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			bool processed = PopupMobiusCommandNavigation.Process(e, Panel);
			return;
		}

		private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			return;
		}
	}
}
