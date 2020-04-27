using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
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
using DevExpress.Charts;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Mobius.ClientComponents
{
	public partial class ChartPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{

		public ChartPropertiesDialog()
		{
			InitializeComponent();
		}

#if false // obsolete
		static ChartPropertiesDialog Instance;
		ChartViewMgr View = null;
		DataTable ShapeSchemeDataTable;
		string OriginalChartState; // used to restore original state upon a cancel
		int OriginalConfigureCount = 0; // render count upon entry
		int OriginalRefreshCount = 0; // number of refreshes of the view

		QueryResultsControl Qrc { get { return QueryResultsControl.GetQrcThatContainsControl(View.RenderingControl); } }

		bool InSetup = false;

		bool AxesTabChanged = false;
		bool MarkerShapesControlChanged = false;
		bool MarkerColorRulesControlChanged = false;

		// Tab page indexes

		static int Tpi = 0;
		static int GeneralTpi = Tpi++;
		static int XAxisTpi = Tpi++;
		static int YAxisTpi = Tpi++;
		static int ZAxisTpi = Tpi++;
		static int AxesTpi = Tpi++;
		static int ColorsTpi = Tpi++;
		static int ShapeTpi = Tpi++;
		static int SizeTpi = Tpi++;
		static int LabelsTpi = Tpi++;
		static int TooltipTpi = Tpi++;
		static int LegendTpi = Tpi++;
		static int SurfaceTpi = Tpi++;
		static int TrellisTpi = Tpi++;
		static int MiscTpi = Tpi++;

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			ChartViewMgr view)
		{
			return ShowDialog(view, "");
		}

		public static DialogResult ShowDialog(
			ChartViewMgr view,
			string tabName)
		{
			if (view.ChartControl == null) return DialogResult.Cancel;

			if (Instance == null) Instance = new ChartPropertiesDialog();
			ChartPropertiesDialog i = Instance;
			return i.ShowDialog2(view, tabName);
		}

		private DialogResult ShowDialog2(
			ChartViewMgr view,
			string tabName)
		{
			View = view;
			Setup(tabName);
			OriginalConfigureCount = view.ConfigureCount;
			OriginalChartState = view.SerializeChartProperties();
			OriginalRefreshCount = view.RefreshCount;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			//view.ChartControl.Series[0].Points
			return dr;
		}

		/// <summary>
		/// Adjust size and positions of main container controls
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ChartPropertiesDialog_Shown(object sender, EventArgs e)
		{
			CommonTabsSetup();
		}

		/// <summary>
		/// Do common setup of tabs in visualization property dialogs
		/// </summary>

		private void CommonTabsSetup()
		{
			TabPageSelector.Dock = DockStyle.Fill;

			TabsContainerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

			Tabs.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

			Tabs.Dock = DockStyle.None;
			Tabs.Location = new Point(-1, -1);
			Tabs.Size = new Size(TabsContainerPanel.Width + 5, TabsContainerPanel.Height + 4);
			//Tabs.Size = new Size(TabsContainerPanel.Width - 10, TabsContainerPanel.Height - 10);
			Tabs.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;
			Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));

			return;
		}


		/// <summary>
		/// Setup the form
		/// </summary>
		/// <param name="tabName"></param>

		void Setup(string tabName)
		{
			QueryColumn qc;

			InSetup = true;

			// Select initial tab

			Tpi = -1;

			if (Lex.Eq(tabName, "General")) Tpi = GeneralTpi;
			else if (Lex.Eq(tabName, "X-Axis")) Tpi = XAxisTpi;
			else if (Lex.Eq(tabName, "Y-Axis")) Tpi = YAxisTpi;
			else if (Lex.Eq(tabName, "Z-Axis")) Tpi = ZAxisTpi;
			else if (Lex.Eq(tabName, "Axes")) Tpi = AxesTpi;
			else if (Lex.Eq(tabName, "Colors")) Tpi = ColorsTpi;
			else if (Lex.Eq(tabName, "Shape")) Tpi = ShapeTpi;
			else if (Lex.Eq(tabName, "Size")) Tpi = SizeTpi;
			else if (Lex.Eq(tabName, "Labels")) Tpi = LabelsTpi;
			else if (Lex.Eq(tabName, "Tootip")) Tpi = TooltipTpi;
			else if (Lex.Eq(tabName, "Legend")) Tpi = LegendTpi;
			else if (Lex.Eq(tabName, "Surface")) Tpi = SurfaceTpi;
			else if (Lex.Eq(tabName, "Trellis")) Tpi = TrellisTpi;
			else if (Lex.Eq(tabName, "Misc")) MiscTpi = 11;

			if (Tpi >= 0 && Tpi < Tabs.TabPages.Count) Tabs.SelectedTabPageIndex = Tpi;

			// Default visible tabs

			GeneralTab.PageVisible = true;
			XAxisTab.PageVisible = YAxisTab.PageVisible = true;
			ZAxisTab.PageVisible = false;
			ColorsTab.PageVisible = SizeTab.PageVisible = ShapeTab.PageVisible = LabelsTab.PageVisible = true;
			SurfaceTab.PageVisible = false;
			TrellisTab.PageVisible = false;
			AxesTab.PageVisible = false; // radar axes

			// Setup for specific char type

			if (View.ViewType == ViewTypeMx.ScatterPlot)
			{
				Text = "Bubble Chart Properties";
			}

			else if (View.ViewType == ViewTypeMx.Heatmap)
			{
				Text = "Heat Map Properties";

				SizeTab.PageVisible = ShapeTab.PageVisible = LabelsTab.PageVisible = false;
				ZAxisTab.PageVisible = false;
				LabelsTab.PageVisible = false;
			}

			else if (View.ViewType == ViewTypeMx.RadarPlot)
			{
				Text = "Radar Plot Properties";
				XAxisTab.PageVisible = YAxisTab.PageVisible = ZAxisTab.PageVisible = false;
				AxesTab.PageVisible = true;
				LabelsTab.PageVisible = false;
			}

			else if (View.ViewType == ViewTypeMx.BarChart)
			{
				Text = "Bar Chart Properties";
				XAxisTab.PageVisible = YAxisTab.PageVisible = ZAxisTab.PageVisible = false;
				AxesTab.PageVisible = true;
			}

			// Setup General tab

			Title.Text = View.Title;
			ShowTitle.Checked = View.ShowTitle;
			Description.Text = View.Description;

			// Setup X-axis tab

			XColumnSelector.Setup(View.BaseQuery, View.XAxisMx.QueryColumn);
			XAxisOptions.Setup(View.XAxisMx);
			ShowXAxisTitle.Checked = View.ShowAxesTitles;
			RotateAxesX.Checked = View.RotateAxes;

			// Setup Y-axis tab

			YColumnSelector.Setup(View.BaseQuery, View.YAxisMx.QueryColumn);
			YColumnSelector.ExcludedDataTypes[MetaColumnType.Date] = true; // dates not allowed on XYDiagram Y axis
			YAxisOptions.Setup(View.YAxisMx);
			ShowYAxisTitle.Checked = View.ShowAxesTitles;
			RotateAxesY.Checked = View.RotateAxes;

			// Setup Z-axis tab

			ZColumnSelector.Setup(View.BaseQuery, View.ZAxisMx.QueryColumn);
			ZAxisOptions.Setup(View.ZAxisMx);
			ShowZAxisTitle.Checked = View.ShowAxesTitles;
			ZAxisOptions.ShowZoomSlider.Enabled = false; // no zoomslider for Z axis

			// Setup Axes Tab

			SetupAxesFieldList();

			ShowAxesTitles2.Checked = View.ShowAxesTitles;
			ShowAxesScaleLabels2.Checked = View.ShowAxesScaleLabels;

			// Setup Colors tab

			ColorBySelector.Setup(View, View.MarkerColor);

			// Setup Size tab

			SizeBySelector.Setup(View, View.MarkerSize);

			// Setup Shape tab

			ShapeBySelector.Setup(View, View.MarkerShape, MarkerShapePopup);

			// Setup Labels tab

			LabelColumnSelector.Setup(View.BaseQuery, View.MarkerLabel.QueryColumn);
			LabelColumnSelector.ExcludedDataTypes[MetaColumnType.Structure] = true; // structs not allowed on labels 

			if (View.MarkerLabel.VisibilityMode == LabelVisibilityModeEnum.AllRows)
				LabelsAll.Checked = true;
			else if (View.MarkerLabel.VisibilityMode == LabelVisibilityModeEnum.MarkedRows)
				LabelsSelected.Checked = true;

			if (View.MarkerLabel.Position == LabelPositionEnum.Center)
				LabelsCenter.Checked = true;
			else LabelsOutside.Checked = true;

			IncludeStructureInLabel.Checked = View.MarkerLabel.IncludeStructure;

			MaxLabels.Value = View.MaxLabels;

			// Setup Tooltip tab

			TooltipFieldListControl.Setup(View.BaseQuery, View.MarkerTooltip.Fields);
			IncludeStructureInTooltip.Checked = View.MarkerTooltip.IncludeStructure;

			// Setup Legend tab

			ShowLegend.Checked = View.ShowLegend;
			LegendAlignmentHorizontal.Text = Lex.ExpandCapitalizedName(View.LegendAlignmentHorizontal.ToString(), true);
			LegendAlignmentVertical.Text = Lex.ExpandCapitalizedName(View.LegendAlignmentVertical.ToString(), true);
			MaxHorizontalPercentage.Text = View.LegendMaxHorizontalPercentage.ToString() + "%";
			MaxVerticalPercentage.Text = View.LegendMaxVerticalPercentage.ToString() + "%";
			LegendItemOrder.Text = Lex.ExpandCapitalizedName(View.LegendItemOrder.ToString(), true);

			// Setup Surface tab

			FillModeComboBox.SelectedIndex = (int)View.SurfaceFillMode;
			FrameModeComboBox.SelectedIndex = (int)View.SurfaceFrameMode;
			SmoothPalette.Checked = View.SmoothPalette;
			SmoothShading.Checked = View.SmoothShading;
			SemitransparentSurface.Checked = View.SemiTransparent;
			DrawFlat.Checked = View.DrawFlat;

			// Setup Trellis tab

			TrellisByRowsAndCols.Checked = View.TrellisFlowQc == null;
			TrellisColumnSelector.Setup(View.BaseQuery, View.TrellisColQc);
			TrellisRowSelector.Setup(View.BaseQuery, View.TrellisRowQc);
			TrellisPageSelector.Setup(View.BaseQuery, View.TrellisPageQc);

			TrellisByPanels.Checked = View.TrellisFlowQc != null;
			TrellisFlowBySelector.Setup(View.BaseQuery, View.TrellisFlowQc);

			View.TrellisManual = TrellisManualLayout.Checked;
			SetTrellisManualLayoutEnableds();
			TrellisMaxRows.Text = View.TrellisMaxRows.ToString();
			TrellisMaxCols.Text = View.TrellisMaxCols.ToString();

			ShowAxesTitles.Checked = View.ShowAxesTitles;
			ShowAxesScaleLabels.Checked = View.ShowAxesScaleLabels;

			// Setup Misc tab

			BackgroundImageFileName.Text = View.BackgroundImageFile;
			ShowLegend.Checked = View.ShowLegend;
			StretchChart.Checked = View.JitterTheSameForXandY;
			ShapeRenderingModeComboBox.SelectedIndex = (int)View.ShapeRenderingMode;
			Jittering.Value = View.JitterX;

			InSetup = false;
			return;
		}

		/// <summary>
		/// Get rules & apply
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ApplyButton_Click(object sender, EventArgs e)
		{
			GetAndApplyPendingChanges();
			return;
		}

		/// <summary>
		/// Apply any changes still pending
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OkButton_Click(object sender, EventArgs e)
		{
			GetAndApplyPendingChanges();

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
			ResultsViewProps.DeserializeChartProperties(OriginalChartState, View.BaseQuery, View); // restore previous state

			if (View.ConfigureCount != OriginalConfigureCount) // reconfigure if config changed
				View.ConfigureRenderingControl();

			else if (View.RefreshCount != OriginalRefreshCount) // refresh if other change
				View.Refresh();

			DialogResult = DialogResult.Cancel;
			return;
		}

		/// <summary>
		/// Retrieve and apply any pending marker color/shape changes
		/// </summary>

		void GetAndApplyPendingChanges()
		{
			AxesTabChanged |= AxesFieldListControl.Changed;
			if (
				MarkerColorRulesControlChanged ||
				MarkerShapesControlChanged ||
				AxesTabChanged)
			{
				if (MarkerColorRulesControlChanged)
				{
					QueryColumn qc = View.MarkerColor.QueryColumn;
					if (qc != null)
					{ // associate rules with QueryColumn
						if (qc.CondFormat == null) qc.CondFormat = new CondFormat();
						//qc.CondFormat.Rules = View.MarkerColorBy.ColorRulesControl.GetRules();
						if (qc.CondFormat.Rules != null)
							qc.CondFormat.Rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);
						View.ApplyMarkerColorRules();
					}
					MarkerColorRulesControlChanged = false;
				}

				//if (MarkerShapesControlChanged)
				//{
				//  GetMarkerShapeSchemeRulesFromGrid();
				//  MarkerShapesControlChanged = false;
				//}

				if (AxesTabChanged)
				{
					GetAxesFieldList();
					AxesTabChanged = AxesFieldListControl.Changed = false;
				}

				View.ConfigureRenderingControl();
			}

			if (TooltipFieldListControl.Changed) // update tooltip info
				View.MarkerTooltip.Fields = TooltipFieldListControl.GetFields();

			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			// todo: restore prev state
			Visible = false;
		}

		private void ChartPropertiesDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Visible) CancelBut_Click(sender, e);
		}

		private void GeneralGroupControl_Enter(object sender, EventArgs e)
		{
		}

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			Title.Focus();
			ApplyButton.Visible = true;

			//if (Tabs.SelectedTabPage.Name == "ColorsTab" || Tabs.SelectedTabPage.Name == "ShapeTab" ||
			// Tabs.SelectedTabPage.Name == "AxesTab")
			//  ApplyButton.Visible = true;
			//else ApplyButton.Visible = false;
		}

		private void Tabs_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void ChartPropertiesDialog_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void ChartPropertiesDialog_Activated(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 0) Title.Focus();
		}

		/////////////////////////////////////////////////////////
		///////////////////// General Tab ///////////////////////
		/////////////////////////////////////////////////////////

		private void Title_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.Title = Title.Text;
			View.BuildTitles();
			View.UpdateContainerTitles();
			View.Refresh();
		}

		private void Title_VisibleChanged(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == GeneralTpi) Title.Focus();
		}

		private void ShowTitle_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ShowTitle = ShowTitle.Checked;
			View.ConfigureRenderingControl();
		}

		private void Description_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.Description = Description.Text;
		}

		////////////////////////////////////////////////////////
		///////////////////// X-axis Tab ///////////////////////
		////////////////////////////////////////////////////////

		private void XColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.XAxisMx.QueryColumn = XColumnSelector.QueryColumn;
			View.ConfigureRenderingControl();
		}

		private void XAxisOptions_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ConfigureRenderingControl();
		}

		private void ShowXAxisTitle_EditValueChanged(object sender, EventArgs e)
		{
			ShowAxisTitle(ShowXAxisTitle.Checked);
		}

		void ShowAxisTitle(bool show)
		{
			if (InSetup) return;

			View.ShowAxesTitles = show;

			ChartControlMx cc = View.ChartControl;
			View.BuildAxes();
			View.Refresh();

			InSetup = true; // sync checks
			ShowXAxisTitle.Checked = ShowYAxisTitle.Checked = ShowZAxisTitle.Checked = View.ShowAxesTitles; // set check for each axis to match
			InSetup = false;
		}

		private void RotateAxesX_EditValueChanged(object sender, EventArgs e)
		{
			RotateAxes(RotateAxesX.Checked);
		}

		private void RotateAxes(bool rotate)
		{
			if (InSetup) return;

			View.RotateAxes = rotate;
			View.ConfigureRenderingControl();

			InSetup = true; // sync checks
			RotateAxesX.Checked = RotateAxesY.Checked = View.RotateAxes; // set check for each axis to match
			InSetup = false;
		}

		////////////////////////////////////////////////////////
		///////////////////// Y-axis Tab ///////////////////////
		////////////////////////////////////////////////////////

		private void YColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.YAxisMx.QueryColumn = YColumnSelector.QueryColumn;
			View.ConfigureRenderingControl();
		}

		private void YAxisOptions_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ConfigureRenderingControl();
		}

		private void ShowYAxisTitle_EditValueChanged(object sender, EventArgs e)
		{
			ShowAxisTitle(ShowYAxisTitle.Checked);
		}

		private void RotateAxesY_EditValueChanged(object sender, EventArgs e)
		{
			RotateAxes(RotateAxesY.Checked);
		}

		////////////////////////////////////////////////////////
		///////////////////// Z-axis Tab ///////////////////////
		////////////////////////////////////////////////////////

		private void ZColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ZAxisMx.QueryColumn = ZColumnSelector.QueryColumn;
			View.ConfigureRenderingControl();
		}

		private void ZAxisOptions_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ConfigureRenderingControl();
		}

		private void ShowZAxisTitle_EditValueChanged(object sender, EventArgs e)
		{
			ShowAxisTitle(ShowZAxisTitle.Checked);
		}

		///////////////////////////////////////////////////////
		////////////////////// Axes Tab ///////////////////////
		///////////////////////////////////////////////////////

		internal void SetupAxesFieldList()
		{
			//List<ColumnMapSx> fieldList = new List<ColumnMapSx>();

			//foreach (AxisMx axis in View.AxesMx)
			//{
			//	ColumnMapSx i = new ColumnMapSx();
			//	i.Selected = true;
			//	i.QueryColumn = axis.QueryColumn;
			//	i.ParameterName = axis.ShortName;
			//	i.Tag = axis;
			//	fieldList.Add(i);
			//}

			//AxesFieldListControl.Setup(View.BaseQuery, fieldList);

			return;
		}

		internal void GetAxesFieldList()
		{
			List<ColumnMapSx> fieldList = AxesFieldListControl.GetFields();
			View.AxesMx.Clear();
			foreach (ColumnMapSx i in fieldList)
			{
				AxisMx axis = i.Tag as AxisMx; // get any existing axis
				if (axis == null) axis = new AxisMx();
				axis.ShortName = i.ParameterName;
				axis.QueryColumn = i.QueryColumn;
				View.AxesMx.Add(axis);
			}

			return;
		}

		private void ShowAxesTitles2_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			View.ShowAxesTitles = ShowAxesTitles2.Checked;
			ShowAxesTitles.Checked = View.ShowAxesTitles;
			InSetup = false;

			View.ConfigureRenderingControl();
		}

		private void ShowAxesScaleLabels2_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			View.ShowAxesScaleLabels = ShowAxesScaleLabels2.Checked;
			ShowAxesScaleLabels.Checked = View.ShowAxesScaleLabels;
			InSetup = false;

			View.ConfigureRenderingControl();
		}


		////////////////////////////////////////////////////////
		///////////////////// Colors Tab ///////////////////////
		////////////////////////////////////////////////////////

		private void ColorBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.BuildMarkerColorDimension();
			View.SetupLegend();
			View.Refresh();
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
			View.BuildMarkerColorDimension();
			View.SetupLegend();
			View.Refresh();
			return;
		}

		//////////////////////////////////////////////////////
		///////////////////// Size Tab ///////////////////////
		//////////////////////////////////////////////////////

		private void SizeBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.BuildMarkerSizeDimension();
			View.SetupLegend();
			View.Refresh();
			return;
		}

		///////////////////////////////////////////////////////
		///////////////////// Shape Tab ///////////////////////
		///////////////////////////////////////////////////////

		private void ShapeBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.BuildMarkerShapeDimension();
			View.SetupLegend();
			View.Refresh();
			return;
		}

		///////////////////////////////////////////////////////
		///////////////////// Label Tab ///////////////////////
		///////////////////////////////////////////////////////

		private void LabelColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.MarkerLabel.QueryColumn = LabelColumnSelector.QueryColumn;
			View.BuildMarkerLabelDimension();
			View.SetupLegend();
			View.Refresh();
		}

		private void LabelsAll_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!LabelsAll.Checked) return;
			View.MarkerLabel.VisibilityMode = LabelVisibilityModeEnum.AllRows;
			View.BuildMarkerLabelDimension();
			View.Refresh();
		}

		private void LabelsSelected_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!LabelsSelected.Checked) return;
			View.MarkerLabel.VisibilityMode = LabelVisibilityModeEnum.MarkedRows;
			View.BuildMarkerLabelDimension();
			View.Refresh();
		}

		private void LabelsCenter_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!LabelsCenter.Checked) return;
			View.MarkerLabel.Position = LabelPositionEnum.Center;
			View.BuildMarkerLabelDimension();
			View.Refresh();
		}

		private void LabelsOutside_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (!LabelsOutside.Checked) return;
			View.MarkerLabel.Position = LabelPositionEnum.Outside;
			View.BuildMarkerLabelDimension();
			View.Refresh();
		}

		private void IncludeStructureInLabel_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.MarkerLabel.IncludeStructure = IncludeStructureInLabel.Checked;
			View.BuildMarkerLabelDimension();
			View.Refresh();
		}

		private void MaxLabels_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.MaxLabels = (int)MaxLabels.Value;
			View.BuildMarkerLabelDimension();
			View.Refresh();
		}

		private void BrowseBackgroundFileButton_Click(object sender, EventArgs e)
		{
			string filter =
				"Files (*.jpg, *.png, .bmp, .gif)|*.jpg; *.png; .bmp; .gif|All files (*.*)|*.*";
			string fileName = UIMisc.GetOpenFilename("File Name", BackgroundImageFileName.Text, filter, ".jpg");
			if (fileName == "") return;
			BackgroundImageFileName.Text = fileName;
		}

		///////////////////////////////////////////////////////
		////////////////////// Tooltip Tab ///////////////////////
		///////////////////////////////////////////////////////

		private void IncludeStructureInTooltip_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.MarkerTooltip.IncludeStructure = IncludeStructureInTooltip.Checked;
		}

		private void TooltipFieldListControl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.MarkerTooltip.Fields = TooltipFieldListControl.GetFields();
		}

		///////////////////////////////////////////////////////
		///////////////////// Legend Tab //////////////////////
		///////////////////////////////////////////////////////

		private void ShowLegendBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ShowLegend = ShowLegend.Checked;
			View.ChartPagePanel.ChartPanel.SetLegendVisibility();
			return;
		}

		private void LegendHorizontalAlignment_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = LegendAlignmentHorizontal.Text;
			txt = Lex.CompressCapitalizedName(txt);
			View.LegendAlignmentHorizontal =
				(DevExpress.XtraCharts.LegendAlignmentHorizontal)Enum.Parse(typeof(DevExpress.XtraCharts.LegendAlignmentHorizontal), txt);
			View.ConfigureRenderingControl();
		}

		private void LegendVerticalAlignment_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = LegendAlignmentVertical.Text;
			txt = Lex.CompressCapitalizedName(txt);
			View.LegendAlignmentVertical =
				(DevExpress.XtraCharts.LegendAlignmentVertical)Enum.Parse(typeof(DevExpress.XtraCharts.LegendAlignmentVertical), txt);
			View.ConfigureRenderingControl();
		}

		private void LegendMaxHorizontalPercentage_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = MaxHorizontalPercentage.Text.Trim();
			if (txt.EndsWith("%"))
				txt = txt.Substring(0, txt.Length - 1).Trim();
			int.TryParse(txt, out View.LegendMaxHorizontalPercentage);
			View.ConfigureRenderingControl();
		}

		private void LegendMaxVerticalPercentage_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = MaxVerticalPercentage.Text.Trim();
			if (txt.EndsWith("%"))
				txt = txt.Substring(0, txt.Length - 1).Trim();
			int.TryParse(txt, out View.LegendMaxVerticalPercentage);
			View.ConfigureRenderingControl();
		}

		private void LegendItemOrder_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = LegendItemOrder.Text;
			txt = Lex.CompressCapitalizedName(txt);
			View.LegendItemOrder =
				(DevExpress.XtraCharts.LegendDirection)Enum.Parse(typeof(DevExpress.XtraCharts.LegendDirection), txt);
			View.ConfigureRenderingControl();
		}

		//////////////////////////////////////////////////////////
		////////////////////// Surface Tab ///////////////////////
		//////////////////////////////////////////////////////////

		private void FillModeComboBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.SurfaceFillMode = (Mobius.Data.SurfaceFillMode)FillModeComboBox.SelectedIndex;
			View.ConfigureRenderingControl();
		}

		private void FrameModeComboBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.SurfaceFrameMode = (Mobius.Data.SurfaceFrameMode)FrameModeComboBox.SelectedIndex;
			View.ConfigureRenderingControl();
		}

		private void SmoothPalette_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.SmoothPalette = SmoothPalette.Checked;
			View.ConfigureRenderingControl();
		}

		private void SmoothShading_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.SmoothShading = SmoothShading.Checked;
			View.ConfigureRenderingControl();
		}

		private void SemitransparentSurface_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.SemiTransparent = SemitransparentSurface.Checked;
			View.ConfigureRenderingControl();
		}

		private void DrawFlat_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.DrawFlat = DrawFlat.Checked;
			View.ConfigureRenderingControl();
		}

		//////////////////////////////////////////////////////////
		////////////////////// Trellis Tab ///////////////////////
		//////////////////////////////////////////////////////////

		private void TrellisByRowsAndCols_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;
			View.TrellisByRowCol = TrellisByRowsAndCols.Checked;
			View.ConfigureRenderingControl();
		}

		private void TrellisColumnSelector_EditValueChanged(object sender, EventArgs e)
		{

			View.TrellisByRowCol = TrellisByRowsAndCols.Checked = true;
			View.TrellisColQc = TrellisColumnSelector.QueryColumn;
			View.ConfigureRenderingControl();
		}

		private void TrellisRowSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.TrellisByRowCol = TrellisByRowsAndCols.Checked = true;
			View.TrellisRowQc = TrellisRowSelector.QueryColumn;
			View.ConfigureRenderingControl();
		}

		private void TrellisPageSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.TrellisByRowCol = TrellisByRowsAndCols.Checked = true;
			View.TrellisPageQc = TrellisPageSelector.QueryColumn;
			////ChartView.TrellisPageIndex = 0; // reset to first page
			View.ConfigureRenderingControl();
		}

		private void TrellisByPanels_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;
			View.TrellisByFlow = TrellisByPanels.Checked;
			View.ConfigureRenderingControl();
		}

		private void TrellisFlowBySelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.TrellisByFlow = TrellisByPanels.Checked = true;
			View.TrellisFlowQc = TrellisFlowBySelector.QueryColumn;
			View.ConfigureRenderingControl();
		}

		private void TrellisManualLayout_EditValueChanged(object sender, EventArgs e)
		{
			SetTrellisManualLayoutEnableds();

			if (InSetup) return;
			View.TrellisManual = TrellisManualLayout.Checked;
			TrellisByPanels.Checked = true;
			View.ConfigureRenderingControl();
		}

		void SetTrellisManualLayoutEnableds()
		{
			TrellisMaxRows.Enabled = TrellisMaxCols.Enabled =
				 TrellisMaxRowsLabel.Enabled = TrellisMaxColsLabel.Enabled = TrellisManualLayout.Checked;
		}

		private void TrellisMaxRows_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.TrellisMaxRows = int.Parse(TrellisMaxRows.Text);
			View.ConfigureRenderingControl();
		}

		private void TrellisMaxCols_Validated(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.TrellisMaxCols = int.Parse(TrellisMaxCols.Text);
			View.ConfigureRenderingControl();
		}

		private void ShowAxesTitles_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			View.ShowAxesTitles = ShowAxesTitles.Checked;
			ShowAxesTitles2.Checked = View.ShowAxesTitles;
			InSetup = false;

			View.ConfigureRenderingControl();
		}

		private void ShowAxesScaleLabels_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			InSetup = true;
			View.ShowAxesScaleLabels = ShowAxesScaleLabels.Checked;
			ShowAxesScaleLabels2.Checked = View.ShowAxesScaleLabels;
			InSetup = false;

			View.ConfigureRenderingControl();
		}

		///////////////////////////////////////////////////////
		////////////////////// Misc Tab ///////////////////////
		///////////////////////////////////////////////////////

		private void BackgroundImageFileName_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.BackgroundImageFile = BackgroundImageFileName.Text;
			View.SetupBackgroundImage();
			View.Refresh();
		}

		private void StretchChart_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.JitterTheSameForXandY = StretchChart.Checked;
			////ChartView.SetupChartStretch();
			View.Refresh();
			return;
		}

		private void ShapeRenderingModeComboBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.ShapeRenderingMode = (Mobius.Data.ShapeRenderingMode)ShapeRenderingModeComboBox.SelectedIndex;
			////ChartView.SetupShapeRenderingMode();
			View.Refresh();
			return;
		}

		private void Jittering_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			View.JitterX = Jittering.Value;
			//ChartView.SetupJitter();
			View.Refresh();
		}

		public static Bitmap GetMarkerCircleSmall()
		{
			if (Instance == null) Instance = new ChartPropertiesDialog();
			Bitmap bm = Instance.SmallMarker16x16.Images[0] as Bitmap;
			return bm;
		}
#endif

	}
}