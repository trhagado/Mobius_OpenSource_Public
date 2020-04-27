using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Services.Native
{
	public partial class NativeSessionForm : Form
	{
		public static NativeSessionForm Instance;

		public NativeSessionForm()
		{
			InitializeComponent();

			if (Instance != null)
				Instance = this;
		}
	}
}
