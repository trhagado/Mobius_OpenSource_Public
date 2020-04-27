using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using ServiceTypes = Mobius.Services.Types;
using Mobius.Services.Native;
using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.ServiceFacade
{

/// <summary>
/// Handle file transfer between the client & server
/// </summary>

	public class ServerFile : IServerFile
	{

    /// <summary>
		/// Copy file from client to server
		/// </summary>
		/// <param name="clientFile2"></param>
		/// <param name="serverFile"></param>

				public static void CopyToServer(
					string clientFile,
					string serverFile)
				{
					string clientFile2 = SharePointUtil.CacheLocalCopyIfSharePointFile(clientFile);

					if (ServiceFacade.UseRemoteServices)
					{
						FileStream fs = new FileStream(clientFile2, FileMode.Open, FileAccess.Read);
						while (true)
						{
							byte[] buffer = new byte[UAL.ServerFile.TransferChunkSize];
							int bufLen = fs.Read(buffer, 0, buffer.Length);

							if (bufLen < UAL.ServerFile.TransferChunkSize) // at end
							{
								fs.Close();

								byte[] buffer2 = new byte[bufLen];
								Array.Copy(buffer, buffer2, bufLen);
								buffer = buffer2;
							}

							NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
									ServiceCodes.MobiusFileService,
									MobiusFileService.CopyToServer,
									new object[] { buffer, serverFile });

							if (bufLen < UAL.ServerFile.TransferChunkSize) break;
							serverFile = null; // null for subsequent calls
						}
					}

			else // direct call
			{
				FileStream fs = new FileStream(clientFile2, FileMode.Open, FileAccess.Read);
				while (true)
				{
					byte[] buffer = new byte[UAL.ServerFile.TransferChunkSize];
					int bufLen = fs.Read(buffer, 0, buffer.Length);

					if (bufLen < UAL.ServerFile.TransferChunkSize) // at end
					{
						fs.Close();

						byte[] buffer2 = new byte[bufLen];
						Array.Copy(buffer, buffer2, bufLen);
						buffer = buffer2;
					}

					UAL.ServerFile.CopyToServer(serverFile, buffer);
					if (bufLen < UAL.ServerFile.TransferChunkSize) break;
					serverFile = null; // null for subsequent calls
				}
			}

			if (clientFile2 != clientFile) File.Delete(clientFile2); // delete any temp file
			return;
		}

		/// <summary>
		/// Copy file from server to client
		/// </summary>
		/// <param name="serverFile"></param>
		/// <param name="clientFile2"></param>

		public static void CopyToClient(
			string serverFile,
			string clientFile)
		{
			FileStream fw = null;

			try
			{
				string tempFile = TempFile.GetTempFileName();

				if (ServiceFacade.UseRemoteServices)
				{
					fw = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
					while (true)
					{
						NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
								ServiceCodes.MobiusFileService,
								MobiusFileService.CopyToClient,
								new object[] { serverFile });

						byte[] buffer = (byte[])resultObject.Value;
						if (buffer == null) break;
						fw.Write(buffer, 0, buffer.Length);
						if (buffer.Length < UAL.ServerFile.TransferChunkSize) break;
						serverFile = null; // null to get subsequent chunks
						System.Windows.Forms.Application.DoEvents();
					}

					fw.Close();
				}

				else // direct call
				{
					fw = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
					while (true)
					{
						byte[] buffer = UAL.ServerFile.CopyToClient(serverFile);
						if (buffer == null) break;
						fw.Write(buffer, 0, buffer.Length);
						if (buffer.Length < UAL.ServerFile.TransferChunkSize) break;
						serverFile = null; // null to get subsequent chunks
						System.Windows.Forms.Application.DoEvents();
					}

					fw.Close();
				}

				bool normalFile = !SharePointUtil.IsSharePointName(clientFile);
				if (normalFile)
				{ // normal file, move temp file to dest file
					FileUtil.ReplaceFile(clientFile, tempFile);
				}

				else // sharepoint
				{
					SharePointUtil.CopyToSharePoint(tempFile, clientFile);
					File.Delete(tempFile); // delete any temp file
				}
				return;

			}

			catch (Exception ex) // close file on any exception
			{
				if (fw != null)
					try { fw.Close(); }	catch { }
				throw new Exception(ex.Message, ex);
			}
		}

/// <summary>
/// Get server file if changed from client file
/// </summary>
/// <param name="serverFile"></param>
/// <param name="clientFile"></param>

		public static bool GetIfChanged(
			string serverFile,
			string clientFile)
		{
			DateTime serverLwt = GetLastWriteTime(serverFile); // get last write time for server file
			if (serverLwt == DateTime.MinValue) return false; // doesn't exist on server

			if (File.Exists(clientFile) && FileUtil.GetFileLength(clientFile) > 0)
			{
				DateTime clientLwt = FileUtil.GetFileLastWriteTime(clientFile); // get last write time of local file
				if (DateTime.Compare(clientLwt, serverLwt) >= 0) return false; // no need to get if client file newer
			}

			try { CopyToClient(serverFile, clientFile); }
			catch (Exception ex) { throw new Exception(ex.Message, ex); } // debug
			return true;
		}

/// <summary>
/// Get the last write time for a server file
/// </summary>
/// <param name="serverFile"></param>
/// <returns></returns>

		public static DateTime GetLastWriteTime(string serverFile)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusFileService,
						MobiusFileService.GetLastWriteTime,
						new object[] { serverFile });
				DateTime result = (DateTime)resultObject.Value;
				return result;
			}

			else return UAL.ServerFile.GetLastWriteTime(serverFile);
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
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
						ServiceCodes.MobiusFileService,
						MobiusFileService.GetTempFileName,
						new object[] { extension, deleteOnAppExit });
				if (resultObject == null) return null;
				string tempFileName = (string)resultObject.Value;
				return tempFileName;
			}
			else return UAL.ServerFile.GetTempFileName(extension, deleteOnAppExit);
		}

/// <summary>
/// Return true if Mobius can write to the specified file from the service side
/// </summary>
/// <param name="path"></param>
/// <returns></returns>

		public static bool CanWriteFileFromServiceAccount(string path)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusFileService,
					MobiusFileService.CanWriteFileFromServiceAccount,
					new object[] { path });
				if (resultObject == null) return false;
				bool result = (bool)resultObject.Value;
				return result;
			}
			else return UAL.ServerFile.CanWriteFileFromServiceAccount(path);
		}

/// <summary>
/// Retrieve & read server file into a string (member of IServerFile interface)
/// </summary>
/// <param name="fileName"></param>
/// <returns></returns>

		public string ReadAll(string serverFile)
		{
			string clientFile = TempFile.GetTempFileName(".txt");
			CopyToClient(serverFile, clientFile); 

			if (!File.Exists(clientFile) || FileUtil.GetFileLength(clientFile) <= 0) 
				return null;
			StreamReader sr = new StreamReader(clientFile);
			string txt = sr.ReadToEnd();
			sr.Close();
			try { File.Delete(clientFile); } catch { };
			return txt;
		}

	}
}
