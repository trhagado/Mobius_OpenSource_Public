using Mobius.ComOps;

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
/// Pages containing the current views onto the retrieved data and its derivatives
/// </summary>

	public class ResultsPages 
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public List<ResultsPage> Pages = new List<ResultsPage>(); // the list of pages for the query
		public Query RootQuery; // associated root query, a particular page or view may be associated with the root or another query derived from the root

/// <summary>
/// Basic constructor
/// </summary>

		public ResultsPages()
		{
			return;
		}

		/// <summary>
		/// Indexer into pages
		/// </summary>
		/// <param name="cpi"></param>
		/// <returns></returns>

		public ResultsPage this[int cpi]
		{
			get
			{
				AssertMx.IsTrue(cpi >= 0 && cpi < Pages.Count);
				return Pages[cpi];
			}

			set
			{
				AssertMx.IsTrue(cpi >= 0 && cpi < Pages.Count);
				Pages[cpi] = value;
			}
		}

		/// <summary>
		/// Allocate ResultsPages
		/// </summary>
		/// <returns></returns>

		public ResultsPages NewResultsPages(Query rootQuery)
		{
			ResultsPages pages = new ResultsPages();
			pages.RootQuery = rootQuery;
			return pages;
		}

		/// <summary>
		/// Serialize the ResultsPages in a query
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public void Serialize(XmlTextWriter tw)
		{
			if (Pages == null || Pages.Count == 0) return;

			if (Pages.Count == 1 && Pages[0].Views.Count == 1 &&
			 Pages[0].Views[0].ViewType == ResultsViewType.Table)
				return; // don't serialize if just a single table view since this is the default

			tw.WriteStartElement("ResultsPages");
			foreach (ResultsPage page in Pages)
				page.Serialize(tw);
			tw.WriteEndElement();
		}

		/// <summary>
		/// Deserialize the results pages in a query
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static ResultsPages Deserialize(Query q, XmlTextReader tr)
		{
			ResultsPages pages = new ResultsPages();
			pages.RootQuery = q;

			if (!Lex.Eq(tr.Name, "ResultsPages"))
				throw new Exception("ResultsPages element not found");

			if (tr.IsEmptyElement) return pages; // no pages

			while (true)
			{
				tr.Read(); tr.MoveToContent();

				if (tr.NodeType == XmlNodeType.EndElement && Lex.Eq(tr.Name, "ResultsPages")) 
					break;

				ResultsPage page = ResultsPage.Deserialize(q, tr);

				if (pages.Pages.Count == 1 && Query.DeserializeTruncateToSingleResultsPage) continue;

				pages.AddPage(page);
			}

			return pages;
		}

/// <summary>
/// SetupQueryPagesAndViews
/// </summary>
/// <param name="query"></param>
/// <returns></returns>

		public void SetupQueryPagesAndViews(
			Query query,
			ResultsViewType tableViewType)
		{
			QueryTable qt;
			ResultsPage page;
			ResultsViewProps view;
			ResultsViewType viewType;
			string name, title;
			bool createdTableView = false;
			int pi;

			List<ResultsPage> oldPages = query.ResultsPages.Pages; // existing pages
			List<ResultsPage> newPages = query.ResultsPages.Pages = new List<ResultsPage>(); // clear ResultsPages new pages built here

// Build/Copy views on data retrieved by the Mobius QE

			if (query.RetrievesMobiusData) // if query retrieves data from Mobius then replace the existing table view with the specified type
			{
				AddNewPageAndView(query, tableViewType, null, out page, out view); // basic table view goes first
				page.Name = view.Name = "TableView";
				page.Title = view.Title = "Table View";

				if (tableViewType == ResultsViewType.Table) // if regular grid table views add other views on Mobius QE data
				{
					for (int opi = 1; opi < oldPages.Count; opi++)
					{
						ResultsPage op = oldPages[opi];

					//if (op.Views.Count > 0 && op.Views[0].IsSpotfireUrlDrivenVisualization) continue; // Spotfire link view added later, not here

						newPages.Add(op);
					}
				}

			}

			return;
		}

		/// <summary>
		/// Add a default results page and table view if no views defined
		/// </summary>
		/// <returns></returns>

		public bool AddDefaultPageAndViewIfNeeded(
			Query query)
		{
			ResultsPage page;
			ResultsViewProps view;

			if (Pages.Count == 0)
			{
				AddNewPageAndView(query, ResultsViewType.Table, null, out page, out view);
				return true;
			}

			else if (Pages[0].Views.Count == 0)
			{
				view = Pages[0].AddNewView(ResultsViewType.Table, null, query);
				return true;
			}

			else return false;
		}

		/// <summary>
		/// Add a new results page with a view of the specified type
		/// </summary>
		/// <param name="query"></param>
		/// <param name="viewType"></param>
		/// <returns></returns>

		public static void AddNewPageAndView(
			Query query,
			ResultsViewType viewType,
			string viewSubtype,
			out ResultsPage page,
			out ResultsViewProps view)
		{
			page = AddNewPage(query);
			view = ResultsViewProps.NewResultsView(query, page, viewType, viewSubtype); // create the view & add to page
			return;
		}

