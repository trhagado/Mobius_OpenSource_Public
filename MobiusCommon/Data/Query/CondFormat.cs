using Mobius.ComOps;
using Mobius.Data;

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
	/// Conditional Formatting representation and serialization
	/// </summary>

	public class CondFormat
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public MetaColumnType ColumnType = MetaColumnType.Unknown; // type for metacolumn used in cases where no explicit table, column
		public string Name = ""; // assigned name, allows sharing
		public bool Option1 = false; // first dataType specific formatting option
		public bool Option2 = false; // second dataType specific formatting option
		public CondFormatRules Rules = new CondFormatRules(); // associated rules

		public static string DefaultColorSet = "ColorSet1";
		public static string DefaultColorScale = "ColorScale1";
		public static Color CsfGreen = Color.FromArgb(128, 255, 128); // muted green
		public static Color CsfYellow = Color.FromArgb(255, 255, 128); // muted yellow
		public static Color CsfRed = Color.FromArgb(255, 128, 128); // muted red
		public static Color CsfUndefined = Color.FromArgb(0, 128, 255); // muted blue

		public static string PredefinedCondFormatFolderName = "PREDEFINED_CONDITIONAL_FORMAT_FOLDER";
		public static Dictionary<string, string> PredefinedDict = null; // dict of serialized cond formats we know about keyed by internal name

		public bool ShowInHeaders // return true if CF rules exist and should be shown in headers
		{
			get
			{
				if (Rules == null) return false;
				if (Rules.Count == 0 || Rules.Count > 5) return false;
				if (Rules.ColoringStyle != CondFormatStyle.ColorSet) return false;

				return true;
			}
		}


		/// <summary>
		/// Default constructor
		/// </summary>

		public CondFormat()
		{
			return;
		}

		/// <summary>
		/// Setup conditional formatting in basic Critical Success Factor format
		/// </summary>
		/// <param name="cf"></param>

		public static CondFormat BuildDefaultConditionalFormatting()
		{
			CondFormatRule r;

			CondFormat cf = new CondFormat();
			cf.Rules.ColoringStyle = CondFormatStyle.ColorSet;

			if (Bitmaps.ColorSetImageColors.ContainsKey(DefaultColorSet))
			{
				Color[] colors = Bitmaps.ColorSetImageColors[DefaultColorSet];
				if (colors.Length >= 4)
				{
					CsfGreen = colors[0];
					CsfYellow = colors[1];
					CsfRed = colors[2];
					CsfUndefined = colors[3];
				}
			}

			r = new CondFormatRule();
			r.Name = "Pass";
			r.Op = "<=";
			r.OpCode = CondFormatOpCode.Le;
			r.BackColor1 = CsfGreen;
			cf.Rules.Add(r);

			r = new CondFormatRule();
			r.Name = "BorderLine";
			r.OpCode = CondFormatOpCode.Le;
			r.Op = "<=";
			r.BackColor1 = CsfYellow;
			cf.Rules.Add(r);

			r = new CondFormatRule();
			r.Name = "Fail";
			r.OpCode = CondFormatOpCode.NotNull;
			r.Op = "Any other value";
			r.BackColor1 = CsfRed;
			cf.Rules.Add(r);

			return cf;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

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
			tw.WriteStartElement("CondFormat");

			tw.WriteAttributeString("ColumnType", ColumnType.ToString());
			tw.WriteAttributeString("Name", Name);

			//if (!ShowInHeaders) tw.WriteAttributeString("ShowInHeaders", ShowInHeaders.ToString());
			if (Option1) tw.WriteAttributeString("Option1", Option1.ToString());
			if (Option2) tw.WriteAttributeString("Option2", Option2.ToString());

			Rules.Serialize(tw);
			tw.WriteEndElement();
			return;
		}


		/// <summary>
		/// Deserialize conditional formatting.
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static CondFormat Deserialize(
			string serializedForm)
		{
			if (!Lex.Contains(serializedForm, "<Condformat ")) return DeserializeOld(serializedForm);

			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get CondFormat element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "CondFormat"))
				throw new Exception("CondFormat.Deserialize - \"CondFormat\" element not found");

			CondFormat cf = Deserialize(mstr.Reader);
			mstr.Close();
			return cf;
		}

		public static CondFormat Deserialize(
			XmlTextReader tr)
		{
			string txt;

			CondFormat cf = new CondFormat();

			txt = tr.GetAttribute("ColumnType");
			if (txt != null) EnumUtil.TryParse(txt, out cf.ColumnType);

			XmlUtil.GetStringAttribute(tr, "Name", ref cf.Name);
			XmlUtil.GetBoolAttribute(tr, "Option1", ref cf.Option1);
			XmlUtil.GetBoolAttribute(tr, "Option2", ref cf.Option2);
			//XmlUtil.GetBoolAttribute(tr, "ShowInHeaders", ref cf.ShowInHeaders);

			tr.Read(); // get CondFormatRules element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "CondFormatRules"))
				throw new Exception("CondFormat.Deserialize - \"CondFormat\" end element not found");

			if (!tr.IsEmptyElement)
			{
				cf.Rules = CondFormatRules.Deserialize(tr);
				cf.Rules.InitializeInternalMatchValues(cf.ColumnType);
			}

			else cf.Rules = new CondFormatRules(); // no rules

			tr.Read(); // get CondFormat end element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "CondFormat") || tr.NodeType != XmlNodeType.EndElement)
				throw new Exception("CondFormat.Deserialize - Expected CondFormat end element");

			if (cf.ColumnType == MetaColumnType.Date && cf.Rules != null)
			{ // store normalized dates
				foreach (CondFormatRule rule in cf.Rules)
				{
					if (!String.IsNullOrEmpty(rule.Value))
						rule.ValueNormalized = DateTimeMx.Normalize(rule.Value);

					if (!String.IsNullOrEmpty(rule.Value2))
						rule.Value2Normalized = DateTimeMx.Normalize(rule.Value2);
				}
			}

			return cf;
		}

		/// <summary>
		/// Deserialize old format conditional formatting.
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static CondFormat DeserializeOld(
			string serializedForm)
		{
			CondFormat cf = new CondFormat();

			string[] sa = serializedForm.Split('\n');
			string[] sa2 = sa[0].Split('\t');
			//			cf.IsCSF = Boolean.Parse(sa2[0]); // (obsolete)
			if (sa2.Length >= 3)
			{
				MetaTable mt = MetaTableCollection.Get(sa2[1]);
				if (mt == null) return null;
				MetaColumn mc = mt.GetMetaColumnByName(sa2[2]);
				if (mc != null) cf.ColumnType = mc.DataType;
			}

			if (sa.Length > 1 && sa[1] != "")
				cf.Rules.Add(CondFormatRules.DeserializeOld(sa[1]));
			if (sa.Length > 2 && sa[2] != "")
				cf.Rules.Add(CondFormatRules.DeserializeOld(sa[2]));
			if (sa.Length > 3 && sa[3] != "")
				cf.Rules.Add(CondFormatRules.DeserializeOld(sa[3]));
			if (sa.Length > 4 && sa[4] != "")
				cf.Rules.Add(CondFormatRules.DeserializeOld(sa[4]));

			return cf;
		}

		/// <summary>
		/// Return true if conditional formatting is allowed on the specified column
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		public static bool CondFormattingAllowed(
			MetaColumnType mcType)
		{
			if (mcType == MetaColumnType.Image ||
			 mcType == MetaColumnType.Unknown)
				return false;

			else return true;
		}

		/// <summary>
		/// Lookup predefined conditional formatting by name
		/// </summary>
		/// <param name="cfName"></param>
		/// <returns></returns>

		public static CondFormat GetPredefined(string cfName)
		{
			CondFormat cf = null;

			cfName = cfName.ToUpper();

			if (Lex.Eq(cfName, "Activity Class Conditional Formatting") || Lex.Eq(cfName, "ActivityClassCondFormat")) // special internal format
			{
				cf = UnpivotedAssayResult.BuildActivityClassCondFormat();
				return cf;
			}

			else if (Lex.Eq(cfName, "Activity Bin Conditional Formatting") || Lex.Eq(cfName, "ActivityBinCondFormat")) // special internal format
			{
				cf = UnpivotedAssayResult.BuildActivityBinCondFormat();
				return cf;
			}

			else if (Lex.StartsWith(cfName, "CONDFORMAT_"))
			{
				if (PredefinedDict == null) // need to build?
				{
					if (InterfaceRefs.IUserObjectTree == null) return null;
					List<UserObject> uoList = InterfaceRefs.IUserObjectTree.GetUserObjectsByType(UserObjectType.CondFormat);
					if (uoList == null) return null;

					PredefinedDict = new Dictionary<string, string>();
					foreach (UserObject uo0 in uoList)
					{
						PredefinedDict[uo0.InternalName.ToUpper()] = uo0.Content;
					}

				}

				if (!PredefinedDict.ContainsKey(cfName)) return null;

				cf = CondFormat.Deserialize(PredefinedDict[cfName]);
				return cf;
			}

			else return null;
		}

		/// <summary>
		/// Add or update a predefined cf
		/// </summary>
		/// <param name="cfName"></param>
		/// <param name="cf"></param>

		public static void UpdatePredefined(
			string cfName,
			CondFormat cf)
		{
			cfName = cfName.ToUpper();
			CondFormat cf2 = GetPredefined(cfName); // be sure dict has been build

			string content = cf.Serialize();
			PredefinedDict[cfName] = content;
			return;
		}

		/// <summary>
		/// Clone cf
		/// </summary>
		/// <returns></returns>

		public CondFormat Clone()
		{
			string serializedForm = this.Serialize();
			CondFormat clone = CondFormat.Deserialize(serializedForm);
			return clone;
		}
	}


	/// <summary>
	/// List of CondFormatRule objects
	/// </summary>

	public class CondFormatRules : List<CondFormatRule>
	{
		public CondFormatStyle ColoringStyle = CondFormatStyle.ColorSet;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public CondFormatRules()
		{
			return;
		}

		/// <summary>
		/// Construct defining coloring style
		/// </summary>
		/// <param name="coloringStyle"></param>

		public CondFormatRules(CondFormatStyle coloringStyle)
		{
			ColoringStyle = coloringStyle;
			return;
		}

		/// <summary>
		/// Initialize internal match values for a list of rules single rule
		/// </summary>
		/// <param name="columnType"></param>

		public void InitializeInternalMatchValues(MetaColumnType columnType)
		{
			foreach (CondFormatRule rule in this)
			{
				rule.InitializeInternalMatchValues(columnType);
			}
			return;
		}

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			Serialize(mstw.Writer);
			return mstw.GetXmlAndClose();
		}

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("CondFormatRules");

			if (ColoringStyle == CondFormatStyle.ColorScale)
				tw.WriteAttributeString("ColorContinuously", true.ToString());

			else if (ColoringStyle == CondFormatStyle.DataBar)
				tw.WriteAttributeString("ColorAsDataBar", true.ToString());

			else if (ColoringStyle == CondFormatStyle.IconSet)
				tw.WriteAttributeString("ColorAsIconset", true.ToString());

			else tw.WriteAttributeString("ColorDiscretely", true.ToString());

			for (int ri = 0; ri < this.Count; ri++)
			{
				tw.WriteStartElement("CondFormatRule");
				CondFormatRule r = this[ri];
				tw.WriteAttributeString("Name", r.Name);
				tw.WriteAttributeString("Op", r.Op);
				tw.WriteAttributeString("Value", r.Value);
				tw.WriteAttributeString("Value2", r.Value2);
				//if (r.Font != null) tw.WriteAttributeString("FontName", r.Font.FontFamily.Name);
				//if (r.Font != null) tw.WriteAttributeString("FontSize", r.Font.Size.ToString());
				//if (r.Font != null) tw.WriteAttributeString("FontStyle", ((int)r.Font.Style).ToString());
				tw.WriteAttributeString("ForeColor", r.ForeColor.ToArgb().ToString());
				tw.WriteAttributeString("BackColor", r.BackColor1.ToArgb().ToString());
				if (Lex.IsDefined(r.ImageName))
					tw.WriteAttributeString("ImageName", r.ImageName);

				tw.WriteEndElement();
			}
			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize conditional formatting rules.
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static CondFormatRules Deserialize(
			string serializedForm)
		{
			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read(); // get CondFormatRules element
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "CondFormatRules"))
				throw new Exception("CondFormatRules.Deserialize - No \"CondFormatRules\" element found");

			if (tr.IsEmptyElement) return new CondFormatRules(); // if nothing there return empty rule list

			CondFormatRules rules = Deserialize(mstr.Reader);
			mstr.Close();
			return rules;
		}

		public static CondFormatRules Deserialize(
			XmlTextReader tr)
		{
			string txt;

			CondFormatRules rules = new CondFormatRules();

			if (Lex.Eq(XmlUtil.GetAttribute(tr, "ColorContinuously"), "true"))
				rules.ColoringStyle = CondFormatStyle.ColorScale;

			else if (Lex.Eq(XmlUtil.GetAttribute(tr, "ColorAsDataBar"), "true"))
				rules.ColoringStyle = CondFormatStyle.DataBar;

			else if (Lex.Eq(XmlUtil.GetAttribute(tr, "ColorAsIconSet"), "true"))
				rules.ColoringStyle = CondFormatStyle.IconSet;

			else rules.ColoringStyle = CondFormatStyle.ColorSet;

			if (tr.IsEmptyElement) // all done if no rules
				return rules;

			while (true) // loop on list of rules
			{
				tr.Read(); // move to next CondFormatRule
				tr.MoveToContent();

				if (Lex.Eq(tr.Name, "CondFormatRule")) // start or end of rule
				{
					if (tr.NodeType == XmlNodeType.EndElement) continue; // end of current rule
				}

				else if (tr.NodeType == XmlNodeType.EndElement) break; // done with all rules

				else throw new Exception("CondFormatRules.Deserialize - Unexpected element: " + tr.Name);

				CondFormatRule r = new CondFormatRule();
				rules.Add(r);

				txt = tr.GetAttribute("Name");
				if (txt != null) r.Name = txt;

				txt = tr.GetAttribute("Op");
				if (txt != null)
				{
					r.Op = txt;
					r.OpCode = CondFormatRule.ConvertOpNameToCode(r.Op);
				}

				txt = tr.GetAttribute("Value");
				if (txt != null)
				{
					r.Value = txt;
					double.TryParse(txt, out r.ValueNumber);
				}

				txt = tr.GetAttribute("Value2");
				if (txt != null)
				{
					r.Value2 = txt;
					double.TryParse(txt, out r.Value2Number);
				}

				string fontName = tr.GetAttribute("FontName");
				string fontSizeTxt = tr.GetAttribute("FontSize");
				string fontStyleTxt = tr.GetAttribute("FontStyle");

				float fontsize;
				int fontStyle;

				if (fontName != null && fontSizeTxt != null && fontStyleTxt != null &&
					float.TryParse(fontSizeTxt, out fontsize) && int.TryParse(fontStyleTxt, out fontStyle))
				{
					r.Font = new Font(fontName, fontsize, (FontStyle)fontStyle);
				}

				txt = tr.GetAttribute("ForeColor");
				if (txt != null) r.ForeColor = Color.FromArgb(int.Parse(txt));

				txt = tr.GetAttribute("BackColor");
				if (txt != null) r.BackColor1 = Color.FromArgb(int.Parse(txt));

				txt = tr.GetAttribute("ImageName");
				if (txt != null) r.ImageName = txt;
			}

			return rules;
		}

		/// <summary>
		/// Deserialize old form of rule
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static CondFormatRule DeserializeOld(
			string serializedForm)
		{
			CondFormatRule rule = new CondFormatRule();
			string[] sa = serializedForm.Split('\t');

			rule.Name = sa[0];

			rule.Op = sa[1];
			rule.OpCode = CondFormatRule.ConvertOpNameToCode(rule.Op);

			rule.Value = sa[2];
			try { rule.ValueNumber = Double.Parse(rule.Value); }
			catch (Exception ex) { }

			rule.Value2 = sa[3];
			try { rule.Value2Number = Double.Parse(rule.Value2); }
			catch (Exception ex) { }

			rule.Font = new Font(sa[4], Single.Parse(sa[5]), (FontStyle)Int32.Parse(sa[6]));
			rule.ForeColor = Color.FromArgb(Int32.Parse(sa[7]));
			rule.BackColor1 = Color.FromArgb(Int32.Parse(sa[8]));

			return rule;
		}
	}

	/// <summary>
	/// A single conditional formatting rule
	/// </summary>

	public class CondFormatRule
	{
		public string Name = ""; // name of rule
		public string Op = ""; // operation
		public CondFormatOpCode OpCode;
		public string Value = ""; // external string form of value
		public string ValueNormalized = null; // for holding normalized values (e.g. normalized dates)
		public double ValueNumber; // number form of value
		public Dictionary<string, object> ValueDict = null; // dictionay of list values
		public string Value2 = "";
		public string Value2Normalized = null;
		public double Value2Number;
		public double Epsilon = 0; // fuzz factor for decimal comparisons, usually .5 * 10**(-decimals)
		public Font Font = null;
		public Color ForeColor = Color.Black;
		public Color BackColor1 = Color.White;
		public Color BackColor2 = Color.White; 
		public Color BackColor3 = Color.White;
		public string ImageName = ""; // name of image to display if rule matches

		/// <summary>
		/// Default constructor
		/// </summary>

		public CondFormatRule()
		{
			return;
		}

		/// <summary>
		/// Construct with basic values
		/// </summary>

		public CondFormatRule(
			string name,
			CondFormatOpCode opCode,
			string value)
		{
			Name = name;
			OpCode = opCode;
			Op = ConvertOpCodeToName(opCode);
			Value = value;
			return;
		}

		/// <summary>
		/// Initialize internal match values for a single rule
		/// </summary>
		/// <param name="columnType"></param>

		public void InitializeInternalMatchValues(MetaColumnType columnType)
		{
			OpCode = ConvertOpNameToCode(Op);

			bool calculateEpsilonFromCfValue = false; // if true use cf value (note: may not be same number of decimals as output format)
			Epsilon = 0;

			if (MetaColumn.IsNumericMetaColumnType(columnType) && !String.IsNullOrEmpty(Value))
			{
				double.TryParse(Value, out ValueNumber);

				if (calculateEpsilonFromCfValue)
					Epsilon = MobiusDataType.GetEpsilon(Value);

				else
				{
					int decimals = 10; // use default epsilon value
					Epsilon = MobiusDataType.GetEpsilon(decimals);
				}
			}

			else if (columnType == MetaColumnType.Date && !String.IsNullOrEmpty(Value))
				ValueNormalized = DateTimeMx.Normalize(Value);

			if (MetaColumn.IsNumericMetaColumnType(columnType) && !String.IsNullOrEmpty(Value2))
			{
				double.TryParse(Value2, out Value2Number);
				double e2 = MobiusDataType.GetEpsilon(Value2);
				if (e2 < Epsilon) Epsilon = e2;
			}
			else if (columnType == MetaColumnType.Date && !String.IsNullOrEmpty(Value2))
				Value2Normalized = DateTimeMx.Normalize(Value2);
		}

		/// <summary>
		/// Translate op name to code
		/// </summary>
		/// <param name="opName"></param>
		/// <returns></returns>

		public static CondFormatOpCode ConvertOpNameToCode(
			string opName)
		{
			CondFormatOpCode opCode = CondFormatOpCode.Unknown;

			if (Lex.StartsWith(opName, "Unknown")) opCode = CondFormatOpCode.Unknown;
			else if (Lex.StartsWith(opName, "Between")) opCode = CondFormatOpCode.Between;
			else if (Lex.StartsWith(opName, "Not Between")) opCode = CondFormatOpCode.NotBetween;
			else if (Lex.StartsWith(opName, "Equal to")) opCode = CondFormatOpCode.Eq;
			else if (Lex.StartsWith(opName, "Not Equal to")) opCode = CondFormatOpCode.NotEq;
			else if (Lex.StartsWith(opName, ">=")) opCode = CondFormatOpCode.Ge; // check before >
			else if (Lex.StartsWith(opName, ">")) opCode = CondFormatOpCode.Gt;
			else if (Lex.StartsWith(opName, "<=")) opCode = CondFormatOpCode.Le; // check before <
			else if (Lex.StartsWith(opName, "<")) opCode = CondFormatOpCode.Lt;
			else if (Lex.StartsWith(opName, "Contains Substring")) opCode = CondFormatOpCode.Substring;
			else if (Lex.StartsWith(opName, "Within")) opCode = CondFormatOpCode.Within;
			else if (Lex.StartsWith(opName, "SSS")) opCode = CondFormatOpCode.SSS;
			else if (Lex.StartsWith(opName, "Exists")) opCode = CondFormatOpCode.Exists;
			else if (Lex.StartsWith(opName, "Not Exists")) opCode = CondFormatOpCode.NotExists;
			else if (Lex.StartsWith(opName, "Missing")) opCode = CondFormatOpCode.Null;
			else if (Lex.StartsWith(opName, "Not Missing") || Lex.StartsWith(opName, "Any other value")) opCode = CondFormatOpCode.NotNull;

			return opCode;
		}

		/// <summary>
		/// ConvertOpCodeToName
		/// </summary>
		/// <param name="opCode"></param>
		/// <returns></returns>

		public static string ConvertOpCodeToName(
			CondFormatOpCode opCode)
		{
			if (opCode == CondFormatOpCode.Unknown) return "Unknown";
			else if (opCode == CondFormatOpCode.Between) return "Between";
			else if (opCode == CondFormatOpCode.NotBetween) return "Not Between";
			else if (opCode == CondFormatOpCode.Eq) return "Equal to";
			else if (opCode == CondFormatOpCode.NotEq) return "Not Equal to";
			else if (opCode == CondFormatOpCode.Ge) return ">=";
			else if (opCode == CondFormatOpCode.Gt) return ">";
			else if (opCode == CondFormatOpCode.Le) return "<=";
			else if (opCode == CondFormatOpCode.Lt) return "<";
			else if (opCode == CondFormatOpCode.Substring) return "Contains Substring";
			else if (opCode == CondFormatOpCode.Within) return "Within";
			else if (opCode == CondFormatOpCode.SSS) return "SSS";
			else if (opCode == CondFormatOpCode.Exists) return "Exists";
			else if (opCode == CondFormatOpCode.NotExists) return "Not Exists";
			else if (opCode == CondFormatOpCode.Null) return "Missing";
			else if (opCode == CondFormatOpCode.NotNull) return "Not Missing";
			else throw new Exception("Invalid OpCode: " + opCode);
		}

		/// <summary>
		/// Format the string representation of the rule
		/// </summary>
		/// <param name="rulei"></param>
		/// <param name="cf"></param>
		/// <param name="mc"></param>
		/// <returns></returns>

		public string ToString(
			MetaColumn mc,
			int rulei)
		{
			string txt;

			string value = Value;
			string value2 = Value2;

			if (mc.DataType == MetaColumnType.Structure)
			{ // for structure type use rule name or number in header
				if (!String.IsNullOrEmpty(Name))
					txt = Name;
				else
				{
					txt = "Rule";
					if (rulei >= 0) txt += " " + (rulei + 1);
				}
			}

			else // for other types use the condition in the header
			{

				if (mc.DataType == MetaColumnType.String)
				{
					value = Lex.Dq(value);
					value2 = Lex.Dq(value2);
				}

				if (Lex.Eq(Op, "Between") || Lex.Eq(Op, "Between (Inclusive)"))
					txt = value + " - " + value2;

				else if (Lex.Eq(Op, "Not Between"))
					txt = "Not " + value + " - " + value2;

				else if (Lex.Eq(Op, "Equal to"))
					txt = "= " + value;

				else if (Lex.Eq(Op, "Not Equal to"))
					txt = "Not = " + value;

				else if (Lex.Eq(Op, "Within the Last"))
					txt = "Within " + value + " " + value2;

				else txt = Op + " " + value;
			}

			return txt;
		}

		/// <summary>
		/// Clone the rule
		/// </summary>
		/// <returns></returns>

		public CondFormatRule Clone()
		{
			return (CondFormatRule)this.MemberwiseClone();
		}
	}

	/// <summary>
	/// Coloring style for cond formatting
	/// </summary>

	public enum CondFormatStyle
	{
		Undefined = 0,
		ColorSet = 1,
		ColorScale = 2,
		DataBar = 3,
		IconSet = 4
	}


	/// <summary>
	/// Cond format operation enumeration
	/// </summary>

	public enum CondFormatOpCode
	{
		Unknown = 0,
		Between = 1,
		NotBetween = 2,
		Eq = 3,
		NotEq = 4,
		Gt = 5,
		Lt = 6,
		Ge = 7,
		Le = 8,
		Null = 9,
		NotNull = 10,
		Exists = 11,
		Substring = 12,
		Within = 13,
		SSS = 14,
		NotExists = 15
	}

}
