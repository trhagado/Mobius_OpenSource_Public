namespace Mobius.SpotfireClient
{
	partial class SelectColumnsPanel
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
			this.SelectColumnsGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.SelectColumnsDataMapControl = new Mobius.SpotfireClient.DataMapControl();
			((System.ComponentModel.ISupportInitialize)(this.SelectColumnsGroupControl)).BeginInit();
			this.SelectColumnsGroupControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SelectColumnsGroupControl
			// 
			this.SelectColumnsGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold);
			this.SelectColumnsGroupControl.AppearanceCaption.Options.UseFont = true;
			this.SelectColumnsGroupControl.Controls.Add(this.SelectColumnsDataMapControl);
			this.SelectColumnsGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectColumnsGroupControl.Location = new System.Drawing.Point(0, 0);
			this.SelectColumnsGroupControl.Name = "SelectColumnsGroupControl";
			this.SelectColumnsGroupControl.Size = new System.Drawing.Size(642, 564);
			this.SelectColumnsGroupControl.TabIndex = 0;
			this.SelectColumnsGroupControl.Text = "Columns";
			// 
			// SelectColumnsDataMapControl
			// 
			this.SelectColumnsDataMapControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectColumnsDataMapControl.Location = new System.Drawing.Point(2, 25);
			this.SelectColumnsDataMapControl.Name = "SelectColumnsDataMapControl";
			this.SelectColumnsDataMapControl.ShowSelectedColumnCheckBoxes = true;
			this.SelectColumnsDataMapControl.Size = new System.Drawing.Size(638, 537);
			this.SelectColumnsDataMapControl.TabIndex = 0;
			// 
			// SelectColumnsPanel
			// 
			this.Controls.Add(this.SelectColumnsGroupControl);
			this.Name = "SelectColumnsPanel";
			this.Size = new System.Drawing.Size(642, 564);
			((System.ComponentModel.ISupportInitialize)(this.SelectColumnsGroupControl)).EndInit();
			this.SelectColumnsGroupControl.ResumeLayout(false);
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
		private DevExpress.XtraEditors.GroupControl SelectColumnsGroupControl;
		private DataMapControl SelectColumnsDataMapControl;
	}
}
