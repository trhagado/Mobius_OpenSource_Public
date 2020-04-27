using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Data;
using System.Data.Common;

using Oracle.DataAccess.Client;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// DbCommandMx DataReader functions 
	/// </summary>

	public partial class GenericMetaBroker : IMetaBroker
	{

		/// <summary>
		/// Simple prepare query with no parameters
		/// </summary>
		/// <param name="sql"></param>

		public void PrepareReader(
			DbCommandMx drd,
			string sql)
		{
			if (BuildSqlOnly)
				Qe.AddSqlToSqlStatementList(this, sql);

			else
			{
				DateTime t0 = DateTime.Now;
				drd.Prepare(sql, DbType.String, 0);
				double dt = TimeOfDay.Delta(t0);
				ExecuteReaderTime += dt;
				//ExecuteReaderCount++;
			}

			return;
		}

		public DbDataReader ExecuteReader(
			DbCommandMx drd,
			object[] parmValues)
		{
			DateTime t0 = DateTime.Now;
			DbDataReader dr = drd.ExecuteReader(parmValues);
			double dt = TimeOfDay.Delta(t0);
			ExecuteReaderTime += dt;
			ExecuteReaderCount++;
			return dr;
		}

		/// <summary>
		/// Prepare query with string parameters
		/// </summary>
		/// <param name="drd"></param>
		/// <param name="sql"></param>
		/// <param name="stringParameterCount"></param>

		public void PrepareMultipleParameterReader(
			DbCommandMx drd,
			string sql,
			int stringParameterCount)
		{
			if (BuildSqlOnly)
				Qe.AddSqlToSqlStatementList(this, sql);

			else
			{
				DateTime t0 = DateTime.Now;
				drd.Prepare(sql, OracleDbType.Varchar2, stringParameterCount);
				double dt = TimeOfDay.Delta(t0);
				ExecuteReaderTime += dt;
				//ExecuteReaderCount++;
			}
			return;
		}

		/// <summary>
		/// Prepare a query that takes a list as part of criteria
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterType"></param>

		public void PrepareListReader(
			DbCommandMx drd,
			string sql,
			DbType parameterType)
		{
			drd.PrepareListReader(sql, parameterType);
			return;
		}

		/// <summary>
		/// Prepare a query that takes a list as part of criteria
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterType"></param>
		/// <param name="keyPredType"></param>

		public void PrepareListReader(
			DbCommandMx drd,
			string sql,
			DbType parameterType,
			KeyListPredTypeEnum keyListPredType)
		{
			drd.PrepareListReader(sql, parameterType, keyListPredType);
			return;
		}

		/// <summary>
		/// Open a list reader
		/// </summary>
		/// <param name="list"></param>

		public void ExecuteListReader(
			DbCommandMx drd,
			List<string> list)
		{
			if (BuildSqlOnly)
			{
				string sql = drd.ListSql; // sql with list item place holder
				Qe.AddSqlToSqlStatementList(this, sql);
			}

			else
			{
				DateTime t0 = DateTime.Now;
				drd.ExecuteListReader(list);
				double dt = TimeOfDay.Delta(t0);
				ExecuteReaderTime += dt;
				ExecuteReaderCount++;
			}

			return;
		}

		/// <summary>
		/// Read next row for DbCommand
		/// </summary>
		/// <returns></returns>

		public bool Read(DbCommandMx drd)
		{
			object o;
			string key;

			DateTime t0 = DateTime.Now;
			while (true)
			{
				bool rowWasRead = drd.Read();
				double dt = TimeOfDay.Delta(t0);
				ReadRowTime += dt;
				if (rowWasRead) ReadRowCount++;
				else return false; // read failed

				HashSet<string> keysToExclude = (Query != null ? Query.KeysToExclude : null);
				if (keysToExclude != null && keysToExclude.Count > 0  && drd.Cmd != null) // remove any keys to be excluded from keySubset
				{
					try
					{

						o = drd.GetObject(0); // assume key is in first position
						if (TryGetKeyValueString(o, out key) && Lex.IsDefined(key) && 
							keysToExclude.Contains(key))
						{
							ReadRowsFilteredByKeyExclusionList++;
							continue;
						}
					}

					catch (Exception ex) { return rowWasRead; } // accept record as is
				}

				return rowWasRead;
			}
		}

/// <summary>
/// Try to convert an object into a key value string
/// </summary>
/// <param name="o"></param>
/// <param name="keyString"></param>
/// <returns></returns>

		public static bool TryGetKeyValueString(
			object o,
			out string keyString)
		{
			int intVal;

			keyString = null;

			if (o == null || o is DBNull) return false;

			else if (o is string)
			{
				keyString = o as string;
				return true;
			}

			else if (RowUtil.TryConvertToInt(o, out intVal))
			{
				keyString = string.Format("{0,8:00000000}", intVal); // normalized for database CorpId
				return true;
			}

			else
			{
				keyString = o as string;
				return Lex.IsDefined(keyString);
			}
		}


	}
}
