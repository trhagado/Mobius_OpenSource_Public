using Mobius.Data;
using Mobius.ComOps;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;

namespace Mobius.ClientComponents
{
	public class BackgroundQuery
	{

		/// <summary>
		/// Start new session to run background query
		/// </summary>
		/// <param name="bgeUoId">The Id of the BackgroundExport userobject </param>
		/// <returns></returns>

		public static string SpawnBackgroundQuery(
			int queryId,
			bool emailWhenDone) 
		{
			string msg = "";

			string command = "RunQueryInBackground " + queryId + " " + emailWhenDone;
			try
			{
				CommandLine.StartBackgroundSession(command);
				msg = "The query has been started in the background.";
				if (emailWhenDone) msg += " A link to the results will be E-mailed to you when the query completes";
			}

			catch (Exception ex)
			{
				msg = "Couldn't start new Mobius session:\n" + ex.Message;
				ServicesLog.Message(DebugLog.FormatExceptionMessage(ex, msg));
			}

			return msg;
		}

		/// <summary>
		/// Command entry to execute a query in the background and save the results for later retrieval
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>

		public static string RunBackgroundQuery(
			string args)
		{
			UserObject uo = null;
			bool sendEmail = false;
			string emailSubject = null;
			string msg;
			int queryId = -1;

			string[] sa = args.Split(' ');
			if (sa.Length > 0)
			{
				if (int.TryParse(sa[0].Trim(), out queryId))
					uo = UserObjectDao.Read(queryId);
			}

			if (uo == null)
			{
				emailSubject = UmlautMobius.String + " background query results error";
				msg = "RunQueryInBackground failed to read query " + queryId;
				ServicesLog.Message(msg);
				Email.Send(
					null, SS.I.UserInfo.EmailAddress, emailSubject, msg);
				return msg;
			}

			if (sa.Length > 1)
				bool.TryParse(sa[1].Trim(), out sendEmail);

			Query q = Query.Deserialize(uo.Content);
			q.UserObject = uo; // copy user object to get current name, etc.
			q.IncludeRootTableAsNeeded();

			if (sendEmail)
				emailSubject = UmlautMobius.String + " background query results for " + Lex.Dq(q.UserObject.Name);
			return RunBackgroundQuery(q, emailSubject, "MobiusBackgroundQueryEmailTemplate.htm");
		}


/// <summary>
/// Method to run query in background and save the results for later retrieval
/// </summary>
/// <param name="q"></param>
/// <param name="emailSubject">Send email if defined</param>
/// <param name="templateName"></param>
/// <returns></returns>

		public static string RunBackgroundQuery(
			Query q,
			string emailSubject,
			string templateName)
		{
			ResultsFormat rf;
			QueryManager qm;
			DataTableManager dtm;
			string msg = "", html = "", resultsFileName;
			string viewCmd = "View Background Query Results";

			bool notifyUserByEmail = !Lex.IsNullOrEmpty(emailSubject);

			try // execute the query & read in all results
			{
				QbUtil.AddQueryAndRender(q, false); // add it to the query builder
				q.BrowseSavedResultsUponOpen = false; // be sure query is run rather than using existing results

				msg = QueryExec.RunQuery(q, OutputDest.WinForms);

				qm = q.QueryManager as QueryManager;
				dtm = qm.DataTableManager;
				DialogResult dr = dtm.CompleteRetrieval();
			}
			catch (Exception ex)  // some exceptions are normal, e.g. no criteria, others may be bugs
			{
				msg = "RunQueryInBackground could not complete due to an unexpected exception: " + DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				if (notifyUserByEmail)
					Email.Send(null, SS.I.UserInfo.EmailAddress, emailSubject, msg);

				return msg;
			}

			if (dtm.KeyCount == 0)
			{
				msg = "Query " + Lex.Dq(q.UserObject.Name) + " returned no results.";
				if (notifyUserByEmail)
					Email.Send(null, SS.I.UserInfo.EmailAddress, emailSubject, msg);
				return msg;
			}

			try
			{
				resultsFileName = q.ResultsDataTableFileName; // see if name supplied in query
				if (Lex.IsNullOrEmpty(resultsFileName)) resultsFileName = "Query_" + q.UserObject.Id + "_Results.bin";
				resultsFileName = ServicesIniFile.Read("BackgroundExportDirectory") + @"\" + resultsFileName;
				dtm.WriteBinaryResultsFile(resultsFileName); // write the file
				UserObject cidListUo = SaveBackgroundQueryResultsReferenceObject(qm, "BkgrndQry", resultsFileName);

				if (!Lex.IsNullOrEmpty(templateName)) html = ReadTemplateFile(templateName);

				if (notifyUserByEmail)
				{
					AlertUtil.MailResultsAvailableMessage( // send the mail
					 q,
					 dtm.KeyCount,
					 SS.I.UserInfo.EmailAddress,
					 emailSubject,
					 viewCmd,
					 cidListUo.Id,
					 null,
					 html);
				}

				else
				{
					html = SubstituteBackgroundExportParameters(html, "", "", dtm.RowCount, dtm.KeyCount, false, "");
					return html; // return the html
				}

			}
			catch (Exception ex)
			{
				msg = "Error sending background query results: " + DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
			}

			return msg;
		}

