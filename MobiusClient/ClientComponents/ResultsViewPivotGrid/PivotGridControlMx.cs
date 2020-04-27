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
/// Mobius extensions of the DevExpress.XtraPivotGrid.PivotGridControl
/// </summary>

	public partial class PivotGridControlMx : PivotGridControl
	{
		internal PivotGridView View; // the view that projects onto this control

		public PivotGridControlMx()
		{
			InitializeComponent();

			OptionsCustomization.CustomizationFormStyle = CustomizationFormStyle.Excel2007; // fancier format for customization
		}

/// <summary>
/// Destroy & redisplay customization
/// </summary>

		public void RedisplayCustomization()
		{

			bool showCustomization = false;
			if (showCustomization)
			{
				DestroyCustomization(); // hides the form
				Point showPoint = CustomizationFormBounds.Location;
				FieldsCustomization(showPoint); // Invokes the customization form at the specified point
			}
		}

/// <summary>
/// Set default summary type 
/// </summary>
/// <param name="f"></param>

		internal static void SetDefaultFieldSummaryType(
			PivotGridFieldMx f)
		{
			ResultsField rfld = f.ResultsField as ResultsField;
			if (rfld == null) return;
			MetaColumn mc = rfld.MetaColumn;

			if (mc.IsNumeric)
			{
				f.SummaryTypeMx = SummaryTypeEnum.ArithmeticMean;
			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				f.SummaryTypeMx = SummaryTypeEnum.Count;
			}

			else
			{
				f.SummaryTypeMx = SummaryTypeEnum.Count;
			}
		}

/// <summary>
/// Set field caption
/// </summary>
/// <param name="f"></param>

		public static void SetFieldCaption(
			PivotGridFieldMx f)
		{
			string caption = BuildFieldCaption(f);
			f.Caption = caption;
		}

		public static string BuildFieldCaption(
			PivotGridFieldMx f)
		{
			ResultsField rfld = f.ResultsField as ResultsField;
			if (rfld == null) return "Unknown";
			QueryColumn qc = rfld.QueryColumn;
			ResultsTable rt = rfld.ResultsTable;
			ResultsFormat rf = rt.ResultsFormat;
			AggregationDef ad = f.Aggregation;

			string caption = qc.GetActiveLabel(includeAggregationType:false);
			if (rf.Tables.Count > 1)
				caption = "T" + (rt.Position + 1) + "." + caption;

			if (f.Role == AggregationRole.DataSummary)
			{
				if (!ad.IsGroupingType) caption += " " + ad.TypeLabel;
			}

			f.Caption = caption;

			return caption;
		}


		/// <summary>
		/// Set the visibility of the filter header based on whether
		/// </summary>

		public void SetFilterHeaderVisibility()
		{
			foreach (PivotGridFieldMx f in Fields)
			{
				if (!f.Visible) continue;
				if (f.Area != PivotArea.FilterArea) continue;
				if (!OptionsView.ShowFilterHeaders)
					OptionsView.ShowFilterHeaders = true;
				return;
			}

			if (OptionsView.ShowFilterHeaders)
				OptionsView.ShowFilterHeaders = false;
			return;
		}

	}
}
