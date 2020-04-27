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
	public partial class TablePlotPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{
		static TablePlotPropertiesDialog Instance;
		static string CurrentTabName = "Columns";

		SpotfireViewProps SVP; // view properties associated with this dialog 
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap
		SpotfireApiClient Api => SpotfireSession.SpotfireApiClient;

		TablePlotMsx V
		{
			get => SVP?.ActiveVisual as TablePlotMsx; 
			set => SVP.ActiveVisual = value; 
		}

		string OriginalChartState; // used to restore original state upon a cancel
		int OriginalConfigureCount = 0; // render count upon entry
		int OriginalRefreshCount = 0; // number of refreshes of the view

		//QueryResultsControl Qrc { get { return QueryResultsControl.GetQrcThatContainsControl(SVP.RenderingControl); } }

		bool InSetup = false;

		bool AxesTabChanged = false;
		bool MarkerShapesControlChanged = false;
		bool MarkerColorRulesControlChanged = false;

		public TablePlotPropertiesDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			TablePlotMsx v,
			SpotfireViewProps svp)
		{
			Instance = new TablePlotPropertiesDialog();
			return Instance.ShowDialog2(v, svp);
		}

		private DialogResult ShowDialog2(
			TablePlotMsx v,
			SpotfireViewProps svp)
		{
			SVP = svp;
			V = v;

			//OriginalConfigureCount = view.ConfigureCount;
			OriginalChartState = v.Serialize();
			//OriginalRefreshCount = view.RefreshCount;

			SetupForm();

			DialogResult dr = ShowDialog(Form.ActiveForm);
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

		private void TablePlotPropertiesDialog_Shown(object sender, EventArgs e)
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

		void SetupForm()
		{
			InSetup = true;

			ValidateViewInitialization();

			GeneralPropertiesPanel.Setup(V);

			//DataMapPanel.Setup(SVM);
			
			//SelectColumnsPanel.Setup(SVP, null);

			//SetupTrellisTab();

			SortingPropertiesPanel.Setup(SVP, V.SortInfo);

			PropertyDialogsUtil.SelectPropertyPage(CurrentTabName, TabPageSelector, Tabs);

			InSetup = false;
			return;
		}

		void SelectColumnsPanel_Setup()
		{
			//List<DataColumnMsx> dataCols =  V.TableColumns.GetDataColumns();

			//DataTableMap dataMap2 = CurrentMap.CopyAndSetSelectedValues(dataCols);

			//SelectColumnsPanel.Setup(SVM, dataMap2);
			return;
		}


		void ValidateViewInitialization()
		{
			if (SVP == null) throw new Exception("ViewManager not defined");

			//SVP.ValidateSpotfireViewPropsInitialization();

			if (V == null)
				V = new TablePlotMsx();

			return;
		}
		// Setup the Data tab

		// Setup Appearance tab

		// Setup Colors tab
#if false

			ColorBySelector.Setup(SVM, SVM.MarkerColor);


			// Setup Legend tab

			ShowLegend.Checked = SVM.ShowLegend;
			LegendAlignmentHorizontal.Text = Lex.ExpandCapitalizedName(SVM.LegendAlignmentHorizontal.ToString(), true);
			LegendAlignmentVertical.Text = Lex.ExpandCapitalizedName(SVM.LegendAlignmentVertical.ToString(), true);
			MaxHorizontalPercentage.Text = SVM.LegendMaxHorizontalPercentage.ToString() + "%";
			MaxVerticalPercentage.Text = SVM.LegendMaxVerticalPercentage.ToString() + "%";
			LegendItemOrder.Text = Lex.ExpandCapitalizedName(SVM.LegendItemOrder.ToString(), true);

		InSetup = false;
			return;
	}
#endif

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
		/// Close
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void CloseButton_Click(object sender, EventArgs e)
		{
			CurrentTabName = Tabs.SelectedTabPage.Name;

			//ApplyPendingChanges();

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
			DialogResult = DialogResult.Cancel;
			return;
		}

		/// <summary>
		/// Retrieve and apply any pending marker color/shape changes
		/// </summary>

		void ApplyPendingChanges()
		{
			if (
				MarkerColorRulesControlChanged ||
				MarkerShapesControlChanged ||
				AxesTabChanged)
			{
				//if (MarkerColorRulesControlChanged)
				//{
				//	QueryColumn qc = SVM.MarkerColor.QueryColumn;
				//	if (qc != null)
				//	{ // associate rules with QueryColumn
				//		if (qc.CondFormat == null) qc.CondFormat = new CondFormat();
				//		//qc.CondFormat.Rules = View.MarkerColorBy.ColorRulesControl.GetRules();
				//		if (qc.CondFormat.Rules != null)
				//			qc.CondFormat.Rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);
				//		//SVM.ApplyMarkerColorRules();
				//	}
				//	MarkerColorRulesControlChanged = false;
				//}


				//SVM.ConfigureRenderingControl();
			}

			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			// todo: restore prev state
			Visible = false;
		}

		private void TablePlotPropertiesDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Visible) CancelBut_Click(sender, e);
		}

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			GeneralPropertiesPanel.Title.Focus();

			//if (Tabs.SelectedTabPage.Name == "ColorsTab" || Tabs.SelectedTabPage.Name == "ShapeTab" ||
			// Tabs.SelectedTabPage.Name == "AxesTab")
			//  ApplyButton.Visible = true;
			//else ApplyButton.Visible = false;
		}

		private void Tabs_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void TablePlotPropertiesDialog_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void TablePlotPropertiesDialog_Activated(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 0) GeneralPropertiesPanel.Title.Focus();
		}

		////////////////////////////////////////////////////////
		///////////////////// Sorting Tab ///////////////////////
		////////////////////////////////////////////////////////

		private void XColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//xxxSVM.XAxisMx.QueryColumn = XColumnSelector.QueryColumn;
			//SVM.ConfigureRenderingControl();
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
			//		(DevExpress.XtraCharts.LegendAlignmentHorizontal)Enum.Parse(typeof(DevExpress.XtraCharts.LegendAlignmentHorizontal), txt);
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

		private void SortingPropertiesPanel_EditValueChanged(object sender, EventArgs e)
		{
			ProcessEditValueChangedEvent();
		}

		void ProcessEditValueChangedEvent()
		{
			return; // todo - refresh
		}

	}
}