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

namespace Mobius.SpotfireClient
{

/// <summary>
/// Control that contains a WebBrowser control and tools for viewing Spotfire analyses via the Web Player
/// </summary>

	public partial class SpotfirePageControl : DevExpress.XtraEditors.XtraUserControl
	{
		//internal ResultsViewProps View { get { return SpotfirePanel?.SVP?.ResultsViewProps; } } // currently active chart
		//internal QueryManager Qm { get { return SpotfirePanel?.RootQm; } } // QueryManager associated with currently active chart on the page

		public SpotfirePageControl()
		{
			InitializeComponent();

			Controls.Remove(ToolPanel);

			SpotfirePanel.Dock = DockStyle.Fill;
		}

		internal QueryManager RootQm // current QueryManager for the tab page
		{
			get
			{
				QueryResultsControl qrc = QueryResultsControl.GetQrcThatContainsControl(this.SpotfirePanel.WebBrowser);
				return qrc.CrvQm;
			}
		}

/// <summary>
/// Edit query
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void EditQueryBut_Click(object sender, EventArgs e)
		{
			if (ViewManager.TryCallCustomExitingQueryResultsCallback(SpotfirePanel, ExitingQueryResultsType.EditQuery)) return;

			CommandExec.ExecuteCommandAsynch("EditQuery");
			return;
		}

		private void OpenFromFileMenuItem_Click(object sender, EventArgs e)
		{
			return;
		}

		private void OpenFromLibraryMenuItem_Click(object sender, EventArgs e)
		{
			return;
		}

		private void SpotfirePageControl_Resize(object sender, EventArgs e)
		{
			return;
		}

		private void SpotfirePanel_Resize(object sender, EventArgs e)
		{
			return;
		}
	}
}
