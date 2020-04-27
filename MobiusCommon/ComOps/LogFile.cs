using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Log a debug message to a log file
	/// </summary>
	public class LogFile
	{
		static LogFile LogFileLock = new LogFile("Lock");

		/// <summary>
		/// Getter/setter of log file
		/// </summary>
		/// <param name="logFileName"></param>

		public string LogFileName
		{
			set { _logFileName_ = value; }
			get { return _logFileName_; }
		}
		private string _logFileName_ = "";

		/// <summary>
/// Constructor
/// </summary>
		public LogFile (
			string logFileName)
		{
			LogFileName = logFileName;
		}

/// <summary>
/// Delete the log file
/// </summary>

		public void ResetFile()
		{
			try 
			{
				File.Delete(LogFileName);
			}
			catch (Exception ex) {}
		}

/// <summary>
/// Log a Message 
/// </summary>
/// <param name="msg"></param>

		public void Message (
			string msg)
		{
			Message(msg,LogFileName);
		}

/// <summary>
/// Write entry to specified file
/// </summary>
/// <param name="msg"></param>
/// <param name="logFileName"></param>

		public static void Message (
			string msg,
			string logFileName)
		{
			if (Lex.IsUndefined(logFileName)) // assign default name
				logFileName = Application.ExecutablePath + ".log";

			try 
			{
				logFileName = GetDatedLogFileName(logFileName);

				lock (LogFileLock) // avoid simultaneous write by current process
				{
					DateTime dt = DateTime.Now;
					StreamWriter sw = new StreamWriter(logFileName, true);
					msg = "=====>>> " + // make messages easier to pick out within large chunks of text
						dt.ToShortDateString() + " " + dt.ToLongTimeString() + " " + msg;
					sw.WriteLine(msg);
					sw.Close();
				}
			}
			catch (Exception ex) { } // ignore errors
		}

/// <summary>
/// GetDatedLogFileName
/// </summary>
/// <param name="logFileName"></param>
/// <returns></returns>

		public static string GetDatedLogFileName(string logFileName)
		{
			DateTime dt = DateTime.Now;

			if (Lex.Contains(logFileName, "[Date]")) // substitute current date in YYYY-MM-DD format into file name
			{
				String tok = String.Format("{0,4:0000}-{1,2:00}-{2,2:00}", dt.Year, dt.Month, dt.Day);
				logFileName = Lex.Replace(logFileName, "[Date]", tok);
			}

			return logFileName;
		}

/// <summary>
/// Log a delta time message 
/// </summary>
/// <param name="msg"></param>
/// <param name="t0"></param>
/// <returns></returns>

		public DateTime TimeMessage (
			string msg,
			DateTime t0)
		{
			return TimeMessage(msg, t0, LogFileName);
		}

/// <summary>
/// Log a delta time message to specified file
/// </summary>
/// <param name="msg"></param>
/// <param name="t0"></param>
/// <param name="logFileName"></param>
/// <returns></returns>

		public static DateTime TimeMessage (
			string msg,
			DateTime t0,
			string logFileName)
		{
			DateTime t = t0;
			string msg2 = FormatTimeMessage(msg, ref t);
			Message(msg2, logFileName);
			return t; // return current time
		}

/// <summary>
/// Format a delta time message
/// </summary>
/// <param name="msg"></param>
/// <param name="t"></param>
/// <returns></returns>

		public static string FormatTimeMessage(
			string msg,
			ref DateTime t)
		{
			double ms = TimeOfDay.Delta(ref t);
			string msg2 = msg + string.Format(" {0:0.00} ms.", ms);
			return msg2;
		}


/// <summary>
/// FormatElapsedTimeInMs
/// </summary>
/// <param name="t"></param>
/// <returns></returns>

		public static string FormatElapsedTimeInMs(
			DateTime t)
		{
			double ms = TimeOfDay.Delta(t);
			string msg =  string.Format("{0:0.00} ms.", ms);
			return msg;
		}

		/// <summary>
		/// Log a delta time message 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="t0"></param>
		/// <returns></returns>

		public int TimeMessage(
			string msg,
			int t0,
			string logFileName = "")
		{
			Message(msg + " " + MSTime.FormatDelta(ref t0), logFileName);
			return t0; // return current time
		}

		/// <summary>
		/// Log a delta time message to specified file
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="t0"></param>
		/// <param name="logFileName"></param>
		/// <returns></returns>

		public static void StopwatchMessage(
			string msg,
			Stopwatch sw,
			string logFileName)
		{
			string msg2 = FormatStopwatchMessage(msg, sw);
			Message(msg2, logFileName);
			return;
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

	} // end of Log class
}
