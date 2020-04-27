using Mobius.ComOps;
using Mobius.Data;
using Mobius.MolLib1;
using Mobius.ClientComponents;
using Mobius.MolLib1;
using Mobius.ServiceFacade;

using DevExpress.XtraEditors;
using DevExpress.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.Tools
{
/// <summary>
/// Rgroup Matrix tool
/// </summary>

	public partial class RgroupMatrix : DevExpress.XtraEditors.XtraForm
	{

		static int MatrixCount = 0; // used for assigning names to matrices

		public RgroupMatrix()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Main tool entry point
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public string Run(
			string args)
		{
			QbUtil.SetMode(QueryMode.Build); // be sure in build mode

			if (ServicesIniFile.Read("RgroupMatrixHelpUrl") != "")
				Help.Enabled = true;

			this.ShowDialog();
			return "";
		}

		private void EditStructure_Click(object sender, EventArgs e)
		{
			DelayedCallback.Schedule(SQuery.EditMolecule);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			DialogResult = ProcessInput();
			return;
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			return;
		}

		private void Help_Click(object sender, EventArgs e)
		{
			QbUtil.ShowConfigParameterDocument("RgroupMatrixHelpUrl", "Rgroup Matrix Help");
		}

		/// <summary>
		/// OK button clicked, process input
		/// </summary>
		/// <returns></returns>

		DialogResult ProcessInput()
		{
			int rgCount; // number of Rgroups
			List<Rgroup> rgList; // list of existing Rgroups
			bool[] rgExists; // entry = true if rgroup exists in core
			Rgroup rg;
			bool oneD, twoD; // matrix dimensionality
			List<string> keys = null;
			Dictionary<string, List<QualifiedNumber>> mElem; // matrix element dictionary
			List<RgroupSubstituent>[] rSubs; // substituents seen for each Rgroup
			Query q, q0, q2;
			QueryTable qt, qt2;
			QueryColumn qc, qc2;
			MetaTable mt, mt2;
			MetaColumn mc, mc2;
			DataTableMx dt;
			DataRowMx dr;
			DialogResult dlgRslt;
			string tok;
			int ri, rii, si, qti, qci, bi, bi2;

			// Get core structure & list of R-groups

			MoleculeMx core = new MoleculeMx(MoleculeFormat.Molfile, SQuery.MolfileString);
			if (core.AtomCount == 0)
			{
				MessageBoxMx.ShowError("A Core structure with R-groups must be defined");
				return DialogResult.None;
			}

			if (!Structure.Checked && !Smiles.Checked && !Formula.Checked &&
				!Weight.Checked && !Index.Checked)
			{
				MessageBoxMx.ShowError("At least one substituent display format must be selected.");
				return DialogResult.None;
			}

			mt = MetaTableCollection.GetWithException("Rgroup_Decomposition");
			qt = new QueryTable(mt);
			qc = qt.GetQueryColumnByNameWithException("Core");

			qc.MolString = core.GetMolfileString(); // put core structure into table criteria
			qc.CriteriaDisplay = "Substructure search (SSS)";
			qc.Criteria = "CORE SSS SQUERY";

			qc = qt.GetQueryColumnByNameWithException("R1_Structure");
			if (ShowCoreStructure.Checked)
			{
				qc.Label = "R-group, Core\tChime=" + core.GetChimeString(); // reference core in query col header label
				qc.MetaColumn.Width = 25;
			}

			RgroupDecomposition.SetSelected(qt, "R1_Structure", Structure.Checked); // select for retrieval if checked
			RgroupDecomposition.SetSelected(qt, "R1_Smiles", Smiles.Checked);
			RgroupDecomposition.SetSelected(qt, "R1_Formula", Formula.Checked);
			RgroupDecomposition.SetSelected(qt, "R1_Weight", Weight.Checked);
			RgroupDecomposition.SetSelected(qt, "R1_SubstNo", Index.Checked);

			string terminateOption = "First mapping"; // terminate on first complete match
			qc = qt.GetQueryColumnByNameWithException("Terminate_Option");
			qc.Criteria = qt.MetaTable.Name + " = " + Lex.AddSingleQuotes(terminateOption);
			qc.CriteriaDisplay = "= " + Lex.AddSingleQuotes(terminateOption);

			QueryTable rgdQt = qt; // keep a ref to it

			if (QbUtil.Query == null || QbUtil.Query.Tables.Count == 0)
			{
				MessageBoxMx.ShowError("No current query.");
				return DialogResult.None;
			}

			q0 = QbUtil.Query; // original query this analysis is based on
			q = q0.Clone(); // make copy of source query we can modify
			q.SingleStepExecution = false;

			qti = 0;
			while (qti < q.Tables.Count) // deselect query columns that we don't want
			{
				qt = q.Tables[qti];
				if (Lex.Eq(qt.MetaTable.Name, "Rgroup_Decomposition"))
				{ // remove any rgroup decomp table
					qti++;
					continue;
				}

				mt = qt.MetaTable;
				if (mt.MultiPivot || // check for tables not allowed in underlying query
				 mt.MetaBrokerType == MetaBrokerType.CalcField || // (called ShouldPresearchAndTransform previously)
				 mt.MetaBrokerType == MetaBrokerType.MultiTable ||
				 mt.MetaBrokerType == MetaBrokerType.RgroupDecomp)
				{
					MessageBoxMx.ShowError("Multipivot/Rgroup table \"" + qt.ActiveLabel +
						"\" can't be included in an underlying Rgroup Matrix query");
					return DialogResult.None;
				}

				for (qci = 0; qci < qt.QueryColumns.Count; qci++)
				{
					qc = qt.QueryColumns[qci];
					if (qc.MetaColumn == null) continue;

					switch (qc.MetaColumn.DataType)
					{
						case MetaColumnType.CompoundId: // keep only these
						case MetaColumnType.Integer:
						case MetaColumnType.Number:
						case MetaColumnType.QualifiedNo:
						case MetaColumnType.String:
							break;

						default:
							qc.Selected = false;
							break;
					}
				}

				qti++;
			}

			q.AddQueryTable(rgdQt); // Add Rgroup decom table to end of cloned source query

			Progress.Show("Retrieving data...");
			try
			{
				dlgRslt = ToolHelper.ExecuteQuery(ref q, out keys);
				if (dlgRslt != DialogResult.OK) return dlgRslt;
			}

			catch (Exception ex)
				{
				MessageBoxMx.ShowError("Error executing query:\r\n" + ex.Message);
				return DialogResult.None;
			}

			if (keys == null || keys.Count == 0)
			{
				Progress.Hide();
				MessageBoxMx.ShowError("No results were returned by the query.");
				return DialogResult.None;
			}

// Scan modified query to get list of rgroup indexes that are present

			rgExists = new bool[32];
			rgList = new List<Rgroup>();

			QueryTable rgQt = q.GetQueryTableByName("Rgroup_Decomposition");
			foreach (QueryColumn qc0 in rgQt.QueryColumns)
			{
				mc = qc0.MetaColumn;
				if (!(mc.Name.StartsWith("R") && mc.Name.EndsWith("_STRUCTURE") && qc0.Selected))
					continue; // skip if not a selected Rgroup structure

				int len = mc.Name.Length - ("R" + "_STRUCTURE").Length;
				tok = mc.Name.Substring(1, len);
				if (!int.TryParse(tok, out ri)) continue;
				rgExists[ri - 1] = true;
				rg = new Rgroup();
				rg.RIndex = ri;
				rg.VoPos = qc0.VoPosition;
				rgList.Add(rg);
			}

			for (bi = 1; bi < rgList.Count; bi++)
			{ // sort by increasing R index
				rg = rgList[bi];
				for (bi2 = bi - 1; bi2 >= 0; bi2--)
				{
					if (rg.RIndex >= rgList[bi2].RIndex) break;
					rgList[bi2 + 1] = rgList[bi2];
				}

				rgList[bi2 + 1] = rg;
			}

			rgCount = rgList.Count;

			twoD = TwoD.Checked;
			if (rgCount == 1) twoD = false; // if only 1 rgroup can't do as 2d
			oneD = !twoD;

// Read data into mElem and rgroup substituents into rSubs.
// Matrix mElem is keyed on [R1Smiles, R2Smiles,... RnSmiles, FieldName] for 1d and
// [R1Smiles, R2Smiles,... FieldName, RnSmiles] for 2d

			QueryManager qm = q.QueryManager as QueryManager;
			DataTableManager dtm = qm.DataTableManager;
			dt = qm.DataTable;

			mElem = new Dictionary<string, List<QualifiedNumber>>(); // matrix element dictionary
			rSubs = new List<RgroupSubstituent>[32]; // list of substituents seen for each Rgroup
			for (rii = 0; rii < rgCount; rii++) // alloc substituent list for rgroup
				rSubs[rii] = new List<RgroupSubstituent>();

			int rowCount = 0;
			while (true)
			{ // scan data accumulating rgroup substituents and data values
				dr = dtm.FetchNextDataRow();
				if (dr == null) break;
				rowCount++;

				string cid = dr[dtm.KeyValueVoPos] as string;
				string lastMapCid = "", rgroupKey = "", rgroupKeyLast = "";
				int mapCount = 0;
				for (rii = 0; rii < rgCount; rii++) // for 
				{
					MoleculeMx rSub = dr[rgList[rii].VoPos] as MoleculeMx;
					if (rSub == null || rSub.AtomCount == 0) continue;

					ri = rgList[rii].RIndex; // actual R index in query
					int subIdx = RgroupSubstituent.Get(rSub, rSubs[rii]);
					//					if (ri == 1 && subIdx != 0) subIdx = subIdx; // debug
					if (subIdx < 0) continue;
					string rKey = "R" + ri.ToString() + "_" + (subIdx + 1).ToString();

					if (oneD || rii < rgCount - 1)
					{
						if (rgroupKey != "") rgroupKey += "\t";
						rgroupKey += rKey;
					}

					else rgroupKeyLast = rKey;
					lastMapCid = cid;
					mapCount++;
				}

				if (lastMapCid == cid) // add the data if compound has a mapping
					AccumulateMatrixElements(mElem, q, dr, rgroupKey, rgroupKeyLast, cid);

				if (Progress.IsTimeToUpdate)
					Progress.Show("Retrieving data: " +  StringMx.FormatIntegerWithCommas(rowCount) + " rows...");
			}
			if (rowCount == 0)
			{
				Progress.Hide();
				MessageBoxMx.ShowError("No data rows retrieved");
				return DialogResult.None;
			}

			if (twoD && (rSubs[rgCount - 1] == null || rSubs[rgCount - 1].Count == 0))
			{ // if 2D be sure we have at least one substituent for the last Rgroup
				Progress.Hide();
				MessageBoxMx.ShowError("No substituents found for R" + rgCount.ToString());
				return DialogResult.None;
			}

		// Create a MetaTable & DataTable for matrix results

		Progress.Show("Analyzing data...");

			mt = new MetaTable(); // create output table
			MatrixCount++;
			mt.Name = "RGROUPMATRIX_" + MatrixCount;
			mt.Label = "R-group Matrix " + MatrixCount;
			mt.MetaBrokerType = MetaBrokerType.RgroupDecomp;

			mc = // use sequence for key
				mt.AddMetaColumn("RgroupMatrixId", "No.", MetaColumnType.Integer, ColumnSelectionEnum.Selected, 3);
			mc.ClickFunction = "None"; // avoid hyperlink on this key
			mc.IsKey = true;

			int maxLeftR = rgCount;
			if (twoD) maxLeftR = rgCount - 1;
			for (ri = 0; ri < maxLeftR; ri++)
			{
				string rStr = "R" + (ri + 1).ToString();
				if (Structure.Checked)
				{
					mc = mt.AddMetaColumn(rStr + "Str", rStr, MetaColumnType.Structure, ColumnSelectionEnum.Selected, 12);
					if (ri == 0 && ShowCoreStructure.Checked) // include core structure above R1 if requested
					{
						string chimeString = MolLib1.StructureConverter.MolfileStringToSmilesString(SQuery.MolfileString);
						mc.Label = "R1, Core\tChime=" + chimeString;
						mc.Width = 25;
					}

				}
				if (Smiles.Checked)
				{
					mc = mt.AddMetaColumn(rStr + "Smi", rStr + " Smiles", MetaColumnType.String, ColumnSelectionEnum.Selected, 12);
				}
				if (Formula.Checked)
				{
					mc = mt.AddMetaColumn(rStr + "Mf", rStr + " Formula", MetaColumnType.String, ColumnSelectionEnum.Selected, 8);
				}
				if (Weight.Checked)
				{
					mc = mt.AddMetaColumn(rStr + "MW", rStr + " Mol. Wt.", MetaColumnType.Number, ColumnSelectionEnum.Selected, 6, ColumnFormatEnum.Decimal, 2);
				}

				if (Index.Checked)
				{
					mc = mt.AddMetaColumn(rStr + "Index", rStr + " Subst. Idx.", MetaColumnType.Number, ColumnSelectionEnum.Selected, 4);
					mc.Format = ColumnFormatEnum.Decimal;
				}
			}

			mc = // add column to contain result type
				mt.AddMetaColumn("ResultType", "Result Type", MetaColumnType.String, ColumnSelectionEnum.Selected, 12);

			if (oneD) // add just 1 column to contain results
			{
				mc = mt.AddMetaColumn("Results", "Results", MetaColumnType.QualifiedNo, ColumnSelectionEnum.Selected, 12);
				mc.MetaBrokerType = MetaBrokerType.RgroupDecomp; // broker to do special col handling for cond formtting
				if (QbUtil.Query.UserObject.Id > 0)
					mc.DetailsAvailable = true;
			}

			else // add col for each substituent for last rgroup
			{
				string rStr = "R" + rgCount.ToString();
				for (si = 0; si < rSubs[rgCount - 1].Count; si++)
				{
					string cName = rStr + "_" + (si + 1).ToString();
					string cLabel = cName.Replace("_", ".");
					RgroupSubstituent rgs = rSubs[ri][si]; // get substituent info
					if (Structure.Checked) // include structure
						cLabel += "\tChime=" + rgs.Struct.GetChimeString();

					else if (Smiles.Checked)
						cLabel += " = " + rgs.Struct.GetSmilesString();

					else if (Formula.Checked)
						cLabel += " = " + rgs.Struct.MolFormula;

					else if (Weight.Checked)
						cLabel += " = " + rgs.Struct.MolWeight;

					else if (Index.Checked)
						cLabel += " = " + (si + 1).ToString();

					mc = mt.AddMetaColumn(cName, cLabel, MetaColumnType.QualifiedNo, ColumnSelectionEnum.Selected, 12);
					mc.MetaBrokerType = MetaBrokerType.RgroupDecomp;
					if (QbUtil.Query.UserObject.Id > 0)
						mc.DetailsAvailable = true;
				}
			}

			MetaTableCollection.UpdateGlobally(mt); // add as a known metatable

			if (mElem.Count == 0) // be sure we have a matrix
			{
				Progress.Hide();
				MessageBoxMx.ShowError("No matrix can be created because insufficient data was found.");
				return DialogResult.None;
			}

			// Build the DataTable

			Progress.Show("Building data table...");

			q2 = new Query(); // build single-table query to hold matrix
			qt2 = new QueryTable(q2, mt);
			dt = DataTableManager.BuildDataTable(q2);

			Dictionary<string, List<QualifiedNumber>>.KeyCollection kc = mElem.Keys;
			string[] rgKeys = new string[mElem.Count];
			kc.CopyTo(rgKeys, 0);
			Array.Sort(rgKeys);

			string[] rgKey = null, lastRgKey = null;
			int rki = 0;

			for (rki = 0; rki < rgKeys.Length; rki++)
			{
				rgKey = rgKeys[rki].Split('\t');

				int riTop = rgCount + 1; // all r substituents & field name on left
				if (twoD) riTop = rgCount;

				for (ri = 0; ri < riTop; ri++) // see if any changes in left side substituents or field name
				{
					if (lastRgKey == null || rgKey[ri] != lastRgKey[ri]) break;
				}
				if (ri < riTop || oneD) // if 2d then new row only if some change before last R
				{
					dr = dt.NewRow();
					dt.Rows.Add(dr);
					dr[dtm.KeyValueVoPos + 1] = new NumberMx(dt.Rows.Count); // integer row key
				}

				if (!HideRepeatingSubstituents.Checked) ri = 0; // start at first if not hiding

				lastRgKey = rgKey;

				for (ri = ri; ri < riTop; ri++) // build row with these
				{
					string rgSub = rgKey[ri]; // get substituent id or table.column name
					if (rgSub == "") continue;

					if (ri < riTop - 1)
					{ // output substituent and/or smiles
						string rStr = "R" + (ri + 1).ToString();
						si = rgSub.IndexOf("_");
						si = Int32.Parse(rgSub.Substring(si + 1)) - 1; // get substituent index
						RgroupSubstituent rgs = rSubs[ri][si]; // get substituent info

						if (Structure.Checked)
						{
							qc2 = qt2.GetQueryColumnByName(rStr + "Str");
							dr[QcToDcName(qc2)] = rgs.Struct;
						}

						if (Smiles.Checked)
						{
							qc2 = qt2.GetQueryColumnByName(rStr + "Smi");
							dr[QcToDcName(qc2)] = new StringMx(rgs.Struct.GetSmilesString());
						}

						if (Formula.Checked)
						{
							qc2 = qt2.GetQueryColumnByName(rStr + "Mf");
							dr[QcToDcName(qc2)] = new StringMx(rgs.Struct.MolFormula);
						}

						if (Weight.Checked)
						{
							qc2 = qt2.GetQueryColumnByName(rStr + "Mw");
							dr[QcToDcName(qc2)] = new NumberMx(rgs.Struct.MolWeight);
						}

						if (Index.Checked)
						{
							qc2 = qt2.GetQueryColumnByName(rStr + "Index");
							dr[QcToDcName(qc2)] = new NumberMx(si + 1);
						}
					}

					else // output field name
					{
						string[] sa = rgSub.Split('.'); // get field name
						qt = q.GetQueryTableByName(sa[0]);
						qc = qt.GetQueryColumnByName(sa[1]);
						string fieldName = qc.ActiveLabel;
						if (q0.Tables.Count >= 3) // qualify by table if 3 or more tables in original query
							fieldName = qt.ActiveLabel + " - " + fieldName;

						qc2 = qt2.GetQueryColumnByName("ResultType");
						dr[QcToDcName(qc2)] = new StringMx(fieldName);
					}
				}

				// Output value

				string cName;
				if (oneD) cName = "Results";
				else cName = rgKey[rgCount]; // get key for this substituent (e.g. R2_1)
				if (Lex.IsUndefined(cName)) continue; // may be no substituent match

				qc2 = qt2.GetQueryColumnByName(cName);
				QualifiedNumber qn = SummarizeData(mElem[rgKeys[rki]]); // get summarized value
				dr[QcToDcName(qc2)] = qn;
			}

			ToolHelper.DisplayData(q2, dt, true);

			UsageDao.LogEvent("RgroupMatrix");
			Progress.Hide();
			return DialogResult.OK;
		}

/// <summary>
/// Convert a QueryColumn to a DataTable Column name
/// </summary>
/// <param name="qc"></param>
/// <returns></returns>

		string QcToDcName(QueryColumn qc)
		{
			return qc.QueryTable.Alias + "." + qc.MetaColumn.Name;
		}

		/// <summary>
		/// Accumulate data for a compound
		/// </summary>
		/// <param name="mElem"></param>
		/// <param name="dt"></param>
		/// <param name="dr"></param>
		/// <param name="rgroupKey"></param>

		static void AccumulateMatrixElements(
			Dictionary<string, List<QualifiedNumber>> mElem,
			Query q,
			DataRowMx dr,
			string rgroupKey,
			string rgroupKeyLast,
			string cid)
		{
			QualifiedNumber qn = null;

			foreach (QueryTable qt in q.Tables)
			{
				MetaTable mt = qt.MetaTable;
				if (Lex.Eq(mt.Name, "Rgroup_Decomposition")) continue; // ignore the rgroup info here
				foreach (QueryColumn qc in qt.QueryColumns)
				{
					if (!qc.Selected) continue;
					MetaColumn mc = qc.MetaColumn;
					if (mc.DataType == MetaColumnType.CompoundId) continue; // no data for compound ids

					object o = dr[qc.VoPosition];
					if (NullValue.IsNull(o)) continue; // skip null;

					string key = rgroupKey + "\t" + mt.Name + "." + mc.Name + "\t" + rgroupKeyLast;
					if (!mElem.ContainsKey(key))
						mElem[key] = new List<QualifiedNumber>();

					if (!QualifiedNumber.TryConvertTo(o, out qn)) continue;

					if (qn.NumberValue == QualifiedNumber.NullNumber) continue;

					qn.Hyperlink = qn.DbLink = mt.Name + "," + mc.Name + "," + cid; // store metatable/col name & compound id used for drilldown

					mElem[key].Add(qn); // add object 
				}
			}

		}

		/// <summary>
		/// Summarize data for a cell in the array
		/// </summary>
		/// <param name="qnList"></param>
		/// <returns></returns>

		static QualifiedNumber SummarizeData(
			List<QualifiedNumber> qnList)
		{
			QualifiedNumber qn = null;

			double maxGt = NullValue.NullNumber; // seen number qualified with ">"
			double minLt = NullValue.NullNumber; // seen number qualified with "<"
			bool seenExact = false;
			int nt = 0; // number of observations
			string textV = null;
			List<QualifiedNumber> qnList2 = new List<QualifiedNumber>(); // list of included elements

			for (int li = 0; li < qnList.Count; li++)
			{
				qn = qnList[li];
				if (qn.NumberValue == NullValue.NullNumber && qn.TextValue == "") continue;

				nt++;

				if (qn.Qualifier == "" || qn.Qualifier == "=")
				{
					if (!seenExact)
					{
						qnList2.Clear();
						seenExact = true;
					}

					qnList2.Add(qn);
				}

				else if (seenExact) continue; // ignore rest if seen exact

				else if (qn.Qualifier == ">")
				{
					if (maxGt == NullValue.NullNumber ||
						qn.NumberValue > maxGt)
					{
						qnList2.Clear();
						qnList2.Add(qn);
						maxGt = qn.NumberValue;
					}
					else if (qn.NumberValue == maxGt)
						qnList2.Add(qn);
				}

				else if (maxGt != NullValue.NullNumber) continue;

				else if (qn.Qualifier == "<")
				{
					if (minLt == NullValue.NullNumber ||
						qn.NumberValue < minLt)
					{
						qnList2.Clear();
						qnList2.Add(qn);
						minLt = qn.NumberValue;
					}
					else if (qn.NumberValue == minLt)
						qnList2.Add(qn);
				}

				else if (minLt != NullValue.NullNumber) continue;

				else if (qn.TextValue != "")
				{
					if (textV == null) // first text
					{
						qnList2.Clear();
						qnList2.Add(qn);
						textV = qn.TextValue;
					}
					else if (Lex.Eq(textV, qn.TextValue)) // same text
						qnList2.Add(qn);

					else // other text, link to them all
					{
						textV = "N.D.";
						qnList2.Add(qn);
					}

				}
			}

			int n = qnList2.Count;
			if (n == 0) return null;

			qn = new QualifiedNumber(); // build summarized value here

			if (seenExact)
			{
				if (n == 1)
					qn.NumberValue = qnList2[0].NumberValue;

				else // calc standard deviation
				{
					double sum = 0;
					foreach (QualifiedNumber qn2 in qnList2) // sum included values
						sum += qn2.NumberValue;
					double mean = sum / n; // arithmetic mean (not geometric)
					sum = 0;
					foreach (QualifiedNumber qn2 in qnList2) // sum squares of difference from mean
						sum += Math.Pow(qn2.NumberValue - mean, 2);
					double variance = (1.0 / (n - 1.0)) * sum;
					double sd = Math.Sqrt(variance);
					qn.NumberValue = mean;
					qn.StandardDeviation = sd;
				}
			}

			else if (maxGt != NullValue.NullNumber)
			{
				qn.Qualifier = ">";
				qn.NumberValue = qnList2[0].NumberValue;
			}

			else if (minLt != NullValue.NullNumber)
			{
				qn.Qualifier = "<";
				qn.NumberValue = qnList2[0].NumberValue;
			}

			else if (textV != null)
			{
				qn.TextValue = textV;
			}

			else return null; // null

			qn.NValue = n;
			qn.NValueTested = nt;

			//QualifiedNumber qnt = new QualifiedNumber(); // store formatted value in text for display purposes
			//QnfEnum qnFormat = QnfEnum.Combined |
			// QnfEnum.StdDev | QnfEnum.NValue | QnfEnum.NValueTested | QnfEnum.DisplayNLabel;
			//QueryColumn qc = new QueryColumn(); // dummy query column
			//qc.MetaColumn = mc;
			//qnt.TextValue = QueryExec.FormatQualifiedNumber (
			//  qn, qc, false, DisplayFormat.SigDigits, 3, qnFormat, OutputDest.Grid);
			//qn = qnt;

			string sourceMt = null, sourceMc = null;
			StringBuilder sb = new StringBuilder();
			Dictionary<string, string> cnDict = new Dictionary<string, string>(); // dict of keys we have seen
			foreach (QualifiedNumber qn2 in qnList2) // get list of compound ids
			{
				string[] sa = qn2.Hyperlink.Split(',');
				if (sourceMt == null)
				{
					sourceMt = sa[0];
					sourceMc = sa[1];
				}
				string cn = sa[2];
				if (cnDict.ContainsKey(cn)) continue;
				cnDict[cn] = null;
				if (sb.Length > 0) sb.Append(",");
				sb.Append(sa[2]);
			}

			Query q = SessionManager.Instance.QueryBuilderQuery;
			if (q.UserObject.Id > 0)
				qn.Hyperlink = qn.DbLink = // build drilldown command: queryId, mtName, mcName, sn1, sn2,...snn
					q.UserObject.Id.ToString() + "," + // queryId
					sourceMt + "," + sourceMc + "," + // source table & column
					sb.ToString(); // compound ids

			return qn;
		}

/// <summary>
/// Information on Rgroup
/// </summary>

		class Rgroup
		{
			internal int RIndex; // subscript for group
			internal int VoPos; // position of data in vo
		}

	/// <summary>
	/// Substituent information
	/// </summary>

		public class RgroupSubstituent
		{
			internal double Mw; // mol weight, used for sorting & speeding match identity
			internal MoleculeMx Struct;

			static StructureMatcher matcher;

			/// <summary>
			/// Get substituent for a decomposition rgroup & map position & store in list
			/// </summary>
			/// <param name="rSub">Substituent structure</param>
			/// <param name="rSubs">List of substituents so far</param>
			/// <returns>Index of the substituent in rSubs</returns>

			public static int Get(
				MoleculeMx rSub,
				List<RgroupSubstituent> rSubs)
			{
				double mw = rSub.MolWeight;
				int si;
				for (si = 0; si < rSubs.Count; si++)
				{ // see if we've seen this substituent
					if (mw != rSubs[si].Mw) continue; // quick compare on mol weight first

					if (matcher == null) matcher =  new StructureMatcher();
					if (matcher.Equal(rSub, rSubs[si].Struct)) break;
				}

				if (si >= rSubs.Count)
				{ // add new item
					RgroupSubstituent rgs = new RgroupSubstituent();
					rgs.Mw = mw;
					rgs.Struct = rSub;
					rSubs.Add(rgs);
				}

				return si;
			}

		}

		private void RetrieveModel_Click(object sender, EventArgs e)
		{
			MoleculeSelectorControl.ShowModelSelectionMenu(new System.Drawing.Point(0, RetrieveModel.Size.Height), SQuery, StructureSearchType.Substructure);
		}
	}
}