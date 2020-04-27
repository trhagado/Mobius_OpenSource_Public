using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.SpotfireDocument
{
	/// <summary>
	/// DataTableCollection
	/// </summary>

	public class DataTableCollectionMsx
	{

		public DataTableMsx GetByName(string name)
			{
				foreach (DataTableMsx dt0 in DataTables)
				{
					if (Lex.Eq(dt0.Name, name)) return dt0;
				}

				return null;
			}

		public DataTableMsx GetById(string id)
		{
			foreach (DataTableMsx dt0 in DataTables)
			{
				if (Lex.Eq(dt0.Id, id)) return dt0;
			}

			return null;
		}

		public List<DataTableMsx> DataTables = new List<DataTableMsx>();

		public DataTableMsx DefaultTableReference = null;

		public int Count { get { return DataTables.Count; } }

	}
}
