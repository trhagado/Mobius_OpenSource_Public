namespace Mobius.ClientComponents
{
	partial class CalcFieldColumnControl
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
			this.fieldLabel = new System.Windows.Forms.GroupBox();
			this.Constant = new DevExpress.XtraEditors.TextEdit();
			this.ConstantLabel1 = new DevExpress.XtraEditors.LabelControl();
			this.label4 = new DevExpress.XtraEditors.LabelControl();
			this.Function = new DevExpress.XtraEditors.ComboBoxEdit();
			this.FieldSelectorControl = new Mobius.ClientComponents.FieldSelectorControl();
			this.fieldLabel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Constant.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Function.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// fieldLabel
			// 
			this.fieldLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fieldLabel.Controls.Add(this.FieldSelectorControl);
			this.fieldLabel.Controls.Add(this.Constant);
			this.fieldLabel.Controls.Add(this.ConstantLabel1);
			this.fieldLabel.Controls.Add(this.label4);
			this.fieldLabel.Controls.Add(this.Function);
			this.fieldLabel.Location = new System.Drawing.Point(0, 0);
			this.fieldLabel.Name = "fieldLabel";
			this.fieldLabel.Size = new System.Drawing.Size(464, 106);
			this.fieldLabel.TabIndex = 15;
			this.fieldLabel.TabStop = false;
			this.fieldLabel.Text = "Data Field";
			// 
			// Constant
			// 
			this.Constant.Location = new System.Drawing.Point(321, 74);
			this.Constant.Name = "Constant";
			this.Constant.Size = new System.Drawing.Size(109, 20);
			this.Constant.TabIndex = 18;
			// 
			// ConstantLabel1
			// 
			this.ConstantLabel1.Location = new System.Drawing.Point(267, 77);
			this.ConstantLabel1.Name = "ConstantLabel1";
			this.ConstantLabel1.Size = new System.Drawing.Size(48, 13);
			this.ConstantLabel1.TabIndex = 8;
			this.ConstantLabel1.Text = "Constant:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(21, 76);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(45, 13);
			this.label4.TabIndex = 12;
			this.label4.Text = "Function:";
			// 
			// Function
			// 
			this.Function.Location = new System.Drawing.Point(72, 75);
			this.Function.Name = "Function";
			this.Function.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.Function.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.Function.Size = new System.Drawing.Size(180, 20);
			this.Function.TabIndex = 19;
			this.Function.SelectedValueChanged += new System.EventHandler(this.Function_SelectedValueChanged);
			// 
			// Column
			// 
			this.FieldSelectorControl.Location = new System.Drawing.Point(28, 22);
			this.FieldSelectorControl.QueryColumn = null;
			this.FieldSelectorControl.Name = "Column";
			this.FieldSelectorControl.OptionIncludeAllSelectableColumns = true;
			this.FieldSelectorControl.Size = new System.Drawing.Size(425, 46);
			this.FieldSelectorControl.TabIndex = 20;
			this.FieldSelectorControl.CallerEditValueChangedHandler += new System.EventHandler(this.Column_EditValueChanged);
			// 
			// CalcFieldColumnControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.fieldLabel);
			this.Name = "CalcFieldColumnControl";
			this.Size = new System.Drawing.Size(466, 107);
			this.fieldLabel.ResumeLayout(false);
			this.fieldLabel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Constant.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Function.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public FieldSelectorControl FieldSelectorControl;
		public DevExpress.XtraEditors.TextEdit Constant;
		public DevExpress.XtraEditors.LabelControl ConstantLabel1;
		public DevExpress.XtraEditors.LabelControl label4;
		public DevExpress.XtraEditors.ComboBoxEdit Function;
		private System.Windows.Forms.GroupBox fieldLabel;
	}
}