		/// <summary>
		/// Save list of hits and optional reference to a results file from a background query
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="listOwner"></param>
		/// <returns></returns>

		public static UserObject SaveBackgroundQueryResultsReferenceObject(
			QueryManager qm,
			string listOwner,
			string resultsFileName)
		{
			Query q = qm.Query;
			DataTableManager dtm = qm.DataTableManager;

			UserObject cidListUo = new UserObject(UserObjectType.CnList);
			cidListUo.Owner = listOwner;
			cidListUo.Id = UserObjectDao.GetNextId();
			cidListUo.Name = "List " + cidListUo.Id; // assign unique name
			cidListUo.Description = q.UserObject.Id + "\t" + // store query id
				DateTimeUS.ToString(DateTime.Now); // and a time stamp
			if (!Lex.IsNullOrEmpty(resultsFileName))
				cidListUo.Description += "\t" + resultsFileName;

			SortOrder sortDirection = (q.KeySortOrder > 0) ? SortOrder.Ascending : SortOrder.Descending;
			ResultsSorter.SortKeySet(dtm.ResultsKeys, sortDirection); // sort properly

			StringBuilder sb = new StringBuilder();
			foreach (string s in dtm.ResultsKeys)
			{ // build comma separated list of numbers
				if (sb.Length > 0) sb.Append("\r\n");
				sb.Append(s);
			}

			cidListUo.Content = sb.ToString();
			cidListUo.Count = dtm.ResultsKeys.Count;
			UserObjectDao.Write(cidListUo, cidListUo.Id); // write list with supplied id
			return cidListUo;
		}

		/// <summary>
		/// Start new session to do background export
		/// </summary>
		/// <param name="bgeUoId">The Id of the BackgroundExport userobject </param>
		/// <returns></returns>

		public static string SpawnBackgroundExport(int bgeUoId)
		{
			string msg = "";

			string command = "run background export " + bgeUoId;
			try // Example: Unattended IniFile = c:\\Mobius\\MobiusServer\\Server\\bin\\Debug\\MobiusServices.ini UserName = <userId> Command = 'run background export 919625'
			{
				CommandLine.StartBackgroundSession(command);
				msg =
					"The Background Export has been started.\n" +
					"You will receive an email when it completes.";
			}
			catch (Exception ex)
			{
				msg = "Failed to start background export: " + ex.Message;
				ServicesLog.Message(DebugLog.FormatExceptionMessage(ex, msg));
			}

			return msg;
		}

		/// <summary>
		///  This method runs in a background process and exports data. Results are emailed to the user
		/// </summary>
		/// <param name="objectIdString">Id of UserObject containing run parameters in a serialized ResultsFormat</param>
		/// <returns></returns>

		public static string RunBackgroundExport(
			string objectIdString)
		{
			string templateFileName = null;
			bool copyToFinalDest;

			return RunBackgroundExport(
				objectIdString,
				templateFileName,
				true,
				out copyToFinalDest);
		}

		/// <summary>
		/// This method runs in a background process and exports data to the specified destination
		/// </summary>
		/// <param name="objectIdString">Id of UserObject containing run parameters in a serialized ResultsFormat</param>
		/// <param name="templateFileName">Name of template file to use</param>
		/// <param name="emailResultsHtml">If true then send email otherwise just return html</param>
		/// <returns></returns>

