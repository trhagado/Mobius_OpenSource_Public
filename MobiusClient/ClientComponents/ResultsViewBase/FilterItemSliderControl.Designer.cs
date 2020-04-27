namespace Mobius.ClientComponents
{
	partial class FilterItemSliderControl
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
			this.ItemFilter = new DevExpress.XtraEditors.TrackBarControl();
			this.ValueLabel = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.ItemFilter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemFilter.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// ItemFilter
			// 
			this.ItemFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ItemFilter.EditValue = null;
			this.ItemFilter.Location = new System.Drawing.Point(0, 14);
			this.ItemFilter.Name = "ItemFilter";
			this.ItemFilter.Properties.AllowFocused = false;
			this.ItemFilter.Size = new System.Drawing.Size(246, 42);
			this.ItemFilter.TabIndex = 109;
			this.ItemFilter.ValueChanged += new System.EventHandler(this.ItemFilter_ValueChanged);
			// 
			// ValueLabel
			// 
			this.ValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ValueLabel.Appearance.Options.UseTextOptions = true;
			this.ValueLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.ValueLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.ValueLabel.Location = new System.Drawing.Point(7, 2);
			this.ValueLabel.Name = "ValueLabel";
			this.ValueLabel.Size = new System.Drawing.Size(233, 13);
			this.ValueLabel.TabIndex = 111;
			this.ValueLabel.Text = "ValueLabel";
			// 
			// FilterItemSliderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ValueLabel);
			this.Controls.Add(this.ItemFilter);
			this.Name = "FilterItemSliderControl";
			this.Size = new System.Drawing.Size(246, 48);
			((System.ComponentModel.ISupportInitialize)(this.ItemFilter.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemFilter)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.LabelControl ValueLabel;
		internal DevExpress.XtraEditors.TrackBarControl ItemFilter;
	}
}
