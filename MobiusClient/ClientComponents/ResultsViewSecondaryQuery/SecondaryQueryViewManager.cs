using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireClient;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Create a view that comes from another query but that uses the hit list from the current query
	/// </summary>

	public class SecondaryQueryViewManager : ViewManager
	{
		QueryManager Sqm = null; // query manager for SubQuery

		/// <summary>
		/// Basic constructor
		/// </summary>

		public SecondaryQueryViewManager(ResultsViewModel rvm)
		{
			UserObjectType uoType;
			int uoId;

			ViewType = ResultsViewType.SecondaryQuery;
			if (Lex.IsUndefined(rvm.ViewSubtype)) return;

			ViewSubtype = rvm.ViewSubtype; // if view subtype defined it should be a value like Query_12345. If so parse out the SubQUeryId.

			if (Lex.IsDefined(rvm.CustomViewTypeImageName))
				CustomViewTypeImageName = rvm.CustomViewTypeImageName;

			if (Lex.IsDefined(rvm.Title))
				Title = rvm.Title;

			bool parseOk = UserObject.ParseObjectTypeAndIdFromInternalName(ViewSubtype, out uoType, out uoId);
			if (parseOk) SubQueryId = uoId;
			return;
		}

	/// <summary>
	/// Show dialog for initial view setup
	/// </summary>
	/// <returns></returns>

	public override DialogResult ShowInitialViewSetupDialog()
		{
			DialogResult dr;

			QueryColumn qc = null;
			if (SubQueryId < 0)
			{
				QueryViewSelectorDialog qvsd = new QueryViewSelectorDialog();
				qvsd.QuerySelectorControl.ShowCurrentQueryMenuItem = false;

				dr = qvsd.ShowDialog(this.SubQuery, "");
				if (dr == DialogResult.Cancel) return DialogResult.Cancel;

				SubQuery = qvsd.SelectedQuery;
				if (SubQuery == null) return DialogResult.Cancel;

				SubQuerySelectedView = qvsd.SelectedView;
				if (SubQuerySelectedView == null) return DialogResult.Cancel;

				SubQueryId = SubQuery.UserObject.Id;
				SubQuerySelectedViewId = SubQuerySelectedView.Id;
			}

			if (SubQuerySelectedView == null) RealizeSubQueryView();
			if (SubQuerySelectedView == null) return DialogResult.Cancel; // no view realized

			string defaultViewTitle = ResultsViewProps.GetViewTypeLabel(SubQuerySelectedView.ViewType);

			// Setup done here, realization of query results and the view happens in ConfigureRenderingControl

			dr = ToolHelper.InvokeToolBasedInitialViewSetupDialog(SubQuery, BaseQuery);
			return dr;
		}

		/// <summary>
		/// Show the properties for the view after the initial setup and rendering of the view
		/// </summary>
		/// <returns></returns>

		public override DialogResult ShowInitialViewPropertiesDialog()
		{
			return DialogResult.OK; // just return OK, main work happens in ConfigureRenderingControl
		}

		/// <summary>
		/// Create the view display controls and link them into the QueryResultsControl and the ResultsView object
		/// </summary>
		/// <returns></returns>

		public override Control AllocateRenderingControl()
		{
			if (RealizeSubQueryView() == null) // be sure the subquery and view still exist
			{
				MessageBoxMx.ShowError("Failed to realize SubQueryId: " + SubQueryId + ", SubQuerySelectedViewId: " + SubQuerySelectedViewId);
				return null;
			}

			Query sq = SubQuery;
			ResultsViewProps sqView = SubQuerySelectedView;

			if (Lex.IsUndefined(CustomViewTypeImageName))
				CustomViewTypeImageName = sqView.CustomViewTypeImageName;

			if (Lex.IsUndefined(Title))
				Title = sqView.Title;

			if (Sqm == null)
				Sqm = SetupAndExecuteSecondaryQuery(sq, sqView);

			RenderingControl = sqView.AllocateRenderingControl(); // allocate proper control for the subquery view we want to use

			return RenderingControl;
		}

		/// <summary>
		/// Setup subquery with and QueryManger and execute the search step with the proper set of results keys from the parent query
		/// </summary>
		/// <param name="sq"></param>
		/// <param name="view"></param>
		/// <returns></returns>

		QueryManager SetupAndExecuteSecondaryQuery(
			Query sq,
			ResultsViewProps view)
		{
			//sq.ResultKeys = Qm.DataTableManager.ResultsKeys;
			//sq.UseResultKeys = true; 

			QueryEngine qe = new QueryEngine();
			Query nq;
			List<string> resultKeys = qe.TransformAndExecuteQuery(sq, out nq);

			if (nq != null) sq = nq; // if modified then replace original query and QE with the new ones

			QueryManager sqm = new QueryManager();
			OutputDest outputDest = OutputDest.WinForms;
			QueryExec.InitializeQueryManager(sqm, sq, outputDest, qe, resultKeys);
			DataTableManager dtm = sqm.DataTableManager;

			sqm.QueryResultsControl = Qrc; // same query results control as us 
			//sq.Parent = Qm.Query; // set parent of subQuery;

			return sqm;
		}

		/// <summary>
		/// Insert the rendering control and associated tools into the display panel 
		/// </summary>

		public override void InsertRenderingControlIntoDisplayPanel(
			XtraPanel viewPanel)
		{
			SubQuerySelectedView.InsertRenderingControlIntoDisplayPanel(viewPanel); // should handle tools as well
			return;
		}

		/// <summary>
		/// Insert tools into display panel
		/// </summary>

		public override void InsertToolsIntoDisplayPanel()
		{
			if (Qrc == null) return;

			SubQuerySelectedView.InsertToolsIntoDisplayPanel();
			return;
		}

		/// <summary>
		/// Return true if the view has been defined
		/// </summary>

		public override bool IsDefined
		{
			get
			{
				return SubQuerySelectedView.IsDefined;
			}
		}

		/// <summary>
		/// Render the view into the specified results control
		/// </summary>

		public override void ConfigureRenderingControl()
		{
			SubQuerySelectedView.ConfigureRenderingControl();

			Sqm.DataTableManager.StartRowRetrieval(); // start retrieval of data

			return;
		}

		/// <summary>
		/// Verify that a SubQueryId is defined then verify that the Subquery still exists
		/// </summary>
		/// <returns></returns>

		public bool VerifySubQueryExistsIfDefined()
		{
			if (SubQueryId <= 0) return false;

			else if (RealizeSubQuery() != null) return true;

			else return false;
		}

		/// <summary>
		/// Get the query that this secondary query view uses
		/// </summary>
		/// <returns></returns>

		public Query RealizeSubQuery()
		{
			if (SubQuery != null)
			{
				return SubQuery;
			}

			else if (SubQueryId > 0)
			{
				SubQuery = QbUtil.ReadQuery(SubQueryId);
				return SubQuery;
			}

			else return null;
		}

		/// <summary>
		/// Realize the ResultsViewProps for the subquery selected view id
		/// </summary>
		/// <returns></returns>

		public ResultsViewProps RealizeSubQueryView()
		{
			ResultsViewProps activeView;

			if (SubQuerySelectedView != null) return SubQuerySelectedView;

			if (SubQuery == null) // need to get the subquery
			{
				if (RealizeSubQuery() == null) return null;
			}

			List<ResultsViewProps> views = SubQuery.GetResultsViews(out activeView); // get the list of views and the "active" view if defined
			if (views == null || views.Count == 0) return null;

			if (Lex.IsDefined(SubQuerySelectedViewId)) // look for the selected view
			{
				foreach (ResultsViewProps vp in views)
				{
					if (vp.Id == SubQuerySelectedViewId)
					{
						SubQuerySelectedView = vp;
						SubQuerySelectedViewId = vp.Id;
						return vp;
					}
				}
			}

			// If selected view not defined or not found then use the last view in the query by default

			if (activeView == null) activeView = views[views.Count - 1]; // last view
			SubQuerySelectedViewId = activeView.Id;
			SubQuerySelectedView = activeView;
			return activeView;
		}

		/// <summary>
		/// Reset the state of the view to allow execution of a new or modified source query
		/// </summary>

		public override void ResetStateForNewQueryExecution()
		{
			Sqm = null; // force subquery reexecution also
			RenderingControl = null;
			ConfigureCount = 0;

			if (SubQuerySelectedView != null) // reset associated subquery view if defined
				SubQuerySelectedView.ResetStateForNewQueryExecution();

			return;
		}

	}
}
