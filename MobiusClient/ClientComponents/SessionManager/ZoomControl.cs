using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Mobius.ClientComponents
{
	public partial class ZoomControl : DevExpress.XtraEditors.XtraUserControl
	{
		public event EventHandler EditValueChanged; // event to fire when edit value changes
		bool InSetter = false;

/// <summary>
/// Constructor
/// </summary>

		public ZoomControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Get/Set the zoom percentage
		/// </summary>

		public int ZoomPct
		{
			get
			{
				return _zoomPct;
			}

			set
			{
				_zoomPct = value;
				if (!InSetter)
				{
					InSetter = true;
					ZoomSlider.EditValue = PctValToSliderVal(_zoomPct); // ranges 0 - 100
					InSetter = false;
				}
				ZoomPctTextEdit.Text = _zoomPct + "%";

				if (EditValueChanged != null) // fire EditValueChanged event if handlers present
					EditValueChanged(this, EventArgs.Empty);
			}
		}

		int _zoomPct = 100; // private name

		private void ZoomPctTextEdit_KeyDown(object sender, KeyEventArgs e)
		{
			ShowZoomDialog();
			e.Handled = true;
			ZoomSlider.Focus();
		}

		private void ZoomPctTextEdit_MouseDown(object sender, MouseEventArgs e)
		{
			ShowZoomDialog();
			ZoomSlider.Focus();
		}

/// <summary>
/// Clicked zoom button
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void ShowZoomDialog()
		{
			int pct;

			Query q = QueriesControl.BaseQuery;
			if (q == null) return;

			string txt = ZoomPctTextEdit.Text;
			txt = txt.Replace("%", "");
			if (Lex.IsInteger(txt)) pct = int.Parse(txt);
			else pct = q.ViewScale; // use existing query value if above fails

			Point p = new Point(ZoomPctTextEdit.Left, ZoomPctTextEdit.Bottom);
			p = ZoomPctTextEdit.PointToScreen(p);
			DialogResult dr = ZoomDialog.Show(ref pct, p, null);
			if (dr == DialogResult.Cancel) return;

			ZoomPct = pct; // update zoom control display

			return;
		}

		/// <summary>
		/// Update zoom value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ZoomSlider_ValueChanged(object sender, EventArgs e)
		{
			if (InSetter) return;

			InSetter = true;
			int ctlVal = ZoomSlider.Value; // ranges 0 - 100
			ZoomPct = SliderValToPctVal(ctlVal);
			ZoomPctTextEdit.Text = ZoomPct + "%";
			InSetter = false;
			return;
		}

		static int SliderValToPctVal(int sliderVal)
		{
			int min, max;

			if (sliderVal <= 50) // 0 - 100 %
			{
				min = 10; // 10% zoom is minimum
				max = 100; // 100 % is maximum
			}
			else // 100 - 500%
			{
				sliderVal -= 50; // reduce to 0 - 50 range
				min = 100; // 100% is minimum
				max = 800; // 800 is maximum
			}

			int pctVal = (int)(min + (max - min) * (sliderVal / 50.0));
			return pctVal;
		}

		static int PctValToSliderVal(int pctVal)
		{
			int min, max;
			float pctRange = 0;

			if (pctVal <= 100) // 0 - 100 range
			{
				min = 0; // 0 is minimum value
				max = 50; // 50 is max value at midpoint
				pctRange = 100;
			}
			else // 100 - 800%
			{
				pctVal -= 100; // reduce to 0 - 900 range
				min = 50; // midpoint
				max = 100; // max slider value
				pctRange = 700;
			}

			int sliderVal = (int)(min + (max - min) * (pctVal / pctRange));
			return sliderVal;
		}

	}
}
