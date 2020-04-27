using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Mobius.ComOps
{
	public class VersionMx
	{

		/// <summary>
		/// GetExecutingAssemblyVersion
		/// </summary>
		/// <returns></returns>

		public static string GetExecutingAssemblyVersion()
		{
			Version cv = Assembly.GetExecutingAssembly().GetName().Version;
			string version = VersionMx.FormatVersion(cv);
			return version;
		}
		/// <summary>
		/// Format version string
		/// e.g.: 1.2.3.4, January 1, 2001
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>

		public static string FormatVersion(
			Version v)
		{
			if (v == null) return "";
			string txt = v.Major.ToString() + "." + v.Minor;

			if (v.Build > 0 && v.Revision > 0) // show as easy-to-read date also
			{
				DateTime dt = new DateTime(2000, 1, 1, 0, 0, 0);
				dt = dt.AddDays(v.Build);
				dt = dt.AddSeconds(v.Revision * 2);

				// Seems to unnecessarily add an hour
				//if (TimeZone.IsDaylightSavingTime(dt, 
				//	TimeZone.CurrentTimeZone.GetDaylightChanges(dt.Year)))
				//	dt = dt.AddHours(1);

				string tok = dt.ToString("dd-MMMM-yyyy");
				txt += " (" + tok + ")";
			}

			return txt;
		}
	}
}
