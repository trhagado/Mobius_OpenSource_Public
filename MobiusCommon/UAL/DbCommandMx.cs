using Mobius.ComOps;
using Mobius.Data;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;

using Oracle.DataAccess.Client; 
using Oracle.DataAccess.Types;

namespace Mobius.UAL
{
	/// <summary>
	/// DbCommandOracle Methods
	/// </summary>
	
	public partial class DbCommandMx
	{
		internal int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public string LastSql // most recent Sql statement seen
		{
			get => _lastSql;
			set => _lastSql = DbConnectionMx.RemoveDatabasePasswords(value); // strip out any passwords before storing/logging, etc.
		}
		string _lastSql = null;

		public string LastSqlFormatted { get { return OracleDao.FormatSql(LastSql); } }
		public int ParameterCount = 0; // number of parameters in SQL
		public string ParameterTypes = ""; // parameter types
		public string ParameterValues = ""; // list of parameters applied
		public string DatabaseName = ""; // associated database command is executed against
		public string DatabaseAcct = ""; // account name used to execute the sql

		public DbConnectionMx MxConn; // associated Mobius connection
		public bool IsOracle = false; // true if Oracle access

		public DbCommand Cmd; // associated command
		public OracleCommand OracleCmd { get { return Cmd as OracleCommand; } }
		public OdbcCommand OdbcCmd { get { return Cmd as OdbcCommand; } }
		public OleDbCommand OleDbCmd { get { return Cmd as OleDbCommand; } }

		//public int InitialLOBFetchSize { set { if (OracleCmd != null) OracleCmd.InitialLOBFetchSize = value; } } // set to positive value to avoid trips back to the DB to get lob values 

		public DbDataReader Rdr; // data reader for command (can set FetchSize for bigger read buffer)
		public OracleDataReader OracleRdr { get { return Rdr as OracleDataReader; } }
		public OdbcDataReader OdbcRdr { get { return Rdr as OdbcDataReader; } }
		public OleDbDataReader OleDbRdr { get { return Rdr as OleDbDataReader; } }

		public ICheckForCancel CheckForCancel;
		public bool Cancelled = false;
		public int Timeout = 0; // time in seconds to allow query to run 

		public string ListSql; // sql for reading from list

		public KeyListPredTypeEnum KeyListPredType = DefaultKeyListPredType;
		public bool SingleKeyRetrieval = false; // if true retrieve keys singly for potentially improved performance
		public DbType KeyListParameterType; 
		public List<string> KeyList;
		public int FirstKeyIdx = -1;
		public int LastKeyIdx = -1;
		public int LastKeyCount = -1;

		public static int DbListTotalCount; // total number of db lists created
		public static long DbListTotalSize;
		public static int DbListAvgSize;
		public static int DbListMaxSize;
		public static double DbListTotalTime;
		public static double DbListAvgTime;

		public static int IdCount; // number of DbCommandMx objects created
		public static bool LogExceptions = true;
		public static bool LogDatabaseCalls = false;
		public static double LogDatabaseCallsTime = 0;
		public static bool LogDbCommandDetail = false;
		public static HashSet<string> SqlHash = new HashSet<string>(); // hash of sql statments if logging

		public static int LogReadsMaxRows = 2; // max number of reads to log if Debug = true
		public static int LogLongReadLimit = 0; // 10; // log any reads that take longer than this many ms if > 0
		public static bool FixupSqlFlag;
		
		public static int MaxOdbcInListItemCount = 256; // maximum number of items in a SQL sublist (playing it safe for Netezza)
		public static int MaxOracleInListItemCount = 1000; // maximum number of items in a SQL sublist
		public static int MaxOracleStringSize = 4000; // for char, varchar2 (32767 possible for 12C and later) 
		public static KeyListPredTypeEnum DefaultKeyListPredType = KeyListPredTypeEnum.Parameterized; // default pred type to use for key lists
		public static int DbFetchRowCount = 100; // number of rows to attempt to fetch in each call to the database system
		public static int DbFetchSizeMax = 131072; // Upper size limit in bytes for DbFetchRowCount * OracleRdr.RowSize

		public static int GlobalPrepareReaderCount = 0;
		public static double GlobalLastPrepareReaderTime = 0;
		public static double GlobalTotalPrepareReaderTime = 0;

		public static int GlobalExecuteReaderCount = 0;
		public static double GlobalLastExecuteReaderTime = 0;
		public static double GlobalTotalExecuteReaderTime = 0;

		public static int GlobalReadCount = 0;
		public static double GlobalLastReadTime = 0;
		public static double GlobalTotalReadTime = 0;

		public static int GlobalCloseReaderCount = 0;

		public static int GlobalExecuteNonReaderCount = 0;
		public static double GlobalLastExecuteNonReaderTime = 0;
		public static double GlobalTotalExecuteNonReaderTime = 0;

		public int PrepareReaderCount = 0;
		public double PrepareReaderTime = 0;

		public int ExecuteReaderCount = 0; // instance stats
		public double ExecuteReaderTime = 0;

		public int ReadCount = 0; 
		public double LastReadTime = 0;
		public double LastLongReadTime = 0;
		public double TotalReadTime = 0;
		public double AverageReadTime = 0;

		public int ExecuteNonReaderRowCount = 0;
		public double ExecuteNonReaderTime = 0;

		public Exception CancellableOpException;
		public string StackTraceString;
		public bool ReadComplete; // true if read is complete
		public bool ReadResult; // result of read operation

		public static bool NoDatabaseAccessIsAvailable { get => DbConnectionMx.NoDatabaseAccessIsAvailable; } // set to true for test mode with no database access

		// Delegates 

		//delegate DbDataReader ExecuteReaderMethod(object[] parameters);
		//delegate bool ReadMethod();
		//delegate void CancellableReadMethod();
		//delegate string GetClobMethod(int pos);
		//delegate int GetIntMethod(int pos);
		//delegate long GetLongMethod(int pos);
		//delegate double GetDoubleMethod(int pos);

		// Delegate definitions

		//ReadMethod ReadDelegate;
		//GetClobMethod GetClobDelegate;
		//GetIntMethod GetIntDelegate;
		//GetLongMethod GetLongDelegate;
		//GetDoubleMethod GetDoubleDelegate;

		public DbCommandMx()
		{
			return;
		}

/// <summary>
/// Destructor 
/// </summary>

		/// <summary>
		/// Finalizer
		/// </summary>

		~DbCommandMx()
		{
			Dispose();
			return;
		}

		/// <summary>
		/// Set the connection from the sql
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public DbConnectionMx SetConnection (
			string sql)
		{
			DbConnectionMx mxConn = DbConnectionMx.MapSqlToConnection(ref sql); // get connection name from sql
			if (mxConn!=null) 
			{
				if (this.MxConn != null) MxConn.Close(); // free any prev connection
				this.MxConn = mxConn; // MxConn may be preset for non-reader sql
			}

			return mxConn;
		}

/// <summary>
/// abbreviated AddDataColumn
/// </summary>
/// <param name="table"></param>
/// <param name="colName"></param>
/// <param name="type"></param>
/// <returns></returns>

		public static DataColumn ADC(  
			DataTable table,
			string colName,
			DbType type)
		{
			OracleDbType oracleType = DbTypeToOracleDbType(type);
			return AddDataColumn(table, colName, oracleType);
		}

		/// <summary>
		/// Add a data column to data table
		/// </summary>
		/// <param name="table"></param>
		/// <param name="colName"></param>
		/// <param name="type"></param>
		/// <returns></returns>

		public static DataColumn AddDataColumn(
			DataTable table,
			string colName,
			OracleDbType type)
		{
			DataColumn dc = table.Columns.Add(colName);
			dc.ExtendedProperties.Add("OracleDbType", type);
			return dc;
		}

		/// <summary>
		/// Build basic select statement for a data table
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>

		public static string BuildSelectSql(
			DataTable table)
		{
			string sql =
				"select " + GetFieldListString(table) + " " +
				" from " + table.TableName + " ";
			return sql;
		}

		/// <summary>
		/// Build a basic insert statement for a data table
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>

		public static string BuildInsertSql(
			DataTable table)
		{
			string sql =
				"insert into " + table.TableName +
				"(" + GetFieldListString(table) + ") " +
				" values (" + GetParameterValuestring(table) + ")";
			return sql;
		}

		/// <summary>
		/// Build a basic update statement for a data table
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>

		public static string BuildUpdateSql(
			DataTable table)
		{
			StringBuilder sb = new StringBuilder();
			for (int ci = 0; ci < table.Columns.Count; ci++)
			{
				if (ci > 0) sb.Append(", ");
				sb.Append(table.Columns[ci].ColumnName);
				sb.Append("=");
				sb.Append(":"); sb.Append(ci.ToString());
			}

			string sql = "update " + table.TableName + " set " + sb.ToString();
			return sql;
		}

		/// <summary>
		/// Get a comma-delimited string of all of the column names for a dataTable
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>

