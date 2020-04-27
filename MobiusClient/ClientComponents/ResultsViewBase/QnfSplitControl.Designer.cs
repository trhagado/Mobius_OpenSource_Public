namespace Mobius.ClientComponents
{
	partial class QnfSplitControl
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
			this.QnfSplitEdit = new DevExpress.XtraEditors.SimpleButton();
			this.QnfSplitFormat = new DevExpress.XtraEditors.TextEdit();
			this.QnfSplit = new DevExpress.XtraEditors.CheckEdit();
			this.QnfCombined = new DevExpress.XtraEditors.CheckEdit();
			((System.ComponentModel.ISupportInitialize)(this.QnfSplitFormat.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QnfSplit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QnfCombined.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// QnfSplitEdit
			// 
			this.QnfSplitEdit.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QnfSplitEdit.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QnfSplitEdit.Appearance.Options.UseFont = true;
			this.QnfSplitEdit.Appearance.Options.UseForeColor = true;
			this.QnfSplitEdit.Cursor = System.Windows.Forms.Cursors.Default;
			this.QnfSplitEdit.Location = new System.Drawing.Point(291, 2);
			this.QnfSplitEdit.Name = "QnfSplitEdit";
			this.QnfSplitEdit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QnfSplitEdit.Size = new System.Drawing.Size(52, 21);
			this.QnfSplitEdit.TabIndex = 40;
			this.QnfSplitEdit.Tag = "OK";
			this.QnfSplitEdit.Text = "Edit...";
			this.QnfSplitEdit.Click += new System.EventHandler(this.QnfSplitEdit_Click);
			// 
			// QnfSplitFormat
			// 
			this.QnfSplitFormat.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.QnfSplitFormat.EditValue = "Qualifier, Number";
			this.QnfSplitFormat.Location = new System.Drawing.Point(115, 3);
			this.QnfSplitFormat.Name = "QnfSplitFormat";
			this.QnfSplitFormat.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.QnfSplitFormat.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QnfSplitFormat.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.QnfSplitFormat.Properties.Appearance.Options.UseBackColor = true;
			this.QnfSplitFormat.Properties.Appearance.Options.UseFont = true;
			this.QnfSplitFormat.Properties.Appearance.Options.UseForeColor = true;
			this.QnfSplitFormat.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QnfSplitFormat.Size = new System.Drawing.Size(168, 20);
			this.QnfSplitFormat.TabIndex = 39;
			this.QnfSplitFormat.Tag = "Title";
			this.QnfSplitFormat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QnfSplitFormat_KeyDown);
			this.QnfSplitFormat.MouseClick += new System.Windows.Forms.MouseEventHandler(this.QnfSplitFormat_MouseClick);
			this.QnfSplitFormat.MouseDown += new System.Windows.Forms.MouseEventHandler(this.QnfSplitFormat_MouseDown);
			// 
			// QnfSplit
			// 
			this.QnfSplit.Cursor = System.Windows.Forms.Cursors.Default;
			this.QnfSplit.EditValue = true;
			this.QnfSplit.Location = new System.Drawing.Point(3, 3);
			this.QnfSplit.Name = "QnfSplit";
			this.QnfSplit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.QnfSplit.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QnfSplit.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QnfSplit.Properties.Appearance.Options.UseBackColor = true;
			this.QnfSplit.Properties.Appearance.Options.UseFont = true;
			this.QnfSplit.Properties.Appearance.Options.UseForeColor = true;
			this.QnfSplit.Properties.Caption = "Split into columns:";
			this.QnfSplit.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.QnfSplit.Properties.RadioGroupIndex = 1;
			this.QnfSplit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QnfSplit.Size = new System.Drawing.Size(114, 19);
			this.QnfSplit.TabIndex = 38;
			this.QnfSplit.CheckedChanged += new System.EventHandler(this.QnfSplit_CheckedChanged);
			// 
			// QnfCombined
			// 
			this.QnfCombined.Cursor = System.Windows.Forms.Cursors.Default;
			this.QnfCombined.Location = new System.Drawing.Point(3, 28);
			this.QnfCombined.Name = "QnfCombined";
			this.QnfCombined.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.QnfCombined.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QnfCombined.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QnfCombined.Properties.Appearance.Options.UseBackColor = true;
			this.QnfCombined.Properties.Appearance.Options.UseFont = true;
			this.QnfCombined.Properties.Appearance.Options.UseForeColor = true;
			this.QnfCombined.Properties.Caption = "Single text column (e.g. \">50\")";
			this.QnfCombined.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.QnfCombined.Properties.RadioGroupIndex = 1;
			this.QnfCombined.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.QnfCombined.Size = new System.Drawing.Size(235, 19);
			this.QnfCombined.TabIndex = 37;
			this.QnfCombined.TabStop = false;
			// 
			// QnfSplitControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.QnfSplitEdit);
			this.Controls.Add(this.QnfSplitFormat);
			this.Controls.Add(this.QnfCombined);
			this.Controls.Add(this.QnfSplit);
			this.Name = "QnfSplitControl";
			this.Size = new System.Drawing.Size(348, 47);
			((System.ComponentModel.ISupportInitialize)(this.QnfSplitFormat.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QnfSplit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QnfCombined.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton QnfSplitEdit;
		public DevExpress.XtraEditors.TextEdit QnfSplitFormat;
		public DevExpress.XtraEditors.CheckEdit QnfSplit;
		public DevExpress.XtraEditors.CheckEdit QnfCombined;
	}
}
