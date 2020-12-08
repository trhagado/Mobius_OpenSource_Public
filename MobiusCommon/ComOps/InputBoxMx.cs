using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Summary description for PromptBox.
	/// </summary>
	public class InputBoxMx : XtraForm
	{
		static InputBoxMx Instance;

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		private WebBrowser HtmlPrompt;
		public ComboBoxEdit Input;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public InputBoxMx()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Instance = this;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.HtmlPrompt = new System.Windows.Forms.WebBrowser();
			this.Input = new DevExpress.XtraEditors.ComboBoxEdit();
			((System.ComponentModel.ISupportInitialize)(this.Input.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Prompt
			// 
			this.Prompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Location = new System.Drawing.Point(8, 9);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(317, 65);
			this.Prompt.TabIndex = 0;
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(193, 101);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(60, 22);
			this.OK.TabIndex = 2;
			this.OK.Text = "OK";
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(265, 101);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(60, 22);
			this.Cancel.TabIndex = 3;
			this.Cancel.Text = "Cancel";
			// 
			// HtmlPrompt
			// 
			this.HtmlPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HtmlPrompt.Location = new System.Drawing.Point(8, 9);
			this.HtmlPrompt.MinimumSize = new System.Drawing.Size(20, 20);
			this.HtmlPrompt.Name = "HtmlPrompt";
			this.HtmlPrompt.Size = new System.Drawing.Size(317, 65);
			this.HtmlPrompt.TabIndex = 5;
			// 
			// Input
			// 
			this.Input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Input.Location = new System.Drawing.Point(8, 75);
			this.Input.Name = "Input";
			this.Input.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.Input.Properties.Sorted = true;
			this.Input.Size = new System.Drawing.Size(317, 20);
			this.Input.TabIndex = 86;
			// 
			// InputBoxMx
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(331, 128);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.HtmlPrompt);
			this.Controls.Add(this.Input);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InputBoxMx";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "InputBox";
			this.Activated += new System.EventHandler(this.InputBox_Activated);
			((System.ComponentModel.ISupportInitialize)(this.Input.Properties)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

/// <summary>
/// Get input if not already defined
/// </summary>
/// <param name="prompt"></param>
/// <param name="title"></param>
/// <param name="input"></param>
/// <returns></returns>

		public static bool ShowAsNeeded(
			string prompt,
			string title,
			ref string input)
		{
			if (!Lex.IsNullOrEmpty(input)) return true;

			input = Show(prompt, title);
			if (Lex.IsNullOrEmpty(input)) return false;
			else return true;
		}

		/// <summary>
		/// Get a line of input from the user
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="title"></param>
		/// <returns></returns>

		public static string Show(
			string prompt,
			string title)
		{
			return Show(prompt, title, "", -1, -1);
		}

		public static string Show( 
			string prompt, 
			string title,
			int width,
			int height) 
		{
			return Show(prompt, title, "", width, height);
		}

		public static string Show ( 
			string prompt, 
			string title, 
			string defaultText) 
		{
			return Show(prompt, title, defaultText, -1, -1);
		}

		public static string Show(
			string prompt,
			string title,
			string defaultText,
			int width,
			int height)
		{
			return Show(prompt, title, defaultText, null, width, height);
		}

		/// <summary>
		/// Get a line of input from the user
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="title"></param>
		/// <param name="defaultText"></param>
		/// <param name="dictionary"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>New input or null if cancelled</returns>

		public static string Show ( 
			string prompt, 
			string title, 
			string defaultText,
			List<string> dictionary,
			int width,
			int height) 
		{
			string txt;
			if (Instance==null) Instance = new InputBoxMx();

			if (width <= 0) width = 337;
			if (height <= 0) height = 153;
			Instance.Width = width;
			Instance.Height = height;

			if (prompt.Contains("</") || prompt.Contains("/>") || Lex.Contains(prompt, "<br>"))
			{ // display HTML prompt
				Instance.Prompt.Visible = false;
				Instance.HtmlPrompt.Visible = true;
				string htmlFile = TempFile.GetTempFileName("html");
				StreamWriter sw = new StreamWriter(htmlFile);
				int backColor = ((Instance.BackColor.R * 256 + Instance.BackColor.G) * 256) + Instance.BackColor.B;
				string hexColor = String.Format("#{0:X}", backColor);
				sw.Write("<body " +
					" topmargin='0' leftmargin='0' marginwidth='0' marginheight='0' hspace='0' vspace='0' " +
					" style=\"font-size:8.5pt;font-family:'Tahoma';background-color:" + hexColor + "\">");
				sw.Write(prompt);
				sw.Write("</body>");
				sw.Close();
				Instance.HtmlPrompt.Navigate(htmlFile);
			}

			else // display simple label prompt
			{
				Instance.HtmlPrompt.Visible = false;
				Instance.Prompt.Visible = true;
				Instance.Prompt.Text = prompt;
			}

			new SyncfusionConverter().ToRazor(Instance);

			Instance.Text = title;
			Instance.Input.Text = defaultText;

			Instance.Input.Properties.Items.Clear();
			if (dictionary != null) 
				Instance.Input.Properties.Items.AddRange(dictionary);

			DialogResult dr = Instance.ShowDialog(Form.ActiveForm); // (SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;
			else return Instance.Input.Text;
		}

		private void InputBox_Activated(object sender, System.EventArgs e)
		{
			Input.SelectAll();
			Input.Focus();
		}

	}
}
