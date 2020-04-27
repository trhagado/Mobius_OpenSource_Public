using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireClient;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Display the menu that allows the user to select a new view type.
	/// This operation is implemented as a form that contains the menu. 
	/// The form is created and shown which triggers the display of the menu.
	/// The menu is created in it's own form because an XtraBars.PopupMenu is difficult
	/// to integrate into the shell's ribbon form.
	/// </summary>

	public partial class ViewTypeSelectionMenu : DevExpress.XtraEditors.XtraForm
	{
		private QueryResultsControl Qrc;
		private QueryManager Qm;
		private SpotfireViewManager ChartView;

		public ViewTypeSelectionMenu()
		{
			InitializeComponent();

			return;
		}

		/// <summary>
		/// Show as a menu
		/// </summary>
		/// <param name="qrc"></param>
		/// <param name="qm"></param>
		/// <param name="location"></param>
		/// <returns></returns>

		public static void ShowPopupMenu(
			QueryResultsControl qrc,
			QueryManager qm,
			Point location)
		{
			ViewTypeSelectionMenu vtsd = new ViewTypeSelectionMenu();
			vtsd.Qrc = qrc;
			vtsd.Qm = qm;
			vtsd.Location = location; // position the form
			vtsd.Size = new Size(2, 2); // make smaller so hidden behind menu
			vtsd.Show(SessionManager.ActiveForm);
			return;
		}

		private void ShowMenu_Click(object sender, EventArgs e)
		{ // dev test
			AddChartViewMenu.ShowPopup(this.Location); // show where form is
		}

		// Add a new results page that can contain views

		private void AddPageButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			AddPage();
		}

		//////////////////////////////////////////////////////////////////////
		/////////////////////// Target Summaries /////////////////////////////
		//////////////////////////////////////////////////////////////////////

		private void TargetSummaryTableButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.TargetSummaryUnpivoted);
		}

		private void TargetSummaryPivotedButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.TargetSummaryUnpivoted);
		}

		private void TargetSummaryDendogramButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.TargetSummaryImageMap);
		}

		private void TargetSummaryHeatmapButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.TargetSummaryHeatmap);
		}

		private void TargetTargetSummaryTableButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.TargetTargetTable);
		}

		private void TargetTargetNetworkButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			//			AddView(ViewTypeMx.TargetTargetNetwork);
		}

		//////////////////////////////////////////////////////////////////////
		//////////////////////// Other Summaries /////////////////////////////
		//////////////////////////////////////////////////////////////////////

		private void CondFormatSummaryTableButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			AddTentativeNewView(ResultsViewType.SecondaryQuery);
		}

		//////////////////////////////////////////////////////////////////////
		////////////////////////// Base Views ////////////////////////////////
		//////////////////////////////////////////////////////////////////////

		private void TableViewButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			AddTentativeNewView(ResultsViewType.Table);
		}

		private void PivotViewButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			AddTentativeNewView(ResultsViewType.PivotGrid);
		}

		private void BubbleChartButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.ScatterPlot);
		}

		private void HeatmapButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			throw new NotImplementedException(); // AddTentativeNewView(ViewTypeMx.Heatmap);
		}

		private void SpotfireButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
		{
			AddTentativeNewView(ResultsViewType.Spotfire);
		}

		/// <summary>
		/// Add a new empty results page
		/// </summary>

		void AddPage()
		{
			DialogResult = DialogResult.OK; // close the selection menu
			Hide();
			SessionManager.ActivateShell();

			ResultsPage page = ResultsPages.AddNewPage(Qm.Query);

			Qrc.AddResultsPageTabPage(page); // add tab for page
			Qrc.ConfigureResultsPage(page); // render the empty page

			return;
		}

		/// <summary>
		/// Tentatively add a new view of the specified type to the current page
		/// </summary>
		/// <param name="chartType"></param>
		/// <returns></returns>

		ResultsViewProps AddTentativeNewView(ResultsViewType viewType)
		{
			int intVal, max;

			DialogResult = DialogResult.OK; // close the selection menu
			Hide();
			SessionManager.ActivateShell();
			ResultsViewProps view = AddViewHelper.AddTentativeNewView(viewType, Qm.Query, Qrc);
			return view;
		}

		private void AddChartViewMenu_BeforePopup(object sender, CancelEventArgs e)
		{
			//DebugLog.Message("AddChartViewMenu_BeforePopup");
		}

		private void AddChartViewMenu_Popup(object sender, EventArgs e)
		{
			//DebugLog.Message("AddChartViewMenu_Popup");
		}

		private void AddChartViewMenu_CloseUp(object sender, EventArgs e)
		{
			this.Close(); // close the form
										//DebugLog.Message("AddChartViewMenu_CloseUp");
		}

		private void ViewTypeSelectionDialog_Activated(object sender, EventArgs e)
		{
			//DebugLog.Message("Activated");
		}

		private void ViewTypeSelectionDialog_VisibleChanged(object sender, EventArgs e)
		{
			//DebugLog.Message("VisibleChanged");
		}

		private void ViewTypeSelectionDialog_Enter(object sender, EventArgs e)
		{
			//DebugLog.Message("Enter");
		}

		private void ViewTypeSelectionDialog_Shown(object sender, EventArgs e)
		{
			//DebugLog.Message("Shown");

			//while ((Form.MouseButtons & MouseButtons.Left) != 0)
			//  Application.DoEvents();

			AddChartViewMenu.ShowPopup(this.Location); // show the menu at the location where the form is shown

			//ShowMenu.PerformClick();

			//ShowMenu.Focus();
			//SendKeys.Send("{ENTER}");
		}

	}

}