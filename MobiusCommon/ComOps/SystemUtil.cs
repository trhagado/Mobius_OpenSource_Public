using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;

namespace Mobius.ComOps
{
	/// <summary>
	/// System utilities
	/// </summary>

	public class SystemUtil
	{

		static Dictionary<string, string> ProcessUserName = new Dictionary<string, string>(); // used for quick lookup of usernames for processes
		private static bool? inDesignMode = null;

		/// <summary>
		/// Return true if the current thread has a message loop running and can be used for UI operations
		/// </summary>

		bool IsUiThread { get { return System.Windows.Forms.Application.MessageLoop; } }

		/// <summary>
		/// Return true if Vista or newer Windows version
		/// </summary>

		public static bool IsVistaOrNewer
		{
			get
			{
				System.OperatingSystem osInfo = System.Environment.OSVersion;
				return (osInfo.Version.Major >= 6); // Vista or newer
			}
		}

		/// <summary>
		/// Determine if we are currently in design mode in Visual Studio
		/// </summary>
		/// <returns></returns>

		public static bool InDesignMode
		{
			get
			{
				if (inDesignMode == null)
					inDesignMode = (Process.GetCurrentProcess().ProcessName.ToLower().Contains("devenv"));

				return inDesignMode.Value;
			}
		}

		/// <summary>
		/// Determine if we are currently in runtime mode rather than design mode
		/// </summary>
		/// <returns></returns>

		public static bool InRuntimeMode
		{ get {	return !InDesignMode; } }


		/// <summary>
		/// Get assembly version without loading
		/// </summary>
		/// <param name="assemblyPath"></param>
		/// <returns></returns>

		public static string GetAssemblyVersion(
			string assemblyPath)
		{
			if (!File.Exists(assemblyPath)) return "";
			AssemblyName an = AssemblyName.GetAssemblyName(assemblyPath);
			string versionString = an.Version.ToString();
			return versionString;
		}

		/// <summary>
		/// Load the specified assembly
		/// </summary>
		/// <param name="assemblyPath"></param>

