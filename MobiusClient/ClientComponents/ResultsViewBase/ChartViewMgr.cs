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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// SpotfireViewMgr2 supports more flexible Spotfire visualizations than the original SpotfireViewMgr
	/// </summary>

	public partial class ChartViewMgr : ViewManager
	{
		internal ChartPageControl ChartPage; // the "page" containing the chart controls
		internal ChartPagePanel ChartPagePanel { get { return ChartPage != null ? ChartPage.ChartPagePanel : null; } set { ChartPage.ChartPagePanel = value; } }
		internal ChartPanel ChartPanel { get { return ChartPagePanel != null ? ChartPagePanel.ChartPanel : null; } }
		internal ChartControlMx ChartControl { get { return ChartPanel != null ? ChartPanel.ChartControl : null; } } // the associated chart control 		
		internal Series Series; // the single series that is on the chart

		internal BubbleSeriesView BubbleView { get { return Series.View as BubbleSeriesView; } }

		internal XYDiagram XYDiagram { get { return ChartControl.Diagram as XYDiagram; } } // diagram subclass for 2D charts (e.g. Bubble)

		public static int LargeDatasetSize = 1000; // use special optimizations to render charts with more data points
		internal bool NativeMarkerRendering = true; // if true let DevExpress render the markers
		internal bool CustomMarkerRendering = false; // if true do custom high-speed marker rendering
		internal bool ForceCustomMarkerRendering = false; // set to true for chart types that must always use custom rendering, e.g. Heatmaps

		internal string CurrentBackgroundImageFile = ""; // current background image file, either a local name or a server-qualified name
		internal string CachedBackgroundImageFile = ""; // actual file name to use for image

/// <summary>
/// Get Axis2D AxisX object common to both XY and SP diagrams
/// </summary>

		internal Axis2D AxisX
		{
			get
			{
				return XYDiagram.AxisX;
			}
		}
/// <summary>
/// Get Axis2D AxisY object common to both XY and SP diagrams
/// </summary>
/// 
		internal Axis2D AxisY
		{
			get
			{
				return XYDiagram.AxisY;
			}
		}

		internal bool IsAxisX(AxisBase axis)
		{
			return Lex.Contains(axis.Name, "AxisX");
		}

		internal bool IsAxisY(AxisBase axis)
		{
			return Lex.Contains(axis.Name, "AxisY");
		}

		internal SeriesPoint [] SeriesPoints; // The array of series data points with attributes stored in the Tag field

		internal List<LegendItem> LegendItems = new List<LegendItem>();

		internal List<object> ChartPointIndexToDataTableIndex; // supports mapping of chart points back to one or more data rows

		internal SeriesPoint HilightedPoint; // point currently hilighted

		/// <summary>
		/// Default constructor
		/// </summary>

		public ChartViewMgr()
		{
			return; // type must be defined by subclass
		}

		/// <summary>
		/// Return true if the view has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				return false; // not implemented
			}
		}

		/// <summary>
		/// Build and render the full chart
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			return; // implemented by specific chart type subclasses
		}

		/// <summary>
		/// Build subquery results if needed
		/// </summary>

		internal void BuildSubqueryResultsIfRequired()
		{
			throw new NotImplementedException(); // need anything here?
		}

		/// <summary>
		/// Build the chart panel
		/// </summary>
		/// <returns></returns>

		internal void BuildChartPanel()
		{
			ChartControl chart; // single cartesian or polar chart that is built


			if (!IsDefined) return; // don't build control if not ready for rendering yet

			else if (IsTrellis)
			{
				////CreateAndShowLegend(); // create & show legend before building trellis
				////BuildTrellis();
			}

			else // normal, non-trellised chart
			{
				////TrellisPanel.Visible = false;

				chart = BuildChart();
				chart.Tag = this; // link the chart to the associated Mobius chart info
				ChartControl.Tag = this; // also link the top-level chart control

				////XtraChartPanel.Visible = true;
				////ChartsPanel.ChildPanels.Add(chart); // add the chart to the charts panel
				////CreateAndShowLegend();
			}

			return;
		}

		/// <summary>
		/// Complete build of basic chart
		/// </summary>
		/// <returns></returns>

		internal ChartControl BuildChart()
		{
			ChartControl chart;

			// Build bubble chart

			if (ViewType == ViewTypeMx.ScatterPlot)
				chart = null; // chart = BuildScatterPlot();

// Build heatmap

			else if (ViewType == ViewTypeMx.Heatmap)
				throw new NotImplementedException(); // chart = BuildHeatMap(); 

// Build radar chart

			else if (ViewType == ViewTypeMx.RadarPlot)
				throw new NotImplementedException(); // chart = BuildRadarChart();

			else throw new NotImplementedException();

			// Setup global settings

			////SetupChartStretch();

			////SetupShapeRenderingMode();

			return chart;
		}

		/// <summary>
		/// Check that each QueryColumn references in the chart corresponds to a report field 
		/// </summary>

		internal void ValidateQueryColumns()
		{
			ValidateQueryColumn(ref XAxisMx.QueryColumn);
			ValidateQueryColumn(ref YAxisMx.QueryColumn);
			ValidateQueryColumn(ref ZAxisMx.QueryColumn);
			ValidateQueryColumn(ref MarkerColor.QueryColumn);
			ValidateQueryColumn(ref MarkerSize.QueryColumn);
			ValidateQueryColumn(ref MarkerShape.QueryColumn);
			ValidateQueryColumn(ref MarkerLabel.QueryColumn);
			return;
		}

		internal bool ValidateQueryColumn(ref QueryColumn qc)
		{
			if (qc == null) return false;
			MetaColumn mc = qc.MetaColumn;

			ColumnInfo colInfo = null;
			try { colInfo = Rf.GetColumnInfo(qc); }
			catch (Exception ex) { ex = ex; }
			if (colInfo == null || colInfo.Rfld == null)
			{
				qc = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Setup a marker series for color, size and shape
		/// </summary>


#if false
			NLength markerSize;
			NMarkerStyle ms;
			ColumnStatistics stats;

			NBubbleSeries series = Chart.Series[0] as NBubbleSeries;

			NIndexedAttributeSeries markerSeries =
				new NIndexedAttributeSeries(DataSeriesType.MarkerStyle, "MarkerStyle");

			NDataSeriesDouble nullSizeSeries =
				new NDataSeriesDouble("Sizes");

			if (Qm == null || Dt == null) // no data
			{
				series.MarkerStyles = markerSeries;
				series.Sizes = nullSizeSeries;
				return;
			}

			// Need full-size array of sizes for markers to appear (values appear to be ignored)

			object[] nullSizes = new object[Dt.Rows.Count];
			for (int i1 = 0; i1 < Dt.Rows.Count; i1++)
				nullSizes[i1] = 0; // i1 % 5;
			nullSizeSeries.AddRange(nullSizes);
			series.Sizes = nullSizeSeries;

			series.MarkerStyles.Clear();

			series.MarkerStyle.Visible = false; // turn off default style

			// Build the real marker series

			ApplyMarkerColorRules(); // apply rules & set colors in DataTable
			Color[] colors = GetMarkerColors(MarkerColor.Column, MarkerColor.FixedColor);

			int[] sizes = GetMarkerSizes(MarkerSizeColumn, MarkerMinSize, MarkerMaxSize);

			PointShape[] shapes = GetMarkerShapes();

			for (int ri = 0; ri < Dt.Rows.Count; ri++)
			{
				ms = new NMarkerStyle();
				ms.Visible = true;
				ms.FillStyle = new NColorFillStyle(colors[ri]);
				ms.BorderStyle.Color = MarkerColor.BorderColor;
				ms.BorderStyle.Width = new NLength(1, NGraphicsUnit.Pixel); // make marker borders 1 pixel wide
				ms.AutoDepth = false;

				if (ViewType == ViewTypeMx.ScatterPlot)
				{
					markerSize = new NLength(sizes[ri] / (1000f * .06f), NRelativeUnit.ParentPercentage); // reasonable relative size so trellis plots look ok (was NGraphicsUnit.Inch) 
					ms.Width = ms.Height = ms.Depth = markerSize;
					//				series.Sizes[ri] = sizes[ri];

					ms.PointShape = shapes[ri];
				}

				markerSeries.Add(ri, ms);
			}

			series.MarkerStyles = markerSeries;
			return;

		}
#endif

		/// <summary>
		/// Convert data types for purpose of conditional formatting matching
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>

		internal MetaColumnType NormalizeColumnType(MetaColumnType type)
		{
			return type; // noop

			//if (type == MetaColumnType.CompoundId)
			//  type = MetaColumnType.String; // map these types to strings so handled properly
			//return type;
		}

		/// <summary>
		/// Apply the color rules to the data & store new colors in DataTable MobiusDataType value object BackColor fields
		/// </summary>

		internal void ApplyMarkerColorRules()
		{
			QueryColumn qc = MarkerColor.QueryColumn; // .Clone(); // make copy we can attach current formatting to
			if (qc == null || qc.CondFormat == null || qc.CondFormat.Rules == null) return;
			CondFormatRules rules = qc.CondFormat.Rules;

			MetaColumn mc = qc.MetaColumn;

			ColumnStatistics stats = GetStats(qc);
			ColumnInfo colInfo = ResultsFormat.GetColumnInfo(qc);
			int voi = colInfo.DataColIndex;

			bool colInRootTable = false;
			if (BaseQuery.Tables.Count > 1) colInRootTable = qc.MetaColumn.MetaTable.IsRootTable;

			// "Normal" continuous-type cond formatting
			
			if (qc.MetaColumn.IsContinuous)
			{
				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					DataRowMx dr = Dt.Rows[ri];
					DataRowAttributes dra = Dtm.GetRowAttributes(dr);
					object o = dr[voi];
					MobiusDataType mdt = MobiusDataType.ConvertToMobiusDataType(mc.DataType, o);
					if (mdt == null)
					{
						if (mc.IsNumeric) mdt = new NumberMx();
						else if (mc.DataType == MetaColumnType.Date) mdt = new DateTimeMx();
						else throw new Exception("Unexpected DataType: " + mc.DataType);
					}

					CellStyleMx cs = // get the current color
						ResultsFormatter.GetCondFormatCellStyle(qc, mdt, Qm.ResultsFormat);

					if (cs != null) mdt.BackColor = cs.BackColor;
					else mdt.BackColor = CondFormatMatcher.DefaultMissingValueColor;
					dr[voi] = mdt; // store value back so grid gets redrawn
				}
			}

// Categorical (ordinal) data column

			else
			{
				Color c1 = CondFormatMatcher.DefaultMissingValueColor, c2 = CondFormatMatcher.DefaultMissingValueColor;
				int v1 = 0, v2 = 0;

				if (rules.Count >= 2)
					do
					{
						string key1 = rules[0].Value.ToUpper();
						string key2 = rules[1].Value.ToUpper();
						if (!stats.DistinctValueDict.ContainsKey(key1) ||
							!stats.DistinctValueDict.ContainsKey(key2)) break;

						v1 = stats.DistinctValueDict[key1].Ordinal;
						c1 = rules[0].BackColor1;
						v2 = stats.DistinctValueDict[key2].Ordinal;
						c2 = rules[1].BackColor1;
					}
					while (false);

				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					DataRowMx dr = Dt.Rows[ri];
					DataRowAttributes dra = Dtm.GetRowAttributes(dr);
					object o = dr[voi];
					MobiusDataType mdt = MobiusDataType.ConvertToMobiusDataType(mc.DataType, o);

					if (NullValue.IsNull(mdt))
						mdt.BackColor = CondFormatMatcher.DefaultMissingValueColor;

					else
					{
						string key = GetFormattedText(qc, mdt).ToUpper();
						if (stats.DistinctValueDict.ContainsKey(key)) // shouldn't happen
						{
							int v = stats.DistinctValueDict[key].Ordinal;
							mdt.BackColor = CondFormatMatcher.CalculateColorForGradientValue(v, v1, v2, c1, c2);
						}
						else mdt.BackColor = CondFormatMatcher.DefaultMissingValueColor; // shouldn't happen
					}

					dr[voi] = mdt; // store value back so grid gets redrawn
				}
			}

			return;
		}

