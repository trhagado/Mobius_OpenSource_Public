using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;
using Mobius.Helm;

using SF = Mobius.ServiceFacade;
using Mobius.ServiceFacade;

using NSC = Mobius.NativeSessionClient;

using DevExpress.XtraEditors;
using Dx = DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.Utils;

using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.InteropServices;

namespace Mobius.ClientComponents
{
	public class SessionManager : ISessionManager
	{
		static public SessionManager Instance; // singleton instance

// The following are set by the calling shell program

		public Form ShellForm; // form serving a host for controls
		public string ShellFormSize = ""; // most-recent ShellForm window state and size
		public Form Splash; // splash form put up at initiation

		public MainMenuControl MainMenuControl; // any allocated main menu control
		public BarButtonItem HelpButton; // control with help button
		public RibbonControl RibbonCtl; // any ribbon control on main form

		public BarEditItem QuickSearchControl; // any quick search control on ribbon

		public StatusBarManager StatusBarManager = new StatusBarManager(); // info on StatusBar

		public QueryManager QueryManager // current query manager, maintained in statusbarmanager
			{ get { return StatusBarManager != null ? StatusBarManager.QueryManager : null; }}

		public QueriesControl QueriesControl; // any queries and results control on main form
		public MRUEdit CommandLineControl; // QuickSearch command line control

		public bool InShutDown = false; // in process of shutting down the app
		public static bool IsInShutDown // static in process of shutting down the app
		{	get	{ return Instance != null ? Instance.InShutDown : false;  } }

		static bool MainFormControlPositionsAdjusted = false;

// Derived references

        public QbContentsTree MainContentsControl // main database contents control
		{ get { return QueriesControl != null ? QueriesControl.QueryBuilderControl.QbContentsCtl : null; } }

		public ContentsTreeControl MainContentsTree // main database contents control tree
		{ get { return QueriesControl != null ? QueriesControl.QueryBuilderControl.QbContentsCtl.QbContentsTreeCtl : null; } }

		public QueryTablesControl QueryTablesControl // Control containing tables for current query
		{ get { return QueriesControl != null ? QueriesControl.QueryBuilderControl.QueryTablesControl : null; } }

		public Query QueryBuilderQuery // Current query in QueryBuilder
		{ get { return QueryTablesControl != null ? QueryTablesControl.Query : null; } }

		public QueryResultsControl QueryResultsControl // Control containing current query results
		{ get { return QueriesControl != null ? QueriesControl.QueryResultsControl : null; } }

		public QueryManager QueryResultsQueryManager // QueryManager for currently displayed results
		{ get { return QueryResultsControl != null ? QueryResultsControl.CrvQm : null; } }

		public static bool LogStartupDetails = false;

/// <summary>
/// Current Query
/// </summary>

		public static Query CurrentQuery
		{
			get
			{
				QueryManager qm = Instance.QueryManager;
				if (qm != null) return qm.Query;
				else return null;
			}
		}

		/// <summary>
		/// Current result keys
		/// </summary>

		public static List<string> CurrentResultKeys
		{ 
			get
			{
				QueryManager qm = Instance.QueryManager;

				if (qm == null || qm.Query == null || qm.DataTableManager == null)
					return new List<string>();

				Query q = qm.Query;

				if (q.Mode == QueryMode.Browse && MqlUtil.SingleStepExecution(q))
				{ // if browsing single-step query then get proper list
					qm = Instance.QueryResultsQueryManager; 
					if (qm == null || qm.Query == null || qm.DataTableManager == null)
						return new List<string>();
				}

				List<string> keyList  = qm?.DataTableManager?.ResultsKeys;
				if (keyList != null)
					return new List<string>(qm?.DataTableManager?.ResultsKeys); // return a copy of the list

				else return new List<string>();
			}

			set 
			{
				QueryManager qm = Instance.QueryManager;

				if (qm != null && qm.DataTableManager != null)
					qm.DataTableManager.ResultsKeys = new List<string>(value); // save a copy of the list
			}
		}

		public static int CurrentResultKeysCount { get { return CurrentResultKeys.Count; } } // Count of current result keys
		public static int CurrentCountNotNull { get { return CurrentResultKeysCount >= 0 ? CurrentResultKeysCount : 0; } } // return non-negative current count
		public static int CurrentListId; // temp UserObject id assigned to current list for this session

/// <summary>
/// Basic constructor
/// </summary>

		public SessionManager ()
		{
			Instance = this; // save ref to singleton instance
			return;
		}

/// <summary>
/// Init session
/// </summary>
/// <param name="args"></param> 

		public bool Initialize(string argString)
		{
			UserObject uo;
			string initialCommand = "";
			string msg, tok, fn;
			bool b1 = false;
			int startupTimer;

			DisplayStartupMessage("Initializing...");

			try // surrounding try block
			{
				LogStartupMessage("Initialize startup parameters: " + argString);

				// Set up handler to allow logging of assembly loads

				PrintLoadedAssemblies();
				AppDomain domain = AppDomain.CurrentDomain;
				domain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoadEventHandler);

				domain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);

				// Parse out arguments

				string usernameArg = null;
				Lex lex = new Lex();
				lex.SetDelimiters(" , ; = ");
				lex.OpenString(argString);
				while (true)
				{
					tok = lex.Get();
					if (tok == "") break;

					if (Lex.Eq(tok, "Attended"))
						SS.I.Attended = true;

					else if (Lex.Eq(tok, "Unattended")) // run noninteractively via script
						SS.I.Attended = false;

					else if (Lex.Eq(tok, "UserName")) // acct to log in as
					{
						tok = lex.Get();
						if (tok == "=") tok = lex.Get();
						usernameArg = tok;
					}

					else if (Lex.Eq(tok, "Command") ||  // initial command (e.g. Command='SelectAllCompoundData 12345')
					 Lex.Eq(tok, "Mobius:Command")) // form used in Run_Query_123456.bat file (e.g. "C:\xxx\Mobius\Bin\MobiusClient.exe" Mobius:Command='Run Query 377898')
					{
						tok = lex.Get();
						if (tok == "=") tok = lex.Get();
						initialCommand = Lex.RemoveAllQuotes(tok);
						initialCommand = initialCommand.Replace("%20", " "); // convert any special URL characters (todo: more complete translation)
					}
				}

				// Copy executable file path and it's containing directory

				ClientDirs.ExecutablePath = Application.ExecutablePath;
				ClientDirs.StartupDir = Application.StartupPath;

				SS.I.ClientOSVersion = System.Environment.OSVersion.ToString();
				SS.I.ClientMachineName = System.Environment.MachineName;

				SS.I.MobiusClientVersion =
					System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

				ClientState.MobiusClientVersion = SS.I.MobiusClientVersion; // also set in local security setting

				// Create access to inifiles

				SS.I.IniFile = // app specific settings
					IniFile.OpenClientIniFile("MobiusClient.ini");

				SS.I.UserIniFile = // user specific settings
					IniFile.OpenClientIniFile("MobiusClientUser.ini");

				SS.I.StandAlone = SS.I.UserIniFile.ReadBool("StandAlone", false);
				LogWindow.Display = SS.I.UserIniFile.ReadBool("DisplayLogWindow", false);

				LogStartupDetails = // log startup timing
					SS.I.UserIniFile.ReadBool("LogStartupDetails", false);
				SF.ServiceFacade.LogStartupDetails = LogStartupDetails;

				SS.I.AllowAdminFullObjectAccess = // special admin access to other's user objects
					SS.I.UserIniFile.ReadBool("AllowAdminFullObjectAccess", false);

				if (SS.I.UserIniFile.ReadBool("DisposeServiceSession", true)) // setup handler to dispose of service session
					Application.ApplicationExit += new EventHandler(DisposeServiceSession);

