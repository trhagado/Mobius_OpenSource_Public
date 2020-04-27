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
	public partial class CriteriaStructureSimOptions : DevExpress.XtraEditors.XtraForm
	{
		static internal CriteriaStructureSimOptions Instance;

		ParsedStructureCriteria Psc;

		public CriteriaStructureSimOptions()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Allow user to edit the form
		/// </summary>
		/// <returns></returns>

		public static DialogResult Edit(ParsedStructureCriteria psc)
		{
			if (Instance == null) Instance = new CriteriaStructureSimOptions();
			Instance.Psc = psc;
			Instance.Setup();

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		void Setup()
		{
			InitSimOptionsIfUndefined(Psc);

			if (Psc.SimilarityType == SimilaritySearchType.Normal) Normal.Checked = true;
			else if (Psc.SimilarityType == SimilaritySearchType.Sub) Sub.Checked = true;
			else if (Psc.SimilarityType == SimilaritySearchType.Super) Super.Checked = true;

			else if (Psc.SimilarityType == SimilaritySearchType.ECFP4) ECFP4.Checked = true;

			MinSim.Text = Psc.MinimumSimilarity.ToString();
			if (Psc.MaxSimHits > 0) MaxHits.Text = Psc.MaxSimHits.ToString();
			else MaxHits.Text = "";
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

			if (psc.MinimumSimilarity < 0 || psc.MinimumSimilarity > 1.0)
				psc.MinimumSimilarity = .75;
		}

/// <summary>
/// Enable/disable gfp options
/// </summary>
/// <param name="enable"></param>

		public static void Enable(bool basicChemAvailable, bool ECFP4_Available)
		{
			if (Instance == null) Instance = new CriteriaStructureSimOptions();

			Instance.ECFP4.Enabled = ECFP4_Available;

			Instance.Normal.Enabled = basicChemAvailable;
			Instance.Sub.Enabled = basicChemAvailable;
			Instance.Super.Enabled = basicChemAvailable;

			return;
		}

		private void SaveSimDefault_Click(object sender, EventArgs e)
		{
			string parm = GetSelectedSimSearchTypeName() + " " + 
				MinSim.Text + " " + MaxHits.Text;
			Preferences.Set("DefaultSimMethod", parm);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			double minSim = 0;
			double.TryParse(MinSim.Text, out minSim);
			if (minSim < 0 || minSim > 1.0)
			{
				MessageBoxMx.ShowError("Upper similarity must be between 0.0 and 1.0");
				return;
			}

			Psc.MinimumSimilarity = minSim;

			int maxHits = -1;
			int.TryParse(MaxHits.Text, out maxHits);
			Psc.MaxSimHits = maxHits;

			Psc.SimilarityType = GetSelectedSimSearchType();

			DialogResult = DialogResult.OK;
		}

		SimilaritySearchType GetSelectedSimSearchType()
		{
			SimilaritySearchType simSearchType;
			string simSearchTypeName;

			GetSelectedMethod(out simSearchType, out simSearchTypeName);
			return simSearchType;
		}

		string GetSelectedSimSearchTypeName()
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

			if (Normal.Checked) { }

			else if (Sub.Checked)
			{
				simSearchType = SimilaritySearchType.Sub;
				simSearchTypeName = "Sub";
			}
			else if (Super.Checked)
			{
				simSearchType = SimilaritySearchType.Super;
				simSearchTypeName = "Super";
			}
			else if (ECFP4.Checked)
			{
				simSearchType = SimilaritySearchType.ECFP4;
				simSearchTypeName = "ECFP4";
			}
		}

	}
}