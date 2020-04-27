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
	public partial class DataPropertiesPanel : DevExpress.XtraEditors.XtraUserControl
	{

		SpotfireViewManager SVM; // view manager associated with this dialog
		SpotfireApiClient Api => SVM?.SpotfireApiClient;
		Query ViewQuery => SVM.BaseQuery;
		Query BaseQuery => SVM.BaseQuery;
		SpotfireViewProps SVP => SVM?.SVP;

		public event EventHandler CallerEditValueChangedEventHandler = null; // event to fire when edit value changes
		bool InSetup = false;

		public DataPropertiesPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		public void Setup(
			VisualMsx v,
			SpotfireViewManager svm,
			EventHandler callerEditValueChangedEventHandler = null)
		{
			DataTableMsx mainDataTable = v?.Data?.DataTableReference;
			SVM = svm;

			MainDataTableSelectorControl.Setup(mainDataTable, SVP, callerEditValueChangedEventHandler);
			CallerEditValueChangedEventHandler = callerEditValueChangedEventHandler;

			return;
		}

		public void GetValues(VisualMsx v)
		{
			InSetup = true;
			
			//v.Title = Title.Text;
			//v.ShowTitle = ShowTitle.Checked;
			//v.ShowDescription = ShowDescriptionInVis.Checked;
			//v.Description = Description.Text;

			InSetup = false;
		}

		private void MainDataTableSelectorControl_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		/// <summary>
		/// Call any EditValueChanged event
		/// </summary>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (CallerEditValueChangedEventHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedEventHandler(this, EventArgs.Empty);

			return;
		}

	}
}
