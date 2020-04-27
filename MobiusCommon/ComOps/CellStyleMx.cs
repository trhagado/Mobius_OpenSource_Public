using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Mobius CellStype class
	/// </summary>

	public class CellStyleMx 
	{
		public Font Font = null;
		public Color ForeColor = Color.Black;
		public Color BackColor = Color.Empty;

		public CellStyleMx()
		{
		}

/// <summary>
/// Construct with supplied value
/// </summary>
/// <param name="font"></param>
/// <param name="foreColor"></param>
/// <param name="backColor"></param>
		
		public CellStyleMx (
			Font font,
			Color foreColor,
			Color backColor)
		{
			this.Font = (Font)font.Clone();
			this.ForeColor = foreColor;
			this.BackColor = backColor;
		}

/// <summary>
/// Serialize the XlCellStyle
/// </summary>
/// <returns></returns>
		public string Serialize ()
		{
			string txt;

			txt =
				Font.FontFamily.Name + " " +
				Font.Size.ToString() + " " +
				((int)Font.Style).ToString() + " " +
				ForeColor.ToArgb().ToString() + " " +
				BackColor.ToArgb().ToString();

			return txt;
		}

/// <summary>
/// Deserialize the style
/// </summary>
/// <param name="serializedForm"></param>
/// <returns></returns>
		
		public static CellStyleMx Deserialize (
			string serializedForm)
		{
			CellStyleMx cs = new CellStyleMx();
			string [] sa = serializedForm.Split(' ');

			cs.Font = new Font(sa[0],Single.Parse(sa[1]),(FontStyle)Int32.Parse(sa[2]));
			cs.ForeColor = Color.FromArgb(Int32.Parse(sa[3]));
			cs.BackColor = Color.FromArgb(Int32.Parse(sa[4]));

			return cs;
		}

/// <summary>
/// If the supplied text contains a CellStyle tag, remove it from 
/// the text and return the associated cell style.
/// </summary>
/// <param name="txt"></param>
/// <returns></returns>

		public static CellStyleMx ExtractTag (
			ref string txt) // see if any cell style tag
		{
			CellStyleMx xlcs;
			int i1,i2;

			string tag = "<CellStyle ";
			i1 = txt.IndexOf(tag);
			if (i1 < 0) return null;
 
			i2 = txt.Substring(i1).IndexOf(">");
			if (i2 < 0) return null;
 
			string tok = txt.Substring(i1 + tag.Length, i2 - tag.Length); // get serialized style info
			if (i1>0)	txt = txt.Substring(0, i1) + txt.Substring(i1 + i2 + 1);
			else txt = txt.Substring(i1 + i2 + 1);
			xlcs = CellStyleMx.Deserialize(tok);

			return xlcs;
		}

	}

}
