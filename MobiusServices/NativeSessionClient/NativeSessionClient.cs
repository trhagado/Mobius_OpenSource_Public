using Mobius.ComOps;
using Mobius.Data;

// name space for WCF MobiusSessionService client 
using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Native;
using Mobius.Services.Types;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
//using System.Windows.Forms;

namespace Mobius.NativeSessionClient
{

	/// <summary>
	/// NativeSessionClient - Make a call to a native session
	/// </summary>

	public class NativeSessionClient
	{
		public static bool UseMobiusServices = true; // if true use remote rather than local services
		public static bool DebugNativeSessionHost = false; // if true debug simple stand-alone native session

		public static EndpointTypeEnum EndpointType = EndpointTypeEnum.TCPEndpoint;
		public static string ServiceHost = "localhost";
		public static int ServiceBasePort = 7700;
		public static EndpointIdentity ServiceEndpointIdentity = null;

		//  Specifying "RememberAndUpdateMobiusObjects" below preserves references to the following object types:
		//    MetaTable, MetaColumn, CondFormat
		//    Query, QueryTable, QueryColumn, CalcField
		//    DictionaryEx
		//  References are preserved at the cost of not being able to garbage collect the instances of these objects,
		//  but, since instances of objects of these types are shared by reference and not throw-away,
		//  things blow up if any of these AREN'T remembered (potential memory leak)

		public static TypeConversionHelper TypeConversionHelper =
				new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.RememberAndUpdateMobiusObjects);

		public static Dictionary<string, MethodCallStats> CallStats; // usage stats on service calls
		private object NativeServiceClientFactoryLock = new object();
		private ChannelFactory<INativeSession> NativeServiceClientFactory = null;

		private static object StaticLockObject = new object();

		public Session Session = null; // The authoritative source for the current session object
		public bool SessionCreated { get { return Session != null; } } // return true if session created
		private Thread SessionHeartbeatThread = null;
		private TimeSpan SessionHeartbeatInterval = TimeSpan.FromMinutes(1); // TimeSpan.FromSeconds(10); // (debug)
		private TimeSpan SessionHeartbeatRetryInterval = TimeSpan.FromMinutes(1);

		public bool InCreateSessionAsynch = false;
		public bool InCreateSession = false;
		public Exception CreateSessionException;

		public static bool LogStartupDetails = false; // true if startup details should be logged

		public static bool LogServiceCalls
		{ // true if service calls should be logged
			get { return NativeMethodInvoker.LogServiceCalls; }
			set { NativeMethodInvoker.LogServiceCalls = value; }
		}

		public static bool AllowAsynchServiceCalls
		{
			get { return NativeMethodInvoker.AllowAsynchServiceCalls; }
			set { NativeMethodInvoker.AllowAsynchServiceCalls = value; }
		}

		public static bool RunUIServiceCallsOnSeparateThread
		{
			get { return NativeMethodInvoker.RunUIServiceCallsOnSeparateThread; }
			set { NativeMethodInvoker.RunUIServiceCallsOnSeparateThread = value; }
		}

		public NativeMethodInvoker NativeMethodInvokerInstance;
		public bool InServiceCall
		{
			get { return NativeMethodInvokerInstance.InServiceCall; }
		}

		/// <summary>
		/// Client log to write service calls to
		/// </summary>

		public static IDebugLog IDebugLog
		{
			get { return NativeMethodInvoker.IDebugLog; }
			set { NativeMethodInvoker.IDebugLog = value; }
		}

/// <summary>
/// Initialize the static class members from iniFile settings
/// </summary>
/// <param name="iniFile"></param>

