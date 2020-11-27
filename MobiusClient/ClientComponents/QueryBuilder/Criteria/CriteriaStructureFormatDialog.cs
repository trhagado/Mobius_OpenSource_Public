using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaStructureFormatDialog : DevExpress.XtraEditors.XtraForm
	{

		static CriteriaStructureFormatDialog Instance = null;

		public CriteriaStructureFormatDialog()
		{
			InitializeComponent();
		}

		public static DialogResult Show(
			QueryColumn qc)
		{
			ParsedStructureCriteria pssc;
			string tok;

			if (Instance == null) Instance = new CriteriaStructureFormatDialog();
			CriteriaStructureFormatDialog cfd = Instance;

			if (!ParsedStructureCriteria.TryParse(qc, out pssc) || 
				(pssc.SearchType != StructureSearchType.Substructure && pssc.SearchType != StructureSearchType.SmallWorld &&
				 pssc.SearchType != StructureSearchType.Related))
			{
				XtraMessageBox.Show("Only structure columns with criteria can have formatting defined");
				return DialogResult.Cancel;
			}

			// Setup

			new SyncfusionConverter().ToRazor(Instance);

			if (pssc.SearchType == StructureSearchType.SmallWorld)
			{
				cfd.HilightStructures.Checked = pssc.SmallWorldParameters.Highlight;
				cfd.AlignStructures.Checked = pssc.SmallWorldParameters.Align;
			}

			else // Other structure match hilighting (i.e. SS)
			{
				cfd.HilightStructures.Checked = pssc.Highlight;
				cfd.AlignStructures.Checked = pssc.Align;
			}

			// Show form & get new values

			DialogResult dr = cfd.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			if (pssc.SearchType == StructureSearchType.SmallWorld)
			{
				pssc.SmallWorldParameters.Highlight = cfd.HilightStructures.Checked;
				pssc.SmallWorldParameters.Align = cfd.AlignStructures.Checked;
			}

			else // Other structure match hilighting (i.e. SS)
			{
				pssc.Highlight = cfd.HilightStructures.Checked;
				pssc.Align = cfd.AlignStructures.Checked;
			}

			pssc.ConvertToQueryColumnCriteria(qc);
			return dr;
		}

	}
}