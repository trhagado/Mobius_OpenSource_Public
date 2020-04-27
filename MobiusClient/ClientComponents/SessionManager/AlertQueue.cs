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
using System.Net.Mail;
using Microsoft.Office.Interop.Word;

namespace Mobius.ClientComponents
{
	public class AlertQueue
	{

		private int _startingAlertCount;
		private int _numberOfCurrentProcessors;

		private static string _serverLogDir;
		private static string _alertQueueFileName;

		private static string _mobiusAlertQueue = "MobiusAlertQueue";

		public int NumberOfCurrentProcessors
		{
			get
			{
				return _numberOfCurrentProcessors;
			}
		}

		public int StartingAlertCount
		{
			get
			{
				return _startingAlertCount;
			}
		}

		/// <summary>
		/// This constructor will read all the alerts from the database and create an alert queue.  The queue consists of a
		/// comma delimeted file with all the alert ids.
		/// </summary>
		public AlertQueue(bool loadAlertsFromDb)
		{
			string alertQueueFileName = GetAlertQueueFileName();

			if (loadAlertsFromDb)
			{
				List<Alert> alerts = GetAllAlerts();

				StringBuilder sb = new StringBuilder();
				const int firstToCheck = 0;
				foreach (Alert ai0 in alerts)
				{
					if (ai0.Id < firstToCheck) continue;
					if (sb.Length > 0) sb.Append(",");
					sb.Append(ai0.Id.ToString());
					_startingAlertCount++;
				}
				//_remainingingAlertCount = _startingAlertCount;
				Mutex mutex = new Mutex(false, _mobiusAlertQueue);
				mutex.WaitOne(); // get exclusive access
				try
				{
					StreamWriter sw = new StreamWriter(alertQueueFileName);
					sw.Write(sb);
					sw.Close();
				}
				finally { mutex.ReleaseMutex(); }
				return;
			}

			//string[] alertIds = GetAlertIds();
			//int alertCount = alertIds.Length;
			//if (alertIds.Length == 1 && alertIds[0].Length == 0) alertCount = 0;
			//_remainingingAlertCount = _startingAlertCount = alertCount;

		}

		/// <summary>
		/// Get all alerts from the DB. Sort alerts from oldest to newest LastCheck Date so oldest alerts are put at the top of the queue.
		/// </summary>
		/// <returns></returns>
		private static List<Alert> GetAllAlerts()
		{
			List<UserObject> alertUos = UserObjectDao.ReadMultiple(UserObjectType.Alert, false);

			List<Alert> alerts = new List<Alert>();
			for (int ai = 1; ai < alertUos.Count; ai++) // deserialize alerts so they can be sorted
			{
				UserObject uo = alertUos[ai];
				Alert alert = Alert.GetAlertFromUserObject(uo, false);
				alerts.Add(alert);
			}
			List<Alert> sortedAlerts = alerts.OrderBy(o => o.StartTime).ThenBy(o => o.LastCheck).ToList();
			return sortedAlerts;
		}

		/// <summary>
		/// Get a list of all remaining alert ids from the AlertQueue on the file server.
		/// </summary>
		/// <returns></returns>
		private string[] GetAlertIds()
		{
			Mutex mutex = new Mutex(false, _mobiusAlertQueue);
			mutex.WaitOne(); // get exclusive access
			string alertQueueText;
			try
			{
				string alertQueueFileName = GetAlertQueueFileName();
				StreamReader sr = new StreamReader(alertQueueFileName);
				alertQueueText = sr.ReadToEnd();
				sr.Close();
			}
			finally { mutex.ReleaseMutex(); }

			string[] sa = alertQueueText.Split(',');
			return sa;
		}

		/// <summary>
		/// Delay the running of the alert because the time specified is in the future.
		/// Sleep for thirty minutes
		/// </summary>
		/// <param name="alert"></param>
		public static void AddAlertBack(Alert alert)
		{
			Mutex mutex = new Mutex(false, _mobiusAlertQueue);
			mutex.WaitOne(); // get exclusive access
			string alertQueueText;
			try
			{
				string alertQueueFileName = GetAlertQueueFileName();
				StreamReader sr = new StreamReader(alertQueueFileName);
				alertQueueText = sr.ReadToEnd();
				sr.Close();

				//if (alertQueueText.EndsWith(alert.Id.ToString())) return;  // this should never happen

				//alertQueueText = alertQueueText.Replace(alert.Id + ",", "");
				//if (!string.IsNullOrEmpty(alertQueueText)) alertQueueText += ",";
				//alertQueueText += alert.Id;

				// add the alert right back to the top of the queue
				alertQueueText = alertQueueText.Length > 0 ? alert.Id + "," + alertQueueText : alert.Id.ToString();

				StreamWriter sw = new StreamWriter(alertQueueFileName);
				sw.Write(alertQueueText);
				sw.Close();

			}
			finally { mutex.ReleaseMutex(); }
		}


