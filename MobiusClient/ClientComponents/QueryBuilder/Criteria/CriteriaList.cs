using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	public partial class CriteriaList : XtraForm
	{
		static CriteriaList Instance;

		public CriteriaList()
		{
			InitializeComponent();
		}

		public static string Edit(
			string listText,
			string title)
		{
			if (Instance == null) Instance = new CriteriaList();

			Instance.Text = title;
			Instance.ValueList.Text = listText;

			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;

			listText = Instance.ValueList.Text;
			string[] sArray = listText.Split('\n');
			List<string> list = new List<string>();
			foreach (string s in sArray)
			{
				string s2 = s;
				if (s.IndexOf("\r") >= 0) s2 = s2.Replace("\r", "");
				s2 = s2.Trim();
				if (s2 == "") continue;
				list.Add(s2);
			}

			listText = Csv.JoinCsvString(list);
			return listText;
		}
	}
}
