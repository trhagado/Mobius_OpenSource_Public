using Mobius.ComOps;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Mobius.Data
{
	/// <summary>
	/// Summary description for CalcField.
	/// </summary>

	public class CalcField
	{
		public string Name => UserObject?.Name; // name displayed to user

		public CalcTypeEnum CalcType = CalcTypeEnum.Basic; // type of calculation (Basic, Advanced)
		public MetaColumnType SourceColumnType = MetaColumnType.Unknown;
		public MetaColumnType PreclassificationlResultType = MetaColumnType.Unknown; // datatype for result of calc field before classification
		public MetaColumnType FinalResultType = MetaColumnType.Unknown; // final datatype for result of calc field

		public string Operation = "";
		public CalcOpEnum OpEnum = CalcOpEnum.Unknown;

		public List<CalcFieldColumn> CfCols = new List<CalcFieldColumn>(); // list of input cols

		public string AdvancedExpr = ""; // expression that calculates the value if in advanced mode
		public string OuterJoinRoot = ""; // name of table to be used as base for outer joins to other tables

		public CondFormat Classification = null; // defines any classification for calc field

		public string ColumnLabel = ""; // the label to use for the resulting column
		public string Description = ""; // calc field description
		public UserObject UserObject = new UserObject(UserObjectType.CalcField); // name, etc of CalcField
		public string Prompt = ""; // for standard calc fields this contains the prompt for the data to be supplied by the user

		// Full set of functions and operations

		public static string[] Funcs = {
			"None",
			"Abs (Absolute value)", 
			"- (Negative value)",
			"Log (Base 10 log)",
			"-Log (Negative base 10 log)",
			"-Log molar concentration",
//			"-Log uM concentration",
//			"-Log nM concentration",
			"Ln (Natural log)",
			"-Ln (Negative natural log)",
			"Sqrt (Square Root)",
			"**2 (Square)",
			"+ Constant",
			"* Constant",
			"Days Since" };

		public static string[] Ops = {
			"None (Use first data field only)",
			"/ (Division)",
			"* (Multiplication)",
			"+ (Addition)",
			"- (Subtraction)",
			"Overlay Curves"};

		// Numeric functions and operations

		public static string[] NumericFuncs = {
			"None",
			"Abs (Absolute value)", 
			"- (Negative value)",
			"Log (Base 10 log)",
			"-Log (Negative base 10 log)",
			"-Log molar concentration",
//			"-Log uM concentration",
			//"-Log nM concentration",
			"Ln (Natural log)",
			"-Ln (Negative natural log)",
			"Sqrt (Square Root)",
			"**2 (Square)",
			"+ Constant",
			"* Constant" };

		public static string[] NumericOps = {
			"/ (Division)",
			"* (Multiplication)",
			"+ (Addition)",
			"- (Subtraction)",
			"None (Use first data field only)" };

		// Date functions and operations

		public static string[] DateFuncs = {
		    "Days Since",
		    "None" };

		public static string[] DateOps = {
		    "- (Subtraction)", // subtraction is the default op
		    "None (Use first data field only)" };

		// Concentration response curves (crc) functions and operations

		public static string[] CrcOverlayFuncs = { "None" };
		public static string[] CrcOverlayOps = { "Overlay Curves" };
		public static string OverlayOp = "overlay";
		public static string OverlayOpPadded = " overlay ";

		/// <summary>
		/// Advanced functions
		/// </summary>
		public static string[] AdvancedFuncs = {
			"ABS(n) - Absolute value", 
			"CEIL(n) - Smallest integer >= n",
			"EXP(n) - Natural exponential",
			"FLOOR(n) - Largest integer <= n",
			"LN(n) - Natural log",
			"LOG(m,n) - Log base m of n",
			"POWER(m,n) - m raised to the nth power",
			"ROUND(m,n) - n rounded to m decimal places",
			"SQRT(n) - Square root",
			"MOD(m,n) - Remainder of m divided by n"};

		/// <summary>
		/// Advanced operations
		/// </summary>
		public static string[] AdvancedOps = {
			"/ (Division)",
			"* (Multiplication)",
			"+ (Addition)",
			"- (Subtraction)", 
			"() (Grouping of subexpressions)"};// Other column types support no functions or operations

		/// <summary>
		/// No functions/ops
		/// </summary>
		public static string[] NoFuncs = { "None" };
		public static string[] NoOps = { "None (Use first data field only)" };

		/// <summary>
		/// Constructor
		/// </summary>

		public CalcField()
		{
			CfCols.Add(new CalcFieldColumn()); // start with two cols
			CfCols.Add(new CalcFieldColumn());
		}

		/// <summary>
		/// Serialize calculated field
		/// </summary>
		/// <returns></returns>
		/// 
		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			mstw.Writer.Formatting = Formatting.Indented;
			Serialize(mstw.Writer);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("CalcField");

			XmlUtil.WriteAttributeIfDefined(tw, "Name", Name);

			tw.WriteAttributeString("CalcType", CalcType.ToString());
			tw.WriteAttributeString("SourceColumnType", SourceColumnType.ToString());

			if (PreclassificationlResultType != MetaColumnType.Unknown)
				tw.WriteAttributeString("PreclassificationlResultType", PreclassificationlResultType.ToString());

			for (int ci = 0; ci < CfCols.Count; ci++)
			{
				CalcFieldColumn cfc = CfCols[ci];
				if (cfc == null) continue;

				int ci1 = ci + 1;
				if (cfc.MetaColumn != null)
				{
					tw.WriteAttributeString("Table" + ci1, cfc.MetaColumn.MetaTable.Name);
					tw.WriteAttributeString("Column" + ci1, cfc.MetaColumn.Name);
				}


				if (cfc.Function != null)
					tw.WriteAttributeString("Function" + ci1, cfc.Function);
				if (cfc.Constant != "")
					tw.WriteAttributeString("Constant" + ci1, cfc.Constant);
			}

			tw.WriteAttributeString("Operation", Operation);

			if (!String.IsNullOrEmpty(AdvancedExpr))
				tw.WriteAttributeString("AdvancedExpr", AdvancedExpr);

			if (!String.IsNullOrEmpty(OuterJoinRoot))
				tw.WriteAttributeString("OuterJoinRoot", OuterJoinRoot);

			if (!String.IsNullOrEmpty(ColumnLabel))
				tw.WriteAttributeString("ColumnLabel", ColumnLabel);

			if (!String.IsNullOrEmpty(Description))
				tw.WriteAttributeString("Description", Description);

			if (!String.IsNullOrEmpty(Prompt))
				tw.WriteAttributeString("Prompt", Prompt);

			if (Classification != null) Classification.Serialize(tw);
			tw.WriteEndElement();
		}

		/// <summary>
		/// Deserialize calculated field
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static CalcField Deserialize(
			string serializedForm)
		{
			if (Lex.Contains(serializedForm, "<CalcField"))
			{
				XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

				XmlTextReader tr = mstr.Reader;
				tr.Read(); // get CalcField element
				tr.MoveToContent();
				if (!Lex.Eq(tr.Name, "CalcField"))
					throw new Exception("CalcField.Deserialize - \"CalcField\" element not found");

				CalcField cf = Deserialize(mstr.Reader);
				mstr.Close();
				return cf;
			}

			return DeserializeOld(serializedForm); // must be old form
		}

		/// <summary>
		/// Deserialize CalcField from an XmlTextReader
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static CalcField Deserialize(
			XmlTextReader tr)
		{
			string mtName, mcName, txt, errMsg = "";
			MetaTable mt;
			MetaColumn mc;

			CalcField cf = new CalcField();

			XmlUtil.GetStringAttribute(tr, "Name", ref cf.UserObject.Name);

			txt = tr.GetAttribute("CalcType");
			EnumUtil.TryParse(txt, out cf.CalcType);
			if (cf.CalcType != CalcTypeEnum.Basic && cf.CalcType != CalcTypeEnum.Advanced)
				cf.CalcType = CalcTypeEnum.Basic; // default to basic type

			txt = tr.GetAttribute("SourceColumnType");
			if (txt != null) cf.SourceColumnType = MetaColumn.ParseMetaColumnTypeString(txt);

			txt = tr.GetAttribute("PreclassificationlResultType");
			if (txt != null) cf.PreclassificationlResultType = MetaColumn.ParseMetaColumnTypeString(txt);

			for (int ci = 1; ; ci++) // get the set of columns
			{
				mtName = tr.GetAttribute("Table" + ci);
				if (String.IsNullOrEmpty(mtName))
				{
					//if (ci == 1) continue; // first col may be undefined
					//else 
					if (ci > 1) break; // if beyond the first then col then all done
				}

				if (ci > cf.CfCols.Count) cf.CfCols.Add(new CalcFieldColumn());
				CalcFieldColumn cfc = cf.CfCols[ci - 1];

				mt = MetaTableCollection.Get(mtName);
				if (mt != null)
				{
					mcName = tr.GetAttribute("Column" + ci);
					if (mcName != null)
					{
						cfc.MetaColumn = mt.GetMetaColumnByName(mcName);
						if (cfc.MetaColumn == null)
							errMsg += "Unable to find column: " + mcName + " in data table " + mt.Label + "(" + mt.Name + ")\r\n";
					}
				}

				else if (ci != 1) errMsg += "Unable to find data table: " + mtName + "\r\n";

				txt = tr.GetAttribute("Function" + ci);
				if (txt != null)
					cfc.Function = txt;

				txt = tr.GetAttribute("Constant" + ci);
				if (txt != null)
					cfc.Constant = txt;
			}

			txt = tr.GetAttribute("Operation");
			if (txt != null) cf.Operation = txt;

			XmlUtil.GetStringAttribute(tr, "AdvancedExpr", ref cf.AdvancedExpr);
			XmlUtil.GetStringAttribute(tr, "OuterJoinRoot", ref cf.OuterJoinRoot);

			XmlUtil.GetStringAttribute(tr, "ColumnLabel", ref cf.ColumnLabel);
			XmlUtil.GetStringAttribute(tr, "Description", ref cf.Description);
			XmlUtil.GetStringAttribute(tr, "Prompt", ref cf.Prompt);


			if (!tr.IsEmptyElement)
			{
				tr.Read();
				tr.MoveToContent();
				if (Lex.Eq(tr.Name, "CondFormat")) // and cond format
				{
					if (!tr.IsEmptyElement)
						cf.Classification = CondFormat.Deserialize(tr);
					tr.Read(); // get next element
					tr.MoveToContent();
				}

				if (!Lex.Eq(tr.Name, "CalcField") || tr.NodeType != XmlNodeType.EndElement)
					throw new Exception("CalcField.Deserialize - Expected CalcField end element");
			}

			cf.SetDerivedValues();

			return cf;
		}

		/// <summary>
		/// Set values derived from primary values
		/// </summary>

		public void SetDerivedValues()
		{
			try
			{
				SetDerivedValuesWithException();
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
		}

		/// <summary>
		/// Set values derived from primary values
		/// </summary>

		public void SetDerivedValuesWithException()
		{
			MetaColumnType firstColumnType = MetaColumnType.Unknown;
			MetaColumnType column2Type = MetaColumnType.Unknown;

			List<MetaColumn> inputMetaColumns = GetInputMetaColumnList();

			if (SourceColumnType == MetaColumnType.Unknown)
				SourceColumnType = MetaColumnType.Number; // default type

			if (CalcType == CalcTypeEnum.Basic)
			{
				if (MetaColumn1 != null) SourceColumnType = MetaColumn1.DataType;

				if (!String.IsNullOrEmpty(Operation))
					OpEnum = ConvertCalcOpStringToEnum(ref Operation);
				else OpEnum = CalcOpEnum.None;

				for (int ci = 0; ci < CfCols.Count; ci++)
				{
					CalcFieldColumn cfc = CfCols[ci];
					SetDerivedColumnValues(SourceColumnType, cfc); // set the result type for the col

					if (ci == 0) firstColumnType = cfc.ResultColumnType;
					else if (ci == 1) column2Type = cfc.ResultColumnType;

					if (ci > 0 && cfc.MetaColumn != null &&
						MetaColumn1 != null && OpEnum != CalcOpEnum.None)
					{ // be sure column types are compatible if binary op

						if (!MetaColumn.AreCompatibleMetaColumnTypes(Column1.ResultColumnType, cfc.ResultColumnType))
							throw new Exception("Incompatible column types");

						if (MetaColumn.IsNumericMetaColumnType(MetaColumn1.DataType)) { } // all operations allowed

						else if (MetaColumn1.DataType == MetaColumnType.Date)
						{
							if (OpEnum != CalcOpEnum.Sub)
								throw new Exception("Invalid date operation");
						}

						else if (MetaColumn1.DataType == MetaColumnType.Image)
						{
							if (OpEnum != CalcOpEnum.Overlay)
								throw new Exception("Invalid Curve/Image operation");
						}

						else throw new Exception("Invalid operation");
					}
				}

				// Determine the result type from the source column types, functions, operations and mapping

				PreclassificationlResultType = MetaColumnType.Unknown;

				// Set result type before any classification is applied

				if (firstColumnType == MetaColumnType.Integer && // see if integer type output
				 (column2Type == MetaColumnType.Integer || OpEnum == CalcOpEnum.None) &&
				 (OpEnum == CalcOpEnum.None || OpEnum == CalcOpEnum.Add || OpEnum == CalcOpEnum.Sub))
					PreclassificationlResultType = MetaColumnType.Integer;

				else if (MetaColumn.IsNumericMetaColumnType(firstColumnType))
					PreclassificationlResultType = MetaColumnType.Number;

				else if (firstColumnType == MetaColumnType.Date)
				{
					if (Column1.FunctionEnum == CalcFuncEnum.None && OpEnum == CalcOpEnum.None) PreclassificationlResultType = MetaColumnType.Date;
					else PreclassificationlResultType = MetaColumnType.Integer; // either days elapsed or difference in date
				}

				else PreclassificationlResultType = firstColumnType;

				if (Classification != null) // set the type info for the classification
					Classification.ColumnType = PreclassificationlResultType;

			}

			else if (CalcType == CalcTypeEnum.Advanced) // user defines result type via input
			{
				if (PreclassificationlResultType == MetaColumnType.Unknown)
					PreclassificationlResultType = MetaColumnType.Number; // default to number
			}

			// Set final result type 

			FinalResultType = PreclassificationlResultType; // default type without classification

			if (IsClassificationDefined)
			{ // if mapping to class then class names determine the type
				FinalResultType = MetaColumnType.Integer; // start assuming integer type

				foreach (CondFormatRule rule in Classification.Rules)
				{
					if (Lex.IsInteger(rule.Name)) continue;

					else if (Lex.IsDouble(rule.Name))
						FinalResultType = MetaColumnType.Number;

					else
					{
						FinalResultType = MetaColumnType.String;
						break;
					}
				}
			}

			return;
		}

		/// <summary>
		/// Set derived values for a single calc function field
		/// </summary>
		/// <param name="sourceColumnType"></param>
		/// <param name="cfc"></param>
		void SetDerivedColumnValues(
				MetaColumnType sourceColumnType,
				CalcFieldColumn cfc)
		{
			cfc.FunctionEnum = CalcFuncEnum.Unknown;
			cfc.ConstantDouble = 0;
			cfc.ResultColumnType = MetaColumnType.Unknown;

			if (sourceColumnType == MetaColumnType.Unknown) return;

			if (!String.IsNullOrEmpty(cfc.Function))
				cfc.FunctionEnum = ConvertCalcFuncStringToEnum(cfc.Function);
			else cfc.FunctionEnum = CalcFuncEnum.None;

			double.TryParse(cfc.Constant, out cfc.ConstantDouble);

			// Get datatype after applying function

			cfc.ResultColumnType = sourceColumnType;
			if (sourceColumnType == MetaColumnType.Date)
			{
				if (cfc.FunctionEnum == CalcFuncEnum.DaysSince)
					cfc.ResultColumnType = MetaColumnType.Integer;
				else if (cfc.FunctionEnum == CalcFuncEnum.None) { }
				else throw new Exception("Invalid function for field");
			}

			else if (MetaColumn.IsNumericMetaColumnType(sourceColumnType))
			{
				if (cfc.FunctionEnum == CalcFuncEnum.DaysSince)
					throw new Exception("Invalid function for field");
			}

			else
			{
				if (cfc.FunctionEnum != CalcFuncEnum.None)
					throw new Exception("Invalid function for field");
			}
		}

		/// <summary>
		/// Parse serialized form of calculated field
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static CalcField DeserializeOld(
			string content)
		{
			MetaTable mt, mt2;
			string mtName;

			CalcField cf = new CalcField();
			CalcFieldColumn c1 = cf.Column1;
			CalcFieldColumn c2 = cf.Column2;

			string[] sa = content.Split('\t');

			cf.CalcType = CalcTypeEnum.Basic; // assume basic type

			if (sa[0] != "")
			{

				mt = MetaTableCollection.GetWithException(sa[0]);
				if (mt.SummarizedExists && !mt.UseSummarizedData)
				{ // use summarized form if exists
					mtName = mt.Name + MetaTable.SummarySuffix;
					mt2 = MetaTableCollection.Get(mtName);
					if (mt2 != null && mt2.GetMetaColumnByName(sa[1]) != null)
						mt = mt2;
				}

				c1.MetaColumn = mt.GetMetaColumnByNameWithException(sa[1]);
				if (c1.MetaColumn == null) return null;
				c1.Function = sa[2];
				c1.Constant = sa[3];

				cf.Operation = sa[4];
			}

			if (sa.Length <= 5 || // old form with only first field defined
			 (sa.Length == 6 && String.IsNullOrEmpty(sa[5])))
			{
				cf.SetDerivedValues();
				return cf;
			}

			if (sa[6] != "")
			{
				mt = MetaTableCollection.GetWithException(sa[5]);
				if (mt == null) return null;

				if (mt.SummarizedExists && !mt.UseSummarizedData)
				{ // use summarized form if exists
					mtName = mt.Name + MetaTable.SummarySuffix;
					mt2 = MetaTableCollection.Get(mtName);
					if (mt2 != null && mt2.GetMetaColumnByName(sa[6]) != null)
						mt = mt2;
				}

				c2.MetaColumn = mt.GetMetaColumnByNameWithException(sa[6]);
				if (c2.MetaColumn == null) return null;
				c2.Function = sa[7];
				c2.Constant = sa[8];
			}

			if (sa.Length >= 13) // get other values if new form
			{
				EnumUtil.TryParse(sa[9], out cf.CalcType);
				cf.AdvancedExpr = sa[10];
				string obsoleteIncludedQueryXml = sa[11]; // obsolete cf.IncludedQueryXml
				cf.Description = sa[12];
			}

			cf.SetDerivedValues();

			return cf;
		}

		/// <summary>
		/// Return searchability of calculated field
		/// </summary>

		public bool IsSearchable
		{
			get
			{
				return !IsClassificationDefined;
			}
		}

		/// <summary>
		/// Return true if calc field is mapped to class names
		/// </summary>

		public bool IsClassificationDefined
		{
			get
			{
				if (Classification != null && Classification.Rules != null && Classification.Rules.Count > 0)
					return true;
				return false;
			}
		}

		/// <summary>
		/// Convert a basic calc field to an advanced one
		/// </summary>
		/// <returns></returns>

		public string ConvertBasicToAdvanced()
		{
			string expr = "", expr1 = "", expr2 = "", tableAlias = "";

			CalcField cf = this;
			string op = GetOperator();

			//CalcOpEnum coe = CalcField.ConvertCalcOpStringToEnum(ref operation);

			foreach (CalcFieldColumn cfc in CfCols)
			{
				if (cfc.MetaColumn == null) continue;
				expr1 = BuildColumnExpr(cfc, tableAlias);

				if (cfc.Constant != "") expr1 = "(" + expr1 + ")";

				if (expr == "") expr = expr1;
				else if (cf.Operation != "")
				{
					expr += " " + op + " " + expr1;
				}
			}

			return expr;
		}

		/// <summary>
		/// Get operator for advanced form of calc field
		/// </summary>
		/// <returns></returns>

		public string GetOperator()
		{
			string op;

			CalcField cf = this;
			if (cf.OpEnum == CalcOpEnum.Overlay)
				op = "|| ' " + OverlayOp + " ' ||"; // build sql form

			else if (Lex.IsDefined(cf.Operation))
				op = cf.Operation.Substring(0, 1);

			else op = "/"; // default

			return op;
		}


		/// <summary>
		/// Convert a single basic column to advanced form
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="function"></param>
		/// <param name="constant"></param>
		/// <param name="tableAlias"></param>
		/// <returns></returns>

		public static string BuildColumnExpr(
			CalcFieldColumn cfc,
			string tableAlias)
		{
			MetaColumn mc = cfc.MetaColumn;
			string function = cfc.Function;
			string constant = cfc.Constant;

			string exp;

			if (mc == null) return "";

			CalcFuncEnum cfe = ConvertCalcFuncStringToEnum(function);

			if (cfe == CalcFuncEnum.None)
			{
				if (mc.DataType == MetaColumnType.Date)
					exp = "trunc(<v>)"; // truncate all dates
				else exp = "<v>";
			}
			else if (cfe == CalcFuncEnum.Abs)
				exp = "abs(<v>)";
			else if (cfe == CalcFuncEnum.Neg)
				exp = "-(<v>)";
			else if (cfe == CalcFuncEnum.Log)
				exp = "log(10,<v>)";
			else if (cfe == CalcFuncEnum.NegLog)
				exp = "-log(10,<v>)";
			else if (cfe == CalcFuncEnum.Ln)
				exp = "ln(<v>)";
			else if (cfe == CalcFuncEnum.NegLn)
				exp = "-ln(<v>)";
			else if (cfe == CalcFuncEnum.Sqrt)
				exp = "sqrt(<v>)";
			else if (cfe == CalcFuncEnum.Square)
				exp = "power(<v>,2)";
			else if (cfe == CalcFuncEnum.AddConst)
				exp = "(<v> +  " + constant + ")";
			else if (cfe == CalcFuncEnum.MultConst)
				exp = "(<v> *  " + constant + ")";

			else if (cfe == CalcFuncEnum.NegLogMolConc)
				exp = GetNegLogConcExpr(mc, 1);

			else if (cfe == CalcFuncEnum.DaysSince)
				exp = "(trunc(sysdate) - trunc(<v>))"; // truncate to whole days

			else throw new Exception("BuildFunctionExpression unexpected function " + function);

			string fieldRef = '"' + mc.MetaTable.Name + "." + mc.Name + '"';
			exp = exp.Replace("<v>", fieldRef);
			return exp;
		}

		/// <summary>
		/// Get -Log of concentration
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="desiredUnitsFactor">1=molar, 1000000 = uM, 1000000000 = nM</param>
		/// <returns></returns>

		static string GetNegLogConcExpr(
			MetaColumn mc,
			double desiredUnitsFactor)
		{
			double molFactor = AssayAttributes.GetMolarConcFactor(mc);
			string factorString = (desiredUnitsFactor * molFactor).ToString();
			string exp = "-log(10,<v> * " + factorString + ")";
			return exp;
		}

		/// <summary>
		/// Convert a calc function name into its corresponding enum
		/// </summary>
		/// <param name="calcFunc"></param>
		/// <returns></returns>

		public static CalcFuncEnum ConvertCalcFuncStringToEnum(
			string calcFunc)
		{
			if (String.IsNullOrEmpty(calcFunc)) return CalcFuncEnum.None;

			for (int fi = 0; fi < Funcs.Length; fi++)
			{
				if (Lex.StartsWith(Funcs[fi], calcFunc)) return (CalcFuncEnum)(fi + 1);
			}

			if (Lex.Contains(calcFunc, "(Negative value)")) return CalcFuncEnum.Neg; // old value had two spaces after minus
			if (Lex.Contains(calcFunc, "(Square)")) return CalcFuncEnum.Square;

			throw new Exception("Unrecognized function: " + calcFunc);
		}

		/// <summary>
		/// Convert a calc operation name into its corresponding enum
		/// </summary>
		/// <param name="calcOp"></param>
		/// <returns></returns>

		public static CalcOpEnum ConvertCalcOpStringToEnum(
			ref string calcOp)
		{
			if (String.IsNullOrEmpty(calcOp)) return CalcOpEnum.None;

			for (int oi = 0; oi < Ops.Length; oi++)
			{
				if (Lex.StartsWith(Ops[oi], calcOp))
				{
					calcOp = Ops[oi]; // get full string
					return (CalcOpEnum)(oi + 1);
				}
			}

			throw new Exception("Unrecognized operation: " + calcOp);
		}

		/// <summary>
		/// Get first calculated field column
		/// </summary>

		public CalcFieldColumn Column1
		{
			get
			{
				while (CfCols.Count < 1) CfCols.Add(new CalcFieldColumn());
				return CfCols[0];
			}
		}

		/// <summary>
		/// Get second calculated field column
		/// </summary>

		public CalcFieldColumn Column2
		{
			get
			{
				while (CfCols.Count < 2) CfCols.Add(new CalcFieldColumn());
				return CfCols[1];
			}
		}


		/// <summary>
		/// Get first metacolumn, if any, in the CF
		/// </summary>

		public MetaColumn MetaColumn1
		{
			get
			{
				return Column1.MetaColumn;
				//List<MetaColumn> mcList = GetMetaColumnList();
				//if (mcList.Count > 0) return mcList[0];
				//else return null;
			}
		}

		/// <summary>
		/// Get second metacolumn, if any, in the CF
		/// </summary>
		public MetaColumn MetaColumn2
		{
			get
			{
				return Column2.MetaColumn;
			}
		}

		/// <summary>
		/// Get count of input cols in CF
		/// </summary>

		public int GetInputColumnCount()
		{
			int count = GetInputMetaColumnList().Count;
			return count;
		}

		/// <summary>
		/// GetFirstInputMetaColumn
		/// </summary>
		/// <returns></returns>

		public MetaColumn GetFirstInputMetaColumn()
		{
			List<MetaColumn> mcList = GetInputMetaColumnList();
			if (mcList.Count > 0) return mcList[0];
			return null;
		}

		/// <summary>
		/// Return list of MetaColumns
		/// </summary>
		/// <returns></returns>

		public List<MetaColumn> GetInputMetaColumnList()
		{
			int mci;

			List<MetaColumn> mcList = new List<MetaColumn>();

			if (CalcType == CalcTypeEnum.Basic)
			{
				foreach (CalcFieldColumn cfc in CfCols)
				{
					if (cfc != null && cfc.MetaColumn != null)
						mcList.Add(cfc.MetaColumn);
				}
			}

			else if (CalcType == CalcTypeEnum.Advanced)
			{
				Lex lex = new Lex();
				lex.OpenString(AdvancedExpr);
				while (true)
				{
					string tok = lex.Get();
					if (tok == "") break;
					if (!Lex.IsQuoted(tok, '"')) continue;
					tok = Lex.RemoveDoubleQuotes(tok);
					MetaColumn mc = MetaColumn.ParseMetaTableMetaColumnName(tok);
					if (mc == null) continue;
					for (mci = 0; mci < mcList.Count; mci++)
						if (mcList[mci] == mc) break;

					if (mci >= mcList.Count) mcList.Add(mc);
				}
			}

			return mcList;
		}

		/// <summary>
		/// Clone the CalcField
		/// </summary>
		/// <returns></returns>

		public CalcField Clone()
		{
			string content = Serialize();
			CalcField copy = Deserialize(content);
			return copy;
		}

	} // CalcField

	/// <summary>
	/// Column of calculated fields
	/// </summary>

	public class CalcFieldColumn
	{
		public MetaColumn MetaColumn = null;
		public string Function = "";
		public CalcFuncEnum FunctionEnum = CalcFuncEnum.Unknown;
		public string Constant = "";
		public double ConstantDouble = 0;
		public MetaColumnType ResultColumnType = MetaColumnType.Unknown;

		public CalcFieldColumn()
		{
			return;
		}

		public CalcFieldColumn(MetaColumn mc) 
		{
			MetaColumn = mc;

			return;
		}

	}

	/// <summary>
	/// Type of calculation
	/// </summary>

	public enum CalcTypeEnum
	{
		Unknown = 0,
		Basic = 1,
		Advanced = 2
	}

	/// <summary>
	/// Single column functions
	/// </summary>

	public enum CalcFuncEnum
	{
		Unknown = 0,
		None = 1,
		Abs = 2,
		Neg = 3,
		Log = 4,
		NegLog = 5,
		NegLogMolConc = 6,
		//		NegLogUmConc  = xxx,
		//		NegLogNmConc  = xxx,
		Ln = 7,
		NegLn = 8,
		Sqrt = 9,
		Square = 10,
		AddConst = 11,
		MultConst = 12,
		DaysSince = 13
	}

	/// <summary>
	/// Operations between columns (must correspond to Ops array count and order)
	/// </summary>

	public enum CalcOpEnum
	{
		Unknown = 0,
		None = 1,
		Div = 2,
		Mul = 3,
		Add = 4,
		Sub = 5,
		Overlay = 6
	}

	/// <summary>
	/// MetaTable extensions for calculated fields
	/// </summary>

	public class CalcFieldMetaTable : MetaTable
	{
		public CalcField CalcField = null; // full detail on associated calculated field
	}
}