		public static string GetFieldListString(
			DataTable table)
		{
			StringBuilder sb = new StringBuilder();
			for (int ci = 0; ci < table.Columns.Count; ci++)
			{
				sb.Append(table.Columns[ci].ColumnName);
				if (ci < table.Columns.Count - 1) sb.Append(",");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Get a comma-delimited string of the parameter place holders for a dataTable
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>

		public static string GetParameterValuestring(
			DataTable table)
		{
			StringBuilder sb = new StringBuilder();
			for (int ci = 0; ci < table.Columns.Count; ci++)
			{
				sb.Append(":" + ci.ToString());
				if (ci < table.Columns.Count - 1) sb.Append(",");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Get an array of oracle parameter types for the columns in a dataTable
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>

		public static DbType[] GetParameterTypes(
			DataTable table)
		{
			return GetParameterTypes(table, 0);
		}

/// <summary>
/// Get an array of oracle parameter types for the columns in a dataTable
/// </summary>
/// <param name="table"></param>
/// <param name="extraParameterCount"></param>
/// <returns></returns>

		public static DbType[] GetParameterTypes(
			DataTable table,
			int extraParameterCount)
		{
			DbType[] pta = new DbType[table.Columns.Count + extraParameterCount];
			for (int ci = 0; ci < table.Columns.Count; ci++)
			{
				OracleDbType oracleType = (OracleDbType)table.Columns[ci].ExtendedProperties["OracleDbType"];
				pta[ci] = OracleDbTypeToDbType(oracleType);
			}

			return pta;
		}


		/// <summary>
		/// Prepare and execute reader for a simple query with no parameters
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static DbCommandMx PrepareAndExecuteReader(string sql)
		{
			DbCommandMx cmd = new DbCommandMx();
			cmd.Prepare(sql);
			cmd.ExecuteReader();
			return cmd;
		}

		/// <summary>
		/// Prepare, execute and do first read for a simple query with no parameters
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static DbCommandMx PrepareExecuteAndRead(string sql)
		{
			DbCommandMx cmd = PrepareExecuteAndRead(sql, null, null);
			return cmd;
		}

		/// <summary>
		/// Prepare, execute and do first read for a multi-parameter query
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterType"></param>
		/// <param name="parmValue"></param>
		/// <returns></returns>

		public static DbCommandMx PrepareExecuteAndRead(
			string sql,
			OracleDbType parameterType,
			object parmValue)
		{
			return PrepareExecuteAndRead(
				sql,
				new OracleDbType[] { parameterType },
				new object[] { parmValue } );
		}

		/// <summary>
		/// Prepare, execute and do first read for a multi-parameter query
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parmTypes"></param>
		/// <param name="parmValues"></param>
		/// <returns></returns>

		public static DbCommandMx PrepareExecuteAndRead(
			string sql,
			OracleDbType[] parmTypes,
			object[] parmValues)
		{
			DbCommandMx cmd = new DbCommandMx();
			cmd.Prepare(sql, parmTypes);
			cmd.ExecuteReader(parmValues);
			if (cmd.Read()) return cmd;
			else
			{
				cmd.CloseReader();
				cmd.Dispose();
				return null;
			}
		}

		/// <summary>
		/// Simple prepare query with no parameters
		/// </summary>
		/// <param name="sql"></param>

		public void Prepare (
			string sql)
		{
			Prepare(sql, DbType.String, 0);
		}

		/// <summary>
		/// Prepare query with string parameters
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="stringParameterCount"></param>

		public void PrepareMultipleParameter (
			string sql,
			int stringParameterCount)
		{
			Prepare(sql, OracleDbType.Varchar2, stringParameterCount);
			//			Prepare(sql,stringParameterCount,OracleType.Int32); // corp # is numeric
		}

/// <summary>
/// Prepare query for a single parameter of the specified type
/// </summary>
/// <param name="sql"></param>
/// <param name="parameterType"></param>

		public void PrepareParameterized(
			string sql,
			DbType parameterType)
		{
			OracleDbType oracleType = DbTypeToOracleDbType(parameterType);
			Prepare(sql, oracleType, 1);
		}

		/// <summary>
		/// Prepare query for a single parameter of the specified type
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterType"></param>

		public void Prepare(
			string sql,
			OracleDbType parameterType)
		{
			Prepare(sql, parameterType, 1);
		}

/// <summary>
/// Prepare query for a single parameter of the specified type
/// </summary>
/// <param name="sql"></param>
/// <param name="parameterType"></param>
/// <param name="parameterCount"></param>

		public void Prepare(
			string sql,
			DbType parameterType,
			int parameterCount)
		{
			OracleDbType oracleType = DbTypeToOracleDbType(parameterType);
			Prepare(sql, oracleType, parameterCount);
		}

		/// <summary>
		/// Prepare query for a set of parameters of same type
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterCount"></param>
		/// <param name="parameterType"></param>

		public void Prepare(
			string sql,
			OracleDbType parameterType,
			int parameterCount)
		{
			OracleDbType [] pa = null;
			if (parameterCount>0)
			{
				pa = new OracleDbType[parameterCount];
				for (int i1=0; i1<parameterCount; i1++)
					pa[i1]=parameterType;
			}
			Prepare(sql, pa);
      return;
		}

/// <summary>
/// Prepare query, possibly modifying sql to use dblinks
/// </summary>
/// <param name="sql"></param>
/// <param name="parmArray"></param>

		public void PrepareParameterized(
			string sql,
			DbType[] parmArray)
		{
			OracleDbType[] oracleParms = null;
			if (parmArray != null)
				oracleParms = new OracleDbType[parmArray.Length];

			for (int pai = 0; pai < parmArray.Length; pai++)
			{
				oracleParms[pai] =  DbTypeToOracleDbType(parmArray[pai]);
			}

			Prepare(sql, oracleParms);
			return;
		}

		/// <summary>
		/// Prepare query, possibly modifying sql to use dblinks
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parmArray"></param>

		public void Prepare(
			string sql,
			OracleDbType [] parmArray)
		{
			OracleParameter p;

			//if (Lex.Contains(sql, "t12")) sql = Lex.Replace(sql, "t12", "t12"); // debug

			if (NoDatabaseAccessIsAvailable) return;

			if (FixupSqlFlag) sql = FixupSql(sql);

			//if (Lex.Contains(sql, ":1001")) sql = sql; // debug
			//sql = Lex.Replace(sql, "first_rows", ""); // debug

			DbConnectionMx mxConn = DbConnectionMx.MapSqlToConnection(ref sql); // get connection name from sql
			if (mxConn != null)
			{
				if (this.MxConn != null) MxConn.Close(); // free any prev connection
				this.MxConn = mxConn; // MxConn may be preset for non-reader sql
			}

			if (MxConn == null) throw new Exception("Could not get connection, Sql: " + OracleDao.FormatSql(sql));

			PrepareUsingDefinedConnection(sql, parmArray);
			return;
		}

/// <summary>
/// Prepare the statement using the supplied sql and connection defined for command
/// </summary>
/// <param name="sql"></param>

		public void PrepareUsingDefinedConnection(
			string sql)
		{
			PrepareUsingDefinedConnection(sql, null);
			return;
		}

/// <summary>
/// Prepare the statement using the supplied sql and connection defined for command
/// </summary>
/// <param name="sql"></param>
/// <param name="parmArray"></param>

		public void PrepareUsingDefinedConnection(
			string sql,
			OracleDbType[] parmArray)
		{
			if (NoDatabaseAccessIsAvailable) return;

			try
			{
				//if (Lex.Contains(sql, "where obj_typ_id=8")) sql = sql; // debug

				if (LogDatabaseCalls && ExecuteReaderCount > 0) // log previous sql call on this command if a new sql statement
					LogDatabaseCall();

				DatabaseName = MxConn.DbConn.DataSource;

				if (MxConn.SessionConn != null && MxConn.SessionConn.DataSource != null)
					DatabaseAcct = MxConn.SessionConn.DataSource.UserName.ToUpper();

				MxConn.LastSql = sql;
				LastSql = sql;

				Stopwatch sw = Stopwatch.StartNew();
				IsOracle = false;

				if (MxConn.DbConn is OracleConnection)
					PrepareOracleSql(sql, parmArray);

				else if (MxConn.DbConn is OdbcConnection)
					PrepareOdbcSql(sql, parmArray);

				else if (MxConn.DbConn is OleDbConnection)
					PrepareOleDbSql(sql, parmArray);

				else throw new ArgumentException("Invalid connection type");

				double t0 = sw.Elapsed.TotalMilliseconds;

				PrepareReaderTime += t0;
				PrepareReaderCount++;
				
				GlobalLastPrepareReaderTime = t0;
				GlobalTotalPrepareReaderTime += t0;
				GlobalPrepareReaderCount++;

				if (LogDbCommandDetail)
					// || ClientState.IsDeveloper) // debug
					DebugLog.Message("CommandId = " + Id + ", Prepare Time = " + FT(t0) + ", Sql = " + Lex.RemoveLineBreaksAndTabs(sql));

			}
			catch (Exception ex)
			{
				if (MqlUtil.IsUserQueryErrorMessage(ex.Message))  // is exception from a user error in the query?
					throw new UserQueryException(ex.Message, ex);

				else
				{
					string msg = "DbCommandMx.Prepare Error:\r\n" +
						ex.Message + "\r\n\r\n" +
						"Sql: " + OracleDao.FormatSql(LastSql);
					LogException(DebugLog.FormatExceptionMessage(ex, msg));
					throw new Exception(msg, ex);
				}
			}

			return;
		}

/// <summary>
/// PrepareOracleSql
/// </summary>
/// <param name="sql"></param>
/// <param name="parmArray">Array of parameter types</param>
// /// <param name="arrayBindSize">Parameter count if binding as an array of parameters of the same type</param>

		void PrepareOracleSql(
			string sql,
			OracleDbType[] parmArray)
		{
			OracleParameter p;

			//sql = Lex.Replace(sql, "first_rows", ""); // debug

			Cmd = new OracleCommand();
			OracleCmd.InitialLOBFetchSize = 4000; // set InitialLOBFetchSize to avoid most extra trips to the database for Lob data (e.g. chem structures) 

			IsOracle = true;

			if (!Security.UserInfo.Privileges.CanRetrieveStructures)
				sql = MoleculeMx.RemoveSqlStructureRetrievalMethods(sql);

			if (!Security.UserInfo.Privileges.CanRetrieveSequences)
				sql = MoleculeMx.RemoveSqlSequenceRetrievalMethods(sql);

			LastSql = sql; 

			// Create paramters

			Cmd.Parameters.Clear();
			if (parmArray != null && parmArray.Length > 0)
			{

				for (int pi = 0; pi < parmArray.Length; pi++)
				{
					p = new OracleParameter();
					p.ParameterName = pi.ToString(); // name by position
					p.OracleDbType = parmArray[pi];
					p.Direction = ParameterDirection.Input;
					Cmd.Parameters.Add(p);
				}
			}

			// Prepare command

			Cmd.Connection = MxConn.DbConn;
			Cmd.CommandText = sql;
			Cmd.Prepare();

			return;
		}

		/// <summary>
		/// Prepare query
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parmArray"></param>

		void PrepareOdbcSql(
			string sql,
			OracleDbType[] parmArray)
		{
			OdbcParameter p;
			OdbcType odbcType;

			Cmd = new OdbcCommand();
			//if (ClientState.IsDeveloper) // debug
			//  SystemUtil.Beep();

			// Create paramters

			Cmd.Parameters.Clear();
			if (parmArray != null && parmArray.Length > 0)
			{
				for (int pi = 0; pi < parmArray.Length; pi++)
				{
					p = new OdbcParameter();
					p.ParameterName = pi.ToString();
					odbcType = ConvertOracleDbTypeToOdbcType(parmArray[pi]);
					p.OdbcType = odbcType;
					Cmd.Parameters.Add(p);
					string parmName = ":" + pi;
					if (sql.Contains(parmName)) sql = sql.Replace(parmName, "?");
					else continue; // may have already been converted to ODBC form 
				}
			}

			// Prepare command

			Cmd.Connection = MxConn.DbConn;

			if (Lex.Eq(DatabaseName, "NullDb")) // use fixed query to empty csv file for null database
				sql = @"SELECT * FROM MobiusServerData\MetaData\NullDb.csv";

			Cmd.CommandText = sql;
			Cmd.Prepare();

			return;
		}

		/// <summary>
		/// ConvertOracleDbTypeToOdbcType
		/// </summary>
		/// <param name="oType"></param>
		/// <returns></returns>

		static OdbcType ConvertOracleDbTypeToOdbcType(OracleDbType oType)
		{
			switch (oType)
			{
				case OracleDbType.Char: return OdbcType.Char;
				case OracleDbType.Varchar2: return OdbcType.VarChar;
				case OracleDbType.Long: return OdbcType.VarChar;
				case OracleDbType.Clob: return OdbcType.VarChar;

				case OracleDbType.Decimal: return OdbcType.Decimal;
				case OracleDbType.Byte: return OdbcType.TinyInt;
				case OracleDbType.Int16: return OdbcType.SmallInt;
				case OracleDbType.Int32: return OdbcType.Int;
				case OracleDbType.Int64: return OdbcType.BigInt;
				case OracleDbType.Double: return OdbcType.Double;
				case OracleDbType.Single: return OdbcType.Real;

				case OracleDbType.Date: return OdbcType.Timestamp;

				case OracleDbType.Raw: return OdbcType.Binary;

				default:
					throw new ArgumentException("Invalid DbType: " + oType);
			}
		}

		/// <summary>
		/// Prepare query
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parmArray"></param>

		void PrepareOleDbSql(
			string sql,
			OracleDbType[] parmArray)
		{
			OleDbParameter p;
			OleDbType odbType;

			Cmd = new OleDbCommand();

			// Create paramters

			Cmd.Parameters.Clear();
			if (parmArray != null && parmArray.Length > 0)
			{
				for (int pi = 0; pi < parmArray.Length; pi++)
				{
					p = new OleDbParameter();
					p.ParameterName = pi.ToString();
					odbType = ConvertOracleDbTypeToOleDbType(parmArray[pi]);
					p.OleDbType = odbType;
					Cmd.Parameters.Add(p);
				}
			}

			// Prepare command

			Cmd.Connection = MxConn.DbConn;
			Cmd.CommandText = sql;
			Cmd.Prepare();

			return;
		}

/// <summary>
/// ConvertOracleDbTypeToOleDbType
/// </summary>
/// <param name="oType"></param>
/// <returns></returns>

		static OleDbType ConvertOracleDbTypeToOleDbType(OracleDbType oType)
		{
			switch (oType)
			{
				case OracleDbType.Char: return OleDbType.Char;
				case OracleDbType.Varchar2: return OleDbType.VarChar;
				case OracleDbType.Long: return OleDbType.LongVarChar;
				case OracleDbType.Clob: return OleDbType.LongVarChar;

				case OracleDbType.Decimal: return OleDbType.Decimal;
				case OracleDbType.Byte: return OleDbType.UnsignedTinyInt;
				case OracleDbType.Int16: return OleDbType.SmallInt;
				case OracleDbType.Int32: return OleDbType.Integer;
				case OracleDbType.Int64: return OleDbType.BigInt;
				case OracleDbType.Double: return OleDbType.Double;
				case OracleDbType.Single: return OleDbType.Single;

				case OracleDbType.Date: return OleDbType.DBTimeStamp;

				case OracleDbType.Raw: return OleDbType.Binary;

				default:
					throw new ArgumentException("Invalid DbType: " + oType);
			}
		}

/// <summary>
/// 
/// </summary>
/// <returns></returns>

		public DbDataReader ExecuteReader()
		{
			object[] parmValues = new object[0];
			return ExecuteReader(parmValues);
		}
	
/// <summary>
/// 
/// </summary>
/// <param name="parmValue"></param>
/// <returns></returns>
	
		public DbDataReader ExecuteReader(
			object parmValue)
		{
			object [] parmValues = new object[1];
			parmValues[0] = parmValue;
			return ExecuteReader(parmValues);
		}

		/// <summary>
		/// Main ExecuteReader method, can be cancelled
		/// </summary>
		/// <param name="parmValues"></param>
		/// <returns></returns>

		public DbDataReader ExecuteReader(
			object[] parmValues)
		{

			if (NoDatabaseAccessIsAvailable) return null;

			int fieldCount = -1;
			long rowSize = -1;
			long fetchSize = -1;

			Rdr = null;
			Cancelled = false;
			CancellableOpException = null;

			if (parmValues != null) // set parameter values
			{
				ParameterCount = parmValues.Length;
				StringBuilder sb0 = new StringBuilder();
				for (int pi = 0; pi < parmValues.Length; pi++)
				{
					Cmd.Parameters[pi].Value = parmValues[pi];

					if (LogDatabaseCalls)
					{
						if (pi > 0) sb0.Append(",");
						if (parmValues[pi] != null) sb0.Append(parmValues[pi].ToString());
					}
				}
				ParameterValues = sb0.ToString();
			}

			//CheckForCancel = null; // debug !!!!

			//if (LastSql.Contains("select obj_id, obj_ty")) LastSql = LastSql; // debug

			if (CheckForCancel == null && Timeout <= 0) // no need to check for cancel or timeout
			{
					ExecuteReader2(false);
			}

			else // allow cancel by starting executeReader on separate thread
			{
				Stopwatch sw = Stopwatch.StartNew();
				ThreadStart ts = new ThreadStart(ExecuteReaderThreadMethod);
				Thread newThread = new Thread(ts);
				newThread.Name = "Cancellable ExecuteReader";
				newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start();
				Thread.Sleep(1); // sleep for 1 ms initially

				while (true) // loop until cancelled or complete
				{
					TimeSpan elapsed = sw.Elapsed; // get elapsed time for query

					if (Rdr != null) break; // complete ExecuteReader?

					else if (CheckForCancel != null && CheckForCancel.IsCancelRequested) // if not, see if user requested cancel
					{
						Cmd.Cancel();
						//while (!Cancelled) // loop til Oracle has completed cancel
						//	Thread.Sleep(10);
						Cancelled = true;
						break;
					}

					else if (Timeout > 0 && elapsed.TotalSeconds >= Timeout)
					{
						Cmd.Cancel();
						//while (!Cancelled) // loop til Oracle has completed cancel
						//	Thread.Sleep(10);
						throw new TimeoutException("DbCommandMx Timeout");
					}

					else if (CancellableOpException != null) // some exception occured
					{
						Exception ex = CancellableOpException;
						if (MqlUtil.IsUserQueryErrorMessage(ex.Message))  // is exception from a user error in the query?
							throw new UserQueryException(ex.Message, CancellableOpException);

						else
						{
							string msg = "DbCommandMx.CancellableExecuteReader Error:\r\n" +
								ex.Message;
							
							if (Lex.Contains(ex.Message, "ORA-01012") || Lex.Contains(ex.Message, "ORA-00028")) 
								{} // keep simple if forced logoff

							else // include sql and stack trace
							{
								msg += "\r\n\r\n" + "Sql: " + OracleDao.FormatSql(LastSql) + "\r\n\r\n";
								msg = DebugLog.FormatExceptionMessage(ex, msg);
							}
							LogException(msg);
							throw new Exception(msg);
						}
					}

					Thread.Sleep(10);
				}
			}

// Adjust fetchsize for Oracle

			if (Rdr != null)
			{
				//fieldCount = Rdr.FieldCount; // debug

				if (IsOracle) // expand FetchSize to at least 100 rows if practical
				{
					rowSize = OracleRdr.RowSize;
					fetchSize = OracleRdr.FetchSize; // default size is 131072
					long fetchSize2 = rowSize * DbFetchRowCount; // size for desired number of rows 
					if (fetchSize2 > DbFetchSizeMax) fetchSize2 = DbFetchSizeMax; // limit to a max size
					if (fetchSize2 > fetchSize) 
						OracleRdr.FetchSize = fetchSize = fetchSize2;
				}
			}

			//			DataTable dt = Rdr.GetSchemaTable(); // for dev
			//if (ExecuteReaderTime > 2000) ExecuteReaderTime = ExecuteReaderTime; // debug
			return Rdr;
		}

		/// <summary>
		/// Method for call to ExecuteReader from a separate thread
		/// </summary>

		void ExecuteReaderThreadMethod()
		{
			ExecuteReader2(true);
			return;
		}

		/// <summary>
		/// Internal ExecuteReader with logging 
		/// May be called in synchronously or asynchronously
		/// </summary>

		void ExecuteReader2(bool runningOnSeparateThread)
		{
			Rdr = null;
			Cancelled = false;
			CancellableOpException = null;

			try
			{
				Stopwatch sw = Stopwatch.StartNew();
				if (Cmd.Connection.State != ConnectionState.Open) Cmd.Connection.Open();
				Rdr = Cmd.ExecuteReader();

				double t1 = (int)sw.Elapsed.TotalMilliseconds;

				ExecuteReaderCount++;
				ExecuteReaderTime += t1;

				GlobalExecuteReaderCount++;
				GlobalLastExecuteReaderTime = t1;
				GlobalTotalExecuteReaderTime += t1;

				if (LogDbCommandDetail)
					DebugLog.Message("CommandId = " + Id + ", time: " + FT(t1)); //ExecuteReader: " + Lex.RemoveLineBreaksAndTabs(LastSql)); // debug
			}

			catch (Exception ex) // catch any cancel operation (also other exceptions)
			{
				if (Lex.Contains(ex.Message, "ORA-01013")) // Is the expected Oracle cancel message included? i.e.: "ORA-01013: user requested cancel of current operation"
					Cancelled = true;

				else
				{
					CancellableOpException = ex; // store other exceptions for pickup by main thread
					StackTraceString = ex.StackTrace.ToString();

					string msg = "DbCommandMx.ExecuteReader Error:\r\n" +
					 ex.Message + "\r\n\r\n" +	"Sql: " + OracleDao.FormatSql(LastSql);

					LogException(DebugLog.FormatExceptionMessage(ex, msg));
					if (!runningOnSeparateThread) throw new Exception(msg);
				}
			}
		}

		/// <summary>
		/// Read next row for query
		/// </summary>
		/// <returns></returns>

		public bool Read ()
		{
			Stopwatch sw = Stopwatch.StartNew();

			if (Rdr == null) return false;
			if (Rdr.IsClosed) return false;
			if (NoDatabaseAccessIsAvailable) return false;

			//CheckForCancel = null; // debug !!!!

			if (CheckForCancel == null || ReadCount > 0) // non-cancellable read or past 1st record
			{
				if (CheckForCancel != null && CheckForCancel.IsCancelRequested) // check cancel for rows after first
				{
					Cancelled = true;
					return false;
				}

				try
				{
					ReadResult = Rdr.Read();
				}
				catch (Exception ex)
				{
					string msg = "DbCommandMx.Read Error:\r\n" +
						ex.Message + "\r\n\r\n" +
						"Sql: " + OracleDao.FormatSql(LastSql);
					LogException(DebugLog.FormatExceptionMessage(ex, msg));
					throw new Exception(msg);
				}

			}

			else // allow cancel
			{
				ReadComplete = false;
				ThreadStart ts = new ThreadStart(CancellableRead);
				Thread newThread = new Thread(ts);
				newThread.Name = "Cancellable DB Read";
				newThread.IsBackground = true;
				newThread.SetApartmentState(ApartmentState.STA);
				newThread.Start();
				Thread.Sleep(1); // sleep for 1 ms initially
				while (true) // loop until cancelled or complete
				{
					if (ReadComplete) break; // did read complete
					else if (Cancelled) break; // or did we see an Oracle cancel exception

					else if (CheckForCancel.IsCancelRequested) // if not, see if cancelled by user
					{
						Cmd.Cancel();
						while (!Cancelled) // loop til Oracle has done cancel
							Thread.Sleep(10);
						Cancelled = true; // return immediately, cancel may not be done yet by Oracle
						return false;
					}

					else if (CancellableOpException != null) // some exception occured
					{
						string msg = "DbCommandMx.CancellableRead Error:\r\n" +
							CancellableOpException.Message + "\r\n\r\n" +
							"Sql: " + OracleDao.FormatSql(LastSql);
						DebugLog.Message(DebugLog.FormatExceptionMessage(CancellableOpException, msg));
						throw new Exception(msg);
					}


					Thread.Sleep(10);
				}
			}

//			if (LastSql.Contains("GLUCOKINASE_ACTIVATOR_I")) // debug
			//if (Lex.Contains(LastSql, "DCSGTR")) // debug
			//  {
			//  if (ReadResult)
			//  {
			//    object[] oa = new object[Rdr.FieldCount];
			//    int oaCnt = Rdr.GetValues(oa);
			//    LastSql = LastSql; 
			//  }
			//}

			if (!ReadResult) return false; // end of rows?

			ReadCount++;
			LastReadTime = sw.Elapsed.TotalMilliseconds;

			if (LogDbCommandDetail)
			{
				if (ReadCount <= LogReadsMaxRows || LastReadTime > 1)
				{
					DebugLog.Message("CommandId = " + Id + ", Count = " + ReadCount + ", Time = " + FT((int)LastReadTime));
					// Could serialize and log data values for Cmd, Rdr
				}


				else if (ReadCount == LogReadsMaxRows + 1)
					DebugLog.Message("CommandId = " + Id + ", ...");
			}

			if (LogLongReadLimit > 0 && LogLongReadLimit < LastReadTime)
			{
				LastLongReadTime = LastReadTime;
				int parmCount = 0;
				foreach (char c in LastSql)
					if (c == ':') parmCount++;
				DebugLog.Message("CommandId = " + Id + ", Long Read Time = " + FT(LastReadTime) + ", Parms = " + parmCount + ", Sql = " + Lex.RemoveLineBreaksAndTabs(LastSql)); // debug
			}

			TotalReadTime += LastReadTime;
			AverageReadTime = TotalReadTime / ReadCount;

			GlobalReadCount++;
			GlobalLastReadTime = LastReadTime;
			GlobalTotalReadTime += LastReadTime;

			//if (Lex.Contains(LastSql, "CompoundId=")) LastSql = LastSql; // debug

			return true;
		}

		/// <summary>
		/// Method called on new thread to perform a cancelable Read
		/// </summary>

		public void CancellableRead()
		{
			Cancelled = false;
			CancellableOpException = null;

			try
			{
				ReadResult = Rdr.Read();
				ReadComplete = true;
			}
			catch (Exception ex) // catch any cancel operation
			{
				if (Lex.Contains(ex.Message, "ORA-01013")) // Is the expected Oracle cancel message included? i.e.: "ORA-01013: user requested cancel of current operation"
					Cancelled = true;

				else CancellableOpException = ex; // store other exceptions for pickup by main thread
			}

			return;
		}

/// <summary>
/// CloseReader
/// </summary>

		public void CloseReader ()
		{
			if (LogDatabaseCalls) LogDatabaseCall();

			if (Rdr != null && !Rdr.IsClosed)
			{
				Rdr.Close();
				GlobalCloseReaderCount++;

				if (LogDbCommandDetail) // || Security.IsDeveloper) // debug 
				{
					string msg = "CommandId = " + Id + ", SQL: " + Lex.RemoveLineBreaksAndTabs(LastSql) + "\r\n" + FormatCommandStats();
					DebugLog.Message(msg);
				}
			}
		}

		public string FormatCommandStats()
		{
			long ms = 0, totalMs = 0;
			double sec = 0, totalSec = 0, totalMin = 0;

			ms = (long)TotalReadTime;
			sec = ms / 1000.0;

			totalMs = (long)(PrepareReaderTime + ExecuteReaderTime + TotalReadTime);
			totalSec = totalMs / 1000.0;
			totalMin = totalSec / 60.0;

			string msg = "Prepares: " + PrepareReaderCount + ", Time (ms): " + (int)PrepareReaderTime;
			msg += "\r\n" + "Executes: " + ExecuteReaderCount + ", Time (ms): " + (int)ExecuteReaderTime + ", Avg Time (ms): " + (ExecuteReaderCount > 0 ? (int)(ExecuteReaderTime / ExecuteReaderCount) : 0);
			msg += "\r\n" + "Reads: " + ReadCount + ", Time(ms): " + ms + ", Rows/Sec: " + (sec > 0 ? (int)(ReadCount / sec) : 0);
			msg += "\r\n" + "Total Rows: " + ReadCount + ", Total Time (min): " + String.Format("{0:f2}", totalMin) + ", Total Rows/Sec: " + (totalSec > 0 ? (int)(ReadCount / totalSec) : 0);

			return msg;
		}

		/// <summary>
		/// LogParameters
		/// </summary>
		/// <param name="parmValues"></param>

		void GetParmTypes(object[] parmValues, out int count, out string types)
		{
			count = 0;
			types = "";

			StringBuilder sb0 = new StringBuilder();
			StringBuilder sb = new StringBuilder();
			string txt;

			try
			{

				if (parmValues != null && parmValues.Length == 0) return;

				ParameterCount = parmValues.Length;

				for (int pi = 0; pi < parmValues.Length; pi++)
				{
					if (ParameterValues == "") // include parameter names & types
					{
						if (pi > 0)
							sb0.Append(",");

						DbParameter p = Cmd.Parameters[pi];
						sb0.Append(p.ParameterName + " " + p.DbType);
					}

					if (pi > 0)
						sb.Append(",");

					object o = parmValues[pi];
					if (o != null) txt = o.ToString();
					else txt = "";
					if (txt.Contains(",")) txt = Lex.AddDoubleQuotes(txt);
					sb.Append(txt);
				}

				if (ParameterValues == "") // include parameter names & types
				{
					sb0.Append("\r\n");
					ParameterValues += sb0.ToString();
				}

				sb.Append("\r\n");
				ParameterValues += sb.ToString();

				return;
			}

			catch (Exception Exception) { return; } // ignore
		}

/// <summary>
/// Log reader database call
/// </summary>

		void LogDatabaseCall()
		{
			LogDatabaseCall("Reader");
		}

/// <summary>
/// LogDatabaseCall
/// </summary>

		void LogDatabaseCall(string commandType)
		{
			try
			{
				if ( // skip some stuff
					!LogDatabaseCalls ||
					Cmd == null ||
					ExecuteReaderCount == 0 ||
					Lex.Contains(LastSql, "MBS_USR_OBJ") ||
					Lex.Contains(LastSql, "MBS_LOG") ||
					Lex.Contains(LastSql, "FROM DUAL")) return;

				string logFile = ServicesDirs.LogDir + @"\DatabaseCalls - [Date].log";
				string msg = "";

				Stopwatch sw = Stopwatch.StartNew();

				string sql =  Lex.RemoveLineBreaksAndTabs(LastSql);
				string txt = DatabaseName + " " + DatabaseAcct + " " + sql; // string to hash
				string hash = Lex.ComputeMD5Hash(txt);

				DateTime now = DateTime.Now;
				string dtString = now.ToShortDateString() + " " + now.ToLongTimeString();
				string process = Process.GetCurrentProcess().Id.ToString();
				string username = Security.UserName;

				lock (SqlHash)
				{

// Log sql statement if needed

					if (!SqlHash.Contains(hash))
					{
						SqlHash.Add(hash);

						SerializeParameterTypes();

						msg =
							"<SqlDef DT='&dt' SqlId='&sqlId' Process='&process' Username='&username' DB='&db' Acct='&acct' ParmCnt='&parmCnt' Parms='&parms' >\r\n" +
							"<![CDATA[&sql]]> </SqlDef>\r\n";

						msg = msg.Replace('\'', '\"');
						msg = msg.Replace("&dt", dtString);
						msg = msg.Replace("&sqlId", hash);
						msg = msg.Replace("&process", process);
						msg = msg.Replace("&username", username);
						msg = msg.Replace("&db", DatabaseName);
						msg = msg.Replace("&acct", DatabaseAcct);
						msg = msg.Replace("&parmCnt", ParameterCount.ToString());
						msg = msg.Replace("&parms", ParameterTypes);
						msg = msg.Replace("&sql", sql);

						LogDatabaseCallMsg(logFile, msg);
					}

// Log call stats

					msg =
						"<SqlExec DT='&dt' SqlId='&sqlId' Process='&process' Username='&username' ReadCnt='&readCnt' ReadTime='&readTime' ParmCnt='&parmCnt' ParmVals='&parmVals' />\r\n";

					if (commandType == "NonReader") // adjust for nonreader
					{
						msg = Lex.Replace(msg, "ReadCnt=", "NonReaderCnt="); 
						ReadCount = ExecuteNonReaderRowCount;

						msg = Lex.Replace(msg, "ReadTime=", "NonReaderTime="); 
						ExecuteReaderTime = ExecuteNonReaderTime;
					}

					string readTime = (ExecuteReaderTime + TotalReadTime).ToString();

					SerializeParameterValues();

					msg = msg.Replace('\'', '\"');
					msg = msg.Replace("&dt", dtString);
					msg = msg.Replace("&sqlId", hash);
					msg = msg.Replace("&process", process);
					msg = msg.Replace("&username", username);
					msg = msg.Replace("&readCnt", ReadCount.ToString());
					msg = msg.Replace("&readTime", readTime);
					msg = msg.Replace("&parmCnt", ParameterCount.ToString());

					msg = msg.Replace("&parmVals", ParameterValues);

					LogDatabaseCallMsg(logFile, msg);

					ExecuteReaderCount = 0; // clear stats
					ExecuteReaderTime = 0;
					ReadCount = 0;
					LastReadTime = 0;
					LastLongReadTime = 0;
					TotalReadTime = 0;
				} // end of lock

				LogDatabaseCallsTime += sw.Elapsed.TotalMilliseconds;
				return;
			}

			catch (Exception Exception) { return; } // ignore
		}

/// <summary>
/// SerializeParameterTypes
/// </summary>

		void SerializeParameterTypes()
		{
			DbParameterCollection parms = Cmd.Parameters;

			StringBuilder sb = new StringBuilder();
			string txt;

			ParameterCount = 0;
			ParameterTypes = "";

			if (parms == null || parms.Count == 0) return;

			ParameterCount = parms.Count;

			for (int pi = 0; pi < parms.Count; pi++)
			{
				if (pi > 0)	sb.Append(",");

				DbParameter p = parms[pi];
				sb.Append(p.ParameterName + " " + p.DbType);
			}

			ParameterTypes = sb.ToString();
			return;
		}


/// <summary>
		/// SerializeParameterValues
/// </summary>

		void SerializeParameterValues()
		{
			DbParameterCollection parms = Cmd.Parameters;

			StringBuilder sb = new StringBuilder();
			string txt;

			ParameterCount = 0;
			ParameterValues = "";

			if (parms == null || parms.Count == 0) return;

			ParameterCount = parms.Count;

			for (int pi = 0; pi < parms.Count; pi++)
			{
				if (pi > 0)	sb.Append(",");

				DbParameter p = parms[pi];

				object o = p.Value;
				if (o != null) txt = o.ToString();
				else txt = "";
				if (txt.Contains(",")) txt = Lex.AddDoubleQuotes(txt);
				sb.Append(txt);
			}

			ParameterValues = sb.ToString();
			return;
		}

/// <summary>
/// LogDatabaseCallMsg
/// </summary>
/// <param name="logFileName"></param>
/// <param name="msg"></param>

		void LogDatabaseCallMsg(string logFileName, string msg)
		{
			StreamWriter sw = null;

			try
			{
				logFileName = LogFile.GetDatedLogFileName(logFileName);
				sw = new StreamWriter(logFileName, true);
				sw.WriteLine(msg);
				sw.Close();
			}

			catch (Exception ex) { FileUtil.CloseStreamWriter(sw); }

			return;
		}

		/// <summary>
		/// /// Prepare and execute nonreader sql
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterType"></param>
		/// <param name="parmValue"></param>
		/// <returns></returns>

		public static int PrepareAndExecuteNonReader(
			string sql,
			OracleDbType[] parameterType,
			object[] parmValue)
		{
			int count = 0;
			DbCommandMx cmd = new DbCommandMx();

			try
			{
				cmd.Prepare(sql, parameterType);
				count = cmd.ExecuteNonReader(parmValue);
			}
			finally { cmd.Dispose(); }
			return count;
		}


		/// <summary>
		/// Prepare and execute nonreader sql
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static int PrepareAndExecuteNonReaderSql(string sql)
		{
			int count = 0;
			DbCommandMx cmd = new DbCommandMx();

			try
			{
				cmd.Prepare(sql);
				count = cmd.ExecuteNonReader();
			}
			finally { cmd.Dispose(); }
			return count;
		}

		/// <summary>
		/// Prepare and execute single-parameter nonreader sql
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameterType"></param>
		/// <param name="parmValue"></param>
		/// <returns></returns>

		public static int PrepareAndExecuteNonReader(
			string sql,
			OracleDbType parameterType,
			object parmValue)
		{
			int count = 0;
			DbCommandMx drDao = new DbCommandMx();

			try
			{
				drDao.Prepare(sql, parameterType);
				count = drDao.ExecuteNonReader(parmValue);
			}
			finally { drDao.Dispose(); }
			return count;
		}

/// <summary>
/// Prepare and execute non reader
/// </summary>
/// <param name="sql"></param>
/// <returns></returns>

		public int PrepareAndExecuteNonReader(
			string sql,
			bool logExceptions = true)
		{
			Prepare(sql);
			return ExecuteNonReader(logExceptions);
		}

		/// <summary>
		/// ExecuteNonReader
		/// </summary>
		/// <returns></returns>

		public int ExecuteNonReader(
			bool logExceptions = true)
		{
			object [] parmValues = null;
			return ExecuteNonReader(parmValues, logExceptions);
		}

		public int ExecuteNonReader(
			object parmValue,
			bool logExceptions = true)
		{
			object [] parmValues = new object[1];
			parmValues[0] = parmValue;
			return ExecuteNonReader(parmValues, logExceptions);
		}

		public int ExecuteNonReader(
			object [] parmValues,
			bool logExceptions = true)
		{
			int count = 0;

			if (parmValues!=null) // set parameter values
			{
				for (int pi=0; pi<parmValues.Length; pi++) 
					Cmd.Parameters[pi].Value = parmValues[pi]; 
			}

			Stopwatch sw = Stopwatch.StartNew();
			try 
			{
				count = Cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				if (logExceptions)
				{
					string msg = "DbCommandMx.ExecuteNonReader Error:\r\n" +
						ex.Message + "\r\n\r\n" +
						"Sql: " + OracleDao.FormatSql(LastSql);
					LogException(DebugLog.FormatExceptionMessage(ex, msg));
					throw new Exception (ex.Message, ex);
				}
			}

			ExecuteNonReaderRowCount = count; 
			GlobalExecuteNonReaderCount += count;

			int tDelta = (int)sw.ElapsedMilliseconds;
			ExecuteNonReaderTime = tDelta;
			GlobalLastExecuteNonReaderTime = tDelta;
			GlobalTotalExecuteReaderTime += tDelta;

			if (LogDbCommandDetail) 
				DebugLog.Message("ExecuteNonReader: RowCount = " + ExecuteNonReaderRowCount + ", Time = " + FT(tDelta) + ", Sql = " + Lex.RemoveLineBreaksAndTabs(LastSql));

			//if (LogDatabaseCalls) // log NonReader database call (disabled)
			//  LogDatabaseCall("NonReader");

			return count;
		}

		/// <summary>
		/// Execute insert/update/delete on an array of rows
		/// </summary>
		/// <param name="rowList"></param>
		/// <returns></returns>
		/// 
		public int ExecuteArrayNonReader(
			List<object[]> rowList)
		{
			int rowCount = rowList.Count;
			if (rowCount == 0) return 0;
			int colCount = rowList[0].Length;

			object[][] pva = NewObjectArrayArray(colCount, rowCount);
			for (int ci = 0; ci < colCount; ci++)
			{
				for (int ri = 0; ri < rowCount; ri++)
				{
					pva[ci][ri] = rowList[ri][ci]; // invert
				}
			}

			return ExecuteArrayNonReader(pva, ref rowCount);
		}

		/// <summary>
		/// Execute insert/update/delete on an array of rows
		/// </summary>
		/// <param name="drd"></param>
		/// <param name="pva"></param>
		/// <param name="pvaCount"></param>
		/// <returns></returns>

		public int ExecuteArrayNonReader(
			object[][] pva,
			ref int pvaCount)
		{
			if (pvaCount <= 0) return 0;

			OracleCmd.ArrayBindCount = pvaCount;
			int count = ExecuteNonReader(pva); // do insert
			pvaCount = 0;
			return count;
		}

		/// <summary>
		/// Allocate array of arrays for fast data insert
		/// </summary>
		/// <param name="dim1"></param>
		/// <param name="dim2"></param>
		/// <returns></returns>

		public static object[][] NewObjectArrayArray(
			int dim1,
			int dim2)
		{
			object[][] aa = new object[dim1][]; // build array pointing to array for each parm
			for (int ai = 0; ai < aa.Length; ai++)
				aa[ai] = new object[dim2];

			return aa;
		}

/// <summary>
/// Begin transaction
/// </summary>

		public void BeginTransaction()
		{
			if (MxConn == null) return;
			MxConn.BeginTransaction();
		}

/// <summary>
/// Commit transaction
/// </summary>

		public void Commit()
		{
			if (MxConn == null) return;
			MxConn.Commit();
		}

/// <summary>
/// Rollback transaction
/// </summary>

		public void Rollback()
		{
			if (MxConn == null) return;
			MxConn.Rollback();
		}

/// <summary>
/// Prepare a query that takes a list as part of criteria
/// </summary>
/// <param name="sql"></param>
/// <param name="parameterType"></param>

		public void PrepareListReader (
			string sql,
			DbType parameterType)
		{
			PrepareListReader(sql, parameterType, DefaultKeyListPredType);
			return;
		}

/// <summary>
/// Prepare a query that takes a list as part of criteria
/// </summary>
/// <param name="sql"></param>
/// <param name="parameterType"></param>
/// <param name="keyPredType"></param>

		public void PrepareListReader(
			string sql,
			DbType parameterType,
			KeyListPredTypeEnum keyListPredType)
		{
			ListSql = sql;
			KeyListParameterType = parameterType;
			KeyListPredType = keyListPredType;
			return;
		}

/// <summary>
/// Open a list reader
/// </summary>
/// <param name="list"></param>

		public void ExecuteListReader (
			List<string> list)
	{
			AssertMx.IsNotNull(ListSql, "ListSql");
			KeyList = list;
			LastKeyCount = LastKeyIdx = -1;
			if (list==null || list.Count<=0) return;
			PrepareNextListChunk();
	}

/// <summary>
/// Read next row from list
/// </summary>
/// <returns></returns>

		public bool ListRead ()
		{
			if (LastKeyCount < 0) return false;
			if (this.Read()) return true;
			if (Cancelled) return false;
			if (LastKeyIdx >= KeyList.Count - 1) return false;
			PrepareNextListChunk();
			return ListRead();
		}

/// <summary>
/// Prepare & execute sql for next chunk of list entries
/// </summary>

		void PrepareNextListChunk()
		{
			object[] keyParmArray = null;
			bool intKey;
			string sql2 = "";
			long longKeyVal;
			int intKeyVal, keyCount, i1;

			Stopwatch sw = Stopwatch.StartNew();

			FirstKeyIdx = LastKeyIdx + 1; // advance key index

			if (KeyListPredType == KeyListPredTypeEnum.DbList)
			{ // do full list in single step
				FirstKeyIdx = 0;
				LastKeyIdx = KeyList.Count - 1;
				keyCount = KeyList.Count;

				if (IsNumericDbType(KeyListParameterType)) intKey = true;
				else intKey = false;

				sql2 = ListSql;
				string listExpr = BuildTempDbTableKeyListPredicate(ref sql2, null, intKey, KeyList, FirstKeyIdx, keyCount);

				sql2 = Lex.Replace(sql2, "<list>", listExpr);
				Prepare(sql2);

				ExecuteReader(); // run the query
			}

// Subsetting, select next chunk of keys

			else
			{
				int keyBlockSize = DbCommandMx.MaxOracleInListItemCount;
				if (SingleKeyRetrieval) keyBlockSize = 1;

				LastKeyIdx = FirstKeyIdx + (keyBlockSize - 1);
				if (LastKeyIdx >= KeyList.Count) // past end
					LastKeyIdx = KeyList.Count - 1;
				keyCount = LastKeyIdx - FirstKeyIdx + 1;

				if (KeyListPredType == KeyListPredTypeEnum.Parameterized)
				{
					if (keyCount != LastKeyCount) // need to prepare sql?
					{
						string parmList = "";
						for (i1 = 0; i1 < keyCount; i1++) // build parameters
						{
							parmList += ":" + i1; 
							if (i1 < keyCount - 1)
							{
								parmList += ",";
							}
						}

						sql2 = Lex.Replace(ListSql, "<list>", parmList);
						Prepare(sql2, KeyListParameterType, keyCount);
					}

					keyParmArray = new object[keyCount]; // array of parameter values

					if (KeyListParameterType == DbType.Int64)
					{
						for (i1 = 0; i1 < keyCount; i1++) // copy keys to parameter array properly normalized
						{
							if (Int64.TryParse(KeyList[FirstKeyIdx + i1], out longKeyVal))
								keyParmArray[i1] = longKeyVal;
						}
					}

					if (KeyListParameterType == DbType.Int32)
					{
						for (i1 = 0; i1 < keyCount; i1++) // copy keys to parameter array properly normalized
						{
							if (Int32.TryParse(KeyList[FirstKeyIdx + i1], out intKeyVal))
								keyParmArray[i1] = intKeyVal;
						}
					}

					else if (KeyListParameterType == DbType.String)
					{
						for (i1 = 0; i1 < keyCount; i1++) // copy keys to parameter array properly normalized
							keyParmArray[i1] = KeyList[FirstKeyIdx + i1];
					}

					else throw new Exception("Unexpected KeyListParameterType: " + KeyListParameterType);

					ExecuteReader(keyParmArray); // run the query
				}

				else // use literal lists
				{
					StringBuilder sb = new StringBuilder();
					for (i1 = 0; i1 < keyCount; i1++) // build parameters
					{
						sb.Append(Lex.AddSingleQuotes(KeyList[FirstKeyIdx + i1]));
						if (i1 < keyCount - 1)
						{
							sb.Append(",");
						}
					}

					sql2 = Lex.Replace(ListSql, "<list>", sb.ToString());
					Prepare(sql2);
					ExecuteReader(); // run the query
				}
			}

			LastKeyCount = keyCount;

			if (LogDbCommandDetail)
			{
				string keyPredType = KeyListPredType.ToString() +  "(" + keyCount + ")";
				int tDelta = (int)sw.Elapsed.TotalMilliseconds;
				DebugLog.Message(this.GetType().Name + " Execute - KeyListPredType: " + keyPredType + ", time: " + FT(tDelta) + ", sql: " + Lex.RemoveLineBreaksAndTabs(sql2));
			}

			if (Cancelled)
				//					Eqp.Cancelled = true;
				return;
		}

/// <summary>
/// Format time for easy id of magnitude <1>, <<10>>, <<<100>>>, <<<<1000>>>>, <<<<<10000>>>>>,
/// </summary>
/// <param name="t"></param>
/// <returns></returns>

		string FT(double t)
		{
			if (t < 10) return "<" + t + ">";
			else if (t < 10) return "<<" + t + ">>";
			else if (t < 100) return "<<<" + t + ">>>";
			else if (t < 1000) return "<<<<" + t + ">>>>";
			else return "<<<<<<" + t + ">>>>>";
		}

/// <summary>
/// Build in list predicate using temporary database table
/// </summary>
/// <param name="baseSql"></param>
/// <param name="keyName"></param>
/// <param name="keyList"></param>
/// <param name="firstKeyIdx"></param>
/// <param name="keyCount"></param>
/// <returns></returns>

		public string BuildTempDbTableKeyListPredicate(
			ref string baseSql,
			string keyName,
			bool intKey,
			List<string> keyList,
			int firstKeyIdx,
			int keyCount)
		{
			//if (QueryEngine.AllowNetezzaUse && eqp.AllowNetezzaUse) // && keyCount > MaxNetzzaInListItemCount)
			//  return DbCommandMx.BuildNetezzaTempDbTableKeyListPredicate(keyName, keyList, firstKeyIdx, keyCount);

			//else 
			return BuildOracleTempDbTableKeyListPredicate(ref baseSql, keyName, intKey, keyList, firstKeyIdx, keyCount);
		}

		/// <summary>
		/// Build a key list predicate that uses a temporary database table to hold the keys
		/// </summary>
		/// <param name="keyName"></param>
		/// <param name="keyList"></param>
		/// <param name="firstKeyIdx"></param>
		/// <param name="keyCount"></param>
		/// <returns></returns>

		public static string BuildOracleTempDbTableKeyListPredicate(
			ref string baseSql,
			string keyName,
			bool intKey,
			List<string> keyList,
			int firstKeyIdx,
			int keyCount)
		{
			string dsName, listName, keyColName, hint, txt, pred, keyName2, matchString;
			int i1, i2;

			Stopwatch sw = Stopwatch.StartNew();

			string sqlCopy = baseSql;
			DbConnectionMx mxConn = DbConnectionMx.MapSqlToConnection(ref sqlCopy); // get connection name from sql
			dsName = mxConn.SessionConn.DataSource.Name;

			if (intKey) keyColName = "intKey";
			else keyColName = "stringKey";

			listName = "TempKeyList1"; // todo: generalize
			string schemaAndTableName = TempDbList.Write(dsName, listName, keyList, firstKeyIdx, keyCount, intKey);

			string sql = @"
				select <keyColName> 
				from <schemaAndTableName>";

			sql = sql.Replace("<keyColName>", keyColName);
			sql = sql.Replace("<schemaAndTableName>", schemaAndTableName);

			if (!Lex.IsNullOrEmpty(keyName))
				sql = keyName + " in (" + sql + ")";

			hint = "cardinality(" + schemaAndTableName + "," + keyCount + ") ";

			string hintMatch = "/*+ first_rows";
			if (!Lex.Contains(baseSql, hintMatch))
				hintMatch = "/*+ ";

			if (Lex.Contains(baseSql, hintMatch))
				baseSql = Lex.Replace(baseSql, hintMatch, "/*+ " + hint);

			// Insert list criteria at other places specified in sql comments

			while (true)
			{
				string prefix = "/* AndIncludeKeySubsetOn(";

				i1 = Lex.IndexOf(baseSql, prefix);
				if (i1 < 0) break;

				txt = baseSql.Substring(i1 + prefix.Length);
				i2 = Lex.IndexOf(txt, "*/");
				if (i2 < 0) break;

				matchString = baseSql.Substring(i1, prefix.Length + i2 + 2);

				keyName2 = txt.Substring(0, i2);
				keyName2 = keyName2.Replace(")", "").Trim();

				pred = " and " + sql.Replace("<keyName>", keyName2);
				baseSql = Lex.Replace(baseSql, matchString, pred);
			}

			DbListTotalCount++;

			DbListTotalSize += keyCount;
			DbListAvgSize = (int)(DbListTotalSize / DbListTotalCount);
			if (keyCount > DbListMaxSize) DbListMaxSize = keyCount;

			DbListTotalTime += sw.Elapsed.TotalMilliseconds;
			DbListAvgTime = DbListTotalTime / DbListTotalCount;

			return sql;
		}

		/// <summary>
		/// Build Netezza list predicate that uses an anonymous external table
		/// </summary>
		/// <param name="keyName"></param>
		/// <param name="keyList"></param>
		/// <param name="firstKeyIdx"></param>
		/// <param name="keyCount"></param>
		/// <returns></returns>

		public static string BuildNetezzaTempDbTableKeyListPredicate(
			string keyName,
			List<string> keyList,
			int firstKeyIdx,
			int keyCount)
		{
			string sql = @"
				<keyName> in (
				select keyColName from
				EXTERNAL '<fileName>'
				(keyColName int)
				USING
				(
					SKIPROWS 0
					MAXROWS 0
					MAXERRORS 3
					DELIMITER ','
					DATESTYLE 'MDY'
					TIMESTYLE '12HOUR'
					Y2BASE 2000
					DATEDELIM '/'
					TIMEDELIM ':'
					QUOTEDVALUE 'DOUBLE'
					NULLVALUE 'null'
					TRUNCSTRING TRUE
					LOGDIR 'c:\\netezza\\log'
					ENCODING 'internal'
					REMOTESOURCE 'ODBC'
					CRINSTRING FALSE
					CTRLCHARS FALSE
					IGNOREZERO FALSE
					));";

			string fileName = TempFile.GetTempFileName("txt");
			StreamWriter sw = new StreamWriter(fileName);
			for (int ki = firstKeyIdx; ki < firstKeyIdx + keyCount; ki++)
			{
				sw.WriteLine(keyList[ki]);
			}

			sw.Close();
			sql = sql.Replace("<keyName>", keyName);
			sql = sql.Replace("<fileName>", fileName);
			return sql;
		}

		public bool CancelTest()
		{
			bool result;
			
			Rdr = Cmd.ExecuteReader();

			//			if (Rdr.Read())
			//				result=true;
			//			else result=false;

			ReadComplete = false;
			ThreadStart ts = new ThreadStart(CancellableRead);
			Thread t1 = new Thread(ts);
			t1.Name = "CancelReadTest";
      t1.IsBackground = true;
			t1.SetApartmentState(ApartmentState.STA);
			t1.Start();

			Thread.Sleep(1000);
			if (ReadComplete) return ReadResult;
			Cmd.Cancel(); // not complete, cancel it
			return false;
		}

/// <summary>
/// See if a reader value is null by name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public bool IsNullByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name

			return IsNull(pos);
		}

		/// <summary>
		/// See if a reader value is null by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public bool IsNull (
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return true;
			else return false;
		}

		/// <summary>
		/// Get an object value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public object GetObjectByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetObject(pos);
		}

		/// <summary>
		/// Get an object value by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public object GetObject (
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return null;
			return Rdr.GetValue(pos);
		}

