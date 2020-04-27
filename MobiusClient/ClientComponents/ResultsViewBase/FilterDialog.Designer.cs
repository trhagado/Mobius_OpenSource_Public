namespace Mobius.ClientComponents
{
	partial class FilterDialog
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
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.ClearFilter = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ChangeFilterType = new DevExpress.XtraEditors.DropDownButton();
			this.FilterTypesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.BasicFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ListFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ItemSliderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RangeSliderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.EditStructure = new DevExpress.XtraEditors.SimpleButton();
			this.FilterStructureControl = new Mobius.ClientComponents.FilterStructureControl();
			this.FilterRangeControl = new Mobius.ClientComponents.FilterRangeSliderControl();
			this.FilterBasicControl = new Mobius.ClientComponents.FilterBasicCriteriaControl();
			this.FilterItemControl = new Mobius.ClientComponents.FilterItemSliderControl();
			this.FilterListControl = new Mobius.ClientComponents.FilterCheckBoxListControl();
			this.Divider = new DevExpress.XtraEditors.LabelControl();
			this.FilterTypesContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(83, 555);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(50, 22);
			this.OK.TabIndex = 104;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// ClearFilter
			// 
			this.ClearFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearFilter.Location = new System.Drawing.Point(138, 555);
			this.ClearFilter.Name = "ClearFilter";
			this.ClearFilter.Size = new System.Drawing.Size(52, 22);
			this.ClearFilter.TabIndex = 102;
			this.ClearFilter.Text = "Remove";
			this.ClearFilter.Click += new System.EventHandler(this.ClearFilter_Click);
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(195, 555);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(50, 22);
			this.Cancel.TabIndex = 103;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// ChangeFilterType
			// 
			this.ChangeFilterType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ChangeFilterType.Location = new System.Drawing.Point(3, 555);
			this.ChangeFilterType.Name = "ChangeFilterType";
			this.ChangeFilterType.Size = new System.Drawing.Size(52, 22);
			this.ChangeFilterType.TabIndex = 118;
			this.ChangeFilterType.Text = "More";
			this.ChangeFilterType.Click += new System.EventHandler(this.ChangeFilterType_Click);
			this.ChangeFilterType.ShowDropDownControl += new DevExpress.XtraEditors.ShowDropDownControlEventHandler(this.ChangeFilterType_ShowDropDownControl);
			// 
			// FilterTypesContextMenu
			// 
			this.FilterTypesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BasicFilterMenuItem,
            this.ListFilterMenuItem,
            this.ItemSliderMenuItem,
            this.RangeSliderMenuItem});
			this.FilterTypesContextMenu.Name = "FilterTypesContextMenu";
			this.FilterTypesContextMenu.Size = new System.Drawing.Size(146, 92);
			// 
			// BasicFilterMenuItem
			// 
			this.BasicFilterMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.TextEdit;
			this.BasicFilterMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.BasicFilterMenuItem.Name = "BasicFilterMenuItem";
			this.BasicFilterMenuItem.Size = new System.Drawing.Size(145, 22);
			this.BasicFilterMenuItem.Text = "Basic Filter";
			this.BasicFilterMenuItem.Click += new System.EventHandler(this.BasicFilterMenuItem_Click);
			// 
			// ListFilterMenuItem
			// 
			this.ListFilterMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.List2;
			this.ListFilterMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ListFilterMenuItem.Name = "ListFilterMenuItem";
			this.ListFilterMenuItem.Size = new System.Drawing.Size(145, 22);
			this.ListFilterMenuItem.Text = "List Filter";
			this.ListFilterMenuItem.Click += new System.EventHandler(this.CheckBoxFilterMenuItem_Click);
			// 
			// ItemSliderMenuItem
			// 
			this.ItemSliderMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.TrackBar;
			this.ItemSliderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ItemSliderMenuItem.Name = "ItemSliderMenuItem";
			this.ItemSliderMenuItem.Size = new System.Drawing.Size(145, 22);
			this.ItemSliderMenuItem.Text = "Item Slider";
			this.ItemSliderMenuItem.Click += new System.EventHandler(this.ItemSliderMenuItem_Click);
			// 
			// RangeSliderMenuItem
			// 
			this.RangeSliderMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.RangeTrackBar;
			this.RangeSliderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RangeSliderMenuItem.Name = "RangeSliderMenuItem";
			this.RangeSliderMenuItem.Size = new System.Drawing.Size(145, 22);
			this.RangeSliderMenuItem.Text = "Range Slider";
			this.RangeSliderMenuItem.Click += new System.EventHandler(this.RangeSliderMenuItem_Click);
			// 
			// EditStructure
			// 
			this.EditStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditStructure.Location = new System.Drawing.Point(3, 520);
			this.EditStructure.Name = "EditStructure";
			this.EditStructure.Size = new System.Drawing.Size(52, 22);
			this.EditStructure.TabIndex = 128;
			this.EditStructure.Text = "Edit";
			this.EditStructure.Click += new System.EventHandler(this.EditStructure_Click);
			// 
			// FilterStructureControl
			// 
			this.FilterStructureControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterStructureControl.Location = new System.Drawing.Point(1, 342);
			this.FilterStructureControl.Name = "FilterStructureControl";
			this.FilterStructureControl.Size = new System.Drawing.Size(247, 160);
			this.FilterStructureControl.TabIndex = 127;
			// 
			// FilterRangeControl
			// 
			this.FilterRangeControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterRangeControl.Location = new System.Drawing.Point(1, 288);
			this.FilterRangeControl.Name = "FilterRangeControl";
			this.FilterRangeControl.Size = new System.Drawing.Size(247, 44);
			this.FilterRangeControl.TabIndex = 125;
			// 
			// FilterBasicControl
			// 
			this.FilterBasicControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterBasicControl.Location = new System.Drawing.Point(1, 0);
			this.FilterBasicControl.Name = "FilterBasicControl";
			this.FilterBasicControl.Size = new System.Drawing.Size(247, 28);
			this.FilterBasicControl.TabIndex = 122;
			// 
			// FilterItemControl
			// 
			this.FilterItemControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterItemControl.Location = new System.Drawing.Point(1, 240);
			this.FilterItemControl.Name = "FilterItemControl";
			this.FilterItemControl.Size = new System.Drawing.Size(247, 44);
			this.FilterItemControl.TabIndex = 124;
			// 
			// FilterListControl
			// 
			this.FilterListControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterListControl.Location = new System.Drawing.Point(1, 28);
			this.FilterListControl.Name = "FilterListControl";
			this.FilterListControl.Size = new System.Drawing.Size(247, 202);
			this.FilterListControl.TabIndex = 126;
			// 
			// Divider
			// 
			this.Divider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Divider.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Divider.Appearance.ForeColor = System.Drawing.Color.Black;
			this.Divider.Appearance.Options.UseFont = true;
			this.Divider.Appearance.Options.UseForeColor = true;
			this.Divider.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Divider.LineVisible = true;
			this.Divider.Location = new System.Drawing.Point(0, 548);
			this.Divider.Name = "Divider";
			this.Divider.Size = new System.Drawing.Size(252, 2);
			this.Divider.TabIndex = 194;
			// 
			// FilterDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(249, 581);
			this.ControlBox = false;
			this.Controls.Add(this.Divider);
			this.Controls.Add(this.EditStructure);
			this.Controls.Add(this.ChangeFilterType);
			this.Controls.Add(this.FilterStructureControl);
			this.Controls.Add(this.FilterRangeControl);
			this.Controls.Add(this.FilterBasicControl);
			this.Controls.Add(this.FilterItemControl);
			this.Controls.Add(this.FilterListControl);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.ClearFilter);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FilterDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Deactivate += new System.EventHandler(this.CriteriaInteractive_Deactivate);
			this.Activated += new System.EventHandler(this.FilterDialog_Activated);
			this.FilterTypesContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		private DevExpress.XtraEditors.DropDownButton ChangeFilterType;
		private System.Windows.Forms.ContextMenuStrip FilterTypesContextMenu;
		private System.Windows.Forms.ToolStripMenuItem ListFilterMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BasicFilterMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ItemSliderMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RangeSliderMenuItem;
		private FilterRangeSliderControl FilterRangeControl;
		private FilterItemSliderControl FilterItemControl;
		private FilterBasicCriteriaControl FilterBasicControl;
		private FilterCheckBoxListControl FilterListControl;
		public DevExpress.XtraEditors.SimpleButton ClearFilter;
		private FilterStructureControl FilterStructureControl;
		private DevExpress.XtraEditors.SimpleButton EditStructure;
		public DevExpress.XtraEditors.LabelControl Divider;
	}
}