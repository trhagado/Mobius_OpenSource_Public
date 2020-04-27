using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Util
{
	/// <summary>
	/// Nomenclator - Build names for semaphores and endpoints
	/// </summary>

	public class Nomenclator
	{
		private Nomenclator()
		{
		}

		public static string GetSemaphoreName(int ownerProcessId, string creatorClassTypeName)
		{
			string semaphoreName =
					creatorClassTypeName + "_" + ownerProcessId;
			return semaphoreName;
		}

		public static string GetSessionSemaphoreName(int creatorProcessId, string creatorClassTypeName, int sessionId)
		{
			string sessionSemaphoreName =
					creatorClassTypeName + "_" + creatorProcessId + "_" + sessionId;
			return sessionSemaphoreName;
		}

		public static string GetIPCEndpointAddress(Type contractInterfaceType, int hostProcessId)
		{
			string endpointAddress =
					"net.pipe://" +
					System.Environment.MachineName +
					"/MobiusServices/" +
					contractInterfaceType.Name + "_" + hostProcessId;
			return endpointAddress;
		}

		public static string GetNativeSessionHostShutdownRequestSemaphoreName(int sessionId, int hostProcessId)
		{
			string semaphoreName =
					"MobiusNativeSession_" + sessionId + "_in_" + hostProcessId + "_ShutdownRequest";
			return semaphoreName;
		}

		public static string GetNativeSessionHostShutdownRequestReceivedSemaphoreName(int sessionId, int hostProcessId)
		{
			string semaphoreName =
					"MobiusNativeSession_" + sessionId + "_in_" + hostProcessId + "_ShutdownRequestReceived";
			return semaphoreName;
		}
	}
}
