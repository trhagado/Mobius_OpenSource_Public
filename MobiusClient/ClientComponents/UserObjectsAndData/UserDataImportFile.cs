using Mobius.ComOps;
using Mobius.Data;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Get import parameters for user data for a basic file 
	/// </summary>

	public class UserDataImportFile : XtraForm
	{
		static UserDataImportFile Instance;

		string InitialName = ""; // initial name for file when dialog entered
		public string FileFilter; // file extension filter
		public string DefaultFolder = "";
		public string DefaultExt = ""; // default extension

		public DevExpress.XtraEditors.SimpleButton Browse;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.LabelControl Label1;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl label3;
		public DevExpress.XtraEditors.CheckEdit DeleteExistingData;
		public DevExpress.XtraEditors.CheckEdit ImportInBackground;
		public DevExpress.XtraEditors.CheckEdit CheckForFileUpdates;
        private ImageList Bitmaps16x16;
		public CheckEdit HeaderLine;
		public LabelControl labelControl2;
		public LabelControl labelControl1;
		private IContainer components;

		public UserDataImportFile()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

		}

		/// <summary>
		/// Basic file import dialog (.sdf, .xls)
		/// </summary>
		/// <param name="importParms"></param>
		/// <param name="title"></param>
		/// <param name="filter"></param>
		/// <param name="ext"></param>
		/// <param name="setForm"></param>
		/// <returns></returns>

		public new static DialogResult ShowDialog(
			UserObject uo,
			UserDataImportParms importParms,
			string title,
			string filter,
			string ext,
			bool setForm)
		{
			if (Instance == null) Instance = new UserDataImportFile();
			return Instance.ShowDialog2(uo, importParms, title, filter, ext, setForm);
		}

		public DialogResult ShowDialog2(
			UserObject uo,
			UserDataImportParms importParms,
			string title,
			string filter,
			string ext,
			bool setForm)
		{
			if (setForm)
			{
				FileName.Text = importParms.FileName;
				HeaderLine.Checked = importParms.FirstLineHeaders;
				HeaderLine.Enabled = Lex.Ne(ext, ".sdf");
				DeleteExistingData.Checked = importParms.DeleteExisting;
				ImportInBackground.Checked = importParms.ImportInBackground;
				CheckForFileUpdates.Checked = importParms.CheckForFileUpdates;
				Text = title;
				FileFilter = filter;
				DefaultExt = ext;
			}

		GetInput:
			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			importParms.FileName = FileName.Text;
			importParms.ClientFileModified = FileUtil.GetFileLastWriteTime(importParms.FileName);

			importParms.FirstLineHeaders = HeaderLine.Checked;

			importParms.DeleteExisting = DeleteExistingData.Checked;
			if (importParms.DeleteExisting && uo.Id > 0)
			{
				dr = MessageBoxMx.Show(
					"Are you sure you want to delete all existing data?", UmlautMobius.String,
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dr != DialogResult.Yes) goto GetInput;
			}

			importParms.ImportInBackground = ImportInBackground.Checked;
			importParms.CheckForFileUpdates = CheckForFileUpdates.Checked;

			DialogResult = DialogResult.OK;
			return dr;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserDataImportFile));
            this.Browse = new DevExpress.XtraEditors.SimpleButton();
            this.FileName = new DevExpress.XtraEditors.TextEdit();
            this.Label1 = new DevExpress.XtraEditors.LabelControl();
            this.DeleteExistingData = new DevExpress.XtraEditors.CheckEdit();
            this.Cancel = new DevExpress.XtraEditors.SimpleButton();
            this.OK = new DevExpress.XtraEditors.SimpleButton();
            this.label3 = new DevExpress.XtraEditors.LabelControl();
            this.ImportInBackground = new DevExpress.XtraEditors.CheckEdit();
            this.CheckForFileUpdates = new DevExpress.XtraEditors.CheckEdit();
            this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
            this.HeaderLine = new DevExpress.XtraEditors.CheckEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeleteExistingData.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImportInBackground.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckForFileUpdates.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderLine.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // Browse
            // 
            this.Browse.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Browse.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Browse.Appearance.Options.UseFont = true;
            this.Browse.Appearance.Options.UseForeColor = true;
            this.Browse.Cursor = System.Windows.Forms.Cursors.Default;
            this.Browse.Location = new System.Drawing.Point(315, 80);
            this.Browse.Name = "Browse";
            this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Browse.Size = new System.Drawing.Size(84, 22);
            this.Browse.TabIndex = 32;
            this.Browse.Text = "&Browse...";
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // FileName
            // 
            this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.FileName.Location = new System.Drawing.Point(75, 82);
            this.FileName.Name = "FileName";
            this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
            this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FileName.Properties.Appearance.Options.UseBackColor = true;
            this.FileName.Properties.Appearance.Options.UseFont = true;
            this.FileName.Properties.Appearance.Options.UseForeColor = true;
            this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FileName.Size = new System.Drawing.Size(234, 20);
            this.FileName.TabIndex = 31;
            this.FileName.Tag = "Title";
            // 
            // Label1
            // 
            this.Label1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.Label1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Location = new System.Drawing.Point(11, 84);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(50, 13);
            this.Label1.TabIndex = 33;
            this.Label1.Text = "File Name:";
            // 
            // DeleteExistingData
            // 
            this.DeleteExistingData.Location = new System.Drawing.Point(13, 136);
            this.DeleteExistingData.Name = "DeleteExistingData";
            this.DeleteExistingData.Properties.AutoWidth = true;
            this.DeleteExistingData.Properties.Caption = "Delete existing data before importing this file";
            this.DeleteExistingData.Size = new System.Drawing.Size(237, 19);
            this.DeleteExistingData.TabIndex = 36;
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
            this.Cancel.Location = new System.Drawing.Point(333, 216);
            this.Cancel.Name = "Cancel";
            this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Cancel.Size = new System.Drawing.Size(68, 24);
            this.Cancel.TabIndex = 38;
            this.Cancel.Tag = "Cancel";
            this.Cancel.Text = "Cancel";
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.OK.Appearance.Options.UseFont = true;
            this.OK.Appearance.Options.UseForeColor = true;
            this.OK.Cursor = System.Windows.Forms.Cursors.Default;
            this.OK.Location = new System.Drawing.Point(257, 216);
            this.OK.Name = "OK";
            this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.OK.Size = new System.Drawing.Size(68, 24);
            this.OK.TabIndex = 37;
            this.OK.Tag = "OK";
            this.OK.Text = "OK";
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // label3
            // 
            this.label3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.label3.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.label3.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.label3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.label3.Location = new System.Drawing.Point(5, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(394, 58);
            this.label3.TabIndex = 41;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // ImportInBackground
            // 
            this.ImportInBackground.Location = new System.Drawing.Point(13, 159);
            this.ImportInBackground.Name = "ImportInBackground";
            this.ImportInBackground.Properties.AutoWidth = true;
            this.ImportInBackground.Properties.Caption = "Import in the background";
            this.ImportInBackground.Size = new System.Drawing.Size(143, 19);
            this.ImportInBackground.TabIndex = 44;
            // 
            // CheckForFileUpdates
            // 
            this.CheckForFileUpdates.Location = new System.Drawing.Point(13, 182);
            this.CheckForFileUpdates.Name = "CheckForFileUpdates";
            this.CheckForFileUpdates.Properties.AutoWidth = true;
            this.CheckForFileUpdates.Properties.Caption = "Re-import this file in the future whenever it changes";
            this.CheckForFileUpdates.Size = new System.Drawing.Size(272, 19);
            this.CheckForFileUpdates.TabIndex = 45;
            // 
            // Bitmaps16x16
            // 
            this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
            this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
            this.Bitmaps16x16.Images.SetKeyName(0, "SharePoint.bmp");
            // 
            // HeaderLine
            // 
            this.HeaderLine.EditValue = true;
            this.HeaderLine.Location = new System.Drawing.Point(13, 112);
            this.HeaderLine.Name = "HeaderLine";
            this.HeaderLine.Properties.AutoWidth = true;
            this.HeaderLine.Properties.Caption = "First line of file contains column header labels";
            this.HeaderLine.Size = new System.Drawing.Size(238, 19);
            this.HeaderLine.TabIndex = 49;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl2.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelControl2.LineVisible = true;
            this.labelControl2.Location = new System.Drawing.Point(-1, 63);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(406, 10);
            this.labelControl2.TabIndex = 51;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelControl1.LineVisible = true;
            this.labelControl1.Location = new System.Drawing.Point(-1, 203);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(406, 10);
            this.labelControl1.TabIndex = 52;
            // 
            // UserDataImportFile
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(405, 245);
            this.Controls.Add(this.HeaderLine);
            this.Controls.Add(this.CheckForFileUpdates);
            this.Controls.Add(this.ImportInBackground);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.FileName);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.DeleteExistingData);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.labelControl2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserDataImportFile";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import File";
            this.Activated += new System.EventHandler(this.AnnotationImport_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AnnotationImport_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeleteExistingData.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImportInBackground.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckForFileUpdates.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderLine.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void Browse_Click(object sender, System.EventArgs e)
		{
			string fileName = UIMisc.GetOpenFilename(Text, FileName.Text, FileFilter, DefaultExt);
			if (fileName != "") FileName.Text = fileName;
		}

		private void OK_Click(object sender, System.EventArgs e)
		{

			string fullFileName = UIMisc.CheckFileName(1, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, DefaultExt);
			if (fullFileName=="") return;
			FileName.Text = fullFileName;
			DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
		}

		private void AnnotationImport_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			return;
		}

		private void AnnotationImport_Activated(object sender, System.EventArgs e)
		{
			FileName.Focus();
		}

	}
}
