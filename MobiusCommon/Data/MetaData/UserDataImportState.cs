using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.Data
{
		/// <summary>
		/// Class to keep track of user data import progress.
		/// State user object is named Database_nnn or Annotation_nnn
		/// depending on the target of import.
		/// When a background import is requested an import state user object
		/// is created. A background process is started and updates LastStarted and
		/// LastCheckPoint as it proceeds. When the import is complete the object is deleted
		/// for a one time import and LastCheckPoint is cleared if CheckForFileUpdates is set.
		/// At the start of each Mobius session CheckForImportFileUpdates runs and checks each
		/// ImportState object to see if the file has changed. Each file that is changed
		/// is uploaded and a background import process is started.
		/// 
		/// </summary>

		public class UserDataImportState
		{
			public int Id; // Id of user object storing state

			public bool UserDatabase; // true if user database otherwise annotation table
			public int UserDataObjectId; // id of associated user database or annotation user object
			public string ClientFile = ""; // name of source file on client that updates are performed from
			public bool CheckForFileUpdates = false; // check for updated client file & automatically update
			public DateTime ClientFileModified; // last observed modification date for client file
			public string FileName = ""; // server file that client file has been uploaded to
			public DateTime LastStarted; // when last import was started
			public DateTime LastCheckpoint; // last checkpoint for import
			public int StartedCount; // number of times started
			public int CompletedCount; // number of times completed

			public string Serialize()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(UserDatabase.ToString()); // [0]
				sb.Append("\t");
				sb.Append(UserDataObjectId.ToString()); // [1]
				sb.Append("\t");
				sb.Append(ClientFile); // [2]
				sb.Append("\t");
				sb.Append(CheckForFileUpdates.ToString()); // [3]
				sb.Append("\t");
				sb.Append(DateTimeUS.ToString(ClientFileModified)); // [4]
				sb.Append("\t");
				sb.Append(FileName); // [5]
				sb.Append("\t");
				sb.Append(DateTimeUS.ToString(LastStarted)); // [6]
				sb.Append("\t");
				sb.Append(DateTimeUS.ToString(LastCheckpoint)); // [7]
				sb.Append("\t");
				sb.Append(StartedCount.ToString()); // [8]
				sb.Append("\t");
				sb.Append(CompletedCount.ToString()); // [9]

				return sb.ToString();
			}

			public static UserDataImportState Deserialize(
				UserObject uo)
			{
				try
				{
					UserDataImportState udis = new UserDataImportState();
					udis.Id = uo.Id;
					string[] sa = uo.Description.Split('\t');
					udis.UserDatabase = Boolean.Parse(sa[0]);
					udis.UserDataObjectId = int.Parse(sa[1]);
					udis.ClientFile = sa[2];
					udis.CheckForFileUpdates = bool.Parse(sa[3]);
					udis.ClientFileModified = DateTimeUS.Parse(sa[4]);
					udis.FileName = sa[5];
					udis.LastStarted = DateTimeUS.Parse(sa[6]);
					udis.LastCheckpoint = DateTimeUS.Parse(sa[7]);
					udis.StartedCount = int.Parse(sa[8]);
					udis.CompletedCount = int.Parse(sa[9]);

					return udis;
				}
				catch (Exception ex)
				{
					throw new Exception("Error deserializing UserDataImportState: " + uo.Description, ex);
				}
			}

			/// <summary>
			/// Return true if it appears import is running (i.e. LastCheckpoint has been recently updated)
			/// </summary>

			public bool ImportIsRunning
			{
				get
				{
					if (LastCheckpoint == DateTime.MinValue) return false; // if minvalue then complete or not started

					TimeSpan ts = DateTime.Now.Subtract(LastCheckpoint); // get time since last import checkpoint

					if (ts.TotalMinutes < 15) // if less than 15 minutes since last checkpoint (or LastCheckpoint = MinValue) then assume running
						return true;

					else return false;
				}
			}

			/// <summary>
			/// Return true if it appears import has failed
			/// </summary>

			public bool ImportHasFailed
			{
				get
				{
					if (LastCheckpoint == DateTime.MinValue) return false; // if minvalue then complete or not started

					TimeSpan ts = DateTime.Now.Subtract(LastCheckpoint); // get time since last import checkpoint

					if (ts.TotalMinutes > 15) // if more than 15 minutes since last checkpoint then not running
						return true; // prev load attempt hasn't failed

					else return false;
				}
			}
		}
	}
