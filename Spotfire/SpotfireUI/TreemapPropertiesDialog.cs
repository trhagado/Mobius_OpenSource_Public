using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

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
	public partial class TreemapPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{
		static TreemapPropertiesDialog Instance;
		static string CurrentTabName = "Columns";

		SpotfireViewProps SVP; // view properties associated with this dialog
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap
		SpotfireApiClient Api => SpotfireSession.SpotfireApiClient;

		TreemapMsx V
		{
			get { return SVP.ActiveVisual as TreemapMsx; }
			set { SVP.ActiveVisual = value; }
		}

		string OriginalChartState; // used to restore original state upon a cancel
		int OriginalConfigureCount = 0; // render count upon entry
		int OriginalRefreshCount = 0; // number of refreshes of the view

		//QueryResultsControl Qrc { get { return QueryResultsControl.GetQrcThatContainsControl(SVP.RenderingControl); } }

		bool InSetup = false;

		bool AxesTabChanged = false;
		bool MarkerShapesControlChanged = false;
		bool MarkerColorRulesControlChanged = false;

		public TreemapPropertiesDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			TreemapMsx v,
			SpotfireViewProps svp)
		{
			Instance = new TreemapPropertiesDialog();
			return Instance.ShowDialog2(v, svp);
		}

		private DialogResult ShowDialog2(
			TreemapMsx v,
			SpotfireViewProps svp)
		{
			SVP = svp;
			V = v;

			//OriginalConfigureCount = view.ConfigureCount;
			OriginalChartState = v.Serialize();
			//OriginalRefreshCount = view.RefreshCount;

			SetupForm();

			DialogResult dr = ShowDialog(Form.ActiveForm);
			//view.ChartControl.Series[0].Points
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

		private void TreemapPropertiesDialog_Shown(object sender, EventArgs e)
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
		/// Setup the form
		/// </summary>
		/// <param name="tabName"></param>

		void SetupForm()
		{
			InSetup = true;

			ValidateViewInitialization();

			GeneralPropertiesPanel.Setup(V);

			//DataMapPanel.Setup(SVP);

			TrellisPropertiesPanel.Setup(V, V.Trellis);

			InSetup = false;
			return;
		}

		void ValidateViewInitialization()
		{
			if (SVP == null) throw new Exception("ViewManager not defined");

			//SVP.ValidateSpotfireViewPropsInitialization();

			if (V == null)
				V = new TreemapMsx();

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

		private void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
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

		private void TreemapPropertiesDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Visible) CancelBut_Click(sender, e);
		}

		private void GeneralGroupControl_Enter(object sender, EventArgs e)
		{
		}

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			//Title.Focus();

			//if (Tabs.SelectedTabPage.Name == "ColorsTab" || Tabs.SelectedTabPage.Name == "ShapeTab" ||
			// Tabs.SelectedTabPage.Name == "AxesTab")
			//  ApplyButton.Visible = true;
			//else ApplyButton.Visible = false;
		}

		private void Tabs_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void TreemapPropertiesDialog_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void TreemapPropertiesDialog_Activated(object sender, EventArgs e)
		{
			//if (Tabs.SelectedTabPageIndex == 0) Title.Focus();
		}

		/////////////////////////////////////////////////////////
		///////////////////// General Tab ///////////////////////
		/////////////////////////////////////////////////////////

		private void Title_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.Title = Title.Text;
			//SVM.BuildTitles();
			//SVM.UpdateContainerTitles();
			//SVM.Refresh();
		}

		private void Title_VisibleChanged(object sender, EventArgs e)
		{
			//if (Tabs.SelectedTabPageIndex == GeneralTpi) Title.Focus();
		}

		private void ShowTitle_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.ShowTitle = ShowTitle.Checked;
			//SVM.ConfigureRenderingControl();
		}

		private void Description_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.Description = Description.Text;
		}
		////////////////////////////////////////////////////////
		///////////////////// Colors Tab ///////////////////////
		////////////////////////////////////////////////////////

		private void ColorBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.BuildMarkerColorDimension();
			//SVM.SetupLegend();
			//SVM.Refresh();
		}

		/// <summary>
		/// Get colors from column field values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldValueContainsColor_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//View.MarkerFieldValueContainsColor = ColorBySelector.valueMarkerField.ValueContainsColor.Checked;
			//SVM.BuildMarkerColorDimension();
			//SVM.SetupLegend();
			//SVM.Refresh();
			return;
		}

		//////////////////////////////////////////////////////
		///////////////////// Size Tab ///////////////////////
		//////////////////////////////////////////////////////

		private void SizeBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.BuildMarkerSizeDimension();
			//SVM.SetupLegend();
			//SVM.Refresh();
			return;
		}

		///////////////////////////////////////////////////////
		///////////////////// Shape Tab ///////////////////////
		///////////////////////////////////////////////////////

		private void ShapeBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.BuildMarkerShapeDimension();
			//SVM.SetupLegend();
			//SVM.Refresh();
			return;
		}

		///////////////////////////////////////////////////////
		///////////////////// Label Tab ///////////////////////
		///////////////////////////////////////////////////////

		//private void LabelColumnSelector_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	//SVM.MarkerLabel.QueryColumn = LabelColumnSelector.QueryColumn;
		//	//SVM.BuildMarkerLabelDimension();
		//	//SVM.SetupLegend();
		//	//SVM.Refresh();
		//}

		//private void LabelsAll_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	if (!LabelsAll.Checked) return;
		//	//SVM.MarkerLabel.VisibilityMode = LabelVisibilityModeEnum.AllRows;
		//	//SVM.BuildMarkerLabelDimension();
		//	//SVM.Refresh();
		//}

		//private void LabelsSelected_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	if (!LabelsSelected.Checked) return;
		//	//SVM.MarkerLabel.VisibilityMode = LabelVisibilityModeEnum.MarkedRows;
		//	//SVM.BuildMarkerLabelDimension();
		//	//SVM.Refresh();
		//}

		//private void LabelsCenter_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	if (!LabelsCenter.Checked) return;
		//	//SVM.MarkerLabel.Position = LabelPositionEnum.Center;
		//	//SVM.BuildMarkerLabelDimension();
		//	//SVM.Refresh();
		//}

		//private void LabelsOutside_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	if (!LabelsOutside.Checked) return;
		//	//SVM.MarkerLabel.Position = LabelPositionEnum.Outside;
		//	//SVM.BuildMarkerLabelDimension();
		//	//SVM.Refresh();
		//}

		//private void IncludeStructureInLabel_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	//SVM.MarkerLabel.IncludeStructure = IncludeStructureInLabel.Checked;
		//	////SVM.BuildMarkerLabelDimension();
		//	//SVM.Refresh();
		//}

		//private void MaxLabels_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	//SVM.MaxLabels = (int)MaxLabels.Value;
		//	////SVM.BuildMarkerLabelDimension();
		//	//SVM.Refresh();
		//}

		private void BrowseBackgroundFileButton_Click(object sender, EventArgs e)
		{
			//string filter =
			//	"Files (*.jpg, *.png, .bmp, .gif)|*.jpg; *.png; .bmp; .gif|All files (*.*)|*.*";
			//string fileName = UIMisc.GetOpenFilename("File Name", BackgroundImageFileName.Text, filter, ".jpg");
			//if (fileName == "") return;
			//BackgroundImageFileName.Text = fileName;
		}

		///////////////////////////////////////////////////////
		////////////////////// Tooltip Tab ///////////////////////
		///////////////////////////////////////////////////////

		private void IncludeStructureInTooltip_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.MarkerTooltip.IncludeStructure = IncludeStructureInTooltip.Checked;
		}

		private void TooltipFieldListControl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.MarkerTooltip.Fields = TooltipFieldListControl.GetFields();
		}

		///////////////////////////////////////////////////////
		///////////////////// Legend Tab //////////////////////
		///////////////////////////////////////////////////////

		private void ShowLegendBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.ShowLegend = ShowLegend.Checked;
			//SVM.ChartPagePanel.ChartPanel.SetLegendVisibility();
			return;
		}

		private void LegendHorizontalAlignment_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = LegendAlignmentHorizontal.Text;
			txt = Lex.CompressCapitalizedName(txt);
			//SVM.LegendAlignmentHorizontal =
			//	(DevExpress.XtraCharts.LegendAlignmentHorizontal)Enum.Parse(typeof(DevExpress.XtraCharts.LegendAlignmentHorizontal), txt);
			//SVM.ConfigureRenderingControl();
		}

		private void LegendVerticalAlignment_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = LegendAlignmentVertical.Text;
			txt = Lex.CompressCapitalizedName(txt);
			//SVM.LegendAlignmentVertical =
			//	(DevExpress.XtraCharts.LegendAlignmentVertical)Enum.Parse(typeof(DevExpress.XtraCharts.LegendAlignmentVertical), txt);
			//SVM.ConfigureRenderingControl();
		}

		private void LegendMaxHorizontalPercentage_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = MaxHorizontalPercentage.Text.Trim();
			if (txt.EndsWith("%"))
				txt = txt.Substring(0, txt.Length - 1).Trim();
			//int.TryParse(txt, out SVM.LegendMaxHorizontalPercentage);
			//SVM.ConfigureRenderingControl();
		}

		private void LegendMaxVerticalPercentage_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = MaxVerticalPercentage.Text.Trim();
			if (txt.EndsWith("%"))
				txt = txt.Substring(0, txt.Length - 1).Trim();
			//int.TryParse(txt, out SVM.LegendMaxVerticalPercentage);
			//SVM.ConfigureRenderingControl();
		}

		private void LegendItemOrder_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = LegendItemOrder.Text;
			txt = Lex.CompressCapitalizedName(txt);
			//SVM.LegendItemOrder =
			//	(DevExpress.XtraCharts.LegendDirection)Enum.Parse(typeof(DevExpress.XtraCharts.LegendDirection), txt);
			//SVM.ConfigureRenderingControl();
		}

		//////////////////////////////////////////////////////////
		////////////////////// Trellis Tab ///////////////////////
		//////////////////////////////////////////////////////////

		private void TrellisByRowsAndCols_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;
			//SVM.TrellisByRowCol = TrellisByRowsAndCols.Checked;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisColumnSelector_EditValueChanged(object sender, EventArgs e)
		{

			//SVM.TrellisByRowCol = TrellisByRowsAndCols.Checked = true;
			//SVM.TrellisColQc = TrellisColumnSelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisRowSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.TrellisByRowCol = TrellisByRowsAndCols.Checked = true;
			//SVM.TrellisRowQc = TrellisRowSelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisPageSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.TrellisByRowCol = TrellisByRowsAndCols.Checked = true;
			//SVM.TrellisPageQc = TrellisPageSelector.QueryColumn;
			////ChartView.TrellisPageIndex = 0; // reset to first page
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisByPanels_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;
			//SVM.TrellisByFlow = TrellisByPanels.Checked;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisFlowBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.TrellisByFlow = TrellisByPanels.Checked = true;
			//SVM.TrellisFlowQc = TrellisFlowBySelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisManualLayout_EditValueChanged(object sender, EventArgs e)
		{
			SetTrellisManualLayoutEnableds();

			if (InSetup) return;
			//SVM.TrellisManual = TrellisManualLayout.Checked;
			//TrellisByPanels.Checked = true;
			//SVM.ConfigureRenderingControl();
		}

		void SetTrellisManualLayoutEnableds()
		{
			//TrellisMaxRows.Enabled = TrellisMaxCols.Enabled =
			//	 TrellisMaxRowsLabel.Enabled = TrellisMaxColsLabel.Enabled = TrellisManualLayout.Checked;
		}

		private void TrellisMaxRows_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.TrellisMaxRows = int.Parse(TrellisMaxRows.Text);
			//SVM.ConfigureRenderingControl();
		}

		private void TrellisMaxCols_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.TrellisMaxCols = int.Parse(TrellisMaxCols.Text);
			//SVM.ConfigureRenderingControl();
		}

		private void ShowAxesTitles_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			//SVM.ShowAxesTitles = ShowAxesTitles.Checked;
			//ShowAxesTitles2.Checked = View.ShowAxesTitles;
			InSetup = false;

			//SVM.ConfigureRenderingControl();
		}

		private void ShowAxesScaleLabels_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			//SVM.ShowAxesScaleLabels = ShowAxesScaleLabels.Checked;
			InSetup = false;

			//SVM.ConfigureRenderingControl();
		}

		/////////////////////////////////////////////////////////////
		////////////////////// Appearance Tab ///////////////////////
		/////////////////////////////////////////////////////////////

		private void BackgroundImageFileName_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.BackgroundImageFile = BackgroundImageFileName.Text;
			//SVM.SetupBackgroundImage();
			//SVM.Refresh();
		}

		private void JitterX_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SVM.JitterX = JitteringX.Value;
			//SVM.Refresh();
		}

		private void JitteringY_EditValueChanged(object sender, EventArgs e)
		{
			//SVM.JitterY = JitteringY.Value;
			//SVM.Refresh();
		}

	}
}