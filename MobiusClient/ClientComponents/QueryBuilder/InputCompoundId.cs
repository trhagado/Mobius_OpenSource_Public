using Mobius.ComOps;
using Mobius.Data; 
using Mobius.CdkMx;
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
	public partial class InputCompoundId : DevExpress.XtraEditors.XtraForm
	{
		string PreviousCid = ""; // most recently seen number
		static bool InTimer_Tick = false;
		MetaTable MetaTable; 

		static InputCompoundId Instance;

		public InputCompoundId()
		{
			InitializeComponent();
		}

		public static string Show(
			string prompt,
			string title,
			string initialCid,
			MetaTable mt)
		{
			//if (Instance == null) 
			Instance = new InputCompoundId(); // always allocate a new class

			Instance.Text = title;
			Instance.Prompt.Text = prompt;
			Instance.CidCtl.Text = initialCid;
			Instance.PreviousCid = "";
			Instance.QuickStructure.ClearMolecule();
			Instance.MetaTable = mt;

			if (mt != null && Lex.Eq(mt.Name, MetaTable.SmallWorldMetaTableName))
				Instance.MetaTable = null; // don't use smallworld db directly since can't retrieve by CID

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK) return Instance.CidCtl.Text;
			else return null;
		}

/// <summary>
/// Get the current molecule
/// </summary>

		public static MoleculeMx Molecule
		{
			get
			{
				if (Instance == null) return null;
				return Instance.QuickStructure.Molecule;
			}
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			if ( ServiceFacade.ServiceFacade.InServiceCall) return; // avoid multiplexing service calls that could cause problems
			if (InTimer_Tick) return;
			InTimer_Tick = true;

			string currentCid = CidCtl.Text;
			if (currentCid == PreviousCid)
			{
				InTimer_Tick = false;
				return;
			}

			PreviousCid = currentCid;

			string cid = CompoundId.Normalize(currentCid, MetaTable);
			MetaTable mt = CompoundId.GetRootMetaTableFromCid(cid, MetaTable);
			MoleculeMx mol = MoleculeUtil.SelectMoleculeForCid(cid, mt);

			QuickStructure.SetupAndRenderMolecule(mol);

			InTimer_Tick = false;
			return;
		}

		private void InputCompoundId_Activated(object sender, EventArgs e)
		{
			Timer1.Enabled = true;
			CidCtl.Focus();
		}

		private void InputCompoundId_Deactivate(object sender, EventArgs e)
		{
			Timer1.Enabled = false;
		}

	}
}
