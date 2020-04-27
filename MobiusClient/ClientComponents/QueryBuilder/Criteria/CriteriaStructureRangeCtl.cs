using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaStructureRangeCtl : UserControl
	{
		[DefaultValue("(+)")]
		public String RangeSuffix { get { return _rangeSuffix;  }
		set { _rangeSuffix = value; SetRangeLabel(TrackBar, _rangeSuffix, RangeLabel); }
		}
		string _rangeSuffix = "";

		[DefaultValue("")]
		public String ToolTipMx
		{
			get { return TrackBar.ToolTip; }
			set { TrackBar.ToolTip = value; }
		}

		public RangeParm RangeParm; // range associated with this control
		public event EventHandler EditValueChanged; // event to fire when edit value changes

		/// <summary>
		/// Constructor
		/// </summary>

		public CriteriaStructureRangeCtl()
		{
			InitializeComponent();
			return;
		}

		public void Set(
			RangeParm rangeParm)
		{
			RangeParm = rangeParm;
			if (rangeParm != null)
				SetRange(TrackBar, _rangeSuffix, RangeLabel, BarLabel, rangeParm);
		}

		public static void SetRange(
			RangeTrackBarControl bar,
			string labelSuffix,
			LabelControl labelCtl,
			LabelControl disabledCtl,
			RangeParm range)
		{
			if (range.Active)
			{
				bar.Value = new TrackBarRange(range.Low, range.High);
				bar.Visible = true;
				bar.Enabled = range.Enabled;

				labelCtl.Visible = true;
				SetRangeLabel(bar, labelSuffix, labelCtl);

				if (disabledCtl != null)
				{
					disabledCtl.Text = "Fixed";
					disabledCtl.Visible = !range.Enabled;
				}
			}

			else
			{
				bar.Value = new TrackBarRange(range.Low, range.High); // set bar values so reappear when reactivated
				bar.Visible = false;
				labelCtl.Visible = false;

				if (disabledCtl != null)
				{
					disabledCtl.Text = "Disabled";
					disabledCtl.Visible = true;
				}

			}

			return;
		}

		public static void SetRangeLabel(
			RangeTrackBarControl bar,
			string label,
			LabelControl labelCtl)
		{
			TrackBarRange r = bar.Value;
			string txt = "";
			txt += r.Minimum.ToString();
			if (r.Maximum != r.Minimum) txt += "-" + r.Maximum;
			if (Lex.IsDefined(label)) txt += "  " + label;
			labelCtl.Text = txt;
			return;
		}

		public RangeParm GetRange()
		{
			return GetRange(TrackBar);
		}

		RangeParm GetRange(
			RangeTrackBarControl bar)
		{
			RangeParm range = new RangeParm(bar.Value.Minimum, bar.Value.Maximum);
			range.Active = bar.Visible;
			range.Enabled = bar.Enabled;
			return range;
		}

		private void Range1_EditValueChanged(object sender, EventArgs e)
		{
			SetRangeLabel(TrackBar, _rangeSuffix, RangeLabel);

			if (EditValueChanged != null) // fire EditValueChanged event if handlers present
				EditValueChanged(this, EventArgs.Empty);

		}

	}
}
