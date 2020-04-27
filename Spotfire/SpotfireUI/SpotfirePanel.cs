using Mobius.ComOps;
using Mobius.Data;
//using Mobius.ServiceFacade;
//using Mobius.ClientComponents;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Mobius.SpotfireClient
{

	/// <summary>
	/// Panel containing the Spotfire Web Player content in a WebBrowser control
	/// </summary>

	public partial class SpotfirePanel : XtraUserControl, IDisposable
	{
		public SpotfireViewProps SVP; // Spotfire view we are interacting with

		public SpotfireApiClient SpotfireApiClient => SpotfireSession.SpotfireApiClient;  // API link to Spotfire session to interact with

		WindowsMessageFilter BrowserRtClickMessageFilter; // to catch rt-click within webbrowser

		//MessageSnatcher MessageSnatcher; // to catch rt-click within webbrowser (Spotfire WebBrowser rt-click menu still appears when using this)

		int SpotfireApiLogHeight = -1;

		static string SpotfireServer = null;
		static string SpotfireLibraryUrl = null;
		static bool SpotfireApiLogVisible = false;


		// References to containing control members

		//internal QueryManager RootQueryManager // current QueryManager for the panel
		//{
		//	get
		//	{
		//		QueryResultsControl qrc = QueryResultsControl.GetQrcThatContainsControl(WebBrowser);
		//		return qrc.CrvQm;
		//	}
		//}

		//internal QueryManager RootQm { get { return RootQueryManager; } }

		//internal DataTableManager Dtm
		//{ get { return (RootQm != null) ? RootQueryManager.DataTableManager : null; } }

		//internal MoleculeGridControl MoleculeGrid // molecule grid for selected data rows
		//{ get { return (RootQm != null) ? RootQm.MoleculeGrid : null; } }

		/// <summary>
		/// Constructor
		/// </summary>

		public SpotfirePanel()
		{
			InitializeComponent();

			if (SystemUtil.InDesignMode) return;

			WebBrowser wb = WebBrowser;

			//SetupMessageSnatcher();

			BrowserRtClickMessageFilter = 
				WindowsMessageFilter.CreateRightClickMessageFilter(WebBrowser, BrowserControlRightMouseButtonMessageReceived);

			SpotfireApiLogHeight = SpotfireApiLog.Height; // save height

			ShowSpotfireApiLog(SpotfireApiLogVisible); // don't show log initially

			return;
		}

		/// <summary>
		/// Callback for Rt-click picked up by Windows MessageFilter
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>

		private bool BrowserControlRightMouseButtonMessageReceived(int msg)
		{
			if (msg == WindowsMessage.WM_RBUTTONUP) // show menu on button up 
				DelayedCallback.Schedule(ShowVisContextMenu, null, 200); // schedule callback, need 200 ms to keep Spotfire selection rectangle from appearing

			return true; // say handled if down or up
		}

		//public void SetupMessageSnatcher()
		//{
		//	MessageSnatcher = new MessageSnatcher(WebBrowser);
		//	MessageSnatcher.RightMouseClickOccured += BrowserControlRightMouseClickOccured;
		//}

		/// <summary>
		/// Rt-click picked up by MessageSnatcher
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		//private void BrowserControlRightMouseClickOccured(object sender, EventArgs e)
		//{
		//	DelayedCallback.Schedule(ShowVisContextMenu, e, 200); // schedule callback, need 200 ms to keep Spotfire selection rectangle from appearing
		//	return;
		//}

		private void ShowVisContextMenu(object state)
		{
			Point p = WindowsHelper.GetMousePosition();

			// Setup the menu

			VisualMsx v = SpotfireSession.SpotfireApiClient?.GetActiveVisual();

			if (SpotfireToolbar.CanEditVisualProperties(v))
			{
				VisualPropertiesMenuItem.Visible = true;
			}

			else VisualPropertiesMenuItem.Visible = true;

			WebBrowserRtClickContextMenu.Show(p);

			return;
		}

		/// <summary>
		/// Starting navigation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Completed navigation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// 
		private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			if (SpotfireApiClient?.SWPC != null)
				SpotfireApiClient.SWPC.WebBrowser_Navigated(sender, e);


			return;
		}

		public void SetupControlMouseCapture() // no luck with Browser control
		{
			SplitContainerControl.Panel1.Capture = true; // 
			return;
		}

		public void SetupBrowserDocumentMouseCapture() // no luck with Browser control
		{
			WebBrowser.Document.MouseDown += new HtmlElementEventHandler(Document_MouseDown);
			WebBrowser.IsWebBrowserContextMenuEnabled = false;
			return;
		}

		void Document_MouseDown(object sender, HtmlElementEventArgs e)
		{
			if (e.MouseButtonsPressed == MouseButtons.Right)
			{
				return;
			}

			else return;
		}


		private void WebBrowser_Resize(object sender, EventArgs e)
		{
			if (WebBrowser.Visible == false) return;

			if (SpotfireApiClient?.SWPC != null)
				SpotfireApiClient.SWPC.InvokeScriptMethod("resize"); // Resize the WebPlayer div to fill available area
		}

		private void ShowVisualPropertiesDialog()
		{
			//WebBrowser.Document.MouseDown += new HtmlElementEventHandler(Document_MouseDown);

			VisualMsx v = SpotfireApiClient.GetActiveVisual();
			SpotfireToolbar.EditVisualProperties(v, SVP);

			return;
		}

		private void DataMenuItem_Click(object sender, EventArgs e)
		{
			SpotfireToolbar.EditDataProperties(SVP);
		}

		private void VisualPropertiesMenuItem_Click(object sender, EventArgs e)
		{
			VisualMsx v = SpotfireSession.SpotfireApiClient?.GetActiveVisual();
			SpotfireToolbar.EditVisualProperties(v, SVP);

			return;
		}

		// Reverse ApiLog visibility

		private void ShowSpotfireApiLogMenuItem_Click(object sender, EventArgs e)
		{
			SpotfireApiLogVisible = !SpotfireApiLogVisible;
			ShowSpotfireApiLog(SpotfireApiLogVisible);
		}

		public void ApiLogMessage(string msg)
		{
			if (!msg.EndsWith("\n")) msg += "\r\n";
			SpotfireApiLog.Text += msg;
			DelayedCallback.Schedule(ScrollToBottomOfText);
		}

		void ScrollToBottomOfText()
		{
			SpotfireApiLog.SelectionStart = Int32.MaxValue;
			SpotfireApiLog.ScrollToCaret();
		}

		public void ClearApiLog()
		{
			SpotfireApiLog.Text = "";
		}

		public void ShowSpotfireApiLog(bool visible)
		{
			if (visible)
				SplitContainerControl.PanelVisibility = SplitPanelVisibility.Both;

			else SplitContainerControl.PanelVisibility = SplitPanelVisibility.Panel1;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing) // remove any message filter
			{
				if (BrowserRtClickMessageFilter != null)
				{
					BrowserRtClickMessageFilter.RemoveFilter();
					BrowserRtClickMessageFilter = null;
				}
			}

			base.Dispose(disposing);
		}

	}
}
