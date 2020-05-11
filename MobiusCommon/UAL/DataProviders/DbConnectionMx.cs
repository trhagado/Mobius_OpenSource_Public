using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;

using Oracle.DataAccess.Client;

using MySql.Data.MySqlClient;

namespace Mobius.UAL 
{
	/// <summary>
	/// Database connection management
	/// Three levels of types of connection are defined in this module
	/// 
	/// 1. DbConnectionMx - Primary requested connection object. This must be returned when
	///    done via a Close. If multiple active statements use the same data source
	///    then multiple DbConnection will exist associated with the same SessionConnection.
	///    
	/// 2. SessionConnection - This represents a single database connection associated
	///    with a session. A counter keeps track of how many times this connection
	///    has been given out to the session and the connection is returned to the pool
	///    when the count reaches zero.
	///    
	/// 3. DataSource - Static information on available data sources.
	/// 
	/// </summary>
	/// 
	public class DbConnectionMx
	{
		/// <summary>
		/// Request connection
		/// </summary>

		public static Dictionary<int, DbConnectionMx> DbConnectionsDict = new Dictionary<int, DbConnectionMx>(); // Active DbConnections
		public static int IdSeq = 0; // number of connections
		public static bool LoggedConnectionLeak; // true if we have logged a connection leak for this session
		public static Thread ConnectionLeakMonitorThread; // thread to check to connection leaks
		public static int ConnectionLeakDetectionInterval = -1; // time in seconds a connection must exist to be considered a leak
		public static bool NoDatabaseAccessIsAvailable = false; // set to true for test mode with no database access
		public static Version OracleClientVersion = null;
		public static Version MySqlClientVersion = null;

		public static bool LogDbConnectionDetail = false;
		public static bool Debug => LogDbConnectionDetail;

		public int Id; // id of this connection

		public DbConnection DbConn; // associated database connection 
		public SessionConnection SessionConn; // associated session connection

		public OracleConnection OracleConn { get { return DbConn as OracleConnection; } }
		public bool IsOracleConn { get { return OracleConn != null; } }

		public MySqlConnection MySqlDbConn { get { return DbConn as MySqlConnection; } }
		public bool IsMySqlConn { get { return MySqlDbConn != null; } }

		public OdbcConnection OdbcDbConn { get { return DbConn as OdbcConnection; } }
		public bool IsOdbcConn { get { return OdbcDbConn != null; } }

		public OleDbConnection OleDbConn { get { return DbConn as OleDbConnection; } }
		public bool IsOleDbConn { get { return OleDbConn != null; } }

		public DateTime CreationTime = DateTime.Now; // when connection was created
		public bool ReportedAsLeak = false; // true if this has been reported as a leak
		public StackTrace StackTrace; // where request came from (for debugging)
		public string LastSql  // Last Sql used to get this connection
		{
			get => _lastSql;
			set => _lastSql = RemoveDatabasePasswords(value); // strip out any passwords before storing/logging, etc.
		}
		string _lastSql = null;

		public DbSchemaMx LastRootSchema = null; // last root schema that was used when getting this connection

		public Dictionary<string, SessionConnection> SessionConnections = new Dictionary<string, SessionConnection>(StringComparer.OrdinalIgnoreCase); // dict of all connections for session indexed by connection name

		private DbConnectionMx() // constructor, should only be called in this module
		{ // use Get(name) for GetFromSql for outside calls
			return;
		}

		/// <summary>
		/// Get Mobius connection by name, connecting to database if needed.
		/// </summary>
		/// <param name="connName"></param>
		/// <returns></returns>

