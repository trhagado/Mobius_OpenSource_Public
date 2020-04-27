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
	public partial class CriteriaStructureOptionsSS : UserControl
	{
		public CriteriaStructureOptionsSS()
		{
			InitializeComponent();
		}

/// <summary>
/// Setup option subcontrols for ParsedStructureCriteria
/// </summary>
/// <param name="psc"></param>

		public void Setup(ParsedStructureCriteria psc)
		{
			Highlight.Checked = psc.Highlight;
			Align.Checked = psc.Align;
		}

/// <summary>
/// Store option subcontrol values in ParsedStructureCriteria
/// </summary>
/// <returns></returns>

		public bool GetValues(ParsedStructureCriteria psc)
		{
			psc.Highlight = Highlight.Checked;
			psc.Align = Align.Checked;

			return true;
		}

	}
}