		/// <summary>
		/// Start new process to check queued alerts
		/// </summary>

		public void StartQueueProcessor(
				string args)
		{
			_numberOfCurrentProcessors++;
			string msg = "Starting queue processor " + NumberOfCurrentProcessors;
			AlertUtil.LogAlertMessage(msg);
			Progress.Show(msg);

			if (Lex.Contains(args, "singleProcessor")) // use single queue processor within this process (for debug)
			{
				CheckQueuedAlerts(args);
			}

			else // start separate process to check the queued alerts
			{
				try
				{ CommandLine.StartForegroundSession("Check Queued Alerts " + args, null); }
				catch (Exception ex)
				{ AlertUtil.LogAlertMessage("Couldn't start new queue processor:\r\n" + ex.Message); }
			}
		}

		public static UserObject GetNextAlert(string processorId, string alertQueueFileName)
		{
			Mutex mutex = new Mutex(false, _mobiusAlertQueue);
			mutex.WaitOne(); // get exclusive access
			int alertId;
			try
			{
				StreamReader sr = new StreamReader(alertQueueFileName);
				string content = sr.ReadToEnd();
				sr.Close();
				if (Lex.IsNullOrEmpty(content)) // all done if nothing left in queue
				{
					//AlertCount = 0;
					return null;
				}

				int i1 = content.IndexOf(",");
				if (i1 >= 0)
				{
					alertId = Int32.Parse(content.Substring(0, i1));
					content = content.Substring(i1 + 1);
				}
				else
				{
					alertId = Int32.Parse(content);
					content = "";
				}
				StreamWriter sw = new StreamWriter(alertQueueFileName);
				sw.Write(content);
				sw.Close();
				//_remainingingAlertCount--;
			}
			catch (Exception ex)
			{
				AlertUtil.LogAlertMessage("Error accessing alert queue: " + ex.Message + processorId);
				return null;
			}

			finally { mutex.ReleaseMutex(); }

			UserObject uo = UserObjectDao.Read(alertId); // read the alert
			if (uo == null)
			{
				AlertUtil.LogAlertMessage("Error reading alert " + alertId + processorId);
			}
			return uo;
		}

