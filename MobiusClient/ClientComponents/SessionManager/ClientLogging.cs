using Mobius.ComOps;
using Mobius.Data;
using Mobius.CdkMx;
//using Mobius.CdkMx;
using Mobius.ClientComponents;

using SF = Mobius.ServiceFacade;
using Mobius.ServiceFacade;

//using NSC = Mobius.NativeSessionClient;

using DevExpress.XtraEditors;
using Dx = DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.Utils;

using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
//using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.InteropServices;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Log a message to the services log file
	/// </summary>

	public class ServicesLog
	{
		static ServicesLogFile ServicesLogFile;

		public static void Message(
			string msg,
			string logFileName = null)
		{
			if (ServicesLogFile == null)
			{
				ServicesLogFile = new ServicesLogFile(CommonConfigInfo.ServicesLogFileName);
				//ServicesLogFile.ResetFile();
			}

			try
			{
				ServicesLogFile.Message(msg, logFileName);
			}

			catch (Exception ex) // log locally in case service message fails
			{
				string msg2 = DebugLog.FormatExceptionMessage(ex)
					+ "\r\nOriginal Message: " + msg;
				ClientLog.Message(msg2);
			}

			return;
		}
	}

	/// <summary>
	/// Log a message to the client log file and to the LogWindow if enabled.
	/// All messages from DebugLog and ServerLog get routed here.
	/// </summary>

	public class ClientLog
	{
		public static string LogFileName; // Client log file name
		public static string FatalErrorLogFileName;

		/// <summary>
		/// Initialize client log
		/// </summary>
		/// <param name="qualifier">Optional log file qualifier (e.g. Username)</param>

		public static void Initialize(
			string qualifier)
		{
			string logDir;

			//DebugLog.Message("ClientLog.Initialize for user: " + qualifier);

			LogFileName = "MobiusClient";

			if (Lex.IsDefined(qualifier)) // qualify log file name, usually by username 
			{
				LogFileName += "." + qualifier; // add qualifier

				logDir = Application.StartupPath + @"\Log";
				if (!Directory.Exists(logDir))
				{
					try { Directory.CreateDirectory(logDir); }
					catch (Exception ex) { logDir = Application.StartupPath; }
				}
			}

			else
			{
				string iniFilePath = MobiusClientUtil.GetMobiusBaseDirectoryFilePath("MobiusClient.ini");
				if (Lex.IsDefined(iniFilePath)) logDir = Path.GetDirectoryName(iniFilePath);
				else logDir = Application.StartupPath;
			}

			LogFileName = logDir + @"\" + LogFileName + ".log"; // full name

			//DebugLog.Message("ClientLog.LogFileName = " + LogFileName);

			DebugLog.LogFileName = LogFileName;
			DebugLog.ResetFile();
			DebugLog.IDebugLog = new DebugLogMediator(); // link DebugLog to us

			Mobius.NativeSessionClient.NativeSessionClient.IDebugLog = DebugLog.IDebugLog; // also for the client to the native sessions

			FatalErrorLogFileName = logDir + @"\MobiusClientFatalError.log";

			return;
		}

		/// <summary>
		/// Log a delta time message 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static int TimeMessage(
			string msg,
			int t0)
		{
			int t = t0;
			Message(msg + " " + MSTime.FormatDelta(ref t));
			return t;
		}

		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="msg"></param>

		public static void Message(string msg)
		{
			DebugLog.MessageDirect(msg, LogFileName);

			if (LogWindow.Display) // write to log window also if displaying
				LogWindow.Message(msg);
			return;
		}

		/// <summary>
		/// Log a fatal error for later pickup
		/// </summary>
		/// <param name="msg"></param>

		public static void LogFatalErrorMessage(
			string msg)
		{
			DebugLog.ResetFile(FatalErrorLogFileName);
			DebugLog.MessageDirect(msg, FatalErrorLogFileName);
		}

		/// <summary>
		/// Read any existing fatal error message
		/// </summary>
		/// <returns></returns>

		public static string GetFatalErrorMessage()
		{
			string msg = FileUtil.ReadFile(FatalErrorLogFileName);
			return msg;
		}

		/// <summary>
		/// DeleteFatalErrorMessage
		/// </summary>
		/// <returns></returns>

		public static void DeleteFatalErrorMessage()
		{
			DebugLog.ResetFile(FatalErrorLogFileName);
			return;
		}

	}

	/// <summary>
	/// Log mediator for DebugLog to properly route messages (common operations & non-service-mode services message)
	/// from the DebugLog class to ClientLog
	/// </summary>

	public class DebugLogMediator : IDebugLog
	{
		/// <summary>
		/// Log a Message
		/// </summary>
		/// <param name="msg"></param>

		public void Message(
			string msg,
			string logFileName)
		{
			ClientLog.Message(msg); // log to client

			if (!SF.ServiceFacade.UseRemoteServices) // if not using remote services then 
				ServicesLog.Message(msg, logFileName); // write the message to the local services log also

			return;
		}
	}

	/// <summary>
	/// Log a message to the script log
	/// </summary>

	public class ScriptLog
	{
		public static string FileName = ""; // log file to write to
																				/// <summary>
																				/// Log a message to the script log
																				/// </summary>
																				/// <param name="msg"></param>

		public static void Message(string msg)
		{
			if (!Lex.IsNullOrEmpty(FileName))
				LogFile.Message(msg, FileName);

			ClientLog.Message(msg);

			return;
		}
	}

}

namespace Mobius.Client // MobiusClientIntegrationPoint must be in the Mobius.Client namespace
{
	/// <summary>
	/// This class provides the method that is called over the IPC channel (e.g. from MobiusClientStart) 
	/// </summary>

	public class MobiusClientIntegrationPoint : MarshalByRefObject
	{

		public void Execute(string command)
		{
			try
			{
				command = command.Replace("%20", " "); // convert any special URL characters (todo: more complete translation)
				DebugLog.Message("MobiusClientIntegrationPoint Command: " + Lex.AddDoubleQuotes(command));
				CommandExec.PostCommand(command);
			}

			catch (Exception ex)
			{
				string msg =
					"Error executing external command: " + command + "\n\n" +
					DebugLog.FormatExceptionMessage(ex);

				ServicesLog.Message(msg);
				MessageBoxMx.ShowError(msg);
			}

		}

	}
}
