namespace Mobius.ClientComponents
{
	partial class SelectFieldFromContents
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
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
			this.FieldGrid = new Mobius.ClientComponents.QueryTableControl();
			this.SummarizationLevelContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.UnsummarizedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SummarizedViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ContentsTreeWithSearch = new Mobius.ClientComponents.ContentsTreeWithSearch();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
			this.splitContainerControl1.SuspendLayout();
			this.SummarizationLevelContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Prompt
			// 
			this.Prompt.Location = new System.Drawing.Point(7, 6);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(324, 13);
			this.Prompt.TabIndex = 15;
			this.Prompt.Text = "Select a data table/assay from Database Contents and a Data Field";
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
			this.Cancel.Location = new System.Drawing.Point(686, 563);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(59, 22);
			this.Cancel.TabIndex = 17;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(618, 563);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(59, 22);
			this.OK.TabIndex = 16;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// splitContainerControl1
			// 
			this.splitContainerControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainerControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.splitContainerControl1.Location = new System.Drawing.Point(2, 27);
			this.splitContainerControl1.Name = "splitContainerControl1";
			this.splitContainerControl1.Panel1.Controls.Add(this.ContentsTreeWithSearch);
			this.splitContainerControl1.Panel1.Text = "Panel1";
			this.splitContainerControl1.Panel2.Controls.Add(this.FieldGrid);
			this.splitContainerControl1.Panel2.Text = "Panel2";
			this.splitContainerControl1.Size = new System.Drawing.Size(745, 529);
			this.splitContainerControl1.SplitterPosition = 428;
			this.splitContainerControl1.TabIndex = 18;
			this.splitContainerControl1.Text = "splitContainerControl1";
			// 
			// FieldGrid
			// 
			this.FieldGrid.CanDeselectKeyCol = false;
			this.FieldGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FieldGrid.Location = new System.Drawing.Point(0, 0);
			this.FieldGrid.Name = "FieldGrid";
			this.FieldGrid.QueryTable = null;
			this.FieldGrid.SelectOnly = true;
			this.FieldGrid.SelectSingle = true;
			this.FieldGrid.Size = new System.Drawing.Size(308, 525);
			this.FieldGrid.TabIndex = 149;
			// 
			// SummarizationLevelContextMenu
			// 
			this.SummarizationLevelContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UnsummarizedMenuItem,
            this.SummarizedViewMenuItem});
			this.SummarizationLevelContextMenu.Name = "SelectMenu";
			this.SummarizationLevelContextMenu.Size = new System.Drawing.Size(183, 48);
			// 
			// UnsummarizedMenuItem
			// 
			this.UnsummarizedMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Add;
			this.UnsummarizedMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.UnsummarizedMenuItem.Name = "UnsummarizedMenuItem";
			this.UnsummarizedMenuItem.Size = new System.Drawing.Size(182, 22);
			this.UnsummarizedMenuItem.Text = "Unsummarized View";
			this.UnsummarizedMenuItem.Click += new System.EventHandler(this.UnsummarizedMenuItem_Click);
			// 
			// SummarizedViewMenuItem
			// 
			this.SummarizedViewMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.AddSummary;
			this.SummarizedViewMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.SummarizedViewMenuItem.Name = "SummarizedViewMenuItem";
			this.SummarizedViewMenuItem.Size = new System.Drawing.Size(182, 22);
			this.SummarizedViewMenuItem.Text = "Summarized View";
			this.SummarizedViewMenuItem.Click += new System.EventHandler(this.SummarizedViewMenuItem_Click);
			// 
			// ContentsTreeWithSearch
			// 
			this.ContentsTreeWithSearch.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ContentsTreeWithSearch.Appearance.Options.UseBackColor = true;
			this.ContentsTreeWithSearch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentsTreeWithSearch.Location = new System.Drawing.Point(0, 0);
			this.ContentsTreeWithSearch.Name = "ContentsTreeWithSearch";
			this.ContentsTreeWithSearch.Size = new System.Drawing.Size(428, 525);
			this.ContentsTreeWithSearch.TabIndex = 1;
			// 
			// SelectFieldFromContents
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(749, 589);
			this.Controls.Add(this.splitContainerControl1);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Prompt);
			this.MinimizeBox = false;
			this.Name = "SelectFieldFromContents";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Data Field";
			((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
			this.splitContainerControl1.ResumeLayout(false);
			this.SummarizationLevelContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
		private QueryTableControl FieldGrid;
		public System.Windows.Forms.ContextMenuStrip SummarizationLevelContextMenu;
		public System.Windows.Forms.ToolStripMenuItem UnsummarizedMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SummarizedViewMenuItem;
		private ContentsTreeWithSearch ContentsTreeWithSearch;
	}
}