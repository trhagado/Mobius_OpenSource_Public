namespace Mobius.ClientComponents
{
	partial class EditFind
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
			this.MatchEntireCell = new DevExpress.XtraEditors.CheckEdit();
			this.MatchCase = new DevExpress.XtraEditors.CheckEdit();
			this.Progress = new DevExpress.XtraEditors.LabelControl();
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.FindNext = new DevExpress.XtraEditors.SimpleButton();
			this.CloseButton = new DevExpress.XtraEditors.SimpleButton();
			this.FindText = new DevExpress.XtraEditors.MRUEdit();
			((System.ComponentModel.ISupportInitialize)(this.MatchEntireCell.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchCase.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FindText.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// MatchEntireCell
			// 
			this.MatchEntireCell.Location = new System.Drawing.Point(62, 54);
			this.MatchEntireCell.Name = "MatchEntireCell";
			this.MatchEntireCell.Properties.Caption = "Match entire cell contents";
			this.MatchEntireCell.Size = new System.Drawing.Size(160, 18);
			this.MatchEntireCell.TabIndex = 93;
			// 
			// MatchCase
			// 
			this.MatchCase.Location = new System.Drawing.Point(62, 34);
			this.MatchCase.Name = "MatchCase";
			this.MatchCase.Properties.Caption = "Match case";
			this.MatchCase.Size = new System.Drawing.Size(84, 18);
			this.MatchCase.TabIndex = 92;
			// 
			// Progress
			// 
			this.Progress.Location = new System.Drawing.Point(6, 86);
			this.Progress.Name = "Progress";
			this.Progress.Size = new System.Drawing.Size(107, 13);
			this.Progress.TabIndex = 91;
			this.Progress.Text = "Searching row 1234...";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(4, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(51, 13);
			this.label1.TabIndex = 90;
			this.label1.Text = "Find what:";
			// 
			// FindNext
			// 
			this.FindNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FindNext.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FindNext.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FindNext.Appearance.Options.UseFont = true;
			this.FindNext.Appearance.Options.UseForeColor = true;
			this.FindNext.Cursor = System.Windows.Forms.Cursors.Default;
			this.FindNext.Location = new System.Drawing.Point(156, 82);
			this.FindNext.Name = "FindNext";
			this.FindNext.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FindNext.Size = new System.Drawing.Size(76, 24);
			this.FindNext.TabIndex = 89;
			this.FindNext.Text = "&Find Next";
			this.FindNext.Click += new System.EventHandler(this.FindNext_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CloseButton.Appearance.Options.UseFont = true;
			this.CloseButton.Appearance.Options.UseForeColor = true;
			this.CloseButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(238, 82);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CloseButton.Size = new System.Drawing.Size(76, 24);
			this.CloseButton.TabIndex = 88;
			this.CloseButton.Text = "Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FindText
			// 
			this.FindText.Location = new System.Drawing.Point(61, 11);
			this.FindText.Name = "FindText";
			this.FindText.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.FindText.Size = new System.Drawing.Size(251, 20);
			this.FindText.TabIndex = 94;
			this.FindText.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FindText_KeyUp);
			// 
			// EditFind
			// 
			this.AcceptButton = this.FindNext;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CloseButton;
			this.ClientSize = new System.Drawing.Size(318, 111);
			this.Controls.Add(this.MatchEntireCell);
			this.Controls.Add(this.MatchCase);
			this.Controls.Add(this.Progress);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.FindNext);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.FindText);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditFind";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Find";
			this.Deactivate += new System.EventHandler(this.EditFind_Deactivate);
			this.Activated += new System.EventHandler(this.EditFind_Activated);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditFind_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.MatchEntireCell.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MatchCase.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FindText.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.CheckEdit MatchEntireCell;
		private DevExpress.XtraEditors.CheckEdit MatchCase;
		private DevExpress.XtraEditors.LabelControl Progress;
		private DevExpress.XtraEditors.LabelControl label1;
		public DevExpress.XtraEditors.SimpleButton FindNext;
		public DevExpress.XtraEditors.SimpleButton CloseButton;
		private DevExpress.XtraEditors.MRUEdit FindText;
	}
}