		public static DbConnectionMx GetConnection(
			string connName)
		{
			try
			{
				lock (DbConnectionsDict) // lock in case multiple threads trying to connect
				{
					if (connName == null || connName == "")
						throw new Exception("Missing connection name");

					if (NoDatabaseAccessIsAvailable) connName = "NullDb"; // send all queries to the null database

					connName = connName.ToUpper();

					if (Debug) DebugLog.Message("Get connection " + connName);

					DbConnectionMx conn = new DbConnectionMx();
					IdSeq++;
					conn.Id = IdSeq;
					conn.SessionConn = SessionConnection.Get(connName);
					conn.DbConn = conn.SessionConn.DbConn;
					conn.StackTrace = new StackTrace(true); // for debug

					DbConnectionsDict[IdSeq] = conn; // save in map

					if (IdSeq == 1) // start connection leak-checking loop if requested
					{
						if (ServicesIniFile.IniFile == null)
							throw new Exception("UalUtil.IniFile is null");
						ConnectionLeakDetectionInterval = ServicesIniFile.ReadInt("DbConnectionLeakDetectionInterval", -1);
						if (ConnectionLeakDetectionInterval > 0)
							DbConnectionMx.StartConnectionLeakMonitor(ConnectionLeakDetectionInterval);
					}

					return conn;
				} // end of lock
			} // end of try
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Close of all connections
		/// </summary>

		public static void CloseAll()
		{
			if (Debug) DebugLog.Message("");

			if (DbConnectionsDict == null) return;
			foreach (int id in DbConnectionsDict.Keys)
			{
				DbConnectionMx conn = DbConnectionsDict[id];
				conn.Close();
			}
		}

		/// <summary>
		/// Close connection
		/// </summary>

		public void Close()
		{
			if (SessionConn == null) return;

			lock (DbConnectionsDict)
			{
				if (Debug) DebugLog.Message("Closing DB connection " + SessionConn.InstanceName);
				try
				{
					SessionConn.Close();
				}
				catch (Exception ex)
				{
					string x = null;
				}
				SessionConn = null;
				DbConn = null;

				if (DbConnectionsDict != null && DbConnectionsDict.ContainsKey(Id))
					DbConnectionsDict.Remove(Id); // remove from list of active connections
			}
		}

		/// <summary>
		/// Force close of all connections
		/// </summary>

		public static void ForceCloseAll()
		{
			if (Debug) DebugLog.Message("");
			if (DbConnectionsDict == null) return;

			lock (DbConnectionsDict)
			{
				foreach (int id in DbConnectionsDict.Keys) // can mod collection (no fix)?
				{
					DbConnectionMx conn = DbConnectionsDict[id];
					conn.ForceClose();
				}
			}
		}

		/// <summary>
		/// Force a connection to be released
		/// </summary>

		public void ForceClose()
		{
			if (SessionConn == null) return;
			if (Debug) DebugLog.Message("Force close connection " + SessionConn.InstanceName);
			SessionConn.ForceClose();
			SessionConn = null;
			DbConn = null;
			if (DbConnectionsDict != null)
				lock (DbConnectionsDict)
				{
					DbConnectionsDict.Remove(Id); // remove from list of active connections
				}
		}

		/// <summary>
		/// Replace any database passwords in a string with "xxxxxx"
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public static string RemoveDatabasePasswords(string s)
		{
			if (DataSourceMx.DataSources == null) return s;

			foreach (DataSourceMx source in DataSourceMx.DataSources.Values)
			{
				string pw = source.Password;
				if (Lex.IsDefined(pw) && Lex.Contains(s, pw))
					s = Lex.Replace(s, pw, "xxxxxx");
			}
			return s;
		}

		/// <summary>
		/// Test each connection & return text of results
		/// </summary>
		/// <returns></returns>

		public static string TestConnections()
		{
			SessionConnection.ForceCloseAll(); // force close of all connections

			string txt = "";
			foreach (DataSourceMx source in DataSourceMx.DataSources.Values)
			{
				txt += source.DataSourceName + " ";
				DateTime dt;
				try { dt = TestConnection(source.DataSourceName); }
				catch (Exception ex)
				{
					txt += ex.Message + "\n";
					continue;
				}

				txt += "OK";

				txt += ", Expires " + dt.ToShortDateString() + "\n";
			}

			SessionConnection.ForceCloseAll();

			return txt;
		}

		/// <summary>
		/// Test key connections & return message
		/// </summary>
		/// <returns></returns>

		public static string TestKeyConnections()
		{
			DateTime dt;
			SessionConnection.ForceCloseAll(); // force close of all connections

			string txt = "";
			int errorCount = 0;
			foreach (DataSourceMx source in DataSourceMx.DataSources.Values)
			{
				if (!source.IsKeyDataSource) continue;

				try { dt = TestConnection(source.DataSourceName); }
				catch (Exception ex)
				{
					errorCount++;
					txt += errorCount.ToString() + ". " + source.DataSourceName + " - " + ex.Message + "\n";
					foreach (DbSchemaMx schema in DataSourceMx.Schemas.Values)
					{
						if (Lex.Eq(schema.DataSourceName, source.DataSourceName))
							txt += "    - " + schema.Label + " (" + schema.Name + ")\n";
					}
				}
			}

			if (txt != "") txt =
				"The following Mobius databases are currently unavailable and queries that access them will not execute properly.\n" +
				"Normally these issues are corrected reasonably quickly; however, if they persist you can contact your local IT Service Desk\n" +
				"to verify that the problem has been reported and for a status update.\n\n" +
				txt;

			SessionConnection.ForceCloseAll();

			return txt;
		}

		/// <summary>
		/// Test a single connection & return expiration date of account
		/// </summary>
		/// <param name="sourceName"></param>
		/// <returns></returns>

		public static DateTime TestConnection(
			string sourceName)
		{
			DbConnectionMx c = GetConnection(sourceName);
			string sql = "select expiry_date from user_users";
			DateTime dt = SelectSingleValueDao.SelectDateTime(c, sql);
			c.Close();
			return dt;
		}

		/// <summary>
		/// Update database links from each instance to every other instance
		/// </summary>
		/// <returns></returns>

		public static string UpdateDatabaseLinks(
			string singleInstance)
		{
			DbConnectionMx c = null;
			string sql, txt, msg;
			int linkCount = 0;

			msg = "database links created from the following sources:\n\n";

			foreach (DataSourceMx source in DataSourceMx.DataSources.Values)
			{
				if (source.DbType != DatabaseType.Oracle) continue; // oracle sources only

				msg += source.DataSourceName + "\n";
				try // create new link
				{ c = GetConnection(source.DataSourceName); } // get connection to database
				catch (Exception ex)
				{
					msg += "Failed to get connection or create any links for " + source.DataSourceName + ": " + ex.Message + "\n";
					continue;
				}

				DbCommandMx drd = new DbCommandMx();
				drd.MxConn = c;

				foreach (DataSourceMx source2 in DataSourceMx.DataSources.Values)
				{
					if (source2 == source) continue; // no link to self
					if (!IsOracleDataSource(source2)) continue; // oracle sources only

					if (Lex.IsDefined(singleInstance) &&
						Lex.Ne(singleInstance, source.DataSourceName) && Lex.Ne(singleInstance, source2.DataSourceName))
						continue; // if single instance but link does not involve the as link source or dest instance then skip

					string linkName = source2.DataSourceName + "_LINK";
					txt = "Creating link " + source.DataSourceName + " to " + source2.DataSourceName;
					Progress.Show(txt);
					System.Windows.Forms.Application.DoEvents();

					try // drop any existing link
					{
						sql = "drop database link " + linkName;
						drd.Prepare(sql);
						drd.ExecuteNonReader(null, false);
					}
					catch (Exception ex) { } // ok if doesn't exist

					try // create new link
					{
						sql = "create database link " + linkName +
							" connect to " + source2.UserName +
							" identified by " + source2.Password +
							" using '" + source2.DatabaseLocator + "'";

						drd.Prepare(sql);
						drd.ExecuteNonReader();
						linkCount++;
					}
					catch (Exception ex)
					{
						msg += source.DataSourceName + " link " + linkName + " create failed: " + ex.Message + "\n";
					}
				}

				drd.Dispose();
			}

			Progress.Hide();
			msg = linkCount.ToString() + " " + msg;
			return msg;
		}

		/// <summary>
		/// Return true if the supplied sql maps to an Oracle source 
		/// </summary>

		public static bool IsSqlFromOracleSource(string sql)
		{
			DataSourceMx rootSource = GetRootDataSource(sql);
			if (rootSource == null) return false;
			return (rootSource.DbType == DatabaseType.Oracle);
		}

		/// <summary>
		/// Return true if Oracle data source
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>

		public static bool IsOracleDataSource(DataSourceMx source)
		{
			return (source.DbType == DatabaseType.Oracle);
		}

		/// <summary>
		/// Return true if the supplied SQL maps to an MySql source
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsSqlFromMySqlSource(string sql)
		{
			DataSourceMx rootSource = GetRootDataSource(sql);
			if (rootSource == null) return false;
			return (rootSource.DbType == DatabaseType.MySql);
		}

		/// <summary>
		/// Return true if Oracle data source
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>

		public static bool IsMySqlDataSource(DataSourceMx source)
		{
			return (source.DbType == DatabaseType.MySql);
		}

		/// <summary>
		/// Return true if the supplied SQL maps to an ODBC source
		/// </summary>
		/// <param name="mt"></param>
		/// <returns></returns>

		public static bool IsSqlFromOdbcSource(string sql)
		{
			DataSourceMx rootSource = GetRootDataSource(sql);
			if (rootSource == null) return false;
			return (rootSource.DbType == DatabaseType.ODBC);
		}

		/// <summary>
		/// Return true if ODBC data source
		/// </summary>
		/// <param name="dbName"></param>
		/// <returns></returns>

		public static bool IsOdbcDataSource(DataSourceMx source)
		{
			return (source.DbType == DatabaseType.Oracle);
		}

		/// <summary>
		/// Scan sql & get connection based on schema name(s) in the SQL.
		/// </summary>
		/// <param name="sql">Initial sql which may be modified by this method to include link references</param>
		/// <returns>Mobius connection to execute sql on</returns>

		public static DbConnectionMx MapSqlToConnection(
			ref string sql)
		{
			DbConnectionMx conn = MapSqlToConnection(ref sql, null, null);
			return conn;
		}

		/// <summary>
		/// Scan sql & get connection based on schema name(s) in the SQL.
		/// If multiple connections are needed then modify sql to use db_links
		/// and return the connection that has access to the first table that
		/// appears in the sql. Note that structure searches/retrieval must be 
		/// directly connected since they don't work throught links. This currently
		/// works because any structure-containing table will be positioned first. 
		/// This may have to be enhanced later when cases are supported that include
		/// structure access from other table positions.
		/// </summary>
		/// <param name="sql">Initial sql which may be modified by this method to include link references</param>
		/// <param name="forcedSourceName"></param>
		/// <param name="forcedSchemaName"></param>
		/// <returns></returns>

		public static DbConnectionMx MapSqlToConnection(
			ref string sql,
			string forcedSourceName,
			string forcedSchemaName)
		{
			DataSourceMx rootSource;
			DbConnectionMx mxConn;
			DbSchemaMx rootSchema;
			string rootSourceName = "", sourceName = "", placeHolder;

			Dictionary<string, Dictionary<string, DbSchemaMx>> connDict = GetDataSources(sql, out rootSource, out rootSchema);
			if (rootSource != null) rootSourceName = rootSource.DataSourceName;

			if (NoDatabaseAccessIsAvailable)
			{
				mxConn = new DbConnectionMx();
				//mxConn = DbConnectionMx.Get("NullDb");
				mxConn.LastRootSchema = rootSchema;
				return mxConn;
			}

			if (Lex.IsDefined(forcedSourceName) && Lex.Ne(rootSourceName, forcedSourceName) && Lex.IsDefined(forcedSchemaName)) // forcing a particular source & schema?
			{
				forcedSourceName = forcedSourceName.ToUpper();
				if (!DataSourceMx.DataSources.ContainsKey(forcedSourceName)) throw new Exception("Can't find data source: " + forcedSourceName);
				rootSource = DataSourceMx.DataSources[forcedSourceName];
				rootSourceName = rootSource.DataSourceName;

				forcedSchemaName = forcedSchemaName.ToUpper();
				if (!DataSourceMx.Schemas.ContainsKey(forcedSchemaName)) throw new Exception("Can't find data schema: " + forcedSchemaName);
				rootSchema = DataSourceMx.Schemas[forcedSchemaName];
			}

			if (connDict == null || connDict.Count == 0) return null; // throw new Exception("DbConnection.GetFromSql found no connection");

			//else if (connDict.Count == 1) // single direct connection (don't use, may need to handle alias)
			//	connName = rootConnection;

			// Tables from multiple connections referenced or forced DbLink
			// Modify sql to access through dblinks
			// Map schema.tablename to schema.tablename@connName_link

			string uniqueString = TimeOfDay.Milliseconds().ToString();
			while (sql.Contains(uniqueString))
				uniqueString = TimeOfDay.Milliseconds().ToString();

			List<string> snobl = GetSchemaNamesOrderedByLength(connDict);
			foreach (string schemaName in snobl)
			{
				//if (schemaName == "MBS_OWNER") snobl=snobl; // debug

				string target = schemaName + ".";
				if (!Lex.Contains(sql, target)) continue;

				DbSchemaMx schema = DataSourceMx.Schemas[schemaName];
				sourceName = schema.DataSourceName;
				if (sourceName != rootSourceName) // remap if not root source
				{
					string pattern = schemaName + @"\.[a-zA-Z0-9_]+"; // capture table name
					string replaceText = "$&" + "@" + sourceName + "_link";
					sql = Regex.Replace(sql, pattern, replaceText, RegexOptions.IgnoreCase);
				}

				if (!String.IsNullOrEmpty(schema.AliasFor)) // if alias then plug in real table owner
					placeHolder = schema.AliasFor + uniqueString;

				else // replace dot with unique string so not picked up again
					placeHolder = schema.Name + uniqueString;

				sql = Lex.Replace(sql, target, placeHolder);

				sql = Lex.Replace(sql, uniqueString, "."); // restore proper sql

				sourceName = rootSourceName; // use this source
			}

			mxConn = DbConnectionMx.GetConnection(sourceName);
			mxConn.LastRootSchema = rootSchema;

			if (Debug) DebugLog.Message("Connection " + mxConn.SessionConn.InstanceName);

			return mxConn;
		}

		/// <summary>
		/// Get names of schemas ordered by length to avoid false matches on shorter names
		/// </summary>
		/// <param name="connDict"></param>
		/// <returns></returns>

		static List<string> GetSchemaNamesOrderedByLength(
			Dictionary<string, Dictionary<string, DbSchemaMx>> connDict)
		{
			HashSet<string> sHash = new HashSet<string>();
			foreach (Dictionary<string, DbSchemaMx> sd in connDict.Values)
			{
				foreach (DbSchemaMx s in sd.Values)
				{
					if (!sHash.Contains(s.Name)) sHash.Add(s.Name);
				}
			}

			List<string> snobl = new List<string>(sHash);
			int i1, i2;
			for (i1 = 1; i1 < snobl.Count; i1++)
			{
				string s2 = snobl[i1];
				for (i2 = i1 - 1; i2 >= 0; i2--)
				{
					if (snobl[i2].Length > s2.Length) break;
					snobl[i2 + 1] = snobl[i2];
				}

				snobl[i2 + 1] = s2;
			}

			return snobl;
		}

		/// <summary>
		/// Get the root data source for a sql statement
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static DataSourceMx GetRootDataSource(
			string sql)
		{
			DataSourceMx rootSource;
			DbSchemaMx rootSchema;

			Dictionary<string, Dictionary<string, DbSchemaMx>> connDict = GetDataSources(sql, out rootSource, out rootSchema);
			return rootSource;
		}

		/// <summary>
		/// Get list of connections referenced in Sql.
		/// The data source metadata is assumed to have been loaded.
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>Dictionary indexed by source name with list of schemas associated with each source</returns>

		public static Dictionary<string, Dictionary<string, DbSchemaMx>> GetDataSources(
			string sql,
			out DataSourceMx rootSource,
			out DbSchemaMx rootSchema)
		{
			//if (Lex.Contains(sql, "_moltable")) sql = sql; // debug

			Dictionary<string, Dictionary<string, DbSchemaMx>> sourceDict =
				new Dictionary<string, Dictionary<string, DbSchemaMx>>(StringComparer.OrdinalIgnoreCase);

			rootSource = null;
			rootSchema = null;
			int rootSourcePos = 1000000;
			int moltablePos = 1000000;

			Lex lex = new Lex();

			lex = new Lex();
			lex.SetDelimiters("/* ' \" */ @ . , ; ( ) < = > <= >= <> != !> !< + - * /");
			lex.OpenString(sql);

			while (true) // parse the SQL looking for schema names
			{
				PositionedToken ptok = lex.GetPositionedToken();
				if (ptok == null) break;
				string tok = ptok.Text.ToUpper();
				//DebugLog.Message("Tok: " + tok); // debug
				if (!DataSourceMx.Schemas.ContainsKey(tok)) continue; // schema name?

				string schemaName = tok;
				DbSchemaMx schema = DataSourceMx.Schemas[schemaName];
				int schemaNamePos = ptok.Position;

				ptok = lex.GetPositionedToken();
				if (ptok == null || ptok.Text != ".") continue; // if not followed by period then can't be schema ref

				string sourceName = DataSourceMx.Schemas[schemaName].DataSourceName;
				DataSourceMx source = DataSourceMx.DataSources[sourceName];

				if (!sourceDict.ContainsKey(sourceName)) // first entry for source?
					sourceDict[sourceName] = new Dictionary<string, DbSchemaMx>(StringComparer.OrdinalIgnoreCase); // create schema dict

				sourceDict[sourceName][schemaName] = schema; // include the schema

				// See if there is any reference to a table named xxx_moltable and make it the root source 
				// to avoid the LOB locators from remote table problem.

				int tnp = schemaNamePos + schemaName.Length + 1; // position for table name
				int tni;
				for (tni = tnp; tni < sql.Length; tni++) // find end of name
				{
					char c = sql[tni];
					if (!Char.IsLetterOrDigit(c) && c != '_') break;
				}
				string tableName = sql.Substring(tnp, tni - tnp).ToLower();
				if (Lex.EndsWith(tableName, "_moltable"))
				{ // if reference to a _moltable exists then use the first of these
					if (schemaNamePos < moltablePos)
					{
						moltablePos = schemaNamePos;
						schemaNamePos = -1;
					}
				}

				// Make the moltable source or the first source the root connection

				if (schemaNamePos < rootSourcePos)
				{ // save first connection positionally
					rootSourcePos = schemaNamePos;
					rootSource = source;
					rootSchema = schema;
				}
			}

			return sourceDict;
		}

		/// <summary>
		/// Scan sql & get name of connection (single) based on schema name
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>

		public static string GetNameFromSql(
			string sql)
		{
			foreach (string schemaName in DataSourceMx.Schemas.Keys)
			{
				if (sql.ToLower().IndexOf(schemaName.ToLower() + ".") >= 0)
				{
					string connName = DataSourceMx.Schemas[schemaName].DataSourceName;
					return connName;
				}
			}

			return null;
		}

		/// <summary>
		/// Begin transaction
		/// </summary>

		public void BeginTransaction()
		{
			if (Debug) DebugLog.Message("Connection " + SessionConn.InstanceName);
			try
			{
				if (OracleConn != null)
					SessionConn.Txn = OracleConn.BeginTransaction();

				else if (OdbcDbConn != null) ; // todo

				else if (OleDbConn != null) ; // todo

				else { }
			}
			catch (Exception ex) { return; }
		}

		/// <summary>
		/// Commit transaction
		/// </summary>

		public void Commit()
		{
			if (Debug) DebugLog.Message("Connection " + SessionConn.InstanceName);
			try
			{
				if (SessionConn.Txn != null) SessionConn.Txn.Commit();
				SessionConn.Txn = null;
			}
			catch (Exception ex) { return; }
		}

		/// <summary>
		/// Rollback transaction
		/// </summary>

		public void Rollback()
		{
			if (Debug) DebugLog.Message("Connection " + SessionConn.InstanceName);
			try
			{
				if (SessionConn.Txn != null) SessionConn.Txn.Rollback();
				SessionConn.Txn = null;
			}
			catch (Exception ex) { return; }
		}

		/// <summary>
		/// Customize a MobiusConnectionOpenException to include the schema label
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="schemaName"></param>

		public static void ThrowSpecificConnectionOpenException(
				MobiusConnectionOpenException ex,
				string schemaName)
		{
			DbSchemaMx schema = DataSourceMx.Schemas[schemaName.ToUpper()];
			string sourceLabel = schema.Label;
			if (sourceLabel == null || sourceLabel == "") sourceLabel = schema.DataSourceName;
			else sourceLabel += " (" + schema.DataSourceName + ")";

			string message = GetSpecificConnectionOpenMessage(schemaName) + "\n" +
				ex.Message;
			throw new MobiusConnectionOpenException(message);
		}

		/// <summary>
		/// Create message that identifies data source for connection error
		/// </summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>

		public static string GetSpecificConnectionOpenMessage(
			string schemaName)
		{
			DbSchemaMx schema = DataSourceMx.Schemas[schemaName.ToUpper()];
			string sourceLabel = schema.Label;
			if (sourceLabel == null || sourceLabel == "") sourceLabel = schema.DataSourceName;
			else sourceLabel += " (" + schema.DataSourceName + ")";

			string message = "The \"" + sourceLabel + "\" data source is currently unavailable.";
			return message;
		}

		/// <summary>
		/// Start monitor to check for connection leaks
		/// </summary>

		public static void StartConnectionLeakMonitor(
			int interval)
		{
			ConnectionLeakDetectionInterval = interval;

			if (ConnectionLeakMonitorThread != null) return;

			ThreadStart threadStart = new ThreadStart(ConnectionLeakMonitor);
			ConnectionLeakMonitorThread = new Thread(threadStart);
			Thread t = ConnectionLeakMonitorThread;
			t.IsBackground = true;
			t.SetApartmentState(ApartmentState.STA);
			t.Name = "ConnectionLeakMonitor";
			t.Start();
		}

		/// <summary>
		/// ConnectionLeakMonitor - Thread method to check periodically for connection leaks
		/// </summary>

		private static void ConnectionLeakMonitor()
		{
			int checkIntervalSecs = 60;

			while (true)
			{
				Thread.Sleep(checkIntervalSecs * 1000);
				string result = CheckForConnectionLeaks(ConnectionLeakDetectionInterval, true);
				if (Lex.IsDefined(result))
					DebugLog.Message(result);
			}
		}

		/// <summary>
		/// Check for connection leaks
		/// </summary>
		/// <returns></returns>

		public static string CheckForConnectionLeaks()
		{
			return CheckForConnectionLeaks(0, false);
		}

		/// <summary>
		/// Check for connection leaks
		/// 
		/// </summary>
		/// <param name="minSeconds">Minimum secs held to be considered a leak</param>
		/// <param name="reportOnceOnly">If true only report a single time</param>
		/// <returns></returns>

		public static string CheckForConnectionLeaks(
			int minSeconds,
			bool reportOnceOnly)
		{
			if (!SessionConnection.Pooling || DbConnectionsDict == null || LoggedConnectionLeak) return null;

			foreach (DbConnectionMx c in DbConnectionsDict.Values)
			{
				if (c.SessionConn == null || c.SessionConn.DbConn == null) continue;
				if (minSeconds > 0 && DateTime.Now.Subtract(c.CreationTime).TotalSeconds < minSeconds)
					continue; // continue if not long enough

				if (c.ReportedAsLeak && reportOnceOnly) continue; // don't report again

				c.ReportedAsLeak = true;
				LoggedConnectionLeak = true; // have a leak

				string msg = "Connection leak on " + c.SessionConn.InstanceName + "\r\n" + c.StackTrace;
				if (!String.IsNullOrEmpty(c.LastSql)) msg += "\r\n" +
					"LastSql = " + c.LastSql;
				return msg;
			}

			return null;
		}

		/// <summary>
		/// Debug test of oracle connection
		/// </summary>

		public static void TestOracle()
		{
			try
			{
				OracleConnection conn = new OracleConnection();
				conn.ConnectionString =
					"Data Source=prd123" +
					";User ID=<userId>" +
					";Password=<password>";
				conn.Open();

				string sql = "select username from user_users where username is not null";
				OracleCommand cmd = new OracleCommand(sql, conn);
				OracleDataReader rdr = cmd.ExecuteReader();
				bool b = rdr.Read();
				string userName = rdr.GetString(0);
				rdr.Close();
				conn.Close();
				conn.Dispose();
			}
			catch (Exception ex)
			{
				return;
			}
		}

		/// <summary>
		/// Get list of existing database connections
		/// </summary>
		/// <returns></returns>

		public static string GetActiveDatabaseConnections()
		{
			Dictionary<int, DbConnectionMx> conns = DbConnectionsDict;

			string s = "";
			foreach (DbConnectionMx c in conns.Values)
			{
				if (s.Contains(c.StackTrace.ToString())) continue;
				if (s.Length > 0) s += "\r\n";
				if (c.SessionConn == null || c.SessionConn.DbConn == null) continue;
				s += c.SessionConn.InstanceName + "\r\n" + c.StackTrace;
			}

			return s;
		}

		/// <summary>
		/// Add the definition of a new data source
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string DefineDataSource(string args)
		{
			Lex lex = new Lex();
			lex.OpenString(args);

			DataSourceMx ds = new DataSourceMx();


			string dbType = lex.Get();
			if (!Enum.TryParse<DatabaseType>(dbType, true, out ds.DbType))
				throw new Exception("Invalid Database type: " + dbType);

			ds.DataSourceName = lex.GetUpper();
			string asTok = lex.Get();
			ds.DatabaseLocator = lex.GetUpper();
			ds.UserName = lex.GetUpper();
			ds.Password = lex.Get();
			ds.InitCommand = lex.Get();

			if (Lex.Ne(asTok, "As") ||
					ds.DatabaseLocator == "" ||
					ds.UserName == "" ||
					ds.Password == "")
				return "Syntax: Define Data Source data-source-name As oracle-instance-id userId password";

			DataSourceMx oldDs = null;
			if (DataSourceMx.DataSources.ContainsKey(ds.DataSourceName))
				oldDs = DataSourceMx.DataSources[ds.DataSourceName];

			DataSourceMx.DataSources[ds.DataSourceName] = ds;
			try // make sure we can connect to it
			{
				SessionConnection mxConn = SessionConnection.Get(ds.DataSourceName);
				mxConn.Close();
			}
			catch (Exception ex)
			{
				if (oldDs != null) DataSourceMx.DataSources[oldDs.DataSourceName] = oldDs;
				DataSourceMx.DataSources.Remove(ds.DataSourceName);
				return "Error connecting to new data source: " + ex.Message;
			}

			return "Data source definition complete";
		}

		/// <summary>
		/// Associate a schema name with a data source
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>

		public static string AssociateSchema(string args)
		{
			string dsName;
			Lex lex = new Lex();
			lex.OpenString(args);

			string schemaName = lex.GetUpper();
			while (true)
			{
				dsName = lex.GetUpper();
				if (Lex.Eq(dsName, "With") ||
				Lex.Eq(dsName, "Data") ||
				Lex.Eq(dsName, "Source"))
					continue;
				else break;
			}

			if (dsName == "")
				return "Syntax: Associate Schema schema-name With Data Source data-source-name";

			if (!DataSourceMx.DataSources.ContainsKey(dsName))
				return "Data source " + dsName + " is not defined";

			DbSchemaMx s = new DbSchemaMx();
			s.Name = schemaName;
			s.DataSourceName = dsName;
			DataSourceMx.Schemas[schemaName] = s;

			return "Association complete";
		}

	}

