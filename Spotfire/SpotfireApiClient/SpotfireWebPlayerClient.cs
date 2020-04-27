using Mobius.ComOps;
using Mobius.SpotfireClient.ComAutomation;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Mobius.SpotfireClient
{
	public class SpotfireWebPlayerClient
	{
		static string WebPlayerServerUrl = "https://[server]";
		string WebPlayerControlPage = "http://[server]/MobiusQueryExecutorApp/MobiusSpotfireWebPlayerControlPage.htm";
		//string WebPlayerControlPage = "http://[server]/MobiusQueryExecutorApp/JavaScriptApiTestSimple.htm";
		//string WebPlayerControlPage = "http://[server]/MobiusQueryExecutorApp/JavaScriptApiTest.htm";

		// Note that the htm files should contain the following line to get IE into the proper Document Mode to
		// be able to handle the Spotfire JavaScript
		//
		// "<meta http-equiv="X-UA-Compatible" content="IE=edge" />";


		internal bool WebPlayerApiIsAvailable = false; // true if web control page and Spotfire WebPlayer API successfully loaded
		internal string MobiusSpotfireApiCallDocumentPropertyname = "MobiusSpotfireApiCall"; // Spotfire Document property set to start an API call
		internal string MobiusSpotfireApiResponseDocumentPropertyname = "MobiusSpotfireApiResponse"; // Base Spotfire Document property where API call response is returned

		internal string AnalysisPath = ""; // path to analysis to load

		internal Control WbContainer;
		internal WebBrowser WebBrowser;
		internal WebBrowserWrapper WebBrowserWrapper = null;
		internal TextEdit StatusTextBox;

		public bool WaitingForWindowOnloadEvent = false;

		public bool WaitingForAnalysisOpenedEvent = false;

		public string MobiusSpotfireApiResponse = null; // result from call to MobiusSpotfireApiServer
		public bool ResponseHasBeenReceived = false; // true if response recieved
		public bool ResponseHasBeenRetrieved = true; // true if recived response has been retrieved

		public bool WaitingForGetDocumentProperty = false;
		public string GetDocumentPropertyName = "";
		public string GetDocumentPropertyValue = "";

		static int CallId = 0;

		static bool Debug => SpotfireApiClient.Debug;

		/// <summary>
		/// Constructor
		/// </summary>

		public SpotfireWebPlayerClient()
		{
			return;
		}

		/// <summary>
		/// Initialize WebBrowser control 
		/// </summary>
		/// <param name="webBrowserContainer"></param>
		/// <param name="webBrowser"></param>
		/// <param name="statusTextBox"></param>

		public void InitializeWebBrowserControl(
			WebBrowser webBrowser,
			Control webBrowserContainer,
			TextEdit statusTextBox)
		{
			WebPlayerApiIsAvailable = false;

			WebBrowser = webBrowser;
			WebBrowser.ScriptErrorsSuppressed = true; //JavaScriptManager.SuppressJavaScriptErrors;
			WbContainer = webBrowserContainer;
			StatusTextBox = statusTextBox;

			WebBrowser.ObjectForScripting = new ScriptManager(this); // create ScriptManager that can call back to this JavaScriptInterface instance from within web page JavaScript
			WebBrowserWrapper = new WebBrowserWrapper(WebBrowser);

			return;
		}

		/// <summary>
		/// Open Spotfire WebPlayer app and analysis
		/// </summary>
		/// <param name="analysisPath"></param>

		public bool OpenAnalysis(
			string analysisPath)
		{
			AnalysisPath = analysisPath;
			Stopwatch sw = new Stopwatch();

			//UpdateStatusTextBox("Loading Mobius Webplayer control page...");

			while (true)
			{
				UpdateStatusTextBox("Opening analysis: " + analysisPath + "...");

				WaitingForAnalysisOpenedEvent = true; // when this becomes set to false the open is comoplete
				WebPlayerApiIsAvailable = true; // assume api is available unless set to false later

				WebBrowserWrapper.NavigateAndWait(WebPlayerControlPage); // load the control page

				Application.DoEvents();
				Thread.Sleep(500); // wait a bit for stabilization

				// If Mobius control page failed to load, need to login or some other problem

				if (!WebPlayerApiIsAvailable)
				{
					UpdateStatusTextBox("WebPlayerApi is not available, trying to login...");

					bool loggedIn = LoginToWebPlayer(); // wait for login
					if (loggedIn) continue; // try to load control page and open analysis again
					else return false;
				}

				// Should be logged in and opening now, wait until open complete or some Spotfire error

				sw.Restart();
				while (true)
				{
					if (!WaitingForAnalysisOpenedEvent) // open complete?
					{
						UpdateStatusTextBox("Open completed");
						InvokeScriptMethod("resize"); // Resize the WebPlayer div to fill available area
						return true;
					}

					if (sw.ElapsedMilliseconds > 8000) return false; // timeout?

					Application.DoEvents();
					Thread.Sleep(100);
				}

			}

		} //OpenAnalysis

		bool LoginToWebPlayer()
		{
			WebBrowser.Navigate(WebPlayerServerUrl); // load the page to login to Webplayer
			Stopwatch sw = new Stopwatch();

			while (!IsLoginPageDisplayed())
			{
				Application.DoEvents();
				Thread.Sleep(100);
				if (sw.ElapsedMilliseconds > 5000) return false; // login page not appearing for some reason
			}

			while (!IsLoggedIn()) // wait until logged in
			{
				Application.DoEvents();
				Thread.Sleep(100);
			}

			return true;
		}

		bool IsLoginPageDisplayed()
		{
			int tokenMatchCount = CountWebBrowserDocumentTokenMatches(
				new string[]
				{
					"app.login",
					"<title>Login</title>"
				});

			if (tokenMatchCount >= 1) return true;
			else return false;
		}


		bool IsAnalysisClosedPageDisplayed()
		{
			int tokenMatchCount = CountWebBrowserDocumentTokenMatches(
				new string[]
				{
					"Your analysis has been closed"
				});

			if (tokenMatchCount >= 1) return true;
			else return false;

		}

		int CountWebBrowserDocumentTokenMatches (string[] tokens)
		{
			string html = WebBrowser.DocumentText;

			int tokCnt = tokens.Length;
			int tokMatch = 0;

			foreach (string tok in tokens)
			{
				if (Lex.Contains(html, tok)) tokMatch++;
			}

			return tokMatch;
		}


		/// <summary>
		/// Check if WebPlayer appears to be logged in
		/// </summary>

		bool IsLoggedIn()
		{
			if (WebBrowser.IsDisposed) return false;

			string html = WebBrowser.DocumentText;

			string[] tokens =
			{
				"Interactive Data Visualization",
			 "LibraryBrowser",
			 "library",
			 "appLoader",
			 "library-navbar"
			};

			int tokCnt = tokens.Length;
			int tokMatch = 0;

			foreach (string tok in tokens)
			{
				if (Lex.Contains(html, tok)) tokMatch++;
			}

			if (tokMatch >= 2)
				return true;

			else return false;

			// Sample WebPlayer html after logging in used to get tokens above

			/* 
			 <div id="appLoader">
			 <tss-header app="library"></tss-header>
			 <div class="startPage twbs" ng-cloak>
					 <div ng-controller="NavigationController" id="library-navbar">
							 <drag-and-drop-files ng-if="viewHandlers.createAnalysisFromLocalFile"></drag-and-drop-files>
							 <div class="modal-backdrop modal-backdrop-customized fade in"
										ng-show="navigation.modalActive && navigation.noTouch" responsive-window=""></div>
							 <div class="collapse searchbarCollapsed navbar-inverse" ng-controller="SearchController"
										ng-include="'search.html'"></div>
							 <library-sub-header></library-sub-header>

							 <!-- Temporary placeholder for end point -->
							 <div class="container container-customized" ng-include src="'debugEndPoints.html'"></div>

							 <div ng-view class="container container-customized container-padding library-view cf"></div>

							 <div ng-include src="'modal-dialog.html'"></div>
					 </div>
			 </div>

	 </div>
			*/
		}
		/// <summary>
		/// Called when WebBrowser has navigated
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		public void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			return;
		}

		/// <summary>
		/// CloseAnalysis
		/// </summary>

		public void CloseAnalysis()
		{
			return; // anything to do?
		}

		public void UpdateStatusTextBox(string msg)
		{
			if (StatusTextBox == null) return;
			StatusTextBox.Text += DateTime.Now.ToString("h: mm:ss") + " - " + msg + "\r\n";
		}

		/// <summary>
		/// Call the Spotfire API
		/// </summary>
		/// <param name="mobiusApiMethodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>

		public string CallSpotfireApi(
			string mobiusApiMethodName,
			params string[] args)
		{
			CallId++;

			int responseId = -1, responseLength = -1, chunkId = -1; ;

			string methodAndParms = // join the mobius
				SpotfireComClient.JoinMethodAndParms(mobiusApiMethodName, args);

			methodAndParms = CallId.ToString() + "\t" + methodAndParms;

			if (Debug) DebugLog.Message("CallMobiusSpotfireApi: " + methodAndParms);
			
			ResponseHasBeenReceived = false;

			string result = InvokeScriptMethod("CallMobiusSpotfireApi", methodAndParms); // start the call

			while (!ResponseHasBeenReceived)
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}

			string response = MobiusSpotfireApiResponse; // copy the response
			if (response == null) response = "<null>";

			MobiusSpotfireApiResponse = null; // null out the buffer
			ResponseHasBeenRetrieved = true; // indicate it's been retrieved 

			if (Debug) DebugLog.Message("Response: " + response);

			if (Lex.Contains(response, "<MsxException>"))
				return response;

			// Parse out the chunk id, response id and length

			Lex.TryParseOffFirstIntegerToken(ref response, '\t', out chunkId);
			Lex.TryParseOffFirstIntegerToken(ref response, '\t', out responseId);
			Lex.TryParseOffFirstIntegerToken(ref response, '\t', out responseLength);

			if (responseLength > response.Length) // get remaining chunks for long response strings stored in additional doc properties
			{
				for (int i1 = 2; ; i1++) // loop until we get all chunks 
				{
					string propName = MobiusSpotfireApiResponseDocumentPropertyname + i1.ToString(); // next prop to get
					string chunk = GetDocumentProperty(propName);

					if (chunk == null || chunk.Length == 0) throw new Exception("Null/empty value for Document Property : " + propName);

					Lex.TryParseOffFirstIntegerToken(ref chunk, '\t', out chunkId);

					response += chunk;
					if (response.Length >= responseLength) break;
					else if (response.Length > responseLength)
						throw new Exception("Response longer than expected " + response.Length + " > " + responseLength + " for Document Property : " + propName);
				}
			}

			if (response == "<null>")
				response = null;

			return response;
		}

		/// <summary>
		/// Call a method in a script
		/// </summary>
		/// <param name="jsMethodName"></param>
		/// <param name="args">Itemized list of args, not an array</param>
		/// <returns></returns>

		public string InvokeScriptMethod(
			string jsMethodName,
			params string[] args)
		{
			object result = null;

			if (WebBrowser == null || WebBrowser.IsDisposed || WebBrowser.Document == null) return null;

			if (args == null || args.Length == 0) // no args
				result = WebBrowser.Document.InvokeScript(jsMethodName);

			else // convert args string array to object array
			{
				object[] oArgs = new object[args.Length];
				args.CopyTo(oArgs, 0);
				result = WebBrowser.Document.InvokeScript(jsMethodName, args);
			}

			if (result != null) return result.ToString();
			else return null;
		}

		/// <summary>
		/// Set a document property
		/// </summary>
		/// <param name="propName"></param>
		/// <param name="propValue"></param>

		public void SetDocumentProperty(
			string propName,
			string propValue)
		{
			InvokeScriptMethod("setDocumentProperty", propName, propValue);
			return;
		}

		/// <summary>
		/// Get a document property
		/// </summary>
		/// <param name="propName"></param>
		/// <returns></returns>

		public string GetDocumentProperty(
			string propName)
		{
			InvokeScriptMethod("getDocumentProperty", propName);

			WaitingForGetDocumentProperty = true;
			while (WaitingForGetDocumentProperty)
			{
				Application.DoEvents();
				Thread.Sleep(10);
				//await Task.Delay(1);
			}

			return GetDocumentPropertyValue;
		}

	}

	/// <summary>
	/// Script manager class allows script to call back into C# code
	/// </summary>

	[ComVisible(true)]
	public class ScriptManager
	{
		SpotfireWebPlayerClient SWPC;

		public ScriptManager(SpotfireWebPlayerClient swpc) // create a script manager and reference it back to us
		{
			SWPC = swpc;
		}

		/// <summary>
		/// Called by JavaScript when window.onload function is called
		/// </summary>
		/// <param name="obj"></param>

		public void WindowOnloadCallback(object obj)
		{
			if (obj == null) obj = "";
			bool.TryParse(obj.ToString(), out SWPC.WebPlayerApiIsAvailable);

			SWPC.UpdateStatusTextBox("WindowOnloadCallback() called, WebPlayerApiIsAvailable = " + SWPC.WebPlayerApiIsAvailable);

			if (SWPC.WebPlayerApiIsAvailable) return; // available
			else return; // not available, probably need to login first
		}

		/// <summary>
		/// Called by JavaScript to get the path to the analysis to open
		/// </summary>
		/// <returns></returns>

		public string GetAnalysisPath()
		{
			SWPC.UpdateStatusTextBox("GetAnalysisPath called, returning: " + SWPC.AnalysisPath);
			return SWPC.AnalysisPath;
		}

		/// <summary>
		/// Called by JavaScript when an Open Analysis operation is complete
		/// </summary>
		/// <param name="obj"></param>

		public void AnalysisOpenedCallback()
		{
			SWPC.WaitingForAnalysisOpenedEvent = false;
			SWPC.UpdateStatusTextBox("AnalysisOpenedCallback() called");
		}

		/// <summary>
		/// When result of call to MobiusSpotfireApiServer is available it is returned via call to this func from JavaScript
		/// </summary>
		/// <param name="obj"></param>

		public void MobiusSpotfireApiCallback(object obj)
		{
			if (obj == null) obj = "";
			SWPC.MobiusSpotfireApiResponse = obj.ToString(); // return result
			SWPC.ResponseHasBeenReceived = true; // indicate response has been recieved
			SWPC.ResponseHasBeenRetrieved = false; // but not yet retrieved
			return;
		}

		public void GetDocumentPropertyCallback(object propertyName, object propertyValue)
		{
			if (propertyName == null) propertyName = "";
			if (propertyValue == null) propertyValue = "";

			SWPC.GetDocumentPropertyName = propertyName.ToString();
			SWPC.GetDocumentPropertyValue = propertyValue.ToString();
			SWPC.WaitingForGetDocumentProperty = false;
		}

		/// <summary>
		/// Called by JavaScript when a Close Analysis operation is complete
		/// </summary>
		/// <param name="obj"></param>

		public void AnalysisClosedCallback(object obj)
		{
			return;
		}

		/// <summary>
		/// Called by JavaScript when a user logged out event occurs (explicit request or session timeout)
		/// </summary>
		/// <param name="obj"></param>

		public void AnalysisLoggedOutCallback(object obj)
		{
			return;
		}

		/// <summary>
		/// Method that gets called from JavaScript
		/// </summary>
		/// <param name="obj"></param>
		public void LogJavaScriptMessage(object obj)
		{
			string msg = "";

			if (obj != null) msg = obj.ToString();
			msg = msg.Replace("\n", " ");
			msg = msg.Replace("\r", "");
			msg = msg.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
			msg = msg.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");

			if (SWPC.StatusTextBox != null)
			{
				if (msg.Length > 256) msg = msg.Substring(0, 256) + "...";

				SWPC.UpdateStatusTextBox("JavaScript Msg: " + msg);
			}

			if (Lex.Contains(msg, "Error:")) // if error from JavaScript convert to <MsxException>
			{
				SWPC.ResponseHasBeenRetrieved = false; // but not yet retrieved
				msg = "<MsxException>: " + msg;
				SWPC.MobiusSpotfireApiResponse = msg; // return result
				SWPC.ResponseHasBeenReceived = true; // indicate response has been recieved
				SWPC.ResponseHasBeenRetrieved = true;
			}
		}
	} // ScriptManager
}

