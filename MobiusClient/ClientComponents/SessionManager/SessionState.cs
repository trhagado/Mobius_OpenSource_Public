using Mobius.ComOps;
using Mobius.Data;
using Mobius.MolLib1;
using Mobius.ServiceFacade;

using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Ipc;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// SessionState instance reference for this session 
	/// </summary>

	public class SS
	{
		public static SessionState I = new SessionState(); // singleton for SessionState
	}

	/// <summary>
	/// Global client session state data
	/// </summary>
	 
	public class SessionState
	{
		public bool RemoteServiceMode { get { return ServiceFacade.ServiceFacade.UseRemoteServices; } }
		public bool LocalServiceMode { get { return !ServiceFacade.ServiceFacade.UseRemoteServices; } }
		public bool StandAlone = false; // set to true if running in standalone (no services/database access) mode

		public bool ConnectedToServices = false; // set to true when connected to services
		public int SessionNumber; // sequential number of this session

		//		public Socket Socket; // socket to use for session 
		public Thread Thread; // managed thread we are running on
		public int PhysicalThreadId = 0; // OS thread we are running on

		public string CurrentDate; // current date in yyyymmdd format

		public const int NORMAL_LISTENER_PORT = 4010; // port for server to listen on
		public int ListenerPort = NORMAL_LISTENER_PORT;

		public IpcServerChannel IpcChan; // for app-level integration

		public string IniFilePath = ""; // name of ini file for app
		public IniFile IniFile; // .ini file for app
		public IniFile UserIniFile; // .ini file for user
		public IniFile ServicesIniFile { get { return Mobius.ComOps.ServicesIniFile.IniFile; }} // .ini file of service parameters

		public string UserName = ""; // session user name
		public string UserDomainName = ""; // session user domain name
		public bool IsDeveloper { get { return UserInfo.Privileges.HasPrivilege("Developer"); } }
		public bool IsAdmin { get { return UserInfo.Privileges.HasPrivilege("Administrator"); } }
		public string NewUserName = ""; // Used when switching to new user

		public UserInfo UserInfo // User info and privs. Stored in ClientState to allow access from all assemblies
		{
			get { return ClientState.UserInfo; }
			set { ClientState.UserInfo = value; }
		}

		public string EmailAddress // session user email address
		{
			get
			{
				if (emailAddress == null)
					emailAddress = Security.GetUserEmailAddress(UserName);
				return emailAddress;
			}
		}
		string emailAddress = null;

		public bool ShowWaitCursor = true; // if true show hourglass between input requests

		public bool DebugClient = false; // if true turn on logging of extra client debugging info
		public string [] DebugFlags = new string[7]; // flags used for debugging

// List parameters

		public bool ValidateCompoundIds = true; // if true check number validity
		public bool AllowDuplicates = false; // if true allow dup numbers in lists

		public List<TempCidList> TempCidLists // Current and other temporary session lists
		{
			get
			{
				if (_tempCidLists == null) // create if not done yet
				{
					_tempCidLists = new List<TempCidList>(); // Current and other temporary session lists
					TempCidList curList = new TempCidList();
					curList.Name = "Current";
					_tempCidLists.Add(curList);
				}

				return _tempCidLists;
			}
		}
		List<TempCidList> _tempCidLists;

		public string DatabaseSubsetListName = ""; // name of database subset list
		public int DatabaseSubsetListSize = -1; // size of database subset list

// Structure parameters

		public HydrogenDisplayMode HydrogenDisplay = HydrogenDisplayMode.Hetero; // Option for H display	
		public int DisplayAtomNumbers = 0; // control display of atom numbers in structures
		public int StructureFontSize = 90; // standard structure font size in decipoints
		public bool ShowStereoComments = false; // include any stereo comments with structure display
		public bool SaltGroupingEnabled = true; // global flag for salt grouping enabled
		public bool EnhanceStructureDisplay = true; // do other structure display enhancements
		public bool ScaleBondWidth = true; // scale down bond width for small structs

// QueryBuilder parameters

		public List<HistoryItem> History = new List<HistoryItem>(); // query history for session
		//public bool ShowCriteriaTab = true; // if true display criteria in first tab
		//public bool ShowMqlNames = false; // if true include MQL names in menu
		//public bool ShowHiddenFields = false; // if true include hidden fields
		public bool BreakHtmlPopupsAtPageWidth = true; // orientation of HTML popup windows
		public bool FindRelatedCpdsInQuickSearch = true; // execute a search for related compounds when typing in the QuickSearch line
		public bool RestoreWindowsAtStartup = false; // if true load queries open in last session at startup
		public OutputDest DefaultQueryDest = OutputDest.WinForms; // output dest for runquery command
		public bool CheckCnUniqueness = true; // remove duplicate compound number data

		public bool ScriptCancel = false; // cancel of running script pending
		public int ScriptLevel = -1; // depth of script files
		public List<StreamReader> ScriptStreams = new List<StreamReader>(); // stream associated with each level of the script
		public Dictionary<string, string> ScriptParameters = new Dictionary<string, string>(); // parameters to be substituted in scripts

		public bool QueryTestMode = false; // if true in special query test mod
		public bool UseDistinctInSearch = true; // if distinct key should be used in queries
		public bool FindDeactivatedActCodes = false;

// QueryFormatter parameters

		public bool RepeatReport = false; // repeat report multiple times across grid if possible
		public bool DuplicateKeyValues = false; // if true default for duplication of key fields
		public int RowFraming = -1; // undefined<0, True > 0 or False = 0
		public string NullValueString = ""; // string used to display missing values
		//public bool DefaultToSingleStepQueryExecution = false; // true to default to single step search/retrieval

		public int ResultsPageScale=100; 
		public int ResultsFontSize=9; 
		public string ResultsFontName = "Arial";
		public bool UseColumnNameLabels = false; // if true use column names instead of labels
		public bool OutputEmptyTables = false;
		public bool AutoPage = false; // automatically page ahead
		public bool ShowConditionalFormatting = true; // if true display any conditional formatting for report (todo: move to Rf)
		public ColumnFormatEnum DefaultNumberFormat = ColumnFormatEnum.SigDigits;
		public int DefaultDecimals = 3;
		public QnfEnum QualifiedNumberSplit = QnfEnum.Combined; // splitting of qualified numbers on export
			//QnfEnum.Split | QnfEnum.NumericValue | QnfEnum.Qualifier | QnfEnum.TextValue; // debug

		public bool HilightCidChanges = true; // highlight Corp Cids / submission Ids with changed Cids
		public bool RemoveLeadingZerosFromCids = true; // leading zero removal on compound ids

		//public MetaTable KeyMetaTable = null;
		//public MetaColumn KeyMetaColumn = null;
		//public MetaColumn KeyMolStructureColumn = null;
		//public MetaColumn KeyMolSimilarityColumn = null;
		//public MetaColumn KeyMolFormulaColumn = null;
		//public MetaColumn KeyMolNameColumn = null;

		public int UnitsOfMeasure = UNITS_INCH;
		public const int UNITS_INCH = 1;
		public const int UNITS_CM =   2;
		public const int UNITS_PTS =  3; // points

// Grid parameters

		public bool GridShowCheckBoxes = true; // allow selections using check boxes
		public bool GridMarkCheckBoxesInitially = false; // if true mark check boxes as selected initially
		public int  GridMaxBufferedRows = 100; // max number of rows to prebuffer to client
		public Color EvenRowBackgroundColor = Color.White;
		public Color OddRowBackgroundColor = Color.White;
		//		public QueryColumn [] SortMultipleCols; // current set of columns selected for sorting

// HTML parameters

		public int HtmlScreensPerPage = 1; // physical screen pages per html page
		public int HtmlPageWidth = 0; // set to a nonzero value to force an HTML page width

// MS Word parameters

		public string WordFontName = "Arial"; // expected Word font
		public int WordFontSize = 10; // expected  Word font size
		public int WordPageWidth = 8500; 
		public int WordPageHeight = 11000;
		public PageMargins WordPageMargins = new PageMargins(1000,1000,1000,1000); // page margins (unscaled)

// Printer parameters

		public int PrintPageWidth = 8500; // unscaled page width in milliinches
		public int PrintPageHeight = 11000; // unscaled page height in milliinches
//		public Orientation PrintOrientation = Orientation.Vertical; 
//		public PageMargins PrintPageMargins = new PageMargins(500,500,500,500); // page margins (unscaled)
		public int PrintPageScale = 100; // scale of object coords to screen coords (percent)
		public int PrintFontSize = 10; 
		public string PrintFont = "Arial";
		public int PrintRepeatCount = -1; 
		public bool PrintHeaders = true;

// Client parameters

		public bool Attended = true; // running interactively with real user

		public string ClientOSVersion = ""; // Client OS version
		public string ClientMachineName = ""; // Client machine name
		public Version MobiusClientVersion // client version
		{ get { return ClientState.MobiusClientVersion; }
			set { ClientState.MobiusClientVersion = value; }
		}
			
		public Version MobiusServerVersion; // server version

		public int GridScaleAdjustment = 80; // final scaling of view to get proper size
		public int HtmlScaleAdjustment = 80;

		public string PreferredProjectId = ""; // if of preferred project
		public int TableColumnZoom = 100; // default zoom of grid columns
		public int GraphicsColumnZoom = 100; // extra zoom of graphical grid columns added to TableColumnZoom
		public bool ScrollGridByPixel = false; // if true scroll grid by pixel rather than row (Disabled: many performance issues with this feature)
		public bool AsyncImageRetrieval = true; // async or synchronous image retrieval
		public string LookAndFeel = ""; // look and feel for Mobius interface

		public int UISetupLevel = -1; // level for UI setup

		public Dictionary<string, int> CommandUsageStatistics = new Dictionary<string, int>(); // number of times each command accessed
		public Dictionary<string, int> TableUsageStatistics = new Dictionary<string, int>(); // number of times each table accessed

// Parameters retrieved from the server

		public bool ShowMetaTableStats = true; // show stats in metatree if true

// Misc parameters

		public int LoadChunkSize = 1; // chunk size for database loading
		public PopupHtml BrowserPopup; // most-recently shown browser popup form
		public bool AllowAdminFullObjectAccess = false; // if true admins can modify any object
		public bool AllowGroupingBySalts = false; // if true use salt sibling information
		public bool UsePersonNameInPlaceOfUserName = true; // to identify user objects
		public bool ShowAddNewViewTab = false; // multiple view capability enabled
		public bool UseTibcoSpotfire = false; // use Tibco Spotfire (DXP)
		public bool UseSpotfireDecisionSite // use Spotfire DecisionSite (obsolete)
		{
			get { return !UseTibcoSpotfire; }
			set { UseTibcoSpotfire = !value; }
		}

	}

	/// <summary>
	/// History of queries executed during session
	/// </summary>

	public class HistoryItem
	{
		public string QueryName;
		public string QueryFileName;
		public int ListCount;
		public string ListFileName;
		public DateTime DateTime;
	}

/// <summary>
/// Temporary (session) list
/// </summary>

	public class TempCidList
	{
		public string Name = "";
		public int Id = 0; // temp user object id
		public int Count = 0; // number of compound ids
		public DateTime CreationTime = DateTime.Now;

		public TempCidList()
		{
			return;
		}
	}


}
