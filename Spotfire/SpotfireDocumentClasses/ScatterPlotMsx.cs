using Mobius.SpotfireDocument;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// Scatterplot Visualization
	/// </summary>

	public class ScatterPlotMsx : VisualMsx
	{
		public ShapeAxisMsx ShapeAxis = null;
		public LabelVisibilityMsx LabelVisibility { get; set; }
		public bool ShowStraightLineFit { get; set; }
		//public PieMarker PieMarker { get; }
		public MarkerClassMsx MarkerClass { get; set; }
		//public ScatterPlotDetails Details { get; }
		//public ValueRendererSettings LabelRenderer { get; }
		//public LineConnection LineConnection { get; }
		public OrderByAxisMsx DrawingOrderAxis { get; set; }
		public GroupByAxisMsx MarkerByAxis { get; set; }
		public SizeAxisMsx SizeAxis { get; set; }
		public ColorAxisMsx ColorAxis { get; set; }
		//public Font LabelFont { get; set; }
		public double YJitter { get; set; }
		public double XJitter { get; set; }
		public ScaleAxisMsx XAxis { get; set; }
		public float MarkerSize { get; set; }
		public int MaxNumberOfLabels { get; set; }
		public float LabelImageSize { get; set; }
		//public MarkerLabelLayout MarkerLabelLayout { get; set; }
		public bool ShowEmptyLabels { get; set; }
		//public FittingModelCollection FittingModels { get; set; }
		public ScaleAxisMsx YAxis { get; set; }
		public string LabelColumn { get; set; } // Column expression

		public TrellisMsx Trellis = new TrellisMsx();

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			base.UpdatePreSerializationSecondaryReferences(); // Update refs for base VisualMsx members

			UpdatePreSerializationSecondaryReferences(XAxis);

			UpdatePreSerializationSecondaryReferences(YAxis);

			UpdatePreSerializationSecondaryReferences(ColorAxis);

			UpdatePreSerializationSecondaryReferences(SizeAxis);

			UpdatePreSerializationSecondaryReferences(ShapeAxis);

			UpdatePreSerializationSecondaryReferences(DrawingOrderAxis);

			UpdatePreSerializationSecondaryReferences(MarkerByAxis);

			UpdatePreSerializationSecondaryReferences(Trellis);

			return;
		}


		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			base.UpdatePostDeserializationSecondaryReferences(); // Update refs for base VisualMsx members

			UpdatePostDeserializationSecondaryReferences(XAxis);

			UpdatePostDeserializationSecondaryReferences(YAxis);

			UpdatePostDeserializationSecondaryReferences(ColorAxis);

			UpdatePostDeserializationSecondaryReferences(SizeAxis);

			UpdatePostDeserializationSecondaryReferences(ShapeAxis);

			UpdatePostDeserializationSecondaryReferences(DrawingOrderAxis);

			UpdatePostDeserializationSecondaryReferences(MarkerByAxis);

			UpdatePostDeserializationSecondaryReferences(Trellis);

			return;
		}
	}
}
