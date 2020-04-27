using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents 
{
	public partial class TargetSummaryOptionsControl : DevExpress.XtraEditors.XtraUserControl
	{
		TargetSummaryOptions Tso; // parm values associated with control
		Dictionary<string, string> TargetMapNamesAndLabels; // list of maps by name & label
		List<string> MapLabelList; // list of map labels for drop down control
		Dictionary<string, object> MapLabelDict; // label list keyed by label
		List<string> UserMapNames = new List<string>(); // maps specific to this user
		string SelectedTargetMap = null; // label for selected target map

		bool Initialized = false;
		bool InSetup = false;

/// <summary>
/// Basic constructor
/// </summary>

		public TargetSummaryOptionsControl()
		{
			InitializeComponent();
		}

/// <summary>
/// Allocate & initialize a TargetResultsViewerParms instance
/// </summary>
/// <returns></returns>

		public static TargetSummaryOptions GetPreferences()
			{
				TargetSummaryOptions tso;
				string tsoString = Preferences.Get("MultiDbViewerPrefs");
				if (Lex.Contains(tsoString, "MultiDbViewerPrefs"))
					tso = TargetSummaryOptions.Deserialize(tsoString);
				else tso = new TargetSummaryOptions();

				return tso;
			}


/// <summary>
/// Save preferences
/// </summary>
/// <param name="tso"></param>

		public static void SavePreferences(TargetSummaryOptions tso)
		{
			string tsoString = tso.Serialize();
			Preferences.Set("MultiDbViewerPrefs", tsoString);
		}

/// <summary>
/// Setup the form values
/// </summary>
/// <param name="tso"></param>

		public void SetFormValues(
			TargetSummaryOptions tso,
			bool allowDbChange)
		{
			InSetup = true;

			Tso = tso; // TargetSummaryOptions instance associated with this control

			if (!Initialized) InitializeTargetMapControl(tso);

			Cids.Text = tso.CidCriteria; // set initial cid criteria
			Targets.Text = tso.TargetList;
			GeneFamilies.Text = tso.GeneFamilies;

			if (Lex.Eq(tso.DbName, "CorpDb")) CorpDb.Checked = true;
			else CorpDbAndChemblDb.Checked = true;
			DatabaseGroupBox.Enabled = allowDbChange; // allow database changes only if no query so far

			TableView.Checked = true; // default view
			if (Lex.Eq(tso.PreferredView, "Table")) TableView.Checked = true;
			else if (Lex.Eq(tso.PreferredView, "Map")) MapView.Checked = true;
			else if (Lex.Eq(tso.PreferredView, "Network")) NetworkView.Checked = true;
			else if (Lex.Eq(tso.PreferredView, "Heatmap")) HeatmapView.Checked = true;
			else if (Lex.Eq(tso.PreferredView, "WebPlayer")) WebPlayerView.Checked = true;

			IncludeStructures.Checked = tso.IncludeStructures;

			TargetsWithActivesOnly.Checked = tso.TargetsWithActivesOnly;
			AssayTypes = tso.AssayTypesToInclude;
			MaxCRC.Text = tso.CrcUpperBound;
			MinSP.Text = tso.SpLowerBound;
			if (tso.UseMeans) MeanSummarization.Checked = true;
			else MinMaxSummarization.Checked = true;

			FilterableTargets.Text = tso.FilterableTargets;

			InSetup = false;

			return;
		}

/// <summary>
/// Initialize form with map info
/// </summary>
/// <param name="tso"></param>

		void InitializeTargetMapControl(TargetSummaryOptions tso)
		{
			MapLabelList = new List<string>();
			MapLabelDict = new Dictionary<string, object>();
			UserMapNames = new List<string>(); // pathways specific to this user

			TargetMapNamesAndLabels = TargetMapDao.GetTargetNamesAndLabels(); // get full list of maps
			List<string> commonMapNames = TargetMapDao.GetCommonMapNames(); // get common maps
			foreach (string mapName in commonMapNames)
			{
				if (!TargetMapNamesAndLabels.ContainsKey(mapName)) continue;
				string mapLabel = TargetMapNamesAndLabels[mapName];
				MapLabelList.Add(mapLabel);
				MapLabelDict[mapLabel.ToUpper()] = null;
			}

			if (tso.UserMapNames.Length > 0) // get user-specific pathways
			{
				List<string> sa = Csv.SplitCsvString(tso.UserMapNames);
				foreach (string mapName0 in sa)
				{
					if (!TargetMapNamesAndLabels.ContainsKey(mapName0)) continue;
					string mapLabel = TargetMapNamesAndLabels[mapName0];
					if (MapLabelDict.ContainsKey(mapLabel.ToUpper())) continue; // don't add if already have

					MapLabelList.Add(mapLabel);
					MapLabelDict[mapLabel.ToUpper()] = null;
					UserMapNames.Add(mapName0);
				}
			}

			Lex.SortList(MapLabelList);

			MapLabelList.Add("None");
			MapLabelList.Add("---------------");
			MapLabelList.Add("Retrieve KEGG Pathway..."); // allow addition of new pathway
			TargetMap.Properties.Items.Clear();
			TargetMap.Properties.Items.AddRange(MapLabelList);

			if (Lex.IsNullOrEmpty(tso.TargetMapName)) tso.TargetMapName = "General Multi-platform Dendogram";
			SelectedTargetMap = tso.TargetMapName;
			if (String.IsNullOrEmpty(SelectedTargetMap)) SelectedTargetMap = MapLabelList[0];
			if (TargetMapNamesAndLabels.ContainsKey(SelectedTargetMap))
				SelectedTargetMap = TargetMapNamesAndLabels[SelectedTargetMap];
			TargetMap.Text = SelectedTargetMap;

			Initialized = true;
		}

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
		private void Cids_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;

			string c = Char.ConvertFromUtf32(e.KeyValue);

			if (Cids.Text == "" && Char.IsLetterOrDigit(c,0)) // handle first char
			{
				Cids.Text = "= " + c;
				if (!EditCids()) Cids.Text = "";
			}

			else EditCids(); // edit existing value
		}

/// <summary>
/// Editing compound id list
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Cids_KeyPress(object sender, KeyPressEventArgs e)
		{
			//e.Handled = true;

			//if (Cids.Text == "") // handle 1st char
			//{
			//  Cids.Text = "= " + e.KeyChar;
			//  if (!EditCids()) Cids.Text = "";
			//}

			//else EditCids(); // edit existing value
		}

		private void Cids_MouseDown(object sender, MouseEventArgs e)
		{
			EditCids();
		}

		private void SelectCids_Click(object sender, EventArgs e)
		{
			EditCids();
		}

/// <summary>
/// Edit the Cids criteris
/// </summary>
/// <returns></returns>

		bool EditCids()
		{
			Query q;
			QueryTable qt;
			MetaTable mt;
			QueryColumn qc;

			mt = MetaTableCollection.GetWithException(Tso.GetSummarizedMetaTableName());

			q = new Query();
			qt = new QueryTable(q, mt);
			qc = qt.KeyQueryColumn;

			qc.Criteria = Cids.Text;
			qc.CriteriaDisplay = Cids.Text;
			if (!CriteriaEditor.GetCompoundIdCriteria(qc)) return false;
			if (qc.Criteria.StartsWith(" = ")) // make equality look nice
				Cids.Text = qc.CriteriaDisplay;
			else Cids.Text = qc.Criteria;

			return true;
		}

/// <summary>
/// Show target editing dialog
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void EditTargets_Click(object sender, EventArgs e)
		{
			string listText = Targets.Text;
			List<string> list = Csv.SplitCsvString(listText); // only comma delimiters allowed here since some values (e.g. names) may contain spaces
			StringBuilder sb = new StringBuilder();
			foreach (string s in list)
			{
				sb.Append(s);
				sb.Append("\r\n");
			}

			string title = "Target Symbol List";
			listText = CriteriaList.Edit(sb.ToString(), title);
			if (listText != null)
				Targets.Text = listText;
			return;
		}

		private void EditFilterableTargets_Click(object sender, EventArgs e)
		{
			string listText = FilterableTargets.Text;
			List<string> list = Csv.SplitCsvString(listText); // only comma delimiters allowed here since some values (e.g. names) may contain spaces
			StringBuilder sb = new StringBuilder();
			foreach (string s in list)
			{
				sb.Append(s);
				sb.Append("\r\n");
			}

			string title = "Target Symbol List";
			listText = CriteriaList.Edit(sb.ToString(), title);
			if (listText != null)
				FilterableTargets.Text = listText;
			return;
		}

/// <summary>
/// GetFormValues
/// </summary>
/// <param name="tso"></param>
/// <returns></returns>

		public bool GetFormValues(
			TargetSummaryOptions tso)
		{
			tso.CidCriteria = Cids.Text;
			tso.TargetList = Targets.Text;
			tso.GeneFamilies = GeneFamilies.Text;

			if (CorpDb.Checked) tso.DbName = "CorpDb";
			else tso.DbName = "CorpDbChEMBL";

			if (TableView.Checked)
				tso.PreferredView = "Table";

			else if (MapView.Checked)
				tso.PreferredView = "Map";

			else if (NetworkView.Checked)
				tso.PreferredView = "Network";

			else if (HeatmapView.Checked)
				tso.PreferredView = "Heatmap";

			else if (WebPlayerView.Checked)
				tso.PreferredView = "WebPlayer";

			else throw new Exception("Unrecognized output format");

			tso.IncludeStructures = IncludeStructures.Checked;
			tso.TargetsWithActivesOnly = TargetsWithActivesOnly.Checked;
			tso.CrcUpperBound = MaxCRC.Text;
			tso.SpLowerBound = MinSP.Text;
			tso.UseMeans = MeanSummarization.Checked;
			tso.AssayTypesToInclude = AssayTypes;

			tso.FilterableTargets = FilterableTargets.Text;

			string mapLabel = TargetMap.Text;
			tso.TargetMapName = "";

			if (mapLabel != "")
			{
				foreach (string name0 in TargetMapNamesAndLabels.Keys)
				{
					if (Lex.Eq(TargetMapNamesAndLabels[name0], mapLabel))
					{
						tso.TargetMapName = name0;
						break;
					}
				}
			}

			tso.UserMapNames = Csv.JoinCsvString(UserMapNames);

			if (TargetsWithActivesOnly.Checked) // (not currently used)
			{
				double d;
				if (!double.TryParse(MaxCRC.Text, out d))
				{
					MessageBoxMx.Show("You must supply a valid upper CRC bound value.", UmlautMobius.String);
					MaxCRC.Focus();
					return false;
				}

				if (!double.TryParse(MinSP.Text, out d))
				{
					MessageBoxMx.Show("You must supply a valid lower SP bound value.", UmlautMobius.String);
					MinSP.Focus();
					return false;
				}
			}

			return true;
		}

/// <summary>
/// Add gene family name if assoc ctl is checked
/// </summary>
/// <param name="ctl"></param>
/// <param name="name"></param>
/// <param name="list"></param>
/// <returns></returns>

		bool AddIfChecked(
			CheckEdit ctl,
			string name,
			ref string list)
		{
			if (!ctl.Checked) return false;

			if (list != "") list += ", ";
			list += name;
			return true;
		}

/// <summary>
/// Set check for gene family
/// </summary>
/// <param name="ctl"></param>
/// <param name="name"></param>
/// <param name="list"></param>

		void SetCheck(
			CheckEdit ctl,
			string name,
			ref string list)
		{
			if (Lex.Eq(name, "All") || Lex.IsNullOrEmpty(name))
				ctl.Checked = true;

			else ctl.Checked = Lex.Contains(list, name);

			return;
		}


		/// <summary>
		/// Serialize / deserialize assay types controls on form
		/// </summary>

		public string AssayTypes
		{
			get
			{
				string assayTypes = "";
				bool allAssayTypes = true;
				foreach (CheckEdit cb in AssayTypesGroup.Controls)
				{
					if (cb.Checked)
					{
						if (assayTypes != "") assayTypes += ",";
						string s2 = cb.Text;
						s2 = Lex.AddSingleQuotes(s2);
						assayTypes += s2;
					}

					else allAssayTypes = false;
				}

				if (allAssayTypes) return "All";
				else return assayTypes;
			}

			set
			{
				string[] sa = value.Split(',');

				foreach (CheckEdit cb in AssayTypesGroup.Controls)
				{
					bool check = false;
					foreach (string s in sa)
					{
						string s2 = s.Trim();
						s2 = Lex.RemoveAllQuotes(s2);
						if (Lex.Eq(cb.Text, s2) || Lex.IsNullOrEmpty(value) || Lex.Eq(value, "All"))
						{
							check = true;
							break;
						}
					}

					cb.Checked = check;
				}
			}
		}

		private void TargetsWithActivesOnly_CheckedChanged(object sender, EventArgs e)
		{
			if (TargetsWithActivesOnly.Checked)
			{
				MaxCRC.Enabled = true;
				MinSP.Enabled = true;
			}
			else
			{
				MaxCRC.Enabled = false;
				MinSP.Enabled = false;
			}
		}

		/// <summary>
		/// Selected a new target map
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void TargetMap_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (InSetup) return;
			string newTargetMap = TargetMap.Text;
			if (Lex.Eq(newTargetMap, "Retrieve KEGG Pathway...")) // lookup KEGG pathway
			{

				if (Math.Abs(1) == 1) // 2/1/2013, best available now e.g. http://www.kegg.jp/kegg-bin/show_pathway?hsa04010
				{
					MessageBoxMx.ShowError(@"Automated access to pathway maps is now available through subsciption only.");
					return;
				}

				SetTargetMapText(SelectedTargetMap); // keep current map for now

				string pathwayId = InputBoxMx.Show(
					"Enter the KEGG pathway id (e.g. hsa00010) for the desired pathway. " +
					"Pathway identifiers can be looked up at: " +
					"<a href=\"http://www.genome.jp/kegg/pathway.html\" target=\"_blank\">http://www.genome.jp/kegg/pathway.html</a>",
					"KEGG Pathway Identifier");

				if (String.IsNullOrEmpty(pathwayId)) return;

				try
				{
					Progress.Show("Retrieving KEGG information for pathway: " + pathwayId, UmlautMobius.String, false);
					TargetMap tm = TargetMapDao.GetKeggPathway(pathwayId);
					Progress.Hide();
					if (!MapLabelDict.ContainsKey(tm.Label.ToUpper()))
					{
						MapLabelDict[tm.Label.ToUpper()] = null;
						MapLabelList.Add(tm.Label);
						UserMapNames.Add(tm.Name);
					}

					TargetMap.Properties.Items.Clear();
					TargetMap.Properties.Items.AddRange(MapLabelList); // update list
					SelectedTargetMap = tm.Label;
					SetTargetMapText(SelectedTargetMap); // make current selection
				}

				catch (Exception ex)
				{
					string msg = ex.Message;
					Progress.Hide();
					MessageBoxMx.ShowError("Pathway not found");
					return;
				}

				return;
			}

			else if (newTargetMap.StartsWith("---")) // divider selected, ignore
				SetTargetMapText(SelectedTargetMap);

			SetTargetMapText(newTargetMap); // new map selected
		}

		void SetTargetMapText(string text)
		{
			InSetup = true;
			TargetMap.Text = text;
			InSetup = false;
		}

		private void CorpDb_CheckedChanged(object sender, EventArgs e)
		{
			if (!CorpDb.Checked) return;
			CidLabel.Text = MetaTable.PrimaryKeyColumnLabel + ":";
		}

		private void CorpDbAndChemblDb_CheckedChanged(object sender, EventArgs e)
		{
			if (!CorpDbAndChemblDb.Checked) return;
			CidLabel.Text = "Compound Ids:";
		}

/// <summary>
/// Edit gene families
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void EditGeneFamilies_Click(object sender, EventArgs e)
		{
			string title = "Allowed Gene Families";
			string prompt = "Select one or more Gene Families from the list below.";
			string dictName = "TA_Gene_Family";
			string selections = GeneFamilies.Text;

			selections = Csv.JoinCsvString(Csv.SplitCsvString(selections)); // quote any items with spaces in them

			string criteria = "GeneFamilies in (" + selections + ")"; // put in criteria form for dialog box

			selections = CriteriaDictMultSelect.CheckedListBoxDialog(title, prompt, dictName, criteria);

			if (selections != null)
				GeneFamilies.Text = selections;
			return;
		}

	}
}
