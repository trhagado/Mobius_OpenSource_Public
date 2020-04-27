namespace Mobius.SpotfireClient
{
	partial class SpotfirePageControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpotfirePageControl));
			DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem1 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList();
			this.ToolPanel = new DevExpress.XtraEditors.PanelControl();
			this.separatorControl5 = new DevExpress.XtraEditors.SeparatorControl();
			this.EditQueryBut = new DevExpress.XtraEditors.SimpleButton();
			this.OpenDocMenu = new System.Windows.Forms.ContextMenuStrip();
			this.OpenFromFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenFromLibraryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SpotfireToolbar = new Mobius.SpotfireClient.SpotfireToolbar();
			this.SpotfirePanel = new Mobius.SpotfireClient.SpotfirePanel();
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).BeginInit();
			this.ToolPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.separatorControl5)).BeginInit();
			this.OpenDocMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Properties.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "EditQuery.bmp");
			// 
			// ToolPanel
			// 
			this.ToolPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ToolPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ToolPanel.Controls.Add(this.SpotfireToolbar);
			this.ToolPanel.Controls.Add(this.separatorControl5);
			this.ToolPanel.Controls.Add(this.EditQueryBut);
			this.ToolPanel.Location = new System.Drawing.Point(446, 3);
			this.ToolPanel.Name = "ToolPanel";
			this.ToolPanel.Size = new System.Drawing.Size(318, 24);
			this.ToolPanel.TabIndex = 228;
			// 
			// separatorControl5
			// 
			this.separatorControl5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.separatorControl5.LineColor = System.Drawing.Color.Gray;
			this.separatorControl5.LineOrientation = System.Windows.Forms.Orientation.Vertical;
			this.separatorControl5.LineThickness = 1;
			this.separatorControl5.Location = new System.Drawing.Point(224, 1);
			this.separatorControl5.Name = "separatorControl5";
			this.separatorControl5.Padding = new System.Windows.Forms.Padding(0);
			this.separatorControl5.Size = new System.Drawing.Size(8, 22);
			this.separatorControl5.TabIndex = 229;
			// 
			// EditQueryBut
			// 
			this.EditQueryBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditQueryBut.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.EditQueryBut.ImageOptions.ImageIndex = 1;
			this.EditQueryBut.ImageOptions.ImageList = this.Bitmaps16x16;
			this.EditQueryBut.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.EditQueryBut.Location = new System.Drawing.Point(233, 1);
			this.EditQueryBut.LookAndFeel.SkinName = "Money Twins";
			this.EditQueryBut.Name = "EditQueryBut";
			this.EditQueryBut.Size = new System.Drawing.Size(83, 22);
			toolTipTitleItem1.Text = "Edit Query";
			toolTipItem1.LeftIndent = 6;
			toolTipItem1.Text = "Return to editing of the current query.";
			superToolTip1.Items.Add(toolTipTitleItem1);
			superToolTip1.Items.Add(toolTipItem1);
			this.EditQueryBut.SuperTip = superToolTip1;
			this.EditQueryBut.TabIndex = 205;
			this.EditQueryBut.Text = "Edit Query";
			this.EditQueryBut.Click += new System.EventHandler(this.EditQueryBut_Click);
			// 
			// OpenDocMenu
			// 
			this.OpenDocMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenFromFileMenuItem,
            this.OpenFromLibraryMenuItem});
			this.OpenDocMenu.Name = "OpenDocMenu";
			this.OpenDocMenu.Size = new System.Drawing.Size(184, 48);
			// 
			// OpenFromFileMenuItem
			// 
			this.OpenFromFileMenuItem.Name = "OpenFromFileMenuItem";
			this.OpenFromFileMenuItem.Size = new System.Drawing.Size(183, 22);
			this.OpenFromFileMenuItem.Text = "Open From File...";
			this.OpenFromFileMenuItem.Click += new System.EventHandler(this.OpenFromFileMenuItem_Click);
			// 
			// OpenFromLibraryMenuItem
			// 
			this.OpenFromLibraryMenuItem.Name = "OpenFromLibraryMenuItem";
			this.OpenFromLibraryMenuItem.Size = new System.Drawing.Size(183, 22);
			this.OpenFromLibraryMenuItem.Text = "Open  from Library...";
			this.OpenFromLibraryMenuItem.Click += new System.EventHandler(this.OpenFromLibraryMenuItem_Click);
			// 
			// SpotfireToolbar
			// 
			this.SpotfireToolbar.Location = new System.Drawing.Point(4, 3);
			this.SpotfireToolbar.Name = "SpotfireToolbar";
			this.SpotfireToolbar.Size = new System.Drawing.Size(220, 20);
			this.SpotfireToolbar.TabIndex = 230;
			// 
			// SpotfirePanel
			// 
			this.SpotfirePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SpotfirePanel.Location = new System.Drawing.Point(0, 34);
			this.SpotfirePanel.Name = "SpotfirePanel";
			this.SpotfirePanel.Size = new System.Drawing.Size(768, 514);
			this.SpotfirePanel.TabIndex = 227;
			this.SpotfirePanel.Resize += new System.EventHandler(this.SpotfirePanel_Resize);
			// 
			// SpotfirePageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ToolPanel);
			this.Controls.Add(this.SpotfirePanel);
			this.Name = "SpotfirePageControl";
			this.Size = new System.Drawing.Size(768, 548);
			this.Resize += new System.EventHandler(this.SpotfirePageControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).EndInit();
			this.ToolPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.separatorControl5)).EndInit();
			this.OpenDocMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ImageList Bitmaps16x16;
		internal SpotfirePanel SpotfirePanel;
		internal DevExpress.XtraEditors.PanelControl ToolPanel;
		private System.Windows.Forms.ContextMenuStrip OpenDocMenu;
		private System.Windows.Forms.ToolStripMenuItem OpenFromFileMenuItem;
		private System.Windows.Forms.ToolStripMenuItem OpenFromLibraryMenuItem;
		private DevExpress.XtraEditors.SeparatorControl separatorControl5;
		public SpotfireClient.SpotfireToolbar SpotfireToolbar;
		internal DevExpress.XtraEditors.SimpleButton EditQueryBut;
	}
}
