using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Mobius.ClientComponents
{
/// <summary>
/// HTML Table View
/// </summary>

	public class HtmlTableView : ViewManager
	{
		internal HtmlTablePageControl HtmlPageControl;
		internal HtmlTablePanel HtmlPanel { get { return HtmlPageControl != null ? HtmlPageControl.HtmlPagePanel : null; } }
		internal WebBrowser WebBrowser { get { return HtmlPanel != null ? HtmlPanel.WebBrowser : null; } }

		/// <summary>
		/// Basic constructor
		/// </summary>

		public HtmlTableView()
		{
			ViewType = ResultsViewType.HtmlTable;
			Title = "HTML Table View";
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			HtmlPageControl = new HtmlTablePageControl();
			HtmlPanel.View = this; // link the html table control panel to this view
			HtmlPanel.Dock = DockStyle.Fill; // dock full grid panel

			RenderingControl = HtmlPanel;
			ConfigureCount = 0;

			return RenderingControl;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel viewPanel)
		{
			viewPanel.Controls.Clear(); // remove anything in there
			viewPanel.Controls.Add(HtmlPanel); // add it to the display panel

			InsertToolsIntoDisplayPanel();
			return;
		}

		/// <summary>
		/// Insert tools into display panel
		/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			Qrc.SetToolBarTools(HtmlPageControl.ToolPanel, BaseQuery.ViewScale); // show the proper tools and zoom
			return;
		}

		/// <summary>
		/// Render the view by configuring the control for the current view settings
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			WebBrowser.Navigate(ContentUri); 
			ConfigureCount++;
			return;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>

		public new void FreeResources()
		{
			RenderingControl = null;
			HtmlPageControl = null;
			return;
		}
	}
}
