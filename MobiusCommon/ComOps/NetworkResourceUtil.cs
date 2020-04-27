using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Management;
using System.Net;
using System.Reflection;
using System.Diagnostics;

namespace Mobius.ComOps
{
/// <summary>
/// Network resource utilities (Connection to SharePoint sites via UNCs)
/// </summary>

	public class NetworkResourceUtil
	{
		[DllImport("mpr.dll")]
		private static extern int WNetAddConnection3(
			IntPtr hWndOwner,
			ref NETRESOURCE lpNetResource, 
			string lpPassword,
			string lpUserName, 
			int dwFlags);

		[DllImport("mpr.dll")]
		private static extern int WNetCancelConnection(
			string Name, 
			bool Force);

		[DllImport("mpr.dll")]
		private static extern uint WNetGetResourceInformation(
			ref NETRESOURCE lpNetResource, 
			[MarshalAs(UnmanagedType.LPTStr)] IntPtr lpBuffer,
			ref int lpcbBuffer,
			[MarshalAs(UnmanagedType.I4)]out IntPtr lplpSystem);

		[DllImport("mpr.dll")]
		public static extern int WNetEnumResource(
			IntPtr hEnum,
			ref int lpcCount,
			IntPtr lpBuffer,
			ref int lpBufferSize);

		[DllImport("mpr.dll")]
		public static extern int WNetOpenEnum(
			RESOURCE_SCOPE dwScope,
			RESOURCE_TYPE dwType,
			RESOURCE_USAGE dwUsage,
			[MarshalAs(UnmanagedType.AsAny)][In] Object lpNetResource,
			out IntPtr lphEnum);

		[DllImport("mpr.dll")]
		public static extern int WNetCloseEnum(IntPtr hEnum);

		static bool DebugAddConnection = true;

/// <summary>
/// Check if directory exists
/// </summary>
/// <param name="connectionString"></param>
/// <returns></returns>

		public static bool DirectoryExists(string path)
		{
			NetworkConnectionWorker worker = new NetworkConnectionWorker();
			worker.path = path;
			worker.directoryExists = false;

			int t0 = TimeOfDay.Milliseconds();
			if (DebugAddConnection) DebugLog.Message(
				"Calling Directory.Exists for: " + path);

			Thread workerThread = new Thread(new ThreadStart(worker.DirectoryExists));
			workerThread.Name = "DirectoryExists";
			workerThread.Start();
			while (!workerThread.IsAlive) ; // loop until worker thread activates.
			while (workerThread.IsAlive) // allow limited time to check since may be slow if doesn't exist (~25 secs)
			{
				Thread.Sleep(10);
				Application.DoEvents();
			}

			//workerThread.Join(); // (don't join because causes delay)

			if (DebugAddConnection) DebugLog.Message(
				"Exists = " + worker.directoryExists + " time (ms): " + (TimeOfDay.Milliseconds() - t0).ToString());

			return worker.directoryExists;
		}

		/// <summary>
		/// Add a new connection
		/// </summary>
		/// <param name="connectionString"></param>

		public static void AddConnection(string connectionString)
		{
			AddConnection(connectionString, "", "", true);
			return;
		}

/// <summary>
/// Add a new connection
/// </summary>
/// <param name="connectionString"></param>
/// <param name="accountName"></param>
/// <param name="accountPw"></param>
/// <param name="interactive"></param>

		public static void AddConnection(
			string connectionString,
			string accountName,
			string accountPw,
			bool interactive)
		{
			try
			{
				int t0 = TimeOfDay.Milliseconds();
				connectionString = NormalizeConnectionStringToUncShareName(connectionString);

				CancelConnection(connectionString); // try to cancel first (seems to make reconnect faster)

				NetworkConnectionWorker worker = new NetworkConnectionWorker();
				worker.connectionString = connectionString;
				worker.directoryExists = false;

				worker.accountName = accountName;
				worker.accountPw = accountPw;
				worker.interactive = interactive;

				Thread workerThread = new Thread(new ThreadStart(worker.AddConnection));
				workerThread.Name = "AddConnection";
				workerThread.Start();
				while (!workerThread.IsAlive) ; // loop until worker thread activates.
				while (workerThread.IsAlive)
				{
					Thread.Sleep(10);
					Application.DoEvents();
				}

				//workerThread.Join();
				if (worker.ex != null) throw worker.ex;
			}
			catch (Exception ex)
			{
				DebugLog.Message("AddConnection exception: " + DebugLog.FormatExceptionMessage(ex));
			}
			return;
		}

/// <summary>
/// Cancel a connection
/// </summary>
/// <param name="connectionString"></param>