		/// <summary>
		/// Get a string value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public string GetStringByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetString(pos);
		}

		/// <summary>
		/// Get string value by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public string GetString (
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return "";
			return Rdr.GetString(pos);
		}


		/// <summary>
		/// Get a clob value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public string GetClobByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetClob(pos);
		}

		/// <summary>
		/// Get clob value by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public string GetClob(
			int pos)
		{
			string value = "";
			if (Rdr.IsDBNull(pos)) return "";

			Stopwatch sw = Stopwatch.StartNew();
			if (IsOracle)
			{
				OracleClob ol = OracleRdr.GetOracleClob(pos);
				value = ol.Value.ToString();
			}

			else value = Rdr.GetValue(pos).ToString();

			double ms = sw.Elapsed.TotalMilliseconds;
			return value;
		}

		/// <summary>
		/// Get a binary value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public byte[] GetBinaryByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetBinary(pos);
		}

		/// <summary>
		/// Get binary value by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public byte[] GetBinary(
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return null;

			if (IsOracle)
			{
				byte[] ba = (byte [])OracleRdr.GetOracleBinary(pos);
				return ba;
			}

			else throw new NotImplementedException();
		}

		/// <summary>
		/// Get a blob value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public byte[] GetBlobByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetBlob(pos);
		}

		/// <summary>
		/// Get blob value by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public byte[] GetBlob(
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return null;

			if (IsOracle)
			{
				OracleBlob ob = OracleRdr.GetOracleBlob(pos);
				byte[] ba = new byte[ob.Length];
				ob.Read(ba, 0, (int)ob.Length);
				return ba;
			}

			else throw new NotImplementedException();
		}

		/// <summary>
		/// Get an int value from the reader by name
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public int GetIntByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name

			return GetInt(pos);
		}

		/// <summary>
		/// Get an int value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public int GetInt (
			int pos)
		{
			int value;

			//if (!IsOracle) IsOracle = IsOracle; // debug
			if (Rdr.IsDBNull(pos)) return NullValue.NullNumber;
			if (IsOracle)
				value = OracleRdr.GetOracleDecimal(pos).ToInt32();
			else
				value =  Convert.ToInt32(Rdr.GetValue(pos));

			return value;
		}

		/// <summary>
		/// Get a long value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public long GetLongByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name

			return GetLong(pos);
		}

		/// <summary>
		/// Get a long value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>

		public long GetLong (
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return NullValue.NullNumber;

			if (IsOracle)
				return OracleRdr.GetOracleDecimal(pos).ToInt64();
			else
				return Convert.ToInt64(Rdr.GetValue(pos));
		}

		/// <summary>
		/// Get a double value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public double GetDoubleByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetDouble(pos);
		}

		/// <summary>
		/// Get a double value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public double GetDouble (
			int pos)

		{
			if (Rdr.IsDBNull(pos)) return NullValue.NullNumber;

			if (IsOracle)
				return OracleRdr.GetOracleDecimal(pos).ToDouble(); // GetOracleDecimal return up to 38 digits where GetDecimal returns 28 digits.

			else
				return Convert.ToDouble(Rdr.GetValue(pos));
		}

		/// <summary>
		/// Get a DateTime value from the reader by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public DateTime GetDateTimeByName (
			string name)
		{
			int pos;
			pos = GetOrdinal(Rdr, name); // throws exception if bad name
			return GetDateTime(pos);
		}

