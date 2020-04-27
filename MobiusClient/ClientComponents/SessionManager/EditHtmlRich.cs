using Mobius.ComOps;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraRichEdit;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

	/// <summary>
	/// Edit HTML using "rich" set of tags supported by the RichEditControl
	/// </summary>

	public partial class EditHtmlRich : XtraForm
	{

		public EditHtmlRich()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Edit HTML using "rich" set of tags supported by the RichEditControl
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>

		public static string ShowDialog(
			string html)
		{
			EditHtmlRich form = new EditHtmlRich();

			form.HtmlTextCtl.Text = html;
			form.SetRenderedHtmlView();

			DialogResult dr = form.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK) return form.HtmlTextCtl.Text;
			else return null;
		}

		private void HtmlTextCtl_EditValueChanged(object sender, EventArgs e)
		{
			SetRenderedHtmlView();
		}

		void SetRenderedHtmlView()
		{
			Stream stream = XmlUtil.StringToMemoryStream(HtmlTextCtl.Text);
			RichEditControl.LoadDocument(stream, DocumentFormat.Html);
		}

		private void EditHtml_Activated(object sender, EventArgs e)
		{
			if (HtmlTextCtl.Text == " ") HtmlTextCtl.Text = "";
			HtmlTextCtl.Focus();
		}

		private void OK_Click(object sender, EventArgs e)
		{

		}

		private void Cancel_Click(object sender, EventArgs e)
		{

		}

		private void RichEditControl_HyperlinkClick(object sender, HyperlinkClickEventArgs e)
		{
			return;
		}
	}
}
