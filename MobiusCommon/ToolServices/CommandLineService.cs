using Mobius.ComOps;
using Mobius.Data;
using Mobius.MetaFactoryNamespace;
using Mobius.QueryEngineLibrary;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;

namespace Mobius.ToolServices
{
	/// <summary>
	/// Command delegates
	/// </summary>
	/// <returns></returns>

	public delegate string CM0(); // command method with no parameters
	public delegate string CM1(string args); // command method with args parameter

	/// <summary>
	/// CommandLineService
	/// </summary>

	public class CommandLineService
	{
		static Dictionary<string, object> DispatchTable; // method associated with each command
		static Dictionary<string, string> ProcessUserName = new Dictionary<string, string>(); // used for quick lookup of usernames for processes
		static Exception ThreadException; // exception from thread for command
		static ManualResetEvent ThreadWaitEvent = new ManualResetEvent(false);
		static string CommandResponse;

		/// <summary>
		/// Build dictionary of commands & associated methods in initialization
		/// Note that for proper operation a command in the dispath table cannot
		/// be a prefix of another command.
		/// </summary>

		static void InitializeDispatchTable()
		{
			DispatchTable = new Dictionary<string, object>();

			AC("Check Mobile Queries", new CM0(GetStandardMobileQueries));

			AC("Analyze Queries", new CM1(AnalyzeQueries));
			AC("Associate Schema", new CM1(AssociateSchema));
			AC("Build QueryTable Oracle View", new CM1(BuildQueryTableOracleView));

			AC("Call", new CM1(CallClassMethod));

			AC("CopyPrdAnnotationTableDataToDev", new CM0(AnnotationDao.CopyPrdAnnotationTableDataToDev));

			AC("Delete Inactive Temp Files", new CM0(DeleteInactiveTempFiles));
			AC("Fix QueryObject Permissions", new CM0(FixQueryObjectPermissions));
			AC("Get", new CM1(GetStaticStringMemberValue));
			AC("GcServices", new CM0(WindowsHelper.GarbageCollect));

			AC("Load MetaData", new CM1(LoadMetadata));

			AC("Set", new CM1(SetCommand));
			AC("StartBackgroundSession", new CM1(StartBackgroundSession));
			AC("StartServerProcess", new CM1(StartServerProcess));

			AC("Test SimSearchMx", new CM1(Mobius.CdkSearchMx.CdkSimSearchMx.Test));
			AC("Test Experimental Code", new CM0(TestExperimentalCode));
			AC("Test Key Connections", new CM0(TestKeyOracleConnections));
			AC("Test SQL", new CM1(TestSql));

			AC("ProjectTreeFactory.GetAfsProjectId", new CM1(AfsProject.GetAfsProjectIdString));
			AC("ProjectTreeFactory.GetProjectHtmlDescription", new CM1(AfsProject.GetProjectHtmlDescription));

			AC("Show ApprovedMobiusUsers", new CM1(ShowApprovedMobiusUsers));
			AC("Show Qe Stats", new CM0(ShowQeStats));
			AC("Show DesktopHeap", new CM0(ShowDesktopHeap));
			AC("Show OracleClientVersion", new CM0(ShowOracleClientVersion));
			AC("Show SmallWorld DbList", new CM1(ShowSmallWorldDbList));

			AC("Update AnnotationTableDataRowOrder", new CM1(AnnotationDao.ReorderRows));
			AC("Update AssayAttributesTable", new CM1(UnpivotedAssayMetaFactory.UpdateAssayAttributesTable));
			AC("Update Key Connections Message", new CM0(UpdateKeyOracleConnectionsMessage));
			AC("Update CorpDbMoltableMx", new CM1(StructureTableDao.UpdateCorpDbMoltableMx));
			AC("Update FingerprintDatabaseMx", new CM1(Mobius.CdkSearchMx.FingerprintDbMx.Update));
			AC("Update MetaTable Statistics", new CM1(UpdateMetaTableStatistics));
			AC("Update MetaTree Cache", new CM0(UpdateMetaTreeCache));
			AC("Update MetatreeNodeTable", new CM0(UpdateMetatreeNodeTable));
			AC("Update Assay Metadata", new CM1(UpdateAssayMetadata));
			AC("Update TnsNames", new CM0(UpdateTnsNames));
		}

		/// <summary>
		/// Add command to hash table of normalized commands
		/// </summary>
		/// <param name="commandName"></param>
		/// <param name="commandMethod"></param>

		public static void AC(
				string commandName,
				object commandMethod)
		{
			AddCommand(commandName, commandMethod);
		}

		public static void AddCommand(
				string commandName,
				object commandMethod)
		{
			commandName = commandName.Replace(" ", "").ToLower();
			DispatchTable[commandName] = commandMethod;
		}

