using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using CefSharp.OffScreen;
using CefSharp;

using Mobius.ComOps;

namespace Mobius.Helm
{
	/// <summary>
	/// Wrapper for  CEFSharp (Chromium Embedded Framework) Chromium Browser (Headless)
	/// </summary>

	public class ChromiumHeadlessBrowserMx
	{
		public ChromiumWebBrowser CefHeadlessBrowser = null;

		ManualResetEvent BrowserInitializedEvent = new ManualResetEvent(false);
		ManualResetEvent PageLoadedEvent = new ManualResetEvent(false);
		ManualResetEvent BitmapRetrievedEvent = new ManualResetEvent(false);
		ManualResetEvent ScriptEvaluatedEvent = new ManualResetEvent(false);

		/// <summary>
		/// Constructor
		/// </summary>

		public ChromiumHeadlessBrowserMx()
		{
			ChromiumBrowserMx.InitLogFile();

			ChromiumBrowserMx.InitializeCef();

			BrowserInitializedEvent.Reset();
			CefHeadlessBrowser = new ChromiumWebBrowser(); // Create the offscreen Chromium browser.

			CefHeadlessBrowser.BrowserInitialized += BrowserInitializedEventHandler;
			CefHeadlessBrowser.FrameLoadStart += FrameLoadStartEventHandler;
			CefHeadlessBrowser.LoadingStateChanged += BrowserLoadingStateChanged;
			CefHeadlessBrowser.AddressChanged += AddressChangedEventHandler;
			CefHeadlessBrowser.StatusMessage += StatusMessageEventHandler;
			CefHeadlessBrowser.ConsoleMessage += ConsoleMessageEventHandler;
			CefHeadlessBrowser.FrameLoadEnd += FrameLoadEndEventHandler;
			CefHeadlessBrowser.TitleChanged += TitleChangedEventHandler;
			CefHeadlessBrowser.LoadError += LoadErrorEventHandler;
			CefHeadlessBrowser.JavascriptMessageReceived += JavascriptMessageReceivedEventHandler;
			CefHeadlessBrowser.Paint += OnPaintEventHandler;

			BrowserInitializedEvent.WaitOne(); // wait for initialization to complete

			DebugLog.Message("Browser created");
			return;
		}

		private void BrowserInitializedEventHandler(object sender, EventArgs e)
		{
			DebugLog.Message("ChromiumHeadlessBrowserMx");
			//CefHeadlessBrowser.BrowserInitialized -= BrowserInitializedEventHandler;

			BrowserInitializedEvent.Set();
			return;
		}

		/// <summary>
		/// Navigate to a URL and wait for page loading to complete
		/// </summary>
		/// <param name="url"></param>

		public void NavigateTo(string url)
		{
			DebugLog.Message("Loading page: " + url);

			//WebBrowser.Size = new Size((int)(800 + 1.25), (int)(400 * 1.25));
			CefHeadlessBrowser.Load(url);

			// An event that is fired when the first page is finished loading.
			// This returns to us from another thread.
			PageLoadedEvent.Reset();
			PageLoadedEvent.WaitOne();

			DebugLog.Message("Loading complete");

			return;
		}

		/// <summary>
		/// BrowserLoadingStateChanged
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			DebugLog.Message("IsLoading: " + e.IsLoading);

			// Check to see if loading is complete - this event is called twice, one when loading starts
			// second time when it's finished
			// (rather than an iframe within the main frame).
			if (!e.IsLoading)
			{
				// Remove the load event handler, because we only want one snapshot of the initial page.
				//CefHeadlessBrowser.LoadingStateChanged -= BrowserLoadingStateChanged;
				PageLoadedEvent.Set();

				Task<JavascriptResponse> scriptTask = CefHeadlessBrowser.EvaluateScriptAsync("document.getElementById('lst-ib').value = 'CefSharp Was Here!'");
				scriptTask.Wait();
				Thread.Sleep(500); //Give the browser a little time to render
			}
		}

		/// <summary>
		/// Execute a chunk of JavaScript & wait for completion
		/// </summary>
		/// <param name="script"></param>

