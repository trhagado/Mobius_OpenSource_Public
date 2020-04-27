using Mobius.ComOps;
using Mobius.ServiceFacade;

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using IO = System.IO;

using Microsoft.Office.Interop.Word;
using Microsoft.Office.Core;

namespace Mobius.ClientComponents
{

/// <summary>
/// SharePointFileDialog
/// 
/// This class provides the GetOpenFilename and GetSaveAsFilename dialogs which use the
/// Microsoft Office file open and save dialogs which provide acces to SharePoint as well as other
/// file systems. 
/// </summary>
/// 

	public class SharePointFileDialog
	{

/// <summary>
/// GetOpenFileName
/// </summary>
/// <param name="title"></param>
/// <param name="initialFile"></param>
/// <param name="filter"></param>
/// <param name="defaultExt"></param>
/// <returns></returns>

		public static string GetOpenFilename(
			string title,
			string initialFile,
			string filter,
			string defaultExt)
		{
			return GetFilename(1, title, initialFile, filter, defaultExt);
		}

/// <summary>
/// GetSaveFileName
/// </summary>
/// <param name="title"></param>
/// <param name="initialFile"></param>
/// <param name="filter"></param>
/// <param name="defaultExt"></param>
/// <returns></returns>

		public static string GetSaveAsFilename(
			string title,
			string initialFile,
			string filter,
			string defaultExt)
		{
			return GetFilename(2, title, initialFile, filter, defaultExt);
		}

		/// <summary>
		/// Get a SharePoint open or save file name
		/// </summary>
		/// <param name="action">1=open, 2=save</param>
		/// <param name="title">Dialog title</param>
		/// <param name="initialFile">Initial file name.
		/// If no filename is supplied upon entry then the "SharePointSiteUrl" 
		/// parameter is used as the initial folder. If this is not defined then
		/// "DefaultSharePointSiteUrl" .ini file setting is used. Examples:
		/// </param>
		/// <param name="filter">Filter string, e.g. "Lists (*.lst)|*.lst" </param>
		/// <param name="defaultExt">Default file extension</param>
		/// <returns></returns>

