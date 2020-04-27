using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using IO = System.IO;

using Microsoft.SharePoint.Client;
using SP = Microsoft.SharePoint.Client;

namespace Mobius.ComOps
{
	public class SharePointUtil
	{

/// <summary>
/// Test code
/// </summary>

		public static void Test()
		{
			bool b;
			DateTime dt;
			string msg;

			string url1 = "<someUrl>";

			msg = CanWriteFile(url1 + "xxx");

			b = FileExists(url1);
			b = FileExists(url1 + "xxx");

			msg = CanReadFile(url1);
			msg = CanReadFile(url1 + "xxx");

			msg = CanWriteFile(url1);
			msg = CanWriteFile(url1 + "xxx");

			//DeleteFile(url2);

			dt = GetFileUpdateDate(url1);

			return;
		}

		/// <summary>
		/// Check if file exists
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>

		public static bool FileExists(
					string url)
		{
			if (GetFileUpdateDate(url) != DateTime.MinValue) return true;
			else return false;
		}

		/// <summary>
		/// Check the update date for the document pointed to by the specified URL
		/// </summary>
		/// <param name="url"></param>
		/// <returns>Date/TimeLastModified or DateTime.MinValue if not found</returns>

		public static DateTime GetFileUpdateDate(
			string url)
		{
			try
			{
				ClientContext context = GetClientContext(url);
				Web web = context.Web;
				Uri uri = new Uri(url);
				string serverRelativeUrl = uri.PathAndQuery;
				File file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
				context.Load(file);
				context.ExecuteQuery();

				if (file.Exists)
				{
					DateTime dt = file.TimeLastModified; 
					dt = dt.ToLocalTime(); // convert to local time so comparisons with other local times (e.g. last import time) work
					return dt;
				}

				else return DateTime.MinValue;
			}
			catch (Exception ex) // catch "File Not Found" exception
			{
				return DateTime.MinValue;
			}

		}

		/// <summary>
		/// See if file can be opened for reading, return error message if not
		/// </summary>
		/// <param name="url"></param>
		/// 

		public static string CanReadFile(
			string url)
		{
			try
			{
				ClientContext context = GetClientContext(url);
				Web web = context.Web;
				Uri uri = new Uri(url);
				string serverRelativeUrl = uri.PathAndQuery;
				File file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
				context.Load(file);
				context.ExecuteQuery();

				if (file.Exists)
					return "";

				else return "File Not Found";
			}
			catch (Exception ex) // catch "File Not Found" exception
			{
				return ex.Message;
			}
		}

		/// <summary>
		/// See if file can be opened for writing, return error if not
		/// </summary>
		/// <param name="path"></param>

		public static string CanWriteFile(
			string url)
		{
			try
			{
				bool existedOnEntry = FileExists(url);

				string tempName = TempFile.GetTempFileName();
				IO.StreamWriter sw = new IO.StreamWriter(tempName);
				sw.Close();

				CopyToSharePoint(tempName, url);
				IO.File.Delete(tempName);

				if (!existedOnEntry) // if didn't exist before then delete it
					DeleteFile(url);
			}
			catch (Exception ex) // catch "File Not Found" exception
			{
				return ex.Message;
			}

			return "";
		}

/// <summary>
/// Delete file
/// </summary>
/// <param name="url"></param>

		public static void DeleteFile(
			string url)
		{
			try
			{
				ClientContext context = GetClientContext(url);
				Web web = context.Web;
				Uri uri = new Uri(url);
				string serverRelativeUrl = uri.PathAndQuery;
				File file = web.GetFileByServerRelativeUrl(serverRelativeUrl);
				context.Load(file);
				file.DeleteObject();
				context.ExecuteQuery();
				return;
			}
			catch (Exception ex) // catch "File Not Found" exception
			{
				return; // ignore any error
			}

		}

		/// <summary>
		/// If SharePoint file create a local cached copy of the file that can be read directly
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>

		public static string CacheLocalCopyIfSharePointFile(string path)
		{
			if (!IsSharePointName(path)) return path;

			string ext = IO.Path.GetExtension(path); // use same extension
			string tempFile = TempFile.GetTempFileName(ext);
			SharePointUtil.CopyFromSharePoint(path, tempFile);
			return tempFile;
		}

		/// <summary>
		/// Copy file from SharePoint
		/// </summary>
		/// <param name="url"></param>
		/// <param name="filePath"></param>

		public static void CopyFromSharePoint(
			string url,
			string filePath)
		{
			try
			{
				ClientContext context = GetClientContext(url);

				Uri uri = new Uri(url);
				string serverRelativeUrl = uri.PathAndQuery;
				FileInformation info = File.OpenBinaryDirect(context, serverRelativeUrl);
				System.IO.Stream sr = info.Stream;

				IO.FileStream sw = new IO.FileStream(filePath, IO.FileMode.Create, IO.FileAccess.Write);
				int Length = 65536;
				Byte[] buffer = new Byte[Length];
				int bytesRead = sr.Read(buffer, 0, Length);
				long totalBytes = 0;
				while (bytesRead > 0)
				{
					sw.Write(buffer, 0, bytesRead);
					totalBytes += bytesRead;
					bytesRead = sr.Read(buffer, 0, Length);
				}

				sr.Close();
				sw.Close();
				return;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Copy file to SharePoint
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="url"></param>

		public static void CopyToSharePoint(
			string filePath,
			string url)
		{
			try
			{
				ClientContext context = GetClientContext(url);
				Uri uri = new Uri(url);
				string serverRelativeUrl = uri.PathAndQuery;
				IO.FileStream fileStream = new IO.FileStream(filePath, IO.FileMode.Open);
				File.SaveBinaryDirect(context, serverRelativeUrl, fileStream, true);
				fileStream.Close();

				return;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

		}

		/// <summary>
		/// Get the ClientContext for the site name contained within the URL
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>

		private static ClientContext GetClientContext(string url)
		{
			ClientContext context = null;

			string[] sa = url.Split('/');

			if (sa.Length < 4) return null;
			string siteUrl = sa[0] + "/" + sa[1] + "/" + sa[2] + "/"; // schema & server

			for (int sai = 3; sai < sa.Length; sai++)
			{
				string tok = sa[sai];
				siteUrl += sa[sai] + "/";

				if (sai == 3 && Lex.Eq(tok, "sites") || Lex.Eq(tok, "personal"))
					continue;

				try // see if we can connect to site
				{
					context = new ClientContext(siteUrl);
					context.ExecuteQuery();
					return context;
				}
				catch (Exception ex)
				{
					if (sai == sa.Length - 1) throw ex;
				}
			}

			throw new Exception("Cannot contact site at the specified URL");
		}

/// <summary>
/// Return true if regular file system name (i.e. not SharePoint)
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static bool IsRegularFileSystemName(string name)
		{
			return !IsSharePointName(name);
		}

/// <summary>
/// Return true if name is potentially a valid SharePoint file or folder name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static bool IsSharePointName(string name)
		{
			if (Lex.StartsWith(name, "http")) return true;
			else return false;
		}

	}

}
