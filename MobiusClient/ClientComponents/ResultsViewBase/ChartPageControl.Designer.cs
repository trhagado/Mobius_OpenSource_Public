namespace Mobius.ClientComponents
{
	partial class ChartPageControl
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartPageControl));
			this.MarkDataBut = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.PrintBut = new DevExpress.XtraEditors.SimpleButton();
			this.EditQueryBut = new DevExpress.XtraEditors.SimpleButton();
			this.ChartPropertiesButton = new DevExpress.XtraEditors.SimpleButton();
			this.ToolPanel = new DevExpress.XtraEditors.PanelControl();
			this.MouseModeButton = new DevExpress.XtraEditors.SimpleButton();
			this.MouseModeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MouseSelectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MouseTranslateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ChartPagePanel = new Mobius.ClientComponents.ChartPagePanel();
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).BeginInit();
			this.ToolPanel.SuspendLayout();
			this.MouseModeContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// MarkDataBut
			// 
			this.MarkDataBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MarkDataBut.ImageIndex = 8;
			this.MarkDataBut.ImageList = this.Bitmaps16x16;
			this.MarkDataBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.MarkDataBut.Location = new System.Drawing.Point(54, 1);
			this.MarkDataBut.LookAndFeel.SkinName = "Money Twins";
			this.MarkDataBut.Name = "MarkDataBut";
			this.MarkDataBut.Size = new System.Drawing.Size(22, 22);
			this.MarkDataBut.TabIndex = 199;
			this.MarkDataBut.ToolTip = "Marked data rows manipulation";
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Properties.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "MouseSelect.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "MouseRotate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "MouseTranslate2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "MouseZoom2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "ResetChartView.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "ChartAdd.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "PageLayout.bmp");
			this.Bitmaps16x16.Images.SetKeyName(8, "CheckOn.bmp");
			this.Bitmaps16x16.Images.SetKeyName(9, "Print.bmp");
			this.Bitmaps16x16.Images.SetKeyName(10, "EditQuery.bmp");
			this.Bitmaps16x16.Images.SetKeyName(11, "hand.bmp");
			// 
			// PrintBut
			// 
			this.PrintBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintBut.ImageIndex = 9;
			this.PrintBut.ImageList = this.Bitmaps16x16;
			this.PrintBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.PrintBut.Location = new System.Drawing.Point(80, 1);
			this.PrintBut.LookAndFeel.SkinName = "Money Twins";
			this.PrintBut.Name = "PrintBut";
			this.PrintBut.Size = new System.Drawing.Size(22, 22);
			this.PrintBut.TabIndex = 200;
			this.PrintBut.ToolTip = "Print";
			this.PrintBut.Click += new System.EventHandler(this.PrintBut_Click);
			// 
			// EditQueryBut
			// 
			this.EditQueryBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditQueryBut.ImageIndex = 10;
			this.EditQueryBut.ImageList = this.Bitmaps16x16;
			this.EditQueryBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.EditQueryBut.Location = new System.Drawing.Point(106, 1);
			this.EditQueryBut.LookAndFeel.SkinName = "Money Twins";
			this.EditQueryBut.Name = "EditQueryBut";
			this.EditQueryBut.Size = new System.Drawing.Size(83, 22);
			this.EditQueryBut.TabIndex = 201;
			this.EditQueryBut.Text = "Edit Query";
			this.EditQueryBut.ToolTip = "Return to editing the current query";
			this.EditQueryBut.Click += new System.EventHandler(this.EditQueryBut_Click);
			// 
			// ChartPropertiesButton
			// 
			this.ChartPropertiesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChartPropertiesButton.ImageIndex = 0;
			this.ChartPropertiesButton.ImageList = this.Bitmaps16x16;
			this.ChartPropertiesButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.ChartPropertiesButton.Location = new System.Drawing.Point(28, 1);
			this.ChartPropertiesButton.LookAndFeel.SkinName = "Money Twins";
			this.ChartPropertiesButton.Name = "ChartPropertiesButton";
			this.ChartPropertiesButton.Size = new System.Drawing.Size(22, 22);
			this.ChartPropertiesButton.TabIndex = 209;
			this.ChartPropertiesButton.ToolTip = "Chart properties";
			this.ChartPropertiesButton.Click += new System.EventHandler(this.ChartPropertiesButton_Click);
			// 
			// ToolPanel
			// 
			this.ToolPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ToolPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ToolPanel.Controls.Add(this.MouseModeButton);
			this.ToolPanel.Controls.Add(this.EditQueryBut);
			this.ToolPanel.Controls.Add(this.ChartPropertiesButton);
			this.ToolPanel.Controls.Add(this.MarkDataBut);
			this.ToolPanel.Controls.Add(this.PrintBut);
			this.ToolPanel.Location = new System.Drawing.Point(595, 2);
			this.ToolPanel.Name = "ToolPanel";
			this.ToolPanel.Size = new System.Drawing.Size(190, 24);
			this.ToolPanel.TabIndex = 221;
			// 
			// MouseModeButton
			// 
			this.MouseModeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MouseModeButton.ImageIndex = 1;
			this.MouseModeButton.ImageList = this.Bitmaps16x16;
			this.MouseModeButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.MouseModeButton.Location = new System.Drawing.Point(2, 1);
			this.MouseModeButton.LookAndFeel.SkinName = "Money Twins";
			this.MouseModeButton.Name = "MouseModeButton";
			this.MouseModeButton.Size = new System.Drawing.Size(22, 22);
			this.MouseModeButton.TabIndex = 222;
			this.MouseModeButton.ToolTip = "Mouse selection/movement mode";
			this.MouseModeButton.Click += new System.EventHandler(this.MouseModeButton_Click);
			// 
			// MouseModeContextMenu
			// 
			this.MouseModeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MouseSelectMenuItem,
            this.MouseTranslateMenuItem});
			this.MouseModeContextMenu.Name = "DataFieldColRtClickContextMenu";
			this.MouseModeContextMenu.Size = new System.Drawing.Size(160, 48);
			this.MouseModeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.MouseModeContextMenu_Opening);
			// 
			// MouseSelectMenuItem
			// 
			this.MouseSelectMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MouseSelectMenuItem.Image")));
			this.MouseSelectMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MouseSelectMenuItem.Name = "MouseSelectMenuItem";
			this.MouseSelectMenuItem.Size = new System.Drawing.Size(159, 22);
			this.MouseSelectMenuItem.Text = "Select";
			this.MouseSelectMenuItem.ToolTipText = resources.GetString("MouseSelectMenuItem.ToolTipText");
			this.MouseSelectMenuItem.Click += new System.EventHandler(this.MouseSelectMenuItem_Click);
			// 
			// MouseTranslateMenuItem
			// 
			this.MouseTranslateMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MouseTranslateMenuItem.Image")));
			this.MouseTranslateMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MouseTranslateMenuItem.Name = "MouseTranslateMenuItem";
			this.MouseTranslateMenuItem.Size = new System.Drawing.Size(159, 22);
			this.MouseTranslateMenuItem.Text = "Scroll the Chart";
			this.MouseTranslateMenuItem.ToolTipText = resources.GetString("MouseTranslateMenuItem.ToolTipText");
			this.MouseTranslateMenuItem.Click += new System.EventHandler(this.MouseTranslateMenuItem_Click);
			// 
			// ChartPagePanel
			// 
			this.ChartPagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ChartPagePanel.Location = new System.Drawing.Point(-2, 29);
			this.ChartPagePanel.Name = "ChartPagePanel";
			this.ChartPagePanel.Size = new System.Drawing.Size(785, 544);
			this.ChartPagePanel.TabIndex = 198;
			// 
			// ChartPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ToolPanel);
			this.Controls.Add(this.ChartPagePanel);
			this.Name = "ChartPageControl";
			this.Size = new System.Drawing.Size(788, 573);
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).EndInit();
			this.ToolPanel.ResumeLayout(false);
			this.MouseModeContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.SimpleButton MarkDataBut;
		private DevExpress.XtraEditors.SimpleButton PrintBut;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		private DevExpress.XtraEditors.SimpleButton ChartPropertiesButton;
		internal ChartPagePanel ChartPagePanel = null;
		internal DevExpress.XtraEditors.PanelControl ToolPanel;
		private DevExpress.XtraEditors.SimpleButton MouseModeButton;
		public System.Windows.Forms.ContextMenuStrip MouseModeContextMenu;
		private System.Windows.Forms.ToolStripMenuItem MouseSelectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MouseTranslateMenuItem;
		internal DevExpress.XtraEditors.SimpleButton EditQueryBut;
	}
}
