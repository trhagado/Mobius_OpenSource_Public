using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;
using System.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Net.Mail;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// New Data Alert class
	/// This class supports the saving of alerts associated with queries,
	/// nightly checking of alerts to see if new data is available and
	/// the display of new data for the query.
	/// 
	/// When a query is saved that has alerting turned on the Alert.Save method
	/// is called which saves an Alert user object containing the members of 
	/// this class. The saved alert is named Alert_query-id and is stored
	/// in no folder (the null-string folder -- FolderType = unknown).
	/// 
	/// AlertUtil.Check is called, usually during a nightly run to check for new data.
	/// Each query is run and checked for new data. New data may be new compounds
	/// or new rows for an existing compound. When new data is found the alert 
	/// is updated with the list of compound ids and the number of records associated
	/// with each number. A list object is written containing the new/changed numbers
	/// and an E-mail is sent to the user(s) containing a command to display 
	/// (also to delete) the data. The list object contains the id of the associated
	/// query and the date that the new data was found.
	/// 
	/// AlertUtil.ViewAlertData is called with the id of the new data list. It reads the
	/// associated query, modifies it to use the new data list and then runs it.
	/// </summary>

	public static class AlertUtil
	{
		private const string AlertLogFileName = "AlertLog - [Date].log";
		static ServicesLogFile _log;

		private const string AlertErrorLogFileName = "AlertErrorLog - [Date].log";
		static ServicesLogFile _errorLog;

		static Alert AlertExecuting; // current alert executing
		static Query QueryExecuting;
		static int AlertCpuTimeout = 30; // CPU time timeout in minutes

		/// <summary>
		/// Display the new data for an alert 
		/// </summary>
		/// <param name="listIdString"></param>
		/// <returns></returns>
		public static string ViewAlertData(
				string listIdString)
		{
			int listId;

			if (!int.TryParse(listIdString, out listId))
				return "Invalid alert results list id: " + listIdString;

			UserObject uo = UserObjectDao.ReadHeader(listId);
			if (uo == null) return "Alert alert results list id not found: " + listIdString;

			if (uo.Type != UserObjectType.CnList || uo.Description == null ||
			 uo.Description.Split('\t').Length < 2) // need at least
				return "Alert alert results list object is not valid: " + listIdString;

			string[] sa = uo.Description.Split('\t');
			int queryId = Int32.Parse(sa[0]);
			UserObject quo = UserObjectDao.Read(queryId);
			if (quo == null)
				return "Query " + queryId + " could not be retrieved.\n" + CommandExec.GetUserObjectReadAccessErrorMessage(queryId, "query");

			Query q = Query.Deserialize(quo.Content);
			q.UserObject = quo; // copy user object to get current name, etc.
			quo.Name += " - Alert results for " + sa[1]; // alert.LastNewData.ToShortDateString(); // append alert date to query name
			quo.Id = 0; // different query, clear id

			// Run query using list

			if (q.LogicType != QueryLogicType.Complex) // simple logic
			{
				q.KeyCriteria = " IN LIST " + Lex.AddSingleQuotes(uo.InternalName);
				q.KeyCriteriaDisplay = "In list: " + uo.Name;
			}

			else // complex logic
			{
				List<MqlToken> critToks = MqlUtil.ParseComplexCriteria(q.ComplexCriteria, q);
				critToks = // remove any key criteria
						MqlUtil.DisableCriteria(critToks, q, null, null, null, null, true, true, false);

				string criteria = MqlUtil.CatenateCriteriaTokens(critToks);
				q.ComplexCriteria = q.Tables[0].Alias + "." + q.Tables[0].MetaTable.KeyMetaColumn.Name + " in list cnlist_" + uo.Id;
				if (criteria.Trim() != "") q.ComplexCriteria += " and (" + criteria + ")";
			}

			q.ResultKeysListName = uo.InternalName; // name of key list to use to avoid search step
			q.UseResultKeys = true; // use the keys in the list

			QbUtil.AddQueryAndRender(q, false);

			string nextCommand = QueryExec.RunQuery(q, SS.I.DefaultQueryDest);
			return nextCommand;
		}

		/// <summary>
		/// View background query results
		/// </summary>
		/// <param name="queryIdString"></param>
		/// <returns></returns>
		public static string ViewBackgroundQueryResults(
				string queryIdString)
		{
			int uoId, queryId;

			if (!int.TryParse(queryIdString, out uoId))
				return "Background query results object id is not valid + (" + queryIdString + ")";

			UserObject uo = UserObjectDao.ReadHeader(uoId);
			if (uo == null) return CommandExec.GetUserObjectReadAccessErrorMessage(uoId, "query id");

			if (uo.Type == UserObjectType.Query)
				queryId = uo.Id;

			else if (uo.Type == UserObjectType.CnList && uo.Description != null &&
			 uo.Description.Split('\t').Length >= 2)
			{
				string[] sa = uo.Description.Split('\t');
				queryId = Int32.Parse(sa[0]);
			}

			else return "Background query results object is not valid + (" + queryIdString + ")";

			UserObject quo = UserObjectDao.Read(queryId);
			if (quo == null) return "Query " + queryId + " could not be retrieved.\n" + CommandExec.GetUserObjectReadAccessErrorMessage(uoId, "query");

			Query q = Query.Deserialize(quo.Content);
			q.UserObject = quo; // copy user object to get current name, etc.
			quo.Name += " - Background Query Results"; //  for " + sa[1]; // alert.LastNewData.ToShortDateString(); // append date to query name

			if (Lex.IsNullOrEmpty(q.ResultsDataTableFileName))
				q.ResultsDataTableFileName = "Query_" + queryId + "_Results.bin"; // file containing results, references original query
			q.BrowseSavedResultsUponOpen = true; // cause data to be browsed
			QbUtil.AddQueryAndRender(q, true);
			return "";
		}

		/// <summary>
		/// Cancel an alert
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>

		public static string DeleteAlert(
				string arg)
		{
			int alertId;

			try { alertId = Int32.Parse(arg); }
			catch (Exception) { return "Invalid alert Id: " + arg; }

			bool deleted = UserObjectDao.Delete(alertId); // delete the alert
			if (deleted) return "Alert Deleted";
			return "Alert not found: " + arg;
		}

		/// <summary>
		/// Delete inactive alerts
		/// </summary>
		/// <param name="arg">Delete if time since last new data is greater than this in days</param>
		/// <returns></returns>

		public static string DeleteInactiveAlerts(
				string arg)
		{
			int maxDays;
			int.TryParse(arg, out maxDays);
			if (maxDays <= 0) maxDays = 365; // default to one year

			List<UserObject> alerts = // get the alerts
					UserObjectDao.ReadMultiple(UserObjectType.Alert, false);

			List<int> oldList = new List<int>();
			List<int> newList = new List<int>();
			List<int> badList = new List<int>();

			foreach (UserObject auo in alerts)
			{
				Alert a;
				try { a = Alert.GetAlertFromUserObject(auo, true); }
				catch (Exception)
				{
					badList.Add(auo.Id);
					continue;
				}

				TimeSpan elapsed = DateTime.Now.Subtract(a.LastNewData);

				if (elapsed.TotalDays < maxDays || a.LastNewData.Equals(DateTime.MinValue))
					newList.Add(a.Id);

				else oldList.Add(a.Id);
			}

			string msg =
					"New alerts: " + newList.Count + ", Old alerts: " + oldList.Count + ", Bad alerts: " + badList.Count + "\r\n\r\n" +
					"Do you want to delete the old alerts that are older than " + maxDays + " days old?";
			DialogResult dr = MessageBoxMx.Show(msg, "Delete Inactive Alerts", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (dr != DialogResult.Yes) return "No alerts deleted";

			int delCount = 0;
			foreach (int alertId in oldList)
			{
				bool deleted = UserObjectDao.Delete(alertId); // delete the alert
				if (deleted) delCount++;
			}

			return "Alerts deleted: " + delCount;
		}

		/// <summary>
		/// Force an alert check to run
		/// Example: check alert 269246 forceEmail checkOnly
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string CheckSingle(
				string args)
		{
			string alertIdString, msg = "";
			int alertId;
			bool checkOnly = false;
			bool forceEmail = false;

			if (String.IsNullOrEmpty(args))
				return "Syntax: Check Alert <alertId> [forceEmail] [checkOnly]";

			string[] sa = args.Split(' ');

			try
			{
				alertIdString = sa[0];
				alertId = Int32.Parse(alertIdString);
			}
			catch (Exception)
			{ return "Syntax: Check Alert <alertId>"; }

			if (args.Trim().Length > 0)
			{
				if (Lex.Contains(args, "forceEmail")) forceEmail = true;
				if (Lex.Contains(args, "checkOnly")) checkOnly = true;
			}

			UserObject uo = UserObjectDao.Read(alertId); // get the content
			if (uo == null) return "Alert not found: " + alertId;
			if (uo.Type != UserObjectType.Alert) return "Object is not an alert: " + alertId;

			if (SS.I.Attended)
				Progress.Show("Checking alert " + alertIdString);

			try
			{
				msg = Check(uo, true, checkOnly, forceEmail); // do the check
			}

			catch (Exception ex)
			{
				msg = "Error executing Alert (manual): " + uo.Id;

				if (Lex.Contains(ex.Message, "ORA-01012") || Lex.Contains(ex.Message, "ORA-00028"))
					msg += ", Error: " + ex.Message; // keep simple if forced logoff

				else // include stack trace otherwise
					msg += "Error: " + DebugLog.FormatExceptionMessage(ex, msg);

				AlertUtil.LogAlertMessage(msg);
				AlertUtil.LogAlertErrorMessage(msg);
				return msg;
			}

			string msg0 =  // message prefix
				"Completed Alert (manual): " + uo.Id;

			msg = msg0 + msg;
			AlertUtil.LogAlertMessage(msg);

			if (Lex.Contains(msg, "Error executing alert")) // keep separate log of errors
				AlertUtil.LogAlertErrorMessage(msg);

			//if (Security.IsDeveloper) while(DebugMx.True) // debug - loop on Check calls to monitor memory usage, etc.
			//		msg = Check(uo, true, checkOnly, forceEmail);

			Progress.Hide();
			return msg;
		}

		/// <summary>
		/// Queue up alerts, start separate process & monitor progress
		/// This is the preferred method for nightly alert processing since this
		/// method avoids bottlenecks caused by slow running alerts. It starts a
		/// separate CheckQueuedAlerts process and then monitors progress. If no
		/// additional alerts are processsed within a specified time then a new
		/// process is started up so that alert processing can continue.
		/// CommandLine command to start processing in background:
		///    StartBackgroundSession Unattended UserName = MOBIUS Command = 'run script E:\Mobius\MobiusServerData\Script\CheckAlerts.mob'
		/// </summary>
		/// <returns></returns>

		public static string CheckAll(
				string args)
		{
			//foreach (Process clsProcess in Process.GetProcesses())
			//{
			//    if (clsProcess.ProcessName.ToLower().Contains("mobius"))
			//    {
			//        LogAlertMessage("==================================================================================");
			//        LogAlertMessage("== Alerts are already running. Only one Alert Queue can be processed at a time. ==");
			//        LogAlertMessage("== Process " + clsProcess.ProcessName + " is running. ==");
			//        LogAlertMessage("==================================================================================");
			//        Application.Exit();
			//    }
			//}

			Process[] processes = Process.GetProcessesByName("MobiusClient.exe");
			if (processes.Length > 1)
			{
				LogAlertMessage("==================================================================================");
				LogAlertMessage("== Alerts are already running. Only one Alert Queue can be processed at a time. ==");
				LogAlertMessage("== Process " + processes[0].ProcessName + " is running.                                         ==");
				LogAlertMessage("==================================================================================");
				Application.Exit();
			}

			int maxAlertProcessors = ServicesIniFile.ReadInt("MaxAlertProcessors", 16);
			//_log = new ServicesLogFile(AlertLogFileName);
			//_errorLog = new ServicesLogFile(AlertErrorLogFileName);

			AlertQueue alertQueue = new AlertQueue(true);

			LogAlertMessage("======================================================================");
			LogAlertMessage("========= Checking Alerts " + DateTime.Now + ", Count = " + alertQueue.StartingAlertCount + ", Max processors = " + maxAlertProcessors + " =========");
			LogAlertMessage("======================================================================");



			Progress.Show("Queueing alerts...");

			alertQueue.StartQueueProcessor(args); // start first queue processor

			string msg;
			int progressTime = TimeOfDay.Milliseconds();
			string lastMsg = "";

			while (true)
			{
				int processedAlertCount = alertQueue.GetProcessedAlertCount();

				msg = "Processed " + (processedAlertCount) +
							" of " + alertQueue.StartingAlertCount + " alerts ";
				if (msg != lastMsg) // show/log message if changed
				{
					Progress.Show(msg);
					lastMsg = msg;
				}

				Thread.Sleep(10000); // sleep 10 secs

				int remainingAlertCount = alertQueue.GetRemainingingAlertCount();
				if (remainingAlertCount == 0)
				{
					LogAlertMessage("Alert Queue is empty");
					break; // done 
				}

				int now = TimeOfDay.Milliseconds(); // see if time to check for progress
				if (now - progressTime > 5 * 60 * 1000) // 5 minutes elapsed
				{
					progressTime = now;

					if ((processedAlertCount >= alertQueue.NumberOfCurrentProcessors && alertQueue.NumberOfCurrentProcessors >= 10) || // did we finish on average one query per processor? Start 10 right away.
							alertQueue.NumberOfCurrentProcessors >= maxAlertProcessors) // limit to max number
					{ // just continue if so
						LogAlertMessage("alertsProcessed: " + processedAlertCount + " processors: " + alertQueue.NumberOfCurrentProcessors);
						continue;
					}

					LogAlertMessage("alertsProcessed: " + processedAlertCount + " processors: " + alertQueue.NumberOfCurrentProcessors);

					alertQueue.StartQueueProcessor(args); // start additional queue processor
				}
			}

			// Finish up

			int errorCount = 0;
			if (File.Exists(AlertErrorLogFileName))
			{
				StreamReader sr = new StreamReader(AlertErrorLogFileName);
				StringBuilder sb = new StringBuilder();
				while (true)
				{
					string rec = sr.ReadLine();
					if (rec == null) break;
					if (rec.IndexOf("Error executing alert", StringComparison.OrdinalIgnoreCase) >= 0)
						errorCount++;
					sb.Append(rec);
					sb.Append("\r\n");
				}
				sr.Close();

				string mailTo = ServicesIniFile.Read("AlertErrorEmailAddress");
				if (errorCount > 0 && !String.IsNullOrEmpty(mailTo))
				{ // email errors if requested
					MailMessage mail = new MailMessage();
					mail.From = new MailAddress("Mobius@[server]", UmlautMobius.String);
					mail.Subject = "Mobius Alert Check Errors: " + errorCount;
					mail.To.Add(mailTo);
					mail.Body = mail.Subject + "\r\n\r\n" + sb;
					Email.Send(mail);
				}
			}

			msg = "Check Alerts queue empty, Alerts checked: " + alertQueue.StartingAlertCount + ", Errors: " + errorCount;
			LogAlertMessage("");
			LogAlertMessage(msg);
			Progress.Hide();
			return msg;
		}

		public static string CheckQueuedAlerts(
				string args)
		{
			return AlertQueue.CheckQueuedAlerts(args);
		}

		/// <summary>
		/// Scan alerts and run those that are scheduled (old style)
		/// </summary>
		/// <returns></returns>

		//public static string CheckAllOld(
		//    string args)
		//{
		//    string msg;
		//    //_log = new ServicesLogFile(AlertLogFileName);
		//    //_errorLog = new ServicesLogFile(AlertErrorLogFileName);

		//    bool forceCheck = false; // force check
		//    bool checkOnly = false; // only do check, no update or email
		//    bool forceEmail = false; // if true send email even if no changes
		//    int firstToCheck = 0;
		//    if (args != null && args.Trim().Length > 0)
		//    {
		//        if (Lex.Contains(args, "forceCheck")) forceCheck = true;
		//        if (Lex.Contains(args, "checkOnly")) checkOnly = true;

		//        try { firstToCheck = Int32.Parse(args); } // see if object id of first alert to run
		//        catch (Exception) { }
		//    }

		//    List<UserObject> alerts = UserObjectDao.ReadMultiple(UserObjectType.Alert, false);

		//    msg = "========= Checking Alerts " + DateTime.Now + ", Count = " + alerts.Count + " =========";
		//    LogAlertMessage("");
		//    LogAlertMessage(msg);
		//    Progress.Show(msg);

		//    int newDataCount = 0;
		//    int processCount = 0;

		//    foreach (UserObject uo in alerts)
		//    {
		//        //				force = true; // debug
		//        //				if (uo.Id != 42819) continue; // debug

		//        if (uo.Id < firstToCheck) continue; // skip?

		//        UserObject uo2 = UserObjectDao.Read(uo.Id); // get the content

		//        int queryId = 0;
		//        int i1 = uo.Name.IndexOf("_");
		//        if (i1 > 0)
		//        {
		//            try { queryId = Int32.Parse(uo.Name.Substring(i1 + 1)); }
		//            catch (Exception) { }
		//        }

		//        msg = "Processing Alert " + (processCount + 1) + ": " + uo.Id +
		//            ", QueryId: " + queryId;
		//        LogAlertMessage(msg);
		//        Progress.Show(msg);

		//        msg = Check(uo2, forceCheck, checkOnly, forceEmail); // do the check
		//        LogAlertMessage(msg);
		//        if (msg.Contains("Changed compounds")) newDataCount++;
		//        processCount++;
		//    }

		//    msg = "Alerts checked: " + alerts.Count +
		//        ", Alert E-mails: " + newDataCount;

		//    LogAlertMessage("");
		//    LogAlertMessage(msg);
		//    Progress.Hide();
		//    return msg;
		//}

		/// <summary>
		/// Check a single alert writing out a CidList user object containing the keys with any new data.
		/// </summary>
		/// <param name="alertUserObject">UserObject containing serialized alert</param>
		/// <param name="forceCheck">If true force check even if not time yet</param>
		/// <param name="checkOnly">If true then just check, don't send e-mail or update alert user object</param>
		/// <param name="forceEmail"></param>
		/// <returns></returns>

		public static string Check(
				UserObject alertUserObject,
				bool forceCheck,
				bool checkOnly,
				bool forceEmail)
		{
			QueryManager qm;
			DataTableManager dtm;
			DataTableMx dt;
			QueryTable qt;
			Query q0;
			int totalRows = 0, newRows, qti;
			string response;

			Alert alert = Alert.GetAlertFromUserObject(alertUserObject, true);

			//if (alert.ChangedCompounds == -1) alert.ChangedCompounds = 0;
			//if (alert.TotalCompounds == -1) alert.TotalCompounds = 0;
			//if (alert.NewCompounds == -1) alert.NewCompounds = 0;
			//if (alert.NewRows == -1) alert.NewRows = 0;
			if (alert.TotalRows == -1) alert.TotalRows = 0;

			if (Lex.IsNullOrEmpty(alert.MailTo)) alert.MailTo = Security.GetUserEmailAddress(alert.Owner); // mail to alert owner if no mailto list

			UserObject queryUserObject = UserObjectDao.Read(alert.QueryObjId); // read the query
			if (queryUserObject == null) // query no longer there
			{
				LogAlertMessage("Debug Alerts: Could not find query id: " + alert.QueryObjId);
				UserObjectDao.Delete(alertUserObject.Id);
				return "Deleting Alert_" + alert.QueryObjId + ", no associated query";
			}

			if (!forceCheck)
			{
				string message;
				//message = CheckForDelay(alert, queryUserObject);

				if (alert.Pattern == Alert.PatternEnum.Daily.ToString())
				{
					message = CheckDaily(alert, queryUserObject);
					if (!string.IsNullOrEmpty(message)) return message;
				}

				else if (alert.Pattern == Alert.PatternEnum.Weekly.ToString())
				{
					message = CheckWeekly(alert, queryUserObject);
					if (!string.IsNullOrEmpty(message)) return message;
				}

				else if (alert.Pattern == Alert.PatternEnum.Monthly.ToString())
				{
					message = CheckMonthly(alert, queryUserObject);
					if (!string.IsNullOrEmpty(message)) return message;
				}
			}

			Query query = Query.Deserialize(queryUserObject.Content);
			query.UserObject = queryUserObject; // copy query user object to get current name, etc.
			query.IncludeRootTableAsNeeded();

			int qGetCriteriaCount = query.GetCriteriaCount();

			if (query.LogicType != QueryLogicType.Complex && qGetCriteriaCount == 0)
			{

				LogAlertMessage("Debug Alerts: Query Name: " + query.Name + "Query ID: " + query.InstanceId + "q.LogicType: " + query.LogicType);
				LogAlertMessage("Debug Alerts: Query Name: " + query.Name + "Query ID: " + query.InstanceId + "q.GetCriteriaCount(): " + qGetCriteriaCount);
				UserObjectDao.Delete(alertUserObject.Id);
				return "Deleting Alert_" + alert.QueryObjId + ", no criteria for query";
			}

			//LogAlertMessage("Debug: auo.Content.Length: " + auo.Content.Length.ToString());
			//LogAlertMessage("Debug: a.QueryObjId: " + a.QueryObjId.ToString());
			//LogAlertMessage("Debug: a.LastResults.Count: " + a.LastResults.Count.ToString());
			//LogAlertMessage("Debug: quo.Content.Length: " + quo.Content.Length.ToString());
			//LogAlertMessage("Debug: q.UserObject.Id: " + q.UserObject.Id.ToString());
			//LogAlertMessage("Debug: q.UserObject.Name: " + q.UserObject.Name);
			//LogAlertMessage("Debug: q.Tables.Count: " + q.Tables.Count.ToString());

			DateTime startTime = DateTime.Now;

			// Get list of tables with criteria & count only their data (todo: make this optional later?)

			HashSet<string> checkTable = new HashSet<string>();
			for (qti = 0; qti < query.Tables.Count; qti++)
			{
				if (alert.CheckTablesWithCriteriaOnly)
				{
					string mtName = query.Tables[qti].MetaTable.Name;
					if (qti == 0 && query.KeyCriteria != "" && !checkTable.Contains(mtName)) checkTable.Add(mtName); // apply key criteria to first table
					qt = query.Tables[qti];
					int qci;
					for (qci = 0; qci < qt.QueryColumns.Count; qci++)
					{
						QueryColumn qc = qt.QueryColumns[qci];
						if (qc.MetaColumn == null) continue;
						if (qc.IsKey) continue;
						if (qc.Criteria != "" && !qc.MetaColumn.IsDatabaseSetColumn)
						{
							if (!checkTable.Contains(mtName)) checkTable.Add(mtName);
							if (!qc.Selected) qc.Selected = true; // select col with criteria to avoid getting a suppressed null row when no cols selected other than key
						}
					}
				}
			}

			/////////////////////////////////////////////////////////////////////////////
			// Run the query counting number of result rows for each table for each key
			/////////////////////////////////////////////////////////////////////////////

			string timeOut = ServicesIniFile.Read("AlertTimeoutInMinutes"); // set any timeout
			try { query.Timeout = Int32.Parse(timeOut) * 60; } // set any timeout in seconds
			catch (Exception) { }

			AlertExecuting = alert;
			QueryExecuting = query;
			AlertCpuTimeout = ServicesIniFile.ReadInt("AlertCPUTimeoutInMinutes", 30); // CPU time timeout checked here

			ThreadStart ts = new ThreadStart(MonitorAlertCpuTime);
			Thread monitorThread = new Thread(ts);
			monitorThread.IsBackground = true;
			monitorThread.SetApartmentState(ApartmentState.STA);
			monitorThread.Start();

			Dictionary<string, int> results = new Dictionary<string, int>();

			try
			{
				QueryEngine qe = new QueryEngine();
				Query nq;
				List<string> resultKeys = qe.TransformAndExecuteQuery(query, out nq);

				if (nq != null) query = nq; // if modified then replace original query and QE with the new ones

				qm = new QueryManager();
				OutputDest outputDest = OutputDest.TextFile;
				QueryExec.InitializeQueryManager(qm, query, outputDest, qe, resultKeys);
				dtm = qm.DataTableManager;

				int keys = 0;
				while (true)
				{
					object[] row = qe.NextRow();
					if (row == null) break;
					totalRows++;

					if (Progress.IsTimeToUpdate && SS.I.Attended)
						Progress.Show("Checking alert " + alertUserObject.Id + "\n" +
								keys + " keys, " + totalRows + " rows");

					Application.DoEvents();

					string key = (string)row[0];

					for (qti = 0; qti < query.Tables.Count; qti++)
					{ // count tables with new data
						qt = query.Tables[qti];
						if (alert.CheckTablesWithCriteriaOnly && !checkTable.Contains(qt.MetaTable.Name)) continue; // skip if not checking
						int voPos = qt.VoPosition - dtm.KeyValueVoPos;
						//if (voPos >= row.Length) voPos = voPos; // debug
						if (!NullValue.IsNull(row[voPos]))
						{
							if (!results.ContainsKey(key))
							{
								results[key] = 1; // first table with results for key
								keys++;
							}
							else
							{
								results[key] = results[key] + 1; // additional table with results for key
							}
						}
					}
				}
				newRows = totalRows - alert.TotalRows;

				qe.Close();
			}

			catch (Exception ex)
			{
				if (Lex.Eq(ex.Message, "DataReaderDao TimeOut"))
				{ // if timeout then remove alert & send message to user
					UserObjectDao.Delete(alertUserObject.Id);

					MailMessage mail = new MailMessage();
					mail.From = new MailAddress("Mobius@[server]", UmlautMobius.String);
					mail.Subject = "Mobius alert for query " + Lex.Dq(query.UserObject.Name);
					Email.SetMultipleMailTo(mail, alert.MailTo);
					mail.Body =
							"The execution time of the alert on query \"" + query.UserObject.Name +
							"\" has exceeded the " + timeOut + " minute time limit and as a result the alert has been disabled. " +
							"You may want to modify the query so that executes more quickly, " +
							"if possible, and then reenable the alert.";
					Email.Send(mail);
				}

				Progress.Hide();

				string msg = "Error executing alert: " + alert.Id + ", query: " + queryUserObject.Id + ", " +
						queryUserObject.InternalName;

				if (ex is UserQueryException || // if recognized query error don't include stack trace
				 Lex.Contains(ex.Message, "ORA-01012") || Lex.Contains(ex.Message, "ORA-00028")) // or forced logoff
					msg += "Error: " + ex.Message;

				else msg += ",\r\n" + // unrecognized error, include stack trace
						DebugLog.FormatExceptionMessage(ex);

				ClientLog.Message(msg);
				return msg;
			}

			finally
			{
				AlertExecuting = null; // signal done
				QueryExecuting = null;
			}

			Progress.Show(
					"Checking alert " + alertUserObject.Id + "\n" +
					"Comparing results...");

			/////////////////////////////////////////////
			// Compare these results to previous results
			/////////////////////////////////////////////

			Dictionary<string, object> changedKeys = new Dictionary<string, object>();
			int newKeyCount = 0;
			//int rowDelta = 0;
			int changedKeyCount = 0;

			foreach (string key in results.Keys)
			{
				int count = results[key];

				if (!alert.LastResults.ContainsKey(key)) // new key?
				{
					changedKeys[key] = null;
					newKeyCount++;
					//rowDelta += count;
				}

				else
				{
					int lastCount = alert.LastResults[key];
					if (count > lastCount) // more rows for key? (Count is not Rows, it is tables, so rowDelta is really the table count, may or may not equate to rows.)
					{
						changedKeys[key] = null;
						changedKeyCount++;
						//rowDelta += count - lastCount;
						//keyDelta++;
					}
				}
			}

			/////////////////////////////////////////////////////////////////////////////
			// If any change then save the change info & send e-mail if not initial check
			/////////////////////////////////////////////////////////////////////////////

			bool exportParametersAreValid =
					alert.ExportParms != null && alert.ExportParms.OutputDestination != OutputDest.Unknown;

			if (alert.LastCheck == DateTime.MinValue || // first time
					newKeyCount > 0 || changedKeyCount > 0 || newRows > 0 || forceEmail) // some change
			{
				if (alert.LastCheck == DateTime.MinValue) // if first time then just save a record of the state of the data
				{
					if (exportParametersAreValid) // if exporting complete results, do it the first time
						ExportCompleteResultsForAlert(alert, query, null, null);

					response = "First check for alert, initializing results";
				}

				else if (!checkOnly) try
					{
						string html = BackgroundQuery.ReadTemplateFile("MobiusAlertEmailTemplate.htm");

						List<string> cidList = new List<string>(changedKeys.Keys);
						dtm.ResultsKeys = cidList; // pass list to write
																			 //cidList.Add("00012345"); // debug
																			 //UserObject cidListUo = new UserObject(); // debug

						UserObject cidListUo = BackgroundQuery.SaveBackgroundQueryResultsReferenceObject(qm, "AlertMonitor", null);

						if (exportParametersAreValid)
							html = ExportCompleteResultsForAlert(alert, query, cidListUo, html); // write as other format

						string subject = "Mobius alert for query " + Lex.Dq(query.UserObject.Name);
						string viewCmd = "View Alert Data";

						MailResultsAvailableMessage( // send the mail
								query,
								cidList.Count,
								alert.MailTo,
								subject,
								viewCmd,
								cidListUo.Id,
								alertUserObject,
								html);
					}
					catch (Exception ex)
					{ // may occur routinely for invalid e-mail addresses
						Progress.Hide();
						DebugLog.Message("Error mailing notice for alert: " + alert.Id + ", query: " + alert.QueryObjId + ", Error: " + DebugLog.FormatExceptionMessage(ex));
						return "Error mailing notice: " + ex.Message;
					}

				response = "Changed compounds = " + changedKeyCount +
						", new compounds = " + newKeyCount +
						", new data rows = " + (newRows); //+ rowDelta;


				//alert.NewCompounds = keyDelta; // store stats for alert
				//alert.ChangedCompounds = changedKeys.Count;
				//alert.TotalCompounds = results.Count;
				//alert.NewRows = newRows; 
				//alert.TotalRows = totalRows; // totalRows;
				alert.LastResults = results; // copy new results
				alert.LastNewData = DateTime.Now;

			}

			else
			{
				//if (Security.IsDeveloper && checkOnly) alert.RunImmediately = true; // debug

				if (exportParametersAreValid && alert.RunImmediately) // immediate export?
					ExportCompleteResultsForAlert(alert, query, null, null);

				response = "No change";
			}

			// Update the Alert user object

			if (!checkOnly)
			{
				alert.NewCompounds = newKeyCount; // store stats for alert
				alert.ChangedCompounds = changedKeyCount; //changedKeys.Count;
				alert.TotalCompounds = results.Count;
				alert.NewRows = newRows;
				alert.TotalRows = totalRows; // totalRows;

				alert.LastCheck = DateTime.Now;
				alert.RunImmediately = false; // reset immediate flag
				alert.LastCheckExecutionTime = (int)DateTime.Now.Subtract(startTime).TotalSeconds; // execution time
				alert.Write();
			}

			TimeSpan timeSinceLastCheck = DateTime.Now.Subtract(startTime);
			string txt = String.Format("{0:F2}", timeSinceLastCheck.TotalMinutes);
			response = "(" + txt + " min.) " + response + " (Query: " + queryUserObject.InternalName + ")";

			Progress.Hide();
			return response;
		}

		private static string CheckForDelay(Alert alert, UserObject queryUserObject)
		{
			string message = "";
			DateTime lastCall = Convert.ToDateTime("8:00:00 PM");
			if (alert.StartTime.TimeOfDay > DateTime.Now.TimeOfDay && DateTime.Now < lastCall)
			{
				message = "Delayed Alert: " + alert.Id + ", Run after " + alert.StartTime.ToShortTimeString() +
						". (Query: " + queryUserObject.InternalName + ")";
			}
			return message;
		}

		/// <summary>
		/// Monitor alert CPU time and kill process if timeout exceeded
		/// </summary>

		private static string CheckDaily(Alert alert, UserObject queryUserObject)
		{
			int checkInterval = alert.Interval;
			string msg = "";

			// Expand interval if relatively long time since last data found (if allowed)
			bool intervalExpanded = false;
			bool allowExtraDelayForSlowlyChangingData = ServicesIniFile.ReadBool("AllowExtraAlertDelayForSlowlyChangingData", true);
			if (allowExtraDelayForSlowlyChangingData && !alert.LastCheck.Equals(DateTime.MinValue) && alert.Pattern == Alert.PatternEnum.Daily.ToString())
			{
				int newInterval = (int)(alert.DaysSinceNewData / 10.0); // new interval (e.g. 20 days since new data -> 2-day interval2)
				if (newInterval > checkInterval)
				{
					checkInterval = newInterval;
					intervalExpanded = true;
				}
			}

			// Run every weekday?
			if (alert.Days == Alert.DaysEnum.WeekDay.ToString())
			{
				if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
				{
					msg = "Alert only runs on weekdays." +
											 " (Query: " + queryUserObject.InternalName + ")";
				}
				if (alert.DaysSinceLastCheck < 1)
				{
					msg = "Elapsed days (" + alert.DaysSinceLastCheck + " days), alert already ran today";
				}
				return msg;
			}
			// Long enough since last check
			if (alert.DaysSinceLastCheck < checkInterval)
			{
				msg = "Elapsed days (" + alert.DaysSinceLastCheck + " days) less than" +
												(!intervalExpanded ? "" : " expanded") + " interval (" + checkInterval + " days) " +
												" (Query: " + queryUserObject.InternalName + ")";
				return msg;
			}



			msg = CheckForDelay(alert, queryUserObject);
			return msg;
		}

		private static string CheckWeekly(Alert alert, UserObject queryUserObject)
		{
			string msg = "";
			if (!alert.CorrectDayOfWeek)
			{
				msg = "Alert only runs on " + alert.Days +
				 ". (Query: " + queryUserObject.InternalName + ")";
				return msg;
			}

			// Did alert already run today?
			if (alert.DaysSinceLastCheck < 1)
			{
				msg = "Alert already ran today. (Query: " + queryUserObject.InternalName + ")";
				return msg;
			}

			// If an alert already ran this week then we already decided to run this week.
			// e.g. if today is Thusday, and we ran Tuesday, current week and last week checked are the same, but we still 
			// need to run this week
			DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
			if (dfi == null) return "DateTimeFormatInfo dfi is null.";
			int currentWeek = dfi.Calendar.GetWeekOfYear(DateTime.Today, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

			bool checkThisWeek = currentWeek == alert.LastWeekChecked;
			int weeksSinceCheck = currentWeek - alert.LastWeekChecked;

			// Is the duration of weeks since we last checked less than the interval?
			// And we have not already decided this earlier in the week?
			if (weeksSinceCheck < alert.Interval && !checkThisWeek)
			{
				msg = "Alert only runs every " + alert.Interval + " weeks. " +
												". (Query: " + queryUserObject.InternalName + ")";
				return msg;
			}

			msg = CheckForDelay(alert, queryUserObject);
			return msg;
		}

		private static string CheckMonthly(Alert alert, UserObject queryUserObject)
		{
			string msg = "";
			if (!string.IsNullOrEmpty(alert.Days))
			{
				// Process <alert.DayInterval> <alert.Days> of every <alert.Interval> month
				msg = "Alert only runs on " + alert.Days + " of week " + alert.DayInterval + " every " + alert.Interval + " months" +
										". (Query: " + queryUserObject.InternalName + ")";

				if (!alert.CorrectDayOfWeek) return msg;
				if (!alert.CorrectWeekOfMonth) return msg;
				if (!alert.CorrectMonthOfYear) return msg;

				msg = CheckForDelay(alert, queryUserObject);
				return msg;
			}

			// Process Day <alert.DayInterval> of every <alert.Interval> month
			msg = "Only run on day " + alert.DayInterval + " of every " + alert.Interval + " month(s)." +
							" (Query: " + queryUserObject.InternalName + ")";

			if (!alert.CorrectDayOfMonth) return msg;  // CorrectDayOfMonth
			if (!alert.CorrectMonthOfYear) return msg;

			msg = CheckForDelay(alert, queryUserObject);
			return msg;
		}

		static void MonitorAlertCpuTime()
		{
			TimeSpan processorTime0 = Process.GetCurrentProcess().TotalProcessorTime;
			if (AlertExecuting == null || QueryExecuting == null) return;
			Alert a = AlertExecuting;
			int id = a.Id;
			Query q = QueryExecuting;

			while (true)
			{
				TimeSpan processorTime = Process.GetCurrentProcess().TotalProcessorTime;
				if (AlertExecuting == null || AlertExecuting.Id != id) return;

				double alertCpuTime = processorTime.Subtract(processorTime0).TotalMinutes;
				if (alertCpuTime > AlertCpuTimeout) break;
				//if (Math.Abs(1) == 1) break; // debug

				Thread.Sleep(60 * 1000); // sleep a minute
			}

			// Log the timeout and end this process

			string msg =
					"Alert CPU time exceeded " + AlertCpuTimeout + " mins. Killing process for alert: " + a.Id +
					", QueryId: " + a.QueryObjId + ", QueryName: " + a.QueryName + ", Owner: " + (q.UserObject != null ? q.UserObject.Owner : "");

			LogAlertMessage(msg);
			LogAlertErrorMessage(msg);
			Thread.Sleep(1 * 1000);
			Environment.Exit(-1); // exit process immediately

#if false


					MailMessage mail = new MailMessage();
					mail.From = new MailAddress("Mobius@[server]", UmlautMobius.String);
					mail.Subject = "Mobius alert for query " + Lex.Dq(q.UserObject.Name);
					Email.SetMultipleMailTo(mail, a.MailTo);
					mail.Body =
						"The execution time of the alert on query \"" + q.UserObject.Name +
						"\" has exceeded the " + timeOut + " minute time limit and as a result the alert has been disabled. " +
						"You may want to modify the query so that executes more quickly, " +
						"if possible, and then reenable the alert.";
					Email.Send(mail);
			return;
#endif

		}

		/// <summary>
		/// Export complete results
		/// </summary>
		/// <param name="a"></param>
		/// <param name="q"></param>
		/// <param name="cidListUo"></param>
		/// <param name="html">Base html to insert results into</param>
		/// <returns></returns>
		static string ExportCompleteResultsForAlert(
				Alert a,
				Query q,
				UserObject cidListUo,
				string html)
		{
			string html2;

			string templateFileName = "MobiusAlertBackgroundExportEmailTemplate.htm";
			bool emailResultsHtml = false;
			if (Lex.IsNullOrEmpty(html)) // if no parent html then email from here as just a background export
			{
				templateFileName = "MobiusBackgroundExportEmailTemplate.htm";
				emailResultsHtml = true;
			}

			ResultsFormat rf = a.ExportParms; // get ref to export options from alert

			if (a.HighlightChangedCompounds && cidListUo != null)
			{ // if requested, build new query with added calc field column showing changed columns

				// Build calculated field

				QueryTable qt = q.Tables[0];
				MetaTable mt = qt.MetaTable;
				MetaColumn mc = mt.KeyMetaColumn;
				CalcField cf = new CalcField();
				cf.CalcType = CalcTypeEnum.Basic;
				cf.Operation = "None";

				cf.SourceColumnType = mc.DataType;
				cf.PreclassificationlResultType = mc.DataType;
				cf.FinalResultType = MetaColumnType.String;

				CalcFieldColumn cfc = cf.Column1;
				cfc.MetaColumn = mc;
				cfc.Function = "None";
				cfc.Constant = "None";

				CondFormat map = new CondFormat();
				cf.Classification = map;
				map.ColumnType = mc.DataType;
				CondFormatRule rule = new CondFormatRule();
				map.Rules.Add(rule);
				rule.Name = "Y"; // display Y if compound is in changed list
				rule.Value2 = cidListUo.Id.ToString(); // need list id
				rule.Value = cidListUo.Name; // name for label only
				rule.BackColor1 = Color.FromArgb(153, 255, 102); // light green

				UserObject cfUo = new UserObject(UserObjectType.CalcField);
				cfUo.Owner = "AlertMonitor";
				cfUo.Name = "NewDataCalcFieldForQuery_" + q.UserObject.Id; // name based on query
				cfUo.AccessLevel = UserObjectAccess.Public;

				cfUo.Content = cf.Serialize();
				UserObjectDao.Write(cfUo); // save the calculated field

				string cfMtName = "CalcField_" + cfUo.Id; // metatable name for this calculated field
				MetaTable cfMt = QbUtil.UpdateMetaTableCollection(cfMtName);

				// Build & save modified query

				Query q2 = q.Clone();
				q2.BrowseSavedResultsUponOpen = true; // need to actually run the query
				q2.ResultsDataTableFileName = "Query_" + q.UserObject.Id + "_Results.bin"; // name file by original query id
				if (q2.Tables.Count >= 2 && // remove any existing "new data" table
						q2.Tables[1].MetaTable.MetaColumns.Count == 2 &&
						Lex.Eq(q2.Tables[1].MetaTable.MetaColumns[1].Label, "New Data"))
					q2.RemoveQueryTableAt(1);

				QueryTable qt2 = new QueryTable(cfMt);
				qt2.Label = "New Data";
				qt2.QueryColumns[1].Label = "New Data";
				qt2.QueryColumns[1].DisplayWidth = 3;

				if (q2.Tables.Count == 1 && q2.Tables[0].MetaTable.Parent != null)
				{ // if going from one to two tables with the added hilight calc field table and first table is not a root then add a root table
					QueryTable qt0 = new QueryTable(q2.Tables[0].MetaTable.Parent);
					qt0.SelectKeyOnly();
					q2.InsertQueryTable(0, qt0); // put in 1st position
				}

				q2.InsertQueryTable(1, qt2); // put the calc field in the 2nd position

				q2.UserObject.Owner = "AlertMonitor";
				q2.UserObject.Name = "NewDataQueryForQuery_" + q.UserObject.Id; // name based on query
				q2.UserObject.ParentFolder = ""; // no parent folder
				q2.UserObject.AccessLevel = UserObjectAccess.Public;
				q2.UserObject.Content = q2.Serialize();
				UserObjectDao.Write(q2.UserObject); // save the query

				q = q2; // use this query in place of the original
				rf.QueryId = q.UserObject.Id;

				QueryManager qm = new QueryManager();
				qm.LinkMember(q);
				qm.LinkMember(rf);
			}

			UserObject bexUo = new UserObject(UserObjectType.BackgroundExport);
			bexUo.Id = UserObjectDao.GetNextId();

			//string ext = Path.GetExtension(rf.OutputFileName);
			//rf.OutputFileName = bexUo.Id + ext; // use export object id as unique server file name

			//string clientFile = rf.OutputFileName;

			bexUo.Owner = "AlertMonitor";
			bexUo.Name = "BackgroundExportForQuery_" + bexUo.Id; // unique name for export object
			bexUo.Content = rf.Serialize();
			UserObjectDao.Write(bexUo, bexUo.Id); // save the background export object

			try
			{
				if (a.ExportParms.OutputDestination == OutputDest.WinForms) // write results as native Mobius format
				{ // run query & write native format Mobius file
					html2 = BackgroundQuery.RunBackgroundQuery(q, null, "MobiusAlertBackgroundQueryEmailTemplate.htm");
				}

				else // do export to other non-Mobius format
				{
					bool copiedToDestinationFile; // copied to final destination file
					html2 = BackgroundQuery.RunBackgroundExport(
							bexUo.Id.ToString(),
							templateFileName,
							emailResultsHtml,
							out copiedToDestinationFile,
							a.Id);
				}
			}

			catch (Exception ex)
			{
				html2 = "<br>Background export failed<br>" + ex.Message + "<br>";
			}

			if (!Lex.IsNullOrEmpty(html))
				html = Lex.Replace(html, "<!-- export-text-goes-here -->", html2);

			return html;
		}

		/// <summary>
		/// Mail a message indicating that alert or background query results are available
		/// </summary>
		/// <param name="q"></param>
		/// <param name="changedCompoundCount"></param>
		/// <param name="mailTo"></param>
		/// <param name="mailSubject"></param>
		/// <param name="viewCmd"></param>
		/// <param name="listOrQueryId"></param>
		/// <param name="cancelObj"></param>
		/// <param name="html"></param>

		public static void MailResultsAvailableMessage(
				Query q,
				int changedCompoundCount,
				string mailTo,
				string mailSubject,
				string viewCmd,
				int listOrQueryId,
				UserObject cancelObj,
				string html)
		{
			// Mail notice to user

			MailMessage mail = new MailMessage();
			mail.From = new MailAddress("Mobius@[server]", UmlautMobius.String);
			mail.Subject = mailSubject;
			Email.SetMultipleMailTo(mail, mailTo);

			html = SubstituteHtmlParms(html, q, changedCompoundCount, viewCmd, listOrQueryId, cancelObj);

			mail.Body = html;
			mail.IsBodyHtml = true;

			Email.Send(mail);
		}

		public static string SubstituteHtmlParms(
				string html,
				Query q,
				int changedCompoundCount,
				string viewCmd,
				int listOrQueryId,
				UserObject cancelObj)
		{
			string queryLabel = q.UserObject.Name;
			string txt;
			try
			{
				txt = SecurityUtil.GetShortPersonNameReversed(q.UserObject.Owner);  // include owner's name
				queryLabel += " (" + txt + ")";
			}
			catch (Exception) { }

			html = html.Replace("query-name", queryLabel);
			html = html.Replace("changed-cmpd-count", changedCompoundCount.ToString());
			//						html = html.Replace("new-cmpd-cnt", keyDelta.ToString());
			//						html = html.Replace("new-rec-cnt", rowDelta.ToString());

			viewCmd = viewCmd + " " + listOrQueryId;
			string viewLink = "Mobius:Command=" + Lex.AddSingleQuotes(viewCmd); // Uses Mobius pluggable protocol
			html = html.Replace(@"file:///\\view-data-link", viewLink);
			html = html.Replace("view-command", viewCmd);

			if (cancelObj != null) // fill in cancel command
			{
				string cancelCmd = "DeleteAlert " + cancelObj.Id;
				string cancelLink = "Mobius:Command=" + Lex.AddSingleQuotes(cancelCmd);
				html = html.Replace(@"file:///\\cancel-alert-link", cancelLink);
				html = html.Replace("cancel-command", cancelCmd);
			}

			return html;
		}

		/// <summary>
		/// Write message to log with exclusive access
		/// </summary>
		/// <param name="message"></param>

		public static void LogAlertMessage(
				string message)
		{
			if (_log == null) _log = new ServicesLogFile(AlertLogFileName);

			Mutex mutex = new Mutex(false, "MobiusAlertLog");
			mutex.WaitOne(); // get exclusive access

			try
			{
				message = "[Alert] - " + message;
				if (_log != null) _log.Message(message);
			}

			finally { mutex.ReleaseMutex(); }
		}

		public static void LogAlertErrorMessage(
		string message)
		{
			if (_errorLog == null) _errorLog = new ServicesLogFile(AlertErrorLogFileName);

			Mutex mutex = new Mutex(false, "MobiusAlertErrorLog");
			mutex.WaitOne(); // get exclusive access
			try
			{
				message = "[Alert] - " + message;
				if (_errorLog != null) _errorLog.Message(message);
			}
			finally { mutex.ReleaseMutex(); }

		}

	}


}