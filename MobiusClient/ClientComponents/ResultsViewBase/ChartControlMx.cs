using DevExpress.XtraEditors;
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
	public partial class ChartControlMx : ChartControl
	{
		public ChartViewMgr ChartView; // the chartview that corresponds to this control

		public ChartControlMx()
		{
			InitializeComponent();
		}

	}
}
