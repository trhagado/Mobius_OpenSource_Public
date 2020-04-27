namespace Mobius.Helm
{
	partial class HelmEditorDialog
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
			this.SaveButton = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel_Button = new DevExpress.XtraEditors.SimpleButton();
			this.HelmControl = new Mobius.Helm.HelmControl();
			this.SuspendLayout();
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveButton.Appearance.Options.UseFont = true;
			this.SaveButton.Appearance.Options.UseForeColor = true;
			this.SaveButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveButton.Location = new System.Drawing.Point(845, 632);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveButton.Size = new System.Drawing.Size(66, 24);
			this.SaveButton.TabIndex = 22;
			this.SaveButton.Tag = "";
			this.SaveButton.Text = "Save";
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel_Button.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel_Button.Appearance.Options.UseFont = true;
			this.Cancel_Button.Appearance.Options.UseForeColor = true;
			this.Cancel_Button.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = new System.Drawing.Point(920, 632);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel_Button.Size = new System.Drawing.Size(59, 24);
			this.Cancel_Button.TabIndex = 23;
			this.Cancel_Button.Tag = "Cancel";
			this.Cancel_Button.Text = "Cancel";
			this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// HelmControl
			// 
			this.HelmControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HelmControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.HelmControl.HelmMode = Mobius.Helm.HelmControlMode.BrowserEditor;
			this.HelmControl.Location = new System.Drawing.Point(0, 0);
			this.HelmControl.Margin = new System.Windows.Forms.Padding(2);
			this.HelmControl.Name = "HelmControl";
			this.HelmControl.Size = new System.Drawing.Size(982, 628);
			this.HelmControl.TabIndex = 0;
			this.HelmControl.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HelmControl_Scroll);
			// 
			// HelmEditorDialog
			// 
			this.AcceptButton = this.SaveButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 661);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.HelmControl);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MinimizeBox = false;
			this.Name = "HelmEditorDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Helm Editor";
			this.Shown += new System.EventHandler(this.HelmEditorDialog_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private HelmControl HelmControl;
		public DevExpress.XtraEditors.SimpleButton SaveButton;
		public DevExpress.XtraEditors.SimpleButton Cancel_Button;
	}
}