		public static string RunBackgroundExport(
			string objectIdString,
			string templateFileName,
			bool emailResultsHtml,
      out bool copiedToDestinationFile,
      int alertId = 0)
		{
			ResultsFormat rf;
			Query q;
			string msg = "";
			int objId;

			//if (ClientState.IsDeveloper)
			//{
			//  ServicesLog.Message(SS.I.UserName + ": BackgrounExport Debug");
			//  //DataTableManager.AllowCaching = false;
			//  DataTableManager.DebugBasics = true;
			//  DataTableManager.DebugCaching = true;
			//}

			if (String.IsNullOrEmpty(templateFileName))
				templateFileName = "MobiusBackgroundExportEmailTemplate.htm";

			ServicesLog.Message("RunBackgroundExport started: UserObject id = " + objectIdString);
			string emailSubject = UmlautMobius.String + " background export results";

			copiedToDestinationFile = false;

			try
			{
				if (!int.TryParse(objectIdString, out objId))
					throw new Exception("Invalid UserObjectId");

				UserObject uo = UserObjectDao.Read(objId);
				if (uo == null) throw new Exception("UserObject not found");

				QueryManager qm = new QueryManager();
				rf = ResultsFormat.Deserialize(uo.Content);
				if (rf == null) throw new Exception("Failed to deserialize ResultsFormat");

				string clientFile = rf.OutputFileName; // ultimate file we want to go to

				rf.OutputFileName = GetServerFileName(rf, objId); // get file name to export to on server & use here temporarily

				qm.ResultsFormat = rf;
				rf.QueryManager = qm;

				q = QbUtil.ReadQuery(rf.QueryId);
				if (q == null) throw new Exception("Failed to read query: " + rf.QueryId);
				q.IncludeRootTableAsNeeded();
				qm.Query = q;
				q.QueryManager = qm;

				emailSubject += " for query " + Lex.Dq(q.UserObject.Name); // include query name in subject

				ResultsFormatFactory rff = new ResultsFormatFactory(rf);
				rff.Build();

				ResultsFormatter fmtr = new ResultsFormatter(qm);

				DataTableManager dtm = new DataTableManager(qm);
				dtm.BeginCaching(); // allow caching of DataTable
				dtm.PurgeDataTableWithoutWritingToCacheFile = true; // skip actual writing of cache since it won't be read back in

				qm.DataTableManager = dtm;

				qm.DataTable = DataTableManager.BuildDataTable(rf.Query); // build data table to receive data

				QueryExec qex = new QueryExec(rf);
				msg = qex.RunQuery3(rf, false, false); // do the export

				int compoundCount = 0;
				int rowCount = 0;

				QueryEngine qe = qex.QueryEngine;
				if (qe != null)
				{
					compoundCount = qm.DataTableManager.KeyCount;
					rowCount = qm.DataTableManager.TotalRowsTransferredToDataTable; // use this for accurate row count
				}

				dtm.EndCaching(); // close cache file (note: resets key/row counts)

				if (compoundCount <= 0 || rowCount <= 0) // any results
				{
					msg = "Query " + Lex.Dq(q.UserObject.Name) + " returned no results.";
					Email.Send(
						null, SS.I.UserInfo.EmailAddress, emailSubject, msg);
					return msg;
				}

				if (ServerFile.CanWriteFileFromServiceAccount(clientFile)) 
				{ // copy to dest file if possible
					try
					{
						FileUtil.CopyFile(rf.OutputFileName, clientFile);
						copiedToDestinationFile = true;
						rf.OutputFileName = clientFile;
					}
					catch (Exception ex) 
					{ 
						ServicesLog.Message("Error copying file from service account: " + clientFile + "\n" + DebugLog.FormatExceptionMessage(ex)); 
					}
				}

				string viewCmd = "Retrieve Background Export " + uo.Id;
				msg = "RunBackgroundExport ended: UserObjectId = " + objectIdString;

				if (emailResultsHtml)
				{
					MailBackgroundExportResults(
						q,
						clientFile,
						rowCount,
						compoundCount,
						copiedToDestinationFile,
						viewCmd,
						SS.I.UserInfo.EmailAddress,
						emailSubject,
						templateFileName);

					ServicesLog.Message(msg);
					return msg;
				}

				else // just fill in values & return
				{
					string html = ReadTemplateFile(templateFileName);
					html = SubstituteBackgroundExportParameters(
						html, "", rf.OutputFileName, rowCount, compoundCount, copiedToDestinationFile, viewCmd);
					ServicesLog.Message(msg);
					return html;
				}

			}

			catch (Exception ex)
			{
			    if (alertId > 0) msg += "Alert: " + alertId + " ";
                msg +=
				"RunBackgroundExport exception: BackgroundExportId = " + objectIdString + ",\r\n" +
					DebugLog.FormatExceptionMessage(ex);
				Email.Send(
					null, SS.I.UserInfo.EmailAddress, emailSubject, msg);
				ServicesLog.Message(msg);
				return msg;
			}
		}

/// <summary>
/// Get server file name to export to
/// </summary>
/// <param name="rf"></param>
/// <param name="objId"></param>
/// <returns></returns>

