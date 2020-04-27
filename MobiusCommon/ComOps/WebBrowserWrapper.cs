using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.ComOps
{

	/// <summary>
	/// WebBrowserWrapper 
	/// Based on: C# Webbrowser Wrapper Control, used to be able to wait until a page completely loads prior to taking action on it. For web automation projects (hmcclungiii)
	/// </summary>
	public class WebBrowserWrapper
	{

		public WebBrowser WebBrowser = null; // brower being wrapped

		/// <summary>
		/// This event is raised when the current document is entirely complete, 
		/// and no more navigation is expected to take place for it to load.
		/// </summary>
		/// <author>hmcclungiii</author>
		/// <date>2/21/2014</date>
		public event AbsolutelyCompleteEventHandler AbsolutelyComplete;
		public delegate void AbsolutelyCompleteEventHandler(object sender, EventArgs e);

		private int _onNavigatingCount = 0;
		private int _onNavigatedCount = 0;
		private int _onDocumentCompleteCount = 0;

		public WebBrowserWrapper(WebBrowser webBrowser)
		{
			WebBrowser = webBrowser;
			WebBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
			WebBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebBrowser_Navigating);
			WebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.WebBrowser_Navigated);
		}
		
		/// <summary>
		/// This method is used to clear the counters.  Should not be used
		/// externally, but I left it open for testing, and just in case
		/// scenarios
		/// </summary>
		/// <author>hmcclungiii</author>
		/// <date>2/21/2014</date>
		public void ClearCounters()
		{
			_onNavigatingCount = 0;
			_onNavigatedCount = 0;
			_onDocumentCompleteCount = 0;
		}

		/// <summary>
		/// This property returns true when all the counters have become equal
		/// signifying that the navigation has completely completed
		/// </summary>
		/// <author>hmcclungiii</author>
		/// <date>2/21/2014</date>
		public bool Busy
		{
			get
			{
				//sometimes the first navigating event isn't fired so we just have to make sure the navigating count is 
				//more than the navigated, navigated should never be more than navigating
				bool bBusy = !(_onNavigatingCount <= _onNavigatedCount);
				//if our navigating counts check out, we should always have a documentcompleted count
				//for each navigated event that is fired
				if (!bBusy)
				{
					bBusy = (_onNavigatedCount > _onDocumentCompleteCount);
				}
				else
				{
					bBusy = !(_onNavigatedCount == _onDocumentCompleteCount);
					if (!bBusy) bBusy = !(_onNavigatedCount > 0);
				}
				return bBusy;
			}
		}

		/// <summary>
		/// This method is used to wait until the page has completely loaded.  Use
		/// after calling a submit, or click, or similar method to not execute further
		/// code in the calling class until it has completed.  Also helps to reduce
		/// processor load
		/// </summary>
		/// <author>hmcclungiii</author>
		/// <date>2/21/2014</date>
		public void WaitUntilComplete()
		{
			//first we wait to make sure it starts
			while (!Busy)
			{
				Application.DoEvents();
				//we should sleep for a moment to let the processor have a timeslice
				//for something else - in other words, don't hog the resources
				System.Threading.Thread.Sleep(1);
			}
			//now we wait until it is done
			while (Busy)
			{
				Application.DoEvents();
				// Note that if a script error occurs and display is enabled the error will popup and block during the DoEnvents

				// Sleep for a moment to let the processor have a timeslice doing other work

				System.Threading.Thread.Sleep(1);
			}
		}

		//we have to catch the following three event callers to keep a count
		//of them so that we will be able to determine when the navigation
		//process actually completes
		private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			_onNavigatingCount += 1;
			if (!Busy)
				OnAbsolutelyComplete();
		}
		private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			_onNavigatedCount += 1;
			if (!Busy)
				OnAbsolutelyComplete();
		}
		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			_onDocumentCompleteCount += 1;
			if (!Busy)
				OnAbsolutelyComplete();
		}

		/// <summary>
		/// Start navigate and return immediately
		/// </summary>
		/// <param name="URL"></param>

		public void Navigate(string URL)
		{
			ClearCounters();
			WebBrowser.Navigate(URL);
		}

		/// <summary>
		/// This method should be used in place of the Navigate method to navigate to
		/// a specific URL.  The navigate method was not overridden because it might
		/// be required in future modifications to have access to both methods.  When
		/// calling this NavigateAndWait method, control will not be returned to the
		/// calling class until the document has completely loaded
		/// </summary>
		/// <author>hmcclungiii</author>
		/// <date>2/21/2014</date>
		public void NavigateAndWait(string URL)
		{
			ClearCounters();
			WebBrowser.Navigate(URL);
			WaitUntilComplete();
		}

		protected void OnAbsolutelyComplete()
		{
			ClearCounters();
			if (AbsolutelyComplete != null)
			{
				AbsolutelyComplete(this, new EventArgs());
			}
		}

/******************************************
/* possible way to implement TimeOuts 
 ****************************************** 
		//Navigation Timer
		timer2.Enabled = true;
        timer2.Interval = 30000;

        br.DocumentCompleted += browser_DocumentCompleted;
        br.DocumentCompleted += writeToTextBoxEvent;
        br.Navigating += OnNavigating;
        br.Navigated  += OnNavigated;

        br.ScriptErrorsSuppressed = true;
        br.Navigate(ConfigValues.websiteUrl);

    private void OnNavigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			//Reset Timer
			timer2.Stop();
			timer2.Start();

			WriteLogFunction("OnNavigating||||||" + e.Url.ToString());
		}

		private void OnNavigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			//Stop Timer
			timer2.Stop();

			WriteLogFunction("NAVIGATED <><><><><><><> " + e.Url.ToString());
		}


		private void timer2_Tick(object sender, EventArgs e)
		{
			WriteLogFunction(" Navigation Timeout TICK");
			br.Stop();
			br.Navigate(ConfigValues.websiteUrl);
		}
*/

	}
}
