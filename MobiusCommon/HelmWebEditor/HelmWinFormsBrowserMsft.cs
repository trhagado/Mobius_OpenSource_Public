﻿#define UseMsft // define this var is using the Microsoft WebBrowserControl rather than the CEF browser control 

#if UseMsft // if defined then this file will be compiled and used as the HelmWinFormsBrowser

using System; 
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Mobius.ComOps;

namespace Mobius.Helm
{

	/// <summary>
	/// Wrapper class for reference to HelmWinFormsBrowser from HelmControl
	/// </summary>

	public class HelmWinFormsBrowser : HelmWinFormsBrowserMsft
	{
		public HelmWinFormsBrowser(
			HelmControlMode renderMode,
			Control containingControl) : base(renderMode, containingControl) { }
	}

	/// <summary>
	/// Wrapper class for reference to HelmOffScreenBrowser from HelmControl
	/// </summary>

	public class HelmOffScreenBrowser : HelmWinFormsBrowserMsft
	{
		public HelmOffScreenBrowser() : base(HelmControlMode.OffScreenBitmap, null) { }
	}

	/// <summary>
	/// Mobius wrapper for Microsoft WebBrowserControl
	/// </summary>

	public partial class HelmWinFormsBrowserMsft
	{
		public WebBrowser Browser; // underlying browser
		WebBrowserWrapper BrowserWrapper = null;

		public WebBrowser OffScreenBrowser = null; // "OffScreen" browser

		public string Helm = null; // most-recently rendered helm

		public HelmControlMode RendererMode = HelmControlMode.BrowserViewOnly;
		public Control ContainingControl; // control that will contain the browser control
		public Size CurrentSize = new Size(); // size of most-recently rendered bitmap
		public int GetBitmapCount = 0;
		public int GetSvgCount = 0;
		//public int OnPaintCount = 0; // number of times OnPaint called

		public int Id = IdCounter++;
		public string IdText => " (BId: " + Id + ")";
		static int IdCounter = 0;

		public static int TempHtmlCount = 0;

		public static bool Debug => JavaScriptManager.Debug;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public HelmWinFormsBrowserMsft()
		{
			if (Debug) DebugLog.Message("HelmWinFormsBrowserMsft instance created" + IdText);
		}

		/// <summary>
		/// Constructor with setup parameters
		/// </summary>

		public HelmWinFormsBrowserMsft(
			HelmControlMode renderMode,
			Control containingControl)
		{
			RendererMode = renderMode;

			ContainingControl = containingControl;

			if (renderMode == HelmControlMode.OffScreenSvg) // create browser control and navigate to a blank page so the Browser.Document is defined
			{
				Browser = new WebBrowser(); // create the browser to contain the generated SVG
				Browser.ScrollBarsEnabled = false;
				Browser.ScriptErrorsSuppressed = JavaScriptManager.SuppressJavaScriptErrors;
				Browser.ObjectForScripting = new JavaScriptManager(this); // create ScriptManager that can call back to this JavaScriptInterface instance from within web page JavaScript

				WindowsMessageFilter BrowserRtClickMessageFilter; // to catch rt-click within Scilligence Webbrowser control

				BrowserRtClickMessageFilter =
					WindowsMessageFilter.CreateRightClickMessageFilter(Browser, BrowserControlRightMouseButtonMessageReceived);

				BrowserWrapper = new WebBrowserWrapper(Browser);
				BrowserWrapper.NavigateAndWait("about:blank"); // create initial blank page
			}

			if (Debug) DebugLog.Message("HelmWinFormsBrowserMsft instance created" + IdText);
			return;
		}

		/// <summary>
		///  Create WinForms.ChromiumWebBrowser and navigate to the initial page
		/// </summary>

