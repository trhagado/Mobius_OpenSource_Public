using Mobius.ComOps;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraRichEdit;
using DevExpress.Utils;

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
	public partial class EditHtmlBasic : XtraForm
	{

		/// <summary>
		/// Edit HTML using basic set of tags supported by the HyperLinkLabelControl ("HTML Text Formatting")
		/// </summary>

		public EditHtmlBasic()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Edit HTML using basic set of tags supported by the HyperLinkLabelControl ("HTML Text Formatting")
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>

		public static string ShowDialog(
			string html)
		{
			EditHtmlBasic form = new EditHtmlBasic();

			form.HtmlTextCtl.Text = html;
			form.SetRenderedHtmlView();

			DialogResult dr = form.ShowDialog(SessionManager.ActiveForm);

			string html2 = AdjustToHtmlTextFormattingSupportedTags(form.HtmlTextCtl.Text);
			if (dr == DialogResult.OK) return html2;
			else return null;
		}

		private void HtmlTextCtl_EditValueChanged(object sender, EventArgs e)
		{
			SetRenderedHtmlView();
		}

		void SetRenderedHtmlView()
		{

			PreviewControl.Text = AdjustToHtmlTextFormattingSupportedTags(HtmlTextCtl.Text);
		}

		static string AdjustToHtmlTextFormattingSupportedTags(string html)
		{
			html = Lex.Replace(html, "<a href", "<href");
			html = Lex.Replace(html, "</a>", "</href>");
			return html;
		}

		private void EditHtmlTextFormatting_Activated(object sender, EventArgs e)
		{
			if (HtmlTextCtl.Text == " ") HtmlTextCtl.Text = "";
			HtmlTextCtl.Focus();
		}

		private void PreviewControl_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
		{
			return;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
