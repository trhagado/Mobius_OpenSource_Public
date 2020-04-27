using Mobius.ComOps;

//using Smrf.NodeXL.Core;
//using Smrf.NodeXL.Layouts;
//using Smrf.NodeXL.Algorithms;
//using Smrf.NodeXL.Visualization.Wpf;
//using Smrf.NodeXL.ApplicationUtil;
//using Smrf.NodeXL.ExcelTemplate;

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
/// Network view properties that are part of the ResultsView
/// Members are included in ResultsView class
/// </summary>

	public class NetworkProperties
	{
		public static bool NetworkPropertiesEnabled = true; // Set to true if network properties are enabled

		//public GraphDirectedness GraphDirectedness = GraphDirectedness.Undirected; // undirected initially
		public Color SelectedColor = DefaultSelectedColor; // color of selected vertices and edges

		public VertexMx Vertex1 = new VertexMx(); // first vertex
		public VertexMx Vertex2 = new VertexMx(); // second vertex if different from first

		public EdgeMx Edge = new EdgeMx(); // edge properties

		public LayoutSettingsMx LayoutSettings = new LayoutSettingsMx();

		public VertexGroupMethodMx VertexGroupMethod = VertexGroupMethodMx.FieldValue;
		public QueryColumn GroupByQc; // QueryColumn to group by if VertexGroupMethod = VertexGroupMethod.Attribute
		public bool GroupingDisabled = false; // grouping temporarily disabled

		public double GraphScale = 1.0; // relative size of vertex markers and line widths
		public double GraphZoom = 1.0;

		public static Color DefaultColor = Color.Black; // default color for drawing vertices & edges
		public static Color DefaultSelectedColor = Color.Red; // default color for drawing selected vertices & edges

/// <summary>
/// Constructor
/// </summary>

		public NetworkProperties()
		{
			//Vertex1.Size.FixedSize = NetworkProperties.GraphScaleToPctScale(GraphScale);
			//Vertex1.Shape.FixedShape = (int)VertexShape.Circle;

			//Vertex2.Size.FixedSize = NetworkProperties.GraphScaleToPctScale(GraphScale);
			//Vertex2.Shape.FixedShape = (int)VertexShape.Diamond;

			//Edge.Width.FixedSize = NetworkProperties.GraphScaleToPctScale(GraphScale);

			return;
		}

		/// <summary>
		/// Serialize Network View properties
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			XmlTextWriter tw = mstw.Writer;
			tw.Formatting = Formatting.Indented;
			Serialize(tw);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		/// <summary>
		/// Serialize Network View Properties
		/// </summary>
		/// <param name="tw"></param>

		public void Serialize(XmlTextWriter tw)
		{
			if (!NetworkPropertiesEnabled) return;

			tw.WriteStartElement("NetworkProperties");

			//tw.WriteAttributeString("GraphDirectedness", GraphDirectedness.ToString());

			LayoutSettingsMx s = LayoutSettings; // layout settings
			//tw.WriteAttributeString("LayoutType", s.LayoutType.ToString());
			//tw.WriteAttributeString("LayoutStyle", s.LayoutStyle.ToString());
			//tw.WriteAttributeString("GroupRectanglePenWidth", s.GroupRectanglePenWidth.ToString());
			//tw.WriteAttributeString("IntergroupEdgeStyle", s.IntergroupEdgeStyle.ToString());
			//tw.WriteAttributeString("ImproveLayoutOfGroups", s.ImproveLayoutOfGroups.ToString());
			//tw.WriteAttributeString("MaximumVerticesPerBin", s.MaximumVerticesPerBin.ToString());
			//tw.WriteAttributeString("BinLength", s.BinLength.ToString());
			//tw.WriteAttributeString("FruchtermanReingoldC", s.FruchtermanReingoldC.ToString());
			//tw.WriteAttributeString("FruchtermanReingoldIterations", s.FruchtermanReingoldIterations.ToString());
			//tw.WriteAttributeString("Margin", s.Margin.ToString());

			//tw.WriteAttributeString("VertexGroupMethod", VertexGroupMethod.ToString());
			//tw.WriteAttributeString("GroupingDisabled", GroupingDisabled.ToString());
			//tw.WriteAttributeString("GraphScale", GraphScale.ToString());
			
			Vertex1.Serialize("Vertex1", tw);
			Vertex2.Serialize("Vertex1", tw);

			Edge.Serialize("Edge", tw);
			ResultsViewProps.SerializeQueryColumn(GroupByQc, "GroupByQc", tw);

			tw.WriteEndElement(); // NetworkProperties
			return;
		}

		/// <summary>
		/// Deserialize into an existing view
		/// </summary>
		/// <param name="serializedPropString"></param>
		/// <param name="q"></param>
		/// <param name="view"></param>

		public static void Deserialize(
			string serializedPropString,
			Query q,
			ResultsViewProps view)
		{
			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedPropString);
			XmlTextReader tr = mstr.Reader;
			tr.MoveToContent();
			Deserialize(q, tr, view);
			tr.Close();
			return;
		}

