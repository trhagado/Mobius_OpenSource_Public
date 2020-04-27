using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// ChartView for Bubble Chart (2nd level members)
	/// </summary>

	public partial class ChartViewBubble : ChartViewMgr
	{

		Boolean DrawingSelectionRectangle = false;
		Point SelectionRectangleOrigin = new Point();
		Point SelectionRectangleEndPoint = new Point();

		public void ShowSelectionRectangle(Object sender, MouseEventArgs e)
		{
			// Make a note that we "have the mouse".
			DrawingSelectionRectangle = true;
			// Store the "starting point" for this rubber-band rectangle.
			SelectionRectangleOrigin.X = e.X;
			SelectionRectangleOrigin.Y = e.Y;
			// Special value lets us know that no previous
			// rectangle needs to be erased.
			SelectionRectangleEndPoint.X = -1;
			SelectionRectangleEndPoint.Y = -1;
		}

		/// <summary>
		/// Convert and normalize the points and draw the reversible frame.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>

		private void DrawReversibleRectangle(Point p1, Point p2)
		{
			Rectangle rc = new Rectangle();

			// Convert the points to screen coordinates.
			p1 = ChartControl.PointToScreen(p1);
			p2 = ChartControl.PointToScreen(p2);
			// Normalize the rectangle.
			if (p1.X < p2.X)
			{
				rc.X = p1.X;
				rc.Width = p2.X - p1.X;
			}
			else
			{
				rc.X = p2.X;
				rc.Width = p1.X - p2.X;
			}
			if (p1.Y < p2.Y)
			{
				rc.Y = p1.Y;
				rc.Height = p2.Y - p1.Y;
			}
			else
			{
				rc.Y = p2.Y;
				rc.Height = p1.Y - p2.Y;
			}
			// Draw the reversible frame.
			ControlPaint.DrawReversibleFrame(rc,
				Color.Yellow, FrameStyle.Dashed);
		}

/// <summary>
/// Done drawing 
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		public void HideSelectionRectangle()
		{
			// Set internal flag to know we no longer "have the mouse".
			DrawingSelectionRectangle = false;
			// If we have drawn previously, draw again in that spot
			// to remove the lines.
			if (SelectionRectangleEndPoint.X != -1)
			{
				//Point ptCurrent = new Point(e.X, e.Y);
				DrawReversibleRectangle(SelectionRectangleOrigin, SelectionRectangleEndPoint);
			}
			// Set flags to know that there is no "previous" line to reverse.
			SelectionRectangleEndPoint.X = -1;
			SelectionRectangleEndPoint.Y = -1;
			SelectionRectangleOrigin.X = -1;
			SelectionRectangleOrigin.Y = -1;
		}

/// <summary>
/// Update the selection rectangle
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		public void UpdateSelectionRectangle(Object sender, MouseEventArgs e)
		{
			Point ptCurrent = new Point(e.X, e.Y);
			// If we "have the mouse", then we draw our lines.
			if (DrawingSelectionRectangle)
			{
				// If we have drawn previously, draw again in
				// that spot to remove the lines.
				if (SelectionRectangleEndPoint.X != -1)
				{
					DrawReversibleRectangle(SelectionRectangleOrigin, SelectionRectangleEndPoint);
				}
				// Update last point.
				SelectionRectangleEndPoint = ptCurrent;
				// Draw new lines.
				DrawReversibleRectangle(SelectionRectangleOrigin, ptCurrent);
			}
		}
	}
}