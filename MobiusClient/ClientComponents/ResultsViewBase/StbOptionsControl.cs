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
	public partial class StbOptionsControl : DevExpress.XtraEditors.XtraUserControl
	{
		TargetSummaryOptions Tso; // parm values associated with control

		bool Initialized = false;
		bool InSetup = false;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public StbOptionsControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
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

			Cids.Text = tso.CidCriteria; // set initial cid criteria
			InSetup = false;

			return;
		}

		/// <summary>
		/// Cids_KeyDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Cids_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;

			string c = Char.ConvertFromUtf32(e.KeyValue);

			if (Cids.Text == "" && Char.IsLetterOrDigit(c, 0)) // handle first char
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
		/// GetFormValues
		/// </summary>
		/// <param name="tso"></param>
		/// <returns></returns>

		public bool GetFormValues(
			TargetSummaryOptions tso)
		{
			tso.CidCriteria = Cids.Text;
			return true;
		}

	}
}
