using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace Mobius.ComOps
{
/// <summary>
/// Mobius debugging class 
/// </summary>

	public class DebugMx
	{

		public static bool True = Math.Abs(1) == 1; // True constant (avoids compiler warning)
		public static bool False = Math.Abs(1) == 2; // False constant (avoids compiler warning)

/// <summary>
/// Log an invalid condition
/// </summary>
/// <param name="message"></param>

		public static void InvalidConditionException(string message)
		{
			string msg = 
				message + "\r\n" + 
				new StackTrace(true);

			DebugLog.Message(msg);
			throw new Exception(msg);
		}

		/// <summary>
		/// Throw Exception
		/// </summary>
		/// <param name="msg"></param>

		public static void Exception(
			string msg)
		{
			throw new Exception(msg);
		}

		/// <summary>
		/// Throw ArgumentException
		/// </summary>
		/// <param name="msg"></param>

		public static void ArgException(
			string msg)
		{
			throw new ArgumentException(msg);
		}

		/// <summary>
		/// Throw InvalidDataException
		/// </summary>
		/// <param name="msg"></param>

		public static void DataException(
			string msg)
		{
			throw new InvalidDataException(msg);
		}


	}

}
