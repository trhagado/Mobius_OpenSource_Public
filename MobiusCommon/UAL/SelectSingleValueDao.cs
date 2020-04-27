using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Data;
using System.Data.Common;

using Oracle.DataAccess.Client;

namespace Mobius.UAL
{
	/// <summary>
	/// Summary description for SelectSingleValueDao.
	/// </summary>
	public class SelectSingleValueDao
	{
		public SelectSingleValueDao()
		{
			return;
		}

		public static string SelectString (
			string sql)
		{
			return SelectString(null,sql);
		}

		public static string SelectString (
			DbConnectionMx mxConn,
			string sql)
		{
			string value;
			DbDataReader dr = null;

			DbCommandMx drd = new DbCommandMx();
			drd.MxConn = mxConn; // set connection to use 
			drd.Prepare(sql);
			dr = drd.ExecuteReader();

			if (!dr.Read()) throw (new Exception("SelectString Read failed"));
			if (dr.IsDBNull(0)) value = "";
			else value = dr.GetString(0);
			dr.Close();
			drd.Dispose();
			return value;	
		}

		public static int SelectInt (
			string sql)
		{
			return SelectInt(null,sql);
		}

		public static int SelectInt (
			DbConnectionMx mxConn,
			string sql)
		{
			int value;
			DbDataReader dr = null;

			DbCommandMx drd = new DbCommandMx();
			drd.MxConn = mxConn; // set connection to use 
			drd.Prepare(sql);
			dr = drd.ExecuteReader() as OracleDataReader;

			if (!dr.Read()) throw (new Exception("SelectInt Read failed"));

			value =  drd.GetInt(0);
			dr.Close();
			drd.Dispose();
			return value;	
		}

		public static int SelectInt (
			string sql,
			OracleDbType parmType,
			object parmValue)
		{
			DbCommandMx cmd = DbCommandMx.PrepareExecuteAndRead(
				sql,
				new OracleDbType[] { parmType },
				new object[] { parmValue });

			if (cmd == null) return NullValue.NullNumber;

			int value = cmd.GetInt(0);
			cmd.CloseReader();
			return value;
		}

		public static int SelectInt (
			string sql,
			OracleDbType[] parmTypes,
			object[] parmValues)
		{ 
			DbCommandMx cmd = DbCommandMx.PrepareExecuteAndRead(sql, parmTypes, parmValues);
			if (cmd == null) return NullValue.NullNumber;

			int value = cmd.GetInt(0);
			cmd.CloseReader();
			return value;
		}




	public static DateTime SelectDateTime (
			string sql)
		{
			return SelectDateTime(null,sql);
		}

		public static DateTime SelectDateTime (
			DbConnectionMx mxConn,
			string sql)
		{
			DateTime value;
			DbDataReader dr = null;

			DbCommandMx drd = new DbCommandMx();
			drd.MxConn = mxConn; // set connection to use 
			drd.Prepare(sql);
			dr = drd.ExecuteReader();

			if (!dr.Read()) throw (new Exception("SelectDateTime Read failed"));
			if (dr.IsDBNull(0)) value = DateTime.MinValue;
			else value = dr.GetDateTime(0);
			dr.Close();
			drd.Dispose();
			return value;	
		}


	}
}
