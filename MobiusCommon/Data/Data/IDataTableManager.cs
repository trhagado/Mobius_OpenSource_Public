using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.Data
{
/// <summary>
/// Interface for manipulation of a MoleculeDataTable
/// </summary>

	public interface IDataTableManager
	{

		/// <summary>
		/// Serialize a DataTable to an XmlTextWriter
		/// </summary>
		/// <param name="tw"></param>
		/// <param name="dt"></param>

		void SerializeDataTable(
			XmlTextWriter tw,
			IDataTableMx dt);

		/// <summary>
		/// Serialize a DataTable to an XmlTextWriter
		/// </summary>
		/// <param name="tw"></param>
		/// <param name="qm"></param>

		void SerializeDataTable(
			XmlTextWriter tw,
			IQueryManager qm);

/// <summary>
/// Deserialize a DataTable
/// </summary>
/// <param name="tr"></param>
/// <returns></returns>

		IDataTableMx Deserialize(
			XmlTextReader tr);
	}
}
