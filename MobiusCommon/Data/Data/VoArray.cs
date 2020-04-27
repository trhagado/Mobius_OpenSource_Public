using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Globalization;
using System.IO;

namespace Mobius.Data
{
/// <summary>
/// Data row value object array manipulations
/// </summary>

	public class VoArray
	{

		/// <summary>
		/// Set vo with specified QueryColumn name
		/// </summary>
		/// <param name="qt"></param>
		/// <param name="colName"></param>
		/// <param name="vo"></param>
		/// <param name="voi"></param>
		/// <param name="value"></param>
		/// <param name="withException"></param>

		public static void SetVo(
			QueryTable qt,
			string colName,
			object[] vo,
			object value)
		{
			bool withException = false; // always false for now

			QueryColumn qc;

			if (withException)
				qc = qt.GetQueryColumnByNameWithException(colName);

			else
			{
				qc = qt.GetQueryColumnByName(colName);
				if (qc == null) return;
			}

			int voPos = qc.VoPosition;

			SetVo(vo, voPos, value);
			return;
		}

		/// <summary>
		/// Set vo at specified position
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="voi"></param>
		/// <param name="value"></param>

		public static void SetVo(
			object[] vo,
			int voi,
			object value)
		{
			if (vo == null || voi < 0 || voi >= vo.Length) return;

			if ((value is int && (int)value == NullValue.NullNumber) ||
			 (value is long && (long)value == NullValue.NullNumber) ||
			 (value is double && (double)value == NullValue.NullNumber) ||
			 (value is DateTime && ((DateTime)value).Equals(DateTime.MinValue)))
				vo[voi] = null;

			else vo[voi] = value;

			return;
		}

		/// <summary>
		/// Deserialize a string format DataRow value object array
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>

		public static object[] DeserializeText(
			string content)
		{
			string[] sa;
			string vos;
			int voi = 0, sai = 0;

			int cnt = Lex.CountCharacter(content, '>');
			object[] oa = new object[cnt];
			int ltbrk = 0, rtbrk = 0;

			try
			{
				for (voi = 0; voi < cnt; voi++)
				{
					for (rtbrk = ltbrk + 1; rtbrk < content.Length; rtbrk++)
					{
						if (content[rtbrk] == '>') break;
					}

					vos = content.Substring(ltbrk + 1, rtbrk - ltbrk - 1);
					ltbrk = rtbrk + 1; // move to next
					if (vos.Length == 0) continue; // null

					sa = vos.Split(',');

					for (sai = 0; sai < sa.Length; sai++) // convert back to original form
						sa[sai] = MobiusDataType.DenormalizeForDeserialize(sa[sai]);

					char type = sa[0][0];
					switch (type)
					{
						case 's': // string
							oa[voi] = sa[1];
							break;

						case 'i': // integer
							oa[voi] = int.Parse(sa[1]);
							break;

						case 'f': // double
							oa[voi] = double.Parse(sa[1]);
							break;

						case 'd': // date time - stored as tick count
							long ticks = long.Parse(sa[1]);
							oa[voi] = new DateTime(ticks);
							break;

						default:
							oa[voi] = MobiusDataType.Deserialize(sa);
							break;
					}

				}

				//if (oa[0] == "02507049") oa = oa; // debug

			}

			catch (Exception ex)
			{
				string msg = "Deserialization error at position: " + voi + ", " + sai + " for string:\n\n" +
					content + "\n\n" + DebugLog.FormatExceptionMessage(ex);

				//ServicesLog.Message(msg);
				throw new Exception(msg, ex);
			}

			return oa;
		}

		/// <summary>
		/// Serialize a DataRow to a StringBuilder object
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>

		public static StringBuilder SerializeToText(
			object[] oa,
			int startPos,
			int length)
		{
			StringBuilder sb = new StringBuilder();

			ConvertMoleculesToChimeStrings(oa);

			for (int vi = startPos; vi < startPos + length; vi++)
			{
				object o = oa[vi];
				if (NullValue.IsNull(o))
					sb.Append("<>");

				else if (o is MobiusDataType)
				{
					MobiusDataType mdt = o as MobiusDataType;
					sb.Append(mdt.Serialize());
				}

				else
				{
					sb.Append("<");
					if (o is string)
					{
						sb.Append('s');
						o = MobiusDataType.NormalizeForSerialize(o.ToString());
					}
					else if (o is int) sb.Append('i');
					else if (o is float) sb.Append('f');
					else if (o is double) sb.Append('f');
					else if (o is decimal) sb.Append('f');
					else if (o is DateTime)
					{ // write datetime as number of ticks
						sb.Append('d');
						o = ((DateTime)o).Ticks;
					}
					else throw new Exception("Invalid type: " + o.GetType());
					sb.Append(',');
					sb.Append(o);
					sb.Append(">");
				}

			}

			return sb;
		}

