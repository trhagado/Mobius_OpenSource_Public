using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Mobius.ComOps
{
	/// <summary>
	/// Create a New INI file to store or load data
	/// </summary>
	public class IniFile
	{
		public string IniFileName; // name of the file reading from
		public string Section; // section in file to read from 

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section,
			string key,string val,string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section,
			string key,string def, StringBuilder retVal,
			int size,string filePath);

		/// <summary>
		/// INIFile Constructor.
		/// </summary>
		/// <PARAM name="INIPath"></PARAM>
		public IniFile(string iniFileName)
		{
			IniFileName = iniFileName;
		}

		public IniFile(string iniFilePath, string sectionArg)
		{
			IniFileName = iniFilePath;
			Section = sectionArg;
		}

		/// <summary>
		/// Locate and open a client IniFile
		/// </summary>
		/// <param name="iniFileName">File name</param>
		/// <param name="sectionArg"></param>
		/// <returns></returns>

		public static IniFile OpenClientIniFile(
			string iniFileName,
			string sectionArg = "Mobius")
		{
			string iniFilePath = MobiusClientUtil.GetMobiusBaseDirectoryFilePath(iniFileName);

			IniFile iniFile = new IniFile(iniFilePath, sectionArg);
			return iniFile;
		}

		/// <summary>
		/// Write Data to the INI File
		/// </summary>
		/// <PARAM name="Section"></PARAM>
		/// Section name
		/// <PARAM name="Key"></PARAM>
		/// Key Name
		/// <PARAM name="Value"></PARAM>
		/// Value Name
		public void Write(string Section,string Key,string Value)
		{
			WritePrivateProfileString(Section,Key,Value,this.IniFileName);
		}

/// <summary>
/// 
/// </summary>
/// <param name="Key"></param>
/// <param name="Value"></param>
		public void Write(string Key,string Value)
		{
			WritePrivateProfileString(this.Section,Key,Value,this.IniFileName);
		}


/// <summary>
/// Read Data Value From the Ini File
/// </summary>
/// <param name="section"></param>
/// <param name="key"></param>
/// <param name="defaultValue"></param>
/// <returns></returns>
        
		public string Read (
			string section, 
			string key, 
			string defaultValue)
		{
			StringBuilder temp = new StringBuilder(1024);
			int i = GetPrivateProfileString(section,key,defaultValue,temp, 
				1024, this.IniFileName);
			return temp.ToString();
		}

		public string Read (
			string key, 
			string defaultValue)
		{
			return Read(this.Section,key,defaultValue);
		}

		public string Read (
			string key)
		{
			return Read(this.Section,key,"");
		}

/// <summary>
/// Get boolean value
/// </summary>
/// <param name="key"></param>
/// <param name="defaultValue"></param>
/// <returns></returns>

		public bool ReadBool(
			string key,
			bool defaultValue)
		{
			string val = Read(Section, key, defaultValue.ToString());
			if (Lex.Eq(val, "true")) return true;
			else if (Lex.Eq(val, "false")) return false;
			else return defaultValue;
		}

/// <summary>
/// Try to read a bool value
/// </summary>
/// <param name="key"></param>
/// <param name="value"></param>
/// <returns></returns>

		public bool TryReadBool(
			string key,
			ref bool value)
		{
			bool readSucceeded = true;
			string val = Read(Section, key, "");
			if (Lex.Eq(val, "true")) value = true;
			else if (Lex.Eq(val, "false")) value = false;
			else readSucceeded = false;

			return readSucceeded;
		}

/// <summary>
/// Get int value
/// </summary>
/// <param name="key"></param>
/// <param name="defaultValue"></param>
/// <returns></returns>

		public int ReadInt(
			string key,
			int defaultValue)
		{
			int intVal = defaultValue;
			string val = Read(Section, key, defaultValue.ToString());
			bool parseResult = int.TryParse(val, out intVal);
			return intVal;
		}


/// <summary>
/// Try to read an int value
/// </summary>
/// <param name="key"></param>
/// <param name="value"></param>
/// <returns></returns>

		public bool TryReadInt(
			string key,
			ref int value)
		{
			int intVal;
			string val = Read(Section, key, "");
			if (int.TryParse(val, out intVal))
			{
				value = intVal;
				return true;
			}

			else return false;
		}

	} // IniFile
}
