using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraPivotGrid;
using DevExpress.XtraPivotGrid.Customization;
using DevExpress.XtraPivotGrid.Data;
using DevExpress.XtraPivotGrid.FilterDropDown;
using DevExpress.XtraPivotGrid.Frames;
using DevExpress.XtraPivotGrid.Printing;
using DevExpress.XtraPivotGrid.Selection;
using DevExpress.XtraPivotGrid.TypeConverters;
using DevExpress.XtraPivotGrid.Utils;
using DevExpress.XtraPivotGrid.ViewInfo;

using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.Data.PivotGrid;
//using DevExpress.XtraCharts;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// View information for pivot table
/// </summary>

	public class PivotGridView : ViewManager
	{
		internal bool DataIncludesDates = false; // true if the data includes data columns

		// Other class vars

		internal PivotGridPageControl PageControl; // the page control allocated to the view 
		internal PivotGridPanel PivotGridPanel { get { return PageControl != null ? PageControl.PivotGridPanel : null; } }
		internal PivotGridControlMx PivotGridCtl { get { return PivotGridPanel != null ? PivotGridPanel.PivotGrid : null; } } // the control
		//internal ChartControl Chart { get { return PivotGridPanel != null ? PivotGridPanel.Chart : null; } } // the associated chart control

		internal static int UnboundFieldNameCount = 0; // assign unique unbound field names

		/// <summary>
		/// Basic constructor
		/// </summary>

		public PivotGridView()
		{
			ViewType = ResultsViewType.PivotGrid;
			Title = "Pivot View";
			PivotGridPropertiesMx = new PivotGridPropertiesMx();
		}

		/// <summary>
		/// Return true if the view has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				PivotGridPropertiesMx p = PivotGridPropertiesMx;
				if (p == null || p.PivotFields != null && p.PivotFields.Count > 0) return true;
				else return false;
			}
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			PageControl = new PivotGridPageControl();
			PageControl.View = this; // link the rendering control to us
			PivotGridPanel.PivotGrid.Visible = false; // control is initially hidden
			PivotGridPanel.Dock = DockStyle.Fill;

			RenderingControl = PivotGridPanel;
			ConfigureCount = 0;

			return RenderingControl;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel viewPanel)
		{
			viewPanel.Controls.Clear(); // remove any control current in the panel
			viewPanel.Controls.Add(PivotGridPanel); // add our rendering control to the display panel

			InsertToolsIntoDisplayPanel();

			PageControl.EditQueryBut.Enabled =
				ViewManager.IsControlContainedInQueriesControl(viewPanel) ||
				ViewManager.IsCustomExitingQueryResultsCallbackDefined(viewPanel);

			return;
		}

		/// <summary>
		/// Insert tools into display panel
		/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			Qrc.SetToolBarTools(PageControl.ToolPanel, -1); // show the proper tools
			return;
		}

		/// <summary>
		/// Show the properties for the view
		/// </summary>
		/// <returns></returns>

		public override DialogResult ShowInitialViewPropertiesDialog()
		{
			SyncMxFieldListWithSourceQuery(BaseQuery, ref PivotGridPropertiesMx.PivotFields);

			DialogResult dr = PivotGridDialog.ShowDialog(this);
			return dr;
		}

		/// <summary>
		/// Synchronize the Mobius pivot view field list with the source query fields
		/// </summary>
		/// <param name="query"></param>
		/// <param name="fieldsMx">The persisted PivotGridProperties.PivotFields for the Mobius view</param>

		public static void SyncMxFieldListWithSourceQuery(
			Query query,
			ref List<PivotGridFieldMx> fieldsMx)
		{
			PivotGridFieldMx f = null;
			int fi;

			Query q = query;
			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null) return;

			if (fieldsMx == null) fieldsMx = new List<PivotGridFieldMx>();
			foreach (PivotGridFieldMx f0 in fieldsMx)
				f0.ResultsField = null;

			// Add any fields in the source that aren't in the list

			foreach (ResultsTable rt in qm.ResultsFormat.Tables)
			{
				QueryTable qt = rt.QueryTable;
				MetaTable mt = qt.MetaTable;

				foreach (ResultsField rfld in rt.Fields)
				{
					Mobius.Data.QueryColumn qc = rfld.QueryColumn;
					MetaColumn mc = qc.MetaColumn;

					string ufn = qt.Alias + "." + mc.Name + "."; // unbound name without unique id suffix
					if (mc.Name == "ACTIVITY_CLASS") mc = mc; // debug

					List<PivotGridFieldMx> matches = new List<PivotGridFieldMx>(); // list of pivot grid fields that match fields in the incoming results set
					foreach (PivotGridFieldMx f0 in fieldsMx)
					{ // see if have already
						if (Lex.StartsWith(f0.UnboundFieldName, ufn))
						{
							f0.ResultsField = rfld; // store associated results field
							matches.Add(f0);
						}
					}

					if (matches.Count > 0) continue;

					f = AddField(rfld, fieldsMx, null, GroupingTypeEnum.EqualValues);
				}
			}

			// Remove any fields in the list not in the source

			fi = 0;
			while (fi < fieldsMx.Count)
			{
				f = fieldsMx[fi];
				if (f.ResultsField == null) fieldsMx.Remove(f); // if no resultsField then not seen in source query
				else fi++;
			}

			//if (Ctl.Groups.Count > 0) // debug
			//{ 
			//  g = Ctl.Groups[0];
			//  PivotGroupFields flds = (PivotGroupFields)g.Fields;
			//  int fCnt = flds.Count;
			//}

			return;
		}

		/// <summary>
		/// Add a new field associated with specified ResultsField
		/// </summary>
		/// <param name="rfld"></param>
		/// <returns></returns>

		internal static PivotGridFieldMx AddField(
			ResultsField rfld,
			List<PivotGridFieldMx> Fields,
			PivotGridGroup group,
			GroupingTypeEnum pgi)
		{
			return AddField(rfld, Fields, PivotArea.RowArea, false, group, pgi);
		}

		/// <summary>
		/// Add a new field associated with specified ResultsField
		/// </summary>
		/// <param name="rfld"></param>
		/// <param name="Fields"></param>
		/// <param name="area"></param>
		/// <param name="visible"></param>
		/// <param name="pgi"></param>
		/// <returns></returns>

		internal static PivotGridFieldMx AddField(
			ResultsField rfld,
			List<PivotGridFieldMx> Fields,
			PivotArea area,
			bool visible,
			PivotGridGroup group,
			GroupingTypeEnum pgi)
		{
			QueryColumn qc = rfld.QueryColumn;
			MetaColumn mc = qc.MetaColumn;
			QueryTable qt = qc.QueryTable;

			PivotGridFieldMx f = new PivotGridFieldMx();

			if (mc.IsKey) f.ImageIndex = (int)Bitmaps16x16Enum.Key;
			else f.ImageIndex = (int)mc.DataTypeImageIndex;

			f.UnboundFieldName = qt.Alias + "." + mc.Name + // identify by tableAlias.mcName (allows multiple instances of same metatable in query)
			 "." + (UnboundFieldNameCount++); // and make unique in case used in multiple PivotGroupIntervals (needed?)

			f.ResultsField = rfld; // store associated results field
			f.SummaryTypeMx = SummaryTypeEnum.Count;
			f.Area = area;
			f.Visible = visible; // if not visible then put in list of unused fields 
			f.Aggregation.GroupingType = pgi;

			PivotGridControlMx.SetFieldCaption(f);

			Fields.Add(f);

			if (group != null) group.Add(f); // add to group

			return f;
		}

		/// <summary>
		/// Render the view
		/// </summary>
		/// <param name="queryResultsControl"></param>

		public override void ConfigureRenderingControl()
		{
			if (!BuildUnpivotedResults(false)) return;

			PivotGridPropertiesMx p = PivotGridPropertiesMx;
			if (p == null) return;

			SyncMxFieldListWithSourceQuery( // be sure main grid & any pivot view fields are in synch
				BaseQuery, ref p.PivotFields); // be sure 

			if (p.PivotFields == null) // if not defined then show dialog to allow user to define
				PivotGridDialog.ShowDialog(this);

			AssureQueryManagerIsDefined(BaseQuery); 
			ConfigurePivotGridControl(); // configure the control to display the data

			PivotGridCtl.DataSource = Qm.DataTableManager.DataSource; // set the data table to start rendering

			PivotGridCtl.Visible = true; // be sure the control is visible
			ConfigureCount++;

			return;
		}

		/// <summary>
		/// Build the data table of unpivoted results
		/// </summary>
		/// <param name="includeActivityClass"></param>
		/// <returns></returns>

		internal bool BuildUnpivotedResults(
				bool includeActivityClass)
		{
			int fi;

			DialogResult dr = Qm.DataTableManager.CompleteRetrieval();
			if (dr == DialogResult.Cancel) return false;

			DataIncludesDates = false;

			int tables = 0, tablesWithDates = 0; // see what percentage of tables have dates
			foreach (ResultsTable rt in Rf.Tables)
			{
				for (fi = 0; fi < rt.Fields.Count; fi++)
				{
					if (fi == 1) tables++; // count if more that just key field
					ResultsField rfld = rt.Fields[fi];
					if (rfld.MetaColumn.DataType == MetaColumnType.Date)
					{
						tablesWithDates++;
						break;
					}
				}

				if (fi < rt.Fields.Count) break;
			}

			if (tables > 0 && (float)tablesWithDates / tables >= .5)
				DataIncludesDates = true;

			SetupActivityClassCondFormat();

			if (Math.Sqrt(4) == 4) // todo: determine when to do this
			{
				AssayHeatmapProperties p = AssayHeatmapProperties;
				OutputDest outputDest = OutputDest.WinForms;
				QueryManager sqm = Qm.DataTableManager.Summarize(p.SumLevel, p.SumMethod, p.ColsToSum, outputDest, null);
			}

			//string name = rootQuery.UserObject.Name; // (don't change name of base query)
			//if (!Lex.StartsWith(name, "Pivot View of "))
			//  name = "Pivot View of " + name;
			//Qm.Query.UserObject.Name = name;

			return true;
		}

		/// <summary>
		/// Analyze the set of CFs and create a column reflecting the cf assignment for each column
		/// </summary>

		void SetupActivityClassCondFormat()
		{
			CondFormat cf;

			// If the standard unpivoted table is the source then use the standard set of rules

			SetupActivityClassCondFormat(UnpivotedAssayView.UnsummarizedMetaTableName);
			SetupActivityClassCondFormat(UnpivotedAssayView.SummarizedMetaTableName);
			SetupActivityClassCondFormat(MultiDbAssayDataNames.CombinedNonSumTableName);

			// Specific conditional formatting for an assay overrides the default CF

			Query q = Qm.Query;
			List<Mobius.Data.QueryColumn> cfQcList = q.GetSelectedCondFormatColumns();

			List<StringMx> cfVals = new List<StringMx>(); // list of cf names & assoc colors

			foreach (Mobius.Data.QueryColumn qc in cfQcList)
			{
				cf = qc.CondFormat;
				if (cf == null || cf.Rules.Count == 0) continue;

				// todo...
			}
		}

