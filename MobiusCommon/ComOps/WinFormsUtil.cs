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
					div += "width: 100%; height: 100 %; ";

				else // use Anchor settings
				{
					if ((c.Anchor & AnchorStyles.Left) != 0)
						div += "left: " + c.Left + "px; ";

					if ((c.Anchor & AnchorStyles.Right) != 0)
						div += "right: " + c.Right + "px; ";

					if ((c.Anchor & AnchorStyles.Top) != 0)
						div += "top: " + c.Top + "px; ";

					if ((c.Anchor & AnchorStyles.Bottom) != 0)
						div += "bottom: " + c.Bottom + "px; ";

				}

				div += "border: 3px solid #73AD21;\">"; // finish up style and div tag

				// insert the element

				div += "</div>\r\n"; // close the div

				html += div; // add to html
			}

			sw.Close();

			string txt = Lex.Replace(template, "<html>", html);
			sw2.Write(txt);
			sw2.Close();
			return;
		}


	}
}
