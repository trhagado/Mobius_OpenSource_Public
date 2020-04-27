using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// AxisMsx (changed in server)
	/// </summary>
	public class AxisMsx : NodeMsx
	{
		public AxisModeMsx AxisMode { get; set; }
		//public Font TitleFont { get; set; }
		//public AxisBinding Binding { get; set; }
		//public AxisEvaluationMode EvaluationMode { get; set; }
		public CategoryModeMsx CategoryMode { get; set; }
		public string Name = "";
		public string Expression;
		public bool MultiExpressionSelectionAllowed = false; // Mobius-only column to indicate if multiple cols can be selected for this axis

		public List<DataTableMsx> GetAllowedDataTables()
		{
			List<DataTableMsx> dtList = new List<DataTableMsx>();
			foreach (DataTableMsx dtObj in Doc_Tables)
			{
				dtList.Add(dtObj);
			}

			return dtList;
		}

	} // AxisMsx

	/// <summary>
	/// ShapeAxisMsx
	/// </summary>

	public class ShapeAxisMsx : AxisMsx
	{
		public LegendMarkerShapeItemMsx LegendItem = new LegendMarkerShapeItemMsx();
		public ShapeMapMsx ShapeMap = new ShapeMapMsx();
		public MarkerShapeMsx DefaultShape = new MarkerShapeMsx();
	}

	public class ShapeMapMsx : NodeMsx
	{
		[XmlIgnore] // Can't XmlSerialize a Dictionary, pass as list defined below
		public Dictionary<CategoryKeyMsx, MarkerShapeMsx> CategoryToMarkerShapeDict = new Dictionary<CategoryKeyMsx, MarkerShapeMsx>();

		public List<KeyValuePair<CategoryKeyMsx, MarkerShapeMsx>> CategoryToMarkerShapeList
		{
			get { return CategoryToMarkerShapeDict.ToList(); }
			set { CategoryToMarkerShapeDict = value.ToDictionary(x => x.Key, x => x.Value); }
		}

	}

	public class MarkerShapeMsx : NodeMsx
	{
		public MarkerTypeMsx MarkerType;
	}

	public class LegendMarkerShapeItemMsx : LegendAxisItemMsx
	{
		// empty
	}
	
	public class AxisBindingMsx
	{
		//public DataMarkingSelection MarkingReference { get; set; }
		public ColumnMapParms ColumnReference { get; set; }
		public string ExpressionTemplate { get; set; }
	}

	public class OrderByAxisMsx : AxisMsx
	{
		public bool Reversed { get; set; }
	}

	public class GroupByAxisMsx : CategoricalAxisBaseMsx
	{
		public LegendGroupByItemMsx LegendItem { get; set; }
	}

	public class SizeAxisMsx : AxisMsx
	{
		public LegendSizeItemMsx LegendItem { get; set; }
		public AxisRangeMsx Range { get; set; }
		public ContinuousScaleTypeMsx ScaleType { get; set; }

		public double FixedSize // relative percentage size in the range of 1 - 100 (temporary, remove later)
		{ get { return _fixedSize; } set { _fixedSize = value; } }
		double _fixedSize = -1;
	}

	public class LegendSizeItemMsx : LegendAxisItemMsx
	{	} // no new members

	public class ContinuousColorAxisMsx : ContinuousAxisMsx { }

	/// <summary>
	/// ContinuousAxisMsx
	/// </summary>

	public class ContinuousAxisMsx : ContinuousAxisBaseMsx { }

	/// <summary>
	/// ContinuousAxisBaseMsx - Base class for the continuous part of an axis.
	/// </summary>
	
	public class ContinuousAxisBaseMsx : AxisMsx { }

	/// <summary>
	/// CategoricalColorAxisMsx - Class containing properties and methods used for 
	///  categorical coloring within a visualization.
	/// </summary>

	public sealed class CategoricalColorAxisMsx : CategoricalAxisMsx { }
	/// <summary>
	/// 
	/// </summary>

	public class CategoricalAxisMsx : CategoricalAxisBaseMsx { }

	public class CategoricalAxisBaseMsx : AxisMsx { }

	public sealed class ColorAxisMsx : AxisMsx
	{
		public LegendColorItemMsx LegendItem { get; set; }
		public XmlColor DefaultColor { get; set; }
		public ColoringMsx Coloring { get; set; }
	}


	public class LegendColorItemMsx : LegendAxisItemMsx
	{
		// empty
	}

	/// <summary>
	/// ScaleAxisMsx -  Represents an X or Y axis in a visualization. 
	/// It is used to map values to an axis, typically by applying a range. 
	/// When mapping, the scale axis can transform the values.
	/// </summary>

	public class ScaleAxisMsx : ScaleAxisBaseMsx
	{
		public ScaleMsx Scale { get; set; }
		//public ErrorBars ErrorBars { get; set; }
		//public ScaleLabels ScaleLabels { get; set; }
	}

	/// <summary>
	/// ScaleAxisBaseMsx
	/// </summary>

	public class ScaleAxisBaseMsx : AxisMsx
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

	/// <summary>
	/// ContinuousScaleAxisMsx
	/// </summary>

	public class ContinuousScaleAxisMsx : ContinuousAxisMsx { }

	/// <summary>
	/// CategoricalScaleAxisMsx
	/// </summary>

	public class CategoricalScaleAxisMsx : CategoricalAxisMsx { }

	/// <summary>
	/// ScaleMsx
	/// </summary>

	public sealed class ScaleMsx : ScaleBaseMsx
	{
		public override float Span { get; set; }
		public float NearSpan { get; set; }
		public float FarSpan { get; set; }
		public ScaleLabelLayoutMsx LabelLayout { get; set; }

		//public ScaleDock Dock { get; set; }
		//public IndexedScaleDock IndexedDock { get; }
	}

	public abstract class ScaleBaseMsx : NodeMsx
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

	/// <summary>
	/// Controls the ranges of an Axis (Range, IndexedRange, ZoomRange)
	/// Consists of a Low, High pair of values compatible with the associated
	/// column type (e.g. string, integer, double, DateTime etc).
	/// </summary>

	public struct AxisRangeMsx
	{
		public object Low { get; set; }
		public object High { get; set; }

		public AxisRangeMsx(object low, object high)
		{
			Low = low;
			High = high;
		}
	}

	public class LabelColumnMsx : ExpressionColumnMsx { }

	public class ExpressionColumnMsx : NodeMsx
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

	public enum ScaleLabelLayoutMsx
	{
		MaximumNumberOfTicks = 0,
		Automatic = 1
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

	public enum AxisModeMsx
	{
		Categorical = 0,
		Continuous = 1
	}

	public enum CategoryModeMsx
	{
		ShowAll = 0,
		ShowFiltered = 1,
		ShowFilteredRange = 2
	}

	/// <summary>
	/// Class: CategoryKeyMsx
	/// 
	///	This class is primarily used when mapping categories to aestetic attributes like color and shape.
	///	Since categorical axes are hierarchical, a CategoryKey is made from the values of each level in the hierarchy. 
	///	For instance, if an axis has an expression that nests an integer column within a string column, a category key would be defined by a string value and an integer.
	/// </summary>

	public class CategoryKeyMsx
	{
		public object[] Parts;
	}

	public enum LabelVisibilityModeEnum
	{
		Unknown = 0,
		AllRows = 1,
		MarkedRows = 2,
		RolloverRow = 3
	}

	public enum LabelPositionEnum
	{
		Center = 0,
		Outside = 1
	}

	public enum ShapeRenderingMode
	{
		Default = 0,
		HighSpeed = 1,
		HighQuality = 2,
		None = 3,
		AntiAlias = 4,
	}

	public enum SurfaceFillMode
	{
		None = 0,
		Zone = 1,
		Uniform = 2,
		CustomColors = 3,
	}

	public enum SurfaceFrameMode
	{
		None = 0,
		Mesh = 1,
		Contour = 2,
		MeshContour = 3,
		Dots = 4,
	}

	public enum AxisLabelResolveOverlappingMode
	{
		None = 0,
		HideOverlapped = 1 // If two or more labels overlap, some of them are automatically hidden, to avoid overlapping.  
	}

	public enum MarkerClassMsx
	{
		Simple = 0,
		Chart = 1,
		Tile = 2
	}

	public enum LabelVisibilityMsx
	{
		None = 0,
		Marked = 1,
		All = 2
	}

}