		public static void CancelConnection(
			string connectionString)
		{
			int t0 = TimeOfDay.Milliseconds();
			connectionString = NormalizeConnectionStringToUncShareName(connectionString);
			int rv = WNetCancelConnection(connectionString, false);

			if (DebugAddConnection)
				DebugLog.Message("WNetCancelConnection for: " + connectionString + 
					", time: " + (TimeOfDay.Milliseconds() - t0).ToString() + ", return value: " + rv); // debug

			return;
		}

		/// <summary>
		/// Internal worker class that runs on a background thread
		/// </summary>

// The Mobius system account has something unusual about it that prohibits it from connecting
// normally to other users' sites. To work around this we must supply a password for this account
//string accountName = SS.I.UserIniFile.Read("accountName");

		class NetworkConnectionWorker
		{
			internal string path;
			internal string connectionString;
			internal string accountName;
			internal string accountPw;
			internal bool interactive;
			internal bool directoryExists;
			internal Exception ex;

/// <summary>
/// Check if the directory exists
/// </summary>

			internal void DirectoryExists()
			{
				try
				{
					directoryExists = Directory.Exists(path);
				}

				catch (Exception ex2)
				{
					ex = ex2;
					DebugLog.Message("Directory.Exists exception: " + ex.Message);
				}

				//if (exists)
				//{ // seems like this always succeeds as long as the server exists
				//  DateTime dirCreateTime = Directory.GetCreationTime(connectionString);
				//  string root = Directory.GetDirectoryRoot(connectionString);
				//  return; // just return if already exists
				//}

				return;
			}

/// <summary>
/// Add the connection
/// </summary>

			internal void AddConnection()
			{
				const int ERROR_SUCCESS = 0;
				NETRESOURCE nr;

				int t0 = TimeOfDay.Milliseconds();

				IntPtr hWndOwner = IntPtr.Zero;
				//if (Form.ActiveForm != null) hWndOwner = (IntPtr)Form.ActiveForm.Handle; // (May not work because different thread than was created on)

				nr.dwScope = (int)RESOURCE_SCOPE.RESOURCE_CONNECTED;
				nr.dwType = (int)RESOURCE_TYPE.RESOURCETYPE_DISK;
				nr.dwDisplayType = (int)RESOURCE_DISPLAYTYPE.RESOURCEDISPLAYTYPE_GENERIC;
				nr.dwUsage = (int)RESOURCE_USAGE.RESOURCEUSAGE_CONNECTABLE;
				nr.LocalName = null; // no local device
				nr.RemoteName = connectionString;
				nr.Comment = null;
				nr.Provider = null;

				string lpUserName = // need to qualify with domain so the domain is included in the login box
					System.Environment.UserDomainName + @"\" + System.Environment.UserName;
				//string lpUserName = null; // use default user name from the context for the process

				//string lpPassword = ""; // don't use password
				string lpPassword = null; // use current default password associated with lpUserName (still prompts)

				if (!Lex.IsNullOrEmpty(accountName) && !Lex.IsNullOrEmpty(accountPw)) // supplied account info?
				{
					lpUserName = accountName;
					lpPassword = accountPw;
				}

				int dwFlags = 0;
				if (interactive)
				{
					dwFlags |= (int)CONNECT_FLAGS.CONNECT_INTERACTIVE; // allow interaction
					//dwFlags |= (int)CONNECT_FLAGS.CONNECT_PROMPT; // force prompt (don't force but will prompt anyway)
					//dwFlags |= (int)CONNECT_FLAGS.CONNECT_CMD_SAVECRED; // save any credentials
				}

				if (DebugAddConnection)
				{
					string msg = "WNetAddConnection3 for user: " + lpUserName +
						//"/" + lpPassword +
						(interactive ? " - Interactive" : " - Non-interactive");
					DebugLog.Message(msg);
				}
				int rv = WNetAddConnection3(hWndOwner, ref nr, lpPassword, lpUserName, dwFlags);

				if (DebugAddConnection) 
					DebugLog.Message("WNetAddConnection3 time: " + (TimeOfDay.Milliseconds() - t0).ToString() + ", return value: " + rv); // debug

				if (rv == ERROR_SUCCESS) return;

				ex = new Win32Exception(rv); 
			}
		}

/// <summary>
/// See if specified disk share is connected (doesn't seem to include SharePoint "Web Client Network" connections)
/// </summary>
/// <param name="connectionString"></param>
/// <returns></returns>

// Could use the following? uint rv = WNetGetResourceInformation(ref nr, lpBuffer, ref lpcbBuffer, out lplpSystem);

