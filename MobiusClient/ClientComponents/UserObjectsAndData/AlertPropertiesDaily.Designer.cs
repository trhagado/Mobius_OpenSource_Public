namespace Mobius.ClientComponents
{
    partial class AlertPropertiesDaily
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
            this.rbtnEvery = new System.Windows.Forms.RadioButton();
            this.rbtnEveryWeekday = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxEveryDay = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxEveryDay.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // rbtnEvery
            // 
            this.rbtnEvery.AutoSize = true;
            this.rbtnEvery.Checked = true;
            this.rbtnEvery.Location = new System.Drawing.Point(13, 13);
            this.rbtnEvery.Name = "rbtnEvery";
            this.rbtnEvery.Size = new System.Drawing.Size(65, 21);
            this.rbtnEvery.TabIndex = 0;
            this.rbtnEvery.TabStop = true;
            this.rbtnEvery.Text = "Every";
            this.rbtnEvery.UseVisualStyleBackColor = true;
            // 
            // rbtnEveryWeekday
            // 
            this.rbtnEveryWeekday.AutoSize = true;
            this.rbtnEveryWeekday.Location = new System.Drawing.Point(13, 41);
            this.rbtnEveryWeekday.Name = "rbtnEveryWeekday";
            this.rbtnEveryWeekday.Size = new System.Drawing.Size(128, 21);
            this.rbtnEveryWeekday.TabIndex = 1;
            this.rbtnEveryWeekday.Text = "Every Weekday";
            this.rbtnEveryWeekday.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(141, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "day(s)";
            // 
            // textBoxEveryDay
            // 
            this.textBoxEveryDay.EditValue = "1";
            this.textBoxEveryDay.Location = new System.Drawing.Point(84, 13);
            this.textBoxEveryDay.Name = "textBoxEveryDay";
            this.textBoxEveryDay.Properties.Mask.EditMask = "d";
            this.textBoxEveryDay.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.textBoxEveryDay.Properties.MaxLength = 2;
            this.textBoxEveryDay.Size = new System.Drawing.Size(51, 22);
            this.textBoxEveryDay.TabIndex = 4;
            this.textBoxEveryDay.Enter += new System.EventHandler(this.textBoxEveryDay_Enter);
            // 
            // AlertPropertiesDaily
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxEveryDay);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbtnEveryWeekday);
            this.Controls.Add(this.rbtnEvery);
            this.Name = "AlertPropertiesDaily";
            this.Size = new System.Drawing.Size(500, 100);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxEveryDay.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbtnEvery;
        private System.Windows.Forms.RadioButton rbtnEveryWeekday;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit textBoxEveryDay;
    }
}
