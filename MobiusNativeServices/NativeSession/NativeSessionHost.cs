using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Windows.Forms;

using Mobius.Services.Util;
using Mobius.Services.Types;
using Mobius.Services.Types.Internal;
using Mobius.ComOps;
using Mobius.Data;

namespace Mobius.Services.Native
{

	/// <summary>
	/// NativeSessionHost
	/// Initializes the native process & sets up the service for the main InvokeNativeMethod method.
	/// Also includes methods from IINativeSessionManager to call back to service host that started the native process
	/// </summary>

	[ServiceBehavior(
		Namespace = "http://<server>/MobiusServices/Native/v1.0",
		InstanceContextMode = InstanceContextMode.Single,
		ConcurrencyMode = ConcurrencyMode.Multiple,
		IncludeExceptionDetailInFaults = true)]

	public class NativeSessionHost : Native.INativeSession, Types.Internal.INativeSession
	{
		private static int ServiceHostProcessId = -1; // the process id of the ServiceHost that started us
		private static InternalSession InternalSession; // internal session object created by service host
		private static object SessionObjectLock = new object();
		private static DateTime LastInvocationStart = DateTime.Now; // last time InvokeNativeMethod was called
		private static DateTime LastInvocationFinish = DateTime.Now; // last time InvokeNativeMethod was exited
		private static int InvocationDepth = 0; // current depth of InvokeNativeMethod calls
		private static int MaxInvocationDepth = 0; // max invocation depth achieved
		private static DateTime LastFreshen = DateTime.Now; // last time the session was freshened

		private static Dictionary<int, IInvokeServiceOps> OpInvokers = null; // dispatch tables for service operations
		private static ChannelFactory<INativeSessionManager> NativeSessionManagerClientFactory; // creates clients for calling back to service host

		private static bool LogServiceCalls = false;
		internal static bool NativeDebugMode = false;

		/// <summary>
		/// Main method for native session host
		/// </summary>
		/// <param name="args"></param>

		public static void StartUp(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			bool debug = (args.Length == 0);

			InitializeServicesLogFileName();

			try
			{

				// Debug mode

				if (debug)
				{
					LogServiceCalls = true;
					NativeDebugMode = true;
					ServicesLog.LogToConsole = true;

					Initialize();
					OpenServiceHost(); // Start the service host
					int processId = Process.GetCurrentProcess().Id;
					string endpointAddress = NativeSessionEndpointAddress.Build(processId);
					ServicesLog.Message("NativeSessionHost started in debug mode at endpoint address: " + endpointAddress);

					StartIdleMonitor(); // debug idle monitor
				}

// Started by HostingApp

				else
				{
					ServiceHostProcessId = int.Parse(args[0]);
					string creatorClassTypeName = args[1];
					int sessionId = int.Parse(args[2]);

					AppSettingsReader asr = new AppSettingsReader();
					string enableLoggingStr = asr.GetValue("EnableNativeSessionLogging", typeof(string)) as string;
					bool.TryParse(enableLoggingStr, out LogServiceCalls);

					Initialize();

					OpenServiceHost(); // Start the service host listening for requests

					RegisterSession(ServiceHostProcessId, sessionId); // register session with the service host that started us

					string msg = "MobiusNativeSession started for session " + sessionId + " in process " + Process.GetCurrentProcess().Id;

					Version version = Assembly.GetExecutingAssembly().GetName().Version;
					string tok = VersionMx.FormatVersion(version);
					msg += ", Services: " + tok;

					ServicesLog.Message(msg); // log initial message

					StartIdleMonitor(); // start thread that will exit if idle too long
				}

				return;
			}

			catch (Exception ex)
			{
				string msg = DebugLog.FormatExceptionMessage(ex);
				if (debug) MessageBox.Show(msg);
				ServicesLog.Message(msg);
				return;
			}
		}

		/// <summary>
		/// Set the ServicesLog to write to the proper file
		/// </summary>

		static void InitializeServicesLogFileName()
		{
			try
			{
				string iniFilePath = NativeSessionInitializer.GetMobiusServicesIniFileLocation();
				if (Lex.IsUndefined(iniFilePath) || !File.Exists(iniFilePath)) return;

				IniFile iniFile = new IniFile(iniFilePath, "Mobius");
				string logDir = iniFile.Read("LogDirectory");
				if (!Lex.IsDefined(logDir)) return;

				string logFile = logDir + @"\" + CommonConfigInfo.ServicesLogFileName; // log file name
				ServicesLog.Initialize(logFile); // initialize for logging 


			}

			catch (Exception ex)
			{
				ServicesLog.Message(DebugLog.FormatExceptionMessage(ex));
			}
		}

