using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Management;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Data;
using System.Text;

namespace Mobius.ComOps
{

	/// <summary>
	/// Misc utility directory methods
	/// </summary>

	public class DirectoryMx
	{

		/// <summary>
		/// Add a terminal slash to a directory if not there already
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>

		public static string IncludeTerminalBackSlash(string dir)
		{
			dir = dir.TrimEnd();
			if (String.IsNullOrWhiteSpace(dir)) return dir;
			if (dir[dir.Length - 1] != '\\')
				dir += '\\';
			return dir;
		}

		/// <summary>
		/// Remove any terminal slash in a directory name
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>

		public static string RemoveTerminalBackSlash(string dir)
		{
			dir = dir.TrimEnd();
			if (String.IsNullOrWhiteSpace(dir)) return dir;
			int lci = dir.Length - 1;
			if (dir[lci] == '\\')
				dir = dir.Substring(0, lci);
			return dir;
		}

		/// <summary>
		///  Get a list of file shares on the current machine
		/// </summary>
		/// <returns></returns>

		public static string GetSharedFolderAccessRule()
		{
			DataTable DT = new DataTable();
			StringBuilder sb = new StringBuilder();

			try
			{
				DT.Columns.Add("ShareName");
				DT.Columns.Add("Caption");
				DT.Columns.Add("Path");
				DT.Columns.Add("Domain");
				DT.Columns.Add("User");
				DT.Columns.Add("AccessMask");
				DT.Columns.Add("AceType");

				ManagementScope Scope = new ManagementScope(@"\\.\root\cimv2");
				Scope.Connect();
				ObjectQuery Query = new ObjectQuery("SELECT * FROM Win32_LogicalShareSecuritySetting");
				ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);
				ManagementObjectCollection QueryCollection = Searcher.Get();

				foreach (ManagementObject SharedFolder in QueryCollection)
				{
					{
						String ShareName = (String)SharedFolder["Name"];
						String Caption = (String)SharedFolder["Caption"];
						String LocalPath = String.Empty;
						ManagementObjectSearcher Win32Share = new ManagementObjectSearcher("SELECT Path FROM Win32_share WHERE Name = '" + ShareName + "'");
						foreach (ManagementObject ShareData in Win32Share.Get())
						{
							LocalPath = (String)ShareData["Path"];
						}

						ManagementBaseObject Method = SharedFolder.InvokeMethod("GetSecurityDescriptor", null, new InvokeMethodOptions());
						ManagementBaseObject Descriptor = (ManagementBaseObject)Method["Descriptor"];
						ManagementBaseObject[] DACL = (ManagementBaseObject[])Descriptor["DACL"];
						foreach (ManagementBaseObject ACE in DACL)
						{
							ManagementBaseObject Trustee = (ManagementBaseObject)ACE["Trustee"];

							// Full Access = 2032127, Modify = 1245631, Read Write = 118009, Read Only = 1179817
							DataRow Row = DT.NewRow();

							Row["ShareName"] = ShareName;
							Row["Caption"] = Caption;
							Row["Path"] = LocalPath;
							Row["Domain"] = (String)Trustee["Domain"];
							Row["User"] = (String)Trustee["Name"];
							Row["AccessMask"] = (UInt32)ACE["AccessMask"];
							Row["AceType"] = (UInt32)ACE["AceType"];

							DT.Rows.Add(Row);
							DT.AcceptChanges();

							Append("ShareName", ShareName, sb);
							Append("Caption", Caption, sb);
							Append("Path", LocalPath, sb);
							Append("Domain", (String)Trustee["Domain"], sb);
							Append("User", (String)Trustee["Name"], sb);
							//Append("AccessMask", (UInt32)ACE["AccessMask"], sb);
							//Append("AceType", (UInt32)ACE["AceType"], sb);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}

			//return DT;
			return sb.ToString();


			void Append(string s1, string s2, StringBuilder sb2)
			{
				sb2.Append(s1 + ": " + s2 + "\r\n");
			}

		}
	}
}
