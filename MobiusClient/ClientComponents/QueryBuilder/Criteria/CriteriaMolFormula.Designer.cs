namespace Mobius.ClientComponents
{
	partial class CriteriaMolFormula
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
			this.ExactMF = new DevExpress.XtraEditors.CheckEdit();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.Formula = new DevExpress.XtraEditors.TextEdit();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.PartialMF = new DevExpress.XtraEditors.CheckEdit();
			this.Frame5 = new System.Windows.Forms.GroupBox();
			this.Prompt2 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.ExactMF.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PartialMF.Properties)).BeginInit();
			this.Frame5.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExactMF
			// 
			this.ExactMF.Cursor = System.Windows.Forms.Cursors.Default;
			this.ExactMF.EditValue = true;
			this.ExactMF.Location = new System.Drawing.Point(7, 16);
			this.ExactMF.Name = "ExactMF";
			this.ExactMF.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ExactMF.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExactMF.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ExactMF.Properties.Appearance.Options.UseBackColor = true;
			this.ExactMF.Properties.Appearance.Options.UseFont = true;
			this.ExactMF.Properties.Appearance.Options.UseForeColor = true;
			this.ExactMF.Properties.Caption = "Exact match of the complete formula";
			this.ExactMF.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ExactMF.Properties.RadioGroupIndex = 1;
			this.ExactMF.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ExactMF.Size = new System.Drawing.Size(267, 18);
			this.ExactMF.TabIndex = 7;
			// 
			// Cancel
			// 
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(311, 146);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(59, 22);
			this.Cancel.TabIndex = 14;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			// 
			// Formula
			// 
			this.Formula.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Formula.Location = new System.Drawing.Point(7, 78);
			this.Formula.Name = "Formula";
			this.Formula.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Formula.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Formula.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Formula.Properties.Appearance.Options.UseBackColor = true;
			this.Formula.Properties.Appearance.Options.UseFont = true;
			this.Formula.Properties.Appearance.Options.UseForeColor = true;
			this.Formula.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Formula.Size = new System.Drawing.Size(363, 19);
			this.Formula.TabIndex = 12;
			this.Formula.Tag = "Formula";
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseFont = true;
			this.Prompt.Appearance.Options.UseForeColor = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(7, 6);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(363, 41);
			this.Prompt.TabIndex = 15;
			this.Prompt.Tag = "Prompt";
			this.Prompt.Text = "Enter the Molecular formula that you want to search for specifying allowable coun" +
					"t values and/or ranges for each element. Separate the counts for adjacent elemen" +
					"ts with a space.";
			// 
			// OK
			// 
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(311, 118);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(59, 22);
			this.OK.TabIndex = 13;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			// 
			// PartialMF
			// 
			this.PartialMF.Cursor = System.Windows.Forms.Cursors.Default;
			this.PartialMF.Location = new System.Drawing.Point(7, 40);
			this.PartialMF.Name = "PartialMF";
			this.PartialMF.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.PartialMF.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PartialMF.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.PartialMF.Properties.Appearance.Options.UseBackColor = true;
			this.PartialMF.Properties.Appearance.Options.UseFont = true;
			this.PartialMF.Properties.Appearance.Options.UseForeColor = true;
			this.PartialMF.Properties.Caption = "Subformula match of only the specified elements";
			this.PartialMF.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.PartialMF.Properties.RadioGroupIndex = 1;
			this.PartialMF.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.PartialMF.Size = new System.Drawing.Size(283, 18);
			this.PartialMF.TabIndex = 6;
			this.PartialMF.TabStop = false;
			// 
			// Frame5
			// 
			this.Frame5.BackColor = System.Drawing.Color.Transparent;
			this.Frame5.Controls.Add(this.ExactMF);
			this.Frame5.Controls.Add(this.PartialMF);
			this.Frame5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame5.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame5.Location = new System.Drawing.Point(7, 102);
			this.Frame5.Name = "Frame5";
			this.Frame5.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame5.Size = new System.Drawing.Size(296, 66);
			this.Frame5.TabIndex = 17;
			this.Frame5.TabStop = false;
			this.Frame5.Text = "Type of Formula Search ";
			// 
			// Prompt2
			// 
			this.Prompt2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt2.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Prompt2.Appearance.Options.UseBackColor = true;
			this.Prompt2.Appearance.Options.UseFont = true;
			this.Prompt2.Appearance.Options.UseForeColor = true;
			this.Prompt2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt2.Location = new System.Drawing.Point(7, 54);
			this.Prompt2.Name = "Prompt2";
			this.Prompt2.Size = new System.Drawing.Size(176, 13);
			this.Prompt2.TabIndex = 16;
			this.Prompt2.Tag = "Prompt2";
			this.Prompt2.Text = "  Examples: C6 H6 O,  C12 Br(1-2) S2";
			// 
			// CriteriaMolFormula
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(376, 175);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Formula);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Frame5);
			this.Controls.Add(this.Prompt2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaMolFormula";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Molecular Formula Criteria";
			this.Activated += new System.EventHandler(this.CriteriaMolFormula_Activated);
			((System.ComponentModel.ISupportInitialize)(this.ExactMF.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Formula.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PartialMF.Properties)).EndInit();
			this.Frame5.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.CheckEdit ExactMF;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.TextEdit Formula;
		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.CheckEdit PartialMF;
		public System.Windows.Forms.GroupBox Frame5;
		public DevExpress.XtraEditors.LabelControl Prompt2;
	}
}