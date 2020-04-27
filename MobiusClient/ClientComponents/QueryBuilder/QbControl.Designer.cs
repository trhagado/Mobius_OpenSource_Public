namespace Mobius.ClientComponents
{
    partial class QbControl
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
			this.QbSplitter = new DevExpress.XtraEditors.SplitContainerControl();
			this.QbContentsCtl = new Mobius.ClientComponents.QbContentsTree();
			this.QueryTablesControl = new Mobius.ClientComponents.QueryTablesControl();
			((System.ComponentModel.ISupportInitialize)(this.QbSplitter)).BeginInit();
			this.QbSplitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// QbSplitter
			// 
			this.QbSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QbSplitter.Location = new System.Drawing.Point(0, 0);
			this.QbSplitter.Name = "QbSplitter";
			this.QbSplitter.Panel1.Controls.Add(this.QbContentsCtl);
			this.QbSplitter.Panel1.Text = "Panel1";
			this.QbSplitter.Panel2.Controls.Add(this.QueryTablesControl);
			this.QbSplitter.Panel2.Text = "Panel2";
			this.QbSplitter.Size = new System.Drawing.Size(876, 509);
			this.QbSplitter.SplitterPosition = 335;
			this.QbSplitter.TabIndex = 0;
			this.QbSplitter.Text = "splitContainerControl1";
			this.QbSplitter.Paint += new System.Windows.Forms.PaintEventHandler(this.QbSplitter_Paint);
			// 
			// QbContentsCtl
			// 
			this.QbContentsCtl.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.QbContentsCtl.Appearance.Options.UseBackColor = true;
			this.QbContentsCtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QbContentsCtl.Location = new System.Drawing.Point(0, 0);
			this.QbContentsCtl.Name = "QbContentsCtl";
			this.QbContentsCtl.Size = new System.Drawing.Size(335, 509);
			this.QbContentsCtl.TabIndex = 0;
			// 
			// QueryTablesControl
			// 
			this.QueryTablesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryTablesControl.Location = new System.Drawing.Point(0, 0);
			this.QueryTablesControl.Name = "QueryTablesControl";
			this.QueryTablesControl.Query = null;
			this.QueryTablesControl.ShowCriteriaTab = true;
			this.QueryTablesControl.ShowHeader = true;
			this.QueryTablesControl.Size = new System.Drawing.Size(536, 509);
			this.QueryTablesControl.TabIndex = 0;
			// 
			// QbControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.QbSplitter);
			this.Name = "QbControl";
			this.Size = new System.Drawing.Size(876, 509);
			((System.ComponentModel.ISupportInitialize)(this.QbSplitter)).EndInit();
			this.QbSplitter.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

				public QueryTablesControl QueryTablesControl;
				public DevExpress.XtraEditors.SplitContainerControl QbSplitter;
				public QbContentsTree QbContentsCtl;
    }
}