		/// <summary>
		/// Start idle monitor thread that will kill this session if idle for too long
		/// </summary>

		static void StartIdleMonitor()
		{
			ThreadStart threadStart = new ThreadStart(IdleMonitor);
			Thread monitorThread = new Thread(threadStart);
			monitorThread.IsBackground = true;
			monitorThread.Name = "IdleMonitor";
			monitorThread.Start();
		}

		/// <summary>
		/// IdleMonitor - Loop to check if session is idle
		/// - IdleTime - Amount of time since last service call
		/// - UnfreshenedTime - Amount of time since last Freshen() call
		/// </summary>

		private static void IdleMonitor()
		{
			TimeSpan maxIdleTime, maxUnfreshenedTime, checkInterval, idleTime = TimeSpan.Zero, unfreshenedTime = TimeSpan.Zero;

			int maxIdleMinutes = 10;
			int maxUnfreshenedMinutes = 0; // not defined
			bool isNative = (InternalSession != null && InternalSession.IsNativeSession); // MobiusClient sessions are Native sessions
			if (isNative)
			{
				maxIdleMinutes = ServicesIniFile.IniFile.ReadInt("MaxNativeSessionIdleTime", 0); // max idle time for native Mobius client (0 = no limit)
				maxUnfreshenedMinutes = ServicesIniFile.IniFile.ReadInt("MaxNativeUnfreshenedTime", 0); // max unrefreshed time for native Mobius client
			}

			else
			{
				maxIdleMinutes = ServicesIniFile.IniFile.ReadInt("MaxNonnativeSessionIdleTime", 0); // max idle time for non-native clients
				maxUnfreshenedMinutes = ServicesIniFile.IniFile.ReadInt("MaxNonnativeUnfreshenedTime", 0); // max unrefreshed time for non-native clients (0 = no limit)
			}

			//DebugLog.Message(InternalSession.ToString()); // debug
			//DebugLog.Message("MaxIdleTime: " + minutes);

			maxIdleTime = TimeSpan.FromMinutes(maxIdleMinutes); // max idle time before exiting process
			maxUnfreshenedTime = TimeSpan.FromMinutes(maxUnfreshenedMinutes); // max unfreshened time before exiting process

			checkInterval = TimeSpan.FromMinutes(1); // time between checks

			DateTime now = DateTime.Now;
			DateTime lastCheck = now;
			LastFreshen = now;
			LastInvocationFinish = now;
			DateTime lastInvocationTimeSentToServiceHost = now;
			DateTime lastInvocationActiveTime = now;

			while (true)
			{
				Thread.Sleep(checkInterval);

				if (InvocationDepth > 0) lastInvocationActiveTime = DateTime.Now;
				else lastInvocationActiveTime = LastInvocationFinish;

				now = DateTime.Now;
				idleTime = now.Subtract(lastInvocationActiveTime); // how long since last method invocation
				unfreshenedTime = now.Subtract(LastFreshen); // how long since last freshen

				// Check of idle for too long

				if (maxIdleMinutes != 0 && idleTime.CompareTo(maxIdleTime) >= 0) // exit if idle limit defined and idling too long
				{ // and also not freshened within freshen limit
					if (maxUnfreshenedMinutes == 0 || unfreshenedTime.CompareTo(maxUnfreshenedTime) >= 0)
					{
						string msg =
							"Idle time limit (" + maxIdleTime + ") exceeded, exiting session process " + Process.GetCurrentProcess().Id +
							" for user: " + Mobius.UAL.Security.UserName;
						ServicesLog.Message(msg);
						break;
					}
				}

				// Check if unrefreshed for too long

				if (maxUnfreshenedMinutes != 0 && // also exit if unfreshened limit defined and no activity within that time
				 unfreshenedTime.CompareTo(maxUnfreshenedTime) >= 0)
				{
					if (maxIdleMinutes == 0 || idleTime.CompareTo(maxUnfreshenedTime) >= 0) // and also idle more than unfreshened limit
					{
						string msg =
							"Unrefreshed idle time limit (" + maxUnfreshenedTime + ") exceeded, exiting session process " + Process.GetCurrentProcess().Id +
							" for user: " + Mobius.UAL.Security.UserName;
						msg += ", (disabled, MaxNativeUnfreshenedTime = " + ServicesIniFile.IniFile.ReadInt("MaxNativeUnfreshenedTime", 10) + ")";
						ServicesLog.Message(msg);
					}
				}

				if (!lastInvocationActiveTime.Equals(lastInvocationTimeSentToServiceHost))
				{
					UpdateServiceHostWithLastInvocationTime(lastInvocationActiveTime);
					lastInvocationTimeSentToServiceHost = lastInvocationActiveTime;
				}

				lastCheck = now;
			}

			// Dispose the session

			//ServicesLog.Message("IdleTime: " + idleTime); // debug
			//ServicesLog.Message("MaxIdleTime: " + maxIdleTime);
			//ServicesLog.Message("CheckInterval: " + checkInterval);
			//ServicesLog.Message("LastCheck: " + lastCheck);
			//ServicesLog.Message("LastInvocation: " + LastInvocation);

			DisposeSession();
			System.Environment.Exit(0); // be sure exiting
		}

