namespace Mobius.SpotfireClient
{
	partial class DataMapPanel
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
			this.DataMapGroupControl = new DevExpress.XtraEditors.GroupControl();
			this.EditMapButton = new DevExpress.XtraEditors.SimpleButton();
			this.DataMapControl = new Mobius.SpotfireClient.DataMapControl();
			((System.ComponentModel.ISupportInitialize)(this.DataMapGroupControl)).BeginInit();
			this.DataMapGroupControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// DataMapGroupControl
			// 
			this.DataMapGroupControl.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DataMapGroupControl.AppearanceCaption.Options.UseFont = true;
			this.DataMapGroupControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
			this.DataMapGroupControl.Controls.Add(this.EditMapButton);
			this.DataMapGroupControl.Controls.Add(this.DataMapControl);
			this.DataMapGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataMapGroupControl.Location = new System.Drawing.Point(0, 0);
			this.DataMapGroupControl.Name = "DataMapGroupControl";
			this.DataMapGroupControl.Size = new System.Drawing.Size(642, 564);
			this.DataMapGroupControl.TabIndex = 4;
			this.DataMapGroupControl.Text = "Data - Mapping of Spotfire Data Tables to Mobius Query Tables";
			// 
			// EditMapButton
			// 
			this.EditMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditMapButton.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditMapButton.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditMapButton.Appearance.Options.UseFont = true;
			this.EditMapButton.Appearance.Options.UseForeColor = true;
			this.EditMapButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditMapButton.Location = new System.Drawing.Point(554, 81);
			this.EditMapButton.Name = "EditMapButton";
			this.EditMapButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditMapButton.Size = new System.Drawing.Size(75, 22);
			this.EditMapButton.TabIndex = 225;
			this.EditMapButton.Text = "Edit Mapping";
			this.EditMapButton.Visible = false;
			this.EditMapButton.Click += new System.EventHandler(this.EditMapButton_Click);
			// 
			// DataMapControl
			// 
			this.DataMapControl.CanEditMapping = true;
			this.DataMapControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataMapControl.Location = new System.Drawing.Point(2, 23);
			this.DataMapControl.Name = "DataMapControl";
			this.DataMapControl.SelectSingleColumn = false;
			this.DataMapControl.ShowSelectedColumnCheckBoxes = true;
			this.DataMapControl.Size = new System.Drawing.Size(638, 539);
			this.DataMapControl.TabIndex = 223;
			// 
			// DataMapPanel
			// 
			this.Controls.Add(this.DataMapGroupControl);
			this.Name = "DataMapPanel";
			this.Size = new System.Drawing.Size(642, 564);
			((System.ComponentModel.ISupportInitialize)(this.DataMapGroupControl)).EndInit();
			this.DataMapGroupControl.ResumeLayout(false);
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
		private DevExpress.XtraEditors.GroupControl DataMapGroupControl;
		private Mobius.SpotfireClient.DataMapControl DataMapControl;
		public DevExpress.XtraEditors.SimpleButton EditMapButton;
	}
}
