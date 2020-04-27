using Mobius.ComOps;

using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Collections.Generic;

namespace Mobius.Data
{

	/// <summary>
	/// Collection of known UserObjects for this session
	/// </summary>

	public class UserObjectCollection
	{

		// The set of known UserObjects for a session is maintained in this collection. 
		// In general, they will be the set of UserObjects that are owned by the current 
		// user as well as those that the user has read or write access to. This list is
		// maintained by both the client and the services processes for a session or
		// just a single list is maintained for a in-proc, no services session.
		//
		// A single static instance of the class is used. It also contains state information for the
		// initial population of the user objects.

		public static Dictionary<UserObjectType, List<UserObject>> UserObjectsByType = new Dictionary<UserObjectType, List<UserObject>>(); // dictionary of each user object by type

		public static int SubTreesToRead = 0; // how many trees will be read
		public static int SubtreesRead = 0; // how many of the subtrees have been read
		public static ManualResetEvent BuildWaitEvent = new ManualResetEvent(false);
		public static bool BuildStarted = false;
		public static bool BuildComplete = false;

		/// <summary>
		/// Initialize
		/// </summary>

		public static void Initialize()
		{
			UserObjectsByType = new Dictionary<UserObjectType, List<UserObject>>(); // dictionary of each user object by type

			SubTreesToRead = 0; // how many trees will be read
			SubtreesRead = 0; // how many of the subtrees have been read
			BuildWaitEvent = new ManualResetEvent(false);
			BuildStarted = false;
			BuildComplete = false;

			return;
		}
	}
}


