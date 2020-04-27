using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraPivotGrid;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class NumericIntervalDialog : DevExpress.XtraEditors.XtraForm
	{
		AggregationDef AggregationDef = null; // current aggregation definition being edited

		public NumericIntervalDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Show the dialog
		/// </summary>
		/// <param name="f"></param>
		/// <param name="location"></param>
		/// <param name="dialog"></param>

		public static DialogResult ShowDialog(
			AggregationDef ad,
			Point location)
		{
			NumericIntervalDialog nfd = new NumericIntervalDialog();

			nfd.AggregationDef = ad; // save def ref for later use
			nfd.IntervalSize.Text = ad.NumericIntervalSize.ToString();
			nfd.Location = location;
			nfd.IntervalSize.Focus();
			DialogResult dr = nfd.ShowDialog();
			return dr;
		}

		private void NumericIntervalDialog_OK_Click(object sender, EventArgs e)
		{
			Decimal intervalSize;

			if (!Decimal.TryParse(IntervalSize.Text, out intervalSize) || intervalSize <= 0)
			{
				MessageBoxMx.ShowError("The interval size must be a positive number");
				IntervalSize.Focus();
				return;
			}

// Update the aggregation def

			AggregationDef ad = AggregationDef;

			if (!ad.IsGroupingType)
				ad.Role = AggregationRole.RowGrouping;

			ad.GroupingType = GroupingTypeEnum.NumericInterval;
			AggregationDef.NumericIntervalSize = intervalSize;
			DialogResult = DialogResult.OK;
			return;

			//UpdateGrid();
			//Hide();
		}

		private void NumericIntervalDialog_Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			return;

			//FieldCopy.CopyField(Field); // restore any changed values
			//Hide();
		}

		private void PivotGridDialogNumericIntervalPopup_Enter(object sender, EventArgs e)
		{
			return;
		}

		//void UpdateGrid() // update grid with new values
		//{
		//	if (PgDlg == null || PgDlg.FieldGrid == null || PgDlg.FieldGrid.MainView == null)
		//		return;

		//	string groupTypeLabel = "xxx"; // txt = PgDlg.GroupIntervalLabel(Field);

		//	GridView gv = PgDlg.FieldGrid.MainView as GridView;
		//	gv.SetRowCellValue(PgDlg.FieldGridRow, "HeaderBinningCol", groupTypeLabel);
		//}

	}
}