/// <summary>
/// SetupActivityClassCondFormat 
/// </summary>
/// <param name="tableName"></param>

		void SetupActivityClassCondFormat(string tableName)
		{
			QueryTable qt = BaseQuery.GetQueryTableByName(tableName);
			if (qt == null) return;

			QueryColumn qc = qt.GetQueryColumnByName("ACTIVITY_CLASS");
			if (qc != null && qc.Selected)
			{
				qc.CondFormat = UnpivotedAssayResult.BuildActivityClassCondFormat();
			}

			qc = qt.GetQueryColumnByName("ACTIVITY_BIN");
			if (qc != null && qc.Selected)
			{
				qc.CondFormat = UnpivotedAssayResult.BuildActivityClassCondFormat();
			}

			return;
		}

		/// <summary>
		/// Create and configure the PivotGridFields for the grid control
		/// </summary>

		internal void ConfigurePivotGridControl()
		{
			PivotGridCtl.View = this;
			PivotGridCtl.DataSource = null; // be sure source is clear while configuring
			PivotGridCtl.HeaderImages = Bitmaps.Bitmaps16x16;

			PivotGridCtl.BeginUpdate();

			PivotGridPropertiesMx p = PivotGridPropertiesMx;

			SyncMxFieldListWithSourceQuery(BaseQuery, ref p.PivotFields); // be sure initialized

			BuildGridFieldsFromViewFields();

			ConfigurePivotGridControlFields();

			ConfigurePivotGridControlOptions();

			PivotGridCtl.EndUpdate();

			return;
		}

