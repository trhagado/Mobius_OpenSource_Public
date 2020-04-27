using Mobius.ComOps;
using Mobius.SpotfireDocument;

using DevExpress.XtraPivotGrid;
using DevExpress.Data.PivotGrid;
//using DevExpress.XtraCharts;
using DevExpress.XtraEditors;

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
		/// ResultsViewProps Class 
		/// This class defines the properties of a view onto a set of query results. 
		/// Each view has basic ViewType that determines the type of view (e.g. Table, HtmlTable, PivotGrid, Spotfire)
		/// The visual rendering of the view itself is produced from the view type, properties and the associated data.
		/// ResultsViewProps objects are contained in a ResultsPage that is associated with a Query. 
		/// </summary>

		public partial class ResultsViewProps
	{
		public string Id; // Unique persisted Guid id for the view
		public string Name  // internal name for view
		{
			get => _name; 
			set => _name = value; 
		}
		string _name = "";

		public string Title  // title of the view that appears in the view tab
		{
			get => _title; 
			set => _title = value; 
		}
		string _title = "";

		public bool ShowTitle = false; // show title in secondary area (e.g. Chart header) 
		public string Description = "";

		public int IdForSession = -1; // id of the view for the current session (for debug, not persisted)
		
		/// <summary>
		/// Base Query associated with this view (note that there may be a query derived from this one that is used for execution)
		/// If this view uses another separate query it is defined in the SubQuery fields
		/// </summary>

		public Query BaseQuery = null;

		public ResultsPage ResultsPage; // ResultsPage this ResultsView is a part of

		public ResultsViewType ViewType = ResultsViewType.Unknown; // main view type
		public string ViewSubtype = ""; // SubType

		public Control RenderingControl; // the control instance that the view is rendered into, usually a PagePanel control
		public int ConfigureCount = 0; // number of times view has been configured and rendered to its associated control instance
		public virtual bool DisposeOfPageControlsWhenSwitchingToOtherPage() { return true; } // by default dispose of existing controls when a new results page is rendered

		// Additional parameters for SecondaryQuery view

		public Query SubQuery = null; // another query, separate from the base query, that this view uses
		public int SubQueryId = -1; // UserObject id of associated saved subquery if any
		public bool SubqueryBuilt = false; // true if a subquery Query object has been built for this view
		public bool SubqueryResultsBuilt = false; // true if the data for the subquery has been built

		public ResultsViewProps SubQuerySelectedView = null; // selected view in subquery if SecondaryQuery view
		public string SubQuerySelectedViewId = ""; // selected view id in subquery if SecondaryQuery view

		// Additional parameters for specific view types

		public SpotfireViewProps SpotfireViewProps = null; // Spotfire view properties

		public PivotGridPropertiesMx PivotGridPropertiesMx = null; // PivotGridView members

		public TargetSummaryOptions TargetSummaryOptions = null; // Target results view parameters

		public AssayHeatmapProperties AssayHeatmapProperties = null; // AssayResultsHeatMap members

		public string ContentUri = ""; // content for html pages

		// Class statics

		public static int SessionIdCount = 0; // used to sequentially assign Ids for this session
		public static IResultsViewManagerFactory ResultsViewFactory; // constructor for derived classes
		public static bool AllowUndefinedResultsViewFactory = true; // if true allow ResultsViewFactory to be null and create just a default ResultsView
		public static ResultsViewProps ModelView = new ResultsViewProps(); // model with default values

		/// <summary>
		/// Default constructor (use NewResultsView instead of calling this constructor directly)
		/// </summary>

		public ResultsViewProps()
		{
			ViewType = ResultsViewType.Unknown;

			IdForSession = SessionIdCount++;
			Id = System.Guid.NewGuid().ToString();

			return;
		}

		public ResultsViewProps(ResultsViewModel rvi)
		{

			ViewType = rvi.ViewType;
			ViewSubtype = rvi.ViewSubtype;
			BaseQuery = rvi.Query;

			return;
		}

		/// <summary>
		/// Construct view setting type and parent ResultsPage and Query
		/// </summary>
		/// <param name="qm"></param>
		/// <param name="rPage"></param>
		/// <param name="viewType"></param>

		public static ResultsViewProps NewResultsView(
			Query query,
			ResultsPage page,
			ResultsViewType viewType,
			string viewSubtype)
		{
			ResultsViewProps view = NewResultsView(viewType, viewSubtype, query);
			view.SetDefaultTitle(page, viewType, query);
			page.AddView(view);
			return view;
		}

		/// <summary>
		/// Set default title for view within a page based on view type
		/// </summary>
		/// <param name="page"></param>
		/// <param name="viewType"></param>
		/// 

		public void SetDefaultTitle(
			ResultsPage page,
			ResultsViewType viewType,
			Query query)
		{
			Title = page.AddSequentialSuffixToTitle(GetViewTypeLabel(viewType), query.ResultsPages); // assign sequential number across page names (only one view per page for now)

			//Title = AddSequentialSuffixToTitle(GetViewTypeLabel(viewType), page); // assign sequential number for views within page

			return;
		}

		/// <summary>
		/// Add suffix to title to make it unique
		/// </summary>
		/// <param name="title"></param>
		/// <param name="page"></param>
		/// <returns></returns>

		public string AddSequentialSuffixToTitle(
			string title,
			ResultsPage page)
		{
			int max, intVal;
			max = -1;

			foreach (ResultsViewProps view0 in page.Views) // get max number of this chart type
			{
				if (Lex.Eq(view0.Title, title))
				{
					max = 1;
					continue;
				}

				if (!Lex.StartsWith(view0.Title, title)) continue;

				string tok = view0.Title.Substring(title.Length).Trim();
				if (!int.TryParse(tok, out intVal)) continue;

				if (intVal > max) max = intVal;
			}

			if (max > 0) title += " " + (max + 1);

			return title;
		}

		/// <summary>
		/// Create a new ResultsView object
		/// </summary>
		/// <param name="viewType"></param>
		/// <param name="viewSubtype"></param>
		/// <param name="query"></param>
		/// <returns></returns>

		public static ResultsViewProps NewResultsView(
				ResultsViewType viewType,
				string viewSubtype,
				Query query)
		{
			ResultsViewModel rvi = new ResultsViewModel();
			rvi.ViewType = viewType;
			rvi.ViewSubtype = viewSubtype;
			rvi.Query = query;
			ResultsViewProps viewProps = NewResultsView(rvi);
			return viewProps;
		}

		/// <summary>
		/// Create a new ResultsView object
		/// </summary>
		/// <param name="rvi"></param>
		/// <returns></returns>

		public static ResultsViewProps NewResultsView(
			ResultsViewModel rvi)
		{
			ResultsViewProps view;

			if (ResultsViewFactory != null)
				view = ResultsViewFactory.NewResultsViewManager(rvi);

			else if (AllowUndefinedResultsViewFactory)
				view = new ResultsViewProps();

			else throw new Exception("ResultsViewFactory not defined");

			return view;
		}

		/// <summary>
		/// Get the type label for the view 
		/// </summary>

		public string ViewTypeLabel
		{
			get
			{
				return GetViewTypeLabel(ViewType);
			}
		}

		/// <summary>
		/// Get label for view type
		/// </summary>
		/// <param name="viewType"></param>
		/// <returns></returns>

		public static string GetViewTypeLabel(ResultsViewType viewType)
		{
			switch (viewType)
			{
				case ResultsViewType.Table: return "Table View";
				case ResultsViewType.HtmlTable: return "HTML Table View";
				case ResultsViewType.PivotGrid: return "Pivot Table";
				case ResultsViewType.Spotfire: return "Spotfire";
				case ResultsViewType.SecondaryQuery: return "Other Query Based";

				//case ViewTypeMx.ScatterPlot: return "Bubble Chart";
				//case ViewTypeMx.BarChart: return "Bar Chart";
				//case ViewTypeMx.RadarPlot: return "Radar Plot";
				//case ViewTypeMx.Heatmap: return "Heat Map";

				//case ViewTypeMx.Network: return "Network";

				//case ViewTypeMx.TargetSummaryUnpivoted: return "Target Summary Unpivoted";
				//case ViewTypeMx.TargetSummaryPivoted: return "Target Summary Pivoted";
				//case ViewTypeMx.TargetTargetTable: return "Target-target Pivoted";
				//case ViewTypeMx.TargetTargetNetwork: return "Target-target Network";

				//case ViewTypeMx.TargetSummaryHeatmap: return "Target Summary Heatmap";
				//case ViewTypeMx.TargetSummaryImageMap: return "Target Summary Dendogram/Pathway Map";
				//case ViewTypeMx.SasMap: return "Structure-Activity Similarity Map";

				default: return "Unknown";
			}
		}

		/// <summary>
		/// Get view index within the list of views for the page
		/// </summary>

		public int GetViewIndex()
		{
			if (ResultsPage != null)
				return ResultsPage.GetViewIndex(this);
			else return -1;
		}

		/// <summary>
		/// Get/set the view type image name for the view
		/// </summary>
		/// <returns></returns>

		public string ViewTypeImageName
		{
			get
			{
				if (Lex.IsDefined(CustomViewTypeImageName)) // if specifically defined custom value then return it
					return CustomViewTypeImageName;

				else return ViewType.ToString(); // otherwise return image name based on type
			}
		}

		public string CustomViewTypeImageName = "";

		/// <summary>
		/// Return true if the view content has been defined
		/// </summary>

		public virtual bool IsDefined
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Set the property values for the view to default values (or based on supplied data)
		/// </summary>

		public virtual void SetDefaultPropertyValues()
		{
			return;
		}

		/// <summary>
		/// Show dialog for initial view setup prior to any rendering
		/// </summary>
		/// <returns></returns>

		public virtual DialogResult ShowInitialViewSetupDialog()
		{
			return DialogResult.OK;
		}

		/// <summary>
		/// Build any subquery results needed for the current view type
		/// </summary>
		/// <returns></returns>

		public virtual IQueryManager BuildSubqueryResults()
		{
			return null;
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public virtual Control AllocateRenderingControl()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public virtual void InsertRenderingControlIntoDisplayPanel(
			XtraPanel viewPanel)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Insert tools into display panel
		/// </summary>

		public virtual void InsertToolsIntoDisplayPanel()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Show the properties for the view after the initial setup and rendering of the view
		/// </summary>
		/// <returns></returns>

		public virtual DialogResult ShowInitialViewPropertiesDialog()
		{
			return DialogResult.OK;
		}

		/// <summary>
		/// Get context menu strip for view to be shown on a right mouse click
		/// </summary>
		/// <returns></returns>

		public virtual ContextMenuStrip GetContextMenuStrip()
		{
			return null;
		}

		/// <summary>
		/// Render the view by configuring the control for the current view settings
		/// </summary>

		public virtual void ConfigureRenderingControl()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reset the state of the view to allow execution of a new or modified source query
		/// </summary>

		public virtual void ResetStateForNewQueryExecution()
		{
			RenderingControl = null;
			ConfigureCount = 0;
		}

		/// <summary>
		/// Update the view to reflect changes in filtering
		/// </summary>

		public virtual void UpdateFilteredView()
		{
			return;
		}

		/// <summary>
		/// Refresh view 
		/// </summary>
		/// <param name="q"></param>
		/// <param name="viewIndex"></param>

		public static void Refresh(
			Query q,
			int viewIndex)
		{
			q = q.Root; // all views are associated with the root query
			ResultsPage page = q.ResultsPages[viewIndex]; // 1 view/page for now
			if (page.Views.Count <= 0) return;
			page.Views[0].Refresh();
			return;
		}

		/// <summary>
		/// Refresh the view control
		/// </summary>

		public virtual void Refresh()
		{
			return;
		}

		/// <summary>
		/// Get the scale to fit view within pagewidth
		/// </summary>
		/// <param name="pct"></param>

		public virtual int GetFitPageWidthScale()
		{
			return 100; // default to 100%
		}

		/// <summary>
		/// Scale the view
		/// </summary>
		/// <param name="pct"></param>

		public virtual void ScaleView(int pct)
		{
			return;
		}

		/// <summary>
		/// Serialize the view
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public virtual void Serialize(XmlTextWriter tw)
		{
			BeginSerialization(tw);
			EndSerialization(tw);

			return;
		}

		/// <summary>
		/// Begin Serialization
		/// </summary>
		/// <param name="tw"></param>

		public virtual void BeginSerialization(XmlTextWriter tw)
		{
			int queryId;

			tw.WriteStartElement("ResultsView");
			XmlUtil.WriteAttributeIfDefined(tw, "Id", Id);
			XmlUtil.WriteAttributeIfDefined(tw, "Name", Name);
			XmlUtil.WriteAttributeIfDefined(tw, "Title", Title);
			if (ShowTitle != ModelView.ShowTitle) tw.WriteAttributeString("ShowTitle", ShowTitle.ToString());
			XmlUtil.WriteAttributeIfDefined(tw, "Description", Description);
			
			if (BaseQuery != null) queryId = BaseQuery.InstanceId; // top-level query id
			else queryId = -1;
			tw.WriteAttributeString("QueryId", queryId.ToString());

			tw.WriteAttributeString("ViewType", ViewType.ToString());

			XmlUtil.WriteAttributeIfDefined(tw, "CustomViewTypeImageName", CustomViewTypeImageName);

			XmlUtil.WriteAttributeIfDefined(tw, "ViewSubtype", ViewSubtype);

			if (SubQueryId > 0)
				tw.WriteAttributeString("SubQueryId", SubQueryId.ToString());

			XmlUtil.WriteAttributeIfDefined(tw, "SubQuerySelectedViewId", SubQuerySelectedViewId);

			if (SpotfireViewProps != null)
				SpotfireViewProps.Serialize(tw);

			return;
		}

		/// <summary>
		/// EndSerialization
		/// </summary>
		/// <param name="tw"></param>

		public virtual void EndSerialization(XmlTextWriter tw)
		{
			tw.WriteEndElement(); // ResultsView
			return;
		}

		/// <summary>
		/// Deserialize from a string into an existing view object
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <param name="view"></param>

		public static void Deserialize(
			string serializedForm,
			ResultsViewProps view)
		{
		}

		/// <summary>
		/// Deserialize the ResultsView
		/// </summary>
		/// <param name="rootQuery"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static ResultsViewProps Deserialize(
			Query q,
			XmlTextReader tr)
		{
			int i1 = 0, qid = -1;
			Enum iEnum = null;
			string errorMsg, txt, tok= "";

			if (!Lex.Eq(tr.Name, "ResultsView"))
				throw new Exception("ResultsView element not found");

			if (!XmlUtil.GetEnumAttribute(tr, "ViewType", typeof(ResultsViewType), ref iEnum, out errorMsg))
				throw new Exception(errorMsg);
			ResultsViewType viewType = (ResultsViewType)iEnum;

			string viewSubtype = XmlUtil.GetAttribute(tr, "ViewSubtype");

			ResultsViewProps view = NewResultsView(viewType, viewSubtype, q);

			XmlUtil.GetStringAttribute(tr, "CustomViewTypeImageName", ref view.CustomViewTypeImageName);

			XmlUtil.GetStringAttribute(tr, "Id", ref view.Id);

			if (XmlUtil.GetStringAttribute(tr, "Name", ref tok))
				view.Name = tok;

			if (XmlUtil.GetStringAttribute(tr, "Title", ref tok))
				view.Title = tok;

			XmlUtil.GetBoolAttribute(tr, "ShowTitle", ref view.ShowTitle);
			XmlUtil.GetStringAttribute(tr, "Description", ref view.Description);

			XmlUtil.GetIntAttribute(tr, "QueryId", ref qid);
			view.BaseQuery = q; // default to this query;
			if (qid >= 0) // map a query id to a query reference
			{
				Query q2 = q.GetQueryRefFromId(qid);
				if (q2 != null) view.BaseQuery = q2;
			}

			XmlUtil.GetIntAttribute(tr, "SubQueryId", ref view.SubQueryId);
			XmlUtil.GetStringAttribute(tr, "SubQuerySelectedViewId", ref view.SubQuerySelectedViewId);

			while (true)
			{
				if (!XmlUtil.MoreSubElements(tr, "ResultsView")) break;

				else if (Lex.Eq(tr.Name, "SpotfireViewProps"))
					SpotfireViewProps.DeserializeSpotfireProperties(view.BaseQuery, tr, view);

				else if (Lex.Eq(tr.Name, "MultiDbViewerPrefs") || // viewer prefs?
					Lex.Eq(tr.Name, "TargetResultsViewerParms")) // old name
				{
					view.TargetSummaryOptions = TargetSummaryOptions.Deserialize(tr);
					tr.Read(); tr.MoveToContent();
				}

				else if (DeserializeAssayResultsHeatMapProperties(view.BaseQuery, tr, view))
				{ tr.Read(); tr.MoveToContent(); }

				else if (DeserializePivotGridProperties(view.BaseQuery, tr, view))
				{ tr.Read(); tr.MoveToContent(); }

				//if (NetworkProperties.Deserialize(view.Query, tr, view)) // (obsolete)
				//{ tr.Read(); tr.MoveToContent(); }
				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return view;
		}

		/// <summary>
		/// DeserializeAssayResultsHeatMapProperties
		/// </summary>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <param name="p"></param>
		/// <returns></returns>

		public static bool DeserializeAssayResultsHeatMapProperties(
			Query q,
			XmlTextReader tr,
			ResultsViewProps view)
		{
			Enum iEnum = null;

			if (!Lex.Eq(tr.Name, "AssayResultsHeatMap")) return false;

			AssayHeatmapProperties p = view.AssayHeatmapProperties;
			if (p == null) return false;

			if (XmlUtil.GetEnumAttribute(tr, "ColsToSum", typeof(ColumnsToTransform), ref iEnum))
				p.ColsToSum = (ColumnsToTransform)iEnum;

			if (XmlUtil.GetEnumAttribute(tr, "SumLevel", typeof(TargetAssaySummarizationLevel), ref iEnum))
				p.SumLevel = (TargetAssaySummarizationLevel)iEnum;

			if (XmlUtil.GetEnumAttribute(tr, "SumMethod", typeof(SummarizationType), ref iEnum))
				p.SumMethod = (SummarizationType)iEnum;

			if (XmlUtil.GetEnumAttribute(tr, "PivotFormat", typeof(TargetAssayPivotFormat), ref iEnum))
				p.PivotFormat = (TargetAssayPivotFormat)iEnum;

			if (!tr.IsEmptyElement)
			{
				tr.Read(); tr.MoveToContent();
				if (!Lex.Eq(tr.Name, "AssayResultsHeatMap"))
					throw new Exception("Expected AssayResultsHeatMap end element");
			}

			return true;
		}

		/// <summary>
		/// DeserializePivotGridProperties
		/// </summary>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <param name="p"></param>
		/// <returns></returns>

		public static bool DeserializePivotGridProperties(
			Query q,
			XmlTextReader tr,
			ResultsViewProps view)
		{
			Enum iEnum = null;
			bool b1 = false;
			string txt = "";
			int i1 = -1;
			Decimal dec1 = -1;

			if (!Lex.Eq(tr.Name, "PivotGridView")) return false;

			PivotGridPropertiesMx p = view.PivotGridPropertiesMx = new PivotGridPropertiesMx();

			XmlUtil.GetBoolAttribute(tr, "CompactLayout", ref p.CompactLayout);
			XmlUtil.GetBoolAttribute(tr, "ShowColumnTotals", ref p.ShowColumnTotals);
			XmlUtil.GetBoolAttribute(tr, "ShowColumnGrandTotals", ref p.ShowColumnGrandTotals);
			XmlUtil.GetBoolAttribute(tr, "ShowRowTotals", ref p.ShowRowTotals);
			XmlUtil.GetBoolAttribute(tr, "ShowRowGrandTotals", ref p.ShowRowGrandTotals);
			XmlUtil.GetBoolAttribute(tr, "ShowFilterHeaders", ref p.ShowFilterHeaders);

			XmlUtil.GetStringAttribute(tr, "PivotGridChartType", ref p.PivotGridChartType);
			XmlUtil.GetBoolAttribute(tr, "PgcShowSelectionOnly", ref p.PgcShowSelectionOnly);
			XmlUtil.GetBoolAttribute(tr, "PgcDataVertical", ref p.PgcProvideDataByColumns);
			XmlUtil.GetBoolAttribute(tr, "PgcShowPointLabels", ref p.PgcShowPointLabels);
			XmlUtil.GetBoolAttribute(tr, "PgcShowColumnGrandTotals", ref p.PgcShowColumnGrandTotals);
			XmlUtil.GetBoolAttribute(tr, "PgcShowRowGrandTotals", ref p.PgcShowRowGrandTotals);

			p.PivotFields = new List<PivotGridFieldMx>();

			if (tr.IsEmptyElement) return true;

			while (true)
			{
				tr.Read(); tr.MoveToContent();
				if (tr.NodeType == XmlNodeType.EndElement && Lex.Eq(tr.Name, "PivotGridView")) break;
				else if (Lex.Eq(tr.Name, "PivotGridFields")) { } // just ignore
				else if (Lex.Eq(tr.Name, "PivotGridField"))
				{
					PivotGridFieldMx f = new PivotGridFieldMx();

					if (XmlUtil.GetStringAttribute(tr, "Caption", ref txt))
						f.Caption = txt;

					if (XmlUtil.GetStringAttribute(tr, "UnboundFieldName", ref txt)) // tableAlias.mcName link to source query table/column
						f.UnboundFieldName = txt;

					if (XmlUtil.GetEnumAttribute(tr, "Role", typeof(AggregationRole), ref iEnum))
						f.Aggregation.Role = (AggregationRole)iEnum;

					if (XmlUtil.GetIntAttribute(tr, "AreaIndex", ref i1))
						f.AreaIndex = i1;

					if (XmlUtil.GetEnumAttribute(tr, "SummaryType", typeof(SummaryTypeEnum), ref iEnum))
						f.SummaryTypeMx = (SummaryTypeEnum)iEnum;

					if (XmlUtil.GetEnumAttribute(tr, "GroupingType", typeof(GroupingTypeEnum), ref iEnum))
						f.GroupingType = (GroupingTypeEnum)iEnum;

					if (XmlUtil.GetDecimalAttribute(tr, "NumericIntervalSize", ref dec1))
						f.NumericIntervalSize = dec1;

					if (XmlUtil.GetIntAttribute(tr, "Width", ref i1))
						f.Width = i1;

					f.SyncDxAreaToMxRole();

					p.PivotFields.Add(f);
				}
				else throw new Exception("Unexpected element: " + tr.Name);
			}

			return true;
		}

		/// <summary>
		/// Serialize a QueryColumn Reference
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="elementName"></param>
		/// <param name="tw"></param>

		public static void SerializeQueryColumnRef(
			QueryColumn qc,
			string elementName,
			XmlTextWriter tw)
		{
			if (qc == null || qc.MetaColumn == null) return;
			QueryTable qt = qc.QueryTable;
			if (qt == null || qt.MetaTable == null) return;

			tw.WriteStartElement(elementName);

			if (Lex.IsDefined(qt.Alias))
				tw.WriteAttributeString("QtAlias", qt.Alias);

			tw.WriteAttributeString("MetaTable", qt.MetaTable.Name);

			tw.WriteAttributeString("MetaColumn", qc.MetaColumn.Name);

			tw.WriteEndElement();
		}


		/// <summary>
		/// DeserializeQueryColumn
		/// </summary>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static QueryColumn DeserializeQueryColumnRef(
			Query q,
			XmlTextReader tr)
		{
			string tableName = "", qtAlias = "", colName = "";

			if (q == null) return null;

			string elementName = tr.Name;

			XmlUtil.GetStringAttribute(tr, "QtAlias", ref qtAlias); // ignored for now, process later
			XmlUtil.GetStringAttribute(tr, "MetaTable", ref tableName);
			XmlUtil.GetStringAttribute(tr, "MetaColumn", ref colName);
			if (!tr.IsEmptyElement) // normally empty, but read end element if exists
			{
				tr.Read(); tr.MoveToContent(); // move to next element which should be end for Column
				if (!Lex.Eq(tr.Name, elementName))
					throw new Exception("Expected \"" + elementName + "\" end element");
			}

			if (Lex.IsNullOrEmpty(tableName) || Lex.IsNullOrEmpty(colName))
				return null;

			QueryTable qt = q.GetTableByName(tableName);
			if (qt == null) return null;

			QueryColumn qc = qt.GetQueryColumnByName(colName);
			return qc;
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. e.g. forms, controls)
		/// </summary>

		public virtual void FreeControlReferences()
		{
			RenderingControl = null;
			return;
		}

	} // ResultsViewProps

	/// <summary>
	/// ResultsView interface for creating managers for specific view types
	/// </summary>

	public interface IResultsViewManagerFactory
	{
		ResultsViewProps NewResultsViewManager(
			ResultsViewModel rvi);
	}

	/// <summary>
	/// AssayResultsHeatMap members
	/// </summary>

	public class AssayHeatmapProperties
	{
		public ColumnsToTransform ColsToSum; // existing CF or main results
		public TargetAssaySummarizationLevel SumLevel; // assay or target
		public SummarizationType SumMethod; // BioResponseMean or MostPotent
		public TargetAssayPivotFormat PivotFormat; // Query, Assay Name, gene symbol or gene family
	}

	/// <summary>
	/// Definition of sizing for a view dimension
	/// </summary>

	public class SizeDimension
	{
		public QueryColumn QueryColumn; // QueryColumn used to display color information, color info is in QC condformatting

		public double FixedSize // relative percentage size in the range of 1 - 100
		{ get { return _fixedSize; } set { _fixedSize = value; } }
		double _fixedSize = -1;

		/// <summary>
		/// Constructor
		/// </summary>

		public SizeDimension()
		{
			return;
		}

		/// <summary>
		/// Contruct with initial size
		/// </summary>
		/// <param name="size"></param>

		public SizeDimension(double size)
		{
			FixedSize = size;
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(string name, XmlTextWriter tw)
		{
			tw.WriteStartElement(name);
			tw.WriteAttributeString("Size", FixedSize.ToString());

			ResultsViewProps.SerializeQueryColumnRef(QueryColumn, "SizeColumn", tw);
			tw.WriteEndElement();

			return;
		}

	}

	/// <summary>
	/// Definition of coloring to be used by a view dimension
	/// </summary>

	public class ColorDimension
	{
		public QueryColumn QueryColumn; // QueryColumn used to display color information, color info is in QC condformatting
		public Color FixedColor = Color.CornflowerBlue; // color to use if no color column selected
		public Color BorderColor = Color.Black; // marker border color
		public bool FieldValueContainsColor = false; // color value is precomputed in the column

		/// <summary>
		/// Constructor
		/// </summary>

		public ColorDimension()
		{
			return;
		}

		/// <summary>
		/// Contruct with initial size
		/// </summary>
		/// <param name="size"></param>

		public ColorDimension(Color color)
		{
			FixedColor = color;
		}

#if false
		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(string name, XmlTextWriter tw)
		{
			tw.WriteStartElement(name);
			tw.WriteAttributeString("FixedColor", FixedColor.ToArgb().ToString());
			tw.WriteAttributeString("BorderColor", BorderColor.ToArgb().ToString());
			tw.WriteAttributeString("FieldValueContainsColor", FieldValueContainsColor.ToString());
			ResultsViewProps.SerializeQueryColumnRef(QueryColumn, "ColorColumn", tw);
			tw.WriteEndElement();

			return;
		}
		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		internal static ColorDimension Deserialize(
			string name,
			Query q,
			XmlTextReader tr)
		{
			ColorDimension cb = new ColorDimension();

			XmlUtil.GetColorAttribute(tr, "FixedColor", ref cb.FixedColor);
			XmlUtil.GetColorAttribute(tr, "BorderColor", ref cb.BorderColor);
			XmlUtil.GetBoolAttribute(tr, "FieldValueContainsColor", ref cb.FieldValueContainsColor);

			if (tr.IsEmptyElement) return cb;

			tr.Read(); tr.MoveToContent();

			if (Lex.Eq(tr.Name, "ColorColumn"))
			{
				cb.QueryColumn = ResultsViewProps.DeserializeQueryColumnRef(q, tr);
				tr.Read(); tr.MoveToContent();
			}

			if (Lex.Eq(tr.Name, "CondFormatRules")) // obsolete
			{
				CondFormatRules obsoleteRules = CondFormatRules.Deserialize(tr);
				tr.Read(); tr.MoveToContent();
			}

			if (!Lex.Eq(tr.Name, name))
				throw new Exception("Expected " + name + " ColorBy end element");

			return cb;
		}
#endif
	}

	public class ShapeDimension
	{
		public QueryColumn QueryColumn; // QueryColumn used to display shape information
		public int FixedShape; // shape to use if no shape column selected
		public CondFormatRules Rules; // rules for assigning shape, kept separate from QueryColumn cond formatting
		Dictionary<string, int> ValueToShapeMap;

		/// <summary>
		/// Constructor
		/// </summary>

		public ShapeDimension()
		{
			return;
		}

		/// <summary>
		/// Contruct with initial size
		/// </summary>
		/// <param name="size"></param>

		public ShapeDimension(int shape)
		{
			FixedShape = shape;
		}

		/// <summary>
		/// Build map associating values with shape ids
		/// </summary>

		public void BuildValueToShapeMap()
		{
			ValueToShapeMap = new Dictionary<string, int>();
			if (Rules == null) return;

			foreach (CondFormatRule r in Rules)
			{
				int shapeIndex = r.ForeColor.ToArgb();
				if (shapeIndex >= 0)
					ValueToShapeMap[r.Value.ToUpper()] = // assoc value to shape
						shapeIndex;
			}
		}

		/// <summary>
		/// Assign a shape to the value using the current set of rules
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>

		public int AssignShapeToValue(
			string value)
		{
			int shape = 0;

			if (QueryColumn == null) return FixedShape;
			MetaColumn mc = QueryColumn.MetaColumn;

			if (ValueToShapeMap == null) BuildValueToShapeMap();

			if (NullValue.IsNull(value))
			{
				string key = "(Blank)".ToUpper();
				if (ValueToShapeMap.ContainsKey(key))
					shape = ValueToShapeMap[key];
				else shape = FixedShape; // acts as null value for now
			}

			else
			{
				//MobiusDataType mdt = MobiusDataType.ConvertToMobiusDataType(mc.DataType, value);
				string key = value.ToUpper();
				string key2 = "(Other)".ToUpper();

				if (ValueToShapeMap.ContainsKey(key)) shape = ValueToShapeMap[key];
				else if (ValueToShapeMap.ContainsKey(key2)) shape = ValueToShapeMap[key2];
				else shape = FixedShape; // acts as null value for now
			}

			return shape;
		}

#if false
		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(string name, XmlTextWriter tw)
		{
			tw.WriteStartElement(name);
			tw.WriteAttributeString("FixedShape", FixedShape.ToString()); // write integer value from source enum for shape
			ResultsViewProps.SerializeQueryColumnRef(QueryColumn, "ShapeColumn", tw);
			if (Rules != null) Rules.Serialize(tw);
			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static ShapeDimension Deserialize(
			string name,
			Query q,
			XmlTextReader tr)
		{
			ShapeDimension sb = new ShapeDimension();

			XmlUtil.GetIntAttribute(tr, "FixedShape", ref sb.FixedShape);

			if (tr.IsEmptyElement) return sb; // may be just attributes

			tr.Read(); tr.MoveToContent();

			if (Lex.Eq(tr.Name, "ShapeColumn"))
			{
				sb.QueryColumn = ResultsViewProps.DeserializeQueryColumnRef(q, tr);
				tr.Read(); tr.MoveToContent();
			}

			if (Lex.Eq(tr.Name, "CondFormatRules"))
			{
				sb.Rules = CondFormatRules.Deserialize(tr);
				tr.Read(); tr.MoveToContent();
			}

			if (!Lex.Eq(tr.Name, name))
				throw new Exception("Expected " + name + " end element");

			return sb;
		}
#endif

	}


	/// <summary>
	/// Label dimension definition
	/// </summary>

	public class LabelDimensionDef
	{
		public QueryColumn QueryColumn;  // QueryColumn used to display label information
																		 //public LabelVisibilityModeEnum VisibilityMode = LabelVisibilityModeEnum.RolloverRow;
		public bool IncludeStructure = true;
		//public LabelPositionEnum Position = LabelPositionEnum.Center;

#if false
		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(
			string name,
			XmlTextWriter tw)
		{
			tw.WriteStartElement(name);
			//tw.WriteAttributeString("VisibilityMode", VisibilityMode.ToString());
			//tw.WriteAttributeString("Position", Position.ToString());
			//tw.WriteAttributeString("IncludeStructure", IncludeStructure.ToString());

			ResultsViewProps.SerializeQueryColumnRef(QueryColumn, "Column", tw);
			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="name"></param>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static LabelDimensionDef Deserialize(
			string name,
			Query q,
			XmlTextReader tr)
		{
			LabelDimensionDef ld = new LabelDimensionDef();

			string txt = tr.GetAttribute("VisibilityMode");
			//if (txt != null)
			//	ld.VisibilityMode = (LabelVisibilityModeEnum)EnumUtil.Parse(typeof(LabelVisibilityModeEnum), txt);

			txt = tr.GetAttribute("Position");
			//if (txt != null)
			//	ld.Position = (LabelPositionEnum)EnumUtil.Parse(typeof(LabelPositionEnum), txt);

			XmlUtil.GetBoolAttribute(tr, "IncludeStructure", ref ld.IncludeStructure);
			if (tr.IsEmptyElement) return ld; // may be just attributes

			tr.Read(); tr.MoveToContent();

			if (Lex.Eq(tr.Name, "Column"))
			{
				ld.QueryColumn = ResultsViewProps.DeserializeQueryColumnRef(q, tr);
				tr.Read(); tr.MoveToContent();
			}

			if (!Lex.Eq(tr.Name, name))
				throw new Exception("Expected " + name + " end element");

			return ld;
		}
#endif

	}

	/// <summary>
	/// ToolTip dimension definition
	/// </summary>

	public class TooltipDimensionDef
	{
		public ColumnMapCollection Fields = new ColumnMapCollection(); // the list of fields that make up the tooltip
		public bool IncludeStructure = true; // include structure in the tooltip

#if false
		/// <summary>
		/// Serialize a Tooltip def
		/// </summary>
		/// <param name="TooltipFields"></param>
		/// <param name="IncludeStructureInTooltip"></param>
		/// <param name="name"></param>
		/// <param name="tw"></param>

		public void Serialize(
			string name,
			XmlTextWriter tw)
		{
			tw.WriteStartElement(name);
			tw.WriteAttributeString("IncludeStructure", IncludeStructure.ToString());
			for (int ai = 0; ai < Fields.Count; ai++)
			{ // serialize each field in turn
				Fields[ai].Serialize("ToolTipFieldItem", tw);
			}
			tw.WriteEndElement();

			return;
		}

		/// <summary>
		/// Deserialize tooltip
		/// </summary>
		/// <param name="name"></param>
		/// <param name="q"></param>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static TooltipDimensionDef Deserialize(
			string name,
			Query q,
			XmlTextReader tr)
		{
			TooltipDimensionDef tt = new TooltipDimensionDef();

			XmlUtil.GetBoolAttribute(tr, "IncludeStructureInTooltip", ref tt.IncludeStructure);
			if (tr.IsEmptyElement) return tt; // may be just attributes

			tr.Read(); tr.MoveToContent(); // move to first item

			while (Lex.Eq(tr.Name, "TooltipFieldItem"))
			{
				ColumnMapMsx fli = ColumnMapMsx.Deserialize(q, tr);
				tt.Fields.Add(fli);
				tr.Read(); tr.MoveToContent(); // move to next item or end element
			}

			if (!Lex.Eq(tr.Name, name))
				throw new Exception("Expected " + name + " end element");


			return tt;
		}
#endif

	}

	/// <summary>
	/// Definition for model view that is yet to be fully realized
	/// </summary>

	public class ResultsViewModel
	{
		public string Name = "";  // internal name for view
		public string Title = "";  // title of the view that appears in the view tab
		public string Description = "";

		public ResultsViewType ViewType = ResultsViewType.Unknown;
		public string ViewSubtype = "";
		public string CustomViewTypeImageName = "";
		public bool ShowInViewsMenu = false;

		public Query Query = null;
		public Control QueryResultsControl = null;

		public ResultsViewModel()
		{
			return;
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>

		public ResultsViewModel Clone()
		{
			ResultsViewModel rvm = (ResultsViewModel)this.MemberwiseClone();
			return rvm;
		}

	}

	/// <summary>
	/// View types supported by Mobius
	/// </summary>

	public enum ResultsViewType
	{
		Unknown = 0,
		Table = 1, // grid-type table
		HtmlTable = 2, // Html table
		PivotGrid = 3,
		Spotfire = 4,
		ScatterPlot = 5, // via Spotfire
		BarChart = 6, // via Spotfire
		SecondaryQuery = 7, // Query-based view in which a secondary query defines the data and actual view(s)
	}

	/// <summary>
	/// ShowLabels enum
	/// </summary>

	public enum ShowLabels
	{
		Unknown = 0,
		All = 1,
		Selected = 2,
		None = 3
	}
}

