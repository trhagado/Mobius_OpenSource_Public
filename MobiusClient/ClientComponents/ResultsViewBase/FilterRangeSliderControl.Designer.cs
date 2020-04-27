namespace Mobius.ClientComponents
{
	partial class FilterRangeSliderControl
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
			this.RangeFilter = new DevExpress.XtraEditors.RangeTrackBarControl();
			this.LowValueLabel = new DevExpress.XtraEditors.LabelControl();
			this.HighValueLabel = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.RangeFilter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RangeFilter.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// RangeFilter
			// 
			this.RangeFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RangeFilter.EditValue = new DevExpress.XtraEditors.Repository.TrackBarRange(0, 0);
			this.RangeFilter.Location = new System.Drawing.Point(0, 14);
			this.RangeFilter.Name = "RangeFilter";
			this.RangeFilter.Properties.AllowFocused = false;
			this.RangeFilter.Size = new System.Drawing.Size(245, 45);
			this.RangeFilter.TabIndex = 99;
			this.RangeFilter.ValueChanged += new System.EventHandler(this.RangeFilter_ValueChanged);
			// 
			// LowValueLabel
			// 
			this.LowValueLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.LowValueLabel.AutoEllipsis = true;
			this.LowValueLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.LowValueLabel.Location = new System.Drawing.Point(6, 2);
			this.LowValueLabel.Name = "LowValueLabel";
			this.LowValueLabel.Size = new System.Drawing.Size(76, 13);
			this.LowValueLabel.TabIndex = 101;
			this.LowValueLabel.Text = "Low Value Label";
			this.LowValueLabel.Click += new System.EventHandler(this.LowValueLabel_Click);
			// 
			// HighValueLabel
			// 
			this.HighValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HighValueLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
			this.HighValueLabel.AutoEllipsis = true;
			this.HighValueLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.HighValueLabel.Location = new System.Drawing.Point(160, 2);
			this.HighValueLabel.Name = "HighValueLabel";
			this.HighValueLabel.Size = new System.Drawing.Size(78, 13);
			this.HighValueLabel.TabIndex = 102;
			this.HighValueLabel.Text = "High Value Label";
			this.HighValueLabel.Click += new System.EventHandler(this.HighValueLabel_Click);
			// 
			// FilterRangeSliderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.HighValueLabel);
			this.Controls.Add(this.LowValueLabel);
			this.Controls.Add(this.RangeFilter);
			this.Name = "FilterRangeSliderControl";
			this.Size = new System.Drawing.Size(246, 48);
			this.Resize += new System.EventHandler(this.FilterRangeControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.RangeFilter.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RangeFilter)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl LowValueLabel;
		private DevExpress.XtraEditors.LabelControl HighValueLabel;
		internal DevExpress.XtraEditors.RangeTrackBarControl RangeFilter;
	}
}
