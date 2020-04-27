using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Mobius.Data
{
	/// <summary>
	/// DateTime with extended with Mobius attributes
	/// </summary>

  [DataContract]
	public class DateTimeMx : MobiusDataType, IComparable
	{

		static string[] Month = 
		{
			"Jan","Feb","Mar","Apr","May","Jun",
			"Jul","Aug","Sep","Oct","Nov","Dec"
		};

		static string[] MonthFull = 
		{
			"January","February","March","April","May","June",
			"July","August","September","October","November","December"
		};

		static string DefaultFormat = "d-MMM-yyyy"; // default format

		/// <summary>
		/// DateTime value
		/// </summary>

		[DataMember]
		public DateTime Value = DateTime.MinValue;

		public DateTimeMx()
		{
			return;
		}

		public DateTimeMx(
			DateTime value)
		{
			Value = value;
		}

/// <summary>
/// Create an instance from the specified internal or external format string
/// </summary>
/// <param name="s"></param>

		public DateTimeMx(
			String s)
		{
			if (DateTimeUS.TryParseDate(s, out Value)) return; // try external format
			Value = NormalizedToDateTime(s); // try internal format
			return;
		}

		/// <summary>
		/// Convert the specified object to the corresponding MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static DateTimeMx ConvertTo(
			object o)
		{
			if (o is DateTimeMx) return (DateTimeMx)o;
			else if (NullValue.IsNull(o)) return new DateTimeMx();
			else if (o is DateTime) return new DateTimeMx((DateTime)o);
			else if (o is string) return new DateTimeMx((string)o);

			throw new InvalidCastException(o.GetType().ToString());
		}

/// <summary>
/// Try to parse a DateTime
/// </summary>
/// <param name="textValue"></param>
/// <param name="dtEx"></param>
/// <returns></returns>

		public static bool TryParse(
			string textValue,
			out DateTimeMx dtEx)
		{
			dtEx = new DateTimeMx(textValue);
			if (dtEx.Value == DateTime.MinValue) return false;
			return true;
		}

		/// <summary>
		/// Get hash code for this MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			int hash = Value.GetHashCode();
			return hash;
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
		/// Compare two values (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public override int CompareTo(
			object o)
		{
			if (o == null) return 1;

			if (!(o is DateTimeMx)) throw new Exception("Can't compare a " + GetType().Name + " to a " + o.GetType());

			DateTimeMx s = this;
			DateTimeMx s2 = o as DateTimeMx;


			if (s.IsNull && s2.IsNull) return 0;
			else if (!s.IsNull && s2.IsNull) return 1;
			else if (s.IsNull && !s2.IsNull) return -1;
			else
			{
				int compareResult = s.Value.Date.CompareTo(s2.Value.Date); // compare dates without times
//				if (compareResult == 0) s = s; // debug
				return compareResult;
			}
		}

		/// <summary>
		/// Return true if null value
		/// </summary>

		[XmlIgnore]
		public override bool IsNull
		{
			get
			{
				if (Value == DateTime.MinValue)
					return true;
				else return false;
			}
		}