/// <summary>
/// Build chart title(s)
/// </summary>

		internal void BuildTitles()
		{
			ChartControl.Titles.Clear();

			if (Lex.IsNullOrEmpty(Title) || !ShowTitle) return;
			
			ChartTitle title = new ChartTitle();
			title.Text = Title;
			ChartControl.Titles.Add(title);
		}

/// <summary>
/// Build the axes for the chart
/// </summary>

		internal void BuildAxes()
		{
			ChartViewBubble cvb = this as ChartViewBubble;
			cvb.BuildAxis(Series, XAxisMx, AxisX);
			cvb.BuildAxis(Series, YAxisMx, AxisY);
			return;
		}

/// <summary>
/// Build the data for an axis dimension of the chart
/// </summary>
/// <param name="columnName"></param>
/// <param name="axisMx"></param>
/// <param name="ueliminateFilteredOutValues"></param>

		internal void BuildDataForAxisDimension (
			AxisMx axisMx,
			bool isArg,
			bool eliminateFilteredOutValues)
		{
			DataTableMx sdt = null;
			DataTable ddt;
			DataRow dr;
			MobiusDataType mdt;
			SeriesPoint p;
			double min, max, rMin, rMax;
			object o, o2;
			int sci = -1, dci, cidInt;

			int t0 = TimeOfDay.Milliseconds();

			QueryColumn qc = axisMx.QueryColumn;
			if (qc == null) return; // anything to build?

			sdt = Qm.DataTable; // source data table
			sci = qc.VoPosition; // source column index

			MetaColumn mc = qc.MetaColumn;
			ColumnInfo colInfo = Rf.GetColumnInfo(qc);
			int voi = colInfo.DataColIndex;
			MetaColumnType mct = colInfo.Mc.DataType;
			string colName = colInfo.Mc.Name;
			int attrPos = Dtm.RowAttributesVoPos; // position for attributes

			ColumnStatistics stats = colInfo.Rfld.GetStats();
			if (stats == null) stats = new ColumnStatistics();

			if (stats.DistinctValueList.Count == 0 ||
			 stats.MaxValue == null || stats.MaxValue == null)
			{
				if (!isArg) return; // if arg need to continue to fill null values
			}

			bool categorical = false;

			if (mc.DataType == MetaColumnType.CompoundId)
			{
				categorical = true; // treat compound ids as categorical
			}

			else if (mc.IsNumeric)
			{ // get the range based on data values and user inputs
				if (stats.MinValue != null) min = stats.MinValue.NumericValue;
				else min = NullValue.NullNumber;

				if (stats.MaxValue != null) max = stats.MaxValue.NumericValue;
				else max = NullValue.NullNumber;

				if (AxisOptionsControl.ParseRangeValue(axisMx.RangeMin, out rMin))
					min = rMin;
				if (AxisOptionsControl.ParseRangeValue(axisMx.RangeMax, out rMax))
					max = rMax;

				if (axisMx.IncludeOrigin)
				{
					if (min > 0) min = 0;
					else if (max < 0) max = 0;
				}
			}

			else if (mc.DataType == MetaColumnType.Date) 
			{
				categorical = false; // todo: parse range?
			}

			else // categorical string data, DataSeriesType is "int" since string ordinals will be stored
			{
				categorical = true;
			}

			//ChartPointIndexToDataTableIndex = new List<object>(); // (need this now?)

//

			for (int ri = 0; ri < Dt.Rows.Count; ri++)
			{
				//if (CurrentColInRootTable) dr = Dt.Rows[dra.FirstRowForKey]; // get from root table row if appropriate

				o = GetVo(ri, qc, voi, eliminateFilteredOutValues, false);

				//if (o.ToString() == "02501503") o = o; // debug

				if (NullValue.IsNull(o)) // store missing values as DBNulls
					o = DBNull.Value;

				else if (mc.DataType == MetaColumnType.CompoundId)
				{
					if (o is CompoundId)
					{
						CompoundId cid = o as CompoundId;
						//o = cid.Value; // note: may have leading zeros but must for now to match key values
						o = GetFormattedText(qc, cid);
					}

					else o = o.ToString();
				}

				else if (o is ChemicalStructureMx)
				{
					ChemicalStructureMx cs = o as ChemicalStructureMx;
					o = cs.SmilesString; // Smiles strings are stored in stats distinct values
				}

				else if (o is MobiusDataType)
				{
					mdt = o as MobiusDataType;
					if (categorical) // use formatted version of data
						o = GetFormattedText(qc, mdt);

					else o = (o as MobiusDataType).ToPrimitiveType(); // use number or date continuous value
				}

				else if (o is string || o is DateTime || o.GetType().IsPrimitive) { } // ok as is

				else throw new Exception("Can't convert object to primitive type: " + o.GetType());

				if (o is string) // map categorical string data to the corresponding integer ordinal
				{
					string s = (o as string).ToUpper();
					//if (mc.DataType == MetaColumnType.Structure) ClientLog.Message(s); // debug
					if (!stats.DistinctValueDict.ContainsKey(s)) o = null; // shouldn't happen
					else o = stats.DistinctValueDict[s].Ordinal;
				}

				if (isArg) // must be argument, create new series point and set the argument value
				{

					if (NullValue.IsNull(o)) // null value?
					{
						p = new SeriesPoint(double.NaN, double.NaN, double.NaN);
						p.IsEmpty = true;
					}

					else // defined value
					{
						if (o is double)
							p = new SeriesPoint((double)o, double.NaN, double.NaN);
						else if (o is int)
							p = new SeriesPoint(Convert.ToDouble((int)o), double.NaN, double.NaN);
						else if (o is DateTime)
							p = new SeriesPoint((DateTime)o, double.NaN, double.NaN);
						else throw new ArgumentException();

						p.IsEmpty = false;
					}

					SeriesPointTag tag = new SeriesPointTag(); // alloc the tag
					tag.DataRowIndex = ri; // & set source row index
					if (p == null) continue;
					p.Tag = tag;

					p.SeriesPointID = ri; // set the id
					SeriesPoints[ri] = p;
				}

				else // must be a value, store it in the first value entry for the associated series point
				{
					p = SeriesPoints[ri]; 

					if (NullValue.IsNull(o))
						p.IsEmpty = true;

					else
					{
						if (o is double)
							p.Values[0] = (double)o;
						else if (o is int)
							p.Values[0] = Convert.ToDouble((int)o);
						else throw new ArgumentException();
						p.IsEmpty = false;
					}
				}

//				ChartPointIndexToDataTableIndex.Add(ri); // keep map back to data rows
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Setup the visible range for the axis
		/// </summary>
		/// <param name="axisMx"></param>
		/// <param name="axis"></param>

		internal void SetupAxisRange(
			AxisMx axisMx,
			Axis2D axis)
		{
			object minValue, maxValue;
			WholeRange wr = axis.WholeRange; // get range from chart control / DataTable

			if (axisMx.QueryColumn == null)
			{ // if no query column set to auto
				wr.Auto = true;
				return;
			}

			GetAxisRange(axisMx, out minValue, out maxValue); // get range of data from Mobius perspective
			if (minValue != null && maxValue != null) // defined range in data
			{
				if (minValue.Equals(maxValue)) // adjust so min < max as DX requires
				{
					if (minValue is DateTime)
					{
						DateTime minDt = (DateTime)minValue;
						DateTime maxDt = (DateTime)maxValue;
						if (minDt.CompareTo(maxDt) > 0)
						{
							minDt = (DateTime)maxValue;
							maxDt = (DateTime)minValue;
						}

						minValue = minDt.AddDays(-1);
						maxValue = maxDt.AddDays(1);
					}

					else if (minValue is int)
					{
						int minInt = (int)minValue;
						int maxInt = (int)maxValue;
						if (minInt > maxInt)
						{
							minInt = (int)maxValue;
							maxInt = (int)minValue;
						}

						minValue = minInt - 1;
						maxValue = maxInt + 1;
					}

					else if (minValue is double)
					{
						double minDouble = (double)minValue;
						double maxDouble = (double)maxValue;
						if (minDouble > maxDouble)
						{
							minDouble = (double)maxValue;
							maxDouble = (double)minValue;
						}

						minValue = minDouble - 1;
						maxValue = maxDouble + 1;
					}

					else throw new Exception("Unexpected type: " + minValue.GetType());
				}

				wr.Auto = false; // use supplied range
				wr.SideMarginsValue = 0; // do side margins manually

				try // set whole/visible range
				{ 
					wr.SetMinMaxValues(minValue, maxValue);
				} 
				catch (Exception ex) { ex = ex; }

				VisualRange vr = axis.VisualRange;
				vr.Auto = false; // set scrolling range to match
				vr.SideMarginsValue = 0; // do side margins manually
				vr.SetMinMaxValues(minValue, maxValue); // also set scrolling range

				if (axisMx.SideMarginsEnabled) // expand ranges to allow space for markers?
				{ 
					double d = (wr.MaxValueInternal - wr.MinValueInternal) * .04;
					//wr.SetInternalMinMaxValues(wr.MinValueInternal - d, wr.MaxValueInternal + d);
					//vr.SetInternalMinMaxValues(wr.MinValueInternal - d, wr.MaxValueInternal + d);
				}
			}

			else // undefined range
			{
				//ar.Auto = true; // just say auto for now
				//ar.SideMarginsEnabled = true;
			}

			return;
		}

		/// <summary>
		/// Calculate the axis range for a dimension or get the user-specified value
		/// </summary>
		/// <param name="axisMx"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>

		internal void GetAxisRange(
			AxisMx axisMx,
			out object minValue,
			out object maxValue)
		{
			DateTime dt;
			double d;

			QueryColumn qc = axisMx.QueryColumn;
			MetaColumn mc = qc.MetaColumn;
			ColumnStatistics stats = GetStats(qc);

			if (stats == null || stats.DistinctValueList.Count == 0) // no data
			{
				minValue = maxValue = null;
				return;
			}

			else if (mc.IsNumeric && mc.IsContinuous) // numeric and continuous
			{
				if (Lex.IsNullOrEmpty(axisMx.RangeMin) || !Lex.IsDouble(axisMx.RangeMin))
				{ // calculate if not specified
					d = stats.MinValue.NumericValue;
					if (d > 0 && axisMx.IncludeOrigin) d = 0;
					minValue = d;
				}
				else minValue = double.Parse(axisMx.RangeMin);

				if (Lex.IsNullOrEmpty(axisMx.RangeMax) || !Lex.IsDouble(axisMx.RangeMax))
				{ // calculate if not specified
					d = stats.MaxValue.NumericValue;
					if (d < 0 && axisMx.IncludeOrigin) d = 0;
					maxValue = d;
				}
				else maxValue = double.Parse(axisMx.RangeMax);
			}

			else if (mc.DataType == MetaColumnType.Date) // date
			{
				minValue = stats.MinValue.DateTimeValue;
				maxValue = stats.MaxValue.DateTimeValue;
			}

			else // categorical, number from zero to element count - 1
			{
				minValue = 0;
				int mv = stats.DistinctValueList.Count - 1;
				if (mv < 0) mv = 0;
				maxValue = mv;
			}

			return;
		}

		/// <summary>
		/// Get the scale type to be applied to an axis.
		/// For qualitative axes this will be numerical.
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>

		internal ScaleType GetScaleType(MetaColumn mc)
		{
			if (mc.IsNumeric) return ScaleType.Numerical;
			else if (mc.DataType == MetaColumnType.Date) return ScaleType.DateTime;
			else return ScaleType.Numerical; // qualitative
		}

		/// <summary>
		/// Calculate & store marker sizes
		/// </summary>

		internal void BuildMarkerSizeDimension()
		{
			MetaColumn mc = null;
			ColumnInfo colInfo = null;
			ColumnStatistics stats = null; // stats for size QueryColumn
			SeriesPoint p;
			SeriesPointTag tag;
			int voi = -1;

// Calculate min & max marker size in x axis units
// Note: Axes and their ranges must already be set up since they are used in size calc

			if (XYDiagram == null || AxisX == null) return; // can't do if can't get x axis range
			WholeRange ar = AxisX.WholeRange;
			AxisMx axisMx = XAxisMx;

			float asixMinInternal = (float)ar.MinValueInternal;
			if (axisMx.IncludeOrigin && asixMinInternal > 0) asixMinInternal = 0;

			float axisMaxInternal = (float)ar.MaxValueInternal;
			if (axisMx.IncludeOrigin && axisMaxInternal < 0) axisMaxInternal = 0;

			double axisRange = Math.Abs(axisMaxInternal - asixMinInternal);

			double markerSizeFraction = MarkerSize.FixedSize / 100.0; // fraction of max marker size to use size

			double axisMinMarkerSize = // min size in xaxis coords
				axisRange * 0.01 * markerSizeFraction; // range * min size fraction * overall % markersize parameter 

			double axisMaxMarkerSize = 
				axisRange * 0.05 * markerSizeFraction; // range * max size fraction * overall % markersize parameter 

			double axisMarkerSizeRange = axisMaxMarkerSize - axisMinMarkerSize;

			if (Series.View is BubbleSeriesView)
			{ // set min & max bubble size to avoid clipping of size
				BubbleSeriesView v = BubbleView;
				if (axisMinMarkerSize >= v.MaxSize) // avoid setting minimum greater than max
				{ v.MaxSize = axisMaxMarkerSize; v.MinSize = axisMinMarkerSize; }
				else // or max less than minimum
				{ v.MinSize = axisMinMarkerSize; v.MaxSize = axisMaxMarkerSize; }
			}

			else throw new ArgumentException();

			QueryColumn qc = MarkerSize.QueryColumn;
			if (qc != null)
			{
				mc = qc.MetaColumn;
				voi = qc.VoPosition;
				stats = GetStats(qc);
				if (stats.DistinctValueList.Count == 0) stats = null;

				// Numeric column

				if (mc.IsNumeric && mc.DataType != MetaColumnType.CompoundId)
				{
					double minVal = (double)stats.MinValue.ToPrimitiveType();
					double maxVal = (double)stats.MaxValue.ToPrimitiveType();
					double valRange = maxVal - minVal;

					for (int ri = 0; ri < Dt.Rows.Count; ri++)
					{
						object o = GetVo(ri, qc, voi, false, false);
						p = SeriesPoints[ri];
						if (p == null) continue;
						tag = (SeriesPointTag)p.Tag;
						if (!NullValue.IsNull(o))
						{
							MobiusDataType mdt = o as MobiusDataType;
							double d = (double)mdt.ToPrimitiveType();
							double relsize = (d - minVal) / valRange;
							double axisSize = (float)(axisMinMarkerSize + relsize * axisMarkerSizeRange); // size in axis coords
							if (p.Values.Length >= 2) p.Values[1] = axisSize;
						}
					}
				}

	// Date column

				else if (mc.DataType == MetaColumnType.Date)
				{
					DateTime minVal = (DateTime)stats.MinValue.ToPrimitiveType();
					DateTime maxVal = (DateTime)stats.MaxValue.ToPrimitiveType();
					double valRange = maxVal.Subtract(minVal).TotalDays;

					for (int ri = 0; ri < Dt.Rows.Count; ri++)
					{
						object o = GetVo(ri, qc, voi, false, false);
						p = SeriesPoints[ri];
						if (p == null) continue;
						tag = (SeriesPointTag)p.Tag;

						if (NullValue.IsNull(o))
						{
							MobiusDataType mdt = o as MobiusDataType;
							DateTime d = (DateTime)mdt.ToPrimitiveType();
							double relsize = d.Subtract(minVal).TotalDays / valRange;
							double axisSize = (float)(axisMinMarkerSize + relsize * axisMarkerSizeRange); // size in axis coords
							if (p.Values.Length >= 2) p.Values[1] = axisSize;
						}
					}
				}

	// Categorical value column

				else // categorical string data
				{
					double minVal = 0;
					double maxVal = stats.DistinctValueList.Count - 1;
					double valRange = maxVal - minVal;

					for (int ri = 0; ri < Dt.Rows.Count; ri++)
					{
						object o = GetVo(ri, qc, voi, false, false);
						p = SeriesPoints[ri];
						if (p == null) continue;
						tag = (SeriesPointTag)p.Tag;

						if (!NullValue.IsNull(o))
						{
							MobiusDataType mdt = o as MobiusDataType;
							string txt = GetFormattedText(qc, mdt);
							int ordinal = stats.DistinctValueDict[txt.ToUpper()].Ordinal;
							double relsize = (ordinal - minVal) / valRange;
							double axisSize = (float)(axisMinMarkerSize + relsize * axisMarkerSizeRange); // size in axis coords
							if (p.Values.Length >= 2) p.Values[1] = axisSize;
						}
					}
				}
			}

			else // constant size
			{
				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					p = SeriesPoints[ri];
					if (p == null) continue;
					tag = (SeriesPointTag)p.Tag;
					double size = axisMaxMarkerSize * markerSizeFraction;
					if (p.Values.Length >= 2) p.Values[1] = size;
				}
			}

			return;
		}

		/// <summary>
		/// Update the marker shape dimension
		/// </summary>

		internal void BuildMarkerShapeDimension()
		{
			MobiusDataType mdt;
			SeriesPoint p;
			SeriesPointTag tag;
			string key, key2;

			DateTime t0 = DateTime.Now;

			if (Series.View is BubbleSeriesView)
			{
				BubbleSeriesView v = BubbleView;
				MarkerBase m = v.BubbleMarkerOptions;
				m.Kind = (MarkerKind)MarkerShape.FixedShape;
				m.StarPointCount = 5; // default star size
			}

			else throw new InvalidCastException();

			QueryColumn qc = MarkerShape.QueryColumn;
			if (qc != null)
			{
				MetaColumn mc = qc.MetaColumn;
				ColumnInfo colInfo = ResultsFormat.GetColumnInfo(qc);
				int voi = colInfo.DataColIndex;
				Dictionary<string, MarkerKind> valueMap = new Dictionary<string, MarkerKind>();
				if (MarkerShape.Rules != null)
				{
					foreach (CondFormatRule r in MarkerShape.Rules)
					{
						int markerShapeIndex = r.ForeColor.ToArgb();
						if (markerShapeIndex >= 0)
							valueMap[r.Value.ToUpper()] = // assoc value to PointShape
								(MarkerKind)markerShapeIndex;
					}
				}

				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					p = SeriesPoints[ri];
					if (p == null) continue;
					tag = (SeriesPointTag)p.Tag;
					if (tag == null) continue;

					object o = GetVo(ri, qc, voi, false, false);

					if (NullValue.IsNull(o))
					{
						key = "(Blank)".ToUpper();
						if (valueMap.ContainsKey(key))
							tag.MarkerKind = valueMap[key];
						else tag.MarkerKind = MarkerKind.Square; // acts as null value for now
					}

					else
					{
						mdt = MobiusDataType.ConvertToMobiusDataType(mc.DataType, o);
						key = GetFormattedText(qc, mdt).ToUpper();
						key2 = "(Other)".ToUpper();

						if (valueMap.ContainsKey(key)) tag.MarkerKind = (MarkerKind)valueMap[key];
						else if (valueMap.ContainsKey(key2)) tag.MarkerKind = (MarkerKind)valueMap[key2];
						else tag.MarkerKind = MarkerKind.Square; // acts as null value for now
					}

				}
			}

			else // constant shape
			{
				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					p = SeriesPoints[ri];
					if (p == null) continue;
					tag = (SeriesPointTag)p.Tag;
					if (tag == null) continue;

					tag.MarkerKind = (MarkerKind)(int)MarkerShape.FixedShape;
				}
			}

			double ms = TimeOfDay.Delta(t0);
			return;
		}

		/// <summary>
		/// Update the DataTable for the marker color dimension
		/// </summary>

		internal void BuildMarkerColorDimension()
		{
			SeriesPoint p;
			SeriesPointTag tag;
			int missingCount = 0;

			DateTime t0 = DateTime.Now;

			QueryColumn qc = MarkerColor.QueryColumn;
			Color fixedColor = MarkerColor.FixedColor;

			if (Series.View is BubbleSeriesView)
			{
				BubbleSeriesView v = BubbleView;
				v.Color = MarkerColor.FixedColor;
			}

			else throw new InvalidCastException();

			if (qc != null)
			{
				MetaColumn mc = qc.MetaColumn;
				ColumnInfo colInfo = ResultsFormat.GetColumnInfo(qc);
				int voi = colInfo.DataColIndex;

				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					p = SeriesPoints[ri];
					if (p == null) continue;
					tag = (SeriesPointTag)p.Tag;
					if (tag == null) continue;

					object o = GetVo(ri, qc, voi, false, false);

					if (o == null || o is DBNull)
					{
						tag.Color = CondFormatMatcher.DefaultMissingValueColor;
						missingCount++;
					}

					else
					{
						MobiusDataType mdt = o as MobiusDataType;
						if (mdt == null || mdt.BackColor == Color.Empty)
						{ // if not back color then get cell style
							CellStyleMx style = // get the back color
								ResultsFormatter.GetCondFormatCellStyle(qc, o, Qm.ResultsFormat);

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
							tag.Color = mdt.BackColor;
							if (mdt.BackColor == CondFormatMatcher.DefaultMissingValueColor ||
							 NullValue.IsNull(mdt))
								missingCount++;
						}
						else tag.Color = CondFormatMatcher.DefaultMissingValueColor;
					}
				}
			}

			else // constant color
			{
				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					p = SeriesPoints[ri];
					if (p == null) continue;
					tag = (SeriesPointTag)p.Tag;
					if (tag == null) continue;

					tag.Color = MarkerColor.FixedColor;
				}
			}

			double ms = TimeOfDay.Delta(t0);
			return;
		}

		/// <summary>
		/// Setup the label column series
		/// </summary>

		internal void BuildMarkerLabelDimension()
		{
			SeriesPoint p;
			SeriesPointTag tag;
			MobiusDataType mdt;

			DateTime t0 = DateTime.Now;

			QueryColumn qc = MarkerLabel.QueryColumn;
			if (qc == null) return;

			MetaColumn mc = qc.MetaColumn;
			ColumnInfo colInfo = ResultsFormat.GetColumnInfo(qc);
			int voi = colInfo.DataColIndex;

			if (MarkerLabel.QueryColumn != null)
			{
				for (int ri = 0; ri < Dt.Rows.Count; ri++)
				{
					p = SeriesPoints[ri];
					if (p == null) continue;
					tag = (SeriesPointTag)p.Tag;
					if (tag == null) continue;

					object o = GetVo(ri, qc, voi, false, false);

					if (o == null || o is DBNull) tag.Label = null;
					else
					{
						bool isMdt = MobiusDataType.IsMobiusDataType(o);
						if (isMdt) mdt = o as MobiusDataType;
						else mdt = MobiusDataType.ConvertToMobiusDataType(qc.MetaColumn.DataType, o);
						string txt = GetFormattedText(qc, mdt);
						if (!isMdt) { } // todo: store back in source datatable?
						tag.Label = txt;
					}
				}
			}

			if (BubbleView != null)
			{
				if (MarkerLabel.QueryColumn != null)
					Series.LabelsVisibility = DefaultBoolean.True;
				else Series.LabelsVisibility = DefaultBoolean.False;

				BubbleSeriesLabel bsl = Series.Label as BubbleSeriesLabel;
                bsl.Position = // set label position
                    (MarkerLabel.Position == LabelPositionEnum.Center) ? PointLabelPosition.Center : PointLabelPosition.Outside;
                //(MarkerLabel.Position == LabelPositionEnum.Center) ? BubbleLabelPosition.Center : BubbleLabelPosition.Outside; // DevExpress 15.2.7 Upgrade
            }

            else throw new ArgumentException();

			double ms = TimeOfDay.Delta(t0);
			return;
		}

		/// <summary>
		/// Setup the chart background image
		/// </summary>

		internal void SetupBackgroundImage()
		{
			XYDiagramDefaultPane pane = XYDiagram.DefaultPane;

			try
			{
				if (Lex.Ne(BackgroundImageFile, CurrentBackgroundImageFile)) // have ref to current image?
				{
					if (!Lex.IsNullOrEmpty(BackgroundImageFile)) // file defined?
					{
						string serverImagesFolder = @"<TargetMaps>";
						if (Lex.StartsWith(BackgroundImageFile, serverImagesFolder))
						{
							string fileName = BackgroundImageFile.Substring(serverImagesFolder.Length + 1);

							string serverFileName = BackgroundImageFile; // ref to server file name
							CurrentBackgroundImageFile = serverFileName; // save that as current

							string clientFileName = ClientDirs.CacheDir + @"\" + fileName; // actual file name
							CachedBackgroundImageFile = clientFileName;

							ServerFile.GetIfChanged(serverFileName, clientFileName); // get file from server if client file doesn't exist or is older
							if (!File.Exists(clientFileName))
								CachedBackgroundImageFile = "";
						}

						else // all the same file if local file
							CurrentBackgroundImageFile = CachedBackgroundImageFile = BackgroundImageFile;
					}

					else CurrentBackgroundImageFile = CachedBackgroundImageFile = BackgroundImageFile; // no background image

					if (!Lex.IsNullOrEmpty(CachedBackgroundImageFile)) // if defined read it in
						BackgroundImage = (Bitmap)Bitmap.FromFile(CachedBackgroundImageFile);

					else BackgroundImage = null; // no defined

				}

				//if (ChartControl != null)
				//    ChartControl.RefreshData(); // redraw the chart control

				Refresh(); // redraw with new background
				//AdjustBackgroundImage();
				return;
			}
			catch (Exception ex) { ex = ex; }
		}

		/// <summary>
		/// Render background image within custom paint event
		/// Note: Scrolling range stays fixed and visible range changes with scrolling/zooming
		/// </summary>

		internal void RenderBackgroundImage(
			CustomPaintEventArgs e)
		{
			XYDiagramDefaultPane p = XYDiagram.DefaultPane;

			if (BackgroundImage == null) return;

			DateTime t0 = DateTime.Now;

			Graphics g = e.Graphics;
			Rectangle drt = DiagramToPointHelper.CalculateDiagramBounds(XYDiagram);

			WholeRange xr = AxisX.WholeRange;
			WholeRange yr = AxisY.WholeRange;

			Bitmap b = BackgroundImage;

			double vw = xr.MaxValueInternal - xr.MinValueInternal; // visible width
			if (vw <= 0) vw = 1;
			VisualRange vr = AxisX.VisualRange;
			double sw = vr.MaxValueInternal - vr.MinValueInternal; // scrolling width
			int x = (int)((xr.MinValueInternal - vr.MinValueInternal) / sw * b.Width);
			int width = (int)(vw / sw * b.Width);
			if (width <= 0) width = 1;

			double vh = yr.MaxValueInternal - yr.MinValueInternal; // visible height
			if (vh <= 0) vh = 1;
			vr = AxisY.VisualRange;
			double sh = vr.MaxValueInternal - vr.MinValueInternal; // scrolling height
			int y = (int)((vr.MaxValueInternal - yr.MaxValueInternal) / sh * b.Height);
			//int y = (int)((sr.MinValueInternal - yr.MinValueInternal) / vh * b.Height);
			int height = (int)(vh / sh * b.Height);
			if (height <= 0) height = 1;

			//Rectangle section = new Rectangle(x, y, width, height);
			//Bitmap i2 = CopyBitmapRectangle(BackgroundImage, section);

			g.DrawImage(BackgroundImage, drt, x, y, width, height, GraphicsUnit.Pixel);

			//i2.Save(@"c:\download\section.bmp");
			//p.BackImage.Image = i2;
			//p.BackImage.Stretch = true;
			double ms = TimeOfDay.Delta(t0);
			return;
		}

