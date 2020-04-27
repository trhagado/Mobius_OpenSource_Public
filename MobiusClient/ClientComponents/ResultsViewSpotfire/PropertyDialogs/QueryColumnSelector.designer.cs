namespace Mobius.SpotfireClient
{
	partial class QueryColumnSelect
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryColumnSelect));
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.SelectFieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.SelectFromDatabaseContentsTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ColumnName = new DevExpress.XtraEditors.ButtonEdit();
			this.ShowPopupMenuTimer = new System.Windows.Forms.Timer(this.components);
			this.SelectFieldMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnName.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDownChevron8x8.bmp");
			// 
			// SelectFieldMenu
			// 
			this.SelectFieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem,
            this.toolStripMenuItem1,
            this.SelectFromDatabaseContentsTreeMenuItem});
			this.SelectFieldMenu.Name = "SelectFieldMenu";
			this.SelectFieldMenu.Size = new System.Drawing.Size(274, 54);
			this.SelectFieldMenu.Opening += new System.ComponentModel.CancelEventHandler(this.SelectFieldMenu_Opening);
			// 
			// Table1MenuItem
			// 
			this.Table1MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Field1MenuItem});
			this.Table1MenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Table1MenuItem.Image")));
			this.Table1MenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.Table1MenuItem.Name = "Table1MenuItem";
			this.Table1MenuItem.Size = new System.Drawing.Size(273, 22);
			this.Table1MenuItem.Text = "Table1";
			// 
			// Field1MenuItem
			// 
			this.Field1MenuItem.Name = "Field1MenuItem";
			this.Field1MenuItem.Size = new System.Drawing.Size(105, 22);
			this.Field1MenuItem.Text = "Field1";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(270, 6);
			// 
			// SelectFromDatabaseContentsTreeMenuItem
			// 
			this.SelectFromDatabaseContentsTreeMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.WorldSearch;
			this.SelectFromDatabaseContentsTreeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SelectFromDatabaseContentsTreeMenuItem.Name = "SelectFromDatabaseContentsTreeMenuItem";
			this.SelectFromDatabaseContentsTreeMenuItem.Size = new System.Drawing.Size(273, 22);
			this.SelectFromDatabaseContentsTreeMenuItem.Text = "Select From Database Contents Tree...";
			// 
			// ColumnName
			// 
			this.ColumnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ColumnName.Location = new System.Drawing.Point(0, 0);
			this.ColumnName.Name = "ColumnName";
			this.ColumnName.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.ColumnName.Size = new System.Drawing.Size(302, 20);
			this.ColumnName.TabIndex = 1;
			this.ColumnName.Click += new System.EventHandler(this.ColumnName_Click);
			// 
			// ShowPopupMenuTimer
			// 
			this.ShowPopupMenuTimer.Tick += new System.EventHandler(this.ShowPopupMenuTimer_Tick);
			// 
			// ColumnSelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ColumnName);
			this.Name = "ColumnSelectorControl";
			this.Size = new System.Drawing.Size(303, 21);
			this.SelectFieldMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ColumnName.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps8x8;
		private System.Windows.Forms.ContextMenuStrip SelectFieldMenu;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem SelectFromDatabaseContentsTreeMenuItem;
		private DevExpress.XtraEditors.ButtonEdit ColumnName;
		private System.Windows.Forms.Timer ShowPopupMenuTimer;
	}
}
