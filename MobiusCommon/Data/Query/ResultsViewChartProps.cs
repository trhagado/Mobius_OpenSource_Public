using Mobius.ComOps;

using DevExpress.XtraCharts;

using System;
using System.IO;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{

	/// <summary>
	/// Chart view properties that are part of the ResultsView
	/// Members are included in ResultsView class
	/// </summary>

	public partial class ResultsViewProps
	{

#if false // Obsolete - now stored within Spotfire analysis document visualization properties

		// Trellis data

		public bool TrellisByRowCol = true;

		public QueryColumn TrellisRowQc; // QueryColumn to use to determine trellis rows
		public QueryColumn TrellisColQc; // QueryColumn to use to determine trellis columns
		public QueryColumn TrellisPageQc; // QueryColumn to use to determine trellis pages

		public bool TrellisByFlow { get { return !TrellisByRowCol; } set { TrellisByRowCol = !value; } }
		public QueryColumn TrellisFlowQc; // QueryColumn for a flowed trellis
		public bool TrellisManual = false; // if true manually specify the trellis rows and cols for a single page
		public int TrellisMaxRows = 2; // manual max number of rows per page
		public int TrellisMaxCols = 2; // manual max number of cols per page

		public int TrellisPageCount;
		public int TrellisPageIndex;

		public bool IsTrellis // return true if trellising is turned on for chart
		{
			get
			{
				if (TrellisByRowCol && (TrellisColQc != null || TrellisRowQc != null || TrellisPageQc != null))
					return true;
				else if (TrellisByFlow && TrellisFlowQc != null) return true;
				else return false;
			}
		}


		///////////////////////////////
		////// Chart properties ///////
		///////////////////////////////

		// Chart properties

		public static bool ChartPropertiesEnabled = true; // Set to true if chart properties are enabled

		public AxisMx XAxisMx = new AxisMx(); // X axis
		public AxisMx YAxisMx = new AxisMx(); // Y axis
		public AxisMx ZAxisMx = new AxisMx(); // Z axis

		public List<AxisMx> AxesMx = new List<AxisMx>(); // list of axes for radar chart etc.

		public ColorDimension MarkerColor = new ColorDimension(); // Coloring to use for marker

		public SizeDimension MarkerSize = new SizeDimension(50); // size to use for marker
		public double MaxMarkerSize = .1; // max marker size a portion of full axis range

		public ShapeDimension MarkerShape = new ShapeDimension(); // QueryColumn used to control marker shape

		public LabelDimensionDef MarkerLabel = new LabelDimensionDef(); // marker labels
		public int MaxLabels = 100;

		public TooltipDimensionDef MarkerTooltip = new TooltipDimensionDef();

		public bool UseExistingCondFormatting = true; // use existing formatting or calculate it ourselves
		public bool PivotedData = false; // if true viewing pivoted data otherwise viewing unpivoted data

		// Legend

		public bool ShowLegend = true;
		public LegendAlignmentHorizontal LegendAlignmentHorizontal = LegendAlignmentHorizontal.RightOutside;
		public LegendAlignmentVertical LegendAlignmentVertical = LegendAlignmentVertical.Top;
		public int LegendMaxHorizontalPercentage = 50;
		public int LegendMaxVerticalPercentage = 100;
		public LegendDirection LegendItemOrder = LegendDirection.TopToBottom;

		// Misc data

		public string TemplateFileName = ""; // template file to use (Spotfire visualizations)
		public string BackgroundImageFile = ""; // name of the image file, may reference server (e.g. @"<TargetMaps>\Dendograms.png")
		public Bitmap BackgroundImage = null; // the image after it's been read in
		public Rectangle VisibleRange; // current visible range (not yet implemented)
		public int JitterX = 0;
		public int JitterY = 0;
		public ShapeRenderingMode ShapeRenderingMode = ShapeRenderingMode.HighSpeed; // default to high speed for "crisp" edges on 2D markers
		public bool JitterTheSameForXandY = false; // stretch to fit panel
		public bool MainViewPanelMaximized = false; // overrides other show parameters
		public bool ShowFilters = true;
		public bool ShowSelectedDataRows = true;

		public bool ShowAxesTitles = true;
		public bool ShowAxesScaleLabels = true;
		public bool RotateAxes = false;


		// Surface-specific parameters

		public SurfaceFillMode SurfaceFillMode = SurfaceFillMode.Zone; // how surface should be filled with color
		public SurfaceFrameMode SurfaceFrameMode = SurfaceFrameMode.None; // display of frame
		public bool SmoothPalette = false; // smooth palette coloring between zones
		public bool SmoothShading = true; // smooth or flat (shows polygons) shading (ShadingMode)
		public bool SemiTransparent = false;
		public bool DrawFlat = false;

/// <summary>
/// Serialize chart properties
/// </summary>
/// <returns></returns>

		public string SerializeChartProperties()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			XmlTextWriter tw = mstw.Writer;
			tw.Formatting = Formatting.Indented;
			SerializeChartProperties(tw);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		/// <summary>
		/// Serialize Chart Properties
		/// </summary>
		/// <param name="tw"></param>

		public void SerializeChartProperties(XmlTextWriter tw)
		{
			if (!ChartPropertiesEnabled) return;

			tw.WriteStartElement("ChartProperties");

			tw.WriteAttributeString("ShapeRenderingMode", ShapeRenderingMode.ToString());
			tw.WriteAttributeString("Jitter", JitterX.ToString());
			tw.WriteAttributeString("ChartStretch", JitterTheSameForXandY.ToString());

			tw.WriteAttributeString("ShowLegend", ShowLegend.ToString());
			if (LegendAlignmentHorizontal != ModelView.LegendAlignmentHorizontal) 
				tw.WriteAttributeString("LegendAlignmentHorizontal", LegendAlignmentHorizontal.ToString());
			if (LegendAlignmentVertical != ModelView.LegendAlignmentVertical)
				tw.WriteAttributeString("LegendAlignmentVertical", LegendAlignmentVertical.ToString());
			if (LegendMaxHorizontalPercentage != ModelView.LegendMaxHorizontalPercentage)
				tw.WriteAttributeString("LegendMaxHorizontalPercentage", LegendMaxHorizontalPercentage.ToString());
			if (LegendMaxVerticalPercentage != ModelView.LegendMaxVerticalPercentage)
				tw.WriteAttributeString("LegendMaxVerticalPercentage", LegendMaxVerticalPercentage.ToString());
			if (LegendItemOrder != ModelView.LegendItemOrder)
				tw.WriteAttributeString("LegendItemOrder", LegendItemOrder.ToString());

			tw.WriteAttributeString("MainChartAreaMaximized", MainViewPanelMaximized.ToString());
			tw.WriteAttributeString("ShowFilters", ShowFilters.ToString());
			tw.WriteAttributeString("ShowSelectedDataRows", ShowSelectedDataRows.ToString());
			tw.WriteAttributeString("ShowAxesTitles", ShowAxesTitles.ToString());
			tw.WriteAttributeString("ShowAxesScaleLabels", ShowAxesScaleLabels.ToString());
			tw.WriteAttributeString("RotateAxes", RotateAxes.ToString());

			tw.WriteAttributeString("UseExistingCondFormatting", UseExistingCondFormatting.ToString());
			tw.WriteAttributeString("PivotedData", PivotedData.ToString());

			SerializeAxis(XAxisMx, "XAxis", tw);
			SerializeAxis(YAxisMx, "YAxis", tw);
			SerializeAxis(ZAxisMx, "ZAxis", tw);

			for (int ai = 0; ai < AxesMx.Count; ai++)
			{
				SerializeAxis(AxesMx[ai], "Axis" + (ai + 1).ToString(), tw);
			}

			MarkerColor.Serialize("MarkerColor", tw); // marker color

			MarkerSize.Serialize("MarkerSize", tw); // marker size

			MarkerShape.Serialize("MarkerShape", tw); // marker shape

			MarkerLabel.Serialize("MarkerLabel", tw); // marker labels

			// Tooltip

			MarkerTooltip.Serialize("ToolTipFields", tw);

			// Surface attributes

			tw.WriteStartElement("Surface");
			tw.WriteAttributeString("FillMode", SurfaceFillMode.ToString());
			tw.WriteAttributeString("FrameMode", SurfaceFrameMode.ToString());
			tw.WriteAttributeString("SmoothPalette", SmoothPalette.ToString());
			tw.WriteAttributeString("SmoothShading", SmoothShading.ToString());
			tw.WriteAttributeString("SemiTransparent", SemiTransparent.ToString());
			tw.WriteAttributeString("DrawFlat", DrawFlat.ToString());
			tw.WriteEndElement();

			// Trellis attributes

			tw.WriteStartElement("Trellis");
			tw.WriteAttributeString("ByRowCol", TrellisByRowCol.ToString());
			tw.WriteAttributeString("Manual", TrellisManual.ToString());
			tw.WriteAttributeString("MaxRows", TrellisMaxRows.ToString());
			tw.WriteAttributeString("MaxCols", TrellisMaxCols.ToString());
			SerializeQueryColumnRef(TrellisColQc, "ColumnQc", tw);
			SerializeQueryColumnRef(TrellisRowQc, "RowQc", tw);
			SerializeQueryColumnRef(TrellisPageQc, "PageQc", tw);
			SerializeQueryColumnRef(TrellisFlowQc, "FlowQc", tw);
			tw.WriteEndElement();

			tw.WriteEndElement(); // ChartProperties
			return;
		}

/// <summary>
/// Serialize Axis
/// </summary>
/// <param name="axis"></param>
/// <param name="name"></param>
/// <param name="tw"></param>

		void SerializeAxis(AxisMx axis, string name, XmlTextWriter tw)
		{
			if (axis == null || axis.QueryColumn == null) return;

			tw.WriteStartElement(name);

			tw.WriteAttributeString("ShortName", axis.ShortName);
			tw.WriteAttributeString("RangeMin", axis.RangeMin);
			tw.WriteAttributeString("RangeMax", axis.RangeMax);
			tw.WriteAttributeString("IncludeOrigin", axis.IncludeOrigin.ToString());
			tw.WriteAttributeString("SideMarginsEnabled", axis.SideMarginsEnabled.ToString());
			tw.WriteAttributeString("ShowLabels", axis.ShowLabels.ToString());
			tw.WriteAttributeString("LabelAngle", axis.LabelAngle.ToString());
			tw.WriteAttributeString("LabelsStaggered", axis.LabelsStaggered.ToString());
			tw.WriteAttributeString("LabelResolveOverlappingMode", axis.LabelResolveOverlappingMode.ToString());
			tw.WriteAttributeString("ShowZoomSlider", axis.ShowZoomSlider.ToString());
			tw.WriteAttributeString("ShowGridLines", axis.ShowGridLines.ToString());
			tw.WriteAttributeString("ShowGridStrips", axis.ShowGridStrips.ToString());
			tw.WriteAttributeString("LogScale", axis.LogScale.ToString());
			tw.WriteAttributeString("ReverseScale", axis.ReverseScale.ToString());
			SerializeQueryColumnRef(axis.QueryColumn, name + "Column", tw);
			tw.WriteEndElement();
		}

/// <summary>
/// Deserialize chart props into an existing view
/// </summary>
/// <param name="serializedPropString"></param>
/// <param name="q"></param>
/// <param name="view"></param>

		public static void DeserializeChartProperties(
			string serializedPropString,
			Query q,
			ResultsViewProps view)
		{
			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedPropString);
			XmlTextReader tr = mstr.Reader;
			tr.MoveToContent();
			DeserializeChartProperties(q, tr, view);
			tr.Close();
			return;
		}

		/// <summary>
		/// Deserialize Chart Properties
		/// </summary>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <param name="view"></param>

		public static bool DeserializeChartProperties(
			Query q,
			XmlTextReader tr,
			ResultsViewProps view)
		{
			AxisMx ax;
			Enum iEnum = null;
			string txt;

			if (!Lex.Eq(tr.Name, "ChartProperties")) return false;

			if (XmlUtil.GetEnumAttribute(tr, "ShapeRenderingMode", typeof(ShapeRenderingMode), ref iEnum))
				view.ShapeRenderingMode = (ShapeRenderingMode)iEnum;

			//XmlUtil.GetStringAttribute(tr, "BackgroundImageFile", ref view.BackgroundImageFile);
			XmlUtil.GetIntAttribute(tr, "Jitter", ref view.JitterX);
			XmlUtil.GetBoolAttribute(tr, "ChartStretch", ref view.JitterTheSameForXandY);

			XmlUtil.GetBoolAttribute(tr, "ShowLegend", ref view.ShowLegend);

			if (tr.GetAttribute("LegendAlignmentHorizontal") != null)
				view.LegendAlignmentHorizontal = 
					(LegendAlignmentHorizontal)EnumUtil.Parse(typeof(LegendAlignmentHorizontal), tr.GetAttribute("LegendAlignmentHorizontal"));

			if (tr.GetAttribute("LegendAlignmentVertical") != null)
				view.LegendAlignmentVertical =
					(LegendAlignmentVertical)EnumUtil.Parse(typeof(LegendAlignmentVertical), tr.GetAttribute("LegendAlignmentVertical"));

			XmlUtil.GetIntAttribute(tr, "LegendMaxHorizontalPercentage", ref view.LegendMaxHorizontalPercentage);
			XmlUtil.GetIntAttribute(tr, "LegendMaxVerticalPercentage", ref view.LegendMaxVerticalPercentage);

			if (tr.GetAttribute("LegendItemOrder") != null)
				view.LegendItemOrder =
					(LegendDirection)EnumUtil.Parse(typeof(LegendDirection), tr.GetAttribute("LegendItemOrder"));

			XmlUtil.GetBoolAttribute(tr, "MainChartAreaMaximized", ref view.MainViewPanelMaximized);
			XmlUtil.GetBoolAttribute(tr, "ShowFilters", ref view.ShowFilters);
			XmlUtil.GetBoolAttribute(tr, "ShowSelectedDataRows", ref view.ShowSelectedDataRows);

			XmlUtil.GetBoolAttribute(tr, "ShowAxesTitles", ref view.ShowAxesTitles);
			XmlUtil.GetBoolAttribute(tr, "ShowAxesScaleLabels", ref view.ShowAxesScaleLabels);
			XmlUtil.GetBoolAttribute(tr, "RotateAxes", ref view.RotateAxes);

			XmlUtil.GetBoolAttribute(tr, "UseExistingCondFormatting", ref view.UseExistingCondFormatting);
			XmlUtil.GetBoolAttribute(tr, "PivotedData", ref view.PivotedData);

			while (true) // loop through elements of chart
			{
				tr.Read(); tr.MoveToContent();

				if (Lex.Eq(tr.Name, "XAxis"))
					view.XAxisMx = AxisMx.DeserializeAxis("XAxis", q, tr);

				else if (Lex.Eq(tr.Name, "YAxis"))
					view.YAxisMx = AxisMx.DeserializeAxis("YAxis", q, tr);

				else if (Lex.Eq(tr.Name, "ZAxis"))
					view.ZAxisMx = AxisMx.DeserializeAxis("ZAxis", q, tr);

				else if (Lex.Eq(tr.Name, "Axis"))
				{
					ax = AxisMx.DeserializeAxis("Axis", q, tr);
					view.AxesMx.Add(ax);
				}

				else if (Lex.Eq(tr.Name, "MarkerColor"))
				{
					view.MarkerColor = ColorDimension.Deserialize("MarkerColor", q, tr);
				}

				else if (Lex.Eq(tr.Name, "MarkerSize"))
				{
					view.MarkerSize = SizeDimension.Deserialize("MarkerSize", q, tr);
				}

				else if (Lex.Eq(tr.Name, "MarkerShape"))
				{
					view.MarkerShape = ShapeDimension.Deserialize("MarkerShape", q, tr);
				}

				else if (Lex.Eq(tr.Name, "MarkerLabel"))
				{
					view.MarkerLabel = LabelDimensionDef.Deserialize("MarkerLabel", q, tr);
				}

				else if (Lex.Eq(tr.Name, "Labels")) // obsolete label tag
				{
					view.MarkerLabel = LabelDimensionDef.Deserialize("Labels", q, tr);
				}

				else if (Lex.Eq(tr.Name, "TooltipFields"))
				{
					view.MarkerTooltip = TooltipDimensionDef.Deserialize("TooltipFields", q, tr);
				}

				else if (Lex.Eq(tr.Name, "Surface"))
				{
					txt = tr.GetAttribute("FillMode");
					if (txt != null)
						view.SurfaceFillMode = (SurfaceFillMode)EnumUtil.Parse(typeof(SurfaceFillMode), txt);

					XmlUtil.GetBoolAttribute(tr, "SmoothPalette", ref view.SmoothPalette);
					XmlUtil.GetBoolAttribute(tr, "SmoothShading", ref view.SmoothShading);
					XmlUtil.GetBoolAttribute(tr, "SemiTransparent", ref view.SemiTransparent);
					XmlUtil.GetBoolAttribute(tr, "DrawFlat", ref view.DrawFlat);

					txt = tr.GetAttribute("FrameMode");
					if (txt != null)
						view.SurfaceFrameMode = (SurfaceFrameMode)EnumUtil.Parse(typeof(SurfaceFrameMode), txt);

					if (tr.IsEmptyElement) continue; // may be just attributes

					tr.Read(); tr.MoveToContent();
					if (!Lex.Eq(tr.Name, "Surface"))
						throw new Exception("Expected Surface end element");
				}

				else if (Lex.Eq(tr.Name, "Trellis"))
				{
					if (tr.IsEmptyElement) continue; // may be just attributes

					XmlUtil.GetBoolAttribute(tr, "ByRowCol", ref view.TrellisByRowCol);

					XmlUtil.GetBoolAttribute(tr, "Manual", ref view.TrellisManual);
					XmlUtil.GetIntAttribute(tr, "MaxRows", ref view.TrellisMaxRows);
					XmlUtil.GetIntAttribute(tr, "MaxCols", ref view.TrellisMaxCols);

					while (true)
					{
						tr.Read(); tr.MoveToContent();

						if (Lex.Eq(tr.Name, "ColumnQc"))
							view.TrellisColQc = DeserializeQueryColumnRef(q, tr);

						else if (Lex.Eq(tr.Name, "RowQc"))
							view.TrellisRowQc = DeserializeQueryColumnRef(q, tr);

						else if (Lex.Eq(tr.Name, "PageQc"))
							view.TrellisPageQc = DeserializeQueryColumnRef(q, tr);

						else if (Lex.Eq(tr.Name, "FlowQc"))
							view.TrellisFlowQc = DeserializeQueryColumnRef(q, tr);

						else if (tr.NodeType == XmlNodeType.EndElement && // end of trellis definition
							Lex.Eq(tr.Name, "Trellis")) break;

						else throw new Exception("Unexpected element: " + tr.Name);
					}
				}

				else if (tr.NodeType == XmlNodeType.EndElement && // end of chart props
					Lex.Eq(tr.Name, "ChartProperties")) break;

				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return true;
		}

	} // ResultsView

	/// <summary>
	/// Axis information
	/// </summary>

	public class AxisMx
	{
		public QueryColumn QueryColumn;
		public List<QueryColumn> OrderBy; // list of columns to order by
		public string ShortName = ""; // shorter name to display on axis
		public string RangeMin = ""; // user-defined range minimum
		public string RangeMax = ""; // user-defined range maximum
		public bool IncludeOrigin = false;
		public bool SideMarginsEnabled = true; // add space for the outermost markers
		public bool ShowZoomSlider = false;
		public bool ShowGridLines = false;
		public bool ShowGridStrips = false;
		public bool LogScale = false; // log or linear scale
		public bool ReverseScale = false;
		public bool ShowLabels = true;
		public int  LabelAngle = 0; // label angle in degrees
		public bool HorizontalLabels { get { return LabelAngle == 0; } }
		public bool VerticalLabels { get { return LabelAngle == -90; } }
		public bool LabelsStaggered = false;
		public AxisLabelResolveOverlappingMode LabelResolveOverlappingMode = AxisLabelResolveOverlappingMode.None;

		public string Title // get current axis title
		{
			get
			{
				if (!String.IsNullOrEmpty(ShortName)) return ShortName;
				else return QueryColumn.ActiveLabel;
			}
		}

		public bool ShowScaleLabels(ResultsViewProps view) // see if we should show scale labels
		{
			return view.ShowAxesTitles && ShowLabels;
		}

		/// <summary>
		/// Deserialize an axis definition
		/// </summary>
		/// <param name="name"></param>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static AxisMx DeserializeAxis (
			string axisName, 
			Query q, 
			XmlTextReader tr)
		{
			Enum iEnum = null;

			AxisMx axis = new AxisMx();

			XmlUtil.GetStringAttribute(tr, "ShortName", ref axis.ShortName);

			XmlUtil.GetStringAttribute(tr, "RangeMin", ref axis.RangeMin);
			XmlUtil.GetStringAttribute(tr, "RangeMax", ref axis.RangeMax);

			XmlUtil.GetBoolAttribute(tr, "IncludeOrigin", ref axis.IncludeOrigin);
			XmlUtil.GetBoolAttribute(tr, "SideMarginsEnabled", ref axis.SideMarginsEnabled);

			XmlUtil.GetBoolAttribute(tr, "ShowLabels", ref axis.ShowLabels);
			XmlUtil.GetIntAttribute(tr, "LabelAngle", ref axis.LabelAngle);
			XmlUtil.GetBoolAttribute(tr, "LabelsStaggered", ref axis.LabelsStaggered);
			if (XmlUtil.GetEnumAttribute(tr, "LabelResolveOverlappingMode", typeof(AxisLabelResolveOverlappingMode), ref iEnum))
				axis.LabelResolveOverlappingMode = (AxisLabelResolveOverlappingMode)iEnum;

			XmlUtil.GetBoolAttribute(tr, "ShowZoomSlider", ref axis.ShowZoomSlider);
			XmlUtil.GetBoolAttribute(tr, "ShowGridLines", ref axis.ShowGridLines);
			XmlUtil.GetBoolAttribute(tr, "ShowGridStrips", ref axis.ShowGridStrips);
			XmlUtil.GetBoolAttribute(tr, "LogScale", ref axis.LogScale);
			XmlUtil.GetBoolAttribute(tr, "ReverseScale", ref axis.ReverseScale);

			tr.Read(); tr.MoveToContent();

			if (Lex.Eq(tr.Name, axisName + "Column"))
			{
				axis.QueryColumn = ResultsViewProps.DeserializeQueryColumnRef(q, tr);
				tr.Read(); tr.MoveToContent();
			}

			if (!Lex.Eq(tr.Name, axisName))
				throw new Exception("Expected " + axisName + " end element");

			return axis;
		}

		/// <summary>
		/// Memberwise clone
		/// </summary>
		/// <returns></returns>

		public AxisMx Clone()
		{
			AxisMx ax = (AxisMx)this.MemberwiseClone();
			return ax;
		}
	} // AxisEx

	/// <summary>
	/// Enumeration for label rows to be shown
	/// </summary>

	public enum LabelVisibilityModeEnum
	{
		Unknown = 0,
		AllRows = 1,
		MarkedRows = 2,
		RolloverRow = 3
	}

	//public enum LabelPositionEnum
	//{
	//	Center = 0,
	//	Outside = 1
	//}

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

#endif
	}
}
