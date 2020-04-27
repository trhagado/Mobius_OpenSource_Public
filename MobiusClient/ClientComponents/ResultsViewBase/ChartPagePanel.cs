using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraCharts;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Mobius.ClientComponents
{
/// <summary>
/// Contains the ChartPanel, filters and details on demand
/// </summary>

	public partial class ChartPagePanel : XtraUserControl
	{
		internal static bool ShowFilters = true;
		internal static bool ShowLegend = true;
		internal static bool ShowSelectedDataRows = true;
		internal static int FiltersDockPanelWidth = 200;
		internal static int DoDGridDockPanelHeight = 200;

		internal bool Insetup = false;

		internal ChartControlMx ChartControl { get { return ChartPanel.ChartControl; } }
		internal ChartViewMgr ChartView { get { return ChartPanel.ChartControl.ChartView; } } // current chart for the page panel 
		internal ResultsPage ResultsPage { get { return ChartView.ResultsPage; } }  // the results page associated with the page panel
		internal ChartHitInfo HighlightedDataPoint = null;

		internal ChartPageControl ChartPageControl; // containing ChartPageControl, must be set by owner

		internal QueryManager Qm { get { return ChartView.Qm; } }

		internal DataTableManager Dtm 
			{ get { return (Qm != null) ? Qm.DataTableManager : null; } }

		internal MoleculeGridControl DetailsOnDemandGrid // molecule grid for selected data rows
			{ get { return (Qm != null) ? Qm.MoleculeGrid : null; } }


		public ChartPagePanel()
		{
			InitializeComponent();

			ChartPanel.ChartPagePanel = this; // link chartpanel up to up
		}

/// <summary>
/// Initialize chart panel
/// </summary>

		internal void Initialize()
		{
			ChartPanel.ChartControl.Visible = false;
			ChartPanel.TrellisPanel.Visible = false; 
			return;
		}

/// <summary>
/// Return count of charts on page
/// </summary>
/// <returns></returns>

		internal int ChartCount
		{
			get
			{
				////if (ChartControl != null && ChartControl.Charts != null)
				//// return ChartControl.Charts.Count;

				if (ChartControl != null) return 1; 
				else return 0;
			}
		}

	}
}
