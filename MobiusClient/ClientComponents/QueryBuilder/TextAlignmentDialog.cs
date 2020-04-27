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
using System.Windows;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class TextAlignmentDialog : DevExpress.XtraEditors.XtraForm
	{
		public static TextAlignmentDialog Instance;

		public TextAlignmentDialog()
		{
			InitializeComponent();
		}

		public static DialogResult Show(QueryColumn qc)
		{
			if (Instance == null) Instance = new TextAlignmentDialog();
			return Instance.ShowInstance(qc);
		}

		DialogResult ShowInstance(QueryColumn qc)
		{
			MetaColumn mc = qc.MetaColumn;

			HorizontalAlignmentEx ha = qc.ActiveHorizontalAlignment;
			SetHorizontalAlignment(qc.ActiveHorizontalAlignment);

			SetVerticalAlignment(qc.ActiveVerticalAlignment);

			ApplyToAllColumns.Checked = false;
			SetAsDefault.Checked = false;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			qc.HorizontalAlignment = GetHorizontalAlignment();
			qc.VerticalAlignment = GetVerticalAlignment();

			if (ApplyToAllColumns.Checked && qc.QueryTable != null &&
				qc.QueryTable.Query != null)
			{ // apply to all columns
				foreach (QueryTable qt0 in qc.QueryTable.Query.Tables)
				{
					foreach (QueryColumn qc0 in qt0.QueryColumns)
					{
						qc0.HorizontalAlignment = qc.HorizontalAlignment;
						qc0.VerticalAlignment = qc.VerticalAlignment;
					}
				}
			}

			if (SetAsDefault.Checked)
			{ // set as user default
				MetaColumn.SessionDefaultHAlignment = qc.ActiveHorizontalAlignment;
				UserObjectDao.SetUserParameter(SS.I.UserName, "DefaultHorizontalAlignment", MetaColumn.SessionDefaultHAlignment.ToString());

				MetaColumn.SessionDefaultVAlignment = qc.ActiveVerticalAlignment;
				UserObjectDao.SetUserParameter(SS.I.UserName, "DefaultVerticalAlignment", MetaColumn.SessionDefaultVAlignment.ToString());
			}

			return dr;
		}

		void SetHorizontalAlignment(HorizontalAlignmentEx ha)
		{
			if (ha == HorizontalAlignmentEx.Default) HaGeneral.Checked = true;
			else if (ha == HorizontalAlignmentEx.Left) HaLeft.Checked = true;
			else if (ha == HorizontalAlignmentEx.Center) HaCenter.Checked = true;
			else if (ha == HorizontalAlignmentEx.Right) HaRight.Checked = true;
			else HaGeneral.Checked = true;

			SetHorizontalImages();
		}

		void SetHorizontalImages()
		{
			SetImage(HaGeneral);
			SetImage(HaLeft);
			SetImage(HaCenter);
			SetImage(HaRight);
		}

		HorizontalAlignmentEx GetHorizontalAlignment()
		{
			if (HaGeneral.Checked) return HorizontalAlignmentEx.Default;
			else if (HaLeft.Checked) return HorizontalAlignmentEx.Left;
			else if (HaCenter.Checked) return HorizontalAlignmentEx.Center;
			else if (HaRight.Checked) return HorizontalAlignmentEx.Right;
			else return HorizontalAlignmentEx.Default;
		}

		void SetVerticalAlignment(VerticalAlignmentEx va)
		{
			if (va == VerticalAlignmentEx.Top) VaTop.Checked = true;
			else if (va == VerticalAlignmentEx.Middle) VaMiddle.Checked = true;
			else if (va == VerticalAlignmentEx.Bottom) VaBottom.Checked = true;
			else VaTop.Checked = true;

			SetVerticalImages();
		}

		void SetVerticalImages()
		{
			SetImage(VaTop);
			SetImage(VaMiddle);
			SetImage(VaBottom);
		}

		void SetImage(CheckButton b)
		{
			int ii = (b.ImageIndex / 2) * 2;
			if (!b.Checked) b.ImageIndex = ii;
			else b.ImageIndex = ii + 1;
		}

		VerticalAlignmentEx GetVerticalAlignment()
		{
			if (VaTop.Checked) return VerticalAlignmentEx.Top;
			else if (VaMiddle.Checked) return VerticalAlignmentEx.Middle;
			else if (VaBottom.Checked) return VerticalAlignmentEx.Bottom;
			else return VerticalAlignmentEx.Default;
		}

		private void HaGeneral_CheckedChanged(object sender, EventArgs e)
		{
			SetHorizontalImages();
		}

		private void HaLeft_CheckedChanged(object sender, EventArgs e)
		{
			SetHorizontalImages();
		}

		private void HaCenter_CheckedChanged(object sender, EventArgs e)
		{
			SetHorizontalImages();
		}

		private void HaRight_CheckedChanged(object sender, EventArgs e)
		{
			SetHorizontalImages();
		}

		private void VaTop_CheckedChanged(object sender, EventArgs e)
		{
			SetVerticalImages();
		}

		private void VaMiddle_CheckedChanged(object sender, EventArgs e)
		{
			SetVerticalImages();
		}

		private void VaBottom_CheckedChanged(object sender, EventArgs e)
		{
			SetVerticalImages();
		}

		private void GeneralLabel_Click(object sender, EventArgs e)
		{
			HaGeneral.Checked = !HaGeneral.Checked;
		}

		private void LeftLabel_Click(object sender, EventArgs e)
		{
			HaLeft.Checked = !HaLeft.Checked;
		}

		private void CenterLabel_Click(object sender, EventArgs e)
		{
			HaCenter.Checked = !HaCenter.Checked;
		}

		private void RightLabel_Click(object sender, EventArgs e)
		{
			HaRight.Checked = !HaRight.Checked;
		}

		private void TopLabel_Click(object sender, EventArgs e)
		{
			VaTop.Checked = !VaTop.Checked;
		}

		private void MiddleLabel_Click(object sender, EventArgs e)
		{
			VaMiddle.Checked = !VaMiddle.Checked;
		}

		private void BottomLabel_Click(object sender, EventArgs e)
		{
			VaBottom.Checked = !VaBottom.Checked;
		}

	}
}