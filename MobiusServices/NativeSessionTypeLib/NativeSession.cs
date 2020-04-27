using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mobius.Services.Native
{

	/// <summary>
	/// NativeSession interface
	/// </summary>

	[ServiceContract(Namespace = "http://server/MobiusServices/Native/v1.0")]
	public interface INativeSession
	{
		//may or may not be used
		[OperationContract]
		string GetCurrentVersionNumber();

		[OperationContract]
		string GetCurrentUserId();

		[OperationContract]
		string GetCurrentSessionUserId(Types.Session session);

		[OperationContract]
		Types.Session FreshenSession(Types.Session session);

		[OperationContract]
		bool DisposeSession(Types.Session session);

		/// <summary>
		/// Main method to be called by the native Mobius client
		/// </summary>
		/// <param name="serviceCode"></param>
		/// <param name="opCode"></param>
		/// <param name="argObject"></param>
		/// <returns></returns>

		[OperationContract]
		[FaultContract(typeof(Exception))]
		NativeMethodTransportObject InvokeNativeMethod(int serviceCode, int opCode, NativeMethodTransportObject argObject);
	}

	/// <summary>
	/// Call native method & process any exception
	/// </summary>

	public class NativeMethodInvoker
	{
		public static string NativeServicesExecutableName = "MobiusNativeServices"; // root name of native services executable
		public static bool LogServiceCalls = false;
		public static bool AllowAsynchServiceCalls = true; // not used
		public static bool RunUIServiceCallsOnSeparateThread = true;
		public static IDebugLog IDebugLog = null;
		public bool InServiceCall = false; // in a service call
		public static DateTime FirstServiceCallTime = DateTime.MinValue;
		public static DateTime LastServiceCallTime = DateTime.MinValue;
		public static int ServiceCallCount = 0;

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
			NativeMethodTransportObject resultObj = null;
			string servicesExceptionHeader = "*** InvokeNativeMethod Exception ***\r\n"; // this exception header is added by the services upon an exception
			bool isException = false;

			int t0 = 0;
			string classAndMethod = "", msg = "", exceptionType = "";

			if (LogServiceCalls)
			{
				t0 = ComOps.TimeOfDay.Milliseconds();
				classAndMethod = DebugLog.GetCallingClassAndMethodPrefix();
				if (IDebugLog != null)
					IDebugLog.Message("[ServiceCallBegin]" + classAndMethod + "Begin");
			}

			DateTime callStart = DateTime.Now;

			if (ServiceCallCount == 0)
				FirstServiceCallTime = DateTime.Now;
			LastServiceCallTime = DateTime.Now;

			NativeMethodCaller nmc = new NativeMethodCaller();
			InServiceCall = true;
			nmc.InvokeNativeMethod(nativeClient, serviceCodeInt, opCodeInt, transportObject);
			resultObj = nmc.InvokeNativeMethodResult; // any result returned
			InServiceCall = false;

			ServiceCallCount++; // count it as complete

			// Regular exception thown on the client?

			if (nmc.InvokeNativeMethodException != null)
			{
				isException = true;

				msg = DebugLog.FormatExceptionMessage(nmc.InvokeNativeMethodException);

				// Check if link to services no longer exists (e.g. Server process killed or Mobius server restarted)

				if (Lex.Contains(msg, "There was no endpoint listening")  ||
					Lex.Contains(msg, "Count not connect") ||
					Lex.Contains(msg, "A connection attempt failed"))	// && ServiceCallCount > 1) 
				{
					msg =
@"The connection to Mobius services has been lost. 
Session start time: {0:G}, Service Calls: {1}, Last service call: {2:G})

Do you want to restart the Mobius client?";

					msg = String.Format(msg, FirstServiceCallTime, ServiceCallCount, LastServiceCallTime);

					DebugLog.Message(msg);
					DialogResult dr = MessageBox.Show(msg, "No connection to Mobius services",MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
					if (dr == DialogResult.Yes)
					{
						Process p = Process.Start(Application.ExecutablePath);
					}
					Environment.Exit(-1);
				}
			}

			// Check if exception from services by looking at header in string passed back from services

			else if
					(resultObj != null && resultObj.Value is string && (resultObj.Value as string).StartsWith(servicesExceptionHeader))
			{
				isException = true;

				msg = (resultObj.Value as string).Substring(servicesExceptionHeader.Length);
				int i1 = msg.IndexOf("\n");
				exceptionType = msg.Substring(0, i1 - 1);
				msg = msg.Substring(i1 + 1);
			}

			if (!isException)
			{
				if (LogServiceCalls)
				{
					t0 = ComOps.TimeOfDay.Milliseconds() - t0;

					if (IDebugLog != null) IDebugLog.Message("[ServiceCallEnd]" + classAndMethod +
						" Time: " + t0 + " ms");
				}
				return resultObj; // normal return value
			}

			else // the call threw an exception
			{
				bool userQueryException = Lex.Eq(exceptionType, "UserQueryException");
				bool queryException = Lex.Eq(exceptionType, "QueryException");

				if (IDebugLog != null && !userQueryException) IDebugLog.Message(msg); // log any exceptions except user query exception

				if (userQueryException)
					throw new UserQueryException(msg);

				else if (queryException)
					throw new QueryException(msg);

				else throw new Exception(msg);
			}
		}
	}

	/// <summary>
	/// Class to call native method on separate thread
	/// </summary>

	internal class NativeMethodCaller
	{
		internal NativeMethodTransportObject InvokeNativeMethodResult;
		internal Exception InvokeNativeMethodException;
		internal bool InvokeNativeMethodEntered = false;

		internal void InvokeNativeMethod(
			INativeSession nativeClient,
			int serviceCodeInt,
			int opCodeInt,
			NativeMethodTransportObject argObject)
		{
			//NativeMethodInvoker.ServiceCallSyncLock.WaitOne(); // get the mutex (beware of deadlocks)

			if (SynchronizationContext.Current is WindowsFormsSynchronizationContext && NativeMethodInvoker.RunUIServiceCallsOnSeparateThread)
			{ // do in separate thread if this is the UI thread
				ThreadStart ts = delegate () { InvokeNativeMethodThread(nativeClient, serviceCodeInt, opCodeInt, argObject); };
				Thread workerThread = new Thread(ts);
				workerThread.Name = "InvokeNativeMethod " + serviceCodeInt + ", " + opCodeInt;
				workerThread.IsBackground = true;
				workerThread.SetApartmentState(ApartmentState.STA);

				InvokeNativeMethodResult = null;
				InvokeNativeMethodException = null;
				InvokeNativeMethodEntered = false;
				workerThread.Start();
				while (!InvokeNativeMethodEntered) Thread.Sleep(1); // loop until worker thread activates and the method is entered
				while (workerThread.IsAlive)
				{
					if (InvokeNativeMethodResult != null || InvokeNativeMethodException != null) break;
					Thread.Sleep(2);
					Application.DoEvents();
				}

				//NativeMethodInvoker.ServiceCallSyncLock.ReleaseMutex(); // release after call (keeps other non-UI threads from making calls until we are complete)
			}

			else // non-UI thread, just call directly
			{
				//NativeMethodInvoker.ServiceCallSyncLock.ReleaseMutex(); // release before call
				InvokeNativeMethodThread(nativeClient, serviceCodeInt, opCodeInt, argObject);
			}

			return;
		}

		/// <summary>
		/// Invoke service method thread method
		/// </summary>
		/// <param name="nativeClient"></param>
		/// <param name="serviceCodeInt"></param>
		/// <param name="opCodeInt"></param>
		/// <param name="argObject"></param>

		void InvokeNativeMethodThread(
			INativeSession nativeClient,
			int serviceCodeInt,
			int opCodeInt,
			NativeMethodTransportObject argObject)
		{
			object resultObj = null;
			try
			{

				InvokeNativeMethodEntered = true;

				resultObj = nativeClient.InvokeNativeMethod(serviceCodeInt, opCodeInt, argObject);
				InvokeNativeMethodResult = resultObj as NativeMethodTransportObject;

				//if (xxxNativeMethodInvoker.ServiceCallCount == 10) throw new Exception("There was no endpoint listening"); // debug

				if (InvokeNativeMethodResult != null) return;
				else throw new Exception("Null result returned");
			}

			catch (Exception ex) // unexpected exception
			{
				InvokeNativeMethodResult = null;
				InvokeNativeMethodException = ex;
				return;
			}
		}

	}

	/// <summary>
	/// Class to build the endpoint address for the native session
	/// </summary>

	public class NativeSessionEndpointAddress
	{
		public static string ServiceHostName = "localhost"; // set before calling build
		public static int ServicePort = 9902;

		/// <summary>
		/// Build the endpoint address from the host name, port and process id
		/// </summary>
		/// <param name="processId"></param>
		/// <returns></returns>

		public static string Build(int processId)
		{
			string endpointAddress =
					"net.tcp://" + ServiceHostName + ":" + ServicePort + // server name & port
					"/MobiusServices/Native/SessionHost_" + processId; // service name
			return endpointAddress;
		}
	}
}
