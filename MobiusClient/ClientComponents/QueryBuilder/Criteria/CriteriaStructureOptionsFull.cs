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
	public partial class CriteriaStructureOptionsFull : UserControl
	{
		public CriteriaStructureOptionsFull()
		{
			InitializeComponent();
		}

		public void Setup(ParsedStructureCriteria psc)
		{

			if (String.IsNullOrEmpty(psc.Options))
				psc.Options = FullStructureSearchType.Tautomer;

			if (psc.Exact) MExact.Checked = true;
			else if (psc.Isomer) Isomer.Checked = true;
			else if (psc.Tautomer) Tautomer.Checked = true;
			else if (psc.Parent) ParentMol.Checked = true;
			else MExact.Checked = true;
		}

		public bool GetValues(ParsedStructureCriteria psc)
		{
			if (MExact.Checked) psc.Options = FullStructureSearchType.Exact;
			else if (Isomer.Checked) psc.Options = FullStructureSearchType.Isomer;
			else if (Tautomer.Checked) psc.Options = FullStructureSearchType.Tautomer;
			else if (ParentMol.Checked) psc.Options = FullStructureSearchType.Parent;
			return true;
		}

	}
}
