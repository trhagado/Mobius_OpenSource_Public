namespace Mobius.ClientComponents
{
	partial class SelectFromContents
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
			this.SelectionName = new DevExpress.XtraEditors.TextEdit();
			this.SelectionTarget = new DevExpress.XtraEditors.TextEdit();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.ObjectNameLabel = new DevExpress.XtraEditors.LabelControl();
			this.SummarizationLevelContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.UnsummarizedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SummarizedViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ContentsTreeWithSearch = new Mobius.ClientComponents.ContentsTreeWithSearch();
			((System.ComponentModel.ISupportInitialize)(this.SelectionName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectionTarget.Properties)).BeginInit();
			this.SummarizationLevelContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Prompt
			// 
			this.Prompt.Location = new System.Drawing.Point(5, 4);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(34, 13);
			this.Prompt.TabIndex = 39;
			this.Prompt.Text = "Prompt";
			// 
			// SelectionName
			// 
			this.SelectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectionName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.SelectionName.Location = new System.Drawing.Point(41, 561);
			this.SelectionName.Name = "SelectionName";
			this.SelectionName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.SelectionName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectionName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.SelectionName.Properties.Appearance.Options.UseBackColor = true;
			this.SelectionName.Properties.Appearance.Options.UseFont = true;
			this.SelectionName.Properties.Appearance.Options.UseForeColor = true;
			this.SelectionName.Properties.ReadOnly = true;
			this.SelectionName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SelectionName.Size = new System.Drawing.Size(318, 20);
			this.SelectionName.TabIndex = 34;
			// 
			// SelectionTarget
			// 
			this.SelectionTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectionTarget.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.SelectionTarget.Location = new System.Drawing.Point(261, 569);
			this.SelectionTarget.Name = "SelectionTarget";
			this.SelectionTarget.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.SelectionTarget.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SelectionTarget.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.SelectionTarget.Properties.Appearance.Options.UseBackColor = true;
			this.SelectionTarget.Properties.Appearance.Options.UseFont = true;
			this.SelectionTarget.Properties.Appearance.Options.UseForeColor = true;
			this.SelectionTarget.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SelectionTarget.Size = new System.Drawing.Size(67, 20);
			this.SelectionTarget.TabIndex = 38;
			this.SelectionTarget.Visible = false;
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
			this.Cancel.Location = new System.Drawing.Point(439, 559);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(57, 22);
			this.Cancel.TabIndex = 36;
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
			this.OK.Location = new System.Drawing.Point(375, 559);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(58, 22);
			this.OK.TabIndex = 35;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// ObjectNameLabel
			// 
			this.ObjectNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectNameLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ObjectNameLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ObjectNameLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ObjectNameLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.ObjectNameLabel.Location = new System.Drawing.Point(3, 563);
			this.ObjectNameLabel.Name = "ObjectNameLabel";
			this.ObjectNameLabel.Size = new System.Drawing.Size(31, 13);
			this.ObjectNameLabel.TabIndex = 40;
			this.ObjectNameLabel.Text = "Name:";
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
			this.ContentsTreeWithSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentsTreeWithSearch.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ContentsTreeWithSearch.Appearance.Options.UseBackColor = true;
			this.ContentsTreeWithSearch.Location = new System.Drawing.Point(5, 23);
			this.ContentsTreeWithSearch.Name = "ContentsTreeWithSearch";
			this.ContentsTreeWithSearch.Size = new System.Drawing.Size(503, 530);
			this.ContentsTreeWithSearch.TabIndex = 0;
			// 
			// SelectFromContents
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(512, 585);
			this.Controls.Add(this.ContentsTreeWithSearch);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.SelectionName);
			this.Controls.Add(this.SelectionTarget);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.ObjectNameLabel);
			this.MinimizeBox = false;
			this.Name = "SelectFromContents";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SelectFromContents";
			this.Shown += new System.EventHandler(this.SelectFromContents_Shown);
			((System.ComponentModel.ISupportInitialize)(this.SelectionName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectionTarget.Properties)).EndInit();
			this.SummarizationLevelContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.TextEdit SelectionName;
		public DevExpress.XtraEditors.TextEdit SelectionTarget;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl ObjectNameLabel;
		public System.Windows.Forms.ContextMenuStrip SummarizationLevelContextMenu;
		public System.Windows.Forms.ToolStripMenuItem UnsummarizedMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SummarizedViewMenuItem;
		private ContentsTreeWithSearch ContentsTreeWithSearch;
	}
}