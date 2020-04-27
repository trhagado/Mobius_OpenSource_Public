using Mobius.ComOps;

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Mobius.Data
{

	/// <summary>
	/// Target assay result row
	/// </summary>

	public class UnpivotedAssayResult : AssayAttributes
	{
		public string CompoundId;
		public MoleculeMx Structure;

		public int ActivityBin = NullValue.NullNumber; // default binning of result
		public StringMx ActivityClass; // cond format class name and color for result

		public QualifiedNumber ResultValue;
		public string ResultQualifier;
		public double ResultNumericValue = NullValue.NullNumber;
		public string ResultTextValue;
		public int NValue = NullValue.NullNumber;
		public int NValueTested = NullValue.NullNumber;
		public double StdDev = NullValue.NullNumber;
		public double StdErr = NullValue.NullNumber;
		public string Units;
		public double Conc = NullValue.NullNumber;
		public string ConcUnits;

		public QualifiedNumber MostPotentVal;
		public int ActivityBinMostPotent = NullValue.NullNumber;
		public int ResultID; // id of summarized result

		public DateTime RunDate = DateTime.MinValue;
		public string ResultDetail; // individual assay results going into target summary value

		//public int TargetMapX = NullValue.NullNumber;
		//public int TargetMapY = NullValue.NullNumber;

		public string Sources; // source databases contributing to this value
		public double DrawingOrder = NullValue.NullNumber;
		public int CidOrder = NullValue.NullNumber;

		public static Color Green = Color.FromArgb(230, 59, 59); // muted green
		public static Color Yellow = Color.FromArgb(252, 224, 57); // muted yellow
		public static Color Red = Color.FromArgb(135, 210, 49); // muted red

		//static Color Green = Color.FromArgb(128, 255, 128); // muted green
		//static Color Yellow = Color.FromArgb(255, 255, 128); // muted yellow
		//static Color Red = Color.FromArgb(255, 128, 128); // muted red

		/// <summary>
		/// Return true if table name is a summarized MultiDb view table
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsSummarizedMdbAssayTable(string mtName)
		{
			return IsUnpivotedSummarizedMdbAssayTable(mtName) ||
				IsPivotedMdbAssayTableName(mtName);
		}

		/// <summary>
		/// Return true if unpivoted summarized MultiDb table name
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsUnpivotedSummarizedMdbAssayTable(string mtName)
		{
			if (Lex.EndsWith(mtName, "_NZ")) mtName = mtName.Substring(0, mtName.Length - 3); // Netezza fix
			return Lex.Eq(mtName, MultiDbAssayDataNames.BaseTableName) || Lex.Eq(mtName, MultiDbAssayDataNames.CombinedTableName);
		}

		/// <summary>
		/// Return true if MultiDb pivoted table
		/// </summary>

		public static bool IsPivotedMdbAssayTableName(string mtName)
		{
			return Lex.StartsWith(mtName, MultiDbAssayDataNames.CombinedPivotTablePrefix) ||
				Lex.StartsWith(mtName, MultiDbAssayDataNames.BasePivotTablePrefix);
		}

		/// <summary>
		/// Return true if multi-database view table
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static bool IsCombinedMdbAssayTable(string mtName)
		{
			return Lex.Eq(mtName, MultiDbAssayDataNames.CombinedTableName) ||
			 Lex.StartsWith(mtName, MultiDbAssayDataNames.CombinedPivotTablePrefix) ||
			 Lex.Eq(mtName, MultiDbAssayDataNames.CombinedNonSumTableName);
		}

		/// <summary>
		/// Parse a pivoted table name
		/// </summary>
		/// <param name="mtName"></param>
		/// <param name="geneSymbol"></param>
		/// <param name="metaTableTemplateName"></param>
		/// <returns></returns>

		public static bool TryParsePivotedTableName(
			string mtName,
			out string geneSymbol,
			out string metaTableTemplateName)
		{
			geneSymbol = metaTableTemplateName = null;

			if (Lex.StartsWith(mtName, MultiDbAssayDataNames.BasePivotTablePrefix))
			{
				geneSymbol = mtName.Substring(MultiDbAssayDataNames.BasePivotTablePrefix.Length).ToUpper();
				metaTableTemplateName = MultiDbAssayDataNames.BasePivotTemplate;
			}

			else if (Lex.StartsWith(mtName, MultiDbAssayDataNames.CombinedPivotTablePrefix))
			{
				geneSymbol = mtName.Substring(MultiDbAssayDataNames.CombinedPivotTablePrefix.Length).ToUpper();
				metaTableTemplateName = MultiDbAssayDataNames.CombinedPivotTemplate;
			}

			else return false;

			return true;
		}

		/// <summary>
		/// Return true if the MetaColumn is from an unpivoted table that contains both SP and CRC values
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static bool IsSpAndCrcUnpivotedAssayResultColumn(MetaColumn mc)
		{
			string mtName = mc.MetaTable.Name;
				if ((Lex.StartsWith(mtName, MultiDbAssayDataNames.NgrNonSumUnpivotedTableName) && // unpivoted NGR table
				 (Lex.Eq(mc.Name, "RSLT_VALUE") || Lex.Eq(mc.Name, "RSLT_VALUE_NBR")))
				||
				 (Lex.Eq(mtName, MultiDbAssayDataNames.CombinedNonSumTableName) && // old unpivoted MultiDb table
				 (Lex.Eq(mc.Name, "RSLT_VAL") || Lex.Eq(mc.Name, "RSLT_VAL_NBR"))))
				return true;

			else return false;
		}

		/// <summary>
		/// Serialize a UnpivotedAssayResult into value object
		/// </summary>
		/// <param name="voLength"></param>
		/// <param name="voi"></param>
		/// <returns></returns>

		new public object[] ToValueObject(
			int voLength,
			UnpivotedAssayResultFieldPositionMap voMap)
		{
			object[] vo = new object[voLength];
			ToValueObject(vo, voMap);
			return vo;
		}

		new public void ToValueObject(
			object[] vo,
			UnpivotedAssayResultFieldPositionMap voMap)
		{
			base.ToValueObject(vo, voMap); // set the basic attribute values

			UnpivotedAssayResult row = this;
			//if (row.ResultValue != null) row = row; // debug

			SetVo(vo, voMap.CompoundId.Voi, row.CompoundId);
			SetVo(vo, voMap.Structure.Voi, row.Structure);

			SetVo(vo, voMap.ActivityBin.Voi, row.ActivityBin);
			SetVo(vo, voMap.ActivityClass.Voi, row.ActivityClass);
			SetVo(vo, voMap.ResultValueQn.Voi, row.ResultValue);
			SetVo(vo, voMap.ResultQualifier.Voi, row.ResultQualifier);
			SetVo(vo, voMap.ResultNumericValue.Voi, row.ResultNumericValue);
			SetVo(vo, voMap.ResultTextValue.Voi, row.ResultTextValue);
			SetVo(vo, voMap.NValue.Voi, row.NValue);
			SetVo(vo, voMap.NValueTested.Voi, row.NValueTested);
			SetVo(vo, voMap.StdDev.Voi, row.StdDev);
			SetVo(vo, voMap.Units.Voi, row.Units);
			SetVo(vo, voMap.Conc.Voi, row.Conc);
			SetVo(vo, voMap.ConcUnits.Voi, row.ConcUnits);
			SetVo(vo, voMap.RunDate.Voi, row.RunDate);

			SetVo(vo, voMap.MostPotentValueQn.Voi, row.MostPotentVal);
			SetVo(vo, voMap.ActivityBinMostPotent.Voi, row.ActivityBinMostPotent);

			StringMx sx = new StringMx(row.ResultDetail); // put link to cid, targetId & resultType
			sx.DbLink = "'" + row.CompoundId + "','" + row.GeneId + "','" + row.ResultTypeConcType + "'";
			SetVo(vo, voMap.ResultDetailId.Voi, sx);

			//SetVo(vo, voMap.TargetMapX.Voi, row.TargetMapX);
			//SetVo(vo, voMap.TargetMapY.Voi, row.TargetMapY);

			SetVo(vo, voMap.Sources.Voi, row.Sources);
			SetVo(vo, voMap.DrawingOrder.Voi, row.DrawingOrder);
			SetVo(vo, voMap.CidOrder.Voi, row.CidOrder);

			return;
		}
		static void SetVo(
			object[] vo,
			int voi,
			object value)
		{
			VoArray.SetVo(vo, voi, value);
			return;
		}

		/// <summary>
		/// Convert a value object[] into a new UnpivotedAssayResult
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="voi"></param>
		/// <returns></returns>

		new public static UnpivotedAssayResult FromValueObjectNew(
			object[] vo,
			UnpivotedAssayResultFieldPositionMap voMap)
		{
			UnpivotedAssayResult row = new UnpivotedAssayResult();
			row.FromValueObject(vo, voMap);
			return row;
		}

		/// <summary>
		/// Convert a value object[] into a ResultsRow
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="voi"></param>
		/// <returns></returns>

		new public void FromValueObject(
			object[] vo,
			UnpivotedAssayResultFieldPositionMap voMap)
		{
			string txt;
			int i1;
			double d1;
			DateTime dt;

			base.FromValueObject(vo, voMap);
			UnpivotedAssayResult row = this;

			row.CompoundId = GetStringVo(vo, voMap.CompoundId.Voi);
			row.Structure = GetStructureVo(vo, voMap.Structure.Voi);

			row.ActivityBin = GetIntVo(vo, voMap.ActivityBin.Voi);
			row.ActivityClass = GetSxVo(vo, voMap.ActivityClass.Voi);

			row.ResultValue = GetQnVo(vo, voMap.ResultValueQn.Voi);
			if (!NullValue.IsNull(row.ResultValue))
			{ // copy values to individual fields in case not in the vo
				row.ResultQualifier = row.ResultValue.Qualifier;
				row.ResultNumericValue = row.ResultValue.NumberValue;
				row.ResultTextValue = row.ResultValue.TextValue;
				row.NValue = row.ResultValue.NValue;
				row.NValueTested = row.ResultValue.NValueTested;
				row.StdDev = row.ResultValue.StandardDeviation;
				row.StdErr = row.ResultValue.StandardError;
			}

			txt = GetStringVo(vo, voMap.ResultQualifier.Voi);
			if (txt != null) row.ResultQualifier = txt;

			d1 = GetDoubleVo(vo, voMap.ResultNumericValue.Voi);
			if (d1 != NullValue.NullNumber) row.ResultNumericValue = d1;

			txt = GetStringVo(vo, voMap.ResultTextValue.Voi);
			if (txt != null) row.ResultTextValue = txt;

			i1 = GetIntVo(vo, voMap.NValue.Voi);
			if (d1 != NullValue.NullNumber) row.NValue = i1;

			i1 = GetIntVo(vo, voMap.NValueTested.Voi);
			if (i1 != NullValue.NullNumber) row.NValueTested = i1;

			d1 = GetDoubleVo(vo, voMap.StdDev.Voi);
			if (d1 != NullValue.NullNumber) row.StdDev = d1;

			row.Units = GetStringVo(vo, voMap.Units.Voi);
			row.Conc = GetDoubleVo(vo, voMap.Conc.Voi);
			row.ConcUnits = GetStringVo(vo, voMap.ConcUnits.Voi);
			row.RunDate = GetDateVo(vo, voMap.RunDate.Voi);

			row.MostPotentVal = GetQnVo(vo, voMap.MostPotentValueQn.Voi);
			row.ActivityBinMostPotent = GetIntVo(vo, voMap.ActivityBinMostPotent.Voi);

			row.ResultDetail = GetStringVo(vo, voMap.ResultDetailId.Voi);

			//row.TargetMapX = GetIntVo(vo, voMap.TargetMapX.Voi);
			//row.TargetMapY = GetIntVo(vo, voMap.TargetMapY.Voi);

			row.Sources = GetStringVo(vo, voMap.Sources.Voi);
			row.DrawingOrder = GetDoubleVo(vo, voMap.DrawingOrder.Voi);
			row.CidOrder = GetIntVo(vo, voMap.CidOrder.Voi);

			return;
		}

		/// <summary>
		/// Put row values into standard form
		/// </summary>
		/// <param name="r"></param>

		public void NormalizeValues()
		{
			string msg;

			UnpivotedAssayResult r = this;

			// Normalize SP concentrations to micromolar units

			if (String.IsNullOrEmpty(r.ConcUnits) && r.AssayName != null)
			{ // if no conc in data try to get from assay name
				Match m = Regex.Match(r.AssayName, "(\\d+\\.*\\d*) ?(uM|mM|nM)", RegexOptions.IgnoreCase);
				if (m.Success)
				{
					r.ConcUnits = m.Groups[2].Value;
					r.Conc = double.Parse(m.Groups[1].Value);
				}
			}

			// Normalize SP concentrations to micromolar units

			if (!String.IsNullOrEmpty(r.ConcUnits) && r.Conc > 0)
			{
				if (Lex.Eq(r.ConcUnits, "mM"))
				{
					r.Conc *= 1000.0;
					r.ConcUnits = "uM";
				}

				else if (Lex.Eq(r.ConcUnits, "nM"))
				{
					r.Conc /= 1000.0;
					r.ConcUnits = "uM";
				}

				else if (Lex.Ne(r.ConcUnits, "uM"))
					msg = "Can't convert units: " + r.ConcUnits;
			}

			// Normalize CRC concentrations to micromolar units

			if (Lex.Eq(r.ResultTypeConcType, "CRC"))
			{

				if (Lex.Eq(r.Units, "mM"))
				{
					if (r.ResultNumericValue != NullValue.NullNumber)
						r.ResultNumericValue *= 1000.0;
					if (r.ResultValue != null && r.ResultValue.NumberValue != NullValue.NullNumber)
						r.ResultValue.NumberValue *= 1000.0;
					if (r.StdDev > 0) r.StdDev *= 1000.0;
					r.Units = "uM";
				}

				else if (Lex.Eq(r.Units, "nM"))
				{
					if (r.ResultNumericValue != NullValue.NullNumber)
						r.ResultNumericValue /= 1000.0;
					if (r.ResultValue != null && r.ResultValue.NumberValue != NullValue.NullNumber)
						r.ResultValue.NumberValue /= 1000.0;
					if (r.StdDev > 0) r.StdDev /= 1000.0;
					r.Units = "uM";
				}

				else if (Lex.Ne(r.Units, "uM"))
					msg = "Can't convert units: " + r.Units;
			}

			return;
		}

		/// <summary>
		/// Activity class includes both a class name (High, Medium, Low) and an activity color (Green to Red)
		/// and is calculated from ActivityBin
		/// </summary>

		public void SetActivityClass()
		{
			UnpivotedAssayResult sRow = this;
			int bin = sRow.ActivityBin;

			StringMx sx = new StringMx();

			if (bin < 0)
			{
				sx.Value = "Missing Data";
				sx.BackColor = CondFormat.CsfUndefined;
			}

			else if (bin <= 3)
			{
				sx.Value = "Fail";
				sx.BackColor = CondFormat.CsfRed;
			}

			else if (bin <= 6)
			{
				sx.Value = "BorderLine";
				sx.BackColor = CondFormat.CsfYellow;
			}

			else
			{
				sx.Value = "Pass";
				sx.BackColor = CondFormat.CsfGreen;
			}

			sRow.ActivityClass = sx;
			return;
		}

		/// <summary>
		/// Build the ConditionalFormatting used for the ActivityClass column
		/// </summary>
		/// <returns></returns>

		public static CondFormat BuildActivityClassCondFormat()
		{
			CondFormatRule rule;
			Color color;
			string name;

			CondFormat cf = CondFormat.BuildDefaultConditionalFormatting();
			//cf.ShowInHeaders = false;
			cf.ColumnType = MetaColumnType.String;
			cf.Name = "Activity Class Conditional Formatting";
			for (int ri = 0; ri < cf.Rules.Count; ri++) // change to equality on integer class values
			{
				CondFormatRule r = cf.Rules[ri];
				if (r.OpCode != CondFormatOpCode.Null)
				{
					r.OpCode = CondFormatOpCode.Eq;
					r.Op = CondFormatRule.ConvertOpCodeToName(r.OpCode);
					r.Value = r.Name;
				}

			}

			cf.Rules.InitializeInternalMatchValues(cf.ColumnType);

			return cf;
		}


		/// <summary>
		/// Set the activity bin for the row
		/// </summary>

		public void SetActivityBin()
		{
			SetActivityBin(ResultValue, ResultNumericValue, out ActivityBin); // set the activity bin if possible
		}

		/// <summary>
		/// Set the activity bin for the row (now set in SQL)
		/// </summary>

		public void SetActivityBin(
			QualifiedNumber resultValueQn,
			double resultNumericValue,
			out int activityBin)
		{
			UnpivotedAssayResult sRow = this;

			double val = 0;
			if (resultNumericValue > 0) // use numeric value if we have it
				val = resultNumericValue;
			else if (resultValueQn != null && resultValueQn.NumberValue > 0) // otherwise use any qualified number numeric value
				val = resultValueQn.NumberValue;

			if (val <= 0) activityBin = 0; // no valid value

			else if (Lex.Eq(sRow.ResultTypeConcType, "SP"))
			{ // calc SP activity bin
				double crc = ConvertSPToCRC(val, sRow.Conc);
				activityBin = CalcCRCActivityBin(crc);
			}

			else // calc CRC activity bin
				activityBin = CalcCRCActivityBin(val);
		}

		/// <summary>
		/// Get label for bin
		/// </summary>
		/// <param name="bin"></param>
		/// <returns></returns>

		public static string GetBinLabel(int bin)
		{
			if (bin >= 1 && bin <= 10)
			{
				if (bin < 10) return "Bin  " + bin; // add extra space so text sorts properly
				else return "Bin " + bin;
			}

			else return "Undefined";
		}

		/// <summary>
		/// Calculate the color of the bin, values range from 1 - 10
		/// </summary>
		/// <param name="bin"></param>
		/// <returns></returns>

		public static Color CalculateBinColor(int bin)
		{
			if (bin <= 0 || bin > 10) return Color.FromArgb(0, 128, 255); // muted blue if out of known range

			Color green = Color.FromArgb(80, 175, 40);
			Color red = Color.FromArgb(169,1,72); 
				
			Color color = CondFormatMatcher.CalculateColorForGradientValue(bin, 1, 10, green, red);
			//Color color = CondFormatMatcher.CalculateColorForGradientValue(bin, 1, 10, Red, Green);
			return color;
		}

		/// <summary>
		/// Build the ConditionalFormatting used for the ActivityClass column
		/// </summary>
		/// <returns></returns>

		public static CondFormat BuildActivityBinCondFormat()
		{
			CondFormatRule rule;
			CondFormat cf = new CondFormat();
			//cf.ShowInHeaders = false;
			cf.ColumnType = MetaColumnType.Integer;
			cf.Name = "Activity Bin Conditional Formatting";
			CondFormatRules rules = cf.Rules = new CondFormatRules();

			for (int bin = 1; bin <= 10; bin++) // create one rule for each value from 1 - 10
			{
				rule = new CondFormatRule();
				rule.Name = GetBinLabel(bin);
				rule.Op = "Equal to";
				rule.OpCode = CondFormatOpCode.Eq;
				rule.Value = bin.ToString();

				rule.BackColor1 = CalculateBinColor(bin);
				rules.Add(rule);
			}

			//rule = new CondFormatRule("Low", CondFormatOpCode.Lt, "1");
			//rule.BackColor = CalculateBinColor(1);
			//rules.Add(rule);

			//rule = new CondFormatRule("High", CondFormatOpCode.Gt, "10");
			//rule.BackColor = CalculateBinColor(10);
			//rules.Add(rule);

			rule = new CondFormatRule("Missing", CondFormatOpCode.Null, "");
			rule.BackColor1 = Color.White;
			rules.Add(rule);

			cf.Rules.InitializeInternalMatchValues(cf.ColumnType);

			return cf;
		}

		/// <summary>
		/// Set result value conditional format color based on  bin value
		/// </summary>

		public void SetResultValueBackColor()
		{
			SetResultValueBackColor(ResultValue, ActivityBin);
		}

		/// <summary>
		/// Set result value conditional format color based on  bin value
		/// </summary>

		public void SetResultValueBackColor(
			QualifiedNumber resultValue,
			int activityBin)
		{
			if (activityBin < 0) return; // just return if bin not defined

			QualifiedNumber v = resultValue;
			if (v == null) return;
			v.BackColor = CalculateBinColor(activityBin);

			return;
		}

		/// <summary>
		/// Do crude conversion of SP value to CRC value
		/// </summary>
		/// <param name="spVal"></param>
		/// <param name="Conc"></param>
		/// <returns></returns>

		double ConvertSPToCRC(
			double spVal,
			double spConc)
		{
			double crcVal = 99.9;
			if (spVal >= 90) crcVal = 0.999;
			else if (spVal < 90 && spVal >= 70) crcVal = 9.99;

			if (spConc > 0 && spVal >= 40 && spConc < crcVal)
			{ // take the most favorable of scheme above considering concentration
				crcVal = spConc;
				crcVal -= 0.001;
			}

			return crcVal;
		}

		/// <summary>
		/// Calculate activity bin for CRC data that gives reasonable distribution of color in visualization
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>

		int CalcCRCActivityBin(
			double crcVal)
		{
			int bin;

			if (crcVal <= 0.01)
				bin = 10;
			else if (crcVal <= 0.1 && crcVal > 0.01)
				bin = 9;
			else if (crcVal <= 0.5 && crcVal > 0.1)
				bin = 8;
			else if (crcVal <= 1 && crcVal > 0.5)
				bin = 7;
			else if (crcVal <= 5 && crcVal > 1)
				bin = 5;
			else if (crcVal < 10 && crcVal > 5)
				bin = 4;
			else
				bin = 1;

			return bin;
		}

		/// <summary>
		/// Set Spotfire drawing order so CRC is always on top of SP
		/// </summary>

		public void SetDrawingOrder()
		{
			double v = ResultNumericValue;
			if (Lex.Eq(ResultTypeConcType, "CRC"))
				DrawingOrder = 1000 + 1 / v;

			else // SP
			{
				if (v > 900) v = 900; // don't want wacky SP values to bump up a value into the CRC range
				if (v <= 0) v = 1;
				DrawingOrder = v;
			}

			DrawingOrder = -DrawingOrder; // make negative since drawing order is from largest to smallest value
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public UnpivotedAssayResult CloneUnpivotedAssayResult()
		{
			return (UnpivotedAssayResult)this.MemberwiseClone();
		}

		// Added to get TargetAssayMetabroker to compile

		public static string AllUnpivotedAssayDataTableName = "";
		public static string AllUnpivotedTargetSummaryTableName = "";
		public static string TargetSummaryPivotTablePrefix = "";

	} // end of UnpivotedAssayResult

	/// <summary>
	/// Mapping between UnpivotedAssayResult and a value object array
	/// </summary>

	public class UnpivotedAssayResultFieldPositionMap : TargetAssayAttrsFieldPositionMap
	{
		public AssayAttributePosition CompoundId = new AssayAttributePosition(TarColEnum.CompoundId);
		public AssayAttributePosition ParentId = new AssayAttributePosition(TarColEnum.ParentId);
		public AssayAttributePosition Structure = new AssayAttributePosition(TarColEnum.Structure);

		public AssayAttributePosition ActivityBin = new AssayAttributePosition(TarColEnum.ActivityBin);
		public AssayAttributePosition ActivityClass = new AssayAttributePosition(TarColEnum.ActivityClass);
		public AssayAttributePosition ResultValueQn = new AssayAttributePosition(TarColEnum.ResultValue);
		public AssayAttributePosition ResultQualifier = new AssayAttributePosition(TarColEnum.ResultQualifier);
		public AssayAttributePosition ResultNumericValue = new AssayAttributePosition(TarColEnum.ResultNumericValue);
		public AssayAttributePosition ResultTextValue = new AssayAttributePosition(TarColEnum.ResultTextValue);
		public AssayAttributePosition NValue = new AssayAttributePosition(TarColEnum.NValue);
		public AssayAttributePosition NValueTested = new AssayAttributePosition(TarColEnum.NValueTested);
		public AssayAttributePosition StdDev = new AssayAttributePosition(TarColEnum.StdDev);
		public AssayAttributePosition Units = new AssayAttributePosition(TarColEnum.Units);
		public AssayAttributePosition Conc = new AssayAttributePosition(TarColEnum.Conc);
		public AssayAttributePosition ConcUnits = new AssayAttributePosition(TarColEnum.ConcUnits);
		public AssayAttributePosition RunDate = new AssayAttributePosition(TarColEnum.RunDate);
		public AssayAttributePosition ResultDetailId = new AssayAttributePosition(TarColEnum.ResultDetail);
		//public AssayAttributePosition TargetMapX = new AssayAttributePosition(TarColEnum.TargetMapX);
		//public AssayAttributePosition TargetMapY = new AssayAttributePosition(TarColEnum.TargetMapY);
		public AssayAttributePosition Sources = new AssayAttributePosition(TarColEnum.Sources);
		public AssayAttributePosition DrawingOrder = new AssayAttributePosition(TarColEnum.DrawingOrder);
		public AssayAttributePosition CidOrder = new AssayAttributePosition(TarColEnum.CidOrder);

		public AssayAttributePosition MostPotentValueQn = new AssayAttributePosition(TarColEnum.MostPotentVal);
		public AssayAttributePosition ActivityBinMostPotent = new AssayAttributePosition(TarColEnum.ActivityBinMostPotent);

		public static UnpivotedAssayResultFieldPositionMap NewMdbAssayMap(QueryTable qt)
		{
			UnpivotedAssayResultFieldPositionMap m = new UnpivotedAssayResultFieldPositionMap();
			m.InitializeMultiDbUnpivotedAssayResultFieldPositionMap(qt);
			return m;
		}

		/// <summary>
		/// InitializeMultiDbUnpivotedAssayResultFieldPositionMap
		/// </summary>
		/// <param name="qt"></param>

		public void InitializeMultiDbUnpivotedAssayResultFieldPositionMap(QueryTable qt)
		{
			ColNameToFieldPosition[qt.MetaTable.KeyMetaColumn.Name] = CompoundId; 

			ColNameToFieldPosition[MultiDbAssayDataNames.Structure] = Structure;

			ColNameToFieldPosition[MultiDbAssayDataNames.GeneFamily] = GeneFamily;
			ColNameToFieldPosition[MultiDbAssayDataNames.GeneSymbol] = TargetSymbol;
			ColNameToFieldPosition[MultiDbAssayDataNames.ResultType] = ResultTypeConcType;
			ColNameToFieldPosition[MultiDbAssayDataNames.AssayType] = AssayType;
			ColNameToFieldPosition[MultiDbAssayDataNames.AssayMode] = AssayMode;
			ColNameToFieldPosition[MultiDbAssayDataNames.ActivityBin] = ActivityBin;
			ColNameToFieldPosition[MultiDbAssayDataNames.AverageValueQn] = ResultValueQn;
			ColNameToFieldPosition[MultiDbAssayDataNames.ResultQualifier] = ResultQualifier;
			ColNameToFieldPosition[MultiDbAssayDataNames.AverageValue] = ResultNumericValue;
			ColNameToFieldPosition[MultiDbAssayDataNames.NValue] = NValue;
			ColNameToFieldPosition[MultiDbAssayDataNames.StdDev] = StdDev;
			ColNameToFieldPosition[MultiDbAssayDataNames.Units] = Units;
			ColNameToFieldPosition[MultiDbAssayDataNames.Conc] = Conc;
			ColNameToFieldPosition[MultiDbAssayDataNames.MostPotentValueQn] = MostPotentValueQn;
			ColNameToFieldPosition[MultiDbAssayDataNames.ActivityBinMostPotent] = ActivityBinMostPotent;
			ColNameToFieldPosition[MultiDbAssayDataNames.ResultDetailId] = ResultDetailId;
			ColNameToFieldPosition[MultiDbAssayDataNames.TargetMapX] = TargetMapX;
			ColNameToFieldPosition[MultiDbAssayDataNames.TargetMapY] = TargetMapY;
			ColNameToFieldPosition[MultiDbAssayDataNames.Sources] = Sources;
		}

		/// <summary>
		/// Init original map
		/// </summary>

		public static UnpivotedAssayResultFieldPositionMap NewOriginalMap()
		{
			UnpivotedAssayResultFieldPositionMap m = new UnpivotedAssayResultFieldPositionMap();
			m.InitializeOriginalUnpivotedAssayResultFieldPositionMap();
			return m;
		}

		/// <summary>
		/// Init original map
		/// </summary>

		void InitializeOriginalUnpivotedAssayResultFieldPositionMap()
		{
			ColNameToFieldPosition["COMPOUND_ID"] = CompoundId;
			ColNameToFieldPosition["ACTIVITY_BIN"] = ActivityBin;
			ColNameToFieldPosition["ACTIVITY_CLASS"] = ActivityClass;
			ColNameToFieldPosition["RSLT_VAL"] = ResultValueQn;
			ColNameToFieldPosition["RSLT_VAL_QUALIFIER"] = ResultQualifier;
			ColNameToFieldPosition["RSLT_VAL_NBR"] = ResultNumericValue;
			ColNameToFieldPosition["RSLT_VAL_TXT"] = ResultTextValue;
			ColNameToFieldPosition["RSLT_N_VAL"] = NValue;
			ColNameToFieldPosition["RSLT_NBR_OF_RUN"] = NValueTested;
			ColNameToFieldPosition["RSLT_STD_DEVTN"] = StdDev;
			ColNameToFieldPosition["RSLT_UOM"] = Units;
			ColNameToFieldPosition["CONC_VAL"] = Conc;
			ColNameToFieldPosition["CONC_UOM"] = ConcUnits;
			ColNameToFieldPosition["RUN_DT"] = RunDate;
			ColNameToFieldPosition["RSLT_DTL"] = ResultDetailId;
			ColNameToFieldPosition["TARGET_MAP_X"] = TargetMapX;
			ColNameToFieldPosition["TARGET_MAP_Y"] = TargetMapY;
			ColNameToFieldPosition["DRWNG_ORDR"] = DrawingOrder;
			ColNameToFieldPosition["CID_ORDR"] = CidOrder;
		}

	}

	/// <summary>
	/// Enum associating an integer value to each column
	/// </summary>

	public enum TarColEnum
	{
		Unknown,

		// Attributes

		Id,
		TargetSymbol,
		TargetId,
		TargetDescription,
		GeneFamily,
		GeneFamilyTargetSymbol, // catenation of target family & gene symbol

		AssayDatabase, 
		AssayId2, // secondary id
		AssayIdNbr, // numeric form of assay/table identifier
		AssayIdTxt, // text form of assay/table identifier
		AssayMetaTableName, // name of the metatable containing the data
		AssayName,
		AssayNameSx, // contains name & link when deserialized from StringEx
		AssayDesc,
		AssayLocation,
		AssayType,
		AssayMode,
		AssayStatus,
		AssayGeneCount,

		ResultName, // result label
		ResultTypeId2, // secondary result type id
		ResultTypeIdNbr, // numeric form of result type code
		ResultTypeIdTxt, // text form of result type code
		ResultTypeUnits, // result units
		ResultTypeConcType, // SP or CRC
		ResultTypeConcUnits, // concentration units
		TopLevelResult, // Y if this is a CRC or if a SP with no associated CRC

		Remapped,
		Multiplexed,
		Reviewed,
		ProfilingAssay,
		CompoundsAssayed,

		ResultCount,
		AssayUpdateDate, // date of most recent assay result
		AssociationSource, // ASSAY2EG
		AssociationConflict, // conflicting source & it's assignment

		TargetMapX,
		TargetMapY,
		TargetMapZ,

		// Results

		CompoundId,
		ParentId,
		Structure,
		ActivityBin,
		ActivityClass,
		ResultValue,
		ResultQualifier,
		ResultNumericValue,
		ResultTextValue,
		NValue,
		NValueTested,
		StdDev,
		Units,
		Conc,
		ConcUnits,
		MostPotentVal,
		ActivityBinMostPotent,

		RunDate,
		ResultDetail,
		Sources,
		DrawingOrder,
		CidOrder
	}

	/// <summary>
	/// MultiDbAssayDataNames
	/// </summary>

	public class MultiDbAssayDataNames
	{
		public const string BaseTableName = "BASE_MULTI_DB_SUM"; // unpivoted MultiDb summarized table
		public const string BasePivotedTableName = "BASE_MULTI_DB_SUM_PIVOTED"; // creates pivots for all targets in result set with data
		public const string BasePivotTablePrefix = "BASE_MULTI_DB_PIVOT_"; // prefix for MultiDb pivoted target result tables
		public const string BaseNonSumTableName = "BASE_MULTI_DB_NONSUM"; // MultiDb unpivoted assay level data
		public const string BasePivotTemplate = "BASE_MULTI_DB_PIVOT_TEMPLATE"; // template metatable for pivots by target
		public const string BaseModelQueryParmName = "TargetSummaryModelQuery";

		public const string CombinedTableName = "COMBINED_MULTI_DB_SUM"; // unpivoted combined summarized table
		public const string CombinedPivotTablePrefix = "COMBINED_MULTI_DB_PIVOT_"; // prefix for combined pivoted target result tables
		public const string CombinedNonSumTableName = "COMBINED_MULTI_DB_NONSUM"; // unpivoted assay level data
		public const string CombinedPivotTemplate = "COMBINED_MULTI_DB_PIVOT_TEMPLATE";
		public const string CombinedModelQueryParmName = "CombinedTargetSummaryModelQuery";

		public const string NgrNonSumUnpivotedTableName = "NGR_UNPIVOTED"; // unpivoted unsummarized NGR assay level data

		public const string Assay2EGTableName = "ASSAY2EG"; // table with assay/EntrezGene associations

		public const string TargetTargetUnpivotedTableName = "TARGET_TARGET_UNPIVOTED"; // target/target data

		// Summary table column names

		public const string Structure = "SMILES";
		public const string GeneSymbol = "GENE_SYMBOL";
		public const string GeneFamily = "GENE_FAMILY";
		public const string AssayType = "ASSAY_TYPE";
		public const string AssayMode = "ASSAY_MODE";
		public const string ResultType = "ASSAY_MEASUREMENT";
		public const string AverageValueQn = "AVERAGE_VAL_QN";
		public const string ActivityBin = "ACTIVITY_BIN";

		public const string ResultQualifier = "PREFIX";
		public const string AverageValue = "AVERAGE_VAL";
		public const string NValue = "N";
		public const string StdDev = "SDEV";
		public const string Units = "UOM";
		public const string Conc = "CONC";
		public const string MostPotentValueQn = "MOST_POTENT_VAL_QN";
		public const string ActivityBinMostPotent = "ACTIVITY_BIN_MOST_POTENT";
		public const string ResultDetailId = "RESULT_ID";

		public const string MultiDbViewOptions = "MULTI_DB_VIEW_OPTIONS";
		public const string TargetMapX = "TARGET_MAP_X";
		public const string TargetMapY = "TARGET_MAP_Y";
		public const string Sources = "SOURCES";

		// Unsummarized table column names

		public const string SumResultId = "SUM_RESULT_ID";
		public const string BaseSumResultId = "BASE_SUM_RESULT_ID";

	}

#if false
/// <summary>
/// Interface for handling a set of rows (e.g. DataTable or Array or List of object[]s
/// </summary>

	public interface IRowSet
	{

/// <summary>
/// Number of rows
/// </summary>

			int Count
	    {
				get;
			}

/// <summary>
/// Row indexer 
/// </summary>
/// <param name="index"></param>
/// <returns></returns>

			object[] this[int index]
			{
				get;
			}
	}
#endif

}
