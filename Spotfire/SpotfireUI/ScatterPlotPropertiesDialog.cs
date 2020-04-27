using Mobius.ComOps;
using Mobius.Data;
//using Mobius.ClientComponents;

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
	public partial class ScatterPlotPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{
		static ScatterPlotPropertiesDialog Instance;
		static string CurrentTabName = "Columns";

		internal SpotfireViewProps SVP; // associated Spotfire View Properties
		internal SpotfireApiClient Api => SpotfireSession.SpotfireApiClient;

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap
		AnalysisApplicationMsx Analysis => SVP?.AnalysisApp; // the analysis

		ScatterPlotMsx V {
			get { return SVP?.Doc?.ActiveVisualReference as ScatterPlotMsx; }
			set { SVP.Doc.ActiveVisualReference = value; }
		}

		string OriginalChartState; // used to restore original state upon a cancel
		int OriginalConfigureCount = 0; // render count upon entry
		int OriginalRefreshCount = 0; // number of refreshes of the view

		//QueryResultsControl Qrc { get { return QueryResultsControl.GetQrcThatContainsControl(SVM.RenderingControl); } }

		bool InSetup = false;

		bool AxesTabChanged = false;
		bool MarkerShapesControlChanged = false;
		bool MarkerColorRulesControlChanged = false;

		public ScatterPlotPropertiesDialog()
		{
			InitializeComponent();
			Mobius.SpotfireDocument.TrellisMsx tmsx = new TrellisMsx();
		}

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			ScatterPlotMsx v,
			SpotfireViewProps svp)
		{
			Instance = new ScatterPlotPropertiesDialog();
			return Instance.ShowDialog2(v, svp);
		}

		private DialogResult ShowDialog2(
			ScatterPlotMsx v,
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

		private void ScatterPlotPropertiesDialog_Shown(object sender, EventArgs e)
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

		void SetupForm()
		{
			InSetup = true;

			ValidateViewInitialization();

			GeneralPropertiesPanel.Setup(V, EditValueChanged);

			DataPropertiesPanel.Setup(V, SVP, EditValueChanged);

			XAxisPanel.Setup(V.XAxis, V, SVP, EditValueChanged);

			YAxisPanel.Setup(V.YAxis, V, SVP, EditValueChanged);

			ColorByPanel.Setup(V.ColorAxis, V, SVP, EditValueChanged);

			ShapeByPanel.Setup(V.ShapeAxis, V, SVP, EditValueChanged);

			SizeByPanel.Setup(V.SizeAxis, V, SVP, EditValueChanged);
			
			TrellisPropertiesPanel.Setup(V, V.Trellis);

			PropertyDialogsUtil.SelectPropertyPage(CurrentTabName, TabPageSelector, Tabs);

			InSetup = false;
			return;
		}

		void ValidateViewInitialization()
		{
			if (SVP == null) throw new Exception("ViewProperties not defined");

			//SVP.ValidateSpotfireViewPropsInitialization();

			if (V == null)
				V = new ScatterPlotMsx();

			return;
		}

		/// <summary>
		/// The value of a property has been changed by the user
		/// </summary>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			SetVisualProperties();
		}

		/// <summary>
		/// Apply any changes still pending
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

		void SetVisualProperties()
		{
			GetFormValues();
			string serializedText = V.Serialize();

			//VisualMsx visMx = VisualMsx.Deserialize(serializedText, Analysis); // debug
			Api.SetVisualProperties(V.Id, serializedText);
		}

		void GetFormValues()
		{

			ScatterPlotMsx v = V; // update existing visual instance 

			// General

			GeneralPropertiesPanel.GetValues(v);

			XAxisPanel.GetValues();

			YAxisPanel.GetValues();

			// Other panels
			// --- todo ---

			if (
				MarkerColorRulesControlChanged ||
				MarkerShapesControlChanged ||
				AxesTabChanged)
			{
				if (MarkerColorRulesControlChanged)
				{
					//QueryColumn qc = SVM.MarkerColor.QueryColumn;
					//if (qc != null)
					//{ // associate rules with QueryColumn
					//	if (qc.CondFormat == null) qc.CondFormat = new CondFormat();
					//	//qc.CondFormat.Rules = View.MarkerColorBy.ColorRulesControl.GetRules();
					//	if (qc.CondFormat.Rules != null)
					//		qc.CondFormat.Rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);
					//	//SVM.ApplyMarkerColorRules();
					//}
					MarkerColorRulesControlChanged = false;
				}

				//if (MarkerShapesControlChanged)
				//{
				//  GetMarkerShapeSchemeRulesFromGrid();
				//  MarkerShapesControlChanged = false;
				//}

				if (AxesTabChanged)
				{
					//GetAxesFieldList();
				}

				//SVP.ConfigureRenderingControl();
			}

			//if (TooltipFieldListControl.Changed) // update tooltip info
			//	SVM.MarkerTooltip.Fields = TooltipFieldListControl.GetFields();

			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			// todo: restore prev state
			Visible = false;
		}

		private void ScatterPlotPropertiesDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Visible) CancelBut_Click(sender, e);
		}

		private void GeneralGroupControl_Enter(object sender, EventArgs e)
		{
		}

		private void Tabs_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
		{
			GeneralPropertiesPanel.Title.Focus();
			//ApplyButton.Visible = true;

			//if (Tabs.SelectedTabPage.Name == "ColorsTab" || Tabs.SelectedTabPage.Name == "ShapeTab" ||
			// Tabs.SelectedTabPage.Name == "AxesTab")
			//  ApplyButton.Visible = true;
			//else ApplyButton.Visible = false;
		}

		private void Tabs_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void ScatterPlotPropertiesDialog_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void ScatterPlotPropertiesDialog_Activated(object sender, EventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == 0) GeneralPropertiesPanel.Title.Focus();
		}

	}
}