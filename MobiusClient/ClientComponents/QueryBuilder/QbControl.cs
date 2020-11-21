using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

/// <summary>
/// This class builds a single query
/// </summary>

	public partial class QbControl : XtraUserControl
	{
		public QueryTable Qt; // current query table
		public int Qti // index of query table within query
		{
			get
			{
				if (Qt == null) return -1;
				else return Qt.Query.GetQueryTableIndex(Qt);
			}
		}

		public int[] QbGridRowToQueryColIdx; // maps query builder row to Query column Index
		public int[] QueryColIdxToQbGridRow; // maps Query column Index to query builder row
		public MoleculeGridControl MoleculeGrid; // current MoleculeGrid instance with focus

		public int InitialSplitterPos = -1; // splitter position to set when first painting

		public QbControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);
		}

		private void QbSplitter_Paint(object sender, PaintEventArgs e)
		{
			if (InitialSplitterPos > 0)
			{
				QbSplitter.SplitterPosition = InitialSplitterPos;
				InitialSplitterPos = -1;
			}

		}

		private void QueryTablesControl_Load(object sender, EventArgs e)
		{

		}
	}
}
