using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class TargetSummaryDialog : DevExpress.XtraEditors.XtraForm
	{
		static TargetSummaryDialog Instance;
		internal ViewManager ResultsView; // the view associated with the dialog

		public TargetSummaryDialog()
		{
			InitializeComponent();
		}

		public static DialogResult ShowDialog(
			ViewManager view)
		{
			if (Instance == null) Instance = new TargetSummaryDialog();
			TargetSummaryDialog i = Instance;
			i.ResultsView = view;
			i.Setup();

			i.Text = view.Title;
			//if (view.ViewType == ViewTypeMx.TargetSummaryPivoted ||
			// view.ViewType == ViewTypeMx.TargetSummaryImageMap ||
			// view.ViewType == ViewTypeMx.Heatmap)
			//	i.AdvancedButton.Enabled = true;

			//else i.AdvancedButton.Enabled = false;

			DialogResult dr = i.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}


		void Setup()
		{
			if (ResultsView.TargetSummaryOptions == null)
				ResultsView.TargetSummaryOptions = TargetSummaryOptionsControl.GetPreferences();

			TargetSummaryOptionsControl.SetFormValues(ResultsView.TargetSummaryOptions, true);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (!TargetSummaryOptionsControl.GetFormValues(ResultsView.TargetSummaryOptions)) return;

			TargetSummaryOptionsControl.SavePreferences(ResultsView.TargetSummaryOptions);

			ResultsView.ConfigureRenderingControl(); // update the view

			DialogResult = DialogResult.OK;
			return;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void AdvancedButton_Click(object sender, EventArgs e)
		{

			//if (ResultsView.ViewType == ViewTypeMx.TargetSummaryPivoted)
			//	PivotGridDialog.ShowDialog(ResultsView);

			//else if (ResultsView.ViewType == ViewTypeMx.TargetSummaryImageMap ||
			// ResultsView.ViewType == ViewTypeMx.Heatmap)
				; // ChartPropertiesDialog.ShowDialog(ResultsView as ChartViewBubble);

			return;
		}


	}
}