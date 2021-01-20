using Mobius.ComOps; 
using Mobius.Data;
using Mobius.CdkMx;
using Mobius.Helm;
using Mobius.SpotfireClient;
using Mobius.ClientComponents;
using Mobius.ToolServices;

//using java.io;

using System;
using System.Collections; 
using System.Collections.Generic; 
using System.IO;
using System.Threading;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
 
namespace Mobius.Client 
{
	 
/// <summary>
/// Main Program entry point
/// </summary>
	static class Program
	{
		public static SessionManager SessionManager; // session manager instance
		public static bool LogDetailedMessages = false; 

		/// <summary>
		/// The main entry point for the Mobius application.
		/// </summary>  

		[STAThread]
		static void Main(string[] args)
		{
			Splash splash = null;
			bool unattended = false;
			bool startedByMobiusClientStart = false;
			string username = "", msg;

			string argString = "";
			for (int ai = 0; ai < args.Length; ai++)
			{
				string arg = args[ai];

				Lex.AppendItemToStringList(ref argString, " ", arg); // append arg separating by a space

				if (Lex.Eq(arg, "username") && ai + 2 < args.Length)
					username = args[ai + 2].ToUpper();

				if (Lex.Eq(arg, "StartedByMobiusClientStart")) startedByMobiusClientStart = true;

				if (Lex.Eq(arg, "unattended")) unattended = true;
			}
			 
			string machineName = Environment.MachineName;
			LogDetailedMessages = // flag to do special detailed logging
				Lex.Contains(machineName, "<server>");

			// If not started by MobiusClientStart then do our own check to see if we are the current version

			if (!startedByMobiusClientStart) 
				UpdateClientVersionIfNecessary(); 

			ClientLog.Initialize(username); // initialize client logging
			ClientLog.Message("Mobius.Client started, ParmString: " + argString);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false); // must be done before first window is opened

			/* 
			 * DevExpress.XtraEditors.WindowsFormsSettings.SetDPIAware(); // don't do unless Mobius refactored as a DPI aware app (e.g. DX LayoutControl) 
			 * Note that setting Override high DPI scaling behavior = System (Enhanced) for the Mobius app can be used
			 * to provide better Win 10 rendering for high DPI displays and/or non-100% display scaling
			 * 
			 * To prevent the DevExpress de, es, ja, ru folders from being created delete the language files
 			 * under: C:\Program Files(x86)\DevExpress 19.1\Components\Bin\Framework (replace the version with the current version)
			 */

			// Set up handling of unexpected exits

			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(ThreadException);  // Occurs when an untrapped thread exception is thrown
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException); // Always route exceptions to the System.Windows.Forms.Application.ThreadException handler

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			Application.ThreadExit += new EventHandler(ThreadExit);
			Application.ApplicationExit += new EventHandler(ApplicationExit);

			ApplicationRecovery.OnApplicationCrash += new ApplicationRecovery.ApplicationCrashHandler(ApplicationRecovery_OnApplicationCrash); // try to catch crashes not caught via c# exceptions
			ApplicationRecovery.RegisterForRestart();

			/////////////////////
			// Development tests
			/////////////////////

			//SpotfireApiClient.UseAnalystClient = true; // debug

			bool RunDevTest = false;
			if (RunDevTest) // && ClientState.IsDeveloper) // do a quick dev test & then exit
			{
				if (DebugMx.True)
				{
					Mobius.KekuleJs.KekuleTestForm f = new Mobius.KekuleJs.KekuleTestForm();
					Application.Run(f);
					return;
				}

				if (DebugMx.False)
				{
					MoleculeViewer v = new MoleculeViewer();
					Application.Run(v);
					return;
				}

				if (DebugMx.False)
				{
					//NCDK_Example.DepictionGenerator_Example.Run();

					//string version = java.lang.Package.getPackage("java.lang").getImplementationVersion();

					//StreamReader sr = new StreamReader(@"c:\downloads\aspirin.mol");
					//string molfile = sr.ReadToEnd();
					//sr.Close();

					//IAtomContainer ac = CdkMol.MolfileToAtomContainer(molfile);

					//if (DebugMx.False) // Helm editor
					//{
					//	Helm.CefMx.InitializeCef();
					//	HelmEditorDialog editor = new HelmEditorDialog();
					//	string helm = "PEPTIDE1{A.L.C}$$$$";
					//	string newHelm = editor.Edit(helm);
					//	return;
					//
				}


				if (DebugMx.False)
				{
					RelatedCompoundsDialog rcd = new RelatedCompoundsDialog();
					rcd.QueryMolCtl.SetPrimaryTypeAndValue(MoleculeFormat.Smiles, "CC(=O)OC1=CC=CC=C1C(=O)O"); // aspirin
					Application.Run(rcd);
					return;
				}

				if (DebugMx.False) // Spotfire API test form
				{
					Application.Run(new Mobius.SpotfireClient.TestMobiusSpotfireApiForm());
					//Application.Run(new Mobius.SpotfireClient.ScatterPlotPropertiesDialog());
					return;
				}

				//string shares = DirectoryMx.GetSharedFolderAccessRule(); // get shares


				//if (DebugMx.False) // test old Spotfire Webplayer interface 
				//{
				//	Application.Run(new Mobius.SpotfireClient.TestWebplayerInterfaceForm());
				//	return;
				//}

				//Mobius.ComOps.SVGParser.Test();

				//string windowsLoginName = WindowsIdentity.GetCurrent().Name;
				//if (Lex.Eq(windowsLoginName, "<UserDomainAndName>")) // debugging settings
				//{
				//  CommandLineService.ExecuteCommand("Set DebugQE true");
				//  CommandLineService.ExecuteCommand("Set DebugDbCmd true");
				//}

				//if (DebugMx.True) while (true) { new BitmapTest().ShowDialog(); }

				//RunDevTestMethod();
				//return;
			}

