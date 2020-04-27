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

	public partial class QueryTableSelectorControl : DevExpress.XtraEditors.XtraUserControl
	{
		DataTableMapMsx DataTableMap;  // associated DataMap
		SpotfireViewProps SVP => DataTableMap?.SVP; // view manager associated with this dialog
		Query Query => SVP?.Query; // Mobius query that supplies data to the Spotfire view
		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps;

		public QueryTable SelectedQueryTable = null;

		public bool IsMappedToSingleQueryTable => (SelectedQueryTable != null);
		public bool IsMappedToAllQueryTables => (QueryTablesComboBox.SelectedItem == AllTablesItem);
		public bool IsMapped => (QueryTablesComboBox.SelectedItem != NoTablesItem);
		public bool IsUnmapped => (QueryTablesComboBox.SelectedItem == NoTablesItem);

		LabeledUiItem NoTablesItem = new LabeledUiItem("(None)", null);
		LabeledUiItem AllTablesItem = new LabeledUiItem("(All Tables)", null);

		public event EventHandler CallerEditValueChangedHandler; // event to fire back to caller when edit value changes here

		bool InSetup = false;

		public QueryTableSelectorControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup the control for specified view
		/// </summary>
		/// <param name="view"></param>

		public void Setup(
			DataTableMapMsx dataTableMap,
			EventHandler callerEditValueChangedEventHandler = null)
		{
			InSetup = true;
			DataTableMap = dataTableMap;

			CallerEditValueChangedHandler = callerEditValueChangedEventHandler;

			QueryTablesComboBox.Text = "";
			QueryTablesComboBox.SelectedItem = null;

			ComboBoxItemCollection items = QueryTablesComboBox.Properties.Items;
			items.Clear();

			QueryTablesComboBox.Text = "";

			AssertMx.IsNotNull(Query, "Query");

			foreach (QueryTable qtEnum in Query.Tables)
			{
				items.Add(new ComboBoxItem(new LabeledUiItem(qtEnum.ActiveLabel, qtEnum)));
			}

			items.Add(AllTablesItem);

			items.Add(NoTablesItem);

			QueryTable qt = DataTableMap?.QueryTable;
			if (dataTableMap.IsMappedToSingleQueryTable)
			{
				QueryTablesComboBox.Text = qt.ActiveLabel;
				QueryTablesComboBox.SelectedItem = new LabeledUiItem(qt.ActiveLabel, qt);
			}

			else if (dataTableMap.IsMappedToAllQueryTables) // if cols are mapped assume all tables
			{
				QueryTablesComboBox.Text = AllTablesItem.Label;
				QueryTablesComboBox.SelectedItem = AllTablesItem;
			}

			else // not mapped to any tables
			{
				QueryTablesComboBox.Text = NoTablesItem.Label;
				QueryTablesComboBox.SelectedItem = NoTablesItem;
			}

			InSetup = false;

			return;
		}

		private void DataTablesComboBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			SelectedQueryTable = ((QueryTablesComboBox.SelectedItem as LabeledUiItem)?.Tag) as QueryTable;

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

	/// <summary>
	/// Used for creating items for UI controls that contain a string label
	/// and a reference to the underlying object (similar to a Tag field)
	/// </summary>

	public class LabeledUiItem
	{
		public string Label = "";
		public object Tag = null;

		public LabeledUiItem(
			string label = "",
			object tag = null)
		{
			Label = label;
			Tag = tag;
			return;
		}

		public override string ToString()
		{
			return Label;
		}

	}
}
