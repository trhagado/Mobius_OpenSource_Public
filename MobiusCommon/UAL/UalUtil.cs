using Mobius.ComOps; 
using Mobius.Data;

using Oracle.DataAccess.Client; 
using Oracle.DataAccess.Types;

using System;
using System.Windows.Forms;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net.Mail;

namespace Mobius.UAL
{
	/// <summary>
	/// Unified Access Layer (UAL) utility functions
	/// </summary>

	public class UalUtil
	{
		public static IClient IClient; // object to handle IClient calls
		public static IQueryExec IQueryExec; // QueryExec to handle exports

		public static bool LogExecutionTimes = false;

		/// <summary>
		/// Initialize the UAL
		/// </summary>

		public static void Initialize(string iniFileName)
		{
			ServicesDirs.ExecutablePath = Application.ExecutablePath;
			ServicesDirs.ServerStartupDir = Application.StartupPath;

			//DebugLog.Message("UalUtil.Initialize iniFileName: " + iniFileName);

			ServicesIniFile.Initialize(iniFileName);
			CommonConfigInfo.MiscConfigDir = ServicesIniFile.Read("MiscConfigDirectory");

			ServicesDirs.MetaDataDir = ServicesIniFile.Read("MetaDataDirectory");
			//DebugLog.Message("MetaDataDir: " + MetaDataDir);

			ServicesDirs.TargetMapsDir = ServicesDirs.MetaDataDir + @"\TargetMaps"; // target maps relative to MetaDataDir

			ServicesDirs.DocDir = ServicesIniFile.Read("DocumentDirectory");
			ServicesDirs.LogDir = ServicesIniFile.Read("LogDirectory");
			ServicesDirs.CacheDir = ServicesIniFile.Read("CacheDirectory");
			ServicesDirs.TempDir = TempFile.TempDir = ServicesIniFile.Read("TempDirectory");
			ServicesDirs.QueryResultsDataSetDir = ServicesIniFile.Read("QueryResultsDataSetDirectory");
			ServicesDirs.BackgroundExportDir = ServicesIniFile.Read("BackgroundExportDirectory");
			ServicesDirs.BinaryDataDir = ServicesIniFile.Read("BinaryDataDirectory");

			ServicesDirs.ModelQueriesDir = ServicesIniFile.Read("ModelQueriesDirectory");
			ServicesDirs.ModelQueriesDir = ServicesIniFile.Read("ModelQueriesDirectory");
			ServicesDirs.ImageDir = ServicesIniFile.Read("ImageDirectory");
			ServicesDirs.ImageDirUNC = ServicesIniFile.Read("ImageDirectoryUNC");

			LogExecutionTimes = ServicesIniFile.ReadBool("LogUalExecutionTimes", false);

			DbCommandMx.LogDatabaseCalls = ServicesIniFile.ReadBool("LogDatabaseCalls", DbCommandMx.LogDatabaseCalls);

			return;
		}

/// <summary>
/// Read the services iniFile
/// </summary>
/// <returns></returns>

		public static string GetServicesIniFile()
		{
			StreamReader sr = new StreamReader(ServicesIniFile.IniFilePath);
			string content = sr.ReadToEnd();
			sr.Close();
			return content;
		}

		/// <summary>
		/// Select an Oracle Blob value
		/// </summary>
		/// <param name="table"></param>
		/// <param name="matchCol"></param>
		/// <param name="typeCol"></param>
		/// <param name="contentCol"></param>
		/// <param name="matchVal"></param>

		public static void SelectOracleBlob(
				string table,
				string matchCol,
				string typeCol,
				string contentCol,
				string matchVal,
				out string typeVal,
				out byte[] ba)
		{
			typeVal = null;
			ba = null;

			string sql =
				"select " + typeCol + ", " + contentCol + " " +
				"from " + table + " " +
				"where " + matchCol + " = :0";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareMultipleParameter(sql, 1);

			for (int step = 1; step <= 2; step++) // try two different forms of matchVal
			{
				if (step == 2) // try alternate form of spaces
				{
					if (matchVal.Contains("%20"))
						matchVal = matchVal.Replace("%20", " "); // convert html spaces to regular spaces
					else matchVal = matchVal.Replace(" ", "%20"); // convert regular spaces to html spaces
				}

				drd.ExecuteReader(matchVal);
				if (!drd.Read()) continue;

				typeVal = drd.GetString(0);
				if (drd.Rdr.IsDBNull(1)) break;

				OracleBlob ob = drd.OracleRdr.GetOracleBlob(1);
				if (ob != null && ob.Length >= 0)
				{
					ba = new byte[ob.Length];
					ob.Read(ba, 0, (int)ob.Length);
				}

				break; // have value
			}

			drd.Dispose();
			return;
		}

		/// <summary>
		/// Select an Oracle Clob value
		/// </summary>
		/// <param name="table"></param>
		/// <param name="matchCol"></param>
		/// <param name="typeCol"></param>
		/// <param name="contentCol"></param>
		/// <param name="matchVal"></param>
		/// <param name="typeVal"></param>
		/// <param name="clobString"></param>

		public static void SelectOracleClob(
				string table,
				string matchCol,
				string typeCol,
				string contentCol,
				string matchVal,
				out string typeVal,
				out string clobString)
		{
			typeVal = null;
			clobString = null;

			string sql =
				"select " + typeCol + ", " + contentCol + " " +
				"from " + table + " " +
				"where " + matchCol + " = :0";

			DbCommandMx drd = new DbCommandMx();
			drd.PrepareMultipleParameter(sql, 1);

			for (int step = 1; step <= 2; step++) // try two different forms of matchVal
			{
				if (step == 2) // try alternate form of spaces
				{
					if (matchVal.Contains("%20"))
						matchVal = matchVal.Replace("%20", " "); // convert html spaces to regular spaces
					else matchVal = matchVal.Replace(" ", "%20"); // convert regular spaces to html spaces
				}

				drd.ExecuteReader(matchVal);
				if (!drd.Read()) continue;

				typeVal = drd.GetString(0);
				if (drd.Rdr.IsDBNull(1)) break;

				OracleClob oc = drd.OracleRdr.GetOracleClob(1);
				if (oc != null && oc.Length >= 0)
					clobString = oc.Value;

				break; // have value
			}

			drd.Dispose();
			return;
		}

		/// <summary>
		/// Send mail object
		/// </summary>
		/// <param name="mail"></param>

		public static void SendEmail(
				MailMessage mail)
		{
			SmtpClient mailClient = new SmtpClient();
			string mailServerName = ServicesIniFile.Read("SmtpServer");
			mailClient.Host = mailServerName;
			mailClient.UseDefaultCredentials = true;
			//						mailClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
			mailClient.Send(mail);
		}
	}

}
