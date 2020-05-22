namespace Mobius.ClientComponents
{
	partial class MoleculeControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoleculeControl));
			this.ToolTipController = new DevExpress.Utils.ToolTipController(this.components);
			this.RtClickContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.EditMoleculeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Separator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ViewMoleculeInNewWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.CutMolecule = new System.Windows.Forms.ToolStripMenuItem();
			this.CopyMolecule = new System.Windows.Forms.ToolStripMenuItem();
			this.PasteMolecule = new System.Windows.Forms.ToolStripMenuItem();
			this.DeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.EnlargeStructureButton = new DevExpress.XtraEditors.SimpleButton();
			this.HelmControl = new Mobius.Helm.HelmControl();
			this.KekuleControl = new Mobius.KekuleJs.KekuleJsControl();
			this.RtClickContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// ToolTipController
			// 
			this.ToolTipController.AutoPopDelay = 1000000;
			this.ToolTipController.InitialDelay = 1;
			// 
			// RtClickContextMenu
			// 
			this.RtClickContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.RtClickContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EditMoleculeMenuItem,
            this.Separator1,
            this.ViewMoleculeInNewWindowMenuItem,
            this.toolStripSeparator2,
            this.CutMolecule,
            this.CopyMolecule,
            this.PasteMolecule,
            this.DeleteMenuItem});
			this.RtClickContextMenu.Name = "CellRtClickContextMenu";
			this.RtClickContextMenu.Size = new System.Drawing.Size(252, 172);
			// 
			// EditMoleculeMenuItem
			// 
			this.EditMoleculeMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Edit;
			this.EditMoleculeMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.EditMoleculeMenuItem.Name = "EditMoleculeMenuItem";
			this.EditMoleculeMenuItem.Size = new System.Drawing.Size(251, 26);
			this.EditMoleculeMenuItem.Text = "Edit Molecule...";
			this.EditMoleculeMenuItem.Click += new System.EventHandler(this.EditMolecule_Click);
			// 
			// Separator1
			// 
			this.Separator1.Name = "Separator1";
			this.Separator1.Size = new System.Drawing.Size(248, 6);
			// 
			// ViewMoleculeInNewWindowMenuItem
			// 
			this.ViewMoleculeInNewWindowMenuItem.Image = global::Mobius.ClientComponents.Properties.Resources.Struct;
			this.ViewMoleculeInNewWindowMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.ViewMoleculeInNewWindowMenuItem.Name = "ViewMoleculeInNewWindowMenuItem";
			this.ViewMoleculeInNewWindowMenuItem.Size = new System.Drawing.Size(251, 26);
			this.ViewMoleculeInNewWindowMenuItem.Text = "View Molecule in a New Window";
			this.ViewMoleculeInNewWindowMenuItem.Click += new System.EventHandler(this.ViewMoleculeInNewWindow_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(248, 6);
			// 
			// CutMolecule
			// 
			this.CutMolecule.Image = global::Mobius.ClientComponents.Properties.Resources.Cut;
			this.CutMolecule.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CutMolecule.Name = "CutMolecule";
			this.CutMolecule.Size = new System.Drawing.Size(251, 26);
			this.CutMolecule.Text = "Cut";
			this.CutMolecule.Click += new System.EventHandler(this.CutMolecule_Click);
			// 
			// CopyMolecule
			// 
			this.CopyMolecule.Image = global::Mobius.ClientComponents.Properties.Resources.Copy;
			this.CopyMolecule.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.CopyMolecule.Name = "CopyMolecule";
			this.CopyMolecule.Size = new System.Drawing.Size(251, 26);
			this.CopyMolecule.Text = "Copy";
			this.CopyMolecule.Click += new System.EventHandler(this.CopyMolecule_Click);
			// 
			// PasteMolecule
			// 
			this.PasteMolecule.Image = global::Mobius.ClientComponents.Properties.Resources.Paste;
			this.PasteMolecule.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.PasteMolecule.Name = "PasteMolecule";
			this.PasteMolecule.Size = new System.Drawing.Size(251, 26);
			this.PasteMolecule.Text = "Paste";
			this.PasteMolecule.Click += new System.EventHandler(this.PasteMolecule_Click);
			// 
			// DeleteMenuItem
			// 
			this.DeleteMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("DeleteMenuItem.Image")));
			this.DeleteMenuItem.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.DeleteMenuItem.Name = "DeleteMenuItem";
			this.DeleteMenuItem.Size = new System.Drawing.Size(251, 26);
			this.DeleteMenuItem.Text = "Delete";
			this.DeleteMenuItem.Click += new System.EventHandler(this.DeleteMenuItem_Click);
			// 
			// EnlargeStructureButton
			// 
			this.EnlargeStructureButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EnlargeStructureButton.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.EnlargeStructureButton.Appearance.Options.UseBackColor = true;
			this.EnlargeStructureButton.AppearanceHovered.BackColor = System.Drawing.Color.Transparent;
			this.EnlargeStructureButton.AppearanceHovered.Options.UseBackColor = true;
			this.EnlargeStructureButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("EnlargeStructureButton.ImageOptions.Image")));
			this.EnlargeStructureButton.Location = new System.Drawing.Point(427, 0);
			this.EnlargeStructureButton.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
			this.EnlargeStructureButton.LookAndFeel.UseDefaultLookAndFeel = false;
			this.EnlargeStructureButton.Name = "EnlargeStructureButton";
			this.EnlargeStructureButton.Size = new System.Drawing.Size(16, 16);
			this.EnlargeStructureButton.TabIndex = 23;
			this.EnlargeStructureButton.ToolTip = "View Molecule in a New Window";
			this.EnlargeStructureButton.Click += new System.EventHandler(this.EnlargeStructureButton_Click);
			this.EnlargeStructureButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.EnlargeStructureButton_MouseClick);
			this.EnlargeStructureButton.MouseHover += new System.EventHandler(this.EnlargeStructureButton_MouseHover);
			// 
			// HelmControl
			// 
			this.HelmControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.HelmControl.Location = new System.Drawing.Point(27, 151);
			this.HelmControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.HelmControl.Name = "HelmControl";
			this.HelmControl.Size = new System.Drawing.Size(170, 72);
			this.HelmControl.TabIndex = 21;
			this.HelmControl.MoleculeChanged += new System.EventHandler(this.HelmControl_MoleculeChanged);
			// 
			// KekuleJsCtl
			// 
			this.KekuleControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.KekuleControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.KekuleControl.EditorReturnedHandler = null;
			this.KekuleControl.KekuleJsMode = Mobius.KekuleJs.KekuleJsControlMode.BrowserViewOnly;
			this.KekuleControl.Location = new System.Drawing.Point(27, 26);
			this.KekuleControl.Margin = new System.Windows.Forms.Padding(2);
			this.KekuleControl.MolfileString = "";
			this.KekuleControl.Name = "KekuleJsCtl";
			this.KekuleControl.Size = new System.Drawing.Size(161, 72);
			this.KekuleControl.TabIndex = 24;
			this.KekuleControl.MoleculeChanged += new System.EventHandler(this.KekuleControl_MoleculeChanged);
			// 
			// MoleculeControl
			// 
			this.Appearance.BackColor = System.Drawing.Color.White;
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this.KekuleControl);
			this.Controls.Add(this.EnlargeStructureButton);
			this.Controls.Add(this.HelmControl);
			this.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.Name = "MoleculeControl";
			this.Size = new System.Drawing.Size(447, 367);
			this.RtClickContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public Helm.HelmControl HelmControl;
		private DevExpress.Utils.ToolTipController ToolTipController;
		public System.Windows.Forms.ContextMenuStrip RtClickContextMenu;
		public System.Windows.Forms.ToolStripMenuItem EditMoleculeMenuItem;
		public System.Windows.Forms.ToolStripSeparator Separator1;
		public System.Windows.Forms.ToolStripMenuItem CutMolecule;
		public System.Windows.Forms.ToolStripMenuItem CopyMolecule;
		public System.Windows.Forms.ToolStripMenuItem PasteMolecule;
		public System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		public System.Windows.Forms.ToolStripMenuItem ViewMoleculeInNewWindowMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DeleteMenuItem;
		private DevExpress.XtraEditors.SimpleButton EnlargeStructureButton;
		public KekuleJs.KekuleJsControl KekuleControl;
	}
}
