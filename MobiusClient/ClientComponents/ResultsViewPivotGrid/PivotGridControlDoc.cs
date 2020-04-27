using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraPivotGrid;
using DevExpress.XtraPivotGrid.Customization;

using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.Data.PivotGrid;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// List of key classes related to the PivotGridControl with comments for Mobius developer reference use
	/// </summary>

	public class PivotGridControlDoc
	{

/// <summary>
/// Key DX classes
/// </summary>

		void PivotGridControl()
		{
			PivotGridControl pgc = new PivotGridControl(); // DX main control

			PivotGridField pgf = pgc.Fields[0];
			{
				PivotArea pga = pgf.Area; // location of field
				int pgai = pgf.AreaIndex; // position within area
				bool visible = pgf.Visible; // true if really in an Area

				PivotGroupInterval pgi = pgf.GroupInterval; // Grouping enum (Mobius uses AggregationDef.GroupingType from GroupingTypeEnum)
				int pgiNr = pgf.GroupIntervalNumericRange; // Numerical interval range (Mobius uses AggregationDef.NumericIntervalSize)

				PivotSummaryType pst = pgf.SummaryType; // Summary type (Mobius uses AggregationDef.SummaryType from SummaryTypeEnum)
			}

		}

/// <summary>
/// Key Mobius classes related to and/or extending the DX classes
/// </summary>

		void PivotGridControlMx()
		{
			PivotGridControlMx pgcMx = new PivotGridControlMx(); // Mx method extensions of the DX PivotGridControl


			PivotGridFieldMx pgfMx = pgcMx.Fields[0] as PivotGridFieldMx; // Mx extensions of the DX PivotGridField
			{
				AggregationDef ad = pgfMx.Aggregation; // Grouping and summarization for Mx field
				{
					AggregationRole ar = ad.Role; // Role of field in agg/pivot (similar to DX Area)
					SummaryTypeEnum st = ad.SummaryType; // Type of Mx summary
					GroupingTypeEnum gt = ad.GroupingType; // Type of Mx grouping
					Decimal nis = ad.NumericIntervalSize; // interval size for numeric grouping
				}

				ResultsField rfld = pgfMx.ResultsField as ResultsField; // Mobius ResultsField, QueryColumn and MetaColumn associated with field
			}

		}
	}
}
