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

namespace Mobius.ComOps
{
	public class WinFormsUtil
	{

		static HashSet<string> ControlsLogged = new HashSet<string>();

		/// <summary>
		/// Log the child controls of the current control
		/// </summary>
		/// <param name="pc"></param>

		public static void LogControlChildren(Control pc)
		{
			string s;

			if (DebugMx.True)
			{
				ToRazor(pc);
				return;
			}

			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			StreamWriter sw = new StreamWriter(@"c:\downloads\MobiusControlTemplates\" + ctlFullName + ".txt");

			sw.WriteLine("Parent Type:\t" + t.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + t.FullName);
			sw.WriteLine("");

			string format = "{0,-26}\t{1,-26}\t{2,5}\t{3,5}\t{4,5}\t{5,6}\t{6,-26}\t{7,-6}\t{8,-14}\t{9,-32}";
			sw.WriteLine(string.Format(format, "Type", "Name", "Left", "Top", "Width", "Height", "Anchor", "Dock", "Text", "Full Type Name"));
			sw.WriteLine(string.Format(format, "--------------------------", "--------------------------", "----", "----", "-----", "------", "--------------------------", "----", "--------------", "--------------------------"));

			foreach (Control c in pc.Controls)
			{
				//if (!cc.Visible) continue;

				t = c.GetType();

				sw.WriteLine(string.Format(format, t.Name, c.Name,
					c.Left, c.Top, c.Width, c.Height,
					c.Anchor, c.Dock, c.Text, t.FullName));

			}

			sw.Close();
			return;
		}

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

			sw.WriteLine("Parent Type:\t" + t.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + t.FullName);
			sw.WriteLine("");

			string format = "{0,-26}\t{1,-26}\t{2,5}\t{3,5}\t{4,5}\t{5,6}\t{6,-26}\t{7,-6}\t{8,-14}\t{9,-32}";
			sw.WriteLine(string.Format(format, "Type", "Name", "Left", "Top", "Width", "Height", "Anchor", "Dock", "Text", "Full Type Name"));
			sw.WriteLine(string.Format(format, "--------------------------", "--------------------------", "----", "----", "-----", "------", "--------------------------", "----", "--------------", "--------------------------"));

			html += // wrap control in div with class name matching the Winforms/Dx control class name
				"<div class=\"" + t.Name + "\" style=\"position: relative; width: 100%; height: 100%; border: 1px solid orange; \">\r\n";

			foreach (Control c in pc.Controls)
			{
				//if (!cc.Visible) continue;

				t = c.GetType();

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

				if (Lex.IsDefined(c.Text)) // insert the element content
					div += c.Text;
				else div += c.Name; // or just name if no text

				div += "</div>\r\n"; // close the div

				html += div; // add to html
			}

			html += "</div>\r\n"; // close the component div

			sw.Close();

			string txt = Lex.Replace(template, "<html>", html);
			sw2.Write(txt);
			sw2.Close();
			return;
		}


	}
}
