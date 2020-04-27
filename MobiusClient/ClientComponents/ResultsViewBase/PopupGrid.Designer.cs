namespace Mobius.ClientComponents
{
	partial class PopupGrid
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopupGrid));
			this.BarManager = new DevExpress.XtraBars.BarManager();
			this.StatusBar = new DevExpress.XtraBars.Bar();
			this.ZoomPctBarItem = new DevExpress.XtraBars.BarStaticItem();
			this.ZoomSlider = new DevExpress.XtraBars.BarEditItem();
			this.repositoryItemZoomTrackBar1 = new DevExpress.XtraEditors.Repository.RepositoryItemZoomTrackBar();
			this.BarAndDockingController = new DevExpress.XtraBars.BarAndDockingController();
			this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
			this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList();
			this.MoleculeGridPanel = new Mobius.ClientComponents.MoleculeGridPanel();
			this.PrintBut = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemZoomTrackBar1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BarAndDockingController)).BeginInit();
			this.SuspendLayout();
			// 
			// BarManager
			// 
			this.BarManager.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.StatusBar});
			this.BarManager.Controller = this.BarAndDockingController;
			this.BarManager.DockControls.Add(this.barDockControlTop);
			this.BarManager.DockControls.Add(this.barDockControlBottom);
			this.BarManager.DockControls.Add(this.barDockControlLeft);
			this.BarManager.DockControls.Add(this.barDockControlRight);
			this.BarManager.Form = this;
			this.BarManager.Images = this.Bitmaps16x16;
			this.BarManager.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ZoomSlider,
            this.ZoomPctBarItem});
			this.BarManager.MaxItemId = 4;
			this.BarManager.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemZoomTrackBar1});
			this.BarManager.StatusBar = this.StatusBar;
			// 
			// StatusBar
			// 
			this.StatusBar.BarName = "Status bar";
			this.StatusBar.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
			this.StatusBar.DockCol = 0;
			this.StatusBar.DockRow = 0;
			this.StatusBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
			this.StatusBar.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.ZoomPctBarItem),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.Width, this.ZoomSlider, "", false, true, true, 135)});
			this.StatusBar.OptionsBar.AllowQuickCustomization = false;
			this.StatusBar.OptionsBar.DrawDragBorder = false;
			this.StatusBar.OptionsBar.DrawSizeGrip = true;
			this.StatusBar.OptionsBar.UseWholeRow = true;
			this.StatusBar.Text = "Status bar";
			// 
			// ZoomPctBarItem
			// 
			this.ZoomPctBarItem.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
			this.ZoomPctBarItem.Border = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ZoomPctBarItem.Caption = "100%";
			this.ZoomPctBarItem.Id = 1;
			this.ZoomPctBarItem.Name = "ZoomPctBarItem";
			this.ZoomPctBarItem.TextAlignment = System.Drawing.StringAlignment.Near;
			// 
			// ZoomSlider
			// 
			this.ZoomSlider.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
			this.ZoomSlider.Edit = this.repositoryItemZoomTrackBar1;
			this.ZoomSlider.EditWidth = 135;
			this.ZoomSlider.Id = 0;
			this.ZoomSlider.ItemAppearance.Normal.BackColor = System.Drawing.Color.Transparent;
			this.ZoomSlider.ItemAppearance.Normal.Options.UseBackColor = true;
			this.ZoomSlider.Name = "ZoomSlider";
			// 
			// repositoryItemZoomTrackBar1
			// 
			this.repositoryItemZoomTrackBar1.AppearanceFocused.BackColor = System.Drawing.Color.Transparent;
			this.repositoryItemZoomTrackBar1.AppearanceFocused.Options.UseBackColor = true;
			this.repositoryItemZoomTrackBar1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.repositoryItemZoomTrackBar1.Maximum = 100;
			this.repositoryItemZoomTrackBar1.Name = "repositoryItemZoomTrackBar1";
			this.repositoryItemZoomTrackBar1.ScrollThumbStyle = DevExpress.XtraEditors.Repository.ScrollThumbStyle.ArrowDownRight;
			// 
			// BarAndDockingController
			// 
			this.BarAndDockingController.PropertiesBar.AllowLinkLighting = false;
			this.BarAndDockingController.PropertiesBar.DefaultGlyphSize = new System.Drawing.Size(16, 16);
			this.BarAndDockingController.PropertiesBar.DefaultLargeGlyphSize = new System.Drawing.Size(32, 32);
			// 
			// barDockControlTop
			// 
			this.barDockControlTop.CausesValidation = false;
			this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
			this.barDockControlTop.Size = new System.Drawing.Size(534, 0);
			// 
			// barDockControlBottom
			// 
			this.barDockControlBottom.CausesValidation = false;
			this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.barDockControlBottom.Location = new System.Drawing.Point(0, 369);
			this.barDockControlBottom.Size = new System.Drawing.Size(534, 25);
			// 
			// barDockControlLeft
			// 
			this.barDockControlLeft.CausesValidation = false;
			this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
			this.barDockControlLeft.Size = new System.Drawing.Size(0, 369);
			// 
			// barDockControlRight
			// 
			this.barDockControlRight.CausesValidation = false;
			this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.barDockControlRight.Location = new System.Drawing.Point(534, 0);
			this.barDockControlRight.Size = new System.Drawing.Size(0, 369);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Print.bmp");
			// 
			// MoleculeGridPanel
			// 
			this.MoleculeGridPanel.AutoScroll = true;
			this.MoleculeGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoleculeGridPanel.Location = new System.Drawing.Point(0, 0);
			this.MoleculeGridPanel.Name = "MoleculeGridPanel";
			this.MoleculeGridPanel.Size = new System.Drawing.Size(534, 369);
			this.MoleculeGridPanel.TabIndex = 4;
			// 
			// PrintBut
			// 
			this.PrintBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PrintBut.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.PrintBut.Appearance.Options.UseBackColor = true;
			this.PrintBut.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.PrintBut.ImageIndex = 0;
			this.PrintBut.ImageList = this.Bitmaps16x16;
			this.PrintBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.PrintBut.Location = new System.Drawing.Point(5, 373);
			this.PrintBut.LookAndFeel.SkinName = "Money Twins";
			this.PrintBut.Name = "PrintBut";
			this.PrintBut.Size = new System.Drawing.Size(20, 20);
			this.PrintBut.TabIndex = 188;
			this.PrintBut.ToolTip = "Print";
			this.PrintBut.Click += new System.EventHandler(this.PrintBut_Click);
			// 
			// PopupGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(534, 394);
			this.Controls.Add(this.PrintBut);
			this.Controls.Add(this.MoleculeGridPanel);
			this.Controls.Add(this.barDockControlLeft);
			this.Controls.Add(this.barDockControlRight);
			this.Controls.Add(this.barDockControlBottom);
			this.Controls.Add(this.barDockControlTop);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PopupGrid";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Mobius";
			((System.ComponentModel.ISupportInitialize)(this.BarManager)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repositoryItemZoomTrackBar1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BarAndDockingController)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraBars.BarManager BarManager;
		private DevExpress.XtraBars.Bar StatusBar;
		private DevExpress.XtraBars.BarEditItem ZoomSlider;
		private DevExpress.XtraEditors.Repository.RepositoryItemZoomTrackBar repositoryItemZoomTrackBar1;
		private DevExpress.XtraBars.BarDockControl barDockControlTop;
		private DevExpress.XtraBars.BarDockControl barDockControlBottom;
		private DevExpress.XtraBars.BarDockControl barDockControlLeft;
		private DevExpress.XtraBars.BarDockControl barDockControlRight;
		private DevExpress.XtraBars.BarStaticItem ZoomPctBarItem;
		internal MoleculeGridPanel MoleculeGridPanel;
		private DevExpress.XtraBars.BarAndDockingController BarAndDockingController;
		private System.Windows.Forms.ImageList Bitmaps16x16;
		public DevExpress.XtraEditors.SimpleButton PrintBut;

	}
}