		/// <summary>
		/// Convert any ChemicalStructures containing a sketch value to a ChimeString for proper serialization
		/// </summary>
		/// <param name="oa"></param>

		public static void ConvertMoleculesToChimeStrings(object[] oa)
		{
			for (int ci = 0; ci < oa.Length; ci++)
			{
				if (!(oa[ci] is MoleculeMx)) continue;
				MoleculeMx cs = oa[ci] as MoleculeMx;
				if (Math.Abs(1) == 1) continue; // noop for now
				//if (cs.Type != StructureFormat.Sketch) continue; // obsolete
				MoleculeFormat type = MoleculeFormat.Chime;
				string value = cs.GetChimeString();

				cs.SetPrimaryTypeAndValue(type, value);
			}
		}

		/// <summary>
		/// Get extended types that XmlSerializer needs to be aware of
		/// </summary>
		/// <returns></returns>

		public static Type[] GetExtendedTypes()
		{
			Type[] extendedTypes = new Type[] { // list additional types that may be encountered
								typeof(DBNull),
								typeof(MobiusDataType),
								typeof(CompoundId), 
								typeof(MoleculeMx),
								typeof(QualifiedNumber),
								typeof(NumberMx),
								typeof(DateTimeMx),
								typeof(StringMx),
								typeof(ImageMx)	};

			return extendedTypes;
		}

/// <summary>
/// Serialize a VO array list in a compact form and quickly
/// </summary>
/// <param name="vo"></param>
/// <returns></returns>

		public static byte[] SerializeBinaryVoArrayListToByteArray(
			List<object[]> voList,
			bool includeListHeader = true)
		{
			DateTime t0 = DateTime.Now;
			int voLen = 0;

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			if (includeListHeader)
			{
				bw.Write(voList.Count); // count of list elements
				if (voList.Count > 0)
					voLen = voList[0].Length;
				bw.Write(voLen); // length of each vo
			}

			for (int li = 0; li < voList.Count; li++)
			{
				object[] voa = voList[li];
				WriteBinaryVoArray(voa, bw, 0);
			}

			bw.Flush();
			byte[] ba = new byte[ms.Length];
			ms.Seek(0, 0);
			ms.Read(ba, 0, (int)ms.Length);
			bw.Close();

			double et = TimeOfDay.Delta(t0);
			return ba;
		}

/// <summary>
/// Serialize a single vo array
/// </summary>
/// <param name="voa"></param>
/// <param name="bw"></param>

		public static void WriteBinaryVoArray(
			object[] voa,
			BinaryWriter bw,
			int offset)
		{
			for (int voi = offset; voi < voa.Length; voi++)
			{
				WriteBinaryItem(voa[voi], bw);
			}
		}

/// <summary>
/// Serialize a single value
/// </summary>
/// <param name="vo"></param>
/// <param name="bw"></param>

		public static void WriteBinaryItem(
			object vo, 
			BinaryWriter bw)
		{
			if (vo == null)
			{
				bw.Write((byte)VoDataType.Null);
			}

			else if (vo is DBNull)
			{
				bw.Write((byte)VoDataType.DbNull);
			}

			else if (vo is int)
			{
				bw.Write((byte)VoDataType.Int32);
				bw.Write((int)vo);
			}

			else if (vo is string)
			{
				bw.Write((byte)VoDataType.String);
				bw.Write(Lex.S((string)vo));
			}

			else if (vo is double)
			{
				bw.Write((byte)VoDataType.Double);
				bw.Write((double)vo);
			}

			else if (vo is MobiusDataType)
			{
				(vo as MobiusDataType).SerializeBinary(bw);
			}

			else if (vo is DateTime)
			{
				bw.Write((byte)VoDataType.DateTime);
				DateTime dt = (DateTime)vo;
				bw.Write(dt.Ticks); // ticks is a long value
			}

			else if (vo is byte)
			{
				bw.Write((byte)VoDataType.Byte);
				bw.Write(Lex.S((string)vo));
			}

			else if (vo is char)
			{
				bw.Write((byte)VoDataType.Char);
				bw.Write((char)vo);
			}

			else if (vo is Int16)
			{
				bw.Write((byte)VoDataType.Int16);
				bw.Write((Int16)vo);
			}

			else if (vo is Int64)
			{
				bw.Write((byte)VoDataType.Int64);
				bw.Write((Int64)vo);
			}

			else if (vo is float)
			{
				bw.Write((byte)VoDataType.Float);
				bw.Write((float)vo);
			}

			else throw new Exception("Unexpected type: " + vo.GetType());

			return;
		}

/// <summary>
/// Deserialize byte array into a list of vo arrays
/// </summary>
/// <param name="ba"></param>
/// <returns></returns>

