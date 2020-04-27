namespace Mobius.SpotfireClient
{
	partial class DataPropertiesPanel
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
			this.DataGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
			this.MainDataTableSelectorControl = new Mobius.SpotfireClient.TableSelectorControlMsx();
			this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.DataGroupControl)).BeginInit();
			this.DataGroupControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
			this.panelControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// DataGroupControl
			// 
			this.DataGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold);
			this.DataGroupControl.AppearanceCaption.Options.UseFont = true;
			this.DataGroupControl.Controls.Add(this.panelControl1);
			this.DataGroupControl.Controls.Add(this.labelControl22);
			this.DataGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataGroupControl.Location = new System.Drawing.Point(0, 0);
			this.DataGroupControl.Name = "DataGroupControl";
			this.DataGroupControl.Size = new System.Drawing.Size(553, 576);
			this.DataGroupControl.TabIndex = 1;
			this.DataGroupControl.Text = "Data";
			// 
			// panelControl1
			// 
			this.panelControl1.Controls.Add(this.MainDataTableSelectorControl);
			this.panelControl1.Location = new System.Drawing.Point(8, 53);
			this.panelControl1.Name = "panelControl1";
			this.panelControl1.Size = new System.Drawing.Size(350, 29);
			this.panelControl1.TabIndex = 201;
			// 
			// MainDataTableSelectorControl
			// 
			this.MainDataTableSelectorControl.Appearance.BackColor = System.Drawing.Color.White;
			this.MainDataTableSelectorControl.Appearance.Options.UseBackColor = true;
			this.MainDataTableSelectorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainDataTableSelectorControl.Location = new System.Drawing.Point(2, 2);
			this.MainDataTableSelectorControl.Name = "MainDataTableSelectorControl";
			this.MainDataTableSelectorControl.Size = new System.Drawing.Size(346, 25);
			this.MainDataTableSelectorControl.TabIndex = 200;
			this.MainDataTableSelectorControl.EditValueChanged += new System.EventHandler(this.MainDataTableSelectorControl_EditValueChanged);
			// 
			// labelControl22
			// 
			this.labelControl22.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelControl22.Appearance.ForeColor = System.Drawing.Color.Black;
			this.labelControl22.Appearance.Options.UseFont = true;
			this.labelControl22.Appearance.Options.UseForeColor = true;
			this.labelControl22.Location = new System.Drawing.Point(8, 33);
			this.labelControl22.Name = "labelControl22";
			this.labelControl22.Size = new System.Drawing.Size(87, 14);
			this.labelControl22.TabIndex = 199;
			this.labelControl22.Text = "Main data table:";
			// 
			// DataPropertiesPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.DataGroupControl);
			this.Name = "DataPropertiesPanel";
			this.Size = new System.Drawing.Size(553, 576);
			((System.ComponentModel.ISupportInitialize)(this.DataGroupControl)).EndInit();
			this.DataGroupControl.ResumeLayout(false);
			this.DataGroupControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
			this.panelControl1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.GroupControl DataGroupControl;
		public DevExpress.XtraEditors.LabelControl labelControl22;
		private TableSelectorControlMsx MainDataTableSelectorControl;
		private DevExpress.XtraEditors.PanelControl panelControl1;
	}
}