				// Create app directories

				//LogStartupMessage("CommonApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
				//LogStartupMessage("MyDocuments: " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

				CreateDirectory(ClientDirs.DefaultMobiusUserDocumentsFolder);
				CreateDirectory(ClientDirs.TempDir);
				CreateDirectory(ClientDirs.CacheDir);

				TempFile.TempDir = ClientDirs.TempDir;

				try
				{ Directory.SetCurrentDirectory(ClientDirs.DefaultMobiusUserDocumentsFolder); } // make Mobius dir current
				catch (Exception ex)
				{ LogStartupMessage("Error in SetCurrentDirectory: " + ex.Message); }

				ClientDirs.CurrentDir = System.Environment.CurrentDirectory;

				// Start timing

				startupTimer = TimeOfDay.Milliseconds();

				// User-specific settings

				DataTableManager.DebugBasics = SS.I.UserIniFile.ReadBool("DebugDataTableManagerBasics", false);
				DataTableManager.DebugDetails = SS.I.UserIniFile.ReadBool("DebugDataTableManagerDetails", false);
				DataTableManager.DebugCaching = SS.I.UserIniFile.ReadBool("DebugDataTableManagerCaching", false);

				ViewManager.Debug = SS.I.UserIniFile.ReadBool("DebugViewManager", false);
				Mobius.SpotfireClient.SpotfireApiClient.Debug = SS.I.UserIniFile.ReadBool("DebugSpotfireApiClient", false);

				JavaScriptManager.SuppressJavaScriptErrors = SS.I.UserIniFile.ReadBool("SuppressJavaScriptErrors", true);

				if (SS.I.UserIniFile.TryReadBool("HelmEnabled", ref b1)) // this overrides the standalone QE if defined
					MoleculeMx.HelmEnabled = b1;

				// Read other client directory parameters

				ClientDirs.PluginConfigDir = SS.I.IniFile.Read("PluginConfigDirectory", ClientDirs.PluginConfigDir); // plugin configuration info
				ClientDirs.MiscConfigDir = SS.I.IniFile.Read("MiscConfigDirectory", ClientDirs.MiscConfigDir); // scripts & templates
				CommonConfigInfo.MiscConfigDir = ClientDirs.MiscConfigDir; // copy misc config dir to common area

				// Copy misc .jpg config files

