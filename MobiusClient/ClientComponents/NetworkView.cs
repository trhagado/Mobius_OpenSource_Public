using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using Smrf.NodeXL.Core;
using Smrf.NodeXL.Layouts;
using Smrf.NodeXL.Algorithms;
using Smrf.NodeXL.Visualization.Wpf;
using Smrf.NodeXL.ApplicationUtil;

using DevExpress.XtraEditors;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Data;
using System.Text;
using System.Drawing;
using SD = System.Drawing;
using System.Windows.Media;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Windows.Forms.Integration;

namespace Mobius.ClientComponents
{

/// <summary>
/// Manages the network view
/// </summary>

	public class NetworkView : ViewManager
	{
		internal NetworkPageControl PageControl; // the page control allocated to the view 
		internal NetworkPagePanel NetworkPagePanel { get { return PageControl.NetworkPagePanel; } set { PageControl.NetworkPagePanel = value; } }
		internal NetworkPanel NetworkPanel { get { return PageControl != null ? PageControl.NetworkPagePanel.NetworkPanel : null; } }
		internal NodeXLControl NetworkControl { get { return NetworkPanel != null ? NetworkPanel.NodeXLControl : null; } } 
		internal IGraph Graph { get { return NetworkControl.Graph; } }

		Dictionary<string, IVertex> VertexNameToVertexMap; // vertex name to Vertex 

		Dictionary<string, List<int>> Vertex1NameToRowMap; // vertex name to associated list of row indexes linking to Vertex1 vertices

		Dictionary<string, List<int>> Vertex2NameToRowMap; // edge name to associated list of row indexes linking to Vertex2 vertices

		Dictionary<string, IEdge> EdgeNameToEdgeMap; // edge name to Edge

		Dictionary<string, AssocVertices> EdgeNameToVertex1NameMap; // edge name to associated list of row indexes linking to Vertex1 vertices

		Dictionary<string, AssocVertices> EdgeNameToVertex2NameMap; // edge name to associated list of row indexes linking to Vertex2 vertices

		internal static VisibilityKeyValue FilteredElementVisibility = VisibilityKeyValue.Hidden; // global visibility of filtered items

		/// <summary>
		/// Basic constructor
		/// </summary>

		public NetworkView()
		{
			ViewType = ViewTypeMx.Network;
			Title = "Network View";
		}

		/// <summary>
		/// Return true if the view has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				if (NetworkProperties != null && NetworkProperties.Vertex1.QueryColumn != null)
					return true;

				else return false;
			}
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			PageControl = new NetworkPageControl();
			NetworkPanel.View = this; // link the rendering control to us
			NetworkPanel.ElementHost.Visible = false; // control is initially hidden
			NetworkPanel.Dock = DockStyle.Fill;
			IdForSession = IdForSession;
			RenderingControl = NetworkPanel;
			return RenderingControl;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel panel)
		{
			panel.Controls.Clear(); // remove any control current in the panel
			panel.Controls.Add(NetworkPagePanel); // add our rendering control to the display panel 

			InsertToolsIntoDisplayPanel();

			PageControl.EditQueryBut.Enabled = ViewManager.IsControlContainedInQueriesControl(panel);

			return;
		}

/// <summary>
/// Insert tools into display panel
/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			int zoomPct = NetworkProperties.GraphZoomToPctZoom(NetworkControl.GraphZoom);
			Qrc.SetToolBarTools(PageControl.ToolPanel, zoomPct); // show the proper tools
			return;
		}

		/// <summary>
		/// Zoom the view to the specified pct value
		/// </summary>
		/// <param name="pct"></param>

		public override void ScaleView(int pct)
		{
			NetworkPanel.ZoomView(pct);
		}

		/// <summary>
		/// Show the properties for the view
		/// </summary>
		/// <returns></returns>

		public override DialogResult ShowPropertiesDialog()
		{
			DialogResult dr = NetworkPropertiesDialog.ShowDialog(this);
			return dr;
		}

		/// <summary>
		/// Render the view
		/// </summary>
		/// <param name="queryResultsControl"></param>

		public override void ConfigureRenderingControl()
		{
			AssureQueryManagerIsDefined(Query);
			DialogResult dr = Qm.DataTableManager.CompleteRetrieval();
			if (dr == DialogResult.Cancel) return;

			ConfigureNetworkControl(); // configure the control to display the data

			NetworkPanel.ElementHost.Visible = true; // be sure the control is visible
			ConfigureCount++;

			return;
		}

/// <summary>
/// Create and configure the network control
/// </summary>

		internal void ConfigureNetworkControl()
		{
			NetworkPanel.View = this; // link the panel to this view

			NodeXLControl c = NetworkControl;
			if (c == null) return;

			NetworkProperties p = NetworkProperties;
			if (p == null) return;

			DialogResult dr = Qm.DataTableManager.CompleteRetrieval();
			if (dr == DialogResult.Cancel) return;

			//if (Math.Abs(1) == 1) // debug, just draw a simple graph
			//{
			//  NetworkPanel.PopulateAndDrawGraph();
			//  return;
			//}

// Set control-level parameters

			VertexMx v = p.Vertex1;
			if (v != null)
			{
				c.VertexColor = GraphicsHelper.ConvertDrawingColorToMediaColor(v.Color.FixedColor);
				c.VertexRadius = VertexMx.DefaultRadius;
				c.VertexShape = (VertexShape)v.Shape.FixedShape;
				c.VertexLabelFillColor = GraphicsHelper.ConvertDrawingColorToMediaColor(v.Color.FixedColor);
				//c.VertexLabelPosition = ...
			}

			c.VertexSelectedColor = GraphicsHelper.ConvertDrawingColorToMediaColor(p.SelectedColor);

			EdgeMx e = p.Edge;
			if (e != null)
			{
				c.EdgeColor = GraphicsHelper.ConvertDrawingColorToMediaColor(e.Color.FixedColor);
				c.EdgeWidth = EdgeMx.DefaultWidth;
			}

			c.EdgeSelectedColor = GraphicsHelper.ConvertDrawingColorToMediaColor(p.SelectedColor);

			//c.EdgeWidth = e.EdgeWidthFixed;

			//Typeface tf = new Typeface("Arial");
			//double fontSize = 12.0; // font size to use in WPF units
			//c.SetFont(tf, fontSize); // set label font & size

			c.GraphScale = p.GraphScale;
			c.GraphZoom = p.GraphZoom;

// Update the QueryColumn references to match the current query

			UpdateColumnReferences();

// Build the graph

			BuildGraph();

// Layout the graph

			if (!NetworkPanel.ElementHost.Visible) // control must be visible to get proper size for layout
				NetworkPanel.ElementHost.Visible = true;

			LayoutGraph();

// Draw the graph

			DrawGraph();

			c.AllowVertexDrag = true;
			c.MouseAlsoSelectsIncidentEdges = true;
			//c.MouseMode = MouseMode...

			c.ShowVertexToolTips = true;

			return;
		}

