using Mobius.ComOps;
using Mobius.Data;
using Mobius.MetaFactoryNamespace;
using Mobius.QueryEngineLibrary;
using Mobius.UAL;
using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Native;
using Mobius.Services.Types;
using Mobius.NativeSessionClient;
using NCNS = Mobius.NativeSessionClient;

using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Mobius.ServiceFacade
{

	/// <summary>
	/// ServiceFacade - Make internal or external service calls
	/// </summary>

	public class ServiceFacade
	{
		public static bool UseRemoteServices = true; // if true use remote rather than local services
		public static bool DebugNativeSessionHost = false; // if true debug simple stand-alone native session

		static NCNS.NativeSessionClient NativeSessionClient = null; // Native session client to call

		public static bool SessionCreated
		{
			get
			{
				if (NativeSessionClient != null)
					return NativeSessionClient.SessionCreated;

				else return _sessionCreatedStandAlone;
			}
		}
		static bool _sessionCreatedStandAlone = false;

		public static bool InCreateSession
		{
			get
			{
				if (NativeSessionClient != null)
					return NativeSessionClient.InCreateSession;

				else return _inCreateSessionStandAlone;
			}
		}
		static bool _inCreateSessionStandAlone = false;

		public static Exception CreateSessionException
		{
			get
			{
				if (NativeSessionClient != null)
					return NativeSessionClient.CreateSessionException;

				else return null;
			}
		}

		public static bool LogStartupDetails // true if startup details should be logged
		{
			get { return Mobius.NativeSessionClient.NativeSessionClient.LogStartupDetails; }
			set { Mobius.NativeSessionClient.NativeSessionClient.LogStartupDetails = value; }
		}

		//public static bool InCreateSessionAsynch { get { return ServiceSession.InCreateSessionAsynch; } }

		public static TypeConversionHelper TypeConversionHelper
		{
			get { return NCNS.NativeSessionClient.TypeConversionHelper; }
		}

		/// <summary>
		/// Create session 
		/// </summary>
		/// <param name="iniFile"></param>
		/// <param name="asynch"></param>

		public static void CreateSession(
			IniFile iniFile,
			string argString,
			bool asynch)
		{
				UseRemoteServices = iniFile.ReadBool("UseMobiusServices", true);
				DebugNativeSessionHost = iniFile.ReadBool("DebugNativeSessionHost", false);

				if (UseRemoteServices)
				{
					NativeSessionClient = new NCNS.NativeSessionClient();
					NativeSessionClient.CreateSession(iniFile, asynch);
					return;
				}

				else // if not using services just do Initialize
				{
					try
					{
						_inCreateSessionStandAlone = true;

						string servicesIniFileName = iniFile.Read("ServicesIniFile"); // services inifile name
						if (Lex.IsNullOrEmpty(servicesIniFileName)) throw new Exception("MobiusServices IniFile not defined");
						UalUtil.Initialize(servicesIniFileName);

						string msg = "Standalone Session started on " + Environment.MachineName + " in process " + Process.GetCurrentProcess().Id + " at " + DateTime.Now + ", ParmString: " + argString;
						DebugLog.Message(msg); // log initial message 
						_sessionCreatedStandAlone = true;
					}
					finally { _inCreateSessionStandAlone = false; }

					return;
				}

		}

/// <summary>
/// Start a "heartbeat" to keep native session process alive even if no client activity
/// </summary>

		public static void StartSessionHeartbeat()
		{
			if (NativeSessionClient != null)
				NativeSessionClient.StartSessionHeartbeat();

			return;
		}

/// <summary>
/// Dispose of the session on the services side
/// </summary>

		public static void DisposeSession()
		{
			if (NativeSessionClient != null)
			{
				NativeSessionClient.DisposeSession();
				NativeSessionClient = null;
			}

			return;
		}

		/// <summary>
		/// Call a service method and return result
		/// </summary>
		/// <param name="serviceCode"></param>
		/// <param name="subServiceCode"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>

		public static NativeMethodTransportObject CallServiceMethod(
				ServiceCodes serviceCode,
				object subServiceCode,
				object parameters)
		{
			return NativeSessionClient.CallServiceMethod(serviceCode, subServiceCode, parameters);
		}

		/// <summary>
		/// CreateNativeSessionProxy
		/// </summary>
		/// <returns></returns>

		public static INativeSession CreateNativeSessionProxy()
		{
			return NativeSessionClient.CreateNativeSessionProxy();
		}

		/// <summary>
		/// Call native method & process any exception
		/// </summary>
		/// <param name="nativeClient"></param>
		/// <param name="serviceCode"></param>
		/// <param name="opCode"></param>
		/// <param name="argObject"></param>
		/// <returns></returns>

		public static NativeMethodTransportObject InvokeNativeMethod(
			INativeSession nativeClient,
			int serviceCodeInt,
			int opCodeInt,
			NativeMethodTransportObject argObject)
		{
			NativeMethodTransportObject resultObj =  NativeSessionClient.InvokeNativeMethod(nativeClient, serviceCodeInt, opCodeInt, argObject);
			return resultObj;
		}

/// <summary>
/// Return true if in service call
/// </summary>

		public static bool InServiceCall
		{
			get
			{
				if (NativeSessionClient != null)
					return NativeSessionClient.InServiceCall;
				else return false;
			}
		}

		/// <summary>
		/// Get service info for MobiusServiceHost
		/// </summary>
		/// <returns></returns>

		public static ServiceHostInfo GetServiceHostInfo()
		{
			return NativeSessionClient.GetServiceHostInfo();
		}

		/// <summary>
		/// Define interface for service code to call back to client
		/// </summary>

		public static IClient IClient
		{
			set { Mobius.UAL.UalUtil.IClient = value; }
		}

		/// <summary>
		/// Interface to allow service calls to call back into QueryExec
		/// </summary>

		public static IQueryExec IQueryExec
		{
			set { Mobius.UAL.UalUtil.IQueryExec = value; }
		}


		static ServiceHostInfo ServiceHostInfo = null;

		/// <summary>
		/// Get Client and Services versions and server name
		/// </summary>
		/// <param name="clientVersion"></param>
		/// <param name="servicesVersion"></param>
		/// <param name="server"></param>

		public static void GetClientAndServicesVersions(
			out string clientVersion,
			out string servicesVersion,
			out string server,
			bool includeSessionCount = false)
		{
			Version cv = Assembly.GetExecutingAssembly().GetName().Version; // version comes from this assembly (ServiceFacade)
			clientVersion = VersionMx.FormatVersion(cv);
			servicesVersion = server = "";

			if (ServiceFacade.UseRemoteServices)
			{
				if (ServiceHostInfo == null) // get from services if don't have already
					ServiceHostInfo = ServiceFacade.GetServiceHostInfo();

				if (ServiceHostInfo != null)
				{ 
					servicesVersion = VersionMx.FormatVersion(ServiceHostInfo.Version);
					server = ServiceHostInfo.ServerName;
					if (includeSessionCount)
					{
						int sessionCount = UsageDao.GetCurrentSessionCount();
						server += " (" + sessionCount + " sessions)";
					}
				}
			}

			else // not using services
				servicesVersion = "Integrated";

			return;
		}
	}
}
