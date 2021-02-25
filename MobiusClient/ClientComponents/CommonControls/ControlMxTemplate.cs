using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.ComOps
{
	public class ControlMxTemplate
	{
	}

#if false
using Mobius.ComOps;
using Mobius.Data;
using Mobius.BaseControls;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Text;

namespace Mobius.ClientComponents
{
	public partial class <className> : DialogBoxMx, IDialogBoxMx
	{

		/******************************* File links *********************************/
		public static <className> DesignFile => <className>.CsFile;
		/****************************************************************************/

		public static <className> Instance { get; set; }
		public string HeaderText = "MessageBoxEx";
		public static bool IncludeInRenderTree { get; set; } = true;
		public bool DialogVisible { get; set; } = false;

    PictureBoxMx MessageBoxImage = new PictureBoxMx() { StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; left: 12px; top: 60px; width: 32px; height: 32px; ") };

		LabelControlMx Message = new LabelControlMx()
		{
			Text = "Message...",
			StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; " +
			"overflow: hidden; text-overflow: ellipsis; " +
		"scroll; left: 56px; top: 34px; width: calc(100% - 74px); height: calc(100% - 68px); background-color: #eeeeee; ")
		};

		ButtonMx YesButton = new ButtonMx() { Text = "Yes", ImageName = "", StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; right: 264px; bottom: 8px; width: 76px; height: 23px; ") };
		ButtonMx NoButton = new ButtonMx() { Text = "No", ImageName = "", StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; right: 182px; bottom: 8px; width: 76px; height: 23px; ") };
		ButtonMx NoToAllButton = new ButtonMx() { Text = "No to All", ImageName = "", StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; right: 182px; bottom: 8px; width: 76px; height: 23px; ") };
		ButtonMx OK = new ButtonMx() { Text = "OK", ImageName = "", StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; right: 100px; bottom: 8px; width: 76px; height: 23px; ") };
		ButtonMx Cancel = new ButtonMx() { Text = "Cancel", ImageName = "", StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; right: 18px; bottom: 8px; width: 76px; height: 23px; ") };


    public <className>()
    {

      StyleProps = new CssStyleMx("display: inline-flex; position: absolute; align-items: center; width: 354px; height: 154px; font-size: 13px; border: 1px solid #acacac;");
      StyleClasses = new HashSet<string> { "control-div-mx", "font-mx", "defaults-mx" };

      ChildControls.Add(MessageBoxImage);
      MessageBoxImage.ImageName = "MessageBoxQuestionIcon32.png";

      ChildControls.Add(Message);
      ChildControls.Add( YesButton);
      ChildControls.Add( NoButton);
      ChildControls.Add( NoToAllButton);
      ChildControls.Add( OK);
      ChildControls.Add( Cancel);
      return;
    }

		/*************************************************/
		/*** Basic SfDialog overrides and Click events ***/
		/*************************************************/



          /// <summary>
          /// YesButton_Click
          /// </summary>
          /// <returns></returns>

            private async Task YesButton_Click()
            {
              await Task.Yield();
              return;
            }


          /// <summary>
          /// NoButton_Click
          /// </summary>
          /// <returns></returns>

            private async Task NoButton_Click()
            {
              await Task.Yield();
              return;
            }


          /// <summary>
          /// NoToAllButton_Click
          /// </summary>
          /// <returns></returns>

            private async Task NoToAllButton_Click()
            {
              await Task.Yield();
              return;
            }

#if false
    /// <summary>
    /// OK_Click
    /// </summary>

    private async Task OK_Click()
          {
            DialogResult = DialogResult.OK;
            await SfDialog.Hide();
            return;
          }


            /// <summary>
            /// Cancel_Click
            /// </summary>

            private async Task Cancel_Click()
          {
            DialogResult = DialogResult.Cancel;
            await SfDialog.Hide();
          }

#endif


	}
}

#endif
}
