using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.XtraCharts.Native;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// ChartView for Bubble Chart
	/// </summary>

	public partial class ChartViewBubble : ChartViewMgr
	{

		object CurrentTooltipObject; // object currently being shown in tooltip (series point or axis label)

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ChartViewBubble()
		{
			ViewType = ViewTypeMx.ScatterPlot;
			Title = "Bubble Chart";
		}

/// <summary>
/// Get the view type image index for the view
/// </summary>
/// <returns></returns>

		public override ViewTypeImageEnum ViewTypeImageIndex
		{
			get
			{
				if (BackgroundImageFile != null && Lex.Contains(BackgroundImageFile, "Dendograms"))
					return ViewTypeImageEnum.TargetMap;

				else return ViewTypeImageEnum.BubbleChart;
			}
		}

		/// <summary>
		/// Return true if the view content has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				if (XAxisMx != null && XAxisMx.QueryColumn != null)
					return true;
				else return false;
			}
		}

		/// <summary>
		/// Do initial creation of bubble chart
		/// </summary>
		/// <param name="qm"></param>
		/// <returns></returns>

		public override void SetDefaultPropertyValues()
		{
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			int qti, rti, rfi;
			int art = 100, art2; // assigned result type

			if (Qm == null || Qm.Query == null || Qm.Query.Tables.Count == 0) return;

			// Build the chart associated with the data table

			//ViewType = ViewTypeMx.BubbleChart; // (already set, may be heatmap or other view
			//cex.MarkerColor.BorderColor = Color.Empty; // no border color

			foreach (ResultsTable rt in Qm.ResultsFormat.Tables)
			{ // Do initial assignment of QueryColumns to chart
				qt = rt.QueryTable;
				mt = qt.MetaTable;

				if (XAxisMx.QueryColumn == null)
				{ // assign key to X axis
					mc = mt.KeyMetaColumn;
					qc = qt.GetQueryColumnByName(mc.Name);
					XAxisMx.QueryColumn = qc;
				}

				foreach (ResultsField rfld in rt.Fields)
				{ // make a reasonable selection for the Y axis by assigning a priority to each column type
					qc = rfld.QueryColumn;
					mc = qc.MetaColumn;

					if (mc.IsKey) art2 = 100; // don't use key
					else if (mc.IsGraphical) art2 = 100; // don't use structure or chart initially
					else if (mc.PrimaryResult) art2 = 1;
					else if (mc.SecondaryResult) art2 = 2;
					else if (mc.IsNumeric) art2 = 3;
					else art2 = 4;

					if (art2 < art)
					{
						YAxisMx.QueryColumn = qc;
						art = art2;
					}

					if (MarkerColor.QueryColumn == null && qc.CondFormat != null && qc.CondFormat.Rules.Count > 0)
						MarkerColor.QueryColumn = qc; // assign first col with cond formatting to marker color

					if (MarkerLabel.QueryColumn == null && mc.IsKey)
					{ // assign key field & structure to label column
						MarkerLabel.QueryColumn = qc;
						MarkerLabel.IncludeStructure = true;
						MarkerLabel.VisibilityMode = LabelVisibilityModeEnum.RolloverRow;
					}
				}
			}

			return;
		}

		/// <summary>
		/// Build any subquery results needed for the current view type
		/// </summary>
		/// <returns></returns>

		public override IQueryManager BuildSubqueryResults()
		{
			return null;
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			ChartPage = new ChartPageControl();
			ChartControl.ChartView = this; // link the chart control  to us
			ChartControl.Visible = false; // chart control is initially hidden
			ChartPagePanel.Dock = DockStyle.Fill; // dock full chart page

			ChartControl cc = ChartControl;

			cc.CustomPaint += // custom painting of chart diagram
				new DevExpress.XtraCharts.CustomPaintEventHandler(this.ChartControl_CustomPaint);

			cc.CustomDrawSeriesPoint += // setup callback for defining custom point attributes (marker shape, color & label) (also legend items)
				new DevExpress.XtraCharts.CustomDrawSeriesPointEventHandler(ChartControlMx_CustomDrawSeriesPoint);

			cc.CustomDrawAxisLabel += // setup callback for custom drawing of axis labels
				new DevExpress.XtraCharts.CustomDrawAxisLabelEventHandler(ChartControlMx_CustomDrawAxisLabel);

			cc.Scroll += // setup event handler for scroll event
				new DevExpress.XtraCharts.ChartScrollEventHandler(this.ChartControlMx_Scroll);

			cc.Zoom += // setup event handler for zoom event
				new DevExpress.XtraCharts.ChartZoomEventHandler(this.ChartControlMx_Zoom);

			cc.ObjectHotTracked += // setup event handler for chart element tracking
				new DevExpress.XtraCharts.HotTrackEventHandler(this.ChartControlMx_ObjectHotTracked);

			cc.ObjectSelected += // setup event handler for chart element selection
				new DevExpress.XtraCharts.HotTrackEventHandler(this.ChartControlMx_ObjectSelected);

			cc.MouseDown +=
				new System.Windows.Forms.MouseEventHandler(this.ChartControlMx_MouseDown);

			cc.MouseMove += // event handler for mouse movement over the chart
				new System.Windows.Forms.MouseEventHandler(this.ChartControlMx_MouseMove);

			cc.MouseUp +=
				new System.Windows.Forms.MouseEventHandler(this.ChartControlMx_MouseUp);

			cc.MouseClick += // handle mouse click events
				new System.Windows.Forms.MouseEventHandler(this.ChartControlMx_MouseClick);

			cc.MouseEnter += // mouse is entering chart control 
				new System.EventHandler(this.ChartControl_MouseEnter);

			cc.MouseLeave += // mouse is leaving the chart control
				new System.EventHandler(this.ChartControlMx_MouseLeave);

			cc.KeyDown +=
				new System.Windows.Forms.KeyEventHandler(this.ChartControl_KeyDown);

			cc.KeyUp +=
				new System.Windows.Forms.KeyEventHandler(this.ChartControl_KeyUp);

			RenderingControl = ChartPagePanel;
			return RenderingControl;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel panel)
		{
			panel.Controls.Clear(); // remove anything in there
			panel.Controls.Add(ChartPagePanel); // add it to the display panel

			InsertToolsIntoDisplayPanel();

			ChartPage.EditQueryBut.Enabled = // enable Edit Query button if QRC is in a QueriesControl that can do the editing
				ViewManager.IsControlContainedInQueriesControl(Qrc);

			ChartPagePanel.ChartPanel.UseMouseForScrolling = false;

			return;
		}