			///////////////////////////
			// End of Development tests
			///////////////////////////

			// Check for required software versions

			if (!SessionManager.CheckForCorrectSoftwareVersions())
				Environment.Exit(-1);

			// Put up splash screen

			Splash.Unattended = unattended;
			LogDetailedMessage("Putting up splash");

			try
			{
				//if (Math.Abs(1) == 1) throw new Exception("Fatal Error Test"); // debug
				//if (!Lex.Contains(argString, "Attempt 3")) throw new Exception("Fatal Error Test"); // debug

				IniFile iniFile = null;

				// Put up splash screen

				if (splash == null)
					splash = Splash.ShowForm();

				iniFile = MobiusClientUtil.OpenAppropriateClientIniFile();
				if (iniFile == null) throw new Exception("MobiusClient.ini not found");

				msg = "Startup iniFile: " + iniFile.IniFileName;
				ClientLog.Message(msg);

				// Initialize services and get a session, session is created asynch while rest of client code loads in

				try
				{
					int t0 = TimeOfDay.Milliseconds();
					bool asynch = true; // initialize session asynch

					ServiceFacade.ServiceFacade.CreateSession(iniFile, argString, asynch);
					t0 = TimeOfDay.Milliseconds() - t0;
					LogDetailedMessage("ServiceFacade.Initialize time (asynch): " + t0);
				}
				catch (Exception ex)
				{
					msg = "ServiceFacade.Initialize failed:" + "\r\n\r\n" +
					DebugLog.FormatExceptionMessage(ex);
					DebugLog.Message(msg);
					ClientLog.Message(msg);
					MessageBoxMx.ShowError(msg);
					return;
				}

				LogDetailedMessage("Creating SessionManager");
				SessionManager = new SessionManager();

				SessionManager.Splash = splash;

				// Setup and display the Shell 

				LogDetailedMessage("Setting up Shell");

				Shell shell = new Shell(); // create the shell form
				SessionManager.ShellForm = shell;

				SessionManager.MainMenuControl = shell.MainMenuControl;
				SessionManager.HelpButton = shell.HelpButtonItem;
				SessionManager.RibbonCtl = shell.Ribbon;
				SessionManager.QuickSearchControl = shell.CommandLine;

				StatusBarManager sbm = SessionManager.StatusBarManager;
				SessionManager.StatusBarManager.StatusBarCtl = shell.StatusBar;

				LogDetailedMessage("Setting up Status Controls");
				sbm.SetupStatusControls(
					shell.RetrievalProgressBar,
					shell.RetrievalProgressButton,
					shell.RowCountCtl,
					shell.DatabaseSubsetButtonItem,
					shell.ClearFiltersCtl,
					shell.FiltersEnabledCtl,
					shell.FilterStringCtl);

				LogDetailedMessage("Setting up Zoom Controls");
				sbm.SetupViewZoomControls(
					shell.ZoomButtonItem,
					shell.ZoomPctBarItem,
					shell.ZoomSlider);

				SessionManager.QueriesControl = shell.QueriesControl;
				//SystemUtil.Beep();
				if (!SessionManager.Initialize(argString)) return; // Initialize the session, just return if init fails
																													 //SystemUtil.Beep();

				LogDetailedMessage("Calling Application.Run");
				Application.Run(shell); // show the shell & start message loop
				return; // app normally never gets here
			}

