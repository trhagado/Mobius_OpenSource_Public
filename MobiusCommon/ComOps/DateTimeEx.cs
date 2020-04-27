using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Mobius.ComOps
{
	/// <summary>
	/// Summary description for Time.
	/// </summary>
	
	public class MSTime
	{

/// <summary>
/// Get time of day in milliseconds
/// </summary>
/// <returns></returns>
		
		public static int Milliseconds ()
		{
			long l1 = System.DateTime.Now.Ticks; // get 100 nanoseconds since 1/1/01
			l1 = l1 / 10000; // convert to milliseconds
			l1 = l1 % (24 * 60 * 60 * 1000); // ms today 
			return (int)l1;
		}

/// <summary>
/// Get delta time in fractional milliseconds
/// </summary>
/// <param name="t0"></param>
/// <returns></returns>

		public static double Delta(DateTime t0)
		{
			DateTime t1 = DateTime.Now;
			TimeSpan ts = t1.Subtract(t0);
			return ts.TotalMilliseconds;
		}

		/// <summary>
		/// Format delta time in fractional milliseconds
		/// </summary>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static string FormatDelta(DateTime t0)
		{
			double msDelta = Delta(ref t0);
			string s = String.Format("{0:0.00}", msDelta);
			return s;
		}

		/// <summary>
		/// Get delta time in ms & reset t0
		/// </summary>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static double Delta(ref DateTime t0)
		{
			DateTime t1 = DateTime.Now;
			TimeSpan ts = t1.Subtract(t0);
			t0 = t1;
			return ts.TotalMilliseconds;
		}

		/// <summary>
		/// Format delta time in ms & reset t0
		/// </summary>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static string FormatDelta(ref DateTime t0)
		{
			double msDelta = Delta(ref t0);
			string s = String.Format("{0:0.00}", msDelta);
			return s;
		}

		/// <summary>
		/// Format an integer MS delta time
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="t0"></param>
		/// <returns></returns>

		public static string FormatDelta(
			ref int t0)
		{
			int t1 = TimeOfDay.Milliseconds();
			string s = (t1 - t0).ToString();
			t0 = t1;
			return s;
		}

	}

	/// <summary>
	/// Deprecated class name
	/// </summary>

	public class TimeOfDay : MSTime
	{
		// Deprecated
	}

	public class StopwatchMx
	{
		/// <summary>
		/// Return the number of milliseconds on the stopwatch and reset
		/// </summary>
		/// <param name="sw"></param>
		/// <returns></returns>

		public static double GetMsAndReset(Stopwatch sw)
		{
			double ms = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			return ms;
		}

	}


	/// <summary>
	/// PerformanceTimer class to keep stats on timing of various sections of code
	/// </summary>
	public class PT : PerformanceTimer { /* Abbreviated alias */ }

	public class PerformanceTimer
	{
		public static Dictionary<string, PerformanceTimer> Stats = new Dictionary<string, PerformanceTimer>();


		public double MsTotal { get { return timeSpan.TotalMilliseconds; } }
		public int Count = 0;
		public double MsAvg { get { return Count > 0 ? MsTotal / Count : 0; } }
		public double MsMin = double.NaN;
		public double MsMax = double.NaN;

		Stopwatch Sw = new Stopwatch();
		TimeSpan timeSpan = new TimeSpan();

		public static PerformanceTimer Start(string name)
		{
			PerformanceTimer pt;

			if (!Stats.ContainsKey(name))
			{
				pt = new PerformanceTimer();
				Stats.Add(name, pt);
			}

			pt = Stats[name];
			pt.Sw.Restart();
			return pt;
		}

		public double Update()
		{
			TimeSpan ts0 = Sw.Elapsed;
			double ms = ts0.TotalMilliseconds;

			timeSpan = timeSpan.Add(ts0);
			Sw.Restart();

			Count++;

			if (double.IsNaN(MsMin) || ms < MsMin)
				MsMin = ms;

			if (double.IsNaN(MsMax) || ms > MsMax)
				MsMax = ms;

			return MsAvg;
		}

		public override string ToString() // note: this sometimes seems to put codelens into a big CPU loop in process Microsoft.Alm.Shared.Remoting.RemoteContainer
		{
			string s = String.Format("Count: {0} , Total ms: {1}, Per Call ms: {2:f1}, Min: {3}, Max: {4}", Count, (int)MsTotal, MsAvg, (int)MsMin, (int)MsMax);
			return s;
		}
	}

	/// <summary>
	/// DateTimeEx - Just some comments for now
	/// </summary>

	public class DateTimeEx
	{

		/**********************************************************
		 *** Here we see all the patterns of the DateTime Class ***
		 **********************************************************

		Format ---------------------------> Result
	  ----------------------------------- -------------------------------------------------------------
		DateTime.Now.ToString("MM/dd/yyyy")	05/29/2015
		DateTime.Now.ToString("dddd, dd MMMM yyyy")	Friday, 29 May 2015
		DateTime.Now.ToString("dddd, dd MMMM yyyy")	Friday, 29 May 2015 05:50
		DateTime.Now.ToString("dddd, dd MMMM yyyy")	Friday, 29 May 2015 05:50 AM
		DateTime.Now.ToString("dddd, dd MMMM yyyy")	Friday, 29 May 2015 5:50
		DateTime.Now.ToString("dddd, dd MMMM yyyy")	Friday, 29 May 2015 5:50 AM
		DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")	Friday, 29 May 2015 05:50:06
		DateTime.Now.ToString("MM/dd/yyyy HH:mm")	05/29/2015 05:50
		DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")	05/29/2015 05:50 AM
		DateTime.Now.ToString("MM/dd/yyyy H:mm")	05/29/2015 5:50
		DateTime.Now.ToString("MM/dd/yyyy h:mm tt")	05/29/2015 5:50 AM
		DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")	05/29/2015 05:50:06
		DateTime.Now.ToString("MMMM dd")	May 29
		DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK")	2015-05-16T05:50:06.7199222-04:00
		DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’")	Fri, 16 May 2015 05:50:06 GMT
		DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss")	2015-05-16T05:50:06
		DateTime.Now.ToString("HH:mm")	05:50
		DateTime.Now.ToString("hh:mm tt")	05:50 AM
		DateTime.Now.ToString("H:mm")	5:50
		DateTime.Now.ToString("h:mm tt")	5:50 AM
		DateTime.Now.ToString("HH:mm:ss")	05:50:06
		DateTime.Now.ToString("yyyy MMMM")	2015 May

	******************************************************************

		d -> Represents the day of the month as a number from 1 through 31. 
		dd -> Represents the day of the month as a number from 01 through 31. 
		ddd-> Represents the abbreviated name of the day (Mon, Tues, Wed etc).
		dddd-> Represents the full name of the day (Monday, Tuesday etc).

		h 12-hour clock hour (e.g. 4).
		hh 12-hour clock, with a leading 0 (e.g. 06)
		H 24-hour clock hour (e.g. 15)
		HH 24-hour clock hour, with a leading 0 (e.g. 22)

		m Minutes
		mm Minutes with a leading zero

		M Month number(eg.3)
		MM Month number with leading zero(eg.04)
		MMM Abbreviated Month Name (e.g. Dec)
		MMMM Full month name (e.g. December)

		s Seconds
		ss Seconds with leading zero

		t Abbreviated AM / PM (e.g. A or P)
		tt AM / PM (e.g. AM or PM

		y Year, no leading zero (e.g. 2015 would be 15)
		yy Year, leadin zero (e.g. 2015 would be 015)
		yyy Year, (e.g. 2015)
		yyyy Year, (e.g. 2015)

		K Represents the time zone information of a date and time value (e.g. +05:00)

		z With DateTime values, represents the signed offset of the local operating system's time zone from
			Coordinated Universal Time (UTC), measured in hours. (e.g. +6)
		zz As z, but with leading zero (e.g. +06)
		zzz With DateTime values, represents the signed offset of the local operating system's time zone from UTC,measured in hours and minutes. (e.g. +06:00)

		f Represents the most significant digit of the seconds fraction; that is, it represents the tenths of a second in a date and time value.
		ff Represents the two most significant digits of the seconds fraction in date and time

		 */
	}

	/// <summary>
	/// Date Intervals
	/// </summary>

	public enum DateInterval
	{
		Undefined = 0,
		None = 0,
		Day = 1,
		Week = 2,
		Month = 3,
		Quarter = 4,
		Year = 5
	}

}
