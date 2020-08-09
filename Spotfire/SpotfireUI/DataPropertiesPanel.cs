using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
//using Mobius.ClientComponents;

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
		internal SpotfireViewProps SVP; // associated Spotfire View Properties
		internal VisualMsx Visual; 

		public event EventHandler ValueChangedCallback = null; // event to fire when edit value changes
		bool InSetup = false;

		public DataPropertiesPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		public void Setup(
			VisualMsx v,
			SpotfireViewProps svp,
			EventHandler valueChangedCallback = null)
		{
			InSetup = true;

			Visual = v;
			SVP = svp;

			DataTableMsx mainDataTable = v?.Data?.DataTableReference;

			MainDataTableSelectorControl.Setup(svp.Doc.Doc_Tables.TableList, mainDataTable, SVP, MainDataTableSelectorControl_EditValueChanged);
			ValueChangedCallback = valueChangedCallback;

			InSetup = false;
			
			return;
		}

		/// <summary>
		/// New Datatable selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void MainDataTableSelectorControl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			DataTableMsx dt = MainDataTableSelectorControl.SelectedDataTable;
			Visual.Data.DataTableReference = dt; // update document

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

			if (ValueChangedCallback != null) // fire EditValueChanged event if handlers present
				ValueChangedCallback(this, EventArgs.Empty);

			return;
		}

	}
}