	/// <summary>
	/// Connection associated with a session. 
	/// These connections are held until the count goes to zero or
	/// they are explicitly closed.
	/// </summary>

	public class SessionConnection
	{
		public int Id = InstanceCount++; // id of this instance
		internal static int InstanceCount = 0; // instance count

		public DataSourceMx DataSource; // associated datasource
		public string InstanceName = ""; // connection instance name, usually matches datasource namexxx

		public DbConnection DbConn; // associated database connection 
		public OracleConnection OracleConn { get { return DbConn as OracleConnection; } }
		public OleDbConnection OleDbConn { get { return DbConn as OleDbConnection; } }

		public int ActiveCount = 0; // count of active open references for this connection
		public OracleTransaction Txn; // transaction for connection
		public Exception ConnectionException; // any exception from previous failed connection attempt
		public DateTime LastConnectionExceptionTime; // time of last connection exception

		// Dictionary of all connections for session

		public static Dictionary<string, SessionConnection> SessionConnections =
			new Dictionary<string, SessionConnection>(StringComparer.OrdinalIgnoreCase);

		public static bool Pooling = true; // process wide pooling information used when connecting 
		public static int MinPoolSize = 1; // set to zero to release all connections for process
		public static int IncrPoolSize = 5;
		public static int MaxPoolSize = 100;
		public static int DecrPoolSize = 1; // max number of idle connections per pool to release every 3 minutes (until MinPoolSize reached)
		public static int ConnectionLifetime = 0; // Connection lifetime in seconds (only checked when going back to pool)

