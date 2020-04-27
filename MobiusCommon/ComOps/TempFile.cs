using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ComOps
{

/// <summary>
/// Special file access functions
/// </summary>

	public class TempFile
	{
		public static string TempDir; // folder for temp files
		static ArrayList TempFileList; // list of temp files created

/// <summary>
/// Get temp file name
/// </summary>
/// <returns></returns>

		public static string GetTempFileName()
		{
			return GetTempFileName(TempDir, null);
		}

/// <summary>
/// Get temp file name
/// </summary>
/// <param name="directory"></param>
/// <param name="extension"></param>
/// <returns></returns>

		public static string GetTempFileName(
			string extension)
		{
			return GetTempFileName(TempDir, extension, true);
		}

/// <summary>
/// Get a temp file name
/// </summary>
/// <param name="directory"></param>
/// <param name="extension"></param>
/// <returns></returns>

		public static string GetTempFileName (
			string directory, 
			string extension)
		{
			return GetTempFileName(directory, extension, true);
		}

/// <summary>
/// Get a temp file name
/// </summary>
/// <param name="directory"></param>
/// <param name="extension"></param>
/// <param name="deleteOnAppExit"></param>
/// <returns></returns>

		public static string GetTempFileName(
			string directory,
			string extension,
			bool deleteOnAppExit)
		{
			long l1;
			string fileName;
			FileStream fs;

			directory = GetTempDirectory(directory); // get valid temp directory

			if (TempFileList==null) TempFileList = new ArrayList();

			if (directory.Length>0 && directory[directory.Length - 1] != '\\')
				directory += "\\";
			l1 = System.DateTime.Now.Ticks;
			for (int i1=0; i1<100; i1++) // 100 tries
			{ 
				l1= l1 % 100000000;
				fileName = directory + l1.ToString();
				if (extension != null && extension.Trim().Length > 0)
				{
					if (!extension.StartsWith(".")) fileName += ".";
					fileName += extension;
				}
				try
				{
					fs = new FileStream(fileName,FileMode.CreateNew);
				}
				catch (Exception ex)
				{
					l1++;
					continue;
				}

				fs.Close();

				if (deleteOnAppExit) // add to temp list for deletion at app exit
				 TempFileList.Add(fileName);
				return fileName;
			}

			return null; // failed
		}

/// <summary>
/// Get a valid temp directory
/// </summary>
/// <param name="directory"></param>
/// <returns></returns>

		public static string GetTempDirectory(string directory = null)
		{
			if (directory != null && Directory.Exists(directory)) return directory; // use existing dir?

			directory = ClientDirs.TempDir; // use default temp dir?
			if (directory != null && Directory.Exists(directory)) return directory;

			directory = Path.GetTempPath(); // get default user temp dir
			return directory;
		}

		/// <summary>
		/// PurgeTempFiles
		/// </summary>
		/// <returns></returns>

		public static int PurgeTempFiles ()
		{
			int i1;
			if (TempFileList == null) return 0;

			for (i1=0; i1<TempFileList.Count; i1++)
			{
				try
				{
					File.Delete((string)TempFileList[i1]);
				}

				catch (Exception ex) {}
			}
			TempFileList.Clear();
			return i1;
		}

/// <summary>
/// Remove invalid fileName characters from a string
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static string CreateValidFileName(string name)
		{
			name = name.Trim();

			char[] badChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*', '\r', '\n', '\t' }; 
			//char[] badChars = Path.InvalidPathChars;

			foreach (char c in badChars)
			{
				string sc = c.ToString();
				if (name.Contains(sc)) name = name.Replace(sc, "");
			}

			return name;
		}


	}
}
