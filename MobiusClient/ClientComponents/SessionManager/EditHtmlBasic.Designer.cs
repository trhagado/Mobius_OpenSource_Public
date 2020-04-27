namespace Mobius.ClientComponents
{
	partial class EditHtmlBasic
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
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.HtmlTextCtl = new DevExpress.XtraEditors.MemoEdit();
			this.label2 = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.PreviewControl = new DevExpress.XtraEditors.HyperlinkLabelControl();
			((System.ComponentModel.ISupportInitialize)(this.HtmlTextCtl.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelControl2.Location = new System.Drawing.Point(10, 153);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(42, 13);
			this.labelControl2.TabIndex = 56;
			this.labelControl2.Text = "Preview:";
			// 
			// HtmlTextCtl
			// 
			this.HtmlTextCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HtmlTextCtl.Location = new System.Drawing.Point(7, 31);
			this.HtmlTextCtl.Name = "HtmlTextCtl";
			this.HtmlTextCtl.Properties.EditValueChangedDelay = 500;
			this.HtmlTextCtl.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
			this.HtmlTextCtl.Size = new System.Drawing.Size(538, 115);
			this.HtmlTextCtl.TabIndex = 55;
			this.HtmlTextCtl.EditValueChanged += new System.EventHandler(this.HtmlTextCtl_EditValueChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 13);
			this.label2.TabIndex = 51;
			this.label2.Text = "HTML:";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(397, 264);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.ShowToolTips = false;
			this.OK.Size = new System.Drawing.Size(68, 23);
			this.OK.TabIndex = 50;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
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
			this.Cancel.Location = new System.Drawing.Point(477, 264);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 23);
			this.Cancel.TabIndex = 49;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(1, 249);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(548, 12);
			this.labelControl1.TabIndex = 53;
			// 
			// PreviewControl
			// 
			this.PreviewControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewControl.Appearance.BackColor = System.Drawing.Color.White;
			this.PreviewControl.Appearance.Options.UseBackColor = true;
			this.PreviewControl.Appearance.Options.UseTextOptions = true;
			this.PreviewControl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.PreviewControl.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.PreviewControl.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.PreviewControl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.PreviewControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
			this.PreviewControl.Cursor = System.Windows.Forms.Cursors.Default;
			this.PreviewControl.Location = new System.Drawing.Point(10, 178);
			this.PreviewControl.Name = "PreviewControl";
			this.PreviewControl.Size = new System.Drawing.Size(535, 65);
			this.PreviewControl.TabIndex = 57;
			this.PreviewControl.HyperlinkClick += new DevExpress.Utils.HyperlinkClickEventHandler(this.PreviewControl_HyperlinkClick);
			// 
			// EditHtmlBasic
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(551, 294);
			this.Controls.Add(this.PreviewControl);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.HtmlTextCtl);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelControl1);
			this.MinimizeBox = false;
			this.Name = "EditHtmlBasic";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Edit HTML";
			this.Activated += new System.EventHandler(this.EditHtmlTextFormatting_Activated);
			((System.ComponentModel.ISupportInitialize)(this.HtmlTextCtl.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.LabelControl labelControl2;
		internal DevExpress.XtraEditors.MemoEdit HtmlTextCtl;
		public DevExpress.XtraEditors.LabelControl label2;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.HyperlinkLabelControl PreviewControl;
	}
}