				fn = "";
				try
				{
					string[] files = Directory.GetFiles(ClientDirs.MiscConfigDir);
					for (int fi0 = 0; fi0 < files.Length; fi0++)
					{
						fn = files[fi0];
						if (!Lex.EndsWith(fn, ".jpg")) continue;
						try { File.Copy(fn, ClientDirs.TempDir + @"\" + Path.GetFileName(fn)); }
						catch (Exception ex) { ex = ex; } // ignore if locked or other problem
					}
				}

				catch (Exception ex)
				{ LogStartupMessage("Error copying temp file: " + fn + ", " + ex.Message); }

				// Delete temp files that are over a day old

				fn = "";

				int skipCnt = 0, deleteCnt = 0, failCnt = 0;
				try  
				{
					string[] files = Directory.GetFiles(ClientDirs.TempDir);
					for (int fi0 = 0; fi0 < files.Length; fi0++)
					{
						fn = files[fi0];
						DateTime dt = FileUtil.GetFileLastWriteTime(fn);
						TimeSpan ts = DateTime.Now.Subtract(dt); // more than one day old?
						if (ts.TotalDays < 1)
							skipCnt++;
						else try
							{
								File.Delete(fn);
								deleteCnt++;
							}
							catch (Exception ex) { failCnt++; } // ignore if locked or other problem
					}
				}

				catch (Exception ex)
				{ LogStartupMessage("Error deleting temp file: " + fn + ", " + ex.Message); }


				// Do service initialization and session creation if not done yet

				if (!SF.ServiceFacade.SessionCreated)
					try
					{
						DisplayStartupMessage("Connecting to Mobius services...");

						if (!SF.ServiceFacade.InCreateSession)
						{ // startup session creation if not already done yet
							IniFile clientServicesIniFile = // see if service info is in MobiusClientUser.ini & use if so
								IniFile.OpenClientIniFile("MobiusClientUser.ini");

							if (clientServicesIniFile == null || clientServicesIniFile.Read("UseMobiusServices") == "") // otherwise use MobiusClient.ini
								clientServicesIniFile = IniFile.OpenClientIniFile("MobiusClient.ini");
							//CommonConfigInfo.ServicesIniFile = clientServicesIniFile; // (incorrect)

							bool asynch = true;
							SF.ServiceFacade.CreateSession(clientServicesIniFile, argString, asynch); // start asynch creation of link to services
						}

						startupTimer = TimeOfDay.Milliseconds();

						int timeout = 20000;
						int createSessionTime = 0;
						while (SF.ServiceFacade.InCreateSession && SF.ServiceFacade.CreateSessionException == null)
						{
							createSessionTime = TimeOfDay.Milliseconds() - startupTimer;
							if (createSessionTime > timeout)
								throw new Exception("Connection timed out, Mobius services or server are currently unavailable");

							Thread.Sleep(100);
						}

						if (SF.ServiceFacade.CreateSessionException != null)
							throw SF.ServiceFacade.CreateSessionException;

						if (LogStartupDetails)
							startupTimer = LogStartupTimeMessage("CreateSession time: ", startupTimer);
					}

					catch (Exception ex)
					{
						msg = "[Startup] - ServiceFacade.CreateSession failed:\n" + DebugLog.FormatExceptionMessage(ex);
						ClientLog.Message(msg);

						msg =
							"Mobius can't continue with this session because the Mobius server or services are currently unavailable.\n" +
							"Normally this issue is corrected reasonably quickly; however, if it persists you can contact your\n" +
							"local IT Service Desk to verify that the problem has been reported and for a status update.\n" +
							"If the problem is unreported then you can request a reboot of the Mobius server ([server])\n" +
							"which will normally correct the problem.";
						MessageBoxMx.ShowError(msg);
						return false;
					}

				//UserObjectDao.Ping(); // check that a service works (debug)

				//ServiceFacade.ServiceFacade.IDebugLog = new DebugLogMediator(); // so ServiceFacade can log to ClientLog
				ServiceFacade.ServiceFacade.IClient = new ClientComponents.IClientClass();
				ServiceFacade.ServiceFacade.IQueryExec = new QueryExec(); // so services can call back into QueryExec

				// Check for any fatal client error from previous session & send message to server if one exists

				try
				{
					string fatalMsg = ClientLog.GetFatalErrorMessage();
					if (Lex.IsDefined(fatalMsg))
					{
						fatalMsg =
							"Unexpected Mobius client exception from previous client session: \r\n" +
							"        " + fatalMsg + "\r\n" +
							"=====>>> End of unexpected Mobius client exception from previous client session";

						ServicesLog.Message(fatalMsg);
						ClientLog.DeleteFatalErrorMessage();
					}
				}
				catch (Exception ex)
				{
					ClientLog.Message(DebugLog.FormatExceptionMessage(ex));
				}

				// if using remote services copy MobiusServices.ini from the server and define it as ServicesIniFile for access from the client perspective

				bool debugRemoteIniFileCopy = false;
				if (SF.ServiceFacade.UseRemoteServices || debugRemoteIniFileCopy)
				{
					try
					{

						StreamWriter sw = null;

						string iniFileString = UalUtil.GetServicesIniFile(); // get the inifile from the server

						string iniFileName = TempFile.GetTempFileName(ClientDirs.TempDir, ".MobiusServices.ini"); // always get unique name for session

						if (debugRemoteIniFileCopy)
							ServicesLog.Message("Services IniFileName and length: " + iniFileName + " (" + iniFileString.Length + ")");

						try // save the file
						{ // may throw an exception if another session on this machine is is writing this file at the same time
							sw = new StreamWriter(iniFileName);
							sw.Write(iniFileString); // write it out locally
							sw.Close();
							////break;
						}
						catch (Exception ex)
						{
							try { sw.Close(); }
							catch (Exception ex2) { ex2 = ex2; }
						}

						ServicesIniFile.Initialize(iniFileName);
						string testParm = ServicesIniFile.Read("NativeServicesExeFilePath");
						if (debugRemoteIniFileCopy)
							ServicesLog.Message("Standalone NativeServicesExeFilePath: " + testParm);
					}

					catch (Exception ex)
					{
						msg = DebugLog.FormatExceptionMessage(ex);
						ServicesLog.Message(msg);
						MessageBoxMx.ShowError(msg);
					}
				}

				// Set interface look and feel

				SS.I.LookAndFeel = SS.I.UserIniFile.Read("LookAndFeel", "Blue");
				PreferencesDialog.SetLookAndFeel(SS.I.LookAndFeel);

				// Obsolete
				//string backColorString = SS.I.UserIniFile.Read("MainMenuBackgroundColor");
				//if (MainMenuControl != null && !String.IsNullOrEmpty(backColorString))
				//	try
				//	{
				//		string[] sa = backColorString.Split(',');
				//		int red = int.Parse(sa[0]);
				//		int green = int.Parse(sa[1]);
				//		int blue = int.Parse(sa[2]);
				//		Color c = Color.FromArgb(red, green, blue);
				//		MainMenuControl.BackColor = c;
				//		MainMenuControl.MainMenu.BackColor = c;
				//		if (Lex.Contains(SS.I.LookAndFeel, "Black") || // if a black look set white font color
				//		 Lex.Contains(SS.I.LookAndFeel, "Dark") || Lex.Contains(SS.I.LookAndFeel, "Sharp"))
				//			MainMenuControl.SetFontColor(Color.White);
				//		else MainMenuControl.SetFontColor(SystemColors.ControlText);

				//	}
				//	catch (Exception ex) { }

				// Attach context menu to MS Office 2007 style Help button (not currently used)
				//if (HelpButton != null && MainMenuControl != null)
				//{
				//  PopupMenu pm = MainMenuControl.HelpContextMenu;
				//  HelpButton.DropDownControl = pm;
				//}

				QueryResultsControl.ShowResultsViewTabs = // show or hide the full set of results views tabs
					SS.I.UserIniFile.ReadBool("ShowResultsViewTabs", false);

				if (QueriesControl != null && QueriesControl.QueryResultsControl != null)
					QueriesControl.QueryResultsControl.Tabs.Visible = true;

				// Restore to prev position & size

				if (ShellForm != null)
				{
					if (UIMisc.RestoreWindowPlacement(ShellForm, "ShellFormPlacement") && QueriesControl != null)
					{ // also restore size of right panel
						try
						{
							tok = SS.I.UserIniFile.Read("ShellFormSplitterPosition");
							QueriesControl.QueryBuilderControl.InitialSplitterPos = Int32.Parse(tok); // store position to be set when first painted
						}
						catch (Exception ex) { }
					}

					ShellForm.Shown += new System.EventHandler(Shell_Shown); // pickup shown event for form

					//Progress.OwnerFormToUse = ShellForm;
				}

				tok = SS.I.UserIniFile.Read("RibbonQuickAccessToolbarLocation", "Below");
				if (RibbonCtl != null && !String.IsNullOrEmpty(tok))
				{
					if (tok == "Below") RibbonCtl.ToolbarLocation = RibbonQuickAccessToolbarLocation.Below;
					else RibbonCtl.ToolbarLocation = RibbonQuickAccessToolbarLocation.Above;
				}


				// Setup event handler for ribbon quicksearch control

				if (QuickSearchControl != null)
				{ // setup handler for when quicksearch control is shown
					QuickSearchControl.ShownEditor += new DevExpress.XtraBars.ItemClickEventHandler(CommandLine_ShownEditor);
				}

				DoFoundationDependencyInjections();

				// See if running in StandAlone mode

				if (SS.I.StandAlone)
				{
					InitializeForStandAloneMode();
				}

				else
				{
					Exception ex = SF.ServiceFacade.CreateSessionException; // get any init exception

					if (ex == null) // if no exception ping a service to be sure they are responding in a basic way
						try
						{
							DateTime lastWriteTime = ServerFile.GetLastWriteTime(@"C:\Windows\System32\Cmd.exe");
							bool isMinValue = lastWriteTime == DateTime.MinValue;
						}
						catch (Exception ex2)
						{
							ex = ex2;
						}

					if (ex != null)
					{
						if (Lex.Contains(ex.Message, "Attempted to perform an unauthorized operation"))
						{ // probably not a Mobius user
							msg = "User " + System.Environment.UserName.ToUpper() + " may not be a registered Mobius user.";
						}
						else
						{
							if (Lex.StartsWith(ex.Message, "Could not connect to ") && // standard message if services not running
							 Lex.Contains(ex.Message, "No connection could be made because the target machine actively refused it"))
								msg = "The Mobius services are not currently available."; // use something a bit more user-friendly
							else msg = DebugLog.FormatExceptionMessage(ex);
							msg = "Error connecting to Mobius Services:\r\n\r\n" + msg;
						}
						MessageBoxMx.ShowError(msg);
						return false;
					}

					SS.I.ConnectedToServices = true; // will be connected to server

					SF.ServiceFacade.StartSessionHeartbeat(); // keep server side alive as long as client is alive
				}

				// See if system is available, show message and exit if not

				string unavailableMsg = ServicesIniFile.Read("SystemUnavailableMessage");
				if (unavailableMsg != "")
				{
					msg = "Mobius is not currently available." + "\n\n" + unavailableMsg;
					SystemAvailabilityMsg.Show(msg, UmlautMobius.String + " Error", MessageBoxIcon.Error, Splash);
					Environment.Exit(0);
				}

				// Show any system warning message

				string warningMessage = ServicesIniFile.Read("SystemWarningMessage");
				if (warningMessage != "")
					SystemAvailabilityMsg.Show(warningMessage, UmlautMobius.String + " Warning", MessageBoxIcon.Warning, Splash);

				// Get list of valid privileges

				PrivilegesMx.SetValidPrivilegeListFromInifile(ServicesIniFile.IniFile);

				// Load datasource metadata

				try // load connection metadata
				{
					DisplayStartupMessage("Loading DataSources...");
					DbConnection.LoadMetaData();
					if (LogStartupDetails)
						startupTimer = LogStartupTimeMessage("Datasource metadata load time", startupTimer);
				}
				catch (Exception ex)
				{
					msg = "Error processing DataSources.xml: " + ex.Message;
					MessageBoxMx.ShowError(msg);
				}

				// Ping Oracle

				DisplayStartupMessage("Connecting to Database...");

				try
				{
					UserObjectDao.Ping(); // Check that we can read the user object table
				}
				catch (Exception ex)
				{
					msg = "Error connecting to the Mobius Database: " + DebugLog.FormatExceptionMessage(ex);
					MessageBoxMx.ShowError(msg);
					Environment.Exit(0);
				}

				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("Connect to Oracle time", startupTimer);

				// See if user is authorized for access & logon if so

				if (!String.IsNullOrEmpty(usernameArg))
				{
					SS.I.UserName = usernameArg;
				}
				else
				{
					SS.I.UserName = System.Environment.UserName.ToUpper();
					SS.I.UserDomainName = System.Environment.UserDomainName.ToUpper();
				}

				bool usingDebugUserName = CheckForDebugModeUserName(); // see if we need to switch to a particular username for debugging
				if (usingDebugUserName) MessageBox.Show("Using predefined debug user name: " + SS.I.UserName);

				DisplayStartupMessage("Authorizing...");

				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("Security.GetUserInfo time", startupTimer);

				string clientName = "MobiusClient - " + VersionMx.FormatVersion(SS.I.MobiusClientVersion);
				if (!Security.Logon(SS.I.UserName, SS.I.UserDomainName, clientName))
				{
					msg = "<br>User " + SS.I.UserName + " is not authorized for Mobius access.";
					string contactMsg = ServicesIniFile.Read("AccountCreationContactMsg"); // get message, may be html
					if (!Lex.IsNullOrEmpty(contactMsg)) msg += "<br><br>" + contactMsg;

					if (SS.I.Attended)
						MessageBoxMx.ShowWithCustomButtons(msg, UmlautMobius.String + " Error", "OK", null, null, null, MessageBoxIcon.Error, 500, 150);

					else throw new Exception(msg);

					return false;
				}

				SS.I.UserInfo = Security.GetUserInfo(SS.I.UserName); // get complete information for the user

				UsageDao.LogEvent("Begin");
				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("Security.Logon time", startupTimer);

				// Get server-side parameters

				SS.I.AllowGroupingBySalts = ServicesIniFile.ReadBool("ActivateSaltSiblings", true);
				DataTableManager.DefaultRowRequestSize = ServicesIniFile.ReadInt("DefaultRowRequestSize", 16);
				SS.I.UsePersonNameInPlaceOfUserName = ServicesIniFile.ReadBool("UsePersonNameInPlaceOfUserName", true);

				// Load preferences for user

				DisplayStartupMessage("Loading User Preferences...");
				Preferences.Load();
				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("Preference load time", startupTimer);

				SS.I.ShowAddNewViewTab = Preferences.GetBool("ShowAddNewViewTab", SS.I.ShowAddNewViewTab);

				SS.I.UseTibcoSpotfire = Preferences.GetBool("UseTibcoSpotfire", SS.I.UseTibcoSpotfire);

				// Get email address, prompting if necessary

				SS.I.UserInfo.EmailAddress = GetEmailAddress();
				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("GetEmailAddress time", startupTimer);

				// Check for any key databases that are unavailable

				string connectionsMessage = GetKeyOracleConnectionsMessage();
				if (connectionsMessage != "" && unavailableMsg == "") // show if no previous warning
					MessageBoxMx.Show(connectionsMessage, "Mobius Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("GetKeyOracleConnectionsMessage time", startupTimer);

				// Load dictionary metadata

				try
				{
					DisplayStartupMessage("Loading Dictionaries...");
					DictionaryFactory dictFact = new DictionaryFactory();
					DictionaryMx.LoadDictionaries(dictFact);

					if (LogStartupDetails)
						startupTimer = LogStartupTimeMessage("LoadDictionaryMetaData time", startupTimer);
				}
				catch (Exception ex)
				{
					msg = "Error loading dictionaries: " + DebugLog.FormatExceptionMessage(ex);
					MessageBoxMx.ShowError(msg);
				}

				// Load initial set of metatables

				try
				{
					DisplayStartupMessage("Loading Base Metatables...");
					MetaTableCollection.MetaTableFactory = new MetaTableFactory(); // factory to call to get metatables built
					MetaTableFactory.Initialize();

					MetaTable.KeyMetaTable = MetaTableCollection.GetDefaultRootMetaTable();

					RootTable.Build(); // Get RootTable information

					if (LogStartupDetails)
						startupTimer = LogStartupTimeMessage("MetaTableFactory.Build time", startupTimer);
				}
				catch (Exception ex)
				{
					msg = "Error processing table definitions: " + ex.Message;
					MessageBoxMx.ShowError(msg);
				}

				// Load the contents tree

				try
				{
					DisplayStartupMessage("Building database contents tree...");
					MetaTree.MetaTreeFactory = new MetaTreeFactory(); // factory to call to build metatree
					MetaTree.GetMetaTree(); // build the full tree
					if (LogStartupDetails)
						startupTimer = LogStartupTimeMessage("ContentsTree build time", startupTimer);
				}

				catch (Exception ex)
				{
					msg = "Exception building the database contents tree: ";
					MessageBoxMx.ShowError(msg + ex.Message);
					ServicesLog.Message(msg + DebugLog.FormatExceptionMessage(ex));
				}

				// Get set of user objects for preferred project

				DisplayStartupMessage("Building database contents tree (user objects)...");
				UserObjectTree.RetrieveAndBuildSubTree(SS.I.PreferredProjectId);

				// Initialize CommandExec, QueryEngine

				CommandExec.Initialize();
				QueryEngine.InitializeForSession();

				// Load plugins

				DisplayStartupMessage("Loading plugins...");
				try // load plugin definitions
				{
					string pluginConfigDir = SS.I.IniFile.Read("PluginConfigDirectory", ClientDirs.StartupDir + @"\PluginConfig");
					Plugins.LoadDefinitions(pluginConfigDir);
				}
				catch (Exception ex) { MessageBoxMx.ShowError("Error processing plugin definitions: " + ex.Message); }
				if (LogStartupDetails)
					startupTimer = LogStartupTimeMessage("Plugin definition load time", startupTimer);

				// Show the shell

				if (SS.I.Attended)
				{
					DisplayStartupMessage("Building the main window...");
					ShellForm.Show();
					Application.DoEvents();
					SetShellTitle("");

					if (Splash != null) Splash.BringToFront();
					Application.DoEvents();
					if (LogStartupDetails)
						startupTimer = LogStartupTimeMessage("ShellForm.Show time", startupTimer);

					// Start asynch build of full user object tree for current user and let user get started
					// As contents tree needs to be expanded get needed UserObjects from database until tree build complete.

					bool buildFullTree = ServicesIniFile.ReadBool("BuildFullUserObjectTree", true);
					if (buildFullTree) UserObjectTree.BuildTreeStart();
					else UserObjectTree.BuildComplete = true;

					MainContentsControl.ShowNormal(); // show empty tree for now

					if (LogStartupDetails)
						startupTimer = LogStartupTimeMessage("Format database contents tree time", startupTimer);

					try
					{
						Plugins.UpdateUI(); // update mainform with plugin menu items

						if (MainMenuControl != null) MainMenuControl.EnableToolsMenuItems();

						if (MainMenuControl != null) MainMenuControl.LoadMruList();
						if (LogStartupDetails)
							startupTimer = LogStartupTimeMessage("LoadMruList time", startupTimer);

						if (MainMenuControl != null) MainMenuControl.LoadFavorites(); // load favorites menu
						if (LogStartupDetails)
							startupTimer = LogStartupTimeMessage("LoadFavorites time", startupTimer);

						CidList currentList = new CidList();
						CidListCommand.WriteCurrentList(currentList); // init empty current list
						uo = CidListCommand.ReadCurrentListHeader();
						CurrentListId = uo.Id; // get the id to be used for this sessions

						DisplayCurrentCount(); // display initially empty current count

						// Open current set of queries if wanted by user

						string cws = ""; // last window set
						if (SS.I.RestoreWindowsAtStartup)
						{
							cws = Preferences.Get("CurrentWindowSet");
							if (cws.EndsWith("\n") && cws.Length > 1) cws = cws.Substring(0, cws.Length - 1);
							string[] sa = cws.Split('\n');
							for (int wi = 0; wi < sa.Length; wi++)
							{
								string[] sa2 = sa[wi].Split('\t');
								if (sa2.Length != 2 || sa2[0] == "" || sa2[1] == "") continue;
								if (sa2[0] == DocumentType.Query.ToString())
								{
									string queryName = sa2[1];
									int i1 = queryName.IndexOf("FOLDER_");
									if (i1 > 0) queryName = queryName.Substring(i1); // remove owner if included
									if (Lex.StartsWith(queryName, "QUERY_"))
									{ // be sure query still exists
										uo = UserObject.ParseInternalUserObjectName(queryName, "");
										uo = UserObjectDao.ReadHeader(uo.Id);
										if (uo == null) continue;
										DisplayStartupMessage("Opening query: " + uo.Name);
									}

									bool allowBrowse = (wi == sa.Length - 1);
									QbUtil.OpenQuery(queryName, false, allowBrowse, false); // open query & get any next command
								}

								else continue; // unexpected type
							}
						}

						StatusBarManager.AdjustRetrievalProgressBarLocation();
						StatusBarManager.DisplayFilterCounts();

						if (SS.I.Attended) // see if we need to do any background imports for automatic update annotation tables / user structure databases
							UserData.CheckForCurrentUserImportFileUpdates();

						UpdateMobiusClientStartAsNeeded(); // If needed update MobiusClientStart module

						if (Splash != null) Splash.Hide();

						ActivateQuickSearchControl();
					}

					catch (Exception ex) // may be able to continue even if one of these operations failed
					{
						msg = DebugLog.FormatExceptionMessage(ex);
						DebugLog.Message(msg);

						DialogResult dr = MessageBoxMx.Show(
							"Mobius experienced the startup error listed below.\r\n" +
							"You may be able to continue normally with this session\r\n" +
							"Do you want to continue?\r\n\r\n" + msg,
							"Startup Error", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error, 600, 200);

						if (dr != DialogResult.Yes) Environment.Exit(-1);
					}

					if (QbUtil.DocumentList.Count == 0) // start with new query if nothing existing
						QbUtil.NewQuery();

					//string currentQueryName = (QbUtil.Query != null ? QbUtil.Query.UserObject.Name : "");
					//SetShellTitle(currentQueryName);
				} // end of UI setup

				if (!String.IsNullOrEmpty(initialCommand))
				{
					if (usernameArg != null)
					{
						SS.I.UserName = usernameArg;
						ServiceFacade.Security.SetUser(usernameArg);
					}
					string result = CommandExec.ExecuteCommand(initialCommand); // execute any initial command

					if (!SS.I.Attended) // all done if not attended
					{
						ShutDown();
					}
				}

				try { CreateIntegrationPoint(null); } // create integration point for other apps to call Mobius
				catch (Exception ex) { ClientLog.Message("CreateIntegrationPoint failed: " + ex.Message); } // log message & continue if fails

				LogStartupMessage("Initialize complete.");
				return true; // successfully initialized
			} // end of surrounding try block

			catch (Exception ex)
			{
				try
				{
					msg = DebugLog.FormatExceptionMessage(ex);
					ServicesLog.Message(msg); // try to write to server
					XtraMessageBox.Show(msg, "Unexpected Mobius Initialization Exception");
					return false;
				}
				catch (Exception ex2) { return false; } // ignore errors
			}

			finally
			{
				LogStartupDetails = false; // turn off so we don't do anymore startup logging in case it was turned on
			}

		}

		/// <summary>
		/// Do Foundation dependency injections
		/// </summary>

		public static void DoFoundationDependencyInjections()
		{
			Mobius.Data.ResultsViewProps.ResultsViewFactory = new Mobius.ClientComponents.ViewFactory();
			Mobius.Data.Query.IDataTableManager = new Mobius.ClientComponents.DataTableManager();
			Mobius.Data.CidList.ICidListDao = new Mobius.ClientComponents.CidListCommand();
			Mobius.Data.MoleculeMx.IMoleculeMxUtil = new Mobius.ServiceFacade.MoleculeUtil();
			Mobius.Data.RootTable.IServerFile = new Mobius.ServiceFacade.ServerFile();

			Mobius.Data.InterfaceRefs.IUserObjectDao = new Mobius.ServiceFacade.IUserObjectDaoMethods();
			Mobius.Data.InterfaceRefs.IUserObjectIUD = new Mobius.ClientComponents.IUserObjectIUDMethods();
			Mobius.Data.InterfaceRefs.IUserObjectTree = new Mobius.ClientComponents.IUserObjectTreeMethods();

			Mobius.Data.MolLibFactory.I = new CdkMolFactory();
		}

		/// <summary>
		/// AdjustMainMenuPosition
		/// </summary>

		public static void AdjustMainMenuPosition()
		{
			PreferencesDialog.SetMainMenuTopPosition();
			return;
		}

		/// <summary>
		/// AdjustMainFormControlPositionsAsNeeded
		/// </summary>

		public static void AdjustMainFormControlPositionsAsNeeded()
		{
			if (!MainFormControlPositionsAdjusted)
			{
				if (Instance?.StatusBarManager != null)
					Instance.StatusBarManager.AdjustRetrievalProgressBarLocation();

				MainFormControlPositionsAdjusted = true;
			}
		}

		/// <summary>
		/// Create directory, logging any error
		/// </summary>
		/// <param name="path"></param>

        void CreateDirectory(string path)
		{
			try
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			catch (Exception ex)
			{
				LogStartupMessage("Error creating directory: " + path);
			}

			return;
		}

/// <summary>
/// Log AssemblyLoad events
/// </summary>
/// <param name="sender"></param>
/// <param name="args"></param>

		static void AssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args) 
		{
			if (!LogStartupDetails) return;

			LogStartupMessage("Loaded assembly: " + args.LoadedAssembly.FullName);
			//if (Lex.Contains(args.LoadedAssembly.FullName, "BonusSkins")) return; // debug
			return;
		}