		/// <summary>
		/// Update the ServiceHost with the last invocation time
		/// </summary>
		/// <param name="idleTime"></param>

		static void UpdateServiceHostWithLastInvocationTime(DateTime lastInvocationActiveTime)
		{
			try
			{
				if (InternalSession == null) return;

				if (LogServiceCalls)
					DebugLog.Message("UpdateServiceHostWithLastInvocationTime, Session: " + InternalSession.Id + ", Process: " + Process.GetCurrentProcess().Id);

				InternalSession.LastFreshenDT = lastInvocationActiveTime; // send last invocation time

				SetSessionManagerProcessId(ServiceHostProcessId);

				INativeSessionManager sessionManager = NativeSessionManagerClientFactory.CreateChannel();
				InternalSession session = sessionManager.FreshenNativeSession(InternalSession);
				((IClientChannel)sessionManager).Close();
			}

			catch (Exception ex)
			{
				string msgToIgnore = "There was no endpoint listening at net.pipe:";
				if (Lex.Contains(ex.Message, msgToIgnore)) return;

				DebugLog.Message("Session: " + InternalSession.Id + ", Process: " + Process.GetCurrentProcess().Id + ", " + DebugLog.FormatExceptionMessage(ex));
			}

			return;
		}

		/// <summary>
		/// Catch an log any unhandled exception
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = (Exception)e.ExceptionObject;
			string msg = "Unhandled exception: " + DebugLog.FormatExceptionMessage(ex);
			ServicesLog.Message(msg);
			if (NativeDebugMode)
			{
				Console.WriteLine("Press <ENTER>");
				Console.ReadLine();
			}
		}

		/// <summary>
		/// Initialize the Mobius internals and command routing table
		/// </summary>

		public static void Initialize()
		{

			string servicesIniFileName = NativeSessionInitializer.GetMobiusServicesIniFileLocation();
			ServicesIniFile.IniFilePath = servicesIniFileName;
			Mobius.UAL.UalUtil.Initialize(servicesIniFileName);

			NativeDebugMode = ServicesIniFile.IniFile.ReadBool("NativeDebugMode", NativeDebugMode);
			LogServiceCalls = ServicesIniFile.IniFile.ReadBool("LogServiceCalls", LogServiceCalls);
			ServicesLog.LogToConsole = ServicesIniFile.IniFile.ReadBool("LogToConsole", ServicesLog.LogToConsole);

			NativeSessionInitializer.Instance.InitializeUAL(); // Initialize the UAL to where we can do basic Oracle operations

			Mobius.QueryEngineLibrary.QueryEngine.InitializeForSession(); // Need to initialize the QE on the UI thread

			// Create and populate the command routing table

			Mobius.ServiceFacade.ServiceFacade.UseRemoteServices = false; // we are the Mobius services, don't want to try to call out for them

			string[] serviceNames = Enum.GetNames(typeof(ServiceCodes));
			OpInvokers = new Dictionary<int, IInvokeServiceOps>(serviceNames.Length);
			Assembly opInvokerAssembly = Assembly.GetAssembly(typeof(IInvokeServiceOps));
			foreach (string serviceName in serviceNames)
			{
				//if (Lex.Eq(serviceName, "MobiusDynamicWebServceDao")) serviceNames = serviceNames; // debug (note typo)

				if (Lex.Contains(serviceName, "Obsolete")) continue;
				try
				{
					string typeName = "Mobius.Services.Native.OpInvokers." + serviceName + "OpInvoker";
					Type iInvokeServiceOpsType = opInvokerAssembly.GetType(typeName);
					if (iInvokeServiceOpsType == null) throw new Exception("Service OpInvoker type not found: " + typeName);
					ConstructorInfo iInvokeServiceOpsConstructor = iInvokeServiceOpsType.GetConstructor(Type.EmptyTypes);
					IInvokeServiceOps serviceOpInvoker = iInvokeServiceOpsConstructor.Invoke(null) as IInvokeServiceOps;
					OpInvokers.Add(
							(int)Enum.Parse(typeof(ServiceCodes), serviceName),
							serviceOpInvoker);
				}
				catch (Exception ex)
				{
					DebugLog.Message("Error initializing service: " + serviceName + "\r\n" +
						DebugLog.FormatExceptionMessage(ex)); // debug
				}
			}

			return;
		}

