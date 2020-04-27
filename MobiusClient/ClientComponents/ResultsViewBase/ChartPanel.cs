using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraCharts;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Panel that contains a single chart and it's legend
	/// The chart may be an Chart or a TrellisPanel
	/// </summary>

	public partial class ChartPanel : DevExpress.XtraEditors.XtraUserControl
	{

		public ChartPanel()
	{
		InitializeComponent();

		if (!SystemUtil.InDesignMode) // fill panel with the chart control initially
		{
			TrellisPanel.ChartPanel = this;
			TrellisPanel.Visible = false; // hide trellis panel control

			//InitChartControl(ChartControl); // init chart control
		}

		return;
	}

#if false
		//internal ChartPanel ChartPanel; // panel that this trellis is a part of
		internal ChartPagePanel ChartPagePanel; // ChartPagePanel that this ChartPanel is a part of
		internal ChartViewMgr ChartView { get { return ChartPagePanel.ChartView; } }

		internal SeriesPoint CurrentSeriesPoint; // point currently selected with mouse
		internal bool InSetup = false;

		public static int PaintCount = 0; // number of times paint called
		public static int PaintCountSp = 0; // number of times paint called

/// <summary>
/// Constructor
/// </summary>


/// <summary>
/// InitChartControl
/// </summary>
/// <param name="cc"></param>

		void InitChartControl(ChartControl cc)
		{
			cc.Visible = false; // hide the chart control
			cc.Dock = DockStyle.Fill;
			cc.Series.Clear();
			//cc.RuntimeSelection = false; // do better selection in Mobius code
            cc.SelectionMode = ElementSelectionMode.None;
			cc.RuntimeHitTesting = true;

			//cc.RuntimeHitTesting = false; // try for faster performance (not helpful)
			//cc.CacheToMemory = true;
			//cc.RefreshDataOnRepaint = false;

			UseMouseForScrolling = false; // disable mouse for scrolling since we want to use it for selection
		}

/// <summary>
/// Set flag to use mouse for scrolling or selection
/// </summary>

		public bool UseMouseForScrolling
		{
			get
			{
				XYDiagram2D dgm = ChartControl.Diagram as XYDiagram2D;
				if (dgm == null) return true;

				return dgm.ScrollingOptions.UseMouse;
			}

			set
			{
				XYDiagram2D dgm = ChartControl.Diagram as XYDiagram2D;
				if (dgm == null) return;

				//if (value) dgm = dgm;
				dgm.ScrollingOptions.UseMouse = value;
			}
		}

/// <summary>
/// Setup the chart properties menu upon open
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ChartPropertiesContextMenu_Opening(object sender, CancelEventArgs e)
		{
			ShowFilterPanelMenuItem.Checked = ChartView.ResultsPage.ShowFilterPanel;
			ShowSelectedDataRowsPanelMenuItem.Checked = ChartView.ResultsPage.ShowDetailsOnDemand;

			ShowLegendMenuItem.Checked = ChartView.ShowLegend;
			ShowAxisTitlesMenuItem.Checked = ChartView.ShowAxesTitles;
			ShowAxisScaleLabelsMenuItem.Checked = ChartView.ShowAxesScaleLabels;

			//bool showTrellisItems = ChartView.IsTrellis;
			//ShowAxisTitlesMenuItem.Visible = showTrellisItems;
			//ShowAxisScaleLabelsMenuItem.Visible = showTrellisItems;
			//TrellisDividerMenuItem.Visible = showTrellisItems;

			//bool enableZoomItems = ChartView.XAxis.ShowZoomSlider || ChartView.YAxis.ShowZoomSlider;
			//ZoomMenuItem.Enabled = enableZoomItems;
			//ResetZoomMenuItem.Enabled = enableZoomItems;

			return;
		}

/// <summary>
/// Show the point properties menu
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		internal void ShowPointPropertiesContextMenu(
			SeriesPoint point,
			MouseEventArgs e)
		{
			CurrentSeriesPoint = point;

			int dri = GetDataRowIndex(CurrentSeriesPoint);
			if (dri < 0) return;

			bool selected = ChartView.Dtm.RowIsSelected(dri);
			SelectPointMenuItem.Checked = selected;
			SelectPointMenuItem.Text = selected ? "Deselect" : "Select";

			bool marked = ChartView.Dtm.RowIsMarked(dri);
			MarkPointMenuItem.Checked = marked;
			MarkPointMenuItem.Text = marked ? "Unmark" : "Mark";

			PointPropertiesContextMenu.Show(this, new Point(e.X, e.Y));
		}

		private void SelectPointMenuItem_Click(object sender, EventArgs e)
		{
			int dri = GetDataRowIndex(CurrentSeriesPoint);
			if (dri < 0) return;

			bool selected = ChartView.Dtm.RowIsSelected(dri);
			ChartView.Dtm.SelectRow(dri, !selected);

			ChartView.ResultsPagePanel.RefreshDetailsOnDemand();
			return;
		}

		private void MarkPointMenuItem_Click(object sender, EventArgs e)
		{
			int dri = GetDataRowIndex(CurrentSeriesPoint);
			if (dri < 0) return;

			bool marked = ChartView.Dtm.RowIsMarked(dri);
			ChartView.Dtm.SetRowMark(dri, !marked);
			return;
		}

		/// <summary>
		/// Get the index of a data from from the corresponding SeriesPoint
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>

		internal int GetDataRowIndex(SeriesPoint point)
		{
			if (point.Tag == null) return -1;
			SeriesPointTag tag = (SeriesPointTag)point.Tag;
			int dri = tag.DataRowIndex;
			return dri;
		}

/// <summary>
/// Show/hide legend
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		internal void ShowLegendMenuItem_Click(object sender, EventArgs e)
		{
			ChartViewMgr view = ChartPagePanel.ChartView;
			view.ShowLegend = !view.ShowLegend;

			SetLegendVisibility();
			return;
		}

		/// <summary>
		/// Show or hide the legend based on current visibility setting
		/// </summary>

		internal void SetLegendVisibility()
		{
			bool visible = !ChartView.MainViewPanelMaximized && ChartView.ShowLegend; // change value, causes redraw

		    if (visible) // setup to be sure properly configured
		    {
		        ChartView.SetupLegend();
                ChartControl.Legend.Visibility = DefaultBoolean.True;

            }
		    else
		    {
                ChartControl.Legend.Visibility = DefaultBoolean.False;
            }
		    //ChartControl.Legend.Visible = visible;  DevExpress 15.2.7 Upgrade
            
			return;
		}

/// <summary>
/// Show axis titles
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		internal void ShowAxisTitlesMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.ShowAxesTitles = !ChartView.ShowAxesTitles;
			ChartView.ConfigureRenderingControl();
		}

