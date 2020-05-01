using Mobius.ComOps;
using Mobius.Data;
using Mobius.Data.DataEx;
using Mobius.MolLib2;
using Mobius.CdkMx;
using Mobius.SpotfireClient;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Management;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using Mobius.ClientComponents.Dialogs;
using DevExpress.XtraSplashScreen;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Command delegates
	/// </summary>
	/// <returns></returns>

	public delegate string CM0(); // command method with no parameters
	public delegate string CM1(string args); // command method with args parameter

	/// <summary>
	/// Commandline operations
	/// </summary>

	public partial class CommandLine
	{
		static Dictionary<string, Command> DispatchTable; // command dispatch table
		static List<Command> CommandList; // ordered list of commands

		static int ClipBoardQuery = 0; // index of current clipboard query
		static string CommandLineString; // current command
		static string LastSql = "";


		/// <summary>
		/// Build dictionary of commands & associated methods in initialization
		/// Note that for proper operation a command in the dispath table cannot
		/// be a prefix of another command.
		/// </summary>
		/// 
		/// <summary>
		/// Build Oracle view to metatable data
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		static void InitializeDispatchTable()
		{
			DispatchTable = new Dictionary<string, Command>();
			CommandList = new List<Command>();

			// Admin commands

			//AC("Create User", new CM1(SecurityUtil.CreateUser), 2, true); // obsolete
			//AC("Update User", new CM1(SecurityUtil.UpdateUser), 2, false); // obsolete
			//AC("Disable User", new CM1(SecurityUtil.DisableUser), 2,76 false); // obsolete 
			//AC("Delete User", new CM1(SecurityUtil.DeleteUser), 2, false); // obsolete

			AC("Check Mobile Queries", new CM0(GetStandardMobileQueries));

			AC("Check User", new CM1(CheckUser));
			AC("CopyPrdAnnotationTableDataToDev", new CM0(CallServerCommand), 0);

			AC("Show User", new CM1(CheckUser));

			// Commands that only appear in the developer menu

			AC("Analyze Queries", new CM1(AnalyzeQueries), 1, true);
			AC("Analyze Usage Data", new CM1(AnalyzeUsageData));
			AC("Associate Schema", new CM1(AssociateSchema));
			AC("Base64 Decode", new CM1(B64Decode), 0);
			AC("Base64 Encode", new CM1(B64Encode), 0);
			AC("Build MetaTable From Database Catalog", new CM1(BuildMetaTableXmlFromDatabaseCatalog), 1, false, false);
			//AC("Build MetaTable Oracle View", new CM0(CallServerCommand));

			AC("Call", new CM1(CallClassMethod), 0);

			AC("Change Owner", new CM1(ChangeOwner), 1, true);

			AC("Check Alert", new CM1(AlertUtil.CheckSingle));
			AC("Check Alerts", new CM1(AlertUtil.CheckAll));
			//AC("Check Alerts Old", new CM1(AlertUtil.CheckAllOld));
			AC("Check Import Files", new CM0(UserData.CheckForCurrentUserImportFileUpdates));
			AC("Check All Import Files", new CM0(UserData.CheckForAllUsersImportFileUpdates));
			//TODO delete AC("Test Retrieving AD Groups", new CM0(UserData.TestRetrievingADGroups)); 
			AC("Check Queued Alerts", new CM1(AlertUtil.CheckQueuedAlerts));

			AC("Debug Test", new CM1(DebugTest), 0);
			AC("Delete Alert", new CM1(AlertUtil.DeleteAlert), 0);
			AC("Delete Inactive Alerts", new CM1(AlertUtil.DeleteInactiveAlerts), 0);
			AC("Delete Annotation Table Data", new CM1(UserData.DeleteAnnotationTableData), 0);
			AC("Delete Annotation Table Orphans", new CM0(DeleteAnnotationTableOrphans), 0);
			AC("Delete Inactive Temp Files", new CM0(DeleteInactiveTempFiles), 0);

			AC("DemoView", new CM1(DemoView));

			AC("Export To MST", new CM1(ExportToMST), 1, true);
			AC("Find Invalid Metatables", new CM0(FindInvalidMetatables));

			AC("Fix Cache Files", new CM0(FixCacheFiles), 0);
			AC("Fix Annotation Tables", new CM0(FixAnnotationTables), 0);
			AC("Fix QueryObject Permissions", new CM0(FixQueryObjectPermissions), 0);
			AC("Fix Log File", new CM0(FixLogFile), 0);
			AC("Fix Queries", new CM0(FixQueries), 0);
			AC("Fix Deleted Alerts", new CM0(FixDeletedAlerts), 0);

			AC("GcClient", new CM0(WindowsHelper.GarbageCollect), 0);
			AC("GcServices", new CM1(ExecuteServiceCommand), 0);

			AC("Get", new CM1(GetStaticStringMemberValue));
			AC("Grant", new CM1(SecurityUtil.GrantPrivilege));
			AC("Import User Data", new CM1(UserData.ImportFile));
			AC("Kill Session", new CM1(KillSession));

			AC("Load MetaData", new CM1(LoadMetadata), 1, true);
			AC("Load MetaTables", new CM1(LoadMetaTables));
			AC("Load MetaTree", new CM1(LoadMetaTree));

			AC("Open DataSet", new CM1(OpenDataSet), 0);
			AC("OpenQueryFromClipboard", new CM0(OpenQueryFromClipboard));
			AC("Open Query", new CM1(OpenQuery));

			AC("QueryTest", new CM1(QueryTest.Run), 0);
			AC("RegisterAssembly", new CM1(RegisterAssembly), 0);
			AC("ResetTableAliases", new CM0(ResetTableAliases), 0);
			AC("Revoke", new CM1(SecurityUtil.RevokePrivilege));
			AC("Retrieve Background Export", new CM1(BackgroundQuery.RetrieveBackgroundExport));

			AC("Run Background Export", new CM1(BackgroundQuery.RunBackgroundExport));
			AC("Run Query In Background", new CM1(BackgroundQuery.RunBackgroundQuery));
			AC("Run Query", new CM1(RunQuery));
			AC("Run QueryAsPopup", new CM0(RunQueryAsPopup));

			AC("Run Script", new CM1(RunScript));
			AC("Run Test Queries", new CM1(QueryTest.Run));

			AC("Save CurrentWindowSet", new CM0(SessionManager.SaveCurrentWindowSet));
			AC("Save DataSet", new CM1(SaveDataSet), 0);
			AC("Save Spotfire SQL", new CM0(SpotfireLinkUI.StoreSpotfireQueryTableSql), 1, false, false);
			AC("SCmd", new CM1(ExecuteServiceCommandWithProgress), 0); // service command
			AC("Select", new CM1(Select));
			AC("Set", new CM1(SetCommand));
			AC("Send Email Notification", new CM1(Email.SendNotification));

			AC("Show Alerts", new CM1(AlertGridDialog.Show));
			AC("Show ApprovedMobiusUsers", new CM0(ShowApprovedMobiusUsers));
			AC("Show ApprovedMobiusChemView", new CM0(ShowApprovedMobiusChemView));
			AC("Show ApprovedMobiusSequenceView", new CM0(ShowApprovedMobiusSequenceView));
			AC("Show ApprovedMobiusNoChemView", new CM0(ShowApprovedMobiusNoChemView));
			AC("Show BitmapTest", new CM0(ShowBitmapTest));
			AC("Show ASSAY Status", new CM0(CallServerCommand));
			AC("Show Connections", new CM0(ShowOracleConnections));
			AC("Show MetaTable Xml", new CM1(ShowMetaTableXml));
			AC("Show Mql", new CM1(ShowMql));
			AC("Show Mql2Query", new CM0(ShowMql2Query));
			AC("Show OracleClientVersion", new CM0(CallServerCommand));
			AC("Show Process", new CM0(ShowProcess));
			AC("Show Query Xml", new CM0(ShowQueryXml));
			AC("Show Qe Stats", new CM0(ShowQeStats));
			AC("Show Last Boot Up Time", new CM0(ShowLastBootUpTime));
			AC("Show Session", new CM0(ShowProcess));
			AC("Show Sessions", new CM1(ShowSessions));
			AC("Show SmallWorld DbList", new CM1(ShowSmallWorldDbList));
			AC("Show SpecialFolders", new CM0(ShowSpecialFolders));
			AC("Show Sql", new CM0(ShowQuerySqlStatements));
			AC("Show User Databases", new CM1(UserDatabasesExplorer.Explore));
			AC("Show GenericMetaBrokerVars", new CM0(ShowGenericMetaBrokerVars));

			AC("Sort Tree Node", new CM1(SortTreeNode));
			AC("Spawn", new CM1(SpawnScript));
			AC("Spotfire API Dev/Test Utility", new CM0(MobiusSpotfireDevTestUtility));
			AC("StartBackgroundSession", new CM0(CallServerCommand));
			AC("StartServerProcess", new CM0(CallServerCommand));
			AC("Structure Conversion", new CM0(TestStructureConversion));

			AC("Test Connections", new CM0(TestOracleConnections));
			AC("Test Experimental Code", new CM0(CallServerCommand));
			AC("Test Key Connections", new CM0(TestKeyOracleConnections));
			AC("Test Key Connections Message", new CM0(TestKeyOracleConnectionsMessage));

			AC("Test Oracle Connections", new CM0(TestOracleConnections));
			AC("Test Services Log", new CM0(TestServicesLog));
			AC("Test Server Load", new CM1(LoadTestServer));
			AC("Test SimSearchMx", new CM0(CallServerCommand));
			AC("Test SQL", new CM0(TestSql));

			AC("Test WebPlayer", new CM1(SpotfireViewManager.TestWebPlayer));

			AC("Update AnnotationTableDataRowOrder", new CM0(CallServerCommand));
			AC("Update AssayAttributesTable", new CM0(CallServerCommand));
			AC("Update CorpIdChangeLists", new CM0(CallServerCommand));
			AC("Update DbLinks", new CM1(UpdateDatabaseLinks));
			AC("Update Key Connections Message", new CM0(UpdateKeyOracleConnectionsMessage));
			AC("Update CorpDbMoltableMx", new CM0(CallServerCommand));
			AC("Update FingerprintDatabaseMx", new CM0(CallServerCommand));
			AC("Update MetaTable Statistics", new CM1(UpdateMetaTableStatistics));
			AC("Update MetaTree Cache", new CM0(UpdateMetaTreeCache));
			AC("Update MetatreeNodeTable", new CM0(CallServerCommand));

			AC("Update TnsNames", new CM0(UpdateTnsNames));
			//AC("Update Related Structures", new CM1(UpdateRelatedStructs.UpdateRelatedStructures));
			AC("Update Target Assay Attributes Dictionary File From Database", new CM0(UpdateTargetAssayAttributesDictionaryFileFromDatabase));
			//AC("Update UCDB Model Results", new CM1(UserData.UpdateModelResults), 0);

			AC("Usage", new CM1(AnalyzeUsageData));
			AC("View Alert Data", new CM1(AlertUtil.ViewAlertData));
			AC("View Background Query Results", new CM1(AlertUtil.ViewBackgroundQueryResults));

			AC("Who Am I", new CM0(WhoAmI), 2, true);
		}

		/// <summary>
		/// Add command to hash table of normalized commands
		/// </summary>
		/// <param name="commandName"></param>
		/// <param name="commandMethod"></param>

		public static void AC(
				string name,
				object method)
		{
			AddCommand(name, method, 1, false, true); // just visible to devs by default
		}

		public static void AC(
			string name,
			object method,
			int visibilityLevel)
		{
			AddCommand(name, method, visibilityLevel, false, true);
		}

		public static void AC(
			string name,
			object method,
			int visibilityLevel,
			bool isStartOfSection)
		{
			AddCommand(name, method, visibilityLevel, isStartOfSection, true);
		}

		public static void AC(
			string name,
			object method,
			int visibilityLevel,
			bool isStartOfSection,
			bool allowAddToSubmenu)
		{
			AddCommand(name, method, visibilityLevel, isStartOfSection, allowAddToSubmenu);
		}

		public static void AddCommand(
				string name,
				object method,
				int visibilityLevel,
				bool isStartOfSection,
				bool allowAddToSubmenu)
		{
			Command c = new Command();
			c.Name = name;
			c.Method = method;
			c.VisibilityLevel = visibilityLevel;
			c.IsStartOfSection = isStartOfSection;
			c.AllowAddToSubmenu = allowAddToSubmenu;

			string key = name.Replace(" ", "").ToLower();
			DispatchTable[key] = c;
			CommandList.Add(c);
			return;
		}

		/// <summary>
		/// Build dropdown menu of commandline items
		/// </summary>
		/// <param name="dropDownItems"></param>

		public static void BuildDropDownMenu(ToolStripItemCollection dropDownItems)
		{
			if (dropDownItems.Count > 1) return; // just return if already built

			dropDownItems.Clear();
			ToolStripMenuItem lastSubmenu = null;

			if (!SS.I.IsAdmin && !SS.I.IsDeveloper) return;

			if (CommandList == null)
				InitializeDispatchTable();

			for (int ci = 0; ci < CommandList.Count; ci++)
			{
				Command c = CommandList[ci];
				if (c.VisibilityLevel >= 2 && SS.I.IsAdmin) { }
				else if (c.VisibilityLevel >= 1 && SS.I.IsDeveloper) { } // only show if admin command or user is developer
				else continue;

				if (c.IsStartOfSection && dropDownItems.Count > 0)
					dropDownItems.Add(new ToolStripSeparator());

				ToolStripMenuItem mi = new ToolStripMenuItem();
				mi.Tag = c.Name; // store full command in tag
				mi.Click += CommandMenuItem_Click;

				if (!c.AllowAddToSubmenu) lastSubmenu = null;

				string tok1 = CmdTok1(c.Name);
				if (lastSubmenu != null && Lex.Eq(lastSubmenu.Text, tok1))
				{ // add to existing submenu
					mi.Text = CmdRest(c.Name);
					lastSubmenu.DropDownItems.Add(mi);
				}

				else if (c.AllowAddToSubmenu && ci + 1 < CommandList.Count && Lex.Eq(CmdTok1(CommandList[ci + 1].Name), tok1))
				{ // start new submenu
					lastSubmenu = new ToolStripMenuItem();
					lastSubmenu.Text = tok1;
					dropDownItems.Add(lastSubmenu);

					mi.Text = CmdRest(c.Name);
					lastSubmenu.DropDownItems.Add(mi);
				}

				else // single full-name item
				{
					mi.Text = c.Name;
					dropDownItems.Add(mi);
				}
			}
		}

		static string CmdTok1(string cmd)
		{
			string tok1, rest;
			CmdParse(cmd, out tok1, out rest);
			return tok1;
		}

		static string CmdRest(string cmd)
		{
			string tok1, rest;
			CmdParse(cmd, out tok1, out rest);
			return rest;
		}

		static void CmdParse(string cmd, out string tok1, out string rest)
		{
			tok1 = "";
			rest = "";
			int i1 = cmd.IndexOf(" ");
			if (i1 > 0)
			{
				tok1 = cmd.Substring(0, i1);
				rest = cmd.Substring(i1 + 1);
			}

			else tok1 = cmd; // single token
		}

		static void CommandMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			string response = Execute(mi.Tag as string);
			if (!Lex.IsNullOrEmpty(response))
				MessageBoxMx.Show(response);
			return;
		}

		public void ToolsMenuPluginItem_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			string command = mi.Text;
			string msg = Execute(command);
		}

		/// <summary>
		/// Call method to process a command line command
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns>Command response or null if not a command</returns>

		public static string Execute(string commandLine)
		{
			string args, response;
			int ci, i1;

			if (DispatchTable == null) InitializeDispatchTable();

			if (Lex.IsNullOrEmpty(commandLine)) return null;

			if (commandLine.ToLower().StartsWith("command ") || // remove any leading "command"
					commandLine.ToLower().StartsWith("commandline "))
			{
				i1 = commandLine.IndexOf(" ");
				if (i1 >= 0 && i1 + 1 < commandLine.Length)
					commandLine = commandLine.Substring(i1 + 1);
			}

			if (Lex.IsNullOrEmpty(commandLine)) return null;

			CommandLineString = commandLine;

			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string command = "";

			while (true)
			{ // catenate command tokens until we get a match
				string tok = lex.GetLower();
				if (tok == "") break;
				command += tok;
				if (DispatchTable.ContainsKey(command))
				{
					int pos = lex.Position; // get any remaining arguments
					if (pos >= commandLine.Length) args = "";
					else args = commandLine.Substring(pos).Trim();

					object cmo;
					try
					{
						cmo = DispatchTable[command].Method;
						if (cmo is CM0) response = ((CM0)cmo)();
						else if (cmo is CM1) response = ((CM1)cmo)(args);
						else return "";

						if (response == null) response = ""; // indicate that command was processed
						return response;
					}

					catch (Exception ex)
					{
						return "Exception for command: " + commandLine + "\n" +
							DebugLog.FormatExceptionMessage(ex);
					}
				}
			}

			// No luck, see if command is a plugin name

			lex.OpenString(commandLine);
			command = lex.GetLower();
			Plugin p = Plugins.GetPluginByName(command);
			if (p != null)
			{
				args = commandLine.Substring(command.Length).Trim();
				try
				{
					response = Plugins.CallExtensionPointRunMethod(p.Id, args);
					if (response == null) response = ""; // indicate that command was processed
					return response;
				}
				catch (Exception ex)
				{
					if (ex.InnerException != null) ex = ex.InnerException;
					response = "Exception in Mobius plugin run method for extension id: " + p.Id + "\r\n" + DebugLog.FormatExceptionMessage(ex);
					ServicesLog.Message(response);
					return response;
				}
			}

			return null; // not a command
		}

		/// <summary>
		/// Execute a command on the server side
		/// </summary>
		/// <returns></returns>

		public static string CallServerCommand()
		{
			Progress.Show("Executing Command...");
			string result = ExecuteServiceCommand(CommandLineString);
			Progress.Hide();
			return result;
		}


		/// <summary>
		/// Read all queries and output each column selected or filtered
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		static string AnalyzeQueries(string command)
		{
			Progress.Show("Analyzing Queries");
			string result = ExecuteServiceCommand("AnalyzeQueries " + command);
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Analyse usage data over a specified period
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		static string AnalyzeUsageData(string command)
		{
			Progress.Show("Analyzing Usage Data...", UmlautMobius.String, false);
			Thread.Sleep(100);
			Application.DoEvents();

			try
			{
				string report = UsageDao.AnalyzeUsageData(command);
				string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\UsageData.txt";
				if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return "";
				StreamWriter sw = new StreamWriter(cFile);
				sw.Write(report);
				sw.Close();

				SystemUtil.StartProcess(cFile); // show it
				Progress.Hide();
				return "Analysis of usage data complete";
			}

			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		/// <summary>
		/// Save any current data set and set to view the dataset immediately when the the query is opened
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		static string SaveDataSet(
		 string fileName)
		{
			if (Lex.IsNullOrEmpty(fileName)) return "Must supply file name";
			Query q = QueriesControl.BaseQuery;
			if (q == null || q.QueryManager == null) return "Not saved";
			QueryManager qm = q.QueryManager as QueryManager;
			if (qm.DataTableManager == null) return "Not saved";

			bool ssr = q.SerializeResultsWithQuery;
			q.SerializeResultsWithQuery = true; // include results

			bool sbpr = q.BrowseSavedResultsUponOpen;
			q.BrowseSavedResultsUponOpen = true; // browse when opened

			q.SerializeToFile(fileName, true);

			q.SerializeResultsWithQuery = ssr;
			q.BrowseSavedResultsUponOpen = sbpr;
			return "Saved";
		}

		/// <summary>
		/// Save any current data set
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		static string OpenDataSet(
		 string fileName)
		{
			if (Lex.IsNullOrEmpty(fileName)) return "Must supply file name";
			if (!File.Exists(fileName)) return "File does not exist";

			QbUtil.SetMode(QueryMode.Build); // be sure in build mode
			Query q = Query.DeserializeFromFile(fileName);
			QbUtil.AddQueryAndRender(q, true);
			return "";
		}

		/// <summary>
		/// Process a Mobius Query Language (Mql) select statement
		/// Example: select * from corp_structure where corp_cid = 12345
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		static string Select(
				string command)
		{
			Query q = null;
			command = "select " + command.Trim();
			try
			{
				q = MqlUtil.ConvertMqlToQuery(command);
			}
			catch (Exception ex) { return ex.Message; }

			QbUtil.NewQuery("Select Statement"); // show in query builder
			QbUtil.SetCurrentQueryInstance(q);
			QbUtil.RenderQuery(0);

			if (q.ComplexCriteria == "" && q.Tables.Count == 1) // if no criteria & one table do as preview
			{
				Query q2 = q.Clone();
				q2.Preview = true;
				q2.ClearSorting(); // no sorting on preview
				q = q2;
			}

			string nextCommand = QueryExec.RunQuery(q, OutputDest.WinForms);
			return nextCommand;
		}

		/// <summary>
		/// Display current user id
		/// </summary>
		/// <returns></returns>

		public static string WhoAmI()
		{
			string txt = "";
			if (SS.I.UserInfo != null && SS.I.UserInfo.LastName != null && SS.I.UserInfo.LastName != "")
				txt = SS.I.UserInfo.FirstName + " " + SS.I.UserInfo.LastName + "\n";
			if (SS.I.UserDomainName != "") txt += SS.I.UserDomainName + @"\";
			txt += SS.I.UserName;
			return txt;
		}

		private static void ReloadContentsTree()
		{
			Progress.InvokeShow("Rebuilding and reloading metatree...\n(Please be patient -- This may take up to a minute.)", ComOps.UmlautMobius.String, false);
			MetaTree.ReloadMetaTree();
			Progress.InvokeShow("Refilling the Contents Tree...", ComOps.UmlautMobius.String, false);
			SessionManager.Instance.MainContentsControl.InvokeShowNormal();
			Progress.InvokeHide();
		}

		/// <summary>
		/// Update the cached TargetAssayDictionary file from the database
		/// </summary>
		/// <returns></returns>

		static string UpdateTargetAssayAttributesDictionaryFileFromDatabase()
		{
			Progress.Show("Updating Target Assay Attributes Dictionary File From Database");
			string result = ExecuteServiceCommand("UpdateTargetAssayAttributesDictionaryFileFromDatabase");
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Update row count and most-recent update data for each metatable in tree
		/// </summary>
		/// <returns></returns>

		static string UpdateMetaTableStatistics(
				string brokerName)
		{
			Progress.Show("Updating metatable statistics...", UmlautMobius.String, false);
			int count = MetaTableFactory.UpdateStats(brokerName);
			Progress.Hide();
			return "Updated metatable statistics for " + count + " metatables";
		}

		public static string UpdateMetaTreeCache()
		{
			Progress.Show("Updating MetaTree cache file...");
			string result = ExecuteServiceCommand("UpdateMetaTreeCache");
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Reload metadata possibly from another source
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		private static string LoadMetadata(string dir)
		{
			Progress.Show("Loading Metadata");

			MetaTableCollection.Reset(); // clear the old
			MetaTree.Reset();
			SessionManager.Instance.MainContentsTree.TreeList.ClearNodes();

			string result = ExecuteServiceCommand("LoadMetadata");
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Reload MetaTree 
		/// Note that the file can't refer to subfiles
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		static string LoadMetaTree(string fileName)
		{
			fileName = Lex.RemoveAllQuotes(fileName);

			if (Lex.IsUndefined(fileName))
				fileName = UIMisc.GetOpenFilename("Load MetaTree", "", ".xml");

			if (Lex.IsUndefined(fileName)) return "";

			if (!File.Exists(fileName)) return "File not found: " + fileName;

			if (MetaTree.MetaTreeFactory == null)
				MetaTree.MetaTreeFactory = new MetaTreeFactory(); // factory to call to build metatree

			Progress.Show("Building new metatree...", "Mobius", false);
			MetaTree.Reset();
			MetaTree.GetMetaTree(fileName); // build the full tree

			UserObjectTree.Reset();
			UserObjectTree.RetrieveAndBuildSubTree(SS.I.PreferredProjectId);

			Progress.Hide();

			SessionManager.Instance.MainContentsControl.ShowNormal();
			return "Metatree reloaded";
		}

		/// <summary>
		/// Reload metatable definitions
		/// Note that the file can't refer to subfiles
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		static string LoadMetaTables(
						string fileName)
		{
			fileName = Lex.RemoveAllQuotes(fileName);
			if (!File.Exists(fileName)) return "File not found: " + fileName;
			Progress.Show("Building metatables...", "Mobius", false);
			MetaTableCollection.MetaTableFactory.BuildFromFile(fileName);
			Progress.Hide();
			return "Metatables reloaded";
		}

		/// <summary>
		/// Delete Inactive Temp Files
		/// </summary>
		/// <returns></returns>

		static string DeleteInactiveTempFiles()
		{
			Progress.Show("Deleting inactive temp files...");
			string result = ExecuteServiceCommand("Delete Inactive Temp Files");
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Delete Annotation Table Orphans
		/// </summary>
		/// <returns></returns>

		static string DeleteAnnotationTableOrphans()
		{
			throw new NotImplementedException();
#if false
			int deleteCount = 0;
			DbCommandMx drd = new DbCommandMx();
			string sql =
			"delete /*+rule*/ " +
			"from mbs_owner.mbs_adw_rslt r " +
			"where not exists " +
			" (select * " +
			"  from mbs_owner.mbs_usr_obj o " +
			"  where o.obj_id = r.mthd_vrsn_id) " +
			"and rownum <= 10000 ";

			drd.PrepareQuery(sql);
			drd.BeginTransaction();
			while (true)
			{
				int count = drd.ExecuteNonReader();
				if (count == 0) break;
				deleteCount += count;
				drd.Commit();
				Progress.Show("Deleted " + deleteCount.ToString());
			}

			drd.Commit();
			drd.Dispose();
			Progress.Hide();
			return "Deleted " + deleteCount.ToString() + " orphan annotation rows";
#endif
		}

		/// <summary>
		/// Set internal program variable
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string SetCommand(string args)
		{
			string tok, tok2;
			int i1;

			try
			{
				Lex lex = new Lex();
				lex.OpenString(args);
				string itemName = lex.Get(); // item to set

				if (itemName.Contains(".")) // if qualified name, assume class.member name
				{
					SetClassStaticMemberValue(args);
				}

				else if (Lex.Eq(itemName, "Attended"))
					SS.I.Attended = GetBoolSetting(lex);

				else if (Lex.Eq(itemName, "ChunkSize"))
				{
					tok = lex.Get();
					SS.I.LoadChunkSize = Int32.Parse(tok);
					return "ChunkSize is now " + tok;
				}

				else if (Lex.Eq(itemName, "ConsoleLogFile"))
				{ // turn script logging on or off
					tok = lex.Get();
					if (tok == "" || Lex.Eq(tok, "Off") || Lex.Eq(tok, "False"))
					{
						ScriptLog.FileName = "";
						return "Console logging is now off";
					}

					ScriptLog.FileName = Lex.RemoveAllQuotes(tok).Trim();

					tok = lex.Get();
					if (!Lex.Eq(tok, "Append")) // erase unless append mode
						try { File.Delete(ScriptLog.FileName); }
						catch (Exception ex) { }

					return "Logging console to " + Lex.Dq(ScriptLog.FileName);
				}

				else if (Lex.StartsWith(args, "DatabaseView ") || Lex.StartsWith(args, "Database View "))
				{
					i1 = Lex.IndexOf(args, "View ");
					string viewName = args.Substring(i1 + 4).Trim();

					string result = ExecuteServiceCommand("SET " + args); // try to execute on service side

					UserInfo ui = UserInfo.Deserialize(result);
					ClientState.UserInfo.RestrictedViewAllowedMetaTables = ui.RestrictedViewAllowedMetaTables;
					//ClientState.UserInfo.RestrictedViewCorpIds = ui.RestrictedViewCorpIds; // not passed, don't set
					//ClientState.UserInfo.RestrictedViewUsers = ui.RestrictedViewUsers; // not passed, don't set
					if (Lex.Eq(viewName, "AllData"))
						return "Now using unrestricted view of all data";

					else return "Now using restricted database view: " + viewName.ToUpper();
				}

				else if (Lex.Eq(itemName, "LogWindow"))
				{ // turn log window on or off
					tok = lex.Get();
					if (tok == "" || Lex.Eq(tok, "Off") || Lex.Eq(tok, "False"))
					{
						LogWindow.Display = false;
						if (LogWindow.Instance != null) LogWindow.Instance.Hide();
					}

					else
					{
						LogWindow.Display = true;
						if (LogWindow.Instance != null) LogWindow.Instance.Show();
					}

					return "";
				}

				else if (Lex.Eq(itemName, "DebugClient"))
					SS.I.DebugClient = GetBoolSetting(lex);

				else if (Lex.Eq(itemName, "FindDeactivatedActCodes"))
					SS.I.FindDeactivatedActCodes = GetBoolSetting(lex);

				//else if (Lex.Eq(parm, "LogSqlPrepares"))
				//  UalUtil.Set("LogSqlPrepares", GetBoolSetting(lex));

				//else if (Lex.Eq(parm, "LogLongReadLimit"))
				//  UalUtil..Set("LogLongReadLimit", Int32.Parse(tok = lex.Get()));

				else if (Lex.Eq(itemName, "ShowNews")) // set automatic display of new news
					NewsDialog.SetShowNewsEnabled(GetBoolSetting(lex));

				else if (Lex.Eq(itemName, "UseDistinctInSearch") ||
						Lex.Eq(itemName, "UseDistinct"))
					SS.I.UseDistinctInSearch = GetBoolSetting(lex);

				else if (Lex.Eq(itemName, "AllowNetezzaUse") || Lex.Eq(itemName, "UseNetezza") || Lex.Eq(itemName, "Netezza"))
					QueryEngine.SetParameter("AllowNetezzaUse", GetBoolSetting(lex).ToString());

				else if (Lex.Eq(itemName, "AllowAdminFullObjectAccess")) // special admin access to other's user objects
					SS.I.AllowAdminFullObjectAccess = GetBoolSetting(lex);

				else if (Lex.Eq(itemName, "User"))
					return SetUser(lex.Get());

				else if (Lex.Eq(itemName, "SmallSsqMaxAtoms"))
					QbUtil.Query.SmallSsqMaxAtoms = Int32.Parse(tok = lex.Get());

				else if (Lex.Eq(itemName, "ImpreciseSsq"))
				{
					bool imprecise = GetBoolSetting(lex);
					if (imprecise) QbUtil.Query.SmallSsqMaxAtoms = 1000;
					else QbUtil.Query.SmallSsqMaxAtoms = 1;
				}

				else if (Lex.Eq(itemName, "AllowMultiTablePivot"))
					QueryEngine.SetParameter("AllowMultiTablePivot", GetBoolSetting(lex).ToString());

				else if (Lex.Eq(itemName, "FilterAllDataQueriesByDatabaseContents"))
				{
					throw new NotImplementedException();
				}

				else if (Lex.Eq(itemName, "Parm") || Lex.Eq(itemName, "Parmeter"))
					return SetScriptParameter(args);

				else if (Lex.Eq(itemName, "DebugFlag"))
				{
					int flagIndex = Int32.Parse(lex.Get());
					SS.I.DebugFlags[flagIndex] = lex.Get();
				}

				else if (Lex.Eq(itemName, "EnhanceStructureDisplay"))
					SS.I.EnhanceStructureDisplay = GetBoolSetting(lex);

				else if (Lex.Eq(itemName, "ShowAddNewViewTab"))
				{
					SS.I.ShowAddNewViewTab = GetBoolSetting(lex);
					Preferences.Set("ShowAddNewViewTab", SS.I.ShowAddNewViewTab);
				}

				else if (Lex.Eq(itemName, "UseTibcoSpotfire"))
				{
					SS.I.UseTibcoSpotfire = GetBoolSetting(lex);
					Preferences.Set("UseTibcoSpotfire", SS.I.UseTibcoSpotfire);
				}

				else if (Lex.Eq(itemName, "PauseRetrieval"))
					DataTableManager.PauseRowRetrievalEnabled = GetBoolSetting(lex);

				else if (Lex.Eq(itemName, "ScrollGridByPixel"))
				{
					SS.I.ScrollGridByPixel = GetBoolSetting(lex);
					Preferences.Set("ScrollGridByPixel", SS.I.ScrollGridByPixel);
				}

				else if (Lex.Eq(itemName, "AsyncImageRetrieval"))
				{
					SS.I.AsyncImageRetrieval = GetBoolSetting(lex);
					Preferences.Set("AsyncImageRetrieval", SS.I.AsyncImageRetrieval);
				}

				else return ExecuteServiceCommand("SET " + args); // try to execute on service side

				//else return "Invalid parameter: " + parm;
			}
			catch (Exception ex) { return "Set command failed: Set " + args; }

			return "The parameter value has been set";
		}

		/// <summary>
		/// Get a boolean setting value
		/// </summary>
		/// <returns></returns>

		static bool GetBoolSetting(
						Lex lex)
		{
			string tok = lex.Get().ToLower();
			bool setValue = false;
			if (tok == "on" || tok == "yes" || tok == "true") setValue = true;
			return setValue;
		}

		static string MobiusSpotfireDevTestUtility()
		{
			TestMobiusSpotfireApiForm form = new TestMobiusSpotfireApiForm();
			DialogResult dr = form.ShowDialog(Form.ActiveForm);
			return "";
			//Application.Run(new BrowserAndScriptTestForm());
			//Application.Run(new Mobius.Spotfire.ComAutomation.PE_TestForm());
		}
		/// <summary>
		/// Test writing to the services log
		/// </summary>
		/// <returns></returns>

		static string TestServicesLog()
		{
			ServicesLog.Message("TestServicesLog Message");
			return "\"TestServicesLog Message\" send to services log";
		}

		/// <summary>
		/// Test Oracle connections
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		static string TestOracleConnections()
		{
			Progress.Show("Testing connections...", UmlautMobius.String, false);
			string txt = DbConnection.TestConnections();
			Progress.Hide();
			return txt;
		}

		/// <summary>
		/// Test just the key connections
		/// </summary>
		/// <returns></returns>

		static string TestKeyOracleConnections()
		{
			Progress.Show("Testing key connections...", UmlautMobius.String, false);
			string result = ExecuteServiceCommand("Test Key Connections");
			Progress.Hide();
			return result;
		}

		static string TestKeyOracleConnectionsMessage()
		{
			Progress.Show("Testing time of last message...", UmlautMobius.String, false);
			string result = ExecuteServiceCommand("Test Key Connections Message");
			Progress.Hide();
			return result;
		}


		static string TestStructureConversion()
		{
			MoleculeViewer.ShowMolecule(new MoleculeMx());
			return "";
		}

		static string UpdateKeyOracleConnectionsMessage()
		{
			Progress.Show("Updating key Oracle connections message...", UmlautMobius.String, false);
			string result = ExecuteServiceCommand("Update Key Connections Message");
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Show any held Oracle connections
		/// </summary>
		/// <returns></returns>

		static string ShowOracleConnections()
		{
			string s = DbConnection.GetOracleConnections();

			if (String.IsNullOrEmpty(s)) return "No connections";

			string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\OpenConnections.txt";
			if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return "";
			StreamWriter sw = new StreamWriter(cFile);
			sw.Write(s);
			sw.Close();

			SystemUtil.StartProcess(cFile); // show it
			return "";
		}

		/// <summary>
		/// Prepare, Execute and Read the rows for a Sql statement logging performance
		/// </summary>
		/// <returns></returns>

		static string TestSql()
		{
			string sql = InputBoxLarge.Show("SQL", "Enter SQL:", LastSql, -1, -1);
			if (Lex.IsUndefined(sql)) return "";

			LastSql = sql;

			Progress.Show("Executing Command...");
			string result = ExecuteServiceCommand("TestSql " + sql);
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Update the Mobius server side TnsNames file from the corporate area
		/// </summary>
		/// <returns></returns>

		static string UpdateTnsNames()
		{
			Progress.Show("Updating TNSNames.ora...");
			string result = ExecuteServiceCommand("UpdateTnsNames");
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Update the database links 
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		static string UpdateDatabaseLinks(
		 string commandLine)
		{
			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string tok = lex.Get();
			Progress.Show("Updating Database Links...", UmlautMobius.String, false);
			string txt = DbConnection.UpdateDatabaseLinks(tok);
			Progress.Hide();
			return txt;
		}

		/// <summary>
		/// Define/redefine a data source
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		static string DefineDataSource(
		 string args)
		{
			return ServiceFacade.DbConnection.DefineDataSource(args);
#if false
			Lex lex = new Lex();
			lex.OpenString(args);

			DataSource ds = new DataSource();

			ds.Name = lex.GetUpper();
			string asTok = lex.Get();
			ds.OracleName = lex.GetUpper();
			ds.UserName = lex.GetUpper();
			ds.Password = lex.Get();
			ds.InitCommand = lex.Get();

			if (Lex.Ne(asTok,"As") ||
				ds.OracleName == "" ||
				ds.UserName == "" ||
				ds.Password == "")
				return "Syntax: Define Data Source data-source-name As oracle-instance-id userId password";

			DataSource oldDs = null;
			if (DataSource.DataSources.ContainsKey(ds.Name))
				oldDs = DataSource.DataSources[ds.Name];

			DataSource.DataSources[ds.Name] = ds;
			try // make sure we can connect to it
			{
				SessionConnection mxConn = SessionConnection.Get(ds.Name);
				mxConn.Close();
			}
			catch (Exception ex)
			{
				if (oldDs != null) DataSource.DataSources[oldDs.Name] = oldDs;
				DataSource.DataSources.Remove(ds.Name);
				return "Error connecting to new data source: " + ex.Message;
			}

			return "Data source definition complete";
#endif
		}

		/// <summary>
		/// Associate a schema with a particular data source
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		static string AssociateSchema(
		 string args)
		{
			return ServiceFacade.DbConnection.AssociateSchema(args);
#if false
			string dsName;
			Lex lex = new Lex();
			lex.OpenString(args);

			string schemaName = lex.GetUpper();
			while (true)
			{
				dsName = lex.GetUpper();
				if (Lex.Eq(dsName, "With") ||
				Lex.Eq(dsName, "Data") ||
				Lex.Eq(dsName, "Source"))
					continue;
				else break;
			}

			if (dsName == "")
				return "Syntax: Associate Schema schema-name With Data Source data-source-name";

			if (!DataSource.DataSources.ContainsKey(dsName))
				return "Data source " + dsName + " is not defined";

			Schema s = new Schema();
			s.Name = schemaName;
			s.DataSourceName = dsName;
			DataSource.Schemas[schemaName] = s;

			return "Association complete";
#endif
		}

		/// <summary>
		/// Start another session under a new userName if a Mobius admin
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		static string SetUser(
						string userName)
		{
			if (Lex.IsUndefined(userName)) return "The new UserName must be supplied in the command line";

			if (!Security.IsAdministrator(SS.I.UserName))
				return "You must be a Mobius administrator to switch to another user";

			UserInfo newUserInfo = null;
			if ((newUserInfo = Security.ReadUserInfo(userName)) == null)
				return userName + " is not a valid user";

			string fileName = ClientDirs.ExecutablePath;
			string args = "UserName = " + userName;
			Process p = System.Diagnostics.Process.Start(fileName, args);

			return "A new Mobius session has been started for user: " + userName + " (" + newUserInfo.FullName + ")";
		}

		/// <summary>
		/// Get list of invalid metatables in tree
		/// </summary>
		/// <returns></returns>

		static string FindInvalidMetatables()
		{
			Hashtable checkedMt = new Hashtable();
			Hashtable badMt = new Hashtable();

			int t1 = 0;
			int checkCount = 0;

			MetaTreeNode mtn = MetaTree.GetNode("root");
			if (mtn == null) return "Can't find root";

			Progress.Show("Checking metatables for validity...", UmlautMobius.String, false);

			FindInvalidMetatables(mtn, checkedMt, badMt, ref t1);

			int defaultCols = 0;
			int selectableCols = 0;
			int totalCols = 0;

			foreach (string mtName in MetaTableCollection.GetContentsTables().Keys)
			{
				MetaTable mt = MetaTableCollection.Get(mtName);
				if (mt == null) continue;

				foreach (MetaColumn mc in mt.MetaColumns)
				{
					if (mc.InitialSelection == ColumnSelectionEnum.Selected)
					{
						defaultCols++;
						selectableCols++;
					}

					else if (mc.InitialSelection == ColumnSelectionEnum.Unselected)
						selectableCols++;

					totalCols++;
				}
			}
			Progress.Hide();

			if (t1 < 0) return ""; // cancelled

			ArrayList badList = new ArrayList(badMt.Keys);
			badList.Sort();

			string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\InvalidMetafiles.txt";
			if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return "";
			StreamWriter sw = new StreamWriter(cFile);
			sw.WriteLine("Invalid Metatables");
			sw.WriteLine("");

			foreach (string s in badList)
				sw.WriteLine(s);

			sw.Close();

			SystemUtil.StartProcess(cFile); // show it

			Progress.Hide();
			return badMt.Count.ToString() + " / " +
							checkedMt.Count.ToString() + " invalid metatables found,\n" +
							"Total cols = " + totalCols.ToString() +
							", selectable = " + totalCols.ToString() +
							", default = " + defaultCols.ToString();
		}

		/// <summary>
		/// Recursively check nodes for valid metatables
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="checkedMt"></param>
		/// <param name="badMt"></param>
		/// <param name="t1"></param>

		static void FindInvalidMetatables(
						MetaTreeNode parent,
						Hashtable checkedMt,
						Hashtable badMt,
						ref int t1)
		{
			foreach (MetaTreeNode mtn in parent.Nodes)
			{
				if (mtn.IsFolderType)
				{
					FindInvalidMetatables(mtn, checkedMt, badMt, ref t1); // go recursive
					if (t1 < 0) return; // cancelled
				}

				else if (mtn.Type == MetaTreeNodeType.MetaTable ||
								mtn.Type == MetaTreeNodeType.Annotation ||
								mtn.Type == MetaTreeNodeType.CalcField)
				{
					checkedMt[mtn.Target] = null;
					if (MetaTableCollection.Get(mtn.Target) == null)
					{
						badMt[mtn.Target] = mtn.Label;
					}
					//					if (badMt.Count >= 10) break;
					if (TimeOfDay.Milliseconds() - t1 < 1000) continue;

					Progress.Show("Finding Invalid Metatables " + badMt.Count.ToString() + " / " +
									checkedMt.Count.ToString() + "	...", UmlautMobius.String);

					t1 = TimeOfDay.Milliseconds();

					if (Progress.CancelRequested)
					{
						Progress.Hide();
						t1 = -1; // indicate cancel
						return;
					}

				}
			}
		}

		/// <summary>
		/// Export to Molecular Selection Tool
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string ExportToMST(
				string args)
		{
			List<string> keys = null;

			string html = BackgroundQuery.ReadTemplateFile("MolecularSelectionTool.htm");
			string title = "Export Data to the Molecular Selection Toolkit (MST)";
			int width = 650;
			int height = 400;

			int dr = MessageBoxMx.ShowWithCustomButtons(html, title, "OK", "Cancel", "", "", MessageBoxIcon.Information, width, height);
			if (dr == 2) return ""; // cancel button

			if (QbUtil.Query == null || QbUtil.Query.Tables.Count == 0)
			{
				MessageBoxMx.ShowError("No current query.");
				return "";
			}

			Query q = QbUtil.Query.Clone(); // original query this analysis is based on
			ExportParms ep = new ExportParms();

			ep.OutputDestination = OutputDest.TextFile;
			ep.ExportFileFormat = ExportFileFormat.Tsv;

			string mstDir = ServicesIniFile.Read("MstWindowsDir");
			if (!mstDir.EndsWith(@"\")) mstDir += @"\";
			string fileName = @"Mobius_" + SS.I.UserName + "_" + TimeOfDay.Milliseconds() + ".txt"; // build unique name
			ep.OutputFileName = mstDir + fileName;

			ep.QualifiedNumberSplit = QnfEnum.Split | QnfEnum.Qualifier | QnfEnum.NumericValue;
			ep.IncludeDataTypes = false;
			ep.DuplicateKeyValues = false;
			ep.RunInBackground = false;
			ep.ExportStructureFormat = ExportStructureFormat.Smiles; // correct?

			Progress.Show("Retrieving data...");
			try
			{
				QueryExec qexec = new QueryExec();
				string result = qexec.RunQuery(q, ep);
				if (Lex.Eq(result, "Cancelled")) return "";
			}

			catch (Exception ex)
			{
				MessageBoxMx.ShowError("Error executing query:\r\n" + ex.Message);
				return "";
			}

			if (SessionManager.CurrentResultKeysCount == 0)
			{
				Progress.Hide();
				MessageBoxMx.ShowError("No results were returned by the query.");
				return "";
			}

			string mstUrl = ServicesIniFile.Read("MstUrl");
			mstUrl = Lex.Replace(mstUrl, "<fileName>", fileName);
			SystemUtil.StartProcess(mstUrl);

			return "";
		}


		/// <summary>
		/// Convert 32 bit cache file to 16 bit
		/// </summary>
		/// <returns></returns>

		static string FixCacheFiles()
		{
			int[] ia = ReadIntArray(@"c:\download\ResultTypeId.bin");

			BinaryWriter bw = BinaryFile.OpenWriter(@"c:\download\ResultTypeId.new");
			foreach (int i in ia)
			{
				if (i >= 65536) bw = bw;
				bw.Write((ushort)i);
			}
			bw.Close();

			return "Fixed";
		}

		/// <summary>
		/// Look for annotation tables where codes don't match
		/// </summary>
		/// <returns></returns>

		public static string FixAnnotationTables()
		{
			StreamWriter sw1 = new StreamWriter(@"c:\xxx\mobius\annot1.txt");
			StreamWriter sw2 = new StreamWriter(@"c:\xxx\mobius\annot2.txt");

			List<UserObject> auos = UserObjectDao.ReadMultiple(UserObjectType.Annotation, true);
			foreach (UserObject uo in auos)
			{
				MetaTable mt = MetaTable.Deserialize(uo.Content);
				if (mt == null) continue; // something wrong with the annotation table content
				if (Lex.Ne("ANNOTATION_" + uo.Id, mt.Name))
					sw1.WriteLine(uo.Id + "," + mt.Name + "," + mt.Code);
				else if (Lex.Ne(uo.Id.ToString(), mt.Code))
					sw2.WriteLine(uo.Id + "," + mt.Name + "," + mt.Code);
				else continue;
			}

			sw1.Close();
			sw2.Close();
			return "";
		}

		static int[] ReadIntArray(string fileName)
		{
			int rowCount = 25238832;

			BinaryReader br = BinaryFile.OpenReader(fileName);

			int[] a = new int[rowCount];
			for (int i1 = 0; i1 < rowCount; i1++)
			{
				a[i1] = br.ReadInt32();
			}

			br.Close();
			return a;
		}

		/// <summary>
		/// Scan all queries and fix permissions on contained annotations, calc fields and lists to match those of the query
		/// </summary>
		/// <returns></returns>

		public static string FixQueryObjectPermissions()
		{
			string msg = ExecuteServiceCommand("FixQueryObjectPermissions");
			return msg;
		}

		public static string FixLogFile()
		{
			throw new NotImplementedException();
		}


		public static string GetStandardMobileQueries()
		{
			Progress.Show("Get Standard Mobile Queries");
			string result = ExecuteServiceCommand("CheckMobileQueries");
			Progress.Hide();
			return result;
		}
		/// <summary>
		/// Return info on user
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string CheckUser(
	string userName)
		{
			Progress.Show("Checking user...");
			UserInfo ui = Security.ReadUserInfo(userName);
			Progress.Hide();
			if (ui == null) return userName + " is not an authorized Mobius user";

			string s =
				"User: " + ui.UserName + "\n" +
				"Domain: " + ui.UserDomainName + "\n" +
				"First Name: " + ui.FirstName + "\n" +
				"Middle Initial: " + ui.MiddleInitial + "\n" +
				"Last Name: " + ui.LastName + "\n\n" +

				"Privileges: " + ui.Privileges.PrivilegeListString;

			return s;
		}

		public static string DemoView(string args)
		{
			bool val;

			if (Lex.IsUndefined(args))
				val = true;

			else val = Lex.BoolParse(args);

			if (val == true)
				return SetCommand("Database View PublicCorpIdSubset");

			else return SetCommand("Database View AllData");
		}

		/// <summary>
		/// Get services reboot time
		/// </summary>
		/// <returns></returns>

		public static string ShowLastBootUpTime()
		{
			DateTime dt = UsageDao.GetServiceServerRebootTime();
			return dt.ToString();
		}

		/// <summary>
		/// Show stats on this process
		/// </summary>
		/// <returns></returns>

		public static string ShowProcess()
		{
			System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();

			string txt =
							"MachineName: " + System.Environment.MachineName + "\n\n" +

							"VirtualMemorySize:		" + (p.VirtualMemorySize64 / 1000000).ToString() + "\n" +
							"PeakVirtualMemorySize:	" + (p.PeakVirtualMemorySize64 / 1000000).ToString() + "\n\n" +
							"PagedMemorySize:		" + (p.PagedMemorySize64 / 1000000).ToString() + "\n" +
							"PeakPagedMemorySize:	" + (p.PeakPagedMemorySize64 / 1000000).ToString() + "\n" +
							"PagedSystemMemorySize:	" + (p.PagedSystemMemorySize64 / 1000000).ToString() + "\n" +
							"NonpagedSystemMemorySize: " + (p.NonpagedSystemMemorySize64 / 1000000).ToString() + "\n" +
							"PrivateMemorySize:		" + (p.PrivateMemorySize64 / 1000000).ToString() + "\n" +

							"WorkingSet:		" + (p.WorkingSet64 / 1000000).ToString() + "\n" +
							"PeakWorkingSet:		" + (p.PeakWorkingSet64 / 1000000).ToString() + "\n" +
							"MinWorkingSet:		" + ((int)p.MinWorkingSet / 1000000).ToString() + "\n" +
							"MaxWorkingSet:		" + ((int)p.MaxWorkingSet / 1000000).ToString() + "\n\n" +

							"Threads: " + p.Threads.Count + "\n" +
							"HandleCount: " + p.HandleCount + "\n" +
							"UserProcessorTime: " + p.UserProcessorTime.ToString() + "\n" +
							"TotalProcessorTime: " + p.TotalProcessorTime.ToString();


			return txt;
		}

		/// <summary>
		/// ShowApprovedMobiusUsers
		/// </summary>
		/// <returns></returns>

		public static string ShowApprovedMobiusUsers()
		{
			return ShowApprovedMobiusUsers("ApprovedMobiusUsers");
		}

		/// <summary>
		/// ShowApprovedMobiusChemView
		/// </summary>
		/// <returns></returns>

		public static string ShowApprovedMobiusChemView()
		{
			return ShowApprovedMobiusUsers("ApprovedMobiusChemView");
		}

		/// <summary>
		/// ShowApprovedMobiusSequenceView
		/// </summary>
		/// <returns></returns>

		public static string ShowApprovedMobiusSequenceView()
		{
			return ShowApprovedMobiusUsers("ApprovedMobiusSequenceView");
		}

		/// <summary>
		/// ShowApprovedMobiusNoChemView
		/// </summary>
		/// <returns></returns>

		public static string ShowApprovedMobiusNoChemView()
		{
			return ShowApprovedMobiusUsers("ApprovedMobiusNoChemView");
		}

		/// <summary>
		/// ShowApprovedMobiusUsers for specified group
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns></returns>

		static string ShowApprovedMobiusUsers(string groupName)
		{
			string adGroupName;

			if (Lex.Eq(groupName, "ApprovedMobiusNoChemView"))
				adGroupName = groupName; // special synthesized group

			else adGroupName = ServicesIniFile.Read(groupName);

			if (Lex.IsUndefined(adGroupName)) return "Group is not defined in .ini file: " + groupName;

			Progress.Show("Retrieving group members...");
			string result = ExecuteServiceCommand("ShowApprovedMobiusUsers " + adGroupName);

			string clientFileName = // name file with lot & test name
					ClientDirs.TempDir + @"\" + groupName + ".txt";

			StreamWriter sw = new StreamWriter(clientFileName);
			if (sw == null) return result;

			sw.Write(result);
			sw.Close();
			Progress.Hide();

			SystemUtil.StartProcess(clientFileName); // show the file (normally via notepad)

			return "";
		}

		/// <summary>
		/// Show Mobius processes
		/// </summary>
		/// <returns></returns>

		public static string ShowSessions(string arg)
		{
			SessionMonitorDialog.ShowDialog();
			return "";
		}

		public static string ShowSmallWorldDbList(string arg)
		{
			string dbList = ExecuteServiceCommand(CommandLineString);
			string clientFileName = TempFile.GetTempFileName(".txt");

			StreamWriter sw = new StreamWriter(clientFileName);
			if (sw == null) return dbList;

			sw.Write(dbList);
			sw.Close();
			Progress.Hide();

			SystemUtil.StartProcess(clientFileName); // show the file (normally via notepad)

			return "";
		}

		/// <summary>
		/// Show special folders
		/// </summary>
		/// <returns></returns>

		static string ShowSpecialFolders()
		{
			string msg =
				"Programs: " + Environment.GetFolderPath(Environment.SpecialFolder.Programs) + "\r\n" +
				"Personal: " + Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\r\n" +
				"MyDocuments: " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\r\n" +
				"ApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\r\n" +
				"LocalApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\r\n" +
				"CommonApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\r\n" +
				"System: " + Environment.GetFolderPath(Environment.SpecialFolder.System) + "\r\n" +
				"ProgramFiles: " + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\r\n" +
				"CommonProgramFiles: " + Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);

			return msg;
		}

		/// <summary>
		/// Show test of bitmap display
		/// </summary>
		/// <returns></returns>

		static string ShowBitmapTest()
		{
			DialogResult dr = new BitmapTest().ShowDialog();
			return "";
		}

		/// <summary>
		/// Format a time span as hhh:mm:ss with leading zeros in a sortable form
		/// </summary>
		/// <param name="span"></param>
		/// <returns></returns>

		public static string FormatTimeSpan(
						TimeSpan span)
		{
			string sHours;

			int hours = span.Days * 24 + span.Hours;
			if (hours > 99) sHours = hours.ToString();
			else sHours = String.Format(" {0,2:00}", hours);

			return String.Format("{0}:{1,2:00}:{2,2:00}", sHours, span.Minutes, span.Seconds);
		}

		/// <summary>
		/// ResetTableAliases
		/// </summary>
		/// <returns></returns>

		static string ResetTableAliases()
		{
			QbUtil.Query.ResetTableAliases();
			return "";
		}

		/// <summary>
		/// Build prototype metatable xml for a native table name.
		/// Syntax: BuildMetaTableFromDatabaseDictionary [schema].[owner]
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>

		public static string BuildMetaTableXmlFromDatabaseCatalog(
			string tableName)
		{
			if (!InputBoxMx.ShowAsNeeded("Enter Owner.TableName to create metatable for:", "Build MetaTable from Database Catalog", ref tableName)) return "";

			//DbConnection.AssociateSchema
			

			MetaTable mt = MetaTableFactory.GetMetaTableFromDatabaseDictionary(tableName);
			if (mt == null) return "Failed";

			mt.Parent = mt; // point to self for now
			MetaTableCollection.UpdateGlobally(mt);
			QbUtil.AddAndRenderTables(mt.Name); // show it

			string xml = mt.Serialize();
			string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\" + mt.Name + ".txt";
			if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return "";
			StreamWriter sw = new StreamWriter(cFile);
			sw.Write(xml);
			sw.Close();

			SystemUtil.StartProcess(cFile); // show it

			return "";
		}

		/// <summary>
		/// Show the full xml of a query
		/// </summary>
		/// <returns></returns>

		public static string ShowQueryXml()
		{
			string sq = QbUtil.Query.SerializeForDebugging(true);
			UIMisc.ShowTextInBrowserWindow(sq, QbUtil.Query.UserObject.Name);
			return "";
		}

		/// <summary>
		/// ShowQeStats
		/// </summary>
		/// <returns></returns>

		public static string ShowQeStats()
		{
			QueryEngineStatsForm.StartNewQueryExecution(QbUtil.Query);
			return "";
		}

		/// <summary>
		/// Show the Xml for a metatable
		/// </summary>
		/// <param name="metaTableName"></param>
		/// <returns></returns>

		public static string ShowMetaTableXml(
			string metaTableName)
		{
			MetaTable mt;
			if (metaTableName != "") // if arg is defined then it is the metatable name
			{
				mt = MetaTableCollection.Get(metaTableName);
				if (mt == null) return "Can't find metatable: " + metaTableName;
			}
			else // try to use currently selected table in query
			{
				if (QbUtil.Qt == null)
					return "Select a table in the current query that you want to see the Xml for.";
				mt = QbUtil.Qt.MetaTable;
			}

			string xml = mt.Serialize();

			//xml = ""; // alternative for code dev
			//foreach (MetaColumn mc in mt.MetaColumns)
			//{
			//  xml += "AddItem(\"" + mc.Name + "\", \"" + mc.TableMap + "\", \"" + mc.ColumnMap + "\");\r\n";
			//}

			UIMisc.ShowTextInBrowserWindow(xml, mt.Label);
			return "";
		}

		/// <summary>
		/// Convert current query to Mobius Query Language & show it
		/// </summary>
		/// <returns></returns>

		public static string ShowMql(
			string arg)
		{
			string mql;
			try { mql = MqlUtil.ConvertQueryToMql(QbUtil.Query); }
			catch (Exception ex)
			{
				MessageBoxMx.ShowError(ex.Message);
				return "";
			}

			if (Lex.Eq(arg, "format")) // format into multiple lines
				mql = mql.Replace("Append Select ", " -\r\nAppend Select ");

			string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\ShowMql.txt";
			if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return "";
			StreamWriter sw = new StreamWriter(cFile);
			sw.Write(mql);
			sw.Close();

			SystemUtil.StartProcess(cFile); // show it

			QbUtil.RenderQuery(0); // rerender query since root may have been added

			return "";
		}

		/// <summary>
		/// Convert current query to MQL & then back to a query & show it
		/// </summary>
		/// <returns></returns>

		public static string ShowMql2Query()
		{
			string mql = MqlUtil.ConvertQueryToMql(QbUtil.Query); // to mql
			Query q = MqlUtil.ConvertMqlToQuery(mql); // back to query
			QbUtil.NewQuery(); // show in query builder
			QbUtil.SetCurrentQueryInstance(q);
			QbUtil.RenderQuery(0);

			return "";
		}

		/// <summary>
		/// Show the SQL for the search and retrieval steps of the query
		/// </summary>
		/// <returns></returns>

		public static string ShowQuerySqlStatements()
		{
			QbUtil.ShowQuerySqlStatements();
			return "";
		}

		/// <summary>
		/// Set the value of a parameter to be used in a script
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		public static string SetScriptParameter(
			string commandLine)
		{
			string tok, response;

			Lex lex = new Lex();
			lex.OpenString(commandLine);
			tok = lex.Get(); // "parm" or "parameter"

			string parm = lex.Get(); // get parameter to set

			tok = lex.Get();
			if (tok == "=") tok = lex.Get();

			if (Lex.Eq(tok, "GetSearchCriteria"))
			{ // prompt for the search criteria on a metatable.column
				tok = lex.Get();
				if (tok == "(") tok = lex.Get(); // get table.column
				string[] sa = tok.Split('.');
				if (sa.Length != 2)
					return "Syntax: Set Parm <parmName> = GetSearchCriteria(<table.column>)";

				string table = sa[0];
				MetaTable mt = MetaTableCollection.Get(table);
				if (mt == null) return table + " is an invalid table name";

				string column = sa[1];
				MetaColumn mc = mt.GetMetaColumnByName(column);
				if (mc == null) return column + " is an unknown column in table " + table;

				string label = mt.Label + " " + mc.Label;

				QueryTable qt = new QueryTable(mt);
				QueryColumn qc = qt.AddQueryColumnByName(mc.Name, false);

				switch (mc.DataType)
				{

					case MetaColumnType.CompoundId:
						if (CriteriaEditor.GetCompoundIdCriteria(qc))
							qc.Criteria = mc.Name + " " + qc.Criteria;

						break;

					case MetaColumnType.Structure:
						CriteriaEditor.EditStructureCriteria(qc);
						break;

					default:
						CriteriaEditor.GetGeneralCriteria(qc);
						break;
				}

				if (qc.Criteria == "")
				{ // nothing supplied
					if (SS.I.ScriptLevel > 0) SS.I.ScriptCancel = true; // cancel any running script
					return "Parameter not set";
				}

				SS.I.ScriptParameters[parm.ToLower()] = qc.Criteria;

				return parm + " set to " + qc.Criteria;

			} // end for GetSearchCriteria

			else if (tok != "")
			{ // simple set to constant value
				SS.I.ScriptParameters[parm.ToLower()] = Lex.RemoveAllQuotes(tok);
				return parm + " set to " + tok;
			}

			else return "Syntax: Set Parm <parmName> = <value>";
		}

		/// <summary>
		/// Run a script of commands
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static string RunScript(
			string fileName)
		{
			StreamReader sr;
			string txt;

			if (!File.Exists(fileName)) return "Can't open file " + Lex.Dq(fileName);
			sr = new StreamReader(fileName);

			SS.I.ScriptLevel++;
			SS.I.ScriptStreams.Add(null);
			SS.I.ScriptStreams[SS.I.ScriptLevel] = sr;
			return "";
		}

		/// <summary>
		/// Read next line from script file
		/// </summary>
		/// <returns></returns>
		/// 

		public static string ReadScriptFile()
		{
			string txt, tok;
			int i1, i2, i3, i4;

			while (true)
			{

				if (SS.I.ScriptLevel < 0) return null;

				txt = ReadScriptCommand((StreamReader)SS.I.ScriptStreams[SS.I.ScriptLevel]);

				if (txt == null || SS.I.ScriptCancel)
				{ // end of file or script cancelled
					(SS.I.ScriptStreams[SS.I.ScriptLevel]).Close();
					SS.I.ScriptStreams.RemoveAt(SS.I.ScriptLevel);
					SS.I.ScriptLevel--;
					continue; // check prev level
				}

				while ((i1 = txt.IndexOf("&")) >= 0)
				{ // substitute parameters
					for (i2 = i1 + 1; i2 < txt.Length; i2++)
					{
						if (!Char.IsLetterOrDigit(txt[i2])) break;
					}
					tok = txt.Substring(i1 + 1, i2 - i1 - 1).ToLower();
					if (SS.I.ScriptParameters.ContainsKey(tok))
						tok = SS.I.ScriptParameters[tok];
					else tok = "";

					if (i2 < txt.Length)
						txt = txt.Substring(0, i1) + tok + txt.Substring(i2);
					else txt = txt = txt.Substring(0, i1) + tok;
				}

				if (txt.StartsWith("*") || txt.StartsWith("//") ||
								txt.StartsWith(";")) continue; // comment

				return txt;

			} // end of while(true) loop
		}

		/// <summary>
		/// Read a line from a script file.
		/// If the line ends in a dash remove the dash and
		/// append the following line.
		/// </summary>
		/// <param name="sr"></param>
		/// <returns></returns>

		public static string ReadScriptCommand(
						StreamReader sr)
		{
			string rec = "";
			while (true)
			{
				string txt = sr.ReadLine();
				if (txt == null) return txt;
				txt = txt.Trim();
				if (txt == "") continue;
				rec += txt;
				if (!rec.EndsWith("-")) return rec; // continuation? 
				else rec = rec.Substring(0, rec.Length - 1);
			}
		}

		/// <summary>
		/// Spawn a new script in a separate process
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static string SpawnScript(string args)
		{
			Lex lex = new Lex();
			lex.OpenString(args);
			string fileName = lex.Get(); // name of script to run
			string userName = lex.Get(); // optional user name to run it under

			fileName = Lex.RemoveAllQuotes(fileName);
			if (fileName.Trim() == "") return "A file name must be supplied";

			if (!File.Exists(fileName)) return "File " + Lex.Dq(fileName) + " does not exist";

			string command = "run script " + Lex.Dq(fileName);
			try
			{
				StartForegroundSession(command, userName);
			}
			catch (Exception ex)
			{
				DebugLog.Message("ex.InnerException: " + ex.InnerException);
				DebugLog.Message("ex.StackTrace" + ex.StackTrace);
				return "Failed to spawn process for file: " + fileName + ", Error = " + ex.Message;
			}

			return "Process started for file " + fileName;
		}

		/// <summary>
		/// Start a new foreground session and start it with the specified command
		/// </summary>
		/// <param name="command"></param>

		public static void StartForegroundSession(
			string command,
			string userName)
		{

			if (Lex.IsNullOrEmpty(userName) || Lex.Eq(userName, SS.I.UserName))
				userName = SS.I.UserName;

			else // start as other user
			{
				if (!Security.IsAdministrator(SS.I.UserName))
					throw new Exception("Account " + SS.I.UserName + " is not authorized to start a script as user " + userName);
			}

			string args = "";
			if (!SS.I.Attended) args = "Unattended ";
			args +=
					"UserName = " + userName + " " +
					"Command = " + Lex.AddSingleQuotes(command);

			string fileName = ClientDirs.ExecutablePath;
			DebugLog.Message("fileName: " + fileName);
			DebugLog.Message("args: " + args);
			try
			{
				Process process = new Process();
				process.StartInfo.FileName = fileName;
				process.StartInfo.Arguments = args;
				process.StartInfo.UseShellExecute = false;

				ServicesLog.Message("FileName:        " + process.StartInfo.FileName);
				ServicesLog.Message("Arguments:       " + process.StartInfo.Arguments);
				ServicesLog.Message("UseShellExecute: " + process.StartInfo.UseShellExecute);

				process.Start();
				//process.WaitForExit();  // may try this later
				//Process p = Process.Start(fileName, args);
			}
			catch (Exception ex)
			{
				ServicesLog.Message(ex.Message);
			}

		}

		/// <summary>
		/// Run a background command as a separate Mobius session
		/// Example full command: StartBackgroundSession Unattended UserName = [userId] Command = 'RunQueryInBackground 647978 True'
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static void StartBackgroundSession(string command)
		{
			string args =
					"Unattended " +
					"UserName = " + SS.I.UserName + " " +
					"Command = " + Lex.AddSingleQuotes(command);

			string fullCommand = "StartBackgroundSession " + args;

			bool runInBackground = !ServicesIniFile.ReadBool("RunBackgroundProcessesInForeground", false);
			if (runInBackground) // normal background start
			{
				string result = ExecuteServiceCommand(fullCommand);
			}

			else // background processing turned off, do within current process
			{
				SS.I.Attended = false;
				CommandLine.Execute(command);
				SS.I.Attended = true;
			}

			return;
		}

		/// <summary>
		/// Sort the children of a contents tree node by name & display result
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static string SortTreeNode(
						string nodeName)
		{
			MetaTreeNode mtn, mtn2;
			int i1, i2;

			mtn = MetaTree.GetNode(nodeName);
			if (mtn == null) return "Node not found";

			MetaTreeNode[] mtna = new MetaTreeNode[mtn.Nodes.Count];
			for (i1 = 0; i1 < mtn.Nodes.Count; i1++)
			{
				mtn2 = (MetaTreeNode)mtn.Nodes[i1];
				for (i2 = i1 - 1; i2 >= 0; i2--)
				{
					if (String.Compare(mtna[i2].Label, mtn2.Label, true) < 0) break;
					mtna[i2 + 1] = mtna[i2];
				}

				mtna[i2 + 1] = mtn2;
			}

			string cFile = ClientDirs.DefaultMobiusUserDocumentsFolder + "\\" + nodeName + ".txt";
			if (!UIMisc.CanWriteFileToDefaultDir(cFile)) return "";
			StreamWriter sw = new StreamWriter(cFile);
			for (i1 = 0; i1 < mtna.Length; i1++)
			{
				mtn2 = mtna[i1];
				string xml =
								"<child name=" + Lex.Dq(mtn2.Name) + " " +
								"l=" + Lex.Dq(mtn2.Label) + " " +
								"type=\"metatable\" />";
				sw.WriteLine(xml);
			}

			sw.Close();

			SystemUtil.StartProcess(cFile); // show it

			return "";
		}

		public static string ShowGenericMetaBrokerVars()
		{
			throw new NotImplementedException();
#if false
			return "MultipleKeyRetrievalCount = " + GenericMetaBroker.MultipleKeyRetrievalCount +
				"\r\nSingleKeyRetrievalCount = " + GenericMetaBroker.SingleKeyRetrievalCount;
#endif
		}

		/// <summary>
		/// Paste query xml from clipboard & open. This utility is used mainly 
		/// for examining queries that have been logged to the debuglog.
		/// </summary>
		/// <returns></returns>

		public static string OpenQueryFromClipboard()
		{
			Query q = null;
			string msg;

			Progress.Show("Opening query from clipboard...");

			string queryXml = Clipboard.GetText();
			try { q = Query.Deserialize(queryXml); }
			catch (Exception ex)
			{
				Progress.Hide();
				return DebugLog.FormatExceptionMessage(ex);
			}

			if (Lex.IsNullOrEmpty(q.UserObject.Name) || q.UserObject.Id <= 0)
			{ // assign a name
				ClipBoardQuery++;
				q.UserObject.Name = "Clipboard Query " + ClipBoardQuery;
			}

			string nextCommand = QbUtil.AddQueryAndRender(q, false);
			Progress.Hide();
			return nextCommand;
		}

		/// <summary>
		/// Command line open query with queryId
		/// </summary>
		/// <param name="sid"></param>
		/// <returns></returns>

		public static string OpenQuery(
			string queryIdString)
		{
			UserObject uo = null;
			int objId = -1;
			string msg;

			if (Lex.IsInteger(queryIdString))
				objId = Int32.Parse(queryIdString);

			else
			{
				string querySubString = Lex.RemoveAllQuotes(queryIdString);
				objId = QueryReader.FindQueryByNameSubstring(querySubString, out msg);
				if (!String.IsNullOrEmpty(msg)) return msg;
			}

			uo = UserObjectDao.ReadHeader(objId);
			if (uo == null || uo.Type != UserObjectType.Query)
				if (uo == null) return "Failed to read query " + queryIdString;

			if (!UserObjectUtil.UserHasReadAccess(SS.I.UserName, objId))
			{
				if (Security.IsAdministrator(SS.I.UserName)) { } // allow read for admin
				else return "Your account is not authorized to read this query";
			}

			string queryName = uo.InternalName;
			string nextCommand = QbUtil.OpenQuery(queryName);
			return nextCommand;
		}

		/// <summary>
		/// Command line run query with queryId
		/// </summary>
		/// <param name="queryId"></param>
		/// <returns></returns>

		public static string RunQuery(
		string queryIdString)
		{
			int objId = -1;
			UserObject uo = null;

			try
			{
				objId = Int32.Parse(queryIdString);
				uo = UserObjectDao.ReadHeader(objId);
			}
			catch { }

			if (uo == null || uo.Type != UserObjectType.Query)
				if (uo == null) return "Failed to read query " + queryIdString;

			if (!UserObjectUtil.UserHasReadAccess(SS.I.UserName, objId))
				return "Your account is not authorized to read this query";

			string queryName = uo.InternalName;
			string nextCommand = QbUtil.OpenQuery(queryName);
			return QueryExec.RunQuery(QbUtil.Query, OutputDest.WinForms); // run it
		}


		/// <summary>
		///  Run current query query as popup
		/// </summary>
		/// <returns></returns>

		public static string RunQueryAsPopup()
		{
			Progress.Show("Retrieving data..."); // put up progress dialog since this may take a while
			QbUtil.RunPopupQuery(QbUtil.Query, QbUtil.Query.Name);
			Progress.Hide();
			return "";
		}

		/// <summary>
		/// Kill a user session in the current process identified by thread Id
		/// </summary>
		/// <param name="sid"></param>
		/// <returns></returns>
		/// 

		public static string KillSession(
						string sid)
		{
			throw new NotImplementedException();
#if false
			if (!Security.IsAdministrator())
				return "You must be a Mobius administrator to kill a session";

			for (int ti = 0; ti < ProcessState.Sessions.Count; ti++)
			{ // see if main Mobius thread
				SessionState ms = ProcessState.Sessions[ti];
				if (ms == null) continue;
				if (ms.PhysicalThreadId.ToString() == sid)
				{
					try { ms.Thread.Abort(); }
					catch (Exception ex) { return "Kill failed: " + ex.Message; }
					return "Session killed";
				}
			}

			return sid + " is an invalid session identifier";
#endif
		}

		/// <summary>
		/// Decode a B64 string
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static string B64Decode(
			string txt)
		{
			return Lex.B64Decode(txt);
		}

		/// <summary>
		/// Encode a B64 string
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>

		public static string B64Encode(
			string txt)
		{
			return Lex.B64Encode(txt);
		}

		/// <summary>
		/// Change the owner of a UserObject
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string ChangeOwner(string args)
		{
			int objId = -1;

			Lex lex = new Lex();
			lex.OpenString(args);
			string uoName = lex.GetUpper();
			if (Lex.Eq(uoName, "of")) uoName = lex.GetUpper();

			string newOwner = lex.GetUpper();
			if (Lex.Eq(newOwner, "to")) newOwner = lex.GetUpper();

			if (uoName == "" || newOwner == "")
				return "Example syntax: Change Owner Of Annotation_123 To RX123456";

			if (uoName.Contains("_"))
				objId = UserObject.ParseObjectIdFromInternalName(uoName);
			else int.TryParse(uoName, out objId);

			return UserObjectUtil.ChangeOwner(objId, newOwner);
		}

		/// <summary>
		/// ExecuteServiceCommandWithProgress
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static string ExecuteServiceCommandWithProgress(string command)
		{
			Progress.Show("Executing command...");
			string result = ExecuteServiceCommand(command);
			Progress.Hide();
			return result;
		}

		/// <summary>
		/// Execute a service command
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static string ExecuteServiceCommand(string command)
		{
			string result = CommandDao.Execute(command);
			return result;
		}

		/// <summary>
		/// DebugTest
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string DebugTest(
				string args)
		{
			string msg = "There was no endpoint listening at";
			ServicesLog.Message(msg);
			return msg;
		}

		/// <summary>
		/// Load Test server / services
		/// </summary>
		/// <param name="args">Number of sessions to create optionally followed by an initial command to run.
		/// Example: Test ServerLoad 200 Command='Run Query 377898'
		/// </param>
		/// <returns></returns>

		public static string LoadTestServer(
				string args)
		{
			string sessionCountArg;
			int sessionCount;

			IniFile clientServicesIniFile = // see if service connection info is in MobiusClientUser.ini & use if so
				IniFile.OpenClientIniFile("MobiusClientUser.ini");

			List<Mobius.NativeSessionClient.NativeSessionClient> nscList = new List<Mobius.NativeSessionClient.NativeSessionClient>();

			int i1 = args.IndexOf(" ");
			if (i1 < 0)
			{
				sessionCountArg = args.Trim();
				args = "";
			}

			else
			{
				sessionCountArg = args.Substring(0, i1).Trim();
				args = args.Substring(i1 + 1);
			}

			sessionCount = Int32.Parse(sessionCountArg);

			for (int pCnt = 1; pCnt <= sessionCount; pCnt++)
			{

				// Start normal interactive session

				Process p = Process.Start(Application.ExecutablePath, args);

				// Start session that communicates via service host

				//Mobius.NativeSessionClient.NativeSessionClient nsc = new NativeSessionClient.NativeSessionClient();
				//nsc.CreateSession(clientServicesIniFile, false, false); // start a service host type session
				//nscList.Add(nsc);

				//SessionManager sm = new SessionManager();
				//sm.Initialize(null);

				//ExecuteServiceCommand("StartBackgroundSession");

				Progress.Show("Sessions created: " + pCnt + " / " + sessionCount);
				Thread.Sleep(3000);
			}


			Progress.Hide();

			return "";
		}

		/// <summary>
		/// Register assembly 
		/// </summary>
		/// <param name="dllPath"></param>
		/// <returns></returns>

		public static string RegisterAssembly(string dllPath)
		{
			SessionManager.RegisterAssembly(dllPath);
			return "Registered assembly: " + dllPath;
		}

		/// <summary>
		/// FixQueries
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string FixQueries()
		{
			UserObject uo;
			Alert a = null;
			Query q;
			QueryColumn qc;

			// Scan alert queries (find alert query that doesn't complete)

			if (true)
			{
				string queries = "";
				List<UserObject> alerts = UserObjectDao.ReadMultiple(UserObjectType.Alert, false);
				for (int i1 = 0; i1 < alerts.Count; i1++)
				{
					uo = alerts[i1];
					a = null;
					q = null;
					try
					{
						a = Alert.GetAlertFromUserObject(uo, true);
						q = QbUtil.ReadQuery(a.QueryObjId);
					}
					catch (Exception ex)
					{
						if (a == null) queries += "\n" + uo.Id + " (alert error)";
						else queries += "\n" + a.QueryObjId + " (query error)";
						continue;
					}
					bool r3 = false, r5 = false, r42 = false;
					foreach (QueryTable qt in q.Tables)
					{
						qc = qt.GetQueryColumnByName("R_3");
						if (qc != null && qc.Criteria != "") r3 = true;
						qc = qt.GetQueryColumnByName("R_5");
						if (qc != null && qc.Criteria != "") r5 = true;
						qc = qt.GetQueryColumnByName("R_42");
						if (qc != null && qc.Criteria != "") r42 = true;
					}

					if (r3 && r5 && r42)
						queries += "\n" + a.QueryObjId;

					if (Lex.Contains(q.ComplexCriteria, "r_3") && Lex.Contains(q.ComplexCriteria, "r_5") && Lex.Contains(q.ComplexCriteria, "r_42"))
						queries += "\n" + a.QueryObjId;

					//if (q.LogicType == QueryLogicType.Or && q.GetCriteriaCount(false, false) == 3)  // find alert queries with OR login
					//    queries += "\n" + a.QueryObjId;
				}
				return queries;
			}

			// Deserialize UserDataImportState

#if false
			uo = UserObjectDao.Read(333142);
			UserDataImportState uds =	UserDataImportState.Deserialize(uo);
			return "";
#endif
		}

		/// <summary>
		/// Recreate accidently automatically deleted alerts (2/5/2015)
		/// Input line: =====>>> 1/28/2015 12:20:32 AM [MOBIUS] - [Alert] - Completed Alert: 570624, QueryId: 570623, Deleting Alert_570623, no associated query, Time to process = 00:00:00, Pid: 1232
		/// </summary>
		/// <returns></returns>

		public static string FixDeletedAlerts()
		{
			int fixCnt = 0;

			StreamReader sr = new StreamReader(@"c:\download\AlertsDeleted2.txt");
			StreamWriter sw = new StreamWriter(@"c:\download\AlertsFixed.txt");
			while (true)
			{
				string rec = sr.ReadLine();
				if (rec == null) break;

				if (!Lex.Contains(rec, "Deleting Alert_"))
				{
					//sw.WriteLine("Deleting Alert_ not found: " + rec);
					continue;
				}

				int qid = ParseUoId(rec, ", QueryId: ");
				if (qid < 0)
				{
					sw.WriteLine("QueryId not found: " + rec);
					continue;
				}

				int aid = ParseUoId(rec, "Completed Alert: ");
				if (aid < 0)
				{
					sw.WriteLine("AlertId not found: " + rec);
					continue;
				}

				UserObject quo = UserObjectDao.ReadHeader(qid);
				if (quo == null)
				{
					sw.WriteLine("Query not found: " + rec);
					continue;
				}

				Alert alert = Alert.GetAlertByQueryId(qid);
				if (alert != null)
				{
					sw.WriteLine("Alert " + alert.Id + " already exists for Query : " + qid);
					continue;
				}

				alert = new Alert(quo);
				alert.Id = aid; // ignored if new (which it should be)
												//alert.Interval = 1;
												//alert.MailTo = Security.GetUserEmailAddress(quo.Owner);
												//alert.Owner = quo.Owner;
												//alert.QueryObjId = quo.Id;
												//alert.QueryName = quo.Name;
												//alert.LastQueryUpdate = quo.UpdateDateTime;

				alert.Write();

				sw.WriteLine("Created alert for query: " + qid + ", " + quo.Name + ", Owner: " + alert.MailTo + " --- " + rec);
				fixCnt++;
			}

			sr.Close();
			sw.Close();

			return "Alerts fixed: " + fixCnt;
		}

		static int ParseUoId(string rec, string match)
		{
			int uoId;

			int pos = rec.IndexOf(match);
			if (pos < 0) return -1;
			string s = rec.Substring(pos + match.Length);
			pos = s.IndexOf(",");
			if (pos < 0) return -1;
			s = s.Substring(0, pos).Trim();
			if (!int.TryParse(s, out uoId)) return -1;
			return uoId;
		}

		/// <summary>
		/// Call a static method on a class using reflection
		/// </summary>
		/// <param name="cmdLine">e.g. Call Class.Method arg1 arg2 ... argn</param>
		/// <returns></returns>

		public static string CallClassMethod(
			string cmdLine)
		{
			string methodRef = "", args = "";
			object returnValue;

			cmdLine = cmdLine.Trim();
			int i1 = cmdLine.IndexOf(" ");
			if (i1 < 0)
				methodRef = cmdLine.Trim();
			else
			{
				methodRef = cmdLine.Substring(0, i1).Trim();
				args = cmdLine.Substring(i1 + 1).Trim();
			}

			if (ReflectionMx.IsServiceClassMemberRef(methodRef))
				returnValue = ExecuteServiceCommand(CommandLineString);

			else returnValue = ReflectionMx.CallMethod(methodRef, args);

			if (returnValue == null) return "";
			else return returnValue.ToString();
		}

		/// <summary>
		/// Set a static property/field value for a class using reflection
		/// </summary>
		/// <param name="cmdLine">e.g. Set RelatedStructureSearch.DefaultMaxHits 200</param>
		/// <returns></returns>

		public static string SetClassStaticMemberValue(
			string cmdLine)
		{
			string memberRef = "", value = "";
			cmdLine = cmdLine.Trim();
			int i1 = cmdLine.IndexOf(" ");
			if (i1 < 0) return "Syntax: SET <class.memberName> <value>";

			memberRef = cmdLine.Substring(0, i1).Trim();
			value = cmdLine.Substring(i1 + 1).Trim();
			value = Lex.RemoveAllQuotes(value);

			ReflectionMx.SetMemberValue(memberRef, value);
			return "";
		}

		/// <summary>
		/// Get a static property/field value for a class using reflection
		/// </summary>
		/// <param name="cmdLine">e.g. Get RelatedStructureSearch.DefaultMaxHits</param>
		/// <returns></returns>

		public static string GetStaticStringMemberValue(
			string cmdLine)
		{
			string value;

			string memberRef = cmdLine.Trim();

			if (ReflectionMx.IsServiceClassMemberRef(memberRef))
				value = ExecuteServiceCommand(CommandLineString);

			else value = ReflectionMx.GetMemberStringValue(memberRef);

			if (value == null) return memberRef + " = null";
			else return memberRef + " = " + value;
		}

		//private static LongRunningCommand CreateLongRunningCommand(string commandName, bool reloadTreeOnCompletion)
		//{
		//    LongRunningCommand longRunningCommand = new LongRunningCommand(commandName);
		//    longRunningCommand.UpdateProgress = new LongRunningCommandUpdateDelegate(Progress.InvokeShow);
		//    longRunningCommand.HideProgress = new LongRunningCommandHideProgressDelegate(Progress.InvokeHide);
		//    longRunningCommand.ShowResponse = new LongRunningCommandShowResponseDelegate(MessageBoxMx.InvokeShow);
		//    if (reloadTreeOnCompletion)
		//    {
		//        longRunningCommand.OnCompletion =
		//            new LongRunningCommandOnCompletionDelegate(
		//                ReloadContentsTree);
		//    }
		//    return longRunningCommand;
		//}

		//private static LongRunningCommand StartLongRunningCommand(LongRunningCommand longRunningCommand)
		//{
		//    //move out of the gui thread so that Thread.Sleep between polls doesn't freeze the ui
		//    ParameterizedThreadStart threadStart =
		//        new ParameterizedThreadStart(ServiceFacade.ExecuteServiceCommandLongRunningCommand);
		//    Thread thread = new Thread(threadStart);
		//    thread.IsBackground = false;
		//    thread.Start(longRunningCommand);
		//    return longRunningCommand;
		//}

	} // CommandLine

	/// <summary>
	/// Info on a command
	/// </summary>

	class Command
	{
		public string Name = "";
		public object Method = null;
		public int VisibilityLevel = 0; // 0 = hidden, 1 = devs, 2 = admins
		public bool IsStartOfSection = false;
		public bool AllowAddToSubmenu = true;
	}
}
