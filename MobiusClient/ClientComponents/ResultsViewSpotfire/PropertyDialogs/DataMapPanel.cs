using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class DataMapPanel : DevExpress.XtraEditors.XtraUserControl
	{
		SpotfireViewManager SVM; // view manager associated with this dialog
		Query ViewQuery => SVM.BaseQuery;
		Query BaseQuery => SVM.BaseQuery;
		SpotfireViewProps SVP => SVM?.SVP;
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap
		ColumnMapCollection ColumnMap => CurrentMap?.ColumnMapCollection;

		SpotfireApiClient SpotfireApiClient => SpotfireSession.SpotfireApiClient;  // API link to Spotfire session to interact with

		//public event EventHandler EditValueChanged; // event to fire when edit value changes

		public DataMapPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		public void Setup(
			SpotfireViewManager svm,
			EventHandler editValueChangedEventHandler = null)
		{
			SVM = svm;
			DataMapControl.ShowSelectedColumnCheckBoxes = false;
			DataMapControl.Setup(SVM, editValueChangedEventHandler);
			return;
		}

		/// <summary>
		/// EditMapButton_Click (button is currently hidden since editing is always allowed)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void EditMapButton_Click(object sender, EventArgs e)
		{
			DialogResult dr = DataMapDialog.ShowDialog(SVM);
			return;
		}
	}
}