/// <summary>
/// Deserialize
/// </summary>
/// <param name="q"></param>
/// <param name="tr"></param>
/// <param name="view"></param>
/// <returns></returns>

		public static bool Deserialize(
			Query q,
			XmlTextReader tr,
			ResultsViewProps view)
		{
			Enum iEnum = null;
			bool b1 = false;
			string txt = "";
			int i1 = -1;
			double d1 = -1;

			if (!Lex.Eq(tr.Name, "NetworkProperties")) return false;

			if (view.NetworkProperties == null)
				view.NetworkProperties = new NetworkProperties();

			NetworkProperties p = view.NetworkProperties;

			//if (XmlUtil.GetEnumAttribute(tr, "GraphDirectedness", typeof(GraphDirectedness), ref iEnum))
			//	p.GraphDirectedness = (GraphDirectedness)iEnum;

			//LayoutSettingsMx s = p.LayoutSettings; // layout settings

			//if (XmlUtil.GetEnumAttribute(tr, "LayoutType", typeof(LayoutType), ref iEnum))
			//	s.LayoutType = (LayoutType)iEnum;

			//if (XmlUtil.GetEnumAttribute(tr, "LayoutStyle", typeof(LayoutStyle), ref iEnum))
			//	s.LayoutStyle = (LayoutStyle)iEnum;
			//XmlUtil.GetDoubleAttribute(tr, "GroupRectanglePenWidth", ref s.GroupRectanglePenWidth);
			//if (XmlUtil.GetEnumAttribute(tr, "IntergroupEdgeStyle", typeof(IntergroupEdgeStyle), ref iEnum))
			//	s.IntergroupEdgeStyle = (IntergroupEdgeStyle)iEnum;
			//XmlUtil.GetBoolAttribute(tr, "ImproveLayoutOfGroups", ref s.ImproveLayoutOfGroups);
			//XmlUtil.GetIntAttribute(tr, "MaximumVerticesPerBin", ref s.MaximumVerticesPerBin);
			//XmlUtil.GetIntAttribute(tr, "BinLength", ref s.BinLength);
			//XmlUtil.GetFloatAttribute(tr, "FruchtermanReingoldC", ref s.FruchtermanReingoldC);
			//XmlUtil.GetIntAttribute(tr, "FruchtermanReingoldIterations", ref s.FruchtermanReingoldIterations);
			//XmlUtil.GetIntAttribute(tr, "Margin", ref s.Margin);

			//if (XmlUtil.GetEnumAttribute(tr, "VertexGroupMethod", typeof(VertexGroupMethodMx), ref iEnum))
			//	p.VertexGroupMethod = (VertexGroupMethodMx)iEnum;
			//XmlUtil.GetBoolAttribute(tr, "GroupingDisabled", ref p.GroupingDisabled);
			//XmlUtil.GetDoubleAttribute(tr, "GraphScale", ref p.GraphScale);

			if (tr.IsEmptyElement) return true; // return if no elements

			while (true) // loop through elements of network
			{
				tr.Read(); tr.MoveToContent();

				if (Lex.Eq(tr.Name, "Vertex1"))
					p.Vertex1 = VertexMx.Deserialize("Vertex1", q, tr);

				else if (Lex.Eq(tr.Name, "Vertex2"))
					p.Vertex2 = VertexMx.Deserialize("Vertex2", q, tr);

				else if (Lex.Eq(tr.Name, "Edge"))
					p.Edge = EdgeMx.Deserialize("Edge", q, tr);

				else if (Lex.Eq(tr.Name, "GroupByQc"))
				{
					p.GroupByQc = ResultsViewProps.DeserializeQueryColumn(q, tr);
					tr.Read(); tr.MoveToContent();
				}

				else if (tr.NodeType == XmlNodeType.EndElement && // end of props
					Lex.Eq(tr.Name, "NetworkProperties")) break;

				else throw new Exception("Unexpected element: " + tr.Name);

			}

			return true;
		}

		/// <summary>
		///  GraphZoomToPctZoom
		/// </summary>
		/// <param name="zoom"></param>
		/// <returns></returns>

		//public static int GraphZoomToPctZoom(double zoom)
		//{
		//	double pct = GeometryMx.ConvertValueBetweenRanges(
		//		zoom,
		//		//NodeXLControl.MinimumGraphZoom,
		//		//NodeXLControl.MaximumGraphZoom,
		//		1, 100);
		//	return (int)pct;
		//}

		/// <summary>
		/// PctZoomToGraphZoom
		/// </summary>
		/// <param name="pct"></param>
		/// <returns></returns>

		//public static double PctZoomToGraphZoom(int pct)
		//{
		//	double zoom = GeometryMx.ConvertValueBetweenRanges(
		//		pct, 1, 100,
		//		NodeXLControl.MinimumGraphZoom,
		//		NodeXLControl.MaximumGraphZoom);

		//	return zoom;
		//}

		/// <summary>
		/// GraphScaleToPctScale
		/// </summary>
		/// <param name="zoom"></param>
		/// <returns></returns>

		//public static int GraphScaleToPctScale(double zoom)
		//{
		//	double pct = GeometryMx.ConvertValueBetweenRanges(
		//		zoom,
		//		//NodeXLControl.MinimumGraphScale,
		//		//NodeXLControl.MaximumGraphScale,
		//		1, 100);
		//	return (int)pct;
		//}

		/// <summary>
		/// PctScaleToGraphScale
		/// </summary>
		/// <param name="pct"></param>
		/// <returns></returns>

		//public static double PctScaleToGraphScale(int pct)
		//{
		//	double zoom = GeometryMx.ConvertValueBetweenRanges(
		//		pct, 1, 100,
		//		//NodeXLControl.MinimumGraphScale,
		//		//NodeXLControl.MaximumGraphScale);

		//	return zoom;
		//}
	}

