using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Mobius.ServiceFacade
{

/// <summary>
/// Write to server log file
/// </summary>

	public class ServicesLogFile
	{
		string LogFileName_ = ""; // basic log file name without directory

/// <summary>
/// Constructor
/// </summary>

		public ServicesLogFile (
			string logFileName)
		{
			LogFileName_ = logFileName;
		}

/// <summary>
/// Getter/setter of log file
/// </summary>
/// <param name="logFileName"></param>

		public string LogFileName
		{			
			set { LogFileName_ = value; }
			get { return LogFileName_; }
		}

/// <summary>
/// Delete the log file
/// </summary>

		public void ResetFile()
		{
            if (ServiceFacade.UseRemoteServices)
            {
                    Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
                    Services.Native.NativeMethodTransportObject resultObject =
                        ServiceFacade.InvokeNativeMethod(nativeClient,
                            (int)Services.Native.ServiceCodes.MobiusServerLogFileService,
                            (int)Services.Native.ServiceOpCodes.MobiusServerLogFileService.ResetFile,
                            new Services.Native.NativeMethodTransportObject(new object[] { LogFileName_ }));
                    ((System.ServiceModel.IClientChannel)nativeClient).Close();
            }
            else
            {
                try
                {
									string logFileName = ServicesDirs.LogDir + @"\" + LogFileName_;
									File.Delete(logFileName);
                }
                catch (Exception ex) { }
            }
		}

/// <summary>
/// Log a Message 
/// </summary>
/// <param name="msg"></param>

		public void Message (
			string msg)
		{
			Message(msg, LogFileName_);

//			string logFileName = UAL.ServicesDirs.LogDir + @"\" + LogFileName_;
//			Message(msg, logFileName);
		}

/// <summary>
/// Write entry to specified file
/// </summary>
/// <param name="msg"></param>
/// <param name="logFileName"></param>

		public static void Message(
			string msg,
			string logFileName)
		{
			if (ServiceFacade.UseRemoteServices)
			{
				if (!msg.StartsWith("[")) // include local method info if needed
				{
					string prefix = DebugLog.GetCallingClassAndMethodPrefix();
					msg = prefix + msg;
				}

					Mobius.Services.Native.INativeSession nativeClient = ServiceFacade.CreateNativeSessionProxy();
					Services.Native.NativeMethodTransportObject resultObject =
							ServiceFacade.InvokeNativeMethod(nativeClient,
									(int)Services.Native.ServiceCodes.MobiusServerLogFileService,
									(int)Services.Native.ServiceOpCodes.MobiusServerLogFileService.LogMessage,
									new Services.Native.NativeMethodTransportObject(new object[] { msg, logFileName }));
					((System.ServiceModel.IClientChannel)nativeClient).Close();
			}

			else // just log locally
			{
				if (Lex.IsNullOrEmpty(logFileName)) // assign default name
					logFileName = CommonConfigInfo.ServicesLogFileName;

				logFileName = Path.GetFileName(logFileName);
				//if (!logFileName.Contains(":") && !logFileName.StartsWith(@"\\")) // include directory if none defined
				logFileName = ServicesDirs.LogDir + @"\" + logFileName;

				DebugLog.MessageDirect(msg, logFileName);
			}
		}

/// <summary>
/// Log a delta time message 
/// </summary>
/// <param name="msg"></param>
/// <param name="t0"></param>
/// <returns></returns>

		public int TimeMessage (
			string msg,
			int t0)
		{
			return TimeMessage(msg,t0,this.LogFileName_);
		}

/// <summary>
/// Log a delta time message to specified file
/// </summary>
/// <param name="msg"></param>
/// <param name="t0"></param>
/// <param name="logFileName"></param>
/// <returns></returns>

		public static int TimeMessage (
			string msg,
			int t0,
			string logFileName)
		{
			int t1 = TimeOfDay.Milliseconds();
			Message(msg + " " + (t1-t0).ToString(),logFileName);
			return t1; // return current time
		}


	} // end of Log class
}
