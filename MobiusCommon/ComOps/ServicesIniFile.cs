using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ComOps
{
	public class ServicesIniFile
	{
		public static string IniFilePath = null;
		public static IniFile IniFile = null;

		//public static string ServicesIniFilePath; // name of Services iniFile
		//public static IniFile ServicesIniFile; // inifile with service settings

		/// <summary>
		/// Constructor
		/// </summary>

		private ServicesIniFile()
		{
			return;
		}

/// <summary>
/// Try to get path to inifile in static constructor
/// </summary>

		static ServicesIniFile()
		{
			try
			{
				System.Configuration.AppSettingsReader asr = new System.Configuration.AppSettingsReader();
				if (asr != null)
					IniFilePath = asr.GetValue("IniFilePath", typeof(string)) as string;
			}

			catch (Exception ex) { ex = ex; }

			if (IniFilePath == null) // default path if not defined in inifile
				IniFilePath = System.Environment.CurrentDirectory + @"\MobiusServices.ini";

			if (!FileUtil.Exists(IniFilePath) && ClientState.IsDeveloper) // dev/debug
				IniFilePath = @"C:\Mobius_OpenSource\MobiusClient\ServiceFacade\MobiusServicesDev.ini";

			if (!FileUtil.Exists(IniFilePath)) // just return if not found
			{
				IniFilePath = null;
				return;
			}
	
			IniFile = new IniFile(IniFilePath, "Mobius");
			return;
		}

		/// <summary>
		/// Initialize 
		/// </summary>
		/// <param name="iniFilePath"></param>
		/// <returns></returns>

		public static IniFile Initialize(string iniFilePath)
		{
			if (Lex.IsUndefined(iniFilePath)) throw new ArgumentException("iniFilePath not defined");

			IniFilePath = iniFilePath;
			IniFile = new IniFile(iniFilePath, "Mobius");
			return IniFile;
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
		/// Get boolean value
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>

		public static bool ReadBool(
			string key,
			bool defaultValue = false)
		{
			if (IniFile == null) return false;

			bool value = IniFile.ReadBool(key, defaultValue);
			return value;
		}

		/// <summary>
		/// Try to read a bool value
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>

		public static bool TryReadBool(
			string key,
			ref bool value)
		{
			if (IniFile == null) return false;

			bool readSucceeded = IniFile.TryReadBool(key, ref value);
			return readSucceeded;
		}

		/// <summary>
		/// Get integer value
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>

		public static int ReadInt(
			string key,
			int defaultValue = NumberEx.NullNumber)
		{
			if (IniFile == null) return NumberEx.NullNumber;

			int value = IniFile.ReadInt(key, defaultValue);
			return value;
		}

/// <summary>
/// Try to read an int value
/// </summary>
/// <param name="key"></param>
/// <param name="value"></param>
/// <returns></returns>

		public static bool TryReadInt(
			string key,
			ref int value)
		{
			if (IniFile == null) return false;

			bool readSucceeded = IniFile.TryReadInt(key, ref value);
			return readSucceeded;
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