/// <summary>
/// Adjust any background image for scrolling/zooming changes
/// Note: Scrolling range stays fixed and visible range changes with scrolling/zooming
/// </summary>

		internal void AdjustBackgroundImage()
		{
			if (Math.Abs(1) == 1) return; // background image now drawn in custom paint operation

			XYDiagramDefaultPane p = XYDiagram.DefaultPane;

			if (BackgroundImage == null)
			{ // just clear any image
				if (p.BackImage != null && p.BackImage.Image != null) p.BackImage.Image = null;
				return;
			}

			WholeRange xr = AxisX.WholeRange;
			WholeRange yr = AxisY.WholeRange;

			if (xr.MinValueInternal == AxisX.VisualRange.MinValueInternal &&
				xr.MaxValueInternal == AxisX.VisualRange.MaxValueInternal)
			{ // full image
				p.BackImage.Image = BackgroundImage;
				p.BackImage.Stretch = true;
				return;
			}

			Bitmap b = BackgroundImage;

			double vw = xr.MaxValueInternal - xr.MinValueInternal; // visible width
			if (vw <= 0) vw = 1;
			VisualRange vr = AxisX.VisualRange;
			double sw = vr.MaxValueInternal - vr.MinValueInternal; // scrolling width
			int x = (int)((xr.MinValueInternal - vr.MinValueInternal) / sw * b.Width);
			int width = (int)(vw / sw * b.Width);
			if (width <= 0) width = 1;

			double vh = yr.MaxValueInternal - yr.MinValueInternal; // visible height
			if (vh <= 0) vh = 1;
			vr = AxisY.VisualRange;
			double sh = vr.MaxValueInternal - vr.MinValueInternal; // scrolling height
			int y = (int)((vr.MaxValueInternal - yr.MaxValueInternal) / sh * b.Height);
			//int y = (int)((sr.MinValueInternal - yr.MinValueInternal) / vh * b.Height);
			int height = (int)(vh / sh * b.Height);
			if (height <= 0) height = 1;

			Rectangle section = new Rectangle(x, y, width, height);
			Bitmap i2 = CopyBitmapRectangle(BackgroundImage, section);
			//i2.Save(@"c:\download\section.bmp");
			DateTime t0 = DateTime.Now;
			//p.BackImage.Image = i2;
			//p.BackImage.Stretch = true;
			double ms = TimeOfDay.Delta(t0);
			return;
		}


		/// <summary>
		/// Copy a rectangular area from a bitmap into a new bitmap
		/// </summary>
		/// <param name="srcBitmap"></param>
		/// <param name="section"></param>
		/// <returns></returns>

		static public Bitmap CopyBitmapRectangle(Bitmap srcBitmap, Rectangle rect)
		{
			Bitmap bmp = srcBitmap.Clone(rect, srcBitmap.PixelFormat);
			return bmp;
		}