		public static Assembly LoadAssembly(
			string assemblyPath)
		{
			int t0 = TimeOfDay.Milliseconds();
			Assembly assembly = null;

			AssemblyName an = new AssemblyName();
			an.CodeBase = assemblyPath;
			try { assembly = Assembly.Load(an); }
			catch (Exception ex)
			{
				MessageBox.Show("Error loading assembly: " + assemblyPath + ", " + ex.Message,
					UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return assembly;
		}

		/// <summary>
		/// Get last bootup time
		/// </summary>
		/// <returns></returns>

		public static DateTime GetLastBootUpTime()
		{
			PerformanceCounter uptime = new PerformanceCounter("System", "System Up Time");
			uptime.NextValue();       // call this an extra time before reading its value
			TimeSpan ts = TimeSpan.FromSeconds(uptime.NextValue());
			return DateTime.Now.Subtract(ts);
		}

		/// <summary>
		/// Trivial property that always returns true without generating a compiler warning
		/// </summary>

		public static bool AlwaysTrue
		{
			get { return true; }
		}

		/// <summary>
		/// Trivial property that always returns false without generating a compiler warning
		/// </summary>

		public static bool AlwaysFalse
		{
			get { return false; }
		}

		/// <summary>
		/// Play a simple system beep sound
		/// </summary>

		public static void Beep()
		{
			System.Media.SystemSounds.Beep.Play();
		}

		/// <summary>
		/// Check version of .Net
		/// </summary>
		/// <param name="version"></param>
		/// <param name="service"></param>
		/// <returns></returns>

		public static bool IsDotNetVersionInstalled(string version, int service)
		{

			// Indicates whether the specified version and service pack of the .NET Framework is installed.
			//
			// version -- Specify one of these strings for the required .NET Framework version:
			//    'v1.1.4322'     .NET Framework 1.1
			//    'v2.0.50727'    .NET Framework 2.0
			//    'v3.0'          .NET Framework 3.0
			//    'v3.5'          .NET Framework 3.5
			//    'v4\Client'     .NET Framework 4.0 Client Profile
			//    'v4\Full'       .NET Framework 4.0 Full Installation
			//    'v4.5'          .NET Framework 4.5
			//
			// service -- Specify any non-negative integer for the required service pack level:
			//    0               No service packs required
			//    1, 2, etc.      Service pack 1, 2, etc. required
			// .NET 4.5 installs as update to .NET 4.0 Full
			//
			// Example:
			//   bool installed = Mobius.ComOps.SystemUtil.IsDotNetVersionInstalled(@"v4\Client", 0);

			bool check45 = false, success, result;
			int install = 0, release = 0, serviceCount = 0;
			string key;

			if (version == "v4.5")
			{
				version = @"v4\Full";
				check45 = true;
			}
			else check45 = false;

			// installation key group for all .NET versions
			key = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\" + version;

			// .NET 3.0 uses value InstallSuccess in subkey Setup
			if (version.StartsWith("v3.0"))
				success = RegQueryDWordValue(key + @"\Setup", "InstallSuccess", out install);
			else
				success = RegQueryDWordValue(key, "Install", out install);

			// .NET 4.0/4.5 uses value Servicing instead of SP
			if (version.StartsWith("v4"))
				success = success && RegQueryDWordValue(key, "Servicing", out serviceCount);
			else
				success = success && RegQueryDWordValue(key, "SP", out serviceCount);

			// .NET 4.5 uses additional value Release
			if (check45)
			{
				success = success && RegQueryDWordValue(key, "Release", out release);
				success = success && (release >= 378389);
			}

			result = success && (install == 1) && (serviceCount >= service);

			return result;
		}

		static bool RegQueryDWordValue(string subKeyName, string valueName, out int value)
		{
			value = 0;
			RegistryKey rkey = Registry.LocalMachine.OpenSubKey(subKeyName); // must open before getting value
			if (rkey == null) return false;

			object o = rkey.GetValue(valueName);
			rkey.Close();
			if (o is int)
			{
				value = (int)o;
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Get process owner
		/// This routine calls the system management console. This is quite slow so the
		/// user names are stored in a hash on processId & processName for speed.
		/// Also could do elaborate MTS call. See: http://www.dotnet247.com/247reference/msgs/20/100126.aspx
		/// </summary>
		/// <param name="process"></param>
		/// <returns></returns>

		public static string GetProcessOwner(
			Process process)
		{
			try
			{
				if (ProcessUserName.ContainsKey(process.Id.ToString() + process.ProcessName))
					return (string)ProcessUserName[process.Id.ToString() + process.ProcessName];

				if (process.ProcessName == "Idle") return "SYSTEM";

				int t0 = TimeOfDay.Milliseconds();
				string query = "Select * From Win32_Process Where ProcessID = " + process.Id;
				ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
				ManagementObjectCollection processList = searcher.Get();

				foreach (ManagementObject obj in processList)
				{
					string[] argList = new string[] { string.Empty };
					int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
					if (returnVal == 0)
					{
						string userName = argList[0];
						ProcessUserName[process.Id.ToString() + process.ProcessName] =
										userName; // cache for fast retrieval
						t0 = TimeOfDay.Milliseconds() - t0;
						return userName;
					}
				}
			}
			catch (Exception ex) { return ""; }

			return "NO OWNER";
		}

		/// <summary>
		/// Start a process
		/// </summary>
		/// <param name="arg"></param>

		public static void StartProcess(
			string arg)
		{
			Exception ex = null;

			if (arg != null) arg = arg.Trim();

			try { Process.Start(arg); }
			catch (Exception ex2) { ex = ex2; }

			if (ex != null && Lex.IsUri(arg)) // if failed && uri try starting via IE
			{
				ex = null;
				try { Process.Start("IEXPLORE.EXE", arg); }
				catch (Exception ex2) { ex = ex2; }
			}

			if (ex != null)
			{
				MessageBox.Show("Unable to launch the following url:\n" + arg + "\n\nAn application may be required that is not installed.");
			}
		}

	}

}
