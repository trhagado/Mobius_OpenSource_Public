using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Mobius.Data
{
/// <summary>
/// Number class extended with Mobius attributes
/// </summary>

  [Serializable]
	public class NumberMx : MobiusDataType, IComparable, IConvertible
	{
		/// <summary>
		/// Numeric value
		/// </summary>

		[DataMember]
		public double Value = QualifiedNumber.NullNumber;

		public int NullNumber => QualifiedNumber.NullNumber;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// 
		public NumberMx()
		{
			return;
		}

		/// <summary>
/// General constructor from an object
/// </summary>
/// <param name="o"></param>

		public NumberMx(
			object o)
		{
			if (NullValue.IsNull(o)) return;

			if (o is int) Value = (int)o;
			else if (o is double) Value = (double)o;
			else if (o is string)
			{
				if (!double.TryParse((string)o, out Value))
					throw new Exception("Invalid number: " + (string)o);
			}

			else if (o is QualifiedNumber)
			{
				QualifiedNumber qn = o as QualifiedNumber;
				Value = qn.NumberValue;
				((MobiusDataType)o).MemberwiseCopy(this); // copy base values
			}

			else throw new InvalidCastException(o.GetType().ToString());
		}

		/// <summary>
		/// Convert the specified object to the corresponding MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static NumberMx ConvertTo(
			object o)
		{
			if (o is NumberMx) return (NumberMx)o;
			else return new NumberMx(o);
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

// First convert o to a double value

			double compVal = NullNumber;

			if (o == null) compVal = NullNumber;

			else if (o is int)
				compVal = (int)o;

			else if (o is long)
				compVal = (long)o;

			else if (o is float)
				compVal = (float)o;

			else if (o is double)
				compVal = (double)o;

			else if (o is decimal)
				compVal = decimal.ToDouble((decimal)o);

			else if (o is double)
				compVal = (double)o;

			else if (o is string)
			{
				if (!NumberEx.DoubleTryParseEx((string)o, out compVal))
					compVal = NullNumber;
			}

			else if (o is StringMx)
			{
				StringMx sx = o as StringMx;
				if (!NumberEx.DoubleTryParseEx(sx?.Value, out compVal))
					compVal = NullNumber;
			}

			else if (o is MobiusDataType)
				compVal = ((MobiusDataType)o).NumericValue;

			else throw new Exception("Can't compare a " + GetType().Name + " to a " + o.GetType());

// Now compare compVal to this NumberMx value

			if (this.Value != NullNumber && compVal != NullNumber) // both not null 
				return this.Value.CompareTo(compVal);

			else if (this.Value == NullNumber && compVal == NullNumber) // both null 
				return 0; // say equal 

			else if (this.Value != NullNumber) // this not null, compValue is null
				return -1; // indicate this is less (put non-null first)

			else // compValue not null, this is null
				return 1; // indicate that this is greater (put non-null first)
		}

		/// <summary>
		/// Return true if null value
		/// </summary>

		[XmlIgnore]
		public override bool IsNull
		{
			get
			{
				if (Value == QualifiedNumber.NullNumber)
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
				return unchecked(Value == (int)Value) && // unchecked avoids overflow exceptions for very large integers
						Value != QualifiedNumber.NullNumber;
			}
		}

		/// <summary>
		/// Get/set numeric value
		/// </summary>

		[XmlIgnore]
		public override double NumericValue
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
		/// IConvertible.GetTypeCode;
		/// </summary>
		/// <returns></returns>
		public new TypeCode GetTypeCode()
		{
			return TypeCode.Double;
		}

		/// <summary>
		/// Return double value (IConvertible)
		/// </summary>
		/// <returns></returns>
		public override double GetDoubleValue()
		{
			return Value;
		}

		/// <summary>
		/// Custom compact serialization of common MobiusDataType attributes
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<N>");
			StringBuilder sb = BeginSerialize("N");
			sb.Append(Value);
			sb.Append(">");
			return sb;
		}

		/// <summary>
		/// Custom Compact deserialization
		/// </summary>
		/// <param name="sai"></param>
		/// <returns></returns>

		public static NumberMx Deserialize(string[] sa, int sai)
		{

			NumberMx nx = new NumberMx();
			if (!Lex.IsNullOrEmpty(sa[sai]))
				nx.Value = double.Parse(sa[sai]);
			return nx;
		}

/// <summary>
/// Binary serialize
/// </summary>
/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.NumberMx);
			base.SerializeBinary(bw);
			bw.Write(Value);
		}

/// <summary>
/// Binary Deserialize
/// </summary>
/// <param name="br"></param>
/// <returns></returns>

		public static NumberMx DeserializeBinary(BinaryReader br)
		{
			NumberMx nx = new NumberMx();
			MobiusDataType.DeserializeBinary(br, nx);
			nx.Value = br.ReadDouble();
			return nx;
		}

		/// <summary>
		/// Clone 
		/// </summary>
		/// <returns></returns>

		public new NumberMx Clone()
		{
			NumberMx nx = (NumberMx)this.MemberwiseClone();
			if (nx.MdtExAttr != null) nx.MdtExAttr = MdtExAttr.Clone();
			return nx;
		}

/// <summary>
/// Return numeric value converted to string
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			if (Value == NullValue.NullNumber) return NullValue.NullValueString;

			return Value.ToString();
		}

	}
}