			catch (Exception ex) // unexpected fatal error
			{
				if (unattended && !argString.Contains("Attempt 3")) // try restart up to three times if unattended
				{
					int attempt = 1;
					if (!Lex.Contains(argString, "Attempt")) argString += " Attempt 2"; // get current attempt number and set new attempt
					else if (Lex.TryReplace(ref argString, "Attempt 2", "Attempt 3")) attempt = 2;
					else attempt = 3;

					ClientLog.Message("Startup error, Attempt: " + attempt + ", " + DebugLog.FormatExceptionMessage(ex));

					Process p = Process.Start(Application.ExecutablePath, argString);
					return;
				}

				LogUnexpectedError(ex); // log message and exit
				return;
			}
		}

		/// <summary>
		/// Check client version & start MobiusClientStart to do an update if current version is out of date
		/// </summary>

		public static void UpdateClientVersionIfNecessary()
		{
			if (MobiusClientUtil.UseMobiusClientUserIniFileAsMainIniFile) return; // skip if not using Mobius services

			if (!MobiusClientUtil.ClientVersionIsUpToDate())
			{
				try
				{
					string localMobiusClientStartPath = MobiusClientUtil.GetMobiusBaseDirectoryFilePath("MobiusClientStart.exe");
					if (!System.IO.File.Exists(localMobiusClientStartPath)) return;

					Process.Start(localMobiusClientStartPath);
					Thread.Sleep(1 * 1000);
					Environment.Exit(-1); // exit process immediately
				}
				catch (Exception ex)
				{
					string msg = DebugLog.FormatExceptionMessage(ex, "Error checking client version");
					DebugLog.Message(msg);
					return;
				}
			}
		}

		/// <summary>
		/// Thread exit
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		static void ThreadExit(object sender, EventArgs e)
		{
			ClientLog.Message("ThreadExit");
		}

		/// <summary>
		/// Thread exception not otherwise caught in code
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		static void ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			Exception ex = e.Exception;
			string msg = DebugLog.FormatExceptionMessage(ex);

			if (Lex.Contains(msg, "LayoutViewPainter.Draw")) // ignore this exception, sometimes occurs within the DevExpress Grid control layout view 
				return; // e.g.substructure search with just key & structure selected

			LogUnexpectedError(ex);

			return;
		}

		/// <summary>
		/// Some other exception in the current domain, perhaps in another thread
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;
			if (e == null)
				ex = new Exception("UnhandledException: " + e.ToString());
			LogUnexpectedError(ex);
			return;
		}

		/// <summary>
		/// Log fatal error & display to user
		/// </summary>
		/// <param name="ex"></param>

		static void LogUnexpectedError(Exception ex)
		{
			string msg = DebugLog.FormatExceptionMessage(ex);
			ClientLog.LogFatalErrorMessage(msg); // special log
			ClientLog.Message(msg); // regular log
			MessageBoxMx.ShowError(msg); // show to user
		}

		/// <summary>
		/// Detailed message loging
		/// </summary>
		/// <param name="msg"></param>

		public static void LogDetailedMessage(string msg)
		{
			if (!LogDetailedMessages) return;

			ClientLog.Message(msg);

			return;
		}


		/// <summary>
		/// Test that required assemblies can be loaded. Used for testing only. Should be commented out for
		/// production version to avoid delay that occurs when assemblies are initially loaded.
		/// </summary>
		static void LoadAssemblyTest()
		{
#if false // should be false other then when testing
					Microsoft.Vbe.Interop.vbext_ProjectType vbPt =
						Microsoft.Vbe.Interop.vbext_ProjectType.vbext_pt_HostProject;
#endif
			return;
		}

		/// <summary>
		/// Handler invoked on Application exit to attempt to dispose of the user session
		/// If 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		static void ApplicationExit(object sender, EventArgs e)
		{
			try
			{
				DebugLog.Message("Exiting");
				ServiceFacade.ServiceFacade.DisposeSession();
			}
			catch (Exception ex)
			{
				ClientLog.Message(DebugLog.FormatExceptionMessage(ex)); // just log it
			}
		}

		/// <summary>
		/// Handler invoked for "application crash"  
		/// </summary>

		static void ApplicationRecovery_OnApplicationCrash()
		{
			Exception e = new Exception("ApplicationRecovery_OnApplicationCrash event occurred");
			LogUnexpectedError(e);
			ApplicationRecovery.ApplicationRecoveryFinished(true);
			return;
		}

		/// <summary>
		/// Run development test without delay/overhead of doing a full Mobius startup
		/// </summary>

		static void RunDevTestMethod()
		{
			bool Y = DebugMx.True; // run code (avoid compiler warning)
			bool N = DebugMx.False; // don't run code

			if (N)
			{
				Mobius.CdkSearchMx.CdkSimSearchMx.Test("12345 12345");
				Application.Exit();
			}

			if (N)
			{
				CommandLineService.ExecuteCommand("Test Experimental Code");
				Application.Exit();
			}

			if (N)
			{
				Environment.Exit(-1); 
				//Application.Exit();
			}

			if (N)
			{
				List<string> members = Mobius.UAL.ActiveDirectoryDao.GetGroupMembers(@"<domain>\Approved_Mobius_Chemview");
				return;
			}

		}

	}
}
