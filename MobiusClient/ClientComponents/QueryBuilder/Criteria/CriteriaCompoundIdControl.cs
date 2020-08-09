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
	public partial class CriteriaCompoundIdControl : DevExpress.XtraEditors.XtraUserControl
	{
		public QueryColumn QueryColumn; // parm values associated with control

		bool Initialized = false;
		bool InSetup = false;

		/// <summary>
		/// Basic constructor
		/// </summary>

		public CriteriaCompoundIdControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		/// <summary>
		/// Setup the form values from a model query using the first key column of the first QueryTable
		/// </summary>
		/// <param name="q0"></param>

		public void Setup(
			Query q0)
		{
			QueryColumn qc0, qc = null;

			if (q0 == null) Setup(qc); // create default qc if no query

			Query q = null;
			QueryTable qt;
			MetaTable mt = null;

			if (q0 != null && q0.Tables.Count > 0)
			{
				qc0 = q0.Tables[0].KeyQueryColumn;
				if (qc0 != null)
				{
					qc0.CopyCriteriaFromQueryKeyCriteria(); // sync the qc to query KeyCriteria

					qc = qc0.Clone();
					qc.QueryTable = null; // unlink new QC from model QueryTable & Query
				}
			}

			Setup(qc);

			return;
		}

		/// <summary>
		/// Setup the form values
		/// </summary>
		/// <param name="qc"></param>

		public void Setup(
			QueryColumn qc)
		{
			InSetup = true;

			MetaTable mt = null;

			if (qc != null)
			{
				qc.CopyCriteriaFromQueryKeyCriteria(); // sync the qc to query KeyCriteria
			}

			else // create a default qc
				qc = QueryTable.GetDefaultRootQueryTable()?.KeyQueryColumn;

			if (Lex.IsDefined(qc.CriteriaDisplay)) Cids.Text = qc.CriteriaDisplay;
			else if (Lex.IsDefined(qc.Criteria)) Cids.Text = qc.Criteria;
			else Cids.Text = "";

			QueryColumn = qc; // QueryColumn instance associated with this control

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
				Cids.Text = "= " + c; // todo: put Qc.criteria...
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
			QueryColumn qc = QueryColumn;
			AssertMx.IsNotNull(qc, "QueryColumn");

			if (!CriteriaEditor.GetCompoundIdCriteria(qc)) return false;

			if (Lex.IsDefined(qc.CriteriaDisplay))
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
			QueryColumn tso)
		{
			QueryColumn qc = QueryColumn;
			AssertMx.IsNotNull(qc, "QueryColumn");

			return true;
		}

	}
}
