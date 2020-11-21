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

			string template = @"
@using Syncfusion.Blazor.Navigations
@using Syncfusion.Blazor.Buttons
@using Syncfusion.Blazor.SplitButtons
@using Syncfusion.Blazor.DropDowns
@using Syncfusion.Blazor.Lists
@using Newtonsoft.Json

@using System.Reflection
@using Microsoft.AspNetCore.Components

@using WebShell
@using WebShell.Shared

@inject Microsoft.AspNetCore.Components.NavigationManager UriHelper
@inject IJSRuntime JsRuntime;

@********@
@* HTML *@
@********@

<html>

@********@
@*Code *@
@********@

@*******@
@*CSS *@
@*******@
 ";


			//////////////////////////////////////////////
			// Build the HTML and plug into the template
			//////////////////////////////////////////////

			string html = ""; // html built here
			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);


			StreamWriter sw = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + ctlFullName + ".txt");

			StreamWriter sw2 = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + ctlFullName + ".razor");


			ConvertContainerControl(pc, sw, "", "", "", ref html);

			sw.Close();

			string txt = Lex.Replace(template, "<html>", html);
			sw2.Write(txt);
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
		/// <param name="html"></param>

		static void ConvertContainerControl(
			Control pc,
			StreamWriter sw,
			string customDivStyling,
			string header,
			string footer,
			ref string html)
		{
			Type pcType = pc.GetType();

			sw.WriteLine("Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);
			sw.WriteLine("");

			string format = "{0,-26}\t{1,-26}\t{2,5}\t{3,5}\t{4,5}\t{5,6}\t{6,-26}\t{7,-6}\t{8,-14}\t{9,-32}";
			sw.WriteLine(string.Format(format, "Type", "Name", "Left", "Top", "Width", "Height", "Anchor", "Dock", "Text", "Full Type Name"));
			sw.WriteLine(string.Format(format, "--------------------------", "--------------------------", "----", "----", "-----", "------", "--------------------------", "----", "--------------", "--------------------------"));

			html += // wrap control in div with class name matching the Winforms/Dx control class name
				"<div class=\"" + pcType.Name + "\" style=\"position: relative; width: 100%; height: 100%; border: 1px solid orange; " +
				customDivStyling + " \">\r\n";

			html += header;

			foreach (Control c in pc.Controls)
			{
				//if (!cc.Visible) continue;

				Type t = c.GetType();

				// Log basic info

				sw.WriteLine(string.Format(format, t.Name, c.Name,
					c.Left, c.Top, c.Width, c.Height,
					c.Anchor, c.Dock, c.Text, t.FullName));

				string div = "<div style =\"position: absolute; ";

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

					bool top = ((c.Anchor & AnchorStyles.Top) != 0);
					bool bottom = ((c.Anchor & AnchorStyles.Bottom) != 0);

					// Note: for buttons it looks like they could move up one px & be 2px taller

					if (top)
					{
						div += "top: " + c.Top + "px; ";
						if (!bottom)
							widthHeight += "height: " + c.Height + "px; ";

						else // if top & bottom defined then set height as a calc()
							widthHeight += "height: calc(100% - " + (c.Top + (pc.Height - c.Bottom)) + "px); ";
					}

					else if (bottom) // set bottom and height
					{
						div += "bottom: " + (pc.Height - c.Bottom) + "px; ";
						widthHeight += "height: " + c.Height + "px; ";
					}

					div += widthHeight; // put width height after location
				}

				div += "border: 1px solid yellow;\">"; // finish up style and div tag

				ConvertControl(pc, sw, ref div); // convert the control and include conversion in containing div

				div += "</div>\r\n"; // close the div

				html += div; // add to html

			} // child control list

			html += footer;
			return;
		}

		static void ConvertControl(
			Control c,
			StreamWriter sw,
			ref string html)
		{
			string header = "", footer = "", txt;

			Type cType = c.GetType();

			////////////////////////////////////////////////////////////////////////////////
			// For user controls write reference to this file and then the control and 
			// its contents to a new file
			////////////////////////////////////////////////////////////////////////////////

			if (c is UserControl || c is XtraUserControl)
			{

				html += "< " + cType.Name + " />/r/n"; // add control reference (and parameters) to html

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
					"  <legend style=\"color:blue;font-weight:bold;\">" + gb.Text + "</legend>";

				footer += "</fieldset>";

				ConvertContainerControl(c, sw, "", header, footer, ref html);
			}

			else if (c is Panel || c is XtraPanel)
			{
				ConvertContainerControl(c, sw, "", header, footer, ref html);
			}

			////////////////////////////////////////////////////////////////////////////////
			// Winforms/DX builtin control. Just the control header with and property values
			////////////////////////////////////////////////////////////////////////////////

			else if (c is LabelControl)
			{
				LabelControl l = c as LabelControl;
				if (Lex.IsDefined(l.Text))
				{
					txt = "<span class='mobius-mx' @onclick='mx_click'>" + l.Text + "</span>\r\n";
					//if (l.Click.Get == null)
					txt = txt.Replace("@onclick = 'mx_click'", "");
				}
			}

			else if (c is CheckEdit)
			{
				CheckEdit ce = c as CheckEdit;
				if (ce.Properties.CheckBoxOptions.Style == CheckBoxStyle.CheckBox)
				{
					txt = "<SfCheckBox Label='Default' @bind-Checked='isChecked' CssClass='checkedit-mx'/>\r\n";
				}
				else
				{
					txt = "<SfRadioButton Label='Male' Name='Gender' Value='male' @bind-Checked='@Annotate.Gender'></SfRadioButton>";
				}
			}

			else if (c is SimpleButton)
			{
				SimpleButton b = c as SimpleButton;
				// todo...
			}

			else // unrecognized
			{
				html += "<" + c.Name;

				if (Lex.IsDefined(c.Text)) // insert the element content
					html += " name=\"" + c.Text + '"';
				html += " />";
			}

			return;
		}

	}
}
