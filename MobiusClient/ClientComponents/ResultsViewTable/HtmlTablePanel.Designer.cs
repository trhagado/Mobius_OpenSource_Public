namespace Mobius.ClientComponents
{
	partial class HtmlTablePanel
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.WebBrowser = new System.Windows.Forms.WebBrowser();
			this.Panel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// WebBrowser
			// 
			this.WebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebBrowser.Location = new System.Drawing.Point(0, 0);
			this.WebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.WebBrowser.Name = "WebBrowser";
			this.WebBrowser.Size = new System.Drawing.Size(359, 311);
			this.WebBrowser.TabIndex = 6;
			this.WebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.WebBrowser_Navigated);
			this.WebBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebBrowser_Navigating);
			// 
			// Panel
			// 
			this.Panel.Location = new System.Drawing.Point(13, 3);
			this.Panel.Name = "Panel";
			this.Panel.Size = new System.Drawing.Size(60, 52);
			this.Panel.TabIndex = 7;
			// 
			// HtmlTablePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.WebBrowser);
			this.Controls.Add(this.Panel);
			this.Name = "HtmlTablePanel";
			this.Size = new System.Drawing.Size(359, 311);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.WebBrowser WebBrowser;
		private System.Windows.Forms.Panel Panel;
	}
}
