using Mobius.ComOps;

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Mobius.Data
{
	/// <summary>
	/// String with extended with Mobius attributes
	/// </summary>

  [Serializable]
	public class ImageMx : MobiusDataType
	{
		/// <summary>
		/// String value
		/// </summary>

		[DataMember]
		public Bitmap Value = null;

		/// <summary>
		/// Caption to be displayed above image
		/// </summary>

		[DataMember]
		public string Caption = "";

/// <summary>
/// Basic constructor
/// </summary>

		public ImageMx()
		{
			return;
		}

/// <summary>
/// Construct from Bitmap
/// </summary>
/// <param name="value"></param>

		public ImageMx(
			Bitmap value)
		{
			Value = value;
		}

/// <summary>
/// Construct from string dblink
/// </summary>
/// <param name="dbLink"></param>

		public ImageMx(
			string dbLink)
		{
			DbLink = dbLink;
		}

		/// <summary>
		/// Convert the specified object to the corresponding MobiusDataType
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>

		public static ImageMx ConvertTo(
			object o)
		{
			if (o is ImageMx) return (ImageMx)o;
			else if (NullValue.IsNull(o)) return new ImageMx();
			else return new ImageMx(o.ToString()); // create image comtaining the link info
		}

		/// <summary>
		/// Return true if null value
		/// </summary>

		[XmlIgnore]
		public override bool IsNull
		{
			get
			{
				if (Value == null && String.IsNullOrEmpty(DbLink) && String.IsNullOrEmpty(Hyperlink))
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Return true if column is sortable
		/// </summary>

		[XmlIgnore]
		public override bool IsSortable
		{
			get { return false; }
		}

		/// <summary>
		/// Return true if column normally has a graphical representation
		/// </summary>

		[XmlIgnore]
		public override bool IsGraphical
		{
			get { return true; }
		}

		/// <summary>
		/// Convert the value to the nearest primitive type
		/// </summary>
		/// <returns></returns>

		public override object ToPrimitiveType()
		{
			return "Image";
		}
		/// <summary>
		/// Custom compact serialization
		/// </summary>
		/// <returns></returns>

		public override StringBuilder Serialize()
		{
			if (IsNull) return new StringBuilder("<I>");
			StringBuilder sb = BeginSerialize("I");
			if (Value != null) // have bitmap
			{
				MemoryStream stream = new MemoryStream();
				Value.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
				Byte[] bytes = stream.ToArray();
				string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
				sb.Append(NormalizeForSerialize(encodedData));
			}

			sb.Append(",");

			if (!String.IsNullOrEmpty(Caption))
				sb.Append(NormalizeForSerialize(Caption));
			sb.Append(">");
			return sb;
		}

		/// <summary>
		/// Custom Compact deserialization
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="sai"></param>
		/// <returns></returns>

		public static ImageMx Deserialize(string[] sa, int sai)
		{
			ImageMx ix = new ImageMx();

			if (!Lex.IsNullOrEmpty(sa[0]))
			{
				string s = sa[0];
				int p = s.IndexOf(':');
				int length = Convert.ToInt32(s.Substring(0, p));
				byte[] memorydata = Convert.FromBase64String(s.Substring(p + 1));
				MemoryStream rs = new MemoryStream(memorydata, 0, length);
				ix.Value = new Bitmap(rs);
			}

			if (!Lex.IsNullOrEmpty(sa[1]))
				ix.Caption = sa[1];
			return ix;
		}

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <param name="bw"></param>

		public override void SerializeBinary(BinaryWriter bw)
		{
			bw.Write((byte)VoDataType.ImageMx);
			base.SerializeBinary(bw);

			if (Value != null)
			{
				bw.Write(true);
				MemoryStream stream = new MemoryStream();
				Value.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
				Byte[] bytes = stream.ToArray();
				bw.Write(bytes.Length);
				bw.Write(bytes);
			}
			else bw.Write(false);

			bw.Write(Lex.S(Caption));
		}

		/// <summary>
		/// Binary Deserialize
		/// </summary>
		/// <param name="br"></param>
		/// <returns></returns>

		public static ImageMx DeserializeBinary(BinaryReader br)
		{
			ImageMx ix = new ImageMx();
			MobiusDataType.DeserializeBinary(br, ix);

			if (br.ReadBoolean())
			{
				int len = br.ReadInt32();
				byte[] bytes = br.ReadBytes(len);
				MemoryStream ms = new MemoryStream(bytes, 0, len);
				ix.Value = new Bitmap(ms);
			}

			ix.Caption = br.ReadString();

			return ix;
		}

		/// <summary>
		/// Clone 
		/// </summary>
		/// <returns></returns>

		public new ImageMx Clone()
		{
			ImageMx iMx = (ImageMx)this.MemberwiseClone();
			if (iMx.MdtExAttr != null) iMx.MdtExAttr = MdtExAttr.Clone();
			return iMx;
		}

	}
}
