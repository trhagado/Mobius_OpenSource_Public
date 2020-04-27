using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;

using Mobius.SpotfireDocument;
using Mobius.SpotfireClient;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{

/// <summary>
/// Basic visual properties dialog (General & Data tabs)
/// </summary>

	public partial class BasicVisualPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{
		static BasicVisualPropertiesDialog Instance = null;
		static string CurrentTabName = "Columns";

		SpotfireViewManager SVM; // view manager associated with this dialog
		SpotfireApiClient Api => SVM?.SpotfireApiClient; 
		SpotfireViewProps SVP => SVM?.SVP; 
		DataTableMapMsx DataMap => SVP?.DataTableMaps?.CurrentMap;  // associated DataMap

		VisualMsx V
		{
			get { return SVP.ActiveVisual as VisualMsx; }
			set { SVP.ActiveVisual = value; }
		}


		bool InSetup = false;

		public BasicVisualPropertiesDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// ShowDialog
		/// </summary>
		/// <param name="svm"></param>
		/// <param name="initialTabName"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			VisualMsx v,
			SpotfireViewManager svm)
		{
			Instance = new BasicVisualPropertiesDialog();

			return Instance.ShowDialog2(v, svm);
		}

		private DialogResult ShowDialog2(
			VisualMsx v,
			SpotfireViewManager svm)
		{
			SVM = svm;
			V = v;

			PropertyDialogsUtil.AdjustPropertyPageTabs(Tabs, TabPageSelector, TabsContainerPanel); ;

			SetupForm(); 

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			return dr;
		}

		///////////////////////////////////////////////////////////////////
		//////////// Common code for setting up the dialog ////////////////
		///////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adjust size and positions of main container controls (done above)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void GeneralAnalysisPropertiesDialog_Shown(object sender, EventArgs e)
		{
			return;
		}

		/// <summary>
		/// Do common setup of tabs in visualization property dialogs
		/// </summary>

		private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (this.SplitContainer.CanFocus)
			{
				this.SplitContainer.ActiveControl = SelectorPanel;
			}
		}

		/// <summary>
		/// The selected page has changed. Show it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void TabPageSelector_AfterSelect(object sender, TreeViewEventArgs e)
		{
			XtraTabPage page = e.Node.Tag as XtraTabPage;
			Tabs.SelectedTabPage = page;
		}

		//////////////////////////////////////////////////////////////////////////
		//////////// End of common code for setting up the dialog ////////////////
		//////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Setup the form
		/// </summary>

		void SetupForm()
		{
			InSetup = true;

			ValidateViewInitialization();

			GeneralPropertiesPanel.Setup(V, EditValueChanged);

			DataMapPanel.Setup(SVM, EditValueChanged);

			PropertyDialogsUtil.SelectPropertyPage(CurrentTabName, TabPageSelector, Tabs);

			InSetup = false;
			return;
		}

		void ValidateViewInitialization()
		{
			if (SVM == null) throw new Exception("ViewManager not defined");

			SVM.ValidateSpotfireViewPropsInitialization();

			if (V == null)
				V = new VisualMsx();

			return;
		}


		/// <summary>
		/// The value of a property has been changed by the user
		/// </summary>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (SpotfireViewManager.ApplyChangesImmediately)
				ApplyPendingChanges();
		}

		/// <summary>
		/// Get any property changes and apply
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void ApplyButton_Click(object sender, EventArgs e)
		{
			ApplyPendingChanges();
			return;
		}

		/// <summary>
		/// Apply any changes still pending and return
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void OkButton_Click(object sender, EventArgs e)
		{
			CurrentTabName = Tabs.SelectedTabPage.Name;

			//ApplyPendingChanges();

			DialogResult = DialogResult.OK;
			return;
		}

		/// <summary>
		/// Retrieve and apply any pending view property changes
		/// </summary>

		void ApplyPendingChanges()
		{
			//if (!changed) return;

			GetFormValues();
			string serializedText = SerializeMsx.Serialize(V); // serialize
			Api.SetVisualProperties(V.Id, serializedText);

			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			CurrentTabName = Tabs.SelectedTabPage.Name;
			Visible = false;
			DialogResult = DialogResult.Cancel;
		}

		private void GeneralAnalysisProperties_FormClosing(object sender, FormClosingEventArgs e)
		{
			//if (Visible) CancelBut_Click(sender, e);

			//ViewMgr.SpotfireViewProps = new SpotfireViewProps();
			//SVP.DataMap = DataMapperControl.GetDataMap();
			return;
		}

		private void Tabs_SelectedPageChanging(object sender, TabPageChangingEventArgs e)
		{
			return;
		}

		private void Tabs_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
		{
			return;
		}


		/// <summary>
		/// Get visualization properties from form 
		/// </summary>

		void GetFormValues()
		{
			VisualMsx v = V; // update existing visual instance

			// General

			GeneralPropertiesPanel.GetValues(v);
				
			// Data
			// --- todo ---

			return;
		}

	}
}
