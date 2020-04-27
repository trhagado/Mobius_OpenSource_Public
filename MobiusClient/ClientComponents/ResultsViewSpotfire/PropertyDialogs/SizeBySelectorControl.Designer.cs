﻿namespace Mobius.ClientComponents
{
	partial class SizeBySelectorControl
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
			this.SizeByColumn = new DevExpress.XtraEditors.CheckEdit();
			this.SizeByFixedSize = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
			this.OverallSizeLabel = new DevExpress.XtraEditors.LabelControl();
			this.OverallSize = new DevExpress.XtraEditors.TrackBarControl();
			//this.SizeColumnSelector = new Mobius.ClientComponents.FieldSelectorControl();
			((System.ComponentModel.ISupportInitialize)(this.SizeByColumn.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SizeByFixedSize.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OverallSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OverallSize.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// SizeByColumn
			// 
			this.SizeByColumn.EditValue = true;
			this.SizeByColumn.Location = new System.Drawing.Point(5, 56);
			this.SizeByColumn.Name = "SizeByColumn";
			this.SizeByColumn.Properties.AutoWidth = true;
			this.SizeByColumn.Properties.Caption = "";
			this.SizeByColumn.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SizeByColumn.Properties.RadioGroupIndex = 1;
			this.SizeByColumn.Size = new System.Drawing.Size(23, 19);
			this.SizeByColumn.TabIndex = 196;
			this.SizeByColumn.EditValueChanged += new System.EventHandler(this.SizeByColumn_EditValueChanged);
			// 
			// SizeByFixedSize
			// 
			this.SizeByFixedSize.Location = new System.Drawing.Point(5, 23);
			this.SizeByFixedSize.Name = "SizeByFixedSize";
			this.SizeByFixedSize.Properties.AutoWidth = true;
			this.SizeByFixedSize.Properties.Caption = " Fixed size";
			this.SizeByFixedSize.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SizeByFixedSize.Properties.RadioGroupIndex = 1;
			this.SizeByFixedSize.Size = new System.Drawing.Size(73, 19);
			this.SizeByFixedSize.TabIndex = 195;
			this.SizeByFixedSize.TabStop = false;
			this.SizeByFixedSize.EditValueChanged += new System.EventHandler(this.SizeByFixedSize_EditValueChanged);
			// 
			// labelControl4
			// 
			this.labelControl4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl4.LineVisible = true;
			this.labelControl4.Location = new System.Drawing.Point(5, 2);
			this.labelControl4.Name = "labelControl4";
			this.labelControl4.Size = new System.Drawing.Size(439, 15);
			this.labelControl4.TabIndex = 193;
			this.labelControl4.Text = "Size by";
			// 
			// OverallSizeLabel
			// 
			this.OverallSizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.OverallSizeLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.OverallSizeLabel.LineVisible = true;
			this.OverallSizeLabel.Location = new System.Drawing.Point(5, 118);
			this.OverallSizeLabel.Name = "OverallSizeLabel";
			this.OverallSizeLabel.Size = new System.Drawing.Size(441, 12);
			this.OverallSizeLabel.TabIndex = 192;
			this.OverallSizeLabel.Text = "Overall size";
			// 
			// OverallSize
			// 
			this.OverallSize.EditValue = null;
			this.OverallSize.Location = new System.Drawing.Point(17, 137);
			this.OverallSize.Name = "OverallSize";
			this.OverallSize.Properties.AllowFocused = false;
			this.OverallSize.Properties.LargeChange = 20;
			this.OverallSize.Properties.Maximum = 100;
			this.OverallSize.Properties.TickFrequency = 5;
			this.OverallSize.Size = new System.Drawing.Size(235, 45);
			this.OverallSize.TabIndex = 191;
			this.OverallSize.EditValueChanged += new System.EventHandler(this.OverallSize_EditValueChanged);
			// 
			// SizeBySelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.SizeByColumn);
			this.Controls.Add(this.SizeByFixedSize);
			//this.Controls.Add(this.SizeColumnSelector);
			this.Controls.Add(this.labelControl4);
			this.Controls.Add(this.OverallSizeLabel);
			this.Controls.Add(this.OverallSize);
			this.Name = "SizeBySelectorControl";
			this.Size = new System.Drawing.Size(447, 189);
			((System.ComponentModel.ISupportInitialize)(this.SizeByColumn.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SizeByFixedSize.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OverallSize.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OverallSize)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.CheckEdit SizeByColumn;
		private DevExpress.XtraEditors.CheckEdit SizeByFixedSize;
		//private FieldSelectorControl SizeColumnSelector;
		private DevExpress.XtraEditors.LabelControl labelControl4;
		internal DevExpress.XtraEditors.TrackBarControl OverallSize;
		internal DevExpress.XtraEditors.LabelControl OverallSizeLabel;
	}
}
