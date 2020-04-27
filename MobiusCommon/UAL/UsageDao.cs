using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections;
using System.Data;
using System.Text;
using System.IO;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{
	/// <summary>
	/// Methods for updating & analyzing usage log
	/// </summary>

	public class UsageDao
	{
		// The following are used for stats generation

		static Hashtable TableData; // number of times each table accessed
		static Hashtable CommandData; // number of times each command accessed
		static Hashtable TransactionData; // count for each transaction type
		static Hashtable UserDataHash; // info on each user, site, department

		public UsageDao()
		{
			return;
		}

		/// <summary>
		/// Log a usage event
		/// </summary>
		/// <param name="eventName"></param>
		public static void LogEvent(
			string eventName)
		{
			LogEvent(eventName, "", 0);
		}

		/// <summary>
		/// Log a usage event
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="eventData"></param>

		public static void LogEvent(
			string eventName,
			string eventData)
		{
			LogEvent(eventName, eventData, 0);
		}

		/// <summary>
		/// Log a usage event
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="eventData"></param>
		/// <param name="eventNumber"></param>

		public static void LogEvent(
			string eventName,
			string eventData,
			int eventNumber)
		{
			const int LogEntryType_Usage = 1;

			UserObject uo = new UserObject();
			uo.Type = (UserObjectType)LogEntryType_Usage;
			uo.Owner = Security.UserInfo.UserName;
			uo.Name = "UsageLog";
			uo.Description = eventName;
			uo.Content = eventData;
			uo.Count = eventNumber;
			bool result = UsageDao.Insert(uo);

			return;
		}

		/// <summary>
		/// Insert row into log table
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool Insert(
			UserObject uo)
		{

			if (DbConnectionMx.NoDatabaseAccessIsAvailable)
				return true; // don't try to write

			try
			{
				int t1 = TimeOfDay.Milliseconds();
				string sql = @"
					insert into mbs_owner.mbs_log (
						obj_id, 
						obj_typ_id, 
						ownr_id, 
						obj_nm, 
						obj_desc_txt, 
						fldr_typ_id, 
						fldr_nm, 
						acs_lvl_id, 
						obj_itm_cnt, 
						obj_cntnt, 
						chng_op_cd, 
						chng_usr_id, 
						crt_dt, 
						upd_dt) 
					values (:0, :1, :2, :3, :4, :5, :6, :7, :8, :9, 'I', :2, sysdate, sysdate)";

				// Note that chng_usr_id is truncated to 12 chars to avoid exceptions for longer user ids. Fix after column is expanded

				DbCommandMx drDao = new DbCommandMx();

				OracleDbType[] pa = new OracleDbType[10];

				pa[0] = OracleDbType.Int32;
				pa[1] = OracleDbType.Int32;
				pa[2] = OracleDbType.Varchar2;
				pa[3] = OracleDbType.Varchar2;
				pa[4] = OracleDbType.Varchar2;
				pa[5] = OracleDbType.Int32;
				pa[6] = OracleDbType.Varchar2;
				pa[7] = OracleDbType.Int32;
				pa[8] = OracleDbType.Int32;
				pa[9] = OracleDbType.Clob;

				drDao.Prepare(sql, pa);

				object[] p = new object[10];
				uo.Id = SequenceDao.NextVal("mbs_owner.mbs_log_seq");
				p[0] = uo.Id;
				p[1] = (int)uo.Type;
				p[2] = (Lex.IsDefined(uo.Owner) ? uo.Owner.ToUpper() : "UNKNOWN");
				p[3] = uo.Name;
				p[4] = (Lex.IsUndefined(uo.Description) ? uo.Description : " ");
				p[5] = uo.ParentFolderType;
				p[6] = uo.ParentFolder;
				p[7] = uo.AccessLevel;
				p[8] = uo.Count;
				p[9] = uo.Content;
				//					if (uo.Content!="")	p[9] = uo.Content;
				//					else p[9]=null; 

				int count = drDao.ExecuteNonReader(p); // insert the row
				drDao.Dispose();

				int t2 = TimeOfDay.Milliseconds();
				int time = t2 - t1; // 80ms 5/24/05

				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		/// <summary>
		/// Open select statement
		/// </summary>
		/// <param name="where"></param>
		/// <param name="orderBy"></param>
		/// <returns></returns>

		public static DbCommandMx Select(
			string where,
			string orderBy)
		{
			string sql = @"
				select  
					obj_id,      
					obj_typ_id,           
					ownr_id,              
					obj_nm,               
					obj_desc_txt,         
					fldr_typ_id,             
					fldr_nm,              
					acs_lvl_id,           
					obj_itm_cnt,          
					obj_cntnt,            
					crt_dt,               
					upd_dt               
				from mbs_owner.mbs_log " + "\r\n" +
				where + " \r\n" +
				//" where obj_id between 9300232 and 9301232 " + // debug
				orderBy;

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql);
			drd.ExecuteReader();
			return drd;

		}

		/// <summary>
		/// Fetch next log object for reader opened by Select method
		/// </summary>
		/// <param name="drd"></param>
		/// <returns></returns>

		public static UserObject Read(
			DbCommandMx drd)
		{
			OracleDataReader dr = drd.OracleRdr;
			if (!dr.Read()) return null;

			UserObject uo = new UserObject();
			if (!dr.IsDBNull(0)) uo.Id = (int)dr.GetOracleDecimal(0);
			if (!dr.IsDBNull(1)) uo.Type = (UserObjectType)(int)dr.GetOracleDecimal(1);
			if (!dr.IsDBNull(2)) uo.Owner = dr.GetString(2);
			if (!dr.IsDBNull(3)) uo.Name = dr.GetString(3);
			if (!dr.IsDBNull(4)) uo.Description = dr.GetString(4);
			if (!dr.IsDBNull(5)) uo.ParentFolderType = (FolderTypeEnum)(int)dr.GetOracleDecimal(5);
			if (!dr.IsDBNull(6)) uo.ParentFolder = dr.GetString(6);
			if (!dr.IsDBNull(7)) uo.AccessLevel = (UserObjectAccess)(int)dr.GetOracleDecimal(7);
			if (!dr.IsDBNull(8)) uo.Count = (int)dr.GetOracleDecimal(8);
			if (!dr.IsDBNull(9))
			{
				OracleClob ol = dr.GetOracleClob(9);
				uo.Content = ol.Value.ToString();
			}
			if (!dr.IsDBNull(10)) uo.UpdateDateTime = dr.GetDateTime(10);

			return uo;
		}

		/// <summary>
		/// Read log object given the object id
		/// </summary>
		/// <param name="objectId"></param>
		/// <returns></returns>

		public static UserObject Read( // read object
			int objectId) // id of item to read
		{
			string sql =
				"select obj_id, obj_typ_id, ownr_id, obj_nm, obj_desc_txt, " +
				"fldr_typ_id, fldr_nm, acs_lvl_id, obj_itm_cnt, obj_cntnt, crt_dt, upd_dt " +
				"from mbs_owner.mbs_log where obj_id=:0";

			DbCommandMx drd = new DbCommandMx();
			drd.Prepare(sql, OracleDbType.Int32);
			drd.ExecuteReader(objectId);

			UserObject uo = UserObjectDao.FetchUserObject(drd);
			drd.Dispose();
			return uo;
		}

		/// <summary>
		/// Commandline command to analyse usage data
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		public static string AnalyzeUsageData(
			string commandArgs)
		{
			string currentUserid = "";
			DateTime startTime = DateTime.MinValue, finishTime;
			int count, pass, n, rowCount = 0;
			int i1, i2;
			string txt, rec, buf;
			string sql, sqlmsg, stmt, table;
			int objid, curobj, emp_id;
			int noMatchingBegin = 0;
			DbCommandMx drd;

			Lex lex = new Lex();
			lex.OpenString(commandArgs);
			string startDate = lex.Get();
			string endDate = lex.Get();
			string startDate2 = DateTimeMx.Normalize(startDate);
			string endDate2 = DateTimeMx.Normalize(endDate);
			if (startDate2 == null || endDate2 == null)
				throw new Exception("Syntax: Analyze Usage Data <start_date> <end_date>");

			startDate2 = "to_date('" + startDate2 + "','YYYYMMDD')";
			endDate2 = "to_date('" + endDate2 + " 235959','YYYYMMDD HH24MISS')";

			string where = "where crt_dt between " + startDate2 + " and " + endDate2;

			string arg = lex.Get();
			if (Lex.Eq(arg, "DumpUsageData"))
				return DumpUsageData(where);

			// Init data

			TableData = new Hashtable(); // number of times each table accessed
			CommandData = new Hashtable(); // number of times each command accessed
			TransactionData = new Hashtable(); // count for each transaction type
			UserDataHash = new Hashtable(); // info on each user, site, department

			// Process usage log records. Find the users for the time period,
			// how many times each user accessed app and the average 
			// session time in minutes for each user

			string orderBy = "order by ownr_id, crt_dt";
			drd = UsageDao.Select(where, orderBy);

			UserObject beginUo = new UserObject();

			while (true)
			{
				UserObject uo = UsageDao.Read(drd);
				if (uo == null)
				{
					drd.Dispose();
					if (rowCount > 0) break;
					throw new Exception("No data found for specified for time period");
				}

				if (uo.Owner == null || uo.Owner.Trim().Length == 0)
					continue; // skip if owner not specified

				if (rowCount % 1000 == 0)
				{
					UAL.Progress.Show("Analyzing Usage Data, rows: " + rowCount + " ...", UmlautMobius.String, false);
				}

				rowCount++;

				string eventName = uo.Description;
				string eventData = uo.Content;

				// Increment count on event type

				object o = TransactionData[eventName];
				if (o == null)
				{
					TransactionData.Add(eventName, 0);
					count = 0;
				}
				else count = (int)o;
				TransactionData[eventName] = count + 1;

				// Beginning of session?

				if (Lex.Eq(eventName, "Begin"))
				{
					beginUo = uo;
					currentUserid = uo.Owner;
					startTime = uo.UpdateDateTime;
					UserData(currentUserid).Count++;
				}

				else if (Lex.Eq(eventName, "SSS") ||
					eventName.ToLower().StartsWith("strsrch sss"))
					UserData(uo.Owner).SsCount++;

				else if (Lex.Eq(eventName, "MSimilar") ||
					eventName.ToLower().StartsWith("strsrch msimilar"))
					UserData(uo.Owner).SimCount++;

				else if (eventName.StartsWith("QueryGrid")) // get QueryGridAnd, Complex & Or 
					UserData(uo.Owner).QueryCount++;

				else if (Lex.Eq(eventName, "TableStats") && uo.Content != null)
				{
					string[] sa = uo.Content.Split('\n');
					for (i1 = 0; i1 < sa.Length; i1++)
					{
						if (sa[i1] == null || sa[i1] == "" || sa[i1] == "") continue;
						string[] sa2 = sa[i1].Split('\t'); // split into table name & count
						if (sa2.Length != 2) continue;
						o = TableData[sa2[0]]; // lookup table
						if (o == null)
						{
							TableData.Add(sa2[0], null);
							count = 0;
						}
						else count = (int)o;
						TableData[sa2[0]] = count + Int32.Parse(sa2[1]);
					}
				}

				else if (Lex.Eq(eventName, "CommandStats") && uo.Content != null)
				{
					string[] sa = uo.Content.Split('\n');
					for (i1 = 0; i1 < sa.Length; i1++)
					{
						if (sa[i1] == null || sa[i1] == "" || sa[i1] == "") continue;
						string[] sa2 = sa[i1].Split('\t'); // split into table name & count
						if (sa2.Length != 2) continue;
						o = CommandData[sa2[0]]; // lookup table
						if (o == null)
						{
							CommandData.Add(sa2[0], null);
							count = 0;
						}
						else count = (int)o;
						CommandData[sa2[0]] = count + Int32.Parse(sa2[1]);
					}
				}

				else if (Lex.Eq(eventName, "End"))
				{
					if (uo.Owner == currentUserid) // same user?
					{
						UserData(currentUserid).Ended++;

						TimeSpan elapsed = uo.UpdateDateTime.Subtract(startTime);
						UserData(currentUserid).TotalTime += elapsed.Minutes;
					}
					else noMatchingBegin++;

					currentUserid = "";
				}

			} // end of main loop

			// Calculate totals

			UserDataVo totalVo = UserData("=Total=");
			foreach (UserDataVo vo in UserDataHash.Values)
			{
				if (vo.Userid == totalVo.Userid) continue;

				totalVo.Users++;
				totalVo.Count += vo.Count;
				totalVo.Ended += vo.Ended;
				totalVo.SsCount += vo.SsCount;
				totalVo.SimCount += vo.SimCount;
				totalVo.QueryCount += vo.QueryCount;
				totalVo.TotalTime += vo.TotalTime;
			}

			// Calculate site totals


			foreach (UserDataVo vo in UserDataHash.Values)
			{
				if (vo.Users > 0) continue; // just individuals
				if (vo.Site == "") continue; // ignore if no site info

				totalVo.Users++;
				totalVo.Count += vo.Count;
				totalVo.Ended += vo.Ended;
				totalVo.SsCount += vo.SsCount;
				totalVo.SimCount += vo.SimCount;
				totalVo.QueryCount += vo.QueryCount;
				totalVo.TotalTime += vo.TotalTime;
			}

			// Order user data by descending usage count

			ArrayList UserDataList = new ArrayList(UserDataHash.Values);

			UserDataVo ud1, ud2;
			for (i1 = 1; i1 < UserDataList.Count; i1++)
			{
				ud1 = (UserDataVo)UserDataList[i1];
				for (i2 = i1 - 1; i2 >= 0; i2--)
				{
					ud2 = (UserDataVo)UserDataList[i2];
					if (ud1.Count < ud2.Count) break;
					UserDataList[i2 + 1] = UserDataList[i2];
				}
				UserDataList[i2 + 1] = ud1;
			}

			// Output user info

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Mobius Usage Statistics for " + startDate + " through " + endDate);
			sb.AppendLine("");

			string format, s;
			object[] args;

			format = "{0,-38} {1,5} {2,4} {3,7} {4,4} {5,4}";
			args = new object[] { "User", "Uses", "Time", "Queries", "SSS", "Sim" };
			s = String.Format(format, args);
			sb.AppendLine(s);

			args = new object[] { "--------------------------------------", "-----", "----", "-------", "----", "----" };
			s = String.Format(format, args);
			sb.AppendLine(s);

			for (pass = 1; pass <= 2; pass++) // do summary values first, then users
			{
				for (int ui = 0; ui < UserDataList.Count; ui++)
				{
					UserDataVo vo = (UserDataVo)UserDataList[ui];
					if (pass == 1 && vo.Users == 0) continue; // skip users on first past
					else if (pass == 2 && vo.Users > 0) continue; // skip totals on 2nd pass
					if (vo.Ended == 0) vo.AverageTime = 0;
					else vo.AverageTime = vo.TotalTime / vo.Ended; // avg. Time (min) for properly ended sessions

					if (vo.Users > 0) // summary
						txt = vo.Userid + " (" + vo.Users.ToString() + " users)";

					else
					{ // single user 
						try
						{
							UserInfo sui = Security.GetUserInfo(vo.Userid);
							txt = sui.LastName + ", " + sui.FirstName;
							txt += " (" + vo.Userid + ")";
						}
						catch (Exception ex) { txt = vo.Userid; }

						if (vo.Site != "") txt += " (" + vo.Site + ")";
					}

					args = new object[] { txt, vo.Count, vo.AverageTime, vo.QueryCount, vo.SsCount, vo.SimCount };
					s = String.Format(format, args);
					sb.AppendLine(s);
				}
			}

			// Output table usage stats

			sb.AppendLine("");
			format = "{0,-18} {1,8}";
			s = String.Format(format, "MetaTable", "Accesses");
			sb.AppendLine(s);
			s = String.Format(format, "------------------", "--------");
			sb.AppendLine(s);

			ArrayList al = new ArrayList();
			foreach (string key in TableData.Keys)
			{
				s = String.Format(format, key, TableData[key]);
				al.Add(s);
			}
			al.Sort();
			foreach (string s2 in al)
				sb.AppendLine(s2);

			// Output command usage stats

			sb.AppendLine("");
			format = "{0,-18} {1,8}";
			s = String.Format(format, "Command", "Accesses");
			sb.AppendLine(s);
			s = String.Format(format, "------------------", "--------");
			sb.AppendLine(s);

			al = new ArrayList();
			foreach (string key in CommandData.Keys)
			{
				s = String.Format(format, key, CommandData[key]);
				al.Add(s);
			}
			al.Sort();
			foreach (string s2 in al)
				sb.AppendLine(s2);

			// Output transaction counts

			sb.AppendLine("");
			format = "{0,-18} {1,8}";
			s = String.Format(format, "Transaction", "Count");
			sb.AppendLine(s);
			s = String.Format(format, "------------------", "--------");
			sb.AppendLine(s);

			al = new ArrayList();
			foreach (string key in TransactionData.Keys)
			{
				s = String.Format(format, key, TransactionData[key]);
				al.Add(s);
			}
			al.Sort();
			foreach (string s2 in al)
				sb.AppendLine(s2);

			return sb.ToString();
		}

		/// <summary>
		/// Dump out usage data for analysis (e.g. Usage 19-apr-2013 19-apr-2013 DumpUsageData)
		/// </summary>
		/// <param name="where"></param>
		/// <returns></returns>

		public static string DumpUsageData(string where)
		{
			string rec;
			int readCount = 0, writeCount = 0;

			//where += " and obj_desc_txt like 'Query%'"; // limit to query data
			string orderBy = "order by ownr_id, crt_dt";
			DbCommandMx drd = UsageDao.Select(where, orderBy);
			string fileName = ServicesDirs.LogDir + @"\QueryStatsDump.csv";
			StreamWriter sw = new StreamWriter(fileName);
			sw.WriteLine("Owner, DateTime, Event, EventData");

			Progress.Show("Executing query...");

			while (true)
			{
				UserObject uo = UsageDao.Read(drd);
				if (uo == null)
				{
					drd.Dispose();
					sw.Close();
					Progress.Hide();
					if (readCount == 0)
						throw new Exception("No data found for specified for time period");

					return "Rows written to file " + fileName + ": " + writeCount;
				}

				if (uo.Owner == null || uo.Owner.Trim().Length == 0)
					continue; // skip if owner not specified

				readCount++;
				if (readCount % 1000 == 0)
					Progress.Show("Rows read: " + readCount + "...");

				string eventName = uo.Description;
				string eventData = uo.Content;

				if (Lex.Eq(eventName, "QueryEngineStats")) // multiple lines for QE stats
				{
					string[] sa = eventData.Split('\n');
					foreach (string s in sa)
					{
						if (s == "") continue;
						rec = uo.Owner + "," + uo.UpdateDateTime.ToString() + "," + eventName + "," + s;
						sw.WriteLine(rec);
						writeCount++;
					}

				}

				else if (Lex.Eq(eventName, "CommandStats")) continue; // ignore these

				else
				{
					rec = uo.Owner + "," + uo.UpdateDateTime.ToString() + "," + eventName + "," + eventData;
					sw.WriteLine(rec);
					writeCount++;
				}

			}

		}

		/// <summary>
		/// Return vo for user, adding if necessary
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>

		static UserDataVo UserData(
			string userId)
		{
			object o = UserDataHash[userId];
			if (o != null) return (UserDataVo)o;

			UserDataVo vo = new UserDataVo();
			vo.Userid = userId;

			UserDataHash.Add(userId, vo);
			return vo;
		}

	} // end of UsageDao

	/// <summary>
	/// Class to contain usage data during analysis
	/// </summary>

	class UserDataVo
	{
		public string Userid = "";
		public string Site = "";
		public int Users; // number of users if site
		public int Count; // session count
		public int Ended; // session count for successfully ended sessions
		public int SsCount; // substructure searches count
		public int SimCount; // similarity search count
		public int QueryCount;
		public int TotalTime; // in minutes 
		public int AverageTime; // average time per session
	}

}
