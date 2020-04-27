using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusUsageServiceOpInvoker : IInvokeServiceOps
	{
		private static object _analyzeLockObject = new object();

		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusUsageService op = (MobiusUsageService)opCode;
			switch (op)
			{
				case MobiusUsageService.LogEvent:
					{
						string eventName = (string)args[0];
						string eventData = (string)args[1];
						int eventNumber = (int)args[2];
						UAL.UsageDao.LogEvent(eventName, eventData, eventNumber);
						return true;
					}

				case MobiusUsageService.AnalyzeUsageData:
					{
						string commandArgs = (string)args[0];
						string result = null;
						lock (_analyzeLockObject)
						{
							result = UAL.UsageDao.AnalyzeUsageData(commandArgs);
						}
						return result;
					}

				case MobiusUsageService.GetCurrentSessionCount:
					{
						int result = 1;
						try
						{
							result = 0;
							System.Diagnostics.Process[] processes =
									System.Diagnostics.Process.GetProcesses();
							foreach (System.Diagnostics.Process process in processes)
							{
								if (Lex.StartsWith(process.ProcessName, NativeMethodInvoker.NativeServicesExecutableName))
									result++;
							}
						}
						catch (Exception) { }
						return result;
					}

				case MobiusUsageService.GetLastBootUpTime:
					{
						return SystemUtil.GetLastBootUpTime();
					}

			}
			return null;
		}

		#endregion
	}
}
