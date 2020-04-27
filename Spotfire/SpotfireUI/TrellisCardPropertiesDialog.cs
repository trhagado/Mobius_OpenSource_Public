using Mobius.ComOps;
using Mobius.Data;

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
	public partial class TrellisCardPropertiesDialog : DevExpress.XtraEditors.XtraForm
	{
		static TrellisCardPropertiesDialog Instance = null;
		static string CurrentTabName = "Columns";

		SpotfireViewProps SVP; // view properties associated with this dialog

		SpotfireApiClient Api => SpotfireSession.SpotfireApiClient; 

		DataTableMapsMsx DataTableMaps => SVP?.DataTableMaps; // associated DataMap

		DataTableMapMsx CurrentMap => SVP?.DataTableMaps?.CurrentMap; // current DataTableMap

		TrellisCardVisualMsx V
		{
			get { return SVP.ActiveVisual as TrellisCardVisualMsx; }
			set { SVP.ActiveVisual = value; }
		}

		string OriginalChartState; // used to restore original state upon a cancel


		// Columns tab data

		QueryColumn IdColumnQc = null;
		QueryColumn FocusColumnQc = null;

		//CondFormatRules HorizontalGradientRules = null;
		//CondFormatRules VerticalSingleColorRules = null;

		bool InSetup = false;

		public TrellisCardPropertiesDialog()
		{
			InitializeComponent();

			//TableColumnList.ParameterNameCol.Visible = false;
			//BarChartColumnList.ParameterNameCol.Visible = false;
		}

		/// <summary>
		/// ShowDialog
		/// </summary>
		/// <param name="svm">Definition of the parameters of the Trellis Card view</param>
		/// <param name="initialTabName"></param>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			TrellisCardVisualMsx v,
			SpotfireViewProps svp)
		{
			Instance = new TrellisCardPropertiesDialog();

			return Instance.ShowDialog2(v, svp);
		}

		private DialogResult ShowDialog2(
			TrellisCardVisualMsx v,
			SpotfireViewProps svp)
		{
			SVP = svp;
			V = v;

			PropertyDialogsUtil.AdjustPropertyPageTabs(Tabs, TabPageSelector, TabsContainerPanel); ;

			OriginalChartState = v.Serialize();

			SetupForm(); 

			DialogResult dr = ShowDialog(Form.ActiveForm);
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

		private void TrellisCardPropertiesDialog_Shown(object sender, EventArgs e)
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
		/// Setup the form with the TrellisCard
		/// </summary>

		void SetupForm()
		{
			InSetup = true;

			ValidateViewInitialization();

			GeneralPropertiesPanel.Setup(V, EditValueChanged);

			//DataMapPanel.Setup(SVP, EditValueChanged);

			SetupColumnsTab();

			SetupTrellisTab();

			SetupSortingTab();

			PropertyDialogsUtil.SelectPropertyPage(CurrentTabName, TabPageSelector, Tabs);

			InSetup = false;
			return;
		}

		void ValidateViewInitialization()
		{
			if (SVP == null) throw new Exception("ViewManager not defined");

			//SVP.ValidateSpotfireViewPropsInitialization();

			if (V == null)
				V = new TrellisCardVisualMsx();

			return;
		}

		internal void SetupColumnsTab()
		{
			QueryColumn qc = null;
			ColumnMapMsx map = null;

			AssertMx.IsNotNull(SVP, "ViewManager");
			//AssertMx.IsNotNull(SVP.BaseQuery, "ViewQuery");

			//ColumnsTabSourceQueryControl.Setup(SVM.SVP.DataMap);

			// Id column

			//IdColumnQc = CurrentMap.ColumnMapCollection.GetQueryColumnForSpotfireColumnName(V.IdColumnName);
			//if (IdColumnQc == null)
			//{
			//	IdColumnQc = SVP.BaseQuery.GetFirstMatchingQueryColumn(MetaColumnType.CompoundId, mustBeSelected: true);
			//}
			//IdColumnSelector.Setup(SVP, IdColumnQc, EditValueChanged);

			// Focus Column

			//FocusColumnQc = CurrentMap.ColumnMapCollection.GetQueryColumnForSpotfireColumnName(V.FocusColumnName);
			//if (FocusColumnQc == null)
			//{
			//	FocusColumnQc = SVP.BaseQuery.GetFirstMatchingQueryColumn(MetaColumnType.Structure, mustBeSelected: true);
			//}
			//FocusColumnSelector.Setup(SVP, FocusColumnQc, EditValueChanged);

			// Horizontal & vertical chart setups

			SetupCondFormatRulesFromColorCodingItems(HorizontalGradientBarChartControl, V.SelectedColumns, CondFormatStyle.ColorScale);
			SetupCondFormatRulesFromColorCodingItems(VerticalBarChartControl, V.FeatureColumns, CondFormatStyle.ColorSet);

			return;
		}

		void SetupCondFormatRulesFromColorCodingItems(
			TrellisCardBarChartControl cc,
			List<ColorCodingItemMsx> ccl,
			CondFormatStyle cfStyle)
		{
			CondFormatRules rules = new CondFormatRules(cfStyle);

			foreach (ColorCodingItemMsx i in ccl)
			{
				CondFormatRule r = new CondFormatRule();
				r.Name = i.ColumnName;
				string suffix = "";

				if (cfStyle == CondFormatStyle.ColorSet)
				{
					r.BackColor1 = i.BackColor;
				}

				else // Gradient
				{
					int ruleCnt = i.SubRules.Count;

					List<string> subruleText = new List<string>();
					foreach (ColorCodingSubRuleMsx ccr in i.SubRules)
					{
						string txt = ccr.Value;
						if (ccr.CalcType != ValueCalcTypeEnumMsx.Value)
							txt += " (" + ccr.CalcType + ")";
						subruleText.Add(txt);
					}

					List<Color> colors = new List<Color>();
					if (ruleCnt > 0)
					{
						r.Value = subruleText[0];
						colors.Add(i.SubRules[0].Color);
					}

					if (ruleCnt > 2)
					{
						r.Value2= subruleText[2]; // max rule text
						colors.Add(i.SubRules[1].Color); // avg color
						colors.Add(i.SubRules[2].Color); // max color
					}

					else if (ruleCnt > 1)
					{
						r.Value2 = subruleText[1];
						colors.Add(i.SubRules[1].Color);
					}

					string scaleName = Bitmaps.GetColorScaleNameFromColorset(colors.ToArray());
					if (Lex.IsUndefined(scaleName)) scaleName = CondFormat.DefaultColorScale;
					r.ImageName = scaleName;
				}

				rules.Add(r);
			}

			cc.Setup(SVP, rules, EditValueChanged);
			return;
		}

		/// <summary>
		/// Id column assignment changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void IdColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			//IdColumnQc = IdColumnSelector.QueryColumn;
			EditValueChanged();
		}

		/// <summary>
		/// Focus column assignment changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void FocusColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
			//FocusColumnQc = FocusColumnSelector.QueryColumn;
			EditValueChanged();
		}

		void SetupTrellisTab()
		{
			NumberOfRowsCtl.Value = V.RowAmount;
			NumberOfColsCtl.Value = V.ColumnAmount;

			if (V.NavigationType == 0)
				PageUpDownNavigation.Checked = true;

			else ScrollNavigation.Checked = true;
		}

		private void NumberOfRowsCtl_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void NumberOfColsCtl_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void ScrollNavigation_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void PageUpDownNavigation_EditValueChanged(object sender, EventArgs e)
		{
			return; // handled by ScrollNavigation_EditValueChanged
		}

		void SetupSortingTab()
		{
			SortInfoCollectionMsx sic = GetSortInfoCollection();
			SortingPropertiesPanel.Setup(SVP, sic, EditValueChanged);
		}

		/// <summary>
		/// Convert individual view sort items to list
		/// </summary>
		/// <returns></returns>

		SortInfoCollectionMsx GetSortInfoCollection()
		{
			SortInfoCollectionMsx sic = new SortInfoCollectionMsx();
			AddSortInfo(sic, V.SortingByColumn, V.SortingByType);
			AddSortInfo(sic, V.ThenBy1Column, V.ThenBy1Type);
			AddSortInfo(sic, V.ThenBy2Column, V.ThenBy2Type);
			return sic;
		}

		void AddSortInfo(
			SortInfoCollectionMsx sic,
			string col,
			SortOrderMsx sortType)
		{
			if (!Lex.IsDefined(col)) return;
			SortInfoMsx si = new SortInfoMsx();
			si.DataColumnReferenceSerializedId = col;
			DataTableMsx dt = V.DataTable;
			if (dt == null) dt = Api?.Document?.ActiveDataTableReference;
			if (dt != null)
				si.DataColumnReference = dt.GetColumnByName(col);

			si.SortOrder = sortType;
			sic.SortList.Add(si);
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

			ApplyPendingChanges();
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
			string serializedText = V.Serialize(); // serialize
			Api.SetVisualProperties(V.Id, serializedText);

			return;
		}

		private void CancelBut_Click(object sender, EventArgs e)
		{
			CurrentTabName = Tabs.SelectedTabPage.Name;
			Visible = false;
			DialogResult = DialogResult.Cancel;
		}

		private void TrellisCardDialog_FormClosing(object sender, FormClosingEventArgs e)
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
			//if (e.Page == GeneralTabPage)
			//	RenderGeneralTabPage();

			//else if (e.Page == DataTabPage)
			//	RenderDataTabPage();

			//else if (e.Page == ColumnsTabPage)
			//	RenderColumnsTabPage();

			//else if (e.Page == TrellisTabPage)
			//	RenderTrellisColumnsTabPage();

			//else if (e.Page == SortingTabPage)
			//	RenderSortingTabPage();

			return;
		}


		/// <summary>
		/// Get visualization properties from form and store in TrellisCardVisualMx
		/// </summary>

		void GetFormValues()
		{
			TrellisCardVisualMsx v = V; // update existing visual instance

			// General

			GeneralPropertiesPanel.GetValues(v);
				
			// Data
			// --- todo ---

			// Columns

			//v.IdColumnName = CurrentMap.GetSpotfireNameFromQueryColumn(IdColumnSelector.QueryColumn);
			//v.FocusColumnName = CurrentMap.GetSpotfireNameFromQueryColumn(FocusColumnSelector.QueryColumn);

			v.SelectedColumns = GetColorCodingColumnsFormValues(HorizontalGradientBarChartControl);
			v.FeatureColumns = GetColorCodingColumnsFormValues(VerticalBarChartControl);

			// Trellis

			v.RowAmount = (int)NumberOfRowsCtl.Value;
			v.ColumnAmount = (int)NumberOfColsCtl.Value;

			if (PageUpDownNavigation.Checked)
				v.NavigationType = 0;

			else v.NavigationType = 1; // scrolling

			// Sorting

			GetSortingProperties();

			//string serializedText = MsxUtil.Serialize(v); // debug serialize 
			//TrellisCardVisualMsx tcvCopy = (TrellisCardVisualMsx)MsxUtil.Deserialize(serializedText, typeof(TrellisCardVisualMsx)); // deserialize

			return;
		}

		void GetSortingProperties()
		{
			SortInfoCollectionMsx sic = SortingPropertiesPanel.GetValues();
			V.SortingByColumn = V.ThenBy1Column = V.ThenBy2Column = "";

			for (int i1 = 0; i1 < sic.SortList.Count; i1++)
			{
				SortInfoMsx si = sic.SortList[i1];
				string col = si.DataColumnReference.Name;
				SortOrderMsx so = si.SortOrder;

				if (i1 == 0)
				{
					V.SortingByColumn = col;
					V.SortingByType = so;
				}

				else if (i1 == 1)
				{
					V.ThenBy1Column = col;
					V.ThenBy1Type = so;
				}

				else if (i1 == 2)
				{
					V.ThenBy2Column = col;
					V.ThenBy2Type = so;
				}
			}

			return;
		}

		/// <summary>
		/// GetDataDisplayColumnsFormValue
		/// </summary>
		/// <param name="rules"></param>
		/// <returns></returns>

		List<ColorCodingItemMsx> GetColorCodingColumnsFormValues(
				TrellisCardBarChartControl cc)
		{
			CondFormatRules rules = cc.GetCondFormatRulesFromDataTable();
			List<ColorCodingItemMsx> ccl = ConvertCondFormatRulesToColorCodingItems(rules);
			return ccl;
		}

		List<ColorCodingItemMsx> ConvertCondFormatRulesToColorCodingItems(
			CondFormatRules rules)
		{
			List<ColorCodingItemMsx> ccl = new List<ColorCodingItemMsx>();

			double v1, v2;

			foreach (CondFormatRule r in rules)
			{
				ColorCodingItemMsx i = new ColorCodingItemMsx();
				i.ColumnName = r.Name;

				if (rules.ColoringStyle == CondFormatStyle.ColorSet)
				{
					i.BackColor = r.BackColor1;
				}

				else // Gradient
				{
					//i.ImageName = r.ImageName;
					Color[] colors = Bitmaps.GetColorSetByName(Bitmaps.ColorScaleImageColors, r.ImageName);
					if (colors == null) Bitmaps.GetColorSetByName(Bitmaps.ColorScaleImageColors, CondFormat.DefaultColorScale);

					if (colors == null)
						colors = new Color[] { Color.LightGray, Color.DarkGray };

					int cl = colors.Length;

					if (cl > 0)
						i.SubRules.Add(BuildSubrule(r.Value, colors[0]));

					if (cl == 3)
					{
						i.SubRules.Add(BuildSubrule("Average", colors[1]));
						i.SubRules.Add(BuildSubrule(r.Value2, colors[2]));
					}

					else if (cl == 2)
						i.SubRules.Add(BuildSubrule(r.Value2, colors[1]));
				}

				ccl.Add(i);
			}

			return ccl;
		}

		ColorCodingSubRuleMsx BuildSubrule(
			string value,
			Color color)
		{
			ColorCodingSubRuleMsx sr = new ColorCodingSubRuleMsx();
			if (Lex.Contains(value, "min"))
			{
				sr.CalcType = ValueCalcTypeEnumMsx.Min;
			}

			else if (Lex.Contains(value, "avg"))
			{
				sr.CalcType = ValueCalcTypeEnumMsx.Average;
			}

			else if (Lex.Contains(value, "max"))
			{
				sr.CalcType = ValueCalcTypeEnumMsx.Max;
			}

			else
			{
				sr.CalcType = ValueCalcTypeEnumMsx.Value;
				sr.Value = value;
			}

			sr.Color = color;
			return sr;
		}

	}
}