		public void LoadInitialPage()
		{
			if (Browser != null) return;

			string url = "";

			if (RendererMode == HelmControlMode.OffScreenBitmap)
				url = "http://[server]/HelmWebEditor/MobiusHelmRenderer.htm";

			else if (RendererMode == HelmControlMode.BrowserViewOnly)
				url = "http://[server]/HelmWebEditor/MobiusHelmRenderer.htm";

			else if (RendererMode == HelmControlMode.BrowserEditor)
				url = "http://[server]/HelmWebEditor/MobiusHelmEditor.htm";

			else throw new Exception("Unexpected HelmControlMode: " + RendererMode);

			if (Debug) DebugLog.Message("Loading initial " + RendererMode + " mode page: " + url + IdText);

			Browser = new WebBrowser(); // create the browser 

			Browser.Location = new Point(0, 0);
			Browser.Size = new Size(256, 256);

			Browser.Dock = DockStyle.None;
			Browser.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
			Browser.Name = "WebBrowser";
			Browser.ScrollBarsEnabled = false;
			Browser.ScriptErrorsSuppressed = JavaScriptManager.SuppressJavaScriptErrors;
			Browser.ObjectForScripting = new JavaScriptManager(this); // create ScriptManager that can call back to this JavaScriptInterface instance from within web page JavaScript

			// The following three events are also in WebBrowserWrapper (not needed here)
			Browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Browser_DocumentCompleted);
			Browser.Navigated += new WebBrowserNavigatedEventHandler(this.WebBrowser_Navigated);
			Browser.Navigating += new WebBrowserNavigatingEventHandler(this.WebBrowser_Navigating);

			Browser.ProgressChanged += new WebBrowserProgressChangedEventHandler(Browser_ProgressChanged);
			Browser.StatusTextChanged += new EventHandler(WebBrowserStatusTextChanged);
			Browser.Resize += new EventHandler(WebBrowser_Resize);
			Browser.SizeChanged += new EventHandler(Browser_SizeChanged);

			if (ContainingControl != null)
			{
				Browser.Size = ContainingControl.Size;
				ContainingControl.Controls.Clear();
				ContainingControl.Controls.Add(Browser);
			}
			else Browser.Size = new Size(1024, 768);

			BrowserWrapper = new WebBrowserWrapper(Browser);
			BrowserWrapper.NavigateAndWait(url); // navigate to the initial page

			if (Debug) DebugLog.Message("Browser created, initial page loaded" + IdText);

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

		/// <summary>
		/// Load the HTML in a string into the browser
		/// </summary>
		/// <param name="html"></param>

		public void LoadHtmlString(string html)
		{
			string htmlFileName = TempFile.GetTempFileName(".htm");

			StreamWriter sw = new StreamWriter(htmlFileName);
			sw.Write(html);
			sw.Close();

			BrowserWrapper.Navigate(htmlFileName);
			return;
		}

#if false // doesn't refresh properly on resize, etc.
		public void LoadHtmlString(string html)
			{
				Browser.Navigate("about:blank");
			try
			{
				if (Browser.Document != null)
					Browser.Document.Write(string.Empty);
			}
			catch (Exception ex)
			{ string msg = ex.Message; } // do nothing with this

			Browser.DocumentText = html;
			return;
		}
#endif

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
			return;
		}