		/// <summary>
		/// Potentially resolve any issues related to Assembly loading
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>

		private static Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
		{
			// Note that this method seems to get called a lot without any real problems appearing and it appears that these can
			// safely be ignored most of the time.
			// Example:
			//  Name of the item to resolve: ClientComponents.resources, Version=5.0.7200.22115, Culture=en-US, PublicKeyToken=null
			//  at Mobius.ClientComponents.SessionManager.AssemblyResolveEventHandler(Object sender, ResolveEventArgs args) 
			//  in C:\Mobius_OpenSource\MobiusClient\ClientComponents\SessionManager\SessionManager.cs:line 1023

			if (!LogStartupDetails) return null;

			string name = args.Name;

			string msg = "Name of the item to resolve: " + args.Name;
			if (args.RequestingAssembly != null)
				msg += ", RequestingAssembly: " + args.RequestingAssembly.FullName;
			msg += "\r\n" + new StackTrace(true);

			LogStartupMessage(msg);

			return null;

			// For cases where .Net in looking for a class in an assembly and can't find it then
			// code like the following may resolve the issue.
			//return typeof(ProblemClass).Assembly;
		}

		/// <summary>
		/// Print a list of loaded assemblies
		/// </summary>

		static void PrintLoadedAssemblies()
		{
			if (!LogStartupDetails) return;

			AppDomain domain = AppDomain.CurrentDomain;
			LogStartupMessage("Loaded assemblies:");
			foreach (Assembly a in domain.GetAssemblies())
			{
				LogStartupMessage("   " + a.FullName);
			}
			return;
		}

