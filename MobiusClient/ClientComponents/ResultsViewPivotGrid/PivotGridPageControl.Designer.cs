namespace Mobius.ClientComponents
{
	partial class PivotGridPageControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PivotGridPageControl));
			DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
			DevExpress.Utils.ToolTipTitleItem toolTipTitleItem2 = new DevExpress.Utils.ToolTipTitleItem();
			DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
			this.EditQueryBut = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.PrintBut = new DevExpress.XtraEditors.SimpleButton();
			this.ToolPanel = new DevExpress.XtraEditors.PanelControl();
			this.ExportBut = new DevExpress.XtraEditors.SimpleButton();
			this.ExportContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ExportExcelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ExportCSVTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PivotGridPanel = new Mobius.ClientComponents.PivotGridPanel();
			this.ShowFieldListButton = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).BeginInit();
			this.ToolPanel.SuspendLayout();
			this.ExportContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// EditQueryBut
			// 
			this.EditQueryBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditQueryBut.ImageIndex = 10;
			this.EditQueryBut.ImageList = this.Bitmaps16x16;
			this.EditQueryBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.EditQueryBut.Location = new System.Drawing.Point(204, 1);
			this.EditQueryBut.LookAndFeel.SkinName = "Money Twins";
			this.EditQueryBut.Name = "EditQueryBut";
			this.EditQueryBut.Size = new System.Drawing.Size(83, 22);
			this.EditQueryBut.TabIndex = 216;
			this.EditQueryBut.Text = "Edit Query";
			this.EditQueryBut.ToolTip = "Return to editing the current query";
			this.EditQueryBut.Click += new System.EventHandler(this.EditQueryBut_Click);
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
			this.Bitmaps16x16.Images.SetKeyName(7, "ViewChartWindows.bmp");
			this.Bitmaps16x16.Images.SetKeyName(8, "CheckOn.bmp");
			this.Bitmaps16x16.Images.SetKeyName(9, "Print.bmp");
			this.Bitmaps16x16.Images.SetKeyName(10, "EditQuery.bmp");
			this.Bitmaps16x16.Images.SetKeyName(11, "Export.bmp");
			this.Bitmaps16x16.Images.SetKeyName(12, "Excel.bmp");
			this.Bitmaps16x16.Images.SetKeyName(13, "PivotFields_16x16.png");
			// 
			// PrintBut
			// 
			this.PrintBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintBut.ImageIndex = 9;
			this.PrintBut.ImageList = this.Bitmaps16x16;
			this.PrintBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.PrintBut.Location = new System.Drawing.Point(114, 1);
			this.PrintBut.LookAndFeel.SkinName = "Money Twins";
			this.PrintBut.Name = "PrintBut";
			this.PrintBut.Size = new System.Drawing.Size(22, 22);
			this.PrintBut.TabIndex = 215;
			this.PrintBut.ToolTip = "Print";
			this.PrintBut.Click += new System.EventHandler(this.PrintBut_Click);
			// 
			// ToolPanel
			// 
			this.ToolPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ToolPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.ToolPanel.Controls.Add(this.ShowFieldListButton);
			this.ToolPanel.Controls.Add(this.ExportBut);
			this.ToolPanel.Controls.Add(this.EditQueryBut);
			this.ToolPanel.Controls.Add(this.PrintBut);
			this.ToolPanel.Location = new System.Drawing.Point(443, 0);
			this.ToolPanel.Name = "ToolPanel";
			this.ToolPanel.Size = new System.Drawing.Size(291, 24);
			this.ToolPanel.TabIndex = 229;
			// 
			// ExportBut
			// 
			this.ExportBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportBut.Appearance.Options.UseTextOptions = true;
			this.ExportBut.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.ExportBut.ImageIndex = 11;
			this.ExportBut.ImageList = this.Bitmaps16x16;
			this.ExportBut.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.ExportBut.Location = new System.Drawing.Point(139, 1);
			this.ExportBut.LookAndFeel.SkinName = "Money Twins";
			this.ExportBut.Name = "ExportBut";
			this.ExportBut.Size = new System.Drawing.Size(62, 22);
			toolTipTitleItem2.Text = "Export";
			toolTipItem2.LeftIndent = 6;
			toolTipItem2.Text = "Export data to Excel, text files and other formats.";
			superToolTip2.Items.Add(toolTipTitleItem2);
			superToolTip2.Items.Add(toolTipItem2);
			this.ExportBut.SuperTip = superToolTip2;
			this.ExportBut.TabIndex = 218;
			this.ExportBut.Text = "Export";
			this.ExportBut.Click += new System.EventHandler(this.ExportBut_Click);
			// 
			// ExportContextMenu
			// 
			this.ExportContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportExcelMenuItem,
            this.ExportCSVTextMenuItem});
			this.ExportContextMenu.Name = "ExportContextMenu";
			this.ExportContextMenu.Size = new System.Drawing.Size(169, 48);
			// 
			// ExportExcelMenuItem
			// 
			this.ExportExcelMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Excel2007;
			this.ExportExcelMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ExportExcelMenuItem.Name = "ExportExcelMenuItem";
			this.ExportExcelMenuItem.Size = new System.Drawing.Size(168, 22);
			this.ExportExcelMenuItem.Text = "Excel Worksheet...";
			this.ExportExcelMenuItem.Click += new System.EventHandler(this.ExportExcelMenuItem_Click);
			// 
			// ExportCSVTextMenuItem
			// 
			this.ExportCSVTextMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.NumberFormat;
			this.ExportCSVTextMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ExportCSVTextMenuItem.Name = "ExportCSVTextMenuItem";
			this.ExportCSVTextMenuItem.Size = new System.Drawing.Size(168, 22);
			this.ExportCSVTextMenuItem.Text = "CSV / Text File...";
			this.ExportCSVTextMenuItem.Click += new System.EventHandler(this.ExportTextFileMenuItem_Click);
			// 
			// PivotGridPanel
			// 
			this.PivotGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PivotGridPanel.Location = new System.Drawing.Point(3, 28);
			this.PivotGridPanel.Name = "PivotGridPanel";
			this.PivotGridPanel.Size = new System.Drawing.Size(726, 568);
			this.PivotGridPanel.TabIndex = 0;
			// 
			// ShowFieldListButton
			// 
			this.ShowFieldListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowFieldListButton.Appearance.Options.UseTextOptions = true;
			this.ShowFieldListButton.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.ShowFieldListButton.ImageIndex = 13;
			this.ShowFieldListButton.ImageList = this.Bitmaps16x16;
			this.ShowFieldListButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.ShowFieldListButton.Location = new System.Drawing.Point(3, 1);
			this.ShowFieldListButton.LookAndFeel.SkinName = "Money Twins";
			this.ShowFieldListButton.Name = "ShowFieldListButton";
			this.ShowFieldListButton.Size = new System.Drawing.Size(108, 22);
			this.ShowFieldListButton.TabIndex = 232;
			this.ShowFieldListButton.Text = "Show Field List";
			this.ShowFieldListButton.Click += new System.EventHandler(this.ShowFieldListButton_Click);
			// 
			// PivotGridPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ToolPanel);
			this.Controls.Add(this.PivotGridPanel);
			this.Name = "PivotGridPageControl";
			this.Size = new System.Drawing.Size(729, 599);
			((System.ComponentModel.ISupportInitialize)(this.ToolPanel)).EndInit();
			this.ToolPanel.ResumeLayout(false);
			this.ExportContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.SimpleButton PrintBut;
		public System.Windows.Forms.ImageList Bitmaps16x16;
		internal DevExpress.XtraEditors.PanelControl ToolPanel;
		internal PivotGridPanel PivotGridPanel;
		private DevExpress.XtraEditors.SimpleButton ExportBut;
		public System.Windows.Forms.ContextMenuStrip ExportContextMenu;
		public System.Windows.Forms.ToolStripMenuItem ExportExcelMenuItem;
		public System.Windows.Forms.ToolStripMenuItem ExportCSVTextMenuItem;
		internal DevExpress.XtraEditors.SimpleButton EditQueryBut;
		private DevExpress.XtraEditors.SimpleButton ShowFieldListButton;
	}
}
