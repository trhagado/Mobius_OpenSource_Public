namespace Mobius.KekuleJs
{
	partial class KekuleTestForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Browser = new System.Windows.Forms.WebBrowser();
			this.SuspendLayout();
			// 
			// Browser
			// 
			this.Browser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Browser.Location = new System.Drawing.Point(0, 0);
			this.Browser.MinimumSize = new System.Drawing.Size(20, 20);
			this.Browser.Name = "Browser";
			this.Browser.Size = new System.Drawing.Size(1288, 857);
			this.Browser.TabIndex = 0;
			// 
			// KekuleTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1288, 857);
			this.Controls.Add(this.Browser);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "KekuleTestForm";
			this.Text = "KekuleTestForm";
			this.Shown += new System.EventHandler(this.KekuleTestForm_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser Browser;
	}
}