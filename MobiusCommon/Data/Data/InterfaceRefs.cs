using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{

/// <summary>
/// Data assembly dependency injections
/// Used to allow lower-level assemblies to call higher-level class methods
/// </summary>

	public class InterfaceRefs
	{
		public static IUserObjectDao IUserObjectDao; // Interface to basic UserObjectDao functions called from lower level assemblies

		public static IUserObjectIUD IUserObjectIUD; // Interface for callbacks for UserObjectDao UserObject Insert/Update/Delete operations

		public static IUserObjectTree IUserObjectTree; // Interface for UserObjectTree functions called from lower level assemblies
	}
}
