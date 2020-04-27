namespace Mobius.SpotfireClient
{
	partial class GeneralPropertiesPanel
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
			this.GeneralGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.ShowDescriptionInVis = new DevExpress.XtraEditors.CheckEdit();
			this.ShowTitle = new DevExpress.XtraEditors.CheckEdit();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.Description = new DevExpress.XtraEditors.MemoEdit();
			this.Title = new DevExpress.XtraEditors.TextEdit();
			this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.GeneralGroupControl)).BeginInit();
			this.GeneralGroupControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShowDescriptionInVis.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowTitle.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Description.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Title.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// GeneralGroupControl
			// 
			this.GeneralGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold);
			this.GeneralGroupControl.AppearanceCaption.Options.UseFont = true;
			this.GeneralGroupControl.Controls.Add(this.ShowDescriptionInVis);
			this.GeneralGroupControl.Controls.Add(this.ShowTitle);
			this.GeneralGroupControl.Controls.Add(this.labelControl1);
			this.GeneralGroupControl.Controls.Add(this.Description);
			this.GeneralGroupControl.Controls.Add(this.Title);
			this.GeneralGroupControl.Controls.Add(this.labelControl22);
			this.GeneralGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GeneralGroupControl.Location = new System.Drawing.Point(0, 0);
			this.GeneralGroupControl.Name = "GeneralGroupControl";
			this.GeneralGroupControl.Size = new System.Drawing.Size(553, 576);
			this.GeneralGroupControl.TabIndex = 1;
			this.GeneralGroupControl.Text = "General";
			// 
			// ShowDescriptionInVis
			// 
			this.ShowDescriptionInVis.AllowDrop = true;
			this.ShowDescriptionInVis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowDescriptionInVis.Location = new System.Drawing.Point(12, 552);
			this.ShowDescriptionInVis.Name = "ShowDescriptionInVis";
			this.ShowDescriptionInVis.Properties.AutoWidth = true;
			this.ShowDescriptionInVis.Properties.Caption = "Show description in visualization";
			this.ShowDescriptionInVis.Size = new System.Drawing.Size(175, 19);
			this.ShowDescriptionInVis.TabIndex = 219;
			this.ShowDescriptionInVis.EditValueChanged += new System.EventHandler(this.ShowDescriptionInVis_EditValueChanged);
			// 
			// ShowTitle
			// 
			this.ShowTitle.AllowDrop = true;
			this.ShowTitle.Location = new System.Drawing.Point(12, 79);
			this.ShowTitle.Name = "ShowTitle";
			this.ShowTitle.Properties.AutoWidth = true;
			this.ShowTitle.Properties.Caption = "Show title bar";
			this.ShowTitle.Size = new System.Drawing.Size(88, 19);
			this.ShowTitle.TabIndex = 218;
			this.ShowTitle.EditValueChanged += new System.EventHandler(this.ShowTitle_EditValueChanged);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl1.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl1.Appearance.Options.UseFont = true;
			this.labelControl1.Appearance.Options.UseForeColor = true;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(8, 110);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(521, 14);
			this.labelControl1.TabIndex = 198;
			this.labelControl1.Text = "Description:";
			// 
			// Description
			// 
			this.Description.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Description.Location = new System.Drawing.Point(5, 132);
			this.Description.Name = "Description";
			this.Description.Properties.EditValueChangedDelay = 750;
			this.Description.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
			this.Description.Size = new System.Drawing.Size(542, 416);
			this.Description.TabIndex = 195;
			this.Description.EditValueChanged += new System.EventHandler(this.Description_EditValueChanged);
			// 
			// Title
			// 
			this.Title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Title.Location = new System.Drawing.Point(5, 55);
			this.Title.Name = "Title";
			this.Title.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Title.Properties.Appearance.Options.UseFont = true;
			this.Title.Properties.EditValueChangedDelay = 750;
			this.Title.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
			this.Title.Size = new System.Drawing.Size(542, 20);
			this.Title.TabIndex = 0;
			this.Title.EditValueChanged += new System.EventHandler(this.Title_EditValueChanged);
			// 
			// labelControl22
			// 
			this.labelControl22.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl22.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl22.Appearance.Options.UseFont = true;
			this.labelControl22.Appearance.Options.UseForeColor = true;
			this.labelControl22.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl22.LineVisible = true;
			this.labelControl22.Location = new System.Drawing.Point(8, 33);
			this.labelControl22.Name = "labelControl22";
			this.labelControl22.Size = new System.Drawing.Size(521, 14);
			this.labelControl22.TabIndex = 199;
			this.labelControl22.Text = "Title: ";
			// 
			// GeneralPropertiesPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.GeneralGroupControl);
			this.Name = "GeneralPropertiesPanel";
			this.Size = new System.Drawing.Size(553, 576);
			((System.ComponentModel.ISupportInitialize)(this.GeneralGroupControl)).EndInit();
			this.GeneralGroupControl.ResumeLayout(false);
			this.GeneralGroupControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShowDescriptionInVis.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShowTitle.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Description.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Title.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.GroupControl GeneralGroupControl;
		internal DevExpress.XtraEditors.CheckEdit ShowDescriptionInVis;
		internal DevExpress.XtraEditors.CheckEdit ShowTitle;
		public DevExpress.XtraEditors.LabelControl labelControl1;
		public DevExpress.XtraEditors.MemoEdit Description;
		public DevExpress.XtraEditors.TextEdit Title;
		public DevExpress.XtraEditors.LabelControl labelControl22;
	}
}
