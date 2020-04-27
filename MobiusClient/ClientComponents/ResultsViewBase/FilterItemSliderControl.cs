using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class FilterItemSliderControl : DevExpress.XtraEditors.XtraUserControl
	{
		internal QueryManager QueryManager { get { return QueryResultsControl.GetCurrentViewQm(this); } }
		DataTableManager MolTable { get { return QueryManager.DataTableManager; } }
		ColumnInfo ColInfo; // info on column being edited
		ColumnStatistics Stats; // statistics on associated column

		bool InSetup = false;

		public FilterItemSliderControl()
		{
			InitializeComponent();
		}

		public void Setup(ColumnInfo colInfo)
		{
			MobiusDataType mdtLow, mdtHigh;

			InSetup = true;

			ColInfo = colInfo; // save ref to colInfo

			Stats = colInfo.Rfld.GetStats();
			ItemFilter.Properties.Minimum = 0;
			ItemFilter.Properties.Maximum = Stats.DistinctValueList.Count + 2 - 1; // (All) on left, (Blanks) on right

			ParsedSingleCriteria psc = MqlUtil.ParseSingleCriteria(ColInfo.Qc.SecondaryCriteria); // parse criteria

			ItemFilter.Value = 0;
			ValueLabel.Text = "(All)";
			if (psc != null && psc.OpEnum == CompareOp.Eq && Stats.DistinctValueList.Count > 0)
			{
				MetaColumnType type = ColInfo.Mc.DataType;
				MobiusDataType lowVal = MobiusDataType.New(type, psc.Value);
				MobiusDataType highVal = MobiusDataType.New(type, psc.Value);

				if (MetaColumn.IsDecimalMetaColumnType(type))
				{ // adjust decimal comparison values by an epsilon
					double e = MobiusDataType.GetEpsilon(Stats.MaxValue.FormattedText);
					lowVal.NumericValue -= e;
					highVal.NumericValue += e;
				}

				for (int i1 = 0; i1 < Stats.DistinctValueList.Count; i1++)
				{
					MobiusDataType mdt = Stats.DistinctValueList[i1];
					string fTxt = mdt.FormattedText;

					if (Lex.Eq(psc.Value, fTxt) ||
						(mdt.CompareTo(lowVal) >= 0 && mdt.CompareTo(highVal) <= 0))
					{
						ItemFilter.Value = i1 + 1;
						ValueLabel.Text = Stats.DistinctValueList[i1].FormattedText;
						break;
					}
				}
			}

			else if (psc != null && psc.OpEnum == CompareOp.IsNull) // (Blanks)
			{
				ItemFilter.Value = Stats.DistinctValueList.Count + 1;
				ValueLabel.Text = "(Blanks)";
			}

			ItemFilter.Focus();
			InSetup = false;
			return;
		}

		private void ItemFilter_ValueChanged(object sender, EventArgs e)
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
			QueryColumn qc = ColInfo.Qc;
			if (ItemFilter.Value == 0)
			{
				ValueLabel.Text = "(All)";
				qc.SecondaryCriteria = qc.SecondaryCriteriaDisplay = "";
			}

			else if (ItemFilter.Value == Stats.DistinctValueList.Count + 1)
			{
				ValueLabel.Text = "(Blanks)";
				qc.SecondaryCriteria = qc.MetaColumn.Name + " is null";
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " is null";
			}

			else
			{
				MobiusDataType mdt = Stats.DistinctValueList[ItemFilter.Value - 1];
				ValueLabel.Text = mdt.FormattedText;
				string normalizedString = mdt.FormatForCriteria();
				qc.SecondaryCriteria = qc.MetaColumn.Name + " = " + Lex.AddSingleQuotes(normalizedString);
				qc.SecondaryCriteriaDisplay = qc.ActiveLabel + " = " + Lex.AddSingleQuotes(ValueLabel.Text);
			}

			qc.SecondaryFilterType = FilterType.ItemSlider;
			FilterBasicCriteriaControl.SyncBaseQuerySecondaryCriteria(qc); // sync any base query
		}

	}
}