		private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			Browser.Document.Body.AttachEventHandler("ondblclick", Document_DoubleClick); // allow double-click to be handled
			return;
		}

		void Document_DoubleClick(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(OpenEditor);
		}

		private void WebBrowser_Resize(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Size of Browser changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Browser_SizeChanged(object sender, EventArgs e)
		{
			if (Browser?.Document == null) return; // just return if no document to resize yet

			if (Debug) DebugLog.Message("Browser_SizeChanged: " + Browser.Size.Width + ", " + Browser.Size.Height + IdText);

			ResizeRendering();
			return;
		}

		/// <summary>
		/// Refresh the Helm renderer to redraw the content
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>

		public void ResizeRendering()
		{
			string script = "";

			if (RendererMode != HelmControlMode.BrowserEditor) // all modes except editor
			{
				script += "jsd.setSize(" + Browser.Width + "," + Browser.Height + ");";
				script += "jsd.refresh();"; // redraws the structure
				JavaScriptManager.ExecuteScript(this, script);
				return;
			}

			else // editor mode
			{
				script += "app.resizeWindow();";
				//script += "app.refresh();"; // this seems to be slower than the resizeWindow call above
				JavaScriptManager.ExecuteScript(this, script);
				return;
			}
		}

		private void Browser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
		{
			return;
		}

		private void WebBrowserStatusTextChanged(object sender, EventArgs e)
		{
			return;
		}

		private void WaitForReadyStateComplete()
		{
			while (Browser.ReadyState != WebBrowserReadyState.Complete)
			{
				Application.DoEvents();
			}
		}

		/// <summary>
		/// Open the HelmEditor for the current Helm string
		/// </summary>

		public void OpenEditor()
		{
			HelmEditorDialog editor = new HelmEditorDialog();

			string newHelm = editor.Edit(Helm);
			if (newHelm != null)
			{
				Helm = newHelm;
				SetHelmAndRender(newHelm); // display new helm
			}
		}

		public void SetHelm(string helm)
		{
			SetHelmAndRender(helm, Size.Empty);
		}

		/// <summary>
		/// Store the helm in the Scilligence component and render it
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>

		public void SetHelmAndRender(
			string helm,
			Size size = new Size())
		{
			string script = "";

			Helm = helm;

			if (RendererMode != HelmControlMode.BrowserEditor) // all modes except editor
			{
				//if (!String.IsNullOrWhiteSpace(helm)) // don't do this check, need to set if blank to clear out existing structure
				{
					script += "jsd.setHelm(\"" + helm + "\");"; // var jsd = new JSDraw(...) is JSDraw canvas object JavaScript variable
																											//JavaScriptManager.ExecuteScript(this, script); script = ""; // debug - one command at a time
				}

				if (!size.IsEmpty)
				{
					script += "jsd.setSize(" + size.Width + "," + size.Height + ");";

					//JavaScriptManager.ExecuteScript(this, script); script = ""; // debug - one command at a time
				}

				script += "jsd.refresh();"; // redraws the structure
				JavaScriptManager.ExecuteScript(this, script);
				return;
			}

			else // editor mode
			{
				script += // set the helm allowing for blank helm to clear the structure
					"app.canvas.setHelm(\"" + helm + "\");"; // var app = new scil.helm.App(), app.canvas is JSDraw canvas object

				//if (!size.IsEmpty)
				//	script += "app.canvas.setSize(" + scaledCanvasWidth + "," + scaledCanvasHeight + ");";

				script += "app.canvas.refresh();"; // redraws the structure
				JavaScriptManager.ExecuteScript(this, script);
				return;
			}

		}

		/// <summary>
		/// Get molfile form of structure
		/// </summary>
		/// <param name="outputFormat">molfile, molfile2000 or molfile3000</param>
		/// <returns></returns>

		public string GetMolfile(
		string outputFormat = "molfile")
		{
			if (Debug) DebugLog.Message("GetMolfile Entered" + IdText);

			Stopwatch sw = Stopwatch.StartNew();

			// Define the JavaScript function to get the molfile for the helm

			//string script = @"
			//	function getMolfileString() {
			//		var data = { jsdraw: jsd.getXml(), outputformat: 'molfile' };
			//		var result = scil.Utils.ajaxwait( // call ajax to invoke the service 
			//			JSDrawServices.url + '?cmd=jsdraw.helm2mol', // the url and command
			//			data); // the data); i.e. parameters for service call
			//		return result == null ? null : result.output 
			//	}
			//";

			//script = script.Replace("<outputFormat>", outputFormat);

			//JavaScriptManager.ExecuteJavaScript(Browser, script);

			string molfile = JavaScriptManager.CallFunction(this, "getMolfileString"); // call asynch function to get the molfile
			string txt = (molfile != null ? molfile.Length.ToString() : "<null>");
			if (Debug) DebugLog.StopwatchMessage("GetMolfileString Complete: " + txt + ", Time: ", sw);

			return molfile;
		}

		/************************************************************************************
		 * The code above is based on the following snippet from: Scilligence.JSDraw2.Pro.js
		 ************************************************************************************ 
		  copyAs: function(b) {
        if (b != "helm" && this.hasHelmNodes()) {
            var c = {
                jsdraw: this.getXml(),
                outputformat: b
            };
            JSDraw2.JSDrawIO.callWebservice("jsdraw.helm2mol", c, function(a) {
                scil.Clipboard.copy(a.output)
            }, {
                showprogress: true
            });
            return
        }
        var s = null;
        switch (b) {
            case "molfile":
                s = this.getMolfile();
                break;
            case "molfile2000":
                s = this.getMolfile(false);
                break;
            case "molfile3000":
                s = this.getMolfile(true);
                break;
            case "smiles":
                s = this.getSmiles(true);
                break;
            case "helm":
                s = this.getHelm();
                break 
        }
        if (scil.Utils.isNullOrEmpty(s)) {
            scil.Utils.alert("Nothing placed on clipboard");
            return
        }
        scil.Clipboard.copy(s)
    }
	
	callWebservice: function(a, b, c, d) {
        if (JSDrawServices.url == null || JSDrawServices.url == "") scil.Utils.alert("JSDraw web service is not available");
        else scil.Utils.ajax(JSDrawServices.url + "?cmd=" + a, c, b, d)
    },
		 */

		public string GetSmiles()
		{
			ExecuteJavaScript("function getSmilesString() { return jsd.getSmiles(); };"); // define function to get the Smiles

			string smiles = CallJavaScriptMethod("getSmilesString"); // call the function to get the smiles
			return smiles;
		}

		public string GetSequence()
		{
			ExecuteJavaScript("function getSequenceString() { return jsd.getSequence(); };"); // define function to get the Sequence

			string seq = CallJavaScriptMethod("getSequenceString"); // call the function to get the Sequence
			return seq;
		}

		/// <summary>
		/// Get a bitmap of the specified HELM in the specified size
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>
		/// <returns></returns>

		public Bitmap GetBitmap(
			string helm,
			Size size)
		{
			try
			{
				Bitmap bm = null;

				LoadInitialPage(); // be sure initial page is loaded

				if (Debug) DebugLog.Message("GetBitmap Entered: " + helm + ", " + size + IdText);

				Stopwatch sw = Stopwatch.StartNew();

				string key = string.Format("GetHelmBitmap({0}, {1}, {2})", helm, size.Width, size.Height);
				if (CacheMx<Bitmap>.TryGetValue(key, out bm))
					return bm;

				SetHelmAndRender(helm, size); // render the structure

				bm = new Bitmap(Browser.Width, Browser.Height);
				Rectangle rt = new Rectangle(0, 0, Browser.Width, Browser.Height);
				Browser.DrawToBitmap(bm, rt);

				if (bm.Width > 1)
					CacheMx<Bitmap>.Add(key, bm);

				GetBitmapCount++;

				if (Debug) DebugLog.StopwatchMessage("GetBitmap Complete, Time: ", sw);

				return bm;
			}

			catch (Exception ex)
			{
				return new Bitmap(1, 1); // failed for some reason
			}
		}

		/// <summary>
		/// Get SVG of the specified HELM
		/// </summary>
		/// <param name="helm"></param>
		/// <returns></returns>

		public string GetSvg(
			string helm)
		{
			return GetSvg(helm, new Size(1024, 768)); // default size (too small results in poor quality svg)
		}

		/// <summary>
		/// Get SVG of the specified HELM
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>
		/// <returns></returns>

		public string GetSvg(
			string helm,
			Size size)
		{
			string svg = null;

			string key = string.Format("GetHelmSvg({0}, {1}, {2})", helm, size.Width, size.Height);
			if (CacheMx<string>.TryGetValue(key, out svg))
				return svg;

			LoadInitialPage(); // be sure initial page is loaded

			if (Debug) DebugLog.Message("GetSvg Entered: " + helm + ", " + size + IdText);

			Stopwatch sw = Stopwatch.StartNew();

			SetHelmAndRender(helm, size); // render the structure

			ExecuteJavaScript("function getSvgString() { return jsd.getSvg(); };"); // define function to get the SVG

			svg = CallJavaScriptMethod("getSvgString"); // call the function to get the SVG

			if (Lex.IsDefined(svg))
				CacheMx<string>.Add(key, svg);

			GetSvgCount++;


			if (Debug) DebugLog.StopwatchMessage("GetSvg Complete, Length: " + Lex.GetStringLength(svg) + ", Time: ", sw);

			return svg;
		}

		/// <summary>
		/// ExecuteJavaScript
		/// </summary>
		/// <param name="script"></param>

		void ExecuteJavaScript(string script)
		{
			LoadInitialPage(); // be sure initial page is loaded
			JavaScriptManager.ExecuteScript(this, script);
			return;
		}

		/// <summary>
		/// CallJavaScriptMethod
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>

		string CallJavaScriptMethod(
			string methodName,
			params string[] args)
		{
			LoadInitialPage(); // be sure initial page is loaded
			string result = JavaScriptManager.CallFunction(this, methodName, args);
			return result;
		}

		/// <summary>
		/// Callback for Rt-click picked up by Windows MessageFilter
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>

		private bool BrowserControlRightMouseButtonMessageReceived(int msg)
		{
			if (msg == WindowsMessage.WM_RBUTTONUP) // show menu on button up 
				DelayedCallback.Schedule(ShowHelmBrowserContextMenu, null, 200); // schedule callback, need 200 ms to keep Spotfire selection rectangle from appearing

			return true; // say handled if down or up
		}

		private void ShowHelmBrowserContextMenu(object state)
		{
			Point p = WindowsHelper.GetMousePosition();

			// Setup the menu

			//VisualMsx v = SpotfireSession.SpotfireApiClient?.GetActiveVisual();

			//if (SpotfireToolbar.CanEditVisualProperties(v))
			//{
			//	VisualPropertiesMenuItem.Visible = true;
			//}

			//else VisualPropertiesMenuItem.Visible = true;

			//WebBrowserRtClickContextMenu.Show(p);

			return;
		}


	}

	/// <summary>
	/// Static methods for WebBrowserControl
	/// </summary>

	/// <summary>
	/// Script manager class allows script to call back into C# code
	/// </summary>

	[ComVisible(true)]
	public class JavaScriptManager
	{
		public static bool SuppressJavaScriptErrors = true;

		HelmWinFormsBrowserMsft HelmBrowser;

		public JavaScriptManager(HelmWinFormsBrowserMsft helmBrowser) // create a script manager and reference it back to us
		{
			HelmBrowser = helmBrowser;
		}
		public static bool Debug = true;

		/// <summary>
		/// Execute JavaScript that return a string result
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>

		public static string ExecuteStringResultJavaScript(
			HelmWinFormsBrowser helmBrowser,
			string script)
		{
			object r = ExecuteScript(helmBrowser, script);
			if (r != null)
				return r.ToString();

			else return null;
		}

		/// <summary>
		/// Execute a chunk of JavaScript, wait for completion & return response
		/// </summary>
		/// <param name="script"></param>

		public static object ExecuteScript(
			HelmWinFormsBrowserMsft helmBrowser,
			string script)
		{
			//if (DebugMx.True) return new JavascriptResponse(); // disable - debug

			Stopwatch sw = Stopwatch.StartNew();

			if (helmBrowser?.Browser?.Document == null)
				AssertMx.IsNotNull(helmBrowser?.Browser?.Document, "HelmBrowser?.Browser?.Document");

			WebBrowser browser = helmBrowser.Browser;

			if (Debug) DebugLog.Message("Script: " + script + helmBrowser.IdText);

			object result = browser.Document.InvokeScript("execScript", new Object[] { script, "JavaScript" });

			if (Debug) DebugLog.StopwatchMessage("Script complete, Time: ", sw);

			return result;
		}

		/// <summary>
		/// Call a function in a script
		/// </summary>
		/// <param name="jsMethodName"></param>
		/// <param name="args">Itemized list of args, not an array</param>
		/// <returns>Function result</returns>

		public static string CallFunction(
				HelmWinFormsBrowserMsft helmBrowser,
				string jsMethodName,
				params string[] args)
		{
			object resultObj = null;

			Stopwatch sw = Stopwatch.StartNew();

			WebBrowser browser = helmBrowser.Browser;
			if (browser == null || browser.IsDisposed || browser.Document == null) return null;

			string funcCallString = jsMethodName + "(" + Lex.ArrayToCsvString(args) + ")";

			if (Debug) DebugLog.Message("Script function call: " + funcCallString + helmBrowser.IdText);

			if (args == null || args.Length == 0) // no args
				resultObj = browser.Document.InvokeScript(jsMethodName);

			else // convert args string array to object array
			{
				object[] oArgs = new object[args.Length];
				args.CopyTo(oArgs, 0);
				resultObj = browser.Document.InvokeScript(jsMethodName, args);
			}

			string resultString = (resultObj != null ? resultObj.ToString() : null);

			if (Debug) DebugLog.StopwatchMessage("Script function call complete, Time: ", sw);

			return resultString;
		}

		/// <summary>
		/// Called by JavaScript when molfile has been retrieved by JavaScript call to JSDraw
		/// </summary>
		/// <param name="obj"></param>

		public void GetMolfileStringResult(object obj)
		{
			if (obj == null) obj = "";
			string molfile = obj.ToString();

			//DelayedCallback.Schedule(HelmBrowser.OpenEditor);
		}


		/// <summary>
		/// Called by JavaScript when page is clicked, normally editor is opened
		/// </summary>
		/// <param name="obj"></param>

		public void DocumentClickedCallback(object obj)
		{
			return;

			//if (obj == null) obj = "";
			//string elementClicked = obj.ToString();

			//DelayedCallback.Schedule(HelmBrowser.OpenEditor);
		}
	}
}

#endif // end of surrounding conditional to include or exclude this class

