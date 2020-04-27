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

namespace Mobius.ClientComponents
{
	public partial class TrellisPanel : DevExpress.XtraEditors.XtraUserControl
	{
		internal ChartPanel ChartPanel; // panel that this trellis is a part of
		internal ChartPagePanel ChartPagePanel { get { return ChartPanel.ChartPagePanel; }}
		internal ChartViewMgr ChartView { get { return ChartPanel.ChartView; }}
		internal QueryColumn TrellisPageQc { get { return ChartView.TrellisPageQc; }}
		internal int TrellisPageCount { get { return ChartView.TrellisPageCount; } }
		internal int TrellisPage { get { return ChartView.TrellisPageIndex; } }
		internal DataTableManager Dtm { get { return ChartView.Dtm; } }
		internal ContextMenuStrip ChartPropertiesContextMenu { get { return ChartPagePanel.ChartPageControl != null ? ChartPagePanel.ChartPageControl.MouseModeContextMenu : null; } } // todo: fixup

		bool Scrolling; // currently scrolling

		public TrellisPanel()
		{
			InitializeComponent();

			//Dock = DockStyle.Fill; // default to fill
			Grid.RowCount = 0; // start with empty grid
			Grid.ColumnCount = 0;
		}

/// <summary>
/// Setup the scrollbar for the current trellis paging values
/// </summary>

		internal void SetupScrollBar()
		{
			if (ChartPanel == null || ChartPanel.ChartPagePanel == null) return;

			DevExpress.XtraEditors.VScrollBar sb = ScrollBar;

			//if (ChartEx.TrellisByRowCol)
			//{
				if (TrellisPageCount <= 1)
				{
					sb.Visible = false;
					return;
				}

				if (!sb.Visible)
				{
					sb.Location = new Point(this.Width - (sb.Width + 1), 36); // adjust size & location
					sb.Height = this.Height - (sb.Top + 1);
					sb.Visible = true;
				}

				bool saveIs = ChartPagePanel.Insetup;
				ChartPagePanel.Insetup = true;
				sb.Minimum = 0;
				sb.Maximum = TrellisPageCount - 1;
				sb.SmallChange = 1;
				sb.LargeChange = 1;

				sb.Value = TrellisPage;
				ChartPagePanel.Insetup = saveIs;

				sb.Refresh();
			//}

			//else // trellis by flow
			//{
			//  // todo
			//}

			return;
		}

/// <summary>
/// Adjust the size of the panel so the legend displays
/// </summary>
		internal void AdjustPanelToShowLegend()
		{
			////if (!ChartEx.IsTrellis) return;

			////if (!ChartEx.ShowLegend || ChartEx.LegendsPanel == null || ChartEx.LegendsPanel.ChildPanels.Count == 0)
			////{
			////  this.Dock = DockStyle.Fill; // fill parent panel with TrellisPanel
			////}

			////else
			////{
			////  Size pSize = ChartPanel.Size; // size of parent panel
			////  NRectangleF lArea = ChartEx.LegendsPanel.ContentArea; // legend size in pixels
			////  this.Dock = DockStyle.None;
			////  this.Location = new Point(1, 1);
			////  this.Size = new Size(pSize.Width - (int)lArea.Width, pSize.Height - 2);
			////}

			return;
		}

		/// <summary>
		/// Scroll to new trellis page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			if (ChartPagePanel.Insetup) return;
			int newPage = e.NewValue;
			if (ChartView.TrellisPageIndex == newPage) return;
			if (Scrolling) return;
			Scrolling = true;

			ChartView.TrellisPageIndex = newPage;
			//ClientLog.Message("ScrollBar.Value = " + e.NewValue + ", TrellisPage = " + View.TrellisPage);
			ChartView.ConfigureRenderingControl();
			Scrolling = false;
			return;
		}

/// <summary>
/// Handle rt-clicks on trellis
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			TrellisPanel_MouseDown(sender, e);
		}

/// <summary>
/// Show chart properties context menu 
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void TrellisPanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && // show chart properties context menu on right-button click
			 ChartPropertiesContextMenu != null)
				ChartPropertiesContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
		}

	}
}
