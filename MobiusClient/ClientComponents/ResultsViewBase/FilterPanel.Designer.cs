namespace Mobius.ClientComponents
{
	partial class FilterPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterPanel));
			this.FilterContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.BasicFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ListFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ItemSliderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RangeSliderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.RemoveFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ResetAllFiltersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RemoveAllFiltersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ScrollablePanel = new DevExpress.XtraEditors.XtraScrollableControl();
			this.ColumnLabel = new DevExpress.XtraEditors.LabelControl();
			this.FilterDropDownButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps5x5 = new System.Windows.Forms.ImageList(this.components);
			this.TableLabel = new DevExpress.XtraEditors.LabelControl();
			this.FilterStructureControl = new Mobius.ClientComponents.FilterStructureControl();
			this.FilterRangeControl = new Mobius.ClientComponents.FilterRangeSliderControl();
			this.FilterListControl = new Mobius.ClientComponents.FilterCheckBoxListControl();
			this.FilterItemControl = new Mobius.ClientComponents.FilterItemSliderControl();
			this.FilterBasicControl = new Mobius.ClientComponents.FilterBasicCriteriaControl();
			this.AddFilterButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.FiltersEnabledCtl = new DevExpress.XtraEditors.CheckEdit();
			this.FilterContextMenu.SuspendLayout();
			this.ScrollablePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FiltersEnabledCtl.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// FilterContextMenu
			// 
			this.FilterContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BasicFilterMenuItem,
            this.ListFilterMenuItem,
            this.ItemSliderMenuItem,
            this.RangeSliderMenuItem,
            this.toolStripSeparator2,
            this.RemoveFilterMenuItem,
            this.toolStripSeparator1,
            this.ResetAllFiltersMenuItem,
            this.RemoveAllFiltersMenuItem});
			this.FilterContextMenu.Name = "FilterContextMenu";
			this.FilterContextMenu.Size = new System.Drawing.Size(171, 170);
			// 
			// BasicFilterMenuItem
			// 
			this.BasicFilterMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("BasicFilterMenuItem.Image")));
			this.BasicFilterMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.BasicFilterMenuItem.Name = "BasicFilterMenuItem";
			this.BasicFilterMenuItem.Size = new System.Drawing.Size(170, 22);
			this.BasicFilterMenuItem.Text = "Basic Filter";
			this.BasicFilterMenuItem.Click += new System.EventHandler(this.BasicFilterMenuItem_Click);
			// 
			// ListFilterMenuItem
			// 
			this.ListFilterMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ListFilterMenuItem.Image")));
			this.ListFilterMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ListFilterMenuItem.Name = "ListFilterMenuItem";
			this.ListFilterMenuItem.Size = new System.Drawing.Size(170, 22);
			this.ListFilterMenuItem.Text = "List Filter";
			this.ListFilterMenuItem.Click += new System.EventHandler(this.ListFilterMenuItem_Click);
			// 
			// ItemSliderMenuItem
			// 
			this.ItemSliderMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ItemSliderMenuItem.Image")));
			this.ItemSliderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ItemSliderMenuItem.Name = "ItemSliderMenuItem";
			this.ItemSliderMenuItem.Size = new System.Drawing.Size(170, 22);
			this.ItemSliderMenuItem.Text = "Item Slider";
			this.ItemSliderMenuItem.Click += new System.EventHandler(this.ItemSliderMenuItem_Click);
			// 
			// RangeSliderMenuItem
			// 
			this.RangeSliderMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RangeSliderMenuItem.Image")));
			this.RangeSliderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RangeSliderMenuItem.Name = "RangeSliderMenuItem";
			this.RangeSliderMenuItem.Size = new System.Drawing.Size(170, 22);
			this.RangeSliderMenuItem.Text = "Range Slider";
			this.RangeSliderMenuItem.Click += new System.EventHandler(this.RangeSliderMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(167, 6);
			// 
			// RemoveFilterMenuItem
			// 
			this.RemoveFilterMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("RemoveFilterMenuItem.Image")));
			this.RemoveFilterMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RemoveFilterMenuItem.Name = "RemoveFilterMenuItem";
			this.RemoveFilterMenuItem.Size = new System.Drawing.Size(170, 22);
			this.RemoveFilterMenuItem.Text = "Remove Filter";
			this.RemoveFilterMenuItem.Click += new System.EventHandler(this.RemoveFilterMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(167, 6);
			// 
			// ResetAllFiltersMenuItem
			// 
			this.ResetAllFiltersMenuItem.Name = "ResetAllFiltersMenuItem";
			this.ResetAllFiltersMenuItem.Size = new System.Drawing.Size(170, 22);
			this.ResetAllFiltersMenuItem.Text = "Reset All Filters";
			this.ResetAllFiltersMenuItem.Click += new System.EventHandler(this.ResetAllFiltersMenuItem_Click);
			// 
			// RemoveAllFiltersMenuItem
			// 
			this.RemoveAllFiltersMenuItem.Name = "RemoveAllFiltersMenuItem";
			this.RemoveAllFiltersMenuItem.Size = new System.Drawing.Size(170, 22);
			this.RemoveAllFiltersMenuItem.Text = "Remove All Filters";
			this.RemoveAllFiltersMenuItem.Click += new System.EventHandler(this.RemoveAllFiltersMenuItem_Click);
			// 
			// ScrollablePanel
			// 
			this.ScrollablePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ScrollablePanel.Controls.Add(this.ColumnLabel);
			this.ScrollablePanel.Controls.Add(this.FilterDropDownButton);
			this.ScrollablePanel.Controls.Add(this.TableLabel);
			this.ScrollablePanel.Controls.Add(this.FilterStructureControl);
			this.ScrollablePanel.Controls.Add(this.FilterRangeControl);
			this.ScrollablePanel.Controls.Add(this.FilterListControl);
			this.ScrollablePanel.Controls.Add(this.FilterItemControl);
			this.ScrollablePanel.Controls.Add(this.FilterBasicControl);
			this.ScrollablePanel.Location = new System.Drawing.Point(3, 41);
			this.ScrollablePanel.Name = "ScrollablePanel";
			this.ScrollablePanel.Size = new System.Drawing.Size(294, 351);
			this.ScrollablePanel.TabIndex = 2;
			this.ScrollablePanel.Resize += new System.EventHandler(this.ScrollablePanel_Resize);
			// 
			// ColumnLabel
			// 
			this.ColumnLabel.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ColumnLabel.Appearance.ForeColor = System.Drawing.Color.Black;
			this.ColumnLabel.Location = new System.Drawing.Point(5, 35);
			this.ColumnLabel.Name = "ColumnLabel";
			this.ColumnLabel.Size = new System.Drawing.Size(72, 14);
			this.ColumnLabel.TabIndex = 190;
			this.ColumnLabel.Text = "Column Label";
			// 
			// FilterDropDownButton
			// 
			this.FilterDropDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterDropDownButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.FilterDropDownButton.Appearance.Options.UseBackColor = true;
			this.FilterDropDownButton.ImageIndex = 0;
			this.FilterDropDownButton.ImageList = this.Bitmaps5x5;
			this.FilterDropDownButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.FilterDropDownButton.Location = new System.Drawing.Point(282, 66);
			this.FilterDropDownButton.Name = "FilterDropDownButton";
			this.FilterDropDownButton.Size = new System.Drawing.Size(12, 20);
			this.FilterDropDownButton.TabIndex = 192;
			this.FilterDropDownButton.Click += new System.EventHandler(this.FilterDropDownButton_Click);
			// 
			// Bitmaps5x5
			// 
			this.Bitmaps5x5.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps5x5.ImageStream")));
			this.Bitmaps5x5.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps5x5.Images.SetKeyName(0, "DropDown5x5.bmp");
			// 
			// TableLabel
			// 
			this.TableLabel.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TableLabel.Appearance.ForeColor = System.Drawing.Color.Blue;
			this.TableLabel.Location = new System.Drawing.Point(0, 15);
			this.TableLabel.Name = "TableLabel";
			this.TableLabel.Size = new System.Drawing.Size(62, 14);
			this.TableLabel.TabIndex = 189;
			this.TableLabel.Text = "Table Label";
			// 
			// FilterStructureControl
			// 
			this.FilterStructureControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterStructureControl.Location = new System.Drawing.Point(3, 275);
			this.FilterStructureControl.Name = "FilterStructureControl";
			this.FilterStructureControl.Size = new System.Drawing.Size(279, 76);
			this.FilterStructureControl.TabIndex = 4;
			// 
			// FilterRangeControl
			// 
			this.FilterRangeControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterRangeControl.Location = new System.Drawing.Point(3, 215);
			this.FilterRangeControl.Name = "FilterRangeControl";
			this.FilterRangeControl.Size = new System.Drawing.Size(279, 54);
			this.FilterRangeControl.TabIndex = 3;
			// 
			// FilterListControl
			// 
			this.FilterListControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterListControl.Location = new System.Drawing.Point(3, 100);
			this.FilterListControl.Name = "FilterListControl";
			this.FilterListControl.Size = new System.Drawing.Size(279, 70);
			this.FilterListControl.TabIndex = 2;
			// 
			// FilterItemControl
			// 
			this.FilterItemControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterItemControl.Location = new System.Drawing.Point(3, 176);
			this.FilterItemControl.Name = "FilterItemControl";
			this.FilterItemControl.Size = new System.Drawing.Size(279, 33);
			this.FilterItemControl.TabIndex = 1;
			// 
			// FilterBasicControl
			// 
			this.FilterBasicControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterBasicControl.Location = new System.Drawing.Point(3, 62);
			this.FilterBasicControl.Name = "FilterBasicControl";
			this.FilterBasicControl.Size = new System.Drawing.Size(279, 29);
			this.FilterBasicControl.TabIndex = 0;
			// 
			// AddFilterButton
			// 
			this.AddFilterButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.AddFilterButton.Appearance.Options.UseBackColor = true;
			this.AddFilterButton.ImageIndex = 0;
			this.AddFilterButton.ImageList = this.Bitmaps16x16;
			this.AddFilterButton.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleLeft;
			this.AddFilterButton.Location = new System.Drawing.Point(3, 4);
			this.AddFilterButton.Name = "AddFilterButton";
			this.AddFilterButton.Size = new System.Drawing.Size(49, 20);
			this.AddFilterButton.TabIndex = 191;
			this.AddFilterButton.Text = "Add";
			this.AddFilterButton.Click += new System.EventHandler(this.AddFilterButton_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "FilterQuick.bmp");
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(0, 26);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(300, 4);
			this.labelControl1.TabIndex = 193;
			// 
			// FiltersEnabledCtl
			// 
			this.FiltersEnabledCtl.AutoSizeInLayoutControl = true;
			this.FiltersEnabledCtl.Location = new System.Drawing.Point(66, 5);
			this.FiltersEnabledCtl.Name = "FiltersEnabledCtl";
			this.FiltersEnabledCtl.Properties.Caption = "Enabled";
			this.FiltersEnabledCtl.Size = new System.Drawing.Size(102, 19);
			this.FiltersEnabledCtl.TabIndex = 193;
			this.FiltersEnabledCtl.CheckedChanged += new System.EventHandler(this.FiltersEnabled_CheckedChanged);
			// 
			// FilterPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.FiltersEnabledCtl);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.ScrollablePanel);
			this.Controls.Add(this.AddFilterButton);
			this.Name = "FilterPanel";
			this.Size = new System.Drawing.Size(300, 398);
			this.Load += new System.EventHandler(this.ChartFilterPanel_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.FilterPanel_Paint);
			this.FilterContextMenu.ResumeLayout(false);
			this.ScrollablePanel.ResumeLayout(false);
			this.ScrollablePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FiltersEnabledCtl.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip FilterContextMenu;
		private System.Windows.Forms.ToolStripMenuItem RemoveFilterMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem ResetAllFiltersMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RemoveAllFiltersMenuItem;
		private DevExpress.XtraEditors.XtraScrollableControl ScrollablePanel;
		private FilterStructureControl FilterStructureControl;
		private FilterRangeSliderControl FilterRangeControl;
		private FilterCheckBoxListControl FilterListControl;
		private FilterItemSliderControl FilterItemControl;
		private FilterBasicCriteriaControl FilterBasicControl;
		public DevExpress.XtraEditors.LabelControl ColumnLabel;
		public DevExpress.XtraEditors.LabelControl TableLabel;
		private System.Windows.Forms.ToolStripMenuItem BasicFilterMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ListFilterMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ItemSliderMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RangeSliderMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private DevExpress.XtraEditors.SimpleButton AddFilterButton;
		private DevExpress.XtraEditors.SimpleButton FilterDropDownButton;
		private System.Windows.Forms.ImageList Bitmaps5x5;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		private System.Windows.Forms.ImageList Bitmaps16x16;
		private DevExpress.XtraEditors.CheckEdit FiltersEnabledCtl;

	}
}