/// <summary>
/// Get/set DateTime value
/// </summary>

		[XmlIgnore]
		public override DateTime DateTimeValue
		{
			get
			{
				return Value;
			}

			set
			{
				Value = value;
			}
		}

		/// <summary>
		/// Convert the value to the nearest primitive type
		/// </summary>
		/// <returns></returns>

		public override object ToPrimitiveType()
		{
			return Value;
		}

		/// <summary>
		/// Return the internal normalized string version of the MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override string FormatForCriteria()
		{
			DateTime dt = Value;
			string normalizedDate = String.Format("{0,4:0000}{1,2:00}{2,2:00}", dt.Year, dt.Month, dt.Day);
			return normalizedDate;
		}

		/// <summary>
		/// Return a formatted string for the MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override string FormatCriteriaForDisplay()
		{
			string normalizedDate = FormatForCriteria();
			string formattedDate = Format(normalizedDate);
			return formattedDate;
		}

		/// <summary>
		/// Get current date in yyyymmdd format
		/// </summary>
		/// <returns>current date in yyyymmdd format</returns>
		/// 
		public static string GetCurrentDate()
		{
			DateTime dt = DateTime.Now;
			String tok = String.Format("{0,4:0000}{1,2:00}{2,2:00}", dt.Year, dt.Month, dt.Day);
			return tok;
		}

		/// <summary>
		/// Return the current data and time to minute resolution in a sortable format
		/// </summary>
		/// <returns></returns>

		public static string GetCurrentDateTimeToMinuteResolution()
		{
			DateTime dt = DateTime.Now;
			String tok = String.Format("{0,4:0000}-{1,2:00}-{2,2:00} {3,2:00}.{4,2:00}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
			return tok;
		}

		/// <summary>
		/// Normalize a date to standard internal yyyymmdd format
		/// </summary>
		/// <param name="extDate"></param>
		/// <returns></returns>

		public static string Normalize (
			string extDate)
		{
			int y,m,d,i1,i2;
			string tok;
			string [] toks;
			int [] val = new int[3];

			tok = Lex.RemoveAllQuotes(extDate);

			if (tok.Contains(":")) // if time included remove it
			{
				i1 = tok.IndexOf(" ");
				if (i1 >= 0) tok = tok.Substring(0, i1);
			}
			
			toks = tok.Split(new char [] {'.', ',', ' ', '-', '/'});
			if (toks.Length!=3) return null;

			int monthPos=-1;

			for (i1=0; i1<3; i1++) 
			{
				tok=toks[i1];
				if (tok.Length==0) return null;
				if (Char.IsDigit(tok[0]))
				{
					try
					{
						val[i1] = Convert.ToInt32(tok);
					}
					catch (Exception e) 
					{
						return null;
					}
				}
				else // spelled out month
				{ 
					for (i2=0; i2<12; i2++) 
					{
						if (Lex.Eq(tok,Month[i2])) break;
						if (Lex.Eq(tok,MonthFull[i2])) break;
					}
					if (i2>=12) return null;
					val[i1] = i2+1;
					monthPos=i1;
				}
			}

			if (monthPos==1) 
			{
				d = val[0];
				m = val[1];
				y = val[2];
			}
			else 
			{
				d = val[1];
				m = val[0];
				y = val[2];
			}

			if (y<1900 && y>=40) y+=1900; 
			if (y<40) y+=2000; 

			if ( // check ranges
				d>31 || d<=0 || 
				m>12 || m<=0 ||
				y<1900 || y>2100) 
				return null;

			//string usDate = m.ToString() + "/" + d.ToString() + "/" + y.ToString();
			//try { DateTime dt = ParseUS(usDate); } // see if valid day of month
			//catch (Exception ex) { return null; }

			tok = String.Format("{0,4:0000}{1,2:00}{2,2:00}", y,m,d);
			return tok;
		}

		/// <summary>
/// Format an internal yyyymmdd date into the default external format
/// </summary>
/// <param name="intDate"></param>
/// <returns></returns>

		public static string Format(
			string intDate)
		{
			DateTime dt = NormalizedToDateTime(intDate);
			string extDate = Format(dt, DefaultFormat);
			return extDate;
		}

		/// <summary>
		/// Format a DateTime into an external format
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>

		public static string Format(
			DateTime dt)
		{
			return Format(dt, DefaultFormat);
		}

// Get format string for a date QueryColumn
		public static string GetFormatString(
			QueryColumn qc)
		{
			string formatString = qc.DisplayFormatString;
			if (qc.IsAggregationGroupBy)
			{
				AggregationDef at = qc.Aggregation;
				AggregationTypeDetail atd = at.TypeDetail;
				if (atd != null) formatString = atd.FormatString;
			}

			if (Lex.Contains(formatString, "none")) // remove any none for date format
				formatString = Lex.Replace(formatString, "none", "").Trim();

			if (Lex.IsNullOrEmpty(formatString))
				formatString = DefaultFormat;

			return formatString;
		}

		/// <summary>
		/// Format a DateTime into an external format
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>

		public static string Format(
			DateTime dt,
			string formatString)
		{
			string extDt = "", txt;

			try
			{
				extDt = dt.ToString(formatString);

				if (Lex.Contains(extDt, "<Q>")) // special case to handle quarter
				{
					txt = "Q" + ((dt.Month + 2) / 3).ToString();
					extDt = Lex.Replace(extDt, "<Q>", txt);
				}

				return extDt;
			}
			catch (Exception ex)
			{
				return dt.ToString();
			}
		}

		/// <summary>
		/// Normalize
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>

		public static String Normalize (
			DateTime dt)
		{
			string tok = String.Format("{0,4:0000}{1,2:00}{2,2:00}", dt.Year, dt.Month, dt.Day);
			return tok;
		}

/// <summary>
/// NormalizedToDateTime
/// </summary>
/// <param name="intDate"></param>
/// <returns></returns>

		public static DateTime NormalizedToDateTime (
			string intDate)
		{
			int y,m,d;

			if (String.IsNullOrEmpty(intDate)) return DateTime.MinValue;

			try
			{
				y = Convert.ToInt32(intDate.Substring(0, 4));
				m = Convert.ToInt32(intDate.Substring(4, 2));
				d = Convert.ToInt32(intDate.Substring(6, 2));
				DateTime dt = new DateTime(y, m, d);
				return dt;
			}
			catch (Exception ex) { return DateTime.MinValue; }
		}


		/// <summary>
		/// Custom compact serialization
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<D>");
			StringBuilder sb = BeginSerialize("D");
			sb.Append(NormalizeForSerialize(DateTimeUS.ToDateString(Value)));
			sb.Append(">");
			return sb;
		}

		/// <summary>
		/// Custom Compact deserialization
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="sai"></param>
		/// <returns></returns>

		public static DateTimeMx Deserialize(string[] sa, int sai)
		{
			DateTimeMx dtx = new DateTimeMx();
			if (!Lex.IsNullOrEmpty(sa[sai]))
				dtx.Value = DateTimeUS.ParseDate(sa[sai]);
			return dtx;
		}

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.DateTimeMx);
			base.SerializeBinary(bw);
			bw.Write(Value.Ticks);
		}

		/// <summary>
		/// Binary Deserialize
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>

		public static DateTimeMx DeserializeBinary(BinaryReader br)
		{
			DateTimeMx dtx = new DateTimeMx();
			MobiusDataType.DeserializeBinary(br, dtx);
			long ticks = br.ReadInt64();
			dtx.Value = new DateTime(ticks);
			return dtx;
		}

		/// <summary>
		/// Clone 
		/// </summary>
		/// <returns></returns>

		public new DateTimeMx Clone()
		{
			DateTimeMx dtMx = (DateTimeMx)this.MemberwiseClone();
			if (dtMx.MdtExAttr != null) dtMx.MdtExAttr = MdtExAttr.Clone();
			return dtMx;
		}

		/// <summary>
		/// Return date value converted to string
		/// </summary>
		/// <returns></returns>

		public override string ToString()
		{
			if (Value == DateTime.MinValue) return NullValue.NullValueString;

			return Format(Value, DefaultFormat);
		}
	}

	/// <summary>
	/// Conversion to/from US format Date and DateTimes
	/// </summary>
	public class DateTimeUS
	{
		static CultureInfo EnUsFormatProvider = new CultureInfo("en-US"); // format provider for English, United States type formats
		static CultureInfo EnGbFormatProvider = new CultureInfo("en-GB"); // format provider for English, UK type formats

		/// <summary>
		/// Parse a US format string
		/// </summary>
		/// <param name="dtString"></param>
		/// <returns></returns>

		public static DateTime ParseDate(string dtString)
		{
			DateTime dt;
			if (dtString == "1/1/0001") return DateTime.MinValue;
			if (TryParseDate(dtString, out dt)) return dt;
			else return DateTime.Parse(dtString, EnUsFormatProvider); // cause exception to be thrown
		}

		/// <summary>
		/// Try parse of a US format date string string to a DateTime
		/// </summary>
		/// <param name="dtString"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>

		public static bool TryParseDate(string dtString, out DateTime dateTime)
		{
			dateTime = DateTime.MinValue;

			try
			{
				string normalized = DateTimeMx.Normalize(dtString);
				if (String.IsNullOrEmpty(normalized)) return false;
				dateTime = DateTimeMx.NormalizedToDateTime(normalized);
				if (dateTime == DateTime.MinValue) return false;
				else return true;
			}

			catch (Exception ex) { return false; }
		}

		/// <summary>
		/// Convert a DateTime to a US string format
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>

		public static string ToDateString(DateTime dateTime)
		{
			string txt = dateTime.Month.ToString() + "/" + dateTime.Day + "/" + dateTime.Year;
			return txt; // use US format (date only) to avoid writing a cache file that old Mobius can't process
		}

		/// <summary>
		/// Parse a United States format DateTime string
		/// </summary>
		/// <param name="dtString"></param>
		/// <returns></returns>

		public static DateTime Parse(string dtString)
		{
			DateTime dt;
			if (Parse(dtString, out dt)) return dt;
			else return DateTime.Parse(dtString, EnUsFormatProvider); // cause exception to be thrown
		}

		/// <summary>
		/// Try parse a United States format DateTime string
		/// </summary>
		/// <param name="dtString"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>

		public static bool Parse(string dtString, out DateTime dateTime)
		{
			bool result = DateTime.TryParse(dtString, EnUsFormatProvider, DateTimeStyles.None, out dateTime);

			if (result == false) // some older persisted dates may be in UK format
				result = DateTime.TryParse(dtString, EnGbFormatProvider, DateTimeStyles.None, out dateTime);

			return result;
		}

		/// <summary>
		/// Convert a DateTime to a US string format
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>

		public static string ToString(DateTime dateTime)
		{
			return dateTime.ToString(EnUsFormatProvider);
		}

		}
	}
