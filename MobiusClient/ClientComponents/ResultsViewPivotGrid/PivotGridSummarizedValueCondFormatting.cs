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

namespace Mobius.ClientComponents.ResultsViews
{
	public partial class PivotGridSummarizedValueCondFormatting : DevExpress.XtraEditors.XtraForm
	{
		PivotGridFieldMx Field;
		PivotGridFieldMx FieldCopy; // backup copy
		PivotGridDialog PgDlg;

		public PivotGridSummarizedValueCondFormatting()
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
#if false
			Field = f;
			FieldCopy = new PivotGridFieldMx();
			f.CopyField(FieldCopy);
			PgDlg = dialog;

			Control c;
			IntervalBinCount.Text =
			FixedIntervalSize.Text =
			VariableIntervals.Text = "";

			if (f.GroupIntervalMx == PivotGroupIntervalMx.FixedIntervalCount)
			{
				FixedIntervalCountOption.Checked = true;
				txt = f.GroupIntervalCount.ToString();
				if (f.GroupIntervalCount <= 0) txt = "3";
				IntervalBinCount.Text = txt;
				c = IntervalBinCount;
			}

			else if (f.GroupIntervalMx == PivotGroupIntervalMx.FixedIntervalSize)
			{
				FixedIntervalSizeOption.Checked = true;

				txt = f.GroupIntervalSize.ToString();
				if (f.GroupIntervalCount <= 0) txt = "100";
				FixedIntervalSize.Text = txt;
				c = FixedIntervalSize;
			}

			else if (f.GroupIntervalMx == PivotGroupIntervalMx.VariableIntervals)
			{
				VariableIntervalsOption.Checked = true;
				VariableIntervals.Text = f.GroupVariableIntervals;
				c = VariableIntervals;
			}

			else
			{
				NoInterval.Checked = true;
				c = NoInterval;
			}

			Location = location;
			Show();
			c.Focus();
			return;
#endif
		}

		private void OKButton_Click(object sender, EventArgs e)
		{
			if (!Rules.AreValid()) return;

			UpdateGrid();
			Hide();

			this.Visible = false; // hide so not called again by FormClosing event
			DialogResult = DialogResult.OK;
		}

		void UpdateGrid() // update grid with new values
		{
			//GridView gv = PgDlg.FieldGrid.MainView as GridView;
			//string txt = PgDlg.GroupIntervalLabel(Field);
			//gv.SetRowCellValue(PgDlg.FieldGridRow, "HeaderBinningCol", txt);
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			FieldCopy.CopyField(Field); // restore any changed values
			Hide();
		}

	}
}