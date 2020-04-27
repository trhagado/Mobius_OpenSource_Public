using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaStructureOptionsSim : UserControl
	{

		public CriteriaStructureOptionsSim()
		{
			InitializeComponent();
		}

		public void Setup(ParsedStructureCriteria psc)
		{
			InitSimOptionsIfUndefined(psc);

			if (psc.SimilarityType == SimilaritySearchType.Normal) MolsimNormalCtl.Checked = true; 
			else if (psc.SimilarityType == SimilaritySearchType.Sub) MolsimSubsimCtl.Checked = true;
			else if (psc.SimilarityType == SimilaritySearchType.Super) MolsimSuperCtl.Checked = true;
			else if (psc.SimilarityType == SimilaritySearchType.ECFP4) ECFP4Ctl.Checked = true;

			if (psc.MinimumSimilarity <= 0 || psc.MinimumSimilarity > 1)
				psc.MinimumSimilarity = MoleculeMx.DefaultMinSimScore;
			MinSim.Text = psc.MinimumSimilarity.ToString();

			if (psc.MaxSimHits <= 0)
				psc.MaxSimHits = MoleculeMx.DefaultMaxSimHits;
			MaxHits.Text = psc.MaxSimHits.ToString();

			ECFP4Ctl.Enabled = EcfpSimilaritySearchIsSupportedForColumn(psc);
		}


		/// <summary>
		/// EcfpSimilaritySearchIsSupportedForColumn
		/// </summary>
		/// <param name="psc"></param>
		/// <returns></returns>

		bool EcfpSimilaritySearchIsSupportedForColumn(
			ParsedStructureCriteria psc)
		{
			QueryColumn qc = psc.QueryColumn;
			if (qc == null || qc.MetaColumn == null || qc.QueryTable == null || qc.QueryTable.MetaTable == null)
				return false; // assume not available if no info

			MetaColumn mc = qc.MetaColumn;
			MetaTable mt = mc.MetaTable;

			if (mc.DataType != MetaColumnType.Structure || !mc.IsSearchable) return false;

			if (mc.IsMetaTableEcfpSimilaritySearchable()) return true;

			else return false;
		}

		/// <summary>
		/// If sim options are undefined then initialize with default values
		/// </summary>
		/// <param name="psc"></param>

		public static void InitSimOptionsIfUndefined(ParsedStructureCriteria psc)
		{
			if (psc.SimilarityType == SimilaritySearchType.Unknown) // get any default if unknown
			{
				string simType, minSimTok, maxHitsTok;
				ToolHelper.GetDefaultSimMethod(out simType, out minSimTok, out maxHitsTok);

				psc.SimilarityType = SimilaritySearchType.Normal;
				try { psc.SimilarityType = (SimilaritySearchType)Enum.Parse(typeof(SimilaritySearchType), simType, true); }
				catch { }

				double.TryParse(minSimTok, out psc.MinimumSimilarity);
				int.TryParse(maxHitsTok, out psc.MaxSimHits);
			}

			if (psc.MinimumSimilarity > 10) psc.MinimumSimilarity = psc.MinimumSimilarity / 10; // convert 1-100 range to 0.0 - 1.0

			if (psc.MinimumSimilarity <= 0 || psc.MinimumSimilarity > 1)
				psc.MinimumSimilarity = .75;
		}

		private void SaveSimDefault_Click(object sender, EventArgs e)
		{
			string parm = GetSelectedSimSearchTypeName() + " " +
				MinSim.Text + " " + MaxHits.Text;
			Preferences.Set("DefaultSimMethod", parm);
		}

		public bool GetValues(ParsedStructureCriteria psc)
		{
			double minSim = 0;
			double.TryParse(MinSim.Text, out minSim);
			if (minSim < 0 || minSim > 1.0)
			{
				MessageBoxMx.ShowError("Upper similarity must be between 0.0 and 1.0");
				return false;
			}

			psc.MinimumSimilarity = minSim;

			int maxHits = -1;
			int.TryParse(MaxHits.Text, out maxHits);
			psc.MaxSimHits = maxHits;

			psc.SimilarityType = GetSelectedSimSearchType();

			return true;
		}

		SimilaritySearchType GetSelectedSimSearchType()
		{
			SimilaritySearchType simSearchType;
			string simSearchTypeName;

			GetSelectedMethod(out simSearchType, out simSearchTypeName);
			return simSearchType;
		}

		public string GetSelectedSimSearchTypeName()
		{
			SimilaritySearchType simSearchType;
			string simSearchTypeName;

			GetSelectedMethod(out simSearchType, out simSearchTypeName);
			return simSearchTypeName;
		}

		/// <summary>
		/// Get the enum & text of the selected method
		/// </summary>
		/// <param name="simSearchType"></param>
		/// <param name="simSearchTypeName"></param>

		void GetSelectedMethod(out SimilaritySearchType simSearchType, out string simSearchTypeName)
		{

			simSearchType = SimilaritySearchType.Normal;
			simSearchTypeName = "Normal";

			if (MolsimNormalCtl.Checked) { }

			else if (MolsimSubsimCtl.Checked)
			{
				simSearchType = SimilaritySearchType.Sub;
				simSearchTypeName = "Sub";
			}

			else if (MolsimSuperCtl.Checked)
			{
				simSearchType = SimilaritySearchType.Super;
				simSearchTypeName = "Super";
			}

			else if (ECFP4Ctl.Checked)
			{
				simSearchType = SimilaritySearchType.ECFP4;
				simSearchTypeName = "ECFP4";
			}

		}

		private void SaveAsDefaults_Click(object sender, EventArgs e)
		{
			string preferences = GetSelectedSimSearchTypeName() + " " +
					MinSim.Text + " " + MaxHits.Text;
				Preferences.Set("DefaultSimMethod", preferences);
			return;
		}

	}
}