/// <summary>
/// Insert tools into display panel
/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			Qrc.SetToolBarTools(ChartPage.ToolPanel, GetZoomPct()); // show the proper tools
			return;
		}

		/// <summary>
		/// Show the properties for the view
		/// </summary>
		/// <returns></returns>

		public override DialogResult ShowPropertiesDialog()
		{
			return ChartPropertiesDialog.ShowDialog(this);
		}

		/// <summary>
		/// Get context menu strip for view to be shown on a right mouse click
		/// </summary>
		/// <returns></returns>

		public override ContextMenuStrip GetContextMenuStrip()
		{
			if (ChartPage == null || ChartPagePanel == null || ChartPagePanel.ChartPanel == null) return null;
			else return ChartPagePanel.ChartPanel.ChartPropertiesContextMenu;
		}

		/// <summary>
		/// Render the view by configuring the control for the current view settings
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			DateTime t0 = DateTime.Now;

			if (!IsDefined) return; // just return if not defined

			AssureQueryManagerIsDefined(BaseQuery);

			DialogResult dr = Qm.DataTableManager.CompleteRetrieval(); // be sure we have all data
			if (dr == DialogResult.Cancel) return;

			ChartControl.Series.BeginUpdate();
			try
			{
				BuildBubbleChartDataAndControl(); // setup the axes and series data
			}

			finally // be sure update ended
			{
				ChartControl.Series.EndUpdate();
			}

			ChartControl.Visible = true; // be sure the control is visible
			ConfigureCount++;

			ChartPagePanel.ChartPanel.UseMouseForScrolling = false;

			double ms = TimeOfDay.Delta(t0);
			return;
		}

		/// <summary>
		/// Build the DataTable for the chart and configure the chart control according to the view definition 
		/// </summary>

		internal void BuildBubbleChartDataAndControl()
		{
			QueryColumn qc;
			MetaColumn mc;

			ChartControl cc = ChartControl;
			cc.Series.Clear(); // reset the chart
			cc.Titles.Clear();
			cc.Annotations.Clear();
			cc.DataSource = null;

			DateTime t0 = DateTime.Now;

			NativeMarkerRendering = CustomMarkerRendering = false;

			Series = new Series("BubbleSeries", DevExpress.XtraCharts.ViewType.Bubble);
			cc.Series.Add(Series); // add series to chart control which also creates the diagram and axis objects
			BubbleView.ColorEach = true; // allow individual point to appear in legend

			XYDiagram.Rotated = RotateAxes; // set diagram rotation

			Series.LabelsVisibility = DefaultBooleanMx.Convert(MarkerLabel.QueryColumn != null);
			Series.ShowInLegend = true; // so events are fired for series points
			//Series.LegendPointOptions.PointView = PointView.ArgumentAndValues; // cause point members to be displayed
			double t1 = TimeOfDay.Delta(ref t0);

			ValidateQueryColumns(); // clear any non-valid query column values
			double t2 = TimeOfDay.Delta(ref t0);

			BuildTitles();
			double t3 = TimeOfDay.Delta(ref t0);

			BuildSeriesDataPoints(true);
			double t7 = TimeOfDay.Delta(ref t0);

			SetupBackgroundImage();
			if (BackgroundImage != null) // if background image must do custom rendering of it and the markers
				CustomMarkerRendering = true;
			double t8 = TimeOfDay.Delta(ref t0);

			SetupLegend();
			double t9 = TimeOfDay.Delta(ref t0);

			if (SeriesPoints.Length < LargeDatasetSize && !ForceCustomMarkerRendering && !CustomMarkerRendering)
			{
				NativeMarkerRendering = true;
				Series.Points.AddRange(SeriesPoints); // add the points to the control
			}

			else // custom rendering
			{
				int ptCnt = 100; // enough points for a large legend
				//if (LegendItems != null && LegendItems.Count > 1) // also, at least as many points as there are legend items to get them drawn in CustomDrawSeriesPoint
				//  ptCnt = LegendItems.Count;

				SeriesPoint[] sp2 = new SeriesPoint[ptCnt];
				for (int pi = 0; pi < ptCnt; pi++)
					sp2[pi] = new SeriesPoint(0, 0, 1);

				Series.Points.AddRange(sp2);
				CustomMarkerRendering = true;
			}

			double t10 = TimeOfDay.Delta(ref t0);

			return;
		} // BuildBubbleChartDataAndControl


		/// <summary>
		/// Build the set of datarows for the series
		/// </summary>

		internal void BuildSeriesDataPoints(
			bool buildAxes)
		{
			ScaleType newScaleType;

			DateTime t0 = DateTime.Now;

			int size = Qm.DataTable.Rows.Count;
			if (SeriesPoints == null || SeriesPoints.Length != size)
				SeriesPoints = new SeriesPoint[size];

			// Setup X axis

			if (XAxisMx.QueryColumn != null)
			{ // build X series applying any enabled filters
				newScaleType = GetScaleType(XAxisMx.QueryColumn.MetaColumn);
				if (newScaleType != Series.ArgumentScaleType)
					Series.ArgumentScaleType = newScaleType;
				BuildDataForAxisDimension(XAxisMx, true, false);
			}

			if (buildAxes) BuildAxis(Series, XAxisMx, AxisX); // build X axis definition
			double t1 = TimeOfDay.Delta(ref t0);

			// Setup Y axis

			if (YAxisMx.QueryColumn != null)
			{ // build Y series applying any enabled filters
				newScaleType = GetScaleType(YAxisMx.QueryColumn.MetaColumn);
				if (newScaleType != Series.ValueScaleType)
					Series.ValueScaleType = newScaleType;
				BuildDataForAxisDimension(YAxisMx, false, Qm.DataTableManager.FiltersEnabled);
			}
			if (buildAxes) BuildAxis(Series, YAxisMx, AxisY); // build Y axis definition
			double t2 = TimeOfDay.Delta(ref t0);

			for (int pi = 0; pi < SeriesPoints.Length; pi++) // be sure all points are defined
			{
				if (SeriesPoints[pi] == null) // insert empty point
				{
					SeriesPoints[pi] = new SeriesPoint(double.NaN, double.NaN, double.NaN);
					SeriesPoints[pi].IsEmpty = true;
				}
			}

			// Setup other dimensions and chart properties

			BuildMarkerSizeDimension();
			double t3 = TimeOfDay.Delta(ref t0);

			BuildMarkerShapeDimension();
			double t4 = TimeOfDay.Delta(ref t0);

			BuildMarkerColorDimension();
			double t5 = TimeOfDay.Delta(ref t0);

			BuildMarkerLabelDimension();
			double t6 = TimeOfDay.Delta(ref t0);

			return;
		}

		/// <summary>
		/// Thin out the set of points (POC)
		/// </summary>

		void ThinPoints()
		{
			if (SeriesPoints.Length > LargeDatasetSize)
			{ // trim down the set of points
				SeriesPoint[] pts2 = new SeriesPoint[LargeDatasetSize];
				double stride = SeriesPoints.Length / LargeDatasetSize;
				double pir = 0;
				for (int pi2 = 0; pi2 < pts2.Length; pi2++)
				{
					int pi = (int)pir;
					if (pir >= SeriesPoints.Length) break;
					pts2[pi2] = SeriesPoints[pi];
					pir += stride;
				}
				SeriesPoints = pts2;
			}
		}

		/// <summary>
		/// Build the axis
		/// </summary>
		/// <param name="series"></param>
		/// <param name="axisMx"></param>
		/// <param name="axis"></param>

		internal void BuildAxis(
			Series series,
			AxisMx axisMx,
			Axis2D axis)
		{
			Axis2D a = axis;
			AxisMx aMx = axisMx;

			QueryColumn qc = aMx.QueryColumn;
			if (qc == null) return;
			MetaColumn mc = qc.MetaColumn;

			// Setup the axis visible and scrolling ranges

			SetupAxisRange(aMx, axis);

			// Setup scroll bar visibility (appears that it must be done after setting axis range)

			bool enable = aMx.ShowZoomSlider;

			if (IsAxisX(a))
			{
				XYDiagram.EnableAxisXScrolling = XYDiagram.EnableAxisXZooming = enable; // enable for diagram
				XYDiagram.DefaultPane.EnableAxisXScrolling = XYDiagram.DefaultPane.EnableAxisXZooming = enable ? DefaultBoolean.True : DefaultBoolean.False; //  enable for pane
			}

			else if (IsAxisY(a))
			{
				XYDiagram.EnableAxisYScrolling = XYDiagram.EnableAxisYZooming = enable; // enable for diagram
				XYDiagram.DefaultPane.EnableAxisYScrolling = XYDiagram.DefaultPane.EnableAxisYZooming = enable ? DefaultBoolean.True : DefaultBoolean.False; //  enable for pane
			}

			else throw new ArgumentException();

			// Grid lines

			a.GridLines.Visible = aMx.ShowGridLines;
			a.NumericScaleOptions.AutoGrid = true;
			a.DateTimeScaleOptions.AutoGrid = true;

			// Grid strips

			a.Interlaced = aMx.ShowGridStrips;

			// Log or linear scale

			if (mc.IsNumeric && mc.DataType != MetaColumnType.CompoundId)
			{ // Numeric type
				a.Logarithmic = aMx.LogScale;
				a.LogarithmicBase = 10;
			}

			else a.Logarithmic = false;

			// Normal or reverse scale

			if (a.GetType().IsSubclassOf(typeof(Axis))) // can only reverse if XYDiagram not SPDiagram
				((Axis)a).Reverse = aMx.ReverseScale;

			// Labels

			if (!aMx.ShowLabels)
			{
				a.Label.Visible = a.Tickmarks.Visible = a.Tickmarks.MinorVisible = false;
			}

			else if (mc.IsContinuous)
			{
				a.Label.Visible = a.Tickmarks.Visible = a.Tickmarks.MinorVisible = true;
			}

			else // categorical
			{
				a.Label.Visible = a.Tickmarks.Visible = true;
				a.Tickmarks.MinorVisible = false; // no minor tick marks

				//if (mc.DataType == MetaColumnType.Structure) // Note: currently uses Smiles instead
				//  BuildStructureAxisLabels(series, axisMx, axis);

			}

			// Label orientation & overlap management

			a.Label.Angle = aMx.LabelAngle;
			a.Label.EnableAntialiasing = DefaultBooleanMx.Convert(aMx.LabelAngle != 0); // antialias if not horiz
			a.Label.Staggered = aMx.LabelsStaggered;
            //a.Label.ResolveOverlappingMode = aMx.LabelResolveOverlappingMode; // turn on with 2011v2

            // Title

            // DevExpress 15.2.7 Upgrade

            if (ShowAxesTitles && ViewType != ViewTypeMx.RadarPlot)
			{
				a.Title.Text = aMx.Title; // +" " + Convert.ToChar(0x25BC); (down arrow char to indicate that you can click on label to select another column)
				a.Title.Visibility = DefaultBoolean.True;
			}

			else a.Title.Visibility = DefaultBoolean.False;

            return;
		}

		/// <summary>
		/// Build structure labels
		/// Note: This is incomplete dev code. Further work would be necessary to complete it.
		/// Display of Smiles with structure popups is currently being used instead.
		/// </summary>
		/// <param name="series"></param>
		/// <param name="axisMx"></param>
		/// <param name="axis"></param>

		void BuildStructureAxisLabels(
			Series series,
			AxisMx axisMx,
			Axis axis)
		{
			int height = 200;
			int width = 200;

			QueryColumn qc = axisMx.QueryColumn;
			if (qc == null) return;

			ColumnStatistics stats = GetStats(qc);
			if (stats == null || stats.DistinctValueList.Count == 0)
				return;

			XYDiagramDefaultPane pane = XYDiagram.DefaultPane;

			for (int si = 0; si < stats.DistinctValueList.Count; si++)
			{
				ChemicalStructureMx cs = stats.DistinctValueList[si] as ChemicalStructureMx;
				if (NullValue.IsNull(cs)) continue;

				int miWidth = ChemicalStructureMx.PixelsToMilliinches(width);
				FormattedFieldInfo ffi = Qm.ResultsFormatter.FormatStructure(cs, new CellStyleMx(), 's', 0, miWidth, -1);

				ImageAnnotation a = pane.Annotations.AddImageAnnotation(axis.Name + si, ffi.FormattedBitmap);
				PaneAnchorPoint ap = (PaneAnchorPoint)a.AnchorPoint;

				if (axis is AxisX)
				{
					ap.AxisYCoordinate.AxisValue = 0;
					ap.AxisXCoordinate.AxisValue = si;
				}

				else if (axis is AxisY)
				{
					ap.AxisXCoordinate.AxisValue = 0;
					ap.AxisYCoordinate.AxisValue = si;
				}

				else throw new Exception("Unexpected axis type");

				RelativePosition pos = (RelativePosition)a.ShapePosition; // position is relative to the pane
				pos.Angle = 270;
				pos.ConnectorLength = 200;

				a.SizeMode = ChartImageSizeMode.Zoom;
				a.Height = height;
				a.Width = width;
				a.LabelMode = true; // fit into diagram
				a.ConnectorStyle = AnnotationConnectorStyle.Line;
				a.ShapeKind = ShapeKind.Rectangle;

				a.RuntimeAnchoring = true;
				a.RuntimeMoving = true;
				a.RuntimeResizing = true;
				a.RuntimeRotation = true;
			}
		}

		/// <summary>
		/// Custom drawing of bubble marker
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_CustomDrawSeriesPoint(object sender, CustomDrawSeriesPointEventArgs e)
		{
			// This function is called as each point is drawn to set the marker type and color, the label and
			// any any legend items. It does not add significantly to the rendering time for large data sets

			MarkerKind mk;

			ChartControlMx cc = ChartControl;
			if (cc == null || cc.Series.Count == 0 || Qm == null)
				return;

			SeriesPoint point = e.SeriesPoint;
			PointDrawOptionsBase options = e.SeriesDrawOptions as PointDrawOptionsBase; // options for point
			MarkerBase marker = options.Marker; // ref to marker options

			if (!NativeMarkerRendering) // not rendered by DevExpress
			{ // hide the point
				//point.IsEmpty = true; // (can't set to empty if want to see legend)
				options.Color = Color.Transparent;
				options.Marker.BorderVisible = false;
				e.LabelText = "";

				SetLegendItemAttributes(e);

				return;
			}

			int dri = ChartPanel.GetDataRowIndex(e.SeriesPoint);
			if (dri < 0) return;
			object[] drVo = Qm.DataTable.Rows[dri].ItemArrayRef;

			if (point.IsEmpty || !(point.Tag is SeriesPointTag))
			{
				e.LabelText = "";
				options.Color = Color.Transparent;
				options.Marker.BorderVisible = false;
				return;
			}

			if (Dtm.RowAttributesVoPos >= 0 && Dtm.FiltersEnabled)
			{ // hide point if filtered
				DataRowAttributes dra = Dtm.GetRowAttributes(drVo);
				if (dra.Filtered)
				{
					point.IsEmpty = true; // hide the point
					options.Color = Color.Transparent;
					options.Marker.BorderVisible = false;
					e.LabelText = "";
					return;
				}
			}

			SeriesPointTag tag = (SeriesPointTag)point.Tag;

			// Set marker shape

			if (MarkerShape.QueryColumn != null)
			{
				marker.Kind = tag.MarkerKind;
				if (marker.Kind == MarkerKind.Star && marker.StarPointCount != 5)
					marker.StarPointCount = 5;
			}

			// Set marker color and border

			if (MarkerColor.QueryColumn != null)
				options.Color = tag.Color;

			else
				options.Color = MarkerColor.FixedColor;

			if (Dtm.RowIsSelected(dri)) // show selected points with black border
				marker.BorderColor = Color.Black;

			marker.BorderVisible = true; // best to just leave border visible for now

			//options.Color = Color.White; // debug
			//marker.BorderColor = Color.Black;

			// Set marker label

			if (MarkerLabel.QueryColumn != null)
			{
				if (tag.Label != null) e.LabelText = tag.Label;
				else e.LabelText = "";
			}

			else if (e.LabelText != "")
				e.LabelText = "";

			// Set any legend info

			SetLegendItemAttributes(e);

			return;
		}

		/// <summary>
		/// Add any legend item from precomputed list of items
		/// </summary>
		/// <param name="e"></param>

		void SetLegendItemAttributes(CustomDrawSeriesPointEventArgs e)
		{
			PointDrawOptionsBase legendOptions = e.LegendDrawOptions as PointDrawOptionsBase; // options for corresponding legend item
			MarkerBase legendMarker = legendOptions.Marker; // ref to marker options

			SeriesPoint point = e.SeriesPoint;
			if (point.SeriesPointID < LegendItems.Count)
			{
				//if (point.SeriesPointID > 10) point = point; // debug
				LegendItem i = LegendItems[point.SeriesPointID];

				//DebugLog.Message("CustomDrawSeriesPoint " + point.SeriesPointID + ", " + i.ShowMarker + ", " +
				//  i.MarkerKind + ", " + i.Text);

				if (i.ShowMarker)
				{
					legendOptions.Color = i.Color;
					legendMarker.Kind = i.MarkerKind;
					legendMarker.BorderVisible = true;
					legendMarker.BorderColor = Color.DarkGray;
					e.LegendMarkerVisible = true;
					if (i.MarkerSize > 0)
					{ // Special case image for small "size by" legend item
						e.LegendMarkerImage = ChartPanel.MarkerSquareSmallImage.Image;
						//e.LegendMarkerSize = new Size(20, 20);
						//e.LegendMarkerImageSizeMode = ChartImageSizeMode.AutoSize;
					}
				}
				else
				{
					e.LegendMarkerVisible = false;
					e.LegendFont = new Font(e.LegendFont, FontStyle.Bold); // make text-only lines bold
				}

				e.LegendText = i.Text;
				e.LegendTextVisible = true;
			}

			else
			{
				if (e.LegendText != "") e.LegendText = "";
				e.LegendMarkerVisible = e.LegendTextVisible = false;
			}

			return;
		}

		/// <summary>
		/// CustomDrawAxisLabel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_CustomDrawAxisLabel(object sender, CustomDrawAxisLabelEventArgs e)
		{
			AxisLabelItem i = e.Item as AxisLabelItem;
			AxisBase axis = i.Axis;
			AxisMx axisMx = null;

			if (Lex.Contains(axis.Name, "AxisX"))
				axisMx = XAxisMx;

			else if (Lex.Contains(axis.Name, "AxisY"))
				axisMx = YAxisMx;

			else throw new ArgumentException();

			QueryColumn qc = axisMx.QueryColumn;
			if (qc == null) return;
			if (qc.MetaColumn.IsContinuous)
			{
				if ((qc.MetaColumn.DataType == MetaColumnType.Integer || // if integer value avoid non-integer labels
					qc.MetaColumn.DataType == MetaColumnType.CompoundId) &&
					e.Item.Text.Contains("."))
					e.Item.Text = "";
				return; // if continuous ok as is (refine formatting?)
			}

			else // categorical string value, map the integer value to the associated string
			{
				int idx = (int)i.AxisValueInternal;
				if (i.AxisValueInternal != idx) // ignore if not integral value
				{
					i.Text = "";
					return;
				}

				ColumnStatistics stats = GetStats(qc);
				if (stats == null || idx < 0 || idx >= stats.DistinctValueList.Count)
				{ // no label if not within range of stats (may be added points)
					i.Text = "";
					return;
				}

				MobiusDataType mdt = stats.DistinctValueList[idx];
				if (NullValue.IsNull(mdt)) i.Text = "";

				else if (qc.MetaColumn.DataType == MetaColumnType.Structure)
				{ // convert smiles to a structure
					ChemicalStructureMx cs = mdt as ChemicalStructureMx;
					if (cs != null)
						i.Text = ((ChemicalStructureMx)mdt).SmilesString;
				}

				else i.Text = stats.DistinctValueList[idx].ToString();

				return;
			}
		}

		/// <summary>
		/// Custom Paint
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControl_CustomPaint(object sender, CustomPaintEventArgs e)
		{
			Rectangle rt;
			Point p, p2, p3, dp1, dp2;
			Color borderColorForPoint;
			double xmin, ymin, xmax, ymax;
			double dw, dh;
			int markerWidth = 0, markerHeight = 0;
			int pi, selectedCount=0;

			//SystemUtil.Beep();

			if (!CustomMarkerRendering) return;

			DateTime t0 = DateTime.Now;

			Graphics g = e.Graphics;
			Rectangle drt = DiagramToPointHelper.CalculateDiagramBounds(XYDiagram);
			g.SetClip(drt);

			RenderBackgroundImage(e);

			XYDiagram diagram = XYDiagram;

			dp1 = new Point(drt.Left, drt.Top);
			dp2 = new Point(drt.Right, drt.Bottom);

			DiagramCoordinates dc1 = diagram.PointToDiagram(dp1);
			DiagramCoordinates dc2 = diagram.PointToDiagram(dp2);

			xmin = Math.Min(dc1.NumericalArgument, dc2.NumericalArgument);
			xmax = Math.Max(dc1.NumericalArgument, dc2.NumericalArgument);

			ymin = Math.Min(dc1.NumericalValue, dc2.NumericalValue);
			ymax = Math.Max(dc1.NumericalValue, dc2.NumericalValue);
			MarkerHelper mh = new MarkerHelper();

			int ptsRendered = 0;

			Color markerColor = MarkerColor.FixedColor; // fixed color
			bool getColorFromDataPoint = (MarkerColor.QueryColumn != null);

			Color borderColor = MarkerColor.BorderColor; // fixed border color
			
			VisualRange sr = AxisX.VisualRange;
			double axisRange = Math.Abs(sr.MaxValueInternal - sr.MinValueInternal);

			WholeRange ar = AxisX.WholeRange; // visible width
			double visibleRange = Math.Abs(ar.MaxValueInternal - ar.MinValueInternal);
			double zoomScale = axisRange / visibleRange;

			int paneSize = diagram.DefaultPane.SizeInPixels;

			bool recalculateSizes = true;
			if (ObjectEx.IsSameOrSubclassOf(this, typeof(ChartViewHeatmap)))
			{
				((ChartViewHeatmap)this).CalculateHeatmapMarkerSize(out dw, out dh);
				double rmrs = MarkerHelper.RectangularMarkerRelativeSize(); 
				markerWidth = (int)(dw / rmrs); // scale up because of relatively smaller size of rectangular marker
				markerHeight = (int)(dh / rmrs);
				recalculateSizes = false;
				borderColor = Color.Transparent;
			}

			for (pi = 0; pi < SeriesPoints.Length; pi++)
			{
				SeriesPoint sp = SeriesPoints[pi];
				if (sp.IsEmpty) continue; // skip if filtered
				double value = sp.Values[0];
				double markerSizeValue = sp.Values[1]; // size of marker on axis

				if (sp.NumericalArgument < xmin || sp.NumericalArgument > xmax) continue;
				if (value < ymin || value > ymax) continue;

				SeriesPointTag tag = sp.Tag as SeriesPointTag;

				p = diagram.DiagramToPoint(sp.NumericalArgument, value).Point; // pixel position for marker

				if (recalculateSizes)
					CalculateMarkerSizeInPixels(sp, drt, zoomScale, out markerHeight, out markerWidth);

				if (getColorFromDataPoint) markerColor = tag.Color;

				int dri = tag.DataRowIndex;
				if (Dtm.RowIsSelected(dri)) // show selected points with black border
				{
					borderColorForPoint = Color.Black;
					selectedCount++;
				}

				else borderColorForPoint = borderColor;

				mh.RenderMarker(tag.MarkerKind, markerWidth, markerHeight, markerColor, borderColorForPoint, p, g);

				ptsRendered++;
			}

			double ms = TimeOfDay.Delta(t0);
			return;
		}

