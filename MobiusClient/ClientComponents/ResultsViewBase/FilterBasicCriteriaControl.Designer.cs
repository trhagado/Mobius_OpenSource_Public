namespace Mobius.ClientComponents
{
	partial class FilterBasicCriteriaControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterBasicCriteriaControl));
			this.Value = new DevExpress.XtraEditors.TextEdit();
			this.OpMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ContainsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.EqMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.NotEqMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LtMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GtMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.BetweenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.IsBlankMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.IsNotBlankMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.ListFilterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ItemSliderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RangeSliderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OpButton = new DevExpress.XtraEditors.DropDownButton();
			this.Value2 = new DevExpress.XtraEditors.TextEdit();
			((System.ComponentModel.ISupportInitialize)(this.Value.Properties)).BeginInit();
			this.OpMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Value2.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Value
			// 
			this.Value.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Value.Location = new System.Drawing.Point(85, 4);
			this.Value.Name = "Value";
			this.Value.Size = new System.Drawing.Size(74, 20);
			this.Value.TabIndex = 2;
			this.Value.TextChanged += new System.EventHandler(this.Value_TextChanged);
			// 
			// OpMenu
			// 
			this.OpMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContainsMenuItem,
            this.EqMenuItem,
            this.NotEqMenuItem,
            this.LtMenuItem,
            this.LeMenuItem,
            this.GtMenuItem,
            this.GeMenuItem,
            this.BetweenMenuItem,
            this.IsBlankMenuItem,
            this.IsNotBlankMenuItem,
            this.toolStripMenuItem1,
            this.ListFilterMenuItem,
            this.ItemSliderMenuItem,
            this.RangeSliderMenuItem});
			this.OpMenu.Name = "BasicOpMenu";
			this.OpMenu.Size = new System.Drawing.Size(153, 318);
			// 
			// ContainsMenuItem
			// 
			this.ContainsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ContainsMenuItem.Image")));
			this.ContainsMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ContainsMenuItem.Name = "ContainsMenuItem";
			this.ContainsMenuItem.Size = new System.Drawing.Size(152, 22);
			this.ContainsMenuItem.Text = "Contains";
			this.ContainsMenuItem.Click += new System.EventHandler(this.ContainsMenuItem_Click);
			// 
			// EqMenuItem
			// 
			this.EqMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("EqMenuItem.Image")));
			this.EqMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.EqMenuItem.Name = "EqMenuItem";
			this.EqMenuItem.Size = new System.Drawing.Size(152, 22);
			this.EqMenuItem.Text = "Equals";
			this.EqMenuItem.Click += new System.EventHandler(this.EqMenuItem_Click);
			// 
			// NotEqMenuItem
			// 
			this.NotEqMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("NotEqMenuItem.Image")));
			this.NotEqMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.NotEqMenuItem.Name = "NotEqMenuItem";
			this.NotEqMenuItem.Size = new System.Drawing.Size(152, 22);
			this.NotEqMenuItem.Text = "Not equal";
			this.NotEqMenuItem.Click += new System.EventHandler(this.NotEqMenuItem_Click);
			// 
			// LtMenuItem
			// 
			this.LtMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("LtMenuItem.Image")));
			this.LtMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.LtMenuItem.Name = "LtMenuItem";
			this.LtMenuItem.Size = new System.Drawing.Size(152, 22);
			this.LtMenuItem.Text = "Less";
			this.LtMenuItem.Click += new System.EventHandler(this.LtMenuItem_Click);
			// 
			// LeMenuItem
			// 
			this.LeMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("LeMenuItem.Image")));
			this.LeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.LeMenuItem.Name = "LeMenuItem";
			this.LeMenuItem.Size = new System.Drawing.Size(152, 22);
			this.LeMenuItem.Text = "Lt or eq";
			this.LeMenuItem.Click += new System.EventHandler(this.LeMenuItem_Click);
			// 
			// GtMenuItem
			// 
			this.GtMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("GtMenuItem.Image")));
			this.GtMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.GtMenuItem.Name = "GtMenuItem";
			this.GtMenuItem.Size = new System.Drawing.Size(152, 22);
			this.GtMenuItem.Text = "Greater";
			this.GtMenuItem.Click += new System.EventHandler(this.GtMenuItem_Click);
			// 
			// GeMenuItem
			// 
			this.GeMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("GeMenuItem.Image")));
			this.GeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.GeMenuItem.Name = "GeMenuItem";
			this.GeMenuItem.Size = new System.Drawing.Size(152, 22);
			this.GeMenuItem.Text = "Gt or eq";
			this.GeMenuItem.Click += new System.EventHandler(this.GeMenuItem_Click);
			// 
			// BetweenMenuItem
			// 
			this.BetweenMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("BetweenMenuItem.Image")));
			this.BetweenMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.BetweenMenuItem.Name = "BetweenMenuItem";
			this.BetweenMenuItem.Size = new System.Drawing.Size(152, 22);
			this.BetweenMenuItem.Text = "Between";
			this.BetweenMenuItem.Click += new System.EventHandler(this.BetweenMenuItem_Click);
			// 
			// IsBlankMenuItem
			// 
			this.IsBlankMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("IsBlankMenuItem.Image")));
			this.IsBlankMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.IsBlankMenuItem.Name = "IsBlankMenuItem";
			this.IsBlankMenuItem.Size = new System.Drawing.Size(152, 22);
			this.IsBlankMenuItem.Text = "Is blank";
			this.IsBlankMenuItem.Click += new System.EventHandler(this.IsBlankMenuItem_Click);
			// 
			// IsNotBlankMenuItem
			// 
			this.IsNotBlankMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("IsNotBlankMenuItem.Image")));
			this.IsNotBlankMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.IsNotBlankMenuItem.Name = "IsNotBlankMenuItem";
			this.IsNotBlankMenuItem.Size = new System.Drawing.Size(152, 22);
			this.IsNotBlankMenuItem.Text = "Not blank";
			this.IsNotBlankMenuItem.Click += new System.EventHandler(this.IsNotBlankMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
			// 
			// ListFilterMenuItem
			// 
			this.ListFilterMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.List2;
			this.ListFilterMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ListFilterMenuItem.Name = "ListFilterMenuItem";
			this.ListFilterMenuItem.Size = new System.Drawing.Size(152, 22);
			this.ListFilterMenuItem.Text = "List Filter";
			this.ListFilterMenuItem.Click += new System.EventHandler(this.ListFilterMenuItem_Click);
			// 
			// ItemSliderMenuItem
			// 
			this.ItemSliderMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.TrackBar;
			this.ItemSliderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ItemSliderMenuItem.Name = "ItemSliderMenuItem";
			this.ItemSliderMenuItem.Size = new System.Drawing.Size(152, 22);
			this.ItemSliderMenuItem.Text = "Item Slider";
			this.ItemSliderMenuItem.Click += new System.EventHandler(this.ItemSliderMenuItem_Click);
			// 
			// RangeSliderMenuItem
			// 
			this.RangeSliderMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.RangeTrackBar;
			this.RangeSliderMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.RangeSliderMenuItem.Name = "RangeSliderMenuItem";
			this.RangeSliderMenuItem.Size = new System.Drawing.Size(152, 22);
			this.RangeSliderMenuItem.Text = "Range Slider";
			this.RangeSliderMenuItem.Click += new System.EventHandler(this.RangeSliderMenuItem_Click);
			// 
			// OpButton
			// 
			this.OpButton.Location = new System.Drawing.Point(3, 4);
			this.OpButton.Name = "OpButton";
			this.OpButton.Size = new System.Drawing.Size(74, 20);
			this.OpButton.TabIndex = 1;
			this.OpButton.Text = "Contains";
			this.OpButton.Click += new System.EventHandler(this.OpButton_Click);
			this.OpButton.ShowDropDownControl += new DevExpress.XtraEditors.ShowDropDownControlEventHandler(this.OpButton_ShowDropDownControl);
			// 
			// Value2
			// 
			this.Value2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.Value2.Location = new System.Drawing.Point(167, 4);
			this.Value2.Name = "Value2";
			this.Value2.Size = new System.Drawing.Size(74, 20);
			this.Value2.TabIndex = 3;
			this.Value2.TextChanged += new System.EventHandler(this.Value2_TextChanged);
			// 
			// FilterBasicCriteriaControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Value2);
			this.Controls.Add(this.OpButton);
			this.Controls.Add(this.Value);
			this.Name = "FilterBasicCriteriaControl";
			this.Size = new System.Drawing.Size(246, 28);
			this.Resize += new System.EventHandler(this.FilterBasicControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.Value.Properties)).EndInit();
			this.OpMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Value2.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraEditors.TextEdit Value;
		public System.Windows.Forms.ContextMenuStrip OpMenu;
		public System.Windows.Forms.ToolStripMenuItem EqMenuItem;
		public System.Windows.Forms.ToolStripMenuItem NotEqMenuItem;
		public System.Windows.Forms.ToolStripMenuItem LtMenuItem;
		public System.Windows.Forms.ToolStripMenuItem LeMenuItem;
		public System.Windows.Forms.ToolStripMenuItem GtMenuItem;
		public System.Windows.Forms.ToolStripMenuItem GeMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BetweenMenuItem;
		private System.Windows.Forms.ToolStripMenuItem IsBlankMenuItem;
		private System.Windows.Forms.ToolStripMenuItem IsNotBlankMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ContainsMenuItem;
		private DevExpress.XtraEditors.DropDownButton OpButton;
		internal DevExpress.XtraEditors.TextEdit Value2;
		private System.Windows.Forms.ToolStripMenuItem ListFilterMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ItemSliderMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RangeSliderMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
	}
}
