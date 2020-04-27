namespace Mobius.ClientComponents
{
	partial class UserObjectSaveDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserObjectSaveDialog));
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.ObjectName = new DevExpress.XtraEditors.TextEdit();
			this.ObjectTarget = new DevExpress.XtraEditors.TextEdit();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.SaveButton = new DevExpress.XtraEditors.SimpleButton();
			this.ObjectNameLabel = new DevExpress.XtraEditors.LabelControl();
			this.ProjectName = new DevExpress.XtraEditors.TextEdit();
			this.ProjectTarget = new DevExpress.XtraEditors.TextEdit();
			this.ProjectLabel = new DevExpress.XtraEditors.LabelControl();
			this.SaveDialogContentsTree = new Mobius.ClientComponents.ContentsTreeControl();
			this.PermissionsButton = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.ObjectName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ObjectTarget.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectTarget.Properties)).BeginInit();
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
			// ObjectName
			// 
			this.ObjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ObjectName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.ObjectName.Location = new System.Drawing.Point(46, 474);
			this.ObjectName.Name = "ObjectName";
			this.ObjectName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.ObjectName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ObjectName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ObjectName.Properties.Appearance.Options.UseBackColor = true;
			this.ObjectName.Properties.Appearance.Options.UseFont = true;
			this.ObjectName.Properties.Appearance.Options.UseForeColor = true;
			this.ObjectName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ObjectName.Size = new System.Drawing.Size(248, 19);
			this.ObjectName.TabIndex = 1;
			// 
			// ObjectTarget
			// 
			this.ObjectTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectTarget.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.ObjectTarget.Location = new System.Drawing.Point(183, 478);
			this.ObjectTarget.Name = "ObjectTarget";
			this.ObjectTarget.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.ObjectTarget.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ObjectTarget.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ObjectTarget.Properties.Appearance.Options.UseBackColor = true;
			this.ObjectTarget.Properties.Appearance.Options.UseFont = true;
			this.ObjectTarget.Properties.Appearance.Options.UseForeColor = true;
			this.ObjectTarget.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ObjectTarget.Size = new System.Drawing.Size(67, 20);
			this.ObjectTarget.TabIndex = 38;
			this.ObjectTarget.Visible = false;
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CancelBut.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CancelBut.Appearance.Options.UseFont = true;
			this.CancelBut.Appearance.Options.UseForeColor = true;
			this.CancelBut.Cursor = System.Windows.Forms.Cursors.Default;
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(374, 496);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CancelBut.Size = new System.Drawing.Size(57, 22);
			this.CancelBut.TabIndex = 5;
			this.CancelBut.Text = "Cancel";
			this.CancelBut.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveButton.Appearance.Options.UseFont = true;
			this.SaveButton.Appearance.Options.UseForeColor = true;
			this.SaveButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveButton.Location = new System.Drawing.Point(310, 496);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveButton.Size = new System.Drawing.Size(58, 22);
			this.SaveButton.TabIndex = 4;
			this.SaveButton.Text = "Save";
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// ObjectNameLabel
			// 
			this.ObjectNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectNameLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ObjectNameLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ObjectNameLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ObjectNameLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.ObjectNameLabel.Location = new System.Drawing.Point(8, 475);
			this.ObjectNameLabel.Name = "ObjectNameLabel";
			this.ObjectNameLabel.Size = new System.Drawing.Size(31, 13);
			this.ObjectNameLabel.TabIndex = 40;
			this.ObjectNameLabel.Text = "Name:";
			// 
			// ProjectName
			// 
			this.ProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ProjectName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.ProjectName.Location = new System.Drawing.Point(46, 498);
			this.ProjectName.Name = "ProjectName";
			this.ProjectName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.ProjectName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProjectName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ProjectName.Properties.Appearance.Options.UseBackColor = true;
			this.ProjectName.Properties.Appearance.Options.UseFont = true;
			this.ProjectName.Properties.Appearance.Options.UseForeColor = true;
			this.ProjectName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ProjectName.Size = new System.Drawing.Size(248, 19);
			this.ProjectName.TabIndex = 2;
			// 
			// ProjectTarget
			// 
			this.ProjectTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProjectTarget.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.ProjectTarget.Enabled = false;
			this.ProjectTarget.Location = new System.Drawing.Point(182, 506);
			this.ProjectTarget.Name = "ProjectTarget";
			this.ProjectTarget.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.ProjectTarget.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProjectTarget.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ProjectTarget.Properties.Appearance.Options.UseBackColor = true;
			this.ProjectTarget.Properties.Appearance.Options.UseFont = true;
			this.ProjectTarget.Properties.Appearance.Options.UseForeColor = true;
			this.ProjectTarget.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ProjectTarget.Size = new System.Drawing.Size(68, 20);
			this.ProjectTarget.TabIndex = 44;
			this.ProjectTarget.Visible = false;
			// 
			// ProjectLabel
			// 
			this.ProjectLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProjectLabel.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ProjectLabel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProjectLabel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.ProjectLabel.Location = new System.Drawing.Point(7, 499);
			this.ProjectLabel.Name = "ProjectLabel";
			this.ProjectLabel.Size = new System.Drawing.Size(32, 13);
			this.ProjectLabel.TabIndex = 42;
			this.ProjectLabel.Text = "Folder:";
			// 
			// SaveDialogContentsTree
			// 
			this.SaveDialogContentsTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.SaveDialogContentsTree.Location = new System.Drawing.Point(2, 26);
			this.SaveDialogContentsTree.Name = "SaveDialogContentsTree";
			this.SaveDialogContentsTree.Size = new System.Drawing.Size(432, 443);
			this.SaveDialogContentsTree.TabIndex = 201;
			this.SaveDialogContentsTree.FocusedNodeChanged += new System.EventHandler(this.SaveDialogContentsTree_FocusedNodeChanged);
			this.SaveDialogContentsTree.DoubleClick += new System.EventHandler(this.SaveDialogContentsTree_DoubleClick);
			this.SaveDialogContentsTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SaveDialogContentsTree_MouseDown);
			// 
			// PermissionsButton
			// 
			this.PermissionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PermissionsButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PermissionsButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.PermissionsButton.Appearance.Options.UseFont = true;
			this.PermissionsButton.Appearance.Options.UseForeColor = true;
			this.PermissionsButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.PermissionsButton.ImageIndex = 0;
			this.PermissionsButton.ImageList = this.Bitmaps16x16;
			this.PermissionsButton.Location = new System.Drawing.Point(310, 471);
			this.PermissionsButton.Name = "PermissionsButton";
			this.PermissionsButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.PermissionsButton.Size = new System.Drawing.Size(121, 22);
			this.PermissionsButton.TabIndex = 202;
			this.PermissionsButton.Text = "Permissions...";
			this.PermissionsButton.Click += new System.EventHandler(this.Permissions_Click);
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "Permissions.bmp");
			// 
			// UserObjectSaveDialog
			// 
			this.AcceptButton = this.SaveButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelBut;
			this.ClientSize = new System.Drawing.Size(435, 522);
			this.Controls.Add(this.PermissionsButton);
			this.Controls.Add(this.ProjectName);
			this.Controls.Add(this.ProjectTarget);
			this.Controls.Add(this.ProjectLabel);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.ObjectName);
			this.Controls.Add(this.ObjectTarget);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.ObjectNameLabel);
			this.Controls.Add(this.SaveDialogContentsTree);
			this.MinimizeBox = false;
			this.Name = "UserObjectSaveDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "UserObjectSaveDialog";
			this.Activated += new System.EventHandler(this.UserObjectSaveDialog_Activated);
			((System.ComponentModel.ISupportInitialize)(this.ObjectName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ObjectTarget.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectTarget.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.TextEdit ObjectName;
		public DevExpress.XtraEditors.TextEdit ObjectTarget;
		public DevExpress.XtraEditors.SimpleButton CancelBut;
		public DevExpress.XtraEditors.SimpleButton SaveButton;
		public DevExpress.XtraEditors.LabelControl ObjectNameLabel;
		public DevExpress.XtraEditors.TextEdit ProjectName;
		public DevExpress.XtraEditors.TextEdit ProjectTarget;
		public DevExpress.XtraEditors.LabelControl ProjectLabel;
		internal ContentsTreeControl SaveDialogContentsTree;
		public DevExpress.XtraEditors.SimpleButton PermissionsButton;
		private System.Windows.Forms.ImageList Bitmaps16x16;
	}
}