/// <summary>
/// Layout settings stored in ResultsViewNetwork
/// </summary>

	public class LayoutSettingsMx
	{
		//*************************************************************************
		//  Constructor: LayoutUserSettings()
		//
		/// <summary>
		/// Initializes a new instance of the LayoutUserSettings class.
		/// </summary>
		//*************************************************************************

		public LayoutSettingsMx()
		{
			return;
		}

		//*************************************************************************
		//  Property: LayoutType
		//
		/// <summary>
		/// Gets or sets the layout type to use.
		/// </summary>
		///
		/// <value>
		/// The layout type to use, as a <see cref="LayoutType" />.  The default is
		/// <see cref="LayoutType.FruchtermanReingold" />.
		/// </value>
		//*************************************************************************

		//public LayoutType LayoutType = LayoutType.FruchtermanReingold;

		//*************************************************************************
		//  Property: LayoutStyle
		//
		/// <summary>
		/// Gets or sets the style to use when laying out the graph.
		/// </summary>
		///
		/// <value>
		/// The style to use when laying out the graph.  The default value is
		/// <see cref="Smrf.NodeXL.Layouts.LayoutStyle.Normal" />.
		/// </value>
		//*************************************************************************

		//public LayoutStyle LayoutStyle = LayoutStyle.Normal;

		//*************************************************************************
		//  Property: GroupRectanglePenWidth
		//
		/// <summary>
		/// Gets or sets the width of the pen used to draw group rectangles.
		/// </summary>
		///
		/// <value>
		/// The width of the pen used to draw group rectangles.  The default value
		/// is 1.0.
		/// </value>
		//*************************************************************************

		public Double GroupRectanglePenWidth = 1.0;

		//*************************************************************************
		//  Property: IntergroupEdgeStyle
		//
		/// <summary>
		/// Gets or sets a value that specifies how the edges that connect vertices
		/// in different groups should be shown.
		/// </summary>
		///
		/// <value>
		/// A value that specifies how the intergroup edges should be shown.  The
		/// default value is <see
		/// cref="Smrf.NodeXL.Layouts.IntergroupEdgeStyle.Show" />.
		/// </value>
		//*************************************************************************

		//public IntergroupEdgeStyle IntergroupEdgeStyle = IntergroupEdgeStyle.Show;

		//*************************************************************************
		//  Property: ImproveLayoutOfGroups
		//
		/// <summary>
		/// Gets or sets a flag indicating whether the layout should attempt to
		/// improve the appearance of groups.
		/// </summary>
		///
		/// <value>
		/// true to attempt to improve the appearance of groups.  The default value
		/// is false.
		/// </value>
		//*************************************************************************

		public Boolean ImproveLayoutOfGroups = false;

		//*************************************************************************
		//  Property: MaximumVerticesPerBin
		//
		/// <summary>
		/// Gets or sets the maximum number of vertices a binned component can
		/// have.
		/// </summary>
		///
		/// <value>
		/// The maximum number of vertices a binned component can have.  The
		/// default value is 3.
		/// </value>
		//*************************************************************************

		public Int32 MaximumVerticesPerBin = 3;

		//*************************************************************************
		//  Property: BinLength
		//
		/// <summary>
		/// Gets or sets the height and width of each bin, in graph rectangle
		/// units.
		/// </summary>
		///
		/// <value>
		/// The height and width of each bin, in graph rectangle units.  The
		/// default value is 16.
		/// </value>
		//*************************************************************************

		public Int32 BinLength = 16;

		//*************************************************************************
		//  Property: FruchtermanReingoldC
		//
		/// <summary>
		/// Gets or sets the constant that determines the strength of the
		/// attractive and repulsive forces between the vertices when using the
		/// FruchtermanReingoldLayout.
		/// </summary>
		///
		/// <value>
		/// The "C" constant in the "Modelling the forces" section of the
		/// Fruchterman-Reingold paper.  Must be greater than 0.  The default value
		/// is 3.0.
		/// </value>
		//*************************************************************************

		public Single FruchtermanReingoldC = 3.0f;

		//*************************************************************************
		//  Property: FruchtermanReingoldIterations
		//
		/// <summary>
		/// Gets or sets the number of times to run the Fruchterman-Reingold
		/// algorithm when using the FruchtermanReingoldLayout.
		/// </summary>
		///
		/// <value>
		/// The number of times to run the Fruchterman-Reingold algorithm when the
		/// graph is laid out, as an Int32.  Must be greater than zero.  The
		/// default value is 10.
		/// </value>
		//*************************************************************************

		public Int32 FruchtermanReingoldIterations = 10;

		//*************************************************************************
		//  Property: Margin
		//
		/// <summary>
		/// Gets or sets the margin to subtract from each edge of the graph
		/// rectangle before laying out the graph.
		/// </summary>
		///
		/// <value>
		/// The margin to subtract from each edge.  Must be greater than or equal
		/// to zero.  The default value is 6.
		/// </value>
		//*************************************************************************

		public Int32 Margin = 6;

	}

