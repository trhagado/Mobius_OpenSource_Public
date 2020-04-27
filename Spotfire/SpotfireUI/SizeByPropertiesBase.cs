using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;
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

namespace Mobius.SpotfireClient
{
	public partial class SizeBySelectorControl : DevExpress.XtraEditors.XtraUserControl
	{
		public SpotfireViewProps SVP; // associated Spotfire View Properties
		public VisualMsx Visual; // visual we are operating with
		public SizeDimension SizeBy; // associated SizeBy object that contains size values
		public SizeAxisMsx SizeAxis; 

		//public Query Query { get { return View.BaseQuery; } }  // associated query

		public bool InSetup = false;
		public event EventHandler EditValueChanged; // event to fire when edit value changes
		public Control ControlChanged = null; // the control that was changed

		public SizeBySelectorControl()
		{ 
			InitializeComponent();
		}

/// <summary>
/// Setup
/// </summary>
/// <param name="spotfireViewProps"></param>
/// <param name="sizeAxis"></param>

		public void Setup(
			SizeAxisMsx sizeAxis,
			VisualMsx visual,
			SpotfireViewProps spotfireViewProps,
			EventHandler editValueChangedEventHandler = null)
		{
			InSetup = true;

			SVP = spotfireViewProps;
			Visual = visual;
			SizeAxis = sizeAxis;

			EditValueChanged = editValueChangedEventHandler;

			if (Lex.IsUndefined(sizeAxis.Expression))
				SizeByFixedSize.Checked = true;
			else SizeByColumn.Checked = true;

			SizeColumnSelector.Setup(SizeAxis, Visual, SVP, SizeByColumn_EditValueChanged);
			OverallSize.Value = (int)SizeAxis.FixedSize;

			InSetup = false;
			return;
		}

		private void SizeByFixedSize_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup || !SizeByFixedSize.Checked) return;

			InSetup = true;
			SizeAxis.Expression = "";
			SizeColumnSelector.Setup(SizeAxis, Visual, SVP, SizeByColumn_EditValueChanged);
			InSetup = false;

			FireEditValueChanged(SizeByFixedSize);
		}

		private void SizeByColumn_EditValueChanged(object sender, EventArgs e)
		{ // option box value changed
			return;
		}

		/// <summary>
		/// Changed the size column
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void SizeColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			//SizeByColumn.Checked = true;
			//QueryColumn qc = SizeColumnSelector.QueryColumn;
			//SizeBy.QueryColumn = qc;

			//InSetup = true;
			//if (qc == null)
			//	SizeByFixedSize.Checked = true;
			//else
			//	SizeByColumn.Checked = true;
			//InSetup = false;

			//FireEditValueChanged(SizeColumnSelector);
			return;
		}

		/// <summary>
		/// Changed the edit slider
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OverallSize_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			int size = OverallSize.Value;
			if (size == 0) size = 1; // keep in 1-100% range
			SizeAxis.FixedSize = size;
			FireEditValueChanged(OverallSize);
		}

		/// <summary>
		/// Fire the EditValueChanged event
		/// </summary>

		void FireEditValueChanged(Control controlChanged)
		{
			ControlChanged = controlChanged;

			if (EditValueChanged != null)
				EditValueChanged(this, EventArgs.Empty);
		}

	}

}