		public static void Initialize(IniFile iniFile)
		{
      UseMobiusServices = iniFile.ReadBool("UseMobiusServices", true);
			DebugNativeSessionHost = iniFile.ReadBool("DebugNativeSessionHost", false);
			LogServiceCalls = iniFile.ReadBool("LogServiceCalls", false);
			AllowAsynchServiceCalls = iniFile.ReadBool("AllowAsynchServiceCalls", true);
			RunUIServiceCallsOnSeparateThread = iniFile.ReadBool("RunUIServiceCallsOnSeparateThread", true);

			string endpointType = iniFile.Read("ServiceEndpointType", "TCPEndpoint");
			try
			{ EndpointType = (EndpointTypeEnum)Enum.Parse(typeof(EndpointTypeEnum), endpointType); }
			catch (Exception ex)
			{
				string msg =
					"The EndpointType (" + endpointType + ") is not valid." +
					"Valid options include: ";

				string[] endpointTypeNames = Enum.GetNames(typeof(EndpointTypeEnum));
				for (int i = 0; i < endpointTypeNames.Length; i++)
				{
					msg += ((i > 0) ? ", " : "") + endpointTypeNames[i];
				}
				msg += "\r\n " + EndpointType.ToString() + " endpoints will be used.";
				throw new InvalidCastException(msg, ex);
			}

			ServiceHost = iniFile.Read("ServiceHost", "localhost");
			ServiceBasePort = iniFile.ReadInt("ServiceBasePort", 7700);
			//string availableServices = iniFile.Read("AvailableServices", "All");

			// Determine the type of identity to use
			// The identity specified in *.exe.config is NOT used.
			// WCF appears to ignore the identity of the endpoint config for
			// some reason when we specify an alternate address...

			string serviceIdentityType = iniFile.Read("ServiceIdentityType", "dns");
			string serviceIdentity = iniFile.Read("ServiceIdentity", "localhost");
			if (serviceIdentityType == "dns")
			{
				ServiceEndpointIdentity =
						new DnsEndpointIdentity(serviceIdentity);
			}
			else if (serviceIdentityType == "upn")
			{
				ServiceEndpointIdentity =
						new UpnEndpointIdentity(serviceIdentity);
			}
			else if (serviceIdentityType == "spn")
			{
				ServiceEndpointIdentity =
						new SpnEndpointIdentity(serviceIdentity);
			}
			else
			{
				throw new InvalidOperationException("Only identity types \"dns\", \"upn\", and \"spn\" (with a corresponding valid ServiceIdentity) are supported.");
			}

			// If we want to use the native services, then the session service and the native service
			// are both assumed to be available and to "do it all"
			// AvailableServices = AvailableServicesEnum.All;

			// Set the hostname and port number to which the native proxies are to connect

			NativeSessionEndpointAddress.ServiceHostName = ServiceHost;
            NativeSessionEndpointAddress.ServicePort = ServiceBasePort + 5; // native session port offset = +5

		}

/// <summary>
/// Get client to connect to session service
/// </summary>
/// <returns></returns>

		private MobiusSessionService.MobiusSessionServiceClient GetSessionServiceClient()
		{
			
            if (_sessionServiceClient == null)
			{
				//EndpointType = EndpointTypeEnum.HTTPEndpoint; // debug

				string endpointname = GetServiceEndpointName(AvailableServicesEnum.Session);
                EndpointAddress serviceAddress = GetServiceAddress(AvailableServicesEnum.Session);
                
                _sessionServiceClient = // get service client object (e.g. http://[server]:7700/MobiusServices/MobiusSessionService)
				new MobiusSessionService.MobiusSessionServiceClient(endpointname, serviceAddress);
			}
			return _sessionServiceClient;
		}
		private MobiusSessionService.MobiusSessionServiceClient _sessionServiceClient;

/// <summary>
/// Create session asynchronously
/// </summary>

		internal void CreateSessionAsyncThreadMethod(object parm)
		{
			try
			{
				bool native = true;
				if (parm is bool) native = (bool)parm;

				lock (StaticLockObject) // only allow one asynch session creation at a time
				{
					if (Session == null)
					{
						InCreateSessionAsynch = true;
						Session = CreateSession(native);
						InCreateSessionAsynch = false;
					}
				}
			}
			catch (Exception ex)
			{
				if (!String.IsNullOrEmpty(DebugLog.LogFileName))
				{
					string msg = "CreateSessionAsync failed:" + "\r\n\r\n" +
						DebugLog.FormatExceptionMessage(ex);
					DebugLog.Message(msg);
				}

			}

		}

		/// <summary>
		/// Get the name of the endpoint in the config file
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>

