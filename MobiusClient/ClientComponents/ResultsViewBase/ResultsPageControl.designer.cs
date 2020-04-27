namespace Mobius.ClientComponents
{
	partial class ResultsPageControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResultsPageControl));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList();
			this.Tools = new DevExpress.XtraEditors.PanelControl();
			this.MarkDataBut = new DevExpress.XtraEditors.SimpleButton();
			this.ResultsPagePanel = new Mobius.ClientComponents.ResultsPagePanel();
			((System.ComponentModel.ISupportInitialize)(this.Tools)).BeginInit();
			this.Tools.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "PageLayout.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "PageLayoutStacked.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "PageLayoutSideBySide.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "PageLayoutEvenly.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "PageLayoutTabbed.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "Filter.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "DetailsOnDemand.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "CheckOn.bmp");
			// 
			// Tools
			// 
			this.Tools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.Tools.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.Tools.Controls.Add(this.MarkDataBut);
			this.Tools.Location = new System.Drawing.Point(348, 3);
			this.Tools.Name = "Tools";
			this.Tools.Size = new System.Drawing.Size(59, 24);
			this.Tools.TabIndex = 223;
			// 
			// MarkDataBut
			// 
			this.MarkDataBut.ImageOptions.ImageIndex = 7;
			this.MarkDataBut.ImageOptions.ImageList = this.Bitmaps16x16;
			this.MarkDataBut.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.MarkDataBut.Location = new System.Drawing.Point(5, 1);
			this.MarkDataBut.LookAndFeel.SkinName = "Money Twins";
			this.MarkDataBut.Name = "MarkDataBut";
			this.MarkDataBut.Size = new System.Drawing.Size(22, 22);
			this.MarkDataBut.TabIndex = 225;
			this.MarkDataBut.ToolTip = "Marked data rows manipulation";
			this.MarkDataBut.Click += new System.EventHandler(this.MarkDataBut_Click);
			// 
			// ResultsPagePanel
			// 
			this.ResultsPagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ResultsPagePanel.Location = new System.Drawing.Point(0, 29);
			this.ResultsPagePanel.Name = "ResultsPagePanel";
			this.ResultsPagePanel.Size = new System.Drawing.Size(788, 543);
			this.ResultsPagePanel.TabIndex = 224;
			this.ResultsPagePanel.Resize += new System.EventHandler(this.ResultsPagePanel_Resize);
			// 
			// ResultsPageControl
			// 
			this.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(230)))));
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ResultsPagePanel);
			this.Controls.Add(this.Tools);
			this.Name = "ResultsPageControl";
			this.Size = new System.Drawing.Size(788, 573);
			this.Resize += new System.EventHandler(this.ResultsPageControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.Tools)).EndInit();
			this.Tools.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ImageList Bitmaps16x16;
		internal DevExpress.XtraEditors.PanelControl Tools;
		internal ResultsPagePanel ResultsPagePanel;
		private DevExpress.XtraEditors.SimpleButton MarkDataBut;
	}
}
