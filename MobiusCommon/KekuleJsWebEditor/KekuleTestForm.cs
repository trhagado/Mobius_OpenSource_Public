using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using Microsoft.Win32

namespace Mobius.KekuleJs
{
	public partial class KekuleTestForm : Form
	{
		public KekuleTestForm()
		{
			InitializeComponent();
		}

		private void KekuleTestForm_Shown(object sender, EventArgs e)
		{
			Browser.ObjectForScripting = true;
			Browser.ScriptErrorsSuppressed = false;
			Browser.Navigate("http://localhost/MobiusWebPages/kekule/MobiusKekuleJsEditor.htm");

			//Browser.Navigate("http://localhost/MobiusWebPages/kekule/MobiusKekuleJsRenderer.htm");

		}
	}
}