		public void ExecuteJavaScript(string script)
		{
			DebugLog.Message("Script: " + script);

			Task<JavascriptResponse> task = WebBrowserExtensions.EvaluateScriptAsync(CefHeadlessBrowser, script);
			task.Wait();

			DebugLog.Message("Script complete");
			return;
		}

		/// <summary>
		/// Get bitmap of full browser window
		/// </summary>
		/// <returns></returns>

		public Bitmap GetBitmap()
		{
			Size size = new Size();
			Bitmap bitmap = GetBitmap(size);
			return bitmap;
		}

		/// <summary>
		/// Extract a bitmap from the browser window
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>

		public Bitmap GetBitmap(
			Size size)
		{
			Bitmap bitmap = null;

			DebugLog.Message("Getting bitmap");

			BitmapRetrievedEvent.Reset();

			var task = CefHeadlessBrowser.ScreenshotAsync(); // start getting screen shot & then wait for completion
			task.ContinueWith(x =>
			{
				// Make a file to save it to (e.g. C:\Users\jan\Desktop\CefSharp screenshot.png)
				//var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CefSharp screenshot.png");

				//Console.WriteLine();
				//Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);

				// Save the Bitmap to the path.
				// The image type is auto-detected via the ".png" extension.
				//task.Result.Save(screenshotPath);
				//task.Result.Save(@"c:\Downloads\HelmDepiction.png"); // debug
				//Bitmap = new Bitmap(task.Result);

				if (!size.IsEmpty)
				{
					Rectangle section = new Rectangle(0, 0, size.Width, size.Height);
					//Rectangle section = new Rectangle(0, 0, (int)(size.Width + 1.25), (int)(size.Height + 1.25));
					bitmap = task.Result.Clone(section, task.Result.PixelFormat);
				}

				else bitmap = new Bitmap(task.Result);

				//Bitmap.Save(@"c:\Downloads\HelmDepiction.png"); // debug

				// We no longer need the original Bitmap.
				// Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
				task.Result.Dispose();

				BitmapRetrievedEvent.Set();

				//Console.WriteLine("Screenshot saved.  Launching your default image viewer...");

				// Tell Windows to launch the saved image.
				//Process.Start(new ProcessStartInfo(screenshotPath)
				//{
				//	// UseShellExecute is false by default on .NET Core.
				//	UseShellExecute = true
				//});

				//Console.WriteLine("Image viewer launched.  Press any key to exit.");
			}, TaskScheduler.Default);

			BitmapRetrievedEvent.WaitOne();

			DebugLog.Message("Bitmap Retrieved");
			return bitmap;
		}

		private void FrameLoadStartEventHandler(object sender, FrameLoadStartEventArgs e)
		{
			DebugLog.Message("Frame.Name: " + e.Frame.Name);

			return;
		}

		private void AddressChangedEventHandler(object sender, AddressChangedEventArgs e)
		{
			DebugLog.Message("Address: " + e.Address);
			return;
		}

		private void StatusMessageEventHandler(object sender, StatusMessageEventArgs e)
		{
			DebugLog.Message("Value: " + e.Value.ToString());
			return;
		}

		private void ConsoleMessageEventHandler(object sender, ConsoleMessageEventArgs e)
		{
			DebugLog.Message("Message: " + e.Message);
			return;
		}

		private void FrameLoadEndEventHandler(object sender, FrameLoadEndEventArgs e)
		{
			DebugLog.Message("HttpStatusCode: " + e.HttpStatusCode);
			return;
		}

		private void TitleChangedEventHandler(object sender, TitleChangedEventArgs e)
		{
			DebugLog.Message("Title: " + e.Title);
			return;
		}

		private void LoadErrorEventHandler(object sender, LoadErrorEventArgs e)
		{
			DebugLog.Message("ErrorText: " + e.ErrorText);
			return;
		}

		private void JavascriptMessageReceivedEventHandler(object sender, JavascriptMessageReceivedEventArgs e)
		{
			DebugLog.Message("Message: " + e.Message);
			return;
		}

		private void OnPaintEventHandler(object sender, OnPaintEventArgs e)
		{
			DebugLog.Message("OnPaint");
			return;
		}

	}
}
