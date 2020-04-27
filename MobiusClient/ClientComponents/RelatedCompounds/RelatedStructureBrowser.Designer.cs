namespace Mobius.ClientComponents
{
	partial class RelatedStructureBrowser
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RelatedStructureBrowser));
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.CompoundIdControl = new DevExpress.XtraEditors.TextEdit();
			this.RelatedStructuresControl = new Mobius.ClientComponents.RelatedStructuresControl();
			((System.ComponentModel.ISupportInitialize)(this.CompoundIdControl.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Timer
			// 
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// CompoundIdControl
			// 
			this.CompoundIdControl.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.CompoundIdControl.Location = new System.Drawing.Point(8, 11);
			this.CompoundIdControl.Name = "CompoundIdControl";
			this.CompoundIdControl.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.CompoundIdControl.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CompoundIdControl.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.CompoundIdControl.Properties.Appearance.Options.UseBackColor = true;
			this.CompoundIdControl.Properties.Appearance.Options.UseFont = true;
			this.CompoundIdControl.Properties.Appearance.Options.UseForeColor = true;
			this.CompoundIdControl.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CompoundIdControl.Size = new System.Drawing.Size(130, 20);
			this.CompoundIdControl.TabIndex = 76;
			this.CompoundIdControl.Tag = "RegNo";
			this.CompoundIdControl.Visible = false;
			this.CompoundIdControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CompoundIdControl_KeyDown);
			// 
			// RelatedStructuresControl
			// 
			this.RelatedStructuresControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedStructuresControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RelatedStructuresControl.Location = new System.Drawing.Point(6, 7);
			this.RelatedStructuresControl.Name = "RelatedStructuresControl";
			this.RelatedStructuresControl.Size = new System.Drawing.Size(612, 617);
			this.RelatedStructuresControl.TabIndex = 219;
			// 
			// RelatedStructureBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 636);
			this.Controls.Add(this.RelatedStructuresControl);
			this.Controls.Add(this.CompoundIdControl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "RelatedStructureBrowser";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "RelatedStructureBrowser";
			this.Activated += new System.EventHandler(this.RelatedStructureBrowser_Activated);
			this.Deactivate += new System.EventHandler(this.RelatedStructureBrowser_Deactivate);
			((System.ComponentModel.ISupportInitialize)(this.CompoundIdControl.Properties)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		public System.Windows.Forms.Timer Timer;
		private RelatedStructuresControl RelatedStructuresControl;
		public DevExpress.XtraEditors.TextEdit CompoundIdControl;
	}
}