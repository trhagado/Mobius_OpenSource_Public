using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
//using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class TrellisPropertiesPanel : DevExpress.XtraEditors.XtraUserControl
	{
		VisualMsx V = null;
		TrellisMsx T = null;


		bool InSetup = false;
		// Note: Font for caption uses Calibri, 11.25pt, style=Bold
		// Tahoma 9.75 bold is not very readable

		public TrellisPropertiesPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		public void Setup(
			VisualMsx v,
			TrellisMsx t)
		{
			InSetup = true;
			V = v;
			T = t;

			if (T.TrellisMode == TrellisModeMsx.RowsColumns)
				TrellisByRowsAndCols.Checked = true;
			else TrellisByPanels.Checked = true;

			//TrellisColumnSelector.Setup(T.BaseQuery, T.TrellisColQc);
			//TrellisRowSelector.Setup(T.BaseQuery, T.TrellisRowQc);
			//TrellisPageSelector.Setup(T.BaseQuery, T.TrellisPageQc);

			//TrellisFlowBySelector.Setup(T.BaseQuery, T.TrellisFlowQc);

			TrellisManualLayout.Checked = T.ManualLayout;
			SetTrellisManualLayoutEnableds();
			TrellisMaxRows.Text = T.ManualRowCount.ToString();
			TrellisMaxCols.Text = T.ManualColumnCount.ToString();

			//ShowAxesTitles.Checked = T.ShowAxesTitles;
			//ShowAxesScaleLabels.Checked = T.ShowAxesScaleLabels;


			InSetup = false;
		}

		//////////////////////////////////////////////////////////
		////////////////////// Trellis Tab ///////////////////////
		//////////////////////////////////////////////////////////

		private void TrellisByRowsAndCols_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			GetTrellisMode();
			////SVM.ConfigureRenderingControl();
		}

		void GetTrellisMode()
		{
			if (TrellisByRowsAndCols.Checked) T.TrellisMode = TrellisModeMsx.RowsColumns;
			else T.TrellisMode = TrellisModeMsx.Panels;
		}


		private void TrellisColumnSelector_EditValueChanged(object sender, EventArgs e)
		{

			GetTrellisMode();
			//T.TrellisColQc = TrellisColumnSelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisRowSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			GetTrellisMode();
			//T.TrellisRowQc = TrellisRowSelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisPageSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			GetTrellisMode();

			T.TrellisMode = TrellisModeMsx.RowsColumns;
			TrellisByRowsAndCols.Checked = true;
			//T.TrellisPageQc = TrellisPageSelector.QueryColumn;
			////ChartView.TrellisPageIndex = 0; // reset to first page
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisByPanels_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;
			GetTrellisMode();

			//SVM.ConfigureRenderingControl();
		}

		private void TrellisFlowBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			T.TrellisMode = TrellisModeMsx.Panels;
			TrellisByPanels.Checked = true;
			//T.TrellisFlowQc = TrellisFlowBySelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisManualLayout_EditValueChanged(object sender, EventArgs e)
		{
			SetTrellisManualLayoutEnableds();

			if (InSetup) return;

			T.TrellisMode = TrellisModeMsx.Panels;
			TrellisByPanels.Checked = true;
			//SVM.ConfigureRenderingControl();
		}

		void SetTrellisManualLayoutEnableds()
		{
			TrellisMaxRows.Enabled = TrellisMaxCols.Enabled =
				 TrellisMaxRowsLabel.Enabled = TrellisMaxColsLabel.Enabled = TrellisManualLayout.Checked;
		}

		private void TrellisMaxRows_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			T.ManualRowCount = int.Parse(TrellisMaxRows.Text);
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisMaxCols_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			T.ManualColumnCount = int.Parse(TrellisMaxCols.Text);
			//SVM.ConfigureRenderingControl();
		}

		private void ShowAxesTitles_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			//T.ShowAxesTitles = ShowAxesTitles.Checked;
			//ShowAxesTitles2.Checked = View.ShowAxesTitles;
			InSetup = false;

			//SVM.ConfigureRenderingControl();
		}

		private void ShowAxesScaleLabels_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			//T.ShowAxesScaleLabels = ShowAxesScaleLabels.Checked;
			InSetup = false;

			//SVM.ConfigureRenderingControl();
		}


	}
}
