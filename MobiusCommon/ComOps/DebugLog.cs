using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Mobius.ComOps
{
	/// <summary>
	/// Log a debug message to a debug log file
	/// </summary>
	
	public class DebugLog
	{
		public static string LogFileName
		{
			set { _logFileName_ = value; }
			get { return _logFileName_; }
		}
		private static string _logFileName_ = "";

		public static string UserName = "";
		public static IDebugLog IDebugLog; // redirector for messages

		public static Dictionary<string, string> SecondaryErrorMessages = null; // second level debug error messages to be put in separater file

		/// <summary>
		/// Constructor
		/// </summary>

		public DebugLog ()
		{
		}

/// <summary>
/// Delete the log file
/// </summary>

		public static void ResetFile()
		{
			ResetFile(LogFileName);
			return;
		}

		/// <summary>
		/// Delete a log file
		/// </summary>

		public static void ResetFile(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch (Exception ex) { }
		}

		/// <summary>
		/// Log a delta time message based on a DateTime
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static DateTime TimeMessage(
			string msg,
			DateTime t0,
			string logFileName = null)
		{
			DateTime t = t0;
			msg = LogFile.FormatTimeMessage(msg, ref t);
			Message(msg, logFileName);
			return t;
		}

		/// <summary>
		/// Log a delta time message based on an millisecond int time of day
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static int TimeMessage (
			string msg,
			int t0,
			string logFileName = null)
		{
			int t = t0;
			Message(msg + MSTime.FormatDelta(ref t), logFileName);
			return t;
		}

		/// <summary>
		/// Log a delta time message based on a stopwatch
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="sw"></param>
		/// <param name="logFileName"></param>

		public static void StopwatchMessage(
			string msg,
			Stopwatch sw,
			string logFileName = null)
		{
			msg = LogFile.FormatStopwatchMessage(msg, sw);
			Message(msg, logFileName);
		}

		/// <summary>
		/// Log exception message with stack trace
		/// </summary>
		/// <param name="ex"></param>

		public static void Message(
			Exception ex,
			string logFileName = null)
		{
			string msg = FormatExceptionMessage(ex, true);
			Message(msg, logFileName);
		}

		/// <summary>
		/// Log a Message
		/// </summary>
		/// <param name="msg"></param>

		public static void Message(
			string msg,
			string logFileName = null)
		{
			if (logFileName == null) logFileName = LogFileName;

			if (IDebugLog != null)
			{
				IDebugLog.Message(msg, logFileName); // redirect to other object
				return;
			}

			else MessageDirect(msg, logFileName);
		}

		/// <summary>
		/// Log a message directly to the log file
		/// </summary>
		/// <param name="msg"></param>

		public static void MessageDirect(
			string msg, 
			string logFileName)
		{
			LogFileName = logFileName;

			logFileName = AssignSuffixToFileNameBasedOnMessageContent(msg, logFileName);

			if (!msg.StartsWith("[")) // include class & method name at start
			{
				string classAndMethod = DebugLog.GetCallingClassAndMethodPrefix();
				msg = classAndMethod + msg;
			}

			string userName = UserName;
			if (Lex.IsDefined(userName)) userName += ":";
			string pid = Process.GetCurrentProcess().Id.ToString();
			string tid = Thread.CurrentThread.ManagedThreadId.ToString();  // AppDomain.GetCurrentThreadId() not stable when managed threads are running on fibers (aka lightweight threads)
			msg = "[" + userName + pid + ":" + tid + "] - " + msg;

			LogFile.Message(msg, logFileName);
			return;
		}

		/// <summary>
		/// Allow messages to be sent to different files based on substring matching on message content
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="logFileName"></param>
		/// <returns></returns>

		static string AssignSuffixToFileNameBasedOnMessageContent(
			string msg,
			string logFileName)
		{
			string suffix = null;


			if (SecondaryErrorMessages != null) // write to secondary message file if msg is in exclusion list
			{
				foreach (string msg0 in SecondaryErrorMessages.Keys)
				{
					if (Lex.IsUndefined(msg0)) continue;

					if (Lex.Contains(msg, msg0))
					{
						suffix = SecondaryErrorMessages[msg0];
						if (Lex.IsUndefined(suffix)) suffix = "Secondary";
						break;
					}
				}
			}

			if (Lex.IsUndefined(suffix)) // check special cases before SecondaryErrorMessages may be available
			{
				if (Lex.Contains(msg, "ApplicationRecovery_OnApplicationCrash event occurred") ||
					Lex.Contains(msg, "There was no endpoint listening at"))
					suffix = "ClientCrash";

				else if (Lex.Contains(msg, "out of range for Vo of size"))
					suffix = "VoPosError";
			}

			if (Lex.IsDefined(suffix))
			{
				if (!suffix.StartsWith("."))
					suffix = "." + suffix;
				string ext = Path.GetExtension(logFileName);
				if (Lex.IsUndefined(ext)) logFileName += suffix;
				else logFileName = Lex.ReplaceFirst(logFileName, ext, suffix + ext);
			}

			return logFileName;
		}

/// <summary>
/// Format an exception message with stack trace & inner exceptions
/// </summary>
/// <param name="ex"></param>
/// <returns></returns>

		public static string FormatExceptionMessage(
			Exception ex,
			bool includeStackTrace = true)
		{
			return FormatExceptionMessage(ex, "", includeStackTrace);
		}

		/// <summary>
		/// Format an exception message with inner exceptions and optional first message and pre-exception stack trace
		/// <param name="ex"></param>
		/// <param name="firstMessage"></param>
		/// <param name="includeStackTrace"></param>
		/// <returns></returns>

		public static string FormatExceptionMessage(
			Exception ex,
			string firstMessage,
			bool includeStackTrace = true)
		{
			string msg = "";

			int depth = 0;

			if (Lex.IsDefined(firstMessage)) // optional exception context message included?
			{
				msg += "***** Exception Context *****\r\n";

				msg += firstMessage + "\r\n";

				if (includeStackTrace) msg += new StackTrace(true);

				msg += "\r\n";
			}

			while (ex != null) // loop through initial and all inner exceptions
			{
				if (depth == 0) msg += "***** Exception *****\r\n";
				else msg += "***** Inner exception " + depth + " *****\r\n";

				msg += ex.Message + "\r\n";

				if (Lex.Contains(msg, "(Logged: ")) break; // if logged then don't need to show more

				if (includeStackTrace) msg += ex.StackTrace;

				msg += "\r\n";

				ex = ex.InnerException;
				depth++;
			}

			return msg;
		}

/// <summary>
/// Get the name of the calling class
/// </summary>
/// <returns></returns>

		public static string GetCallingClassAndMethodPrefix()
		{
			StackTrace st = new StackTrace(true);
			for (int fi = 0; fi < st.FrameCount; fi++)
			{
				StackFrame f = st.GetFrame(fi);
				MethodBase mb = f.GetMethod();
				string className = mb.DeclaringType.Name;
				string methodName = mb.Name;
				int lineNumber = f.GetFileLineNumber();

				if (

// Classes to ignore
					
				 Lex.Eq(className, "DebugLog") ||
				 Lex.Eq(className, "DebugLogMediator") ||
				 Lex.Eq(className, "ClientLog") ||
				 Lex.Eq(className, "ScriptLog") ||
				 Lex.Eq(className, "ServicesLog") ||
				 Lex.Eq(className, "ServicesLogFile") ||
				 Lex.Eq(className, "ServiceFacade") ||
				 Lex.Eq(className, "NativeSessionClient") ||
				 Lex.Eq(className, "NativeMethodInvoker") ||

// Methods to ignore

				 Lex.StartsWith(methodName, "DebugLog") || // ignore any method that starts with DebugLog
				 Lex.Eq(methodName, "LogStartupMessage") ||
				 Lex.Eq(methodName, "LogStartupTimeMessage") ||
				 Lex.Eq(methodName, "InvokeNativeMethod") ||
					Lex.Eq(methodName, ".ctor"))
					continue;

				return "[" + className + "." + methodName + "." + lineNumber + "] - ";
			}

			return "";
		}

		/// <summary>
		/// Another method to get the context of the specified call (skipFrames) as File:Method.LineNumber
		/// </summary>
		/// <param name="skipFrames"></param>
		/// <param name="includeFileName"></param>
		/// <param name="includeMethod"></param>
		/// <param name="includeLineNumber"></param>
		/// <returns></returns>

		public static string GetCallContext(
			int skipFrames = 1,
			bool includeFileName = false,
			bool includeMethod = false,
			bool includeLineNumber = false)
		{
			string msg = "";

			if (includeFileName || includeMethod || includeLineNumber)
			{
				StackFrame callStack = new StackFrame(skipFrames, true);
				if (includeFileName) msg += callStack.GetFileName() + ": ";
				if (includeMethod) msg += callStack.GetMethod();
				if (includeLineNumber) msg += "." + callStack.GetFileLineNumber();
			}

			return msg;
		}


	} // end of DebugLog class


	/// <summary>
	/// Simple interface to a debug log
	/// </summary>

	public interface IDebugLog
	{

/// <summary>
/// Log a Message
/// </summary>
/// <param name="msg"></param>

		void Message(
			string msg,
			string logFileName = null);

	} 
}
