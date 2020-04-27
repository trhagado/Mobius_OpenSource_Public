using System;

namespace Mobius.ComOps
{
	/// <summary>
	/// Enumeration utility methods
	/// </summary>
	public class EnumUtil
	{

/// <summary>
/// Get the values associated with an enum name
/// This complements Enum.GetName
/// </summary>
/// <param name="enumType"></param>
/// <param name="enumName"></param>
/// <returns></returns>

		public static int Parse(
			Type enumType,
			string enumName)
		{
			foreach (int value in Enum.GetValues(enumType))
			{
				if (Lex.Eq(Enum.GetName(enumType,value), enumName))
					return value;
			}

			return -1; // not found
		}

		/// <summary>
		/// Try to parse an enum ignoring case
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>

		public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
		{
			return Enum.TryParse(value, true, out result); // call builtin method
		}
	}

	/// <summary>
	/// Helper for manipulating individual "flag" bits in an enum
	/// </summary>

	public static class FlagHelper
	{
		public static bool IsSet<T>(T flags, T flag) where T : struct
		{
			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			return (flagsValue & flagValue) != 0;
		}

		public static void Set<T>(ref T flags, T flag, bool value = true) where T : struct
		{
			int flagsValue = (int)(object)flags;
			int flagValue = (int)(object)flag;

			if (value == true) // set the bit
				flags = (T)(object)(flagsValue | flagValue);

			else // clear the bit
				flags = (T)(object)(flagsValue & (~flagValue));
		}
	}
}
