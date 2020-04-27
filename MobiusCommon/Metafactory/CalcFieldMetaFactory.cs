using Mobius.UAL;
using Mobius.ComOps;
using Mobius.Data;
using Mobius.QueryEngineLibrary;

using System;
using System.Collections.Generic;

namespace Mobius.MetaFactoryNamespace
{
	/// <summary>
	/// Build calculated field metatables
	/// </summary>
	/// 
	public class CalcFieldMetaFactory : IMetaFactory
	{
		int RecursionDepth = 0;

		public CalcFieldMetaFactory()
		{
			return;
		}

		public MetaTable GetMetaTable (
			string name)
		{
			MetaTable mt = null;

			if (RecursionDepth > 4) return null; // avoid infinite depth circular references
			RecursionDepth++;
			try { mt = GetMetaTableLevel2(name); }
			catch (Exception ex) { string msg = ex.Message; }
			RecursionDepth--;
			return mt;
		}

		private MetaTable GetMetaTableLevel2(
			string name)
		{
			UserObject uo;
			Query incQry = null; // included query
			QueryTable qt, qt1, qt2;
			QueryColumn qc, qc1, qc2;
			CalcFieldMetaTable mt;
			MetaTable mt1, mt2;
			MetaColumn mc, mc1, mc2, cfMc, cfMcModel;
			ExecuteQueryParms eqp;
			string cfLabel = "", mcName, mcLabel;
			string sql = null, tables = null, exprs = null, criteria = null;
			int objectId, startingSuffix, ci, i1;

			string prefix = "calcfield_"; // calculated field metatable names begin with "calcfield_"
			if (name.ToLower().IndexOf(prefix) != 0) return null;

			if (Lex.Eq(name, "CALCFIELD_826476")) name = name; // debug

			string tok = name.Substring(prefix.Length); // get the object id
			try { objectId = Int32.Parse(tok); }
			catch (Exception ex) { return null; }

			uo = UserObjectDao.Read(objectId);
			if (uo==null) return null;

			if (!Permissions.UserHasReadAccess(Security.UserName, uo)) return null; 

			CalcField cf = CalcField.Deserialize(uo.Content);
			if (cf==null) return null;
			if (cf.FinalResultType == MetaColumnType.Unknown) return null; // need a final result type

			mt = new CalcFieldMetaTable();
			mt.Name = name;
			mt.CalcField = cf; // include associated CalcField object

			mt.Label = uo.Name;
			if (MetaTableFactory.ShowDataSource) // add source to label if requested
				mt.Label = MetaTableFactory.AddSourceToLabel(mt.Name,mt.Label);

			mt.Description = cf.Description;

			mt.MetaBrokerType = MetaBrokerType.CalcField;

			bool keyAdded = false;

			cfLabel = cf.ColumnLabel; // get any user defined label for result

// Add metacolumns for basic calculated field

			if (cf.CalcType == CalcTypeEnum.Basic && cf.Column1.MetaColumn != null)
			{
				cfMcModel = cf.Column1.MetaColumn;

				tok = cf.Operation;
				if (Lex.IsNullOrEmpty(cfLabel))
				{
					if (tok.IndexOf("/") == 0) cfLabel = "Ratio";
					else if (tok.IndexOf("*") == 0) cfLabel = "Product";
					else if (tok.IndexOf("+") == 0) cfLabel = "Sum";
					else if (tok.IndexOf("-") == 0) cfLabel = "Difference";
					else
					{
						if (cf.Column1.FunctionEnum == CalcFuncEnum.None) // just use existing col name
							cfLabel = cf.Column1.MetaColumn.Label;

						else // use function & col name
						{
							tok = cf.Column1.Function;
							i1 = tok.IndexOf("(");
							if (i1 >= 0) tok = tok.Substring(0, i1);
							if (!tok.EndsWith(" ")) tok += " ";
							tok += cf.MetaColumn1.Label;
							cfLabel = tok;
						}
					}
					if (cf.IsClassificationDefined) cfLabel += " Class"; // add class suffix if classifier
				}
			}

			else if (cf.CalcType == CalcTypeEnum.Advanced && cf.AdvancedExpr != "")
			{
				List<MetaColumn> mcList = cf.GetInputMetaColumnList();
				if (mcList.Count == 0) return null;
				cfMcModel = mcList[0];
				if (cfLabel == "") cfLabel = "Value";
			}

			else return null;

			mt.Parent = cfMcModel.MetaTable.Parent; // set parent
			if (mt.Root == mt) mt.Parent = cfMcModel.MetaTable; // if F1 table is a root set it as parent

			// Add key value

			mc = cfMcModel.MetaTable.KeyMetaColumn; // make copy of first table key field for our key
			if (mc == null) return null;
			mc = mc.Clone();
			mc.ColumnMap = mc.Name; // map to self
			mc.MetaTable = mt;
			mt.AddMetaColumn(mc);
			keyAdded = true;
			mc.IsSearchable = cf.IsSearchable;

			// Add calculated value column

			mc = new MetaColumn();
			mt.AddMetaColumn(mc);
			mc.MetaTable = mt;
			mc.Name = "CALC_FIELD";
			mc.Label = cfLabel;

			mc.DataType = cf.FinalResultType; // final result type

			mc.InitialSelection = ColumnSelectionEnum.Selected;
			mc.Format = cfMcModel.Format;
			mc.Width = cfMcModel.Width;
			mc.Decimals = cfMcModel.Decimals;

			if (cf.FinalResultType == MetaColumnType.Integer)
			{
				mc.Format = ColumnFormatEnum.Decimal;
				mc.Decimals = 0;
			}

			if (cf.FinalResultType == MetaColumnType.Image)
				mc.Width = (int)(mc.Width * 2.0); // make CF images wider

			mc.IsSearchable = cfMcModel.IsSearchable;
			if (mc.IsSearchable) // refine searchability
				mc.IsSearchable = cf.IsSearchable;

			cfMc = mc;

			// Add metacolumns for the underlying columns that go into the calculation

			foreach (CalcFieldColumn cfc in cf.CfCols)
			{
				if (cfc.MetaColumn == null) continue;

				if (cfc.MetaColumn.IsKey) continue; // don't add additional key

				mc = cfc.MetaColumn.Clone();
				mcName = mc.MetaTable.Name + "_" + mc.Name;
				mc.Name = mt.GetValidMetaColumnName(mcName);
				mc.DetailsAvailable = false; // no drill-down for now (can cause issues)
				mc.IsSearchable = false; // no searching for now (can cause issues)
				mc.ColumnMap = mc.Name; // map to self

				if (Lex.Eq(mc.Label, cfLabel)) // if same as result start suffix checking at 2
					startingSuffix = 1;
				else startingSuffix = 2;
				mc.Label = mt.GetUniqueMetaColumnLabel(mc.Label, startingSuffix); 

				mc.InitialSelection = ColumnSelectionEnum.Hidden; // hide for now

				mc.TableMap = cfc.MetaColumn.MetaTable.Name + "." + cfc.MetaColumn.Name; // save source table and column
				//mc.TableMap = cfc.MetaColumn.MetaTable.Name; // save source table and column
				//mc.ColumnMap = cfc.MetaColumn.Name;

				mt.AddMetaColumn(mc);
			}

			//mt.MetaColumns.Remove(cfMc); // move cf result column to end after the inputs
			//mt.MetaColumns.Add(cfMc);

 			return mt;
		}

		/// <summary>
		/// Calculate & persist metatable stats for tables belonging to broker
		/// </summary>
		/// <returns></returns>

		public int UpdateMetaTableStatistics()
		{
			return 0;
		}

		/// <summary>
		/// Load persisted metatable stats
		/// </summary>
		/// <param name="metaTableStats"></param>
		/// <returns></returns>

		public int LoadMetaTableStats(
			Dictionary<string, MetaTableStats> metaTableStats)
		{
			return 0;
		}

	}
}
