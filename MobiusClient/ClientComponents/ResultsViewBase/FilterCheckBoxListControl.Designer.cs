namespace Mobius.ClientComponents
{
	partial class FilterCheckBoxListControl
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
			this.ItemList = new DevExpress.XtraEditors.CheckedListBoxControl();
			((System.ComponentModel.ISupportInitialize)(this.ItemList)).BeginInit();
			this.SuspendLayout();
			// 
			// ItemList
			// 
			this.ItemList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.ItemList.CheckOnClick = true;
			this.ItemList.Location = new System.Drawing.Point(3, 4);
			this.ItemList.Name = "ItemList";
			this.ItemList.Size = new System.Drawing.Size(239, 192);
			this.ItemList.TabIndex = 101;
			this.ItemList.ItemCheck += new DevExpress.XtraEditors.Controls.ItemCheckEventHandler(this.ItemList_ItemCheck);
			// 
			// FilterCheckBoxListControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ItemList);
			this.Name = "FilterCheckBoxListControl";
			this.Size = new System.Drawing.Size(246, 201);
			((System.ComponentModel.ISupportInitialize)(this.ItemList)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DevExpress.XtraEditors.CheckedListBoxControl ItemList;
	}
}
