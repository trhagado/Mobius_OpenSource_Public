namespace Mobius.ClientComponents
{
	partial class CriteriaStructureSimOptions
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
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.ECFP4 = new DevExpress.XtraEditors.CheckEdit();
			this.SaveSimDefault = new DevExpress.XtraEditors.SimpleButton();
			this.Sub = new DevExpress.XtraEditors.CheckEdit();
			this.Super = new DevExpress.XtraEditors.CheckEdit();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.MinSim = new DevExpress.XtraEditors.TextEdit();
			this.MinSim_Label = new DevExpress.XtraEditors.LabelControl();
			this.MaxHits_Label = new DevExpress.XtraEditors.LabelControl();
			this.MaxHits = new DevExpress.XtraEditors.TextEdit();
			this.Normal = new DevExpress.XtraEditors.CheckEdit();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ToolTip1 = new System.Windows.Forms.ToolTip();
			this.Frame2 = new System.Windows.Forms.GroupBox();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.Help = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.ECFP4.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Sub.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Super.Properties)).BeginInit();
			this.Frame1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MinSim.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxHits.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Normal.Properties)).BeginInit();
			this.Frame2.SuspendLayout();
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
			this.OK.Location = new System.Drawing.Point(21, 170);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 13;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// ECFP4
			// 
			this.ECFP4.Cursor = System.Windows.Forms.Cursors.Default;
			this.ECFP4.Location = new System.Drawing.Point(7, 44);
			this.ECFP4.Name = "ECFP4";
			this.ECFP4.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ECFP4.Properties.Appearance.Options.UseBackColor = true;
			this.ECFP4.Properties.Caption = "ECFP4 Fingerprint";
			this.ECFP4.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ECFP4.Properties.RadioGroupIndex = 1;
			this.ECFP4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ECFP4.Size = new System.Drawing.Size(132, 19);
			this.ECFP4.TabIndex = 18;
			this.ECFP4.TabStop = false;
			// 
			// SaveSimDefault
			// 
			this.SaveSimDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveSimDefault.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveSimDefault.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveSimDefault.Appearance.Options.UseFont = true;
			this.SaveSimDefault.Appearance.Options.UseForeColor = true;
			this.SaveSimDefault.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveSimDefault.Location = new System.Drawing.Point(152, 170);
			this.SaveSimDefault.Name = "SaveSimDefault";
			this.SaveSimDefault.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveSimDefault.Size = new System.Drawing.Size(95, 23);
			this.SaveSimDefault.TabIndex = 19;
			this.SaveSimDefault.Text = "Save as Default";
			this.SaveSimDefault.Click += new System.EventHandler(this.SaveSimDefault_Click);
			// 
			// Sub
			// 
			this.Sub.Cursor = System.Windows.Forms.Cursors.Default;
			this.Sub.Location = new System.Drawing.Point(161, 16);
			this.Sub.Name = "Sub";
			this.Sub.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Sub.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Sub.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Sub.Properties.Appearance.Options.UseBackColor = true;
			this.Sub.Properties.Appearance.Options.UseFont = true;
			this.Sub.Properties.Appearance.Options.UseForeColor = true;
			this.Sub.Properties.Caption = "Sub-similar";
			this.Sub.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Sub.Properties.RadioGroupIndex = 1;
			this.Sub.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Sub.Size = new System.Drawing.Size(122, 19);
			this.Sub.TabIndex = 17;
			this.Sub.TabStop = false;
			this.ToolTip1.SetToolTip(this.Sub, "Retrieve structures that contain more structural complexity than your query");
			// 
			// Super
			// 
			this.Super.Cursor = System.Windows.Forms.Cursors.Default;
			this.Super.Location = new System.Drawing.Point(161, 44);
			this.Super.Name = "Super";
			this.Super.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Super.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Super.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Super.Properties.Appearance.Options.UseBackColor = true;
			this.Super.Properties.Appearance.Options.UseFont = true;
			this.Super.Properties.Appearance.Options.UseForeColor = true;
			this.Super.Properties.Caption = "Super-similar";
			this.Super.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Super.Properties.RadioGroupIndex = 1;
			this.Super.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Super.Size = new System.Drawing.Size(133, 19);
			this.Super.TabIndex = 16;
			this.Super.TabStop = false;
			this.ToolTip1.SetToolTip(this.Super, "Retrieve structures that contain less structural complexity than your query");
			// 
			// Frame1
			// 
			this.Frame1.BackColor = System.Drawing.Color.Transparent;
			this.Frame1.Controls.Add(this.MinSim);
			this.Frame1.Controls.Add(this.MinSim_Label);
			this.Frame1.Controls.Add(this.MaxHits_Label);
			this.Frame1.Controls.Add(this.MaxHits);
			this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame1.Location = new System.Drawing.Point(7, 91);
			this.Frame1.Name = "Frame1";
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(307, 72);
			this.Frame1.TabIndex = 12;
			this.Frame1.TabStop = false;
			this.Frame1.Text = "Similarity limit";
			// 
			// MinSim
			// 
			this.MinSim.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.MinSim.EditValue = "67";
			this.MinSim.Location = new System.Drawing.Point(177, 16);
			this.MinSim.Name = "MinSim";
			this.MinSim.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.MinSim.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinSim.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.MinSim.Properties.Appearance.Options.UseBackColor = true;
			this.MinSim.Properties.Appearance.Options.UseFont = true;
			this.MinSim.Properties.Appearance.Options.UseForeColor = true;
			this.MinSim.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MinSim.Size = new System.Drawing.Size(42, 20);
			this.MinSim.TabIndex = 7;
			// 
			// MinSim_Label
			// 
			this.MinSim_Label.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MinSim_Label.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinSim_Label.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MinSim_Label.Appearance.Options.UseBackColor = true;
			this.MinSim_Label.Appearance.Options.UseFont = true;
			this.MinSim_Label.Appearance.Options.UseForeColor = true;
			this.MinSim_Label.Cursor = System.Windows.Forms.Cursors.Default;
			this.MinSim_Label.Location = new System.Drawing.Point(15, 20);
			this.MinSim_Label.Name = "MinSim_Label";
			this.MinSim_Label.Size = new System.Drawing.Size(154, 13);
			this.MinSim_Label.TabIndex = 6;
			this.MinSim_Label.Text = "Minimum required sim. (0.0 - 1.0):";
			// 
			// MaxHits_Label
			// 
			this.MaxHits_Label.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.MaxHits_Label.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaxHits_Label.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MaxHits_Label.Appearance.Options.UseBackColor = true;
			this.MaxHits_Label.Appearance.Options.UseFont = true;
			this.MaxHits_Label.Appearance.Options.UseForeColor = true;
			this.MaxHits_Label.Cursor = System.Windows.Forms.Cursors.Default;
			this.MaxHits_Label.Location = new System.Drawing.Point(15, 46);
			this.MaxHits_Label.Name = "MaxHits_Label";
			this.MaxHits_Label.Size = new System.Drawing.Size(116, 13);
			this.MaxHits_Label.TabIndex = 14;
			this.MaxHits_Label.Text = "Maximum number of hits:";
			// 
			// MaxHits
			// 
			this.MaxHits.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.MaxHits.EditValue = "500";
			this.MaxHits.Location = new System.Drawing.Point(177, 43);
			this.MaxHits.Name = "MaxHits";
			this.MaxHits.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.MaxHits.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaxHits.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.MaxHits.Properties.Appearance.Options.UseBackColor = true;
			this.MaxHits.Properties.Appearance.Options.UseFont = true;
			this.MaxHits.Properties.Appearance.Options.UseForeColor = true;
			this.MaxHits.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MaxHits.Size = new System.Drawing.Size(42, 20);
			this.MaxHits.TabIndex = 16;
			// 
			// Normal
			// 
			this.Normal.Cursor = System.Windows.Forms.Cursors.Default;
			this.Normal.EditValue = true;
			this.Normal.Location = new System.Drawing.Point(7, 16);
			this.Normal.Name = "Normal";
			this.Normal.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Normal.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Normal.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Normal.Properties.Appearance.Options.UseBackColor = true;
			this.Normal.Properties.Appearance.Options.UseFont = true;
			this.Normal.Properties.Appearance.Options.UseForeColor = true;
			this.Normal.Properties.Caption = "Fingerprint";
			this.Normal.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Normal.Properties.RadioGroupIndex = 1;
			this.Normal.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Normal.Size = new System.Drawing.Size(101, 19);
			this.Normal.TabIndex = 15;
			this.ToolTip1.SetToolTip(this.Normal, "Retrieve structures that contain the same structural complexity as your query");
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
			this.Cancel.Location = new System.Drawing.Point(86, 170);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 15;
			this.Cancel.Text = "Cancel";
			// 
			// Frame2
			// 
			this.Frame2.BackColor = System.Drawing.Color.Transparent;
			this.Frame2.Controls.Add(this.ECFP4);
			this.Frame2.Controls.Add(this.Sub);
			this.Frame2.Controls.Add(this.Super);
			this.Frame2.Controls.Add(this.Normal);
			this.Frame2.Controls.Add(this.labelControl1);
			this.Frame2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame2.Location = new System.Drawing.Point(7, 3);
			this.Frame2.Name = "Frame2";
			this.Frame2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame2.Size = new System.Drawing.Size(307, 76);
			this.Frame2.TabIndex = 18;
			this.Frame2.TabStop = false;
			this.Frame2.Text = "Similarity search type";
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(3, 70);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(299, 12);
			this.labelControl1.TabIndex = 50;
			this.labelControl1.Visible = false;
			// 
			// Help
			// 
			this.Help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Help.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Help.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Help.Appearance.Options.UseFont = true;
			this.Help.Appearance.Options.UseForeColor = true;
			this.Help.Cursor = System.Windows.Forms.Cursors.Default;
			this.Help.Enabled = false;
			this.Help.Location = new System.Drawing.Point(253, 170);
			this.Help.Name = "Help";
			this.Help.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Help.Size = new System.Drawing.Size(60, 23);
			this.Help.TabIndex = 17;
			this.Help.Text = "&Help";
			// 
			// CriteriaStructureSimOptions
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(320, 199);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.SaveSimDefault);
			this.Controls.Add(this.Frame1);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Frame2);
			this.Controls.Add(this.Help);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaStructureSimOptions";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Similarity Search Options";
			((System.ComponentModel.ISupportInitialize)(this.ECFP4.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Sub.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Super.Properties)).EndInit();
			this.Frame1.ResumeLayout(false);
			this.Frame1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MinSim.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxHits.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Normal.Properties)).EndInit();
			this.Frame2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public System.Windows.Forms.ToolTip ToolTip1;
		public DevExpress.XtraEditors.CheckEdit ECFP4;
		public DevExpress.XtraEditors.SimpleButton SaveSimDefault;
		public DevExpress.XtraEditors.CheckEdit Sub;
		public DevExpress.XtraEditors.CheckEdit Super;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.TextEdit MinSim;
		public DevExpress.XtraEditors.LabelControl MinSim_Label;
		public DevExpress.XtraEditors.CheckEdit Normal;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public System.Windows.Forms.GroupBox Frame2;
		public DevExpress.XtraEditors.SimpleButton Help;
		public DevExpress.XtraEditors.TextEdit MaxHits;
		public DevExpress.XtraEditors.LabelControl MaxHits_Label;
		public DevExpress.XtraEditors.LabelControl labelControl1;
	}
}