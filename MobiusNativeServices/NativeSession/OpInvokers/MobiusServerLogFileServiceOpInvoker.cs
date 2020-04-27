using Mobius.ComOps;
using Mobius.Data;

using Mobius.Services.Util;
using Mobius.Services.Native.ServiceOpCodes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusServerLogFileServiceOpInvoker : IInvokeServiceOps
	{
		#region IInvokeServiceOps Members

		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusServerLogFileService op = (MobiusServerLogFileService)opCode;
			switch (op)
			{
				case MobiusServerLogFileService.ResetFile:
					{
						Mobius.ComOps.DebugLog.ResetFile();
						return null;
					}

				case MobiusServerLogFileService.LogMessage:
					{
						string message = (string)args[0];
						string logFileName = (string)args[1]; // ignored

						DebugLog.LogFileName = ServicesDirs.LogDir + @"\" + CommonConfigInfo.ServicesLogFileName; // set file name
						DebugLog.Message(message);

						return null;
					}
			}
			return null;
		}

		#endregion
	}
}
