using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Handle Mobius-Spotfire Aggregation functions
	/// </summary>

	public class Aggregation
	{
		public static string None = "(None)"; // no aggregation

		public static List<string> NumericAggregationMethods = new List<string>() {
			"Sum",
			"Avg (Average)",
			"Count",
			"UniqueCount",
			"Min",
			"Max",
			"Median",
			"StdDev (Standard Deviation)",
			"StdErr (Standard Error)",
			"Var (Variance)",
			"L95 (Lower endpoint of 95% Confidence Interval)",
			"U95 (Upper endpoint of 95% Confidence Interval)",
			"Q1 (First Quartile)",
			"Q3 (Third Quartile)",
			"LAV (Lower Adjacent Value)",
			"UAV (Upper Adjacent Value)",
			"UniqueConcatenate",
			"Concatenate",
			"CountBig",
			"First",
			"GeometricMean",
			"IQR (Interquartile Range)",
			"Last",
			"LIF (Lower Inner Fence)",
			"LOF (Lower Outer Fence)",
			"MeanDeviation",
			"MedianAbsoluteDeviation",
			"MostCommon",
			"Outliers (Outlier Count)",
			"P10 (10th Percentile)",
			"P90 (90th Percentile)",
			"PctOutliers (Outlier Percentage)",
			"Product",
			"Range",
			"UIF (Upper Inner Fence)",
			"UOF (Upper Outer Fence)"
		};

		public static List<string> StringAggregationMethods = new List<string>()
		{
			"Count",
			"UniqueCount",
			"Min",
			"Max",
			"UniqueConcatenate",
			"Concatenate",
			"CountBig",
			"First",
			"Last",
			"MostCommon"
		};

		public static List<string> DateAggregationMethods = new List<string>()
		{
			"Count",
			"UniqueCount",
			"Min",
			"Max",
			"UniqueConcatenate",
			"Concatenate",
			"CountBig",
			"First",
			"Last",
			"MostCommon",
			"Range"
		};

		public static List<string> ExpressionShortcutAggregationMeasures = new List<string>()
		{
			"Cumulative Sum",
			"Moving Average",
			"Difference",
			"Difference %",
			"Difference Year Over Year",
			"Difference % Year over Year",
			"% of Total",
			"Year to Date Total",
			"Year to Date Growth",
			"Change Relative to Start",
			"Change Relative to Fixed Point",
			"Compound Annual Growth Rate",
			"Top Category"
		};

		public static List<string> GetAggregationListForDataType(DataTypeMsxEnum dataType)
		{

			switch (dataType)
			{
				case DataTypeMsxEnum.Integer:
				case DataTypeMsxEnum.Real:
				case DataTypeMsxEnum.LongInteger:
				case DataTypeMsxEnum.SingleReal:
					return NumericAggregationMethods;

				case DataTypeMsxEnum.String:
				case DataTypeMsxEnum.Boolean:
				case DataTypeMsxEnum.Binary:
				case DataTypeMsxEnum.Currency:
					return StringAggregationMethods;

				case DataTypeMsxEnum.Time:
				case DataTypeMsxEnum.DateTime:
				case DataTypeMsxEnum.Date:
				case DataTypeMsxEnum.TimeSpan:
					return DateAggregationMethods;

				default:
					return new List<string>() { "No aggregation possible" };
			}
		}


		public static string GetMatchingLongName(
			List<string> list,
			string shortName)
		{
			string shortName2 = shortName + " ("; // match start of any associated long name

			foreach (string longName in list)
			{
				if (Lex.Eq(longName, shortName) || Lex.StartsWith(longName, shortName2))
					return longName;
			}

			return null; 
		}

		public static string GetShortName(
			string longName)
		{
			if (Lex.StartsWith(longName, "(")) return "";

			int i = longName.IndexOf(" ("); // look for parenthesized part of name
			if (i < 0) return longName;
			else return longName.Substring(0, i);
		}

	} // Aggregation

}