/// <summary>
/// Setup the list of items to be included in the legend
/// </summary>

		internal void SetupLegend()
		{
			CondFormat cf;
			CondFormatRules rules;
			CondFormatRule rule;
			LegendItem i;
			string txt;
			int ri;

			Legend l = ChartControl.Legend;
            
            if (!MainViewPanelMaximized && ShowLegend)
            {
                l.Visibility = DefaultBoolean.True;
            }
            else
            {
                l.Visibility = DefaultBoolean.False;
            }
            //l.Visible = !MainViewPanelMaximized && ShowLegend; DevExpress 15.2.7 Upgrade

            l.AlignmentHorizontal = LegendAlignmentHorizontal;
			l.AlignmentVertical = LegendAlignmentVertical;
			l.MaxHorizontalPercentage = LegendMaxHorizontalPercentage;
			l.MaxVerticalPercentage = LegendMaxVerticalPercentage;
			l.Direction = LegendItemOrder;

			LegendItems = new List<LegendItem>();

			// Marker color legend items

			if (MarkerColor.QueryColumn != null && MarkerColor.QueryColumn.CondFormat != null)
			{
				i = AddLegendItem(new LegendItem("--- Color by: ---"));
				i = AddLegendItem(new LegendItem(MarkerColor.QueryColumn.ActiveLabel));
				cf = MarkerColor.QueryColumn.CondFormat;

				for (ri = 0; ri < cf.Rules.Count; ri++)
				{
					rule = cf.Rules[ri];
					i = AddLegendItem(new LegendItem(MarkerKind.Square, rule.BackColor1, rule.Name));
					if (Lex.IsNullOrEmpty(rule.Name)) // if no name build text description
						i.Text = rule.ToString(MarkerColor.QueryColumn.MetaColumn, ri);
				}
			} 

			// Marker shape legend items

			MarkerKind sizeMarkerKind = MarkerKind.Square;
			if (MarkerShape.QueryColumn != null && MarkerShape.Rules != null)
			{
				i = AddLegendItem(new LegendItem("--- Shape by: ---"));
				i = AddLegendItem(new LegendItem(MarkerShape.QueryColumn.ActiveLabel));
				rules = MarkerShape.Rules;
				for (ri = 0; ri < rules.Count; ri++)
				{
					rule = rules[ri];
					i = AddLegendItem(new LegendItem(rule.Name));
					if (Lex.IsNullOrEmpty(rule.Name)) // if no name build text description
						i.Text = rule.ToString(MarkerShape.QueryColumn.MetaColumn, ri);

					int markerKindIndex = rule.ForeColor.ToArgb();
					i.MarkerKind = (MarkerKind)markerKindIndex;
					if (ri == 0) sizeMarkerKind = i.MarkerKind;
					i.ShowMarker = true;
					i.Color = CondFormatMatcher.DefaultMissingValueColor;
				}
			}

			// Marker size legend items

			if (MarkerSize.QueryColumn != null)
			{
				i = AddLegendItem(new LegendItem("--- Size by: ---"));
				i = AddLegendItem(new LegendItem(MarkerSize.QueryColumn.ActiveLabel));
				ColumnStatistics stats = GetStats(MarkerSize.QueryColumn);
				if (stats != null && stats.DistinctValueList.Count > 0)
				{
					List<MobiusDataType> dvl = stats.DistinctValueList;
					txt = "<= " + dvl[dvl.Count-1].FormattedText;
					i = AddLegendItem(new LegendItem(sizeMarkerKind, CondFormatMatcher.DefaultMissingValueColor, txt));

					txt = ">= " + dvl[0].FormattedText;
					i = AddLegendItem(new LegendItem(sizeMarkerKind, CondFormatMatcher.DefaultMissingValueColor, txt));
					i.MarkerSize = 8; // smaller marker
				}
			}

			return;
		}

