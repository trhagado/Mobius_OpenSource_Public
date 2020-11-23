using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace Mobius.ComOps
{
	public class SyncfusionConverter
	{
		static HashSet<string> ControlsLogged = new HashSet<string>();

		/// <summary>
		/// Convert a Windows/DevExpress Form or UserControl to a Blazor .razor Component file
		/// </summary>
		/// <param name="pc"></param>

		public static void ToRazor(Control pc)
		{
			string s;

			string razorTemplate = @"
@using Mobius.ComOps

@using Syncfusion.Blazor.Popups
@using Syncfusion.Blazor.Layouts
@using Syncfusion.Blazor.Navigations
@using Syncfusion.Blazor.Buttons
@using Syncfusion.Blazor.SplitButtons
@using Syncfusion.Blazor.Lists
@using Syncfusion.Blazor.Grids;

@using Newtonsoft.Json

@using System.Reflection
@using Microsoft.AspNetCore.Components

@using WebShell
@* using WebShell.Shared *@

@inject Microsoft.AspNetCore.Components.NavigationManager UriHelper
@inject IJSRuntime JsRuntime;

@namespace Mobius.ClientComponents
@********************************@


@********@
@* HTML *@
@********@

<html>

@********@
@*Code *@
@********@

@code{
	<code>
}

@*******@
@*CSS *@
@*******@
 ";


			//////////////////////////////////////////////
			// Build the HTML and plug into the template
			//////////////////////////////////////////////

			string html = "", header="", footer="", code=""; // html built here
			int dy = 0; // y offset to subtract WinForms window header and for y values relative to the bottom of the dialog

			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			//if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			StreamWriter sw = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + t.Name + ".txt");

			StreamWriter sw2 = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + t.Name + ".razor");

			if (typeof(Form).IsAssignableFrom(pc.GetType())) // Form or XtraForm?
			{
				Form f = pc as Form;

				dy = 32; // adjustment for WinForms header (~32 pixels). 
				int height = f.Height; // add size of WinForms header to get correct html height
				int width = f.Width;
				string headerText = f.Text;

// Wrap with SfDialog definition

				header = $@"<SfDialog Target='#target' Height='{height}px' Width='{width}px' IsModal='true' ShowCloseIcon='true' AllowDragging='true' EnableResize='false' 

					@ref ='SfDialog' @bind-Visible='DialogVisible' CssClass='dialogboxmx'>
				  <DialogTemplates>
						<Header>{headerText}</Header>
						<Content>
				 ";

				footer = $@"</Content>
					</DialogTemplates>
					<DialogEvents Opened='@DialogOpened' Closed='@DialogClosed'></DialogEvents>
				</SfDialog>" + "\r\n";

// Define variables and event stubs for code section

		code = @"
		public static " + f.Name + @" Instance { get; set; } 
		public SfDialog SfDialog { get; set; } 
		public static bool IncludeInRenderTree { get; set; } = true; 
		public bool DialogVisible { get; set; } = false; 
		public DialogResult DialogResult { get; set; } = DialogResult.None;

    protected override void OnInitialized()
    {
			Instance = this;
			base.OnInitialized();
			return;
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			return;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			return;
		}

		protected override void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
		}

		private void OK_Click()
    {
			DialogResult = DialogResult.OK;
      SfDialog.Hide();
    }

    private void Cancel_Click()
    {
			DialogResult = DialogResult.Cancel;
      SfDialog.Hide();
    }

    private void DialogOpened(OpenEventArgs args)
    {
      return;
    }

    private void DialogClosed(CloseEventArgs args)
    {
      if (Lex.Ne(args?.ClosedBy, ""User Action""))
        DialogResult = DialogResult.Cancel;

      return;
    }
";
			}

			ConvertContainerControl(pc, sw, "", header, footer, dy, ref html, ref code);

			sw.Close();

			string razor = Lex.Replace(razorTemplate, "<html>", html);

			razor = Lex.Replace(razor, "<code>", code);

			sw2.Write(razor);
			sw2.Close();
			return;

		}

		/// <summary>
		/// Convert container; Types:
		///   UserControl, fixed content
		///   WinForms/Dx container
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="sw"></param>
		/// <param name="customDivStyling"></param>
		/// <param name="header"></param>
		/// <param name="footer"></param>
		/// <param name="dy"></param>
		/// <param name="html"></param>

		static void ConvertContainerControl(
			Control pc,
			StreamWriter sw,
			string customDivStyling,
			string header,
			string footer,
			int dy,
			ref string html,
			ref string code)
		{
			Type pcType = pc.GetType();

			sw.WriteLine("Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);
			sw.WriteLine("");

			string format = "{0,-26}\t{1,-26}\t{2,5}\t{3,5}\t{4,5}\t{5,6}\t{6,-26}\t{7,-6}\t{8,-14}\t{9,-32}";
			sw.WriteLine(string.Format(format, "Type", "Name", "Left", "Top", "Width", "Height", "Anchor", "Dock", "Text", "Full Type Name"));
			sw.WriteLine(string.Format(format, "--------------------------", "--------------------------", "----", "----", "-----", "------", "--------------------------", "----", "--------------", "--------------------------"));

			if (Lex.IsDefined(header)) // wrap in header if defined
				html += header;

			else // otherwise wrap in a div
			{
				html += // wrap control in div with class name matching the Winforms/Dx control class name
					$@"<div class='{pcType.Name}' style='position: relative; width: 100%; height: 100%; border: 1px solid orange; {customDivStyling} '>" + "\r\n";
			}

			List<Control> cl = new List<Control>();

			foreach (Control c in pc.Controls) // sort controls by position
			{
				int ci = 0;
				Point p1 = c.Location;
				for (ci = 0; ci < cl.Count; ci++)
				{
					Point p2 = cl[ci].Location;
					if (p1.Y < p2.Y) break;
					if (p1.X < p2.X) break;
				}

				cl.Insert(ci, c);
			}

			foreach (Control c in cl)
			{
				//if (!cc.Visible) continue;

				Type t = c.GetType();

				// Log basic info

				sw.WriteLine(string.Format(format, t.Name, c.Name,
					c.Left, c.Top, c.Width, c.Height,
					c.Anchor, c.Dock, c.Text, t.FullName));

				int cTop = c.Top + dy; // adjust down for added header height
				int cBottom = c.Bottom + dy;

				string div = "<div style =\"position: absolute; display:flex; align-items: center; ";

				if (c.Dock == DockStyle.Fill) // if Dock defined than use that (Fill only for now)
					div += "width: 100%; height: 100%; ";

				else // use Anchor settings
				{
					string widthHeight = "";
					bool left = ((c.Anchor & AnchorStyles.Left) != 0);
					bool right = ((c.Anchor & AnchorStyles.Right) != 0);

					if (left)
					{
						div += "left: " + c.Left + "px; ";
						if (!right)
							widthHeight += "width: " + c.Width + "px; ";

						else // if left & right defined then set width as a calc()
							widthHeight += "width: calc(100% - " + (c.Left + (pc.Width - c.Right)) + "px); ";
					}

					else if (right) // set right and width
					{
						div += "right: " + (pc.Width - c.Right) + "px; ";
						widthHeight += "width: " + c.Width + "px; ";
					}

					bool anchorTop = ((c.Anchor & AnchorStyles.Top) != 0);
					bool anchorBottom = ((c.Anchor & AnchorStyles.Bottom) != 0);

					// Note: for buttons it looks like they could move up one px & be 2px taller

					if (anchorTop)
					{
						div += "top: " + cTop + "px; ";
						if (!anchorBottom)
							widthHeight += "height: " + c.Height + "px; ";

						else // if top & bottom defined then set height as a calc()
							widthHeight += "height: calc(100% - " + (cTop + (pc.Height - cBottom)) + "px); ";
					}

					else if (anchorBottom) // set bottom and height
					{
						div += "bottom: " + (pc.Height - cBottom) + "px; ";
						widthHeight += "height: " + c.Height + "px; ";
					}

					div += widthHeight; // put width height after location
				}

				div += "border: 1px solid yellow;\">\r\n"; // finish up style and div tag

				ConvertControl(c, sw, ref div, ref code); // convert the control and include conversion in containing div

				div += "</div>\r\n"; // close the div

				html += div; // add to html

			} // child control list

			if (Lex.IsDefined(header))
				html += footer;

			else html += "</div>\r\n"; // close the div

			return;
		}

		static void ConvertControl(
			Control c,
			StreamWriter sw,
			ref string html,
			ref string code)
		{
			string header = "", footer = "", htmlFrag = "", codeFrag = "";

			Type cType = c.GetType();

			////////////////////////////////////////////////////////////////////////////////
			// For user controls write reference to this file and then the control and 
			// its contents to a new file
			////////////////////////////////////////////////////////////////////////////////

			if (c is UserControl || c is XtraUserControl)
			{

				html += @"<{cType.Name}/>\r\n"; // add control reference (and parameters) to html

				ToRazor(c); // write the definition to separate Razor file if not done yet
			}

			////////////////////////////////////////////////////////////////////////////////
			// For WinForms/DX container controls write reference the control and subcontrols
			// to this file.
			////////////////////////////////////////////////////////////////////////////////

			else if (c is GroupBox)
			{
				GroupBox gb = c as GroupBox;

				header =
					"<fieldset>\r\n" +
					 $"<legend style='color:blue;font-weight:bold;'>{gb.Text}</legend>" + "\r\n";

				footer += "</fieldset>";

				ConvertContainerControl(c, sw, "", header, footer, 0, ref html, ref code);
			}

			else if (c is Panel || c is XtraPanel)
			{
				ConvertContainerControl(c, sw, "", header, footer, 0, ref html, ref code);
			}

			////////////////////////////////////////////////////////////////////////////////
			// Winforms/DX builtin control. Just the control header with and property values
			////////////////////////////////////////////////////////////////////////////////

			else if (c is LabelControl)
			{
				LabelControl l = c as LabelControl;
				if (l.LineVisible)
				{
					if (Lex.IsUndefined(l.Text))
						htmlFrag = "<hr class='hr-mobius'>\r\n";

					else // horizontal rule with text overlaid using utility css
						htmlFrag = $"<hr data-content='{l.Text}' class=' class='hr-mobius hr-text'>\r\n";
				}

				else if (Lex.IsDefined(l.Text))
				{
					htmlFrag = "<span class='mobius-mx' @onclick='mx_click'>" + l.Text + "</span>\r\n";
					//if (l.Click.Get == null)
					htmlFrag = htmlFrag.Replace("@onclick = 'mx_click'", "");
				}
			}

			else if (c is CheckEdit)
			{
				CheckEdit ce = c as CheckEdit; 
				if (ce.Properties.CheckBoxOptions.Style == CheckBoxStyle.CheckBox)
				{
					htmlFrag = $@"<SfCheckBox Label='{ce.Text}' Name='{c.Name}' Checked='{ce.Checked.ToString().ToLower()}'  
						@ref='{c.Name}' />" + "\r\n";

					codeFrag = $"SfCheckBox<bool> {c.Name};\r\n";
				}

				else
				{
					htmlFrag = $@"<SfRadioButton Label='{ce.Text}' Name='{c.Name}' Checked='{ce.Checked.ToString().ToLower()}'
						@ref='{c.Name}' />" + "\r\n";

					codeFrag = $"SfRadioButton<bool> {c.Name};\r\n";
				}
			}

			else if (c is SimpleButton)
			{ 
				// Example: <SfButton CssClass="e-flat" IsToggle="true" IsPrimary="true" 
				//            Content="@Content" IconCss="@IconCss" @ref="ToggleButton" @onclick="OnToggleClick"></SfButton>
				SimpleButton b = c as SimpleButton;
				htmlFrag = $"<SfButton Content='{c.Text}' ";

				if (Lex.Eq(c.Text, "OK") || Lex.Eq(c.Text, "Yes"))
					htmlFrag += " IsPrimary = 'true'";

				//string handler = WindowsHelper.GetEventHandlerName(c, "Click"); // doesn't work
				//if (Lex.IsDefined(handler)) ...
				htmlFrag += $"@ref='{c.Name}' @onclick = '{c.Name}_Click' />\r\n";

				codeFrag = $"SfButton {c.Name};\r\n";
			}

			else // unrecognized
			{
				htmlFrag = "<" + c.Name;

				if (Lex.IsDefined(c.Text)) // insert the element content
					htmlFrag += " name=\"" + c.Text + '"';
				htmlFrag += " />";
			}

			html += htmlFrag;
			code += codeFrag;
			return;
		}

	}
}
