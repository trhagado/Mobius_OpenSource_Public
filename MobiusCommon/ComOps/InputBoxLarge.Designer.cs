namespace Mobius.ComOps
{
	partial class InputBoxLarge
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
			this.Input = new DevExpress.XtraEditors.MemoEdit();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			((System.ComponentModel.ISupportInitialize)(this.Input.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Input
			// 
			this.Input.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Input.Location = new System.Drawing.Point(6, 31);
			this.Input.Name = "Input";
			this.Input.Size = new System.Drawing.Size(699, 495);
			this.Input.TabIndex = 45;
			// 
			// Prompt
			// 
			this.Prompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Location = new System.Drawing.Point(6, 4);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(699, 22);
			this.Prompt.TabIndex = 46;
			this.Prompt.Text = "Prompt";
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(646, 532);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(60, 22);
			this.Cancel.TabIndex = 48;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(574, 532);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(60, 22);
			this.OK.TabIndex = 47;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// InputBoxLarge
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(710, 558);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.Input);
			this.MinimizeBox = false;
			this.Name = "InputBoxLarge";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "InputBoxLarge";
			((System.ComponentModel.ISupportInitialize)(this.Input.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraEditors.MemoEdit Input;
		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
	}
}