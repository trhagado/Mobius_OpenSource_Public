namespace Mobius.ClientComponents
{
	partial class FieldSelectorDialog
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
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.FSC = new Mobius.ClientComponents.FieldSelectorControl();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(304, 102);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(58, 23);
			this.OK.TabIndex = 74;
			this.OK.Text = "OK";
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(6, 7);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(360, 29);
			this.Prompt.TabIndex = 76;
			this.Prompt.Text = "Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt ";
			// 
			// Cancel
			// 
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(368, 102);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(59, 23);
			this.Cancel.TabIndex = 75;
			this.Cancel.Text = "Cancel";
			// 
			// FSC
			// 
			this.FSC.Location = new System.Drawing.Point(6, 50);
			this.FSC.MetaColumn = null;
			this.FSC.Name = "FSC";
			this.FSC.Size = new System.Drawing.Size(425, 46);
			this.FSC.TabIndex = 77;
			// 
			// FieldSelectorDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(435, 134);
			this.Controls.Add(this.FSC);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.Cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FieldSelectorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FieldSelectorDialog";
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		private FieldSelectorControl FSC;
	}
}