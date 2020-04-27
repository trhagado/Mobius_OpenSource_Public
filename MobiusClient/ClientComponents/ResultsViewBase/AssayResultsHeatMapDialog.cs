using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
/// <summary>
/// Handle setup & creation of an Compound / assay / gene heatmap
/// </summary>

	public partial class AssayResultsHeatMapDialog : DevExpress.XtraEditors.XtraForm
	{
		static AssayResultsHeatMapDialog Instance;

		QueryResultsControl Qrc; // QueryResultsControl containg the results
		QueryManager Qm1; // QueryManager for top level query
		QueryManager Qm2; // QueryManager for derived query
		AssayResultsHeatMapView View = null;
		bool Initializing = false; // initial dialog call to define chart

		public AssayResultsHeatMapDialog()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the dialog and create and display the chart
/// </summary>
/// <param name="chartEx"></param>
/// <param name="tabName"></param>
/// <returns></returns>

		public static DialogResult ShowDialog ( 
			ResultsViewProps view,
			string tabName)
		{
			DialogResult dr;

			if (Instance == null) Instance = new AssayResultsHeatMapDialog();
			AssayResultsHeatMapDialog i = Instance;

			i.View = view as AssayResultsHeatMapView;
			i.Qrc = QueryResultsControl.GetQrcThatContainsControl(view.RenderingControl);

			if (!view.IsDefined) // initialize first time
			{
				i.Initializing = true;
				dr = i.InitializeChart();
				if (dr == DialogResult.Cancel) return dr;
			}

			else i.Initializing = false;

			i.Qm1 = view.BaseQuery.Parent.QueryManager as QueryManager; // root Qm
			i.Qm2 = view.BaseQuery.QueryManager as QueryManager; // Qm for summarized data
			i.SetupForm();
			dr = i.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

/// <summary>
/// Initialize the chart
/// </summary>

		DialogResult InitializeChart()
		{
			Qm1 = View.Qm; // assume chart initially pointing to root query manager
			DialogResult dr = Qm1.DataTableManager.CompleteRetrieval();
			if (dr == DialogResult.Cancel) return dr;
			Qm2 = null; // nothing for new query yet

			SummarizeDataUsingFormParameters(); // get initial summarized results & setup Qm2
			View.BaseQuery = Qm2.Query; // link chart to new query & manager

			QueryTable qt = Qm2.Query.Tables[0];

			// Setup for unpivoted results

			if (Qm2.DataTableManager.UnpivotedResults)
			{
				QueryColumn cidQc = qt.GetQueryColumnByNameWithException("CORP_ID");
				View.YAxisMx.QueryColumn = cidQc;

				QueryColumn assyNameQc = qt.GetQueryColumnByNameWithException("ASSY_NM");
				View.XAxisMx.QueryColumn = assyNameQc;
				View.XAxisMx.LabelAngle = -90; // vertical

				QueryColumn rsltQc = qt.GetQueryColumnByNameWithException("RSLT_VAL");
				View.MarkerColor.QueryColumn = rsltQc;
				View.MarkerColor.FieldValueContainsColor = true;

				View.UseExistingCondFormatting = true;
				View.PivotedData = false;
			}

// Setup for pivoted results

			else
			{
				throw new NotImplementedException(); // todo
			}

			View.ConfigureRenderingControl(); // render
			return DialogResult.OK;
		}

/// <summary>
/// Setup the form
/// </summary>

		void SetupForm()
		{
			if (Qm1.DataTableManager.UnpivotedResults)
			{
				ColsWithCfOnly.Checked = true;
				ByAssay.Checked = true;
				ColsWithCfOnly.Enabled = MainResultsForAll.Enabled = ByQuery.Enabled = false;
			}

			else
				ColsWithCfOnly.Enabled = MainResultsForAll.Enabled = ByQuery.Enabled = true;

			return;
		}

		private void ApplyButton_Click(object sender, EventArgs e)
		{
			SummarizeDataUsingFormParameters(); // redisplay with current parms
			return;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			SummarizeDataUsingFormParameters();
			DialogResult = DialogResult.OK;
			return;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			if (Initializing) // remove the query if this is the initial call
				QueryManager.RemoveSubQuery(Qm2.Query, false, false);

			DialogResult = DialogResult.Cancel;
			return;
		}

/// <summary>
/// Process user input, create QueryManager, Query, DataTable and build chart definition
/// </summary>

		void SummarizeDataUsingFormParameters()
		{
			AssayHeatmapProperties p = View.AssayHeatmapProperties;
			if (p == null) return;

			p.ColsToSum = ColsWithCfOnly.Checked ? ColumnsToTransform.ColsWithCondFormat : ColumnsToTransform.PrimaryResults;
			p.SumLevel = SummarizeByAssay.Checked ? TargetAssaySummarizationLevel.Assay : TargetAssaySummarizationLevel.Target;
			p.SumMethod = MeanSummarization.Checked ? SummarizationType.BioResponseMean : SummarizationType.MostPotent;

			p.PivotFormat = TargetAssayPivotFormat.QueryOrder;
			if (ByAssay.Checked) p.PivotFormat = TargetAssayPivotFormat.ByAssay;
			else if (ByTargetGene.Checked) p.PivotFormat = TargetAssayPivotFormat.ByGeneSymbol;
			else if (ByGeneFamily.Checked) p.PivotFormat = TargetAssayPivotFormat.ByGeneFamily;

			Qm2 = View.BuildAssayResultsHeatMapSubqueryResults();
			return;
		}

	}


}