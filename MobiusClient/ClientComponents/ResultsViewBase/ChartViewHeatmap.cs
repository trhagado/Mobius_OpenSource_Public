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
	/// ChartView for Heatmap
	/// </summary>

	public partial class ChartViewHeatmap : ChartViewBubble
	{
		/// <summary>
		/// Basic constructor
		/// </summary>

		public ChartViewHeatmap()
		{
			ViewType = ViewTypeMx.Heatmap;
			Title = "Heatmap";
			ForceCustomMarkerRendering = true; // must always render with custom marker size
		}

		/// <summary>
		/// Get the view type image index for the view
		/// </summary>
		/// <returns></returns>

		public override ViewTypeImageEnum ViewTypeImageIndex
		{
			get
			{
				return ViewTypeImageEnum.HeatMap;
			}
		}

/// <summary>
/// Calculate width & height for rectangular heatmap markers in pixels so there are not gaps or overlaps between heatmap cells
/// </summary>
/// <param name="markerWidth"></param>
/// <param name="markerHeight"></param>

		public void CalculateHeatmapMarkerSize(
			out double markerWidth,
			out double markerHeight)
		{
			markerWidth = markerHeight = 10;

			Rectangle rt = // get diagram size in pixels
				DiagramToPointHelper.CalculateDiagramBounds(XYDiagram);

			//double xmin = XYDiagram.AxisX.Range.ScrollingRange.MinValueInternal;
			//double xmax = XYDiagram.AxisX.Range.ScrollingRange.MaxValueInternal;

			//double ymin = XYDiagram.AxisY.Range.ScrollingRange.MinValueInternal;
			//double ymax = XYDiagram.AxisY.Range.ScrollingRange.MaxValueInternal;

			int xpix = rt.Width;
			double xmin = XYDiagram.AxisX.VisualRange.MinValueInternal;
			double xmax = XYDiagram.AxisX.VisualRange.MaxValueInternal;
			double xrange = Math.Abs(xmax - xmin);
			markerWidth = (int)(xpix / xrange + 1.0);

			int ypix = rt.Height;
			double ymin = XYDiagram.AxisY.VisualRange.MinValueInternal;
			double ymax = XYDiagram.AxisY.VisualRange.MaxValueInternal;
			double yrange = Math.Abs(ymax - ymin);
			markerHeight = (int)(ypix / yrange + 1.0);

			return;
		}
	}
}