/// <summary>
/// Get ordinal from name with adding name to any exception
/// </summary>
/// <param name="rdr"></param>
/// <param name="name"></param>
/// <returns></returns>

		public int GetOrdinal(
			DbDataReader rdr,
			string name)
		{
			try
			{
				int pos = rdr.GetOrdinal(name);
				return pos;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message + " (ColumnName: " + name + ")", ex);
			}
		}


		/// <summary>
		/// Get a DateTime value from the reader by position
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public DateTime GetDateTime (
			int pos)
		{
			if (Rdr.IsDBNull(pos)) return DateTime.MinValue;
			return Rdr.GetDateTime(pos);
		}

		/// <summary>
		/// Dispose of the command
		/// </summary>

		public void Dispose ()
		{
			if (LogDatabaseCalls) LogDatabaseCall();

			if (Rdr != null)
			{
				if (!Rdr.IsClosed)
					CloseReader();

				Rdr.Dispose();
			}

			if (Cmd != null)
			{
				Cmd.Dispose();
				Cmd = null;
			}

			if (MxConn != null)
			{
				MxConn.Close();
				MxConn = null;
			}
		}

		/// <summary>
		/// Do temp fixups to sql
		/// </summary>
		/// <param name="?"></param>

		string FixupSql(
			string sql)
		{
			//sql = FixupSql(sql, "mbs_owner.mbs_adw_rslt", "(select cmpnd_id ext_cmpnd_id_txt, sbstnc_id ext_cmpnd_id_nbr, t0.* from mbs_owner.mbs_adw_rslt t0)");
			//sql = FixupSql(sql, "mbs_owner.mbs_cpdw_rslt", "(select cmpnd_id ext_cmpnd_id_txt, sbstnc_id ext_cmpnd_id_nbr, t0.* from mbs_owner.mbs_cpdw_rslt t0)");
			//sql = FixupSql(sql, "mbs_owner.mbs_pbchm_rslt", "(select cmpnd_id ext_cmpnd_id_txt, sbstnc_id ext_cmpnd_id_nbr, t0.* from mbs_owner.mbs_pbchm_rslt t0)");
			return sql;
		}

		string FixupSql(
			string sql,
			string match,
			string replace)
		{
			string sql2 = sql;
			int matchCnt = 0;
			while (true)
			{
				int i1 = sql2.IndexOf(match, StringComparison.OrdinalIgnoreCase);
				if (i1 < 0) break;
				if (sql2.Substring(i1).StartsWith(match + "_seq", StringComparison.OrdinalIgnoreCase))
					sql2 = sql2.Substring(0, i1) + "<<^&*>>" + sql2.Substring(i1 + match.Length); // keep as is
				else sql2 = sql2.Substring(0, i1) + "<<!@#>>" + sql2.Substring(i1 + match.Length);
				matchCnt++;
			}

			if (matchCnt == 0) return sql;
			sql2 = sql2.Replace("<<!@#>>", replace);
			sql2 = sql2.Replace("<<^&*>>", match);
			return sql2;
		}

