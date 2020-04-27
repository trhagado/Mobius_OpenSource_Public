//#define UseCef // define this var is using the CEF browser control rather than the Microsoft WebBrowserControl 

#if UseCef // if defined then this file will be compiled and used as the HelmWinFormsBrowser

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

// Uses NuGet packages CefSharp.Common, CefSharp.WinForms, CefSharp.OffScreen (v75.1.142) and cef.redist.x86 (v75.1.14)
using CefSharp.OffScreen;
using CefSharp;

using Mobius.ComOps;

namespace Mobius.Helm
{

	/// <summary>
	/// Wrapper class for reference form HelmControl
	/// </summary>

	public class HelmOffScreenBrowser : HelmOffScreenBrowserCef
	{
		public HelmOffScreenBrowser() : base() { }
	}

	/// <summary>
	/// Wrapper class for low level Cef browser functions
	/// </summary>

	public class WebBrowserControlMx : CefMx { }

		/// <summary>
		/// Mobius wrapper for CefSharp.OffScreen.ChromiumWebBrowser
		/// </summary>

		public class HelmOffScreenBrowserCef
	{

		public CefSharp.OffScreen.ChromiumWebBrowser OffScreenBrowser = null; // underlying OffScreen browser

		public string Helm = null; // most-recently rendered helm

		public HelmControlMode RenderMode = HelmControlMode.BrowserViewOnly;
		public Size CurrentSize = new Size(); // size of most-recently rendered bitmap
		public int GetBitmapCount = 0;
		public int GetSvgCount = 0;
		public int OnPaintCount = 0; // number of times OnPaint called

		ManualResetEvent BrowserInitializedEvent = new ManualResetEvent(false);
		ManualResetEvent PageLoadedEvent = new ManualResetEvent(false);

		public static bool Debug => WebBrowserControlMx.Debug;

		/// <summary>
		/// Constructor
		/// </summary>

		public HelmOffScreenBrowserCef()
		{
			CefMx.InitializeOffScreenCef();

			if (Debug) DebugLog.Message("CefOffScreenMx instance created");
			return;
		}

		/// <summary>
		/// Initialize the renderer
		/// </summary>
		/// 
		public void Initialize()
		{
			LoadInitialPage();
		}

		/// <summary>
		///  Create OffScreen.ChromiumWebBrowser and navigate to the initial page
		/// </summary>

		public void LoadInitialPage()
		{
			const string url = "http://[server]:8080/hwe/Mobius/HelmOffScreenRenderer.htm";
			//const string url = "https://[server]:---8443/hwe/Mobius/HelmOffScreenRenderer.htm"; // htttps

			if (OffScreenBrowser != null) return; // just return if already initialized

			if (Debug) DebugLog.Message("Creating offscreen browser and loading initial page: " + url);

			CreateOffScreenBrowser();

			NavigateTo(url); // load the initial page

			if (Debug) DebugLog.Message("Offscreen browser created, initial page loaded");

			return;
		}

		void CreateOffScreenBrowser()
		{
			BrowserInitializedEvent.Reset();
			OffScreenBrowser = new CefSharp.OffScreen.ChromiumWebBrowser(); // Create the offscreen Chromium browser.
			OffScreenBrowser.Size = Screen.PrimaryScreen.Bounds.Size; // be sure big enough for full screen display

			OffScreenBrowser.BrowserInitialized += BrowserInitializedEventHandler;
			OffScreenBrowser.FrameLoadStart += FrameLoadStartEventHandler;
			OffScreenBrowser.LoadingStateChanged += BrowserLoadingStateChanged;
			OffScreenBrowser.AddressChanged += AddressChangedEventHandler;
			OffScreenBrowser.StatusMessage += StatusMessageEventHandler;
			OffScreenBrowser.ConsoleMessage += ConsoleMessageEventHandler;
			OffScreenBrowser.FrameLoadEnd += FrameLoadEndEventHandler;
			OffScreenBrowser.TitleChanged += TitleChangedEventHandler;
			OffScreenBrowser.LoadError += LoadErrorEventHandler;
			OffScreenBrowser.JavascriptMessageReceived += JavascriptMessageReceivedEventHandler;
			OffScreenBrowser.Paint += OnPaintEventHandler;

			BrowserInitializedEvent.WaitOne(); // wait for initialization to complete

			if (Debug) DebugLog.Message("Offscreen browser created");
			return;
		}

