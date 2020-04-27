using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{

	/// <summary>
	/// Miscellaneous number functions not specific to Mobius data types (e.g. NumberMx, QualifiedNumber)
	/// </summary>

	public class NumberEx
	{

		/// <summary>
		/// Determine if a primitive value is a number type
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public static bool IsNumber(object obj)
		{
			if (obj == null || obj is DBNull)
				return false;

			Type objType = obj.GetType();
			objType = Nullable.GetUnderlyingType(objType) ?? objType;

			if (objType.IsPrimitive)
			{
				return objType != typeof(bool) &&
						objType != typeof(char) &&
						objType != typeof(IntPtr) &&
						objType != typeof(UIntPtr);
			}

			return objType == typeof(decimal);
		}

		/// <summary>
		/// Convert a primitive number to a double value
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public static double ConvertToDouble(object obj)
		{
			double d = unchecked((double)obj);
			return d;
		}

		/// <summary>
		/// Returns true if double is an integer value within valid range of a 32 bit integer
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>

		public static bool DoubleIsInteger(double d)
		{
			return unchecked(d == (int)d); // unchecked avoids overflow exceptions for very large integers
		}

		/// <summary>
		/// Convert double to integer ignoring any overflow
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>

		public static int DoubleToInteger(double d)
		{
			return unchecked((int)d); // unchecked avoids overflow exceptions for very large integers
		}

		/// <summary>
		/// Convert a double to a string for grouping purposes
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>

		public static string DoubleToGroupByString(double d)
		{
			if (unchecked(d == (int)d)) // if integer use full precision
				return d.ToString();
			else return d.ToString("E5"); // fractional, use 5-digit scientific notation
		}

		/// <summary>
		/// Compare two doubles for "near" equality (fast)
		/// See: http://www.cygnus-software.com/papers/comparingfloats/Obsolete%20comparing%20floating%20point%20numbers.htm
		/// Alias: AlmostEqual2sComplement
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="maxUlps">Max Units in Last Place</param>
		/// <returns></returns>

		public static bool DoublesAlmostEqual(double A, double B, int maxUlps = 4)
		{
			// Make sure maxUlps is non-negative and small enough that the
			// default NAN won't compare as equal to anything.
			if (!(maxUlps > 0 && maxUlps < 4 * 1024 * 1024)) throw new Exception("maxUlps is invalid");

			Int64 aInt = BitConverter.ToInt64(BitConverter.GetBytes(A), 0);
			// Make aInt lexicographically ordered as a twos-complement int
			if (aInt < 0)
				aInt = Int64.MinValue + (-aInt);
			// Make bInt lexicographically ordered as a twos-complement int
			Int64 bInt = BitConverter.ToInt64(BitConverter.GetBytes(B), 0);
			if (bInt < 0)
				bInt = Int64.MinValue + (-bInt);
			Int64 intDiff = Math.Abs(aInt - bInt);
			if (intDiff <= maxUlps)
				return true;
			return false;
		}

		/// <summary>
		/// Compare two floats for "near" equality (fast)
		/// See: http://www.cygnus-software.com/papers/comparingfloats/Obsolete%20comparing%20floating%20point%20numbers.htm
		/// Alias: AlmostEqual2sComplement
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="maxUlps">Max Units in Last Place</param>
		/// <returns></returns>

		public static bool FloatsAlmostEqual(float A, float B, int maxUlps = 4)
		{
			// Make sure maxUlps is non-negative and small enough that the
			// default NAN won't compare as equal to anything.
			if (!(maxUlps > 0 && maxUlps < 4 * 1024 * 1024)) throw new Exception("maxUlps is invalid");

			Int32 aInt = BitConverter.ToInt32(BitConverter.GetBytes(A), 0);
			// Make aInt lexicographically ordered as a twos-complement int
			if (aInt < 0)
				aInt = Int32.MinValue + (-aInt);
			// Make bInt lexicographically ordered as a twos-complement int
			Int32 bInt = BitConverter.ToInt32(BitConverter.GetBytes(B), 0);
			if (bInt < 0)
				bInt = Int32.MinValue + (-bInt);
			Int64 intDiff = Math.Abs(aInt - bInt);
			if (intDiff <= maxUlps)
				return true;
			return false;
		}

	}
}
