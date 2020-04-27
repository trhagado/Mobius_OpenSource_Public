//#define UseCef // define this var is using the CEF browser control rather than the Microsoft WebBrowserControl 

#if UseCef // if defined then this file will be compiled and used as the HelmWinFormsBrowser

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

// Uses NuGet packages CefSharp.Common, CefSharp.WinForms, CefSharp.OffScreen (v75.1.142) and cef.redist.x86 (v75.1.14)
using CefSharp.WinForms;
using CefSharp.OffScreen;
using CefSharp;

using Mobius.ComOps;

namespace Mobius.Helm
{

	/// <summary>
	/// Wrapper class for reference to HelmWinFormsBrowser from HelmControl
	/// </summary>

	public class HelmWinFormsBrowser : HelmWinFormsBrowserCef
	{
		public HelmWinFormsBrowser(
			HelmControlMode renderMode,
			Control containingControl) : base(renderMode, containingControl) { }
	}

	/// <summary>
	/// Mobius wrapper for CefSharp.WinForms.ChromiumWebBrowser
	/// </summary>

	public partial class HelmWinFormsBrowserCef
	{
		public CefSharp.WinForms.ChromiumWebBrowser Browser = null; // underlying OffScreen browser

		public string Helm = null; // most-recently rendered helm

		public HelmControlMode RendererMode = HelmControlMode.BrowserViewOnly;
		public Control ContainingControl; // control that will contain the browser control
		public Size CurrentSize = new Size(); // size of most-recently rendered bitmap
		public int GetBitmapCount = 0;
		public int OnPaintCount = 0; // number of times OnPaint called

		ManualResetEvent BrowserInitializedEvent = new ManualResetEvent(false);
		ManualResetEvent PageLoadedEvent = new ManualResetEvent(false);

		public static bool Debug => WebBrowserControlMx.Debug;

		/// <summary>
		/// Constructor
		/// </summary>

		public HelmWinFormsBrowserCef(
			HelmControlMode renderMode,
			Control containingControl)
		{
			CefMx.InitializeCef();

			RendererMode = renderMode;

			if (renderMode == HelmControlMode.OffScreenSvg)
			{
				Browser = new CefSharp.WinForms.ChromiumWebBrowser(""); // Create the Chromium browser to contain the generated SVG
				Browser.FrameLoadEnd += OnFrameLoadEnd; // used to remove scroll bars
			}

			ContainingControl = containingControl;

			if (Debug) DebugLog.Message("CefWinFormsMx instance created");
			return;
		}

		/// <summary>
		/// Initialize the renderer
		/// </summary>

		public void Initialize()
		{
			LoadInitialPage();
		}

		/// <summary>
		///  Create WinForms.ChromiumWebBrowser and navigate to the initial page
		/// </summary>

		public void LoadInitialPage()
		{
			if (Browser != null) return;

			string url = "";

			if (RendererMode == HelmControlMode.BrowserViewOnly) // get proper initial page
				url = "http://[server]:8080/hwe/Mobius/HelmRenderer.htm";

			else // edit mode
				url = "http://[server]:8080/hwe/Mobius/HelmEditor.htm";

			if (Debug) DebugLog.Message("Loading initial " + RendererMode + " mode page: " + url);

			PageLoadedEvent.Reset();
			Browser = new CefSharp.WinForms.ChromiumWebBrowser(url); // Create the Chromium browser.

			//Browser.Dock = DockStyle.Fill; // will fill containing control

			Browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
			Browser.FrameLoadStart += OnFrameLoadStart;
			Browser.LoadingStateChanged += OnBrowserLoadingStateChanged;
			Browser.AddressChanged += OnAddressChanged;
			Browser.StatusMessage += OnStatusMessage;
			Browser.ConsoleMessage += OnConsoleMessage;
			Browser.FrameLoadEnd += OnFrameLoadEnd;
			Browser.TitleChanged += OnTitleChanged;
			Browser.LoadError += OnLoadError;
			Browser.JavascriptMessageReceived += OnJavascriptMessageReceived;
			Browser.Paint += OnPaint;
			Browser.SizeChanged += Browser_SizeChanged;

			if (ContainingControl != null) 
			{
				Browser.Location = new Point(0, 0);
				Browser.Size = ContainingControl.Size;
				Browser.Dock = DockStyle.None;
				Browser.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);

				ContainingControl.Controls.Clear();
				ContainingControl.Controls.Add(Browser);
			}

			PageLoadedEvent.WaitOne(); // wait for page loading to complete

			if (Debug) DebugLog.Message("Browser created, initial page loaded");

			return;
		}

		public void LoadPage(string url)
		{
			LoadInitialPage(); // be sure initial page is loaded
			Browser.Load(url);
			return;
		}

		/// <summary>
		/// Load the browser with html to display the supplied SVG string
		/// </summary>
		/// <param name="svgString"></param>

		public void LoadSvgString(string svgString)
		{
			string html = SvgUtil.CreateHtlmWithSvgCenteredOnPage(svgString);
			LoadHtmlString(html);
		}


		public void LoadHtmlString(string html)
		{
			Browser.Load(html);
			return;
		}

		/// Event called after the underlying CEF browser instance has been created.It's
		/// important to note this event is fired on a CEF UI thread, which by default is
		/// not the same as your application UI thread. It is unwise to block on this thread
		/// for any length of time as your browser will become unresponsive and/or hang..
		/// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.