		static string GetServerFileName(ResultsFormat rf, int objId)
		{
			string clientFile = rf.OutputFileName; // file name on client
			string ext = Path.GetExtension(clientFile);
			string serverExpDir = ServicesIniFile.Read("BackgroundExportDirectory"); 
			string serverFile = serverExpDir + @"\" + objId + ext; // use background export userobject id & client file extension
			return serverFile;
		}

		/// <summary>
		/// Get the ResultsFormat and Query associated with the supplied BackgroundExport UserObject
		/// </summary>
		/// <param name="uo">BackgroundExport UserObject</param>
		/// <param name="q"></param>
		/// <param name="rf"></param>

		public static void GetAssociatedResultsFormatandQuery(
			UserObject uo,
			out ResultsFormat rf,
			out Query q)
		{
			q = null;
			//			Alert a = null;
			rf = null;

			if (uo.Type == UserObjectType.BackgroundExport)
			{ // background export containing ResultsFormat in content
				rf = ResultsFormat.Deserialize(uo.Content);
				q = rf.Query = QbUtil.ReadQuery(rf.QueryId);
				if (q == null) throw new Exception("Failed to read query: " + rf.QueryId);
				//				a = Alert.GetExistingAlertForQuery(rf.QueryId);
			}
#if false
			else if (uo.Type == UserObjectType.Alert)
			{ // alert possibly containing ExportOptions for export of full results to file
				a = Alert.Deserialize(uo);
				q = QbUtil.ReadQuery(a.QueryObjId);
				if (q == null) throw new Exception("Failed to read query: " + a.QueryObjId);

				rf = a.ExportParms;
				if (rf == null) throw new Exception("ResultsFormat not defined");
				rf.Query = q;
				rf.RunInBackground = true;
			}

			else if (uo.Type == UserObjectType.CnList)
			{ // Compound Id list from alert run, description contains queryId
				string[] sa = uo.Description.Split('\t');
				int qId = Int32.Parse(sa[0]);
				q = QbUtil.ReadQuery(qId);
				if (q == null) throw new Exception("Failed to read query: " + rf.QueryId);

				a = Alert.GetExistingAlertForQuery(qId);
				if (a == null) throw new Exception("Alert not found for query: " + qId);

				rf = a.ExportParms;
				if (rf == null) throw new Exception("ResultsFormat not defined");
				rf.Query = q;
				rf.RunInBackground = true;
			}
#endif
			else throw new Exception("Invalid UserObject type: " + uo.Type.ToString());

			return;
		}

		public static void MailBackgroundExportResults(
			Query q,
			string fileName,
			int rowCount,
			int compoundCount,
			bool copiedToDestinationFile,
			string viewCmd,
			string mailTo,
			string mailSubject,
			string templateFileName)
		{
			MailMessage mail = new MailMessage();
			mail.From = new MailAddress("Mobius@[server]", UmlautMobius.String);
			mail.Subject = mailSubject;
			Email.SetMultipleMailTo(mail, mailTo);

			string queryLabel = q.UserObject.Name;
			string txt;
			try
			{
				txt = SecurityUtil.GetShortPersonNameReversed(q.UserObject.Owner);  // include owner's name
				queryLabel += " (" + txt + ")";
			}
			catch (Exception ex) { }

			string html = ReadTemplateFile(templateFileName);
			html = SubstituteBackgroundExportParameters(
				html, queryLabel, fileName, rowCount, compoundCount, copiedToDestinationFile, viewCmd);

			mail.Body = html;
			mail.IsBodyHtml = true;

			Email.Send(mail);
			return;
		}