		public static List<object[]> DeserializeByteArrayToVoArrayList(byte[] content)
		{
			if (content == null) return null;

			DateTime t0 = DateTime.Now;
			List<object[]> voList = new List<object[]>();

			MemoryStream ms = new MemoryStream(content);
			BinaryReader br = new BinaryReader(ms);
			int rows = br.ReadInt32(); // number of rows
			int voLen = br.ReadInt32(); // number of vo elements per row

			for (int ri = 0; ri < rows; ri++)
			{
				object[] oa = ReadBinaryVoArray(br, voLen);
				voList.Add(oa);
			}

			double tms = TimeOfDay.Delta(t0);
			return voList;
		}

/// <summary>
/// Deserialize a single vo array
/// </summary>
/// <returns></returns>
/// 
		public static object[] ReadBinaryVoArray(
			BinaryReader br,
			int voLen)
		{
			object[] vo = new object[voLen];

			for (int voi = 0; voi < voLen; voi++)
			{
				vo[voi] = ReadBinaryItem(br);
			}

			return vo;
		}

/// <summary>
/// Deserialize a single value object
/// </summary>
/// <param name="ba"></param>
/// <returns></returns>

		public static object ReadBinaryItem(
			BinaryReader br)
		{
			object vo = null;
			VoDataType voType = (VoDataType)br.ReadByte();

			switch (voType)
			{
				case VoDataType.Null:
					return null;

				case VoDataType.DbNull:
					return DBNull.Value;

				case VoDataType.Byte:
					return br.ReadByte();

				case VoDataType.Int16:
					return br.ReadInt16();

				case VoDataType.Int32:
					return br.ReadInt32();

				case VoDataType.Int64:
					return br.ReadInt64();

				case VoDataType.Float:
					return br.ReadSingle();

				case VoDataType.Double:
					return br.ReadDouble();

				case VoDataType.Char:
					return br.ReadChar();

				case VoDataType.String:
					return br.ReadString();

				case VoDataType.DateTime:
					return new DateTime(br.ReadInt64());

				case VoDataType.CompoundId:
					return CompoundId.DeserializeBinary(br);

				case VoDataType.NumberMx:
					return NumberMx.DeserializeBinary(br);

				case VoDataType.QualifiedNo:
					return QualifiedNumber.DeserializeBinary(br);

				case VoDataType.StringMx:
					return StringMx.DeserializeBinary(br);

				case VoDataType.DateTimeMx:
					return DateTimeMx.DeserializeBinary(br);

				case VoDataType.ImageMx:
					return ImageMx.DeserializeBinary(br);

				case VoDataType.ChemicalStructure:
					return MoleculeMx.DeserializeBinary(br);

				default:
					throw new Exception("Unrecognized type: " + voType);
			}
		}
	}

/// <summary>
/// Data types that can appear in DataRow value objects and be properly serialized/deserialized
/// </summary>

	public enum VoDataType
	{
		Null = 0,
		DbNull = 1,
		Byte = 2,
		Int16 = 3,
		Int32 = 4,
		Int64 = 5,
		Float = 6,
		Double = 7,
		Char = 8,
		String = 9,
		DateTime = 10,
		CompoundId = 11,
		NumberMx = 12,
		QualifiedNo = 13,
		StringMx = 14,
		DateTimeMx = 15,
		ImageMx = 16,
		ChemicalStructure = 17,
		ChemicalStructureAppendix = 18
	}
}