/// <summary>
/// Update QueryColumn references to match the current query
/// </summary>

		void UpdateColumnReferences()
		{
			NetworkProperties p = NetworkProperties;

			UpdateColumnReferences(p.Vertex1);
			UpdateColumnReferences(p.Vertex2);

			UpdateColumnReferences(p.Edge);
			UpdateColumnReference(ref p.GroupByQc);
		}

		void UpdateColumnReferences(VertexMx v)
		{
			UpdateColumnReference(ref v.QueryColumn);
			UpdateColumnReference(ref v.Size.QueryColumn);
			UpdateColumnReference(ref v.Color.QueryColumn);
			UpdateColumnReference(ref v.Shape.QueryColumn);
		}

		void UpdateColumnReferences(EdgeMx e)
		{
			UpdateColumnReference(ref e.QueryColumn);
			UpdateColumnReference(ref e.Width.QueryColumn);
			UpdateColumnReference(ref e.Color.QueryColumn);
			UpdateColumnReference(ref e.LineStyle.QueryColumn);
		}

		/// <summary>
		/// Build the graph vertices and edges and their associated dimensional values
		/// </summary>

		internal void BuildGraph()
		{
			NodeXLControl c = NetworkControl;
			NetworkProperties p = NetworkProperties;
			Graph g = new Graph(p.GraphDirectedness);
			c.Graph = g; // make this the control graph

			IVertexCollection vertices = g.Vertices;
			IEdgeCollection edges = g.Edges;

			if (p.Vertex1.QueryColumn == null || 
				(p.Vertex2.QueryColumn == null && p.Edge.QueryColumn == null))
			{ // if not enough defined to draw a graph then return empty graph
				return;
			}

			BuildVertexAndEdgeMaps();

			if (p.Edge.QueryColumn != null) // build edges if joined by common col value
			{
				BuildEdges();
				UpdateGraphAttributes();
			}

			else
			{
				throw new NotImplementedException();
			}

			return;
		}

/// <summary>
/// Update the graph edge and vertex attributes including filter-related visibility
/// </summary>

		void UpdateGraphAttributes()
		{
			UpdateVertexVisibilityAndLabels();
			UpdateEdgeVisibilityAndLabels();
			DrawGraph();

			return;
		}

/// <summary>
/// Build maps of vertices their edges and the associated attributes
/// </summary>

		internal void BuildVertexAndEdgeMaps()
		{
			QueryColumn qc;
			MetaColumn mc;
			GetVertexArgs vertexArgs = null, vertexArgs2 = null;
			GetEdgeArgs edgeArgs = null;
			IVertex vertex1 = null, vertex2 = null;
			EdgeVals edgeVals;
			String edgeName, vertex1Name, vertex2Name;
			bool newVertex, newEdge;
			int missingCount = 0;
			object o;

			NetworkProperties p = NetworkProperties;

			IGraph g = NetworkControl.Graph;
			g.Vertices.Clear();

			VertexNameToVertexMap = new Dictionary<string, IVertex>();
			Vertex1NameToRowMap = new Dictionary<string, List<int>>();
			Vertex2NameToRowMap = new Dictionary<string, List<int>>();

			EdgeNameToEdgeMap = new Dictionary<string, IEdge>();
			EdgeNameToVertex1NameMap = new Dictionary<string, AssocVertices>();
			EdgeNameToVertex2NameMap = new Dictionary<string, AssocVertices>();

			vertexArgs = new GetVertexArgs(Qm, p.Vertex1);

			if (p.Vertex2.QueryColumn != null && p.Vertex2.QueryColumn != p.Vertex1.QueryColumn)
				vertexArgs2 = // build args to get 2nd vertex if defined & different from first
					new GetVertexArgs(Qm, p.Vertex2);

			if (p.Edge.QueryColumn != null) // build args to get edge prop values
				edgeArgs = new GetEdgeArgs(Qm, p.Edge);

// Scan each row in data table and link to associated vertice[s] and edge

			qc = p.Vertex1.QueryColumn;
			int ti = qc.QueryTable.Position;
			for (int dri = 0; dri < Qm.DataTable.Rows.Count; dri++)
			{
				//int dri2 = // take filtering into account (do we want to do that here?)
				//  Qm.DataTableManager.AdjustDataRowToCurrentDataForTable(dri, ti, true);
				int dri2 = dri; // don't adjust for initial build
				DataRowMx dr = Qm.DataTable.Rows[dri2];
				DataRowAttributes dra = Dtm.GetRowAttributes(dr);
				string key = Dtm.GetRowKey(dr);
				if (String.IsNullOrEmpty(key)) continue;

				vertex1Name = GetVoString(p.Vertex1.QueryColumn, dri2);
				if (Lex.IsNullOrEmpty(vertex1Name)) continue;

				if (p.Edge.QueryColumn != null)
				{
					edgeName = GetVoString(p.Edge.QueryColumn, dri2);
					AddRowToEdgeMap(dri2, edgeName, vertex1Name, EdgeNameToVertex1NameMap);
					if (p.Vertex2.QueryColumn != null) // lookup or build the 2nd vertex if defined & different from first
					{
						vertex2Name = GetVoString(p.Vertex2.QueryColumn, dri2);
						if (!Lex.IsNullOrEmpty(vertex2Name))
							AddRowToEdgeMap(dri2, edgeName, vertex2Name, EdgeNameToVertex2NameMap);
					}
				}

				else // no column associated with edge, 
				{
					throw new NotImplementedException();
				}

				AddRowToVertexMap(dri2, vertex1Name, Vertex1NameToRowMap);
				AddVertexToGraphIfNew(vertex1Name, vertex1Name, Vertex1NameToRowMap);

				if (p.Vertex2.QueryColumn != null) // lookup or build the 2nd vertex if defined & different from first
				{
					vertex2Name = GetVoString(p.Vertex2.QueryColumn, dri2);
					if (!Lex.IsNullOrEmpty(vertex2Name))
					{ // build list of rows associated with 2nd vertex
						string vertex2Label = vertex2Name;
						vertex2Name = "V2." + vertex2Name; // differentiate name with "V2." prefix to avoid collision with vertex 1 names
						AddRowToVertexMap(dri2, vertex2Name, Vertex2NameToRowMap);
						AddVertexToGraphIfNew(vertex2Name, vertex2Label, Vertex2NameToRowMap);
					}
				}
			}

			return;
		}

/// <summary>
/// Add the named vertex to the graph if it's not yet in the graph
/// </summary>
/// <param name="vertexName"></param>
/// <param name="vertexLabel"></param>
/// <param name="g"></param>
/// <returns></returns>

		IVertex AddVertexToGraphIfNew(
			string vertexName, 
			string vertexLabel,
			Dictionary<string, List<int>> vertexMap)
		{
			IVertex vertex;

			if (FindVertex(vertexName, out vertex))
				return vertex;

			vertex = Graph.Vertices.Add();
			VertexNameToVertexMap[vertexName] = vertex;
			vertex.Name = vertexName;
			if (NetworkProperties.Vertex1.ShowLabels == ShowLabels.All)
				vertex.SetValue(ReservedMetadataKeys.PerVertexLabel, vertexLabel);

			VertexTag tag = new VertexTag();
			tag.RowList = vertexMap[vertexName]; // assign the row list
			vertex.Tag = tag; 

			return vertex;
		}

/// <summary>
/// Add row to edge map organized by vertex name
/// </summary>
/// <param name="ri"></param>
/// <param name="qc"></param>
/// <param name="map"></param>
/// <param name="name"></param>
/// <returns></returns>

		bool AddRowToEdgeMap(
			int ri,
			string edgeName,
			string vertexName,
			Dictionary<string, AssocVertices> map)
		{
			bool added = false;

			if (Lex.IsNullOrEmpty(edgeName) || Lex.IsNullOrEmpty(vertexName)) return false;

			if (!map.ContainsKey(edgeName))
			{
				map[edgeName] = new AssocVertices();
				added = true;
			}

			AssocVertices av = map[edgeName];
			if (!av.Dict.ContainsKey(vertexName))
				av.Dict[vertexName] = new List<int>();

			av.Dict[vertexName].Add(ri);
			return added;
		}

/// <summary>
/// Add row to map for vertex 
/// </summary>
/// <param name="ri"></param>
/// <param name="vertexName"></param>
/// <param name="map"></param>
/// <returns></returns>

		bool AddRowToVertexMap(
			int ri,
			string vertexName,
			Dictionary<string, List<int>> map)
		{
			bool added = false;

			if (Lex.IsNullOrEmpty(vertexName)) return false;

			if (!map.ContainsKey(vertexName))
			{
				map[vertexName] = new List<int>();
				added = true;
			}

			map[vertexName].Add(ri);
			return added;
		}

