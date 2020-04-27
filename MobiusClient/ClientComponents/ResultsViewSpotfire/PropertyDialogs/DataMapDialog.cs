using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using Mobius.SpotfireDocument;
using Mobius.SpotfireClient;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class DataMapDialog : DevExpress.XtraEditors.XtraForm
	{

		SpotfireViewManager SVM; // view manager associated with this dialog
		SpotfireViewProps SVP => SVM?.SVP;

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps;
		SpotfireApiClient SpotfireApiClient => SpotfireSession.SpotfireApiClient;  // API link to Spotfire session to interact with

		bool InSetup = false;

		public DataMapDialog()
		{
			InitializeComponent();

			//TableColumnList.ParameterNameCol.Visible = false;
			//BarChartColumnList.ParameterNameCol.Visible = false;
		}

		/// <summary>
		/// ShowDialog
		/// </summary>
		/// <param name="svm">Definition of the parameters of the Trellis Card view</param>
		/// <param name="initialTabName"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			SpotfireViewManager svm)
		{
			DataMapDialog dmd = new DataMapDialog();

			return dmd.ShowDialog2(svm);
		}

		private DialogResult ShowDialog2(
			SpotfireViewManager svm)
		{
			SVM = svm;
			DataMapCtl.ShowSelectedColumnCheckBoxes = false;
			DataMapCtl.Setup(SVM, EditValueChanged);

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);

			return DialogResult.OK; // always say OK even if window header close button clicked
		}

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			return;

			//if (SpotfireViewManager.ApplyChangesImmediately)
			//	ApplyPendingChanges();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

	}
}
