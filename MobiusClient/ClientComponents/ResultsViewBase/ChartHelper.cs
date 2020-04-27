//using DevExpress.XtraCharts;
using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Helper for fast marker drawing
	/// </summary>

	public class MarkerHelper
	{
		MarkerAttributes MA; // current marker attributes
		Bitmap Bitmap; // current Bitmap
		Dictionary<MarkerAttributes, Bitmap> // dictionary of already built bitmaps
			Dict = new Dictionary<MarkerAttributes, Bitmap>();

		internal static int MarkerCoordsFullWidthHeight = 308; // max width/height of all markers

		internal static int[][] MarkerCoords = { // centered set of coords for each marker type
			new int[] {-109,109, 109,109, 109,-109, -109,-109},
			new int[] {-154,0, 0,154, 154,0, 0,-154},
			new int[] {-133,116, 134,116, 0,-115},
			new int[] {-133,-115, 134,-115, 0,116},
			new int[] {-154,0, 0,154, 154,0, 0,-154},
			new int[] {-155,62, -62,62, -62,155, 62,155, 62,62, 155,62, 155,-62, 62,-62, 62,-155, -62,-155, -62,-62, -155,-62},
			new int[] {-66,154, 0,88, 66,154, 154,66, 88,0, 154,-66, 66,-154, 0,-88, -66,-154, -154,-66, -88,0, -154,66},
			new int[] {-90,139, 0,82, 91,140, 63,36, 146,-32, 40,-37, 0,-139, -39,-38, -145,-33, -63,36},
			new int[] {-91,149, 91,149, 146,-22, 0,-148, -146,-24},
			new int[] {-133,75, 0,152, 134,75, 134,-79, 0,-152, -133,-80}};


			//new int[] {0,218, 218,218, 218,0, 0,0}, // Square = 0,
			//new int[] {45,216, 199,370, 353,216, 199,62}, // Diamond = 1,
			//new int[] {53,276, 320,276, 186,45}, // Triangle = 2,
			//new int[] {53,48, 320,48, 186,279}, // InvertedTriangle = 3,
			//new int[] {45,216, 199,370, 353,216, 199,62}, // Circle = 4 (just diamond coords indicating the width & height),
			//new int[] {0,217, 93,217, 93,310, 217,310, 217,217, 310,217, 310,93, 217,93, 217,0, 93,0, 93,93, 0,93}, // Plus = 5,
			//new int[] {88,308, 154,242, 220,308, 308,220, 242,154, 308,88, 220,0, 154,66, 88,0, 0,88, 66,154, 0,220}, // Cross = 6,
			//new int[] {92,297, 182,240, 273,298, 245,194, 328,126, 222,121, 182,19, 143,120, 37,125, 119,194}, // Star = 7,
			//new int[] {133,321, 315,321, 370,150, 224,24, 78,148}, // Pentagon = 8,
			//new int[] {44,254, 177,331, 311,254, 311,100, 177,27, 44,99}}; // Hexagon = 9,

/// <summary>
/// Render a marker
/// </summary>
/// <param name="kind"></param>
/// <param name="width">In Pixels</param>
/// <param name="height">In Pixels</param>
/// <param name="color"></param>
/// <param name="borderColor"></param>
/// <param name="point"></param>
/// <param name="g"></param>

		public void RenderMarker(

			MarkerKind kind,
			int width,
			int height,
			Color color,
			Color borderColor,
			Point point,
			Graphics g)
		{
			//DateTime t0 = DateTime.Now;

			//g.InterpolationMode = InterpolationMode.Low; // these values should be set for faster rendering
			//g.SmoothingMode = SmoothingMode.None;
			//g.PixelOffsetMode = PixelOffsetMode.None;
			//g.CompositingQuality = CompositingQuality.HighSpeed;
			//g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

			//color = Color.Red; // debug

			if (width < 2) width = 2; // minimum size
			if (height < 2) height = 2; 

			if (kind == MA.Kind && width == MA.Width && height == MA.Height &&
			 color == MA.Color && borderColor == MA.BorderColor) { }
			else
			{
				MA = new MarkerAttributes(kind, (short)width, (short)height, color, borderColor);
				if (Dict.ContainsKey(MA)) Bitmap = Dict[MA];
				else Bitmap = CreateBitmap(MA, g);
			}

			int dx = Bitmap.Width / 2; // center bitmap
			int dy = Bitmap.Height / 2; 
			g.DrawImageUnscaled(Bitmap, point.X - dx, point.Y - dy);

			//GraphicsHelper.BitBlt(point.X, point.Y, size, size, Bitmap, g); // slower

			//double ms = TimeOfDay.Delta(t0);
			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ma"></param>
		/// <param name="controlGraphics"></param>

		Bitmap CreateBitmap(
			MarkerAttributes ma,
			Graphics controlGraphics)
		{
			bool drawBorder;
			Color borderColor;
			Pen borderPen = null;
			int dx, dy;

			Bitmap bmp = new Bitmap(ma.Width+1, ma.Height+1, PixelFormat.Format32bppPArgb); // bitmap to hold image

			int width = ma.Width;
			if (width < 4) width = 4;

			int height = ma.Height;
			if (height < 4) height = 4;

			int xCenter = width / 2;
			int yCenter = height / 2;
			Graphics imageGraphics = Graphics.FromImage(bmp);

			double xScale = (double)width / MarkerCoordsFullWidthHeight; // scale from Marker coords to pixels
			double yScale = (double)height / MarkerCoordsFullWidthHeight; // scale from Marker coords to pixels

			int[] coords = MarkerCoords[(int)ma.Kind];

			drawBorder = (ma.BorderColor != Color.Transparent);
			if (drawBorder) // set up pen for border
			{
				if (ma.BorderColor == Color.Black) // if selected color then use as is
					borderColor = Color.Black;
				else borderColor = GetMatchingBorderColor(ma.Color); // otherwise get a border color to match the marker color
				borderPen = new Pen(borderColor);
			}

			if (ma.Kind == MarkerKind.Square)
			{
				dx = (int)(Math.Abs(coords[0]) * xScale);
				dy = (int)(Math.Abs(coords[1]) * yScale);
				Rectangle rt = new Rectangle(xCenter - dx, yCenter - dy, dx * 2, dy * 2);

				if (ma.Color != Color.Transparent)
				{
					Brush brush = new SolidBrush(ma.Color);
					imageGraphics.FillRectangle(brush, rt);
					if (drawBorder)
						imageGraphics.DrawRectangle(borderPen, rt);
				}
			}

			else if (ma.Kind == MarkerKind.Circle)
			{
				dx = (int)(Math.Abs(coords[0]) * xScale);
				dy = (int)(Math.Abs(coords[0]) * yScale);
				Rectangle rt = new Rectangle(xCenter - dx, yCenter - dy, dy * 2, dy * 2);

				if (ma.Color != Color.Transparent)
				{
					Brush brush = new SolidBrush(ma.Color);
					imageGraphics.FillEllipse(brush, rt);
					if (drawBorder)
						imageGraphics.DrawEllipse(borderPen, rt);
				}
			}

			else // polygon
			{
				Point[] points = new Point[coords.Length / 2];
				for (int pi = 0; pi < coords.Length / 2; pi++)
				{
					double d = xCenter + coords[pi * 2] * xScale;
					int x = d >= 0 ? (int)(d + .5) : (int)(d - .5);

					d = yCenter + coords[pi * 2 + 1] * yScale;
					int y = d >= 0 ? (int)(d + .5) : (int)(d - .5);

					points[pi] = new Point(x, y);
				}

				if (ma.Color != Color.Transparent)
				{
					Brush brush = new SolidBrush(ma.Color);
					imageGraphics.FillPolygon(brush, points);

					if (drawBorder)
						imageGraphics.DrawPolygon(borderPen, points);
				}

				//bmp.Save(@"c:\download\test.bmp");
			}

			Dict[ma] = bmp; // store image in dict
			return bmp;
		}

/// <summary>
/// Get matching border color that is a bit darker in color
/// </summary>
/// <param name="color"></param>
/// <returns></returns>

		Color GetMatchingBorderColor(Color color)
		{
			int r = color.R * 75 / 100;
			int g = color.G * 75 / 100;
			int b = color.B * 75 / 100;
			Color borderColor = Color.FromArgb(r, g, b);
			return borderColor;
		}

		/// <summary>
		/// Normalize coords so that center of each marker is at 0,0
		/// </summary>

		string NormalizeMarkerCoords()
		{
			int x, y;
			string txt = "";

			for (int mi = 0; mi < MarkerCoords.Length; mi++)
			{
				int[] coords = MarkerCoords[mi];

				int minx = 1000000, maxx = -1000000, miny = 1000000, maxy = -1000000;
				for (int ci = 0; ci < coords.Length; ci += 2)
				{
					x = coords[ci];
					y = coords[ci + 1];
					minx = Math.Min(minx, x);
					maxx = Math.Max(maxx, x);
					miny = Math.Min(miny, y);
					maxy = Math.Max(maxy, y);
				}

				x = (int)(minx + (maxx - minx) / 2.0); // center
				y = (int)(miny + (maxy - miny) / 2.0);

				txt += "new int[] {"; // 0,218, 218,218, 218,0, 0,0}, // Square = 0,

				for (int ci = 0; ci < coords.Length; ci += 2)
				{
					coords[ci] -= x;
					coords[ci + 1] -= y;
					if (ci > 0) txt += ", ";
					txt += coords[ci].ToString() + "," + coords[ci + 1];
				}

				txt += "},\r\n";
			}

			MarkerCoordsFullWidthHeight = -MarkerCoordsFullWidthHeight; // indicate normalized
			return txt;
		}

/// <summary>
/// Get scaleup necessary to make square marker fill the full marker area
/// </summary>
/// <returns></returns>

		public static double RectangularMarkerRelativeSize()
		{
			double squareWidth = Math.Abs(MarkerCoords[(int)MarkerKind.Square][1]) * 2;
			double ratio = squareWidth / Math.Abs(MarkerCoordsFullWidthHeight);
			return ratio;
		}
	}

	/// <summary>
	/// The attributes of a marker
	/// </summary>

	public struct MarkerAttributes
	{
		public MarkerKind Kind; // type
		public short Width; // width in pixels
		public short Height; // height in pixels
		public Color Color; // fill color
		public Color BorderColor; // border color

		public MarkerAttributes(
			MarkerKind kind,
			short width,
			short height,
			Color color,
			Color borderColor)
		{
			Kind = kind;
			Width = width;
			Height = height;
			Color = color;
			BorderColor = borderColor;
			return;
		}
	}

/// <summary>
/// DiagramToPointHelper (From DexExpress sample code)
/// </summary>

	public class DiagramToPointHelper
	{
		const double epsilon = 0.001;

		public static Rectangle CalculateDiagramBounds(XYDiagram diagram)
		{
			try
			{
				Point p1 = diagram.DiagramToPoint((double)diagram.AxisX.VisualRange.MinValue, (double)diagram.AxisY.VisualRange.MinValue).Point;
				Point p2 = diagram.DiagramToPoint((double)diagram.AxisX.VisualRange.MaxValue, (double)diagram.AxisY.VisualRange.MaxValue).Point;
				return CreateRectangle(p1, p2);
			}

			catch (Exception ex) // catch any type error in MinValue, MaxValue objects
			{ return new Rectangle(); }
		}

		static PointF CalcRandomPoint(Random random, int xCenter, int yCenter)
		{
			const int dispersion = 2;
			const int expectedSum = 6;
			PointF point = new PointF();
			double sum = 0;
			for (int i = 0; i < 12; i++)
				sum += random.NextDouble();
			double radius = (sum - expectedSum) * dispersion;
			double angle = random.Next(360) * Math.PI / 180;
			point.X = (float)(xCenter + radius * Math.Cos(angle));
			point.Y = (float)(yCenter + radius * Math.Sin(angle));
			return point;
		}
		static bool AreEqual(PointF point1, PointF point2)
		{
			return AreEqual(point1.X, point2.X) && AreEqual(point1.Y, point2.Y);
		}
		static bool AreEqual(double number1, double number2)
		{
			double difference = number1 - number2;
			if (Math.Abs(difference) <= epsilon)
				return true;
			return false;
		}
		static PointF GetClusterCenter(List<PointF> cluster)
		{
			if (cluster.Count == 0)
				return PointF.Empty;
			float centerX = 0;
			float centerY = 0;
			foreach (PointF point in cluster)
			{
				centerX += point.X;
				centerY += point.Y;
			}
			centerX /= cluster.Count;
			centerY /= cluster.Count;
			return new PointF(centerX, centerY);
		}
		static void CreateUpperArc(List<PointF> cluster, List<PointF> sortedCluster)
		{
			for (int i = 1; i < cluster.Count; i++)
			{
				if (i + 1 == cluster.Count)
				{
					sortedCluster.Add(cluster[i]);
					break;
				}
				bool shouldAddPoint = false;
				float x0 = sortedCluster[sortedCluster.Count - 1].X;
				float y0 = sortedCluster[sortedCluster.Count - 1].Y;
				float x1 = cluster[i].X;
				float y1 = cluster[i].Y;
				if (x1 == x0)
				{
					if (y0 < y1)
						shouldAddPoint = true;
				}
				else
					for (int j = i + 1; j < cluster.Count; j++)
					{
						if (cluster[j].Y >= (double)(cluster[j].X - x0) * (double)(y1 - y0) / (double)(x1 - x0) + y0)
						{
							shouldAddPoint = false;
							break;
						}
						else
							shouldAddPoint = true;
					}
				if (shouldAddPoint)
					sortedCluster.Add(cluster[i]);
			}
		}
		static void CreateBottomArc(List<PointF> cluster, List<PointF> sortedCluster)
		{
			for (int i = cluster.Count - 1; i >= 0; i--)
			{
				if (i == 0)
				{
					sortedCluster.Add(cluster[i]);
					break;
				}
				bool shouldAddPoint = false;
				float x0 = sortedCluster[sortedCluster.Count - 1].X;
				float y0 = sortedCluster[sortedCluster.Count - 1].Y;
				float x1 = cluster[i].X;
				float y1 = cluster[i].Y;
				if (x1 == x0)
				{
					if (y0 > y1)
						shouldAddPoint = true;
				}
				else
					for (int j = i - 1; j >= 0; j--)
					{
						if (cluster[j].Y <= (double)(cluster[j].X - x0) * (double)(y1 - y0) / (double)(x1 - x0) + y0)
						{
							shouldAddPoint = false;
							break;
						}
						else
							shouldAddPoint = true;
					}
				if (shouldAddPoint)
					sortedCluster.Add(cluster[i]);
			}
		}
		public static Rectangle CreateRectangle(Point corner1, Point corner2)
		{
			int x = corner1.X < corner2.X ? corner1.X : corner2.X;
			int y = corner1.Y < corner2.Y ? corner1.Y : corner2.Y;
			int width = Math.Abs(corner1.X - corner2.X);
			int height = Math.Abs(corner1.Y - corner2.Y);
			return new Rectangle(x, y, width, height);
		}

		public static RectangleF CreateRectangle(PointF corner1, PointF corner2)
		{
			float x = corner1.X < corner2.X ? corner1.X : corner2.X;
			float y = corner1.Y < corner2.Y ? corner1.Y : corner2.Y;
			float width = Math.Abs(corner1.X - corner2.X);
			float height = Math.Abs(corner1.Y - corner2.Y);
			return new RectangleF(x, y, width, height);
		}

		public static Point GetLastSelectionCornerPosition(Point p, Rectangle bounds)
		{
			if (p.X < bounds.Left)
				p.X = bounds.Left;
			else if (p.X > bounds.Right)
				p.X = bounds.Right - 1;
			if (p.Y < bounds.Top)
				p.Y = bounds.Top;
			else if (p.Y > bounds.Bottom)
				p.Y = bounds.Bottom - 1;
			return p;
		}

		public static SeriesPoint[] CalculatePoints(Random random, int count, int xCenter, int yCenter)
		{
			SeriesPoint[] seriesPoints = new SeriesPoint[count];
			for (int i = 0; i < count; i++)
			{
				PointF point = CalcRandomPoint(random, xCenter, yCenter);
				seriesPoints[i] = new SeriesPoint(point.X, new double[] { point.Y });
			}
			return seriesPoints;
		}
		public static void CalculateClusters(SeriesPointCollection seriesPoints, List<PointF> cluster1, List<PointF> cluster2, List<PointF> cluster3)
		{
			List<PointF> points = new List<PointF>();
			foreach (SeriesPoint point in seriesPoints)
				points.Add(new PointF((float)point.NumericalArgument, (float)point.Values[0]));
			if (points.Count < 100)
				return;
			PointF nextCenter1 = points[0];
			PointF nextCenter2 = points[50];
			PointF nextCenter3 = points[100];
			PointF center1;
			PointF center2;
			PointF center3;
			do
			{
				center1 = nextCenter1;
				center2 = nextCenter2;
				center3 = nextCenter3;
				cluster1.Clear();
				cluster2.Clear();
				cluster3.Clear();
				foreach (PointF point in points)
				{
					float x = point.X;
					float y = point.Y;
					double distance1 = Math.Sqrt((center1.X - x) * (center1.X - x) + (center1.Y - y) * (center1.Y - y));
					double distance2 = Math.Sqrt((center2.X - x) * (center2.X - x) + (center2.Y - y) * (center2.Y - y));
					double distance3 = Math.Sqrt((center3.X - x) * (center3.X - x) + (center3.Y - y) * (center3.Y - y));
					if (distance1 <= distance2 && distance1 <= distance3)
						cluster1.Add(point);
					else if (distance2 <= distance1 && distance2 <= distance3)
						cluster2.Add(point);
					else
						cluster3.Add(point);
				}
				nextCenter1 = GetClusterCenter(cluster1);
				nextCenter2 = GetClusterCenter(cluster2);
				nextCenter3 = GetClusterCenter(cluster3);
			} while (!AreEqual(center1, nextCenter1) || !AreEqual(center2, nextCenter2) || !AreEqual(center3, nextCenter3));
		}

		public static List<PointF> Sort(List<PointF> cluster)
		{
			List<PointF> sortedCluster = new List<PointF>();
			if (cluster.Count == 0)
				return sortedCluster;
			sortedCluster.Add(cluster[0]);
			for (int i = 1; i < cluster.Count; i++)
			{
				if (sortedCluster[0].X >= cluster[i].X)
					sortedCluster.Insert(0, cluster[i]);
				else if (sortedCluster[sortedCluster.Count - 1].X <= cluster[i].X)
					sortedCluster.Insert(sortedCluster.Count, cluster[i]);
				else
					for (int j = 0; j < sortedCluster.Count - 1; j++)
					{
						if (sortedCluster[j].X <= cluster[i].X && sortedCluster[j + 1].X >= cluster[i].X)
						{
							sortedCluster.Insert(j + 1, cluster[i]);
							break;
						}
					}
			}
			return sortedCluster;
		}

		public static List<PointF> CreateClosedCircuit(List<PointF> sortedCluster)
		{
			List<PointF> contourPoints = new List<PointF>();
			if (sortedCluster.Count == 0)
				return contourPoints;
			contourPoints.Add(sortedCluster[0]);
			CreateUpperArc(sortedCluster, contourPoints);
			CreateBottomArc(sortedCluster, contourPoints);
			return contourPoints;
		}
	}

	/// <summary>
	/// Cluster
	/// </summary>

	public class Cluster
	{
		List<PointF> points = new List<PointF>();
		List<PointF> sortedPoints = new List<PointF>();
		List<PointF> contourPoints = new List<PointF>();
		bool isClusterSelected = false;

		public List<PointF> Points { get { return points; } }
		public List<PointF> SortedPoints { get { return sortedPoints; } }
		public List<PointF> ContourPoints { get { return contourPoints; } }
		public bool IsClusterSelected { get { return isClusterSelected; } set { isClusterSelected = value; } }

		public void Calculate(List<PointF> points)
		{
			this.points = points;
			sortedPoints = DiagramToPointHelper.Sort(points);
			contourPoints = DiagramToPointHelper.CreateClosedCircuit(sortedPoints);
			isClusterSelected = false;
		}
		public void Clear()
		{
			points.Clear();
			sortedPoints.Clear();
			contourPoints.Clear();
			isClusterSelected = false;
		}
	}

/// <summary>
/// GraphicsHelper
/// </summary>

	public static class GraphicsHelper
{
/// <summary>
/// StretchBlt
/// </summary>

		public static void StretchBlt(this Graphics graphics, Bitmap image, Rectangle rectangleDst, int nXSrc, int nYSrc, int nWidth, int nHeight)
		{
			IntPtr hdc = graphics.GetHdc();
			IntPtr memdc = GdiInterop.CreateCompatibleDC(hdc);
			IntPtr bmp = image.GetHbitmap();
			GdiInterop.SelectObject(memdc, bmp);
			GdiInterop.SetStretchBltMode(hdc, 0x04);
			GdiInterop.StretchBlt(hdc, rectangleDst.Left, rectangleDst.Top, rectangleDst.Width, rectangleDst.Height, memdc, nXSrc, nYSrc, nWidth, nHeight,
				GdiInterop.TernaryRasterOperations.SRCCOPY);
			GdiInterop.DeleteObject(bmp);
			GdiInterop.DeleteDC(memdc);
			graphics.ReleaseHdc(hdc);
		}

/// <summary>
/// BitBlt
/// </summary>

		public static void BitBlt(int x, int y, int width, int height, Bitmap bmpSrc, Graphics gDest)
		{
			IntPtr hdc = gDest.GetHdc();

			IntPtr memdc = GdiInterop.CreateCompatibleDC(hdc);
			IntPtr bmp = GdiInterop.CreateCompatibleBitmap(hdc, bmpSrc.Width, bmpSrc.Height);
			GdiInterop.SelectObject(memdc, bmp);

			Graphics gMem = Graphics.FromHdc(memdc);
			gMem.DrawImage(bmpSrc, 0, 0);
			IntPtr hMemDC = gMem.GetHdc();
			GdiInterop.BitBlt(hdc, x, y, width, height, hMemDC, 0, 0, GdiInterop.TernaryRasterOperations.SRCCOPY);

			//GdiInterop.BitBlt(hdc, x, y, width, height, memdc, 0, 0, GdiInterop.TernaryRasterOperations.SRCCOPY);

			GdiInterop.DeleteObject(bmp);
			GdiInterop.DeleteDC(memdc);
			//gMem.ReleaseHdc(hMemDC);
			gDest.ReleaseHdc(hdc);
		}

		/// <summary>
		/// ConvertMediaColorToDrawingColor
		/// </summary>
		/// <param name="mediacolor"></param>
		/// <returns></returns>

		//public static System.Drawing.Color ConvertMediaColorToDrawingColor(
		//	System.Windows.Media.Color mediaColor)
		//{
		//	System.Drawing.Color color =
		//		System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
		//	return color;
		//}

		/// <summary>
		/// ConvertDrawingColorToMediaColor
		/// </summary>
		/// <param name="drawingColor"></param>
		/// <returns></returns>

		//public static System.Windows.Media.Color ConvertDrawingColorToMediaColor(
		//	System.Drawing.Color drawingColor)
		//{
		//	System.Windows.Media.Color mediaColor =
		//		System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
		//	return mediaColor;
		//}

}

	public class GdiHelper
	{
		public void Draw(Control control)
		{
			Graphics memDC;
			Bitmap memBmp;
			memBmp = new Bitmap(control.Width, control.Height);

			Graphics clientDC = control.CreateGraphics();
			IntPtr hdc = clientDC.GetHdc();
			IntPtr memdc = GdiInterop.CreateCompatibleDC(hdc);
			GdiInterop.SelectObject(memdc, memBmp.GetHbitmap());
			memDC = Graphics.FromHdc(memdc);
			clientDC.ReleaseHdc(hdc);
		}

		public void Draw2(Control control)
		{
			Graphics memDC;
			Bitmap memBmp;
			memBmp = new Bitmap(control.Width, control.Height);

			//Graphics clientDC = control.CreateGraphics();

			Graphics clientDC = control.CreateGraphics();
			IntPtr hdc = clientDC.GetHdc();
			IntPtr memdc = GdiInterop.CreateCompatibleDC(hdc);

			GdiInterop.SelectObject(memdc, memBmp.GetHbitmap());
			memDC = Graphics.FromHdc(memdc);
			IntPtr hMemdc = memDC.GetHdc();

			// do drawing in memDC 

			// do drawing in memDC 

			// do drawing in memDC 

			// transfer the bits from memDC to clientDC 

			GdiInterop.BitBlt(hdc, 0, 0, control.Width, control.Height,
					hMemdc, 0, 0, GdiInterop.TernaryRasterOperations.SRCCOPY);

			clientDC.ReleaseHdc(hdc);
			memDC.ReleaseHdc(hMemdc);
		}

/// <summary>
/// Create a bitmap with the specified size and transparent color
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
/// <param name="transparentColor"></param>
/// <returns></returns>

		public static Bitmap CreateBitmap(
			int width,
			int height,
			Color transparentColor)
		{
			Bitmap bitmap = new Bitmap(width, height);
			bitmap.MakeTransparent(transparentColor);
			return bitmap;
		}

/// <summary>
/// Get a Graphics DC compatible with a control
/// </summary>
/// <param name="control"></param>
/// <param name="bitmap"></param>

		public static Graphics GetCompatibleGraphicsDC(
			Control control,
			Bitmap bitmap)
		{
			Graphics clientDC = control.CreateGraphics();
			IntPtr hdc = clientDC.GetHdc();
			IntPtr memdc = GdiInterop.CreateCompatibleDC(hdc);
			GdiInterop.SelectObject(memdc, bitmap.GetHbitmap());
			Graphics graphicsDC = Graphics.FromHdc(memdc);
			clientDC.ReleaseHdc(hdc);
			return graphicsDC;
		}
	}

/// <summary>
/// Methods for calling GDI directly
/// </summary>

	public class GdiInterop
	{
		/// <summary>
		/// Enumeration for the raster operations used in BitBlt.
		/// In C++ these are actually #define. But to use these
		/// constants with C#, a new enumeration _type is defined.
		/// </summary>
		/// 
		public enum TernaryRasterOperations
		{
			SRCCOPY = 0x00CC0020, // dest = source
			SRCPAINT = 0x00EE0086, // dest = source OR dest
			SRCAND = 0x008800C6, // dest = source AND dest
			SRCINVERT = 0x00660046, // dest = source XOR dest
			SRCERASE = 0x00440328, // dest = source AND (NOT dest)
			NOTSRCCOPY = 0x00330008, // dest = (NOT source)
			NOTSRCERASE = 0x001100A6, // dest = (NOT src) AND (NOT dest)
			MERGECOPY = 0x00C000CA, // dest = (source AND pattern)
			MERGEPAINT = 0x00BB0226, // dest = (NOT source) OR dest
			PATCOPY = 0x00F00021, // dest = pattern
			PATPAINT = 0x00FB0A09, // dest = DPSnoo
			PATINVERT = 0x005A0049, // dest = pattern XOR dest
			DSTINVERT = 0x00550009, // dest = (NOT dest)
			BLACKNESS = 0x00000042, // dest = BLACK
			WHITENESS = 0x00FF0062, // dest = WHITE
		};

		/// <summary>
		/// Enumeration to be used for those Win32 function 
		/// that return BOOL
		/// </summary>
		public enum Bool
		{
			False = 0,
			True
		};

		/// <summary>
		/// Sets the background color.
		/// </summary>
		/// <param name="hdc">The HDC.</param>
		/// <param name="crColor">Color of the cr.</param>
		/// <returns></returns>
		[DllImport("gdi32.dll")]
		public static extern int SetBkColor(IntPtr hdc, int crColor);

		/// <summary>
		/// CreateCompatibleDC
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		/// <summary>
		/// DeleteDC
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern Bool DeleteDC(IntPtr hdc);

		/// <summary>
		/// SelectObject
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true)]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		/// <summary>
		/// DeleteObject
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern Bool DeleteObject(IntPtr hObject);

		/// <summary>
		/// CreateCompatibleBitmap
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hObject, int width, int height);

		/// <summary>
		/// BitBlt
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern Bool BitBlt(
			IntPtr hObject,
			int nXDest,
			int nYDest,
			int nWidth,
			int nHeight,
			IntPtr hObjSource,
			int nXSrc, int nYSrc,
			TernaryRasterOperations dwRop);

		/// <summary>
		/// TransparentBlt
		/// </summary>
		/// 
		[DllImport("gdi32.dll", EntryPoint = "GdiTransparentBlt")]
		public static extern int TransparentBlt(
			[In] SafeHandle hdcDest,
			int xoriginDest,
			int yoriginDest,
			int wDest,
			int hDest,
			[In] SafeHandle hdcSrc,
			int xoriginSrc,
			int yoriginSrc,
			int wSrc,
			int hSrc,
			UInt32 crTransparent);


		/// <summary>
		/// StretchBlt
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern Bool StretchBlt(IntPtr hObject, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hObjSource, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, TernaryRasterOperations dwRop);

		/// <summary>
		/// SetStretchBltMode
		/// </summary>
		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern Bool SetStretchBltMode(IntPtr hObject, int nStretchMode);

	}

}
