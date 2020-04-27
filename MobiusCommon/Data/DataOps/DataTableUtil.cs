using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Mobius.Data
{

/// <summary>
/// DataTable Utilities
/// </summary>
/// 
	public class DataTableUtil
	{
		/// <summary>
		/// Move a DataTable row to a new position
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="oldPos"></param>
		/// <param name="newPos"></param>
		/// <returns>New copy of row moved</returns>

		public static DataRow MoveRow(
			DataTable dataTable,
			int oldPos,
			int newPos)
		{
			DataRow dr = dataTable.Rows[oldPos];
			if (oldPos == newPos) return dr;

			DataRow newRow = dataTable.NewRow();
			newRow.ItemArray = dr.ItemArray; // copy data
			dataTable.Rows.RemoveAt(oldPos);

			if (newPos < dataTable.Rows.Count) // if moving down adjust for deletion of existing row
				dataTable.Rows.InsertAt(newRow, newPos);

			else dataTable.Rows.Add(newRow);
			return dr;
		}

	}
}