		public static bool Debug => DbConnectionMx.LogDbConnectionDetail;

		/// <summary>
		/// Get a new connection or increment the count if connection already exists
		/// </summary>
		/// <param name="dsName">Data source name</param>
		/// <returns></returns>

		public static SessionConnection Get(
				string dsName)
		{
			SessionConnection conn;
			int connectTime;

			int t0 = TimeOfDay.Milliseconds();

			try
			{
				lock (SessionConnections)
				{

					if (dsName == null || dsName == "")
						throw new Exception("Missing connection name");
					dsName = dsName.ToUpper();

					if (DataSourceMx.DataSources == null ||
						!DataSourceMx.DataSources.ContainsKey(dsName))
						throw new Exception("Connection not found " + dsName);

					DataSourceMx dataSource = DataSourceMx.DataSources[dsName];
					string connName = dsName; // connection name is same as datasource name by default

					if (dataSource.DbType == DatabaseType.MySql)
						connName += "." + (InstanceCount + 1); // force alloc of new connection for each statement

					//if (DbConnectionMx.IsOdbcDatabase(dataSource.DatabaseName) && // reuse existing Odbc connection?
					//  SessionConnections.ContainsKey(connName) && SessionConnections[connName].DbConn != null) 
					//{
					//  OdbcConnection dbc = SessionConnections[connName].DbConn as OdbcConnection;

						//  try dbc.CreateCommand
						//  if (dbc.State == 

						//  connName += "_" + SessionConnections.Count + 1;
						//}

					if (!SessionConnections.ContainsKey(connName)) // create entry if needed
					{
						conn = new SessionConnection();
						SessionConnections[connName] = conn;
						conn.DataSource = dataSource;
						conn.InstanceName = connName;
					}

					else conn = SessionConnections[connName];

					if (conn.DbConn != null) // already have database connection?
					{
						conn.ActiveCount++;
						if (Debug)
							DebugLog.Message("Get connection " + connName + ", count = " + conn.ActiveCount);
						return conn;
					}

					else if (conn.ConnectionException != null &&
						((TimeSpan)DateTime.Now.Subtract(conn.LastConnectionExceptionTime)).TotalSeconds < 5)
					{ // don't retry if a short time since last failure
						throw conn.ConnectionException;
					}

					else // get database connection (from local pool or server)
					{
						string dbName = dataSource.DatabaseLocator;

						try
						{
							if (dataSource.DbType == DatabaseType.Oracle)
								conn.DbConn = GetOracleConnection(dataSource);

							if (dataSource.DbType == DatabaseType.MySql)
								conn.DbConn = GetMySqlConnection(dataSource);

							else if (dataSource.DbType == DatabaseType.ODBC)
								conn.DbConn = GetOdbcConnection(dataSource); // connect with ODBC

							else throw new ArgumentException("Unrecognized database type: " + dbName);
						}

						catch (Exception ex)
						{
							connectTime = TimeOfDay.Milliseconds() - t0;
							conn.ConnectionException = ex; // save exception
							conn.LastConnectionExceptionTime = DateTime.Now;
							DebugLog.Message("Exception connecting to: " + connName + ", " + ex.Message); // debug
							throw new MobiusConnectionOpenException(ex.Message);
						}

						connectTime = TimeOfDay.Milliseconds() - t0;
						if (Debug)
							DebugLog.Message("Acquired connection for: " + connName + ", time = " + connectTime.ToString() + " ms");

						conn.Txn = null;
						conn.ActiveCount = 1;

						return conn;
					}

				} // end of lock
			} // end of try
			catch (Exception ex)
			{
				string msg = "Failed to get connection: " + dsName + ", " + ex.Message;
				DebugLog.Message(msg);
				throw new Exception(msg, ex);
			}
		}

