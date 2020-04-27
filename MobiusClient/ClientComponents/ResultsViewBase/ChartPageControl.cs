using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraTab;
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
	public partial class ChartPageControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal ChartViewMgr View { get { return ChartPagePanel.ChartView; }} // currently active chart
		internal QueryManager Qm { get { return ChartPagePanel.ChartView.Qm; }} // QueryManager associated with currently active chart on the page

		public ChartPageControl()
		{
			InitializeComponent();

			Controls.Remove(ToolPanel);

			ChartPagePanel.ChartPageControl = this; // link ChartPagePanel up to us

			if (!SystemUtil.InDesignMode) // fill panel with the chart control initially
			{
				ChartPagePanel.Dock = DockStyle.Fill;
			}
		}

		public void Initialize()
		{
			ChartPagePanel.Initialize();
		}

/// <summary>
/// Edit chart properties
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ChartPropertiesButton_Click(object sender, EventArgs e)
		{
			DialogResult dr = View.ShowPropertiesDialog();
			return;
		}

		private void PrintBut_Click(object sender, EventArgs e)
		{
			PrintChart();
		}

/// <summary>
/// Print the current chart
/// </summary>

		public DialogResult PrintChart()
		{
			DialogResult dr = PrintPreviewDialog.Show(Qm, ChartPagePanel.ChartControl);
			return dr;
		}

		private void EditQueryBut_Click(object sender, EventArgs e)
		{
			CommandExec.Execute("EditQuery");
		}

		/// <summary>
		/// Show the mouse mode menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MouseModeButton_Click(object sender, EventArgs e)
		{
			MouseModeContextMenu.Show(MouseModeButton,
			new System.Drawing.Point(0, MouseModeButton.Height));
		}

		private void MouseModeContextMenu_Opening(object sender, CancelEventArgs e)
		{
			MouseSelectMenuItem.Checked = !ChartPagePanel.ChartPanel.UseMouseForScrolling;
			MouseTranslateMenuItem.Checked = ChartPagePanel.ChartPanel.UseMouseForScrolling;
		}

		private void MouseSelectMenuItem_Click(object sender, EventArgs e)
		{
			ChartPagePanel.ChartPanel.UseMouseForScrolling = false;
		}

		private void MouseTranslateMenuItem_Click(object sender, EventArgs e)
		{
			ChartPagePanel.ChartPanel.UseMouseForScrolling = true;
		}


	}
}