		/// <summary>
		/// Prompt user for E-mail address if we don't have it
		/// </summary>

		public static string GetEmailAddress()
		{
			string email = Preferences.Get("EmailAddress");
			if (email != null && email != "") return email;

			if (!SS.I.Attended) return "";
			if (SS.I.StandAlone) return "";

			email = InputBoxMx.Show("Please enter your E-mail address", "E-mail Address");
			if (Response.IsCancel(email)) return "";
			UserObjectDao.SetUserParameter(SS.I.UserName, "EmailAddress", email);
			Preferences.Set("EmailAddress", email);

			return email;
		}

		/// <summary>
		/// Get current key connections message and start background job to check key connections & update message if time to do so
		/// </summary>
		/// <returns></returns>

		static string GetKeyOracleConnectionsMessage()
		{
			int updateIntervalInMinutes = 3; // interval to start background process to check key connections & store message
			string msg = "";

			if (!SS.I.Attended) return ""; // don't get if unattended session

			bool getMessage = // check parm to see if we should check & update the message
				SS.I.UserIniFile.ReadBool("GetKeyOracleConnectionsMessage", true);
			if (!getMessage) return "";

			bool update = false;
			string parm = UserObjectDao.GetUserParameter("Mobius", "KeyOracleConnectionsMessage", ""); // time of last update (set to future value to turn off for a while)
			if (Lex.IsNullOrEmpty(parm)) update = true;
			else
				try
				{
					int i1 = parm.IndexOf(",");
					DateTime dt = DateTime.Parse(parm.Substring(0, i1));
					TimeSpan timeDiff = DateTime.Now.Subtract(dt);
					if (timeDiff.TotalMinutes >= updateIntervalInMinutes) update = true; // update if sufficient time since last update
					msg = parm.Substring(i1 + 1);
				}
				catch (Exception ex) { update = true; }

			if (update)
				CommandLine.StartBackgroundSession("Update Key Connections Message");

			return msg;
		}

