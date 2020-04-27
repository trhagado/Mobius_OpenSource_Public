namespace Mobius.ClientComponents
{
	partial class UserObjectOpenDialog
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
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.ObjectName = new DevExpress.XtraEditors.TextEdit();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.ObjectNameLabel = new DevExpress.XtraEditors.LabelControl();
			this.ContentsTreeWithSearch = new Mobius.ClientComponents.ContentsTreeWithSearch();
			((System.ComponentModel.ISupportInitialize)(this.ObjectName.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Prompt
			// 
			this.Prompt.Location = new System.Drawing.Point(6, 4);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(34, 13);
			this.Prompt.TabIndex = 31;
			this.Prompt.Text = "Prompt";
			// 
			// ObjectName
			// 
			this.ObjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ObjectName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.ObjectName.Location = new System.Drawing.Point(47, 498);
			this.ObjectName.Name = "ObjectName";
			this.ObjectName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.ObjectName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ObjectName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ObjectName.Properties.Appearance.Options.UseBackColor = true;
			this.ObjectName.Properties.Appearance.Options.UseFont = true;
			this.ObjectName.Properties.Appearance.Options.UseForeColor = true;
			this.ObjectName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ObjectName.Size = new System.Drawing.Size(257, 20);
			this.ObjectName.TabIndex = 26;
			this.ObjectName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ObjectName_KeyPress);
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
			this.Cancel.Location = new System.Drawing.Point(374, 496);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(57, 22);
			this.Cancel.TabIndex = 28;
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
			this.OK.Location = new System.Drawing.Point(310, 496);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(58, 22);
			this.OK.TabIndex = 27;
			this.OK.Text = "Open";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// ObjectNameLabel
			// 
			this.ObjectNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectNameLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ObjectNameLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ObjectNameLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ObjectNameLabel.Appearance.Options.UseBackColor = true;
			this.ObjectNameLabel.Appearance.Options.UseFont = true;
			this.ObjectNameLabel.Appearance.Options.UseForeColor = true;
			this.ObjectNameLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.ObjectNameLabel.Location = new System.Drawing.Point(9, 500);
			this.ObjectNameLabel.Name = "ObjectNameLabel";
			this.ObjectNameLabel.Size = new System.Drawing.Size(31, 13);
			this.ObjectNameLabel.TabIndex = 32;
			this.ObjectNameLabel.Text = "Name:";
			// 
			// ContentsTreeWithSearch
			// 
			this.ContentsTreeWithSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentsTreeWithSearch.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ContentsTreeWithSearch.Appearance.Options.UseBackColor = true;
			this.ContentsTreeWithSearch.Location = new System.Drawing.Point(2, 23);
			this.ContentsTreeWithSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ContentsTreeWithSearch.Name = "ContentsTreeWithSearch";
			this.ContentsTreeWithSearch.Size = new System.Drawing.Size(431, 468);
			this.ContentsTreeWithSearch.TabIndex = 201;
			// 
			// UserObjectOpenDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(435, 523);
			this.Controls.Add(this.ContentsTreeWithSearch);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.ObjectName);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.ObjectNameLabel);
			this.MinimizeBox = false;
			this.Name = "UserObjectOpenDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "UserObjectOpenDialog";
			this.Activated += new System.EventHandler(this.UserObjectOpenDialog_Activated);
			((System.ComponentModel.ISupportInitialize)(this.ObjectName.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.TextEdit ObjectName;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl ObjectNameLabel;
		private ContentsTreeWithSearch ContentsTreeWithSearch;
	}
}