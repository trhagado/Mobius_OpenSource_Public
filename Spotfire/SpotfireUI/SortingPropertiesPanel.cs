using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;
//using Mobius.ClientComponents;

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
	public partial class SortingPropertiesPanel : DevExpress.XtraEditors.XtraUserControl
	{
		internal SpotfireViewProps SVP; // associated Spotfire View Properties
		VisualMsx V => SVP?.ActiveVisual;  // associated Visual

		SortInfoCollectionMsx SortInfo = null; // ordered list of columns to sort

		public new event EventHandler Click; // event to fire when control is clicked

		public event EventHandler ValueChangedCallback; // event to fire when edit value changes

		//ColumnsSelector SortBySelector1, SortBySelector2, SortBySelector3;

		bool InSetup = false;
		// Note: Font for caption uses Calibri, 11.25pt, style=Bold
		// Tahoma 9.75 bold is not very readable

		public SortingPropertiesPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="v"></param>

		public void Setup(
			SpotfireViewProps svp,
			SortInfoCollectionMsx sortInfo,
			EventHandler editValueChangedEventHandler = null)
		{
			InSetup = true;
			SVP = svp;
			SortInfo = sortInfo;
			ValueChangedCallback = editValueChangedEventHandler;

			SetupSortColumn(sortInfo.GetSortItem(0), SortBySelector1, Ascending1, Descending1, SortBySelector1_EditValueChanged);
			SetupSortColumn(sortInfo.GetSortItem(1), SortBySelector2, Ascending2, Descending2, SortBySelector2_EditValueChanged);
			SetupSortColumn(sortInfo.GetSortItem(2), columnsSelector3, Ascending3, Descending3, SortBySelector3_EditValueChanged);

			InSetup = false;

			return;
		}

		/// <summary>
		/// SetupSortColumn
		/// </summary>
		/// <param name="sortInfo"></param>
		/// <param name="selectorCtl"></param>
		/// <param name="ascendingCtl"></param>
		/// <param name="descendingCtl"></param>

		void SetupSortColumn(
			SortInfoMsx sortInfo,
			ColumnSelectorControl selectorCtl,
			CheckEdit ascendingCtl,
			CheckEdit descendingCtl,
			EventHandler editValueChangedEventHandler = null)
		{
			if (sortInfo == null) sortInfo = new SortInfoMsx();

			selectorCtl.OptionIncludeNoneItem = true;
			AxisMsx axis = new AxisMsx();
			DataColumnMsx dc = sortInfo.DataColumnReference;
			if (dc != null)
				axis.Expression = ExpressionUtilities.EscapeIdentifier(dc.DataTable?.Name) + "." + (dc.Name);

			selectorCtl.Setup(axis, V, SVP, editValueChangedEventHandler);

			if (sortInfo.SortOrder == SortOrderMsx.Ascending)
				ascendingCtl.Checked = true;
			else descendingCtl.Checked = true;

			return;
		}

		public SortInfoCollectionMsx GetValues()
		{
			SortInfoMsx si;

			SortInfoCollectionMsx sic = new SortInfoCollectionMsx();

			si = GetSortColumn(SortBySelector1, Ascending1);
			if (si != null) sic.SortList.Add(si);

			si = GetSortColumn(SortBySelector2, Ascending2);
			if (si != null) sic.SortList.Add(si);

			si = GetSortColumn(columnsSelector3, Ascending3);
			if (si != null) sic.SortList.Add(si);

			return sic;
		}

		SortInfoMsx GetSortColumn(
			ColumnSelectorControl selectorCtl,
			CheckEdit ascendingCtl)
		{
			//ColumnMapMsx cm = selectorCtl.SelectedColumn;
			//if (cm == null) return null;

			SortInfoMsx si = new SortInfoMsx();

			DataColumnMsx dc = selectorCtl.GetFirstSelectedDataColumn();

			if (dc != null)
			{
				si.DataColumnReference = dc;
				si.DataColumnReferenceSerializedId = dc.ReferenceId;
			}

			si.SortOrder = ascendingCtl.Checked ? SortOrderMsx.Ascending : SortOrderMsx.Descending;

			return si;
		}

		private void SortBySelector1_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void SortBySelector2_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void SortBySelector3_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Ascending1_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Descending1_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Ascending2_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Descending2_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Ascending3_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		private void Descending3_EditValueChanged(object sender, EventArgs e)
		{
			EditValueChanged();
		}

		void EditValueChanged()
		{
			if (InSetup || ValueChangedCallback == null) return;

			ValueChangedCallback(this, EventArgs.Empty);
		}

	}
}
