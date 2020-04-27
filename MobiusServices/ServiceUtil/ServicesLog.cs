using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.Services.Util
{
	/// <summary>
	/// Log a message to the services log file and to the Console if enabled 
	/// All messages from DebugLog get routed here via DebugLogMediator
	/// </summary>

	public class ServicesLog
	{
		public static string LogFileName = "";
		public static bool LogToConsole = true; // if true write to Console

		/// <summary>
		/// Initialize ServicesLog
		/// </summary>

		public static void Initialize(string logFileName)
		{
			if (!Lex.IsNullOrEmpty(logFileName)) LogFileName = logFileName;
			DebugLog.IDebugLog = new DebugLogMediator(); // link DebugLog to us
			return;
		}

		/// <summary>
		/// Route a message to the appropriate places
		/// </summary>
		/// <param name="msg"></param>

		public static void Message(
			string msg, 
			string logFileName = null)
		{
			if (Lex.IsUndefined(logFileName))
				logFileName = LogFileName;

			DebugLog.MessageDirect(msg, logFileName); 

			if (LogToConsole) // write to console
				Console.WriteLine(msg);

			return;
		}
	}

	/// <summary>
	/// Log mediator for DebugLog to properly route messages from the DebugLog class
	/// </summary>

	public class DebugLogMediator : IDebugLog
	{
		/// <summary>
		/// Route a message to Services log
		/// </summary>
		/// <param name="msg"></param>

		public void Message(
			string msg,
			string logFileName = null)
		{
			if (Lex.IsUndefined(logFileName))
				logFileName = ServicesLog.LogFileName;

			DebugLog.MessageDirect(msg, logFileName);
			return;
		}
	}
}