/// <summary>
/// Add a new page for the specified query
/// </summary>
/// <param name="query"></param>
/// <returns></returns>

		public static ResultsPage AddNewPage(
			Query query)
		{
			int max, intVal;

			ResultsPages pages = query.Root.ResultsPages; 
			if (pages == null)
			{
				pages = new ResultsPages();
				query.Root.ResultsPages = pages;
			}

			ResultsPage page = pages.AddNewPage();
			return page;
		}

		/// <summary>
		/// Add a new page to the set of results pages
		/// </summary>
		/// <returns></returns>

		public ResultsPage AddNewPage()
		{
			ResultsPage page = new ResultsPage(); // create the page
			page.SetDefaultTitle(this);

			if (Pages.Count > 0) // if other pages use last page as model for extra windows to show
			{
				ResultsPage lastPage = Pages[Pages.Count - 1];
				page.ShowFilterPanel = lastPage.ShowFilterPanel;
				page.ShowDetailsOnDemand = lastPage.ShowDetailsOnDemand;
			}

			AddPage(page); // add page to query

			return page;
		}

		/// <summary>
		/// Add an existing page to the list of pages
		/// </summary>
		/// <param name="page"></param>

		public void AddPage(ResultsPage page)
		{
			Pages.Add(page);
		}

		/// <summary>
		/// Remove the page at the specified index
		/// </summary>
		/// <param name="index"></param>

		public void RemoveAt(int index)
		{
			Remove(Pages[index]);
		}

		/// <summary>
		/// Remove the specified page
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>

		public bool Remove(ResultsPage page)
		{
			if (!Pages.Remove(page)) return false;

			RemoveSubqueriesWithNoViews(RootQuery);

			return true;
		}

		/// <summary>
		/// Remove any subqueries that have no views remaining
		/// </summary>
		/// <param name="q"></param>

		public void RemoveSubqueriesWithNoViews(Query q)
		{
			int pi;

			q.NormalizeQueryIds();

			Dictionary<int, object> queryDict = new Dictionary<int, object>();

			ResultsPages pages = q.ResultsPages;
			foreach (ResultsPage rp in pages.Pages)
			{
				foreach (ResultsViewProps v in rp.Views)
				{
					queryDict[v.BaseQuery.InstanceId] = null;
				}
			}

			int sqi = 0;
			while (sqi < q.Subqueries.Count)
			{
				if (queryDict.ContainsKey(q.InstanceId)) sqi++;
				else q.Subqueries.RemoveAt(sqi);
			}

			return;
		}

/// <summary>
/// Return first view for ResultsPages
/// </summary>

		public ResultsViewProps FirstView
		{
			get
			{
				if (Pages == null || Pages.Count == 0) return null;
				return Pages[0].FirstView;

			}
		}

/// <summary>
/// Get the index of the page, if any, that contains the root query MoleculeGridView
/// </summary>
/// <returns></returns>

		public int GetRootQueryMoleculeGridPageIndex()
		{
			for (int pi = 0; pi < Pages.Count; pi++)
			{
				ResultsPage rp = Pages[pi];
				foreach (ResultsViewProps v in rp.Views)
				{
					if (v.BaseQuery == RootQuery && v.ViewType == ResultsViewType.Table) return pi;
				}
			}

			return -1; // not found
		}

	}
}
