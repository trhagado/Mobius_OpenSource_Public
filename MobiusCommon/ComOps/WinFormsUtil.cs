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
	public delegate void ControlMxConverterDelegate(
			Control pc,
			bool includeMenuStubs);

	public class WinFormsUtil
	{

		static HashSet<string> ControlsLogged = new HashSet<string>();

		public static ControlMxConverterDelegate ControlMxConverter;

		/// <summary>
		/// Log the child controls of the current control
		/// </summary>
		/// <param name="pc"></param>

		public static void LogControlChildren(Control pc)
		{
			string s;

			if (DebugMx.True && ControlMxConverter != null)
			{
				ControlMxConverter(pc, true);
				return;
			}

			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			StreamWriter sw = new StreamWriter(@"C:\MobiusJupyter\GeneratedGuiComponents\" + ctlFullName + ".txt");

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

	}
}
