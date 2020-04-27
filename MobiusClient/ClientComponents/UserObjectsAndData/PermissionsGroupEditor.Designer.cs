namespace Mobius.ClientComponents
{
	partial class PermissionsGroupEditor
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
			this.PermissionsList = new Mobius.ClientComponents.PermissionsList();
			this.OkButton = new DevExpress.XtraEditors.SimpleButton();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.SuspendLayout();
			// 
			// PermissionsList
			// 
			this.PermissionsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.PermissionsList.Location = new System.Drawing.Point(3, 3);
			this.PermissionsList.Name = "PermissionsList";
			this.PermissionsList.Size = new System.Drawing.Size(390, 389);
			this.PermissionsList.TabIndex = 0;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.Location = new System.Drawing.Point(253, 399);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = new System.Drawing.Size(66, 24);
			this.OkButton.TabIndex = 218;
			this.OkButton.Text = "OK";
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(325, 399);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.Size = new System.Drawing.Size(66, 24);
			this.CancelBut.TabIndex = 217;
			this.CancelBut.Text = "Cancel";
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(0, 391);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(394, 4);
			this.labelControl2.TabIndex = 219;
			// 
			// UserGroupEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelBut;
			this.ClientSize = new System.Drawing.Size(395, 427);
			this.Controls.Add(this.PermissionsList);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CancelBut);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UserGroupEditor";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Edit User Group";
			this.Activated += new System.EventHandler(this.UserGroupEditor_Activated);
			this.ResumeLayout(false);

		}

		#endregion

		private PermissionsList PermissionsList;
		private DevExpress.XtraEditors.SimpleButton OkButton;
		private DevExpress.XtraEditors.SimpleButton CancelBut;
		private DevExpress.XtraEditors.LabelControl labelControl2;
	}
}