		/// <summary>
		/// Call method to process a command line command
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		public static string ExecuteCommand(string commandLine)
		{
			string args, response;
			int i1;

			if (DispatchTable == null) InitializeDispatchTable();

			if (commandLine.ToLower().StartsWith("command ") || // remove any leading "command"
					commandLine.ToLower().StartsWith("commandline "))
			{
				i1 = commandLine.IndexOf(" ");
				if (i1 >= 0 && i1 + 1 < commandLine.Length)
					commandLine = commandLine.Substring(i1 + 1);
			}

			if (commandLine.Trim() == "") return null;

			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string command = "";

			while (true)
			{ // catenate command tokens until we get a match
				string tok = lex.GetLower();
				if (tok == "") return null; // not a command
				command += tok;
				if (DispatchTable.ContainsKey(command)) break;
			}

			// Call the command

			object cmo = DispatchTable[command];

			int pos = lex.Position; // get any remaining arguments
			if (pos >= commandLine.Length) args = "";
			else args = commandLine.Substring(pos).Trim();
			object[] parms = new object[] { cmo, args };

			Thread t = new Thread(new ParameterizedThreadStart(ExecuteCommandThreadMethod));
			t.Name = "Command: " + command;
			t.IsBackground = true;
			t.SetApartmentState(ApartmentState.STA);

			ThreadException = null;
			CommandResponse = "";

			t.Start(parms);

			//ThreadWaitEvent.Reset();
			//ThreadWaitEvent.WaitOne(); // seems to block timer event on progress form

			while (t.IsAlive)
			{
				Thread.Sleep(100);
				Application.DoEvents();
				if (Progress.CancelRequested)
				{
					t.Abort();
					Progress.Hide();
					return "Cancelled";
				}
			}

			if (ThreadException == null)
				return CommandResponse;

			else // exception
			{
				Exception ex = ThreadException;
				string msg = "Exception for command: " + commandLine + "\n" +
					DebugLog.FormatExceptionMessage(ex);
				DebugLog.Message(msg);
				throw new Exception(msg, ex);
			}
		}

		/// <summary>
		/// Thread method that executes the command
		/// </summary>
		/// <param name="parm"></param>

		static void ExecuteCommandThreadMethod(object parm)
		{
			object[] parms;
			object cmo;
			string args;

			try
			{
				parms = parm as object[];
				cmo = parms[0];
				args = parms[1] as string;

				if (cmo is CM0) CommandResponse = ((CM0)cmo)();
				else CommandResponse = ((CM1)cmo)(args);

				if (CommandResponse == null) CommandResponse = "";
				return;
			}

			catch (Exception ex)
			{
				ThreadException = ex; // save for later pickup
				return;
			}

			finally
			{
				//ThreadWaitEvent.Set();
			}
		}


		static string GetStandardMobileQueries()
		{
			Query[] queries = QueryEngine.GetStandardMobileQueries();
			StringBuilder stringbuilder = new StringBuilder();
			foreach (Query query in queries)
			{
				stringbuilder.Append(query.Name);
				stringbuilder.Append(Environment.NewLine);
			}
			return stringbuilder.ToString();
		}

		/// <summary>
		/// Read all queries and output each column selected or filtered
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		static string AnalyzeQueries(string command)
		{
			DbCommandMx dao = new DbCommandMx();

			if (Lex.IsNullOrEmpty(command)) command = "0";
			string sql = @"
				select obj_id, obj_cntnt  
				FROM mbs_owner.MBS_USR_OBJ
				where obj_typ_id = 2
				/* and obj_id in (103482, 318320, 256340) */
				and obj_id >= <startId> 
				order by obj_id";

			sql = sql.Replace("<startId>", command);

			string header = "QueryId, TableName, ColumnName, ColumnType, Selected, FilterType";
			StreamWriter sw = new StreamWriter(@"c:\download\AnalyzeQueries.csv");
			sw.WriteLine(header);

			dao.Prepare(sql);
			dao.ExecuteReader();
			int queryCount = 0, rowCount = 0, exceptions = 0;
			while (dao.Read())
			{
				try
				{
					int objId = dao.GetIntByName("obj_id");
					if (dao.IsNull(1)) continue;

					string content = dao.GetClob(1);
					Query q = Query.Deserialize(content);
					queryCount++;
					if (queryCount % 10 == 0)
						Mobius.UAL.Progress.Show("Queries: " + queryCount);
					for (int qti = 0; qti < q.Tables.Count; qti++)
					{
						QueryTable qt = q.Tables[qti];
						foreach (QueryColumn qc in qt.QueryColumns)
						{
							if (!qc.Selected && Lex.IsNullOrEmpty(qc.Criteria)) continue;
							MetaColumn mc = qc.MetaColumn;
							MetaTable mt = mc.MetaTable;
							string rec =
								objId.ToString() + "," +
								mt.Name + "," +
								mc.Name + "," +
								mc.DataType.ToString() + "," +
								qc.Selected.ToString() + ",";

							ParsedSingleCriteria psc = null;
							if (mc.IsKey)
							{
								if (!Lex.IsNullOrEmpty(q.KeyCriteria))
									psc = MqlUtil.ParseSingleCriteria(mc.Name + " " + q.KeyCriteria);
							}

							else if (!Lex.IsNullOrEmpty(qc.Criteria))
								psc = MqlUtil.ParseSingleCriteria(qc.Criteria);

							if (psc != null) rec += psc.Op;
							sw.WriteLine(rec);
							rowCount++;
						}
					}
				}

				catch (Exception ex)
				{
					exceptions++;
				}
			}

			dao.CloseReader();
			sw.Close();

			return "Queries: " + queryCount + ", Rows: " + rowCount + ", Exception: " + exceptions;
		}