/// <summary>
/// Args for GetVertex method
/// </summary>

		class GetVertexArgs
		{
			public VertexMx vertexMx;
			public int vertexVoi, sizeVoi, colorVoi, shapeVoi;
			public ColumnStatistics vertexStats = null, sizeStats = null, colorStats = null, shapeStats = null;

			public GetVertexArgs(
				QueryManager qm,
				VertexMx vertexMx)
			{
				GetVertexArgs args = this;
				args.vertexMx = vertexMx;

				QueryColumn vertexQc = vertexMx.QueryColumn;
				if (vertexQc == null) return;
				args.vertexVoi = vertexQc.VoPosition;
				args.vertexStats = qm.DataTableManager.GetStats(vertexQc);
				if (args.vertexStats.DistinctValueList.Count == 0) args.vertexStats = null;

				QueryColumn sizeQc = vertexMx.Size.QueryColumn;
				if (sizeQc != null)
				{
					args.sizeVoi = sizeQc.VoPosition;
					args.sizeStats = qm.DataTableManager.GetStats(sizeQc);
					if (args.sizeStats.DistinctValueList.Count == 0) args.sizeStats = null;
				}

				QueryColumn colorQc = vertexMx.Color.QueryColumn;
				if (colorQc != null)
				{
					args.colorVoi = colorQc.VoPosition;
					args.colorStats = qm.DataTableManager.GetStats(colorQc);
					if (args.colorStats.DistinctValueList.Count == 0) args.colorStats = null;
				}

				QueryColumn shapeQc = vertexMx.Shape.QueryColumn;
				if (shapeQc != null)
				{
					args.shapeVoi = shapeQc.VoPosition;
					args.shapeStats = qm.DataTableManager.GetStats(shapeQc);
					if (args.shapeStats.DistinctValueList.Count == 0) args.shapeStats = null;
				}

				vertexMx.Shape.BuildValueToShapeMap(); // build map for matching shapes
			}
		}

/// <summary>
/// Get existing vertex with same name or create a new vertex
/// </summary>
/// <param name="vertexMx"></param>
/// <param name="ri"></param>
/// <param name="qc"></param>
/// <param name="voi"></param>
/// <returns></returns>

		IVertex GetVertex(
			GetVertexArgs args,
			int ri,
			IGraph g,
			out bool newVertex)
		{
			VertexTag tag;
			List<int> rowList = null;
			IVertex vertex;
			newVertex = false;

			IVertexCollection vertices = g.Vertices;
			NetworkProperties p = NetworkProperties;
			VertexMx vertexMx = args.vertexMx;
			QueryColumn qc = vertexMx.QueryColumn;

			object o = GetVo(ri, qc, args.vertexVoi);
			if (NullValue.IsNull(o)) return null; // if value is null then no vertex assigned

			MobiusDataType mdt = o as MobiusDataType;
			if (mdt == null) return null;

			string fTxt = GetFormattedText(qc, mdt);

			if (FindVertex(fTxt, out vertex))
			{
				tag = vertex.Tag as VertexTag;
				tag.RowList.Add(ri);
				return vertex;
			}

			vertex = vertices.Add();
			vertex.Name = fTxt;

			if (p.Vertex1.ShowLabels == ShowLabels.All)
				vertex.SetValue(ReservedMetadataKeys.PerVertexLabel, fTxt); // label same as name

			// Set value for size dimension

			if (vertexMx.Size.QueryColumn != null)
			{
				double size = GetVisualElementSize(VertexMx.MinRadius, VertexMx.MaxRadius, VertexMx.DefaultRadius, 
					vertexMx.Size, args.sizeStats, ri);
				vertex.SetValue(ReservedMetadataKeys.PerVertexRadius, (Single)size);
			}

			// Set value for color dimension

			if (vertexMx.Color.QueryColumn != null)
			{
				SD.Color color = GetVisualElementColor(vertexMx.Color, args.colorStats, ri, false);
				vertex.SetValue(ReservedMetadataKeys.PerColor, color);
			}

			// Set value for shape dimension

			if (vertexMx.Shape.QueryColumn != null)
			{
				int shape = GetVisualElementShape(vertexMx.Shape, args.shapeStats, ri);
				vertex.SetValue(ReservedMetadataKeys.PerVertexShape, shape);
			}

			tag = new VertexTag();
			tag.RowList.Add(ri);
			vertex.Tag = tag;

			newVertex = true;
			return vertex;
		}

/// <summary>
/// Find vertex by name (faster than builtin method?)
/// </summary>
/// <param name="vertexName"></param>
/// <param name="g"></param>
/// <param name="vertex"></param>
/// <returns></returns>

		bool FindVertex(
			string vertexName,
			out IVertex vertex)
		{
			if (VertexNameToVertexMap.ContainsKey(vertexName))
			{
				vertex = VertexNameToVertexMap[vertexName];
				return true;
			}

			else
			{
				vertex = null;
				return false;
			}
		}

/// <summary>
/// Find vertex by name (faster than builtin method?)
/// </summary>
/// <param name="edgeName"></param>
/// <param name="g"></param>
/// <param name="vertex"></param>
/// <returns></returns>

		bool FindEdge(
			string edgeName,
			out IEdge edge)
		{
			if (EdgeNameToEdgeMap.ContainsKey(edgeName))
			{
				edge = EdgeNameToEdgeMap[edgeName];
				return true;
			}

			else
			{
				edge = null;
				return false;
			}
		}

/// <summary>
/// Get the value of a column as a string
/// </summary>
/// <param name="ri"></param>
/// <param name="qc"></param>
/// <returns></returns>

		string GetVoString(
			QueryColumn qc,
			int ri)
		{
			int voi = qc.VoPosition;
			object o = GetVo(ri, qc, voi);
			if (NullValue.IsNull(o)) return null; // if value is null then no edge assigned

			MobiusDataType mdt = o as MobiusDataType;
			if (mdt == null) return null;

			string fTxt = GetFormattedText(qc, mdt);
			return fTxt;
		}

