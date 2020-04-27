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
	/// Base class of Mobius custom data types
	/// </summary>

	[DataContract]
	public partial class MobiusDataType : IComparable, IConvertible
	{
		// This is the base class for all custom Mobius data types and contains the common MDT data members.
		// When displaying large data sets a very large number of these objects will be created and so they
		// are optimized to use a minimum amount of memory. 
		// The base MobiusDataType contains the following members (object size ~16 bytes)
		// 1. Bit flags (1 byte)
		// 2. Background color (3 bytes)
		// 3. A unsigned integer link to the source data row
		// 4. A string link to the source data row for non-integer values
		// 5. A reference to the optional MdtExtendedAttributes containing formatting information

		static MdtExtendedAttributes DefaultAttrValues = new MdtExtendedAttributes(); // default attribute values
		static Color DefaultBackColor = Color.Empty; // default background color

/// <summary>
/// Basic constructor
/// </summary>

			public MobiusDataType()
		{
			return;
		}

/// <summary>
/// Create an instance with the specified FormattedText
/// </summary>
/// <param name="s"></param>

		public MobiusDataType (
			String s)
		{
			FormattedText = s;
		}

		public MobiusDataType (
			String s,
			CellStyleMx cellStyle)
		{
			FormattedText = s;
			if (cellStyle != null)
			{
				this.BackColor = cellStyle.BackColor;
				this.ForeColor = cellStyle.ForeColor;
			}
		}

		/// <summary>
		/// Single bit flags combined in a one byte value 
		/// </summary>

		protected byte _flags = 0;

		// Bit flag values

		const byte FilteredFlag = 1; // value is filtered out
		const byte ModifiedFlag = 2; // value has been modified
		const byte HyperlinkedFlag = 4; // a hyperlink is associated with the value
		const byte BackColorDefined = 8; // the background color is defined
		const byte NonExistantValueFlag = 16; // value is nonexistant
		const byte RetrievingValueFlag = 32; // retrieving value (e.g. a bitmap)

		/// <summary>
		/// Filter state for field (from Flags)
		/// </summary>

		public bool Filtered
		{
			set
			{
				if (value == true) _flags |= FilteredFlag;
				else _flags &= (255 - FilteredFlag);
			}
			get { return (_flags & FilteredFlag) != 0; }
		}

		/// <summary>
		/// True if value modified (from Flags)
		/// </summary>

		public bool Modified
		{
			set
			{
				if (value == true) _flags |= ModifiedFlag;
				else _flags &= (255 - ModifiedFlag);
			}
			get { return (_flags & ModifiedFlag) != 0; }
		}

		/// <summary>
		/// True if value has hyperlink associated with it (from Flags)
		/// </summary>

		public bool Hyperlinked
		{
			set
			{
				if (value == true) _flags |= HyperlinkedFlag;
				else _flags &= (255 - HyperlinkedFlag);
			}
			get { return (_flags & HyperlinkedFlag) != 0; }
		}

		/// <summary>
		/// True if value has hyperlink associated with it (from Flags)
		/// </summary>

		public bool IsNonExistant
		{
			set
			{
				if (value == true) _flags |= NonExistantValueFlag;
				else _flags &= (255 - NonExistantValueFlag);
			}
			get { return (_flags & NonExistantValueFlag) != 0; }
		}

		/// <summary>
		/// True if in process of retrieving the value (from Flags)
		/// </summary>

		public bool IsRetrievingValue
		{
			set
			{
				if (value == true) _flags |= RetrievingValueFlag;
				else _flags &= (255 - RetrievingValueFlag);
			}
			get { return (_flags & RetrievingValueFlag) != 0; }
		}

		/// <summary>
		/// Background color
		/// </summary>

		[DataMember]
		public Color BackColor
		{
			set
			{
				_backRed = value.R;
				_backGreen = value.G;
				_backBlue = value.B;
				_backAlpha = value.A;

				if (value.ToArgb() != Color.Empty.ToArgb()) // convert to int for more reliable comparison of color
					_flags |= BackColorDefined;
				else _flags &= (255 - BackColorDefined);
			}
			get
			{
				if ((_flags & BackColorDefined) != 0)
					return Color.FromArgb(_backAlpha, _backRed, _backGreen, _backBlue);
				else return Color.Empty;
			}
		}
		byte _backRed = 0;
		byte _backGreen = 0;
		byte _backBlue = 0;
		byte _backAlpha = 0;

		/// <summary>
		/// DbLink - Identifies a path to the field in the database
		/// </summary>

		[DataMember]
		public string DbLink
		{
			set 
			{
				uint ui = 0;

				if (uint.TryParse(value, out ui)) // store as uint if integer
				{
					_dbLinkInt = ui;
					_dbLinkString = "";
				}

				else
				{
					_dbLinkString = value;
					_dbLinkInt = 0; 
				}
			}

			get 
			{
				if (_dbLinkInt > 0) return _dbLinkInt.ToString();
				else return _dbLinkString; 
			}
		}

		protected uint _dbLinkInt = 0; // integer form of DbLink (more-compact)
		protected string _dbLinkString = ""; // string form of DbLink

/// <summary>
/// Extended attributes
/// Significant memory is saved if these are not set to non-default values
/// which avoids the allocation of an ExtendedAttributes object
/// </summary>

		protected MdtExtendedAttributes MdtExAttr = null; // any non-default attribute values associated with this Mdt instance (not normally referenced)

/// <summary>
/// Return reference for getting attribute values, do not use for setting values
/// </summary>

		MdtExtendedAttributes AttrsGetRef
		{
			get
			{
				if (MdtExAttr != null) return MdtExAttr; // if defined return it
				else return DefaultAttrValues; // if not defined return default value object
			}
		}

/// <summary>
/// Get reference that allows setting of value by allocating if not already done
/// </summary>

		MdtExtendedAttributes AttrsSetRef // reference for setting attribute values
		{
			get
			{
				if (MdtExAttr == null) MdtExAttr = new MdtExtendedAttributes();
				return MdtExAttr;
			}
		}

		/// <summary>
		/// Foreground color
		/// </summary>

		[DataMember]
		public Color ForeColor
		{
			set 
			{
				if (AttrsGetRef.ForeColor == value) return;
				AttrsSetRef.ForeColor = value;
				_setForeColorCount++;
			}
			get { return AttrsGetRef.ForeColor; }
		}
		static int _setForeColorCount;

		/// <summary>
		/// Hyperlink - this is the link from the database (i.e. annotation tables) and for other special cases
		/// Normally just the HyperLinked flag is set and the actual link is calculated from the MDT when needed
		/// </summary>
		/// 
		[DataMember]
		public string Hyperlink
		{
			set 
			{
				if (AttrsGetRef.Hyperlink == value) return;
				AttrsSetRef.Hyperlink = value;
				if (!Lex.IsNullOrEmpty(value)) Hyperlinked = true;
				else Hyperlinked = false;
				_setHyperlinkCount++;
			}
			get { return AttrsGetRef.Hyperlink; }
		}
		static int _setHyperlinkCount;

/// <summary>
/// Formatted value text 
/// </summary>

		public string FormattedText
		{
			set
			{
				if (AttrsGetRef.FormattedText == value) return;
				AttrsSetRef.FormattedText = value;
				_setFormattedTextCount++;

				//if (value == null) value = value; // debug
			}
			get { return AttrsGetRef.FormattedText; }
		}
		static int _setFormattedTextCount;

/// <summary>
/// Formatted value Bitmap 
/// </summary>

		public Bitmap FormattedBitmap
		{
			set
			{
				if (AttrsGetRef.FormattedBitmap == value) return;
				AttrsSetRef.FormattedBitmap = value;
				_setFormattedBitmapCount++;
			}
			get { return AttrsGetRef.FormattedBitmap; }
		}
		static int _setFormattedBitmapCount;

/// <summary>
/// Virtual method to Get/set numeric value of the MobiusDataType (noop here)
/// </summary>

		public virtual double NumericValue
		{
			get
			{
				return NullValue.NullNumber;
			}

			set
			{
				return; // noop
			}
		}

		/// <summary>
		/// Virtual method to Get/set the DateTime value of the MobiusDataType (noop here)
		/// </summary>

		public virtual DateTime DateTimeValue
		{
			get
			{
				return DateTime.MinValue;
			}

			set
			{
				return; // noop
			}
		}

		/////////////////////////////////////////////////////////////////
		//////////////////////////// Methods ////////////////////////////
		/////////////////////////////////////////////////////////////////

/// <summary>
/// Create a Mobius data type based on the supplied MetaColumn type
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static MobiusDataType New(
			MetaColumnType type)
		{
			MobiusDataType o;

			if (type == MetaColumnType.Integer) o = new NumberMx();
			else if (type == MetaColumnType.Number) o = new NumberMx();
			else if (type == MetaColumnType.QualifiedNo) o = new QualifiedNumber();
			else if (type == MetaColumnType.String) o = new StringMx();
			else if (type == MetaColumnType.Date) o = new DateTimeMx();
			else if (type == MetaColumnType.Binary) o = new StringMx();
			else if (type == MetaColumnType.CompoundId) o = new CompoundId();
			else if (type == MetaColumnType.Structure) o = new MoleculeMx();
			else if (type == MetaColumnType.MolFormula) o = new StringMx();
			else if (type == MetaColumnType.Image) o = new ImageMx();
			else if (type == MetaColumnType.DictionaryId) o = new NumberMx();
			else if (type == MetaColumnType.Hyperlink) o = new StringMx();
			else if (type == MetaColumnType.Html) o = new StringMx();
			else throw new Exception("Unrecognized data type: " + type);

			return o;
		}

		/// <summary>
		/// Create a Mobius data type based on the supplied MetaColumn type and an initial value
		/// </summary>
		/// <param name="type">MobiusDataType</param>
		/// <param name="o">Object to convert to type</param>
		/// <returns>MobiusDataType object</returns>

		//MobiusDataType lowVal = MobiusDataType.New(qc.MetaColumn.DataType, psc.Value);
		//MobiusDataType highVal = MobiusDataType.New(qc.MetaColumn.DataType, psc.Value2);

		public static MobiusDataType New(
			MetaColumnType type,
			object o)
		{
			if (o == null || o is DBNull) return null;

			else if (o is MobiusDataType) return (MobiusDataType)o; // if already a MobiusDataType just return existing object

// Create based on string object

			else if (o is string)
			{
				string textValue = o as string;
				switch (type)
				{
					case MetaColumnType.Integer:
					case MetaColumnType.Number:
						return new NumberMx(textValue);

					case MetaColumnType.QualifiedNo:
						return new QualifiedNumber(textValue);

					case MetaColumnType.String:
						return new StringMx(textValue);

					case MetaColumnType.Date:
						return new DateTimeMx(textValue);

					case MetaColumnType.Binary:
						return new StringMx(textValue);

					case MetaColumnType.CompoundId:
						return new CompoundId(textValue);

					case MetaColumnType.Structure:
						return new MoleculeMx(textValue);

					case MetaColumnType.Image:
						return new ImageMx(textValue); // assume text is dblink

					case MetaColumnType.MolFormula:
						return new StringMx(textValue);

					case MetaColumnType.DictionaryId:
						return new NumberMx(textValue);

					case MetaColumnType.Hyperlink:
						return new StringMx(textValue);

					case MetaColumnType.Html:
						return new StringMx(textValue);

					default:
						break;
				}
			}

// Numeric input object

			else if (o is byte || o is sbyte || 
				o is Int16 ||	o is Int32 || o is Int64 || 
				o is float || o is double || o is decimal)
			{
				double d = Convert.ToDouble(o); // convert to double first

				if (type == MetaColumnType.Integer)
					return new NumberMx(d);

				else if (type == MetaColumnType.Number)
					return new NumberMx(d);

				else if (type == MetaColumnType.QualifiedNo)
					return new QualifiedNumber(d);

				else if (type == MetaColumnType.String)
					return new StringMx(o.ToString());

				else if (type == MetaColumnType.Image)
					return new ImageMx(o.ToString()); // assume number is dblink
			}

// Date object

			else if (type == MetaColumnType.Date && o is DateTime)
					return new DateTimeMx((DateTime)o);

// Image object

			else if (type == MetaColumnType.Image && o is Bitmap)
				return new ImageMx((Bitmap)o);

// Can't handle

			throw new Exception("Unrecognized data type: " + o.GetType());
		}

/// <summary>
/// Create a new MobiusDataType based on a basic type
/// </summary>
/// <param name="o"></param>
/// <returns></returns>

		public static MobiusDataType New(
			object o)
		{
			if (o == null || o is DBNull) return null;
			else if (o is MobiusDataType) return (MobiusDataType)o;
			else if (o is int) return new NumberMx((int)o);
			else if (o is double) return new NumberMx((double)o);
			else if (o is string) return new StringMx((string)o);
			else if (o is DateTime) return new DateTimeMx((DateTime)o);
			else throw new Exception("Unrecognized data type: " + o.GetType());
		}


/// <summary>
/// Try to convert object to a MobiusDataType
/// </summary>
/// <param name="type"></param>
/// <param name="o"></param>
/// <param name="mdt"></param>
/// <returns></returns>

		public static bool TryConvertTo(
			MetaColumnType type,
			object o,
			out MobiusDataType mdt)
		{
			mdt = null;

			try
			{
				mdt = ConvertToMobiusDataType(type, o);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Convert object to a MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <param name="type"></param>
		/// <returns></returns>

		public static MobiusDataType ConvertToMobiusDataType(
			MetaColumnType type,
			Object o)
		{
			switch (type)
			{
				case MetaColumnType.Integer:
				case MetaColumnType.Number:
				case MetaColumnType.DictionaryId:
					return NumberMx.ConvertTo(o);

				case MetaColumnType.QualifiedNo: 
					return QualifiedNumber.ConvertTo(o);

				case MetaColumnType.String:
				case MetaColumnType.MolFormula:
				case MetaColumnType.Hyperlink:
				case MetaColumnType.Html:
				case MetaColumnType.Binary:
					return StringMx.ConvertTo(o);

				case MetaColumnType.Date: 
					return DateTimeMx.ConvertTo(o);

				case MetaColumnType.CompoundId: 
					return CompoundId.ConvertTo(o);

				case MetaColumnType.Structure: 
					return MoleculeMx.ConvertTo(o);

				case MetaColumnType.Image: 
					return ImageMx.ConvertTo(o); // assume text is dblink

				default: 
					throw new Exception("Unrecognized data type: " + type);
			}
		}

/// <summary>
/// Try to create the specifed MobiusDataType from a text string
/// </summary>
/// <param name="type"></param>
/// <param name="textValue"></param>
/// <param name="mdt"></param>
/// <returns></returns>

		public static bool TryParse(
			MetaColumnType type,
			string textValue,
			out MobiusDataType mdt)
		{
			try
			{
				mdt = New(type, textValue);
				if (mdt != null) return true;
				else return false;
			}
			catch (Exception ex)
			{
				mdt = null;
				return false;
			}
		}

/// <summary>
/// Get a pair of MobiusDataType objects of the specified type that can be used
/// for comparison to the specified text value. This is useful for comparing 
/// decimal numbers for equality.
/// </summary>
/// <param name="type"></param>
/// <param name="textValue"></param>
/// <param name="mdtLow"></param>
/// <param name="mdtHigh"></param>

		public static void GetFuzzyEqualityComparators(
			MetaColumnType type,
			string textValue,
			out MobiusDataType mdtLow,
			out MobiusDataType mdtHigh)
		{
			try
			{
				mdtLow = MobiusDataType.New(type, textValue);
				mdtHigh = MobiusDataType.New(type, textValue);

				if (MetaColumn.IsDecimalMetaColumnType(type))
				{
					double e = GetEpsilon(textValue);
					mdtLow.NumericValue -= e;
					mdtHigh.NumericValue += e;
				}
			}
			catch (Exception ex)
			{
				mdtLow = MobiusDataType.New(type);
				mdtHigh = MobiusDataType.New(type);
			}

			return;
		}

		/// <summary>
		/// Get epsilon value for fuzzy numeric comparisons = .5 * 10**(-decimals)
		/// where decimals is determined from a numeric value in string form
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static double GetEpsilon(string s)
		{
			int decimals = GetDecimals(s);
			double epsilon = .5 * Math.Pow(10, -decimals);
			return epsilon;
		}

/// <summary>
/// Get epsilon value for fuzzy numeric comparisons = .5 * 10**(-decimals)
/// </summary>
/// <param name="decimals"></param>
/// <returns></returns>

		public static double GetEpsilon(int decimals)
		{
			double epsilon = .5 * Math.Pow(10, -decimals);
			return epsilon;
		}

		/// <summary>
		/// Get number of decimals in number string 
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static int GetDecimals(string s)
		{
			int i1 = s.LastIndexOf(".");
			if (i1 < 0) return 0;
			int decimals = s.Length - (i1 + 1);
			return decimals;
		}

		/// <summary>
		/// Compare two objects 
		/// </summary>
		/// <param name="dataType"></param>
		/// <param name="o1"></param>
		/// <param name="o2"></param>
		/// <returns></returns>

		public static int Compare (
			object o1,
			object o2)
		{
			if (o1 == null || o2 == null) throw new ArgumentNullException("Null argument(s)");

			Type t1 = o1.GetType();
			Type t2 = o2.GetType();

			IComparable c1 = o1 as IComparable;
			if (c1 == null) throw new InvalidDataException("Type doesn't support IComparable: " + t1);

			IComparable c2 = o2 as IComparable;
			if (c2 == null) throw new InvalidDataException("Type doesn't support IComparable: " + t2);

			if (t1.Equals(t2)) return c1.CompareTo(c2); // same type?
			else if (t1.IsAssignableFrom(t2)) return c1.CompareTo(c2); // t1 subclass of t2
			else if (t2.IsAssignableFrom(t1)) return -c2.CompareTo(c1); // t2 subclass of t1 (reverse order)

			else if (NumberEx.IsNumber(o1) && NumberEx.IsNumber(o2)) // two primitive numbers?
			{
				double d1 = unchecked((double)o1); // convert to double ignoring any overflow
				double d2 = unchecked((double)o2);
				return d1.CompareTo(d2);
			}

			else // see if either arg is a MobiusDataType
			{
				Type mdt = typeof(MobiusDataType);
				if (mdt.IsAssignableFrom(t1)) return c1.CompareTo(c2); // t1 is MobiusDataType

				else if (mdt.IsAssignableFrom(t2)) return -c2.CompareTo(c1); // t2 is MobiusDataType (reverse order)

				else throw new ArgumentException("Types not comparable: " + t1 + ", " + t2);
			}
		}

		/// <summary>
		/// Get hash code
		/// (Must be implemented in subclasses)
		/// </summary>
		/// <returns></returns>

		public override int GetHashCode()
		{
			throw new Exception("GetHashCode cannot be called for a base MobiusDataType");
		}

		/// <summary>
		/// Compare this MobiusDataType and another for equality 
		/// (Must be implemented in subclasses)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>

		public override bool Equals(object obj)
		{
			throw new Exception("Equals cannot be called for a base MobiusDataType");
		}

		/// <summary>
		/// Compare two MobiusDataTypes numbers (IComparable.CompareTo)
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public virtual int CompareTo(
			object o)
		{
			throw new Exception("CompareTo cannot be called for a base MobiusDataType");
		}

		/// <summary>
		/// Return true if null value, implemented by each subtype
		/// </summary>

		public virtual bool IsNull
		{
			get { return true; } // always null
		}

		/// <summary>
		/// Return true if column is sortable
		/// </summary>

		public virtual bool IsSortable
		{
			get { return true; } // sortable by default
		}

		/// <summary>
		/// Return true if column normally has a graphical representation
		/// </summary>

		public virtual bool IsGraphical
		{
			get { return false; } // not graphical by default
		}

/// <summary>
/// Return the normalized string version of the MobiusDataType
/// </summary>
/// <returns></returns>

		public virtual string FormatForCriteria()
		{
			return ToString(); // default behavior
		}

/// <summary>
/// Return true if object is a MobiusDataType
/// </summary>
/// <param name="o"></param>
/// <returns></returns>

		public static bool IsMobiusDataType(object o)
		{
			if (o == null) return false;
			else return o.GetType().IsSubclassOf(typeof(MobiusDataType));
		}

		/// <summary>
		/// Convert a metacolumn type to the object type used to store it
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static Type MetaColumnTypeToClassType(
			MetaColumnType mcType)
		{
			if (mcType == MetaColumnType.Integer) return typeof(Int32);
			else if (mcType == MetaColumnType.Number) return typeof(double);
			else if (mcType == MetaColumnType.QualifiedNo) return typeof(QualifiedNumber);
			else if (mcType == MetaColumnType.CompoundId) return typeof(CompoundId);
			else if (mcType == MetaColumnType.String) return typeof(string);
			else if (mcType == MetaColumnType.Date) return typeof(DateTime);
			else if (mcType == MetaColumnType.Structure) return typeof(MoleculeMx);
			return typeof(string); // map other types to string for now
		}

/// <summary>
/// Convert a MobiusDataType class type to the associated primitive type
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static Type ConvertToPrimitiveType(Type type)
		{
			if (!type.IsSubclassOf(typeof(MobiusDataType))) return type; // return as is if not a MobiusDataType
			else if (type.Equals(typeof(CompoundId))) return typeof(string);
			else if (type.Equals(typeof(MoleculeMx))) return typeof(string);
			else if (type.Equals(typeof(StringMx))) return typeof(string);
			else if (type.Equals(typeof(NumberMx))) return typeof(double);
			else if (type.Equals(typeof(QualifiedNumber))) return typeof(double);
			else if (type.Equals(typeof(DateTimeMx))) return typeof(DateTime);
			else if (type.Equals(typeof(ImageMx))) return typeof(string);
			else throw new Exception("Unrecognized type: " + type.Name);
		}

/// <summary>
/// Convert a MobiusDataType object to the associated primitive type
/// </summary>
/// <param name="vo"></param>
/// <returns></returns>

		public static object ConvertToPrimitiveValue(object vo)
		{
			if (vo == null || vo is DBNull || // return as is if not MobiusDataType
				vo.GetType().IsPrimitive || vo is string || vo is DateTime ||
				!vo.GetType().IsSubclassOf(typeof(MobiusDataType)))
				return vo;

			else // return associated primitive type
			{
				MobiusDataType mdt = vo as MobiusDataType;
				return mdt.ToPrimitiveType();
			}
		}

		/// <summary>
		/// Convert MobiusDataType value or to a primitive type or just return value if not a MDT
		/// (does this add anything over the ConvertToPrimitiveValue method above?)
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="mc"></param>
		/// <returns></returns>

		public static object ConvertToPrimitiveValue(
			object vo,
			MetaColumn mc)
		{
			MobiusDataType mdt;
			DateTimeMx dex;
			string cid;
			int iVal;
			double dVal, d2;
			object value = null;

			if (NullValue.IsNull(vo)) return null;

			if (mc.DataType == MetaColumnType.CompoundId)
			{
				cid = vo.ToString();
				if (mc.IsNumeric)
				{
					if (int.TryParse(cid, out iVal))
						value = iVal;
				}

				else value = cid;
			}

			else if (mc.IsNumeric)
			{
				mdt = vo as MobiusDataType;
				if (mdt != null)
					dVal = mdt.NumericValue;
				else if (vo is int) dVal = (int)vo;
				else if (vo is double) dVal = (double)vo;
				else if (vo is string && double.TryParse((string)vo, out d2))
					dVal = d2;
				else throw new Exception("Unexpected type: " + vo.GetType());

				if (mc.DataType == MetaColumnType.Integer)
					value = (int)dVal;
				else value = dVal;
			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				dex = vo as DateTimeMx;
				if (dex != null) value = dex.Value.Date; // ignore time component
				else if (vo is DateTime) value = ((DateTime)vo).Date;
				else throw new Exception("Unexpected type: " + vo.GetType());
			}

			else value = vo.ToString(); // convert everything else to a string

			return value;
		}

		/// <summary>
		/// Convert the value to the nearest primitive type
		/// </summary>
		/// <returns></returns>

		public virtual object ToPrimitiveType()
		{
			throw new Exception("ToPrimitiveType cannot be called for a base MobiusDataType"); 
		}

/// <summary>
/// Delegate for formatting hyperlink
/// </summary>
/// <param name="qc"></param>
/// <param name="mdt"></param>
/// <returns></returns>

		public delegate string FormatHyperlinkDelegate(
					QueryColumn qc,
					MobiusDataType mdt);

		public static FormatHyperlinkDelegate FormatHyperlinkDelegateInstance;

		/// <summary>
		/// Format hyperlink for result
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>
		/// 

		public string FormatHyperlink(
			QueryColumn qc)
		{
			if (FormatHyperlinkDelegateInstance != null)
				return FormatHyperlinkDelegateInstance(qc, this);

			else return "";
		}

/// <summary>
/// Return a formatted string for the MobiusDataType
/// </summary>
/// <returns></returns>

		public virtual string FormatCriteriaForDisplay()
		{
			return ToString(); // default behavior
		}

/// <summary>
/// Custom compact serialization of common MobiusDataType attributes
/// </summary>
/// <returns></returns>

		public virtual StringBuilder Serialize()
		{
			throw new NotImplementedException(); // implemented by subtypes
		}

		/// <summary>
		/// Custom compact Deserialization of MobiusDataType
		/// </summary>
		/// <returns></returns>

		public static MobiusDataType Deserialize(string[] sa)
		{
			int sai;

			MobiusDataType mdt = null;
			char type = sa[0][0];

			sai = 5; // first array entry for specific subclass

			switch (type)
			{
				case 'M': // ChemicalStructure (Molecule)
					mdt = MoleculeMx.Deserialize(sa, sai);
					break;

				case 'C': // CompoundId
					mdt = CompoundId.Deserialize(sa, sai);
					break;

				case 'D': // DateTimeEx
					mdt = DateTimeMx.Deserialize(sa, sai);
					break;

				case 'I': // ImageEx
					mdt = ImageMx.Deserialize(sa, sai);
					break;

				case 'N': //NumberEx
					mdt = NumberMx.Deserialize(sa, sai);
					break;

				case 'Q': // QualifiedNumber
					mdt = QualifiedNumber.Deserialize(sa, sai);
					break;

				case 'S': // StringEx
					mdt = StringMx.Deserialize(sa, sai);
					break;

				default:
					throw new Exception("Unexpected MobiusDataType: " + type);
			}

			if (!Lex.IsNullOrEmpty(sa[1]))
				mdt.BackColor = Color.FromArgb(int.Parse(sa[1]));

			if (!Lex.IsNullOrEmpty(sa[2]))
				mdt.ForeColor = Color.FromArgb(int.Parse(sa[2]));

			if (!Lex.IsNullOrEmpty(sa[3]))
				mdt.DbLink = sa[3];

			if (!Lex.IsNullOrEmpty(sa[4]))
				mdt.Hyperlink = sa[4];

			return mdt;
		}

		/// <summary>
		/// Begin compact serialization MobiusDataType
		/// </summary>
		/// <returns></returns>

		public StringBuilder BeginSerialize(string type)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<");
			sb.Append(type);
			sb.Append(",");
			if (BackColor != DefaultBackColor)
				sb.Append(BackColor.ToArgb().ToString());
			sb.Append(",");

			if (ForeColor != DefaultAttrValues.ForeColor)
				sb.Append(ForeColor.ToArgb().ToString());
			sb.Append(",");

			if (!Lex.IsNullOrEmpty(DbLink))
				sb.Append(NormalizeForSerialize(DbLink));
			sb.Append(",");

			if (!Lex.IsNullOrEmpty(Hyperlink))
				sb.Append(NormalizeForSerialize(Hyperlink));
			sb.Append(",");

			return sb;
		}

/// <summary>
/// Normalize a string for serialization purposes replacing and &, < and > chars
/// </summary>
/// <param name="s"></param>
/// <returns></returns>

		public static string NormalizeForSerialize(string s)
		{
			if (s == null) return s;
			if (s.Contains("&")) s = s.Replace("&", "&amp;");
			if (s.Contains("<")) s = s.Replace("<", "&lt;");
			if (s.Contains(">")) s = s.Replace(">", "&gt;");
			if (s.Contains(",")) s = s.Replace(",", "&c;");
			return s;
		}

/// <summary>
/// Denormalize a string for deserialization purposes replacing and &amp;, &lt; and &gt; strings
/// </summary>
/// <param name="s"></param>
/// <returns></returns>
			
		public static string DenormalizeForDeserialize(string s)
		{
			if (s.Contains("&c;")) s = s.Replace("&c;", ",");
			if (s.Contains("&gt;")) s = s.Replace("&gt;", ">");
			if (s.Contains("&lt;")) s = s.Replace("&lt;", "<");
			if (s.Contains("&amp;")) s = s.Replace("&amp;", "&");
			return s;
		}

/// <summary>
/// Binary serialize a single Mobius data type object
/// </summary>
/// <param name="mdtObject"></param>
/// <returns></returns>

		public static byte[] SerializeBinarySingle(
			MobiusDataType mdtObject)
		{
			if (mdtObject == null) return new byte[0];

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			VoArray.WriteBinaryItem(mdtObject, bw);
		
			bw.Flush();
			byte[] ba = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(ba, 0, (int)ms.Length);
			bw.Close();

			return ba;
		}

		/// <summary>
		/// Binary serialize a single Mobius data type object
		/// </summary>
		/// <param name="mdtObject"></param>
		/// <returns></returns>

		public static MobiusDataType DeserializeBinarySingle(
			byte[] ba)
		{
			if (ba == null) return null;

			MemoryStream ms = new MemoryStream(ba);
			BinaryReader br = new BinaryReader(ms);

			object obj = VoArray.ReadBinaryItem(br);
			MobiusDataType mdt = obj as MobiusDataType;

			br.Close();
			return mdt;
		}

		/// <summary>
		/// Serialize base of Mobius Data Type
		/// </summary>
		/// <param name="br"></param>

		public virtual void SerializeBinary(BinaryWriter bw)
		{
			if (IsNonExistant) // write nonexistant if nonexistant value is set
				bw.Write(NonExistantValueFlag);

			if (BackColor != DefaultBackColor)
			{
				bw.Write(true);
				bw.Write(BackColor.ToArgb());
			}
			else bw.Write(false);

			if (ForeColor != DefaultAttrValues.ForeColor)
			{
				bw.Write(true);
				bw.Write(ForeColor.ToArgb());
			}
			else bw.Write(false);

			if (DbLink != "")
			{
				bw.Write(true);
				bw.Write(Lex.S(DbLink));
			}
			else bw.Write(false);

			if (Hyperlink != "")
			{
				bw.Write(true);
				bw.Write(Lex.S(Hyperlink));
			}
			else bw.Write(false);
		}

/// <summary>
/// Deserialize base of Mobius Data Type
/// </summary>
/// <param name="br"></param>
/// <param name="mdt"></param>

		public static void DeserializeBinary(BinaryReader br, MobiusDataType mdt)
		{
			int nonExistant = br.PeekChar(); 
			if (nonExistant == NonExistantValueFlag)
			{
				mdt.IsNonExistant = true;
				nonExistant = br.ReadByte();
			}

			if (br.ReadBoolean()) mdt.BackColor = Color.FromArgb(br.ReadInt32());
			if (br.ReadBoolean()) mdt.ForeColor = Color.FromArgb(br.ReadInt32());
			if (br.ReadBoolean()) mdt.DbLink = br.ReadString();
			if (br.ReadBoolean()) mdt.Hyperlink = br.ReadString();
		}

/// <summary>
/// Make a memberwise copy of this object onto another existing MobiusDataType
/// </summary>
/// <param name="dest"></param>

		public void MemberwiseCopy(MobiusDataType dest)
		{
			dest._flags = _flags; // copy all flags
			dest.DbLink = DbLink;
			dest.BackColor = BackColor;

			if (MdtExAttr == null) dest.MdtExAttr = null;
			else
			{
				dest.MdtExAttr = new MdtExtendedAttributes();
				MdtExAttr.MemberwiseCopy(dest.MdtExAttr);
			}
			return;
		}

/// <summary>
/// Clone
/// </summary>
/// <returns></returns>

		public MobiusDataType Clone()
		{
			MobiusDataType mdt = (MobiusDataType)this.MemberwiseClone();
			if (mdt.MdtExAttr != null) mdt.MdtExAttr = MdtExAttr.Clone();
			return mdt;
		}

	}

	/// <summary>
	/// Mdt attributes that may or may not be defined (~16 bytes plus any strings/bitmap)
	/// Uses less memory if not defined
	/// </summary>

	[Serializable]
	public class MdtExtendedAttributes
	{

		public Color ForeColor = Color.Black;
		public string FormattedText = null;
		public Bitmap FormattedBitmap = null;
		public string Hyperlink = ""; 

		/// <summary>
		/// Make a memberwise copy
		/// </summary>
		/// <param name="dest"></param>

		public void MemberwiseCopy(MdtExtendedAttributes dest)
		{
			dest.ForeColor = ForeColor;
			dest.FormattedBitmap = FormattedBitmap;
			dest.FormattedText = FormattedText;
			dest.Hyperlink = Hyperlink;
			return;
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public new MdtExtendedAttributes Clone()
		{
			MdtExtendedAttributes exAttr = (MdtExtendedAttributes)this.MemberwiseClone();
			return exAttr;
		}
	}

}
