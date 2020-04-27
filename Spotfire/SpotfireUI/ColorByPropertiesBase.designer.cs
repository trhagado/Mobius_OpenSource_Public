namespace Mobius.SpotfireClient
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
			this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
			this.BorderColor = new DevExpress.XtraEditors.ColorEdit();
			this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
			this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
			this.FixedColor = new DevExpress.XtraEditors.ColorEdit();
			this.ColorByColumn = new DevExpress.XtraEditors.CheckEdit();
			this.ColumnsSelector = new Mobius.SpotfireClient.ColumnSelectorControl();
			this.ColorByFixedColor = new DevExpress.XtraEditors.CheckEdit();
			((System.ComponentModel.ISupportInitialize)(this.BorderColor.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedColor.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByColumn.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByFixedColor.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// labelControl15
			// 
			this.labelControl15.Location = new System.Drawing.Point(345, 77);
			this.labelControl15.Name = "labelControl15";
			this.labelControl15.Size = new System.Drawing.Size(141, 13);
			this.labelControl15.TabIndex = 225;
			this.labelControl15.Text = "Marker border color (hidden):";
			this.labelControl15.Visible = false;
			// 
			// BorderColor
			// 
			this.BorderColor.EditValue = System.Drawing.Color.Empty;
			this.BorderColor.Location = new System.Drawing.Point(489, 74);
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
			this.labelControl11.Location = new System.Drawing.Point(7, 4);
			this.labelControl11.Name = "labelControl11";
			this.labelControl11.Size = new System.Drawing.Size(44, 13);
			this.labelControl11.TabIndex = 223;
			this.labelControl11.Text = "Columns:";
			// 
			// labelControl2
			// 
			this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl2.LineVisible = true;
			this.labelControl2.Location = new System.Drawing.Point(7, 109);
			this.labelControl2.Name = "labelControl2";
			this.labelControl2.Size = new System.Drawing.Size(574, 14);
			this.labelControl2.TabIndex = 220;
			this.labelControl2.Text = "Color scheme (todo)";
			// 
			// FixedColor
			// 
			this.FixedColor.EditValue = System.Drawing.Color.Empty;
			this.FixedColor.Location = new System.Drawing.Point(96, 74);
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
			this.ColorByColumn.Location = new System.Drawing.Point(8, 27);
			this.ColorByColumn.Name = "ColorByColumn";
			this.ColorByColumn.Properties.AutoWidth = true;
			this.ColorByColumn.Properties.Caption = "";
			this.ColorByColumn.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ColorByColumn.Properties.RadioGroupIndex = 1;
			this.ColorByColumn.Size = new System.Drawing.Size(19, 19);
			this.ColorByColumn.TabIndex = 218;
			this.ColorByColumn.EditValueChanged += new System.EventHandler(this.ColorByColumn_EditValueChanged);
			// 
			// ColumnsSelector
			// 
			this.ColumnsSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ColumnsSelector.Appearance.BackColor = System.Drawing.Color.White;
			this.ColumnsSelector.Appearance.BorderColor = System.Drawing.Color.Silver;
			this.ColumnsSelector.Appearance.Options.UseBackColor = true;
			this.ColumnsSelector.Appearance.Options.UseBorderColor = true;
			this.ColumnsSelector.Location = new System.Drawing.Point(31, 27);
			this.ColumnsSelector.Margin = new System.Windows.Forms.Padding(0);
			this.ColumnsSelector.Name = "ColumnsSelector";
			this.ColumnsSelector.OptionIncludeNoneItem = false;
			this.ColumnsSelector.Size = new System.Drawing.Size(575, 27);
			this.ColumnsSelector.TabIndex = 226;
			// 
			// ColorByFixedColor
			// 
			this.ColorByFixedColor.Location = new System.Drawing.Point(8, 73);
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
			// ColorByBaseProperties
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ColumnsSelector);
			this.Controls.Add(this.labelControl11);
			this.Controls.Add(this.labelControl15);
			this.Controls.Add(this.FixedColor);
			this.Controls.Add(this.ColorByFixedColor);
			this.Controls.Add(this.labelControl2);
			this.Controls.Add(this.BorderColor);
			this.Controls.Add(this.ColorByColumn);
			this.Name = "ColorByBaseProperties";
			this.Size = new System.Drawing.Size(610, 440);
			((System.ComponentModel.ISupportInitialize)(this.BorderColor.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FixedColor.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByColumn.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorByFixedColor.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private DevExpress.XtraEditors.LabelControl labelControl15;
		private DevExpress.XtraEditors.ColorEdit BorderColor;
		private DevExpress.XtraEditors.LabelControl labelControl11;
		private DevExpress.XtraEditors.LabelControl labelControl2;
		private DevExpress.XtraEditors.ColorEdit FixedColor;
		private DevExpress.XtraEditors.CheckEdit ColorByColumn;
		private ColumnSelectorControl ColumnsSelector;
		private DevExpress.XtraEditors.CheckEdit ColorByFixedColor;
	}
}