/// <summary>
/// Build the list of edges by joining edgeMap1 and edgeMap2 by the edge value
/// </summary>

		internal void BuildEdges ()
		{
			IVertex v1, v2;
			IEdge edge;
			AssocVertices vertex1List, vertex2List;
			EdgeTag edgeTag;
			Dictionary<string, EdgeRowLists> edgeValRowLists;
			EdgeRowLists rLists;

			NetworkProperties p = NetworkProperties;
			IGraph g = NetworkControl.Graph;

			bool selfJoin = false; // whether col is joining to self or not
			if (NetworkProperties.Vertex2.QueryColumn == null || 
			 NetworkProperties.Vertex1.QueryColumn == NetworkProperties.Vertex2.QueryColumn)
				selfJoin = true;

			IVertexCollection vertices = g.Vertices; // vertices are already built

			IEdgeCollection edges = g.Edges;
			edges.Clear();

// consider each edge name (e.g. compound)

			foreach (string edgeName in EdgeNameToVertex1NameMap.Keys)
			{ 
				vertex1List = EdgeNameToVertex1NameMap[edgeName]; // dict of vertices assoc with this edge
				if (p.Vertex2.QueryColumn != null) // join to other column
				{
					if (!EdgeNameToVertex2NameMap.ContainsKey(edgeName)) continue; // nothing with matching edge Name
					vertex2List = EdgeNameToVertex1NameMap[edgeName];
				}
				else vertex2List = vertex1List; // join to self

// Consider all of the first position vertexes (e.g. targets) connected to this edge (e.g. compoung)

				foreach (string v1Name in vertex1List.Dict.Keys)
				{ // consider each vertex (e.g. target) associated with the current edge name in the first map
					if (!FindVertex(v1Name, out v1)) throw new ArgumentException();

// Consider all of the second position vertexes (e.g. targets) connected to this edge (e.g. compoung)

					foreach (string v2Name in vertex2List.Dict.Keys)
					{ // consider other vertexes associated with the current edge name in the second map
						if (!FindVertex(v2Name, out v2)) throw new ArgumentException();

						if (selfJoin && String.Compare(v2.Name, v1.Name) <= 0) continue; // avoid doubling up on self joins

						string v1v2EdgeName = Lex.Lt(v1Name, v2Name) ? v1Name + "-" + v2Name : v2Name + "-" + v1Name;

						if (!FindEdge(v1v2EdgeName, out edge))
						{ // edge with this name doesn't exist yet in the graph, create it & add to the graph
							edge = new Edge(v1, v2, false);
							edge.Name = v1v2EdgeName;
							EdgeNameToEdgeMap[v1v2EdgeName] = edge;

							edges.Add(edge);
							edgeTag = new EdgeTag();
							edge.Tag = edgeTag;
						}

						else edgeTag = edge.Tag as EdgeTag;
						edgeValRowLists = edgeTag.RowLists;

						if (!edgeValRowLists.ContainsKey(edgeName))
						{
							rLists = new EdgeRowLists();
							edgeValRowLists[edgeName] = rLists;
						}
						else rLists = edgeValRowLists[edgeName];

						rLists.List1.AddRange(vertex1List.Dict[v1Name]); // add in the list of rows
						rLists.List2.AddRange(vertex2List.Dict[v2Name]);
					}
				}
			} // end for each edge name

			return;
		}

/// <summary>
/// Return true if merged edges should be drawn with a secondary vertex
/// </summary>

		bool DrawMergedEdgesWithSecondaryVertex
		{
			get
			{
				NetworkProperties p = NetworkProperties;
				if (p.Vertex2.QueryColumn != null) return true;
				else return false;
			}
		}

/// <summary>
/// Return true if label should be shown
/// </summary>
/// <param name="showLabel"></param>
/// <param name="element"></param>
/// <returns></returns>

		public static bool ShouldShowLabel(
			ShowLabels showLabel,
			IMetadataProvider element)
		{
			if (showLabel == ShowLabels.All) return true;
			else if (showLabel == ShowLabels.None) return false;
			else // just show if selected
				return NodeXLControlUtil.IsSelected(element);
		}

		/// <summary>
		/// Update vertex visibility and labels
		/// </summary>

		internal void UpdateVertexVisibilityAndLabels()
		{
			List<int> rowList;
			QueryColumn qc, qc1, qc2;
			int ti, ti1 = 0, ti2 = 0, ri, rli;
			VisibilityKeyValue visiblity;
			VertexMx vmx1, vmx2;
			VertexTag vertexTag;
			bool showLabel;
			ICollection<IVertex> selected = null;
			string label;

			NodeXLControl c = NetworkControl;
			if (c == null) return;

			NetworkProperties p = NetworkProperties;

			vmx1 = p.Vertex1;
			qc1 = vmx1.QueryColumn;
			ShowLabels showLabels1 = vmx1.ShowLabels;
			if (showLabels1 == ShowLabels.Selected)
				selected = c.SelectedVertices;
			if (qc1 != null) ti1 = qc1.QueryTable.Position;
			ti = ti1;

			//vmx2 = p.Vertex2;
			//qc2 = vmx2.QueryColumn;
			//ShowLabels showLabels2 = vmx2.ShowLabels;
			//if (qc2 != null) ti2 = qc2.QueryTable.Position;

			foreach (Vertex v in c.Graph.Vertices)
			{ 
				visiblity = VisibilityKeyValue.Visible; // assume vertex is visible

				vertexTag = v.Tag as VertexTag;
				rowList = vertexTag.RowList;

				if (vmx1.ShowLabels == ShowLabels.None)
					label = "";

				else if (vmx1.ShowLabels == ShowLabels.All)
					label = v.Name;

				else 
				{
					if (selected.Contains(v)) label = v.Name; // is this fast?
					else label = "";
				}

				//DebugLog.Message("Vertex Name: " + v.Name);

				if (RowListIsCompletelyFiltered(rowList, ti))
					visiblity = NetworkView.FilteredElementVisibility;

				v.SetValue(ReservedMetadataKeys.Visibility, visiblity);

				v.SetValue(ReservedMetadataKeys.PerVertexLabel, label);
			}

			return;
		}

/// <summary>
/// Args for GetEdge method
/// </summary>

		class GetEdgeArgs
		{
			public EdgeMx edgeMx;
			public int edgeVoi, widthVoi, colorVoi, shapeVoi;
			public ColumnStatistics edgeStats = null, widthStats = null, colorStats = null, shapeStats = null;
			public Dictionary<string, EdgeVals> edgeValPropsMap; 

			public GetEdgeArgs(
				QueryManager qm,
				EdgeMx edgeMx)
			{
				GetEdgeArgs args = this;
				args.edgeMx = edgeMx;

				QueryColumn edgeQc = edgeMx.QueryColumn;
				if (edgeQc == null) return;
				args.edgeVoi = edgeQc.VoPosition;

				args.edgeStats = qm.DataTableManager.GetStats(edgeQc);
				if (args.edgeStats.DistinctValueList.Count == 0) args.edgeStats = null;

				QueryColumn widthQc = edgeMx.Width.QueryColumn;
				if (widthQc != null)
				{
					args.widthVoi = widthQc.VoPosition;
					args.widthStats = qm.DataTableManager.GetStats(widthQc);
					if (args.widthStats.DistinctValueList.Count == 0) args.widthStats = null;
				}

				QueryColumn colorQc = edgeMx.Color.QueryColumn;
				if (colorQc != null)
				{
					args.colorVoi = colorQc.VoPosition;
					args.colorStats = qm.DataTableManager.GetStats(colorQc);
					if (args.colorStats.DistinctValueList.Count == 0) args.colorStats = null;
				}

				QueryColumn shapeQc = edgeMx.LineStyle.QueryColumn;
				if (shapeQc != null)
				{
					args.shapeVoi = shapeQc.VoPosition;
					args.shapeStats = qm.DataTableManager.GetStats(shapeQc);
					if (args.shapeStats.DistinctValueList.Count == 0) args.shapeStats = null;

				}

				edgeMx.LineStyle.BuildValueToShapeMap(); // build map for matching line styles
			}
		}

/// <summary>
/// Get edge property values & store in map
/// </summary>
/// <param name="args"></param>
/// <param name="ri"></param>
/// <param name="newEdge"></param>
/// <returns></returns>

		EdgeVals GetEdgeVals(
			GetEdgeArgs args,
			int ri,
			Dictionary<string, EdgeVals> edgeValsMap,
			out bool newEdge)
		{
			newEdge = false;

			NetworkProperties p = NetworkProperties;
			EdgeMx edgeMx = args.edgeMx;
			QueryColumn qc = edgeMx.QueryColumn;

			object o = GetVo(ri, qc, args.edgeVoi);
			if (NullValue.IsNull(o)) return null; // if value is null then no edge assigned

			MobiusDataType mdt = o as MobiusDataType;
			if (mdt == null) return null;

			string fTxt = GetFormattedText(qc, mdt);
			if (edgeValsMap.ContainsKey(fTxt)) return edgeValsMap[fTxt];

			EdgeVals edge = new EdgeVals();
			edge.Name = fTxt;

			// Set value for width dimension

			if (edgeMx.Width.QueryColumn != null)
			{
				double width = GetVisualElementSize(EdgeMx.MinWidth, EdgeMx.MaxWidth, EdgeMx.DefaultWidth, 
					edgeMx.Width, args.widthStats, ri);
				edge.Width = width;
			}

			// Set value for color dimension

			if (edgeMx.Color.QueryColumn != null)
			{
				SD.Color color = GetVisualElementColor(edgeMx.Color, args.colorStats, ri, false);
				edge.Color = color;
			}

			// Set value for line style dimension

			if (edgeMx.LineStyle.QueryColumn != null)
			{
				int lineStyle = GetVisualElementShape(edgeMx.LineStyle, args.shapeStats, ri);
				edge.LineStyle = lineStyle;
			}

			edge.RowIndex = ri;

			newEdge = true;
			return edge;
		}

