using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Mobius.ComOps;

using DevExpress.XtraEditors;

namespace Mobius.Data
{

	public partial class ViewTypeBitmaps : XtraForm
	{
		/// <summary>
		/// Get instance of bitmap collections
		/// </summary>

		public static ViewTypeBitmaps I
		{
			get
			{
				if (Instance == null) Instance = new ViewTypeBitmaps();
				return Instance;
				
			}
		}

		static ViewTypeBitmaps Instance; // single instance

		public ViewTypeBitmaps()
		{
			InitializeComponent();
		}

	}

}
