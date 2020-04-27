using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.ComOps;

namespace Mobius.Services.Util
{
	public class ServicesIniFile
	{
		public static string IniFilePath;
		public static IniFile IniFile;

		/// <summary>
		/// Constructor
		/// </summary>

		private ServicesIniFile()
		{
			return;
		}

/// <summary>
/// Get path to inifile in constructor
/// </summary>

		static ServicesIniFile()
		{
			System.Configuration.AppSettingsReader asr = new System.Configuration.AppSettingsReader();
			IniFilePath = asr.GetValue("IniFilePath", typeof(string)) as string;

			if (IniFilePath == null) // default path if not defined in inifile
				IniFilePath = System.Environment.CurrentDirectory + @"\MobiusServices.ini";

			IniFile = new IniFile(IniFilePath, "Mobius");
		}

/// <summary>
/// Read key value with default
/// </summary>
/// <param name="key"></param>
/// <param name="defaultValue"></param>
/// <returns></returns>

		public static string Read(string key, string defaultValue)
		{
			if (IniFile == null) return defaultValue;
			string value = IniFile.Read(key, defaultValue);
			return value;
		}

		/// <summary>
		/// Read key value
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>

		public static string Read(string key)
		{
			if (IniFile == null) return "";

			string value = IniFile.Read(key);
			return value;
		}

		/// <summary>
		/// Read key value
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>

		public static string ReadWithException(string key)
		{
			string value = IniFile.Read(key);
			if (string.IsNullOrEmpty(value))
			{
				throw new Exception("Key \"" + key + "\" is required but was not found in the configuration file!");
			}
			return value;
		}


	}
}