/// <summary>
/// Set the values for an edge (not used)
/// </summary>
/// <param name="edge"></param>
/// <param name="edgeVals"></param>

		void SetEdgeVals (
			Edge edge,
			EdgeVals edgeVals)
		{
			EdgeTag edgeTag;
			List<int> rowList;

			NetworkProperties p = NetworkProperties;

			edge.Name = edgeVals.Name;

			if (ShouldShowLabel(p.Edge.ShowLabels, edge))
				edge.SetValue(ReservedMetadataKeys.PerEdgeLabel, edgeVals.Name); // label same as name

			if (edgeVals.Width > 0)
			  edge.SetValue(ReservedMetadataKeys.PerEdgeWidth, edgeVals.Width);

			if (edgeVals.Color != SD.Color.Empty)
			  edge.SetValue(ReservedMetadataKeys.PerColor, edgeVals.Color);

			if (edgeVals.LineStyle >= 0)
			  edge.SetValue(ReservedMetadataKeys.PerEdgeStyle, (EdgeStyle)edgeVals.LineStyle);

			if (edgeVals.RowIndex >= 0)
			{
				if (edge.Tag == null) edge.Tag = new EdgeTag();
				edgeTag = edge.Tag as EdgeTag;
				throw new NotImplementedException();
			}

			return;
		}

/// <summary>
/// Update edge visibility and labels
/// </summary>

		internal void UpdateEdgeVisibilityAndLabels()
		{
			Dictionary<string, EdgeRowLists> edgeValRowLists;
			VisibilityKeyValue visibility;
			ICollection<IEdge> selectedEdges = null;
			EdgeTag edgeTag;
			string edgeName;
			string label = "";

			NodeXLControl c = NetworkControl;
			if (c == null) return;

			NetworkProperties p = NetworkProperties;
			ShowLabels showLabels = p.Edge.ShowLabels;
			if (showLabels == ShowLabels.Selected)
				selectedEdges = c.SelectedEdges;

// If no edge column then blank labels (really need this?)

			if (p.Edge.QueryColumn == null) 
			{
				foreach (Edge e in c.Graph.Edges)
					e.SetValue(ReservedMetadataKeys.PerEdgeLabel, "");

				return;
			}

			QueryColumn eQc = p.Edge.QueryColumn;
			int ti = eQc.QueryTable.Position;

// If only one value on edge show that value, otherwise show count

			foreach (Edge e in c.Graph.Edges)
			{
				label = GetEdgeLabel(eQc, e);

				if (showLabels == ShowLabels.None)
				  label = "";

				else if (showLabels == ShowLabels.Selected)
				{
					if (!selectedEdges.Contains(e)) label = ""; // is this fast?
				}

				if (!Lex.IsNullOrEmpty(label)) 
					visibility = VisibilityKeyValue.Visible; 
				else visibility = NetworkView.FilteredElementVisibility;

				e.SetValue(ReservedMetadataKeys.Visibility, visibility);
				e.SetValue(ReservedMetadataKeys.PerEdgeLabel, label);
			}

			return;
		}

/// <summary>
/// Get the label for an edge
/// </summary>
/// <param name="eQc"></param>
/// <param name="e"></param>
/// <returns></returns>

		internal string GetEdgeLabel(
			QueryColumn eQc,
			IEdge e)
		{
			Dictionary<string, EdgeRowLists> edgeValRowLists;
			VisibilityKeyValue visibility;
			EdgeTag edgeTag;
			string edgeName;
			string label = "";

			edgeTag = e.Tag as EdgeTag;
			if (edgeTag == null) return "";
			edgeValRowLists = edgeTag.RowLists;
			if (edgeValRowLists == null) return "";

			NetworkProperties p = NetworkProperties;

			eQc = p.Edge.QueryColumn;
			if (eQc == null) return "";
			int eti = eQc.QueryTable.Position;

			QueryColumn vQc = p.Vertex1.QueryColumn;
			if (vQc == null) return "";
			int vti = vQc.QueryTable.Position;

			int subEdgeCount = 0;
			string lastPassedEdgeName = "";

			foreach (KeyValuePair<string, EdgeRowLists> kv in edgeValRowLists)
			{ // consider each subedge member making up the edge (e.g. compound)
				edgeName = kv.Key;
				EdgeRowLists rLists = kv.Value;
				rLists.List1CompletelyFiltered = RowListIsCompletelyFiltered(rLists.List1, eti);
				rLists.List2CompletelyFiltered = RowListIsCompletelyFiltered(rLists.List2, eti);
				if (rLists.List1CompletelyFiltered || rLists.List2CompletelyFiltered) continue;

				if (vti != eti) // if vertices values come from a different table must check that as well
				{
					rLists.List1CompletelyFiltered = RowListIsCompletelyFiltered(rLists.List1, vti);
					rLists.List2CompletelyFiltered = RowListIsCompletelyFiltered(rLists.List2, vti);
					if (rLists.List1CompletelyFiltered || rLists.List2CompletelyFiltered) continue;
				}

				subEdgeCount++;
				lastPassedEdgeName = edgeName;
			}

			if (subEdgeCount == 0) // no visible records hide edge
				label = "";

			else if (subEdgeCount == 1) // single subedge member passed, use its name
				label = lastPassedEdgeName;

			else label = "(" + subEdgeCount + ")"; // multiple subedge members passed, display the count

			return label;
		}

/// <summary>
/// Accumulate list of visible edge keys
/// </summary>
/// <param name="e"></param>
/// <param name="eoMap"></param>

		internal void AddUnfilteredEdgeKeysToUniqueEdgeObjectSet(
			QueryColumn eQc,
			IEdge e,
			HashSet<string> eoMap)
		{
			Dictionary<string, EdgeRowLists> edgeValRowLists;
			VisibilityKeyValue visibility;
			EdgeTag edgeTag;
			string edgeName;

			edgeTag = e.Tag as EdgeTag;
			if (edgeTag == null) return;
			edgeValRowLists = edgeTag.RowLists;
			if (edgeValRowLists == null) return;

			int ti = eQc.QueryTable.Position;

			foreach (KeyValuePair<string, EdgeRowLists> kv in edgeValRowLists)
			{ // consider each subedge member (e.g. compound) making up the edge 
				edgeName = kv.Key;
				EdgeRowLists rLists = kv.Value;
				if (!RowListIsCompletelyFiltered(rLists.List1, ti) &&
				 !RowListIsCompletelyFiltered(rLists.List2, ti))
				{
					eoMap.Add(edgeName);
				}
			}

			return;
		}

