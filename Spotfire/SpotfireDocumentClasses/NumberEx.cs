using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.SpotfireComOps
{

	/// <summary>
	/// Miscellaneous number functions not specific to Mobius data types (e.g. NumberMx, QualifiedNumber)
	/// </summary>

	public class NumberEx
	{
		public const int NullNumber = -4194303; // "Special" value -(2**22 - 1) used for null numeric values, works in single precision floating values

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
		/// More precise double.TryParse method
		/// </summary>
		/// <param name="str"></param>
		/// <param name="result"></param>
		/// <returns></returns>

		public static bool DoubleTryParseExact(string str, out double result)
		{
			//  The regular decimal.TryParse() is a bit rubbish.  It'll happily accept strings which don't make sense, such as:
			//      123'345'6.78
			//      1''23'345'678
			//      123''345'678
			//
			//  This function does the same as TryParse(), but checks whether the number "makes sense", ie:
			//      - has exactly zero or one "decimal point" characters
			//      - if the string has thousand-separators, then are there exactly three digits inbetween them 
			// 
			//  Assumptions: if we're using thousand-separators, then there'll be just one "NumberGroupSizes" value.
			//
			//  Returns True if this is a valid number
			//          False if this isn't a valid number
			// 
			result = 0;

			if (str == null || string.IsNullOrWhiteSpace(str))
				return false;

			//  First, let's see if TryParse itself falls over, trying to parse the string.
			double val = 0;
			if (!double.TryParse(str, out val))
			{
				//  If the numeric string contains any letters, foreign characters, etc, the function will abort here.
				return false;
			}

			//  Note: we'll ONLY return TryParse's result *if* the rest of the validation succeeds.

			// Use US Culture settings for now // CultureInfo culture = CultureInfo.CurrentCulture;
			string thousands = ","; // culture.NumberFormat.NumberGroupSeparator;               //  Usually a comma, but can be apostrophe in European locations.
			int[] expectedDigitLengths = new int[] { 3 }; // culture.NumberFormat.NumberGroupSizes;         //  Usually a 1-element array:  { 3 }
			string decimalPoint = "."; // culture.NumberFormat.NumberDecimalSeparator;          //  Usually full-stop, but perhaps a comma in France.

			if (!str.Contains(decimalPoint) && !str.Contains(thousands))
				return true;

			str = str.Trim();
			if (str == ".") return false;

			int numberOfDecimalPoints = CountOccurrences(str, decimalPoint);
			if (numberOfDecimalPoints != 0 && numberOfDecimalPoints != 1)
			{
				//  You're only allowed either ONE or ZERO decimal point characters.  No more!
				return false;
			}

			int numberOfThousandDelimiters = CountOccurrences(str, thousands);
			if (numberOfThousandDelimiters == 0)
			{
				result = val;
				return true;
			}

			//  Okay, so this numeric-string DOES contain 1 or more thousand-seperator characters.
			//  Let's do some checks on the integer part of this numeric string  (eg "12,345,67.890" -> "12,345,67")
			if (numberOfDecimalPoints == 1)
			{
				int inx = str.IndexOf(decimalPoint);
				str = str.Substring(0, inx);
			}

			//  Split up our number-string into sections: "12,345,67" -> [ "12", "345", "67" ]
			string[] parts = str.Split(new string[] { thousands }, StringSplitOptions.None);

			if (parts.Length < 2)
			{
				//  If we're using thousand-separators, then we must have at least two parts (eg "1,234" contains two parts: "1" and "234")
				return false;
			}

			//  Note: the first section is allowed to be upto 3-chars long  (eg for "12,345,678", the "12" is perfectly valid)
			if (parts[0].Length == 0 || parts[0].Length > expectedDigitLengths[0])
			{
				//  This should catch errors like:
				//      ",234"
				//      "1234,567"
				//      "12345678,901"
				return false;
			}

			//  ... all subsequent sections MUST be 3-characters in length
			foreach (string oneSection in parts.Skip(1))
			{
				if (oneSection.Length != expectedDigitLengths[0])
					return false;
			}

			result = val;
			return true;
		}

		private static int CountOccurrences(string str, string chr)
		{
			//  How many times does a particular string appear in a string ?
			//
			int count = str.Length - str.Replace(chr, "").Length;
			return count;
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

	/// <summary>
	/// Array utility functions
	/// </summary>

	public class ArrayMx
	{

/// <summary>
/// Fill all elements of an array with a single value
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="arrayToFill"></param>
/// <param name="fillValue"></param>

		public static void Fill<T>(T[] arrayToFill, T fillValue)
		{
			// if called with a single value, wrap the value in an array and call the main function
			ArrayFill<T>(arrayToFill, new T[] { fillValue });
		}

		public static void ArrayFill<T>(T[] arrayToFill, T[] fillValue)
		{
			if (fillValue.Length >= arrayToFill.Length)
			{
				throw new ArgumentException("fillValue array length must be smaller than length of arrayToFill");
			}

			// set the initial array value
			Array.Copy(fillValue, arrayToFill, fillValue.Length);

			int arrayToFillHalfLength = arrayToFill.Length / 2;

			for (int i = fillValue.Length; i < arrayToFill.Length; i *= 2)
			{
				int copyLength = i;
				if (i > arrayToFillHalfLength)
				{
					copyLength = arrayToFill.Length - i;
				}

				Array.Copy(arrayToFill, 0, arrayToFill, i, copyLength);
			}
		}
	}
}