/// <summary>
/// IsNumericDbType
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static bool IsNumericDbType(DbType type)
		{
			return IsIntegerDbType(type) || IsNonintegerNumericDbType(type);
		}

/// <summary>
/// IsIntegerDbType
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static bool IsIntegerDbType(DbType type)
		{
			return type == DbType.Int16 || type == DbType.Int32 ||
				type == DbType.Int64;
		}

/// <summary>
/// IsNonintegerNumericDbType
/// </summary>
/// <param name="type"></param>
/// <returns></returns>

		public static bool IsNonintegerNumericDbType(DbType type)
		{
			return type == DbType.Decimal || type == DbType.Double ||
				type == DbType.Single;
		}

/// <summary>
/// Convert OracleDbType to DbType
/// </summary>
/// <param name="oraDbType"></param>
/// <returns></returns>

		public static DbType OracleDbTypeToDbType(OracleDbType oraDbType)
		{
			switch (oraDbType)
			{
				case OracleDbType.Varchar2:
				case OracleDbType.Char:
				case OracleDbType.NChar:
				case OracleDbType.NClob:
				case OracleDbType.XmlType:
					return DbType.String;

				case OracleDbType.Int16:
					return DbType.Int16;
				case OracleDbType.Int32:
					return DbType.Int32;
				case OracleDbType.Int64:
					return DbType.Int64;
				case OracleDbType.Single:
					return DbType.Single;
				case OracleDbType.Double:
					return DbType.Double;
				case OracleDbType.Decimal:
					return DbType.Decimal;
				case OracleDbType.Date:
				case OracleDbType.TimeStamp:
				case OracleDbType.TimeStampTZ:
				case OracleDbType.TimeStampLTZ:
					return DbType.DateTime;
				case OracleDbType.IntervalDS:
					return DbType.Time;
				case OracleDbType.IntervalYM:
					return DbType.Int32;
				case OracleDbType.Long:
				case OracleDbType.Clob:
					return DbType.String;
				case OracleDbType.Raw:
				case OracleDbType.LongRaw:
				case OracleDbType.Blob:
				case OracleDbType.BFile:
					return DbType.Binary;
				case OracleDbType.RefCursor:
				case OracleDbType.Array:
				case OracleDbType.Object:
				case OracleDbType.Ref:
					return DbType.Object;
				default:
					throw new NotSupportedException();
			}
		}

