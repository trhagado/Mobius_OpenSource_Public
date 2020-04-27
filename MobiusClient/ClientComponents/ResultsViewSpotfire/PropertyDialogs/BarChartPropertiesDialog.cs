using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.SpotfireClient;

using Mobius.SpotfireDocument;
using Mobius.SpotfireClient;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
//using DevExpress.Charts;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Mobius.SpotfireClient
{
	public partial class BarChartPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{
		static BarChartPropertiesDialog Instance;
		static string CurrentTabName = "Columns";

		SpotfireViewProps SVP;

		SpotfireApiClient Api => SpotfireSession.SpotfireApiClient;

		DataTableMapMsx DataMap => SVP?.DataTableMaps.CurrentMap;  // associated DataMap

		AnalysisApplicationMsx Analysis => SVP?.AnalysisApp; // the analysis

		BarChartMsx V
		{
			get { return SVP.ActiveVisual as BarChartMsx; }
			set { SVP.ActiveVisual = value; }
		}

		string OriginalChartState; // used to restore original state upon a cancel

		//QueryResultsControl Qrc { get { return QueryResultsControl.GetQrcThatContainsControl(SVP.RenderingControl); } }

		bool InSetup = false;

		bool AxesTabChanged = false;
		bool MarkerShapesControlChanged = false;
		bool MarkerColorRulesControlChanged = false;

		public BarChartPropertiesDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			BarChartMsx v,
			SpotfireViewProps svp)
		{
			Instance = new BarChartPropertiesDialog();
			return Instance.ShowDialog2(v, svp);
		}

		private DialogResult ShowDialog2(
			BarChartMsx v,
			SpotfireViewProps svp)
		{
			SVP = svp;
			V = v;

			OriginalChartState = SerializeMsx.Serialize(v);

			SetupForm();

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

///////////////////////////////////////////////////////////////////
//////////// Common code for setting up the dialog ////////////////
///////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adjust size and positions of main container controls
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void BarChartPropertiesDialog_Shown(object sender, EventArgs e)
		{
			PropertyDialogsUtil.AdjustPropertyPageTabs(Tabs, TabPageSelector, TabsContainerPanel);
		}

		private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (this.SplitContainer.CanFocus)
			{
				this.SplitContainer.ActiveControl = SelectorPanel;
			}
		}

		/// <summary>
		/// The selected page has changed. Show it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void TabPageSelector_AfterSelect(object sender, TreeViewEventArgs e)
		{
			XtraTabPage page = e.Node.Tag as XtraTabPage;
			Tabs.SelectedTabPage = page;
		}

		//////////////////////////////////////////////////////////////////////////
		//////////// End of common code for setting up the dialog ////////////////
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Setup the form with the TrellisCard
		/// </summary>

		void SetupForm()
		{
			InSetup = true;

			ValidateViewInitialization();

			GeneralPropertiesPanel.Setup(V);

			DataPropertiesPanel.Setup(V, SVP);

			TrellisPropertiesPanel.Setup(V, V.Trellis); 

			InSetup = false;
			return;
		}

		void ValidateViewInitialization()
		{
			if (SVP == null) throw new Exception("ViewManager not defined");

			//SVP.ValidateSpotfireViewPropsInitialization();

			if (V == null)
				V = new BarChartMsx();

			return;
		}

		/// <summary>
		/// Get rules & apply
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ApplyButton_Click(object sender, EventArgs e)
		{
			ApplyPendingChanges();
			return;
		}

		/// <summary>
		/// Apply any changes still pending
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OkButton_Click(object sender, EventArgs e)
		{
			ApplyPendingChanges();

			DialogResult = DialogResult.OK;
			return;
		}

		/// <summary>
		/// Changes cancelled, restore state
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CancelButton_Click(object sender, EventArgs e)
		{
			V = (BarChartMsx)SerializeMsx.DeserializeVisual(OriginalChartState, Analysis);

			//if (SVM.ConfigureCount != OriginalConfigureCount) // reconfigure if config changed
			//	SVM.ConfigureRenderingControl();

			//else if (SVM.RefreshCount != OriginalRefreshCount) // refresh if other change
			//	SVM.Refresh();

			DialogResult = DialogResult.Cancel;
			return;
		}

		/// <summary>
		/// Retrieve and apply any pending marker color/shape changes
		/// </summary>

		void ApplyPendingChanges()
		{
			//if (
			//	MarkerColorRulesControlChanged ||
			//	MarkerShapesControlChanged ||
			//	AxesTabChanged)
			//{
			//	if (MarkerColorRulesControlChanged)
			//	{
			//		QueryColumn qc = SVM.MarkerColor.QueryColumn;
			//		if (qc != null)
			//		{ // associate rules with QueryColumn
			//			if (qc.CondFormat == null) qc.CondFormat = new CondFormat();
			//			//qc.CondFormat.Rules = View.MarkerColorBy.ColorRulesControl.GetRules();
			//			if (qc.CondFormat.Rules != null)
			//				qc.CondFormat.Rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);
			//			//SVM.ApplyMarkerColorRules();
			//		}
			//		MarkerColorRulesControlChanged = false;
			//	}

			//	//if (MarkerShapesControlChanged)
			//	//{
			//	//  GetMarkerShapeSchemeRulesFromGrid();
			//	//  MarkerShapesControlChanged = false;
			//	//}

			//	if (AxesTabChanged)
			//	{
			//		GetAxesFieldList();
			//	}

			//	SVM.ConfigureRenderingControl();
			//}

			//if (TooltipFieldListControl.Changed) // update tooltip info
			//	SVM.MarkerTooltip.Fields = TooltipFieldListControl.GetFields();

			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			// todo: restore prev state
			Visible = false;
		}

		private void BarChartPropertiesDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Visible) CancelBut_Click(sender, e);
		}

		private void GeneralGroupControl_Enter(object sender, EventArgs e)
		{
		}

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 0)	GeneralPropertiesPanel.Title.Focus();
			ApplyButton.Visible = true;

			//if (Tabs.SelectedTabPage.Name == "ColorsTab" || Tabs.SelectedTabPage.Name == "ShapeTab" ||
			// Tabs.SelectedTabPage.Name == "AxesTab")
			//  ApplyButton.Visible = true;
			//else ApplyButton.Visible = false;
		}

		private void Tabs_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void BarChartPropertiesDialog_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void BarChartPropertiesDialog_Activated(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 0) GeneralPropertiesPanel.Title.Focus();
		}

	}
}