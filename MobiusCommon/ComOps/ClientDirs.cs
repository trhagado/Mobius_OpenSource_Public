using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Common client directory locations
	/// </summary>

	public class ClientDirs
	{
		public static string ExecutablePath = ""; // Client executable file name
		public static string StartupDir = ""; // Client directory containing executable
		public static string CurrentDir = ""; // "Current" directory

		public static string TempDir = Application.StartupPath + @"\Temp"; // folder on client for temp files
		public static string PluginConfigDir = Application.StartupPath + @"\PluginConfig"; // plugin configuration info
		public static string MiscConfigDir = Application.StartupPath + @"\MiscConfig"; // scripts & templates
		public static string CacheDir = Application.StartupPath + @"\Cache"; // cached files

		public static string DefaultMobiusClientFolder = // default folder for Mobius client software
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +  @"\Mobius";

		public static string StartupExecutableName = "MobiusClientStart.exe";
		public static string ExecutableName = "MobiusClient.exe"; // in BinX folder under the parent folder

		/// <summary>
		/// Find location for client executable
		/// </summary>
		/// <returns></returns>

		public static string GetClientExecutablePath()
		{
			string path = DefaultMobiusClientFolder + @"\" + StartupExecutableName;
			if (File.Exists(path)) return path;
			else return null; // not found
		}

		/// <summary>
		/// Default user documents folder for importing exporting, etc...
		/// </summary>
		/// <returns></returns>

		public static string DefaultMobiusUserDocumentsFolder
		{
			get
			{
				if (Lex.IsDefined(_defUserDocsFolder)) return _defUserDocsFolder;

				string myDocsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				_defUserDocsFolder = myDocsFolder + @"\Mobius";

				if (Directory.Exists(_defUserDocsFolder))
					return _defUserDocsFolder;

				try // try to create the folder
				{
					Directory.CreateDirectory(_defUserDocsFolder);
					return _defUserDocsFolder;
				}

				catch (Exception ex)
				{
					_defUserDocsFolder = myDocsFolder;
					return _defUserDocsFolder;
				}
			}

			set
			{
				_defUserDocsFolder = value;
				return;
			}
		}
		static string _defUserDocsFolder = "";

		/// <summary>
		/// Get values for special folders for current machine
		/// </summary>
		/// <returns></returns>

		public static string ShowSpecialFolders()
		{
			string msg =
				"Programs: " + Environment.GetFolderPath(Environment.SpecialFolder.Programs) + "\r\n" +
				"Personal: " + Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\r\n" +
				"MyDocuments: " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\r\n" +
				"ApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\r\n" +
				"LocalApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\r\n" +
				"CommonApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\r\n" +
				"System: " + Environment.GetFolderPath(Environment.SpecialFolder.System) + "\r\n" +
				"ProgramFiles: " + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\r\n" +
				"CommonProgramFiles: " + Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);

			return msg;
		}

	}
}