		/// <summary>
		/// Some fatal on on session startup, show to user if interactive or throw exception if service
		/// </summary>
		/// <param name="errmsg"></param>

		static void FatalStartupError(
			string errmsg)
		{
			if (SS.I.Attended)
				MessageBoxMx.ShowError(errmsg);

			else throw new Exception(errmsg);
		}

/// <summary>
/// Update MobiusClientStart as needed
/// </summary>

		void UpdateMobiusClientStartAsNeeded()
		{
			int t0;
			string dllPath = "";

			try
			{
				t0 = TimeOfDay.Milliseconds();
				if (LogStartupDetails)
					LogStartupMessage("Checking for current version of MobiusClientStart.");

				string localMobiusClientStartPath = MobiusClientUtil.GetMobiusBaseDirectoryFilePath("MobiusClientStart.exe");

				string remoteBinDir = SS.I.IniFile.Read("ServerBinDir"); // bin directory on server where things are
				string remoteMobiusClientStartPath = remoteBinDir + @"\MobiusClientStart.exe";

				if (!File.Exists(localMobiusClientStartPath) || // do date specific check (could check for newer version but would require check on server)
					DateTime.Compare(File.GetLastWriteTime(localMobiusClientStartPath), new DateTime(2016, 2, 24)) < 0) // copy if local copy is older than specified date
				{
					ClientLog.Message("Copying file " + remoteMobiusClientStartPath + " to " + localMobiusClientStartPath);
					System.IO.File.Copy(remoteMobiusClientStartPath, localMobiusClientStartPath, true);

				}

				if (LogStartupDetails)
					LogStartupMessage("MobiusClientStart check/update time (ms): " + (TimeOfDay.Milliseconds() - t0).ToString());
			}
			catch (Exception ex)
			{
				ClientLog.Message("Error updating MobiusClientStart: " + ex.Message);
			}

			return;
		}

		/// <summary>
		/// Register an assembly
		/// Corresponds to command: 
		///  C:\Windows\Microsoft.NET\Framework\v2.0.50727\regasm.exe /codebase "...\Mobius\bin\SomeAssembly.dll"
		/// </summary>
		/// <param name="dllPath"></param>

		public static void RegisterAssembly(string dllPath)
		{
			Assembly asm = Assembly.LoadFile(dllPath);
			RegistrationServices reg = new RegistrationServices();
			bool success = reg.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase);
			if (!success) throw new Exception("No classes registered in assembly: " + dllPath);
			return;
		}

/// <summary>
/// Init for StandAlone mode
/// </summary>

		void InitializeForStandAloneMode()
		{
			Query q = null;

			UalUtil.NoDatabaseAccessIsAvailable = true;

			//q = QueriesControl.NewQuery(); // add a single new query so something shows

			//Instance.MainContentsControl.ShowNormal();

			if (Math.Abs(1) == 2) // open specific serialized query 
			{
				StreamReader sr = new StreamReader(@"...\mobius\M3-TwoCmpdsThreeTables.xml");
				//			StreamReader sr = new StreamReader(@"...\mobius\M3-100Structures.xml");
				//			StreamReader sr = new StreamReader(@"...\mobius\M3-SingleCmpndTargetResults.xml");
				string queryXml = sr.ReadToEnd();
				sr.Close();
				q = Query.Deserialize(queryXml);
				QueriesControl.SetCurrentQueryInstance(q);
				QueryTablesControl.Render(q); // set & render the query
			}

			return;
		}

		/// <summary>
		/// Log an instance of a UI command being issued
		/// </summary>
		/// <param name="commandName"></param>

		public static void LogCommandUsage(string commandName)
		{
			int count;

			if (SS.I.CommandUsageStatistics.ContainsKey(commandName))
				count = SS.I.CommandUsageStatistics[commandName];
			else count = 0;
			SS.I.CommandUsageStatistics[commandName] = count + 1;

			return;
		}

/// <summary>
/// Get currently active form or default to shell if no active form
/// </summary>

		public static Form ActiveForm
		{
			get
			{
				if (Form.ActiveForm is Progress) return Instance?.ShellForm;
				else if (Form.ActiveForm.GetType() == typeof(Progress)) return Instance?.ShellForm;

				if (Form.ActiveForm != null) return Form.ActiveForm;
				else return null;
			}
		}

/// <summary>
/// Shutdown the session
/// </summary>

		public bool ShutDown()
		{
			string txt;

			if (InShutDown) return true;

			try
			{
				InShutDown = true;

				SaveCurrentWindowSet();

				// Close all files allowing for cancel

				if (!QbUtil.CloseFileAll()) // close all files allowing for cancel
				{
					InShutDown = false;
					return false;
				}

				// Save shell size & location

				if (DebugMx.False) WriteSaveShellSizeAndLocationDebugInfo(); // debug

				if (Lex.IsDefined(Instance.ShellFormSize)) // save size and location of main shell window
					SS.I.UserIniFile.Write("ShellFormPlacement", Instance.ShellFormSize);

				if (Instance.QueriesControl != null) // also save size of right panel
					SS.I.UserIniFile.Write("ShellFormSplitterPosition", Instance.QueriesControl.QueryBuilderControl.QbSplitter.SplitterPosition.ToString());

				if (Instance.RibbonCtl != null)
					SS.I.UserIniFile.Write("RibbonQuickAccessToolbarLocation", Instance.RibbonCtl.ToolbarLocation.ToString());

				// Log command usage statistics

				txt = "";
				if (SS.I.CommandUsageStatistics != null) // log table usage stats
				{
					foreach (KeyValuePair<string, int> kvp in SS.I.CommandUsageStatistics)
						txt += (string)kvp.Key + "\t" + kvp.Value.ToString() + "\n";
					UsageDao.LogEvent("CommandStats", txt);
				}

				// Log table usage statistics

				txt = "";
				if (SS.I.TableUsageStatistics != null) // log table usage stats
				{
					foreach (KeyValuePair<string, int> kvp in SS.I.TableUsageStatistics)
						txt += (string)kvp.Key + "\t" + kvp.Value.ToString() + "\n";
					UsageDao.LogEvent("TableStats", txt);
				}

				QueryEngine.LogQueryExecutionStatistics();

				UsageDao.LogEvent("End");

				if (!Lex.IsNullOrEmpty(ScriptLog.FileName))
					ScriptLog.Message("> Exiting session");

				// Finish cleanup & exit

				SF.ServiceFacade.DisposeSession(); // shut down the services session -- this should be redundant with the Application exit handler but does no harm

				TempFile.PurgeTempFiles();
			}

			catch (Exception ex)
			{
				ClientLog.Message(DebugLog.FormatExceptionMessage(ex));
			}

			InShutDown = false;

			try
			{
				Environment.Exit(0);
			}

			catch (Exception ex)
			{
				try
				{
					Process.GetCurrentProcess().Kill();  // maybe this will kill it
				}

				catch (Exception ex2)
				{
					return false; // can't kill it for some reason
				}
			}

			return true;
		}