		/// <summary>
		/// Read in a template file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static string ReadTemplateFile(
			string fileName)
		{
			if (!fileName.Contains(@"\"))
				fileName = CommonConfigInfo.MiscConfigDir + @"\" + fileName;

			try
			{
				StreamReader sr = new StreamReader(fileName);
				string content = sr.ReadToEnd();
				sr.Close();
				return content;
			}
			catch (Exception ex) { ex = ex; }

			return "Unable to find file: " + fileName;
		}

		/// <summary>
		/// SubstituteBackgroundExportParameters
		/// </summary>
		/// <param name="sourceHtml"></param>
		/// <param name="queryLabel"></param>
		/// <param name="fileName"></param>
		/// <param name="rowCount"></param>
		/// <param name="compoundCount"></param>
		/// <param name="viewCmd"></param>
		/// <returns></returns>

		public static string SubstituteBackgroundExportParameters(
			string sourceHtml,
			string queryLabel,
			string fileName,
			int rowCount,
			int compoundCount,
			bool copiedToDestinationFile,
			string viewCmd)
		{
			string html = sourceHtml.Replace("query-name", queryLabel);
			html = html.Replace("file-name", fileName);
			html = html.Replace("row-count", rowCount.ToString());
			html = html.Replace("compound-count", compoundCount.ToString());
			if (copiedToDestinationFile) html = Lex.Replace(html, "prepared for download", "downloaded");

			string viewLink = "Mobius:Command=" + Lex.AddSingleQuotes(viewCmd); // Uses Mobius pluggable protocol
			html = html.Replace(@"file:///\\retrieve-background-export-link", viewLink);
			html = html.Replace("retrieve-background-export-command", viewCmd);
			return html;
		}

		/// <summary>
		/// Retrieve the results of a background export
		/// Example: Retrieve Background Export 231243
		/// </summary>
		/// <param name="objectIdString">Id of BackgroundExport UserObject containing serialized ResultsFormat</param>
		/// <returns></returns>

		public static string RetrieveBackgroundExport(
				string objectIdString)
		{
			ResultsFormat rf;
			Query q;
			int objId;

			QbUtil.SetMode(QueryMode.Build); // be sure in build mode

			if (!int.TryParse(objectIdString, out objId))
				return "Invalid UserObjectId: " + objectIdString;

			UserObject uo = UserObjectDao.Read(objId);
			if (uo == null)
				return "RunInBackground UserObject not found: " + objectIdString;
			if (uo.Type != UserObjectType.BackgroundExport)
				return "Object is not the expected RunInBackground UserObject type";

			rf = ResultsFormat.Deserialize(uo.Content);
			if (rf == null) throw new Exception("Failed to deserialize ResultsFormat: " + objectIdString);

			string clientFile = rf.OutputFileName;
			string serverFile = GetServerFileName(rf, objId);

			string ext = Path.GetExtension(clientFile);
			string filter = "*" + ext + "|*" + ext;

			if (SharePointUtil.IsRegularFileSystemName(clientFile)) 
				clientFile =	UIMisc.GetSaveAsFilename("Retrieve Background Export File", clientFile, filter, ext);

			else clientFile =
				SharePointFileDialog.GetSaveAsFilename("Retrieve Background Export File", clientFile, filter, ext);

			if (String.IsNullOrEmpty(clientFile)) return "";

			Progress.Show("Retrieving file...");
			try { ServerFile.CopyToClient(serverFile, clientFile); }
			catch (Exception ex)
			{
				string msg =
					"Unable to retrieve cached export file: " + serverFile + "\n" +
					"to client file: " + clientFile + "\n" +
					DebugLog.FormatExceptionMessage(ex);
				ServicesLog.Message(msg);
				Progress.Hide();
				return msg;
			}

			Progress.Hide();

			if (Lex.Eq(ext, ".xls") || Lex.Eq(ext, ".xlsx") || Lex.Eq(ext, ".doc") || Lex.Eq(ext, ".docx"))
			{
				DialogResult dr = MessageBoxMx.Show("Do you want to open " + clientFile + "?",
					UmlautMobius.String, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr == DialogResult.Yes)
					SystemUtil.StartProcess(clientFile);
			}

			return "Background export file retrieved: " + clientFile;
		}
	}
}
