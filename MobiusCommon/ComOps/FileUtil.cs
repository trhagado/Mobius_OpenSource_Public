using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mobius.ComOps
{

/// <summary>
/// File utilitities
/// </summary>

	public partial class FileUtil
	{

/// <summary>
/// Return true if file exists
/// </summary>
/// <param name="path"></param>
/// <returns></returns>

		public static bool Exists(
			string path)
		{
			if (!SharePointUtil.IsSharePointName(path))
			{
				if (File.Exists(path)) return true;
				else return false;
			}

			else
			{
				if (Lex.IsNullOrEmpty(SharePointUtil.CanReadFile(path)))
					return true;
				else return false;
			}
		}

		/// <summary>
		/// See if file can be opened for reading, return error message if not
		/// </summary>
		/// <param name="path"></param>

		public static string CanReadFile(
			string path)
		{
			try
			{
				bool isNormalFilePath = (!SharePointUtil.IsSharePointName(path));

				if (isNormalFilePath) 
				{
					FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
					fs.Close();
				}

				else return SharePointUtil.CanReadFile(path);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

			return "";
		}

		/// <summary>
		/// See if file can be opened for writing, return error if not
		/// </summary>
		/// <param name="path"></param>

		public static string CanWriteFile (
			string path)
		{
			try
			{
				if (!SharePointUtil.IsSharePointName(path))
				{
					bool exists = File.Exists(path);
					FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
					fs.Close();
					if (!exists) // if didn't exist before then delete it
						try { File.Delete(path); }
						catch (Exception ex) { ex = ex; }
				}

				else	return SharePointUtil.CanWriteFile(path);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

			return "";
		}

/// <summary>
/// Read complete text file
/// </summary>
/// <param name="path"></param>
/// <returns></returns>

		public static string ReadFile(
			string path)
		{
			StreamReader sr = null;

			try
			{
				sr = new StreamReader(path);
				string content = sr.ReadToEnd();
				sr.Close();
				return content;
			}

			catch (Exception ex)
			{
				CloseStreamReader(sr);
				return ""; 
			}
		}

/// <summary>
/// Write complete text file
/// </summary>
/// <param name="path"></param>
/// <param name="content"></param>

		public static void WriteFile(
		 string path,
		 string content)
		{
			StreamWriter sw = null;

			try
			{
				sw = new StreamWriter(path);
				sw.Write(content);
				sw.Close();
				return;
			}

			catch (Exception ex)
			{
				CloseStreamWriter(sw);
				return;
			}
		}


		/// <summary>
		/// CloseStreamReader avoiding any exceptions
		/// </summary>
		/// <param name="sr"></param>

		public static void CloseStreamReader(StreamReader sr)
		{
			try
			{
				if (sr == null) return;
				sr.Close();
			}

			catch (Exception ex) { ex = ex; }
		}

		/// <summary>
		/// CloseStreamWriter avoiding any exceptions
		/// </summary>
		/// <param name="sw"></param>

		public static void CloseStreamWriter(StreamWriter sw)
		{
			try
			{
				if (sw == null) return;
				sw.Close();
			}

			catch (Exception ex) { ex = ex; }
		}

		/// <summary>
		/// Write a file from a Base64 encoded string
		/// </summary>
		/// <param name="b64String"></param>
		/// <param name="outputFile"></param>

		public static void WriteFileFromBase64String(
			string b64String,
			string outputFile)
		{
			byte[] ba = Convert.FromBase64String(b64String);

			FileStream fs = new FileStream(outputFile, FileMode.Create);
			fs.Write(ba, 0, ba.Length);
			fs.Close();
			return;
		}

		/// <summary>
		/// Get length of file or -1 if doesn't exist
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>

		public static long GetFileLength ( 
			string path)
		{
			if (!SharePointUtil.IsSharePointName(path))
			{
				if (!File.Exists(path)) return -1;

				FileInfo fi = new FileInfo(path);
				return fi.Length;
			}

			else
				throw new NotSupportedException(path);
		}

/// <summary>
/// Get last write time for file
/// </summary>
/// <param name="path"></param>
/// <returns></returns>

		public static DateTime GetFileLastWriteTime(
			string path)
		{
			if (!SharePointUtil.IsSharePointName(path))
			{
				if (!File.Exists(path)) return DateTime.MinValue;

				FileInfo fi = new FileInfo(path);
				return fi.LastWriteTime;
			}

			else 
				return SharePointUtil.GetFileUpdateDate(path);
		}

		/// <summary>
		/// Delete contents of a directory with recursive descent
		/// Set any read-only entries to normal to allow deleting as necessary
		/// </summary>
		/// <param name="destDir"></param>
		/// <returns></returns>

		public static bool DeleteDirectory(
			string destDir)
		{
			if (!Directory.Exists(destDir)) return true;

			try
			{
				File.SetAttributes(destDir, FileAttributes.Normal); // set attributes to normal in case is read only
				Directory.Delete(destDir, true);
				return true;
			}

			catch (Exception ex) { ex = ex; } // something failed, try deleting files individually

			string[] entries = Directory.GetFileSystemEntries(destDir);

			foreach (string path in entries)
			{
				// Sub directory

				if (Directory.Exists(path))
				{
					DeleteDirectory(path); // delete recursively
				}

				// Individual file

				else
				{
					DeleteFile(path);
				}
			}

			try // try again
			{
				Directory.Delete(destDir, true);
				return true;
			}

			catch (Exception ex) { return false; }
		}

		/// <summary>
		/// Delete a file not throwing an exception if fails
		/// If read-only set to normal and then try delete again
		/// </summary>
		/// <param name="path"></param>
		/// <returns>True if file has been deleted or didn't exist</returns>

		public static bool DeleteFile(
			string path)
		{
			if (!File.Exists(path)) return true; // return true if file doesn't exist

			try
			{
				File.Delete(path);
				return true;
			}
			catch (Exception ex)
			{
				File.SetAttributes(path, FileAttributes.Normal); // set attributes to normal in case file is read only

				try
				{
					File.Delete(path);
					return true;
				}
				catch (Exception ex2)
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Append line to a file
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="path"></param>

		public static void AppendLineToFile (
			string line,
			string path)
			{
			try 
			{
				StreamWriter sw = new StreamWriter(path,true);
				sw.WriteLine(line);
				sw.Close();
			}
			catch (Exception ex) {} // ignore errors
			return;
		}

		/// <summary>
		/// See if file can be renamed
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static bool CanRename(
			string fileName)
		{
			string fileName2 = fileName + "." + TimeOfDay.Milliseconds();
			try
			{
				File.Move(fileName, fileName2);
				File.Move(fileName2, fileName);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}


		/// <summary>
		/// Backup and replace a file
		/// </summary>
		/// <param name="destFile">Where the new file should go</param>
		/// <param name="backupFile">Backup file to move existing dest file to</param>
		/// <param name="newFile">New file to move to dest file</param>

		public static bool BackupAndReplaceFile(
			string destFile,
			string backupFile,
			string newFile)
		{
			return BackupAndReplaceFile(destFile, backupFile, newFile, false);
		}

		/// <summary>
		/// Backup and replace a file
		/// </summary>
		/// <param name="destFile">Where the new file should go</param>
		/// <param name="backupFile">Backup file to move existing dest file to</param>
		/// <param name="newFile">New file to move to dest file</param>

		public static bool BackupAndReplaceFile(
			string destFile,
			string backupFile,
			string newFile,
			bool logErrors)
		{
			if (File.Exists(backupFile)) // remove any old backup file
			{
				try { File.Delete(backupFile); } 
				catch (Exception ex) 
					{ if (logErrors) DebugLog.Message("Can't delete file: " + backupFile); }
			}

			if (File.Exists(destFile))
			{
				try { File.Move(destFile, backupFile); } // move any existing dest to bak
				catch (Exception ex)
				{ // try deleting existing dest file if it can't be moved
					if (logErrors) DebugLog.Message("Can't move file: " + destFile + " to file: " + backupFile);

					try { File.Delete(destFile); }
					catch (Exception ex2)
					{ if (logErrors) DebugLog.Message("Can't delete file: " + destFile); }
				}
			}

			try { File.Move(newFile, destFile); } // move new to dest
			catch (Exception ex) 
			{
				{ if (logErrors) DebugLog.Message(DebugLog.FormatExceptionMessage(ex, "Can't move file: " + newFile + " to file: " + destFile, true)); }
				return false;
			}

			return true;
		}

		/// <summary>
		/// Backup and replace a file
		/// </summary>
		/// <param name="destFile">Where the new file should go</param>
		/// <param name="newFile">New file to move to dest file</param>

		public static void ReplaceFile(
			string destFile,
			string newFile)
		{
			if (File.Exists(destFile)) // delete if exists
				try
				{
					System.IO.File.SetAttributes(destFile, FileAttributes.Normal); // set attributes to normal in case file is read only
					File.Delete(destFile);  // remove any existing file
				}
				catch (Exception ex)
				{
					try
					{ // if can't delete try renaming
						string tempFile = TempFile.GetTempFileName();
						File.Move(destFile, tempFile);  // move any existing dest temp file
					}
					catch (Exception ex2) { } // no luck, continue on anyway
				}

			try { File.Move(newFile, destFile); } // move new to dest
			catch (Exception ex) { }

			return;
		}

/// <summary>
/// Copy file including SharePoint support
/// </summary>
/// <param name="?"></param>

		public static void CopyFile(
			string sourceFileName, 
			string destFileName)
		{
			if (!SharePointUtil.IsSharePointName(sourceFileName) &&
				!SharePointUtil.IsSharePointName(destFileName))
			{ // both regular files
				File.Copy(sourceFileName, destFileName, true);
			}

			else if (SharePointUtil.IsSharePointName(sourceFileName) &&
				SharePointUtil.IsSharePointName(destFileName))
			{ // both SharePoint
				string tempFile = TempFile.GetTempFileName();
				SharePointUtil.CopyFromSharePoint(sourceFileName, tempFile);
				SharePointUtil.CopyToSharePoint(tempFile, destFileName);
				File.Delete(tempFile);
			}

			else if (SharePointUtil.IsSharePointName(sourceFileName)) // source only is SharePoint
				SharePointUtil.CopyFromSharePoint(sourceFileName, destFileName);

			else // dest only is SharePoint
				SharePointUtil.CopyToSharePoint(sourceFileName, destFileName);

			return;
		}

/// <summary>
/// Copy directory and optionally subdirectories with overwrite
/// </summary>
/// <param name="sourceDir"></param>
/// <param name="destDir"></param>
/// <param name="copySubDirs"></param>
/// <param name="copyIniFiles"></param>
/// <param name="cb">Callback for status and log messages</param>
/// <returns></returns>

		public static string CopyDirectory(
			string sourceDir,
			string destDir,
			bool copySubDirs,
			bool copyIniFiles,
			ICopyCallback cb)
		{
			String[] Files;
			string msgList = "", msg;

			if (cb != null) cb.UpdateStatus("Copying Files...");
			if (cb != null) cb.LogMessage("Copying Directory " + sourceDir + " to " + destDir);

			if (destDir[destDir.Length - 1] != Path.DirectorySeparatorChar)
				destDir += Path.DirectorySeparatorChar; // add separator to dest

			if (!Directory.Exists(destDir))
				try
				{
					if (cb != null) cb.LogMessage("Creating Directory " + destDir);
					Directory.CreateDirectory(destDir);
				}
				catch (Exception ex)
				{
					msg = "Directory.CreateDirectory Exception, " + destDir + ", " + ex.Message;
					if (cb != null) cb.LogMessage(msg);
					msgList += msg + "\r\n";
				}

			Files = Directory.GetFileSystemEntries(sourceDir);
			foreach (string srcFile in Files)
			{
				string dstFile = "";

				// Sub directories
				if (Directory.Exists(srcFile))
				{
					if (!copySubDirs) continue;

					string dirName = Path.GetFileName(srcFile); 
					if (Lex.Eq(dirName, "temp") || Lex.Eq(dirName, "cache")) // don't copy these
						continue;

					dstFile = destDir + Path.GetFileName(srcFile);
					msgList += CopyDirectory(srcFile, destDir + Path.GetFileName(srcFile), true, copyIniFiles, cb);
				}

						// Files in directory

				else
				{
					string fileName = Path.GetFileName(srcFile);
					if (!copyIniFiles)
					{
						if (String.Compare(Path.GetExtension(fileName), ".ini", true) == 0)
							continue; // don't copy .ini files
						if (String.Compare(Path.GetExtension(fileName), ".config", true) == 0)
							continue; // don't copy .config files
						if (String.Compare(Path.GetExtension(fileName), ".log", true) == 0)
							continue; // don't copy .log files
					}

					dstFile = destDir + fileName;

					FileInfo rfi = new FileInfo(srcFile); // remote file
					FileInfo lfi = new FileInfo(dstFile); // local file

					if (lfi.Exists)
					{
						if (lfi.LastWriteTime.CompareTo(rfi.LastWriteTime) >= 0 && lfi.Length == rfi.Length) continue; // continue if same date or newer and same size
						try
						{ // delete/rename the existing file
							System.IO.File.SetAttributes(dstFile, FileAttributes.Normal); // set attributes to normal in case file is read only
							System.IO.File.Delete(dstFile);
						}
						catch (Exception ex) // try rename if delete fails
						{
							try
							{
								cb.LogMessage("Delete failed, attempting rename of: " + dstFile);
								string dstFileOld = dstFile + ".old";
								if (File.Exists(dstFileOld))
								{
									System.IO.File.SetAttributes(dstFileOld, FileAttributes.Normal); // set attributes to normal in case file is read only
									File.Delete(dstFileOld);
								}
								File.Move(dstFile, dstFileOld);
							}
							catch (Exception ex2)
							{
								msg = "File.Delete/Rename Exception, source = " + srcFile + ", dest = " + dstFile + "\r\n" + ex.Message;
								if (cb != null) cb.LogMessage(msg);
							}
						}
					}

					if (cb != null) cb.UpdateStatus("Copying File: " + fileName);

					try
					{
						if (cb != null) cb.LogMessage("Copying file " + srcFile + " to " + dstFile);
						System.IO.File.Copy(srcFile, dstFile, true);
					}
					catch (Exception ex)
					{
						msg = "File.Copy Exception, source = " + srcFile + ", dest = " + dstFile + "\r\n" + ex.Message;
						if (cb != null) cb.LogMessage(msg);
						msgList += msg + "\r\n";
					}
				}
			}

			return msgList;
		}

		/// <summary>
		/// Write an open a text/html document
		/// </summary>
		/// <param name="docName"></param>
		/// <param name="docText"></param>

		public static void WriteAndOpenTextDocument(
			string docName,
			string docText)
		{
			string fileName = docName;

			if (Lex.IsUndefined(Path.GetDirectoryName(fileName))) // put in temp if no directory defined
				fileName = ClientDirs.TempDir + @"\" + fileName;

			if (Lex.IsUndefined(Path.GetExtension(fileName))) // add .txt extension if needed
				fileName += ".txt";

			StreamWriter sw = new StreamWriter(fileName);
			sw.Write(docText);
			sw.Close();

			SystemUtil.StartProcess(fileName); // show the file (normally via notepad)
			return;
		}

	}

	/// <summary>
	/// Callback interface for file copy operation
	/// </summary>

	public interface ICopyCallback
	{

/// <summary>
/// Update status display
/// </summary>
/// <param name="msg"></param>

		void UpdateStatus(string msg);

/// <summary>
/// Log a message
/// </summary>
/// <param name="msg"></param>
/// 
		void LogMessage(string msg);
	}


		/// <summary>
		/// Binary file utilities
		/// </summary>

		public class BinaryFile
		{
			public static DateTime BaseDate = new DateTime(1900, 1, 1);
			public static List<string> FilesWritten; // list of open binaryfile names
			public static List<BinaryWriter> BinaryWriters; // list of binary writers

			/// <summary>
			/// Open a binary writer
			/// </summary>
			/// <param name="fileName"></param>
			/// <returns></returns>

			public static BinaryWriter OpenWriter(string fileName)
			{
				if (FilesWritten == null) FilesWritten = new List<string>();
				FilesWritten.Add(fileName);

				FileStream fs = File.Open(fileName, FileMode.Create);
				BinaryWriter bw = new BinaryWriter(fs);

				if (BinaryWriters == null) BinaryWriters = new List<BinaryWriter>();
				BinaryWriters.Add(bw);
				return bw;
			}

			/// <summary>
			/// Write a binary date as a ushort number of days since BaseDate
			/// </summary>
			/// <param name="bw"></param>
			/// <param name="dt"></param>

			public static void WriteDayCount(BinaryWriter bw, DateTime dt)
			{
				TimeSpan ts = dt.Subtract(BaseDate);
				if (ts.TotalDays > 0 && ts.TotalDays < 65534)
					bw.Write((ushort)ts.TotalDays);
				else bw.Write((ushort)0); // "null" value
			}

/// <summary>
/// Open a binary reader
/// </summary>
/// <param name="fileName"></param>
/// <returns></returns>

			public static BinaryReader OpenReader(string fileName)
			{
				FileStream fs = File.Open(fileName, FileMode.Open);

				BinaryReader br = new BinaryReader(fs);
				return br;
			}

/// <summary>
/// Return true if eof on reader
/// </summary>
/// <param name="br"></param>
/// <returns></returns>

			public static bool ReaderEof(BinaryReader br)
			{
				long streamLength = br.BaseStream.Length;
				if (br.BaseStream.Position >= streamLength - 1) return true;
				else return false;
			}

			/// <summary>
			/// Read a binary date as written by BinaryWriteDayCount
			/// </summary>
			/// <param name="br"></param>
			/// <returns></returns>

			public static DateTime ReadDayCount(BinaryReader br)
			{
				ushort days = br.ReadUInt16();
				if (days == 0) return DateTime.MinValue; // null
				else return BaseDate.AddDays(days);
			}

			/// <summary>
			/// Close all open binary writers 
			/// </summary>

			public static void CloseWriters()
			{
				if (BinaryWriters == null) return;

				foreach (BinaryWriter bw in BinaryWriters)
					try { bw.Close(); }
					catch (Exception ex) { ex = ex; }
				BinaryWriters = null;
				return;
			}

			/// <summary>
			/// Activate files that have been written
			/// </summary>

			public static void ActivateNewFiles()
			{
				if (FilesWritten == null) return;

				foreach (string fileName in FilesWritten)
					ActivateNewFile(fileName);

				FilesWritten = null;
				return;
			}

			/// <summary>
			/// Activate a new file
			/// </summary>
			/// <param name="fileName"></param>

			public static void ActivateNewFile(string fileName)
		{
			string fBase = Replace(fileName, ".new", "");
			FileUtil.BackupAndReplaceFile(fBase + ".bin", fBase + ".bak", fBase + ".new");
		}

			/// <summary>
			/// Case-insensitive Replace
			/// </summary>
			/// <param name="str"></param>
			/// <param name="oldValue"></param>
			/// <param name="newValue"></param>
			/// <returns></returns>

			static public string Replace(
				string str,
				string oldValue,
				string newValue)
			{
				if (str == null) return null;
				StringBuilder sb = new StringBuilder();

				int previousIndex = 0;
				int index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
				while (index != -1)
				{
					sb.Append(str.Substring(previousIndex, index - previousIndex));
					sb.Append(newValue);
					index += oldValue.Length;

					previousIndex = index;
					index = str.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
				}
				sb.Append(str.Substring(previousIndex));

				return sb.ToString();
			} 

	} // BinaryFile
}
