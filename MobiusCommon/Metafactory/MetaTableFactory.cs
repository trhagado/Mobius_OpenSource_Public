using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.QueryEngineLibrary;

using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{

	/// <summary>
	/// MetaTable factory that builds MetaTables from Xml files and datasource-specific factories
	/// </summary>

	public class MetaTableFactory : IMetaTableFactory
	{
		public static MetaTableFactory Instance; // only or most-recently allocated instance
		public static string MetaTableXmlFolder; // folder that contains xml for tables
		public static Dictionary<string, MetaTableStats> TableStats = null; // map of table stats by name
		public static bool ShowDataSource = true; // show data source as part of contents/metatable name
		public static bool IncludeQualifiedNumberDetailColumnsInMetaTables = true; // flag for including hidden Qualified Number detail cols in metatables
		public static List<MetaTableFactoryRef> MetaFactories = new List<MetaTableFactoryRef>(); // List of factories
		static Dictionary<string, ColumnSelectionEnum> DisplayLevelOverride; // override values for display levels from metafactories

		public MetaTableFactory()
		{
			Instance = this;
		}

		public static void Initialize()
		{
			MetaTableXmlFolder = ServicesDirs.MetaDataDir + @"\MetaTables";
			string rootFile = ServicesIniFile.Read("MetaTableRootFile", "Metatables.xml"); // root to start at
			if (rootFile.IndexOf(@"\") < 0) rootFile = MetaTableXmlFolder + @"\" + rootFile;

			IncludeQualifiedNumberDetailColumnsInMetaTables = // get flag for including hidden Qualified Number detail cols in metatables
					ServicesIniFile.ReadBool("IncludeQualifiedNumberDetailColumnsInMetaTables", true);

			BuildFromXmlFile(rootFile); // get initial metatables

			RegisterFactory("CalcField", new CalcFieldMetaFactory());
			RegisterFactory("Annotation", new AnnotationMetaFactory());
			RegisterFactory("MultiTable", new MultiTableMetaFactory());
			RegisterFactory("Oracle", new OracleMetaFactory());
			RegisterFactory("PubChem", new PubChemMetaFactory());
			RegisterFactory("UnpivotedAssay", new UnpivotedAssayMetaFactory());
			RegisterFactory("SpotfireLink", new SpotfireLinkMetafactory());
		}

		/// <summary>
		/// Reset metatable data
		/// </summary>

		public static void Reset()
		{
			MetaFactories = new List<MetaTableFactoryRef>();
			MetaTableCollection.Reset();
			return;
		}

		/// <summary>
		/// Register a new factory
		/// </summary>
		/// <param name="name"></param>
		/// <param name="factory"></param>

		public static void RegisterFactory(
				string name,
				IMetaFactory factory)
		{

			MetaTableFactoryRef mtfr = new MetaTableFactoryRef();
			mtfr.Name = name;
			mtfr.MetaTableFactory = factory;
			MetaFactories.Add(mtfr);
			return;
		}

		/// <summary>
		/// Load/reload from specified file
		/// </summary>

		public void BuildFromFile(string fileName)
		{
			BuildFromXmlFile(fileName);
			return;
		}

		/// <summary>
		/// Do initial load of metatables
		/// </summary>

		public static void BuildFromXmlFile(string rootFile)
		{
			MetaTable[] mta;
			StreamReader sr = null;
			int i1;

			try
			{
				string dir = Path.GetDirectoryName(rootFile);
				if (!String.IsNullOrEmpty(dir)) MetaTableXmlFolder = dir;

				sr = new StreamReader(rootFile);
				XmlTextReader tr = new XmlTextReader(sr);
				mta = MetaTable.DeserializeMetaTables(tr, dir, true);
				sr.Close();
			}
			catch (Exception ex)
			{
				if (sr != null) sr.Close();
				throw new Exception("Error processing metatable file " + Lex.Dq(rootFile) + ": " + ex.Message, ex);
			}
		}

		/// <summary>
		/// Lookup a MetaTable by name throwing any exceptions from underlying factories
		/// </summary>
		/// <param name="name"></param>
		/// <returns>MetaTable or null if not found</returns>
		/// 

		public MetaTable GetMetaTable(
				String name)
		{
			MetaTable mt;

			if (RestrictedMetaTables.MetatableIsRestricted(name)) return null; 

            if (RestrictedMetatable.MetatableIsGenerallyRestricted(name)) return null;

			name = name.Trim().ToUpper();

			if (MetaTableCollection.TableMap.ContainsKey(name))
			{ // see if in collection already
				mt = MetaTableCollection.TableMap[name];
				return mt;
			}

			for (int i1 = 0; i1 < MetaFactories.Count; i1++)
			{
				MetaTableFactoryRef mtfr = MetaFactories[i1];
				mt = mtfr.MetaTableFactory.GetMetaTable(name);

				if (mt != null)
				{
					NormalizeMetaTable(mt);
					return mt;
				}
			}

			// Check to see if this is a summary table that can be created from an unsummarized version of itself

			if (!Lex.EndsWith(name, MetaTable.SummarySuffix)) return null; // see if named as summary table

			string name2 = name.Substring(0, name.Length - MetaTable.SummarySuffix.Length);
			MetaTable mt2 = MetaTableCollection.Get(name2);
			if (mt2 == null || !mt2.SummarizedExists) return null;
			mt = mt2.Clone();
			AdjustForSummarization(mt, true);
			return mt;
		}

		/// <summary>
		/// Adjust metatable to contain proper summarized or unsummarized columns
		/// </summary>
		/// <param name="mt"></param>
		/// <param name="summarized"></param>

		public static void AdjustForSummarization(
				MetaTable mt,
				bool summary)
		{
			mt.UseSummarizedData = summary;

			if (summary)
			{
				if (!Lex.EndsWith(mt.Name, MetaTable.SummarySuffix))
					mt.Name += MetaTable.SummarySuffix; // be sure name indicates summary

				if (!Lex.EndsWith(mt.Label, " Summary"))
					mt.Label += " Summary"; // be sure label indicates summary
			}

			int mci = 0;
			while (mci < mt.MetaColumns.Count)
			{
				MetaColumn mc = mt.MetaColumns[mci];
				//if (mc.ResultCode == "103468") mc = mc; // debug

				if (summary)
				{
					if (mc.SummarizedExists) mci++;
					else mt.MetaColumns.RemoveAt(mci);
				}

				else
				{
					if (mc.UnsummarizedExists) mci++;
					else mt.MetaColumns.RemoveAt(mci);
				}
			}

			return;
		}

		/// <summary>
		/// Return stats for a metatable
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public static MetaTableStats GetStats(
				string mtName)
		{
			if (TableStats == null)
			{
				LoadMetaTableStats();
				if (TableStats == null) return null;
			}

			if (TableStats.ContainsKey(mtName.ToUpper()))
				return TableStats[mtName.ToUpper()];
			else return null;
		}

		/// <summary>
		/// Set any display level override values
		/// </summary>
		/// <param name="mt"></param>

		static void NormalizeMetaTable(
				MetaTable mt)
		{
			string tableColumn;

			if (DisplayLevelOverride == null)
			{ // read in data if necessary
				DisplayLevelOverride = new Dictionary<string, ColumnSelectionEnum>();
				string fileName = ServicesDirs.MetaDataDir + @"\Metatables\DisplayLevelOverride.txt";
				StreamReader sr;
				try { sr = new StreamReader(fileName); }
				catch (Exception ex)
				{ return; }

				while (true)
				{ // parse lines of format: <tableName>.<columnName> <selectLevel>
					string txt = sr.ReadLine();
					if (txt == null) break;
					if (txt.StartsWith(";")) continue;
					int i1 = txt.IndexOf(" ");
					if (i1 <= 0) continue;
					tableColumn = txt.Substring(0, i1).Trim().ToUpper();
					ColumnSelectionEnum dl;
					txt = txt.Substring(i1 + 1).Trim();
					if (Lex.Eq(txt, "Selected") || Lex.Eq(txt, "Default") || Lex.Eq(txt, "SelectedByDefault"))
						dl = ColumnSelectionEnum.Selected;
					else if (Lex.Eq(txt, "Selectable")) dl = ColumnSelectionEnum.Unselected;
					else if (Lex.Eq(txt, "Hidden")) dl = ColumnSelectionEnum.Hidden;
					else continue; // not recognized
					DisplayLevelOverride[tableColumn] = dl;
				}

				sr.Close();
			}

			foreach (MetaColumn mc in mt.MetaColumns)
			{
				tableColumn = mc.MetaTable.Name.ToUpper() + "." + mc.Name.ToUpper();
				if (DisplayLevelOverride.ContainsKey(tableColumn))
					mc.InitialSelection = DisplayLevelOverride[tableColumn];

				if (mc.ColumnMap == null || mc.ColumnMap == "")
					mc.ColumnMap = mc.Name; // define map column same as name if not defined
			}

			return;
		}

		/// <summary>
		/// Calculate metatable statistics for each broker type
		/// </summary>
		/// <param name="factoryName"></param>
		/// <returns></returns>

		public static int UpdateStats(
				string factoryName)
		{
			int total = 0;

			if (Lex.Eq(factoryName, "Generic") || Lex.IsNullOrEmpty(factoryName))
			{
				if (Instance == null) Instance = new MetaTableFactory();
				total += Instance.UpdateMetaTableStatistics();
			}

			for (int i1 = 0; i1 < MetaFactories.Count; i1++)
			{
				MetaTableFactoryRef mtfr = MetaFactories[i1];

				if (!String.IsNullOrEmpty(factoryName) && Lex.Ne(mtfr.Name, factoryName))
					continue; // skip if name supplied and this isn't it
				int cnt = mtfr.MetaTableFactory.UpdateMetaTableStatistics();
				total += cnt;
			}

			return total;
		}

/// <summary>
/// Utility method to write out a metatable stats file for a broker
/// </summary>
/// <param name="stats"></param>
/// <param name="fileName"></param>
/// <returns></returns>

		public static int WriteMetaTableStats(
				Dictionary<string, MetaTableStats> stats,
				string fileName)
		{
			return WriteMetaTableStats(stats, null, fileName);
		}

		/// <summary>
		/// Utility method to write out a metatable stats file for a broker
		/// </summary>
		/// <param name="stats"></param>
		/// <param name="arg"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>

		public static int WriteMetaTableStats(
			Dictionary<string, MetaTableStats> stats,
			string singleMtName,
			string fileName)
		{
			string path = fileName + ".new";
			StreamWriter sw = new StreamWriter(path);

			if (!Lex.IsNullOrEmpty(singleMtName)) // if mtName specified read & write all data but this metatable
			{
				path = fileName + ".txt";
				StreamReader sr = new StreamReader(path); // open existing file
				while (true)
				{
					string rec = sr.ReadLine();
					if (rec == null) break;
					if (rec.StartsWith(singleMtName + "\t")) continue; // skip existing recs

					sw.WriteLine(rec); // write all other recs
				}
				sr.Close();
			}

			foreach (string mtName0 in stats.Keys)
			{
				MetaTableStats mts = stats[mtName0];
				string rec = mtName0 + "\t" + mts.RowCount.ToString() + "\t" + mts.UpdateDateTime.ToShortDateString();
				sw.WriteLine(rec);
			}
			sw.Close();

			FileUtil.BackupAndReplaceFile(fileName + ".txt", fileName + ".bak", fileName + ".new");

			return stats.Count;
		}

		/// <summary>
		/// Load metatable stats for tables associated with each factory
		/// </summary>
		/// <returns></returns>

		public static int LoadMetaTableStats()
		{
			int cnt;

			TableStats = new Dictionary<string, MetaTableStats>();

			int total = 0;
			for (int i1 = 0; i1 < MetaFactories.Count; i1++)
			{
				MetaTableFactoryRef mtfr = MetaFactories[i1];
				cnt = mtfr.MetaTableFactory.LoadMetaTableStats(TableStats);
				total += cnt;
			}

			// Load generic persisted metatable stats

			string fileName = MetaTableXmlFolder + @"\GenericMetaTableStats.txt";
			cnt = LoadMetaTableStats(fileName, TableStats);
			total += cnt;

			return total;
		}

		/// <summary>
		/// Utility routine to load a file of stats for a single broker type
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public static int LoadMetaTableStats(
				string fileName,
				Dictionary<string, MetaTableStats> metaTableStats)
		{
			if (!File.Exists(fileName)) return 0;
			StreamReader sr = new StreamReader(fileName);
			int cnt = 0;
			while (true)
			{
				string rec = sr.ReadLine();
				if (Lex.IsUndefined(rec)) break;
				string[] sa = rec.Split('\t');
				if (sa.Length < 3) continue; 
				string table = sa[0].ToUpper();
				MetaTableStats mts = new MetaTableStats();
				mts.RowCount = long.Parse(sa[1]);
				mts.UpdateDateTime = DateTimeUS.ParseDate(sa[2]);

				metaTableStats[table] = mts;

				cnt++;
			}

			sr.Close();
			return cnt;
		}

		/// <summary>
		/// Get the description or a link to the description for a table
		/// </summary>
		/// <param name="mtName"></param>
		/// <returns></returns>

		public TableDescription GetDescription(string mtName)
		{
			MetaTable mt = GetMetaTable(mtName);
			if (mt == null) return null;

			if (!MetaBrokerUtil.GlobalBrokerExists(mt.MetaBrokerType)) return null;

			TableDescription td = MetaBrokerUtil.GlobalBrokers[(int)mt.MetaBrokerType].GetTableDescription(mt);
			return td;
		}

		/// <summary>
		/// Assign any new label for metatable
		/// </summary>
		/// <param name="mt"></param>

		public static void SetAnyNewMetaTableLabel(
				MetaTable mt)
		{
			DictionaryMx newDict = DictionaryMx.Get("NewNameDict");
			if (newDict == null) return;
			string newLabel = newDict.LookupDefinition(mt.Name);
			if (newLabel == null) return;

			DictionaryMx originalDict = DictionaryMx.Get("OriginalNameDict");
			if (originalDict == null) return;
			originalDict.Add(mt.Name, mt.Label); // save original label
			mt.Label = newLabel;
		}

		/// <summary>
		/// Set any new label for a metacolumn
		/// </summary>
		/// <param name="mc"></param>

		public static void SetAnyNewMetaColumnLabel(
				MetaColumn mc)
		{
			string name = mc.MetaTable.Name + "." + mc.Name;
			DictionaryMx newDict = DictionaryMx.Get("NewNameDict");
			if (newDict == null) return;
			string newLabel = newDict.LookupDefinition(name);
			if (newLabel == null) return;

			DictionaryMx originalDict = DictionaryMx.Get("OriginalNameDict");
			if (originalDict == null) return;
			originalDict.Add(mc.MetaTable.Name + "." + mc.Name, mc.Label); // save original label

			mc.Label = newLabel;
			mc.Units = ""; // prevent addition of units
		}

		/// <summary>
		/// Set data source name in metatable label
		/// </summary>
		/// <param name="showSource"></param>

		public static void ShowHideDataSource(
				bool showSource)
		{
			int t0 = TimeOfDay.Milliseconds();

			foreach (MetaTable mt in MetaTableCollection.TableMap.Values)
			{
				if (showSource) mt.Label = AddSourceToLabel(mt.Name, mt.Label);
				else mt.Label = RemoveSourceFromLabel(mt.Name, mt.Label);
			}

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

		/// <summary>
		/// Add data source info to a table/node label
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="label"></param>
		/// <returns></returns>

		public static string AddSourceToLabel(
				string tableName,
				string label)
		{
			string source = GetSourceFromTableName(tableName);
			if (source == null) return label;

			if (Lex.Contains(label, source)) return label; // see if already in label
			return label.Trim() + " (" + source + ")";
		}

		/// <summary>
		/// Remove data source name from label
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="label"></param>
		/// <returns></returns>

		public static string RemoveSourceFromLabel(
				string tableName,
				string label)
		{
			string source = GetSourceFromTableName(tableName);
			if (source == null) return label;

			source = " (" + source + ")";
			if (!Lex.Contains(label, source)) return label;

			return label.Replace(source, "");
		}

		/// <summary>
		/// Get the string describing the source for a metatable
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>

		public static string GetSourceFromTableName(
				string tableName)
		{
			string sourceString;

			//if (Lex.Eq(tableName, "ASSAY_1234")) tableName = tableName; // debug

			try
			{
				tableName = tableName.ToUpper();
				if (tableName.StartsWith("ASSAY_")) sourceString = "Assay";
				else if (tableName.StartsWith("ANNOTATION_")) sourceString = "Note";
				else if (tableName.StartsWith("CALCFIELD_")) sourceString = "Calc";

				else return null;

				return sourceString;
			}

			catch (Exception ex)
			{ // log & ignore exception since not critical
				DebugLog.Message(DebugLog.FormatExceptionMessage(ex));
				return "";
			}
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			Dictionary<string, MetaTableStats> stats = new Dictionary<string, MetaTableStats>();

			//return stats.Count; // debug

			foreach (MetaTable mt in MetaTableCollection.TableMap.Values)
			{
				if (String.IsNullOrEmpty(mt.RowCountSql) && String.IsNullOrEmpty(mt.UpdateDateTimeSql))
					continue; // if no sql defined for stats then skip

				string mtName = mt.Name.ToUpper();

				// Get row count

				string rowCountSql = null;

				if (Lex.Eq(mt.RowCountSql, "TableMap")) // use native source
					rowCountSql = "select count(*) from " + mt.TableMap;

				else if (mt.RowCountSql != "") // use table-specific sql
					rowCountSql = mt.RowCountSql;

				if (rowCountSql != null)
				{
					try
					{
						int rowCount = SelectSingleValueDao.SelectInt(rowCountSql);
						if (!stats.ContainsKey(mtName)) stats[mtName] = new MetaTableStats();
						stats[mtName].RowCount = rowCount;
					}
					catch (Exception ex) { continue; }
				}

				// Get date

				string dateSql = null;

				if (Lex.Eq(mt.UpdateDateTimeSql, "TableMap")) // use native source
				{
					for (int mci = mt.MetaColumns.Count - 1; mci >= 0; mci--)
					{ // search backwards for date col
						MetaColumn mc = mt.MetaColumns[mci];
						if (mc.DataType == MetaColumnType.Date)
						{
							dateSql = "select max(" + mc.Name + ") from " + mt.TableMap +
									" where " + mc.Name + " <= current_date";
							break;
						}
					}
				}

				else if (mt.UpdateDateTimeSql != "") // use table-specific sql
					dateSql = mt.UpdateDateTimeSql;


				if (dateSql != null)
				{
					try
					{
						if (!stats.ContainsKey(mtName)) stats[mtName] = new MetaTableStats();
						DateTime dt = SelectSingleValueDao.SelectDateTime(dateSql);
						stats[mtName].UpdateDateTime = dt;
					}
					catch (Exception ex) { continue; }
				}
			}

			string fileName = MetaTableXmlFolder + @"\GenericMetaTableStats";
			WriteMetaTableStats(stats, fileName);
			return stats.Count;
		}

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public int LoadMetaTableStats(
				Dictionary<string, MetaTableStats> metaTableStats)
		{
			string fileName = MetaTableXmlFolder + @"\GenericMetaTableStats.txt";
			int cnt = LoadMetaTableStats(fileName, metaTableStats);
			return cnt;
		}

		/// <summary>
		/// Add a metatable to the services side from the client 
		/// </summary>
		/// <param name="mt"></param>

		public void AddServiceMetaTable(MetaTable mt)
		{
			MetaTableCollection.Add(mt);
			MetaTableCollection.AddUserObjectTable(mt.Name);
		}

		/// <summary>
		/// Remove a metatable on the services side from the client
		/// </summary>
		/// <param name="mt"></param>

		public void RemoveServiceMetaTable(string mtName)
		{
			MetaTableCollection.Remove(mtName);
			MetaTableCollection.RemoveUserObjectTable(mtName);
		}

		/// <summary>
		/// Get the labels of the root set of tables
		/// </summary>
		/// <returns></returns>

		public static List<string> GetRootTableLabels()
		{
			List<string> labels = new List<string>();
			// todo
			return labels;
		}
	}

	/// <summary>
	/// Reference to a metatable factory instance
	/// </summary>

	public class MetaTableFactoryRef
	{
		public string Name; // broker name
		public IMetaFactory MetaTableFactory; // metatable factory broker
	}


}
