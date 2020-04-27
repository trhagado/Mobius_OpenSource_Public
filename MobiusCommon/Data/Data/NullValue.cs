using System;
using System.Drawing;

namespace Mobius.Data
{
	/// <summary>
	/// Common Null Values
	/// </summary>
	
    [Serializable]
	public class NullValue
	{
		public const int NullNumber = -4194303; // "special" value -(2**22 - 1) used for null numeric values, works in single precision floating values

		public const int NonExistantValue = -4194302; // a value that "doesn't exist", i.e. it's not even null

		public static string NullValueString = ""; // string used to display missing values

		/// <summary>
		/// Return true if value is "null", i.e. really null or an equivalent null value
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static bool IsNull(object o)
		{
			if (o == null || o is DBNull) return true;

			else if (o is MobiusDataType) return ((MobiusDataType)o).IsNull;

			else if (o is string)
			{
				if (String.IsNullOrWhiteSpace((string)o)) return true;
			}

			else if (o is int)
			{
				if ((int)o == NullNumber) return true;
				if ((int)o == NonExistantValue) return true; // consider nonexistant as null but not vice versa
			}

			else if (o is long)
			{
				if ((long)o == NullNumber) return true;
				if ((long)o == NonExistantValue) return true; // consider nonexistant as null but not vice versa
			}

			else if (o is float)
			{
				if ((float)o == NullNumber) return true;
				if ((float)o == NonExistantValue) return true; // consider nonexistant as null but not vice versa
			}

			else if (o is double)
			{
				if ((double)o == NullNumber) return true;
				if ((double)o == NonExistantValue) return true; // consider nonexistant as null but not vice versa
			}

			else if (o is DateTime)
				if ((DateTime)o == DateTime.MinValue) return true;

			//else if (o.ToString().Trim() == "") return true;

			return false;
		}

/// <summary>
/// Return true if a value is "nonexistant" (i.e. not null & not not-null)
/// These values are used in combined unpivoted/pivoted tables
/// </summary>
/// <param name="o"></param>
/// <returns></returns>

			public static bool IsNonExistant(object o)
		{
			if (o is int && (int)o == NonExistantValue) return true;

			else if (o is long && (long)o == NonExistantValue) return true;

			else if (o is float && (float)o == NonExistantValue) return true;

			else if (o is double && (double)o == NonExistantValue) return true;

			else if (o is MobiusDataType) return ((MobiusDataType)o).IsNonExistant;

			else return false;
		}
	}
}
