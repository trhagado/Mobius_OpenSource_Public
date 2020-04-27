using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ComOps
{

/// <summary>
/// Configuration information that is common between the client & server code
/// </summary>

	public class CommonConfigInfo
	{
		public static string MiscConfigDir; // folder for miscellaneous configuration files

		public static string ServicesLogFileName = "MobiusServices - [Date].log"; // name for services log file, [Date] filled in
	}

	/// <summary>
	/// Möbius with umlaut (Alt + 0246, U+00F6)
	/// </summary>

	public class UmlautMobius
	{
		public const string String = "M\u00F6bius";
	}

}
