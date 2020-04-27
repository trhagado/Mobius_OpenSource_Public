using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ComOps
{
/// <summary>
/// Globally accessible client state information
/// </summary>
	public class ClientState
	{
		public static UserInfo UserInfo = new UserInfo(); // userinfo for this session (may be needed by multiple threads;
		public static string ClientName = ""; // client name as passed by client for this session (e.g. MobiusClient - 4.0.6292.23883, 24-Mar-2017)
		public static Version MobiusClientVersion =  null; // client version if standard Mobius client

		/// <summary>
		/// Return true if the current version of the client is at least the specified version
		/// </summary>
		/// <param name="major"></param>
		/// <param name="minor"></param>
		/// <param name="build"></param>
		/// <param name="revision"></param>
		/// <returns></returns>

		public static bool MobiusClientVersionIsAtLeast(int major, int minor = 0, int build = -1, int revision = -1)
		{
			bool isAtLeast = (MobiusClientVersionCompareTo(major, minor, build, revision) >= 0);
			return isAtLeast;
		}

/// <summary>
/// Compare current version of client to some other version number
/// </summary>
/// <param name="major"></param>
/// <param name="minor"></param>
/// <param name="build"></param>
/// <param name="revision"></param>
/// <returns></returns>

		public static int MobiusClientVersionCompareTo(int major, int minor = 0, int build = -1, int revision = -1)
		{
			if (MobiusClientVersion == null) return -1;

			Version v = MobiusClientVersion;

			if (v.Major > major) return 1;
			else if (v.Major < major) return -1;

			if (v.Minor > minor) return 1;
			else if (v.Minor < minor) return -1;

			if (build >= 0) // match on build?
			{
				if (v.Build > build) return 1;
				else if (v.Build < build) return -1;
			}

			if (revision >= 0) // match on revision?
			{
				if (v.Revision > revision) return 1;
				else if (v.Revision < revision) return -1;
			}

			return 0; // match
		}

		/// <summary>
		/// Return true if session is using native Mobius client
		/// </summary>

		public static bool IsNativeSession { get { return Lex.StartsWith(ClientName, "MobiusClient - ") || Lex.StartsWith(ClientName, "4."); } } // normal Mobius client?

		public static bool IsDeveloper
		{ get { return Lex.Eq(UserInfo.UserName, "<DeveloperUserName>"); } }

		/// <summary>
		/// See if a user is an administrator
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool IsAdministrator()
		{
			return UserInfo.Privileges.HasPrivilege("Administrator");
		}

	}
}