/// <summary>
/// Build the Grid fields for the PivotGridControl from the persisted Mobius view fields
/// </summary>

		void BuildGridFieldsFromViewFields()
		{
			PivotGridFieldMx gf, gf2;

			PivotGridCtl.Fields.Clear();
			PivotGridCtl.Groups.Clear();

			PivotGridPropertiesMx p = PivotGridPropertiesMx;

			for (int fi = 0; fi < p.PivotFields.Count; fi++) // build grid fields from view fields
			{
				PivotGridFieldMx pf = p.PivotFields[fi];
				ResultsField rfld = pf.ResultsField as ResultsField;
				if (rfld == null) continue;
				PivotGridControlMx.SetFieldCaption(pf); // be sure we have a caption

				Mobius.Data.QueryColumn qc = rfld.QueryColumn;
				MetaColumn mc = rfld.MetaColumn;

				gf = new PivotGridFieldMx();
				pf.CopyField(gf);
				pf.SyncDxAreaToMxRole();

				if (mc.IsKey) gf.ImageIndex = (int)Bitmaps16x16Enum.Key;
				else gf.ImageIndex = (int)mc.DataTypeImageIndex;

				gf.Options.AllowRunTimeSummaryChange = true;
				gf.Options.ShowUnboundExpressionMenu = true;

				PivotGridCtl.Fields.Add(gf);
			}

			return;
		}

		/// <summary>
		/// Copy the grid control fields to the view
		/// </summary>
		/// <param name="p"></param>

		public void UpdateViewFieldsFromGridFields()
		{
			if (PivotGridPropertiesMx == null)
				PivotGridPropertiesMx = new PivotGridPropertiesMx();

			PivotGridPropertiesMx p = PivotGridPropertiesMx;

			p.PivotFields = new List<PivotGridFieldMx>();
			foreach (PivotGridFieldMx f in PivotGridCtl.Fields)
			{
				PivotGridFieldMx f2 = new PivotGridFieldMx();
				f.CopyField(f2);
				p.PivotFields.Add(f2);
			}

			return;
		}

		/// <summary>
		/// Configure the grid control fields 
		/// </summary>

		internal void ConfigurePivotGridControlFields()
		{
			foreach (PivotGridFieldMx f in PivotGridCtl.Fields)
				ConfigurePivotGridControlField(f);
		}

