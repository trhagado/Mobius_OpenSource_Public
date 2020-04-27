using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;
using Mobius.SpotfireClient;

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
	/// <summary>
	/// Handle setup & creation of an Compound / assay / gene heatmap
	/// </summary>
	
	public partial class TargetSummaryImageMapDialog : DevExpress.XtraEditors.XtraForm
	{
		static TargetSummaryImageMapDialog Instance;

		public TargetSummaryImageMapDialog()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the dialog & create the chart
/// </summary>
/// <param name="chartEx"></param>
/// <param name="tabName"></param>
/// <returns></returns>

		public static DialogResult ShowDialog ( 
			SpotfireViewManager viewMgr,
			string tabName)
		{
			if (Instance == null) Instance = new TargetSummaryImageMapDialog();
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}
	}
}