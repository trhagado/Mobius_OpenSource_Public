namespace Mobius.ClientComponents
{
	partial class FieldSelectorControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldSelectorControl));
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList();
			this.SelectField = new DevExpress.XtraEditors.SimpleButton();
			this.FieldName = new DevExpress.XtraEditors.TextEdit();
			this.TableName = new DevExpress.XtraEditors.TextEdit();
			this.TableLabel = new DevExpress.XtraEditors.LabelControl();
			this.FieldLabel = new DevExpress.XtraEditors.LabelControl();
			this.SelectFieldMenu = new System.Windows.Forms.ContextMenuStrip();
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.SelectFromDatabaseContentsTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.FieldName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TableName.Properties)).BeginInit();
			this.SelectFieldMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDownChevron8x8.bmp");
			// 
			// SelectField
			// 
			this.SelectField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectField.ImageOptions.ImageIndex = 0;
			this.SelectField.ImageOptions.ImageList = this.Bitmaps8x8;
			this.SelectField.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.SelectField.Location = new System.Drawing.Point(382, 0);
			this.SelectField.Name = "SelectField";
			this.SelectField.Size = new System.Drawing.Size(18, 38);
			this.SelectField.TabIndex = 27;
			this.SelectField.Click += new System.EventHandler(this.SelectField_Click);
			// 
			// FieldName
			// 
			this.FieldName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FieldName.Location = new System.Drawing.Point(44, 19);
			this.FieldName.Name = "FieldName";
			this.FieldName.Size = new System.Drawing.Size(335, 20);
			this.FieldName.TabIndex = 26;
			this.FieldName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FieldName_KeyDown);
			this.FieldName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FieldName_MouseDown);
			// 
			// TableName
			// 
			this.TableName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TableName.Location = new System.Drawing.Point(44, 0);
			this.TableName.Name = "TableName";
			this.TableName.Size = new System.Drawing.Size(335, 20);
			this.TableName.TabIndex = 25;
			this.TableName.VisibleChanged += new System.EventHandler(this.TableName_VisibleChanged);
			this.TableName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TableName_KeyDown);
			this.TableName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TableName_MouseDown);
			// 
			// TableLabel
			// 
			this.TableLabel.Location = new System.Drawing.Point(1, 2);
			this.TableLabel.Name = "TableLabel";
			this.TableLabel.Size = new System.Drawing.Size(30, 13);
			this.TableLabel.TabIndex = 24;
			this.TableLabel.Text = "Table:";
			// 
			// FieldLabel
			// 
			this.FieldLabel.Location = new System.Drawing.Point(1, 20);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = new System.Drawing.Size(39, 13);
			this.FieldLabel.TabIndex = 23;
			this.FieldLabel.Text = "Column:";
			// 
			// SelectFieldMenu
			// 
			this.SelectFieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem,
            this.toolStripMenuItem1,
            this.SelectFromDatabaseContentsTreeMenuItem});
			this.SelectFieldMenu.Name = "SelectFieldMenu";
			this.SelectFieldMenu.Size = new System.Drawing.Size(274, 54);
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
			this.SelectFromDatabaseContentsTreeMenuItem.Click += new System.EventHandler(this.SelectFromDatabaseContentsTreeMenuItem_Click);
			// 
			// FieldSelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SelectField);
			this.Controls.Add(this.FieldName);
			this.Controls.Add(this.TableName);
			this.Controls.Add(this.TableLabel);
			this.Controls.Add(this.FieldLabel);
			this.Name = "FieldSelectorControl";
			this.Size = new System.Drawing.Size(402, 39);
			((System.ComponentModel.ISupportInitialize)(this.FieldName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TableName.Properties)).EndInit();
			this.SelectFieldMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps8x8;
		private DevExpress.XtraEditors.SimpleButton SelectField;
		public DevExpress.XtraEditors.LabelControl TableLabel;
		public DevExpress.XtraEditors.LabelControl FieldLabel;
		private System.Windows.Forms.ContextMenuStrip SelectFieldMenu;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		public DevExpress.XtraEditors.TextEdit FieldName;
		public DevExpress.XtraEditors.TextEdit TableName;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem SelectFromDatabaseContentsTreeMenuItem;
	}
}