/// <summary>
/// CalculateMarkerSizeInPixels
/// </summary>
/// <param name="sp"></param>
/// <param name="drt"></param>
/// <returns></returns>

		void CalculateMarkerSizeInPixels(
			SeriesPoint sp,
			Rectangle drt,
			double zoomScale,
			out int width,
			out int height)
		{
			double dw, dh;

			if (this is ChartViewHeatmap)
			{ // get size in pixels, adjusted for scale/zoom
				((ChartViewHeatmap)this).CalculateHeatmapMarkerSize(out dw, out dh);
			}

			else // get size & then adjust for current scale/zoom
			{
				if (MarkerSize.QueryColumn != null) // variable size
				{ // don't resize with zoom (do now), don't resize with window resize (DXP does)
					dw = dh = sp.Values[1] * .227; // magic scale value to match native draw
				}

				else // fixed size
				{
					double maxSize = drt.Width * .05; // 5% of diagram width is max marker size in pixels
					dw = dh = maxSize * (MarkerSize.FixedSize / 100.0); // convert fixed size as a percentage to pixels
				}

				dw *= zoomScale; // scale for zooming
				dh *= zoomScale; // scale for zooming
			}

			if (dw < 1) dw = 1;
			width = (int)dw;

			if (dh < 1) dh = 1;
			height = (int)dh;

			return;
		}

		/// <summary>
		/// Chart scrolled event from chart control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_Scroll(object sender, ChartScrollEventArgs e)
		{
			AdjustBackgroundImage();
			return;
		}

		/// <summary>
		/// Chart zoomed event from chart control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_Zoom(object sender, ChartZoomEventArgs e)
		{
			AdjustBackgroundImage();

			int pct = GetZoomPct();
			SessionManager.Instance.StatusBarManager.ZoomSliderPct = pct;

			return;
		}

		/// <summary>
		/// Get the pct of zoom for the view as an average of the x and y zoom
		/// </summary>
		/// <returns></returns>

		public int GetZoomPct()
		{

			if (XYDiagram == null || AxisX == null || AxisY == null)
				return 100; // return default if axes not yet defined

			int s1 = GetRangeZoomPct(AxisX); // update pct on zoom control
			int s2 = GetRangeZoomPct(AxisY);
			int pct = (s1 + s2) / 2; // take mean
			return pct;
		}

		/// <summary>
		/// Get the pct view scale of the specified range
		/// </summary>
		/// <param name="range"></param>
		/// <returns></returns>

		int GetRangeZoomPct(
			AxisBase axis)
		{
			WholeRange r = axis.WholeRange;
			VisualRange sr = axis.VisualRange;
			double sw = sr.MaxValueInternal - sr.MinValueInternal; // scrolling width
			double vw = r.MaxValueInternal - r.MinValueInternal; // visible width

			int pct = (int)(sw / vw * 100);
			return pct;
		}

		/// <summary>
		/// Scale the view to the specified pct value
		/// </summary>
		/// <param name="pct"></param>

		public override void ScaleView(int pct)
		{
			ScaleByPct(AxisX, pct);
			ScaleByPct(AxisY, pct);
			AdjustBackgroundImage();
			return;
		}

		/// <summary>
		/// Scale a range to specified percent
		/// </summary>
		/// <param name="range"></param>
		/// <param name="pct"></param>

		void ScaleByPct(
			AxisBase axis,
			int pct)
		{
			double scale = pct / 100.0;
			if (scale < 1) scale = 1;

			WholeRange r = axis.WholeRange;
			VisualRange sr = axis.VisualRange;
			double sw = sr.MaxValueInternal - sr.MinValueInternal; // scrolling width
			double vw = sw / scale; // visible width
			double min = sr.MinValueInternal + (sw - vw) / 2;
			double max = min + vw;
			sr.SetMinMaxValues(min, max);
		}

		/// <summary>
		/// Handle mouse down events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && // if left mouse button
			 XYDiagram.ScrollingOptions.UseMouse == false && // and not in mouse-scrolling mode
			 (Control.ModifierKeys & Keys.Shift) == 0 && // and shift key not down
			 (Control.ModifierKeys & Keys.Alt) == 0) // and alt key not down
			{
				ShowSelectionRectangle(sender, e); // show the selection rectangle
			}

			return;
		}

		/// <summary>
		/// Handle mouse movement events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_MouseMove(object sender, MouseEventArgs e)
		{
			if (DrawingSelectionRectangle)
			{
				DiagramCoordinates dc = XYDiagram.PointToDiagram(new Point(e.X, e.Y));
				if (!dc.IsEmpty) // be sure still within diagram
					UpdateSelectionRectangle(sender, e);
				return;
			}

			else if (CustomMarkerRendering) // if custom rendering show tooltip if over point marker
			{
				int x = e.X;
				int y = e.Y;
				ChartHitInfoMx hi = CalcHitInfo(x, y);
				if (hi.SeriesPoint != null)
				{
					ShowPointToolTip(hi.SeriesPoint);
					return;
				}
				else
				{
					ChartPanel.ToolTipController.HideHint(); // hide any hint
					CurrentTooltipObject = null;
				}
			}

			return;

		}

		/// <summary>
		/// Handle mouse up events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_MouseUp(object sender, MouseEventArgs e)
		{
			if (!DrawingSelectionRectangle) return;

			Point p1 = SelectionRectangleOrigin;
			Point p2 = SelectionRectangleEndPoint;

			HideSelectionRectangle();

			UpdateSelectedRows(p1, p2);
			Refresh();
			return;
		}

		/// <summary>
		/// Update the set of selected rows from the selection rectangle
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>

		void UpdateSelectedRows(Point p1, Point p2)
		{
			SeriesPoint p;
			SeriesPointTag tag;
			double d, d1, d2;
			int c1, c2;
			DateTime dt, dt1, dt2;
			object o;

			bool extending = (Control.ModifierKeys == Keys.Control); // extending selection

			if (p1.X < 0 && p1.Y < 0) return;

			if (p2.X < 0 && p2.Y < 0) p2 = p1; // if p2 undefined make it the same as p1

			// Single point

			if (Math.Abs(p1.X - p2.X) < 4 && Math.Abs(p1.Y - p2.Y) < 4) // if a single point or very small area treat like click
			{
				ChartHitInfoMx hi = CalcHitInfo(p1.X, p1.Y);
				p = hi.SeriesPoint;

				Object hitObject = hi.HitObject;

				// Clicked on point

				if (p != null) // series point
				{
					int dri = ChartPanel.GetDataRowIndex(p);
					if (dri < 0) return;

					DataRowMx dr = Qm.DataTable.Rows[dri];
					if (dr == null) return; // just in case

					if (extending) // extending selection
					{
						if (!Dtm.RowIsSelected(dr))
							Dtm.SelectRow(dr, true);

						else Dtm.SelectRow(dr, false); // remove from list of selected rows
					}

					else // resetting selection to single point 
					{
						Dtm.SelectAllRows(false);
						Dtm.SelectRow(dr, true);
					}

				}

				else // no series point at the clicked location 
				{
					if (!extending) Dtm.SelectAllRows(false); // clear selection
				}

			}

			else // Update from selected rectangle
			{
				DiagramCoordinates dc1 = XYDiagram.PointToDiagram(p1);
				DiagramCoordinates dc2 = XYDiagram.PointToDiagram(p2);

				ScaleType argScaleType = ChartControl.Series[0].ArgumentScaleType;
				ScaleType valueScaleType = ChartControl.Series[0].ValueScaleType;

				for (int pi = 0; pi < SeriesPoints.Length; pi++)
				{
					if (!CustomMarkerRendering)
						p = Series.Points[pi];
					else p = SeriesPoints[pi];

					bool inRectangle = false;
					do
					{
						tag = (SeriesPointTag)p.Tag;

						// Check X value for valid range

						if (argScaleType == ScaleType.Numerical)
						{
							d = p.NumericalArgument;
							if (d == double.NaN) break;
							d1 = dc1.NumericalArgument;
							d2 = dc2.NumericalArgument;
							if (d < d1 && d < d2) break;
							if (d > d1 && d > d2) break;
						}

						else if (argScaleType == ScaleType.DateTime)
						{
							dt = p.DateTimeArgument;
							if (dt.Equals(DateTime.MinValue)) break;
							c1 = dt.CompareTo(dc1.DateTimeArgument);
							c2 = dt.CompareTo(dc2.DateTimeArgument);
							if (c1 < 0 && c2 < 0) break;
							if (c1 > 0 && c2 > 0) break;
						}

						else throw new ArgumentException();

						// Check Y value for valid range

						if (valueScaleType == ScaleType.Numerical)
						{
							d = p.Values[0];
							if (d == double.NaN) break;
							d1 = dc1.NumericalValue;
							d2 = dc2.NumericalValue;
							if (d < d1 && d < d2) break;
							if (d > d1 && d > d2) break;
						}

						else throw new ArgumentException();

						inRectangle = true;
					}
					while (false);

					if (tag != null)
					{
						if (inRectangle)
							Dtm.SelectRow(tag.DataRowIndex, true);

						else if (!extending)
							Dtm.SelectRow(tag.DataRowIndex, false);
					}
				} // row loop
			}

			ChartControl.RefreshData(); // redraw the chart control
			Dtm.UpdateSelectedAndMarkedCounts();

			ResultsPagePanel.RefreshDetailsOnDemand(); // redraw details on demand
			return;
		}

		/// <summary>
		/// KeyDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && !DrawingSelectionRectangle)
			{ // if space key pressed & not drawing the selection rectangle
				XYDiagram.ScrollingOptions.UseMouse = true; // allow scrolling with mouse
			}

			else
			{
				XYDiagram.ScrollingOptions.UseMouse = false; // don't allow scrolling with mouse
			}

			return;
		}

		/// <summary>
		/// KeyUp
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControl_KeyUp(object sender, KeyEventArgs e)
		{
			XYDiagram.ScrollingOptions.UseMouse = false; // no scrolling with mouse on any key up (not just space)

			return;
		}

		/// <summary>
		/// Mouse enters chart control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControl_MouseEnter(object sender, EventArgs e)
		{ // Reset user interaction state info
			DrawingSelectionRectangle = false;
			CurrentTooltipObject = null;
		}

		/// <summary>
		/// Mouse leave chart control event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_MouseLeave(object sender, EventArgs e)
		{
			ChartPanel.ToolTipController.HideHint(); // hide any hint when mouse leaves chart control
			CurrentTooltipObject = null;

			HideSelectionRectangle();
		}

		/// <summary>
		/// Handle mouse click events 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_MouseClick(object sender, MouseEventArgs e)
		{
			ChartHitInfoMx hi = CalcHitInfo(e.X, e.Y);
			if (hi == null) return;

			SeriesPoint point = hi.SeriesPoint;
			Object hitObject = hi.HitObject;
			if (hitObject == null) return;

			if (typeof(XYDiagram).IsAssignableFrom(hitObject.GetType()))
			{
				if (e.Button == MouseButtons.Right && // show chart properties context menu on right-button click
				 ChartPanel.ChartPropertiesContextMenu != null)
					ChartPanel.ChartPropertiesContextMenu.Show(ChartPanel, new System.Drawing.Point(e.X, e.Y));

				return;
			}

			else if (point != null)
			{
				if (e.Button == MouseButtons.Right)
					ChartPanel.ShowPointPropertiesContextMenu(point, e);

				return;
			}

			else if (hitObject is AxisX)
			{
				ChartPropertiesDialog.ShowDialog(this, "X-Axis");
				return;
			}

			else if (hitObject is AxisY)
			{
				ChartPropertiesDialog.ShowDialog(this, "Y-Axis");
				return;
			}

			else if (hitObject is Legend)
			{
				if (e.Button == MouseButtons.Right && // show legend properties context menu on right-button click
				 ChartPanel.LegendPropertiesContextMenu != null)
					ChartPanel.LegendPropertiesContextMenu.Show(ChartPanel, new System.Drawing.Point(e.X, e.Y));

				return;
			}

			return;
		}

		/// <summary>
		/// Handle chart element tracking (can't hilight individual points in native version)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_ObjectHotTracked(object sender, HotTrackEventArgs e)
		{
			ToolTipController ttc = ChartPanel.ToolTipController;
			SuperToolTip stt;
			ToolTipControllerShowEventArgs ttcArgs;
			QueryColumn qc;

			if (DrawingSelectionRectangle) // don't display while drawing selection rectangle
			{
				e.Cancel = true;
				return;
			}

			if (e.AdditionalObject is SeriesPoint) // show tooltip for series point
			{
				SeriesPoint point = e.AdditionalObject as SeriesPoint;

				ShowPointToolTip(point);
				return;
			}

// Show structure tooltip if structure is on axis & mouse if over the Smiles for the structure

			else if (e.HitInfo.InAxis && e.AdditionalObject is AxisLabelItem)
			{
				AxisLabelItem label = e.AdditionalObject as AxisLabelItem;
				AxisBase axis = label.Axis;

				if (IsAxisX(axis)) qc = XAxisMx.QueryColumn;
				else if (IsAxisY(axis)) qc = YAxisMx.QueryColumn;
				else throw new ArgumentException();

				if (CurrentTooltipObject is AxisLabelItem && ((AxisLabelItem)CurrentTooltipObject) == label)
					return; // just return if same label

				if (qc != null && qc.MetaColumn.DataType == MetaColumnType.Structure && // structure col
					label.AxisValueInternal == (int)label.AxisValueInternal) // and integral value
				{
					stt = BuildAxisLabelStructureToolTip(qc, label);
					if (stt != null)
					{
						ttcArgs = BuildSuperTooltipArgs(stt, ChartPanel.ChartControl);
						Point p = e.HitInfo.HitPoint;
						Point ccScreenLoc = ChartControl.PointToScreen(new Point(0, 0)); // screen loc of chart control
						p = new Point(p.X + ccScreenLoc.X, p.Y + ccScreenLoc.Y - 20);

						ttc.HideHint(); // be sure any existing tooltip is hidden first
						ttc.ShowHint(ttcArgs, p);
						CurrentTooltipObject = label; // save ref
						return;
					}
				}
			}

			// Just hide tip unless show point for non-native marker rendering

			else if (NativeMarkerRendering || !(CurrentTooltipObject is SeriesPoint))
			{
				ttc.HideHint();
				CurrentTooltipObject = null;
				return;
			}
		}

