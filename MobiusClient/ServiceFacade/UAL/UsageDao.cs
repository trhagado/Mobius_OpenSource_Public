using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using ServiceTypes = Mobius.Services.Types;
using Mobius.Services.Native;
using Mobius.Services.Native.ServiceOpCodes;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ServiceFacade
{
	public class UsageDao
	{

		/// <summary>
		/// Log a usage event
		/// </summary>
		/// <param name="eventName"></param>
		public static void LogEvent(
			string eventName)
		{
			LogEvent(eventName, "", 0);
		}

		/// <summary>
		/// Log a usage event
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="eventData"></param>

		public static void LogEvent(
			string eventName,
			string eventData)
		{
			LogEvent(eventName, eventData, 0);
		}

		/// <summary>
		/// Log a usage event
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="eventData"></param>
		/// <param name="eventNumber"></param>

		public static void LogEvent(
			string eventName,
			string eventData,
			int eventNumber)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusUsageService,
					MobiusUsageService.LogEvent,
					new object[] { eventName, eventData, eventNumber });
				return;
			}

			else UAL.UsageDao.LogEvent(eventName, eventData, eventNumber);
		}

		/// <summary>
		/// Commandline command to analyse usage data
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		public static string AnalyzeUsageData(
			string commandArgs)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusUsageService,
					MobiusUsageService.AnalyzeUsageData,
					new object[] { commandArgs });

				if (resultObject == null) return null;
				string result = (string)resultObject.Value;
				return result;
			}
			else return UAL.UsageDao.AnalyzeUsageData(commandArgs);
		}

		/// <summary>
		/// GetCurrentSessionCount
		/// </summary>
		/// <returns></returns>

		public static int GetCurrentSessionCount()
		{
			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusUsageService,
					MobiusUsageService.GetCurrentSessionCount,
					null);

				if (resultObject != null && resultObject.Value is int)
					return (int)resultObject.Value;

				else return -1;
			}

			else return 1; // just us
		}

		/// <summary>
		/// GetServiceServerRebootTime
		/// </summary>
		/// <returns></returns>

		public static DateTime GetServiceServerRebootTime()
		{
			int tickCount = 0;

			if (ServiceFacade.UseRemoteServices)
			{
				NativeMethodTransportObject resultObject = ServiceFacade.CallServiceMethod(
					ServiceCodes.MobiusUsageService,
					MobiusUsageService.GetLastBootUpTime,
					null);

				if (resultObject != null && resultObject.Value is int)
					return (DateTime)resultObject.Value;

				else return DateTime.MinValue;
			}

			else return SystemUtil.GetLastBootUpTime();
		}




	}
}
