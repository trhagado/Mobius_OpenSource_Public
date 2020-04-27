using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Mobius.ComOps
{
	/// <summary>
	/// Allocate & free a console window
	/// </summary>

	public class ConsoleWindow
	{
		[DllImport("kernel32.dll")]
		public static extern Boolean AllocConsole();
		[DllImport("kernel32.dll")]
		public static extern Boolean FreeConsole();

		public static bool Display = false;
		public static bool Allocated = false;
		public static bool IncludeTimeStamp = true;

		/// <summary>
		/// Write a line to the console
		/// </summary>
		/// <param name="text"></param>

		public static void WriteLine(string text)
		{
			Write(text + "\r\n");
			return; 
		}

/// <summary>
/// Write a line to the console
/// </summary>
/// <param name="text"></param>

		public static void Write(string text)
		{
			if (!Display) return;
			if (!Allocated) Allocate();

			if (IncludeTimeStamp)
			{
				DateTime dt = DateTime.Now;

				text = "===>>> " + // make messages easier to pick out within large chunks of text
					dt.Hour + ":" + dt.Minute + ":" + dt.Second + "." + dt.Millisecond + " - " + text;
			}
			Console.Write(text);
			return;
		}

		/// <summary>
		/// Allocate console
		/// </summary>
		/// <returns></returns>

		public static bool Allocate()
		{
			bool allocated = AllocConsole();
			if (allocated) Allocated = true;
			return allocated;
		}

		/// <summary>
		/// Free console
		/// </summary>
		/// <returns></returns>

		public static bool Free()
		{
			bool freed = FreeConsole();
			if (freed) Allocated = false;
			return freed;
		}
	}
}