		/// <summary>
		/// Open an Oracle database connection
		/// </summary>
		/// <param name="connInf"></param>
		/// <returns></returns>

		static DbConnection GetOracleConnection(DataSourceMx connInf)
		{
			OracleCommand oraCmd;

			string prefix = "Oracle:";
			string dbName = connInf.DatabaseLocator;
			if (Lex.StartsWith(dbName, prefix)) dbName = dbName.Substring(prefix.Length);

			//if (Lex.Ne(dbName, "DEV857")) dbName = dbName; // debug

			OracleConnection conn = new OracleConnection();
			conn.ConnectionString =
				"Data Source=" + dbName +
				";User ID=" + connInf.UserName +
				";Password=" + connInf.Password +
				";Pooling = " + Pooling.ToString() +
				";Min Pool Size = " + MinPoolSize.ToString() +
				";Incr Pool Size = " + IncrPoolSize.ToString() +
				";Max Pool Size = " + MaxPoolSize.ToString() +
				";Decr Pool Size = " + DecrPoolSize.ToString() +
				";Connection Lifetime = " + ConnectionLifetime.ToString();

			if (Debug)
			{
				string cs2 = Lex.Replace(conn.ConnectionString, connInf.Password, "xxxxxxxx"); // don't log the password
				DebugLog.Message("Opening Oracle connection with ConnectionString: " + cs2);
			}

			int t0 = TimeOfDay.Milliseconds();
			conn.Open();

			// If this is a new connection (i.e. connectTime > 0) then do connection initialization

			int connectTime = TimeOfDay.Milliseconds() - t0;
			if (connectTime > 0)
			{
				if (conn != null)
				{ // For Oracle turn off global_names so dblink name doesn't have to match database name
					string sql = "alter session set global_names = false";
					oraCmd = new OracleCommand(sql, conn);
					int result = oraCmd.ExecuteNonQuery();

					sql = "alter session set nls_date_format = 'dd-Mon-rrrr'"; // debug
					oraCmd = new OracleCommand(sql, conn);
					result = oraCmd.ExecuteNonQuery();

					oraCmd.Dispose();
				}


				// Initialization command

				if (connInf.InitCommand != "")
				{
					string[] cmds = connInf.InitCommand.Split(';');
					for (int ci = 0; ci < cmds.Length; ci++)
					{
						string cmd = cmds[ci];
						if (cmd.Trim() == "") continue;

						while (true) // replace any :UserName parameter
						{
							string p = ":UserName";
							int i1 = Lex.IndexOf(cmd, p);
							if (i1 < 0) break;
							if (UAL.Security.UserInfo == null ||
									String.IsNullOrEmpty(UAL.Security.UserInfo.UserName)) break;

							cmd = cmd.Replace(cmd.Substring(i1, p.Length), UAL.Security.UserInfo.UserName.ToUpper());
						}

						try
						{
							t0 = TimeOfDay.Milliseconds();
							if (Lex.StartsWith(cmd, "select "))
							{
								oraCmd = new OracleCommand(cmd, conn);
								OracleDataReader rdr = oraCmd.ExecuteReader();
								bool readResult = rdr.Read();
								rdr.Dispose();
								oraCmd.Dispose();
							}

							else if (Lex.StartsWith(cmd, "exec ")) // stored procedure call
							{
								Lex lex = new Lex();
								lex.SetDelimiters(", ( ) =");
								lex.OpenString(cmd);
								string tok = lex.Get(); // get exec keyword
								string procName = lex.Get(); // stored procedure name
								oraCmd = new OracleCommand(procName, conn);
								oraCmd.CommandType = CommandType.StoredProcedure;
								while (true) // get any parameters
								{
									tok = lex.Get();
									if (tok == "") break;
									if (tok == "(" || tok == "," || tok == ")") continue;
									string parmName = tok;
									tok = lex.GetExpected("=");
									string parmValue = lex.Get();
									parmValue = Lex.RemoveAllQuotes(parmValue);
									oraCmd.Parameters.Add(parmName, parmValue);

									//oraCmd.Parameters.Add("pin_deptno", OracleType.Number).Value = 20;
									//oraCmd.Parameters.Add("pout_count", OracleType.Number).Direction = ParameterDirection.Output;
								}

								int iResult = oraCmd.ExecuteNonQuery();
								oraCmd.Dispose();

								// Command to check manually from sql plus
								//string sql = "select count(*) from stp_owner.stp_target"; // see how many targets, more for users with access to restricted data
								//oraCmd = new OracleCommand(sql, conn);
								//OracleDataReader rdr = oraCmd.ExecuteReader();
								//bool readResult = rdr.Read();
								//object o = rdr.GetValue(0); // target count
								//rdr.Dispose();
								//oraCmd.Dispose();
							}

							else // assume other non-query statement
							{
								oraCmd = new OracleCommand(cmd, conn);
								int iResult = oraCmd.ExecuteNonQuery();
								oraCmd.Dispose();
							}
						}

						catch (Exception ex)
						{
							DebugLog.Message("Error executing Connection InitCommand: " + cmd + "\n\r" + ex.Message);
						}
					}
				}
			}

			// Get client version

			if (DbConnectionMx.OracleClientVersion == null) try
				{
					DbConnectionMx.OracleClientVersion = typeof(OracleConnection).Assembly.GetName().Version;
				}
				catch (Exception ex) { string msg = ex.Message; }

			return conn;
		}

