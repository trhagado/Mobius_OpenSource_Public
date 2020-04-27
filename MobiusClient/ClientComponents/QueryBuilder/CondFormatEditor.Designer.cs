namespace Mobius.ClientComponents
{
	partial class CondFormatEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CondFormatEditor));
			this.CfNameLabel2 = new DevExpress.XtraEditors.LabelControl();
			this.CfName = new DevExpress.XtraEditors.TextEdit();
			this.CfNameLabel = new DevExpress.XtraEditors.LabelControl();
			this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
			this.OKButton = new DevExpress.XtraEditors.SimpleButton();
			this.SmallBitmaps = new System.Windows.Forms.ImageList(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.Option1 = new DevExpress.XtraEditors.CheckEdit();
			this.Option2 = new DevExpress.XtraEditors.CheckEdit();
			this.Rules = new Mobius.ClientComponents.CondFormatRulesControl();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.ColumnDataType = new DevExpress.XtraEditors.ComboBoxEdit();
			this.ColumnTypeLabel = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.CfName.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Option1.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Option2.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColumnDataType.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// CfNameLabel2
			// 
			this.CfNameLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CfNameLabel2.Location = new System.Drawing.Point(234, 380);
			this.CfNameLabel2.Name = "CfNameLabel2";
			this.CfNameLabel2.Size = new System.Drawing.Size(236, 13);
			this.CfNameLabel2.TabIndex = 196;
			this.CfNameLabel2.Text = "(Optional: If assigned allows reuse of formatting)";
			// 
			// CfName
			// 
			this.CfName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CfName.Location = new System.Drawing.Point(43, 377);
			this.CfName.Name = "CfName";
			this.CfName.Size = new System.Drawing.Size(185, 20);
			this.CfName.TabIndex = 195;
			// 
			// CfNameLabel
			// 
			this.CfNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CfNameLabel.Location = new System.Drawing.Point(4, 379);
			this.CfNameLabel.Name = "CfNameLabel";
			this.CfNameLabel.Size = new System.Drawing.Size(31, 13);
			this.CfNameLabel.TabIndex = 194;
			this.CfNameLabel.Text = "Name:";
			// 
			// CancelBut
			// 
			this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBut.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBut.Location = new System.Drawing.Point(481, 408);
			this.CancelBut.Name = "CancelBut";
			this.CancelBut.Size = new System.Drawing.Size(70, 24);
			this.CancelBut.TabIndex = 191;
			this.CancelBut.Text = "Cancel";
			this.CancelBut.Click += new System.EventHandler(this.CancelBut_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = new System.Drawing.Point(405, 408);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(70, 24);
			this.OKButton.TabIndex = 192;
			this.OKButton.Text = "&OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// SmallBitmaps
			// 
			this.SmallBitmaps.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SmallBitmaps.ImageStream")));
			this.SmallBitmaps.TransparentColor = System.Drawing.Color.Cyan;
			this.SmallBitmaps.Images.SetKeyName(0, "Add.bmp");
			this.SmallBitmaps.Images.SetKeyName(1, "Delete.bmp");
			this.SmallBitmaps.Images.SetKeyName(2, "up2.bmp");
			this.SmallBitmaps.Images.SetKeyName(3, "down2.bmp");
			// 
			// Option1
			// 
			this.Option1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Option1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option1.EditValue = true;
			this.Option1.Location = new System.Drawing.Point(7, 328);
			this.Option1.Name = "Option1";
			this.Option1.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Option1.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Option1.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option1.Properties.Appearance.Options.UseBackColor = true;
			this.Option1.Properties.Appearance.Options.UseFont = true;
			this.Option1.Properties.Appearance.Options.UseForeColor = true;
			this.Option1.Properties.Caption = "Option 1";
			this.Option1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option1.Size = new System.Drawing.Size(312, 19);
			this.Option1.TabIndex = 199;
			// 
			// Option2
			// 
			this.Option2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Option2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option2.EditValue = true;
			this.Option2.Location = new System.Drawing.Point(7, 350);
			this.Option2.Name = "Option2";
			this.Option2.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Option2.Properties.Appearance.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Option2.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option2.Properties.Appearance.Options.UseBackColor = true;
			this.Option2.Properties.Appearance.Options.UseFont = true;
			this.Option2.Properties.Appearance.Options.UseForeColor = true;
			this.Option2.Properties.Caption = "Option 2";
			this.Option2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option2.Size = new System.Drawing.Size(312, 19);
			this.Option2.TabIndex = 198;
			// 
			// Rules
			// 
			this.Rules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Rules.Location = new System.Drawing.Point(0, 5);
			this.Rules.Name = "Rules";
			this.Rules.Size = new System.Drawing.Size(555, 317);
			this.Rules.TabIndex = 197;
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-3, 398);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(559, 6);
			this.labelControl1.TabIndex = 200;
			// 
			// ColumnDataType
			// 
			this.ColumnDataType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ColumnDataType.EditValue = "Number";
			this.ColumnDataType.Location = new System.Drawing.Point(210, 410);
			this.ColumnDataType.Name = "ColumnDataType";
			this.ColumnDataType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.ColumnDataType.Properties.Items.AddRange(new object[] {
            "Number",
            "Integer",
            "Text",
            "Date"});
			this.ColumnDataType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.ColumnDataType.Properties.Enter += new System.EventHandler(this.JoinAnchorComboBox_Properties_Enter);
			this.ColumnDataType.Size = new System.Drawing.Size(121, 20);
			this.ColumnDataType.TabIndex = 219;
			this.ColumnDataType.Visible = false;
			// 
			// ColumnTypeLabel
			// 
			this.ColumnTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ColumnTypeLabel.Location = new System.Drawing.Point(6, 413);
			this.ColumnTypeLabel.Name = "ColumnTypeLabel";
			this.ColumnTypeLabel.Size = new System.Drawing.Size(199, 13);
			this.ColumnTypeLabel.TabIndex = 220;
			this.ColumnTypeLabel.Text = "Data type of the column to be formatted:";
			this.ColumnTypeLabel.Visible = false;
			// 
			// CondFormatEditor
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelBut;
			this.ClientSize = new System.Drawing.Size(554, 435);
			this.Controls.Add(this.ColumnTypeLabel);
			this.Controls.Add(this.ColumnDataType);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.Option1);
			this.Controls.Add(this.Option2);
			this.Controls.Add(this.Rules);
			this.Controls.Add(this.CfNameLabel2);
			this.Controls.Add(this.CfName);
			this.Controls.Add(this.CfNameLabel);
			this.Controls.Add(this.CancelBut);
			this.Controls.Add(this.OKButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CondFormatEditor";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Conditional Formatting Rules";
			this.Activated += new System.EventHandler(this.CondFormatForm_Activated);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CondFormatForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.CfName.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Option1.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Option2.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColumnDataType.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl CfNameLabel2;
		public DevExpress.XtraEditors.TextEdit CfName;
		private DevExpress.XtraEditors.LabelControl CfNameLabel;
		public DevExpress.XtraEditors.SimpleButton CancelBut;
		public DevExpress.XtraEditors.SimpleButton OKButton;
		public System.Windows.Forms.ImageList SmallBitmaps;
		private System.Windows.Forms.ToolTip toolTip1;
		private CondFormatRulesControl Rules;
		public DevExpress.XtraEditors.CheckEdit Option1;
		public DevExpress.XtraEditors.CheckEdit Option2;
		private DevExpress.XtraEditors.LabelControl labelControl1;
		private DevExpress.XtraEditors.ComboBoxEdit ColumnDataType;
		private DevExpress.XtraEditors.LabelControl ColumnTypeLabel;
	}
}