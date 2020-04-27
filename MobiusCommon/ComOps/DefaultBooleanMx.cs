using DevExpress.Utils;

using System;
using System.Text;

namespace Mobius.ComOps
{
	public class DefaultBooleanMx
	{
/// <summary>
/// Convert DefaultBoolean to bool
/// </summary>
/// <param name="db"></param>
/// <returns></returns>

		public static bool Convert(DefaultBoolean db)
		{
			if (db != DefaultBoolean.False) return true; // return true if true or default
			else return false;
		}

/// <summary>
/// Convert bool to DefaultBoolean
/// </summary>
/// <param name="b"></param>
/// <returns></returns>

		public static DefaultBoolean Convert(bool b)
		{
			if (b == true) return DefaultBoolean.True; 
			else return DefaultBoolean.False;
		}

	}
}