		public static string GetServiceEndpointName(AvailableServicesEnum service)
		{
			string endpointNameInAppConfig = service.ToString() + "_" + EndpointType.ToString();
			return endpointNameInAppConfig;
		}

		/// <summary>
		/// GetServiceAddress
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>

		public static EndpointAddress GetServiceAddress(AvailableServicesEnum service)
		{
			string serviceAddress;

		    int servicePort = // Port, HTTP should be the base port value and NET.TCP should be on ServiceBasePort + 1
		        ServiceBasePort + ((int)EndpointType);
		    
            switch (EndpointType)
            {
                case EndpointTypeEnum.TCPEndpoint:
                    serviceAddress = "net.tcp";
                    break;
                case EndpointTypeEnum.HTTPEndpoint:
                    serviceAddress = "http";
                    break;
                default:
                    serviceAddress = "https";
                    break;
            }

			serviceAddress += "://" +	ServiceHost + ":" + servicePort + "/MobiusServices/Mobius" + service + "Service";

			EndpointAddress serviceEndpointAddress =
					new EndpointAddress(
							new Uri(serviceAddress),
							ServiceEndpointIdentity,
							new AddressHeader[0]);

			return serviceEndpointAddress;
		}

///// <summary>
///// Create native session
///// </summary>
///// <param name="iniFile"></param>
///// <param name="asynch"></param>

//		public void CreateNativeSession(
//			IniFile iniFile,
//			bool asynch)
//		{
//			CreateSession(iniFile, asynch, true);
//		}

		/// <summary>
		/// Create session 
		/// </summary>
		/// <param name="iniFile"></param>
		/// <param name="asynch"></param>

		public void CreateSession(
			IniFile iniFile,
			bool asynch,
			bool native = true)
		{
			CreateSessionException = null;

			Initialize(iniFile); // be sure initialized

			//asynch = false; // debug

			if (asynch) // asynchronous creation
			{
				System.Threading.Thread createSessionAsynchThread =
				 new System.Threading.Thread(
					new System.Threading.ParameterizedThreadStart(CreateSessionAsyncThreadMethod));
				createSessionAsynchThread.IsBackground = false;
				createSessionAsynchThread.Start(native);
			}

			else // synchronous session creation
			{
				Session session = CreateSession(native);
			}

			return;
		}

		/// <summary>
		/// Create the proxy to the native session
		/// </summary>
		/// <returns></returns>

		public INativeSession CreateNativeSessionProxy()
		{
			INativeSession sessionClient = null;

			if (NativeServiceClientFactory == null)
			{
				lock (NativeServiceClientFactoryLock)
				{
					if (NativeServiceClientFactory == null)
					{
						if (Session == null) throw new Exception("Session is null");

						string endpointAddressString = // get string form of address
								NativeSessionEndpointAddress.Build(Session.ProcessId);
						Uri endpointUri = new Uri(endpointAddressString);

						EndpointAddress endpointAddress =
								new EndpointAddress(endpointUri, ServiceEndpointIdentity);

						string endpointConfigurationName = "NativeSession_TCPEndpoint";
						NativeServiceClientFactory =
								new ChannelFactory<INativeSession>(endpointConfigurationName, endpointAddress);
					}
				}
			}

			sessionClient = NativeServiceClientFactory.CreateChannel(); // create channel to new client
			return sessionClient;
		}

		/// <summary>
		/// Return any exception from initialization
		/// </summary>
		/// <returns></returns>

		public Exception GetInitializationException()
		{
			WaitForCreateSessionCompletion();

			if (CreateSessionException != null) return CreateSessionException;
			else if (CreateSessionException != null)
				return CreateSessionException;
			else return null;
		}

		/// <summary>
		/// DisposeSession
		/// </summary>

		public void DisposeSession()
		{
			if (Session != null)
			{
				try
				{
					if (DebugNativeSessionHost) return; // don't exit if debugging native session to allow faster restart of the client

					Services.Native.INativeSession nativeClient = CreateNativeSessionProxy();
					nativeClient.DisposeSession(Session);
					((System.ServiceModel.IClientChannel)nativeClient).Close();
				}
				catch (Exception ex)
				{
					ex = ex; //no need to report failure
					//  application should be running down, so this was the last/only chance to clean up the server-side
					//  disposing a NATIVE session actually results in a legitimate exception
					//    ie, the other end of the conversation dies before it can return!
				}
			}
		}