		/// <summary>
		/// Associate a schema with a particular data source
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		static string AssociateSchema(string args)
		{
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

			if (!DataSourceMx.DataSources.ContainsKey(dsName))
				return "Data source " + dsName + " is not defined";

			DbSchemaMx s = new DbSchemaMx();
			s.Name = schemaName;
			s.DataSourceName = dsName;
			DataSourceMx.Schemas[schemaName] = s;

			return "Association complete";
		}

		/// <summary>
		/// DeleteInactiveTempFiles
		/// </summary>
		/// <returns></returns>

		static string DeleteInactiveTempFiles()
		{

			string msg = "Temporary session files: " + DeleteInactiveTempFiles(ServicesDirs.TempDir, 3);
			msg += "\r\nBackground export files: " + DeleteInactiveTempFiles(ServicesDirs.BackgroundExportDir, 30);
			return msg;
		}

		static string DeleteInactiveTempFiles(
			string dir,
			int ageInDays)
		{
			string[] files = Directory.GetFiles(dir);
			int deleteCount = 0;
			int lastUpdate = 0;
			foreach (string fileName in files)
			{
				TimeSpan ts = DateTime.Now.Subtract(File.GetLastWriteTime(fileName));
				if (ts.Days < ageInDays) continue; // beyond cutoff

				try { File.Delete(fileName); }
				catch (Exception ex) { continue; }
				deleteCount++;

				int t1 = TimeOfDay.Milliseconds() - lastUpdate;
				if (t1 > 1000)
				{
					string msg = "Deleted " + deleteCount.ToString() + " of " + files.Length.ToString() + " temp files...";
					UAL.Progress.Show(msg, UmlautMobius.String, false);
					lastUpdate = TimeOfDay.Milliseconds();
				}
			}

			UAL.Progress.Hide();
			return "Deleted " + deleteCount.ToString() + " of " + files.Length.ToString() + " files.";
		}

		/// <summary>
		/// Scan all queries and fix permissions on contained annotations, calc fields and lists to match those of the query
		/// </summary>
		/// <returns></returns>

		public static string FixQueryObjectPermissions()
		{
			int queryCount = 0, updCount = 0;
			List<UserObject> objs = UserObjectDao.ReadMultiple(UserObjectType.Query, false);
			foreach (UserObject uo in objs)
			{
				//if (uo.Id != 318299) continue;
				//if (!queries.Contains(uo.Id.ToString())) continue;
				updCount += UserObjectDao.AssureAccessToSharedQueryUserObjects(uo.Id);
				queryCount++;
				if (queryCount % 10 == 0)
					Mobius.UAL.Progress.Show("Queries: " + queryCount + ", Updates: " + updCount);
			}

			return "Queries: " + queryCount + ", Updates: " + updCount;
		}

