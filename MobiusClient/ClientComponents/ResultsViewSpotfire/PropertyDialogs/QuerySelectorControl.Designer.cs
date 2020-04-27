namespace Mobius.SpotfireClient
{
	partial class QuerySelectorControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuerySelectorControl));
			this.SourceQueryComboBox = new DevExpress.XtraEditors.ComboBoxEdit();
			this.SourceQueryMenu = new System.Windows.Forms.ContextMenuStrip();
			this.CurrentQueryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MruListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FavoritesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SelectFromContentsTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList();
			((System.ComponentModel.ISupportInitialize)(this.SourceQueryComboBox.Properties)).BeginInit();
			this.SourceQueryMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// SourceQueryComboBox
			// 
			this.SourceQueryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceQueryComboBox.Location = new System.Drawing.Point(0, 0);
			this.SourceQueryComboBox.Name = "SourceQueryComboBox";
			this.SourceQueryComboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.SourceQueryComboBox.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.SourceQueryComboBox.Size = new System.Drawing.Size(276, 20);
			this.SourceQueryComboBox.TabIndex = 220;
			this.SourceQueryComboBox.EditValueChanged += new System.EventHandler(this.SourceQueryComboBox_EditValueChanged);
			this.SourceQueryComboBox.Click += new System.EventHandler(this.SourceQueryComboBox_Click);
			// 
			// SourceQueryMenu
			// 
			this.SourceQueryMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CurrentQueryMenuItem,
            this.MruListMenuItem,
            this.FavoritesMenuItem,
            this.SelectFromContentsTreeMenuItem});
			this.SourceQueryMenu.Name = "SourceQueryMenu";
			this.SourceQueryMenu.Size = new System.Drawing.Size(272, 92);
			// 
			// CurrentQueryMenuItem
			// 
			this.CurrentQueryMenuItem.Name = "CurrentQueryMenuItem";
			this.CurrentQueryMenuItem.Size = new System.Drawing.Size(271, 22);
			this.CurrentQueryMenuItem.Text = "The Current Query";
			// 
			// MruListMenuItem
			// 
			this.MruListMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("MruListMenuItem.Image")));
			this.MruListMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.MruListMenuItem.Name = "MruListMenuItem";
			this.MruListMenuItem.Size = new System.Drawing.Size(271, 22);
			this.MruListMenuItem.Text = "Recent Queries";
			// 
			// FavoritesMenuItem
			// 
			this.FavoritesMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("FavoritesMenuItem.Image")));
			this.FavoritesMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.FavoritesMenuItem.Name = "FavoritesMenuItem";
			this.FavoritesMenuItem.Size = new System.Drawing.Size(271, 22);
			this.FavoritesMenuItem.Text = "Favorites";
			// 
			// SelectFromContentsTreeMenuItem
			// 
			this.SelectFromContentsTreeMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("SelectFromContentsTreeMenuItem.Image")));
			this.SelectFromContentsTreeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SelectFromContentsTreeMenuItem.Name = "SelectFromContentsTreeMenuItem";
			this.SelectFromContentsTreeMenuItem.Size = new System.Drawing.Size(271, 22);
			this.SelectFromContentsTreeMenuItem.Text = "Select from Database Contents Tree...";
			this.SelectFromContentsTreeMenuItem.Click += new System.EventHandler(this.SelectFromContentsTreeMenuItem_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Add.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "Edit.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "ScrollDown16x16.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "Delete.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "up2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "down2.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "Copy.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "Paste.bmp");
			this.Bitmaps16x16.Images.SetKeyName(8, "Properties.bmp");
			// 
			// SourceQuerySelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SourceQueryComboBox);
			this.Name = "SourceQuerySelectorControl";
			this.Size = new System.Drawing.Size(277, 21);
			((System.ComponentModel.ISupportInitialize)(this.SourceQueryComboBox.Properties)).EndInit();
			this.SourceQueryMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.ComboBoxEdit SourceQueryComboBox;
		private System.Windows.Forms.ContextMenuStrip SourceQueryMenu;
		private System.Windows.Forms.ToolStripMenuItem CurrentQueryMenuItem;
		private System.Windows.Forms.ToolStripMenuItem MruListMenuItem;
		private System.Windows.Forms.ToolStripMenuItem FavoritesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SelectFromContentsTreeMenuItem;
		public System.Windows.Forms.ImageList Bitmaps16x16;
	}
}
