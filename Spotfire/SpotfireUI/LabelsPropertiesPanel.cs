using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	public partial class LabelsPropertiesPanel : DevExpress.XtraEditors.XtraUserControl
	{

		internal SpotfireViewProps SVP; // associated Spotfire View Properties

		public event EventHandler ValueChangedCallback = null; // event to fire when edit value changes
		bool InSetup = false;

		public LabelsPropertiesPanel()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}
	}
}
