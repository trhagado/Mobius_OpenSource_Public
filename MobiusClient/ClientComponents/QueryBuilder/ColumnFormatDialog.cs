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
/// Generic entry point for formatting dialog
/// </summary>
	public class ColumnFormatDialog
	{

/// <summary>
/// Show formatting dialog appropriate for data type
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		public static DialogResult Show(
			QueryColumn qc)
		{
			ColumnFormatEnum displayFormat;
			int decimals;
			MetaColumn mc = qc.MetaColumn;

			if (mc.IsNumeric && !mc.IsKey)
			{
				return NumberFormatDialog.Show(qc);
			}

			else if (mc.DataType == MetaColumnType.Date)
			{
				return DateFormatDialog.Show(qc);
			}

			else if (mc.DataType == MetaColumnType.Structure)
			{
				return CriteriaStructureFormatDialog.Show(qc);
			}

			else if (mc.DataType == MetaColumnType.String)
			{
				return TextFormatDialog.Show(qc);
			}

			else
			{
				XtraMessageBox.Show("Only numeric, date/time and chemical structure fields can be formatted");
				return DialogResult.Cancel;
			}
		}
	}

		/// <summary>
		/// Summary description for NumberFormat.
		/// </summary>
		public class NumberFormatDialog : XtraForm
	{
		static NumberFormatDialog Instance = null;

		public DevExpress.XtraEditors.TextEdit DecimalPlaces;
		public DevExpress.XtraEditors.CheckEdit Scientific;
		public DevExpress.XtraEditors.CheckEdit SigDigits;
		public System.Windows.Forms.GroupBox groupBox1;
		public DevExpress.XtraEditors.LabelControl label1;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.CheckEdit Decimal;
		public LabelControl labelControl1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public NumberFormatDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		public static DialogResult Show (
			QueryColumn qc)
		{
			ColumnFormatEnum displayFormat;
			int decimals;
			MetaColumn mc = qc.MetaColumn;

			if (Instance == null) Instance = new NumberFormatDialog();
			NumberFormatDialog nfd = Instance;

			ResultsFormatter.GetOutputFormatForQueryColumn(qc, out displayFormat, out decimals);

			if (displayFormat==ColumnFormatEnum.SigDigits) nfd.SigDigits.Checked = true;
			else if (displayFormat==ColumnFormatEnum.Scientific) nfd.Scientific.Checked = true;
			else nfd.Decimal.Checked = true;

			if (qc.Decimals > 0) decimals = qc.Decimals;
			string tok = decimals.ToString();
			nfd.DecimalPlaces.Text = tok;

			SyncfusionConverter.ToRazor(Instance);

			DialogResult dr = nfd.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
			{
				if (nfd.SigDigits.Checked)
					qc.DisplayFormat = ColumnFormatEnum.SigDigits;
				else if (nfd.Scientific.Checked)
					qc.DisplayFormat = ColumnFormatEnum.Scientific;
				else qc.DisplayFormat = ColumnFormatEnum.Decimal;

				tok = nfd.DecimalPlaces.Text;
				qc.Decimals = Int32.Parse(tok); // already checked for validity
			}

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
			this.DecimalPlaces = new DevExpress.XtraEditors.TextEdit();
			this.Decimal = new DevExpress.XtraEditors.CheckEdit();
			this.Scientific = new DevExpress.XtraEditors.CheckEdit();
			this.SigDigits = new DevExpress.XtraEditors.CheckEdit();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new DevExpress.XtraEditors.LabelControl();
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.DecimalPlaces.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Decimal.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Scientific.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SigDigits.Properties)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// DecimalPlaces
			// 
			this.DecimalPlaces.EditValue = "";
			this.DecimalPlaces.Location = new System.Drawing.Point(104, 114);
			this.DecimalPlaces.Name = "DecimalPlaces";
			this.DecimalPlaces.Size = new System.Drawing.Size(58, 20);
			this.DecimalPlaces.TabIndex = 0;
			this.DecimalPlaces.TextChanged += new System.EventHandler(this.DecimalPlaces_TextChanged);
			// 
			// Decimal
			// 
			this.Decimal.EditValue = true;
			this.Decimal.Location = new System.Drawing.Point(14, 19);
			this.Decimal.Name = "Decimal";
			this.Decimal.Properties.Caption = "Decimal (Fixed number of decimal places)";
			this.Decimal.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Decimal.Properties.RadioGroupIndex = 1;
			this.Decimal.Size = new System.Drawing.Size(276, 18);
			this.Decimal.TabIndex = 1;
			this.Decimal.CheckedChanged += new System.EventHandler(this.Decimal_CheckedChanged);
			// 
			// Scientific
			// 
			this.Scientific.Location = new System.Drawing.Point(14, 67);
			this.Scientific.Name = "Scientific";
			this.Scientific.Properties.Caption = "Scientific";
			this.Scientific.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Scientific.Properties.RadioGroupIndex = 1;
			this.Scientific.Size = new System.Drawing.Size(92, 18);
			this.Scientific.TabIndex = 2;
			this.Scientific.TabStop = false;
			this.Scientific.CheckedChanged += new System.EventHandler(this.Scientific_CheckedChanged);
			// 
			// SigDigits
			// 
			this.SigDigits.Location = new System.Drawing.Point(14, 43);
			this.SigDigits.Name = "SigDigits";
			this.SigDigits.Properties.Caption = "Significant Digits (Fixed number of significant digits)";
			this.SigDigits.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.SigDigits.Properties.RadioGroupIndex = 1;
			this.SigDigits.Size = new System.Drawing.Size(284, 18);
			this.SigDigits.TabIndex = 3;
			this.SigDigits.TabStop = false;
			this.SigDigits.CheckedChanged += new System.EventHandler(this.SigDigits_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.Decimal);
			this.groupBox1.Controls.Add(this.SigDigits);
			this.groupBox1.Controls.Add(this.Scientific);
			this.groupBox1.Location = new System.Drawing.Point(6, 9);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(344, 92);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Number Format";
			// 
			// label1
			// 
			this.label1.Appearance.Options.UseTextOptions = true;
			this.label1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.label1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.label1.Location = new System.Drawing.Point(6, 110);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(106, 28);
			this.label1.TabIndex = 5;
			this.label1.Text = "Decimal Places or Significant Digits:";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(224, 154);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(60, 23);
			this.OK.TabIndex = 37;
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
			this.Cancel.Location = new System.Drawing.Point(292, 154);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(60, 23);
			this.Cancel.TabIndex = 36;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.Appearance.Options.UseTextOptions = true;
			this.labelControl1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.labelControl1.LineVisible = true;
			this.labelControl1.Location = new System.Drawing.Point(-2, 142);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(361, 10);
			this.labelControl1.TabIndex = 86;
			// 
			// NumberFormatDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(356, 183);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.DecimalPlaces);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NumberFormatDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Number Format";
			this.Activated += new System.EventHandler(this.NumberFormat_Activated);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.NumberFormat_Closing);
			((System.ComponentModel.ISupportInitialize)(this.DecimalPlaces.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Decimal.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Scientific.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SigDigits.Properties)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void DecimalPlaces_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			int decimals;
			try 
			{ 
				decimals = Int32.Parse(DecimalPlaces.Text); 
				if (decimals < 0 || decimals > 20)
					throw new Exception("");
			}
			catch (Exception ex)
			{
				XtraMessageBox.Show("Invalid number of decimals", UmlautMobius.String);
				return;
			}

			this.DialogResult = DialogResult.OK;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void NumberFormat_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//if (!this.Visible) return; // just return if we already know it's closed
			//this.DialogResult = DialogResult.Cancel;
		}

		private void NumberFormat_Activated(object sender, System.EventArgs e)
		{
			DecimalPlaces.Focus();
		}

		private void Decimal_CheckedChanged(object sender, System.EventArgs e)
		{
			DecimalPlaces.Focus();
		}

		private void SigDigits_CheckedChanged(object sender, System.EventArgs e)
		{
			DecimalPlaces.Focus();
		}

		private void Scientific_CheckedChanged(object sender, System.EventArgs e)
		{
			DecimalPlaces.Focus();
		}
	}
}