		/// <summary>
		/// Get service infor for the Mobius service host
		/// </summary>
		/// <returns></returns>

		public ServiceHostInfo GetServiceHostInfo()
		{
			ServiceHostInfo si = new ServiceHostInfo();

			try
			{
				si.ServerName = ServiceHost;
				si.Version = new Version(GetServiceVersion());
			}
			catch (Exception ex) { ex = ex; }

			return si;
		}

		/// <summary>
		/// Call a service method and return result
		/// </summary>
		/// <param name="serviceCode"></param>
		/// <param name="subServiceCode"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>

		public NativeMethodTransportObject CallServiceMethod(
				ServiceCodes serviceCode,
				object subServiceCode,
				object parameters)
		{
			INativeSession nativeClient = CreateNativeSessionProxy();
			NativeMethodInvokerInstance = new NativeMethodInvoker();
			Services.Native.NativeMethodTransportObject resultObject =
				NativeMethodInvokerInstance.InvokeNativeMethod(
					nativeClient,
					(int)serviceCode,
					(int)subServiceCode,
					new NativeMethodTransportObject(parameters));
			((IClientChannel)nativeClient).Close();
			return resultObject;
		}

		/// <summary>
		/// Call native method & process any exception
		/// </summary>
		/// <param name="nativeClient"></param>
		/// <param name="serviceCode"></param>
		/// <param name="opCode"></param>
		/// <param name="argObject"></param>
		/// <returns></returns>

		public NativeMethodTransportObject InvokeNativeMethod(
			INativeSession nativeClient,
			int serviceCodeInt,
			int opCodeInt,
			NativeMethodTransportObject transportObject)
		{
			NativeMethodInvokerInstance = new NativeMethodInvoker();

			NativeMethodTransportObject resultObject =
				NativeMethodInvokerInstance.InvokeNativeMethod(
					nativeClient,
					serviceCodeInt,
					opCodeInt,
					transportObject);
			return resultObject;
		}

		/// <summary>
		/// Wait for completion of create session
		/// </summary>

		internal void WaitForCreateSessionCompletion()
		{
			while (InCreateSession)
			{
				System.Threading.Thread.Sleep(100);
				if (CreateSessionException != null) return;
			}

			return;
		}

		/// <summary>
		/// Create a session. Note that this should be done within a lock to avoid reentry.
		/// </summary>
		/// <returns></returns>

		internal Session CreateSession(bool native)
		{
			if (!UseMobiusServices) return null; // just return if not using Mobius Services

			while (InCreateSession) // extra reentrant check
			{
				System.Threading.Thread.Sleep(100);
			}

			InCreateSession = true;

			int t0 = TimeOfDay.Milliseconds();
			try
			{
        CreateSessionException = null;

				bool connectViaMobiusServiceHost = !DebugNativeSessionHost;
				if (connectViaMobiusServiceHost) // do normal creation of MobiusNativeServices process via MobiusServiceHost
				{
					MobiusSessionService.MobiusSessionServiceClient client = GetSessionServiceClient();

					if (native) Session = client.CreateNativeSession(); // create normal native session

					else Session = client.CreateSession();

					client.Close();
				}

				else // for local debug just connect to existing NativeSessionHost process - Process.GetProcessesByName("MobiusNativeServices")
				{
					string serviceProcessName = "MobiusNativeServices"; // new name for Visual Studio 2017
					Process[] processes = Process.GetProcessesByName(serviceProcessName);

					if (processes.Length == 0)
					{
						serviceProcessName = "MobiusNativeServices.vshost"; // old name
						processes = Process.GetProcessesByName(serviceProcessName);
					}
					if (processes.Length == 0) throw new Exception("Can't find service process: " + serviceProcessName);

					int nativeSessionLineNumber = processes[0].Id;
					Dictionary<SessionParameterName, string> sessionParams =
							new Dictionary<SessionParameterName, string>();
					sessionParams.Add(SessionParameterName.IsNativeSession, true.ToString());
					sessionParams.Add(SessionParameterName.NativeSessionLine, nativeSessionLineNumber.ToString());

					string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
					Session = // build default session object
						new Session()
							{
								Id = 1,
								UserId = userName,
								CreationDT = DateTime.Now,
								ExpirationDT = DateTime.MaxValue,
								SessionParameters = sessionParams
							};
				}

				t0 = TimeOfDay.Milliseconds() - t0;
				if (LogStartupDetails && !String.IsNullOrEmpty(DebugLog.LogFileName))
				{
					string msg = "CreateSession time";
					if (InCreateSessionAsynch) msg += " (asynch)";
					msg += ": " + t0;
					DebugLog.Message(msg);
				}
			}

			catch (Exception ex)
			{
				CreateSessionException = ex;
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
				throw new Exception(ex.Message, ex);
			}

			InCreateSession = false;
			return Session;
		}

/// <summary>
/// Start heartbeat that periodically pings the native session process as long as the client process is alive
/// </summary>

