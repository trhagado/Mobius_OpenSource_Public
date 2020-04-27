using Mobius.ComOps;
using Mobius.Data;

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
	public partial class CriteriaStructureFSOptions : DevExpress.XtraEditors.XtraForm
	{
		static internal CriteriaStructureFSOptions Instance;
		ParsedStructureCriteria Psc;

		public CriteriaStructureFSOptions()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Allow user to edit the form
		/// </summary>
		/// <returns></returns>

		public static DialogResult Edit(ParsedStructureCriteria psc)
		{
			if (Instance == null) Instance = new CriteriaStructureFSOptions();
			Instance.Psc = psc;
			Instance.Setup();

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			return dr;
		}

		void Setup()
		{

			if (String.IsNullOrEmpty(Psc.Options))
				Psc.Options = FullStructureSearchType.Tautomer;

			else if (Psc.Exact) MExact.Checked = true;
			else if (Psc.Isomer) Isomer.Checked = true;
			else if (Psc.Tautomer) Tautomer.Checked = true;
			else if (Psc.Parent) ParentMol.Checked = true;
			else MExact.Checked = true;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			Psc.Options = FullStructureSearchType.Exact;

			if (Isomer.Checked) Psc.Options = FullStructureSearchType.Isomer;
			else if (Tautomer.Checked) Psc.Options = FullStructureSearchType.Tautomer;
			else if (ParentMol.Checked) Psc.Options = FullStructureSearchType.Parent;

			DialogResult = DialogResult.OK;
		}

		
	}
}