		/// <summary>
		/// Save set of currently open queries for use in next session
		/// </summary>

		public static string SaveCurrentWindowSet()
		{
			string cws = ""; // current window set

			if (SS.I.ConnectedToServices && SS.I.RestoreWindowsAtStartup && QueriesControl.Instance != null)
			{
				Document[] mwa = QueriesControl.Instance.DocumentList.ToArray();
				foreach (Document mw_i in mwa) // get list of open windows
				{
					Query q = (Query)mw_i.Content;
					if (q.UserObject.Content != "" && q.UserObject.Id > 0)
						cws += mw_i.Type.ToString() + "\t" +
								"QUERY_" + q.UserObject.Id + "\n";
				}

				UserObjectDao.SetUserParameter(SS.I.UserName, "CurrentWindowSet", cws);
			}

			return cws;
		}

void WriteSaveShellSizeAndLocationDebugInfo() // debug info
		{
			try
			{
				if (Instance == null)	ClientLog.Message("ShellFormPlacement - Instance is null");

				else if (Lex.IsDefined(Instance.ShellFormSize))	ClientLog.Message("ShellFormPlacement=" + Instance.ShellFormSize);

				else if (Instance.ShellForm == null) ClientLog.Message("ShellFormPlacement - Instance.ShellForm is null");
				else
				{
					Form form = Instance.ShellForm;

					string state =
						((int)form.WindowState).ToString() + "," +
						form.Left.ToString() + "," +
						form.Top.ToString() + "," +
						form.Width.ToString() + "," +
						form.Height.ToString();

					ClientLog.Message("ShellFormPlacementOld=" + state);
				}

				if (SS.I.IniFile == null) ClientLog.Message("SS.I.IniFile is null");
				if (SS.I.UserIniFile == null) ClientLog.Message("SS.I.UserIniFile is null");
			}
			catch (Exception ex)
			{
				ClientLog.Message(DebugLog.FormatExceptionMessage(ex));
			}
		}

		static void TestPrint()
		{
#if false
			SetupPrint sp = new SetupPrint(); // Mobius class
			sp.ShowDialog();

			PrintDocument printDoc = new PrintDocument();

			PageSettings pgs = new PageSettings();
			PrinterSettings ps = new PrinterSettings();

			PageSetupDialog pageDlg = new PageSetupDialog();
			pageDlg.Document = printDoc;
			pageDlg.ShowDialog();

			PrintDialog printDlg = new PrintDialog();
			printDlg.Document = printDoc;
			printDlg.ShowDialog();

			PrintPreviewDialog previewDlg = new PrintPreviewDialog();
			previewDlg.Document = printDoc;
			previewDlg.ShowDialog();
#endif
		}

		static void TestConditionalFormatting()
		{
#if false
			CondFormatForm f = new CondFormatForm();
			CondFormat cf = new CondFormat();
			cf.ColumnType = MetaColumnType.Structure;
			string s = cf.Serialize();
			f.Deserialize(s);
			f.ShowDialog();
#endif
		}

		static void TestSpotfireInteraction()
		{
#if false
					Spotfire.Application app = null;

			try
			{
				try // see if there is an existing instance we can connect to
				{
//					object o = Microsoft.VisualBasic.Interaction.GetObject(null,"Spotfire.Application"); 
					object o = System.Runtime.InteropServices.Marshal.GetActiveObject("Spotfire.Application");
					app = (Spotfire.Application)o;
				}
				catch (Exception ex) { app = null; } // fall through & start new instance

				if (app == null) // try to start new instance
					app = new Spotfire.Application();

				bool b = app.Visible;
			}
			catch (Exception e)
			{
				XtraMessageBox.Show("Error starting Spotfire");
			}

			if (Math.Sqrt(4) == 2) return;


#endif
		}

#if false
		static void TestSharePointFileSaveDialog()
		{
			ClientState.IniFile = // app specific settings
				IniFile.FindAndOpen("MobiusClient.ini", "Mobius");

			ClientState.UserIniFile = // user specific settings
				IniFile.FindAndOpen("MobiusClientUser.ini", "Mobius");

			string uri;
			string filter = "Microsoft Word Files (*.doc)|*.doc";
			DialogResult dr = SharePointFileSaveDialog.ShowDialog("Save Word File to SharePoint", "", filter, ".doc", null, out uri);
			return;
		}
#endif

		static void TestAnnotationForm()
		{
#if false
				UserDataForm af = new UserDataForm(); 
				af.ShowDialog();
#endif
		}


		static void TestAQBForm()
		{
#if false

			CriteriaComplex aqb = new CriteriaComplex(); 
			aqb.ShowDialog();
#endif
		}

		static void TestAlertsForm()
		{
#if false
			AlertGrid a = new AlertGrid(); 
			a.ShowDialog();
#endif
		}

		static void TestSplitForm()
		{
#if false
			QnfSplitForm sf = new QnfSplitForm(); 
			sf.ShowDialog();
#endif
		}

/// <summary>
/// Create integration point for passing in commands from other apps
/// </summary>
/// <param name="ipName"></param>

		private static void CreateIntegrationPoint(string ipName)
		{
			if (ipName == null)
			{
				ipName = "Mobius_" + Process.GetCurrentProcess().Id;
			}

			SS.I.IpcChan = new IpcServerChannel("Mobius");
			ChannelServices.RegisterChannel(SS.I.IpcChan, false);

			RemotingConfiguration.RegisterWellKnownServiceType(
					typeof(Mobius.Client.MobiusClientIntegrationPoint),
					ipName, WellKnownObjectMode.Singleton);

			if (LogStartupDetails)
				LogStartupMessage("Registered IpcChannel as ipc://Mobius/" + ipName + " .");
		}

/// <summary>
/// Attempt to activate the shell and make it visisble
/// </summary>

		public static void ActivateShell()
		{
			if (SessionManager.Instance.ShellForm != null) // make us visible
				try
				{
					SessionManager.Instance.ShellForm.Activate();
					SessionManager.Instance.ShellForm.BringToFront();
					SessionManager.Instance.ShellForm.Focus();
				}

				catch { };
		}

		/// <summary>
		/// Set the title of the shell title bar
		/// </summary>
		/// <param name="docName"></param>

		public static void SetShellTitle(
			string docName)
		{
			//if (Lex.Eq(docName, "Query1")) docName = "debug"; // debug

			string appName = UmlautMobius.String; // Mobius with umlaut

			if (ClientState.IsAdministrator()) // is admin show context info
			{
				string clientVersion, servicesVersion, server;
				ServiceFacade.ServiceFacade.GetClientAndServicesVersions(out clientVersion, out servicesVersion, out server);
				appName += " Client: " +  clientVersion + ", Services: " + servicesVersion;
				if (Lex.IsDefined(server)) appName += " on " + server;
			}

			if (Instance != null && Instance.ShellForm != null)
			{
				if (Instance.RibbonCtl != null)
				{
					Instance.RibbonCtl.ApplicationCaption = appName;
					Instance.RibbonCtl.ApplicationDocumentCaption = docName;
				}

				else Instance.ShellForm.Text = appName + " - " + docName;
			}

			//DebugLog.Message("SetShellTitle " + docName + ", " + appName + "\r\n" + new StackTrace(true).ToString());

			return;
		}

