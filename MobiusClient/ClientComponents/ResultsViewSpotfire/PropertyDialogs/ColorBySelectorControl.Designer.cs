namespace Mobius.ClientComponents
{
	partial class ColorBySelectorControl
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
			this.ColumnValueContainsColor = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
			this.BorderColor = new DevExpress.XtraEditors.ColorEdit();
			this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.FixedColor = new DevExpress.XtraEditors.ColorEdit();
			this.ColorByColumn = new DevExpress.XtraEditors.CheckEdit();
			this.ColorByFixedColor = new DevExpress.XtraEditors.CheckEdit();
			this.ColorRulesControl = new Mobius.ClientComponents.CondFormatRulesControl();
			this.ColorColumnSelector = new Mobius.SpotfireClient.ColumnsSelector();
			((System.ComponentModel.ISupportInitialize)(this.ColumnValueContainsColor.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BorderColor.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedColor.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByColumn.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByFixedColor.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// ColumnValueContainsColor
			// 
			this.ColumnValueContainsColor.AllowDrop = true;
			this.ColumnValueContainsColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ColumnValueContainsColor.Location = new System.Drawing.Point(449, 419);
			this.ColumnValueContainsColor.Name = "ColumnValueContainsColor";
			this.ColumnValueContainsColor.Properties.AutoWidth = true;
			this.ColumnValueContainsColor.Properties.Caption = "Field value contains color";
			this.ColumnValueContainsColor.Size = new System.Drawing.Size(142, 19);
			this.ColumnValueContainsColor.TabIndex = 226;
			this.ColumnValueContainsColor.EditValueChanged += new System.EventHandler(this.ColumnValueContainsColor_EditValueChanged);
			// 
			// labelControl15
			// 
			this.labelControl15.Location = new System.Drawing.Point(315, 37);
			this.labelControl15.Name = "labelControl15";
			this.labelControl15.Size = new System.Drawing.Size(141, 13);
			this.labelControl15.TabIndex = 225;
			this.labelControl15.Text = "Marker border color (hidden):";
			this.labelControl15.Visible = false;
			// 
			// BorderColor
			// 
			this.BorderColor.EditValue = System.Drawing.Color.Empty;
			this.BorderColor.Location = new System.Drawing.Point(459, 34);
			this.BorderColor.Name = "BorderColor";
			this.BorderColor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.BorderColor.Size = new System.Drawing.Size(49, 20);
			this.BorderColor.TabIndex = 224;
			this.BorderColor.Visible = false;
			this.BorderColor.EditValueChanged += new System.EventHandler(this.MarkerBorderColor_EditValueChanged);
			// 
			// labelControl11
			// 
			this.labelControl11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl11.LineVisible = true;
			this.labelControl11.Location = new System.Drawing.Point(17, 11);
			this.labelControl11.Name = "labelControl11";
			this.labelControl11.Size = new System.Drawing.Size(571, 17);
			this.labelControl11.TabIndex = 223;
			this.labelControl11.Text = "Color by";
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(17, 124);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(571, 14);
			this.labelControl2.TabIndex = 220;
			this.labelControl2.Text = "Color scheme";
			// 
			// FixedColor
			// 
			this.FixedColor.EditValue = System.Drawing.Color.Empty;
			this.FixedColor.Location = new System.Drawing.Point(106, 34);
			this.FixedColor.Name = "FixedColor";
			this.FixedColor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.FixedColor.Size = new System.Drawing.Size(49, 20);
			this.FixedColor.TabIndex = 219;
			this.FixedColor.EditValueChanged += new System.EventHandler(this.FixedColor_EditValueChanged);
			// 
			// ColorByColumn
			// 
			this.ColorByColumn.EditValue = true;
			this.ColorByColumn.Location = new System.Drawing.Point(20, 67);
			this.ColorByColumn.Name = "ColorByColumn";
			this.ColorByColumn.Properties.AutoWidth = true;
			this.ColorByColumn.Properties.Caption = "";
			this.ColorByColumn.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ColorByColumn.Properties.RadioGroupIndex = 1;
			this.ColorByColumn.Size = new System.Drawing.Size(19, 19);
			this.ColorByColumn.TabIndex = 218;
			this.ColorByColumn.EditValueChanged += new System.EventHandler(this.ColorByColumn_EditValueChanged);
			// 
			// ColorByFixedColor
			// 
			this.ColorByFixedColor.Location = new System.Drawing.Point(20, 34);
			this.ColorByFixedColor.Name = "ColorByFixedColor";
			this.ColorByFixedColor.Properties.AutoWidth = true;
			this.ColorByFixedColor.Properties.Caption = "Fixed color:";
			this.ColorByFixedColor.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ColorByFixedColor.Properties.RadioGroupIndex = 1;
			this.ColorByFixedColor.Size = new System.Drawing.Size(78, 19);
			this.ColorByFixedColor.TabIndex = 217;
			this.ColorByFixedColor.TabStop = false;
			this.ColorByFixedColor.EditValueChanged += new System.EventHandler(this.ColorByFixedColor_EditValueChanged);
			// 
			// ColorRulesControl
			// 
			this.ColorRulesControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ColorRulesControl.Location = new System.Drawing.Point(19, 144);
			this.ColorRulesControl.Name = "ColorRulesControl";
			this.ColorRulesControl.Size = new System.Drawing.Size(574, 272);
			this.ColorRulesControl.TabIndex = 222;
			this.ColorRulesControl.EditValueChanged += new System.EventHandler(this.ColorRulesControl_EditValueChanged);
			// 
			// ColorColumnSelector
			// 
			this.ColorColumnSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ColorColumnSelector.Appearance.BackColor = System.Drawing.Color.White;
			this.ColorColumnSelector.Appearance.BorderColor = System.Drawing.Color.Silver;
			this.ColorColumnSelector.Appearance.Options.UseBackColor = true;
			this.ColorColumnSelector.Appearance.Options.UseBorderColor = true;
			this.ColorColumnSelector.Location = new System.Drawing.Point(42, 62);
			this.ColorColumnSelector.Margin = new System.Windows.Forms.Padding(0);
			this.ColorColumnSelector.MetaColumn = null;
			this.ColorColumnSelector.Name = "ColorColumnSelector";
			this.ColorColumnSelector.OptionIncludeNoneItem = false;
			this.ColorColumnSelector.Size = new System.Drawing.Size(546, 29);
			this.ColorColumnSelector.TabIndex = 227;
			// 
			// ColorBySelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ColorColumnSelector);
			this.Controls.Add(this.ColumnValueContainsColor);
			this.Controls.Add(this.labelControl15);
			this.Controls.Add(this.BorderColor);
			this.Controls.Add(this.labelControl11);
			this.Controls.Add(this.ColorRulesControl);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.FixedColor);
			this.Controls.Add(this.ColorByColumn);
			this.Controls.Add(this.ColorByFixedColor);
			this.Name = "ColorBySelectorControl";
			this.Size = new System.Drawing.Size(610, 440);
			((System.ComponentModel.ISupportInitialize)(this.ColumnValueContainsColor.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BorderColor.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedColor.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByColumn.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByFixedColor.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DevExpress.XtraEditors.CheckEdit ColumnValueContainsColor;
		private DevExpress.XtraEditors.LabelControl labelControl15;
		private DevExpress.XtraEditors.ColorEdit BorderColor;
		private DevExpress.XtraEditors.LabelControl labelControl11;
		private CondFormatRulesControl ColorRulesControl;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.ColorEdit FixedColor;
		private DevExpress.XtraEditors.CheckEdit ColorByColumn;
		private DevExpress.XtraEditors.CheckEdit ColorByFixedColor;
		private SpotfireClient.ColumnsSelector ColorColumnSelector;
	}
}
