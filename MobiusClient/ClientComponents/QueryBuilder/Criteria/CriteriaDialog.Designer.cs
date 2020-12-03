namespace Mobius.ClientComponents
{
	partial class CriteriaDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaDialog));
			this.IsNotNull = new DevExpress.XtraEditors.CheckEdit();
			this.menuGE = new System.Windows.Forms.ToolStripMenuItem();
			this.menuGT = new System.Windows.Forms.ToolStripMenuItem();
			this.menuLT = new System.Windows.Forms.ToolStripMenuItem();
			this.menuNE = new System.Windows.Forms.ToolStripMenuItem();
			this.menuLE = new System.Windows.Forms.ToolStripMenuItem();
			this.IsNull = new DevExpress.XtraEditors.CheckEdit();
			this.WithinValue = new DevExpress.XtraEditors.TextEdit();
			this.Within = new DevExpress.XtraEditors.CheckEdit();
			this.menuEQ = new System.Windows.Forms.ToolStripMenuItem();
			this.BasicOpMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.TinyBitmaps = new System.Windows.Forms.ImageList(this.components);
			this.OK = new DevExpress.XtraEditors.SimpleButton();
			this.Cancel = new DevExpress.XtraEditors.SimpleButton();
			this.Substring = new DevExpress.XtraEditors.TextEdit();
			this.Limit1 = new DevExpress.XtraEditors.TextEdit();
			this.All = new DevExpress.XtraEditors.CheckEdit();
			this.Limit2 = new DevExpress.XtraEditors.TextEdit();
			this.Like = new DevExpress.XtraEditors.CheckEdit();
			this.EditList = new DevExpress.XtraEditors.SimpleButton();
			this.ImportList = new DevExpress.XtraEditors.SimpleButton();
			this.ValueList = new DevExpress.XtraEditors.TextEdit();
			this.InList = new DevExpress.XtraEditors.CheckEdit();
			this.None = new DevExpress.XtraEditors.CheckEdit();
			this.BasicOp = new DevExpress.XtraEditors.CheckEdit();
			this.Between = new DevExpress.XtraEditors.CheckEdit();
			this.BetweenAnd = new DevExpress.XtraEditors.LabelControl();
			this.Prompt = new DevExpress.XtraEditors.LabelControl();
			this.BasicOpBut = new DevExpress.XtraEditors.SimpleButton();
			this.Value = new DevExpress.XtraEditors.ComboBoxEdit();
			this.WithinUnits = new DevExpress.XtraEditors.ComboBoxEdit();
			this.LabelControl = new DevExpress.XtraEditors.LabelControl();
			((System.ComponentModel.ISupportInitialize)(this.IsNotNull.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IsNull.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WithinValue.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Within.Properties)).BeginInit();
			this.BasicOpMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Substring.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Limit1.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.All.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Limit2.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Like.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ValueList.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InList.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BasicOp.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Between.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Value.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WithinUnits.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// IsNotNull
			// 
			this.IsNotNull.Cursor = System.Windows.Forms.Cursors.Default;
			this.IsNotNull.Location = new System.Drawing.Point(16, 194);
			this.IsNotNull.Name = "IsNotNull";
			this.IsNotNull.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.IsNotNull.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IsNotNull.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IsNotNull.Properties.Appearance.Options.UseBackColor = true;
			this.IsNotNull.Properties.Appearance.Options.UseFont = true;
			this.IsNotNull.Properties.Appearance.Options.UseForeColor = true;
			this.IsNotNull.Properties.Caption = " Data Exists for this field";
			this.IsNotNull.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.IsNotNull.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.IsNotNull.Properties.RadioGroupIndex = 1;
			this.IsNotNull.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.IsNotNull.Size = new System.Drawing.Size(172, 20);
			this.IsNotNull.TabIndex = 72;
			this.IsNotNull.TabStop = false;
			this.IsNotNull.CheckedChanged += new System.EventHandler(this.IsNotNull_CheckedChanged);
			// 
			// menuGE
			// 
			this.menuGE.Image = ((System.Drawing.Image)(resources.GetObject("menuGE.Image")));
			this.menuGE.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuGE.Name = "menuGE";
			this.menuGE.Size = new System.Drawing.Size(185, 22);
			this.menuGE.Text = "Greater than or equal";
			this.menuGE.Click += new System.EventHandler(this.menuGE_Click);
			// 
			// menuGT
			// 
			this.menuGT.Image = ((System.Drawing.Image)(resources.GetObject("menuGT.Image")));
			this.menuGT.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuGT.Name = "menuGT";
			this.menuGT.Size = new System.Drawing.Size(185, 22);
			this.menuGT.Text = "Greater than";
			this.menuGT.Click += new System.EventHandler(this.menuGT_Click);
			// 
			// menuLT
			// 
			this.menuLT.Image = ((System.Drawing.Image)(resources.GetObject("menuLT.Image")));
			this.menuLT.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuLT.Name = "menuLT";
			this.menuLT.Size = new System.Drawing.Size(185, 22);
			this.menuLT.Text = "Less than";
			this.menuLT.Click += new System.EventHandler(this.menuLT_Click);
			// 
			// menuNE
			// 
			this.menuNE.Image = ((System.Drawing.Image)(resources.GetObject("menuNE.Image")));
			this.menuNE.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuNE.Name = "menuNE";
			this.menuNE.Size = new System.Drawing.Size(185, 22);
			this.menuNE.Text = "Not equal";
			this.menuNE.Click += new System.EventHandler(this.menuNE_Click);
			// 
			// menuLE
			// 
			this.menuLE.Image = ((System.Drawing.Image)(resources.GetObject("menuLE.Image")));
			this.menuLE.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuLE.Name = "menuLE";
			this.menuLE.Size = new System.Drawing.Size(185, 22);
			this.menuLE.Text = "Less than or equal";
			this.menuLE.Click += new System.EventHandler(this.menuLE_Click);
			// 
			// IsNull
			// 
			this.IsNull.Cursor = System.Windows.Forms.Cursors.Default;
			this.IsNull.Location = new System.Drawing.Point(16, 219);
			this.IsNull.Name = "IsNull";
			this.IsNull.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.IsNull.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IsNull.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IsNull.Properties.Appearance.Options.UseBackColor = true;
			this.IsNull.Properties.Appearance.Options.UseFont = true;
			this.IsNull.Properties.Appearance.Options.UseForeColor = true;
			this.IsNull.Properties.Caption = " Data Doesn\'t Exist for this field";
			this.IsNull.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.IsNull.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.IsNull.Properties.RadioGroupIndex = 1;
			this.IsNull.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.IsNull.Size = new System.Drawing.Size(204, 20);
			this.IsNull.TabIndex = 84;
			this.IsNull.TabStop = false;
			this.IsNull.CheckedChanged += new System.EventHandler(this.IsNull_CheckedChanged);
			// 
			// WithinValue
			// 
			this.WithinValue.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.WithinValue.Location = new System.Drawing.Point(116, 169);
			this.WithinValue.Name = "WithinValue";
			this.WithinValue.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.WithinValue.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WithinValue.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.WithinValue.Properties.Appearance.Options.UseBackColor = true;
			this.WithinValue.Properties.Appearance.Options.UseFont = true;
			this.WithinValue.Properties.Appearance.Options.UseForeColor = true;
			this.WithinValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.WithinValue.Size = new System.Drawing.Size(52, 20);
			this.WithinValue.TabIndex = 81;
			// 
			// Within
			// 
			this.Within.Cursor = System.Windows.Forms.Cursors.Default;
			this.Within.Location = new System.Drawing.Point(16, 169);
			this.Within.Name = "Within";
			this.Within.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Within.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Within.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Within.Properties.Appearance.Options.UseBackColor = true;
			this.Within.Properties.Appearance.Options.UseFont = true;
			this.Within.Properties.Appearance.Options.UseForeColor = true;
			this.Within.Properties.Caption = " Within the Last:";
			this.Within.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.Within.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Within.Properties.RadioGroupIndex = 1;
			this.Within.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Within.Size = new System.Drawing.Size(261, 20);
			this.Within.TabIndex = 80;
			this.Within.TabStop = false;
			this.Within.CheckedChanged += new System.EventHandler(this.Within_CheckedChanged);
			// 
			// menuEQ
			// 
			this.menuEQ.Image = ((System.Drawing.Image)(resources.GetObject("menuEQ.Image")));
			this.menuEQ.ImageTransparentColor = System.Drawing.Color.Cyan;
			this.menuEQ.Name = "menuEQ";
			this.menuEQ.Size = new System.Drawing.Size(185, 22);
			this.menuEQ.Text = "Equals";
			this.menuEQ.Click += new System.EventHandler(this.menuEQ_Click);
			// 
			// BasicOpMenu
			// 
			this.BasicOpMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuEQ,
            this.menuNE,
            this.menuLT,
            this.menuLE,
            this.menuGT,
            this.menuGE});
			this.BasicOpMenu.Name = "BasicOpMenu";
			this.BasicOpMenu.Size = new System.Drawing.Size(186, 136);
			// 
			// TinyBitmaps
			// 
			this.TinyBitmaps.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TinyBitmaps.ImageStream")));
			this.TinyBitmaps.TransparentColor = System.Drawing.Color.Cyan;
			this.TinyBitmaps.Images.SetKeyName(0, "");
			this.TinyBitmaps.Images.SetKeyName(1, "");
			this.TinyBitmaps.Images.SetKeyName(2, "");
			this.TinyBitmaps.Images.SetKeyName(3, "");
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OK.Appearance.Options.UseFont = true;
			this.OK.Appearance.Options.UseForeColor = true;
			this.OK.Cursor = System.Windows.Forms.Cursors.Default;
			this.OK.Location = new System.Drawing.Point(327, 300);
			this.OK.Name = "OK";
			this.OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.OK.Size = new System.Drawing.Size(68, 22);
			this.OK.TabIndex = 71;
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
			this.Cancel.Location = new System.Drawing.Point(407, 300);
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Cancel.Size = new System.Drawing.Size(68, 22);
			this.Cancel.TabIndex = 70;
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// Substring
			// 
			this.Substring.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Substring.Location = new System.Drawing.Point(152, 143);
			this.Substring.Name = "Substring";
			this.Substring.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Substring.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Substring.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Substring.Properties.Appearance.Options.UseBackColor = true;
			this.Substring.Properties.Appearance.Options.UseFont = true;
			this.Substring.Properties.Appearance.Options.UseForeColor = true;
			this.Substring.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Substring.Size = new System.Drawing.Size(282, 20);
			this.Substring.TabIndex = 65;
			// 
			// Limit1
			// 
			this.Limit1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Limit1.Location = new System.Drawing.Point(88, 117);
			this.Limit1.Name = "Limit1";
			this.Limit1.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Limit1.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Limit1.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Limit1.Properties.Appearance.Options.UseBackColor = true;
			this.Limit1.Properties.Appearance.Options.UseFont = true;
			this.Limit1.Properties.Appearance.Options.UseForeColor = true;
			this.Limit1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Limit1.Size = new System.Drawing.Size(150, 20);
			this.Limit1.TabIndex = 62;
			// 
			// All
			// 
			this.All.Cursor = System.Windows.Forms.Cursors.Default;
			this.All.Location = new System.Drawing.Point(16, 245);
			this.All.Name = "All";
			this.All.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.All.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.All.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.All.Properties.Appearance.Options.UseBackColor = true;
			this.All.Properties.Appearance.Options.UseFont = true;
			this.All.Properties.Appearance.Options.UseForeColor = true;
			this.All.Properties.Caption = " All data rows for this assay/table";
			this.All.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.All.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.All.Properties.RadioGroupIndex = 1;
			this.All.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.All.Size = new System.Drawing.Size(299, 20);
			this.All.TabIndex = 66;
			this.All.TabStop = false;
			this.All.CheckedChanged += new System.EventHandler(this.All_CheckedChanged);
			// 
			// Limit2
			// 
			this.Limit2.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Limit2.Location = new System.Drawing.Point(282, 117);
			this.Limit2.Name = "Limit2";
			this.Limit2.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.Limit2.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Limit2.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Limit2.Properties.Appearance.Options.UseBackColor = true;
			this.Limit2.Properties.Appearance.Options.UseFont = true;
			this.Limit2.Properties.Appearance.Options.UseForeColor = true;
			this.Limit2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Limit2.Size = new System.Drawing.Size(152, 20);
			this.Limit2.TabIndex = 63;
			// 
			// Like
			// 
			this.Like.Cursor = System.Windows.Forms.Cursors.Default;
			this.Like.Location = new System.Drawing.Point(16, 143);
			this.Like.Name = "Like";
			this.Like.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Like.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Like.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Like.Properties.Appearance.Options.UseBackColor = true;
			this.Like.Properties.Appearance.Options.UseFont = true;
			this.Like.Properties.Appearance.Options.UseForeColor = true;
			this.Like.Properties.Caption = " Contains the Substring:";
			this.Like.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.Like.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Like.Properties.RadioGroupIndex = 1;
			this.Like.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Like.Size = new System.Drawing.Size(399, 20);
			this.Like.TabIndex = 64;
			this.Like.TabStop = false;
			this.Like.CheckedChanged += new System.EventHandler(this.Like_CheckedChanged);
			// 
			// EditList
			// 
			this.EditList.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EditList.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.EditList.Appearance.Options.UseFont = true;
			this.EditList.Appearance.Options.UseForeColor = true;
			this.EditList.Cursor = System.Windows.Forms.Cursors.Default;
			this.EditList.Location = new System.Drawing.Point(388, 90);
			this.EditList.Name = "EditList";
			this.EditList.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.EditList.Size = new System.Drawing.Size(46, 22);
			this.EditList.TabIndex = 76;
			this.EditList.Text = "Edit...";
			this.EditList.Click += new System.EventHandler(this.EditList_Click);
			// 
			// ImportList
			// 
			this.ImportList.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ImportList.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportList.Appearance.Options.UseFont = true;
			this.ImportList.Appearance.Options.UseForeColor = true;
			this.ImportList.Cursor = System.Windows.Forms.Cursors.Default;
			this.ImportList.Location = new System.Drawing.Point(328, 90);
			this.ImportList.Name = "ImportList";
			this.ImportList.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ImportList.Size = new System.Drawing.Size(54, 22);
			this.ImportList.TabIndex = 75;
			this.ImportList.Text = "Import...";
			this.ImportList.Click += new System.EventHandler(this.ImportList_Click);
			// 
			// ValueList
			// 
			this.ValueList.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.ValueList.Location = new System.Drawing.Point(88, 92);
			this.ValueList.Name = "ValueList";
			this.ValueList.Properties.Appearance.BackColor = System.Drawing.SystemColors.Window;
			this.ValueList.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ValueList.Properties.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ValueList.Properties.Appearance.Options.UseBackColor = true;
			this.ValueList.Properties.Appearance.Options.UseFont = true;
			this.ValueList.Properties.Appearance.Options.UseForeColor = true;
			this.ValueList.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ValueList.Size = new System.Drawing.Size(232, 20);
			this.ValueList.TabIndex = 74;
			// 
			// InList
			// 
			this.InList.Cursor = System.Windows.Forms.Cursors.Default;
			this.InList.Location = new System.Drawing.Point(16, 90);
			this.InList.Name = "InList";
			this.InList.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.InList.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InList.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.InList.Properties.Appearance.Options.UseBackColor = true;
			this.InList.Properties.Appearance.Options.UseFont = true;
			this.InList.Properties.Appearance.Options.UseForeColor = true;
			this.InList.Properties.Caption = " In List";
			this.InList.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.InList.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.InList.Properties.RadioGroupIndex = 1;
			this.InList.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.InList.Size = new System.Drawing.Size(414, 20);
			this.InList.TabIndex = 73;
			this.InList.TabStop = false;
			this.InList.CheckedChanged += new System.EventHandler(this.InList_CheckedChanged);
			// 
			// None
			// 
			this.None.Cursor = System.Windows.Forms.Cursors.Default;
			this.None.Location = new System.Drawing.Point(16, 270);
			this.None.Name = "None";
			this.None.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.None.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.None.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.None.Properties.Appearance.Options.UseBackColor = true;
			this.None.Properties.Appearance.Options.UseFont = true;
			this.None.Properties.Appearance.Options.UseForeColor = true;
			this.None.Properties.Caption = " No criteria";
			this.None.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.None.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.None.Properties.RadioGroupIndex = 1;
			this.None.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.None.Size = new System.Drawing.Size(182, 20);
			this.None.TabIndex = 67;
			this.None.TabStop = false;
			this.None.CheckedChanged += new System.EventHandler(this.None_CheckedChanged);
			// 
			// BasicOp
			// 
			this.BasicOp.Cursor = System.Windows.Forms.Cursors.Default;
			this.BasicOp.EditValue = true;
			this.BasicOp.Location = new System.Drawing.Point(16, 65);
			this.BasicOp.Name = "BasicOp";
			this.BasicOp.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.BasicOp.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BasicOp.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.BasicOp.Properties.Appearance.Options.UseBackColor = true;
			this.BasicOp.Properties.Appearance.Options.UseFont = true;
			this.BasicOp.Properties.Appearance.Options.UseForeColor = true;
			this.BasicOp.Properties.Caption = "";
			this.BasicOp.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.BasicOp.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.BasicOp.Properties.RadioGroupIndex = 1;
			this.BasicOp.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.BasicOp.Size = new System.Drawing.Size(16, 20);
			this.BasicOp.TabIndex = 60;
			this.BasicOp.CheckedChanged += new System.EventHandler(this.BasicOp_CheckedChanged);
			// 
			// Between
			// 
			this.Between.Cursor = System.Windows.Forms.Cursors.Default;
			this.Between.Location = new System.Drawing.Point(16, 117);
			this.Between.Name = "Between";
			this.Between.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Between.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Between.Properties.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Between.Properties.Appearance.Options.UseBackColor = true;
			this.Between.Properties.Appearance.Options.UseFont = true;
			this.Between.Properties.Appearance.Options.UseForeColor = true;
			this.Between.Properties.Caption = " Between";
			this.Between.Properties.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.Radio;
			this.Between.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
			this.Between.Properties.RadioGroupIndex = 1;
			this.Between.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Between.Size = new System.Drawing.Size(216, 20);
			this.Between.TabIndex = 61;
			this.Between.TabStop = false;
			this.Between.CheckedChanged += new System.EventHandler(this.Between_CheckedChanged);
			// 
			// BetweenAnd
			// 
			this.BetweenAnd.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.BetweenAnd.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BetweenAnd.Appearance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.BetweenAnd.Appearance.Options.UseBackColor = true;
			this.BetweenAnd.Appearance.Options.UseFont = true;
			this.BetweenAnd.Appearance.Options.UseForeColor = true;
			this.BetweenAnd.Cursor = System.Windows.Forms.Cursors.Default;
			this.BetweenAnd.Location = new System.Drawing.Point(248, 119);
			this.BetweenAnd.Name = "BetweenAnd";
			this.BetweenAnd.Size = new System.Drawing.Size(18, 13);
			this.BetweenAnd.TabIndex = 69;
			this.BetweenAnd.Text = "and";
			// 
			// Prompt
			// 
			this.Prompt.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.Prompt.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Prompt.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Prompt.Appearance.Options.UseBackColor = true;
			this.Prompt.Appearance.Options.UseFont = true;
			this.Prompt.Appearance.Options.UseForeColor = true;
			this.Prompt.Appearance.Options.UseTextOptions = true;
			this.Prompt.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
			this.Prompt.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.Prompt.Cursor = System.Windows.Forms.Cursors.Default;
			this.Prompt.Location = new System.Drawing.Point(6, 7);
			this.Prompt.Name = "Prompt";
			this.Prompt.Size = new System.Drawing.Size(420, 52);
			this.Prompt.TabIndex = 68;
			this.Prompt.Tag = "Prompt";
			this.Prompt.Text = resources.GetString("Prompt.Text");
			// 
			// BasicOpBut
			// 
			this.BasicOpBut.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.BasicOpBut.Appearance.BackColor2 = System.Drawing.Color.Transparent;
			this.BasicOpBut.Appearance.Options.UseBackColor = true;
			this.BasicOpBut.Appearance.Options.UseTextOptions = true;
			this.BasicOpBut.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			this.BasicOpBut.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
			this.BasicOpBut.ImageOptions.ImageIndex = 0;
			this.BasicOpBut.ImageOptions.ImageList = this.TinyBitmaps;
			this.BasicOpBut.ImageOptions.ImageUri.Uri = "OpEqDropDownIconMx";
			this.BasicOpBut.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleRight;
			this.BasicOpBut.Location = new System.Drawing.Point(32, 64);
			this.BasicOpBut.Margin = new System.Windows.Forms.Padding(0);
			this.BasicOpBut.Name = "BasicOpBut";
			this.BasicOpBut.Size = new System.Drawing.Size(56, 22);
			this.BasicOpBut.TabIndex = 79;
			this.BasicOpBut.Text = "Equals";
			this.BasicOpBut.Click += new System.EventHandler(this.BasicOpBut_Click);
			// 
			// Value
			// 
			this.Value.Location = new System.Drawing.Point(88, 65);
			this.Value.Name = "Value";
			this.Value.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.Value.Size = new System.Drawing.Size(292, 20);
			this.Value.TabIndex = 85;
			this.Value.Click += new System.EventHandler(this.Value_Click);
			this.Value.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Value_KeyDown);
			// 
			// WithinUnits
			// 
			this.WithinUnits.Location = new System.Drawing.Point(174, 168);
			this.WithinUnits.Name = "WithinUnits";
			this.WithinUnits.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.WithinUnits.Properties.Items.AddRange(new object[] {
            "Day(s)",
            "Week(s)",
            "Month(s)",
            "Year(s)"});
			this.WithinUnits.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
			this.WithinUnits.Size = new System.Drawing.Size(90, 20);
			this.WithinUnits.TabIndex = 86;
			// 
			// LabelControl
			// 
			this.LabelControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelControl.Appearance.BackColor = System.Drawing.Color.Transparent;
			this.LabelControl.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelControl.Appearance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.LabelControl.Appearance.Options.UseBackColor = true;
			this.LabelControl.Appearance.Options.UseFont = true;
			this.LabelControl.Appearance.Options.UseForeColor = true;
			this.LabelControl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
			this.LabelControl.Cursor = System.Windows.Forms.Cursors.Default;
			this.LabelControl.LineVisible = true;
			this.LabelControl.Location = new System.Drawing.Point(-2, 287);
			this.LabelControl.Name = "LabelControl";
			this.LabelControl.Size = new System.Drawing.Size(485, 10);
			this.LabelControl.TabIndex = 87;
			this.LabelControl.Click += new System.EventHandler(this.LabelControl_Click);
			// 
			// CriteriaDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(481, 327);
			this.Controls.Add(this.BasicOp);
			this.Controls.Add(this.WithinUnits);
			this.Controls.Add(this.Value);
			this.Controls.Add(this.Prompt);
			this.Controls.Add(this.IsNotNull);
			this.Controls.Add(this.IsNull);
			this.Controls.Add(this.WithinValue);
			this.Controls.Add(this.Within);
			this.Controls.Add(this.Substring);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Limit1);
			this.Controls.Add(this.All);
			this.Controls.Add(this.Limit2);
			this.Controls.Add(this.Like);
			this.Controls.Add(this.EditList);
			this.Controls.Add(this.ImportList);
			this.Controls.Add(this.ValueList);
			this.Controls.Add(this.InList);
			this.Controls.Add(this.None);
			this.Controls.Add(this.Between);
			this.Controls.Add(this.BetweenAnd);
			this.Controls.Add(this.BasicOpBut);
			this.Controls.Add(this.LabelControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.IconOptions.ShowIcon = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CriteriaDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Criteria";
			this.Activated += new System.EventHandler(this.Criteria_Activated);
			((System.ComponentModel.ISupportInitialize)(this.IsNotNull.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IsNull.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WithinValue.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Within.Properties)).EndInit();
			this.BasicOpMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Substring.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Limit1.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.All.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Limit2.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Like.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ValueList.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InList.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.None.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BasicOp.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Between.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Value.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WithinUnits.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public DevExpress.XtraEditors.CheckEdit IsNotNull;
		public System.Windows.Forms.ToolStripMenuItem menuGE;
		public System.Windows.Forms.ToolStripMenuItem menuGT;
		public System.Windows.Forms.ToolStripMenuItem menuLT;
		public System.Windows.Forms.ToolStripMenuItem menuNE;
		public System.Windows.Forms.ToolStripMenuItem menuLE;
		public DevExpress.XtraEditors.CheckEdit IsNull;
		public DevExpress.XtraEditors.TextEdit WithinValue;
		public DevExpress.XtraEditors.CheckEdit Within;
		public System.Windows.Forms.ToolStripMenuItem menuEQ;
		public System.Windows.Forms.ContextMenuStrip BasicOpMenu;
		private DevExpress.XtraEditors.SimpleButton BasicOpBut;
		public System.Windows.Forms.ImageList TinyBitmaps;
		public DevExpress.XtraEditors.SimpleButton OK;
		public DevExpress.XtraEditors.SimpleButton Cancel;
		public DevExpress.XtraEditors.TextEdit Substring;
		public DevExpress.XtraEditors.TextEdit Limit1;
		public DevExpress.XtraEditors.CheckEdit All;
		public DevExpress.XtraEditors.TextEdit Limit2;
		public DevExpress.XtraEditors.CheckEdit Like;
		public DevExpress.XtraEditors.SimpleButton EditList;
		public DevExpress.XtraEditors.SimpleButton ImportList;
		public DevExpress.XtraEditors.TextEdit ValueList;
		public DevExpress.XtraEditors.CheckEdit InList;
		public DevExpress.XtraEditors.CheckEdit None;
		public DevExpress.XtraEditors.CheckEdit BasicOp;
		public DevExpress.XtraEditors.CheckEdit Between;
		public DevExpress.XtraEditors.LabelControl BetweenAnd;
		public DevExpress.XtraEditors.LabelControl Prompt;
		public DevExpress.XtraEditors.ComboBoxEdit Value;
		public DevExpress.XtraEditors.ComboBoxEdit WithinUnits;
		public DevExpress.XtraEditors.LabelControl LabelControl;
	}
}