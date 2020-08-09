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
	public partial class SelectColumnsPanel : DevExpress.XtraEditors.XtraUserControl
	{
		SpotfireViewManager SVM; // view manager associated with this dialog
		SpotfireViewProps SVP => SVM?.SVP;
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap
		SpotfireApiClient SpotfireApiClient => SpotfireSession.SpotfireApiClient;  // API link to Spotfire session to interact with

		public event EventHandler EditValueChanged; // event to fire when edit value changes

		public SelectColumnsPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		//public void Setup(
		//	SpotfireViewManager svm,
		//	DataTableMapMsx dataMap)
		//{
		//	SVM = svm;
		//	SelectColumnsDataMapControl.Setup(SVM, dataMap);
		//	return;
		//}

		private void FieldList_EditValueChanged(object sender, EventArgs e)
		{
			if (EditValueChanged != null) // fire EditValueChanged event if handlers present
				EditValueChanged(this, EventArgs.Empty);
		}
	}
}
