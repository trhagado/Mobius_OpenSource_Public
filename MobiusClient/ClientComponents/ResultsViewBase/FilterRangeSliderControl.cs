using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class FilterRangeSliderControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal QueryManager QueryManager { get { return QueryResultsControl.GetCurrentViewQm(this); } }
		DataTableManager MolTable { get { return QueryManager.DataTableManager; } }
		ColumnInfo ColInfo; // info on column being edited
		ColumnStatistics Stats; // statistics on associated column

		bool InSetup = false;

		public FilterRangeSliderControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		public void Setup(ColumnInfo colInfo)
		{
			InSetup = true;

			ColInfo = colInfo; // save ref to colInfo
			QueryColumn qc = colInfo.Qc;

			Stats = colInfo.Rfld.GetStats();
			RangeFilter.Properties.Minimum = 0;
			RangeFilter.Properties.Maximum = Stats.DistinctValueList.Count - 1; 

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(ColInfo.Qc.SecondaryCriteria); // parse criteria

			TrackBarRange tbr = new TrackBarRange(0, Stats.DistinctValueList.Count - 1);
			RangeFilter.Value = tbr;

			if (psc != null && psc.OpEnum == CompareOp.Between && Stats.DistinctValueList.Count > 0) // setup prev value
			{
				MetaColumnType type = ColInfo.Mc.DataType;
				MobiusDataType lowVal = MobiusDataType.New(type, psc.Value);
				MobiusDataType highVal = MobiusDataType.New(type, psc.Value2);
				if (MetaColumn.IsDecimalMetaColumnType(type))
				{ // adjust decimal comparison values by an epsilon
					double e = MobiusDataType.GetEpsilon(Stats.MaxValue.FormattedText);
					lowVal.NumericValue += e;
					highVal.NumericValue -= e;
				}

				int lowPos = -1;
				for (int i1 = 0; i1 < Stats.DistinctValueList.Count; i1++)
				{
					MobiusDataType mdt = Stats.DistinctValueList[i1];
					string fTxt = mdt.FormattedText;

					if (mdt.CompareTo(lowVal) <= 0 || Lex.Eq(fTxt, psc.Value))
						lowPos = i1;
					else break;
				}

				int highPos = -1;
				for (int i1 = Stats.DistinctValueList.Count -1; i1 >= 0; i1--)
				{
					MobiusDataType mdt = Stats.DistinctValueList[i1];
					string fTxt = mdt.FormattedText;

					if (mdt.CompareTo(highVal) >= 0 || Lex.Eq(fTxt, psc.Value2))
						highPos = i1;
					else break;
				}

				if (lowPos >= 0 && highPos >= 0)
				{
					tbr = new TrackBarRange(lowPos, highPos);
					RangeFilter.Value = tbr;
				}
			}

			UpdateLabels();

			RangeFilter.Focus();
			InSetup = false;
			return;
		}

/// <summary>
/// When slider is moved update criteria & filtering
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void RangeFilter_ValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			UpdateSecondaryCriteria();
			QueryManager.QueryResultsControl.UpdateFiltering(ColInfo);
			return;
		}

/// <summary>
/// Update the secondary criteria to match the slider
/// </summary>

		void UpdateSecondaryCriteria()
		{
			string lowCriteriaDisplayText, lowCriteriaText, highCriteriaDisplayText, highCriteriaText;

			QueryColumn qc = ColInfo.Qc;
			UpdateLabels(out lowCriteriaDisplayText, out lowCriteriaText, out highCriteriaDisplayText, out highCriteriaText);

			qc.SecondaryCriteria = qc.MetaColumn.Name + " between " + Lex.AddSingleQuotes(lowCriteriaText) +
				" and " + Lex.AddSingleQuotes(highCriteriaText);

			qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " between " + Lex.AddSingleQuotes(lowCriteriaDisplayText) +
				" and " + Lex.AddSingleQuotes(highCriteriaDisplayText);

			qc.SecondaryFilterType = FilterType.RangeSlider;
			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(qc); // sync any base query
		}

/// <summary>
/// Update the labels to reflect the current selected range of the control
/// </summary>

		void UpdateLabels()
		{
			string lowCriteriaDisplayText, lowCriteriaText, highCriteriaDisplayText, highCriteriaText;
			UpdateLabels(out lowCriteriaDisplayText, out lowCriteriaText, out highCriteriaDisplayText, out highCriteriaText);
		}

/// <summary>
/// Update the labels to reflect the current selected range of the control
/// </summary>
/// <param name="lowCriteriaDisplayText"></param>
/// <param name="lowCriteriaText"></param>
/// <param name="highCriteriaDisplayText"></param>
/// <param name="highCriteriaText"></param>

		void UpdateLabels(
			out string lowCriteriaDisplayText, 
			out string lowCriteriaText, 
			out string highCriteriaDisplayText, 
			out string highCriteriaText)
		{
			QualifiedNumber qn;

			lowCriteriaDisplayText = lowCriteriaText = highCriteriaDisplayText = highCriteriaText = "";
			if (ColInfo == null || Stats == null) return;
			QueryColumn qc = ColInfo.Qc;

			if (Stats.DistinctValueList.Count > 0)
			{
				MobiusDataType mdtLow = Stats.DistinctValueList[RangeFilter.Value.Minimum];
				MobiusDataType mdtHigh = Stats.DistinctValueList[RangeFilter.Value.Maximum];
				lowCriteriaDisplayText = LowValueLabel.Text = mdtLow.FormattedText;
				lowCriteriaText = mdtLow.FormatForCriteria();
				highCriteriaDisplayText = HighValueLabel.Text = mdtHigh.FormattedText;
				highCriteriaText = mdtHigh.FormatForCriteria();

				if (qc.MetaColumn.DataType == MetaColumnType.QualifiedNo)
				{
					qn = (QualifiedNumber)mdtLow;
					lowCriteriaDisplayText = QualifiedNumber.FormatNumber(qn.NumberValue, qc.ActiveDisplayFormat, qc.ActiveDecimals); // get basic QN textstats
					qn = (QualifiedNumber)mdtHigh;
					highCriteriaDisplayText = QualifiedNumber.FormatNumber(qn.NumberValue, qc.ActiveDisplayFormat, qc.ActiveDecimals);
				}
			}
			else lowCriteriaDisplayText = highCriteriaDisplayText = ""; // no values

			LowValueLabel.Text = lowCriteriaDisplayText;
			HighValueLabel.Text = highCriteriaDisplayText;

// Update label position and size

			int width = RangeFilter.Width - 10; // decrease for overhangs
			Size labelSize = new Size(width / 2, LowValueLabel.Height);
			LowValueLabel.Size = labelSize;

			HighValueLabel.Left = RangeFilter.Left + RangeFilter.Width / 2;
			HighValueLabel.Size = labelSize;

			return;
		}


		private void FilterRangeControl_Resize(object sender, EventArgs e)
		{
			UpdateLabels();
		}

/// <summary>
/// Allow low value to be manually entered
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void LowValueLabel_Click(object sender, EventArgs e)
		{
			return; // todo
		}

/// <summary>
/// Allow high value to be manually entered
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void HighValueLabel_Click(object sender, EventArgs e)
		{
			return; // todo
		}


	}
}
