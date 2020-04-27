using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mobius.Data
{

	/// <summary>
	/// IConvertible interface for MobiusDataType
	/// </summary>

	public partial class MobiusDataType : IComparable, IConvertible
	{
		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			double d = GetDoubleValue();
			if (d == NullValue.NullNumber) return false;
			else return true;
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(GetDoubleValue());
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(GetDoubleValue());
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(GetDoubleValue());
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(GetDoubleValue());
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return GetDoubleValue();
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(GetDoubleValue());
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(GetDoubleValue());
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(GetDoubleValue());
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(GetDoubleValue());
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(GetDoubleValue());
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return ToString();
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return Convert.ChangeType(GetDoubleValue(), conversionType);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(GetDoubleValue());
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(GetDoubleValue());
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(GetDoubleValue());
		}

/// <summary>
/// Converts MobiusDataType to a double value
/// </summary>
/// <returns></returns>

		public virtual double GetDoubleValue()
		{
			return -1;
		}

	}
}