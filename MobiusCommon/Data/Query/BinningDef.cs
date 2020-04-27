using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Mobius.Data
{

	/// <summary>
	/// Type of binning for grouped values
	/// </summary>

	public enum BinTypeId
	{
		Default = 0, // Groups combine unique field values.
		StringFirstLetter = 14, // Text values are grouped by the first letter of the string
		Numeric = 15, // Values are grouped into intervals as defined by a numeric interval size

		Date = 1, // Values are grouped by the date part. The time part of the values is ignored.
		DateDay = 2,
		DateDayOfWeek = 3,
		DateDayOfYear = 4,
		DateWeekOfMonth = 5, // Values are grouped by the number of the week in which they occur in a month. The following groups can be created: 1, 2, 3, 4 and 5. The first week is the week containing the 1st day of the month.
		DateWeekOfYear = 6,

		DateMonth = 7, // Values are grouped by the month part. Examples of groups: January, February, March
		DateQuarter = 8, // Values are grouped by the quarterly intervals of the year, i.e.: 1, 2, 3 and 4. 
		DateYear = 9, // Values are grouped by the year part. Examples of such groups: 2003, 2004, 2005.

		DateMonthYear = 20, // Values are grouped by months and years. Examples of groups: August 2013, September 2014, January 2015, ...
		DateQuarterYear = 21, // Values are grouped by the year and quarter. Examples of groups: Q3 2012, Q4 2012, Q1 2013, Q2 2013, ...
	}
}