		/// <summary>
		/// Check queued alerts. Several of these process may be running at the same time in parallel.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string CheckQueuedAlerts(
				string args)
		{
			string msg;
			int i1;
			bool forceCheck = false;
			bool checkOnly = false;
			int firstToCheck = 0;
			bool hasMoreAlerts = true;
			int alertsWithNewData = 0;
			int alertsChecked = 0;

			string processorIdTxt = ", Pid: " + (int)DateTime.Now.TimeOfDay.TotalSeconds; // mark each message with id of processor (sec of day started)

			AlertUtil.LogAlertMessage("Queue processor started: " + args + processorIdTxt);

			if (args != null && args.Trim().Length > 0)
			{
				if (Lex.Contains(args, "forceCheck")) forceCheck = true;
				if (Lex.Contains(args, "checkOnly")) checkOnly = true;
			}

			string alertQueueFileName = GetAlertQueueFileName();

			while (hasMoreAlerts)
			{
				UserObject uo = GetNextAlert(processorIdTxt, alertQueueFileName);
				if (uo == null) break;

				int queryId = 0; // get queryId from alert name
				i1 = uo.Name.IndexOf("_");
				if (i1 > 0)
				{
					try { queryId = Int32.Parse(uo.Name.Substring(i1 + 1)); }
					catch (Exception) { }
				}

				msg = "Processing Alert: " + uo.Id +
						", QueryId: " + queryId + processorIdTxt;
				//			Thread.Sleep(10000); // debug
				AlertUtil.LogAlertMessage(msg);
				Progress.Show(msg);

				DateTime begin = DateTime.Now;
				try // check it
				{
					msg = AlertUtil.Check(uo, forceCheck, checkOnly, false);
				}
				catch (Exception ex)
				{
					msg = "Error executing Alert: " + uo.Id +
							", QueryId: " + queryId + ", Processor: " + processorIdTxt;

					if (Lex.Contains(ex.Message, "ORA-01012") || Lex.Contains(ex.Message, "ORA-00028"))
						msg += ", Error: " + ex.Message; // keep simple if forced logoff

					else // include stack trace otherwise
						msg += "Error: " + DebugLog.FormatExceptionMessage(ex, msg);

					AlertUtil.LogAlertMessage(msg);
					AlertUtil.LogAlertErrorMessage(msg);
					alertsChecked++;
					continue;
				}

				if (Lex.Contains(msg, "Delayed Alert"))
				{
					AlertUtil.LogAlertMessage(msg);
					Alert alert = Alert.GetAlertFromUserObject(uo, true);
					AddAlertBack(alert);
					SleepUntilNextAlertStartTime(uo);
					continue;
				}

				string msg0 =  // message prefix
					"Completed Alert: " + uo.Id + ", QueryId: " + queryId + processorIdTxt;

				//DateTime end = DateTime.Now; // (redundant)
				//TimeSpan elapsed = end.Subtract(begin);
				//string eTxt = elapsed.ToString();
				//if ((i1 = eTxt.IndexOf(".")) > 0) eTxt = eTxt.Substring(0, i1);
				//msg0 += ", Time to process = " + eTxt;

				msg = msg0 + " " + msg;
				AlertUtil.LogAlertMessage(msg);

				if (Lex.Contains(msg, "Error executing alert")) // keep separate log of errors
					AlertUtil.LogAlertErrorMessage(msg);

				alertsChecked++;
				if (msg.Contains("Changed compounds")) alertsWithNewData++;
			}

			msg = "Queue processor complete, Alerts checked: " + alertsChecked +
								", Alert E-mails: " + alertsWithNewData + processorIdTxt;

			AlertUtil.LogAlertMessage("");
			AlertUtil.LogAlertMessage(msg);
			Progress.Hide();
			return msg;
		}

		/// <summary>
		/// The next alert in the Queue should not start until the Alert.RunTime arrives
		/// or 8pm.  Sleep until that time.
		/// </summary>
		/// <param name="uo"></param>
		private static void SleepUntilNextAlertStartTime(UserObject uo)
		{
			TimeSpan timespan;
			Alert alert = Alert.GetAlertFromUserObject(uo, false);
			DateTime alertProcessingDeadline = Convert.ToDateTime("8:00:00 PM");
			if (alert.StartTime.TimeOfDay < alertProcessingDeadline.TimeOfDay)
			{
				timespan = alert.StartTime.TimeOfDay - DateTime.Now.TimeOfDay;
			}
			else
			{
				timespan = alert.StartTime.TimeOfDay - alertProcessingDeadline.TimeOfDay;
			}
			DebugLog.Message("Processer sleeping until alert " + alert.Id + " start time at " + alert.StartTime);
			Thread.Sleep((int)Math.Ceiling(timespan.TotalMilliseconds));
			Thread.Sleep(5000); //sleep an extra 5 secondsbecause sometimes this is kicking off 1 second early
		}

		public int GetProcessedAlertCount()
		{
			return _startingAlertCount - GetRemainingingAlertCount();
		}


		public int GetRemainingingAlertCount()
		{
			string[] alertIds = GetAlertIds();
			int alertCount = alertIds.Length;
			if (alertIds.Length == 1 && alertIds[0].Length == 0) alertCount = 0;
			return alertCount;
		}

		/// <summary>
		/// Some methods in the AlertQueue class are static.  Therefore the _alertQueueFileName will not be set in those cases.  
		/// This method will ensure we have a valid file name in all cases.
		/// </summary>
		/// <returns></returns>
		private static string GetAlertQueueFileName()
		{
			if (_alertQueueFileName != null) return _alertQueueFileName;

			if (_serverLogDir == null) _serverLogDir = ServicesIniFile.Read("LogDirectory");
			if (String.IsNullOrEmpty(_serverLogDir)) throw new Exception("Server LogDirectory not defined in config file");

			if (_alertQueueFileName == null) _alertQueueFileName = _serverLogDir + @"\AlertQueue.txt";
			return _alertQueueFileName;
		}

	}
}