		public void StartSessionHeartbeat()
		{
			ThreadStart threadStart = new ThreadStart(SessionHeartbeat);
			if (SessionHeartbeatThread != null)
			{
				throw new InvalidOperationException("Only one session heartbeat may run at a time!");
			}

			else
			{
				SessionHeartbeatThread = new Thread(threadStart);
				SessionHeartbeatThread.Name = "SessionHeartbeatThread";
				SessionHeartbeatThread.IsBackground = true;
				SessionHeartbeatThread.SetApartmentState(ApartmentState.STA);
				SessionHeartbeatThread.Start();
			}
		}

/// <summary>
/// Thread method periodically freshens the native session to keep it alive when no user activity 
/// </summary>

		private void SessionHeartbeat()
		{
			bool sessionThreadAborted = false;
			bool heartbeatSucceeded = false;
			string serviceVersion = null;
			int heartBeatErrorCount = 0;

			while (!sessionThreadAborted && Session != null)
			{
				try
				{
					FreshenSession(); // "freshen" the session without counting as a method invocation (note: this will block if an existing call to the NativeSession service is running)
					heartbeatSucceeded = true;
				}

				catch (Exception ex)
				{ 
					heartbeatSucceeded = false;
					heartBeatErrorCount++;
					if (heartBeatErrorCount <= 3) // just log first few errors
						DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
				} 

				if (heartbeatSucceeded)
				{
					Thread.Sleep(SessionHeartbeatInterval);
					continue;
				}
				else
				{
					Thread.Sleep(SessionHeartbeatRetryInterval);
					continue;
				}
			}

			return;
		}

		/// <summary>
		/// Retrieve session info for all current sessions
		/// </summary>

		public List<SessionInfo> GetSessionInfoForAllSessions()
		{
			if (!UseMobiusServices) return null;

			MobiusSessionService.MobiusSessionServiceClient client = GetSessionServiceClient();
			Dictionary<Mobius.Services.Types.Session, Mobius.Services.Types.SessionInfo[]> sessionInfoDict = client.GetSessionInfoForAllSessions(Session);
			client.Close();

			if (sessionInfoDict == null) return null;

			List<SessionInfo> sessionInfoList = new List<SessionInfo>();
			foreach (Session session in sessionInfoDict.Keys)
			{
				SessionInfo si = new SessionInfo();
				si.Id = session.Id;
				si.UserId = session.UserId;
				si.CreationDT = session.CreationDT;
				si.ExpirationDT = session.ExpirationDT;
				si.Native = IsNativeSession(session);

				if (!sessionInfoDict.ContainsKey(session)) si = si;

				if (sessionInfoDict.ContainsKey(session) && 
					sessionInfoDict[session].Length >= 1 && // should be just one entry
					sessionInfoDict[session][0] != null)
				{
					foreach (SessionInfoDataElement sde in sessionInfoDict[session][0].SessionData)
					{
						if (Lex.Eq(sde.Name, "PID"))
							int.TryParse(sde.Value, out si.ProcessId);

						else if (Lex.Eq(sde.Name, "CPU Time"))
							double.TryParse(sde.Value, out si.CpuTimeSecs);

						else if (Lex.Eq(sde.Name, "Mem (MB)"))
							int.TryParse(sde.Value, out si.MemoryMb);

						else if (Lex.Eq(sde.Name, "WSet (MB)"))
							int.TryParse(sde.Value, out si.WorkingSetMb);

						else if (Lex.Eq(sde.Name, "Threads"))
							int.TryParse(sde.Value, out si.Threads);

						else if (Lex.Eq(sde.Name, "Handles"))
							int.TryParse(sde.Value, out si.Handles);
					}
				}

				sessionInfoList.Add(si);
			}

			return sessionInfoList;
		}

/// <summary>
///  Return true if native session
/// </summary>
/// <param name="session"></param>
/// <returns></returns>

