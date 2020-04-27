using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.XtraBars.Docking;

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// ResultsView with QueryManager info added
	/// </summary>

	public partial class ViewManager : ResultsViewProps
	{
		public QueryManager Qm { get { return GetQm(); } set { SetQm(value); } } // QueryManager associated with this view
		public DataTableMx Dt { get { return Qm == null ? null : Qm.DataTable; } }
		public DataTableManager Dtm { get { return Qm == null ? null : Qm.DataTableManager; } }
		public ResultsFormat ResultsFormat { get { return Qm == null ? null : Qm.ResultsFormat; } } // associated results format
		public ResultsFormat Rf { get { return Qm == null ? null : Qm.ResultsFormat; } } // alias
		public ResultsFormatter Formatter { get { return Qm == null ? null : Qm.ResultsFormatter; } } // associated formatter
		public QueryResultsControl Qrc { get { return GetQrc(); } } // containing Qrc

		public ResultsPagePanel ResultsPagePanel { get { return ResultsPage.ResultsPagePanel as ResultsPagePanel; } } // the control that contains the results page\\\

		public static List<ResultsViewModel> ResultsViewModels = null; // list of predefined model views

		public static bool ApplyChangesImmediately = true;

		public static bool Debug = false; 

		/// <summary>
		/// Get QueryManager from associated query
		/// </summary>
		/// <returns></returns>

		QueryManager GetQm()
		{
			if (BaseQuery == null) return null;

			Query q2 = BaseQuery.PresearchDerivedQuery;
			if (q2 != null && q2.QueryManager != null) // if derived query with QM then use this QM since it's the one we want at run time
				return q2.QueryManager as QueryManager;

			else return BaseQuery.QueryManager as QueryManager;
		}

		/// <summary>
		/// Set QueryManager in associated query
		/// </summary>
		/// <param name="qm"></param>

		void SetQm(QueryManager qm)
		{
			if (BaseQuery == null) throw new Exception("Query not defined");
			else BaseQuery.QueryManager = qm;
		}

		/// <summary>
		/// Define a basic QueryManager for the specified query if not done yet
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		internal QueryManager AssureQueryManagerIsDefined(
			Query q)
		{
			QueryManager qm = QueryManager.AssureQueryManagerIsDefined(q);
			if (qm.QueryResultsControl != null)
				qm.QueryResultsControl = GetQrc();

			return qm;
		}

		/// <summary>
		/// Get the QueryResultsControl associated with the view rendering control
		/// </summary>
		/// <returns></returns>

		QueryResultsControl GetQrc()
		{
			if (Qm != null && Qm.QueryResultsControl != null)
				return Qm.QueryResultsControl;

			return QueryResultsControl.GetQrcThatContainsControl(RenderingControl);
		}

		/// <summary>
		/// Look for a parent QueryResultsControl and see if the EditQueryButtonClicked method is defined and call it if so
		/// </summary>
		/// <param name="ctl"></param>
		/// <returns></returns>

		public static bool TryCallCustomExitingQueryResultsCallback(
			Control ctl,
			ExitingQueryResultsType exitType)
		{
			QueryResultsControl qrc;

			ExitingQueryResultsDelegate callback = GetCustomExitingQueryResultsCallback(ctl, out qrc, true);
			
			if (callback == null) return false;

			ExitingQueryResultsParms p = new ExitingQueryResultsParms();
			p.Qrc = qrc;
			p.ExitType = exitType;

			object[] arg = new object[] { callback, p };
			DelayedCallback.Schedule(CallCustomExitingQueryResultsCallback, arg); // call after return from this event

			return true;
		}

		static void CallCustomExitingQueryResultsCallback(object arg)
		{
			object[] args = arg as object[];

			ExitingQueryResultsDelegate callback = args[0] as ExitingQueryResultsDelegate;
			ExitingQueryResultsParms p = args[1] as ExitingQueryResultsParms;

			callback(p);
		}

		/// <summary>
		/// Check if CustomExitingQueryResultsCallback is defined
		/// </summary>
		/// <param name="ctl"></param>
		/// <returns></returns>

		public static bool IsCustomExitingQueryResultsCallbackDefined(
			Control ctl)
		{
			QueryResultsControl qrc;
			ExitingQueryResultsDelegate callback = GetCustomExitingQueryResultsCallback(ctl, out qrc);

			return callback != null;
		}

		/// <summary>
		/// Get any CustomExitingQueryResultsCallback associated with the supplied control
		/// </summary>
		/// <param name="qrcContainingCtl"></param>
		/// <returns></returns>
		public static ExitingQueryResultsDelegate GetCustomExitingQueryResultsCallback(
			Control qrcContainingCtl,
			out QueryResultsControl qrc,
			bool clearValueAfterGetting = false)
		{
			QueryManager qm;

			if (!TryGetQrcAndQm(qrcContainingCtl, out qrc, out qm))
				return null;

			ExitingQueryResultsDelegate callback = qm?.ResultsFormat?.CustomExitingQueryResultsCallback; // get any callback ref

			if (clearValueAfterGetting && callback != null) // clear call back so not called agin
				qm.ResultsFormat.CustomExitingQueryResultsCallback = null; 

			return callback;
		}

		public static bool TryGetQrcAndQm(
			Control qrcContainingCtl,
			out QueryResultsControl qrc,
			out QueryManager qm)
		{
			ContainerControl cc;
			qrc = null;
			qm = null;

			if (!WindowsHelper.FindContainerControl(qrcContainingCtl, typeof(QueryResultsControl), out cc)) // find the QRC that we are contained in
				return false;

			qrc = cc as QueryResultsControl;
			if (qrc == null) return false;

			qm = qrc?.BaseQuery?.QueryManager as QueryManager;
			if (qm == null) return false;

			return true;
		}

		/// <summary>
		/// Return true if specified control is contained in a QueriesControl
		/// </summary>

		public static bool IsControlContainedInQueriesControl(
			Control ctl)
		{
			while (true)
			{
				if (ctl == null) return false;

				else if (WindowsHelper.IsControlContainedInContainer(ctl, typeof(QueriesControl)) ||
				 Lex.Eq(ctl.Name, "QueriesControl")) // needed?
					return true;

				else ctl = ctl.Parent;
			}
		}

		public static bool IsControlContainedInPopupResultsControl(
			Control ctl)
		{
			if (WindowsHelper.IsControlContainedInContainer(ctl, typeof(PopupResults)))
				return true;
			else return false;
		}

		/// <summary>
		/// Get the stats for the data associated with a QueryColumn
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		internal ColumnStatistics GetStats(QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;
			ColumnInfo colInfo = ResultsFormat.GetColumnInfo(qc);
			int voi = colInfo.DataColIndex;
			ColumnStatistics stats = colInfo.Rfld.GetStats();
			return stats;
		}


		/// <summary>
		/// Get formatted text value for field
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="mdt"></param>
		/// <returns></returns>

		internal string GetFormattedText(QueryColumn qc, MobiusDataType mdt)
		{
			if (qc == null || mdt == null) return "";
			else if (mdt.FormattedText != null)
				return mdt.FormattedText;

			ResultsFormatter fmtr = Qm.ResultsFormatter;
			FormattedFieldInfo ffi = fmtr.FormatField(qc, mdt);
			if (ffi.FormattedText == null) ffi.FormattedText = "";
			mdt.FormattedText = ffi.FormattedText; // store back in the field
			return mdt.FormattedText;
		}

		/// <summary>
		/// Get value object for row & col
		/// </summary>
		/// <param name="ri"></param>
		/// <param name="voi"></param>
		/// <returns></returns>

		internal object GetVo(int ri, QueryColumn qc, int voi)
		{
			DataRowMx dr;
			return GetVo(ri, qc, voi, out dr);
		}

		/// <summary>
		/// Get value object for row & col
		/// </summary>
		/// <param name="ri"></param>
		/// <param name="qc"></param>
		/// <param name="voi"></param>
		/// <param name="dr"></param>
		/// <returns></returns>

		internal object GetVo(
			int ri,
			QueryColumn qc,
			int voi,
			out DataRowMx dr)
		{
			bool returnFilteredValuesAsNulls = true;
			bool markedOnly = false;
			return GetVo(ri, qc, voi, returnFilteredValuesAsNulls, markedOnly, out dr);
		}

		internal object GetVo(
			int ri,
			QueryColumn qc,
			int voi,
			bool returnFilteredValuesAsNulls,
			bool returnUnmarkedValuesAsNulls)
		{
			DataRowMx dr;
			return GetVo(ri, qc, voi, returnFilteredValuesAsNulls, returnUnmarkedValuesAsNulls, out dr);
		}

		/// <summary>
		/// Get value object for row & col
		/// </summary>
		/// <param name="ri"></param>
		/// <param name="qc"></param>
		/// <param name="voi"></param>
		/// <param name="returnFilteredValuesAsNulls"></param>
		/// <param name="returnUnmarkedValuesAsNulls"></param>
		/// <param name="dr"></param>
		/// <returns></returns>

		internal object GetVo(
			int ri,
			QueryColumn qc,
			int voi,
			bool returnFilteredValuesAsNulls,
			bool returnUnmarkedValuesAsNulls,
			out DataRowMx dr)
		{
			object o;

			dr = Dt.Rows[ri];
			DataRowAttributes dra = Dtm.GetRowAttributes(dr);
			if (returnFilteredValuesAsNulls && dra.Filtered) // store filtered values as nulls that don't get shown
				o = DBNull.Value;

			else if (Dtm.RowSubset != null && !Dtm.RowSubset.ContainsKey(ri)) // if subsetting rows & not included return as null
				o = DBNull.Value;

			else if (returnUnmarkedValuesAsNulls && !Dtm.RowIsMarked(dr)) // also null if not marked
				o = DBNull.Value;

			else
			{
				//if (Query.Tables.Count > 1 && qc.MetaColumn.MetaTable.IsRootTable) // get from root table row if appropriate
				//  dr = Dt.Rows[dra.FirstRowForKey];

				int ti = qc.QueryTable.TableIndex; // get table corresponding to specified column
				int dri2 = Dtm.AdjustDataRowToCurrentDataForTable(ri, ti, true); // get the actual row for this table associated with this base row
																																				 //if (dri2 != ri) dri2 = dri2; // debug

				if (dri2 >= 0)
				{
					dr = Qm.DataTable.Rows[dri2];
					o = dr[voi];
				}

				else o = DBNull.Value;
			}

			return o;
		}

		/// <summary>
		///	Update a QueryColumn reference to the current query by matching on MetaTable & MetaColumn name 
		/// </summary>
		/// <param name="qc"></param>

		internal void UpdateColumnReference(
			ref QueryColumn qcArg)
		{
			QueryTable qt1, qt2;
			QueryColumn qc1, qc2 = null;


			qc1 = qcArg; // copy arg
			qcArg = null; // and clear

			if (qc1 == null) return;
			if (BaseQuery.Tables.Count == 0) return;

			if (qc1.IsKey)
			{ // use the first visible key column
				qcArg = BaseQuery.Tables[0].KeyQueryColumn;
				return;
			}

			qt1 = qc1.QueryTable;
			if (qt1 == null) return;

			qt2 = BaseQuery.GetQueryTableByName(qt1.MetaTable.Name); // get new QueryTable
			if (qt2 == null) return;

			qc2 = qt2.GetQueryColumnByName(qc1.MetaColumn.Name); // get new QueryColumn
			if (qc2 == null) return;
			if (qc2.VoPosition < 0) return; // if no mapping to data clear the column

			qcArg = qc2; // return the corresponding column

			return;
		}

		/// <summary>
		/// Build tooltip for data row
		/// </summary>
		/// <param name="dri">Data row index</param>
		/// <returns></returns>

		internal SuperToolTip BuildDataRowTooltip(
			TooltipDimensionDef ttDim,
			int dri)
		{
			ColumnMapMsx cm;
			QueryTable qt;
			QueryColumn qc;
			MetaTable mt;
			MetaColumn mc;
			int i1, i2;

			if (BaseQuery == null || BaseQuery.Tables.Count == 0 || Dtm == null) return null;
			qt = BaseQuery.Tables[0];

			SuperToolTip s = new SuperToolTip();
			s.MaxWidth = 200;
			s.AllowHtmlText = DefaultBoolean.True;

			ToolTipItem i = new ToolTipItem();
			i.AllowHtmlText = DefaultBoolean.True;
			i.Appearance.TextOptions.WordWrap = WordWrap.Wrap;

			ColumnMapCollection cml2 = new ColumnMapCollection(); // list of fields we'll actually display

			int strPos = -1;

			ColumnMapCollection cml = ttDim.Fields;
			if (cml.Count == 0) return s;

			for (i1 = 0; i1 < cml.Count; i1++)
			{
				cm = cml[i1];
				qc = cm.QueryColumn;
				if (qc == null || !cm.Selected) continue;
				//if (qc.IsKey) continue;
				if (qc.MetaColumn.DataType == MetaColumnType.Structure) strPos = i1;

				for (i2 = 0; i2 < cml2.Count; i2++) // see if already have the column
				{
					if (qc == cml2[i2].QueryColumn) break;
				}
				if (i2 < cml2.Count) continue;

				cml2.Add(cm);
			}

			if (cml2.Count == 0) return null; // no fields

			if (strPos < 0 && ttDim.IncludeStructure)
			{ // include str if requested & not already included
				qc = qt.FirstStructureQueryColumn;
				strPos = cml2.Count; // put str at end
														 //strPos = keyPos + 1; // place str after key
				if (qc != null && qc.Selected) // regular selected Qc?
					cml2.ColumnMapList.Insert(strPos, ColumnMapMsx.BuildFromQueryColumn(qc));

				else // look in root table for a structure
				{
					mt = qt.MetaTable.Root;
					if (mt.FirstStructureMetaColumn != null)
					{
						qt = new QueryTable(mt);
						qc = new QueryColumn(); // add qc with no vo pos as indicator that must be selected
						qc.MetaColumn = mt.FirstStructureMetaColumn;
						qc.Selected = true;
						qt.AddQueryColumn(qc);
						cml2.ColumnMapList.Insert(strPos, ColumnMapMsx.BuildFromQueryColumn(qc));
					}
				}
			}

			string keyVal = "";
			foreach (ColumnMapMsx fli0 in cml2.ColumnMapList) // format each field
			{
				qc = fli0.QueryColumn;
				mc = qc.MetaColumn;

				i.Text += "<b>"; // build label
				if (!Lex.IsNullOrEmpty(fli0.ParameterName)) i.Text += fli0.ParameterName;
				else i.Text += fli0.QueryColumn.ActiveLabel;
				i.Text += ": </b>";

				if (qc.VoPosition >= 0)
				{
					int ti = qc.QueryTable.TableIndex;
					int dri2 = Dtm.AdjustDataRowToCurrentDataForTable(dri, ti, true);
					DataRowMx dr = Qm.DataTable.Rows[dri2];
					if (dr == null) continue;
					ResultsTable rt = Rf.Tables[ti];
					object fieldValue = dr[qc.VoPosition];
					if (!NullValue.IsNull(fieldValue))
					{
						if (qc.IsKey) keyVal = fieldValue.ToString();
						MobiusDataType mdt = MobiusDataType.ConvertToMobiusDataType(qc.MetaColumn.DataType, fieldValue);
						FormattedFieldInfo ffi = Qm.ResultsFormatter.FormatField(qc, mdt);

						if (mc.DataType != MetaColumnType.Structure)
						{
							i.Text += ffi.FormattedText;
							i.Text += "<br>";
						}
						else i = ToolTipUtil.AppendBitmapToToolTip(s, i, ffi.FormattedBitmap);
					}

					else i.Text += "<br>"; // no data
				}

				else if (!Lex.IsNullOrEmpty(keyVal)) // select structure from db (may already have)
				{
					MoleculeMx cs = MoleculeUtil.SelectMoleculeForCid(keyVal, qc.MetaColumn.MetaTable);
					if (cs != null)
					{
						int width = ResultsFormatFactory.QcWidthInCharsToDisplayColWidthInMilliinches(mc.Width, ResultsFormat);
						FormattedFieldInfo ffi = Qm.ResultsFormatter.FormatStructure(cs, new CellStyleMx(), 's', 0, width, -1);
						i = ToolTipUtil.AppendBitmapToToolTip(s, i, ffi.FormattedBitmap);
					}
				}
			}

			if (i.Text.Length > 0)
				s.Items.Add(i);

			if (s.Items.Count == 0)
			{ // show something by default
				i = new ToolTipItem();
				i.Text = "No fields selected";
				i.LeftIndent = 6;
				i.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
				s.Items.Add(i);
			}

			//ToolTipTitleItem ti = new ToolTipTitleItem();
			//ti.Text = "Dude";
			//s.Items.Add(ti);

			//ToolTipItem i = new ToolTipItem();
			//i.Text = "Subtext that is fairly long longer longest";
			//i.LeftIndent = 6;
			//i.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
			//s.Items.Add(i);

			//i = new ToolTipItem();
			//Image img = Bitmap.FromFile(@"C:\Mobius_OpenSource\Mobius-3.0\ClientComponents\Resources\Mobius76x76DkBlueBack.bmp");
			//i.Image = img;
			//s.Items.Add(i);

			//ToolTipSeparatorItem si = new ToolTipSeparatorItem();

			return s;

			//ChartPanel.ToolTipController.ToolTipLocation = ToolTipLocation.TopCenter;
			//ChartPanel.ToolTipController.AllowHtmlText = true;
			//string s = point.SeriesPointID.ToString();
			//s = "This <b>SuperToolTip</b> supports <i>HTML formatting</i>";
		}

		/// <summary>
		/// Update the titles on the view containers
		/// </summary>

		internal void UpdateContainerTitles()
		{
			Control ctl = RenderingControl;

			while (true) // if contained in a dock panel then change the dock panel title
			{
				if (ctl == null) break; // no luck

				else if (ctl is DockPanel)
				{
					ctl.Text = Title;
					return;
				}

				else ctl = ctl.Parent;
			}

			if (Qrc != null) // otherwise change the tab panel if appropriate
				Qrc.SetCurrentPageTabTitleAndImage();
		}

		/// <summary>
		/// Get list list of ResultsViewModels
		/// </summary>
		/// <returns></returns>

		public static List<ResultsViewModel> GetResultsViewModels()
		{
			string viewTypeName, viewSubtype, imageName, viewTitle;
			int imageIdx;

			if (ResultsViewModels != null) return ResultsViewModels;

			ResultsViewModels = new List<ResultsViewModel>();

			MetaTable mt = MetaTableCollection.Get("AddResultsViewMenu");
			if (mt == null)
			{
				DebugLog.Message("AddResultsViewMenu MetaTable not found");
				return null;
			}

			foreach (MetaColumn mc in mt.MetaColumns)
			{
				ResultsViewModel m = new ResultsViewModel();
				m.Name = mc.Name;
				m.Title = mc.Label;
				m.Description = mc.Description;
				m.ShowInViewsMenu = (mc.InitialSelection == ColumnSelectionEnum.Selected);

				Lex.Split(mc.ColumnMap, ",", out viewTypeName, out viewSubtype, out imageName);

				Enum.TryParse<ResultsViewType>(viewTypeName, true, out m.ViewType);
				m.ViewSubtype = viewSubtype;
				m.CustomViewTypeImageName = imageName;

				ResultsViewModels.Add(m);
			}

			return ResultsViewModels;
		}
	}

	/// <summary>
	/// /// Helper for tentatively aadding new views
	/// </summary>

	public class AddViewHelper
	{
		public static ResultsViewType LastAddedViewType = ResultsViewType.Unknown; // type of view added most recently

		/// <summary>
		/// Tentatively add a new view of the specified type to the current page
		/// </summary>
		/// <param name="viewModel"></param>
		/// <returns></returns>

		internal static void AddTentativeNewViewDelayed(
			ResultsViewModel viewModel)
		{
			DelayedCallback.Schedule(AddTentativeNewViewCallback, viewModel); // schedule callback
			return;
		}

		static void AddTentativeNewViewCallback(object state)
		{
			ResultsViewModel rvm = state as ResultsViewModel;
			AddTentativeNewView(rvm);
			return;
		}

		/// <summary>
		/// Tentatively add a new view of the specified type to the current page
		/// </summary>
		/// <param name="viewType"></param>
		/// <param name="query"></param>
		/// <param name="qrc"></param>
		/// <returns></returns>

		private static ResultsViewProps AddTentativeNewView(
			ResultsViewModel rvm)
		{
			ResultsPage rp;
			DialogResult dr;
			int intVal, max;

			ResultsViewType viewType = rvm.ViewType;
			string viewSubtype = rvm.ViewSubtype;
			string viewTitle = rvm.Title;
			Query query = rvm.Query;
			QueryResultsControl qrc = rvm.QueryResultsControl as QueryResultsControl;

			LastAddedViewType = viewType;

			int pi = qrc.CurrentPageIndex;
			if (pi < 0 || // add a page if no pages
			 query.Root.ResultsPages.Pages[pi].Views.Count > 0) // or (for now) already a view on the current page 
			{
				rp = ResultsPages.AddNewPage(query);
				pi = query.Root.ResultsPages.Pages.Count - 1;
				qrc.AddResultsPageTabPage(rp);
			}

			ResultsPage page = query.ResultsPages[pi];
			ResultsViewProps view = page.AddNewView(rvm);
			if (view == null) throw new Exception("Failed to add new view: " + viewType);

			//view.CustomViewTypeImageName = rvm.CustomViewTypeImageName; // (should already be set)

			if (Lex.IsDefined(viewTitle) && Lex.Ne(viewTitle, viewType.ToString()))
				view.Title = page.AddSequentialSuffixToTitle(viewTitle, query.ResultsPages); // set custom title (sequentially-numbered) if defined

			qrc.SetCurrentPageTabTitleAndImage();

			dr = view.ShowInitialViewSetupDialog();

			if (dr != DialogResult.OK)
			{
				RemoveTentativeView(view, qrc);
				return null; // not added
			}

			qrc.ConfigureResultsPage(page); // render the default view in the current page

			dr = view.ShowInitialViewPropertiesDialog();
			if (dr == DialogResult.OK)
			{
				//view.ConfigureRenderingControl(); // be sure it's configured (not a good idea, adds overhead)

				qrc.SetCurrentPageTabTitleAndImage(); // update tab title & image accordingly

				return view;
			}

			else // cancelled, remove view
			{
				RemoveTentativeView(view, qrc);
				return null; // not added
			}
		}

		static void RemoveTentativeView(
			ResultsViewProps view,
			QueryResultsControl qrc)
		{
			ResultsPage page = view.ResultsPage;

			page.Views.Remove(view);

			int qti = qrc.Tabs.SelectedTabPageIndex;
			qrc.RemoveTabAndPage(qti);
		}
	}

}
