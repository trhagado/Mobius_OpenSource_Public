using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Mobius.SpotfireDocument
{
#if false
	public class Axis
	{
		public AxisModeMsx AxisMode { get; }
		//public Font TitleFont { get; set; }
		public AxisBindingMsx Binding { get; set; }
		public AxisEvaluationModeMsx EvaluationMode { get; set; }
		public CategoryModeMsx CategoryMode { get; set; }
		public string Name { get; }
		public string Expression { get; set; }
	}

	public abstract class ScaleBaseMsx
	{
		public LabelOrientationMsx LabelOrientation { get; set; }
		public bool ShowLabels { get; set; }
		public int MaximumNumberOfTicks { get; set; }
		//public Font Font { get; set; }
		public bool ShowGridlines { get; set; }
		//public Formatting Formatting { get; }
		public bool Visible { get; set; }
		public virtual float Span { get; set; }
	}

	public class ShapeAxisMsx : AxisMsx
	{
		public LegendMarkerShapeItemMsx LegendItem { get; }
		public ShapeMapMsx ShapeMap { get; }
		public MarkerShapeMsx DefaultShape { get; set; }

	}

	public class ShapeMapMsx
	{
		public Dictionary<CategoryKeyMsx, MarkerShapeMsx> CategoryShapeDict = new Dictionary<CategoryKeyMsx, MarkerShapeMsx>();
	}

	public class MarkerShapeMsx
	{
		public MarkerTypeMsx MarkerType;
	}

	public class LegendMarkerShapeItemMsx : LegendAxisItemMsx
	{
		// empty
	}


	public class LegendAxisItemMsx : LegendItemMsx
	{
		public bool ShowAxisSelector { get; set; }
	}

	public class AxisBindingMsx
	{
		//public DataMarkingSelection MarkingReference { get; set; }
		public DataColumnMsx ColumnReference { get; set; }
		public string ExpressionTemplate { get; set; }
	}

	public class OrderByAxisMsx : Axis
	{
		public bool Reversed { get; set; }
	}

	public class GroupByAxisMsx : CategoricalAxisBaseMsx
	{
		public LegendGroupByItemMsx LegendItem { get; }
	}

	public class SizeAxisMsx : Axis
	{
		public LegendSizeItemMsx LegendItem { get; }
		public AxisRangeMsx Range { get; set; }
		public ContinuousScaleTypeMsx ScaleType { get; set; }
	}

	public class LegendSizeItemMsx : LegendAxisItemMsx
	{
		// empty
	}

	public sealed class ColorAxisMsx : Axis
	{
		public LegendColorItemMsx LegendItem { get; }
		public CategoricalColorAxisMsx Categorical { get; }
		public ContinuousColorAxisMsx Continuous { get; }
		public Color DefaultColor { get; set; }
		public ColoringMsx Coloring { get; }
	}

	public class ContinuousColorAxisMsx : ContinuousAxisMsx { }

	public class ContinuousAxisMsx : ContinuousAxisBaseMsx { }

	public class ContinuousAxisBaseMsx : Axis { }

	public sealed class CategoricalColorAxisMsx : CategoricalAxisMsx
	{
		public ColorMapMsx ColorMap { get; }
	}

	public class CategoryKeyMsx
	{
		public object[] Parts;
	}

	public class CategoricalAxisMsx : CategoricalAxisBaseMsx { }

	public class CategoricalAxisBaseMsx : Axis { }

	public class LegendColorItemMsx : LegendAxisItemMsx
	{
		// empty
	}

	public class ScaleAxisMsx : ScaleAxisBaseMsx
	{
		public ScaleMsx Scale { get; }
		//public ErrorBars ErrorBars { get; }
		//public ScaleLabels ScaleLabels { get; }
	}

	public sealed class ScaleMsx : ScaleBaseMsx
	{
		public override float Span { get; set; }
		public float NearSpan { get; set; }
		public float FarSpan { get; set; }
		//public ScaleDock Dock { get; set; }
		//public IndexedScaleDock IndexedDock { get; }
		//public ScaleLabelLayout LabelLayout { get; set; }
	}


	public class ScaleAxisBaseMsx : Axis
	{
		//public IndexedAxisTransformType IndexedTransformType { get; }
		public bool IndividualScaling { get; set; }
		//public IndexedBool IndexedReversed { get; }
		public bool Reversed { get; set; }
		//public IndexedBool IndexedIncludeZeroInAutoZoom { get; }
		public AxisTransformTypeMsx TransformType { get; set; }
		public AxisRangeMsx ZoomRange { get; set; }
		public AxisRangeMsx Range { get; set; }
		public bool IncludeZeroInAutoZoom { get; set; }
		public bool ManualZoom { get; set; }
		public bool ShowAxisSelector { get; set; }
		public CategoricalScaleAxisMsx Categorical { get; }
		public ContinuousScaleAxisMsx Continuous { get; }
		public IndividualScalingModeMsx IndividualScalingMode { get; set; }
		//public IndexedAxisRange IndexedRange { get; }
	}

	public class ContinuousScaleAxisMsx : ContinuousAxisMsx { }

	public class CategoricalScaleAxisMsx : CategoricalAxisMsx { }

	public struct AxisRangeMsx
	{
		public object Low { get; }
		public object High { get; }
	}

	public class LegendGroupByItemMsx : LegendAxisItemMsx { }

	public class LabelColumnMsx : ExpressionColumnMsx { }

	public class ExpressionColumnMsx
	{
		public string Expression { get; set; }
	}

	public enum ContinuousScaleTypeMsx
	{
		RatioScale = 0,
		IntervalScale = 1
	}

	public enum AxisTransformTypeMsx
	{
		None = 0,
		Log10 = 1
	}

	public enum IndividualScalingModeMsx
	{
		Color = 0,
		Trellis = 1
	}

	public enum LabelOrientationMsx
	{
		Horizontal = 0,
		Vertical = 1
	}

	public enum CategoryModeMsx
	{
		ShowAll = 0,
		ShowFiltered = 1,
		ShowFilteredRange = 2
	}


	public enum AxisModeMsx
	{
		Categorical = 0,
		Continuous = 1

	}

	public enum AxisEvaluationModeMsx
	{
		AllData = 0,
		FilteredData = 1
	}

	public enum LabelVisibilityMsx
	{
		None = 0,
		Marked = 1,
		All = 2
	}

	public enum MarkerClassMsx
	{
		Simple = 0,
		Chart = 1,
		Tile = 2
	}


	public enum MarkerTypeMsx
	{
		Custom = 0,
		Square = 1,
		Circle = 2,
		Diamond = 3,
		TriangleUp = 4,
		TriangleDown = 5,
		TriangleLeft = 6,
		TriangleRight = 7,
		StarFour = 8,
		StarFive = 9,
		Pentagon = 10,
		HorizontalRectangle = 11,
		VerticalRectangle = 12,
		Plus = 13,
		Cross = 14,
		SquareOutline = 15,
		CircleOutline = 16,
		DiamondOutline = 17,
		TriangleUpOutline = 18,
		TriangleDownOutline = 19,
		TriangleLeftOutline = 20,
		TriangleRightOutline = 21,
		StarFourOutline = 22,
		StarFiveOutline = 23,
		PentagonOutline = 24,
		HorizontalRectangleOutline = 25,
		VerticalRectangleOutline = 26,
		PlusOutline = 27,
		CrossOutline = 28,
		HorizontalLine = 29,
		VerticalLine = 30,
		PlusLine = 31,
		CrossLine = 32,
		StarSixLine = 33,
		RightPointingArrow = 34,
		LeftPointingArrow = 35,
		UpPointingArrow = 36,
		DownPointingArrow = 37,
		RightPointingArrowOutline = 38,
		LeftPointingArrowOutline = 39,
		UpPointingArrowOutline = 40,
		DownPointingArrowOutline = 41,
		RightPointingArrowLine = 42,
		LeftPointingArrowLine = 43,
		UpPointingArrowLine = 44,
		DownPointingArrowLine = 45
	}
#endif
}
