using System;
using System.Drawing;

namespace Mobius.ComOps
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GeometryMx
	{
		public GeometryMx()
		{
		}

/// <summary>
/// Find midpoint between two integer values
/// </summary>
/// <param name="v1"></param>
/// <param name="v2"></param>
/// <returns></returns>

		public static int Midpoint(int v1, int v2) /*  */
		{
			return v1 + (v2 - v1 + 1) / 2;
		}

/// <summary>
/// Find the midpoint of a rectangle
/// </summary>
/// <param name="rt"></param>
/// <returns></returns>

		public static Point Midpoint(Rectangle rt)
		{
			return new Point(Midpoint(rt.Left, rt.Right), Midpoint(rt.Top, rt.Bottom));
		}

/// <summary>
/// Find midpoint between two double values
/// </summary>
/// <param name="v1"></param>
/// <param name="v2"></param>
/// <returns></returns>

		public static double Midpoint(double v1, double v2) 
		{
			return v1 + (v2 - v1) / 2;
		}

		/// <summary>
		/// Convert a value within a source range to a proportionally 
		/// equivalent value within a destination range.
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="min1"></param>
		/// <param name="max1"></param>
		/// <param name="min2"></param>
		/// <param name="max2"></param>
		/// <returns></returns>

		public static double ConvertValueBetweenRanges(
			double value1,
			double min1,
			double max1,
			double min2,
			double max2)
		{
			double d = (value1 - min1) / (max1 - min1); // get value 
			double v2 = min2 + (max2 - min2) * d;
			if (v2 < min2) v2 = min2;
			if (v2 > max2) v2 = max2;
			return v2;
		}

/// <summary>
/// Test if a point is on a line segment with a specified tolerance
/// </summary>
/// <param name="p1">First end point</param>
/// <param name="p2">Second end point</param>
/// <param name="p3">Point to test</param>
/// <param name="tolerance"></param>
/// <returns></returns>

		public static bool IsPointOnLineSegment(
			PointF p1,
			PointF p2,
			PointF p3,
			float tolerance)
		{
			float minX = Math.Min(p1.X, p2.X);
			float maxX = Math.Max(p1.X, p2.X);
			float minY = Math.Min(p1.Y, p2.Y);
			float maxY = Math.Max(p1.Y, p2.Y);
			RectangleF bRect = new RectangleF(minX - tolerance, minY - tolerance, 
				maxX - minX + tolerance * 2, maxY - minY + tolerance * 2);

			if (!bRect.Contains(p3)) return false;

			double dist = PerpendicularDistance(p1, p2, p3);
			return dist < tolerance;
		}

		/// <summary>
		/// The distance of a point from a line made from point1 and point2.
		/// </summary>
		/// <param name="p1">First end point</param>
		/// <param name="p2">Second end point</param>
		/// <param name="p3">Point to test</param>
		/// <returns></returns>
		 
		public static Double PerpendicularDistance (
			PointF p1, 
			PointF p2, 
			PointF p3)
		{
			//Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
			//Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
			//Area = .5*Base*H                                          *Solve for height
			//Height = Area/.5/Base

			Double area = Math.Abs(.5 * (p1.X * p2.Y + p2.X *
			p3.Y + p3.X * p1.Y - p2.X * p1.Y - p3.X *
			p2.Y - p1.X * p3.Y));
			Double bottom = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) +
			Math.Pow(p1.Y - p2.Y, 2));
			Double height = area / bottom * 2;

			return height;

			//Another option
			//Double A = Point.X - Point1.X;
			//Double B = Point.Y - Point1.Y;
			//Double C = Point2.X - Point1.X;
			//Double D = Point2.Y - Point1.Y;

			//Double dot = A * C + B * D;
			//Double len_sq = C * C + D * D;
			//Double param = dot / len_sq;

			//Double xx, yy;

			//if (param < 0)
			//{
			//    xx = Point1.X;
			//    yy = Point1.Y;
			//}
			//else if (param > 1)
			//{
			//    xx = Point2.X;
			//    yy = Point2.Y;
			//}
			//else
			//{
			//    xx = Point1.X + param * C;
			//    yy = Point1.Y + param * D;
			//}

			//Double d = DistanceBetweenOn2DPlane(Point, new Point(xx, yy));
		}

		/// <summary>
/// Compute the distance from AB to C
/// If isSegment is true, AB is a segment, not a line.
/// </summary>
/// <param name="A"></param>
/// <param name="B"></param>
/// <param name="C"></param>
/// <param name="isSegment"></param>
/// <returns></returns>

		double LineToPointDistance(PointF A, PointF B, PointF C, bool isSegment)
		{
        double dist = CrossProduct(A,B,C) / Distance(A,B);
        if(isSegment){
					double dot1 = DotProduct(A, B, C);
            if(dot1 > 0)return Distance(B,C);
						double dot2 = DotProduct(B, A, C);
            if(dot2 > 0)return Distance(A,C);
        }
        return Math.Abs(dist);
    }

		/// <summary>
		/// Compute the distance from A to B
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns></returns>

		public static double Distance(PointF A, PointF B)
		{
			double d1 = A.X - B.X;
			double d2 = A.Y - B.Y;
			return Math.Sqrt(d1 * d1 + d2 * d2);
		}

		/// <summary>
		/// Compute the dot product AB ⋅ BC
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="C"></param>
		/// <returns></returns>

		public static double DotProduct(PointF A, PointF B, PointF C)
		{
			PointF AB = new PointF(), BC = new PointF();
			AB.X = B.X - A.X;
			AB.Y = B.Y - A.Y;
			BC.X = C.X - B.X;
			BC.Y = C.Y - B.Y;
			double dot = AB.X * BC.X + AB.Y * BC.Y;
			return dot;
		}

		/// <summary>
		/// Compute the cross product AB x AC
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <param name="C"></param>
		/// <returns></returns>

		public static double CrossProduct(PointF A, PointF B, PointF C)
		{
			PointF AB = new PointF(), AC = new PointF();

			AB.X = B.X - A.X;
			AB.Y = B.Y - A.Y;
			AC.X = C.X - A.X;
			AC.Y = C.Y - A.Y;
			double cross = AB.X * AC.Y - AB.Y * AC.X;
			return cross;
		}

	}

/// <summary>
/// Page margins class
/// </summary>

	public class PageMargins
	{
		public int Left;
		public int Right;
		public int Top;
		public int Bottom;

		public PageMargins(int left, int right, int top, int bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}

		public string Serialize()
		{
			string serializedForm = Left.ToString() + "," + Right.ToString() + "," + 
				Top.ToString() + "," + Bottom.ToString(); 

			return serializedForm;
		}

		public static PageMargins Deserialize (
			string serializedForm)
		{
			int left = 0, right = 0, top = 0, bottom = 0;

			if (String.IsNullOrEmpty(serializedForm)) return null;
			string[] sa = serializedForm.Split(',');
			if (sa.Length != 4) return null;
			int.TryParse(sa[0], out left);
			int.TryParse(sa[1], out right);
			int.TryParse(sa[2], out top);
			int.TryParse(sa[3], out bottom);
			PageMargins pm = new PageMargins(left, right, top, bottom);
			return pm;
		}

		public PageMargins Clone()
		{
			return (PageMargins)this.MemberwiseClone();
		}
	}

}
