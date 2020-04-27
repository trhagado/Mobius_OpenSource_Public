using Mobius.ComOps;

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Mobius.Data
{

	/// <summary>
	/// A QualifiedNumber represents a number with extended attributes
	/// </summary>

	[DataContract]
	public class QualifiedNumber : MobiusDataType, IComparable, IConvertible
	{
		public static QualifiedNumber QnModel = new QualifiedNumber();
		public static QnExtendedAttributes AttrExModel = new QnExtendedAttributes();

/// <summary>
/// Number qualifier (>, < etc.)
/// </summary>
		
		public string Qualifier
		{
			get
			{
				if (_qualifier == ' ') return "";
				else if (_qualifier2 == ' ') return _qualifier.ToString();
				else return _qualifier.ToString() + _qualifier2.ToString();
			}

			set
			{
				_qualifier = _qualifier2 = ' ';
				if (Lex.IsNullOrEmpty(value)) return;
				else 
				{
					_qualifier = value[0]; // store first char
					if (value.Length == 2) // 2nd char?
						_qualifier2 = value[1];
					else if (value.Length > 2) // too long
						throw new Exception("Qualifier longer than two characters: \"" + value + "\"");
				}
			}
		}
		protected char _qualifier = ' '; // first char
		protected char _qualifier2 = ' '; // 2nd char

/// <summary>
/// // low or only numeric value
/// </summary>

		public double NumberValue
		{
			set { _numberValue = value; }
			get { return _numberValue; }
		}
		protected double _numberValue = NullNumber; 

/// <summary>
/// Extended attributes
/// Significant memory is saved if these are not set to non-default values
/// which avoids the allocation of an QnExtendedAttributes object
/// </summary>

		QnExtendedAttributes QnExAttr = null; // any non-default attribute values associated with this Qn instance (not normally referenced)

		/// <summary>
		/// Return reference for getting attribute values, do not use for setting values
		/// </summary>

		QnExtendedAttributes QnExGetRef
		{
			get
			{
				if (QnExAttr != null) return QnExAttr; // if defined return it
				else return AttrExModel; // if not defined return default value object
			}
		}

		/// <summary>
		/// Get reference that allows setting of value by allocating if not already done
		/// </summary>

		QnExtendedAttributes QnExSetRef // reference for setting attribute values
		{
			get
			{
				if (QnExAttr == null) QnExAttr = new QnExtendedAttributes();
				return QnExAttr;
			}
		}

/// <summary>
/// Standard deviation of underlying values
/// </summary>

		public double StandardDeviation
		{
			set
			{
				if (QnExGetRef.StandardDeviation == value) return;
				QnExSetRef.StandardDeviation = value;
			}
			get { return QnExGetRef.StandardDeviation; }
		}		

/// <summary>
/// Standard error of underlying values
/// </summary>

		public double StandardError
		{
			set
			{
				if (QnExGetRef.StandardError == value) return;
				QnExSetRef.StandardError = value;
			}
			get { return QnExGetRef.StandardError; }
		}

/// <summary>
/// Number of values summarized in stats
/// </summary>

		public int NValue
		{
			set
			{
				if (QnExGetRef.NValue == value) return;
				QnExSetRef.NValue = value;
			}
			get { return QnExGetRef.NValue; }
		}		

/// <summary>
/// Number of tests results, may be greater than NValue
/// </summary>

		public int NValueTested
		{
			set
			{
				if (QnExGetRef.NValueTested == value) return;
				QnExSetRef.NValueTested = value;
			}
			get { return QnExGetRef.NValueTested; }
		}		

/// <summary>
/// Text value if not a number
/// </summary>

		public string TextValue
		{
			set
			{
				if (QnExGetRef.TextValue == value) return;
				QnExSetRef.TextValue = value;
			}
			get { return QnExGetRef.TextValue; }
		}	

/// <summary>
/// Constant values
/// </summary>

		public static ColumnFormatEnum DefaultNumberFormat = ColumnFormatEnum.SigDigits;
		public static int DefaultDecimals = 3;
		public static string NullValueString = " "; // string used to display missing values
		public const int NullNumber = -4194303; // "special" value -(2**22 - 1) used for null numeric values

		static QualifiedNumberTextElements StaticQnte = new QualifiedNumberTextElements(); // used to reduce TryParse mem alloc calls

/// <summary>
/// Default constructor
/// </summary>

		public QualifiedNumber()
		{
			return;
		}

/// <summary>
/// General constructor from an object
/// </summary>
/// <param name="o"></param>

		public QualifiedNumber(
			object o)
		{
			if (NullValue.IsNull(o)) return;

			if (o is int) NumberValue = (int)o;
			else if (o is long) NumberValue = (long)o;
			else if (o is float) NumberValue = (float)o;
			else if (o is double) NumberValue = (double)o;
			else if (o is decimal) NumberValue = decimal.ToDouble((decimal)o);

			else if (o is NumberMx) NumberValue = ((NumberMx)o).NumericValue;

			else if (o is StringMx) TextValue = ((StringMx)o).Value;

			else if (o is string)
			{
				if (!TryParse((string)o, this))
					throw new Exception("Invalid qualified number: " + (string)o);
			}

			else throw new InvalidCastException(o.GetType().ToString());
		}

/// <summary>
/// Convert the object to q QualifiedNumber
/// </summary>
/// <param name="o"></param>
/// <param name="qn"></param>
/// <returns></returns>

		public static bool TryConvertTo(
			object o,
			out QualifiedNumber qn)
		{
			qn = null;

			try
			{
				qn = ConvertTo(o);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
			
		/// <summary>
		/// Convert the object to a QualifiedNumber throwing an exception if can't be converted
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static QualifiedNumber ConvertTo(
			object o)
		{
			if (o is QualifiedNumber) return (QualifiedNumber)o;
			else return new QualifiedNumber(o);
		}

		/// <summary>
		/// Get hash code for this MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			if (NumberValue != NullNumber || Lex.IsUndefined(TextValue))
				return NumberValue.GetHashCode();
			else return TextValue.GetHashCode();
		}

		/// <summary>
		/// Compare this MobiusDataType to another for equality 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public override bool Equals(object obj)
		{
			return (CompareTo(obj) == 0);
		}

		/// <summary>
		/// Compare two qualified numbers (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public override int CompareTo (
			object o)
		{
			double compVal = NullNumber;

			if (o == null) compVal = NullNumber;

			else if (o is int)
				compVal = (int)o;

			else if (o is double)
				compVal = (double)o;

			else if (o is StringMx)
			{
				StringMx sx = o as StringMx;
				string s = this.TextValue;
				if (s == null) s = "";
				return s.CompareTo(sx.Value);
			}

			else if (o is MobiusDataType)
				compVal = ((MobiusDataType)o).NumericValue;

			else throw new Exception("Can't compare a " + GetType().Name + " to a " + o.GetType());

			if (this.NumberValue != NullNumber && compVal != NullNumber) // both not null 
				return this.NumberValue.CompareTo(compVal);

			else if (this.NumberValue == NullNumber && compVal == NullNumber) // both null 
			{
				if (o is QualifiedNumber) // if object is QN compare any string values ignoring case
					return String.Compare(this.TextValue, ((QualifiedNumber)o).TextValue, true); 
				else return 0; // say equal otherwise
			}

			else if (this.NumberValue != NullNumber) // this not null, compValue is null
				return -1; // indicate this is less (put non-null first)

			else // compValue not null, this is null
				return 1; // indicate that this is greater (put non-null first)
		}

		/// <summary>
		/// Return true if qualified number is null
		/// </summary>
		/// <returns></returns>

		public override bool IsNull
		{
			get
			{
				if (NumberValue == NullNumber &&
					NValue == NullNumber &&
					NValueTested == NullNumber &&
					(String.IsNullOrEmpty(TextValue) || TextValue.Trim().Length == 0))
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Check if value is an integer
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>

		[XmlIgnore]
		public bool IsInteger
		{
			get
			{
				return unchecked(NumberValue == (int)NumberValue) && // unchecked avoids overflow exceptions for very large integers
						NumberValue != QualifiedNumber.NullNumber &&
						Qualifier != "";
			}
		}


		/// <summary>
		/// Get/set numeric value
		/// </summary>

		public override double NumericValue
		{
			get
			{
				return NumberValue;
			}

			set
			{
				NumberValue = value;
				Qualifier = "";
			}
		}

		/// <summary>
		/// Get/set DateTime value (stored as text in TextValue)
		/// </summary>

		public override DateTime DateTimeValue
		{
			get
			{
				if (Lex.IsNullOrEmpty(TextValue))
					return DateTime.MinValue;
				else
				{
					DateTime dt = DateTimeMx.NormalizedToDateTime(TextValue);
					return dt;
				}
			}

			set
			{
				if (value == DateTime.MinValue)
					TextValue = "";
				else TextValue = DateTimeMx.Normalize(value);

			}
		}

		/// <summary>
		/// Convert the value to the nearest primitive type
		/// </summary>
		/// <returns></returns>

		public override object ToPrimitiveType()
		{
			if (NumberValue == NullValue.NullNumber) return null;
			else return NumberValue;
		}

		/// <summary>
		/// Return the internal normalized string version of the MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override string FormatForCriteria()
		{
			return NumberValue.ToString();
		}

		/// <summary>
		/// Return a formatted string for the MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override string FormatCriteriaForDisplay()
		{
			return NumberValue.ToString();
		}

		/// <summary>
		/// Format qualified number when destination device not known
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="mergedField"></param>
		/// <param name="displayFormat"></param>
		/// <param name="decimals"></param>
		/// <param name="qnFormat"></param>
		/// <param name="includeLink"></param>
		/// <returns></returns>

		public string Format(
			QueryColumn qc,
			bool mergedField,
			ColumnFormatEnum displayFormat,
			int decimals,
			QnfEnum qnFormat,
			bool includeLink)
		{
			OutputDest outputDest;
			if (includeLink) outputDest = OutputDest.WinForms;
			else outputDest = OutputDest.TextFile;

			return Format(qc, mergedField, displayFormat, decimals, qnFormat, outputDest);
		}

		/// <summary>
		/// Format qualified number
		/// </summary>
		/// <param name="qn"></param>
		/// <param name="qc"></param>
		/// <param name="mergedField"></param>
		/// <param name="displayFormat"></param>
		/// <param name="decimals"></param>
		/// <param name="qnFormat"></param>
		/// <param name="outputDest"></param>
		/// <returns></returns>

		public string Format(
			QueryColumn qc,
			bool mergedField,
			ColumnFormatEnum displayFormat,
			int decimals,
			QnfEnum qnFormat,
			OutputDest outputDest)
		{
			string result, uri, href, txt, tok;

			result = "";

			QualifiedNumber qn = this;

			MetaColumn mc = qc.MetaColumn;
			if (qn.Qualifier == null) qn.Qualifier = "";
			if (qn.TextValue == null) qn.TextValue = "";
			if (qn.Hyperlink == null) qn.Hyperlink = "";

			if (QnSubcolumns.IsCombinedFormat(qnFormat)) // normal combined number
			{
				if (qn.NumberValue == NullValue.NullNumber && qn.NValue == NullValue.NullNumber &&
					qn.NValueTested == NullValue.NullNumber && qn.TextValue == "")
				{ // null value
					result = NullValueString;
					if (outputDest == OutputDest.Html && (NullValueString == "" || NullValueString == " "))
						result = "<br>"; // html blank
					return result;
				}

				result = qn.Format(displayFormat, decimals);

				if (qn.NValue > 1 || qn.NValueTested > 1 || // format with sd, n 
					(qn.NValue >= 0 && qn.NValueTested > qn.NValue) || // number tested > n
					(qn.NumberValue == NullValue.NullNumber && // also null numbers if some nonzero nvalue
					(qn.NValue > 0 || qn.NValueTested > 0)))
				{
					txt = "";

					if (qn.NValue > 1 && (qnFormat & QnfEnum.StdDev) != 0 && // include sd
						(qn.StandardDeviation != NullValue.NullNumber ||
						qn.StandardError != NullValue.NullNumber))
					{ // format standard deviation
						if ((qnFormat & QnfEnum.DisplayStdDevLabel) != 0)
							txt += "sd=";

						if (qn.StandardDeviation == NullValue.NullNumber) // calc sd from se if don't have
							qn.StandardDeviation = qn.StandardError * Math.Sqrt(qn.NValue);
						txt += FormatNumber(qn.StandardDeviation, displayFormat, decimals);
					}

					if (qn.NValue > 1 && (qnFormat & QnfEnum.StdErr) != 0 && // include se
						(qn.StandardError != NullValue.NullNumber ||
						qn.StandardDeviation != NullValue.NullNumber))
					{ // format standard error
						if (txt != "") txt += ", ";
						if ((qnFormat & QnfEnum.DisplayStdErrLabel) != 0)
							txt += "se=";

						txt += FormatNumber(qn.StandardError, displayFormat, decimals);
					}

					if (qn.NValue != NullValue.NullNumber &&
						(qnFormat & QnfEnum.NValue) != 0)
					{ // format n value
						if (txt != "") txt += ", ";
						if ((qnFormat & QnfEnum.DisplayNLabel) != 0)
							txt += "n=";

						txt += qn.NValue.ToString();
						if (qn.NValueTested != NullValue.NullNumber && qn.NValueTested != qn.NValue)
							txt += "/" + qn.NValueTested.ToString(); // add number tested if different
					}
					if (txt != "") result += " (" + txt + ")"; // build complete string
				}

				if (qn.DbLink != null && qn.DbLink != "" && qn.DbLink != "." && !mergedField && // do we have an associated resultId
					(outputDest == OutputDest.WinForms || outputDest == OutputDest.Html))
				{
					uri = qn.FormatHyperlink(qc);

					//if (outputDest == OutputDest.Grid) // store link info in separate field
					//  qn.Hyperlink = uri;

					//else if (outputDest == OutputDest.Html) // build full html tag
					//{
					if (uri != "")
						result = result = "<a href=" + Lex.Dq(uri) + ">" + result + "</a>";
					//}
				}
			}

// Format one piece of a split qualified number

			else if ((qnFormat & QnfEnum.Qualifier) != 0) // qualifier
			{
				result = qn.Qualifier;
			}

			else if ((qnFormat & QnfEnum.NumericValue) != 0) // basic number
			{
				if (qn.NumberValue != NullValue.NullNumber)
					result = FormatNumber(qn.NumberValue, displayFormat, decimals);

				else result = null;

				//else (obsolete, don't output any text values into numeric column, add option for separate column later)
				//{
				//  if (qn.NValue != NullValue.NullNumber)
				//    result = "ND"; // some NValue, return not determined

				//  else result = qn.TextValue; // return any text value
				//}
			}

			else if ((qnFormat & QnfEnum.StdDev) != 0) // standard deviation
			{
				if (qn.StandardDeviation != NullValue.NullNumber)
					result = FormatNumber(qn.StandardDeviation, displayFormat, decimals);
			}

			else if ((qnFormat & QnfEnum.StdErr) != 0) // standard error
			{
				if (qn.StandardError != NullValue.NullNumber)
					result = FormatNumber(qn.StandardError, displayFormat, decimals);
			}

			else if ((qnFormat & QnfEnum.NValue) != 0) // N 
			{
				if (qn.NValue != NullValue.NullNumber)
					result = qn.NValue.ToString();
			}

			else if ((qnFormat & QnfEnum.NValueTested) != 0) // number tested
			{
				if (qn.NValueTested != NullValue.NullNumber)
					result = qn.NValueTested.ToString();
			}

			else if ((qnFormat & QnfEnum.TextValue) != 0) // text value
			{
				if (!Lex.IsNullOrEmpty(qn.TextValue))
					result = qn.TextValue;
			}

			return result;
		}

		/// <summary>
		/// Format a basic qualified number with qualifier and value
		/// </summary>
		/// <param name="qn"></param>
		/// <param name="displayFormat"></param>
		/// <param name="decimals"></param>
		/// <returns></returns>

		public string Format(
			ColumnFormatEnum displayFormat,
			int decimals)
		{
			string result;

			QualifiedNumber qn = this;
			if (qn.Qualifier == null) qn.Qualifier = "";
			if (qn.TextValue == null) qn.TextValue = "";
			if (qn.Hyperlink == null) qn.Hyperlink = "";

			if (qn.NumberValue == NullValue.NullNumber)
			{
				if (qn.NValue != NullValue.NullNumber)
					return "ND"; // some NValue, return not determined

				else return qn.TextValue;
			}


			result = FormatNumber(qn.NumberValue, displayFormat, decimals);
			if (qn.Qualifier != "")
				result = qn.Qualifier + result;

			return result;
		}

		/// <summary>
		/// Basic floating point number formatter
		/// </summary>
		/// <param name="number"></param>
		/// <param name="format"></param>
		/// <param name="decimals"></param>
		/// <returns></returns>

		public static string FormatNumber(
			double number,
			ColumnFormatEnum format,
			int decimals)
		{
			string formatString, txt;
			int di;

			if (number == NullValue.NullNumber) return "";

			if (format == ColumnFormatEnum.Unknown || format == ColumnFormatEnum.Default)
			{ // if not specified apply user default
				format = DefaultNumberFormat;
				decimals = DefaultDecimals;
			}

			// Scientific notation

			try
			{
				if (format == ColumnFormatEnum.Scientific)
				{
					if (decimals <= 0) decimals = 1; // minimum of 1
					formatString = "{0:e" + decimals.ToString() + "}";
					txt = String.Format(formatString, number);
					if (txt.IndexOf("e+") > 0) txt = txt.Replace("e+00", "e+");
					else txt = txt.Replace("e-00", "e-"); // remove extra exponent zeros
				}

// Fixed number of significant digits

				else if (format == ColumnFormatEnum.SigDigits)
				{
					if (number == 0) // if zero then show enough digits
						return FormatNumber(number, ColumnFormatEnum.Decimal, decimals - 1);

					double absVal = Math.Abs(number);
					if (absVal > 1000000.0 || absVal < 0.000001) // if extreme do as scientific
						return FormatNumber(number, ColumnFormatEnum.Scientific, decimals);

					// Get position of 1st significant digit. Digits are numbered as follows
					// 7 654 321 012345 (neg numbers if right of decimal)
					// 1,000,000.000001

					double n2 = 1000000.0;
					for (di = 7; di >= -5; di--)
					{
						if (absVal >= n2) break;
						n2 = n2 / 10;
					}

					int sigDigits = decimals; // significant digits stored in decimals variable
					if (sigDigits <= 0) sigDigits = 1; // minimum of 1
					if (sigDigits < di) // no decimals, round & truncate non-sig digits in integer portion
					{
						decimals = 0; // nothing to right of decimal
						double factor = Math.Pow(10, di - sigDigits); // factor for rounding & truncating
						number += .5 * factor * Math.Sign(number); // round appropriately
						number = ((long)(number / factor)) * factor; // truncate
					}
					else decimals = sigDigits - di; // number of decimals needed
					return FormatNumber(number, ColumnFormatEnum.Decimal, decimals);
				}

// Fixed number of decimal places

				else // normal decimal places
				{
					formatString = "{0:F" + decimals.ToString() + "}";
					txt = String.Format(formatString, number);
				}

				return txt;
			}
			catch (Exception ex)
			{
				return "FormatNumber Error";
			}
		}

/// <summary>
/// Get format string for general floating point number
/// </summary>
/// <param name="format"></param>
/// <param name="decimals"></param>
/// <returns></returns>

		public static string GetFormatString(
			ColumnFormatEnum format,
			int decimals)
		{
			string formatString, txt;
			int di;

			if (format == ColumnFormatEnum.Unknown || format == ColumnFormatEnum.Default)
			{ // if not specified apply user default
				format = DefaultNumberFormat;
				decimals = DefaultDecimals;
			}

// Scientific notation

			if (format == ColumnFormatEnum.Scientific)
			{
				if (decimals <= 0) decimals = 1; // minimum of 1
				formatString = "{0:e" + decimals.ToString() + "}";
			}

// Fixed number of decimal places

			else // normal decimal places
			{
				formatString = "{0:F" + decimals.ToString() + "}";
			}

			return formatString;
		}

		/// <summary>
		/// Test number formatting for types of numbers
		/// </summary>

		public void TestFormatNumber()
		{
			double[] testData = { 123456, -123456, 1234.56, 1235.78, -1234.56, -1235.78, .1234, .1235, -.1234, -.1235 };

			foreach (double d in testData)
			{
				string txt = FormatNumber(d, ColumnFormatEnum.SigDigits, 3); // test sigfig formatting
				continue;
			}
		}

/// <summary>
/// Return true if string is qualified number
/// </summary>
/// <param name="txt"></param>
/// <returns></returns>

		public static bool IsQualifiedNumber(
			string txt)
		{
			QualifiedNumber qn;

			return TryParse(txt, out qn);
		}

/// <summary>
/// Parse qualified number (qualifier, number part only)
/// </summary>
/// <param name="txt"></param>
/// <returns></returns>

		public static QualifiedNumber Parse(
			string txt)
		{
			QualifiedNumber qn;

			if (!TryParse(txt, out qn)) throw new Exception("Invalid qualified number: " + txt);
			else return qn;
		}

/// <summary>
/// Try to parse qualified number (qualifier, number part only)
/// </summary>
/// <param name="txt"></param>
/// <param name="qn"></param>
/// <returns></returns>

		public static bool TryParse(
				string txt,
				out QualifiedNumber qn)
		{
			qn = new QualifiedNumber();
			bool isValid = TryParse(txt, qn);
			if (!isValid) qn = null;
			return isValid;
		}

		public static bool TryParse(
			string txt,
			QualifiedNumber qn)
		{
			double d;

			if (txt == null) return false;

			ParseToTextElements(txt, StaticQnte);
			qn.Qualifier = StaticQnte.Qualifier;

			if (NumberEx.DoubleTryParseEx(StaticQnte.NumberValue, out d))
				qn.NumberValue = d;

			else return false;

			// todo: finish parsing stats

			return true;
		}

/// <summary>
/// Parse a QualifiedNumber string into its constituent parts
/// </summary>
/// <param name="qnString"></param>
/// <returns></returns>

		public static QualifiedNumberTextElements ParseToTextElements(string qnString)
		{
			QualifiedNumberTextElements te = new QualifiedNumberTextElements();
			ParseToTextElements(qnString, te);
			return te;
		}

/// <summary>
/// Parse a QualifiedNumber string into its constituent parts
/// </summary>
/// <param name="qnString"></param>
/// <param name="te"></param>

		public static void ParseToTextElements(string qnString, QualifiedNumberTextElements te)
		{
			string txt, stats = "";

			te.Qualifier = "";
			te.NumberValue = "";
			te.StandardDeviation = "";
			te.NValue = "";
			te.NValueTested = "";
			te.TextValue = "";

			if (String.IsNullOrEmpty(qnString)) return;

			int i1 = qnString.IndexOf("(");
			if (i1 > 0)
			{
				stats = qnString.Substring(i1);
				txt = qnString.Substring(0, i1); // remove any stats
			}

			else txt = qnString;

			txt = txt.Trim();
			if (txt.Length == 0) return;

			if (txt.StartsWith("=")) txt = txt.Substring(1).Trim();
			else if (txt.StartsWith("<") || txt.StartsWith(">"))
			{
				te.Qualifier = txt.Substring(0, 1);
				te.NumberValue = txt.Substring(1).Trim();
			}

			else te.NumberValue = txt;

			// todo: finish parsing stats

			return;
		}

		/// <summary>
		/// Try to convert an object value to a qualified number
		/// </summary>
		/// <param name="value"></param>
		/// <param name="qn"></param>
		/// <returns></returns>

		public static bool TryConvert(
			object value,
			out QualifiedNumber qn)
		{
			if (value is double)
			{
				qn = new QualifiedNumber();
				qn.NumberValue = (double)value;
				return true;
			}

			else if (value is float)
			{
				qn = new QualifiedNumber();
				qn.NumberValue = (float)value;
				return true;
			}

			else if (value is int)
			{
				qn = new QualifiedNumber();
				qn.NumberValue = (int)value;
				return true;
			}

			else if (value is long)
			{
				qn = new QualifiedNumber();
				qn.NumberValue = (long)value;
				return true;
			}

			else if (value is string)
			{
				qn = new QualifiedNumber();
				qn.TextValue = (string)value;
				return true;
			}

			else
			{
				qn = null;
				return false;
			}
		}

		/// <summary>
		/// Try to convert an object to a double value
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="dVal"></param>
		/// <returns></returns>

		public static bool TryConvertToDouble(
			object vo,
			out double dVal)
		{
			string qualifier;

			if (!QualifiedNumber.TryConvertToQualifiedDouble(vo, out dVal, out qualifier))
				return false;

			if (String.IsNullOrEmpty(qualifier)) return true; // if no qualifier then ok

			else // failed
			{
				dVal = NullValue.NullNumber;
				return false;
			}
		}


		/// <summary>
		/// Convert to just a double value and optional qualifier
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="dVal"></param>
		/// <param name="qualifier"></param>
		/// <returns></returns>

		public static bool TryConvertToQualifiedDouble(
			object vo,
			out double dVal,
			out string qualifier)
		{
			QualifiedNumber qn;
			string s = null;

			dVal = NullValue.NullNumber;
			qualifier = null;

			if (vo == null || vo is DBNull || NullValue.IsNull(vo))
				return false;

			qualifier = ""; // provide a blank qualifier if not null

			if (vo is double)
				dVal = (double)vo;

			else if (vo is float)
				dVal = (float)vo;

			else if (vo is decimal)
				dVal = decimal.ToDouble((decimal)vo);

			else if (vo is QualifiedNumber)
			{
				qn = (QualifiedNumber)vo;
				dVal = qn.NumberValue;
				if (!String.IsNullOrWhiteSpace(qn.Qualifier)) qualifier = qn.Qualifier;
			}

			else if (vo is NumberMx)
			{
				NumberMx nmx = (NumberMx)vo;
				dVal = nmx.NumericValue;
			}

			else if (vo is int)
				dVal = (int)vo;

			else if (vo is Int16)
				dVal = (Int16)vo;

			else if (vo is Int64)
				dVal = (Int64)vo;

			else if (vo is byte)
				dVal = (byte)vo;

			else // try to convert to number
			{
				s = vo.ToString();
				if (QualifiedNumber.TryParse(s, out qn))
				{
					dVal = qn.NumberValue;
					if (!String.IsNullOrWhiteSpace(qn.Qualifier)) qualifier = qn.Qualifier;
				}

				else return false; // null
			}

			return true;
		}

		/// <summary>
		/// Try to convert a vo to a numeric qualifier
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="qualifier"></param>
		/// <returns></returns>

		public static bool TryConvertToQualifier (
			object vo,
			out string qualifier)
		{
			qualifier = null;

			if (vo == null || vo is DBNull || NullValue.IsNull(vo))
				return false;

			string 	s = vo.ToString().Trim();

			if (s == "" || s == "=")
				qualifier = "";

			else if (s == "<" || s == "<=")
				qualifier = "<";

			else if (s == ">" || s == ">=")
				qualifier = ">";

			else return false; 

			return true;
		}

		/// <summary>
		/// Custom compact serialization
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<Q>");
			StringBuilder sb = BeginSerialize("Q");

			AddIfNonDefault(Qualifier, QnModel.Qualifier, sb);
			AddIfNonDefault(NumberValue, QnModel.NumberValue, sb);
			AddIfNonDefault(StandardDeviation, QnModel.StandardDeviation, sb);
			AddIfNonDefault(StandardError, QnModel.StandardError, sb);
			AddIfNonDefault(NValue, QnModel.NValue, sb);
			AddIfNonDefault(NValueTested, QnModel.NValueTested, sb);
			if (TextValue != QnModel.TextValue) sb.Append(NormalizeForSerialize(TextValue));
			sb.Append(">");
			return sb;
		}

		void AddIfNonDefault(string v, string m, StringBuilder sb)
		{
			if (v != m)	sb.Append(NormalizeForSerialize(v));
			sb.Append(",");
		}

		void AddIfNonDefault(double v, double m, StringBuilder sb)
		{
			if (v != m)	sb.Append(v.ToString());
			sb.Append(",");
		}

		/// <summary>
		/// Custom compact deserialization
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="sai"></param>
		/// <returns></returns>

		public static QualifiedNumber Deserialize(string[] sa, int sai)
		{
			QualifiedNumber qn = new QualifiedNumber();

			qn.Qualifier = DeserializeField(sa[sai + 0], QnModel.Qualifier);
			qn.NumberValue = DeserializeField(sa[sai + 1], QnModel.NumberValue);
			qn.StandardDeviation = DeserializeField(sa[sai + 2], QnModel.StandardDeviation);
			qn.StandardDeviation = DeserializeField(sa[sai + 3], QnModel.StandardError);
			qn.NValue = DeserializeField(sa[sai + 4], QnModel.NValue);
			qn.NValueTested = DeserializeField(sa[sai + 5], QnModel.NValueTested);
			qn.TextValue = DeserializeField(sa[sai + 6], QnModel.TextValue);
			return qn;
		}

		static int DeserializeField(string sv, int v)
		{
			if (!Lex.IsNullOrEmpty(sv))
				return int.Parse(sv);
			else return v;
		}

		static double DeserializeField(string sv, double v)
		{
			if (!Lex.IsNullOrEmpty(sv))
				return double.Parse(sv);
			else return v;
		}

		static string DeserializeField(string sv, string v)
		{
			if (!Lex.IsNullOrEmpty(sv))
				return sv;
			else return v;
		}

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.QualifiedNo);
			base.SerializeBinary(bw);

			AddIfNonDefault(Qualifier, bw);
			AddIfNonDefault(NumberValue, bw);
			AddIfNonDefault(StandardDeviation, bw);
			AddIfNonDefault(StandardError, bw);
			AddIfNonDefault(NValue, bw);
			AddIfNonDefault(NValueTested, bw);
			AddIfNonDefault(TextValue, bw);
		}

		void AddIfNonDefault(string v, BinaryWriter bw)
		{
			if (v != "")
			{
				bw.Write(true);
				bw.Write(Lex.ToString(v));
			}
			else bw.Write(false);
		}

		void AddIfNonDefault(int v, BinaryWriter bw)
		{
			if (v != NullNumber)
			{
				bw.Write(true);
				bw.Write(v);
			}
			else bw.Write(false);
		}

		void AddIfNonDefault(double v, BinaryWriter bw)
		{
			if (v != NullNumber)
			{
				bw.Write(true);
				bw.Write(v);
			}
			else bw.Write(false);
		}

		/// <summary>
		/// Binary Deserialize
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>

		public static QualifiedNumber DeserializeBinary(BinaryReader br)
		{
			QualifiedNumber qn = new QualifiedNumber();
			MobiusDataType.DeserializeBinary(br, qn);

			if (br.ReadBoolean())
				qn.Qualifier = br.ReadString();

			if (br.ReadBoolean())
				qn.NumberValue = br.ReadDouble();

			if (br.ReadBoolean())
				qn.StandardDeviation = br.ReadDouble();

			if (br.ReadBoolean())
				qn.StandardError = br.ReadDouble();

			if (br.ReadBoolean())
				qn.NValue = br.ReadInt32();

			if (br.ReadBoolean())
				qn.NValueTested = br.ReadInt32();

			if (br.ReadBoolean())
				qn.TextValue = br.ReadString();

			return qn;
		}

/// <summary>
/// Clone a qualified number
/// </summary>
/// <returns></returns>

		public new QualifiedNumber Clone()
		{
			QualifiedNumber qn = (QualifiedNumber)this.MemberwiseClone();

			if (qn.MdtExAttr != null) qn.MdtExAttr = MdtExAttr.Clone();
			if (qn.QnExAttr != null) qn.QnExAttr = QnExAttr.Clone();

			return qn;
		}


		/// <summary>
		/// Return rough string version of qualified number
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			QualifiedNumber qn = this;
			string s = "";

			if (IsNull) return NullValue.NullValueString;

			if (!String.IsNullOrEmpty(Qualifier)) s += Qualifier;
			if (NumberValue != QualifiedNumber.NullNumber) s += NumberValue;

			if (qn.NValue > 1 || qn.NValueTested > 1 || // format with sd, n 
				(qn.NValue >= 0 && qn.NValueTested > qn.NValue) || // number tested > n
				(qn.NumberValue == QualifiedNumber.NullNumber && // also null numbers if some nonzero nvalue
				(qn.NValue > 0 || qn.NValueTested > 0)))
			{
				if (qn.StandardDeviation != QualifiedNumber.NullNumber)
					s += ", sd=" + qn.StandardDeviation;

				if (qn.StandardError != QualifiedNumber.NullNumber)
					s += ", se=" + qn.StandardError;

				s += ", n=" + (int)qn.NValue;

				if (qn.NValueTested > qn.NValue) s+= "/" + (int)qn.NValueTested;
			}

			if (!String.IsNullOrEmpty(qn.TextValue)) 
			{
				if (s.Length>0) s+= " ";
				s+= qn.TextValue;
			}

			return s;
		}

	}

/// <summary>
/// QN attributes that may or may not be defined
/// Uses less memory if not defined
/// </summary>

	[Serializable]
	public class QnExtendedAttributes
	{
		public double StandardDeviation = NullValue.NullNumber;
		public double StandardError = NullValue.NullNumber;
		public int NValue = NullValue.NullNumber;
		public int NValueTested = NullValue.NullNumber;
		public string TextValue = "";

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public QnExtendedAttributes Clone()
		{
			QnExtendedAttributes qnea = (QnExtendedAttributes)this.MemberwiseClone();
			return qnea;
		}

	}

/// <summary>
/// QualifiedNumber broken down into text string elements
/// </summary>

	public class QualifiedNumberTextElements
	{
		public string Qualifier = "";
		public string NumberValue = "";
		public string StandardDeviation = "";
		public string NValue = "";
		public string NValueTested = "";
		public string TextValue = "";
	}

	/// <summary>
	/// Result state for qualified number
	/// </summary>

	public enum QnChangeState
	{
		Undefined = 0,
		Added = 1,
		Unchanged = 2,
		Modified = 3,
		Deleted = 4
	}

}
