using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{
/// <summary>
/// Matching of database values agains CondFormatRules
/// </summary>
	public class CondFormatMatcher
	{
		public static Color DefaultMissingValueColor = Color.LightGray;

/// <summary>
/// Match a general object value to conditional formatting & return the first matching item
/// </summary>
/// <param name="rules"></param>
/// <param name="o"></param>
/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			object o)
		{
			if (o == null) return MatchNull(condFormat);
			else if (o is double) return Match(condFormat, (double)o);
			else if (o is int) return Match(condFormat, (int)o);
			else if (o is string) return Match(condFormat, (string)o);
			else if (o is DateTime) return Match(condFormat, (DateTime)o);

			else if (o is CompoundId) return Match(condFormat, ((CompoundId)o).Value);
			else if (o is MoleculeMx) return Match(condFormat, (MoleculeMx)o);
			else if (o is QualifiedNumber) return Match(condFormat, (QualifiedNumber)o);
			else if (o is NumberMx) return Match(condFormat, (o as NumberMx).Value);
			else if (o is StringMx) return Match(condFormat, (o as StringMx).Value);
			else if (o is DateTimeMx) return Match(condFormat, (o as DateTimeMx).Value);
			else return null;
		}

/// <summary>
/// Match a null value
/// </summary>
/// <param name="rules"></param>
/// <returns></returns>

		public static CondFormatRule MatchNull(
			CondFormat condFormat)
		{
			CondFormatRules rules = condFormat.Rules;
			for (int ri = 0; ri < rules.Count; ri++)
			{
				CondFormatRule rule = rules[ri];
				if (rule.OpCode == CondFormatOpCode.Null) return rule;
			}

			return null;
		}

/// <summary>
/// Match QualifiedNumber value 
/// </summary>
/// <param name="rules"></param>
/// <param name="value"></param>
/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			QualifiedNumber value)
		{
			if (value.IsNull)
				return Match(condFormat, NullValue.NullNumber);
			else return Match(condFormat, value.NumberValue);

//			else if (MetaColumn.IsNumericMetaColumnType(condFormat.ColumnType))
//			{
//				return Match(condFormat, value.NumberValue);
//			}
//			else return Match(condFormat, value.TextValue);
		}

		/// <summary>
		/// Match integer value 
		/// </summary>
		/// <param name="rules"></param>
		/// <param name="value"></param>
		/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			int value)
		{
			return Match(condFormat, (double)value);
		}


		/// <summary>
		/// Match a double value to conditional formatting & return the first matching item
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			double value)
		{
			CondFormatRules rules = condFormat.Rules;
			CondFormatRule rule = null;
			int ri;
			object o;

			if (value == NullValue.NullNumber)
				return MatchNull(condFormat);

			int lowerLimitRule = -1;
			int upperLimitRule = -1;
			for (ri = 0; ri < rules.Count; ri++)
			{
				rule = rules[ri];

				if (rule.OpCode == CondFormatOpCode.Between)
				{
					if (rules.ColoringStyle == CondFormatStyle.ColorScale) break; // only one between rule for newer color scale cond formatting (allow reverse ordering)
					if (rules.ColoringStyle == CondFormatStyle.DataBar) break; // only one between rule for data bars 

					if (rule.ValueNumber > rule.Value2Number) // order low to high
						{ double d1 = rule.ValueNumber; rule.ValueNumber = rule.Value2Number; rule.Value2Number = d1; }

					if (value >= rule.ValueNumber && value <= rule.Value2Number) break; // within range?
				}

				else if (rule.OpCode == CondFormatOpCode.NotBetween)
				{
					if (rule.ValueNumber > rule.Value2Number)
					{ double d1 = rule.ValueNumber; rule.ValueNumber = rule.Value2Number; rule.Value2Number = d1; }
					if (value + rule.Epsilon < rule.ValueNumber || value - rule.Epsilon > rule.Value2Number) break;
				}

				else if (rule.OpCode == CondFormatOpCode.Eq &&
					Math.Abs(value - rule.ValueNumber) < rule.Epsilon) break;

				else if (rule.OpCode == CondFormatOpCode.NotEq &&
					Math.Abs(value - rule.ValueNumber) >= rule.Epsilon) break;

				else if (rule.OpCode == CondFormatOpCode.Gt &&
					value + rule.Epsilon > rule.ValueNumber) break;

				else if (rule.OpCode == CondFormatOpCode.Lt &&
					value - rule.Epsilon < rule.ValueNumber) break;

				else if (rule.OpCode == CondFormatOpCode.Ge)
				{
					if (rules.ColoringStyle == CondFormatStyle.ColorScale) // older continuous coloring?
						lowerLimitRule = ri;
					else if (value + rule.Epsilon >= rule.ValueNumber)	break;
				}

				else if (rule.OpCode == CondFormatOpCode.Le)
				{
					if (rules.ColoringStyle == CondFormatStyle.ColorScale) // older continuous coloring?
					{
						upperLimitRule = ri; 
						break;
					}
					else if (value - rule.Epsilon <= rule.ValueNumber) break;
				}

				else if (rule.OpCode == CondFormatOpCode.NotNull || rule.OpCode == CondFormatOpCode.Exists) // value is not null or exists
					break;
			}

			if (ri >= rules.Count) return null; // return null if no rule matches

			// Matches a rule associated with a particular color

			if (rules.ColoringStyle == CondFormatStyle.ColorSet)
			{
				return rule;
			}

			// Calculate a linear gradient between the two color limits

			else if (rules.ColoringStyle == CondFormatStyle.ColorScale && rule.OpCode == CondFormatOpCode.Between)
			{ // newer single color scale rule
				rule = BuildColorRuleForColorScale(value, rule);
				return rule;
			}

			else if (rules.ColoringStyle == CondFormatStyle.ColorScale && lowerLimitRule >= 0 && upperLimitRule >= lowerLimitRule)
			{ // older two-rule, double color scale rules
				CondFormatRule r1 = rules[lowerLimitRule];
				CondFormatRule r2 = rules[upperLimitRule];

				rule = BuildColorRuleForGradientValue(value, r1.ValueNumber, r2.ValueNumber, r1.BackColor1, r2.BackColor1);
				return rule;
			}

			// Calculate a percentage for a data bar for a value between the two color limits

			else if (rules.ColoringStyle == CondFormatStyle.DataBar)
			{
				rule = BuildPercentageRuleForDataBarValue(value, rule);
				return rule;
			}

			else if (rules.ColoringStyle == CondFormatStyle.IconSet)
			{
				rule = BuildImageRuleForIconSetValue(rule);
				return rule; // image name should be included in rule
			}

			else return rule; // shouldn't happen

		}

		/// <summary>
		/// Build a rule that provides the appropriate color for the supplied range limit values and colors (newer single-between-rule CF)
		/// </summary>
		/// <param name="v"></param>
		/// <param name="rule"></param>
		/// <returns></returns>

		public static CondFormatRule BuildColorRuleForColorScale(double v, CondFormatRule rule)
		{
			Color c = Color.Transparent;
			Color c0, c1, c2;

			double v1 = rule.ValueNumber;
			double v2 = rule.Value2Number;

			CondFormatRule r2 = new CondFormatRule();

			Color[] colors = Bitmaps.GetColorSetByName(Bitmaps.ColorScaleImageColors, rule.ImageName);
			if (colors == null) return r2;
			
			if (colors.Length == 2) // 2-Color gradient
			{
				if (v1 <= v2) // normal gradient
					c = CalculateColorForGradientValue(v, v1, v2, colors[0], colors[1]);

				else // reverse gradient
					c = CalculateColorForGradientValue(v, v2, v1, colors[1], colors[0]); 
			}

			else if (colors.Length == 3) // 3-color gradient
			{
				double midpoint = GeometryMx.Midpoint(v1, v2);



				if (v1 <= v2) // normal gradient
				{
					if (v < midpoint) // get color between first two colors
						c = CalculateColorForGradientValue(v, v1, midpoint, colors[0], colors[1]);

					else // get color between second two colors
						c = CalculateColorForGradientValue(v, midpoint, v2, colors[1], colors[2]);
				}

				else // reverse gradient
				{
					if (v < midpoint) // get color between first two colors
						c = CalculateColorForGradientValue(v, v2, midpoint, colors[2], colors[1]);

					else // get color between second two colors
						c = CalculateColorForGradientValue(v, midpoint, v1, colors[1], colors[0]);
				}

			}

			else return r2;

			r2.BackColor1 = c;
			return r2;
		}

		/// <summary>
		/// Build a rule that provides the appropriate color for the supplied range limit values and colors (older double-rule CF)
		/// </summary>
		/// <param name="v"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>

		public static CondFormatRule BuildColorRuleForGradientValue(double v, double v1, double v2, Color c1, Color c2)
		{
			Color c = CalculateColorForGradientValue(v, v1, v2, c1, c2);
			CondFormatRule r2 = new CondFormatRule();
			r2.BackColor1 = c;
			return r2;
		}

		/// <summary>
		/// Calculate the color value for the supplied gradient and column value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>

		public static Color CalculateColorForGradientValue(double v, double v1, double v2, Color c1, Color c2)
		{
			if (v1 == v2) return c1; // both limits the same

			if (v < v1) v = v1;
			if (v > v2) v = v2;
			double f = (v - v1) / (v2 - v1);

			int r = (int)Math.Abs(c1.R + (c2.R - c1.R) * f);
			if (r > 255) r = 255;

			int g = (int)Math.Abs(c1.G + (c2.G - c1.G) * f);
			if (g > 255) g = 255;

			int b = (int)Math.Abs(c1.B + (c2.B - c1.B) * f);
			if (b > 255) b = 255;

			Color c = Color.FromArgb(r, g, b);
			return c;
		}

		/// <summary>
		/// Build a rule that includes the appropriate color and bar percentage for the supplied rule and column value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="rule"></param>
		/// <returns></returns>

		public static CondFormatRule BuildPercentageRuleForDataBarValue(double v, CondFormatRule rule)
		{
			int pct = -1;

			CondFormatRule r2 = new CondFormatRule();
			double v1 = rule.ValueNumber;
			double v2 = rule.Value2Number;

			if (v1 <= v2) // normal bar
				pct = CalculatePercentageForDataBarValue(v, v1, v2);

			else // reverse bar (i.e. v2 < v1) 
			{
				v = v2 - (v - v1);
				pct = CalculatePercentageForDataBarValue(v, v2, v1);
			}

			Color[] colors = Bitmaps.GetColorSetByName(Bitmaps.DataBarsImageColors, rule.ImageName); // get single color associated with image
			if (colors == null || colors.Length < 1) return r2;

			Color c = colors[0];
			//Color c = rule.BackColor1; // get from back color (more general but not currently used)

			r2.BackColor1 = Color.FromArgb(pct, c); // store pct in color alpha value
			return r2;
		}

		/// <summary>
		/// Calculate a bar percentage for the supplied range and value
		/// </summary>
		/// <param name="v"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>Value between 0 and 100</returns>

		public static int CalculatePercentageForDataBarValue(double v, double v1, double v2)
		{
			if (v == NullValue.NullNumber) return 0;
			if (v1 == v2) return 0;

			double dPct = (v - v1) / (v2 - v1) * 100;
			int pct = (int)(dPct + .5);
			if (pct < 0) return 0;
			if (pct > 100) return 100;
			return pct;
		}

		/// <summary>
		/// Create and return a new rule with the icon image index store in the back color alpha field
		/// </summary>
		/// <param name="rule"></param>
		/// <returns></returns>

		public static CondFormatRule BuildImageRuleForIconSetValue(CondFormatRule rule)
		{
			CondFormatRule r2 = new CondFormatRule();

			int ii = Bitmaps.GetImageIndexFromName(Bitmaps.I.IconSetImages, rule.ImageName);
			if (ii < 0) ii = 0; // shouldn't happen, but show error image if it does

			Color c = Color.FromArgb(ii, Color.Transparent); // store icon image index in color alpha
			r2.BackColor1 = c; // store pct in color alpha value
			return r2;
		}

		/// <summary>
		/// Match a DateTime value
		/// </summary>
		/// <param name="rules"></param>
		/// <param name="dtValue"></param>
		/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			DateTime dtValue)
		{
			CondFormatRules rules = condFormat.Rules;
			if (dtValue == DateTime.MinValue) return MatchNull(condFormat);
			string dateString = DateTimeMx.Normalize(dtValue);
			CondFormatRule matchingRule = Match(condFormat, dateString);
			return matchingRule;
		}

		/// <summary>
		/// Match a string value to conditional formatting & return the first matching item
		/// Handles dates converted to normalized string values also (yyyymmdd)
		/// </summary>
		/// <param name="rules"></param>
		/// <param name="value"></param>
		/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			string stringValue)
		{
			double v, v1, v2;
			int ri;
			object o;

			CondFormatRules rules = condFormat.Rules;
			CondFormatRule rule = null;
			DateTime dtValue = DateTime.MinValue, dt2;

			if (condFormat.ColumnType == MetaColumnType.CompoundId)
				return Match(condFormat, new CompoundId(stringValue));

			if (String.IsNullOrEmpty(stringValue)) return MatchNull(condFormat);

			int lowerLimitRule = -1;
			int upperLimitRule = -1;

			for (ri = 0; ri < rules.Count; ri++)
			{
				rule = rules[ri];

				if (!String.IsNullOrEmpty(stringValue))
				{ // check where we have a value

					string value = rule.Value;
					if (rule.ValueNormalized != null) value = rule.ValueNormalized;

					string value2 = rule.Value2;
					if (rule.Value2Normalized != null) value2 = rule.Value2Normalized;

					if (rule.OpCode == CondFormatOpCode.Within) // check date range
					{
						dtValue = DateTimeMx.NormalizedToDateTime(stringValue); // convert to a DateTime
						double withinValue = rule.ValueNumber;

						if (Lex.Contains(value2, "Day"))
							dt2 = dtValue.AddDays(withinValue);

						else if (Lex.Contains(value2, "Week"))
							dt2 = dtValue.AddDays(withinValue * 7);

						else if (Lex.Contains(value2, "Month"))
							dt2 = dtValue.AddMonths((int)withinValue); // note must be integer months

						else if (Lex.Contains(value2, "Year"))
							dt2 = dtValue.AddYears((int)withinValue); // note must be integer years

						else throw new Exception("Unexpected date unit: " + value2);

						dt2 = dt2.AddDays(1); // add one day to dt2 since it's time is 12:00 AM and the Now date includes the hours passed so far today

						if (DateTime.Compare(dt2, DateTime.Now) >= 0)
							break; // if stored date/time + within value is >= current date/time then condition passes
						else continue;
					}

					if (rule.OpCode == CondFormatOpCode.Between && value2 != null && value != null)
					{
						if (Lex.Gt(value, value2))
						{ string s1 = value; value = value2; value2 = s1; }

						if (Lex.Ge(stringValue, value) && Lex.Le(stringValue, value2)) break;
					}

					else if (rule.OpCode == CondFormatOpCode.NotBetween && value2 != null && value != null)
					{
						if (Lex.Gt(value, value2))
						{ string s1 = value; value = value2; value2 = s1; }

						if (Lex.Lt(stringValue, value) || Lex.Gt(stringValue, value2)) break;
					}

					else if (rule.OpCode == CondFormatOpCode.Eq &&
						Lex.Eq(stringValue, value)) break;

					else if (rule.OpCode == CondFormatOpCode.NotEq &&
						Lex.Ne(stringValue, value)) break;

					else if (rule.OpCode == CondFormatOpCode.Gt &&
						Lex.Gt(stringValue, value)) break;

					else if (rule.OpCode == CondFormatOpCode.Lt &&
						Lex.Lt(stringValue, value)) break;

					else if (rule.OpCode == CondFormatOpCode.Ge &&
						Lex.Ge(stringValue, value))
					{
						if (rules.ColoringStyle == CondFormatStyle.ColorScale || ri > rules.Count - 1) break;
						else lowerLimitRule = ri;
					}

					else if (rule.OpCode == CondFormatOpCode.Le &&
						Lex.Le(stringValue, value))
					{
						upperLimitRule = ri;
						break;
					}

					else if (rule.OpCode == CondFormatOpCode.Substring &&
						stringValue.ToLower().IndexOf(value.ToLower()) >= 0) break;

					else if (rule.OpCode == CondFormatOpCode.NotNull || rule.OpCode == CondFormatOpCode.Exists) // value is not null or exists
						break;

					else if (rule.OpCode == CondFormatOpCode.Unknown && // treat unknown as equal
						Lex.Eq(stringValue, value)) break;
				}

				else if (rule.OpCode == CondFormatOpCode.Null || rule.OpCode == CondFormatOpCode.NotExists)
					break; // null value & null conditional format operator
			}

			//DebugLog.Message(stringValue + " " + ri); // debug

			if (ri >= rules.Count) return null; // return null if no rule matches

			// Matches a rule associated with a particular color

			if (rules.ColoringStyle == CondFormatStyle.ColorSet)
			{
				return rule;
			}

			// Calculate a linear gradient between the two color limits

			else if (rules.ColoringStyle == CondFormatStyle.ColorScale && rule.OpCode == CondFormatOpCode.Between)
			{ // newer single color scale rule
				ConvertDateValuesToDoubles(condFormat, stringValue, rule.ValueNormalized, rule.Value2Normalized, out v, out v1, out v2);
				rule.ValueNumber = v1;
				rule.Value2Number = v2;
				rule = BuildColorRuleForColorScale(v, rule);
				return rule;
			}

			else if (rules.ColoringStyle == CondFormatStyle.ColorScale && lowerLimitRule >= 0 && upperLimitRule >= lowerLimitRule)
			{ // older two-rule, double color scale rules
				CondFormatRule r1 = rules[lowerLimitRule];
				CondFormatRule r2 = rules[upperLimitRule];

				ConvertDateValuesToDoubles(condFormat, stringValue, r1.ValueNormalized, r2.ValueNormalized, out v, out v1, out v2);
				rule.ValueNumber = v1;
				rule.Value2Number = v2;
				rule = BuildColorRuleForGradientValue(v, v1, v2, r1.BackColor1, r2.BackColor1);
				return rule;
			}

			// Calculate a percentage for a data bar for a value between the two color limits

			else if (rules.ColoringStyle == CondFormatStyle.DataBar)
			{
				ConvertDateValuesToDoubles(condFormat, stringValue, rule.ValueNormalized, rule.Value2Normalized, out v, out v1, out v2);
				rule.ValueNumber = v1;
				rule.Value2Number = v2;
				rule = BuildPercentageRuleForDataBarValue(v, rule);
				return rule;
			}

			else if (rules.ColoringStyle == CondFormatStyle.IconSet)
			{
				rule = BuildImageRuleForIconSetValue(rule);
				return rule; // image name should be included in rule
			}

			else return rule; // shouldn't happen
		}

		/// <summary>
		/// Convert dates to doubles to allow for databar and color scale Cond formats
		/// </summary>
		/// <param name="condFormat"></param>
		/// <param name="stringValue"></param>
		/// <param name="v1Normal"></param>
		/// <param name="v2Normal"></param>
		/// <param name="v"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>

		static void ConvertDateValuesToDoubles(
			CondFormat condFormat,
			string stringValue,
			string v1Normal,
			string v2Normal,
			out double v,
			out double v1,
			out double v2)
		{
			DateTime date, date1, date2; // convert to delta day values relative to date1

			if (condFormat.ColumnType == MetaColumnType.Date) // !String.IsNullOrEmpty(r1.ValueNormalized))
			{ // build between date values
				date1 = DateTimeMx.NormalizedToDateTime(v1Normal);
				date2 = DateTimeMx.NormalizedToDateTime(v2Normal);
				date = DateTimeMx.NormalizedToDateTime(stringValue);
				v = date.Subtract(date1).TotalDays;
				v1 = 0;
				v2 = date2.Subtract(date1).TotalDays;
			}

			else v = v1 = v2 = 0; // treat ordinal continuous coloring as numeric conditional formatting
			return;
		}

		/// <summary>
		/// Match a CompoundId value (not currently used)
		/// </summary>
		/// <param name="rules"></param>
		/// <param name="dtValue"></param>
		/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			CompoundId cid)
		{
			CondFormatRules rules = condFormat.Rules;
			CondFormatRule rule = null;
			CidList cidList;
			int ri, objId;

			for (ri = 0; ri < rules.Count; ri++)
			{
				rule = rules[ri];
				if (rule.ValueDict == null) // read in cid list if not done yet
				{
					rule.ValueDict = new Dictionary<string, object>(); // set empty dict so if fails we won't try again
					if (!int.TryParse(rule.Value2, out objId)) continue;
					if (CidList.ICidListDao == null) continue;
					cidList = CidList.ICidListDao.VirtualRead(objId, null);
					rule.ValueDict["CidList"] = cidList;
				}

				cidList = rule.ValueDict["CidList"] as CidList;
				if (cidList.Contains(cid.Value)) break;
			}		

			if (ri < rules.Count) return rule;
			else return null;
		}

/// <summary>
/// Match a structure to conditional formatting & return the first matching item
/// </summary>
/// <param name="rules"></param>
/// <param name="structure"></param>
/// <returns></returns>

		public static CondFormatRule Match(
			CondFormat condFormat,
			MoleculeMx structure)
		{
			CondFormatRules rules = condFormat.Rules;
			CondFormatRule rule = null;
			int ri;

			if (structure == null || structure.PrimaryFormat == MoleculeFormat.Unknown)
				return MatchNull(condFormat);

			StructureMatcher matcher = new StructureMatcher();

			for (ri = 0; ri < rules.Count; ri++) // match rules one at a time until get a hit
			{
				rule = rules[ri];
				if (String.IsNullOrEmpty(rule.Value)) continue;

				MoleculeMx query = new MoleculeMx(rule.Value);
				matcher.SetSSSQueryMolecule(query);
				bool matches = matcher.IsSSSMatch(structure);
				//DebugLog.Message("Rule: " + ri + ", " + query.SmilesString + ", " + structure.SmilesString + ", " + matches);
				if (matches) break;
			}

			if (ri < rules.Count) return rule;
			else return null;
		}
	}
}