		private void BrowserInitializedEventHandler(object sender, EventArgs e)
		{
			if (Debug) DebugLog.Message("Browser Initialized");

			var browser = ((CefSharp.OffScreen.ChromiumWebBrowser)sender);
			//CefMx.ShowDevTools(browser); // debug

			//CefOffScreenBrowser.BrowserInitialized -= BrowserInitializedEventHandler;

			BrowserInitializedEvent.Set();
			return;
		}

		/// <summary>
		/// Navigate to a URL and wait for page loading to complete
		/// </summary>
		/// <param name="url"></param>

		public void NavigateTo(string url)
		{
			if (Debug) DebugLog.Message("Loading page: " + url);

			//WebBrowser.Size = new Size((int)(800 + 1.25), (int)(400 * 1.25));

			PageLoadedEvent.Reset();
			OffScreenBrowser.Load(url);

			PageLoadedEvent.WaitOne(); // wait for page to complete loading

			Thread.Sleep(500); // Give the browser a little time to render

			if (Debug) DebugLog.Message("Loading complete");

			return;
		}

		/// <summary>
		/// BrowserLoadingStateChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			if (Debug) DebugLog.Message("BrowserLoadingStateChanged - IsLoading: " + e.IsLoading);

			// Check to see if loading is complete - this event is called twice, one when loading starts
			// second time when it's finished (rather than an iframe within the main frame).

			if (e.IsLoading) return; // just return if still loading

			PageLoadedEvent.Set();
			return;
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
			Bitmap Bitmap = null;
			ManualResetEvent GetBitmapEvent = new ManualResetEvent(false);

			Initialize();

			if (Debug) DebugLog.Message("GetBitmap Entered: " + helm + ", " + size);

			Stopwatch sw = Stopwatch.StartNew();

			Helm = helm;
			CurrentSize = size;

			//if (DebugMx.True) return new Bitmap(1,1);

			int initialOnPaintCount = OnPaintCount; // save current paint count so we can tell when page has been re-rendered with new image

			string script = "";

			if (!String.IsNullOrWhiteSpace(helm))
				script += "jsd.setHelm(\"" + helm + "\");";

			if (!size.IsEmpty)
				script += "jsd.setSize(" + size.Width + "," + size.Height + ");";

			script += "jsd.refresh();"; // redraws the structure

			Task<JavascriptResponse> scriptTask = OffScreenBrowser.EvaluateScriptAsync(script); // start the script async

			// Wait for script completion and then get the screenshot and bitmap

			scriptTask.ContinueWith(t =>
			{
				while (OnPaintCount == initialOnPaintCount) // wait until paint event occurs that renders the new bitmap
					Thread.Sleep(10);

				//Thread.Sleep(2000); //Give the browser a little time to render

				var task = OffScreenBrowser.ScreenshotAsync(ignoreExistingScreenshot: false); // start the screenshot async, should get new bitmap

				// Wait for screenshot completion and get bitmap
				task.ContinueWith(x =>
				{
					Bitmap resultBitmap = task.Result;

					if (!size.IsEmpty)
					{
						Rectangle section = new Rectangle(0, 0, size.Width, size.Height);
						section.Intersect(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height)); // be sure section not larger than result
						if (section.Height > 0 && section.Width > 0)
							Bitmap = resultBitmap.Clone(section, resultBitmap.PixelFormat);
						else Bitmap = new Bitmap(1, 1); // create minimal empty bitmap
					}

					else Bitmap = new Bitmap(resultBitmap);

					// We no longer need the Bitmap.
					// Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
					resultBitmap.Dispose();

					GetBitmapEvent.Set();

				}, TaskScheduler.Default);
			});

			GetBitmapEvent.WaitOne(); // wait for the bitmap to be retrieved

			GetBitmapCount++;

			if (Debug) DebugLog.StopwatchMessage("GetBitmap Complete, Time: ", sw);

