namespace Mobius.ClientComponents
{
	partial class ChartPagePanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartPagePanel));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.Bitmaps8x8 = new System.Windows.Forms.ImageList(this.components);
			this.Table1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Field1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SelectFieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ChartPanel = new Mobius.ClientComponents.ChartPanel();
			this.SelectFieldMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "MouseSelect.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "MouseZoom.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "MouseTranslate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "MouseRotate.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "FullScreen.bmp");
			this.Bitmaps16x16.Images.SetKeyName(5, "maximizeWindow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(6, "RestoreWindow.bmp");
			this.Bitmaps16x16.Images.SetKeyName(7, "CloseWindow.bmp");
			// 
			// Bitmaps8x8
			// 
			this.Bitmaps8x8.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps8x8.ImageStream")));
			this.Bitmaps8x8.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps8x8.Images.SetKeyName(0, "DropDown8x8.bmp");
			// 
			// Table1MenuItem
			// 
			this.Table1MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Field1MenuItem});
			this.Table1MenuItem.Image = ((System.Drawing.Image)(resources.GetObject("Table1MenuItem.Image")));
			this.Table1MenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.Table1MenuItem.Name = "Table1MenuItem";
			this.Table1MenuItem.Size = new System.Drawing.Size(117, 22);
			this.Table1MenuItem.Text = "Table1";
			// 
			// Field1MenuItem
			// 
			this.Field1MenuItem.Name = "Field1MenuItem";
			this.Field1MenuItem.Size = new System.Drawing.Size(113, 22);
			this.Field1MenuItem.Text = "Field1";
			// 
			// SelectFieldMenu
			// 
			this.SelectFieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Table1MenuItem});
			this.SelectFieldMenu.Name = "SelectFieldMenu";
			this.SelectFieldMenu.Size = new System.Drawing.Size(118, 26);
			// 
			// ChartPanel
			// 
			this.ChartPanel.Appearance.BackColor = System.Drawing.Color.WhiteSmoke;
			this.ChartPanel.Appearance.Options.UseBackColor = true;
			this.ChartPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChartPanel.Location = new System.Drawing.Point(0, 0);
			this.ChartPanel.Name = "ChartPanel";
			this.ChartPanel.Size = new System.Drawing.Size(862, 673);
			this.ChartPanel.TabIndex = 201;
			this.ChartPanel.UseMouseForScrolling = true;
			// 
			// ChartPagePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ChartPanel);
			this.Name = "ChartPagePanel";
			this.Size = new System.Drawing.Size(862, 673);
			this.SelectFieldMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps16x16;
		private System.Windows.Forms.ImageList Bitmaps8x8;
		private System.Windows.Forms.ToolStripMenuItem Table1MenuItem;
		private System.Windows.Forms.ToolStripMenuItem Field1MenuItem;
		private System.Windows.Forms.ContextMenuStrip SelectFieldMenu;
		internal ChartPanel ChartPanel;

	}
}
