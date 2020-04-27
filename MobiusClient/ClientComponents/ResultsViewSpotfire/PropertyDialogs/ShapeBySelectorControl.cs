using Mobius.ComOps;
using Mobius.Data;
//using Mobius.SpotfireClient;

using DevExpress.XtraEditors;
//using DevExpress.XtraCharts;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.ClientComponents
{
	public partial class ShapeBySelectorControl : DevExpress.XtraEditors.XtraUserControl
	{
		public ViewManager View; // associated view
		public ShapeDimension ShapeBy; // associated ShapeBy object that contains shape values
		public Query Query { get { return View.BaseQuery; } }  // associated query

		ContextMenuStrip ShapeTypeMenu; // the menu of fixed shapes
		DataTable ShapeSchemeDataTable;

		bool InSetup = false;
		public event EventHandler EditValueChanged; // event to fire when edit value changes

		public ShapeBySelectorControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="view"></param>
		/// <param name="colorBy"></param>

		public void Setup(
			ViewManager view,
			ShapeDimension shapeBy,
			ContextMenuStrip shapeMenu)
		{
			InSetup = true;

			View = view;
			ShapeBy = shapeBy;

			ShapeTypeMenu = shapeMenu;
			SetupFixedShapeButton();
			ShapeColumnSelector.Setup(View.BaseQuery, shapeBy.QueryColumn); // setup fieldSelector
			SetupShapeSchemeGrid();

			if (ShapeColumnSelector.QueryColumn != null)
				ShapeByColumn.Checked = true;
			else ShapeByFixedShape.Checked = true;

			InSetup = false;
			return;
		}

		/// <summary>
		/// Copy the marker shape rules to the DataTable associated with the dialog grid
		/// </summary>

		internal void SetupShapeSchemeGrid()
		{
			DataTable dt = new DataTable();
			DataColumn dc = new DataColumn("Shape", typeof(int));
			dt.Columns.Add(dc);
			dc = new DataColumn("Value", typeof(string));
			dt.Columns.Add(dc);

			SetupGridImageComboBoxRepositoryItem();

			if (ShapeBy.Rules != null)
				foreach (CondFormatRule r in ShapeBy.Rules)
				{
					DataRow dr = dt.NewRow();
					dr[0] = r.ForeColor.ToArgb(); // copy shape type id
					dr[1] = r.Value;
					dt.Rows.Add(dr);
				}

			dt.RowChanged += new DataRowChangeEventHandler(DataRowChangeEventHandler);
			ShapeSchemeDataTable = dt;
			ShapeSchemeGrid.DataSource = dt;

			return;
		}

		/// <summary>
		/// Setup the image combobox for the set of possible shapes
		/// </summary>

		void SetupGridImageComboBoxRepositoryItem()
		{
			int fixedShape;

			RepositoryItemImageComboBox icb = ShapeColumn.ColumnEdit as RepositoryItemImageComboBox;
			if (icb == null) return;
			if (ShapeTypeMenu == null) return;

			icb.Items.Clear();
			icb.SmallImages = MarkerShapes16x16;
			MarkerShapes16x16.Images.Clear();

			foreach (ToolStripItem i in ShapeTypeMenu.Items)
			{
				if (Lex.Eq(i.Tag as string, "EndOfNormalShapes")) break;

				ToolStripMenuItem mi = i as ToolStripMenuItem;
				if (mi == null || !int.TryParse(mi.Tag as string, out fixedShape)) continue;

				MarkerShapes16x16.Images.Add(mi.Image);
				int ii = MarkerShapes16x16.Images.Count - 1;
				ImageComboBoxItem cbi = new ImageComboBoxItem(mi.Text, fixedShape, ii);
				icb.Items.Add(cbi);
			}

			return;
		}

		void DataRowChangeEventHandler(object sender, DataRowChangeEventArgs e)
		{
			GetShapeSchemeRulesFromGrid();

			FireEditValueChanged();
		}

		internal void GetShapeSchemeRulesFromGrid()
		{

			for (int ri = 0; ri < ShapeSchemeDataTable.Rows.Count; ri++)
			{
				DataRow dr = ShapeSchemeDataTable.Rows[ri];
				ShapeBy.Rules[ri].Value = (string)dr[1];
				ShapeBy.Rules[ri].ForeColor = Color.FromArgb((int)dr[0]);
			}

			QueryColumn qc = ShapeBy.QueryColumn;
			if (qc != null)
				ShapeBy.Rules.InitializeInternalMatchValues(qc.MetaColumn.DataType);

			return;
		}

		private void ShapeByFixedShape_EditValueChanged(object sender, EventArgs e)
		{ // option box value changed
			if (InSetup || !ShapeByFixedShape.Checked) return;

			InSetup = true;
			ShapeBy.QueryColumn = null;
			ShapeColumnSelector.Setup(View.BaseQuery, ShapeBy.QueryColumn);
			InitializeShapeRules();
			SetupShapeSchemeGrid();
			InSetup = false;

			FireEditValueChanged();
		}

		//private void FixedShape_EditValueChanged(object sender, EventArgs e)
		//{ // new fixed shape
		//  if (InSetup) return;

		//  ShapeByFixedShape.Checked = true;
		//  ShapeBy.QueryColumn = null;
		//  ShapeBy.FixedShape = // the item value contains the shape enum value
		//    (int)FixedShapeOld.Properties.Items[FixedShapeOld.SelectedIndex].Value;

		//  FireEditValueChanged();
		//}

		private void ShapeByColumn_EditValueChanged(object sender, EventArgs e)
		{ // option box value changed
			return;
		}

		private void ShapeColumnSelector_EditValueChanged(object sender, EventArgs e)
		{ // column selection changed
			if (InSetup) return;

			InSetup = true;
			ShapeBy.QueryColumn = ShapeColumnSelector.QueryColumn;
			if (ShapeBy.QueryColumn != null) ShapeByColumn.Checked = true;
			else ShapeByFixedShape.Checked = true;

			InitializeShapeRules();
			SetupShapeSchemeGrid();
			InSetup = false;

			FireEditValueChanged();
			return;
		}


		/// <summary>
		/// Initialize marker shape rules for newly selected QueryColumn
		/// </summary>

		internal void InitializeShapeRules()
		{
			CondFormatRule r;

			ShapeBy.Rules = new CondFormatRules();
			if (ShapeBy.QueryColumn == null) return;

			QueryColumn qc = ShapeBy.QueryColumn;
			ColumnStatistics stats = View.GetStats(qc);
			for (int i1 = 0; i1 < stats.DistinctValueList.Count; i1++)
			{
				MobiusDataType mdt = stats.DistinctValueList[i1];
				r = new CondFormatRule();
				r.Value = View.GetFormattedText(qc, mdt);
				AddRule(r);
				if (i1 + 1 >= 25) break; // limit number of items
			}

			if (stats.NullsExist)
			{
				r = new CondFormatRule();
				r.Value = "(Blank)";
				AddRule(r);
			}
			return;
		}

		internal void AddRule(CondFormatRule r)
		{
			int max = MaxShapeEnumValue();
			if (ShapeBy.Rules.Count <= max)
				r.ForeColor = Color.FromArgb(ShapeBy.Rules.Count);
			else r.ForeColor = Color.FromArgb(max);
			ShapeBy.Rules.Add(r);
			return;
		}

		int MaxShapeEnumValue()
		{
			int max = -1, fixedShape;
			if (ShapeTypeMenu == null) return max;

			foreach (ToolStripItem i in ShapeTypeMenu.Items)
			{
				if (Lex.Eq(i.Tag as string, "EndOfNormalShapes")) break;

				ToolStripMenuItem mi = i as ToolStripMenuItem;
				if (mi == null || !int.TryParse(mi.Tag as string, out fixedShape)) continue;

				if (fixedShape > max) max = fixedShape;
			}

			return max;
		}

		private void FixedShape_Click(object sender, EventArgs e)
		{ // show the popup of shapes
			SetupFixedShapeDropdown();
			ShapeTypeMenu.Show(FixedShapeButton,
				new Point(0, FixedShapeButton.Size.Height));
		}

		void SetupFixedShapeDropdown()
		{
			int fixedShape;

			foreach (ToolStripItem i in ShapeTypeMenu.Items)
			{
				ToolStripMenuItem mi = i as ToolStripMenuItem;
				if (mi == null || !int.TryParse(mi.Tag as string, out fixedShape)) continue;

				if (fixedShape == ShapeBy.FixedShape) mi.Checked = true;
				else mi.Checked = false;

				mi.Click += new System.EventHandler(this.FixedShape_MenuItemClicked);
			}
		}

		/// <summary>
		/// Selected shape, update properties & button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FixedShape_MenuItemClicked(object sender, EventArgs e)
		{
			int fixedShape;

			ToolStripMenuItem mi = sender as ToolStripMenuItem;
			if (mi == null || !int.TryParse(mi.Tag as string, out fixedShape)) return;

			InSetup = true;
			ShapeBy.FixedShape = fixedShape;
			SetupFixedShapeButton();

			ShapeByFixedShape.Checked = true;
			ShapeBy.QueryColumn = null;
			InSetup = false;

			FireEditValueChanged();
		}

		void SetupFixedShapeButton()
		{
			int fixedShape;

			FixedShapeButton.Text = "";
			if (ShapeTypeMenu == null) return;

			foreach (ToolStripItem i in ShapeTypeMenu.Items)
			{
				ToolStripMenuItem mi = i as ToolStripMenuItem;
				if (mi == null || !int.TryParse(mi.Tag as string, out fixedShape)) continue;

				if (fixedShape != ShapeBy.FixedShape) continue;

				FixedShapeButton.Text = mi.Text;
				FixedShapeButton.Properties.Buttons[0].Image = mi.Image;
			}
		}

		/// <summary>
		/// Fire the EditValueChanged event
		/// </summary>

		void FireEditValueChanged()
		{
			if (EditValueChanged != null)
				EditValueChanged(this, EventArgs.Empty);
		}

	}
}