/// <summary>
/// Show any tooltip for a point
/// </summary>
/// <param name="point"></param>

		void ShowPointToolTip(
			SeriesPoint point)
		{
			ToolTipController ttc = ChartPanel.ToolTipController;
			SuperToolTip stt;
			ToolTipControllerShowEventArgs ttcArgs;

			if (CurrentTooltipObject is SeriesPoint && ((SeriesPoint)CurrentTooltipObject).SeriesPointID == point.SeriesPointID)
				return; // just return if same point

			if (CurrentTooltipObject != null) stt = null;

			int dri = ChartPanel.GetDataRowIndex(point);

			TooltipDimensionDef mtt = MarkerTooltip;
			if (mtt.Fields.Count == 0)
			{ // create default tooltip
				mtt = new TooltipDimensionDef();
				mtt.Fields = GetDefaultTooltipFields();
			}

			stt = BuildDataRowTooltip(mtt, dri);

			ttcArgs = BuildSuperTooltipArgs(stt, ChartPanel.ChartControl);

			ControlCoordinates ccc = GetControlCoordinatesOfSeriesPoint(point);
			Point p = ccc.Point; // coords within chart control
			Point ccScreenLoc = ChartControl.PointToScreen(new Point(0, 0)); // screen loc of chart control
			p = new Point(p.X + ccScreenLoc.X, p.Y + ccScreenLoc.Y - 20);

			ttc.HideHint(); // be sure any existing tooltip is hidden first
			ttc.ShowHint(ttcArgs, p);
			CurrentTooltipObject = point; // save ref
			return;

			//Rectangle cr = ChartControl.FindForm().ClientRectangle; // 
			//Point fl = ChartControl.FindForm().Location;
			//Point cl = ChartControl.Location;
			//Point ccClientLoc = ChartControl.PointToClient(new Point(0, 0));
			//fl = new Point(fl.X + ccClientLoc.X, fl.Y + ccClientLoc.Y);

			//Point p = ChartControl.FindForm().PointToClient(ccc.Point);
		}

		/// <summary>
		/// GetControlCoordinatesOfDataPoint for both numeric & date x axis
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>

		ControlCoordinates GetControlCoordinatesOfSeriesPoint(SeriesPoint p)
		{
			ControlCoordinates ccc;

			if (!double.IsNaN(p.NumericalArgument)) // get point screen coords from point axis coords
				ccc = XYDiagram.DiagramToPoint(p.NumericalArgument, p.Values[0], AxisX, AxisY);
			else ccc = XYDiagram.DiagramToPoint(p.DateTimeArgument, p.Values[0], AxisX, AxisY);

			return ccc;
		}

		/// <summary>
		/// GetDefaultTooltipFields
		/// </summary>
		/// <returns></returns>

		List<ColumnMapSx> GetDefaultTooltipFields()
		{
			List<ColumnMapSx> fl = new List<ColumnMapSx>();

			if (BaseQuery.Tables.Count == 0) return fl;
			QueryTable qt = BaseQuery.Tables[0];

			ColumnMapSx fli = new ColumnMapSx(qt.KeyQueryColumn);
			fl.Add(fli);

			if (XAxisMx.QueryColumn != null && !XAxisMx.QueryColumn.IsKey)
				fl.Add(new ColumnMapSx(XAxisMx.QueryColumn));
			if (YAxisMx.QueryColumn != null && !YAxisMx.QueryColumn.IsKey)
				fl.Add(new ColumnMapSx(YAxisMx.QueryColumn));
			if (MarkerColor.QueryColumn != null && !MarkerColor.QueryColumn.IsKey)
				fl.Add(new ColumnMapSx(MarkerColor.QueryColumn));
			if (MarkerSize.QueryColumn != null && !MarkerSize.QueryColumn.IsKey)
				fl.Add(new ColumnMapSx(MarkerSize.QueryColumn));
			if (MarkerShape.QueryColumn != null && !MarkerShape.QueryColumn.IsKey)
				fl.Add(new ColumnMapSx(MarkerShape.QueryColumn));

			return fl;
		}

		/// <summary>
		/// Build a super tooltip to display a structure associated with an axis label
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="label"></param>
		/// <returns></returns>

		SuperToolTip BuildAxisLabelStructureToolTip(
			QueryColumn qc,
			AxisLabelItem label)
		{
			AxisBase axis = label.Axis;
			int idx = (int)label.AxisValueInternal;

			MetaColumn mc = qc.MetaColumn;

			ColumnStatistics stats = GetStats(qc);
			if (stats == null || stats.DistinctValueList.Count == 0)
				return null;

			ChemicalStructureMx cs = stats.DistinctValueList[idx] as ChemicalStructureMx;
			if (cs == null) return null;

			SuperToolTip s = new SuperToolTip();
			s.MaxWidth = 200;
			ToolTipItem i = new ToolTipItem();
			int width = ResultsFormatFactory.QcWidthInCharsToDisplayColWidthInMilliinches(mc.Width, ResultsFormat);
			FormattedFieldInfo ffi = Qm.ResultsFormatter.FormatStructure(cs, new CellStyleMx(), 's', 0, width, -1);
			i = AppendStructureToToolTip(s, i, ffi.FormattedBitmap);

			return s;
		}

		/// <summary>
		/// Handle chart element selection (can't select individual points)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartControlMx_ObjectSelected(object sender, HotTrackEventArgs e)
		{
			return;
		}

		/// <summary>
		/// Update the view to reflect changes in filtering
		/// </summary>

		public override void UpdateFilteredView()
		{
			BuildSeriesDataPoints(false); // must update all dimensions since row assignments may shift due to filtering

			if (NativeMarkerRendering) // replace chart control series points if doing native rendering
			{
				Series.Points.Clear(); 
				Series.Points.AddRange(SeriesPoints);
			}

			ChartControl.Refresh();

			return;
		}

		/// <summary>
		/// Refresh the view
		/// </summary>

		public override void Refresh()
		{
			DateTime t0 = DateTime.Now;
			if (ChartControl != null)
				ChartControl.Refresh();
			double tDelta = TimeOfDay.Delta(t0);
			RefreshCount++;
			return;
		}

		/// <summary>
		/// Get hit info for chart including for CustomMarkerRendering 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>

		public ChartHitInfoMx CalcHitInfo(
			int x,
			int y)
		{
			Point p, p2;
			int pi, height = -1, width = -1;

			ChartHitInfo hi0 = ChartControl.CalcHitInfo(x, y);
			ChartHitInfoMx hi = new ChartHitInfoMx(hi0);
			if (hi.HitTest != ChartHitTest.Diagram || !CustomMarkerRendering) return hi;

			XYDiagram diagram = XYDiagram;
			Rectangle drt = DiagramToPointHelper.CalculateDiagramBounds(diagram);

			VisualRange sr = AxisX.VisualRange;
			double axisRange = Math.Abs(sr.MaxValueInternal - sr.MinValueInternal);

			WholeRange ar = AxisX.WholeRange; // range of axis
			double visibleRange = Math.Abs(ar.MaxValueInternal - ar.MinValueInternal);
			double zoomScale = axisRange / visibleRange;

			bool variableSize = MarkerSize.QueryColumn != null;

			// Scan the set of points looking for a hit

			for (pi = 0; pi < SeriesPoints.Length; pi++)
			{
				SeriesPoint sp = SeriesPoints[pi];
				if (sp.IsEmpty) continue; // skip if filtered

				if (variableSize || width < 0) // calc size if necessary
				{
					CalculateMarkerSizeInPixels(sp, drt, zoomScale, out width, out height);
					width /= 2; // take half
					if (width < 1) width = 1;
					height /= 2;
					if (height < 1) height = 1;
				}

				double arg = sp.NumericalArgument;
				double value = sp.Values[0];
				p = diagram.DiagramToPoint(arg, value).Point; // pixel position for marker
				if (x < p.X - width || x > p.X + width || y < p.Y - height || y > p.Y + height) continue;

				hi.HitTest = ChartHitTest.Series;
				hi.HitObject = Series;
				hi.Series = Series;
				hi.InSeries = true;
				hi.SeriesPoint = sp;
				break;
			}

			return hi;
		}

	}

