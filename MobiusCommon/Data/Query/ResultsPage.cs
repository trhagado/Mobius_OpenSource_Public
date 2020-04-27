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
	/// ResultsPage - Contains a list of views that appear on the page 
	/// </summary>

	public class ResultsPage
	{
		public string Name  // internal name of page (e.g. Mobius_Main, SpotfireLink_12345)
		{
			get => _name;
			set => _name = value;
		}
		string _name = "";

		public string Title  // title of the page that appears in the tab
		{
			get => _title;
			set => _title = value;
		}
		string _title = "";

		public int ActiveViewIndex = -1; // index of the active view on the page
		public ResultsViewProps ActiveView // the active view 
		{
			get => GetActiveView(); 
			set => SetActiveView(value);
		}

		public bool ShowFilterPanel = false; // if true the filter panel should be shown
		public bool ShowDetailsOnDemand = false; // if true details on demand should be shown
		public bool MainViewPanelMaximized = false; // overrides other show parameters (not currently used)
		public IResultsPagePanel ResultsPagePanel; // the panel that this page is mapped to at run time

		public string LayoutXml = ""; // DevExpress XML for layout DockPanels
		public bool TabbedLayout = false; // if true show page in tabbed layout but don't store layout in LayoutXml
		public ViewLayout LastLayout = ViewLayout.SideBySide; // last layout selected by user

		public List<ResultsViewProps> Views = new List<ResultsViewProps>(); // list of views on the page

/// <summary>
/// Constructor
/// </summary>

		public ResultsPage()
		{
			return;
		}

/// <summary>
/// GetActiveView
/// </summary>
/// <returns></returns>

		private ResultsViewProps GetActiveView()
			{
				if (ActiveViewIndex < 0 && Views.Count > 0) // set first view as active if no view selected as active
					ActiveViewIndex = 0;

				if (ActiveViewIndex >= 0 && ActiveViewIndex < Views.Count) 
					return Views[ActiveViewIndex];
				else return null;
			}

/// <summary>
/// SetActiveView
/// </summary>
/// <param name="value"></param>

		private void SetActiveView (ResultsViewProps value)
			{
				for (int vi = 0; vi < Views.Count; vi++)
				{
					if (value == Views[vi])
					{
						ActiveViewIndex = vi;
						return;
					}
				}

				ActiveViewIndex = -1; // not found 
			}

		/// <summary>
		/// Set the initial title for the page within a set of pages
		/// </summary>
		/// <param name="page"></param>

		public void SetDefaultTitle(
			ResultsPages resultsPages)
		{
			string baseTitle = "Results Page";

			if (Views.Count == 1)
			{
				baseTitle = Views[0].Title;
			}

			Title = AddSequentialSuffixToTitle(baseTitle, resultsPages);
			return;
		}

/// <summary>
/// Add suffix to title to make it unique
/// </summary>
/// <param name="title"></param>
/// <param name="resultsPages"></param>
/// <returns></returns>

		public string AddSequentialSuffixToTitle(
			string title,
			ResultsPages resultsPages)
		{
			int max, intVal;

			max = -1;

			foreach (ResultsPage thisPage in resultsPages.Pages) // get max number of this chart type
			{
				if (Lex.Eq(thisPage.ActiveTitle, title))
				{
					max = 1;
					continue;
				}

				if (!Lex.StartsWith(thisPage.ActiveTitle, title)) continue;

				string tok = thisPage.ActiveTitle.Substring(title.Length).Trim();
				if (!int.TryParse(tok, out intVal)) continue;

				if (intVal > max) max = intVal;
			}

			if (max > 0) title += " " + (max + 1);
			return title;
		}

		/// <summary>
		/// Get the current active label for the page
		/// </summary>

		public string ActiveTitle
		{
			get
			{
				if (Views.Count == 1 && // if a single view then use the image associated with the view
				 (Lex.IsUndefined(Title) || Lex.StartsWith(Title, "Results Page"))) // and no default title
					return Views[0].Title;

				else return Title; // return page title
			}
		}

		/// <summary>
		/// Get the first view for the page
		/// </summary>

		public ResultsViewProps FirstView
		{
			get
			{
				if (Views == null || Views.Count == 0) return null;
				else return Views[0];
			}
		}

		/// <summary>
		/// Page description
		/// </summary>

		public string Description = "";

		/// <summary>
		/// Add a new view to the page
		/// </summary>
		/// <returns></returns>

		public ResultsViewProps AddNewView(
			ResultsViewType viewType,
			string viewSubtype,
			Query query)
		{
			ResultsViewModel rvm = new ResultsViewModel();
			rvm.ViewType = viewType;
			rvm.ViewSubtype = viewSubtype;
			rvm.Query = query;

			ResultsViewProps view = ResultsViewProps.NewResultsView(rvm);
			return view;
		}

		public ResultsViewProps AddNewView(
			ResultsViewModel rvm)
		{
			ResultsViewProps view = ResultsViewProps.NewResultsView(rvm);
			view.SetDefaultTitle(this, rvm.ViewType, rvm.Query);
			AddView(view);
			return view;
		}

		/// <summary>
		/// Add an existing view to the page
		/// </summary>
		/// <param name="view"></param>

		public void AddView(ResultsViewProps view)
		{
			Views.Add(view);
			view.ResultsPage = this;
		}

/// <summary>
/// Remove view from page
/// </summary>
/// <param name="view"></param>

		public bool RemoveView(ResultsViewProps view)
		{
			int vi = Views.IndexOf(view);
			if (vi < 0) return false;

			Views.RemoveAt(vi);
			if (vi == Views.Count - 1)
				ActiveViewIndex--;

			return true; // removed
		}

/// <summary>
/// Get index of specified view
/// </summary>
/// <param name="view"></param>
/// <returns></returns>

		public int GetViewIndex(ResultsViewProps view)
		{
			return Views.IndexOf(view);
		}

		/// <summary>
		/// Serialize the views on the page
		/// </summary>
		/// <param name="tw"></param>
		/// <returns></returns>

		public void Serialize(XmlTextWriter tw)
		{
			tw.WriteStartElement("ResultsPage");
			tw.WriteAttributeString("Name", Name);
			tw.WriteAttributeString("Title", Title);

			tw.WriteAttributeString("LayoutXml", LayoutXml.ToString());
			tw.WriteAttributeString("TabbedLayout", TabbedLayout.ToString());
			tw.WriteAttributeString("LastLayout", LastLayout.ToString());

			tw.WriteAttributeString("ActiveView", ActiveViewIndex.ToString());
			tw.WriteAttributeString("ShowFilterPanel", ShowFilterPanel.ToString());
			tw.WriteAttributeString("ShowDetailsOnDemand", ShowDetailsOnDemand.ToString());

			foreach (ResultsViewProps view in Views)
				view.Serialize(tw);
			tw.WriteEndElement();
		}

		/// <summary>
		/// Deserialize the views on the page
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static ResultsPage Deserialize(Query q, XmlTextReader tr)
		{
			Enum iEnum = null;
			string pageTitle = "", pageName = "";

			ResultsPage page = new ResultsPage();

			if (!Lex.Eq(tr.Name, "ResultsPage"))
				throw new Exception("ResultsPage element not found");

			if (XmlUtil.GetStringAttribute(tr, "Name", ref pageName))
				page.Name = pageName;

			if (XmlUtil.GetStringAttribute(tr, "Title", ref pageTitle))
				page.Title = pageTitle;

			XmlUtil.GetStringAttribute(tr, "LayoutXml", ref page.LayoutXml);
			XmlUtil.GetBoolAttribute(tr, "TabbedLayout", ref page.TabbedLayout);
			if (XmlUtil.GetEnumAttribute(tr, "LastLayout", typeof(ViewLayout), ref iEnum))
				page.LastLayout = (ViewLayout)iEnum;

			XmlUtil.GetIntAttribute(tr, "ActiveView", ref page.ActiveViewIndex);

			XmlUtil.GetBoolAttribute(tr, "ShowFilterPanel", ref page.ShowFilterPanel);
			XmlUtil.GetBoolAttribute(tr, "ShowDetailsOnDemand", ref page.ShowDetailsOnDemand);

			if (tr.IsEmptyElement) return page;

			while (true)
			{
				tr.Read(); tr.MoveToContent();

				if (tr.NodeType == XmlNodeType.EndElement && Lex.Eq(tr.Name, "ResultsPage"))
					break;

				ResultsViewProps view = ResultsViewProps.Deserialize(q, tr);
				page.Views.Add(view); // add the view to the list of views for the page
				view.ResultsPage = page; // link the view to the page
			}

			return page;
		}

		/// <summary>
		/// Get the image to use for the page header/tab
		/// </summary>

		public Image PageHeaderImage
		{
			get
			{
				string imageName = "Undefined";

				if (Views.Count == 1) // if single view show type in page tab
					imageName = Views[0].ViewTypeImageName;

				Image image = Bitmaps.GetImageFromName(Bitmaps.I.ViewTypeImages, imageName, true);
				return image;
			}
		}

		/// <summary>
		/// Clear any pointers to UI resources (e.g. forms, controls)
		/// </summary>

		public void FreeControlReferences()
		{
			ResultsPagePanel = null;
			return;
		}


	} // ResultsPage

	/// <summary>
	/// Placeholder interface for ResultsPagePanel
	/// </summary>

	public interface IResultsPagePanel
	{
		// currently no members
	}

/// <summary>
/// How the views are laid out on the ResultsPage
/// </summary>

	public enum ViewLayout
	{
		SideBySide = 0,
		Stacked = 1,
		RowsAndCols = 2,
		Tabbed = 3
	}

}