		public static bool IsNativeSession(Session session)
		{
			bool isNativeSession = false;

			if (session == null || session.SessionParameters == null || 
			 !session.SessionParameters.ContainsKey(SessionParameterName.IsNativeSession)) 
				return false;

			if (!bool.TryParse(session.SessionParameters[SessionParameterName.IsNativeSession], out isNativeSession))
				return false;

			return isNativeSession;
		}

/// <summary>
/// GetServiceVersion
/// </summary>
/// <returns></returns>

		public string GetServiceVersion()
		{
			if (UseMobiusServices)
			{
				Services.Native.INativeSession nativeClient = CreateNativeSessionProxy();
				string serviceVersion = nativeClient.GetCurrentVersionNumber();
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return serviceVersion;
			}

			else // just return version of this assembly
			{
				string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				return version;
			}
		}

		/// <summary>
		/// Freshen session
		/// </summary>
		/// <returns>Updated Session object</returns>

		public Session FreshenSession()
		{
			if (UseMobiusServices)
			{
				if (LogServiceCalls)
					DebugLog.Message("Freshening session: " + Session.Id);
				Services.Native.INativeSession nativeClient = CreateNativeSessionProxy();
				Session session = nativeClient.FreshenSession(Session);
				((System.ServiceModel.IClientChannel)nativeClient).Close();
				return session;
			}

			else return Session; // just return current session
		}

		/// <summary>
		/// Update call stats
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="startTime"></param>

		public static void UpdateCallStats(string methodName, int startTime)
		{
			int t1 = TimeOfDay.Milliseconds();

			MethodCallStats mcs;
			if (CallStats == null) CallStats = new Dictionary<string, MethodCallStats>();
			if (!CallStats.ContainsKey(methodName))
			{
				mcs = new MethodCallStats();
				mcs.MethodName = methodName;
				CallStats.Add(methodName, mcs);
			}

			else mcs = CallStats[methodName];

			mcs.TotalCalls++;
			mcs.TotalTime += t1 - startTime;
			return;
		}
	}

	/// <summary>
	/// Endpoint Type Enum
	/// </summary>

	public enum EndpointTypeEnum
	{
		HTTPEndpoint = 0,
		TCPEndpoint = 1,
        HTTPTransportEndpoint = 4
	}

	/// <summary>
	/// AvailableServices Enum (just Session service for now)
	/// </summary>

	public enum AvailableServicesEnum
	{
		All = -1,
		None = 0,
		Session = 1 << 0,
		NotImplemented = 0
	}


	/// <summary>
	/// Basic info for the service host
	/// </summary>
	
	public class ServiceHostInfo
	{
		public Version Version; // version of the service host
		public string ServerName; // server the service host is running on
	}

	/// <summary>
	/// Basic info for a session
	/// </summary>
	
	public class SessionInfo
	{
		public int Id = -1;
		public string UserId = "";
		public bool Native = false;
		public DateTime CreationDT = DateTime.MinValue;
		public DateTime ExpirationDT = DateTime.MinValue;
		public int ProcessId = -1;
		public double CpuTimeSecs = -1;
		public int MemoryMb = -1;
		public int WorkingSetMb = -1;
		public int Threads = -1;
		public int Handles = -1;
	}

	/// <summary>
	/// Stats on a method call
	/// </summary>
	
	public class MethodCallStats
	{
		public string MethodName;
		public int TotalCalls;
		public int TotalTime;

		public float AvgTime { get { return (TotalCalls > 0) ? (TotalTime / (float)TotalCalls) : 0; } }
	}

}