/// <summary>
/// Configure the binding for a pivot grid field
/// </summary>
/// <param name="field"></param>

		internal void ConfigurePivotGridControlField(PivotGridFieldMx field)
		{
				ResultsField rfld = field.ResultsField as ResultsField;
				MetaColumn mc = rfld.MetaColumn;

				if (mc.DataType == MetaColumnType.CompoundId)
				{
					if (mc.IsNumeric) field.UnboundType = UnboundColumnType.Integer;
					else field.UnboundType = field.UnboundType = UnboundColumnType.String;
				}

				else if (mc.DataType == MetaColumnType.Integer)
					field.UnboundType = UnboundColumnType.Integer;

				else if (mc.IsNumeric)
					field.UnboundType = UnboundColumnType.Decimal;

				else if (mc.DataType == MetaColumnType.Date)
					field.UnboundType = UnboundColumnType.DateTime;

				else field.UnboundType = UnboundColumnType.String;

				if (mc.IsKey) field.ImageIndex = (int)Bitmaps16x16Enum.Key;
				else field.ImageIndex = (int)mc.DataTypeImageIndex;

				field.Options.AllowRunTimeSummaryChange = true;
				field.Options.ShowUnboundExpressionMenu = true;

//				field.SummaryTypeChanged += new System.EventHandler(PivotGridPanel.PivotGridField_SummaryTypeChanged);
		}


		/// <summary>
		/// Configure the grid and chart controls to the view properties
		/// </summary>

		internal void ConfigurePivotGridControlOptions()
		{
			PivotGridPropertiesMx p = PivotGridPropertiesMx;

			if (p.CompactLayout)
			{
				PivotGridCtl.OptionsView.ShowRowTotals = true;
				PivotGridCtl.OptionsView.ShowTotalsForSingleValues = true;
				PivotGridCtl.OptionsView.RowTotalsLocation = PivotRowTotalsLocation.Tree;
				PivotGridCtl.OptionsBehavior.HorizontalScrolling = PivotGridScrolling.CellsArea;
			}

			else
			{
				PivotGridCtl.OptionsView.RowTotalsLocation = PivotRowTotalsLocation.Far;
				PivotGridCtl.OptionsBehavior.HorizontalScrolling = PivotGridScrolling.Control;
			}

			PivotGridCtl.OptionsView.ShowColumnTotals = p.ShowColumnTotals;
			PivotGridCtl.OptionsView.ShowColumnGrandTotals = p.ShowColumnGrandTotals;
			PivotGridCtl.OptionsView.ShowRowTotals = p.ShowRowTotals;
			PivotGridCtl.OptionsView.ShowRowGrandTotals = p.ShowRowGrandTotals;
			PivotGridCtl.OptionsView.ShowFilterHeaders = p.ShowFilterHeaders;

			PivotGridPanel.ChartType = p.PivotGridChartType;
			PivotGridCtl.OptionsChartDataSource.SelectionOnly = p.PgcShowSelectionOnly;
			PivotGridCtl.OptionsChartDataSource.ProvideDataByColumns = p.PgcProvideDataByColumns;
			//Chart.SeriesTemplate.LabelsVisibility = DefaultBooleanMx.Convert(p.PgcShowPointLabels);
			PivotGridCtl.OptionsChartDataSource.ProvideColumnGrandTotals = p.PgcShowColumnGrandTotals;
			PivotGridCtl.OptionsChartDataSource.ProvideRowGrandTotals = p.PgcShowRowGrandTotals;

			return;
		}

		/// <summary>
		/// Refresh the view
		/// </summary>

		public override void Refresh()
		{
			if (PivotGridCtl != null)
				PivotGridCtl.Refresh();
			return;
		}

