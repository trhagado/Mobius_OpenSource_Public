using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// SmallWorld structure search options control
	/// </summary>

	public partial class CriteriaStructureOptionsSW : UserControl
	{

		public CriteriaStructure CriteriaStructureForm = null; // Criteria structure dialog that contains us
		CriteriaStructure CSF { get { return CriteriaStructureForm; } }

		CriteriaStructurePreview PreviewCtl	{	get { return CSF != null ? CSF.PreviewCtl : null; }	}

		ParsedStructureCriteria Psc { get { return CSF != null ? CSF.Psc : null; } }

		SmallWorldPredefinedParameters Swp
		{
			get { return Psc.SmallWorldParameters; }
			set { Psc.SmallWorldParameters = value; }
		}

		bool InSetup = false;
		//bool ShowAdvanced = false;

		public static DictionaryMx SwDbDict = null; // Dict of available databases

		/// <summary>
		/// Preset options
		/// </summary>

		public static SmallWorldPredefinedParameters
			SmallWorld = ToSwp("preset=SmallWorld; dist=0-4; tup=0-4; tdn=0-4; rup=0-4; rdn=0-4; lup=0-4; ldn=0-4; maj=0-4; min=0-4; sub=0-4; hyb=0-4"),
			Substructure = ToSwp("preset=Substructure; dist=0-4; tup=0-10; tdn=0-0; rup=0-10; rdn=0-0; lup=0-10; ldn=0-0; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
			SuperStructure = ToSwp("preset=SuperStructure; dist=0-4; tup=0-0; tdn=10-0; rup=0-0; rdn=0-10; lup=0-0; ldn=0-10; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
			BemisMurckoFramework = ToSwp("preset=Bemis-Murcko Framework; dist=0-4; tup=0-10; tdn=0-10; rup=0-0; rdn=0-0; lup=0-0; ldn=0-0; maj=0-10; min=0-10; sub=0-4; hyb=0-4"),
			NqMCS = ToSwp("preset=Nq MCS; dist=0-4; tup=0-10; tdn=0-10; rup=0-10; rdn=0-10; lup=0-10; ldn=0-0; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
			ElementGraph = ToSwp("preset=Element Graph; dist=0-4; tup=0-0; tdn=0-0; rup=0-0; rdn=0-0; lup=0-0; ldn=0-0; maj=0-0; min=0-0; sub=0-4; hyb=0-4"),
			CustomSettings = ToSwp("preset=Custom; dist=0-4; tup=0-4; tdn=0-4; rup=0-4; rdn=0-4; lup=0-4; ldn=0-4; maj=0-4; min=0-4; sub=0-4; hyb=0-4"); // defaults to same values as "SmallWorld"

		static Dictionary<string, SmallWorldPredefinedParameters> OptionPresets = BuildDict(
			SmallWorld,
			Substructure,
			SuperStructure,
			BemisMurckoFramework,
			NqMCS,
			ElementGraph,
			CustomSettings);

		static Dictionary<string, SmallWorldPredefinedParameters> BuildDict(params SmallWorldPredefinedParameters[] presets)
		{
			Dictionary<string, SmallWorldPredefinedParameters> optionPresets = new Dictionary<string, SmallWorldPredefinedParameters>();
			foreach (SmallWorldPredefinedParameters swp in presets)
			{
				optionPresets.Add(swp.PresetName, swp);
			}

			return optionPresets;
		}

		/// <summary>
		/// Convert parameter string to SmallWorldParameters object
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		static SmallWorldPredefinedParameters ToSwp(string s)
		{
			SmallWorldPredefinedParameters swp = null;
			bool ok = SmallWorldPredefinedParameters.TryParse(s, out swp);
			return swp;
		}

		public CriteriaStructureOptionsSW()
		{
			InitializeComponent();

			MutationRangeMinor.ToolTipMx = // multiline tooltip
				"• Minor Mutation: When elements are in the same periodic group (e.g.F, Cl, Br, I).\r\n\r\n• Major Mutation -When elements are not in the same periodic group.";

			return;
		}

		/// <summary>
		/// Setup the options from psc
		/// </summary>
		/// <param name="psc"></param>

		public void Setup(
			SmallWorldPredefinedParameters swpArg)
		{
			try
			{
				InSetup = true;
				bool t = true, f = false;

				Swp = swpArg; // save reference to parameters

				if (SwDbDict == null)
				{
					SwDbDict = DictionaryMx.Get("SmallWorldDatabases");
					if (SwDbDict != null)
					{
						UIMisc.SetListControlItemsFromDictionary(DatabaseComboBox.Properties.Items, SwDbDict.Name, true);
					}

					GetDefaultSmallWorldOptions(); // also get any default options for user
				}

				if (Swp == null) // get initial option values
				{
					SmallWorldPredefinedParameters swp = FindStandardPresetMatchingCustomSettings();

					if (swp == null) // if no match then use my preferred
						swp = CustomSettings;

					if (swp.MaxHits <= 0) // be sure maxhits is defined
						swp.MaxHits = SmallWorldPredefinedParameters.DefaultMaxHits;

					Swp = swp.Clone(); // make copy
				}

				List<string> dbList = GetDbSetList();

				string dbs = "";
				foreach (string dbName in dbList)
				{
					if (SwDbDict.LookupDefinition(dbName) == null) continue;
					if (dbs != "") dbs += ", ";
					dbs += dbName;
				}

				Swp.Database = dbs;

				if (Lex.IsUndefined(Swp.Database)) // default to first entry in dict
					Swp.Database = SwDbDict.Words[0]; // assign first database as default if not defined

				DatabaseComboBox.Text = Swp.Database;

				PresetsComboBox.Text = Swp.PresetName;

				CriteriaStructureRangeCtl.SetRange(DistanceRange, "", DistanceRangeLabel, null, Swp.Distance);
				if (Swp.MaxHits < 0)
					MaxHits.Text = "";
				else MaxHits.Text = Swp.MaxHits.ToString();

				// Set the Defined and Enabled attributes for each range based on search type

				Swp.TerminalUp.Enabled = Swp.TerminalDown.Enabled = true;
				Swp.RingUp.Enabled = Swp.RingDown.Enabled = true;
				Swp.LinkerUp.Enabled = Swp.LinkerDown.Enabled = true;
				Swp.MutationMinor.Enabled = Swp.MutationMajor.Enabled = true;
				Swp.SubstitutionRange.Enabled = Swp.HybridisationChange.Enabled  = true;

				if (Lex.Eq(Swp.PresetName, SmallWorld.PresetName) ||
					Lex.Eq(Swp.PresetName, CustomSettings.PresetName)) ;

				else if (Lex.Eq(Swp.PresetName, Substructure.PresetName))
				{
					Swp.TerminalDown.Enabled = false;
					Swp.RingDown.Enabled = false;
					Swp.LinkerDown.Enabled = false;

					Swp.MutationMinor.Enabled = Swp.MutationMajor.Enabled = false;
				}

				else if (Lex.Eq(Swp.PresetName, SuperStructure.PresetName))
				{
					Swp.TerminalUp.Enabled = false;
					Swp.RingUp.Enabled = false;
					Swp.LinkerUp.Enabled = false;

					Swp.MutationMinor.Enabled = Swp.MutationMajor.Enabled = false;
				}

				else if (Lex.Eq(Swp.PresetName, BemisMurckoFramework.PresetName))
				{
					Swp.RingUp.Enabled = Swp.RingDown.Enabled = false;
					Swp.LinkerUp.Enabled = Swp.LinkerDown.Enabled = false;
				}

				else if (Lex.Eq(Swp.PresetName, NqMCS.PresetName))
				{
					Swp.LinkerUp.Enabled = Swp.LinkerDown.Enabled = false;
					Swp.MutationMinor.Enabled = Swp.MutationMajor.Enabled = false;
				}

				else if (Lex.Eq(Swp.PresetName, ElementGraph.PresetName))
				{
					Swp.TerminalUp.Enabled = Swp.TerminalDown.Enabled = false;
					Swp.RingUp.Enabled = Swp.RingDown.Enabled = false;
					Swp.LinkerUp.Enabled = Swp.LinkerDown.Enabled = false;

					Swp.MutationMinor.Enabled = Swp.MutationMajor.Enabled = false;
				}

				MatchAtomTypes.Checked = Swp.MatchAtomTypes;
				bool e = Swp.MatchAtomTypes;
				Swp.LinkerUp.Active = Swp.LinkerDown.Active = !e;
				Swp.MutationMinor.Active = Swp.MutationMajor.Active = e;
				Swp.SubstitutionRange.Active = Swp.HybridisationChange.Active = e;

				// Set the range controls

				TerminalRangeUp.Set(Swp.TerminalUp);
				TerminalRangeDown.Set(Swp.TerminalDown);
				RingRangeUp.Set(Swp.RingUp);
				RingRangeDown.Set(Swp.RingDown);
				LinkerRangeUp.Set(Swp.LinkerUp);
				LinkerRangeDown.Set(Swp.LinkerDown);

				MutationRangeMinor.Set(Swp.MutationMinor);
				MutationRangeMajor.Set(Swp.MutationMajor);
				SubstitutionRange.Set(Swp.SubstitutionRange);
				HybridizationRange.Set(Swp.HybridisationChange);

				ShowColors.Checked = Swp.Highlight;
				SetControlBackgroundColors(ShowColors.Checked);

				AlignStructs.Checked = Swp.Align;
			}

			finally { InSetup = false; }

			return;
		}

		/// <summary>
		/// Get the list of databases from any DbSet column in the underlying source table
		/// </summary>
		/// <returns></returns>

		List<string> GetDbSetList()
		{
			List<string> defaultDbList = new List<string>();
			defaultDbList.Add(SmallWorldPredefinedParameters.DefaultDatabase); // initial database

			QueryColumn qc = Psc.QueryColumn;

			if (qc == null) return defaultDbList;

			MetaTable mt = qc.MetaColumn.MetaTable;

			MetaColumn dbSetMc = mt.DatabaseListMetaColumn;
			if (dbSetMc == null) return defaultDbList;

			QueryColumn dbSetQc = qc.QueryTable.GetQueryColumnByName(dbSetMc.Name);
			if (!Lex.IsDefined(dbSetQc.Criteria)) return defaultDbList;

			ParsedSingleCriteria sc = MqlUtil.ParseQueryColumnCriteria(dbSetQc);
			if (sc != null)
				return sc.ValueList;

			else // some problem with criteria
				return defaultDbList; 
		}


		void SetControlBackgroundColors(bool showColors)
		{
			Identical.Visible = showColors;

			if (showColors)
			{
				Identical.BackColor = Color.FromArgb(44, 190, 132);
				TerminalRangeUp.BackColor = Color.FromArgb(255, 190, 196);
				MutationRangeMinor.BackColor = Color.FromArgb(255, 242, 119);
				MutationRangeMajor.BackColor = Color.FromArgb(255, 182, 126);
				SubstitutionRange.BackColor = Color.FromArgb(173, 255, 171);
				HybridizationRange.BackColor = Color.FromArgb(173, 255, 171);
			}

			else
			{
				Identical.BackColor =
				TerminalRangeUp.BackColor =
				MutationRangeMinor.BackColor =
				MutationRangeMajor.BackColor =
				SubstitutionRange.BackColor =
				HybridizationRange.BackColor = Color.Transparent;
			}

			return;
		}


		/// <summary>
		/// Get option values & store in ParsedStructureCriteria
		/// </summary>
		/// <returns></returns>

		public bool GetValues(ParsedStructureCriteria psc)
		{
			SmallWorldPredefinedParameters swp;

			if (!GetValues(out swp)) return false;
			psc.SmallWorldParameters = swp;

			return true;
		}

/// <summary>
/// Try to get option values checking for errors
/// </summary>
/// <returns></returns>

		bool GetValues(
			out SmallWorldPredefinedParameters swp)
		{
			int maxHits = 0;

			swp = new SmallWorldPredefinedParameters();

			if (Lex.IsUndefined(DatabaseComboBox.Text))
			{
				MessageBoxMx.ShowError("Database to search must be defined");
				return false;
			}
			swp.Database = DatabaseComboBox.Text; // one or more database names

			swp.Distance = new RangeParm(DistanceRange.Value.Minimum, DistanceRange.Value.Maximum);

			if (!TryParseMaxHits(out maxHits)) return false;
			swp.MaxHits = maxHits;

			swp.TerminalUp = TerminalRangeUp.GetRange();
			swp.TerminalDown = TerminalRangeDown.GetRange();

			swp.RingUp = RingRangeUp.GetRange();
			swp.RingDown = RingRangeDown.GetRange();

			swp.LinkerUp = LinkerRangeUp.GetRange();
			swp.LinkerDown = LinkerRangeDown.GetRange();

			swp.MatchAtomTypes = MatchAtomTypes.Checked;

			swp.MutationMinor = MutationRangeMinor.GetRange();
			swp.MutationMajor = MutationRangeMajor.GetRange();

			swp.SubstitutionRange = SubstitutionRange.GetRange();
			swp.HybridisationChange = HybridizationRange.GetRange();

			swp.PresetName = PresetsComboBox.Text;

			swp.Highlight = ShowColors.Checked;
			swp.Align = AlignStructs.Checked;

			return true;
		}

		bool TryParseMaxHits(out int maxHits)
		{
			if (Lex.IsUndefined(MaxHits.Text)) maxHits = -1; // no limit

			else if (!int.TryParse(MaxHits.Text, out maxHits) || maxHits == 0)
			{
				MessageBoxMx.ShowError("Invalid Maximum Hits value");
				return false;
			}

			return true;
		}

		private void DistanceRange_EditValueChanged(object sender, EventArgs e)
		{
			CriteriaStructureRangeCtl.SetRangeLabel(DistanceRange, "", DistanceRangeLabel);
			EditValueChanged();
		}

		private void MaxHits_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void DatabaseComboBox_Click(object sender, EventArgs e)
		{
			string response = ToolHelper.GetCheckedListBoxDialog("SmallWorldDatabases", DatabaseComboBox.Text, "SmallWorld databases");
			if (response != null)
			{
				DatabaseComboBox.Text = response;
				EditValueChanged();
			}
		}

		private void AtomTypeMatch_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			SmallWorldPredefinedParameters swp = null;
			if (!GetValues(out swp)) return;
			Swp = swp;
			Setup(Swp); // adjust other form elements

			EditValueChanged();
		}

		private void ShowColors_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			SmallWorldPredefinedParameters swp = null;
			if (!GetValues(out swp)) return;
			Swp = swp;
			UpdateDepictions();
		}

		private void AlignStructs_CheckedChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			SmallWorldPredefinedParameters swp = null;
			if (!GetValues(out swp)) return;
			Swp = swp;

			UpdateDepictions();
		}

		void UpdateDepictions()
		{
			try
			{
				SetControlBackgroundColors(ShowColors.Checked);
				CriteriaStructureForm.PreviewCtl.UpdateDepictions(Psc);
			}
			catch (Exception ex) { ex = ex; }
		}

		private void TerminalRangeUp_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void TerminalRangeDown_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void RingRangeUp_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void RingRangeDown_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void LinkerRangeUp_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void LinkerRangeDown_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void MutationRangeMinor_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void MutationRangeMajor_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void SubstitutionRange_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void HybridizationRange_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		void EditValueChanged()
		{
			if (InSetup) return;

			GetValues(Psc); // get current values

			if (PreviewCtl == null) return;

			StartQueryExecution();
		}

/// <summary>
/// StartInitialQueryExecution
/// </summary>

		public void StartInitialQueryExecution()
		{
			CriteriaStructureForm.PreviewCtl.ClearDataAndGrid(); // clear any previous data
			StartQueryExecution();
			return;
		}

		/// <summary>
		/// Show tooltip if clicked on rather than just hovered over
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void HelpButton_Click(object sender, EventArgs e)
		{
			// Instead of the following to show the tooltip set the ToolTipController
			// InitialDelay and ReshowDelay to 1 ms.
			// Also, set the AutoPopupDelay to 20000 ms to keep it up for a while.
			//try
			//{
			//ToolTipController.InitialDelay = 100; // make tooltip show quickly
			//	ToolTipController.HideHint();
			//	ToolTipControllerShowEventArgs args = new ToolTipControllerShowEventArgs();
			//	args.SuperTip = HelpButton.SuperTip;
			//	args.ToolTipType = DevExpress.Utils.ToolTipType.SuperTip;
			//	ToolTipController.ShowHint(args);
			//	return;
			//}

			//catch (Exception ex) { ex = ex; }

		}

		/// <summary>
		/// Start query to retrieve structures that match current criteria
		/// </summary>

		public void StartQueryExecution()
		{
			Psc.SearchType = StructureSearchType.SmallWorld; // be sure Psc is setup for SmallWorld search
			Psc.Molecule = new MoleculeMx(MoleculeFormat.Molfile, CriteriaStructureForm.QueryMolCtl.MolfileString); // and includes current structure

			CriteriaStructureForm.PreviewCtl.StartQueryExecution(Psc);
			return;
		}

		/// <summary>
		/// User picked new preset
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void PresetsComboBox_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			string txt = PresetsComboBox.Text;
			if (!OptionPresets.ContainsKey(txt)) return; // shouldn't happen

			SmallWorldPredefinedParameters opts = OptionPresets[txt];
			if (opts == null) return; // shouldn't happen

			Setup(opts);
			Refresh();
			EditValueChanged();
		}

		SmallWorldPredefinedParameters GetDefaultSmallWorldOptions()
		{
			SmallWorldPredefinedParameters swp = SmallWorld;

			string prefs = Preferences.Get("DefaultSmallWorldOptions");
			if (Lex.IsUndefined(prefs)) return CustomSettings;

			if (!SmallWorldPredefinedParameters.TryParse(prefs, out swp))
				return CustomSettings;

			CustomSettings = swp;
			OptionPresets[CustomSettings.PresetName] = swp; // store in dict
			return swp;
		}

		private void SavePreferredSettings_Click(object sender, EventArgs e)
		{
			SavePreferredSettings();
			return;
		}

		private void SavePreferredSettings()
		{
			SmallWorldPredefinedParameters swp = new SmallWorldPredefinedParameters();
			if (!GetValues(out swp)) return;

			swp.PresetName = CustomSettings.PresetName;
			string prefs = swp.Serialize();
			Preferences.Set("DefaultSmallWorldOptions", prefs);

			OptionPresets[CustomSettings.PresetName] = swp;
			CustomSettings = swp;

			if (FindStandardPresetMatchingCustomSettings() == null)
				PresetsComboBox.Text = "Custom";
		}

		SmallWorldPredefinedParameters FindStandardPresetMatchingCustomSettings()
		{
			foreach (SmallWorldPredefinedParameters swp0 in OptionPresets.Values) // look for match
			{
				if (swp0 == CustomSettings) continue;

				if (CustomSettings.Equals(swp0)) return swp0;
			}

			return null;
		}

	}
}
