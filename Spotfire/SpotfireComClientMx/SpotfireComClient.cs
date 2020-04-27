using Mobius.SpotfireComOps; // needed since this must be strongly signed assembly

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Mobius.SpotfireClient.ComAutomation
{
	/// <summary>
	/// Class for calling Spotfire API via COM interface
	/// </summary>
	public class SpotfireComClient
	{
		internal SpotfireComCallbackBase ApiCallback; // callback to class instance that created us

		internal AnalysisApplicationControllerWrapper ControllerWrapper;

		internal ComWrapper ComWrapper;

		public static int CallId = 0;

		public static string SpotfireComServerDllFolder = @"C:\Mobius\Spotfire\SpotfireApiServerMx\bin\Debug"; // folder containing Com server
		public static string SpotfireComServerDllName = @"SpotfireComServerMx.dll"; // file name of ComServer dll that gets loaded into the Spotfire App instance

		public SpotfireComClient()
		{
			return;
			//InitializeComponent();
		}

		/// <summary>
		/// Start a new Spotfire instance
		/// </summary>
		/// <param name="commandLine"></param>

		public void LaunchDxp(
			SpotfireComCallbackBase callbackArg,
			string commandLine)
		{
			this.ApiCallback = callbackArg;

			// Create the COM wrapper for TIBCO Spotfire. This will launch TIBCO Spotfire.
			this.ControllerWrapper = new AnalysisApplicationControllerWrapper();

			this.ControllerWrapper.StartWithCommandLine(new ComCallback(this), commandLine);
		}

		public void RegisterCallback(ComServerCallbackBase callback)
		{
			ComWrapper.InvokeMethod("RegisterCallback", new ComServerCallbackWrapper(callback));
		}

		/// <summary>
		/// Exit Spotfire instance
		/// </summary>

		public void ExitDxp()
		{
			if (this.ControllerWrapper != null)
			{
				this.ControllerWrapper.Exit();
			}
		}

		/// <summary>
		/// OpenDocument
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>

		public bool OpenDocument(string filePath)
		{
			return (bool)ComWrapper.InvokeMethod("OpenDocument", filePath);
		}

		/// <summary>
		/// CallMobiusSpotfireApi
		/// </summary>
		/// <param name="mobiusApiMethodName"></param>
		/// <param name="args"></param>
		/// <returns>Result from method</returns>

		public string CallMobiusSpotfireApi(
			string mobiusApiMethodName,
			params string[] args)
		{
			int chunkId, responseId, responseLength = -1;

			Stopwatch sw = Stopwatch.StartNew();

			CallId++;

			string methodAndParms = JoinMethodAndParms(mobiusApiMethodName, args);

			methodAndParms = CallId.ToString() + "\t" + methodAndParms;

			//DebugLog.Message("CallMobiusSpotfireApi: " + methodAndParms);

			string response = (string)ComWrapper.InvokeMethod("CallMobiusSpotfireApi", methodAndParms);

			//string r2 = "<null>"; // get shorted response
			//if (response != null)
			//{
			//	if (response.Length <= 128)
			//		r2 = response;
			//	else r2 = response.Substring(0, 128);
			//}

			//DebugLog.Message("Response: " + response);

			// Parse out the chunk id, response id and length

			TryParseOffFirstIntegerToken(ref response, '\t', out chunkId);
			TryParseOffFirstIntegerToken(ref response, '\t', out responseId);
			TryParseOffFirstIntegerToken(ref response, '\t', out responseLength);

			if (response == "<null>")
				response = null;

			long ms = sw.ElapsedMilliseconds;
			return response;
		}

		/// <summary>
		/// JoinMethodAndParms
		/// </summary>
		/// <param name="method"></param>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string JoinMethodAndParms(
			string method,
			params string[] args)
		{
			string Delim = "\t"; // tab-separated

			string methodAndParms = method;
			foreach (string s in args)
				methodAndParms += Delim + s;

			return methodAndParms;
		}

		/// <summary>
		/// Try to parse off the first integer token in a string
		/// </summary>
		/// <param name="txt"></param>
		/// <param name="value"></param>
		/// <param name="delim"></param>
		/// <returns></returns>

		static bool TryParseOffFirstIntegerToken(
			ref string txt,
			char delim,
			out int value)
		{
			const int NullNumber = -4194303;

			value = NullNumber;

			int i1 = txt.IndexOf(delim);
			if (i1 < 0) return false;

			if (!int.TryParse(txt.Substring(0, i1), out value)) return false;

			txt = txt.Substring(i1 + 1);
			return true;
		}

		/// <summary>
		/// RunScript
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>

		public string RunScript(string script)
		{
			Stopwatch sw = Stopwatch.StartNew();

			//for (int i1 = 0; i1 < 1000; i1++)
			//{
			// Set directly with C# code in automation server
			// Note that is is 20X faster than doing with script 
			// i.e. .7ms/direct C# call versus 13 ms/call for IronPython script: 
			//  --- Document.Data.Tables.DefaultTableReference.Name = "New Name";
			//
			// ComWrapper.SetDefaultTableName("NoScript Title"); 
			//}

			string result = (string)ComWrapper.InvokeMethod("RunScript", script);

			long ms = sw.ElapsedMilliseconds;
			return result;
		}

		/// <summary>
		/// Configure an analysis for use with Mobius including inserting the Mobius script
		/// and Document properties
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>

		public string ConfigureAnalysisForMobiusUse(string args)
		{
			Stopwatch sw = Stopwatch.StartNew();

			string result = (string)ComWrapper.InvokeMethod("ConfigureAnalysisForMobiusUse", args);

			long ms = sw.ElapsedMilliseconds;
			return result;
		}

		/// <summary>
		/// CloseCurrentDocument
		/// </summary>

		public void CloseCurrentDocument()
		{
			ComWrapper.InvokeMethod("CloseCurrentDocument");
		}

	}

	public class ComCallback : ComCallbackBase
	{
		private SpotfireComClient sfAPI;

		public ComCallback(SpotfireComClient sfApiArg)
		{
			this.sfAPI = sfApiArg;
		}

		public override void Started(StartContextWrapper context)
		{
			base.Started(context);

			if (sfAPI != null && sfAPI.ApiCallback != null)
				sfAPI.ApiCallback.Started(context);
		}
		public override void LoadViews(LoadViewsContextWrapper context)
		{
			base.LoadViews(context);

			string comClientCodeBase = Assembly.GetExecutingAssembly().CodeBase;
			Uri thisCodeBase = new Uri(comClientCodeBase);

			// Load the view object. This will instruct TIBCO Spotfire to load the Mobius.Spotfire.ComAutomationView.dll
			// and create an object of the class Mobius.Spotfire.ComAutomationView.MobiusView. This object is then
			// returned by TIBCO Spotfire and the COM interop framework gives us an object that can be used to interact with the
			// MobiusView instance in TIBCO Spotfire. Since late binding is used, the object returned can only be typed as System.object.

			string comServerCodeBase = SpotfireComClient.SpotfireComServerDllFolder + @"\" + SpotfireComClient.SpotfireComServerDllName;

//			string comServerCodeBase = Application.StartupPath + @"\" + SpotfireAPI.SpotfireComServerDllName;
			object comObject = context.CreateObjectFrom("Mobius.Spotfire.ComAutomationView.MobiusView", comServerCodeBase);

			// Wrap the com object in an MobiusViewWrapper that gives us typed methods to interact with the 
			// com object.
			sfAPI.ComWrapper = new ComWrapper(comObject);

			// Register a callback object so that events from the MobiusView instance loaded in TIBCO Spotfire can 
			// be received.
			this.sfAPI.RegisterCallback(new SpotfireAPI_Callback(this.sfAPI));
		}

		public override void Exited(ExitContextWrapper context)
		{
			base.Exited(context);

			if (sfAPI != null && sfAPI.ApiCallback != null)
				sfAPI.ApiCallback.Exited(context);
		}

	}

	public class SpotfireAPI_Callback : ComServerCallbackBase
	{

		private SpotfireComClient sfAPI;

		public SpotfireAPI_Callback(SpotfireComClient sfApiArg)
		{
			this.sfAPI = sfApiArg;
		}

		public override void OnStatusChanged(string status)
		{
			if (sfAPI != null && sfAPI.ApiCallback != null)
				sfAPI.ApiCallback.OnStatusChanged(status);
		}

	}

	/// <summary>
	/// SpotfireAPICallbackBase
	/// </summary>

	public class SpotfireComCallbackBase
	{

		public virtual void Started(StartContextWrapper context)
		{
		}

		public virtual void OnStatusChanged(string status)
		{
		}

		public virtual void Exited(ExitContextWrapper context)
		{
		}

	}


}
