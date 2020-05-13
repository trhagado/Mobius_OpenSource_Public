using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Svg;
using Svg.Transforms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Class containg utility code for manipulating SVG graphics.
	/// </summary>

	public class SvgUtil
	{
		public static bool Debug = false;

		/// <summary>
		/// Compress a string to a GZipped Base64String
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string CompressSvgString(string s)
		{
			return GZip.CompressToString(s);
		}

		public static bool IsSvgString(
			string s)
		{
			string svg;

			if (TryGetSvgXml(s, out svg))
				return true;

			else return false;
		}

		/// <summary>
		/// Verify that the supplied string is either SVG or compressed SVG and return the uncompressed form
		/// </summary>
		/// <param name="s"></param>
		/// <param name="svgXml"></param>
		/// <returns></returns>

		public static bool TryGetSvgXml(
			string s,
			out string svgXml)
		{
			svgXml = null;
			if (Lex.Contains(s, "<svg"))
			{
				svgXml = s;
				return true;
			}

			else return GZip.TryDecompressFromString(s, out svgXml);
		}

        /// <summary>
        /// Convert a SvgDocument to a SVG Xml string
        /// </summary>
        /// <param name="svgDoc"></param>
        /// <returns></returns>

        public static string SvgDocumentToXml(SvgDocument svgDoc)
        {
            var stream = new MemoryStream(); // convert doc 
            svgDoc.Write(stream);
            string newSvg = Encoding.UTF8.GetString(stream.GetBuffer());
            return newSvg;
        }

        /// <summary>
        /// Get bitmap from SVG XML
        /// </summary>
        /// <param name="svgXml"></param>
        /// <param name="bitmapWidth">Desired width in pixels</param>
        /// <returns></returns>

    public static Bitmap GetBitmapFromSvgXml( 
			string svgXml,
			int bitmapWidth)
		{
			RectangleF bb = new RectangleF();
			if (Lex.IsUndefined(svgXml) || bitmapWidth <= 0) return new Bitmap(1, 1); 

			try
			{
				SvgDocument doc = AdjustSvgDocumentToFitContent(svgXml, bitmapWidth, SvgUnitType.Pixel); 

				if (doc == null)
					return new Bitmap(1, 1);

				int bitmapHeight = 1;
				if (doc.Width > 0) bitmapHeight = (int)((doc.Height / doc.Width) * bitmapWidth);

				Bitmap bmp = doc.Draw(bitmapWidth, bitmapHeight); // create bitmap

				//Bitmap bmp = doc.Draw(); // create bitmap

				if (DebugMx.True && ClientState.IsDeveloper) // debug - dump xml and bitmap
					try
					{
						FileUtil.WriteFile(@"c:\downloads\SvgUtilTestXml.html", svgXml);
						bmp.Save(@"c:\downloads\SvgUtilTestBmp.bmp");
					}
					catch (Exception ex) { ex = ex; }

				return bmp;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				throw new Exception(ex.Message, ex);
			}

			//SvgAspectRatio aspectRatio = doc.AspectRatio; // aspect of the viewport.
			//RectangleF docBounds = doc.Bounds; // bounds of the svg element
			//SvgUnit docLeft = doc.X; // left position
			//SvgUnit docTop = doc.Y; // top position
			//SvgUnit docHeight = doc.Height; // height 
			//SvgUnit docWidth = doc.Width; // width

			//string matchString = "<rect x='.0' y='.0' width='242.0' height='242.0' fill='rgba(0,0,0,0.00)' stroke='none'/>";
			//xml = Lex.Replace(xml, matchString, "");

			//float xRat = (dr.Width / vb.Width); // * (br.Width / vb.Width);
			//float yRat = (dr.Height / vb.Height); // * (br.Height / vb.Height);
			//RectangleF cr = new RectangleF(br.X * xRat, br.Y * yRat, br.Width * xRat, br.Height * yRat);
			//Bitmap bmp2 = bmp.Clone(cr, bmp.PixelFormat); // extract rectangle
		}

		/// <summary>
		/// Adjust SVG bounding box to fit content and document dimensions
		/// </summary>
		/// <param name="svgXml"></param>
		/// <param name="docWidth">Desired document width</param>
		/// <param name="docUnits"></param>
		/// <param name="doc"></param>
		/// <returns></returns>

		public static string AdjustSvgToFitContent(
			string svgXml,
			float docWidth,
			SvgUnitType docUnits,
			out SvgDocument doc)
		{
			doc = AdjustSvgDocumentToFitContent(svgXml, docWidth, docUnits);
			if (doc != null) return doc.GetXML();
			else return null;
		}

		/// <summary>
		/// Adjust SVG to fit content with return of SvgDocument
		/// </summary>
		/// <param name="svgXml"></param>
		/// <param name="docWidth">Desired document width</param>
		/// <param name="docUnits"></param>
		/// <returns></returns>

		public static SvgDocument AdjustSvgDocumentToFitContent(
			string svgXml,
			float docWidth,
			SvgUnitType docUnits)
		{
			string svgXml2 = null;
			SvgDocument doc = null;
			RectangleF bb = new RectangleF();

			if (Lex.IsUndefined(svgXml)) return null;

			try
			{
				doc = SvgDocument.FromSvg<SvgDocument>(svgXml);
				SizeF dr = doc.GetDimensions(); // document dimensions in current units (e.g. pixel, inch, millimeter...)
				SvgViewBox vb = doc.ViewBox; // internal coord view box (viewport) (undefined initially)

				//SvgAspectRatio aspectRatio = doc.AspectRatio; // aspect of the viewport.
				//RectangleF docBounds = doc.Bounds; // bounds of the svg element
				//SvgUnit docLeft = doc.X; // left position
				//SvgUnit docTop = doc.Y; // top position
				//SvgUnit docHeight = doc.Height; // height 
				//SvgUnit docWidth = doc.Width; // width

				RectangleF b = doc.Bounds; // unitless bounds of the document svg elements
				float dy = 5, dx = 5; // padding around the doc elements (this is arbitrary, needs work)

				//if (b.Width > 0 && b.Height > 0) // add padding keeping aspect ration (not necessary)
				//	dx = dy * (b.Width / b.Height);

				bb = new RectangleF(b.X - dx, b.Y - dy, b.Width + 2 * dx, b.Height + 2 * dy);

				float bbLeft = bb.Left;
				float bbTop = bb.Top;
				float bbWidth = bb.Width;
				if (bbWidth <= 0) bbWidth = 1; // avoid possible zero divide
				float bbHeight = bb.Height;

				SvgViewBox vb2 = new SvgViewBox(bbLeft, bbTop, bbWidth, bbHeight); // adjust the view box to just contain the image elements
				doc.ViewBox = vb2;

				docUnits = doc.Width.Type;
				doc.Width = new SvgUnit(docUnits, docWidth);
				float docHeight = docWidth * (bbHeight / bbWidth);
				doc.Height = new SvgUnit(docUnits, docHeight);

				doc.X = new SvgUnit(docUnits, 0);
				doc.Y = new SvgUnit(docUnits, 0);

				if (DebugMx.True && ClientState.IsDeveloper) // debug - dump xml and bitmap
					try
					{
						FileUtil.WriteFile(@"c:\downloads\SvgUtilTestXml.html", svgXml);
						//bmp.Save(@"c:\downloads\SvgUtilTestBmp.bmp");
					}
					catch (Exception ex) { ex = ex; }
				return doc;
			}

			catch (Exception ex)
			{
				return null; // ignore exceptions for now
			}
		}

/// <summary>
/// Get unitless bounds of svg document 
/// </summary>
/// <param name="svgXml"></param>
/// <returns></returns>

		public static RectangleF GetSvgDocumentBounds(
			string svgXml)
		{
			SvgDocument doc = SvgDocument.FromSvg<SvgDocument>(svgXml);

			RectangleF bounds = doc.Bounds; // unitless bounds of the document svg elements
			return bounds;
		}

		/// <summary>
		/// CreateHtlmWithSvgCenteredOnPage
		/// </summary>
		/// <param name="svgString"></param>

		public static string CreateHtlmWithSvgCenteredOnPage(string svgString)
		{
			string html = // HTML template to center the SVG in the browser window
	@"
<!DOCTYPE html>
<html>
  <head>
	<title>Mobius HELM Renderer</title>
	<meta http-equiv='X-UA-Compatible' content='IE=edge' />
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
		<style> 
		
			html, body {
				height: 100%;
				margin: 0;
			}

# svg-container
			{
				display: flex;
				align-items: center;
				height: 100%;
			}

# svg-1
			{
				overflow: visible;
				margin: 0 auto;
				display: block;
			}
			
        </style> 
    </head> 
	
    <body> 
	
	<div id='svg-container'>
	<svg-place-holder>
    </div> 
    </body> 
</html>      
";

			svgString = svgString.Replace("<svg ", "<svg id='svg-1' "); // link svg to centering style info
			html = html.Replace("<svg-place-holder>", svgString); // insert in html template
			return html;
		}

			/// <summary>
			/// Test above code
			/// </summary>
			public static void Test()
		{
			string xml = FileUtil.ReadFile(@"c:\downloads\SvgTest.html");
			Bitmap bm = GetBitmapFromSvgXml(xml, 1024);
			bm.Save(@"c:\downloads\SvgTest.bmp"); // debug
			return;
		}

	}