/// <summary>
/// Show axis scale labels
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		internal void ShowAxisScaleLabelsMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.XAxisMx.ShowLabels = ChartView.YAxisMx.ShowLabels =
			 ChartView.ZAxisMx.ShowLabels = ChartView.ShowAxesScaleLabels =
				!ChartView.ShowAxesScaleLabels;

			ChartView.ConfigureRenderingControl();
		}

		internal void ZoomMenuItem_Click(object sender, EventArgs e)
		{
			ActivateZooming();

			string msg =
@"To zoom in:
  - Hold down the Shift key and then click on the point to zoom in to or
  - Hold down the Shift key and then draw a rectangle around the 
    area to zoom in to or
  - Hold down the Ctrl key and click the plus (+) key or
  - Use the mouse wheel

To zoom out:
  - Hold down the Alt key and then click in the region to be zoomed out or
  - Hold down the Ctrl key and click the minus (-) key or
  - Use the mouse wheel

To scroll:
  - Use the scroll bars or
  - Hold down the Space key and then press and drag with the mouse or
  - Hold down the Ctrl key and then use the cursor (arrow) keys";

			MessageBoxMx.Show(msg);
			return;
		}

		internal void ResetZoomMenuItem_Click(object sender, EventArgs e)
		{
			ActivateZooming();
			ChartView.ScaleView(100);
		}

/// <summary>
/// Activate zooming if not yet active
/// </summary>

		void ActivateZooming()
		{
			if (!ChartView.XAxisMx.ShowZoomSlider || !ChartView.YAxisMx.ShowZoomSlider)
			{ // turn on zoom sliders if not done
				ChartView.XAxisMx.ShowZoomSlider = true;
				ChartView.YAxisMx.ShowZoomSlider = true;
				ChartView.ConfigureRenderingControl();
			}
		}

		/// <summary>
		/// Show the chart properties menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		internal void PropertiesMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dr = ChartPropertiesDialog.ShowDialog(ChartView);
		}

		private void HideLegendMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.ShowLegend = false;
			ChartView.ConfigureRenderingControl();
		}

		private void LegendPropertiesMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult dr = ChartPropertiesDialog.ShowDialog(ChartView, "Legend");
		}

		private void ChartControl_Paint(object sender, PaintEventArgs e)
		{
			PaintCount++;
			return;
		}

		private void ChartControl_CustomPaint(object sender, CustomPaintEventArgs e)
		{
			return;
		}

		private void SelectAllMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.ResultsPagePanel.SelectAll(true);
		}

		private void ShowSelectedDataRowsPanelMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.Qrc.ShowSelectedDataRowsPanelMenuItem_Click(sender, e);
		}

		private void ShowFilterPanelMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.Qrc.ShowFilterPanelMenuItem_Click(sender, e);
		}

		private void CloseMenuItem_Click(object sender, EventArgs e)
		{
			ChartView.ResultsPagePanel.CloseView(ChartView);
		}

#endif
	}
}
