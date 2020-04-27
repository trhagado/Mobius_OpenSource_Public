using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.ComOps
{
	public class ConvertMx
	{

		/// <summary>
		/// Convert object to int array
		/// </summary>
		/// <param name="oa"></param>
		/// <returns></returns>

		public static int[] ObjectToIntArray(object[] oa)
		{

			int len = oa.Length;

			int[] ia = new int[len];

			for (int oai = 0; oai < len; oai++)
				ia[oai] = (int)oa[oai];

			return ia;
		}

		/// <summary>
		/// Convert int to object array
		/// </summary>
		/// <param name="ia"></param>
		/// <returns></returns>

		public static object[] IntToObjectArray(int[] ia)
		{
			object [] oa = ia.Cast<object>().ToArray();
			return oa;
		}

	}
}
