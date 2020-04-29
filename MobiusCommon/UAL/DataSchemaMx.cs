using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.UAL
{
  /// <summary>
  /// Database Schema and associated data source
  /// </summary>

  public class DataSchemaMx
  {
    public string Name = ""; // name of database schema
    public string AliasFor = ""; // really an alias for this schema
    public string Label = ""; // label associated with schema
    public string DataSourceName = ""; // datasource name

    /// <summary>
    /// Get the DataSourceMx associated with a schema name
    /// </summary>
    /// <param name="schemaName"></param>
    /// <returns></returns>

    public static DataSourceMx GetDataSourceForSchemaName(string schemaName)
    {
      if (Lex.IsUndefined(schemaName)) return null; // schema not defined

      if (!DataSourceMx.Schemas.ContainsKey(schemaName)) return null; // unknown schema

      string dsName = DataSourceMx.Schemas[schemaName].DataSourceName;

      if (Lex.IsUndefined(dsName)) return null;

      if (!DataSourceMx.DataSources.ContainsKey(dsName)) return null; // unknown datasource

      DataSourceMx ds = DataSourceMx.DataSources[dsName];
      return ds;
    }
  }

}