		/// <summary>
		/// When the shell is first shown activate any quick search control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Shell_Shown(object sender, EventArgs e)
		{
			//ActivateQuickSearchControl(); // (activate later after Splash is closed, not here)
		}

/// <summary>
/// Activate any quick search control
/// </summary>

		internal void ActivateQuickSearchControl()
		{
			if (QuickSearchControl != null)
			{
				Application.DoEvents();
				QuickSearchControl.Links[0].Focus();
			}
		}

		/// <summary>
		/// When the commandline editor is shown add an event handler to process user input on a per-key basis
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CommandLine_ShownEditor(object sender, ItemClickEventArgs e)
		{
			CommandLineControl = RibbonCtl.Manager.ActiveEditor as MRUEdit;
			if (CommandLineControl == null) return;

			CommandLineControl.Properties.ImmediatePopup = false; // avoid immediate popup that overlays structure
//			CommandLineControl.EnterMoveNextControl = true; // causes enter/tab key to save text to MRU list

			QuickSearchPopup qsp = QuickSearchPopup.Initialize(CommandLineControl, ShellForm, MainContentsTree);
			CommandLineControl.Focus();
			return;
		}

		/// <summary>
		/// Display current count for current query
		/// </summary>
		/// <param name="msg"></param>

		public static void DisplayCurrentCount()
		{
			if (Instance != null && Instance.StatusBarManager != null)
				Instance.StatusBarManager.DisplayCurrentCount();

			DisplayStatusMessage(""); // hide any status message
			return;
		}

/// <summary>
/// Display filter counts and string for current query
/// </summary>

		public static void DisplayFilterCountsAndString()
		{
			if (Instance != null && Instance.StatusBarManager != null)
				Instance.StatusBarManager.DisplayFilterCountsAndString();
		}

		/// <summary>
		/// LockResultsKeys
		/// </summary>

		public static bool LockResultsKeys 
		{
			set
			{
				if (Instance != null && Instance.StatusBarManager != null)
					Instance.StatusBarManager.LockResultsKeys = value;
			}
		}

		/// <summary>
		/// Display status message for main instance
		/// </summary>
		/// <param name="msg"></param>

		public static void DisplayStatusMessage(string msg)
		{
			if (Instance != null && Instance.StatusBarManager != null)
				Instance.StatusBarManager.DisplayStatusMessage(msg);
		}

/// <summary>
/// Reset all filters to retrieve all data
/// </summary>

		public void ResetFilters()
		{
			QueryManager qm = QueryResultsControl.CrvQm;
			if (qm == null || qm.DataTableManager == null) return;

			qm.DataTableManager.ResetFilters(); // reset filters from data table
			qm.DataTableManager.UpdateFilterState(); // update the row-level filter flags
			qm.QueryResultsControl.ResetFilters();
			StatusBarManager.DisplayFilterCountsAndString();

			if (qm.MoleculeGrid != null) qm.MoleculeGrid.Refresh();
		}

/// <summary>
/// Change the filters enabled state & update display
/// </summary>
/// <param name="enable"></param>

		public void EnableFilters(bool enable)
		{
			QueryManager qm = QueryResultsControl.CrvQm;
			if (qm == null || qm.DataTableManager == null) return;

			DataTableManager dtm = qm.DataTableManager;

			dtm.FiltersEnabled = enable; // set filter enablement for data table
			if (enable && !dtm.FiltersApplied) dtm.ApplyFilters(); // need to reapply filters if DataTable has been modified or rebuilt
			dtm.UpdateFilterState(); // update counts

			if (qm.QueryResultsControl != null)
				qm.QueryResultsControl.EnableFilters(enable);

			StatusBarManager.DisplayFilterCounts(); // show updated counts
			//QueryResultsControl.GetQrcFromForm(this).UpdateFilteredViews(); // (done in qrc.EnableFilters)
		}

/// <summary>
/// Clear all filters & redisplay data
/// </summary>

		public void ClearFilters ()
		{
			QueryManager qm = QueryResultsControl.CrvQm;
			if (qm == null || qm.DataTableManager == null) return;

			qm.DataTableManager.ClearFilters(); // clear filters from data table
			qm.DataTableManager.UpdateFilterState(); // update the row-level filter flags
			qm.QueryResultsControl.ClearFilters(); // clear filters from grid
			StatusBarManager.DisplayFilterCountsAndString();

			if (qm.MoleculeGrid != null)
			{
				qm.MoleculeGrid.SetHeaderGlyphs();
				qm.MoleculeGrid.Refresh();
			}
		}

		/// <summary>
		/// Display a startup message on appropriate output device
		/// </summary>

		public void DisplayStartupMessage(
			string msg)
		{
			msg = UmlautMobius.String + " - " + msg;
			if (SS.I.Attended && Splash != null)
			{
				Splash.Text = msg;
				Splash.BringToFront();
				Application.DoEvents();
			}

			ClientLog.Message(msg);
			return;
		}

/// <summary>
/// Execute a command
/// </summary>
/// <param name="command"></param>

		public string ExecuteCommand(
			string command)
		{
			string response = CommandExec.ExecuteCommand(command);
			return response;
		}

		/// <summary>
		///  CheckForCorrectSoftwareVersions
		/// </summary>
		/// <returns></returns>

		public static bool CheckForCorrectSoftwareVersions()
		{

			if (!CheckDotNetFrameworkVersion()) return false;

			return true;
		}

		/// <summary>
		/// Check for proper version of .Net libraries
		/// </summary>

		public static bool CheckDotNetFrameworkVersion()
		{

			bool dotNetV4Installed = // See if .Net V4 Framework is installed & log message if not
				Mobius.ComOps.SystemUtil.IsDotNetVersionInstalled(@"v4\Client", 0);

			if (dotNetV4Installed) return true;

			string msg = ".Net Version 4 is not installed on this machine.";
			ClientLog.Message(msg);

			msg += "\r\n\r\nMobius is not able to continue.";
			DialogResult dr = MessageBox.Show(msg, ".Net Version 4 is not installed", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}


		/// <summary>
		/// Dispose of the service session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		static void DisposeServiceSession(object sender, EventArgs e)
		{
			try
			{
				SF.ServiceFacade.DisposeSession();
				ClientLog.Message("DisposeServiceSession");
			}
			catch (Exception ex)
			{
				ClientLog.Message(DebugLog.FormatExceptionMessage(ex)); // just log
			}
		}

/// <summary>
/// Become other user for debug purposes
/// </summary>

		bool CheckForDebugModeUserName()
		{
			string initialName = SS.I.UserName;

			// Commonly used test accounts are given below.
			// Note that you can also start the Mobius client under a particular user with the command:
			//  "...\Mobius\Bin\MobiusClient.exe" username = RX81237

			//SS.I.UserName = "<TestAcountName>"; SS.I.UserDomainName = "<TestDomainName>"; 

			//SS.I.UserName = "Bogus"; // Invalid userId

			return (SS.I.UserName != initialName); // return true if name changed
		}

/// <summary>
/// Log startup time message 
/// </summary>
/// <param name="msg"></param>
/// <param name="t0"></param>
/// <returns></returns>

		static int LogStartupTimeMessage(
			string msg,
			int t0)
		{
			int t1 = t0;
			LogStartupMessage(msg + " " + MSTime.FormatDelta(ref t1));
			return t1; // return current time
		}

		static void LogStartupMessage(
			string msg)
		{
			ClientLog.Message(msg);
		}

	} // SessionManager

}
