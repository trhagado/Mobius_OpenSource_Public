using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
//using Mobius.ClientComponents;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class ScaleAxisPanel : DevExpress.XtraEditors.XtraUserControl
	{
		[DefaultValue("Axis")]
		public string HeaderText { get { return AxisGroupControl.Text; } set { AxisGroupControl.Text = value; } }

		[DefaultValue(false)]
		public bool MultiExpressionSelectionAllowed { get { return _multiColumnSelectionAllowed; } set { _multiColumnSelectionAllowed = value; } }
		bool _multiColumnSelectionAllowed = false;

		/// <summary>
		/// /////////////////////////////////////////////
		/// </summary>

		public ScaleAxisMsx Axis; // axis these options relate to
		public VisualMsx Visual; // visual we are operating with

		internal SpotfireViewProps SVP; // associated Spotfire View Properties

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap
		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap

		ColumnMapCollection ColumnMap => CurrentMap?.ColumnMapCollection;

		bool InSetup = false;

		public event EventHandler ValueChangedCallback; // event to fire back to caller when edit value changes here

		public ScaleAxisPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup options for an axis
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="svm"></param>

		public void Setup(
			ScaleAxisMsx axis,
			VisualMsx visual,
			SpotfireViewProps svp,
			EventHandler editValueChangedEventHandler = null)
		{
			InSetup = true;

			Axis = axis;
			Visual = visual;
			SVP = svp;
			ValueChangedCallback = editValueChangedEventHandler;

			ColumnSelector.Setup(axis, visual, svp, EditValueChanged);

// Range

			if (axis.Range.Low == null)
				RangeMin.Text = "Automatic";
			else RangeMin.Text = axis.Range.Low.ToString();

			if (axis.Range.High == null)
				RangeMax.Text = "Automatic";
			else RangeMax.Text = axis.Range.High.ToString();

			IncludeOrigin.Checked = axis.IncludeZeroInAutoZoom;
			ShowZoomSlider.Checked = Axis.ManualZoom;
			ShowGridLines.Checked = Axis.Scale.ShowGridlines;

// Labels

			ShowLabels.Checked = Axis.Scale.ShowLabels;
			if (Axis.Scale.LabelOrientation == LabelOrientationMsx.Horizontal)
				HorizontalLabels.Checked = true;
			else VerticalLabels.Checked = true;

			bool usingMaxTickLayout = (Axis.Scale.LabelLayout == ScaleLabelLayoutMsx.MaximumNumberOfTicks);

			MaxNumberOfLabels.Checked = usingMaxTickLayout;
			MaxNumberOfTicks.Enabled = usingMaxTickLayout;
			MaxNumberOfTicks.Value = Axis.Scale.MaximumNumberOfTicks;

// Scaling

			LogScale.Checked = axis.TransformType == AxisTransformTypeMsx.Log10;
			ReverseScale.Checked = axis.Reversed;

			InSetup = false;

			return;
		}

		/// <summary>
		/// GetValues from panel
		/// </summary>

		public void GetValues()
		{
			Axis.Expression = ColumnSelector.GetAxisExpression();

			//// Range

			//if (axis.Range.Low == null)
			//	RangeMin.Text = "Automatic";
			//else RangeMin.Text = axis.Range.Low.ToString();

			//if (axis.Range.High == null)
			//	RangeMax.Text = "Automatic";
			//else RangeMax.Text = axis.Range.High.ToString();

			//IncludeOrigin.Checked = axis.IncludeZeroInAutoZoom;
			//ShowZoomSlider.Checked = Axis.ManualZoom;
			//ShowGridLines.Checked = Axis.Scale.ShowGridlines;

			// Labels

			Axis.Scale.ShowLabels = ShowLabels.Checked;
			Axis.Scale.LabelOrientation = HorizontalLabels.Checked ? LabelOrientationMsx.Horizontal : LabelOrientationMsx.Vertical;

			Axis.Scale.LabelLayout = MaxNumberOfLabels.Checked ? ScaleLabelLayoutMsx.MaximumNumberOfTicks : ScaleLabelLayoutMsx.Automatic;
			Axis.Scale.MaximumNumberOfTicks = (int)MaxNumberOfTicks.Value;

			// Scaling

			Axis.TransformType = LogScale.Checked ? AxisTransformTypeMsx.Log10 : AxisTransformTypeMsx.None;
			Axis.Reversed = ReverseScale.Checked;
		}

// Range

		private void RangeMin_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void RangeMax_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void SetToCurrentRangeButton_Click(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void IncludeOrigin_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void ShowZoomSlider_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void ShowGridLines_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		// Labels

		private void ShowLabels_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void HorizontalLabels_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void VerticalLabels_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void MaxNumberOfLabels_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void MaxNumberOfTicks_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		// Scaling

		private void LogScale_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void ReverseScale_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Expression_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void AxisModeButton_Click(object sender, EventArgs e)
		{
			ContinuousAxisMenuItem.Checked = (Axis.AxisMode == AxisModeMsx.Continuous);
			CategoricalAxisMenuItem.Checked = (Axis.AxisMode == AxisModeMsx.Categorical);

			Point p = new Point(0, AxisModeButton.Height);
			AxisModeMenu.Show(AxisModeButton, p);
			AxisModeMenu.Show();
		}

		private void ContinuousAxisMenuItem_Click(object sender, EventArgs e)
		{
			Axis.AxisMode = AxisModeMsx.Continuous;
			EditValueChanged();
		}

		private void CategoricalAxisMenuItem_Click(object sender, EventArgs e)
		{
			Axis.AxisMode = AxisModeMsx.Categorical;
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

			if (ValueChangedCallback != null) // fire EditValueChanged event if handlers present
				ValueChangedCallback(this, EventArgs.Empty);
		}

		//private void SetToCurrentRangeButton_Click(object sender, EventArgs e)
		//{
		//	QueryColumn qc = Axis.QueryColumn;
		//	if (qc == null) return;
		//	Query q = qc.QueryTable.Query;
		//	if (q == null) return;
		//	QueryManager qm = q.QueryManager as QueryManager;
		//	if (qm == null || qm.DataTableManager == null) return;
		//	DataTableManager dtm = qm.DataTableManager;
		//	ColumnStatistics stats = dtm.GetStats(qc);
		//	Axis.RangeMin = RangeMin.Text = stats.MinValue.FormattedText;
		//	Axis.RangeMax = RangeMax.Text = stats.MaxValue.FormattedText;
		//	ProcessEditValueChangedEvent();
		//	return;
		//}

		//private void XIncludeOrigin_EditValueChanged(object sender, EventArgs e)
		//{
		//	Axis.IncludeOrigin = IncludeOrigin.Checked;
		//	ProcessEditValueChangedEvent();
		//}

		//private void XLogScale_EditValueChanged(object sender, EventArgs e)
		//{
		//	Axis.LogScale = LogScale.Checked;
		//	ProcessEditValueChangedEvent();
		//}

		//private void XReverseScale_EditValueChanged(object sender, EventArgs e)
		//{
		//	ProcessEditValueChangedEvent();
		//}

		//private void XShowGridLines_EditValueChanged(object sender, EventArgs e)
		//{
		//	Axis.ShowGridLines = ShowGridLines.Checked;
		//	ProcessEditValueChangedEvent();
		//}

		//private void XShowGridStrips_EditValueChanged(object sender, EventArgs e)
		//{
		//	ProcessEditValueChangedEvent();
		//}

		//void ProcessEditValueChangedEvent()
		//{
		//	if (EditValueChanged != null) // fire EditValueChanged event if handlers present
		//	{
		//		EditValueChanged(this, EventArgs.Empty);
		//	}
		//}

		//private void XShowZoomSlider_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;
		//	Axis.ShowZoomSlider = ShowZoomSlider.Checked;
		//	ProcessEditValueChangedEvent();
		//}

		//private void ShowLabels_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (InSetup) return;

		//	Axis.ShowLabels = ShowLabels.Checked;
		//	ProcessEditValueChangedEvent();
		//}

		//private void HorizontalLabels_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (!HorizontalLabels.Checked) return;

		//	Axis.LabelAngle = 0;
		//	ProcessEditValueChangedEvent();
		//}

		//private void VerticalLabels_EditValueChanged(object sender, EventArgs e)
		//{
		//	if (!VerticalLabels.Checked) return;

		//	Axis.LabelAngle = -90;
		//	ProcessEditValueChangedEvent();
		//}

		//private void LabelAngle_EditValueChanged(object sender, EventArgs e)
		//{
		//	Axis.LabelAngle = (int)LabelAngle.Value;
		//	ProcessEditValueChangedEvent();
		//}

		//private void StaggerLabels_EditValueChanged(object sender, EventArgs e)
		//{
		//	ProcessEditValueChangedEvent();
		//}

		//private void MaxNumberOfLabels_EditValueChanged(object sender, EventArgs e)
		//{
		//	ProcessEditValueChangedEvent();
		//}

		//private void RangeMin_Validating(object sender, CancelEventArgs e)
		//{
		//	string v = RangeMin.Text;
		//	if (IsValidRangeValue(v)) return;

		//	else
		//	{
		//		e.Cancel = true;
		//		RangeMin.ErrorText = "Invalid Value";
		//	}
		//}

		//private void RangeMax_Validating(object sender, CancelEventArgs e)
		//{
		//	string v = RangeMax.Text;
		//	if (IsValidRangeValue(v)) return;

		//	else
		//	{
		//		e.Cancel = true;
		//		RangeMax.ErrorText = "Invalid Value";
		//	}
		//}

		///// <summary>
		///// Return true if valid range
		///// </summary>
		///// <param name="v"></param>
		///// <returns></returns>

		//		public static bool IsValidRangeValue(string v)
		//		{
		//			double d1;

		//			if (v == null || v.Trim() == "" ||
		//				Lex.Eq(v.Trim(), "Automatic") ||
		//			Double.TryParse(v, out d1)) return true;
		//			else return false;
		//		}

		///// <summary>
		///// Parse a range string into a double value
		///// </summary>
		///// <param name="rangeString"></param>
		///// <param name="rangeVal"></param>
		///// <returns></returns>

		//		public static bool ParseRangeValue(
		//			string rangeString,
		//			out double rangeVal)
		//		{
		//			rangeVal = NullValue.NullNumber; // set default null value
		//			if (String.IsNullOrEmpty(rangeString)) return false;
		//			bool result = Double.TryParse(rangeString, out rangeVal);
		//			return result;
		//		}

		//		private void RangeMin_InvalidValue(object sender, DevExpress.XtraEditors.Controls.InvalidValueExceptionEventArgs e)
		//		{
		//			e.ExceptionMode = DevExpress.XtraEditors.Controls.ExceptionMode.DisplayError;
		//			e.ErrorText = "Invalid Value";
		//			return;
		//		}

		//		private void RangeMax_InvalidValue(object sender, DevExpress.XtraEditors.Controls.InvalidValueExceptionEventArgs e)
		//		{
		//			e.ExceptionMode = DevExpress.XtraEditors.Controls.ExceptionMode.DisplayError;
		//			e.ErrorText = "Invalid Value";
		//			return;
		//		}

		//		private void RangeMin_Validated(object sender, EventArgs e)
		//		{
		//			string txt = RangeMin.Text.Trim();
		//			if (Lex.Eq(txt, "Automatic")) txt = "";
		//			Axis.RangeMin = txt;
		//			ProcessEditValueChangedEvent();
		//		}

		//		private void RangeMax_Validated(object sender, EventArgs e)
		//		{
		//			string txt = RangeMax.Text.Trim();
		//			if (Lex.Eq(txt, "Automatic")) txt = "";
		//			Axis.RangeMax = txt;
		//			ProcessEditValueChangedEvent();
		//		}

	}
}