/// <summary>
/// AddLegendItem
/// </summary>
/// <param name="i"></param>
/// <returns></returns>

		LegendItem AddLegendItem(LegendItem i)
		{
			LegendItems.Add(i);
			return i;
		}

/// <summary>
/// Serialize to a string
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
			SerializeChartProperties(tw);
			return;
		}

		/// <summary>
		/// EndSerialization
		/// </summary>
		/// <param name="tw"></param>

		public override void EndSerialization(XmlTextWriter tw)
		{
			base.EndSerialization(tw);
			return;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>

		public new void FreeResources()
		{
			RenderingControl = null;
			ChartPage = null;
			return;
		}

	}

/// <summary>
/// Item contained in Legend
/// </summary>

	public class LegendItem
	{
		public Color Color;
		public MarkerKind MarkerKind;
		public int MarkerSize = -1;
		public bool ShowMarker = false;
		public string Text;

		public LegendItem()
		{
			return;
		}

		public LegendItem(string text)
		{
			Text = text;
			return;
		}

		public LegendItem(
			MarkerKind markerKind,
			Color color,
			string text)
		{
			MarkerKind = markerKind;
			ShowMarker = true;
			Color = color;
			Text = text;
			return;
		}

	}

/// <summary>
/// Additional series point properties stored in the SeriesPoint tag
/// </summary>

	public class SeriesPointTag
	{
		public int DataRowIndex; // index of DataRow in source table
		public MarkerKind MarkerKind; // marker shape
		public Color Color; // point color
		public string Label; // point label
	}

}