		/// <summary>
		/// Open the service endpoint(s) and wait for connection from client
		/// For this method to operate properly the following must be setup:
		/// 1. .Net port sharing must be set up by issueing the following 
		///    command under an admin account: sc.exe config NetTcpPortSharing start= demand
		/// 2. Must run the app in admin mode or the current user must be added
		///    to the allowedAccounts section in the SMSvcHost.exe.config file.
		/// </summary>

		private static void OpenServiceHost()
		{
			int processId = Process.GetCurrentProcess().Id;

			NativeSessionHost nativeSessionHost = // Create the native session host obj for the IPC and NetTcp service hosts to use
				new NativeSessionHost();

			ServiceHost serviceHost = // Create the host object
				new ServiceHost(nativeSessionHost);

			// Add the native session's IPC endpoint (for use by the NativeSessionManager)

			string endpointAddressName = Nomenclator.GetIPCEndpointAddress(typeof(Types.Internal.INativeSession), processId);
			NetNamedPipeBinding ipcBinding = new NetNamedPipeBinding("ipcBinding");
			ServiceEndpoint ipcEndpoint = serviceHost.AddServiceEndpoint(
							typeof(Types.Internal.INativeSession),
							ipcBinding,
							new Uri(endpointAddressName));

			// Add the NetTcp endpoint (for use by the native client)

			NativeSessionEndpointAddress.ServiceHostName = ServiceHostUtil.HostName;
			NativeSessionEndpointAddress.ServicePort = ServiceHostUtil.BasePort + 5; // //port offset for native services = +2

			endpointAddressName = NativeSessionEndpointAddress.Build(processId);
			NetTcpBinding tcpBinding = new NetTcpBinding("tcpBinding");

			ServiceEndpoint tcpEndpoint = serviceHost.AddServiceEndpoint(
		typeof(Native.INativeSession),
		tcpBinding,
		new Uri(endpointAddressName));

			// Up the value of maxItemsInObjectGraph so that the MetaTree can pass over the wire...

			foreach (OperationDescription op in tcpEndpoint.Contract.Operations)
			{
				if (op.Name != "InvokeNativeMethod") continue;

				DataContractSerializerOperationBehavior dataContractBehavior =
						op.Behaviors.Find<DataContractSerializerOperationBehavior>()
								as DataContractSerializerOperationBehavior;
				if (dataContractBehavior != null)
					dataContractBehavior.MaxItemsInObjectGraph = 2147483647;

			}


			// We really don't need to do this anymore. Why rewrite the address?  Just keep the address from the config file. 
			//ServiceHostUtil.ApplyEndpointAddressAndIdentityConfig(serviceHost);

			serviceHost.Open(); // Open the host

		}

		/// <summary>
		/// Make multiple attempts to register a session
		/// </summary>
		/// <param name="creatorProcessId"></param>
		/// <param name="sessionId"></param>

		private static void RegisterSession(int creatorProcessId, int sessionId)
		{
			for (int attempt = 1; attempt <= 3; attempt++)
			{
				try
				{
					RegisterSession2(creatorProcessId, sessionId); // register session with the service host that started us
					break;
				}

				catch (Exception ex)
				{
					if (attempt < 3) // allow multiple attempts
						Thread.Sleep(1000);

					else // no luck, log error and give up
					{
						DebugLog.Message("RegisterSession error, Attempt: " + attempt + ", CreatorProcessId: " + creatorProcessId +
							", Process: " + Process.GetCurrentProcess().Id + ", Session: " + sessionId + ", " + DebugLog.FormatExceptionMessage(ex));
						throw new Exception(ex.Message, ex);
					}
				}
			}
		}

