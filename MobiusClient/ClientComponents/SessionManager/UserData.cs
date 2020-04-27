using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Edit user databases and annotations
	/// </summary>
	/// 
	public class UserData
	{
		/// <summary>
		/// Create a new annotation
		/// </summary>
		/// <returns></returns>

		public static MetaTable CreateNewAnnotationTable()
		{
			UserDataEditor editor = new UserDataEditor();
			UserObject uo = new UserObject(UserObjectType.Annotation);
			uo = editor.Edit(uo);
			if (uo == null) return null;

			string tName = "ANNOTATION_" + uo.Id.ToString();
			MetaTable mt = MetaTableCollection.Get(tName);
			return mt;
		}

		/// <summary>
		/// Let the user select an existing annotation and edit it
		/// </summary>
		/// <returns></returns>

		public static MetaTable OpenExistingAnnotationTable(
				UserObject uo)
		{
			if (uo == null) // prompt if not supplied
				uo = UserObjectOpenDialog.ShowDialog(UserObjectType.Annotation, "Open Annotation");
			if (uo == null) return null;

			if (!Permissions.UserHasWriteAccess(SS.I.UserName, uo))
			{
				MessageBoxMx.ShowError("You are not authorized to edit this annotation table.");
				return null;
			}

			UserDataEditor editor = new UserDataEditor();
			UserObject uo2 = editor.Edit(uo);
			if (uo2 == null) return null;

			string tName = "ANNOTATION_" + uo.Id.ToString();
			MetaTable mt = MetaTableCollection.Get(tName); // return new version of metatable
			return mt;
		}

		/// <summary>
		/// Prompt user for annotation and return associated metatable
		/// </summary>
		/// <param name="caption"></param>
		/// <returns></returns>

		public static MetaTable SelectAnnotationTable(
				string caption)
		{
			UserObject uo = UserObjectOpenDialog.ShowDialog(UserObjectType.Annotation, caption);
			if (uo == null) return null;
			uo = UserObjectDao.Read(uo);
			MetaTable mt = MetaTable.Deserialize(uo.Content); // deserialize metatable xml
			return mt;
		}

		/// <summary>
		/// See if current user can modify supplied annotation table or calc field
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool CanModifyTable(
				MetaTable mt)
		{
			if (mt.MetaBrokerType == MetaBrokerType.Unknown) return true; // Allow if no broker type (i.e. Transient Conditional Formatting control, etc)

			else if (mt.MetaBrokerType == MetaBrokerType.Annotation || mt.MetaBrokerType == MetaBrokerType.CalcField)
			{
				if (mt.Code == null || mt.Code == "" || mt.Code == "0")
					return true; // assume ok if new object with no code assigned

				return UserObjectUtil.UserHasWriteAccess(SS.I.UserName, Int32.Parse(mt.Code)); // the code for the table is the object id
			}

			else if (mt.IsUserDatabaseStructureTable)
			{
				if (mt.Code == null || mt.Code == "" || mt.Code == "0")
					return true; // assume ok if new object with no code assigned

				long databaseId = Int64.Parse(mt.Code);

				UserCmpndDbDao udbs = new UserCmpndDbDao();
				return udbs.CanModifyDatabase(databaseId, SS.I.UserName);
			}

			else return false;
		}

		/// <summary>
		/// Create a new user database
		/// </summary>
		/// <returns></returns>

		public static UserObject CreateNewUserDatabase()
		{
			UserDataEditor editor = new UserDataEditor();
			UserObject uo = new UserObject(UserObjectType.UserDatabase);
			uo = editor.Edit(uo);
			return uo;
		}

		public static UserObject OpenExistingUserDatabase(
				UserObject uo)
		{
			if (uo == null) // prompt if not supplied
				uo = UserObjectOpenDialog.ShowDialog(UserObjectType.UserDatabase, "Open User Compound Database");
			if (uo == null) return null;

			UserDataEditor editor = new UserDataEditor();
			uo = editor.Edit(uo);
			return uo;
		}

		/// <summary>
		/// Build a basic database query for the user database
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		internal static bool BuildDatabaseQuery(
				UserObject uo)
		{
			List<UserObject> luo = UserDataEditor.GetUcdbUserObjects(uo.Id);
			if (luo == null || luo.Count == 0)
			{
				MessageBoxMx.ShowError("No tables found for database");
				return false;
			}

			Query q = new Query();
			foreach (UserObject uo2 in luo)
			{
				if (!UserObject.IsMetaTableType(uo2.Type)) continue;

				string mtName = uo2.Type.ToString() + "_" + uo2.Id;
				MetaTable mt = MetaTableCollection.Get(mtName);
				if (mt == null) continue;
				QueryTable qt = new QueryTable(mt);
				q.AddQueryTable(qt);
			}

			QbUtil.NewQuery(uo.Name);
			QbUtil.SetCurrentQueryInstance(q);
			return true;
		}

		static int ParseRowNumber(
				string txt)
		{
			string[] sa = txt.Split(' ');
			int row = Int32.Parse(sa[1]);
			return row;
		}

		static string SerializeDatabaseHeaderForGrid(
				List<UcdbUoAssoc> ucdbUoAssoc,
				int position,
				UserCmpndDbDao udbs)
		{
			UcdbDatabase ucdb = ucdbUoAssoc[position].Ucdb;
			StringBuilder sb = new StringBuilder();

			sb.Append((position + 1).ToString());
			sb.Append("\t");
			sb.Append(ucdb.Name);
			sb.Append("\t");
			sb.Append(ucdb.Public.ToString());
			sb.Append("\t");
			sb.Append(ucdb.CompoundCount.ToString());
			sb.Append("\t");
			sb.Append(ucdb.ModelCount.ToString());
			sb.Append("\t");

			if (ucdb.PendingStatus == UcdbWaitState.DatabaseStorage ||
			 ucdb.PendingStatus == UcdbWaitState.ModelPredictions)
			{ // color to show if update is running
				CellStyleMx cs = new CellStyleMx();
				Font f = new Font("Arial", 9, FontStyle.Regular);
				cs = new CellStyleMx(f, Color.Black, Color.White);

				//if (udbs.UpdateIsRunning(ucdb))
				//	cs.ForeColor = Color.Green; // running
				//else if (udbs.UpdateIsPending(ucdb))
				//	cs.ForeColor = Color.Orange; // waiting to start
				//else cs.ForeColor = Color.Red; // started but stalled

				sb.Append("<CellStyle " + cs.Serialize() + ">");
				sb.Append(ucdb.PendingCompoundCount.ToString());
			}
			else sb.Append("Complete");
			sb.Append("\t");

			sb.Append(ucdb.CreationDate.ToShortDateString());
			sb.Append("\t");
			sb.Append(ucdb.UpdateDate.ToShortDateString());
			sb.Append("\t");
			sb.Append(ucdb.OwnerUserName);
			sb.Append("\t");
			sb.Append(ucdb.DatabaseId.ToString());
			sb.Append("\n");
			return sb.ToString();
		}

		/// <summary>
		/// Check to see if any imports need to started
		/// </summary>

		public static string CheckForCurrentUserImportFileUpdates()
		{
			UserData ud = new UserData();

			if (!ServicesIniFile.ReadBool("RunBackgroundProcessesInForeground", false)) // run as separate thread?
			{
				ParameterizedThreadStart ts = new ParameterizedThreadStart(ud.CheckForImportFileUpdatesThreadMethod);
				Thread newThread = new Thread(ts);
				newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start(false);
			}

			else ud.CheckForImportFileUpdatesThreadMethod(false);

			return "CheckForImportFileUpdates started";
		}

		/// <summary>
		/// Check to see if any Annotation table updates need to started
		/// </summary>

		public static string CheckForAllUsersImportFileUpdates()
		{
			if (!Security.IsMobius)
				return "Only the account <mobiusAccount> can check for all annotation table updates";

			UserData ud = new UserData();

			if (!ServicesIniFile.ReadBool("RunBackgroundProcessesInForeground", false))
			{
				ParameterizedThreadStart ts = new ParameterizedThreadStart(ud.CheckForImportFileUpdatesThreadMethod);
				Thread newThread = new Thread(ts);
				newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start(true);
			}

			else ud.CheckForImportFileUpdatesThreadMethod(true);

			return "CheckForAllUsersImportFileUpdates started";
		}


		/// <summary>
		/// Thread to check to see if any imports need to started
		/// </summary>

		public void CheckForImportFileUpdatesThreadMethod(Object CheckAll)
		{
			// Check each ImportState user object for the user to see if any imports need to be started.
			// If any are found then start a new hidden Mobius client & server to upload the file(s)
			// and start an import user data process for each one.

			UserDataImportState udis;

			List<UserObject> imps = new List<UserObject>();
			UserCmpndDbDao udbs = new UserCmpndDbDao();

			int t0 = TimeOfDay.Milliseconds();

			bool checkAllImportFiles = (bool)CheckAll;

			if (checkAllImportFiles)
			{
				imps = UserObjectDao.ReadMultiple(UserObjectType.ImportState, false);
			}
			else
			{
				imps = UserObjectDao.ReadMultiple(UserObjectType.ImportState, SS.I.UserName, false, false);
			}

			int t1 = TimeOfDay.Milliseconds() - t0;
			if (imps.Count == 0) return;
			//			return ""; // debug

			int i1 = 0;
			while (i1 < imps.Count)
			{ // pare list down do those needing updating
				UserObject uo = imps[i1];

				try { udis = UserDataImportState.Deserialize(uo); }
				catch (Exception ex)
				{
					imps.RemoveAt(i1);
					continue;
				}

				if (udis.CheckForFileUpdates && ((checkAllImportFiles == true && udis.ClientFile.Substring(0, 1) == "\\" && FileUtil.Exists(udis.ClientFile)) || checkAllImportFiles == false))
				{
					DateTime clientFileModDt = FileUtil.GetFileLastWriteTime(udis.ClientFile); // get client file mod date

					if (clientFileModDt == DateTime.MinValue || // skip if client file not found or
							udis.ImportIsRunning || // import is already running
							((clientFileModDt - udis.ClientFileModified).TotalSeconds < 1 && // no change in client file mod date and
							 !udis.ImportHasFailed)) // prev load attempt hasn't failed
					{
						imps.RemoveAt(i1);
						continue;
					}

					udis.ClientFileModified = clientFileModDt; // write the updated file date
					uo.Description = udis.Serialize();
					UserObjectDao.Write(uo);
				}

				else // running or failed manual background import
				{
					if (udis.ImportHasFailed) // delete if failed 
					{
						bool deleted = UserObjectDao.Delete(udis.Id);
						udbs.LogMessage("Deleted ImportState object for failed manual background import on " + uo.Name);
					}

					imps.RemoveAt(i1); // don't consider further here
					continue;
				}

				i1++;
			}

			//write a debug message and return        
			udbs.LogMessage(string.Format("Found {0} annotation files that could be updated by the {1} account", imps.Count, SS.I.UserName));

			int t2 = TimeOfDay.Milliseconds() - t0;
			if (imps.Count == 0) return;

			// Upload the file to the server and start a background process to update the annotation table

			foreach (UserObject uo2 in imps)
			{
				try
				{
					udis = UserDataImportState.Deserialize(uo2);
					string internalUoName = "Annotation_" + uo2.Id;
					string exportDir = ServicesIniFile.Read("BackgroundExportDirectory");
					string serverFileName = // location for file on server
							exportDir + @"\" + internalUoName + Path.GetExtension(udis.FileName);
					ServerFile.CopyToServer(udis.FileName, serverFileName);
					string command = "ImportUserData " + serverFileName + ", " + internalUoName;
					CommandLine.StartBackgroundSession("ImportUserData " + serverFileName + ", " + uo2.Name);
					udbs.LogMessage("Auto-upload for ImportState ObjId = " + ", " + uo2.Id + ", Name = " + uo2.Name + ", Desc = " + uo2.Description);
				}

				catch (Exception ex)
				{
					try
					{
						udbs.LogMessage("Auto-upload exception ImportState ObjId = " + uo2.Id + ", Name = " + uo2.Name +
								", Desc = " + uo2.Description +
								"\n" + DebugLog.FormatExceptionMessage(ex));
					}

					catch (Exception ex2) { ex2 = ex2; }

					continue;
				}
			}

			Progress.Hide();

			int t3 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Import from a file
		/// </summary>
		/// <returns></returns>

		public static string ImportFile(
				string importFileName,
				string userDataObjectName
				)
		{
			string args = importFileName + "," + userDataObjectName;
			return ImportFile(args);
		}

		/// <summary>
		/// Command line to import user data file
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string ImportFile(
				string args)
		{
			UserDataEditor editor = new UserDataEditor();
			return editor.ImportFile(args);
		}

		/// <summary>
		/// Return true if user data user object
		/// </summary>
		/// <param name="uo"></param>
		/// <returns></returns>

		public static bool IsUserDataObject(
				UserObject uo)
		{
			if (uo.Type == UserObjectType.Annotation ||
					uo.Type == UserObjectType.UserDatabase)
				return true;
			else return false;
		}


		/// <summary>
		/// Start background delete for user data
		/// </summary>
		/// <param name="uo"></param>

		public static void StartBackgroundDataDelete(
				UserObject uo)
		{
			if (uo.Type == UserObjectType.Annotation)
				CommandLine.StartBackgroundSession("DeleteAnnotationTableData " + uo.Id);

			else if (uo.Type == UserObjectType.UserDatabase)
			{
				string databaseIdstring = uo.Content;
				CommandLine.StartBackgroundSession("DeleteUserDatabaseData " + databaseIdstring);
			}

			else throw new Exception("Not a UserData type " + uo.Type);

			return;
		}

		/// <summary>
		/// Delete data associated with an annotation table
		/// </summary>
		/// <param name="idArg"></param>
		/// <returns></returns>

		public static string DeleteAnnotationTableData(
				string idArg)
		{
			int id = Int32.Parse(idArg);
			AnnotationDao adao = new AnnotationDao();
			int delCount = adao.DeleteTable(id);
			adao.Dispose();
			return delCount + " rows deleted";
		}

	}

	/// <summary>
	/// Hold associations between user objects & their user compound databases
	/// </summary>

	internal class UcdbUoAssoc
	{
		public UserObject Uo;
		public UcdbDatabase Ucdb;
	}

}
