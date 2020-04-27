using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class NumericBinningDialog : DevExpress.XtraEditors.XtraForm
	{

		PivotGridFieldMx Field;
		PivotGridFieldMx FieldCopy; // backup copy
		PivotGridDialog PgDlg;

		public NumericBinningDialog()
		{
			InitializeComponent();
		}

/// <summary>
/// Show the panel
/// </summary>
/// <param name="f"></param>
/// <param name="location"></param>

		public void Show(
			PivotGridFieldMx f,
			Point location, 
			PivotGridDialog dialog)
		{
			string txt;

			Field = f;
			FieldCopy = new PivotGridFieldMx();
			f.CopyField(FieldCopy);
			PgDlg = dialog;

			Control c = NoInterval;
			IntervalBinCount.Text =
			FixedIntervalSize.Text =
			VariableIntervals.Text = "";

			//if (f.GroupIntervalMx == GroupBinType.FixedIntervalCount)
			//{
			//	//FixedIntervalCountOption.Checked = true;
			//	//txt = f.GroupIntervalCount.ToString();
			//	//if (f.GroupIntervalCount <= 0) txt = "3";
			//	//IntervalBinCount.Text = txt;
			//	//c = IntervalBinCount;
			//}

			//else if (f.GroupIntervalMx == GroupBinType.FixedIntervalSize)
			//{
			//	//FixedIntervalSizeOption.Checked = true;

			//	//txt = f.GroupIntervalSize.ToString();
			//	//if (f.GroupIntervalCount <= 0) txt = "100";
			//	//FixedIntervalSize.Text = txt;
			//	//c = FixedIntervalSize;
			//}

			//else if (f.GroupIntervalMx == GroupBinType.VariableIntervals)
			//{
			//	//VariableIntervalsOption.Checked = true;
			//	//VariableIntervals.Text = f.GroupVariableIntervals;
			//	//c = VariableIntervals;
			//}

			//else
			//{
			//	NoInterval.Checked = true;
			//	c = NoInterval;
			//}

			Location = location;
			Show();
			c.Focus();
			return;
		}

// Handle value change events

		private void FixedIntervalCountOption_CheckedChanged(object sender, EventArgs e)
		{
			throw new NotSupportedException();

			//if (!FixedIntervalCountOption.Checked) return;

			//Field.GroupIntervalMx = PivotGroupIntervalMx.FixedIntervalCount;
			//if (Field.GroupIntervalCount <= 0)
			//  Field.GroupIntervalCount = 3;
			//IntervalBinCount.Text = Field.GroupIntervalCount.ToString();
			//IntervalBinCount.Focus();
		}

		private void IntervalBinCount_EditValueChanged(object sender, EventArgs e)
		{
			throw new NotSupportedException();

			//if (Lex.IsInteger(IntervalBinCount.Text))
			//  Field.GroupIntervalCount = int.Parse(IntervalBinCount.Text);
		}

		private void FixedIntervalSizeOption_CheckedChanged(object sender, EventArgs e)
		{
			if (!FixedIntervalSizeOption.Checked) return;

			Field.GroupingType = GroupingTypeEnum.NumericInterval;
			if (Field.GroupIntervalNumericRange <= 0)
				Field.GroupIntervalNumericRange = 100;
			FixedIntervalSize.Text = Field.GroupIntervalNumericRange.ToString();
			FixedIntervalSize.Focus();
		}

		private void FixedIntervalSize_EditValueChanged(object sender, EventArgs e)
		{
			if (Lex.IsInteger(FixedIntervalSize.Text))
				Field.GroupIntervalNumericRange = int.Parse(FixedIntervalSize.Text);
		}

		private void VariableIntervalsOption_CheckedChanged(object sender, EventArgs e)
		{
			if (!VariableIntervalsOption.Checked) return;
			
			//Field.GroupIntervalMx = GroupBinType.VariableIntervals;
			VariableIntervals.Focus();
		}

		private void VariableIntervals_EditValueChanged(object sender, EventArgs e)
		{
			throw new NotSupportedException();

			//Field.GroupVariableIntervals = VariableIntervals.Text;
		}

		private void NoInterval_CheckedChanged(object sender, EventArgs e)
		{
			if (!NoInterval.Checked) return;

			Field.GroupingType = GroupingTypeEnum.EqualValues;
		}

		private void NumericIntervalPanelOK_Click(object sender, EventArgs e)
		{
			UpdateGrid();
			Hide();
		}

		private void NumericIntervalPanelCancel_Click(object sender, EventArgs e)
		{
			FieldCopy.CopyField(Field); // restore any changed values
			Hide();
		}

		private void PivotGridDialogNumericIntervalPopup_Enter(object sender, EventArgs e)
		{
			return;
		}

		private void PivotGridDialogNumericIntervalPopup_Leave(object sender, EventArgs e)
		{
			UpdateGrid();
			Hide();
		}

		void UpdateGrid() // update grid with new values
		{
			GridView gv = PgDlg.FieldGrid.MainView as GridView;
			//string txt = PgDlg.GroupingTypeLabel(Field);
			//gv.SetRowCellValue(PgDlg.FieldGridRow, "HeaderBinningCol", txt);
		}
	}
}