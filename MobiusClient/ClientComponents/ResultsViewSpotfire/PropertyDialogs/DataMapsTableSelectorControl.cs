using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.SpotfireClient

{
	/// <summary>
	/// Control for selecting the Mobius query that provides data for the associated Spotfire data table
	/// </summary>

	public partial class DataMapsTableSelectorControl : DevExpress.XtraEditors.XtraUserControl
	{
		DataTableMapsMsx DataTableMaps; // the list Spotfire data tables and their mappings

		DataTableMapMsx CurrentMap
		{
			get => DataTableMaps?.CurrentMap;
			set => DataTableMaps.CurrentMap = value;
		}

		public event EventHandler CallerEditValueChangedHandler; // event to fire back to caller when edit value changes here

		bool InSetup = false;

		public DataMapsTableSelectorControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup the control for specified view
		/// </summary>
		/// <param name="view"></param>

		public void Setup(
			DataTableMapsMsx dataTableMaps,
			EventHandler callerEditValueChangedEventHandler = null)
		{
			InSetup = true;
			DataRow dr;
			DataColumn dc;
			LabeledUiItem li;
			string dtName = "";

			if (dataTableMaps == null) throw new ArgumentException("Data map is null");

			DataTableMaps = dataTableMaps;
			CallerEditValueChangedHandler = callerEditValueChangedEventHandler;

			ComboBoxItemCollection items = DataTablesComboBox.Properties.Items;
			items.Clear();

			foreach (DataTableMapMsx dm in DataTableMaps)
			{
				li = new LabeledUiItem(dm.SpotfireDataTable?.Name, dm);
				ComboBoxItem i = new ComboBoxItem(li);
				items.Add(i);
			}

			li = new LabeledUiItem(dataTableMaps?.CurrentMap?.SpotfireDataTable?.Name, dataTableMaps?.CurrentMap);
			DataTablesComboBox.Text = li?.ToString();

			InSetup = false;

			return;
		}


		private void DataTablesComboBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			DataTableMapMsx dtm = ((DataTablesComboBox.SelectedItem as LabeledUiItem)?.Tag) as DataTableMapMsx;
			DataTableMaps.CurrentMap = dtm;
			EditValueChanged();
		}

		/// <summary>
		/// Value of some form field has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (CallerEditValueChangedHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedHandler(this, EventArgs.Empty);
		}

	}
}