		static DbConnection GetMySqlConnection(DataSourceMx connInf)
		{
			MySqlCommand oraCmd;

			string dbName = connInf.DatabaseLocator;

			MySqlConnection conn = new MySqlConnection();
			conn.ConnectionString = connInf.DatabaseLocator;

			if (Debug)
			{
				string cs2 = Lex.Replace(conn.ConnectionString, connInf.Password, "xxxxxxxx"); // don't log the password
				DebugLog.Message("Opening MySql connection with ConnectionString: " + cs2);
			}

			int t0 = TimeOfDay.Milliseconds();
			conn.Open();

			int connectTime = TimeOfDay.Milliseconds() - t0;
			if (connectTime > 0) // new connection (i.e. connectTime > 0)
			{
				// do any needed initialization here
			}

			// Get client version

			if (DbConnectionMx.MySqlClientVersion == null) try
				{
					DbConnectionMx.MySqlClientVersion = typeof(MySqlConnection).Assembly.GetName().Version;
				}
				catch (Exception ex) { string msg = ex.Message; }

			return conn;
		}

		/// <summary>
		/// Open an ODBC connection. Connection strings examples:
		///   Amazon Redshift: Driver={Amazon Redshift (x86)};servername=rsrch-infra.ckkzmyikpwvp.us-east-2.redshift.amazonaws.com;port=5439;database=rsinf;username=[userName];password=XXXXXX;
		///   Amazon Postgres: Driver={PostgreSQL Unicode};servername=datalake1.cjaz0pfst0cr.us-east-2.rds.amazonaws.com;port=5432;database=chembio;username=[userName];password=XXXXXX;sslmode=require;" />
		///   Netezza: Driver={NetezzaSQL};servername=[server];port=5480;database=LLYMMP_SND;username=[userName];password=XXXXXX;
		///   MySQL: Driver={MySQL ODBC 3.51 Driver};server=[server];port=3306;database=knimeivdr;user=[userName];password=XXXXXX;  (note: must be user=xxx, not username=xxx)
		/// </summary>
		/// <param name="connInf"></param>
		/// <returns></returns>