		static string LoadMetadata(string serverFileDir)
		{
			DebugLog.UserName = Security.UserName; // set username to put in debug logs

			if (serverFileDir != null)
			{
				while (serverFileDir.EndsWith(@"\"))
				{
					serverFileDir = serverFileDir.Substring(0, serverFileDir.Length - 1);
				}
				if (serverFileDir == "")
				{
					//restore the default paths
					//ServicesDirs.MetaDataDir = _defaultMetaDataDir;
					//UAL.ServicesDirs.BinaryDataDir = _defaultBinaryDataDir;
					//MetaTreeFactory.MetaTreeDir = _defaultMetaTreeDir;
					//MetaTableFactory.MetaTableXmlFolder = _defaultMetaTableXmlDir;
				}
				else
				{
					ServicesDirs.MetaDataDir = serverFileDir;
					//UAL.ServicesDirs.BinaryDataDir = serverFileDir + @"\BinaryData";
					MetaTreeFactory.MetaTreeDir = serverFileDir + @"\MetaTrees";
					MetaTableFactory.MetaTableXmlFolder = serverFileDir + @"\MetaTables";
				}
			}

			//perform the "real" update
			MetaTreeFactory.Reset();
			MetaTableFactory.Reset();  //among other things, unregisters all the factories
			MetaTableFactory.Initialize(); //so we need to reinitialize (with UAL.MetaDataDir possibly changed...)
			MetaTreeFactory.Build(false); // rebuild from sources

			return "Metadata reloaded";
		}

		/// <summary>
		/// Switch the view of the database
		/// </summary>
		/// <param name="viewName"></param>
		/// <returns></returns>

		public static string SetDatabaseView(string viewName)
		{
			if (!Security.IsAdministrator(Security.UserName))
				return "Only Mobius administrators can execute this command";

			RestrictedDatabaseView v = RestrictedDatabaseView.GetRestrictedView(viewName);
			if (v == null) throw new Exception("Unrecognized view: " + viewName);

			UserInfo ui = ClientState.UserInfo;
			if (ui == null) throw new Exception("ClientState.UserInfo is null");

			ui.RestrictedViewUsers = v.Userids;
			ui.RestrictedViewAllowedMetaTables = v.MetaTables;
			ui.RestrictedViewAllowedCorpIds = v.CorpIds;

			string uiString = ui.Serialize();
			return uiString;
		}

		/// <summary>
		/// Set internal program variable
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		static string SetCommand(string args)
		{
			bool b;

			try
			{
				Lex lex = new Lex();
				lex.OpenString(args);
				string itemName = lex.Get(); // item to set
				string arg = lex.Get(); // value to set to

				if (itemName.Contains(".")) // if qualified name, assume class.member name and set using reflection
				{
					SetClassStaticMemberValue(args);
				}

				else if (Lex.Eq(itemName, "LogQeBasics"))
					QueryEngine.LogBasics = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "LogQeDetail"))
					QueryEngine.LogDetail = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "LogQeDataRows"))
					QueryEngine.LogDataRows = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "MultiThreadQE"))
					QueryEngine.MultithreadRowRetrieval = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "AllowMultiTablePivotQE"))
					QueryEngine.AllowMultiTablePivot = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "KeyListPredType") || Lex.Eq(itemName, "KLPT"))
				{
					if (!EnumUtil.TryParse(arg, out DbCommandMx.DefaultKeyListPredType))
						return "Syntax: KLPT [Parameterized | Literal | DbList]";
				}

				else if (Lex.StartsWith(args, "DatabaseView") || Lex.StartsWith(args, "Database View"))
				{
					int i1 = Lex.IndexOf(args, "View ");
					string viewName = args.Substring(i1 + 4).Trim();
					string result = SetDatabaseView(viewName);
					return result;
				}

				else if (Lex.Eq(itemName, "DbList")) // shortcut for database lists
				{
					if (Lex.BoolParse(arg))
						DbCommandMx.DefaultKeyListPredType = KeyListPredTypeEnum.DbList;
					else DbCommandMx.DefaultKeyListPredType = KeyListPredTypeEnum.Parameterized;
				}

				else if (Lex.Eq(itemName, "MinimizeDBLinkUse"))
					QueryEngine.MinimizeDBLinkUse = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "Netezza") || Lex.Eq(itemName, "AllowNetezzaUseForQueryEngine"))
					QueryEngine.AllowNetezzaUse = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "LogDbCommandDetail")) // log DbCommandMx calls
					DbCommandMx.LogDbCommandDetail = Lex.BoolParse(arg);

				else if (Lex.Eq(itemName, "ShowStereoComments"))
					MoleculeUtil.ShowStereoComments = Lex.BoolParse(arg);

				else return "Unrecognized Set command: Set " + args; 
			}

			catch(Exception ex) { throw new Exception(ex.Message, ex); }

			return "The parameter value has been set";
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

			returnValue = ReflectionMx.CallMethod(methodRef, args);

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
			string memberRef = cmdLine.Trim();
			string value = ReflectionMx.GetMemberStringValue(memberRef);
			return value;
		}


		/// <summary>
		/// Start a background session
		/// Examples: 
		///  StartBackgroundSession Unattended UserName = [userName] Command = 'run background export [exportId]'
		///  StartBackgroundSession Unattended UserName = [userName] Command = 'RunQueryInBackground [queryId]'
		///  StartBackgroundSession Unattended UserName = [userName] Command = 'run script [scriptFilePath]'
		///  StartBackgroundSession Unattended UserName = [userName] Command = 'check alert [alertId]'
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>

		public static string StartBackgroundSession(string args)
		{
			if (Lex.Contains(args, "Update Key Connections Message")) // disable here for now
				return "";

			string defaultClientExecutable = ClientDirs.GetClientExecutablePath();
			string fileName = ServicesIniFile.Read("ClientExecutable", defaultClientExecutable);
			if (!Lex.Contains(args, "Unattended")) args = "Unattended " + args; // be sure marked at unattended

			try
			{
				Process process = new Process();
				process.StartInfo.FileName = fileName;
				process.StartInfo.Arguments = args;
				process.StartInfo.UseShellExecute = false;

				DebugLog.Message("FileName:        " + process.StartInfo.FileName);
				DebugLog.Message("Arguments:       " + process.StartInfo.Arguments);
				DebugLog.Message("UseShellExecute: " + process.StartInfo.UseShellExecute);

				process.Start();

				//Process p = Process.Start(fileName, args); // old simple method
			}
			catch (Exception ex)
			{
				DebugLog.Message(ex.Message);
			}

			return "";
		}

		/// <summary>
		/// Start a new Mobius server process
		/// </summary>
		/// <param name="args">Name of executeable to start with optional arguments</param>
		/// <returns></returns>

		public static string StartServerProcess(string args)
		{
			string filePath = "", procName = "";

			if (!Security.IsAdministrator(Security.UserName))
				return "You must be a Mobius administrator to start a new process";

			bool singleInstance = false;
			if (Lex.Contains(args, "SingleInstance"))
			{
				args = Lex.Replace(args, "SingleInstance", "").Trim();
				singleInstance = true;
			}

			Lex lex = new Lex();
			filePath = lex.OpenStringGet(args); 
			filePath = Lex.RemoveAllQuotes(filePath).Trim();
			if (Lex.IsUndefined(filePath)) return "Syntax: StartServerProcess [SingleInstance] <executableFilePath> [<executableParameters>]";
			if (!FileUtil.Exists(filePath)) return "File '" + filePath + "' was not found";
			
			procName = Path.GetFileNameWithoutExtension(filePath);
			args = lex.GetRestOfLine();

			if (singleInstance)
			{
				Process[] processes = Process.GetProcessesByName(procName);
				if (processes.Length > 0)
					return "Process '" + procName + "' is already running";
			}

			Process p = Process.Start(filePath, args);
			return "Process '" + procName + "' has been started";
		}

		/// <summary>
		/// ShowApprovedMobiusUsers for specified group
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns></returns>

		static string ShowApprovedMobiusUsers(string groupName)
		{
			List<string> members = null;
			string adGroupName;

			if (!Lex.Eq(groupName, "ApprovedMobiusNoChemView"))
				members = UAL.ActiveDirectoryDao.GetGroupMembers(groupName);

			else // get users with no struct access
			{
				adGroupName = ServicesIniFile.Read("ApprovedMobiusUsers"); // get all members
				List<string> all = UAL.ActiveDirectoryDao.GetGroupMembers(adGroupName);

				adGroupName = ServicesIniFile.Read("ApprovedMobiusChemView"); // get chemview members
				List<string> chemView = UAL.ActiveDirectoryDao.GetGroupMembers(adGroupName);
				//HashSet<string> chemViewHash = new HashSet<string>(chemView);

				members = new List<string>();
				foreach (string member0 in all) // remove chemview from all
				{
					if (!chemView.Contains(member0))
						members.Add(member0);
				}
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("Members of group: " + groupName + "\r\n");
			sb.Append("===============================================" + "\r\n");

			members.Sort();
			foreach (string s in members)
			{
				sb.Append(s);
				sb.Append("\r\n");
			}

			return sb.ToString();
		}

		/// <summary>
		/// ShowQeStats
		/// </summary>
		/// <returns></returns>

		public static string ShowQeStats()
		{
			List<QueryEngineStats> qes = QueryEngine.SessionStats;
			if (qes == null || qes.Count == 0) return "";
			QueryEngineStats s = QueryEngine.SessionStats[qes.Count - 1];
			string msg =
				" --- Query Execution Stats ---\r\n\r\n" +
				" ExecutionCount: " + s.ExecutionCount + "\r\n" +
				" QueryId:           " + s.QueryId + "\r\n" +
				" Tables:              " + s.Tables + "\r\n" +
				" Columns:           " + s.Columns + "\r\n" +
				" Criteria:             " + s.Criteria + "\r\n\r\n" +
				" PrepareTime:      " + s.PrepareTime / 1000.0 + "\r\n" +
				" ExecuteTime:     " + s.ExecuteTime / 1000.0 + "\r\n" +
				" FetchKeysTime:  " + s.KeyFetchTime / 1000.0 + "\r\n" +
				" KeyCount:          " + s.KeyCount + "\r\n\r\n" +
				" RowRetrievalTime1:   " + s.FirstRowFetchTime / 1000.0 + "\r\n" +
				" RowRetrievalTime:     " + s.TotalRowFetchTime / 1000.0 + "\r\n" +
				" RowRetrievalCount:   " + s.TotalRowCount;

			if (s.TotalRowCount > 0)
			{
				msg += "\r\n" +
				" RowRetrievalTime/Row:  " + s.TotalRowFetchTime / 1000.0 / s.TotalRowCount;
			}

			return msg;
		}

		/// <summary>
		/// ShowDesktopHeap
		/// </summary>
		/// <returns></returns>

		public static string ShowDesktopHeap()
		{

			string exe = @"C:\kktools\dheapmon8.1\x86\dheapmon.exe";
			string dir = @"C:\kktools\dheapmon8.1\x86";
			string summaryFile = @"C:\kktools\dheapmon8.1\x86\summary.txt";
			string detailFile = @"C:\kktools\dheapmon8.1\x86\detail.txt";

			try { File.Delete(summaryFile); }
			catch { };
			try { File.Delete(detailFile); }
			catch { };

			//StreamWriter sw = new StreamWriter(@"C:\kktools\dheapmon8.1\x86\dheapmon.bat");
			//sw.Write(batFile);
			//sw.Close();

			try
			{
				ExecuteWindowsCommand(dir, exe, "-l"); // load
				ExecuteWindowsCommand(dir, exe, "-s -f summary.txt"); // summary
				ExecuteWindowsCommand(dir, exe, "-v -f detail.txt"); // detail

				StreamReader sr = new StreamReader(summaryFile);
				string summary = sr.ReadToEnd();
				sr.Close();

				//sr = new StreamReader(detailFile);
				//string detail = sr.ReadToEnd();
				//sr.Close();

				return summary + "\r\n\r\nDetails: " + detailFile;
			}

			catch (Exception ex)
			{
				return DebugLog.FormatExceptionMessage(ex);
			}
		}

		/// <summary>
		/// ShowOracleClientVersion
		/// </summary>
		/// <returns></returns>

		public static string ShowOracleClientVersion()
		{
			string msg = "Mobius Services Oracle Client Version: ";
			if (DbConnectionMx.OracleClientVersion != null) msg += DbConnectionMx.OracleClientVersion.ToString();
			return msg;
		}

		/// <summary>
		/// ShowSmallWorldDbList
		/// </summary>
		/// <returns></returns>

		public static string ShowSmallWorldDbList(string arg)
		{
			SmallWorldDao swd = new SmallWorldDao();
			string dbList = swd.GetDatabaseList(arg);
			return dbList;
		}

		/// <summary>
		/// Execute command
		/// </summary>
		/// <param name="workingDir"></param>
		/// <param name="fileName"></param>
		/// <param name="args"></param>

		public static void ExecuteWindowsCommand(string workingDir, string fileName, string args)
		{
			ProcessStartInfo psi = new ProcessStartInfo(fileName, args);
			psi.WorkingDirectory = workingDir;
			Process p = Process.Start(psi);
			while (true)
			{
				Thread.Sleep(1000);
				if (p.ExitTime != DateTime.MinValue) break;
			}

			return;
		}

		/// <summary>
		/// Update the Mobius server side TnsNames file from the corporate area
		/// </summary>
		/// <returns></returns>

		static string UpdateTnsNames()
		{
			string msg = "";

			string corpFile = ServicesIniFile.Read("CorpTnsNamesFile");
			if (Lex.IsNullOrEmpty(corpFile)) throw new Exception("CorpTnsNamesFile not defined");
			if (!File.Exists(corpFile)) throw new Exception("CorpTnsNamesFile " + corpFile + " not found");

			string mobFile = ServicesIniFile.Read("MobiusTnsNamesFile");
			if (Lex.IsNullOrEmpty(corpFile)) throw new Exception("MobiusTnsNamesFile not defined");

			try { System.IO.File.Copy(corpFile, mobFile, true); }
			catch (Exception ex)
			{ throw new Exception("Error copying file: " + corpFile + " to " + mobFile + "\n\n" + ex.Message); }

			return "Successfully copied file:\n\n   " + corpFile + "\n\nTo:\n\n   " + mobFile;
		}

		/// <summary>
		/// Build Oracle view to query data 
		/// Example: BuildQueryTableOracleView SF_UNPIV_ASSY_RSLT
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		static string BuildQueryTableOracleView(string args)
		{
			int i1 = args.IndexOf(" ");
			string viewName = args.Substring(0, i1);

			string serializedQuery = args.Substring(i1 + 1);

			Query q = Query.Deserialize(serializedQuery);
			QueryTable qt = q.Tables[0];

			QueryEngine qe = new QueryEngine();
			ExecuteQueryParms eqp = new ExecuteQueryParms(qe, qt);
			eqp.ReturnQNsInFullDetail = false;

			string sql = qe.BuildSqlForSingleTable(eqp);
			sql = Lex.Replace(sql, "/*+ hint */ ", "");

			DbConnectionMx conn = DbConnectionMx.MapSqlToConnection(ref sql, "DEV857", "MBS_OWNER");
			DbCommandMx cmd = new DbCommandMx();
			cmd.MxConn = conn;

			string ddl = "create or replace view " + viewName + " as " + sql;

			cmd.PrepareUsingDefinedConnection(ddl);
			int result = cmd.ExecuteNonReader();

			// Check to see if any cols need recasting of their type

			string fullViewName = "DEV_MBS_OWNER." + viewName; // owner-qualified view name (must use DEV_MBS_OWNER alias)
			List<DbColumnMetadata> cmdList = OracleMx.GetTableMetadataFromOracleDictionary(fullViewName);

			string castCols = "";
			int castCount = 0;
			int sci = -1;
			foreach (QueryColumn qc in qt.QueryColumns)
			{
				if (!qc.Selected) continue;
				sci++; // synch with cmdList

				MetaColumn mc = qc.MetaColumn;
				string expr = mc.Name;

				if (mc.Name == "CORP_SBMSN_ID") mc = mc; // debug

				if (mc.DataType == MetaColumnType.CompoundId) mc = mc; // debug

				if (mc.IsNumeric && (mc.DataType == MetaColumnType.Integer || mc.DataType == MetaColumnType.CompoundId))
				{
					DbColumnMetadata md = cmdList[sci];
					if ((md.data_type == "NUMBER" || md.data_type == "FLOAT") && md.data_scale != 0)
					{
						expr = "cast (" + expr + " as integer) " + expr;  //  integer same as number(22,0)--- " as number(38, 0)) " + expr;
						castCount++;
					}
				}

				if (castCols != "") castCols += ", ";
				castCols += expr;
			}

			if (castCount > 100) // need to recreate the view with casting applied
			{
				sql = "select " + castCols + " from (" + sql + ")"; // build sql with casting

				ddl = "create or replace view " + viewName + " as " + sql;

				cmd = new DbCommandMx();
				cmd.MxConn = conn;

				cmd.PrepareUsingDefinedConnection(ddl);
				result = cmd.ExecuteNonReader();
			}

			ddl = "grant all on " + viewName + " to mbs_user";
			cmd.PrepareUsingDefinedConnection(ddl);
			result = cmd.ExecuteNonReader();


			return "View " + viewName + " created or replaced";
		}

		/// <summary>
		/// Test key connections
		/// </summary>
		/// <returns></returns>

		static string TestKeyOracleConnections()
		{
			string txt = DbConnectionMx.TestKeyConnections();
			return txt;
		}

		/// <summary>
		/// Prepare, Execute and Read the rows for a Sql statement logging performance
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string TestSql(string sql)
		{
			string msg = "";
			int rowCount = 0;
			int lastUpdate = 0;
			long ms = 0;
			double sec = 0;

			//DbCommandMx.LogDbCommandDetail = true;
			DbCommandMx dao = new DbCommandMx();

			Stopwatch sw = Stopwatch.StartNew();
			try
			{
				if (sql.Contains("<list>")) return TestSqlWithList(sql);

				dao.Prepare(sql);
				string msg1 = "Prepare Time (ms): " + sw.ElapsedMilliseconds;
				string msg2 = msg1 + "\r\n" +	"Executing Reader...";
				UAL.Progress.Show(msg2, UmlautMobius.String, true);
				sw.Restart();

				dao.ExecuteReader();
				msg1 += "\r\n" + "Execute Time (ms): " + sw.ElapsedMilliseconds;
				msg2 = msg1 + "\r\n" + "Reading...";
				UAL.Progress.Show(msg2, UmlautMobius.String, false);
				sw.Restart();

				Stopwatch pSw = Stopwatch.StartNew(); // progress stopwatch

				while (true)
				{
					bool readOk = dao.Read();
					if (readOk)	rowCount++;

					if (pSw.Elapsed.TotalSeconds > 1 || !readOk)
					{
						sw.Stop();
						ms = sw.ElapsedMilliseconds;
						sec = ms / 1000.0;
						msg = msg1 + "\r\n" + "Rows: " + rowCount + ", Time(ms): " + ms + ", Rows/Sec: " + (sec > 0 ? (int)(rowCount / sec) : 0);
						UAL.Progress.Show(msg, UmlautMobius.String, false);
						pSw.Restart();
						sw.Start();
					}

					if (!readOk) break;
				}

				dao.CloseReader();
				return msg;
			}

			catch (Exception ex)
			{
				return ex.Message;
			}
		}

/// <summary>
/// Test SQL with list
/// </summary>
/// <param name="sql"></param>
/// <returns></returns>

		public static string TestSqlWithList(string sql)
		{
			string msg = "", msg1 = "";
			int rowCount = 0;
			int lastUpdate = 0;
			long ms = 0, totalMs = 0;
			double sec = 0, totalSec = 0, totalMin = 0;

			//DbCommandMx.LogDbCommandDetail = true;
			DbCommandMx dao = new DbCommandMx();

			try
			{
				string listFileName = @"c:\downloads\TestSql.lst";
				if (!File.Exists(listFileName)) throw new Exception("List file doesn't exist: " + listFileName);
				CidList cidList = CidList.ReadFromFile(listFileName);
				List<string> list = cidList.ToStringList();

				dao.PrepareListReader(sql, DbType.String, KeyListPredTypeEnum.Literal);
				msg1 = "Prepare Time (ms): " + dao.PrepareReaderTime;
				UAL.Progress.Show(msg1, UmlautMobius.String, false);

				dao.ExecuteListReader(list);

				msg1 += "\r\n" + "Execute Time (ms): " + dao.ExecuteReaderTime;
				UAL.Progress.Show(msg1, UmlautMobius.String, true);

				Stopwatch pSw = Stopwatch.StartNew(); // progress stopwatch

				while (true)
				{
					bool readOk = dao.ListRead();

					if (readOk) rowCount++;

					if (pSw.Elapsed.TotalSeconds > 1 || !readOk)
					{
						msg = dao.FormatCommandStats();
						UAL.Progress.Show(msg, UmlautMobius.String, false);
						pSw.Restart();
					}

					if (!readOk) break;
				}

				dao.CloseReader();
				return msg;
			}

			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		/// <summary>
		/// Update the key connections message
		/// </summary>
		/// <returns></returns>

		static string UpdateKeyOracleConnectionsMessage()
		{
			string msg = DbConnectionMx.TestKeyConnections();
			msg = DateTime.Now.ToString() + "," + msg;
			try // update parameter
			{
				UserObjectDao.SetUserParameter("Mobius", "KeyOracleConnectionsMessage", msg);
				return msg;
			}
			catch (Exception ex) // occasionally fails if other user is updating at same time
			{
				msg = "UpdateKeyOracleConnectionsMessage Error: " + ex.Message;
				DebugLog.Message(msg);
				return msg;
			}
		}

		static string UpdateMetaTableStatistics(string brokerName)
		{
			int total = 0;
			if (!String.IsNullOrEmpty(brokerName))
			{
				for (int i = 0; i < MetaTableFactory.MetaFactories.Count; i++)
				{
					MetaTableFactoryRef mtfr = MetaTableFactory.MetaFactories[i];

					if (Lex.Ne(mtfr.Name, brokerName))
					{
						continue; // skip if name supplied and this isn't it
					}
					int updateCount = mtfr.MetaTableFactory.UpdateMetaTableStatistics();
					total += updateCount;
				}
				MetaTreeFactory.DeleteCacheFile(); // delete cache so rebuilt with new stats

				return "Updated metatable statistics for " + total + " metatable" + ((total == 1) ? "" : "s");
			}
			else
			{
				return "No updates made";
			}
		}

		/// <summary>
		/// Build a table that contains one row for each child in each folder in the MetaTree
		/// </summary>

		public static string UpdateMetatreeNodeTable()
		{
			int nodeCnt=0, insCnt = 0;

			MetaTreeNode dht = MetaTreeFactory.GetNode("DHT_view");
			if (dht == null) return "Node DHT_view not found";
			List<MetaTreeNode> mtnList = MetaTreeFactory.GetSubnodes(dht);

			Mobius.UAL.Progress.Show("Deleting existing data...");
			DbCommandMx dao = new DbCommandMx();
			string sql = "delete from " + MetaTreeTableDao.MetatreeNodesTableName;

			dao.Prepare(sql);
			dao.BeginTransaction();
			int delCnt = dao.ExecuteNonReader();

			Mobius.UAL.Progress.Show("Inserting new data...");

			int t0 = TimeOfDay.Milliseconds();


			foreach (MetaTreeNode n in mtnList)
			{
				if (!n.IsFolderType) continue;

				insCnt += MetaTreeTableDao.InsertMetatreeNodeRows(n, dao);
				nodeCnt++;

				if (TimeOfDay.Milliseconds() - t0 > 1000)
				{
					Mobius.UAL.Progress.Show("Nodes processed: " + nodeCnt + ", Rows inserted: " + insCnt + "...");
					t0 = TimeOfDay.Milliseconds();
				}
			}

			dao.Commit();
			dao.Dispose();

			return "Nodes processed: " + nodeCnt + ", Rows inserted: " + insCnt;
		}


		public static string UpdateMetaTreeCache()
		{
			MetaTreeFactory.Build(false);
			return "MetaTree cache updated";
		}

		static string UpdateAssayMetadata(string arg)
		{
			AssayMetaFactory nmf = new AssayMetaFactory();
			nmf.UpdatePrecomputedAssayMetaData(arg);
			nmf.UpdateMetaTableStatistics(arg);
			MetaTreeFactory.DeleteCacheFile(); // delete cache so rebuilt with new stats
			return "Assay Metadata updated ";
		}

		/// <summary>
		/// Test new code
		/// </summary>
		/// <returns></returns>

		static string TestExperimentalCode()
		{

// Scan SmallWorld unmapped compounds
			{
				return SmallWorldDao.CheckUnmappedCompoundsSearchSubmit();
			}


			// Mobius.CDK.CircularFP.Build();


			//CompoundIdUtil.ReadChangedCorpIdSet();
			//return "";

			//string xml = MetaTreeFactory.GetMetatreeXml("CORP_DATABASE", true, false);

			////StringBuilder sb = new StringBuilder(); // get list of nodes in base tree not found (debug)
			////foreach (MetaTreeNode mtn0 in Nodes.Values)
			////{
			////  if (!mtn0.IsFolderType) continue;
			////  string name = mtn0.Name.ToUpper();
			////  if (!nodesDoneHash.Contains(name))
			////    sb.Append(mtn0.Name).Append("\r\n");
			////}


			//int nodeCnt = Lex.CountSubstring(xml, "</Node>");
			//int queryCnt = Lex.CountSubstring(xml, "\"Query\"");

			//byte[] ba = GZip.Compress(xml); // test compress
			////string xml2 = ZipMx.Decompress(ba);
			////if (xml != xml2) xml = xml; // be sure xml is the same

			//string msg = "Nodes: " + nodeCnt + ", User Queries: " + queryCnt + ", Xml Length: " + xml.Length + ", Compressed byte[] length: " + ba.Length;
			//return msg;
		}

	}
}