/// <summary>
/// Return true if the row list is completely filtered
/// </summary>
/// <param name="rowList"></param>
/// <returns></returns>

		internal bool RowListIsCompletelyFiltered(
			List<int> rowList,
			int ti)
		{
			DataRowAttributes dra = null;
			int rli, ri = -1, ri2 = -1, mainRi = -1;

			if (rowList == null) return true;

			bool checkFirstRowForKey = // if ti is pointing to root table for multitable query then
				(ti == 0 && Rf.Tables.Count > 1); // look at first row for key when checking filtering

			//DebugLog.Message("RowListIsCompletelyFiltered");
			for (rli = 0; rli < rowList.Count; rli++)
			{
				ri = rowList[rli];
				dra = Dtm.GetRowAttributes(ri);
				if (checkFirstRowForKey) // shift up to first row for key
				{
					ri2 = dra.FirstRowForKey;
					dra = Dtm.GetRowAttributes(ri2);
				}

				mainRi = dra.MainRowPos[ti]; // get the main row associated with the row associated with the table
				if (mainRi >= 0) return false; // if a main row is assigned then we know this subrow is not filtered

				//if (mainRi < 0) continue; // if no row assigned then assume filtered
				////DebugLog.Message("ri = " + ri + ", ri2 = " + ri2);
				//dr = Qm.DataTable.Rows[mainRi]; // get main row
				//dra = Dtm.GetRowAttributes(dr);
				//if (!dra.Filtered) // check filtering on main row
				//{
				//  //DebugLog.Message("return false");
				//  return false;
				//}
			}

			//DebugLog.Message("return true");
			return true;
		}

/// <summary>
/// Do simple check to see if complete rowlist is filtered
/// </summary>
/// <param name="rowList"></param>
/// <returns></returns>

		internal bool RowListIsCompletelyFiltered(
			List<int> rowList)
		{
			DataRowAttributes dra = null;
			int rli, ri = -1, ri2 = -1, mainRi = -1;

			if (rowList == null) return true;

			//DebugLog.Message("RowListIsCompletelyFiltered");
			for (rli = 0; rli < rowList.Count; rli++)
			{
				ri = rowList[rli];
				dra = Dtm.GetRowAttributes(ri);
				if (!dra.Filtered) return false;
			}

			return true;
		}

		/// <summary>
		/// Do simple check to see if complete rowlist is unfiltered
		/// </summary>
		/// <param name="rowList"></param>
		/// <returns></returns>

		internal bool RowListIsCompletelyUnfiltered(
			List<int> rowList)
		{
			DataRowAttributes dra = null;
			int rli, ri = -1, ri2 = -1, mainRi = -1;

			if (rowList == null) return true;

			//DebugLog.Message("RowListIsCompletelyFiltered");
			for (rli = 0; rli < rowList.Count; rli++)
			{
				ri = rowList[rli];
				dra = Dtm.GetRowAttributes(ri);
				if (dra.Filtered) return false;
			}

			return true;
		}

/// <summary>
/// Get the size of a vertex or edge
/// </summary>
/// <param name="sizeQc"></param>
/// <param name="sizeStats"></param>

		double GetVisualElementSize( 
			double minSize,
			double maxSize,
			double defaultSize,
			SizeDimension sizeDim,
			ColumnStatistics stats,
			int ri)
		{
			QueryColumn qc = null;
			MetaColumn mc = null;
			double relsize, size = 0;
			double sizeRange = maxSize - minSize;
			int voi = -1;

			qc = sizeDim.QueryColumn;
			if (qc != null)
			{
				mc = qc.MetaColumn;
				voi = qc.VoPosition;
			}

			if (qc == null || stats == null || stats.DistinctValueList.Count == 0) // fixed size
				return defaultSize;

			// Numeric column

			if (mc.IsNumeric && mc.DataType != MetaColumnType.CompoundId)
			{
				double minVal = (double)stats.MinValue.ToPrimitiveType();
				double maxVal = (double)stats.MaxValue.ToPrimitiveType();
				double valRange = maxVal - minVal;

				object o = GetVo(ri, qc, voi, false, false);
				if (!NullValue.IsNull(o))
				{
					MobiusDataType mdt = o as MobiusDataType;
					double d = (double)mdt.ToPrimitiveType();
					relsize = (d - minVal) / valRange;
					size = (double)(minSize + relsize * sizeRange); // size in axis coords
				}
			}

// Date column

			else if (mc.DataType == MetaColumnType.Date)
			{
				DateTime minVal = (DateTime)stats.MinValue.ToPrimitiveType();
				DateTime maxVal = (DateTime)stats.MaxValue.ToPrimitiveType();
				double valRange = (double)maxVal.Subtract(minVal).TotalDays;

				object o = GetVo(ri, qc, voi, false, false);

				if (NullValue.IsNull(o))
				{
					MobiusDataType mdt = o as MobiusDataType;
					DateTime d = (DateTime)mdt.ToPrimitiveType();
					relsize = (double)d.Subtract(minVal).TotalDays / valRange;
					size = (double)(minSize + relsize * sizeRange); // size in axis coords
				}
			}

// Categorical value column

			else // categorical string data
			{
				double minVal = 0;
				double maxVal = stats.DistinctValueList.Count - 1;
				double valRange = maxVal - minVal;

				object o = GetVo(ri, qc, voi, false, false);

				if (!NullValue.IsNull(o))
				{
					MobiusDataType mdt = o as MobiusDataType;
					string txt = GetFormattedText(qc, mdt);
					int ordinal = stats.DistinctValueDict[txt.ToUpper()].Ordinal;
					relsize = (ordinal - minVal) / valRange;
					size = (double)(minSize + relsize * sizeRange); // size in axis coords
				}
			}

			if (size < minSize) size = minSize;
			if (size > maxSize) size = maxSize;

			return size;
		}

/// <summary>
/// Get the shape of a vertex or the style of an edge
/// </summary>
/// <param name="shapeDim"></param>
/// <param name="stats"></param>
/// <param name="ri"></param>
/// <returns></returns>

		int GetVisualElementShape(
			ShapeDimension shapeDim,
			ColumnStatistics stats,
			int ri)
		{
			if (shapeDim.QueryColumn == null)
				return shapeDim.FixedShape;

			QueryColumn qc = shapeDim.QueryColumn;
			MetaColumn mc = qc.MetaColumn;
			int voi = qc.VoPosition;
			object o = GetVo(ri, qc, voi, false, false);
			MobiusDataType mdt = o as MobiusDataType; // (what if primitive type value?)
			if (mdt == null) return shapeDim.FixedShape;
			string fTxt = GetFormattedText(qc, mdt);
			int shape = shapeDim.AssignShapeToValue(fTxt);

			return shape;
		}

		/// <summary>
		/// SetupLayout
		/// </summary>
		/// <param name="c"></param>
		/// <param name="p"></param>

		internal void LayoutGraph()
		{
			NodeXLControl c = NetworkControl;
			if (c == null) return;

			NetworkProperties p = NetworkProperties;

			LayoutSettingsMx loMx = p.LayoutSettings;
			ILayout lo = AllLayouts.CreateLayout(loMx.LayoutType);

			lo.Margin = loMx.Margin; // copy Mobius settings to layout settings
			lo.LayoutStyle = loMx.LayoutStyle;
			lo.GroupRectanglePenWidth = loMx.GroupRectanglePenWidth;
			lo.IntergroupEdgeStyle = loMx.IntergroupEdgeStyle;
			lo.ImproveLayoutOfGroups = loMx.ImproveLayoutOfGroups;
			lo.MaximumVerticesPerBin = loMx.MaximumVerticesPerBin;
			lo.BinLength = loMx.BinLength;

			if (lo is FruchtermanReingoldLayout)
			{
				FruchtermanReingoldLayout frl = (FruchtermanReingoldLayout)lo;
				frl.C = loMx.FruchtermanReingoldC;
				frl.Iterations = loMx.FruchtermanReingoldIterations;
			}

			c.Layout = lo; // store layout in NodeXlControl

			ElementHost eh = NetworkPanel.ElementHost;
			Rectangle rect = new Rectangle(0, 0, eh.Width, eh.Height); // layout into full control
			LayoutContext layoutContext = new LayoutContext(rect);

			c.Layout.LayOutGraph(c.Graph, layoutContext);

			// May want to do asynch later...
			//
			//while (c.IsLayingOutGraph)
			//{
			//  Thread.Sleep(1);
			//  Application.DoEvents();
			//}

			return;
		}