		static DbConnection GetOdbcConnection(DataSourceMx connInf)
		{
			string prefix = "ODBC:";
			string cs = connInf.DatabaseLocator; // get connection string prototype
			if (Lex.StartsWith(cs, prefix)) cs = cs.Substring(prefix.Length).Trim(); // remove ODBC: prefix

			if (!Lex.Contains(cs, "user=") && !Lex.Contains(cs, "username=") && !Lex.Contains(cs, "password="))
			{ // add user and password info if not included in connection string prototype
				string userName = connInf.UserName;
				string pw = connInf.Password;
				cs += "username=" + userName + ";password=" + pw + ";";
			}

			if (Debug)
			{
				string cs2 = Lex.Replace(cs, connInf.Password, "xxxxxxxx"); // don't log the password
				DebugLog.Message("Opening ODBC connection with ConnectionString: " + cs2);
			}

			try
			{
				OdbcConnection conn = new OdbcConnection(cs);
				conn.Open();
				return conn;
			}

			catch (Exception ex)
			{
				string msg = "Failed to get ODBC connection for connection string: " + cs + "\r\n" + DebugLog.FormatExceptionMessage(ex);
				DebugLog.Message(msg);
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Open a Netezza OLE-DB connection (not used)
		/// </summary>
		/// <param name="connInf"></param>
		/// <returns></returns>

		static DbConnection GetNetezzaOleDbConnection(DataSourceMx connInf)
		{
			string prefix = "Netezza:";
			string dbString = connInf.DatabaseLocator;
			if (Lex.StartsWith(dbString, prefix)) dbString = dbString.Substring(prefix.Length);

			string userName = connInf.UserName;
			string pw = connInf.Password;
			string port = "5480"; // default port

			string[] sa = dbString.Split('/');
			if (sa.Length != 2) throw new Exception("Invalid database name");

			string server = sa[0].Trim();
			if (server.Contains(":"))
			{
				string[] sa2 = server.Split(':');
				server = sa[0].Trim();
				port = sa[1].Trim();
			}

			string dbName = sa[1].Trim();

			string cString =
				"Provider=NZOLEDB;" +
				"Password=" + pw + ";User ID=" + userName + ";Persist Security Info=True;" +
				"Data Source=" + server + ";Port=" + port + ";Initial Catalog=" + dbName;
			// + ";" + "Logging Level=1;Log Path=" + LogDir;

			OleDbConnection conn = new OleDbConnection(cString);
			conn.Open();
			return conn;
		}

		/// <summary>
		/// If count is reduced to zero then return connection to pool
		/// otherwise just decrement the count.
		/// </summary>

		public void Close()
		{
			lock (SessionConnections)
			{
				if (Debug)
					DebugLog.Message("Removing session connection: " + InstanceName + " count = " + ActiveCount);

				if (DataSource != null && // if ODBC datasource then don't close underlying connection
				 DbConnectionMx.IsOdbcDataSource(DataSource))
				{
					return; // don't close underlying ODBC connections
				}

				ActiveCount--;
				if (ActiveCount > 0) return; // if positive count then keep it
																		 //if (!SessionConnection.Pooling) return;  // also keep if not pooling connections (no, allow pooling to be turned off to force reconnect)

				if (DbConn == null)
				{
					if (Debug) DebugLog.Message("Connection " + InstanceName + " no Oracle connection!");
					return;
				}

				DbConn.Close(); // release back to client pool or Oracle
				DbConn = null;
				ActiveCount = 0;
				if (SessionConnections != null && SessionConnections.ContainsKey(InstanceName))
					SessionConnections.Remove(InstanceName); // remove from list of session connections
			}
		}

		/// <summary>
		/// Force close of all session connections
		/// </summary>

		public static void ForceCloseAll()
		{
			if (SessionConnections == null) return;

			if (Debug) DebugLog.Message("");
			lock (SessionConnections)
			{

				string[] keys = new string[SessionConnections.Count];
				SessionConnections.Keys.CopyTo(keys, 0);
				foreach (string name in keys)
				{
					SessionConnection sessConn = SessionConnections[name];
					sessConn.ForceClose();
				}
			}
		}

		/// <summary>
		/// Force a connection closed
		/// </summary>

		public void ForceClose()
		{
			if (DbConn == null) return;

			if (Debug) DebugLog.Message("Force close of connection " + InstanceName);

			lock (SessionConnections)
			{
				DbConn.Close(); // release back to client pool or Oracle
				DbConn = null;
				ActiveCount = 0;
				SessionConnections.Remove(InstanceName); // remove from list of session connections
			}
		}
	}

	/// <summary>
	/// Exception thrown if connection to database fails
	/// </summary>

	public class MobiusConnectionOpenException : Exception
	{
		public MobiusConnectionOpenException() : base()
		{
		}

		public MobiusConnectionOpenException(string message) : base(message)
		{
		}

	}

	/// <summary>
	/// Account access
	/// </summary>

	public class AccountAccessMx
	{
		/// <summary>
		/// TryGetMobiusSystemWindowsAccount
		/// This is done in this file to keep all password-accessing code in a single file
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="pw"></param>
		/// <returns></returns>

		public static bool TryGetMobiusSystemWindowsAccount(
			out string userName,
			out string pw)
		{
			string MobiusSystemWindowsAccountName = "MobiusSystemWindowsAccount";

			userName = pw = null;

			if (!DataSourceMx.DataSources.ContainsKey(MobiusSystemWindowsAccountName)) return false;

			DataSourceMx ds = DataSourceMx.DataSources[MobiusSystemWindowsAccountName];

			userName = ds.UserName;
			pw = ds.Password;
			return true;
		}

	}

}
