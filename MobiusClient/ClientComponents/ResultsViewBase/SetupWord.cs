using Mobius.ComOps;
using Mobius.Data;

using DevExpress.XtraEditors;

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Summary description for SetupWord.
	/// </summary>
	public class SetupWord : XtraForm
	{
		static SetupWord Instance;

		string InitialName = ""; // initial name for file when dialog entered

		public DevExpress.XtraEditors.CheckEdit FixedHeightStructs;
		public System.Windows.Forms.GroupBox Frame3;
		public DevExpress.XtraEditors.CheckEdit GraphicFormat;
		public DevExpress.XtraEditors.CheckEdit TableFormat;
		public System.Windows.Forms.GroupBox ReportScaling;
		public DevExpress.XtraEditors.CheckEdit ScaleSingle;
		public DevExpress.XtraEditors.CheckEdit ScaleNormal;
		public DevExpress.XtraEditors.CheckEdit ScaleMultiple;
		public DevExpress.XtraEditors.SimpleButton Browse;
		public System.Windows.Forms.GroupBox Frame1;
		public DevExpress.XtraEditors.LabelControl Label2;
		public DevExpress.XtraEditors.LabelControl Label3;
		public DevExpress.XtraEditors.LabelControl Label4;
		public DevExpress.XtraEditors.LabelControl Label5;
		public System.Windows.Forms.GroupBox Frame2;
		public DevExpress.XtraEditors.CheckEdit Portrait;
		public DevExpress.XtraEditors.CheckEdit Landscape;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.LabelControl Label6;
		public DevExpress.XtraEditors.TextEdit FileName;
		public DevExpress.XtraEditors.TextEdit LeftMargin;
		public DevExpress.XtraEditors.TextEdit RightMargin;
		public DevExpress.XtraEditors.TextEdit TopMargin;
		public DevExpress.XtraEditors.TextEdit BottomMargin;
		public DevExpress.XtraEditors.CheckEdit StructureObject;
		public DevExpress.XtraEditors.CheckEdit ExportInBackground;
        private ImageList Bitmaps16x16;
		public ComboBoxEdit RepeatCount;
		private SimpleButton OrientationImage;
		private ImageList Bitmaps32x32;
		private LabelControl labelControl1;
		public CheckEdit DuplicateKeyValues;
		public CheckEdit SaveAsDefaultFolderOption;
		private IContainer components;
	    private bool _alertExport;

		public SetupWord()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Setup & show the dialog
		/// </summary>
		/// <param name="rf"></param>

		public static new DialogResult ShowDialog(
			ResultsFormat rf,
			bool useExistingOptionValues,
            bool alertExport = false)
		{
			if (Instance == null) Instance = new SetupWord();

			return Instance.ShowDialog2(rf, useExistingOptionValues, alertExport);
		}

		public DialogResult ShowDialog2(
			ResultsFormat rf,
			bool useExistingOptionValues,
            bool alertExport)
		{
		    _alertExport = alertExport;
            string fileName;
			int margin;

			rf.OutputDestination = OutputDest.Word;
			rf.QualifiedNumberSplit = QnfEnum.Combined; // only allow combined for Word

			if (useExistingOptionValues) FileName.Text = rf.OutputFileName;
			else GetDefaultExportParameters(rf);

			if (rf.PageOrientation == Orientation.Vertical) Portrait.Checked = true;
			else Landscape.Checked = true;
			LeftMargin.Text = ConvertMilliinchesToString(rf.PageMargins.Left);
			TopMargin.Text = ConvertMilliinchesToString(rf.PageMargins.Top);
			RightMargin.Text = ConvertMilliinchesToString(rf.PageMargins.Right);
			BottomMargin.Text = ConvertMilliinchesToString(rf.PageMargins.Bottom);

			if (rf.RepeatCount < 0) ScaleNormal.Checked = true; // automatic fit 
			else if (rf.RepeatCount == 0) ScaleSingle.Checked = true; // fit across page 
			else // repeat multiple times across page 
			{
				ScaleMultiple.Checked = true;
				RepeatCount.Text = (rf.RepeatCount + 1).ToString(); // number of repeats 
			}

			if (rf.ExportStructureFormat == ExportStructureFormat.Insight)
				StructureObject.Checked = true;
			else StructureObject.Checked = false;

			FixedHeightStructs.Checked = rf.FixedHeightStructures;
			DuplicateKeyValues.Checked = rf.DuplicateKeyTableValues;
			ExportInBackground.Checked = rf.RunInBackground;

			DialogResult dr = ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return dr;

			rf.OutputFileName = FileName.Text;

			GetMarginsAndOrientation(rf, ref SS.I.WordPageWidth, ref SS.I.WordPageHeight, ref SS.I.WordPageMargins);

			if (ScaleNormal.Checked) rf.RepeatCount = -1; // automatic fit 
			else if (ScaleSingle.Checked) rf.RepeatCount = 0; // fit across page 
			else // repeat multiple times across page 
			{
				string tok = RepeatCount.Text;
				rf.RepeatCount = Int32.Parse(tok) - 1; // number of repeats 
			}

			if (StructureObject.Checked)
				rf.ExportStructureFormat = ExportStructureFormat.Insight;
			else rf.ExportStructureFormat = ExportStructureFormat.Metafiles;

			rf.FixedHeightStructures = FixedHeightStructs.Checked;
			rf.DuplicateKeyTableValues = DuplicateKeyValues.Checked;
			rf.RunInBackground = ExportInBackground.Checked;

			SaveDefaultExportParameters(rf);

			return DialogResult.OK;
		}

		/// <summary>
		/// Get default export parameters for user
		/// </summary>
		/// <param name="rf"></param>

		void GetDefaultExportParameters(ResultsFormat rf)
		{
			string txt = Preferences.Get("WordExportDefaults");
			do
			{
				if (!Lex.IsNullOrEmpty(txt))
					try
					{
						string[] sa = txt.Split('\t');
						rf.PageOrientation = (Orientation)int.Parse(sa[0]);
						rf.DuplicateKeyTableValues = bool.Parse(sa[1]);
						rf.RunInBackground = bool.Parse(sa[2]);
						break; // finish up
					}
					catch (Exception ex) { ex = ex; } // fall through to defaults on error

				rf.PageOrientation = Orientation.Vertical;
				rf.DuplicateKeyTableValues = false;
				rf.RunInBackground = false;
			} while (false);

			if (rf.QueryManager != null && rf.QueryManager.Query != null &&
			 rf.QueryManager.Query.DuplicateKeyValues)
				rf.DuplicateKeyTableValues = true;

			if (rf.ExportStructureFormat != ExportStructureFormat.Metafiles)
				rf.ExportStructureFormat = ExportStructureFormat.Insight; // default format

			if (rf.PageMargins == null) // setup page if not done
			{
				rf.PageWidth = SS.I.WordPageWidth;
				rf.PageHeight = SS.I.WordPageHeight;
				int m = SS.I.WordPageMargins.Left;
				rf.PageMargins = new PageMargins(m, m, m, m);

				rf.RepeatCount = -1; // automatic fit
			}

			return;
		}

		/// <summary>
		/// Save default export parameters for user
		/// </summary>
		/// <param name="rf"></param>

		void SaveDefaultExportParameters(ResultsFormat rf)
		{
			string txt =
					((int)rf.PageOrientation).ToString() + '\t' +
					rf.DuplicateKeyTableValues + '\t' +
					rf.RunInBackground;

			Preferences.Set("WordExportDefaults", txt);

			if (SaveAsDefaultFolderOption.Checked)
				SetupExcel.SaveDefaultFolder(rf.OutputFileName);
			return;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWord));
			this.FixedHeightStructs = new DevExpress.XtraEditors.CheckEdit();
			this.Frame3 = new System.Windows.Forms.GroupBox();
			this.GraphicFormat = new DevExpress.XtraEditors.CheckEdit();
			this.TableFormat = new DevExpress.XtraEditors.CheckEdit();
			this.ReportScaling = new System.Windows.Forms.GroupBox();
			this.ScaleSingle = new DevExpress.XtraEditors.CheckEdit();
			this.ScaleNormal = new DevExpress.XtraEditors.CheckEdit();
			this.RepeatCount = new DevExpress.XtraEditors.ComboBoxEdit();
			this.ScaleMultiple = new DevExpress.XtraEditors.CheckEdit();
			this.FileName = new DevExpress.XtraEditors.TextEdit();
			this.Browse = new DevExpress.XtraEditors.SimpleButton();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.LeftMargin = new DevExpress.XtraEditors.TextEdit();
			this.RightMargin = new DevExpress.XtraEditors.TextEdit();
			this.TopMargin = new DevExpress.XtraEditors.TextEdit();
			this.BottomMargin = new DevExpress.XtraEditors.TextEdit();
			this.Label2 = new DevExpress.XtraEditors.LabelControl();
			this.Label3 = new DevExpress.XtraEditors.LabelControl();
			this.Label4 = new DevExpress.XtraEditors.LabelControl();
			this.Label5 = new DevExpress.XtraEditors.LabelControl();
			this.Frame2 = new System.Windows.Forms.GroupBox();
			this.OrientationImage = new DevExpress.XtraEditors.SimpleButton();
			this.Bitmaps32x32 = new System.Windows.Forms.ImageList(this.components);
			this.Portrait = new DevExpress.XtraEditors.CheckEdit();
			this.Landscape = new DevExpress.XtraEditors.CheckEdit();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Label6 = new DevExpress.XtraEditors.LabelControl();
			this.StructureObject = new DevExpress.XtraEditors.CheckEdit();
			this.ExportInBackground = new DevExpress.XtraEditors.CheckEdit();
			this.Bitmaps16x16 = new System.Windows.Forms.ImageList(this.components);
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			this.DuplicateKeyValues = new DevExpress.XtraEditors.CheckEdit();
			this.SaveAsDefaultFolderOption = new DevExpress.XtraEditors.CheckEdit();
			((System.ComponentModel.ISupportInitialize)(this.FixedHeightStructs.Properties)).BeginInit();
			this.Frame3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GraphicFormat.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TableFormat.Properties)).BeginInit();
			this.ReportScaling.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ScaleSingle.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ScaleNormal.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RepeatCount.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ScaleMultiple.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).BeginInit();
			this.Frame1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LeftMargin.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RightMargin.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TopMargin.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BottomMargin.Properties)).BeginInit();
			this.Frame2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Portrait.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Landscape.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StructureObject.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// FixedHeightStructs
			// 
			this.FixedHeightStructs.Cursor = System.Windows.Forms.Cursors.Default;
			this.FixedHeightStructs.Location = new System.Drawing.Point(17, 334);
			this.FixedHeightStructs.Name = "FixedHeightStructs";
			this.FixedHeightStructs.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.FixedHeightStructs.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FixedHeightStructs.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FixedHeightStructs.Properties.Appearance.Options.UseBackColor = true;
			this.FixedHeightStructs.Properties.Appearance.Options.UseFont = true;
			this.FixedHeightStructs.Properties.Appearance.Options.UseForeColor = true;
			this.FixedHeightStructs.Properties.Caption = "Format structures into fixed size boxes";
			this.FixedHeightStructs.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FixedHeightStructs.Size = new System.Drawing.Size(321, 21);
			this.FixedHeightStructs.TabIndex = 37;
			this.FixedHeightStructs.Tag = "FixedHeightStructs";
			// 
			// Frame3
			// 
			this.Frame3.BackColor = System.Drawing.Color.Transparent;
			this.Frame3.Controls.Add(this.GraphicFormat);
			this.Frame3.Controls.Add(this.TableFormat);
			this.Frame3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame3.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame3.Location = new System.Drawing.Point(10, 362);
			this.Frame3.Name = "Frame3";
			this.Frame3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame3.Size = new System.Drawing.Size(481, 89);
			this.Frame3.TabIndex = 36;
			this.Frame3.TabStop = false;
			this.Frame3.Text = "Output Format";
			this.Frame3.Visible = false;
			// 
			// GraphicFormat
			// 
			this.GraphicFormat.Cursor = System.Windows.Forms.Cursors.Default;
			this.GraphicFormat.Enabled = false;
			this.GraphicFormat.Location = new System.Drawing.Point(10, 51);
			this.GraphicFormat.Name = "GraphicFormat";
			this.GraphicFormat.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.GraphicFormat.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GraphicFormat.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GraphicFormat.Properties.Appearance.Options.UseBackColor = true;
			this.GraphicFormat.Properties.Appearance.Options.UseFont = true;
			this.GraphicFormat.Properties.Appearance.Options.UseForeColor = true;
			this.GraphicFormat.Properties.Caption = "Graphics - Output data as a series of uneditable \"pictures\" one per page";
			this.GraphicFormat.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.GraphicFormat.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.GraphicFormat.Size = new System.Drawing.Size(460, 21);
			this.GraphicFormat.TabIndex = 4;
			// 
			// TableFormat
			// 
			this.TableFormat.Cursor = System.Windows.Forms.Cursors.Default;
			this.TableFormat.EditValue = true;
			this.TableFormat.Location = new System.Drawing.Point(10, 21);
			this.TableFormat.Name = "TableFormat";
			this.TableFormat.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.TableFormat.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TableFormat.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TableFormat.Properties.Appearance.Options.UseBackColor = true;
			this.TableFormat.Properties.Appearance.Options.UseFont = true;
			this.TableFormat.Properties.Appearance.Options.UseForeColor = true;
			this.TableFormat.Properties.Caption = "Table - Output data, including structures, into a native Word table";
			this.TableFormat.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.TableFormat.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.TableFormat.Size = new System.Drawing.Size(441, 21);
			this.TableFormat.TabIndex = 2;
			// 
			// ReportScaling
			// 
			this.ReportScaling.BackColor = System.Drawing.Color.Transparent;
			this.ReportScaling.Controls.Add(this.ScaleSingle);
			this.ReportScaling.Controls.Add(this.ScaleNormal);
			this.ReportScaling.Controls.Add(this.RepeatCount);
			this.ReportScaling.Controls.Add(this.ScaleMultiple);
			this.ReportScaling.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReportScaling.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportScaling.Location = new System.Drawing.Point(10, 208);
			this.ReportScaling.Name = "ReportScaling";
			this.ReportScaling.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ReportScaling.Size = new System.Drawing.Size(482, 119);
			this.ReportScaling.TabIndex = 35;
			this.ReportScaling.TabStop = false;
			this.ReportScaling.Text = " Report Scaling ";
			this.ReportScaling.Visible = false;
			// 
			// ScaleSingle
			// 
			this.ScaleSingle.Cursor = System.Windows.Forms.Cursors.Default;
			this.ScaleSingle.Location = new System.Drawing.Point(10, 53);
			this.ScaleSingle.Name = "ScaleSingle";
			this.ScaleSingle.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ScaleSingle.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScaleSingle.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ScaleSingle.Properties.Appearance.Options.UseBackColor = true;
			this.ScaleSingle.Properties.Appearance.Options.UseFont = true;
			this.ScaleSingle.Properties.Appearance.Options.UseForeColor = true;
			this.ScaleSingle.Properties.Caption = "Single Section - Fit report across the page in a single unbroken section";
			this.ScaleSingle.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ScaleSingle.Properties.RadioGroupIndex = 1;
			this.ScaleSingle.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ScaleSingle.Size = new System.Drawing.Size(456, 21);
			this.ScaleSingle.TabIndex = 12;
			this.ScaleSingle.TabStop = false;
			this.ScaleSingle.Tag = "ScaleSingle";
			// 
			// ScaleNormal
			// 
			this.ScaleNormal.Cursor = System.Windows.Forms.Cursors.Default;
			this.ScaleNormal.EditValue = true;
			this.ScaleNormal.Location = new System.Drawing.Point(10, 21);
			this.ScaleNormal.Name = "ScaleNormal";
			this.ScaleNormal.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ScaleNormal.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScaleNormal.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ScaleNormal.Properties.Appearance.Options.UseBackColor = true;
			this.ScaleNormal.Properties.Appearance.Options.UseFont = true;
			this.ScaleNormal.Properties.Appearance.Options.UseForeColor = true;
			this.ScaleNormal.Properties.Caption = "Normal Size - Automatic formatting into multiple sections or columns";
			this.ScaleNormal.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ScaleNormal.Properties.RadioGroupIndex = 1;
			this.ScaleNormal.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ScaleNormal.Size = new System.Drawing.Size(462, 21);
			this.ScaleNormal.TabIndex = 11;
			this.ScaleNormal.Tag = "ScaleNormal";
			// 
			// RepeatCount
			// 
			this.RepeatCount.EditValue = "2";
			this.RepeatCount.Enabled = false;
			this.RepeatCount.Location = new System.Drawing.Point(402, 83);
			this.RepeatCount.Name = "RepeatCount";
			this.RepeatCount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.RepeatCount.Properties.DropDownRows = 9;
			this.RepeatCount.Properties.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
			this.RepeatCount.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.RepeatCount.Size = new System.Drawing.Size(68, 22);
			this.RepeatCount.TabIndex = 42;
			// 
			// ScaleMultiple
			// 
			this.ScaleMultiple.Cursor = System.Windows.Forms.Cursors.Default;
			this.ScaleMultiple.Location = new System.Drawing.Point(10, 85);
			this.ScaleMultiple.Name = "ScaleMultiple";
			this.ScaleMultiple.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ScaleMultiple.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScaleMultiple.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ScaleMultiple.Properties.Appearance.Options.UseBackColor = true;
			this.ScaleMultiple.Properties.Appearance.Options.UseFont = true;
			this.ScaleMultiple.Properties.Appearance.Options.UseForeColor = true;
			this.ScaleMultiple.Properties.Caption = "Multiple Columns - Repeat report across page multiple times:";
			this.ScaleMultiple.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.ScaleMultiple.Properties.RadioGroupIndex = 1;
			this.ScaleMultiple.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ScaleMultiple.Size = new System.Drawing.Size(460, 21);
			this.ScaleMultiple.TabIndex = 13;
			this.ScaleMultiple.TabStop = false;
			this.ScaleMultiple.Tag = "ScaleMultiple";
			this.ScaleMultiple.CheckedChanged += new System.EventHandler(this.ScaleMultiple_CheckedChanged);
			// 
			// FileName
			// 
			this.FileName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.FileName.Location = new System.Drawing.Point(77, 10);
			this.FileName.Name = "FileName";
			this.FileName.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.FileName.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FileName.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FileName.Properties.Appearance.Options.UseBackColor = true;
			this.FileName.Properties.Appearance.Options.UseFont = true;
			this.FileName.Properties.Appearance.Options.UseForeColor = true;
			this.FileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FileName.Size = new System.Drawing.Size(294, 24);
			this.FileName.TabIndex = 28;
			this.FileName.Tag = "Title";
			// 
			// Browse
			// 
			this.Browse.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Browse.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Browse.Appearance.Options.UseFont = true;
			this.Browse.Appearance.Options.UseForeColor = true;
			this.Browse.Cursor = System.Windows.Forms.Cursors.Default;
			this.Browse.Location = new System.Drawing.Point(379, 8);
			this.Browse.Name = "Browse";
			this.Browse.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Browse.Size = new System.Drawing.Size(112, 25);
			this.Browse.TabIndex = 29;
			this.Browse.Text = "&Browse...";
			this.Browse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// Frame1
			// 
			this.Frame1.BackColor = System.Drawing.Color.Transparent;
			this.Frame1.Controls.Add(this.LeftMargin);
			this.Frame1.Controls.Add(this.RightMargin);
			this.Frame1.Controls.Add(this.TopMargin);
			this.Frame1.Controls.Add(this.BottomMargin);
			this.Frame1.Controls.Add(this.Label2);
			this.Frame1.Controls.Add(this.Label3);
			this.Frame1.Controls.Add(this.Label4);
			this.Frame1.Controls.Add(this.Label5);
			this.Frame1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame1.Location = new System.Drawing.Point(10, 461);
			this.Frame1.Name = "Frame1";
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(291, 118);
			this.Frame1.TabIndex = 33;
			this.Frame1.TabStop = false;
			this.Frame1.Text = " Margins ";
			this.Frame1.Visible = false;
			// 
			// LeftMargin
			// 
			this.LeftMargin.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.LeftMargin.EditValue = "1\"";
			this.LeftMargin.Location = new System.Drawing.Point(60, 32);
			this.LeftMargin.Name = "LeftMargin";
			this.LeftMargin.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.LeftMargin.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LeftMargin.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.LeftMargin.Properties.Appearance.Options.UseBackColor = true;
			this.LeftMargin.Properties.Appearance.Options.UseFont = true;
			this.LeftMargin.Properties.Appearance.Options.UseForeColor = true;
			this.LeftMargin.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.LeftMargin.Size = new System.Drawing.Size(72, 24);
			this.LeftMargin.TabIndex = 7;
			this.LeftMargin.Tag = "Left";
			// 
			// RightMargin
			// 
			this.RightMargin.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.RightMargin.EditValue = "1\"";
			this.RightMargin.Location = new System.Drawing.Point(200, 32);
			this.RightMargin.Name = "RightMargin";
			this.RightMargin.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.RightMargin.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RightMargin.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.RightMargin.Properties.Appearance.Options.UseBackColor = true;
			this.RightMargin.Properties.Appearance.Options.UseFont = true;
			this.RightMargin.Properties.Appearance.Options.UseForeColor = true;
			this.RightMargin.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RightMargin.Size = new System.Drawing.Size(72, 24);
			this.RightMargin.TabIndex = 8;
			this.RightMargin.Tag = "Right";
			// 
			// TopMargin
			// 
			this.TopMargin.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.TopMargin.EditValue = "1\"";
			this.TopMargin.Location = new System.Drawing.Point(60, 75);
			this.TopMargin.Name = "TopMargin";
			this.TopMargin.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.TopMargin.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TopMargin.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.TopMargin.Properties.Appearance.Options.UseBackColor = true;
			this.TopMargin.Properties.Appearance.Options.UseFont = true;
			this.TopMargin.Properties.Appearance.Options.UseForeColor = true;
			this.TopMargin.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.TopMargin.Size = new System.Drawing.Size(72, 24);
			this.TopMargin.TabIndex = 9;
			this.TopMargin.Tag = "Top";
			// 
			// BottomMargin
			// 
			this.BottomMargin.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.BottomMargin.EditValue = "1\"";
			this.BottomMargin.Location = new System.Drawing.Point(200, 75);
			this.BottomMargin.Name = "BottomMargin";
			this.BottomMargin.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.BottomMargin.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BottomMargin.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.BottomMargin.Properties.Appearance.Options.UseBackColor = true;
			this.BottomMargin.Properties.Appearance.Options.UseFont = true;
			this.BottomMargin.Properties.Appearance.Options.UseForeColor = true;
			this.BottomMargin.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.BottomMargin.Size = new System.Drawing.Size(72, 24);
			this.BottomMargin.TabIndex = 10;
			this.BottomMargin.Tag = "Bottom";
			// 
			// Label2
			// 
			this.Label2.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label2.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label2.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label2.Appearance.Options.UseBackColor = true;
			this.Label2.Appearance.Options.UseFont = true;
			this.Label2.Appearance.Options.UseForeColor = true;
			this.Label2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label2.Location = new System.Drawing.Point(10, 32);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(28, 17);
			this.Label2.TabIndex = 22;
			this.Label2.Text = "&Left:";
			// 
			// Label3
			// 
			this.Label3.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label3.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label3.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label3.Appearance.Options.UseBackColor = true;
			this.Label3.Appearance.Options.UseFont = true;
			this.Label3.Appearance.Options.UseForeColor = true;
			this.Label3.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label3.Location = new System.Drawing.Point(150, 32);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(37, 17);
			this.Label3.TabIndex = 21;
			this.Label3.Text = "&Right:";
			// 
			// Label4
			// 
			this.Label4.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label4.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label4.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label4.Appearance.Options.UseBackColor = true;
			this.Label4.Appearance.Options.UseFont = true;
			this.Label4.Appearance.Options.UseForeColor = true;
			this.Label4.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label4.Location = new System.Drawing.Point(10, 75);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(29, 17);
			this.Label4.TabIndex = 20;
			this.Label4.Text = "&Top:";
			// 
			// Label5
			// 
			this.Label5.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label5.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label5.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label5.Appearance.Options.UseBackColor = true;
			this.Label5.Appearance.Options.UseFont = true;
			this.Label5.Appearance.Options.UseForeColor = true;
			this.Label5.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label5.Location = new System.Drawing.Point(150, 75);
			this.Label5.Name = "Label5";
			this.Label5.Size = new System.Drawing.Size(48, 17);
			this.Label5.TabIndex = 19;
			this.Label5.Text = "&Bottom:";
			// 
			// Frame2
			// 
			this.Frame2.BackColor = System.Drawing.Color.Transparent;
			this.Frame2.Controls.Add(this.OrientationImage);
			this.Frame2.Controls.Add(this.Portrait);
			this.Frame2.Controls.Add(this.Landscape);
			this.Frame2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame2.Location = new System.Drawing.Point(10, 41);
			this.Frame2.Name = "Frame2";
			this.Frame2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame2.Size = new System.Drawing.Size(482, 74);
			this.Frame2.TabIndex = 32;
			this.Frame2.TabStop = false;
			this.Frame2.Text = " Page Orientation ";
			// 
			// OrientationImage
			// 
			this.OrientationImage.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.OrientationImage.Appearance.BackColor2 = System.Drawing.Color.Transparent;
			this.OrientationImage.Appearance.BorderColor = System.Drawing.Color.Transparent;
			this.OrientationImage.Appearance.Options.UseBackColor = true;
			this.OrientationImage.Appearance.Options.UseBorderColor = true;
			this.OrientationImage.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.OrientationImage.ImageOptions.ImageIndex = 0;
			this.OrientationImage.ImageOptions.ImageList = this.Bitmaps32x32;
			this.OrientationImage.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
			this.OrientationImage.Location = new System.Drawing.Point(12, 16);
			this.OrientationImage.Name = "OrientationImage";
			this.OrientationImage.Size = new System.Drawing.Size(48, 46);
			this.OrientationImage.TabIndex = 9;
			// 
			// Bitmaps32x32
			// 
			this.Bitmaps32x32.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps32x32.ImageStream")));
			this.Bitmaps32x32.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps32x32.Images.SetKeyName(0, "Portrait32.bmp");
			this.Bitmaps32x32.Images.SetKeyName(1, "Landscape32.bmp");
			// 
			// Portrait
			// 
			this.Portrait.Cursor = System.Windows.Forms.Cursors.Default;
			this.Portrait.EditValue = true;
			this.Portrait.Location = new System.Drawing.Point(65, 30);
			this.Portrait.Name = "Portrait";
			this.Portrait.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Portrait.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Portrait.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Portrait.Properties.Appearance.Options.UseBackColor = true;
			this.Portrait.Properties.Appearance.Options.UseFont = true;
			this.Portrait.Properties.Appearance.Options.UseForeColor = true;
			this.Portrait.Properties.Caption = "P&ortrait";
			this.Portrait.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Portrait.Properties.RadioGroupIndex = 0;
			this.Portrait.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Portrait.Size = new System.Drawing.Size(89, 21);
			this.Portrait.TabIndex = 5;
			this.Portrait.Tag = "Portrait";
			this.Portrait.CheckedChanged += new System.EventHandler(this.Portrait_CheckedChanged);
			// 
			// Landscape
			// 
			this.Landscape.Cursor = System.Windows.Forms.Cursors.Default;
			this.Landscape.Location = new System.Drawing.Point(161, 30);
			this.Landscape.Name = "Landscape";
			this.Landscape.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Landscape.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Landscape.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Landscape.Properties.Appearance.Options.UseBackColor = true;
			this.Landscape.Properties.Appearance.Options.UseFont = true;
			this.Landscape.Properties.Appearance.Options.UseForeColor = true;
			this.Landscape.Properties.Caption = "L&andscape";
			this.Landscape.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Landscape.Properties.RadioGroupIndex = 0;
			this.Landscape.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Landscape.Size = new System.Drawing.Size(96, 21);
			this.Landscape.TabIndex = 6;
			this.Landscape.TabStop = false;
			this.Landscape.Tag = "Landscape";
			this.Landscape.CheckedChanged += new System.EventHandler(this.Landscape_CheckedChanged);
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
			this.Cancel.Location = new System.Drawing.Point(326, 183);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(82, 27);
			this.Cancel.TabIndex = 31;
			this.Cancel.Tag = "Cancel";
			this.Cancel.Text = "Cancel";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(231, 183);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(82, 27);
			this.OK.TabIndex = 30;
			this.OK.Tag = "OK";
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Label6
			// 
			this.Label6.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Label6.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label6.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label6.Appearance.Options.UseBackColor = true;
			this.Label6.Appearance.Options.UseFont = true;
			this.Label6.Appearance.Options.UseForeColor = true;
			this.Label6.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label6.Location = new System.Drawing.Point(10, 14);
			this.Label6.Name = "Label6";
			this.Label6.Size = new System.Drawing.Size(67, 17);
			this.Label6.TabIndex = 34;
			this.Label6.Text = "File Name:";
			// 
			// StructureObject
			// 
			this.StructureObject.Cursor = System.Windows.Forms.Cursors.Default;
			this.StructureObject.EditValue = true;
			this.StructureObject.Location = new System.Drawing.Point(7, 690);
			this.StructureObject.Name = "StructureObject";
			this.StructureObject.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.StructureObject.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StructureObject.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.StructureObject.Properties.Appearance.Options.UseBackColor = true;
			this.StructureObject.Properties.Appearance.Options.UseFont = true;
			this.StructureObject.Properties.Appearance.Options.UseForeColor = true;
			this.StructureObject.Properties.Caption = "Allow editing of any structures sent to Word";
			this.StructureObject.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.StructureObject.Size = new System.Drawing.Size(322, 21);
			this.StructureObject.TabIndex = 39;
			this.StructureObject.Visible = false;
			// 
			// ExportInBackground
			// 
			this.ExportInBackground.Cursor = System.Windows.Forms.Cursors.Default;
			this.ExportInBackground.Location = new System.Drawing.Point(19, 177);
			this.ExportInBackground.Name = "ExportInBackground";
			this.ExportInBackground.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.ExportInBackground.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExportInBackground.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportInBackground.Properties.Appearance.Options.UseBackColor = true;
			this.ExportInBackground.Properties.Appearance.Options.UseFont = true;
			this.ExportInBackground.Properties.Appearance.Options.UseForeColor = true;
			this.ExportInBackground.Properties.Caption = "Export in the background";
			this.ExportInBackground.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ExportInBackground.Size = new System.Drawing.Size(174, 21);
			this.ExportInBackground.TabIndex = 12;
			// 
			// Bitmaps16x16
			// 
			this.Bitmaps16x16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Bitmaps16x16.ImageStream")));
			this.Bitmaps16x16.TransparentColor = System.Drawing.Color.Cyan;
			this.Bitmaps16x16.Images.SetKeyName(0, "SharePoint.bmp");
			// 
			// labelControl1
			// 
			this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-1, 166);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(427, 11);
			this.labelControl1.TabIndex = 43;
			// 
			// DuplicateKeyValues
			// 
			this.DuplicateKeyValues.Cursor = System.Windows.Forms.Cursors.Default;
			this.DuplicateKeyValues.Location = new System.Drawing.Point(19, 122);
			this.DuplicateKeyValues.Name = "DuplicateKeyValues";
			this.DuplicateKeyValues.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.DuplicateKeyValues.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DuplicateKeyValues.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DuplicateKeyValues.Properties.Appearance.Options.UseBackColor = true;
			this.DuplicateKeyValues.Properties.Appearance.Options.UseFont = true;
			this.DuplicateKeyValues.Properties.Appearance.Options.UseForeColor = true;
			this.DuplicateKeyValues.Properties.Caption = "Duplicate data that occurs only once per compound";
			this.DuplicateKeyValues.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.DuplicateKeyValues.Size = new System.Drawing.Size(382, 21);
			this.DuplicateKeyValues.TabIndex = 44;
			this.DuplicateKeyValues.Tag = "FixedHeightStructs";
			// 
			// SaveAsDefaultFolderOption
			// 
			this.SaveAsDefaultFolderOption.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveAsDefaultFolderOption.Location = new System.Drawing.Point(19, 150);
			this.SaveAsDefaultFolderOption.Name = "SaveAsDefaultFolderOption";
			this.SaveAsDefaultFolderOption.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveAsDefaultFolderOption.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseBackColor = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseFont = true;
			this.SaveAsDefaultFolderOption.Properties.Appearance.Options.UseForeColor = true;
			this.SaveAsDefaultFolderOption.Properties.Caption = "Use this as the default export folder in the future";
			this.SaveAsDefaultFolderOption.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.SaveAsDefaultFolderOption.Size = new System.Drawing.Size(339, 21);
			this.SaveAsDefaultFolderOption.TabIndex = 45;
			this.SaveAsDefaultFolderOption.Tag = "FixedHeightStructs";
			// 
			// SetupWord
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(415, 218);
			this.Controls.Add(this.DuplicateKeyValues);
			this.Controls.Add(this.FixedHeightStructs);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.ExportInBackground);
			this.Controls.Add(this.StructureObject);
			this.Controls.Add(this.FileName);
			this.Controls.Add(this.Label6);
			this.Controls.Add(this.Browse);
			this.Controls.Add(this.Frame1);
			this.Controls.Add(this.Frame2);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Frame3);
			this.Controls.Add(this.ReportScaling);
			this.Controls.Add(this.SaveAsDefaultFolderOption);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetupWord";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Export to Microsoft Word";
			this.Activated += new System.EventHandler(this.SetupWord_Activated);
			this.VisibleChanged += new System.EventHandler(this.SetupWord_VisibleChanged);
			((System.ComponentModel.ISupportInitialize)(this.FixedHeightStructs.Properties)).EndInit();
			this.Frame3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GraphicFormat.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TableFormat.Properties)).EndInit();
			this.ReportScaling.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ScaleSingle.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ScaleNormal.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RepeatCount.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ScaleMultiple.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FileName.Properties)).EndInit();
			this.Frame1.ResumeLayout(false);
			this.Frame1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LeftMargin.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RightMargin.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TopMargin.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BottomMargin.Properties)).EndInit();
			this.Frame2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Portrait.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Landscape.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StructureObject.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportInBackground.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicateKeyValues.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SaveAsDefaultFolderOption.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void OK_Click(object sender, System.EventArgs e)
		{
			string fullFileName = UIMisc.CheckFileName(2, FileName, InitialName, ClientDirs.DefaultMobiusUserDocumentsFolder, ".doc");
			if (fullFileName=="") return;
			if (!UIMisc.CanWriteFile(fullFileName, true)) return;
			FileName.Text = fullFileName;

			if (ExportInBackground.Checked)
			{ // if background export of unc file tell user if we can't write directly to the file
				if (UIMisc.CanWriteFileFromServiceAccount(fullFileName) == DialogResult.Cancel) return;
                if (_alertExport && UIMisc.PathContainsDrive(fullFileName) == DialogResult.Cancel) return;
            }

			DialogResult = DialogResult.OK;
			return;
		}

