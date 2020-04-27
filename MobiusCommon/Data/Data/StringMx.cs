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
	/// String with extended with Mobius attributes
	/// </summary>

  [Serializable]
	public class StringMx : MobiusDataType, IComparable
	{
		/// <summary>
		/// String value
		/// </summary>

		[DataMember]
		public string Value = "";

		public StringMx()
		{
			return;
		}

		public StringMx(
			string value)
		{
			Value = value;
		}

		/// <summary>
		/// Convert the specified object to the corresponding MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static StringMx ConvertTo(
			object o)
		{
			StringMx sx;

			if (o is StringMx) return (StringMx)o;
			else if (NullValue.IsNull(o)) return new StringMx();
			else if (o is MobiusDataType)
			{
				MobiusDataType mdt = o as MobiusDataType;
				sx = new StringMx(mdt.ToString());
				mdt.MemberwiseCopy(sx);
				return sx;
			}
			else return new StringMx(o.ToString());
		}

		/// <summary>
		/// Get hash code for this MobiusDataType
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			if (Lex.IsUndefined(Value)) return 0;
			else return Value.GetHashCode();
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

			StringMx sx = this;

			if (o is StringMx)
			{
				StringMx sx2 = o as StringMx;

				if (sx.IsNull && sx2.IsNull) return 0;
				else if (!sx.IsNull && sx2.IsNull) return 1;
				else if (sx.IsNull && !sx2.IsNull) return -1;
				else return string.Compare(sx.Value, sx2.Value, true); // compare ignoring case
			}

			else if (o is QualifiedNumber)
			{
				QualifiedNumber qn = o as QualifiedNumber;
				if (sx.IsNull && qn.IsNull) return 0;
				else if (!sx.IsNull && qn.IsNull) return 1;
				else if (sx.IsNull && !qn.IsNull) return -1;
				return string.Compare(this.Value, qn.TextValue, true); // compare ignoring case
			}

			else if (o is string)
			{
				string s = (string)o;
				if (sx.IsNull)
				{
					if (o == null) return 0;
					else return -1;
				}
				else return string.Compare(sx.Value, s, true);
			}

			else throw new Exception("Can't compare a " + GetType().Name + " to a " + o.GetType());

		}

		/// <summary>
		/// Return true if null value
		/// </summary>

		[XmlIgnore]
		public override bool IsNull
		{
			get
			{
				if (Lex.IsUndefined(Value) || Lex.Eq(Value, "null"))
					return true;
				else return false;
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
/// Format an integer into a number with commas
/// </summary>
/// <param name="value"></param>
/// <returns></returns>

		public static string FormatIntegerWithCommas(int value)
		{
			return string.Format("{0:n0}", value);
		}

		/// <summary>
		/// Custom compact serialization
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<S>");
			StringBuilder sb = BeginSerialize("S");
			sb.Append(NormalizeForSerialize(Value));
			sb.Append(">");
			return sb;
		}

/// <summary>
/// Custom Compact deserialization
/// </summary>
/// <param name="sa"></param>
/// <param name="sai"></param>
/// <returns></returns>

		public static StringMx Deserialize(string[] sa, int sai)
		{
			StringMx sx = new StringMx(sa[sai]);
			return sx;
		}

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.StringMx);
			base.SerializeBinary(bw);
			bw.Write(Lex.S(Value));
		}

		/// <summary>
		/// Binary Deserialize
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>

		public static StringMx DeserializeBinary(BinaryReader br)
		{
			StringMx sx = new StringMx();
			MobiusDataType.DeserializeBinary(br, sx);
			sx.Value = br.ReadString();
			return sx;
		}

		/// <summary>
		/// Clone 
		/// </summary>
		/// <returns></returns>

		public new StringMx Clone()
		{
			StringMx sx = (StringMx)this.MemberwiseClone();
			if (sx.MdtExAttr != null) sx.MdtExAttr = MdtExAttr.Clone();
			return sx;
		}

/// <summary>
/// Convert to string
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			if (String.IsNullOrEmpty(Value)) return "";
			else return Value;
		}


	}
}