/// <summary>
/// Mobius vertex properties
/// </summary>

	public class VertexMx
	{
		public QueryColumn QueryColumn;
		public ShowLabels ShowLabels = ShowLabels.All;
		public SizeDimension Size = new SizeDimension(); // col to size the vertex (Fixed size used to set GraphScale) 
		public ColorDimension Color = new ColorDimension(NetworkProperties.DefaultColor);
		public ShapeDimension Shape = new ShapeDimension();
		public TooltipDimensionDef TooltipFields = new TooltipDimensionDef();

		public SummarizationType SummarizationType = SummarizationType.None;

// "VertexRadius" size of vertices

		public static float MinRadius = 1; // minimum "radius" (note: VertexDrawer.MimimumRadius = .1)
		public static float MaxRadius = 30; // maximum "radius" (note: VertexDrawer.MaximumRadius = 549)
		public static float DefaultRadius = 3;

/// <summary>
/// Default constructor
/// </summary>

		public VertexMx()
		{
			return;
		}

		/// <summary>
		/// Serialize vertex
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(string name, XmlTextWriter tw)
		{
			if (QueryColumn == null) return;

			tw.WriteStartElement(name);

			tw.WriteAttributeString("ShowLabels", ShowLabels.ToString());

			ResultsViewProps.SerializeQueryColumn(QueryColumn, name + "Column", tw);
			Color.Serialize("Color", tw);
			Size.Serialize("Size", tw);
			Shape.Serialize("Shape", tw);
			TooltipFields.Serialize("ToolTipFields", tw);

			tw.WriteEndElement();
			return;
		}

/// <summary>
/// Deserialize
/// </summary>
/// <param name="vertexName"></param>
/// <param name="q"></param>
/// <param name="tr"></param>
/// <returns></returns>

		internal static VertexMx Deserialize(
			string name,
			Query q,
			XmlTextReader tr)
		{
			Enum iEnum = null;

			VertexMx v = new VertexMx();

			if (XmlUtil.GetEnumAttribute(tr, "ShowLabels", typeof(ShowLabels), ref iEnum))
				v.ShowLabels = (ShowLabels)iEnum;

			while (true) // loop through elements of network
			{
				tr.Read(); tr.MoveToContent();

				if (Lex.Eq(tr.Name, name + "Column"))
				{
					v.QueryColumn = ResultsViewProps.DeserializeQueryColumn(q, tr);
					//tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "Color"))
				{
					v.Color = ColorDimension.Deserialize("Color", q, tr);
					//tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "Size"))
				{
					v.Size = SizeDimension.Deserialize("Size", q, tr);
					//tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "Shape"))
				{
					v.Shape = ShapeDimension.Deserialize("Shape", q, tr);
					//tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "TooltipFields"))
				{
					v.TooltipFields = TooltipDimensionDef.Deserialize("TooltipFields", q, tr);
					//tr.Read(); tr.MoveToContent();
				}

				else if (tr.NodeType == XmlNodeType.EndElement && // end of props
					Lex.Eq(tr.Name, name)) break;

				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return v;
		}

	}

/// <summary>
/// Mobius edge properties
/// </summary>

	public class EdgeMx
	{
		public QueryColumn QueryColumn; // column supplying the label, overrides the shape
		public ShowLabels ShowLabels = ShowLabels.All;
		public SizeDimension Width = new SizeDimension(); 

		public ColorDimension Color = new ColorDimension(NetworkProperties.DefaultColor);
		public ShapeDimension LineStyle = new ShapeDimension(); // LineStyle not currently used
		public TooltipDimensionDef TooltipFields = new TooltipDimensionDef(); // not supported by NodeXL

		public bool IncludeEdgeVertex = false;
		public SummarizationType SummarizationType = SummarizationType.None; // how to summarize multiple edges between a pair of vertices

/// <summary>
/// Line thickness values
/// </summary>

		public static float MinWidth = 1; // minimum size, note: EdgeDrawer.MinimumWidth  = 1
		public static float MaxWidth = 5; // maximum size, note: EdgeDrawer.MaximumWidth = 20
		public static float DefaultWidth = 1;

/// <summary>
/// Default constructor
/// </summary>

		public EdgeMx()
		{
			return;
		}

		/// <summary>
		/// Serialize edge
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(string name, XmlTextWriter tw)
		{
			if (QueryColumn == null) return;

			tw.WriteStartElement(name);

			tw.WriteAttributeString("ShowLabels", ShowLabels.ToString());
			tw.WriteAttributeString("SummarizationType", SummarizationType.ToString());
			tw.WriteAttributeString("IncludeEdgeVertex", IncludeEdgeVertex.ToString());

			ResultsViewProps.SerializeQueryColumn(QueryColumn, name + "Column", tw);
			Color.Serialize("Color", tw);
			Width.Serialize("Width", tw);
			TooltipFields.Serialize("ToolTipFields", tw);

			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static EdgeMx Deserialize(
			string name,
			Query q,
			XmlTextReader tr)
		{
			Enum iEnum = null;

			EdgeMx e = new EdgeMx();

			if (XmlUtil.GetEnumAttribute(tr, "ShowLabels", typeof(ShowLabels), ref iEnum))
				e.ShowLabels = (ShowLabels)iEnum;

			if (XmlUtil.GetEnumAttribute(tr, "SummarizationType", typeof(SummarizationType), ref iEnum))
				e.SummarizationType = (SummarizationType)iEnum;

			XmlUtil.GetBoolAttribute(tr, "IncludeEdgeVertex", ref e.IncludeEdgeVertex);

			while (true) // loop through elements of network
			{
				tr.Read(); tr.MoveToContent();

				if (Lex.Eq(tr.Name, name + "Column"))
				{
					e.QueryColumn = ResultsViewProps.DeserializeQueryColumn(q, tr);
					tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "Color"))
				{
					e.Color = ColorDimension.Deserialize("Color", q, tr);
					tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "Width"))
				{
					e.Width = SizeDimension.Deserialize("Width", q, tr);
					tr.Read(); tr.MoveToContent();
				}

				else if (Lex.Eq(tr.Name, "TooltipFields"))
				{
					e.TooltipFields = TooltipDimensionDef.Deserialize("TooltipFields", q, tr);
					//tr.Read(); tr.MoveToContent();
				}

				else if (tr.NodeType == XmlNodeType.EndElement && // end of props
					Lex.Eq(tr.Name, name)) break;

				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return e;
		}

	}

/// <summary>
/// Vertex shape 
/// </summary>

// Matches Smrf.NodeXL.Visualization.Wpf.VertexShape

	public enum VertexShapeMx
{
		Undefined = -1,
		Circle = 0,
		Square = 3,
		Diamond = 5, 
		Triangle = 7
	}

/// <summary>
/// How grouping of vertices should be done
/// </summary>

	public enum VertexGroupMethodMx
	{
		Unknown = -1,
		FieldValue = 0, // group based on field value
		ConnectedComponent = 1, // group based on connectivity
		WakitaTsurumi = 2, // Following 3 cluster methods from enum ClusterAlgorithm
		GirvanNewman = 3,
		ClausetNewmanMoore = 4
	}

}