/// <summary>
/// Contains information about a specific point within a chart
/// </summary>

	public class ChartHitInfoMx
	{
		public Annotation Annotation;
		public AxisBase Axis;
		public AxisLabelItemBase AxisLabelItem;
		public AxisTitle AxisTitle;
		public IChartContainer Chart;
		public ChartTitle ChartTitle;
		public ConstantLine ConstantLine;
		public Diagram Diagram;
		public object HitObject;
		public object[] HitObjects;
		public Point HitPoint;
		public ChartHitTest HitTest;
		public bool InAnnotation;
		public bool InAxis;
		public bool InChart;
		public bool InChartTitle;
		public bool InConstantLine;
		public bool InDiagram;
		public Indicator Indicator;
		public bool InIndicator;
		public bool InLegend;
		public bool InNonDefaultPane;
		public bool InSeries;
		public bool InSeriesLabel;
		public bool InSeriesTitle;
		public Legend Legend;
		public XYDiagramPane NonDefaultPane;
		public SeriesBase Series;
		public SeriesLabelBase SeriesLabel;
		public SeriesPoint SeriesPoint;
		public SeriesTitle SeriesTitle;

		public ChartHitInfoMx()
		{	return; }

		public ChartHitInfoMx(ChartHitInfo hi)
		{
			Annotation =        hi.Annotation; 
			Axis =							hi.Axis;
			AxisLabelItem =     hi.AxisLabelItem;
			AxisTitle =					hi.AxisTitle; 
			Chart =							hi.Chart; 
			ChartTitle =				hi.ChartTitle; 
			ConstantLine =			hi.ConstantLine; 
			Diagram =						hi.Diagram; 
			HitObject =					hi.HitObject; 
			HitObjects =				hi.HitObjects; 
			HitPoint =					hi.HitPoint; 
			HitTest =		        hi.HitTest; 
			InAnnotation =			hi.InAnnotation; 
			InAxis =						hi.InAxis; 
			InChart =						hi.InChart; 
			InChartTitle =			hi.InChartTitle; 
			InConstantLine =		hi.InConstantLine; 
			InDiagram =					hi.InDiagram; 
			Indicator =		      hi.Indicator; 
			InIndicator =				hi.InIndicator; 
			InLegend =					hi.InLegend; 
			InNonDefaultPane =	hi.InNonDefaultPane; 
			InSeries =					hi.InSeries; 
			InSeriesLabel =			hi.InSeriesLabel; 
			InSeriesTitle =			hi.InSeriesTitle; 
			Legend =						hi.Legend; 
			NonDefaultPane =		hi.NonDefaultPane; 
			Series =						hi.Series; 
			SeriesLabel =				hi.SeriesLabel; 
			SeriesPoint =				hi.SeriesPoint; 
			SeriesTitle =				hi.SeriesTitle; 
		}

	}
}
