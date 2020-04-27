namespace Mobius.ClientComponents
{
	partial class SpotfireLinkUI
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpotfireLinkUI));
			this.DescriptionLabel = new DevExpress.XtraEditors.LabelControl();
			this.Description = new DevExpress.XtraEditors.MemoEdit();
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.SaveAndClose = new DevExpress.XtraEditors.DropDownButton();
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.SaveAndCloseContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.LibraryPath = new DevExpress.XtraEditors.TextEdit();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.Test = new DevExpress.XtraEditors.SimpleButton();
			this.CriteriaCols = new Mobius.ClientComponents.QueryTableControl();
			this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.Description.Properties)).BeginInit();
			this.SaveAndCloseContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LibraryPath.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DescriptionLabel.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DescriptionLabel.Location = new System.Drawing.Point(12, 369);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = new System.Drawing.Size(57, 13);
			this.DescriptionLabel.TabIndex = 195;
			this.DescriptionLabel.Text = "Description:";
			// 
			// Description
			// 
			this.Description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Description.EditValue = "";
			this.Description.Location = new System.Drawing.Point(11, 388);
			this.Description.Name = "Description";
			this.Description.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Description.Properties.Appearance.Options.UseFont = true;
			this.Description.Size = new System.Drawing.Size(550, 61);
			this.Description.TabIndex = 196;
			this.Description.UseOptimizedRendering = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(500, 465);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(60, 23);
			this.CloseButton.TabIndex = 203;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SaveAndClose
			// 
			this.SaveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndClose.Location = new System.Drawing.Point(400, 465);
			this.SaveAndClose.Name = "SaveAndClose";
			this.SaveAndClose.Size = new System.Drawing.Size(95, 23);
			this.SaveAndClose.TabIndex = 204;
			this.SaveAndClose.Text = "Save && Close";
			this.SaveAndClose.ShowDropDownControl += new DevExpress.XtraEditors.ShowDropDownControlEventHandler(this.SaveAndClose_ShowDropDownControl);
			this.SaveAndClose.Click += new System.EventHandler(this.SaveAndClose_Click);
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "");
			this.Bitmaps8x8.Images.SetKeyName(1, "");
			this.Bitmaps8x8.Images.SetKeyName(2, "ScrollDown.bmp");
			this.Bitmaps8x8.Images.SetKeyName(3, "ScrollUp.bmp");
			this.Bitmaps8x8.Images.SetKeyName(4, "DropDown8x8.bmp");
			// 
			// SaveAndCloseContextMenu
			// 
			this.SaveAndCloseContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveMenuItem,
            this.SaveAsMenuItem});
			this.SaveAndCloseContextMenu.Name = "StandardCalcContextMenu";
			this.SaveAndCloseContextMenu.Size = new System.Drawing.Size(121, 48);
			// 
			// SaveMenuItem
			// 
			this.SaveMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Save;
			this.SaveMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SaveMenuItem.Name = "SaveMenuItem";
			this.SaveMenuItem.Size = new System.Drawing.Size(120, 22);
			this.SaveMenuItem.Text = "Save";
			this.SaveMenuItem.Click += new System.EventHandler(this.SaveMenuItem_Click);
			// 
			// SaveAsMenuItem
			// 
			this.SaveAsMenuItem.Name = "SaveAsMenuItem";
			this.SaveAsMenuItem.Size = new System.Drawing.Size(120, 22);
			this.SaveAsMenuItem.Text = "SaveAs...";
			this.SaveAsMenuItem.Click += new System.EventHandler(this.SaveAsMenuItem_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Location = new System.Drawing.Point(12, 18);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(256, 13);
			this.labelControl1.TabIndex = 206;
			this.labelControl1.Text = "Location of the saved analysis in the Spotfire Library:";
			// 
			// LibraryPath
			// 
			this.LibraryPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LibraryPath.Location = new System.Drawing.Point(12, 35);
			this.LibraryPath.Name = "LibraryPath";
			this.LibraryPath.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LibraryPath.Properties.Appearance.Options.UseFont = true;
			this.LibraryPath.Size = new System.Drawing.Size(550, 20);
			this.LibraryPath.TabIndex = 0;
			this.LibraryPath.Enter += new System.EventHandler(this.LibraryPath_Enter);
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(0, 452);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(571, 10);
			this.labelControl2.TabIndex = 213;
			// 
			// Test
			// 
			this.Test.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Test.Location = new System.Drawing.Point(11, 465);
			this.Test.Name = "Test";
			this.Test.Size = new System.Drawing.Size(68, 23);
			this.Test.TabIndex = 218;
			this.Test.Text = "Test Link...";
			this.Test.Click += new System.EventHandler(this.Test_Click);
			// 
			// CriteriaCols
			// 
			this.CriteriaCols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CriteriaCols.Location = new System.Drawing.Point(12, 70);
			this.CriteriaCols.Name = "CriteriaCols";
			this.CriteriaCols.QueryTable = null;
			this.CriteriaCols.SelectOnly = true;
			this.CriteriaCols.SelectSingle = false;
			this.CriteriaCols.Size = new System.Drawing.Size(547, 282);
			this.CriteriaCols.TabIndex = 220;
			// 
			// labelControl5
			// 
			this.labelControl5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl5.LineVisible = true;
			this.labelControl5.Location = new System.Drawing.Point(12, 74);
			this.labelControl5.Name = "labelControl5";
			this.labelControl5.Size = new System.Drawing.Size(547, 17);
			this.labelControl5.TabIndex = 221;
			this.labelControl5.Text = "Spotfire Filter Criteria columns to display when building queries";
			// 
			// SpotfireLinkUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(571, 492);
			this.Controls.Add(this.labelControl5);
			this.Controls.Add(this.CriteriaCols);
			this.Controls.Add(this.Test);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.LibraryPath);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveAndClose);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.Description);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SpotfireLinkUI";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Spotfire Link";
			((System.ComponentModel.ISupportInitialize)(this.Description.Properties)).EndInit();
			this.SaveAndCloseContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LibraryPath.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl DescriptionLabel;
		public DevExpress.XtraEditors.MemoEdit Description;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		private DevExpress.XtraEditors.DropDownButton SaveAndClose;
		public System.Windows.Forms.ImageList Bitmaps8x8;
		public System.Windows.Forms.ContextMenuStrip SaveAndCloseContextMenu;
		public System.Windows.Forms.ToolStripMenuItem SaveMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SaveAsMenuItem;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.TextEdit LibraryPath;
		public DevExpress.XtraEditors.LabelControl labelControl2;
		public DevExpress.XtraEditors.SimpleButton Test;
		private QueryTableControl CriteriaCols;
		public DevExpress.XtraEditors.LabelControl labelControl5;
	}
}