using Mobius.Data;
using Mobius.ComOps;
using Mobius.ServiceFacade;

using DevExpress.XtraTab;

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Utility Mobius graphics functions
	/// </summary>
	
	public class GraphicsMx
	{
		static int logicalPixelsX = -1;
		static int logicalPixelsY = -1;

		public Graphics G; // associated .net Graphics object
		public string MetaFileName;

		string FontName;
		float FontSize;
		bool FontBold;
		bool FontItalic;
		Brush CurrentBrush = new SolidBrush(Color.Black);
		Font CurrentFont;

		int PageWidth;
		int PageHeight;
		PageMargins PageMargins;

		[System.Runtime.InteropServices.DllImport("Gdi32.dll")]
		static extern IntPtr CreateDC(string lpszDriver, string lpszDeviceName, string lpszOutput, IntPtr devMode);

		[System.Runtime.InteropServices.DllImport("Gdi32.dll")]
		static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

		const int LOGPIXELSX = 88; // code to get logical pixels in X direction
		const int LOGPIXELSY = 90; // code to get logical pixels in Y direction

/******************************************************************************/
/* Simulated graphics routines for fonts & text
/******************************************************************************/

		// Character metric data for Arial, Courier New and Times New Roman fonts

		static short [,] tmMetric = // [3,5] array of height, ascent, descent, ilead, elead
	{ 
		{ 1107,  893,  214,  165,   33 },
		{ 1071,  797,  274,  184,    0 },
		{ 1119,  893,  226,  200,   42 }};

		static short [,] tmWidth = { // [3][96] array of char widths starting with space=32
			{
				278, 278, 353, 555, 555, 889, 667, 190, 333, 333, 389, 583, 278, 333, 278, 278,
				555, 555, 555, 555, 555, 555, 555, 555, 555, 555, 278, 278, 583, 583, 583, 555,
				1016, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 555, 833, 722, 778,
				667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 468, 555,
				333, 555, 555, 500, 555, 555, 278, 555, 555, 222, 222, 500, 222, 833, 555, 555,
				555, 555, 333, 500, 278, 555, 500, 722, 500, 500, 500, 333, 250, 333, 583, 000
			},
			{
				599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 
				599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 
				599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 
				599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599,
				599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599,
				599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 599, 000
			},
			{
				250, 333, 409, 500, 500, 833, 778, 175, 333, 333, 500, 563, 250, 333, 250, 278,
				500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 278, 278, 563, 563, 563, 444,
				920, 722, 667, 667, 722, 611, 555, 722, 722, 333, 389, 722, 611, 889, 722, 722,
				555, 722, 667, 555, 611, 722, 722, 944, 722, 722, 611, 329, 278, 333, 468, 500,
				333, 444, 500, 444, 500, 444, 333, 500, 500, 278, 278, 500, 278, 778, 500, 500,
				500, 500, 333, 389, 278, 500, 500, 722, 500, 500, 444, 480, 198, 480, 540, 000 
			}};

		public GraphicsMx()
		{
		}

/// <summary>
/// Get the location for the supplied rectangle to center it on the screen
/// </summary>
/// <param name="rect"></param>
/// <returns></returns>

		public static Point CenterOnScreen(Rectangle rect)
		{
			Point p = ScreenCenter;
			int x = p.X - rect.Width/2;
			int y = p.Y - rect.Height/2;
			Point pc = new Point(x, y);
			return pc;
		}

/// <summary>
/// Get screen center
/// </summary>
		public static Point ScreenCenter
		{
			get
			{
				Point p = GeometryMx.Midpoint(Screen.PrimaryScreen.Bounds);
				return p;
			}
		}

		public static GraphicsMx OpenMetafile (
			string metaFileName,
			ResultsFormat rf)
		{
			GraphicsMx g = new GraphicsMx();
			g.MetaFileName = metaFileName;
			g.G = MetaFileMx.Open(metaFileName,rf.PageBounds);
			if (g.G == null) return null;

			g.PageWidth = rf.PageWidth;
			g.PageHeight = rf.PageHeight;
			g.PageMargins = rf.PageMargins.Clone();

			g.SetFont(rf.FontName, rf.FontSize);

			return g;
		}

		public void Close() // release resources and close metafile
		{ 
			if (CurrentFont!=null) CurrentFont.Dispose();
			MetaFileMx.Close();
		}

		public void SetPageSize (
			int pageWidth,
			int pageHeight)
		{
			PageWidth = pageWidth;
			PageHeight = pageHeight;
		}

		public void SetPageColor ( 
			int color) 
		{
			return; // todo
		}

		public void Line(
			int x1, 
			int y1, 
			int x2, 
			int y2) 
		{
			y1 = (PageHeight + PageMargins.Top + PageMargins.Bottom) - y1;
			y2 = (PageHeight + PageMargins.Top + PageMargins.Bottom) - y2;

			G.DrawLine(Pens.Black,x1,y1,x2,y2);
			return;
		}

		public void SetFont( 
			string name,
			float size)
		{
			SetFont(name,size,false,false);
		}

		public void SetFont( 
			string name,
			float size,
			bool bold,
			bool italic)
		{
			int miFontSize = (int)((size * 1000.0 + 36) / 72);

			if (!Lex.Eq(name,FontName) ||
				size != FontSize || bold != FontBold || italic != FontItalic)
			{
				FontStyle style = 0;
				if (bold) style |= FontStyle.Bold;
				if (italic) style |= FontStyle.Italic;
				if (!bold & !italic) style = FontStyle.Regular;

				if (CurrentFont!=null) CurrentFont.Dispose();
				CurrentFont = new Font(name,miFontSize,style,GraphicsUnit.World);
				FontName = name;
				FontSize = size;
				FontBold = bold;
				FontItalic = italic;
			}

			return;
		}

		public void SetBrush(
			Color color)
		{
			if (color == ((SolidBrush)CurrentBrush).Color) return;
			CurrentBrush.Dispose();
			CurrentBrush = new SolidBrush(color);
		}

		public void Text (
			int x, 
			int y, 
			string text)
		{
			y = (PageHeight + PageMargins.Top + PageMargins.Bottom) - y; // adjust for inverse y coords
			y -= (int)((FontSize * 1000.0 + 36) / 72); // adjust bottom coord to top
			G.DrawString(text,CurrentFont,CurrentBrush,x,y);
			return;
		}

		public int StringWidth (
			string s)
		{
			int fontIndex,i2;
			float l;
			char c;

			if (Lex.Eq(FontName,"Arial")) fontIndex=0;
			else if (Lex.Eq(FontName,"Courier New")) fontIndex=1; 
			else if (Lex.Eq(FontName,"Times New Roman")) fontIndex=2; 
				//	else if (Lex.Eq(FontName,"Symbol")) fontIndex=3; 
			else fontIndex=0; // default to Arial

			l=0;
			for (i2=0; i2<s.Length; i2++)
			{
				c = s[i2];
				int ci = c - 32; // start with space as index zero
				if (ci<0 || ci>=96) ci = 'x' - 32 ; // character out of range
				l+= (tmWidth[fontIndex, ci] * FontSize) / 72.0f;
			}

			return (int)l;
		}

		public int CharWidth (
			char c)
		{
			return StringWidth(c.ToString());
		}

		public void FillRectangle (
			int x,
			int y,
			int width,
			int height)
		{
			y = (PageHeight + PageMargins.Top + PageMargins.Bottom) - y; // adjust for inverse y coords
			y -= (int)((FontSize * 1000.0 + 36) / 72); // adjust bottom coord to top
			G.FillRectangle(CurrentBrush,x,y,width,height);
		}

/// <summary>
/// Get logical pixels per inch of the screen in the X direction
/// </summary>

		public static int LogicalPixelsX
		{
			get 
			{
				if (logicalPixelsX <= 0)
					logicalPixelsX = DPI(LOGPIXELSX);
				return logicalPixelsX; 
			}
		}

		/// <summary>
		/// Get logical pixels per inch of the screen in the Y direction
		/// </summary>
		/// 
		public static int LogicalPixelsY
		{
			get
			{
				if (logicalPixelsY <= 0)
					logicalPixelsY = DPI(LOGPIXELSY);
				return logicalPixelsY;
			}
		}

		static int DPI(int logPixelOrientation)
		{
			IntPtr displayPointer = CreateDC("DISPLAY", null, null, IntPtr.Zero);
			return Convert.ToInt32(GetDeviceCaps(displayPointer, logPixelOrientation));
		}

		public static int ColumnsToMilliinches (
			float columnCount)
		{
			return (int)(columnCount * 75.0f); // assume for 9pt Courier
		}

		public static float MilliinchesToColumns (
			int pixels)
		{
			return pixels / 75.0f; // assume for 9pt Courier
		}

		public static int PixelsToMilliinches (
			int pixelCount)
		{
			return (int)(pixelCount/(float)GraphicsMx.LogicalPixelsX * 1000.0 + .5);
		}

		public static int MilliinchesToPixels (
			int milliinches)
		{
			return (int)(milliinches/1000.0 * GraphicsMx.LogicalPixelsX + .5);
		}

		public static float PixelsToPoints(
			int pixelCount)
		{
			return (int)(pixelCount / (float)GraphicsMx.LogicalPixelsX * 72.0 + .5);
		}

		public static int PointsToPixels(
			float points)
		{
			return (int)(points / 72.0 * GraphicsMx.LogicalPixelsX + .5);
		}

		public static int TwipsToMilliinches(
			int twips)
		{
			return (int)(twips / 1.440 + .5);
		}

		public static int MilliinchesToTwips (
			int milliinches)
		{
			return (int)(milliinches * 1.440 + .5);
		}

	}
}
