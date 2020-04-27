namespace Mobius.ClientComponents
{
	partial class InputCompoundId
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
			Mobius.Data.MoleculeMx moleculeMx1 = new Mobius.Data.MoleculeMx();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.CidCtl = new DevExpress.XtraEditors.TextEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.Timer1 = new System.Windows.Forms.Timer(this.components);
			this.QuickStructure = new Mobius.ClientComponents.MoleculeControl();
			((System.ComponentModel.ISupportInitialize)(this.CidCtl.Properties)).BeginInit();
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
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.Location = new System.Drawing.Point(243, 325);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(58, 23);
			this.OK.TabIndex = 71;
			this.OK.Text = "OK";
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseFont = true;
			this.Prompt.Appearance.Options.UseForeColor = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(6, 7);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(360, 29);
			this.Prompt.TabIndex = 73;
			this.Prompt.Text = "Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt Prompt ";
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
			this.Cancel.Location = new System.Drawing.Point(307, 325);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(59, 23);
			this.Cancel.TabIndex = 72;
			this.Cancel.Text = "Cancel";
			// 
			// CidCtl
			// 
			this.CidCtl.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.CidCtl.Location = new System.Drawing.Point(6, 42);
			this.CidCtl.Name = "CidCtl";
			this.CidCtl.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.CidCtl.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CidCtl.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.CidCtl.Properties.Appearance.Options.UseBackColor = true;
			this.CidCtl.Properties.Appearance.Options.UseFont = true;
			this.CidCtl.Properties.Appearance.Options.UseForeColor = true;
			this.CidCtl.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CidCtl.Size = new System.Drawing.Size(234, 20);
			this.CidCtl.TabIndex = 70;
			this.CidCtl.Tag = "RegNo";
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.labelControl1.Appearance.Options.UseBackColor = true;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.labelControl1.Location = new System.Drawing.Point(6, 67);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(43, 13);
			this.labelControl1.TabIndex = 75;
			this.labelControl1.Text = "Structure";
			// 
			// Timer1
			// 
			this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
			// 
			// QuickStructure
			// 
			this.QuickStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QuickStructure.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.QuickStructure.Location = new System.Drawing.Point(6, 88);
			this.QuickStructure.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			moleculeMx1.BackColor = System.Drawing.Color.Empty;
			moleculeMx1.DateTimeValue = new System.DateTime(((long)(0)));
			moleculeMx1.DbLink = "";
			moleculeMx1.Filtered = false;
			moleculeMx1.ForeColor = System.Drawing.Color.Black;
			moleculeMx1.FormattedBitmap = null;
			moleculeMx1.FormattedText = null;
			moleculeMx1.Hyperlink = "";
			moleculeMx1.Hyperlinked = false;
			moleculeMx1.IsNonExistant = false;
			moleculeMx1.IsRetrievingValue = false;
			moleculeMx1.Modified = false;
			moleculeMx1.NumericValue = -4194303D;
			this.QuickStructure.Molecule = moleculeMx1;
			this.QuickStructure.Name = "QuickStructure";
			this.QuickStructure.Size = new System.Drawing.Size(360, 230);
			this.QuickStructure.TabIndex = 76;
			// 
			// InputCompoundId
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(373, 352);
			this.Controls.Add(this.QuickStructure);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.CidCtl);
			this.MinimizeBox = false;
			this.Name = "InputCompoundId";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "InputCompoundId";
			this.Activated += new System.EventHandler(this.InputCompoundId_Activated);
			this.Deactivate += new System.EventHandler(this.InputCompoundId_Deactivate);
			((System.ComponentModel.ISupportInitialize)(this.CidCtl.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.TextEdit CidCtl;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public System.Windows.Forms.Timer Timer1;
		private MoleculeControl QuickStructure;
	}
}