using Mobius.Data; 
using Mobius.ComOps;
using Mobius.ServiceFacade;

using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Mobius.ClientComponents 
{
	/// <summary>
	/// New Data Alert class
	/// This class supports the saving of alerts associated with queries,
	/// nightly checking of alerts to see if new data is available and
	/// the display of new data for the query.
	/// This class handles the creation, editing and cancelling of alerts.
	/// </summary>

	public class Alert
	{
		public int Id; // id of alert user object
		public int QueryObjId; // id of associated query object
		public string QueryName = ""; // associated query name (not persisted)
		public DateTime LastQueryUpdate = DateTime.MinValue; // date of query update (not persisted)

		public int Interval; // number of days between checks

		public string Pattern; // Daily|Weekly|Monthly
		public string Days; // day|weekday|weekend day|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday
		public int DayInterval = 0; // e.g. Day "2" of every 1 month; or "2" could second Monday every n month(s)
		public DateTime StartTime;

		public bool CheckTablesWithCriteriaOnly = false; // if true then only check tables with criteria for changes
		public string Owner = ""; // owner of alert
		public string MailTo = ""; // CSV list of userids to e-mail to
		public DateTime LastCheck = DateTime.MinValue; // date of last check
		public DateTime LastNewData = DateTime.MinValue; // date last new data was found
		public int LastCheckExecutionTime = -1; // elapsed time in seconds for execution of last check
		public int NewCompounds = -1; // number of new compounds found in last check
		public int ChangedCompounds = -1; // number of changed compounds found in last check
		public int TotalCompounds = -1; // total number of compounds
		public int NewRows = -1; // number of new rows found in last check
		public int TotalRows = -1; // total rows found in last check
		public int DaysSinceLastCheck = -1;
		public int DaysSinceNewData = -1;

		public Dictionary<string, int> LastResults; // hash of compound id to count of rows
		public ResultsFormat ExportParms = null; // setup information if exporting also
		public bool HighlightChangedCompounds = true; // adds extra column showing changed compounds
		public bool RunImmediately = false; // an immediate run request from the user

	    public bool ExistingAlert
	    {
	        get
	        {
	            return (Id > 0);
	        }
	    }

	    public int LastWeekChecked
		{
			get
			{
				DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
				if (dfi == null) return 0;
				int lastWeekChecked = dfi.Calendar.GetWeekOfYear(LastCheck, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
				return lastWeekChecked;
			}
		}

		public int MonthsSinceLastRun
		{
			get
			{
				return (DateTime.Today.Year - LastCheck.Year) * 12 + DateTime.Today.Month - LastCheck.Month;
			}
		}

		public bool CorrectDayOfWeek
		{
			get { return Days.Contains(DateTime.Today.DayOfWeek.ToString()); }
		}

		public bool CorrectDayOfMonth
		{
			get { return DayInterval == DateTime.Today.Day; }
		}

		public bool CorrectWeekOfMonth
		{
			get
			{
				int currentWeekOfMonth = GetWeekNumberOfMonth(DateTime.Today);
				return DayInterval == currentWeekOfMonth;
			}
		}

		public bool CorrectMonthOfYear
		{
			get { return MonthsSinceLastRun >= Interval; }
		}

		/// <summary>
		/// Create a new alert by passing a QueryUserObject
		/// </summary>
		/// <param name="queryUserObject"></param>
		public Alert(UserObject queryUserObject)
		{
			MailTo = SS.I.UserInfo.EmailAddress;
			Owner = SS.I.UserName;
			Interval = 1; //default
			if (queryUserObject != null) // query to start with?
			{
				QueryObjId = queryUserObject.Id;
				QueryName = queryUserObject.Name;
				LastQueryUpdate = queryUserObject.UpdateDateTime;
			}
		}

		private Alert() { }

		/// <summary>
		/// Get an Alert from a User Object.  
		/// </summary>
		/// <param name="userObject"></param>
		/// <param name="includeResults">Pass a true to get full results</param>
		/// <returns></returns>

		public static Alert GetAlertFromUserObject(UserObject userObject, bool includeResults)
		{
			Alert alert = includeResults ? Deserialize(userObject) : DeserializeHeader(userObject);
			return alert;
		}

		/// <summary>
		/// Return the header information for any existing alert for this user for the specified query
		/// </summary>
		/// <param name="queryId"></param>
		/// <returns></returns>

		public static Alert GetAlertByQueryId(
			int queryId)
		{
			if (queryId <= 0) return null;

			UserObject auo = UserObjectDao.ReadHeader(UserObjectType.Alert, SS.I.UserName, "", "Alert_" + queryId);
			if (auo == null) return null;

			Alert alert = DeserializeHeader(auo);
			return alert;
		}

		/// <summary>
		/// Serialize & write the alert to a UserObject.
		/// Basic information is serialized into the Description
		/// column and the results of the most recent check are 
		/// serialized into Content.
		/// </summary>
		/// <returns></returns>

		public int Write()
		{
		    UserObject alertUserObject;
            if (ExistingAlert)
		    {
		        alertUserObject = UserObjectDao.ReadHeader(Id);
		        //alertUserObject.UpdateDateTime = DateTime.Now;
                alertUserObject.Description = SerializeHeader();
                alertUserObject.Content = SerializeResults();
                UserObjectDao.Write(alertUserObject, alertUserObject.Id);
            }
		    else
		    {
		        alertUserObject = new UserObject(UserObjectType.Alert)
		        {
		            Id = Id,
		            Owner = Owner,
		            Name = "Alert_" + QueryObjId
		        };
                alertUserObject.Description = SerializeHeader();
                alertUserObject.Content = SerializeResults();
                UserObjectDao.Write(alertUserObject);
                Id = alertUserObject.Id;
            }
			return Id;
		}
        
		/// <summary>
		/// Serialize Alert header (everything except results)
		/// </summary>
		/// <returns></returns>

		private string SerializeHeader()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			XmlTextWriter tw = mstw.Writer;
			tw.Formatting = Formatting.Indented;
			tw.WriteStartElement("Alert");

			tw.WriteAttributeString("Owner", Owner);
			tw.WriteAttributeString("QueryObjId", QueryObjId.ToString());
			tw.WriteAttributeString("Interval", Interval.ToString());
			tw.WriteAttributeString("MailTo", MailTo);
			tw.WriteAttributeString("LastCheck", LastCheck.ToString());
			tw.WriteAttributeString("LastCheckElapsedTime", LastCheckExecutionTime.ToString());
			tw.WriteAttributeString("NewCompounds", NewCompounds.ToString());
			tw.WriteAttributeString("ChangedCompounds", ChangedCompounds.ToString());
			tw.WriteAttributeString("NewRows", NewRows.ToString());
			tw.WriteAttributeString("TotalRows", TotalRows.ToString());
			tw.WriteAttributeString("TotalCompounds", TotalCompounds.ToString());
			tw.WriteAttributeString("CheckTablesWithCriteriaOnly", CheckTablesWithCriteriaOnly.ToString());
			tw.WriteAttributeString("LastNewData", LastNewData.ToString());
			tw.WriteAttributeString("HighlightChangedCompounds", HighlightChangedCompounds.ToString());
			tw.WriteAttributeString("RunImmediately", RunImmediately.ToString());
			tw.WriteAttributeString("Pattern", Pattern);
			tw.WriteAttributeString("StartTime", GetEasternStandardFromLocalTime(StartTime).ToShortTimeString());
			if (!String.IsNullOrEmpty(Days)) tw.WriteAttributeString("Days", Days);
			if (DayInterval > 0) tw.WriteAttributeString("DayInterval", DayInterval.ToString());

			if (ExportParms != null)
			{
				ExportParms.QueryId = QueryObjId;
				string ext = System.IO.Path.GetExtension(ExportParms.OutputFileName);
				ExportParms.OutputFileName2 = Id + ext; // save under alert Id if not saving to a share
				ExportParms.Serialize(tw);
			}

			tw.WriteEndElement(); // end of Alert

			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		private static DateTime GetEasternStandardFromLocalTime(DateTime dateTime)
		{
			TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName);
			TimeZoneInfo easternStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, currentTimeZone);
			DateTime easternStandardDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternStandardTimeZone);
			return easternStandardDateTime;
		}

		private static DateTime GetLocalFromEasternStandardTime(DateTime dateTime)
		{

			TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName);
			TimeZoneInfo easternStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, easternStandardTimeZone);
			DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, currentTimeZone);
			return localDateTime;
		}

		/// <summary>
		/// Serialize the last results.
		/// </summary>
		/// <returns></returns>

		private string SerializeResults()
		{
			StringBuilder sb = new StringBuilder();

			if (LastResults != null) // [5] - results go last
			{
				foreach (string key in LastResults.Keys)
				{
					sb.Append(key);
					sb.Append(","); // command between key and value
					sb.Append(LastResults[key]);
					sb.Append("\n"); // newline after each number
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Deserialize a complete alert from a userobject. Some fields are created dynamically using calculations.
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		private static Alert Deserialize(
			UserObject uo)
		{
			Alert alert = DeserializeHeader(uo);
			alert.LastResults = DeserializeResults(uo.Content);
			return alert;
		}

		/// <summary>
		/// Deserialize an alert header in a user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		private static Alert DeserializeHeader(
			UserObject uo)
		{
			Alert alert = new Alert();
			alert.Id = uo.Id;

			string serializedForm = uo.Description;
			if (String.IsNullOrEmpty(serializedForm)) return alert;

			if (!Lex.Contains(serializedForm, "<Alert")) return DeserializeHeaderOld(uo);

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);
			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get CondFormat element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "Alert"))
				throw new Exception("\"Alert\" element not found");

			XmlUtil.GetStringAttribute(tr, "Owner", ref alert.Owner);
			XmlUtil.GetIntAttribute(tr, "QueryObjId", ref alert.QueryObjId);
			XmlUtil.GetIntAttribute(tr, "Interval", ref alert.Interval);
			XmlUtil.GetStringAttribute(tr, "MailTo", ref alert.MailTo);
			XmlUtil.GetDateAttribute(tr, "LastCheck", ref alert.LastCheck);
			XmlUtil.GetIntAttribute(tr, "LastCheckElapsedTime", ref alert.LastCheckExecutionTime);
			XmlUtil.GetIntAttribute(tr, "NewCompounds", ref alert.NewCompounds);
			XmlUtil.GetIntAttribute(tr, "ChangedCompounds", ref alert.ChangedCompounds);
			XmlUtil.GetIntAttribute(tr, "NewRows", ref alert.NewRows);
			XmlUtil.GetIntAttribute(tr, "TotalRows", ref alert.TotalRows);
			XmlUtil.GetIntAttribute(tr, "TotalCompounds", ref alert.TotalCompounds);
			XmlUtil.GetBoolAttribute(tr, "CheckTablesWithCriteriaOnly", ref alert.CheckTablesWithCriteriaOnly);
			XmlUtil.GetDateAttribute(tr, "LastNewData", ref alert.LastNewData);
			XmlUtil.GetBoolAttribute(tr, "HighlightChangedCompounds", ref alert.HighlightChangedCompounds);
			XmlUtil.GetBoolAttribute(tr, "RunImmediately", ref alert.RunImmediately);

			XmlUtil.GetStringAttribute(tr, "Days", ref alert.Days);
			XmlUtil.GetIntAttribute(tr, "DayInterval", ref alert.DayInterval);
			XmlUtil.GetDateAttribute(tr, "StartTime", ref alert.StartTime);
			alert.StartTime = GetLocalFromEasternStandardTime(alert.StartTime);
			XmlUtil.GetStringAttribute(tr, "Pattern", ref alert.Pattern);
			if (string.IsNullOrEmpty(alert.Pattern)) alert.Pattern = PatternEnum.Daily.ToString();

			if (!tr.IsEmptyElement)
			{
				tr.Read(); // move to Export options or end of Alert element
				tr.MoveToContent();

				if (tr.NodeType == XmlNodeType.Element && Lex.Eq(tr.Name, "ResultsFormat"))
				{
					alert.ExportParms = ResultsFormat.Deserialize(tr);
					tr.Read(); // get Alert end element
					tr.MoveToContent();
				}

				if (!Lex.Eq(tr.Name, "Alert") || tr.NodeType != XmlNodeType.EndElement)
					throw new Exception("Alert.Deserialize - Expected Alert end element");
			}

			mstr.Close();

			double daysSinceLastCheckwithDecimal = (alert.LastCheck == DateTime.MinValue) ? alert.Interval : (DateTime.Now - alert.LastCheck).TotalDays;
			double daysSinceNewDatawithDecimal = (alert.LastNewData == DateTime.MinValue) ? 0 : (DateTime.Now - alert.LastNewData).TotalDays;

			//Round these calculations so .5 days will become 1 day, effectively 12 hours or more will count as a whole day.
			alert.DaysSinceLastCheck = (int)Math.Round(daysSinceLastCheckwithDecimal, MidpointRounding.AwayFromZero);
			alert.DaysSinceNewData = (int)Math.Round(daysSinceNewDatawithDecimal, MidpointRounding.AwayFromZero);

			//DebugLog.Message("Alert: " + alert.Id + " daysSinceLastCheckwithDecimal: " + daysSinceLastCheckwithDecimal);
			//DebugLog.Message("Alert: " + alert.Id + " new DaysSinceLastCheck: " + alert.DaysSinceLastCheck);
			//DebugLog.Message("Alert: " + alert.Id + " daysSinceNewDatawithDecimal: " + daysSinceNewDatawithDecimal);            
			//DebugLog.Message("Alert: " + alert.Id + " DaysSinceNewData: " + alert.DaysSinceNewData);

			return alert;
		}

		/// <summary>
		/// Deserialize an old-format alert header in a user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		private static Alert DeserializeHeaderOld(
				UserObject uo)
		{
			return DeserializeHeaderOld(uo, true);
		}

		private static Alert DeserializeHeaderOld(
			UserObject uo,
			bool fast)
		{
			string[] sa;

			Alert a = new Alert();
			a.Id = uo.Id;

			//if (uo.Description.Trim().Length > 0 && uo.Description.Split('\t').Length == 2)
			//{ // sometimes the desc contains just the queryId \t lastNewDate
			//  sa = uo.Description.Split('\t');
			//  string qidString = sa[0].Substring(sa[0].IndexOf(" ") + 1);
			//  a.QueryObjId = int.Parse(qidString);
			//  a.LastNewData = DateTimeEx.DateTimeParseUS(sa[1]);
			//  return a;
			//}

			if (uo.Description.Trim().Length > 0)
			{ // have old new-style info?
				try
				{
					sa = uo.Description.Split('\t');
					a.Owner = sa[0];
					a.QueryObjId = Int32.Parse(sa[1]);
					a.Interval = Int32.Parse(sa[2]);
					a.MailTo = sa[3];
					if (a.MailTo == null || a.MailTo.Trim().Length == 0) // need to get email address
						a.MailTo = Security.GetUserEmailAddress(a.Owner);

					a.LastCheck = DateTimeMx.NormalizedToDateTime(sa[4]);
					a.NewCompounds = Int32.Parse(sa[5]);
					a.ChangedCompounds = Int32.Parse(sa[6]);
					a.TotalCompounds = Int32.Parse(sa[7]);
					a.NewRows = Int32.Parse(sa[8]);
					a.TotalRows = Int32.Parse(sa[9]);
					if (sa.Length > 10) a.CheckTablesWithCriteriaOnly = bool.Parse(sa[10]);
					if (sa.Length > 11) a.LastNewData = DateTimeMx.NormalizedToDateTime(sa[11]);
				}
				catch (Exception ex) { throw new Exception(ex.Message, ex); } // debug
			}

			else // old style alert, get partial header info
			{
				a.Owner = uo.Owner;
				a.QueryObjId = Int32.Parse(uo.Name.Substring(uo.Name.IndexOf("_") + 1));
				a.Interval = 1; // assume just 1 day
				if (!fast)
					try // get old last check date
					{
						Query q = QbUtil.ReadQuery(a.QueryObjId);
						if (q.AlertInterval > 0) a.Interval = q.AlertInterval;
						sa = uo.Content.Split('\t');
						a.LastCheck = DateTimeMx.NormalizedToDateTime(sa[4]);
					}
					catch (Exception ex) { string msg = ex.Message; }

				a.MailTo = Security.GetUserEmailAddress(a.Owner); // need to get email address
			}

			return a;
		}

		/// <summary>
		/// Deserialize the list of compound ids in last results 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static Dictionary<string, int> DeserializeResults(
			string content)
		{
			string[] sa;

			Dictionary<string, int> results = new Dictionary<string, int>();
			if (String.IsNullOrEmpty(content)) return results;

			if (content.Contains("\t"))
			{
				sa = content.Split('\t');
				content = sa[5]; // old form content after other attributes
			}
			sa = content.Split('\n');
			foreach (string s in sa)
			{
				if (s == "") continue;
				string[] sa2 = s.Split(',');
				results[sa2[0]] = Int32.Parse(sa2[1]); // key = cmpnd id, value = row count
			}

			return results;
		}

		/// <summary>
		/// Return true if query is ok for alert or there is no alert associated with query
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public static bool QueryValidForAlert(Query q)
		{
			int criteriaCount = q.GetCriteriaCount(true, false); // count simple criteria
			if (q.LogicType == QueryLogicType.Complex && !String.IsNullOrEmpty(q.ComplexCriteria))
				criteriaCount = 1;

			if (criteriaCount == 0)
			{
				MessageBoxMx.Show(
					"An alert cannot be defined on this query because it does not contain any search criteria.\n" +
					"You may want to use the \"Where data exists\" criteria on one of the fields in the query.",
					UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}

			if (q.KeyCriteria.ToLower().IndexOf("in list current") >= 0 || Lex.Contains(q.KeyCriteria, "in list " + UserObject.TempFolderNameQualified))
			{
				MessageBoxMx.Show(
					"An alert cannot be defined on this query because it uses a volatile temporary list criteria.\n" +
					"If you save the temporary list and then use this saved list as the criteria then you can define an alert on the query.",
					UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}

			return true;
		}

		/// <summary>
		/// See if saving this query would have an acceptable effect
		/// on any persisted alert results.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>True if ok to save</returns>

		public static bool QuerySaveOkForAlertResults(
			Query q)
		{
			Alert alert = GetAlertByQueryId(q.UserObject.Id);
			if (alert == null) return true; // no alert so ok

			if (alert.LastCheck != DateTime.MinValue && // has alert been checked?
				q.AlertQueryState != GetAlertQueryCriteria(q)) // have criteria changed?
			{
				DialogResult dr = MessageBoxMx.Show(
					"Since the search criteria for this new-data alerting query have changed the \n" +
					"alert state will be \"reset\" which means that the next time the alert runs it will\n" +
					"not mail out any detected data changes but will simply record the current state of\n" +
					"the data with respect to the new search criteria. Subsequent alert runs will then\n" +
					"check for new data changes.\n\n" +
					"Do you still want to continue to save the query?",
					UmlautMobius.String, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes) return false;

				alert.LastCheck = DateTime.MinValue; // reset the alert
				alert.LastResults = null; // (should already be null)
				alert.Write();
			}

			return true;
		}

		/// <summary>
		/// Get subset of query state information necessary to determine if alert needs to be reset
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		/// 
		public static string GetAlertQueryCriteria(
			Query q)
		{
			string state = MqlUtil.GetCriteriaString(q); // just criteria for now since non-criteria tables are not checked
			if (state.Length > 2000) state = state.Substring(0, 2000); // limit size for persistance into UserObject.Description
			return state;
		}

		static int GetWeekNumberOfMonth(DateTime date)
		{
			date = date.Date;
			DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
			DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
			if (firstMonthMonday > date)
			{
				firstMonthDay = firstMonthDay.AddMonths(-1);
				firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
			}
			return (date - firstMonthMonday).Days / 7 + 1;
		}

		public enum PatternEnum
		{
			Daily,
			Weekly,
			Monthly
		};

		public enum DaysEnum
		{
			Day,
			WeekDay,
			Saturday,
			Sunday,
			Monday,
			Tuesday,
			Wednesday,
			Thursday,
			Friday
		}
	}
}