#if false // GetPivotGridFieldResultsField needed?
		/// <summary>
		/// Get the ResultsField associated with a PivotGridField
		/// </summary>
		/// <param name="Rf"></param>
		/// <param name="f0"></param>
		/// <returns></returns>

		internal ResultsField GetPivotGridFieldResultsField(
			PivotGridField pgf)
		{
			Query q = Rf.Query;
			string[] sa = pgf.UnboundFieldName.Split('.');
			if (sa.Length < 2) return null;
			QueryTable qt = q.GetTableByName(sa[0]);
			if (qt == null) return null;
			QueryColumn qc = qt.GetQueryColumnByName(sa[1]);
			if (qc == null) return null;
			ResultsField rfld = Rf.GetResultsField(qc);
			return rfld;
		}
#endif

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public override void Serialize(XmlTextWriter tw)
		{
			BeginSerialization(tw);
			EndSerialization(tw);
			return;
		}

		/// <summary>
		/// BeginSerialization
		/// </summary>
		/// <param name="tw"></param>

		public override void BeginSerialization(XmlTextWriter tw)
		{
			base.BeginSerialization(tw);

			PivotGridPropertiesMx p = PivotGridPropertiesMx;
			if (p == null) return;

			tw.WriteStartElement("PivotGridView");

			tw.WriteAttributeString("CompactLayout", p.CompactLayout.ToString());
			tw.WriteAttributeString("ShowColumnTotals", p.ShowColumnTotals.ToString());
			tw.WriteAttributeString("ShowColumnGrandTotals", p.ShowColumnGrandTotals.ToString());
			tw.WriteAttributeString("ShowRowTotals", p.ShowRowTotals.ToString());
			tw.WriteAttributeString("ShowRowGrandTotals", p.ShowRowGrandTotals.ToString());
			tw.WriteAttributeString("ShowFilterHeaders", p.ShowFilterHeaders.ToString());

			tw.WriteAttributeString("PivotGridChartType", p.PivotGridChartType);
			tw.WriteAttributeString("PgcShowSelectionOnly", p.PgcShowSelectionOnly.ToString());
			tw.WriteAttributeString("PgcDataVertical", p.PgcProvideDataByColumns.ToString());
			tw.WriteAttributeString("PgcShowPointLabels", p.PgcShowPointLabels.ToString());
			tw.WriteAttributeString("PgcShowColumnGrandTotals", p.PgcShowColumnGrandTotals.ToString());
			tw.WriteAttributeString("PgcShowRowGrandTotals", p.PgcShowRowGrandTotals.ToString());

			if (p.PivotFields != null)
			{
				tw.WriteStartElement("PivotGridFields");
				foreach (PivotGridFieldMx f in p.PivotFields)
				{
					ResultsField rfld = f.ResultsField as ResultsField;

					tw.WriteStartElement("PivotGridField");

					if (rfld == null || f.Caption != PivotGridControlMx.BuildFieldCaption(f)) // write caption if different than field name
						tw.WriteAttributeString("Caption", f.Caption);

					tw.WriteAttributeString("UnboundFieldName", f.UnboundFieldName); // tableAlias.mcName link to source query table/column

					tw.WriteAttributeString("Role", f.Role.ToString());
					tw.WriteAttributeString("AreaIndex", f.AreaIndex.ToString());

					if (f.SummaryTypeMx != AggregationDef.Model.SummaryType)
						tw.WriteAttributeString("SummaryType", f.SummaryTypeMx.ToString());

					if (f.GroupingType != AggregationDef.Model.GroupingType)
						tw.WriteAttributeString("GroupingType", f.GroupingType.ToString());

					if (f.NumericIntervalSize != AggregationDef.Model.NumericIntervalSize)
						tw.WriteAttributeString("NumericIntervalSize", f.NumericIntervalSize.ToString());

					if (f.Width != PivotGridField.DefaultWidth)
						tw.WriteAttributeString("Width", f.Width.ToString());

					tw.WriteEndElement(); // PivotGridField
				}
				tw.WriteEndElement(); // PivotGridFields
			}

			tw.WriteEndElement(); // PivotGridView

			return;
		}

		/// <summary>
		/// EndSerialization
		/// </summary>
		/// <param name="tw"></param>

		public override void EndSerialization(XmlTextWriter tw)
		{
			base.EndSerialization(tw);
			return;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>

		public new void FreeResources()
		{
			RenderingControl = null;
			PageControl = null;
			return;
		}

	}


}
