using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
//using Mobius.ClientComponents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class SpotfireToolbar : DevExpress.XtraEditors.XtraUserControl
	{
		SpotfireViewProps SVP;

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap

		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap

		public SpotfireApiClient SpotfireApiClient => SpotfireSession.SpotfireApiClient;  // API link to Spotfire session to interact with


		public SpotfireToolbar()
		{
			InitializeComponent();
		}

		private void OpenFromFile_Click(object sender, EventArgs e)
		{

		}

		private void OpenFromLibraryMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void FiltersButton_Click(object sender, EventArgs e)
		{

		}

		private void DetailsOnDemandButton_Click(object sender, EventArgs e)
		{

		}

		private void TableVisualButton_Click(object sender, EventArgs e)
		{

		}

		private void BarChartButton_Click(object sender, EventArgs e)
		{

		}

		private void ScatterPlotButton_Click(object sender, EventArgs e)
		{

		}

		private void TreemapButton_Click(object sender, EventArgs e)
		{

		}

		private void HeatMapButton_Click(object sender, EventArgs e)
		{

		}

		private void TrellisCardButton_Click(object sender, EventArgs e)
		{

		}

		private void DataButton_Click(object sender, EventArgs e)
		{
			EditDataProperties(SVP);
		}

		public static DialogResult EditDataProperties(
			SpotfireViewProps svp)
		{
			throw new NotImplementedException();

			//DialogResult dr = DataMapDialog.ShowDialog(svp);
			//return dr;
		}

		private void VisPropsButton_Click(object sender, EventArgs e)
		{
			VisualMsx v = SpotfireSession.SpotfireApiClient?.GetActiveVisual();
			if (v == null) return;

			DialogResult dr = EditVisualProperties(v, SVP);
			return;
		}

/// <summary>
/// Return true if visual properties can be edited
/// </summary>
/// <param name="v"></param>
/// <returns></returns>

		public static bool CanEditVisualProperties(
			VisualMsx v)
		{
			DialogResult dr = DialogResult.Cancel;

			if (v == null) return false;

			//else if (v is TablePlotMsx) return true;

			else if (v is BarChartMsx) return true;

			else if (v is ScatterPlotMsx) return true;

			//else if (v is TreemapMsx) return true;

			//else if (v is HeatMapMsx) return true;

			//else if (v is TrellisCardVisualMsx) return true;

			else return false;
		}


		public static DialogResult EditVisualProperties(
			VisualMsx v,
			SpotfireViewProps svp)
		{
			DialogResult dr = DialogResult.Cancel;

			if (v == null) return DialogResult.Cancel;

			else if (v is TablePlotMsx)
			{
				TablePlotPropertiesDialog.ShowDialog((TablePlotMsx)v, svp);
			}

			else if (v is BarChartMsx)
			{
				BarChartPropertiesDialog.ShowDialog((BarChartMsx)v, svp);
			}

			else if (v is ScatterPlotMsx)
			{
				ScatterPlotPropertiesDialog.ShowDialog((ScatterPlotMsx)v, svp);
			}

			else if (v is TreemapMsx)
			{
				TreemapPropertiesDialog.ShowDialog((TreemapMsx)v, svp);
			}

			else if (v is HeatMapMsx)
			{
				HeatMapPropertiesDialog.ShowDialog((HeatMapMsx)v, svp);
			}

			else if (v is TrellisCardVisualMsx)
			{
				TrellisCardPropertiesDialog.ShowDialog((TrellisCardVisualMsx)v, svp);
			}

			else
			{
				MessageBox.Show("Can't edit properties for a " + v.TypeId.DisplayName + " visualization", "Spotfire Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return dr;
		}

		private void ArrangeEvenlyButton_Click(object sender, EventArgs e)
		{

		}

		private void ArrangeSideBySideButton_Click(object sender, EventArgs e)
		{

		}

		private void ArrangeStackedButton_Click(object sender, EventArgs e)
		{

		}

		private void ArrangeMaximizeActive_Click(object sender, EventArgs e)
		{

		}

	}
}
