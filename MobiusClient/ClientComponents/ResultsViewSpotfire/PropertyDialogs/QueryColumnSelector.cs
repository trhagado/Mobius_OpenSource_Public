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
	/// Select a Mobius column from the current query or optionally the Mobius Database  for mapping to Spotfire
	/// </summary>

	public partial class QueryColumnSelect : DevExpress.XtraEditors.XtraUserControl
	{
		public SelectColumnOptions Flags = new SelectColumnOptions();

		[DefaultValue(false)]
		public bool OptionSearchableFieldsOnly { get { return Flags.SearchableOnly; } set { Flags.SearchableOnly = value; } }

		[DefaultValue(false)]
		public bool OptionNongraphicFieldsOnly { get { return Flags.NongraphicOnly; } set { Flags.NongraphicOnly = value; } }

		[DefaultValue(false)]
		public bool OptionExcludeKeys { get { return Flags.ExcludeKeys; } set { Flags.ExcludeKeys = value; } }

		[DefaultValue(false)]
		public bool OptionExcludeImages { get { return Flags.ExcludeImages; } set { Flags.ExcludeImages = value; } }

		[DefaultValue(false)]
		public bool OptionAllowNumericTypesOnly { get { return Flags.ExcludeNonNumericTypes; } set { Flags.ExcludeNonNumericTypes = value; } }

		[DefaultValue(true)]
		public bool OptionIncludeNoneItem { get { return Flags.IncludeNoneItem; } set { Flags.IncludeNoneItem = value; } }

		/// <summary>
		/// if both summarized & unsummarized data exists and
		///  - true: select summarized data by default
		///  - false: show menu to allow user to select which they want
		/// </summary>

		[DefaultValue(false)]
		public bool OptionSelectSummarizedDataByDefaultIfExists { get { return _selectSummarizedDataByDefaultIfExists; } set { _selectSummarizedDataByDefaultIfExists = value; } }
		bool _selectSummarizedDataByDefaultIfExists = false;

		public new event EventHandler Click; // event to fire when control is clicked

		public event EventHandler EditValueChanged; // event to fire when edit value changes

		private SpotfireViewProps SVP = null; // Spotfire View props
		private Query Query => SVP?.DataTableMaps?.CurrentMap?.Query;  // associated query
		private DataTableMapMsx DataMap => SVP?.DataTableMaps?.CurrentMap; // associated DataMap
		private VisualMsx Visual => SVP?.ActiveVisual; // associated Visual

		/// <summary>
		/// Setup a SpotfireColumnSelectorControl
		/// </summary>
		/// <param name="query"></param>
		/// <param name="qc"></param>

		public void Setup(
			SpotfireViewProps svp,
			QueryColumn qc,
			EventHandler editValueChangedEventHandler = null)
		{
			SVP = svp;

			SelectedColumn = DataMap.GetColumnMapFromQueryColumn(qc);

			EditValueChanged = editValueChangedEventHandler;
			
			return;
		}

		/// <summary>
		/// Currently selected QueryColumn
		/// </summary>

		public QueryColumn QueryColumn
		{
			get // return the QueryColumn corresponding to the SelectedMetaColumn
			{
				if (SelectedColumn != null)
					return SelectedColumn.QueryColumn;
				else return null;
			}
		}

		/// <summary>
		/// Currently selected MetaColumn
		/// </summary>

		public ColumnMapMsx SelectedColumn
		{
			get { return _selectedColumn; }

			set
			{
				_selectedColumn = value;
				if (_selectedColumn == null || _selectedColumn.QueryColumn == null)
				{
					ColumnName.Text = ""; 
				}

				else
				{
					ColumnName.Text = _selectedColumn.QueryColumn.ActiveLabel; // set control text as well
				}
			}
		}
		ColumnMapMsx _selectedColumn = null;

		public bool MenuItemSelected = false; // set to true if item is selected as opposed to menu is closed without selection

		public bool ExtraItemSelected { get { return SelectedColumn == null && MenuItemSelected; } } // return true if the extra menu item was selected

		/// <summary>
		/// Basic constructor
		/// </summary>

		public QueryColumnSelect()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}


		/// <summary>
		/// Selected a field item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldMenuItem_Click(object sender, EventArgs e)
		{
			SelectedColumn = (sender as ToolStripMenuItem).Tag as ColumnMapMsx;
			MenuItemSelected = true;
		}

		/// <summary>
		/// Selected no item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FieldMenuItemExtra_Click(object sender, EventArgs e)
		{
			SelectedColumn = null;
			MenuItemSelected = true;
		}

		private void ColumnName_KeyDown(object sender, KeyEventArgs e)
		{
			ShowSelectColumnMenu();
			e.Handled = true;
		}

		private void ColumnName_Click(object sender, EventArgs e)
		{
			ShowSelectColumnMenu();
		}

		private void ColumnName_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
		{
			ShowSelectColumnMenu();
		}

		private void ShowSelectColumnMenu()
		{
			//this.Focus(); // be sure focus is on us rather than one of the text boxes
			//Point p = this.PointToScreen(new Point(0, this.Height));
			//ShowMenu(p);

			ShowPopupMenuTimer.Enabled = true; // handle event in timer to avoid strange mouse behavior
		}

		private void ShowPopupMenuTimer_Tick(object sender, EventArgs e)
		{

			if (!ShowPopupMenuTimer.Enabled) return;
			ShowPopupMenuTimer.Enabled = false;

			Point p = this.PointToScreen(new Point(0, this.Height));
			ShowMenu(p);

			return;
		}

		/// <summary>
		/// Direct call to select field
		/// </summary>
		/// <returns></returns>

		public QueryColumn ShowMenu(Point screenLoc)
		{
			DialogResult dr = ShowSelectColumnMenu(screenLoc);
			if (dr == DialogResult.OK && QueryColumn != null)
				return QueryColumn;

			else return null; // no selection
		}

		public DialogResult ShowSelectColumnMenu(Point screenLoc)
		{
			if (Click != null) // fire Clicked event if handlers present
				Click(this, EventArgs.Empty);

			ToolStripMenuItem fmi = null;

			SelectFieldMenu.Items.Clear();

			ColumnMapCollection cml = DataMap.BuildFilteredColumnMapList(Flags, SelectedColumn);

			foreach (ColumnMapMsx cm in cml.ColumnMapList)
			{
				QueryColumn qc = cm.QueryColumn;
				QueryTable qt = qc?.QueryTable;
				fmi = new ToolStripMenuItem();
				if (cm.Selected) fmi.Checked = true;
				else
				{
					fmi.Image = Bitmaps.Bitmaps16x16.Images[(int)qc.MetaColumn.DataTypeImageIndex];
					fmi.ImageTransparentColor = System.Drawing.Color.Cyan;
				}

				fmi.Text = cm.SpotfireColumnName;
				if (qc != null)
				{
					fmi.ToolTipText = 
						"===== Mobius Table and Column =====\r\n" +
						"Table: " + qt.ActiveLabel + "\r\n" + 
						"Column: " + qc.ActiveLabel + "\r\n" +
						"(" + qc.MetaTableDotMetaColumnName + ")";
				}

				fmi.Tag = cm;
				fmi.Click += new System.EventHandler(this.FieldMenuItem_Click);

				SelectFieldMenu.Items.Add(fmi);
			}

			if (Flags.IncludeNoneItem)
			{ // added none item
				fmi = new ToolStripMenuItem();
				fmi.Text = "(None)";
				fmi.Click += new System.EventHandler(this.FieldMenuItemExtra_Click);
				SelectFieldMenu.Items.Add(fmi);
			}

			if (!String.IsNullOrEmpty(Flags.ExtraItem))
			{ // added extra item
				fmi = new ToolStripMenuItem();
				fmi.Text = Flags.ExtraItem;
				fmi.Click += new System.EventHandler(this.FieldMenuItemExtra_Click);
				SelectFieldMenu.Items.Add(fmi);

			}

			SelectFieldMenu.Show(screenLoc.X, screenLoc.Y);

			MenuItemSelected = false;
			while (SelectFieldMenu.Visible) // wait til menu closes
			{
				Application.DoEvents();
				Thread.Sleep(100);
			}

			if (!MenuItemSelected) return DialogResult.Cancel; // treat as cancel if nothing selected

			CheckForSummarizedVersionOfMetaColumn();

			if (EditValueChanged != null) // fire EditValueChanged event if handlers present
				EditValueChanged(this, EventArgs.Empty);

			return DialogResult.OK;
		}

		/// <summary>
		/// If the user selected a field from a non-summarized table, give them the option to replace it with the field from the
		/// summarized table (if available).  Performance is better. This will also avoid extra rows as well as a cartesian product.
		/// </summary>

		private void CheckForSummarizedVersionOfMetaColumn()
		{
			return;

#if false // disabled for now
			if (SelectedColumn == null || SelectedColumn.QueryColumn == null) return;

			QueryColumn qc = SelectedColumn.QueryColumn;
			MetaColumn mc = qc.MetaColumn;
			MetaTable mt = mc.MetaTable;
			QueryTable qt = new QueryTable(mt);

			bool unsummarrizedVersionSelected = (!mt.UseSummarizedData && mt.SummarizedExists);
			if (!unsummarrizedVersionSelected) return;

			QueryTable summarizedQt = qt.AdjustSummarizationLevel(useSummarized: true);
			QueryColumn summarizedQc = summarizedQt.GetQueryColumnByName(qc.ActiveLabel);
			if (summarizedQc == null) return;

			string msg =
				"You have selected the un-summarized version of " + qc.ActiveLabel + ".\n" +
				"Using the summarized version will result in better performance and avoid unwanted extra rows.\n" +
				"It is recommended that the summarized version be used for calculated fields.\n\n" +
				"Would you like to use the summarized version instead?\n";

			DialogResult dialogResult = MessageBoxMx.Show(msg, "Summarized Data Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult == DialogResult.Yes)
				SelectedColumn = null //summarizedQc.MetaColumn;

			return;
#endif
		}

		private void SelectFieldMenu_Opening(object sender, CancelEventArgs e)
		{
			return;
		}

	} // SpotfireColumnSelectorControl

}
