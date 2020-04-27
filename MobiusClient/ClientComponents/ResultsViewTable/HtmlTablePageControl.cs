using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraTab;
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
/// HTML page control containing panel and controls
/// </summary>

	public partial class HtmlTablePageControl : DevExpress.XtraEditors.XtraUserControl
	{
		public HtmlTablePageControl()
		{
			InitializeComponent();
		}

		private void BackBut_Click(object sender, EventArgs e)
		{
			try { HtmlPagePanel.WebBrowser.GoBack(); }
			catch (Exception ex) { }
		}

		private void ForwardBut_Click(object sender, EventArgs e)
		{
			try { HtmlPagePanel.WebBrowser.GoForward(); }
			catch (Exception ex) { }
		}

		private void PrintBut_Click(object sender, EventArgs e)
		{
			HtmlPagePanel.WebBrowser.ShowPrintDialog();
		}

		private void ScrollToTop_Click(object sender, EventArgs e)
		{
			HtmlPagePanel.WebBrowser.Document.Body.ScrollIntoView(true);
		}

		private void ScrollToBottom_Click(object sender, EventArgs e)
		{
			HtmlPagePanel.WebBrowser.Document.Body.ScrollIntoView(false);
		}
	}
}
