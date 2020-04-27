// Developer Express Code Central Example:
// How to use the ExpressionEditorForm as a stand-alone control
// 
// This example demonstrates how to embed the ExpressionEditorForm into the
// PanelControl and provide a functionality to apply expressions to unbound
// GridColumns.
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E1705

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Design;
using System;
using System.ComponentModel;
using DevExpress.XtraGrid.Columns;
using System.Windows.Forms;
using System.ComponentModel.Design;
using DevExpress.Data.Filtering.Helpers;

namespace DXSample {
    public class UnboundExpressionPanel : PanelControl {
        public UnboundExpressionPanel() : base() {
            BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            StartEdit(new GridColumn()); 
        }

        object[] arguments;
        protected object[] Arguments { get { return arguments; } }

        MyUnboundColumnExpressionEditorForm form = null;
        protected MyUnboundColumnExpressionEditorForm Form { get { return form; } }

        private GridColumn fUnboundColumn;
        public GridColumn UnboundColumn {
            get { return fUnboundColumn; }
            set {
                if (fUnboundColumn == value) return;
                StartEdit(value);
            }
        }

        protected MyUnboundColumnExpressionEditorForm CreateForm(params object[] arguments)
        {
            return new MyUnboundColumnExpressionEditorForm(arguments[0], null);
        }

        protected void ApplyExpression(string expression) {
            if (Arguments == null) return;
            ((GridColumn)Arguments[0]).UnboundExpression = expression;
        }

        public void StartEdit(params object[] arguments) {
            if (arguments.Length < 1) return;
            GridColumn unboundColumn = arguments[0] as GridColumn;
            if (unboundColumn == null) return;
            fUnboundColumn = unboundColumn;
            DestroyForm();
            this.arguments = arguments;
            this.form = CreateForm(arguments);
            if (form == null) return;
            form.Dock = DockStyle.Fill;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Closing += new CancelEventHandler(form_Closing);
            form.buttonCancel.Click += new EventHandler(buttonCancel_Click);
            form.buttonOK.Text = "Apply";
            Controls.Add(form);
            form.Visible = true;
        }

        void buttonCancel_Click(object sender, EventArgs e)
        {
            if (form != null) form.Close();
        }

        void form_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (this.arguments == null || this.form == null) return;
            if (form.DialogResult == DialogResult.OK)
            {
                ApplyExpression(form.Expression);
            }
            else
            {
                form.ResetMemoText();
            }
        }

        public void DestroyForm()
        {
            if (form != null) form.Dispose();
            form = null;
        }
    }

    public class MyUnboundColumnExpressionEditorForm : UnboundColumnExpressionEditorForm
    {
        public MyUnboundColumnExpressionEditorForm(object contextInstance, 
            IDesignerHost designerHost) 
            : base(contextInstance, designerHost) { }

        private string GetExpressionMemoEditText()
        {
            GridColumn column = ContextInstance as GridColumn;
            return null == column ? string.Empty : column.UnboundExpression;
        }

        public void ResetMemoText()
        {
            ExpressionMemoEdit.Text = GetExpressionMemoEditText();
        }
    }
}