/// <summary>
/// UpdateVertexSizeDimension
/// </summary>
/// <param name="vertex"></param>

		internal void UpdateVertexSizeDimension(VertexMx vertex)
		{
			ColumnStatistics stats = null;
			VertexTag vTag;
			double size;
			int voi = -1;

			VertexMx vx = vertex;
			NodeXLControl c = NetworkControl;
			if (c == null) return;

			QueryColumn qc = vx.Size.QueryColumn;
			if (qc != null)
			{
				voi = qc.VoPosition;
				stats = Qm.DataTableManager.GetStats(qc);
			}

// Fixed size for vertices

			if (qc == null || stats == null || stats.DistinctValueList.Count <= 1)
			{
				c.VertexRadius = VertexMx.DefaultRadius;
				bool checkedPerVertexValue = false;

				foreach (Vertex v in c.Graph.Vertices)
				{
					if (!checkedPerVertexValue)
					{ // check if per vertex values exist
						if (v.ContainsKey(ReservedMetadataKeys.PerVertexRadius))
							checkedPerVertexValue = true;
						else break;
					}

					v.SetValue(ReservedMetadataKeys.PerVertexRadius, (Single)VertexMx.DefaultRadius);
				}
			}

// Variable size vertices

			else // size varies by vertex 
			{
				foreach (Vertex v in c.Graph.Vertices)
				{
					vTag = v.Tag as VertexTag;
					if (vTag == null) continue;
					List<int> rowList = vTag.RowList;
					if (rowList == null || rowList.Count == 0) continue;
					int ri = rowList[0]; // todo: properly summarize

					size = GetVisualElementSize(VertexMx.MinRadius, VertexMx.MaxRadius, VertexMx.DefaultRadius,
						vx.Size, stats, ri);
					v.SetValue(ReservedMetadataKeys.PerVertexRadius, (Single)size);
					//if (size < .2 || size > .4) break;
				}
			}

			DrawGraph(); // redraw without laying out
			return;
		}

/// <summary>
/// Update the vertex color dimension
/// </summary>
/// <param name="vertex"></param>

		internal void UpdateVertexColorDimension(VertexMx vertex)
		{
			ColumnStatistics stats = null;
			VertexTag vTag;
			SD.Color color;
			int voi = -1;

			VertexMx vmx = vertex;
			NodeXLControl c = NetworkControl;
			if (c == null) return;

			QueryColumn qc = vmx.Color.QueryColumn;
			if (qc != null)
			{
				voi = qc.VoPosition;
				stats = Qm.DataTableManager.GetStats(qc);
			}

// Fixed color

			if (qc == null || stats == null || stats.DistinctValueList.Count <= 1)
			{
				c.VertexColor = GraphicsHelper.ConvertDrawingColorToMediaColor(vmx.Color.FixedColor);
				bool checkedPerVertexValue = false;

				foreach (Vertex v in c.Graph.Vertices)
				{
					if (!checkedPerVertexValue)
					{ // check if per vertex values exist
						if (v.ContainsKey(ReservedMetadataKeys.PerColor))
							checkedPerVertexValue = true;
						else break;
					}

					v.SetValue(ReservedMetadataKeys.PerColor, vmx.Color.FixedColor);
				}
			}

// Variable color vertices

			else // color varies by vertex 
			{
				foreach (Vertex v in c.Graph.Vertices)
				{
					vTag = v.Tag as VertexTag;
					if (vTag == null) continue;
					List<int> rowList = vTag.RowList;
					if (rowList == null || rowList.Count == 0) continue;
					int ri = rowList[0]; // todo: properly summarize

					color = GetVisualElementColor(vmx.Color, stats, ri, true); // get the color forcing a recalculation
					v.SetValue(ReservedMetadataKeys.PerColor, color);
				}
			}

			DrawGraph(); // redraw without laying out
			return;
		}

		/// <summary>
		/// Get the color of a vertex or edge
		/// </summary>
		/// <param name="colorDim"></param>
		/// <param name="stats"></param>
		/// <param name="ri"></param>
		/// <returns></returns>

		SD.Color GetVisualElementColor(
			ColorDimension colorDim,
			ColumnStatistics stats,
			int ri,
			bool forceRecalculation)
		{
			QueryColumn qc = null;
			MetaColumn mc = null;
			int voi = -1;
			SD.Color color = SD.Color.Empty;

			qc = colorDim.QueryColumn;
			if (qc != null)
			{
				mc = qc.MetaColumn;
				voi = qc.VoPosition;
			}

			if (qc == null || stats == null || stats.DistinctValueList.Count == 0) // fixed color
				return colorDim.FixedColor;

			// Numeric column

			object o = GetVo(ri, qc, voi, false, false);

			if (o == null || o is DBNull)
				return CondFormatMatcher.DefaultMissingValueColor;

			MobiusDataType mdt = o as MobiusDataType;
			if (mdt == null || mdt.BackColor == SD.Color.Empty || forceRecalculation)
			{ // if not back color then get cell style
				//CondFormatRules rules = qc.CondFormat.Rules; // debug;
				CellStyleMx style = // get the back color
					ResultsFormatter.GetCondFormatCellStyle(qc, stats, o, Qm.ResultsFormat);

				if (mdt == null)
				{ // create Mobius type to store formatting info in
					mdt = MobiusDataType.ConvertToMobiusDataType(qc.MetaColumn.DataType, o);
					DataRowMx drMx = Dt.Rows[ri];
					drMx[voi] = mdt;
				}

				if (mdt != null && style != null)
					mdt.BackColor = style.BackColor;
			}

			if (mdt != null)
			{
				color = mdt.BackColor;
			}

			else color = CondFormatMatcher.DefaultMissingValueColor;

			return color;
		}

/// <summary>
/// Update the vertex shape dimension
/// </summary>
/// <param name="vertex"></param>

		internal void UpdateVertexShapeDimension(VertexMx vertex)
		{
			ColumnStatistics stats = null;
			VertexTag vTag;
			VertexShape shape;
			int voi = -1;

			VertexMx vmx = vertex;
			NodeXLControl c = NetworkControl;
			if (c == null) return;

			QueryColumn qc = vmx.Shape.QueryColumn;
			if (qc != null)
			{
				voi = qc.VoPosition;
				stats = Qm.DataTableManager.GetStats(qc);
			}

			// Fixed shape

			if (qc == null || stats == null || stats.DistinctValueList.Count <= 1)
			{
				c.VertexShape = (VertexShape)vmx.Shape.FixedShape;
				bool checkedPerVertexValue = false;

				foreach (Vertex v in c.Graph.Vertices)
				{
					if (!checkedPerVertexValue)
					{ // check if per vertex values exist
						if (v.ContainsKey(ReservedMetadataKeys.PerVertexShape))
							checkedPerVertexValue = true;
						else break;
					}

					v.SetValue(ReservedMetadataKeys.PerVertexShape, vmx.Shape.FixedShape);
				}
			}

// Variable shape vertices

			else // shape varies by vertex 
			{
				vmx.Shape.BuildValueToShapeMap(); // build map used for mapping

				foreach (Vertex v in c.Graph.Vertices)
				{
					vTag = v.Tag as VertexTag;
					List<int> rowList = vTag.RowList;
					if (rowList == null || rowList.Count == 0) continue;
					int ri = rowList[0]; // todo: properly summarize

					shape = (VertexShape)GetVisualElementShape(vmx.Shape, stats, ri);
					v.SetValue(ReservedMetadataKeys.PerVertexShape, shape);
				}
			}

			DrawGraph(); // redraw without laying out
			return;
		}