/// <summary>
/// Browse regular file system name
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>

		private void Browse_Click(object sender, System.EventArgs e)
		{
			string fileName;

			string initialFile = FileName.Text;
			if (Lex.IsNullOrEmpty(initialFile))
				initialFile = ClientDirs.DefaultMobiusUserDocumentsFolder;

			string filter =
				"Microsoft Word Files (*.doc)|*.doc|" +
				"All files (*.*)|*.*";
			fileName = UIMisc.GetSaveAsFilename("Word File Name", initialFile, filter, ".doc");
			if (fileName != "") FileName.Text = fileName;
			return;
		}

		private void SetupWord_Activated(object sender, System.EventArgs e)
		{
			FileName.Focus();
		}

		private void Portrait_CheckedChanged(object sender, System.EventArgs e)
		{
			if (Portrait.Checked)
				OrientationImage.ImageIndex = 0;
		}

		private void Landscape_CheckedChanged(object sender, System.EventArgs e)
		{
			if (Landscape.Checked)
				OrientationImage.ImageIndex = 1;
		}

		private void ScaleMultiple_CheckedChanged(object sender, System.EventArgs e)
		{
			if (ScaleMultiple.Checked)
			{
				RepeatCount.Enabled = true;
				if (Visible) RepeatCount.Focus();
			}
			else RepeatCount.Enabled = false;
		}

		private void SetupWord_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible) InitialName = FileName.Text; // save initial name for later compare
		}

		/// <summary>
		/// Convert char string inch quantity to integer milliinches
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static int ConvertStringToMilliinches(
			string s)
		{
			string txt;
			int units, i1, i2, mi;
			double r1, factor = 0;

			txt = s.ToLower();
			if ((i1 = txt.IndexOf("\"")) >= 0) units = SessionState.UNITS_INCH;
			else if ((i1 = txt.IndexOf("in")) >= 0) units = SessionState.UNITS_INCH;
			else if ((i1 = txt.IndexOf("cm")) >= 0) units = SessionState.UNITS_CM;
			else if ((i1 = txt.IndexOf("ce")) >= 0) units = SessionState.UNITS_CM;
			else if ((i1 = txt.IndexOf("pt")) >= 0) units = SessionState.UNITS_PTS;
			else if ((i1 = txt.IndexOf("po")) >= 0) units = SessionState.UNITS_PTS;
			else  // not specified
			{
				units = SS.I.UnitsOfMeasure; // user's preferred units 
				i1 = txt.Length;
			}

			txt = txt.Substring(0, i1).Trim();
			try { r1 = Double.Parse(txt); }
			catch (Exception ex) { return -1; }

			if (units == SessionState.UNITS_INCH) factor = 1.0;
			else if (units == SessionState.UNITS_CM) factor = 2.54;
			else if (units == SessionState.UNITS_PTS) factor = 72.0;

			mi = (int)(r1 * 1000 / factor + .499); // convert rounded float to milliinches 
			return mi;
		}

		/// <summary>
		/// Get margin and page orientation information from dialog box
		/// </summary>
		/// <param name="rf"></param>
		/// <param name="defaultMargin"></param>
		/// <returns></returns>

		bool GetMarginsAndOrientation(
			ResultsFormat rf,
			ref int pageWidth,
			ref int pageHeight,
			ref PageMargins margins)
		{
			string txt;
			int i1;

			rf.PageMargins = margins;

			txt = LeftMargin.Text;
			i1 = ConvertStringToMilliinches(txt);
			if (i1 > 0) rf.PageMargins.Left = i1;

			txt = RightMargin.Text;
			i1 = ConvertStringToMilliinches(txt);
			if (i1 > 0) rf.PageMargins.Right = i1;

			txt = TopMargin.Text;
			i1 = ConvertStringToMilliinches(txt);
			if (i1 > 0) rf.PageMargins.Top = i1;

			txt = BottomMargin.Text;
			i1 = ConvertStringToMilliinches(txt);
			if (i1 > 0) rf.PageMargins.Bottom = i1;

			if (Portrait.Checked)
			{
				rf.PageOrientation = Orientation.Vertical;
				if (pageWidth < pageHeight)
				{
					rf.PageWidth = pageWidth;
					rf.PageHeight = pageHeight;
				}
				else
				{
					rf.PageWidth = pageHeight;
					rf.PageHeight = pageWidth;
				}
			}

			else // landscape
			{
				rf.PageOrientation = Orientation.Horizontal;
				if (pageWidth > pageHeight)
				{
					rf.PageWidth = pageWidth;
					rf.PageHeight = pageHeight;
				}
				else
				{
					rf.PageWidth = pageHeight;
					rf.PageHeight = pageWidth;
				}
			}

			return true;
		}

		/// <summary>
		/// Convert int milliinches to string with units (inches (") for now)
		/// </summary>
		/// <param name="mi"></param>
		/// <returns></returns>

		public static string ConvertMilliinchesToString(
			int mi)
		{
			string fmt;

			if ((mi % 1000) == 0) fmt = "{0}";
			else fmt = "{0:F1}";
			String s = String.Format(fmt, mi / 1000.0) + "\""; // assume inch units for now
			return s;
		}

	}
}