		/// <summary>
		/// Get a handle to the session manager and request the session object by id
		/// this is also the signal to the session manager that this NativeSessionHost is ready
		/// </summary>
		/// <param name="creatorProcessId"></param>
		/// <param name="sessionId"></param>

		private static void RegisterSession2(int creatorProcessId, int sessionId)
		{
			SetSessionManagerProcessId(creatorProcessId);

			INativeSessionManager sessionManager = NativeSessionManagerClientFactory.CreateChannel();
			InternalSession session = sessionManager.RegisterNativeSessionHost(sessionId);
			((IClientChannel)sessionManager).Close();
			InternalSession = session;
		}

		/// <summary>
		/// Create the channel factory for calling the session manager in the host
		/// </summary>
		/// <param name="sessionManagerProcessId"></param>

		private static void SetSessionManagerProcessId(int sessionManagerProcessId)
		{
			string sessionManagerEndpointAddress =
					Nomenclator.GetIPCEndpointAddress(typeof(INativeSessionManager), sessionManagerProcessId);
			EndpointAddress endpointAddress = new EndpointAddress(sessionManagerEndpointAddress);
			NativeSessionManagerClientFactory =
					new ChannelFactory<INativeSessionManager>("NativeSessionManager_IPCEndpoint", endpointAddress);
		}

		/// <summary>
		/// LogMethodInvocation
		/// </summary>
		/// <param name="serviceCode"></param>
		/// <param name="opCode"></param>
		/// <param name="callStart"></param>
		/// <param name="callCompleted"></param>

		private void LogMethodInvocation(int serviceCode, int opCode, DateTime callStart, bool callCompleted)
		{
			if (!LogServiceCalls) return;

			string serviceName = ((Native.ServiceCodes)serviceCode).ToString();
			string msg;

			Type t = // is there a better way to find the correct assembly?
				Type.GetType("Mobius.Services.Native.ServiceOpCodes." + serviceName);

			string opName = Enum.GetName(t, opCode);

			if (!callCompleted)
				msg =
					"[ServiceCallBegin] - " + serviceName + "." + opName;

			else msg =
				"[ServiceCallEnd] - " + serviceName + "." + opName + ", Time: " + (int)(DateTime.Now.Subtract(callStart).TotalMilliseconds) + " ms";

			ServicesLog.Message(msg);
			return;
		}

		#region INativeSession (Public) Members

		/// <summary>
		/// GetCurrentVersionNumber
		/// </summary>
		/// <returns></returns>

		string INativeSession.GetCurrentVersionNumber()
		{
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			LastFreshen = DateTime.Now;

			return version;
		}

		/// <summary>
		/// Get userid for current session
		/// </summary>
		/// <returns></returns>

		string INativeSession.GetCurrentUserId()
		{
			string userId = null;
			if (InternalSession != null)
				userId = InternalSession.UserId;

			LastFreshen = DateTime.Now;

			return userId;
		}

		/// <summary>
		/// GetCurrentSessionUserId
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>

		string INativeSession.GetCurrentSessionUserId(Mobius.Services.Types.Session session)
		{
			string userId = null;
			if (InternalSession != null && session != null && InternalSession.Id == session.Id)
			{
				userId = InternalSession.UserId;
				lock (SessionObjectLock)
				{
					if (InternalSession.SessionParameters.ContainsKey(Types.SessionParameterName.UserNameToImpersonate) &&
							InternalSession.SessionParameters[Types.SessionParameterName.UserNameToImpersonate] != null)
					{
						userId = InternalSession.SessionParameters[Types.SessionParameterName.UserNameToImpersonate];
					}
				}
			}

			LastFreshen = DateTime.Now;

			return userId;
		}

		/// <summary>
		/// Called by the client to keep the session fresh
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>

		Types.Session INativeSession.FreshenSession(Types.Session session)
		{
			LastFreshen = DateTime.Now;
			if (LogServiceCalls)
				DebugLog.Message("Freshened session: " + session.Id + ", process: " + Process.GetCurrentProcess().Id);

			return session;
		}

		/// <summary>
		/// Dispose of the session and schedule the process for termination
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>

		bool INativeSession.DisposeSession(Types.Session session)
		{
			DisposeSession();
			return true;
		}

		/// <summary>
		/// Dispose of session by scheduling exit operation
		/// </summary>
		/// <returns></returns>

