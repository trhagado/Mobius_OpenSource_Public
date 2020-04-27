namespace Mobius.ClientComponents
{
	partial class TableList
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
			this.List = new DevExpress.XtraEditors.ListBoxControl();
			((System.ComponentModel.ISupportInitialize)(this.List)).BeginInit();
			this.SuspendLayout();
			// 
			// List
			// 
			this.List.Dock = System.Windows.Forms.DockStyle.Fill;
			this.List.Location = new System.Drawing.Point(0, 0);
			this.List.Name = "List";
			this.List.Size = new System.Drawing.Size(256, 389);
			this.List.TabIndex = 0;
			this.List.SelectedValueChanged += new System.EventHandler(this.List_SelectedValueChanged);
			// 
			// TableList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(256, 389);
			this.ControlBox = false;
			this.Controls.Add(this.List);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TableList";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Deactivate += new System.EventHandler(this.TableList_Deactivate);
			((System.ComponentModel.ISupportInitialize)(this.List)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal DevExpress.XtraEditors.ListBoxControl List;

	}
}