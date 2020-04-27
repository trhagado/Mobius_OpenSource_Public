namespace Mobius.SpotfireClient
{
	partial class TableSelectorControlMsx
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableSelectorControlMsx));
			this.SelectTableMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.SelectFromDatabaseContentsTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TableNameDropDown = new DevExpress.XtraEditors.DropDownButton();
			this.SelectTableMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// SelectTableMenu
			// 
			this.SelectTableMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem,
            this.toolStripMenuItem1,
            this.SelectFromDatabaseContentsTreeMenuItem});
			this.SelectTableMenu.Name = "SelectFieldMenu";
			this.SelectTableMenu.Size = new System.Drawing.Size(273, 54);
			this.SelectTableMenu.Opening += new System.ComponentModel.CancelEventHandler(this.SelectTableMenu_Opening);
			// 
			// Table1MenuItem
			// 
			this.Table1MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Field1MenuItem});
			this.Table1MenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Table1MenuItem.Image")));
			this.Table1MenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.Table1MenuItem.Name = "Table1MenuItem";
			this.Table1MenuItem.Size = new System.Drawing.Size(272, 22);
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
			this.toolStripMenuItem1.Size = new System.Drawing.Size(269, 6);
			// 
			// SelectFromDatabaseContentsTreeMenuItem
			// 
			this.SelectFromDatabaseContentsTreeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SelectFromDatabaseContentsTreeMenuItem.Name = "SelectFromDatabaseContentsTreeMenuItem";
			this.SelectFromDatabaseContentsTreeMenuItem.Size = new System.Drawing.Size(272, 22);
			this.SelectFromDatabaseContentsTreeMenuItem.Text = "Select From Database Contents Tree...";
			// 
			// TableNameDropDown
			// 
			this.TableNameDropDown.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TableNameDropDown.Appearance.Options.UseFont = true;
			this.TableNameDropDown.AutoSize = true;
			this.TableNameDropDown.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.TableNameDropDown.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("TableNameDropDown.ImageOptions.Image")));
			this.TableNameDropDown.Location = new System.Drawing.Point(2, 2);
			this.TableNameDropDown.Margin = new System.Windows.Forms.Padding(2);
			this.TableNameDropDown.Name = "TableNameDropDown";
			this.TableNameDropDown.Size = new System.Drawing.Size(97, 20);
			this.TableNameDropDown.TabIndex = 6;
			this.TableNameDropDown.Text = "TableName";
			this.TableNameDropDown.ArrowButtonClick += new System.EventHandler(this.TableNameDropDown_ArrowButtonClick);
			this.TableNameDropDown.Click += new System.EventHandler(this.TableNameDropDown_Click);
			// 
			// TableSelectorControlMsx
			// 
			this.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.TableNameDropDown);
			this.Name = "TableSelectorControlMsx";
			this.Size = new System.Drawing.Size(267, 24);
			this.SelectTableMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ContextMenuStrip SelectTableMenu;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem SelectFromDatabaseContentsTreeMenuItem;
		public DevExpress.XtraEditors.DropDownButton TableNameDropDown;
	}
}
