namespace Mobius.ClientComponents
{
	partial class PopupResults
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopupResults));
			this.StatusBar = new DevExpress.XtraBars.Bar();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.QueryResultsControl = new Mobius.ClientComponents.QueryResultsControl();
			this.SuspendLayout();
			// 
			// StatusBar
			// 
			this.StatusBar.BarName = "Status bar";
			this.StatusBar.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
			this.StatusBar.DockCol = 0;
			this.StatusBar.DockRow = 0;
			this.StatusBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
			this.StatusBar.OptionsBar.AllowQuickCustomization = false;
			this.StatusBar.OptionsBar.DrawDragBorder = false;
			this.StatusBar.OptionsBar.DrawSizeGrip = true;
			this.StatusBar.OptionsBar.UseWholeRow = true;
			this.StatusBar.Text = "Status bar";
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Print.bmp");
			// 
			// QueryResultsControl
			// 
			this.QueryResultsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryResultsControl.Location = new System.Drawing.Point(0, 0);
			this.QueryResultsControl.Margin = new System.Windows.Forms.Padding(0);
			this.QueryResultsControl.Name = "QueryResultsControl";
			this.QueryResultsControl.Size = new System.Drawing.Size(721, 504);
			this.QueryResultsControl.TabIndex = 4;
			// 
			// PopupResults
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(721, 504);
			this.Controls.Add(this.QueryResultsControl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PopupResults";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Mobius";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PopupResults_FormClosed);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraBars.Bar StatusBar;
		private System.Windows.Forms.ImageList Bitmaps16x16;
		public QueryResultsControl QueryResultsControl;
	}
}