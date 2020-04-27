using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
using Mobius.ClientComponents;

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
	public partial class GeneralPropertiesPanel : DevExpress.XtraEditors.XtraUserControl
	{
		public event EventHandler CallerEditValueChangedEventHandler; // event to fire when edit value changes
		bool InSetup = false;

		public GeneralPropertiesPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		public void Setup(
			VisualMsx v,
			EventHandler callercallerEditValueChangedEventHandler = null)
		{
			Title.Text = v.Title;
			ShowTitle.Checked = v.ShowTitle;
			Description.Text = v.Description;
			ShowDescriptionInVis.Checked = v.ShowDescription;

			CallerEditValueChangedEventHandler = callercallerEditValueChangedEventHandler;
		}

		public void GetValues(VisualMsx v)
		{
			InSetup = true;
			v.Title = Title.Text;
			v.ShowTitle = ShowTitle.Checked;
			v.ShowDescription = ShowDescriptionInVis.Checked;
			v.Description = Description.Text;
			InSetup = false;
		}

		private void Title_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void ShowTitle_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Description_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}


		private void ShowDescriptionInVis_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		/// <summary>
		/// Call any EditValueChanged event
		/// </summary>

		void EditValueChanged(
			object sender = null,
			EventArgs e = null)
		{
			if (InSetup) return;

			if (CallerEditValueChangedEventHandler != null) // fire EditValueChanged event if handlers present
				CallerEditValueChangedEventHandler(this, EventArgs.Empty);
		}

	}
}
