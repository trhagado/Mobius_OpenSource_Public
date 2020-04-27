namespace Mobius.ClientComponents
{
	partial class ToolResultsContainer
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
			this.QueryResultsControl = new Mobius.ClientComponents.QueryResultsControl();
			this.SuspendLayout();
			// 
			// QueryResultsControl
			// 
			this.QueryResultsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryResultsControl.Location = new System.Drawing.Point(0, 0);
			this.QueryResultsControl.Margin = new System.Windows.Forms.Padding(0);
			this.QueryResultsControl.Name = "QueryResultsControl";
			this.QueryResultsControl.Size = new System.Drawing.Size(645, 361);
			this.QueryResultsControl.TabIndex = 1;
			// 
			// ToolResultsContainer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.QueryResultsControl);
			this.Name = "ToolResultsContainer";
			this.Size = new System.Drawing.Size(645, 361);
			this.ResumeLayout(false);

		}

		#endregion

		public QueryResultsControl QueryResultsControl;
	}
}
