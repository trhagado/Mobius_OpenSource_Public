using Mobius.ComOps;

using DevExpress.XtraPivotGrid;
using DevExpress.Data.PivotGrid;
//using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.Utils;

using System;
using System.IO;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{

	/// <summary>
	/// Mobius extension to PivotGridField to store additional properties used by Mobius
	/// </summary>

	public class PivotGridFieldMx : PivotGridField
	{

		bool InSetup = false;

		/// <summary>
		/// Don't allow constructor to be called outside of the class
		/// </summary>

		public PivotGridFieldMx() : base()
		{
			Tag = this; // point to self
			SummaryType = PivotSummaryType.Custom; // must set to custom for event to fire
			GroupInterval = PivotGroupInterval.Custom;

			this.Options.AllowFilter = DefaultBoolean.False; // do custom filtering;
			this.Options.AllowFilterBySummary = DefaultBoolean.False; 
			return;
		}

		/// <summary>
		/// Type of binning/grouping to apply to row/column header values
		/// </summary>
		/// 
		public AggregationDef Aggregation = new AggregationDef();

		public new AggregationRole Role // abbrev for role
		{
			get { return Aggregation.Role; }
			set { Aggregation.Role = value; }
		}

		public SummaryTypeEnum SummaryTypeMx // abbrev for summary type
		{
			get { return Aggregation.SummaryType; }
			set { Aggregation.SummaryType = value; }
		}

		public GroupingTypeEnum GroupingType // abbrev for grouping type
		{
			get { return Aggregation.GroupingType; }
			set { Aggregation.GroupingType = value; }
		}

		public Decimal NumericIntervalSize // abbrev for numeric interval size
		{
			get { return Aggregation.NumericIntervalSize; }
			set { Aggregation.NumericIntervalSize = value; }
		}

		/// <summary>
		/// The associated Mobius.ClientComponents.ResultsField
		/// </summary>

		public object ResultsField
		{
			get { return _resultsField; }
			set { _resultsField = value; }
		}
		object _resultsField; // The associated Mobius.ClientComponents.ResultsField


/// <summary>
/// Vertical alignment of column labels (potential)
/// </summary>
		public VertAlignment ColumnLabelAlignment
		{
			get { return Appearance.Header.TextOptions.VAlignment; }
			set { Appearance.Header.TextOptions.VAlignment = value; }
		}
		/// <summary>
		/// Adjust DexExpress Area settings to match Mobius AggregationDef
		/// </summary>

		public void SyncDxAreaToMxRole()
		{
			if (Aggregation.Role == AggregationRole.Undefined)
				Visible = false;

			else
			{
				Area = (PivotArea)((int)Aggregation.Role - 1); // convert type
				Visible = true;
				//SetAreaPosition(Area, 1000); // (throws dx exception if called in in deserilization when field not in a control)
				//GroupInterval = PivotGroupInterval.Custom; // use custom Mobius methods (set in constructor)
				//base.SummaryType = PivotSummaryType.Custom;
			}

			return;
		}
		/// <summary>
		/// Adjust Mobius AggregationDef area to match DexExpress Area settings
		/// </summary>

		public void SyncMxRoleToDxArea()
		{
			Aggregation.Role = DxAreaToMxRole(this);
			return;
		}

		public static AggregationRole DxAreaToMxRole(PivotGridFieldBase dxField)
		{
			if (!dxField.Visible)
				return AggregationRole.Undefined;

			else
				return (AggregationRole)(((int)dxField.Area) + 1);
		}

		/// <summary>
		/// Copy field members 
		/// </summary>
		/// <param name="f"></param>
		/// <param name="f2"></param>

		public void CopyField(
		PivotGridFieldMx f2)
		{
			PivotGridFieldMx f = this;

			// PivotGridFieldMx-exclusive fields

			f2.Role = f.Role;
			f2.SummaryTypeMx = f.SummaryTypeMx;
			f2.GroupingType = f.GroupingType;
			f2.NumericIntervalSize = f.NumericIntervalSize;
			f2.ResultsField = f.ResultsField;

			// Common persisted fields 

			f2.Caption = f.Caption; // display name
			f2.UnboundFieldName = f.UnboundFieldName;
			f2.AreaIndex = f.AreaIndex;
			f2.Width = f.Width;

			// Other PivotGridField fields (need these?

			f2.ImageIndex = f.ImageIndex;
			f2.Area = f.Area;
			f2.Visible = f.Visible;
			f2.SummaryType = f.SummaryType;
			f2.SummaryDisplayType = f.SummaryDisplayType;

			f2.GroupInterval = f.GroupInterval;
			f2.GroupIntervalNumericRange = f.GroupIntervalNumericRange;

			f2.UnboundType = f.UnboundType;

			f2.ValueFormat.FormatType = f.ValueFormat.FormatType;
			f2.ValueFormat.FormatString = f.ValueFormat.FormatString;

			f2.CellFormat.FormatType = f.CellFormat.FormatType;
			f2.CellFormat.FormatString = f.ValueFormat.FormatString;

			return;
		}

	} // PivotGridFieldMx 

}
