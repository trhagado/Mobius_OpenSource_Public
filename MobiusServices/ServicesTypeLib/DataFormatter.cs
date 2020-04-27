using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Mobius.Services.Types
{
	public class DataFormatter
	{

		/// <summary>
		/// Format query headers
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>

		public static string FormatQueryHeaders(
			Query query)
		{
			List<string> colNameList;
			List<string> colLabelList;
			List<int> qryColumnsToSuppress;
			List<int> qryColumnsToSplit;

			string tsv = FormatQueryHeaders(query, out colLabelList, out colLabelList, false, out qryColumnsToSuppress, false, out qryColumnsToSplit);
			return tsv;
		}

		/// <summary>
		/// Format query headers
		/// </summary>
		/// <param name="q"></param>
		/// <param name="colNameList"></param>
		/// <returns></returns>

		public static string FormatQueryHeaders(
			Query query,
			out List<string> colNameList,
			out List<string> colLabelList,
			bool suppressExtraneousCompoundIdColumns,
			out List<int> qryColumnsToSuppress,
			bool splitQualifiedNumbers,
			out List<int> qryColumnsToSplit)
		{
			colNameList = new List<string>();
			colLabelList = new List<string>();
			qryColumnsToSuppress = new List<int>();
			qryColumnsToSplit = new List<int>();

			if (query?.Tables == null) return "";

			try
			{

				// If requested, alter the column selection state to suppress extraneous compound id columns

				int qryColIdx = -1;

				if (suppressExtraneousCompoundIdColumns)
				{
					bool compoundIdSelected = false;
					if (query.Tables != null)
					{
						foreach (QueryTable table in query.Tables)
						{
							if (table.QueryColumns != null)
							{
								foreach (QueryColumn column in table.QueryColumns)
								{
									if (column.Selected &&
											column.MetaColumn != null)
									{
										qryColIdx++;
										if (column.MetaColumn.DataType == MetaColumnType.CompoundId)
										{
											if (!compoundIdSelected)
											{
												//found the first one -- keep it
												compoundIdSelected = true;
											}
											else
											{
												//found an additional one -- suppress it
												// would prefer to set column.Selected to false, but
												// that would blow up the Mobius QE while attempting to retrieve results
												qryColumnsToSuppress.Add(qryColIdx);
											}
										}
									}
								}
							}
						}
					}
				}

				StringBuilder sb = new StringBuilder();

				qryColIdx = -1;
				int idx = 0;

				// Include initial root key column that is in all rows returned by the QE and that has to associated QueryTable.QueryColumn

				sb.Append("Root.Key"); 
				colNameList.Add("dgvColumn" + (idx++));
				colLabelList.Add("Root.Key");

				foreach (QueryTable table in query.Tables)
				{
					string tableLabel = table.Label;

					if (tableLabel == null || tableLabel == "")
						tableLabel = table.MetaTable.Label;
 
					if (tableLabel == null || tableLabel == "")
						tableLabel = table.Alias;

 					if (tableLabel == null || tableLabel == "")
						tableLabel = table.MetaTable.Name;

					tableLabel = table.MetaTable.Name; // force metatable name!

					if (table.QueryColumns != null)
					{
						foreach (QueryColumn column in table.QueryColumns)
						{
							if (column.Selected)
							{
								string columnLabel = column.Label;

								if (columnLabel == null || columnLabel == "")
									columnLabel = column.MetaColumn.Label;

								if (columnLabel == null || columnLabel == "")
									columnLabel = column.MetaColumn.Name;

								columnLabel = column.MetaColumn.Name; // force metacolumn name!
								
								qryColIdx++;
								if (qryColumnsToSuppress.Contains(qryColIdx)) // skip?
									continue;

								// Add the column(s)
								// If qualified results are being shown in one column, always add one column
								// Otherwise, add two columns (qualifier before value) for qualified values and one for all other data types

								if (sb.Length > 0) sb.Append("\t");

								if (!splitQualifiedNumbers)
								{
									sb.Append(tableLabel + "." + columnLabel);
									colNameList.Add("dgvColumn" + (idx++));
									colLabelList.Add(tableLabel + "." + columnLabel);
								}

								else
								{
									if (column.MetaColumn.DataType == MetaColumnType.QualifiedNo)
									{
										sb.Append(tableLabel + "." + columnLabel + " (Q)");
										qryColumnsToSplit.Add(qryColIdx);

										colNameList.Add("dgvColumn" + (idx++));
										colLabelList.Add(tableLabel + "." + columnLabel + " (Q)");
									}
									sb.Append(tableLabel + "." + columnLabel);
									colNameList.Add("dgvColumn" + (idx++));
									colLabelList.Add(tableLabel + "." + columnLabel);
								}
							}
						}
					}
				}

				return sb.ToString();
			}

			catch (Exception ex)
			{
				return ex.Message + "\r\n" + new StackTrace(true);
			}
		}

		/// <summary>
		/// FormatDataRow
		/// </summary>
		/// <param name="vo"></param>
		/// <returns></returns>

		public static string FormatDataRow(
			object[] vo)
		{
			List<int> qryColumnsToSuppress = new List<int>();
			List<int> qryColumnsToSplit = new List<int>();
			List<string> valList;

			string tsv = FormatDataRow(vo, qryColumnsToSuppress, qryColumnsToSplit, out valList);
			return tsv;
		}

		/// <summary>
		/// FormatDataRow
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="valList"></param>
		/// <param name="splitQualifiedNumbers"></param>
		/// <returns></returns>

		public static string FormatDataRow(
		object[] vo,
		List<int> qryColumnsToSuppress,
		List<int> qryColumnsToSplit,
		out List<string> valList)
		{
			valList = new List<string>();

			if (vo == null || qryColumnsToSuppress == null || qryColumnsToSplit == null)
				return "";

			try
			{

				int colCount = vo.Length;

				StringBuilder sb = new StringBuilder();

				for (int j = 0; j < colCount; j++) // include initial common key that has no associated QueryTable.QueryColumn
				{
					if (qryColumnsToSuppress.Contains(j - 1)) // skip?
						continue;

					if (vo[j] == null)
					{
						AppendVal("~", valList, sb);

						if (qryColumnsToSplit.Contains(j - 1))
							AppendVal("~", valList, sb);
					}

					else
					{
						if (vo[j] is ChemicalStructure)
						{
							ChemicalStructure chemStruct = vo[j] as ChemicalStructure;
							AppendVal(chemStruct.Type.ToString() + ": " + chemStruct.Value, valList, sb);
						}

						else if (vo[j] is QualifiedNumber)
						{
							QualifiedNumber qualNum = vo[j] as QualifiedNumber;
							if (qualNum.IsNull)
							{
								AppendVal("~", valList, sb);
								if (qryColumnsToSplit.Contains(j - 1))
									AppendVal("~", valList, sb);
							}
							else
							{
								if (qryColumnsToSplit.Contains(j - 1))
								{
									AppendVal(qualNum.Qualifier, valList, sb);
									AppendVal(qualNum.NumberValue.ToString(), valList, sb);
								}

								else
								{
									if (qualNum.FormattedText != null)
										AppendVal(qualNum.FormattedText, valList, sb);

									else
										AppendVal(qualNum.Qualifier + qualNum.NumberValue.ToString(), valList, sb);
								}
							}
						}

						else if (vo[j] is DateTimeEx)
						{
							DateTimeEx dtEx = vo[j] as DateTimeEx;
							if (dtEx.IsNull)
							{
								AppendVal("~", valList, sb);
							}
							else
							{
								if (dtEx.FormattedText != null)
								{
									AppendVal(dtEx.FormattedText, valList, sb);
								}
								else
								{
									AppendVal(dtEx.Value.ToString(), valList, sb);
								}
							}
						}

						else if (vo[j] is NumberEx)
						{
							NumberEx numEx = vo[j] as NumberEx;
							if (numEx.IsNull)
							{
								AppendVal("~", valList, sb);
							}
							else
							{
								if (numEx.FormattedText != null)
									AppendVal(numEx.FormattedText, valList, sb);

								else AppendVal(numEx.Value.ToString(), valList, sb);
							}
						}

						else if (vo[j] is StringEx)
						{
							StringEx stringEx = vo[j] as StringEx;
							if (stringEx.IsNull)
								AppendVal("~", valList, sb);

							else AppendVal(stringEx.Value, valList, sb);
						}

						else // other type
						{
							if (vo[j] == null)
								AppendVal("~", valList, sb);

							else AppendVal(vo[j].ToString(), valList, sb);
						}
					}
				}

				return sb.ToString(); ;
			}

			catch (Exception ex)
			{
				return ex.Message + "\r\n" + new StackTrace(true);
			}
		}

		static void AppendVal(
			string val,
			List<string> valList,
			StringBuilder sb)
		{
			valList.Add(val);
			if (sb.Length > 0) sb.Append("\t");

			if (val.Contains("\r")) val = val.Replace("\r", "");
			if (val.Contains("\n")) val = val.Replace("\n", " ");
			if (val.Contains("\t")) val = val.Replace("\t", " ");

			sb.Append(val);
			return;
		}
	}
}
