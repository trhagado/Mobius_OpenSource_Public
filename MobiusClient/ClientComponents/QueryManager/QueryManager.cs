using Mobius.Data;
using Mobius.ComOps;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Drawing;
using System.Data;

namespace Mobius.ClientComponents
{
/// <summary>
/// Maintains relationships between query components
/// </summary>

	public class QueryManager : IQueryManager
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public Query Query // the query with its tables, columns, criteria, sorting and formatting information
			{ get { return _query; } set { _query = value; } }
		Query _query;

		public DataTableManager DataTableManager; // provides operations to manage the structure and content of the DataTable
		public DataTableMx DataTable; // the data
		public ResultsFormat ResultsFormat; // calculated formatting information for query
		public ResultsFormatter ResultsFormatter; // performs formatting of data
		public QueryResultsControl QueryResultsControl; // the QueryResultsControl used to display the data for the query
		public StatusBarManager StatusBarManager; // link to associated status bar
		public MoleculeGridControl MoleculeGrid; // main grid for viewing data
		public QueryExec QueryExec; // manager of the execution of the query
		public QueryEngine QueryEngine; // executes primary query & streams data into the data table

		public QueryManager()
		{
			Query = new Query(this); // create and link basic empty QM objects
			DataTable = new DataTableMx();
			DataTableManager = new DataTableManager(this);
			return;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(Query m)
		{
			Query = m;
			m.QueryManager = this;
		}

/// <summary>
/// Link in QueryManager member
/// </summary>
/// <param name="m"></param>

		public void LinkMember(DataTableManager m)
		{
			DataTableManager = m;
			m.QueryManager = this;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(DataTableMx m)
		{
			DataTable = m;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(ResultsFormat m)
		{
			ResultsFormat = m;
			m.QueryManager = this;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(ResultsFormatter m)
		{
			ResultsFormatter = m;
			m.QueryManager = this;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="qrc"></param>

		public void LinkMember(QueryResultsControl qrc)
		{
			QueryResultsControl = qrc;
			//qrc.QueryManager = this; (the Qrc Qm is now associated with the current view for the Qrc, i.e. there are multiple QMs for a Qrc
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(StatusBarManager m)
		{
			StatusBarManager = m;
			m.QueryManager = this;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(MoleculeGridControl m)
		{
			MoleculeGrid = m;
			m.QueryManager = this;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(QueryExec m)
		{
			QueryExec = m;
			m.QueryManager = this;
		}

		/// <summary>
		/// Link in QueryManager member
		/// </summary>
		/// <param name="m"></param>

		public void LinkMember(QueryEngine m)
		{
			QueryEngine = m;
		}

/// <summary>
/// Link member objects to this QueryManager
/// </summary>
		
		public void LinkMembers()
		{
			if (Query != null) Query.QueryManager = this;
			if (QueryExec != null) QueryExec.QueryManager = this;
			if (ResultsFormat != null) ResultsFormat.QueryManager = this;
			if (ResultsFormatter != null) ResultsFormatter.QueryManager = this;
			if (DataTableManager != null) DataTableManager.QueryManager = this;
			if (MoleculeGrid != null) MoleculeGrid.QueryManager = this;
			if (StatusBarManager != null) StatusBarManager.QueryManager = this;
			return;
		}

		/// <summary>
		/// Define a basic QueryManager for the specified query if not done yet
		/// </summary>
		/// <param name="q"></param>

		internal static QueryManager AssureQueryManagerIsDefined(
			Query q)
		{
			if (q == null) throw new Exception("Query not defined");

			QueryManager qm = q.QueryManager as QueryManager;
			if (qm == null)  qm = new QueryManager();

			qm.LinkMember(q);

			qm.CompleteInitialization();
			return qm;
		}

/// <summary>
/// Setup the QueryManager members for the specified query 
/// </summary>
/// <param name="q"></param>
/// <param name="outputDest"></param>
/// <param name="qe"></param>
/// <param name="resultsKeys"></param>

		public void Initialize(
			Query q,
			OutputDest outputDest,
			QueryEngine qe,
			List<string> resultsKeys)
		{
			ResultsFormat rf = new ResultsFormat(null, outputDest);
			Initialize(q, rf, qe, resultsKeys);
			return;
		}

		/// <summary>
		/// Setup the QueryManager members for the specified query 
		/// </summary>
		/// <param name="q"></param>
		/// <param name="rf"></param>
		/// <param name="qe"></param>
		/// <param name="resultsKeys"></param>

		public void Initialize(
			Query q,
			ResultsFormat rf,
			QueryEngine qe,
			List<string> resultsKeys)
		{
			QueryManager qm = this;

			qm.Query = q; // query

			ResultsFormat = null;
			ResultsFormatter = null;
			DataTableManager = null;
			DataTable = null;

			CompleteInitialization(rf);

			q.ResultKeys = resultsKeys; // set keys for quick search
			qm.DataTableManager.ResultsKeys = resultsKeys; 

			qm.QueryEngine = qe; // qe2; // update the QM with the new QE

			qm.LinkMembers();

			return;
		}

/// <summary>
/// Complete initialization for Grid output
/// </summary>

		public void CompleteInitialization()
		{
			CompleteInitialization(OutputDest.WinForms);
			return;
		}

/// <summary>
/// Complete initialization for specified OutputDest
/// </summary>

		public void CompleteInitialization(OutputDest outputDest)
		{
			ResultsFormat rf = ResultsFormat;
			if (rf != null)
				rf.OutputDestination = outputDest;

			else rf = new ResultsFormat(this, outputDest);

			CompleteInitialization(rf);
			return;
		}

/// <summary>
/// Complete any missing base QueryManager entries with default values
/// Note: graphics control entries are not initialized
/// </summary>

		public void CompleteInitialization(ResultsFormat rf)
		{

			if (Query == null) throw new Exception("Query not defined");
			if (rf == null) throw new Exception("ResultsFormat not defined");

			QueryManager qm = this;
			ResultsFormat = rf;

			ResultsFormatFactory fmtFactory = new ResultsFormatFactory(qm, rf);
			fmtFactory.Build(); // build ResultsFormat

			ResultsFormatter fmtr = qm.ResultsFormatter;
			if (fmtr == null) // be sure we have a results formatter
				fmtr = new ResultsFormatter(qm);

			DataTableManager dtm = DataTableManager;
			if (dtm == null) // build DataTableManager and DataTableMx
				dtm = new DataTableManager(qm);

			if (qm.DataTable == null || qm.DataTable.Columns.Count == 0)
				qm.DataTable = DataTableManager.BuildDataTable(qm.Query); // build data table to receive data

			return;
		}

/// <summary>
/// Complete setup & execute query
/// </summary>
/// <param name="q"></param>
/// <returns></returns>

		public DialogResult ExecuteQuery(ref Query q)
		{
			List<string> keys;

			return ExecuteQuery(ref q, out keys);
		}

/// <summary>
/// Complete setup & execute query
/// </summary>
/// <param name="q"></param>
/// <param name="keys"></param>
/// <returns></returns>

		public DialogResult ExecuteQuery(ref Query q, out List<string> keys)
		{
			Query modifiedQuery = null;
			DialogResult dr;

			keys = null;
			try
			{
				dr = QueryExec.ValidateQuery(q);
				if (dr == DialogResult.Cancel) return dr;
				modifiedQuery = QueryEngine.DoPresearchTransformations(q); // do any presearch transforms
			}
			catch (Exception ex) // failed a precheck condition
			{
				throw ex;
			}

			if (modifiedQuery != null) q = modifiedQuery; // if modified then replace original query with this query

			Query = q; // link in query
			q.QueryManager = this;

			DataTableManager dtm = new DataTableManager(this);

			ResultsFormatFactory fmtFactory = new ResultsFormatFactory(this, OutputDest.TextFile);
			fmtFactory.Build(); // build ResultsFormat

			DataTable = DataTableManager.BuildDataTable(q); // build data table to receive data

			QueryEngine = new QueryEngine();

			keys = QueryEngine.ExecuteQuery(q);
			DataTableManager.StartRowRetrieval();
			return DialogResult.OK;
		}

		/// <summary>
		/// Return true if more DataRows are available
		/// </summary>

		//public bool MoreDataRowsAvailable
		//{
		//  get { return DataTableManager.MoreDataRowsAvailable; }
		//}

/// <summary>
/// Retrieve next data row
/// </summary>
/// <returns></returns>

		public DataRowMx NextRow()
		{
			return DataTableManager.FetchNextDataRow();
		}

/// <summary>
/// Reset position of fetch cursor in DataTable
/// </summary>

		public void ResetDataTableFetchPosition()
		{
			DataTableManager.ResetDataTableFetchPosition();
			return;
		}

/// <summary>
/// Be sure that the base query secondary criteria match those in the supplied QueryColumn
/// </summary>
/// <param name="qc"></param>

		public void SyncBaseSecondaryCriteria(QueryColumn qc)
		{
		}

/// <summary>
/// Remove a subquery from its parent and optionally any associated views
/// </summary>
/// <param name="sq"></param>
/// <param name="removeResultsViews"></param>
/// <param name="removeResultsPagesIfEmpty"></param>

		public static void RemoveSubQuery(
			Query sq, 
			bool removeResultsViews, 
			bool removeResultsPagesIfEmpty)
		{

			if (sq == null) throw new Exception("Null subquery");
			Query pq = sq.Parent;
			if (pq == null || !pq.Subqueries.Contains(sq))
				throw new Exception("Subquery not contained in query");
			sq.Parent = null;

			pq.Subqueries.Remove(sq); // remove subquery from query

			if (!removeResultsViews) return;

			ResultsPages rPages = pq.ResultsPages; // overall results pages
			sq.ResultsPages = null;

			List<ResultsViewProps> rViews = sq.GetResultsViews();

			if (rViews != null) // any views for subquery?
			{
				for (int vi = 0; vi < rViews.Count; vi++)
				{ // remove any views from overall query ResultsPages
					ResultsViewProps rv = rViews[vi];
					if (rv == null || rv.ResultsPage == null) continue;
					ResultsPage rp = rv.ResultsPage;
					if (rp == null || !rp.Views.Contains(rv)) continue;
					rp.Views.Remove(rv);

					if (removeResultsPagesIfEmpty && rp.Views.Count == 0 && // remove page if empty & requested
						rPages != null && rPages.Pages != null && rPages.Pages.Contains(rp))
					{
						rPages.Remove(rp);
					}
				}
			}

		}

		/// <summary>
		/// Return true if the DTM is in the process of formatting a results set 
		/// </summary>
		/// <returns></returns>

		public bool ContainsRenderedResults()
		{
			if (QueryEngine != null &&
				DataTableManager != null &&
				DataTable != null &&
				ResultsFormat != null &&
				ResultsFormatter != null)
				return true;

			else return false;
		}

/// <summary>
/// Create a clone
/// </summary>
/// <returns></returns>

		public QueryManager Clone()
		{
			return (QueryManager)this.MemberwiseClone();
		}

		/// <summary>
		/// Free resources linked to this instance
		/// </summary>

		public void Dispose()
		{
			try
			{
				DisposeOfControls();

				if (Query != null) Query.FreeControlReferences(); // just free control references
				if (MoleculeGrid != null)	MoleculeGrid.Dispose();
				if (QueryEngine != null) QueryEngine.Dispose();
				if (DataTableManager != null) DataTableManager.Dispose();

				if (ResultsFormat != null) ResultsFormat.QueryManager = null;
				if (ResultsFormatter != null) ResultsFormatter.QueryManager = null;
				if (StatusBarManager != null) StatusBarManager.QueryManager = null;
				if (QueryExec != null) QueryExec.QueryManager = null;

				QueryResultsControl = null;
				Query = null;
				DataTableManager = null;
				DataTable = null;
				ResultsFormat = null;
				ResultsFormatter = null;
				StatusBarManager = null;
				MoleculeGrid = null;
				QueryExec = null;
				QueryEngine = null;
			}

			catch (Exception ex)
			{
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
			}
		}

		/// <summary>
		/// Dispose of any windows control resources associated with the QM or the Query
		/// </summary>

		public void DisposeOfControls()
		{
			if (QueryResultsControl != null) // properly dispose of any DX/Mobius controls in ResultsPageControlContainer
				QueryResultsControl.DisposeOfChildMobiusControls(QueryResultsControl.ResultsPageControlContainer);

			return;
		}

	}
}