		public static bool IsShareConnected(string connectionString)
		{
			List<NETRESOURCE> netRes = new List<NETRESOURCE>();
			GetNetResources(
				RESOURCE_SCOPE.RESOURCE_CONNECTED, RESOURCE_TYPE.RESOURCETYPE_ANY, RESOURCE_USAGE.RESOURCEUSAGE_ALL,
				null, netRes);

			connectionString = NormalizeConnectionStringToUncShareName(connectionString);
			foreach (NETRESOURCE nr in netRes)
			{
				if (Lex.Eq(nr.RemoteName, connectionString))
					return true;
			}
			return false;
		}

/// <summary>
/// Get list of network resources
/// </summary>
/// <param name="resScope"></param>
/// <param name="resType"></param>
/// <param name="resUsage"></param>
/// <param name="o"></param>
/// <param name="netResources"></param>

		static void GetNetResources(
			RESOURCE_SCOPE resScope,
			RESOURCE_TYPE resType,
			RESOURCE_USAGE resUsage,
			Object o,
			List<NETRESOURCE> netResources)
		{
			int iRet;
			IntPtr ptrHandle = new IntPtr();
			try
			{
				iRet = WNetOpenEnum(
					resScope, resType, resUsage, o, out ptrHandle);
				if (iRet != 0)
					return;

				int entries;
				int buffer = 16384;
				IntPtr ptrBuffer = Marshal.AllocHGlobal(buffer);
				NETRESOURCE nr;
				while (true)
				{
					entries = -1;
					buffer = 16384;
					iRet = WNetEnumResource(ptrHandle, ref entries, ptrBuffer, ref buffer);
					if ((iRet != 0) || (entries < 1))
					{
						break;
					}
					Int32 ptr = ptrBuffer.ToInt32();
					for (int i = 0; i < entries; i++)
					{
						nr = (NETRESOURCE)Marshal.PtrToStructure(new IntPtr(ptr), typeof(NETRESOURCE));
						if (RESOURCE_USAGE.RESOURCEUSAGE_CONTAINER == ((RESOURCE_USAGE)nr.dwUsage
								& RESOURCE_USAGE.RESOURCEUSAGE_CONTAINER))
						{
							//call recursively to get all entries in a container
							GetNetResources(resScope, resType, resUsage, nr, netResources);
						}
						ptr += Marshal.SizeOf(nr);
						netResources.Add(nr);
						//Console.WriteLine(" {0} : LocalName='{1}' RemoteName='{2}'",
						//nr.dwDisplayType.ToString(), nr.LocalName, nr.RemoteName);
					}
				}
				Marshal.FreeHGlobal(ptrBuffer);
				iRet = WNetCloseEnum(ptrHandle);
			}
			catch (Exception e)
			{
			}
		}

/// <summary>
/// Normalize Connection String
/// </summary>
/// <param name="connectionString"></param>
/// <returns></returns>

