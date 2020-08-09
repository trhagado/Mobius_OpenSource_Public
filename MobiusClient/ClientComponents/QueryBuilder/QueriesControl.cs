using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;

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
	/// Control to contain the collection of open queries
	/// It contains a single XtraTabControl named Tabs that contains on tab for
	/// each open query.
	/// </summary>

	public partial class QueriesControl : XtraUserControl
	{
		public static QueriesControl Instance; // first instance created

		public List<Document> DocumentList = new List<Document>(); // ordered list of documents (queries), one per tab

		public int CurrentQueryIndex = -1; // index of current query

		public QueryTablesControl Qtc // current query table
		{
			get { return QueriesControl.Instance.QueryBuilderControl.QueryTablesControl; }
		}

		public Query CurrentQuery // current query
		{ // value is stored in associated QueryTablesControl.Query
			get { return Qtc.Query; }
			set { Qtc.Query = value; }
		}

		public Query CurrentBrowseQuery; // query being browsed, may be different than CurrentQuery if there were presearch transformations of the initial query

		Point TabDragPoint = Point.Empty;
		XtraTabPage TabDragPage = null, LastSwappedPage = null;

		public float PrintPreviewAspectRatio;

		static int NewQueryCount = 0; // used for assigning default query names
		const int FixedTabs = 0; // number of fixed tabs in MainTabs

		/// <summary>
		/// Basic constructor
		/// </summary>

		public QueriesControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			while (this.Tabs.TabPages.Count > FixedTabs) // remove all "non-fixed" tabs
				this.Tabs.TabPages.RemoveAt(FixedTabs);

			Instance = this;
		}

		/// <summary>
		/// Find the first QueriesControl on the same form as the specified control
		/// </summary>
		/// <param name="ctl"></param>
		/// <returns></returns>

		public static QueriesControl GetQcFromForm(Control ctl)
		{
			Form form = ctl.FindForm();
			Control[] ctls = form.Controls.Find("QueriesControl", true);
			if (ctls.Length > 0) return ctls[0] as QueriesControl;
			else return null;
		}

		/// <summary>
		/// Get open query by user object id
		/// </summary>
		/// <param name="uoId"></param>
		/// <returns></returns>

		public Query GetQueryByUserObjectId(int uoId)
		{
			int di = GetQueryIndexByUserObjectId(uoId);
			if (di >= 0) return GetQuery(di);

			else return null;
		}

		/// <summary>
		/// Get the query document at the specified position
		/// </summary>
		/// <param name="docIdx"></param>
		/// <returns></returns>

		public Query GetQuery(int docIdx)
		{
			Document mtw = DocumentList[docIdx];
			if (mtw.Type != DocumentType.Query) return null;
			Query q = (Query)mtw.Content;
			return q;
		}

	/// <summary>
	/// Open a new query
	/// </summary>
	/// <returns></returns>

	public Query NewQuery()
		{
			return NewQuery(null);
		}

		/// <summary>
		/// Open a new query with specified name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public Query NewQuery(
			string name)
		{
			int i1, i2;

			if (String.IsNullOrEmpty(name)) // create a name if not supplied
			{
				for (i1 = NewQueryCount + 1; ; i1++)
				{
					name = "Query" + i1.ToString();
					i2 = GetQueryIndex(name);
					if (i2 < 0) break;
				}

				NewQueryCount = i1;
			}

			else // see if this query is already open
			{
				Query q2 = null;
				i1 = GetQueryIndex(name);
				if (i1 >= 0) // already open, just switch to it
				{
					CurrentQueryIndex = i1;
					CurrentQuery = (Query)DocumentList[CurrentQueryIndex].Content;
					Tabs.SelectedTabPageIndex = CurrentQueryIndex;
					Tabs.TabPages[CurrentQueryIndex].Text = name; // make sure tab title is correct
					SessionManager.SetShellTitle(name); // set main window title
					Qtc.Render(CurrentQuery);
					return CurrentQuery;
				}
			}

			Query newQuery = new Query();
			newQuery.Name = name;

			QbUtil.AddQueryAndRender(newQuery, false);

			//AddQuery(newQuery);
			//QbUtil.SetMode(QueryMode.Build); // be sure in build mode
			return newQuery;
		}

		/// <summary>
		/// Add a query
		/// </summary>
		/// <param name="newQuery"></param>

		public void AddQuery(Query newQuery)
		{
			string name = newQuery.UserObject.Name;
			Document newMtw = new Document();
			newMtw.Type = DocumentType.Query;
			newMtw.Content = newQuery;
			DocumentList.Add(newMtw);
			CurrentQueryIndex = DocumentList.Count - 1;

			//newQuery.ShowStereoComments = SS.I.ShowStereoComments; // preference for initial value (no, may overwrite value for existing query)

			CurrentQuery = newQuery;
			XtraTabPage tp = Tabs.TabPages.Add(name);
			tp.ImageIndex = 0;
			Tabs.SelectedTabPageIndex = DocumentList.Count - 1;

			QbUtil.SetMode(QueryMode.Build);
			//Qtc.Render(CurrentQuery);
			SessionManager.SetShellTitle(name); // set main window title
			return;
		}

		public void DuplicateQuery()
		{
			Query q = CurrentQuery;
			int qi = GetQueryIndex(q); // index of query to dup
			if (qi != CurrentQueryIndex) qi = qi; // debug
			if (qi < 0 || qi >= DocumentList.Count) return;
			qi++; // where duplicate query will go

			Query q2 = q.Clone(); // make copy query
			q2.UserObject = new UserObject(UserObjectType.Query); // reset userobject info
			q2.Name = q.Name; // set base name to build new name from
			AssignUniqueQueryName(q2);

			Document doc = new Document();
			doc.Type = DocumentType.Query;
			doc.Content = q2;
			DocumentList.Insert(qi, doc);
			CurrentQuery = q2;
			CurrentQueryIndex = qi;

			XtraTabPage tp = new XtraTabPage();
			tp.ImageIndex = 0;
			tp.Text = q2.Name;
			Tabs.TabPages.Insert(qi, tp);
			Tabs.SelectedTabPageIndex = qi;

			QbUtil.SetMode(QueryMode.Build);
			//Qtc.Render(CurrentQuery);
			SessionManager.SetShellTitle(q2.Name); // set main window title
			return;
		}

		/// <summary>
		/// Assign a unique query name within the DocumentList of queries based on the supplied query
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>

		public void AssignUniqueQueryName(
			Query q)
		{
			Query q2;
			int di;

			string name = q.Name;

			while (true)
			{
				for (di = 0; di < DocumentList.Count; di++) // see if any query matches
				{
					Document doc = DocumentList[di];
					if (doc.Type != DocumentType.Query) continue;

					q2 = doc.Content as Query;
					if (q2 == q) continue; // ignore incoming query

					if (name == q2.Name) break;
				}

				if (di >= DocumentList.Count) break; // break if no match found

				name = Lex.IncrementIntegerSuffix(name);
			}

			q.Name = name;
			return;
		}

		/// <summary>
		/// Set the current query to a new query object
		/// </summary>
		/// <param name="q"></param>

		public void SetCurrentQueryInstance(
			Query q)
		{
			q.UserObject = CurrentQuery.UserObject; // keep same name
			CurrentQuery = q;
			DocumentList[CurrentQueryIndex].Content = q;
			return;
		}

		/// <summary>
		/// Select the specified tab (e.g. query)
		/// </summary>
		/// <param name="tabIndex"></param>

		public void SelectQuery(int queryIndex)
		{
			if (queryIndex >= DocumentList.Count) return; // shouldn't happen
			SS.I.UISetupLevel++;

			CurrentQueryIndex = queryIndex;
			Document doc = DocumentList[queryIndex];
			CurrentQuery = (Query)doc.Content; // restore query
			Query q = CurrentQuery;
			if (q.PresearchDerivedQuery != null) // restore any browse query
				CurrentBrowseQuery = q.PresearchDerivedQuery;
			else CurrentBrowseQuery = q;

			Tabs.TabPages[CurrentQueryIndex].Text = q.UserObject.Name; // make sure tab title is correct
			SessionManager.SetShellTitle(q.UserObject.Name); // set main window title
			Tabs.SelectedTabPageIndex = queryIndex; // select the tab if not done yet
			SS.I.UISetupLevel--;

			if (q.Mode == QueryMode.Unknown || q.Mode == QueryMode.Build)
			{
				QbUtil.SetMode(QueryMode.Build); // check & adjust build mode details
				Qtc.Render(q);
				SessionManager.DisplayCurrentCount();
			}

			else // reenter browse mode
			{
				QbUtil.SetMode(QueryMode.Browse); // check & adjust build mode details
				QueryExec.RunQuery(q, OutputDest.WinForms, OutputFormContext.Session, browseExistingResults: true);
				SessionManager.DisplayFilterCountsAndString();
			}

		}

		/// <summary>
		/// Remove the specified TabPage
		/// </summary>
		/// <param name="tabIndex"></param>

		public void RemoveTab(int tabIndex)
		{
			SS.I.UISetupLevel++;
			int stpi = Tabs.SelectedTabPageIndex;

			if (stpi == tabIndex) // removing selected tab?
			{ // change index to another tab to avoid big red X in tab control
				if (stpi < Tabs.TabPages.Count - 1) Tabs.SelectedTabPageIndex++; // move to next if not last
				else if (stpi > 0) Tabs.SelectedTabPageIndex--; // otherwise move to prev if not first
			}
			if (tabIndex < Tabs.TabPages.Count)
			{
				try // remove all controls contained in tab
				{
					XtraTabPage tp = Tabs.TabPages[tabIndex];
					while (tp.Controls.Count > 0)
					{
						tp.Controls.RemoveAt(0);
					}
				}
				catch (Exception ex) { ex = ex; }

				Tabs.TabPages.RemoveAt(tabIndex);
			}
			SS.I.UISetupLevel--;
		}

		/// <summary>
		/// Set title on query tab for current query
		/// </summary>
		/// <param name="title"></param>

		public static void SetQueryTabTitle
			(string title)
		{
			if (Instance.CurrentQueryIndex < 0 || Instance.CurrentQuery == null || Instance.CurrentQuery.UserObject == null ||
				String.IsNullOrEmpty(Instance.CurrentQuery.UserObject.Name)) return;

			Instance.Tabs.TabPages[Instance.CurrentQueryIndex].Text = title; // Instance.CurrentQuery.UserObject.Name;
			return;
		}

		/// <summary>
		/// Find a query by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public Query FindQueryByName(
			string name)
		{
			int qi = GetQueryIndex(name);
			if (qi < 0) return null;

			Query q = GetQuery(qi);
			return q;
		}

	/// <summary>
	/// Get index of any open window with same name as supplied query
	/// </summary>
	/// <param name="uo"></param>
	/// <returns></returns>

	public int GetQueryIndex(
			Query q)
		{
			return GetQueryIndex(q.UserObject.Name);
		}

		/// <summary>
		/// Get index of any open window with supplied name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public int GetQueryIndex(
			string name)
		{
			int i1;
			for (i1 = 0; i1 < DocumentList.Count; i1++) // see if already open
			{
				Query q = GetQuery(i1);
				if (q != null && Lex.Eq(q.UserObject.Name, name)) break;
			}

			if (i1 < DocumentList.Count) return i1;
			else return -1; // not found
		}

		/// <summary>
		/// GetQueryIndexByUserObjectId
		/// </summary>
		/// <param name="uoId"></param>
		/// <returns></returns>

		public int GetQueryIndexByUserObjectId(
			int uoId)
		{
			if (uoId <= 0) return -1; // not a valid uo id

			int i1;

			for (i1 = 0; i1 < DocumentList.Count; i1++) // see if already open
			{
				Query q = GetQuery(i1);
				if (q != null && q.UserObject.Id == uoId) break;
			}

			if (i1 < DocumentList.Count) return i1;
			else return -1; // not found
		}

		/// <summary>
		/// Switching to a different query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tabs_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
		{
			if (Tabs.SelectedTabPageIndex == CurrentQueryIndex || SS.I.UISetupLevel >= 0) return;

			if (Tabs.SelectedTabPageIndex >= 0) SelectQuery(Tabs.SelectedTabPageIndex);
		}

		/// <summary>
		/// Close current query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tabs_CloseButtonClick(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("FileClose");
		}

		/// <summary>
		/// Get the base, untransformed query
		/// </summary>
		/// <returns></returns>

		internal static Query BaseQuery
		{
			get
			{
				if (SessionManager.Instance == null) return null;
				QueriesControl qCtl = SessionManager.Instance.QueriesControl;
				if (qCtl == null) return null;
				Query q = qCtl.CurrentQuery;
				return q;
			}
		}

		/// <summary>
		/// Attempt to get the corresponding QueryTable in the base, untransformed query
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		internal static QueryTable BaseQueryQt(QueryTable qt)
		{
			Query q = BaseQuery;
			if (q == null) return qt;
			QueryTable qt0 = q.GetQueryTableByName(qt.MetaTable.Name);
			if (qt0 == null) return qt;
			else return qt0;
		}

		/// <summary>
		/// Attempt to get the corresponding QueryColumn in the base, untransformed query
		/// </summary>
		/// <param name="qc"></param>
		/// <returns></returns>

		internal static QueryColumn BaseQueryQc(QueryColumn qc)
		{
			Query q = BaseQuery;
			if (q == null) return qc;
			QueryTable qt = q.GetQueryTableByName(qc.QueryTable.MetaTable.Name);
			if (qt == null) return qc;
			QueryColumn qc0 = qt.GetQueryColumnByName(qc.MetaColumn.Name);
			if (qc0 == null) return qc;
			else return qc0;
		}

		private void Tabs_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Tabs_MouseClick(object sender, MouseEventArgs e)
		{
			return;
		}

		////////////////////////////////////////////////
		///////////////// Query Menu ////////////////////
		////////////////////////////////////////////////

		/// <summary>
		/// Display query command menu on a rt-click on the tab header for a query
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tabs_MouseUp(object sender, MouseEventArgs e)
		{ 
			if (e.Button == MouseButtons.Right)
			{
				XtraTabHitInfo hitInfo = Tabs.CalcHitInfo(Tabs.PointToClient(Control.MousePosition)); 
				if (hitInfo.HitTest == XtraTabHitTest.PageHeader)
				{
					XtraTabPage tp = hitInfo.Page;
					string tabName = (tp.Tag != null) ? tp.Tag.ToString() : "";

					int x = Cursor.Position.X;
					int y = Cursor.Position.Y;
					Point p = new Point(x, y);
					p = this.PointToClient(p);
					QueryContextMenu.Show(this, p);
				}
			}
		}

		private void QuerySaveMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QuerySave");
			CommandExec.ExecuteCommandAsynch("FileSave");
		}

		private void QuerySaveAsMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QuerySaveAs");
			CommandExec.ExecuteCommandAsynch("FileSaveAs");
		}

		public void QueryRunMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryRun");
			CommandExec.ExecuteCommandAsynch("RunQuery");
		}

		private void QueryOptionsMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryOptions");
			CommandExec.ExecuteCommandAsynch("QueryOptions");
		}

		private void DuplicateMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryDuplicate");
			CommandExec.ExecuteCommandAsynch("DuplicateQuery");
		}

		private void QueryRemoveAllCriteriaMenuItem_Click(object sender, EventArgs e)
		{
			SessionManager.LogCommandUsage("QueryRemoveAllCriteria");
			CommandExec.ExecuteCommandAsynch("RemoveAllCriteria");
		}

		private void QueryClearMenuItem_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("ClearQuery");
		}

		private void RenameQueryMenuItem_Click(object sender, EventArgs e)
		{
			CommandExec.ExecuteCommandAsynch("RenameQuery");
		}

		//////////////////////////////////////////////////////////////////////////////////
		///////////////// Handle rearranging of tab order for Queries ////////////////////
		//////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Handle rearranging of tab order for Queries
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Tabs_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			XtraTabControl c = sender as XtraTabControl;
			TabDragPoint = new Point(e.X, e.Y);
			XtraTabHitInfo hi = c.CalcHitInfo(TabDragPoint);
			if (hi.Page == null)
			{
				TabDragPoint = Point.Empty;
				hi.Page = null;
			}

			TabDragPage = hi.Page;
			LastSwappedPage = null;
			return;
		}

		private void Tabs_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if ((TabDragPoint != Point.Empty) && ((Math.Abs(e.X - TabDragPoint.X) > SystemInformation.DragSize.Width) || (Math.Abs(e.Y - TabDragPoint.Y) > SystemInformation.DragSize.Height)))
					Tabs.DoDragDrop(sender, DragDropEffects.Move);

				else if (TabDragPoint != Point.Empty) return;
				else return;
			}
		}


		private void Tabs_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			XtraTabControl c = sender as XtraTabControl;
			if (c == null) return;

			XtraTabHitInfo hi = c.CalcHitInfo(c.PointToClient(new Point(e.X, e.Y)));
			if (hi.Page != null)
			{
				if (hi.Page != TabDragPage && hi.Page != LastSwappedPage) // move it
				{
					int hpi = c.TabPages.IndexOf(hi.Page);
					int dpi = c.TabPages.IndexOf(TabDragPage);
					if (hpi < dpi) // moving left
					{
						c.TabPages.Move(hpi, TabDragPage);
					}

					else // moving right
					{
						c.TabPages.Move(hpi + 1, TabDragPage);
					}

					Document doc = DocumentList[dpi]; // move the query
					DocumentList.RemoveAt(dpi);
					if (hpi < 0 || hpi > DocumentList.Count) throw new DataException("DocumentList.Insert out of range");
					DocumentList.Insert(hpi, doc);
					SelectQuery(hpi);
				}

				LastSwappedPage = hi.Page;
				e.Effect = DragDropEffects.Move;
			}

			else e.Effect = DragDropEffects.None;
		}
	} // QueriesControl

	/// <summary>
	/// Document info
	/// </summary>

	public class Document
	{
		public DocumentType Type; // type of document
		public object Content;
	}

/// <summary>
/// Supported document types
/// </summary>

	public enum DocumentType
	{
		Unknown = 0,
		Query   = 1 // query is currently the only document type
	}

}
