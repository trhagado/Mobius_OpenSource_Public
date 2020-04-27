using Mobius.ComOps;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.LookAndFeel;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// 
	/// </summary>
	public class QnfStats : XtraForm
	{
		public static QnfStats Instance;

		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.CheckEdit StdDev;
		public DevExpress.XtraEditors.CheckEdit StdErr;
		public DevExpress.XtraEditors.CheckEdit NValue;
		public System.Windows.Forms.GroupBox ElementList;
		public DevExpress.XtraEditors.CheckEdit StdDevLabel;
		public DevExpress.XtraEditors.CheckEdit StdErrLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public QnfStats()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		public static string Show(string serializedQnf)
		{
			if (Instance == null) Instance = new QnfStats();
			Instance.Deserialize(serializedQnf);
			DialogResult dr = Instance.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				serializedQnf = Instance.Serialize();
				return serializedQnf;
			}

			else return null;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.ElementList = new System.Windows.Forms.GroupBox();
			this.StdErrLabel = new DevExpress.XtraEditors.CheckEdit();
			this.StdDevLabel = new DevExpress.XtraEditors.CheckEdit();
			this.NValue = new DevExpress.XtraEditors.CheckEdit();
			this.StdErr = new DevExpress.XtraEditors.CheckEdit();
			this.StdDev = new DevExpress.XtraEditors.CheckEdit();
			this.ElementList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StdErrLabel.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StdDevLabel.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NValue.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StdErr.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StdDev.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(238, 113);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 24);
			this.OK.TabIndex = 39;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Cancel.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Cancel.Appearance.Options.UseFont = true;
			this.Cancel.Appearance.Options.UseForeColor = true;
			this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(304, 113);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 24);
			this.Cancel.TabIndex = 38;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// ElementList
			// 
			this.ElementList.BackColor = System.Drawing.Color.Transparent;
			this.ElementList.Controls.Add(this.StdErrLabel);
			this.ElementList.Controls.Add(this.StdDevLabel);
			this.ElementList.Controls.Add(this.NValue);
			this.ElementList.Controls.Add(this.StdErr);
			this.ElementList.Controls.Add(this.StdDev);
			this.ElementList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ElementList.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ElementList.Location = new System.Drawing.Point(4, 9);
			this.ElementList.Name = "ElementList";
			this.ElementList.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ElementList.Size = new System.Drawing.Size(360, 99);
			this.ElementList.TabIndex = 40;
			this.ElementList.TabStop = false;
			this.ElementList.Text = "Display the following statistics for summarized data (if available)";
			// 
			// StdErrLabel
			// 
			this.StdErrLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.StdErrLabel.Location = new System.Drawing.Point(150, 45);
			this.StdErrLabel.Name = "StdErrLabel";
			this.StdErrLabel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StdErrLabel.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StdErrLabel.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StdErrLabel.Properties.Appearance.Options.UseBackColor = true;
			this.StdErrLabel.Properties.Appearance.Options.UseFont = true;
			this.StdErrLabel.Properties.Appearance.Options.UseForeColor = true;
			this.StdErrLabel.Properties.Caption = "Include \"se=\" label";
			this.StdErrLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StdErrLabel.Size = new System.Drawing.Size(142, 18);
			this.StdErrLabel.TabIndex = 22;
			// 
			// StdDevLabel
			// 
			this.StdDevLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.StdDevLabel.Location = new System.Drawing.Point(150, 22);
			this.StdDevLabel.Name = "StdDevLabel";
			this.StdDevLabel.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StdDevLabel.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StdDevLabel.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StdDevLabel.Properties.Appearance.Options.UseBackColor = true;
			this.StdDevLabel.Properties.Appearance.Options.UseFont = true;
			this.StdDevLabel.Properties.Appearance.Options.UseForeColor = true;
			this.StdDevLabel.Properties.Caption = "Include \"sd=\" label";
			this.StdDevLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StdDevLabel.Size = new System.Drawing.Size(142, 18);
			this.StdDevLabel.TabIndex = 21;
			// 
			// NValue
			// 
			this.NValue.Cursor = System.Windows.Forms.Cursors.Default;
			this.NValue.Location = new System.Drawing.Point(14, 69);
			this.NValue.Name = "NValue";
			this.NValue.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.NValue.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NValue.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.NValue.Properties.Appearance.Options.UseBackColor = true;
			this.NValue.Properties.Appearance.Options.UseFont = true;
			this.NValue.Properties.Appearance.Options.UseForeColor = true;
			this.NValue.Properties.Caption = "n=m/t  (Number of values in &mean / number of times &tested)";
			this.NValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.NValue.Size = new System.Drawing.Size(340, 18);
			this.NValue.TabIndex = 19;
			// 
			// StdErr
			// 
			this.StdErr.Cursor = System.Windows.Forms.Cursors.Default;
			this.StdErr.Location = new System.Drawing.Point(14, 45);
			this.StdErr.Name = "StdErr";
			this.StdErr.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StdErr.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StdErr.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StdErr.Properties.Appearance.Options.UseBackColor = true;
			this.StdErr.Properties.Appearance.Options.UseFont = true;
			this.StdErr.Properties.Appearance.Options.UseForeColor = true;
			this.StdErr.Properties.Caption = "Standard error";
			this.StdErr.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StdErr.Size = new System.Drawing.Size(122, 18);
			this.StdErr.TabIndex = 18;
			// 
			// StdDev
			// 
			this.StdDev.Cursor = System.Windows.Forms.Cursors.Default;
			this.StdDev.Location = new System.Drawing.Point(14, 22);
			this.StdDev.Name = "StdDev";
			this.StdDev.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StdDev.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StdDev.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StdDev.Properties.Appearance.Options.UseBackColor = true;
			this.StdDev.Properties.Appearance.Options.UseFont = true;
			this.StdDev.Properties.Appearance.Options.UseForeColor = true;
			this.StdDev.Properties.Caption = "Standard deviation";
			this.StdDev.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StdDev.Size = new System.Drawing.Size(124, 18);
			this.StdDev.TabIndex = 17;
			// 
			// QnfStats
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(368, 142);
			this.Controls.Add(this.ElementList);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "QnfStats";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Summarized Data Statistics Display";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ModelDialogForm_Closing);
			this.ElementList.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.StdErrLabel.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StdDevLabel.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NValue.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StdErr.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StdDev.Properties)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void OK_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void ModelDialogForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{ // this event is triggered by clicking the close button or a call of Hide()
		}

/// <summary>
/// Convert serialized string into set of check boxes
/// </summary>
/// <param name="txt"></param>

		public void Deserialize (
			string txt)
		{
			StdDev.Checked = (txt.IndexOf("stddev") >= 0 ? true : false);
			StdDevLabel.Checked = (txt.IndexOf("sd") >= 0 ? true : false);
			StdErr.Checked = (txt.IndexOf("stderr") >= 0 ? true : false);
			StdErrLabel.Checked = (txt.IndexOf("se") >= 0 ? true : false);
			NValue.Checked = (txt.IndexOf("n") >= 0 ? true : false);
		}

/// <summary>
/// Convert check boxes into string
/// </summary>
/// <returns></returns>

		public string Serialize ()
		{
			string txt = "";

			if (StdDev.Checked)
			{
				if (StdDevLabel.Checked)
				{
					txt += "sd=";
				}

				txt += "stddev";
			}

			if (StdErr.Checked)
			{
				if (txt!="") txt += ", ";
				if (StdErrLabel.Checked)
				{
					txt += "se=";
				}

				txt += "stderr";
			}

			if (NValue.Checked)
			{
				if (txt!="") txt += ", ";
				txt += "n=m/t";
			}

			return txt;
		}

	}
}