			return Bitmap;
		}

		/// <summary>
		/// Get SVG of the specified HELM in the specified size
		/// </summary>
		/// <param name="helm"></param>
		/// <param name="size"></param>
		/// <returns></returns>

		public string GetSvg(
			string helm,
			Size size)
		{
			ManualResetEvent GetSvgEvent = new ManualResetEvent(false);
			string svg = null;

			Initialize();
			
			if (Debug) DebugLog.Message("GetSvg Entered: " + helm + ", " + size);

			Stopwatch sw = Stopwatch.StartNew();

			Helm = helm;
			CurrentSize = size;

			int initialOnPaintCount = OnPaintCount; // save current paint count so we can tell when page has been re-rendered with new image

			string script = "";

			if (!String.IsNullOrWhiteSpace(helm))
				script += "jsd.setHelm(\"" + helm + "\");";

			if (!size.IsEmpty)
			{
				int width = size.Width;
				int height = size.Height;
				if (height <= 0) height = width;
				script += "jsd.setSize(" + width + "," + height + ");";
			}

			script += "jsd.refresh();"; // redraws the structure

			script += "jsd.getSvg();";

			svg = CefMx.ExecuteStringResultJavaScript(OffScreenBrowser, script);

			GetSvgCount++;

			if (Debug) DebugLog.StopwatchMessage("GetSvg Complete, Time: ", sw);

			return svg;
		}

		/// <summary>
		/// Execute JavaScript that return a string result
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>

		public static string ExecuteStringResultJavaScript(
			IWebBrowser browser,
			string script)
		{
			return CefMx.ExecuteStringResultJavaScript(browser, script);
		}

		private void FrameLoadStartEventHandler(object sender, FrameLoadStartEventArgs e)
		{
			if (Debug) DebugLog.Message("Frame.Name: " + e.Frame.Name);

			return;
		}

		private void AddressChangedEventHandler(object sender, AddressChangedEventArgs e)
		{
			if (Debug) DebugLog.Message("Address: " + e.Address);
			return;
		}

		private void StatusMessageEventHandler(object sender, StatusMessageEventArgs e)
		{
			if (Debug) DebugLog.Message("Status Message, Value: " + e.Value);
			return;
		}

		private void ConsoleMessageEventHandler(object sender, ConsoleMessageEventArgs e)
		{
			if (Debug) DebugLog.Message("Console Message: " + e.Message + " (" + e.Level + "), " + ", Source: " + e.Source + "." + e.Line);
			return;
		}

		private void FrameLoadEndEventHandler(object sender, FrameLoadEndEventArgs e)
		{
			if (Debug) DebugLog.Message("HttpStatusCode: " + e.HttpStatusCode);
			return;
		}

		private void TitleChangedEventHandler(object sender, TitleChangedEventArgs e)
		{
			if (Debug) DebugLog.Message("Title: " + e.Title);
			return;
		}

		private void LoadErrorEventHandler(object sender, LoadErrorEventArgs e)
		{
			if (Debug) DebugLog.Message("ErrorText: " + e.ErrorText);
			return;
		}

		private void JavascriptMessageReceivedEventHandler(object sender, JavascriptMessageReceivedEventArgs e)
		{
			if (Debug) DebugLog.Message("Message: " + e.Message);
			return;
		}

		private void OnPaintEventHandler(object sender, OnPaintEventArgs e)
		{
			if (Debug) DebugLog.Message("OnPaint");
			OnPaintCount++;
			return;
		}

	}

	/// <summary>
	/// Common static Cef methods
	/// </summary>

	public class CefMx
	{
		public static HelmOffScreenBrowserCef InitialOffScreenRenderer = null;
		public static double MonitorScalingFactor = 1; // set to -1 to force new calc

		public static bool Debug = true;

		/// <summary>
		/// Initialize Cef
		/// </summary>

		public static void InitializeCef()
		{
			//if (MonitorScalingFactor < 0)
			//	MonitorScalingFactor = WindowsHelper.GetMonitorScalingFactor();

			if (Cef.IsInitialized) return;

			InitializeOffScreenCef(); // note that we MUST use OffScreen not WinForms CefSettings, can only use one Initialize

			//if (DebugMx.True) return; // debug - disable InitialOffScreenRenderer creation

			InitialOffScreenRenderer = new HelmOffScreenBrowserCef(); // if WinFormsCef request then create an OffScreenCef first to avoid window-offset display issues with on-screen browser
			InitialOffScreenRenderer.LoadInitialPage();

			//InitializeWinFormsCef(); // don't use, causes off screen rendering to hang

			/// <summary>
			/// Initialize DebugLog if not done yet
			/// </summary>

			if (Debug && String.IsNullOrWhiteSpace(DebugLog.LogFileName))
			{
				DebugLog.LogFileName = @"c:\Mobius\DebugHelm.log";
				DebugLog.ResetFile();
			}

			return;
		}

		/// <summary>
		/// Initialize OffScreen Cef
		/// Note that we MUST use OffScreen not WinForms CefSettings, can only use one Initialize
		/// </summary>

		public static void InitializeOffScreenCef()
		{
			if (Cef.IsInitialized) return;

			Stopwatch sw = Stopwatch.StartNew();

			//Monitor parent process exit and close subprocesses if parent process exits first
			//This will at some point in the future becomes the default
			CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

			var settings = new CefSharp.OffScreen.CefSettings(); // not that we MUST use OffScreen not WinForms CefSettings

			settings.CachePath = // By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");

			settings.SetOffScreenRenderingBestPerformanceArgs();

			//Perform dependency check to make sure all relevant resources are in our output directory.
			Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

			var cefVersion = string.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}",
			 Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion);

			if (Debug) DebugLog.StopwatchMessage("OffScreen Cef initialized for " + cefVersion + ", Time: ", sw);

			return;
		}

		/// <summary>
		/// Initialize WinForms Cef
		/// Don't use, causes off screen rendering to hang
		/// </summary>

		public static void InitializeWinFormsCef_DoNotUse()
		{
			if (Cef.IsInitialized) return;

			//Monitor parent process exit and close subprocesses if parent process exits first
			//This will at some point in the future becomes the default
			CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

			var settings = new CefSharp.WinForms.CefSettings();

			settings.CachePath = // By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");

			//settings.SetOffScreenRenderingBestPerformanceArgs();

			//Perform dependency check to make sure all relevant resources are in our output directory.
			Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

			var cefVersion = string.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}",
			 Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion);

			if (Debug) DebugLog.Message("WinForms Cef initialized for " + cefVersion);

			return;
		}

		/// <summary>
		/// Shutdown CEF
		/// </summary>

		public static void ShutdownCef()
		{
			if (!Cef.IsInitialized) return;

			// Clean up Chromium objects.  You need to call this in your application otherwise
			// you will get a crash when closing.
			Cef.Shutdown();
			return;
		}

		/// <summary>
		/// Execute JavaScript that return a string result
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>

		public static string ExecuteStringResultJavaScript(
			IWebBrowser browser,
			string script)
		{
			JavascriptResponse r = ExecuteJavaScript(browser, script);
			if (r.Result != null)
				return r.Result.ToString();

			else return null;
		}

		/// <summary>
		/// Execute a chunk of JavaScript, wait for completion & return response
		/// </summary>
		/// <param name="script"></param>

		public static JavascriptResponse ExecuteJavaScript(
			IWebBrowser browser,
			string script)
		{
			//if (DebugMx.True) return new JavascriptResponse(); // disable - debug

			Stopwatch sw = Stopwatch.StartNew();

			if (Debug) DebugLog.Message("Script: " + script);

			Task<JavascriptResponse> task = WebBrowserExtensions.EvaluateScriptAsync(browser, script);
			task.Wait();

			if (Debug) DebugLog.StopwatchMessage("Script complete, Time: ", sw);

			JavascriptResponse r = task.Result;
			return r;
		}

		/// <summary>
		/// Show the dev tools
		/// </summary>
		/// <param name="browser"></param>

		public static void ShowDevTools(IWebBrowser browser)
		{
			if (browser == null) throw new NullReferenceException("Null browser");

			WebBrowserExtensions.ShowDevTools(browser);
			return;
		}

	}
}

#endif // end of surrounding conditional to include or exclude this class

