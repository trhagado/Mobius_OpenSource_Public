using Mobius.ComOps;

using DevExpress.XtraPivotGrid;
using DevExpress.Data.PivotGrid;
//using DevExpress.XtraCharts;
using DevExpress.XtraEditors;

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
	/// Properties for PivotGridView
	/// </summary>

	public class PivotGridPropertiesMx
	{
		public List<PivotGridFieldMx> PivotFields; // this list of pivot fields & their properties

		public bool CompactLayout = false;

		public bool ShowFilterHeaders = true;

		public bool ShowColumnTotals = true;
		public bool ShowColumnGrandTotals = true;
		public bool ShowRowTotals = true;
		public bool ShowRowGrandTotals = true;

		public string PivotGridChartType = "None"; // charting options
		public bool PgcShowSelectionOnly = true;
		public bool PgcProvideDataByColumns = false;
		public bool PgcShowPointLabels = false;
		public bool PgcShowColumnGrandTotals = false;
		public bool PgcShowRowGrandTotals = false;
	}

}