		public static string NormalizeConnectionStringToUncShareName(string connectionString)
		{
			if (Lex.StartsWith(connectionString, "http://") || Lex.StartsWith(connectionString, "https://")) // format into proper connection name
				connectionString = connectionString.Substring(5).Replace(@"/", @"\");
			int slashCnt = 0;
			for (int i1 = 0; ; i1++)
			{
				if (i1 == connectionString.Length || (slashCnt == 3 && connectionString[i1] == '\\'))
				{
					connectionString = connectionString.Substring(0, i1);
					break;
				}

				if (connectionString[i1] == '\\') slashCnt++;
			}

			return connectionString;
		}

/// <summary>
/// Get a list of file shares with/without assoc drive letters
/// </summary>
/// <returns></returns>

		public static List<string> GetSharedFolders()
		{
			List<string> sharedFolders = new List<string>();
			string type, name, path, caption;

			ManagementObjectSearcher searcher = // object to query the WMI Win32_Share API for shared files.
				new ManagementObjectSearcher("select * from win32_share");
			ManagementBaseObject outParams;
			ManagementClass mc = new ManagementClass("Win32_Share"); //for local shares (win32_ShareToDirectory)
			
			ManagementObjectCollection moc = mc.GetInstances();
			string txtList = "";
//			foreach (ManagementObject share in searcher.Get())
			foreach (ManagementObject share in moc)
			{
				//type = share["Type"].ToString();
				//types += type + "\r\n";
				//if (type == "0") // 0 = DiskDrive ,1 = Print Queue, 2 = Device, 3 = IPH,  
				//{ // 2147483648 = Disk Drive Admin, 2147483649 = Print Queue Admin, 2147483650 = Device Admin, else probably IPH Admin
					name = share["Name"].ToString(); //getting share name 
					path = share["Path"].ToString(); //getting share path 
					txtList += name + " : " + path + "\r\n";
					caption = share["Caption"].ToString(); //getting share description 
					sharedFolders.Add(path);
				//}
			}

			return sharedFolders;
		}

		/// <summary>
		/// NETRESOURCE structure
		/// </summary>

		struct NETRESOURCE
		{
			public int dwScope;
			public int dwType;
			public int dwDisplayType;
			public int dwUsage;
			public string LocalName;
			public string RemoteName;
			public string Comment;
			public string Provider;
		}

		public struct NETRESOURCE2
		{
			public RESOURCE_SCOPE dwScope;
			public RESOURCE_TYPE dwType;
			public RESOURCE_DISPLAYTYPE dwDisplayType;
			public RESOURCE_USAGE dwUsage;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
			public string lpLocalName;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
			public string lpRemoteName;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
			public string lpComment;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
			public string lpProvider;
		}

		public enum RESOURCE_DISPLAYTYPE
		{
			RESOURCEDISPLAYTYPE_GENERIC = 0x00000000,
			RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001,
			RESOURCEDISPLAYTYPE_SERVER = 0x00000002,
			RESOURCEDISPLAYTYPE_SHARE = 0x00000003,
			RESOURCEDISPLAYTYPE_FILE = 0x00000004,
			RESOURCEDISPLAYTYPE_GROUP = 0x00000005,
			RESOURCEDISPLAYTYPE_NETWORK = 0x00000006,
			RESOURCEDISPLAYTYPE_ROOT = 0x00000007,
			RESOURCEDISPLAYTYPE_SHAREADMIN = 0x00000008,
			RESOURCEDISPLAYTYPE_DIRECTORY = 0x00000009,
			RESOURCEDISPLAYTYPE_TREE = 0x0000000A,
			RESOURCEDISPLAYTYPE_NDSCONTAINER = 0x0000000B
		}

		// WNetAddConnection3 flags

		public enum CONNECT_FLAGS
		{
			CONNECT_UPDATE_PROFILE = 0x00000001,
			CONNECT_INTERACTIVE = 0x00000008,
			CONNECT_PROMPT = 0x00000010,
			CONNECT_REDIRECT = 0x00000080,
			CONNECT_COMMANDLINE = 0x00000800,
			CONNECT_CMD_SAVECRED = 0x00001000
		}

////////////

public enum RESOURCE_SCOPE
{
	RESOURCE_CONNECTED = 0x00000001,
	RESOURCE_GLOBALNET = 0x00000002,
	RESOURCE_REMEMBERED = 0x00000003,
	RESOURCE_RECENT = 0x00000004,
	RESOURCE_CONTEXT = 0x00000005
}

public enum RESOURCE_TYPE
{
	RESOURCETYPE_ANY = 0x00000000,
	RESOURCETYPE_DISK = 0x00000001,
	RESOURCETYPE_PRINT = 0x00000002,
	RESOURCETYPE_RESERVED = 0x00000008,
}

public enum RESOURCE_USAGE
{
	RESOURCEUSAGE_CONNECTABLE = 0x00000001,
	RESOURCEUSAGE_CONTAINER = 0x00000002,
	RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
	RESOURCEUSAGE_SIBLING = 0x00000008,
	RESOURCEUSAGE_ATTACHED = 0x00000010,
	RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
}

/// <summary>
/// Possible errors
/// </summary>

		public enum NERR
{
	NERR_Success = 0,/* Success */
	ERROR_MORE_DATA = 234, // dderror
	ERROR_NO_BROWSER_SERVERS_FOUND = 6118,
	ERROR_INVALID_LEVEL = 124,
	ERROR_ACCESS_DENIED = 5,
	ERROR_INVALID_PARAMETER = 87,
	ERROR_NOT_ENOUGH_MEMORY = 8,
	ERROR_NETWORK_BUSY = 54,
	ERROR_BAD_NETPATH = 53,
	ERROR_NO_NETWORK = 1222,
	ERROR_INVALID_HANDLE_STATE = 1609,
	ERROR_EXTENDED_ERROR = 1208
}

#if true
		const int NO_ERROR = 0;

		const int ERROR_ACCESS_DENIED = 5;
		const int ERROR_ALREADY_ASSIGNED = 85;
		const int ERROR_BAD_DEVICE = 1200;
		const int ERROR_BAD_NET_NAME = 67;
		const int ERROR_BAD_PROVIDER = 1204;
		const int ERROR_CANCELLED = 1223;
		const int ERROR_EXTENDED_ERROR = 1208;
		const int ERROR_INVALID_ADDRESS = 487;
		const int ERROR_INVALID_PARAMETER = 87;
		const int ERROR_INVALID_PASSWORD = 1216;
		const int ERROR_MORE_DATA = 234;
		const int ERROR_NO_MORE_ITEMS = 259;
		const int ERROR_NO_NET_OR_BAD_PATH = 1203;
		const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;
		const int ERROR_NO_NETWORK = 1222;
#endif

	}
}
