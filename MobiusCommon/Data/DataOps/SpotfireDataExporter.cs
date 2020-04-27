using Mobius.ComOps;
using Mobius.Data;
using Mobius.SpotfireDocument;

using Spotfire.Dxp.Data.Formats.Stdf;
using Spotfire.Dxp.Data.Formats.Sbdf;

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mobius.Data
{

	/// <summary>
	/// Export a Mobius data set to Spotfire text or binary format files
	/// </summary>

	public class SpotfireDataExporter
	{
		SpotfireFileFormat Sff = SpotfireFileFormat.Undefined;
		public bool TextFormat => (Sff == SpotfireFileFormat.Text); // Spotfire Text Data File format
		public bool BinaryFormat => (Sff == SpotfireFileFormat.Binary); // Spotfire Binary Data File format

		int StdfWriteValueCount = 0;
		string StdfValueString = "";

		public const int KeyValueVoPos = 0; // position of common key value in vo

		/// <summary>
		/// Write query results to Spotfile data file(s)
		/// Handles writing of both STDF and SBDF files
		/// </summary>
		/// <param name="query"></param>
		/// <param name="Rows"></param>
		/// <param name="ep"></param>
		/// <returns></returns>

		public string WriteSpotfireDataFiles( 
			Query query,
			VoArrayList Rows,
			ExportParms ep)
		{
			string result = "";

			bool mergedFile = Lex.Contains(ep.OutputFileName2, "Single");
			if (query.Tables.Count <= 1) mergedFile = false; // don't write merged file if only one table

			bool multipleFiles = Lex.Contains(ep.OutputFileName2, "Multiple");
			if (!mergedFile) multipleFiles = true; // do multiple if single doesn't apply

			if (mergedFile)
				result = WriteMergedSpotfireDataFileForCombinedQueryTables(query, Rows, ep) + "\r\n";

			if (multipleFiles)
				result += WriteIndividualSpotfireDataFilesForEachQueryTable(query, Rows, ep) + "\r\n";

			return result;
		}

		/// <summary>
		/// Write results to single Spotfire data file merging results from all QueryTables in the query
		/// Handles writing of both STDF and SBDF files
		/// </summary>
		/// <param name="query"></param>
		/// <param name="Rows"></param>
		/// <param name="ep"></param>
		/// <returns></returns>

		public string WriteMergedSpotfireDataFileForCombinedQueryTables(
			Query query,
			VoArrayList Rows,
			ExportParms ep)
		{
			QueryTable qt;
			QueryColumn qc, qcKey;
			MetaTable mt;
			MetaColumn mc, mcKey;

			object vo, voKey;
			string colName = "", molString = "";
			int gci = 0;
			int fileCount = 0;
			int rowCount = 0;

			Sff = SpotfireFileFormat.Text;
			if (ep.ExportFileFormat == ExportFileFormat.Sbdf)
				Sff = SpotfireFileFormat.Binary;

			string fileName = ep.OutputFileName;
			HashSet<MetaColumn> nValueMetaColumns = new HashSet<MetaColumn>();

			string extraColNameSuffix = ColumnMapParms.SpotfireExportExtraColNameSuffix;

			QueryResultsVoMap voMap = QueryResultsVoMap.BuildFromQuery(query);

			//if (TextFormat)
			//{
			//	if (!Lex.EndsWith(fileName, ".txt")) fileName += ".txt"; // needed for IIS use
			//}
			//else if (!Lex.EndsWith(fileName, ".bin")) fileName += ".bin"; // needed for IIS use

			// Build the metadata for the table

			SpotfireDataFileMetadataBuilder mdb = new SpotfireDataFileMetadataBuilder(Sff);

			Dictionary<string, int> mtDict =  // dictionary keyed on metatable name with the values incremented for each occurance of the metatable in the query
				new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			for (int ti = 0; ti < voMap.Tables.Count; ti++)
			{
				QueryTableVoMap qtMap = voMap.Tables[ti];
				qt = qtMap.Table;
				mt = qt.MetaTable;
				mcKey = mt.KeyMetaColumn;
				qcKey = qt.KeyQueryColumn;

				if (!mtDict.ContainsKey(mt.Name))
					mtDict.Add(mt.Name, 0);

				mtDict[mt.Name]++;

				string nameSuffix = "";
				if (mtDict[mt.Name] > 1) nameSuffix = "." + mtDict[mt.Name];

				for (int fi = 0; fi < qtMap.SelectedColumns.Count; fi++)
				{
					qc = qtMap.SelectedColumns[fi];
					mc = qc.MetaColumn;

					if (ti > 0 && fi == 0 && mc.IsKey) // don't include keys for tables beyond the first
						continue;

					colName = mt.Name + "." + mc.Name + nameSuffix; // use internal mt.mc name
					AddMetadataForColumn(qc, colName, extraColNameSuffix, mdb, ep, nValueMetaColumns);
				}

		} // table loop

			SpotfireDataFileTableMetadata tableMetaData = mdb.Build(); // do build of metadata

			FileUtil.DeleteFile(fileName);
			SpotfireDataFileTableWriter tw = new SpotfireDataFileTableWriter(fileName, tableMetaData); // write the metadata to the stream

			//String mdString = ""; // convert MD to readable string
			//for (int mci = 0; mci < tableMetaData.Columns.Count; mci++)
			//{
			//	StdfColumnMetadata cmd = tableMetaData.Columns[mci];
			//	mdString += mci.ToString() + ", " + cmd.Name + ", " + cmd.DataType.TypeName + "\r\n";
			//}

			// Write out the data for each row

			for (int dri = 0; dri < Rows.TotalRowCount; dri++)
			{
				StdfWriteValueCount = 0;
				StdfValueString = "";

				object[] dr = Rows[dri];
				string keyValueForRow = dr[KeyValueVoPos] as string; // get key value for row

				// Process each table for row

				for (int ti = 0; ti < voMap.Tables.Count; ti++)
				{
					QueryTableVoMap qtMap = voMap.Tables[ti];
					qt = qtMap.Table;
					mt = qt.MetaTable;

					bool outputNValues = (QnSubcolumns.NValueIsSet(ep.QualifiedNumberSplit) && mt.UseSummarizedData); // should output n values for this table

					int keyFieldPos = qt.KeyQueryColumn.VoPosition; // key field position in vo
					string keyValueForTable = dr[keyFieldPos] as string; // get key value for table
					bool noDataForTable = NullValue.IsNull(keyValueForTable); // if key not defined then no data for the table for this data table row

					WriteRowValues(dr, qtMap, tw, ep, outputNValues, nValueMetaColumns, noDataForTable, ti, keyValueForRow);

				} // table loop

				rowCount++;
				WriteValue(tw, null, null); // write end of line as appropriate
			} // row loop

			tw.Close();
			fileCount++;

			string response = "Data exported to file: " + fileName + "\r\n";

			response +=
			"- Data rows: " + rowCount;

			return response;
		} // WriteMergedSpotfireDataFileForCombinedQueryTables


		/// <summary>
		/// Write results to individual Spotfire text data files
		/// Handles writing of both STDF and SBDF files
		/// </summary>
		/// <param name="query"></param>
		/// <param name="Rows"></param>
		/// <param name="ep"></param>
		/// <returns></returns>

		public string WriteIndividualSpotfireDataFilesForEachQueryTable(

			Query query,
			VoArrayList Rows,
			ExportParms ep)
		{
			QueryTable qt;
			QueryColumn qc, qcKey;
			MetaTable mt;
			MetaColumn mc, mcKey;
			SpotfireDataFileValueType sdfType;
			object vo, vo2, voKey;
			string outputFile = "", colName = "", molString = "";
			int gci = 0;
			int fileCount = 0;
			int rowCount = 0;

			Sff = SpotfireFileFormat.Text;
			if (ep.ExportFileFormat == ExportFileFormat.Sbdf)
				Sff = SpotfireFileFormat.Binary;

			string baseOutputFileName = ep.OutputFileName;
			bool outputNValues = QnSubcolumns.NValueIsSet(ep.QualifiedNumberSplit);
			HashSet<MetaColumn> nValueMetaColumns = new HashSet<MetaColumn>();

			string extraColNameSuffix = ColumnMapParms.SpotfireExportExtraColNameSuffix;

			QueryResultsVoMap voMap = QueryResultsVoMap.BuildFromQuery(query, includeKeyColsForAllTables: true);

			string baseStub = "";
			int i1 = baseOutputFileName.IndexOf('.');
			if (i1 >= 0)
				baseStub = baseOutputFileName.Substring(0, i1);

			else baseStub = baseOutputFileName;

			// Process each table

			for (int ti = 0; ti < voMap.Tables.Count; ti++)
			{
				QueryTableVoMap qtMap = voMap.Tables[ti];
				qt = qtMap.Table;
				mt = qt.MetaTable;
				mcKey = mt.KeyMetaColumn;
				qcKey = qt.KeyQueryColumn;

				outputFile = Lex.Replace(baseOutputFileName, baseStub, baseStub + "_" + mt.Name); // append meta table name to stub to get name of file to output

				//if (TextFormat)
				//{
				//	if (!Lex.EndsWith(outputFile, ".txt")) outputFile += ".txt"; // needed for IIS use
				//}
				//else if (!Lex.EndsWith(outputFile, ".bin")) outputFile += ".bin"; // needed for IIS use

				SpotfireDataFileMetadataBuilder mdb = new SpotfireDataFileMetadataBuilder(Sff);

				for (int fi = 0; fi < qtMap.SelectedColumns.Count; fi++)
				{
					qc = qtMap.SelectedColumns[fi];
					mc = qc.MetaColumn;

					sdfType = GetSpotfireDataFileType(mc);
					qc.SpotfireExportType = sdfType;
					colName = mt.Name + "." + mc.Name; // use internal mt.mc name

					AddMetadataForColumn(qc, colName, extraColNameSuffix, mdb, ep, nValueMetaColumns);
				}

				SpotfireDataFileTableMetadata tableMetaData = mdb.Build(); // do build of metadata

				// Write out the data for the table 

				FileUtil.DeleteFile(outputFile);
				SpotfireDataFileTableWriter tw = new SpotfireDataFileTableWriter(outputFile, tableMetaData);

				for (int dri = 0; dri < Rows.TotalRowCount; dri++)
				{
					object[] dr = Rows[dri];

					int qcKeyValueVoPos = qcKey.VoPosition;

					voKey = dr[qcKeyValueVoPos]; // get key value for row for this QueryTable
					if (voKey == null) continue;
					voKey = MobiusDataType.ConvertToPrimitiveValue(voKey, mcKey);
					if (NullValue.IsNull(voKey)) continue; // if key not defined then don't write anything for the row

					WriteRowValues(dr, qtMap, tw, ep, outputNValues, nValueMetaColumns);

					rowCount++;
					WriteValue(tw, null, null); // write end of line as appropriate
				} // row loop

				tw.Close();

				fileCount++;

			} // table loop

			string response;

			if (fileCount == 1) response =
				"Data exported to file: " + outputFile + "\r\n";

			else response =
				"Data exported to folder: " + baseOutputFileName + "\r\n" +
				"- DataTable files: " + fileCount + "\r\n";

			response +=
			"- Data rows: " + rowCount;

			return response;
		} // WriteIndividualSpotfireDataFilesForEachQueryTable

		/// <summary>
		/// Build and add the metadata for a column
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="nameSuffix"></param>
		/// <param name="extraColNameSuffix"></param>
		/// <param name="mdb"></param>
    /// <param name="nValueMetaColumns"></param>

		void AddMetadataForColumn(
			QueryColumn qc,
			string colName,
			string extraColNameSuffix,
			SpotfireDataFileMetadataBuilder mdb,
			ExportParms ep,
			HashSet<MetaColumn> nValueMetaColumns)
		{
			QueryTable qt = qc.QueryTable;
			MetaTable mt = qt.MetaTable;
			MetaColumn mc = qc.MetaColumn;

			SpotfireDataFileValueType sdfType = GetSpotfireDataFileType(mc);
			qc.SpotfireExportType = sdfType;

			// Structures get added as string type columns but the ContentType property for the DataTable Columns should be set to
			// chemical/x-mdl-molfile, chemical/x-mdl-chime  or chemical/x-daylight-smiles
			// Also for visualizations columns that display structures the renderer must be set to a structure renderer
			// if the structure format is chime (molfile and smiles autodetect)

			if (mc.DataType == MetaColumnType.Structure)
				mdb.AddColumn(colName + extraColNameSuffix, sdfType);

			// qualified number -> 1 to 3 Spotfire columns

			else if (mc.DataType == MetaColumnType.QualifiedNo)
			{
				if (QnSubcolumns.IsSplitFormat(ep.QualifiedNumberSplit))
				{
					mdb.AddColumn(colName + "_PRFX_TXT" + extraColNameSuffix, SpotfireDataFileValueType.String); // qualifier

					mdb.AddColumn(colName + extraColNameSuffix, SpotfireDataFileValueType.Double); // main value, use basic colname

					if (mt.SummarizedExists && mt.UseSummarizedData)
					{
						mdb.AddColumn(colName + "_NBR_VALS_CNSDRD" + extraColNameSuffix, SpotfireDataFileValueType.Int); // number of values included if summary value
						nValueMetaColumns.Add(mc);
					}
				}

				else // combined QNs
				{
					mdb.AddColumn(colName + extraColNameSuffix, SpotfireDataFileValueType.String); // combined value
				}
			}

			// Other column types use single column in Spotfire of the specified type

			else mdb.AddColumn(colName + extraColNameSuffix, sdfType);

			return;
		}

		/// <summary>
		/// Write out the values for a data row
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="qtMap"></param>
		/// <param name="tw"></param>
		/// <param name="ep"></param>
		/// <param name="outputNValues"></param>
		/// <param name="nValueMetaColumns"></param>
		/// <param name="noDataForTable"></param>
		/// <param name="ti"></param>
		/// <param name="keyValueForRow"></param>

		void WriteRowValues(
			object[] dr,
			QueryTableVoMap qtMap,
			SpotfireDataFileTableWriter tw,
			ExportParms ep,
			bool outputNValues,
			HashSet<MetaColumn> nValueMetaColumns,
			bool noDataForTable = false,
			int ti = 0,
			string keyValueForRow = "")
		{
			QueryColumn qc;
			MetaColumn mc;
			SpotfireDataFileValueType sdfType;
			QualifiedNumber qn;
			object vo, vo2;
			double dVal;
			string txt;

			for (int fi = 0; fi < qtMap.SelectedColumns.Count; fi++)
			{
				qc = qtMap.SelectedColumns[fi];
				mc = qc.MetaColumn;

				sdfType = (SpotfireDataFileValueType)qc.SpotfireExportType;

				if (noDataForTable && ti == 0 && qc.IsKey) // if this is the root table and no data then supply the row key value
				{
					vo = keyValueForRow;
					noDataForTable = false; // now have data
				}

				if (noDataForTable) vo = null;
				else vo = dr[qc.VoPosition];

				bool isNull = NullValue.IsNull(vo);

				if (isNull && (mc.DataType != MetaColumnType.QualifiedNo)) // write null value (unless QN which may require multiple value writes)
				{
					WriteValue(tw, sdfType, null);
				}

				else if (mc.DataType == MetaColumnType.Structure)
				{
					if (vo is MoleculeMx)
					{
						string molString = GetMoleculeString(vo, ep.ExportStructureFormat);
						WriteValue(tw, sdfType, molString);
					}

					else
					{
						vo2 = MobiusDataType.ConvertToPrimitiveValue(vo, mc);
						WriteValue(tw, sdfType, vo2);
					}
				}


				else if (mc.DataType == MetaColumnType.QualifiedNo) // write 1-3 values for Qualified number
				{

					// Output a split QN

					if (QnSubcolumns.IsSplitFormat(ep.QualifiedNumberSplit))
					{
						if (vo is QualifiedNumber && !isNull) // regular QN
						{
							qn = (QualifiedNumber)vo;
							WriteValue(tw, SpotfireDataFileValueType.String, qn.Qualifier); // qualifier
							WriteValue(tw, SpotfireDataFileValueType.Double, qn.NumberValue);

							if (outputNValues && nValueMetaColumns.Contains(mc))
							{
								if (NullValue.IsNull(qn.NValueTested))
									WriteValue(tw, SpotfireDataFileValueType.Int, null); // number in calc
								else WriteValue(tw, SpotfireDataFileValueType.Int, qn.NValueTested);
							}
						}

						else if (!isNull) // non-qn
						{
							WriteValue(tw, SpotfireDataFileValueType.String, null); // qualifier

							if (QualifiedNumber.TryConvertToDouble(vo, out dVal))
								WriteValue(tw, SpotfireDataFileValueType.Double, dVal);
							else WriteValue(tw, SpotfireDataFileValueType.Double, null);

							if (outputNValues && nValueMetaColumns.Contains(mc))
								WriteValue(tw, SpotfireDataFileValueType.Int, null); // N value
						}

						else // null value
						{
							WriteValue(tw, SpotfireDataFileValueType.String, null); // qualifier
							WriteValue(tw, SpotfireDataFileValueType.Double, null); // value

							if (outputNValues && nValueMetaColumns.Contains(mc))
								WriteValue(tw, SpotfireDataFileValueType.Int, null); // N value
						}
					}

					// Output a non-split (combined) QN

					else // combined
					{
						if (isNull) WriteValue(tw, SpotfireDataFileValueType.String, null);

						else if (vo is QualifiedNumber && !isNull) // regular QN
						{
							qn = (QualifiedNumber)vo;
							txt = qn.Format(qc, false, mc.Format, mc.Decimals, ep.QualifiedNumberSplit, false);
							WriteValue(tw, SpotfireDataFileValueType.String, txt);
						}

						else if (!isNull) // non-qn
						{
							txt = vo.ToString(); 
							WriteValue(tw, SpotfireDataFileValueType.String, txt);
						}

						else // null value
							WriteValue(tw, SpotfireDataFileValueType.String, null); 
					}
				}

				else // write other types as primitive value for now
				{
					vo2 = MobiusDataType.ConvertToPrimitiveValue(vo, mc);
					WriteValue(tw, sdfType, vo2);
				}
			} // col loop

			return;
		}

		/// <summary>
		/// Write out a single value
		/// </summary>
		/// <param name="tw"></param>
		/// <param name="type"></param>
		/// <param name="vo"></param>

		void WriteValue(
			SpotfireDataFileTableWriter tw,
			SpotfireDataFileValueType type,
			object vo)
		{
			if (tw.StdfTw != null)
			{
				if (type != null)
					StdfWriteValue(tw.StdfTw, type.StdfValueType, vo);

				else tw.StdfTw.WriteLine();
			}

			if (tw.SbdfTw != null)
			{
				if (type != null)
					SbdfAddValue(tw.SbdfTw, type.SbdfValueType, vo);

				else; // do nothing for binary mode
			}

			return;
		}

		/// <summary>
		/// Write value, efficiently casting to proper type as required by WriteValue
		/// </summary>
		/// <param name="tw"></param>
		/// <param name="vo"></param>
		/// <param name="typeId"></param>

		void StdfWriteValue(
			StdfTableWriter tw,
			StdfValueType type,
			object vo)
		{
			if (vo == null)
			{
				tw.WriteValue(null);
				StdfValueString += "null, ";
			}

			else
			{
				switch (type.TypeId)
				{
					case StdfValueTypeId.BoolType: tw.WriteValue((bool)vo); break;
					case StdfValueTypeId.IntType: tw.WriteValue((int)vo); break;
					case StdfValueTypeId.LongType: tw.WriteValue((long)vo); break;
					case StdfValueTypeId.FloatType: tw.WriteValue((float)vo); break;
					case StdfValueTypeId.DoubleType: tw.WriteValue((double)vo); break;
					case StdfValueTypeId.DateTimeType: tw.WriteValue((DateTime)vo); break;
					case StdfValueTypeId.DateType: tw.WriteValue((DateTime)vo); break;
					case StdfValueTypeId.StringType: tw.WriteValue((String)vo); break;
					case StdfValueTypeId.DecimalType: tw.WriteValue((Decimal)vo); break;

					case StdfValueTypeId.UnknownType:
					case StdfValueTypeId.TimeType:
					case StdfValueTypeId.TimeSpanType:
					case StdfValueTypeId.BinaryType:
					default:
						tw.WriteValue(null);
						break;
				}

				StdfValueString += vo.ToString() + "\r\n";
			}

			StdfWriteValueCount++;
			return;
		}

		static void SbdfAddValue(
			SbdfTableWriter bw,
			SbdfValueType type,
			object vo)
		{
			if (vo == null) bw.AddValue(type.InvalidValue); // correct way to write null

			else
			{
				switch (type.TypeId)
				{
					case SbdfValueTypeId.BoolType: bw.AddValue((bool)vo); break;
					case SbdfValueTypeId.IntType: bw.AddValue((int)vo); break;
					case SbdfValueTypeId.LongType: bw.AddValue((long)vo); break;
					case SbdfValueTypeId.FloatType: bw.AddValue((float)vo); break;
					case SbdfValueTypeId.DoubleType: bw.AddValue((double)vo); break;
					case SbdfValueTypeId.DateTimeType: bw.AddValue((DateTime)vo); break;
					case SbdfValueTypeId.DateType: bw.AddValue((DateTime)vo); break;
					case SbdfValueTypeId.StringType: bw.AddValue((String)vo); break;
					case SbdfValueTypeId.DecimalType: bw.AddValue((Decimal)vo); break;

					case SbdfValueTypeId.UnknownType:
					case SbdfValueTypeId.TimeType:
					case SbdfValueTypeId.TimeSpanType:
					case SbdfValueTypeId.BinaryType:
					default:
						bw.AddValue(type.InvalidValue);
						break;
				}
			}

			return;
		}

		/// <summary>
		/// Get molecule string in correct format
		/// </summary>
		/// <param name="vo"></param>
		/// <param name="structureExportFormat"></param>
		/// <returns></returns>

		string GetMoleculeString(
			object vo,
			ExportStructureFormat structureExportFormat)
		{
			string molString = "";
			MoleculeMx cs = (MoleculeMx)vo;

			if (structureExportFormat == ExportStructureFormat.Chime)
				molString = cs.GetChimeString(); // save structure as Chime

			else if (structureExportFormat == ExportStructureFormat.Smiles)
				molString = cs.GetSmilesString(); // save structure as Smiles

			else if (structureExportFormat == ExportStructureFormat.Molfile)
				molString = cs.GetMolfileString(); // save structure as molfile

			else // save structure as molfile as default
				molString = cs.GetMolfileString();

			return molString;
		}

		/// <summary>
		/// This example is a simple command line tool that writes a simple SBDF file
		/// with random data.
		/// </summary>

		public static void WriteSbdfFileExample(string[] args)
		{
			// The command line application requires one argument which is supposed to be
			// the name of the SBDF file to generate.
			if (args.Length != 1)
			{
				System.Console.WriteLine("Syntax: WriterSample outputfile.sbdf");
				return;
			}

			var outputFile = args[0];

			// First we just open the file as usual and then we need to wrap the stream
			// in a binary writer.
			using (var stream = File.OpenWrite(outputFile))
			using (var writer = new BinaryWriter(stream))
			{
				// When writing an SBDF file you first need to write the file header.
				SbdfFileHeader.WriteCurrentVersion(writer);

				// The second part of the SBDF file is the metadata, in order to create
				// the table metadata we need to use the builder class.
				var tableMetadataBuilder = new SbdfTableMetadataBuilder();

				// The table can have metadata properties defined. Here we add a custom
				// property indicating the producer of the file. This will be imported as
				// a table property in Spotfire.
				tableMetadataBuilder.AddProperty("GeneratedBy", "WriterSample.exe");

				// All columns in the table need to be defined and added to the metadata builder,
				// the required information is the name of the column and the data type.
				var col1 = new SbdfColumnMetadata("Category", SbdfValueType.String);
				tableMetadataBuilder.AddColumn(col1);

				// Similar to tables, columns can also have metadata properties defined. Here
				// we add another custom property. This will be imported as a column property
				// in Spotfire.
				col1.AddProperty("SampleProperty", "col1");

				var col2 = new SbdfColumnMetadata("Value", SbdfValueType.Double);
				tableMetadataBuilder.AddColumn(col2);
				col2.AddProperty("SampleProperty", "col2");

				var col3 = new SbdfColumnMetadata("TimeStamp", SbdfValueType.DateTime);
				tableMetadataBuilder.AddColumn(col3);
				col3.AddProperty("SampleProperty", "col3");

				// We need to call the build function in order to get an object that we can
				// write to the file.
				var tableMetadata = tableMetadataBuilder.Build();
				tableMetadata.Write(writer);

				const int rowCount = 10000;
				var random = new Random((int)DateTime.Now.Ticks);

				// Now that we have written all the metadata we can start writing the actual data.
				// Here we use a SbdfTableWriter to write the data, remember to dispose the table writer
				// otherwise you will not generate a correct SBDF file.
				using (var tableWriter = new SbdfTableWriter(writer, tableMetadata))
				{
					for (int i = 0; i < rowCount; ++i)
					{
						// You need to perform one AddValue call for each column, for each row in the
						// same order as you added the columns to the table metadata object.
						// In this example we just generate some random values of the appropriate types.
						// Here we write the first string column.
						var col1Values = new[] { "A", "B", "C", "D", "E" };
						tableWriter.AddValue(col1Values[random.Next(0, 5)]);

						// Next we write the second double column.
						var doubleValue = random.NextDouble();
						if (doubleValue < 0.5)
						{
							// Note that if you want to write a null value you shouldn't send null to
							// AddValue, instead you should use the InvalidValue property of the columns
							// SbdfValueType.
							tableWriter.AddValue(SbdfValueType.Double.InvalidValue);
						}
						else
						{
							tableWriter.AddValue(random.NextDouble());
						}

						// And finally the third date time column.
						tableWriter.AddValue(DateTime.Now);
					}
				}
			}

			System.Console.WriteLine("Wrote file: " + outputFile);
		}

		/// <summary>
		/// Get the lowest available Spotfire export table index available
		/// </summary>
		/// <param name="rf"></param>
		/// <returns></returns>

		static int GetFreeSfxTableId(
			QueryResultsVoMap voMap)

		{
			HashSet<int> inUse = new HashSet<int>();

			foreach (QueryTableVoMap qtm in voMap.Tables)
			{
				QueryTable qt = qtm.Table;
				int si = qt.SpotfireExportPos;
				inUse.Add(si);
			}

			for (int free = 1; ; free++) // find free entry
			{
				if (!inUse.Contains(free)) return free;
			}
		}

		/// <summary>
		/// Get max spotfire export column index in use
		/// </summary>
		/// <param name="qt"></param>
		/// <returns></returns>

		static int GetMaxSfxTableColIdAssigned(QueryTable qt) // next generalized column index to use in name
		{
			int max = -1;
			foreach (QueryColumn qc in qt.QueryColumns)
			{
				int si = qc.SpotfireExportPos;
				if (si > max)
				{
					max = si;
					if (qc.DataType == MetaColumnType.QualifiedNo) // add extra cols for QNs
						max += 2; // allow for qualifier and number tested cols
				}
			}

			return max;

		}

		/// <summary>
		/// Get Spotfire Stdf (Text Data Format) column type
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>
		StdfValueType GetSpotfireStdfType(MetaColumn mc)
		{
			StdfValueType tt;
			SbdfValueType bt;

			GetSpotfireDataTypes(mc, out tt, out bt);
			return tt;
		}

		/// <summary>
		/// Get Spotfire SBdf (Binary Data Format) column type
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>
		SbdfValueType GetSpotfireSbdfType(MetaColumn mc)
		{
			StdfValueType tt;
			SbdfValueType bt;

			GetSpotfireDataTypes(mc, out tt, out bt);
			return bt;
		}

		/// <summary>
		/// Get Spotfire SBdf (Binary Data Format) column type
		/// </summary>
		/// <param name="mc"></param>
		/// <returns></returns>
		SpotfireDataFileValueType GetSpotfireDataFileType(MetaColumn mc)
		{
			StdfValueType tt;
			SbdfValueType bt;

			GetSpotfireDataTypes(mc, out tt, out bt);

			SpotfireDataFileValueType vt = new SpotfireDataFileValueType();
			if (TextFormat)
				vt.StdfValueType = tt;
			else vt.SbdfValueType = bt;

			return vt;
		}


		/// <summary>
		/// Get the Spotfire data types corresponding to a Mobius data type for
		/// DataColumns and Text and Binary file readers/writers
		/// </summary>
		/// <param name="mc"></param>
		/// <param name="tt"></param>
		/// <param name="bt"></param>

		void GetSpotfireDataTypes(
			MetaColumn mc,
			out StdfValueType tt,
			out SbdfValueType bt)
		{
			//out Spotfire.Dxp.Data.DataType dt, // dt = null;

			tt = null;
			bt = null;

			switch (mc.DataType)
			{
				case MetaColumnType.CompoundId:
					if (mc.StorageType == MetaColumnStorageType.String)
					{
						//dt = DataType.String;
						tt = StdfValueType.String;
						bt = SbdfValueType.String;
					}
					else
					{
						//dt = DataType.Integer;
						tt = StdfValueType.Int;
						bt = SbdfValueType.Int;
					}
					break;

				case MetaColumnType.Structure: // pass structures and Smiles or Chime
																			 //dt = DataType.String;
					tt = StdfValueType.String;
					bt = SbdfValueType.String;
					break;

				case MetaColumnType.Integer:
					//dt = DataType.Integer;
					tt = StdfValueType.Int;
					bt = SbdfValueType.Int;
					break;

				case MetaColumnType.Number:
					//dt = DataType.Real;
					tt = StdfValueType.Double;
					bt = SbdfValueType.Double;
					break;

				case MetaColumnType.QualifiedNo:
					//dt = DataType.Real;
					tt = StdfValueType.Double;
					bt = SbdfValueType.Double;
					break;

				case MetaColumnType.Date:
					//dt = DataType.Date;
					tt = StdfValueType.Date;
					bt = SbdfValueType.Date;
					break;

				default:
					//dt = DataType.String;
					tt = StdfValueType.String;
					bt = SbdfValueType.String;
					break;
			}

			return;
		}

		class SpotfireDataFileMetadataBuilder
		{
			public SpotfireDataFileMetadataBuilder(SpotfireFileFormat sff)
			{
				if (sff == SpotfireFileFormat.Text)
					stdfMdb = new StdfTableMetadataBuilder();

				else sbdfMdb = new SbdfTableMetadataBuilder();
			}

			public StdfTableMetadataBuilder stdfMdb = null;
			public SbdfTableMetadataBuilder sbdfMdb = null;

			public void AddColumn(string columnName, SpotfireDataFileValueType dataType)
			{
				if (stdfMdb != null) stdfMdb.AddColumn((new StdfColumnMetadata(columnName, dataType.StdfValueType)));
				if (sbdfMdb != null) sbdfMdb.AddColumn((new SbdfColumnMetadata(columnName, dataType.SbdfValueType)));
			}

			public SpotfireDataFileTableMetadata Build()
			{
				SpotfireDataFileTableMetadata md = new SpotfireDataFileTableMetadata();
				if (stdfMdb != null) md.StdfMd = stdfMdb.Build();
				if (sbdfMdb != null) md.SbdfMd = sbdfMdb.Build();
				return md;
			}
		}

		class SpotfireDataFileTableMetadata
		{
			public StdfTableMetadata StdfMd;
			public SbdfTableMetadata SbdfMd;
		}

		class SpotfireDataFileTableWriter
		{
			public StdfTableWriter StdfTw = null;
			public SbdfTableWriter SbdfTw = null;

			public FileStream TextStream;
			public FileStream BinaryStream;
			public BinaryWriter BinaryWriter;

			public SpotfireDataFileTableWriter(string outputFile, SpotfireDataFileTableMetadata metadata)
			{
				if (metadata.StdfMd != null)
				{
					TextStream = File.OpenWrite(outputFile);
					StdfTw = new StdfTableWriter(TextStream, metadata.StdfMd); // open the text writer and write the metadata
				}

				if (metadata.SbdfMd != null)
				{
					BinaryStream = File.OpenWrite(outputFile);
					BinaryWriter = new BinaryWriter(BinaryStream);

					SbdfFileHeader.WriteCurrentVersion(BinaryWriter); // write file header
					metadata.SbdfMd.Write(BinaryWriter); // write the metadata

					SbdfTw = new SbdfTableWriter(BinaryWriter, metadata.SbdfMd);

				}

				return;
			}

			public void Close()
			{
				if (StdfTw != null)
				{
					StdfTw.Flush();
					StdfTw.Close();
					TextStream.Close();
				}

				if (SbdfTw != null)
				{
					SbdfTw.Dispose();
					BinaryWriter.Flush();
					BinaryWriter.Close();
					BinaryStream.Close();
				}

			}

		}

		/// <summary>
		/// Set the file data types for a source type for each export format
		/// </summary>

		class SpotfireDataFileValueType
		{

			public SpotfireDataFileValueType()
			{
				return;
			}

			SpotfireDataFileValueType(int typeId)
			{
				if (typeId == 1)
				{
					StdfValueType = StdfValueType.String;
					SbdfValueType = SbdfValueType.String;
				}

				else if (typeId == 2)
				{
					StdfValueType = StdfValueType.Int;
					SbdfValueType = SbdfValueType.Int;
				}

				if (typeId == 3)
				{
					StdfValueType = StdfValueType.Double;
					SbdfValueType = SbdfValueType.Double;
				}
			}

			public StdfValueType StdfValueType = null;
			public SbdfValueType SbdfValueType = null;

			public static SpotfireDataFileValueType String = new SpotfireDataFileValueType(1);
			public static SpotfireDataFileValueType Int = new SpotfireDataFileValueType(2);
			public static SpotfireDataFileValueType Double = new SpotfireDataFileValueType(3);
		}
	} //

		public enum SpotfireFileFormat
		{
			Undefined = 0,
			Text = 1,
			Binary = 2
		}

	}
