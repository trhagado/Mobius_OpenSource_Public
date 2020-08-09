using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using DevExpress.XtraEditors;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mobius.SpotfireClient
{
	public partial class ColorBySelectorControl : DevExpress.XtraEditors.XtraUserControl
	{

		public SpotfireViewProps SVP; // associated Spotfire View Properties
		public VisualMsx Visual; // visual we are operating with
		public ColorAxisMsx ColorAxis;

		public ColorDimension ColorBy; // ColorsBy object (old)
		//public Query Query { get { return View.BaseQuery; } }  // associated query

		public event EventHandler EditValueChanged; // event to fire when edit value changes
		public Control ControlChanged = null; // the control that was changed

		bool InSetup = false;


/// <summary>
/// Constructor
/// </summary>

		public ColorBySelectorControl()
		{
			InitializeComponent();
			WinFormsUtil.LogControlChildren(this);

			//ColorRulesControl.RulesGrid.V.Columns["RuleName"].Visible = false; // hide rule column
			//ColorRulesControl.RulesGrid.V.Columns["BackColor1"].Caption = "Color";
		}

		/// <summary>
		/// Setup
		/// </summary>
		/// <param name="view"></param>
		/// <param name="colorBy"></param>

		public void Setup(
			ColorAxisMsx colorAxis,
			VisualMsx visual,
			SpotfireViewProps spotfireViewProps,
			EventHandler editValueChangedEventHandler = null)
		{
			InSetup = true;

			SVP = spotfireViewProps;
			Visual = visual;
			ColorAxis = colorAxis;

			EditValueChanged = editValueChangedEventHandler;

			ColumnsSelector.Setup(ColorAxis, Visual, SVP, EditValueChanged);

			//ColorBy = colorBy;
			//FixedColor.Color = colorBy.FixedColor;
			//BorderColor.Color = colorBy.BorderColor;

			//SetupColorSchemeGrid(colorBy.QueryColumn, View);

			if (Lex.IsDefined(ColorAxis.Expression))
				ColorByColumn.Checked = true;
			else ColorByFixedColor.Checked = true;

			//ColumnValueContainsColor.Checked = colorBy.FieldValueContainsColor;

			InSetup = false;
			return;
		}

		private void ColorByFixedColor_EditValueChanged(object sender, EventArgs e)
		{ // fixed color option button changed value
			if (InSetup || !ColorByFixedColor.Checked) return;

			InSetup = true;
			ColorBy.QueryColumn = null; // clear selected col info
			//ColorColumnSelector.Setup(View.BaseQuery, ColorBy.QueryColumn);
			//SetupColorSchemeGrid(ColorBy.QueryColumn, View);
			InSetup = false;

			//FireEditValueChanged(ColorByFixedColor);
		}

		private void FixedColor_EditValueChanged(object sender, EventArgs e)
		{ // new fixed color
			if (InSetup) return;

			ColorByFixedColor.Checked = true;
			ColorBy.FixedColor = FixedColor.Color;

			//FireEditValueChanged(FixedColor);
		}

		private void MarkerBorderColor_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			ColorBy.BorderColor = BorderColor.Color;
			//FireEditValueChanged(BorderColor);
			return;
		}

		private void ColorByColumn_EditValueChanged(object sender, EventArgs e)
		{ // Color by column option button changed value
			return;
		}

		private void ColorColumnSelector_EditValueChanged(object sender, EventArgs e)
		{
#if false
			if (InSetup) return;

			InSetup = true;

			QueryColumn qc = ColorColumnSelector.QueryColumn;
			ColorBy.QueryColumn = qc;
			if (qc != null)
			{	// setup for this column
				ColorByColumn.Checked = true;
				if (qc.CondFormat == null || qc.CondFormat.Rules.Count == 0)
				{ // define default rules
					qc.CondFormat = new CondFormat();
					ResultsField rfld = View.ResultsFormat.GetResultsField(qc);
					if (rfld != null)
					{
						CondFormatStyle cfStyle = qc.CondFormat.Rules.ColoringStyle;
						ColorRulesControl.SetupControl(rfld.QueryColumn.MetaColumn.DataType, cfStyle, rfld);
						//ColorRulesControl.SetupForResultsField(rfld, qc.CondFormat.Rules.ColoringStyle);
					}
					InitializeRulesBasedOnDataValues();
					qc.CondFormat.Rules = ColorRulesControl.GetRules(); // get rules back
				}

				ResetBackgroundColors(ColorBy.QueryColumn);
				SetupColorSchemeGrid(ColorBy.QueryColumn, View);
			}

			else ColorByFixedColor.Checked = true; // no col selected, goto fixed mode

			InSetup = false;

			FireEditValueChanged(ColorColumnSelector);

#endif

			return;
		}

#if false
		/// <summary>
		/// Setup the conditional formatting control for marker colors 
		/// </summary>

		public void SetupColorSchemeGrid(
			QueryColumn qc,
			ViewManager view)
		{
			return; // todo

			if (qc == null || qc.CondFormat == null)
			{
				CondFormatRules rules = new CondFormatRules();
				SetupControl(MetaColumnType.Unknown, rules);
			}

			else // column assigned to color rules
			{
				ResultsField rfld = view.ResultsFormat.GetResultsField(qc);
				if (rfld != null)
					SetupControl(rfld, qc.CondFormat.Rules); // setup existing rules
			}

			return;
		}
#endif


/// <summary>
/// Initialize coloring rules for newly selected QueryColumn
/// </summary>

#if false
		internal CondFormatRules InitializeRulesBasedOnDataValues()
		{
			CondFormatRule r;
			Color[] colors = GetColors();

			CondFormatRules rules = new CondFormatRules();
			if (ResultsField == null) return rules;

			QueryColumn qc = ResultsField.QueryColumn;
			ColumnStatistics stats = ResultsField.GetStats();

			if (qc.MetaColumn.DataType == MetaColumnType.Structure)
			{ // setup substructure search rules if structures
				for (int i1 = 0; i1 < stats.DistinctValueList.Count; i1++)
				{
					MobiusDataType mdt = stats.DistinctValueList[i1];
					r = new CondFormatRule();
					r.Op = "SSS";
					r.OpCode = CondFormatOpCode.SSS;
					ChemicalStructureMx cs = mdt as ChemicalStructureMx;
					if (cs != null) r.Value = cs.ChimeString;
					r.BackColor1 = colors[i1 % colors.Length];
					rules.Add(r);
				}
			}

			else // setup equality rules for other types
			{
				for (int i1 = 0; i1 < stats.DistinctValueList.Count; i1++)
				{
					MobiusDataType mdt = stats.DistinctValueList[i1];
					r = new CondFormatRule();
					r.Op = "Equal to";
					r.OpCode = CondFormatOpCode.Eq;
					r.Value = mdt.FormattedText;
					r.BackColor1 = colors[i1 % colors.Length];
					rules.Add(r);
					//				if (i1 + 1 >= 25) break; // limit number of items
				}
			}

			if (stats.NullsExist)
			{
				r = new CondFormatRule();
				r.Name = "Missing Data";
				r.Op = "Missing";
				r.OpCode = CondFormatOpCode.NotExists;
				r.BackColor1 = CondFormatMatcher.DefaultMissingValueColor;
				rules.Add(r);
			}

			SetRules(ResultsField, rules); // put into the grid
			return rules;
		}

		private void ColorRulesControl_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (ColorBy.QueryColumn != null && ColorBy.QueryColumn.CondFormat != null)
			{
				ColorBy.QueryColumn.CondFormat.Rules = ColorRulesControl.GetRules();
				ResetBackgroundColors(ColorBy.QueryColumn);
			}

			FireEditValueChanged(ColorRulesControl);
		}

		private void ColumnValueContainsColor_EditValueChanged(object sender, EventArgs e)
		{
			if (InSetup) return;

			if (ColorBy.QueryColumn != null && ColorBy.QueryColumn.CondFormat != null)
				ResetBackgroundColors(ColorBy.QueryColumn);

			FireEditValueChanged(ColumnValueContainsColor);
		}

		private void ResetBackgroundColors(QueryColumn qc)
		{
			if (Query == null || Query.QueryManager == null) return;

			QueryManager qm = Query.QueryManager as QueryManager;
			if (qm == null || qm.DataTableManager == null) return;

			qm.DataTableManager.ResetFormattedValues(false, false, true); // reset just back color
		}

		void FireEditValueChanged(Control controlChanged)
		{
			ControlChanged = controlChanged;

			if (EditValueChanged != null)
				EditValueChanged(this, EventArgs.Empty);
		}
#endif

	}
}
