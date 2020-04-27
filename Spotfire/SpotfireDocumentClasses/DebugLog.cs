using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Mobius.SpotfireComOps
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
			//msg = LogFile.FormatTimeMessage(msg, ref t);
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
			//Message(msg + MSTime.FormatDelta(ref t), logFileName);
			return t;
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
		}

		/// <summary>
		/// Log a message directly to the log file
		/// </summary>
		/// <param name="msg"></param>

		public static void MessageDirect(
			string msg, 
			string logFileName)
		{
			throw new NotImplementedException();

#if false
			LogFileName = logFileName;
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
#endif
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

				return "[" + className + "." + methodName + "] - ";
			}

			return "";
		}

		/// <summary>
		///	Add a delta time message based on a stopwatch to a string
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="sw"></param>
		/// <param name="logFileName"></param>

		public static string AddStopwatchMessage(
			string msg,
			Stopwatch sw,
			ref string debugMsg)
		{
			msg = FormatStopwatchMessage(msg, sw);
			debugMsg += msg + "\r\n"; 
			return msg;
		}

		/// <summary>
		/// Format a delta time message from a Stopwatch with optional caller and line nunber
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="sw"></param>
		/// <returns></returns>

		public static string FormatStopwatchMessage(
			string msg,
			Stopwatch sw)
		{
			TimeSpan ts = sw.Elapsed;
			double ms = ts.TotalMilliseconds;
			msg += string.Format(" {0:0.00} ms.", ms);

			sw.Restart(); // restart the stopwatch

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
