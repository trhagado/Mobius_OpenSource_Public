using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace Mobius.ComOps
{
	public class MobiusClientUtil
	{
		public static string DefaultMobiusClientFolder = // default folder for Mobius client software
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Mobius";

		public static string StartupExecutableName = "MobiusClientStart.exe";
		public static string ExecutableName = "MobiusClient.exe"; // in BinX folder under the parent folder


		/// <summary>
		/// Check if the current version of the client is up to date.
		/// 
		/// Mobius is normally installed as follows:
		///  C:\Users\[userName]\AppData\Local\Mobius
		/// 	 MobiusClientStart.exe - Startup program that checks the current version, does updates as needed and launches MobiusClient.exe
		/// 	 MobiusClient.exe - Main Mobius client executeable. In a BinX directory under the Mobius directory (see ClientDeploymentDir below). Also checks version if not started by MobiusClientStart.
		/// 	 MobiusClient.ini - Common client .ini file settings
		/// 	 MobiusClientUser.ini - User-specific client .ini file settings
		/// 	 
		///  
		/// The ClientDeploymentDir setting in MobiusClient.ini will point to the directory
		/// that contains the current client MobiusClient.exe and associated files.
		/// MobiusClientStart.exe, MobiusClient.ini and MobiusClientUser.ini 
		/// will remain in C:\Users\[userName]\AppData\Local\Mobius
		/// </summary>
		/// <returns></returns>

		public static bool ClientVersionIsUpToDate()
		{
			string localIniPath, clientDeploymentDir, remoteIniPath;
			IniFile localIniFile, remoteIniFile;

			bool upToDate = ClientVersionIsUpToDate(
				out localIniPath,
				out localIniFile,
				out clientDeploymentDir,
				out remoteIniPath,
				out remoteIniFile,
				true);

			return upToDate;
		}

		/// <summary>
		/// Check if the current version of the client is up to date and where it is
		/// </summary>
		/// <param name="localIniPath"></param>
		/// <param name="localIniFile"></param>
		/// <param name="clientDeploymentDir"></param>
		/// <param name="remoteIniPath"></param>
		/// <param name="remoteIniFile"></param>
		/// <param name="logActivity"></param>
		/// <returns></returns>

		public static bool ClientVersionIsUpToDate(
			out string localIniPath,
			out IniFile localIniFile,
			out string clientDeploymentDir,
			out string remoteIniPath,
			out IniFile remoteIniFile,
			bool logActivity)
		{
			StreamReader sr = null;
			string localBinDir = null;

			localIniPath = "";
			localIniFile = null;
			clientDeploymentDir = "";
			remoteIniPath = "";
			remoteIniFile = null;

			try
			{
				if (logActivity) DebugLog.Message("Getting local & remote deployment Ids...");

				localIniPath = DefaultMobiusClientFolder + @"\MobiusClient.ini"; // ini file on client
				if (!File.Exists(localIniPath))
				{
					localIniPath = Application.StartupPath + @"\MobiusClient.ini"; // ini file on client
					if (!File.Exists(localIniPath)) return true;
				}

				localIniFile = new IniFile(localIniPath, "Mobius");

				string localDeploymentId = localIniFile.Read("DeploymentId", "UnknownLocalDeployment");
				if (logActivity) DebugLog.Message("LocalDeploymentId: " + localDeploymentId);

				clientDeploymentDir = localIniFile.Read("ClientDeploymentDir", DefaultMobiusClientFolder);
				if (logActivity) DebugLog.Message("ClientDeploymentDir: " + clientDeploymentDir);

				string remoteBinDir = localIniFile.Read("ServerBinDir"); // bin directory on server where things are
				if (Lex.IsNullOrEmpty(remoteBinDir))
				{
					throw new Exception("ServerBinDir not defined in " + localIniPath);
				}

				remoteIniPath = remoteBinDir + @"\MobiusClient.ini"; // ini file on server

				try // be sure we can read remote inifile & trap any error
				{
					sr = new StreamReader(remoteIniPath);
					sr.Close();
					remoteIniFile = new IniFile(remoteIniPath, "Mobius");
					if (remoteIniFile == null) throw new Exception("Null IniFile Object");
				}
				catch (Exception ex)
				{
					throw new Exception("Can't open remote inifile: " + remoteIniPath + ", " + ex.Message);
				}

				string remoteDeploymentId = remoteIniFile.Read("DeploymentId", "UnknownRemoteDeployment");
				bool deploymentIdsMatch = (localDeploymentId == remoteDeploymentId); // local & remote deployment ids the same?

				if (logActivity) DebugLog.Message("RemoteDeploymentId: " + remoteDeploymentId);

				// Get current client deployment dir

				string clientExePath = clientDeploymentDir + @"\MobiusClient.exe"; // make sure executable is there
				if (!File.Exists(clientExePath)) return false;

				if (!deploymentIdsMatch) return false; // local & remote deployment ids the same?

				return true; // up to date
			}

			catch (Exception ex)
			{
				if (logActivity)
				{
					string msg =
						"Client version check failed\r\n" +
					ex.Message + "\r\n" +
					ex.StackTrace;
					DebugLog.Message(msg);

					MessageBox.Show(msg);
				}

				return true; // say true if error
			}
		}

		/// <summary>
		/// Get the path to the specified file in the Mobius "base" directory.
		/// The base directory is normally be one directory above the MobiusClient.exe startup directory
		/// and contains MobiusClientStart.exe, MobiusClient.ini and MobiusClientUser.ini
		/// 
		/// However, if the file is not found in the base diredtory then the
		/// app startup directory is checked as well.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static string GetMobiusBaseDirectoryFilePath(
			string fileName)
		{
			string path = "", path2;
			string mobiusClientIni = "MobiusClient.ini";

			// See if name if full path to existing file

			if (fileName.Contains(@"\")) return fileName; // just return if full path

			string startupPath = Application.StartupPath; // directory app started in

			// Check StartupPath parent directory first

			string subDirPath = Path.GetDirectoryName(startupPath); // parent of startup dir

			if (Lex.IsDefined(subDirPath))
			{
				path = subDirPath + @"\" + fileName;
				if (File.Exists(path)) return path;

				path2 = subDirPath + @"\" + mobiusClientIni; // if MobiusClient.ini in parent dir then return the parent dir anyway. This will force MobiusClientUser.ini to be created in the parent dir
				if (File.Exists(path2)) return path;
			}

			// If no luck then check StartupPath directory

			path = startupPath + @"\" + fileName;
			if (File.Exists(path)) return path;

			path2 = startupPath + @"\" + mobiusClientIni; // if MobiusClient.ini in startup dir then use it
			if (File.Exists(path2)) return path;

			// Check CurrentDirectory

			path = Directory.GetCurrentDirectory() + @"\" + fileName;
			if (File.Exists(path)) return path;

			path2 = Directory.GetCurrentDirectory() + @"\" + mobiusClientIni;
			if (File.Exists(path2)) return path;

			// No luck, use default

			path = MobiusClientUtil.DefaultMobiusClientFolder + @"\" + fileName;
			return path; // file may not exist (e.g. MobiusClientUser.ini)
		}

		/// <summary>
		/// Return true if using Mobius services
		/// </summary>
		/// <returns></returns>

		public static bool UseMobiusServices
		{
			get
			{
				IniFile userIniFile = // see if MobiusClientUser.ini has dev or other special parameters that should avoid auto update
					IniFile.OpenClientIniFile("MobiusClientUser.ini");

				if (userIniFile != null)
					return userIniFile.ReadBool("UseMobiusServices", true);

				else return false;
			}
		}

		/// <summary>
		/// Open the appropriate main client ini file
		/// </summary>
		/// <returns></returns>

		public static IniFile OpenAppropriateClientIniFile()
		{
			string iniFileName = null;

			if (UseMobiusClientUserIniFileAsMainIniFile)
				iniFileName = "MobiusClientUser.ini";

			else iniFileName = "MobiusClient.ini";

			IniFile iniFile = IniFile.OpenClientIniFile(iniFileName);

			return iniFile;
		}

		/// <summary>
		/// Return true if we are using MobiusClientUser.ini for as main client ini file
		/// </summary>
		/// <returns></returns>

		public static bool UseMobiusClientUserIniFileAsMainIniFile
		{
			get
			{
				bool inDevMode = false;

				IniFile userIniFile = // see if MobiusClientUser.ini has dev or other special parameters that should avoid auto update
					IniFile.OpenClientIniFile("MobiusClientUser.ini", "Mobius");

				if (userIniFile != null)
					inDevMode = Lex.IsDefined(userIniFile.Read("UseMobiusServices")) || Lex.IsDefined(userIniFile.Read("DebugNativeSessionHost"));

				return inDevMode;
			}
		}

	}
}