/// <summary>
/// Update the main column dimension for the edge
/// </summary>

		internal void UpdateEdgeColumnDimension()
		{
			return; // todo...
		}

/// <summary>
/// Update the edge size dimension
/// </summary>

		internal void UpdateEdgeSizeDimension(EdgeMx edge)
		{
			ColumnStatistics stats = null;
			double size;
			List<int> edgeList = null;
			int ri = -1, voi = -1;

			EdgeMx emx = edge;
			NodeXLControl c = NetworkControl;
			if (c == null) return;

			QueryColumn qc = emx.Width.QueryColumn;
			if (qc != null)
			{
				voi = qc.VoPosition;
				stats = Qm.DataTableManager.GetStats(qc);
			}

			// Fixed size for edges

			if (qc == null || stats == null || stats.DistinctValueList.Count <= 1)
			{
				c.EdgeWidth = EdgeMx.DefaultWidth;
				bool checkedPerEdgeValue = false;

				foreach (Edge e in c.Graph.Edges)
				{
					if (!checkedPerEdgeValue)
					{ // check if per Edge values exist
						if (e.ContainsKey(ReservedMetadataKeys.PerEdgeWidth))
							checkedPerEdgeValue = true;
						else break;
					}

					e.SetValue(ReservedMetadataKeys.PerEdgeWidth, EdgeMx.DefaultWidth);
				}
			}

// Variable size edges

			else // size varies by Edge 
			{
				foreach (Edge e in c.Graph.Edges)
				{
					ri = GetSampleEdgeRow(e);
					if (ri < 0) continue;

					size = GetVisualElementSize(EdgeMx.MinWidth, EdgeMx.MaxWidth, EdgeMx.DefaultWidth,
						emx.Width, stats, ri);
					e.SetValue(ReservedMetadataKeys.PerEdgeWidth, (Single)size);
					//if (size < .2 || size > .4) break;
				}
			}

			DrawGraph(); // redraw without laying out
			return;
		}

/// <summary>
/// GetSampleEdgeRow - fix later
/// </summary>
/// <param name="tag"></param>
/// <returns></returns>

		int GetSampleEdgeRow(Edge edge)
		{
			EdgeTag tag = edge.Tag as EdgeTag;
			if (tag == null) return -1;
			Dictionary<string, EdgeRowLists> erlDict = tag.RowLists;
			if (erlDict == null || erlDict.Count == 0) return -1;
			foreach (EdgeRowLists erls in erlDict.Values)
			{
				if (erls.List1.Count > 0) return erls.List1[0];
				else if (erls.List2.Count > 0) return erls.List2[0];
				else return -1;
			}

			return -1;
		}

		/// <summary>
		/// Update the edge color dimension
		/// </summary>
		/// <param name="edge"></param>

		internal void UpdateEdgeColorDimension(EdgeMx edge)
		{
			ColumnStatistics stats = null;
			EdgeTag eTag;
			SD.Color color;
			int voi = -1;

			EdgeMx vmx = edge;
			NodeXLControl c = NetworkControl;
			if (c == null) return;

			QueryColumn qc = vmx.Color.QueryColumn;
			if (qc != null)
			{
				voi = qc.VoPosition;
				stats = Qm.DataTableManager.GetStats(qc);
			}

			// Fixed color

			if (qc == null || stats == null || stats.DistinctValueList.Count <= 1)
			{
				c.EdgeColor = GraphicsHelper.ConvertDrawingColorToMediaColor(vmx.Color.FixedColor);
				bool checkedPerEdgeValue = false;

				foreach (Edge e in c.Graph.Edges)
				{
					if (!checkedPerEdgeValue)
					{ // check if per Edge values exist
						if (e.ContainsKey(ReservedMetadataKeys.PerColor))
							checkedPerEdgeValue = true;
						else break;
					}

					e.SetValue(ReservedMetadataKeys.PerColor, vmx.Color.FixedColor);
				}
			}

// Variable color edges

			else // color varies by edge 
			{
				foreach (Edge e in c.Graph.Edges)
				{
					int ri = GetSampleEdgeRow(e);
					if (ri < 0) continue;

					color = GetVisualElementColor(vmx.Color, stats, ri, true); // get the color forcing a recalculation
					e.SetValue(ReservedMetadataKeys.PerColor, color);
				}
			}

			DrawGraph(); // redraw without laying out
			return;
		}

		/// <summary>
		/// Update the edge shape dimension
		/// </summary>
		/// <param name="edge"></param>

		internal void UpdateEdgeShapeDimension(EdgeMx edge)
		{
			return; // not currently supported
		}

/// <summary>
/// Draw the graph
/// </summary>

		internal void DrawGraph()
		{
			NetworkControl.DrawGraph(false); // draw without laying out
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>

		internal string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			XmlTextWriter tw = mstw.Writer;
			tw.Formatting = Formatting.Indented;
			Serialize(tw);
			string xml = mstw.GetXmlAndClose();
			return xml;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public override void Serialize(XmlTextWriter tw)
		{
			BeginSerialization(tw);
			EndSerialization(tw);
			return;
		}

		/// <summary>
		/// BeginSerialization
		/// </summary>
		/// <param name="tw"></param>

		public override void BeginSerialization(XmlTextWriter tw)
		{
			base.BeginSerialization(tw);
			SerializeNetworkProperties(tw);
			return;
		}

		/// <summary>
		/// Serialize network Properties
		/// </summary>
		/// <param name="tw"></param>

		internal void SerializeNetworkProperties(XmlTextWriter tw)
		{
			if (this.NetworkProperties != null)
				NetworkProperties.Serialize(tw);
			return;
		}

		/// <summary>
		/// Update the view to reflect changes in filtering
		/// </summary>

		public override void UpdateFilteredView()
		{

			UpdateVertexVisibilityAndLabels();
			UpdateEdgeVisibilityAndLabels();
			DrawGraph();

			return;
		}

		/// <summary>
		/// Refresh the view
		/// </summary>

		public override void Refresh()
		{
			DateTime t0 = DateTime.Now;

			if (NetworkPanel.ElementHost != null)
				NetworkPanel.ElementHost.Refresh();

			double tDelta = TimeOfDay.Delta(t0);
			return;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>

		public new void FreeResources()
		{
			RenderingControl = null;
			PageControl = null;
			return;
		}
	}

	/// <summary>
	/// Vertex Tag
	/// </summary>

	internal class VertexTag
	{
		public List<int> RowList = new List<int>(); // indexes of rows associated with vertex
	}

/// <summary>
/// 
/// </summary>

	internal class EdgeTag
	{
		public Dictionary<string, EdgeRowLists> RowLists = // keyed by edge value with list of rows to associated vertices
			new Dictionary<string, EdgeRowLists>();
	}

/// <summary>
/// A Vertex that is associated with a particular edge value and the list of rows making up the association
/// </summary>

	internal class AssocVertices
	{
		internal Dictionary <
			string, // the vertex name
			List<int>> // list of rows linking edge name & this vertex name
		 Dict = new Dictionary<string, List<int>>();
	}

/// <summary>
/// The list of data rows associated with each of the vertices for the node for an edge
/// </summary>

		internal class EdgeRowLists
		{
			internal List<int> List1 = new List<int>(); // list of rows going to vertex 1
			internal bool List1CompletelyFiltered = false; 

			internal List<int> List2 = new List<int>(); // list of rows going to vertex 2
			internal bool List2CompletelyFiltered = false;
		}

		/// <summary>
		/// Intermediate edge properties
		/// </summary>

		internal class EdgeVals
		{
			internal string Name = null;
			internal double Width = -1;
			internal SD.Color Color = SD.Color.Empty;
			internal int LineStyle = -1;
			internal string Tooltip = null;
			internal int RowIndex = -1; // row index in data source
		}

}
