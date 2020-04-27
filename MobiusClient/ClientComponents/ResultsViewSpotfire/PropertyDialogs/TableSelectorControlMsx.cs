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
using System.Windows.Forms;
using System.Threading;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Select a Spotfire DataTable
	/// </summary>

	public partial class TableSelectorControlMsx : DevExpress.XtraEditors.XtraUserControl
	{

		public new event EventHandler Click; // event to fire when control is clicked

		public event EventHandler EditValueChanged; // event to fire when edit value changes

		private SpotfireViewProps SVP = null; // Spotfire View props
		private Query Query => SVP?.DataTableMaps?.CurrentMap?.Query;  // associated query
		private DataTableMapMsx DataMap => SVP?.DataTableMaps?.CurrentMap; // associated DataMap
		private DataTableCollectionMsx DataTables => SVP?.Doc?.Doc_Tables; // Spotfire DataTables

		DropDownButton LastDropDownButtonClicked = null;
		bool MenuItemSelected = false; // set to true if item is selected as opposed to menu is closed without selection

		public DataTableMsx SelectedDataTable = null; // Currently selected data table

		//{
		//	get { return _selectedDataTable; }

		//	set
		//	{
		//		_selectedDataTable = value;
		//		if (Lex.IsUndefined(_selectedDataTable?.Name))
		//		{
		//			TableNameDropDown.Text = "";
		//		}

		//		else
		//		{
		//			TableNameDropDown.Text = _selectedDataTable.Name; // set control text as well
		//		}
		//	}
		//}
		//DataTableMsx _selectedDataTable = null;


		/// <summary>
		/// Setup control
		/// </summary>
		/// <param name="query"></param>

		public void Setup(
			DataTableMsx selectedDataTable,
			SpotfireViewProps svp,
			EventHandler editValueChangedEventHandler = null)
		{
			SelectedDataTable = selectedDataTable;
			TableNameDropDown.Text = selectedDataTable?.Name;

			SVP = svp;

			EditValueChanged = editValueChangedEventHandler;
			
			return;
		}

		/// <summary>
		/// Basic constructor
		/// </summary>

		public TableSelectorControlMsx()
		{
			InitializeComponent();
		}

		private void TableNameDropDown_Click(object sender, EventArgs e)
		{
			LastDropDownButtonClicked = sender as DropDownButton;
			DelayedCallback.Schedule(ShowSelectTableMenu);
			return;
		}

		private void TableNameDropDown_ArrowButtonClick(object sender, EventArgs e)
		{
			LastDropDownButtonClicked = sender as DropDownButton;
			DelayedCallback.Schedule(ShowSelectTableMenu);
			return;
		}

		/// <summary>
		/// Direct call to select DataTable
		/// </summary>
		/// <returns></returns>

		public void ShowSelectTableMenu()
		{
			DropDownButton b = LastDropDownButtonClicked;
			if (b == null) return;

			Point screenLoc = PointToScreen(new Point(b.Location.X, b.Location.Y + b.Height));

			DialogResult dr = ShowSelectTableMenu(screenLoc);
			return;

			//if (dr == DialogResult.OK && SelectedDataTable != null)
			//	return SelectedDataTable;

			//else return null; // no selection
		}

		public DialogResult ShowSelectTableMenu(Point screenLoc)
		{
			if (Click != null) // fire Clicked event if handlers present
				Click(this, EventArgs.Empty);

			ToolStripMenuItem fmi = null;

			SelectTableMenu.Items.Clear();

			foreach (DataTableMsx dt in DataTables)
			{
				fmi = new ToolStripMenuItem();
				if (dt.Id == SelectedDataTable?.Id) fmi.Checked = true;

				fmi.Text = dt.Name;

				fmi.Tag = dt;
				fmi.Click += new System.EventHandler(this.TableMenuItem_Click);

				SelectTableMenu.Items.Add(fmi);
			}

			SelectTableMenu.Show(screenLoc.X, screenLoc.Y);

			MenuItemSelected = false;
			while (SelectTableMenu.Visible) // wait til menu closes
			{
				Application.DoEvents();
				Thread.Sleep(100);
			}

			if (!MenuItemSelected) return DialogResult.Cancel; // treat as cancel if nothing selected

			if (EditValueChanged != null) // fire EditValueChanged event if handlers present
				EditValueChanged(this, EventArgs.Empty);

			return DialogResult.OK;
		}

		private void SelectTableMenu_Opening(object sender, CancelEventArgs e)
		{
			return;
		}
		/// <summary>
		/// TableMenu item cliced
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void TableMenuItem_Click(object sender, EventArgs e)
		{
			SelectedDataTable = (sender as ToolStripMenuItem).Tag as DataTableMsx;
			TableNameDropDown.Text = SelectedDataTable?.Name;
			MenuItemSelected = true;
		}

	} // TableSelectorControl

}
