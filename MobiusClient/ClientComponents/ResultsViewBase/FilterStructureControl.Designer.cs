namespace Mobius.ClientComponents
{
	partial class FilterStructureControl
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
			this.components = new System.ComponentModel.Container();
			this.StructureRenditor = new Mobius.CdkMx.CdkMolControl();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// Structure
			// 
			this.StructureRenditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.StructureRenditor.Location = new System.Drawing.Point(3, 4);
			this.StructureRenditor.MolfileString = null;
			this.StructureRenditor.Name = "Structure";
			this.StructureRenditor.Size = new System.Drawing.Size(239, 157);
			this.StructureRenditor.TabIndex = 0;
			this.StructureRenditor.VisibleChanged += new System.EventHandler(this.Structure_VisibleChanged);
			this.StructureRenditor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Structure_MouseDoubleClick);
			this.StructureRenditor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Structure_MouseClick);
			this.StructureRenditor.StructureChanged += new System.EventHandler(this.Structure_StructureChanged);
			// 
			// Timer
			// 
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// FilterStructureControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.StructureRenditor);
			this.Name = "FilterStructureControl";
			this.Size = new System.Drawing.Size(246, 166);
			this.ResumeLayout(false);

		}

		#endregion

		internal CdkMx.CdkMolControl StructureRenditor;
		private System.Windows.Forms.Timer Timer;

	}
}
