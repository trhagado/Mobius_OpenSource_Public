namespace Mobius.ClientComponents
{
	partial class MoleculeGridControl
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
			this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.ToolTipController = new DevExpress.Utils.ToolTipController(this.components);
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// gridView1
			// 
			this.gridView1.GridControl = this;
			this.gridView1.Name = "gridView1";
			// 
			// ToolTipController
			// 
			this.ToolTipController.AutoPopDelay = 1000000;
			this.ToolTipController.InitialDelay = 1;
			this.ToolTipController.RoundRadius = 1;
			// 
			// MoleculeGridControl
			// 
			this.MainView = this.gridView1;
			this.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
			this.VisibleChanged += new System.EventHandler(this.MoleculeGridControl_VisibleChanged);
			this.DoubleClick += new System.EventHandler(this.MoleculeGridControl_DoubleClick);
			this.Enter += new System.EventHandler(this.MoleculeGridControl_Enter);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MoleculeGridControl_KeyUp);
			this.Leave += new System.EventHandler(this.MoleculeGridControl_Leave);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MoleculeGridControl_MouseClick);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoleculeGridControl_MouseDown);
			this.MouseEnter += new System.EventHandler(this.MoleculeGridControl_MouseEnter);
			this.MouseLeave += new System.EventHandler(this.MoleculeGridControl_MouseLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MoleculeGridControl_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MoleculeGridControl_MouseUp);
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
		new internal DevExpress.Utils.ToolTipController ToolTipController;

	}
}
