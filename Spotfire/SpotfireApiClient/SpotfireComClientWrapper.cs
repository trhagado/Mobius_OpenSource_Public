using Mobius.ComOps;
//using Mobius.Data;

using Mobius.SpotfireClient.ComAutomation;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Mobius.SpotfireClient
{
	public class SpotfireComClientWrapper
	{
		string AnalysisPath = ""; // path to analysis: either a file path or a Spotfire server libaray path

		public SpotfireComClient SCC = null;

		/// <summary>
		/// Launch Dxp
		/// </summary>

		public void StartDxpProcess()
		{
			SCC = new SpotfireComClient();
			SpotfireComCallback callBack = new SpotfireComCallback();  //new SpotfireComCallback(this);
			SCC.LaunchDxp(callBack, "");
			callBack.WaitForStartupCompleted();
		}

		/// <summary>
		/// Open analysis
		/// </summary>
		/// <param name="analysisPath"></param>

		public bool OpenAnalysis(
			string analysisPath)
		{
			// Sample library analysis path:
			// https://[server]/spotfire/wp/OpenAnalysis?file=/Mobius/Visualizations/Mobius_TRELLISCARDVISUALIZATIONTEMPLATE

			// Sample DXP file analysis path
			//analysisPath = @"C:\Downloads\TrellisCardVisualizationWithReplacementData.dxp";

			if (SCC == null) StartDxpProcess();

			AnalysisPath = analysisPath;
			bool opened = SCC.OpenDocument(AnalysisPath);
			return opened;
		}

		/// <summary>
		/// Close analysis
		/// </summary>

		public void CloseAnalysis()
		{
			if (SCC == null) return;
			SCC.CloseCurrentDocument();
			AnalysisPath = "";
			return;
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
			if (SCC == null) throw new NullReferenceException("SpotfireComClient is null");

			string methodAndParms = SpotfireComClient.JoinMethodAndParms(mobiusApiMethodName, args);

			methodAndParms = (SpotfireComClient.CallId + 1).ToString() + "\t" + methodAndParms;

			DebugLog.Message("CallMobiusSpotfireApi: " + methodAndParms);

			string response = SCC.CallMobiusSpotfireApi(mobiusApiMethodName, args);
			DebugLog.Message("Response: " + response);

			return response;
		}

		/// <summary>
		/// Configure an analysis for use with Mobius 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public string ConfigureAnalysisForMobiusUse(string args)
		{
			string result = SCC.ConfigureAnalysisForMobiusUse(args);
			return result;
		}

	}

	/// <summary>
	/// Spotfire call back class with methods that get called when particular events occur within Spotfire
	/// </summary>

	public class SpotfireComCallback : SpotfireComCallbackBase
	{
		//internal SpotfireViewManager SVM;
		internal WebBrowser HostWebBrowserControl = null; // WebBrowser control that contans the Webplayer // { get { return SVM.HostWebBrowserControl; } }

		public string Status = "";

		public ManualResetEvent WaitingForStartupCompletedEvent = new ManualResetEvent(false);

		// Calls to these methods are made on a thread that is allocated by  the COM 
		// interop framework. To perform a UI operation such as setting the a text in a Form we must 
		// switch to the Form thread.
		// By doing this with an asynchronous BeginInvoke, this method returns immediately
		// an thus does the COM call from TIBCO Spotfire which allows TIBCO Spotfire to continue execution.                
		// Note that this is used below to set Status but since this is not an IO operation BeginInvoke
		// is not really required.


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="svm"></param>

		public SpotfireComCallback()
		{
			return;
		}

		/// <summary>
		/// Called when Spotfire has started
		/// </summary>
		/// <param name="context"></param>

		public override void Started(StartContextWrapper context)
		{
			base.Started(context);

			//UpdateStatusMessage("Spotfire has started");

			WaitingForStartupCompletedEvent.Set(); // indicate Spotfire has started
			return;
		}

		void UpdateStatusMessage(string msg)
		{
			if (HostWebBrowserControl == null) return;

			HostWebBrowserControl.BeginInvoke(
					new MethodInvoker(delegate { Status = msg; }));
		}

		/// <summary>
		/// Called when Spotfire exits
		/// </summary>
		/// <param name="context"></param>

		public override void Exited(ExitContextWrapper context)
		{
			base.Exited(context);

			UpdateStatusMessage("TIBCO Spotfire exited.");
		}

		/// <summary>
		/// Called when Spotfires status changes
		/// </summary>
		/// <param name="status"></param>

		public override void OnStatusChanged(string status)
		{
			UpdateStatusMessage(status);
		}

		/// <summary>
		/// Wait for startup to complete
		/// </summary>

		public void WaitForStartupCompleted()
		{
			WaitingForStartupCompletedEvent.WaitOne(); // This thread will block here until the reset event is sent.
			WaitingForStartupCompletedEvent.Reset();
		}

	}

}
