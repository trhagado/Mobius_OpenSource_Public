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
		HashSet<string> RadioGroups = new HashSet<string>(); // names of each of the radio button groups seen 
		StreamWriter log;

		/// <summary>
		/// Convert a Windows/DevExpress Form or UserControl to a Blazor .razor Component file
		/// </summary>
		/// <param name="pc"></param>

		public void ToRazor(Control pc)
		{
			string s;

			string razorTemplate = @"
@using Mobius.ComOps
@using WebShell

@using Syncfusion.Blazor.Popups
@using Syncfusion.Blazor.Layouts
@using Syncfusion.Blazor.Navigations
@using Syncfusion.Blazor.Inputs
@using Syncfusion.Blazor.Buttons
@using Syncfusion.Blazor.SplitButtons
@using Syncfusion.Blazor.Lists
@using Syncfusion.Blazor.Grids;

@using Newtonsoft.Json

@using System.Reflection
@using Microsoft.AspNetCore.Components

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

			string html = "", code = ""; // strings for global building of html and code sections

			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			//if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			log = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + t.Name + ".txt");

			RadioGroups = new HashSet<string>();
			

			// ======================================================
			// Process a top level Form or XtraForm (i.e. DialogBox)
			// ======================================================

			if (typeof(Form).IsAssignableFrom(pc.GetType())) // Form or XtraForm?
			{
				Form f = pc as Form;

				int height = f.Height; // add size of WinForms header to get correct html height
				int width = f.Width;
				string headerText = f.Text;

				// Wrap with SfDialog definition

				html = $@"<SfDialog Target='#target' Height='{height}px' Width='{width}px' IsModal='true' ShowCloseIcon='true' AllowDragging='true' EnableResize='false' 

					@ref ='SfDialog' @bind-Visible='DialogVisible' CssClass='dialogboxmx'>
				  <DialogTemplates>
						<Header>
							<div style='display:flex; align-items:center; width: 100%; height: 20px; font-size: 13px; border: 0px;'>@HeaderText</div>
						</Header>
					<Content>
				 ";

			// Define variables and event stubs for code section

				code = @"
		public static " + f.Name + @" Instance { get; set; } 
		public SfDialog SfDialog { get; set; } 
		public string HeaderText = " + Lex.AddDoubleQuotes(headerText)  + @";
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

    private async Task DialogOpened(Syncfusion.Blazor.Popups.OpenEventArgs args)
    {
			args.PreventFocus = true;
			await OK.FocusIn();
      return;
    }

    private void DialogClosed(CloseEventArgs args)
    {
      if (Lex.Ne(args?.ClosedBy, ""User Action""))
        DialogResult = DialogResult.Cancel;

      return;
    }
";

				ConvertContainedControls(pc, 0, ref html, ref code);

				html += // Finish up the SfDialog component
					$@"</Content>
					</DialogTemplates>
					<DialogEvents Opened='@DialogOpened' Closed='@DialogClosed'></DialogEvents>
				</SfDialog>" + "\r\n";

			}

			// ======================================================
			// Process a UserControl or XtraUserControl
			// ======================================================

			else if (pc is UserControl || pc is XtraUserControl)
			{
				html =
				 @"<!-- Cascade this instance of the component down to all lower components -->

					<CascadingValue Value='@this'>" + "\r\n";

				ConvertContainedControls(pc, 0, ref html, ref code);

				html += "</CascadingValue>"; // finish up the HTML part of the component definition

			}

			else throw new Exception("Can't convert control of type: " + pc?.GetType()?.Name);

			string razor = Lex.Replace(razorTemplate, "<html>", html);

			razor = Lex.Replace(razor, "<code>", code);

			log.Close();

			StreamWriter sw = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + t.Name + ".razor");
			sw.Write(razor);
			sw.Close();
			return;

		}

		/// <summary>
		/// Convert a list of controls for a control container which can be any of the following:
		///   UserControl, XtraUserControl
		///   WinForms/Dx predefined container (e.g. GroupBox, Panel...)
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="sw"></param>
		/// <param name="customDivStyling"></param>
		/// <param name="header"></param>
		/// <param name="footer"></param>
		/// <param name="html"></param>

		void ConvertContainedControls(
			Control pc,
			int dy,
			ref string html,
			ref string code)
		{
			Type pcType = pc.GetType();

			log.WriteLine("");
			log.WriteLine("Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);

			string format = "{0,-26}\t{1,-26}\t{2,5}\t{3,5}\t{4,5}\t{5,6}\t{6,-26}\t{7,-6}\t{8,-14}\t{9,-32}";
			log.WriteLine(string.Format(format, "Type", "Name", "Left", "Top", "Width", "Height", "Anchor", "Dock", "Text", "Full Type Name"));
			log.WriteLine(string.Format(format, "--------------------------", "--------------------------", "----", "----", "-----", "------", "--------------------------", "----", "--------------", "--------------------------"));

			//if (Lex.IsDefined(header)) // wrap in header if defined
			//	html += header;

			//else // otherwise wrap in a div
			//{
			//	html += // wrap control in div with class name matching the Winforms/Dx control class name
			//		$@"<div class='font-mx defaults-mx' class='{pcType.Name}' style='position: relative; width: 100%; height: 100%; border: 1px solid orange; {customDivStyling} '>" + "\r\n";
			//}

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

				log.WriteLine(string.Format(format, t.Name, c.Name,
					c.Left, c.Top, c.Width, c.Height,
					c.Anchor, c.Dock, c.Text, t.FullName));

				ConvertControl(pc, c, dy, ref html, ref code); // convert the control and surround with a positioning div

			} // child control list

			log.WriteLine("End of Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);

			return;
		}

		void ConvertControl(
			Control pc,
			Control c,
			int dy,
			ref string html,
			ref string code)
		{
			string htmlFrag = "", codeFrag = "", groupName = "";
			int dy2 = 0;

			Type cType = c.GetType();
			DivMx div = new DivMx(); // the div to surround and position the control

			////////////////////////////////////////////////////////////////////////////////
			// For user controls write reference to this file and then the control and 
			// its contents to a new file
			////////////////////////////////////////////////////////////////////////////////

			if (c is UserControl || c is XtraUserControl)
			{
				htmlFrag += div.Build(pc, c, dy); // build the div

				htmlFrag += $"<{cType.Name} @ref = '{cType.Name}'/>\r\n"; // add control with a @ref reference (and parameters) to html

				htmlFrag += div.Close();

				codeFrag += $"\r\npublic {cType.Name} {cType.Name}\r\n"; // add a variable to contain a reference to the control instance

				new SyncfusionConverter().ToRazor(c); // write the definition to separate Razor file if not done yet
			}

			////////////////////////////////////////////////////////////////////////////////
			// For WinForms/DX container controls write reference the control and subcontrols
			// to this file.
			////////////////////////////////////////////////////////////////////////////////

			else if (c is GroupBox)
			{
				GroupBox gb = c as GroupBox;

				gb.Height += 8;
				gb.Top -= 8;

				htmlFrag += div.Build(pc, c, dy); // build the div

				gb.Height -= 8;
				gb.Top += 8;

				htmlFrag +=
					"<fieldset class='fieldset-mx'>\r\n" +
					 $"<legend class='legend-mx'>{gb.Text}</legend>\r\n";

				dy2 = 4; // move contained controls down a bit
				ConvertContainedControls(c, dy2, ref htmlFrag, ref codeFrag); 
		
				htmlFrag += "</fieldset>\r\n";

				htmlFrag += div.Close();
			}

			else if (c is Panel || c is XtraPanel)
			{
				htmlFrag += div.Build(pc, c, dy);

				htmlFrag += ""; // todo...

				ConvertContainedControls(c, dy2, ref htmlFrag, ref codeFrag);

				htmlFrag += ""; // todo...

				htmlFrag += div.Close();
			}

			////////////////////////////////////////////////////////////////////////////////
			// Winforms/DX builtin controls
			////////////////////////////////////////////////////////////////////////////////

			/////////////////////////////////////////////////////////////////////////////////
			// LabelControl
			/////////////////////////////////////////////////////////////////////////////////


			else if (c is LabelControl)
			{
				htmlFrag += div.Build(pc, c, dy);

				LabelControl l = c as LabelControl;
				if (l.LineVisible)
				{
					if (Lex.IsUndefined(l.Text))
						htmlFrag += "<hr class='hr-mobius'>\r\n";

					else // horizontal rule with text overlaid using utility css
						htmlFrag += $"<hr data-content='{l.Text}' class=' class='hr-mobius hr-text'>\r\n";
				}

				else if (Lex.IsDefined(l.Text))
				{
					htmlFrag += $"<span>@{c.Name}.Text</span>\r\n";
					codeFrag += $"LabelControlMx {c.Name} = new LabelControlMx(\"{l.Text}\");\r\n";
				}

				htmlFrag += div.Close();
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckEdit (CheckBox and RadioButton
			/////////////////////////////////////////////////////////////////////////////////


			else if (c is CheckEdit)
			{
				if (dy == 0)
					dy = -4; // move up a bit

				htmlFrag += div.Build(pc, c, dy);

				CheckEdit ce = c as CheckEdit;
				if (ce.Properties.CheckBoxOptions.Style == CheckBoxStyle.CheckBox ||
						ce.Properties.CheckBoxOptions.Style == CheckBoxStyle.Default)
				{ // CheckBox
					htmlFrag += $@"<SfCheckBox CssClass='font-mx defaults-mx' Label='{ce.Text}' Name='{c.Name}'   
						@ref ='{c.Name}.Button' @bind-Checked='{c.Name}.Checked' />" + "\r\n";

					codeFrag += $"CheckBoxMx {c.Name} = new CheckBoxMx();\r\n";
				}

				else // RadioButton
				{

					groupName = "RadioGroupValue" + ce.Properties.RadioGroupIndex;
						htmlFrag += $@"<SfRadioButton CssClass='font-mx defaults-mx' Label='{ce.Text}' Name='{groupName}' Value='{ce.Name}'
						@ref ='{c.Name}.Button' @bind-Checked='{groupName}.CheckedValue' />" + "\r\n";

					if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
					{
						codeFrag += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
						RadioGroups.Add(groupName);
					}

					codeFrag += $"RadioButtonMx {c.Name} = new RadioButtonMx({groupName});\r\n";
				}

				htmlFrag += div.Close();
			}

			/////////////////////////////////////////////////////////////////////////////////
			// TextEdit
			/////////////////////////////////////////////////////////////////////////////////

			else if (c is TextEdit)
			{
				htmlFrag += div.Build(pc, c, dy);

				// Example: <SfTextBox @bind-Value="@TextBoxValue" @ref="@SfTextBox" Type="InputType.Text" Placeholder="@InitialText" />

				TextEdit te = c as TextEdit;

				htmlFrag += $"<SfTextBox CssClass='e-small defaults-mx' @ref='{c.Name}.SfTextBox' @bind-Value='{c.Name}.Text' Type='InputType.Text' />\r\n";

				codeFrag += $"TextBoxMx {c.Name} = new TextBoxMx();\r\n";

				htmlFrag += div.Close();
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckButton - Button with a checked state and button group like a RadioButton
			/////////////////////////////////////////////////////////////////////////////////

			else if (c is CheckButton)
			{
				// CheckButtons and their groups are handled in the same way as RadioButtons
				// with the value for checked comparison being the ID of the SfButton.

				htmlFrag += div.Build(pc, c, dy);

				CheckButton cb = c as CheckButton;

				groupName = "CheckButtonGroup" + cb.GroupIndex;
				string iconName = c.Name;
				iconName = Lex.Replace(iconName, "Va", ""); // hack for TextAlignment Dialog
				iconName = Lex.Replace(iconName, "Ha", "");

				if (!Lex.StartsWith(iconName, "Align")) iconName = "Align" + iconName; // make name match SF icon names
				if (!Lex.EndsWith(iconName, "IconMx")) iconName += "IconMx";

				htmlFrag += $@"<SfButton @ref ='{c.Name}.Button' @bind-CssClass='{c.Name}.CssClass' IconCss = '{iconName}' 
					Content='{c.Text}' OnClick='{c.Name}_Click' />" + "\r\n";

				groupName = "CheckButtonGroupValue" + (cb.GroupIndex);
				if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
				{
					codeFrag += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
					RadioGroups.Add(groupName);
				}

				codeFrag += $"CheckButtonMx {c.Name} = new CheckButtonMx({groupName});\r\n";

				htmlFrag += div.Close();
			}

			/////////////////////////////////////////////////////////////////////////////////
			// SimpleButton
			/////////////////////////////////////////////////////////////////////////////////

			else if (c is SimpleButton)
			{
				htmlFrag += div.Build(pc, c, dy);

				// Example: <SfButton CssClass="e-flat" IsToggle="true" IsPrimary="true" 
				//            Content="@Content" IconCss="@IconCss" @ref="ToggleButton" @onclick="OnToggleClick"></SfButton>
				SimpleButton b = c as SimpleButton;
				htmlFrag += $"<SfButton CssClass='button-mx' Content='{c.Text}' ";

				if (Lex.Eq(c.Text, "OK") || Lex.Eq(c.Text, "Yes"))
					htmlFrag += " IsPrimary = 'true'";

				//string handler = WindowsHelper.GetEventHandlerName(c, "Click"); // doesn't work
				//if (Lex.IsDefined(handler)) ...
				htmlFrag += $"@ref='{c.Name}' @onclick = '{c.Name}_Click' />\r\n";

				codeFrag += $"SfButton {c.Name};\r\n";

				htmlFrag += div.Close();
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Unrecognized
			/////////////////////////////////////////////////////////////////////////////////

			else // unrecognized
			{
				htmlFrag += "<" + c.Name;

				if (Lex.IsDefined(c.Text)) // insert the element content
					htmlFrag += " name=\"" + c.Text + '"';
				htmlFrag += " />";
			}

			html += htmlFrag;
			code += codeFrag;
			return;
		}

	}

	public class DivMx
	{
		public string Class = "font-mx defaults-mx";
		public string Position = "absolute";
		public string Display = "flex";
		public string AlignItems = "center";

		public string Width = "";
		public string Height = "";

		public string Top = "";
		public string Bottom = "";
		public string Left = "";
		public string Right = "";

		public string Border = "1px solid yellow";

		/// <summary>
		/// Build a Div to wrap the specified control
		/// </summary>
		/// <param name="pc">Parent control</param>
		/// <param name="c">Control being wrapped in the Div</param>
		/// <returns></returns>

		public string Build(
			Control pc,
			Control c,
			int dy,
			string styleAttributesToRemove = null,
			string styleAttributesToAdd = null)
		{

			if (typeof(Form).IsAssignableFrom(pc.GetType())) // Form or XtraForm?
				dy += 32; // add pixel height of WinForms window header to div top/bottom to adjust for move to HTML

			int cTop = c.Top + dy; // move top and bottom down to correct from WinForms to HTML
			int cBottom = c.Bottom + dy;

			string div = "<div class='font-mx defaults-mx' style='position:absolute; display:flex; align-items:center; ";

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

			div += "border: 1px solid yellow;'>\r\n"; // finish up style and div tag

			if (Lex.IsDefined(styleAttributesToRemove))
			{
				div = Lex.Replace(div, styleAttributesToRemove, "");
			}

			if (Lex.IsDefined(styleAttributesToAdd))
			{
				div = Lex.Replace(div, "style='", "style='" + styleAttributesToAdd + " ");
			}

			return div;
		}

		/// <summary>
		/// Return string to close the div
		/// </summary>
		/// <returns></returns>

		public string Close()
		{
			return "</div>\r\n"; 
		}

	}
}
