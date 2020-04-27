using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// DataTableMsx
	/// </summary>

	public class DataTableMsx : NodeMsx
	{
		public string Name = "";
		public string Id = ""; // guid
		public List<DataColumnMsx> Columns = new List<DataColumnMsx>();

		public int RowCount = -1;

		/// <summary>
		/// Get a DataColumn by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public DataColumnMsx GetColumnByName(string name)
		{
			DataColumnMsx dc;

			if (TryGetColumnByName(name, out dc))
				return dc;

			else throw new Exception("Can't find name: " + name);
		}

		/// <summary>
		/// Try to get a DataColumn by name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="dc"></param>
		/// <returns></returns>

		public bool TryGetColumnByName(string name, out DataColumnMsx dc)
		{
			foreach (DataColumnMsx dc0 in Columns)
			{
				if (String.Equals(name, dc0.Name, StringComparison.OrdinalIgnoreCase))
				{
					dc = dc0;
					return true;
				}
			}

			dc = null;
			return false;
		}

		/// <summary>
		/// Get serializable reference name from instance
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>

		public static string GetReferenceId(DataTableMsx dc)
		{
			if (dc != null)
				return dc.Id;

			else return null;
		}

		/// <summary>
		/// Get instance from reference id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>

		public static DataTableMsx GetInstanceFromReferenceId(
			DocumentMsx doc,
			string id)
		{
			if (String.IsNullOrWhiteSpace(id)) return null;
			DataTableMsx dt = doc.DataManager.TableCollection.GetTableById(id);
			return dt;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			foreach (DataColumnMsx dc in Columns)
			{
				UpdatePreSerializationSecondaryReferences(dc);
			}

			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			foreach (DataColumnMsx dc in Columns)
			{
				UpdatePostDeserializationSecondaryReferences(dc);
			}

			return;
		}

		public override string ToString()
		{
			string txt = "DataTableMsx: " + Name + "\r\n" +
				"Idx, Name, DataType, MobiusRole, MobiusFileColumnName, ExternalName, Origin, ContentType\r\n";

			for (int dci = 0; dci < Columns.Count; dci++)
			{
				DataColumnMsx dc = Columns[dci];

				txt += String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}\r\n",
					dci, dc.Name, dc.DataType, dc.MobiusRole, dc.MobiusFileColumnName, dc.ExternalName, dc.Origin, dc.ContentType);
			}

			return txt;
		}
	}

}
