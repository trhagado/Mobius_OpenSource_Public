using Mobius.ComOps;

using System;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace Mobius.Data
{
	public class RowUtil
	{

		public static string GetString(
			DataRow dr,
			string colName)
		{
			return GetString(dr, colName, "");
		}

		/// <summary>
		/// Get a string colName value
		/// </summary>
		/// <param name="colName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>

		public static string GetString(
			DataRow dr,
			string colName,
			string defaultValue)
		{
			object vo;
			if (!TryGet(dr, colName, out vo)) return defaultValue;
			else return vo as string;
		}

		/// <summary>
		/// Get an integer colName value
		/// </summary>
		/// <param name="colName"></param>
		/// <returns></returns>

		public static int GetInt(
			DataRow dr,
			string colName)
		{
			return GetInt(dr, colName, NullValue.NullNumber);
		}

		public static int GetInt(
			DataRow dr,
			string colName,
			int defaultValue)
		{
			object vo;
			int intVal;

			if (!TryGet(dr, colName, out vo)) return defaultValue; // see if non-null

			if (!TryConvertToInt(vo, out intVal)) return defaultValue; // see if can be converted to int

			else return intVal;
		}

		public static bool TryConvertToInt(
			object vo,
			out int iVal)
		{
			double dVal;

			iVal = NullValue.NullNumber;

			if (vo == null || vo is DBNull) return false;

			if (vo is int)
			{
				iVal = (int)vo;
				return true;
			}

			if (!QualifiedNumber.TryConvertToDouble(vo, out dVal)) return false;
			if (dVal > int.MaxValue || dVal < int.MinValue) return false;

			iVal = Convert.ToInt32(dVal);
			return true;
		}

		/// <summary>
		/// Get a boolean colName value
		/// </summary>
		/// <param name="colName"></param>
		/// <returns></returns>

		public static bool GetBool(
			DataRow dr,
			string colName)
		{
			return GetBool(dr, colName, false);
		}

		public static bool GetBool(
			DataRow dr,
			string colName,
			bool defaultValue)
		{
			object vo;
			if (!TryGet(dr, colName, out vo)) return defaultValue;

			if (vo is bool)
				return (bool)vo;

			else return defaultValue;
		}

		public static bool TryGet(
			DataRow dr,
			string colName,
			out object vo)
		{
			vo = dr[colName];
			if (vo == null || vo is DBNull) return false;
			else return true;
		}



	}
}
