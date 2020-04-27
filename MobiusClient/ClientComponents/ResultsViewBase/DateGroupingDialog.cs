using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;
using DevExpress.XtraPivotGrid;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
/// <summary>
/// Show conditional formatting summary dialog 
/// </summary>

	public partial class DateGroupingDialog : XtraForm
	{

		static DateGroupingDialog Instance;

		GroupingTypeEnum TimePeriodType = GroupingTypeEnum.EqualValues;
		bool InSetup = false;

		QueryResultsControl Qrc; // QueryResultsControl containg the results
		QueryManager RootQm; // QueryManager for top level query
		QueryManager Qm2; // QueryManager for derived query

		string ReturnMessage = "";

		public DateGroupingDialog()
		{
			InitializeComponent();
			Instance = this;
		}

		/// <summary>
		/// Show dialog & process results
		/// </summary>
		/// <returns></returns>

		public static DialogResult ShowDialog(
			ref GroupingTypeEnum timePeriodType)
		{
			if (Instance == null) Instance = new DateGroupingDialog();
			DateGroupingDialog i = Instance;
			i.TimePeriodType = timePeriodType;
			i.Setup(timePeriodType);

			DialogResult dr = i.ShowDialog(SessionManager.ActiveForm);
			if (dr == DialogResult.OK)
				timePeriodType = i.TimePeriodType;

			return dr;
		}


		/// <summary>
		/// Copy values from view to form
		/// </summary>

		void Setup(GroupingTypeEnum tpt)
		{
			InSetup = true;

			switch (tpt)
			{
				case GroupingTypeEnum.Date:
					Date.Checked = true;
					break;

				case GroupingTypeEnum.DateDay:
					DateDayOfMonth.Checked = true;
					break;

				case GroupingTypeEnum.DateDayOfWeek:
					DateDayOfWeek.Checked = true;
					break;

				case GroupingTypeEnum.DateDayOfYear:
					DateDayOfYear.Checked = true;
					break;

				case GroupingTypeEnum.DateWeekOfMonth:
					DateWeekOfMonth.Checked = true;
					break;

				case GroupingTypeEnum.DateWeekOfYear:
					DateWeekOfYear.Checked = true;
					break;

				case GroupingTypeEnum.DateMonth:
					DateMonth.Checked = true;
					break;

				case GroupingTypeEnum.DateQuarter:
					DateQuarter.Checked = true;
					break;

				case GroupingTypeEnum.DateYear:
					DateYear.Checked = true;
					break;

				default:
					None.Checked = true;
					break;
			}

			InSetup = false;

			return;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			if (Date.Checked) TimePeriodType = GroupingTypeEnum.Date;
			else if (DateDayOfMonth.Checked) TimePeriodType = GroupingTypeEnum.DateDay;
			else if (DateDayOfWeek.Checked) TimePeriodType = GroupingTypeEnum.DateDayOfWeek;
			else if (DateDayOfYear.Checked) TimePeriodType = GroupingTypeEnum.DateDayOfYear;
			else if (DateWeekOfMonth.Checked) TimePeriodType = GroupingTypeEnum.DateWeekOfMonth;
			else if (DateWeekOfYear.Checked) TimePeriodType = GroupingTypeEnum.DateWeekOfYear;
			else if (DateMonth.Checked) TimePeriodType = GroupingTypeEnum.DateMonth;
			else if (DateQuarter.Checked) TimePeriodType = GroupingTypeEnum.DateQuarter;
			else if (DateYear.Checked) TimePeriodType = GroupingTypeEnum.DateYear;
			else if (None.Checked) TimePeriodType = GroupingTypeEnum.EqualValues;

			DialogResult = DialogResult.OK;
			return;
		}

/// <summary>
/// Process input
/// </summary>

		void ProcessInput()
		{
			CfSummaryView view = null; // todo...

			QueryResultsControl qrc = QueryResultsControl.GetQrcThatContainsControl(view.RenderingControl);
			RootQm = Qrc.CrvQm;
			Query rootQuery = RootQm.Query;

			Query subQuery = BuildQuery();

			RootQm.Query.Subqueries.Add(subQuery); // add subquery to Parent
			subQuery.Parent = rootQuery; // link subquery to parent query
			view.BaseQuery = subQuery; // link view to subquery
		}

/// <summary>
/// Build the query to display results
/// </summary>

		Query BuildQuery()
		{
			Query q = RootQm.Query;
			Query q2 = q.Clone();
			int ti = 0;
			while (ti < q2.Tables.Count)
			{
				QueryTable qt = q2.Tables[ti];
				foreach (QueryColumn qc0 in qt.QueryColumns)
				{
					if (!qc0.Selected) continue;

					if (!qc0.IsKey &&
					 (qc0.CondFormat == null || qc0.CondFormat.Rules.Count == 0))
						qc0.Selected = false;
				}

				if (qt.SelectedCount <= 1 && ti > 0) // remove table if no cf & not 1st table
					q2.RemoveQueryTable(qt);

				else ti++;
			}
			return q2;
		}


	}

}