		private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
		{
			if (Debug) DebugLog.Message("IsBrowserInitializedChanged event");

			var browser = ((CefSharp.WinForms.ChromiumWebBrowser)sender);
			//CefMx.ShowDevTools(browser); // debug

			//this.InvokeOnUiThreadIfRequired(() => b.Focus());

			return;
		}

		private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
		{
			if (Debug) DebugLog.Message("Frame.Name: " + e.Frame.Name);

			return;
		}

		/// <summary>
		/// BrowserLoadingStateChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OnBrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			if (Debug) DebugLog.Message("IsLoading: " + e.IsLoading);

			// Check to see if loading is complete - this event is called twice, one when loading starts
			// second time when it's finished (rather than an iframe within the main frame).

			if (e.IsLoading) return; // just return if still loading

			// Remove the load event handler, because we only want one snapshot of the initial page.
			//CefOffScreenBrowser.LoadingStateChanged -= BrowserLoadingStateChanged;

			PageLoadedEvent.Set();
			return;
		}

		private void OnAddressChanged(object sender, AddressChangedEventArgs e)
		{
			if (Debug) DebugLog.Message("Address: " + e.Address);
			return;
		}

		private void OnStatusMessage(object sender, StatusMessageEventArgs e)
		{
			if (Debug) DebugLog.Message("Value: " + e.Value.ToString());
			return;
		}

		private void OnConsoleMessage(object sender, ConsoleMessageEventArgs e)
		{
			if (Debug) DebugLog.Message("Message: " + e.Message);
			return;
		}

		public void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
		{
			if (e.Frame.IsMain) // get rid of scroll bars
				e.Browser.MainFrame.ExecuteJavaScriptAsync("document.body.style.overflow = 'hidden'");

			if (Debug) DebugLog.Message("HttpStatusCode: " + e.HttpStatusCode);
			return;
		}

		private void OnTitleChanged(object sender, TitleChangedEventArgs e)
		{
			if (Debug) DebugLog.Message("Title: " + e.Title);
			return;
		}

		private void OnLoadError(object sender, LoadErrorEventArgs e)
		{
			if (Debug) DebugLog.Message("ErrorText: " + e.ErrorText);
			return;
		}


		private void OnPaint(object sender, PaintEventArgs e)
		{
			if (Debug) DebugLog.Message("OnPaint");
			OnPaintCount++;

			return;
		}

		private void Browser_SizeChanged(object sender, EventArgs e)
		{
			if (Debug) DebugLog.Message("Browser_SizeChanged: " + Browser.Size.Width + ", " + Browser.Size.Height);

			//if (Browser != null)
			//{
			//	if (Browser.Location.X != 0 || Browser.Location.Y != 0)
			//		Browser.Location = new Point(0, 0);

			//	if (Browser.Size != this.Size)
			//		Browser.Size = this.Size;

			//	SetHelmAndSize(Helm, this.Size);
			//	Browser.Refresh();
			//}

			//if (OffScreenRenderer != null)
			//{
			//	SetHelmAndSize(Helm, this.Size);
			//}
		}

		private void OnJavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
		{
			if (Browser.Parent == null) return;
			Browser.Parent.Invoke(new SimpleDelegate(OpenEditorWithDelay)); // Open editor on UI thread
			if (Debug) DebugLog.Message("Message: " + e.Message);
			return;
		}

		void OpenEditorWithDelay()
		{
			DelayedCallback.Schedule(OpenEditor);
		}

		/// <summary>
		/// Open the HelmEditor for the current Helm string
		/// </summary>

		void OpenEditor()
		{
			HelmEditorDialog editor = new HelmEditorDialog();

			string newHelm = editor.Edit(Helm);
			if (newHelm != null)
			{
				Helm = newHelm;
				SetHelmAndRender(newHelm); // display new helm
			}
		}

		public void SetHelmAndRender(
			string helm,
			Size size = new Size())
		{
			string script = "";

			Helm = helm;

			if (RendererMode == HelmControlMode.BrowserViewOnly)
			{
				if (!String.IsNullOrWhiteSpace(helm))
					script += "jsd.setHelm(\"" + helm + "\");"; // var jsd = new JSDraw(...) is JSDraw canvas object JavaScript variable

				if (!size.IsEmpty)
					script += "jsd.setSize(" + size.Width + "," + size.Height + ");";

				script += "jsd.refresh();"; // redraws the structure
			}

			else // editor mode
			{
				script += // set the helm allowing for blank helm to clear the structure
					"app.canvas.setHelm(\"" + helm + "\");"; // var app = new scil.helm.App(), app.canvas is JSDraw canvas object

				//if (!size.IsEmpty)
				//	script += "app.canvas.setSize(" + scaledCanvasWidth + "," + scaledCanvasHeight + ");";

				script += "app.canvas.refresh();"; // redraws the structure
			}

			CefMx.ExecuteJavaScript(Browser, script);
			return;
		}

		/// <summary>
		/// Execute JavaScript that returns a string result
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>

		public string ExecuteStringResultJavaScript(
			string script)
		{
			return CefMx.ExecuteStringResultJavaScript(Browser, script);
		}

		/// <summary>
		/// GetBitMap
		/// </summary>
		/// <returns></returns>

		public Bitmap GetBitMap()
		{
			Bitmap bm = new Bitmap(Browser.Width, Browser.Height);
			Rectangle rt = new Rectangle(0, 0, Browser.Width, Browser.Height);
			Browser.DrawToBitmap(bm, rt);
			return bm;
		}
	}
}

#endif // end of surrounding conditional to include or exclude this class