		private static string GetFilename(
			int action,
			string title,
			string initialFile,
			string filter,
			string defaultExt)
		{
			string dir = "", fileName = "", lastSite = "";
			object gfo = null;

			try
			{
				if (!Lex.IsNullOrEmpty(defaultExt) && !defaultExt.StartsWith("."))
					defaultExt = '.' + defaultExt;

				try { dir = IO.Path.GetDirectoryName(initialFile); } catch { } // get any directory name
				if (Lex.IsNullOrEmpty(dir)) // if no directory specified get user's preferred SharePoint folder
				{ 
					lastSite = Preferences.Get("SharePointSiteUrl");
					if (lastSite == "")
					{
						lastSite = ServicesIniFile.Read("DefaultSharePointSiteUrl");
						lastSite = Lex.Replace(lastSite, "<UserName>", SS.I.UserName);
					}

					if (Lex.StartsWith(lastSite, "http://spotfire")) // convert incorrect URL to proper UNC
					{
						lastSite = Lex.Replace(lastSite, "http://spotfire", @"\\spotfire");
						lastSite = Lex.Replace(lastSite, "/", @"\");
					}

					if (lastSite != "") // plug into initial file
					{
						initialFile = lastSite;
						if (Lex.StartsWith(initialFile, "http") && !initialFile.EndsWith(@"\") && !initialFile.EndsWith("/"))
							initialFile += "/"; // be sure we have final slash if URL
						else if (Lex.StartsWith(initialFile, @"\\"))
						{
							if (!initialFile.EndsWith(@"\") && !initialFile.EndsWith("/"))
							initialFile += @"\"; // be sure we have final slash if URL
							initialFile += "*" + defaultExt; // need extension to get files listed for a UNC
						}
					}
				}

				if (Lex.StartsWith(initialFile, "http")) // replace any back slashes with forward slashes if http
					initialFile = initialFile.Replace(@"\", @"/"); 

				filter = filter.Replace("|", ","); // use proper filter delimiter for Office dialogs

				if (action == 1) // Open dialog
					fileName = ShowOfficeOpenFileDialog(initialFile, filter, defaultExt, title);

				else // SaveAs Dialog
					fileName = ShowOfficeSaveAsFileDialog(initialFile, filter, defaultExt, title);

				if (Lex.StartsWith(fileName, "http") || Lex.StartsWith(fileName, @"\\")) // if filename is a URL or UNC update default SharePoint folder
					try
					{
						string dirPath = IO.Path.GetDirectoryName(fileName);
						if (dirPath != "" && Lex.Ne(dirPath, lastSite))
						{
							Preferences.Set("SharePointSiteUrl", dirPath);
							lastSite = dirPath;
						}
					}
					catch { }

				return fileName;
			}

			catch (Exception ex)
			{
				return "";
			}
		}

		static void ReleaseObject(
			object obj)
		{
			if (obj == null) return;
			int refCnt = Marshal.ReleaseComObject(obj);
			if (refCnt != 0) refCnt = refCnt; // debug
			obj = null;
		}

/// <summary>
/// GetOpenFilename using FileDialog object since GetOpenFileName doesn't allow an initial name
/// </summary>
/// <param name="initialFile"></param>
/// <param name="filter"></param>
/// <param name="defaultExt"></param>
/// <param name="title"></param>
/// <returns></returns>

		private static string ShowOfficeOpenFileDialog(
			string initialFile,
			string filter,
			string defaultExt,
			string title)
		{
			Microsoft.Office.Interop.Excel._Application app;
			Microsoft.Office.Core.FileDialog fd;
			string fileName = "";

			if (Lex.StartsWith(initialFile, "http")) // url fixups
			{
				if (IO.Path.GetFileName(initialFile).Contains("."))
				{ // if url with file name remove file name so all directory entries appear in dialog (only this one appears otherwise)
					initialFile = IO.Path.GetDirectoryName(initialFile);
					initialFile = initialFile.Replace(@":\", "://"); // fixup from GetDirectoryName
					initialFile = initialFile.Replace(@"\", "/");
				}

				if (!initialFile.EndsWith("/"))
					initialFile += "/"; // be sure we have final slash so web site is properly recognized
			}

			app = new Microsoft.Office.Interop.Excel.Application();
			fd = app.get_FileDialog(MsoFileDialogType.msoFileDialogOpen); // get the open dialog
			fd.Title = title;
			fd.InitialFileName = initialFile;

			SetupFilters(filter, defaultExt, fd); // setup the filters

			int rc = fd.Show();

			if (rc == 0 || fd.SelectedItems.Count == 0)
				fileName = "";

			else
			{
				foreach (object o in fd.SelectedItems)
				{
					fileName = o.ToString();
					break;
				}

				string fileExt = IO.Path.GetExtension(fileName);
				if (fileExt == "" && defaultExt != "") // if no extension add default extension
				{
					if (!defaultExt.StartsWith("."))
						fileName += ".";
					fileName += defaultExt;
				}
			}

			ReleaseObject(fd);
			ReleaseObject(app);

			return fileName;
		}

/// <summary>
/// GetSaveAsFilename using app.GetSaveAsFilename which allows filters
/// </summary>
/// <param name="initialFile"></param>
/// <param name="filter"></param>
/// <param name="defaultExt"></param>
/// <param name="title"></param>
/// <returns></returns>

		private static string ShowOfficeSaveAsFileDialog(
			string initialFile,
			string filter,
			string defaultExt,
			string title)
		{
			Microsoft.Office.Interop.Excel._Application app;
			string fileName = "";

			app = new Microsoft.Office.Interop.Excel.Application();

			int sfi = SetupFilters(filter, defaultExt, null); // get selected filter index

			object gfo = app.GetSaveAsFilename(initialFile, filter, sfi, title);

			if (!(gfo is string) || Lex.IsNullOrEmpty(gfo.ToString()))
				fileName = ""; // cancelled, return blank

			else fileName = gfo.ToString();

			ReleaseObject(app);

			return fileName;
		}

/// <summary>
/// Setup Filters
/// </summary>
/// <param name="fd"></param>
/// <param name="defaultExt"></param>
/// <param name="fltrs"></param>
/// <returns>Index of initial filter</returns>

		static int SetupFilters(
			string filter,
			string defaultExt,
			Microsoft.Office.Core.FileDialog fd)
		{
			FileDialogFilters fltrs = null;

			if (fd != null) // only setup if fd is defined
			{
				fltrs = fd.Filters;
				fltrs.Clear();
			}

			string[] sa = filter.Split(',');
			int sfi = 0;
			for (int fi = 0; fi < sa.Length; fi += 2)
			{
				string desc = sa[fi];
				string ext = sa[fi + 1];
				if (fltrs != null) fltrs.Add(desc, ext);

				if (Lex.Contains(ext, defaultExt)) // selected filter
					sfi = fi/2 + 1;
			}

			if (sfi == 0 && sa.Length > 0) sfi = 1;
			if (fltrs != null) fd.FilterIndex = sfi; // set the filter index
			return sfi;
		}
	}

}
