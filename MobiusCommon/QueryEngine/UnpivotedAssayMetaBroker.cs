using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// Metabroker for integrated view of unpivoted assay results from multiple databases
	/// </summary>

	public class UnpivotedAssayMetaBroker : GenericMetaBroker
	{
		ActivityClass ActivityClass = null;

/// <summary>
/// Build sql and prepare query
/// </summary>
/// <param name="eqp">ExecuteQueryParms</param>
/// <returns>Sql for the query</returns>

		public override string PrepareQuery(
			ExecuteQueryParms eqp)
		{
			Eqp = eqp;
			Qt = eqp.QueryTable;

			Sql = BuildSql(eqp);

			OrderBy = "";

			return Sql;
		}

	/// <summary>
	/// Build the sql for a query
	/// </summary>
	/// <param name="eqp">ExecuteQueryParms</param>
	/// <returns>Sql for the query</returns>

		public override string BuildSql(
			ExecuteQueryParms eqp)
		{
			string sql = "";
			MetaColumn mc;

			List<MetaBrokerType> MbTypes = // metabrokers that can provide unpivoted assay data
				new List<MetaBrokerType>();

			MbTypes.Add(MetaBrokerType.Assay);

			Eqp = eqp;
			Qt = eqp.QueryTable;
			Exprs = FromClause = OrderBy = ""; // outer sql elements
			KeyMc = Mt.KeyMetaColumn;

			SelectList = new List<MetaColumn>(); // build list of selected metacolumns
			foreach (QueryColumn qc in Qt.QueryColumns)
			{
				mc = qc.MetaColumn;
				if (qc.Selected)
					SelectList.Add(mc);
			}

			foreach (MetaBrokerType mbt in MbTypes) // get Sql for each broker and union together
			{
				IMetaBroker mb = MetaBrokerUtil.Create(mbt);
				if (mb == null) throw new Exception("Unrecognized Metabroker: " + mbt);
				string sql2 = mb.BuildUnpivotedAssayResultsSql(eqp);

				//if (actBinAQc != null)
				//  sql2 = Lex.Replace(sql2, "null activity_bin", ActivityBinSqlExpression + " activity_bin");

				if (Lex.IsDefined(sql)) sql += " union all ";
				sql += "/*** MetaBrokerType." + mbt.ToString() + " ***/ " + sql2;
			}

			sql = " select * from ( " + sql + " ) ";
			if (Qt.Alias != "") sql += " " + Qt.Alias;
			if (eqp.CallerSuppliedCriteria != "")
				sql += " where " + eqp.CallerSuppliedCriteria;

			return sql;
		}

/// <summary>
/// Execute the query
/// </summary>
/// <param name="eqp"></param>

		public override void ExecuteQuery(
			ExecuteQueryParms eqp)
		{
			base.ExecuteQuery(eqp);
			ActivityClass = new ActivityClass();
			return;
		}

/// <summary>
/// Get the next data row
/// </summary>
/// <returns>Array of row values</returns>

		public override Object[] NextRow()
		{
			object[] vo = base.NextRow();

			StringMx activityClass = ActivityClass.CalculateAndStoreClass(this, vo);

			return vo;
		}

/// <summary>
/// Gets a dictionary by MetaBrokerType of the assay metatables in a query that can be retrieved in an unpivoted form
/// </summary>
/// <param name="q"></param>

		Dictionary<MetaBrokerType, List<MetaTable>> GetUnpivotedAssayMetaTables
			(Query q)
		{
			QueryTable qt;
			MetaTable mt;
			int qti;

			Dictionary<MetaBrokerType, List<MetaTable>> dict = new Dictionary<MetaBrokerType,List<MetaTable>>();

			for (qti = 0; qti < q.Tables.Count; qti++)
			{
				qt = Query.Tables[qti];
				mt = qt.MetaTable;

				MetaBrokerType mbt = mt.MetaBrokerType;
				IMetaBroker mb = MetaBrokerUtil.GetGlobalBroker(mbt);
				if (mb == null) continue;

				if (!mb.CanBuildUnpivotedAssayResultsSql) continue;

				if (!dict.ContainsKey(mbt)) dict[mbt] = new List<MetaTable>();

				dict[mbt].Add(mt);
			}

			return dict;
		}

	} // UnpivotedAssayMetaBroker

		/// <summary>
		/// Class to handle calculation of result activity class
		/// </summary>

		class ActivityClass
		{
			GenericMetaBroker Mb; // broker that created this instance
			QueryTable Qt;
			object[] Vo;

// Col values retrieved from row

			string Database = "";
			int MethodId = NullValue.NullNumber;
			int ResultTypeId = NullValue.NullNumber;
			string ResultSpCrc = "";
			double ValNbr = NullValue.NullNumber;
			string ValPrefix = "";
			string ValTxt = "";
			string Units = "";

// Positions of values to be retrieved to calculate class

			bool VoPositionsInitialized = false;
			int ActivityClassVoi = -1;
			int DatabaseVoi = -1;
			int MethodIdVoi = -1;
			int ResultTypeIdVoi = -1;
			int ResultSpCrcVoi = -1;
			int ValueVoi = -1;
			int ValuePrfxVoi = -1;
			int ValueNbrVoi = -1;
			int ValueTxtVoi = -1;
			int UnitsVoi = -1;

			bool SP = false; // single-point result
			bool CRC = false; // CRC result
			string MetaDataUnits = ""; // units if available from metadata

/// <summary>
/// CalculateAndStoreClass
/// </summary>
/// <param name="vo"></param>
/// <returns></returns>

			public StringMx CalculateAndStoreClass(
				GenericMetaBroker mb,
				object[] vo)
			{
				Mb = mb;
				Qt = mb.Qt;
				Vo = vo;

				if (!VoPositionsInitialized)
					GetVoPositions();

				if (ActivityClassVoi < 0) return null;
				
				ExtractValues();
				StringMx sx = AssignClass();
				return sx;
			}

			/// <summary>
			/// Find the set of query columns whose values are needed to calculate the activity class
			/// </summary>

			void GetVoPositions()
			{
				ActivityClassVoi = GetVoPosition("ACTIVITY_CLASS");
				DatabaseVoi = GetVoPosition("ASSY_DB");
				MethodIdVoi = GetVoPosition("ASSY_ID_ASSAYMETADATA");
				ResultTypeIdVoi = GetVoPosition("RSLT_TYP_ID_ASSAYMETADATA");
				ResultSpCrcVoi = GetVoPosition("RSLT_TYP");
				ValueVoi = GetVoPosition("RSLT_VALUE");
				ValuePrfxVoi = GetVoPosition("RSLT_PRFX_TXT", "RSLT_MEAN_PRFX_TXT");
				ValueNbrVoi = GetVoPosition("RSLT_VALUE_NBR", "RSLT_MEAN_VALUE_NBR");
				ValueTxtVoi = GetVoPosition("RSLT_VALUE_TXT", "RSLT_MEAN_VALUE_TXT");
				UnitsVoi = GetVoPosition("RSLT_UNITS", "RSLT_MEAN_UNITS");

				VoPositionsInitialized = true;
				return;
			}

			int GetVoPosition(params string[] colNameList)
			{

				QueryColumn qc = Qt.GetSelectedQueryColumnByName(colNameList);
				if (qc == null) return -1;
				string mcName = qc.MetaColumn.Name;

				for (int si = 0; si < Mb.SelectList.Count; si++)
				{
					if (Lex.Eq(Mb.SelectList[si].Name, mcName))
						return si;
				}

				return -1;
			}

/// <summary>
/// ExtractValues
/// </summary>
/// <param name="vo"></param>

			void ExtractValues()
			{
				QualifiedNumber qn = null;

				Database = GetVoValString(DatabaseVoi);
				MethodId = GetVoValInt(MethodIdVoi);
				ResultTypeId = GetVoValInt(ResultTypeIdVoi);
				ResultSpCrc = GetVoValString(ResultSpCrcVoi);

				ValNbr = GetVoValDouble(ValueNbrVoi);
				ValPrefix = GetVoValString(ValuePrfxVoi);
				ValTxt = GetVoValString(ValueTxtVoi);

				qn = GetVoValQualifiedNumber(ValueVoi);
				if (qn != null)
				{
					if (ValNbr == NullValue.NullNumber) ValNbr = qn.NumberValue;
					if (Lex.IsUndefined(ValPrefix)) ValPrefix = qn.Qualifier;
					if (Lex.IsUndefined(ValTxt)) ValTxt = qn.TextValue;
				}

				Units = GetVoValString(UnitsVoi);
				return;
			}

			/// <summary>
			/// GetVoValString
			/// </summary>
			/// <param name="qc"></param>
			/// <param name="vo"></param>
			/// <returns></returns>

			String GetVoValString(
				int voPos)
			{
				object o = GetVoVal(voPos);
				if (o == null) return "";
				else if (o is String) return (String)o;
				else try
					{
						String val = Convert.ToString(o);
						return val;
					}
					catch (Exception ex)
					{ return ""; }
			}


			/// <summary>
			/// GetVoValInt
			/// </summary>
			/// <param name="qc"></param>
			/// <param name="vo"></param>
			/// <returns></returns>

			int GetVoValInt(
				int voPos)
			{
				object o = GetVoVal(voPos);
				if (o == null) return NullValue.NullNumber;
				else if (o is int) return (int)o;
				else try
					{
						int val = Convert.ToInt32(o);
						return val;
					}
					catch (Exception ex)
					{ return NullValue.NullNumber; }
			}

			/// <summary>
			/// GetVoValDouble
			/// </summary>
			/// <param name="qc"></param>
			/// <param name="vo"></param>
			/// <returns></returns>

			double GetVoValDouble(
				int voPos)
			{
				object o = GetVoVal(voPos);
				if (o == null) return NullValue.NullNumber;
				else if (o is double) return (double)o;
				else try
					{
						double val = Convert.ToDouble(o);
						return val;
					}
					catch (Exception ex)
					{ return NullValue.NullNumber; }
			}

/// <summary>
/// GetVoValQualifiedNumber
/// </summary>
/// <param name="qc"></param>
/// <param name="vo"></param>
/// <returns></returns>

			QualifiedNumber GetVoValQualifiedNumber(
				int voPos)
			{
				QualifiedNumber qn = null;

				object value = GetVoVal(voPos);
				if (NullValue.IsNull(value)) return null;
				if (value is QualifiedNumber)
					qn = (QualifiedNumber)value;
				else QualifiedNumber.TryConvertTo(value, out qn);

				return qn;
			}

			/// <summary>
			/// GetVoVal - get untyped value from Vo
			/// </summary>
			/// <param name="qc"></param>
			/// <param name="vo"></param>
			/// <returns></returns>

			object GetVoVal(
				int voPos)
			{
				if (Vo == null || voPos < 0) return null;
				object o = Vo[voPos];
				return o;
			}

			/// <summary>
			/// Assign class to result
			/// </summary>
			/// <returns></returns>

			StringMx AssignClass()
			{
				if (Vo == null || ActivityClassVoi < 0) return null;

				StringMx sx = new StringMx();
				try
				{
					Vo[ActivityClassVoi] = null;

					if (NullValue.IsNull(ValNbr) && NullValue.IsNull(ValTxt)) return null; // need a numeric or text value

					if (NullValue.IsNull(MethodId)) throw new Exception("MethodId not defined");
					if (NullValue.IsNull(ResultTypeId)) throw new Exception("ResultTypeId not defined");

					string actClass = "";

// Default SP/CRC activity class assignment

					if (NullValue.IsNull(ValNbr)) return null; // valid for numeric results only

					// SP

					if (Lex.Eq(ResultSpCrc, "SP"))
					{
						if (Lex.Eq(ValPrefix, "<")) actClass = "Fail";
						else if (ValNbr < 70) actClass = "Fail";
						else if (ValNbr < 90) actClass = "BorderLine";
						else if (ValNbr >= 90) actClass = "Pass";
					}

// CRC

					else if (Lex.Eq(ResultSpCrc, "CRC"))
					{
						double factor = 1.0; // normalize to uM units
						if (Lex.Eq(Units, "uM")) factor = 1;
						else if (Lex.Eq(Units, "mM")) factor = 1000;
						else if (Lex.Eq(Units, "nM")) factor = 0.001;
						else return null;

						double v = ValNbr * factor;

						if (Lex.Eq(ValPrefix, ">")) actClass = "Fail";
						else if (v > 5) actClass = "Fail";
						else if (v > 0.1) actClass = "BorderLine";
						else actClass = "Pass";
					}

					else return null;

					sx.Value = actClass;
					//todo: apply coloring...
				}

				catch (Exception ex) // if exception, store exception message in class result
				{
					sx = new StringMx();
					sx.Value = ex.Message;
				}

				Vo[ActivityClassVoi] = sx; // store class info
				return sx;
			}


		/// <summary>
		/// GetActivityClassSqlExpression
		/// </summary>
		/// <param name="rsltTypeCol"></param>
		/// <param name="rsltPrefixCol"></param>
		/// <param name="rsltValueNumberCol"></param>
		/// <returns></returns>

		public static string GetActivityClassSqlExpression(
			string rsltTypeCol,
			string rsltPrefixCol,
			string rsltValueNumberCol)
		{
			return "'ND'"; // just return not determined here, calculate in NextRow

			//string sql = ActivityClassSqlExpression;

			//sql = Lex.Replace(sql, "rslt_typ", rsltTypeCol);
			//sql = Lex.Replace(sql, "rslt_prfx_txt", rsltPrefixCol);
			//sql = Lex.Replace(sql, "rslt_value_nbr", rsltValueNumberCol);

			//return sql;
		}

		/// <summary>
		/// Sql expression to calculate activity class for SP & CRC results (not used, calculated in code instead)
		/// </summary>

		public static string ActivityClassSqlExpression
		{
			get
			{
				return @" 
	   (case
       when rslt_typ = 'SP' then (case
			  when rslt_prfx_txt = '<' or then 'Fail'
				when rslt_value_nbr<70 then 'Fail'
        when rslt_value_nbr<90 then 'BorderLine'  
        when rslt_value_nbr>=90 then 'Pass'  
        else null
        end)
       when rslt_typ = 'CRC' then (case
			  when rslt_prfx_txt = '>' then 'Fail'
        when rslt_value_nbr>10 then 'Fail'
        when rslt_value_nbr>5 then 'Fail' 
        when rslt_value_nbr>1 then 'BorderLine'  
        when rslt_value_nbr>0.5 then 'BorderLine'  
        when rslt_value_nbr>0.1 then 'BorderLine'  
        when rslt_value_nbr>0.01 then 'Pass' 
        when rslt_value_nbr<=0.01 then 'Pass' 
        else null
        end)
       else null
       end)";
			}
		}

	} // ActivityClass

		class ActivityBin
		{
			/// <summary>
			/// GetActivityBinSqlExpression
			/// </summary>
			/// <param name="rsltTypeCol"></param>
			/// <param name="rsltPrefixCol"></param>
			/// <param name="rsltValueNumberCol"></param>
			/// <returns></returns>

			public static string GetActivityBinSqlExpression(
				string rsltTypeCol,
				string rsltPrefixCol,
				string rsltValueNumberCol)
			{
				string sql = ActivityBinSqlExpression;

				sql = Lex.Replace(sql, "rslt_typ", rsltTypeCol);
				sql = Lex.Replace(sql, "rslt_prfx_txt", rsltPrefixCol);
				sql = Lex.Replace(sql, "rslt_value_nbr", rsltValueNumberCol);

				return sql;
			}

			/// <summary>
			/// Sql expression to calculate activity bin for SP & CRC results
			/// </summary>

			public static string ActivityBinSqlExpression
			{
				get
				{
					return @" 
	   (case
       when rslt_typ = 'SP' then (case
			  when rslt_prfx_txt = '<' then 0
        when rslt_value_nbr<70 then 1 
        when rslt_value_nbr<90 then 4 
        when rslt_value_nbr>=90 then 5  
        else null
        end)
       when rslt_typ = 'CRC' then (case
			  when rslt_prfx_txt = '>' then 0
        when rslt_value_nbr>10 then 1
        when rslt_value_nbr>5 then 4 
        when rslt_value_nbr>1 then 5 
        when rslt_value_nbr>0.5 then 7 
        when rslt_value_nbr>0.1 then 8 
        when rslt_value_nbr>0.01 then 9 
        when rslt_value_nbr<=0.01 then 10 
        else null
        end)
       else null
       end)";
				}
			}

		} // ActivityBin
}
