using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Mobius.SpotfireClient
{
	public partial class QueryViewSelectorDialog : DevExpress.XtraEditors.XtraForm
	{
		public Query SelectedQuery = null;
		public ResultsViewProps SelectedView => (ViewSelectorComboBox?.SelectedItem as ComboBoxViewItem)?.View;

		int ActivationCount = 0;

		public QueryViewSelectorDialog()
		{
			InitializeComponent();
		}

		public DialogResult ShowDialog(
			Query selectedQuery,
			string selectedViewId)
		{
			int selectedQueryId = -1;
			string selectedQueryName = "";

			if (selectedQuery != null)
			{
				selectedQueryId = selectedQuery.UserObject.Id;
				selectedQueryName = selectedQuery.UserObject.Name;
			}

			QuerySelectorControl.Setup(selectedQueryId, selectedQueryName, QueryChangedEventHandler);
			SetupViewSelector(selectedQuery, selectedViewId);
			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		private void QueryViewSelectorDialog_Activated(object sender, EventArgs e)
		{
			ActivationCount++;

			if (QuerySelectorControl.SelectedQueryId < 0 &&  ActivationCount == 1) // show menu on first activation only

				DelayedCallback.Schedule( // schedule callback
					delegate (object state)
					{ QuerySelectorControl.ShowSelectQueryMenu(); });

			else QuerySelectorControl.Focus();
		}

		void QueryChangedEventHandler(object sender, EventArgs e)
		{
			Query q = QbUtil.ReadQuery(QuerySelectorControl.SelectedQueryId);
			if (q == null) throw new Exception("Query not found");

			SelectedQuery = q;
			SetupViewSelector(q, "");

			ViewSelectorComboBox.ShowPopup();

			return;
		}

		void SetupViewSelector(
			Query q,
			string selectedViewId)
		{
			ViewSelectorComboBox.Properties.Items.Clear();
			ViewSelectorComboBox.SelectedItem = null;

			if (q == null) return;

			List<ResultsViewProps> views = q.GetResultsViews();
			foreach (ResultsViewProps v in views)
			{
				ComboBoxViewItem vi = new ComboBoxViewItem(v);
				ViewSelectorComboBox.Properties.Items.Add(vi);

				if (v.Id == selectedViewId)
					ViewSelectorComboBox.SelectedItem = vi;

				else if (ViewSelectorComboBox.SelectedItem == null)
					ViewSelectorComboBox.SelectedItem = vi;
			}
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			if (SelectedQuery == null)
			{
				QuerySelectorControl.Focus();
				MessageBoxMx.ShowError("A query must be selected");
				return;
			}

			if (SelectedView == null)
			{
				ViewSelectorComboBox.Focus();
				MessageBoxMx.ShowError("A view must be selected for the query");
				return;
			}

			DialogResult = DialogResult.OK;
			return;
	}
}

	/// <summary>
	/// Allow views to be stored as ComboBox Items
	/// </summary>

	class ComboBoxViewItem
	{
		public ResultsViewProps View;

		public ComboBoxViewItem(ResultsViewProps view)
		{
			View = view;
		}

		public override string ToString()
		{
			return View != null ? View.Title : "";
		}
	}
}