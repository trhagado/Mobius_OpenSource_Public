using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.UAL
{
	/// <summary>
	/// Datasource information
	/// </summary>

	public class DataSourceMx
	{
		public static Dictionary<string, DataSourceMx> DataSources = null; // data source information
		public static Dictionary<string, DataSchemaMx> Schemas = null; // map of schema names to their corresponding schema

		public DatabaseType DbType = DatabaseType.Undefined; // e.g. Oracle, MySQL...
		public string Name; // data source/connection name
		public string DatabaseName = ""; // Database connection name (e.g. Tnsnames.ora name of Oracle instance to connect to, ODBC connection string, etc)
		public string UserName = ""; // id used to connect to database
		public string Password = ""; // password to connect to database
		public string InitCommand = ""; // initialization command(s)
		public bool IsKeyDataSource = false; // true is this is a key datasource
		public bool CanCreateTempTables = false; // true if the associated account can create temp tables

		/// <summary>
		/// Load metadata describing connections and associated schemas
		/// </summary>
		/// <param name="dsFileName"></param>
		/// <returns></returns>

		public static int LoadMetadata()
		{
			XmlAttributeCollection atts;

			// Get some inifile parameters first

			if (ServicesIniFile.IniFile == null)
				throw new Exception("ServicesIniFile.IniFile is null");

			SessionConnection.Pooling = ServicesIniFile.ReadBool("Pooling", true);
			SessionConnection.MinPoolSize = ServicesIniFile.ReadInt("MinPoolSize", 1);
			SessionConnection.IncrPoolSize = ServicesIniFile.ReadInt("IncrPoolSize", 5);
			SessionConnection.MaxPoolSize = ServicesIniFile.ReadInt("MaxPoolSize", 100);
			SessionConnection.DecrPoolSize = ServicesIniFile.ReadInt("DecrPoolSize", 1);
			SessionConnection.ConnectionLifetime = ServicesIniFile.ReadInt("ConnectionLifetime", 0);

			string tok = ServicesIniFile.Read("KeyListPredType", "Parameterized");
			int enumInt = EnumUtil.Parse(typeof(KeyListPredTypeEnum), tok);
			if (enumInt >= 0) DbCommandMx.DefaultKeyListPredType = (KeyListPredTypeEnum)enumInt;

			DbCommandMx.DbFetchRowCount = ServicesIniFile.ReadInt("DbFetchRowCount", DbCommandMx.DbFetchRowCount);
			DbCommandMx.DbFetchSizeMax = ServicesIniFile.ReadInt("DbFetchSizeMax", DbCommandMx.DbFetchSizeMax);

			DataSources = new Dictionary<string, DataSourceMx>(StringComparer.OrdinalIgnoreCase);

			string dsFileName = ServicesDirs.MetaDataDir + @"\" + "DataSources.xml";

			StreamReader sr = new StreamReader(dsFileName);
			XmlDocument doc = new XmlDocument();
			doc.Load(sr);
			XmlNode node = doc.FirstChild;

			while (node != null)
			{
				if (node.NodeType == XmlNodeType.Element) break;
				node = node.NextSibling;
				if (node == null)
					throw new Exception("No initial element found");
			}

			if (!Lex.Eq(node.Name, "DataSources"))
				throw new Exception("Expected DataSources node: " + node.Name);

			atts = node.Attributes; // datasources attributes
			for (int i = 0; i < atts.Count; i++)
			{
				XmlNode att = atts.Item(i);

				if (Lex.Eq(att.Name, "DblinkConnName")) { } // obsolete key word

				else
					throw new Exception
						("Unexpected DataSources attribute: " + att.Name);
			}

			node = node.FirstChild;
			while (node != null)
			{
				if (node.NodeType != XmlNodeType.Element) ; // ignore non-elements

				else if (Lex.Eq(node.Name, "DataSource") || // connection element
				 Lex.Eq(node.Name, "Connection"))
				{
					DataSourceMx cd = new DataSourceMx();

					atts = node.Attributes;
					for (int i = 0; i < atts.Count; i++)
					{
						XmlNode att = atts.Item(i);

						if (Lex.Eq(att.Name, "DatabaseType") || Lex.Eq(att.Name, "DbType"))
						{
							if (!Enum.TryParse<DatabaseType>(att.Value, true, out cd.DbType))
								throw new Exception("Invalid Database type: " + att.Value);
						}

						else if (Lex.Eq(att.Name, "Name"))
							cd.Name = att.Value.ToUpper().Trim();

						else if (Lex.Eq(att.Name, "DatabaseName") || Lex.Eq(att.Name, "OracleName"))
							cd.DatabaseName = att.Value.ToUpper().Trim();

						else if (Lex.Eq(att.Name, "ConnectionString")) // keep case if connection string
							cd.DatabaseName = att.Value.Trim();

						else if (Lex.Eq(att.Name, "UserName"))
							cd.UserName = att.Value.Trim(); // keep case as is (Oracle 11g)

						else if (Lex.Eq(att.Name, "Password"))
							cd.Password = att.Value.Trim(); // keep case as is (Oracle 11g)

						else if (Lex.Eq(att.Name, "MdlInit")) // mdl initialization
							cd.InitCommand = "select cdcaux.ctenvinit('" + att.Value.Trim() + "') from dual";

						else if (Lex.Eq(att.Name, "InitCommand"))
							cd.InitCommand = att.Value.Trim();

						else if (Lex.Eq(att.Name, "KeyDataSource"))
							bool.TryParse(att.Value.Trim(), out cd.IsKeyDataSource);

						else if (Lex.Eq(att.Name, "CanCreateTempTables"))
							bool.TryParse(att.Value.Trim(), out cd.CanCreateTempTables);

						else if (Lex.Eq(att.Name, "AlwaysUseDbLink")) { } // obsolete

						else
							throw new Exception
								("Unexpected Connection attribute: " + att.Name);
					}

					if (cd.Name == "")
						DebugLog.Message("Connection is missing name: " + cd.DatabaseName);
					else if (DataSourceMx.DataSources.ContainsKey(cd.Name))
						DebugLog.Message("Data source defined twice: " + cd.Name);
					else DataSourceMx.DataSources.Add(cd.Name, cd);
				}

				else if (Lex.Eq(node.Name, "SchemaToDataSource") || // schema map element
				Lex.Eq(node.Name, "SchemaToConnection"))
				{

					DataSchemaMx schema = new DataSchemaMx();
					atts = node.Attributes;
					for (int i = 0; i < atts.Count; i++)
					{
						XmlNode att = atts.Item(i);

						if (Lex.Eq(att.Name, "Schema"))
							schema.Name = att.Value.ToUpper().Trim();

						else if (Lex.Eq(att.Name, "AliasFor"))
							schema.AliasFor = att.Value.ToUpper().Trim();

						else if (Lex.Eq(att.Name, "Connection"))
							schema.DataSourceName = att.Value.ToUpper().Trim();

						else if (Lex.Eq(att.Name, "Label") || Lex.Eq(att.Name, "L"))
							schema.Label = att.Value.Trim();

						else
							throw new Exception
								("Unexpected Connection attribute: " + att.Name);
					}

					if (schema.Name == "") throw new Exception("Missing schema name");
					if (schema.DataSourceName == "") throw new Exception("Missing data source/connection name");
					Schemas[schema.Name] = schema;
				}

				else throw new Exception("Expected Connection or SchemaToDataSource element but saw " +
							 node.Name);

				node = node.NextSibling;
			}
			sr.Close();

			return DataSources.Count;
		}

	} //	DataSourceMx

	/// <summary>
	/// Type of database server
	/// </summary>

	public enum DatabaseType
	{
		Undefined = 0,
		MySql = 1,
		Oracle = 2,
		ODBC = 3
	}


}
