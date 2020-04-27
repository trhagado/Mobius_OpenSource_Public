using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.XtraBars.Docking;

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	public class ToolTipUtil
	{
		/// <summary>
		/// AppendBitmapToToolTip
		/// </summary>
		/// <param name="stt"></param>
		/// <param name="bitmap"></param>
		/// <returns></returns>

		public static ToolTipItem AppendBitmapToToolTip(
			SuperToolTip stt,
			Bitmap bitmap)
		{
			return AppendBitmapToToolTip(stt, null, bitmap);
		}

		/// <summary>
		/// AppendBitmapToToolTip
		/// </summary>
		/// <param name="stt"></param>
		/// <param name="i"></param>
		/// <param name="bitmap"></param>
		/// <returns></returns>

		public static ToolTipItem AppendBitmapToToolTip(
			SuperToolTip stt,
			ToolTipItem i,
			Bitmap bitmap)
		{
			if (bitmap == null) return i;

			if (i != null && i.Text != "") stt.Items.Add(i);

			i = new ToolTipItem();
			i.Image = bitmap;
			stt.Items.Add(i);

			i = new ToolTipItem();
			i.AllowHtmlText = DefaultBoolean.True;
			i.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
			return i;
		}

		/// <summary>
		/// Build the arguments for displaying a SuperTooltip
		/// </summary>
		/// <param name="stt"></param>
		/// <param name="control"></param>
		/// <returns></returns>

		public static ToolTipControllerShowEventArgs BuildSuperTooltipArgs(
			SuperToolTip stt,
			Control control)
		{
			ToolTipControllerShowEventArgs ttcArgs = new ToolTipControllerShowEventArgs();
			ttcArgs.SuperTip = stt;
			ttcArgs.ToolTipType = ToolTipType.SuperTip;
			ttcArgs.Rounded = false;
			ttcArgs.RoundRadius = 1;
			ttcArgs.ShowBeak = true; // (beak doesn't show for supertip)
			ttcArgs.ToolTipLocation = ToolTipLocation.TopCenter;
			ttcArgs.SelectedControl = control;
			return ttcArgs;
		}
	}
}
