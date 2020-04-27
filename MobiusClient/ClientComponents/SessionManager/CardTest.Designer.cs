using DevExpress.Utils;

namespace Mobius.ClientComponents.Dialogs
{
	partial class StructureConverterTest
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
			this.gridControl1 = new DevExpress.XtraGrid.GridControl();
			this.layoutView1 = new DevExpress.XtraGrid.Views.Layout.LayoutView();
			this.layoutViewCard1 = new DevExpress.XtraGrid.Views.Layout.LayoutViewCard();
			this.bandedGridView1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
			this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutViewCard1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bandedGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// gridControl1
			// 
			this.gridControl1.Location = new System.Drawing.Point(19, 19);
			this.gridControl1.MainView = this.layoutView1;
			this.gridControl1.Name = "gridControl1";
			this.gridControl1.Size = new System.Drawing.Size(559, 490);
			this.gridControl1.TabIndex = 0;
			this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.layoutView1,
            this.bandedGridView1});
			// 
			// layoutView1
			// 
			this.layoutView1.GridControl = this.gridControl1;
			this.layoutView1.Name = "layoutView1";
			this.layoutView1.TemplateCard = this.layoutViewCard1;
            // 
            // layoutViewCard1
            // 
            //this.layoutViewCard1.ExpandButtonLocation = DevExpress.Utils.GroupElementLocation.AfterText; DevExpress 15.2.7 Upgrade
            this.layoutViewCard1.HeaderButtonsLocation = GroupElementLocation.AfterText;
            this.layoutViewCard1.Name = "layoutViewTemplateCard";
			// 
			// bandedGridView1
			// 
			this.bandedGridView1.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
			this.bandedGridView1.GridControl = this.gridControl1;
			this.bandedGridView1.Name = "bandedGridView1";
			// 
			// gridBand1
			// 
			this.gridBand1.Caption = "gridBand1";
			this.gridBand1.Name = "gridBand1";
			// 
			// CardTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(883, 543);
			this.Controls.Add(this.gridControl1);
			this.Name = "CardTest";
			this.Text = "CardTest";
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutViewCard1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bandedGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraGrid.GridControl gridControl1;
		private DevExpress.XtraGrid.Views.Layout.LayoutView layoutView1;
		private DevExpress.XtraGrid.Views.Layout.LayoutViewCard layoutViewCard1;
		private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView bandedGridView1;
		private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
	}
}