/// <summary>
/// Calculate bounding box for SVG graphics
/// </summary>
	public class SvgBoundingBox
	{
		public float L = NumberEx.NullNumber;
		public float T = NumberEx.NullNumber;
		public float R = NumberEx.NullNumber;
		public float B = NumberEx.NullNumber;

		/// <summary>
		/// GetBoundingBox
		/// </summary>
		/// <param name="e"></param>
		/// <param name="r"></param>

		public void GetElementsBoundingBox(SvgElement e)
		{
			if (e is SvgVisualElement)
			{
				SvgVisualElement ge = e as SvgVisualElement;
				RectangleF gb = ge.Bounds;
				if (!gb.IsEmpty && !(e is SvgRectangle) && !(e is SvgGroup))
				{
					if (L == NumberEx.NullNumber)
					{
						L = gb.Left;
						T = gb.Top;
						R = gb.Right;
						B = gb.Bottom;
					}
					else
					{
						if (gb.Left < L) L = gb.Left;
						if (gb.Top < T) T = gb.Top;
						if (gb.Right > R) R = gb.Right;
						if (gb.Bottom > B) B = gb.Bottom;
					}
				}
			}

			foreach (SvgElement e2 in e.Children) // recur on children
				GetElementsBoundingBox(e2);
		}
	}

}