namespace Mobius.ClientComponents
{
	partial class ContentsTreeWithSearch
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentsTreeWithSearch));
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.ContentsLabel = new DevExpress.XtraEditors.LabelControl();
			this.CommandLineControl = new DevExpress.XtraEditors.MRUEdit();
			this.ContentsTreeCtl = new Mobius.ClientComponents.ContentsTreeControl();
			((System.ComponentModel.ISupportInitialize)(this.CommandLineControl.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "WorldSearch.bmp");
			this.Bitmaps16x16.Images.SetKeyName(1, "AnnotationTable.bmp");
			this.Bitmaps16x16.Images.SetKeyName(2, "CalcField.bmp");
			this.Bitmaps16x16.Images.SetKeyName(3, "TableStruct.bmp");
			this.Bitmaps16x16.Images.SetKeyName(4, "ChevronRt.bmp");
			// 
			// ContentsLabel
			// 
			this.ContentsLabel.Location = new System.Drawing.Point(5, 4);
			this.ContentsLabel.Name = "ContentsLabel";
			this.ContentsLabel.Size = new System.Drawing.Size(24, 13);
			this.ContentsLabel.TabIndex = 198;
			this.ContentsLabel.Text = "Find:";
			// 
			// CommandLineControl
			// 
			this.CommandLineControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommandLineControl.Location = new System.Drawing.Point(35, 2);
			this.CommandLineControl.Name = "CommandLineControl";
			this.CommandLineControl.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Separator),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Search),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Separator),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Delete)});
			this.CommandLineControl.Properties.ShowDropDown = DevExpress.XtraEditors.Controls.ShowDropDown.Never;
			this.CommandLineControl.Properties.BeforePopup += new System.EventHandler(this.CommandLineControl_Properties_BeforePopup);
			this.CommandLineControl.Size = new System.Drawing.Size(437, 20);
			this.CommandLineControl.TabIndex = 0;
			this.CommandLineControl.AddingMRUItem += new DevExpress.XtraEditors.Controls.AddingMRUItemEventHandler(this.CommandLineControl_AddingMRUItem);
			this.CommandLineControl.Popup += new System.EventHandler(this.CommandLineControl_Popup);
			this.CommandLineControl.BeforePopup += new System.EventHandler(this.CommandLineControl_BeforePopup);
			this.CommandLineControl.QueryPopUp += new System.ComponentModel.CancelEventHandler(this.CommandLineControl_QueryPopUp);
			this.CommandLineControl.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.CommandLineControl_ButtonClick);
			this.CommandLineControl.Enter += new System.EventHandler(this.CommandLineControl_Enter);
			// 
			// ContentsTreeCtl
			// 
			this.ContentsTreeCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ContentsTreeCtl.Location = new System.Drawing.Point(1, 27);
			this.ContentsTreeCtl.Name = "ContentsTreeCtl";
			this.ContentsTreeCtl.Size = new System.Drawing.Size(471, 416);
			this.ContentsTreeCtl.TabIndex = 0;
			this.ContentsTreeCtl.FocusedNodeChanged += new System.EventHandler(this.ContentsTree_FocusedNodeChanged);
			this.ContentsTreeCtl.Click += new System.EventHandler(this.ContentsTree_Click);
			this.ContentsTreeCtl.DoubleClick += new System.EventHandler(this.ContentsTree_DoubleClick);
			this.ContentsTreeCtl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ContentsTree_MouseDown);
			this.ContentsTreeCtl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ContentsTree_MouseClick);
			// 
			// ContentsTreeWithSearch
			// 
			this.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Appearance.Options.UseBackColor = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.CommandLineControl);
			this.Controls.Add(this.ContentsLabel);
			this.Controls.Add(this.ContentsTreeCtl);
			this.Name = "ContentsTreeWithSearch";
			this.Size = new System.Drawing.Size(475, 445);
			((System.ComponentModel.ISupportInitialize)(this.CommandLineControl.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ImageList Bitmaps16x16;
		private DevExpress.XtraEditors.LabelControl ContentsLabel;
		public ContentsTreeControl ContentsTreeCtl;
		public DevExpress.XtraEditors.MRUEdit CommandLineControl;
	}
}