/// <summary>
/// Convert DbType to OracleDbType
/// </summary>
/// <param name="dbType"></param>
/// <returns></returns>

		internal static OracleDbType DbTypeToOracleDbType(DbType dbType)
		{
			switch (dbType)
			{
				case DbType.AnsiString:
				case DbType.String:
					return OracleDbType.Varchar2;
				case DbType.AnsiStringFixedLength:
				case DbType.StringFixedLength:
					return OracleDbType.Char;
				case DbType.Byte:
				case DbType.Int16:
				case DbType.SByte:
				case DbType.UInt16:
				case DbType.Int32:
					return OracleDbType.Int32;
				case DbType.Single:
					return OracleDbType.Single;
				case DbType.Double:
					return OracleDbType.Double;
				case DbType.Date:
					return OracleDbType.Date;
				case DbType.DateTime:
					return OracleDbType.TimeStamp;
				case DbType.Time:
					return OracleDbType.IntervalDS;
				case DbType.Binary:
					return OracleDbType.Blob;
				case DbType.Boolean:
					return OracleDbType.Char;
				case DbType.Int64:
				case DbType.UInt64:
				case DbType.VarNumeric:
				case DbType.Decimal:
				case DbType.Currency:
					return OracleDbType.Decimal;
				case DbType.Object:
					return OracleDbType.Object;
				case DbType.Guid:
					return OracleDbType.Raw;
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Log exception if turned on globally
		/// </summary>
		/// <param name="msg"></param>

		static void LogException(string msg)
		{
			if (LogExceptions)
				DebugLog.Message(msg);
		}
	}
	
	/// <summary>
	/// The caller implements this interface to allow checking for user cancel 
	/// of a running query.
	/// </summary>

	public interface ICheckForCancel
	{
		bool IsCancelRequested
		{
			get;
		}
	}

/// <summary>
/// Form of sql predicate to use for key lists
/// </summary>

	public enum KeyListPredTypeEnum
	{
		Parameterized = 0, // Parameterized lists, e.g. keyCol in (:0, :1, :2, ...) 
		Literal = 1, // Literal lists, e.g. keyCol in (123, 234, 345, ...)
		DbList = 2 // Lists stored in temp database tables, e.g. keyCol in (select intKey from TempKeyList1)
	}

#if false
	public class MyDataReader : IDataReader

     {

         SqlDataReader sourceReader = null;

         DataTable schemaTable = null;

         Dictionary<int, string> userInfo = new Dictionary<int, string>(); //save session and userinfos.

         int currentSPID = -1, currentSPIDOrdinal, userInfoOrdinal, eventClassOrdinal, textDataOrdinal;

         const string UserInfoColumnName = "UserInformation";

         public MyDataReader(SqlDataReader r)

         {

             sourceReader = r;

             schemaTable = r.GetSchemaTable(); //this gets the structure of the source schema information

             userInfoOrdinal = schemaTable.Rows.Count;

             DataRow row = schemaTable.NewRow();

             row.BeginEdit();

             row["ColumnName"] = UserInfoColumnName;

             row["ColumnOrdinal"] = userInfoOrdinal;

             row["ColumnSize"] = 128;

             row["NumericPrecision"] = 128; //we don't care

             row["NumericScale"] = 128;//we don't care

             row["IsUnique"] = false;

             row["IsKey"] = false;

             row["DataType"] = typeof(string);

             row["AllowDBNull"] = true; //in case there is procedure calls can't be linked to user information

             row["ProviderType"] = SqlDbType.NVarChar;

             row["IsIdentity"] = false;

            row["IsAutoIncrement"] = false;

            row["IsRowVersion"] = false;

            row["IsLong"] = false;

            row["IsReadOnly"] = false;

            row["ProviderSpecificDataType"] = typeof(SqlString);

            row["DataTypeName"] = "nvarchar";

            row["IsColumnSet"] = false;

            row.EndEdit();

            schemaTable.Rows.Add(row);



            //following code is for performance concern

            currentSPIDOrdinal = sourceReader.GetOrdinal("SPID"); 

            eventClassOrdinal = sourceReader.GetOrdinal("EventClass");

            textDataOrdinal = sourceReader.GetOrdinal("TextData");

        }

        object GetUserInfo()

        {

            if (currentSPID == -1) return DBNull.Value;

            try

            {

                return userInfo[currentSPID];

            }

            catch

            {

                return DBNull.Value;

            }

        }

        //since we have SqlDataReader passed in, things become simple.

	#region implementation of IDataReader

            public void Close()

        {

            //We don't care since there is nothing opened other than passed in SqlDataReader

            return; 

        }

        public DataTable GetSchemaTable()

        {

            return schemaTable; //this has been generated in the Constructor

        }

        //This is called for next result set implementation this is important

        //Let's say your SQL code has returned a set of record then does some 

        //more operations after. If method NextResult is NOT called before

        //the reader getting closed, the T-SQL code after the T-SQL returning

        //set of records will NOT be executed.

        public bool NextResult()

        {

            return sourceReader.NextResult();

        }

        public bool Read()

        {

            //To-Do

            while (sourceReader.Read())

            {

                if (Convert.ToInt16(sourceReader[eventClassOrdinal]) == 82) // this is user defined event

                {

                    userInfo[Convert.ToInt32(sourceReader[currentSPIDOrdinal])] = sourceReader.GetString(textDataOrdinal);

                    continue; // If the record is customized event, the check next record

                    }

                 else if (sourceReader[textDataOrdinal].ToString().IndexOf("sp_trace_generateevent") >= 0)

                     continue; //skip sp_trace_generateevent calls

                 //next record is a procedure call; 

                 //we don't need to do anything here since the implementation of IDataRecord interface has handled this.

                 currentSPID = Convert.ToInt32(sourceReader[currentSPIDOrdinal]);

                 return true;

             }

             return false;

         }

         // This one is not supported and it will not be called by SqlBulkCopy

         public int Depth { get { return sourceReader.Depth; } }

         //Check whether the reader is closed

         public bool IsClosed { get { return sourceReader.IsClosed; } }

         //check how many records affected. Useless, either return zero or the same value in the source reader

         public int RecordsAffected { get { return sourceReader.RecordsAffected; } }

	#endregion



	#region implementation of IDatarecords

         public bool GetBoolean(int i)

         {

             return (bool) this[i];

         }

         public object this[string name]


						{


					get { return name == UserInfoColumnName ? GetUserInfo() : sourceReader[name]; }


			}




			public object this[int i]


			{


					get { return i == userInfoOrdinal ? GetUserInfo() : sourceReader[i]; }


			}




			public byte GetByte(int i) { return (byte)this[i]; }



       //this method copies bytes in the column to a buffer

       public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)

       {

           if (i != currentSPIDOrdinal)

               return sourceReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length); //we still borrow the method in the sourceReader

           // but here I give you the example code, although this will not be called.

           byte[] b = System.Text.ASCIIEncoding.UTF8.GetBytes((string)GetUserInfo());

           if (bufferoffset >= b.Length)

               return 0;

           length = bufferoffset + length <= b.Length? length : b.Length - bufferoffset;

           Array.Copy(b, bufferoffset, buffer, 0, length);

           return length;

       }

       public char GetChar(int i)

       {

           return (char) this[i];

       }

       public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)

       {

           if (i != currentSPIDOrdinal)

               return sourceReader.GetChars(i, fieldoffset, buffer, bufferoffset, length); //we still borrow the method in the sourceReader

           // but here I give you the example code, although this will not be called.

           char[] b = ((string)GetUserInfo()).ToCharArray();

           if (bufferoffset >= b.Length)

               return 0;

           length = bufferoffset + length <= b.Length? length : b.Length - bufferoffset;

           Array.Copy(b, bufferoffset, buffer, 0, length);

           return length;

       }

       //this is not supported in SqlDatareader anyways

       public IDataReader GetData(int i)

       {

           return sourceReader.GetData(i);

       }

            public string GetDataTypeName(int i)

        {

            return (string) schemaTable.Rows[i]["DataTypeName"];

        }

        public DateTime GetDateTime(int i)

        {

            return (DateTime) this[i];

        }

        public decimal GetDecimal(int i)

        {

            return (decimal) this[i];

        }

        public double GetDouble(int i)

        {

            return (double) this[i];

        }

        public Type GetFieldType(int i)

        {

            return (Type) this[i];

        }

        public float GetFloat(int i)

        {

            return (float) this[i];

        }

            public Guid GetGuid(int i)

        {

            return (Guid) this[i];

        }

        public short GetInt16(int i)

        {

            return (short) this[i];

        }

        public int GetInt32(int i)

        {

            return (int) this[i];

        }

        public long GetInt64(int i)

        {

            return (long) this[i];

        }

        public string GetName(int i)

        {

            if (userInfoOrdinal == i)

                return UserInfoColumnName;

            return sourceReader.GetName(i);

        }

        public int GetOrdinal(string name)

         {

             if (name == UserInfoColumnName)

                 return userInfoOrdinal;

             return sourceReader.GetOrdinal(name);

         }

         public string GetString(int i)

         {

             return (string) this[i];

         }

         public object GetValue(int i)

         {

             return this[i];

         }

         public int GetValues(object[] values)

         {

             int maxAllowableLength = Math.Min(values.Length, schemaTable.Rows.Count);

             for (int i = 0; i<maxAllowableLength; i++)

                 values[i] = this[i];

             return maxAllowableLength;

            }

        public bool IsDBNull(int i)

        {

            return this[i] is DBNull;

        }



        //number of field

        public int FieldCount { get { return schemaTable.Rows.Count; } }



         

	#endregion

        //implementation of IDisposable

        public void Dispose()

        {

            //nothing to dispose actually.

            //let's remove the reference

            sourceReader = null;

            userInfo = null;

        }

    }

    }


#endif

#if false
	DbDataReader CreateUndefinedDbDataReader1()
	{
		string path = @"C:\";
		using (OdbcConnection conn = new OdbcConnection(
			"Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=" +
			path + ";Extensions=asc,csv,tab,txt"))
		{
			OdbcCommand cmd =
				new OdbcCommand("SELECT * FROM verylarge.csv", conn))
				{
				conn.Open();

				using (OdbcDataReader dr =
					cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				}
		}
	}
#endif

#if false
		DbDataReader CreateUndefinedDbDataReader()
						{
							string file = "C:\\CMPNAME.csv";
							string dir = Path.GetDirectoryName(file);
							string excelConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
				dir + @";Extended Properties=""Text;HDR=No;FMT=Delimited""";
							OleDbConnection conn = new OleDbConnection(excelConn);
							conn.Open();
							string query = "SELECT * FROM [" + file + "]";

							OleDbCommand cmd = new OleDbCommand(query, conn);
							OleDbDataReader reader = cmd.ExecuteReader();

							while (reader.Read())
							{
								//MessageBox.Show(reader[0].ToString());
							}
						}
#endif


}