		private static bool DisposeSession()
		{
			ThreadStart threadStart = new ThreadStart(DelayedExit);
			Thread monitorThread = new Thread(threadStart);
			monitorThread.IsBackground = true;
			monitorThread.Name = "IdleMonitor";
			monitorThread.Start();

			if (InternalSession != null)
				DebugLog.Message("Disposing of session " + InternalSession.Id + " in process " + InternalSession.ProcessId + " for user " + InternalSession.UserId);
			else DebugLog.Message("Disposing of undefined session");


			return true;
		}

		/// <summary>
		/// Thread that waits a bit and then exits the process
		/// </summary>

		private static void DelayedExit()
		{
			int delayMs = 1000;
			Thread.Sleep(delayMs);
			System.Environment.Exit(0);
		}

#if false // legacy notice to service host
			{
				//if not running in native debug mode, the native session needs to try to tell the session manager that it is done
				// so that the native session manager can trigger its shutdown
				try
				{
					INativeSessionManager sessionManager = NativeSessionManagerClientFactory.CreateChannel();
					sessionManager.DisposeNativeSession(InternalSession);
					((IClientChannel)sessionManager).Close();
					disposed = true;
				}
				catch (Exception)
				{
					//call to the native session manager failed...
					//assume that the native session outlived the native session manager
					// and shutdown the native session
					bool createdLocally;
					string shutdownRequestSemaphoreName =
							Nomenclator.GetNativeSessionHostShutdownRequestSemaphoreName(InternalSession.Id, Process.GetCurrentProcess().Id);
					Semaphore shutdownRequestSemaphore = new Semaphore(0, 1, shutdownRequestSemaphoreName, out createdLocally);
					shutdownRequestSemaphore.Release();
				}
			}
			return disposed;
		}
#endif

		/// <summary>
		/// Main service method - Calls a native method and returns a response object
		/// </summary>
		/// <param name="serviceCode">Code for service from: Services.Native.ServiceCodes</param>
		/// <param name="opCode">Specific service operation code from: Services.Native.ServiceOpCodes</param>
		/// <param name="argsObject">Object array containing arguments</param>
		/// <returns>NativeMethodTransportObject containing results</returns>

		NativeMethodTransportObject INativeSession.InvokeNativeMethod(
			int serviceCode,
			int opCode,
			NativeMethodTransportObject argsObject)
		{
			lock (SessionObjectLock)
			{
				InvocationDepth++;

				if (InvocationDepth > 1) InvocationDepth = InvocationDepth; // debug

				if (InvocationDepth > MaxInvocationDepth) MaxInvocationDepth = InvocationDepth;
			}

			DateTime callStart = LastInvocationStart = DateTime.Now;
			NativeMethodTransportObject returnObject = null;
			try
			{
				if (LogServiceCalls)
					LogMethodInvocation(serviceCode, opCode, callStart, false);

				object[] args = (argsObject == null) ? null : (object[])argsObject.Value;
				IInvokeServiceOps serviceOpInvoker = OpInvokers[serviceCode];
				object valueObject = serviceOpInvoker.InvokeServiceOperation(opCode, args);
				returnObject = new NativeMethodTransportObject(valueObject);

				if (LogServiceCalls)
				{
					LogMethodInvocation(serviceCode, opCode, callStart, true);
				}
			}
			catch (Exception ex)
			{ // Add text to return message to indicate that an exception has occured in the services

				string msg =
					"*** InvokeNativeMethod Exception ***\r\n" +
					ex.GetType().Name + "\r\n";

				if (ex is Mobius.Data.UserQueryException) // just return message if user exception
					msg += ex.Message;

				else // format message with stack trace & log
				{
					msg += DebugLog.FormatExceptionMessage(ex);
					ServicesLog.Message(msg);
				}

				returnObject = new NativeMethodTransportObject(msg);

				//throw new Exception(msg, ex);
				//throw new FaultException<Exception>(ex, msg);
			}

			finally
			{
				LastInvocationFinish = DateTime.Now;
				lock (SessionObjectLock) { InvocationDepth--; }
			}

			return returnObject;
		}

		#endregion


		#region INativeSession (Internal) Members

		bool Mobius.Services.Types.Internal.INativeSession.SetSessionManagerProcessId(int sessionManagerProcessId)
		{
			SetSessionManagerProcessId(sessionManagerProcessId);
			return true;
		}

		InternalSession Mobius.Services.Types.Internal.INativeSession.GetInternalSession()
		{
			return InternalSession;
		}

		string Mobius.Services.Types.Internal.INativeSession.GetCurrentUserId()
		{
			if (InternalSession != null && InternalSession.UserId != null) return InternalSession.UserId;
			else return "";
		}

		#endregion
	}
}
