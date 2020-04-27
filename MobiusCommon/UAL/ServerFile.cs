using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mobius.UAL
{
	public class ServerFile : IServerFile
	{
		static FileStream CtcReadStream; // CopyToClient state info
		static string CtcFileName = "";
		static int CtcChunks = 0;

		static FileStream CtsWriteStream;
		static string CtsFileName = "";
		static int CtsChunks = 0;

		public const int TransferChunkSize = 1000000; // max bytes to send (must be the same value on the server and the client)

		/// <summary>
		/// Copy file from client to server
		/// </summary>
		/// <param name="serverFile">Name of server file or null to continue writing to currently open file</param>
		/// <param name="buffer"></param>

		public static void CopyToServer( 
			string serverFile,
			byte[] buffer)
		{
			try
			{
				if (serverFile != null) // first call
				{
					CtsFileName = NormalizeName(serverFile);
					CtsWriteStream = new FileStream(CtsFileName, FileMode.Create, FileAccess.Write);
					CtsChunks = 0;
				}

				else if (CtsWriteStream == null) throw new Exception("CopyToServer write FileStream not defined for secondary call");

				CtsWriteStream.Write(buffer, 0, buffer.Length);
				CtsChunks++;

				if (buffer.Length < TransferChunkSize) // at end
				{
					CtsWriteStream.Close();
					CtsWriteStream = null;
				}

				return;
			}

			catch (Exception ex)
			{
				if (CtsWriteStream != null)
					try 	{	CtsWriteStream.Close(); }
					finally { CtsWriteStream = null; }

				DebugLog.Message("CopyToServer failed for file: " + CtsFileName + ", chunks: " + CtsChunks + "\r\n" + // log the error
					DebugLog.FormatExceptionMessage(ex));

				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Copy a chunk of a file from server to the client
		/// </summary>
		/// <param name="serverFile">Name of server file or null to continue reading from currently open file</param>

		public static byte[] CopyToClient(
			string serverFile)
		{
			try
			{
				if (serverFile != null) // first call
				{
					CtcFileName = NormalizeName(serverFile);
					CtcChunks = 0;
				}

				else if (Lex.IsUndefined(CtcFileName)) throw new Exception("CopyToClient server file name not defined for secondary call");

				CtcReadStream = // open as FileShare.ReadWrite to minimize blocking
					new FileStream(CtcFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

				byte[] buffer = new byte[TransferChunkSize];
				CtcReadStream.Position = (long)CtcChunks * TransferChunkSize; // position to next chunk
				int bufLen = CtcReadStream.Read(buffer, 0, TransferChunkSize);
				try { CtcReadStream.Close(); } // close file between calls to avoid locking the file for extended periods
				catch (Exception ex) { }
				CtcReadStream = null;

				if (bufLen < TransferChunkSize) // at end, return partial buffer
				{
					byte[] buffer2 = new byte[bufLen];
					Array.Copy(buffer, buffer2, bufLen);
					buffer = buffer2;

					CtcFileName = ""; // all done, clear file name
				}

				else CtcChunks++; // count normal full-chunk read

				return buffer;
			}

			catch (Exception ex)
			{
				if (CtcReadStream != null)
					try { CtcReadStream.Close();	}
					finally { CtcReadStream = null; }

				CtcFileName = "";

				DebugLog.Message("CopyToClient failed for file: " + CtcFileName + ", chunks: " + CtcChunks + "\r\n" + // log the error
					DebugLog.FormatExceptionMessage(ex));

				throw new Exception(ex.Message, ex);
			}
		}

/// <summary>
/// Get the last write time for a server file
/// </summary>
/// <param name="serverFile"></param>
/// <returns></returns>

		public static DateTime GetLastWriteTime(string serverFile)
		{
				serverFile = ServerFile.NormalizeName(serverFile);
				DateTime serverLwt = GetLocalLastWriteTime(serverFile);
				return serverLwt;
		}

/// <summary>
/// Get last write time for a local file
/// </summary>
/// <param name="fileName"></param>
/// <returns></returns>

		public static DateTime GetLocalLastWriteTime(string fileName)
		{
			DateTime clientLwt = DateTime.MinValue;
			if (File.Exists(fileName))
			{
				FileInfo fi = new FileInfo(fileName);
				clientLwt = fi.LastWriteTime;
			}
			return clientLwt;
		}

/// <summary>
/// Normalize a server files names by replacing symbolic path with actual path
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static string NormalizeName(string name)
		{
			if (Lex.Contains(name, "<MetaDataDir>"))
				name = Lex.Replace(name, "<MetaDataDir>", ServicesDirs.MetaDataDir);

			else if (Lex.Contains(name, "<TargetMaps>"))
				name = Lex.Replace(name, "<TargetMaps>", ServicesDirs.TargetMapsDir);

			else if (Lex.Contains(name, "<TempDir>"))
				name = Lex.Replace(name, "<TempDir>", ServicesDirs.TempDir);

			else if (Lex.Contains(name, "<QueryResultsDataSetDir>"))
				name = Lex.Replace(name, "<QueryResultsDataSetDir>", ServicesDirs.QueryResultsDataSetDir);

			else if (Lex.Contains(name, "<BackgroundExportDir>"))
				name = Lex.Replace(name, "<BackgroundExportDir>", ServicesDirs.BackgroundExportDir);

			else if (Lex.Contains(name, "<ModelQueriesDir>"))
				name = Lex.Replace(name, "<ModelQueriesDir>", ServicesDirs.ModelQueriesDir);

			else if (Lex.Contains(name, "<MiscConfigDir>"))
				name = Lex.Replace(name, "<MiscConfigDir>", ServicesDirs.MiscConfigDir);

			return name;
		}

		/// <summary>
		/// Get a temp file on the server side
		/// </summary>
		/// <param name="extension"></param>
		/// <param name="deleteOnAppExit"></param>
		/// <returns></returns>

		public static string GetTempFileName(
			string extension,
			bool deleteOnAppExit)
		{
			return TempFile.GetTempFileName(ServicesDirs.TempDir, extension, deleteOnAppExit);
		}

/// <summary>
/// Return true if Mobius can write to the specified file (usually in another user's Share area or SharePoint site) from the service side
/// </summary>
/// <param name="path"></param>
/// <returns></returns>

		public static bool CanWriteFileFromServiceAccount(string path)
		{
			try
			{
				if (path.StartsWith(@"\\"))
				{
					string shareName = NetworkResourceUtil.NormalizeConnectionStringToUncShareName(path);
					//bool shareConnected = NetworkResourceUtil.DirectoryExists(shareName); // just verifying directory is faster if share already connected
					//if (!shareConnected)
					//{

					string userName, pw;

					if (!AccountAccessMx.TryGetMobiusSystemWindowsAccount(out userName, out pw))
						return false;

					NetworkResourceUtil.AddConnection(path, userName, pw, false);
					//}

					FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
					fs.Close();
					return true;
				}

				else if (SharePointUtil.IsSharePointName(path))
				{
					string msg = SharePointUtil.CanWriteFile(path); // check for write, note: uses current user id not SystemAccountName
					return Lex.IsNullOrEmpty(msg); 
				}

				else return false;
			}
			catch (Exception ex)
			{
				DebugLog.Message("CanWriteFileFromServiceAccount failed for path: " + path + "\r\n" +
					ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Retrieve & read server file into a string (member of IServerFile interface)
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public string ReadAll(string serverFile)
		{
			serverFile = NormalizeName(serverFile);
			StreamReader sr = new StreamReader(serverFile);
			string txt = sr.ReadToEnd();
			sr.Close();
			return txt;
		}

	}
}
