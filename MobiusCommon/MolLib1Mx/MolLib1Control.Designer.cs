namespace Mobius.MolLib1
{
	partial class MolLib1Control
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
			this.toolTipController1 = new DevExpress.Utils.ToolTipController(this.components);
			this.SuspendLayout();
			// 
			// HelmControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.Name = "HelmControl";
			this.Size = new System.Drawing.Size(361, 267);
			this.SizeChanged += new System.EventHandler(this.MolLib1Control_SizeChanged);
			this.Click += new System.EventHandler(this.MolLib1Control_Click);
			this.DoubleClick += new System.EventHandler(this.MolLib1Control_DoubleClick);
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.Utils.ToolTipController toolTipController1;
	}
}
